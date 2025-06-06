
using System.Collections.Generic;
using Keystone.Physics.Constraints;

namespace Keystone.Physics
{
    public class SimulationIsland
    {
        // Instance Fields
        internal Space mySpace;
        public HashSet<PhysicsBody> entities;
        internal bool isDeactivationCandidate;
        internal bool isActive;
        internal int numDeactivationCandidatesContained;

        // Constructors
        internal SimulationIsland(Space space)
        {
            isActive = true;
            mySpace = space;
            entities = new HashSet<PhysicsBody>();
        }

        // Properties
        public bool isCurrentlyActive
        {
            get { return isActive; }
        }

        public Space space
        {
            get { return mySpace; }
        }

        // Methods
        internal void setup(Space sp)
        {
            mySpace = sp;
        }

        internal void add(PhysicsBody e)
        {
            if (e.isSubBodyOfCompound)
            {
                add(e.compoundBody);
            }
            else
            {
                entities.Add(e);
                if (e.isDeactivationCandidate)
                {
                    numDeactivationCandidatesContained++;
                }
                e.simulationIsland = this;
            }
        }

        internal void addSubBodiesOf(CompoundBody b)
        {
            foreach (PhysicsBody entity1 in b.subBodies)
            {
                entities.Add(entity1);
                entity1.simulationIsland = this;
                if (entity1 is CompoundBody)
                {
                    addSubBodiesOf((CompoundBody) entity1);
                }
            }
        }

        internal void remove(PhysicsBody e)
        {
            if (e.isSubBodyOfCompound)
            {
                remove(e.compoundBody);
            }
            else
            {
                entities.Remove(e);
                if (e.isDeactivationCandidate)
                {
                    numDeactivationCandidatesContained--;
                }
                e.simulationIsland = null;
                if (entities.Count == 0)
                {
                    e.space.simulationIslands.Remove(this);
                    ResourcePool.giveBack(this);
                }
            }
        }

        internal void removeSubBodiesOf(CompoundBody b)
        {
            foreach (PhysicsBody entity1 in b.subBodies)
            {
                entities.Remove(entity1);
                entity1.simulationIsland = null;
                if (entity1 is CompoundBody)
                {
                    removeSubBodiesOf((CompoundBody) entity1);
                }
            }
        }

        public void tryToDeactivate()
        {
            lock (entities)
            {
                if (numDeactivationCandidatesContained != entities.Count)
                {
                    return;
                }
                isActive = false;

                foreach (PhysicsBody ent in entities)
                    ent.myIsActive = false;
            }
        }

        public void activate()
        {
            lock (entities)
            {
                isActive = true;
                foreach (PhysicsBody ent in entities)
                {
                    ent.myIsActive = true;
                    ent.isDeactivationCandidate = false;
                    ent.timeBelowLinearVelocityThreshold = 0.00f;
                    ent.timeBelowAngularVelocityThreshold = 0.00f;
                }
                numDeactivationCandidatesContained = 0;
            }
        }

        internal static void replace(PhysicsBody e, SimulationIsland s)
        {
            if (e.isSubBodyOfCompound)
            {
                replace(e.compoundBody, s);
            }
            else
            {
                e.simulationIsland.remove(e);
                s.add(e);
            }
        }

        internal static void mergeWith(SimulationIsland s1, SimulationIsland s2)
        {
            if (s2.isActive && !s1.isActive)
            {
                s1.activate();
            }
            if (s1.isActive && !s2.isActive)
            {
                s2.activate();
            }

            foreach (PhysicsBody ent in s2.entities)
                s1.add(ent);

            s1.mySpace.simulationIslands.Remove(s2);
            ResourcePool.giveBack(s2);
        }

        internal static void trySplit(PhysicsBody a, PhysicsBody b)
        {
            if (a.isPhysicallySimulated && b.isPhysicallySimulated)
            {
                if (a.isSubBodyOfCompound || b.isSubBodyOfCompound)
                {
                    if (a.isSubBodyOfCompound && !b.isSubBodyOfCompound)
                    {
                        trySplit(a.compoundBody, b);
                    }
                    else if (!a.isSubBodyOfCompound && b.isSubBodyOfCompound)
                    {
                        trySplit(a, b.compoundBody);
                    }
                    else
                    {
                        if (a.isSubBodyOfCompound && b.isSubBodyOfCompound)
                        {
                            trySplit(a.compoundBody, b.compoundBody);
                        }
                    }
                }
                else
                {
                    bool flag1 = a is CompoundBody;
                    bool flag2 = b is CompoundBody;
                    if ((!flag1 && (a.controllers.Count == 0)) && (a.constraints.Count == 0))
                    {
                        SimulationIsland item = ResourcePool.getSimulationIsland(a.space);
                        a.space.simulationIslands.Add(item);
                        replace(a, item);
                    }
                    else
                    {
                        if ((!flag2 && (b.controllers.Count == 0)) && (b.constraints.Count == 0))
                        {
                            SimulationIsland s = ResourcePool.getSimulationIsland(a.space);
                            b.space.simulationIslands.Add(s);
                            replace(b, s);
                        }
                        else
                        {
                            Queue<PhysicsBody> queue = ResourcePool.getEntityQueue();
                            Queue<PhysicsBody> queue2 = ResourcePool.getEntityQueue();
                            List<PhysicsBody> list1 = ResourcePool.getEntityList();
                            List<PhysicsBody> list2 = ResourcePool.getEntityList();
                            if (flag1)
                            {
                                List<PhysicsBody> children = ResourcePool.getEntityList();
                                ((CompoundBody) a).getAllRealChildren(children);
                                foreach (PhysicsBody entity1 in children)
                                {
                                    queue.Enqueue(entity1);
                                    entity1.internalIterationFlag = 1;
                                    list1.Add(entity1);
                                }
                                ResourcePool.giveBack(children);
                            }
                            else
                            {
                                queue.Enqueue(a);
                                a.internalIterationFlag = 1;
                                list1.Add(a);
                            }
                            if (flag2)
                            {
                                List<PhysicsBody> list = ResourcePool.getEntityList();
                                ((CompoundBody) b).getAllRealChildren(list);
                                foreach (PhysicsBody entity2 in list)
                                {
                                    queue2.Enqueue(entity2);
                                    entity2.internalIterationFlag = 2;
                                    list2.Add(entity2);
                                }
                                ResourcePool.giveBack(list);
                            }
                            else
                            {
                                queue2.Enqueue(b);
                                list2.Add(b);
                                b.internalIterationFlag = 2;
                            }
                            while ((queue.Count != 0) && (queue2.Count != 0))
                            {
                                PhysicsBody entity3 = queue.Dequeue();
                                PhysicsBody entity4 = queue2.Dequeue();
                                foreach (Controller controller1 in entity3.controllers)
                                {
                                    if ((controller1.colliderA.internalIterationFlag != 1) &&
                                        controller1.colliderA.isPhysicallySimulated)
                                    {
                                        if (controller1.colliderA.internalIterationFlag != 2)
                                        {
                                            controller1.colliderA.internalIterationFlag = 1;
                                            list1.Add(controller1.colliderA);
                                            queue.Enqueue(controller1.colliderA);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity5 in list1)
                                        {
                                            entity5.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity6 in list2)
                                        {
                                            entity6.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                    if ((controller1.colliderB.internalIterationFlag != 1) &&
                                        controller1.colliderB.isPhysicallySimulated)
                                    {
                                        if (controller1.colliderB.internalIterationFlag != 2)
                                        {
                                            controller1.colliderB.internalIterationFlag = 1;
                                            list1.Add(controller1.colliderB);
                                            queue.Enqueue(controller1.colliderB);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity7 in list1)
                                        {
                                            entity7.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity8 in list2)
                                        {
                                            entity8.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                }
                                foreach (Constraint constraint1 in entity3.constraints)
                                {
                                    if ((constraint1.connectionA.internalIterationFlag != 1) &&
                                        constraint1.connectionA.isPhysicallySimulated)
                                    {
                                        if (constraint1.connectionA.internalIterationFlag != 2)
                                        {
                                            constraint1.connectionA.internalIterationFlag = 1;
                                            list1.Add(constraint1.connectionA);
                                            queue.Enqueue(constraint1.connectionA);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity9 in list1)
                                        {
                                            entity9.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity10 in list2)
                                        {
                                            entity10.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                    if ((constraint1.connectionB.internalIterationFlag != 1) &&
                                        constraint1.connectionB.isPhysicallySimulated)
                                    {
                                        if (constraint1.connectionB.internalIterationFlag != 2)
                                        {
                                            constraint1.connectionB.internalIterationFlag = 1;
                                            list1.Add(constraint1.connectionB);
                                            queue.Enqueue(constraint1.connectionB);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity11 in list1)
                                        {
                                            entity11.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity12 in list2)
                                        {
                                            entity12.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                }
                                foreach (Controller controller2 in entity4.controllers)
                                {
                                    if ((controller2.colliderA.internalIterationFlag != 2) &&
                                        controller2.colliderA.isPhysicallySimulated)
                                    {
                                        if (controller2.colliderA.internalIterationFlag != 1)
                                        {
                                            controller2.colliderA.internalIterationFlag = 2;
                                            list2.Add(controller2.colliderA);
                                            queue2.Enqueue(controller2.colliderA);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity13 in list1)
                                        {
                                            entity13.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity14 in list2)
                                        {
                                            entity14.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                    if ((controller2.colliderB.internalIterationFlag != 2) &&
                                        controller2.colliderB.isPhysicallySimulated)
                                    {
                                        if (controller2.colliderB.internalIterationFlag != 1)
                                        {
                                            controller2.colliderB.internalIterationFlag = 2;
                                            list2.Add(controller2.colliderB);
                                            queue2.Enqueue(controller2.colliderB);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity15 in list1)
                                        {
                                            entity15.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity16 in list2)
                                        {
                                            entity16.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                }
                                foreach (Constraint constraint2 in entity4.constraints)
                                {
                                    if ((constraint2.connectionA.internalIterationFlag != 2) &&
                                        constraint2.connectionA.isPhysicallySimulated)
                                    {
                                        if (constraint2.connectionA.internalIterationFlag != 1)
                                        {
                                            constraint2.connectionA.internalIterationFlag = 2;
                                            list2.Add(constraint2.connectionA);
                                            queue2.Enqueue(constraint2.connectionA);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity17 in list1)
                                        {
                                            entity17.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity18 in list2)
                                        {
                                            entity18.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                    if ((constraint2.connectionB.internalIterationFlag != 2) &&
                                        constraint2.connectionB.isPhysicallySimulated)
                                    {
                                        if (constraint2.connectionB.internalIterationFlag != 1)
                                        {
                                            constraint2.connectionB.internalIterationFlag = 2;
                                            list2.Add(constraint2.connectionB);
                                            queue2.Enqueue(constraint2.connectionB);
                                            continue;
                                        }
                                        foreach (PhysicsBody entity19 in list1)
                                        {
                                            entity19.internalIterationFlag = 0;
                                        }
                                        foreach (PhysicsBody entity20 in list2)
                                        {
                                            entity20.internalIterationFlag = 0;
                                        }
                                        ResourcePool.giveBack(list1);
                                        ResourcePool.giveBack(list2);
                                        ResourcePool.giveBack(queue);
                                        ResourcePool.giveBack(queue2);
                                        return;
                                    }
                                }
                            }
                            SimulationIsland island3 = ResourcePool.getSimulationIsland(a.space);
                            a.space.simulationIslands.Add(island3);
                            if (list1.Count < list2.Count)
                            {
                                for (int index = list1.Count - 1; index >= 0; index--)
                                {
                                    list1[index].internalIterationFlag = 0;
                                    if (list1[index].isSubBodyOfCompound)
                                    {
                                        PhysicsBody entity21 = list1[index].parent;
                                        list1.RemoveAt(index);
                                        if (!list1.Contains(entity21))
                                        {
                                            list1.Add(entity21);
                                        }
                                    }
                                }
                                foreach (PhysicsBody e in list1)
                                {
                                    replace(e, island3);
                                }
                                foreach (PhysicsBody entity23 in list2)
                                {
                                    entity23.internalIterationFlag = 0;
                                }
                            }
                            else
                            {
                                for (int i = list2.Count - 1; i >= 0; i--)
                                {
                                    list2[i].internalIterationFlag = 0;
                                    if (list2[i].isSubBodyOfCompound)
                                    {
                                        PhysicsBody entity24 = list2[i].parent;
                                        list2.RemoveAt(i);
                                        if (!list2.Contains(entity24))
                                        {
                                            list2.Add(entity24);
                                        }
                                    }
                                }
                                foreach (PhysicsBody entity25 in list2)
                                {
                                    replace(entity25, island3);
                                }
                                foreach (PhysicsBody entity26 in list1)
                                {
                                    entity26.internalIterationFlag = 0;
                                }
                            }
                            ResourcePool.giveBack(list1);
                            ResourcePool.giveBack(list2);
                            ResourcePool.giveBack(queue);
                            ResourcePool.giveBack(queue2);
                        }
                    }
                }
            }
        }

        private static void compileLists(List<Controller> controllers, List<Constraint> constraints, PhysicsBody e)
        {
            foreach (Controller item in e.controllers)
            {
                controllers.Add(item);
            }
            foreach (Constraint constraint1 in e.constraints)
            {
                constraints.Add(constraint1);
            }
            if (e is CompoundBody)
            {
                foreach (PhysicsBody entity1 in ((CompoundBody) e).subBodies)
                {
                    // recursive
                    compileLists(controllers, constraints, entity1);
                }
            }
        }
    }
}