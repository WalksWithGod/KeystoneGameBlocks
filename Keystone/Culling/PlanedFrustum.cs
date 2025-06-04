using System;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Culling
{
    // when using a loose bound test such as sphere test, an object seemingly partially 
    // within the volume (for frustum) or outside (for occlusion volume)
    // may not be since a sphere bounding volume doesnt fit very well.  Therefore 
    // only in cases where you have very complex geometry would it be worth the extra effort
    // to try and do a aabb test afterwards.  Otherwise its best to treat "UKNOWN" as visible and just render it

    public abstract class PlanedFrustum
    {
        protected Plane[] _planes;
        protected bool _testAllPoints = false;
        protected bool[] _enabledPlanes;

        public Plane[] Planes
        {
            get { return _planes; }
        }

        public bool[] EnabledPlanes
        {
            get { return _enabledPlanes;}
            set {_enabledPlanes = value;}
        }

        public bool TestAllPoints
        {
            get { return _testAllPoints; }
            set { _testAllPoints = value; }
        }


        //public void Translate(Vector3d translation)
        //{
        //    if (translation.IsNullOrEmpty()) return;
        //    foreach (Plane p in _planes)
        //    {
        //        p.Translate(translation);
        //    }
        //}

        // TODO: for OBB see this URL
        // http://www.gamedev.net/community/forums/topic.asp?topic_id=539116
        // frustum planes tested against mesh box
        public IntersectResult Intersects(BoundingBox box)
        {
            return Intersects(box.Vertices);
        }

        // TODO: I had pasted the following because it seemed advocated, but its far less flexible
        // then my current Intersects (Vector3d[] vertices) 
        //public bool IsBBoxVisible(BoundingBox b)
        //{

        //    if ((Math.Abs(b.Mid.x - Pos.x) < b.Size.x) &&
        //        (Math.Abs(b.Mid.y - Pos.y) < b.Size.y) &&
        //        (Math.Abs(b.Mid.z - Pos.z) < b.Size.z))
        //        return true;

        //    for (int i = 0; i < 6; i++)
        //    {
        //        float m = b.Mid.x * planes[i].x + b.Mid.y * planes[i].y + b.Mid.z * planes[i].z + planes[i].w;
        //        float n = b.Size.x * absPlanes[i].x + b.Size.y * absPlanes[i].y + b.Size.z * absPlanes[i].z;
        //        if (m > n) return false;
        //    }

        //    return true;
        //}

        // if all corners of the bounding box are inside every plane of the frustum, this item is FULLY visible
        // NOTE: When this method is called by the OcclusionFrustum.IsVisible rather than VIewFrustum.IsVisible
        // it treats true returnd from here as being NOT visible since its fully within the bounds of the frustum.
        // (i.e. opposite of how ViewFrustum interprets true)
        public IntersectResult Intersects(Vector3d[] vertices)
        {
           
            int totalIn = 0;
            for (int j = 0; j < _planes.Length; j++)
            {
                if (!_enabledPlanes[j]) // if the plane is NOT enabled, then the vertex is assumed inside
                {
                    totalIn ++;
                    continue;
                }

                int cornersIn = 0;
                for (int i = 0; i < vertices.Length; i++)
                {
                    double distance = Plane.DistanceToPlane(vertices[i], _planes[j]);

                    if (distance >= 0.0)
                    {
                        cornersIn++;
                    }
                }
                
                // if all corners are behind any single plane, this item is fully NOT visible.
                if (cornersIn == vertices.Length)
                    totalIn++;
                else if (cornersIn == 0)
                    return IntersectResult.OUTSIDE;
            }
            return totalIn == _planes.Length ? IntersectResult.INSIDE : IntersectResult.INTERSECT;
        }

        // frustum planes tested against mesh sphere. NOTE: I think my 
        // frustum planes normals are reversed (they are pointing outwards) and so
        // we're testing > instead of < as seen in so mahy online samples
        public IntersectResult Intersects(BoundingSphere sphere)
        {
            for (int i = 0; i < _planes.Length; i++)
            {
                if (!_enabledPlanes[i]) 
                {
                    continue;
                }

                double distance = Plane.DistanceToPlane(sphere.Center, _planes[i]);

                // if the distance from the center of the sphere to any single plane
                // is < -sphere.radius we are definetly outside
                if (distance < -sphere.Radius)
                    return IntersectResult.OUTSIDE; // Sphere is completely outside and thus not visible

                // if the distance from the center of the sphere to the plane is within +-radius the sphere intersects
                // which means this mesh POTENTIALLY is visible since bounding sphere isnt the best fit around the mesh
                // but we'll assume its good enough and Return early
                if (Math.Abs(distance) < sphere.Radius)
                    return IntersectResult.INTERSECT;
            }
            return IntersectResult.INSIDE;
        }

#if DEBUG
        public abstract void Draw();
#endif
        // TODO: why IsVisible vs Intersects overloads?
        // or else why arent Intersects private members?
        public abstract bool IsVisible(Geometry mesh);

        public abstract bool IsVisible(BoundingBox box);

        public abstract bool IsVisible(BoundingSphere sphere);
    }
}