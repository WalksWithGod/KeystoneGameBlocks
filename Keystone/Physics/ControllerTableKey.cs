using Keystone.Physics.Entities;
using System;

namespace Keystone.Physics
{
	internal class ControllerTableKey
	{
		// Constructors
		internal ControllerTableKey (PhysicsBody physicsBodyA, PhysicsBody physicsBodyB)
		{
			this.a = physicsBodyA;
			this.b = physicsBodyB;
			this.currentHashCode = this.a.GetHashCode() + this.b.GetHashCode();
		}
		
		
		// Methods
		public override bool Equals (object o)
		{
			ControllerTableKey key1 = (ControllerTableKey) o;
			if ((key1.a == this.a) && (key1.b == this.b))
			{
				return true;
			}
			if (key1.a == this.b)
			{
				return key1.b == this.a;
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return this.currentHashCode;
		}
		
		internal void setup (PhysicsBody physicsBodyA, PhysicsBody physicsBodyB)
		{
			this.a = physicsBodyA;
			this.b = physicsBodyB;
			this.currentHashCode = this.a.GetHashCode() + this.b.GetHashCode();
		}
		
		
		// Instance Fields
		private  PhysicsBody a;
		private  PhysicsBody b;
		private  int currentHashCode;
	}
}
