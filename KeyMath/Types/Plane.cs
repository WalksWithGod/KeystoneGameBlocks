
using System;

namespace Keystone.Types
{
    // This wrapper has one constructor that allows us to
    // retain the points used to create the plane from the 2nd and 3rd constructors.
    // NOTE: yeah its not that great because _points is null for all other cases...
    // in the future it might be possible to initialize 3 other points on the planes for all cases....
    // but for me right now, that's not useful so i wont bother...
    public class Plane // faster as a class than a struct
    {
        private Vector3d[] _points;
        private Vector3d _normal;
        private double _distance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normal">Expects a unit normal</param>
        public Plane(Vector3d point, Vector3d normal)
        {
            _points = null;
            _normal = normal; // normal must be unit normal
            _distance = point.Length;
            //_distance = Vector3d.DotProduct(normal, point);
            //if (_distance == 0.0)
            //    System.Diagnostics.Debug.WriteLine("problem here");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normal">Expects a unit normal</param>
        /// <param name="distance">distance to the origin</param>
        public Plane(Vector3d normal, double distance) : this(normal.x, normal.y, normal.z, distance)
        {
        }

        /// <summary>
        /// Excepts a unit normal's components and distance from the origin.
        /// </summary>
        /// <param name="normalX"></param>
        /// <param name="normalY"></param>
        /// <param name="normalZ"></param>
        /// <param name="distance">distance to the origin</param>
        public Plane(double normalX, double normalY, double normalZ, double distance)
        {
            _distance = distance;
            _normal.x = normalX;
            _normal.y = normalY;
            _normal.z = normalZ;
            _points = null;
            
        }

        // our retained points version of the constructor
        public Plane(Vector3d p1, Vector3d p2, Vector3d p3)
        {
            _points = new Vector3d[3];
            _points[0] = p1;
            _points[1] = p2;
            _points[2] = p3;

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // TODO: is the p1 - p2 and p1 - p3 correct order?  somehow i broke my culling and i have the
            // scaleculler visibility test forcing return true always
            // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            Vector3d edge1 = p2 - p1;
            Vector3d edge2 = p3 - p1 ;
            _normal = Vector3d.CrossProduct(edge1, edge2);
            // TODO: should we normalize the normal?
            _normal.Normalize(); 
            _distance = -Vector3d.DotProduct( _normal, p1);


            //// slim dx 
            //double x1 = p2.x - p1.x;
            //double y1 = p2.y - p1.y;
            //double z1 = p2.z - p1.z;
            //double x2 = p3.x - p1.x;
            //double y2 = p3.y - p1.y;
            //double z2 = p3.z - p1.z;
            //double yz = (y1 * z2) - (z1 * y2);
            //double xz = (z1 * x2) - (x1 * z2);
            //double xy = (x1 * y2) - (y1 * x2);
            //double invPyth = 1.0d / (Math.Sqrt((yz * yz) + (xz * xz) + (xy * xy)));
            //_normal.x = yz * invPyth;
            //_normal.y = xz * invPyth;
            //_normal.z = xy * invPyth;
            //_distance = -((_normal.x * p1.x) + (_normal.y * p1.y) + (_normal.z * p1.z)); 
        }

        public Plane(Triangle tri)
            : this(tri.Points[0], tri.Points[1], tri.Points[2])
        {
        }

        public Vector3d[] Points
        {
            get { return _points; }
        }

        public Vector3d Normal
        {
             get { return _normal; } 
        }

        public void Translate(Vector3d translation)
        {
            _distance -=  Vector3d.DotProduct(Normal, translation);
            if (_points != null)
                for (int i = 0; i < _points.Length; i++)
                    _points[i] -= translation;

        }

        /// <summary>
        /// Plane must already be normalized before transforming it.
        /// </summary>
        /// <param name="matrix"></param>
        public void Transform (Matrix matrix)
        {
            Matrix matrix1 = Matrix.Inverse(matrix);

            double single4 = this._normal.x;
            double single3 = this._normal.y;
            double single2 = this._normal.z;
            double single1 = this._distance;
         
            _normal.x = (((single4 * matrix1.M11) + (single3 * matrix1.M12)) + (single2 * matrix1.M13)) + (single1 * matrix1.M14);
            _normal.y = (((single4 * matrix1.M21) + (single3 * matrix1.M22)) + (single2 * matrix1.M23)) + (single1 * matrix1.M24);
            _normal.z = (((single4 * matrix1.M31) + (single3 * matrix1.M32)) + (single2 * matrix1.M33)) + (single1 * matrix1.M34);
            _distance = (((single4 * matrix1.M41) + (single3 * matrix1.M42)) + (single2 * matrix1.M43)) + (single1 * matrix1.M44);

        }

        public void Negate()
        {
            _normal = Vector3d.Negate(_normal);
            // note; distance is never negated.  Distance is an absolute value and the normal gives us the direction.
        }

        public double Distance //distance from the origin
        {
            get { return _distance; } 
        }

        public Vector3d Origin
        {
            // NOTE: _normal * -_distance seems correct. It works for gizmo and waypoint placer
            //       where we create plane from a normal and distance and it works for picking
            //       celledregion grid squares where that plane is created using 3 points.
            get { return _normal * -_distance; }
        }
        
        public double DistanceToCoordinate (Vector3d coord)
        {
            return DistanceToPlane(coord, this);
        }

        public static double DistanceToPlane(Vector3d coord, Plane plane)
        {
            return Vector3d.DotProduct(coord, plane.Normal) + plane.Distance;
        }

        public static Plane Normalize(Plane plane)
        {
            double mag;
            Vector3d normal = Vector3d.Normalize(plane.Normal, out mag);
            return new Plane(normal.x, normal.y, normal.z, plane.Distance / mag);
        }

        #region Broken and Obsolete.  New implementation far superior
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="r"></param>
        ///// <param name="p"></param>
        ///// <param name="distance"></param>
        ///// <returns>Oct.22.2010 verified the main (non Points[] version) is equivalent to the XNA counterpart.</returns>
        //public static bool Intersects(Ray r, Plane p, ref double distance)
        //{
        //    const double epsilon = double.Epsilon; // .0001d; // strictly speaking this can be 0 but trying to add margin for floating point precision issues
        //    double d = -p.Distance; // -Vector3d.DotProduct(p.Normal, p.Points[0]); 
        //    if (p.Points != null)
        //    {
        //        //TODO: i commented the below distance calc in favor of just p.Distance 
        //        // because of debugging ScalingManipulator dragging the scaling tabs.  Verify culling, occlusion creation, portals, etc all ok.
        //        // it may be as simple as changing the sign
        //        double d2 = -Vector3d.DotProduct(p.Normal, p.Points[0]);
        //        double dist = System.Math.Abs(d - d2);
        //        System.Diagnostics.Trace.Assert(dist < .01);
        //    }
        //    double denom = Vector3d.DotProduct(p.Normal, r.Direction);
        //    double numer = d - Vector3d.DotProduct(p.Normal, r.Origin);

        //    if (denom <= epsilon) // normal is orthogonal to vector, cant intersect
        //        return false;

        //    distance = -numer / denom;
        //    return true;

        //}
#endregion 

        /// <summary>
        /// Finds the line that defines an intersection of two planes 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool Intersects(Plane plane, ref Line3d intersection)
        {
            throw new NotImplementedException();
            return false;
        }

        public bool Intersects(Ray r, double rayScale, ref double distance)
        {
            Vector3d intersectionPoint;
            intersectionPoint.x = 0;
            intersectionPoint.y = 0;
            intersectionPoint.z = 0;
            return Intersects(r, this, rayScale, ref distance, ref intersectionPoint);
        }

        public bool Intersects(Ray r, double rayScale, ref double distance, ref Vector3d intersectionPoint)
        {
            return Intersects(r, this, rayScale, ref distance, ref intersectionPoint);
        }
        
        /// <summary>
        /// Finds intersection between ray and plane if it exists.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        /// <param name="intersectionPoint"></param>
        /// <remarks>
        /// Copyright 2001, softSurfer (www.softsurfer.com)
        /// This code may be freely used and modified for any purpose
        /// providing that this copyright notice is included with it.
        /// SoftSurfer makes no warranty for this code, and cannot be held
        /// liable for any real or imagined damage resulting from its use.
        /// Users of this code must verify correctness for their application.
        /// http://www.softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm
        /// http://code.google.com/p/slimmath/source/browse/trunk/SlimMath/Collision.cs
        /// </remarks>
        /// <returns></returns>
        public static bool Intersects(Ray r, Plane p, double rayScale, ref double distance, ref Vector3d intersectionPoint)
        {
            bool result = false;

            Vector3d rayDestination = r.Origin + (r.Direction * rayScale);
            Vector3d u = rayDestination - r.Origin;
            Vector3d w = r.Origin - p.Origin; // TODO: make sure this still works.
                                              // used to be p.Points[0] but now i compute p.Origin
                                              // and im not 100% sure that the p.Origin is caled properly
                                              // or whether p.Points[0] can be replaced with origin point (but i dont see why not)

            double D = Vector3d.DotProduct(p.Normal, u);
            double N = -Vector3d.DotProduct(p.Normal, w);

            // segment is parallel to plane 
            if (System.Math.Abs(D) < double.Epsilon) 
            {
                if (N == 0)                     // segment lies in plane
                    result = true;              
                else
                    result = false;                   // no intersection
            }
            // they are not parallel compute intersect param
            else  
            {
                const double zeroEpsilon = double.Epsilon;
                
                double sI = N / D;
                if (sI < 0 || sI > 1) // on wrong side of face no intersection
                    result = false;                       
                else
                {
                    // compute segment intersect point
                    Vector3d scaledDirection = sI * u;
                    intersectionPoint = r.Origin + scaledDirection; 
                    distance = scaledDirection.Length;
                    result = true;
                }
            }
            return result;
        }

        public static Vector3d IntersectionPoint(Ray r, double distance)
        {
            return r.Origin + (r.Direction*distance);
        }

        /// <summary>
        /// Utility function that returns the origin plane whose normal is perpendicular to the 
        /// vectors of the specified axes if multiple axes are specified, or the plane
        /// whose normal is the unit vector of the specified axis if a single axis is specified
        /// </summary>
        /// <param name="axis">The axes for which to retrieve the corresponding plane</param>
        /// <returns>The origin plane that corresponds to the specified axes</returns>
        public static Plane GetPlane(Vector3d origin, AxisFlags axis)
        {
            Vector3d normal;
            normal.x = 0;
            normal.y = 0;
            normal.z = 0;

            switch (axis)
            {
                case AxisFlags.X:
                case AxisFlags.Y | AxisFlags.Z:
                    normal.x = 1;
                    break;

                case AxisFlags.Y:
                case AxisFlags.X | AxisFlags.Z:
                    normal.y = 1;
                    break;

                case AxisFlags.Z:
                case AxisFlags.X | AxisFlags.Y:
                    normal.z = 1;
                    break;
            }

            return new Plane(origin, normal);
        }

        public static Plane GetPlane(AxisFlags axis)
        {
            return GetPlane(Vector3d.Zero(), axis);
        }
    }
}