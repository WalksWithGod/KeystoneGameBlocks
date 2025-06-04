using System;
using System.Collections.Generic;
using System.Diagnostics;
using KeyCommon.Traversal;
using Keystone.Collision;
using Keystone.Elements;
using Keystone.Enums;
using Keystone.Interfaces;
using Keystone.Loaders;
using Keystone.Modeler;
using Keystone.Resource;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using Microsoft.DirectX.Direct3D;
using MTV3D65;
using StateBlock = Microsoft.DirectX.Direct3D.StateBlock;

namespace Keystone.EditDataStructures
{
    // our WaveFrontObj _mesh private var will contain bare bones data
    // but it's this class that will implement the routines against that data for computing bounding boxes and such.
    // we will _NOT_ need some new class to simulate TVMesh to do that.  THis _IS_ that new class
    public class EditableMesh : Elements.Geometry, INotifyDeviceReset
    {
        private Keystone.Types.Matrix _matrix;
        internal Cell _cell;
        private Appearance.Appearance _currentAppearance;
        public List<Modeler.MeshGroup> Groups;
        //   private List<IndexedFace > Faces;
        //   private List<TV_3DVECTOR> Points;

        // as for virtual faces, i believe there is no such thing.  All that's really important is that each IndexedVertex be able
        // to point to all other IndexedVertex's that share it's coord which is simply by  IndexedVertices[] shared =_virtualVertices[this.Index].ToArray()
        // thus when we "move" any shared vertice, we are moving all of them.

        // but there's one other concern, when it comes time to create an new VertexBuffer and IndexBuffer for solid rendering, we need to be able to
        // create these based on the construction of a lookup based on the seperate coord/normal/uv array
        // I think these should be done at the same time as we add IndexedVertex to our faces such that
        //
        //
        //    //// DrawIndexedPrimitive Demystified
        //    ////http://blogs.msdn.com/jsteed/articles/185681.aspx   
        private CustomVertex.PositionNormalTextured[] _transformedVertices;
        PickResults _lastPickResult = new PickResults();

        private int _vertexCount;
        private int _triangleCount;

        private static int COUNTER;
        public StateBlock _meshStateBlock;
        public StateBlock _defaultStateBlock;

        public EditableMesh(string id)
            : base(id)
        {
            CoreClient._CoreClient.RegisterForDeviceResetNotification(this);
            CreateStateBlocks(CoreClient._CoreClient.D3DDevice);
        }

        ~EditableMesh()
        {
            CoreClient._CoreClient.UnregisterForDeviceResetNotification(this);
            ReleaseStateBlocks();
        }

        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        public static EditableMesh Create( string resourcePath, bool loadTextures, bool loadMaterials)
        {
            EditableMesh mesh = (EditableMesh)Repository.Get(resourcePath);
            if (mesh == null)
            {
                WaveFrontObj wavefrontObj = CreateMesh(resourcePath);
                mesh = new EditableMesh(resourcePath);

                Initialize(mesh, wavefrontObj);


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

        private static WaveFrontObj CreateMesh(string resourcePath)
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();

            string[] dummy;
            WaveFrontObj m = WavefrontObjLoader.ParseWaveFrontObj(Core.FullNodePath(resourcePath), true, true, out dummy, out dummy);
            watch.Stop();

            if (m != null)
                Trace.WriteLine("EditableMesh.CreateMesh() - SUCCESS: '" + resourcePath + "' loaded with " + m.Groups.Count + " groups." + watch.Elapsed + "seconds, with = " +
                                m.Points.Count + " vertices in " + m.Faces.Count + " triangles. ");

            return m;
        }

        public bool IsEmpty()
        {
            if (_cell == null) return true;
            return _cell.Vertices == null;
        }

        public PickResults LastPickResult
        {
            get { return _lastPickResult; }
        }
        
        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
                
        public override void LoadTVResource()
        {
            //// TODO: if the mesh's src file has changed, we should unload the previous mesh first
            //if (_mesh != null)
            //{
            //    try
            //    {
            //        //_mesh.Destroy();
            //    }
            //    catch
            //    {
            //        Trace.WriteLine("error on Mesh.Destroy() - mesh path == " + _resourcePath);
            //    }
            //}

            ////// TODO: if _resourcePath == "" and _primitive = box, sphere, teapot, etc, we can load via tv's built in primitives
            //_mesh = CreateMesh(_resourcePath);
            //_mesh.SetMeshFormat((int)_meshformat);
            //_mesh.SetCullMode(_cullMode);
            //_mesh.SetBlendingMode(_blendingMode);
            //_mesh.SetOverlay(_overlay);
            //_mesh.SetAlphaTest(_alphaTestEnable);
            //if (_alphaTestEnable)
            //    _mesh.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);

            _tvfactoryIndex = int.MaxValue - COUNTER; //TODO: this is a temp hack. Why not just make _tvfactoryIndex = 1 for editablemesh?
            COUNTER++;

            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
                Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);

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
                //_mesh.Destroy();
            }
            catch
            {
            }
        }

        #endregion

        #region INotifyDeviceReset Members

        public void OnBeforeReset()
        {
            ReleaseStateBlocks();
        }

        public void OnAfterReset()
        {
            CreateStateBlocks(CoreClient._CoreClient.D3DDevice);
        }

        #endregion

        // called and is required to be called when the device is reset
        public void ReleaseStateBlocks()
        {
            if (_meshStateBlock != null)
            {
                _meshStateBlock.Dispose();
                _meshStateBlock = null;
            }
            if (_defaultStateBlock != null)
            {
                _defaultStateBlock.Dispose();
                _defaultStateBlock = null;
            }
        }

        // TODO: The stateblocks and the vertex buffer and index buffer objects should be created by the Traverser
        // and then used by those meshes when we need them.  We certainly dont want multiple vertex and index buffers since they are dynamic
        // and we should just using "SetData()" on the same one anyway
        // Create our imposter stateblock and the default rollback block 
        public void CreateStateBlocks(Device device)
        {
            // Records a stateblock for use with our imposters.  The actual device remains unchanged.
            device.BeginStateBlock();
            SetupStateBlock(device);
            _meshStateBlock = device.EndStateBlock();

            // to setup the default stateblock, you "record" the states you wish to capture within the Begin\End statements.
            // Then, when we call "Capture" on it will grab the current state on the device for just those states!  This way we can
            // easily rollback
            device.BeginStateBlock();
            SetupStateBlock(device);
            _defaultStateBlock = device.EndStateBlock();
        }

        // TODO: i should ecanpsulate this and the above CreateSTateBlocks into a class that makes using states as simiple
        // as materials
        // create a state block of all the states we will need.  
        //  NOTE: This does not set them to the device, it just creates it
        private void SetupStateBlock(Device device)
        {
            // TODO: maybe i can avoid having to transform the coords myself  by simply setting the World transform to our own mesh's matrix
            // Set world-tranform to identity (imposter billboard's vertices are already in world-space).
            device.SetTransform(TransformType.World, Microsoft.DirectX.Matrix.Identity);
            // device.Transform.

            device.SetRenderState(RenderStates.ZBufferWriteEnable, true);
            device.RenderState.DitherEnable = true;
            device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.CullMode = Cull.None;

            // TODO: these materials and textures should be tracked so they can be properly disposed afterwards...
            //  device.SetTexture(0, Helpers.TVTypeConverter.ToD3DMaterial( _currentAppearance.Groups[i].Layers[0]));
            //   device.Material = Helpers.TVTypeConverter.ToD3DMaterial(_currentAppearance.Groups[i].Material);
            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
            //tmpMat = Helpers.TVTypeConverter.FromKeystoneMaterial(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.red)); 
            tmpMat.Diffuse = System.Drawing.Color.Lavender;
            tmpMat.Ambient = System.Drawing.Color.GreenYellow;
            tmpMat.Specular = System.Drawing.Color.LightGray;
            tmpMat.SpecularSharpness = 15.0F;

            
            device.Material = tmpMat;
            device.RenderState.Ambient = System.Drawing.Color.White;
            device.RenderState.SpecularEnable = true;
            device.SetRenderState(RenderStates.DiffuseMaterialSource, true);
            // device.SetRenderState(RenderStates.AmbientMaterialSource, true);
            // device.SetRenderState(RenderStates.SpecularMaterialSource, true);
            //device.SetRenderState(RenderStates.EmissiveMaterialSource, true);

            device.SetRenderState(RenderStates.Lighting, true);
            device.Lights[0].Enabled = true;
            device.SetRenderState(RenderStates.ShadeMode, (int) ShadeMode.Gouraud);

            //device.SetRenderState(RenderStates.FogEnable, enableFog);
            //device.SetRenderState(RenderStates.FogTableMode , D3DFOG_EXP2);
            //device.SetRenderState(RenderStates.FogColor , fogColor);
            //device.SetRenderState(RenderStates.FogDensity, FloatToDWORD(fogDensity));
            // <-- TODO: must change depending on wireframe or solid
            //device.SetRenderState(RenderStates.AlphaBlendEnable, false);
            //device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            //device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            //device.SetRenderState(RenderStates.AlphaTestEnable, true);
            //device.SetRenderState(RenderStates.ReferenceAlpha, 0);
            //evice.SetRenderState(RenderStates.AlphaFunction, (int)Compare.Greater);

            //device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Modulate);
            //device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.Diffuse);
            //device.SetTextureStageState(0, TextureStageStates.ColorArgument2, (int)TextureArgument.TextureColor);
            //device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.Modulate);
            //device.SetTextureStageState(0, TextureStageStates.AlphaArgument1, (int)TextureArgument.Diffuse);
            //device.SetTextureStageState(0, TextureStageStates.AlphaArgument2, (int)TextureArgument.TextureColor);
            //    device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
            //    device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
            //    device.SetSamplerState(0, SamplerStageStates.AddressU, (int)TextureAddress.Clamp);
            //    device.SetSamplerState(0, SamplerStageStates.AddressV, (int)TextureAddress.Clamp);
        }

        private static void Initialize(EditableMesh mesh, WaveFrontObj wavefrontObj)
        {
            // In the original code, here we would take the wavefrontObj types (WaveFrontObjFace, WaveFrontObjGroup, etc)
            // and convert them to our internal format that used IndexedVertex, VirtualGroup, IndexedFace
            // Once this was done, the wavefrontOjb was no longer needed.
            // 
            // Here things are different.  The Cell is needed but we do still need to track "Groups" and
            // to store references of our Cell's faces somehow in the proper group so we know what materials / textures to use
            // particularly when rendering the solid view and not just wireframe.

            //mesh._cell = Cell.MakeTetrahedron();


            try
            {
                // the call to Create will full init the QuadEdge structure and map them to 
                mesh._cell = ObjLoader.Create(wavefrontObj);
            }
            catch (Exception)
            {
                {
                }
                throw;
            }

            // in the above "ObjLoader.Create(wavefrontObj)"
            // the wavefrontObj.Points and .Faces indices are stored in the _cell.Vertices[i].ID and _cell.Faces[i].ID
            // properties respectively!  That's very convenient.
            // What we do not yet have is a _cell.Faces[i].GroupID for the material group id...

            // Now since we no longer need to use DrawIndexedPrimitve that could change a lot of things... maybe
            // directly modifying the WaveFrontObj is ok given that we dont need this silly "virtualvertex or virtualgroup" crap
            // and all we need is a vertex list for DrawPrimitives and a triangle count..  that means no more "ComputeIndices"

            //      mesh.Faces = new List<IndexedFace>();
            //      mesh.Points = wavefrontObj.Points;

            try
            {
                ////for (int i = 0; i < mesh._cell.Faces.Length; i++)
                //for (int i = 0; i < wavefrontObj.Faces.Count; i++)
                //{
                //    // if (mesh._cell.Faces[i] == null) break; // hack because sometimes our list of _cell.Faces will be larger than actual face count due to empty pre-allocated faces toward the end of the list. TODO: that will change when i eventually switch to generic List

                //    int faceIndex = i; // (int)mesh._cell.Faces[i].ID;

                //    WaveFrontObjIndexedFace face = wavefrontObj.Faces[faceIndex];
                //    IndexedFace newFace = IndexedFaceFromWaveFrontFace(face);
                //    newFace._parentMesh = mesh;
                //    mesh.Faces.Add(newFace);
                //}
            }
            catch (Exception)
            {
                {
                }
                throw;
            }
        }

        private static IndexedFace IndexedFaceFromWaveFrontFace(WaveFrontObjIndexedFace face)
        {
            IndexedFace newface = new IndexedFace();

            for (int i = 0; i < face.Points.Length; i ++)
            {
                if (face.Textures != null)
                    newface.Add(face.Points[i], face.Textures[i]);
                else
                    newface.Add(face.Points[i]);


                newface.Group = -1;
                newface.SmoothingGroup = -1;
            }

            newface.Triangulate();
            return newface;
        }

        /// <summary>
        /// Create's a vertex we can use with DrawPrimitives from an index into our non transformed Point array and a supplied face normal.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="normal"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private CustomVertex.PositionNormalTextured MakeD3DCustomVertex(Vector3d vec, Vector3f normal, bool transform)
        {
            CustomVertex.PositionNormalTextured customVertex; // = new CustomVertex.PositionNormalTextured();

            Vector3d coord = vec;
            if (transform)
                coord = Vector3d.TransformCoord(coord, _matrix);

            customVertex.X = (float) coord.x;
            customVertex.Y = (float) coord.y;
            customVertex.Z = (float) coord.z;
            customVertex.Nx = normal.x;
            customVertex.Ny = normal.y;
            customVertex.Nz = normal.z;
            customVertex.Tu = 0.0f;
            customVertex.Tv = 0.0f;


            //    if (currentNormalIndex > -1)
            //    {
            //        customVertex.Nx = face.Normal.x;
            //        customVertex.Ny = wavefrontObj.Normals[currentNormalIndex].y;
            //        customVertex.Nz = wavefrontObj.Normals[currentNormalIndex].z;
            //    }
            //    if (currentUV > -1)
            //    {
            //        customVertex.Tu = wavefrontObj.UVs[currentUV].x;
            //        customVertex.Tv = wavefrontObj.UVs[currentUV].y;
            //    }
            return customVertex;
        }

        public CustomVertex.PositionNormalTextured[] TransformedVertices(bool wireframe, bool lineList,bool transformed,out int[] vertCountsPerFace)
        {
            int vertexCount = 0;
            int count = 0;
            int[] vertexCountsPerFace;

            // NOTE: TransformedVertices need to be updated every time the camera changes or the mesh moves
            //  however, they can be used readily if used for drawing the wireframe after having already computed for solid
            // We could cache an array of NON transformed vertices that is identicle below but without the transform
            // and then only update it (and potentially only update specific verts that have changed) when it's dirty
            // the main shitty part is needing to have untransformed verts for both wire and solid
            if (wireframe && lineList)
            {
                // untriangulated vertex count 
                for (int i = 0; i < _cell.FaceCount; i++)
                    vertexCount += _cell.Faces[i].VertexCoordinates.Length;

                _transformedVertices = new CustomVertex.PositionNormalTextured[vertexCount*2];
                vertexCountsPerFace = new int[_cell.FaceCount];

                for (int i = 0; i < _cell.FaceCount; i++)
                {
                    Vector3f normal = _cell.Faces[i].FaceNormal;
                    vertexCountsPerFace[i] = _cell.Faces[i].VertexCoordinates.Length;

                    for (int j = 0; j < vertexCountsPerFace[i]; j++)
                    {
                        Vector3d vec = _cell.Faces[i].VertexCoordinates[j];

                        _transformedVertices[count] = MakeD3DCustomVertex(vec, normal, transformed);
                        count++;

                        // connect the face back to the beginning if we've reached the last Point in this face
                        if (j == vertexCountsPerFace[i] - 1)
                            vec = _cell.Faces[i].VertexCoordinates[0];
                        else
                            vec = _cell.Faces[i].VertexCoordinates[j + 1];

                        _transformedVertices[count] = MakeD3DCustomVertex(vec, normal, transformed);
                        count++;
                    }
                }
            }
            else if (wireframe) // fill linestrip array as opposed to linelist above
            {
                for (int i = 0; i < _cell.FaceCount; i++)
                    vertexCount += _cell.Faces[i].VertexCoordinates.Length; //_cell.Faces[i].Vertices.Length;

                _transformedVertices = new CustomVertex.PositionNormalTextured[vertexCount];
                vertexCountsPerFace = new int[_cell.FaceCount];

                for (int i = 0; i < _cell.FaceCount; i++)
                {
                    Vector3f normal = _cell.Faces[i].FaceNormal;
                    vertexCountsPerFace[i] = _cell.Faces[i].VertexCoordinates.Length;

                    for (int j = 0; j < vertexCountsPerFace[i]; j++)
                    {
                        Vector3d vec = _cell.Faces[i].VertexCoordinates[j];
                        _transformedVertices[count] = MakeD3DCustomVertex(vec, normal, transformed);
                        count++;
                    }
                }
            }
            else // triangulated list
            {  
                // first we need the triangulated face count for solid rendering
                for (int i = 0; i < _cell.FaceCount; i++)
                    vertexCount += _cell.Faces[i].TriangulatedVertexCoordinates.Length;

                _transformedVertices = new CustomVertex.PositionNormalTextured[vertexCount];
                vertexCountsPerFace = new int[_cell.FaceCount];

                for (int i = 0; i < _cell.FaceCount; i++)
                {
                    Vector3f normal = _cell.Faces[i].FaceNormal;
                    vertexCountsPerFace[i] = _cell.Faces[i].TriangulatedVertexCoordinates.Length;

                    for (int j = 0; j < vertexCountsPerFace[i]; j++)
                    {
                        Vector3d vec = _cell.Faces[i].TriangulatedVertexCoordinates[j];
                        _transformedVertices[count] = MakeD3DCustomVertex(vec, normal, transformed);
                        count++;
                    }
                }
            }

            vertCountsPerFace = vertexCountsPerFace;
            return _transformedVertices;
        }


        private void DrawVertexTabs()
        {
            // the color at a particular time depends on the selected status of the vert, or edge or face it's on


            // draw the user dragable points
            //  _device.DrawIndexedPrimitives (PrimitiveType.PointList, );
        }


        public Vertex AddVertex(float x, float y, float z)
        {
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
            // for a vertex to be added it has to be added to the list of Points
            // if the point already exists, then obviously the vertex does not have to be added,
            // but it's index is added to a Face.
            // An indext must be added to a Face.  If no face, a new one is made
            // 
            // Now a key point to realize is that only if we're starting with an existing vertex is there
            // any chance that we are dealing with an existing face.  

            //     if (Points == null) Points = new List<TV_3DVECTOR>();
            //     if (Faces == null) Faces = new List<IndexedFace>();

            // shouldnt we have to obtain a face index first?  


            // 1) i think wehenever we plot points, we're talking about a single face
            // 2) whenever we stop drawing a continously plot of face vertex points and stop and start again, we are potentially starting a new face
            // 3) Now i think the way the quadedge structure works, these unclosed faces or "degenerate" faces should be treated as fully legitimate
            //     and have their edges or whatever loop back around to themselves...
            //
            //     - so really, i think this is how we need to approach this, with a single vert for instance being part of an edge that has
            //      the same vert as dest and orig.  Then when we add a new vertex, we split that edge.  The faces really do exist
            //      To loop around to find the face, that is what i'm not sure about...I do know that if you split an edge that is of one vert
            // to consist of two verts it looks like    v1->v2->v2  <-- and we'll know it's degenerate face when the final vert is not the first.
            
            if (_cell == null)
                _cell = Cell.Make();

            Edge edge1;
            Face left;
            Face right;
            Vertex vertex0= _cell.GetVertex(x, y, z); // does the vertex already exist
     
            if (vertex0 == null)
            {
                // create it
                vertex0 = Vertex.Make(_cell); // make also adds the vertex 
                vertex0.Position[0] = x; // = new float[] {x, y, z};
                vertex0.Position[1] = y;
                vertex0.Position[2] = z;
                // here we know the vertex is a closed loop in a degenerate edge and degenerate face
                // TODO: so do we need to set any flags on the edge/faces indicating they are degenerate?
                
            }
            
            System.Diagnostics.Trace.WriteLine("AddVertex");
            return vertex0;
        }



        public Edge AddEdge(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            if (_cell == null) 
                _cell = Cell.Make();

            SetChangeFlags(ChangeStates.GeometryAdded | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
            Vertex v1=null;
            Vertex v2=null;

            // after Cell.Make() vertex count minimum will always be 1, never 0. 
            // TODO: this means that on delete of last edge, we must delete the cell or re-make it.
            if (_cell.VertexCount == 1)
            {
                // grab the initial vertex 
                CellVertexIterator iterator = new CellVertexIterator(_cell);
                v1 = iterator.Next();
                Trace.Assert(v1 != null);
                // position the vertex 
                v1.Position[0] = x1;
                v1.Position[1] = y1;
                v1.Position[2] = z1;

                // v1 must be part of a degenerate edge & thus faces by definition
                // because we do not allow for a single vertex to be added.  Minimum allowed added is one edge.

                Edge edge1 = v1.Edge;
                Trace.Assert(edge1.Origin == edge1.Destination); // proves it's part of a degenerate face
                Face left = edge1.Left;
                Face right = edge1.Right;
             }

             if (v1 == null) 
                 v1 = _cell.GetVertex(x1, y1, z1);
            
            v2 = _cell.GetVertex(x2, y2, z2);
             
            // neither already exist
            if (v1 == null && v2 == null)
            {
                v1 = Vertex.Make(_cell);
                Face left = Face.Make(_cell);
                Face right = Face.Make(_cell);
                Edge edge = Edge.Make(_cell).InvRot;
                edge.Origin = v1;
                edge.Destination = v1;
                edge.Left = left;
                edge.Right = right;

                v1.Position[0] = x1;
                v1.Position[1] = y1;
                v1.Position[2] = z1;
            }
            else  if (v1 != null && v2 != null)
            {
                // if they are apart of the same edge, return
                if (v1.Edge.ID == v2.Edge.ID) return v1.Edge;

                // can they still be isolated vertices not connected to anything at this point?
                // I think no but only because my code should never allow a vertex to be created
                // and added to the scene in isolation
                Trace.Assert(v1.Edge != null);

                 // else the splicing in of a new edge must be done
                
                //Vertex tmp = _cell.MakeVertexEdge(v1, v1.Edge.Left, v1.Edge.Right).Destination;

                Edge splice = Edge.Make(_cell);
                splice.Origin = v1;
                splice.Destination = v2;

                //Edge.Splice( splice)
                //Edge.Splice(v1.Edge, v2.Edge);
                return null; // nothing was added so 
            }

            // at this point one or the other may still be non null, but not both
            if (v2 == null)
            {

                // the 2nd vertex can't exist with vertex Count == 1 so create it and splice it into the degenerate edge of v1
                v2 = _cell.MakeVertexEdge(v1, v1.Edge.Left, v1.Edge.Right).Destination;
                v2.Position[0] = x2;
                v2.Position[1] = y2;
                v2.Position[2] = z2;
                
            }
            else
            {
                // the 2nd vertex can't exist with vertex Count == 1 so create it and splice it into the degenerate edge of v1
                v1 = _cell.MakeVertexEdge(v2, v2.Edge.Right, v2.Edge.Left).Destination;
                v1.Position[0] = x1;
                v1.Position[1] = y1;
                v1.Position[2] = z1;
            }

            Trace.Assert(v1 != null && v2 != null);
            return v1.Edge;
  
                // if the 1st vertex already exists and the 2nd doesnt, then the code is again just like the above 

                // if the 2nd vertex already exists, check that it's not already apart of the edge of v1
                // if so, just return
                // if not and if 1st vertex doesnt already exist, we make vertex edge 
                // if not, and the 1st vertex does exist, we splice with v1
                // else if the 2nd vertex doesnt exist either, we make one and then make vertex edge with the other


            
            


            // AddEdge means we're splitting an edge and that v0 must already exist
            // if v2 also already exists, that doesnt mean the edge already exists... it just means there's at least
            // one other edge in the cell that is using that vertex
            // double check how MakeTetrahedron works...

            //v1 = AddVertex(x1, y1, z1);
            //v2 = AddVertex(x2, y2, z2);

            // if the first vertex is isolated


            // if the second vertex is isolated

            
            

            //// our faces and edge already exist
            //// grab the initial edge and the initial faces
            //edge1 = vertex0.Edge;
            //left = edge1.Left;
            //right = edge1.Right;


            //_cell.MakeVertexEdge(v1, left, right);

            /////////
            // the faces that get created must have their ID assigned to the index value.
            // the obvious way to deal with this is frankly to use mesh._cell.Faces and then to have each Face
            // reference an actual IndexedFace and not just an ID.   That could eventually be a step toward unifying Face and IndexedFace
            // same thing frankly with cell.Vertices and mesh.Points

            // NOTE: that Face.ID and Vertex.ID are only used for debug printing and for objLoader to matchup with the EditableMesh
            // Points and IndexedFaces!  And even with wavefrontobj's loaded, once you actually tried to edit one, your indices
            // would start to screw up.

            // and remember, it's not like we have to worry about Faces and Vertices for the purposes of RENDERING
            // if the index values aren't perfectly 0 - N with no null items inbetween because we dont use DrawIndexedPrimitive 
            // anymore.  The only thing they'd be required for is when creating an exported .obj file.
            // Also note that QuadEdge structure inherently shares vertex coords and such so there's no need to
            // worry about managing the IndexedPoints to the IndexedFace which is the only reason the IndeedFace exists....
            // 
            // So yeah, we've got some design issues still... i have to sort through all these tonite...
            // Ideally, it'd be nice _if_ we could guarantee that our cell Face and Vertices had id's that matched
            // their position in the indexed list, HOWEVER, i think in any case, with any datastructures, editing
            // using these indexed lists would result in having to manage that crap.  That's why a key thing here
            // is to understand that for editing, those lists arent practical.  They only need to be geenrated during export
            // and as i said earlier we are already properly sharing vertex coords with quadedge

            // So our main concern i think is caching just those parts we need for rendering... face normals, groups,
            // but most importantly we're caching the POINT coords each face is currently comprised of _and_
            // that these Points unlike the quadedge faces, are always real and not part of degenerate sort of quadedge datastructure placeholders
            // However, i suppose that _if_ we can properly resolve this such that a call to Vertices() on a particular face will cache
            // those values until that face changes... then that's great _and_ assuming we can also use it to track
            // or figure out whichv erts are real and thus if the face is even draw-able.  

            // of their corresponding _mesh.Faces.  In a dynamically edited mesh im not sure how i maintain this...
        }

        public void RemoveEdge()
        {
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }

        // TODO: Why isn't this a method for a Face ?  Face.PickClosestEdge ()
        private int FindClosestEdge(Edge[] edges, Vector3d intersectionPoint, ref Vector3d coord, ref Vector3d origin, ref Vector3d dest)
        {
            double distance = double.MaxValue;
            int closestEdgeID = -1;
            Vector3d closestPoint;
            if (edges.Length > 0)
            {
                for (int j = 0; j < edges.Length; j++)
                {
                    Vector3d point, o, d;
                    o.x = edges[j].Origin.Position[0];
                    o.y = edges[j].Origin.Position[1];
                    o.z = edges[j].Origin.Position[2];
                    o = Vector3d.TransformCoord(o, _matrix);
                    d.x = edges[j].Destination.Position[0];
                    d.y = edges[j].Destination.Position[1];
                    d.z = edges[j].Destination.Position[2];
                    d = Vector3d.TransformCoord(d, _matrix);
                    double dist = Line3d.DistanceSquared(o, d,
                                                         intersectionPoint, out point);
                    if (dist < distance)
                    {
                        distance = dist;
                        closestPoint = point;
                        closestEdgeID = (int)edges[j].ID; // j;
                        origin = o;
                        dest = d;
                    }
                }
            }
            // the below i beleive is actually finding the cloest endpoint on the cloesest edge
            // the closestPoint is not necessarily an end point of this edge
            // it's just the closest perpendicular to the line
            // Actually, even this is not true.  The closest vertex is not even necessarily on this edge!
            // finding the nearest edge to the impact point does not necessarily mean that the closest 
            // vertex also lies on that closest edge.  In the case of a triangle that is very wide at it's base but with a short
            // peak at center top, you could have an impact point closest to the bottom edge but with closest vertex at peak.
            // So the following code is incorrect really... What we really need is a new routine to find closest vert
            // were we test dist and dist2 and then get the intersectionPoint and then compare those in the above closest edge
            // search loop.  To avoid having to do these loops twice, keep in mind that typically you're in edge or vert selection mode
            // so you'd only do one or the other.  Verify in sketchup how that works
            //result.iNearestVertex =;
            double dist1 = Vector3d.GetDistance3dSquared(origin, intersectionPoint);
            double dist2 = Vector3d.GetDistance3dSquared(dest, intersectionPoint);
            if (dist1 < dist2)
            {
                coord = origin;
                //   result.VertexIndex = (int)edges[iclosestEdge].Origin.ID;
            }
            else
            {
                coord = dest;
                //   result.VertexIndex = (int)edges[iclosestEdge].Destination.ID;
            }

            return closestEdgeID;
        }

        public Face GetFace(uint faceID)
        {
            return _cell.GetFace(faceID);
        }

        public Edge GetEdge(uint edgeID)
        {
            return _cell.GetEdge(edgeID);
        }

        public Vector3d GetVertex(uint vertexID, bool worldSpace)
        {
            Vertex v = _cell.GetVertex(vertexID);
            Vector3d result;
            result.x = v.Position[0];
            result.y = v.Position[1];
            result.z = v.Position[2];
            if (!worldSpace) return result;

            return Vector3d.TransformCoord(result, _matrix);
        }

        internal void TranslateVertex(uint vertexID, Vector3d translation)
        {
            _cell.TranslateVertex(vertexID, translation);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }

        internal void TranslateEdge(uint edgeID, Vector3d translation)
        {
            _cell.TranslateEdge(edgeID, translation);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);

        }
        internal void TranslateFace(uint faceID, Vector3d translation)
        {
            _cell.TranslateFace(faceID, translation);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }


        internal void MoveVertex(uint vertexID, Vector3d position)
        {
            _cell.MoveVertex(vertexID, position);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }

        internal void MoveEdge(uint edgeID, Vector3d[] position)
        {
            _cell.MoveEdge(edgeID, position);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }

        internal void MoveFace(uint faceID, Vector3d[] position)
        {
            _cell.MoveFace(faceID, position);
            SetChangeFlags(ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, ChangeSource.Self);
        }


        //public void AddChild(Mesh3d m, int groupID)
        //{
        //    m.AttachTo(CONST_TV_NODETYPE.TV_NODETYPE_MESH, TVIndex, groupID, false, false);
        //    base.AddChild(m);
        //}

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


        internal bool Overlay
        {
            set
            {
                //_overlay = value;
                // _mesh.SetOverlay(value);
            }
        }


        internal override void SetAlphaTest (bool enable, int iGroup)
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
        //    // no getter because this is a very unreliable matrix _and_ we would lose precision since we want to stay with doubles.
        //    // get the Matrix from the parent Entity instead. NOT HERE!
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
            get { return 0; } // _mesh.GetVertexCount(); }
        }

        public override int GroupCount
        {
            get { return 0; } // _mesh.GetGroupCount(); }
        }

        public override int TriangleCount
        {
            get { return 0; } // _mesh.GetTriangleCount(); }
        }

        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // TODO: since i added the worldMatrix parameter and
            // commented out the Matrix property, i havent taken into account the
            // worldMatrix at all so this collision will probably break.
            _lastPickResult.HasCollided = false;
            if (_cell.Faces == null) return _lastPickResult;

            if (parameters.Accuracy == PickAccuracy.BoundingBox || parameters.Accuracy == PickAccuracy.BoundingSphere)
            {
                _lastPickResult.HasCollided = true;
                _lastPickResult.CollidedObjectType = PickAccuracy.Geometry | PickAccuracy.EditableGeometry;
                return _lastPickResult;
            }

            if (parameters.Accuracy == PickAccuracy.Face)
            {
                int[] vertCounts;
                int count = 0;
                // if the source is picking during editing, then we want to test against  non triangulated faces so we pass (false) for first param to TransformedVertices
                CustomVertex.PositionNormalTextured[] v = TransformedVertices(true, false, true, out vertCounts);

                try
                {
                    // itterate through all faces and test for Polygon intersection
                    for (int i = 0; i < _cell.Faces.Length; i++)
                    {
                        // TODO: why not just transform the ray to modelspace?!
                        Vector3d[] p = new Vector3d[_cell.Faces[i].Vertices.Length];
                        uint[] facePointIDs = new uint[p.Length];
                        for (int j = 0; j < _cell.Faces[i].Vertices.Length; j++)
                        {
                            
                            p[j].x = _transformedVertices[count].X; 
                            p[j].y = _transformedVertices[count].Y;
                            p[j].z = _transformedVertices[count].Z;
                            facePointIDs[j] = _cell.Faces[i].Vertices[j];
                            count++;
                        }

                        // TODO: why not just transform the ray to modelspace?! and then
                        // we can skip all the above TransformedVertices
                        Vector3d dir = Vector3d.Normalize(end - start);
                        Ray r = new Ray(start, dir);
                        //Triangle.TRI_FACE hitResult = new Triangle.TRI_FACE();
                        //hit = Triangle.Intersects(r, p[0], p[1], p[2], true, ref hitResult);
                        Vector3d intersectionPoint;
                        const double RAY_SCALE = 1000000d;// picking an editable mesh assumes camera will be relatively close to it and that the interior is not "huge"
                        bool hit = Polygon.Intersects(r, RAY_SCALE, p, parameters.SkipBackFaces, out intersectionPoint);
                        if (hit)
                        {
                            _lastPickResult.HasCollided = true;
                            _lastPickResult.CollidedObjectType = PickAccuracy.Geometry | PickAccuracy.EditableGeometry;
                            _lastPickResult.FaceID = (int)_cell.Faces[i].ID;
                            _lastPickResult.FacePoints = p;
                            _lastPickResult.FacePointIDs = facePointIDs;
                            _lastPickResult.ImpactPointLocalSpace = intersectionPoint;
                            _lastPickResult.ImpactNormal = -dir; // impact normal is opposite of ray dir
                            if (parameters.Accuracy == PickAccuracy.Edge)
                            {
                                // note that the Face.ID is not necessarily the same as it's index in _cell.Faces[].  But this is not a problem
                                // as long as we're not trying to use Face.ID as an index value.  Instead we would need to itterate via _cell.GetFace(id)
                                Face qeFace = _cell.Faces[i];
                                Edge[] edges = qeFace.Edges;
                                Vector3d coord = new Vector3d();
                                Vector3d origin = new Vector3d();
                                Vector3d dest = new Vector3d();
                                _lastPickResult.EdgeID = FindClosestEdge(edges, intersectionPoint, ref coord, ref origin, ref dest);
                                if (_lastPickResult.EdgeID > -1)
                                {
                                    _lastPickResult.EdgeOrigin = origin;
                                    _lastPickResult.EdgeDest = dest;
                                }
                                // if testType is vertexSelect as opposed to EdgeSelect we should find nearest vertex which is close to the intersection Point
                                // but perhaps only if within some epsilon?  

                                // silo3d renders all points when in vertex select mode as a 3x3 pixel square with the middle pixel empty as if
                                // it's just 3x3 points
                                // Sketchup only draws the line tool's point as blue if it's on a face, red if it's on an edge, and light
                                // blue if it's on a midpoint of an edge, green if it's on a vertex.  For floor plans their system is better
                                // so what we need to do is first in our result, determine 
                                // use the new result
                                // also note in Sketchup the mouse never snaps.  The colored points do snap depending on where the mouse is tho
                                // i suspect this is based on screenspace distance such that if you zoomed in closer being the same phyical world distance
                                // at close range would not result in the snapping
                                // screenspace distance is simply unprojecting both 3d points and comparing distance

                                // Triangle.ClosestPointOnTriangle();

                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
            return _lastPickResult;
        }


        internal override void Render(Keystone.Types.Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
            try
            {

                // TODO: when modifying a mesh, my parent entity isnt getting a changeflag propogation so the SceneNode never updates
                // but why then is the bounding "pick" not the same?  I suspect it's because the low level picking then fails to work.. but
                // why not even when in some of the valid parts of the old bounds?
                // and super annoying, i must fix the ImportMesh to put the object not on the camera's y but on a set plane... 
                UpdateBoundVolume();
                if (model.Appearance != null)
                {
                    _currentAppearance = model.Appearance;
                }
                else
                    _currentAppearance = null;

                _matrix = matrix;
                ////_matrix = model.Matrix * entityInstance.RegionMatrix;
                //_matrix =  entityInstance.RegionMatrix;

                //// replace the position in the matrix by the camera space position since we use camera space rendering
                //_matrix.M41 = cameraSpacePosition.x;
                //_matrix.M42 = cameraSpacePosition.y;
                //_matrix.M43 = cameraSpacePosition.z;
            }
            catch
            {
                Trace.WriteLine(string.Format("Failed to render '{0}'.", _id));
            }

        }

        internal Keystone.Types.Matrix GetMatrix() {return _matrix;}

        #endregion

        #region IBoundVolume Members

        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            // TODO:
            //if (!TVResourceIsLoaded) return;

            float radius = 0f;
            TV_3DVECTOR min, max, center;
            min = new TV_3DVECTOR(0, 0, 0);
            max = new TV_3DVECTOR(0, 0, 0);
            center = new TV_3DVECTOR(0, 0, 0);

            // model space bounding box
            for (int i = 0; i < _cell.Vertices.Length; i++)
            {
                if (_cell.Vertices[i] == null) break;
                if (_cell.Vertices[i].Position[0] < min.x) min.x = _cell.Vertices[i].Position[0];
                if (_cell.Vertices[i].Position[0] > max.x) max.x = _cell.Vertices[i].Position[0];
                if (_cell.Vertices[i].Position[1] < min.y) min.y = _cell.Vertices[i].Position[1];
                if (_cell.Vertices[i].Position[1] > max.y) max.y = _cell.Vertices[i].Position[1];
                if (_cell.Vertices[i].Position[2] < min.z) min.z = _cell.Vertices[i].Position[2];
                if (_cell.Vertices[i].Position[2] > max.z) max.z = _cell.Vertices[i].Position[2];
            }

            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
            _sphere = new BoundingSphere(_box);
            DisableChangeFlags(ChangeStates.BoundingBox_TranslatedOnly | ChangeStates.BoundingBoxDirty);
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