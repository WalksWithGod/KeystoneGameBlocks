using Physics;
using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class MaximumSpeedConstraint : SingleBodyConstraint
	{
        // Instance Fields
        public float maximumLinearSpeed;
        public float maximumAngularSpeed;
        private Vector3d linearVelocityDirection;
        private Vector3d angularVelocityDirection;
        private Matrix inertiaTensor;

		// Constructors
        public MaximumSpeedConstraint(PhysicsBody e, float maxLinearSpeed, float maxAngularSpeed)
		{
			base.myEntity = e;
			this.maximumLinearSpeed = maxLinearSpeed;
			this.maximumAngularSpeed = maxAngularSpeed;
		}
		
		
		// Methods
		public override void preStep (float dt)
		{
			float single1 = base.myEntity.linearVelocity.LengthSquared();
			if (single1 > 0.00F)
			{
				this.linearVelocityDirection = (Vector3d) (base.myEntity.linearVelocity / ((float) Math.Sqrt((double) single1)));
			}
			else
			{
				this.linearVelocityDirection = Toolbox.zeroVector;
			}
			float single2 = base.myEntity.angularVelocity.LengthSquared();
			if (single2 > 0.00F)
			{
				this.angularVelocityDirection = (Vector3d) (base.myEntity.angularVelocity / ((float) Math.Sqrt((double) single2)));
			}
			else
			{
				this.angularVelocityDirection = Toolbox.zeroVector;
			}
			this.inertiaTensor = Matrix.Invert(base.myEntity.internalInertiaTensorInverse);
		}
		
		public override void applyImpulse (float dt)
		{
			float single1 = Vector3d.Dot(base.myEntity.myInternalLinearVelocity, this.linearVelocityDirection);
			if (single1 > this.maximumLinearSpeed)
			{
				float single2 = this.maximumLinearSpeed - single1;
				Vector3d vector1 = (Vector3d) ((single2 * base.myEntity.mass) * this.linearVelocityDirection);
				base.myEntity.applyLinearImpulse(ref vector1);
			}
			float single3 = Vector3d.Dot(base.myEntity.myInternalAngularVelocity, this.angularVelocityDirection);
			if (single3 > this.maximumAngularSpeed)
			{
				float single4 = this.maximumAngularSpeed - single3;
				Vector3d vector2 = Vector3d.Transform((Vector3) (single4 * this.angularVelocityDirection), this.inertiaTensor);
				base.myEntity.applyAngularImpulse(ref vector2);
			}
		}
	}
}
