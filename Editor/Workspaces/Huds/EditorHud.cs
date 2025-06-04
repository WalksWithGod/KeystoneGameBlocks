using System;
using System.Collections.Generic;
using System.Data.SQLite;
using KeyCommon.Flags;
using KeyCommon.Traversal;
using KeyEdit.Database;
using Keystone.Appearance;
using Keystone.Cameras;
using Keystone.EditTools;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Extensions;
using Keystone.Hud;
using Keystone.Resource;
using Keystone.TileMap;
using Keystone.Types;
using KeyEdit.Workspaces.Tools;

namespace KeyEdit.Workspaces.Huds
{

    /// <summary>
    /// A HUD can manage two types of elements.  It can manage menus and direction indicators
    /// and things which are designed to follow the camera and always be in exact same spot
    /// on screen.  And it can manage visual aids/tags for real 3d scene elements such as
    /// ships, worlds, including labels.
    /// I think it is ultimately best if picking and rendering of HUD elements is entirely managed
    /// by the HUD and not by the scene graph.  This way our 2d menu gui can implement its own
    /// picking system.
    /// TODO: Im thinking that our umbrella hud nodes should never be added to the real scene at all.
    /// this way they can never be rendered or picked without going thru hud.pick, hud.render, or moved
    /// without hud.update
    /// </summary>
    public class EditorHud : ClientHUD
    {

        #region Tool Preview Visuals Helper Classes
        private class WaypointPlacementPreview : TransformPreview
        {
            Material mWaypointPlacementMaterialOverride;

            public WaypointPlacementPreview(Keystone.EditTools.TransformTool tool, ModeledEntity sourceEntity,
                                     HUDAddElement_Immediate addImmediateHandler,
                                     HUDAddRemoveElement_Retained addRetainedHandler,
                                     HUDAddRemoveElement_Retained removeRetainedHandler) : base(tool, sourceEntity, addImmediateHandler, addRetainedHandler, removeRetainedHandler)
            {


                CreateWaypointPlacementOverrideMaterial();

                // iterate through all appearances and change their materials to use an alpha red material
                Material materialOverride = mWaypointPlacementMaterialOverride;
                if (materialOverride != null)
                {
                    SetHudPreviewMaterial(tool.Control, materialOverride);
                }
            }


            private void CreateWaypointPlacementOverrideMaterial()
            {
                // create material that should be used to override the material on the waypont placement sphere while tool is active
                mWaypointPlacementMaterialOverride = Keystone.Appearance.Material.Create(Material.DefaultMaterials.yellow_fullbright);
                // PlacementTool.DisposeUnmanagedResources DecrementRef's this
                Keystone.Resource.Repository.IncrementRef(mWaypointPlacementMaterialOverride);
            }

            public override void Preview(RenderingContext context, Keystone.Collision.PickResults pick)
            {
                base.Preview(context, pick);

                // NOTE: Interior uses derived type 'InteriorPlacementToolPreview' and not this one
                // mTool.Control is added in base class ctor as retained 3d element
                //mAddImmediateHandler(context.Scene.Root, mTool.Control);
            }

            protected override void DisposeManagedResources()
            {
                base.DisposeManagedResources();

                if (mWaypointPlacementMaterialOverride != null)
                    Keystone.Resource.Repository.DecrementRef(mWaypointPlacementMaterialOverride);
            }
        }
        #endregion

        private float mLastCameraScale;
        
        private Keystone.Controls.Control mVehicleMenu;

        private TileDataVisualizer[] mTileVisualizers;

        Material vehicleMaterial;
        Material hardpointMaterial;
        Material waypointMatrial;

        private Keystone.Controls.Control mPickPlane;
        private Keystone.Entities.ModeledEntity mPickMarker;
        private ModeledEntity mDestinationMarker;
        private bool mShowMotionField;
        private ModeledEntity mMotionField;

        public EditorHud() : base()
        {
            // LoadOrbitMesh - how do i know when/where to draw these orbit meshes?
            // LoadGridMesh (grid using TVMesh and LineList as opposed to 2dImmediate)
            
            LoadMaterials();
            LoadMotionField();
            //          LoadVehicleMenu();

            InitializePickParameters();
        }

        private void LoadMaterials()
        {
            waypointMatrial = Material.Create(Material.DefaultMaterials.yellow_fullbright);
            hardpointMaterial = Material.Create(Material.DefaultMaterials.yellow_fullbright);
            Color black = new Color(0, 0, 0, 255);
            // vehicleMaterial = Material.DefaultMaterials.red_fullbright;
            vehicleMaterial = Material.Create("tangerine", black, black, black, new Color(255, 204, 0, 255));
            vehicleMaterial.RefCount++;
        }


        #region IMapLayerObserver 		
        public override void NotifyOnDatabaseCreated(Keystone.TileMap.Structure structure, string layerName, int[,] data, int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ, int tileCountX, int tileCountZ, float width, float depth)
        {
            base.NotifyOnDatabaseCreated(structure, layerName, data, layerSubscriptX, layerSubscriptY, layerSubscriptZ, tileCountX, tileCountZ, width, depth);

            // TODO: temp hack, load one visualizer for 9 and don't bother trying to remap
            //       visualizers as we move the camera and zones page in/out
            //       - what do we do with visualizers for the various levels per structure?
            //      
            //if (structureID == Keystone.TileMap.Structure.GetStructureElementPrefix (layerSubscriptX, layerSubscriptY, layerSubscriptZ)) 
            if (layerName == "obstacles")
            {
                // this visualizer should not already exist... but when NotifyOnDatabaseChanged() how do we find correct
                // visualizer to apply them to?
                // we can compare TileDataVisualizer.ID with a flattened ID 

                // TODO: eventually we MUST recycle these rather than recreate them
                //       everytime we load new zones because we are running out of memory. 
                //       for now, we'll disable completely until i implement recycle scheme
                //            	TileDataVisualizer visualizer = new TileDataVisualizer(structure,
                //            	                                                       layerName,
                //            	                                                       data,
                //            	                                                       layerSubscriptX, 
                //            	                                                       layerSubscriptY, 
                //            	                                                       layerSubscriptZ, 
                //            	                                                       tileCountX, 
                //            	                                                       tileCountZ, 
                //            	                                                       width, 
                //            	                                                       depth);
                //            
                //            	mTileVisualizers = mTileVisualizers.ArrayAppend (visualizer); 
            }



            // TODO: where do we get the list of zones loaded? scene._pager exists
            //       ideally, everytime we connect hud to scene we'd get structures and
            //       then when structures were paged in/out the hud would be subscribed to paper
            //       and notified and we'd need thread safety between the interval of paging and
            //       subscribers being added/removed from list of subscribers.
            //       Seriously, I don't see how else we're going to track structures going in/out
            //       so the HUD can respond to changes.
            //       - or would it make sense to have MapLayerGrid.cs be notified? or for the HUD
            //       to be notified when MapLayer's are created?

            //           this.Context.Scene.Pager ;



            //AppMain._core.MapGrid    <-- MapGrid is declared scope "internal" here so how do i Attach()
            //                             hud to it?  well base class can do it.  And we have to be notified when
            //                             MapLayers are paged in, paged out OR when first attaching HUD as observer
            //                             we need to have that notify run for all existing layers that were created before
            //                             this hud attached

        }

        public override void NotifyOnDatabaseChanged(string layerName, int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ, int x, int z, int value)
        {
            base.NotifyOnDatabaseChanged(layerName, layerSubscriptX, layerSubscriptY, layerSubscriptZ, x, z, value);

            if (mTileVisualizers == null) return;

            // find the correct visualizer
            for (int i = 0; i < mTileVisualizers.Length; i++)
                if (mTileVisualizers[i].LayerSubscriptX == layerSubscriptX &&
                    mTileVisualizers[i].LayerSubscriptY == layerSubscriptY &&
                    mTileVisualizers[i].LayerSubscriptZ == layerSubscriptZ)
                {
                    mTileVisualizers[i].AddChange(x, z, value);
                    break;
                }
        }

        public override void NotifyOnDatabaseDestroyed(int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ)
        {
            if (mTileVisualizers == null) return;

            for (int i = 0; i < mTileVisualizers.Length; i++)
                if (mTileVisualizers[i].LayerSubscriptX == layerSubscriptX &&
                    mTileVisualizers[i].LayerSubscriptY == layerSubscriptY &&
                    mTileVisualizers[i].LayerSubscriptZ == layerSubscriptZ)
                {
                    mTileVisualizers[i].Dispose();
                    mTileVisualizers = mTileVisualizers.RemoveAt(i);
                    return;

                }
        }
        #endregion


        private void InitializePickParameters()
        {
            mPickParameters = new PickParameters[1];

            // KeyCommon.Flags.EntityFlags.ExteriorRegion
            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.None; // note: background is not excluded here as our Background allows pointsprite picking


            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes = EntityAttributes.None; //KeyCommon.Flags.EntityAttributes.Region;

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

            //p.ExcludedTypes = EntityFlags.AllEntityTypes & ~EntityFlags.HUD;

            mPickParameters[0] = pickParams;
        }

        public override void ContextualizeMenu(RenderingContext context, System.Drawing.Point vpRelativeMousePos, Keystone.Collision.PickResults pickResult)
        {
            base.ContextualizeMenu(context, vpRelativeMousePos, pickResult);

            if (pickResult.Entity == null)
            {
                // TODO: all hud items should only be queued for add/remove during Hud.Update()
                //       else we have all sorts of threading related bugs including tvmesh res loading errors
                RemoveHUDEntity_Retained(mVehicleMenu);
            }
            else if (pickResult.Entity is Keystone.Vehicles.Vehicle)
            {
                // TODO: this is a bit lame.  Instead, if the selected item has a ContextualMenu, then we should grab
                //       it from script.  
                if (context.Enabled)
                    ConfigureVehicleMenu(context, vpRelativeMousePos, (Keystone.Vehicles.Vehicle)pickResult.Entity);
            }
        }

        #region MenuButton
        private void PositionGUIMenu()
        {
            if (mVehicleMenu == null) return;

            // unlike AxisIndicator, this is a 2D GUI being drawn in 2d screenspace where top left is 0,0
            double width = Context.Viewport.Width;
            double height = Context.Viewport.Height;

            Vector3d v;

            v.x = width;
            v.y = height;
            v.z = 1.0d;

            // TODO: percentages should be passed in.  
            // note: we use percentages because our viewports width and heights can be so variable since we are windowed
            //       and place no restrictions on user resizing it except for minimum size of 800x600 i think.
            v.x *= .05d;
            v.y *= .98d;

            mVehicleMenu.Translation = v;
        }

        private void LoadVehicleMenu()
        {
            if (mVehicleMenu == null)
            {
                // F:\stuff\_KGB_PICS\gui buttons style.png <-- i like this style
                string menuTexture = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\menu_goto.png");
                // TODO: the problem here is, how can we get a root menu item to respond to cancel
                //       or what?  i mean below we have one seperate menu button its parent is the
                //       hud2d gui root... what if our mVehicle Menu was more of menu bar and hosting
                //       menu buttons?  how do we get the entire bar to respond to keyboard?  
                //       I think this is part of what a window'ing system should do.  Messages should always
                //       go from root gui item and forwarded down to relevant children as necessary and 
                //       per window settings.
                //       I think perhaps the root GUI should be similar to our InputController.CurrentTool
                //       where we can set the current InputController.CurrentGUI.  Perhaps the HUD itself _is_ that root GUI.
                //

                mVehicleMenu = LoadMenu2D("goto menu test", menuTexture, 64, 32,
                                          AddWaypointMenuButton_OnMouseEnter, AddWaypointMenuButton_OnMouseLeave,
                                          AddWaypointMenuButton_OnMouseDown, AddWaypointMenuButton_OnMouseUp,
                                          AddWaypointMenuButton_OnMouseClick);

                // increment Menu so that we can render it retained and remove it from retained
                // without it ever falling out of scope and being removed from Repository
                Repository.IncrementRef(mVehicleMenu);
            }
        }


        // TODO: is this menu on just the current viewport if more than one?
        // normally its hud.cs that has the show/hide hud control.  This is why
        // i was going to use the "NotifyHuds" method so that the Hud itself can
        // determine whether that viewport should contain a menu.


        // TODO: the placement of our menu can take the form of a toolbar style set of
        //       menus too.  like in MS Golf 98. 
        //       http://www.youtube.com/watch?v=Krl1tG4WeTk
        //       This way they do not block the
        //       screen.  And positioning the menus is now easy. in fact child menus now
        //       especially are easy to position.
        // TODO: where would we initiate a GUI that is defined by script?  so for instance
        //       when you click on a Vehicle and if it's your vehicle, a particular series of menus
        //       appears?  Maybe we can define those in the HUD but then in script, refer to them
        //       using names?

        // load up the menu for Vehicles.
        // - this menu can be complex though as might be the logic
        //   for determining what parts of the menu are visible
        // - orbit menu might only appear when in orbit around a planet already
        //   - change orbit elevation sub-menu for instance
        // VEHICLE MENU
        // Helm
        //  - Set Target Velocity (ship will accelerate or turn around and decelerate if necessary)
        //  - Add Waypoint
        //      - cursor is changed to waypoint "target", grid appears and elevation
        //        line is drawn to grid
        //      - if target is a planet or star system, maybe popup window a modal 3d viewport
        //        of nav map style of the star system and now allow more precise waypoint to
        //        that system.
        //  - (if orbiting)
        //      - Orbit
        //          - raise orbit
        //          - lower orbit
        //
        // TODO: this could be a celestial interactions menu
        // - scan -> bio / minerals / emissions
        // - goto -> geo orbit, low, medium, high, edge
        // - contact -> target, follow, scan, etc

        // - Interactions menus i suppose can result in crew doing what it takes
        //   to fullfil the order including starting engines, or enabling a scanner, etc
        // - interaction menus can also exist for captain to manually select an engine to turn it on
        //   and set output levels for example.  or you can select a destination and how fast
        //   you'd like to get there and can show your fuel burn or energy useage, and then
        //   select the option to just go there.   You can string gotos to form a list of orders
        //   and these can be displayed as waypoints.
        //   Waypoints can also be uploaded into the Helm for pilot or autopilot to utilize.
        //   - a listview style grid list can show the list of navs similar to a VS.nET Task List
        //   - when in nav, another list can show all nearby contacts which you can face camera to,
        //     nav to, follow, etc



        private void ConfigureVehicleMenu(RenderingContext context, System.Drawing.Point vpRelativeMousePos, Keystone.Vehicles.Vehicle vehicle)
        {
            if (context == null) return; // treeview selection instead of viewport entity selection will cause this to be null
            System.Diagnostics.Trace.WriteLine("EditorHud.ConfigureVehicleMenu - " + vehicle.ID);


            // TODO: here, we must instruct the workspace to essentially put up a dialog
            // or menu which results in a different type of input controller right?

            // TODO: suspending controls, switching control schemes... these are related right?
            // especially if that change can be done through clicking of hud icons at runtime

            // TODO: if one context has a menu and another does not, i think they should all be suspended
            // for the simple fact that a state might not longer be valid in menu selection if
            // it's allowed to persist whilst user takes other actions in other contexts.
            //
            // TODO: alternative is, that menu is auto closed if user clicks off of it and
            // there is no "click" rollover animation feedback provided.

            // TODO: so we show a menu, we switch to a gui controller in workspace, saving the previous
            // we could maybe even use a stack 
            // What is difference between a tool with input capture and a control?
            // especially if both implement IInputCapture

            // - if no interactions menu, show selected entity's interactions menu
            // - if menu, close and show the current interactions possible
            // - if esc, close any current menu (eg remove it from hud)

            // - selection of another interactable object with menu, will show that interaction menu
            // - there is no right mouse click to activate menus
            // - by making it left mouse click, the interactions are automatic but also
            //   easy to close, but also dont block rest of GUI

            // - so should interactions menu be per HUD or per Workspace?  I think either is acceptable
            //   but for placement purposes, probably per HUD where of course each HUD is tied
            //   to a context.  

            // finds a position to show it so that it's fully onscreen if possible
            // this call perhaps automatically adds this control to scene.HudUmbrella branch
            // and disables picking of rest of scene
            //       // LoadMenu here should be for 2D, but our Menu is actually using a Control ModeledEntity
            // that has a TexturedQuad geometry under Model so is handled just like any 3d element under the hood
            // AddHUDEntity_Immediate(context.Scene.Root, mVehicleMenu);
            // Retained for menu is what we need to have abiltiy to mouse pick the menu
            AddHudEntity_Retained(mVehicleMenu);


        }

        #endregion 

        
        #region AddWaypointMenuButton Events
        protected virtual void AddWaypointMenuButton_OnMouseEnter(object sender, EventArgs args)
        {
            // set the transform/axis mode

            // TODO: here we might do something like change the material color of the control to it's RollOver state color
            System.Diagnostics.Trace.WriteLine("EditorHud.AddWaypointMenuButton_OnMouseEnter - " + ((Keystone.Controls.Control)sender).ID);

            // change the material to roll over
        }

        protected virtual void AddWaypointMenuButton_OnMouseLeave(object sender, EventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("EditorHud.AddWaypointMenuButton_OnMouseLeave - " + ((Keystone.Controls.Control)sender).ID);

            // change the material's back to default
        }


        protected virtual void AddWaypointMenuButton_OnMouseDown(object sender, EventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("EditorHud.AddWaypointMenuButton_OnMouseDown - " + ((Keystone.Controls.Control)sender).ID);
        }

        protected virtual void AddWaypointMenuButton_OnMouseUp(object sender, EventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("EditorHud.AddWaypointMenuButton_OnMouseUp - " + ((Keystone.Controls.Control)sender).ID);

            Keystone.Events.InputCaptureEventArgs mouse = (Keystone.Events.InputCaptureEventArgs)args;
            RenderingContext context = mouse.MouseEventArgs.Viewport.Context;
            Entity vehicle = context.Workspace.SelectedEntity.Entity;

            // - lets think in terms of clicking on a ship, bringing up a menu
            //   how do we tie in a menu to the clicking of the ship?
            //   - normally we create the proxy icon which can then directly have the menus
            //     placed when wiring the mouse down event.  
            //     - but with a ship, we have to respond to a "Selected" event or something...
            //       and then place menu options.
            //     - but are these NOT scripted?  
            //     - can we tie in to a menu by "name"?  This way we can define the menus for hte
            //       different types once and then select them from within the HUD based on the type
            //       of entity clicked?
            //       - i think in the short term this is probably sufficient.  Let's try to tie in a
            //       pre-established menu based on the entity selected and the selection tool state.
            //

            // TODO: the vehicle.ID should be tied to the workspace
            // this is some nagging issue i need to resolve with Editor workspace
            // versus a more "game" like runtime workspace.  
            // Normally to set a waypoint, your vehicle is assumed at all times
            // and interacting is always YOU interacting with a SELECTION
            // but in this case, we have no assumed vehicle.  
            //  - If we click the vehicle itself, to bring up an interaction menu, perhaps then
            // our "goto" menu which pops up brings up a waypoint placer.  This perhaps is more
            // appropriate for an Editor hud.  Then our waypoint placer can show a "Target" cross hair
            // that can lock onto planets but also show a grid and height line and on first click
            // fixes x,z and then second click fixes y.
            // - we have to be able to change the tool used by the Editor
            //   Here i think the Workspace should have the Tool.  It seems to me that
            //   there's no good reason for the Scene to have this.
            System.Diagnostics.Debug.Assert(vehicle is Keystone.Vehicles.Vehicle);

            // we can keep the IOController, and we can force the current tool
            // to give input capture to the menu.  
            // or can we set a flag on the context.  to skip pick traversal down
            // the rest of the scene for that context.  And to accomplish that
            // we could set context.Modal = mHudUmbrella  perhaps and then
            // if context.Modal != null, 
            // TODO: also, it still makes me wonder about not simply having umbrella root
            // it's own root, instead of child of scene.root.  It would make it easier to choose
            // one or the other.
            // but i still end up with the damn interpreter calling right mouse look 
            // and perhaps even during textbox input when what i want is to suspend the interpreter
            // and be in a modal default IOController scheme...


            context.Workspace.CurrentTool =
                new KeyEdit.Workspaces.Tools.NavPointPlacer(AppMain.mNetClient);

            // TODO: we shouldn't be removing it but rather showing depressed state with rest of
            //       toolbar inactive since the waypoint placer is active.
            // remove it from rendering, but it will not unload because
            // we've incremented it's refcount +1 on creation
            RemoveHUDEntity_Retained(mVehicleMenu);
        }

        protected virtual void AddWaypointMenuButton_OnMouseClick(object sender, EventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("EditorHud.AddWaypointMenuButton_OnMouseClick - " + ((Keystone.Controls.Control)sender).ID);
        }
        #endregion


        // This handles picking of HUD 2D and 3D Controls and Entities
        public override Keystone.Collision.PickResults Pick(RenderingContext context, Ray regionSpaceRay, PickParameters parameters)
        {
            Keystone.Collision.PickResults results = base.Pick(context, regionSpaceRay, parameters);

            return results;
        }

        private bool mViewpointOrbitBehaviorInitialized = false;
        private void ConfigureChaseViewpoint(Viewpoint viewpoint)
        {
            viewpoint.RemoveChild (viewpoint.Behavior );
            viewpoint.AddChild(EditorWorkspace.CreateIsometricBehavior());

            // todo: the flyto is positioning us between the earth and vehicle and its looking the opposite way
            // todo: if the focus_entity_id is moving, the flyto animation only targets the initial position 

            // todo: i need an event or callback to occur when the flyto animaation is finished so that we can switch to chase/orbit
            viewpoint.BlackboardData.SetString("control", "user");
            viewpoint.BlackboardData.SetString("goal", "goal_none");
            
            viewpoint.BlackboardData.SetBool("chase_enabled", true);
            viewpoint.BlackboardData.SetString("focus_entity_id", AppMain.mPlayerControlledEntity.ID);
            viewpoint.BlackboardData.SetBool("animation_playing", false);

            // todo: after flyto, assign a chase viewpoint behavior, but how do we know when we've arrived? 
            viewpoint.BlackboardData.SetString("behavior", "behavior_orbit");

            double radius = 10000d; // this.Context.GetZoomToFitDistance (AppMain.mLocalVehicle.BoundingBox.Radius);
            viewpoint.BlackboardData.SetDouble("orbit_radius", radius);
            viewpoint.BlackboardData.SetDouble("orbit_radius_min", 100d);
            viewpoint.BlackboardData.SetDouble("orbit_radius_max", 10000d);

            viewpoint.BlackboardData.SetDouble("orbit_radius_destination", radius); // used to lerp to destination.  initialize it with same radius value as "orbit_radius"
            viewpoint.BlackboardData.SetDouble("max_altitude", 10000d);
            viewpoint.BlackboardData.SetDouble("min_altitude", 100d);

            viewpoint.BlackboardData.SetVector("offset", new Keystone.Types.Vector3d()); // offset must be constrained within min_bounds and max_bounds
            viewpoint.BlackboardData.SetVector("min_bounds", new Keystone.Types.Vector3d());
            viewpoint.BlackboardData.SetVector("max_bounds", new Keystone.Types.Vector3d());

            Vector3d startingMouseAngles = new Vector3d(30, 0, 0);
            viewpoint.BlackboardData.SetVector("cam_mouse_angles", startingMouseAngles);
            viewpoint.BlackboardData.SetDouble("min_vertical_angle", -90d);
            viewpoint.BlackboardData.SetDouble("max_vertical_angle", 90d);

            viewpoint.BlackboardData.SetBool("smooth_zoom_enabled", true);
            viewpoint.BlackboardData.SetDouble("smooth_zoom_time", 1d); // seconds
            viewpoint.BlackboardData.SetDouble("smooth_zoom_elapsed", 0d);

            mViewpointOrbitBehaviorInitialized = true;
        }


        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            if (AppMain.mPlayerControlledEntity != null && context.Viewpoint != null && AppMain._core.SceneManager.Scenes[0].Simulation.CurrentMission != null && AppMain._core.SceneManager.Scenes[0].Simulation.CurrentMission.Enable)
            {
                if (!mViewpointOrbitBehaviorInitialized)
                    ConfigureChaseViewpoint(context.Viewpoint);

                if (context.Viewpoint.BlackboardData.GetBool("animation_playing")) return;

                LoadPickPlane();
                LoadPickMarkerAndDestinationMarkers();

                mPickMarker.Enable = true;
                mDestinationMarker.Enable = true;
                mPickPlane.Enable = true;

                UpdatePickPlanePosition();
                UpdatePickPMarkerPosition();

                Grid.RowSpacing = 10000f;
                Grid.ColumnSpacing = 10000f;
                Grid.InfiniteGrid = true;
            }
            else
            {
                if (mPickMarker != null) mPickMarker.Enable = false;
                if (mDestinationMarker != null) mDestinationMarker.Enable = false;
                if (mPickPlane != null) mPickPlane.Enable = false;
            }
            base.UpdateBeforeCull(context, elapsedSeconds);
            // it can take a bit for the context to get connected to the region and then for that region
            // to get connected to the scene.  im not sure why... TODO: investigate some other time if this is safe and not indicative of some other fundamental issue
            if (context.Region == null || context.Region.RegionNode == null) return;

            Keystone.Collision.PickResults pickResult = context.Workspace.MouseOverItem;
            Keystone.EditTools.Tool currentTool = context.Workspace.CurrentTool;


            // tool previews
            if (mLastTool != currentTool)
            {
                OnEditTool_ToolChanged(currentTool);
                mLastTool = currentTool;
            }


            IHUDToolPreview currentPreviewGraphic = null;

            if (currentTool as TransformTool != null)
            {
                TransformTool transformTool =
                        (TransformTool)currentTool;


                if (transformTool as AssetPlacementTool != null)
                {
                    if (mPreviewGraphic as PlacementPreview == null)
                    {
                        if (((AssetPlacementTool)transformTool).ShowPreview == false)
                        {
                            currentPreviewGraphic = new NullPreview();
                        }
                        else
                        {
                            // NOTE: it is ok to clone tool.Source for large vehicle prefabs without running
                            // out of memory because the AssetPlacementTool loads a preview into Source that 
                            // does not contain the Interior.  This is fine because on mouse down, we send
                            // a Prefab_Load command with the relativePrefabPath so the entire Vehicle 
                            // (or any other prefab) gets loaded into the scene
                            ModeledEntity clone;
                            string cloneID = Repository.GetNewName(currentTool.Source.TypeName);
                            clone = (ModeledEntity)currentTool.Source.Clone(cloneID, true, false);
                            
                            currentPreviewGraphic = new PlacementPreview(transformTool, clone,
                                                                      AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                        }
                    }
                }
                else
                {
                    if (mPreviewGraphic is TransformPreview == false && transformTool.Source != null)
                    {
                        currentPreviewGraphic = new TransformPreview(transformTool, (ModeledEntity)transformTool.Source,
                                                              AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                        // when first initiating the transform preview for Move/Rotate/Scaling tools, we must init ComponentTranslation to Source.Translation
                        // or else the Source entity will snap to origin.
                        transformTool.ComponentTranslation = transformTool.Source.Translation;
                    }
                }

                // NOTE: once the placement tool actually drops the prefab on a target Entity, it uses that target as Parent and pickResult.ImpactPointLocalSpace
                if (pickResult != null && pickResult.Entity != null && !(pickResult.Entity is Keystone.Portals.Region))
                    transformTool.ComponentTranslation = pickResult.ImpactPointRelativeToRayOriginRegion;
            }
            else
            {

                currentPreviewGraphic = new NullPreview();
                context.PickParameters = mPickParameters[0];
            }

            if (mPreviewGraphic != currentPreviewGraphic && currentPreviewGraphic != null)
            {
                // dispose existing and assign new if the new one is NOT null, otherwise no change has occurred
                if (mPreviewGraphic != null)
                {
                    mPreviewGraphic.Clear();
                    mPreviewGraphic.Dispose();
                }

                mPreviewGraphic = currentPreviewGraphic;
            }


            if (mPreviewGraphic != null)
                mPreviewGraphic.Clear();

            if (mPreviewGraphic != null)
                mPreviewGraphic.Preview(context, pickResult);


            if (mPlacementToolGraphic != null)
            {
                // yardstick or cell floor marker (i think we should make more of an outline type marker for tile\cell floor)
                if (pickResult.Entity as Structure != null)
                {
                    Structure structure = (Structure)pickResult.Entity;
                    PositionMarker(structure, currentTool, pickResult, mPlacementToolGraphic);
                    AddHUDEntity_Immediate(structure.Region, mPlacementToolGraphic, false);
                }
            }

            // TODO: no more scaling of root... that is causing too many problems.
            //       -instead, the "cam_zoom" is a zoom for a specific branch... say our 3dHUDRoot only
            //       or perhaps a specific parent node for just the starmap?
            float cameraZoom = 1f;
            if (context.Viewpoint.BlackboardData != null)
                cameraZoom = context.Viewpoint.BlackboardData.GetFloat("cam_zoom");

            float scale = 1f / cameraZoom;


            // TODO: orbit using quaternions... but also using delta mouse so our viewpoint behavior 
            // TODO: then with orbit, use mouse wheel as a type of zoom that manipulates distance to target with min/max values for that zoom level
            // TODO: can we use translation delta for grid?
            if (this.Grid.Enable)
            {
                // NOTE: Grid 2d drawing occurs in UpdateAfterClear();
                UpdateGridZoom(cameraZoom);
            }

            UpdateProxies(context);

            // our test "goto" menu
            PositionGUIMenu();

            if ((bool)context.GetCustomOptionValue(null, "show motion field"))
                UpdateMotionField(context.Viewpoint, context.Position, context.LookAt, elapsedSeconds);

            mLastCameraScale = scale;
        }

        public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateAfterCull(context, elapsedSeconds);

            // TODO: can we use translation delta for grid instead of regenerating the lines each frame?
            if (this.Grid.Enable && context.Viewpoint.Region != null)
            {
                // todo: GetRegionRelativeCameraPosition() works far from origin but needs to be tested with multiple Zone game worlds   
                Vector3d gridoffset = -context.GetRegionRelativeCameraPosition(context.Viewpoint.Region.ID); // -context.Position
                gridoffset.y = -context.Position.y;

                Keystone.Immediate_2D.Renderable3DLines[] gridLines =
                    Grid.Update(gridoffset, (bool)context.GetCustomOptionValue(null, "show axis indicator"));

                // the gridlines passed in area already in cameraspace coordinates
                AddHUDEntity_Immediate(context.Viewpoint.Region, gridLines);
            }


            
            if ((bool)context.GetCustomOptionValue(null, "show pathing debug information"))
            {
                // Zone Connectivity Area debug // we are passing in zone at (0,0,0) so we know
                // enough were we can compute the region offset 
                Node[] nodes = context.Scene.Root.Children;
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i] is Keystone.Portals.Zone)
                        {
                            Keystone.Portals.Zone zone = (Keystone.Portals.Zone)nodes[i];
                            BoundingBox[] boxes = context.Scene.PathGetConnectivityAreaBounds(zone.ArraySubscript[0], zone.ArraySubscript[1], zone.ArraySubscript[2]);
                            if (boxes != null)
                                for (int j = 0; j < boxes.Length; j++)
                                {
                                    // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                                    // so we have to convert those box coords to region space
                                    boxes[j].Translate(-zone.BoundingBox.Width / 2.0, 0, -zone.BoundingBox.Depth / 2.0);
                                    // to camera space translation.. 
                                    boxes[j].Translate(-context.GetRegionRelativeCameraPosition(zone.ID));
                                    // by parenting to the relevant zone, these 2d Immediate primitvies
                                    // will be rendered in their Region space.
                                    AddHUDEntity_Immediate(zone, boxes[j], Keystone.Types.Color.Green.ToInt32());
                                }

                            // portals
                            boxes = context.Scene.PathGetAreaPortals(zone.ArraySubscript[0], zone.ArraySubscript[1], zone.ArraySubscript[2]);
                            if (boxes != null)
                                for (int j = 0; j < boxes.Length; j++)
                                {
                                    // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                                    // so we have to convert those box coords to region space
                                    boxes[j].Translate(-zone.BoundingBox.Width / 2.0, 0, -zone.BoundingBox.Depth / 2.0);
                                    // to camera space translation
                                    boxes[j].Translate(-context.GetRegionRelativeCameraPosition(zone.ID));
                                    // by parenting to the relevant zone, these 2d Immediate primitvies
                                    // will be rendered in their Region space.
                                    AddHUDEntity_Immediate(zone, boxes[j], Keystone.Types.Color.Red.ToInt32());
                                }

                        }
                    }
                }
            }


            // render vehicle velocity. Only default vehicle will render velocity so in scene explorer treeview select "Set as default vehicle
            int fontHeight = 10;
            int textLeft = this.Context.Viewport.Width / 2;
            int textTop = this.Context.Viewport.Height - fontHeight - 5;

            if (AppMain.mPlayerControlledEntity != null)
                this.Context.RenderDebugText("Velocity: " + GetVelocityLabel(AppMain.mPlayerControlledEntity.Velocity), textLeft, textTop, this.Context.FontColor.ToInt32());
        }


        private void UpdateTacticalHud(RenderingContext context)
        {
            Predicate<Node> match;

            match = e =>
            {
                if (e as Keystone.Vehicles.Vehicle != null && e.ID == AppMain.mPlayerControlledEntityID)
                    return true;
                return false;
            };

            Keystone.Vehicles.Vehicle vehicle = null;
            if (AppMain.mPlayerControlledEntity is Keystone.Vehicles.Vehicle)
                vehicle = (Keystone.Vehicles.Vehicle)AppMain.mPlayerControlledEntity;


            if (vehicle == null || vehicle.GetEntityFlagValue("destroyed")) return;

            // todo: draw the orbit waypoints if applicable
            match = e =>
            {
                if (e.Name == "helm")
                    return true;
                return false;
            };
            Entity helm = (Entity)vehicle.Interior.FindDescendant(match);
            if (helm != null)
            {
                //Vector3d[] waypoints = (Vector3d[])helm.GetCustomPropertyValue("waypoints");
                //if (waypoints != null)
                //{
                //    string texturePath = System.IO.Path.Combine(AppMain._core.DataPath, @"editor\navpoint.png");

                //    if (waypointEntities == null)
                //    {
                //        waypointEntities = new ModeledEntity[waypoints.Length];

                //        for (int i = 0; i < waypoints.Length; i++)
                //        {

                //            string id = Repository.GetNewName(typeof(Keystone.Entities.ModeledEntity));
                //            waypointEntities[i] = (ModeledEntity)Repository.Create(id, "ModeledEntity");
                //            waypointEntities[i].SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
                //            waypointEntities[i].ScreenSpaceSize = 0.025f;
                //            waypointEntities[i].UseFixedScreenSpaceSize = true;
                //            // TODO: perhaps we can tie the vehicle ID to the name so we can reference it?
                //            // We need a way for the NavPointPlacer to know that this waypoint HUD element
                //            // is tied to which vehicle.
                //            waypointEntities[i].Name = "#WAYPOINT " + i;


                //            // NOROTATION == custom rotation which in this case is to give us Y axis rotation like our stars 	
                //            Model waypointIcon = KeyEdit.ContentCreation.Helper.Load3DBillboardIcon(texturePath, 1f, 1f, MTV3D65.CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION);

                //            waypointEntities[i].AddChild(waypointIcon);

                //            waypointEntities[i].Translation = waypoints[i]; // - context.GetRegionRelativeCameraPosition(vehicle.Region.ID);
                //            // AddHUDEntity_Immediate(vehicle.Region, waypointEntity, true);

                //        }
                //    }
                //    IconizeNonRecursive(context, waypointEntities, null,
                //    texturePath, waypointMatrial, false,
                //    null, null,
                //    null, null,
                //    null);
                //}
                
            }

            // todo: transponder radio signal sensor should also exist to identify friendly routine cargo ships and other types of friendlies
            
            
            match = e =>
            {
                if (e.Name == "tactical")
                    return true;
                return false;
            };

            // todo: why does AppMain.SceneManager.Scenes[0].Query() have 0 children assigned to it? Maybe I should start at AppMain.SceneManager.Scenes[0].Root or i need to make sure Root is returned as a child during Scene.Query
            Node[] matches = vehicle.Interior.Query(true, match);
            if (matches == null) return;

            System.Diagnostics.Debug.Assert(matches.Length == 1); // todo: or can we have multiple tactical stations using same name?
            Entity tacticalStation = (Entity)matches[0];

            string[] contacts = (string[])tacticalStation.GetCustomPropertyValue("contacts");
            if (contacts == null) return;

            Entity[] entityContacts = new Entity[contacts.Length];
            for (int i = 0; i < contacts.Length; i++)
            {
                entityContacts[i] = (Entity)Repository.Get(contacts[i]);
            }

            
            string texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\hud_uknown_vessel_type.png");
               
            IconizeNonRecursive(context, entityContacts, null,
                texturepath, hardpointMaterial, false,
                null, null,
                null, null,
                null);

        }

        #region Position and Update HUD Elements
        private void PositionMarker(Structure celledRegion, Tool tool, Keystone.Collision.PickResults pickResult, ModeledEntity marker)
        {
        	if (pickResult == null) return; 

            Vector3d translation = Vector3d.Zero();

            // compute position of hud entities
            if ((tool is SelectionTool || tool is TileSegmentPainter) && pickResult.FacePoints != null)
            	// mouse over marker is current marker
            	// TODO: for painting tiles, our texture should be different so we know what mode we're in
                translation = (pickResult.FacePoints[0] + pickResult.FacePoints[2]) * .5f;
            else if (tool is AssetPlacementTool & pickResult.FacePoints != null)
            {
            	// yardstick is current marker
            	// we are assuming that the picked entity is our GRID entity otherwise .TileVertexIndex is irrelevant
            	//System.Diagnostics.Debug.Assert (pickResult is Celle
            	
                translation = pickResult.FacePoints[pickResult.TileVertexIndex];
            	
                // TODO: the first time we implemented walls we placed them on edges and allowed
                // for the yardstick marker to be placed on the closest corner vertex of a tile.
                // Now we are implementing walls that are placed on tile and we want the marker
                // to be placed in the center of the closest edge, but depending which axis the user
                // drags, we need to be able to switch to appropriate edge's center.
              	// TODO: i need a line graphic to be placed or the preview of the wall
              	//       a single 2D line should be sufficient for now.
              	Vector3d center = pickResult.ImpactPointLocalSpace;
              	translation = center;
            }

            const float Z_FIGHTING_EPSILON = 0.01f;
            translation.y += Z_FIGHTING_EPSILON * 16;
            if (marker != null)
            {
                marker.Translation = translation;
            }
        }
        #endregion
        
        #region Tool Events
        protected override void OnEditTool_ToolChanged(Tool currentTool)
        {
            if (currentTool == null || currentTool is SelectionTool)
            {
            	mPlacementToolGraphic = null; // TODO LoadMouseTileMarker();
            }

            // TODO: all of the following should be moved into ToolPreview's
            // change marker based on the tool in use and it's brush style.
            // the one difference though is these "preview" items (eg the yardstick)
            // use AddHUDEntity_Immediate for rendering
            if (currentTool is PlacementTool)
            {
                mPlacementToolGraphic = null; // use the mLastSourceEntry for preview mesh/actor
                switch (((PlacementTool)currentTool).BrushStyle)
                {
                	case 0:
                		break;
                	case 1:
                		break;
                	case 2:
                		break;
                	case 3:
                		mPlacementToolGraphic = LoadYardStick();
                		break;
                }
            }
            else if (currentTool is WallSegmentPainter)
            {
            	// TODO: this could be done as part of wallsegmentpainterpreview
                mPlacementToolGraphic = LoadYardStick();
            }
            else if (currentTool is TileSegmentPainter)
            {
                //LoadTileMarker(celledRegion);
                //mLastMarker = mTileMarker;
                mPlacementToolGraphic = null; // TODO LoadMouseTileMarker(); // for now let's not use the big orange tile marker
            }
        }
        #endregion


        private void UpdateProxies(RenderingContext context)
        {
            Predicate<Entity> match;
            List<Entity> matchResults;

            string texturepath = null;
            
            if ((bool)context.GetCustomOptionValue (null, "show hardpoint icons"))
            {
	            // TEMP: render icons for hardpoints
	            // TODO: for this to temp test to work, I think my vehicle must be at origin
	            // since ultimately we wont be computing exterior relative to vehicle positions for the interior
	            // hardpoint components.  But we do want to be able to see them.  
	            match = e =>
	            {
	                if (e.Script == null)
	                {
	                    return false;
	                }
	                return true;
	            };
	

	            texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\hud_engine_hardpoint1.png");
	            texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\hud_uknown_vessel_type.png");
	            matchResults = context.Region.RegionNode.Query(false, match);
	            
	            IconizeNonRecursive(context, matchResults.ToArray(), match,
	                texturepath, hardpointMaterial, false,
	                Proxy_OnMouseEnter, Proxy_OnMouseLeave,
	                Proxy_OnMouseDown, Proxy_OnMouseUp,
	                Proxy_OnMouseClick);
            }

            // todo: if ArcadeEnabled this should only render objects that can be detected via sensors
            // todo: in Simulation, production should not occur unless AracadeEnabled? Well, we want to be able to test the design without having to load the arcade campaign each time
           // if (AppMain._core.ArcadeEnabled)
           // {
                UpdateTacticalHud(context);
            //}
            if ((bool)context.GetCustomOptionValue(null, "show vehicle icons"))
            {
                // render vehicle icons
                match = e =>
	            {
	                if (e as Keystone.Vehicles.Vehicle != null)
	                    return true;
	                return false;
	            };
	
	            // enable hud icons for Vehicles that are too far to see with naked eye
	            texturepath = System.IO.Path.Combine (AppMain.DATA_PATH,  @"editor\hud_uknown_vessel_type.png");
	            texturepath = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\hud_fighter_vessel5.png");
	            matchResults = context.Region.RegionNode.Query(false, match);
	            	
                // todo: this method should have passed in VehicleProxy_Events not BodyProxy            
	            IconizeNonRecursive(context, matchResults.ToArray(), match,
	                texturepath, vehicleMaterial, false,
	                Proxy_OnMouseEnter, Proxy_OnMouseLeave,
	                Proxy_OnMouseDown, Proxy_OnMouseUp,
	                Proxy_OnMouseClick);
            }
            
            if ((bool)context.GetCustomOptionValue (null, "show celestial icons"))
            {
	            // only render child Celestial Body icons based on distance threshold
	            // so the labels arent all jumbled up 
	            match = e =>
	            {
	                if (e as Keystone.Celestial.Body != null)
	                    return true;
	                return false;
	            };
	
	            // enable hud icons for worlds that are too far to see with naked eye (note: RECURSIVE param = true)
	            texturepath = System.IO.Path.Combine (AppMain.DATA_PATH,  @"editor\hud_world_1.png");
	            matchResults = context.Region.RegionNode.Query(false, match);
	            // TODO: confused here - if above line is using context.Region for root, how is it we're getting all zones and not just the zone we're in?
	            //                      WE'RE NOT!  The rest are showing up as pointsprites in our digest.  We only start to show the proxy icons for the zone we're in.
	            

	        	Material worldMaterial = Material.Create (Material.DefaultMaterials.green_fullbright);
            
	        	IconizeNonRecursive(context, matchResults.ToArray(), match,
	                texturepath, worldMaterial, true,
	                Proxy_OnMouseEnter, Proxy_OnMouseLeave,
	                Proxy_OnMouseDown, Proxy_OnMouseUp,
	                Proxy_OnMouseClick);
            }
        }
        


        public override void Render(RenderingContext context, List<RegionPVS> regionPVSList)
        {
            base.Render(context, regionPVSList);
        }


        private Vector3d mLastCameraPosition = Vector3d.Zero();
        // TODO: update of this entity should occur via script
        private void UpdateMotionField(Entity parent, Vector3d cameraPosition, Vector3d cameraLookAt, double elapsedSeconds)
        {
            if (AppMain._core.ArcadeEnabled && mShowMotionField && mMotionField != null && mMotionField.Model != null && mMotionField.Model.Geometry != null)
            {
                Vector3d forward = AppMain.mPlayerControlledEntity.RegionMatrix.Backward; // cameraLookAt; // todo: this should pass in Entities velocity
                Keystone.Celestial.ProceduralHelper.MoveMotionField(mMotionField,
                    (Mesh3d)mMotionField.Model.Geometry,
                    cameraPosition, mLastCameraPosition, forward, elapsedSeconds);

                mLastCameraPosition = cameraPosition;
            }
        }
        private void LoadPickMarkerAndDestinationMarkers()
        {
            if (mPickMarker == null)
            {
                mPickMarker = (ModeledEntity)Keystone.Resource.Repository.Create("ModeledEntity");
                mPickMarker.Dynamic = false;
                mPickMarker.Pickable = false;
                mPickMarker.Enable = true;
                mPickMarker.Overlay = true; // i think this should be false, overlay is for things that should be rendered over everything else and in this case, we still want items placed on the floorplan to be ontop of these markers
                mPickMarker.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mPickMarker.Serializable = false;
                //mPickMarker.ScreenSpaceSize = .25f;
                //mPickMarker.UseFixedScreenSpaceSize = true;
                Vector3d scale = new Vector3d(100, 1, 100);
                mPickMarker.Scale = scale;
                // todo: mPickMarker tracks the mouse, but we also want a destination marker after the user has right mouse clicked on a location
                // todo: do we want a textured marker or a circle primitive?
                Model pickMarkerModel = Keystone.Celestial.ProceduralHelper.CreateSmoothCircle(360, Keystone.Types.Color.Yellow, false, false);

                mPickMarker.AddChild(pickMarkerModel);



                // destination marker is essentially the same for now
                mDestinationMarker = (ModeledEntity)Keystone.Resource.Repository.Create("ModeledEntity");
                mDestinationMarker.Dynamic = false;
                mDestinationMarker.Pickable = false;
                mDestinationMarker.Enable = true;
                mDestinationMarker.Visible = true; // NOTE: not sure why but visible = true is required or it wont be visible
                mDestinationMarker.Overlay = true; // i think this should be false, overlay is for things that should be rendered over everything else and in this case, we still want items placed on the floorplan to be ontop of these markers
                mDestinationMarker.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mDestinationMarker.Serializable = false;
                //mDestinationMarker.ScreenSpaceSize = .25f;
                //mDestinationMarker.UseFixedScreenSpaceSize = true;

                mDestinationMarker.Scale = scale;
                // todo: mPickMarker tracks the mouse, but we also want a destination marker after the user has right mouse clicked on a location
                // todo: do we want a textured marker or a circle primitive?
                Model destMarkerModel = Keystone.Celestial.ProceduralHelper.CreateSmoothCircle(360, Keystone.Types.Color.Red, false, false);

                mDestinationMarker.AddChild(destMarkerModel);

            }
        }

        private void LoadPickPlane()
        {

            if (mPickPlane == null)
            {
                float size = 100000f;
                Mesh3d pickPlaneMesh = Mesh3d.CreateFloor(size, size, 1, 1);
                pickPlaneMesh.Name = "pickplanemesh";

                Model model = (Model)Repository.Create("Model");
                // NOTE: there is no appearance node because the Entity is intended to be invisible
                model.AddChild(pickPlaneMesh);

                // add an appearance that makes the plane completely invisible. This requires we set the lighting mode to NORMAL or MANAGED
                DefaultAppearance appearance = (DefaultAppearance)Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null, true);
                appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
                appearance.Material.Opacity = 0.0f;

                model.AddChild(appearance);

                mPickPlane = LoadControl3D("pickplane", model, null, null, PickPlane_OnMouseDown, null, PickPlane_OnMouseClick);
                AddHudEntity_Retained(mPickPlane);

            }
        }

        private void UpdatePickPMarkerPosition()
        {
            if (mPickMarker == null) return;

            Keystone.Collision.PickResults pickResult = this.Context.Workspace.MouseOverItem;
            Keystone.EditTools.Tool currentTool = this.Context.Workspace.CurrentTool;

            if (pickResult == null || currentTool == null) return;
            if (pickResult.Entity != mPickPlane) return;


            Vector3d center = pickResult.ImpactPointLocalSpace + mPickPlane.DerivedTranslation;
            mPickMarker.Translation = center;


            AddHUDEntity_Immediate(this.Context.Region, mPickMarker, false);
        }

        private void UpdatePickPlanePosition()
        {
            if (mPickPlane != null && AppMain.mPlayerControlledEntity.Translation != null)
                mPickPlane.Translation = AppMain.mPlayerControlledEntity.Translation;
        }

        private void PickPlane_OnMouseDown(object sender, EventArgs e)
        {
            if (e is Keystone.Events.InputCaptureEventArgs)
            {
                Keystone.Events.InputCaptureEventArgs arg = (Keystone.Events.InputCaptureEventArgs)e;

                if (arg.MouseEventArgs.Button == Keystone.Enums.MOUSE_BUTTONS.RIGHT)
                {
                    System.Diagnostics.Debug.WriteLine("EditorHud.PickPlane_OnMouseDown() = RIGHT click");

                    if (AppMain.mPlayerControlledEntity != null)
                    {
                        Keystone.Vehicles.Vehicle vehicle = null;

                        if (AppMain.mPlayerControlledEntity is Keystone.Vehicles.Vehicle)
                            vehicle = (Keystone.Vehicles.Vehicle)AppMain.mPlayerControlledEntity;

                        // EditorHud is geared towards SciFiCommand.  A different game will need to handle setting a
                        // "destination" to a different type of Avatar
                        if (vehicle == null) return;

                        // assign destination position based on pickMarker location during right mouse click
                        Predicate<Keystone.Elements.Node> match = (n) =>
                        {
                            if (n.Name == "helm") return true;

                            return false;
                        };
                        Keystone.Entities.Entity helm = (Keystone.Entities.Entity)vehicle.FindDescendant(match);

                        if (helm == null) return;

                        int[] codes;
                        helm.SetCustomPropertyValue("destination", mPickMarker.DerivedTranslation, false, true, out codes);
                        mDestinationMarker.Translation = mPickMarker.DerivedTranslation;

                        if (mDestinationMarker.Parent == null)
                        {
                            mDestinationMarker.Visible = true;
                            AddHudEntity_Retained(mDestinationMarker);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("EditorHud.PickPlane_OnMouseDown() = LEFT click");
                }
            }
        }

        private void PickPlane_OnMouseClick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EditorHud.PickPlane_OnMouseClick() ");
        }

        // zoom can change and thus gridlines can change spacing
        private void UpdateGridZoom(float cameraZoom)
        {
            // GRID - NOTE: our grid attaches to root for EditorHud and Interior for FLoorplan
            //        so Grid Update should be done in each overriden Hud.cs implementation
            // NOTE: Grid.InfiniteGrid = false will move grid with camera.
            // NOTE: if Grid.AutoScale = true the grid spacing will be based on our distance to the plane of the grid
            // TODO: should i be using -context.Viewpoint.GlobalPosition ?  why should grid with moving origin disappear when we get too far?
            //		  - it's because we should always be rendering them in camera space and never add globaltranslation of camera to it
            //        just so that it can be subtracted prior to rendering.  It's the same issue i had with axis indicator.  Or is it?
            //        im looking at the code and so far it seems lines are rendered in camera space with no precision loss due to  
            // TODO: i believe the lines we are providing will have to be scaled manually?  For instance, if our scale is such that
            //       zoom = 1 unit = 1 / 1000th meter, then we have to divide all our grid line positions by this as well
            //       or, we must use 1/1000th values when computing row spacing and column spacing and such.  So the main thing
            //       we want to work on first though is just a grid that does not move and which does not auto scale 

            // TODO: if root is ZoneRoot then we should be using zone dimensions not zoneroot for SystemDiameter
            float systemDiameter = AppMain.REGION_DIAMETER * .1f; // *.1 for planetary which will be as 1/10th system size
            uint systemOuterRowCount = 100;
            uint systemOuterColumnCount = systemOuterRowCount;
            float systemRowSpacing = systemDiameter / (float)systemOuterRowCount;
            float systemColumnSpacing = systemDiameter / (float)systemOuterColumnCount;
            
            systemRowSpacing = 10 * cameraZoom;
            systemColumnSpacing = 10 * cameraZoom;

            // TODO: i think generally, this should always be false?  most of the time i think grid is useful at a tactical or higher "zoom" level and
            //       not at normal 1:1 scale.    
            Grid.UseFarFrustum = false;
            Grid.InfiniteGrid = true;
            Grid.OuterColumnCount = systemOuterColumnCount;
            Grid.OuterRowCount = systemOuterRowCount;
            // Grid.RowSpacing = systemRowSpacing;
            // Grid.ColumnSpacing = systemColumnSpacing;
        }

        private void LoadMotionField()
        {
            if (mMotionField == null)
            {
                int particleCount = 2500;
                int color = Keystone.Utilities.RandomHelper.RandomColor().ToInt32();

                float spriteSize = 0.125f;

                string texture = @"caesar\Shaders\Planet\star2.dds";
                // unique field name since each viewport can potentially have an indepedant motion field
                string fieldName = "motionfield_" + Repository.GetNewName(typeof(Entity));

                mMotionField = Keystone.Celestial.ProceduralHelper.CreateMotionField(fieldName,
                    Keystone.Celestial.ProceduralHelper.MOTION_FIELD_TYPE.SPRITE, particleCount,
                    texture, spriteSize, color);

                mMotionField.Translation = Vector3d.Zero();
                mMotionField.Name = null; // don't want label to render
                //mMotionField.RenderPriority = 1;

                AddHudEntity_Retained(mMotionField);
                Repository.IncrementRef(mMotionField);
            }
        }

        private ModeledEntity LoadYardStick()
        {
            if (mPlacementToolGraphic == null)
            {
                mPlacementToolGraphic = new ModeledEntity(Keystone.Resource.Repository.GetNewName(typeof(ModeledEntity)));
            	                
                Keystone.Elements.Model model = new Keystone.Elements.Model(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model)));
                Keystone.Elements.Mesh3d yardStickMesh = (Keystone.Elements.Mesh3d)Keystone.Resource.Repository.Create(System.IO.Path.Combine(AppMain._core .DataPath, @"editor\widgets\axis.obj"), "Mesh3d");
                model.Scale = new Vector3d(1.5, 4, 1.5);

               	Keystone.Appearance.DefaultAppearance app = (Keystone.Appearance.DefaultAppearance)Keystone.Resource.Repository.Create("DefaultAppearance");
                Keystone.Appearance.Material yellowMaterial = Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.yellow_fullbright);
                app.AddChild(yellowMaterial);

                model.AddChild(yardStickMesh);
                model.AddChild(app);
                mPlacementToolGraphic.AddChild(model);
                mPlacementToolGraphic.Enable = true;
                mPlacementToolGraphic.Pickable = false;
                mPlacementToolGraphic.Overlay = true; // yardstick we do want overlay = true
                mPlacementToolGraphic.Dynamic = false;
                mPlacementToolGraphic.UseFixedScreenSpaceSize = true;
                mPlacementToolGraphic.ScreenSpaceSize = .1f;
                mPlacementToolGraphic.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mPlacementToolGraphic.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mPlacementToolGraphic.Serializable = false;

                // NOTE: We increment regardless of whether we grabbed the existing copy from Repository
                // because a second instance of the placement tool may be disposing still while this
                // new placementtool instance is activated so it could result in race condition with
                // it's deref.
                // NOTE: increment the ref count to keep it from falling out of scope.
                // NOTE: We only ever have to increment just the top most entity 
                // not each child.
                Keystone.Resource.Repository.IncrementRef(mPlacementToolGraphic);
                Keystone.IO.PagerBase.LoadTVResource (mPlacementToolGraphic);
            }
            
            return mPlacementToolGraphic;
        }

        #region IDisposeable
        public override void Dispose()
        {
            base.Dispose();

            if (mVehicleMenu != null)
                Repository.DecrementRef(mVehicleMenu);
        }
        #endregion
    }
}
