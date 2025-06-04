using System;
using System.Collections.Generic;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.RenderSurfaces;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using MTV3D65;

namespace Keystone.Resource
{
    public class ImposterPool
    {
        private int _totalImposters;
        private const int m_NumImposters = 1;
        private const int NUM_IMPOSTER_VERTS = 6; // 3 for each triangle in the quad
        private const int TRIANGLES_PER_QUAD = 2;
        private int _maxImposterVertices;

        private RSResolution _resolution;
        private RenderSurface _rs;
        private Texture _d3dtex;
        private VertexFormats _FVF;
        private int _FVFSize;
        private VertexBuffer _vertexBuffer;
        private CustomVertex.PositionTextured[] _vertices;

        private Usage _useage;
        private Device _device = new Device(CoreClient._CoreClient.Internals.GetDevice3D());
        private Pool _pool;

        private int _width;
        private int _height;
        private int _impostersX;
        private int _impostersY;

        private Vector2 _scale;

        private Stack<Imposter> _free = new Stack<Imposter>();
        private List<Imposter> _busy = new List<Imposter>();


        private StateBlock _imposterStateBlock;
        private StateBlock _defaultStateBlock;

        /// <summary>
        /// An imposter pool is a collection of imposter objects that share
        /// the same vertex buffer and the same render surface.  Thus all
        /// visible imposters in this pool are depth sorted and rendered in a single 
        /// DrawPrimitves() call.
        /// </summary>
        /// <param name="resolution">The size of the total rendersurface</param>
        /// <param name="numImpostersU"></param>
        /// <param name="numImpostersV"></param>
        public ImposterPool(RSResolution resolution, int numImpostersU, int numImpostersV)
        {
            //TODO: try catch

            _resolution = resolution;
            // Pre-allocate all imposters in an array.
            _impostersY = numImpostersU;
            _impostersX = numImpostersV;

            _totalImposters = _impostersY * _impostersX;
            _maxImposterVertices = _totalImposters * NUM_IMPOSTER_VERTS;

            // sort belongs in the
            //m_pSortBuffer = new Imposter[m_MaxNumImposters];


            _scale.X = 1.0f / numImpostersU; // NOTE: X =  U is actually verticle
            _scale.Y = 1.0f / numImpostersV; // NOTE: Y = V and runs horizontally

            // size
            RenderSurface.GetRSResolutionDimensions(_resolution, out _width, out _height);


            _rs = RenderSurface.CreateAlphaRS(_resolution, false);
            ClearRS();

            //_imposterCam = Program.Core.CameraFactory.CreateCamera("impster");
            //_imposterRenderTexture.SetNewCamera(_imposterCam);

            if (_device.SoftwareVertexProcessing)
                throw new Exception("Unsupported GPU.");

            // NOTE: with Pool.Managed in our VertexBuffer, the Usage type is limited.  So far Usage.None works but Usage.Dynamic | Usage.WriteOnly does not.
            // TODO: BUT, I NEED / WANT TO USE DYNAMIC buffers!!!!  Also read this:
            // http://www.pluralsight.com/blogs/craig/archive/2005/03/14/6693.aspx
            _pool = Pool.Default;
            _useage = Usage.Dynamic | Usage.WriteOnly;
            _FVF = CustomVertex.PositionTextured.Format;
            _FVFSize = CustomVertex.PositionTextured.StrideSize;

            AllocImposters(_impostersY, _impostersX);

            CreateStateBlocks();
        }


        ~ImposterPool()
        {
            ReleaseStateBlocks();
        }

        // called and is required to be called when the device is reset
        public void ReleaseStateBlocks()
        {
            if (_imposterStateBlock != null)
            {
                _imposterStateBlock.Dispose();
                _imposterStateBlock = null;
            }
            if (_defaultStateBlock != null)
            {
                _defaultStateBlock.Dispose();
                _defaultStateBlock = null;
            }
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _vertexBuffer = null;
            if (_d3dtex != null) _d3dtex.Dispose();
            _d3dtex = null;
            //GC.Collect();
        }

        // Create our imposter stateblock and the default rollback block 
        public void CreateStateBlocks()
        {
            // Bind the imposter texture.  Do this BEFORE the stateblock code
            IntPtr textureptr = CoreClient._CoreClient.Internals.GetTexture(_rs.RS.GetTexture());
            _d3dtex = new Texture(textureptr);

            // Records a stateblock for use with our imposters.  The actual device remains unchanged.
            _device.BeginStateBlock();
            SetupStateBlock();
            _imposterStateBlock = _device.EndStateBlock();

            // _defaultStateBlock will capture the existing device state prior to applying the _imposterStateBlock
            _device.BeginStateBlock();
            SetupStateBlock();
            _defaultStateBlock = _device.EndStateBlock();

            _vertexBuffer =
                new VertexBuffer(typeof(CustomVertex.PositionTextured), _maxImposterVertices, _device, _useage,
                                 _FVF, _pool);
        }

        // create a state block of all the states we will need.  
        //  NOTE: This does not set them to the device, it just creates it
        private void SetupStateBlock()
        {
            _device.SetTexture(0, _d3dtex);
            // Set world-tranform to identity (imposter billboard's vertices are already in world-space).
            _device.SetTransform(TransformType.World, Matrix.Identity);

            // NOTE: We disable lighting since we rendered it with the proper lighting
            // and we enable fog (assuming it was already enabled) TODO: need to track whether we had to turn it off or not
            //// Setup render state. 
            _device.SetRenderState(RenderStates.CullMode, (int)Cull.CounterClockwise);
            //_device.SetRenderState( RenderStates.DiffuseMaterialSource ,false);
            _device.SetRenderState(RenderStates.Lighting, false);
            //_device.SetRenderState(RenderStates.FogEnable, enableFog);
            //_device.SetRenderState(RenderStates.FogTableMode , D3DFOG_EXP2);
            //_device.SetRenderState(RenderStates.FogColor , fogColor);
            //_device.SetRenderState(RenderStates.FogDensity, FloatToDWORD(fogDensity));
            _device.SetRenderState(RenderStates.ZBufferWriteEnable, false);
            _device.SetRenderState(RenderStates.AlphaBlendEnable, false);
            _device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            _device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            _device.SetRenderState(RenderStates.AlphaTestEnable, true);
            _device.SetRenderState(RenderStates.ReferenceAlpha, 0);
            _device.SetRenderState(RenderStates.AlphaFunction, (int)Compare.Greater);

            //_device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Modulate);
            //_device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.Diffuse);
            // _device.SetTextureStageState(0, TextureStageStates.ColorArgument2, (int)TextureArgument.TextureColor);
            //_device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.Modulate);
            // _device.SetTextureStageState(0, TextureStageStates.AlphaArgument1, (int)TextureArgument.Diffuse);
            //_device.SetTextureStageState(0, TextureStageStates.AlphaArgument2, (int)TextureArgument.TextureColor);
            _device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
            _device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
            _device.SetSamplerState(0, SamplerStageStates.AddressU, (int)TextureAddress.Clamp);
            _device.SetSamplerState(0, SamplerStageStates.AddressV, (int)TextureAddress.Clamp);
        }

        public Vector2 Scale
        {
            get { return _scale; }
        }

        public Vector2 Size
        {
            get { return new Vector2(_width, _height); }
        }

        public Vector2 ImposterSize
        {
            get
            {
                int width, height;
                RenderSurface.GetRSResolutionDimensions(_resolution, out width, out height);
                return new Vector2(width, height);
            }
        }

        public int Count
        {
            get { return _totalImposters; }
        }

        public Imposter CheckOut()
        {
            Imposter o = _free.Pop();
            _busy.Add(o);
            return o;
        }

        public void CheckIn(Imposter o)
        {
            if (!_busy.Contains(o))
                throw new ArgumentException(
                    "ImposterPool.Checkin() -- Imposter does not exist in this pool.  Cannot check in.");
            _busy.Remove(o);
            _free.Push(o);
        }

        public void ClearRS()
        {
            _rs.SetBackgroundColor(CoreClient._CoreClient.Globals.RGBA(255, 0, 255, 0));
        }

        public Device BeginRender(Camera camera)
        {
            //TODO: disable fog first if its enabled?
            //The fog will get rendered to the imposter when its rendered to the scene
            // NOTE: we use "true" to avoid wiping out the contents of the entire RS since we are not
            // updating the entire RS at once, only specific imposters in Imposter.Render() where we setup
            // a camera to look at just that spot, and we set the viewport's target area as well and then
            // we use device.Clear() to erase just that one spot.
            //    _rs.SetNewCamera(camera.TVCamera);
            _rs.StartRender(true);

            // pD3DDevice->SetRenderState(D3DRS_LIGHTING, TRUE);
            // pD3DDevice->SetRenderState(D3DRS_ZWRITEENABLE, TRUE);
            // pD3DDevice->SetRenderState(D3DRS_FOGENABLE, FALSE);
            // pD3DDevice->SetRenderState(D3DRS_ALPHABLENDENABLE, FALSE); //TODO: might need to enable this if the mesh being rendred has alpha in it
            // pD3DDevice->SetSamplerState(0, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
            // pD3DDevice->SetSamplerState(0, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
            // pD3DDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);

            return new Device(CoreClient._CoreClient.Internals.GetDevice3D());
        }

        public void EndRender()
        {
            _rs.EndRender();
        }

        public void SaveTexture(string path, CONST_TV_IMAGEFORMAT format)
        {
            _rs.SaveTexture(path, format);
        }

        /// Pre allocate imposters and calculate texture coordinates.
        /// NOTE: Imposters are bound to their allocation spot.  If you have a model
        /// that needs to have a higher resolution imposter, then you must
        /// find a new imposter to host it in an imposter "page" of a higher resolution 
        /// then you set the previious imposter's .active = false.
        private void AllocImposters(int numImpostersU, int numImpostersV)
        {
            // Initialise imposters and divide up the imposter render texture 
            // among all of them.
            int i = 0;
            for (int u = 0; u < numImpostersU; ++u)
            {
                for (int v = 0; v < numImpostersV; ++v)
                {
                    Imposter pImposter = new Imposter(_rs.RS);

                    // Initialise imposter members.
                    pImposter.RequiresRegeneration = true;
                    pImposter.StartTime = 0;
                    //pImposter.endTime = double.MaxValue; // not sure what this is for... maybe if 
                    pImposter.LastGeneratedTime = 0;
                    pImposter.InFrustum = false;
                    pImposter.Generated = false;
                    pImposter.CameraDistSq = 0.0f;

                    // Calculate texture coordinates.
                    float u1 = u * _scale.X;
                    float v1 = v * _scale.Y;
                    float u2 = u1 + _scale.X;
                    float v2 = v1 + _scale.Y;

                    pImposter.Left = (int)(u1 * _width);
                    pImposter.Top = (int)(v1 * _height);
                    pImposter.Width = (int)(_width * _scale.Y); // Y = V and runs horizontally
                    pImposter.Height = (int)(_height * _scale.X); //X = U and runs vertically
                    //Trace.WriteLine("Left:" + pImposter.Left + " Top:" + pImposter.Top + " Width:" + pImposter.Width +
                    //                " Height:" + pImposter.Height);
                    pImposter.Vertices[0].Tu = u1;
                    pImposter.Vertices[0].Tv = v1;

                    pImposter.Vertices[1].Tu = u2;
                    pImposter.Vertices[1].Tv = v1;

                    pImposter.Vertices[2].Tu = u1;
                    pImposter.Vertices[2].Tv = v2;

                    pImposter.Vertices[3].Tu = u2;
                    pImposter.Vertices[3].Tv = v1;

                    pImposter.Vertices[4].Tu = u2;
                    pImposter.Vertices[4].Tv = v2;

                    pImposter.Vertices[5].Tu = u1;
                    pImposter.Vertices[5].Tv = v2;

                    _free.Push(pImposter);
                    ++i;
                }
            }
        }


        // note: these verts must be of the format specified in hte vertex buffer
        // namely Translation, Textured
        public void RenderImposters()
        {
            if (_busy.Count > 0)
            // before changing any states.
            {
                // capture the existing state for every state item we have set in our state block
                //        _imposterStateBlock.Capture();

                // now set the state on the device for use
                _defaultStateBlock.Capture();
                _imposterStateBlock.Apply();

                // TODO: would it be possible to use a fixed vertex buffer that i then tweak the vert positions of
                //  and then just update the index buffer for the ones i want to render?
                // Initialise vertex buffer.                
                _vertices = new CustomVertex.PositionTextured[NUM_IMPOSTER_VERTS * _busy.Count];

                //TODO: depth sort
                int i = 0;
                int visible = 0;
                foreach (Imposter p in _busy)
                {
                    if (p.Generated && p.InFrustum)
                    {
                        _vertices[i++] = p.Vertices[0];
                        _vertices[i++] = p.Vertices[1];
                        _vertices[i++] = p.Vertices[2];
                        _vertices[i++] = p.Vertices[3];
                        _vertices[i++] = p.Vertices[4];
                        _vertices[i++] = p.Vertices[5];
                        visible++;
                    }
                }
                _vertexBuffer.SetData(_vertices, 0, 0);

                //int sizeToLock = _maxImposterVertices * vertSize; // in bytes
                //GraphicsStream pVertexBufferMem;
                //CustomVertex.PositionColoredTextured [] pVertexBufferMem;
                //pVertexBufferMem = (CustomVertex.PositionColoredTextured[])_vertexBuffer.Lock(0, LockFlags.NoSystemLock | LockFlags.Discard);

                ////for (int i = 0; i < numActiveImposters; ++i)
                ////{
                ////    Imposter pImposter = m_pSortBuffer[i];
                ////    if (!pImposter.IsActive)
                ////    {
                ////        continue;
                ////    }

                ////    // Copy verts to vertex buffer.
                ////pVertexBufferMem.Write(verts);
                //pVertexBufferMem[0] = verts[0];
                //pVertexBufferMem[1] = verts[1];
                //pVertexBufferMem[2] = verts[2];
                //pVertexBufferMem[3] = verts[3];
                //pVertexBufferMem[4] = verts[4];
                //pVertexBufferMem[5] = verts[5];
                ///}	
                //_vertexBufferGeometry.Unlock();

                _device.SetStreamSource(0, _vertexBuffer, 0);
                //_device.SetStreamSource(1, _indexBuffer, 0); // TODO: use index buffer so we can simply update the indices and not the vertices each frame
                _device.VertexFormat = _FVF;
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, visible * TRIANGLES_PER_QUAD);
                //_device.DrawIndexedUserPrimitives( PrimitiveType.TriangleList ,);

                // NOTE: We need seperate rollback stateblocks for each viewport
                // rollback the state by applying all the existing states we copied prior to setting up for rendering our imposters
                _defaultStateBlock.Apply();
            }
        }
    }
}
