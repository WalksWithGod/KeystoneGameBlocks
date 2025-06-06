using Physics;
using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class DistanceRangeConstraint : Constraint
	{

        // Instance Fields
        private Vector3d anchorA;
        private Vector3d anchorB;
        public float restLength;
        public float minimumLength;
        public float maximumLength;
        private float normalDenominator;
        private Vector3d separationNormal;
        private float distance;
        private float biasVelocity;
        private float accumulatedNormalImpulse;
        private bool wasFar;

		// Constructors
        public DistanceRangeConstraint(PhysicsBody connectionA, PhysicsBody connectionB, Vector3d anchorA, Vector3d anchorB, float minimumLength, float maximumLength, float softness, float biasFactor, float forceMax)
		{
			this.wasFar = true;
			if (connectionA == null)
			{
				connectionA = new Sphere(anchorA, 0.00F);
			}
			if (connectionB == null)
			{
				connectionB = new Sphere(anchorB, 0.00F);
			}
			base.myConnectionA = connectionA;
			base.myConnectionB = connectionB;
			base.softness = softness;
			base.biasFactor = biasFactor;
			this.minimumLength = minimumLength;
			this.maximumLength = maximumLength;
			base.myParentA = connectionA.myParent;
			base.myParentB = connectionB.myParent;
			base.localAnchorA = Vector3.Transform(anchorA - base.myParentA.myInternalCenterOfMass, Matrix.Transpose(base.myParentA.myInternalOrientationMatrix));
			base.localAnchorB = Vector3.Transform(anchorB - base.myParentB.myInternalCenterOfMass, Matrix.Transpose(base.myParentB.myInternalOrientationMatrix));
			Vector3d vector1 = anchorB - anchorA;
			this.restLength = vector1.Length();
			base.useForceLimit = true;
			base.forceMaximum = forceMax;
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
                base.localAnchorA = Vector3d.Transform(this.anchorA - base.myConnectionA.myInternalCenterPosition, Quaternion.Conjugate(base.myConnectionA.myInternalOrientationQuaternion));
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
                base.localAnchorB = Vector3d.Transform(this.anchorB - base.myConnectionB.myInternalCenterPosition, Quaternion.Conjugate(base.myConnectionB.myInternalOrientationQuaternion));
            }
        }

		// Methods
		public override void preStep (float dt)
		{
			if (base.useForceLimit && (Math.Abs(this.accumulatedNormalImpulse) > base.forceLimit))
			{
				base.mySpace.remove(this);
			}
			else
			{
				this.calculateJacobians();
				this.calculateMassMatrix();
				this.calculateError();
				this.calculateBias(dt);
				base.numIterationsAtZeroImpulse = 0;
				base.calculateImpulse = true;
				this.accumulatedNormalImpulse = 0.00F;
			}
		}
		
		protected override void calculateJacobians ()
		{
			base.rA = Vector3d.Transform(base.localAnchorA, base.myConnectionA.myInternalOrientationMatrix);
			base.rB = Vector3d.Transform(base.localAnchorB, base.myConnectionB.myInternalOrientationMatrix);
			this.anchorA = base.myConnectionA.myInternalCenterPosition + base.rA;
			this.anchorB = base.myConnectionB.myInternalCenterPosition + base.rB;
		}
		
		protected override void calculateError ()
		{
			Vector3d vector1 = this.anchorB - this.anchorA;
			this.distance = vector1.Length();
			this.separationNormal = Toolbox.zeroVector;
			if (this.distance > 0.00F)
			{
				this.separationNormal = (Vector3) (vector1 / this.distance);
			}
		}
		
		protected override void calculateMassMatrix ()
		{
			if (base.myConnectionA.isPhysicallySimulated && base.myConnectionB.isPhysicallySimulated)
			{
				this.normalDenominator = ((1.00F / base.myConnectionA.mass) + (1.00F / base.myConnectionB.mass)) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(base.rA, this.separationNormal), base.myConnectionA.internalInertiaTensorInverse), base.rA) + Vector3.Cross(Vector3.Transform(Vector3.Cross(base.rB, this.separationNormal), base.myConnectionB.internalInertiaTensorInverse), base.rB), this.separationNormal);
			}
			else if (base.myConnectionA.isPhysicallySimulated && !base.myConnectionB.isPhysicallySimulated)
			{
				this.normalDenominator = (1.00F / base.myConnectionA.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(base.rA, this.separationNormal), base.myConnectionA.internalInertiaTensorInverse), base.rA), this.separationNormal);
			}
			else
			{
				this.normalDenominator = (1.00F / base.myConnectionB.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(base.rB, this.separationNormal), base.myConnectionB.internalInertiaTensorInverse), base.rB), this.separationNormal);
			}
		}
		
		protected override void calculateBias (float dt)
		{
			if (this.distance > this.maximumLength)
			{
				this.biasVelocity = MathHelper.Clamp((base.biasFactor / dt) * (this.maximumLength - this.distance), -base.mySpace.simulationSettings.maximumPositionCorrectionSpeed, base.mySpace.simulationSettings.maximumPositionCorrectionSpeed);
			}
			else if (this.distance < this.minimumLength)
			{
				this.biasVelocity = MathHelper.Clamp((base.biasFactor / dt) * (this.minimumLength - this.distance), -base.mySpace.simulationSettings.maximumPositionCorrectionSpeed, base.mySpace.simulationSettings.maximumPositionCorrectionSpeed);
			}
			else
			{
				this.biasVelocity = 0.00F;
			}
		}
		
		public override void applyImpulse (float dt)
		{
			if (base.calculateImpulse)
			{
				float single3;
				Vector3 vector1 = base.myConnectionA.myInternalLinearVelocity + Vector3.Cross(base.myConnectionA.myInternalAngularVelocity, base.rA);
				Vector3 vector2 = base.myConnectionB.myInternalLinearVelocity + Vector3.Cross(base.myConnectionB.myInternalAngularVelocity, base.rB);
				Vector3 vector3 = vector2 - vector1;
				float single1 = Vector3.Dot(this.separationNormal, vector3);
				float single2 = 0.00F;
				if ((this.distance > this.maximumLength) && (single1 > 0.00F))
				{
					if (!this.wasFar)
					{
						this.accumulatedNormalImpulse = 0.00F;
					}
					this.wasFar = true;
					single2 = ((this.biasVelocity - Vector3.Dot(this.separationNormal, vector3)) - (base.softness * this.accumulatedNormalImpulse)) / this.normalDenominator;
					single3 = this.accumulatedNormalImpulse;
					this.accumulatedNormalImpulse = Math.Min((float) (this.accumulatedNormalImpulse + single2), (float) 0.00F);
					single2 = this.accumulatedNormalImpulse - single3;
				}
				else
				{
					if ((this.distance < this.minimumLength) && (single1 < 0.00F))
					{
						if (this.wasFar)
						{
							this.accumulatedNormalImpulse = 0.00F;
						}
						this.wasFar = false;
						single2 = ((this.biasVelocity - Vector3.Dot(this.separationNormal, vector3)) - (base.softness * this.accumulatedNormalImpulse)) / this.normalDenominator;
						single3 = this.accumulatedNormalImpulse;
						this.accumulatedNormalImpulse = Math.Max((float) (this.accumulatedNormalImpulse + single2), (float) 0.00F);
						single2 = this.accumulatedNormalImpulse - single3;
					}
				}
				if ((this.distance > this.minimumLength) && (this.distance < this.maximumLength))
				{
					this.accumulatedNormalImpulse = 0.00F;
					single2 = 0.00F;
				}
				Vector3 impulse = (Vector3) (single2 * this.separationNormal);
				base.applyImpulse(impulse);
				base.checkForEarlyOutIterations(impulse);
			}
		}
	}
}
