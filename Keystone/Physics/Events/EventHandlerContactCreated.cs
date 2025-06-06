using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;
using System;

namespace Keystone.Physics.Events
{
    public delegate void EventHandlerContactCreated(PhysicsBody sender, Controller controller, Vector3d position, Vector3d normal, double depth);
}
