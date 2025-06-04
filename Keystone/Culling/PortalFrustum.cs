using System;
using System.Diagnostics;
using Keystone.Cameras;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Culling
{
    public class PortalFrustum : PlanedFrustum
    {
        public PortalFrustum(Vector3d cameraPosition, Vector3d[] portalVerts)
        {
            Trace.Assert(portalVerts.Length == 4, "Invalid portal vertex count.  Expected 4.");

            _planes = new Plane[4];
            _planes[0] = new Plane(portalVerts[0], portalVerts[3], cameraPosition); // left plane
            _planes[1] = new Plane(portalVerts[3], portalVerts[2], cameraPosition); // bottom plane
            _planes[2] = new Plane(portalVerts[2], portalVerts[1], cameraPosition); // right plane
            _planes[3] = new Plane(portalVerts[1], portalVerts[0], cameraPosition); // top plane
            _enabledPlanes = new bool[4];
            _enabledPlanes[0] = true;
            _enabledPlanes[1] = true;
            _enabledPlanes[2] = true;
            _enabledPlanes[3] = true;

            // _position = cameraPosition;
        }

#if DEBUG
        public override void Draw()
        {
        }
#endif

        public override bool IsVisible(Elements.Geometry mesh)
        {
            return IsVisible(mesh.BoundingBox);
        }

        public override bool IsVisible(BoundingBox box)
        {
            IntersectResult result = Intersects(box);

            return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        }

        public override bool IsVisible(BoundingSphere sphere)
        {
            IntersectResult result = Intersects(sphere);

            return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        }
    }
}