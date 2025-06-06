using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;
using System;

namespace Keystone.Physics.Constraints
{
	public abstract class SingleBodyConstraint
	{
		// Constructors
		protected SingleBodyConstraint ()
		{
		}
		
		
		// Methods
		public abstract void preStep (float dt);
		public abstract void applyImpulse (float dt);
		protected void checkForEarlyOutIterations (Vector3d impulse)
		{
			if (impulse.LengthSquared() < (this.mySpace.simulationSettings.minimumImpulse * this.mySpace.simulationSettings.minimumImpulse))
			{
				this.numIterationsAtZeroImpulse++;
			}
			else
			{
				this.numIterationsAtZeroImpulse = 0;
			}
			if (this.numIterationsAtZeroImpulse > this.mySpace.simulationSettings.iterationsBeforeEarlyOut)
			{
				this.calculateImpulse = false;
			}
		}
		
		
		// Properties
		public PhysicsBody physicsBody
		{
			get
			{
				return this.myPhysicsBody;
			}
		}
		
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
		
		public bool isActive
		{
			get
			{
				return this.myPhysicsBody.myIsActive;
			}
		}
		
		
		// Instance Fields
		protected  PhysicsBody myPhysicsBody;
		protected  Space mySpace;
		protected  int numIterationsAtZeroImpulse;
		protected  bool calculateImpulse;
	}
}
