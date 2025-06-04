using System;
using System.IO;
using System.Collections.Generic;


namespace KeyPlugins
{
     /// <summary>
	/// Summary description for PluginServices.
	/// </summary>
	public abstract class PluginServices : IPluginHost   //<--- Notice how it inherits IPluginHost interface!
	{
        private List<AvailablePlugin> colAvailablePlugins = new List<AvailablePlugin>();
        private const string REQUIRED_INTERFACE = "KeyPlugins.IPlugin";
        private const string REQUIRED_EXTENSION = ".dll";
        private const string REQUIRED_NAMING_CONVENTION_KEYWORD = "KEYPLUGIN";
        private bool mPluginChangesSuspended = false;

		/// <summary>
		/// Constructor of the Class
		/// </summary>
		public PluginServices()
		{
		}

        
        public AvailablePlugin CurrentPlugin { get; set; }

		/// <summary>
		/// A List of all Plugins Found and Loaded by the FindPlugins() Method
		/// </summary>
        public List<AvailablePlugin> AvailablePlugins
		{
			get {return colAvailablePlugins;}
			set {colAvailablePlugins = value;}
		}

        // TODO: it'd be nice if we could select using any number of search parameters
        // we could use overloads of SelectPlugin()  
        // 
        public AvailablePlugin SelectPlugin(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            AvailablePlugin selectedPlugin = AvailablePlugins.Find(p => p.Instance.Name == name);
            CurrentPlugin = selectedPlugin;
            return CurrentPlugin;
        }

        public AvailablePlugin SelectPlugin(string profile, string nodeTypeName)
        {
            //Get the selected Plugin
            KeyPlugins.AvailablePlugin selectedPlugin;
            
            if (!string.IsNullOrEmpty(profile))
                selectedPlugin = AvailablePlugins.Find(p => p.Instance.Profile == profile);
            else
                selectedPlugin = AvailablePlugins.Find(p => p.Instance.ContainsSupportedType (nodeTypeName));

            CurrentPlugin = selectedPlugin;
            return CurrentPlugin;

            // OBSOLETE 2.0
            //foreach (AvailablePlugin plugin in AvailablePlugins)
            //{
            //    if (plugin.Instance.TargetType == nodeTypeName)
            //    {
            //        Current = plugin;
            //        return plugin;
            //    }
            //}

            // OBSOLETE 1.0
            //if (AvailablePlugins.TryGetValue(nodeTypeName, out selectedPlugin))
            //{
            //    // for debugging, create a string for the dock panel that will contain the 
            //    // plugin information
            //    string pluginSummary = selectedPlugin.Instance.Summary;

            //    Current = selectedPlugin;
            //    return selectedPlugin;
            //}
            
            CurrentPlugin = null;
            return null;
        }

		/// <summary>
		/// Searches the Application's Startup Directory for Plugins
		/// </summary>
		public void FindPlugins()
		{
			FindPlugins(AppDomain.CurrentDomain.BaseDirectory, null, null);
		}
		/// <summary>
		/// Searches the passed Path for Plugins
		/// </summary>
		/// <param name="Path">Directory to search for Plugins in</param>
		public void FindPlugins(string Path, string modsPath, string modName)
		{

			
			//First empty the collection, we're reloading them all
			colAvailablePlugins.Clear();

            //System.IO.Path.GetFileNameWithoutExtension(pluginFiles[i]);


            string path = System.IO.Path.GetFullPath(Path);
            
            //Go through all the files in the plugin directory
			foreach (string fileOn in Directory.GetFiles(Path))
			{
				FileInfo file = new FileInfo(fileOn);

                // NOTE: It's critical we only Assembly.LoadFrom() plugin's and not any dll's required by the plugins.  
                // Otherwise if we attempt to load those shared dll's elsewhere, we'll get weird errors 
                // such as the one that occurred in my entity scripting "MissingMethodException" or
                // we get weird errors associated with KeyMath.dll loaded

                // That is why we still must search for a naming convention 
                if (!fileOn.ToUpper().Contains(REQUIRED_NAMING_CONVENTION_KEYWORD)) continue; 

				//Preliminary check, must be .dll
				if (file.Extension.Equals(REQUIRED_EXTENSION))
				{	
					//Add the 'plugin'
					this.AddPlugin(fileOn, modsPath, modName);				
				}
			}
		}
		
		/// <summary>
		/// Unloads and Closes all AvailablePlugins
		/// </summary>
		public void ClosePlugins()
		{
			foreach (AvailablePlugin pluginOn in colAvailablePlugins)
			{
				//Close all plugin instances
				//We call the plugins Dispose sub first incase it has to do 
				//Its own cleanup stuff
				pluginOn.Instance.Dispose(); 
				
				//After we give the plugin a chance to tidy up, get rid of it
				pluginOn.Instance = null;
			}
			
			//Finally, clear our collection of available plugins
			colAvailablePlugins.Clear();
		}
		
		private void AddPlugin(string FullPath, string modsPath, string modName)
		{
			//Create a new assembly from the uncompiled plugin file path received
            string fileName = System.IO.Path.GetFileName(FullPath);
            const System.Reflection.BindingFlags VISIBILITY =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;
            Type[] argumentTypes = new Type[] { typeof(IPluginHost) };

            
            
            System.Reflection.Assembly pluginAssembly = System.Reflection.Assembly.LoadFrom(FullPath);
			
            if (pluginAssembly != null)
            {
                Type[] types;

                try
                {
                    types = pluginAssembly.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - " + pluginAssembly.FullName + " " + ex.Message);

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("KGB Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    string errorMessage = sb.ToString();
                    //Display or log the error based on your application.

                    return; // this dll is broken somehow and we should just return so other plugins can be added
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - " + pluginAssembly.FullName + " " + ex.Message);
                    return; // this dll is broken somehow and we should just return so other plugins can be added
                }

                foreach (Type t in types)
                {
                    // CHECK IF CURRENT TYPE IMPLEMENTS INTERFACE IPlugin
                    if (typeof(IPlugin).IsAssignableFrom(t))
                    {
                        //Create a new available plugin since the type implements the IPlugin interface
                        AvailablePlugin newPlugin = new AvailablePlugin();
                        try
                        {
                            //Set the filename where we found it
                            newPlugin.AssemblyPath = FullPath;

                            //Create a new instance and store the instance in the list for later use
                            //We could change this later on to not load an instance.. we have 2 options
                            //1- Make one instance, and use it whenever we need it.. it's always there
                            //2- Don't make an instance, and instead make an instance whenever we use it, then close it
                            //For now we'll just make an instance of all the plugins
  							
                            newPlugin.Instance = (IPlugin)Activator.CreateInstance(t, new object[] { this,  modsPath, modName });

                            //Call the initialization sub of the plugin
                            newPlugin.Instance.Initialize();

                            //Add the new plugin to our collection here
                            this.colAvailablePlugins.Add(newPlugin);
                            System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - Load Succeeded - '" + pluginAssembly.FullName + "'");
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - Load FAILED - '" + pluginAssembly.FullName + "' - " + ex.Message);
                        }
                        finally
                        {
                            newPlugin = null;
                        }
                    }
                }
            }

            return;


            // OBSOLETE - THe above verion finds dlls with correct interface perfectly
            ////Next we'll loop through all the Types found in the assembly
            //foreach (Type pluginType in pluginAssembly.GetTypes())
            //{
            //    if (pluginType.IsPublic && !pluginType.IsAbstract) //Only look at public and non-abstract types
            //    {
            //        // try to retrieve the specific constructor we need
            //        System.Reflection.ConstructorInfo constructor = pluginType.GetConstructor(VISIBILITY, null, argumentTypes, null);
            //        //Gets a type object of the interface we need the plugins to match
            //        Type typeInterface = pluginType.GetInterface(REQUIRED_INTERFACE, true);
					
            //        //Make sure the interface we want to use actually exists
            //        if (constructor != null && typeInterface != null)
            //        {
            //            //Create a new available plugin since the type implements the IPlugin interface
            //            AvailablePlugin newPlugin = new AvailablePlugin();
            //            try
            //            {
            //                //Set the filename where we found it
            //                newPlugin.AssemblyPath = FullPath;

            //                //Create a new instance and store the instance in the collection for later use
            //                //We could change this later on to not load an instance.. we have 2 options
            //                //1- Make one instance, and use it whenever we need it.. it's always there
            //                //2- Don't make an instance, and instead make an instance whenever we use it, then close it
            //                //For now we'll just make an instance of all the plugins
            //                newPlugin.Instance = (IPlugin)Activator.CreateInstance(pluginType, new object[] { this });

            //                //Call the initialization sub of the plugin
            //                newPlugin.Instance.Initialize();

            //                //Add the new plugin to our collection here
            //                // TODO: what if multiple plugins have the same TargetType, we'll get
            //                // a duplicate collection key exception. 
            //                // TODO: we need another way of adding plugins than by TargetType
            //                // so that we can choose a plugin based on various parameters
            //                // TODO: in fact, we could configure the selection rules via 'profiles'
            //                // and then switch 'profiles' to find the proper plugins...
            //                // So when we are in "vehicle runtime" profile, we switch to the
            //                // onboardcomponents plugin for "vehicle" types.   But wait..
            //                // how are we selecting from the collection\list of available plugins?
            //                // Do we have to assign a "profile" name or instead can we just
            //                // configure the plugins ourselves... and that would entail telling the 
            //                // pluginservices which plugins map to which profile
            //                // TODO: so we should add the plugins not by TargetTYpe but by
            //                // name:version  
            //                // Ok so fine, let's assume we have our plugins by name:version
            //                // and we can go to our "settings" and add "plugin profiles" and assign
            //                // plugins to types there and then on our "SelectPlugin" we include a profile
            //                // as well as type...? and then the PluginHost looks up the mapping of that
            //                // type in the profile and selects the proper plugin if it can find one?
            //                // hell, the plugin manager GUI we build can do this for us... 
            //                // and our plugin manager we can have use a sperate Settings file for plugins
            //                // using our normal kvp setup.
            //                this.colAvailablePlugins.Add(newPlugin);
                        
            //            }
            //            catch (MissingMethodException missing)
            //            {
            //                // this occurs when attempting to load as a plugin some of the base
            //                // classes
            //                System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - " + pluginType.FullName + " " + missing.Message);
            //            }
            //            catch (Exception ex)
            //            {
            //                System.Diagnostics.Debug.WriteLine("PluginServices.AddPlugin() - " + pluginType.FullName + " " + ex.Message);
            //            }
            //            finally
            //            {
            //                newPlugin = null;
            //            }
            //        }
            //        typeInterface = null; 				
            //    }			
            //}
            //pluginAssembly = null; 
        }

    
        // the IPluginHost could be a base class, and we inherit such as
        // INodeEditorPluginHost  for editing nodes that then has the API for
        // editing various nodes.  In fact I think I should definetly do that
        
        #region IPluginHost Members
        public bool PluginChangesSuspended
        {
            get { return mPluginChangesSuspended; }
            set { mPluginChangesSuspended = value; }
        }

        // SCENE
        public abstract string Scene_GetRoot();

        // ENGINE
        public abstract IntPtr Engine_CreateViewport();
        public abstract string Engine_GetModPath();

        // IO
        /// <summary>
        /// For Geometry, NodeIDs match relative resource path
        /// </summary>
        /// <param name="currentNodeID"></param>
        /// <param name="newNodeID"></param>
        /// <param name="modsPath"></param>
        public abstract void Geometry_Save(string currentNodeID, string newNodeID, string modsPath);
        public abstract void Geometry_Add(string entityID, string modelID, string resourcePath, bool loadTextures, bool loadMaterial);
        public abstract void OpenFile();
        public abstract void SaveFile();
        
		// Tasks 
        public abstract void Task_Create (Game01.Messages.OrderRequest request); // do we need to get a task id from server? similar to how Node_Create works? i dont think we need wait on it.  we can pass all parameters and 
                                         // then simply refresh the table when we are notified the task has been added to database which will first come via a "Task_Create_Record" from server
                                         // instructin client to add that record to local db.  TODO: I still need to figure out how NPCs create /modify tasks as far as server is concerned
        public abstract void Task_Delete (long taskID); // cancels the task and retires it to tasks_retired table? or does it actually delete it without inserting it as new record to retired table?
        public abstract void Task_Update(long taskID); // edit

        
        public abstract void View_LookAt(string entityID, float percentExtents);
        public abstract void Vehicle_Orbit(string targetBodyID);
        public abstract void Vehicle_TravelTo(string targetID);
        public abstract void Vehicle_Intercept(string targetID);
        public abstract void Vehicle_Dock(string targetID);

        public abstract void Geometry_CreateGroup(string modelID, string geometryID, string groupName, int groupType, int groupClass = 0, string meshPath = null);
        public abstract void Geometry_RemoveGroup(string modelID, string geometryID, int groupIndex, int groupClass = 0);
        public abstract void Geometry_ChangeGroupProperty(string geometryID, int groupIndex, string propertyName, string typename, object newValue, int geometryParams = 0);
        public abstract object Geometry_GetGroupProperty(string geometryID, int groupIndex, string propertyName, int geometryParams = 0);
        public abstract Settings.PropertySpec[] Geometry_GetGroupProperties(string geometryID, int groupIndex, int geometryParams = 0);
        public abstract object Geometry_GetStatistic(string geometryID, string statName);
        public abstract void Geometry_ResetTransform(string geometryID, Keystone.Types.Matrix m);

        public abstract string Node_GetDescendantByName(string startingNode, string descendantName);
        public abstract string Node_GetName(string nodeID);
        public abstract bool Node_HasDescendant(string groupNode, string potentialDescendant);
        public abstract string Node_GetTypeName(string nodeID);
        public abstract string Node_GetChildOfType(string groupNode, string typeName); // returns the first node of that type found
        public abstract string[] Node_GetChildrenOfType(string groupNode, string typeName); // typically used to grab all GroupAttribute nodes under a DefaultAppearance
        public abstract void Node_GetChildrenInfo(string nodeID, string[] filteredTypes, out string[] childID, out string[] childNodeTypes);
        public abstract void Node_ChangeProperty(string nodeID, string propertyName, Type type, object newValue);
        public abstract object Node_GetProperty(string nodeID, string propertyName);
        public abstract Settings.PropertySpec[] Node_GetProperties(string nodeID);
        // TODO: these really should go in an EditorHost that inherits this and has just these abstract methods
        public abstract bool Node_GetFlagValue(string nodeID, string flagName);
        public abstract void Node_SetFlagValue(string nodeID, string flagName, bool value);
        // IO
        public abstract void Entity_SavePrefab(string nodeID, string relativeZipPath, string entryPath, string entryName);
        public abstract bool Entity_GetFlagValue(string entityID, string flagName);
        public abstract void Entity_SetFlagValue(string entityID, string flagName, bool value);

        public abstract uint Entity_GetUserTypeIDFromString(string userTypeName);
        public abstract string Entity_GetUserTypeStringFromID(uint userTypeID);
        public abstract string[] Entity_GetUserTypeIDsToString();

        public abstract Settings.PropertySpec[] Entity_GetCustomProperties(string sceneName, string entityID, string entityTypename);
        public abstract object Entity_GetCustomPropertyValue(string entityID, string propertyName);
        public abstract void Entity_SetCustomPropertyValue(string entityID, string propertyName, string typeName, object newValue);

        public abstract bool Model_GetFlagValue(string modelID, string flagName);
        public abstract void Model_SetFlagValue(string modelID, string flagName, bool value);

        public abstract void Node_Remove(string nodeID, string parentID);
        public abstract void Node_Create(string typeName, string parentID);
        public abstract void Node_Create (string typeName, string parentID, Settings.PropertySpec[] properties);
        public abstract void Node_Create(string typeName, string resourcePath, string parentID, string fileDialogFilter);
        public abstract void Node_MoveChildOrder(string parentID, string nodeID, bool down);
        public abstract void Node_InsertUnderNewNode(string typeName, string parentID, string nodeID);
        public abstract void Node_ResourceRename(string nodeID, string newID, string parentID);
        public abstract void Node_ReplaceResource(string oldResourceID, string newResourceID, string newTypeName, string parentID); // replace a resource with another.  Since it's a resource and the ID is fixed to the resource name, the server doesnt have to receive a "Create" request command to generate the shared GUID
        public abstract void Node_Paste(string nodeID, string parentID);
        public abstract void Node_Copy(string nodeID, string parentID); // copy a specific branch? Actually is parent ever necessary?  not for copy i dont think ever?
        public abstract void Node_Cut(string nodeID, string parentID); // remove the node and copy it's id and previous to memory. Don't let it fall out of scope?  save to prefab?

        // GUI (entities)
        public abstract string Entity_GetGUILayout(string entityID, KeyCommon.Traversal.PickResultsBase pickDetails);
        public abstract void Entity_GUILinkClicked(string entityID, string scriptedMethodName, string linkName);

        // appearance
        public abstract Settings.PropertySpec[] Appearance_GetShaderParameters(string appearanceID);
        public abstract void Appearance_ChangeShaderParameterValue(string appearanceID, string parameterName, string typeName, object newValue);


        // animations
        public abstract double Entity_GetCurrentKeyFrame(string animationNodeID);
        public abstract void Entity_PlayAnimation(string entityID, string animationNodeID);
        public abstract void Entity_StopAnimation(string entityID, string animationNodeID);
        #endregion
    }
}
