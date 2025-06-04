using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keystone.IO;

namespace Keystone.Appearance
{
    // Layers are like Transforms and thus are never shared.
    public abstract class Layer : Elements.Group
    {
        protected Texture mTexture;
        protected Types.Vector2f _tscale;
        protected Types.Vector2f _toffset;  // valid values are between 0.0 and 1.0. Actually higher values are valid too but effectively the value before the decimal is ignored.  Only the fractional amount modifies the offset
        protected float _rotation = 0;     // angle in radians
        protected int _layerID; // layerID's are set by the particular dervived texture types automatically (e.g. NormalMap sets layer = 1)


        // NOTE: no static Create methods as Layer's cannot be shared.
        protected Layer(string id) : base (id)
        {
            _tscale.x = 1;
            _tscale.y = 1;
        }


        public override Keystone.Traversers.ChildSetter GetChildSetter()
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
                        // do we ignore here since only GUI Plugin uses the Get version
                        // of this value, but no set allowed, or do we allow
                        // a change in this value to replace the existing texture?
                        // for now, we ignore
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
        }
        #endregion

        private string GetResourceDescriptor()
        {
            if (mTexture == null) return "";
            return mTexture.ID;
        }

        /// <summary>
        /// Helper function to add a texture node directly from a path.
        /// The child Texture node will get created if it doesn't already exist 
        /// in Repository.
        /// </summary>
        /// <param name="resourcePath"></param>
        public void AddChild(string resourcePath)
        {
            Texture t = Texture.Create(resourcePath);
            this.AddChild(t);
        }

        public void AddChild(Texture texture)
        {
            if (mTexture != null) throw new Exception("Only one texture child allowed.");
            mTexture = texture;
            base.AddChild(texture);
        }

        public override void RemoveChild(Keystone.Elements.Node child)
        {
            if (child == mTexture)
            {
                mTexture = null;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }

            base.RemoveChild(child);
        }

        public bool TVResourceIsLoaded
        {
            get 
            {
                if (mTexture == null) return false;
                return mTexture.TVResourceIsLoaded;
            }
        }

        public int TextureIndex
        {
            get 
            {
                if (mTexture == null) return -1;
                return mTexture.TVIndex;
            }
        }
        // todo: can / should shaders on per-group use any values from
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
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public float TileV
        {
            get { return _tscale.y; }
            set
            {
                _tscale.y = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
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
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
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
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public override int GetHashCode()
        {
            int result = 0;
            // todo: take into account texture mod
            //_tscale.x 
            //_tscale.y
            //_toffset.x
            //_toffset.y
            //_rotation 

            if (mTexture != null)
                result += mTexture.TVIndex;
            
            return result;
        }
    }
}
