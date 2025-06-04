using System;
using System.Collections.Generic;
using System.Text;

namespace LibNoise.Models
{
    /// <summary>
    /// Model that maps the output of a module onto a plane.
    /// </summary>
    public class Plane : NoiseMapModel 
    {
        public Plane(IModule sourceModule) : base(sourceModule) { }

        /// <summary>
        /// Returns noise mapped to the given location on the plane.
        /// </summary>
        public double GetValue(double x, double z)
        {
            if (SourceModule == null)
                throw new NullReferenceException("A source module must be provided.");

            return SourceModule.GetValue(x, z, 0); // (x, 0, z);
        }
    }
}
