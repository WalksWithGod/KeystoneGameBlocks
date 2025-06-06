using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;

namespace BEPUphysics.ForceFields
{
	public class Tornado : ForceField
	{
		// Constructors
		public Tornado (Entity shape, Vector3 axis, float tornadoHeight, bool clockwise, float horizontalWind, float upwardWind, float inwardWind, float horizontalForce, float upwardForce, float inwardForce, float radiusTop, float radiusBottom)
		{
			this.spinClockwise = true;
			base.fieldShape = shape;
			this.tornadoAxis = Vector3.Normalize(axis);
			this.height = tornadoHeight;
			this.spinClockwise = clockwise;
			this.horizontalWindSpeed = horizontalWind;
			this.upwardSuctionSpeed = upwardWind;
			this.inwardSuctionSpeed = inwardWind;
			this.baseHorizontalForce = horizontalForce;
			this.baseUpwardForce = upwardForce;
			this.baseInwardForce = inwardForce;
			this.bottomRadius = radiusBottom;
			this.topRadius = radiusTop;
		}
		
		
		// Methods
		protected override void calculateImpulseAtPoint (Vector3 pos, Entity e, float dt, float timeScale, float timeSinceLastFrame, out Vector3 impulse)
		{
			float single1 = Vector3.Dot(this.tornadoAxis, (pos - base.fieldShape.myInternalCenterPosition) + ((Vector3) (this.tornadoAxis * (this.height / 2.00F))));
			if ((single1 < 0.00F) || (single1 > this.height))
			{
				impulse = Toolbox.zeroVector;
			}
			else
			{
				Vector3 vector1;
				Vector3 vector4;
				float single4;
				Vector3 vector5;
				float single5;
				float single2 = (this.bottomRadius * (1.00F - (single1 / this.height))) + (this.topRadius * (single1 / this.height));
				Vector3 vector2 = base.fieldShape.myInternalCenterPosition + ((Vector3) ((this.tornadoAxis * this.height) / 2.00F));
				Vector3 vector3 = base.fieldShape.myInternalCenterPosition - ((Vector3) ((this.tornadoAxis * this.height) / 2.00F));
				Toolbox.getClosestPointOnSegmentToPoint(ref vector2, ref vector3, ref pos, out vector1);
				Vector3 vector9 = pos - vector1;
				float single3 = vector9.Length();
				if (single3 > single2)
				{
					single4 = Math.Max((float) (single2 / single3), (float) 1.00F);
					vector4 = (Vector3) ((vector1 - pos) / single3);
				}
				else
				{
					if (single3 > 0.00F)
					{
						single4 = 0.50F + ((single3 * 0.50F) / single2);
						vector4 = (Vector3) ((vector1 - pos) / single3);
					}
					else
					{
						single4 = 0.50F;
						vector4 = Toolbox.zeroVector;
					}
				}
				Vector3 vector6 = Vector3.Cross(this.tornadoAxis, vector4);
				if (this.spinClockwise)
				{
					single5 = (Vector3.Dot(e.myInternalLinearVelocity, vector6) * e.mass) + (this.baseHorizontalForce * dt);
					single5 = Math.Min(this.baseHorizontalForce, Math.Max((float) ((this.horizontalWindSpeed * e.mass) - single5), (float) 0.00F));
					vector5 = (Vector3) (single5 * vector6);
				}
				else
				{
					single5 = (Vector3.Dot(e.myInternalLinearVelocity, Vector3.op_UnaryNegation(vector6)) * e.mass) + (this.baseHorizontalForce * dt);
					single5 = Math.Min(this.baseHorizontalForce, Math.Max((float) ((this.horizontalWindSpeed * e.mass) - single5), (float) 0.00F));
					vector5 = (Vector3) (single5 * Vector3.op_UnaryNegation(vector6));
				}
				single5 = (Vector3.Dot(e.myInternalLinearVelocity, this.tornadoAxis) * e.mass) + (this.baseUpwardForce * dt);
				single5 = Math.Min(this.baseUpwardForce, Math.Max((float) ((this.upwardSuctionSpeed * e.mass) - single5), (float) 0.00F));
				Vector3 vector7 = (Vector3) (single5 * this.tornadoAxis);
				single5 = (Vector3.Dot(e.myInternalLinearVelocity, vector4) * e.mass) + (this.baseInwardForce * dt);
				single5 = Math.Min(this.baseInwardForce, Math.Max((float) ((this.inwardSuctionSpeed * e.mass) - single5), (float) 0.00F));
				Vector3 vector8 = (Vector3) (single5 * vector4);
				impulse = (Vector3) ((dt * single4) * ((vector5 + vector7) + vector8));
			}
		}
		
		
		// Instance Fields
		public  float horizontalWindSpeed;
		public  float upwardSuctionSpeed;
		public  float inwardSuctionSpeed;
		public  float baseHorizontalForce;
		public  float baseUpwardForce;
		public  float baseInwardForce;
		public  float bottomRadius;
		public  float topRadius;
		public  float height;
		public  Vector3 tornadoAxis;
		public  bool spinClockwise;
	}
}
