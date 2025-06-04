// Heightmap Functions by Hyperz
// http://www.youtube.com/watch?v=CajuHdM9Deg
// based on http://www.float4x4.net/index.php/2010/06/generating-realistic-and-playable-terrain-height-maps/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Keystone.Types;

namespace Keystone.Perlin
{
    public class HeightMap
    {
    	// the overlap allows better smoothing between adjacent heightmaps which are
    	// stiched together to create a larger world.  But I suspect such a large overlap
    	// is necessary when no overall map is used and each adjacent is generated in a vacuum.
    	// I think in our version, the overlap can just be a value of how much of the actual
    	// size we want to use for averaging neighbors 
        public const byte Overlap = 34;

        private float[,] _heights;
        private int _size, _startX, _startY;

        #region Properties
        public int StartY
        {
            get { return _startY; }
            set { _startY = value; }
        }

        public int StartX
        {
            get { return _startX; }
            set { _startX = value; }
        }

        public float this[int x, int y]
        {
            get { return _heights[x + Overlap, y + Overlap]; }
            set { _heights[x + Overlap, y + Overlap] = value; }
        }

        public int Size
        {
            get { return _size; }
        }
        #endregion

        public HeightMap(int size, int startX = 0, int startY = 0)
        {
            if (size < 2 || (size & (size - 1)) != 0)
            {
                throw new ArgumentException("Size must be bigger than 1 and a power of 2.", "size");
            }

            int realSize = size + (Overlap * 2);

            _size = size;
            _startX = startX;
            _startY = startY;
            _heights = new float[realSize, realSize];
        }

        #region Public Methods
        public float[,] GetOverlappedHeights()
        {
            return _heights;
        }

        public void AlignEdges(HeightMap leftNeighbor, HeightMap rightNeighbor,
            HeightMap topNeighbor, HeightMap bottomNeighbor, int shift = 0)
        {
            int x, y, counter;
            float[,] nHeights;
            float value;
            int size = this.Size;

            if (leftNeighbor != null)
            {
                nHeights = leftNeighbor.GetOverlappedHeights();
                counter = 0;

                // iterate overlapped RIGHT most pixels of the LEFT neighbor to this map
                for (x = size + Overlap - shift; x < size + (Overlap * 2); x++)
                {
                    for (y = 0; y < size; y++)
                    {
                    	// average of overlapped height values on this map and left neighbor
                        value = (_heights[counter, y] + nHeights[x, y]) / 2f;
                        // assign this value to both this map and left neighbor
                        _heights[counter, y] = nHeights[x, y] = value;
                    }

                    counter++;
                }

                x = size - 1;

                for (y = 0; y < size; y++)
                {
                    value = (this[0, y] + leftNeighbor[x, y]) / 2f;
                    this[0, y] = leftNeighbor[x, y] = value;
                }
            }
            if (rightNeighbor != null)
            {
                nHeights = rightNeighbor.GetOverlappedHeights();
                counter = 0;

                for (x = size + Overlap - shift; x < size + (Overlap * 2); x++)
                {
                    for (y = 0; y < size; y++)
                    {
                        value = (_heights[x, y] + nHeights[counter, y]) / 2f;
                        _heights[x, y] = nHeights[counter, y] = value;
                    }

                    counter++;
                }

                x = size - 1;

                for (y = 0; y < size; y++)
                {
                    value = (this[x, y] + rightNeighbor[0, y]) / 2f;
                    this[x, y] = rightNeighbor[0, y] = value;
                }
            }
            if (topNeighbor != null)
            {
                nHeights = topNeighbor.GetOverlappedHeights();
                counter = 0;

                for (y = size + Overlap - shift; y < size + (Overlap * 2); y++)
                {
                    for (x = 0; x < size; x++)
                    {
                        value = (_heights[x, y] + nHeights[x, counter]) / 2f;
                        _heights[x, y] = nHeights[x, counter] = value;
                    }

                    counter++;
                }

                y = size - 1;

                for (x = 0; x < size; x++)
                {
                    value = (this[x, 0] + topNeighbor[x, y]) / 2f;
                    this[x, 0] = topNeighbor[x, y] = value;
                }
            }
            if (bottomNeighbor != null)
            {
                nHeights = bottomNeighbor.GetOverlappedHeights();
                counter = 0;

                for (y = size + Overlap - shift; y < size + (Overlap * 2); y++)
                {
                    for (x = 0; x < size; x++)
                    {
                        value = (_heights[x, counter] + nHeights[x, y]) / 2f;
                        _heights[x, counter] = nHeights[x, y] = value;
                    }

                    counter++;
                }

                y = size - 1;

                for (x = 0; x < size; x++)
                {
                    value = (this[x, 0] + bottomNeighbor[x, y]) / 2f;
                    this[x, 0] = bottomNeighbor[x, y] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="octaves"></param>
        /// <param name="persistence"></param>
        /// <param name="lacunarity"></param>
        /// <param name="additive"></param>
        public void SetNoise(float frequency, byte octaves = 1, float persistence = 0.5f, float lacunarity = 2.0f,
            bool additive = false)
        {
            int size = _heights.GetLength(0);
            int startX = _startX - Overlap;
            int startY = _startY - Overlap;
            float fSize = (float)size;

            // OUTER LOOP -> x axis
            Parallel.For(0, size, x =>
            {
             	// TODO: is this thread priority assignment
             	// only affecting threadpool threads? otherwise shouldn't we be changing it back?
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                byte currentOctave;
                float value, currentPersistence, sample;
                Vector2f coord;

                // INNER LOOP -> y axis
                for (int y = 0; y < size; y++)
                {
                    value = 0.0f;
                    currentPersistence = 1.0f;
                    coord = new Vector2f(
                        (x + startX) / fSize,
                        (y + startY) / fSize);

                    coord *= frequency;

                    // TODO: is there a way to pass in a currentPersistence value
                    //       so that when running the noise against existing noise at lower resolution
                    //       that we can "resume" so to speak.  NOTE: "addictive" at 1:1 resolution is
                    //       already available by passing "addictive=true"
                    //       I suspect that it "should" be possible by pre-seeding the _heights with the value
                    //       we want and then setting "addictive=true"  or wait... no... cuz it should multiply.
                    //       I think what we need to do is affect the values of startX and startY, and then sample
                    //       at a frequency of 32.  And we would use currentPersistance = _heights[x,y] for multiplicative
                    //       It's also important for when we want to be able to seed a map with basic hand painted hints
                    for (currentOctave = 0; currentOctave < octaves; currentOctave++)
                    {
                        sample = Perlin.Noise2(coord.x, coord.y);
                        value += sample * currentPersistence;
                        coord *= lacunarity;
                        currentPersistence *= persistence;
                    }

                    _heights[x, y] = (!additive) ? value : _heights[x, y] + value;
                }
            });
        }

        public void Perturb(float frequency, float depth)
        {
            int u, v, i, j;
            int size = _heights.GetLength(0);
            int startX = _startX - Overlap;
            int startY = _startY - Overlap;
            float[,] temp = new float[size, size];
            float fSize = (float)size;
            Vector2f coord;
            
            for (i = 0; i < size; ++i)
            {
                for (j = 0; j < size; ++j)
                {
                    coord = new Vector2f(
                        (i + startX) / fSize,
                        (j + startY) / fSize);

                    coord *= frequency;

                    u = i + (int)(Perlin.Noise3(coord.x, coord.y, 0.0f) * depth);
                    v = j + (int)(Perlin.Noise3(coord.x, coord.y, 1.0f) * depth);

                    if (u < 0) u = 0;
                    if (u >= size) u = size - 1;
                    if (v < 0) v = 0;
                    if (v >= size) v = size - 1;

                    temp[i, j] = _heights[u, v];
                }
            }

            _heights = temp;
        }

        public void Erode(float smoothness)
        {
            int size = _heights.GetLength(0);

            Parallel.For(1, size - 1, i =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                int u, v;
                float d_max, d_i, d_h;
                int[] match;

                for (int j = 1; j < size - 1; j++)
                {
                    d_max = 0.0f;
                    match = new[] { 0, 0 };

                    for (u = -1; u <= 1; u++)
                    {
                        for (v = -1; v <= 1; v++)
                        {
                            if (Math.Abs(u) + Math.Abs(v) > 0)
                            {
                                d_i = _heights[i, j] - _heights[i + u, j + v];

                                if (d_i > d_max)
                                {
                                    d_max = d_i;
                                    match[0] = u;
                                    match[1] = v;
                                }
                            }
                        }
                    }

                    if (0 < d_max && d_max <= (smoothness / (float)size))
                    {
                        d_h = 0.5f * d_max;

                        _heights[i, j] -= d_h;
                        _heights[i + match[0], j + match[1]] += d_h;
                    }
                }
            });
        }

        public void Smoothen()
        {
            int i, j, u, v;
            int size = _heights.GetLength(0);
            float total;

            // NOTE: border pixels along min x and max x are skipped 
            for (i = 1; i < size - 1; ++i)
            {
            	// NOTE: border pixels along min y and max y are skipped
                for (j = 1; j < size - 1; ++j)
                {
                    total = 0.0f;

                    // get sum height of all adjacents around this center i,j location
                    for (u = -1; u <= 1; u++)
                    {
                        for (v = -1; v <= 1; v++)
                        {
                            total += _heights[i + u, j + v];
                        }
                    }

                    // assign average
                    _heights[i, j] = total / 9.0f;
                }
            }
        }

        public void Normalize()
        {
            int size = _heights.GetLength(0);
            float min = -1f, max = 1f;

            /*for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (_heights[x, y] > max)
                    {
                        max = _heights[x, y];
                    }
                    else if (_heights[x, y] < min)
                    {
                        min = _heights[x, y];
                    }
                }
            }

            if (min >= max) throw new Exception("Hmm...");*/

            Parallel.For(0, size, x =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                for (int y = 0; y < size; y++)
                {
                    _heights[x, y] = (_heights[x, y] - min) / (max - min);
                }
            });
        }

        public void MakeFlat(float height = 0.0f)
        {
            int x, y;
            int size = _heights.GetLength(0);

            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    _heights[x, y] = height;
                }
            }
        }

        public void Multiply(float amount)
        {
            int x, y;
            int size = _heights.GetLength(0);

            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    _heights[x, y] *= amount;
                }
            }
        }

        public void ForEach(Func<float, float> body)
        {
            int size = _heights.GetLength(0);

            Parallel.For(0, size, x =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                for (int y = 0; y < size; y++)
                {
                    _heights[x, y] = body(_heights[x, y]);
                }
            });
        }

        /*public void ForEach(Func<float, float, float, float> body)
        {
            int x, y;
            int size = _heights.GetLength(0);

            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    _heights[x, y] = body(x, y, _heights[x, y]);
                }
            }
        }*/

        #endregion
    }
}


