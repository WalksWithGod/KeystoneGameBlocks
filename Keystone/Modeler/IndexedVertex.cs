
// OBSOLETE AFTER IMPLEMENTING QUADEDGE + DrawPrimitives instead of DrawIndexedPrimtives for EditableMesh
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Keystone.Modeler
//{
//    /// <summary>
//    /// </summary>
//    public class IndexedVertex
//    {
//        // TODO: for adjacency purposes, shouldnt each of these also contain a reference to which face it belongs to so that
//        // when editing, we know which ones to delete (potentially) from the _virtualVertices list?
//        public int Index;    // index into actual vertices array used for rendering triangulated solid
//        public int Coord;    // index into coordinate array
//        public int Normal;  // index into normal array
//        public int UV;        // index into UV array

//        public IndexedVertex(int index)
//        {
//            Index = index;
//            Coord = -1;
//            Normal = -1;
//            UV = -1;
//        }
//    }
//}
