﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibNoise.Modifiers
{
    public struct CurveControlPoint
    {
        public double Input;
        public double Output;
    }

    public class CurveOutput
        : Math, IModule
    {
        public IModule SourceModule { get; set; }
        public List<CurveControlPoint> ControlPoints = new List<CurveControlPoint>();

        public CurveOutput(IModule sourceModule)
        {
            if (sourceModule == null)
                throw new ArgumentNullException("A source module must be provided.");

            SourceModule = sourceModule;
        }

        public double GetValue(double x, double y, double z)
        {
            if (SourceModule == null)
                throw new NullReferenceException("A source module must be provided.");
            if (ControlPoints.Count < 4)
                throw new Exception("Four or more control points must be specified.");

            // Get the output value from the source module.
            double sourceModuleValue = SourceModule.GetValue(x, y, z);

            int controlPointCount = ControlPoints.Count;

            // Find the first element in the control point array that has an input value
            // larger than the output value from the source module.
            int indexPos;
            for (indexPos = 0; indexPos < controlPointCount; indexPos++)
            {
                if (sourceModuleValue < ControlPoints[indexPos].Input)
                {
                    break;
                }
            }

            // Find the four nearest control points so that we can perform cubic
            // interpolation.
            int index0 = Math.ClampValue(indexPos - 2, 0, controlPointCount - 1);
            int index1 = Math.ClampValue(indexPos - 1, 0, controlPointCount - 1);
            int index2 = Math.ClampValue(indexPos, 0, controlPointCount - 1);
            int index3 = Math.ClampValue(indexPos + 1, 0, controlPointCount - 1);

            // If some control points are missing (which occurs if the value from the
            // source module is greater than the largest input value or less than the
            // smallest input value of the control point array), get the corresponding
            // output value of the nearest control point and exit now.
            if (index1 == index2)
            {
                return ControlPoints[index1].Output;
            }

            // Compute the alpha value used for cubic interpolation.
            double input0 = ControlPoints[index1].Input;
            double input1 = ControlPoints[index2].Input;
            double alpha = (sourceModuleValue - input0) / (input1 - input0);

            // Now perform the cubic interpolation given the alpha value.
            return CubicInterpolate(
              ControlPoints[index0].Output,
              ControlPoints[index1].Output,
              ControlPoints[index2].Output,
              ControlPoints[index3].Output,
              alpha);
        }
    }
}
