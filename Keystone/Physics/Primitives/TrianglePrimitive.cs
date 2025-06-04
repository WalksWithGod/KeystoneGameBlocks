using System;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class TrianglePrimitive : CollisionPrimitive
    {
        public Triangle Triangle;
        public Vector3d[] vertices;
        public bool tryToUseFaceNormal;
        public double useFaceNormalWithinAngle;

        public TrianglePrimitive(PhysicsBody body)
            : base(body)
        {
        }

        public Vector3d Normal;

        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            if (Toolbox.findRayTriangleIntersection(origin, direction, maximumLength, vertices[0], vertices[1], vertices[2], out hitLocation, out t))
            {
                if (Vector3d.DotProduct(Normal, direction) < 0.00F)
                {
                    hitNormal = Normal;
                }
                else
                {
                    hitNormal = -Normal;
                }
                return true;
            }
            hitLocation = Toolbox.noVector;
            hitNormal = Toolbox.noVector;
            t = float.NegativeInfinity;
            return false;
        }

        public override void getExtremePoint(ref Keystone.Types.Vector3d d, ref Keystone.Types.Vector3d positionToUse, ref Keystone.Types.Quaternion orientationToUse, double margin, out Keystone.Types.Vector3d extremePoint)
        {

            // TODO: uncomment this after i figure out what i want to do with "localVertices"
            // because right now im not sure how\where we want to deal with local space coords and world/region space
            throw new NotImplementedException();
            //double single2 = double.NegativeInfinity;
            //Quaternion quaternion1 = Quaternion.Conjugate(orientationToUse);
            //Vector3d vector1 = Vector3d.TransformCoord(d, quaternion1);
            //double single1 = Vector3d.DotProduct(localVertices,  vector1);
            //extremePoint = localVertices[0];
            //single2 = single1;
            //single1 = Vector3d.DotProduct(localVertices[1], vector1);
            //if (single1 > single2)
            //{
            //    extremePoint = localVertices[1];
            //    single2 = single1;
            //}
            //single1 = Vector3d.DotProduct(localVertices[2], vector1);
            //if (single1 > single2)
            //{
            //    extremePoint = localVertices[2];
            //}
            //extremePoint = Vector3d.TransformCoord(extremePoint, orientationToUse);
            //extremePoint += positionToUse;
            //Vector3d vector2 = Vector3d.Normalize(d);
            //vector2 *= margin;
            //extremePoint += vector2;
        }

        public override void getExtremePoints(ref Vector3d d, out Vector3d min, out Vector3d max, double margin)
        {
            double single2 = double.NegativeInfinity;
            double single3 = double.PositiveInfinity;
            max = Toolbox.zeroVector;
            min = Toolbox.zeroVector;
            double single1 = Vector3d.DotProduct(vertices[0], d);
            if (single1 > single2)
            {
                max = vertices[0];
                single2 = single1;
            }
            if (single1 < single3)
            {
                min = vertices[0];
                single3 = single1;
            }
            single1 =Vector3d.DotProduct(vertices[1], d);
            if (single1 > single2)
            {
                max = vertices[1];
                single2 = single1;
            }
            if (single1 < single3)
            {
                min = vertices[1];
                single3 = single1;
            }
            single1 = Vector3d.DotProduct(vertices[2], d);
            if (single1 > single2)
            {
                max = vertices[2];
                single2 = single1;
            }
            if (single1 < single3)
            {
                min = vertices[2];
                single3 = single1;
            }
            Vector3d vector1 = Vector3d.Normalize( d);
            vector1 *= margin;
            min -= vector1;
            max += vector1;
        }
    }
}
