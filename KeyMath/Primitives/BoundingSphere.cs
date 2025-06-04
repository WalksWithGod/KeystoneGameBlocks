namespace Keystone.Types
{
    public class BoundingSphere
    {
        private Vector3d _center;
        private double _radius;

        /// <summary>
        /// Calculates a bounding sphere to encompass a bounding box.
        /// </summary>
        /// <remarks>
        /// It takes into account the diagonal extents of the box, not just the max axis length
        /// </remarks>
        /// <param name="box"></param>
        public BoundingSphere(BoundingBox box) : this (box.Center, (box.Max - box.Min).Length / 2d)
        {
        }
        
        public BoundingSphere(BoundingSphere sphere) : this (sphere._center, sphere._radius)
        {
        }
        
        public BoundingSphere(Vector3d center, double radius)
        {
            _center = center;
            _radius = radius;
        }

        public BoundingSphere (double centerX, double centerY, double centerZ, double radius)
        {
            _center.x = centerX;
            _center.y = centerY;
            _center.z = centerZ;
            _radius = radius;
        }

        // Sphere to sphere can be a faster test, but results in much greater overdraw because the sphere is HUGE to make 
        // sure it doesnt cull things that are visible.  This is best used as a preliminary stage cull.
        public BoundingSphere(float nearPlane, float farPlane, float fovRadians,
                              Vector3d cameraPosition, Vector3d lookAt)
        {
            double diameter = farPlane - nearPlane;
            double sngRadius = diameter * 0.5;
            double farPlaneHeight = diameter * System.Math.Tan(fovRadians * 0.5);

            // with an aspect ratio of 1, our width = height
            double farPlaneWidth = farPlaneHeight;

            //TODO: once we have the radius, we dont actually have to update it unless the far/near/fov changes
            // but we'll still always need to update the center based on the LookAt
            Vector3d center;
            center.x = 0;
            center.y = 0;
            center.z = nearPlane + sngRadius;

            Vector3d farCorner;
            farCorner.x = farPlaneWidth;
            farCorner.y = farPlaneHeight;
            farCorner.z = diameter;

            // the frustum sphere radius becomes the length of this vector
            _radius = (farCorner - center).Length;

            // TODO: Below is actually the only things that need to be updated every frame.  So to optimize
            // this seperate out the FrustumSphereInitialization from the FrustumSphereUpdate.  Only re-init
            // if for some reason the near/far/fov changes.
            // calculate the center of the sphere    
            // note in TV3d the lookAt is actually the point in world coordinates of where we are looking
            // to get the real direction vector, substract it from the camera position
            Vector3d dir = Vector3d.Normalize(lookAt - cameraPosition);
            dir *= _radius;
            _center = cameraPosition + dir;
        }

        public double Radius
        {
            get { return _radius; }
        }

        public Vector3d Center
        {
        	get { return _center; } set {_center = value;} 
        }
        
        public void Scale (double scale)
        {
        	_radius *= scale;
        }

        public BoundingSphere Transform(Matrix matrix)
        {
           return Transform(this, matrix);
        }

        public static BoundingSphere Transform(BoundingSphere sphere, Matrix matrix)
        {
            Vector3d pointOnSurface; 
            pointOnSurface.x = sphere._radius;
            pointOnSurface.y = 0;
            pointOnSurface.z = 0;
            pointOnSurface += sphere._center; 

            Vector3d center = Vector3d.TransformCoord(sphere._center, matrix);
            pointOnSurface = Vector3d.TransformCoord (pointOnSurface, matrix);

            double radius = (pointOnSurface - center).Length;

            return new BoundingSphere(center, radius);
        }


        // if the distance between the sphere center is less than radius it contains the point
        public bool Contains(Vector3d point)
        {
            Vector3d v = _center - point;
            double distance = v.LengthSquared();
            return distance < _radius;
        }

        /// <summary>
        /// Returns whether the targetSphere is fully contained by this sphere instance.
        /// </summary>
        /// <param name="targetSphere"></param>
        /// <returns>True if this sphere instance fully contains the target sphere.  False otherwise.</returns>
        public bool Contains(BoundingSphere targetSphere)
        {
            Vector3d v = _center - targetSphere.Center;
            double distance = v.LengthSquared();
            // similar to intersect only instead of sumRadiiSquared, its just the radius squared of the source sphere
            // for small meshes being tested against a frustum sphere this results in much less "intersect" false positives
            // however for large meshes, this will ignore the ones that dont fully fit within the source (e.g. frustum) sphere.
            return distance < _radius * _radius && _radius > targetSphere.Radius;
        }

        
        // ----------------------------------------------------------------------
        // Name  : CheckPointInTriangle()
        // Input : point - point we wish to check for inclusion
        //         sO - Origin of sphere
        //         sR - radius of sphere 
        // Notes : 
        // Return: TRUE if point is in sphere, FALSE if not.
        // -----------------------------------------------------------------------  
        //private bool CheckPointInSphere(Vector3d point, Vector3d sO, double sR)
        //{
        //    double d = (point - sO).Length;

        //    if (d <= sR) return true;
        //    return false;
        //}

        /// <summary>
        /// Sphere 2 Sphere intersection.
        /// </summary>
        /// <param name="targetSphere"></param>
        /// <returns></returns>
        public IntersectResult Intersects(BoundingSphere targetSphere)
        {
            Vector3d v = _center - targetSphere.Center;
            double distance = v.LengthSquared();

            // if the distance between the centers is less than the radius of this instance
            // _and_ the radius of this instance is larger than the target, this instance fully 
            // contains the target
            if (distance < _radius * _radius && _radius > targetSphere.Radius)
                return IntersectResult.INSIDE;

            // if the distance between the centers is less than the sum of 
            // the radii then the two spheres intersect
            double RadiiSum = _radius + targetSphere.Radius;
            double RadiiSumSquared = RadiiSum * RadiiSum;
            if (distance < RadiiSumSquared)
                return IntersectResult.INTERSECT;

            return IntersectResult.OUTSIDE;

        }

        /// <summary>
        /// Ray 2 Sphere intersection.
        /// </summary>
        /// <param name="ray">ray</param>
        /// <param name="i1">first intersection distance</param>
        /// <param name="i2">second intersection distance</param>
        /// <returns>true if intersection is found, false otherwise.</returns>
        public bool Intersects (Ray  ray,  ref double  i1, ref double i2)
        {
            Vector3d p = ray.Origin - _center;
            double b = -Vector3d.DotProduct(p, ray.Direction);
            double c = Vector3d.DotProduct(p, p) + _radius * _radius;
            double det = b * b - c;
	        
            if (det < 0) return false;
        	
	        det = System.Math.Sqrt(det);
	        
	        // because this is polynomial, 2 possible solutions -> +/-
	        i1 = b - det;
	        i2 = b + det;
	        // intersecting with ray?
	        
	        // if i2 is less than 0, the collision occurred in the ray's negative direction?
	        if (i2 < 0) return false;
	        
	        // if i1 is less than 0, the collission occurred at i2?
	        if(i1 < 0) i1 = 0;
	        return true;
        }

        // TODO: make sure above version works before deleting this... and verify number of operations is optimal
        // ----------------------------------------------------------------------
        // Name  : intersectRaySphere()
        // Input : rO - origin of ray in world space
        //         rV - vector describing direction of ray in world space
        //         sO - Origin of sphere 
        //         sR - radius of sphere
        // Notes : Normalized directional vectors expected
        // Return: distance to sphere in world units, -1 if no intersection.
        // -----------------------------------------------------------------------  
        //private double intersectRaySphere(Vector3d rO, Vector3d rV, Vector3d sO, double sR)
        //{
        //    Vector3d Q = sO - rO;

        //    double c = Q.Length;
        //    double v = Vector3d.DotProduct(Q, rV);
        //    double d = sR * sR - (c * c - v * v);

        //    // If there was no intersection, return -1
        //    if (d < 0.0) return (-1.0f);

        //    // Return the distance to the [first] intersecting point
        //    return (double)(v - Math.Sqrt(d));
        //}
        
        // TODO: followinig is from
        // http://wiki.cgsociety.org/index.php/Ray_Sphere_Intersection
        // and may be less efficient but may have better precision when ray origin is far
        public bool Intersects(Ray ray, ref double t)
        {
        	// NOTE: This assumes that the sphere is at origin and that the ray is in modelspace, but if
        	// not then we have to move the ray to modelspace.  Obviously both ray and sphere must be in same space.
        	ray = new Ray ( ray.Origin - _center, ray.Direction);
        	
            //Compute A, B and C coefficients
            double a = Vector3d.DotProduct(ray.Direction, ray.Direction);
            double b = 2 * Vector3d.DotProduct(ray.Direction, ray.Origin);
            double c = Vector3d.DotProduct(ray.Origin, ray.Origin) - (_radius * _radius);

            //Find discriminant
            double disc = b * b - 4 * a * c;
            
            // if discriminant is negative there are no real roots, so return 
            // false as ray misses sphere
            if (disc < 0)
                return false;

            // compute q as described above
            double distSqrt = System.Math.Sqrt(disc);
            double q;
            if (b < 0)
                q = (-b - distSqrt) / 2.0;
            else
                q = (-b + distSqrt) /2.0;

            // compute t0 and t1
            double t0 = q / a;
            double t1 = c / q;

            // make sure t0 is smaller than t1
            if (t0 > t1)
            {
                // if t0 is bigger than t1 swap them around
                double temp = t0;
                t0 = t1;
                t1 = temp;
            }

            // if t1 is less than zero, the object is in the ray's negative direction
            // and consequently the ray misses the sphere
            if (t1 < 0)
                return false;

            // if t0 is less than zero, the intersection point is at t1
            if (t0 < 0)
            {
                t = t1;
                return true;
            }
            // else the intersection point is at t0
            else
            {
                t = t0;
                return true;
            }
        }

        public struct SweepResult
        {
            public bool Intersection; // true if there is some intersection(including an initial intersection)
            public float? T;
            public int? FaceIndex;
            public Vector3d? Point;
            public Vector3d? Normal;
            public float? PenetrationDepth;
        }


        //// TODO: need to add sweep tests to BoundingBox and Lines too
        //// http://therealdblack.wordpress.com/ 
        //// http://therealdblack.wordpress.com/category/sweep-tests/   - xna blog posts about various sweep tests
        //public static bool Sweep(BoundingSphere sweepSphere, BoundingSphere otherSphere, Vector3d direction, out SweepResult sweepResult)
        //{

        //    //like a sphere-point sweep with a sphere the size of the sum of the radii
        //    sweepResult = new SweepResult(null);

        //    BoundingSphere infSphere = new BoundingSphere(sweepSphere.Center, sweepSphere.Radius + otherSphere.Radius);

        //    SweepResult infSweepResult; //Inflated sphere result

        //    bool infResult = SweepSpherePoint(infSphere, otherSphere.Center, direction, out infSweepResult);

        //    sweepResult.T = infSweepResult.T;

        //    if (infSweepResult.T != null)
        //    {
        //        sweepResult.Point = infSweepResult.Point + infSweepResult.Normal * otherSphere.Radius;
        //        sweepResult.Normal = infSweepResult.Normal;
        //    }

        //    return sweepResult.Intersection = infResult;
        //}


        //public static bool SweepSpherePoint(BoundingSphere sweepSphere, Vector3d pt, Vector3d direction, out SweepResult sweepResult)
        //{
        //    //sweep point against sphere along -direction
        //    sweepResult = new SweepResult(null);

        //    if (direction.Length < DirectionEpsilon)
        //    {
        //        //zero direction, is the point initially touching the sphere?
        //        return sweepResult.Intersection = Intersects(sweepSphere, pt);
        //    }

        //    Vector3d P = pt - sweepSphere.Center;

        //    double PdotV = Vector3d.DotProduct(P, -direction);
        //    double PdotP = Vector3d.DotProduct(pt, pt);

        //    double a = Vector3d.DotProduct(direction, direction);
        //    double b = 2.0d * PdotV;
        //    double c = PdotP - sweepSphere.Radius * sweepSphere.Radius;

        //    double t0, t1;

        //    if (!Utilities.MathHelper.SolveQuadratic(a, b, c, out t0, out t1))
        //    {
        //        return sweepResult.Intersection = false;
        //    }

        //    Utilities.MathHelper.Sort(ref t0, ref t1);

        //    if ((t1 < 0.0f) || (t0 > 1.0f))
        //    {
        //        return sweepResult.Intersection = false;
        //    }

        //    if (t0 < 0.0f)
        //    {
        //        return sweepResult.Intersection = true;
        //    }

        //    Vector3d sphereHitCen0 = sweepSphere.Center + direction * t0;

        //    sweepResult.T = t0;
        //    sweepResult.Point = pt;
        //    sweepResult.Normal = Vector3d.Normalize(sphereHitCen0 - pt);

        //    return sweepResult.Intersection = true;
        //}


    }
}