namespace Keystone.Types
{
    ///
    /// Ray class, for use with the optimized ray-box intersection test
    /// described in:
    ///
    ///      Amy Williams, Steve Barrus, R. Keith Morley, and Peter Shirley
    ///      "An Efficient and Robust Ray-Box Intersection Algorithm"
    ///      Journal of graphics tools, 10(1):49-54, 2005
    /// 
    /// http://www.realtimerendering.com/intersections.html
    public class Ray
    {
        public Vector3d Origin;
        public Vector3d Direction;
        
        // these two members are only used by BoundingBox.Intersects() - i could compute them there... it's only
        // 1 divide and 3 compares
        public Vector3d InverseDirection;
        public int[] Sign = new int[3];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="dir">Expects a normalized direction</param>
        public Ray(Vector3d orig, Vector3d dir)
        {
            Origin = orig;
            Direction = Vector3d.Normalize (dir);

            //InverseDirection = 1d / Direction ; // <-- this produces the wrong result.  And this is the only place we ever called that op_Divide overloaded function.
            // March.11.2024 - NOTE: we don't want to prevent divide by 0 here
            //                 as we did in the operator overloaded op_Divide.
            //                 This breaks the BoundingBox.Inversects(r) test
            InverseDirection.x = 1d / Direction.x;
            InverseDirection.y = 1d / Direction.y;
            InverseDirection.z = 1d / Direction.z;

            Sign[0] = (InverseDirection.x < 0d) ? 1 : 0;
            Sign[1] = (InverseDirection.y < 0d) ? 1 : 0;
            Sign[2] = (InverseDirection.z < 0d) ? 1 : 0;
        }

        // clone
        public Ray(Ray r)
        {
            Origin = r.Origin;
            Direction = r.Direction;
            Sign[0] = r.Sign[0];
            Sign[1] = r.Sign[1];
            Sign[2] = r.Sign[2];
        }

        /// <summary>
        ///  typically used to return a ray that is transformed to modelspace
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Ray Transform(Matrix m)
        {
            Vector3d newOrig, newDir;
                       
            // Transform ray origin and direction by matrix
            newOrig = Vector3d.TransformCoord(Origin, m);
            newDir = Vector3d.TransformNormal(Direction, m);
            return new Ray (newOrig, Vector3d.Normalize(newDir));
        }
    }
}