using System;
using System.Collections.Generic;
using System.Diagnostics;

using Keystone.Appearance;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.Enum;
using Keystone.Interfaces;
using Keystone.IO;
using Keystone.Loaders;
using Keystone.Resource;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using MTV3D65;

namespace Keystone.Elements
{
    // our WaveFrontObj _mesh private var will contain bare bones data
    // but it's this class that will implement the routines against that data for computing bounding boxes and such.
    // we will _NOT_ need some new class to simulate TVMesh to do that.  THis _IS_ that new class
    public class MeshWaveFrontObj : Geometry, INotifyDeviceReset 
    {
        private Keystone.Types.Matrix _matrix;
        private WaveFrontObj _mesh;
      
        private int _maxVertices = 50000;
        private int _vertexCount;
        private int _triangleCount;

        private Usage _useage;
        private Device _device = new Device(CoreClient._CoreClient.Internals.GetDevice3D());
        private Pool _pool;

        private VertexFormats _FVF;
        private VertexFormats _wireFrameFVF;
        private CustomVertex.PositionColored[] _wireFrameVertices;
        private VertexBuffer _wireFrameVertexBuffer;
        private IndexBuffer _wireFrameIndexBuffer;

        private VertexDeclaration _vertexDeclaration;
        public readonly VertexElement[] _vertexElements =
                new VertexElement[] 
                {
                    new VertexElement(0, 0, 
                            DeclarationType.Float3,
                            DeclarationMethod.Default,
                            DeclarationUsage.Position,0),
                    new VertexElement(1, 0,
                            DeclarationType.Float3,
                            DeclarationMethod.Default, 
                            DeclarationUsage.Normal, 0),
                   new VertexElement(2, 0, 
                            DeclarationType.Float2,
                            DeclarationMethod.Default,
                            DeclarationUsage.TextureCoordinate, 0),
                            VertexElement.VertexDeclarationEnd
                };

        struct TexCoord
        {
            float tu, tv;

            public TexCoord(float _tu, float _tv)
            {
                tu = _tu;
                tv = _tv;
            }
        };


        private VertexBuffer _uvBuffer;
        private VertexBuffer _normalBuffer;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private static int COUNTER;
        private StateBlock _meshStateBlock;
        private StateBlock _defaultStateBlock;

        public MeshWaveFrontObj(string id) : base(id) 
        {
            // NOTE: with Pool.Managed in our VertexBuffer, the Usage type is limited.  So far Usage.None works but Usage.Dynamic | Usage.WriteOnly does not.
            // TODO: BUT, I NEED / WANT TO USE DYNAMIC buffers!!!!  Also read this:
            // http://www.pluralsight.com/blogs/craig/archive/2005/03/14/6693.aspx
            _pool = Pool.Default;
            _useage = Usage.Dynamic | Usage.WriteOnly;
            _FVF = CustomVertex.PositionNormalTextured.Format;
            _wireFrameFVF = CustomVertex.PositionColored.Format ;

            _vertexDeclaration = new VertexDeclaration(_device, _vertexElements);

            CoreClient._CoreClient.RegisterForDeviceResetNotification(this);
            CreateStateBlocks();
        }

        public static MeshWaveFrontObj Create( string resourcePath, bool loadTextures, bool loadMaterials) 
        {
            MeshWaveFrontObj mesh = (MeshWaveFrontObj)Repository.Get(resourcePath);
            if (mesh == null)
            {
                WaveFrontObj wavefrontObj = CreateMesh(resourcePath);
                mesh = new MeshWaveFrontObj(resourcePath);
                mesh._mesh = wavefrontObj;
                mesh._tvfactoryIndex = int.MaxValue - COUNTER; //TODO: this is a temp hack
                COUNTER++;

                // here we're going to populate the internal fields required for realtime modification of the mesh
                // so edge lists and adjacency lists, etc
            }
            else
            {
                Trace.WriteLine("WaveFrontMesh " + resourcePath + " already found in cache... sharing mesh instance.");
            }

            if ((loadMaterials) || (loadTextures))
            {
                // TODO: actually t his needs to be done in ParseFile since its handled differently for wavefront
                //ImportLib.GetMeshTexturesAndMaterials(mesh._mesh, resourcePath, ref appearance);
            }
            return mesh;
        }

        private static WaveFrontObj CreateMesh (string resourcePath)
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();

            string[] dummy;
            WaveFrontObj m = WavefrontObjLoader.ParseWaveFrontObj(Core.FullNodePath(resourcePath), true, true, out dummy, out dummy);
            watch.Stop();

            if (m != null)
                Trace.WriteLine("MeshWaveFrontObj.CreateMesh() - SUCCESS: '" + resourcePath + "' loaded with " + m.Groups.Count + " groups." + watch.Elapsed + "seconds, with = " + m.Points.Count + " vertices in " + m.Faces.Count + " triangles. ");

            return m;
        }

        ~MeshWaveFrontObj()
        {
            CoreClient._CoreClient.UnregisterForDeviceResetNotification(this);
            ReleaseStateBlocks();
        }

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	DisposeManagedResources();
        }
        	
        public override void LoadTVResource()
        {
            // TODO: if the mesh's src file has changed, we should unload the previous mesh first
            if (_mesh != null)
            {
                try
                {
                    //_mesh.Destroy();
                }
                catch
                {
                    Trace.WriteLine("error on Mesh.Destroy() - mesh path == " + _id);
                }
            }

            //// TODO: if _resourcePath == "" and _primitive = box, sphere, teapot, etc, we can load via tv's built in primitives
            _mesh = CreateMesh(_id);
            //_mesh.SetMeshFormat((int)_meshformat);
            //_mesh.SetCullMode(_cullMode);
            //_mesh.SetBlendingMode(_blendingMode);
            //_mesh.SetOverlay(_overlay);
            //_mesh.SetAlphaTest(_alphaTestEnable);
            //if (_alphaTestEnable)
            //    _mesh.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);

            _tvfactoryIndex = int.MaxValue - COUNTER; //TODO: this is a temp hack
            COUNTER++;
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty | Keystone.Enums.ChangeStates.MatrixDirty , Keystone.Enums.ChangeSource.Self); 
            // if no existing hull, we fallback to a box or sphere.
            // TODO: actually, th ehull should be created if the xml property says "use Hull" and
            // there is no hullPath or if the hullPath is invalid.  
            // so we need attributes for collisionHull type and be "sphere", "box", "hull"and perhaps "geometry" for full blown model which i suspect we'll never want to use
            //Hull = ConvexHull.ReadXml(xmlNode); // not implemented but i think this was for if the hull data was embedded in the xml
           // ConvexHull tmpHull = ConvexHull.GetStanHull(m); 


            // TODO: how do we load the convexhull if this LoadTVResource() doesnt get called from the server? because
            // either mesh3d itself isnt loaded or because we skip the method call on the server
            // and if mesh3d is not even laoded, then this ConvexHull property doesnt exist so where does it sit?
            // TODO: also i need to go back and look at how jiglibx handles convex hulls and whether they do in fact 
            // re-compute world hull every itteration..  In fact
            // our jiglibx PhysicsBody property could be something we use instead of convexhull property here..?
            // UPDATE: jiglibx uses Primitive object which
            //  and inside of CollisionSkin object it maintains lists of local space , world space and the previous frame's world space collision skins
            // it uses lists because you can have multiple collision skins to make up a single object such as three spheres to represent a football

        }

        public override void SaveTVResource(string resourcePath)
        {

            //if (TVResourceIsLoaded)
            //    _mesh.SaveTVM(resourcePath); // this is why name i think should not be equal to filepath right?
        }
        #endregion

        #region ResourceBase members
        protected override void DisposeUnmanagedResources()
        {
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
            	//if (_mesh != null)
	            //    _mesh.Destroy();
            }
            catch
            {
            }
        }
        #endregion

        //public void AddChild(Mesh3d m, int groupID)
        //{
        //    m.AttachTo(CONST_TV_NODETYPE.TV_NODETYPE_MESH, TVIndex, groupID, false, false);
        //    base.AddChild(m);
        //}


        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }


        #region INotifyDeviceReset Members
        public void OnBeforeReset()
        {
            ReleaseStateBlocks();
        }

        public void OnAfterReset()
        {
            CreateStateBlocks();
        }
        #endregion


        // called and is required to be called when the device is reset
        public void ReleaseStateBlocks()
        {
            if (_meshStateBlock  != null)
            {
                _meshStateBlock.Dispose();
                _meshStateBlock = null;
            }
            if (_defaultStateBlock != null)
            {
                _defaultStateBlock.Dispose();
                _defaultStateBlock = null;
            }
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _vertexBuffer = null;
            if (_indexBuffer != null) _indexBuffer.Dispose();
            _indexBuffer = null;

            if (_wireFrameVertexBuffer != null) _wireFrameVertexBuffer.Dispose();
            _wireFrameVertexBuffer = null;
            if (_wireFrameIndexBuffer != null) _wireFrameIndexBuffer.Dispose();
            _wireFrameIndexBuffer = null;

            //if (_d3dtex != null) _d3dtex.Dispose();
            //_d3dtex = null;
            //GC.Collect();
        }

        // TODO: The stateblocks and the vertex buffer and index buffer objects should be created by the Traverser
        // and then used by those meshes when we need them.  We certainly dont want multiple vertex and index buffers since they are dynamic
        // and we should just using "SetData()" on the same one anyway
        // Create our imposter stateblock and the default rollback block 
        public void CreateStateBlocks()
        {
          // // Bind the imposter texture.  Do this BEFORE the stateblock code
          // IntPtr textureptr = CoreClient._CoreClient.Internals.GetTexture(_rs.RS.GetTexture());
          // _d3dtex = new Texture(textureptr);

            // Records a stateblock for use with our imposters.  The actual device remains unchanged.
            _device.BeginStateBlock();
            SetupStateBlock();
            _meshStateBlock = _device.EndStateBlock();

            // to setup the default stateblock, you "record" the states you wish to capture within the Begin\End statements.
            // Then, when we call "Capture" on it will grab the current state on the device for just those states!  This way we can
            // easily rollback
            _device.BeginStateBlock();
            SetupStateBlock();
            _defaultStateBlock = _device.EndStateBlock();


            _vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionOnly), _maxVertices, _device, _useage, VertexFormats.Position, _pool);
            _normalBuffer = new VertexBuffer(typeof(CustomVertex.PositionOnly), _maxVertices, _device, _useage, VertexFormats.Normal, _pool);
            _uvBuffer = new VertexBuffer(typeof(TexCoord), _maxVertices, _device, _useage, VertexFormats.Texture0, _pool);

            //_vertexBuffer =
            //    new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), _maxVertices, _device, _useage,
            //                     _FVF, _pool);

            _wireFrameVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), _maxVertices, _device, _useage, _wireFrameFVF, _pool);

            // TODO: some cards cant use 16 bit indices but i suspect every SM 2.0 card will and thats all we support.
            _indexBuffer = new IndexBuffer( _device, _maxVertices * sizeof(int), _useage, _pool, false);
            _wireFrameIndexBuffer = new IndexBuffer(_device, _maxVertices * sizeof(int), _useage, _pool, false);
        }

        // create a state block of all the states we will need.  
        //  NOTE: This does not set them to the device, it just creates it
        private void SetupStateBlock()
        {

            // TODO: maybe i can avoid having to transform the coords myself  by simply setting the World transform to our own mesh's matrix
            // Set world-tranform to identity (imposter billboard's vertices are already in world-space).
            _device.SetTransform(TransformType.World, Microsoft.DirectX.Matrix.Identity);
           // _device.Transform.

            _device.SetRenderState(RenderStates.ZBufferWriteEnable, true);
            _device.RenderState.DitherEnable = true;
            _device.RenderState.FillMode = FillMode.Solid ;
            _device.RenderState.CullMode = Cull.CounterClockwise ;
            
            // TODO: these materials and textures should be tracked so they can be properly disposed afterwards...
          //  _device.SetTexture(0, Helpers.TVTypeConverter.ToD3DMaterial( _currentAppearance.Groups[i].Layers[0]));
         //   _device.Material = Helpers.TVTypeConverter.ToD3DMaterial(_currentAppearance.Groups[i].Material);
            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
            //tmpMat = Helpers.TVTypeConverter.FromKeystoneMaterial(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.red)); 
            tmpMat.Diffuse = System.Drawing.Color.Lavender;
            tmpMat.Ambient = System.Drawing.Color.GreenYellow;
            tmpMat.Specular = System.Drawing.Color.LightGray;
            tmpMat.SpecularSharpness = 15.0F;
                                    
            _device.Material = tmpMat;
            _device.RenderState.Ambient = System.Drawing.Color.White;
            _device.RenderState.SpecularEnable = true;
            _device.SetRenderState( RenderStates.DiffuseMaterialSource ,true);
           // _device.SetRenderState(RenderStates.AmbientMaterialSource, true);
           // _device.SetRenderState(RenderStates.SpecularMaterialSource, true);
            //_device.SetRenderState(RenderStates.EmissiveMaterialSource, true);

            _device.SetRenderState(RenderStates.Lighting, true);
            _device.Lights[0].Enabled = true;
            _device.SetRenderState(RenderStates.ShadeMode, (int)ShadeMode.Gouraud);

            //_device.SetRenderState(RenderStates.FogEnable, enableFog);
            //_device.SetRenderState(RenderStates.FogTableMode , D3DFOG_EXP2);
            //_device.SetRenderState(RenderStates.FogColor , fogColor);
            //_device.SetRenderState(RenderStates.FogDensity, FloatToDWORD(fogDensity));
                             // <-- TODO: must change depending on wireframe or solid
            //_device.SetRenderState(RenderStates.AlphaBlendEnable, false);
            //_device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            //_device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            //_device.SetRenderState(RenderStates.AlphaTestEnable, true);
            //_device.SetRenderState(RenderStates.ReferenceAlpha, 0);
            //_device.SetRenderState(RenderStates.AlphaFunction, (int)Compare.Greater);

            //_device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Modulate);
            //_device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.Diffuse);
            // _device.SetTextureStageState(0, TextureStageStates.ColorArgument2, (int)TextureArgument.TextureColor);
            //_device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.Modulate);
            // _device.SetTextureStageState(0, TextureStageStates.AlphaArgument1, (int)TextureArgument.Diffuse);
            //_device.SetTextureStageState(0, TextureStageStates.AlphaArgument2, (int)TextureArgument.TextureColor);
        //    _device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
        //    _device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
        //    _device.SetSamplerState(0, SamplerStageStates.AddressU, (int)TextureAddress.Clamp);
        //    _device.SetSamplerState(0, SamplerStageStates.AddressV, (int)TextureAddress.Clamp);
        }


        internal override Shader Shader
        {
            set
            {
                if (_shader == value) return;

                _shader = value;
                //if (_shader == null)
                //    _mesh.SetShader(null);
                //else
                //    _mesh.SetShader(_shader.TVShader);
            }
        }

        internal override  void SetAlphaTest (bool enable, int iGroup)
        {            
        }


        internal override CONST_TV_BLENDINGMODE BlendingMode
        {
            set
            {
                // _mesh.SetBlendingMode(value);
            }
        }

        #region Geometry Member
        //public override Keystone.Types.Matrix Matrix
        //{
        //    set
        //    {
        //        _matrix = value;
        //        //_mesh.SetMatrix(value);

        //        // NOTE: Geometry does not result in dirty bound volume if matrix is set!

        //        // the only time the boundingVolume changes is if verts are added/removed
        //        // or the Origin inside the model is moved or some other thing that effects the local space
        //        // bounding volume calc. 
        //    }
        //}

        public override int CullMode
        {
            set
            {
                _cullMode = value;
                //_mesh.SetCullMode((CONST_TV_CULLING)value);
            }
        }


        public override int VertexCount
        {
            get { return 0; }// _mesh.GetVertexCount(); }
        }

        public override int GroupCount
        {
            get { return 0; }// _mesh.GetGroupCount(); }
        }

        public override int TriangleCount
        {
            get { return 0; } // _mesh.GetTriangleCount(); }
        }


        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            PickResults result = new PickResults();
            // _mesh.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), Helpers.TVTypeConverter.ToTVVector(end), ref result, testType);
            return result;
        }


        private Appearance.Appearance _currentAppearance;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal override void Render(Keystone.Types.Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
        	
        	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
        	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
        	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
        	
        	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished adding vertices
        	// via .AddVertex()  or .SetGeometry() or even loaded the mesh from file.
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
            if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
            {
                _currentAppearance = model.Appearance;
            }
            else
                _currentAppearance = null;


            ////_matrix = model.Matrix * entityInstance.RegionMatrix;
            //_matrix =  entityInstance.RegionMatrix;

            //// replace the position in the matrix by the camera space position since we use camera space rendering
            //_matrix.M41 = cameraSpacePosition.x;
            //_matrix.M42 = cameraSpacePosition.y;
            //_matrix.M43 = cameraSpacePosition.z;

            _matrix = matrix;

            try
            {
                if (BoundVolumeIsDirty) UpdateBoundVolume(); // TODO: i dont believe this line should be here.  Too late to do bounds checking

                // now set the state on the device for use
                _defaultStateBlock.Capture();
                _meshStateBlock.Apply();
                RenderSolid();
                RenderModelerFrame();

                // NOTE: We need seperate rollback stateblocks for each viewport
                // rollback the state by applying all the existing states we copied prior to setting up for rendering our imposters
                _defaultStateBlock.Apply();
            }
            catch
            {
                Trace.WriteLine(string.Format("Failed to render '{0}'.", _id));
            }
        }


        //private void InitializeVertexBuffer()
        //{
        //    _device.VertexDeclaration = _vertexDeclaration;

        //    // Initialise coordinates 
        //    _vertexCount = _mesh.Points.Count;
        //    _vertices = new CustomVertex.PositionOnly[_vertexCount];
        //    for (int i = 0; i < _mesh.Points.Count; i++)
        //    {
        //        Vector3d result = Vector3d.TransformCoord(Helpers.TVTypeConverter.FromTVVector(_mesh.Points[i]), Helpers.TVTypeConverter.FromTVMatrix(_matrix));
        //        _vertices[i] = new CustomVertex.PositionOnly((float)result.x, (float)result.y, (float)result.z);
        //    }         
            
        //    // normals
        //    _normals = new CustomVertex.PositionOnly[_vertexCount];
        //    for (int i = 0; i < _mesh.Points.Count; i++)
        //    {
        //        _normals[i] = new CustomVertex.PositionOnly(0, 0, 0);
        //    }
   
            
        //    // UVs
        //    _uvs = new TexCoord[_vertexCount];
        //    for (int i = 0; i < _mesh.Points.Count; i++)
        //    {
        //        _uvs[i] = new TexCoord (0,0);
        //    }
            
        //}

        private void InitializeVertexBuffer(Dictionary<string, KeyValuePair<int, CustomVertex.PositionNormalTextured>> verts)
        {


            //// Initialise vertex buffer [old].   
            //_vertexCount = _mesh.Points.Count;
            //_vertices = new CustomVertex.PositionNormalTextured[_vertexCount];
            //for (int i = 0; i < _mesh.Points.Count; i++)
            //{
            //    Vector3d result = Vector3d.TransformCoord(Helpers.TVTypeConverter.FromTVVector(_mesh.Points[i]), Helpers.TVTypeConverter.FromTVMatrix(_matrix));
            //   // _mesh.Normals[i].x, _mesh.Points[i].y, _mesh.Points[i].z
            //    _vertices[i] = new CustomVertex.PositionNormalTextured((float)result.x, (float)result.y, (float)result.z, 0, 0, 0,0,0);
            //}
            //_vertexBuffer.SetData(_vertices, 0, 0);

           
            // we need to create vertex buffer and indices suitable for rendering.  That means our vertex buffer
            // must actually contain the normal data and uv data and so we must remap the indices
            // to use these new ones

            // initialize an array large enough to hold our data
           // int maxCount = Math.Max(_mesh.Points.Count);

            for (int i = 0; i < _mesh.Faces.Count; i++)
            {
                // itterate through each vertex in the face.. should be multiples of 3 since its triangulated
                System.Diagnostics.Trace.Assert (_mesh.Faces[i].TriangulatedPoints.Length % 3 == 0);
                for (int j = 0; j < _mesh.Faces[i].TriangulatedPoints.Length; j++)
                {
                    string key = _mesh.Faces[i].TriangulatedPoints[j] + "," + _mesh.Faces[i].Normals[j] + "," + _mesh.Faces[i].Textures[j];
                    KeyValuePair <int, CustomVertex.PositionNormalTextured> kvp;
                    if (verts.TryGetValue (key, out kvp))
                        //the vert already exists and we'll just re-use this vert's index
                        //kvp.Key;
                    {}
                    else
                    {
                        // create a new vertex with that key and add it and use the new index
                        Vector3 pos = new Vector3(_mesh.Points[(int)_mesh.Faces[i].TriangulatedPoints[j]].x, _mesh.Points[(int)_mesh.Faces[i].TriangulatedPoints[j]].y, _mesh.Points[(int)_mesh.Faces[i].TriangulatedPoints[j]].z);
                        Vector3 normal = new Vector3(_mesh.Normals[(int)_mesh.Faces[i].Normals[j]].x, _mesh.Normals[(int)_mesh.Faces[i].Normals[j]].y, _mesh.Normals[(int)_mesh.Faces[i].Normals[j]].z);
                        Vector2 uv = new Vector2(_mesh.UVs[(int)_mesh.Faces[i].Textures[j]].x, _mesh.UVs[(int)_mesh.Faces[i].Textures[j]].y);
                        CustomVertex.PositionNormalTextured v = new CustomVertex.PositionNormalTextured(pos, normal, uv.X, uv.Y);
                        kvp = new KeyValuePair<int,CustomVertex.PositionNormalTextured> (verts.Count, v);
                        verts.Add (key, kvp);
                    }
                }
            }
                       
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


            //_vertexCount = verts.Count;
            //_vertices = new CustomVertex.PositionNormalTextured[_vertexCount];
            //int z = 0;
            //foreach (KeyValuePair <int, CustomVertex.PositionNormalTextured> kvp in verts.Values)
            //{

            //  //   Vector3d result = Vector3d.TransformCoord(Helpers.TVTypeConverter.FromTVVector(kvp.Value.), Helpers.TVTypeConverter.FromTVMatrix(_matrix));
            ////   // _mesh.Normals[i].x, _mesh.Points[i].y, _mesh.Points[i].z
            //    Vector4 temp = Microsoft.DirectX.Vector3.Transform(kvp.Value.Position, Helpers.TVTypeConverter.ToDirectXMatrix(_matrix));
            //    Vector3 position = new Vector3(temp.X, temp.Y, temp.Z);
            //    CustomVertex.PositionNormalTextured vertex = new CustomVertex.PositionNormalTextured( position,kvp.Value.Normal , kvp.Value.Tu , kvp.Value.Tv);
            //    _vertices[z] =vertex;
            //    z++;
            //}

            //// DrawIndexedPrimitive Demystified
            ////http://blogs.msdn.com/jsteed/articles/185681.aspx   
            //_device.VertexFormat = _FVF;
            //_vertexBuffer.SetData(_vertices, 0, 0);
            //_device.SetStreamSource(0, _vertexBuffer, 0);
        }

        private void InitializeIndexBuffer()
        {
 
        }


        private void RenderSolid()
        {
            // Initialise coordinates 
            _vertexCount = _mesh.Points.Count;
            Vector3[] vertices = new Vector3 [_vertexCount];
            Vector3[] normals = new Vector3 [_vertexCount];
            TexCoord[] uvs = new TexCoord[_vertexCount];
            
            _device.VertexDeclaration = _vertexDeclaration;

            //CoreClient._CoreClient.Maths.TVVec
            //Vector3d[] tmp = Vector3d.TransformCoordArray(_mesh.Points, _matrix);

            for (int i = 0; i < _mesh.Points.Count; i++)
            {
                Vector3 vec = new Vector3(_mesh.Points[i].x, _mesh.Points[i].y, _mesh.Points[i].z);
                Vector3 res = Vector3.TransformCoordinate(vec, Helpers.TVTypeConverter.ToDirectXMatrix(_matrix));
                vertices[i] = new Vector3(res.X, res.Y, res.Z);
            }

            // normals
            for (int i = 0; i < _mesh.Points.Count; i++)
            {
                normals[i] = new Vector3(0, 0, 0);
            }


            // UVs
            for (int i = 0; i < _mesh.Points.Count; i++)
            {
                uvs[i] = new TexCoord(0, 0);
            }

            // Now that've set the vertex buffer and the vertex format, we should itterate thru all faces, grabbing the index buffer
            // and setting that and  making a new DrawPrimitives call for each GROUP
            int indicesCount = 0;
            for (int i = 0; i < _mesh.Groups.Count; i++)
            {
                // build our indices array
                if (_mesh.Groups[i].Faces == null) continue;
                for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
                    indicesCount += _mesh.Groups[i].Faces[j].TriangulatedPoints.Length;
            }

            int offset = 0;
            uint[] indices = new uint[indicesCount];
            int[] offsets = new int[_mesh.Groups.Count];
            uint[] minimumVertexIndices = new uint[_mesh.Groups.Count];
            uint[] maximumVertexIndices = new uint[_mesh.Groups.Count];

            // if we have > 1 group in this mesh, build a seperate indices array for each group
            for (int i = 0; i < _mesh.Groups.Count; i++)
            {
                if (_mesh.Groups[i].Faces == null) continue;
                minimumVertexIndices[i] = int.MaxValue;
                maximumVertexIndices[i] = 0;
                for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
                {
                    Vector3 faceNormal = new Vector3(_mesh.Groups[i].Faces[j].GetFaceNormal().x, _mesh.Groups[i].Faces[j].GetFaceNormal().y, _mesh.Groups[i].Faces[j].GetFaceNormal().z);
                    for (int k = 0; k < _mesh.Groups[i].Faces[j].TriangulatedPoints.Length; k++)
                    {
                        // add to indices
                        uint index = _mesh.Groups[i].Faces[j].TriangulatedPoints[k];

                        // store the face normal here... this does no real good because
                        // the normals 
                       // if (offset + k < normals.Length)
                        normals[index] = faceNormal;

                        indices[offset + k] = index;
                        // track the minimum index value into the vertex buffer for this group
                        if (minimumVertexIndices[i] > index)
                            minimumVertexIndices[i] = index;
                        if (maximumVertexIndices[i] < index)
                            maximumVertexIndices[i] = index;
                    }
                    offset += _mesh.Groups[i].Faces[j].TriangulatedPoints.Length;
                }
                offsets[i] = offset; // this number tells us how many vertices are in this group
            }

            // is it faster to graphic stream + lock/unlock over SetData?  
            // according to the following url, the underlying codes are about the same
            // http://www.gamedev.net/community/forums/topic.asp?topic_id=346308
            GraphicsStream gs = _vertexBuffer.Lock(0, 0, 0);
            gs.Write(vertices);
            _vertexBuffer.Unlock();

            gs = _normalBuffer.Lock(0, 0, 0);
            gs.Write(normals);
            _normalBuffer.Unlock();

            gs = _uvBuffer.Lock(0, 0, 0);
            gs.Write(uvs);
            _uvBuffer.Unlock();

           // _vertexBuffer.SetData(_vertices, 0, 0);
           // _uvBuffer.SetData(_uvs, 0, 0);
           // _normalBuffer.SetData(_normals, 0, 0);

            _indexBuffer.SetData(indices, 0, 0);
            _device.Indices = _indexBuffer;
            _device.SetStreamSource(0, _vertexBuffer, 0);
            _device.SetStreamSource(1, _normalBuffer, 0);
            _device.SetStreamSource(2, _uvBuffer, 0);
            

            // material
            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
            Microsoft.DirectX.Direct3D.Material tmpNullMat = new Microsoft.DirectX.Direct3D.Material();

            // draw the indexed primitives
            for (int i = 0; i < offsets.Length; i++)
            {
                int nextStartIndex = offsets[i];
                int startIndex;
                if (i == 0)
                    startIndex = 0;
                else
                    startIndex = offsets[i - 1];

                int triangleCount = (nextStartIndex - startIndex) / 3;
                System.Diagnostics.Trace.Assert(triangleCount * 3 == nextStartIndex - startIndex); // verify a whole non fractional number of triangles

                uint numVertices = maximumVertexIndices[i] - minimumVertexIndices[i];
                if (numVertices == 0) continue;
                int baseVertexIndex = 0;
                // BaseVertexIndex is a value that's effectively added to every VB Index stored in the index buffer. 
                // For example, if we had passed in a value of 50 for BaseVertexIndex during the previous call, 
                // that would functionally be the same as using the following index buffer for the duration of the DrawIndexedPrimitive call: 
                // The minVertexIndex is a hint to help DX optimzie memory usage when working with the vertex buffer so if you can tell it
                // in advance the position of the vertex in the vertex buffer that will be the lowest, that will help.. it could be set to 0 though
                // This value is rarely set to anything other than 0, but can be useful if you want to decouple the index buffer from the vertex buffer: If when filling in the index buffer for a particular mesh the location of the mesh within the vertex buffer isn't yet known, you can simply pretend the mesh vertices will be located at the start of the vertex buffer; when it comes time to make the draw call, simply pass the actual starting location as the BaseVertexIndex.
                // This technique can also be used when drawing multiple instances of a mesh using a single index buffer; for example, if the vertex buffer contained two meshes with identical draw order but slightly different vertices (perhaps different diffuse colors or texture coordinates), both meshes could be drawn by using different values for BaseVertexIndex. Taking this concept one step further, you could use one index buffer to draw multiple instances of a mesh, each contained in a different vertex buffer, simply by cycling which vertex buffer is active and adjusting the BaseVertexIndex as needed. Note that the BaseVertexIndex value is also automatically added to the MinIndex argument, which makes sense when you see how it's used: 

                // modify the material
                if (_mesh.Groups[i].Material != null)
                {
                    tmpMat = Helpers.TVTypeConverter.FromWavefrontObjMaterial(_mesh.Groups[i].Material);
                    _device.Material = tmpMat;
                }
                else
                {
                    _device.Material = tmpNullMat;
                }

                // startIndex is obviously the starting index in the indices for a group
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertexIndex, (int)minimumVertexIndices[i], (int)numVertices, startIndex, triangleCount);
                // _device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList,(int)minimumVertexIndices[i], (int)numVertices, triangleCount, indices, false, _vertices); 
            }
        }

        //Dictionary<string, KeyValuePair<int, CustomVertex.PositionNormalTextured>> verts = new Dictionary<string, KeyValuePair<int, CustomVertex.PositionNormalTextured>>();
        //private void RenderSolid()
        //{
        //    verts.Clear();
        //    // TODO: would it be possible to use a fixed vertex buffer that i then tweak the vert positions of
        //    //  and then just update the index buffer for the ones i want to render?  I dont think so.
        //    // in fact its not even possible to get shading on your model when your vertices are essentially
        //    // seperated from faces and thus have no normals!  Is there a way to get "normals" generated for faces
        //    // on the fly?
        //    InitializeVertexBuffer();
        //    //InitializeVertexBuffer(verts);

        //    InitializeIndexBuffer();

        //    // Now that've set the vertex buffer and the vertex format, we should itterate thru all faces, grabbing the index buffer
        //    // and setting that and  making a new DrawPrimitives call for each GROUP
        //    int indicesCount = 0;
        //    for (int i = 0; i < _mesh.Groups.Count; i++)
        //    {
        //        // build our indices array
        //        if (_mesh.Groups[i].Faces == null) continue;
        //        for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
        //            indicesCount += _mesh.Groups[i].Faces[j].TriangulatedPoints.Length;
        //    }

        //    int offset = 0;
        //    uint[] indices = new uint[indicesCount];
        //    int[] offsets = new int[_mesh.Groups.Count];
        //    uint[] minimumVertexIndices = new uint[_mesh.Groups.Count];
        //    uint[] maximumVertexIndices = new uint[_mesh.Groups.Count];

        //    // if we have > 1 group in this mesh, build a seperate indices array for each group
        //    for (int i = 0; i < _mesh.Groups.Count; i++)
        //    {
        //        if (_mesh.Groups[i].Faces == null) continue;
        //        minimumVertexIndices[i] = int.MaxValue;
        //        maximumVertexIndices[i] = 0;
        //        for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
        //        {
        //            CustomVertex.PositionOnly faceNormal = new CustomVertex.PositionOnly(_mesh.Groups[i].Faces[j].GetFaceNormal().x, _mesh.Groups[i].Faces[j].GetFaceNormal().y, _mesh.Groups[i].Faces[j].GetFaceNormal().z);
        //            for (int k = 0; k < _mesh.Groups[i].Faces[j].TriangulatedPoints.Length; k++)
        //            {
        //                // add to indices
        //                string key = _mesh.Groups[i].Faces[j].TriangulatedPoints[k] + "," + _mesh.Groups[i].Faces[j].Normals[k] + "," + _mesh.Groups[i].Faces[j].Textures[k];
                        
        //                // 
        //                //[OBSOLETE] 
        //                uint index = _mesh.Groups[i].Faces[j].TriangulatedPoints[k];

        //                // store the face normal here 
        //                if(offset + k < _normals.Length )
        //                     _normals[offset + k] = faceNormal;

        //                // remap the index to an actual d3d vertexbuffer vertex
        //                //KeyValuePair <int, CustomVertex.PositionNormalTextured> kvp;
        //                //if (!verts.TryGetValue (key, out kvp))
        //                //{
        //                //    throw new Exception ("invalid key.  this should not be possible.  must be a bug");
        //                //}
                        
        //                //uint index = (uint)kvp.Key;
        //                indices[offset + k] = index;
        //                // track the minimum index value into the vertex buffer for this group
        //                if (minimumVertexIndices[i] > index)
        //                    minimumVertexIndices[i] = index;
        //                if (maximumVertexIndices[i] < index)
        //                    maximumVertexIndices[i] = index;
        //            }
        //            offset += _mesh.Groups[i].Faces[j].TriangulatedPoints.Length;
        //        }
        //        offsets[i] = offset; // this number tells us how many vertices are in this group
        //    }

        //    _vertexBuffer.SetData(_vertices, 0, 0);
        //    _uvBuffer.SetData(_uvs, 0, 0);
        //    _normalBuffer.SetData(_normals, 0, 0);
        //    _device.SetStreamSource(0, _vertexBuffer, 0);
        //    _device.SetStreamSource(1, _normalBuffer, 0);
        //    _device.SetStreamSource(2, _uvBuffer, 0);
        //    _indexBuffer.SetData(indices, 0, 0);
        //    _device.Indices = _indexBuffer;

        //    // draw the indexed primitives
        //    for (int i = 0; i < offsets.Length; i++)
        //    {
        //        int nextStartIndex = offsets[i];
        //        int startIndex;
        //        if (i == 0)
        //            startIndex = 0;
        //        else
        //            startIndex = offsets[i - 1];

        //        int triangleCount = (nextStartIndex - startIndex) / 3;
        //        System.Diagnostics.Trace.Assert(triangleCount * 3 == nextStartIndex - startIndex); // verify a whole non fractional number of triangles

        //        uint numVertices = maximumVertexIndices[i] - minimumVertexIndices[i];
        //        if (numVertices == 0) return;
        //        int baseVertexIndex = 0;
        //        // BaseVertexIndex is a value that's effectively added to every VB Index stored in the index buffer. 
        //        // For example, if we had passed in a value of 50 for BaseVertexIndex during the previous call, 
        //        // that would functionally be the same as using the following index buffer for the duration of the DrawIndexedPrimitive call: 
        //        // The minVertexIndex is a hint to help DX optimzie memory usage when working with the vertex buffer so if you can tell it
        //        // in advance the position of the vertex in the vertex buffer that will be the lowest, that will help.. it could be set to 0 though
        //        // This value is rarely set to anything other than 0, but can be useful if you want to decouple the index buffer from the vertex buffer: If when filling in the index buffer for a particular mesh the location of the mesh within the vertex buffer isn't yet known, you can simply pretend the mesh vertices will be located at the start of the vertex buffer; when it comes time to make the draw call, simply pass the actual starting location as the BaseVertexIndex.
        //        // This technique can also be used when drawing multiple instances of a mesh using a single index buffer; for example, if the vertex buffer contained two meshes with identical draw order but slightly different vertices (perhaps different diffuse colors or texture coordinates), both meshes could be drawn by using different values for BaseVertexIndex. Taking this concept one step further, you could use one index buffer to draw multiple instances of a mesh, each contained in a different vertex buffer, simply by cycling which vertex buffer is active and adjusting the BaseVertexIndex as needed. Note that the BaseVertexIndex value is also automatically added to the MinIndex argument, which makes sense when you see how it's used: 
        //        numVertices = (uint)_vertexCount;
        //        minimumVertexIndices[i] = 0;
        //        // startIndex is obviously the starting index in the indices for a group
        //        _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertexIndex, (int)minimumVertexIndices[i], (int)numVertices, startIndex, triangleCount);
        //        // _device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList,(int)minimumVertexIndices[i], (int)numVertices, triangleCount, indices, false, _vertices); 
        //    }
        //}

        private void RenderModelerFrame()
        {

            _device.SetRenderState(RenderStates.ZBufferWriteEnable, false);
            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
            //tmpMat.Diffuse = System.Drawing.Color.Lavender;
            tmpMat.Ambient = System.Drawing.Color.Black;
            //// tmpMat.Specular = System.Drawing.Color.LightGray;
            //// tmpMat.SpecularSharpness = 15.0F;
            _device.Material = tmpMat;

            // we need a new vertex buffer that has a Position and Colored type and which as a result has a different stride
            // TODO: would it be possible to use a fixed vertex buffer that i then tweak the vert positions of
            //  and then just update the index buffer for the ones i want to render?
            // Initialise vertex buffer.   
            _vertexCount = _mesh.Points.Count;
            CustomVertex.PositionColored[] wireFrameVertices = new CustomVertex.PositionColored[_vertexCount];
            for (int i = 0; i < _mesh.Points.Count; i++)
            {
                Vector3 vec = new Vector3(_mesh.Points[i].x, _mesh.Points[i].y, _mesh.Points[i].z);
                Vector3 res = Vector3.TransformCoordinate(vec, Helpers.TVTypeConverter.ToDirectXMatrix(_matrix));
                wireFrameVertices[i] = new CustomVertex.PositionColored(res.X, res.Y, res.Z, 0);
            }

            _wireFrameVertexBuffer.SetData(wireFrameVertices, 0, 0);
            _device.SetStreamSource(0, _wireFrameVertexBuffer, 0);
            _device.VertexFormat = _wireFrameFVF;

            // i believe that lines need to have references to the faces they adjoin.  I see no way around this for efficiency and
            // for future ability to modify these models in realtime.  The question though is, since WaveFrontObjMesh doesnt really
            // need this for loading, its really our internal modeling format we're talking about that needs to know this stuff
            // I believe that's something that should occur after we've loaded and before we would typically convert to TVMesh.
            // So this MeshWaveFrontObj  actually could be that object...

            // we need to create new offsets and 
            // indices list using the regular Points and not the TriangulatedPoints
            // Now that've set the vertex buffer and the vertex format into the device, we should itterate thru all faces, grabbing the index buffer
            // and setting that and  making a new DrawPrimitives call for each GROUP
            int indicesCount = 0;
            for (int i = 0; i < _mesh.Groups.Count; i++)
            {
                // build our indices array
                if (_mesh.Groups[i].Faces == null) continue;
                for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
                    indicesCount += _mesh.Groups[i].Faces[j].Points.Length * 2;
            }

            int offset = 0;
            uint[] indices = new uint[indicesCount];
            int[] offsets = new int[_mesh.Groups.Count];
            uint[] minimumVertexIndices = new uint[_mesh.Groups.Count];
            uint[] maximumVertexIndices = new uint[_mesh.Groups.Count];
            // build the indices array
            for (int i = 0; i < _mesh.Groups.Count; i++)
            {
                if (_mesh.Groups[i].Faces == null) continue;
                minimumVertexIndices[i] = int.MaxValue;
                maximumVertexIndices[i] = 0;
                for (int j = 0; j < _mesh.Groups[i].Faces.Count; j++)
                {
                    for (int k = 0; k < _mesh.Groups[i].Faces[j].Points.Length; k++)
                    {
                        // since we're not dealing with triangulated indexed points, we 
                        // must manually create indexed line list here on the fly by adding two points
                        // every itteration.  We do this because LineList allows us to use a single drawprimitive call
                        // rather than use LineStrip which will require fewer verts but more calls as we have to break between faces
                        // to avoid LineStrip's making unwanted diagonal connections between end of one face and start of next
                        if (k == _mesh.Groups[i].Faces[j].Points.Length - 1)
                        {
                            // connect the face back to the beginning
                            indices[offset + k * 2 ] = _mesh.Groups[i].Faces[j].Points[k];
                            indices[offset + k * 2 + 1] = _mesh.Groups[i].Faces[j].Points[0];
                        }
                        else
                        {
                            indices[offset + k * 2] = _mesh.Groups[i].Faces[j].Points[k];
                            indices[offset + k * 2 + 1] = _mesh.Groups[i].Faces[j].Points[k + 1];
                        }
                        // track the minimum index value into the vertex buffer for this group
                        if (minimumVertexIndices[i] > _mesh.Groups[i].Faces[j].Points[k])
                            minimumVertexIndices[i] = _mesh.Groups[i].Faces[j].Points[k];
                        if (maximumVertexIndices[i] < _mesh.Groups[i].Faces[j].Points[k])
                            maximumVertexIndices[i] = _mesh.Groups[i].Faces[j].Points[k];
                    }
                    offset += _mesh.Groups[i].Faces[j].Points.Length * 2;
                }
                offsets[i] = offset; // this number tells us how many vertices are in this group
            }

            _wireFrameIndexBuffer.SetData(indices, 0, 0);
            _device.Indices = _wireFrameIndexBuffer;

            // draw the user created edges
            for (int i = 0; i < offsets.Length; i++)
            {
                int nextStartIndex = offsets[i];
                int startIndex;
                if (i == 0)
                    startIndex = 0;
                else
                    startIndex = offsets[i - 1];

                int lineCount = (nextStartIndex - startIndex) / 2;
                System.Diagnostics.Trace.Assert(lineCount * 2 == nextStartIndex - startIndex); // verify a whole non fractional number of lines

                uint numVertices = maximumVertexIndices[i] - minimumVertexIndices[i];
                if (numVertices == 0) continue;
                int baseVertexIndex = 0;
                // BaseVertexIndex is a value that's effectively added to every VB Index stored in the index buffer. 
                // For example, if we had passed in a value of 50 for BaseVertexIndex during the previous call, 
                // that would functionally be the same as using the following index buffer for the duration of the DrawIndexedPrimitive call: 
                // The minVertexIndex is a hint to help DX optimzie memory usage when working with the vertex buffer so if you can tell it
                // in advance the position of the vertex in the vertex buffer that will be the lowest, that will help.. it could be set to 0 though
                // This value is rarely set to anything other than 0, but can be useful if you want to decouple the index buffer from the vertex buffer: If when filling in the index buffer for a particular mesh the location of the mesh within the vertex buffer isn't yet known, you can simply pretend the mesh vertices will be located at the start of the vertex buffer; when it comes time to make the draw call, simply pass the actual starting location as the BaseVertexIndex.
                // This technique can also be used when drawing multiple instances of a mesh using a single index buffer; for example, if the vertex buffer contained two meshes with identical draw order but slightly different vertices (perhaps different diffuse colors or texture coordinates), both meshes could be drawn by using different values for BaseVertexIndex. Taking this concept one step further, you could use one index buffer to draw multiple instances of a mesh, each contained in a different vertex buffer, simply by cycling which vertex buffer is active and adjusting the BaseVertexIndex as needed. Note that the BaseVertexIndex value is also automatically added to the MinIndex argument, which makes sense when you see how it's used: 

                // startIndex is obviously the starting index in the indices for a group
                // one question is though, when rendering we need to exclude those lines (if solid mode is enabled) for faces that are not visible
                // maybe we can ignore for now
                _device.DrawIndexedPrimitives(PrimitiveType.LineList, baseVertexIndex, (int)minimumVertexIndices[i], (int)numVertices, startIndex, lineCount);
            }
        }

        private void DrawVertexTabs()
        {

            // the color at a particular time depends on the selected status of the vert, or edge or face it's on



            // draw the user dragable points
          //  _device.DrawIndexedPrimitives (PrimitiveType.PointList, );
 
        }

        #endregion

        #region IBoundVolume Members
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            if (!TVResourceIsLoaded) return;

            float radius = 0f;
            TV_3DVECTOR min, max, center;
            min = new TV_3DVECTOR(0, 0, 0);
            max = new TV_3DVECTOR(0, 0, 0);
            center = new TV_3DVECTOR(0, 0, 0);
            
            // model space bounding box
            for (int i = 0; i < _mesh.Points.Count; i ++)
            {
                if (_mesh.Points[i].x < min.x) min.x = _mesh.Points[i].x;
                if (_mesh.Points[i].x > max.x) max.x = _mesh.Points[i].x;
                if (_mesh.Points[i].y < min.y) min.y = _mesh.Points[i].y;
                if (_mesh.Points[i].y > max.y) max.y = _mesh.Points[i].y;
                if (_mesh.Points[i].z < min.z) min.z = _mesh.Points[i].z;
                if (_mesh.Points[i].z > max.z) max.z = _mesh.Points[i].z;  
            }

            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
            _sphere = new BoundingSphere(_box);
            DisableChangeFlags(Keystone.Enums.ChangeStates.Translated | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

#region TVMesh members
        //public new int GetIndex()
        //{
        //    throw new NotImplementedException();
        //}

        //public new void AttachTo(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex, bool bKeepMatrix, bool bRemoveScale){}

        //public new int GetVertexCount()
        //{
        //    return 0;
        //    return _wavefrontObj.Points.Count;
        //}

        //public new int GetGroupCount()
        //{
        //    return 0; 
        //    throw new NotImplementedException();
        //}

        //public new int GetTriangleCount()
        //{
        //    return 0;
        //    throw new NotImplementedException();
        //}

        //public new TV_3DMATRIX GetMatrix() { throw new NotImplementedException(); }
        //public new void SetMatrix(TV_3DMATRIX mMatrix){}

        //public new void SetPrimitiveType(CONST_TV_PRIMITIVETYPE ePrimitiveType){}

        //public new void SetMeshFormat(int combinedFormat)
        //{ 
        //}
        //public new bool SetShader(TVShader sShader) { throw new NotImplementedException(); }

        //public new void SetCullMode(CONST_TV_CULLING cullmode) { }

        /// <summary>
        /// NOTE: SetLightingMode must be called after geometry is loaded because the lightingmode
        /// is stored in .tvm, .tva files and so anything you set prior to loading, will be
        /// replaced with the lightingmode stored in the file.
        /// </summary>
        //public new void SetLightingMode(CONST_TV_LIGHTINGMODE eLightingMode){}

        //public new void SetBlendingMode(CONST_TV_BLENDINGMODE blendingMode) { }

        //public new void SetOverlay(bool enable) { }

        //public new void SetAlphaTest(bool enable) { }

        //public new void SetAlphaTest(bool enable, int refTestValue, bool depthBufferWriteEnable) {}
        //public new void SetShadowCast(bool bEnable, bool bUseAdditiveShadows){}
        //public new void EnableFrustumCulling(bool bEnable){}

        //public new int GetMaterial(int Group) { throw new NotImplementedException(); }
        //public new void SetMaterial(int iMaterial) {}
        //public new void SetMaterial(int iMaterial, int iGroup){}
        //public new int GetTextureEx(int iLayer, int iGroup) { throw new NotImplementedException(); }
        //public new void SetTexture(int iTexture) {}
        //public new void SetTexture(int iTexture, int iGroup){}
        //public new void SetTextureEx(int iLayer, int iTexture, int iGroup){}

        //public new void SetPosition(float x, float y, float z)
        //{
        //}

        //public new bool IsVisible() { throw new NotImplementedException(); }
        //public new void SetCollisionEnable(bool bEnableCollision){}
        //public new bool AdvancedCollision(TV_3DVECTOR pStart, TV_3DVECTOR pEnd, ref TV_COLLISIONRESULT retResult, CONST_TV_TESTTYPE eTestType) 
        //{
        //    return false;
        //   //throw new NotImplementedException(); 
        //}

        //public new void GetBoundingSphere(ref TV_3DVECTOR center, float radius, bool localSpace)
        //{ }

        //public new void GetBoundingBox(ref TV_3DVECTOR min, ref TV_3DVECTOR max, bool localSpace)
        //{

        //}
        //public new void ForceMatrixUpdate()
        //{}
        //public new void ComputeNormals() {}


        //public new void CreateBox(int width, int height, int depth) { }
        //public new void CreateSphere (float radius, int slices, int stacks) {}
        //public new int AddFloorGrid(int iTexture, float fX1, float fZ1, float fX2, float fZ2, int iNumCellsX, int iNumCellsZ, float fAltitude, float fTileU, float fTileV) { throw new NotImplementedException(); }


        //public new int AddVertex(float x, float y, float z, float nx, float ny, float nz, float tu1, float tv1) { throw new NotImplementedException(); }


        //public new void Render()
        //{
        
        //}

        //public new void SaveTVM(string filePath) { }
        //public new void Destroy() { }
#endregion

    }
}
