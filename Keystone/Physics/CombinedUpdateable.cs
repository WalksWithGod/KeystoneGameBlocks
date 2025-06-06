using System;

namespace Keystone.Physics
{
	public abstract class CombinedUpdateable
	{
		// Constructors
		protected CombinedUpdateable ()
		{
			this.isUpdating = true;
		}
		
		
		// Methods
		public virtual void preStep (float dt)
		{
		}
		
		public abstract void updateVelocities (float dt, float timeScale, float timeSinceLastFrame);
		public virtual void updateAtEndOfFrame (float dt, float timeScale, float timeSinceLastFrame)
		{
		}
		
		public virtual void updateAtEndOfUpdate (float dt, float timeScale, float timeSinceLastFrame)
		{
		}
		
		public virtual void updateDuringForces (float dt, float timeScale, float timeSinceLastFrame)
		{
		}
		
		public virtual void updateBeforeCollisionDetection (float dt, float timeScale, float timeSinceLastFrame)
		{
		}
		
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
		public  bool isUpdating;
	}
}
