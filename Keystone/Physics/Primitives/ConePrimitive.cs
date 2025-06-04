using System;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class ConePrimitive : CollisionPrimitive
    {
        public double Height;
        public double Radius;
        public ConePrimitive(PhysicsBody body)
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
            double single1 = d.Length;
            double num1 = Math.Pow(((vector1.x * vector1.x) + (vector1.z * vector1.z)), 0.5);
            double single2 = Radius / num1;
            double single3 = Height / 2.00d;
            double single4 = Radius / (Math.Pow(((Radius * Radius) + ((4.00d * single3) * single3)), 0.5));
            if (vector1.y > (single1 * single4))
            {
                extremePoint = new Vector3d(0.00F, single3, 0.00F);
            }
            else if (num1 > 1.0000000116860974E-07)
            {
                extremePoint = new Vector3d(single2 * vector1.x, -single3, single2 * vector1.z);
            }
            else
            {
                extremePoint = new Vector3d(0.00F, -single3, 0.00d);
            }
            Vector3d vector2 = new Vector3d(0.00F, 0.25F * Height, 0.00d);
            extremePoint = extremePoint + vector2;
            extremePoint = Vector3d.TransformCoord(extremePoint, orientationToUse);
            vector2 = d *   (margin / single1);
            extremePoint += vector2;
            extremePoint +=positionToUse;
        }
    }
}
