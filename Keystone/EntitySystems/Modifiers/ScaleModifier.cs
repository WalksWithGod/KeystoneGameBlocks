using System;
using Keystone.EntitySystems.Emitters;
using Keystone.Types;

namespace Keystone.EntitySystems.Modifiers
{

	internal class ScaleModifier : Modifier
	{
        private Vector3d mStartingScale;
        private Vector3d mEndingScale;
        
        public ScaleModifier (Vector3d start, Vector3d end)
        {
        	StartingScale = start;
        	EndingScale = end;
        }
        
        /// <summary>
        /// Starting keyframe Scale of Particle.
        /// </summary>
        public Vector3d StartingScale
        {
            get { return mStartingScale; }
            set
            {
                if (value == Vector3d.Zero()) throw new ArgumentOutOfRangeException();
                mStartingScale = value;
            }
        }

        /// <summary>
        /// Ending keyframe Scale of Particle.
        /// </summary>
        public Vector3d EndingScale
        {
            get { return mEndingScale; }
            set
            {
            	if (value == Vector3d.Zero()) throw new ArgumentOutOfRangeException();
                mEndingScale = value;
            }
        }
        		
		public override void Process(Keystone.EntitySystems.Emitters.Particle[] particles, double elapsedSeconds)
		{
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Scale.x = mStartingScale.x + ((mEndingScale.x - mStartingScale.x) * particles[i].Age);
                particles[i].Scale.y = mStartingScale.y + ((mEndingScale.y - mStartingScale.y) * particles[i].Age);
                particles[i].Scale.z = mStartingScale.z + ((mEndingScale.z - mStartingScale.z) * particles[i].Age);
            }
		} 
	}
}
