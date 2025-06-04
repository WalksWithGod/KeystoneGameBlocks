using System;
using DevComponents.DotNetBar;
using KeyCommon.Traversal;
using KeyEdit.Controls;
using Keystone.Cameras;
using Keystone.Entities;
using Keystone.Portals;
using Keystone.Types;

namespace KeyEdit.Workspaces
{
	// TODO: re-read  E:\dev\c#\KeystoneGameBlocks\Design\vehicledesign.txt
    public partial class FloorPlanDesignWorkspace : EditorWorkspace  
    {   	
        // gui controls used in this workspace
        private bool mToolBoxDocked = false;
        private bool mComponentBrowserDocked = false;

        private MTV3D65.CONST_TV_BLENDINGMODE mPreviousVehicleBlendingMode;
        private float mPreviousVehicleOpacity;
        System.Collections.Generic.List<float> mPreviousOpacities = new System.Collections.Generic.List<float>();
        System.Collections.Generic.List<MTV3D65.CONST_TV_BLENDINGMODE> mPreviousBlendingModes = new System.Collections.Generic.List<MTV3D65.CONST_TV_BLENDINGMODE>();
        int mPreviousCullMode;

        private Keystone.Vehicles.Vehicle mVehicle;
        
		protected Keystone.Entities.Viewpoint  mIsometricBehaviorViewpoint;

		
        public FloorPlanDesignWorkspace(string name, Keystone.Vehicles.Vehicle vehicle) 
            : base(name)
        {
            if (vehicle == null) throw new ArgumentNullException("FloorPlanDesignWorkspace.ctor() - vehicle must not be null");
            mVehicle = vehicle;

            InitializeViewpoint();
            
        }

        // TODO: is it ok that Vehicle here means all viewports assigned to this workspace
        //       must use same Vehilce?  I think this is why i use RenderingContext and HUDs for
        //       storing so much info...  and perhaps why "CustomOptions" in RenderingContext
        //       is a good place to store workspace specific user data that must also be context specific.

        /// <summary>
        /// This is assigned to the workspace permanently unlike mSelectedEntity
        /// which can be used to select items that were placed in the Interior.
        /// </summary>
        public Keystone.Vehicles.Vehicle Vehicle 
        {
            get { return mVehicle; } 
        }
        
        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            //base.Configure(manager); // NOTE: Dont call base.Configure because it'll instatiates viewportcontrols that get duplicated here

            if (manager == null) throw new ArgumentNullException("FloorPlanDesignWorkspace.Configre() - WorkspaceManager parameter is NULL.");
            if (scene == null) throw new ArgumentNullException();
            mScene = scene;
            mWorkspaceManager = manager;

            //mWorkspaceManager.LoadLayout();

            // NOTE: dim the mViewportControls array before mDocument creation 
            mViewportControls = new ViewportControl[1]; // one for interior, one for exterior hardpoint placement

            // create our viewports document.  only need 1 to hold all 4 viewportControls
            mDocument = new KeyEdit.Controls.ViewportsDocument(this.Name, KeyEdit.Controls.ViewportsDocument.DisplayMode.Single, OnViewportOpening, OnViewportClosing);
			mDocument.Resize += base.OnDocumentResized;

            //mDocument.SendToBack(); <-- fix control zorder issues?
            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, OnGotFocus);

            //((KeyEdit.Controls.ViewportsDocument)mDocument).ConfigureLayout(KeyEdit.Controls.ViewportsDocument.DisplayMode.Single);
            
            // TODO: what is our exterior view like?  


            // TODO: Why don't we have to BindViewpoints() here?
            // - it's because we want to bind a viewpoint using the same region
            //   as the target vehicle (eg root region) but not necessarily! coudl be a zone!
            //   and so we have to do this upon "workspace.Selected" being set to the Vehicle
            //   thus... 


            // SetDisplayMode() in EditorWorkspace calls .Show
            // but here we dont call that so we directly call .Show.
            // Show() is required here to activate this tab properly right away.. or is it? Actually
            // the only reason Show() is not needed in the following case is because usually the edit tab
            // is visible when floorplan tab is created and we must explicilty click the floorplan tab
            // to switch which results in a call to .Show()
            // the caller should do it then from formmain that initiated creation of floorplan view
            
            Show();

            this.Resize();
        }


        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs e)
        {

            if (mViewportControls[e.ViewportIndex] == null)
            {
                ViewportControl c = 
                    new ViewportFloorplanControl(CreateNewViewportName((int)e.ViewportIndex), 
                                                                this, mScene, CategoryChanged);
                mViewportControls[e.ViewportIndex] = c; 
                e.HostControl.Controls.Add(mViewportControls[e.ViewportIndex]);

                ConfigureViewport(mViewportControls[e.ViewportIndex], new Huds.FloorplanHud(this));

                c.Viewport.BackColor = Keystone.Types.Color.RoyalBlue;

                // wire cull being/completed events in case we want a chance to modify before and after
                // TODO: perhaps the HUD can also subscribe to these events with it's own methods?
                c.Viewport.Context.CullingStart += OnCullingStart;
                c.Viewport.Context.CullingComplete += OnCullingCompleted;
                c.Viewport.Context.RenderingStart += OnRenderingStart;
                c.Viewport.Context.RenderingComplete += OnRenderingCompleted;


                // hud options
                c.Viewport.Context.AddCustomOption(null, "show axis indicator", typeof(bool).Name, true);
				c.Viewport.Context.AddCustomOption(null, "show pathing debug information", typeof(bool).Name, false);
                c.Viewport.Context.AddCustomOption(null, "show connectivity", typeof(bool).Name, true);
                c.Viewport.Context.AddCustomOption(null, "show orbit lines", typeof(bool).Name, false);
				c.Viewport.Context.AddCustomOption(null, "show celestial grid", typeof(bool).Name, false);
				c.Viewport.Context.AddCustomOption(null, "show motion field", typeof(bool).Name, false);
				c.Viewport.Context.AddCustomOption(null, "show star map", typeof(bool).Name, false);
                c.Viewport.Context.AddCustomOption(null, "show exterior", typeof(bool).Name, true);
                c.Viewport.Context.AddCustomOption(null, "exterior vehicle opacity", typeof(float).Name, 0.00f);
                c.Viewport.Context.AddCustomOption(null, "wall show mode", typeof(int).Name, 1); // cut away
                c.Viewport.Context.AddCustomOption(null, "show current floor only", typeof(bool).Name, false);
                
                c.Viewport.Context.AddCustomOption(null, "cell grid visibility", typeof(string).Name, "legal"); // none, legal & illegal, legal
                c.Viewport.Context.AddCustomOption(null, "tile grid visibility", typeof(string).Name, "none");  // none, mask visualizer

                // TODO: need to do a better job of restoring ABOVE settings from last run for Editor and Floorplan
 
                
                c.Viewport.Enabled = true;

                // Bind() to the exterior of this ship
                c.Viewport.Context.Bind(mIsometricBehaviorViewpoint);

                // TODO: switch to c.Viewport.Context.State.SetFloat("cam_speed", 10f); and perhaps we can even
                //       preconfigure each datatype if we know precise count of each so that our state doesnt waste mem.
                //       if we ever have to expand, we can warn in debug log but then resize and continue
              //  c.Viewport.Context.Viewpoint.CustomData.SetFloat("cam_speed", 10f);

                // Configure of Picking and HUD only tweaks setting, does NOT create or enable either
                ConfigurePicking(c.Viewport);
                ConfigureHUD(c.Viewport);
            }
        }


        private void InitializeViewpoint()
        {
            // create a viewpoint for the interior of the ship
            string id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Entities.Viewpoint));
            mIsometricBehaviorViewpoint = Viewpoint.Create(id + "_" + mVehicle.ID,  // todo: should this Region.ID be the Interior.ID?
                                                            mVehicle.Region.ID);

            mIsometricBehaviorViewpoint.BlackboardData.SetString("control", "user"); // {"user, "animated"}
            mIsometricBehaviorViewpoint.BlackboardData.SetString("focus_entity_id", mVehicle.ID); // todo: should this be the mVehicle.Interior.ID? what about when we want to focus and follow individual crew? Do we need to AddChild() the viewpoint to the Interior?

            mIsometricBehaviorViewpoint.BlackboardData.SetString("goal", "goal_none");
            // note: we're currently viewing the Interior from the Exterior space and so we need an element of "chase" to keep the camera position relative to the Interior
            mIsometricBehaviorViewpoint.BlackboardData.SetBool("chase_enabled", true);

            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("orbit_radius", 50d);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("orbit_radius_min", 25d);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("orbit_radius_max", 400d);

            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("orbit_radius_destination", 100d);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("max_altitude", 500d);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("min_altitude", 10d);

            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("min_vertical_angle", 44.99d);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("max_vertical_angle", 45d);
            mIsometricBehaviorViewpoint.BlackboardData.SetVector("offset", new Vector3d()); // offset must be constrained within min_bounds and max_bounds
            mIsometricBehaviorViewpoint.BlackboardData.SetVector("min_bounds", new Vector3d());
            mIsometricBehaviorViewpoint.BlackboardData.SetVector("max_bounds", new Vector3d());

            mIsometricBehaviorViewpoint.BlackboardData.SetBool("smooth_zoom_enabled", true);
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("smooth_zoom_time", 1d); // seconds
            mIsometricBehaviorViewpoint.BlackboardData.SetDouble("smooth_zoom_elapsed", 0d);

            // TODO: added serializable = false on June.26.2014 - is correct?            
            mIsometricBehaviorViewpoint.Serializable = false;

            // create behavior for this viewpoint.  notice that there is only ONE behavior
            // and the logic for how this viewpoint should act under different conditions
            // must be fully contained within this behavior.  In other words, there should never
            // be a need to swap out new behavior trees because a single behavior tree should
            // cover all of our scenarios and decide on what appropriate actions to take on it's own.
            // That's it's job!
            Keystone.Behavior.Composites.Composite behaviorTree = CreateIsometricBehavior(); // CreateVPRootSelector();
            mIsometricBehaviorViewpoint.AddChild(behaviorTree);
        }


        // TODO: i could modify this to accept a specific viewport that is created during
        // OnViewportOpening
        protected override void ConfigureHUD(Keystone.Cameras.Viewport viewport)
        {
            //base.ConfigureHUD();
            // set RenderingContext options appropriate for whether to use config file graphics
            // settings or to override and force those specific for editor viewports
            //vpControl.ReadSettings(_iniFile);

            if (viewport != null)
                if (viewport.Context.Hud != null)
                {
                    // TODO: it would be preferable I think if we could make these
                    //       settings more arbitrary by just having a hashtable of
                    //       keys and values
                    //mViewportControls[i].Viewport.Context.
                    viewport.Context.ShowEntityBoundingBoxes = false;
                    viewport.Context.ShowOctreeBoundingBoxes = false;
                    viewport.Context.ShowEntityLabels = false;
                    viewport.Context.ShowFPS = true;
                    viewport.Context.ShowTVProfiler = false;
                    viewport.Context.TraverseInteriorsAlways = true;
                    viewport.Context.TraverseInteriorsThroughPortals = true;
                }
        }

        protected override void ConfigurePicking(Keystone.Cameras.Viewport viewport)
        {
            //base.ConfigurePicking();
            // set RenderingContext PickParameters appropriate for Floorplan Viewports

            // NOTE: Accuracy is used to determine the precision to use when testing if a hit occurs or not. 
            // (eg bounding box versus per face to determine hit) 
            PickAccuracy accuracy = PickAccuracy.Tile | PickAccuracy.Face; // |  
            //    PickAccuracy.Vertex | PickAccuracy.EditableGeometry;

            // KeyCommon.Flags.EntityFlags.ExteriorRegion
            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.Background;

            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.ContainerExterior |
                KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Interior_Region;

            // NOTE: use PickSearchType flag PickSearchType.Interior  if we want to limit our search to children of
            //       interior regions.

            if (viewport != null)
            {
                PickParameters pickParams = new PickParameters
                {
                    T0 = -1,
                    T1 = -1,
                    SearchType = PickCriteria.Closest,
                    SkipBackFaces = true,
                    Accuracy = accuracy,
                    ExcludedTypes = excludedObjectTypes,
                    IgnoredTypes = ignoredObjectTypes,
                    FloorLevel = int.MinValue
                };
                viewport.Context.PickParameters = pickParams;
            }
        }

        /// <summary>
        /// RenderingContext specific rendering state overrides can be applied here
        /// to part of the scene or the entire scene.  for instance, we can temporarily make
        /// the exterior of a vehicle transparent for just one particular RenderingContext.
        /// </summary>
        /// <param name="context"></param>
        protected override void OnCullingStart(RenderingContext context)
        {
            
            // is a vehicle target set?

            if (Vehicle == null) return;

            // TODO: ensure that Vehicle cannot be changed until OnCullingCompleted at this point

            if ((bool)mViewportControls[0].Viewport.Context.GetCustomOptionValue(null, "show exterior") == true)
            {
                // 
            }
            else
            {
                // create 
            }
            // find our exterior model
            Keystone.Elements.Model exteriorModel = Vehicle.Model;

            // get it's appearance and see if it has changed since last time
            Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)exteriorModel.Appearance;
            // NOTE: We do this at cull start so that RegionPVS.Add() can sort it based on
            //       the new transparent material!
            DoTransparency(exteriorModel, appearance, context);
           

            //   - note: cache must be RenderingContext specific.  Ideally i think that is why
            //     our RenderingContext should be responding to these events somehow...
            //     or why perhaps our RC's can contain CustomOptions we can define in workspace
            //     and in any custom viewport, but then here in the workspace we pass the
            //     context and from there we can grab the context specific data values
            //     and apply the appropriate operations here in the Workspace.  This way its just
            //     our workspaces that are mostly unqiue and we dont have this mess of code in
            //     every different hud and every different viewportControl implemention.  Just
            //     the different workspace imlementations (mostly)
            // 

           
            FilterExteriorFacingWalls(context, Vehicle);

            // use double sided rendering if current tool is boundaries painting
            if (mCurrentTool is KeyEdit.Workspaces.Tools.InteriorSegmentPainter)
            {
                if ((mCurrentTool as Tools.InteriorSegmentPainter).LayerName == "boundaries")
                {
                    mPreviousCullMode = exteriorModel.Geometry.CullMode;
                    exteriorModel.Geometry.CullMode = 1; // MTV3D65.CONST_TV_CULLING.TV_DOUBLESIDED;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This is currently done every frame for every wall segment regardless of 
        /// culling visibility.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="vehicle"></param>
        private void FilterExteriorFacingWalls(RenderingContext context, Keystone.Vehicles.Vehicle vehicle)
        {
            if (vehicle.Interior == null) return;
            // if this wall is on an edge that border Out of Bound cell, then it's an exterior
            // edge segment

            // cache this filtered list so we can undo it.
            // and so if camera has not moved, we can re-use it 
            // and also, when camera is moving, we will limit the Hz which we compute
            // new filtered walls

            object temp = context.GetCustomOptionValue (null, "wall show mode");
            int mode = 1; // 0 = down, 1 = cutaway, 2 = up
            if (temp != null) mode = (int)temp;

            ((Keystone.Portals.Interior)Vehicle.Interior).FilterCutAway(mode, context.Position, context.LookAt);

            
        }

        // TODO: this call SetVisibleFloor is done here in the control however it should be done i think
        //       in the workspace along with FilterWalls for the wall show mode because it is per context
        //       and not app wide.  Different viewports may have different reqts.
        // note: don't confuse the mesh grid we use as HUD element
        //       with an actual floor model.  This method is for actual interior floors
        internal void SetVisibleFloor(RenderingContext context, int floor, bool disableAllVisibleStructures)
        {
        	// todo: this seems to only occur when we change the current floor from the toolbar.  It does not get called every frame.

            // Disable the Quadtree children within QuadtreeCollection that stores all NON floor/ 
            // ceiling, and wall entities (keep in mind floor/ceiling/walls are not even entities!)
            // within each deck for each deck that is not visible, and Enable the quadtree for visible
            // decks. This is fastest way to enable and disable entities on visible and non visible decks
            // respectively.   It does not work on floor/ceiling/wall models because those are not seperate
            // entities but are interior structures of the CelledRegion itself.
            Keystone.Entities.Container vehicle = Vehicle;
            Keystone.Elements.RegionNode interiorRegionNode = (Keystone.Elements.RegionNode)vehicle.Interior.SceneNode;
            Keystone.QuadTree.QuadtreeCollection quadtreeCollection = (Keystone.QuadTree.QuadtreeCollection)interiorRegionNode.SpatialNodeRoot;
            bool showCurrentFloorOnly = (bool)context.GetCustomOptionValue(null, "show current floor only");

            // NOTE: I don't believe walls are added to quadtreeCollection or if they are, its not really helpful
            //       because the entire MinimeshGeometry is added where individual elements are placed all over the deck
            for (int i = 0; i < quadtreeCollection.Children.Length; i++)
                if (floor == -1)
                    // make all floors visible
                    quadtreeCollection.Children[i].Visible = true;
                else
                {
                    if (showCurrentFloorOnly)
                        quadtreeCollection.Children[i].Visible = i == floor;
                    else
                        // render all floors below or equal to the current floor.
                        // this is necessary to see components such as hatches and components
                        // visible through holes in the current floor and lower floors.
                        // we should render higher floors first and lower floors last so
                        // we get early out Z test in shaders.
                        quadtreeCollection.Children[i].Visible = i <= floor;
                }

            // After QuadtreeCollection, we must still enable/disable the Floor, Ceiling and
            // Wall models because these are
            // not treated as seperate entities within a CelledRegion.  They are treated as sub
            // elements or structural internals of the CelledRegion itself!  This is why they are
            // not inserted into any Quadtree and thus why although say a chair component will 
            // be invisible when we disable a floor, the actual floors and walls for that floor
            // will not if we do not explicility disable those models.

            vehicle.Interior.Visible = true;

            // define a match for finding all nodes of type "Model" who's IDs start with a prefix
            // that we know all floor\ceiling and wall models for any one particular Interior share.
            Predicate<Keystone.Elements.Node> match = e =>
            {
                return
                    e is Keystone.Elements.Model &&
                    e.ID.Contains(Interior.GetInteriorElementPrefix(vehicle.Interior.ID, typeof(Keystone.Elements.Model)));
            };

            // Perform the actual query passing in our match 
            // NOTE: we are guaranteed because of the Predicate delegate we are using, that all results
            // are of type Keystone.Elements.Model
            Keystone.Elements.Node[] foundModels = vehicle.Interior.Query(true, match);

            // disable rendering of all interior floor\ceiling and wall Models
            // unless we are in boundary mode
             // NOTE: we want this to use ".Visible" not ".Enable"
            // because making interior .Enable = false disables
            // picking and disables cull traversal, and prevents us
            // from attaching to same coord system any hud elements
            // such as our floor boundary grids
            if (foundModels != null)
                for (int i = 0; i < foundModels.Length; i++)
                {
                    bool enable = false;

                    // ideally we avoid having to parse a naming convention....
                    // NOTE: for a floor we are viewing, we may also enable the ceiling of
                    // the floor above if there is one since during bounds painting we can
                    // specify if any tile can only host a ceiling or only host a floor.
                    // f -it, we will parse the name...
                    string[] splitString = foundModels[i].ID.Split(Interior.PREFIX_DELIMIETER);
                    if (splitString == null || splitString.Length  == 0) continue;

                    string temp = splitString[splitString.Length - 1];
                    int index = int.Parse(temp);
                    bool isFloor = splitString[splitString.Length - 2] == Interior.PREFIX_FLOOR;
                    bool isCeiling = splitString[splitString.Length - 2] == Interior.PREFIX_CEILING;
                    bool isWall = ContainsPrefixWall(splitString);

                    // when painting illegal/legal interior cell we want to hide all inteior geometry
                    if (disableAllVisibleStructures == false)
                    {
                        if (isFloor)
                        {
                            if (showCurrentFloorOnly)
                                enable = index == floor;
                            else
                                enable = index <= floor; // show current floor and floors beneath
                        }
                        if (isCeiling)
                            enable = true; // TEMP HACK - force true to show all ceilings because currently no ceilings are rendering // index == floor + 1;
                        if (isWall)
                        {
                            if (showCurrentFloorOnly)
                                enable = index == floor;
                            else
                                enable = index <= floor; // show current walls and walls on lower decks
                        }
                    }
                    foundModels[i].Enable = enable;
                }
        }

        private bool ContainsPrefixWall(string[] split)
        {
            if (split == null) return false;
            for (int i = 0; i < split.Length; i++)
                if (split[i].Contains(Interior.PREFIX_WALL))
                    return true;

            return false;
        }

        protected override void OnCullingCompleted(RenderingContext context)
        {

        }

        protected override void OnRenderingStart(RenderingContext context)
        {

            
        }


        protected override void OnRenderingCompleted(RenderingContext context)
        {
            // must cleanup any outstanding overrides that were 

            if (Vehicle == null) return;

            // TODO: how do we ensure that the Vehicle's Appearance cannot be changed until
            //       the restore has occurred?  otherwise we will basically modify the changed appearance
            //       in an uninted way with cached restore values that actually originated from the prev 
            //       appearance
            if ((bool)mViewportControls[0].Viewport.Context.GetCustomOptionValue(null, "show exterior") == true)
            {
                // 
            }
            else
            {
                // create 
            }

            // retore the original blending mode and tranparency to exterior vehicle model
            Keystone.Elements.Model exteriorModel = Vehicle.Model;

            Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)exteriorModel.Appearance;

            UnDoTransparency(exteriorModel, appearance);

            if (mCurrentTool is KeyEdit.Workspaces.Tools.InteriorSegmentPainter)
            {
                if ((mCurrentTool as Tools.InteriorSegmentPainter).LayerName == "boundaries" )
                    exteriorModel.Geometry.CullMode = mPreviousCullMode;
            }
        }

        
        private void DoTransparency(Keystone.Elements.Model exterior, Keystone.Appearance.DefaultAppearance appearance, RenderingContext context)
        {
            if (appearance == null) return;

            mPreviousBlendingModes.Clear();
            mPreviousOpacities.Clear();

            float opacity = 0.5f;
            object tmpOpacity = context.GetCustomOptionValue(null, "exterior vehicle opacity");
            if (tmpOpacity != null)
            {
                opacity = (float)tmpOpacity;
                // NOTE: it is only the exterior Model not ModeledEntity that is being disabled
                if (opacity == 0.0f) exterior.Enable = false;
            }
            // NOTE: the overall appearance blending mode must be set even if there is no appearance.Material
            //       otherwise none of the child GroupAttributes will have proper alpha tranparency
            mPreviousBlendingModes.Add(appearance.BlendingMode);
          //  mPreviousVehicleBlendingMode = appearance.BlendingMode;
            appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

            // appearance.Material may be null if instead of an overall Material
            //       we have a TVMesh with many groups and seperate Material for each GroupAttribute
            // In this case, we need to grab all the child Material's and set their opacity, and then
            // we have to set them all back.  Ideally our exterior ship mesh has just one group.
            if (appearance.Material != null)
            {
                //mPreviousVehicleOpacity = appearance.Material.Opacity;
                mPreviousOpacities.Add(appearance.Material.Opacity);
                appearance.Material.Opacity = (float)opacity;
            }

            // iterate through any child GroupAttributes
            for (int i = 0; i < appearance.ChildCount; i++)
            {
                Keystone.Appearance.GroupAttribute ga = appearance.Children[i] as Keystone.Appearance.GroupAttribute;
                if (ga != null)
                {
                    mPreviousBlendingModes.Add(ga.BlendingMode);
                    ga.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

                    if (ga.Material != null)
                    {
                        mPreviousOpacities.Add( ga.Material.Opacity);
                        ga.Material.Opacity = opacity;
                    }
                }
            }
        }

        private void UnDoTransparency(Keystone.Elements.Model exterior, Keystone.Appearance.DefaultAppearance appearance)
        {
            int materialIndex = 0;
            int blendingModeIndex = 0;

            exterior.Enable = true;

            if (appearance == null) return;
            if (appearance.Material != null)
            {
                appearance.Material.Opacity = mPreviousOpacities[materialIndex++]; ;
            }

           

            // NOTE: overall appearance.BlendingMode must be reset even if there is no default Material otherwise child
            //       GroupAttributes will not render with appropriate blending mode
            appearance.BlendingMode = mPreviousBlendingModes[blendingModeIndex++]; // mPreviousVehicleBlendingMode;


            // iterate through any child GroupAttributes
            for (int i = 0; i < appearance.ChildCount; i++)
            {
                Keystone.Appearance.GroupAttribute ga = appearance.Children[i] as Keystone.Appearance.GroupAttribute;
                if (ga != null)
                {
                    ga.BlendingMode = mPreviousBlendingModes[blendingModeIndex++];
                    if (ga.Material != null)
                    {
                        ga.Material.Opacity = mPreviousOpacities[materialIndex++];
                    }
                }
            }

        }


        private void OnGotFocus(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FloorPlanDesignWorkspace.GotFocus() - ");
        }

        #region IWorkspace Members
        public override void Show()
        {
            //base.Show(); // NOTE: Do NOT call base.Show() as we do not want a new Scene Explorer treeview
            //             // We do need a new AssetBrowser though but it must be a custom one with different
            //             // filters.   We will want to hide the existing and show this when in this workspace active.


            // TODO: The asset browser for the Main Viewer is configured to change the placement tool
            // only in that specific context and NOT whatever is the "active" workspace.
            // Thus we must have a seperate asset browser but one tailored to just components needed
            // for floorplan creation such as partitions, components, engines, reactors, weapons, etc
            //
            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = true;
                }

            mDocument.Visible = true;
            mDocument.Enabled = true;


       
            // load a special type of asset browser that is geared towards interior components
            // place-able when in Floorplan 
            if (mComponentBrowserDocked == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mBrowser, "Component Browser", "leftDockSiteBar", "Component Browser", eDockSide.Left);
                mComponentBrowserDocked = true;
            }
            
            mIsActive = true;
        }

        public override void Hide()
        {
            UnDoTransparency(mVehicle.Model, (Keystone.Appearance.DefaultAppearance)mVehicle.Model.Appearance);
            // base.Hide(); // as a rule, if we don't use base.Show() we can't use base.Hide() 
            //              // especially since different docked panels are loaded by each

            // loading a new definition before we remove these viewports will result in these viewports
            // being disposed.  So we remove these controls from the DockContainerItem first

            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = false;
                }

            mDocument.Visible = false;
            mDocument.Enabled = false;

            if (mComponentBrowserDocked == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Component Browser", "leftDockSiteBar");
                mComponentBrowserDocked = false;
            }
            
            mIsActive = false;

        }
        #endregion
    }
}
