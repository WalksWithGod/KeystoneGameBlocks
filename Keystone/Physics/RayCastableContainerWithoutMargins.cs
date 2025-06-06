using Keystone.Physics.Entities;
using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics
{
	public interface RayCastableContainerWithoutMargins
	{
		// Methods
        bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, out PhysicsBody hitPhysicsBody, out Vector3d hitLocation, out Vector3d hitNormal, out double toi);
        bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals, List<double> tois);
	}
}
