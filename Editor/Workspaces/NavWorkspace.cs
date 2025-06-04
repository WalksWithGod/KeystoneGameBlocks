using System;
using System.Windows.Forms;
using Keystone.Entities; // for treeview
using System.Collections.Generic;
using Keystone.Types;
using KeyEdit.Controls;
using DevComponents.DotNetBar;

namespace KeyEdit.Workspaces
{
    public partial class NavWorkspace : WorkspaceBase3D
    {
        private struct ViewState
        {
            public Vector3d Translation;
            public double OrthoZoom;
        }


        uint VIEWPORTS_COUNT = 1; // if quicklook has a viewport, we must increment the VIEWPORTS_COUNT by 1
        Keystone.Entities.Viewpoint mViewpoint;
        private Stack<ViewState> mViewHistory;
		
        // we track seperate treeviews for galaxy, star systems and planetary views
        // because we want to be able to remember our preview location when navigating
        // back/forward/up in the nav workspace.
        //  - like wise we'll maintain the current viewport position for each view
        //    eg we use a view history stack
        //
        //
        // D:\dev\c#\KeystoneGameBlocks\Design\nav_plugin_panel.png
        // D:\dev\c#\KeystoneGameBlocks\Design\starwalk.jpg
        // D:\dev\c#\KeystoneGameBlocks\Design\mp_nav4_2_large.gif
        //      - http://www.producerelease.com/sgc/ 
        //      - http://www.producerelease.com/sgc/sgc2.htm
        // D:\dev\c#\KeystoneGameBlocks\Design\nearstar.gif
        // D:\dev\c#\KeystoneGameBlocks\Design\gliese2in7parsecs.png    <-- a good place to start as far as a hand made universe for star locations
                                    // all we need to do is limit our 1.0 test demo to these systems and to
        //                          // allow us to pass these stars in as parameters to our generator and thus
        //                          // not generate own stars, but intead only generate worlds

        // the above image can be placed horizontally in the plugin area when
        // in Nav mode and we want a short cut method to select between celestial
        // bodies in the stellar system.

        // it's a very easy gui aid to navigate to different celestial objects.
        // It can even be done when clicking the "nav" icon during normal 3d exterior view
        // to popup that panel instead of some other panel.



        public NavWorkspace(string name)
            : base(name)
        {
            SelectTool(Keystone.EditTools.ToolType.NavPointPlacer);
            mDisplayMode = ViewportsDocument.DisplayMode.Single;
            
            
            // - Workspace Document - add a custom 3d viewport document that has a fixed ortho
            // 
            // - Workspace Panel #1 - use a simple treeview for now with sphere world icons and star icons for 
            //    list depending on whether showing galactic, star system, planetary system views
            //      - treeview can be used to select a system to lookat in the 3d view
            //      we will prevent camera from leaving the system
            //      - they can isometricly pan the camera within the bounds however
            //      - based on the nav_plugin_panel.png layout of bodies
            //        only we'll make it vertical and as a regular left hand panel.
            //      legend plugin for each system showing all stars, and their planets
            //      and their moons and sats
            //      - treeview looks for stellarsystems and then looks for
            //        bodies
            //      - what about vehicles/artificial satelites?
            //          - HUD style replacement of entity renders with icons
            //              - how to exactly?
            //          - the nav workspace can set in context.Hud.someoption to signify
            //            that icons should be used for "vehicles" perhaps.
            //          - the nav workspace can maybe set in context.somerenderingoption to skip
            //            rendering of something's normal render... knowing it'll be replaced by hud
            //              - how do you then handle picking of the icons?  icons need to be a proxy
            //                and so a proxy must be a type of renderable entity that references another
            //                - so Hud must be able to create proxys.  An orbit line entity is a proxy.
            //                It can rely on it's own picking, but to pick means you've picked the item it references.
            //                - proxys are also used for the overall galactic star map... 
            //                   - how are proxys picked if not added to the scene though? else
            //                     our normal picking does not work... as in traversing scenegraph
            //                      - if added, when and when removed?
            //                   - HUD AS AN OVERLAY SCENE? <-- TRIED THIS AND WAS DISASTER. Dec.12.2012.  Using "Layer" bitflag masks for cull traversal is better.
            //                      - so an embedded scene that we can then traverse but which is otherwise not
            //                        part of the main scene.  Cuz hud elements arent "really there" in the main scene
            //                        but they can be in the overlay scene provided by the HUD.
            //                        - but what about the fact im currently adding some hud elements to the main scene
            //                          and dynamically adding/removign them.  can i avoid that?  can i make them overlay
            //                           - either it seems im updating the overlay hud scene.
            // 

            

            //   1.0 view will always be planar to the systems elliptic (and we may have all
            //      systems use same plane for 1.0)

            //  - using our preview code that takes a thumbnail, we can take a thumbnail of every
            //    star, world, moon and use different sizes for them and then lay them out 
            //    in our custom panel.
            //

            // 
            // - Workspace Plugin - Waypoint plugin
            //  - uses a listview treeview that has list of waypoints
            //      - with the list of maneuvers (locked, can only delete entire waypoints and recalc 
            //       the reqt maneuvers for subseqent waypoints)
            //      - if any waypoint is unreachable because of fuel potentially, it shows up as red.
            //      - giving\placing orders for maneuvers/waypoints/sequences 
        }


        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            try
            {
                //base.Configure(manager); // NOTE: Dont call base.Configure because it'll instantiates a viewportcontrol we dont want here

                if (manager == null) throw new ArgumentNullException("NavWorkspace.Configre() - WorkspaceManager parameter is NULL.");
                if (scene == null) throw new ArgumentNullException("NavWorkspace.Configre() - scene cannot be NULL.");

                mWorkspaceManager = manager;
                mScene = scene;
          
                //  create a distinct viewpoint that is not tied to a vehicle.  
			    mViewpoint = Keystone.Entities.Viewpoint.Create ("viewpoint_starmap", mScene.Root.ID);

                // NOTE: the behavior is initialized on first use on mViewpoint.Update()
			    Keystone.Behavior.Composites.Composite behaviorTree = EditorWorkspace.CreateOrbitAndSnapToBehavior();
                mViewpoint.AddChild (behaviorTree);

                // Feb.21.2017 - HACK - force mViewpoint.Update() so that the CustomData gets initialized in Behavior tree.
                //               Otherwise we get a race condition.
                mViewpoint.Update(0);

                // NOTE: dim the mViewportControls array before mDocument creation 
                mViewportControls = new ViewportControl[VIEWPORTS_COUNT];
           
			    // create our viewports document.  only need 1 to hold all 4 viewportControls
                mDocument = new KeyEdit.Controls.ViewportsDocument(this.Name, mDisplayMode, OnViewportOpening, OnViewportClosing);
            

                //mDocument.SendToBack(); <-- fix control zorder issues?
                mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, OnGotFocus);

                // ConfigureLayout will initialize the viewportcontrols but must be called 
                // AFTER .CreateWorkspaceDocumentTab()
                ((KeyEdit.Controls.ViewportsDocument)mDocument).ConfigureLayout(mDisplayMode);
                        
                
            
                // TODO: Configure is being called on mWorkspaceManager.Add (workspace, scene)
                //       and that is a lousy place to do it.   
                // TODO: race condition here... we're attempting to navigate to point before
                //       we've initialized the Viewpoint Behavior.  How can we navigate_to later?
                // Can we call this from the Workspace after the first mViewpoint.Update() has occurred?
                ((KeyEdit.Workspaces.Huds.NavigationHud)mViewportControls[0].Context.Hud).Navigate_To (null, "ZoneRoot");

                // https://gamedev.stackexchange.com/questions/104374/non-linear-scaling-to-show-a-sol-system
                // might as well place the orbits equal distance from eacch other and based on how many orbits there are and divide that by the regionDiameter and then scale them to fit.
                // This is what other apps do.  it's basically completely arbitrary.
                // todo: if im not going to scale the distances and sizes properly, then why bother
                // with a navigation implementation like that?  All you really need is a list of destinations and 
                // perhaps a general idea of where they are and in which direction.
                // todo: i could have seperate zoom levels for "inner planets" and "outer planets."
                // In fact, i can just include a scaling slider or set of buttons or mouse wwheel, and allow the user to customize the scale of both the planets and the distances.
                // NOTE: the star itself can have a different scaling than of the planets so that it does not dominate the view.

                // SetDisplayMode() in EditorWorkspace calls .Show
                // but here we dont call that so we directly call .Show.
                // Show() is required here to activate this tab properly right away.. or is it? Actually
                // the only reason Show() is not needed in the following case is because usually the edit tab
                // is visible when nav tab is created and we must explicilty click the nav tab
                // to switch which results in a call to .Show()
                // the caller should do it then from formmain that initiated creation of nav view
                if (mDocument.InvokeRequired)
                    mDocument.Invoke((System.Windows.Forms.MethodInvoker)delegate { Show(); });
                else
                    Show();

                this.Resize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TacticalWorkspace.Configure() - ERROR: " + ex.Message);
            }
        }


		protected override void ConfigureHUD(Keystone.Cameras.Viewport viewport)
		{
			base.ConfigureHUD(viewport);
			
			if (viewport != null)
                if (viewport.Context.Hud != null)
                {        		
					viewport.Context.Viewport.BackColor = Keystone.Types.Color.Black;

                    // cull everything but our HUD elements
                    // TODO: I think this is inadvertantly including HUD elements that are generated
                    //       at runtime from non HUD elements (eg. lense flare from Star)
                    //       It also allows rendering of labels.
                    // TODO: so we need to step through the code while in NavWorkspace and verify
                    //       that we're at least trying to skip rendering of things like Stars
                    viewport.Context.IncludeEntityAttributes(KeyCommon.Flags.EntityAttributes.All);
                    viewport.Context.AllowedEntityAttributes (KeyCommon.Flags.EntityAttributes.HUD);
						
					viewport.Context.ShowLineGrid = true;
					viewport.Context.Hud.Grid.Enable = true;
					// space our rows and colums so that entire grid is same width and depth as a Zone
                    viewport.Context.Hud.Grid.InfiniteGrid = false;
                    viewport.Context.Hud.Grid.UseFarFrustum = false;
                    viewport.Context.Hud.Grid.AutoScale = false;
					viewport.Context.Hud.Grid.DrawAxis = false;
                    viewport.Context.Hud.Grid.OuterRowCount = 40;
                    viewport.Context.Hud.Grid.RowSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterRowCount);
                    viewport.Context.Hud.Grid.OuterColumnCount = 40;
                    viewport.Context.Hud.Grid.ColumnSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterColumnCount);
                    
					viewport.Context.Hud.Grid.InnerColor =  new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey
					Keystone.Types.Color outerColor = new Keystone.Types.Color(45, 57, 86, 255);
                    viewport.Context.Hud.Grid.OuterColor = outerColor; // new Keystone.Types.Color(69, 83, 114, 255);
                    
                    viewport.Context.ShowEntityLabels = true;
                    //viewport.Context.AddCustomOption (null, "show labels", typeof (bool).Name, true);
                    viewport.Context.AddCustomOption (null, "show axis indicator", typeof(bool).Name, true);
                    viewport.Context.AddCustomOption (null, "show pathing debug information", typeof(bool).Name, false);
                    
                    viewport.Context.AddCustomOption (null, "show orbit lines", typeof(bool).Name, false);
                    viewport.Context.AddCustomOption (null, "show celestial grid", typeof(bool).Name, false);
                    
    				viewport.Context.AddCustomOption (null, "show motion field", typeof(bool).Name, false);
					viewport.Context.AddCustomOption (null, "show star map", typeof(bool).Name, false);
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
                ViewportNavigationControl c = 
                    new ViewportNavigationControl(CreateNewViewportName((int)args.ViewportIndex), mScene);
                c.ShowToolbar = true;
                mViewportControls[args.ViewportIndex] = c;
                args.HostControl.Controls.Add(c);


                ConfigureViewport(c, new Huds.NavigationHud());

                //c.Projection = Keystone.Cameras.Viewport.ProjectionType.Orthographic;
                //c.View = Keystone.Cameras.Viewport.ViewType.Top;
                // "cam_speed" must always be set
 
                // hud options
                //c.Viewport.Context.AddCustomOption("", "bleh", typeof(float).Name, 10.0f);
				//c.Viewport.Context.AddCustomOption ("", "bleh2", typeof(float).Name, 1f);

                // Bind() to viewpoint which will give us the camera control functionality we need
    			// TODO: i do not like binding viewpoint here.  I should be able to bind it and rebind it
    			//       as I please elsewhere.
                c.Viewport.Context.Bind(mViewpoint);

                
                // TODO: i have not tested the following two lines with NavWorkspace
                //       since a code refactor of all workspaces june.10.2013
                //       however, they don't create the hud or enable picking, they just tweak them
                ConfigureHUD(c.Viewport);
                ConfigurePicking(c.Viewport);
            }
        }

        public override void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("NavWorkspace.Unconfigure() - No WorkspaceManager set.");
        }

        public override void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            //throw new NotImplementedException();
        }

        public override void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            //throw new NotImplementedException();
        }
        
        public override void OnEntitySelected(Keystone.Collision.PickResults pickResult)
        {
            base.OnEntitySelected (pickResult);
                    
            // is the entity a proxy ? 
            // if so, can we run a OnBeginEntitySelected and OnEndEntitySelected?
            // Then, in the OnEndEntitySelected we can call the HUD option.
            
        
            if (mSelectedEntity == null) return;
        }

        #region IWorkspace Members
        public override void Show()
        {
            //base.Show(); // TODO: we dont actually want to inherit this!  Is this true for NavWorkspace?
                           // NavWorkspace no longer inherits from EditorWorkspace so maybe it's ok?

            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = true;
                }

            if (mQuickLookHostPanelInitialized == false)
            {
                //if (mQuickLookPanel == null)
                //{
                //    mQuickLookHostPanel = new KeyEdit.GUI.QuickLookHostPanel();

                //    mQuickLookPanel = new GUI.QuickLook3(AppMain.PluginService);
                //    this.mQuickLookHostPanel.Controls.Add(mQuickLookPanel);
                //    mQuickLookPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                //    mQuickLookPanel.BringToFront();
                //}
                //mQuickLookHostPanelInitialized = true;
                //((WorkspaceManager)mWorkspaceManager).DockControl(mQuickLookHostPanel, "Quick Look Host", "leftDockSiteBar", "Information", eDockSide.Left);
            }

            mDocument.Visible = true;
            mDocument.Enabled = true;
                        
			mIsActive = true;
        }

        public override void Hide()
        {
            // base.Hide();


            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = false;
                }

            mDocument.Visible = false;
            mDocument.Enabled = false;

            if (mQuickLookHostPanelInitialized == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Quick Look Host", "leftDockSiteBar");
                mQuickLookHostPanelInitialized = false;
            }

            mIsActive = false;
        }
        #endregion
    }
}
