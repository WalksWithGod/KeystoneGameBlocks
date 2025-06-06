using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;

namespace BEPUphysics.ForceFields
{
	public class PushField : ForceField
	{
		// Constructors
		public PushField (Entity shape, Vector3 forceToApply, float maxPushSpeed)
		{
			base.fieldShape = shape;
			this.force = forceToApply;
			this.maximumPushSpeed = maxPushSpeed;
		}
		
		
		// Methods
		protected override void calculateImpulseAtPoint (Vector3 pos, Entity e, float dt, float timeScale, float timeSinceLastFrame, out Vector3 impulse)
		{
			if (this.maximumPushSpeed > 0.00F)
			{
				float val1 = this.force.Length();
				Vector3 vector1 = (Vector3) (this.force / val1);
				float single2 = (Vector3.Dot(e.myInternalLinearVelocity, vector1) * e.mass) + (val1 * dt);
				single2 = Math.Min(val1, Math.Max((float) ((this.maximumPushSpeed * e.mass) - single2), (float) 0.00F));
				impulse = (Vector3) (single2 * vector1);
			}
			else
			{
				impulse = (Vector3) (this.force * dt);
			}
		}
		
		
		// Instance Fields
		public  Vector3 force;
		public  float maximumPushSpeed;
	}
}
