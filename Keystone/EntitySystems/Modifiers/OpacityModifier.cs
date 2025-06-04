using System;
using Keystone.EntitySystems.Emitters;

namespace Keystone.EntitySystems.Modifiers
{

	internal class OpacityModifier : Modifier
	{
        private float mStartingOpacity;
        private float mEndingOpacity;
        
        public OpacityModifier (float start, float end)
        {
        	StartingOpacity = start;
        	EndingOpacity = end;
        }
        
        /// <summary>
        /// Starting keyframe opacity of Particle.
        /// </summary>
        public float StartingOpacity
        {
            get { return mStartingOpacity; }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException();
                mStartingOpacity = value;
            }
        }

        /// <summary>
        /// Ending keyframe opacity of Particle.
        /// </summary>
        public float EndingOpacity
        {
            get { return mEndingOpacity; }
            set
            {
            	if (value < 0 || value > 1) throw new ArgumentOutOfRangeException();
                mEndingOpacity = value;
            }
        }
        		
		public override void Process(Keystone.EntitySystems.Emitters.Particle[] particles, double elapsedSeconds)
		{
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Color.a = mStartingOpacity + ((mEndingOpacity - mStartingOpacity) * particles[i].Age);
            }
		} 
	}
}
