using System;
using Keystone.Types;
using Keystone.Elements;

namespace Keystone.Culling
{
    class FrustumCone
    {
        BoundingCone _cone;

        // TODO: really all we need is a BoundingCone.CreateFromFrustum() 
        // which is actually what i do already!
        public void Update(Cameras.Camera camera, float near, float far, float fovRadians, 
                           Vector3d cameraPosition, Vector3d cameraLookAt, 
                           Matrix view, Matrix projection, 
                           float viewportWidth, float viewportHeight)
        {
            _cone = new BoundingCone(fovRadians, cameraPosition, cameraLookAt, viewportWidth, viewportHeight);
        }

        public bool IsVisible(Geometry mesh)
        {
            bool result = _cone.Intersects(mesh.BoundingSphere) != IntersectResult.OUTSIDE;
            return result;
        }

        //public override bool IsVisible(BoundingBox box)
        //{
        //    IntersectResult result = _cone.Intersects(box);

        //    return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        //}

        public bool IsVisible(BoundingSphere sphere)
        {
            IntersectResult result = _cone.Intersects(sphere);

            return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        }
    }
}
