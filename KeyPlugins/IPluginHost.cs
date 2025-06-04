using System;

namespace KeyPlugins
{
    /// <summary>
    /// The primary difference between IPluginHost and BaseScript is that
    /// the plugin host creates asychronous messages to be sent to the server
    /// whereas the script runs directly against the scene sycrhonously.
    /// </summary>
    public interface IPluginHost
    {
        AvailablePlugin SelectPlugin(string profile, string typeName);
        bool PluginChangesSuspended {get; set;}
        //void Feedback(string Feedback, IPlugin Plugin);


        void View_LookAt(string entityID, float percentExtents);

        void Vehicle_Orbit(string targetBodyID);
        void Vehicle_TravelTo(string targetID);
        void Vehicle_Intercept(string targetID);
        void Vehicle_Dock(string targetID);

        // ENGINE
        IntPtr Engine_CreateViewport ();
        string Engine_GetModPath();

        // IO
        void Geometry_Add(string entityID, string modelID, string resourcePath, bool loadTextures, bool loadMaterial);
        void Geometry_Save(string currentNodeID, string newNodeID, string modsPath);
        void Entity_SavePrefab(string nodeID, string relativeZipPath, string entryPath, string entryName);
        
        // Tasks 
        
        void Task_Create (Game01.Messages.OrderRequest request); // do we need to get a task id from server? similar to how Node_Create works? i dont think we need wait on it.  we can pass all parameters and 
                                         // then simply refresh the table when we are notified the task has been added to database which will first come via a "Task_Create_Record" from server
                                         // instructin client to add that record to local db.  TODO: I still need to figure out how NPCs create /modify tasks as far as server is concerned
        void Task_Delete (long taskID); // cancels the task and retires it to tasks_retired table? or does it actually delete it without inserting it as new record to retired table?
        void Task_Update(long taskID); // edit
                                       // obsolete - Task_Suspend is done via a "Update/Edit" of the task which sets the resolution to "suspended"
                                       // void Task_Suspend(int taskID);


        // Property and Value manipulation

        void Geometry_CreateGroup(string modelID, string geometryID, string groupName, int groupType, int groupClass = 0, string meshPath = null);
        void Geometry_RemoveGroup(string modelID, string geometryID, int groupIndex, int groupClass = 0);
        void Geometry_ChangeGroupProperty(string geometryID, int groupIndex, string propertyName, string typename, object newValue, int geometryParams = 0);
        object Geometry_GetGroupProperty(string geometryID, int groupIndex, string propertyName, int geometryParams = 0);
        Settings.PropertySpec[] Geometry_GetGroupProperties(string geometryID, int groupIndex, int geometryParams = 0);
        object Geometry_GetStatistic(string geometryID, string statName);
        void Geometry_ResetTransform(string geometryID, Keystone.Types.Matrix m);

        string Node_GetName(string nodeID);
        void Node_ChangeProperty(string nodeID, string propertyName, Type type, object newValue);
        object Node_GetProperty(string nodeID, string propertyName);
        Settings.PropertySpec[] Node_GetProperties(string nodeID);
        bool Node_GetFlagValue(string ndoeID, string flagName);
        void Node_SetFlagValue(string nodeID, string flagName, bool value);
        bool Entity_GetFlagValue(string entityID, string flagName);
        void Entity_SetFlagValue(string entityID, string flagName, bool value);
        bool Model_GetFlagValue(string modelID, string flagName);
        void Model_SetFlagValue(string modelID, string flagName, bool value);
        
        uint Entity_GetUserTypeIDFromString(string userTypeName);
        string Entity_GetUserTypeStringFromID(uint userTypeID);
        string[] Entity_GetUserTypeIDsToString();

        // Entity_GetCustomProperties() is for generating GUI interface for properties.  Is typically NOT for values 
        Settings.PropertySpec[] Entity_GetCustomProperties(string sceneName, string entityID, string entityTypename);
        object Entity_GetCustomPropertyValue(string entityID, string propertyName);
        void Entity_SetCustomPropertyValue(string entityID, string propertyName, string typeName, object newValue);

        // Node Manipulation
        string Node_GetDescendantByName(string startingNode, string descendantName);
        
        bool Node_HasDescendant(string groupNode, string potentialDescendant);
        string Node_GetTypeName(string nodeID);
        string Node_GetChildOfType(string groupNode, string typeName); // returns the first node of that type found
        string[] Node_GetChildrenOfType(string groupNode, string typeName); // typically used to grab all GroupAttribute nodes under a DefaultAppearance
        void Node_GetChildrenInfo(string nodeID, string[] filteredTypes, out string[] childID, out string[] childNodeTypes);
        void Node_Remove(string nodeID, string parentID);
        void Node_Create(string typeName, string parentID); // creates from type
        void Node_Create (string typeName, string parentID, Settings.PropertySpec[] properties);
        void Node_Create(string typeName, string parentID, string resourcePath, string fileDialogFilter); // adds from a resource
        void Node_MoveChildOrder(string parentID, string nodeID, bool down);
        void Node_InsertUnderNewNode(string typeName, string parentID, string nodeID);
        void Node_ResourceRename(string nodeID, string newID, string parentID);
        void Node_ReplaceResource(string oldResourceID, string newResourceID, string newTypeName, string parentID); // replace a resource with another.  Since it's a resource and the ID is fixed to the resource name, the server doesnt have to receive a "Create" request command to generate the shared GUID
        void Node_Paste(string nodeID, string parentID);
        void Node_Copy(string nodeID, string parentID); 
        void Node_Cut(string nodeID, string parentID);

        // SCENE
        string Scene_GetRoot();

        // GUI (entities)
        string Entity_GetGUILayout(string entityID, KeyCommon.Traversal.PickResultsBase pickDetails);
        void Entity_GUILinkClicked(string entityID, string scriptedMethodName, string linkName);

        // Appearance
        Settings.PropertySpec[] Appearance_GetShaderParameters(string appearanceID);
        void Appearance_ChangeShaderParameterValue(string appearanceID, string parameterName, string typeName, object newValue);
    

        // animations
        double Entity_GetCurrentKeyFrame(string animationNodeID);
        void Entity_PlayAnimation (string entityID, string animationNodeID);
        void Entity_StopAnimation(string entityID, string animationNodeID);
    }
}
