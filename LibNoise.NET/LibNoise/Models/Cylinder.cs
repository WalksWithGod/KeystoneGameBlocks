using System;
using System.Collections.Generic;
using System.Text;

namespace LibNoise.Models
{
    /// <summary>
    /// Model that maps the output of a module onto a cylinder.
    /// </summary>
    public class Cylinder : NoiseMapModel 
    {
        public Cylinder(IModule sourceModule) : base(sourceModule) { }
        
        /// <summary>
        /// Returns noise mapped to the given angle and height along the cylinder.
        /// </summary>
        public double GetValue(double angle, double height)
        {
            if (SourceModule == null)
                throw new NullReferenceException("A source module must be provided.");

            double x, y, z;
            x = System.Math.Cos(angle);
            y = height;
            z = System.Math.Sin(angle);
            return SourceModule.GetValue(x, y, z);
        }
    }
}
