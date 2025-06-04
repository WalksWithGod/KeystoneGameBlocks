using System;
using System.Collections.Generic;
using Keystone.IO;

namespace Keystone.Appearance
{
    // Layers are like Transforms and thus are never shared.
    public abstract class Layer : Elements.Group
    {
        protected Texture mTexture;         // TODO: 
                                            // if the mTexture is an AnimatedTexture
                                            // then there is a chance that a new keyframe
                                            // is selected based on the elapsedTIme and prevFrame 
                                            // and TextureAnimation.cs animation speed and num frames total.
                                            // we can then use that to update the shader parameters
                                            // so that the 
        // textureWidth
        // textureHeight
        // spriteSpacing  
        
        // TextureAnimation: Animation will hold frameCount (aka spriteCount) and stats
        // spriteCount; // can't count be deduced from spriteWidth and textureWidth?                          
        // spriteWidth
        // spriteHeight
        // float2 scale; // user might still want to tile a sprite across the geometry
        // float2 offset;  
        

        // animation track will hold
        // elapsed :TIME
        // float speed;

                                            // or maybe TV's time semantic... but our TextureAnimation
                                            // still needs to track completion so that an eXplosion
                                            // entity knows when to terminate.
                                            //
                                            // textureFrame, etc for dynamically picking the UV in shader
        // UV's are per vertex so here we only
        // allow scaling and offset parameters which ideally
        // should update a shader
        protected Types.Vector2f _tscale;
        protected Types.Vector2f _toffset;  // valid values are between 0.0 and 1.0. Actually higher values are valid too but effectively the value before the decimal is ignored.  Only the fractional amount modifies the offset
        protected float _rotation = 0;     // angle in radians
        protected int _layerID; // layerID's are set by the particular dervived texture types automatically (e.g. NormalMap sets layer = 1)


        // NOTE: no static Create methods as Layer's cannot be shared.
        protected Layer(string id) : base (id)
        {
            _tscale.x = 1;
            _tscale.y = 1;
            Shareable = false; // layers are not textures, but i think we may just decide to roll this class
                               // into Appearance/GroupAppearance because the only thing layers are really good for
                               // is fixed function texture mods and we should get away from that.  We can use
                               // the shader parameters we store in Appearance/GroupAppearance for modding textures
                               // in shader
        }


        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        public Texture Texture 
        {
            get { return mTexture; }
        }

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[4 + tmp.Length];
            tmp.CopyTo(properties, 4);

            properties[0] = new Settings.PropertySpec("tscale", _tscale.GetType().Name);
            properties[1] = new Settings.PropertySpec("toffset", _toffset.GetType().Name);
            properties[2] = new Settings.PropertySpec("trotation", _rotation.GetType().Name);
            // "Get" of "texture" is allowed by "Set" is not.
            // only GUI Plugin uses the Get version of the "texture" property
            // Unfortunately, the "Get" allows for this property to appear in the saveed XML
            // making it appear as if it can be modified.  But it cannot and must not.
            properties[3] = new Settings.PropertySpec("texture", typeof(string).Name);
            if (!specOnly)
            {
                properties[0].DefaultValue = _tscale;
                properties[1].DefaultValue = _toffset;
                properties[2].DefaultValue = _rotation;
                properties[3].DefaultValue = GetResourceDescriptor();
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
                    case "tscale":
                        _tscale = (Types.Vector2f)properties[i].DefaultValue;
                        break;
                    case "toffset":
                        _toffset = (Types.Vector2f)properties[i].DefaultValue;
                        break;
                    case "trotation":
                        _rotation = (float)properties[i].DefaultValue;
                        break;
                    case "texture":
                        // We must NOT allow "Set" here.  So we ignore.
                        // only GUI Plugin uses the Get version of the "texture" property
                        // Unfortunately, the "Get" allows for this property to appear in the saveed XML
                        // making it appear as if it can be modified.  But it cannot and must not.
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }
        #endregion

        private string GetResourceDescriptor()
        {
            if (mTexture == null) return "";
            return mTexture.ID;
        }

//        /// <summary>
//        /// Helper function to add a texture node directly from a path.
//        /// The child Texture node will get created if it doesn't already exist 
//        /// in Repository.
//        /// </summary>
//        /// <param name="resourcePath"></param>
//        public void AddChild(string resourcePath)
//        {
//            Texture t = Texture.Create(resourcePath);
//            this.AddChild(t);
//        }

        public virtual void AddChild(Texture texture)
        {
        	if (this is SplatAlpha == false)
        	{
        		if (mTexture != null) throw new Exception("Layer.AddChild() - Only one texture child allowed.");
            	mTexture = texture;
        	}
            base.AddChild(texture);
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Child);
        }

        public override void RemoveChild(Keystone.Elements.Node child)
        {
            if (child == mTexture)
            {
                mTexture = null;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }

            base.RemoveChild(child);
        }

        public virtual bool TVResourceIsLoaded
        {
            get 
            {
                if (mTexture == null) return false;
                return mTexture.TVResourceIsLoaded;
            }
        }

        public virtual int TextureIndex
        {
            get 
            {
                if (mTexture == null) return -1;
                return mTexture.TVIndex;
            }
        }
        // TODO: can / should shaders on per-group use any values from
        // the layer?  as in parameters

        public int LayerID
        {
            get { return _layerID; }
        }

        public bool TextureModEnabled
        {
            get
            {
                return _tscale.x != 1 || _tscale.y != 1 ||
                    _toffset.x != 0 || _toffset.y != 0 ||
                    _rotation != 0;
            }
        }

        public float TileU
        {
            get { return _tscale.x; }
            set
            {
                _tscale.x = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public float TileV
        {
            get { return _tscale.y; }
            set
            {
                _tscale.y = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Valid value is between 0.0 and 1.0f
        /// </summary>
        public float TranslationU
        {
            get { return _toffset.x; }
            set
            {
                _toffset.x = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Valid value is between 0.0 and 1.0f
        /// </summary>
        public float TranslationV
        {
            get { return _toffset.y; }
            set
            {
                _toffset.y = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
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

            if (mTexture != null)
            {
                Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(mTexture.GetHashCode()), ref result);
            }
            

            return (int)result;
        }
    }
}
