using System;
using System.Collections.Generic;
using Keystone.Cameras;
using System.Data.SQLite;
namespace KeyEdit.PluginHost
{
    // NOTE: The difference between our entity scripting and this plugin scripting is that
    // entity scripts are usually created by the developers or modders and plugins by the end user.
    // So entity scripts are trusted, plugins are not.  Entity scritps can be designed to have validation
    // rules written into them so that when users try to modify entity values, they can be validated
    // with that custom validation code that is entity specific.  But plugin validation might eventually
    // have to call the entity's validation code for a specific property or method.
    // 
    // Thus entity scripts can call the various EntityAPI, AudioAPI, VisualFXAPI etc which directly modify
    // scene.  These scripts dont need to be validated or run against Rules because they are made by the developer.
    // Plugin's however need to be packaged into requests and sent as IRemotable requests to the 
    // server (loopback or remote)

    // TODO: first always package the message in an IRemotable SendMessage command container to the
    // network (which may be loopback but may also be our networked editor), however
    // here we're just directly modifying the scene.


    // TODO: when do we check permissions if the command is authorized by this user on this entity or node?

    // at this point, the message must  be packaged for sending over the wire or to the loopback
    // based on the nodeID we should Node node = Repository.Get(nodeID).Capture(dataset);
    // then it arrives to the processor where it gets sent to be verified and applied to the node on the server
    // or loopback.  
    // Potentially this will a return update for that entity if either there was a failure or something...?

    // rename ->  uses Node_ChangeProperty
    // copy
    // paste 
    // export texture, .obj, etc 
    //attach to bone #
    // lookAt              
    //http://www.youtube.com/watch?v=yv03HefZ39U&feature=related

    // add diffuse
    //// TODO: use the AddNode command so undo is trivial
    //System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
    //string path = enc.GetString(value);

    // TODO: Should the EditorHost have a reference to the primary EditorController.cs instance?
    // Afterall, they should be able to call bound functions by name instead of keypress..
    public class EditorHost : KeyPlugins.PluginServices
    {
        public Keystone.Resource.IResource Selected;
        private Amib.Threading.PostExecuteWorkItemCallback mCompletionCallback;


        internal EditorHost(Amib.Threading.PostExecuteWorkItemCallback commandCompletedHandler)
        {
            // TODO: i dont remember what this callback function is supposed to be for but
            // currently it's un-used (Feb.27.2012)
            mCompletionCallback = commandCompletedHandler;
        }

        #region NodeWatching
        public void NotifyNodeCustomPropertyChanged(string nodeID, string typeName)
        {
            if (CurrentPlugin != null)
                CurrentPlugin.Instance.WatchedNodeCustomPropertyChanged(nodeID, typeName);
        }

        public void NotifyNodePropertyChanged(string nodeID, string typeName)
        {
            if (CurrentPlugin != null)
                CurrentPlugin.Instance.WatchedNodePropertyChanged(nodeID, typeName);
        }

        public void NotifyNodeAdded(string parentID, string childID, string childTypename)
        {
            if (CurrentPlugin != null)
                CurrentPlugin.Instance.WatchedNodeAdded(parentID, childID, childTypename);
        }

        public void NotifyNodeRemoved(string parentID, string childID, string childTypename)
        {
            if (CurrentPlugin != null)
                CurrentPlugin.Instance.WatchedNodeAdded(parentID, childID, childTypename);
        }

        public void NotifyNodeMoved(string oldParentID, string newParentID, string childID, string childTypename)
        {
            if (CurrentPlugin != null)
                CurrentPlugin.Instance.WatchedNodeMoved(oldParentID, newParentID, childID, childTypename);
        }
        #endregion

        public override void OpenFile()
        { }

        public override void SaveFile()
        { }


        #region View Manipulation
        public override void View_LookAt(string entityID, float percentExtents)
        {
            // TODO: fix this camera currenview and "viewport0" indexing crap.. shoudlnt be hardcoded, but
            // currentView should be the editor view with so far the other is the  loginView 
            // TODO: which camera context is used during lookat anyway? all?
            Keystone.Entities.Entity target = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(entityID);

            AppMain._core.Viewports["viewport0"].Context.Viewpoint_MoveTo(target, 0.9f, false);
        }

        public override void Vehicle_TravelTo(string targetID)
        {
            if (AppMain.mPlayerControlledEntity != null && AppMain.mPlayerControlledEntity.GetEntityFlagValue("playercontrolled"))
            {
                // assign a heading and destination position to the vehicle's helm
                Predicate<Keystone.Elements.Node> match = (n) =>
                {
                    if (n.Name == "helm") return true;

                    return false;
                };
                Keystone.Entities.Entity helm = (Keystone.Entities.Entity)AppMain.mPlayerControlledEntity.FindDescendant(match);
                if (helm == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - No helm exists within player controlled vehicle.");
                    return;
                }
                Keystone.Entities.Entity target = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(targetID);
                if (target == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - invalid destination.");
                    return;
                }
                int[] dummy;



                Keystone.Types.Vector3d targetPosition = target.GlobalTranslation;
                Keystone.Types.Vector3d dir = target.GlobalTranslation - AppMain.mPlayerControlledEntity.GlobalTranslation;

                if (target is Keystone.Celestial.Body)
                {
                    Keystone.Types.Vector3d normal = Keystone.Types.Vector3d.Normalize(dir);
                    double radius = Keystone.Celestial.Temp.GetGeostationaryOrbitRadius((Keystone.Celestial.Body)target); // todo: move this call to our Keystone.Physics.Newtownian.cs
                    radius *= 2d;
                    targetPosition -= normal * radius;
                }

                // todo: what do we do if part-way through the translation, we want to switch to another target?  Do we first come to a complete stop and then set the new heading?
                //       Or do we just not allow partial way translations to new heading?  Well, there also needs to be a delay
                // between receiving a course update and actually engaging on that new course.

                // todo: currently its required to apply heading and destination but we should fix the code to auto compute the heading in the script and to create those maneuvers
                Keystone.Types.Vector3d heading = new Keystone.Types.Vector3d();
                
                // todo: since we for now just use steering behavior to move to a destination, "heading" is not used.
                heading.y = Keystone.Utilities.MathHelper.Heading2DDegrees(dir);
                helm.SetCustomPropertyValue("heading", heading, false, true, out dummy);
                helm.SetCustomPropertyValue("destination", targetPosition, false, true, out dummy);

                // todo: do we want to test implementation just using same code as Viewpoint_MoveTo() where
                // all we need to change is the time to reach the destination which we can compute based on the
                // distance to destination and the acceleration (or let's say acceleration "factor" of the ship?
                AppMain.mPlayerControlledEntity.SetCustomPropertyValue("maxspeed", 1000000000d, false, true, out dummy);
                AppMain.mPlayerControlledEntity.SetCustomPropertyValue("maxforce", 10000000d, false, true, out dummy);


                // todo: set the "state" from here to override the .Translation state that is set when the destination property is modified
                Keystone.Portals.ZoneRoot z;
                //z.GlobalCoordinatesToZoneCoordinates;
                // z.ZoneCoordinateToGlobalCoordinate (string zoneID, Vector3d position);
            }
        }

        /// <summary>
        /// Assumes the targetBody is not moving which is fine for 1.0
        /// </summary>
        /// <param name="targetBodyID"></param>
        public override void Vehicle_Orbit(string targetBodyID)
        {
            Keystone.Entities.Entity targetBody = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(targetBodyID);
            if (targetBody is Keystone.Celestial.Body)
            {
                Keystone.Vehicles.Vehicle vehicle = null;
                if (AppMain.mPlayerControlledEntity is Keystone.Vehicles.Vehicle)
                    vehicle = (Keystone.Vehicles.Vehicle)AppMain.mPlayerControlledEntity;

                if (vehicle == null) return;

                const uint NUM_OF_WAYPOINTS = 15;

                // get the helm of the selected vehicle
                Predicate<Keystone.Elements.Node> match = (n) =>
                {
                    if (n.Name == "helm") return true;

                    return false;
                };
                Keystone.Entities.Entity helm = (Keystone.Entities.Entity)vehicle.FindDescendant(match);
                if (helm == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - No helm exists within player controlled vehicle.");
                    return;
                }

                // todo: since the distance to the waypoint is relatively small, use aft thrusters to translate and not main engines
                //       The OnUpdate() script should determine that i think and set a max velocity for traversal to the current waypoint
                Keystone.Types.Vector3d vehiclePosition = vehicle.DerivedTranslation;

                // generate and assign the waypoints to the helm. NOTE: the waypoints must all be within the vehicle's current region
                double radius = ((Keystone.Celestial.Body)targetBody).Radius * 2f;


                Keystone.Types.Vector3d tangent1 = new Keystone.Types.Vector3d();
                Keystone.Types.Vector3d tangent2 = new Keystone.Types.Vector3d();
                bool test = Keystone.Utilities.MathHelper.CircleTangents_2(targetBody.DerivedTranslation, radius, vehiclePosition, ref tangent1, ref tangent2);

                double theta = Keystone.Utilities.MathHelper.AngleOfPointOnCircle(tangent1, targetBody.DerivedTranslation);
                theta *= Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS;
                // initialize with a starting angle which will be the injection point of the travelto to orbit injection
                Keystone.Types.Vector3d[] waypoints = Keystone.Utilities.MathHelper.GenerateOrbitalWaypoints(theta, targetBody.DerivedTranslation, radius, NUM_OF_WAYPOINTS);
                int[] dummy;



                helm.SetCustomPropertyValue("waypoints", waypoints, false, true, out dummy);




                int closestIndex = 0;
                //double distanceSquared = double.MaxValue;
                //for (int i = 0; i < waypoints.Length; i++)
                //{
                //    double dsquared = (waypoints[i] - vehiclePosition).LengthSquared();
                //    if (dsquared <= distanceSquared)
                //    {
                //        distanceSquared = dsquared;
                //        closestIndex = i;
                //    }
                //}

                Keystone.Types.Vector3d dir = waypoints[closestIndex] - vehiclePosition;
                double distance = dir.Length;
                dir = Keystone.Types.Vector3d.Normalize(dir);

                helm.SetCustomPropertyValue("waypoint_currentindex", closestIndex, false, true, out dummy);

                helm.SetCustomPropertyValue("waypoint_loop", true, false, true, out dummy);

                
                // todo: perhaps when calculating the tangent point, we use that as waypoint 0 and then construct the rest of the points from there
                // todo: while the ship is traversing the waypoints, i may need to do constant course corrections or maybe see if i can use the Intercept code to handle the fact that we cant head directly to the waypoint because we have existing velocity that needs to be cancelled out
                // todo: we should have 3 different orbit options high, medium, and low
                // todo: inject at a tangent to the orbital path
                // todo: set a max velocity for orbiting at a given radius.  Afterall, if we inject into the orbital path too fast, we can't easily change course to traverse the waypoints anymore.
                //       since we're not using gravitation, we could allow for this velocity to be higher than what the actual orbital velocity would be so that we can traverse around the world faster
                Keystone.Types.Vector3d heading = Keystone.Types.Vector3d.Zero();
                heading.y = Keystone.Utilities.MathHelper.Heading2DDegrees(dir);
                helm.SetCustomPropertyValue("heading", heading, false, true, out dummy);
                helm.SetCustomPropertyValue("destination", waypoints[closestIndex], false, true, out dummy);


                // todo: set helm state "waypoint_follow" = true;

                //                Keystone.Physics.Newtonian.GetTargetLeadingPositionQuadratic(vehicle.DerivedTranslation, vehicle.Velocity, Keystone.Types.Vector3d.Zero(), waypoints[i], Keystone.Types.Vector3d.Zero(), Keystone.Types.Vector3d.Zero(), distance, projectileSpeed, 0);
                //Keystone.Physics.Newtonian.GetTargetLeadingVelocityQuadratic();
                // todo: does our helm.OnUpdate() need to increment currentWaypointIndex (or loop) ?  where else would we do this?

                // https://www.youtube.com/watch?v=1s8fl8lCUs8
            }
        }


        public override void Vehicle_Intercept(string targetID)
        {
            if (AppMain.mPlayerControlledEntity != null && AppMain.mPlayerControlledEntity.GetEntityFlagValue("playercontrolled"))
            {
                // assign a heading and destination position to the vehicle's helm
                Predicate<Keystone.Elements.Node> match = (n) =>
                {
                    if (n.Name == "helm") return true;

                    return false;
                };
                Keystone.Entities.Entity helm = (Keystone.Entities.Entity)AppMain.mPlayerControlledEntity.FindDescendant(match);
                if (helm == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - No helm exists within player controlled vehicle.");
                    return;
                }
                Keystone.Entities.Entity target = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(targetID);
                if (target == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - invalid destination.");
                    return;
                }

                int[] dummy;
                double speed = 1; // 27000; // todo: i think this is max speeed and assumes we do not allow infinite velocity
                int iterations = 1;
                Keystone.Types.Vector3d dir = target.GlobalTranslation - AppMain.mPlayerControlledEntity.GlobalTranslation;
                Keystone.Types.Vector3d acceleration = new Keystone.Types.Vector3d(0, 0, 40000d); // AppMain.mLocalVehicle.Acceleration
                acceleration = Keystone.Types.Vector3d.Normalize(dir) * acceleration; // acceleration needs to already be in world space
                // todo: would be nice if we could run this computation in a seperate thread
                // todo: we don't necessarily know our vehicle's acceleration before the engines are enabled
                Keystone.Types.Vector3d targetFuturePosition = Keystone.Physics.Newtonian.GetTargetLeadingPositionQuadratic(AppMain.mPlayerControlledEntity.GlobalTranslation, AppMain.mPlayerControlledEntity.Velocity, acceleration, target.GlobalTranslation, target.Velocity, target.Acceleration, dir.Length, speed, iterations);
                Keystone.Types.Vector3d heading = new Keystone.Types.Vector3d();
                // todo: i think Utilitiess.MathHelper should exist in KeystoneStandardLibrary
                heading.y = Keystone.Utilities.MathHelper.Heading2DDegrees(targetFuturePosition - AppMain.mPlayerControlledEntity.GlobalTranslation);
                helm.SetCustomPropertyValue("heading", heading, false, true, out dummy);
                helm.SetCustomPropertyValue("destination", targetFuturePosition, false, true, out dummy);
                // todo: we may not want 0 acceleration when we reach destination.  we may want to match velocity and acceleration
                // todo: if the target distance + acceleration is too great, we cannot intercept (also take into account our own fuel levels)
                //      todo: also, we can add code that says you must be > 1AU from a large gravity well before your jump drives will activate so the distance to intercept needs to be before the target will jump away
                // todo: sensor operator should report changes to target acceleration and heading
                // todo: look at NPC behaviors for randomly walking to positions.  We need a behavior for patrolling the earth+moon area
            }
        }

        public override void Vehicle_Dock(string targetID)
        {
            if (AppMain.mPlayerControlledEntity != null && AppMain.mPlayerControlledEntity.GetEntityFlagValue("playercontrolled"))
            {
                // assign a heading and destination position to the vehicle's helm
                Predicate<Keystone.Elements.Node> match = (n) =>
                {
                    if (n.Name == "helm") return true;

                    return false;
                };
                Keystone.Entities.Entity helm = (Keystone.Entities.Entity)AppMain.mPlayerControlledEntity.FindDescendant(match);
                if (helm == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - No helm exists within player controlled vehicle.");
                    return;
                }
                Keystone.Entities.Entity target = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(targetID);
                if (target == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHost.Vehicle_TravelTo() - invalid destination.");
                    return;
                }

                int[] dummy;
                double speed = 1; // 27000; // todo: i think this is max speeed and assumes we do not allow infinite velocity
                int iterations = 1;
                Keystone.Types.Vector3d dir = target.GlobalTranslation - AppMain.mPlayerControlledEntity.GlobalTranslation;
                Keystone.Types.Vector3d acceleration = new Keystone.Types.Vector3d(0, 0, 40000d); // AppMain.mLocalVehicle.Acceleration
                acceleration = Keystone.Types.Vector3d.Normalize(dir) * acceleration; // acceleration needs to already be in world space
                // todo: would be nice if we could run this computation in a seperate thread
                // todo: we don't necessarily know our vehicle's acceleration before the engines are enabled
                Keystone.Types.Vector3d targetFuturePosition = Keystone.Physics.Newtonian.GetTargetLeadingPositionQuadratic(AppMain.mPlayerControlledEntity.GlobalTranslation, AppMain.mPlayerControlledEntity.Velocity, acceleration, target.GlobalTranslation, target.Velocity, target.Acceleration, dir.Length, speed, iterations);
                Keystone.Types.Vector3d heading = new Keystone.Types.Vector3d();
                // todo: i think Utilitiess.MathHelper should exist in KeystoneStandardLibrary
                heading.y = Keystone.Utilities.MathHelper.Heading2DDegrees(targetFuturePosition - AppMain.mPlayerControlledEntity.GlobalTranslation);
                helm.SetCustomPropertyValue("heading", heading, false, true, out dummy);
                helm.SetCustomPropertyValue("destination", targetFuturePosition, false, true, out dummy);
            }
        }
        #endregion

        #region SCENE
        public override string Scene_GetRoot()
        {
            return AppMain._core.SceneManager.Scenes[0].Root.ID;
        }
        #endregion

        #region Engine
        /// <summary>
        /// This is a version of WorkspaceManager.ConfigureViewport for our Plugins
        /// so that htey can create viewports for rendering previews of geometry
        /// </summary>
        /// <remarks>
        /// I don't really like this function existing.  I don't like it because
        /// it goes around our WorkspaceManager.  Can we modify this to always go
        /// throught he workspace and use the ConfigureViewport function?
        /// </remarks>
        /// <returns></returns>
        public override IntPtr Engine_CreateViewport()
        {
            // TODO: PickParameters should be passed in?  
            // what about an edit controller?
            // ugh i hate having this function.
            //
            // viewport control creation should be here in exe app not in keystone.dll
            ViewportEditorControl vpControl = new ViewportEditorControl();
            Keystone.Cameras.Viewport vp = Keystone.Cameras.Viewport.Create(vpControl, vpControl.Name, vpControl.RenderHandle);


            Keystone.Scene.Scene geometryPreviewScene = Keystone.Scene.Scene.CreatePreviewScene(AppMain._core.SceneManager, "geometryPreviewScene");

            RenderingContext context = new RenderingContext( null, geometryPreviewScene, vp,
                AppMain.NEARPLANE, AppMain.FARPLANE,
                AppMain.NEARPLANE_LARGE, AppMain.FARPLANE_LARGE,
                AppMain.MAX_VISIBLE_DISTANCE,
                AppMain.FOV * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS);

            context.ViewType = vpControl.View;
            context.ProjectionType = vpControl.Projection;

            vpControl.Context = context;

            return vpControl.RenderHandle;
        }

        public override string Engine_GetModPath()
        {
            return AppMain.MOD_PATH;
        }
        #endregion 

        #region IO
        /// <summary>
        /// Used for saving geometry data such as .tvm, .terrain, .tvp (particle systems), and .tva and potentially .interior 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="modsPath"></param>
        public override void Geometry_Save(string currentNodeID, string nodeID, string modsPath)
        {
            KeyCommon.Messages.Geometry_Save save = new KeyCommon.Messages.Geometry_Save();
            save.NewRelativePath = nodeID; // for geometry, nodeID should always match the relative resource path so that they can be properly shared
            save.CurrentRelativePath = currentNodeID; // NOTE: if this geomety is a primitive Mesh3d type, then the currentNodeID is just the primitive name and this is fine

            AppMain.mNetClient.SendMessage(save);
        }

        /// <summary>
        /// Renames a node belonging to just the specific parentID.  It does not iterate through all scene Nodes and updates their resources to the new ID.
        /// This is intended to be used only for Editing of the scene or prefab or resource. 
        /// NOTE: Because we are only renaming the resource, if its a Geometry with multiple groups, there's no need to auto-manage the GroupAttributes as the geometry Groups have not changed.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="newID"></param>
        /// <param name="parentID"></param>
        public override void Node_ResourceRename(string nodeID, string newID, string parentID)
        {
            KeyCommon.Messages.Node_RenameResource rename = new KeyCommon.Messages.Node_RenameResource();
            rename.OldResourceID = nodeID;
            rename.NewResourceID = newID;
            rename.ParentID = parentID;

            AppMain.mNetClient.SendMessage(rename);
        }

        // TODO: im confused as to why Node_Create() is not used here instead of a completely seperate command.
        //       Well, one thing Geometry_add does is it takes both the entityID and modelID whereas Node_Create just takes the parentID.
        //       This alows us to manage the Appearance and handle embedded animations, shaders, texture and materials.  The only problem is, the Appearance nodes are not
        //       assigned by the server so the IDs will not be there.  However, server has no need for nodes that are purely client side visuals.
        //       But why not just modify Node_Create to handle these cases? Why a seperate command?
        public override void Geometry_Add(string entityID, string modelID, string resourcePath, bool loadTextures, bool loadMaterials)
        {

            KeyCommon.Messages.Geometry_Add add = new KeyCommon.Messages.Geometry_Add();
            add.EntityID = entityID;
            add.ModelID = modelID;
            add.ResourcePath = resourcePath;
            add.LoadMaterials = loadMaterials;
            add.LoadTextures = loadTextures;

            AppMain.mNetClient.SendMessage(add);
        }

        public override void Entity_SavePrefab(string nodeID, string modsPath, string modName, string entryName)
        {

        	// TODO: check if modName is a zip Archive mod or a folder
            KeyEdit.GUI.AssetBrowserDialog browser = new KeyEdit.GUI.AssetBrowserDialog(AppMain._core.ModsPath, modName, new string[] {".KGBENTITY", ".KGBSEGMENT"});
            System.Windows.Forms.DialogResult result = browser.ShowSaveAsDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // TODO: better file name validity check
                // TODO: especially if i can prevent typing of "." and so no
                // extension by them is allowed.  instead i show ghosted ".kgbentity"
                // after what they type.  I could do that by superimposing a label
                // or owner drawing it...
                if (string.IsNullOrEmpty(browser.ModDBSelectedEntry)) return; 
                bool validFileName = Settings.Initialization.IsValidFileName (browser.ModDBSelectedEntry);
                //if (validFileName == false) return;


                string entry = System.IO.Path.Combine (browser.ModName, browser.ModDBSelectedEntry.ToLower());

                string extension = System.IO.Path.GetExtension(entry);
                if (extension.ToUpper() != ".KGBENTITY" && extension.ToUpper () != ".KGBSEGMENT")
                {
                    entry += ".kgbentity";
                }
                

                // create the message
                KeyCommon.Messages.Prefab_Save message = new KeyCommon.Messages.Prefab_Save();
                message.NodeID = nodeID;
                message.ModName = modName;
                message.EntryPath = entry;
                message.EntryName = null; // obsolete -  entryName2;

                AppMain.mNetClient.SendMessage(message);
            }
        }
        #endregion 
        
        #region Tasks
        // Tasks 
        public override void Task_Create (Game01.Messages.OrderRequest request) 
        {
        	// do we need to get a task id from server? similar to how Node_Create works? i dont think we need wait on it.  we can pass all parameters and 
                                         // then simply refresh the table when we are notified the task has been added to database which will first come via a "Task_Create_Record" from server
                                         // instructin client to add that record to local db.  TODO: I still need to figure out how NPCs create /modify tasks as far as server is concerned
         	try 
         	{
    	     	//TestSQLiteDB();	
            }
         	catch (SQLiteException ex)
         	{
         		System.Diagnostics.Debug.WriteLine (ex.Message);
         	}
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine (ex.Message);
            }

            // create and send the message
            Game01.Messages.Task_Create_Request message = new Game01.Messages.Task_Create_Request();
            //message.mAssignedByID = AppMain.mLocalVehicleID; // i think this should be a crew member name or the player's avatar name
            //message.mAssignedDateTime = DateTime.Now;
            //message.mAssignedStationID = "helm";
            //message.mTaskType = 1;
            //message.mPriority = 1;
            //message.mNotes1 = "Hello World!";

            //message.mArgs = // waypoint coords
            message.Request = request;

            AppMain.mNetClient.SendMessage(message);
        }
        
        // cancels the task and retires it to tasks_retired table? or does it actually delete it without inserting it as new record to retired table?
        public override void  Task_Delete (long taskID)
        {
        }
        
        // edits an existing task
        public override void  Task_Update(long taskID)
        {
        }
        
//		  obsolete - Task_Suspend is done via a "Update/Edit" of the task which sets the resolution to "suspended"
//        public override void  Task_Suspend(int taskID)
//        {
//        }
        
        private SQLiteConnection mDBConnection;
		private void CreateSQLiteDBConnection ()
		{
			// TODO: is there a way to change the default db path and database child names with awesomium?
            // TODO: I believe there is by changing the WebSession
			string path = System.IO.Path.Combine (AppMain.STARTUP_PATH,  @"Cache\databases\file__0\3");
			
			mDBConnection = new SQLiteConnection("Data Source=" + path);
			 
            mDBConnection.StateChange += OnSqliteDB_StateChange;

		}
        
		private void TestSQLiteDB()
		{

			if (mDBConnection == null) CreateSQLiteDBConnection ();
			
            using (SQLiteCommand cmd = mDBConnection.CreateCommand())
            {
                mDBConnection.Open();
                
                //SQLiteTransaction t =  mDBConnection.BeginTransaction();
                
//                // Create a new table
//                for (int i = 0; i < 4096; i++)
//                {
//                    cmd.CommandText = "CREATE TABLE NODE" + i + " ([ID] INTEGER PRIMARY KEY, [MyValue] NVARCHAR(256))";
//
//                    cmd.ExecuteNonQuery(); // Create the table, don't expect returned data
//                }
//                
//                //catch (SQLiteException ex)
//                //{
//                //    Debug.WriteLine(ex.Message); 
//                //}
//
//                t.Commit();
//                
//                // Insert something into the table
//                cmd.CommandText = "INSERT INTO FOO (MyValue) VALUES('Hello World')";
//                cmd.ExecuteNonQuery();

                // Read the values back out
                cmd.CommandText = "SELECT * FROM Tasks";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Id = {0}, Command = {1}", reader[0], reader[1]));
                    }
                }
                
                mDBConnection.Close();
        	}			
		}
		
		private void OnSqliteDB_Update (object sender, UpdateEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnSqliteDB_Update() - " + e.ToString());
			
			// TODO: notify quicklook to redraw if records added/removed/modified?
		}
		
		private void OnSqliteDB_StateChange (object sender, System.Data.StateChangeEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnSqliteDB_StateChange() - " + e.ToString());
			
            if (e.CurrentState == System.Data.ConnectionState.Open)
            {
            	//Update event seems to only be allowed to be set when the db is opened
				mDBConnection.Update += OnSqliteDB_Update;
            }
            else if (e.CurrentState == System.Data.ConnectionState.Closed)
            {
            	// NOTE: trying to remove this event handler after the connection is .Closed() fails
            	//mDBConnection.Update -= OnSqliteDB_Update;
            }
		}
        #endregion

        #region Node Manipulation
        /// <summary>
        /// Request the server to generate new node based on a type.  
        /// The server will generate a guid/id and
        /// send back the serialized node for creation on the relevant clients.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parentID"></param>
        public override void Node_Create(string type, string parentID)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_Create_Request(type, parentID);

            AppMain.mNetClient.SendMessage(message);
        }

        public override void Node_Create(string type, string parentID, Settings.PropertySpec[] properties)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_Create_Request(type, parentID, properties);

            AppMain.mNetClient.SendMessage(message);
        }

        /// <summary>
        /// Same as normal Node_Create above but includes a resourcePath
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="parentID"></param>
        public override void Node_Create(string typeName, string parentID, string resourcePath, string browseDialogfilter)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
                                               // create message
            KeyCommon.Messages.MessageBase message = null;

            if (typeName == "Material")
            {
                message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                AppMain.mNetClient.SendMessage(message);

            }
            else if (string.IsNullOrEmpty(resourcePath))
            {
                // TODO: need to validate that current shader does not exist
                // otherwise user is only able to change the existing not add a second
                //                KeyEdit.GUI.AssetBrowserDialog browser =
                //                    new KeyEdit.GUI.AssetBrowserDialog(AppMain._core.ModsPath, modName, GetAllowedExtensions(typeName));

                System.Windows.Forms.FileDialog browser = new System.Windows.Forms.OpenFileDialog();
                browser.InitialDirectory = AppMain.MOD_PATH;
                browser.RestoreDirectory = false;
                browser.Filter = browseDialogfilter; // eg:  "Images|*.png;*.bmp;*.jpg";
                System.Windows.Forms.DialogResult result = browser.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                    return;


                //              resourcePath = new KeyCommon.IO.ResourceDescriptor(browser.ModName.ToLower(), browser.ModDBSelectedEntry.ToLower()).ToString();
                resourcePath = browser.FileName;
                int modPathLength = AppMain.MOD_PATH.Length + 1;
                // use relative path to "mods" folder so that code works on any install folder location
                resourcePath = resourcePath.Remove(0, modPathLength);


                if (string.IsNullOrEmpty(resourcePath))
                    return;


                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(resourcePath);

                string ext = System.IO.Path.GetExtension(descriptor.EntryName);

                // what type of resource is this? 
                switch (ext.ToUpper())
                {
                    case ".TVP": // particle 
                        message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                        break;
                    case ".CSS":
                        message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                        break;
                    case ".FX":
                        message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                        break;
                    case ".OBJ":
                    case ".TVM":
                    case ".X":
                    case ".TVA":
                        message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                        break;
                    case ".DDS":
                    case ".PNG":
                    case ".BMP":
                    case ".JPG":
                    case ".GIF":
                    case ".TGA":
                        message = new KeyCommon.Messages.Node_Create_Request(typeName, resourcePath, parentID);
                        break;
                    case ".KGBBEHAVIOR":
                        message = new KeyCommon.Messages.Prefab_Load(resourcePath);
                        ((KeyCommon.Messages.Prefab_Load)message).ParentID = parentID;
                        break;
                    case ".KGBENTITY":
                    case ".KGBSEGMENT":
                        message = new KeyCommon.Messages.Prefab_Load(resourcePath);
                        ((KeyCommon.Messages.Prefab_Load)message).ParentID = parentID;
                        break;
                    default:
                        break;
                }


                AppMain.mNetClient.SendMessage(message);
            }
        }

        public override string Node_GetName(string nodeID)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodeID);
            if (node == null) return null;
            return node.Name;
        }

        public override string Node_GetDescendantByName(string startingNode, string descendantName)
        {
            Keystone.Elements.IGroup group = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(startingNode);
            if (group == null) return null;

            Predicate<Keystone.Elements.Node> match = (n) =>
            {
                if (n.Name == descendantName) return true;

                return false;
            };
            Keystone.Elements.Node result = group.FindDescendant(match);
            if (result == null) return null;
            return result.ID;
        }

        public override bool Node_HasDescendant(string groupNode, string potentialDescendant)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(potentialDescendant);
            if (node == null) return false;
            return node.IsDescendantOf(groupNode); 
        }

        public override string Node_GetChildOfType(string groupNodeID, string typeName)
        {
            // returns the first node of that type found
            Keystone.Elements.IGroup group = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(groupNodeID);
            if (group == null) return null;

            bool recurse = false;
            Keystone.Elements.Node result = group.FindDescendantOfType(typeName, recurse);
            if (result == null) return null;
            return result.ID;
        }

        public override string[] Node_GetChildrenOfType(string groupNodeID, string typeName)
        {
            // typically used to grab all GroupAttribute nodes under a DefaultAppearance
            Keystone.Elements.IGroup group = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(groupNodeID);
            if (group == null) return null;

            bool recurse = false;
            Keystone.Elements.Node[] results = group.FindDescendantsOfType(typeName, recurse);
            if (results == null) return null;

            string[] ids = new string[results.Length];
            for (int i = 0; i < results.Length; i++)
                ids[i] = results[i].ID;

            return ids;
        }

        public override void Node_GetChildrenInfo(string groupNodeID, string[] filteredTypes, out string[] childIDs, out string[] childNodeTypes)
        {
            childIDs = null;
            childNodeTypes = null;
            // note: we dont query PluginChangesSuspended unless we are actually trying to change a value, not simply read it
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(groupNodeID);
            if (node == null || !(node is Keystone.Elements.IGroup)) return;

            ((Keystone.Elements.IGroup)node).GetChildIDs(filteredTypes, out childIDs, out childNodeTypes);
        }

        public override string Node_GetTypeName(string nodeID)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodeID);
            if (node == null) return "";

            return node.TypeName;
        }

        public override void Node_Remove(string nodeID, string parentID)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_Remove(nodeID, parentID);

            AppMain.mNetClient.SendMessage(message);
        }

        public override void Node_MoveChildOrder(string parentID, string nodeID, bool down)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_MoveChildOrder(parentID, nodeID, down);

            AppMain.mNetClient.SendMessage(message);
        }

        /// <summary>
        /// Moves an existing node from it's current parent to a new parent that will be created.
        /// </summary>
        /// <param name="typeName">The typename of the new node that will be created. It must
        /// be of type IGroup so that the existing nodeID can be moved under it.</param>
        /// <param name="parentID">The parent which the existing NodeID will be removed and
        /// to which the new node will be added as child instead.</param>
        /// <param name="nodeID">The existing node which will be added under the new node.</param>
        public override void Node_InsertUnderNewNode(string typeName, string parentID, string nodeID)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_InsertUnderNew_Request(typeName, parentID, nodeID);

            AppMain.mNetClient.SendMessage(message);
        }

        private string[] GetAllowedExtensions(string typename)
        {
            switch (typename)
            {
                case "Entity":
                    return new string[] {".KGBENTITY", ".KGBSEGMENT"};
                case "Actor3d":
                    return new string[] {".OBJ", ".TVM", ".X",".TVA"};
                case "Mesh3d":
                    return new string[] {".OBJ", ".TVM", ".X"};
                case "DomainObject":
                    return new string[] {".CSS", ".CS", ".TXT"};
                default:
                    return null;
            }
        }

        /// <summary>
        /// // replace a resource with another.  Since it's a resource and the ID is 
        /// fixed to the resource name, the server doesnt have to receive a "Create" 
        /// request command to generate the shared GUID
        /// </summary>
        /// <param name="oldResourceID"></param>
        /// <param name="newResourceID"></param>
        /// <param name="parentID"></param>
        public override void Node_ReplaceResource(string oldResourceID, string newResourceID, string newTypeName, string parentID) 
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.

            if (string.IsNullOrEmpty(newResourceID))
            {
            	
            	if (newTypeName == "Material")
            	{
            		// just popup a dialog for a name
            		string userText = null;
            		System.Windows.Forms.DialogResult result = KeyPlugins.StaticControls.InputBox ("Enter name of Material:", "enter new material name", ref userText);
            		if (result ==  System.Windows.Forms.DialogResult.OK)
            		{
            			Keystone.Resource.IResource res = Keystone.Resource.Repository.Get (userText);
            			if (res != null)
            			{
            				newResourceID = userText;
            			}
            		}
            	}
            	else // browse mod database 
            	{
            		System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            		openFile.InitialDirectory = AppMain.MOD_PATH;
            		openFile.RestoreDirectory = false;
            		System.Windows.Forms.DialogResult result = openFile.ShowDialog ();
	                //KeyEdit.GUI.AssetBrowserDialog browser = new KeyEdit.GUI.AssetBrowserDialog(AppMain._core.ModsPath, "common.zip");
	                //System.Windows.Forms.DialogResult result = browser.ShowDialog();
	
	                if (result == System.Windows.Forms.DialogResult.OK)
	                {
	                	string resourcePath = openFile.FileName;
                		int modPathLength = AppMain.MOD_PATH.Length + 1;
                		// use relative path to "mods" folder so that code works on any install folder location
                		resourcePath = resourcePath.Remove (0, modPathLength);
                
	                    // TODO: there is no check that the new resource is of the proper type!
	                    // i.e can't replace a texture with a .css script or vice versa!
	                    newResourceID = resourcePath;
	                }
            	}
            }

            // if still null, user canceled browse so just abort
            if (string.IsNullOrEmpty(newResourceID)) return;

            KeyCommon.Messages.MessageBase message = null;
            message = new KeyCommon.Messages.Node_ReplaceResource(oldResourceID, newResourceID, newTypeName, parentID);

            AppMain.mNetClient.SendMessage(message);
            
        }

        public override void Node_Paste(string nodeID, string parentID)
        {
            throw new NotImplementedException();
        }
        public override void Node_Copy(string nodeID, string parentID)
        {
            throw new NotImplementedException();
        }
        public override void Node_Cut(string nodeID, string parentID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Property value change needs to be sent across the wire.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="propertyName"></param>
        /// <param name="type"></param>
        /// <param name="newValue"></param>
        public override void Node_ChangeProperty(string nodeID, string propertyName, Type type, object newValue)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(nodeID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.ChangeNodeProperty() - NodeID not set.");
                return;
            }
            Settings.PropertySpec spec = new Settings.PropertySpec(propertyName, type.Name);
            spec.DefaultValue = newValue;

            KeyCommon.Messages.Node_ChangeProperty changeProperty = new KeyCommon.Messages.Node_ChangeProperty();
            changeProperty.Add(spec);
            changeProperty.NodeID = nodeID;

            AppMain.mNetClient.SendMessage(changeProperty);
        }


        // TODO: problem here is, we aren't guaranteed to get the property value during the main thread which is
        //       guaranteed to be after any temporary changes to node propertly values such as "blendingmode" that gets
        //       altered for Vehicie.Model during Floorplan rendering for example.
        //       It's not even the Repository.Get() that is problematic.  That just gets a refefence to the node.  The 
        //       problem is that on node.GetProperties() we don't know what state the rendering loop is in and the node's properties
        //       can be changing on multiple threads.
        //       What we need is a AppMain.mNetClient.SendMessage (getProperties) and then have the plugin be able to get the properties during ProcessCommandCompleted() somehow
        //       wait, would that even solve the problem? because ProcessCommandCompleted() occurs on render thread.
        //       also, if both floorplan and mainviewer are both visible and rendering, its just a race condition to determine which properties are set on Vehiciel.Model.Appearance are set.
        //
        //       We could perhaps tell the plugin "these are the properties for node type DefaultAppearance with id xyz" and the plugin has to update the correct GUI elements
        //       We have  Keystone.IO.NodeState.cs type for SceneReader, maybe we can use this to cache a copy of the node state and then as far as the Plugin is concerned, it is getting the real node state, but
        //       EditorHost simply caches them in advance.  Remember, just caching the actual nodes doesn't help us because they are a reference and the properties can still change.
        //       Maybe when we "SelectEntity" we just grab and cache the entire branch so that Node_GetProperty() and Node_GetProperties() are already available... but we'd still want to monitor changes 
          
        //       Perhaps using asynchronous monitoring can allow us to do things like update the position, rotation and scale vars in the GUI in realtime  But ideally we'd only want them for changed values
        //       We could add a method PushUpdates() to the plugin to accompany the basic SelectEntity
        //       During a PushUpdates() we can compare PropertySpecs[] and only update the values that have changed
        //

        // Getter's never needs to go thru the server, it always stays local.  Period.
        // That's because the client already has that Node, it just has to get to the plugin.  Changing the value is
        // different because it has to change local and server (undo'ing if operation is not allowed or cant be performed
        // for some reason). 
        public override object Node_GetProperty(string nodeID, string propertyName)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodeID);
            if (node == null) return null;

            // get all properties initially since it's a fast operation
            Settings.PropertySpec[] specs = node.GetProperties(false);

            // find the specific property requested by the user
            if (specs != null)
                for (int i = 0; i < specs.Length; i++)
                    if (specs[i].Name == propertyName)
                        return specs[i].DefaultValue;

            return null;
        }

        public override Settings.PropertySpec[] Node_GetProperties(string nodeID)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodeID);
            if (node == null) return null;

            Settings.PropertySpec[] specs = node.GetProperties(false);
            return specs;
        }

        public override bool Node_GetFlagValue(string nodeID, string flagName)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodeID);
            if (node == null) return false;

            return node.GetFlagValue(flagName);
        }

        public override void Node_SetFlagValue(string nodeID, string flagName, bool value)
        {
            if (PluginChangesSuspended) return;
            if (string.IsNullOrEmpty(nodeID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Entity_SetFlagValue() - EntityID not set.");
                return;
            }

            KeyCommon.Messages.Node_ChangeFlag changeFlag = new KeyCommon.Messages.Node_ChangeFlag();
            changeFlag.Add(flagName, value);
            changeFlag.NodeID = nodeID;
            changeFlag.FlagType = "Node";
            AppMain.mNetClient.SendMessage(changeFlag);
        }
        #endregion

        #region Models
        public override bool Model_GetFlagValue(string entityID, string flagName)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(entityID);
            if (node == null || !(node is Keystone.Elements.Model)) return false;

            return ((Keystone.Elements.Model)node).GetFlagValue(flagName);
        }

        // TODO: what if to set a flag value we instead
        // get the flag then re-set the entire flag value?
        public override void Model_SetFlagValue(string entityID, string flagName, bool value)
        {
            if (PluginChangesSuspended) return;
            if (string.IsNullOrEmpty(entityID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Model_SetFlagValue() - ModelID not set.");
                return;
            }

            KeyCommon.Messages.Node_ChangeFlag changeFlag = new KeyCommon.Messages.Node_ChangeFlag();
            changeFlag.Add(flagName, value);
            changeFlag.NodeID = entityID;
            changeFlag.FlagType = "Model";
            AppMain.mNetClient.SendMessage(changeFlag);
        }
        #endregion

        #region Geometry
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelID"></param>
        /// <param name="geometryID"></param>
        /// <param name="groupName"></param>
        /// <param name="groupType">Pointsprite, Billboard, Minimesh</param>
        /// <param name="groupClass"></param>
        public override void Geometry_CreateGroup(string modelID, string geometryID, string groupName, int groupType, int groupClass = 0, string meshPath = null)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(geometryID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Geometry_CreateGroup() - geometryID not set.");
                return;
            }

            const int MAX_PARTICLES = 32000;
            int maxPartciles = 1;
            if (groupClass == 0)
            {
                string result = "";

                KeyPlugins.StaticControls.InputBox("Max Particles", "Max Particles:", ref result);
                if (int.TryParse(result, out maxPartciles))
                {
                    if (maxPartciles <= 0 || maxPartciles > MAX_PARTICLES)
                    {
                        System.Windows.Forms.MessageBox.Show("Invalid number of particles.");
                        return;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid number of particles.");
                    return;
                }

            }

            KeyCommon.Messages.Geometry_CreateGroup createGroup = new KeyCommon.Messages.Geometry_CreateGroup();
            createGroup.ModelID = modelID;
            createGroup.GeometryNodeID = geometryID;
            createGroup.Name = groupName;
            createGroup.GroupClass = groupClass;
            createGroup.MaxParticles = maxPartciles;
            createGroup.GroupType = groupType;
            createGroup.MeshPath = meshPath;
            AppMain.mNetClient.SendMessage(createGroup);
        }

        // TODO: I could just prevent removal and enforce an entirely new particle system must be created so that we don't have to worry about aligning GroupAttributes with Emitter indices
        public override void Geometry_RemoveGroup(string modelID, string geometryID, int groupIndex, int groupClass)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(geometryID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Geometry_CreateGroup() - NodeID not set.");
                return;
            }
            // todo: should remove corrresponding GroupAttribute
            KeyCommon.Messages.Geometry_RemoveGroup removeGroup = new KeyCommon.Messages.Geometry_RemoveGroup();
            removeGroup.ModelID = modelID;
            removeGroup.GroupIndex = groupIndex;
            removeGroup.GeometryNodeID = geometryID;
            removeGroup.GroupClass = groupClass;

            AppMain.mNetClient.SendMessage(removeGroup);
        }
        /// <summary>
        /// Group Properties need to be sent across the wire
        /// </summary>
        /// <param name="geometryID"></param>
        /// <param name="groupIndex"></param>
        /// <param name="propertyName"></param>
        /// <param name="typename"></param>
        /// <param name="newValue"></param>
        /// <param name="groupClass">0 = emitter, 1 = attractor</param>
        public override void Geometry_ChangeGroupProperty(string geometryID, int groupIndex, string propertyName, string typename, object newValue, int groupClass = 0)
        {
            if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(geometryID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Geometry_ChangeGroupProperty() - NodeID not set.");
                return;
            }
            Settings.PropertySpec spec = new Settings.PropertySpec(propertyName, typename);
            spec.DefaultValue = newValue;

            KeyCommon.Messages.Geometrty_ChangeGroupProperty changeGeometryGroupProperty = new KeyCommon.Messages.Geometrty_ChangeGroupProperty();
            changeGeometryGroupProperty.GroupIndex = groupIndex;
            changeGeometryGroupProperty.GeometryNodeID = geometryID;
            changeGeometryGroupProperty.GroupClass = groupClass;

            changeGeometryGroupProperty.Add(groupIndex, spec, groupClass);
            AppMain.mNetClient.SendMessage(changeGeometryGroupProperty);
        }

        public override object Geometry_GetGroupProperty(string geometryID, int groupIndex, string propertyName, int geometryParams = 0)
        {
            Keystone.Elements.ParticleSystem node = (Keystone.Elements.ParticleSystem)Keystone.Resource.Repository.Get(geometryID);
            if (node == null) return null;

            return node.GetProperty(groupIndex, propertyName, geometryParams);
        }

        public override Settings.PropertySpec[] Geometry_GetGroupProperties(string geometryID, int groupIndex, int geometryParams = 0)
        {
            Keystone.Elements.ParticleSystem node = (Keystone.Elements.ParticleSystem)Keystone.Resource.Repository.Get(geometryID);
            if (node == null) return null;

            Settings.PropertySpec[] specs = node.GetProperties(groupIndex, geometryParams, false);
            return specs;
        }

        public override object Geometry_GetStatistic(string geometryID, string statName)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(geometryID);
            if (node == null || !(node is Keystone.Elements.Geometry)) return false;

            return ((Keystone.Elements.Geometry)node).GetStatistic(statName);
        }

        public override void Geometry_ResetTransform(string geometryID, Keystone.Types.Matrix m)
        {
            if (PluginChangesSuspended) return;
            if (string.IsNullOrEmpty(geometryID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Geometry_ResetTransform() - geometryID not set.");
                return;
            }

            // TODO: if the transform results in save from original .obj to
            // .tvm, then we MUST switch the Mesh3d reference in the .kgbentity or
            // we will never be able to parse the .obj extension file which is now actually
            // a .tvm
            // TODO: in general, the idea would be if we want to rename a resource but also
            // update all entities that use it, then 
            // TODO: Perhaps the best alternative is to notify the user that a new model with different
            // extnesion created and then that will remind them to change the resource to the tvm. And hopefull
            // the groups will still match up under the appearance.
            KeyCommon.Messages.Geometry_ResetTransform reset = new KeyCommon.Messages.Geometry_ResetTransform();
            reset.Transform = m;
            reset.GeometryID = geometryID;

            AppMain.mNetClient.SendMessage(reset);
        }
        #endregion 

        #region Entities
        public override string Entity_GetGUILayout(string entityID, KeyCommon.Traversal.PickResultsBase pickDetails)
        {
        	// TODO: ideally i could just pass the damn pick result
            // Try to get the entity instance from the local repository
            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(entityID);
            string markup = null;
            if (entity != null)
            {
                markup = (string)entity.Execute("GUILayout_GetMarkup", new object[] { entityID, pickDetails});
            }
            return markup;
        }

        public override void Entity_GUILinkClicked(string entityID, string scriptedMethodName, string linkName)
        {
            // Try to get the entity instance from the local repository
            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(entityID);
            if (entity != null)
            {
                entity.Execute("GUILayout_LinkClick", new object[] { entityID, linkName });
            }
        }

        public override uint Entity_GetUserTypeIDFromString (string userTypeName)
        {
            // NOTE: if we edit the enum's and delete or rename a member, then this will fail
            return (uint)Enum.Parse(typeof(Game01.Enums.COMPONENT_TYPE), userTypeName);
        }

        public override string Entity_GetUserTypeStringFromID(uint userTypeID)
        {

            foreach (var enumValue in typeof(Game01.Enums.COMPONENT_TYPE).GetEnumValues())
            {
                if (userTypeID == (uint)enumValue)
                    return  enumValue.ToString();
            }

            return null;
        }

        public override string[] Entity_GetUserTypeIDsToString()
        {
            List<string> names = new List<string>();
            foreach (var enumValue in typeof(Game01.Enums.COMPONENT_TYPE).GetEnumValues())
            {
                names.Add (enumValue.ToString());
                uint value = (uint)enumValue;
            }

            return names.ToArray();
        }

        public override Settings.PropertySpec[] Entity_GetCustomProperties (string sceneName, string entityID, string entityTypename)
        {
            Settings.PropertySpec[] customProperties;

            // Try to get the entity instance from the local repository
            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(entityID);
            
            if (entity != null)
            {
                // get all custom properties 
                customProperties = entity.GetCustomProperties(false);
                if (customProperties == null) return null;

                Settings.PropertySpec[] clonedProperties = new Settings.PropertySpec[customProperties.Length];
                for (int i = 0; i < customProperties.Length; i++)
                {
                    //System.Diagnostics.Debug.WriteLine(i.ToString() + ") " + customProperties[i].Name + " Type = " + customProperties[i].TypeName);

                    // NOTE: We create new copies of the properties because we do not want
                    // the fully qualified typename to be set in the originals. 
                    Settings.PropertySpec clone = new Settings.PropertySpec();

                    clone.Name = customProperties[i].Name;
                    clone.DefaultValue = customProperties[i].DefaultValue;
                    // the fully qualified name is required for property to display in propertyGrid
                    clone.TypeName = KeyCommon.Helpers.ExtensionMethods.GetFullyQualifiedTypeName(customProperties[i].TypeName);
                    clone.EditorTypeName = customProperties[i].EditorTypeName;
                    clone.ConverterTypeName = customProperties[i].ConverterTypeName;
                    clone.Category = customProperties[i].Category;
                    clone.Description = customProperties[i].Description;
                    clonedProperties[i] = clone;
                }

                return clonedProperties;
            }
            else
            {
                // TODO: client can first check it's own xmldb which will hold best
                // available intel at the time.
                // TODO: also send request to server to see if we're allowed to see
                // this data or a newer version of it (eg conduct sensor scan that succeeds
                // and reveals full current state of data).  
                // In editor mode, obviously yes, but in runtime of game, depends.
                // How do we handle the fact that the call is no sychronous though
                // since it must go thru server? how do we know to send this info
                // to plugin or to quicklook bar or workspace?
                //
                // I suppose whenever we recv the results from server, we can in addition
                // to updating our local copy, save to db and pass to all open workspaces
                // so that they can update if applicable?
                KeyCommon.Messages.Entity_GetCustomProperties request =
                    new KeyCommon.Messages.Entity_GetCustomProperties(sceneName, entityID, entityTypename);

                AppMain.mNetClient.SendMessage(request);
                return null;
            }
        }

        public override object Entity_GetCustomPropertyValue(string entityID, string propertyName)
        {
            // Try to get the entity instance from the local repository
            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(entityID);
            
            if (entity != null)
            {
                return entity.GetCustomPropertyValue (propertyName);
            }
            else 
            {
                throw new NotImplementedException();
            }

        }
        /// <summary>
        /// Custom Property value change needs to be sent across the wire.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        public override void Entity_SetCustomPropertyValue(string entityID, string propertyName, string typeName, object newValue)
        {
            // TODO: scripts also make changes via this plugin interface so suspending changes here is no good!
            //       trying to suspend changes is probably a bad idea.
            //if (PluginChangesSuspended) return;// user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(entityID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Entity_ChangeCustomPropertyValue() - EntityID not set.");
                return;
            }
            Settings.PropertySpec spec = new Settings.PropertySpec(propertyName, typeName);
            spec.DefaultValue = newValue;

            KeyCommon.Messages.Entity_ChangeCustomPropertyValue changeCustomProperty = new KeyCommon.Messages.Entity_ChangeCustomPropertyValue();
            changeCustomProperty.Add(spec, KeyCommon.Simulation.PropertyOperation.Replace);
            changeCustomProperty.EntityID = entityID;

            changeCustomProperty.SetFlag(KeyCommon.Messages.Flags.SourceIsClient);
            // TODO: commands need to be cached as unconfirmed so they can be rolled back
            // if invalid... however this is complicated
            // do all commands need to be so?  should they be undone if failed or not ever
            // performend and only performed when confirmed.  If so, all SendMessage
            // should result in messages being sent to "UnConfirmed" dictionary
            // NOTE: THe main issue that got us to thinking about this again was validation
            //       during "build" where custom properties are modified.  Perhaps in the short term
            //       we table validation.  It's not needed for kickstarter...  Let's just
            //       work on designing the panels.  Or maybe we can do client and server side validation
            //       in the short term and assume that client side validation  is exactly as server side
            //     

            AppMain.mNetClient.SendMessage(changeCustomProperty);
        }

        public override bool Entity_GetFlagValue(string entityID, string flagName)
        {
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(entityID);
            if (node == null || !(node is Keystone.Entities.Entity)) return false;

            return ((Keystone.Entities.Entity)node).GetEntityFlagValue(flagName);
        }

        public override void Entity_SetFlagValue(string entityID, string flagName, bool value)
        {
            if (PluginChangesSuspended) return;
            if (string.IsNullOrEmpty(entityID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Entity_SetFlagValue() - EntityID not set.");
                return;
            }

            KeyCommon.Messages.Node_ChangeFlag changeFlag = new KeyCommon.Messages.Node_ChangeFlag();
            changeFlag.Add(flagName, value);
            changeFlag.NodeID = entityID;
            changeFlag.FlagType = "Entity";
            AppMain.mNetClient.SendMessage(changeFlag);
        }
        #endregion

        #region Appearance
        public override Settings.PropertySpec[] Appearance_GetShaderParameters(string appearanceID)
        {
            Keystone.Appearance.GroupAttribute appearance = (Keystone.Appearance.GroupAttribute)Keystone.Resource.Repository.Get(appearanceID);
            if (appearance == null) return null;

            // get all custom properties 
            Settings.PropertySpec[] shaderParameters = appearance.GetShaderParameters();

            if (shaderParameters == null) return null;

            Settings.PropertySpec[] clonedProperties = new Settings.PropertySpec[shaderParameters.Length];
            for (int i = 0; i < shaderParameters.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(i.ToString() + ") " + shaderParameters[i].Name + " Type = " + shaderParameters[i].TypeName);

                // NOTE: We create new copies of the properties because we do not want
                // the fully qualified typename to be set in the originals. 
                Settings.PropertySpec clone = new Settings.PropertySpec();

                clone.Name = shaderParameters[i].Name;
                clone.DefaultValue = shaderParameters[i].DefaultValue;
                // the fully qualified name is required for property to display in propertyGrid
                clone.TypeName = KeyCommon.Helpers.ExtensionMethods.GetFullyQualifiedTypeName(shaderParameters[i].TypeName);

                // ConverterTypeName used to convert user input into correct property value
                clone.ConverterTypeName = KeyCommon.Helpers.ExtensionMethods.GetFullQualifiedTypeConverterName(shaderParameters[i].TypeName);
                //EditorTypeName used as custom editor
                //clone.EditorTypeName  
                clonedProperties[i] = clone;
            }

            return clonedProperties;
        }

        public override void Appearance_ChangeShaderParameterValue(string appearanceID, string parameterName, string typeName, object newValue)
        {
            if (PluginChangesSuspended) return;  // user is not allowed to make changes via plugin interface.
            if (string.IsNullOrEmpty(appearanceID))
            {
                System.Diagnostics.Debug.WriteLine("EditorHost.Entity_ChangeShaderParameterValue() - EntityID not set.");
                return;
            }
            Settings.PropertySpec spec = new Settings.PropertySpec(parameterName, typeName);
            spec.DefaultValue = newValue;

            KeyCommon.Messages.Entity_ChangeCustomPropertyValue changeCustomProperty = new KeyCommon.Messages.Entity_ChangeCustomPropertyValue();
            changeCustomProperty.Add(spec, KeyCommon.Simulation.PropertyOperation.Replace);
            changeCustomProperty.EntityID = appearanceID;

            changeCustomProperty.SetFlag(KeyCommon.Messages.Flags.SourceIsClient);

            AppMain.mNetClient.SendMessage(changeCustomProperty);
        }
        #endregion

        #region Animations
        // TODO: add overload to play an animation by name
        public override void Entity_PlayAnimation(string entityID, string animationNodeID)
        {
            Keystone.Resource.IResource res = Keystone.Resource.Repository.Get(entityID);
            if (res == null) return;
            if (!(res is Keystone.Entities.ModeledEntity)) return;

            Keystone.Entities.ModeledEntity ent = (Keystone.Entities.ModeledEntity)res;

            Keystone.Animation.Animation anim = (Keystone.Animation.Animation)Keystone.Resource.Repository.Get(animationNodeID);
            if (anim == null) return;
            
            // NOTE: Setting the animation is same as playing it.  Otherwise we
            // can clear the animmation to stop it or explicily call stop 
            ent.Animations.Play(anim.Name);
            //ent.Animations.Play();

        }

        // TODO: this needs a clip ID too perhaps or else the keyframe is a value from 0 - duration
        public override double Entity_GetCurrentKeyFrame(string animationNodeID)
        {
            Keystone.Resource.IResource res = Keystone.Resource.Repository.Get(animationNodeID);
            if (res == null) return 0f;
            if (!(res is Keystone.Animation.Animation)) return 0f;

            Keystone.Animation.AnimationClip clip = (Keystone.Animation.AnimationClip)res;
  //          throw new NotImplementedException ();
            double currentKeyFrame = 0; //= ani.CurrentKeyFrame;
            return currentKeyFrame;
        }

        public override void Entity_StopAnimation(string entityID, string animationNodeID)
        {
            Keystone.Resource.IResource res = Keystone.Resource.Repository.Get(entityID);
            if (res == null) return;
            if (!(res is Keystone.Entities.ModeledEntity)) return;

            Keystone.Entities.ModeledEntity ent = (Keystone.Entities.ModeledEntity)res;

            Keystone.Animation.Animation anim = (Keystone.Animation.Animation)Keystone.Resource.Repository.Get(animationNodeID);
			if (anim == null) return;

            ent.Animations.Stop(anim.Name);
        }

        #endregion
    }
}
