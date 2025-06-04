using System;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Loaders
{
    public class WaveFrontObjIndexedFace
    {
        public uint Index;

        // single slash is standard seperator for face data
        private const string ID_SINGLE_SLASH = "/";
        // double slash used when there is only vertex and normal data and not UV data. Normally two elements seperated by one slash means its vertex and UV data only
        private const string ID_DOUBLE_SLASH = "//";
        private WaveFrontObjSmoothingGroup _smoothingGroup;
        private bool _reverseWinding = true;  // note: this should be set to true for my editor picking to work properly. Most .obj files imported use gl winding so if those .obj are per spec, reversewingind = true is correct.  otherwise the file is not to spec and it's not really our problem.  So when using any .obj exporter/convertor program, DO NOT  have it automatically reverse the windings on export
        private WaveFrontObj _wavefrontObj;
        
        // TODO: i think that this indexed face should ideally share array with the overal Group so that this face can be more easily rendered in a single call
        public uint[] Points;
        public uint[] TriangulatedPoints;  
        public uint[] Textures;
        public uint[] Normals;
        
        private WaveFrontObjIndexedFace(WaveFrontObj parentObj)
        {
            if (parentObj == null) throw new ArgumentNullException();
            _wavefrontObj = parentObj;
        }

        public WaveFrontObjIndexedFace(WaveFrontObj parentObj, uint a, uint b, uint c)
            : this(parentObj)
        {
            Points = new uint[3] { a, b, c };
            Textures = null;
            Normals = null;
        }

        public WaveFrontObjIndexedFace(WaveFrontObj parentObj, uint a, uint b, uint c, uint t1, uint t2, uint t3, bool treatAsTextures)
            : this(parentObj)
        {
            Points = new uint[3] { a, b, c };
            if (treatAsTextures)
            {
                Textures = new uint[3] { t1, t2, t3 };
                Normals = null;
            }
            else
            {
                Normals = new uint[3] { t1, t2, t3 };
                Textures = null;
            }
        }

        public WaveFrontObjIndexedFace(WaveFrontObj parentObj, uint a, uint b, uint c, uint t1, uint t2, uint t3, uint n1, uint n2, uint n3) 
            : this(parentObj)
        {
            Points = new uint[3] { a, b, c };
            Textures = new uint[3] { t1, t2, t3 };
            Normals = new uint[3] { n1, n2, n3 };
        }

        public WaveFrontObjIndexedFace(WaveFrontObj parentObj, string[] tokens)
            : this(parentObj)
        {
            if (_reverseWinding) // force a reversal of the winding orders here
            {
                string[] tmp = new string[tokens.Length];
                tokens.CopyTo(tmp, 0);

                for (int i = 1; i < tokens.Length; i++)
                    tmp[i] = tokens[tokens.Length - i];

                tokens = tmp;
            }

            bool containsNormalsOnly = tokens[1].Contains(ID_DOUBLE_SLASH);
            // if the face data contains at least 1 slash in each token, then there is also a UV index
            bool containsUV = false;
            if (!containsNormalsOnly) containsUV = tokens[1].Contains(ID_SINGLE_SLASH);
            // if the face data contains 2 seperate single slashes in each token, there are both UV and Normal data
            bool containsUVAndNormals = false;
            if (!containsNormalsOnly && containsUV)
            {
                int index = tokens[1].IndexOf(ID_SINGLE_SLASH);
                string substring = tokens[1].Substring(index + 1);
                containsUVAndNormals = substring.Contains(ID_SINGLE_SLASH);
            }

            Points = new uint[tokens.Length - 1];

            if (containsUVAndNormals) // contains vertices, UV's and normals
            {
                Textures = new uint[tokens.Length - 1];
                Normals = new uint[tokens.Length - 1];

                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] inner = tokens[i].Split('/');
                    System.Diagnostics.Trace.Assert(inner.Length == 3);
                    Points[i - 1] = RelativeCoordToAbsolute(int.Parse(inner[0]));
                    Textures[i - 1] = RelativeUVToAbsolute(int.Parse(inner[1]));
                    Normals[i - 1] = RelativeNormalToAbsolute(int.Parse(inner[2]));
                }
            }
            else if (containsUV || containsNormalsOnly) // contains either UV's or Normals.  
            {
                string[] delim;
                if (containsUV)
                {
                    delim = new string[1] { ID_SINGLE_SLASH };
                    Textures = new uint[tokens.Length - 1];
                }
                else
                {
                    delim = new string[1] { ID_DOUBLE_SLASH };
                    Normals = new uint[tokens.Length - 1];
                }
                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] inner = tokens[i].Split(delim, StringSplitOptions.None);
                    System.Diagnostics.Trace.Assert(inner.Length == 2);
                    Points[i - 1] = RelativeCoordToAbsolute(int.Parse(inner[0]));
                    if (containsUV)
                        Textures[i - 1] = RelativeUVToAbsolute(int.Parse(inner[1]));
                    else
                        Normals[i - 1] = RelativeNormalToAbsolute(int.Parse(inner[1]));
                }
            }
            else // contains just vertices only
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    Points[i - 1] = RelativeCoordToAbsolute(int.Parse(tokens[i]));
                }
            }
        }

        public TV_3DVECTOR GetFaceNormal()
        {
            TV_3DVECTOR v1 = VectorFromIndex(Points[0]) - VectorFromIndex(Points[1]);
            TV_3DVECTOR v2 = VectorFromIndex(Points[1]) - VectorFromIndex(Points[2]);
            return CoreClient._CoreClient.Maths.VNormalize(CoreClient._CoreClient.Maths.VCrossProduct(v1, v2));
        }

        public bool ContainsVertex(uint index)
        {
            foreach (uint i in Points )
                if (i == index) return true;

            return false;
        }

        private TV_3DVECTOR VectorFromIndex(uint index)
        {
            return _wavefrontObj.Points[(int) index];
        }

        public void Draw (Matrix transform, CONST_TV_COLORKEY color)
        {
            
        }

        public WaveFrontObjSmoothingGroup  SmoothingGroup { get { return _smoothingGroup; } set { _smoothingGroup = value; } }
        
        // these points need to be transformed by the current matrix 
        public void Draw(CONST_TV_COLORKEY color)
        {
            //Line3d[] lines = new Line3d[ Points.Length];
           
            //for (int i = 0; i < Points.Length - 1; i++ )
            //{
            //    lines[i] = new Line3d(VectorFromIndex(Points[i]), VectorFromIndex(Points[i + 1]));
            //}
            //// last line is wrapped around back to first vertex
            //lines[Points.Length - 1] = new Line3d(VectorFromIndex(Points[Points.Length -1]), VectorFromIndex(Points[0]));
            //DebugDraw.DrawLines(lines, color);
        }
        public void ReverseWinding()
        {
            
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
            if (Points.Length < 3) throw new Exception("Not a polygon");

            if (Points.Length == 3) 
            {
                TriangulatedPoints = Points;
                return; // nothing to triangulate
            }

            int numIterations = Points.Length - 2;
            uint[] newNormals = null;
            uint[] newTextures = null;
            uint[] newCoords = new uint[numIterations * 3];
            if (Normals != null) newNormals = new uint[numIterations * 3];
            if (Textures != null) newTextures = new uint[numIterations * 3];

            int j = 0;
            // triangles using fan method
            for (int i = 0; i < numIterations; i++)
            {
                newCoords[j++] = Points[0];
                newCoords[j++] = Points[i + 1];
                newCoords[j++] = Points[i + 2];

                if (Normals != null)
                {
                    j -= 3;
                    newNormals[j++] = Normals[0];
                    newNormals[j++] = Normals[i + 1];
                    newNormals[j++] = Normals[i + 2];
                }
                if (Textures != null)
                {
                    j -= 3;
                    newTextures[j++] = Textures[0];
                    newTextures[j++] = Textures[i + 1];
                    newTextures[j++] = Textures[i + 2];
                }
            }

            TriangulatedPoints  = newCoords;
            if (Normals != null) Normals = newNormals;
            if (Textures != null) Textures = newTextures;
        }

        
        private uint RelativeCoordToAbsolute(int index)
        {
            if (index > 0) return (uint)(--index); // obj uses base 1 indices.  we switch to base 0

            int length = _wavefrontObj.Points.Count;
            System.Diagnostics.Trace.Assert(length + index >= 0);
            return (uint)(length + index); // the result is already rebased to 0;
        }

        private uint RelativeNormalToAbsolute(int index)
        {
            if (index > 0) return (uint)(--index); // obj uses base 1 indices.  we switch to base 0

            int length = _wavefrontObj.Normals.Count;
            System.Diagnostics.Trace.Assert(length + index >= 0);
            return (uint)(length + index); // the result is already rebased to 0;
        }

        private uint RelativeUVToAbsolute(int index)
        {
            if (index > 0) return (uint)(--index); // obj uses base 1 indices.  we switch to base 0

            int length = _wavefrontObj.UVs.Count;
            System.Diagnostics.Trace.Assert(length + index >= 0);
            return (uint)(length + index); // the result is already rebased to 0;
        }
    }
}
