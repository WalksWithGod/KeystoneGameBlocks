using Physics;
using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class GrabConstraint : Updateable
	{
        // Instance Fields
        public PhysicsBody  entity;
        public Vector3d localOffset;
        public Vector3d goalPosition;
        public float correctionFactor;
        public float linearDamping;
        public float angularDamping;
        private Vector3d grabbedPosition;

		// Constructors
        public GrabConstraint(PhysicsBody e, Vector3d grabbedLocation, Vector3d goalLocation, float correctiveStrength, float linearDamp, float angularDamp) 
            : this(correctiveStrength , linearDamp , angularDamp )
		{
			this.entity = e;
			this.localOffset = Vector3.Transform(grabbedLocation - e.myInternalCenterOfMass, Quaternion.Conjugate(e.myInternalOrientationQuaternion));
			this.goalPosition = goalLocation;
		}

        public GrabConstraint(float correctiveStrength, float linearDamp, float angularDamp)
        {
            this.correctionFactor = correctiveStrength;
            this.linearDamping = linearDamp;
            this.angularDamping = angularDamp;
        }

        // Properties
        public Vector3d entityGrabPosition
        {
            get
            {
                return this.grabbedPosition;
            }
        }
		
		
		// Methods
        public void setup(PhysicsBody e, Vector3d grabbedLocation, Vector3d goalLocation)
		{
			this.entity = e;
			this.localOffset = Vector3.Transform(grabbedLocation - e.myInternalCenterOfMass, Quaternion.Conjugate(e.myInternalOrientationQuaternion));
			this.goalPosition = goalLocation;
		}
		
		public override void updateDuringForces (float dt, float timeScale, float timeSinceLastFrame)
		{
			if (this.entity.isPhysicallySimulated)
			{
				Vector3d pos = Vector3d.Transform(this.localOffset, this.entity.myInternalOrientationMatrix) + this.entity.myInternalCenterOfMass;
				Vector3d vector2 = this.goalPosition - pos;
				this.entity.applyImpulse(pos, (Vector3d) (((vector2 * this.correctionFactor) * dt) * this.entity.mass));
				this.entity.modifyLinearDamping(this.linearDamping);
				this.entity.modifyAngularDamping(this.angularDamping);
			}
		}
		
		public override void updateAtEndOfFrame (float dt, float timeScale, float timeSinceLastFrame)
		{
			this.grabbedPosition = Vector3d.Transform(this.localOffset, this.entity.myOrientationMatrix) + this.entity.myCenterOfMass;
		}
	}
}
