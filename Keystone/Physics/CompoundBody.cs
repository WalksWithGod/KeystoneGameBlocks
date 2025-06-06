using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Physics.Primitives;

namespace Keystone.Physics
{
    /// <summary>
    /// A compound body represents a single entity that has a shape that is better represented by a set of
    /// primitives rather than a single one.  For instance, a weightlifting dumbbell is a single object
    /// but would be best represented by two spheres and a rectangular bounding box for the bar. So the three
    /// primitives work together as a single physics body.
    /// </summary>
    public class CompoundBody : PhysicsBody
    {
        // Instance Fields
        internal bool usePropertyMasks;
        public List<PhysicsBody> subBodies;
        public List<Vector3d> subBodyLocalOffsets;
        public List<Quaternion> subBodyLocalRotations;
        internal int numChildren;
        internal int numEntities;
        private CompoundBodyBVHNode root;
        internal List<PhysicsBody> allChildren;

        // Constructors
        public CompoundBody(Keystone.Entities.Entity entity)
            : base(entity)
        {
            usePropertyMasks = true;
            allChildren = new List<PhysicsBody>();
            subBodies = new List<PhysicsBody>();
            numChildren = 0;
            numEntities = 1;
        }

        public CompoundBody(Keystone.Entities.Entity entity, List<PhysicsBody> bodies)
            : base(entity)
        {
            usePropertyMasks = true;
            allChildren = new List<PhysicsBody>();
            subBodies = bodies;
            foreach (PhysicsBody entity1 in bodies)
            {
                entity1.compoundBody = this;
                entity1.isSubBodyOfCompound = true;
            }
            initialize(true);

            // TODO: uncomment here and in Initialize
            //myMaximumRadius = 0.00d;
            //for (int i = 0; i < subBodies.Count; i++)
            //{
            //    // add up the maximum radius to find compound bodie's maximum radius
            //    // TODO: i dont know why he didnt just compute this from it's combined bounding volume.  
            //    // wtf is up with this guy?
            //    Vector3d vector1 = subBodies[i].myInternalCenterPosition - myInternalCenterPosition;
            //    double single1 = vector1.Length + subBodies[i].myMaximumRadius;
            //    if (myMaximumRadius < single1)
            //    {
            //        myMaximumRadius = single1;
            //    }
            //}
            internalInertiaTensorInverse = Matrix.Identity();
            updateBufferedStates();
            countNumberOfCollidableChildren();
            countEntities();
            createBoundingVolumeHierarchy();
            setParents();
        }


        // Methods
        private void setParents()
        {
            List<PhysicsBody> children = ResourcePool.getEntityList();
            getAllRealChildren(children);
            foreach (PhysicsBody entity1 in children)
            {
                entity1.myParent = this;
            }
            ResourcePool.giveBack(children);
        }

        internal override void initialize(bool physicallySimulated)
        {
            subBodyLocalOffsets = new List<Vector3d>();
            subBodyLocalRotations = new List<Quaternion>();
            usePropertyMasks = false;
            CollisionPrimitive.findBoundingBox(0.00f);
            Vector3d vector1 = new Vector3d();
            Vector3d vector2 = new Vector3d( );
            mass = 0.00f;
            volume = 0.00f;
            for (int i = 0; i < subBodies.Count; i++)
            {
                mass += subBodies[i].mass;
                volume += subBodies[i].volume;
                // translate the center of mass by the position and scale of the mass of the subBody?
                vector2 += (subBodies[i].internalCenterOfMass * subBodies[i].mass);
                // weighted position that is basically geometric center scaled by mass?
                vector1 += (subBodies[i].myInternalCenterPosition*subBodies[i].mass); // wtf?  why isn't this taking the center of the combined bounding volume's center?
            }
            
            // TODO: wtf?  why are we computing the geometric center this way? surely since mass can be anything regardless of
            // the physical size of the bounding volume, this is no reliable way to get the physical center
        //    myInternalCenterPosition = vector1/mass;
            internalCenterOfMass = vector2 / mass;
            internalCenterOfMassOffset = internalCenterOfMass - myInternalCenterPosition;
            localInertiaTensor = Toolbox.zeroMatrix;
            Matrix matrix1 = Matrix.Transpose(myInternalOrientationMatrix);
            for (int j = 0; j < subBodies.Count; j++)
            {
                Vector3d a = Vector3d.TransformCoord(subBodies[j].internalCenterOfMass - internalCenterOfMass, matrix1);
                subBodyLocalOffsets.Add(
                    Vector3d.TransformCoord(myInternalCenterPosition - subBodies[j].myInternalCenterPosition, matrix1));
                subBodyLocalRotations.Add(Quaternion.Conjugate(myInternalOrientationQuaternion)*
                                               subBodies[j].myInternalOrientationQuaternion);
                Matrix matrix2 = new Matrix(subBodyLocalRotations[j]);
                Matrix matrix3 = Matrix.Inverse(matrix2);
                localInertiaTensor += ((matrix3*subBodies[j].localInertiaTensor)*matrix2) +
                                           (((Matrix.Identity() * Vector3d.DotProduct(a, a)) - Toolbox.getOuterProduct(a, a))*subBodies[j].mass);
            }
            localInertiaTensorInverse = Matrix.Inverse(localInertiaTensor);

            // TODO: why is this not simply taking the hypotenus of the combined volume?
            //myMaximumRadius = 0.00f;
            //for (int z = 0; z < subBodies.Count; z++)
            //{
            //    Vector3d vector4 = subBodyLocalOffsets[z];
            //    double single1 = vector4.Length + subBodies[z].myMaximumRadius;
            //    if (single1 > myMaximumRadius)
            //    {
            //        myMaximumRadius = single1;
            //    }
            //}
            density = mass/volume;
            if (physicallySimulated)
            {
                initializePhysicalData();
            }
            else
            {
                initializeNonDynamicData();
            }
        }

        internal void makeChildrenNonDynamic()
        {
            foreach (PhysicsBody entity1 in subBodies)
            {
                CompoundBody body1 = entity1 as CompoundBody;
                if (body1 != null)
                {
                    body1.makeChildrenNonDynamic();
                }
                entity1.initialize(false);
                entity1.myParent = this;
            }
        }

        internal void makeChildrenDynamic(float newMass)
        {
            foreach (PhysicsBody entity1 in subBodies)
            {
                CompoundBody body1 = entity1 as CompoundBody;
                if (body1 != null)
                {
                    body1.makeChildrenDynamic(newMass/(subBodies.Count));
                }
                entity1.mass = newMass/(subBodies.Count);
                entity1.initialize(true);
                entity1.myParent = this;
            }
        }

        public override void makePhysical(float m)
        {
            bool flag1 = isPhysicallySimulated;
            if (!isPhysicallySimulated)
            {
                makeChildrenDynamic(m);
            }
            makePhysical(m);
            if (flag1)
            {
                double single1 = m/mass;
                localSpaceInertiaTensor *= single1;
            }
        }

        public override void makeNonDynamic()
        {
            if (isPhysicallySimulated)
            {
                makeNonDynamic();
                makeChildrenNonDynamic();
            }
        }

        // TODO: i think perhaps "move" and the internalcenterofmass and internalcenterposition should be moved into CollisionPrimitive
        public override void move(ref Vector3d translation)
        {
            myInternalCenterOfMass +=  translation;
            //CollisionPrimitive.Translate(translation);
            Entity.Translation = translation;

            usePropertyMasks = false;
            for (int i = 0; i < subBodies.Count; i++)
            {
                subBodies[i].move(ref translation);
            }
            usePropertyMasks = true;
        }

        public override void move(Vector3d translation)
        {
            move(ref translation );
        }

        internal void moveDirect(ref Vector3d v)
        {
            if (isSubBodyOfCompound)
            {
                compoundBody.moveDirect(ref v);
            }
            else
            {
                move(ref v);
            }
        }

        public override void moveTo(Vector3d v, bool useGeometricCenter)
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

        public void addBody(PhysicsBody body)
        {
            if (body.compoundBody == null)
            {
                subBodies.Add(body);
                body.compoundBody = this;
                body.isSubBodyOfCompound = true;
                initialize(true);
                CompoundBody body1 = body as CompoundBody;
                if (body1 != null)
                {
                    body1.countNumberOfCollidableChildren();
                    body1.countEntities();
                    numChildren += body1.numChildren;
                    numEntities += body1.numEntities;
                }
                else
                {
                    numChildren++;
                    numEntities++;
                }
                updateBufferedStates();
                createBoundingVolumeHierarchy();
                setParents();
            }
        }

        public bool removeBody(PhysicsBody body)
        {
            int num1 = subBodies.IndexOf(body);
            if (num1 <= -1)
            {
                return false;
            }
            subBodyLocalOffsets.Remove(subBodyLocalOffsets[num1]);
            subBodies.Remove(body);
            body.compoundBody = null;
            body.isSubBodyOfCompound = false;
            CompoundBody body1 = body as CompoundBody;
            if (body1 != null)
            {
                numChildren -= body1.numChildren;
                numEntities -= body1.numEntities;
            }
            else
            {
                numChildren--;
                numEntities--;
            }
            initialize(true);
            while (body.controllers.Count > 0)
            {
                space.removeController(body.controllers[0]);
            }
            if (simulationIsland != null)
            {
                simulationIsland.remove(body);
            }
            updateBufferedStates();
            createBoundingVolumeHierarchy();
            body.myParent = body;
            if (body1 != null)
            {
                body1.setParents();
            }
            return true;
        }

        internal void subCompoundBodyUpdate()
        {
            usePropertyMasks = false;
            for (int i = 0; i < subBodies.Count; i++)
            {
                Vector3d vector1 = subBodyLocalOffsets[i];
                vector1=Vector3d.TransformCoord(vector1, myInternalOrientationMatrix);
                vector1=Vector3d.Negate(vector1);
                Vector3d vector2 = Vector3d.CrossProduct(myInternalAngularVelocity, vector1);
                Vector3d vector3 = myInternalLinearVelocity + vector2;
                subBodies[i].internalLinearVelocity = vector3;
                subBodies[i].setAngularVelocity(ref myInternalAngularVelocity);
                Vector3d vector4 = vector1 +  myInternalCenterPosition;
                vector4 -= subBodies[i].myInternalCenterPosition;
                subBodies[i].move(ref vector4);
                Quaternion quaternion1 = subBodyLocalRotations[i];
                subBodies[i].myInternalOrientationQuaternion = Quaternion.Multiply(myInternalOrientationQuaternion, quaternion1);
                subBodies[i].myInternalOrientationMatrix = new Matrix(subBodies[i].myInternalOrientationQuaternion);
                if (subBodies[i] is CompoundBody)
                {
                    ((CompoundBody) subBodies[i]).subCompoundBodyUpdate();
                }
                else
                {
                    if (subBodies[i].CollisionPrimitive is TrianglePrimitive)
                    {
                        TrianglePrimitive triangle1 = (TrianglePrimitive)subBodies[i].CollisionPrimitive;
                        // TODO: temp comment out
                        //triangle1.Triangle.Normal = Vector3d.TransformCoord(triangle1.localNormal,
                        //                                           myInternalOrientationMatrix);
                    }
                }
            }
            usePropertyMasks = true;
            refitTree();
        }

        internal override sealed void updateBufferedStates()
        {
            updateBufferedStates();
            foreach (PhysicsBody entity1 in subBodies)
            {
                entity1.updateBufferedStates();
            }
        }

        internal override void interpolateStates(double originalAmount, double finalAmount)
        {
            interpolateStates(originalAmount, finalAmount);
            foreach (PhysicsBody entity1 in subBodies)
            {
                entity1.interpolateStates(originalAmount, finalAmount);
            }
        }

        //public override sealed void getExtremePoint(ref Vector3d d, ref Vector3d positionToUse,
        //                                            ref Quaternion orientationToUse, double margin,
        //                                            out Vector3d extremePoint)
        //{
        //    double single1 = double.NegativeInfinity;
        //    extremePoint = Toolbox.noVector;
        //    for (int i = 0; i < subBodies.Count; i++)
        //    {
        //        Vector3d vector1;
        //        subBodies[i].CollisionPrimitive.getExtremePoint(ref d, ref subBodies[i].CenterPosition,
        //                                          ref subBodies[i].myInternalOrientationQuaternion,
        //                                          margin + subBodies[i].CollisionPrimitive.myCollisionMargin, out vector1);
        //        double single2 = Vector3d.DotProduct(vector1, d);
        //        if (single2 > single1)
        //        {
        //            single1 = single2;
        //            extremePoint = vector1;
        //        }
        //    }
        //}

        //// i'm inclined to make the collision primitives contain the physicsBody references and children and such... hrm...
        //// well, hierarchically speaking that wouldnt really be correct for computing combined mass and such
        //// but for collision perhaps it's different?
        //public override sealed void getExtremePoints(ref Vector3d d, out Vector3d minPoint, out Vector3d maxPoint,
        //                                             double margin)
        //{
        //    double single1 = double.NegativeInfinity;
        //    double single2 = double.PositiveInfinity;
        //    maxPoint.ZeroVector();
        //    minPoint.ZeroVector();
        //    for (int i = 0; i < subBodies.Count; i++)
        //    {
        //        Vector3d vector1;
        //        Vector3d vector2;
        //        subBodies[i].CollisionPrimitive.getExtremePoints(ref d, out vector1, out vector2,
        //                                           margin + subBodies[i].CollisionPrimitive.myCollisionMargin);
        //        double single3 = Vector3d.DotProduct(vector2, d);
        //        if (single3 > single1)
        //        {
        //            single1 = single3;
        //            maxPoint = vector2;
        //        }
        //        if (single3 < single2)
        //        {
        //            single2 = single3;
        //            minPoint = vector1;
        //        }
        //        single3 =Vector3d.DotProduct(vector1, d);
        //        if (single3 < single2)
        //        {
        //            single2 = single3;
        //            minPoint = vector1;
        //        }
        //        if (single3 > single1)
        //        {
        //            single1 = single3;
        //            maxPoint = vector2;
        //        }
        //    }
        //}

        internal void countNumberOfCollidableChildren()
        {
            numChildren = 0;
            foreach (PhysicsBody entity1 in subBodies)
            {
                CompoundBody body1 = entity1 as CompoundBody;
                if (body1 != null)
                {
                    body1.countNumberOfCollidableChildren();
                    numChildren += body1.numChildren;
                    continue;
                }
                numChildren++;
            }
        }

        internal void countEntities()
        {
            numEntities = 1;
            foreach (PhysicsBody entity1 in subBodies)
            {
                numEntities++;
                CompoundBody body1 = entity1 as CompoundBody;
                if (body1 != null)
                {
                    body1.countEntities();
                    numEntities += body1.numEntities;
                }
            }
        }

        public void getAllRealChildren(List<PhysicsBody> children)
        {
            foreach (PhysicsBody item in subBodies)
            {
                if (item is CompoundBody)
                {
                    ((CompoundBody) item).getAllRealChildren(children);
                    continue;
                }
                children.Add(item);
            }
        }

        public void getControllers(List<Controller> controllers)
        {
            foreach (PhysicsBody entity1 in subBodies)
            {
                if (entity1 is CompoundBody)
                {
                    (entity1 as CompoundBody).getControllers(controllers);
                    continue;
                }
                foreach (Controller item in entity1.controllers)
                {
                    controllers.Add(item);
                }
            }
        }

        internal bool findNextState(float dt, out Vector3d nextPosition)
        {
            Vector3d vector1;
            Vector3d vector2;
            List<Controller> ctrlrs = ResourcePool.getControllerList();
            getControllers(controllers);
            int num1 = 0x7fffffff;
            for (int i = 0; i < ctrlrs.Count; i++)
            {
                if ((ctrlrs[i].detectedCollisionInContinuousTest && (ctrlrs[i].timeOfImpact != 0.00d)) &&
                    ((num1 == 0x7fffffff) || (ctrlrs[i].timeOfImpact < ctrlrs[num1].timeOfImpact)))
                {
                    num1 = i;
                }
            }
            Toolbox.integrateLinearVelocity(this, dt, out vector1, out vector2);
            if (num1 == 0x7fffffff)
            {
                nextPosition = vector2;
                ResourcePool.giveBack(ctrlrs);
                return false;
            }
            Controller controller1 = ctrlrs[num1];
            Vector3d vector3 = controller1.colliderA.myInternalLinearVelocity * dt;
            Vector3d vector4 = controller1.colliderB.myInternalLinearVelocity * dt;
            Vector3d vector5 = vector3 - vector4;
            Vector3d vector6 = controller1.colliderB.myInternalCenterPosition - controller1.colliderA.myInternalCenterPosition;
            double value = Vector3d.DotProduct(vector5, vector6);
            value = Math.Abs(value);
            double single2 = ctrlrs[num1].timeOfImpact;
            Vector3d vector7 = vector1 * (1.00D - single2);
            Vector3d vector8 = vector2 * single2;
            nextPosition= vector7 + vector8;
            ResourcePool.giveBack(ctrlrs);
            return true;
        }

        internal bool findNextState(float dt, out Vector3d nextPosition, out Quaternion nextOrientation)
        {
            Vector3d vector3;
            Vector3d vector4;
            Quaternion quaternion3;
            Quaternion quaternion4;
            List<Controller> controllers = ResourcePool.getControllerList();
            getControllers(controllers);
            int num1 = 0x7fffffff;
            for (int i = 0; i < controllers.Count; i++)
            {
                if ((controllers[i].detectedCollisionInContinuousTest && (controllers[i].timeOfImpact != 0.00d)) &&
                    ((num1 == 0x7fffffff) || (controllers[i].timeOfImpact < controllers[num1].timeOfImpact)))
                {
                    num1 = i;
                }
            }
            if (num1 == 0x7fffffff)
            {
                Vector3d vector1;
                Vector3d vector2;
                Quaternion quaternion1;
                Quaternion quaternion2;
                Toolbox.integrateLinearVelocity(this, dt, out vector1, out vector2);
                Toolbox.integrateAngularVelocity(this, dt, out quaternion1, out quaternion2);
                nextPosition = vector2;
                nextOrientation = quaternion2;
                ResourcePool.giveBack(controllers);
                return false;
            }
            Controller controller1 = controllers[num1];
            Toolbox.integrateLinearVelocity(this, dt, out vector3, out vector4);
            Toolbox.integrateAngularVelocity(this, dt, out quaternion3, out quaternion4);
            Vector3d vector5 = controller1.colliderA.myInternalLinearVelocity*dt;
            Vector3d vector6 = controller1.colliderB.myInternalLinearVelocity*dt;
            double single1 = controller1.colliderA.myInternalAngularVelocity.Length*dt;
            double single2 = controller1.colliderB.myInternalAngularVelocity.Length*dt;
            Vector3d vector7 = vector5 - vector6;
            // TODO: huh? vector7.Length;
            double single4 = controller1.colliderA.CollisionPrimitive.myMaximumRadius;
            double single5 = controller1.colliderB.CollisionPrimitive.myMaximumRadius;
            double single3 = controllers[num1].timeOfImpact;
            Vector3d vector8 = vector3*(1.00D - single3);
            Vector3d vector9 = vector4*single3;
            nextPosition = vector8 + vector9;
            nextOrientation = Quaternion.Slerp(quaternion3, quaternion4, single3);
            ResourcePool.giveBack(controllers);
            return true;
        }

        public bool isEntityWithin(PhysicsBody e)
        {
            if (e == this)
            {
                return true;
            }
            foreach (PhysicsBody entity1 in subBodies)
            {
                if (entity1 == e)
                {
                    return true;
                }
                if (entity1 is CompoundBody)
                {
                    return (entity1 as CompoundBody).isEntityWithin(e);
                }
            }
            return false;
        }

        internal bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin,
                              out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            hitLocation = Toolbox.noVector;
            hitNormal = Toolbox.noVector;
            t = double.NegativeInfinity;
            double single2 = double.PositiveInfinity;
            bool flag1 = false;
            List<PhysicsBody> nearbyEntities = new List<PhysicsBody>();
            getEntitiesNearRay(new Ray(origin, direction), maximumLength, nearbyEntities);
            for (int i = 0; i < nearbyEntities.Count; i++)
            {
                Vector3d vector1;
                Vector3d vector2;
                double single1;
                if (
                    Toolbox.rayCastGJK(origin, direction, maximumLength, nearbyEntities[i], withMargin, out vector1,
                                       out vector2, out single1) && (single1 < single2))
                {
                    single2 = single1;
                    flag1 = true;
                    hitLocation = vector1;
                    hitNormal = vector2;
                    t = single1;
                }
            }
            ResourcePool.giveBack(nearbyEntities);
            return flag1;
        }

        internal bool rayTestInfinite(Vector3d origin, Vector3d direction, bool withMargin, out Vector3d hitLocation,
                                      out Vector3d hitNormal, out double t)
        {
            hitLocation = Toolbox.noVector;
            hitNormal = Toolbox.noVector;
            t = double.NegativeInfinity;
            double single2 = double.PositiveInfinity;
            bool flag1 = false;
            List<PhysicsBody> nearbyEntities = new List<PhysicsBody>();
            getEntitiesNearRay(new Ray(origin, direction), float.MaxValue, nearbyEntities);
            foreach (PhysicsBody target in nearbyEntities)
            {
                Vector3d vector1;
                Vector3d vector2;
                double single1;
                if (
                    Toolbox.rayCastGJKInfinite(origin, direction, target, withMargin, out vector1, out vector2,
                                               out single1) && (t < single2))
                {
                    hitLocation = vector1;
                    hitNormal = vector2;
                    t = single1;
                    flag1 = true;
                }
            }
            ResourcePool.giveBack(nearbyEntities);
            return flag1;
        }

        internal bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, double margin,
                              out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            hitLocation = Toolbox.noVector;
            hitNormal = Toolbox.noVector;
            t = double.NegativeInfinity;
            double single2 = double.PositiveInfinity;
            bool flag1 = false;
            direction = (direction*maximumLength);
            List<PhysicsBody> nearbyEntities = new List<PhysicsBody>();
            getEntitiesNearRay(new Ray(origin, direction), maximumLength, nearbyEntities);
            foreach (PhysicsBody target in nearbyEntities)
            {
                Vector3d vector1;
                Vector3d vector2;
                double single1;
                if (
                    Toolbox.rayCast(origin, direction, maximumLength, target, margin, out vector1, out vector2,
                                    out single1) && (t < single2))
                {
                    hitLocation = vector1;
                    hitNormal = vector2;
                    t = single1;
                    flag1 = true;
                }
            }
            ResourcePool.giveBack(nearbyEntities);
            return flag1;
        }

        public bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin,
                            List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals,
                            List<double> tois)
        {
            bool flag1 = false;
            direction = (direction*maximumLength);
            List<PhysicsBody> nearbyEntities = new List<PhysicsBody>();
            getEntitiesNearRay(new Ray(origin, direction), maximumLength, nearbyEntities);
            foreach (PhysicsBody target in nearbyEntities)
            {
                Vector3d item;
                Vector3d vector2;
                double single1;
                if (Toolbox.rayCast(origin, direction, maximumLength, target, CollisionPrimitive.myCollisionMargin, out item,
                                    out vector2, out single1))
                {
                    hitLocations.Add(item);
                    hitNormals.Add(vector2);
                    tois.Add(single1);
                    flag1 = true;
                }
            }
            ResourcePool.giveBack(nearbyEntities);
            return flag1;
        }

        internal void createBoundingVolumeHierarchy()
        {
            List<SortableEntity> children = new List<SortableEntity>();
            fillSortableChildrenList(children);
            new Queue<List<SortableEntity>>();
            root = new CompoundBodyBVHNode(children);
        }

        private void fillSortableChildrenList(List<SortableEntity> children)
        {
            allChildren.Clear();
            foreach (PhysicsBody item in subBodies)
            {
                CompoundBody body1 = item as CompoundBody;
                if (body1 != null)
                {
                    body1.fillSortableChildrenList(children);
                    allChildren.AddRange(body1.allChildren);
                    continue;
                }
                allChildren.Add(item);
                children.Add(new SortableEntity(item));
            }
        }

        private void refitTree()
        {
            root.refit();
        }

        internal void getEntitiesNearEntity(PhysicsBody e, List<PhysicsBody> nearbyEntities)
        {
            root.getEntitiesNearEntity(e, nearbyEntities);
        }

        internal void getEntitiesNearRay(Ray ray, double maximumDistance, List<PhysicsBody> nearbyEntities)
        {
            root.getEntitiesNearRay(ray, maximumDistance, nearbyEntities);
        }


        // Properties
        public int numberOfCollidableChildren
        {
            get { return numChildren; }
        }

        public int numberOfEntities
        {
            get { return numEntities; }
        }
    }
}