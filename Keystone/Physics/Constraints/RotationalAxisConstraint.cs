using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class RotationalAxisConstraint : SingleBodyConstraint
	{
        // Instance Fields
        public Vector3d disallowedRotationalAxis;
        public bool isLocal;
        private Vector3d restrictedAxis;
        private Matrix inertiaTensor;

		// Constructors
        public RotationalAxisConstraint(PhysicsBody target, Vector3d disallowedAxis, bool isInLocalSpace)
		{
			base.myEntity = target;
			this.disallowedRotationalAxis = disallowedAxis;
			this.isLocal = isInLocalSpace;
			this.disallowedRotationalAxis.Normalize();
		}
		
		
		// Methods
		public override void preStep (float dt)
		{
			base.calculateImpulse = true;
			if (this.isLocal)
			{
				this.restrictedAxis = Vector3.Transform(this.disallowedRotationalAxis, base.myEntity.orientationMatrix);
			}
			else
			{
				this.restrictedAxis = this.disallowedRotationalAxis;
			}
			this.inertiaTensor = Matrix.Invert(base.myEntity.internalInertiaTensorInverse);
		}
		
		public override void applyImpulse (float dt)
		{
			if (base.calculateImpulse)
			{
				float single1 = Vector3d.Dot(this.restrictedAxis, base.myEntity.myInternalAngularVelocity);
				Vector3d vector1 = (Vector3d) (this.restrictedAxis * single1);
				Vector3d impulse = Vector3d.op_UnaryNegation(Vector3.Transform(vector1, this.inertiaTensor));
				base.myEntity.applyAngularImpulse(ref impulse);
				base.checkForEarlyOutIterations(impulse);
			}
		}
	}
}
