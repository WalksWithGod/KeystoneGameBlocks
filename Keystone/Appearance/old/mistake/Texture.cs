using System;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Traversers;
using MTV3D65;
using Keystone.Resource;

namespace Keystone.Appearance
{
    public class Texture : Node, IPageableTVNode
    {
        private enum TextureType : byte
        {
            Diffuse = 0,
            Alpha,
            Cube,
            Volume,
            DUDV
        }

        // specific layer type (e.g. diffuse, normal, specular, emmissive, alpha, splat, etc all inherit this class
        //There are max 4 layers on a Mesh that are represented by
        //- TV_LAYER_0
        //- TV_LAYER_1
        //- TV_LAYER_2
        //- TV_LAYER_3
        //If you want to save/get all textures (for an editor or something), use these 4, you won't forget or do redundant things.
        //Then you have some "helpers" layer, that points to one of the layer. The names are only for ease of use and means nothing to the engine. 
        //It's just to not to have to remember that lightmaps or normalmaps are expected to go in the layer1. The engine expects you to have the textures in these layers for the effects.
        //TV_LAYER_BASETEXTURE -> layer 0 ( same as diffuse )
        //TV_LAYER_LIGHTMAP -> layer 1
        //TV_LAYER_DETAILMAP -> layer 1
        //TV_LAYER_NORMALMAP -> layer 1 (same as bumpmap)
        //TV_LAYER_BUMPMAP -> layer 1 (same as normalmap)
        //TV_LAYER_HEIGHTMAP -> layer 2 ( heightmap is used for OffsetBumpmapping)
        //TV_LAYER_SPECULARMAP -> layer 2 ( well it's better to put the specular map in the normalmap alpha :p)
        //TV_LAYER_EMISSIVE -> layer 3 (only used for Glow emissive)
        // -Sylvain
        //
        // also read http://www.truevision3d.com/phpBB2/viewtopic.php?t=14642
        // for Zak's picutres of the different combinations.  Yes its true that in those pictures
        // you wont see detail maps + lightmaps or lightmaps + normalmaps because those two use the same layers.
        // People can do it using custom shaders though and by instructing the shader how to interpret the texture
        // in a particular layer.
    
        //protected float _tileU = 1;        // landscape only has tile (aka scale)
        //protected float _tileV = 1;
        //protected float _translationU = 0; // mesh and actors have translation offset, scale and rotation
        //protected float _translationV = 0;
    //    protected Types.Vector2f _tscale;
    //    protected Types.Vector2f _toffset;  // valid values are between 0.0 and 1.0. Actually higher values are valid too but effectively the value before the decimal is ignored.  Only the fractional amount modifies the offset
    //    protected float _rotation = 0;     // angle in radians

        protected int _tvfactoryIndex = -1;
        private TextureType _textureType;

        protected PageableResourceStatus _resourceStatus;

        //protected int _layerID; // layerID's are set by the particular dervived texture types automatically (e.g. NormalMap sets layer = 1)


        private Texture(string resourcePath)
            : base(resourcePath)
        {
            // NOTE: Texture is not immediately loaded in constructor.
            // It's scheduled for paging instead.
        }

        private Texture(int index, MTV3D65.TV_TEXTURE textureInfo)
            : this(ImportLib.AbsolutePathToRelative(textureInfo.Filename))
        {
            // tv's textureInfo returns texture paths in absolute path format
            _id = ImportLib.AbsolutePathToRelative(textureInfo.Filename);
            _tvfactoryIndex = index;
        }

        private static Texture Create(string resourcePath, TextureType textureType)
        {
            Texture t = (Texture)Repository.Get(resourcePath);
            if (t != null) return t;

            t = new Texture(resourcePath);
            t._id = resourcePath;
            t._textureType = textureType;
            return t;
        }

        public static Texture Create(string resourcePath)
        {
            return Create(resourcePath, TextureType.Diffuse);
        }

        public static Texture CreateAlpha(string resourcePath)
        {
            return Create(resourcePath, TextureType.Alpha);
        }

        public static Texture CreateCubeMap(string resourcePath)
        {
            return Create(resourcePath, TextureType.Cube);
        }

        public static Texture CreateVolumeTexture(string resourcePath)
        {
            return Create(resourcePath, TextureType.Volume);     
        }

        public static Texture CreateDUDVTexture(string resourcePath)
        {
            return Create(resourcePath, TextureType.DUDV);
        }

        public static Texture Create(int index)
        {
            if (Texture.IsInFactory(index))
            {
                string resourceID = ImportLib.AbsolutePathToRelative(CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
                Texture t = (Texture)Repository.Get(resourceID);
                if (t != null) return t;

                return new Texture(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
            }
            else throw new Exception("Texture does not exist in factory.");
        }


        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="specOnly">True returns the properties without any values assigned</param>
        ///// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
        //    tmp.CopyTo(properties, 3);

        //    properties[0] = new Settings.PropertySpec("tscale", _tscale.GetType().Name);
        //    properties[1] = new Settings.PropertySpec("toffset", _toffset.GetType().Name);
        //    properties[2] = new Settings.PropertySpec("trotation", _rotation.GetType().Name);

        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = _tscale;
        //        properties[1].DefaultValue = _toffset;
        //        properties[2].DefaultValue = _rotation;
        //    }

        //    return properties;
        //}

        //public override void SetProperties(Settings.PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "tscale":
        //                _tscale = (Types.Vector2f)properties[i].DefaultValue;
        //                break;
        //            case "toffset":
        //                _toffset = (Types.Vector2f)properties[i].DefaultValue;
        //                break;
        //            case "trotation":
        //                _rotation = (float)properties[i].DefaultValue;
        //                break;

        //        }
        //    }
        //    SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
        //}


        #region IPageableTVNode Members
        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public bool TVResourceIsLoaded
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
            get { return _id; }
            set { _id = value; }
        }

        public PageableResourceStatus ResourceStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public virtual void LoadTVResource()
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_id);
            if (descriptor.IsArchivedResource)
            {
                System.Runtime.InteropServices.GCHandle gchandle;
                string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.ArchiveEntryName, "", Keystone.Core.FullArchivePath(descriptor.RelativePathToArchive), out gchandle);
                if (string.IsNullOrEmpty(memoryFile)) throw new Exception("Error importing file from archive.");

                _tvfactoryIndex = Load(_id, memoryFile, _textureType);
            }
            else
                _tvfactoryIndex = Load(descriptor.ArchiveEntryName, descriptor.ArchiveEntryName, _textureType);
            


            if (_tvfactoryIndex == -1)
                System.Diagnostics.Trace.WriteLine("Texture:LoadTVResource() -- Error loading texture '" + _id +  "'");

            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
        }

        public void SaveTVResource(string filepath)
        {
            if (ResourceStatus == PageableResourceStatus.Loaded)
                CoreClient._CoreClient.TextureFactory.SaveTexture(_tvfactoryIndex, filepath);
        }
        #endregion

        public override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        //relative path is path + filename starting after the default \\Data\\ path.
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Since resourcePath is not necessarily the same as file path or archive
        /// path, we pass in a seperate id</param>
        /// <param name="resourcePath">Directory path or a MEMORY point path</param>
        /// <param name="textureType"></param>
        /// <returns></returns>
        private int Load(string id, string resourcePath, TextureType textureType)
        {      
            int index = -1;
            switch (textureType)
            {
                // case TextureType.Bump:
                    // todo: LoadBumpTexture is only for heightmaps that are to be converted
                    // to normal maps
                    //CoreClient._CoreClient.TextureFactory.LoadBumpTexture (resourcePath, id);
                    //break;
                case TextureType.Diffuse :
                    index = CoreClient._CoreClient.TextureFactory.LoadTexture(resourcePath, id);
                    break;
                case TextureType.Alpha :
                    index = CoreClient._CoreClient.TextureFactory.LoadAlphaTexture(resourcePath, id);
                    break;
                case TextureType.Cube:
                    index = CoreClient._CoreClient.TextureFactory.LoadCubeTexture(resourcePath, id);
                   break;
                case TextureType.Volume:
                    throw new NotImplementedException ();
                    //index =
                    //CoreClient._CoreClient.TextureFactory.LoadVolumeTexture(resourcePath, id, colorKey, textureFilter);
                    break;
                case TextureType.DUDV :
                    throw new NotImplementedException();
                    //index = CoreClient._CoreClient.TextureFactory.LoadDUDVTexture(resourcePath, id, _width, width, amplitude, false);
                    break;
                default:
#if DEBUG
                    throw new Exception ("Unknown texture type " + _textureType.ToString());
#else
                    break;
#endif
            }   

            return index;
        }


        // attempt to create our wrapper texture from just an index (such as if the texture was loaded automatially)
        // but has not (most likely) been added to the Repository yet because it hasnt been wrapped yet.
        // NOTE: It will however be in the factory if we're loading another mesh automatically that ends up using the same texture.
        public static bool IsInFactory(int index)
        {
            TV_TEXTURE t = CoreClient._CoreClient.TextureFactory.GetTextureInfo(index);
            if (string.IsNullOrEmpty(t.Filename)) return false;
            if (string.IsNullOrEmpty(t.Name))
                throw new Exception(
                    "Texture.IsInFactor() -- Texture name is null or empty.  Make sure automatically loaded model textures have names.");
            return true;
        }


        #region ResourceBase members
        protected override void DisposeUnmanagedResources()
        {
            CoreClient._CoreClient.TextureFactory.DeleteTexture(_tvfactoryIndex);
            base.DisposeUnmanagedResources();
        }
        #endregion
        //public int LayerID
        //{
        //    get { return _layerID; }
        //}

        //public bool TextureModEnabled
        //{
        //    get 
        //    {
        //        return _tscale.x != 1 || _tscale.y != 1 ||
        //            _toffset.x != 0 || _toffset.y != 0 ||
        //            _rotation != 0;
        //    }
        //}

        //public float TileU
        //{
        //    get { return _tscale.x; }
        //    set
        //    {
        //        _tscale.x = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}

        //public float TileV
        //{
        //    get { return _tscale.y; }
        //    set
        //    {
        //        _tscale.y = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}

        ///// <summary>
        ///// Valid value is between 0.0 and 1.0f
        ///// </summary>
        //public float TranslationU
        //{
        //    get { return _toffset.x; }
        //    set
        //    {
        //        _toffset.x = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}

        ///// <summary>
        ///// Valid value is between 0.0 and 1.0f
        ///// </summary>
        //public float TranslationV
        //{
        //    get { return _toffset.y; }
        //    set
        //    {
        //        _toffset.y = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}

        //public float Rotation
        //{
        //    get { return _rotation ; }
        //    set
        //    {
        //        _rotation = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}
    }
}