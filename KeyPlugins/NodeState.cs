using System;
using System.Collections.Generic;


namespace KeyPlugins
{ 

    // todo: wait, what am I doing here?  I still need to get the state AFTER the graphics loop so essentially during ProcessCommandComopleted() so why not just call the SelectTarget() then? Well for some plugin updates
    //       like Appearance and GroupAttributes we need to make a call to the Getters of nodes, but this too can simply be done asychronously.
    public class NodeState
    {
        private string mNodeTypename;
        private string modeID;
        private Settings.PropertySpec[] mProperties;
        public object GetPropertyValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        private NodeState mParent;
        private NodeState[] mChildren;

        

        // when selecting an Entity, we need to build the NodeState after the graphics loop is completed.
        // on changes to a property of any node within the NodeState hierarchy, we update the mProperties
        // we can set a flag to ignore updates to the plugin for position, rotation, scale changes that occur every frame


    }
}
