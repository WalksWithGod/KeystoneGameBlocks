using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics
{
    // bounding volume hierarchy
	internal class CompoundBodyBVHNode
	{
        // Instance Fields
        internal BoundingBox boundingBox;
        internal CompoundBodyBVHNode childA;
        internal CompoundBodyBVHNode childB;
        internal PhysicsBody leaf;

		// Constructors
		internal CompoundBodyBVHNode (List<SortableEntity> entities)
		{
			if (entities.Count == 1)
			{
				leaf = entities[0].e;
				boundingBox = leaf.CollisionPrimitive.BoundingBox;
			}
			else
			{
				boundingBox = getMergedBoundingBox(entities);
				List<SortableEntity> part1 = new List<SortableEntity>();
				List<SortableEntity> part2 = new List<SortableEntity>();
				getSplit(entities, boundingBox, part1, part2);
				childA = new CompoundBodyBVHNode(part1);
				childB = new CompoundBodyBVHNode(part2);
			}
		}
		
		internal CompoundBodyBVHNode (BoundingBox box, CompoundBodyBVHNode childA, CompoundBodyBVHNode childB)
		{
			boundingBox = box;
			this.childA = childA;
			this.childB = childB;
			leaf = null;
		}
		
		internal CompoundBodyBVHNode (BoundingBox box)
		{
			boundingBox = box;
			leaf = null;
		}
		
		internal CompoundBodyBVHNode (PhysicsBody e)
		{
            boundingBox = e.CollisionPrimitive.BoundingBox;
			leaf = e;
		}
		
		
		// Methods
		private BoundingBox getMergedBoundingBox (List<SortableEntity> toMerge)
		{
            BoundingBox box1 = toMerge[0].e.CollisionPrimitive.BoundingBox;
			for (int i = 1;i < toMerge.Count; i++)
			{
                box1.Combine(toMerge[i].e.CollisionPrimitive.BoundingBox);
			}
			return box1;
		}
		
		private void getSplit (List<SortableEntity> wholeList, BoundingBox wholeBox, List<SortableEntity> part1, List<SortableEntity> part2)
		{
			double single1 = wholeBox.Max.x - wholeBox.Min.x;
            double single2 = wholeBox.Max.y - wholeBox.Min.y;
            double single3 = wholeBox.Max.z - wholeBox.Min.z;
			if ((single1 > single2) && (single1 > single3))
			{
				foreach (SortableEntity entity1 in wholeList)
				{
					entity1.axis = Vector3d.Right();
				}
			}
			else if ((single2 > single1) && (single2 > single3))
			{
				foreach (SortableEntity entity2 in wholeList)
				{
					entity2.axis = Vector3d.Up();
				}
			}
			else
			{
				foreach (SortableEntity entity3 in wholeList)
				{
					entity3.axis = Vector3d.Forward();
				}
			}
			wholeList.Sort();
			for (int i = 0;i < (wholeList.Count / 2); i++)
			{
				part1.Add(wholeList[i]);
			}
			for (int j = wholeList.Count / 2;j < wholeList.Count; j++)
			{
				part2.Add(wholeList[j]);
			}
		}
		
		internal void refit ()
		{
			if (leaf == null)
			{
				childA.refit();
				childB.refit();
                boundingBox = BoundingBox.Combine(childA.boundingBox, childB.boundingBox);
			}
			else
			{
                boundingBox = leaf.CollisionPrimitive.BoundingBox;
			}
		}
		
		internal void getEntitiesNearEntity (PhysicsBody e, List<PhysicsBody> nearbyEntities)
		{
            if (this.boundingBox.Contains(e.CollisionPrimitive.BoundingBox))
			{
				if (leaf == null)
				{
					childA.getEntitiesNearEntity(e, nearbyEntities);
					childB.getEntitiesNearEntity(e, nearbyEntities);
				}
				else
				{
					nearbyEntities.Add(leaf);
				}
			}
		}
		
		internal void getEntitiesNearRay (Ray ray, double maximumDistance, List<PhysicsBody> nearbyEntities)
		{
            //Nullable<double> nullable1;
            //ray.Intersects(this.boundingBox, ref nullable1);
            ////boundingBox.Intersects( r)
            //if (this.boundingBox.Intersects( r))
            //{
            //    Nullable<double> nullable2 = nullable1;
            //    double single1 = maximumDistance;
            //    if ((( nullable2.GetValueOrDefault()) < single1) && nullable2.HasValue)
            //    {
            //        if (leaf == null)
            //        {
            //            childA.getEntitiesNearRay(ray, maximumDistance, nearbyEntities);
            //            childB.getEntitiesNearRay(ray, maximumDistance, nearbyEntities);
            //        }
            //        else
            //        {
            //            nearbyEntities.Add(leaf);
            //        }
            //    }
            //}
		}
	}
}
