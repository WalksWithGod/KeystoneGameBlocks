using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BEPUphysics.Vehicle
{
	public class Vehicle : CombinedUpdateable
	{
		// Constructors
		public Vehicle (Entity shape, float maxForwardSpeed, float maxBackwardSpeed)
		{
			this.wheels = new List<Wheel>();
			this.body = shape;
			this.body.isAlwaysActive = true;
			this.maximumForwardSpeed = maxForwardSpeed;
			this.maximumBackwardSpeed = maxBackwardSpeed;
		}
		
		public Vehicle (Entity shape, float maxForwardSpeed, float maxBackwardSpeed, List<Wheel> wheelList)
		{
			this.wheels = new List<Wheel>();
			this.body = shape;
			this.body.isAlwaysActive = true;
			this.body.isAlwaysActive = true;
			this.maximumForwardSpeed = maxForwardSpeed;
			this.maximumBackwardSpeed = maxBackwardSpeed;
			foreach (Wheel wheel in wheelList)
			{
				this.addWheel(wheel);
			}
		}
		
		
		// Methods
		public void addWheel (Wheel wheel)
		{
			this.wheels.Add(wheel);
			if (base.space != null)
			{
				base.space.add(wheel.detectorShape);
			}
			wheel.vehicle = this;
			wheel.detectorShape.nonCollidableEntities.Add(this.body);
		}
		
		public void removeWheel (Wheel wheel)
		{
			if (this.wheels.Remove(wheel))
			{
				if (base.space != null)
				{
					base.space.remove(wheel.detectorShape);
				}
				wheel.vehicle = null;
			}
			wheel.detectorShape.nonCollidableEntities.Remove(this.body);
		}
		
		public override void addToSpace (Space newSpace)
		{
			base.addToSpace(newSpace);
			foreach (Wheel wheel1 in this.wheels)
			{
				base.space.add(wheel1.detectorShape);
			}
			base.space.add(this.body);
		}
		
		public override void removeFromSpace ()
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				base.space.remove(wheel1.detectorShape);
			}
			base.space.remove(this.body);
			base.removeFromSpace();
		}
		
		public void move (Vector3 v)
		{
			this.body.centerPosition += v;
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.move(v);
			}
		}
		
		public void moveTo (Vector3 v)
		{
			Vector3 vector1 = v - this.body.myInternalCenterPosition;
			this.move(vector1);
		}
		
		public void accelerate (float acceleration)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.acceleration = acceleration;
			}
		}
		
		public void brake (float brakingFriction)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.brake(brakingFriction);
			}
		}
		
		public void releaseBrake ()
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.releaseBrake();
			}
		}
		
		public override void preStep (float dt)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.preStep(dt);
			}
		}
		
		public override void updateVelocities (float dt, float timeScale, float timeSinceLastFrame)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.updateVelocities(dt);
			}
		}
		
		public override void updateAtEndOfUpdate (float dt, float timeScale, float timeSinceLastFrame)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.updateAtEndOfUpdate(dt);
			}
		}
		
		public override void updateAtEndOfFrame (float dt, float timeScale, float timeSinceLastFrame)
		{
			foreach (Wheel wheel1 in this.wheels)
			{
				wheel1.updateAtEndOfFrame(dt);
			}
		}
		
		
		// Properties
		public bool hasWheelSupport
		{
			get
			{
				foreach (Wheel wheel1 in this.wheels)
				{
					if (wheel1.hasSupport)
					{
						return true;
					}
				}
				return false;
			}
		}
		
		public int supportedWheelCount
		{
			get
			{
				int num1 = 0;
				foreach (Wheel wheel1 in this.wheels)
				{
					if (wheel1.hasSupport)
					{
						num1++;
					}
				}
				return num1;
			}
		}
		
		
		// Instance Fields
		public  List<Wheel> wheels;
		public  Entity body;
		public  float maximumForwardSpeed;
		public  float maximumBackwardSpeed;
	}
}
