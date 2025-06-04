//#define DX_FRUSTUM 

// ViewFrustum.cs
// Written By Michael P. Joseph (Hypnotron)
// Code adapted from the article "Frustum Culling" by Dion Picco
// http://www.flipcode.com/articles/article_frustumculling.shtml
using System;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Culling
{
    // a custom view frustum to replace calls to TVCamera.IsBoxVisible & .IsSphereVisible.
    // whereas an OcclusionFrustum must consist of planes, a view frustum can be made
    // from planes but also (for faster visibility testing) from a sphere or cone.
    public class ViewFrustum : PlanedFrustum
    {

        public ViewFrustum()
        {
            _planes = new Plane[6];
            _enabledPlanes = new bool[6] { true, true, true, true, true, true };
        }

        public ViewFrustum Clone()
        {
            ViewFrustum clonedCopy = new ViewFrustum();
            clonedCopy._planes[0] = _planes[0];
            clonedCopy._planes[1] = _planes[1];
            clonedCopy._planes[2] = _planes[2];
            clonedCopy._planes[3] = _planes[3];
            clonedCopy._planes[4] = _planes[4];
            clonedCopy._planes[5] = _planes[5];

            clonedCopy._enabledPlanes[0] = _enabledPlanes[0];
            clonedCopy._enabledPlanes[1] = _enabledPlanes[1];
            clonedCopy._enabledPlanes[2] = _enabledPlanes[2];
            clonedCopy._enabledPlanes[3] = _enabledPlanes[3];
            clonedCopy._enabledPlanes[4] = _enabledPlanes[4];
            clonedCopy._enabledPlanes[5] = _enabledPlanes[5];

            clonedCopy._testAllPoints = _testAllPoints;
            return clonedCopy;
        }

        /// <summary>
        /// Clones the source ViewFrustum's private variable data to the target Frustums.
        /// This way a new Frustum doesn't need to be created for the target an existing old
        /// one from a cache of available old frustums can be recycled.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Clone(ViewFrustum source, ViewFrustum target)
        { 
        }

#if DEBUG
        // TODO: was for debugging but never implemented...
        public override void Draw()
        {
            //foreach (PLANE p in Planes)
            //{

            //}
        }
#endif

        public static Vector3d[] GetCorners(double nearDepth, double farDepth, double fovRadians, int viewportWidth, int viewportHeight)
        {
            // all 8 corners of the frustum
            Vector3d[] near = GetCorners(nearDepth, fovRadians, viewportWidth, viewportHeight);
            Vector3d[] far = GetCorners(farDepth, fovRadians, viewportWidth, viewportHeight);

            return new Vector3d[] { near[0], near[1], near[2], near[3], far[0], far[1], far[2], far[3] };
        }

        public static Vector3d[] GetCorners(double depth, double fovRadians, int viewportWidth, int viewportHeight)
        {
            // only 4 corners of a plane
            Vector3d[] corners = new Vector3d[4];


            // get the width and height of near plane
            double nearHeight = 2d * Math.Tan(fovRadians / 2d) * depth;
            double nearWidth = nearHeight * viewportWidth / viewportHeight;

            Vector3d center = Vector3d.Forward() * depth;

            // bottom left corner
            corners[0].x = center.x - (nearWidth / 2d);
            corners[0].y = center.y - (nearHeight / 2d);
            corners[0].z = center.z;

            // bottom right corner
            corners[1].x = -corners[0].x;
            corners[1].y = corners[0].y;
            corners[1].z = center.z;

            // top left corner
            corners[2].x = corners[0].x;
            corners[2].y = center.y + (nearHeight / 2d);
            corners[2].z = center.z;

            // top right corner
            corners[3].x = -corners[2].x;
            corners[3].y = corners[2].y;
            corners[3].z = center.z;

            // NOTE: corner does not take into account View/InverseView.
            //       So it assumes 0,0,0 origin and 0,0,0 rotation
            return corners;
        }

        public static Vector3d GetFixedNearPlanePoint(double fovRadians, int viewportWidth, int viewportHeight, double percentWidth, double percentHeight)
        {
        	            // we can use a DEPTH of 1.0 because our axis indicator fits inside unit sphere
            double depth = 1.5d;
            // get the 4 corners of the frustum at the specified z depth;
            Vector3d[] frustumCorners = GetCorners(depth, fovRadians, viewportWidth, viewportHeight);

            double planeWidth = frustumCorners[1].x - frustumCorners[0].x;
            double planeHeight = frustumCorners[2].y - frustumCorners[0].y;

            // WRONG! DO NOT CALL UNPROJECT!  Our .GetCorners(depth) call 
            // this.Context.Viewport.UnProject ()
            // already gives us the mix and max bounds at the specified depth!  
            Vector3d v = frustumCorners[0];

            // as long as we now position at fixed percentage of width and height
            // the item will appear to be anchored because as the width and height
            // scale up or down, so does the size of the object at the same percentage.
            v.x += planeWidth * percentWidth; // eg 0.05d == over 5% from left
            v.y += planeHeight * percentHeight; // eg 0.05d = up 5% from bottom

            return v;
        }
                
        public void Update(float near, float far, float fovRadians, Vector3d cameraPosition, Vector3d cameraLookAt, Matrix view, Matrix projection, float viewportWidth, float viewportHeight)
        {
            _planes = CreateFrustumPlanes(view, projection, true);
            //_planes = CreateFrustumPlanes(camera);
        }

        public override bool IsVisible(Geometry mesh)
        {
            bool result = IsVisible(mesh.BoundingBox);
            return result;
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


#if (DX_FRUSTUM)
    // This version works.  Not sure if its faster than the below method.
    // you may get exceptions from DX on first call... to disable in VS.NET ide
    //Debug\Exceptions\Managed Debugging Assistants\LoaderLock thrown = off
        private Plane[] CreateFrustumPlanes(bool normalize)
        {
            Microsoft.DirectX.Matrix m, v, p;

            Microsoft.DirectX.Direct3D.Device device =
                new Microsoft.DirectX.Direct3D.Device(Core._Core.Internals.GetDevice3D());
            v = device.GetTransform(Microsoft.DirectX.Direct3D.TransformType.View);
            p = device.GetTransform(Microsoft.DirectX.Direct3D.TransformType.Projection);
            m = Microsoft.DirectX.Matrix.Multiply(v, p);

            Plane[] planes = new Plane[6];
            // left clipping plane
            planes[0] = new Plane(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41);

            // right clipping plane
            planes[1] = new Plane(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41);

            // top clipping plane
            planes[2] = new Plane(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42);

            // bottom clippingplane
            planes[3] = new Plane(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42);

            // near clipping plane
            planes[4] = new Plane(m.M13, m.M23, m.M33, m.M43);

            // far clipping plane
            planes[5] = new Plane(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43);

            if (normalize)
            {
                Plane.Normalize(planes[0]);
                Plane.Normalize(planes[1]);
                Plane.Normalize(planes[2]);
                Plane.Normalize(planes[3]);
                Plane.Normalize(planes[4]);
                Plane.Normalize(planes[5]);
            }
            return planes;
        }

        // This version works.  Not sure if its faster than the below method.
        // you may get exceptions from DX on first call... to disable in VS.NET ide
        //Debug\Exceptions\Managed Debugging Assistants\LoaderLock thrown = off
        private Plane[] CreateFrustumPlanes(Camera camera, bool normalize)
        {
            Matrix m, m2, m3;
            Microsoft.DirectX.Direct3D.Device device =
                new Microsoft.DirectX.Direct3D.Device(Core._Core.Internals.GetDevice3D());
            m2 = new Matrix(device.GetTransform(Microsoft.DirectX.Direct3D.TransformType.View));

            m3 = new Matrix(device.GetTransform(Microsoft.DirectX.Direct3D.TransformType.Projection));
            m = Matrix.Multiply(m2 , m3);

            Plane[] planes = new Plane[6];
            // left clipping plane
            planes[0] = new Plane(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41);

            // right clipping plane
            planes[1] = new Plane(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41);

            // top clipping plane
            planes[2] = new Plane(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42);

            // bottom clippingplane
            planes[3] = new Plane(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42);

            // near clipping plane
            planes[4] = new Plane(m.M13, m.M23, m.M33, m.M43);

            // far clipping plane
            planes[5] = new Plane(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43);

            if (normalize)
            {
                Plane.Normalize(planes[0]);
                Plane.Normalize(planes[1]);
                Plane.Normalize(planes[2]);
                Plane.Normalize(planes[3]);
                Plane.Normalize(planes[4]);
                Plane.Normalize(planes[5]);
            }
            return planes;
        }
#else

        //private Plane[] CreateFrustumPlanes(Camera camera)
        //{
        //    Plane[] planes = new Plane[6];
        //    TV_PLANE[] tvplanes = new TV_PLANE[6];
        //    camera.TVCamera.GetFrustumPlanes(ref tvplanes[0]);

        //    for (int i = 0; i < 6; i++)
        //    {
        //        //Helpers.TVTypeConverter.FromTVPlane(tvplanes[i]);
        //        planes[i] = new Plane(Helpers.TVTypeConverter.FromTVVector(tvplanes[i].Normal), tvplanes[i].Dist);
        //    }
        //    return planes;
        //}


        //private Plane[] CreateFrustumPlanes(Camera camera, bool normalize)
        //{
        //    Plane[] planes = new Plane[6];
            
        //    // Matrix.Inverse() results in inversion errors!  UGH!
        //    Matrix m2 = Matrix.Inverse(camera.Matrix);
        //    //Matrix m = m2*camera.Projection;
        //    Matrix m = Matrix.MultiplyFull(m2, camera.Projection);
        //    //Matrix m = Matrix.MultiplyFull(camera.View, camera.Projection); // for some reason trying to use the View to avoid having to inverse the invViewMatrix doesnt work.  Orothograhic culling breaks at borders of viewport

        //    // left clipping plane
        //    planes[0] = new Plane(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41);

        //    // right clipping plane
        //    planes[1] = new Plane(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41);

        //    // top clipping plane
        //    planes[2] = new Plane(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42);

        //    // bottom clippingplane
        //    planes[3] = new Plane(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42);

        //    // near clipping plane
        //    planes[4] = new Plane(m.M13, m.M23, m.M33, m.M43);

        //    // far clipping plane
        //    planes[5] = new Plane(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43);

        //    //  normalize if requested
        //    if (normalize)
        //    {
        //        Plane.Normalize(planes[0]);
        //        Plane.Normalize(planes[1]);
        //        Plane.Normalize(planes[2]);
        //        Plane.Normalize(planes[3]);
        //        Plane.Normalize(planes[4]);
        //        Plane.Normalize(planes[5]);
        //    }
        //    return planes;
        //}


        /// <summary>
        /// Since the planes are built from our camera's matrices and our matrices if we are using camera space
        /// origin, then the position for the view matrix will arleady be 0,0,0 so the computed frustum planes will
        /// be in camera space as well.  In other words, there's no need to modify this function to support both.
        /// It simply accepts whatever values are in teh camera matrix.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        private Plane[] CreateFrustumPlanes(Matrix view, Matrix projection, bool normalize)
        {
            Plane[] planes = new Plane[6];

            // must use 4x4 matrix multiplication with the projection matrix
            Matrix m = Matrix.Multiply4x4(view, projection);

            // left clipping plane
            planes[0] = new Plane(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41);
            //planes[0] = new Plane(-m.M14 - m.M11, -m.M24 - m.M21, -m.M34 - m.M31, -m.M44 - m.M41);

            // right clipping plane
            planes[1] = new Plane(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41);
            //planes[1] = new Plane(-m.M14 + m.M11, -m.M24 + m.M21, -m.M34 + m.M31, -m.M44 + m.M41);

            // top clipping plane
            planes[2] = new Plane(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42);
            //planes[2] = new Plane(-m.M14 + m.M12, -m.M24 + m.M22, -m.M34 + m.M32, -m.M44 + m.M42);

            // bottom clippingplane
            planes[3] = new Plane(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42);
            //planes[3] = new Plane(-m.M14 - m.M12, -m.M24 - m.M22, -m.M34 - m.M32, -m.M44 - m.M42);

            // near clipping plane
            planes[4] = new Plane(m.M13, m.M23, m.M33, m.M43);
            //planes[4] = new Plane(-m.M13, -m.M23, -m.M33, -m.M43);

            // far clipping plane
            planes[5] = new Plane(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43);
            //planes[5] = new Plane(-m.M14 + m.M13, -m.M24 + m.M23, -m.M34 + m.M33, -m.M44 + m.M43);

            //  normalize if requested seems to screw things up..
            if (normalize)
            {
                planes[0] = Plane.Normalize(planes[0]);
                planes[1] = Plane.Normalize(planes[1]);
                planes[2] = Plane.Normalize(planes[2]);
                planes[3] = Plane.Normalize(planes[3]);
                planes[4] = Plane.Normalize(planes[4]);
                planes[5] = Plane.Normalize(planes[5]);
            }

            return planes;
        }
#endif
        // clipplanes are something completely different, doesnt belong here.
        //private PLANE[] CreateFrustumPlanes3()
        //{
        //    Microsoft.DirectX.Direct3D.Device device =
        //        new Microsoft.DirectX.Direct3D.Device(Core._CoreClient.Internals.GetDevice3D());
        //    Microsoft.DirectX.Direct3D.ClipPlanes clipplanes; //= new Microsoft.DirectX.Direct3D.ClipPlanes;
        //    PLANE[] planes = new PLANE[6];

        //    clipplanes = device.ClipPlanes;
        //    for (int i = 0; i < 6; i++)
        //    {
        //        double[] coefficients = clipplanes[i].GetSingleArray();

        //        planes[i] = new PLANE(coefficients[0], coefficients[1], coefficients[2], coefficients[3]);
        //    }
        //    return planes;
        //}
    }
}