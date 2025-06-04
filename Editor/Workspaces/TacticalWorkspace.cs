using System;
using System.Diagnostics;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using KeyCommon.Traversal;
using KeyEdit.Controls;
using Keystone.Resource;

namespace KeyEdit.Workspaces
{
	/// <summary>
	/// Description of TacticalWorkspace.
	/// </summary>
	public class TacticalWorkspace : WorkspaceBase3D
	{
        internal string mStartingRegionID;
        Keystone.Vehicles.Vehicle mVehicle;
        uint VIEWPORTS_COUNT = 1; // if quicklook has a viewport, we must increment the VIEWPORTS_COUNT by 1
			
		// TODO: test that we can instantiate the workspace to receive the EntityAdded event
		//		 and delay rendering until we do.  Then on EntityAdded we create the viewpoint and 
		//		 attach it to the user's Vehicle, then bind the viewpoint.		
		public TacticalWorkspace(string name) 
            : base(name)
		{
			
		}
		
		
		public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
        	try 
        	{
                // NOTE: Dont call base.Configure because it'll instantiates a viewportcontrol we dont want here
                //base.Configure(manager); 

                if (manager == null) throw new ArgumentNullException("TacticalWorkspace.Configre() - ERROR: WorkspaceManager parameter is NULL.");
	            if (scene == null) throw new ArgumentNullException("TacticalWorkspace.Configre() - ERROR: scene cannot be NULL.");
	
	            mWorkspaceManager = manager;
	            mScene = scene;

                // NOTE: there is only one mWorkspaceManager which we call .LoadLayout() on in EditorWorkspace
  //              mWorkspaceManager.LoadLayout();
	
	            // NOTE: dim the mViewportControls array before mDocument creation 
	            mViewportControls = new KeyEdit.Controls.ViewportControl[VIEWPORTS_COUNT];
	
	            // create our viewports document. 
	            mDocument = new KeyEdit.Controls.ViewportsDocument(this.Name, mDisplayMode, OnViewportOpening, OnViewportClosing);
	            
	            // Commented out because no Drag Events need to be wired to main viewport
	            // WireDocumentEvents();

	            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, OnGotFocus);
	
	            // ConfigureLayout will initialize the viewportcontrols but must be called 
            	// AFTER .CreateWorkspaceDocumentTab()
            	((KeyEdit.Controls.ViewportsDocument)mDocument).ConfigureLayout(mDisplayMode);


                // NOTE: creating and binding of viewponts is also handled on OnEntityAdded when user's vehicle is loaded
                // TODO: If the vehicle's Zone is not paged in (such as in cases where the Vehicle's starting Zone is 
                //       far from the center Zone) then the Vehicle never gets paged in and the Viewpoint never gets bound!!!
                // - i think to fix this issue, the only option is to serialize the viewpoint for the user's ship in the
                //   SceneInfo right?
                // TODO: find vehicle finds first any vehicle so this will break when we have more than 1 vehicle in the game!!!
                //       we need to ensure we are searching for the PLAYER'S vehicle.
                mVehicle = FindVehicle();
                if (mVehicle != null)
                {
                    InitializeViewpoint(mVehicle.Region.ID);
                    AttachVehicle(mVehicle);
                }
                else
                    InitializeViewpoint(mStartingRegionID);

	 			if (mDocument.InvokeRequired)
                	mDocument.Invoke((System.Windows.Forms.MethodInvoker)delegate { Show(); });
            	else
                	Show();

            
	            this.Resize();
        	}
        	catch (Exception ex)
        	{
        		System.Diagnostics.Debug.WriteLine ("TacticalWorkspace.Configure() - ERROR: " + ex.Message);
        	}
        }
		
		
		protected override void ConfigureHUD(Keystone.Cameras.Viewport viewport)
        {       
            if (viewport != null)
                if (viewport.Context.Hud != null)
                {                       
                    // space our rows and colums so that entire grid is same width and depth as a Zone
                    viewport.Context.Hud.Grid.InfiniteGrid = false;
                    viewport.Context.Hud.Grid.UseFarFrustum = true;
                    viewport.Context.Hud.Grid.AutoScale = false;
					viewport.Context.Hud.Grid.DrawAxis = false;
                    viewport.Context.Hud.Grid.OuterRowCount = 40;
                    viewport.Context.Hud.Grid.RowSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterRowCount);
                    viewport.Context.Hud.Grid.OuterColumnCount = 40;
                    viewport.Context.Hud.Grid.ColumnSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterColumnCount);
                    
					viewport.Context.Hud.Grid.InnerColor = new Keystone.Types.Color(45, 57, 86, 255);
					Keystone.Types.Color outerColor = new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey
                    viewport.Context.Hud.Grid.OuterColor = outerColor; // new Keystone.Types.Color(69, 83, 114, 255);
                    
                    viewport.Context.AddCustomOption (null, "show axis indicator", typeof(bool).Name, false);
                    viewport.Context.AddCustomOption (null, "show pathing debug information", typeof(bool).Name, false);
                    
                    viewport.Context.AddCustomOption (null, "show orbit lines", typeof(bool).Name, false);
                    viewport.Context.AddCustomOption (null, "show celestial grid", typeof(bool).Name, false);
                    
    				viewport.Context.AddCustomOption (null, "show motion field", typeof(bool).Name, false);
					viewport.Context.AddCustomOption (null, "show star map", typeof(bool).Name, true);
                    viewport.Context.AddCustomOption (null, "show hardpoint icons", typeof(bool).Name, false);
        			viewport.Context.AddCustomOption (null, "show vehicle icons", typeof(bool).Name, false);
        			viewport.Context.AddCustomOption (null, "show celestial icons", typeof(bool).Name, false);
                }
                
                
            //// TODO: i should  always read these values from ini 
            //// and do so via vpControl.ReadSettings (iniFile)
            //viewport.Cursor = AppMain._core.PrimaryContext.Viewport.Cursor;
            //viewport.BackColor = AppMain._core.PrimaryContext.Viewport.BackColor;
        }

        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs args)
        {

            if (mViewportControls[args.ViewportIndex] == null)
            {
                ViewportEditorControl  c =
                    new ViewportEditorControl (CreateNewViewportName((int)args.ViewportIndex));
                c.ShowToolbar = true;
                mViewportControls[args.ViewportIndex] = c;
                args.HostControl.Controls.Add(c);


                ConfigureViewport(c, new Huds.TacticalHud());

                // "cam_speed" must always be set

                // hud options
                //c.Viewport.Context.AddCustomOption("", "bleh", typeof(float).Name, 10.0f);
                //c.Viewport.Context.AddCustomOption ("", "bleh2", typeof(float).Name, 1f);

                // Bind() to viewpoint which will give us the camera control functionality we need
                // TODO: i do not like binding viewpoint here.  I should be able to bind it and rebind it
                //       as I please elsewhere.
// NOTE: In tactical workspace, binding of viewpoint to viewport.Context occurs after AttachVehicle()
//                c.Viewport.Context.Bind(mViewpoint);


                // TODO: i have not tested the following two lines with NavWorkspace
                //       since a code refactor of all workspaces june.10.2013
                //       however, they don't create the hud or enable picking, they just tweak them
                ConfigureHUD(c.Viewport);
                ConfigurePicking(c.Viewport);
            }


        }

        private void InitializeViewpoint(string startingRegionID)
        {
            // server doesn't care about viewpoints (only the vehicle that it's bound to) we just generate a viewpoint here.
            //
            string id = Repository.GetNewName(typeof(Keystone.Entities.Viewpoint));
            Keystone.Entities.Viewpoint vp = (Keystone.Entities.Viewpoint)Repository.Create (id, "Viewpoint");
            vp.Serializable = false;

            Keystone.Behavior.Composites.Composite behavior = EditorWorkspace.CreateOrbitOnlyBehavior();
            vp.StartingRegionID = startingRegionID; // April.22.2017 - line added, but why did it work ok without it? 
            vp.AddChild(behavior);
            vp.Update(0); // HACK - call Update(0) to initialize Viewpoint.CustomData
  
            mViewpointsInitialized = true;

            // bind to the context
            mViewportControls[0].Viewport.Context.Bind(vp);
  //          System.Diagnostics.Debug.Assert(AppMain.mLocalVehicleID == vehicle.ID);
            System.Diagnostics.Debug.Assert(vp == mViewportControls[0].Viewport.Context.Viewpoint);

        }

        /// <summary>
        /// Iterate through the Scene and find the user's vehicle
        /// </summary>
        private Keystone.Vehicles.Vehicle FindVehicle()
        {
            Predicate<Keystone.Elements.Node> vehicleMatch = e => {
                if (e is Keystone.Vehicles.Vehicle && 
                   ((Keystone.Vehicles.Vehicle)e).GetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.PlayerControlled))
                    return true;
                else
                    return false;
            };

            Keystone.Elements.Node[] results = mScene.Root.Query(true, vehicleMatch);
            if (results == null) return null;

            // iterate through all results to find correct user Vehicle
            for (int i = 0; i < results.Length; i++)
            {
                Keystone.Vehicles.Vehicle vehicle = (Keystone.Vehicles.Vehicle)results[i];
                // NOTE: AppMain.mLocalVehicleID is assigned in FormMain.Commands.Worker_NodeCreate()
                if (vehicle.ID == AppMain.mPlayerControlledEntityID)
                    return vehicle;
            }

            return null;
        }

        /// <summary>
        /// Bind the user's vehicle to this workspace.
        /// We also create and bind a viewpoint to that vehicle.
        /// </summary>
        private void AttachVehicle(Keystone.Vehicles.Vehicle vehicle)
        {
            AppMain.mPlayerControlledEntityID = vehicle.ID;
            AppMain.mPlayerControlledEntity = vehicle;

 //           vehicle.AddChild(mViewportControls[0].Viewport.Context.Viewpoint);
            mViewportControls[0].Viewport.Context.Viewpoint.BlackboardData.SetString("focus_entity_id", vehicle.ID);

            this.mTacticalPlugin.Instance.SelectTarget(vehicle.Scene.Name, vehicle.ID, vehicle.TypeName);
        }

    #region IWorkspace
        public override void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
		{
            // don't call base.  TODO: maybe we should make base method abstract and not virtual
            // base.OnEntityAdded(parent, child);
            if (mVehicle == null)
            {
                if (child as Keystone.Vehicles.Vehicle != null)
                {
                    // is it user vehicle or NPC?
                    Keystone.Vehicles.Vehicle vehicle = (Keystone.Vehicles.Vehicle)child;
                    if (vehicle.GetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.PlayerControlled))
                    {
                        // is it local player's vehicle or remote
                        // how do we differentiate?
                        // - we can apply compare the Entity.ID with the "tag" on client/connection?
                        // - when we generate that vehicle, we can assign the entityID to connection/client tag
                        //	 - and that tag/ID needs to be stored in client config
                        //	- we need to make sure the server generates the ID for our Vehicle

                        if (child.ID == AppMain.mPlayerControlledEntityID)
                        {
                             AttachVehicle((Keystone.Vehicles.Vehicle)child);


                            // 1) New Universe
                            //   - user sets New Universe specifications and sends random seed used.
                            //   - user selects ship and captain's avatar
                            //   - this leads to Existing Universe code path
                            //
                            // 2) Existing Universe 
                            //  A) if joining existing universe for first time, 
                            //      - user ship selection occurs
                            //      - server creates DB record and initializes their Vehicle translation and orbit parent.
                            //      - server then waits for user to "join existing"
                            //      - this leads to RESUME PLAY (B) code path.
                            //
                            //  B) if joining existing universe to resume play,
                            //      - no client side ship selection
                            //      - server looks up ship record for this user.
                            //      - do we need to send user their entire vehicle state
                            //          - does the client request the state and receive response or does the server push all state to the client?
                            //        This could be a large operation to send all vehicle state to client.
                            //          - there is potentially a ton of state 
                            //              - prefab state
                            //              - crew stats
                            //              - hull damage
                            //              - ship components and damage
                            //              - other vehicles we clearly don't need that much data.  Only what we scan or request from friendly/allied controlled vehicle.
                            //              - can we send these over piecemeal? And what about other universe data like scans of planets and stars?  That
                            //              all needs to be sent piecemeal or on demand as needed.
                            //
                            //          - but it is TCP/IP so ok?
                            //

                            // In both cases, the server tells us which record in the db and thus which prefab we're using.


                            //	            mViewpointsInitialized = true;

                            // TODO: Bind viewpoint to the local player's Vehicle Viewpoint.  Or can we create a Viewpoint so
                            // that we don't have to attach one to the Vehicle during UniverseGen or server command?  The server
                            // doesn't care about/need the viewpoint right?  Only client needs it for 3D rendering camera position/orientation/control.
                            // So the client should create the Viewpoint when the Vehicle is loaded and here we should bind to it.

                            //BindViewpoints();
                            //           mViewportControls[0].Viewport.Context.Bind(clonedVP);
                        }
                    }
                }
            }
			System.Diagnostics.Debug.WriteLine ("TacticalWorkspace.OnEntityAdded() - ");
		} 
	
		public override void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
		{
            // don't call base.  TODO: maybe we should make base method abstract and not virtual
			// base.OnEntityRemoved(parent, child);
			
			System.Diagnostics.Debug.WriteLine ("TacticalWorkspace.OnEntityRemoved() - ");
		}


        KeyPlugins.AvailablePlugin mTacticalPlugin;
        public override void Show()
		{
			base.Show();
			
			
			mDocument.Visible = true;
            mDocument.Enabled = true;

            if (mQuickLookHostPanelInitialized == false)
            {

                //if (mQuickLookHostPanel == null)
                //{
                //    mQuickLookHostPanel = new KeyEdit.GUI.QuickLookHostPanel();

                //    mQuickLookPanel = new GUI.QuickLook3(AppMain.PluginService);
                //    this.mQuickLookHostPanel.Controls.Add(mQuickLookPanel);
                //    mQuickLookPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                //}

                ((WorkspaceManager)mWorkspaceManager).DockControl(mQuickLookHostPanel, "Quick Look Host", "leftDockSiteBar", "Quick Look Host",eDockSide.Left);
                mQuickLookHostPanelInitialized = true;
            }

            if (mPluginPanelInitialized == false)
            {
                if (mTacticalPlugin == null)
                {
                    mTacticalPlugin = AppMain.PluginService.SelectPlugin("Command & Control");                   
                }

                ((WorkspaceManager)mWorkspaceManager).DockControl(mTacticalPlugin.Instance.MainInterface, "Command && Control", "leftDockSiteBar", "Command && Control", eDockSide.Left);
                mPluginPanelInitialized = true;
             
            }


            // Tactical workspace's Quick Look Panel will NOT have a viewport in Version 1.0
            //if (mQuickLook3DViewportEnabled)
            //{
            //	QuickLookViewportInitialization(VIEWPORTS_COUNT);
            //}
        }

        public override void Hide()
        {
            base.Hide();

            mDocument.Visible = false;
            mDocument.Enabled = false;

            // undock the quick look container
            if (mQuickLookHostPanelInitialized == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Quick Look Host", "leftDockSiteBar");
                mQuickLookHostPanelInitialized = false;
            }

            if (mPluginPanelInitialized)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Command && Control", "leftDockSiteBar");
                mPluginPanelInitialized = false;
            }
        } 
	#endregion
	}
}
