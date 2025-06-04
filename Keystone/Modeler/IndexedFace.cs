using System;
using System.Collections.Generic;
using Keystone.EditDataStructures;

namespace Keystone.Modeler
{
    /// <summary>
    /// Unlike WaveFrontObjIndexed face which has seperate index arrays for coordinates, normals and texture coords
    /// this IndexedFaceSet for modeling hosts indices to just Points and UVs.  Normals are computed (TODO: ask a real modeler
    /// if this is appropriate.  From the ASE (3ds max ascii format) it seems max does the same thing).  Normals should only be exported.
    /// </summary>
    public class IndexedFace
    {
        internal EditableMesh _parentMesh;

        public List<uint> Points;
        public List<uint> UVs;
        public uint[] TriangulatedPoints;  // triangulated points always reference existing real points
        public uint[] TriangulatedUVs; 
        private Types.Vector3f _faceNormal;
        public int Group = -1;
        public int SmoothingGroup = -1;

        private bool _isDirty = true;
        
        public void Add(uint coordIndex)
        {
            if (Points == null) Points = new List<uint>();
            Points.Add(coordIndex);
            _isDirty = true;
            if (UVs != null) throw new Exception("Either no UVs can be used, or all Points must have UVs.");
        }

        public void Add (uint coordIndex, uint uvIndex)
        {
            if (Points == null) Points = new List<uint>();
            Points.Add(coordIndex);
            if (UVs == null) UVs = new List<uint>();
            UVs.Add(uvIndex);
            _isDirty = true;
            if (UVs.Count != Points.Count) throw new Exception("Point indices length must equal UVs indices length");
        }

        public Types.Vector3f GetFaceNormal()
        {
            if (_isDirty)
            {
                Types.Vector3f v1 = VectorFromIndex(Points[0]) - VectorFromIndex(Points[1]);
                Types.Vector3f v2 = VectorFromIndex(Points[1]) - VectorFromIndex(Points[2]);
                _faceNormal = Types.Vector3f.Normalize(Types.Vector3f.CrossProduct(v1, v2));
                _isDirty = false;
            }
            return _faceNormal;
        }

        private Types.Vector3f VectorFromIndex(uint index)
        {
            return new Keystone.Types.Vector3f(_parentMesh._cell.Vertices[(int)index].Position[0], _parentMesh._cell.Vertices[(int)index].Position[1],_parentMesh._cell.Vertices[(int)index].Position[2]);
        }

        /// <summary>
        /// Triangulation is required for rendering d3d meshes which only uses triangle faces
        /// however, we also do want to be able to render >3 vertices faces when editing so 
        /// we can see the faces properly.  Thus, whenever a new vertex is added/removed
        /// we need to re-triangulate.  If a vertex is just moved without being merged, we don't
        /// have to do anything since vertices are referenced from a single list for both the triangulated faces
        /// and the polygonal faces.
        /// This algorithm works on any convex polygon.  Concave polygons will surely result in artifacts.
        /// </summary>
        public void Triangulate()
        {
            if (Points.Count < 3) throw new Exception("Not a polygon");

            if (Points.Count == 3)
            {
                TriangulatedPoints = Points.ToArray ();
                return; // nothing to triangulate
            }

            int faceCount = Points.Count - 2;
            uint[] newVerts = new uint[faceCount * 3];

            uint[] newUVs = null;
            if (UVs != null)
                newUVs = new uint[faceCount * 3];

            int j = 0;
            int k = 0;
            // triangles using fan method
            for (int i = 0; i < faceCount; i++)
            {
                newVerts[j++] = Points[0];
                newVerts[j++] = Points[i + 1];
                newVerts[j++] = Points[i + 2];
                if (newUVs != null)
                {
                    newUVs[k++] = UVs[0];
                    newUVs[k++] = UVs[i + 1];
                    newUVs[k++] = UVs[i + 2];
                }
            }

            TriangulatedPoints = newVerts;
            TriangulatedUVs = newUVs;
        }
    }
}
