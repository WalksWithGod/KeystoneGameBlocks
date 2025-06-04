using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Types
{
    /// <summary>
    /// Defines the current axes on which a manipulator is operating
    /// </summary>
    [Flags]
    public enum AxisFlags : int
    {
        None = 0,

        X = (0x1 << 0),
        Y = (0x1 << 1),
        Z = (0x1 << 2),

        XY = X | Y,
        YX = Y | X,
        XZ = X | Z,
        ZX = Z | X,
        YZ = Y | Z,
        ZY = Z | Y,

        XYZ = X | Y | Z,

        All = XYZ
    }

    /// <summary>
    /// Defines the vector space in which a manipulator is operating
    /// </summary>
    public enum VectorSpace
    {
        World,              // Manipulating with world space basis vectors
        Local               // Manipulating with local space basis vectors
    }

    public class Axis
    {

        /// <summary>
        /// Utility function that returns the unit axis in Vector3 format that corresponds to the 
        /// specified axes, oriented based on the vector space of the manipulator
        /// </summary>
        /// <param name="axis">The axes for which to retrieve the corresponding unit axis</param>
        /// <returns>The unit axis that corresponds to the specified axes</returns>
        public static Vector3d GetUnitAxis(Quaternion targetRotation, AxisFlags axes, VectorSpace vectorSpace)
        {
            Vector3d unit;
            unit.x = 0;
            unit.y = 0;
            unit.z = 0;

            // note these are NOT if / else blocks.  Execution falls through and each successive flag can
            // potentially be true when multiple axis are ORd together
            if ((axes & AxisFlags.X) == AxisFlags.X)
                unit.x += 1;
            if ((axes & AxisFlags.Y) == AxisFlags.Y)
                unit.y += 1;
            if ((axes & AxisFlags.Z) == AxisFlags.Z)
                unit.z += 1;

            if (unit.x != 0 || unit.y != 0 || unit.z != 0)
                unit.Normalize();

            // in local vector space, rotate the axis with the transform's
            // rotation component, otherwise return the axis in its default
            // form for world vector space
            unit = (vectorSpace == VectorSpace.Local)
                ? Vector3d.TransformNormal(unit, targetRotation)
                : unit;

            return unit;
        }
    }

    
}
