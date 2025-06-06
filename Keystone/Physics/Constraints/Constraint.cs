using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;
using System;

namespace Keystone.Physics.Constraints
{
    public abstract class Constraint
    {
        // Instance Fields
        protected PhysicsBody myConnectionA;
        protected PhysicsBody myConnectionB;
        protected PhysicsBody myParentA;
        protected PhysicsBody myParentB;
        protected Space mySpace;
        protected Vector3d error;
        protected int numIterationsAtZeroImpulse;
        protected bool calculateImpulse;
        public bool useForceLimit;
        protected float forceLimitSquared;
        protected float forceLimit;
        protected Vector3d anchor;
        protected Vector3d localAnchorA;
        protected Vector3d localAnchorB;
        public Vector3d rA;
        public Vector3d rB;
        protected Matrix massMatrix;
        protected Vector3d biasTimesTimeStep;
        protected Vector3d accumulatedImpulse;
        public float softness;
        public float biasFactor;

        // Constructors
        protected Constraint()
        {
        }

        // Methods
        protected abstract void calculateJacobians();
        protected abstract void calculateMassMatrix();
        protected abstract void calculateError();
        public abstract void preStep(float dt);
        public abstract void applyImpulse(float dt);

        protected void checkForEarlyOutIterations(Vector3d impulse)
        {
            if (impulse.LengthSquared() <
                (this.mySpace.simulationSettings.minimumImpulse*this.mySpace.simulationSettings.minimumImpulse))
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

        protected abstract void calculateBias(float dt);

        protected void applyImpulse(Vector3d impulse)
        {
            if (this.myParentA.isPhysicallySimulated)
            {
                Vector3d vector1 = Vector3d.Negate(impulse);
                this.myParentA.applyLinearImpulse(vector1);
                Vector3d vector2 = Vector3d.CrossProduct(this.rA, vector1);
                this.myParentA.applyAngularImpulse(vector2);
            }
            if (this.myParentB.isPhysicallySimulated)
            {
                this.myParentB.applyLinearImpulse(impulse);
                Vector3d vector3 = Vector3d.CrossProduct(this.rB, impulse);
                this.myParentB.applyAngularImpulse(vector3);
            }
        }


        // Properties
        public PhysicsBody connectionA
        {
            get { return this.myConnectionA; }
        }

        public PhysicsBody connectionB
        {
            get { return this.myConnectionB; }
        }

        public PhysicsBody parentA
        {
            get { return this.myParentA; }
        }

        public PhysicsBody parentB
        {
            get { return this.myParentB; }
        }

        public Space space
        {
            get { return this.mySpace; }
            set { this.mySpace = value; }
        }

        public float forceMaximum
        {
            get { return this.forceLimit; }
            set
            {
                this.forceLimit = value;
                this.forceLimitSquared = this.forceLimit*this.forceLimit;
            }
        }

        public bool isActive
        {
            get
            {
                if (!this.myParentA.myIsActive)
                {
                    return this.myParentB.myIsActive;
                }
                return true;
            }
        }
    }
}