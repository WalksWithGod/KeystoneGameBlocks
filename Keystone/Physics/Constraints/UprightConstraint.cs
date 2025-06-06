using Physics;
using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class UprightConstraint : Updateable
	{

        // Instance Fields
        public PhysicsBody entity;
        private Vector3d localUpVector;
        private Vector3d realUpVector;
        public float minimumAngle;
        public float maximumAngle;
        public float correctionFactor;
        public bool useVelocityBasedSpring;

		// Constructors
        public UprightConstraint(PhysicsBody target, Vector3d upDirection, float minAngle, float maxAngle, float correctionStrength)
		{
			this.maximumAngle = 3.14F;
			this.entity = target;
			this.upVector = upDirection;
			this.minimumAngle = minAngle;
			this.maximumAngle = maxAngle;
			this.correctionFactor = correctionStrength;
		}

        // Properties
        public Vector3d upVector
        {
            get
            {
                return this.realUpVector;
            }
            set
            {
                this.realUpVector = Vector3d.Normalize(value);
                this.localUpVector = Vector3d.Transform(this.realUpVector, Matrix.Invert(this.entity.orientationMatrix));
            }
        }
		
		// Methods
		public override void updateDuringForces (float dt, float timeScale, float timeSinceLastFrame)
		{
			Vector3d vector1 = Vector3d.Transform(this.localUpVector, this.entity.myInternalOrientationMatrix);
			Vector3d vector2 = Vector3d.Cross(vector1, this.realUpVector);
			float single1 = vector2.Length();
			float single2 = (float) Math.Asin((double) single1);
			if (Vector3d.Dot(vector1, this.realUpVector) < 0.00F)
			{
				single2 = 3.14F - single2;
			}
			if ((single2 > this.minimumAngle) && (single2 < this.maximumAngle))
			{
				single2 -= this.minimumAngle;
				vector2.Normalize();
				if (this.useVelocityBasedSpring)
				{
					this.entity.angularVelocity += (Vector3d) (vector2 * ((single2 * dt) * this.correctionFactor));
				}
				else
				{
					this.entity.applyAngularImpulse((Vector3d) (vector2 * ((single2 * dt) * this.correctionFactor)));
				}
			}
		}
	}
}
