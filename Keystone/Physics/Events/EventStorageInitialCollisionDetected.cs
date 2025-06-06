using Keystone.Physics;
using System.Runtime.InteropServices;

namespace Keystone.Physics.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct EventStorageInitialCollisionDetected
	{
		// Constructors
		internal EventStorageInitialCollisionDetected (Controller controller)
		{
			this.controller = controller;
		}
		
		
		// Instance Fields
		internal  Controller controller;
	}
}
