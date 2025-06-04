using System;
using Keystone.Elements;
using Keystone.Entities;

namespace Keystone.Interfaces
{
			/** Called when a node gets updated.
		@remarks
			Note that this happens when the node's derived update happens,
			not every time a method altering it's state occurs. There may 
			be several state-changing calls but only one of these calls, 
			when the node graph is fully updated.
		*/
    public interface INodeListener
    {
		/** Node has been attached to a parent */
        void NodeAttached(Entity node);
		/** Node has been detached from a parent */
        void NodeDetached(Entity node);  // verify this fires when a node is paged out and/or it's entire region is unloaded)

        void NodeUpdated(Entity node);
        
        void NodeMoved(Entity node);
        /** Node is being destroyed */
        void NodeDestroyed(Entity node); 
    }

    // if a light has moved, we may have to notify all entities that were affected by it
    // so that they can test if they still are.
    // NOTE: in that case, doesn't the light still need to find all other entities it might affect?
    

}
