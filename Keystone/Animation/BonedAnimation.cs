using System;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;

namespace Keystone.Animation
{
    public class BonedAnimation : AnimationClip 
    {
        // TODO: This should be treated as a special type of AnimationSequence

        // boned animation sub-sets from larger range need a start and end frame values
        protected int _index;
        protected float _startFrame;
        protected float _endFrame;

        protected int mBlendedAnimationID;
        protected string mBlendedAnimationByName;
        protected float mBlendedAnimationWeight;
        protected bool mBlendedAnimationLoop;
        protected bool mAnimationLoop;
        protected string mAnimationByName;

        internal bool _isIntrinsicAnimation; // an animation stored in the actor file and cannot be deleted.
        protected int _tvSourceAnimationID;  // TODO: perhaps intrinsic and tvsourceanimationID can share var
        private bool _initialized;  // if this is NOT an intrinsic, then at some point it must be

        protected bool _changed;
        // TODO: these animation names should be based on the mesh path and the animation name
        // eg:   "Rancor.tvm@Attack"
        // user can perhaps view the "friendly name" but the "id" is appended of path + friendly
        protected BonedAnimation(string id)
            : base(id)
        {
        	_index = -1;
        }

        /// <summary>
        /// Creates an ActorAnimation from an animation already configured in the TVActor
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="name">Friendly name of the animation.</param>
        /// <param name="srcAnimationID">intrinsic source index </param>
        private BonedAnimation(string id, string name, int srcAnimationID)
            : this(id, name, srcAnimationID, 0, 0)
        {
        }

        ///// <summary>
        ///// Creates an BonedAnimation from a specified subset of frames within a TVActor
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="name"></param>
        ///// <param name="startFrame"></param>
        ///// <param name="endFrame"></param>
        //public ActorAnimation(string id, string name, float startFrame, float endFrame)
        //    : base(id)
        //{

        //}

        private BonedAnimation(string id, string name, int srcAnimationID, float startFrame, float endFrame)
            : this(id)
        {
            _tvSourceAnimationID = srcAnimationID;
            // NOTE: mFriendlyName isn't very useful.  We use the _id of this node to create the ranged animation.
            //       This is just a clip underneath an Animation.cs object
            //       and only the Animation.cs needs a friendly name so that our scripts can call them.  
            //       We can't play individual clips under an Animation.  If we want to do that, then
            //       just make an Animation.cs that only has a single Clip under it.
            mFriendlyName = name;
            _startFrame = startFrame;
            _endFrame = endFrame;
            Debug.Assert(!String.IsNullOrEmpty(mFriendlyName));

        }

        public static BonedAnimation Create (string id, string name, int srcAnimationID, float startFrame, float endFrame)
        {
            BonedAnimation anim;
            anim = (BonedAnimation)Keystone.Resource.Repository.Get(id);
            if (anim != null) return anim;
            anim = new BonedAnimation(id, name, srcAnimationID, startFrame, endFrame);
            return anim;
        }

        public static BonedAnimation Create(string id)
        {
            BonedAnimation anim;
            anim = (BonedAnimation)Keystone.Resource.Repository.Get(id);
            if (anim != null) return anim;
            anim = new BonedAnimation(id);
            return anim;
        }

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 + tmp.Length];
            tmp.CopyTo(properties, 5);

            // NOTE: it's important to be able to initialize the NON intrinsic animations
            // AFTER they are loaded from XML so that means we need to do this dynamically

            properties[0] = new Settings.PropertySpec("sourcecid", _tvSourceAnimationID.GetType().Name);
            properties[1] = new Settings.PropertySpec("intrinsic", _isIntrinsicAnimation.GetType().Name);
            properties[2] = new Settings.PropertySpec("startframe", _startFrame.GetType().Name);
            properties[3] = new Settings.PropertySpec("endframe", _endFrame.GetType().Name);
            properties[4] = new Settings.PropertySpec("index", _index.GetType().Name);
            
            if (!specOnly)
            {
                //properties[0].DefaultValue = mFrameCount;
                properties[0].DefaultValue = _tvSourceAnimationID;
                properties[1].DefaultValue = _isIntrinsicAnimation;
                properties[2].DefaultValue = _startFrame;
                properties[3].DefaultValue = _endFrame;
                properties[4].DefaultValue = _index;
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
                    case "intrinsic":
                        _isIntrinsicAnimation = (bool)properties[i].DefaultValue;
                        break;
                    case "sourcecid":
                        _tvSourceAnimationID = (int)properties[i].DefaultValue;
                        break;
                        // NOTE: _index is generated at runtime so is read only.  Also
                        //       because BonedAnimation.cs instances will eventually be shareable 
                        //      (it's not shareable yet because we inherit from AnimationClip.cs which 
                        //       contains some state variables that we should move to AnimationTrack)
                        //       , if the _index
                        //       value changes at runtime, it ruins things.  _index should only be
                        //       generated once across all shared BonedEntities and their animations.
                        //       IT'S IMPORTANT to realize that the animations we create when we call
                        //       _index = actor._actor.AddAnimationRange(_tvSourceAnimationID, _startFrame, _endFrame, _id);
                        //        these animations get created and shared across all duplicate actors! 
                        //       
                        //case "index":
                        //    _index = (int)properties[i].DefaultValue;
                        break;
                    case "startframe":
                        _startFrame = (float)properties[i].DefaultValue;
                        _initialized = false;
                        break;
                    case "endframe":
                        _endFrame = (float)properties[i].DefaultValue;
                        _initialized = false;
                        break;
                }
            }

            _changed = true;
            if (_startFrame > _endFrame) _endFrame = _startFrame; // provided we dont exceed the intrinsic's frame count
            _duration = KeyFrameCount / 24;
        }
        #endregion


        /// <summary>
        /// Read Only property that returns whether this is an animation that is included in the
        /// .X or .TVA file and therefore is not allowed to be removed from the parent via client
        /// command.
        /// </summary>
        /// <remarks>
        /// Intrinsic animations always show up first in the list of animations.
        /// </remarks>
        public bool IsIntrinsicAnimation { get { return _isIntrinsicAnimation; } }

        public int TVSourceAnimationID
        {
            get { return _tvSourceAnimationID; }
        }

        public int Index // todo; what is difference between TVSourceAnimationID and Index?
                         // well one is a built in source animation, the other is user index for custom animation
        {
            get { return _index; }
            //set { _index = value; }
        }

        public float StartFrame
        {
            get { return _startFrame; }
        }

        public float EndFrame
        {
            get { return _endFrame; }
        }

        /// <summary>
        /// For NON INTRINSIC initialization of Animations specified from a range
        /// of an existing INTRINSIC actor animation.  Initialize() must only be called ONCE
        /// to load the animation and return an index.  If we wish to modify things later
        /// we can mdoify without changing the index so DO NOT call Initialize() again.
        /// </summary>
        /// <param name="model"></param>
        internal int Initialize(Model model)
        {
            // TODO: Should Initialize try to find the Model Source itself
            // by _model = (Model)Repository.Get (_sourceName)
            // HOWEVER, it seems to me that the ACtor3d is guaranteed to be loaded if we're able
            // to create any intrinsic animations whatsoever... this of course
            // Initialize() is only for NON intrinsics which could very well be loaded before
            // the Actor3d is loaded.  The Entity would receive event when an Actor3d resource
            // was paged in/added and could then notify it's AnimationController to initialize
            // applicable animations.
            //
            //
            // TODO: is this boned animation specific to one TVActor instance or all duplicates?
            // well, we do have to make a call to tvactor.AddAnimationRange for all LODs
            // if we want to have them match... or
            // we can make it so that each Model independantly manages it's last animations
            // and some lod versions wont have any animations for certain things but instead
            // our entity will track an animation abstractly as a state and not as an actual animation data
            // I think that is important.  BUT isn't that then
            // Behavior / AI logic and not animation which _IS_ data based?

            System.Diagnostics.Debug.Assert (model.Name == this._targetName);
            System.Diagnostics.Debug.WriteLine("BonedAnimation.Initialize() - id = " + _id);

            if (model == null) throw new Exception("BonedAnimation.Initialize() - Model is null.");
            if (model.Geometry == null) throw new Exception("BonedAnimation.Initialize() - Geometry is null.");
            if (model.Geometry.TVResourceIsLoaded == false) throw new Exception("BonedAnimation.Initialize() - Required Geometry is not loaded.");

            if (_startFrame == _endFrame)
            {
                System.Diagnostics.Debug.WriteLine("BonedAnimation.Initialize() - startFrame and endFrame cannot be the same.");
                _initialized = false;
                return -1;
            }

            Actor3d actor = (Actor3d)model.Geometry;

            try 
            {
                if (this._isIntrinsicAnimation == false)
                {
                    if (_index == -1)
                    {
                        // NOTE: for BonedAnimations, the mFriendlyName is the same friendly name of the Animation.cs parent node for this clip.
                        //       TVActor and Duplicates all share animations that are added.  In other words, calling AddAnimationRange() on
                        //       one duplicate, adds it to all the duplicates so we first check if that animation already exists, otherwise
                        //       we just waste memory.

                        _index = actor._actor.GetAnimationIDByName(mFriendlyName); // this returns -1 if the animation name does not exist

                        if (_index == -1)
                            // NOTE: if _startFrame and _endFrame are the same, it will return 0.  This seems like a tv3d bug.
                            _index = actor._actor.AddAnimationRange(_tvSourceAnimationID, _startFrame, _endFrame, mFriendlyName);

                        System.Diagnostics.Debug.Assert(_index  != -1 && _index == actor._actor.GetAnimationIDByName(mFriendlyName));
                        System.Diagnostics.Debug.WriteLine("BonedAnimation.Initialize() - Animation count = " + actor._actor.GetAnimationCount());
                        System.Diagnostics.Debug.WriteLine("BonedAnimation.Initialize() - Animation Range added at index = " + _index.ToString());
                    }
                    else
                    {
                        // NOTE: we will not necessarily get the same _index when we call AddAnimationRange() after DeleteAnimationRange(_index).
                        //       It shouldn't be a problem because we don't rely on the _index, but rather the mFriendlyName
                        //  tv3d doesn't have a .ChangeAnimationRange()
                        actor._actor.DeleteAnimationRange(_index);
                        int newIndex = actor._actor.AddAnimationRange(_tvSourceAnimationID, _startFrame, _endFrame, mFriendlyName);
                        //System.Diagnostics.Debug.Assert(_index == newIndex); // Dec.3.2022 - verified that this assert can fail
                        _index = newIndex;
                        System.Diagnostics.Debug.Assert(_index == actor._actor.GetAnimationIDByName(mFriendlyName));
                    }
                }
            }
        	catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("BonedAnimation.Initialize() - " + ex.Message);
            }
            _initialized = true;
            return _index;
        }

        internal bool Initialized { get {return _initialized;}}
        
        internal override void Update(AnimationTrack track, object target)
        {        	
            //throw new NotImplementedException();
            
        }

        public override int KeyFrameCount
        {
            get {return (int)(_endFrame - _startFrame); }
        }

    }
}
