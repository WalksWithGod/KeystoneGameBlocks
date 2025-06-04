using System;
using System.Diagnostics;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class SplatAlpha : Layer
    {
        private string _baseTextureFileName;
        private int _baseTextureIndex;
        private Int32 _ChunkStartX = 0;
        private Int32 _ChunkStartY = 0;
        private Int32 _ChunksX = 1;
        private Int32 _ChunksY = 1;

        //// todo: why am i not forcing the inclusion of the diffuse base layer in the constuctor?
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

        public SplatAlpha(string resourcePath) : base (resourcePath )
        {
            _tscale.x = 2;
            _tscale.y = 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 + tmp.Length];
            tmp.CopyTo(properties, 5);

            properties[0] = new Settings.PropertySpec("basefilename", _baseTextureFileName.GetType().Name);
            properties[1] = new Settings.PropertySpec("chunkstartx", _ChunkStartX.GetType().Name);
            properties[2] = new Settings.PropertySpec("chunkstarty", _ChunkStartY.GetType().Name);
            properties[3] = new Settings.PropertySpec("chunksx", _ChunksX.GetType().Name);
            properties[4] = new Settings.PropertySpec("chunksy", _ChunksY.GetType().Name);


            if (!specOnly)
            {
                properties[0].DefaultValue = _baseTextureFileName;
                properties[1].DefaultValue = _ChunkStartX;
                properties[2].DefaultValue = _ChunkStartY;
                properties[3].DefaultValue = _ChunksX;
                properties[4].DefaultValue = _ChunksY;
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
                    case "basefilename":
                        _baseTextureFileName = (string)properties[i].DefaultValue;
                        break;
                    case "chunkstartx":
                        _ChunkStartX = int.Parse((string)properties[i].DefaultValue);
                        break;
                    case "chunkstarty":
                        _ChunkStartY = int.Parse((string)properties[i].DefaultValue);
                        break;
                    case "chunksx":
                        _ChunksX = int.Parse((string)properties[i].DefaultValue);
                        break;
                    case "chunksy":
                        _ChunksY = int.Parse((string)properties[i].DefaultValue);
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
        }


        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion

        public string baseTextureFileName
        {
            get { return _baseTextureFileName; }
            set { _baseTextureFileName = value; }
        }

        public Int32 baseTextureIndex
        {
            get { return _baseTextureIndex; }
            set
            {
                _baseTextureIndex = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunkStartX
        {
            get { return _ChunkStartX; }
            set
            {
                _ChunkStartX = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunkStartY
        {
            get { return _ChunkStartY; }
            set
            {
                _ChunkStartY = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunksX
        {
            get { return _ChunksX; }
            set
            {
                _ChunksX = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }

        public Int32 ChunksY
        {
            get { return _ChunksY; }
            set
            {
                _ChunksY = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }
    }
}