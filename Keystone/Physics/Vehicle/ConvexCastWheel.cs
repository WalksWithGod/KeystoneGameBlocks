using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BEPUphysics.Vehicle
{
	public class ConvexCastWheel : Wheel
	{
		// Constructors
		public ConvexCastWheel (Vector3 position, Quaternion orientation, Entity shape, float wheelRadius, Vector3 suspensionDirection, Vector3 forwardVector, float suspensionLen, float suspensionConstant, float slidingFriction, float rollFriction, float grip, float maxSuspensionForce, float suspensionDamp)
		{
			base.localRayDirection = Vector3.Normalize(suspensionDirection);
			base.localChassisConnection = position;
			base.localDetectorPosition = position + ((Vector3) (base.localRayDirection * (suspensionLen * 0.50F)));
			this.wheelShape = shape;
			shape.internalOrientationQuaternion = orientation;
			Vector3 vector1 = (Vector3) (base.localRayDirection * suspensionLen);
			shape.moveTo(base.localChassisConnection);
			shape.findBoundingBox(0.00F);
			float single1 = shape.boundingBox.Min.X;
			float single2 = shape.boundingBox.Max.X;
			float single3 = shape.boundingBox.Min.Y;
			float single4 = shape.boundingBox.Max.Y;
			float single5 = shape.boundingBox.Min.Z;
			float single6 = shape.boundingBox.Max.Z;
			if (vector1.X > 0.00F)
			{
				single2 += vector1.X;
			}
			else
			{
				single1 += vector1.X;
			}
			if (vector1.Y > 0.00F)
			{
				single4 += vector1.Y;
			}
			else
			{
				single3 += vector1.Y;
			}
			if (vector1.Z > 0.00F)
			{
				single6 += vector1.Z;
			}
			else
			{
				single5 += vector1.Z;
			}
			base.detectorShape = new Box(base.localDetectorPosition, (single2 - single1) + 0.01F, (single4 - single3) + 0.01F, (single6 - single5) + 0.01F, 1.00F);
			base.detectorShape.myIsTangible = false;
			base.detectorShape.isAffectedByGravity = false;
			base.detectorShape.isAlwaysActive = true;
			base.detectorShape.moveTo(position);
			base.detectorShape.internalOrientationQuaternion = orientation;
			base.suspensionStiffness = suspensionConstant;
			base.suspensionLength = suspensionLen;
			base.forward = Vector3.Normalize(forwardVector);
			base.totalLocalRotation = Matrix.CreateFromQuaternion(orientation);
			base.suspensionDamping = suspensionDamp;
			base.initialLocalRotation = base.totalLocalRotation;
			base.rollingFriction = rollFriction;
			base.slideFriction = slidingFriction;
			base.maximumSuspensionForce = maxSuspensionForce;
			base.tireGrip = grip;
			base.radius = wheelRadius;
		}
		
		
		// Methods
		protected override void getSupportRayHits (float dt, List<Vector3> locs, List<Vector3> normals, List<float> tois, List<float> depths, List<Entity> supports)
		{
			this.wheelShape.moveTo(base.chassisConnection);
			this.wheelShape.internalOrientationQuaternion = Quaternion.CreateFromRotationMatrix(base.totalLocalRotation * base.vehicle.body.myInternalOrientationMatrix);
			foreach (Controller controller1 in base.detectorShape.controllers)
			{
				Entity p;
				if (controller1.colliderA == base.detectorShape)
				{
					p = controller1.colliderB;
				}
				else
				{
					p = controller1.colliderA;
				}
				if (p.myIsTangible && !p.myIsDetector)
				{
					Vector3 item;
					Vector3 vector2;
					float single1;
					Toolbox.getHighestRankCompoundBody(p);
					Vector3 vector6 = base.chassisConnection;
					Vector3 vector3 = (Vector3) (base.worldRayDirection * base.suspensionLength);
					Vector3 vector4 = vector3 + ((Vector3) (Toolbox.getVelocityOfPoint(base.wheelPosition, base.vehicle.body) * dt));
					Vector3 vector5 = (Vector3) (p.myInternalLinearVelocity * dt);
					if (Toolbox.areSweptObjectsColliding(this.wheelShape, p, this.wheelShape.myCollisionMargin, p.myCollisionMargin, ref vector4, ref vector5, out item, out vector2, out single1))
					{
						if (vector2.LengthSquared() > 0.00F)
						{
							vector2.Normalize();
						}
						else
						{
							vector2 = Vector3.op_UnaryNegation(base.worldRayDirection);
						}
						if (Vector3.Dot(vector2, base.worldRayDirection) > 0.00F)
						{
							vector2 = (Vector3) (vector2 * -1.00F);
						}
						supports.Add(p);
						depths.Add(base.suspensionLength * (1.00F - single1));
						locs.Add(item);
						normals.Add(vector2);
						tois.Add(single1);
					}
				}
			}
		}
		
		
		// Instance Fields
		public  Entity wheelShape;
	}
}
