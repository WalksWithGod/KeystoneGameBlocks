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
        public enum TEXTURETYPE : byte
        {
            Default = 0,
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
    
        protected readonly object mSyncRoot;
        protected int _tvfactoryIndex = -1;
        protected int mWidth;
        protected int mHeight;
        protected TEXTURETYPE _textureType;

        protected PageableNodeStatus _resourceStatus;


        internal Texture(string resourcePath)
            : base(resourcePath)
        {
            // NOTE: Texture is not immediately loaded. 
            Shareable = true;
            mSyncRoot = new object();
            _textureType = TEXTURETYPE.Default;
        }

        private Texture(int index, MTV3D65.TV_TEXTURE textureInfo)
            : this(ImportLib.AbsolutePathToRelative(textureInfo.Filename))
        {
            // tv's textureInfo returns texture paths in absolute path format
            _id = ImportLib.AbsolutePathToRelative(textureInfo.Filename);
            _tvfactoryIndex = index;
            TV_TEXTURE info = CoreClient._CoreClient.TextureFactory.GetTextureInfo(_tvfactoryIndex);
            mWidth = info.Width;
            mHeight = info.Height;

            _resourceStatus = PageableNodeStatus.Loaded;
        }
        
        // TextureType detemines how this Texture is loaded whether as default, cubemap, dudv.
        // NormalMaps, EmissiveMaps, SpecularMaps and DiffuseMaps and textures where you'll 
        // employ alpha blending should ALL use TEXTURETYPE.Default.
		// TEXTURETYPE.Alpha is only for ..... i forget, but trust me its not for things like transparent billboard or particle textures      
        public TEXTURETYPE TextureType 
        {
        	get {return _textureType; } 
        	set 
        	{
        		_textureType = value;
        		//System.Diagnostics.Debug.Assert (TVResourceIsLoaded == false && ResourceStatus != PageableResourceStatus.Loading || ResourceStatus != PageableResourceStatus.Loaded);
        	}
        }
//
//        public static Texture Create(string resourcePath)
//        {
//            return Create(resourcePath, TextureType.Diffuse);
//        }
//
//        public static Texture CreateAlpha(string resourcePath)
//        {
//            return Create(resourcePath, TextureType.Alpha);
//        }
//
//        public static Texture CreateCubeMap(string resourcePath)
//        {
//            return Create(resourcePath, TextureType.Cube);
//        }
//
//        public static Texture CreateVolumeTexture(string resourcePath)
//        {
//            return Create(resourcePath, TextureType.Volume);     
//        }
//
//        public static Texture CreateDUDVTexture(string resourcePath)
//        {
//            return Create(resourcePath, TextureType.DUDV);
//        }

        // TODO: here we have problem in that one resource loaded as different textureType would use same "id" and we cant allow that
        //       either we disallow or we 
//        private static Texture Create(string resourcePath, TextureType textureType)
//        {
//            Texture t = (Texture)Repository.Get(resourcePath);
//            if (t != null) return t;
//
//            t = new Texture(resourcePath);
//            System.Diagnostics.Debug.Assert(t._id == resourcePath, "The following _id assignment seems redundant.  No assert means we can delete that line.");
//            t._id = resourcePath; 
//            t._textureType = textureType;
//            return t;
//        }

        public static Texture Create(int index)
        {
            if (Texture.IsInFactory(index))
            {
            	MTV3D65.TV_TEXTURE info = CoreClient._CoreClient.TextureFactory.GetTextureInfo(index);
                string resourceID = ImportLib.AbsolutePathToRelative(info.Filename);
                Texture t = (Texture)Repository.Get(resourceID);
                if (t != null) return t;

                t = new Texture(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
                switch (info.TextureType)
                {
                	case CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP:
   		                t.TextureType = TEXTURETYPE.Default ;
                		break;
                	//case CONST_TV_TEXTURETYPE.:
   		            //    t.TextureType = TEXTURETYPE.Alpha ;
                	//	break;
                	case CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBICMAP:
   		                t.TextureType = TEXTURETYPE.Cube ;
                		break;
                	case CONST_TV_TEXTURETYPE.TV_TEXTURE_VOLUMEMAP:
   		                t.TextureType = TEXTURETYPE.Volume ;
                		break;
            		case CONST_TV_TEXTURETYPE.TV_TEXTURE_DUDVMAP:
   		                t.TextureType = TEXTURETYPE.DUDV;
                		break;
                	default:
                		throw new ArgumentException ("Texture.Create() - Currently unsupported texture type.");
                }
				return t;
            }
            else throw new Exception("Texture does not exist in factory. NOTE: If index is for RenderSurface.GetTexture() this will fail.");
        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 

        #region Resource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);
            
            properties[0] = new Settings.PropertySpec("texturetype", typeof(byte).Name);

            if (!specOnly)
            {
            	properties[0].DefaultValue = (byte)_textureType;

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
                    case "texturetype":
                		_textureType = (TEXTURETYPE)(byte)properties[i].DefaultValue;
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }
#endregion

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

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
            set { throw new Exception ("Texture.ResourcePath - cannot change texture resource path since it's same as node.ID"); }
        }

        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        
        public void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
                
        public virtual void LoadTVResource()
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_id);
            if (descriptor.IsArchivedResource)
            {
                System.Runtime.InteropServices.GCHandle gchandle;
                string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.EntryName, "", Keystone.Core.FullNodePath(descriptor.ModName), out gchandle);
                if (string.IsNullOrEmpty(memoryFile)) throw new Exception("Error importing file from archive.");

                _tvfactoryIndex = Load(_id, memoryFile, _textureType);
            }
            else
            {
            	string resourcePath = Core.FullNodePath (descriptor.EntryName);
            	_tvfactoryIndex = Load(descriptor.EntryName, resourcePath, _textureType);
            }


            if (_tvfactoryIndex == -1)
                System.Diagnostics.Trace.WriteLine("Texture:LoadTVResource() -- Error loading texture '" + _id +  "'");

            TV_TEXTURE info = CoreClient._CoreClient.TextureFactory.GetTextureInfo(_tvfactoryIndex);
            mWidth = info.Width;
            mHeight = info.Height;

            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }

        public void SaveTVResource(string filepath)
        {
            if (PageStatus == PageableNodeStatus.Loaded)
                CoreClient._CoreClient.TextureFactory.SaveTexture(_tvfactoryIndex, filepath);
        }
        #endregion

        internal override ChildSetter GetChildSetter()
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
        private int Load(string id, string resourcePath, TEXTURETYPE textureType)
        {      
            // NOTE: Forum post on combining different diffuse and alpha channel masks
            // at runtime   http://www.truevision3d.com/forums/tv3d_sdk_65/how_to_use_a_nondds_alpha_texture-t15554.0.html;msg107075#msg107075
            int index = -1;
            switch (textureType)
            {
                // case TextureType.Bump:
                    // TODO: LoadBumpTexture is only for Dot3 bump textures which have
                //  heightmap info baked in with the normal ifno. So LoadBumpTexture
                // converts them to regular normal maps with offset info in a seperate channel
                    // to normal maps
                    //CoreClient._CoreClient.TextureFactory.LoadBumpTexture (resourcePath, id);
                    //break;
                case TEXTURETYPE.Default :
                    // TODO: LoadTexture(id, path) loads .dds with alpha just fine, but png with alpha seems to require TV_COLORKEY_USE_ALPHA_CHANNEL
                    if (resourcePath.ToLower().EndsWith (".png"))
                    {
                    	index = CoreClient._CoreClient.TextureFactory.LoadTexture(resourcePath, id, -1, -1, CONST_TV_COLORKEY.TV_COLORKEY_USE_ALPHA_CHANNEL, true);
                    	System.Diagnostics.Debug.WriteLine ("Texture.Load() - Loading PNG '" + resourcePath + "' with TV_COLORKEY_USE_ALPHA_CHANNEL");
                    }
                    else
                   		index = CoreClient._CoreClient.TextureFactory.LoadTexture(resourcePath, id);
                    break;
                case TEXTURETYPE.Alpha :
                    // TODO: need to confirm, but i think alpha textures are textures where alpha values are not actually
                    // stored in the alpha component of the image but in the other channels, but where we wish to treat all of that as alpha
                    index = CoreClient._CoreClient.TextureFactory.LoadAlphaTexture(resourcePath, id);
                    break;
                case TEXTURETYPE.Cube:
                    index = CoreClient._CoreClient.TextureFactory.LoadCubeTexture(resourcePath, id);
                   break;
                case TEXTURETYPE.Volume:
                    index =
                    CoreClient._CoreClient.TextureFactory.LoadVolumeTexture(resourcePath, id); // TODO: other posible parameters -> colorKey, textureFilter);
                    break;
                case TEXTURETYPE.DUDV :
                    //throw new NotIm 
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

        public void LockTexture()
        {
            CoreClient._CoreClient.TextureFactory.LockTexture(_tvfactoryIndex);
        }

        public void UnlockTexture()
        {
            CoreClient._CoreClient.TextureFactory.UnlockTexture(_tvfactoryIndex);
        }

        public int GetPixel(int x, int y)
        {
            if (TVResourceIsLoaded == false) return -1;
            if (PageStatus != PageableNodeStatus.Loaded) return -1;

            // must return if pixel is out of bounds or access violation will occur
            if (x < 0 || y < 0 || x >= mWidth || y >= mHeight) return -1;

            // note: we don't lock textures here in case caller wishes to do it for a loop of calls to this method
            return CoreClient._CoreClient.TextureFactory.GetPixel(_tvfactoryIndex, x, y);
        }

        // http://wiki.truevision3d.com/tutorialsarticlesandexamples/bake_terrain_splatting_to_a_single_texture
        public void SetPixel(int x, int y, int color)
        {
            if (TVResourceIsLoaded == false) return;
            if (PageStatus != PageableNodeStatus.Loaded) return;

            // must return if pixel is out of bounds or access violation will occur
            if (x < 0 || y < 0 || x >= mWidth || y >= mHeight) return;
            
            // note: we don't lock textures here in case caller wishes to do it for a loop of calls to this method
            CoreClient._CoreClient.TextureFactory.SetPixel(_tvfactoryIndex, x, y, color);            
        }

        public int[] GetPixelArray(int x, int y, int width, int height)
        {
            if (TVResourceIsLoaded == false) return null;
            if (PageStatus != PageableNodeStatus.Loaded) return null;

            if (width < 1 || height < 1) return null;
            
            // must return if pixel is out of bounds or access violation will occur
            if (x < 0 || y < 0 || x + width >= mWidth || y + height >= mHeight) return null;

            int[] colors = new int[width * height];

            CoreClient._CoreClient.TextureFactory.LockTexture(_tvfactoryIndex, true);
            CoreClient._CoreClient.TextureFactory.GetPixelArray(_tvfactoryIndex, x, y, width, height, colors);
            CoreClient._CoreClient.TextureFactory.UnlockTexture(_tvfactoryIndex);

            return colors;
        }

        /// <summary>
        /// Set's pixels using the supplied colors and 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color">Array of colors to assign.  Array length must equal width * height</param>
        public void SetPixelArray(int x, int y, int width, int height, int[] colors)
        {
            if (TVResourceIsLoaded == false) return;
            if (PageStatus != PageableNodeStatus.Loaded) return;

            if (width < 1 || height < 1) return;
            if (colors == null || colors.Length != width * height) return;
            // must return if pixel is out of bounds or access violation will occur
            if (x < 0 || y < 0 || x + width > mWidth || y + height > mHeight) return;

            CoreClient._CoreClient.TextureFactory.LockTexture(_tvfactoryIndex, false);
            CoreClient._CoreClient.TextureFactory.SetPixelArray(_tvfactoryIndex, x, y, width, height, colors);
            //CoreClient._CoreClient.TextureFactory.SetPixelArrayFloatEx 
            CoreClient._CoreClient.TextureFactory.UnlockTexture(_tvfactoryIndex);
        }


        public override int GetHashCode()
        {
            uint result = 0;
            Keystone.Utilities.JenkinsHash.Hash(_id, ref result);
            // the factory index will cause hashcode to change depending on whether the texture
            // is loaded or not.  That is probably not proper .net code... oh well.
            Keystone.Utilities.JenkinsHash.Hash (BitConverter.GetBytes(_tvfactoryIndex) , ref result);

            return (int)result;
        }

        #region ResourceBase members
        protected override void DisposeUnmanagedResources()
        {
            if (TVResourceIsLoaded)
                CoreClient._CoreClient.TextureFactory.DeleteTexture(_tvfactoryIndex);
    
            base.DisposeUnmanagedResources();
        }
        #endregion
    }
}