using System;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class CylinderPrimitive : CollisionPrimitive
    {
        public double Height;
        public double Radius;

        public CylinderPrimitive(PhysicsBody body)
            : base(body)
        {
        }
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }
        public override void getExtremePoint(ref Keystone.Types.Vector3d d, ref Keystone.Types.Vector3d positionToUse, ref Keystone.Types.Quaternion orientationToUse, double margin, out Keystone.Types.Vector3d extremePoint)
        {
            Quaternion quaternion1 = Quaternion.Conjugate(orientationToUse);
            Vector3d vector1 = Vector3d.TransformCoord(d, quaternion1);
            double num1 = Math.Sqrt((vector1.x * vector1.x) + (vector1.z * vector1.z));
            double single1 = Radius / num1;
            double single2 = Height / 2.00d;
            if (num1 > 1.0000000116860974E-07)
            {
                extremePoint = new Vector3d(single1 * vector1.x, Math.Sign(vector1.y) * single2, single1 * vector1.z);
            }
            else
            {
                extremePoint = new Vector3d(0.00F, Math.Sign(vector1.y) * single2, 0.00d);
            }
            extremePoint= Vector3d.TransformCoord(extremePoint, orientationToUse);
            extremePoint +=  positionToUse;
            Vector3d vector2 = Vector3d.Normalize(d);
            vector2 *= margin;
            extremePoint += vector2;
        }
    }
}
