using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BEPUphysics.ForceFields
{
	public abstract class ForceField : Updateable
	{
		// Constructors
		protected ForceField ()
		{
		}
		
		
		// Methods
		protected abstract void calculateImpulseAtPoint (Vector3 pos, Entity e, float dt, float timeScale, float timeSinceLastFrame, out Vector3 impulse);
		public override void updateDuringForces (float dt, float timeScale, float timeSinceLastFrame)
		{
			bool flag1 = this.fieldShape.myInternalLinearVelocity != Toolbox.zeroVector;
			this.fieldShape.space = base.mySpace;
			this.fieldShape.update(dt, timeScale, true, true);
			this.fieldShape.findBoundingBox(dt);
			List<Entity> entities = ResourcePool.getEntityList();
			base.mySpace.myBroadPhase.getEntities(this.fieldShape.boundingBox, entities);
			if (this.fieldShape is CompoundBody)
			{
				foreach (Entity objA in entities)
				{
					if (!objA.isPhysicallySimulated || (!objA.myIsActive && !flag1))
					{
						continue;
					}
					List.Enumerator<Entity> enumerator2 = (List.Enumerator<Entity>) ((CompoundBody) this.fieldShape).subBodies.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							T t1 = enumerator2.Current;
							if (Toolbox.areObjectsCollidingMPR(objA, this.fieldShape, 0.00F, 0.00F))
							{
								Vector3 vector1;
								if (flag1)
								{
									objA.activate();
								}
								this.calculateImpulseAtPoint(objA.myInternalCenterPosition, objA, dt, timeScale, timeSinceLastFrame, out vector1);
								objA.applyLinearImpulse(ref vector1);
								break;
							}
						}
						continue;
					}
					finally
					{
						enumerator2.Dispose();
					}
				}
			}
			else
			{
				foreach (Entity e in entities)
				{
					if (!e.isPhysicallySimulated || (!e.myIsActive && !flag1))
					{
						continue;
					}
					if (Toolbox.areObjectsCollidingMPR(e, this.fieldShape, 0.00F, 0.00F))
					{
						Vector3 vector2;
						if (flag1)
						{
							e.activate();
						}
						this.calculateImpulseAtPoint(e.myInternalCenterPosition, e, dt, timeScale, timeSinceLastFrame, out vector2);
						e.applyLinearImpulse(ref vector2);
					}
				}
			}
			ResourcePool.giveBack(entities);
		}
		
		
		// Instance Fields
		public  Entity fieldShape;
	}
}
