// Heightmap Functions by Hyperz
// http://www.youtube.com/watch?v=CajuHdM9Deg
// based on http://www.float4x4.net/index.php/2010/06/generating-realistic-and-playable-terrain-height-maps/

using System;

namespace Keystone.Perlin
{
	internal static class NoiseMath
    {
        public static float CubicInterpolate(float n0, float n1, float n2, float n3, float a)
        {
            float p = (n3 - n2) - (n0 - n1);
            float q = (n0 - n1) - p;
            float r = n2 - n0;
            float s = n1;

            return p * a * a * a + q * a * a + r * a + s;
        }

        public static float GetMin(float a, float b)
        {
            return (a < b ? a : b);
        }

        public static float GetMax(float a, float b)
        {
            return (a > b ? a : b);
        }

        public static void SwapValues(ref float a, ref float b)
        {
            float c = a;

            a = b;
            b = c;
        }

        public static float LinearInterpolate(float n0, float n1, float a)
        {
            return ((1.0f - a) * n0) + (a * n1);
        }

        public static double MakeInt32Range(double n)
        {
            if (n >= 1073741824.0)
            {
                return ((2.0 * Math.IEEERemainder(n, 1073741824.0)) - 1073741824.0);
            }
            else if (n <= -1073741824.0)
            {
                return ((2.0 * Math.IEEERemainder(n, 1073741824.0)) + 1073741824.0);
            }
            else
            {
                return n;
            }
        }

        public static float SCurve3(float a)
        {
            return (a * a * (3.0f - 2.0f * a));
        }

        public static float SCurve5(float a)
        {
            float a3 = a * a * a;
            float a4 = a3 * a;
            float a5 = a4 * a;

            return (6.0f * a5) - (15.0f * a4) + (10.0f * a3);
        }
    }
}
