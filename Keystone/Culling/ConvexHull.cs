using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Types;
using MTV3D65;
using ManagedStanHull;

namespace Keystone.Culling
{
    public class ConvexHull // TODO: should this implement IPageableTVResource? and should we just save our hulls in .obj?
    {
        protected Vector3d[] _vertices;  // point cloud
        protected Triangle[] _triangles; // faces

        public ConvexHull(string filepath)
        {
            // just a data file of verts?
        }

        //Pre-processes the input point cloud by converting it to a unit-normal cube. 
        //Duplicate vertices are removed based on a normalized tolerance
        // level (i.e. 0.1 means collapse vertices within 1/10th the width/breadth/depth of any side. 
        //This is extremely useful in eliminating slivers. When cleaning up ‘duplicates and/or nearby neighbors’ 
        //it also keeps the one which is ‘furthest away’ from the centroid of the volume.
        public static ConvexHull GetStanHull(TVMesh m)
        {

            MHull hullLib = new MHull();
            int count;
            float[] verts;

            if (m == null) throw new ArgumentNullException();

            count = m.GetVertexCount();
            verts = new float[count * 3];
            float ny = 0, nx = 0, nz = 0, tu1 = 0, tu2 = 0, tv1 = 0, tv2 = 0;
            int color = 0;

            for (int i = 0; i < count; i++)
            {
                m.GetVertex(i, ref verts[i * 3], ref verts[i * 3 + 1], ref verts[i * 3 + 2], ref ny, ref nx, ref nz, ref tu1,
                            ref tv1,
                            ref tu2, ref tv2, ref color);
            }

            //4096, 8192, 
            System.Diagnostics.Stopwatch watch = new Stopwatch() ;
            watch.Reset() ;
            watch.Start();
            MHullResult res = hullLib.CreateConvexHull(true, false, false, count, verts, 12, 0.001f, 8192, 8192, 0.01F);
            watch.Stop();
            Trace.Assert(res.Triangles);
            Trace.Assert(res.GetIndices().Length / 3 == res.FaceCount,
                         "Import.GetStanHull() -- Invalid face count " + res.GetVertices().Length);
            Trace.WriteLine("Convex hull created in " + watch.Elapsed + "seconds, with = " + res.Count + " vertices in " + res.FaceCount + " triangles.");

            return new ConvexHull(res.GetVertices(), res.GetIndices());

        }

        /// <summary>
        /// Constructor assumes Mesh is already a convex hull as opposed to concave
        /// </summary>
        /// <param name="obj"></param>
        public ConvexHull(TVMesh obj)
        {
            int count = obj.GetTriangleCount();
            _triangles = new Triangle[count];

            int index1, index2, index3;
            int group = 0;
            index1 = index2 = index3 = 0;

            for (int i = 0; i < count; i++)
            {
                obj.GetTriangleInfo(i, ref index1, ref index2, ref index3, ref group);
                _triangles[i] = Helpers.TVTypeConverter.FromTVMeshIndexedFace(obj, index1, index2, index3);
            }
        }

        public ConvexHull(float[] Vertices, uint[] indices)
        {
            Trace.Assert(Vertices.Length%3 == 0);
            int length = Vertices.Length/3;

            Vector3d[] tmp = new Vector3d[Vertices.Length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = new Vector3d(Vertices[i*3], Vertices[i*3 + 1], Vertices[i*3 + 2]);
            }

            _vertices = new Vector3d[indices.Length];
            for (int i = 0; i < indices.Length; i ++)
            {
                _vertices[i] = tmp[indices[i]];
            }

            _triangles = new Triangle[indices.Length/3];
            for (int i = 0; i < indices.Length/3; i ++)
            {
                _triangles[i] = new Triangle(_vertices[i*3], _vertices[i*3 + 1], _vertices[i*3 + 2]);
            }
        }

        /// <summary>
        /// Called from CollisionTest.collideWithWorld() to create a new hull in world coords.  This should be cashed
        /// until the entity using it has changed. NOTE: Generally, we only need to test a hull after bounding volume so this
        /// elminates all but the potential hits.  This way the "per frame" update is minimal since 99% of
        /// everything will be eliminated by the bounding volume test.
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="matrix"></param>
        public ConvexHull (Triangle[] triangles, Matrix matrix)
        {
            // takes an existing set of triangles, transforms them to world coords or any other matrix
            _triangles = new Triangle[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {

                Vector3d p1, p2, p3;
                p1 = Vector3d.TransformCoord(triangles[i].Points[0], matrix);
                p2 = Vector3d.TransformCoord(triangles[i].Points[1], matrix);
                p3 = Vector3d.TransformCoord(triangles[i].Points[2], matrix);
                _triangles[i] = new Triangle(p1, p2, p3);
            }
        }

        // a convex hull created from a bounding box
        public ConvexHull(BoundingBox box)
        {
            _triangles = BoundingBox.GetTriangleFaces(box);
            _vertices = box.Vertices;
        }

        // a convex hull created from an array of triangles
        public ConvexHull(Triangle[] tris)
        {
        }

        // a convex hull created from an array of vertices 
        public ConvexHull(Vector3d[] verts)
        {
            // if (verts.Length % 3 > 0) throw new ArgumentOutOfRangeException();
            _vertices = verts;
        }

        public ConvexHull(Triangle[] tris, Vector3d referencePoint, double scale)
        {
            // get the triangles for the bounding box

            // determine which ones are NOT facing the camera and extrude
            // the verts... hrm, but we have to match them still with the regular verts of the box 

            // would be easier using hte quad faces.  Start by getting the quad faces, computing hte normals
            // and finding which ones are forward facing.  Then extrude the ones not facing and skip the ones
            // already extruded.

            // 
        }

        // TODO: Ideally we should use the Box and its vertex normals to first determine which are the "back"
        // vertices which need to be extruded.  You're supposed to use vertex normals, but since our vertices are
        // shared by 3 faces of the box, we'd actually need to use face normals which is normally wrong but probably ok
        // in this case since we can "always" only use a box type shadow volume.  
        // a convex hull created from an array of contour vertices and extruded from the reference position by scale
        public ConvexHull(Vector3d[] verts, Vector3d referencePoint, double scale)
        {
            List<Vector3d> extrudedVerts = new List<Vector3d>();

            for (int i = 0; i < verts.Length; i++)
            {
                //    // NOTE: we can either store just the camera position to have fewer corner tests against
                //    // the frustum, or we can add the src poitns on the contour as well as the extruded points.

                //    // extrude the vertex by the scale (for shadowVolume its usually the light falloff range)
                //    Vector3d ex, tmp;
                //    Vector3d dir = Core._CoreClient.Maths.VSubtract(verts[i], referencePoint);
                //    ex = Core._CoreClient.Maths.VNormalize(dir);
                //    tmp = Core._CoreClient.Maths.VScale(ex, scale);
                //    ex = Core._CoreClient.Maths.VAdd(verts[i], tmp);

                //    bool add = true;
                //    foreach (Vector3d v in extrudedVerts)
                //    {
                //        // TODO: i can probably check for the redundant vertex prior to computing the extruded version
                //        if (v.Equals(verts[i]))
                //        {
                //            add = false;
                //            break;
                //        }
                //    }
                //    if (add)
                //    {
                //        // here we add both the original and extruded 
                //        // TODO: what we should be doing is taking the bounding volume
                //        // and simply MOVING the verts that are facing away and extrude those.
                extrudedVerts.Add(verts[i]);
                //        extrudedVerts.Add(ex);
                //    } 
            }
            _vertices = extrudedVerts.ToArray();
        }

        public Triangle[] Triangles
        {
            get { return _triangles; }
        }

        public Vector3d[] Vertices
        {
            get { return _vertices; }
        }

#if DEBUG
        //// debug visual aids.
        //public void Draw(TV_3DMATRIX mat, CONST_TV_COLORKEY color)
        //{
        //    // TODO: temporarily commented out til i fix this massive overhaul
        //    Vector3d[] tmp = new Vector3d[_vertices.Length];
        //    for (int i = 0; i < _vertices.Length; i++)
        //    {
        //        tmp[i] = Vector3d.TransformCoord(_vertices[i], Helpers.TVTypeConverter.FromTVMatrix(mat));
        //    }
        //    DebugDraw.DrawHull(tmp, color);
        //    for (int i = 0; i < _vertices.Length; i += 3)
        //    {
        //        DebugDraw.DrawNormalVector(tmp[i], tmp[i + 1], tmp[i + 2], 3);
        //    }
        //}

        //public void Draw(CONST_TV_COLORKEY color)
        //{
        //    DebugDraw.DrawHull(_vertices, color);
        //    //for (int i = 0; i < _vertices.Length; i += 3)
        //    //{
        //    //    DebugDraw.DrawNormalVector(_vertices[i], _vertices[i + 1], _vertices[i + 2], 3);
        //    //}
        //}
#endif
    }
}