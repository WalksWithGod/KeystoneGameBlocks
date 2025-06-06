﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibNoise.Modifiers
{
    public class LargerOutput
        : IModule
    {
        public IModule SourceModule1 { get; set; }
        public IModule SourceModule2 { get; set; }

        public LargerOutput(IModule sourceModule1, IModule sourceModule2)
        {
            if (sourceModule1 == null || sourceModule2 == null)
                throw new ArgumentNullException("Source modules must be provided.");

            SourceModule1 = sourceModule1;
            SourceModule2 = sourceModule2;
        }

        public double GetValue(double x, double y, double z)
        {
            if (SourceModule1 == null || SourceModule2 == null)
                throw new NullReferenceException("Source modules must be provided.");

            return Math.GetLarger(SourceModule1.GetValue(x, y, z),SourceModule2.GetValue(x, y, z));
        }
    }
}
