using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Keystone.Elements;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    
    // TODO: For GroupAttribute to have an AnimationTrack is there
    // a more generic struct/class we should use for hosting Shader runtime 
    // state?  GroupAttribute.RunTimeStates[""].Parameters[""]
    // Consider how SHaders work, GroupAttribute.CustomParameters[]  retrieves
    // a list of PropertySpec that contains the parameters and the persisted values.
    // I think the same can be done for TextureCycle runtime state....  
    // 

    /// <summary>
    /// TextureCycle should be thought of as an Appearance Effect like a Shader or Texture
    /// and NOT as an implementation of a generic animation.  
    /// This means that a TextureCycle should be updated similarly to a Shader... by script
    /// changing the parameters and by enabling/disabling through scripted game logic.  And just
    /// as Shader parameters are stored in the Appearance, TextureCycle state is stored in the Appearance.
    /// However there's no need to store TextureCycle state in a serializeable parameter string.
    /// It's only needed during runtime.
    /// </summary>
    public class TextureCycle : Layer, IPageableTVNode 
    {
        private readonly object mSyncRoot;
        PageableNodeStatus _resourceStatus;
        int _tvfactoryIndex;
        

        // all animations registered with entity for all models?
    //    Animation mThruster = Entity.Animation["afterburner"]; 

        // this actually calls an animation sequence which 1) scrolls the texture
        // 2) vibrates\pulses the exhaust plume size
  //      Entity.Animation.Play(mThruster, speed); // will create a track for it

        // a special type of Texture implementation that has interface of single Texture
        // but internally represents an animation of a sequence of textures. 
        // NOTE: This class is just a data container.  The tracking and driverof the texture animation
        // (eg. current frame, elapsed, computing next frame, fading, etc) is done by
        // our AnimationController.cs and the Track.cs struct which tracks actual instance information.
        //
        // Note our linear interpolator will work with TextureCycle very readily.

        // Is TextureCycle a Texture or an Animation or both?
        // I think rather than a Texture, it's more of an Animation.  It's a specific type of
        // Animation that we can define the keyframes for and where the keyframes are textures.
        // Then during running of the TextureCycle animation, we somehow have to associate it with
        // a particular Layer so that we can rely on the layer to tell us what group and such to
        // apply the texturecycle to... [CHANGE: No more reliance on layer/groups so re-apply
        // the current texture frame, we are requiring textured animations to use spritesheets
        // with all animations in on sheet so the textureID never changes.  To use more than one
        // sprite sheet, we use multiple texture layers and then can select the right one in shader
        // based on a parameter we can toggle in the shader.]
        // 
        // When Mesh/Actor  Render() method
        // we must provide a way to modify the group's texture based on the animation...
        // Normally, the trackInfo holds keyframe state data... I think maybe it's ok though
        // for a TextureCycle to have a groupID (so we dont have to assume -1 overall texture apply)
        // 

        public static TextureCycle Create(string id)
        {
            TextureCycle t = (TextureCycle)Repository.Get(id);
            if (t != null) return t;

            t = new TextureCycle(id);
            t._id = id;
            return t;
        }

        private TextureCycle(string id) : base (id)
        {
            _id = id;
            _tvfactoryIndex = -1;
            mSyncRoot = new object();
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 


        // TODO: this Texture[] array is totally horrible.  we will not use an array of textures 
        // instead we will switch to using a spritesheet
        public Texture[] Textures
        {
            get 
            {
                if (_children == null) return null;
                Texture[] tmp = new Texture[_children.Count];
                int j = 0;
                for (int i = 0; i < _children.Count; i++)
                    if (_children[i] is Texture)
                    {
                        tmp[j] = (Texture)_children[i];
                        j++;
                    }

                // need to resize to just the length of the Texture nodes
                Texture[] final = new Texture[j];
                Array.Copy(tmp, final, j);
                return final;
            }
        }

        #region IGroup Members
        public void AddChild(Texture texture)
        {
            base.AddChild((Node)texture); // CAST CHILD TO NODE to avoid the Layer.AddChild
        }

        public void RemoveChild(Node child)
        {
            base.RemoveChild(child);
        }
        #endregion

        public override int TextureIndex
        {
            get
            {
                return _tvfactoryIndex;
            }
        }

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public override bool TVResourceIsLoaded // overrides Layer and implements IPageable here (because Layer does not)
        {
            get { return _tvfactoryIndex > -1; }
        }

        /// <summary>
        /// This is the relative path of the resource with respect to the \Data folder.
        /// Since resources can be shared across mods, the mod name is included after \Data instead of simply
        /// using the relative path that included the mod name already.
        /// </summary>
        public string ResourcePath
        {
            // TODO: what kind of resource path shoudl this use?  a combination of all child paths?
            get { return _id; }
            set { _id = value; }
        }

        
        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
                
        public virtual void LoadTVResource()
        {
            // all children must be loaded before this TextureCycle can return 
            // >0 for _tvtextureFactoryIndex

            if (_children == null) return;
            int numTexturesLoaded = 0;

            Texture[] textures = Textures;

            if (textures != null)
            {
	            for (int i = 0; i < textures.Length; i++)
	            {
	            	Keystone.IO.PagerBase.LoadTVResource (textures[i], false);
	            	
	            	if (textures[i].PageStatus == PageableNodeStatus.Loaded)
	            		numTexturesLoaded++;
	            	
	            }
            }
            
            if (numTexturesLoaded != textures.Length)
                _tvfactoryIndex = -1;
            else
                _tvfactoryIndex = 1;

            if (_tvfactoryIndex == -1)
                System.Diagnostics.Trace.WriteLine("Texture:LoadTVResource() -- Error loading texture '" + _id + "'");

            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException("TextureCycle cannot be saved as single texture yet...");
        }
        #endregion

        private float mKeyFrame = 0;
        internal void Update(double elapsedSeconds)
        {
            mKeyFrame += (float)elapsedSeconds;

            if (mKeyFrame > 1.0) 
                mKeyFrame %= 1.0f;

            float ratio = Utilities.InterpolationHelper.LinearMapValue(0f, 1f, mKeyFrame);
            int childIndex = (int)(_children.Count * mKeyFrame);
            _tvfactoryIndex = ((Texture)_children[childIndex]).TVIndex;
        }


        public override int GetHashCode()
        {
            uint result = 0;
            // TODO: take into account texture mod
            //_tscale.x 
            //_tscale.y
            //_toffset.x
            //_toffset.y
            //_rotation 

            if (_children != null)
            {
                for (int i = 0; i < _children.Count ; i++)
                    Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(((Texture)_children[i]).GetHashCode()), ref result);
                
            }

            return (int)result;
        }
    }
}
