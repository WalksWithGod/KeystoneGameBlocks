using System;
using Keystone.EntitySystems.Emitters;

namespace Keystone.EntitySystems.Modifiers
{
	/// <summary>
	/// Description of RotationModifier.
	/// </summary>
	internal class RotationModifier : Modifier
	{
		
		public RotationModifier (float value)
		{
			RotationRate = value;
		}
		
		/// <summary>
        /// The rate of rotation in radians per second.
        /// </summary>
        public double RotationRate;
        
		public override void Process(Keystone.EntitySystems.Emitters.Particle[] particles, double elapsedSeconds)
		{
            double deltaRotation = this.RotationRate * elapsedSeconds;
            
			for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Rotate(deltaRotation);
            }
		} 
	}
}
