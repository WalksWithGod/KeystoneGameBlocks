// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using Keystone.Types;

namespace Steering.Helpers
{
    public static class Vector3Helpers
    {
        /// <summary>
        /// return component of vector parallel to a unit basis vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="unitBasis">A unit length basis vector</param>
        /// <returns></returns>
        public static Vector3d ParallelComponent(Vector3d vector, Vector3d unitBasis)
        {
            double projection = Vector3d.DotProduct(vector, unitBasis);
            return unitBasis * projection;
        }

        /// <summary>
        /// return component of vector perpendicular to a unit basis vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="unitBasis">A unit length basis vector</param>
        /// <returns></returns>
        public static Vector3d PerpendicularComponent(Vector3d vector, Vector3d unitBasis)
        {
            return (vector - ParallelComponent(vector, unitBasis));
        }

        /// <summary>
        /// clamps the length of a given vector to maxLength.  If the vector is
        /// shorter its value is returned unaltered, if the vector is longer
        /// the value returned has length of maxLength and is parallel to the
        /// original input.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static Vector3d TruncateLength(this Vector3d vector, double  maxLength)
        {
            double  maxLengthSquared = maxLength * maxLength;
            double vecLengthSquared = vector.LengthSquared();
            if (vecLengthSquared <= maxLengthSquared)
                return vector;

            return (vector * (maxLength / (double )Math.Sqrt(vecLengthSquared)));
        }

        /// <summary>
        /// rotate this vector about the global Y (up) axis by the given angle
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static Vector3d RotateAboutGlobalY(this Vector3d vector, double  radians)
        {
            double  s = 0;
            double  c = 0;
            return RotateAboutGlobalY(vector, radians, ref s, ref c);
        }

        /// <summary>
        /// Rotate this vector about the global Y (up) axis by the given angle
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="radians"></param>
        /// <param name="sin">Either Sin(radians) or default(double ), if default(double ) this value will be initialized with Sin(radians)</param>
        /// <param name="cos">Either Cos(radians) or default(double ), if default(double ) this value will be initialized with Cos(radians)</param>
        /// <returns></returns>
        public static Vector3d RotateAboutGlobalY(this Vector3d vector, double  radians, ref double  sin, ref double  cos)
        {
            // if both are default, they have not been initialized yet
// ReSharper disable CompareOfFloatsByEqualityOperator
            if (sin == default(double ) && cos == default(double ))
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                sin = (double )Math.Sin(radians);
                cos = (double )Math.Cos(radians);
            }
            return new Vector3d((vector.x * cos) + (vector.z * sin), vector.y, (vector.z * cos) - (vector.x * sin));
        }

        /// <summary>
        /// Wrap a position around so it is always within 1 radius of the sphere (keeps repeating wrapping until position is within sphere)
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector3d SphericalWrapAround(this Vector3d vector, Vector3d center, double radius)
        {
            double r;
            do
            {
                Vector3d offset = vector - center;
                r = offset.Length;

                if (r > radius)
                    vector = vector + ((offset / r) * radius * -2);

            } while (r > radius);

            return vector;
        }

        /// <summary>
        /// Returns a position randomly distributed on a disk of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will range between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static Vector3d RandomVectorOnUnitRadiusXZDisk()
        {
            Vector3d v;
            do
            {
                v.x = (RandomHelpers.Random() * 2) - 1;
                v.y = 0;
                v.z = (RandomHelpers.Random() * 2) - 1;
            }
            while (v.Length >= 1);

            return v;
        }

        /// <summary>
        /// Returns a position randomly distributed inside a sphere of unit radius
        /// centered at the origin.  Orientation will be random and length will range
        /// between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static Vector3d RandomVectorInUnitRadiusSphere()
        {
            Vector3d v = new Vector3d();
            do
            {
                v.x = (RandomHelpers.Random() * 2) - 1;
                v.y = (RandomHelpers.Random() * 2) - 1;
                v.z = (RandomHelpers.Random() * 2) - 1;
            }
            while (v.Length >= 1);

            return v;
        }

        /// <summary>
        /// Returns a position randomly distributed on the surface of a sphere
        /// of unit radius centered at the origin.  Orientation will be random
        /// and length will be 1
        /// </summary>
        /// <returns></returns>
        public static Vector3d RandomUnitVector()
        {
            return Vector3d.Normalize(RandomVectorInUnitRadiusSphere());
        }

        /// <summary>
        /// Returns a position randomly distributed on a circle of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will be 1
        /// </summary>
        /// <returns></returns>
        public static Vector3d RandomUnitVectorOnXZPlane()
        {
            Vector3d temp = RandomVectorInUnitRadiusSphere();
            temp.y = 0;
            temp = Vector3d.Normalize(temp);

            return temp;
        }

        /// <summary>
        /// Clip a vector to be within the given cone
        /// </summary>
        /// <param name="source">A vector to clip</param>
        /// <param name="cosineOfConeAngle">The cosine of the cone angle</param>
        /// <param name="basis">The vector along the middle of the cone</param>
        /// <returns></returns>
        public static Vector3d LimitMaxDeviationAngle(this Vector3d source, double cosineOfConeAngle, Vector3d basis)
        {
            return LimitDeviationAngleUtility(true, // force source INSIDE cone
                source, cosineOfConeAngle, basis);
        }

        /// <summary>
        /// Clip a vector to be outside the given cone
        /// </summary>
        /// <param name="source">A vector to clip</param>
        /// <param name="cosineOfConeAngle">The cosine of the cone angle</param>
        /// <param name="basis">The vector along the middle of the cone</param>
        /// <returns></returns>
        public static Vector3d LimitMinDeviationAngle(this Vector3d source, double cosineOfConeAngle, Vector3d basis)
        {
            return LimitDeviationAngleUtility(false, // force source OUTSIDE cone
                source, cosineOfConeAngle, basis);
        }

        /// <summary>
        /// used by limitMaxDeviationAngle / limitMinDeviationAngle
        /// </summary>
        /// <param name="insideOrOutside"></param>
        /// <param name="source"></param>
        /// <param name="cosineOfConeAngle"></param>
        /// <param name="basis"></param>
        /// <returns></returns>
        private static Vector3d LimitDeviationAngleUtility(bool insideOrOutside, Vector3d source, double cosineOfConeAngle, Vector3d basis)
        {
            // immediately return zero length input vectors
            double sourceLength = source.Length;
            if (sourceLength < double.Epsilon)
                return source;

            // measure the angular diviation of "source" from "basis"
            Vector3d direction = source / sourceLength;

            double cosineOfSourceAngle = Vector3d.DotProduct(direction, basis);

            // Simply return "source" if it already meets the angle criteria.
            // (note: we hope this top "if" gets compiled out since the flag
            // is a constant when the function is inlined into its caller)
            if (insideOrOutside)
            {
                // source vector is already inside the cone, just return it
                if (cosineOfSourceAngle >= cosineOfConeAngle)
                    return source;
            }
            else if (cosineOfSourceAngle <= cosineOfConeAngle)
                return source;

            // find the portion of "source" that is perpendicular to "basis"
            Vector3d perp = PerpendicularComponent(source, basis);
            if (perp == Vector3d.Zero())
                return Vector3d.Zero();

            // normalize that perpendicular
            Vector3d unitPerp = Vector3d.Normalize(perp);

            // construct a new vector whose length equals the source vector,
            // and lies on the intersection of a plane (formed the source and
            // basis vectors) and a cone (whose axis is "basis" and whose
            // angle corresponds to cosineOfConeAngle)
            double  perpDist = (double )Math.Sqrt(1 - (cosineOfConeAngle * cosineOfConeAngle));
            Vector3d c0 = basis * cosineOfConeAngle;
            Vector3d c1 = unitPerp * perpDist;
            return (c0 + c1) * sourceLength;
        }

        /// <summary>
        /// Returns the distance between a point and a line.
        /// </summary>
        /// <param name="point">The point to measure distance to</param>
        /// <param name="lineOrigin">A point on the line</param>
        /// <param name="lineUnitTangent">A UNIT vector parallel to the line</param>
        /// <returns></returns>
        public static double DistanceFromLine(this Vector3d point, Vector3d lineOrigin, Vector3d lineUnitTangent)
        {
            Vector3d offset = point - lineOrigin;
            Vector3d perp = PerpendicularComponent(offset, lineUnitTangent);
            return perp.Length;
        }

        /// <summary>
        /// Find any arbitrary vector which is perpendicular to the given vector
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3d FindPerpendicularIn3d(this Vector3d direction)
        {
            // to be filled in:
            Vector3d quasiPerp;  // a direction which is "almost perpendicular"
            Vector3d result;     // the computed perpendicular to be returned

            // three mutually perpendicular basis vectors
            Vector3d i = Vector3d.Right();
            Vector3d j = Vector3d.Up();
            Vector3d k = Vector3d.Forward();

            // measure the projection of "direction" onto each of the axes
            double id = Vector3d.DotProduct(i, direction);
            double jd = Vector3d.DotProduct(j, direction);
            double kd = Vector3d.DotProduct(k, direction);

            // set quasiPerp to the basis which is least parallel to "direction"
            if ((id <= jd) && (id <= kd))
                quasiPerp = i;           // projection onto i was the smallest
            else if ((jd <= id) && (jd <= kd))
                quasiPerp = j;           // projection onto j was the smallest
            else
                quasiPerp = k;           // projection onto k was the smallest

            // return the cross product (direction x quasiPerp)
            // which is guaranteed to be perpendicular to both of them
            result = Vector3d.CrossProduct(direction, quasiPerp);

            return result;
        }
    }
}
