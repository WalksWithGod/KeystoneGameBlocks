using System;
using Keystone.EntitySystems.Emitters;
using Keystone.Types;

namespace Keystone.EntitySystems.Modifiers
{
	/// <summary>
	/// Description of ColorModifier.
	/// </summary>
	internal class ColorModifier : Modifier
	{
		public ColorModifier (Color startValue, Color endValue)
		{
			StartingColor = startValue;
			EndingColor = endValue;
		}
		
        /// <summary>
        /// Starting keyframe color of Particles.
        /// </summary>
        public Color StartingColor;

        /// <summary>
        /// Ending keyframe color of Particles.
        /// </summary>
        public Color EndingColor;

		public override void Process(Keystone.EntitySystems.Emitters.Particle[] particles, double elapsedSeconds)
		{
			for (int i = 0; i < particles.Length; i++)
            { 
				particles[i].Color.r = StartingColor.r + ((EndingColor.r - StartingColor.r) * particles[i].Age);
                particles[i].Color.g = StartingColor.g + ((EndingColor.g - StartingColor.g) * particles[i].Age);
                particles[i].Color.b = StartingColor.b + ((EndingColor.b - StartingColor.b) * particles[i].Age);
                
			} 
		}
	}
}
