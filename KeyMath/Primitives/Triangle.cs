using System;
using Keystone.Types;

namespace Keystone.Types
{
    public class Triangle : Polygon
    {
        private const double LOCAL_EPSILON = 0.000001d; // triangle intersection fudge factor

        // tri_face structure // TODO: needs to be a more universal u,v distance structure for not just triangles
        public struct TRI_FACE
        {
            public double U;
            public double V;
            public double Distance;
        }

        public Triangle(Vector3d p1, Vector3d p2, Vector3d p3) : base (new Vector3d[]{p1, p2,p3})
        {
        }

        public Vector3d Center
        {
            get { return getCenter(Points[0], Points[1], Points[2]); }
        }

        public static Vector3d getCenter(Vector3d v1, Vector3d v2, Vector3d v3)
        {
            return new Vector3d((v1.x + v2.x + v3.x)/3, (v1.y + v2.y + v3.y)/3, (v1.z + v2.z + v3.z)/3);
        }

        
        /// <summary>
        /// Tomas Möller's RayTri collision test.
        /// usage - itterate through triangles passing the verts
        /// and depending on whether we want first contact exit or to build an entire list of hits
        /// we continue itterating.
        /// we can also cache the previous frame's results and if our test parameters are the same
        /// we can try to test those first, else we start back at beginning.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="vert0"></param>
        /// <param name="vert1"></param>
        /// <param name="vert2"></param>
        /// <param name="backfaceCulling">skip backface triangles</param>
        /// <param name="hitResult"></param>
        /// <returns></returns>
        public static bool Intersects(Ray r, Vector3d vert0, Vector3d vert1, Vector3d vert2, bool backfaceCulling, ref TRI_FACE hitResult )
        {
            // Find vectors for two edges sharing vert0
            Vector3d edge1 = vert1 - vert0; // vert0 - vert1;
            Vector3d edge2 = vert2 - vert0; // vert0 - vert2;

            // Begin calculating determinant - also used to calculate U parameter
            Vector3d pvec = Vector3d.CrossProduct(r.Direction, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            double det = Vector3d.DotProduct(edge1, pvec);
            double OneOverDet;

            if (backfaceCulling)  // only test frontward facing triangles
            {
                if (det < LOCAL_EPSILON)
                    return false;
                // From here, det is > 0. So we can use integer cmp.

                // Calculate distance from vert0 to ray origin
                Vector3d tvec =  r.Origin - vert0;

                // Calculate barycentric U parameter and test bounds
                hitResult.U = Vector3d.DotProduct(tvec, pvec);

                if ((hitResult.U < 0.0f) || (hitResult.U > det))
                    return false;

                // Prepare to test V parameter
                Vector3d qvec = Vector3d.CrossProduct(tvec, edge1);

                // Calculate barrycentric V parameter and test bounds
                hitResult.V = Vector3d.DotProduct(r.Direction, qvec);
                if ((hitResult.V < 0.0f) || (hitResult.U + hitResult.V > det))
                    return false;

                // Calculate t, scale parameters, ray intersects triangle
                hitResult.Distance = Vector3d.DotProduct(edge2, qvec);
                // Det > 0 so we can early exit here
                // Intersection point is valid if distance is positive (else it can just be a face behind the orig point)
                if (hitResult.Distance < 0.0f) return false;
                // here in Moeller's code he includes in the if (mStabbedFace.mU + mStabbedFace.mV > det) 
                // Else go on
                OneOverDet = 1.0f / det;
                hitResult.Distance *= OneOverDet;
                hitResult.U *= OneOverDet;
                hitResult.V *= OneOverDet;
            }
            else
            {
                // the non-culling branch
                if (det > -LOCAL_EPSILON && det < LOCAL_EPSILON)
                    return false;

                // Calculate distance from vert0 to ray origin
                Vector3d tvec = r.Origin - vert0;
                OneOverDet = 1.0f / det;
                // Calculate U parameter and test bounds
                hitResult.U = (Vector3d.DotProduct(tvec, pvec)) * OneOverDet;

                if ((hitResult.U < 0.0f) || (hitResult.U > 1.0f))
                    return false;

                // prepare to test V parameter
                Vector3d qvec = Vector3d.CrossProduct(tvec, edge1);

                // Calculate V parameter and test bounds
                hitResult.V = (Vector3d.DotProduct(r.Direction, qvec)) * OneOverDet;
                if ((hitResult.V < 0.0f) || (hitResult.U + hitResult.V > 1.0f))
                    return false;

                // Calculate t, ray intersects triangle
                hitResult.Distance = (Vector3d.DotProduct(edge2, qvec)) * OneOverDet;
                // Intersection point is valid if distance is positive (else it can just be a face behind the orig point)
                if (hitResult.Distance < 0.0f)
                    return false;
            }
            return true;
        }

        // ----------------------------------------------------------------------
        // Name  : CheckPointInTriangle()
        // Input : point - point we wish to check for inclusion
        //         a - first vertex in triangle
        //         b - second vertex in triangle 
        //         c - third vertex in triangle
        // Notes : Triangle should be defined in clockwise order a,b,c
        // Return: TRUE if point is in triangle, FALSE if not.
        // -----------------------------------------------------------------------  
        public static bool CheckPointInTriangle(Vector3d point, Vector3d a, Vector3d b, Vector3d c)
        {
            double total_angles = 0.0f;
            double epsilon = 0.005;
            // make the 3 vectors
            Vector3d v1 = point - a;
            Vector3d v2 = point - b;
            Vector3d v3 = point - c;

            v1 = Vector3d.Normalize(v1);
            v2 = Vector3d.Normalize(v2);
            v3 = Vector3d.Normalize(v3);

            total_angles += Math.Acos(Vector3d.DotProduct(v1, v2));
            total_angles += Math.Acos(Vector3d.DotProduct(v2, v3));
            total_angles += Math.Acos(Vector3d.DotProduct(v3, v1));

            if (Math.Abs(total_angles - 2 * Math.PI) <= epsilon)
                return (true);

            return (false);
        }

        // ----------------------------------------------------------------------
        // Name  : closestPointOnTriangle()
        // Input : a - first vertex in triangle
        //         b - second vertex in triangle 
        //         c - third vertex in triangle
        //         p - point we wish to find closest point on triangle from 
        // Notes : 
        // Return: closest point on line triangle edge
        // -----------------------------------------------------------------------  
        public static Vector3d ClosestPointOnTriangle(Vector3d a, Vector3d b, Vector3d c, Vector3d p)
        {
            Vector3d Rab = Line3d.ClosestPointOnLine(a, b, p);
            Vector3d Rbc = Line3d.ClosestPointOnLine(b, c, p);
            Vector3d Rca = Line3d.ClosestPointOnLine(c, a, p);

            double dAB = (p - Rab).Length;
            double dBC = (p - Rbc).Length;
            double dCA = (p - Rca).Length;


            double min = dAB;
            Vector3d result = Rab;

            if (dBC < min)
            {
                min = dBC;
                result = Rbc;
            }

            if (dCA < min)
                result = Rca;

            return (result);
        }
    }
}