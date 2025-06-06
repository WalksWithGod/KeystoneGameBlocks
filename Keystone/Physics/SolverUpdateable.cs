using System;

namespace Keystone.Physics
{
	public abstract class SolverUpdateable
	{
		// Constructors
		protected SolverUpdateable ()
		{
		}
		
		
		// Methods
		public virtual void preStep (float dt)
		{
		}
		
		public abstract void update (float dt, float timeScale, float timeSinceLastFrame);
		public virtual void addToSpace (Space newSpace)
		{
			this.space = newSpace;
		}
		
		public virtual void removeFromSpace ()
		{
			this.space = null;
		}
		
		
		// Properties
		public Space space
		{
			get
			{
				return this.mySpace;
			}
			set
			{
				this.mySpace = value;
			}
		}
		
		
		// Instance Fields
		protected  Space mySpace;
	}
}
