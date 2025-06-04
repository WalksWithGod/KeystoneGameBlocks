//using System;
//using System.Collections.Generic;
//using MTV3D65;
//using Keystone.Interfaces;
//using Keystone.Loaders;
//using Microsoft.DirectX;
//using Microsoft.DirectX.Direct3D;
//using Keystone.Traversers;
//using Keystone.Types;
//using Keystone.Shaders;
//using Keystone.Entities;
//using Keystone.Elements;
//using Keystone.Enum;
//using System.Diagnostics;
//using Keystone.Appearance;
//using Keystone.Resource;

//namespace Keystone.Modeler
//{
//    // our WaveFrontObj _mesh private var will contain bare bones data
//    // but it's this class that will implement the routines against that data for computing bounding boxes and such.
//    // we will _NOT_ need some new class to simulate TVMesh to do that.  THis _IS_ that new class
//    public class MeshEditable : Elements.Geometry, INotifyDeviceReset
//    {
//        // TODO: adjacency?          //    // i believe that lines need to have references to the faces they adjoin.  I see no way around this for efficiency and
//        // for future ability to modify these models in realtime.  The question though is, since WaveFrontObjMesh doesnt really
//        // need this for loading, its really our internal modeling format we're talking about that needs to know this stuff
//        // I believe that's something that should occur after we've loaded and before we would typically convert to TVMesh.
//        // So this MeshWaveFrontObj  actually could be that object..

//        private Appearance.Appearance _currentAppearance;
//        public List<VirtualGroup> Groups;
//        private List<IndexedFace> Faces;
//        private TV_3DMATRIX _matrix;

//        // now our global list of vertices that we build when we start creating faces
//        // should be that we add a new IndexedVertex in a dictionary such as this
//        Dictionary<uint, List<IndexedVertex>> _virtualVertices; // all vertices at a specific Point index, will be shared here
//        internal List<CustomVertex.PositionNormalTextured> _vertices = new List<CustomVertex.PositionNormalTextured>();

//        // as for virtual faces, i believe there is no such thing.  All that's really important is that each IndexedVertex be able
//        // to point to all other IndexedVertex's that share it's coord which is simply by  IndexedVertices[] shared =_virtualVertices[this.Index].ToArray()
//        // thus when we "move" any shared vertice, we are moving all of them.

//        // but there's one other concern, when it comes time to create an new VertexBuffer and IndexBuffer for solid rendering, we need to be able to
//        // create these based on the construction of a lookup based on the seperate coord/normal/uv array
//        // I think these should be done at the same time as we add IndexedVertex to our faces such that
//        //
//        //

//        //    //// DrawIndexedPrimitive Demystified
//        //    ////http://blogs.msdn.com/jsteed/articles/185681.aspx   
//        CustomVertex.PositionNormalTextured[] _transformedVertices;
        

//        private int _vertexCount;
//        private int _triangleCount;
        

//        private Line _lineDrawer;

//        private static int COUNTER;
//        public StateBlock _meshStateBlock;
//        public StateBlock _defaultStateBlock;

//        public MeshEditable(string id)
//            : base(id)
//        {

//            CoreClient._CoreClient.RegisterForDeviceResetNotification(this);
//            CreateStateBlocks(CoreClient._CoreClient.D3DDevice);
//        }

//        public static MeshEditable Create(string id, string resourcePath, bool loadTextures, bool loadMaterials)
//        {
//            MeshEditable mesh = (MeshEditable)Repository.Get(id);
//            if (mesh == null)
//            {
//                WaveFrontObj wavefrontObj = CreateMesh(resourcePath);
//                mesh = new MeshEditable(id);

//                Initialize(mesh, wavefrontObj);

                
//                mesh._resourcePath = resourcePath;
//                mesh._tvfactoryIndex = int.MaxValue - COUNTER; //TODO: this is a temp hack
//                COUNTER++;

//                // here we're going to populate the internal fields required for realtime modification of the mesh
//                // so edge lists and adjacency lists, etc
//            }
//            else
//            {
//                Trace.WriteLine("WaveFrontMesh " + resourcePath + " already found in cache... sharing mesh instance.");
//            }

//            if ((loadMaterials) || (loadTextures))
//            {
//                // TODO: actually t his needs to be done in ParseFile since its handled differently for wavefront
//                //ImportLib.GetMeshTexturesAndMaterials(mesh._mesh, resourcePath, ref appearance);
//            }
//            return mesh;
//        }

//        private static WaveFrontObj CreateMesh(string resourcePath)
//        {
//            Stopwatch watch = new Stopwatch();
//            watch.Reset();
//            watch.Start();

//            WaveFrontObj m = WavefrontObjLoader.ParseFile(resourcePath, true);
//            watch.Stop();

//            if (m != null)
//                Trace.WriteLine("Mesh loaded with " + m.Groups.Count + " groups." + watch.Elapsed + "seconds, with = " + m.Points.Count + " vertices in " + m.Faces.Count + " triangles. " + resourcePath);

//            return m;
//        }
        
//        private static void Initialize(MeshEditable mesh, WaveFrontObj wavefrontObj)
//        {
//            mesh._virtualVertices = new Dictionary<uint,List<IndexedVertex>> ();
//            mesh._vertices = new List<CustomVertex.PositionNormalTextured> ();

//            // we dont really need to retain normals, uvs and coords as seperate indices. We only need those to generate our _virtualVertices
//            // and _vertices.  We do need groups and we also need to triangulated faces prior to adding them....
//            for (int m = 0; m < wavefrontObj.Groups.Count; m++)
//            {
//                WaveFrontObjGroup group = wavefrontObj.Groups[m];
//                VirtualGroup currentGroup = new VirtualGroup(group._name);
//                if (group.Material != null)
//                    currentGroup.Material = Helpers.TVTypeConverter.FromWavefrontObjMaterial(group.Material);

//                // to create the virtual faces and the unique vertices for each unique combination of normal and uv itterate thru the faces
//                if (group.Faces == null) continue;
//                for (int i = 0; i <group.Faces.Count; i++)
//                {
//                    IndexedFace currentFace = new IndexedFace();
//                    for (int j = 0; j < group.Faces[i].Points.Length; j++)
//                    {
//                        IndexedVertex currentIndexedVertex = null;

//                        int coord = (int)group.Faces[i].Points[j];
//                        int currentNormalIndex = -1;
//                        if (group.Faces[i].Normals != null)
//                            currentNormalIndex = (int)group.Faces[i].Normals[j];
//                        int currentUV = -1;
//                        if (group.Faces[i].Textures != null)
//                            currentUV = (int)group.Faces[i].Textures[j];

//                        // while reading in the faces
//                        // see if this coord/normal/texture already exists in our _virtualVertices
//                        List<IndexedVertex> tmpIndexedVertices;
//                        if (!mesh._virtualVertices.TryGetValue((uint)coord, out tmpIndexedVertices))
//                        {
//                            // doesnt exist in our _virtualVertices, add it here as well as an actual customvertex to to our _vertices array
//                            currentIndexedVertex = AddNewVertex(mesh, wavefrontObj, coord, currentNormalIndex, currentUV);
//                            tmpIndexedVertices = new List<IndexedVertex>() { currentIndexedVertex };
//                            mesh._virtualVertices.Add((uint)coord, tmpIndexedVertices);
//                        }
//                        else
//                        {
//                            int found = -1;
//                            // the coord already exists, but lets see if an exact match considering normal and uv exists
//                            for (int k = 0; k < tmpIndexedVertices.Count; k++)
//                            {
//                                if (tmpIndexedVertices[k].Normal == currentNormalIndex)
//                                    if (tmpIndexedVertices[k].UV == currentUV)
//                                    {
//                                        found = k;
//                                        break;
//                                    }
//                            }

//                            if (found > -1)
//                            {
//                                // an exact match exists, that means it aleady is added to _vertices. 
//                                // So all we need to do is set this new instance's  Index equal to the existing ones index 
//                                // and then we just add it again to the _virtualVertex dictionary at that coord key location
//                                currentIndexedVertex = new IndexedVertex(tmpIndexedVertices[found].Index);
//                                currentIndexedVertex.Coord = tmpIndexedVertices[found].Coord;
//                                currentIndexedVertex.UV = tmpIndexedVertices[found].UV;
//                                currentIndexedVertex.Normal = tmpIndexedVertices[found].Normal;

//                                tmpIndexedVertices.Add(currentIndexedVertex);
//                            }
//                            else // coord exists, but this is a new vertex still since normals and uvs are different
//                            {
//                                currentIndexedVertex = AddNewVertex(mesh, wavefrontObj, coord, currentNormalIndex, currentUV);
//                                // add to the existing key location in _virtualVertices
//                                tmpIndexedVertices.Add(currentIndexedVertex);
//                            }
//                        }

//                        // now add the index to our IndexedFace
//                        currentFace.Add(currentIndexedVertex);
//                    }

//                    if (mesh.Faces == null) mesh.Faces = new List<IndexedFace>();
//                    currentFace.Triangulate();
//                    currentGroup.AddFace(currentFace);
//                    mesh.Faces.Add(currentFace);
//                }

//                if (mesh.Groups == null) mesh.Groups = new List<VirtualGroup>();
//                mesh.Groups.Add(currentGroup);
//            }           
//        }

//        private static IndexedVertex AddNewVertex (MeshEditable mesh, WaveFrontObj wavefrontObj, int coord, int currentNormalIndex, int currentUV)
//        {
//            IndexedVertex currentIndexedVertex = new IndexedVertex (mesh._vertices.Count);  // note this index is .Count and not coord;
//            currentIndexedVertex.Coord = coord;
//            currentIndexedVertex.Normal = currentNormalIndex;
//            currentIndexedVertex.UV = currentUV;

//            CustomVertex.PositionNormalTextured customVertex = new CustomVertex.PositionNormalTextured ();
//            customVertex.X = wavefrontObj.Points [coord].x ;
//            customVertex.Y = wavefrontObj.Points [coord].y ;
//            customVertex.Z = wavefrontObj.Points [coord].z ;
//            if (currentNormalIndex > -1)
//            {
//                customVertex.Nx = wavefrontObj.Normals [currentNormalIndex].x ;
//                customVertex.Ny = wavefrontObj.Normals [currentNormalIndex].y ;
//                customVertex.Nz = wavefrontObj.Normals [currentNormalIndex].z ;
//            }
//            if (currentUV >-1)
//            {
//                customVertex.Tu  = wavefrontObj.UVs [currentUV].x ;
//                customVertex.Tv = wavefrontObj.UVs [currentUV].y ;
//            }
//            mesh._vertices.Add(customVertex);

//            return currentIndexedVertex;
//        }

//        ~MeshEditable()
//        {
//            CoreClient._CoreClient.UnregisterForDeviceResetNotification(this);
//            ReleaseStateBlocks();
//        }

        


//        #region IPageableNode Members
//        public override void LoadTVResource()
//        {
//            //// TODO: if the mesh's src file has changed, we should unload the previous mesh first
//            //if (_mesh != null)
//            //{
//            //    try
//            //    {
//            //        //_mesh.Destroy();
//            //    }
//            //    catch
//            //    {
//            //        Trace.WriteLine("error on Mesh.Destroy() - mesh path == " + _resourcePath);
//            //    }
//            //}

//            ////// TODO: if _resourcePath == "" and _primitive = box, sphere, teapot, etc, we can load via tv's built in primitives
//            //_mesh = CreateMesh(_resourcePath);
//            //_mesh.SetMeshFormat((int)_meshformat);
//            //_mesh.SetCullMode(_cullMode);
//            //_mesh.SetBlendingMode(_blendingMode);
//            //_mesh.SetOverlay(_overlay);
//            //_mesh.SetAlphaTest(_alphaTestEnable);
//            //if (_alphaTestEnable)
//            //    _mesh.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);

//            _tvfactoryIndex = int.MaxValue - COUNTER; //TODO: this is a temp hack
//            COUNTER++;

//            // if no existing hull, we fallback to a box or sphere.
//            // TODO: actually, th ehull should be created if the xml property says "use Hull" and
//            // there is no hullPath or if the hullPath is invalid.  
//            // so we need attributes for collisionHull type and be "sphere", "box", "hull"and perhaps "geometry" for full blown model which i suspect we'll never want to use
//            //Hull = ConvexHull.ReadXml(xmlNode); // not implemented but i think this was for if the hull data was embedded in the xml
//            // ConvexHull tmpHull = ConvexHull.GetStanHull(m); 


//            // TODO: how do we load the convexhull if this LoadTVResource() doesnt get called from the server? because
//            // either mesh3d itself isnt loaded or because we skip the method call on the server
//            // and if mesh3d is not even laoded, then this ConvexHull property doesnt exist so where does it sit?
//            // TODO: also i need to go back and look at how jiglibx handles convex hulls and whether they do in fact 
//            // re-compute world hull every itteration..  In fact
//            // our jiglibx PhysicsBody property could be something we use instead of convexhull property here..?
//            // UPDATE: jiglibx uses Primitive object which
//            //  and inside of CollisionSkin object it maintains lists of local space , world space and the previous frame's world space collision skins
//            // it uses lists because you can have multiple collision skins to make up a single object such as three spheres to represent a football

//        }

//        public override void SaveTVResource(string resourcePath)
//        {
//            _resourcePath = resourcePath;
//            //if (TVResourceIsLoaded)
//            //    _mesh.SaveTVM(resourcePath); // this is why name i think should not be equal to filepath right?
//        }
//        #endregion

//        #region ResourceBase members
//        protected override void DisposeUnmanagedResources()
//        {
//        }

//        protected override void DisposeManagedResources()
//        {
//            base.DisposeManagedResources();
//            try
//            {
//                //_mesh.Destroy();
//            }
//            catch
//            {
//            }
//        }
//        #endregion

//        //public void AddChild(Mesh3d m, int groupID)
//        //{
//        //    m.AttachTo(CONST_TV_NODETYPE.TV_NODETYPE_MESH, TVIndex, groupID, false, false);
//        //    base.AddChild(m);
//        //}


//        public override object Traverse(ITraverser target, object data)
//        {
//            return target.Apply(this, data);
//        }


//        #region INotifyDeviceReset Members
//        public void OnBeforeReset()
//        {
//            ReleaseStateBlocks();
//        }

//        public void OnAfterReset()
//        {
//            CreateStateBlocks(CoreClient._CoreClient.D3DDevice);
//        }
//        #endregion


//        // called and is required to be called when the device is reset
//        public void ReleaseStateBlocks()
//        {
//            if (_meshStateBlock != null)
//            {
//                _meshStateBlock.Dispose();
//                _meshStateBlock = null;
//            }
//            if (_defaultStateBlock != null)
//            {
//                _defaultStateBlock.Dispose();
//                _defaultStateBlock = null;
//            }

//            if ( _lineDrawer != null)_lineDrawer.Dispose();
//            _lineDrawer = null;
//        }

//        // TODO: The stateblocks and the vertex buffer and index buffer objects should be created by the Traverser
//        // and then used by those meshes when we need them.  We certainly dont want multiple vertex and index buffers since they are dynamic
//        // and we should just using "SetData()" on the same one anyway
//        // Create our imposter stateblock and the default rollback block 
//        public void CreateStateBlocks(Device device)
//        {
//            // Records a stateblock for use with our imposters.  The actual device remains unchanged.
//            device.BeginStateBlock();
//            SetupStateBlock(device);
//            _meshStateBlock = device.EndStateBlock();

//            // to setup the default stateblock, you "record" the states you wish to capture within the Begin\End statements.
//            // Then, when we call "Capture" on it will grab the current state on the device for just those states!  This way we can
//            // easily rollback
//            device.BeginStateBlock();
//            SetupStateBlock(device);
//            _defaultStateBlock = device.EndStateBlock();

//            _lineDrawer = new Line(device);
//            _lineDrawer.Antialias = true;
//            _lineDrawer.GlLines = true;
//            //_lineDrawer.Width = 3.0f;


//        }

//        // TODO: i should ecanpsulate this and the above CreateSTateBlocks into a class that makes using states as simiple
//        // as materials
//        // create a state block of all the states we will need.  
//        //  NOTE: This does not set them to the device, it just creates it
//        private void SetupStateBlock(Device device)
//        {
//            // TODO: maybe i can avoid having to transform the coords myself  by simply setting the World transform to our own mesh's matrix
//            // Set world-tranform to identity (imposter billboard's vertices are already in world-space).
//            device.SetTransform(TransformType.World, Microsoft.DirectX.Matrix.Identity);
//            // device.Transform.

//            device.SetRenderState(RenderStates.ZBufferWriteEnable, true);
//            device.RenderState.DitherEnable = true;
//            device.RenderState.FillMode = FillMode.Solid;
//            device.RenderState.CullMode = Cull.CounterClockwise;

//            // TODO: these materials and textures should be tracked so they can be properly disposed afterwards...
//            //  device.SetTexture(0, Helpers.TVTypeConverter.ToD3DMaterial( _currentAppearance.Groups[i].Layers[0]));
//            //   device.Material = Helpers.TVTypeConverter.ToD3DMaterial(_currentAppearance.Groups[i].Material);
//            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
//            //tmpMat = Helpers.TVTypeConverter.FromKeystoneMaterial(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.red)); 
//            tmpMat.Diffuse = System.Drawing.Color.Lavender;
//            tmpMat.Ambient = System.Drawing.Color.GreenYellow;
//            tmpMat.Specular = System.Drawing.Color.LightGray;
//            tmpMat.SpecularSharpness = 15.0F;

//            device.Material = tmpMat;
//            device.RenderState.Ambient = System.Drawing.Color.White;
//            device.RenderState.SpecularEnable = true;
//            device.SetRenderState(RenderStates.DiffuseMaterialSource, true);
//            // device.SetRenderState(RenderStates.AmbientMaterialSource, true);
//            // device.SetRenderState(RenderStates.SpecularMaterialSource, true);
//            //device.SetRenderState(RenderStates.EmissiveMaterialSource, true);

//            device.SetRenderState(RenderStates.Lighting, true);
//            device.Lights[0].Enabled = true;
//            device.SetRenderState(RenderStates.ShadeMode, (int)ShadeMode.Gouraud);

//            //device.SetRenderState(RenderStates.FogEnable, enableFog);
//            //device.SetRenderState(RenderStates.FogTableMode , D3DFOG_EXP2);
//            //device.SetRenderState(RenderStates.FogColor , fogColor);
//            //device.SetRenderState(RenderStates.FogDensity, FloatToDWORD(fogDensity));
//            // <-- TODO: must change depending on wireframe or solid
//            //device.SetRenderState(RenderStates.AlphaBlendEnable, false);
//            //device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
//            //device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
//            //device.SetRenderState(RenderStates.AlphaTestEnable, true);
//            //device.SetRenderState(RenderStates.ReferenceAlpha, 0);
//            //evice.SetRenderState(RenderStates.AlphaFunction, (int)Compare.Greater);

//            //device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Modulate);
//            //device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.Diffuse);
//            //device.SetTextureStageState(0, TextureStageStates.ColorArgument2, (int)TextureArgument.TextureColor);
//            //device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.Modulate);
//            //device.SetTextureStageState(0, TextureStageStates.AlphaArgument1, (int)TextureArgument.Diffuse);
//            //device.SetTextureStageState(0, TextureStageStates.AlphaArgument2, (int)TextureArgument.TextureColor);
//            //    device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
//            //    device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
//            //    device.SetSamplerState(0, SamplerStageStates.AddressU, (int)TextureAddress.Clamp);
//            //    device.SetSamplerState(0, SamplerStageStates.AddressV, (int)TextureAddress.Clamp);
//        }

//        #region Geometry Member
//        public override TV_3DMATRIX Matrix
//        {
//            set
//            {
//                _matrix = value;
//                //_mesh.SetMatrix(value);

//                // NOTE: Geometry does not result in dirty bound volume if matrix is set!

//                // the only time the boundingVolume changes is if verts are added/removed
//                // or the Origin inside the model is moved or some other thing that effects the local space
//                // bounding volume calc. 
//            }
//        }

//        public override TV_COLLISIONRESULT AdvancedCollide(Vector3d start, Vector3d end, CONST_TV_TESTTYPE testType)
//        {
           
//            TV_COLLISIONRESULT result = new TV_COLLISIONRESULT();
//            result.bHasCollided = false;

//            if (Faces == null) return result;

//            switch (testType)
//            { 
//                case CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING :
//                    break;
//                default:
//                    break;
//            }

//            bool hit = false;
//            Triangle.TRI_FACE hitResult = new Triangle.TRI_FACE ();
//            Polygon t = null;

//            try
//            {
//                // iterate through all faces and find a hit
//                for (int i = 0; i < Faces.Count; i++)
//                {
//                    IndexedFace f = Faces[i];
//                    Vector3d[] p = new Vector3d[f.Points.Count];

//                    for (int j = 0; j < p.Length; j++)
//                    {
//                        p[j] = new Vector3d(_transformedVertices[f.Points[j].Index].X, _transformedVertices[f.Points[j].Index].Y, _transformedVertices[f.Points[j].Index].Z);
//                    }

//                    // create a triangle primitive to test
//                    //     hit = Triangle.Intersects(r, p[0], p[1], p[2], true, ref hitResult);
//                    hit = Polygon.Intersects(p, start, end, true);
//                    if (hit)
//                    {
//                        t = new Polygon(p);
//                        break;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine(ex.Message);
//            }
//            result.bHasCollided = hit;
//            if (hit)
//            {
//                result.eCollidedObjectType = CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH;
//                DebugDraw.Draw(t, CONST_TV_COLORKEY.TV_COLORKEY_RED);
                
//                //result.
//            }
//            return result;
//        }

//        internal override void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix,
//                                        bool removeScale)
//        {
//            //_mesh.AttachTo(type, objIndex, subIndex, keepMatrix, removeScale);
//        }

//        public override Shader Shader
//        {
//            set
//            {
//                if (_shader == value) return;

//                _shader = value;
//                //if (_shader == null)
//                //    _mesh.SetShader(null);
//                //else
//                //    _mesh.SetShader(_shader.TVShader);
//            }
//        }

//        public override bool IsVisible
//        {
//            get { return true; } // _mesh.IsVisible(); }
//        }

//        public override bool Overlay
//        {
//            set
//            {
//                _overlay = value;
//                // _mesh.SetOverlay(value);
//            }
//        }

//        public override bool CollisionEnable
//        {
//            set
//            {
//                _collisionEnabled = value;
//                // _mesh.SetCollisionEnable(value);
//            }
//        }

//        public override bool AlphaTest
//        {
//            set
//            {
//                _alphaTestEnable = value;
//                // _mesh.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
//            }
//        }

//        public override void SetShadowCast(bool enable, bool shadowMapping, bool selfshadows, bool additive)
//        {
//            //_mesh.SetShadowCast(enable, additive);
//        }

//        public override CONST_TV_BLENDINGMODE BlendingMode
//        {
//            set
//            {
//                _blendingMode = value;
//                // _mesh.SetBlendingMode(value);
//            }
//        }

//        public override CONST_TV_CULLING CullMode
//        {
//            set
//            {
//                _cullMode = value;
//                //_mesh.SetCullMode(value);
//            }
//        }

//        public override FrustumCullMode FrustumCullMode
//        {
//            set
//            {
//                _frustumCullMode = value;
//                //if (_frustumCullMode == FrustumCullMode.tv3d)
//                //    _mesh.EnableFrustumCulling(true);
//                //else _mesh.EnableFrustumCulling(false);
//            }
//        }

//        public override int VertexCount
//        {
//            get { return 0; }// _mesh.GetVertexCount(); }
//        }

//        public override int GroupCount
//        {
//            get { return 0; }// _mesh.GetGroupCount(); }
//        }

//        public override int TriangleCount
//        {
//            get { return 0; } // _mesh.GetTriangleCount(); }
//        }


//        public void AddGroup(Modeler.VirtualGroup group)
//        {
 
//        }

//        public void AddVertex( float x, float y, float z)
//        {
//            // _virtualVertices is designed to track when a vertex position is shared
//            // by a face where the vertex normal and / or UV coords for each shared position is different
//            // currently i dont really use it because when im reading in a .obj, i immediately check if a vertex has 
//            // any unique attribute and create a new vertex to add to _vertices if so

//            // check if the vertex already exists in _vertices
//            // 
//            // TODO: for now we're just going to add it and assume it doesnt exist
//            if (_vertices == null) _vertices = new List<CustomVertex.PositionNormalTextured> ();

//            int index = _vertices.Count;
//            _vertices.Add (new CustomVertex.PositionNormalTextured (x,y,z, 0,0,0,0,0));

//            // these freestyle vertices must also be added to the current face... of the current group.. how do we do that?
//            // i think lines that dont form a full face should still be added as a face and that such a face should be tagged as
//            // closed == false;

//                // if the vertex already exists, return

//                // 
//        }



//        public override void Update(ModeledEntity entityInstance)
//        {
//        }

//        public override void Render(ModeledEntity entityInstance, ModelBase model, Vector3d cameraSpacePosition)
//        {
//            if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
//            {
//                _currentAppearance = entityInstance.Model.Appearance;
//            }
//            else
//                _currentAppearance = null;


//            _matrix = Helpers.TVTypeConverter.ToTVMatrix(model.Matrix * entityInstance.RegionMatrix);

//            // replace the position in the matrix by the camera space position since we use camera space rendering
//            _matrix.m41 = (float)cameraSpacePosition.x;
//            _matrix.m42 = (float)cameraSpacePosition.y;
//            _matrix.m43 = (float)cameraSpacePosition.z;
//        }

//        public override void Render()
//        {
//            try
//            {

           
//            }
//            catch
//            {
//                Trace.WriteLine(string.Format("Failed to render '{0}'.", _id));
//            }
//        }



//        public CustomVertex.PositionNormalTextured[] TransformedVertices
//        {
//            get
//            {
//                // if the cached version is up to date, send it.
//                // NOTE: TransformedVertices need to be updated every time the camera changes or the mesh moves
//                //  however, they can be used readily if used for drawing the wireframe after having already computed for solid

//                // else recompute and send
//                int vertexCount = _vertices.Count;
//                _transformedVertices = new CustomVertex.PositionNormalTextured[vertexCount];
//                for (int i = 0; i < vertexCount; i++)
//                {
//                    Vector3 vec = new Vector3(_vertices[i].X, _vertices[i].Y, _vertices[i].Z);
//                    Vector3 res = Vector3.TransformCoordinate(vec, Helpers.TVTypeConverter.ToDirectXMatrix(_matrix));
//                    _transformedVertices[i] = new CustomVertex.PositionNormalTextured(res.X, res.Y, res.Z, _vertices[i].Nx, _vertices[i].Ny, _vertices[i].Nz, _vertices[i].Tu, _vertices[i].Tv);
//                }
//                return _transformedVertices;
//            }    
//        }

//        public int[] Indices
//        {
//            get
//            {
//                if (_indicesUpToDate) return _indices;
//                ComputeIndices();
//                return _indices;
//            }
//        }
        
//        public int[] Offsets
//        {
//            get
//            {
//                if (_indicesUpToDate) return _offsets;

//                ComputeIndices();
//                return _offsets;
//            }
//        }

//        public int[] MinimumVertexIndices
//        {
//            get
//            {
//                if (_indicesUpToDate) return _minimumVertexIndices;

//                ComputeIndices();
//                return _minimumVertexIndices;
//            }
//        }

//        public int[] MaximumVertexIndices
//        {
//            get
//            {
//                if (_indicesUpToDate) return _maximumVertexIndices;

//                ComputeIndices();
//                return _maximumVertexIndices;
//            }
//        }

//        private bool _indicesUpToDate = false;
//        private int[] _indices;
//        private int[] _offsets;
//        private int[] _maximumVertexIndices;
//        private int[] _minimumVertexIndices;

//        public int[] WireIndices
//        {
//            get
//            {
//                if (_wireIndicesUpToDate) return _wireIndices;
//                ComputeWireIndices();
//                return _wireIndices;
//            }
//        }
        
//        public int[] WireOffsets
//        {
//            get
//            {
//                if (_wireIndicesUpToDate) return _wireOffsets;

//                ComputeWireIndices();
//                return _wireOffsets;
//            }
//        }

//        public int[] WireMinimumVertexIndices
//        {
//            get
//            {
//                if (_wireIndicesUpToDate) return _wireMinimumVertexIndices;

//                ComputeWireIndices();
//                return _wireMinimumVertexIndices;
//            }
//        }

//        public int[] WireMaximumVertexIndices
//        {
//            get
//            {
//                if (_wireIndicesUpToDate) return _wireMaximumVertexIndices;

//                ComputeWireIndices();
//                return _wireMaximumVertexIndices;
//            }
//        }
//        private bool _wireIndicesUpToDate = false;
//        private int[] _wireIndices;
//        private int[] _wireOffsets;
//        private int[] _wireMaximumVertexIndices;
//        private int[] _wireMinimumVertexIndices;

//        private void ComputeIndices()
//        {
//            int indicesCount = 0;

//            if (Groups == null) return;

//            for (int i = 0; i < Groups.Count; i++)
//            {
//                // build our indices array
//                if (Groups[i].Faces == null) continue;
//                for (int j = 0; j < Groups[i].Faces.Count; j++)
//                    indicesCount += Groups[i].Faces[j].TriangulatedPoints.Length;  // <-- Uses the TriangulatedPoints.Length
//            }
            
//            // Now that've set the vertex buffer and the vertex format, we should itterate thru all faces, grabbing the index buffer
//            // and setting that and  making a new DrawPrimitives call for each GROUP
//            int offset = 0;
//            _indices = new int[indicesCount];
//            _offsets = new int[Groups.Count];
//            _minimumVertexIndices = new int[Groups.Count];
//            _maximumVertexIndices = new int[Groups.Count];

//            // if we have > 1 group in this mesh, build a seperate indices array for each group
//            for (int i = 0; i < Groups.Count; i++)
//            {
//                if (Groups[i].Faces == null) continue;
//                _minimumVertexIndices[i] = int.MaxValue;
//                _maximumVertexIndices[i] = 0;
//                for (int j = 0; j < Groups[i].Faces.Count; j++)
//                {
//                    for (int k = 0; k < Groups[i].Faces[j].TriangulatedPoints.Length; k++)
//                    {
//                        // add to indices
//                        int index = Groups[i].Faces[j].TriangulatedPoints[k].Index;

//                        _indices[offset + k] = index;
//                        // track the minimum index value into the vertex buffer for this group
//                        if (_minimumVertexIndices[i] > index)
//                            _minimumVertexIndices[i] = index;
//                        if (_maximumVertexIndices[i] < index)
//                            _maximumVertexIndices[i] = index;
//                    }
//                    offset += Groups[i].Faces[j].TriangulatedPoints.Length;
//                }
//                _offsets[i] = offset; // this number tells us how many vertices are in this group
//            }
//            _indicesUpToDate = true;
//        }

//        /// <summary>
//        /// We need to create new offset and idices list using the regular Points and not the TriangulatedPoints
//        /// </summary>
//        private void ComputeWireIndices()
//        {
//            int wireIndicesCount = 0;
//            if (Groups == null) return;
//            for (int i = 0; i < Groups.Count; i++)
//            {
//                // build our indices array
//                if (Groups[i].Faces == null) continue;
//                for (int j = 0; j < Groups[i].Faces.Count; j++)
//                    wireIndicesCount += Groups[i].Faces[j].Points.Count * 2;
//            }

//            // Now that've set the vertex buffer and the vertex format into the device, we should itterate thru all faces, grabbing the index buffer
//            // and setting that and  making a new DrawPrimitives call for each GROUP
//            int offset = 0;
//            _wireIndices = new int[wireIndicesCount];
//            _wireOffsets = new int[Groups.Count];
//           _wireMinimumVertexIndices = new int[Groups.Count];
//            _wireMaximumVertexIndices = new int[Groups.Count];
//            // build the indices array
//            for (int i = 0; i < Groups.Count; i++)
//            {
//                if (Groups[i].Faces == null) continue;
//                _wireMinimumVertexIndices[i] = int.MaxValue;
//                _wireMaximumVertexIndices[i] = 0;
//                for (int j = 0; j < Groups[i].Faces.Count; j++)
//                {
//                    for (int k = 0; k < Groups[i].Faces[j].Points.Count; k++)
//                    {
//                        int index = Groups[i].Faces[j].Points[k].Index;
//                        // since we're not dealing with triangulated indexed points, we 
//                        // must manually create indexed line list here on the fly by adding two points
//                        // every itteration.  We do this because LineList allows us to use a single drawprimitive call
//                        // rather than use LineStrip which will require fewer verts but more calls as we have to break between faces
//                        // to avoid LineStrip's making unwanted diagonal connections between end of one face and start of next
//                        if (k == Groups[i].Faces[j].Points.Count - 1)
//                        {
//                            // connect the face back to the beginning
//                            _wireIndices[offset + k * 2] = index;
//                            _wireIndices[offset + k * 2 + 1] = Groups[i].Faces[j].Points[0].Index;
//                        }
//                        else
//                        {
//                            _wireIndices[offset + k * 2] = index;
//                            _wireIndices[offset + k * 2 + 1] = Groups[i].Faces[j].Points[k + 1].Index;
//                        }
//                        // track the minimum index value into the vertex buffer for this group
//                        if (_wireMinimumVertexIndices[i] > index)
//                            _wireMinimumVertexIndices[i] = index;
//                        if (_wireMaximumVertexIndices[i] < index)
//                            _wireMaximumVertexIndices[i] = index;
//                    }
//                    offset += Groups[i].Faces[j].Points.Count * 2;
//                }
//                _wireOffsets[i] = offset; // this number tells us how many vertices are in this group
//            }
//            _wireIndicesUpToDate = true;
//        }

//        /// <summary>

//        /// </summary>
//        private void RenderMesh()
//        {


//            //RenderModelerFrameWithLineDraw(_transformedVertices, wireIndicesCount);
//        }


//        /// <summary>
//        /// Uses LineDraw
//        /// </summary>
//        /// <param name="vertices"></param>
//        /// <param name="indicesCount"></param>
//        //private void RenderModelerFrameWithLineDraw(CustomVertex.PositionNormalTextured[] vertices, int indicesCount)
//        //{
//        //    // LineDraw expects that the vertices are already transformed to screenspace or that such a transform is proivded in the method  call
//        //    Microsoft.DirectX.Matrix transform = (Microsoft.DirectX.Matrix.Multiply(_device.GetTransform(TransformType.View), _device.GetTransform(TransformType.Projection)));

//        //    _device.RenderState.ZBufferEnable = true;
//        //    _device.SetRenderState(RenderStates.ZBufferWriteEnable, false);
//        //                // i believe that lines need to have references to the faces they adjoin.  I see no way around this for efficiency and
//        //    // for future ability to modify these models in realtime.  The question though is, since WaveFrontObjMesh doesnt really
//        //    // need this for loading, its really our internal modeling format we're talking about that needs to know this stuff
//        //    // I believe that's something that should occur after we've loaded and before we would typically convert to TVMesh.
//        //    // So this MeshWaveFrontObj  actually could be that object...

//        //    // we need to create new offsets and 
//        //    // indices list using the regular Points and not the TriangulatedPoints
//        //    // Now that've set the vertex buffer and the vertex format into the device, we should itterate thru all faces, grabbing the index buffer
//        //    // and setting that and  making a new DrawPrimitives call for each GROUP
//        //    int offset = 0;
//        //    int[] indices = new int[indicesCount];
//        //    int[] offsets = new int[Groups.Count];
//        //    int[] minimumVertexIndices = new int[Groups.Count];
//        //    int[] maximumVertexIndices = new int[Groups.Count];

//        //    Vector3[] vertexList = new Vector3 [2];

//        //    _lineDrawer.Begin();

//        //    // build the indices array
//        //    for (int i = 0; i < Groups.Count; i++)
//        //    {
//        //        if (Groups[i].Faces == null) continue;
//        //        minimumVertexIndices[i] = int.MaxValue;
//        //        maximumVertexIndices[i] = 0;
//        //        for (int j = 0; j < Groups[i].Faces.Count; j++)
//        //        {
//        //            // check for backfacing faces which we potentically skip if this option is enabled
//        //            Types.Vector3f normal; // = Groups[i].Faces[j].GetFaceNormal();
//        //            Types.Vector3f a = new Vector3f(vertices[Groups[i].Faces[j].Points[0].Index].X,vertices[Groups[i].Faces[j].Points[0].Index].Y, vertices[Groups[i].Faces[j].Points[0].Index].Z );
//        //            Types.Vector3f b = new Vector3f(vertices[Groups[i].Faces[j].Points[1].Index].X, vertices[Groups[i].Faces[j].Points[1].Index].Y, vertices[Groups[i].Faces[j].Points[1].Index].Z);
//        //            Types.Vector3f c = new Vector3f(vertices[Groups[i].Faces[j].Points[2].Index].X, vertices[Groups[i].Faces[j].Points[2].Index].Y, vertices[Groups[i].Faces[j].Points[2].Index].Z);
//        //            Types.Vector3f v1 = a - b;
//        //            Types.Vector3f v2 = b - c;
//        //            normal =  Types.Vector3f.Normalize(Types.Vector3f.CrossProduct(v1, v2));

//        //            Types.Vector3f dir = new Vector3f(vertices[Groups[i].Faces[j].Points[0].Index].X, vertices[Groups[i].Faces[j].Points[0].Index].Y, vertices[Groups[i].Faces[j].Points[0].Index].Z);

//        //            if (Types.Vector3f.DotProduct ( dir , normal) < 0)
//        //            {
//        //                vertexList = new Vector3[Groups[i].Faces[j].Points.Count + 1];
//        //                for (int k = 0; k < Groups[i].Faces[j].Points.Count; k++)
//        //                {
//        //                    int index = Groups[i].Faces[j].Points[k].Index;
//        //                    // since we're not dealing with triangulated indexed points, we 
//        //                    // must manually create indexed line list here on the fly by adding two points
//        //                    // every itteration.  We do this because LineList allows us to use a single drawprimitive call
//        //                    // rather than use LineStrip which will require fewer verts but more calls as we have to break between faces
//        //                    // to avoid LineStrip's making unwanted diagonal connections between end of one face and start of next

//        //                    vertexList[k].X = vertices[index].X;
//        //                    vertexList[k].Y = vertices[index].Y;
//        //                    vertexList[k].Z = vertices[index].Z;

//        //                    if (k == Groups[i].Faces[j].Points.Count - 1)
//        //                    {
//        //                        // connect the face back to the beginning
//        //                        indices[offset + k * 2] = index;
//        //                        indices[offset + k*2 + 1] = Groups[i].Faces[j].Points[0].Index;

//        //                        vertexList[k + 1].X = vertices[Groups[i].Faces[j].Points[0].Index].X;
//        //                        vertexList[k + 1].Y = vertices[Groups[i].Faces[j].Points[0].Index].Y;
//        //                        vertexList[k + 1].Z = vertices[Groups[i].Faces[j].Points[0].Index].Z;
//        //                    }
//        //                    else
//        //                    {
//        //                        indices[offset + k * 2] = index;
//        //                        indices[offset + k * 2 + 1] = Groups[i].Faces[j].Points[k + 1].Index;
//        //                    }
//        //                    // track the minimum index value into the vertex buffer for this group
//        //                    if (minimumVertexIndices[i] > index)
//        //                        minimumVertexIndices[i] = index;
//        //                    if (maximumVertexIndices[i] < index)
//        //                        maximumVertexIndices[i] = index;
//        //                }
//        //                _lineDrawer.DrawTransform(vertexList, transform, System.Drawing.Color.Black.ToArgb());
//        //                offset += Groups[i].Faces[j].Points.Count * 2;
//        //            }
//        //        }
//        //        offsets[i] = offset; // this number tells us how many vertices are in this group
//        //    }

//        //    _lineDrawer.End();
//        //}


//        private void DrawVertexTabs()
//        {

//            // the color at a particular time depends on the selected status of the vert, or edge or face it's on



//            // draw the user dragable points
//            //  _device.DrawIndexedPrimitives (PrimitiveType.PointList, );

//        }

//        #endregion

//        #region IBoundVolume Members
//        // From Geometry always return the Local box/sphere
//        // and then have the model's return World box/sphere based on their instance
//        public override void UpdateBoundVolume()
//        {
//            if (!TVResourceIsLoaded) return;

//            float radius = 0f;
//            TV_3DVECTOR min, max, center;
//            min = new TV_3DVECTOR(0, 0, 0);
//            max = new TV_3DVECTOR(0, 0, 0);
//            center = new TV_3DVECTOR(0, 0, 0);

//            // model space bounding box
//            for (int i = 0; i < _vertices.Count; i++)
//            {
//                if (_vertices[i].X < min.x) min.x = _vertices[i].X;
//                if (_vertices[i].X > max.x) max.x = _vertices[i].X;
//                if (_vertices[i].Y < min.y) min.y = _vertices[i].Y;
//                if (_vertices[i].Y > max.y) max.y = _vertices[i].Y;
//                if (_vertices[i].Z < min.z) min.z = _vertices[i].Z;
//                if (_vertices[i].Z > max.z) max.z = _vertices[i].Z;
//            }

//            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
//            _sphere = new BoundingSphere(_box);
//            ClearChangeFlags();
//        }
//        #endregion

//        #region TVMesh members
//        //public new int GetIndex()
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //public new void AttachTo(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex, bool bKeepMatrix, bool bRemoveScale){}

//        //public new int GetVertexCount()
//        //{
//        //    return 0;
//        //    return _wavefrontObj.Points.Count;
//        //}

//        //public new int GetGroupCount()
//        //{
//        //    return 0; 
//        //    throw new NotImplementedException();
//        //}

//        //public new int GetTriangleCount()
//        //{
//        //    return 0;
//        //    throw new NotImplementedException();
//        //}

//        //public new TV_3DMATRIX GetMatrix() { throw new NotImplementedException(); }
//        //public new void SetMatrix(TV_3DMATRIX mMatrix){}

//        //public new void SetPrimitiveType(CONST_TV_PRIMITIVETYPE ePrimitiveType){}

//        //public new void SetMeshFormat(int combinedFormat)
//        //{ 
//        //}
//        //public new bool SetShader(TVShader sShader) { throw new NotImplementedException(); }

//        //public new void SetCullMode(CONST_TV_CULLING cullmode) { }

///// <summary>
///// NOTE: SetLightingMode must be called after geometry is loaded because the lightingmode
///// is stored in .tvm, .tva files and so anything you set prior to loading, will be
///// replaced with the lightingmode stored in the file.
///// </summary>
//        //public new void SetLightingMode(CONST_TV_LIGHTINGMODE eLightingMode){}

//        //public new void SetBlendingMode(CONST_TV_BLENDINGMODE blendingMode) { }

//        //public new void SetOverlay(bool enable) { }

//        //public new void SetAlphaTest(bool enable) { }

//        //public new void SetAlphaTest(bool enable, int refTestValue, bool depthBufferWriteEnable) {}
//        //public new void SetShadowCast(bool bEnable, bool bUseAdditiveShadows){}
//        //public new void EnableFrustumCulling(bool bEnable){}

//        //public new int GetMaterial(int Group) { throw new NotImplementedException(); }
//        //public new void SetMaterial(int iMaterial) {}
//        //public new void SetMaterial(int iMaterial, int iGroup){}
//        //public new int GetTextureEx(int iLayer, int iGroup) { throw new NotImplementedException(); }
//        //public new void SetTexture(int iTexture) {}
//        //public new void SetTexture(int iTexture, int iGroup){}
//        //public new void SetTextureEx(int iLayer, int iTexture, int iGroup){}

//        //public new void SetPosition(float x, float y, float z)
//        //{
//        //}

//        //public new bool IsVisible() { throw new NotImplementedException(); }
//        //public new void SetCollisionEnable(bool bEnableCollision){}

//        //public new void GetBoundingSphere(ref TV_3DVECTOR center, float radius, bool localSpace)
//        //{ }

//        //public new void GetBoundingBox(ref TV_3DVECTOR min, ref TV_3DVECTOR max, bool localSpace)
//        //{

//        //}
//        //public new void ForceMatrixUpdate()
//        //{}
//        //public new void ComputeNormals() {}


//        //public new void CreateBox(int width, int height, int depth) { }
//        //public new void CreateSphere (float radius, int slices, int stacks) {}
//        //public new int AddFloorGrid(int iTexture, float fX1, float fZ1, float fX2, float fZ2, int iNumCellsX, int iNumCellsZ, float fAltitude, float fTileU, float fTileV) { throw new NotImplementedException(); }


//        //public new int AddVertex(float x, float y, float z, float nx, float ny, float nz, float tu1, float tv1) { throw new NotImplementedException(); }


//        //public new void Render()
//        //{

//        //}

//        //public new void SaveTVM(string filePath) { }
//        //public new void Destroy() { }
//        #endregion

//    }
//}
