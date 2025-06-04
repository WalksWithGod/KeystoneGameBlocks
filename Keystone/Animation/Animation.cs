using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Types;
using MTV3D65;
using Keystone.Elements;
using Keystone.Entities;

namespace Keystone.Animation
{
    // An Animation hosts arbitrary sequence of AnimationClip nodes.

    // TODO: SunBurn engine allows for multiple animations
    // to be sequenced together and treated as a single animation.
    // That is something we should easily be able to do...
    // http://sgmotion.codeplex.com/SourceControl/changeset/view/65608#637389
    // http://animationcomponents.codeplex.com/
    //
    // todo; the question is, shoudl our Animation in effect each be considered an AnimationController
    // track, or an AnimationInfo...  the problem is, for Actor3d, how do we ensure that an
    // animation can be shared by many entities for use with many different targets?  Target itself
    // is something that belongs in an AnimationController... and the AnimationInfo is something
    // that holds actual raw keyframes and such... 
    // In fact, the Target is kind of the Source for which the Animation class is the Controller...
    // just as a Transform node or Model or Entity is a Source for Interpolator nodes
    // Hrm... for KeyFrameAnimation, the actual AnimationInfo data should contain reference to Actor3d
    // and perhaps it's model and then be treated as a SOURCE that an AnimationController would
    // reference as it's Target.
    // So I think this tells us that our "Target" data is to be like our "resource" and the
    // actual AnimationController is a non shareable Node that holds state.
    // Entity.AnimationTracksManager stores an array of "Tracks" ...
    // what if the Tracks maintained the pairing of Animation and Source info?
    // the so called "routing" info as well as state info?
    // Then, our Tracks are created when each animation is loaded, but tracks can be active or unactive
    //
    // Channels are per Target/Source.  So all Animations based on the keyframes of an TVActor
    // will exist in the same channel and only these types can be blended to/from.
    // Fuck i dunno... im making this too complicated for now yes?
    // i mean then we have sequences of animations, and what channel do those go?
    //
    // Isn't using friendly names how shareable triggers would work?  You build a model of something
    // (visual model, physical model, etc) and you use friendly name fields to assign targets because
    // at runtime, the actual target instance reference gets assigned after the target is instantiated
    // So this solves it i think.  Our Animation's are actually
    // What would cause the least problems if Model friendly names of specific instances were changed?
    // Are friendly names used as id's for Instancing tracking?
    // 
    // An Animation instance hosts arbitrary sequence of AnimationClip nodes.
    public class Animation : Group
    {

        // http://www.okino.com/conv/exp_xof.htm
        // DirectX keys are output by multiplying the frame time by 
        // (ticks-per-second / frames-per-second). The ticks-per-second
        // is set by this combo box, and the frames-per-second is set by 
        // the current scene being exported. Most DirectX viewers use 3600 
        // as the "ticks per scond", but it has been reported that some newer 
        // DirectX viewers have opted for the more sane 4800 ticks per second. 
        //-- 
        // thus 
        // speed1 = 24/3600 = 0.00666666666666666666666666666667f; //
        // speed2 = 24/4800 = 0.005; //
        
        protected float _speed = 0.005f; // scripts can change the speed of a track based on various factors 
        protected bool _looping;
        protected bool _forward;  // this animation should play forward or reverse by default
        // NOTE: Feb.13.2015 - autoplay was an ill conceived idea! Make the app call it explicitly and implement it's own "autoplay"
		// protected bool _autoplay; 
		
        protected SelectorNodeStyle mStyle;
        protected int mChildrenEnabledFlags; // 32 bits to indicate which of up to 32 children are enabled


        /// <summary>
        /// Animation is shareable.  AnimationTrack contains unique state.
        /// </summary>
        /// <param name="id"></param>
        public Animation(string id) 
            : base(id)
        {
            Shareable = false; // TODO: actually im not sure yet if Animation is shareable... since it wasnt 
                               // set = true before and i thought it was an oversight, but not sure
                               // july.22.2013
                               // TODO: should IGroup nodes ever be shareable?  Even our Appearance and GroupAttributes
                               // are not.  
                                // TODO: it's not shareable at least yet because AnimationClip's are not shareable
                                // but i might be able to fix that and then make both shareable.  not sure yet.
                                // it requires AnimationClip's targetName and _targetPropertyName vars be moved 
                                // to AnimationTrack so that there is no per instance data in the AnimationClip
            _forward = true;
        }

        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            properties[0] = new Settings.PropertySpec("speed", _speed.GetType().Name);
            properties[1] = new Settings.PropertySpec("forward", _forward.GetType().Name);
            properties[2] = new Settings.PropertySpec("loop", _looping.GetType().Name);
            
			if (!specOnly)
            {
                properties[0].DefaultValue = _speed;
                properties[1].DefaultValue = _forward;
                properties[2].DefaultValue = _looping;
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {

                    case "speed":
                        _speed = (float)properties[i].DefaultValue;
                        break;
					case "forward":
                        _forward = (bool)properties[i].DefaultValue;
                        break;
                    case "loop":
                        _looping = (bool)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        // TODO: we must propogate change flags to AnimationTrack when target/property/component
        // changes in an KeyframeInterpolator child.  The change of target/property can only
        // happen programmatically and not through the Animation editor by design. In the editor
        // clips must be deleted and a new node added under a different treeNode to change it's target/property.

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        public AnimationClip[] AnimationClips
        {
            get
            {
                if (_children == null || _children.Count == 0) return null;
                AnimationClip[] results = new AnimationClip[_children.Count];
                for (int i = 0; i < _children.Count; i++)
                    results[i] = (AnimationClip)_children[i];

                return results;
            }
        }

        public void AddChild(AnimationClip clip)
        {
            base.AddChild(clip);
        }
                

        /// <summary>
        /// Individiaul clips can have a variable durations.  The animation's duration
        /// is the maximum duration of any of it's clips.
        /// </summary>
        public float Duration 
        { 
            get 
            {
                if (_children == null || _children.Count == 0) return 0;

                float max = 0;
                AnimationClip[] clips = AnimationClips;
                for (int i = 0; i < clips.Length; i++)
                    max = Math.Max(max, clips[i].Duration);

                return max; 
            }
        }
        
        public float Speed
        {
            get { return _speed; }
            // TODO: these setters are never used because we always go through SetProperties() right?
            set { _speed = value; }
        }

        public bool Forward
        {
            get { return _forward ; }
            // TODO: these setters are never used because we always go through SetProperties() right?
            set { _forward = value; }
        }
                
        public bool Looping
        {
            get { return _looping; }
            // TODO: these setters are never used because we always go through SetProperties() right?
            set 
            { 
            	_looping = value; 
            }
        }
        
        // Feb.13.2015 - ill conceived idea.  Calling app should implement it's own "autoplay"
//        public bool AutoPlay // start playing automatically upon adding of animation to scene 
//        {
//            get { return _autoplay; }
//            // TODO: these setters are never used because we always go through SetProperties() right?
//            set { _autoplay = value; }
//        }
    }
 
}