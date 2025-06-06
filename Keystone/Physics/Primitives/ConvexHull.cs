using Physics;
using Core.Types;
using System;
using System.Collections.Generic;

namespace Physics.Primitives
{
	public class ConvexHull : CollisionPrimitive 
	{

        // Instance Fields
        public List<Vector3d> bodyPoints;

		// Constructors
		public ConvexHull (List<Vector3d> points, float m)
		{
			this.bodyPoints = new List<Vector3d>(points);
			base.mass = m;
			this.initialize(true);
			base.myInternalOrientationQuaternion = Quaternion.get_Identity();
			base.myInternalLinearMomentum = Toolbox.zeroVector;
			base.myInternalAngularMomentum = Toolbox.zeroVector;
			base.internalInertiaTensorInverse = Matrix.get_Identity();
			base.myInternalOrientationMatrix = Matrix.get_Identity();
			base.myInternalLinearVelocity = Toolbox.zeroVector;
			base.myInternalAngularVelocity = Toolbox.zeroVector;
			base.force = Toolbox.zeroVector;
			base.torque = Toolbox.zeroVector;
			this.updateBufferedStates();
		}
		
		public ConvexHull (Vector3d position, List<Vector3d> points, float m)
		{
			this.bodyPoints = new List<Vector3d>(points);
			base.mass = m;
			this.initialize(true);
			base.moveTo(position);
			base.myInternalOrientationQuaternion = Quaternion.get_Identity();
			base.myInternalLinearMomentum = Toolbox.zeroVector;
			base.myInternalAngularMomentum = Toolbox.zeroVector;
			base.internalInertiaTensorInverse = Matrix.get_Identity();
			base.myInternalOrientationMatrix = Matrix.get_Identity();
			base.myInternalLinearVelocity = Toolbox.zeroVector;
			base.myInternalAngularVelocity = Toolbox.zeroVector;
			base.force = Toolbox.zeroVector;
			base.torque = Toolbox.zeroVector;
			this.updateBufferedStates();
		}
		
		public ConvexHull (List<Vector3d> points)
		{
			this.bodyPoints = new List<Vector3d>(points);
			this.initialize(false);
			this.updateBufferedStates();
		}
		
		public ConvexHull (Vector3d position, List<Vector3d> points)
		{
			this.bodyPoints = new List<Vector3>(points);
			this.initialize(false);
			base.moveTo(position);
			this.updateBufferedStates();
		}
		
		
		// Methods
		public override void makeNonDynamic ()
		{
			Vector3d v = base.myInternalCenterPosition;
			Quaternion quaternion1 = base.myInternalOrientationQuaternion;
			base.makeNonDynamic();
			base.moveTo(v);
			base.internalOrientationQuaternion = quaternion1;
		}
		
		public override void makePhysical (float m)
		{
			Vector3d v = base.myInternalCenterPosition;
			Quaternion quaternion1 = base.myInternalOrientationQuaternion;
			base.makePhysical(m);
			base.moveTo(v);
			base.internalOrientationQuaternion = quaternion1;
		}
		
		internal override void initialize (bool physicallySimulated)
		{
			base.findBoundingBox(0.00F);
			List<int> indices = ResourcePool.getIntList();
			List<Vector3d> hullVertices = ResourcePool.getVectorList();
			Toolbox.getConvexHull(this.bodyPoints, indices, hullVertices);
			List<Vector3d> list3 = ResourcePool.getVectorList();
			for (int i = 0;i < indices.Count; i++)
			{
				list3.Add(this.bodyPoints[indices[i]]);
			}
			Vector3d vector1 = Toolbox.zeroVector;
			this.bodyPoints.Clear();
			for (int j = 0;j < hullVertices.Count; j++)
			{
				this.bodyPoints.Add(hullVertices[j]);
				vector1 += hullVertices[j];
			}
			vector1 = (Vector3d) (vector1 / ((float) hullVertices.Count));
			base.volume = 0.00F;
			List<float> list4 = ResourcePool.getFloatList();
			List<Vector3d> list = ResourcePool.getVectorList();
			for (int z = 0;z < list3.Count; z += 3)
			{
				list4.Add(Vector3d.Dot(Vector3.Cross(list3[(z + 1)] - list3[z], list3[(z + 2)] - list3[z]), vector1 - list3[z]));
				base.volume += list4[z / 3];
				list.Add((T) ((((list3[z] + list3[(z + 1)]) + list3[(z + 2)]) + vector1) / 4.00F));
			}
			base.myInternalCenterPosition = Toolbox.zeroVector;
			for (int k = 0;k < list.Count; k++)
			{
				base.myInternalCenterPosition += (Vector3) (list[k] * (list4[k] / base.volume));
			}
			base.volume /= 6.00F;
			base.density = base.mass / base.volume;
			for (int l = 0;l < list3.Count; l++)
			{
				List<Vector3d> list6;
				int num8;
				list6 = list3[num8 = l] = list6[num8] - base.myInternalCenterPosition;
			}
			for (int a = 0;a < this.bodyPoints.Count; a++)
			{
				List<Vector3d> list7;
				int num9;
				list7 = this.bodyPoints[num9 = a] = list7[num9] - base.myInternalCenterPosition;
			}
			if (physicallySimulated)
			{
				base.localInertiaTensor = new Matrix(0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 0.00F, 1.00F);
				float single2 = 0.00F;
				float single3 = 0.00F;
				float single4 = 0.00F;
				float single5 = 0.00F;
				float single6 = 0.00F;
				float single7 = 0.00F;
				float single8 = base.density / 60.00F;
				float single9 = -base.density / 120.00F;
				for (int b = 0;b < list3.Count; b += 3)
				{
					Vector3d vector2 = list3[b];
					Vector3d vector3 = list3[b + 1];
					Vector3d vector4 = list3[b + 2];
					float single11 = ((vector2 + vector3) + vector4) / 4.00F;
					float single1 = Math.Abs((float) (((vector2.X * ((vector3.Y * vector4.Z) - (vector3.Z * vector4.Y))) - (vector3.X * ((vector2.Y * vector4.Z) - (vector2.Z * vector4.Y)))) + (vector4.X * ((vector2.Y * vector3.Z) - (vector2.Z * vector3.Y)))));
					single2 += single1 * ((((((((((((vector2.Y * vector2.Y) + (vector2.Y * vector3.Y)) + (vector3.Y * vector3.Y)) + (vector2.Y * vector4.Y)) + (vector3.Y * vector4.Y)) + (vector4.Y * vector4.Y)) + (vector2.Z * vector2.Z)) + (vector2.Z * vector3.Z)) + (vector3.Z * vector3.Z)) + (vector2.Z * vector4.Z)) + (vector3.Z * vector4.Z)) + (vector4.Z * vector4.Z));
					single3 += single1 * ((((((((((((vector2.X * vector2.X) + (vector2.X * vector3.X)) + (vector3.X * vector3.X)) + (vector2.X * vector4.X)) + (vector3.X * vector4.X)) + (vector4.X * vector4.X)) + (vector2.Z * vector2.Z)) + (vector2.Z * vector3.Z)) + (vector3.Z * vector3.Z)) + (vector2.Z * vector4.Z)) + (vector3.Z * vector4.Z)) + (vector4.Z * vector4.Z));
					single4 += single1 * ((((((((((((vector2.X * vector2.X) + (vector2.X * vector3.X)) + (vector3.X * vector3.X)) + (vector2.X * vector4.X)) + (vector3.X * vector4.X)) + (vector4.X * vector4.X)) + (vector2.Y * vector2.Y)) + (vector2.Y * vector3.Y)) + (vector3.Y * vector3.Y)) + (vector2.Y * vector4.Y)) + (vector3.Y * vector4.Y)) + (vector4.Y * vector4.Y));
					single5 += single1 * ((((((((((2.00F * vector2.Y) * vector2.Z) + (vector3.Y * vector2.Z)) + (vector4.Y * vector2.Z)) + (vector2.Y * vector3.Z)) + ((2.00F * vector3.Y) * vector3.Z)) + (vector4.Y * vector3.Z)) + (vector2.Y * vector4.Z)) + (vector3.Y * vector4.Z)) + ((2.00F * vector4.Y) * vector4.Z));
					single6 += single1 * ((((((((((2.00F * vector2.X) * vector2.Z) + (vector3.X * vector2.Z)) + (vector4.X * vector2.Z)) + (vector2.X * vector3.Z)) + ((2.00F * vector3.X) * vector3.Z)) + (vector4.X * vector3.Z)) + (vector2.X * vector4.Z)) + (vector3.X * vector4.Z)) + ((2.00F * vector4.X) * vector4.Z));
					single7 += single1 * ((((((((((2.00F * vector2.X) * vector2.Y) + (vector3.X * vector2.Y)) + (vector4.X * vector2.Y)) + (vector2.X * vector3.Y)) + ((2.00F * vector3.X) * vector3.Y)) + (vector4.X * vector3.Y)) + (vector2.X * vector4.Y)) + (vector3.X * vector4.Y)) + ((2.00F * vector4.X) * vector4.Y));
				}
				single2 *= single8;
				single3 *= single8;
				single4 *= single8;
				single5 *= single9;
				single6 *= single9;
				single7 *= single9;
				base.localInertiaTensor = new Matrix(single2, single6, single7, 0.00F, single6, single3, single5, 0.00F, single7, single5, single4, 0.00F, 0.00F, 0.00F, 0.00F, 1.00F);
				base.localInertiaTensorInverse = Matrix.Invert(base.localInertiaTensor);
				if (SimulationSettings.padInertiaTensors)
				{
					base.padInertiaTensor();
				}
				base.scaleInertiaTensor(SimulationSettings.inertiaTensorScale);
				base.initializePhysicalData();
			}
			else
			{
				base.density = float.PositiveInfinity;
				base.initializeNonDynamicData();
			}
			float single10 = 0.00F;
			foreach (Vector3d vector5 in this.bodyPoints)
			{
				single10 = vector5.Length();
				if (base.myMaximumRadius < single10)
				{
					base.myMaximumRadius = single10;
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(list4);
			ResourcePool.giveBack(indices);
			ResourcePool.giveBack(hullVertices);
			ResourcePool.giveBack(list3);
		}
		
		public sealed override void getExtremePoint (ref Vector3d d, ref Vector3d positionToUse, ref Quaternion orientationToUse, double margin, out Vector3d extremePoint)
		{
			Vector3d vector1;
			Quaternion quaternion1;
			Vector3d vector3;
			Quaternion.Conjugate(ref orientationToUse, ref quaternion1);
			Vector3d.Transform(ref d, ref quaternion1, ref vector1);
			float single1 = float.MinValue;
			extremePoint = Toolbox.noVector;
			for (int i = 0;i < this.bodyPoints.Count; i++)
			{
				float single2;
				Vector3 vector2 = this.bodyPoints[i];
				Vector3.Dot(ref vector2, ref vector1, ref single2);
				if (single2 > single1)
				{
					extremePoint = vector2;
					single1 = single2;
				}
			}
			Vector3d.Transform(ref extremePoint, ref orientationToUse, ref extremePoint);
			Vector3d.Add(ref extremePoint, ref positionToUse, ref extremePoint);
			Vector3d.Normalize(ref d, ref vector3);
			Vector3d.Multiply(ref vector3, margin, ref vector3);
			Vector3d.Add(ref extremePoint, ref vector3, ref extremePoint);
		}
		
		public sealed override void getExtremePoints (ref Vector3d d, out Vector3d min, out Vector3d max, double margin)
		{
			Vector3d vector1;
			Matrix matrix1;
			Vector3d vector3;
			Matrix.Transpose(ref this.myInternalOrientationMatrix, ref matrix1);
			Vector3d.Transform(ref d, ref matrix1, ref vector1);
			float single1 = float.MinValue;
			float single2 = float.MaxValue;
			min = Toolbox.zeroVector;
			max = Toolbox.zeroVector;
			for (int i = 0;i < this.bodyPoints.Count; i++)
			{
				float single3;
				Vector3d vector2 = this.bodyPoints[i];
				Vector3d.Dot(ref vector2, ref vector1, ref single3);
				if (single3 < single2)
				{
					min = vector2;
					single2 = single3;
				}
				if (single3 > single1)
				{
					max = vector2;
					single1 = single3;
				}
			}
			Vector3d.Transform(ref max, ref this.myInternalOrientationMatrix, ref max);
			Vector3d.Add(ref max, ref this.myInternalCenterPosition, ref max);
			Vector3d.Transform(ref min, ref this.myInternalOrientationMatrix, ref min);
			Vector3d.Add(ref min, ref this.myInternalCenterPosition, ref min);
			Vector3d.Normalize(ref d, ref vector3);
			Vector3d.Multiply(ref vector3, margin, ref vector3);
			Vector3d.Subtract(ref min, ref vector3, ref min);
			Vector3d.Add(ref max, ref vector3, ref max);
		}
	}
}
