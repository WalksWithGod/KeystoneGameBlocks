using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BEPUphysics.Vehicle
{
	public abstract class Wheel
	{
		// Constructors
		protected Wheel ()
		{
			this.frictionType = WheelFrictionTypes.averageFriction;
			this.steeringRotation = Matrix.get_Identity();
			this.updateTireSpinInternally = true;
			this.stopTiresOnBrake = true;
			this.stopTireGraphicsOnBrake = true;
			this.supportLocations = new List<Vector3>();
			this.supportNormals = new List<Vector3>();
			this.supportTois = new List<float>();
			this.supportDepths = new List<float>();
			this.supportEntities = new List<Entity>();
			this.calculateRollingFrictionImpulse = true;
			this.calculateSlidingFrictionImpulse = true;
		}
		
		
		// Methods
		public void move (Vector3 v)
		{
			this.detectorShape.centerPosition += v;
		}
		
		internal void preStep (float dt)
		{
			this.supportDepths.Clear();
			this.supportEntities.Clear();
			this.supportLocations.Clear();
			this.supportNormals.Clear();
			this.supportTois.Clear();
			this.worldRayDirection = Vector3.Transform(this.localRayDirection, this.vehicle.body.myInternalOrientationMatrix);
			Matrix matrix1 = this.totalLocalRotation * this.vehicle.body.myInternalOrientationMatrix;
			this.worldForward = Vector3.Transform(this.forward, matrix1);
			this.worldSide = Vector3.Cross(this.worldForward, this.worldRayDirection);
			bool flag1 = false;
			if (this.isBraking)
			{
				if (this.stopTireGraphicsOnBrake)
				{
					flag1 = true;
				}
				if (this.stopTiresOnBrake)
				{
					this.slideFrictionToUse = this.brakeFriction;
				}
				else
				{
					this.slideFrictionToUse = this.slideFriction;
				}
				this.rollingFrictionToUse = this.brakeFriction;
			}
			else
			{
				this.rollingFrictionToUse = this.rollingFriction;
				this.slideFrictionToUse = this.slideFriction;
			}
			this.chassisConnection = Vector3.Transform(this.localChassisConnection, this.vehicle.body.myInternalOrientationMatrix) + this.vehicle.body.myInternalCenterPosition;
			this.supported = false;
			this.getSupportRayHits(dt, this.supportLocations, this.supportNormals, this.supportTois, this.supportDepths, this.supportEntities);
			if (this.supportEntities.Count > 0)
			{
				Vector3 vector1;
				float single1;
				float single5;
				this.supported = true;
				int num1 = 0;
				float single2 = float.PositiveInfinity;
				for (int i = 0;i < this.supportTois.Count; i++)
				{
					if (this.supportTois[i] < single2)
					{
						single2 = this.supportTois[i];
						num1 = i;
					}
				}
				this.supportLocation = this.supportLocations[num1];
				this.supportNormal = this.supportNormals[num1];
				this.supportToi = this.supportTois[num1];
				this.supportEntity = this.supportEntities[num1];
				this.supportDepth = this.supportDepths[num1];
				this.supportEntityParent = this.supportEntity.myParent;
				float val1 = this.tireGrip;
				if (this.frictionType == WheelFrictionTypes.averageFriction)
				{
					this.slideFrictionToUse = (this.slideFrictionToUse + this.supportEntity.myDynamicFriction) / 2.00F;
					val1 = (val1 + this.supportEntity.myDynamicFriction) / 2.00F;
				}
				else if (this.frictionType == WheelFrictionTypes.maxFriction)
				{
					this.slideFrictionToUse = Math.Max(this.slideFrictionToUse, this.supportEntity.myDynamicFriction);
					val1 = Math.Max(val1, this.supportEntity.myDynamicFriction);
				}
				else
				{
					if (this.frictionType == WheelFrictionTypes.minFriction)
					{
						this.slideFrictionToUse = Math.Min(this.slideFrictionToUse, this.supportEntity.myDynamicFriction);
						val1 = Math.Min(val1, this.supportEntity.myDynamicFriction);
					}
				}
				this.maxSlideFrictionImpulse = (this.slideFrictionToUse * this.suspensionStiffness) * this.supportDepth;
				this.maxRollFrictionImpulse = (this.rollingFrictionToUse * this.suspensionStiffness) * this.supportDepth;
				float single4 = this.supportDepth;
				this.suspensionForce = this.suspensionStiffness * single4;
				this.normalizedSuspensionForce = (this.suspensionForce * 60.00F) * dt;
				bool flag2 = true;
				foreach (Controller controller1 in this.supportEntity.controllers)
				{
					if (controller1.contacts.Count <= 0)
					{
						continue;
					}
					if (this.vehicle.body is CompoundBody)
					{
						CompoundBody body1 = this.vehicle.body as CompoundBody;
						if (controller1.colliderA.myIsTangible && !body1.isEntityWithin(controller1.colliderA))
						{
							flag2 = false;
							break;
						}
						if (!controller1.colliderB.myIsTangible || body1.isEntityWithin(controller1.colliderB))
						{
							continue;
						}
						flag2 = false;
						break;
					}
					if (controller1.colliderA.myIsTangible && (controller1.colliderA != this.vehicle.body))
					{
						flag2 = false;
						break;
					}
					if (controller1.colliderB.myIsTangible && (controller1.colliderB != this.vehicle.body))
					{
						flag2 = false;
						break;
					}
				}
				this.Ra = this.supportLocation - this.vehicle.body.myInternalCenterOfMass;
				this.Rb = this.supportLocation - this.supportEntity.myInternalCenterOfMass;
				this.rollFrictionDenominator = ((1.00F / this.vehicle.body.mass) + (1.00F / this.supportEntityParent.mass)) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Ra, this.worldForward), this.vehicle.body.internalInertiaTensorInverse), this.Ra) + Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Rb, this.worldForward), this.supportEntityParent.internalInertiaTensorInverse), this.Rb), this.worldForward);
				if (flag2)
				{
					this.normalDenominator = ((1.00F / this.vehicle.body.mass) + (1.00F / this.supportEntityParent.mass)) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Ra, this.supportNormal), this.vehicle.body.internalInertiaTensorInverse), this.Ra) + Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Rb, this.supportNormal), this.supportEntityParent.internalInertiaTensorInverse), this.Rb), this.supportNormal);
					single5 = this.rollFrictionDenominator;
				}
				else
				{
					this.normalDenominator = (1.00F / this.vehicle.body.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Ra, this.supportNormal), this.vehicle.body.internalInertiaTensorInverse), this.Ra), this.supportNormal);
					single5 = (1.00F / this.vehicle.body.mass) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Ra, this.worldForward), this.vehicle.body.internalInertiaTensorInverse), this.Ra), this.worldForward);
				}
				this.slidingDirection = Vector3.Cross(this.supportNormal, this.worldForward);
				this.slideFrictionDenominator = ((1.00F / this.vehicle.body.mass) + (1.00F / this.supportEntityParent.mass)) + Vector3.Dot(Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Ra, this.slidingDirection), this.vehicle.body.internalInertiaTensorInverse), this.Ra) + Vector3.Cross(Vector3.Transform(Vector3.Cross(this.Rb, this.slidingDirection), this.supportEntityParent.internalInertiaTensorInverse), this.Rb), this.slidingDirection);
				if (!flag1)
				{
					vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
					single1 = -Vector3.Dot(vector1, this.worldForward);
					this.myAngularVelocity = (Vector3) (Vector3.Cross((Vector3) (this.worldRayDirection * this.radius), (Vector3) (-single1 * this.worldForward)) / (this.radius * this.radius));
					float single6 = this.myAngularVelocity.Length();
					Vector3 vector2 = Toolbox.zeroVector;
					if (single6 < (0.19F / this.radius))
					{
						single6 = 0.00F;
						this.myAngularVelocity = Toolbox.zeroVector;
					}
					else
					{
						if (single6 > 0.00F)
						{
							vector2 = (Vector3) (this.myAngularVelocity / single6);
						}
					}
					this.tireSpinDelta = (Math.Sign(Vector3.Dot(vector2, this.worldSide)) * single6) * dt;
				}
				else
				{
					this.myAngularVelocity = (Vector3) (this.myAngularVelocity * 0.90F);
					float single7 = this.myAngularVelocity.Length();
					Vector3 vector3 = Toolbox.zeroVector;
					if (single7 < (0.19F / this.radius))
					{
						single7 = 0.00F;
						this.myAngularVelocity = Toolbox.zeroVector;
					}
					else
					{
						if (single7 > 0.00F)
						{
							vector3 = (Vector3) (this.myAngularVelocity / single7);
						}
					}
					this.tireSpinDelta = (Math.Sign(Vector3.Dot(vector3, this.worldSide)) * single7) * dt;
				}
				vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
				float single8 = 1.00F - ((float) Math.Pow((double) MathHelper.Clamp(1.00F - this.suspensionDamping, 0.00F, 1.00F), (double) dt));
				float single9 = Math.Min((float) (((this.suspensionForce * dt) + (Vector3.Dot(this.supportNormal, vector1) * single8)) / this.normalDenominator), (float) (this.maximumSuspensionForce * dt));
				Vector3 vector4 = (Vector3) (single9 * this.supportNormal);
				Vector3 vector5 = Vector3.Cross(this.Ra, vector4);
				this.vehicle.body.applyLinearImpulse(ref vector4);
				this.vehicle.body.applyAngularImpulse(ref vector5);
				if (this.supportEntity.isPhysicallySimulated)
				{
					Vector3 vector6 = new Vector3(-vector4.X, -vector4.Y, -vector4.Z);
					Vector3 vector7 = Vector3.Cross(this.Rb, vector6);
					this.supportEntityParent.applyLinearImpulse(ref vector6);
					this.supportEntityParent.applyAngularImpulse(ref vector7);
				}
				if (!this.isBraking)
				{
					float single10;
					vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
					single1 = -Vector3.Dot(vector1, this.worldForward);
					if (single1 > 0.00F)
					{
						single10 = Math.Min((float) (single1 + (this.acceleration * dt)), this.vehicle.maximumForwardSpeed);
					}
					else
					{
						single10 = Math.Max((float) (single1 + (this.acceleration * dt)), -this.vehicle.maximumBackwardSpeed);
					}
					float single11 = single10 - single1;
					float val2 = -single11 / single5;
					val2 = Math.Max(Math.Min((float) (val1 * this.normalizedSuspensionForce), val2), (float) (-val1 * this.normalizedSuspensionForce));
					vector4 = (Vector3) (-val2 * this.worldForward);
					vector5 = Vector3.Cross(this.Ra, vector4);
					this.vehicle.body.applyLinearImpulse(ref vector4);
					this.vehicle.body.applyAngularImpulse(ref vector5);
					if (this.supportEntity.isPhysicallySimulated)
					{
						Vector3 vector8 = new Vector3(-vector4.X, -vector4.Y, -vector4.Z);
						Vector3 vector9 = Vector3.Cross(this.Rb, vector8);
						this.supportEntityParent.applyLinearImpulse(ref vector8);
						this.supportEntityParent.applyAngularImpulse(ref vector9);
					}
				}
				this.rollFrictionTotal = 0.00F;
				this.slideFrictionTotal = 0.00F;
				this.calculateRollingFrictionImpulse = true;
				this.calculateSlidingFrictionImpulse = true;
				this.numIterationsAtZeroRollingFrictionImpulse = 0.00F;
				this.numIterationsAtZeroSlidingFrictionImpulse = 0.00F;
			}
			else
			{
				if (flag1)
				{
					this.myAngularVelocity = (Vector3) (this.myAngularVelocity * 0.90F);
					float single13 = this.myAngularVelocity.Length();
					Vector3 vector10 = Toolbox.zeroVector;
					if (single13 < (0.19F / this.radius))
					{
						single13 = 0.00F;
						this.myAngularVelocity = Toolbox.zeroVector;
					}
					else
					{
						if (single13 > 0.00F)
						{
							vector10 = (Vector3) (this.myAngularVelocity / single13);
						}
					}
					this.tireSpinDelta = (Math.Sign(Vector3.Dot(vector10, this.worldSide)) * single13) * dt;
				}
				else
				{
					this.myAngularVelocity = (Vector3) (this.myAngularVelocity * 0.99F);
					float single14 = this.myAngularVelocity.Length();
					Vector3 vector11 = Toolbox.zeroVector;
					if (single14 < (0.19F / this.radius))
					{
						single14 = 0.00F;
						this.myAngularVelocity = Toolbox.zeroVector;
					}
					else
					{
						if (single14 > 0.00F)
						{
							vector11 = (Vector3) (this.myAngularVelocity / single14);
						}
					}
					this.tireSpinDelta = (Math.Sign(Vector3.Dot(vector11, this.worldSide)) * single14) * dt;
				}
				this.supportToi = 1.00F;
			}
			this.tireSpin += this.tireSpinDelta;
		}
		
		internal void updateVelocities (float dt)
		{
			Vector3 vector1 = Toolbox.zeroVector;
			if (this.supportEntities.Count > 0)
			{
				if (this.calculateSlidingFrictionImpulse)
				{
					vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
					float single1 = -Vector3.Dot(vector1, this.slidingDirection);
					float single2 = single1 / this.slideFrictionDenominator;
					float single3 = this.slideFrictionTotal;
					this.slideFrictionTotal = Math.Max(Math.Min((float) (this.slideFrictionToUse * this.normalizedSuspensionForce), (float) (this.slideFrictionTotal + single2)), (float) (-this.slideFrictionToUse * this.normalizedSuspensionForce));
					single2 = this.slideFrictionTotal - single3;
					Vector3 vector2 = (Vector3) (-single2 * this.slidingDirection);
					Vector3 vector3 = Vector3.Cross(this.Ra, vector2);
					this.vehicle.body.applyLinearImpulse(ref vector2);
					this.vehicle.body.applyAngularImpulse(ref vector3);
					if (this.supportEntity.isPhysicallySimulated)
					{
						Vector3 vector4 = new Vector3(-vector2.X, -vector2.Y, -vector2.Z);
						Vector3 vector5 = Vector3.Cross(this.Rb, vector4);
						this.supportEntityParent.applyLinearImpulse(ref vector4);
						this.supportEntityParent.applyAngularImpulse(ref vector5);
					}
					if (vector2.LengthSquared() < (this.vehicle.space.simulationSettings.minimumImpulse * this.vehicle.space.simulationSettings.minimumImpulse))
					{
						this.numIterationsAtZeroSlidingFrictionImpulse += 1.00F;
					}
					else
					{
						this.numIterationsAtZeroSlidingFrictionImpulse = 0.00F;
					}
					if (this.numIterationsAtZeroSlidingFrictionImpulse > this.vehicle.space.simulationSettings.iterationsBeforeEarlyOut)
					{
						this.calculateSlidingFrictionImpulse = false;
					}
				}
				if (this.calculateRollingFrictionImpulse)
				{
					vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
					float single4 = -Vector3.Dot(vector1, this.worldForward) / this.rollFrictionDenominator;
					float single5 = this.rollFrictionTotal;
					this.rollFrictionTotal = Math.Max(Math.Min((float) (this.rollingFrictionToUse * this.normalizedSuspensionForce), (float) (this.rollFrictionTotal + single4)), (float) (-this.rollingFrictionToUse * this.normalizedSuspensionForce));
					single4 = this.rollFrictionTotal - single5;
					Vector3 vector6 = (Vector3) (-single4 * this.worldForward);
					Vector3 vector7 = Vector3.Cross(this.Ra, vector6);
					this.vehicle.body.applyLinearImpulse(ref vector6);
					this.vehicle.body.applyAngularImpulse(ref vector7);
					if (this.supportEntity.isPhysicallySimulated)
					{
						Vector3 vector8 = new Vector3(-vector6.X, -vector6.Y, -vector6.Z);
						Vector3.Cross(this.Rb, vector8);
						this.supportEntityParent.applyLinearImpulse(ref vector8);
						this.supportEntityParent.applyAngularImpulse(ref vector8);
					}
					if (vector6.LengthSquared() < (this.vehicle.space.simulationSettings.minimumImpulse * this.vehicle.space.simulationSettings.minimumImpulse))
					{
						this.numIterationsAtZeroRollingFrictionImpulse += 1.00F;
					}
					else
					{
						this.numIterationsAtZeroRollingFrictionImpulse = 0.00F;
					}
					if (this.numIterationsAtZeroRollingFrictionImpulse > this.vehicle.space.simulationSettings.iterationsBeforeEarlyOut)
					{
						this.calculateRollingFrictionImpulse = false;
					}
				}
			}
		}
		
		internal void updateAtEndOfUpdate (float dt)
		{
			Vector3 v = Vector3.Transform(this.localDetectorPosition, this.vehicle.body.myInternalOrientationMatrix) + this.vehicle.body.myInternalCenterPosition;
			this.detectorShape.moveTo(v);
			this.detectorShape.applyQuaternion(Quaternion.CreateFromRotationMatrix(this.steeringRotation * this.vehicle.body.myInternalOrientationMatrix));
			this.detectorShape.internalLinearVelocity = Toolbox.getVelocityOfPoint(this.detectorShape.myInternalCenterPosition, this.vehicle.body);
			this.detectorShape.internalAngularVelocity = this.vehicle.body.myInternalAngularVelocity;
		}
		
		internal void updateAtEndOfFrame (float dt)
		{
			Matrix matrix1 = this.initialLocalRotation * this.steeringRotation;
			Matrix matrix2 = matrix1 * this.vehicle.body.myInternalOrientationMatrix;
			Vector3 vector1 = Vector3.Transform(this.localRayDirection, this.vehicle.body.myInternalOrientationMatrix);
			Vector3 vector2 = Vector3.Transform(this.forward, matrix2);
			Vector3 vector3 = Vector3.Cross(vector2, vector1);
			Matrix matrix3 = Matrix.CreateFromAxisAngle(vector3, this.tireSpin);
			Vector3 vector4 = Vector3.Transform(this.localChassisConnection, this.vehicle.body.myInternalOrientationMatrix) + this.vehicle.body.myInternalCenterPosition;
			if (this is ConvexCastWheel)
			{
				this.wheelPos = vector4 + ((Vector3) (vector1 * (this.supportToi * this.suspensionLength)));
			}
			else
			{
				this.wheelPos = vector4 + ((Vector3) (vector1 * ((this.supportToi * this.suspensionLength) - this.radius)));
			}
			this.transformMatrix = (matrix2 * matrix3) * Matrix.CreateTranslation(this.wheelPos);
		}
		
		public void brake (float brakingFriction)
		{
			this.brakeFriction = brakingFriction;
			this.isBraking = true;
		}
		
		public void releaseBrake ()
		{
			this.brakeFriction = 0.00F;
			this.isBraking = false;
		}
		
		protected abstract void getSupportRayHits (float dt, List<Vector3> locs, List<Vector3> normals, List<float> tois, List<float> depths, List<Entity> supports);
		
		// Properties
		public float wheelFacingAngle
		{
			get
			{
				return this.wheelFacing;
			}
			set
			{
				this.steeringRotation = Matrix.CreateFromAxisAngle(this.localRayDirection, value);
				this.totalLocalRotation = this.initialLocalRotation * this.steeringRotation;
				this.wheelFacing = value;
			}
		}
		
		public Vector3 wheelPosition
		{
			get
			{
				return this.wheelPos;
			}
		}
		
		public Matrix worldMatrix
		{
			get
			{
				return this.transformMatrix;
			}
		}
		
		public Vector3 angularVelocity
		{
			get
			{
				return this.myAngularVelocity;
			}
		}
		
		public Vector3 linearVelocity
		{
			get
			{
				return Toolbox.getVelocityOfPoint(this.chassisConnection, this.vehicle.body);
			}
		}
		
		public float supportPointRelativeVelocity
		{
			get
			{
				Vector3 vector1 = ((this.supportEntityParent.myInternalLinearVelocity + Vector3.Cross(this.supportEntityParent.myInternalAngularVelocity, this.Rb)) - this.vehicle.body.myInternalLinearVelocity) - Vector3.Cross(this.vehicle.body.myInternalAngularVelocity, this.Ra);
				return -Vector3.Dot(vector1, this.worldForward);
			}
		}
		
		public bool hasSupport
		{
			get
			{
				return this.supported;
			}
		}
		
		public Entity supportingEntity
		{
			get
			{
				return this.supportEntityParent;
			}
		}
		
		
		// Instance Fields
		public  Box detectorShape;
		public  Vector3 localRayDirection;
		public  WheelFrictionTypes frictionType;
		public  float rollingFriction;
		public  float slideFriction;
		public  float tireGrip;
		public  float suspensionStiffness;
		public  float suspensionLength;
		public  Vehicle vehicle;
		public  Vector3 chassisConnection;
		public  Vector3 localChassisConnection;
		public  Vector3 forward;
		public  Matrix totalLocalRotation;
		public  Matrix steeringRotation;
		public  Matrix initialLocalRotation;
		public  float maximumSuspensionForce;
		public  float suspensionDamping;
		public  float acceleration;
		private  Vector3 wheelPos;
		private  Matrix transformMatrix;
		public  float radius;
		private  Vector3 myAngularVelocity;
		public  float tireSpin;
		public  bool updateTireSpinInternally;
		private  float wheelFacing;
		public  bool stopTiresOnBrake;
		public  bool stopTireGraphicsOnBrake;
		private  List<Vector3> supportLocations;
		private  List<Vector3> supportNormals;
		private  List<float> supportTois;
		private  List<float> supportDepths;
		private  List<Entity> supportEntities;
		private  Vector3 supportLocation;
		private  Vector3 supportNormal;
		private  float supportToi;
		private  float supportDepth;
		private  Entity supportEntity;
		private  Entity supportEntityParent;
		private  float maxSlideFrictionImpulse;
		private  float maxRollFrictionImpulse;
		private  float suspensionForce;
		private  float normalizedSuspensionForce;
		private  float normalDenominator;
		private  float slideFrictionDenominator;
		private  float rollFrictionDenominator;
		private  Vector3 worldForward;
		private  Vector3 worldSide;
		protected  Vector3 worldRayDirection;
		private  Vector3 slidingDirection;
		private  Vector3 Ra;
		private  Vector3 Rb;
		private  bool isBraking;
		private  float rollingFrictionToUse;
		private  float brakeFriction;
		private  float slideFrictionToUse;
		protected  Vector3 localDetectorPosition;
		private  bool supported;
		private  float numIterationsAtZeroRollingFrictionImpulse;
		private  float numIterationsAtZeroSlidingFrictionImpulse;
		private  bool calculateRollingFrictionImpulse;
		private  bool calculateSlidingFrictionImpulse;
		private  float slideFrictionTotal;
		private  float rollFrictionTotal;
		private  float tireSpinDelta;
	}
}
