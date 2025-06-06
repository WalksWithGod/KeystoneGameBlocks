using Keystone.Physics;
using Keystone.Physics.Entities;
using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics.BroadPhases
{
	public class DynamicBinaryHierarchy : BroadPhase
	{

        // Statics
        public static float maximumAllowedVolumeFactor;
        public static float maximumChildEntityLoad;
        public static int maximumEntitiesInLeaves;

        // Instance Fields
        public DynamicBinaryHierarchyNode root;
        private Queue<DynamicBinaryHierarchyNode> aNodes;
        private Queue<DynamicBinaryHierarchyNode> bNodes;
        private Queue<DynamicBinaryHierarchyNode> dynamicHierarchyNodes;
		// Constructors
		public DynamicBinaryHierarchy ()
		{
			aNodes = new Queue<DynamicBinaryHierarchyNode>();
			bNodes = new Queue<DynamicBinaryHierarchyNode>();
			dynamicHierarchyNodes = new Queue<DynamicBinaryHierarchyNode>();
			root = new DynamicBinaryHierarchyNode(this);
		}
		
		static DynamicBinaryHierarchy ()
		{
			maximumAllowedVolumeFactor = 1.40F;
			maximumChildEntityLoad = 0.80F;
			maximumEntitiesInLeaves = 2;
		}
		
		
		// Methods
		public override void addEntity (PhysicsBody e)
		{
			root.addEntity(e);
			root.addControllersWithEntity(e);
		}
		
		public override void removeEntity (PhysicsBody e)
		{
			root.removeEntity(e);
		}
		
		public override void updateControllers (float dt, float timeSinceLastFrame)
		{
			lock (base.lockerBroadPhaseUpdating)
			{
				for (int i = base.space.controllers.Count - 1;i >= 0; i--)
				{
					bool flag1;
					Controller controller1 = base.space.controllers[i];

                    if (!controller1.colliderA.CollisionPrimitive.BoundingBox.Intersects(controller1.colliderB.CollisionPrimitive.BoundingBox))
					{
						base.space.removeController(i);
						i--;
					}
				}
				root.binaryUpdateNode();
				root.binaryCollideAgainst(root);
			}
		}
		
		private void updateNodes ()
		{
		}
		
		private void revalidateNodes ()
		{
		}
		
		private void updateCollisions ()
		{
			aNodes.Enqueue(root);
			bNodes.Enqueue(root);
			while (aNodes.Count > 0)
			{
				bool flag1;
				DynamicBinaryHierarchyNode node1 = aNodes.Dequeue();
				DynamicBinaryHierarchyNode item = bNodes.Dequeue();
				if (node1 == item)
				{
					if (node1.children.Count == 0)
					{
						for (int i = 0;i < (node1.entities.Count - 1); i++)
						{
							for (int j = i + 1;j < node1.entities.Count; j++)
							{
								if (base.isValidPair(node1.entities[i], node1.entities[j]))
								{
									base.addController(node1.entities[i], node1.entities[j]);
								}
							}
						}
						continue;
					}
					
					if (node1.children[0].boundingBox.Intersects( node1.children[1].boundingBox))
					{
						aNodes.Enqueue(node1.children[0]);
						bNodes.Enqueue(node1.children[1]);
					}
					aNodes.Enqueue(node1.children[0]);
					bNodes.Enqueue(node1.children[0]);
					aNodes.Enqueue(node1.children[1]);
					bNodes.Enqueue(node1.children[1]);
					continue;
				}
				if ((node1.children.Count > 0) && (item.children.Count > 0))
				{
					
					if (node1.children[0].boundingBox.Intersects(item.children[0].boundingBox))
					{
						aNodes.Enqueue(node1.children[0]);
						bNodes.Enqueue(item.children[0]);
					}
					
					if (node1.children[0].boundingBox.Intersects(item.children[1].boundingBox))
					{
						aNodes.Enqueue(node1.children[0]);
						bNodes.Enqueue(item.children[1]);
					}
					
					if (node1.children[1].boundingBox.Intersects(item.children[0].boundingBox))
					{
						aNodes.Enqueue(node1.children[1]);
						bNodes.Enqueue(item.children[0]);
					}
					
					if (node1.children[1].boundingBox.Intersects(item.children[1].boundingBox))
					{
						aNodes.Enqueue(node1.children[1]);
						bNodes.Enqueue(item.children[1]);
					}
					continue;
				}
				if (node1.children.Count > 0)
				{
					
					if (node1.children[0].boundingBox.Intersects(item.boundingBox))
					{
						aNodes.Enqueue(node1.children[0]);
						bNodes.Enqueue(item);
					}
					
					if (node1.children[1].boundingBox.Intersects(item.boundingBox))
					{
						aNodes.Enqueue(node1.children[1]);
						bNodes.Enqueue(item);
					}
					continue;
				}
				if (item.children.Count > 0)
				{
					
					if (node1.boundingBox.Intersects(item.children[0].boundingBox))
					{
						aNodes.Enqueue(node1);
						bNodes.Enqueue(item.children[0]);
					}
					
					if (node1.boundingBox.Intersects(item.children[1].boundingBox))
					{
						aNodes.Enqueue(node1);
						bNodes.Enqueue(item.children[1]);
					}
					continue;
				}
				for (int z = 0;z < node1.entities.Count; z++)
				{
					for (int k = 0;k < item.entities.Count; k++)
					{
						if (base.isValidPair(node1.entities[z], item.entities[k]))
						{
							base.addController(node1.entities[z], item.entities[k]);
						}
					}
				}
			}
		}
		
		public override void getEntities (BoundingBox box, List<PhysicsBody> entities)
		{
			lock (base.lockerBroadPhaseUpdating)
			{
				root.getEntities(ref box, entities);
			}
		}

        public override void getEntities(Keystone.Culling.ViewFrustum frustum, List<PhysicsBody> entities)
		{
			lock (base.lockerBroadPhaseUpdating)
			{
				root.getEntities(ref frustum, entities);
			}
		}
		
		public override void getEntities (BoundingSphere sphere, List<PhysicsBody> entities)
		{
			lock (base.lockerBroadPhaseUpdating)
			{
				root.getEntities(ref sphere, entities);
			}
		}

        public override bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals, List<double> tois)
		{
			lock (base.lockerBroadPhaseUpdating)
			{
				root.rayCast( new Ray(origin, direction), maximumLength, withMargin, hitEntities, hitLocations, hitNormals, tois);
			}
			if (hitEntities.Count > 0)
			{
				return true;
			}
			return false;
		}

        public override bool rayCast(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out PhysicsBody hitPhysicsBody, out Vector3d hitLocation, out Vector3d hitNormal, out double toi)
		{
			List<PhysicsBody> hitEntities = ResourcePool.getEntityList();
			List<Vector3d> hitLocations = ResourcePool.getVectorList();
			List<Vector3d> hitNormals = ResourcePool.getVectorList();
            List<double> tois = new List<double>(); // ResourcePool.getFloatList();
			lock (base.lockerBroadPhaseUpdating)
			{
				root.rayCast( new Ray(origin, direction), maximumLength, withMargin, hitEntities, hitLocations, hitNormals, tois);
			}
			int num1 = 0;
            double single1 = double.MaxValue;
			if (tois.Count > 0)
			{
				for (int i = 0;i < tois.Count; i++)
				{
					if (single1 > tois[i])
					{
						num1 = i;
						single1 = tois[i];
					}
				}
				hitPhysicsBody = hitEntities[num1];
				hitLocation = hitLocations[num1];
				hitNormal = hitNormals[num1];
				toi = tois[num1];
				ResourcePool.giveBack(hitEntities);
				ResourcePool.giveBack(hitLocations);
				ResourcePool.giveBack(hitNormals);
				//ResourcePool.giveBack(tois);
				return true;
			}
			hitPhysicsBody = null;
			hitLocation = Toolbox.noVector;
			hitNormal = Toolbox.noVector;
			toi = float.NaN;
			ResourcePool.giveBack(hitEntities);
			ResourcePool.giveBack(hitLocations);
			ResourcePool.giveBack(hitNormals);
			//ResourcePool.giveBack(tois);
			return false;
		}
		
		private DynamicBinaryHierarchyNode getNode ()
		{
			if (dynamicHierarchyNodes.Count > 0)
			{
				return dynamicHierarchyNodes.Dequeue();
			}
			return new DynamicBinaryHierarchyNode(this);
		}
		
		private void giveBack (DynamicBinaryHierarchyNode node)
		{
			node.entities.Clear();
			foreach (DynamicBinaryHierarchyNode node1 in node.children)
			{
				giveBack(node1);
			}
			node.children.Clear();
			dynamicHierarchyNodes.Enqueue(node);
		}
		

		
		// Nested Types
		public class DynamicBinaryHierarchyNode
		{

            			// Statics
			private static AxisComparer axisComparer;
			
			// Instance Fields
			public  BoundingBox boundingBox;
            internal double maximumAllowedVolume;
            internal double currentVolume;
			internal  List<DynamicBinaryHierarchyNode> children;
			internal  List<PhysicsBody> entities;
			internal  DynamicBinaryHierarchy hierarchy;
			
			// Nested Types
			public enum ComparerAxis
			{
				x = 0,
				y = 1,
				z = 2

            }
            
			// Constructors
			internal DynamicBinaryHierarchyNode (DynamicBinaryHierarchy hierarchyOwner)
			{
				children = new List<DynamicBinaryHierarchyNode>(2);
				entities = new List<PhysicsBody>();
				hierarchy = hierarchyOwner;
			}
			
			static DynamicBinaryHierarchyNode ()
			{
				DynamicBinaryHierarchyNode.axisComparer = new DynamicBinaryHierarchyNode.AxisComparer();
			}
			
			
			// Methods
			internal void addControllersWithEntity (PhysicsBody e)
			{
                if (e.CollisionPrimitive.BoundingBox.Intersects(boundingBox))
				{
					if (children.Count > 0)
					{
						foreach (DynamicBinaryHierarchyNode node1 in children)
						{
							node1.addControllersWithEntity(e);
						}
					}
					else
					{
						foreach (PhysicsBody b in entities)
						{
							if ((e != b) && hierarchy.isValidPair(e, b))
							{
								hierarchy.addController(e, b);
							}
						}
					}
				}
			}
			
			internal void binaryCollideAgainst (DynamicBinaryHierarchyNode node)
			{
				bool flag1;
				if (this == node)
				{
					if (children.Count == 0)
					{
						for (int i = 0;i < (entities.Count - 1); i++)
						{
							for (int j = i + 1;j < entities.Count; j++)
							{
								if (hierarchy.isValidPair(entities[i], entities[j]))
								{
									hierarchy.addController(entities[i], entities[j]);
								}
							}
						}
					}
					else
					{
						
						if (children[0].boundingBox.Intersects(children[1].boundingBox))
						{
							children[0].binaryCollideAgainst(children[1]);
						}
						children[0].binaryCollideAgainst(children[0]);
						children[1].binaryCollideAgainst(children[1]);
					}
				}
				else if ((children.Count > 0) && (node.children.Count > 0))
				{
					
					if (children[0].boundingBox.Intersects(node.children[0].boundingBox))
					{
						children[0].binaryCollideAgainst(node.children[0]);
					}

                    if (children[0].boundingBox.Intersects(node.children[1].boundingBox))
					{
						children[0].binaryCollideAgainst(node.children[1]);
					}
					
					if (children[1].boundingBox.Intersects(node.children[0].boundingBox))
					{
						children[1].binaryCollideAgainst(node.children[0]);
					}
					
					if (children[1].boundingBox.Intersects(node.children[1].boundingBox))
					{
						children[1].binaryCollideAgainst(node.children[1]);
					}
				}
				else if (children.Count > 0)
				{

                    if (children[0].boundingBox.Intersects(node.boundingBox))
					{
						children[0].binaryCollideAgainst(node);
					}
					
					if (children[1].boundingBox.Intersects(node.boundingBox))
					{
						children[1].binaryCollideAgainst(node);
					}
				}
				else if (node.children.Count > 0)
				{
					
					if (boundingBox.Intersects(node.children[0].boundingBox))
					{
						binaryCollideAgainst(node.children[0]);
					}
					
					if (boundingBox.Intersects(node.children[1].boundingBox))
					{
						binaryCollideAgainst(node.children[1]);
					}
				}
				else
				{
					for (int z = 0;z < entities.Count; z++)
					{
						for (int k = 0;k < node.entities.Count; k++)
						{
							if (hierarchy.isValidPair(entities[z], node.entities[k]))
							{
								hierarchy.addController(entities[z], node.entities[k]);
							}
						}
					}
				}
			}
			
			internal void collideAgainst (DynamicBinaryHierarchyNode node)
			{
				bool flag1;
				if (this == node)
				{
					if (children.Count == 0)
					{
						for (int i = 0;i < (entities.Count - 1); i++)
						{
							for (int j = i + 1;j < entities.Count; j++)
							{
								if (hierarchy.isValidPair(entities[i], entities[j]))
								{
									hierarchy.addController(entities[i], entities[j]);
								}
							}
						}
					}
					else
					{
						for (int z = 0;z < (children.Count - 1); z++)
						{
							for (int k = z + 1;k < children.Count; k++)
							{
								
								if (children[z].boundingBox.Intersects(children[k].boundingBox))
								{
									children[z].collideAgainst(children[k]);
								}
							}
						}
						for (int l = 0;l < children.Count; l++)
						{
							children[l].collideAgainst(children[l]);
						}
					}
				}
				else if ((children.Count > 0) && (node.children.Count > 0))
				{
					for (int a = 0;a < children.Count; a++)
					{
						for (int b = 0;b < node.children.Count; b++)
						{
							
							if (children[a].boundingBox.Intersects(node.children[b].boundingBox))
							{
								children[a].collideAgainst(node.children[b]);
							}
						}
					}
				}
				else if (children.Count > 0)
				{
					for (int c = 0;c < children.Count; c++)
					{
						
						if (children[c].boundingBox.Intersects(node.boundingBox))
						{
							children[c].collideAgainst(node);
						}
					}
				}
				else if (node.children.Count > 0)
				{
					for (int d = 0;d < node.children.Count; d++)
					{
						
						if (boundingBox.Intersects(node.children[d].boundingBox))
						{
							collideAgainst(node.children[d]);
						}
					}
				}
				else
				{
					for (int e = 0;e < entities.Count; e++)
					{
						for (int f = 0;f < node.entities.Count; f++)
						{
							if (hierarchy.isValidPair(entities[e], node.entities[f]))
							{
								hierarchy.addController(entities[e], node.entities[f]);
							}
						}
					}
				}
			}
			
			internal void binaryUpdateNode ()
			{
				if (currentVolume > maximumAllowedVolume)
				{
					revalidate();
				}
				else
				{
					Vector3d vector1;
					if (children.Count > 0)
					{
						foreach (DynamicBinaryHierarchyNode node1 in children)
						{
							node1.binaryUpdateNode();
						}
                        boundingBox =BoundingBox.Combine(children[0].boundingBox, children[1].boundingBox);
					}
					else
					{
                        boundingBox = entities[0].CollisionPrimitive.BoundingBox;
						for (int i = 1;i < entities.Count; i++)
						{
                            boundingBox = BoundingBox.Combine(boundingBox, entities[i].CollisionPrimitive.BoundingBox);
						}
					}
					vector1= boundingBox.Max -  boundingBox.Min;
					currentVolume = (vector1.x * vector1.y) * vector1.z;
				}
			}
			
			internal void updateNode ()
			{
				if (currentVolume > maximumAllowedVolume)
				{
					revalidate();
				}
				else
				{
					Vector3d vector1;
					if (children.Count > 0)
					{
						foreach (DynamicBinaryHierarchyNode node1 in children)
						{
							node1.updateNode();
						}
						boundingBox = children[0].boundingBox;
						for (int i = 1;i < children.Count; i++)
						{
                            boundingBox = BoundingBox.Combine(boundingBox, children[i].boundingBox);
						}
					}
					else
					{
                        boundingBox = entities[0].CollisionPrimitive.BoundingBox;
						for (int j = 1;j < entities.Count; j++)
						{
                            boundingBox = BoundingBox.Combine(boundingBox, entities[j].CollisionPrimitive.BoundingBox);
						}
					}
				    vector1 = boundingBox.Max -  boundingBox.Min;
					currentVolume = (vector1.x * vector1.y) * vector1.z;
				}
			}
			
			private void revalidate ()
			{
			    boundingBox = entities[0].CollisionPrimitive.BoundingBox;
				for (int i = 1;i < entities.Count; i++)
				{
                    boundingBox = BoundingBox.Combine(boundingBox, entities[i].CollisionPrimitive.BoundingBox);
				}
				Vector3d vector1 = boundingBox.Max - boundingBox.Min;
				currentVolume = (vector1.x * vector1.y) * vector1.z;
				maximumAllowedVolume = currentVolume * maximumAllowedVolumeFactor;
				if (entities.Count <= maximumEntitiesInLeaves)
				{
					if (children.Count > 0)
					{
						foreach (DynamicBinaryHierarchyNode node in children)
						{
							hierarchy.giveBack(node);
						}
						children.Clear();
					}
				}
				else
				{
                    double single4;
					DynamicBinaryHierarchyNode.ComparerAxis minimumAxis;
					foreach (DynamicBinaryHierarchyNode node2 in children)
					{
						hierarchy.giveBack(node2);
					}
					children.Clear();
					Vector3d vector2 = boundingBox.Min;
					Vector3d vector3 = boundingBox.Max;
					DynamicBinaryHierarchyNode node3 = hierarchy.getNode();
					DynamicBinaryHierarchyNode node4 = hierarchy.getNode();
					double single1 = vector3.x - vector2.x;
                    double single2 = vector3.y - vector2.y;
                    double single3 = vector3.z - vector2.z;
					if ((single1 > single2) && (single1 > single3))
					{
						minimumAxis = DynamicBinaryHierarchyNode.ComparerAxis.x;
						single4 = (vector3.x + vector2.x) * 0.50d;
						foreach (PhysicsBody item in entities)
						{
							if (item.myInternalCenterPosition.x > single4)
							{
								node3.entities.Add(item);
								continue;
							}
							node4.entities.Add(item);
						}
					}
					else
					{
						if ((single2 > single1) && (single2 > single3))
						{
							minimumAxis = DynamicBinaryHierarchyNode.ComparerAxis.z;
							single4 = (vector3.y + vector2.y) * 0.50d;
							foreach (PhysicsBody entity2 in entities)
							{
								if (entity2.myInternalCenterPosition.y > single4)
								{
									node3.entities.Add(entity2);
									continue;
								}
								node4.entities.Add(entity2);
							}
						}
						else
						{
							minimumAxis = DynamicBinaryHierarchyNode.ComparerAxis.z;
							single4 = (vector3.z + vector2.z) * 0.50d;
							foreach (PhysicsBody entity3 in entities)
							{
								if (entity3.myInternalCenterPosition.z > single4)
								{
									node3.entities.Add(entity3);
									continue;
								}
								node4.entities.Add(entity3);
							}
						}
					}
					int num2 = entities.Count;
					if (node3.entities.Count >= num2)
					{
						redoSplit(minimumAxis);
					}
					else
					{
						if (node4.entities.Count >= num2)
						{
							redoSplit(minimumAxis);
						}
						else
						{
							node4.revalidate();
							children.Add(node4);
							node3.revalidate();
							children.Add(node3);
						}
					}
				}
			}
			
			private void redoSplit (ComparerAxis minimumAxis)
			{
				foreach (DynamicBinaryHierarchyNode node in children)
				{
					hierarchy.giveBack(node);
				}
				children.Clear();
				axisComparer.axis = minimumAxis;
				entities.Sort(axisComparer);
				DynamicBinaryHierarchyNode node2 = hierarchy.getNode();
				DynamicBinaryHierarchyNode item = hierarchy.getNode();
				for (int i = 0;i < (entities.Count / 2); i++)
				{
					item.entities.Add(entities[i]);
				}
				for (int j = entities.Count / 2;j < entities.Count; j++)
				{
					node2.entities.Add(entities[j]);
				}
				children.Add(item);
				children.Add(node2);
				foreach (DynamicBinaryHierarchyNode node4 in children)
				{
					node4.revalidate();
				}
			}
			
			internal void getEntities (ref BoundingBox box, List<PhysicsBody> entities)
			{
				bool flag1;
				if (children.Count > 0)
				{
					
					if (boundingBox.Intersects(box))
					{
						foreach (DynamicBinaryHierarchyNode node1 in children)
						{
							
							if (node1.boundingBox.Intersects(box))
							{
								node1.getEntities(ref box, entities);
							}
						}
					}
				}
				else
				{
					foreach (PhysicsBody item in entities)
					{

                        if (item.CollisionPrimitive.BoundingBox.Intersects(box))
						{
							entities.Add(item);
						}
					}
				}
			}
			
			internal void getEntities (ref BoundingSphere sphere, List<PhysicsBody> entities)
			{
			    throw new NotImplementedException();

                //bool flag1;
                //if (children.Count > 0)
                //{
					
                //    if (boundingBox.Intersects(sphere))
                //    {
                //        foreach (DynamicBinaryHierarchyNode node1 in children)
                //        {
							
                //            if (node1.boundingBox.Intersects(sphere))
                //            {
                //                node1.getEntities(ref sphere, entities);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (PhysicsBody item in entities)
                //    {
						
                //        if (item.boundingBox.Intersects(sphere))
                //        {
                //            entities.Add(item);
                //        }
                //    }
                //}
			}
			
			internal void getEntities (ref Keystone.Culling.ViewFrustum  frustum, List<PhysicsBody> entities)
			{
                throw new NotImplementedException();
                //if (children.Count > 0)
                //{
                //    if (boundingBox.Intersects(frustum))
                //    {
                //        foreach (DynamicBinaryHierarchyNode node1 in children)
                //        {
                //            if (node1.boundingBox.Intersects(frustum))
                //            {
                //                node1.getEntities(ref frustum, entities);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (PhysicsBody item in entities)
                //    {
                //        if (item.boundingBox.Intersects(frustum))
                //        {
                //            entities.Add(item);
                //        }
                //    }
                //}
			}
			
			internal void addEntity (PhysicsBody e)
			{
				entities.Add(e);
				if (children.Count > 0)
				{
					double single1 = float.MaxValue;
					DynamicBinaryHierarchyNode node1 = null;
                    double single6 = e.myInternalCenterPosition.x;
                    double single7 = e.myInternalCenterPosition.y;
                    double single8 = e.myInternalCenterPosition.z;
					foreach (DynamicBinaryHierarchyNode node2 in children)
					{
						Vector3d vector1 = node2.boundingBox.Min;
						Vector3d vector2 = node2.boundingBox.Max;
                        double single3 = (vector2.x + vector1.x) * 0.50d;
                        double single4 = (vector2.y + vector1.y) * 0.50d;
                        double single5 = (vector2.z + vector1.z) * 0.50d;
                        double single2 = (Math.Abs((single3 - single6)) + Math.Abs((single4 - single7))) + Math.Abs((single5 - single8));
						if (single2 < single1)
						{
							node1 = node2;
							single1 = single2;
						}
					}
					node1.addEntity(e);
				}
				else
				{
					revalidate();
				}
			}
			
			internal void removeEntity (PhysicsBody e)
			{
				if (entities.Remove(e))
				{
					for (int index = 0;index < children.Count; index++)
					{
						if ((children[index].entities.Count <= (maximumEntitiesInLeaves + 1)) && children[index].entities.Contains(e))
						{
							hierarchy.giveBack(children[index]);
							children.RemoveAt(index);
							index--;
							if (entities.Count > 0)
							{
								revalidate();
							}
						}
						else
						{
							children[index].removeEntity(e);
						}
					}
				}
			}
			
            //public void collectBoundingBoxLines (List<VertexPositionColor> lineEndpoints, bool includeInternalNodes)
            //{
            //    if ((children.Count == 0) || includeInternalNodes)
            //    {
            //        Vector3d[] vectorArray1 = boundingBox.GetCorners();
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[0], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[1], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[0], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[3], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[0], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[4], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[1], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[2], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[1], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[5], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[2], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[3], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[2], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[6], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[3], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[7], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[4], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[5], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[4], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[7], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[5], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[6], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[6], Color.get_DarkRed()));
            //        lineEndpoints.Add(new VertexPositionColor(vectorArray1[7], Color.get_DarkRed()));
            //    }
            //    foreach (DynamicBinaryHierarchyNode node1 in children)
            //    {
            //        node1.collectBoundingBoxLines(lineEndpoints, includeInternalNodes);
            //    }
			//}

            internal void rayCast(Ray ray, double maximumLength, bool withMargin, List<PhysicsBody> hitEntities, List<Vector3d> hitLocations, List<Vector3d> hitNormals, List<double> tois)
			{
                throw new NotImplementedException();
                //if (children.Count > 0)
                //{
                //    foreach (DynamicBinaryHierarchyNode node1 in children)
                //    {
                //        Nullable<float> nullable1;
                //        ray.Intersects(ref node1.boundingBox, ref nullable1);
                //        if (nullable1.HasValue)
                //        {
                //            Nullable<float> nullable2 = nullable1;
                //            double single2 = maximumLength;
                //            if ((((float) nullable2.GetValueOrDefault()) < single2) && nullable2.HasValue)
                //            {
                //                node1.rayCast(ray, maximumLength, withMargin, hitEntities, hitLocations, hitNormals, tois);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (PhysicsBody item in entities)
                //    {
                //        Vector3d vector1;
                //        Vector3d vector2;
                //        double single1;
                //        if (!item.isMemberOfRayCastableContainer && item.rayTest(ray.origin, ray.direction , maximumLength, withMargin, out vector1, out vector2, out single1))
                //        {
                //            hitEntities.Add(item);
                //            hitLocations.Add(vector1);
                //            hitNormals.Add(vector2);
                //            tois.Add(single1);
                //        }
                //    }
                //}
			}
			
			public class AxisComparer : IComparer<PhysicsBody>
			{
                // Instance Fields
                internal DynamicBinaryHierarchyNode.ComparerAxis axis;

				// Constructors
				public AxisComparer ()
				{
				}
				
				
				// Methods
				public int Compare (PhysicsBody x, PhysicsBody y)
				{
					switch (axis)
					{
						case ComparerAxis.x:
						{
							if (x.myInternalCenterPosition.x > y.myInternalCenterPosition.x)
							{
								return 1;
							}
							if (x.myInternalCenterPosition.x < y.myInternalCenterPosition.x)
							{
								return -1;
							}
							return 0;
						}
						case ComparerAxis.y:
						{
							if (x.myInternalCenterPosition.y > y.myInternalCenterPosition.y)
							{
								return 1;
							}
							if (x.myInternalCenterPosition.y < y.myInternalCenterPosition.y)
							{
								return -1;
							}
							return 0;
						}
						case ComparerAxis.z:
						{
							if (x.myInternalCenterPosition.z > y.myInternalCenterPosition.z)
							{
								return 1;
							}
							if (x.myInternalCenterPosition.z < y.myInternalCenterPosition.z)
							{
								return -1;
							}
							return 0;
						}
						default:
						{
							return 0;
						}
					}
				}
				
			}
		}
	}
}
