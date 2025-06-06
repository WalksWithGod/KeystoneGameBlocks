using System;

namespace Keystone.Physics
{
	public abstract class Updateable
	{
		// Constructors
		protected Updateable ()
		{
			this.isUpdating = true;
		}
		
		
		// Methods
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
			this.mySpace = newSpace;
		}
		
		public virtual void removeFromSpace ()
		{
			this.mySpace = null;
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
