using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Collision
{

    public interface IPickResultComparer : System.Collections.Generic.IComparer<PickResults>
    {
        //int Compare(PickResults a, PickResults b);
    }

    /// <summary>
    /// Items that are closer are inserted ahead of items that are far.
    /// </summary>
    public class PickNearComparer : IPickResultComparer
    {
        #region IComparer<VisibleState> Members
        public int Compare(PickResults newItem, PickResults existingItem)
        {
            // if the score is higher, we will always pick it first over distance
            if (newItem.Score > existingItem.Score)
                return 1;
            else if (newItem.Score < existingItem.Score)
                return 0;

            // else if scores are the same, continue to distance test

            // If the new item is CLOSER then we want to pick it first
            // and so we want it HIGHER in the list so we return 1
            int result =  newItem.DistanceSquared < existingItem.DistanceSquared ? 1 : 0;
        
            return result;
        }
        #endregion
    }

    /// <summary>
    /// Items that are farther away are inserted ahead of items that are near.
    /// </summary>
    public class PickFarComparer : IPickResultComparer
    {
        #region IComparer<PickResults> Members
        public int Compare(PickResults newItem, PickResults existingItem)
        {
            // if the score is higher, we will always pick it first over distance
            if (newItem.Score > existingItem.Score)
                return 1;
            else if (newItem.Score < existingItem.Score)
                return 0;

            // eles if == continue to distance test

            // If the new item is FARTHER then we want to pick it first
            // and so we want it HIGHER in the list so we return 1
            return newItem.DistanceSquared > existingItem.DistanceSquared ? 1 : 0;
        }
        #endregion
    }

    public class PickDeepestNestLevelComparer : IPickResultComparer 
    {
        #region IComparer<PickResults> Members
        public int Compare(PickResults newItem, PickResults existingItem)
        {
           // WARNING: hack: we are assuming that the entities we're testing here are Regions
           //          and have RegionNode's as their SceneNode type.
           
            // If the new item is DEEPER then we want to pick it first
            // and so we want it HIGHER in the list so we return 1
            if (((Keystone.Elements.RegionNode)newItem.Entity.SceneNode).NestingLevel > ((Keystone.Elements.RegionNode)existingItem.Entity.SceneNode).NestingLevel)
                return 1;
            else if (((Keystone.Elements.RegionNode)newItem.Entity.SceneNode).NestingLevel < ((Keystone.Elements.RegionNode)existingItem.Entity.SceneNode).NestingLevel)
                return 0;
            
            // if the nesting level is the same, we prefer the farthest one
            
            // If the new item is FARTHER then we want to pick it first
            // and so we want it HIGHER in the list so we return 1
            int result =  newItem.DistanceSquared > existingItem.DistanceSquared ? 1 : 0;
            return result;
        }
        #endregion
    }
}
