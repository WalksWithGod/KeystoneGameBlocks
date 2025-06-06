using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;

namespace BEPUphysics.ForceFields
{
	public class GravitationalField : ForceField
	{
		// Constructors
		public GravitationalField (Entity shape, float Gm, float forceMax)
		{
			base.fieldShape = shape;
			this.multiplier = Gm;
			this.maxForce = forceMax;
		}
		
		
		// Methods
		protected override void calculateImpulseAtPoint (Vector3 pos, Entity e, float dt, float timeScale, float timeSinceLastFrame, out Vector3 impulse)
		{
			Vector3 vector1 = pos - base.fieldShape.myInternalCenterPosition;
			if (vector1 == Toolbox.zeroVector)
			{
				impulse = Toolbox.zeroVector;
			}
			else
			{
				float single1 = dt * Math.Min(this.maxForce, (float) ((this.multiplier * e.mass) / vector1.LengthSquared()));
				impulse = (Vector3) (-single1 * vector1);
			}
		}
		
		
		// Instance Fields
		public  float multiplier;
		public  float maxForce;
	}
}
