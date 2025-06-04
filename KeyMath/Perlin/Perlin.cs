// Perlin Functions by Hyperz
// http://www.youtube.com/watch?v=CajuHdM9Deg
// based on http://www.float4x4.net/index.php/2010/06/generating-realistic-and-playable-terrain-height-maps/

using System;
using System.Collections.Generic;


namespace Keystone.Perlin
{
    public static class Perlin
    {
        private static byte[] _randomPermutations = new byte[512];
        private static byte[] _selectedPermutations = new byte[512];
        private static float[] _gradientTable = new float[512];
        private static int _seed;

        
        static Perlin()
        {
            Seed = (int)Math.Abs(Environment.TickCount);
        }
                
        public static int Seed
        {
            get { return _seed; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Perlin.Seed.Set() - Seed must be positive.");
                }

                _seed = value;

                // Generate new random permutations with this seed.
                Random random = new Random(_seed);

                random.NextBytes(_randomPermutations);

                for (int i = 0; i < 256; i++)
                    _selectedPermutations[256 + i] = _selectedPermutations[i] = _randomPermutations[i];

                // Generate a new gradient table
                float[] kkf = new float[256];

                for (int i = 0; i < 256; i++)
                    kkf[i] = -1.0f + 2.0f * ((float)i / 255.0f);

                for (int i = 0; i < 256; i++)
                    _gradientTable[i] = kkf[_selectedPermutations[i]];

                for (int i = 256; i < 512; i++)
                    _gradientTable[i] = _gradientTable[i & 255];
            }
        }

        /// <summary>
        /// Classic 3-D Noise. 3-D noise accepts 'z' parameter as 'weight' value for final interpolation.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Noise3(float x, float y, float z)
        {
            int x0 = (x > 0.0f ? (int)x : (int)x - 1);
            int y0 = (y > 0.0f ? (int)y : (int)y - 1);
            int z0 = (z > 0.0f ? (int)z : (int)z - 1);

            int X = x0 & 255;
            int Y = y0 & 255;
            int Z = z0 & 255;

            // Lower Quality
            //float u = (x - x0),
            //      v = (y - y0),
            //      w = (z - z0);

            // Normal Quality
            float u = NoiseMath.SCurve3(x - x0),
                  v = NoiseMath.SCurve3(y - y0),
                  w = NoiseMath.SCurve3(z - z0);

            // Higher Quality
            //float u = NoiseMath.SCurve5(x - x0),
            //      v = NoiseMath.SCurve5(y - y0),
            //      w = NoiseMath.SCurve5(z - z0);

            int A = _selectedPermutations[X] + Y, AA = _selectedPermutations[A] + Z, AB = _selectedPermutations[A + 1] + Z,
                B = _selectedPermutations[X + 1] + Y, BA = _selectedPermutations[B] + Z, BB = _selectedPermutations[B + 1] + Z;

            float a = NoiseMath.LinearInterpolate(_gradientTable[AA], _gradientTable[BA], u);
            float b = NoiseMath.LinearInterpolate(_gradientTable[AB], _gradientTable[BB], u);
            float c = NoiseMath.LinearInterpolate(a, b, v);
            float d = NoiseMath.LinearInterpolate(_gradientTable[AA + 1], _gradientTable[BA + 1], u);
            float e = NoiseMath.LinearInterpolate(_gradientTable[AB + 1], _gradientTable[BB + 1], u);
            float f = NoiseMath.LinearInterpolate(d, e, v);

            return NoiseMath.LinearInterpolate(c, f, w);
        }

        /// <summary>
        /// Classic 2-D Noise.
        /// Finds noise value using interpolated values from pre-computed lookups generated with seed value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float Noise2(float x, float y)
        {
            int x0 = (x > 0.0f ? (int)x : (int)x - 1);
            int y0 = (y > 0.0f ? (int)y : (int)y - 1);

            int X = x0 & 255;
            int Y = y0 & 255;

            // Lower Quality
            //float u = (x - x0),
            //      v = (y - y0);

            // Normal Quality
            float u = NoiseMath.SCurve3(x - x0),
                  v = NoiseMath.SCurve3(y - y0);

            // Higher Quality
            //float u = NoiseMath.SCurve5(x - x0),
            //        v = NoiseMath.SCurve5(y - y0);

            int A = _selectedPermutations[X] + Y, AA = _selectedPermutations[A], AB = _selectedPermutations[A + 1],
                B = _selectedPermutations[X + 1] + Y, BA = _selectedPermutations[B], BB = _selectedPermutations[B + 1];

            float a = NoiseMath.LinearInterpolate(_gradientTable[AA], _gradientTable[BA], u);
            float b = NoiseMath.LinearInterpolate(_gradientTable[AB], _gradientTable[BB], u);
            float c = NoiseMath.LinearInterpolate(a, b, v);
            float d = NoiseMath.LinearInterpolate(_gradientTable[AA + 1], _gradientTable[BA + 1], u);
            float e = NoiseMath.LinearInterpolate(_gradientTable[AB + 1], _gradientTable[BB + 1], u);
            float f = NoiseMath.LinearInterpolate(d, e, v);

            return NoiseMath.LinearInterpolate(c, f, 0);
        }
    }

}