using System;
using Keystone.Modeler;
using Keystone.Types;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Color=System.Drawing.Color;
using Matrix=Keystone.Types.Matrix;


namespace Keystone.DXObjects
{
    internal class MeshRenderer : IDisposable
    {
        private int _maxVertices = 500000;
        private Usage _useage;
        private Device _device = new Device(CoreClient._CoreClient.Internals.GetDevice3D());
        private Pool _pool;
        private VertexFormats _FVF;
        private VertexFormats _wireFrameFVF;
        private VertexBuffer _wireFrameVertexBuffer;
        private IndexBuffer _wireFrameIndexBuffer;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        //private VertexDeclaration _vertexDeclaration;
        //public readonly VertexElement[] _vertexElements =
        //        new[] 
        //        {
        //            new VertexElement(0, 0, 
        //                    DeclarationType.Float3,
        //                    DeclarationMethod.Default,
        //                    DeclarationUsage.Position,0),
        //            new VertexElement(0, 0,
        //                    DeclarationType.Float3,
        //                    DeclarationMethod.Default, 
        //                    DeclarationUsage.Normal, 0),
        //           new VertexElement(0, 0, 
        //                    DeclarationType.Float2,
        //                    DeclarationMethod.Default,
        //                    DeclarationUsage.TextureCoordinate, 0),
        //                    VertexElement.VertexDeclarationEnd
        //        };


        public MeshRenderer()
        {
            // NOTE: with Pool.Managed in our VertexBuffer, the Usage type is limited.  So far Usage.None works but Usage.Dynamic | Usage.WriteOnly does not.
            // TODO: BUT, I NEED / WANT TO USE DYNAMIC buffers!!!!  Also read this:
            // http://www.pluralsight.com/blogs/craig/archive/2005/03/14/6693.aspx
            _pool = Pool.Default;
            _useage = Usage.Dynamic | Usage.WriteOnly;
            _FVF = CustomVertex.PositionNormalTextured.Format;
            _wireFrameFVF = CustomVertex.PositionColored.Format;

            //_vertexDeclaration = new VertexDeclaration(_device, _vertexElements);

            _vertexBuffer =
                     new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), _maxVertices, _device, _useage,
                     _FVF, _pool);

            _wireFrameVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), _maxVertices, _device, _useage, _wireFrameFVF, _pool);

            // TODO: some cards cant use 16 bit indices but i suspect every SM 2.0 card will and thats all we support.
            _indexBuffer = new IndexBuffer(_device, _maxVertices * sizeof(int), _useage, _pool, false);
            _wireFrameIndexBuffer = new IndexBuffer(_device, _maxVertices * sizeof(int), _useage, _pool, false);

            _lineDrawer = new Line(_device);
            _lineDrawer.Antialias = true;
            _lineDrawer.GlLines = true;
            //_lineDrawer.Width = 3.0f;
        }

        ~MeshRenderer()
        {
            if (_lineDrawer != null) _lineDrawer.Dispose();
            _lineDrawer = null;
        }

        public void Render (EditDataStructures.EditableMesh mesh, Matrix entityMatrix, Matrix view, Matrix projection)
        {
            if (mesh.IsEmpty()) return; // empty new editable mesh

            // now set the state on the device for use
    //        mesh._defaultStateBlock.Capture();
    //        mesh._meshStateBlock.Apply();
            _device.VertexShader = null;
            _device.PixelShader = null;
            _device.SetRenderState(RenderStates.CullMode, (int)Cull.None);
            _device.RenderState.FillMode = FillMode.Solid;
            _device.RenderState.DitherEnable = true;
            _device.SetRenderState(RenderStates.ShadeMode, (int)ShadeMode.Gouraud);

            _device.SetRenderState(RenderStates.AlphaTestEnable, false);
            _device.SetRenderState(RenderStates.AlphaBlendEnable, false);
            _device.SetRenderState(RenderStates.ZEnable, true);
            _device.SetRenderState(RenderStates.ZBufferWriteEnable, true);

            // global ambient
            Color globalAmbient = Color.FromArgb(0, 0, 0, 0); //System.Drawing.Color.White;
            _device.RenderState.Ambient = globalAmbient;

            // configure and assign material
            Color diffuse, ambient, specular, emissive;
            diffuse = Color.FromArgb(1, 0, 0, 1); //System.Drawing.Color.Khaki ;
            ambient = Color.FromArgb(1, 0, 0, 0); // System.Drawing.Color.Gold;
            specular = Color.FromArgb(1, 0, 0, 0); //System.Drawing.Color.LightGray;
            emissive = Color.FromArgb(1, 0, 0, 0);
            Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
            tmpMat.Diffuse = diffuse;
            tmpMat.Ambient = ambient;
            tmpMat.Specular = specular;
            tmpMat.Emissive = emissive;
            tmpMat.SpecularSharpness = 15.0F;
            _device.Material = tmpMat;

            // ColorSource.Color1 and ColorSource.Color2 refer to the vertexformat's Color1 and Color2 values
            _device.SetRenderState(RenderStates.DiffuseMaterialSource, (int)ColorSource.Material);
            _device.SetRenderState(RenderStates.AmbientMaterialSource, (int)ColorSource.Material);
            _device.SetRenderState(RenderStates.SpecularMaterialSource, (int)ColorSource.Material);
            _device.SetRenderState(RenderStates.EmissiveMaterialSource, (int)ColorSource.Material);

            // enable and configure light
            _device.RenderState.LocalViewer = true; // LOCALVIEWER is used to compute accurate (but slower) specular highlights. You should use it only where objects fill a large portion of the viewport or where accurate calculation of lighting effects is required. Since LOCALVIEWER is set to TRUE by default in DX7 you should be aware of the implied cost and consider disabling it unless you feel that the benefits outweigh the costs.
            _device.RenderState.SpecularEnable = true; // enable specular lighting highlights
            _device.RenderState.Lighting = true;

            diffuse = Color.FromArgb(1, 1, 1, 1);
            ambient = Color.FromArgb(1, 1, 1, 1);
            specular = Color.FromArgb(1, 1, 1, 1);
            _device.Lights[1].Type = LightType.Directional;
            _device.Lights[1].Ambient = ambient;
            _device.Lights[1].Diffuse = diffuse;
            _device.Lights[1].Specular = specular;
            _device.Lights[1].Direction = new Vector3(0.5f, -0.5f, 0.8f);
            _device.Lights[1].Update();
            _device.Lights[1].Enabled = true;
            _device.Lights[0].Enabled = false;



            int[] dummy;
            CustomVertex.PositionNormalTextured[] transformedVerts = mesh.TransformedVertices (true, true, false,out dummy);

            //// draw solid

            //// is it faster to graphic stream + lock/unlock over SetData?  
            //// according to the following url, the underlying codes are about the same
            //// http://www.gamedev.net/community/forums/topic.asp?topic_id=346308
            //GraphicsStream gs = _vertexBuffer.Lock(0, 0, 0);
            //gs.Write(transformedVerts);
            //_vertexBuffer.Unlock();
            ////_vertexBuffer.SetData(transformedVerts, 0, 0);
            //_device.SetStreamSource(0, _vertexBuffer, 0);
            //_device.DrawPrimitives(PrimitiveType.TriangleList, 0, transformedVerts.Length / 3);
            // TODO: is the view already 0,0,0?
            // TODO: wait, we _do_ want to render with respect to 0,0,0 so a translated entityMatrix!

            // draw wire
            // - i cant get the primitives to be visible withou a tvmesh added aftewrads
            _device.SetTransform(TransformType.View, Helpers.TVTypeConverter.ToDirectXMatrix(view));
          //  _device.SetTransform(TransformType.Projection, Helpers.TVTypeConverter.ToDirectXMatrix(projection));
            Matrix matrix = mesh.GetMatrix();
            _device.SetTransform(TransformType.World, Helpers.TVTypeConverter.ToDirectXMatrix(matrix));

            // _device.VertexDeclaration = _vertexDeclaration;
            _device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            _wireFrameVertexBuffer.SetData(transformedVerts, 0, 0);
            _device.SetStreamSource(0, _wireFrameVertexBuffer, 0);
            int count = transformedVerts.Length / 2;
            _device.DrawPrimitives(PrimitiveType.LineList, 0, count);
            _device.Lights[1].Enabled = false;
            _device.Lights[0].Enabled = true;

            // NOTE: We need seperate rollback stateblocks for each viewport
            // rollback the state by applying all the existing states we copied prior to setting up for rendering our imposters
  //          mesh._defaultStateBlock.Apply();

            // now render any debug or real time editing UI markers 
            RenderDebug(mesh); // TODO: renderDebug is temporary because ideally, we should render those things above but with special case handling
            // for affected faces
            // yes, these debug lines should be drawn in special in place of the other lines (it's just changing the color really)
            // so this is the proper place/time to draw and NOT in Scene after everything is drawn.
            // second, same goes for drawing a transparent face.. we set a transparent texture, and draw the triangles in the face
            // third, the scaling tool which implements a box around the entire primitive SHOULD use an external mesh
            // just like we do for manipulator tool.  This way all our proper picking works.
            // sketchup scale tool vid http://www.youtube.com/watch?v=I_xqUsZnzJA
            // however you know, looking at sketchup, a lot of the items are scaled by a factor ofdistance to camera (or ortho zoom value)and 
            // as if they are drawn in 2d on the near plane but perhaps it's just scaling... in fact
            // they are 3d because zdepth is preserved and thus some of the pickable "tabs" are occluded by closer geometry.
            // so definetly 3d.  This goes for everything including vertex handles
        }

        private void RenderDebug(EditDataStructures.EditableMesh mesh)
        {
            Collision.PickResults result = mesh.LastPickResult;
            // TODO: testing for Scenes[0] is  kind of hackish.  need a proper way to verify that the mesh who's debug info
            // is being rendered is currently mouseover.  I thinkthis shows us that maybe rendering deug here in MeshRenderer is wrong
            // and that it should be done from Scene.cs
            // the issue with that though is here we can replace the line or face textures, etc draws in the regular meshrender
            // if there is debug info that needs to be drawn with it.  Perhaps one way to do that is when setting MouseOverItem
            // we can there modify the LastPickResult.HaColided = false when the MouseOverItem value changes to null or a diff mesh
            // or better yet, add a .MouseOver property to the entity/mesh that we can check here
            //if (mesh == ((Scene.ClientScene)CoreClient._CoreClient.SceneManager.Scenes[0]).MouseOverItem.Geometry &&  mesh.LastPickResult.HasCollided) 
            //{
                // draw the selected polygon
                if (result.FaceID > -1)
                {
                    if (result.FacePoints != null)
                    {
                        // draw the face
                        // DebugDraw.Draw(new Polygon(result.FacePoints), CONST_TV_COLORKEY.TV_COLORKEY_GREEN);

                        // let's debug draw the closest edge to verify we've got it right
                        if (result.EdgeID > -1)
                        {
                            EditDataStructures.Edge e;
                            e = mesh._cell.GetEdge((uint) result.EdgeID);
                            Vector3d o, d;
                            o = mesh.GetVertex(e.Origin.ID, true);
                            d = mesh.GetVertex(e.Destination.ID, true);

                            //DebugDraw.DrawLine(o, d);
                        }

                        //   DrawNeighboringFaces(qeFace);
                    }
                }
            //}
        }

        //public void Render(EditDataStructures.EditableMesh mesh)
        //{
        //    // now set the state on the device for use
        //    mesh._defaultStateBlock.Capture();
        //    mesh._meshStateBlock.Apply();

        //    if (mesh.Indices == null) return;
        //    CustomVertex.PositionNormalTextured[]  transformedVerts = mesh.TransformedVertices;
           
        //    int[] indices = mesh.Indices;

        //    // is it faster to graphic stream + lock/unlock over SetData?  
        //    // according to the following url, the underlying codes are about the same
        //    // http://www.gamedev.net/community/forums/topic.asp?topic_id=346308
        //    GraphicsStream gs = _vertexBuffer.Lock(0, 0, 0);
        //    gs.Write(transformedVerts);
        //    _vertexBuffer.Unlock();
        //    //_vertexBuffer.SetData(transformedVerts, 0, 0);
        //    _indexBuffer.SetData(indices, 0, 0);
        //    _device.Indices = _indexBuffer;
        //    _device.SetStreamSource(0, _vertexBuffer, 0);


        //    int[] offsets = mesh.Offsets;
        //    int[] minimumVertexIndices = mesh.MinimumVertexIndices;
        //    int[] maximumVertexIndices = mesh.MaximumVertexIndices;
            
        //    DrawSolid(mesh.Groups.ToArray(), offsets,minimumVertexIndices , maximumVertexIndices);


        //    // setup and draw wireframe
        //    Microsoft.DirectX.Direct3D.Material tmpMat = new Microsoft.DirectX.Direct3D.Material();
        //    //tmpMat.Diffuse = System.Drawing.Color.Lavender;
        //    tmpMat.Ambient = System.Drawing.Color.Black;
        //    //// tmpMat.Specular = System.Drawing.Color.LightGray;
        //    //// tmpMat.SpecularSharpness = 15.0F;
        //    _device.Material = tmpMat;

        //    _device.SetRenderState(RenderStates.ZBufferWriteEnable, false);
        //    //             _device.VertexDeclaration = _vertexDeclaration;
        //    _device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
        //    _wireFrameVertexBuffer.SetData(transformedVerts, 0, 0);
        //    _device.SetStreamSource(0, _wireFrameVertexBuffer, 0);
        //    indices = mesh.WireIndices;
        //    _wireFrameIndexBuffer.SetData(indices, 0, 0);
        //    _device.Indices = _wireFrameIndexBuffer;


        //    offsets = mesh.WireOffsets;
        //    minimumVertexIndices = mesh.WireMinimumVertexIndices;
        //    maximumVertexIndices = mesh.WireMaximumVertexIndices;

        //    DrawWire(mesh.Groups.ToArray(), offsets, minimumVertexIndices, maximumVertexIndices);
        //    ////RenderModelerFrame(_transformedVertices, wireIndicesCount);
            
        //    // NOTE: We need seperate rollback stateblocks for each viewport
        //    // rollback the state by applying all the existing states we copied prior to setting up for rendering our imposters
        //    mesh._defaultStateBlock.Apply();
        //}
        
        ///// Shares a vertex and index buffer passed in from Draw traverser.  If the max vertex count is > than
        ///// the number of verts we have to render, then we must break up the render into batches.  Now actually
        ///// this transform code could maybe be moved into the Draw traverser as well
        //private void DrawSolid(VirtualGroup[] Groups, int[] offsets, int[] minimumVertexIndices, int[] maximumVertexIndices)
        //{
        //    // material
        //    Microsoft.DirectX.Direct3D.Material tmpNullMat = new Microsoft.DirectX.Direct3D.Material();

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

        //        int numVertices = maximumVertexIndices[i] - minimumVertexIndices[i];
        //        if (numVertices == 0) continue;
        //        int baseVertexIndex = 0;
        //        // BaseVertexIndex is a value that's effectively added to every VB Index stored in the index buffer. 
        //        // For example, if we had passed in a value of 50 for BaseVertexIndex during the previous call, 
        //        // that would functionally be the same as using the following index buffer for the duration of the DrawIndexedPrimitive call: 
        //        // The minVertexIndex is a hint to help DX optimzie memory usage when working with the vertex buffer so if you can tell it
        //        // in advance the position of the vertex in the vertex buffer that will be the lowest, that will help.. it could be set to 0 though
        //        // This value is rarely set to anything other than 0, but can be useful if you want to decouple the index buffer from the vertex buffer: If when filling in the index buffer for a particular mesh the location of the mesh within the vertex buffer isn't yet known, you can simply pretend the mesh vertices will be located at the start of the vertex buffer; when it comes time to make the draw call, simply pass the actual starting location as the BaseVertexIndex.
        //        // This technique can also be used when drawing multiple instances of a mesh using a single index buffer; for example, if the vertex buffer contained two meshes with identical draw order but slightly different vertices (perhaps different diffuse colors or texture coordinates), both meshes could be drawn by using different values for BaseVertexIndex. Taking this concept one step further, you could use one index buffer to draw multiple instances of a mesh, each contained in a different vertex buffer, simply by cycling which vertex buffer is active and adjusting the BaseVertexIndex as needed. Note that the BaseVertexIndex value is also automatically added to the MinIndex argument, which makes sense when you see how it's used: 

        //        // modify the material
        //        if (Groups[i].Material != null)
        //        {
        //            // TODO: would comparing lastMaterial help at all?
        //            _device.Material = Groups[i].Material;
        //        }
        //        else
        //        {
        //            _device.Material = tmpNullMat;
        //        }

        //        // startIndex is obviously the starting index in the indices for a group
        //        _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertexIndex, minimumVertexIndices[i], numVertices, startIndex, triangleCount);
        //    }    
        //}

        //private void DrawWire(VirtualGroup[] Groups, int[] offsets, int[] minimumVertexIndices, int[] maximumVertexIndices)
        //{

        //    for (int i = 0; i < offsets.Length; i++)
        //    {
        //        int nextStartIndex = offsets[i];
        //        int startIndex;
        //        if (i == 0)
        //            startIndex = 0;
        //        else
        //            startIndex = offsets[i - 1];

        //        int lineCount = (nextStartIndex - startIndex) / 2;
        //        System.Diagnostics.Trace.Assert(lineCount * 2 == nextStartIndex - startIndex); // verify a whole non fractional number of lines

        //        int numVertices = maximumVertexIndices[i] - minimumVertexIndices[i];
        //        if (numVertices == 0) continue;
        //        int baseVertexIndex = 0;
        //        // BaseVertexIndex is a value that's effectively added to every VB Index stored in the index buffer. 
        //        // For example, if we had passed in a value of 50 for BaseVertexIndex during the previous call, 
        //        // that would functionally be the same as using the following index buffer for the duration of the DrawIndexedPrimitive call: 
        //        // The minVertexIndex is a hint to help DX optimzie memory usage when working with the vertex buffer so if you can tell it
        //        // in advance the position of the vertex in the vertex buffer that will be the lowest, that will help.. it could be set to 0 though
        //        // This value is rarely set to anything other than 0, but can be useful if you want to decouple the index buffer from the vertex buffer: If when filling in the index buffer for a particular mesh the location of the mesh within the vertex buffer isn't yet known, you can simply pretend the mesh vertices will be located at the start of the vertex buffer; when it comes time to make the draw call, simply pass the actual starting location as the BaseVertexIndex.
        //        // This technique can also be used when drawing multiple instances of a mesh using a single index buffer; for example, if the vertex buffer contained two meshes with identical draw order but slightly different vertices (perhaps different diffuse colors or texture coordinates), both meshes could be drawn by using different values for BaseVertexIndex. Taking this concept one step further, you could use one index buffer to draw multiple instances of a mesh, each contained in a different vertex buffer, simply by cycling which vertex buffer is active and adjusting the BaseVertexIndex as needed. Note that the BaseVertexIndex value is also automatically added to the MinIndex argument, which makes sense when you see how it's used: 

        //        // startIndex is obviously the starting index in the indices for a group
        //        // one question is though, when rendering we need to exclude those lines (if non transparent mode is enabled) for faces that are not visible
        //        // maybe we can ignore for now
        //        _device.DrawIndexedPrimitives(PrimitiveType.LineList, baseVertexIndex, minimumVertexIndices[i], numVertices, startIndex, lineCount);
        //    }
        //}


        private Line _lineDrawer;
        /// <summary>
        /// Uses LineDraw
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indicesCount"></param>
        //private void RenderModelerFrameWithLineDraw(CustomVertex.PositionNormalTextured[] vertices, int indicesCount)
        //{
        //    // LineDraw expects that the vertices are already transformed to screenspace or that such a transform is proivded in the method  call
        //    Microsoft.DirectX.Matrix transform = (Microsoft.DirectX.Matrix.Multiply(_device.GetTransform(TransformType.View), _device.GetTransform(TransformType.Projection)));

        //    _device.RenderState.ZBufferEnable = true;
        //    _device.SetRenderState(RenderStates.ZBufferWriteEnable, false);
        //                // i believe that lines need to have references to the faces they adjoin.  I see no way around this for efficiency and
        //    // for future ability to modify these models in realtime.  The question though is, since WaveFrontObjMesh doesnt really
        //    // need this for loading, its really our internal modeling format we're talking about that needs to know this stuff
        //    // I believe that's something that should occur after we've loaded and before we would typically convert to TVMesh.
        //    // So this MeshWaveFrontObj  actually could be that object...

        //    // we need to create new offsets and 
        //    // indices list using the regular Points and not the TriangulatedPoints
        //    // Now that've set the vertex buffer and the vertex format into the device, we should itterate thru all faces, grabbing the index buffer
        //    // and setting that and  making a new DrawPrimitives call for each GROUP
        //    int offset = 0;
        //    int[] indices = new int[indicesCount];
        //    int[] offsets = new int[Groups.Count];
        //    int[] minimumVertexIndices = new int[Groups.Count];
        //    int[] maximumVertexIndices = new int[Groups.Count];

        //    Vector3[] vertexList = new Vector3 [2];

        //    _lineDrawer.Begin();

        //    // build the indices array
        //    for (int i = 0; i < Groups.Count; i++)
        //    {
        //        if (Groups[i].Faces == null) continue;
        //        minimumVertexIndices[i] = int.MaxValue;
        //        maximumVertexIndices[i] = 0;
        //        for (int j = 0; j < Groups[i].Faces.Count; j++)
        //        {
        //            // check for backfacing faces which we potentically skip if this option is enabled
        //            Types.Vector3f normal; // = Groups[i].Faces[j].GetFaceNormal();
        //            Types.Vector3f a = new Vector3f(vertices[Groups[i].Faces[j].Points[0].Index].X,vertices[Groups[i].Faces[j].Points[0].Index].Y, vertices[Groups[i].Faces[j].Points[0].Index].Z );
        //            Types.Vector3f b = new Vector3f(vertices[Groups[i].Faces[j].Points[1].Index].X, vertices[Groups[i].Faces[j].Points[1].Index].Y, vertices[Groups[i].Faces[j].Points[1].Index].Z);
        //            Types.Vector3f c = new Vector3f(vertices[Groups[i].Faces[j].Points[2].Index].X, vertices[Groups[i].Faces[j].Points[2].Index].Y, vertices[Groups[i].Faces[j].Points[2].Index].Z);
        //            Types.Vector3f v1 = a - b;
        //            Types.Vector3f v2 = b - c;
        //            normal =  Types.Vector3f.Normalize(Types.Vector3f.CrossProduct(v1, v2));

        //            Types.Vector3f dir = new Vector3f(vertices[Groups[i].Faces[j].Points[0].Index].X, vertices[Groups[i].Faces[j].Points[0].Index].Y, vertices[Groups[i].Faces[j].Points[0].Index].Z);

        //            if (Types.Vector3f.DotProduct ( dir , normal) < 0)
        //            {
        //                vertexList = new Vector3[Groups[i].Faces[j].Points.Count + 1];
        //                for (int k = 0; k < Groups[i].Faces[j].Points.Count; k++)
        //                {
        //                    int index = Groups[i].Faces[j].Points[k].Index;
        //                    // since we're not dealing with triangulated indexed points, we 
        //                    // must manually create indexed line list here on the fly by adding two points
        //                    // every itteration.  We do this because LineList allows us to use a single drawprimitive call
        //                    // rather than use LineStrip which will require fewer verts but more calls as we have to break between faces
        //                    // to avoid LineStrip's making unwanted diagonal connections between end of one face and start of next

        //                    vertexList[k].X = vertices[index].X;
        //                    vertexList[k].Y = vertices[index].Y;
        //                    vertexList[k].Z = vertices[index].Z;

        //                    if (k == Groups[i].Faces[j].Points.Count - 1)
        //                    {
        //                        // connect the face back to the beginning
        //                        indices[offset + k * 2] = index;
        //                        indices[offset + k*2 + 1] = Groups[i].Faces[j].Points[0].Index;

        //                        vertexList[k + 1].X = vertices[Groups[i].Faces[j].Points[0].Index].X;
        //                        vertexList[k + 1].Y = vertices[Groups[i].Faces[j].Points[0].Index].Y;
        //                        vertexList[k + 1].Z = vertices[Groups[i].Faces[j].Points[0].Index].Z;
        //                    }
        //                    else
        //                    {
        //                        indices[offset + k * 2] = index;
        //                        indices[offset + k * 2 + 1] = Groups[i].Faces[j].Points[k + 1].Index;
        //                    }
        //                    // track the minimum index value into the vertex buffer for this group
        //                    if (minimumVertexIndices[i] > index)
        //                        minimumVertexIndices[i] = index;
        //                    if (maximumVertexIndices[i] < index)
        //                        maximumVertexIndices[i] = index;
        //                }
        //                _lineDrawer.DrawTransform(vertexList, transform, System.Drawing.Color.Black.ToArgb());
        //                offset += Groups[i].Faces[j].Points.Count * 2;
        //            }
        //        }
        //        offsets[i] = offset; // this number tells us how many vertices are in this group
        //    }

        //    _lineDrawer.End();
        //}


        #region IDisposable Members
        public void Dispose()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _vertexBuffer = null;
            if (_indexBuffer != null) _indexBuffer.Dispose();
            _indexBuffer = null;

            if (_wireFrameVertexBuffer != null) _wireFrameVertexBuffer.Dispose();
            _wireFrameVertexBuffer = null;
            if (_wireFrameIndexBuffer != null) _wireFrameIndexBuffer.Dispose();
            _wireFrameIndexBuffer = null;
        }
        #endregion
    }
}
