using Physics.Entities;
using Core.Types;
using System;

namespace Physics.Constraints
{
	public class PointOnLineJoint : Constraint
	{
        // Instance Fields
        protected Vector3d axis;
        protected Vector3d localAxisA;
        protected Vector3d localOrthogonalBasisY;
        protected Vector3d localOrthogonalBasisZ;
        protected Vector3d orthogonalBasisY;
        protected Vector3d orthogonalBasisZ;
        protected Jacobian jacobianY;
        protected Jacobian jacobianZ;
        protected Matrix massMatrixZ;
        protected Vector3d accumulatedImpulseZ;
        protected Vector3d biasTimesTimeStepZ;
        protected Vector3d errorZ;
        private Vector3d imcheckingY;
        private Vector3d imcheckingZ;

		// Constructors
        public PointOnLineJoint(PhysicsBody conA, PhysicsBody conB, Vector3d anchorLocation, Vector3d lineDirection, float soft, float bias)
		{
			Vector3 vector1;
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
			this.axis = Vector3.Normalize(lineDirection);
			this.jacobianY = new Jacobian();
			this.jacobianZ = new Jacobian();
			base.myParentA = base.myConnectionA.myParent;
			base.myParentB = base.myConnectionB.myParent;
			base.rA = base.anchor - base.myParentA.myInternalCenterOfMass;
			base.rB = base.anchor - base.myParentB.myInternalCenterOfMass;
			base.localAnchorA = Vector3.Transform(base.rA, Matrix.Transpose(base.myConnectionA.myInternalOrientationMatrix));
			base.localAnchorB = Vector3.Transform(base.rB, Matrix.Transpose(base.myConnectionB.myInternalOrientationMatrix));
			this.localAxisA = Vector3.Transform(this.axis, Matrix.Transpose(base.myConnectionA.myInternalOrientationMatrix));
			if (this.localAxisA != Vector3.get_Right())
			{
				vector1 = Vector3d.Transform(this.localAxisA, Matrix.CreateFromAxisAngle(Vector3.get_Right(), 1.57F));
			}
			else
			{
				vector1 = Vector3d.Transform(this.localAxisA, Matrix.CreateFromAxisAngle(Vector3.get_Forward(), 1.57F));
			}
			this.localOrthogonalBasisY = Vector3.Normalize(Vector3.Cross(vector1, this.localAxisA));
			this.localOrthogonalBasisZ = Vector3.Normalize(Vector3.Cross(this.localAxisA, this.localOrthogonalBasisY));
			this.calculateJacobians();
		}

        public PointOnLineJoint(PhysicsBody conA, PhysicsBody conB, Vector3d anchorLocation, Vector3d lineDirection, float soft, float bias, float forceMax)
		{
			Vector3 vector1;
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
			this.axis = Vector3.Normalize(lineDirection);
			this.jacobianY = new Jacobian();
			this.jacobianZ = new Jacobian();
			base.myParentA = base.myConnectionA.myParent;
			base.myParentB = base.myConnectionB.myParent;
			base.rA = base.anchor - base.myParentA.myInternalCenterOfMass;
			base.rB = base.anchor - base.myParentB.myInternalCenterOfMass;
			base.localAnchorA = Vector3.Transform(base.rA, Matrix.Transpose(base.myConnectionA.myInternalOrientationMatrix));
			base.localAnchorB = Vector3.Transform(base.rB, Matrix.Transpose(base.myConnectionB.myInternalOrientationMatrix));
			this.localAxisA = Vector3.Transform(this.axis, Matrix.Transpose(base.myConnectionA.myInternalOrientationMatrix));
			if (this.localAxisA != Vector3.get_Right())
			{
				vector1 = Vector3d.Transform(this.localAxisA, Matrix.CreateFromAxisAngle(Vector3.get_Right(), 1.57F));
			}
			else
			{
				vector1 = Vector3d.Transform(this.localAxisA, Matrix.CreateFromAxisAngle(Vector3.get_Forward(), 1.57F));
			}
			this.localOrthogonalBasisY = Vector3.Normalize(Vector3.Cross(vector1, this.localAxisA));
			this.localOrthogonalBasisZ = Vector3.Normalize(Vector3.Cross(this.localAxisA, this.localOrthogonalBasisY));
			this.calculateJacobians();
			base.useForceLimit = true;
			base.forceMaximum = forceMax;
		}
		
		
		// Methods
		protected override void calculateJacobians ()
		{
			base.rA = Vector3d.Transform(base.localAnchorA, base.myConnectionA.myInternalOrientationMatrix);
			base.rB = Vector3d.Transform(base.localAnchorB, base.myConnectionB.myInternalOrientationMatrix);
			this.orthogonalBasisY = Vector3.Transform(this.localOrthogonalBasisY, base.myConnectionA.myInternalOrientationMatrix);
			this.orthogonalBasisZ = Vector3.Transform(this.localOrthogonalBasisZ, base.myConnectionA.myInternalOrientationMatrix);
			this.jacobianY.vecLinearA = Vector3.op_UnaryNegation(this.orthogonalBasisY);
			this.jacobianY.vecAngularA = Vector3.op_UnaryNegation(Vector3.Cross((base.myConnectionB.myInternalCenterPosition + base.rB) - base.myConnectionA.myInternalCenterPosition, this.orthogonalBasisY));
			this.jacobianY.vecLinearB = this.orthogonalBasisY;
			this.jacobianY.vecAngularB = Vector3.Cross(base.rB, this.orthogonalBasisY);
			this.jacobianZ.vecLinearA = Vector3.op_UnaryNegation(this.orthogonalBasisZ);
			this.jacobianZ.vecAngularA = Vector3.op_UnaryNegation(Vector3.Cross((base.myConnectionB.myInternalCenterPosition + base.rB) - base.myConnectionA.myInternalCenterPosition, this.orthogonalBasisZ));
			this.jacobianZ.vecLinearB = this.orthogonalBasisZ;
			this.jacobianZ.vecAngularB = Vector3.Cross(base.rB, this.orthogonalBasisZ);
			this.imcheckingY = Vector3.op_UnaryNegation(Vector3.Cross((base.myConnectionB.myInternalCenterPosition + base.rB) - base.myConnectionA.myInternalCenterPosition, this.orthogonalBasisY));
			this.imcheckingZ = Vector3.op_UnaryNegation(Vector3.Cross((base.myConnectionB.myInternalCenterPosition + base.rB) - base.myConnectionA.myInternalCenterPosition, this.orthogonalBasisZ));
		}
		
		protected override void calculateBias (float dt)
		{
			base.biasTimesTimeStep = (Vector3) (-(base.biasFactor / dt) * base.error);
			if (this.biasTimesTimeStep.LengthSquared() > (base.mySpace.simulationSettings.maximumPositionCorrectionSpeed * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed))
			{
				base.biasTimesTimeStep = (Vector3) (Vector3.Normalize(base.biasTimesTimeStep) * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed);
			}
			this.biasTimesTimeStepZ = (Vector3) (-(base.biasFactor / dt) * this.errorZ);
			if (this.biasTimesTimeStepZ.LengthSquared() > (base.mySpace.simulationSettings.maximumPositionCorrectionSpeed * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed))
			{
				this.biasTimesTimeStepZ = (Vector3) (Vector3.Normalize(this.biasTimesTimeStepZ) * base.mySpace.simulationSettings.maximumPositionCorrectionSpeed);
			}
		}
		
		protected override void calculateError ()
		{
			Vector3 vector1 = (base.myParentB.myInternalCenterPosition + base.rB) - (base.myParentA.myInternalCenterPosition + base.rA);
			base.error = (Vector3) (Vector3.Dot(vector1, this.orthogonalBasisY) * this.orthogonalBasisY);
			this.errorZ = (Vector3) (Vector3.Dot(vector1, this.orthogonalBasisZ) * this.orthogonalBasisZ);
		}
		
		public override void preStep (float dt)
		{
			if (base.useForceLimit)
			{
				Vector3 vector1 = base.accumulatedImpulse + this.accumulatedImpulseZ;
				if (vector1.LengthSquared() > base.forceLimitSquared)
				{
					base.mySpace.remove(this);
					return;
				}
			}
			this.calculateJacobians();
			this.calculateMassMatrix();
			this.calculateError();
			this.calculateBias(dt);
			base.numIterationsAtZeroImpulse = 0;
			base.calculateImpulse = true;
			base.applyImpulse(base.accumulatedImpulse + this.accumulatedImpulseZ);
		}
		
		protected override void calculateMassMatrix ()
		{
			Matrix matrix1 = Matrix.get_Identity();
			if (base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
			{
				matrix1 = (Matrix) (((((1.00F / base.myParentA.mass) * Matrix.get_Identity()) + ((1.00F / base.myParentB.mass) * Matrix.get_Identity())) + (base.myParentA.internalInertiaTensorInverse * Vector3.Dot(this.jacobianY.vecAngularA, this.jacobianY.vecAngularA))) + (base.myParentB.internalInertiaTensorInverse * Vector3.Dot(this.jacobianY.vecAngularB, this.jacobianY.vecAngularB)));
			}
			else if (base.myParentA.isPhysicallySimulated && !base.myParentB.isPhysicallySimulated)
			{
				matrix1 = (Matrix) ((((1.00F / base.myParentA.mass) * Vector3.Dot(this.jacobianY.vecLinearA, this.jacobianY.vecLinearA)) * Matrix.get_Identity()) + (base.myParentA.internalInertiaTensorInverse * Vector3.Dot(this.jacobianY.vecAngularA, this.jacobianY.vecAngularA)));
			}
			else
			{
				if (!base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
				{
					matrix1 = (Matrix) ((((1.00F / base.myParentB.mass) * Vector3.Dot(this.jacobianY.vecLinearB, this.jacobianY.vecLinearB)) * Matrix.get_Identity()) + (base.myParentB.internalInertiaTensorInverse * Vector3.Dot(this.jacobianY.vecAngularB, this.jacobianY.vecAngularB)));
				}
			}
			matrix1.M11 += base.softness;
			matrix1.M22 += base.softness;
			matrix1.M33 += base.softness;
			matrix1.M44 += base.softness;
			base.massMatrix = Matrix.Invert(matrix1);
			if (base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
			{
				matrix1 = (Matrix) (((((1.00F / base.myParentA.mass) * Matrix.get_Identity()) + ((1.00F / base.myParentB.mass) * Matrix.get_Identity())) + (base.myParentA.internalInertiaTensorInverse * Vector3.Dot(this.jacobianZ.vecAngularA, this.jacobianZ.vecAngularA))) + (base.myParentB.internalInertiaTensorInverse * Vector3.Dot(this.jacobianZ.vecAngularB, this.jacobianZ.vecAngularB)));
			}
			else if (base.myParentA.isPhysicallySimulated && !base.myParentB.isPhysicallySimulated)
			{
				matrix1 = (Matrix) ((((1.00F / base.myParentA.mass) * Vector3.Dot(this.jacobianZ.vecLinearA, this.jacobianZ.vecLinearA)) * Matrix.get_Identity()) + (base.myParentA.internalInertiaTensorInverse * Vector3.Dot(this.jacobianZ.vecAngularA, this.jacobianZ.vecAngularA)));
			}
			else
			{
				if (!base.myParentA.isPhysicallySimulated && base.myParentB.isPhysicallySimulated)
				{
					matrix1 = (Matrix) ((((1.00F / base.myParentB.mass) * Vector3.Dot(this.jacobianZ.vecLinearB, this.jacobianZ.vecLinearB)) * Matrix.get_Identity()) + (base.myParentB.internalInertiaTensorInverse * Vector3.Dot(this.jacobianZ.vecAngularB, this.jacobianZ.vecAngularB)));
				}
			}
			matrix1.M11 += base.softness;
			matrix1.M22 += base.softness;
			matrix1.M33 += base.softness;
			matrix1.M44 += base.softness;
			this.massMatrixZ = Matrix.Invert(matrix1);
		}
		
		public override void applyImpulse (float dt)
		{
			if (base.calculateImpulse)
			{
				Vector3 vector1 = base.myConnectionA.myInternalLinearVelocity + Vector3.Cross(base.myConnectionA.myInternalAngularVelocity, base.rA);
				Vector3 vector2 = base.myConnectionB.myInternalLinearVelocity + Vector3.Cross(base.myConnectionB.myInternalAngularVelocity, base.rB);
				Vector3 vector3 = (Vector3) (Vector3.Dot(vector1, this.orthogonalBasisY) * this.orthogonalBasisY);
				Vector3 vector4 = (Vector3) (Vector3.Dot(vector1, this.orthogonalBasisZ) * this.orthogonalBasisZ);
				Vector3 vector5 = (Vector3) (Vector3.Dot(vector2, this.orthogonalBasisY) * this.orthogonalBasisY);
				Vector3 vector6 = (Vector3) (Vector3.Dot(vector2, this.orthogonalBasisZ) * this.orthogonalBasisZ);
				Vector3 vector7 = vector5 - vector3;
				Vector3 vector8 = vector6 - vector4;
				Vector3 vector9 = Vector3.Transform((base.biasTimesTimeStep - vector7) - ((Vector3) (base.softness * base.accumulatedImpulse)), base.massMatrix);
				Vector3 vector10 = Vector3.Transform((this.biasTimesTimeStepZ - vector8) - ((Vector3) (base.softness * this.accumulatedImpulseZ)), this.massMatrixZ);
				base.accumulatedImpulse += vector9;
				this.accumulatedImpulseZ += vector10;
				Vector3 impulse = vector9 + vector10;
				base.applyImpulse(impulse);
				base.checkForEarlyOutIterations(impulse);
			}
		}
	}
}
