using System;
using System.Collections.Generic;
using Keystone.Cameras;
using Keystone.Culling;
using Keystone.Elements;
using Keystone.Types;
using MTV3D65;

namespace Keystone
{
    // http://www.truevision3d.com/forums/tv3d_sdk_65/prerelease_completion_progress_post-t5988.0.html;msg103388#msg103388
    // generally speaking, I think one of these should be instanced for each occluder
    // in the area and the update method called every time the camera position has changed.
    public class OcclusionFrustum : PlanedFrustum
    {
        private Vector3d _lastCameraPos;
        public ConvexHull Hull;
        public List<Line3d> _edges;
        private List<Triangle> _facingTris;

        private bool _isDirty;
                     // NOTE: This is just for debugging in order to create a occlusion frustum and render it in one spot

        // without it updating every frame.  This way we can render it once and then walk around it to confirm its ok.  
        // otherwise normally, we must update the frustum everytime the camera moves and so its always "dirty"
        // As far as actually moving the occluders themselves, we rebuild the entire object instance via constructor below.
        // in other words, dont move occluders because rebuilding them is slower than updating them.

        public OcclusionFrustum(ConvexHull hull)
        {

            // TODO: actually im using incorrect vars here
            // First, for an occluder there's only a "planes" hull type.
            // For VIewFrustum there is "sphere, cone and planes.
            // For either occluder or viewfrustum
            // - sphereFrustum can test mesh Sphere or mesh AABB
            // - coneFrustum can only test mesh Spheres
            // - planes can test mesh Spheres or mesh AABB

            // further, you can run Sphere, Cone and Planes in a cascading way.
            // for instance, first test the mesh against Sphere.  If its INTERSECT
            // then go on to test Cone or Planes.  

            if (hull == null) throw new ArgumentNullException();
            Hull = hull;

            _facingTris = new List<Triangle>();
            _edges = new List<Line3d>();
            _isDirty = true;
        }

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public override bool IsVisible(Geometry mesh)
        {
            bool result = IsVisible(mesh.BoundingBox);

            return result;
        }


        public bool IsVisible(Vector3d[] vertices)
        {
            IntersectResult result = Intersects((vertices));

            //return (result == IntersectResult.INSIDE || result == IntersectResult.INTERSECT );
            return result != IntersectResult.OUTSIDE;
        }

        public override bool IsVisible(BoundingBox box)
        {
            // NOTE: for occluders, we return the opposite of the normal visiblity test
            return (Intersects(box) != IntersectResult.INSIDE);
        }

        public override bool IsVisible(BoundingSphere sphere)
        {
            // NOTE: for occluders, we return the opposite of the normal visiblity test
            return (Intersects(sphere) != IntersectResult.INSIDE);
        }

        public void Update(Vector3d camPos, bool contourPlanesOnly, bool forceUpdate)
        {
            if (!_lastCameraPos.Equals(camPos) || forceUpdate)
            {
                _edges.Clear();
                _facingTris.Clear();
                _lastCameraPos = camPos;

                // This implementation based on the psuedo code from the following article.
                //http://www.gamasutra.com/features/20020717/bacik_03.htm
                //To build contours from a convex hull, we use a simple algorithm utilizing the fact
                // that each edge in a convex hull connects exactly two faces. The algorithm is this:

                // 1. Iterate through all polygons, and detect whether a polygon faces the viewer. 
                // (To detect whether a polygon faces the viewer, use the dot product of the polygon’s
                // normal and direction to any of the polygon’s vertices. When this is less than 0, 
                // the polygon faces the viewer.)

                for (int i = 0; i < Hull.Triangles.Length; i++)
                {
                    Vector3d dir = Hull.Triangles[i].Points[0] - camPos;
                    // TODO: hope this is correct.  Down the line if there's any problems switch from using
                    // triangle face normal to a vertex normal of any point on the triangle
                    if (Vector3d.DotProduct(dir, Hull.Triangles[i].Normal) < 0)
                    {
                        _facingTris.Add(Hull.Triangles[i]);
                    }
                }

                // 2. If the polygon faces viewer, do the following for all its edges: If the edge is already 
                // in the edge list, remove the edge from the list since this means its an interior edge.
                // Otherwise, add the edge into the list.

                foreach (Triangle tri in _facingTris)
                {
                    Line3d e = new Line3d(tri.Points[0], tri.Points[1]);
                    UpdateEdges(e);
                    e = new Line3d(tri.Points[1], tri.Points[2]);
                    UpdateEdges(e);
                    e = new Line3d(tri.Points[2], tri.Points[0]);
                    UpdateEdges(e);
                }

                // After this, we should have collected all the edges forming the occluder’s contour, as seen
                // from the viewer’s position. Once you’ve got it, it’s time to build the occlusion frustum itself,
                // as shown in Figure 7 (note that this figure shows a 2D view of the situation). The frustum is a 
                // set of planes defining a volume being occluded. The property of this occlusion volume is that any 
                // point lying behind all planes of this volume is inside of the volume, and thus is occluded. 
                // So in order to define an occlusion volume, we just need a set of planes forming the occlusion volume.

                //Looking closer, we can see that the frustum is made of all of the occluder’s polygons facing the
                // viewer, and from new planes made of edges and the viewer’s position. So we will do the following:

                //1. Add planes of all facing polygons of the occluder.
                int count;
                if (contourPlanesOnly)
                    count = _edges.Count;
                else count = _facingTris.Count + _edges.Count;

                _planes = new Plane[count];
                if (!contourPlanesOnly)
                {
                    for (int i = 0; i < _facingTris.Count; i++)
                    {
                        // TODO: As an optimization, should combine the planes of facing triangles that lay within the same plane themselves.
                        // (see paragraph comments below)
                        _planes[i] = new Plane(_facingTris[i]);
                        // we flip the normal so that the planes face inward 
                        // this way it matches the ViewFrustum planes creation so they can all
                        // share the same Intersect methods.
                        _planes[i].Negate();
                    }
                }

                //2. Construct planes from the two points of each edge and the view-er’s position.
                for (int i = 0; i < _edges.Count; i++)
                {
                    int j;
                    if (!contourPlanesOnly)
                        j = _facingTris.Count + i;
                    else j = i;

                    _planes[j] = new Plane(_edges[i].Point[0], _edges[i].Point[1], camPos);
                    _planes[j].Negate();
                }

                //If you’ve gotten this far and it’s all working for you, there’s one useful optimization to implement 
                // at this point. It lies in minimizing the number of facing planes (which will speed up intersection 
                // detection). You may achieve this by collapsing all the facing planes into a single plane, with a 
                // normal made of the weighted sum of all the facing planes. Each participating normal is weighted by 
                // the area of its polygon. Finally, the length of the computed normal is made unit-length. The d part 
                // of this plane is computed using the farthest contour point. Occlusion testing will work well without 
                // this optimization, but implementing it will speed up further computations without loss of accuracy. 

                // TODO: another optimization during the cullling process is to skip occlusion tests for volumes that 
                // do not take up alot of room?  (though for things like castles that are far away, would we want to use
                // occlusion for thigns inside the walls or LOD at that point and just not render things inside because they
                // are too far away?
            }
            _isDirty = false;
        }

        public Vector3d[] EdgeVertices()
        {
            Vector3d[] verts = new Vector3d[_edges.Count*2];
            int k = 0;
            for (int i = 0; i < _edges.Count; i++)
            {
                verts[k] = _edges[i].Point[0];
                verts[k + 1] = _edges[i].Point[1];
                k += 2;
            }
            return verts;
        }

        private void UpdateEdges(Line3d e)
        {
            foreach (Line3d edge in _edges)
            {
                if (edge == e)
                {
                    _edges.Remove(edge);
                    return;
                }
            }
            _edges.Add(e);
        }

#if DEBUG
        // debug visual aids.
        public override void Draw()
        {
            const double NORMAL_LENGTH = 10;
            if ((Planes == null) || (Planes.Length == 0)) return;
            foreach (Plane p in Planes)
            {
                // draw  lines connecting all the vertices.  
                CoreClient._CoreClient.Screen2D.Draw_Line3D((float)p.Points[0].x, (float)p.Points[0].y, (float)p.Points[0].z,
                                                (float) p.Points[1].x,
                                                (float) p.Points[1].y, (float) p.Points[1].z);
                CoreClient._CoreClient.Screen2D.Draw_Line3D((float)p.Points[1].x, (float)p.Points[1].y, (float)p.Points[1].z,
                                                (float) p.Points[2].x,
                                                (float) p.Points[2].y, (float) p.Points[2].z);
                CoreClient._CoreClient.Screen2D.Draw_Line3D((float)p.Points[2].x, (float)p.Points[2].y, (float)p.Points[2].z,
                                                (float) p.Points[0].x,
                                                (float) p.Points[0].y, (float) p.Points[0].z);

            //    DebugDraw.DrawLine(Triangle.getCenter(p.Points[0], p.Points[1], p.Points[2]), p.Normal, NORMAL_LENGTH);
            }
        }
#endif
    }
}