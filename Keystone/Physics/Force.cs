using Keystone.Physics.Entities;
using Keystone.Types;
using System;

namespace Keystone.Physics
{
	public class Force
	{
        // Instance Fields
        public Vector3d position;
        public Vector3d direction;
        public PhysicsBody target;
        public bool isTrackingTarget;
        public bool isChangingDirectionWhileTracking;
        public bool isActive;
        private Vector3d myRelationToTargetWhenApplied;
        private Vector3d myRelativeDirection;
        public float age;
        public float lifeSpan;

		// Constructors
		public Force (Vector3d pos, Vector3d dir, float time)
		{
			this.isChangingDirectionWhileTracking = true;
			this.isActive = true;
			this.position = pos;
			this.direction = dir;
			this.lifeSpan = time;
		}
		
		public Force (Vector3d pos, Vector3d dir)
		{
			this.isChangingDirectionWhileTracking = true;
			this.isActive = true;
			this.position = pos;
			this.direction = dir;
		}
		
		
		// Methods
		internal void strictlyTarget (PhysicsBody e)
		{
			this.target = e;
		}
		
		public void setTarget (PhysicsBody e)
		{
			if (this.target != null)
			{
				this.target.removeForce(this);
			}
			if (!e.forces.Contains(this))
			{
				e.applyForce(this);
			}
			this.target = e;
            this.myRelationToTargetWhenApplied = this.target.myInternalCenterPosition - this.position;
            this.myRelativeDirection = Vector3d.TransformCoord(this.direction, Matrix.Transpose(this.target.myInternalOrientationMatrix));
            Vector3d.TransformCoord(this.myRelationToTargetWhenApplied, Matrix.Transpose(this.target.myInternalOrientationMatrix));
		}
		
		internal void track ()
		{
            this.position = this.target.myInternalCenterPosition + Vector3d.TransformCoord(this.myRelationToTargetWhenApplied, this.target.myInternalOrientationMatrix);
			if (this.isChangingDirectionWhileTracking)
			{
                this.direction = Vector3d.TransformCoord(this.myRelativeDirection, this.target.myInternalOrientationMatrix);
			}
		}
	}
}
