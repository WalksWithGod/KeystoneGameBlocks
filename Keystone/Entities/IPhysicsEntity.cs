using System;
using Keystone.Types;

namespace Keystone.Entities
{
    // most things like mass, volume, and such we get through the PhysicsBody property directly
    // the main importance of the interface however is to tie things like Translation, Rotation, Scaling changes
    // made outside of the PhysicsEngine (editor, ISteerable agent, etc) to the PhysicsBody so that PhysicsEngine knows aobut the changes
    // similarly, you want changes to the PhysicsBody by the PhysicsEngine to update the Entity itself via IPhysicsEntity
    public interface IPhysicsEntity // maybe rename IRigidBody
    {
        Vector3d Mass { get; set;}
        Physics.PhysicsBody PhysicsBody { get;} // rename this PhysicsController and strip out all the properties that belong here

        bool PhysicsEnabled { get; set; }

        //HalfHeight
        Vector3d Translation { get; set; }

        // in the constructor we will create the physics body
        // but also the constructor needs to either pass a siimple primitive type 
        // (box, sphere, cone, etc which can be computed) or an actual collisionprimitive
        //  such as a convex hull or triangle list
        // 
        // public IPhysicsEntity()
        // {
        //      PhysicsBody = new Physics.PhysicsBody(this);
        //      Physics.Primitives.CollisionPrimitive primitve = new Physics.Primitives.BoxPrimitive(physicsBody, entity.BoundingBox);
        // }

    }
}
