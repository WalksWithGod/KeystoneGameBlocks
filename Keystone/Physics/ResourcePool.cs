using Keystone.Physics;
using Keystone.Physics.BroadPhases;
using Keystone.Physics.Constraints;
//using Keystone.Physics.DataStructures;
using Keystone.Physics.Entities;
using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics
{
	public sealed class ResourcePool
    {
        // Statics
        private static Queue<List<bool>> availableBoolLists;
        private static Queue<List<int>> availableIntLists;
        private static Queue<Queue<int>> availableIntQueues;
        private static Queue<List<float>> availableFloatLists;
        private static Queue<List<Vector3d>> availableVectorLists;
        private static Queue<List<PhysicsBody>> availableEntityLists;
        private static Queue<Queue<PhysicsBody>> availableEntityQueues;
      //  private static Queue<List<Int3>> availableInt3Lists;
        private static Queue<List<Triangle>> availableStaticTriangleLists;
        private static Queue<List<Force>> availableForceLists;
        private static Queue<List<Contact>> availableContactLists;
        private static Queue<Contact> availableContacts;
     //   private static Queue<Int3> availableInt3s;
        internal static Queue<Controller> availableControllers;
        private static Queue<List<Controller>> availableControllerLists;
        private static Queue<List<Constraint>> availableConstraintLists;
        private static Queue<Triangle> availableStaticTriangles;
        private static Queue<SimulationIsland> availableSimulationIslands;
     //   private static Queue<List<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>> availableTriangleBoundingVolumeHierarchyNodeLists;
     //   private static Queue<Queue<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>> availableTriangleBoundingVolumeHierarchyNodeQueues;
        private static Queue<ControllerTableKey> availableControllerTableKeys;
    //    private static Queue<SAPEndpoint> availableSAPEndpoints;
        //private static Queue<Multiset> availableMultisets;
        //private static Queue<MultiSetEntry> availableMultiSetEntries;
        //private static Queue<SAPInterval> availableSAPIntervals;
        //private static Queue<List<SAPInterval>> availableSAPIntervalLists;
        public static int defaultBoolCapacity;
        public static int defaultIntCapacity;
        public static int defaultFloatCapacity;
        public static int defaultVectorCapacity;
        public static int defaultEntityCapacity;
        public static int defaultInt3Capacity;
        public static int defaultStaticTriangleCapacity;
        public static int defaultForceCapacity;
        public static int defaultContactCapacity;
        public static int defaultControllerCapacity;
        public static int defaultConstraintCapacity;
        internal static int defaultTriangleHierarchyNodeCapacity;
        internal static int defaultTriangleHierarchyNodeQueueCapacity;
        internal static int defaultTriangleBoundingVolumeHierarchyNodeCapacity;
        internal static int defaultTriangleBoundingVolumeHierarchyNodeQueueCapacity;
        internal static int defaultSAPIntervalCapacity;
		// Constructors
		static ResourcePool ()
		{
			availableBoolLists = new Queue<List<bool>>(2);
			availableIntLists = new Queue<List<int>>(2);
			availableIntQueues = new Queue<Queue<int>>(2);
			availableFloatLists = new Queue<List<float>>(2);
			availableVectorLists = new Queue<List<Vector3d>>(7);
			availableEntityLists = new Queue<List<PhysicsBody>>(0x1000);
			availableEntityQueues = new Queue<Queue<PhysicsBody>>(2);
			
			availableStaticTriangleLists = new Queue<List<Triangle>>(0x1000);
			availableForceLists = new Queue<List<Force>>(1);
			availableContactLists = new Queue<List<Contact>>(1);
			availableContacts = new Queue<Contact>(0x4000);

			availableControllers = new Queue<Controller>(0x1000);
			availableControllerLists = new Queue<List<Controller>>(10);
			availableConstraintLists = new Queue<List<Constraint>>(10);
			availableStaticTriangles = new Queue<Triangle>(0x100);
			availableSimulationIslands = new Queue<SimulationIsland>(0x100);
            availableControllerTableKeys = new Queue<ControllerTableKey>(0x64);
            //availableInt3Lists = new Queue<List<Int3>>(2);
            //availableInt3s = new Queue<Int3>(0x2000);
            //availableTriangleBoundingVolumeHierarchyNodeLists = new Queue<List<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>>(2);
            //availableTriangleBoundingVolumeHierarchyNodeQueues = new Queue<Queue<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>>(2);

            //availableSAPEndpoints = new Queue<SAPEndpoint>(0x64);
            //availableMultisets = new Queue<Multiset>(5);
            //availableMultiSetEntries = new Queue<MultiSetEntry>(0x64);
            //availableSAPIntervals = new Queue<SAPInterval>(0x64);
            //availableSAPIntervalLists = new Queue<List<SAPInterval>>(10);
			defaultBoolCapacity = 4;
			defaultIntCapacity = 4;
			defaultFloatCapacity = 4;
			defaultVectorCapacity = 4;
			defaultEntityCapacity = 10;
			defaultInt3Capacity = 0x40;
			defaultStaticTriangleCapacity = 2;
			defaultForceCapacity = 2;
			defaultContactCapacity = 4;
			defaultControllerCapacity = 6;
			defaultConstraintCapacity = 6;
			defaultTriangleHierarchyNodeCapacity = 0x14;
			defaultTriangleHierarchyNodeQueueCapacity = 10;
			defaultTriangleBoundingVolumeHierarchyNodeCapacity = 0x14;
			defaultTriangleBoundingVolumeHierarchyNodeQueueCapacity = 10;
			defaultSAPIntervalCapacity = 10;
		}
		
		
		// Methods
		public static List<bool> getBoolList ()
		{
			List<bool> list1;
			lock (availableBoolLists)
			{
				if (availableBoolLists.Count > 0)
				{
					return availableBoolLists.Dequeue();
				}
				list1 = new List<bool>(defaultBoolCapacity);
			}
			return list1;
		}
		
		public static List<int> getIntList ()
		{
			List<int> list1;
			lock (availableIntLists)
			{
				if (availableIntLists.Count > 0)
				{
					return availableIntLists.Dequeue();
				}
				list1 = new List<int>(defaultIntCapacity);
			}
			return list1;
		}
		
		public static Queue<int> getIntQueue ()
		{
			Queue<int> queue1;
			lock (availableIntQueues)
			{
				if (availableIntQueues.Count > 0)
				{
					return availableIntQueues.Dequeue();
				}
				queue1 = new Queue<int>(defaultIntCapacity);
			}
			return queue1;
		}
		
		public static List<float> getFloatList ()
		{
			List<float> list1;
			lock (availableFloatLists)
			{
				if (availableFloatLists.Count > 0)
				{
					return availableFloatLists.Dequeue();
				}
				list1 = new List<float>(defaultFloatCapacity);
			}
			return list1;
		}
		
		public static List<Vector3d> getVectorList ()
		{
			List<Vector3d> list1;
			lock (availableVectorLists)
			{
				if (availableVectorLists.Count > 0)
				{
					return availableVectorLists.Dequeue();
				}
				list1 = new List<Vector3d>(defaultVectorCapacity);
			}
			return list1;
		}
		
		public static List<PhysicsBody> getEntityList ()
		{
			List<PhysicsBody> list1;
			lock (availableEntityLists)
			{
				if (availableEntityLists.Count > 0)
				{
					return availableEntityLists.Dequeue();
				}
				list1 = new List<PhysicsBody>(defaultEntityCapacity);
			}
			return list1;
		}
		
		public static Queue<PhysicsBody> getEntityQueue ()
		{
			Queue<PhysicsBody> queue1;
			lock (availableEntityQueues)
			{
				if (availableEntityQueues.Count > 0)
				{
					return availableEntityQueues.Dequeue();
				}
				queue1 = new Queue<PhysicsBody>(defaultEntityCapacity);
			}
			return queue1;
		}
		

		
		public static List<Triangle> getStaticTriangleList ()
		{
			List<Triangle> list1;
			lock (availableStaticTriangleLists)
			{
				if (availableStaticTriangleLists.Count > 0)
				{
					return availableStaticTriangleLists.Dequeue();
				}
				list1 = new List<Triangle>(defaultStaticTriangleCapacity);
			}
			return list1;
		}
		
		public static List<Force> getForceList ()
		{
			List<Force> list1;
			lock (availableForceLists)
			{
				if (availableForceLists.Count > 0)
				{
					return availableForceLists.Dequeue();
				}
				list1 = new List<Force>(defaultForceCapacity);
			}
			return list1;
		}
		
		public static List<Contact> getContactList ()
		{
			List<Contact> list1;
			lock (availableContactLists)
			{
				if (availableContactLists.Count > 0)
				{
					return availableContactLists.Dequeue();
				}
				list1 = new List<Contact>(defaultContactCapacity);
			}
			return list1;
		}
		
		public static Contact getContact (Vector3d pos, Vector3d norm, PhysicsBody collider, PhysicsBody collidee, PhysicsBody parentA, PhysicsBody parentB, float depth)
		{
			Contact contact2;
			lock (availableContacts)
			{
				if (availableContacts.Count > 0)
				{
					Contact contact1 = availableContacts.Dequeue();
					contact1.setup(pos, norm, collider, collidee, parentA, parentB, depth);
					return contact1;
				}
				contact2 = new Contact(pos, norm, collider, collidee, parentA, parentB, depth);
			}
			return contact2;
		}

		
		public static Controller getController (PhysicsBody a, PhysicsBody b, Space s)
		{
			Controller controller2;
			lock (availableControllers)
			{
				if (availableControllers.Count > 0)
				{
					Controller controller1 = availableControllers.Dequeue();
					controller1.setup(a, b, s);
					return controller1;
				}
				controller2 = new Controller(a, b, s);
			}
			return controller2;
		}
		
		public static List<Controller> getControllerList ()
		{
			List<Controller> list1;
			lock (availableControllerLists)
			{
				if (availableControllerLists.Count > 0)
				{
					return availableControllerLists.Dequeue();
				}
				list1 = new List<Controller>(defaultControllerCapacity);
			}
			return list1;
		}
		
		public static List<Constraint> getConstraintList ()
		{
			List<Constraint> list1;
			lock (availableConstraintLists)
			{
				if (availableConstraintLists.Count > 0)
				{
					return availableConstraintLists.Dequeue();
				}
				list1 = new List<Constraint>(defaultConstraintCapacity);
			}
			return list1;
		}
		
		public static Triangle getStaticTriangle (Vector3d v1, Vector3d v2, Vector3d v3)
		{
			Triangle triangle2;
			lock (availableStaticTriangles)
			{
				if (availableStaticTriangles.Count > 0)
				{
					Triangle triangle1 = availableStaticTriangles.Dequeue();
	// TODO: temp				triangle1.setup(v1, v2, v3);
					return triangle1;
				}
				triangle2 = new Triangle(v1, v2, v3);
			}
			return triangle2;
		}
		
		internal static SimulationIsland getSimulationIsland (Space space)
		{
			SimulationIsland island2;
			lock (availableSimulationIslands)
			{
				if (availableSimulationIslands.Count > 0)
				{
					SimulationIsland island1 = availableSimulationIslands.Dequeue();
					island1.setup(space);
					return island1;
				}
				island2 = new SimulationIsland(space);
			}
			return island2;
		}

        //public static List<Int3> getInt3List()
        //{
        //    List<Int3> list1;
        //    lock (availableInt3Lists)
        //    {
        //        if (availableInt3Lists.Count > 0)
        //        {
        //            return availableInt3Lists.Dequeue();
        //        }
        //        list1 = new List<Int3>(defaultInt3Capacity);
        //    }
        //    return list1;
        //}
        //public static Int3 getInt3(int x, int y, int z)
        //{
        //    Int3 num2;
        //    lock (availableInt3s)
        //    {
        //        if (availableInt3s.Count > 0)
        //        {
        //            Int3 num1 = availableInt3s.Dequeue();
        //            num1.X = x;
        //            num1.Y = y;
        //            num1.Z = z;
        //            return num1;
        //        }
        //        num2 = new Int3(x, y, z);
        //    }
        //    return num2;
        //}
        //internal static List<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode> getTriangleBoundingVolumeHierarchyNodeList ()
        //{
        //    List<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode> list1;
        //    lock (availableTriangleBoundingVolumeHierarchyNodeLists)
        //    {
        //        if (availableTriangleBoundingVolumeHierarchyNodeLists.Count > 0)
        //        {
        //            return availableTriangleBoundingVolumeHierarchyNodeLists.Dequeue();
        //        }
        //        list1 = new List<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>(defaultTriangleBoundingVolumeHierarchyNodeCapacity);
        //    }
        //    return list1;
        //}
		
        //internal static Queue<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode> getTriangleBoundingVolumeHierarchyNodeQueue ()
        //{
        //    Queue<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode> queue1;
        //    lock (availableTriangleBoundingVolumeHierarchyNodeQueues)
        //    {
        //        if (availableTriangleBoundingVolumeHierarchyNodeQueues.Count > 0)
        //        {
        //            return availableTriangleBoundingVolumeHierarchyNodeQueues.Dequeue();
        //        }
        //        queue1 = new Queue<TriangleBoundingVolumeHierarchy.TriangleBoundingVolumeHierarchyNode>(defaultTriangleBoundingVolumeHierarchyNodeQueueCapacity);
        //    }
        //    return queue1;
        //}
		
		internal static ControllerTableKey getControllerTableKey (PhysicsBody a, PhysicsBody b)
		{
			if (availableControllerTableKeys.Count > 0)
			{
				ControllerTableKey key1 = availableControllerTableKeys.Dequeue();
				key1.setup(a, b);
				return key1;
			}
			return new ControllerTableKey(a, b);
		}
		
        //internal static SAPEndpoint getSAPEndpoint ()
        //{
        //    if (availableSAPEndpoints.Count > 0)
        //    {
        //        return availableSAPEndpoints.Dequeue();
        //    }
        //    return new SAPEndpoint();
        //}
		
        //internal static Multiset getMultiset ()
        //{
        //    if (availableMultisets.Count > 0)
        //    {
        //        return availableMultisets.Dequeue();
        //    }
        //    return new Multiset();
        //}
		
        //internal static MultiSetEntry getMultiSetEntry (bool xAxis, bool yAxis, bool zAxis)
        //{
        //    if (availableMultiSetEntries.Count > 0)
        //    {
        //        MultiSetEntry entry1 = availableMultiSetEntries.Dequeue();
        //        entry1.xAxis = xAxis;
        //        entry1.yAxis = yAxis;
        //        entry1.zAxis = zAxis;
        //        return entry1;
        //    }
        //    return new MultiSetEntry(xAxis, yAxis, zAxis);
        //}
		
        //internal static SAPInterval getSAPInterval (SAPEndpoint minPoint, SAPEndpoint maxPoint, PhysicsBody physicsBody)
        //{
        //    if (availableSAPIntervals.Count > 0)
        //    {
        //        SAPInterval interval1 = availableSAPIntervals.Dequeue();
        //        interval1.setup(minPoint, maxPoint, physicsBody);
        //        return interval1;
        //    }
        //    return new SAPInterval(minPoint, maxPoint, physicsBody);
        //}
		
        //internal static List<SAPInterval> getSAPIntervalList ()
        //{
        //    if (availableSAPIntervalLists.Count > 0)
        //    {
        //        return availableSAPIntervalLists.Dequeue();
        //    }
        //    return new List<SAPInterval>(defaultSAPIntervalCapacity);
        //}
		
		public static void giveBack (List<bool> list)
		{
			lock (availableBoolLists)
			{
				list.Clear();
				availableBoolLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<int> list)
		{
			lock (availableIntLists)
			{
				list.Clear();
				availableIntLists.Enqueue(list);
			}
		}
		
		public static void giveBack (Queue<int> queue)
		{
			lock (availableIntQueues)
			{
				queue.Clear();
				availableIntQueues.Enqueue(queue);
			}
		}
		
		public static void giveBack (List<float> list)
		{
			lock (availableFloatLists)
			{
				list.Clear();
				availableFloatLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<Vector3d> list)
		{
			lock (availableVectorLists)
			{
				list.Clear();
				availableVectorLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<PhysicsBody> list)
		{
			lock (availableEntityLists)
			{
				list.Clear();
				availableEntityLists.Enqueue(list);
			}
		}
		
		public static void giveBack (Queue<PhysicsBody> queue)
		{
			lock (availableEntityQueues)
			{
				queue.Clear();
				availableEntityQueues.Enqueue(queue);
			}
		}
		
        //public static void giveBack (List<Int3> list)
        //{
        //    lock (availableInt3Lists)
        //    {
        //        list.Clear();
        //        availableInt3Lists.Enqueue(list);
        //    }
        //}
		
		public static void giveBack (List<Triangle> list)
		{
			lock (availableStaticTriangleLists)
			{
				list.Clear();
				availableStaticTriangleLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<Force> list)
		{
			lock (availableForceLists)
			{
				list.Clear();
				availableForceLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<Contact> list)
		{
			lock (availableContactLists)
			{
				list.Clear();
				availableContactLists.Enqueue(list);
			}
		}
		
		public static void giveBack (Contact contact)
		{
			lock (availableContacts)
			{
				availableContacts.Enqueue(contact);
			}
		}
		
        //public static void giveBack (Int3 int3)
        //{
        //    lock (availableInt3s)
        //    {
        //        availableInt3s.Enqueue(int3);
        //    }
        //}
		
		public static void giveBack (Controller controller)
		{
			lock (availableControllers)
			{
				availableControllers.Enqueue(controller);
			}
		}
		
		public static void giveBack (List<Controller> list)
		{
			lock (availableControllerLists)
			{
				list.Clear();
				availableControllerLists.Enqueue(list);
			}
		}
		
		public static void giveBack (List<Constraint> list)
		{
			lock (availableConstraintLists)
			{
				list.Clear();
				availableConstraintLists.Enqueue(list);
			}
		}
		
		public static void giveBack (Triangle staticTriangle)
		{
			lock (availableStaticTriangles)
			{
				availableStaticTriangles.Enqueue(staticTriangle);
			}
		}
		
		internal static void giveBack (SimulationIsland simulationIsland)
		{
			lock (availableSimulationIslands)
			{
				simulationIsland.isDeactivationCandidate = false;
				simulationIsland.isActive = true;
				simulationIsland.numDeactivationCandidatesContained = 0;
				simulationIsland.mySpace = null;
				simulationIsland.entities.Clear();
				availableSimulationIslands.Enqueue(simulationIsland);
			}
		}
		
		internal static void giveBack (ControllerTableKey key)
		{
			availableControllerTableKeys.Enqueue(key);
		}
		
        //internal static void giveBack (SAPEndpoint endpoint)
        //{
        //    availableSAPEndpoints.Enqueue(endpoint);
        //}
		
        //internal static void giveBack (Multiset set)
        //{
        //    set.overlaps.Clear();
        //    availableMultisets.Enqueue(set);
        //}
		
        //internal static void giveBack (MultiSetEntry setEntry)
        //{
        //    availableMultiSetEntries.Enqueue(setEntry);
        //}
		
        //internal static void giveBack (SAPInterval interval)
        //{
        //    interval.overlaps.overlaps.Clear();
        //    availableSAPIntervals.Enqueue(interval);
        //}
		
        //internal static void giveBack (List<SAPInterval> intervalList)
        //{
        //    intervalList.Clear();
        //    availableSAPIntervalLists.Enqueue(intervalList);
        //}
	
	}
}
