using System;
using System.Collections.Generic;
using System.Threading;
using Keystone.Types;
using Keystone.Physics.BroadPhases;
using Keystone.Physics.Constraints;
using Keystone.Physics.Entities;

namespace Keystone.Physics
{
    public class Space
    {
        // Instance Fields
        public SimulationSettings simulationSettings;
        public List<PhysicsBody> entities;
        internal BroadPhase myBroadPhase;
        public List<Constraint> constraints;
        public List<SingleBodyConstraint> singleBodyConstraints;
        public List<SolverUpdateable> solverUpdateables;
        public List<PhysicsBody> physObjects;
        public List<Controller> controllers;
        public List<Updateable> updateables;
        public List<CombinedUpdateable> combinedUpdateables;
        public List<RayCastableContainer> rayCastableUpdateables;
        public List<RayCastableContainerWithoutMargins> rayCastableUpdateablesWithoutMargins;
        public List<SimulationIsland> simulationIslands;
        public object lockerUpdate;
        public object lockerEntities;
        public object lockerControllers;
        public object lockerConstraints;
        public object lockerUpdateables;
        private int simulationIslandToCheck;
        private int oldSimulationIslandCount;
        private bool useDynamicIterationCount;
        private float inverseGoalFPS;
        private int minimumIterations;
        private int maximumIterations;
        private int priorIterations;
        internal Dictionary<ControllerTableKey, Controller> controllersTable;
        private double accumulatedTime;
        private List<Updateable> updateablesToRemove;
        private List<CombinedUpdateable> combinedUpdateablesToRemove;
        internal List<PhysicsBody> eventfulEntities;
        private Queue<PhysicsBody> dispatchQueue;

        // Constructors
        public Space()
        {
            simulationSettings = new SimulationSettings();
            entities = new List<PhysicsBody>();
            constraints = new List<Constraint>();
            singleBodyConstraints = new List<SingleBodyConstraint>();
            solverUpdateables = new List<SolverUpdateable>();
            physObjects = new List<PhysicsBody>();
            controllers = new List<Controller>();
            updateables = new List<Updateable>();
            combinedUpdateables = new List<CombinedUpdateable>();
            rayCastableUpdateables = new List<RayCastableContainer>();
            rayCastableUpdateablesWithoutMargins = new List<RayCastableContainerWithoutMargins>();
            simulationIslands = new List<SimulationIsland>();
            lockerUpdate = new object();
            lockerEntities = new object();
            lockerControllers = new object();
            lockerConstraints = new object();
            lockerUpdateables = new object();
            controllersTable = new Dictionary<ControllerTableKey, Controller>(0x64);
            updateablesToRemove = new List<Updateable>();
            combinedUpdateablesToRemove = new List<CombinedUpdateable>();
            eventfulEntities = new List<PhysicsBody>();
            dispatchQueue = new Queue<PhysicsBody>();
            myBroadPhase = new DynamicBinaryHierarchy();
            myBroadPhase.space = this;
        }

        public Space(BroadPhase broadPhaseToUse) : this()
        {
            myBroadPhase = broadPhaseToUse;
            myBroadPhase.space = this;
        }

        // TODO: this confuses me.  I'm simply assigning entities but shouldnt i itterate and call Add() since it inits things?
        public Space(List<PhysicsBody> ents) : this()
        {
            entities = ents;
        }

        public Space(BroadPhase broadPhaseToUse, List<PhysicsBody> ents) : this()
        {
            myBroadPhase = broadPhaseToUse;
            myBroadPhase.space = this;
            entities = ents;
        }

        // Properties
        public BroadPhase broadPhase
        {
            get { return myBroadPhase; }
            set
            {
                lock (lockerUpdate)
                {
                    myBroadPhase = value;
                    myBroadPhase.space = this;
                    foreach (PhysicsBody e in entities)
                    {
                        myBroadPhase.addEntity(e);
                    }
                }
            }
        }

        // Methods
        public void add(PhysicsBody physicsBody)
        {
            lock (lockerUpdate)
            {
                lock (lockerEntities)
                {
                    if (physicsBody.space != null)
                    {
                        throw new Exception("PhysicsBody already assigned to a space.  Must first be removed...");
                        return;
                    }
                    entities.Add(physicsBody);
                    physicsBody.space = this;
                    if (physicsBody.isPhysicallySimulated)
                    {
                        physObjects.Add(physicsBody);
                        SimulationIsland item = ResourcePool.getSimulationIsland(this);
                        simulationIslands.Add(item);
                        item.add(physicsBody);
                    }
                    if (physicsBody.CollisionPrimitive.myCollisionMargin <= 0.00d)
                    {
                        physicsBody.collisionMargin = simulationSettings.defaultMargin;
                        physicsBody.allowedPenetration = simulationSettings.defaultAllowedPenetration;
                    }
                    physicsBody.CollisionPrimitive.findBoundingBox(0.00f);
                    myBroadPhase.addEntity(physicsBody);
                    foreach (Constraint constraint1 in physicsBody.constraints)
                    {
                        if (((constraint1.parentA.simulationIsland != constraint1.parentB.simulationIsland) &&
                             (constraint1.parentA.simulationIsland != null)) &&
                            (constraint1.parentB.simulationIsland != null))
                        {
                            SimulationIsland.mergeWith(constraint1.parentA.simulationIsland,
                                                       constraint1.parentB.simulationIsland);
                        }
                    }
                    if (physicsBody.isEventful)
                    {
                        eventfulEntities.Add(physicsBody);
                    }
                }
            }
        }

        public bool remove(PhysicsBody physicsBody)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerEntities)
                {
                    int index = entities.IndexOf(physicsBody);
                    if (index <= -1)
                    {
                        return false;
                    }
                    myBroadPhase.removeEntity(physicsBody);
                    entities.RemoveAt(index);
                    if (physicsBody.isEventful)
                    {
                        eventfulEntities.Remove(physicsBody);
                    }
                    if (physicsBody.isPhysicallySimulated)
                    {
                        physicsBody.activate();
                    }
                    while (physicsBody.controllers.Count > 0)
                    {
                        removeController(physicsBody.controllers[0]);
                    }
                    if (physicsBody.isPhysicallySimulated)
                    {
                        physObjects.Remove(physicsBody);
                        physicsBody.simulationIsland.remove(physicsBody);
                    }
                    physicsBody.space = null;
                    return true;
                }
            }
            return flag1;
        }

        public void add(Constraint constraint)
        {
            lock (lockerUpdate)
            {
                lock (lockerConstraints)
                {
                    if (constraint.space != null)
                    {
                        return;
                    }
                    constraint.space = this;
                    constraints.Add(constraint);
                    constraint.connectionA.constraints.Add(constraint);
                    constraint.connectionB.constraints.Add(constraint);
                    if (constraint.connectionA.simulationIsland != null)
                    {
                        constraint.connectionA.activate();
                    }
                    if ((!constraint.connectionA.isPhysicallySimulated || !constraint.connectionB.isPhysicallySimulated) ||
                        (constraint.connectionA.simulationIsland == constraint.connectionB.simulationIsland))
                    {
                        return;
                    }
                    if (constraint.connectionA.simulationIsland.entities.Count < constraint.connectionB.simulationIsland.entities.Count)
                    {
                        SimulationIsland.mergeWith(constraint.connectionB.simulationIsland,
                                                   constraint.connectionA.simulationIsland);
                    }
                    else
                    {
                        SimulationIsland.mergeWith(constraint.connectionA.simulationIsland,
                                                   constraint.connectionB.simulationIsland);
                    }
                }
            }
        }

        public bool remove(Constraint constraint)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerConstraints)
                {
                    if (!constraints.Remove(constraint))
                    {
                        return false;
                    }
                    constraint.space = null;
                    constraint.connectionA.constraints.Remove(constraint);
                    constraint.connectionB.constraints.Remove(constraint);
                    if (constraint.connectionA.simulationIsland != null)
                    {
                        constraint.connectionA.activate();
                    }
                    SimulationIsland.trySplit(constraint.connectionA, constraint.connectionB);
                    return true;
                }
            }
            return flag1;
        }

        public void add(SingleBodyConstraint constraint)
        {
            lock (lockerUpdate)
            {
                lock (lockerConstraints)
                {
                    if (constraint.space == null)
                    {
                        constraint.space = this;
                        singleBodyConstraints.Add(constraint);
                    }
                }
            }
        }

        public bool remove(SingleBodyConstraint constraint)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerConstraints)
                {
                    if (singleBodyConstraints.Remove(constraint))
                    {
                        constraint.space = null;
                        return true;
                    }
                    return false;
                }
            }
            return flag1;
        }

        public void add(Updateable updateable)
        {
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (updateable.space != null)
                    {
                        return;
                    }
                    updateables.Add(updateable);
                    updateable.addToSpace(this);
                    if (updateable is RayCastableContainer)
                    {
                        rayCastableUpdateables.Add(updateable as RayCastableContainer);
                    }
                    else
                    {
                        if (updateable is RayCastableContainerWithoutMargins)
                        {
                            rayCastableUpdateablesWithoutMargins.Add(updateable as RayCastableContainerWithoutMargins);
                        }
                    }
                }
            }
        }

        public bool remove(Updateable updateable)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (!updateables.Remove(updateable))
                    {
                        return false;
                    }
                    updateable.removeFromSpace();
                    if (updateable is RayCastableContainer)
                    {
                        rayCastableUpdateables.Remove(updateable as RayCastableContainer);
                    }
                    else
                    {
                        if (updateable is RayCastableContainerWithoutMargins)
                        {
                            rayCastableUpdateablesWithoutMargins.Remove(updateable as RayCastableContainerWithoutMargins);
                        }
                    }
                    return true;
                }
            }
            return flag1;
        }

        public void add(CombinedUpdateable updateable)
        {
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (updateable.space != null)
                    {
                        return;
                    }
                    combinedUpdateables.Add(updateable);
                    updateable.addToSpace(this);
                    if (updateable is RayCastableContainer)
                    {
                        rayCastableUpdateables.Add(updateable as RayCastableContainer);
                    }
                    else
                    {
                        if (updateable is RayCastableContainerWithoutMargins)
                        {
                            rayCastableUpdateablesWithoutMargins.Add(updateable as RayCastableContainerWithoutMargins);
                        }
                    }
                }
            }
        }

        public bool remove(CombinedUpdateable updateable)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (!combinedUpdateables.Remove(updateable))
                    {
                        return false;
                    }
                    updateable.removeFromSpace();
                    if (updateable is RayCastableContainer)
                    {
                        rayCastableUpdateables.Remove(updateable as RayCastableContainer);
                    }
                    else
                    {
                        if (updateable is RayCastableContainerWithoutMargins)
                        {
                            rayCastableUpdateablesWithoutMargins.Remove(updateable as RayCastableContainerWithoutMargins);
                        }
                    }
                    return true;
                }
            }
            return flag1;
        }

        public void add(SolverUpdateable solverUpdateable)
        {
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (solverUpdateable.space == null)
                    {
                        solverUpdateables.Add(solverUpdateable);
                        solverUpdateable.addToSpace(this);
                    }
                }
            }
        }

        public bool remove(SolverUpdateable solverUpdateable)
        {
            bool flag1;
            lock (lockerUpdate)
            {
                lock (lockerUpdateables)
                {
                    if (solverUpdateables.Remove(solverUpdateable))
                    {
                        solverUpdateable.removeFromSpace();
                        return true;
                    }
                    return false;
                }
            }
            return flag1;
        }

        internal void addController(PhysicsBody a, PhysicsBody b)
        {
            ControllerTableKey key = ResourcePool.getControllerTableKey(a, b);
            if (controllersTable.ContainsKey(key))
            {
                ResourcePool.giveBack(key);
            }
            else
            {
                Controller item = ResourcePool.getController(a, b, this);
                item.tableKey = key;
                controllers.Add(item);
                a.controllers.Add(item);
                b.controllers.Add(item);
                controllersTable.Add(key, item);
                a.onControllerCreated(item);
                b.onControllerCreated(item);
                if ((item.parentA.isPhysicallySimulated && item.parentB.isPhysicallySimulated) &&
                    (item.parentA.simulationIsland != item.parentB.simulationIsland))
                {
                    if (item.parentA.simulationIsland.entities.Count < item.parentB.simulationIsland.entities.Count)
                    {
                        SimulationIsland.mergeWith(item.parentB.simulationIsland, item.parentA.simulationIsland);
                    }
                    else
                    {
                        SimulationIsland.mergeWith(item.parentA.simulationIsland, item.parentB.simulationIsland);
                    }
                }
                else if (item.parentA.isPhysicallySimulated && !item.parentB.isPhysicallySimulated)
                {
                    item.parentA.activate();
                }
                else
                {
                    if (item.parentB.isPhysicallySimulated && !item.parentA.isPhysicallySimulated)
                    {
                        item.parentB.activate();
                    }
                }
            }
        }

        internal void removeController(Controller controller)
        {
            controller.clearContacts();
            controller.colliderA.onControllerRemoved(controller.colliderA, controller.colliderB);
            controller.colliderB.onControllerRemoved(controller.colliderA, controller.colliderB);
            controllers.Remove(controller);
            controller.colliderA.controllers.Remove(controller);
            controller.colliderB.controllers.Remove(controller);
            controllersTable.Remove(controller.tableKey);
            ResourcePool.giveBack(controller.tableKey);
            SimulationIsland.trySplit(controller.colliderA, controller.colliderB);
            ResourcePool.giveBack(controller);
        }

        internal void removeController(int i)
        {
            Controller item = controllers[i];
            item.clearContacts();
            item.colliderA.onControllerRemoved(item.colliderA, item.colliderB);
            item.colliderB.onControllerRemoved(item.colliderA, item.colliderB);
            controllers.RemoveAt(i);
            item.colliderA.controllers.Remove(item);
            item.colliderB.controllers.Remove(item);
            controllersTable.Remove(item.tableKey);
            ResourcePool.giveBack(item.tableKey);
            SimulationIsland.trySplit(item.colliderA, item.colliderB);
            ResourcePool.giveBack(item);
        }

        public bool isControllerPresent(PhysicsBody a, PhysicsBody b)
        {
            ControllerTableKey key = ResourcePool.getControllerTableKey(a, b);
            if (controllersTable.ContainsKey(key))
            {
                ResourcePool.giveBack(key);
                return true;
            }
            ResourcePool.giveBack(key);
            return false;
        }

        public void activateDynamicIterationCount(float desiredFPS, int minIterations, int maxIterations)
        {
            lock (simulationSettings)
            {
                useDynamicIterationCount = true;
                inverseGoalFPS = 1.00F/desiredFPS;
                minimumIterations = minIterations;
                maximumIterations = maxIterations;
                priorIterations = simulationSettings.iterations;
            }
        }

        public void deactivateDynamicIterationCount()
        {
            lock (simulationSettings)
            {
                if (useDynamicIterationCount)
                {
                    useDynamicIterationCount = false;
                    simulationSettings.iterations = priorIterations;
                }
            }
        }

        internal void manageDynamicIterations(float timeSinceLastFrame)
        {
            lock (simulationSettings)
            {
                if (!useDynamicIterationCount || (timeSinceLastFrame == 0.00d))
                {
                    return;
                }
                if (timeSinceLastFrame > inverseGoalFPS)
                {
                    if (timeSinceLastFrame > 0.20F)
                    {
                        simulationSettings.iterations -= 4;
                    }
                    else
                    {
                        simulationSettings.iterations--;
                    }
                }
                else
                {
                    if (timeSinceLastFrame < inverseGoalFPS)
                    {
                        simulationSettings.iterations++;
                    }
                }
                if (simulationSettings.iterations < minimumIterations)
                {
                    simulationSettings.iterations = minimumIterations;
                }
                else
                {
                    if (simulationSettings.iterations > maximumIterations)
                    {
                        simulationSettings.iterations = maximumIterations;
                    }
                }
            }
        }

        internal void sleepPulse()
        {
            if (oldSimulationIslandCount != simulationIslands.Count)
            {
                simulationIslandToCheck = 0;
            }
            int num1 = 0;
            int num2 = 0;
            while ((simulationIslandToCheck < simulationIslands.Count) &&
                   (num2 < simulationSettings.numEntitiesToTryToDeactivatePerFrame))
            {
                simulationIslands[simulationIslandToCheck].tryToDeactivate();
                num2 += simulationIslands[simulationIslandToCheck].entities.Count;
                num1++;
                simulationIslandToCheck++;
                if (simulationIslandToCheck == simulationIslands.Count)
                {
                    simulationIslandToCheck = 0;
                }
                if (num1 == simulationIslands.Count)
                {
                    break;
                }
            }
            oldSimulationIslandCount = simulationIslands.Count;
        }

        internal void findBoundingBoxes(float dt)
        {
            lock (lockerEntities)
            {
                foreach (PhysicsBody body in entities)
                {
                    if (body.myIsActive)
                    {
                        body.CollisionPrimitive.findBoundingBox(dt);
                    }
                }
            }
        }

        internal void solveVelocities(float dt, float timeSinceLastFrame)
        {
            lock (lockerConstraints)
            {
                for (int i = 0; i < constraints.Count; i++)
                {
                    if (constraints[i].isActive)
                    {
                        constraints[i].preStep(dt);
                    }
                }
                for (int j = 0; j < singleBodyConstraints.Count; j++)
                {
                    if (singleBodyConstraints[j].isActive)
                    {
                        singleBodyConstraints[j].preStep(dt);
                    }
                }
            }
            lock (lockerControllers)
            {
                for (int z = 0; z < controllers.Count; z++)
                {
                    if ((controllers[z].parentA.myIsActive || controllers[z].parentB.myIsActive) &&
                        (!controllers[z].parentA.myIsDetector && !controllers[z].parentB.myIsDetector))
                    {
                        controllers[z].preStep(dt);
                    }
                }
            }
            lock (lockerUpdateables)
            {
                for (int k = 0; k < solverUpdateables.Count; k++)
                {
                    solverUpdateables[k].preStep(dt);
                }
                for (int l = 0; l < combinedUpdateables.Count; l++)
                {
                    combinedUpdateables[l].preStep(dt);
                }
            }
            lock (simulationSettings)
            {
                for (int a = 0; a < simulationSettings.iterations; a++)
                {
                    lock (lockerControllers)
                    {
                        for (int b = 0; b < controllers.Count; b++)
                        {
                            if ((controllers[b].parentA.myIsActive || controllers[b].parentB.myIsActive) &&
                                (!controllers[b].parentA.myIsDetector && !controllers[b].parentB.myIsDetector))
                            {
                                controllers[b].applyImpulse();
                            }
                        }
                    }
                    lock (lockerConstraints)
                    {
                        for (int c = 0; c < constraints.Count; c++)
                        {
                            if (constraints[c].isActive)
                            {
                                constraints[c].applyImpulse(dt);
                            }
                        }
                        for (int d = 0; d < singleBodyConstraints.Count; d++)
                        {
                            if (singleBodyConstraints[d].physicsBody.myIsActive)
                            {
                                singleBodyConstraints[d].applyImpulse(dt);
                            }
                        }
                    }
                    lock (lockerUpdateables)
                    {
                        for (int e = 0; e < solverUpdateables.Count; e++)
                        {
                            solverUpdateables[e].update(dt, simulationSettings.timeScale, timeSinceLastFrame);
                        }
                        for (int f = 0; f < combinedUpdateables.Count; f++)
                        {
                            combinedUpdateables[f].updateVelocities(dt, simulationSettings.timeScale, timeSinceLastFrame);
                        }
                    }
                }
            }
        }

        internal void updateBufferedStates()
        {
            lock (lockerEntities)
            {
                foreach (PhysicsBody body in entities)
                {
                    if (body.myIsActive)
                    {
                        body.updateBufferedStates();
                    }
                }
            }
        }

        internal void writeBufferedStates()
        {
            lock (lockerEntities)
            {
                foreach (PhysicsBody entity1 in entities)
                {
                    //entity1.writeBufferedStates();
                }
            }
        }

        private void updateActivity(float dt)
        {
            lock (lockerEntities)
            {
                foreach (PhysicsBody body in entities)
                {
                    body.updateActivity(dt);
                }
            }
        }

        private void applyForces(float dt)
        {
            lock (lockerEntities)
            {
                foreach (PhysicsBody body in physObjects)
                {
                    if (body.myIsActive)
                    {
                        body.applyForces(dt, simulationSettings.timeScale);
                    }
                }
            }
        }

        private void updateControllers(float dt, float timeSinceLastFrame)
        {
            lock (lockerControllers)
            {
                lock (lockerEntities)
                {
                    myBroadPhase.updateControllers(dt, timeSinceLastFrame);
                }
            }
        }

        private void updateControllersPrePositionUpdate(float dt, float timeSinceLastFrame)
        {
            lock (lockerControllers)
            {
                lock (lockerEntities)
                {
                    myBroadPhase.preUpdate(dt, timeSinceLastFrame);
                }
            }
        }

        private void updateCollisionDetectionDiscrete(float dt)
        {
            lock (lockerControllers)
            {
                foreach (Controller controller1 in controllers)
                {
                    if (!controller1.parentA.myIsActive && !controller1.parentB.myIsActive)
                    {
                        continue;
                    }
                    controller1.updateIterations = 0;
                    if (simulationSettings.collisionDetectionType == CollisionDetectionType.discreteMPRGJK)
                    {
                        controller1.updateCollisionDiscreteMPRGJK(dt);
                        continue;
                    }
                    if (simulationSettings.collisionDetectionType == CollisionDetectionType.discreteMPR)
                    {
                        controller1.updateCollisionDiscreteMPR(dt);
                        continue;
                    }
                    controller1.updateCollisionDiscreteGJK(dt);
                }
            }
        }

        private void updateCollisionDetectionCCD(float dt)
        {
            lock (lockerControllers)
            {
                foreach (Controller controller1 in controllers)
                {
                    if (!controller1.parentA.myIsActive && !controller1.parentB.myIsActive)
                    {
                        continue;
                    }
                    controller1.updateCollisionDiscreteMPRGJK(dt);
                }
            }
        }

        private void updatePositionsDiscrete(float dt)
        {
            lock (lockerEntities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (entities[i].myIsActive)
                    {
                        entities[i].update(dt, simulationSettings.timeScale);
                    }
                }
            }
        }

        private void updateContinuousMotion(double dt)
        {
            lock (lockerControllers)
            {
                if (simulationSettings.collisionDetectionType == CollisionDetectionType.linearContinuous)
                {
                    foreach (Controller controller in controllers)
                    {
                        Vector3d vector1;
                        Vector3d vector2;
                        Vector3d vector3;
                        Vector3d vector4;
                        if (isNonContinuousDetectionPair(controller))
                        {
                            controller.detectedCollisionInContinuousTest = false;
                            continue;
                        }
                        Toolbox.integrateLinearVelocity(controller.colliderA, controller.colliderB, dt, out vector1,
                                                        out vector2, out vector3, out vector4);
                        //controller.colliderA.myInternalLinearVelocity * dt;
                        //controller.colliderB.myInternalLinearVelocity * dt;
                        controller.detectedCollisionInContinuousTest =
                            Toolbox.areSweptObjectsCollidingCA(controller.colliderA, controller.colliderB, ref vector1,
                                                               ref vector2,
                                                               ref controller.colliderA.myInternalOrientationQuaternion,
                                                               ref controller.colliderB.myInternalOrientationQuaternion,
                                                               ref vector3, ref vector4, out controller.nextPositionA,
                                                               out controller.nextPositionB, out controller.timeOfImpact);
                        double single1 = 1.00d - controller.timeOfImpact;
                        controller.nextPositionA = ((vector1*single1) + (vector3*controller.timeOfImpact));
                        controller.nextPositionB = ((vector2*single1) + (vector4*controller.timeOfImpact));
                    }
                }
                else
                {
                    foreach (Controller controller2 in controllers)
                    {
                        Vector3d vector5;
                        Vector3d vector6;
                        Vector3d vector7;
                        Vector3d vector8;
                        Quaternion quaternion1;
                        Quaternion quaternion2;
                        Quaternion quaternion3;
                        Quaternion quaternion4;
                        if (isNonContinuousDetectionPair(controller2))
                        {
                            controller2.detectedCollisionInContinuousTest = false;
                            continue;
                        }
                        Toolbox.integrateLinearVelocity(controller2.colliderA, controller2.colliderB, dt, out vector5,
                                                        out vector6, out vector7, out vector8);
                        Toolbox.integrateAngularVelocity(controller2.colliderA, controller2.colliderB, dt,
                                                         out quaternion1, out quaternion2, out quaternion3,
                                                         out quaternion4);
                        controller2.detectedCollisionInContinuousTest =
                            Toolbox.areSweptObjectsCollidingCA(controller2.colliderA, controller2.colliderB, ref vector5,
                                                               ref vector6, ref quaternion1, ref quaternion2,
                                                               ref vector7, ref vector8, ref quaternion3,
                                                               ref quaternion4, out controller2.nextPositionA,
                                                               out controller2.nextPositionB,
                                                               out controller2.nextOrientationA,
                                                               out controller2.nextOrientationB,
                                                               out controller2.timeOfImpact);
                    }
                }
            }
        }

        private void updatePositionsCCD(float dt)
        {
            lock (lockerEntities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (!entities[i].myIsActive)
                    {
                        continue;
                    }
                    if (entities[i] is CompoundBody)
                    {
                        Vector3d vector2;
                        if (simulationSettings.collisionDetectionType == CollisionDetectionType.fullyContinuous)
                        {
                            Vector3d v;
                            Quaternion q;
                            CompoundBody body1 = entities[i] as CompoundBody;
                            if (body1.findNextState(dt, out v, out q))
                            {
                                entities[i].moveTo(v, false);
                                entities[i].applyQuaternion(q);
                                entities[i].update(dt, simulationSettings.timeScale, false, false);
                                continue;
                            }
                            entities[i].update(dt, simulationSettings.timeScale);
                            continue;
                        }
                        CompoundBody body2 = entities[i] as CompoundBody;
                        if (body2.findNextState(dt, out vector2))
                        {
                            entities[i].moveTo(vector2, false);
                            entities[i].update(dt, simulationSettings.timeScale, false, true);
                            continue;
                        }
                        entities[i].update(dt, simulationSettings.timeScale);
                        continue;
                    }
                    Controller controller1 = null;
                    double single1 = 2.00F;
                    foreach (Controller controller2 in entities[i].controllers)
                    {
                        if (((controller2.colliderA.myIsTangible && controller2.colliderB.myIsTangible) &&
                             (controller2.detectedCollisionInContinuousTest && (controller2.timeOfImpact < single1))) &&
                            (controller2.timeOfImpact > 0.00d))
                        {
                            controller1 = controller2;
                            single1 = controller2.timeOfImpact;
                        }
                    }
                    if (controller1 != null)
                    {
                        if (simulationSettings.collisionDetectionType == CollisionDetectionType.fullyContinuous)
                        {
                            if (entities[i] == controller1.colliderA)
                            {
                                entities[i].moveTo(controller1.nextPositionA, false);
                                entities[i].applyQuaternion(controller1.nextOrientationA);
                            }
                            else
                            {
                                entities[i].moveTo(controller1.nextPositionB, false);
                                entities[i].applyQuaternion(controller1.nextOrientationB);
                            }
                            entities[i].update(dt, simulationSettings.timeScale, false, false);
                            continue;
                        }
                        if (entities[i] == controller1.colliderA)
                        {
                            entities[i].moveTo(controller1.nextPositionA, false);
                        }
                        else
                        {
                            entities[i].moveTo(controller1.nextPositionB, false);
                        }
                        entities[i].update(dt, simulationSettings.timeScale, false, true);
                        continue;
                    }
                    entities[i].update(dt, simulationSettings.timeScale);
                }
            }
        }


        public void update(float elapsedMilliseconds)
        {
            float elapsedSeconds = elapsedMilliseconds ; /// 1000; // convert to seconds
            lock (lockerUpdate)
            {
                if (simulationSettings.useInternalTimeStepping)
                {
                    updateWithInternalTimeSteps(elapsedSeconds);
                }
                else
                {
                    updateWithoutInternalTimeSteps(elapsedSeconds);
                }
                updateEndOfFrameUpdateables(simulationSettings.timeStep, elapsedSeconds);
            }
        }

        internal void updateWithInternalTimeSteps(float elapsedSeconds)
        {
            CompoundBody body1;
            object obj;
            accumulatedTime += elapsedSeconds;
            accumulatedTime = Toolbox.Clamp(accumulatedTime,
                                            simulationSettings.timeStepCountPerFrameMinimum*simulationSettings.timeStep,
                                            simulationSettings.timeStepCountPerFrameMaximum*simulationSettings.timeStep);
            if ((simulationSettings.collisionDetectionType != CollisionDetectionType.linearContinuous) &&
                (simulationSettings.collisionDetectionType != CollisionDetectionType.fullyContinuous))
            {
                goto Label_02AE;
            }
            while (accumulatedTime >= simulationSettings.timeStep)
            {
                lock (lockerEntities)
                {
                    foreach (PhysicsBody entity1 in entities)
                    {
                        entity1.myInternalPreviousCenterOfMass = entity1.myInternalCenterOfMass;
                        entity1.myInternalPreviousOrientationQuaternion = entity1.myInternalOrientationQuaternion;
                        body1 = entity1 as CompoundBody;
                        if (body1 != null)
                        {
                            foreach (PhysicsBody entity2 in body1.subBodies)
                            {
                                entity2.myInternalPreviousCenterOfMass = entity2.myInternalCenterOfMass;
                                entity2.myInternalPreviousOrientationQuaternion =
                                    entity2.myInternalOrientationQuaternion;
                            }
                            continue;
                        }
                    }
                }
                updateCCD(elapsedSeconds);
                accumulatedTime -= simulationSettings.timeStep;
            }
            double finalAmount = accumulatedTime/simulationSettings.timeStep;
            double originalAmount = 1.00d - finalAmount;
            lock (lockerEntities)
            {
                foreach (PhysicsBody entity3 in entities)
                {
                    entity3.interpolateStates(originalAmount, finalAmount);
                }
                return;
            }
            Label_01CF:
            Monitor.Enter(obj = lockerEntities);
            try
            {
                foreach (PhysicsBody entity4 in entities)
                {
                    entity4.myInternalPreviousCenterOfMass = entity4.myInternalCenterOfMass;
                    entity4.myInternalPreviousOrientationQuaternion = entity4.myInternalOrientationQuaternion;
                    body1 = entity4 as CompoundBody;
                    if (body1 != null)
                    {
                        foreach (PhysicsBody entity5 in body1.subBodies)
                        {
                            entity5.myInternalPreviousCenterOfMass = entity5.myInternalCenterOfMass;
                            entity5.myInternalPreviousOrientationQuaternion = entity5.myInternalOrientationQuaternion;
                        }
                        continue;
                    }
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
            updateDiscrete(elapsedSeconds);
            accumulatedTime -= simulationSettings.timeStep;
            Label_02AE:
            if (accumulatedTime >= simulationSettings.timeStep)
            {
                goto Label_01CF;
            }
            finalAmount = accumulatedTime/simulationSettings.timeStep;
            double single3 = 1.00F - finalAmount;
            lock (lockerEntities)
            {
                foreach (PhysicsBody entity6 in entities)
                {
                    entity6.interpolateStates(single3, finalAmount);
                }
            }
        }

        internal void updateWithoutInternalTimeSteps(float elapsedSeconds)
        {
            if ((simulationSettings.collisionDetectionType == CollisionDetectionType.linearContinuous) ||
                (simulationSettings.collisionDetectionType == CollisionDetectionType.fullyContinuous))
            {
                updateCCD(elapsedSeconds);
            }
            else
            {
                updateDiscrete(elapsedSeconds);
            }
        }

        internal void updateDiscrete(float elapsedSeconds)
        {
            float dt = simulationSettings.timeStep;
            if (dt == 0.00d)
            {
                dt = 0.02F;
            }
            dt *= simulationSettings.timeScale;
            manageDynamicIterations(elapsedSeconds);
            sleepPulse();
            writeBufferedStates();
            applyForces(dt);
            updateDuringForcesUpdateables(dt, elapsedSeconds);
            findBoundingBoxes(dt);
            updateControllers(dt, elapsedSeconds);
            updateBeforeCollisionDetectionUpdateables(dt, elapsedSeconds);
            updateCollisionDetectionDiscrete(dt);
            solveVelocities(dt, elapsedSeconds);
            updateControllersPrePositionUpdate(dt, elapsedSeconds);
            updatePositionsDiscrete(dt);
            updateBufferedStates();
            updateActivity(dt);
            updateEndOfUpdateUpdateables(dt, elapsedSeconds);
            dispatchEvents();
        }

        internal void updateCCD(float elapsedSeconds)
        {
            float dt = simulationSettings.timeStep;
            if (dt == 0.00d)
            {
                dt = 0.02F;
            }
            dt *= simulationSettings.timeScale;
            manageDynamicIterations(elapsedSeconds);
            sleepPulse();
            writeBufferedStates();
            applyForces(dt);
            updateDuringForcesUpdateables(dt, elapsedSeconds);
            findBoundingBoxes(dt);
            updateControllers(dt, elapsedSeconds);
            updateBeforeCollisionDetectionUpdateables(dt, elapsedSeconds);
            updateCollisionDetectionCCD(dt);

            // i believe the collisions above are more or less ok.. the problems involve the following
            solveVelocities(dt, elapsedSeconds);
            updateControllersPrePositionUpdate(dt, elapsedSeconds);
            updateContinuousMotion(dt);
            updatePositionsCCD(dt);
            updateBufferedStates();
            updateActivity(dt);
            updateEndOfUpdateUpdateables(dt, elapsedSeconds);
            dispatchEvents();
        }

        internal bool isNonContinuousDetectionPair(Controller controller)
        {
            PhysicsBody entity1 = controller.colliderA;
            PhysicsBody entity2 = controller.colliderB;
            if ((entity1.myIsActive || entity2.myIsActive) &&
                ((entity1.myIsTangible && entity2.myIsTangible) &&
                 (simulationSettings.useContinuousDetectionAgainstDetectors ||
                  (!entity1.myIsDetector && !entity2.myIsDetector))))
            {
                if (simulationSettings.useContinuousDetectionAgainstMovingKinematics)
                {
                    return false;
                }
                if (entity1.isPhysicallySimulated ||
                    ((entity1.myInternalLinearVelocity.IsNullOrEmpty()) &&
                     (entity1.myInternalAngularVelocity.IsNullOrEmpty())))
                {
                    if (entity2.isPhysicallySimulated)
                    {
                        return false;
                    }
                    if (entity2.myInternalLinearVelocity.IsNullOrEmpty())
                    {
                        return !entity2.myInternalAngularVelocity.IsNullOrEmpty();
                    }
                    return true;
                }
            }
            return true;
        }

        public void addToRemovalList(Updateable toRemove)
        {
            updateablesToRemove.Add(toRemove);
        }

        public void addToRemovalList(CombinedUpdateable toRemove)
        {
            combinedUpdateablesToRemove.Add(toRemove);
        }

        private void dispatchEvents()
        {
            foreach (PhysicsBody item in eventfulEntities)
            {
                dispatchQueue.Enqueue(item);
            }

            while (dispatchQueue.Count > 0)
            {
                dispatchQueue.Dequeue().dispatchEvents();
            }
        }

        private void updateDuringForcesUpdateables(float dt, float timeSinceLastFrame)
        {
            lock (lockerUpdateables)
            {
                foreach (Updateable updateable1 in updateables)
                {
                    if (updateable1.isUpdating)
                    {
                        updateable1.updateDuringForces(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (CombinedUpdateable updateable2 in combinedUpdateables)
                {
                    if (updateable2.isUpdating)
                    {
                        updateable2.updateDuringForces(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (Updateable item in updateablesToRemove)
                {
                    updateables.Remove(item);
                }
                foreach (CombinedUpdateable updateable4 in combinedUpdateablesToRemove)
                {
                    combinedUpdateables.Remove(updateable4);
                }
                updateablesToRemove.Clear();
                combinedUpdateablesToRemove.Clear();
            }
        }

        private void updateBeforeCollisionDetectionUpdateables(float dt, float timeSinceLastFrame)
        {
            lock (lockerUpdateables)
            {
                foreach (Updateable updateable1 in updateables)
                {
                    if (updateable1.isUpdating)
                    {
                        updateable1.updateBeforeCollisionDetection(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (CombinedUpdateable updateable2 in combinedUpdateables)
                {
                    if (updateable2.isUpdating)
                    {
                        updateable2.updateBeforeCollisionDetection(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (Updateable item in updateablesToRemove)
                {
                    updateables.Remove(item);
                }
                foreach (CombinedUpdateable updateable4 in combinedUpdateablesToRemove)
                {
                    combinedUpdateables.Remove(updateable4);
                }
                updateablesToRemove.Clear();
                combinedUpdateablesToRemove.Clear();
            }
        }

        private void updateEndOfUpdateUpdateables(float dt, float timeSinceLastFrame)
        {
            lock (lockerUpdateables)
            {
                foreach (Updateable updateable1 in updateables)
                {
                    if (updateable1.isUpdating)
                    {
                        updateable1.updateAtEndOfUpdate(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (CombinedUpdateable updateable2 in combinedUpdateables)
                {
                    if (updateable2.isUpdating)
                    {
                        updateable2.updateAtEndOfUpdate(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (Updateable item in updateablesToRemove)
                {
                    updateables.Remove(item);
                }
                foreach (CombinedUpdateable updateable4 in combinedUpdateablesToRemove)
                {
                    combinedUpdateables.Remove(updateable4);
                }
                updateablesToRemove.Clear();
                combinedUpdateablesToRemove.Clear();
            }
        }

        private void updateEndOfFrameUpdateables(float dt, float timeSinceLastFrame)
        {
            lock (lockerUpdateables)
            {
                foreach (Updateable updateable1 in updateables)
                {
                    if (updateable1.isUpdating)
                    {
                        updateable1.updateAtEndOfFrame(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (CombinedUpdateable updateable2 in combinedUpdateables)
                {
                    if (updateable2.isUpdating)
                    {
                        updateable2.updateAtEndOfFrame(dt, simulationSettings.timeScale, timeSinceLastFrame);
                    }
                }
                foreach (Updateable item in updateablesToRemove)
                {
                    updateables.Remove(item);
                }
                foreach (CombinedUpdateable updateable4 in combinedUpdateablesToRemove)
                {
                    combinedUpdateables.Remove(updateable4);
                }
                updateablesToRemove.Clear();
                combinedUpdateablesToRemove.Clear();
            }
        }

        public void getContacts(List<Contact> contacts)
        {
            lock (lockerControllers)
            {
                lock (lockerEntities)
                {
                    foreach (Controller controller1 in controllers)
                    {
                        foreach (Contact item in controller1.contacts)
                        {
                            contacts.Add(item);
                        }
                    }
                }
            }
        }

        #region Unused rayCast?  Hrm..
        //public bool rayCast(Vector3d origin, Vector3d direction, float maximumLength, bool withMargin,
        //                    out PhysicsBody hitEntity, out Vector3d hitLocation, out Vector3d hitNormal, out double toi)
        //{
        //    PhysicsBody item;
        //    Vector3d vector1;
        //    Vector3d vector2;
        //    double single1;
        //    bool flag1 = false;
        //    hitEntity = null;
        //    hitLocation = Toolbox.noVector;
        //    hitNormal = Toolbox.noVector;
        //    toi = double.NegativeInfinity;
        //    List<PhysicsBody> list = ResourcePool.getEntityList();
        //    List<Vector3d> list2 = ResourcePool.getVectorList();
        //    List<Vector3d> list3 = ResourcePool.getVectorList();
        //    List<double> list4 = ResourcePool.getFloatList();
        //    lock (lockerEntities)
        //    {
        //        if (myBroadPhase.rayCast(origin, direction, maximumLength, false, out item, out vector1, out vector2,
        //                                 out single1))
        //        {
        //            list.Add(item);
        //            list2.Add(vector1);
        //            list3.Add(vector2);
        //            list4.Add(single1);
        //        }
        //    }
        //    lock (lockerUpdateables)
        //    {
        //        foreach (RayCastableContainer container1 in rayCastableUpdateables)
        //        {
        //            if (container1.rayCast(origin, direction, maximumLength, withMargin, out item, out vector1,
        //                                   out vector2, out single1))
        //            {
        //                list.Add(item);
        //                list2.Add(vector1);
        //                list3.Add(vector2);
        //                list4.Add(single1);
        //            }
        //        }
        //        foreach (RayCastableContainerWithoutMargins margins1 in rayCastableUpdateablesWithoutMargins)
        //        {
        //            if (margins1.rayCast(origin, direction, maximumLength, out item, out vector1, out vector2,
        //                                 out single1))
        //            {
        //                list.Add(item);
        //                list2.Add(vector1);
        //                list3.Add(vector2);
        //                list4.Add(single1);
        //            }
        //        }
        //    }
        //    if (list2.Count > 0)
        //    {
        //        flag1 = true;
        //        double single2 = double.PositiveInfinity;
        //        int num1 = 0;
        //        for (int i = 0; i < list4.Count; i++)
        //        {
        //            if (list4[i] < single2)
        //            {
        //                num1 = i;
        //                single2 = list4[i];
        //            }
        //        }
        //        hitEntity = list[num1];
        //        hitLocation = list2[num1];
        //        hitNormal = list3[num1];
        //        toi = list4[num1];
        //    }
        //    ResourcePool.giveBack(list);
        //    ResourcePool.giveBack(list3);
        //    ResourcePool.giveBack(list2);
        //    ResourcePool.giveBack(list4);
        //    return flag1;
        //}

        //public bool rayCast(Vector3d origin, Vector3d direction, float maximumLength, bool withMargin,
        //                    List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals,
        //                    List<double> tois)
        //{
        //    bool flag1 = false;
        //    lock (lockerEntities)
        //    {
        //        if (myBroadPhase.rayCast(origin, direction, maximumLength, false, hitEntities, hitLocations, hitNormals,
        //                                 tois))
        //        {
        //            flag1 = true;
        //        }
        //    }
        //    lock (lockerUpdateables)
        //    {
        //        foreach (RayCastableContainer container1 in rayCastableUpdateables)
        //        {
        //            if (container1.rayCast(origin, direction, maximumLength, withMargin, hitEntities, hitLocations,
        //                                   hitNormals, tois))
        //            {
        //                flag1 = true;
        //            }
        //        }
        //        foreach (RayCastableContainerWithoutMargins margins1 in rayCastableUpdateablesWithoutMargins)
        //        {
        //            if (margins1.rayCast(origin, direction, maximumLength, hitEntities, hitLocations, hitNormals, tois))
        //            {
        //                flag1 = true;
        //            }
        //        }
        //        return flag1;
        //    }
        //    return flag1;
        //}

        //public bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin)
        //{
        //    Vector3d vector1;
        //    Vector3d vector2;
        //    double single1;
        //    PhysicsBody entity1;
        //    lock (lockerEntities)
        //    {
        //        if (myBroadPhase.rayCast(origin, direction, maximumLength, false, out entity1, out vector1, out vector2,
        //                                 out single1))
        //        {
        //            return true;
        //        }
        //    }
        //    lock (lockerUpdateables)
        //    {
        //        foreach (RayCastableContainer container1 in rayCastableUpdateables)
        //        {
        //            if (container1.rayCast(origin, direction, maximumLength, withMargin, out entity1, out vector1,
        //                                   out vector2, out single1))
        //            {
        //                return true;
        //            }
        //        }
        //        foreach (RayCastableContainerWithoutMargins margins1 in rayCastableUpdateablesWithoutMargins)
        //        {
        //            if (margins1.rayCast(origin, direction, maximumLength, out entity1, out vector1, out vector2,
        //                                 out single1))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
#endregion
    }
}