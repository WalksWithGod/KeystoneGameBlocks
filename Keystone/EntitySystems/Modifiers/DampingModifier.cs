using System;
using Keystone.EntitySystems.Emitters;

namespace Keystone.EntitySystems.Modifiers
{

	internal class DampingModifier : Modifier
	{
        public double DampingCoefficient;

        public DampingModifier (float value)
        {
        	DampingCoefficient = value;
        }
        
        public override void Process(Particle[] particles, double elapsedSeconds)
        {
            double inverseCoefficientDelta = ((DampingCoefficient * elapsedSeconds) * -1d);

            for (int i = 0; i < particles.Length; i++)
            {
            	// slow acceleration in direction of velocity by inverseCoefficientDelta
                particles[i].Acceleration += particles[i].Velocity * inverseCoefficientDelta;
            }
        }
	}
}
