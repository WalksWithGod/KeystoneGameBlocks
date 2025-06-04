using System;
using System.Collections.Generic;
using System.Text;

namespace LibNoise.Models
{
    /// <summary>
    /// Model that maps the output of a module onto a sphere.
    /// </summary>
    public class Sphere : NoiseMapModel 
    {  
        public Sphere(IModule sourceModule) : base(sourceModule) { }

        /// <summary>
        /// Returns noise mapped to the given location in the sphere.
        /// </summary>
        public double GetValue(double latitude, double longitude)
        {
            if (SourceModule == null)
                throw new NullReferenceException("A source module must be provided.");

            double x=0, y=0, z=0;
            LatLonToXYZ(latitude, longitude, ref x, ref y, ref z);
            return SourceModule.GetValue(x, y, z);
        }

        public int[] Generate()
        {
 	        return Generate((uint)this.Width , (uint) this.Height );
        }
        
        // a friendly helper method that makes multiple calls to GetValue() to generate an entire texture
        // note that Line, Plane, Cylinder don't have any friendly helper methods (we could add them ourselves tho) - Hypno
        public int[] Generate(uint textureWidth, uint textureHeight)
        {
            int[] colors = new int[textureWidth * textureHeight];

            float xExtent = _maxX - _minX;
            float yExtent = _maxY - _minY;
            float xDelta = xExtent / textureWidth;
            float yDelta = yExtent / textureHeight;
            float curLon = _minX;
            float curLat = _minY;
            int currentColor = 0;

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    double value = GetValue(curLat, curLon);
                    value = LibNoise.Math.ClampValue(value, 0f, 1.0f);

                    curLon += xDelta;
                    if (Palette != null)
                    {
                        colors[currentColor] = Palette.GetColor(value).ToArgb();
                    }
                    else
                    {
                        if (value < 0) value = 0;
                        if (value > 1.0) value = 1.0;
                        byte intensity = (byte)(value * 255.0);
                        colors[currentColor] = System.Drawing.Color.FromArgb(255, intensity, intensity, intensity).ToArgb();
                    }
                    currentColor++;
                }
                curLon = _minX;
                curLat += yDelta;
            }
            return colors;
        }
    }
}
