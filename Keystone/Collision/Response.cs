using System;
using System.Collections.Generic;
using Keystone.Entities;
using Keystone.Types;
using MTV3D65;
using Keystone.Octree;

namespace Keystone.Collision
{
    //-----------------------------------------------------------------------------
    // File: Response.cpp
    //
    // Desc: Implementation of the collision response
    //
    // Copyright (c) 2000 Telemachos of Peroxide
    // www.peroxide.dk
    //-----------------------------------------------------------------------------
    public class CollisionResponse
    {
        private OctreeOctant _root;
        private PlayerCharacter[] _players;
        private const double EPSILON = 0.05f;


        public CollisionResponse(OctreeOctant root, PlayerCharacter[] p)
        {
            _root = root;
            _players = p;
        }

        public void Update(int elapsed)
        {
            //// players must track their own collision object which includes their last results for comparison
            //// to determine things like being "stuck"
            //foreach (Player p in _players)
            //{

            //    // do we need to worry ?
            //    if (Core._CoreClient.Maths.VLength(p.Velocity) < EPSILON)
            //        continue;

            //    // create a collision info structure to track the collision status
            //    TCollisionPacket collision = new TCollisionPacket(p.Translation, p.Velocity);

            //    Vector3d newPosition = Slide(collision);
            //    p.Translation = newPosition;
            //}
        }


        ////-----------------------------------------------------------------------------
        //// Name: Slide()
        //// Desc: Main collision detection function. This is what you call to get
        ////       a position.
        ////-----------------------------------------------------------------------------
        public Vector3d Slide(TCollisionPacket collision)
        {
            Vector3d scaledPosition, scaledVelocity;
            Vector3d finalPosition;
            Vector3d ellipsoidRadius = collision.eRadius;

            // the first thing we do is scale the player and his velocity to
            // ellipsoid space
            scaledPosition = collision.ScaledPosition; // position / ellipsoidRadius;
            scaledVelocity = collision.ScaledVelocity; // velocity / ellipsoidRadius;

            // call the recursive collision response function	
            finalPosition = collideWithWorld(scaledPosition, scaledVelocity, ref collision);

            // when the function returns the result is still in ellipsoid space, so
            // we have to scale it back to R3 before we return it 	
            finalPosition = new Vector3d(finalPosition.x*ellipsoidRadius.x, finalPosition.y*ellipsoidRadius.y,
                                         finalPosition.z*ellipsoidRadius.z);

            return finalPosition;
        }


        //private Vector3d Slide(Vector3d OldPos, Vector3d NewPos)
        //{
        //    Vector3d Normal = new Vector3d();
        //    Vector3d Impact;
        //    Vector3d Final;
        //    Vector3d Relative;


        //    TV_COLLISIONRESULT CollisionResult = new TV_COLLISIONRESULT();

        //    Core.Core._CoreClient.Scene.AdvancedCollision(OldPos, NewPos, ref CollisionResult,
        //                                  (int) (CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH),
        //                                  CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING);
        //    if (CollisionResult.bHasCollided)
        //    {
        //        Normal.x = CollisionResult.vCollisionNormal.x;
        //        Normal.y = CollisionResult.vCollisionNormal.y;
        //        Normal.z = CollisionResult.vCollisionNormal.z;

        //        Impact.x = CollisionResult.vCollisionImpact.x;
        //        Impact.y = CollisionResult.vCollisionImpact.y;
        //        Impact.z = CollisionResult.vCollisionImpact.z;

        //        //Normal = maths.VNormalize(Normal);
        //        Normal = Core.Core._CoreClient.Maths.VScale(Normal, DIST_FROM_WALL);
        //        //Final = maths.VAdd(Impact, Normal);
        //        Final = new Vector3d(Impact.x + Normal.x, Impact.y + Normal.y, Impact.z + Normal.z);
        //        return Final;
        //    }

        //    return NewPos;
        //}

        //-----------------------------------------------------------------------------
        // Name: collideWithWorld()
        // Desc: Recursive part of the collision response. This function is the
        //       one who actually calls the collision check on the meshes
        //-----------------------------------------------------------------------------
        private Vector3d collideWithWorld(Vector3d position, Vector3d velocity, ref TCollisionPacket collision)
        {
            //Vector3d destinationPoint = position + velocity;

            //// get a pointer to your meshes in some way
            //Picker pick = new Picker(_root );
            //pick.Apply(_root);
            //Mesh3d[] meshes = pick.FoundMeshes; 


            //// run a ray test and return all meshes who's bounding boxes intersect
            //// itterate thru the hull data of each

            //// For all candiate meshes (meshes who's bounding boxes tested positive
            //// Check against the current entity's ellipsoid
            //foreach (Mesh3d m in meshes)
            //{
            //    CheckCollision(collision, m.Hull);
            //}


            //// check return value here, and possibly call recursively 	
            //if (collision.FoundCollision == false)
            //{
            //    // if no collision move very close to the desired destination. 
            //    double l = Core._CoreClient.Maths.VLength(velocity);
            //    Vector3d V = velocity;

            //    //setLength(V, l - EPSILON); // this is equivalent to scaling the vector by newlength/currentLength 
            //    V *= ((l - EPSILON) / l);

            //    // update the last safe position for future error recovery	
            //    collision.LastSafePosition = position;

            //    // return the final position
            //    return position + V;
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
            //        //setLength(V, collision.nearestDistance - EPSILON);  // scale the vector by newlength / currentlength
            //        double len = Core._CoreClient.Maths.VLength(V);
            //        V = Core._CoreClient.Maths.VScale(V, (collision.NearestDistance - EPSILON) /  len);
            //        newSourcePoint = collision.SourcePoint + V;
            //    }
            //    else
            //        newSourcePoint = collision.SourcePoint;


            //    // Now we must calculate the sliding plane
            //    //Vector3d slidePlaneOrigin = collision.nearestPolygonIntersectionPoint;
            //    //Vector3d slidePlaneNormal = newSourcePoint - collision.nearestPolygonIntersectionPoint;
            //    PLANE slidePlane =
            //        new PLANE(collision.NearestPolygonIntersectionPoint,
            //                  newSourcePoint - collision.NearestPolygonIntersectionPoint);

            //    // We now project the destination point onto the sliding plane
            //    double l = 0; //= intersectRayPlane(destinationPoint, slidePlaneNormal,
            //    //                            slidePlaneOrigin, slidePlaneNormal);

            //    if (PLANE.Intersects(new Ray( destinationPoint, slidePlane.Normal ), slidePlane, ref l))
            //    {
            //        // We can now calculate a new destination point on the sliding plane
            //        Vector3d newDestinationPoint;
            //        //newDestinationPoint.x = destinationPoint.x + l * slidePlaneNormal.x;
            //        //newDestinationPoint.y = destinationPoint.y + l * slidePlaneNormal.y;
            //        //newDestinationPoint.z = destinationPoint.z + l * slidePlaneNormal.z;

            //        newDestinationPoint = destinationPoint + (slidePlane.Normal*l);

            //        // Generate the slide vector, which will become our new velocity vector
            //        // for the next iteration
            //        Vector3d newVelocityVector = newDestinationPoint - collision.NearestPolygonIntersectionPoint;

            //        // now we recursively call the function with the new position and velocity 
            //        collision.LastSafePosition = position;
            //        return collideWithWorld(newSourcePoint, newVelocityVector, ref collision);
            //    }
            //    {
            //        throw new Exception("Unexpected.");
            //    }
            //}
            return new Vector3d();
        }
    }
}