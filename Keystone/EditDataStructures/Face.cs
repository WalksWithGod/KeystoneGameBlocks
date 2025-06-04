using System;
using Keystone.Types;

namespace Keystone.EditDataStructures
{
    public sealed class Face
    {
        private Cell mCell;
        private Edge mEdge;
        private uint mID;
        public object mData;

        // cached face data 
        private Edge[] mEdges;
        private Face[] mNeighbors;
        private uint[] mVertices;
        private Types.Vector3d[] mVertexCoordinates;
        private uint[] mTriangulatedVertexCoordinates; // these index into the mVertexCoordinates 
        private Types.Vector3f[] mNormals;
        private Types.Vector3f mFaceNormal;
        private Types.Vector2f[] mUVs;
        private bool mIsClosed; // returns if Edge.Length >=3 and the last edge's destination == first edges origin
        private bool mChanged;
        private int mGroupID;


        private Face(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();

            this.mCell = cell;
            this.mID = cell.MakeFaceID();
            this.mData = null;
            this.mEdge = null;
            this.mChanged = true;

            mCell.AddFace(this);
        }

        ~Face()
        {
            mCell.RemoveFace(this);
        }

        public Edge Edge
        {
            get { return mEdge; }
        }

        public static Face Make(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();

            return new Face(cell);
        }

        public static void Kill(Face face)
        {
            if (face == null) throw new ArgumentNullException();

            //face.Dispose();
            face = null;
            //delete face;
        }

        public uint ID
        {
            get { return mID; }
            set { mID = value; }
        }


        public void AddEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();
            // only keep track of one edge in the orbit--this one is as good as any
            this.mEdge = edge;
            mChanged = true;
        }

        public void RemoveEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();

            // replace the arbitrary edge with another edge in the orbit
            // use null if this is the only edge
            // assumes that the edge hasn't been actually removed yet

            Edge next = edge.ONext;

            this.mEdge = next != edge ? next : null;
            mChanged = true;
        }

        public Face[] Neighbors
        {
            get
            {
                // Neighbors can't /shouldnt be cached because there is no simple way to be notified if a neighbor has been changed/added/removed
                // is the above even true?  We do have an AddEdge/RemoveEdge so surely we're notified?  Will have to test i think... to verify
                // that all edge adds/removes are indeed resulting in the above methods being called.  For now we'll just recompute every time
                System.Collections.Generic.List<Face> list = new System.Collections.Generic.List<Face>();
                FaceEdgeIterator faceEdges = new FaceEdgeIterator(this);
                Edge edge;
                while ((edge = faceEdges.Next()) != null)
                    list.Add(edge.Right);

                return list.ToArray();
            }
        }

        public uint[] Vertices
        {
            get
            {
                if (mChanged)
                {
                    System.Collections.Generic.List<uint> list = new System.Collections.Generic.List<uint>();

                    mEdges = Edges;

                    for (int i = 0; i < mEdges.Length; i++)
                    {
                        list.Add(mEdges[i].Origin.ID);
                    }
                    mVertices = list.ToArray();
                    //    mChanged = false;
                }
                return mVertices;
            }
        }

        public Types.Vector3d[] VertexCoordinates
        {
            get
            {
                if (mChanged)
                {
                    System.Collections.Generic.List<Types.Vector3d> list = new System.Collections.Generic.List<Types.Vector3d>();

                    mEdges = Edges;

                    for (int i = 0; i < mEdges.Length ; i++)
                    {
                        Types.Vector3d vec;
                        vec.x = mEdges[i].Origin.Position[0];
                        vec.y = mEdges[i].Origin.Position[1];
                        vec.z = mEdges[i].Origin.Position[2];
                        list.Add(vec);
                    }
                    mVertexCoordinates = list.ToArray();
               //    mChanged = false;
                }
                return mVertexCoordinates;
            }
        }

        public Vector3d [] TriangulatedVertexCoordinates
        {
            get
            {
                if (mChanged )
                {
                    // recalc the triangulated verts
                }
                Vector3d[] verts = new Vector3d[mTriangulatedVertexCoordinates.Length];
                for (int i = 0; i < mTriangulatedVertexCoordinates.Length ; i++)
                    verts[i] = mVertexCoordinates[mTriangulatedVertexCoordinates[i]];

                return verts;
            }
        }

        public Edge[] Edges
        {
            get
            {
                if (mChanged)
                {

                    System.Collections.Generic.List<Edge> list = new System.Collections.Generic.List<Edge>();
                    FaceEdgeIterator faceEdges = new FaceEdgeIterator(this);
                    Edge edge;
                    while ((edge = faceEdges.Next()) != null)
                        list.Add(edge);

                    mEdges = list.ToArray();
              //      mChanged = false;
                }

                return mEdges;
            }
        }

        public Types.Vector3f FaceNormal
        {
            get
            {
                // TODO: if we load from .obj then we do have to reverse the vertex points, but we should never have to reverse the normal
                // because that deals with POST reversed vertex points
                if (mChanged)
                {
                    // compute a normal to the face
                    // (we could precompute these and store them with the faces)
                    FaceEdgeIterator edges = new FaceEdgeIterator(this);
                    Edge e1 = edges.Next();
                    Edge e2 = edges.Next();
                    Types.Vector3f v1;
                    v1.x = e1.Origin.Position[0];
                    v1.y = e1.Origin.Position[1];
                    v1.z = e1.Origin.Position[2];
                    Types.Vector3f v2 ;
                    v2.x = e1.Destination.Position[0]; // ==e2.Org().pos
                    v2.y = e1.Destination.Position[1]; // ==e2.Org().pos
                    v2.z = e1.Destination.Position[2]; // ==e2.Org().pos

                    Types.Vector3f v3;
                    v3.x = e2.Destination.Position[0];
                    v3.y = e2.Destination.Position[1];
                    v3.z = e2.Destination.Position[2];
                    // v1, v2, v3 should be in CW
                    mFaceNormal = Types.Vector3f.Normalize(Types.Vector3f.CrossProduct(v2 - v1, v3 - v2));
                }
                return mFaceNormal;
            }
       }

            public override string ToString()
        {
            string s = "Face " + ID;
            s += mEdge.ToString();
            return s;
        }
    }
}