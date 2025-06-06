using Keystone.Physics.Entities;
using Keystone.Types;
using System;

namespace Keystone.Physics
{
	public class Contact
	{
        // Instance Fields
        public PhysicsBody colliderA;
        public PhysicsBody colliderB;
        public Vector3d position;
        public Vector3d normal;
        internal double normalImpulseTotal;
        internal double normalImpulseBiasTotal;
        internal double frictionImpulseTotal;
        public double bounciness;
        public double friction;
        internal double restitutionBias;
        internal double normalDenominator;
        internal double frictionDenominator;
        internal double bias;
        public double penetrationDepth;
        public Vector3d tangentDirection;
        public Vector3d Ra;
        public Vector3d Rb;
        public int id;
        public double baseDepth;

		// Constructors
        internal Contact(Vector3d pos, Vector3d norm, PhysicsBody collider, PhysicsBody collidee, PhysicsBody parentA, PhysicsBody parentB, double depth)
		{
			this.id = -1;
			this.baseDepth = depth;
			this.position = pos;
			this.normal = norm;
			this.colliderA = collider;
			this.colliderB = collidee;
			this.Ra = this.position - parentA.myInternalCenterOfMass;
			this.Rb = this.position - parentB.myInternalCenterOfMass;
			this.penetrationDepth = depth;
			this.normal = Vector3d.Normalize(this.normal);
		}
		
		
		// Methods
        internal void setup(Vector3d pos, Vector3d norm, PhysicsBody collider, PhysicsBody collidee, PhysicsBody parentA, PhysicsBody parentB, double depth)
		{
			this.baseDepth = depth;
			this.position = pos;
			this.normal = norm;
			this.colliderA = collider;
			this.colliderB = collidee;
			this.Ra = this.position - parentA.myInternalCenterOfMass;
			this.Rb = this.position - parentB.myInternalCenterOfMass;
			this.penetrationDepth = depth;
			this.normal = Vector3d.Normalize(this.normal);
			this.normalImpulseTotal = 0.00f;
			this.normalImpulseBiasTotal = 0.00f;
			this.frictionImpulseTotal = 0.00f;
		}
		
		internal bool isSimilarTo (Contact c)
		{
			Vector3d vector1 = this.position - c.position;
			if (vector1.LengthSquared() < 0.1d)
			{
				return true;
			}
			return false;
		}
		
		public override string ToString ()
		{
			return string.Concat(new object[]{"Position: ", this.position, "\nNormal: ", this.normal, "\nDepth: ", this.penetrationDepth});
		}

        //public string Serialize()
        //{

        //}
	}
}
