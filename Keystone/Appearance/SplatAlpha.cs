using System;
using System.Diagnostics;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class SplatAlpha : Layer
    {
        protected Texture mAlphaTexture; // baseTexture uses base.mTexture
        
        private Int32 _ChunkStartX = 0;
        private Int32 _ChunkStartY = 0;
        private Int32 _ChunksX = 1;
        private Int32 _ChunksY = 1;

        // // alpha can be null if it's the first base texture, all other splat layers require both base and alpha map
        //// TODO: why am i not forcing the inclusion of the diffuse base layer in the constuctor?
        //// NOTE: as i recall, for when i just want a base layer with no alpha, i just use a regular Diffuse object so this SplatAlpha class
        //// is meant to always have both right?  So why isnt it here? looks like an oversight but havent i 
        //public static SplatAlpha Create(string id, string relativeFilePath)
        //{
        //    int index = CoreClient._CoreClient.TextureFactory.LoadAlphaTexture(relativeFilePath, id);
        //    return Create(index);
        //}

        //internal SplatAlpha(string id) : base(id)
        //{
        //}

        //public static SplatAlpha Create(int index)
        //{
        //    if (Texture.IsInFactory(index))
        //    {
        //        string resourceID = ImportLib.AbsolutePathToRelative(CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
        //        SplatAlpha t = (SplatAlpha)Repository.Get(resourceID );
        //        if (t != null) return t;

        //        return new SplatAlpha(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
        //    }
        //    else throw new Exception("Texture does not exist in factory.");
        //}

		// alpha can be null if it's the first base texture, all other splat layers require both base and alpha map
        public SplatAlpha(string id) : base (id )
        {
            _tscale.x = 2;
            _tscale.y = 2;
            _layerID = 0;
        }

        #region IResource Members
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

            properties[0] = new Settings.PropertySpec("chunkstartx", _ChunkStartX.GetType().Name);
            properties[1] = new Settings.PropertySpec("chunkstarty", _ChunkStartY.GetType().Name);
            properties[2] = new Settings.PropertySpec("chunksx", _ChunksX.GetType().Name);
            properties[3] = new Settings.PropertySpec("chunksy", _ChunksY.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = _ChunkStartX;
                properties[1].DefaultValue = _ChunkStartY;
                properties[2].DefaultValue = _ChunksX;
                properties[3].DefaultValue = _ChunksY;
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
                    case "chunkstartx":
                        _ChunkStartX = (int)properties[i].DefaultValue;
                        break;
                    case "chunkstarty":
                        _ChunkStartY = (int)properties[i].DefaultValue;
                        break;
                    case "chunksx":
                        _ChunksX = (int)properties[i].DefaultValue;
                        break;
                    case "chunksy":
                        _ChunksY = (int)properties[i].DefaultValue;
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }
#endregion

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        public Texture AlphaTexture 
        {
            get { return mAlphaTexture; }
        }

        public Int32 ChunkStartX
        {
            get { return _ChunkStartX; }
            set
            {
                _ChunkStartX = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunkStartY
        {
            get { return _ChunkStartY; }
            set
            {
                _ChunkStartY = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunksX
        {
            get { return _ChunksX; }
            set
            {
                _ChunksX = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunksY
        {
            get { return _ChunksY; }
            set
            {
                _ChunksY = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }
        
        public override void AddChild(Texture texture)
        { 
            if (texture.TextureType == Texture.TEXTURETYPE.Alpha)
            {
            	if (mAlphaTexture != null) throw new Exception ("SplatAlpha.AddChild() - Only one alpha texture child allowed.");
            	mAlphaTexture = texture;
            }
            else
            {
	             if (mTexture != null) throw new Exception("SplatAlpha.AddChild() - Only one base texture child allowed.");
            	mTexture = texture;
            }
            
            base.AddChild(texture);
        }
        
		public override void RemoveChild(Keystone.Elements.Node child)
		{
			if (child == mAlphaTexture)
			{
				mAlphaTexture = null;
				SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Child);
			}
			
			base.RemoveChild(child);
		} 
        
        public override bool TVResourceIsLoaded
        {
            get 
            {
                if (mTexture == null) return false; // alpha can be null if it's the first base texture, all other splat layers require both base and alpha map
                return mTexture.TVResourceIsLoaded;
            }
        }
    }
}