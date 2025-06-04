using System;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.EditDataStructures
{
    /// <summary>
    /// A cell is a loop of edges that when completed has a last edge who's Destination is the same as 
    /// the Origin of the first edge.  Thus, a cell defines a face.
    /// http://www.cs.cmu.edu/afs/andrew/scs/cs/15-463/2001/pub/src/a2/quadedge.html
    /// 
    ///An enclosed volume, bounded by a set of vertices and faces.
    ///
    /// Vertices   the vertices of the cell;
    ///           all are nonnull
    /// VertexIDs  an increasing sequence of positive integers used to number
    ///            distinct vertices;
    ///            all are positive
    /// Faces      the faces of the cell;
    ///            all are nonnull
    /// FaceIDs    an increasing sequence of positive integers used to number
    ///            distinct faces;
    ///            all are positive
    /// </summary>
    public sealed class Cell : IDisposable
    {
        // The vertices in this cell.
        // Nonnull.
        private List<Vertex> mVertices;

        // The next unused vertex ID.
        private uint mVertexID;

        private List<Edge> mEdges;

        // The faces in this cell.
        // Nonnull.
        private List<Face> mFaces;

        // The next unused face ID.
        private uint mFaceID;

        ///<summary >
        ///Initialize this cell consisting of no vertices and no faces.
        ///</summary>
        public Cell()
        {
            mVertices = new List<Vertex>(8);
            mVertexID = 1;

            mFaces = new List<Face>(6);
            mFaceID = 1;

            mEdges = new List<Edge>(4);
        }

        /// <summary>
        /// reclaim each of the vertices and faces still owned by the cell
        /// go in backwards order so that when the elements try to remove themselves
        /// it will be linear time
        /// </summary>
        ~Cell()
        {
            for (int i = mVertices.Count; i > 0; i--)
                Vertex.Kill(mVertices[i - 1]);

            for (int i = mFaces.Count; i > 0; i--)
                Face.Kill(mFaces[i - 1]);
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

        /// <summary>
        /// Return a new, degenerate cell consisting of a single closed edge (loop),
        /// a single vertex at the origin, and a pair of faces.
        /// <- the new cell;
        ///    will be nonnull
        /// </summary>
        public static Cell Make()
        {
            // create a looping edge that connects to itself at a single vertex
            // the edge delimits two faces
            // this is the smallest cell that is consistent with our invariants

            Cell cell = new Cell();

            Vertex vertex = Vertex.Make(cell);
            Face left = Face.Make(cell);

            Face right = Face.Make(cell);
            Edge edge = Edge.Make(cell).InvRot;

            edge.Origin = vertex;
            edge.Destination = vertex;
            edge.Left = left;
            edge.Right = right;

            return cell;
        }

        /// <summary>
        /// Release the storage occupied by a given cell.
        /// cell . the cell to kill;
        ///         must be nonnull
        /// </summary>
        public static void Kill(ref Cell cell)
        {
            if (cell == null) throw new ArgumentNullException();

            cell.Dispose();
            cell = null;
        }

        /// <summary>
        /// Return a new cell with the topology of a tetrahedron and all vertices at
        /// the origin.
        /// <- the new tetrahedron;
        ///    will be nonnull
        /// </summary>
        public static Cell MakeTetrahedron()
        {
            // use the Euler operators to construct a tetrahedron
            Cell cell = Make();

            // grab the initial vertex
            Vertex vertex0;
            {
                CellVertexIterator iterator = new CellVertexIterator(cell);
                vertex0 = iterator.Next();
                System.Diagnostics.Trace.Assert(vertex0 != null);
            }

            // grab the initial edge and the initial faces
            Edge edge1 = vertex0.Edge;
            System.Diagnostics.Debug.WriteLine(edge1.ToString());
            Face left = edge1.Left;
            System.Diagnostics.Debug.WriteLine("left: " + left);
            Face right = edge1.Right;
            System.Diagnostics.Debug.WriteLine("right: " + right);

            // drop in four vertices along the initial edge
            Vertex vertex1 = cell.MakeVertexEdge(vertex0, left, right).Destination;
            System.Diagnostics.Debug.WriteLine("Vertex0.Edge:" + vertex1.Edge);
            Vertex vertex2 = cell.MakeVertexEdge(vertex1, left, right).Destination;
            System.Diagnostics.Debug.WriteLine("Vertex1.Edge:" + vertex2.Edge);
            Vertex vertex3 = cell.MakeVertexEdge(vertex2, left, right).Destination;
            System.Diagnostics.Debug.WriteLine("Vertex2.Edge:" + vertex3.Edge);
            // cut each of the faces in half from complementary vertices
            Face front = cell.MakeFaceEdge(left, vertex1, vertex3).Right;
            System.Diagnostics.Debug.WriteLine("front: " + front);
            Face bottom = cell.MakeFaceEdge(right, vertex0, vertex2).Right;
            System.Diagnostics.Debug.WriteLine("bottom: " + bottom);


            float sqrt2 = (float) Math.Sqrt(2.0d);
            vertex0.Position[0] = -sqrt2; // probably off ???
            vertex0.Position[1] = -1.0f;
            vertex0.Position[2] = -1.0f;

            vertex1.Position[0] = sqrt2;
            vertex1.Position[1] = -1.0f;
            vertex1.Position[2] = -1.0f;

            vertex2.Position[0] = 0.0f;
            vertex2.Position[1] = 1.0f;
            vertex2.Position[2] = -1.0f;

            vertex3.Position[0] = 0.0f;
            vertex3.Position[1] = 0.0f;
            vertex3.Position[2] = 1.0f;

            return cell;
        }


        /// <summary>
        /// Return a new vertex ID.
        /// <- a new vertex ID;
        ///    will be positive
        /// </summary>
        public uint MakeVertexID()
        {
            return mVertexID++;
        }

        /// <summary>
        /// Return a new face ID.
        /// <- a new face ID;
        ///    will be positive
        /// </summary>
        // NOTE: It's important for undo/redo purposes that faceID is always incremented and that the faceid's of removed faces are not re-used except for when theose faces are restored via Undo
        // same goes for VertexIDs
        public uint MakeFaceID()
        {
            return mFaceID++;
        }

        internal int VertexCount
        {
            get { return mVertices.Count; }
        }

        internal int FaceCount
        {
            get { return mFaces.Count; }
        }

        internal Face[] Faces
        {
            get { return mFaces.ToArray(); }
        }

        internal Edge[] Edges
        {
            get { return mEdges.ToArray(); }
        }

        internal Vertex[] Vertices
        {
            get { return mVertices.ToArray(); }
        }

        internal Face GetFace(uint faceID)
        {
            for (int i = 0; i < mFaces.Count; i++)
                if (mFaces[i].ID == faceID)
                    return mFaces[i];

            return null;
        }

        internal Edge GetEdge(uint edgeID)
        {
            for (int i = 0; i < mEdges.Count; i++)
                if (mEdges[i].ID == edgeID)
                    return mEdges[i];

            return null;
        }

        internal Vertex GetVertex(uint vertexID)
        {
            for (int i = 0; i < mVertices.Count; i++)
                if (mVertices[i].ID == vertexID)
                    return mVertices[i];

            return null;
        }

        internal Vertex GetVertex(float x, float y, float z)
        {
            for (int i = 0; i < mVertices.Count; i++)
                if (mVertices[i].Position[0] == x && mVertices[i].Position[1] == y && mVertices[i].Position[2] == z)
                    return mVertices[i];

            return null;
        }

        internal void TranslateVertex(uint vertexID, Vector3d translation)
        {
            Vertex v = GetVertex(vertexID);
            v.Position[0] += (float) translation.x;
            v.Position[1] += (float) translation.y;
            v.Position[2] += (float) translation.z;
        }

        internal void TranslateEdge(uint edgeID, Vector3d translation)
        {
            Edge e = GetEdge(edgeID);
            TranslateVertex(e.Origin.ID, translation);
            TranslateVertex(e.Destination.ID, translation);
        }

        internal void TranslateFace(uint faceID, Vector3d translation)
        {
            Face f = GetFace(faceID);
            for (int i = 0; i < f.Vertices.Length; i++)
                TranslateVertex(f.Vertices[i], translation);
        }

        internal void MoveVertex(uint vertexID, Vector3d position)
        {
            Vertex v = GetVertex(vertexID);
            v.Position[0] = (float) position.x;
            v.Position[1] = (float) position.y;
            v.Position[2] = (float) position.z;
        }

        internal void MoveEdge(uint edgeID, Vector3d[] position)
        {
            if (position == null || position.Length != 2)
                throw new ArgumentException("position array must contain exactly 2 elements for a line segment");
            Edge e = GetEdge(edgeID);
            MoveVertex(e.Origin.ID, position[0]);
            MoveVertex(e.Destination.ID, position[1]);
        }

        internal void MoveFace(uint faceID, Vector3d[] position)
        {
            // TODO: the position array may not be in the proper order for the face vertices...
            Face f = GetFace(faceID);
            if (position == null || position.Length != f.Vertices.Length)
                throw new ArgumentException(
                    "position array must contain the exact number of elements as the Face vertices");
            for (int i = 0; i < f.Vertices.Length; i++)
                MoveVertex(f.Vertices[i], position[i]);
        }

        public void AddEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();
            mEdges.Add(edge);
        }

        public void RemoveEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();
            mEdges.Remove(edge);
        }

        /// <summary>
        /// Add a given vertex to this cell.
        /// vertex . the vertex to add;
        ///           must be nonnull;
        ///           must not be in the cell
        /// </summary>
        public void AddVertex(Vertex vertex)
        {
            if (vertex == null) throw new ArgumentNullException();
            mVertices.Add(vertex);
        }

        /// <summary>
        /// Remove a given vertex from this cell.
        /// vertex . the vertex to remove;
        ///           must be nonnull;
        ///           must be in the cell
        /// </summary>
        public void RemoveVertex(Vertex vertex)
        {
            if (vertex == null) throw new ArgumentNullException();
            mVertices.Remove(vertex);
        }

        /* -- public instance methods (Euler operators) -------------------------- */

        /*
       * Use these methods to construct cells with guaranteed consistent topology.
       * Other means of modifying a cell can potentially produce bizarre results.
       */


        /// <summary>
        /// Return a new edge formed by splitting a given vertex between a given pair
        /// of faces.
        /// A new vertex is introduced at the destination of the new edge.
        /// The new edge has _left_ along its left and _right_ along its right.
        /// vertex      . the vertex to split to make the new edge;
        ///                must be nonnull;
        ///                must share an edge with both _left_ and _right_
        /// left, right . the faces adjacent to the new edge;
        ///                must be nonnull;
        ///                must share an edge with _vertex_
        ///  the new edge  will be nonnull
        /// </summary>
        public Edge MakeVertexEdge(Vertex vertex, Face left, Face right)
        {
            if (vertex == null || left == null || right == null) throw new ArgumentNullException();

            // locate the edges to the right of each of the faces in the orbit of the vertex
            Edge edge = vertex.Edge;
            Edge edge1 = GetOrbitLeft(edge, right);
            Edge edge2 = GetOrbitLeft(edge, left);

            if (edge1 == null)
            {
                System.Diagnostics.Trace.WriteLine(
                    string.Format("Cell::makeVertexEdge: unable to locate right face {0} on vertex {1}", right.ID,
                                  vertex.ID));
                // abort();
            }

            if (edge2 == null)
            {
                System.Diagnostics.Trace.WriteLine(
                    string.Format("Cell::makeVertexEdge: unable to locate left face {0} on vertex {1}", left.ID,
                                  vertex.ID));
                // abort();
            }

            // create a new vertex and copy the position of the vertex of origin
            Vertex vertexNew = Vertex.Make(this);
            vertexNew.Position[0] = vertex.Position[0];
            vertexNew.Position[1] = vertex.Position[1];
            vertexNew.Position[2] = vertex.Position[2];

            // create a new edge and rotate it to make a clockwise loop
            Edge edgeNew = Edge.Make(this).Rot;

            // connect the origin (and destination) of the new edge to _vertex_ so that
            // the left face of the edge is _left_
            // this makes a loop on the inside of _left_
            Edge.Splice(edge2, edgeNew);

            // split the origin and destination of the loop so that the right face of the
            // edge is now _right_
            // this results in a non-loop edge dividing _left_ from _right_
            Edge.Splice(edge1, edgeNew.Sym);

            // initialize the secondary attributes of the new edge
            edgeNew.Origin = edge1.Origin;
            edgeNew.Left = edge2.Left;
            edgeNew.Right = edge1.Left;

            // all edges leaving the destination orbit of the new edge now have the new
            // vertex as their vertex of origin
            SetOrbitOrg(edgeNew.Sym, vertexNew);

            return edgeNew;
        }

        /// <summary>
        /// Delete a given edge from this cell, along with its destination vertex.
        /// edge . the edge to kill;
        ///         must be nonnull
        ///</summary>
        public void KillVertexEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();

            // locate _edge1_ and _edge2_ as in _makeVertexEdge_
            Edge edge1 = edge.OPrev;
            Edge edge2 = edge.LNext;

            // use _edge1_ for _edge2_ if the destination vertex is isolated
            if (edge2 == edge.Sym)
                edge2 = edge1;

            // inverse of _makeVertexEdge_
            Edge.Splice(edge1, edge.Sym);
            Edge.Splice(edge2, edge);

            // all edges leaving the destination orbit of the deleted edge now have its
            // origin vertex as their vertex of origin
            SetOrbitOrg(edge2, edge1.Origin);

            // don't use the deleted edge as a reference edge any more
            edge1.Origin.AddEdge(edge1);
            edge1.Left.AddEdge(edge1);
            edge2.Left.AddEdge(edge2);

            // reclaim the vertex and the edge
            edge.Destination.Dispose();
            edge.Destination = null;
            edge.Dispose();
            edge = null;
        }

        ///<summary>
        /// Return a new edge formed by splitting a given face through a given pair
        /// of vertices.
        /// A new face is introduced to the right of the new edge.  This new face
        /// can be read via the new edge's .Right property.  The .Left property will be 
        /// equivalent to the face that was passed in and split
        /// The new edge has _org_ as its origin and _dest_ as its destination.
        /// face      . the face to divide to make the new edge;
        ///              must be nonnull;
        ///              must have both _org_ and _dest_ on its perimiter
        /// org, dest . the vertices for the endpoints of the new edge;
        ///              must be nonnull;
        ///              must be located on the perimiter of _face_
        /// <- the new edge;
        ///    will be nonnull
        /// </summary>
        public Edge MakeFaceEdge(Face face, Vertex org, Vertex dest)
        {
            if (face == null || org == null || dest == null) throw new ArgumentNullException();

            // locate the edges leaving each of the vertices in the orbit of the face
            Edge edge = face.Edge;
            Edge edge1 = GetOrbitOrg(edge, org);
            Edge edge2 = GetOrbitOrg(edge, dest);

            if (edge1 == null)
            {
                System.Diagnostics.Trace.WriteLine(
                    string.Format("Cell.MakeFaceEdge: unable to locate origin vertex {0} on face {1}", org.ID, face.ID));
                //abort();
            }

            if (edge2 == null)
            {
                System.Diagnostics.Trace.WriteLine(
                    string.Format("Cel.MakeFaceEdge: unable to locate destination vertex {0} on face {1}", dest.ID,
                                  face.ID));
                //abort();
            }

            // create a new face
            Face faceNew = Face.Make(this);

            // create a new (non-loop) edge
            Edge edgeNew = Edge.Make(this);

            // connect the destination of the new edge to the origin of _edge2_
            // both faces of the edge are now _face_

            Edge.Splice(edge2, edgeNew.Sym);

            // connect the origin of the new edge to _edge1_
            // _face_ is split in half along the new edge, with the new face introduced
            // on the right

            Edge.Splice(edge1, edgeNew);

            // initialize the secondary attributes of the new edge

            edgeNew.Origin = edge1.Origin;
            edgeNew.Destination = edge2.Origin;
            edgeNew.Left = edge2.Left;

            // all edges in the right orbit of the new edge (i.e. the left orbit of its
            // Sym) now have the new face as their left face

            SetOrbitLeft(edgeNew.Sym, faceNew);

            return edgeNew;
        }

        ///<summary>
        /// Delete a given edge from this cell, along with its right face.
        /// edge . the edge to kill;
        ///         must be nonnull
        /// </summary>
        public void KillFaceEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException();

            // locate _edge1_ and _edge2_ as in _makeFaceEdge_
            Edge edge1 = edge.OPrev;
            Edge edge2 = edge.LNext;

            // use _edge2_ for _edge1_ if the right face is inside a loop
            if (edge1 == edge.Sym)
                edge1 = edge2;

            // inverse of _makeFaceEdge_
            Edge.Splice(edge2, edge.Sym);
            Edge.Splice(edge1, edge);

            // all edges in the right orbit of the deleted edge now have its left face
            // as their left face
            SetOrbitLeft(edge1, edge2.Left);

            // don't use the deleted edge as a reference edge any more
            edge1.Origin.AddEdge(edge1);
            edge2.Origin.AddEdge(edge2);
            edge2.Left.AddEdge(edge2);

            // reclaim the face and the edge
            //edge.Right.Dipose();
            edge.Right = null;
            //Face.Kill(ref edge.Right);
            Edge.Kill(ref edge);
        }

        ///<summary>
        /// Add a given face to this cell.
        /// face . the face to add;
        ///         must be nonnull
        ///         must not be in the cell
        /// </summary>
        public void AddFace(Face face)
        {
            if (face == null) throw new ArgumentNullException();
            mFaces.Add(face);
        }

        /// Remove a given face from this cell.
        /// face . the face to remove;
        ///         must be nonnull;
        ///         must be in the cell
        /// </summary>
        public void RemoveFace(Face face)
        {
            if (face == null) throw new ArgumentNullException();
            mFaces.Remove(face);
        }

        ///<summary>
        /// Return the edge with a given origin vertex in the face orbit of a given
        /// edge.
        /// edge . an edge of the orbit to look for the vertex in;
        ///         must be nonnull
        /// org  . the origin vertex to look for;
        ///         must be nonnull
        /// <- the edge in the same face orbit as _edge_ with origin vertex _org_;
        ///    null if not found
        /// </summary>
        private Edge GetOrbitOrg(Edge edge, Vertex org)
        {
            if (edge == null || org == null) throw new ArgumentNullException();

            // traverse the Lnext orbit of _edge_ looking for an edge whose origin is  _org_
            Edge scan = edge;

            do
            {
                if (scan.Origin == org)
                    return scan;

                scan = scan.LNext;
            } while (scan != edge);

            return null;
        }

        ///<summary>
        /// Set the origin of the vertex orbit (radial traversal around the vertex like spokes) of a given edge to a given vertex.
        /// edge . an edge of the orbit to set the origin vertex of;
        ///         must be nonnull
        /// org  . the new origin vertex;
        ///         must be nonnull
        /// </summary>
        private void SetOrbitOrg(Edge edge, Vertex org)
        {
            if (edge == null || org == null) throw new ArgumentNullException();

            // traverse the Onext orbit of _edge_, setting the origin of each edge to  _org_
            Edge scan = edge;

            do
            {
                scan.Origin = org;
                scan = scan.ONext;
            } while (scan != edge);
        }

        ///<summary>
        /// Return the edge with a given left face in the vertex orbit of a given
        /// edge.
        /// edge . an edge of the orbit to look for the face in;
        ///         must be nonnull
        /// left . the left face to look for;
        ///         must be nonnull
        /// <- the edge in the same vertex orbit as _edge_ with left face _left_;
        ///    null if not found
        /// </summary>
        private Edge GetOrbitLeft(Edge edge, Face left)
        {
            if (edge == null || left == null) throw new ArgumentNullException();

            // traverse the Onext orbit of _edge_ looking for an edge whose left face is _left
            Edge scan = edge;

            do
            {
                if (scan.Left == left)
                    return scan;

                scan = scan.ONext;
            } while (scan != edge);

            return null;
        }

        ///<summary>
        /// Set the left face of the face orbit of a given edge to a given face.
        /// edge . an edge of the orbit to set the left face of;
        ///         must be nonnull
        /// left . the new left face;
        ///         must be nonnull
        /// </summary>
        private void SetOrbitLeft(Edge edge, Face left)
        {
            if (edge == null || left == null) throw new ArgumentNullException();


            // traverse the Lnext orbit of _edge_, setting the left face of each edge to _left_
            Edge scan = edge;

            do
            {
                scan.Left = left;
                scan = scan.LNext;
            } while (scan != edge);
        }
    }
}