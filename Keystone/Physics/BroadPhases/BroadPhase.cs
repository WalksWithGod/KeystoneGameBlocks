using Keystone.Types;
using System.Collections.Generic;

namespace Keystone.Physics.BroadPhases
{
	public abstract class BroadPhase
	{
        // Instance Fields
        public Space space;
        public object lockerBroadPhaseUpdating;
        protected int numAddControllerAttempts;

		// Constructors
		protected BroadPhase ()
		{
			lockerBroadPhaseUpdating = new object();
		}
		
		
		// Methods
        public abstract void getEntities(BoundingBox box, List<PhysicsBody> entities);
        public abstract void getEntities(Keystone.Culling.ViewFrustum frustum, List<PhysicsBody> entities);
        public abstract void getEntities(BoundingSphere sphere, List<PhysicsBody> entities);
        public virtual void addEntity(PhysicsBody e)
        {
        }

        public virtual void removeEntity(PhysicsBody e)
        {
        }

        public abstract bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out PhysicsBody hitPhysicsBody, out Vector3d hitLocation, out Vector3d hitNormal, out double toi);
        public abstract bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals, List<double> tois);

		public virtual void preUpdate (float dt, float timeSinceLastFrame)
		{
		}
		
		public abstract void updateControllers (float dt, float timeSinceLastFrame);
		public bool isValidPair (PhysicsBody a, PhysicsBody b)
		{
			if (((a.myIsActive || b.myIsActive) && ((a.isPhysicallySimulated || b.isPhysicallySimulated) 
                || (a.myIsDetector || b.myIsDetector))) && ((((a.collisionFilter & b.collisionFilter) > ulong.MinValue) 
                && !a.nonCollidableEntities.Contains(b)) && !b.nonCollidableEntities.Contains(a)))
			{
                return a.CollisionPrimitive.BoundingBox.Intersects(b.CollisionPrimitive.BoundingBox);
			}
			return false;
		}
		
		protected void addController (PhysicsBody e1, PhysicsBody e2)
		{
			CompoundBody body1 = e1 as CompoundBody;
			CompoundBody body2 = e2 as CompoundBody;
			bool flag1 = body1 != null;
			bool flag2 = body2 != null;
			if (flag1 && flag2)
			{
				List<PhysicsBody> nearbyEntities = ResourcePool.getEntityList();
				if (body1.numChildren < body2.numChildren)
				{
					foreach (PhysicsBody e in body1.allChildren)
					{
						nearbyEntities.Clear();
						body2.getEntitiesNearEntity(e, nearbyEntities);
						foreach (PhysicsBody b in nearbyEntities)
						{
							space.addController(e, b);
						}
					}
				}
				else
				{
					foreach (PhysicsBody a in body2.allChildren)
					{
						nearbyEntities.Clear();
						body1.getEntitiesNearEntity(a, nearbyEntities);
						foreach (PhysicsBody entity4 in nearbyEntities)
						{
							space.addController(a, entity4);
						}
					}
				}
				ResourcePool.giveBack(nearbyEntities);
			}
			else if (flag1 && !flag2)
			{
				List<PhysicsBody> list = ResourcePool.getEntityList();
				list.Clear();
				body1.getEntitiesNearEntity(e2, list);
				foreach (PhysicsBody entity5 in list)
				{
					space.addController(e2, entity5);
				}
				ResourcePool.giveBack(list);
			}
			else if (!flag1 && flag2)
			{
				List<PhysicsBody> list3 = ResourcePool.getEntityList();
				list3.Clear();
				body2.getEntitiesNearEntity(e1, list3);
				foreach (PhysicsBody entity6 in list3)
				{
					space.addController(e1, entity6);
				}
				ResourcePool.giveBack(list3);
			}
			else
			{
				space.addController(e1, e2);
			}
		}
		
		protected void removeController (PhysicsBody e1, PhysicsBody e2)
		{
			Controller controller;
			ControllerTableKey key = ResourcePool.getControllerTableKey(e1, e2);
			if (space.controllersTable.TryGetValue(key, out controller))
			{
				space.removeController(controller);
			}
			ResourcePool.giveBack(key);
		}
	}
}
