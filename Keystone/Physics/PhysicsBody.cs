using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Physics.Constraints;
using Keystone.Physics.Events;
using Keystone.Physics.Primitives;

namespace Keystone.Physics
{
    public class PhysicsBody // from old BEPU physics port
    {
        // TODO: seperate out certain files like the friction to material class
        // http://www.newtondynamics.com/wiki/index.php5?title=Materials

        // Instance Fields
        private Space mySpace;
        public CollisionPrimitive CollisionPrimitive;
        private Keystone.Entities.Entity _entity;

        internal PhysicsBody myParent;
        public CompoundBody compoundBody;
        public List<PhysicsBody> nonCollidableEntities;
        public SimulationIsland simulationIsland;
        public List<Controller> controllers;
        public List<Constraint> constraints;
        public List<Force> forces;

        public bool isSubBodyOfCompound;


        internal float timeBelowLinearVelocityThreshold;
        internal float timeBelowAngularVelocityThreshold;
        private Vector3d previousLinearMomentum;
        private Vector3d previousAngularMomentum;

        internal Matrix localInertiaTensor;
        internal Matrix localInertiaTensorInverse;
        internal Matrix internalInertiaTensorInverse;
        internal Matrix internalInertiaTensor;

        public float linearDamping;
        public float angularDamping;
        private float linearDampingBoost;
        private float angularDampingBoost;
        public Vector3d correctiveLinearVelocity;
        public Vector3d correctiveAngularVelocity;

        
        internal Matrix myInternalOrientationMatrix;
        internal Quaternion myInternalOrientationQuaternion;
        internal Quaternion myInternalPreviousOrientationQuaternion;
        internal Vector3d myInternalCenterOfMass;  // myInternalCenterofMass is world coordinates whereas myCenterOfMass is local space
        internal Vector3d myInternalPreviousCenterOfMass;
        internal Vector3d myInternalLinearMomentum;
        internal Vector3d myInternalAngularMomentum;
        internal Vector3d myInternalLinearVelocity;
        internal Vector3d myInternalAngularVelocity;

        //internal Matrix myOrientationMatrix;
        // internal Quaternion myOrientationQuaternion;
        //internal Vector3d myCenterOfMass;
        internal Vector3d myCenterOfMassOffset; // offset of center of mass with respect to center of bounding box
        // obsolete? internal Vector3d myCenterPosition;
        // obsolete?    internal Vector3d myLinearMomentum;
        // obsolete?     internal Vector3d myAngularMomentum;
        // obsolete?     internal Vector3d myLinearVelocity;
        // obsolete?    internal Vector3d myAngularVelocity;
        private float _mass;
        public float mass 
        {
            get { return _mass;} 
            set {
                _mass = value;
                massReciprocal = 1/_mass;
            }
        }
        public float massReciprocal;
        public float volume;
        public float density;
        internal Vector3d force;
        internal Vector3d torque;

        internal float myBounciness;
        internal float myDynamicFriction;
        internal float myStaticFriction;
        internal bool myIsTangible;
        internal bool myIsDetector;
        internal bool isMemberOfRayCastableContainer;
        public bool allowInterpolation;
        public bool isPhysicallySimulated;
        internal bool myIsActive;
        public bool isAlwaysActive;
        internal bool isDeactivationCandidate;
        public bool isAffectedByGravity;
        internal bool isEventful;

        internal byte internalIterationFlag;
        public ulong collisionFilter;


        //// sycnhronization
        //private object lockerCenterPosition;
        //private object lockerCenterOfMassOffset;
        //private object lockerOrientationQuaternion;
        //private object lockerAngularMomentum;
        //private object lockerAngularVelocity;
        //private object lockerLinearMomentum;
        //private object lockerLinearVelocity;
        //private object lockerGeneralWriteBuffer;

        //// write buffer
        //private Vector3d writeBufferCenterPosition;
        //private Vector3d writeBufferCenterOfMassOffset;
        //private Quaternion writeBufferOrientationQuaternion;
        //private Vector3d writeBufferLinearMomentum;
        //private Vector3d writeBufferAngularMomentum;
        //private Vector3d writeBufferLinearVelocity;
        //private Vector3d writeBufferAngularVelocity;
        //private bool writeBufferCenterPositionChanged;
        //private bool writeBufferCenterOfMassOffsetChanged;
        //private bool writeBufferOrientationQuaternionChanged;
        //private bool writeBufferLinearMomentumChanged;
        //private bool writeBufferAngularMomentumChanged;
        //private bool writeBufferLinearVelocityChanged;
        //private bool writeBufferAngularVelocityChanged;
        //private bool writeBufferWritten;

        private List<EventStorageControllerCreated> eventStorageControllersCreated;
        private List<EventStorageControllerRemoved> eventStorageControllersRemoved;
        private List<EventStorageContactCreated> eventStorageContactsCreated;
        private List<EventStorageContactRemoved> eventStorageContactsRemoved;
        private List<EventStorageInitialCollisionDetected> eventStorageInitialCollisionsDetected;
        private List<EventStorageCollisionEnded> eventStorageCollisionsEnded;
        private bool eventStorageEntityUpdated;

        // Events
        public event EventHandlerControllerCreated eventControllerCreated;
        public event EventHandlerControllerCreatedImmediate eventControllerCreatedImmediate;
        public event EventHandlerControllerRemoved eventControllerRemoved;
        public event EventHandlerContactCreated eventContactCreated;
        public event EventHandlerContactCreatedImmediate eventContactCreatedImmediate;
        public event EventHandlerContactRemoved eventContactRemoved;
        public event EventHandlerInitialCollisionDetected eventInitialCollisionDetected;
        public event EventHandlerCollisionEnded eventCollisionEnded;
        public event EventHandlerEntityUpdated eventEntityUpdated;
        public event EventHandlerEntityUpdatedImmediate eventEntityUpdatedImmediate;

        // Constructors
        public PhysicsBody(Keystone.Entities.Entity entity)
            : this()
        {
            if (entity == null) throw new ArgumentNullException();
            _entity = entity;
        }

        private PhysicsBody()
        {
            //lockerCenterPosition = new object();
            //lockerCenterOfMassOffset = new object();
            //lockerOrientationQuaternion = new object();
            //lockerAngularMomentum = new object();
            //lockerAngularVelocity = new object();
            //lockerLinearMomentum = new object();
            //lockerLinearVelocity = new object();
            //lockerGeneralWriteBuffer = new object();

            //myOrientationMatrix = Matrix.Identity();
            //myOrientationQuaternion = Quaternion.Identity();
            // obsolete?    myLinearVelocity.ZeroVector();
            // obsolete? myAngularVelocity.ZeroVector();

            controllers = new List<Controller>();
            constraints = new List<Constraint>();
            nonCollidableEntities = new List<PhysicsBody>();
            forces = new List<Force>();

            myInternalOrientationMatrix = Matrix.Identity();
            myInternalOrientationQuaternion = Quaternion.Identity();
            myInternalLinearVelocity.ZeroVector();
            myInternalAngularVelocity.ZeroVector();
            allowInterpolation = true;
            myDynamicFriction = 0.30F;
            myStaticFriction = 0.60F;
            myIsTangible = true;
            isAffectedByGravity = true;
            myIsActive = true;

            collisionFilter = ulong.MaxValue;
            force.ZeroVector();
            torque.ZeroVector();
            angularDamping = 0.06F;
            
            eventStorageControllersCreated = new List<EventStorageControllerCreated>();
            eventStorageControllersRemoved = new List<EventStorageControllerRemoved>();
            eventStorageContactsCreated = new List<EventStorageContactCreated>();
            eventStorageContactsRemoved = new List<EventStorageContactRemoved>();
            eventStorageInitialCollisionsDetected = new List<EventStorageInitialCollisionDetected>();
            eventStorageCollisionsEnded = new List<EventStorageCollisionEnded>();
        }

        // Properties
        private PhysicsBody getParent()
        {
            if (isSubBodyOfCompound)
            {
                return compoundBody.myParent;
            }
            return this;
        }

        public PhysicsBody parent
        {
            get { return myParent; }
        }

        public Keystone.Entities.Entity Entity { get { return _entity; } }

        public Space space
        {
            get { return mySpace; }
            set
            {
                mySpace = value;
                if (this is CompoundBody && ((CompoundBody)this).subBodies != null)
                {
                    foreach (PhysicsBody entity1 in ((CompoundBody)this).subBodies)
                    {
                        entity1.space = value;
                    }
                }
            }
        }

        //internal Vector3d myInternalCenterPosition;
        internal Vector3d myInternalCenterPosition
        {
            get { return CollisionPrimitive.CenterPosition; } // CollisionPrimitive.BoundingBox.Center; }
        }

        //public Vector3d internalCenterPosition
        //{
        //    get{return myInternalCenterPosition;}
        //    set{move(value - myInternalCenterPosition);}
        //}
        //public Vector3d centerPosition
        //{
        //    get { return myCenterPosition; }
        //    set
        //    {
        //        lock (lockerCenterPosition)
        //        {
        //            writeBufferCenterPosition = value;
        //            writeBufferCenterPositionChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //        }
        //    }
        //}

        public Vector3d internalCenterOfMass
        {
            get { return myInternalCenterOfMass; }
            set
            {
                myInternalCenterOfMass = value;
                value = myInternalCenterOfMass - myInternalCenterPosition;
                Vector3d.TransformCoord(value, Quaternion.Conjugate(myInternalOrientationQuaternion));
                internalCenterOfMassOffset = value;
            }
        }

        //public Vector3d centerOfMass
        //{
        //    get { return myCenterOfMass; }
        //    set
        //    {
        //        Vector3d vector1 = centerOfMass - centerPosition;
        //        vector1 = Vector3d.TransformCoord(vector1, Quaternion.Conjugate(myInternalOrientationQuaternion));
        //        lock (lockerCenterOfMassOffset)
        //        {
        //            writeBufferCenterOfMassOffset = vector1;
        //            writeBufferCenterOfMassOffsetChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //        }
        //    }
        //}

        public Vector3d internalCenterOfMassOffset // this is an OFFSET so it's relative to model's bounding box center
        {
            get { return myCenterOfMassOffset; }
            set
            {
                myCenterOfMassOffset = value;  
                value = Vector3d.TransformCoord(value, myInternalOrientationMatrix); // matrix without position 
                myInternalCenterOfMass = myInternalCenterPosition + value;  // translated to final world position for myInternalCenterOfMass
            }
        }

        //public Vector3d centerOfMassOffset
        //{
        //    get { return myCenterOfMassOffset; }
        //    set
        //    {
        //        lock (lockerCenterOfMassOffset)
        //        {
        //            writeBufferCenterOfMassOffset = value;
        //            writeBufferCenterOfMassOffsetChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //        }
        //    }
        //}

        public Quaternion internalOrientationQuaternion
        {
            get { return myInternalOrientationQuaternion; }
            set { applyQuaternion(value); }
        }

        //public Quaternion orientationQuaternion
        //{
        //    get { return myOrientationQuaternion; }
        //    set
        //    {
        //        lock (lockerOrientationQuaternion)
        //        {
        //            writeBufferOrientationQuaternion = value;
        //            writeBufferOrientationQuaternionChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //        }
        //    }
        //}

        public Matrix internalOrientationMatrix
        {
            get { return myInternalOrientationMatrix; }
            set { applyQuaternion(new Quaternion(value)); }
        }

        //public Matrix orientationMatrix
        //{
        //    get { return myOrientationMatrix; }
        //    set
        //    {
        //        lock (lockerOrientationQuaternion)
        //        {
        //            writeBufferOrientationQuaternion = new Quaternion(value);
        //            writeBufferOrientationQuaternionChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //        }
        //    }
        //}

        //public Vector3d linearVelocity
        //{
        //    get { return myLinearVelocity; }
        //    set
        //    {
        //        lock (lockerLinearVelocity)
        //        {
        //            writeBufferLinearVelocity = value;
        //            writeBufferLinearVelocityChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //            if (!myIsActive)
        //            {
        //                activate();
        //            }
        //        }
        //    }
        //}

        public Vector3d internalLinearVelocity
        {
            get { return myInternalLinearVelocity; }
            set
            {
                if (isPhysicallySimulated)
                {
                    myInternalLinearMomentum = value * mass;
                    myInternalLinearVelocity = value;
                }
                else
                {
                    myInternalLinearVelocity = value;
                }
                if (!myIsActive)
                {
                    activate();
                }
            }
        }

        // internal void setLinearVelocity(ref Vector3d v)
        //{
        //    if (isPhysicallySimulated)
        //    {
        //        myInternalLinearMomentum = v*mass;
        //        myInternalLinearVelocity = v;
        //    }
        //    else
        //    {
        //        myInternalLinearVelocity = v;
        //    }
        //}

        //public Vector3d angularVelocity
        //{
        //    get { return myAngularVelocity; }
        //    set
        //    {
        //        lock (lockerAngularVelocity)
        //        {
        //            writeBufferAngularVelocity = value;
        //            writeBufferAngularVelocityChanged = true;
        //            lock (lockerGeneralWriteBuffer)
        //            {
        //                writeBufferWritten = true;
        //            }
        //            if (!myIsActive)
        //            {
        //                activate();
        //            }
        //        }
        //    }
        //}

        public Vector3d internalAngularVelocity
        {
            get { return myInternalAngularVelocity; }
            set
            {
                setAngularVelocity(ref value);
                if (!myIsActive)
                {
                    activate();
                }
            }
        }

        //public Vector3d linearMomentum
        //{
        //    get { return myLinearMomentum; }
        //    set
        //    {
        //        if (isPhysicallySimulated)
        //        {
        //            lock (lockerLinearMomentum)
        //            {
        //                writeBufferLinearMomentum = value;
        //                writeBufferLinearMomentumChanged = true;
        //                lock (lockerGeneralWriteBuffer)
        //                {
        //                    writeBufferWritten = true;
        //                }
        //                if (!myIsActive)
        //                {
        //                    activate();
        //                }
        //            }
        //        }
        //    }
        //}

        public Vector3d internalLinearMomentum
        {
            get { return myInternalLinearMomentum; }
            set
            {
                if (isPhysicallySimulated)
                {
                    applyLinearImpulse(new Vector3d(value.x - myInternalLinearMomentum.x,
                                                    value.y - myInternalLinearMomentum.y,
                                                    value.z - myInternalLinearMomentum.z));
                    if (!myIsActive)
                    {
                        activate();
                    }
                }
            }
        }

        //public Vector3d angularMomentum
        //{
        //    get { return myAngularMomentum; }
        //    set
        //    {
        //        if (isPhysicallySimulated)
        //        {
        //            lock (lockerAngularMomentum)
        //            {
        //                writeBufferAngularMomentum = value;
        //                writeBufferAngularMomentumChanged = true;
        //                lock (lockerGeneralWriteBuffer)
        //                {
        //                    writeBufferWritten = true;
        //                }
        //                if (!myIsActive)
        //                {
        //                    activate();
        //                }
        //            }
        //        }
        //    }
        //}

        public Vector3d internalAngularMomentum
        {
            get { return myInternalAngularMomentum; }
            set
            {
                if (isPhysicallySimulated)
                {
                    applyAngularImpulse(new Vector3d(value.x - myInternalAngularMomentum.x,
                                                     value.y - myInternalAngularMomentum.y,
                                                     value.z - myInternalAngularMomentum.z));
                    if (!myIsActive)
                    {
                        activate();
                    }
                }
            }
        }

        internal void setAngularVelocity(ref Vector3d v)
        {
            if (isPhysicallySimulated)
            {
                myInternalAngularVelocity = v;
                myInternalAngularMomentum = Vector3d.TransformCoord(myInternalAngularVelocity, internalInertiaTensor);
            }
            else
            {
                myInternalAngularVelocity = v;
            }
        }

        public float collisionMargin
        {
            get { return CollisionPrimitive.myCollisionMargin; }
            set
            {
                CollisionPrimitive.myCollisionMargin = value;
                CollisionPrimitive.myCollisionMarginSquared = value * value;
                if (space != null)
                {
                    lock (space.lockerControllers)
                    {
                        foreach (Controller controller1 in controllers)
                        {
                            controller1.updateMargin();
                        }
                    }
                }
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.collisionMargin = value;
                    }
                }
            }
        }

        public float allowedPenetration
        {
            get { return CollisionPrimitive.myAllowedPenetration; }
            set
            {
                CollisionPrimitive.myAllowedPenetration = value;
                if (space != null)
                {
                    lock (space.lockerControllers)
                    {
                        foreach (Controller controller1 in controllers)
                        {
                            controller1.updateMargin();
                        }
                    }
                }
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.allowedPenetration = value;
                    }
                }
            }
        }

        public float bounciness
        {
            get { return myBounciness; }
            set
            {
                myBounciness = value;
                if (space != null)
                {
                    lock (space.lockerControllers)
                    {
                        foreach (Controller controller1 in controllers)
                        {
                            controller1.updateBounciness();
                        }
                    }
                }
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.bounciness = value;
                    }
                }
            }
        }

        public float staticFriction
        {
            get { return myStaticFriction; }
            set
            {
                myStaticFriction = value;
                if (space != null)
                {
                    lock (space.lockerControllers)
                    {
                        foreach (Controller controller1 in controllers)
                        {
                            controller1.updateFriction();
                        }
                    }
                }
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.staticFriction = value;
                    }
                }
            }
        }

        public float dynamicFriction
        {
            get { return myDynamicFriction; }
            set
            {
                myDynamicFriction = value;
                if (space != null)
                {
                    lock (space.lockerControllers)
                    {
                        foreach (Controller controller1 in controllers)
                        {
                            controller1.updateFriction();
                        }
                    }
                }
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.dynamicFriction = value;
                    }
                }
            }
        }

        public Vector3d totalForce
        {
            get { return force; }
        }

        public Vector3d totalTorque
        {
            get { return torque; }
        }

        public Matrix localSpaceInertiaTensor
        {
            get { return localInertiaTensor; }
            set
            {
                localInertiaTensor = value;
                if (double.IsInfinity(value.M11))
                {
                    localInertiaTensor.M11 = Math.Sign(value.M11) * 0x98967f;
                }
                if (double.IsInfinity(value.M12))
                {
                    localInertiaTensor.M12 = Math.Sign(value.M12) * 0x98967f;
                }
                if (double.IsInfinity(value.M13))
                {
                    localInertiaTensor.M13 = Math.Sign(value.M13) * 0x98967f;
                }
                if (double.IsInfinity(value.M21))
                {
                    localInertiaTensor.M21 = Math.Sign(value.M21) * 0x98967f;
                }
                if (double.IsInfinity(value.M22))
                {
                    localInertiaTensor.M22 = Math.Sign(value.M22) * 0x98967f;
                }
                if (double.IsInfinity(value.M23))
                {
                    localInertiaTensor.M23 = Math.Sign(value.M23) * 0x98967f;
                }
                if (double.IsInfinity(value.M31))
                {
                    localInertiaTensor.M31 = Math.Sign(value.M31) * 0x98967f;
                }
                if (double.IsInfinity(value.M32))
                {
                    localInertiaTensor.M32 = Math.Sign(value.M32) * 0x98967f;
                }
                if (double.IsInfinity(value.M33))
                {
                    localInertiaTensor.M33 = Math.Sign(value.M33) * 0x98967f;
                }
                localInertiaTensorInverse = Matrix.Inverse(localInertiaTensor);
                if (localInertiaTensorInverse.M11 < 0.00d)
                {
                    localInertiaTensorInverse.M11 = 0.00d;
                }
                if (localInertiaTensorInverse.M12 < 0.00d)
                {
                    localInertiaTensorInverse.M12 = 0.00d;
                }
                if (localInertiaTensorInverse.M13 < 0.00d)
                {
                    localInertiaTensorInverse.M13 = 0.00d;
                }
                if (localInertiaTensorInverse.M21 < 0.00d)
                {
                    localInertiaTensorInverse.M21 = 0.00d;
                }
                if (localInertiaTensorInverse.M22 < 0.00d)
                {
                    localInertiaTensorInverse.M22 = 0.00d;
                }
                if (localInertiaTensorInverse.M23 < 0.00d)
                {
                    localInertiaTensorInverse.M23 = 0.00d;
                }
                if (localInertiaTensorInverse.M31 < 0.00d)
                {
                    localInertiaTensorInverse.M31 = 0.00d;
                }
                if (localInertiaTensorInverse.M32 < 0.00d)
                {
                    localInertiaTensorInverse.M32 = 0.00d;
                }
                if (localInertiaTensorInverse.M33 < 0.00d)
                {
                    localInertiaTensorInverse.M33 = 0.00d;
                }
            }
        }

        public Matrix localSpaceInertiaTensorInverse
        {
            get { return localInertiaTensorInverse; }
            set
            {
                value.M44 = 1.00F;
                localInertiaTensorInverse = value;
                if (0.00d == value.M11)
                {
                    localInertiaTensorInverse.M11 = 0.00d;
                }
                if (0.00d == value.M22)
                {
                    localInertiaTensorInverse.M22 = 0.00d;
                }
                if (0.00d == value.M33)
                {
                    localInertiaTensorInverse.M33 = 0.00d;
                }
                localInertiaTensor = Matrix.Inverse(localInertiaTensorInverse);
                if (localInertiaTensor.M11 > 9999999.00F)
                {
                    localInertiaTensor.M11 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M12 > 100000000.00F)
                {
                    localInertiaTensor.M12 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M13 > 100000000.00F)
                {
                    localInertiaTensor.M13 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M21 > 9999999.00F)
                {
                    localInertiaTensor.M21 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M22 > 9999999.00F)
                {
                    localInertiaTensor.M22 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M23 > 9999999.00F)
                {
                    localInertiaTensor.M23 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M31 > 9999999.00F)
                {
                    localInertiaTensor.M31 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M32 > 9999999.00F)
                {
                    localInertiaTensor.M32 = 10000000000000000000000.00F;
                }
                if (localInertiaTensor.M33 > 9999999.00F)
                {
                    localInertiaTensor.M33 = 10000000000000000000000.00F;
                }
                localInertiaTensorInverse = value;
            }
        }
        public bool isTangible
        {
            get { return myIsTangible; }
            set
            {
                myIsTangible = value;
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.isTangible = value;
                    }
                }
            }
        }

        public bool isDetector
        {
            get { return myIsDetector; }
            set
            {
                myIsDetector = value;
                CompoundBody body1 = this as CompoundBody;
                if (body1 != null)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.isDetector = value;
                    }
                }
            }
        }

        public bool isActive
        {
            get { return myIsActive; }
            set
            {
                if (value)
                {
                    activate();
                }
                else
                {
                    deactivate();
                }
            }
        }
        public void deactivate()
        {
            if (isSubBodyOfCompound)
            {
                compoundBody.deactivate();
            }
            else
            {
                if (isPhysicallySimulated)
                {
                    if (myIsActive)
                    {
                        myIsActive = false;
                        if (!isDeactivationCandidate && (space != null))
                        {
                            simulationIsland.numDeactivationCandidatesContained++;
                        }
                        isDeactivationCandidate = true;
                    }
                }
                else
                {
                    isDeactivationCandidate = true;
                    myIsActive = false;
                }
            }
        }

        public void activate()
        {
            if (isSubBodyOfCompound)
            {
                compoundBody.activate();
            }
            else
            {
                if (isPhysicallySimulated)
                {
                    if (space != null)
                    {
                        simulationIsland.activate();
                    }
                    else
                    {
                        isDeactivationCandidate = false;
                        myIsActive = true;
                    }
                }
                else
                {
                    isDeactivationCandidate = false;
                    myIsActive = true;
                }
            }
        }
                
        
        // Methods
        public bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin,
                            out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            if (CollisionPrimitive is SpherePrimitive)
            {
                return (this.CollisionPrimitive).rayTest(origin, direction, maximumLength, withMargin, out hitLocation,
                                                         out hitNormal, out t);
            }
            if (!withMargin && (CollisionPrimitive is TrianglePrimitive))
            {
                return (this.CollisionPrimitive).rayTest(origin, direction, maximumLength, out hitLocation,
                                                         out hitNormal, out t);
            }
            return Toolbox.rayCastGJK(origin, direction, maximumLength, this, withMargin, out hitLocation, out hitNormal,
                                      out t);
        }

        internal void initializeNonDynamicData()
        {
            if (isPhysicallySimulated)
            {
                internalLinearVelocity = myInternalLinearMomentum/mass;
                internalAngularVelocity = Vector3d.TransformCoord(myInternalAngularMomentum,
                                                                    internalInertiaTensorInverse);
            }
            internalLinearMomentum.ZeroVector();
            internalAngularMomentum.ZeroVector();
            localInertiaTensor = new Matrix(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                            double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                            double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                            double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                            double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                            double.PositiveInfinity);
            localInertiaTensorInverse = new Matrix(0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d,
                                                   0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
            internalInertiaTensorInverse = new Matrix(0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d,
                                                      0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
            mass = float.PositiveInfinity;
            force.ZeroVector();
            torque.ZeroVector();
            if (isPhysicallySimulated && (space != null))
            {
                space.physObjects.Remove(this);
            }
            isPhysicallySimulated = false;
            myInternalCenterOfMass = myInternalCenterPosition;
            myInternalPreviousCenterOfMass = myInternalCenterOfMass;
            myInternalPreviousOrientationQuaternion = myInternalOrientationQuaternion;
            myParent = this;
        }

        
        internal virtual void initialize(bool physicallySimulated)
        {
            throw new NotImplementedException(); // must override.  Currently only CompoundBody overrides this though.. i'm
            // thinking that a collisionPrimitive should do what it does there? hrm..
        }

        internal void initializePhysicalData()
        {
            if ((space != null) && !isPhysicallySimulated)
            {
                space.physObjects.Add(this);
            }
            internalInertiaTensorInverse = (myInternalOrientationMatrix*localInertiaTensorInverse)*
                                           Matrix.Transpose(myInternalOrientationMatrix);
            myInternalLinearMomentum = myInternalLinearVelocity*mass;
            internalInertiaTensor = localInertiaTensor;
            myInternalAngularMomentum = Vector3d.TransformCoord(myInternalAngularVelocity, internalInertiaTensor); // 
            isPhysicallySimulated = true;
            internalCenterOfMass = myInternalCenterPosition;
            myInternalPreviousCenterOfMass = myInternalCenterOfMass;
            myInternalPreviousOrientationQuaternion = myInternalOrientationQuaternion;
            myParent = this;
        }

        public virtual void makePhysical(float m)
        {
            mass = m;
            Space space1 = mySpace;
            if (space1 != null)
            {
                space1.remove(this);
            }
            initialize(true);
            if (space1 != null)
            {
                space1.add(this);
            }
            activate();
        }

        public virtual void makeNonDynamic()
        {
            Space space1 = mySpace;
            if (space1 != null)
            {
                space1.remove(this);
            }
            initialize(false);
            if (space1 != null)
            {
                space1.add(this);
            }
        }

        public void setCollisionFilter(int x, bool value)
        {
            if (!value)
            {
                ulong num1 = ulong.MaxValue - ((ulong) Math.Pow(2, x));
                collisionFilter &= num1;
            }
            else
            {
                ulong num2 = (ulong) Math.Pow(2, x);
                collisionFilter |= num2;
            }
        }

        internal void applyQuaternion(Quaternion q)
        {
            myInternalOrientationQuaternion = Quaternion.Normalize(q);
            myInternalOrientationMatrix = new Matrix(myInternalOrientationQuaternion);
            // TODO: fix triangle crap
            //if (this.CollisionPrimitive is TrianglePrimitive)
            //{
            //    TrianglePrimitive triangle1 = (TrianglePrimitive)this.CollisionPrimitive;
            //    for (int i = 0; i < triangle1.vertices.Length; i++)
            //    {
            //        ((TrianglePrimitive)this.CollisionPrimitive).vertices[i] = Vector3d.TransformCoord(triangle1.localVertices[i], myInternalOrientationMatrix) + CenterPosition;
            //    }
            //    triangle1.Normal = Vector3d.TransformCoord(triangle1.localNormal, myInternalOrientationMatrix);
            //}
            //else
            //{
            //    if (this is CompoundBody)
            //    {
            //        CompoundBody body1 = (CompoundBody)this;
            //        body1.subCompoundBodyUpdate();
            //    }
            //}
        }

        internal void applyDamping(float dt)
        {
            Vector3d vector1 = myInternalLinearVelocity*
                               Math.Pow(Toolbox.Clamp(1.00d - (linearDamping + linearDampingBoost), 0.00d, 1.00d), dt);

            internalLinearVelocity = vector1;
            
            vector1 = myInternalAngularVelocity*
                      Math.Pow(Toolbox.Clamp(1.00d - (angularDamping + angularDampingBoost), 0.00d, 1.00d), dt);
            setAngularVelocity(ref vector1);
            linearDampingBoost = 0.00f;
            angularDampingBoost = 0.00f;
        }

        public void modifyLinearDamping(float damping)
        {
            float single1 = linearDamping + linearDampingBoost;
            float single2 = 1.00F - single1;
            linearDampingBoost += damping*single2;
        }

        public void modifyAngularDamping(float damping)
        {
            float single1 = angularDamping + angularDampingBoost;
            float single2 = 1.00F - single1;
            angularDampingBoost += damping*single2;
        }
        public void applyLinearImpulse(Vector3d impulse)
        {
            myInternalLinearMomentum += impulse;
            myInternalLinearVelocity = myInternalLinearMomentum * massReciprocal; 
        }

        public void applyAngularImpulse(Vector3d torq)
        {
            myInternalAngularMomentum += torq;
            if (mySpace.simulationSettings.conserveAngularMomentum)
            {
                myInternalAngularVelocity = Vector3d.TransformCoord(myInternalAngularMomentum,
                                                                    internalInertiaTensorInverse);
            }
            else
            {
                Vector3d vector1 = Vector3d.TransformCoord(torque, internalInertiaTensorInverse);
                myInternalAngularVelocity += vector1;
            }
        }

        public void applyImpulse(Vector3d pos, Vector3d direction, bool wakeUp)
        {
            if (isPhysicallySimulated)
            {
                applyLinearImpulse(direction);
                Vector3d torq = Vector3d.CrossProduct((pos - myInternalCenterOfMass), direction);
                applyAngularImpulse(torq);
                if (wakeUp)
                {
                    activate();
                }
            }
        }

        public void applyImpulse(Vector3d pos, Vector3d direction)
        {
            applyImpulse(pos, direction, true);
        }

        internal void applyForces(float dt, float timeScale)
        {
            Vector3d vector2;
            if (forces.Count > 0)
            {
                List<Force> list = ResourcePool.getForceList();
                foreach (Force item in forces)
                {
                    if (item.isTrackingTarget)
                    {
                        item.track();
                    }
                    item.age += dt;
                    if ((item.lifeSpan > 0.00d) && (item.age > item.lifeSpan))
                    {
                        list.Add(item);
                    }
                }
                foreach (Force force2 in list)
                {
                    forces.Remove(force2);
                }
                ResourcePool.giveBack(list);
            }
            Vector3d vector1 = getTotalForce();
            if (isAffectedByGravity)
            {
                vector2 = space.simulationSettings.gravity;
                vector2 *= mass;
                vector2 += vector1;
                vector2 *= dt;
                internalLinearMomentum += vector2;
            }
            else
            {
                vector2 = vector1*dt;
                internalLinearMomentum += vector2;
            }

            Vector3d vector3 = getTotalTorque();
            vector2 = vector3*dt;
            myInternalAngularMomentum += vector2;
            Matrix matrix1 = Matrix.Transpose(myInternalOrientationMatrix);
            Matrix matrix2 = matrix1*localInertiaTensorInverse;
            internalInertiaTensorInverse = matrix2*myInternalOrientationMatrix;
            
            matrix2 = matrix1*localInertiaTensor;
            internalInertiaTensor = matrix2*myInternalOrientationMatrix;
                                                                                                              
            if (mySpace.simulationSettings.conserveAngularMomentum)
            {
                myInternalAngularVelocity = Vector3d.TransformCoord(myInternalAngularMomentum,
                                                                    internalInertiaTensorInverse);
            }
            else
            {
                vector2 = Vector3d.TransformCoord(vector2, internalInertiaTensorInverse);
                myInternalAngularVelocity += vector2;
            }
        }

        public void applyForce(Force f)
        {
            forces.Add(f);
            if (f.target != this)
            {
                f.setTarget(this);
            }
        }

        public void removeForce(Force f)
        {
            forces.Remove(f);
            f.strictlyTarget(null);
        }

        internal Vector3d getTotalForce()
        {
            Vector3d vector1 = new Vector3d(0.00d, 0.00d, 0.00d);
            for (int i = 0; i < forces.Count; i++)
            {
                if (forces[i].isActive)
                {
                    vector1 += forces[i].direction;
                }
            }
            return vector1;
        }

        internal Vector3d getTotalTorque()
        {
            Vector3d vector1 = new Vector3d(0.00d, 0.00d, 0.00d);
            for (int i = 0; i < forces.Count; i++)
            {
                if (forces[i].isActive)
                {
                    vector1 += Vector3d.CrossProduct(forces[i].position - myInternalCenterOfMass, forces[i].direction);
                }
            }
            return vector1;
        }
        public void clearForces()
        {
            forces.Clear();
        }

    
        public void padInertiaTensor()
        {
            double padding;
            if (CollisionPrimitive is CylinderPrimitive)
            {
                CylinderPrimitive cylinder1 = (CylinderPrimitive) CollisionPrimitive;
                padding = mass*((cylinder1.Height/cylinder1.Radius)/2.00F);
            }
            else if (CollisionPrimitive is CapsulePrimitive)
            {
                CapsulePrimitive capsule1 = (CapsulePrimitive) CollisionPrimitive;
                padding = mass*((capsule1.Length/capsule1.Radius)/2.00F);
            }
            else if (CollisionPrimitive is ConePrimitive)
            {
                ConePrimitive cone1 = (ConePrimitive) CollisionPrimitive;
                padding = mass*((cone1.Height/cone1.Radius)/2.00F);
            }
            else
            {
                padding = mass;
            }
            padInertiaTensor(padding);
        }

        public void padInertiaTensor(double padding)
        {
            localInertiaTensor.M11 += padding;
            localInertiaTensor.M22 += padding;
            localInertiaTensor.M33 += padding;
            localInertiaTensorInverse = Matrix.Inverse(localInertiaTensor);
        }

        public void scaleInertiaTensor(float scale)
        {
            localInertiaTensor.M11 *= scale;
            localInertiaTensor.M12 *= scale;
            localInertiaTensor.M13 *= scale;
            localInertiaTensor.M21 *= scale;
            localInertiaTensor.M22 *= scale;
            localInertiaTensor.M23 *= scale;
            localInertiaTensor.M31 *= scale;
            localInertiaTensor.M32 *= scale;
            localInertiaTensor.M33 *= scale;
            localInertiaTensorInverse = Matrix.Inverse(localInertiaTensor); 
        }

        internal void update(float dt, float timeScale)
        {
            update(dt, timeScale, true, false);
        }

        internal void update(float dt, float timeScale, bool translate, bool rotate)
        {
            if (isPhysicallySimulated)
            {
                updateClamping(dt);
                if (this.CollisionPrimitive is TrianglePrimitive)
                {
                    this.moveAndRotate(dt, translate, rotate);
                }
                else
                {
                    moveAndRotate(dt, translate, rotate);
                }
                applyDamping(dt);
                if (this is CompoundBody)
                {
                    ((CompoundBody) this).subCompoundBodyUpdate();
                }
                correctiveLinearVelocity.ZeroVector();
                correctiveAngularVelocity.ZeroVector();
                force = myInternalLinearMomentum - previousLinearMomentum;
                torque = myInternalAngularMomentum - previousAngularMomentum;
                previousLinearMomentum = myInternalLinearMomentum;
                previousAngularMomentum = myInternalAngularMomentum;
            }
            else
            {
                moveAndRotate(dt, translate, rotate);
                if (this is CompoundBody)
                {
                    ((CompoundBody) this).subCompoundBodyUpdate();
                }
            }
            onEntityUpdated();
        }

        internal void updateClamping(float dt)
        {
            if (myInternalLinearVelocity.LengthSquared() <
                (space.simulationSettings.linearVelocityClamping * space.simulationSettings.linearVelocityClamping))
            {
                timeBelowLinearVelocityThreshold += dt;
                if (timeBelowLinearVelocityThreshold > space.simulationSettings.linearVelocityClampingTime)
                {
                    myInternalLinearVelocity.ZeroVector();
                    myInternalLinearMomentum.ZeroVector();
                }
            }
            else
            {
                timeBelowLinearVelocityThreshold = 0.00f;
            }
            if (myInternalAngularVelocity.LengthSquared() <
                (space.simulationSettings.angularVelocityClamping * space.simulationSettings.angularVelocityClamping))
            {
                timeBelowAngularVelocityThreshold += dt;
                if (timeBelowAngularVelocityThreshold <= space.simulationSettings.angularVelocityClampingTime)
                {
                    return;
                }
                myInternalAngularVelocity.ZeroVector();
                myInternalAngularMomentum.ZeroVector();
            }
            else
            {
                timeBelowAngularVelocityThreshold = 0.00f;
            }
        }

        internal void updateActivity(float dt)
        {
            if (!isAlwaysActive)
            {
                if (isPhysicallySimulated && space.simulationSettings.useSplitImpulsePositionCorrection)
                {
                    Vector3d vector1 = myInternalLinearVelocity + correctiveLinearVelocity;
                    Vector3d vector2 = myInternalAngularVelocity + correctiveAngularVelocity;
                    if ((vector1.IsNullOrEmpty()) && (vector2.IsNullOrEmpty()))
                    {
                        if (!isDeactivationCandidate)
                        {
                            simulationIsland.numDeactivationCandidatesContained++;
                        }
                        isDeactivationCandidate = true;
                    }
                    else
                    {
                        if (isDeactivationCandidate)
                        {
                            simulationIsland.numDeactivationCandidatesContained--;
                        }
                        isDeactivationCandidate = false;
                    }
                }
                else if (isPhysicallySimulated)
                {
                    if ((myInternalLinearVelocity.IsNullOrEmpty()) &&
                        (myInternalAngularVelocity.IsNullOrEmpty()))
                    {
                        if (!isDeactivationCandidate)
                        {
                            simulationIsland.numDeactivationCandidatesContained++;
                        }
                        isDeactivationCandidate = true;
                    }
                    else
                    {
                        if (isDeactivationCandidate)
                        {
                            simulationIsland.numDeactivationCandidatesContained--;
                        }
                        isDeactivationCandidate = false;
                    }
                }
                else
                {
                    if ((myInternalLinearVelocity.IsNullOrEmpty()) &&
                        (myInternalAngularVelocity.IsNullOrEmpty()))
                    {
                        isDeactivationCandidate = true;
                        myIsActive = false;
                    }
                    else
                    {
                        isDeactivationCandidate = false;
                        myIsActive = true;
                    }
                }
            }
        }

       internal virtual void interpolateStates(double originalAmount, double finalAmount)
        {
            if (allowInterpolation)
            {
                //myCenterOfMass = ((myInternalPreviousCenterOfMass*originalAmount) + (myInternalCenterOfMass*finalAmount));
                myInternalCenterOfMass = ((myInternalPreviousCenterOfMass * originalAmount) + (myInternalCenterOfMass * finalAmount));
                //myOrientationQuaternion = Quaternion.Slerp(myInternalPreviousOrientationQuaternion,
                //                                           myInternalOrientationQuaternion, finalAmount);
                myInternalOrientationQuaternion = Quaternion.Slerp(myInternalPreviousOrientationQuaternion,
                                                           myInternalOrientationQuaternion, finalAmount);
                //myOrientationMatrix = new Matrix(myOrientationQuaternion);
                myInternalOrientationMatrix = new Matrix(myInternalOrientationQuaternion);
                Vector3d vector1 = myInternalPreviousCenterOfMass -
                                   Vector3d.TransformCoord(myCenterOfMassOffset, myInternalPreviousOrientationQuaternion);
                Vector3d vector2 = myInternalCenterOfMass -
                                   Vector3d.TransformCoord(myCenterOfMassOffset, myInternalOrientationQuaternion);
                //myCenterPosition = ((vector1*originalAmount) + (vector2*finalAmount)); //TODO: obsolete right?
                moveTo( ((vector1 * originalAmount) + (vector2 * finalAmount)));
            }
            else
            {
                //myCenterOfMass = myInternalCenterOfMass;
                //myOrientationQuaternion = myInternalOrientationQuaternion;
                //myOrientationMatrix = myInternalOrientationMatrix;
                //myCenterPosition = myInternalCenterPosition; //TODO: obsolete right?
            }
        }
        public virtual void move(Vector3d translation)
        {
            move(ref translation);
        }

        public virtual void move(ref Vector3d translation)
        {
            if (!isSubBodyOfCompound || (isSubBodyOfCompound && !compoundBody.usePropertyMasks))
            {
                if (!myIsActive && isPhysicallySimulated)
                {
                    activate();
                }
                myInternalCenterOfMass = myInternalCenterOfMass + translation;
                // TODO: here we should be moving the associated Core.Entity (not to be confused with physicsBody)
                // or where is the callback to notify Core.Entity we've moved?
               // CollisionPrimitive.Translate(translation);
                Entity.Translation += translation;
            }
            else
            {
                compoundBody.moveDirect(ref translation);
            }
        }

        public void moveTo(Vector3d v)
        {
            moveTo(v, true);
        }

        public virtual void moveTo(Vector3d v, bool useGeometricCenter)
        {
            Vector3d vector1;
            if (useGeometricCenter)
            {
                vector1 = v - myInternalCenterPosition;
            }
            else
            {
                vector1 = v - myInternalCenterOfMass;
            }
            move(ref vector1);
        }

        internal void moveAndRotate(float dt, bool translate, bool rotate)
        {
            if ((translate && !myInternalLinearVelocity.IsNullOrEmpty())
                              || (space.simulationSettings.useSplitImpulsePositionCorrection
                                  && (!correctiveLinearVelocity.IsNullOrEmpty())))
            {
                Vector3d vector1 = myInternalLinearVelocity + correctiveLinearVelocity;
                vector1 *= dt;
                move(ref vector1);
            }
            if (rotate && !myInternalAngularVelocity.IsNullOrEmpty() || !correctiveAngularVelocity.IsNullOrEmpty())
            {
                if (space.simulationSettings.useRK4AngularIntegration)
                {
                    if (!space.simulationSettings.useSplitImpulsePositionCorrection)
                    {
                        Toolbox.updateOrientationRK4(ref myInternalOrientationQuaternion, ref localInertiaTensorInverse,
                                                     ref myInternalAngularMomentum, dt,
                                                     out myInternalOrientationQuaternion);
                    }
                    else
                    {
                        Vector3d angularmomentum = correctiveAngularVelocity*dt;
                        angularmomentum = Vector3d.TransformCoord(angularmomentum, internalInertiaTensor);
                        Vector3d combinedAngularMomentum = myInternalAngularMomentum + angularmomentum;
                        if ((double.IsNaN(combinedAngularMomentum.x) || double.IsNaN(combinedAngularMomentum.y)) || double.IsNaN(combinedAngularMomentum.z))
                        {
                            combinedAngularMomentum = myInternalAngularMomentum;
                        }
                        Toolbox.updateOrientationRK4(ref myInternalOrientationQuaternion, ref localInertiaTensorInverse,
                                                     ref combinedAngularMomentum, dt, out myInternalOrientationQuaternion);
                    }
                }
                else
                {
                    Vector3d angularVelocity = myInternalAngularVelocity + correctiveAngularVelocity;
                    angularVelocity *= dt * 0.50D;
                    Quaternion quaternion1 = new Quaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0.00d);
                    quaternion1 *= myInternalOrientationQuaternion;
                    myInternalOrientationQuaternion = myInternalOrientationQuaternion + quaternion1;
                    myInternalOrientationQuaternion = Quaternion.Normalize(myInternalOrientationQuaternion);
                }
                myInternalOrientationMatrix = new Matrix(myInternalOrientationQuaternion);
            
                Vector3d vector5 = -myCenterOfMassOffset;
                    // offset of center of mass with respect to center of bounding box
                vector5 = Vector3d.TransformCoord(vector5, myInternalOrientationMatrix);
                // TODO: this looks wrong.  why the hell is the geometric enter using centerofmass?
                // TODO: here we should be moving the associated Core.Entity (not to be confused with physicsBody)
                Entity.Translation = vector5 + myInternalCenterPosition;// myInternalCenterOfMass; 
                //throw new Exception("below line should not be commetned out");
              //  CollisionPrimitive.Move (vector5 + myInternalCenterOfMass); 
            }
        }
        public virtual void rotate(Vector3d w)
        {
            myInternalOrientationQuaternion += ((new Quaternion(w, 0.00d)*0.50F)*myInternalOrientationQuaternion);
            myInternalOrientationQuaternion = Quaternion.Normalize(myInternalOrientationQuaternion);
            myInternalOrientationMatrix = new Matrix(myInternalOrientationQuaternion);
        }
        

        #region events
        private void addToEventfuls()
        {
            isEventful = true;
            if ((space != null) && !space.eventfulEntities.Contains(this))
            {
                space.eventfulEntities.Add(this);
            }
        }

        public void addEventHook(EventHandlerControllerCreated eventHandler)
        {
            eventControllerCreated += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerControllerCreatedImmediate eventHandler)
        {
            eventControllerCreatedImmediate += eventHandler;
        }

        public void addEventHook(EventHandlerControllerRemoved eventHandler)
        {
            eventControllerRemoved += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerContactCreated eventHandler)
        {
            eventContactCreated += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerContactCreatedImmediate eventHandler)
        {
            eventContactCreatedImmediate += eventHandler;
        }

        public void addEventHook(EventHandlerContactRemoved eventHandler)
        {
            eventContactRemoved += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerInitialCollisionDetected eventHandler)
        {
            eventInitialCollisionDetected += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerCollisionEnded eventHandler)
        {
            eventCollisionEnded += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerEntityUpdated eventHandler)
        {
            eventEntityUpdated += eventHandler;
            addToEventfuls();
        }

        public void addEventHook(EventHandlerEntityUpdatedImmediate eventHandler)
        {
            eventEntityUpdatedImmediate += eventHandler;
        }

        public void removeEventHook(EventHandlerControllerCreated eventHandler)
        {
            eventControllerCreated -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerControllerCreatedImmediate eventHandler)
        {
            eventControllerCreatedImmediate -= eventHandler;
        }

        public void removeEventHook(EventHandlerControllerRemoved eventHandler)
        {
            eventControllerRemoved -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerContactCreated eventHandler)
        {
            eventContactCreated -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerContactCreatedImmediate eventHandler)
        {
            eventContactCreatedImmediate -= eventHandler;
        }

        public void removeEventHook(EventHandlerContactRemoved eventHandler)
        {
            eventContactRemoved -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerInitialCollisionDetected eventHandler)
        {
            eventInitialCollisionDetected -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerCollisionEnded eventHandler)
        {
            eventCollisionEnded -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerEntityUpdated eventHandler)
        {
            eventEntityUpdated -= eventHandler;
            verifyEventStatus();
        }

        public void removeEventHook(EventHandlerEntityUpdatedImmediate eventHandler)
        {
            eventEntityUpdatedImmediate -= eventHandler;
            verifyEventStatus();
        }

        private void verifyEventStatus()
        {
            if ((((eventControllerCreated == null) && (eventControllerRemoved == null)) &&
                 ((eventContactCreated == null) && (eventContactRemoved == null))) &&
                (((eventCollisionEnded == null) && (eventInitialCollisionDetected == null)) &&
                 (eventEntityUpdated == null)))
            {
                space.eventfulEntities.Remove(this);
                isEventful = false;
            }
        }

        public void removeAllEvents()
        {
            if (isEventful)
            {
                space.eventfulEntities.Remove(this);
            }
            isEventful = false;
            eventControllerCreated = null;
            eventControllerCreatedImmediate = null;
            eventControllerRemoved = null;
            eventContactCreated = null;
            eventContactCreatedImmediate = null;
            eventContactRemoved = null;
            eventInitialCollisionDetected = null;
            eventCollisionEnded = null;
            eventEntityUpdated = null;
            eventEntityUpdatedImmediate = null;
        }

        internal void onControllerCreated(Controller controller)
        {
            if (eventControllerCreated != null)
            {
                eventStorageControllersCreated.Add(new EventStorageControllerCreated(controller));
            }
            if (eventControllerCreatedImmediate != null)
            {
                eventControllerCreatedImmediate(this, controller);
            }
        }

        internal void onControllerRemoved(PhysicsBody a, PhysicsBody b)
        {
            if (eventControllerRemoved != null)
            {
                PhysicsBody entity1 = a == this ? b : a;
                eventStorageControllersRemoved.Add(new EventStorageControllerRemoved(entity1));
            }
        }

        internal void onContactCreated(Controller controller, Contact contact)
        {
            if (eventContactCreated != null)
            {
                eventStorageContactsCreated.Add(new EventStorageContactCreated(controller, contact.position,
                                                                               contact.normal, contact.penetrationDepth));
            }
            if (eventContactCreatedImmediate != null)
            {
                eventContactCreatedImmediate(this, controller, contact);
            }
        }

        internal void onContactRemoved(Controller controller, Vector3d position, Vector3d normal, double depth)
        {
            if (eventContactRemoved != null)
            {
                eventStorageContactsRemoved.Add(new EventStorageContactRemoved(controller, position, normal, depth));
            }
        }

        internal void onInitialCollisionDetected(Controller controller)
        {
            if (eventInitialCollisionDetected != null)
            {
                eventStorageInitialCollisionsDetected.Add(new EventStorageInitialCollisionDetected(controller));
            }
        }

        internal void onCollisionEnded(Controller controller)
        {
            if (eventCollisionEnded != null)
            {
                eventStorageCollisionsEnded.Add(new EventStorageCollisionEnded(controller));
            }
        }

        internal void onEntityUpdated()
        {
            if (eventEntityUpdated != null)
            {
                eventStorageEntityUpdated = true;
            }
            if (eventEntityUpdatedImmediate != null)
            {
                eventEntityUpdatedImmediate(this);
            }
        }


        internal void dispatchEvents()
        {
            foreach (EventStorageControllerCreated created1 in eventStorageControllersCreated)
            {
                eventControllerCreated(this, created1.controller);
            }
            foreach (EventStorageControllerRemoved removed1 in eventStorageControllersRemoved)
            {
                eventControllerRemoved(this, removed1.otherPhysicsBody);
            }
            foreach (EventStorageContactCreated created2 in eventStorageContactsCreated)
            {
                eventContactCreated(this, created2.controller, created2.position, created2.normal, created2.depth);
            }
            foreach (EventStorageInitialCollisionDetected detected1 in eventStorageInitialCollisionsDetected)
            {
                eventInitialCollisionDetected(this, detected1.controller);
            }
            foreach (EventStorageContactRemoved removed2 in eventStorageContactsRemoved)
            {
                eventContactRemoved(this, removed2.controller, removed2.position, removed2.normal, removed2.depth);
            }
            foreach (EventStorageCollisionEnded ended1 in eventStorageCollisionsEnded)
            {
                eventCollisionEnded(this, ended1.controller);
            }
            if (eventStorageEntityUpdated)
            {
                eventEntityUpdated(this);
            }
            eventStorageControllersCreated.Clear();
            eventStorageControllersRemoved.Clear();
            eventStorageContactsCreated.Clear();
            eventStorageContactsRemoved.Clear();
            eventStorageInitialCollisionsDetected.Clear();
            eventStorageCollisionsEnded.Clear();
            eventStorageEntityUpdated = false;
        }
#endregion

        #region buffered updates
        internal virtual void updateBufferedStates()
        {
            //myCenterPosition = myInternalCenterPosition;
            //myCenterOfMass = myInternalCenterOfMass;
            //myOrientationQuaternion = myInternalOrientationQuaternion;
            //myOrientationMatrix = myInternalOrientationMatrix;
        // obsolete?    myLinearMomentum = myInternalLinearMomentum;
            // obsolete?     myAngularMomentum = myInternalAngularMomentum;
            // obsolete?      myLinearVelocity = myInternalLinearVelocity;
            // obsolete?    myAngularVelocity = myInternalAngularVelocity;
        }

        //internal virtual void writeBufferedStates()
        //{
        //    if (writeBufferWritten)
        //    {
        //        if (writeBufferCenterPositionChanged)
        //        {
        //            moveTo(writeBufferCenterPosition);
        //            lock (lockerCenterPosition)
        //            {
        //                writeBufferCenterPositionChanged = false;
        //            }
        //        }
        //        if (writeBufferOrientationQuaternionChanged)
        //        {
        //            applyQuaternion(writeBufferOrientationQuaternion);
        //            lock (lockerOrientationQuaternion)
        //            {
        //                writeBufferOrientationQuaternionChanged = false;
        //            }
        //        }
        //        if (writeBufferCenterOfMassOffsetChanged)

        //            myCenterOfMassOffset = writeBufferCenterOfMassOffset;
        //        Vector3d vector1 = Vector3d.TransformCoord(writeBufferCenterOfMassOffset, myInternalOrientationMatrix);
        //        myInternalCenterOfMass = myInternalCenterPosition + vector1;
        //        lock (lockerCenterOfMassOffset)
        //        {
        //            writeBufferCenterOfMassOffsetChanged = false;
        //        }
        //    }
        //    if (writeBufferLinearMomentumChanged)
        //    {
        //        applyLinearImpulse(new Vector3d(writeBufferLinearMomentum.x - myInternalLinearMomentum.x,
        //                                        writeBufferLinearMomentum.y - myInternalLinearMomentum.y,
        //                                        writeBufferLinearMomentum.z - myInternalLinearMomentum.z));
        //        lock (lockerLinearMomentum)
        //        {
        //            writeBufferLinearMomentumChanged = false;
        //        }
        //    }
        //    if (writeBufferLinearVelocityChanged)
        //    {
        //        internalLinearVelocity = writeBufferLinearVelocity;
        //        lock (lockerLinearVelocity)
        //        {
        //            writeBufferLinearVelocityChanged = false;
        //        }
        //    }
        //    if (writeBufferAngularMomentumChanged)
        //    {
        //        applyAngularImpulse(new Vector3d(writeBufferAngularMomentum.x - myInternalAngularMomentum.x,
        //                                         writeBufferAngularMomentum.y - myInternalAngularMomentum.y,
        //                                         writeBufferAngularMomentum.z - myInternalAngularMomentum.z));
        //        lock (lockerAngularMomentum)
        //        {
        //            writeBufferAngularMomentumChanged = false;
        //        }
        //    }
        //    if (writeBufferAngularVelocityChanged)
        //    {
        //        setAngularVelocity(ref writeBufferAngularVelocity);
        //        lock (lockerAngularVelocity)
        //        {
        //            writeBufferAngularVelocityChanged = false;
        //        }
        //    }
        //    lock (lockerGeneralWriteBuffer)
        //    {
        //        writeBufferWritten = false;
        //    }
        //}
        #endregion
    }
}