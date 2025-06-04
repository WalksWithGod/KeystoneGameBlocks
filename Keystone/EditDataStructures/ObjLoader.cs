using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Keystone.Loaders;
using MTV3D65;

namespace Keystone.EditDataStructures
{
    public class ObjLoader
    {
        private class Tface // a (temporary) face
        {
            public LinkedList<Tvert> vlist; // the vertices of this face, in ccw order
            public uint no; // face number
            public Face face; // final face in cell, null if not inst. yet
            // need anything else in here??
        } ;


        private class Tsector
        {
            public Tvert p; // first ccw vertex
            public Tface f; // intervening face
            public Tvert q; // second ccw vertex

            public Tsector(Tvert p, Tface f, Tvert q)
            {
                this.p = p;
                this.f = f;
                this.q = q;
            }
        }

        private List<Tsector> Arc; // arc of consec. edges emanating from a vertex
        // in counterclockwise order
        // (a linked list of pointers to other vertex)


        private LinkedList<LinkedList<Tsector>> Arclist; // unordered collection of arcs about a vertex
        // (a linked list of pointers to Arcs)
        // when done, this (linear) list contains
        // the ccw cycle of edges about a vertex

        // For example, for vertex v below,
        //
        //       c------ b------i
        //      / \     /      /
        //     /   \   /      /
        //    /     \ /      /
        //   d------ v -----a--h
        //    \     / \        |
        //     \   /   \       |
        //      \ /     \      |
        //       e-------f-----g
        //
        // some valid Arcs are the lists (a,b), (a,b,c), (b,c), (c,d),
        // (f,a), (e,f,a,b), etc. because those are the other endpoints of
        // edges emanating from v, in counterclockwise (ccw) order.
        // An arc always consists of at least two vertices.
        // A valid Arclist is any set of disjoint arcs, in arbitrary order.
        // When done, the Arclist for this vertex would be a single Arc.
        // It would be a cyclic permutation of (a,b,c,d,e,f).

        private class Tvert // a (temporary) vertex
        {
            public uint no; // ??for debugging
            public bool done; // is topology fully set & arclist complete?
            public float[] p; // position
            public LinkedList<LinkedList<Tsector>> arclist; // info about the vertices adjacent to this one
            public Vertex vertex; // final vertex in cell, null if not id. yet
            public bool instantiated; // true if identified and instantiated

            public override bool Equals(object obj)
            {
                if (obj is Tvert)
                {
                    Tvert v = (Tvert) obj;
                    return (v.p[0] == p[0] && v.p[1] == p[1] && v.p[2] == p[2]);
                }
                else
                    return base.Equals(obj);
            }
        }

        public static Cell Create(WaveFrontObj obj)
        {
            List<Tface> faces = new List<Tface>(); // all the faces
            List<Tvert> verts = new List<Tvert>(); // all vertices
            uint nvert = 0;
            uint nface = 0;

            foreach (TV_3DVECTOR vert in obj.Points)
            {
                Tvert v = new Tvert();
                v.arclist = new LinkedList<LinkedList<Tsector>>();
                v.no = nvert++;
                v.done = false;
                v.instantiated = false;
                v.p =
                    new float[3] {vert.x, vert.y, vert.z};
                verts.Add(v);
            }

            nvert = 0;
            foreach (WaveFrontObjIndexedFace currentFace in obj.Faces)
            {
                Tface f = new Tface();
                f.vlist = new LinkedList<Tvert>();
                for (uint i = 0; i < currentFace.Points.Length; i++)
                {
                    f.vlist.AddLast(verts[(int) currentFace.Points[i]]);
                    nvert++;
                }
                add_arcs(f.vlist, f);
                f.no = nface++;
                    // note that the resulting quad edge face id's will match the face id's of the EditableMesh IndexedFace's list
                faces.Add(f);
            }

            // from the .obj loader they use when initializing the quad edge data structure
            // they store in the Face.Data a reference to IndexedFace for instance
            // this is how they can track the other data.  If our IndexedFaces also retain their "Group" reference
            // then we can also know which materials to use and when

            return BuildQuadedge(verts.ToArray(), faces);
            //       
            // TODO: there are some concerns about performance... both our .obj loader and the generation of the qe structure...
        }


        private static void add_arcs(LinkedList<Tvert> vlist, Tface f)
        {
            // cout << "add_arcs " << vlist;
            // vlist is not a circular list, but we need to step through all
            // consecutive triples as if it were circular
            //List_item<Tvert> u, v, w;
            LinkedListNode<Tvert> u, v, w;
            for (u = vlist.Last, v = vlist.First, w = v.Next; w != null; u = v, v = w, w = w.Next)
                merge_arc(v.Value, w.Value, u.Value, f);

            merge_arc(v.Value, vlist.First.Value, u.Value, f); // one more that we missed
        }

        private static void merge_arc(Tvert v, Tvert p, Tvert q, Tface f)
        {
            // Merge the arc (p,q) into the list of arcs around vertex v.
            // Cases:
            //  1. ( bef &&  aft) it connects two existing arcs
            //  2. ( bef && !aft) it goes on the end of an existing arc
            //  3. (!bef &&  aft) it goes on the beginning of an existing arc
            //  4. (!bef && !aft) it does not connect with an existing arc
            // cout << "merge_arc " << *v << " " << *p << " " << *q << endl;
            // cout << "before, arclist=" << v.arclist;
            LinkedListNode<LinkedList<Tsector>> a, aft_item;
            aft_item = null;

            LinkedList<Tsector> bef, aft;
            bef = null;
            aft = null;

            Tsector sector = new Tsector(p, f, q);
            for (a = v.arclist.First; a != null; a = a.Next)
            {
                // a.Value is an Arc
                if (a.Value.Last.Value.q.Equals(p)) bef = a.Value;
                if (a.Value.First.Value.p.Equals(q))
                {
                    aft = a.Value;
                    aft_item = a;
                }
            }
            // cout << "  bef=" << *bef << "  aft=" << *aft;
            // now concatenate the three arcs bef, (p,q), and aft
            // where bef and aft might be null
            if (bef != null)
            {
                if (aft != null)
                {
                    // 1. ( bef &&  aft) it connects two existing arcs
                    bef.AddLast(sector); // insert new sector
                    if (bef == aft)
                    {
                        // done with vertex! connecting these would make arc circular
                        // cout << v.arclist << " done" << endl;
                        v.done = true;
                        return;
                    }
                    // now we'll merge two arcs in the arclist
                    v.arclist.Remove(aft_item); // remove following arc
                    Concat(bef, aft); // and concat it into previous
                }
                else // 2. ( bef && !aft) it goes on the end of existing arc
                    bef.AddLast(sector);
            }
            else
            {
                if (aft != null) // 3. (!bef &&  aft) it goes on beg. of existing arc
                    aft.AddFirst(sector);
                else
                {
                    // 4. (!bef && !aft) it doesn't connect w. existing arc
                    LinkedList<Tsector> arc = new LinkedList<Tsector>();
                    //Trace.Assert(arc);
                    arc.AddLast(sector);
                    v.arclist.AddLast(arc);
                }
            }
            // cout << "after, arclist=" << v.arclist;
        }

        private static void Concat(LinkedList<Tsector> list1, LinkedList<Tsector> list2)
        {
            foreach (Tsector value in list2)
                list1.AddLast(value);
        }

        // Functions below do not use the above which is just for dealing with obj
        // so perhaps the below are also useful for actual realtime model construction in an editor
        // such as when plotting points for faces
        private static Cell BuildQuadedge(Tvert[] verts, List<Tface> faces)
        {
            check_closed(verts);

            // create a cell and fetch its initial vertex
            Cell cell = Cell.Make();

            Vertex vertex1;
            {
                CellVertexIterator vertices = new CellVertexIterator(cell);
                vertex1 = vertices.Next();
            }

            // instantiate a face of the initial vertex
            {
                Tvert v = verts[0];
                v.vertex = vertex1;
                v.vertex.Position = v.p;
                v.vertex.ID = v.no;

                makeFace(cell, v.arclist.First.Value.First.Value.f);
            }

            // instantiate identified vertices until all are instantiated
            for (;;)
            {
                bool instantiated = true;

                for (int i = 0; i < verts.Length; i++)
                {
                    Tvert v = verts[i];

                    if (v.vertex != null && !v.instantiated)
                        makeVertex(cell, v);

                    instantiated &= v.instantiated;
                }

                if (instantiated)
                    break;
            }

            // reset the data pointers of all faces
            CellFaceIterator iterator = new CellFaceIterator(cell);

            Face face;

            while ((face = iterator.Next()) != null)
                face.mData = null;

            return cell;
        }

        /*
         * identified   <=> Tvert has been associated with a particular Vertex
         * instantiated <=> Tface has been associated with a particular Face AND
         *                  all vertices of the face have been identified
         * instantiated <=> Tvert has been identified AND
         *                  all adjacent Tfaces have been instantiated
         */

        /*
         * Return true if a given pair of vertices is connected directly by an edge
         * along a given left face.
         * vertex1, vertex2 . the vertices to check;
         *                     must be nonnull
         * left             . the left face to check for;
         *                     must be nonnull
         * <- true if there is an edge from _vertex1_ to _vertex2_ with left face
         *    _left_
         */

        private static bool isConnected(Vertex vertex1, Vertex vertex2, Face left)
        {
            System.Diagnostics.Trace.Assert(vertex1 != null);
            System.Diagnostics.Trace.Assert(vertex2 != null);
            System.Diagnostics.Trace.Assert(left != null);

            // check the orbit of vertex1 for an edge to vertex2
            VertexEdgeIterator edges = new VertexEdgeIterator(vertex1);

            EditDataStructures.Edge edge;

            while ((edge = edges.Next()) != null)
                if (edge.Destination == vertex2 && edge.Left == left)
                    return true;

            return false;
        }

        /*
         * Return the face to the right of a given face around a given vertex.
         * vertex . the vertex to look for the face around;
         *           must be nonnull
         * left   . the left face to return the right face of;
         *           must be nonnull
         * <- the face to the right of _left_ around _vertex_;
         *    null if none
         */

        private static Face RightFace(Vertex vertex, Face left)
        {
            System.Diagnostics.Trace.Assert(vertex != null);
            System.Diagnostics.Trace.Assert(left != null);

            // check the left face of each edge in the orbit of the vertex
            Edge start = vertex.Edge;
            Edge scan = start;

            do
            {
                if (scan.Left == left)
                    return scan.Right;

                scan = scan.ONext;
            } while (scan != start);

            return null;
        }

        /*
         * Return true if a given vertex is adjacent to a given face.
         * face   . the face to look for the vertex in;
         *           must be nonnull
         * vertex . the vertex to look for;
         *           must be nonnull
         * <- true if _vertex_ is on _face_
         */

        private static bool hasVertex(Face face, Vertex vertex)
        {
            System.Diagnostics.Trace.Assert(face != null);
            System.Diagnostics.Trace.Assert(vertex != null);

            // check the origin vertex of each edge on the face

            FaceEdgeIterator edges = new FaceEdgeIterator(face);

            Edge edge;

            while ((edge = edges.Next()) != null)
                if (edge.Origin == vertex)
                    return true;

            return false;
        }

        /*
         * Return true if a given face includes all the identified vertices on a given
         * Tvert list.
         * face  . the face to check;
         *          must be nonnull
         * vlist . the vertex list to check against;
         *          must be nonnull
         * <- true if _face_ is adjacent to all the vertices on _vlist_
         */

        private static bool hasVertices(Face face, LinkedList<Tvert> vlist)
        {
            System.Diagnostics.Trace.Assert(face != null);
            System.Diagnostics.Trace.Assert(vlist != null);

            // check each vertex on the list
            for (LinkedListNode<Tvert> vi = vlist.First; vi != null; vi = vi.Next)
            {
                Vertex vertex = vi.Value.vertex;

                if (vertex != null && !hasVertex(face, vertex))
                    return false;
            }

            return true;
        }

        /*
         * Return a face that can be used to instantiate a given Tface.
         * cell . the cell to get the face from;
         *         must be nonnull
         * f    . Tface to get the face for;
         *         must be nonnull
         * <- a face that can be used to instantiate _f_;
         *    null if none are available
         */

        private static Face getFace(Cell cell, Tface f)
        {
            System.Diagnostics.Trace.Assert(cell != null);
            // System.Diagnostics.Trace.Assert(f!=null);

            // locate all the unused faces in the cell
            Face[] faces = new Face[cell.FaceCount];
            uint count = 0;

            {
                CellFaceIterator iterator = new CellFaceIterator(cell);

                Face face;

                while ((face = iterator.Next()) != null)
                    if (face.mData == null)
                        faces[count++] = face;
            }

            // discard any faces that don't include all the identified vertices of the
            // Tface

            {
                uint i = 0;

                while (i < count)
                {
                    Face face = faces[i];

                    if (hasVertices(face, f.vlist))
                        i++;
                    else
                        faces[i] = faces[--count];
                }
            }

            Face face2 = count > 0 ? faces[0] : null;
            faces = null;

            return face2;
        }

        /*
         * Instantiate a given Tface in a given cell by identifying its vertices.
         * cell . the cell to instantiate the face in;
         *         must be nonnull
         * f    . the Tface to instantiate;
         *         must be nonnull
         */

        private static void makeFace(Cell cell, Tface f)
        {
            System.Diagnostics.Trace.Assert(cell != null);
            //System.Diagnostics.Trace.Assert(f!=null);

            // get the face to use for the Tface
            Face face = getFace(cell, f);

            System.Diagnostics.Trace.Assert(face != null);

            // connect all pairs of identified vertices on the face, as necessary
            {
                for (LinkedListNode<Tvert> vi = f.vlist.First; vi != null; vi = vi.Next)
                {
                    Vertex vertex1 = vi.Value.vertex;
                    Vertex vertex2;

                    if (vertex1 != null)
                    {
                        // find the next identified vertex, even if just itself
                        LinkedListNode<Tvert> vj = vi;

                        for (;;)
                        {
                            vj = vj.Next;

                            if (vj == null)
                                vj = f.vlist.First;

                            vertex2 = vj.Value.vertex;

                            if (vertex2 != null)
                                break;
                        }

                        // connect the vertices, if necessary
                        if (!isConnected(vertex1, vertex2, face))
                            //cell.MakeFaceEdge(face, vertex1, vertex2).Right = null;
                            cell.MakeFaceEdge(face, vertex1, vertex2);
                    }
                }
            }

            // find the first identified vertex
            LinkedListNode<Tvert> vi0 = f.vlist.First;

            while (vi0.Value.vertex == null)
                vi0 = vi0.Next;

            // identify all the following and preceding vertices
            LinkedListNode<Tvert> vi2 = vi0;
            Vertex vertex = vi0.Value.vertex;

            for (;;)
            {
                vi2 = vi2.Next;

                if (vi2 == null)
                    vi2 = f.vlist.First;

                if (vi2 == vi0)
                    break;

                Tvert v = vi2.Value;

                if (v.vertex == null)
                {
                    Face right = RightFace(vertex, face);

                    Trace.Assert(right != null);

                    v.vertex = cell.MakeVertexEdge(vertex, face, right).Destination;
                    v.vertex.Position = v.p;

                    v.vertex.ID = v.no;
                }

                vertex = v.vertex;
            }

            // the face is now instantiated
            f.face = face;

            face.ID = f.no;
            face.mData = f;
        }

        /*
         * Instantiate a given identified Tvert in a given cell by instantiating its
         * adjacent faces.
         * cell . the cell to instantiate the Tvert in;
         *         must be nonnull
         * v    . the Tvert to instantiate;
         *         must be nonnull
         */

        private static void makeVertex(Cell cell, Tvert v)
        {
            System.Diagnostics.Trace.Assert(cell != null);
            //System.Diagnostics.Trace.Assert(v!=null);

            // find the first sector with an identified p vertex
            LinkedListNode<Tsector> wi0 = v.arclist.First.Value.First;

            while (wi0.Value.p.vertex == null)
                wi0 = wi0.Next;

            // instantiate all following sectors of the vertex in counterclockwise order
            LinkedListNode<Tsector> wi = wi0;

            do
            {
                Tface f = wi.Value.f;

                if (f.face == null)
                    makeFace(cell, f);

                wi = wi.Next;

                if (wi == null)
                    wi = v.arclist.First.Value.First;
            } while (wi != wi0);

            // the vertex is now instantiated
            v.instantiated = true;
        }

        //static void print_quadedge(Array<Tvert> verts, List<Tface> faces) {
        //    // print vertices around each face and vertex currently

        //    cout << "VERTICES OF EACH FACE:" << endl;
        //    List_item<Tface> *fi;
        //    for (fi=faces.first(); fi; fi=fi.next()) {
        //    cout << "face:";
        //    List_item<Tvert> *vi;
        //    for (vi=fi.obj.vlist.first(); vi; vi=vi.next())
        //        cout << " " << vi.obj.no;
        //    cout << endl;
        //    }
        //    cout << endl;

        //    cout << "VERTICES AROUND EACH VERTEX:" << endl;
        //    int i;
        //    for (i=0; i<verts.num(); i++) {
        //    Tvert *v;
        //    v = &verts[i];
        //    cout << "around vertex " << v.no << ":";
        //    assert(v.done);
        //    assert(v.arclist.length()==1);
        //    // step through the Tverts in the first (and only) arc of arclist
        //    List_item<Tsector> *wi;
        //    for (wi=v.arclist.first().obj.first(); wi; wi=wi.next()) {
        //        Tsector *sector = wi.obj;
        //        cout << " " << *sector;
        //    }
        //    cout << endl;
        //    }
        //}

        private static void check_closed(Tvert[] verts)
        {
            // check to see if polyhedron is closed (else we'd crash soon anyway)
            int i;
            for (i = 0; i < verts.Length; i++)
            {
                LinkedList<LinkedList<Tsector>> al = verts[i].arclist;
                if (!verts[i].done || al.Count != 1)
                {
                    if (al.Count == 0)
                        Trace.WriteLine("\nERROR in OBJ file: unused vertex " + verts[i].no);
                    else if (!verts[i].done)
                        Trace.WriteLine("\nERROR in OBJ file: vertex " + verts[i].no + " is not surrounded by polygons");
                    else
                        Trace.WriteLine("\nERROR in OBJ file: repeated face: " + al.First.Next.Value.First.Value.f);
                }
            }
        }
    }
}