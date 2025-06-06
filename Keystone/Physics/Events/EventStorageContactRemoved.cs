using Keystone.Physics;
using Keystone.Types;
using System;
using System.Runtime.InteropServices;

namespace Keystone.Physics.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct EventStorageContactRemoved
	{
		// Constructors
        internal EventStorageContactRemoved(Controller controller, Vector3d position, Vector3d normal, double depth)
		{
			this.controller = controller;
			this.position = position;
			this.normal = normal;
			this.depth = depth;
		}
		
		
		// Instance Fields
		internal  Controller controller;
		internal  Vector3d position;
		internal  Vector3d normal;
        internal double depth;
	}
}
