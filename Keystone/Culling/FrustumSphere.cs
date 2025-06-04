using System;
using Keystone.Types;
using Keystone.Elements;

namespace Keystone.Culling
{
    class FrustumSphere
    {
        BoundingSphere _sphere;

        // TODO: all i really need is a constructor in BoundingSphere to create from frustum
        //       which in fact i do call below already!
        public void Update(Cameras.Camera camera, float near, float far, float fovRadians, Vector3d cameraPosition, Vector3d cameraLookAt, Matrix view, Matrix projection, float viewportWidth, float viewportHeight)
        {

            // TODO: here presuming the near, far, fov have not changed, we should be able to
            // upddate the previous sphere by translating it
            _sphere = new BoundingSphere(near, far, fovRadians, cameraPosition, cameraLookAt);

        }

        public bool IsVisible(Geometry mesh)
        {
            bool result = _sphere.Intersects(mesh.BoundingSphere) != IntersectResult.OUTSIDE;  // see notes in Sphere.Intersect for difference between Sphere.Contains()

            return result;
        }

        //public override bool IsVisible(BoundingBox box)
        //{
        //    IntersectResult result = _sphere.Intersects(box);

        //    return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        //}

        public bool IsVisible(BoundingSphere sphere)
        {
            IntersectResult result = _sphere.Intersects(sphere);

            return result == IntersectResult.INTERSECT || result == IntersectResult.INSIDE;
        }

    }
}
