//-----------------------------------------------------------------------------
//// File: Collision.cpp
////
//// Desc: Implementation of the collision detection
////
//// Copyright (c) 2000 Telemachos of Peroxide
//// www.peroxide.dk
////-----------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Text;

using System;
using Keystone.Culling;
using Keystone.Elements;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Collision
{
//// ----------------------------------------------------------------------------
//// Descr : Structure keeping track of the collisions as we find them. 
//// Note  : Somewhat simplified from what is needed if you deside to implement
////         the features described in the section of dynamic geometry.
//// ----------------------------------------------------------------------------

    public struct TCollisionPacket
    {
        // data about player movement
        public Vector3d Velocity;
        public Vector3d SourcePoint;

        // radius of ellipsoid.  
        public Vector3d eRadius;

        // for error handling  
        public Vector3d LastSafePosition;
        public bool Stuck;

        // data for collision response 
        public bool FoundCollision;
        public double NearestDistance; // nearest distance to hit
        public Vector3d NearestIntersectionPoint; // on sphere
        public Vector3d NearestPolygonIntersectionPoint; // on polygon

        public TCollisionPacket(Vector3d position, Vector3d velocity, double radiusX, double radiusY, double radiusZ)
        {
            if (radiusZ == 0 || radiusY == 0 || radiusX == 0) throw new ArgumentOutOfRangeException();
            Velocity = velocity;
            SourcePoint = position;
            FoundCollision = false;
            Stuck = false;
            LastSafePosition = new Vector3d();
            NearestDistance = -1;
            eRadius = new Vector3d(radiusX, radiusY, radiusZ);
            NearestIntersectionPoint = new Vector3d();
            NearestPolygonIntersectionPoint = new Vector3d();
        }

        public Vector3d ScaledPosition
        {
            get { return new Vector3d(SourcePoint.x/eRadius.x, SourcePoint.y/eRadius.y, SourcePoint.z/eRadius.z); }
        }

        public Vector3d ScaledVelocity
        {
            get { return new Vector3d(Velocity.x/eRadius.x, Velocity.y/eRadius.y, Velocity.z/eRadius.z); }
        }
    }

    // TODO: IMPORTANT  This URL has great continuous collision detection for ellipsoid!
    // http://www.gamedev.net/reference/programming/features/ellipsoid-ccd/page2.asp
    // http://www.gamedev.net/reference/articles/article2426.asp

    // ----------------------------------------------------------------------
    // Name  : CheckCollision()
    // Descr : Checks one mesh for collision
    // Return: updated collision structure.
    // -----------------------------------------------------------------------  
    public class CollisionTest
    {
        public void CheckCollision(TCollisionPacket colPackage, ConvexHull hull)
        {
            //// plane data
            //int A, B, C;
            //Vector3d p1, p2, p3;
            //Vector3d pNormal;
            //Vector3d pOrigin;
            //Vector3d v1, v2;


            //// from package
            //Vector3d source = colPackage.ScaledPosition; // colPackage.SourcePoint;
            //Vector3d eRadius = colPackage.eRadius;
            //Vector3d velocity = colPackage.ScaledVelocity; // colPackage.Velocity;

            //// how long is our velocity
            //double distanceToTravel = velocity.Length;

            //// keep a copy of this as it's needed a few times
            //Vector3d normalizedVelocity = velocity;
            //normalizedVelocity = Vector3d.Normalize(normalizedVelocity);

            //// intersection data
            //Vector3d sIPoint; // sphere intersection point
            //Vector3d pIPoint; // plane intersection point 	
            //Vector3d polyIPoint; // polygon intersection point


            //double distToPlaneIntersection;
            //double distToEllipsoidIntersection;


            //// loop through all faces in mesh.  These arent in world coords?
            //for (int i = 0; i < hull.Triangles.Length; i++)
            //{
            //    //A = vertexIndices[i*3];
            //    //B = vertexIndices[i*3 + 1];
            //    //C = vertexIndices[i*3 + 2];

            //    Triangle t = hull.Triangles[i];

            //    // Get the data for the triangle in question and scale to ellipsoid space
            //    p1.x = hull.Triangles[i].Points[0].x/eRadius.x; // m_pIndexedVertices[A].x/eRadius.x;
            //    p1.y = hull.Triangles[i].Points[0].y/eRadius.y; // m_pIndexedVertices[A].y/eRadius.y;
            //    p1.z = hull.Triangles[i].Points[0].z/eRadius.z; // m_pIndexedVertices[A].z/eRadius.z;

            //    p2.x = hull.Triangles[i].Points[1].x/eRadius.x; // m_pIndexedVertices[B].x/eRadius.x;
            //    p2.y = hull.Triangles[i].Points[1].y/eRadius.y; // m_pIndexedVertices[B].y/eRadius.y;
            //    p2.z = hull.Triangles[i].Points[1].z/eRadius.z; // m_pIndexedVertices[B].z/eRadius.z;

            //    p3.x = hull.Triangles[i].Points[2].x/eRadius.x; // m_pIndexedVertices[C].x/eRadius.x;
            //    p3.y = hull.Triangles[i].Points[2].y/eRadius.y; // m_pIndexedVertices[C].y/eRadius.y;
            //    p3.z = hull.Triangles[i].Points[2].z/eRadius.z; // m_pIndexedVertices[C].z/eRadius.z;


            //    // Make the plane containing this triangle.      
            //    pOrigin = p1;
            //    v1 = p2 - p1;
            //    v2 = p3 - p1;

            //    // PLANE pPlane = new PLANE(p1, p2, p3);


            //    // You might not need this if you KNOW all your triangles are valid
            //    if (!(isZeroVector(v1) || !(isZeroVector(v2))))
            //    {
            //        // determine normal to plane containing polygon  
            //        pNormal = wedge(v1, v2);
            //        pNormal = Vector3d.Normalize(pNormal);
            //        //PLANE p = new PLANE(v1, pNormal); //hypno

            //        // calculate sphere intersection point
            //        sIPoint = source - pNormal;

            //        // classify point to determine if ellipsoid span the plane
            //        // find the plane intersection point
            //        if (classifyPoint(sIPoint, pOrigin, pNormal) == VECTOR_CLASSIFICATION.PLANE_BACKSIDE)
            //        {
            //            // plane is embedded in ellipsoid

            //            // find plane intersection point by shooting a ray from the 
            //            // sphere intersection point along the planes normal.
            //            distToPlaneIntersection = Plane.IntersectionPoint();// TODO: convert to use Plane.IntersectPoint intersectRayPlane(sIPoint, pNormal, pOrigin, pNormal);
            //            //distToPlaneIntersection = 0;
            //            //    PLANE.Intersects(new Ray(sIPoint, pNormal), p,ref distToPlaneIntersection); //hypno

            //            // calculate plane intersection point
            //            pIPoint.x = sIPoint.x + distToPlaneIntersection*pNormal.x;
            //            pIPoint.y = sIPoint.y + distToPlaneIntersection*pNormal.y;
            //            pIPoint.z = sIPoint.z + distToPlaneIntersection*pNormal.z;
            //        }
            //        else
            //        {
            //            // shoot ray along the velocity vector
            //            distToPlaneIntersection = Plane.IntersectionPoint(); // TODO: convert to use Plane.IntersectPoint intersectRayPlane(sIPoint, normalizedVelocity, pOrigin, pNormal);

            //            //distToPlaneIntersection = 0;
            //            //    PLANE.Intersects(new Ray(sIPoint, normalizedVelocity), p, ref distToPlaneIntersection); //hypno

            //            // calculate plane intersection point
            //            pIPoint.x = sIPoint.x + distToPlaneIntersection*normalizedVelocity.x;
            //            pIPoint.y = sIPoint.y + distToPlaneIntersection*normalizedVelocity.y;
            //            pIPoint.z = sIPoint.z + distToPlaneIntersection*normalizedVelocity.z;
            //        }

            //        // find polygon intersection point. By default we assume its equal to the 
            //        // plane intersection point

            //        polyIPoint = pIPoint;
            //        distToEllipsoidIntersection = distToPlaneIntersection;

            //        if (!CheckPointInTriangle(pIPoint, p1, p2, p3))
            //        {
            //            // if not in triangle
            //            polyIPoint = closestPointOnTriangle(p1, p2, p3, pIPoint);

            //            distToEllipsoidIntersection =
            //                BoundingSphere.Intersects(r, ); // TODO: convert to use Sphere.Intersects intersectRaySphere(polyIPoint,
            //                                   new Vector3d(-normalizedVelocity.x, -normalizedVelocity.y,
            //                                                -normalizedVelocity.z), source, 1.0f);

            //            if (distToEllipsoidIntersection > 0)
            //            {
            //                // calculate true sphere intersection point
            //                sIPoint.x = polyIPoint.x + distToEllipsoidIntersection*-normalizedVelocity.x;
            //                sIPoint.y = polyIPoint.y + distToEllipsoidIntersection*-normalizedVelocity.y;
            //                sIPoint.z = polyIPoint.z + distToEllipsoidIntersection*-normalizedVelocity.z;
            //            }
            //        }

            //        // Here we do the error checking to see if we got ourself stuck last frame
            //        if (CheckPointInSphere(polyIPoint, source, 1.0f))
            //            colPackage.Stuck = true;


            //        // Ok, now we might update the collision data if we hit something
            //        if ((distToEllipsoidIntersection > 0) && (distToEllipsoidIntersection <= distanceToTravel))
            //        {
            //            if ((colPackage.FoundCollision == false) ||
            //                (distToEllipsoidIntersection < colPackage.NearestDistance))
            //            {
            //                // if we are here we have a closest hit so far. We save the information
            //                colPackage.NearestDistance = distToEllipsoidIntersection;
            //                colPackage.NearestIntersectionPoint = sIPoint;
            //                colPackage.NearestPolygonIntersectionPoint = polyIPoint;
            //                colPackage.FoundCollision = true;
            //                DebugDraw.DrawFilledTriangle(t, CONST_TV_COLORKEY.TV_COLORKEY_BLUE);
            //            }
            //        }
            //    } // if a valid plane 	
            //} // for all faces	
        }

        //I've noticed if you change following line , results are giving no bouncing effect :
        //
        //Code:
        //Normal = maths.VScale(Normal, 0,001)
        //(replace DIST_FROM_WALL + 2 wiith 0,001)
        private double DIST_FROM_WALL = .1F;


        private int COLLISION_CHECKS = 2;
        private double COLLISION_RADIUS = 1f;

        //private Vector3d Collision(Vector3d Pos)
        //{
        //    double AngleWidth;
        //    double[,] Angles = new double[COLLISION_CHECKS,2];
        //    double SortedL = 0;
        //    double SortedA = 0;
        //    int PrevAngle = 0;
        //    int NextAngle = 0;
        //    double NewA1, NewD1, NewA2, NewD2, NewA3, NewD3;
        //    int i;

        //    AngleWidth = 360/COLLISION_CHECKS;
        //    SortedL = 999999;

        //    for (i = 0; i < COLLISION_CHECKS; i++)
        //    {
        //        TV_COLLISIONRESULT Result = new TV_COLLISIONRESULT();
        //        Vector3d CollisionVector = Core.Core._CoreClient.Maths.MoveAroundPoint(Pos, 0, AngleWidth * i, 1);
        //        int hits = 0;
        //        if (
        //            Core.Core._CoreClient.Scene.AdvancedCollision(Pos, CollisionVector, ref Result,
        //                                          (int) CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH,
        //                                          CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING, ref hits))
        //        {
        //            Angles[i, 0] = Result.fDistance;
        //            Trace.WriteLine(Angles[i, 0]);
        //        }

        //        else
        //        {
        //            Angles[i, 0] = 0;

        //            Angles[i, 1] = i*AngleWidth;
        //            if (Angles[i, 0] == 0) Angles[i, 0] = 1000000;
        //        }
        //    }

        //    for (i = 0; i < COLLISION_CHECKS - 1; i++)
        //    {
        //        if (Angles[i, 0] < SortedL)
        //        {
        //            SortedL = Angles[i, 0];
        //            SortedA = Angles[i, 1];
        //        }
        //    }


        //    PrevAngle = (int) (WrapAngle(SortedA - 90)/AngleWidth);
        //    NextAngle = (int) (WrapAngle(SortedA + 90)/AngleWidth);
        //    NewD1 = COLLISION_RADIUS - SortedL;
        //    NewA1 = WrapAngle(SortedA - 180);
        //    NewD2 = COLLISION_RADIUS - Angles[PrevAngle, 0];
        //    NewA2 = WrapAngle(Angles[PrevAngle, 1] - 180);
        //    NewD3 = COLLISION_RADIUS - Angles[NextAngle, 0];
        //    NewA3 = WrapAngle(Angles[NextAngle, 1] - 180);

        //    //Debug.Print Angles(NextAngle, 1)

        //    if (SortedL < COLLISION_RADIUS && SortedL > 0)
        //        Pos = Core.Core._CoreClient.Maths.MoveAroundPoint(Pos.ToTV3DVector(), 0, NewA1, NewD1);
        //    if (Angles[PrevAngle, 0] < COLLISION_RADIUS && Angles[PrevAngle, 0] > 0)
        //        Pos = Core.Core._CoreClient.Maths.MoveAroundPoint(Pos, 0, NewA2, NewD2);
        //    if (Angles[NextAngle, 0] < COLLISION_RADIUS && Angles[NextAngle, 0] > 0)
        //        Pos = Core.Core._CoreClient.Maths.MoveAroundPoint(Pos, 0, NewA3, NewD3);

        //    return Pos;
        //}


        private static CollisionTest _ct = new CollisionTest();
        private const double EPSILON = .00005f;
        //-----------------------------------------------------------------------------
        // Name: collideWithWorld()
        // Desc: Sliding collision. Recursive part of the collision response. This function is the
        //       one who actually calls the collision check on the meshes
        //-----------------------------------------------------------------------------
        private Vector3d collideWithWorld(Vector3d position, Vector3d velocity, ref TCollisionPacket collision)
        {
            //Vector3d destinationPoint = position + velocity;

            //// get a pointer to your meshes in some way
            //Picker pick = new Picker();
            //pick.Pick(new Ray(collision.SourcePoint, collision.Velocity));
            //Mesh3d[] meshes = pick.FoundMeshes;
            
            //// run a ray test and return all meshes who's bounding boxes intersect
            //// itterate thru the hull data of each

            //// TODO: needs to use entities
            //// For all candiate meshes (meshes who's bounding boxes tested positive
            //// Check against the current entity's ellipsoid
            //foreach (Mesh3d m in meshes)
            //{
            //    // transform the hull coords from local space to world space for this Entity's mesh
            //    ConvexHull worldCoordHull = new ConvexHull( m.Hull, m.Matrix);
            //    _ct.CheckCollision(collision, worldCoordHull);
            //}


            //// check return value here, and possibly call recursively 	
            //if (collision.FoundCollision == false)
            //{
            //    // if no collision move very close to the desired destination. 
            //    double l = velocity.Length;
            //    Vector3d V = velocity;

            //    //setLength(V, l - EPSILON); // this is equivalent to scaling the vector by newlength/currentLength 
            //    V *= ((l - EPSILON) / l);

            //    // update the last safe position for future error recovery	
            //    collision.LastSafePosition = position;

            //    // return the final position
            //    return position + velocity; // V;
            //}
            //else
            //{
            //    // There was a collision
            //    // If we are stuck, we just back up to last safe position
            //    if (collision.Stuck)
            //        return collision.LastSafePosition;

            //    // OK, first task is to move close to where we hit something :
            //    Vector3d newSourcePoint;

            //    // only update if we are not already very close
            //    if (collision.NearestDistance >= EPSILON)
            //    {
            //        Vector3d V = velocity;
            //        // setLength(V, collision.nearestDistance - EPSILON);  // scale the vector by newlength / currentlength
            //        double len = V.Length;
            //        V = V * ((collision.NearestDistance - EPSILON) / len);
            //        newSourcePoint = collision.SourcePoint + V;
            //    }
            //    else
            //        newSourcePoint = collision.SourcePoint;


            //    // Now we must calculate the sliding plane
            //    Vector3d slidePlaneOrigin = collision.NearestPolygonIntersectionPoint;
            //    Vector3d slidePlaneNormal = newSourcePoint - collision.NearestPolygonIntersectionPoint;
            //    // huh?  shouldnt the sliding plane take into account the normal of the triangle we hit?
            //    //PLANE slidePlane =
            //    //    new PLANE(collision.NearestPolygonIntersectionPoint,
            //    //              newSourcePoint - collision.NearestPolygonIntersectionPoint);

            //    // We now project the destination point onto the sliding plane
            //    double l = intersectRayPlane(destinationPoint, slidePlaneNormal,
            //                                slidePlaneOrigin, slidePlaneNormal);

            //    //if (PLANE.Intersects(new Ray(destinationPoint, slidePlane.Normal), slidePlane, ref l))
            //    //{
            //    // We can now calculate a new destination point on the sliding plane
            //    Vector3d newDestinationPoint;
            //    newDestinationPoint.x = destinationPoint.x + l * slidePlaneNormal.x;
            //    newDestinationPoint.y = destinationPoint.y + l * slidePlaneNormal.y;
            //    newDestinationPoint.z = destinationPoint.z + l * slidePlaneNormal.z;

            //    //newDestinationPoint = destinationPoint + (slidePlane.Normal * l);

            //    // Generate the slide vector, which will become our new velocity vector
            //    // for the next iteration
            //    Vector3d newVelocityVector = newDestinationPoint - collision.NearestPolygonIntersectionPoint;

            //    // now we recursively call the function with the new position and velocity 
            //    collision.LastSafePosition = position;
            //    return collideWithWorld(newSourcePoint, newVelocityVector, ref collision);
            //    //}
            //    //{
            //    //    throw new Exception("Unexpected.");
            //    //}
            //}

            //todo; debug.  Delete this line after uncommenting hte above
            return new Vector3d(0, 0, 0);
        }

        private double WrapAngle(double ang)
        {
            if (ang > 359) ang -= 360;
            if (ang < 0) ang += 360;
            return ang;
        }

        private bool isZeroVector(Vector3d v)
        {
            if ((v.x == 0.0f) && (v.y == 0.0f) && (v.z == 0.0f))
                return true;

            return false;
        }

        private Vector3d wedge(Vector3d v1, Vector3d v2)
        {
            Vector3d result;

            result.x = (v1.y*v2.z) - (v2.y*v1.z);
            result.y = (v1.z*v2.x) - (v2.z*v1.x);
            result.z = (v1.x*v2.y) - (v2.x*v1.y);

            return (result);
        }

        private enum VECTOR_CLASSIFICATION
        {
            PLANE_BACKSIDE = 0x000001,
            PLANE_FRONT = 0x000010,
            ON_PLANE = 0x000100
        }

        // ----------------------------------------------------------------------
        // Name  : classifyPoint()
        // Input : point - point we wish to classify 
        //         pO - Origin of plane
        //         pN - Normal to plane 
        // Notes : 
        // Return: One of 3 classification codes
        // -----------------------------------------------------------------------  

        private VECTOR_CLASSIFICATION classifyPoint(Vector3d point, Vector3d pO, Vector3d pN)
        {
            Vector3d dir = pO - point;
            double d = Vector3d.DotProduct(dir, pN);

            if (d < -0.001f)
                return VECTOR_CLASSIFICATION.PLANE_FRONT;
            else if (d > 0.001f)
                return VECTOR_CLASSIFICATION.PLANE_BACKSIDE;

            return VECTOR_CLASSIFICATION.ON_PLANE;
        }
    }
}