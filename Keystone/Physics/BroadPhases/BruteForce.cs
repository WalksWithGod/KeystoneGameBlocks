using System;
using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;
using System.Collections.Generic;

namespace Keystone.Physics.BroadPhases
{
	public class BruteForce : BroadPhase
	{
		// Constructors
		public BruteForce ()
		{
		}
		
		
		// Methods
		public override void updateControllers (float dt, float timeSinceLastFrame)
		{
			for (int i = 0;i < (space.entities.Count - 1); i++)
			{
				for (int j = i + 1;j < space.entities.Count; j++)
				{
					if ((i != j) && isValidPair(space.entities[i], space.entities[j]))
					{
						addController(space.entities[i], space.entities[j]);
					}
				}
			}
			List<Controller> list = ResourcePool.getControllerList();
			foreach (Controller item in space.controllers)
			{
                if (!item.colliderA.CollisionPrimitive.BoundingBox.Intersects(item.colliderB.CollisionPrimitive.BoundingBox))
				{
					list.Add(item);
				}
			}
			foreach (Controller controller in list)
			{
				space.removeController(controller);
			}
			ResourcePool.giveBack(list);
		}
		
		public override void getEntities (BoundingBox box, List<PhysicsBody> entities)
		{
			foreach (PhysicsBody item in space.entities)
			{
                if (box.Contains(item.CollisionPrimitive.BoundingBox) != null)
				{
					entities.Add(item);
				}
			}
		}

        public override void getEntities(Keystone.Culling.ViewFrustum frustum, List<PhysicsBody> entities)
        {
            foreach (PhysicsBody item in space.entities)
            {
                if (frustum.IsVisible(item.CollisionPrimitive.BoundingBox))
                {
                    entities.Add(item);
                }
            }
        }
		
		public override void getEntities (BoundingSphere sphere, List<PhysicsBody> entities)
		{
            throw new NotImplementedException();
            //foreach (PhysicsBody item in space.entities)
            //{
            //    if (sphere.Intersects(item.CollisionPrimitive.BoundingBox))
            //    {
            //        entities.Add(item);
            //    }
            //}
		}
		

		public override bool rayCast (Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out PhysicsBody hitPhysicsBody, out Vector3d hitLocation, out Vector3d hitNormal, out double toi)
		{
			bool flag1 = false;
			hitLocation = Toolbox.noVector;
			hitNormal = Toolbox.noVector;
            toi = double.PositiveInfinity;
			hitPhysicsBody = null;
			foreach (PhysicsBody target in space.entities)
			{
				double single1;
				Vector3d vector1;
				Vector3d vector2;
				if ((Toolbox.rayCastGJKInfinite(origin, direction, target, false, out vector1, out vector2, out single1) && (single1 < toi)) && (single1 < maximumLength))
				{
					hitLocation = vector1;
					hitNormal = vector2;
					toi = single1;
					hitPhysicsBody = target;
					flag1 = true;
				}
			}
			return flag1;
		}

        public override bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals, List<double> tois)
		{
			bool flag1 = false;
			foreach (PhysicsBody target in space.entities)
			{
				Vector3d item;
				Vector3d vector2;
				double single1;
				if (Toolbox.rayCastGJKInfinite(origin, direction, target, withMargin, out item, out vector2, out single1) && (single1 < maximumLength))
				{
					hitEntities.Add(target);
					hitLocations.Add(item);
					hitNormals.Add(vector2);
					tois.Add(single1);
					flag1 = true;
				}
			}
			return flag1;
		}
		
	}
}
