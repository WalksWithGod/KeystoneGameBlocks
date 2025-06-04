using System;
using KeyCommon.Traversal;
using Keystone.Elements;

namespace Keystone.Behavior.Composites
{
    public class Selector : Composite
    {
    		    	
        public Selector(string id) : base(id) { }
                
        // NOTE: When traversing a sequence, we test higher sequeneced child nodes that are inactive
        // but once (if) we reach the child that is active and none of the previous children
        // have pre-empted it, then we Execute it.  However if one of the earlier children does
        // pre-empt it, then we will traverse the state and DeActivate() those nodes
        //
        // 
        // Avoid O(n^2) coupling of behaviors by brute force, and make the
     //brute force execution efficient.  The entire active part of the
     //tree is ticked every frame, which--in the case of the common
     //prioritized list group decision policy--means all designated
     //higher priority behaviors get a chance to interrupt the current
     //active behavior before it is ticked.
        // http://chrishecker.com/My_Liner_Notes_for_Spore/Spore_Behavior_Tree_Docs
        public override BehaviorResult Perform(Entities.Entity entity, double elapsedSeconds)
        {	
        	if (_children == null || _children.Count == 0) return BehaviorResult.Fail;
        	
        	foreach (Node behavior in _children)
        	{
        		BehaviorResult result = ((Behavior)behavior).Perform (entity, elapsedSeconds);
        		// return on first SUCCESS or RUNNING
        		if (result == BehaviorResult.Success || result == BehaviorResult.Running) 
        			return result;
        	}
        	
        	// none succeeded
        	return BehaviorResult.Fail;
        }

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
    }
}
