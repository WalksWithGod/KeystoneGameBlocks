using System;
using Keystone.EntitySystems.Emitters;

namespace Keystone.EntitySystems.Modifiers
{

	internal class RotationRateModifier : Modifier
	{
        private float mStartingRotationRate;
        private float mEndingRotationRate;
     
        public RotationRateModifier (float startValue, float endValue)
        {
        	StartingRotationRate = startValue;
        	EndingRotationRate = endValue;
        }
        
        /// <summary>
        /// Starting keyframe rotation rate of Particle.
        /// </summary>
        public float StartingRotationRate
        {
            get { return mStartingRotationRate; }
            set
            {
                mStartingRotationRate = value;
            }
        }

        /// <summary>
        /// Ending keyframe rotation rate of Particle.
        /// </summary>
        public float EndingRotationRate
        {
            get { return mEndingRotationRate; }
            set
            {
                mEndingRotationRate = value;
            }
        }
        		
		public override void Process(Keystone.EntitySystems.Emitters.Particle[] particles, double elapsedSeconds)
		{
            for (int i = 0; i < particles.Length; i++)
            {
                double rate = mStartingRotationRate + ((mEndingRotationRate - mStartingRotationRate) * particles[i].Age);
                double deltaRate = rate * elapsedSeconds;

                particles[i].Rotate(deltaRate);
            }
		} 
	}
}
