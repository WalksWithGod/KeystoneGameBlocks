using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keystone.Elements;

namespace Keystone.Modeler
{
    internal class AdjacencyInfo
    {
        private struct Edge
        {
            int P0;
            int P1;
            int Triangle1; // an edge that is not fully connected into a closed face can exist with -1 for both triangle1 and 2
            int Triangle2; 
        }

        private struct AdjacencyTable
        {
            public int[] Neigbors;
        }

        List<Edge> _edges;

        // our key is an int that tells us the index of this coordinate point in the overall coordinate list
        // I do not think this requires that the vertex itself even know its own index because what we will
        // be doing is traversing the index on "pick" traversals and such and thus know exactly which point we're near when we reach it
        Dictionary<int, AdjacencyTable> _coordinatesAdjacency; 

        // http://www.gamedev.net/community/forums/topic.asp?topic_id=320848
        //one thing that may put a hamper into your code: a fixed vertex in space may have several different texture co-ordinates, so the vertex gets duplicated... now the MD2 format itself does not do this (since the "glcommands" part of the md2 file gives for each triangle strip/triangle fan a texture co-ordinate and a vertex index) but if you duplicating the vertexes tht have multiple texture co-ordinates (so you can use vertex arrays/VBOs) then you have to calculate your edges BEFORE you do this translation.... it also may be a good idea to check if the vertexes stored in the MD2 file are really unique, if not then you need to handle "merging" these same vertexes together.

        // to find which triangles share a point, you find all edges that share that point
        // so points could use an adjacency list of all edges that share it
        // 
        
        // a triangle needs to know
        // - ref to edges it has
        // - ref to triangles that share it's edges... this is something that can be computed at run time just by knowing the edges
        Dictionary<int, AdjacencyTable> _triangleAdjacency;


        // an edge needs to know
        // - ref to both points
        // - ref to triangles that share this edge
        Dictionary<int, AdjacencyTable> _edgeAdjacency;
       
        Loaders.WaveFrontObj _mesh; 

        internal AdjacencyInfo (Loaders.WaveFrontObj mesh)
        {
            if (mesh == null) throw new ArgumentNullException ();

            _mesh = mesh;
            _edges = new List<Edge>();
            _coordinatesAdjacency = new Dictionary<int,AdjacencyTable> ();
            _edgeAdjacency = new Dictionary<int,AdjacencyTable> ();
            _triangleAdjacency = new Dictionary<int,AdjacencyTable> ();

            GenerateAdjacencyInfo();
        }

        // TODO: I believe ultimately i also need a "Shape" class that is a
        // 3d version of a 2d object.  Specifically it is an extruded polygon that has mirrored
        // vertices at face normal angle and just seperated by a given distance.  Without that
        // there'd be no proper way to track mirrored faces.  Now actually this isnt needed for extrusion itself
        // but it is needed i think in order to be able to cut holes through 3d polygons (shapes)  With a simple
        // rule such as "You can only use CSG (constructive solid geometry boolean ops) against SHAPES which are always
        // 2d polygons with depth (mirrored faces) then doing things like creating holes and such is simple.  
        // ACTUALLY, i just verified that in sketchup, it allows multiple extrusions within extrusions so this isnt exactly correct.
        // Maybe shapes isnt needed and its just doing the planar tests...verifying that intersections are only occurring within a single
        // polygon and not crossing
        private void GenerateAdjacencyInfo()
        {
       //     // -1 means "no triangle shares this edge"
       //     AdjacencyInfo   empty = { {-1,-1,-1} };
       //     AdjacencyTable  table;

       //       // Resize and fill the table with dummy adjacency information.
       ////       table.resize(source.GetTriangleCount());
       ////       std::fill(table.begin(),table.end(),empty);
              
       //       // For each face...
       //       for(int i = 0; i < _mesh.Faces; ++i) // TODO: for now we'll just deal with untriangulated face points
       //       {
       //            Loaders.WaveFrontObjIndexedFace face =  _mesh.Faces[i];
       //            _triangleAdjacency.Add (i, new AdjacencyTable ());

       //            //now so far our face doesnt actually maintain a list of edges, however
       //           // those edges should be index based and that means we can null any unused index with -1 
       //           // how this translates into updating the vertex buffer itself im not 100% sure.  
       //           // Our edges could be generated during load of the mesh, but my question now is 
       //           // whether they should be loaded by the wavefront obj?  

       //           // TODO: edges dont currently exist in our faces...  when we load a wavefront obj, should we generate
       //           // a global edge list and triangle list?  My initial thought is that if we were to import .X or some other
       //           // format for real time editing, could we still use most of the WaveFrontObj classes such as IndexedFaceSet and such
       //           // and just replace the parser 

       //         // For each edge...
       //         for(int j = 0; j < face.Points.Length; ++j)
       //         { 
       //             int  p0 = face.Points[j];
       //             int  p1 = face.Points[j + 1];
                
       //             int result = FindNeighboringFaceOnEdge(p1,p0,source);
       //             _triangleAdjacency[i].Neigbors[j] = result;
       //         }
       //       }
        }

        /// <summary>
        /// Returns triangles that share edges with the triangle who's index was passed in
        /// </summary>
        /// <param name="triangleIndex"></param>
        /// <returns></returns>
        internal Types.Triangle FindTriangles (int triangleIndex)
        {
            Types.Vector3d dummy = new Types.Vector3d();
            return new Keystone.Types.Triangle(dummy, dummy, dummy);
        }


        /// <summary>
        /// Returns the edges that share the coordinate who's index was passed in
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        internal Types.Line3d FindEdges (int pointIndex)
        {
            return new Keystone.Types.Line3d ();
        }


        /// <summary>
        /// Itterates through all faces and returns the index of another face that shares the same edge.  The below code
        /// assumes a properly constructed model where an edge can only be shared by two faces.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        int FindNeighboringFaceOnEdge(int edgePoint0, int edgePoint1, int srcFace, Loaders.WaveFrontObj mesh)
        {

          //for(int i = 0; i < mesh.Faces.Count; ++i)
          //{
          //    if (i == srcTriangle ) continue;

          //    Loaders.WaveFrontObjIndexedFace face  = mesh.Faces[i]; 
          
          //    for (int j = 0; j < face.Points.Length ; j++)
          //    {
          //        int v0 = face.Points[j];
          //        if (v0 != edgePoint0 ) continue;

          //        int  v1 = face.Points[j+1];
                
          //        if (v1 == edgePoint1)
          //          return i;
          //      }
          //}
          
          // Not found.
          return (-1);
        }
    }
}
