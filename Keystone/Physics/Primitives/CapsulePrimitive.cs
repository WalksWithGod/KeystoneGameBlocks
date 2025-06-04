using System;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class CapsulePrimitive : CollisionPrimitive
    {
        public double Length;
        public double Radius;

        public CapsulePrimitive(PhysicsBody body)
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
            vector1.Normalize();
            Vector3d vector2 = new Vector3d(0.00F, Length / 2.00F, 0.00F);
            double single1 = Vector3d.DotProduct(vector1, vector2);
            if (single1 > 0.00F)
            {
                extremePoint = vector2;
            }
            else if (single1 < 0.00F)
            {
                extremePoint = -vector2;
            }
            else
            {
                extremePoint = Toolbox.zeroVector;
            }
            vector1 =  vector1 *  (Radius + margin);
            extremePoint = vector1 + extremePoint;
            extremePoint = Vector3d.TransformCoord(extremePoint, orientationToUse);
            extremePoint +=positionToUse;
        }
    }
}
