using System;
using Keystone.Types;


namespace Keystone.EditDataStructures
{
    /// <summary>
    /// C# port of
    /// http://www.cs.cmu.edu/afs/andrew/scs/cs/15-463/2001/pub/src/a2/cell/vertex.hh
    /// and
    /// http://www.cs.cmu.edu/afs/andrew/scs/cs/15-463/2001/pub/src/a2/cell/vertex.cc
    /// 
    /// A vertex of a cell, with an outgoing set of directed edges.
    /// </summary>
    public sealed class Vertex : IDisposable
    {
        private Cell mCell;
        private object mData;
        private uint mID;
        private Edge mEdge;
        private float[] mPosition;

        public Vertex(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();
            mCell = cell;
            mID = cell.MakeVertexID();
            mPosition = new float[3];
            mPosition[0] = 0.0f;
            mPosition[1] = 0.0f;
            mPosition[2] = 0.0f;

            mData = null;
            cell.AddVertex(this);
        }

        ~Vertex()
        {
            Dispose(false);
        }

        public static Vertex Make(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();

            return new Vertex(cell);
        }

        public static void Kill(Vertex vertex)
        {
            if (vertex == null) throw new ArgumentNullException();
            vertex.Dispose(true);
            vertex = null;
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
                mCell.RemoveVertex(this);
            }
        }

        #endregion

        /// <summary>
        /// Unique ID assigned to this vertex by it's cell.
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

        public float[] Position
        {
            get { return mPosition; }
            set{mPosition = value;}
        }

        /// <summary>
        /// The cell that this vertex belongs to will be non-null.
        /// </summary>
        public Cell Cell
        {
            get { return mCell; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                mCell = value;
            }
        }

        /// <summary>
        /// Returns an arbitrary edge who's Origin is this vertex.  Returns null if this vertex is isolated.
        /// </summary>
        public Edge Edge
        {
            get { return mEdge; }
        }

        /// <summary>
        /// only keep track of one edge in the orbit--this one is as good as any
        /// </summary>
        /// <param name="newEdge">An edge who's Origin is at this vertex</param>
        public void AddEdge(Edge newEdge)
        {
            if (newEdge == null) throw new ArgumentNullException();
            mEdge = newEdge;
        }

        /// <summary>
        /// replace this arbitrary edge with another edge in the orbit
        /// use null if this is the only edge
        /// assumes that the edge hasn't been actually removed yet
        /// </summary>
        /// <param name="edge">An edge who's Origin is no longer at this vertex</param>
        public void RemoveEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();

            Edge next = edge.ONext;
            mEdge = next != edge ? next : null;
        }
    }
}