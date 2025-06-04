using System;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.RenderSurfaces
{
    public class RenderSurface : ResourceBase
    {
        // TODO: i think RenderSurfacePool should be a public static variable so that externally we can run a maintenance
        // call on it
        private static RenderSurfacePool<RenderSurface> _pool = new RenderSurfacePool<RenderSurface>(60000);
        public CONST_TV_TEXTURETYPE _surfaceType; //used for the IsCubeMap
        public RSResolution _resolution;

        /// <summary>
        /// if .DEFAULT color is selected, this will get translated to the proper 16 or 32 bit depth based on GraphicsDevice.BPP
        /// </summary>
        public CONST_TV_RENDERSURFACEFORMAT _colorFormat;

        public bool _useDepthBuffer;
        public bool _useMainBuffer;
        public double _mainBufferScale;

        private TVRenderSurface _rs;
        public int Height;
        public int Width;
        private static int counter = 0;

        // accepts an instanced RS and other info about the RS
        private RenderSurface(TVRenderSurface rs, RSResolution resolution, CONST_TV_TEXTURETYPE surfaceType,
                              CONST_TV_RENDERSURFACEFORMAT colorFormat, bool useDepthBuffer,
                              bool useMainBuffer, double mainBufferScale)
            : base(Repository.GetNewName(typeof (RenderSurface)))
        {
            GetRSResolutionDimensions(resolution, out Width, out Height);
            _resolution = resolution;
            _colorFormat = colorFormat; // essentially its the color depth format
            _surfaceType = surfaceType;
            _rs = rs;
            _useDepthBuffer = useDepthBuffer;
            _useMainBuffer = useMainBuffer;
            _mainBufferScale = mainBufferScale;
            _id = Repository.GetNewName(this.GetType()) + counter++;

            if (surfaceType == CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBERENDERSURFACE)
            {
                for (int i = 0; i < 6; i++)
                {
                    rs.StartCubeRender(i);
                    rs.EndCubeRender(i);
                }
            }
            else
            {
                // clear the RS since by default they are junky on creation
                rs.StartRender(false);
                rs.EndRender();

                
            }
        }

        private RenderSurface(TVRenderSurface rs, uint width, uint height, CONST_TV_TEXTURETYPE surfaceType,
                              CONST_TV_RENDERSURFACEFORMAT colorFormat, bool useDepthBuffer,
                              bool useMainBuffer, double mainBufferScale)
            : base(Repository.GetNewName(typeof(RenderSurface)) + counter++)
        {
            
            _colorFormat = colorFormat; // essentially its the color depth format
            _surfaceType = surfaceType;
            _rs = rs;
            _useDepthBuffer = useDepthBuffer;
            _useMainBuffer = useMainBuffer;
            _mainBufferScale = mainBufferScale;

            if (surfaceType == CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBERENDERSURFACE)
            {
                for (int i = 0; i < 6; i++)
                {
                    rs.StartCubeRender(i);
                    rs.EndCubeRender(i);
                }
            }
            else
            {
                // clear the RS since by default they are junky on creation
                rs.StartRender(false);
                rs.EndRender();
            }
        }


        public void Release()
        {
            _pool.CheckIn(this);
            // Core._CoreClient.RSPool.CheckIn(this);
        }

        // since our pool will retain a reference, this will truely only be called when the Pool releases the reference
        protected override void DisposeUnmanagedResources()
        {
            // TODO: this is throwing an exception on close... why?
            if (_rs != null)
            {
                _rs.Destroy();
                _rs = null;
            }
        }

        //TODO: Ideally these loading functions need to be sent to a worker thread
        // 1) User requests a RS by calling for example, RenderSurface.CreateCubeRS(bleh);
        // 2) This function first checks the pool for an existing available one by calling
        //    CheckOut() with all of the arguments.  Then here in DisposeManagedResources() we
        //    will check it back in
        // 
        // 3) While there are available ones, validate it to make sure its the proper requested specification
        // 4) if validate = true, then that existing RenderSurface object is returned and its added to the locked hashtable
        // 5) If no available ones, then the "create" is called which calls back here to create the actual
        //    proper RS and wrapper RenderSurface object.  That is returned to the CheckOut() method
        //   which adds it to the locked pool and sets the expiration key
        public static RenderSurface CreateCubeRS(RSResolution resolution, CONST_TV_RENDERSURFACEFORMAT colorFormat,
                                                 bool useDepthBuffer)
        {
            int height, width;
            GetRSResolutionDimensions(resolution, out width, out height);

            RSFormatInfo format = new RSFormatInfo();
            format.Resolution = resolution;
            format.SurfaceType = CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBERENDERSURFACE;
            format.ColorFormat = colorFormat;
            format.UseDepthBuffer = useDepthBuffer;
            format.UseMainBuffer = false;
            format.MainBufferScale = 1;

            RenderSurface poolitem = _pool.CheckOut(format);
            if (poolitem == null)
            {
                poolitem =
                    new RenderSurface(
                        CoreClient._CoreClient.Scene.CreateCubeRenderSurface(width, useDepthBuffer, (int)colorFormat),
                        resolution, CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBERENDERSURFACE,
                        colorFormat, useDepthBuffer, false, 1);
                _pool.CheckOut(poolitem);
            }
            return poolitem;
        }

        // NOTE: 
        public static RenderSurface CreateAlphaRS(RSResolution resolution, bool useDepthBuffer)
        {
            int height, width;
            GetRSResolutionDimensions(resolution, out width, out height);

            RSFormatInfo format = new RSFormatInfo();
            format.Resolution = resolution;
            format.SurfaceType = CONST_TV_TEXTURETYPE.TV_TEXTURE_RENDERSURFACE;
            format.ColorFormat = CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT;
            format.UseDepthBuffer = useDepthBuffer;
            format.UseMainBuffer = false;
            format.MainBufferScale = 1;

            RenderSurface poolitem = _pool.CheckOut(format);
            if (poolitem == null)
            {
                poolitem = new RenderSurface(CoreClient._CoreClient.Scene.CreateAlphaRenderSurface(width, height, useDepthBuffer),
                                             resolution, CONST_TV_TEXTURETYPE.TV_TEXTURE_RENDERSURFACE,
                                             CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT,
                                             useDepthBuffer, false, 1);
                // NOTE: an alpha RS still by default has alpha as 1 for all pixels.  Must
                //       set to 0 for full transparency
                poolitem.SetBackgroundColor (1,1,1,0); // 
                _pool.CheckOut(poolitem);
            }
            return poolitem;
        }

        public static RenderSurface CreateRS(RSResolution resolution, CONST_TV_RENDERSURFACEFORMAT colorFormat,
                                             bool useDepthBuffer)
        {
            return CreateRSEx(resolution, colorFormat, useDepthBuffer, true, 1);
        }

        public static RenderSurface CreateRSEx(RSResolution resolution, CONST_TV_RENDERSURFACEFORMAT colorFormat,
                                               bool useDepthBuffer, bool useMainBufferSize, float mainBufferScale)
        {
            int height, width;
            GetRSResolutionDimensions(resolution, out width, out height);

            RSFormatInfo format = new RSFormatInfo();
            format.Resolution = resolution;
            format.SurfaceType = CONST_TV_TEXTURETYPE.TV_TEXTURE_RENDERSURFACE;
            format.ColorFormat = colorFormat;
            format.UseDepthBuffer = useDepthBuffer;
            format.UseMainBuffer = useMainBufferSize;
            format.MainBufferScale = mainBufferScale;

            RenderSurface poolitem = _pool.CheckOut(format);
            if (poolitem == null)
            {
                poolitem =
                    new RenderSurface(
                        CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(width, height, colorFormat, useDepthBuffer,
                                                               useMainBufferSize, mainBufferScale),
                        resolution, CONST_TV_TEXTURETYPE.TV_TEXTURE_RENDERSURFACE, colorFormat,
                        useDepthBuffer, useMainBufferSize, mainBufferScale);
                _pool.CheckOut(poolitem);
            }
            return poolitem;
        }

        public static RenderSurface CreateRSEx(uint width, uint height, CONST_TV_RENDERSURFACEFORMAT colorFormat,
                                       bool useDepthBuffer, bool useMainBufferSize, float mainBufferScale)
        {

            RenderSurface poolitem = null; // _pool.CheckOut(format);
            if (poolitem == null)
            {
                poolitem =
                    new RenderSurface(
                        CoreClient._CoreClient.Scene.CreateRenderSurfaceEx((int)width, (int)height,
                                                                           colorFormat, useDepthBuffer,
                                                                           useMainBufferSize, mainBufferScale),
                        width, height,
                        CONST_TV_TEXTURETYPE.TV_TEXTURE_RENDERSURFACE, colorFormat,
                        useDepthBuffer, useMainBufferSize, mainBufferScale);
                _pool.CheckOut(poolitem);
            }
            return poolitem;
        }

        public int Size
        {
            get { return Width; }
        }

        public TVRenderSurface RS
        {
            get { return _rs; }
        }

        public void SetNewCamera(TVCamera cam)
        {
            _rs.SetNewCamera(cam);
        }

        public TVCamera GetCamera()
        {
            return _rs.GetCamera();
        }

        public int GetTexture()
        {
            return _rs.GetTexture();
        }

        public void SetCubeMapProperties(bool automaticCameraHandling, Vector3d position)
        {
            _rs.SetCubeMapProperties(automaticCameraHandling, Helpers.TVTypeConverter.ToTVVector(position));
        }

        // NOTE: even on alpha RS, default alpha component across all pixels is 1.0 fully opaque
        public void SetBackgroundColor(float r, float g, float b, float a)
        {
            SetBackgroundColor(CoreClient._CoreClient.Globals.RGBA(r, g, b, a));
        }

        public void SetBackgroundColor(int color)
        {
            _rs.SetBackgroundColor(color);
        }

        public void StartRender(bool clearZOnly)
        {
            _rs.StartRender(clearZOnly);
        }

        public void StartRender()
        {
            _rs.StartRender();
        }

        public void EndRender()
        {
            _rs.EndRender();
        }

        public void SaveTexture(string path, CONST_TV_IMAGEFORMAT format)
        {
            _rs.SaveTexture(path, format);
        }

        public void StartCubeRender(int faceNum)
        {
            _rs.StartCubeRender(faceNum);
        }

        public void EndCubeRender(int faceNum)
        {
            _rs.EndCubeRender(faceNum);
        }


        public bool IsCubeMap
        {
            get { return _surfaceType == CONST_TV_TEXTURETYPE.TV_TEXTURE_CUBERENDERSURFACE; }
        }

        public static void GetRSResolutionDimensions(RSResolution resolution, out int width, out int height)
        {
            switch (resolution)
            {
                case RSResolution.R_32x32:
                    width = height = 32;
                    break;

                case RSResolution.R_64x64:
                    width = height = 64;
                    break;
                case RSResolution.R_128x128:
                    width = height = 128;
                    break;
                case RSResolution.R_256x256:
                    width = height = 256;
                    break;
                case RSResolution.R_384x384:
                    width = height = 384;
                    break;
                case RSResolution.R_512x512:
                    width = height = 512;
                    break;
                case RSResolution.R_768x768:
                    width = height = 768;
                    break;
                case RSResolution.R_896x896:
                    width = height = 896;
                    break;
                case RSResolution.R_1024x1024:
                    width = height = 1024;
                    break;
                case RSResolution.R_1152x1152:
                    width = height = 1152;
                    break;
                case RSResolution.R_1536x1536:
                    width = height = 1536;
                    break;
                case RSResolution.R_2048x2048:
                    width = height = 2048;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static RSResolution GetRSResolution(int size)
        {
            switch (size)
            {
                case 32:
                    return RSResolution.R_32x32;
                case 64:
                    return RSResolution.R_64x64;
                case 128:
                    return RSResolution.R_128x128;
                case 256:
                    return RSResolution.R_256x256;
                case 384:
                    return RSResolution.R_384x384;
                case 512:
                    return RSResolution.R_512x512;
                case 768:
                    return RSResolution.R_768x768;
                case 896:
                    return RSResolution.R_896x896;
                case 1024:
                    return RSResolution.R_1024x1024;
                case 1152:
                    return RSResolution.R_1152x1152;
                case 1536:
                    return RSResolution.R_1536x1536;
                case 2048:
                    return RSResolution.R_2048x2048;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}