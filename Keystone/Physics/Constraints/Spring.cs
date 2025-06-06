using Physics;
using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class Spring : Updateable
	{
        // Instance Fields
        public PhysicsBody connectionA;
        public PhysicsBody connectionB;
        public bool useVelocityBasedSpring;
        public float strength;
        public float damping;
        public float maximumForce;
        public float restLength;
        public float expansionBreakLimit;
        public float compressionBreakLimit;
        public Vector3d localAnchorA;
        public Vector3d localAnchorB;
        private Vector3d anchorA;
        private Vector3d anchorB;

		// Constructors
        public Spring(PhysicsBody connectionA, PhysicsBody connectionB, Vector3d anchorA, Vector3d anchorB, float strength, float damping, float maximumForce, float expansionBreakLimit, float compressionBreakLimit)
		{
			if (connectionA == null)
			{
				connectionA = new Sphere(anchorA, 0.00F);
			}
			if (connectionB == null)
			{
				connectionB = new Sphere(anchorB, 0.00F);
			}
			this.connectionA = connectionA.myParent;
			this.localAnchorA = Vector3d.Transform(anchorA - connectionA.myInternalCenterPosition, Quaternion.Conjugate(connectionA.myInternalOrientationQuaternion));
			this.connectionB = connectionB.myParent;
			this.localAnchorB = Vector3d.Transform(anchorB - connectionB.myInternalCenterPosition, Quaternion.Conjugate(connectionB.myInternalOrientationQuaternion));
			this.strength = strength;
			this.damping = damping;
			this.maximumForce = maximumForce;
			this.expansionBreakLimit = expansionBreakLimit;
			this.compressionBreakLimit = compressionBreakLimit;
			Vector3 vector1 = anchorB - anchorA;
			this.restLength = vector1.Length();
		}

        // Properties
        public Vector3d worldAnchorA
        {
            get
            {
                return this.anchorA;
            }
            set
            {
                this.localAnchorA = Vector3d.Transform(this.anchorA - this.connectionA.myInternalCenterPosition, Quaternion.Conjugate(this.connectionA.myInternalOrientationQuaternion));
            }
        }

        public Vector3d worldAnchorB
        {
            get
            {
                return this.anchorB;
            }
            set
            {
                this.localAnchorB = Vector3d.Transform(this.anchorB - this.connectionB.myInternalCenterPosition, Quaternion.Conjugate(this.connectionB.myInternalOrientationQuaternion));
            }
        }
		
		// Methods
		public override void updateDuringForces (float dt, float timeScale, float timeSinceLastFrame)
		{
			if ((!this.connectionA.myIsActive && this.connectionB.myIsActive) && this.connectionA.isPhysicallySimulated)
			{
				this.connectionA.activate();
			}
			if ((this.connectionA.myIsActive && !this.connectionB.myIsActive) && this.connectionB.isPhysicallySimulated)
			{
				this.connectionB.activate();
			}
			if (this.connectionA.myIsActive || this.connectionB.myIsActive)
			{
				float single3;
				float value;
				Vector3d vector1 = Toolbox.zeroVector;
				Vector3d vector2 = Toolbox.zeroVector;
				vector1 = Vector3.Transform(this.localAnchorA, this.connectionA.myInternalOrientationMatrix);
				this.anchorA = vector1 + this.connectionA.myInternalCenterPosition;
				Vector3d vector3 = this.connectionA.myInternalLinearVelocity + Vector3.Cross(this.connectionA.myInternalAngularVelocity, vector1);
				vector2 = Vector3.Transform(this.localAnchorB, this.connectionB.myInternalOrientationMatrix);
				this.anchorB = vector2 + this.connectionB.myInternalCenterPosition;
				Vector3d vector4 = this.connectionB.myInternalLinearVelocity + Vector3.Cross(this.connectionB.myInternalAngularVelocity, vector2);
				Vector3d vector5 = vector4 - vector3;
				Vector3d vector6 = this.anchorB - this.anchorA;
				float single1 = vector6.Length();
				Vector3 vector7 = Toolbox.zeroVector;
				if (single1 > 0.00F)
				{
					vector7 = (Vector3) (vector6 / single1);
				}
				float single2 = (this.strength * dt) * (this.restLength - single1);
				if (this.connectionA.isPhysicallySimulated && this.connectionB.isPhysicallySimulated)
				{
					single3 = ((1.00F / this.connectionA.mass) + (1.00F / this.connectionB.mass)) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(vector1, vector7), this.connectionA.internalInertiaTensorInverse), vector1) + Vector3.Cross(Vector3.Transform(Vector3.Cross(vector2, vector7), this.connectionB.internalInertiaTensorInverse), vector2), vector7);
				}
				else if (this.connectionA.isPhysicallySimulated && !this.connectionB.isPhysicallySimulated)
				{
					single3 = (1.00F / this.connectionA.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(vector1, vector7), this.connectionA.internalInertiaTensorInverse), vector1), vector7);
				}
				else
				{
					single3 = (1.00F / this.connectionB.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(vector2, vector7), this.connectionB.internalInertiaTensorInverse), vector2), vector7);
				}
				float single4 = 1.00F - ((float) Math.Pow((double) MathHelper.Clamp(1.00F - this.damping, 0.00F, 1.00F), (double) dt));
				if (this.useVelocityBasedSpring)
				{
					value = Math.Min((float) ((single2 + (Vector3.Dot(Vector3.op_UnaryNegation(vector7), vector5) * single4)) / single3), this.maximumForce);
				}
				else
				{
					value = Math.Min((float) (single2 + ((Vector3.Dot(Vector3.op_UnaryNegation(vector7), vector5) * single4) / single3)), this.maximumForce);
				}
				if (Math.Abs(value) < base.mySpace.simulationSettings.minimumImpulse)
				{
					value = 0.00F;
				}
				if (((value / dt) < -this.expansionBreakLimit) || ((value / dt) > this.compressionBreakLimit))
				{
					base.mySpace.addToRemovalList(this);
				}
				else
				{
					Vector3d vector8 = (Vector3) (value * vector7);
					if (this.connectionA.isPhysicallySimulated)
					{
						Vector3d vector9 = new Vector3(-vector8.X, -vector8.Y, -vector8.Z);
						Vector3d vector10 = Vector3.Cross(vector1, vector9);
						this.connectionA.applyLinearImpulse(ref vector9);
						this.connectionA.applyAngularImpulse(ref vector10);
					}
					if (this.connectionB.isPhysicallySimulated)
					{
						Vector3d vector11 = Vector3.Cross(vector2, vector8);
						this.connectionB.applyLinearImpulse(ref vector8);
						this.connectionB.applyAngularImpulse(ref vector11);
					}
				}
			}
		}
	}
}
