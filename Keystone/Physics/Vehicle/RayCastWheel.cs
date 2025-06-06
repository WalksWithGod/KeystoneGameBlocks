using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BEPUphysics.Vehicle
{
	public class RayCastWheel : Wheel
	{
		// Constructors
		public RayCastWheel (Vector3 position, Quaternion orientation, float wheelRadius, Vector3 suspensionDirection, Vector3 forwardVector, float suspensionLen, float suspensionConstant, float slidingFriction, float rollFriction, float grip, float maxSuspensionForce, float suspensionDamp)
		{
			base.localRayDirection = Vector3.Normalize(suspensionDirection);
			base.localChassisConnection = position;
			base.localDetectorPosition = position + ((Vector3) (base.localRayDirection * (suspensionLen * 0.50F)));
			Vector3 vector1 = base.localChassisConnection;
			Vector3 vector2 = base.localChassisConnection + ((Vector3) (base.localRayDirection * suspensionLen));
			Vector3 vector3 = Vector3.Min(vector1, vector2);
			Vector3 vector4 = Vector3.Max(vector1, vector2);
			base.detectorShape = new Box(base.localDetectorPosition, (vector4.X - vector3.X) + 0.01F, (vector4.Y - vector3.Y) + 0.01F, (vector4.Z - vector3.Z) + 0.01F, 1.00F);
			base.detectorShape.myIsTangible = false;
			base.detectorShape.isAffectedByGravity = false;
			base.detectorShape.isAlwaysActive = true;
			base.detectorShape.moveTo(position);
			base.detectorShape.orientationQuaternion = orientation;
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
					Vector3 origin = base.chassisConnection;
					Vector3 direction = (Vector3) (base.worldRayDirection * base.suspensionLength);
					if (p.rayTest(origin, direction, 1.00F, true, out item, out vector2, out single1))
					{
						if (vector2.LengthSquared() > 0.00F)
						{
							vector2.Normalize();
						}
						else
						{
							vector2 = Vector3.op_UnaryNegation(base.worldRayDirection);
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
		
	}
}
