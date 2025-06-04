using System;
using System.Collections.Generic;
using System.Diagnostics;
using KeyCommon.Traversal;
using KeyEdit.Database;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Types;
using KeyEdit.ContentCreation;

namespace KeyEdit.Workspaces.Huds
{
    public class NavigationHud : ClientHUD
    {
       		
        // only enough information to create the renderables
        private struct OrbitInfo
        {
            public string BodyID;
            public string BodyTypeName;
            public string BodyParent;

            public float OrbitalRadius;
            public float Eccentricity;
            public float SemiMajorAxis;
            public float OrbitalProcession;
            public float OrbitalInclination;

        }

        
        private enum NavigationViewType
        {
            StarMap,
            SystemMap,
            PlanetaryMap
        }

        internal enum MapMode
        {
            None  = 0,
        	Galaxy = 1,
        	SingleSystem = 2,
        	PlanetaryMap = 3
        }
        private struct NavigationViewState
        {

            public string ID { get; set; }
            public string TypeName { get ; set ; }

            // camera state
            //public float Zoom;
        }

        private bool DRAW_AU_MARKERS = false; // since our distance scaling is linear, we no longer need this.
        private bool GENERATE_ALTITUDE_LINES = false;

        // Nycterent 002A Preview - Procedural Solar System and navigation
        // also note halfway through the video it shows power curcuit management
        // http://www.youtube.com/watch?v=el6tfwaiB8E

        const float VEHICLE_SCREENSPACE_SIZE = 0.025f;
        const float STAR_MESH_RADIUS = 0.0325f; // todo: why don't i compute the radius for stars, meshes and moons as a percent screenspace size like i do with the vehicle?
        const float PLANET_MESH_RADIUS = 0.0165f;
        const float MOON_MESH_RADIUS = 0.0075f;// 0.00375f;

        // state vars
        MapMode mLastMode;
        Keystone.Vehicles.Vehicle mLastVehicle;
        private bool mWaypointTabsChanged = false;
        private string mCurrentRegionID;

        MapMode mMode = MapMode.SingleSystem;
        Vector3d mGalaxyScale;
        const double SINGLE_SYSTEM_SCALE = 1d; // todo: this should be configurable from the GUI
        Vector3d mSingleSystemScale;
        const double mWorldScaleModifier = 1d;
        const double SATELLITE_SYSTEM_SCALE = .00001;
        Vector3d mSatelliteSystemScale;
        Vector3d mInvGalaxyScale;
        //Vector3d mInvSingleSystemScale;

        ModeledEntity mCurrentView;
        ModeledEntity mGalacticView;                
        ModeledEntity mSingleSystemView;
        ModeledEntity mPlanetaryView;

        Keystone.Vehicles.Vehicle mVehicle;
		Proxy3D mVehicleProxy;
        Proxy3D mVehicleProxyAltitudeLine;
		        
        // used for grid line spacing
        private double mScaledZoneDiameter;
        Keystone.Collision.PickResults mPickResult;

        private Model mShipIcon;

        private Dictionary<string, Game01.GameObjects.SensorContact> mContacts =
            new Dictionary<string, Game01.GameObjects.SensorContact>();
        private Game01.GameObjects.SensorContact[] mPreviousContacts;
        Proxy3D mShipProxy; // todo: there maybe multiple targets so maybe this should be an array
        private AppDatabaseHelper.StarRecord[] mRecords;
        private AppDatabaseHelper.WorldRecord[] mWorldRecords;

        NavigationViewState mCurrentState;
        Stack<NavigationViewState> mPreviousStates = new Stack<NavigationViewState>();
        Stack<NavigationViewState> mForwardStates = new Stack<NavigationViewState>();


        ModeledEntity mSelectionOverlay;
        Tools.NavPointPlacer mNavPointPlacer;
        BoundingBox mNavPointPlacementBounds; // TODO: i don't think i need or want navpoints.  just a single waypoint at a time.  we are not flying a CAP mission in a fighter.

        public NavigationHud()
        {
            mFont.Color = Color.Green;


            InitializePickParameters();

            mGalaxyScale.x = 1 / (AppMain.REGION_DIAMETER * AppMain.REGIONS_ACROSS);
            mGalaxyScale.y = 1 / (AppMain.REGION_DIAMETER * AppMain.REGIONS_HIGH);
            mGalaxyScale.z = 1 / (AppMain.REGION_DIAMETER * AppMain.REGIONS_DEEP);

            // todo: i need to prevent x and z rotation in the viewpoint behavior tree
            // todo: also i think it should use orthographic projection.
            // todo: if there is only 1 zone, we should go straight to singlesystem view


            mInvGalaxyScale.x = AppMain.REGION_DIAMETER * AppMain.REGIONS_ACROSS;
            mInvGalaxyScale.y = AppMain.REGION_DIAMETER * AppMain.REGIONS_HIGH;
            mInvGalaxyScale.z = AppMain.REGION_DIAMETER * AppMain.REGIONS_DEEP;

            // scale a single zone to galactic scaling
            mScaledZoneDiameter = AppMain.REGION_DIAMETER * mGalaxyScale.x;

            mSingleSystemScale = new Vector3d(1 / (AppMain.REGION_DIAMETER * SINGLE_SYSTEM_SCALE), 1 / (AppMain.REGION_DIAMETER * SINGLE_SYSTEM_SCALE), 1 / (AppMain.REGION_DIAMETER * SINGLE_SYSTEM_SCALE));

            //mInvSingleSystemScale.x = AppMain.REGION_DIAMETER / 10d;
            //mInvSingleSystemScale.y = AppMain.REGION_DIAMETER / 10d;
            //mInvSingleSystemScale.z = AppMain.REGION_DIAMETER / 10d;

            mSatelliteSystemScale = new Vector3d(1 / (AppMain.REGION_DIAMETER * SATELLITE_SYSTEM_SCALE), 1 / (AppMain.REGION_DIAMETER * SATELLITE_SYSTEM_SCALE), 1 / (AppMain.REGION_DIAMETER * SATELLITE_SYSTEM_SCALE));
            //mSatelliteSystemScale = new Vector3d(SATELLITE_SYSTEM_SCALE, SATELLITE_SYSTEM_SCALE, SATELLITE_SYSTEM_SCALE);
        }

        ~NavigationHud()
        {
        	RemoveHUDEntity_Retained (mSelectionOverlay);
        	if (mCurrentView != null)
	        	RemoveHUDEntity_Retained (mCurrentView);
    
        	Repository.DecrementRef (mSelectionOverlay);

            RemoveWaypointTabs();
        }
        

        private void InitializePickParameters()
        {

            mPickParameters = new PickParameters[1];

            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.None; 
                //EntityFlags.AllEntityTypes & ~EntityFlags.HUD;

            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
            	KeyCommon.Flags.EntityAttributes.Region;

            PickParameters pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Face,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[0] = pickParams;
        }
        
        // D:\My Pictures\_KGB_PICS\OuterWilds_NavScreen.png 
        // http://www.infinitespacegames.com/  <-- there nav screen is something to check out
        // http://en.wikipedia.org/wiki/NATO_Military_Symbols_for_Land_Based_Systems
        // http://en.wikipedia.org/wiki/Brevity_code   <-- radio chatter from fighters should sound like this
        //                                            <-- user should have to learn the brevity code to follow


       
        
        public override VisibleItem? Iconize(VisibleItem item, double iconScale)
        {
			if (item.Entity is Keystone.Vehicles.Vehicle)
            {
                // NOTE: This is not the same as sensor contact hud icon.
                //       Therefore, I believe this should not be used unless in EditorMode
                //       and we want to more easily find ships.
                //     
                //       A Vehicle that is in the user's scenegraph and fall beyond visual range
                //       ... does the server instruct the game to remove this vehicle?  
                //       eg when the vehicle falls beyond visual range, remove it and then
                //       only use a sensor contact icon IF sensor contact is made?
                //       Maybe the server can notify the clients that they should detach it but
                //       perhaps cache it so it can be quickly re added if needed?
                //       Or do we add a really large buffer of distance where once its no longer visible
                //       updates wont be sent, so it's as good as removed from the scene if it's no longer
                //       moving and no longer being simulated and no longer animating, etc.
                //       Similar to how a physics engine can freeze an object.
                // 
                // TODO: does this item already exist as a previous visible item?
                //       i think we should use a Keystone\Resource\ObjectPool
                // here where elements can time out.  well... hmm
                // Or do we tag Entity's and then we can timeout the VisibleItem
                // if that Entity does not utiltize it's iconified version in x interval


                // NOTE: I think Keystone\Resource\ObjectPool should be moved to 
                // KeyMaths\DataStructures\ObjectPool
                //if (mShipProxy == null)
                //{
                //	// TODO: where am I adding the model and TexturedQuad Geometry?
                //    string id = Repository.GetNewName(typeof(Proxy3D));
                //    mShipProxy = new Proxy3D(id, item.Entity);
                //    mShipProxy.SetEntityAttributesValue ((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                //    mShipProxy.AddChild(mShipIcon);
                //    mShipProxy.UseFixedScreenSpaceSize = true;
                //    mShipProxy.ScreenSpaceSize = 0.05f;

                //    // TODO: Iconize is called on all VisibleItemFound right so this should be AddHUDEntity_Immediate right?
                //    //AddHUDEntity_Immediate(mHud3DRoot, mShipProxy, false);
                //}
                //return new VisibleItem(mShipProxy, mShipIcon, item.CameraSpacePosition);
            }
            else
            { }

            return null;
        }


		public override void Render(RenderingContext context, List<RegionPVS> regionPVSList)
		{
           
            base.Render(context, regionPVSList);


            if (mVehicleProxy != null)
            {
                // TODO: only render the vehicle if it's in StarMap mode or the current System if in SingleSystem Map mode
                // update proxy position.  Instead of a vehicle icon, perhaps some other indicator that there is a starship in that star system?
                if (mMode == MapMode.Galaxy)
                {
                    mVehicleProxy.Translation = mVehicle.GlobalTranslation * mGalaxyScale;
                    Vector3d yawPitchRoll = mVehicle.Rotation.GetEulerAngles(false);
                    double angleRadians = yawPitchRoll.y - 1.5708;  // billboard graphic needs to be rotated 90 degrees // 180 * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS;
                    mVehicleProxy.Rotation = new Quaternion(0, 0, angleRadians); // new Quaternion(Vector3d.Up(), angleRadians);
                }
                else if (mMode == MapMode.SingleSystem)
                {
                    int index = GetDistanceMapIndex(mVehicle.Translation.Length);
                    mVehicleProxy.Translation = mVehicle.Parent.Translation * mSingleSystemScale + (Vector3d.Normalize(mVehicle.Translation) * mWorldSpacing * (index + 1));

                    Vector3d yawPitchRoll = mVehicle.Rotation.GetEulerAngles(false);
                    double angleRadians = yawPitchRoll.y - 1.5708;  // billboard graphic needs to be rotated 90 degrees // 180 * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS;
                    mVehicleProxy.Rotation = new Quaternion(0, 0, angleRadians); // new Quaternion(Vector3d.Up(), angleRadians);
                }
                else if (mMode == MapMode.PlanetaryMap)
                { }

                // update the altitude line // this is obsolete now i think since we're just doing 2-D galaxies and every celestial body and vehicle sits on same plane
                if (GENERATE_ALTITUDE_LINES)
                {
                    Mesh3d mesh = (Mesh3d)mVehicleProxyAltitudeLine.Model.Geometry;
                    Vector3d start = mVehicleProxy.Translation;
                    Vector3d end = start;
                    end.y = 0;
                    mesh.SetVertex(0, start);
                    mesh.SetVertex(1, end);
                }
            }


			if (mRecords != null)
			{
				// render star labels only when StarMap NOT SystemMap is visible		
				if (Mode == MapMode.Galaxy)
				{		
					for (int i = 0; i < mRecords.Length; i++)
					{
						// TODO:  - look at how projects like Celestia handle star label overlapping.
						Vector3d cameraSpacePosition = (mRecords[i].GlobalTranslation * mGalaxyScale) - context.Viewpoint.Translation;
						string label =  mRecords[i].Name;
						this.CreateLabels (cameraSpacePosition, label);
					}
				}
				else if (Mode == MapMode.SingleSystem)
				{

                    // TODO: get the habitability zone parameters for this star system
                    //       scale them and assign to shader

                    // draw AU (astronomical unit) markers
                    // TODO: is the star at 0,0,0 or is it in scaled world coordinates?
                    if (DRAW_AU_MARKERS)
                    {
                        GenerateAUDistanceMarkers(context);
                        if (mDistanceDisk != null)
                            // center the AU distance map on the star's position
                            mDistanceDisk.Translation = mSingleSystemView.Translation;
                    }
                    // TODO: verify we can restore and resume Simulation from last
                    //       positions along orbit... in other words, we don't start at
                    //       0 elapsed epoch each time we try to resume a campaign.
                    // update positions of world proxies
                    if (mSingleSystemView != null && mSingleSystemView.Children != null && mSingleSystemView.ChildCount > 0)
					{
						Proxy3D starProxy =(Proxy3D)mSingleSystemView;
						if (starProxy.Children != null)
						{
                            //int worldCount = GetWorldCount(starProxy.Children);

							for (int i = 0; i < starProxy.Children.Length; i++)
							{	
                                // NOTE: we don't need to update the worldProxy.Translation after initial is set unless we are actually animating the orbits of the planets which for version 1.0 we canceled.
								Proxy3D worldProxy = starProxy.Children[i] as Proxy3D;
								if (worldProxy != null)
								{
                                    // TODO: based on number of Worlds and the view width, compute evenly spaced translations for each world from inner to outer.
                                    //       This is fine since we're intentionally not going to scale and we want all planets in the System to be visible and pickable.
									// worlds only, not orbit lines
									//if (!worldProxy.ID.Contains("#ORBIT"))
									//	worldProxy.Translation = worldProxy.ReferencedEntity.Translation * (mSingleSystemScale * mWorldScaleModifier);
								}
							}
						}
					}
				}
			}
		}


        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateBeforeCull(context, elapsedSeconds);
            Keystone.EditTools.Tool currentTool = context.Workspace.CurrentTool;

            if  (mVehicleProxy == null)
            {
                // TODO: this match should be user's vehicle and perhaps other friendly vehicles
                //       and non allied vehicles in sensor range
                // TODO: i'm not updating the proxy's position as the ship translates through the galaxy
                //       - i could cache the found vehicle and then use that.. and REMEMBER the translation
                //       needs to be scaled by the galaxyScale of our navmap
	            Predicate<Node> match = e => e is Keystone.Vehicles.Vehicle;
	            
	            Node[] v = this.Context.Scene.Root.Query (true, match);
	            if (v != null)
	            {
	            	mVehicle = (Keystone.Vehicles.Vehicle)v[0];
	            	string id = Repository.GetNewName (typeof (Keystone.Vehicles.Vehicle));
	            	mVehicleProxy = new Proxy3D (id, mVehicle);
	            	mVehicleProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                    mVehicleProxy.ScreenSpaceSize = VEHICLE_SCREENSPACE_SIZE;
                    mVehicleProxy.UseFixedScreenSpaceSize = true;
                    mVehicleProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Overlay, true); // vehicle icons render on top of everything else
                    
                    string texturePath = System.IO.Path.Combine (AppMain._core.DataPath, @"editor\hud_uknown_vessel_type.png");
                    // NOROTATION == custom rotation which in this case is to give us Y axis rotation like our stars 	
                    mShipIcon = Helper.Load3DBillboardIcon(texturePath, 1f, 1f, MTV3D65.CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION);
                    	            
	            	mVehicleProxy.AddChild (mShipIcon);

                    Keystone.IO.PagerBase.LoadTVResource(mShipIcon);

                    
                    if (GENERATE_ALTITUDE_LINES)
                    {
                        mVehicleProxyAltitudeLine = GenerateAltitudeLine(mVehicle.ID, mVehicleProxy.Translation);
                        AddHudEntity_Retained(mVehicleProxyAltitudeLine);
                    }
                  	AddHudEntity_Retained (mVehicleProxy);

                    // TODO: select the vehicle so that the HTML panel is generated and shows the list of waypoints
                    this.mPickResult = new Keystone.Collision.PickResults();
                    this.mPickResult.SetEntity (mVehicle);
                    WorkspaceBase3D ws = (WorkspaceBase3D)this.Context.Workspace;
                    // TODO: this requires an invoke to GUI thread
                    // Perhaps for now we skip this and work on getting the planetary view going
                    // and then snapping waypoints to stars, worlds and moons
                    // ws.QuickLookPanelSelect((PluginHost.EditorHost)AppMain.PluginService.SelectPlugin ("Editor", mVehicle.TypeName).Instance.Host, mVehicle.ID, mVehicle.TypeName, this.mPickResult);

                    mWaypointTabsChanged = true;
                }
            }
            else
            {
            	// update the Vehicle's altitude line
            	// We need to just SetVertex() to move it the start0 and end1 points.
            	
            }

            // this navigation hud is able to define and retreive methods in a 
            // entity script that only it knows about and which are NOT required 
            // methods of Keystone.dll.   These are .exe specific.

            // TODO: game wise, we also want our vehicle perhaps to be able to cache
            //       contact histories.  I think Contact and Waypoint management can be
            //       performed here even though only the actual array of contacts and waypoints
            //       need be stored in the private vars of a Vehicle.
            // TODO: so for now, lets consider we call a script in the domain object
            //       that is to create proxies or renderables or get the data
            //       necessary to do that.  how?  Seems to create just the proxy
            //       i only need camera space position.  but ideally id also want
            //       IFF information so we know what color to render.
            // TODO: the downside of calling a special function is it presumes that we 
            //       know this is a special case.  Ideally it would not be and neither would
            //       the return.  Ideally, then, it'd be a generic scenario where we want our scripts
            //       as we did with hex flares, to create the renderables. 
            //       Then it becomes a matter of picking.
            //          - BUT also, we only want to render for this one, not every single entity.
            //          so how do we keep generic yet only used for the currently selected vehicle?
            //       context.Target.DomainObject.Execute();
            if (mVehicle != null)
            {
                GenerateWaypointTabs();

                if (mLastMode != mMode || mLastVehicle != mVehicle)
                {
                    mLastVehicle = mVehicle;
                    mLastMode = mMode;
                }
                 
                Tools.NavPointPlacer tool = currentTool as Tools.NavPointPlacer;
                if (tool != null)
                {
                    // TODO: this scaling doesn't change with current MapMode
                    
                    if (mMode == MapMode.Galaxy)
                    {
                        tool.mRegionDimensions = new Vector3d(AppMain.REGION_DIAMETER, AppMain.REGION_DIAMETER, AppMain.REGION_DIAMETER);
                        tool.Bounds = context.Scene.Root.BoundingBox;
                    }
                    else
                    {
                        Keystone.Portals.ZoneRoot root = (Keystone.Portals.ZoneRoot)context.Scene.Root;
                        BoundingBox bounds = root.GetChildZoneSize();
                        bounds.Translate(-mStarTranslation);
                        tool.Bounds = bounds;
                    }
                }


                // What about future proofing where database writes should be managed by server (should they?)?
                // For now, let's monitor changes to the property in script.  We'll use the DatabaseAPI to write
                // changes to the DB.


                // so the trick is particularly with contacts, how to keep the gui
                // updated without duplicating and how to know when a new contact has been
                // added or a previous deleted.

                // do we track previousContacts and currentContacts?
                // we could do that here in the Hud.



                //mPreviousWaypoints = waypoints;

                Game01.GameObjects.SensorContact[] contacts = null;
                object tmp = null; // vehicle.GetCustomPropertyValue("contacts");
                if (tmp != null)
                {
                    contacts = (Game01.GameObjects.SensorContact[])tmp;
                    if (contacts.Length > 0)
                    {
                        // free world tides of war: tactical overview
                        // http://www.youtube.com/watch?v=_QfsYIvJ8pY
                        // TODO: now here we need to manage contacts
                        //     Merge(context.Scene.Root, contacts);
                        //System.Diagnostics.Trace.WriteLine(string.Format("{0} contacts found.", contacts.Length));
                        //for (int i = 0; i < contacts.Length; i++)
                        //    System.Diagnostics.Trace.WriteLine("SensorContact " + contacts[i].EntityID);
                    }

                }




                // TODO: null contacts should not necessarily remove all previous contacts.  
                //       instead we only want to merge contacts really.  Here we deal with
                //       stale contacts and the contact histories.  So we should have a 
                //       ContactManager and then ContactManager.Merge (contacts);
                //       followed by an update of the contact manager where it will 
                //       prune the list of stale items.
                //      also settings for the contact manager for how much history to show

                // our contact manager will manage which contacts are already added to scene
                mPreviousContacts = contacts;
            }
            // foreach new contact 
            // AddHudEntityToScene();
            // 

            // now we can cache these contacts until "Render" occurs and then we can inject them
            // as VisibleItems

            // by caching them here, i think maybe we can pick them without adding them to scene.
            // inserting into scene is not desireable anyway.  
            // Inserting them maybe is just fine, we treat them as hud elements under the umbrella
            // root of the HUD so they never get inserted into octree.  There's no need because
            // well... we'll see.

            // TODO: damage states of radar and destruction animation, etc...
            // TODO: animation of our radar is something we havent done yet
            // TODO: radar dish / transmitter vs radar scope/display and radar targeting computers.
            //       so in terms of the geometry models when placing radar, I think the dish represents
            //       an interior hardpoint to connect a data line to a station or computer.
        }
        
		public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
		{
			base.UpdateAfterCull(context, elapsedSeconds);
		
            if (this.Grid.Enable && context.Viewpoint != null && context.Viewpoint.Region != null)
            {
                if (mCurrentView != null)
                {
                    Vector3d position;
                    position.x = position.y = position.z = 0;

                    if (mCurrentView.Children != null)
                    {
                        if (mCurrentView is Keystone.Celestial.Star)
                            // if in SingleSystemMap, center grid on Star Proxy which is mCurrentMap)
                            position = mCurrentView.Translation;
                    }
                    // TODO: I dont think our outer gridlines are scaled to match Zone diameters
                    Vector3d offset = -context.GetRegionRelativeCameraPosition(context.Viewpoint.Region.ID) + position;
                    Keystone.Immediate_2D.Renderable3DLines[] gridLines =
                        Grid.Update(offset, (bool)context.GetCustomOptionValue(null, "show axis indicator"));

                    AddHUDEntity_Immediate(context.Viewpoint.Region, gridLines);

                    if (mVehicle != null)
                    {
                        object tmp = mVehicle.GetCustomPropertyValue("navpoints");
                        if (tmp != null)
                        {
                            Game01.GameObjects.NavPoint[] waypoints = (Game01.GameObjects.NavPoint[])tmp;
                            if (waypoints != null && waypoints.Length > 0)
                            {
                                Keystone.Immediate_2D.Renderable3DLines waypointLines = GenerateWaypointLines(mVehicle.Region.ID, mVehicle.Translation, waypoints, context.Position);
                                AddHUDEntity_Immediate(context.Viewpoint.Region, waypointLines);
                            }
                        }
                    }
                }
            }
		}
        
        // can the histories be used to generate an arbitrary length tail?
        // and what if we want to select a specific contact and load all
        // history data so we can figure out what that ship is up to?
        public void Merge(Entity parent,  Game01.GameObjects.SensorContact[] contacts)
        {
            if (contacts == null || contacts.Length == 0) return;

            for (int i = 0; i < contacts.Length; i++)
            {
                // todo: sensor contact perhaps not visible in Nav but in "tactical" view? hrm
                Game01.GameObjects.SensorContact c;
                if (mContacts.TryGetValue(contacts[i].ContactID, out c))
                {
                    // 
                    //c.AddPlot();
                    
                }
                else
                {
                    // TODO: we could associate a proxy model right here and then
                    // add it as tag to contact?

                    mContacts.Add(contacts[i].ContactID, contacts[i]);
                    string path = System.IO.Path.Combine (AppMain.DATA_PATH , @"editor\hud_uknown_vessel_type.png");
                    ModeledEntity proxy = Helper.Create3DProxyBillboard(this.Context.Viewport.TVIndex, contacts[i].ContactID, path);
                    AddHUDEntity_Immediate (parent, proxy, false);
                }

                Helper.SetTranslation(contacts[i].ContactID, contacts[i].Position);

            }
        }
        
        internal MapMode Mode 
        {
        	get {return mMode;}
        	set {mMode = value;}
        }
        

        public override void OnEntityClicked(Keystone.Collision.PickResults pickResult)
        {
        	mPickResult = pickResult;
			Debug.WriteLine ("NavigationHud.OnEntiyCLicked() - ***************  SINGLE Click");
            if (pickResult.Entity == null) return;

            if (mPickResult.Entity is Proxy2D)
            {

            }
            else if (mPickResult.Entity is Proxy3D) // can be 3D billboard such as our Vehicle icons
            {
                System.Diagnostics.Debug.WriteLine("NavigationHud.OnEntityClicked()");
            }

        	switch (mMode)
            {
            	case MapMode.Galaxy:
        		{
        			// single click position selection icon over this star or world
        			if (mSelectionOverlay == null)
        			{
                        GenerateSelectionOverlay();
        			}
        			
        			// TODO: why when switching from some SystemMap back to StarMap do we
        			//       start mousepicking Worlds? The Worlds in the SystemMap should be unloaded when we change views
        			if (mPickResult.FaceID >= mRecords.Length) return;
                    if (mPickResult.FaceID < 0) return;

        			Vector3d translation = mRecords[mPickResult.FaceID].GlobalTranslation * mGalaxyScale;
        			mSelectionOverlay.Translation = translation;
        			break;
        		}
            	case MapMode.SingleSystem:
            		// did we click the star or a planet?
            		
            		//// each pick result represents a Proxy3D
            		//Proxy3D p = (Proxy3D)pickResult.Entity;
            		//// is the referenced entity loaded
            		//if (p.ReferencedEntity != null)
            		//{
            			
            		//}
            		break;
                case MapMode.PlanetaryMap:
                    break;
            	default:
            		break;
            }
        }
        
        public override void OnEntityDoubleClicked (Keystone.Collision.PickResults pickResult)
        {
            mPickResult = pickResult;
            System.Diagnostics.Debug.WriteLine ("NavigationHud.OnEntiyDoubleCLicked() - ***************  Double Click");

            // TODO: how do i get access to the Workspace.CurrentTool from here.  I could cache it from Render()
            //       but that is a little hackish.  But maybe that's the only good way.  For Single System
            //       we don't need to compute the bounds, we know that all Zone bounds are the same. Except
            //       currently i'm not resetting origin to 0,0,0. I'm using galactic star positions.
            // ((Tools.NavPointPlacer)context.Workspace.CurrentTool).Bounds = context.Scene.Root.BoundingBox;
            switch (mMode)
            {
            	case MapMode.Galaxy:
        		{
                    if (pickResult.FaceID >= mRecords.Length) return;
                    if (pickResult.FaceID < 0) return;
                    // if double clicking vehicle proxy billboard or waypoint tab billboard
                    // FaceID in pickResult will be 1.  So we must filter these out.
                    if (pickResult.Entity is Keystone.Vehicles.Vehicle) return;
                    if (!string.IsNullOrEmpty (pickResult.Entity.Name) && pickResult.Entity.Name.Contains("WAYPOINT")) return;

                    string starID = pickResult.EntityID;
                    int starIndex = pickResult.FaceID;
                    starID = mRecords[starIndex].ID;

                    mCurrentRegionID = mRecords[starIndex].RegionID; 
                    Navigate_To (starID, "Star");
                    // TODO: when we click back arrow button, we need to update the boundingbox
                    //mNavPointPlacer.Bounds = pickResult.Context.Scene.Root.RegionDiameterX; 
                    break;
        		}
            	case MapMode.SingleSystem:
            		if (pickResult.Entity == null) return;

                    //// each pick result represents a World Proxy3D or the parent Star
                    //Proxy3D p = (Proxy3D)pickResult.Entity;
                    //// is the referenced entity loaded
                    //if (p.ReferencedEntity != null)
                    //{

                    //}
                    // TODO: if the picked entity is a proxy, the region.ID will be wrong?
                    mCurrentRegionID = pickResult.Entity.Region.ID;

                    Navigate_To(pickResult.Entity.ID, "World");
            		break;
                case MapMode.PlanetaryMap:
                    break;
            	default:
            		break;
            }
        }

        private void On_MapModeChanged()
        {
            RemoveWaypointTabs();

            GenerateWaypointTabs();
        }

        public void Navigate_Forward()
        {
            if (mForwardStates.Count == 0) return;
            // if there is a forward state, read it's navigation type
            // and the ID and pass that to the appropriate function

            // pop from forward state and then Navigate_To will make it current state on mPreviousStates stack
            NavigationViewState current = mForwardStates.Pop();

            // now we must navigate to the new current state
            Navigate_To(current.ID, current.TypeName);

            Debug.WriteLine("Navigate_Forward");
        }

        public void Navigate_Up()
        { 
        }

        public void Navigate_Previous()
        {
            // we can only go to previous if there are at least 2 states on the stack
            if (mPreviousStates.Count <= 1) return;

            mForwardStates.Push (mPreviousStates.Pop());

            // now the current top state on previous stack is the current
            // NOTE: We .Pop() again because Navigate_To() will push a new value onto stack
            NavigationViewState current = mPreviousStates.Pop();

            // now we must navigate to the new current state
            Navigate_To(current.ID, current.TypeName);

            Debug.WriteLine("Navigate_Previous");
        }
        
        internal void Navigate_To ()
        {
			Debug.Assert (mRecords != null, "NavigationHud.Navigate_To()");
            Navigate_To(null, "ZoneRoot"); // now that the scene is assigned, we can generate and load starmap
        }
        

        public void Navigate_To(string id, string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException();

            // TODO: should these return states we can push onto stack?
            switch (typeName)
            {
                case "ZoneRoot":

					if (!(Context.Scene.Root is Keystone.Portals.ZoneRoot))
						return;

        			if (mGalacticView == null)
        			{	        			
        				GenerateGalaxyView();
        			}
        			
                    // are we already looking at the mGalaxyMap as the current view?
                    if (mCurrentView != null && mCurrentView == mGalacticView)
                        return;

                    if (mCurrentView != null)
                        RemoveHUDEntity_Retained(mCurrentView);

                    mCurrentView = mGalacticView;

					AddHudEntity_Retained (mCurrentView);
                    //AddHudEntity_Retained (mSelectionOverlay);
                    
                    ConfigureGalaxyMapView();
                    if (mDistanceDisk != null)
                    {
                        RemoveHUDEntity_Retained(mDistanceDisk);
                    }
                    break;

                case "World":

                    GeneratePlanetaryView(id);

                    // are we already looking at this single system view for the current view?
                    // TODO: but we want our focus entity id to change in the ConfigureSystemView() call
                    // so we cannot return here we can only skip unload, RemoveHUDEntity_Retained and AddHudEntity_Retained!
                    if (mCurrentView != null && mCurrentView == mPlanetaryView)
                        return;

                    // "focus_entity_id" must be re-assigned from Behavior before removing the mGalaxyMap
                    Context.Viewpoint.BlackboardData.SetString("focus_entity_id", null);

                    if (mCurrentView != null)
                    {
                        RemoveHUDEntity_Retained(mCurrentView);
                        //mCurrentMap.Visible = false;
                    }
                    if (mSelectionOverlay != null)
                        RemoveHUDEntity_Retained(mSelectionOverlay);

                    mCurrentView = mPlanetaryView;

                    AddHudEntity_Retained(mCurrentView);
                    ConfigureSatelliteView(MapMode.PlanetaryMap);
                    
                    break;
                case "Star":
                    // TODO: if instead of loading the star system from XML and all it's worlds and moons
                    // i just grab from the sqlitedb, then I don't get the proper positions along the orbit
                    // since i use Animation.Play(0) to do that.  I would need to duplicate that data in the
                    // db so that i could compute the correct current position.  But maybe that is faster.
                    // TODO: it appears i'm using the star's actual global translation when positioing
                    // the star in the Single System.  I thought I was placing it at origin.  Double check this.
                    // For very large galaxies do i lose precision this way at such small scales?   And also
                    // is my waypoint position calculations off because of the incorrect assumption that the star was at origin
                    // in Zone coordinates.  Also, if at origin in Zone coordinates, how would that affect implementing 
                    // a camera transition from galaxy->star system->planetary system.  And what about when we
                    // do want to generate HTML when clicking on a star or world?  Even though it's the nav screen
                    // maybe we do want to be able to click a world and render the data to an HTML panel? 
                    // (eg maybe we have two html panels - one for waypoints and one that represents a knowledge base 
                    // for stars/worlds/life forms, etc)                                
                    GenerateSingleSystemView(id, typeName);

                    if (mDistanceDisk == null)
                    {
                        GenerateAUDistanceDisk();
                        AddHudEntity_Retained(mDistanceDisk);
                    }

                    // are we already looking at this single system view for the current view?
                    // TODO: but we want our focus entity id to change in the ConfigureSystemView() call
                    // so we cannot return here we can only skip unload, RemoveHUDEntity_Retained and AddHudEntity_Retained!
                    if (mCurrentView != null && mCurrentView == mSingleSystemView)
                        return;

                    // "focus_entity_id" must be re-assigned from Behavior before removing the mGalaxyMap
                    Context.Viewpoint.BlackboardData.SetString ("focus_entity_id", null);
                    
                    if (mCurrentView != null)
                    {
                        RemoveHUDEntity_Retained(mCurrentView);
                        //mCurrentMap.Visible = false;
                    }
                    if (mSelectionOverlay != null)
                        RemoveHUDEntity_Retained(mSelectionOverlay);

                    mCurrentView = mSingleSystemView;

                    AddHudEntity_Retained(mCurrentView);
                    ConfigureSingleSystemView(MapMode.SingleSystem);
                    break;
            }

            NavigationViewState newState = new NavigationViewState ();
            newState.ID = id;
            newState.TypeName = typeName;

            // set as current state
            mPreviousStates.Push (newState);
        }

        void GenerateAUDistanceMarkers(RenderingContext context)
        {
            if (mSingleSystemView == null || mSingleSystemView.Region == null) return;

            Vector3d cameraSpacePosition = context.Position;
            double distanceAU = Keystone.Celestial.Temp.AU_TO_METERS;
            double scaledDistanceAU = distanceAU * mSingleSystemScale.x;
            cameraSpacePosition = new Vector3d(scaledDistanceAU, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            Vector3d position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());

            // 1 AU
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            Keystone.Immediate_2D.IRenderable2DItem textAU = new Keystone.Immediate_2D.Renderable2DText("1AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);

            // 5 AU
            cameraSpacePosition = new Vector3d(scaledDistanceAU * 5, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            textAU = new Keystone.Immediate_2D.Renderable2DText("5AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);

            // 10 AU
            cameraSpacePosition = new Vector3d(scaledDistanceAU * 10, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            textAU = new Keystone.Immediate_2D.Renderable2DText("10AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);

            // 20 AU
            cameraSpacePosition = new Vector3d(scaledDistanceAU * 20, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            textAU = new Keystone.Immediate_2D.Renderable2DText("20AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);

            // 50 AU
            cameraSpacePosition = new Vector3d(scaledDistanceAU * 50, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            textAU = new Keystone.Immediate_2D.Renderable2DText("50AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);

            // 100 AU
            cameraSpacePosition = new Vector3d(scaledDistanceAU * 100, 0, 0) + mSingleSystemView.Translation;
            cameraSpacePosition -= context.Position;
            position = this.Context.Viewport.Project(cameraSpacePosition, Context.Camera.View, Context.Camera.Projection, Matrix.Identity());
            textAU = new Keystone.Immediate_2D.Renderable2DText("100AU", (int)position.x, (int)position.y, Color.Yellow.ToInt32(), Context.TextureFontID);
            AddHUDEntity_Immediate(mSingleSystemView.Region, textAU);
        }

        void GenerateSelectionOverlay()
        {
            string path = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\navmap_selection.png");

            Model model = Helper.Load3DBillboardIcon(path, 1.3f, 1.3f);
            string id = Repository.GetNewName(typeof(ModeledEntity));
            mSelectionOverlay = new ModeledEntity(id);
            mSelectionOverlay.Name = null; // we don't want label rendering
            mSelectionOverlay.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            mSelectionOverlay.Serializable = false;
            mSelectionOverlay.UseLargeFrustum = false;
            mSelectionOverlay.Pickable = false;
            mSelectionOverlay.Dynamic = false;
            mSelectionOverlay.AddChild(model);
            //mSelectionOverlay.UseFixedScreenSpaceSize = true;
            //mSelectionOverlay.ScreenSpaceSize = 0.05f;

            // TODO: incrementRef/decrementRef the overlay
            Repository.IncrementRef(mSelectionOverlay);
        }

        ModeledEntity mDistanceDisk;
        void GenerateAUDistanceDisk()
        {
            if (Mode == MapMode.SingleSystem)
            {
                string shaderPath = @"caesar\shaders\planet\habital_zone_disk.fx";
                // shaderPath = "tvdefault";
                string path = @"editor\disk.obj";
                string diffuseTexturePath = @"caesar\shaders\planet\habital_zone_disk.png";

                string id = Repository.GetNewName(typeof(ModeledEntity));
                mDistanceDisk = new ModeledEntity(id);
                mDistanceDisk.Name = null; // we don't want label rendering
                mDistanceDisk.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                mDistanceDisk.Serializable = false;
                mDistanceDisk.UseLargeFrustum = false;
                mDistanceDisk.Pickable = false;
                mDistanceDisk.Dynamic = false;
                //mDistanceDisk.UseFixedScreenSpaceSize = true;
                //mDistanceDisk.ScreenSpaceSize = 0.05f;


                Mesh3d diskMesh = Mesh3d.CreateDisk(1, 1, 100); // (Mesh3d)Repository.Create(System.IO.Path.Combine(AppMain._core.DataPath, path), "Mesh3d");
                diskMesh.CullMode = (int)MTV3D65.CONST_TV_CULLING.TV_DOUBLESIDED;


                Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, shaderPath, diffuseTexturePath, null, null, null, true);
                appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
                appearance.Material.Emissive = Color.Red;
                appearance.Material.Opacity = 0.15f;


                id = Repository.GetNewName(typeof(Model));
                Model diskModel = (Model)Repository.Create(id, "Model");
                double scale = 2.5; // TODO: this scale should be the bounds of the solar system / singleSystemScaling
                scale = AppMain.REGION_DIAMETER * mSingleSystemScale.x;
                diskModel.Scale = new Vector3d(scale, scale, scale);

                diskModel.AddChild(appearance);
                diskModel.AddChild(diskMesh);

                mDistanceDisk.AddChild(diskModel);


                // TODO: incrementRef/decrementRef the mDistanceDisk
                Repository.IncrementRef(mDistanceDisk);
            }
        }

        void GenerateGalaxyView()
        {
        	
        	mRecords = AppDatabaseHelper.GetStarRecords();

        	if (mRecords == null || mRecords.Length == 0) 
        	{
        		System.Diagnostics.Debug.WriteLine ("NavigationHud.GenerateStarMap() - ERROR: Record Count = 0");
        		return;
        	}
        	string newID = Repository.GetNewName( typeof(ModeledEntity));
	        	
    		// when switching to zoom, we use a version of the starMap that is NOT Background3D and
    		// so does not follow camera.  Also, we scale the Translations of each star to fit inside the viewport.
        	mGalacticView = new ModeledEntity (newID);
        	mGalacticView.SetEntityAttributesValue ((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
        	// do not treat this galaxy map as a background or it will get culled since
        	// we are rendering HUD items only on this NavMap
        	mGalacticView.SetEntityAttributesValue ((uint)KeyCommon.Flags.EntityAttributes.Background , false);
        	mGalacticView.Serializable = false;
        	mGalacticView.UseLargeFrustum = false;
        	mGalacticView.Pickable = true;
        	mGalacticView.Dynamic = false;
        	mGalacticView.Name = null; // don't want the label rendering

            // TODO: March.11.2017 - We're getting a CTD with no exception thrown when trying to generate the altitude lines for medium sized galaxy
            
            if (GENERATE_ALTITUDE_LINES)
            {
                for (int i = 0; i < mRecords.Length; i++)
                {
                    try
                    {
                        Proxy3D altitudeLine = GenerateAltitudeLine(mRecords[i].ID, mRecords[i].GlobalTranslation * mGalaxyScale);
                        altitudeLine.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                        mGalacticView.AddChild(altitudeLine);
                        System.Diagnostics.Debug.WriteLine("NavigationHUD.GenerateStarMap() - altitude line = " + i.ToString());
                        if (i == 106)
                        {
                            System.Diagnostics.Debug.WriteLine("NavigationHUD.GenerateStarMap() - max?");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("NavigationHud.GenerateStarMap() - ERROR: " + ex.Message);
                    }

                }
            }
        	     	
        	// create a sphere mesh used to represent each star
        	Mesh3d sphereMesh = Mesh3d.CreateSphere (STAR_MESH_RADIUS, 32, 32, true);
        		        	
        	// create a minimesh
        	MinimeshGeometry mini = (MinimeshGeometry)Repository.Create ("MinimeshGeometry");
        	mini.SetProperty ("meshortexresource", typeof(string), sphereMesh.ID);
        	mini.SetProperty ("precache", typeof (bool), true);
        	mini.MaxInstancesCount =(uint)mRecords.Length;
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance (MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", null, null, null, null, true);
            appearance.Material.Emissive = Color.Yellow;

            string id = Repository.GetNewName( typeof(Model));
            Model galaxyModel = (Model)Repository.Create (id, "Model");
            galaxyModel.AddChild (appearance);
            galaxyModel.AddChild (mini);
            
            mGalacticView.AddChild (galaxyModel);
            
            
            // increment the ref count of the starmap so that we can
            // navigate to/from it after adding/removing from HUD Retained Root node.
            Repository.IncrementRef (mGalacticView);
            
            Keystone.IO.PagerBase.LoadTVResource (galaxyModel, true);

            try 
        	{
            	// TODO: should we use something like starModel.AddInstance?	
            	// iterate through each record and scale the minimesh elements' positions
	        	for (int i = 0; i < mRecords.Length; i++)
	        	{
	        		// NOTE: I assign entity.GlobalTranslation to mRecords[i].Translation also since at least for a StarDigest
	        		//        we're only working with global coords. (unlike say a digest of Factories\Mines\Creatures\Planets\Citizens\etc on a world)
	        		Vector3d position = mRecords[i].GlobalTranslation * mGalaxyScale;
                    Vector3d test = position * mInvGalaxyScale;

	        		mini.ChangeElement  ((uint)i, (float)position.x, (float)position.y, (float)position.z, 1, 1, 1, 0, 0, 0, 0, i);
	                mini.SetElementEnable ((uint)i, 1);
	        	}
        	}
        	catch (Exception ex)
        	{
        		Debug.WriteLine ("NavWorkspace.Navigate_To() - ERROR: " + ex.Message);
        	}
        }
        
        void GenerateSingleSystemView(string centralBodyID, string centralBodyTypeName)
        {
        	Debug.WriteLine ("NavWorkspace.GenerateSingleSystemView() - " + centralBodyID);
        	if (mSingleSystemView != null && mSingleSystemView.ID != centralBodyID)
            {
                // TODO: i think the following crashes when trying to 
                // go from one system view, to star digest then to a different system view
                // where the old system view is already removed from scene but is now
                // being removed recursively from repository.... i think in that instance
                // we are trying to remove it's sceneNode which is not set.  My question is
                // why didnt this happen before?  I can only assume its because we were not
                // decrement ref'ing it before?
                // unload it so we can build new one that is of the correct zone
                Repository.DecrementRef(mSingleSystemView);
                mSingleSystemView = null;
            }
         
            // TODO: when navigating back here from Satellite map, the pickresult.FaceID is not valid!
            if (mSingleSystemView == null)
            {

                int index = mPickResult.FaceID;
                Debug.Assert(index >= 0);
                Debug.Assert(index < mRecords.Length);
                

                // loads the root system's view.  All nested systems are treated as part of the overall system
                // view.  In other words, the system although code wise consists of nested systems
                // game wise, it's to be viewed as a single aggregate of stars and worlds.
                //TODO: in this case, rather than pass in a starID, what we really intend is to pass in a 
                // ZoneID and only to use the starID to determine the zoneID
                string id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Entities.Proxy3D));
                //Entity starEntity = (Entity)Repository.Get(centralBodyID);
                // TODO: I don't like tying in a paged in starEntity to this Proxy3D.  Do we need it?  
                //       Surely we can just pass the centralBodyID?
		        mSingleSystemView = new Keystone.Entities.Proxy3D(id, centralBodyID); // starEntity
                //mSystemMap.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.AllEntityAttributes, false);
                mSingleSystemView.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
                mSingleSystemView.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
		     	mSingleSystemView.Serializable = false;
		      	mSingleSystemView.UseLargeFrustum = false;
		       	mSingleSystemView.Pickable = true;
		       	mSingleSystemView.Dynamic = false;
		       	mSingleSystemView.Name = null; // don't want the label rendering


                // note: we tranlsate the scale to match the starmap scaling
                Vector3d translation = mRecords[index].GlobalTranslation * mSingleSystemScale; // mGalaxyScale;
                // TODO: confused here, weren't we centering at origin and then positioning planets relative to that?
                mSingleSystemView.Translation = translation;

                mWorldRecords = AppDatabaseHelper.GetWorldRecords(mRecords[index].ID);

                // prevent it from unpaging whenever we switch between starmap, system, planetary views
                Repository.IncrementRef(mSingleSystemView);

                bool recurse = true;
	            bool cloneEntities = false;
	            // since we are modifying the translation, we CLONE and treat these as proxies
	            // TODO: if we do not clone the entities, then we cannot parent them again if
	            //       they are already parented.  We need to make proxies of them.
                // TODO: can we grab the world data from the db instead? I think we'll be moving
                // progressively away from xmldb, especially for non rendering, spatial (culling/picking) entity data
                // TODO: since we're already loading in and recursing entire star system, I think we should just
                //       get the planet and moon data from that.  And we keep (for now) the planet's coordinate
                //       and we zoom our view with that planet as the focus entity.
	            Context.Scene.XMLDB.Read(centralBodyTypeName, centralBodyID, null, recurse, cloneEntities, null, SystemMapPageCompleteHandler);
            }
        }

        // https://en.wikipedia.org/wiki/Satellite_system_(astronomy)
        private void GeneratePlanetaryView(string planetID)
        {
            // TODO: do we need to remove any existing mPlanetaryMap hiearchy?

            ModeledEntity primary = (ModeledEntity)Repository.Get(planetID);
            string newID = Repository.GetNewName(typeof(ModeledEntity));

            mPlanetaryView = new ModeledEntity(newID);
            mPlanetaryView.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            // do not treat this mPlanetaryMap as a background or it will get culled since
            // we are rendering HUD items only on this NavMap
            mPlanetaryView.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
            mPlanetaryView.Serializable = false;
            mPlanetaryView.UseLargeFrustum = false;
            mPlanetaryView.Pickable = true;
            mPlanetaryView.Dynamic = false;
            mPlanetaryView.Name = null; // don't want the label rendering

            // increment the ref count of the mPlanetaryMap so that we can
            // navigate to/from it after adding/removing from HUD Retained Root node.
            Repository.IncrementRef(mPlanetaryView);


            // sphere mesh added for Geometry
            Mesh3d planetMesh = Mesh3d.CreateSphere(PLANET_MESH_RADIUS, 16, 16, true);
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", null, null, null, null, true);
            appearance.Material.Emissive = Color.Green;

            Model planetModel = (Model)Repository.Create(typeof(Model).Name);
            planetModel.AddChild(appearance);
            planetModel.AddChild(planetMesh);
            Keystone.IO.PagerBase.LoadTVResource(planetModel, true);

            mPlanetaryView.AddChild(planetModel);

            
            mPlanetaryView.Translation = primary.Translation;

            Proxy3D[] moonProxies = GenerateProxyWorlds(primary, mScaledZoneDiameter);
            if (moonProxies != null)
                for (int i = 0; i < moonProxies.Length; i++)
                {
                    mPlanetaryView.AddChild(moonProxies[i]);

                    //double radius = world.OrbitalRadius * mSingleSystemScale.x;

                    //Proxy3D orbitLines = GenerateOrbitLines(world,
                    //										radius,
                    //                                        world.OrbitalEccentricity, 
                    //                                        world.OrbitalInclination, 
                    //                                        world.OrbitalProcession);

                    // purely circular orbitlines that match the worldProxy's linear equal distance world spacing
                    Proxy3D orbitLines = GenerateOrbitLines(moonProxies[i].ReferencedEntity,
                                                            moonProxies[i].Translation.Length,
                                                            0.0f,
                                                            0.0f,
                                                            0.0f);

                    mPlanetaryView.AddChild(orbitLines);
                }

            mDistanceMap = GenerateDistanceMap(primary);


        }


        // while transition is in progress, we must not be allowed to make new inputs
        private void Transition(NavigationViewState from , NavigationViewState to)
        {
        	
        	
            // how can we transition from current view to the next?
            //		- we create an Animation that will contain a sequence of clips to 
            //		both modify scales, modify distances, and camera rotation 
            //      - temporarily disable input controls until the transition is over
            //      - is transition partially an automation of the input controller?
            //
            // especially if scales are different

            // camera positions are different

            // pointsprite meshes are visible and zooming too closely into that is not good
            //  - for this i think it could help by fading that pointsprite vertex out, fading the
            //    real star in, and zooming in and making sure the fake pointsprite is completely
            //    faded out along with all other stars (so we can actually fade out the entire
            //    mesh!) by the time we reach a certain minimum distance

        }

        Keystone.Types.Color mBackgroundColor = new Color(65, 94, 152, 255);
        Color mStarColor = Color.Yellow;
        Color mWorldColor = new Color(51, 153, 255, 255); // Color.Gray;
        Color mOrbitLinesColor = Color.White; // new Keystone.Types.Color(100, 149, 237, 255);
        //Color mOrbitLinesColor = new Keystone.Types.Color(176, 196, 222, 255); // light steel blue
    	//Color mOrbitLinesColor = new Keystone.Types.Color(70, 130, 180, 255); // steel blue
		//Color mOrbitLinesColor = new Keystone.Types.Color(102, 102, 102, 255); // dove gray
		//Color mOrbitLinesColor = new Keystone.Types.Color(224, 233, 233, 255); // baby blue
		//Color mOrbitLinesColor = new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey

		//color = new Keystone.Types.Color(255, 26, 0, 255); // scarlet  <-- nice color for enemy ship markers

        #region StarMap
        private void ConfigureGalaxyMapView()
        {
        	mMode = MapMode.Galaxy;
            Keystone.Portals.ZoneRoot zoneRoot = (Keystone.Portals.ZoneRoot)Context.Scene.Root;


            // RESTORE STATE FOR STARMAP
            // the HUD is the overlay scene.
  
            if (Context.Viewpoint == null || Context.Viewpoint.BlackboardData == null)
            	throw new Exception ("NavigationHud.ConfigureStarMapView() - ERROR: Viewpoint or CustomData is NULL.");

            Context.Viewpoint.BlackboardData.SetString ("focus_entity_id", mGalacticView.ID);
            Context.Viewpoint.BlackboardData.SetString ("control", "user");
            Context.Viewpoint.BlackboardData.SetString ("behavior", "behavior_orbit"); 
            double radius = this.Context.GetZoomToFitDistance (mGalacticView.BoundingBox.Radius);
            Context.Viewpoint.BlackboardData.SetDouble ("orbit_radius", -radius);
            // TODO: set Min and Max zooms? - 
            // viewDepth as max zoom
            //       double viewDepth =
            //           this.ViewportControls[0].Viewport.Context.GetZoomToFitDistance(zoneRoot.HalfDepth * DISTANCE_SCALING);
            this.Context.Viewport.BackColor = mBackgroundColor;
            this.Context.ShowFPS = true;
            this.Context.ShowCullingStats = false;
            // grid - // space our rows and colums so that entire grid is same width and depth as a Zone
            this.Grid.Enable = true;
            this.Grid.Position = new Vector3d (0,0,0);
            this.Grid.InfiniteGrid = false;
            this.Grid.UseFarFrustum = false;
            this.Grid.AutoScale = false;
            this.Grid.OuterRowCount = zoneRoot.RegionsAcross;
            this.Grid.RowSpacing = (float)mScaledZoneDiameter * zoneRoot.RegionsAcross / (float)Grid.OuterRowCount;
            this.Grid.OuterColumnCount = zoneRoot.RegionsDeep;
            this.Grid.ColumnSpacing = (float)mScaledZoneDiameter * zoneRoot.RegionsDeep / (float)Grid.OuterColumnCount;
// TODO
//            Keystone.Cameras.ViewpointController viewCtrlr = mViewportControls[0].Viewport.Context.ViewpointController;
//            viewCtrlr.MaxZoom = 100000.0f;
//            viewCtrlr.MinZoom = 1.0f;
//            viewCtrlr.ZoomMultiplier = 100; // 100000;
//            // TODO: "chase" should just be a behavior node that we select during performing of
//            //       the behavior and so these "behavior" states would be part of the 
//            //       ViewpointController.State[] dictionary perhaps that our behaviors can query?
//            ((Keystone.Cameras.ChaseViewController)viewCtrlr).MaxRadius = 15000.0f;
//            ((Keystone.Cameras.ChaseViewController)viewCtrlr).MinRadius = 1500.0f;
        }
        #endregion


        #region SystemMap
        void ConfigureSingleSystemView(MapMode mode)
        {
            mMode = mode;

            Context.Viewpoint.BlackboardData.SetString("focus_entity_id", mSingleSystemView.ID);
            Context.Viewpoint.BlackboardData.SetString("control", "user");
            Context.Viewpoint.BlackboardData.SetString("behavior", "behavior_orbit");
            // NOTE: no planets are computed at >100AU so scaled 250AU radius is too large.  
            //       But what if we want to plot a course to the edge of a solar system?
            double HALF_DIAMETER = 0.5d;
            double radius = this.Context.GetZoomToFitDistance(AppMain.REGION_DIAMETER * mSingleSystemScale.x * HALF_DIAMETER);
            Context.Viewpoint.BlackboardData.SetDouble("orbit_radius", -radius);

            // configure system view
            this.Context.Viewport.BackColor = mBackgroundColor;
            this.Context.ShowEntityBoundingBoxes = false;
            this.Context.ShowFPS = true;
            this.Context.ShowCullingStats = false;
            
            // TODO: implement circular grid with AU intervals posted
            this.Grid.Enable = true;
            this.Grid.InfiniteGrid = false;
            // space our rows and colums so that entire grid is same width and depth as a Zone
            this.Grid.UseFarFrustum = false;
            this.Grid.OuterRowCount = 10;
            this.Grid.RowSpacing = (int)(mScaledZoneDiameter / Grid.OuterRowCount);
            this.Grid.OuterColumnCount = 10;
            this.Grid.ColumnSpacing = (int)(mScaledZoneDiameter / Grid.OuterColumnCount);

            // TODO: 
//            Keystone.Cameras.ViewpointController viewCtrlr = mViewportControls[0].Viewport.Context.ViewpointController;
//            viewCtrlr.MaxZoom = 100000.0f;
//            viewCtrlr.MinZoom = 1.0f;
//            viewCtrlr.ZoomMultiplier = 100; // 100000;
//            ((Keystone.Cameras.ChaseViewController)viewCtrlr).MaxRadius = 1500.0f;
//            ((Keystone.Cameras.ChaseViewController)viewCtrlr).MinRadius = 100.0f;
        }


        void ConfigureSatelliteView(MapMode mode)
        {
            mMode = mode;
            Context.Viewpoint.BlackboardData.SetString("focus_entity_id", mPlanetaryView.ID);
            Context.Viewpoint.BlackboardData.SetString("control", "user");
            Context.Viewpoint.BlackboardData.SetString("behavior", "behavior_orbit");
            // NOTE: no planets are computed at >100AU so scaled 250AU radius is too large.  
            //       But what if we want to plot a course to the edge of a solar system?
            double radius = this.Context.GetZoomToFitDistance(AppMain.REGION_DIAMETER * mSingleSystemScale.x);
            Context.Viewpoint.BlackboardData.SetDouble("orbit_radius", -radius);

            // configure system view
            this.Context.Viewport.BackColor = mBackgroundColor;
            this.Context.ShowEntityBoundingBoxes = false;
            this.Context.ShowFPS = true;
            this.Context.ShowCullingStats = false;

            this.Grid.Enable = false;
        }

        Vector3d mStarTranslation;
        // since Systems can be outside of area of interest of the player's starship
        // we still need to be able to page in basic data about other stellar systems so 
        // they must be paged in temporarily
        private void SystemMapPageCompleteHandler(Amib.Threading.IWorkItemResult result)
        {
            Keystone.IO.ReadContext rc = (Keystone.IO.ReadContext)result.State;

            // NOTE: rc.Node can still be NOT null even when the Zone is Empty and does not
            //       exist in the XMLDB because our XMLDB automatically first tries to find
            //       an instanced version of the Entity (if cloneEntities == false) and if
            //       that Zone was created by Pager.cs and EmptyZone entity flag set, then 
            //       that will return here as rc.Node.  So we must test for that flag.
            if (rc.Node != null)
            {
            	Keystone.Celestial.Body primary = rc.Node as Keystone.Celestial.Body;
            	if (primary != null)
            		Debug.WriteLine ("NavigationHUD.SystemMapPageCompletedHandler() - System BEGIN loading...");

                // NOTE: we cache the translation, but the primary is rendered in scaled galactic coordinates.
                //       This might change to origin.  But for now, when our navplacer places a waypoint
                //       that is in SingleSystem view, we need to note that we are starting in galactic coords.
                // todo: actually here we are using primary.Translation which should be at origin with just one Star in the system.
                mStarTranslation = primary.Translation;

                // TODO: following RemoveChild is broken and needs to be fixed after making mSingleSystemMap node the Star.
                //       It's first child is the starModel!            		
                // remove any previous Star proxy from the mSystemMap
                if (mSingleSystemView.ChildCount > 0)
            	{
            		mSingleSystemView.RemoveChild (mSingleSystemView.Children[0]);   				
					// TODO: do we need to decrementRef of this child proxy?
            	}

				// sphere mesh added for Geometry
				Mesh3d starMesh = Mesh3d.CreateSphere (STAR_MESH_RADIUS / 2f, 16, 16, true);
				Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance (MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", null, null, null, null, true);
				appearance.Material.Emissive = mStarColor;
				
				Model primaryModel = (Model)Repository.Create (typeof(Model).Name);
				primaryModel.AddChild (appearance);
				primaryModel.AddChild (starMesh);
				Keystone.IO.PagerBase.LoadTVResource (primaryModel, true);
				
				mSingleSystemView.AddChild (primaryModel);

                // todo: when the viewport resizes, i should be repositioing all the proxies and orbitlines
                Proxy3D[] childProxies = GenerateProxyWorlds(primary, mSingleSystemScale.x);

                if (childProxies != null)
                    for (int i = 0; i < childProxies.Length; i++)
                    {
                        mSingleSystemView.AddChild(childProxies[i]);

                        //double radius = world.OrbitalRadius * mSingleSystemScale.x;
                        //Proxy3D orbitLines = GenerateOrbitLines(world,
                        //										radius,
                        //                                        world.OrbitalEccentricity, 
                        //                                        world.OrbitalInclination, 
                        //                                        world.OrbitalProcession);

                        // purely circular orbitlines that match the worldProxy's equal distance orbit lines
                        Proxy3D orbitLines = GenerateOrbitLines(childProxies[i].ReferencedEntity,
                                                                childProxies[i].Translation.Length,
                                                                0.0f,
                                                                0.0f,
                                                                0.0f);

                        mSingleSystemView.AddChild(orbitLines);
                    }

                mDistanceMap = GenerateDistanceMap(primary);

                // http://arstechnica.com/science/2013/01/binary-star-systems-make-for-unstable-planets/
                // A significant fraction of stars in the Milky Way are in binary systems. 
                // Some, like the Alpha Centauri system, are tight binaries: the two stars 
                // comprising Alpha Centauri are about 18 astronomical units (AU) apart. 
                // (1 AU is Earth's average distance to the Sun. For comparison, Neptune's 
                // average distance is about 30 AU.) However, some are known as wide 
                // binaries, with separations greater than 1000 AU.


                // TODO: fix the worlds not being in correct orbit spots (works for some but not all Systems)
                // TODO: fix that the selected Star in the SystemMap isn't near 0,0,0. NO.  That's not necessary
                //       We can "flyto" using it's mGalaxyScale 'd translation.

                // TODO: how do we animate the Viewpoint to transition from galaxy map to single system map?
                // TODO: we need to calculate the zoom to fit distance
                // TODO: can we add a fly to animation to the viewpoint entity?
                //		  - how do we normally set up a fly to animation to a selected world or star?
                // TODO: Let's think on what we want... we want the mSystemMap to be centered at 0,0,0 right? No.
                //       We then want to switch from Starmap to that SystemMap
                // 		 We then want to animated automatically til the SystemMap fills the screen.
                //		 After completing the animation, we want user control of the viewpoint back.
                //		 - can we break down what's going on in the ViewpointController?
                //		 - is the bbox of the mSystemMap correct and encompassing planet orbits?
                //       Keep BREAKING DOWN the requirements into small steps that i can test and validate.
                //       and combined, all steps becomes final solution.

                // TODO: Ok, time for camera transition during our navigation to the selected star
                //       What do we need to do?
                // TODO: the focus entity will be the Star proxy
                //		 on single click should we focus on a star proxy rather than starmap center?
                // TODO: zoom to fit distance calculated
                //       zoom in and out when navigating back to star map

                Entity entityStar = mSingleSystemView;
				Vector3d position = entityStar.Translation;
				Context.Hud.Grid.Position = position ;
            	
				// fly to with zoom to fit the bbox of the mSingleStarMap
				// TODO: I think the MoveTo never switches back to "user" control behavior
				//       after the animation flyto completes.
				// TODO:  - when do we do zoom to fit distance for MoveTo?  I think it's just
				//     computed from the diameter of the focus_entity_id 
				//     - verify this.  And our transition should work because
				//     we're not changing the camera scale or anything, we're also
				//     adjusting the planet scales to the star maps scale right? 
				// - added "user" behavior when going back to StarMap from SystemMap.
				//   seems to work ok.  Still need a transition animation.
				// - need "orbit" behavior after animation as well. so we need some kind
				//   of notification event that the flyto animation completed.
				// fixed: animation flyto now animates because normal viewpoint behavior tree is used. (instead of orbit only behavior tree)
				// TODO: entityStar's bbox is too small.  We need bbox of entire scaled down stellar system, can we create a proxy that does this? Because
				//       otherwise it's SceneNode's that contain hierarchical bboxes and not Entities.  Or something like a Region proxy
				// We need a type of Proxy that has no Model but still has a bounding box.  Basically a Region (NOT a Zone though, do we need to reference the Zone though?)
				// TODO: need to zoom back out to StarMap view after leaving SystemMap.
				//       or can we just use a box that is not visible?  Well that's pretty much a Region isn't it?
				// TODO: limit planet orbit generation to be within a certain min and max distance so that the SystemMaps are easier to see.
				//       Or can we call a version of MoveTo() that accepts a min & max distance from target? Then we could compute
				//       a ZoomToFit distance based on furthest orbit.
				//       There is a "orbit_radius" var, maybe we can assign that? along with min_orbit_radius and max_orbit_radius?
				//       Indeed, i think we must have min/max orbit_radius because setting it will just get overwritten during flyto animation
				//       TODO: we're not in "orbit" behavior upon switching to SystemMap.

				// -> SetString("focus_entity_id") is called for us in Viewpoint_MoveTo() -> Context.Viewpoint.CustomData.SetString("focus_entity_id", mSystemMap.Children[0].ID);
				// TODO: currently what is happening is, I remove the starmap view and add the system view and then i zoom 
				//       into the star.  I think this is ok so long as the StarProxy we put into the SystemView is at same coordinate.
				//       Then we can zoom in to reveal the planets in the System.
				// TODO: what is the current functionality?  It seems to happen so fast.
				//    
				//		 1) camera seems to start off correct position which is at StarMap position and
				//		 2) target starProxy seems to start off at same position as the StarMap's version of that star.
				//		 3) we zoom in really close
				//       4) camera suddenly jumps back towards viewer to min distance setting
				//		 5) on going back to StarMap, camera is fixed at max distance setting
				// TODO: I think the max and min distances are reversed somehow. eg. -60 is our distance away from focus_entity
				// TODO: what if i just insta positioned the camera and forgot about "flyto."  Then we could move on.  Cut this difficult feature
				//		 I think this is what I must do.  1.0 just a quick cut to the system map.  Ha, if a feature is too tough to implement, cut it!
				//       I would add a new behavior action that just jumped to instead of "flyto"
				// TODO: i still need to use Viewpoint Behaviors and I just need to do it.  wtf.
				//       A snap-to behavior instead that transitions to orbit behavior.
//				Context.Viewpoint_MoveTo (entityStar, 1.0f, true);
            }
        }

        double[] mDistanceMap;
        double mWorldSpacing;
        // map the linear world spacing to an array of actual distances so that starships icons can be rendered appropriately
        private double[] GenerateDistanceMap(ModeledEntity parent)
        {
            int worldCount = GetWorldCount(parent.Children);
            double[] result = new double[worldCount];
            mWorldSpacing = this.mScaledZoneDiameter / 2d / worldCount;
            int index = 0;

            foreach (Node child in parent.Children)
            {
                Keystone.Celestial.World world = child as Keystone.Celestial.World;
                if (world == null) continue;

                result[index++] = world.Translation.Length;
            }

            return result;
        }


        private int GetDistanceMapIndex(double distance)
        {
            if (mDistanceMap == null) return 0;

            for (int i = 0; i < mDistanceMap.Length; i++)
                if (distance < mDistanceMap[i])
                    return i;

            return mDistanceMap.Length - 1;
        }

        private Proxy3D[] GenerateProxyWorlds(ModeledEntity parent, double diameter)
        {
            // add world proxies to the Primary Body Proxy
            if (parent.Children != null)
            {
                int worldCount = GetWorldCount(parent.Children);
                Proxy3D[] results = new Proxy3D[worldCount];

                double worldSpacing = this.mScaledZoneDiameter / 2d / worldCount;
                int index = 0;

                foreach (Node child in parent.Children)
                {
                    Keystone.Celestial.World world = child as Keystone.Celestial.World;
                    if (world == null) continue;

                    string id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Entities.Proxy3D));
                    Proxy3D worldProxy = new Proxy3D(id, world);
                    worldProxy.Serializable = false;
                    worldProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                    worldProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
                    worldProxy.UseLargeFrustum = false;
                    worldProxy.Pickable = true;
                    worldProxy.Dynamic = false;

                    // TODO: vary the sphere width depending on rocky planet or gas giant.
                    Mesh3d worldMesh = Mesh3d.CreateSphere(PLANET_MESH_RADIUS, 16, 16, true);
                    Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", null, null, null, null, true);
                    appearance.Material.Emissive = mWorldColor;

                    Model worldModel = (Model)Repository.Create(typeof(Model).Name);
                    worldModel.AddChild(appearance);
                    worldModel.AddChild(worldMesh);

                    // load the proxy model
                    Keystone.IO.PagerBase.LoadTVResource(worldModel, true);
                    worldProxy.AddChild(worldModel);

                    // play the animation to get the world translation in correct position along elliptical orbit animation
                    // POSTPONED - we no longer animate orbits for celestial bodies in version 1.0
                    //if (world.Animations != null)
                    //   world.Animations.Play (0);
                    // NOTE: we use World.Translation and not World.GlobalTranslation because 
                    //       we are adding the world as child to starProxy, so it's hierarchical.
                    Vector3d translation = parent.Translation * mSingleSystemScale + (Vector3d.Normalize(world.Translation) * worldSpacing * (index + 1));
                    //Vector3d translation = world.Translation * mSingleSystemScale;
                    worldProxy.Translation = translation;

                    results[index] = worldProxy;
                    index++;
                }
                return results;
            }
            return null;
        }

        private int GetWorldCount(Node[] nodes)
        {
            if (nodes == null || nodes.Length == 0) return 0;

            int result = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] is Keystone.Celestial.World)
                    result++;
            }

            return result;
        }

        // TODO: we need to remove and then regenerate the waypoint tabs when
        // 1) switching map modes
        // 2) adding/removing waypoints
        // 3) selecting different allied vehicles (though for these we cannot move their waypoints since player does not own them
        private ModeledEntity[] mWaypointTabs;
        private Game01.GameObjects.NavPoint[] mWaypoints;
        void RemoveWaypointTabs()
        {
            // would it be simpler to delete the existing and recreate them whenever
            // we add/remove a waypoint?
            if (mWaypointTabs == null || mWaypointTabs.Length == 0) return;

            for (int i = 0; i < mWaypointTabs.Length; i++)
            {
                RemoveHUDEntity_Retained(mWaypointTabs[i]);
            }

            // TODO: do we have to decrement ref each tab or are they unloaded
            // once we remove retained them?
            mWaypointTabs = null;
            mWaypointTabsChanged = false;
        }

        private bool WaypointsChanged()
        {
            object tmp = mVehicle.GetCustomPropertyValue("navpoints");
            if (tmp == null && mWaypoints == null) return false;
            if (tmp != null && mWaypoints == null) return true;

            Game01.GameObjects.NavPoint[] waypoints = (Game01.GameObjects.NavPoint[])tmp;
            if (waypoints.Length != mWaypoints.Length) return true;

            for (int i = 0; i < waypoints.Length; i++)
                if (waypoints[i] != mWaypoints[i]) return true;


            return false;
        }

        /// <summary>
        /// Generates the mouse pickable / draggable waypoint tab icons
        /// </summary>
        /// <param name="waypoint"></param>
        /// <returns></returns>
        void GenerateWaypointTabs()
        {
            if (WaypointsChanged())
            {
                RemoveWaypointTabs();
                object tmp = mVehicle.GetCustomPropertyValue("navpoints");

                if (tmp != null)
                {
                    Game01.GameObjects.NavPoint[] waypoints = (Game01.GameObjects.NavPoint[])tmp;
                    mWaypoints = waypoints;
                    mWaypointTabsChanged = false;

                    if (waypoints == null || waypoints.Length == 0) return;

                    mWaypointTabs = new ModeledEntity[waypoints.Length];

                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        // TODO: if we use this id naming scheme to identify waypoints in other areas of the program
                        // then we need to be really careful to delete and remove these waypoint proxies from the Repository
                        // when they are deleted, OR when we re-order them!  The re-ordering is what makes this potentially
                        // a bad idea.  But what other option do we have?  We can put it in the "proxy.Name" property
                        // and then we can just rename them easily, but then we don't want to render the name label.
                        string id = Repository.GetNewName(typeof(Keystone.Entities.Proxy3D));
                        mWaypointTabs[i] = new ModeledEntity(id);
                        mWaypointTabs[i].SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                        mWaypointTabs[i].ScreenSpaceSize = VEHICLE_SCREENSPACE_SIZE;
                        mWaypointTabs[i].UseFixedScreenSpaceSize = true;
                        // TODO: perhaps we can tie the vehicle ID to the name so we can reference it?
                        // We need a way for the NavPointPlacer to know that this waypoint HUD element
                        // is tied to which vehicle.
                        mWaypointTabs[i].Name = "#WAYPOINT" + "," + waypoints[i].RowID + "," + mVehicle.ID;

                        string texturePath = System.IO.Path.Combine(AppMain._core.DataPath, @"editor\MouseOverTile.png");
                        // NOROTATION == custom rotation which in this case is to give us Y axis rotation like our stars 	
                        Model waypointIcon = Helper.Load3DBillboardIcon(texturePath, 1f, 1f, MTV3D65.CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION);

                        mWaypointTabs[i].AddChild(waypointIcon);

                        Keystone.IO.PagerBase.LoadTVResource(waypointIcon);


                        Vector3d translation = waypoints[i].Position;

                        if (mMode == MapMode.Galaxy)
                        {
                            translation += Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i].RegionID);
                            translation *= mGalaxyScale;
                            mNavPointPlacer = (Tools.NavPointPlacer)Context.Workspace.CurrentTool;
                            if (mNavPointPlacer == null) return;

                            Debug.Assert(mNavPointPlacer != null);
                            mNavPointPlacer.mCoordinateScaling = mGalaxyScale;
                            mNavPointPlacer.mMode = MapMode.Galaxy;
                            mNavPointPlacer.mSingleSystemZoneID = null;
                        }
                        else if (mMode == MapMode.SingleSystem)
                        {
                            // each waypoint coord is in region space, however for those coordinates
                            // not in the current Zone (SingleSystem) we need to compute a relative
                            // translation so that the waypoint lines stay true.
                            Vector3d relativeTranslation = Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, mCurrentRegionID);
                            relativeTranslation -= Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i].RegionID);
                            translation -= (relativeTranslation + mStarTranslation);
                            translation *= mSingleSystemScale;

                            mNavPointPlacer = (Tools.NavPointPlacer)Context.Workspace.CurrentTool;
                            Debug.Assert(mNavPointPlacer != null);
                            mNavPointPlacer.mCoordinateScaling = mSingleSystemScale;
                            mNavPointPlacer.mMode = MapMode.SingleSystem;
                            mNavPointPlacer.mStarOffset = mStarTranslation;
                            mNavPointPlacer.mSingleSystemZoneID = waypoints[i].RegionID;
                        }
                        mWaypointTabs[i].Translation = translation;
                        AddHudEntity_Retained(mWaypointTabs[i]);
                    }
                }
            }
        }

        Keystone.Immediate_2D.Renderable3DLines GenerateWaypointLines(string vehicleRegionID, Vector3d vehiclePosition, Game01.GameObjects.NavPoint[] waypoints, Vector3d cameraPosition)
        {
            if (waypoints == null || waypoints.Length == 0) return null;

            Vector3d scale = mGalaxyScale;
            if (mMode == MapMode.SingleSystem)
            {
                scale = mSingleSystemScale;
            }

            Line3d[] lines = new Line3d[waypoints.Length];
                                        
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3d start, end;
                if (i == 0)
                {
                    start = vehiclePosition;
                    if (mMode == MapMode.Galaxy)
                    {
                        start += Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, vehicleRegionID);
                        start *= mGalaxyScale;
                    }
                    else
                    {
                        start *= mSingleSystemScale;
                    }
                }
                else
                {
                    start = waypoints[i - 1].Position;
                    if (mMode == MapMode.Galaxy)
                    {
                        start += Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i - 1].RegionID);
                        start *= mGalaxyScale;
                    }
                    else
                    {
                        Vector3d relativeTranslation = Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, mCurrentRegionID);
                        relativeTranslation -= Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i - 1].RegionID);
                        start += relativeTranslation - mStarTranslation; ;
                        start *= mSingleSystemScale;
                    }
                }


                end = waypoints[i].Position;
                if (mMode == MapMode.Galaxy)
                {
                    end += Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i].RegionID);
                    end *= mGalaxyScale;
                }
                else
                {
                    Vector3d relativeTranslation = Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, mCurrentRegionID);
                    relativeTranslation -= Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root, waypoints[i].RegionID);
                    end += relativeTranslation - mStarTranslation;
                    end *= mSingleSystemScale;
                }
                // create lines in camera space position
                lines[i] = new Line3d(start.x - cameraPosition.x, 
                                      start.y - cameraPosition.y,
                                      start.z - cameraPosition.z,
                                      end.x - cameraPosition.x, 
                                      end.y - cameraPosition.y, 
                                      end.z - cameraPosition.z); 

            }

            Color color = Color.White;
            Keystone.Immediate_2D.Renderable3DLines waypointLines = new Keystone.Immediate_2D.Renderable3DLines(lines, color);

            return waypointLines;

        }

        Proxy3D GenerateWaypointLine(string referencedEntityID, Vector3d start, Vector3d end)
        {
            string id = Repository.GetNewName(typeof(Proxy3D));
            Proxy3D waypointProxy = new Proxy3D(id, referencedEntityID);
            waypointProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            waypointProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
            waypointProxy.UseLargeFrustum = false;
            waypointProxy.Pickable = false;
            waypointProxy.Dynamic = false;
            waypointProxy.Name = "#WAYPOINT";

            Color color = Color.Green;
            
            Model waypointModel = Keystone.Celestial.ProceduralHelper.CreateLine(start, end, color, false);

            waypointProxy.AddChild(waypointModel);
            return waypointProxy;
        }
        
        Proxy3D GenerateAltitudeLine(string referencedEntityID, Vector3d start)
        {
        	string id = Repository.GetNewName (typeof(Proxy3D));
        	Proxy3D altitudeProxy = new Proxy3D(id, referencedEntityID);
            altitudeProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            altitudeProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
            altitudeProxy.UseLargeFrustum = false;
            altitudeProxy.Pickable = false;
            altitudeProxy.Dynamic = false;
            altitudeProxy.Name = "#ALTITUDE"; 
        	Color colorGreen = new Color(0, 255, 0, 255);
        	Color colorRed = new Color (255, 0, 0, 255);
        	
        	Vector3d end = start;
        	end.y = 0.0;        	

        	Color color = colorGreen;
        	if (start.y < 0.0)
        		color = colorRed;
        	        	
        	Model altitudeModel = Keystone.Celestial.ProceduralHelper.CreateLine (start, end, color, false);
        	
        	altitudeProxy.AddChild (altitudeModel);
        	return altitudeProxy;
        	
        }
        
        Proxy3D GenerateOrbitLines(Entity referencedEntity, double orbitalRadius, float orbitalEccentricity, float orbitalInclination, float orbitalProcession)
        {
            const string REQUIRED_PREFIX = "#ORBIT";
        	string id = REQUIRED_PREFIX + Repository.GetNewName (typeof(Proxy3D));
        	Proxy3D orbitProxy = new Proxy3D(id, referencedEntity);
            orbitProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            orbitProxy.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Background, false);
            orbitProxy.UseLargeFrustum = false;
            orbitProxy.Pickable = false;
            orbitProxy.Dynamic = false;
            orbitProxy.Name = null;
            int segmentCount = 100;
                            
        	
			
			bool quadLines = false;
	  		Keystone.Celestial.ProceduralHelper.InitOrbitLineStrip(orbitProxy, segmentCount, orbitalRadius, orbitalEccentricity,
    	                                                       	   orbitalInclination, orbitalProcession, mOrbitLinesColor, quadLines);
  			                    
        
			return orbitProxy;
        }
#endregion 

    }
}
