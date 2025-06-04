using System;

namespace Keystone.EditDataStructures
{
    /*
 * A directed edge from one vertex to another, adjacent to two faces.
 * Based on Dani Lischinski's code from Graphics Gems IV.
 * Original quad-edge data structure due to Guibas and Stolfi (1985).
 *
 * ID     the ID number assigned to the edge;
 *        positive
 * data   generic data attached to the edge by the client
 * Org    the vertex of origin for the edge;
 *        null if currently unknown
 * Dest   the destination vertex for the edge;
 *        null if currently unknown
 * Left   the left face of the edge;
 *        null if currently unknown
 * Right  the right face of the edge;
 *        null if currently unknown
 */

    /// <summary>
    /// A directed edge from an "origin" vertex to a "Destination" vertex, adjacent to two faces "Left" and "Right."
    /// Paul Heckbert's code from Carnegie Mellon University which in turn was based on Dani Lischinski's code from Graphics Gems IV. 
    /// Original quad-edge data structure due to Guibas and Stolfi (1985).
    /// </summary>
    public sealed class Edge : IDisposable
    {
        private byte mFlags;

        private object mData;

        private uint mID;
        private uint mIndex;
        private Vertex mOrigin;
        private Face mFace;
        private Edge mNext;
        private Cell mCell;

        private static uint mNextID = 0;
        private Edge[] mEdges;

        private Edge(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();
            mCell = cell;
            mCell.AddEdge(this);
        }

        ~Edge()
        {
            Dispose(false);
        }


        public static Edge Make(Cell cell)
        {
         
            Edge e = new Edge(cell);
            e.mEdges = new Edge[4];

            e.mEdges[0] = new Edge(cell); // directed edge
            e.mEdges[1] = new Edge(cell); // Rot
            e.mEdges[2] = new Edge(cell); // Sym
            e.mEdges[3] = new Edge(cell); // InvRot

            e.mEdges[0].Index = 0;
            e.mEdges[1].Index = 1;
            e.mEdges[2].Index = 2;
            e.mEdges[3].Index = 3;

            e.mEdges[0].ONext = e.mEdges[0]; // directed edge is itself in a closed loop
            e.mEdges[1].ONext = e.mEdges[3];
            e.mEdges[2].ONext = e.mEdges[2]; // sym's next is also itself
            e.mEdges[3].ONext = e.mEdges[1];

            uint id = Edge.mNextID;
            e.mEdges[0].ID = id + 0;
            e.mEdges[1].ID = id + 1;
            e.mEdges[2].ID = id + 2;
            e.mEdges[3].ID = id + 3;

            e.mEdges[0].mEdges = e.mEdges;
            e.mEdges[1].mEdges = e.mEdges;
            e.mEdges[2].mEdges = e.mEdges;
            e.mEdges[3].mEdges = e.mEdges;
            Edge.NextID = id + 4;

            return e.mEdges[0];
        }


        /// <summary>
        /// Kill is designed to manage the re-assignment of references and to update
        /// the entire data structure when an edge is removed.
        /// </summary>
        /// <param name="edge"></param>
        public static void Kill(ref Edge edge)
        {
            if (edge == null) throw new ArgumentException();

            // detach the edge from its cell
            Splice(edge, edge.OPrev);
            Splice(edge.Sym, edge.Sym.OPrev);

            // free the quad edge that the edge belongs to
            // edge.mQuadEdge.Dispose();
            // edge.mQuadEdge = null;
            //edge.Dipose(true);
            edge.mCell.RemoveEdge(edge);
            edge = null;
        }

        /// <summary>
        /// Splice a given pair of edges.
        /// a, b . the edges to splice;
        ///         must be nonnull
        ///
        /// This operator affects the two edge rings around the origins of a and b,
        /// and, independently, the two edge rings around the left faces of a and b.
        /// In each case, (i) if the two rings are distinct, Splice will combine
        /// them into one; (ii) if the two are the same ring, Splice will break it
        /// into two separate pieces.
        /// Thus, Splice can be used both to attach the two edges together, and
        /// to break them apart. See Guibas and Stolfi (1985) p.96 for more details
        /// and illustrations.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Splice(Edge a, Edge b)
        {
            if (a == null || b == null) throw new ArgumentNullException();

            // see Guibas and Stolfi
            Edge alpha = a.ONext.Rot;
            Edge beta = b.ONext.Rot;

            Edge t1 = b.ONext;
            Edge t2 = a.ONext;
            Edge t3 = beta.ONext;
            Edge t4 = alpha.ONext;

            a.ONext = t1;
            b.ONext = t2;
            alpha.ONext = t3;
            beta.ONext = t4;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool explicitDispose)
        {
            if (explicitDispose)
            {
            }
        }

        #endregion

        public static uint NextID
        {
            get { return mNextID; }
            set { mNextID = value; }
        }

        /// <summary>
        /// The index of this edge in its quad-edge structure.
        /// Between 0 and 3 inclusive.
        /// </summary>
        public uint Index
        {
            get { return mIndex; }
            set { mIndex = value; }
        }

        /// <summary>
        /// Unique ID assigned to this Edge by a QuadEdge.
        /// </summary>
        public uint ID
        {
            get { return mID; }
            set { mID = value; }
        }

        /// <summary>
        /// Generic User data associated with this vertex.
        /// </summary>
        public object Data
        {
            get { return mData; }
            set { mData = value; }
        }

        /// <summary>
        /// Origin (starting) vertex of this directed edge.  Null if unknown
        /// </summary>
        public Vertex Origin
        {
            get { return mOrigin; }
            set
            {
                mOrigin = value;
                mOrigin.AddEdge(this);
            }
        }

        /// <summary>
        /// Destination (ending) vertex of this directed edge.  Null if unknown
        /// </summary>
        public Vertex Destination
        {
            get { return Sym.mOrigin; }
            set
            {
                Sym.Origin = value;
                value.AddEdge(Sym);
            }
        }


        /// <summary>
        /// The left face of this edge.  Null if currently unknown.
        /// </summary> Rot().face;
        public Face Left
        {
            get { return Rot.mFace; }
            set
            {
                Rot.mFace = value;
                value.AddEdge(this);
            }
        }

        /// <summary>
        /// The right face of this edge.  Null if currently unknown.
        /// </summary> InvRot().face;
        public Face Right
        {
            get { return InvRot.mFace; }
            set
            {
                InvRot.mFace = value;
                value.AddEdge(Sym);
            }
        }

        /// <summary>
        /// Return the dual of this edge, directed from its right to its left.
        /// the right to left dual of this edge;
        /// will be nonnull
        /// </summary>
        public Edge Rot
        {
            //get {return mIndex < 3 ? mQuadEdge.Edges[mIndex + 1] : mQuadEdge.Edges[mIndex - 3];}
            get { return mIndex < 3 ? mEdges[mIndex + 1] : mEdges[mIndex - 3]; }
        }

        /// <summary>
        ///  Return the dual of this edge, directed from its left to its right.
        /// the left to right dual of this edge;
        /// will be nonnull
        /// </summary>
        public Edge InvRot
        {
            //get {return mIndex > 0 ? mQuadEdge.Edges[mIndex - 1] : mQuadEdge.Edges[mIndex + 3];}
            get { return mIndex > 0 ? mEdges[mIndex - 1] : mEdges[mIndex + 3]; }
        }


        /// <summary>
        /// Return the edge from the destination to the origin of this edge.
        /// the symmetric of this edge;
        /// will be nonnull
        /// </summary>
        public Edge Sym
        {
            //get {return mIndex < 2 ? mQuadEdge.Edges[mIndex + 2] : mQuadEdge.Edges[mIndex - 2];}
            get { return mIndex < 2 ? mEdges[mIndex + 2] : mEdges[mIndex - 2]; }
        }

        /// <summary>
        /// Return the next ccw edge around (from) the origin of this edge.
        /// the next edge from the origin;
        /// will be nonnull
        /// </summary>
        public Edge ONext
        {
            get { return mNext; }
            set
            {
                // fundamentally when you re-set mNext, you're impacting a lot 
                mNext = value;
            }
        }

        /// <summary>
        /// Return the next cw edge around (from) the origin of this edge.
        /// the previous edge from the origin;
        /// will be nonnull
        /// </summary>
        public Edge OPrev
        {
            get { return Rot.ONext.Rot; }
        }

        /// <summary>
        /// Return the next ccw edge around (into) the destination of this edge.
        /// the next edge to the destination;
        /// will be nonnull
        /// </summary>
        public Edge DNext
        {
            get { return Sym.ONext.Sym; }
        }

        /// <summary>
        /// Return the next cw edge around (into) the destination of this edge.
        /// the previous edge to the destination;
        /// will be nonnull
        /// </summary>
        public Edge DPrev
        {
            get { return InvRot.ONext.InvRot; }
        }

        /// <summary>
        /// Return the ccw edge around the left face following this edge.
        /// the next left face edge;
        /// will be nonnull
        /// </summary>
        public Edge LNext
        {
            get { return InvRot.ONext.Rot; }
        }

        /// <summary>
        /// Return the ccw edge around the left face before this edge.
        /// the previous left face edge;
        /// will be non null
        /// </summary>
        public Edge LPrev
        {
            get { return ONext.Sym; }
        }

        /// <summary>
        /// Return the edge around the right face ccw following this edge.
        /// the next right face edge;
        /// will be nonnull
        /// </summary>
        public Edge RNext
        {
            get { return Rot.ONext.InvRot; }
        }

        /// <summary>
        /// Return the edge around the right face ccw before this edge.
        /// the previous right face edge;
        /// will be nonnull
        /// </summary>
        public Edge RPrev
        {
            get { return Sym.ONext; }
        }

        public override string ToString()
        {
            string s = "Edge " + ID;
            s += String.Format("\t Edge0 = {0}, Edge1 = {1}, Edge2 = {2}, Edge3={3} \n", mEdges[0].ID, mEdges[1].ID,
                               mEdges[2].ID, mEdges[3].ID);

            s += String.Format("\t ONext = {0}, Sym = {1}, Rot = {2}, InvRot = {3}\n", mNext.ID, Sym.ID, Rot.ID,
                               InvRot.ID);
            s += String.Format("\t Origin = {0}, Dest = {1}, Left = {2}, Right = {3}\n", Origin.ID, Destination.ID,
                               Left.ID,
                               Right.ID);
            return s;
        }
    }
}