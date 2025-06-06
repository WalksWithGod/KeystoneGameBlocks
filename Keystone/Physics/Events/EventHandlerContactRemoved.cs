using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;

namespace Keystone.Physics.Events
{
	public delegate void EventHandlerContactRemoved(PhysicsBody sender, Controller controller, Vector3d position, Vector3d normal, double depth);}
