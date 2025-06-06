using Physics.Entities;
using Core.Types;
using System;
using Physics.Constraints;

namespace Physics.Constraints
{
	public class BallSocketJoint : Constraint
	{
		// Constructors
        public BallSocketJoint(PhysicsBody conA, PhysicsBody conB, Vector3d anchorLocation, float soft, float bias)
		{
			if (conA == null)
			{
				conA = new Sphere(anchorLocation, 0.00F);
			}
			if (conB == null)
			{
				conB = new Sphere(anchorLocation, 0.00F);
			}
			base.anchor = anchorLocation;
			base.myConnectionA = conA;
			base.myConnectionB = conB;
			base.softness = soft;
			base.biasFactor = bias;
			base.myParentA = base.myConnectionA.myParent;
			base.myParentB = base.myConnectionB.myParent;
			base.localAnchorA = Vector3d.Transform(base.anchor - base.myParentA.myInternalCenterOfMass, Matrix.Transpose(base.myParentA.myInternalOrientationMatrix));
			base.localAnchorB = Vector3d.Transform(base.anchor - base.myParentB.myInternalCenterOfMass, Matrix.Transpose(base.myParentB.myInternalOrientationMatrix));
		}

        public BallSocketJoint(PhysicsBody conA, PhysicsBody conB, Vector3d anchorLocation, float soft, float bias, float forceMax)
		{
			if (conA == null)
			{
				conA = new Sphere(anchorLocation, 0.00F);
			}
			if (conB == null)
			{
				conB = new Sphere(anchorLocation, 0.00F);
			}
			base.anchor = anchorLocation;
			base.myConnectionA = conA;
			base.myConnectionB = conB;
			base.softness = soft;
			base.biasFactor = bias;
			base.myParentA = base.myConnectionA.myParent;
			base.myParentB = base.myConnectionB.myParent;
			base.localAnchorA = Vector3.Transform(base.anchor - base.myParentA.myInternalCenterOfMass, Matrix.Transpose(base.myParentA.myInternalOrientationMatrix));
			base.localAnchorB = Vector3.Transform(base.anchor - base.myParentB.myInternalCenterOfMass, Matrix.Transpose(base.myParentB.myInternalOrientationMatrix));
			base.useForceLimit = true;
			base.forceMaximum = forceMax;
		}
		
		
		// Methods
		public override void preStep (float dt)
		{
			if (base.useForceLimit && (this.accumulatedImpulse.LengthSquared() > base.forceLimitSquared))
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
				base.applyImpulse(base.accumulatedImpulse);
			}
		}
		
		protected override void calculateJacobians ()
		{
			base.rA = Vector3.Transform(base.localAnchorA, base.myConnectionA.myInternalOrientationMatrix);
			base.rB = Vector3.Transform(base.localAnchorB, base.myConnectionB.myInternalOrientationMatrix);
		}
		
		protected override void calculateError ()
		{
			base.error = (base.myParentB.myInternalCenterPosition + base.rB) - (base.myParentA.myInternalCenterPosition + base.rA);
		}
		
		protected override void calculateBias (float dt)
		{
			base.biasTimesTimeStep = (Vector3d) (-(base.biasFactor / dt) * base.error);
			if (this.biasTimesTimeStep.LengthSquared() > (base.mySpace.simulationSettings.maximumPositionCorrectionSpeed * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed))
			{
				base.biasTimesTimeStep = (Vector3d) (Vector3d.Normalize(base.biasTimesTimeStep) * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed);
			}
		}
		
		protected override void calculateMassMatrix ()
		{
			Matrix matrix1 = Toolbox.getCrossProductMatrix(base.rA);
			Matrix matrix2 = Toolbox.getCrossProductMatrix(base.rB);
			Matrix matrix3 = Matrix.get_Identity();
			if (base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
			{
				matrix3 = (((Matrix) (((1.00F / base.myParentA.mass) + (1.00F / base.myParentB.mass)) * Matrix.get_Identity())) - ((matrix1 * base.myParentA.internalInertiaTensorInverse) * matrix1)) - ((matrix2 * base.myParentB.internalInertiaTensorInverse) * matrix2);
			}
			else if (base.myParentA.isPhysicallySimulated && !base.myParentB.isPhysicallySimulated)
			{
				matrix3 = ((Matrix) ((1.00F / base.myParentA.mass) * Matrix.get_Identity())) - ((matrix1 * base.myParentA.internalInertiaTensorInverse) * matrix1);
			}
			else
			{
				if (!base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
				{
					matrix3 = ((Matrix) ((1.00F / base.myParentB.mass) * Matrix.get_Identity())) - ((matrix2 * base.myParentB.internalInertiaTensorInverse) * matrix2);
				}
			}
			matrix3.M11 += base.softness;
			matrix3.M22 += base.softness;
			matrix3.M33 += base.softness;
			matrix3.M44 += base.softness;
			base.massMatrix = Matrix.Invert(matrix3);
		}
		
		public override void applyImpulse (float dt)
		{
			if (base.calculateImpulse)
			{
				Vector3d vector1 = base.myConnectionA.myInternalLinearVelocity + Vector3d.Cross(base.myConnectionA.myInternalAngularVelocity, base.rA);
				Vector3d vector2 = base.myConnectionB.myInternalLinearVelocity + Vector3d.Cross(base.myConnectionB.myInternalAngularVelocity, base.rB);
				Vector3d vector3 = vector2 - vector1;
				Vector3d impulse = Vector3d.Transform((base.biasTimesTimeStep - vector3) - ((Vector3d) (base.softness * base.accumulatedImpulse)), base.massMatrix);
				base.applyImpulse(impulse);
				base.accumulatedImpulse += impulse;
				base.checkForEarlyOutIterations(impulse);
			}
		}
		
	}
}
