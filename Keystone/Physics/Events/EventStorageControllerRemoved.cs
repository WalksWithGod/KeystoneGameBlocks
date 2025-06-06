using Keystone.Physics.Entities;
using System.Runtime.InteropServices;

namespace Keystone.Physics.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct EventStorageControllerRemoved
	{
		// Constructors
		internal EventStorageControllerRemoved (PhysicsBody otherPhysicsBody)
		{
			this.otherPhysicsBody = otherPhysicsBody;
		}
		
		
		// Instance Fields
		internal  PhysicsBody otherPhysicsBody;
	}
}
