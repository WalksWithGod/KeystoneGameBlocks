using System;

namespace Keystone.Types
{
    public class Polygon
    {
        protected int[] _indices;
        protected Vector3d[] _points;
        protected Vector3d _normal;

        public Polygon(Vector3d[] points)
        {
            if (points.Length < 3) throw new ArgumentException();

            _points = points;

            _normal = FaceNormal(_points[0], _points[1], _points[2]);

            // bad triangles where any two points are the same could result in exception?
            if (_points[0].Equals(_points[1]) || _points[0].Equals(_points[2]) || _points[1].Equals(_points[2]))
            {
                //throw new ArgumentException("Triangle cannot have two identicle vertices.");
            }
            _indices = null;
        }

		public Polygon(Vector3d a, Vector3d b, Vector3d c, Vector3d d)
        {
            _points = new Vector3d[4];
            _points[0] = a;
            _points[1] = b;
            _points[2] = c;
            _points[3] = d;
            
            _normal = FaceNormal(_points[0], _points[1], _points[2]);

            // bad triangles where any two points are the same could result in exception?
            if (_points[0].Equals(_points[1]) || _points[0].Equals(_points[2]) || _points[1].Equals(_points[2]))
            {
                //throw new ArgumentException("Triangle cannot have two identicle vertices.");
            }
            _indices = null;
        }
        
        public Vector3d Normal
        {
            get { return _normal; }
        }

        public int[] Indices
        {
            get { return _indices; }
        }

        public Vector3d[] Points
        {
            get { return _points; }
        }

        public Plane GetPlane()
        {
            return new Plane(_points[0], _points[1], _points[2]);
        }

        public static Vector3d FaceNormal(Vector3d v1, Vector3d v2, Vector3d v3)
        {
            //1. The two edges chosen must not be parallel, i.e. the angle between the edges must not be 0 or 180 degrees. 
            //   The normal will be more accurate if the angle between the lines is closer to 90 degrees. 
            //2. The length of the edges must be non-zero and the normal will be more accurate if the length is high compared 
            //   with the accuracy of the coordinates. 
            //3. If the angle is concave then the direction of the normal needs to be reversed.

            Vector3d a, b;

            a = v1 - v2;
            b = v2 - v3;
            return Vector3d.Normalize(Vector3d.CrossProduct(a, b));
        }

        public Polygon Transform(Matrix transform)
        {
            return Transform(this, transform);
        }

        public static Polygon Transform(Polygon p, Matrix transform)
        {
            Vector3d[] points = Vector3d.TransformCoordArray(p.Points, transform);

            Polygon result = new Polygon(points);
            return result;

        }

        public bool Intersects (Vector3d start, Vector3d end, bool skipBackFaces, out Vector3d intersectionPoint)
        {
        	return Polygon.Intersects (this._points, start, end, skipBackFaces , out intersectionPoint);
        }
        
        public bool Intersects(Ray r, double rayScale, bool skipBackFaces, out Vector3d intersectionPoint)
        {
        	return Polygon.Intersects (r, rayScale, this._points, skipBackFaces, out intersectionPoint);
        }
        
        public static bool Intersects(Polygon poly, Vector3d start, Vector3d end, bool skipBackFaces, out Vector3d intersectionPoint)
        {
            return Polygon.Intersects(poly.Points, start, end, skipBackFaces, out intersectionPoint);
        }

        public static bool Intersects(Vector3d[] points, Vector3d start, Vector3d end, bool skipBackFaces, out Vector3d intersectionPoint)
        {
            Vector3d dir = Vector3d.Normalize(end - start);
            Ray r = new Ray(start, dir);
            return Intersects(r, dir.Length, points, skipBackFaces, out intersectionPoint);
        }



        public static bool Intersects(Ray r, double rayScale, Vector3d[] points, bool skipBackFaces, out Vector3d intersectionPoint)
        {
            if (points.Length < 3) throw new ArgumentException();

            double distance = 0;
            Vector3d intersectPoint = Vector3d.Zero();
            intersectionPoint = intersectPoint;

            //if (skipBackFaces)
            //{
            //    // Find vectors for two edges sharing vert0
            //    Vector3d edge1 = points[1] - points[0];
            //    Vector3d edge2 = points[points.Length - 1] - points[0];

            //    // Begin calculating determinant - also used to calculate U parameter
            //    Vector3d pvec = Vector3d.CrossProduct(r.direction, edge2);

            //    // If determinant is near zero, ray lies in plane of triangle
            //    double det = Vector3d.DotProduct(edge1, pvec);
            //    double OneOverDet = 1.0f / det;
            //    // Calculate distance from vert0 to ray origin
            //    Vector3d tvec = r.origin - points[0];

            //    // Calculate U parameter and test bounds
            //    double U = Vector3d.DotProduct(tvec, pvec);

            //    if ((U < 0.0f) || (U > det))
            //        return false;
            //}

            Plane p = new Plane(points[0], points[1], points[points.Length - 1]);
            //if (r.Origin == points[0] || r.Origin == points[1] || r.Origin == points[points.Length - 1])
            //{
            //    // TODO: I believe this code is ok.  I added it July.1.2011 to catch case where
            //    // if any of the points that define the plane is the same as the ray  origin
            //    // then we should short circuit and return true with the intersection point equal to
            //    // the ray origin and obviously distance == 0
            //    // TODO: actually this doesnt work at all.  I itterate through all cells so this 
            //    // gets evaluated and returns true every time when it hits the first cell!
            //    intersectPoint = r.Origin;
            //    return true;
            //}
            //p = new Plane(points[0], points[1], points[2]);
            //if (skipBackFaces)
            //{
            //    if( Plane.DistanceToPlane(r.Origin, p) < 0)
            //        return false;
            //}

            bool result = (p.Intersects(r, rayScale, ref distance, ref intersectPoint));
            intersectionPoint = intersectPoint;
            if (!result) return false;

            // now that we have an intersection point, find if that point is in the set of points that make up the poly
            return ContainsPoint(intersectPoint, points);
        }

        public bool ContainsPoint(Vector3d rayPlaneIntersectionPoint)
        {
            return ContainsPoint(rayPlaneIntersectionPoint, _points);
        }

        public bool ContainsPoints(Vector3d[] points)
        {
            if (points == null || points.Length == 0) return false;

            for (int i = 0; i < points.Length; i++)
                if (ContainsPoint(points[i]) == false) return false;

            return true;
        }

        // Tests if a point on the plane of a polygon is within the actual bounds of the point
        // http://www.realtimerendering.com/intersections.html
        public static bool ContainsPoint(Vector3d rayPlaneIntersectionPoint, Vector3d[] polygonVertices)
        {
            const double MATCH_FACTOR = 0.99; // Used to cover up the error in floating point
            double angle = 0.0; // Initialize the angle

            for (int i = 0; i < polygonVertices.Length; i++) // Go in a circle to each vertex and get the angle between
            {
                Vector3d vA = polygonVertices[i] - rayPlaneIntersectionPoint;
                    // Subtract the intersection point from thecurrent vertex
                Vector3d vB = polygonVertices[(i + 1) % polygonVertices.Length] - rayPlaneIntersectionPoint;
                    // Subtract the point from the next vertex
                angle += Vector3d.AngleBetweenVectors(vA, vB);
                    // Find the angle between the 2 vectors and add them all up as we go along
            }
          
            if (angle >= (MATCH_FACTOR*(2.0*Math.PI))) // If the angle is greater than 2 PI, (360 degrees)
                return true; // The point is inside of the polygon

            return false; // If you get here, it obviously wasn't inside the polygon, so Return FALSE
        }
    }
}