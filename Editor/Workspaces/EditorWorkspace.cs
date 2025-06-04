using System;
using System.Diagnostics;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using KeyCommon.Traversal;
using KeyEdit.Controls;
using Keystone.Resource;

namespace KeyEdit.Workspaces
{
    public partial class EditorWorkspace : WorkspaceBase3D 
    {
        uint VIEWPORTS_COUNT = 5;// 4+1 for quicklook even if only main viewport is ever used                
        protected bool mQuickLook3DViewportEnabled = false; // TODO: this should be read and assigned from INI file settings
        
        // gui controls used in this workspace
        protected KeyEdit.GUI.AssetBrowserControl mBrowser;
        private TreeView mTreeEntityBrowser;
        private Silver.UI.ToolBox mToolBox; // lights, primitives, celestials
        protected bool mAssetBrowserDocked = false;
        bool mTreeViewDocked = false;
        bool mToolBoxInitialized = false;

        // NOTE: this viewpoint is just the basis for which we will clone for each mViewportControls[].Viewpoint 
        //       so if we want to change the behavior on the fly, we need to call for instance mViewportControls[0].Viewpoint.CustomData.SetString();
        Keystone.Entities.Viewpoint mExteriorFreeViewpoint;
        

        
        public EditorWorkspace(string name) 
            : base(name)
        {
            InitializeToolBox(); 
            InitializeAssetBrowser();
            InitializeTreeview();
        }

        private object mSyncLock = new object();
        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
        	try 
        	{
            	base.Configure(manager, scene);
		
                // TODO: if the EditorWorkspace is not used (eg. retail release) then
                //       we never LoadLayout.  I do not like LoadLayout here in Configure.  It should
                //       be done within WorkspaceManager.  The main issue is, the layout is just for
                //       dockbar layouts, not for containers.  Otherwise we'd need separate layout
                //       definition files for each workspace and we don't currently do that.
	            mWorkspaceManager.LoadLayout();
	
	
	            // NOTE: dim the mViewportControls array before mDocument creation 
	            mViewportControls = new KeyEdit.Controls.ViewportControl[VIEWPORTS_COUNT];
	
	            // create our viewports document.  This document will host 4 of our viewport controls in splitter panels
	            mDocument = new KeyEdit.Controls.ViewportsDocument(this.Name, mDisplayMode, OnViewportOpening, OnViewportClosing);
                WireDocumentEvents();
		
	            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, OnGotFocus);

	            // ConfigureLayout will initialize the viewportcontrols but must be called 
	            // AFTER .CreateWorkspaceDocumentTab()
	            //((KeyEdit.Controls.ViewportsDocument)mDocument).ConfigureLayout(mDisplayMode);
	            
	            // Clone new viewpoints from the default viewpoint supplied in scene info
	            if (mViewpointsInitialized == false)
	            	InitializeViewpoints();
	            
	            // Bind all viewports to a cloned default configured viewpoint
	            BindViewpoints();

                // NOTE: This call to PopulateTreeview is not needed if the treeview exists prior to the Scene being loaded.
                // EditorWorkspace.Treeview.AddTreeItemHandler() will get called when Entities are "attached" to the scene.
                // Howevever, perhaps if this workspace isn't loaded until after the scene is loaded, we would need to populateTreeview
                // but that code needs to be debugged because it's resulting in all Entites being added as root nodes.
                //  PopulateTreeview(null, mScene.Root);
                this.Resize();
        	}
        	catch (Exception ex)
        	{
        		System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Configure() - ERROR: " + ex.Message);
        	}
        }
                
        private void WireDocumentEvents()
        {
            // fill style seems to screw up toolbars?
           // mDocument.Dock = System.Windows.Forms.DockStyle.Fill;
            //mDocument.Enabled = true; // preview window is immediately always enabled and visible since Show/Hide are only called for Document tabs 
            //mDocument.Visible = true; // preview window is immediately always enabled and visible since Show/Hide are only called for Document tabs

            // these events fire when dragging from Toolbox _ONTO_ the mDocument
            mDocument.AllowDrop = true;
            mDocument.DragEnter += this.dragContainer_DragEnter;
            mDocument.QueryContinueDrag += this.dragContainer_QueryContinueDrag;
            mDocument.DragLeave += this.dragContainer_DragLeave;
            mDocument.DragDrop += this.dragContainer_DragDrop;
            mDocument.DragOver += this.dragContainer_DragOver;
            
            //mViewDocument.SendToBack();
        }

        protected void InitializeViewpoints()
        {
            // NOTE: We clone the 2nd of our default viewpoints at index  = 1 which is a Zone. 
            // Index = 0 is ZoneRoot 
            // but no paging will automatically take place while our camera is tied to that viewpoint.
            string id = Repository.GetNewName(typeof(Keystone.Entities.Viewpoint));
            if (mScene.Root is Keystone.Portals.ZoneRoot)
                mExteriorFreeViewpoint = (Keystone.Entities.Viewpoint)mScene.Viewpoints[1].Clone(id, true, false, false);
            else
                mExteriorFreeViewpoint = (Keystone.Entities.Viewpoint)mScene.Viewpoints[0].Clone(id, true, false, false);


            mExteriorFreeViewpoint.Serializable = false;
            // create behavior for this viewpoint.  notice that there is only ONE behavior
            // and the logic for how this viewpoint should act under different conditions
            // must be fully contained within this behavior.  In other words, there should never
            // be a need to swap out new behavior trees because a single behavior tree should
            // cover all of our scenarios and decide on what appropriate actions to take on it's own.
            // That's it's job!
            Keystone.Behavior.Composites.Composite behaviorTree = CreateVPRootSelector();

            // chase is in a selector switch with orbit and normal free form cam
            // Keystone.Behavior.Composites.Sequence subSequence

            // this viewpoint and it's behavior tree will be cloned by any viewport we open in this workspace
            mExteriorFreeViewpoint.AddChild(behaviorTree);
            mViewpointsInitialized = true;

            System.Diagnostics.Debug.WriteLine("EditorWorkspace.InitializeViewpoints() - Success.");
        }

        protected void BindViewpoints()
        {
            // Bind all viewports to a cloned default configured viewpoint
            // TODO: in the future, we will assign viewpoints by network server for "spawns"
            // and such.
            // TODO: wait a minute, we don't want server side assigned viewpoints do we? THe server
            //       doesn't care about viewpoints... the server only cares about the entities that the viewpoints are bound to.
            for (int i = 0; i < mViewportControls.Length; i++)
                if (mViewportControls[i] != null)
                {
                    // TODO: i added this Repository.IncrementRef to prevent errors with 
                    // the clonedivewpoint getting removed from repository (presumeably when the
                    // viewpoint gets paged out) but i dont understand why this is necessary now when
                    // it wasnt before.  I do kow the Viewpoints[0] and [1] is new.  Didnt used to
                    // have more than 1.
                    string id = Repository.GetNewName (typeof(Keystone.Entities.Viewpoint));
                    try 
                    {
                        // todo: i don't think i have to clone this viewpooint because mExteriorFreeViewpoint is never used anywhere else
                        //       and it, itself, is cloned from a SceneInfo.Viewpoint that contains the relevant startingRegionID and translation
                    	Keystone.Entities.Viewpoint clone = (Keystone.Entities.Viewpoint)mExteriorFreeViewpoint.Clone(id, true, false, false);
                        clone.Name = "EditorWorkspace_Viewpoint - " + i;
                        Repository.IncrementRef(clone);
                        // NOTE: the bound viewpoint is added to the relevant Region in ClientPager.Update() after the Region has been paged in
                        mViewportControls[i].Viewport.Context.Bind(clone);
                    	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.BinidViewpoints() - Viewpoint bound.");
                    }
                    catch (Exception ex)
                    {
                    	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.BinidViewpoints() - ERROR: " + ex.Message);
                    }
                    
                }
        }

		protected override void ConfigureViewport(ViewportControl vpControl, Keystone.Hud.Hud hud)
		{
			base.ConfigureViewport(vpControl, hud);
		
	        vpControl.AllowDrop = true;
            vpControl.DragDrop += new DragEventHandler(this.dragContainer_DragDrop);
            vpControl.DragOver += new DragEventHandler(this.dragContainer_DragOver);  
		}
		
        protected override void ConfigureHUD(Keystone.Cameras.Viewport viewport)
        {       
            if (viewport != null)
                if (viewport.Context.Hud != null)
                {
                    viewport.Context.IncludeEntityAttributes(KeyCommon.Flags.EntityAttributes.All);
                    viewport.Context.AllowedEntityAttributes(KeyCommon.Flags.EntityAttributes.All);

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

                    viewport.Context.AddCustomOption(null, "smooth_zoom_enabled", typeof(bool).Name, false);
                }
                

                
            //// TODO: i should  always read these values from ini 
            //// and do so via vpControl.ReadSettings (iniFile)
            //viewport.Cursor = AppMain._core.PrimaryContext.Viewport.Cursor;
            //viewport.BackColor = AppMain._core.PrimaryContext.Viewport.BackColor;
        }

        private void QuickLookViewportInitialization(uint viewportsCount)
        {
            // TODO: PickParameters should be passed in?  
            // what about an edit controller?
            // ugh i hate having this method.
            //
            // TODO: to update and render this control properly, i think we must include this control
            //       in the array of viewports.
            // viewport control creation should be here in exe app not in keystone.dll
            // TODO: what if our QuickLook inherited ViewportEditorControl ?

            
            	
    		// TODO: if this is enabled, it screws up Move/Rotation/Scaling widgets.  The widget
            //       appears in the QuickLookViewport and not in the main editor viewport.
            //OnViewportOpening (this, new ViewportsDocument.ViewportEventArgs(mQuickLookPanel, viewportsCount - 1));
            
            ViewportEditorControl c = (ViewportEditorControl)mViewportControls[viewportsCount - 1];
            c.Width = 128;
            c.Height = 128;
            c.Top = 0;
            c.Left = 0;
            
            c.Dock = System.Windows.Forms.DockStyle.None; // undo the DockStyle.Fill which occurs during viewport config
            c.ShowToolbar = false;  
            
			
			// hud options
            c.Viewport.Context.ShowFPS = false;
			c.Viewport.Context.ShowEntityLabels = false;

            c.Viewport.Context.AddCustomOption (null, "show axis indicator", typeof(bool).Name, false);
            c.Viewport.Context.AddCustomOption (null, "show pathing debug information", typeof(bool).Name, false);
            
            c.Viewport.Context.AddCustomOption (null, "show orbit lines", typeof(bool).Name, false);
            c.Viewport.Context.AddCustomOption (null, "show celestial grid", typeof(bool).Name, false);
			c.Viewport.Context.AddCustomOption(null, "show motion field", typeof(bool).Name, false);
			c.Viewport.Context.AddCustomOption(null, "show star map", typeof(bool).Name, false);
			
			c.Viewport.Context.AddCustomOption (null, "show vehicle icons", typeof(bool).Name, false);
            c.Viewport.Context.AddCustomOption (null, "show hardpoint icons", typeof(bool).Name, false);
			c.Viewport.Context.AddCustomOption (null, "show celestial icons", typeof(bool).Name, false);
					
			
			c.Enabled = mQuickLook3DViewportEnabled;  
			c.Visible = mQuickLook3DViewportEnabled;          	
        }
        
        
        protected override void ConfigurePicking(Keystone.Cameras.Viewport viewport)
        {
            // set RenderingContext PickParameters appropriate for Editor window viewports


            // NOTE: Accuracy is used to determine the precision to use when testing if a hit occurs or not. 
            // (eg bounding box versus per face to determine hit)
            PickAccuracy accuracy = PickAccuracy.Face; // PickAccuracy.Geometry |
            //PickAccuracy.Vertex |
            //PickAccuracy.EditableGeometry;

            // skip traversing of interiors without recursing children
            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes excludedObjectTypes = 
                KeyCommon.Flags.EntityAttributes.Structure;

            // TODO: this is incorrect, it needs to change not on CONFIGURE
            // but on pick activation on the viewdocument represented by this
            // workspace.  That means it cannot be applied as a context pickparameter
            // but as a workspace pick parameter that must be set prior to picking
            // in that workspace (given that workspaces change dynamically)
            // so im not sure how that works... the pickparameters should be stored here
            // and then adopted by the context's picker... or the picker exists here
            // and the parameters are set on it.. either way its the same problem so
            // might as well just use on picker... but must update parameters dynamically...
            // how?  
            // Wait, why are these PickParameters not per context where a context is unique 1:1
            // to every viewport? In fact, these pickparameters then should be set by the HUD perhaps
            // where we can update these parameters based on the toolbar options that are selected

            KeyCommon.Flags.EntityAttributes ignoredObjectTypes = KeyCommon.Flags.EntityAttributes.None;//  	KeyCommon.Flags.EntityAttributes.Region;
            
            PickParameters pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = accuracy,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };
            viewport.Context.PickParameters = pickParams;
                
        }

        #region IWorkspace Members
        public void Activate()
        {
 
        }

        /// <summary>
        /// Event occurs when TreeView selection of entity OR during entity mousepick event 
        /// in Scene. 
        /// </summary>
        /// <param name="pickResult"></param>
        public override void OnEntitySelected(Keystone.Collision.PickResults pickResult)
        {
        	base.OnEntitySelected(pickResult);
				
			// move our quicklook viewport to the selected entity
			if (mQuickLook3DViewportEnabled && mSelectedEntity != null)
			{
				float SCREEN_SPACE_RATIO = 0.8f;
            	mViewportControls[VIEWPORTS_COUNT - 1].Viewport.Context.Viewpoint_MoveTo (mSelectedEntity, SCREEN_SPACE_RATIO, false);
			}
			
			 //                            
            ContextualizeToolbar(mPickResult.Entity, mPickResult.Geometry );
            //mouse.Viewport.Context.Scene.DebugPickLine = new Keystone.Types.Line3d(mPickResult.PickOrigin, mPickResult.PickEnd);
            // call ContextualizeHUDs even if .Selected == null this way we can clear hud menus
            ContextualizeHUDs(pickResult.Context, pickResult.vpRelativeMousePos, mPickResult);

        }

        private void ContextualizeToolbar (Keystone.Entities.Entity entity, Keystone.Elements.Geometry geometry)
        {
        	// if null then we switch to default
        	if (entity == null)
        	{
        		mViewportControls[0].SetToolbar (null);
        		return;
        	}
        	
        	// TODO: this should be done from the Entity's Script itself... not based on If/Then or Switch on type.
        	// if terrain, we switch to terrain editing specific toolbar
        	if (geometry is Keystone.Elements.Terrain)
	        	this.InitializeTerrainToolbar();
        	
        	// if editable mesh, switch to that
        	else if (geometry is Keystone.Elements.Mesh3d)
        		mViewportControls[0].SetToolbar (null);
        }
        
        private void ContextualizeHUDs(Keystone.Cameras.RenderingContext context, System.Drawing.Point vpRelativeMousePos, Keystone.Collision.PickResults pickResults)
        {
            // instead of notify, perhaps we can directly manipulate a HUD toolbar?
            // but the downside there is, not all huds would have\need such a thing i suspect.
            // further different huds may have different toolbar reqts.  
            // All we really need is a better naming convention over 
            // this "NotifyHUDs" what we're doing is sending a message to the HUD
            // of some selection change.

            // TODO: we should notify all HUDs that are open in this workspace.  
            if (mViewportControls != null && mViewportControls.Length > 0)
                for (int i = 0; i < mViewportControls.Length ; i++)
                    if (mViewportControls[i] != null && 
                        mViewportControls[i].Viewport.Context.Hud != null &&
                        mViewportControls[i].Viewport.Context == context)
                        context.Hud.ContextualizeMenu(context, vpRelativeMousePos, pickResults);
        }

        public override void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            // todo: if the entity being removed is the localVehicle we need to remove it from AppMain.mLocalVehicle and AppMain.mLocalVehicleID
            // notify all workspaces or perhaps these SceneNodeRemovedHandler
            // and AddedHandler exists in WorkspaceManager
            RemoveTreeItem item = RemoveTreeItemHandler;
            if (mTreeEntityBrowser.InvokeRequired)
                mTreeEntityBrowser.BeginInvoke(item, parent, child);
            else
                RemoveTreeItemHandler(parent, child);
        }

        public override void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {

            lock (mSyncLock)
            {
                AddTreeItem item = AddTreeItemHandler;
                if (mTreeEntityBrowser.InvokeRequired)
                {
                    mTreeEntityBrowser.BeginInvoke(item, parent, child);
                }
                else
                    AddTreeItemHandler(parent, child);
            }

            // TODO: we need to assign a user's Vehicle that's added
            //		the problem here is, its only this particular workspace... even though it's the primary workspace
            //		it's still not all of them, does it need to be?
            //		We can make an assignment to AppMain from here?
            if (child as Keystone.Vehicles.Vehicle != null)
            {
                // is it user vehicle or NPC? I think we need to set via right mouse click menu on the treeview the PlayerControlled flag and iterate through all other Vehicles and set PlayerControlled = false.
                Keystone.Vehicles.Vehicle vehicle = (Keystone.Vehicles.Vehicle)child;
                if (vehicle.GetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.PlayerControlled))
                {
                    // todo: when are we assigning the PlayerControlled flag? 
                    // is it local player's vehicle or remote
                    // how do we differentiate?
                    // - we can apply compare the Entity.ID with the "tag" on client/connection?
                    // - when we generate that vehicle, we can assign the entityID to connection/client tag
                    //	 - and that tag/ID needs to be stored in client config
                    //	- we need to make sure the server generates the ID for our Vehicle


                    AppMain.mPlayerControlledEntity = (Keystone.Vehicles.Vehicle)child;
                    AppMain.mPlayerControlledEntityID = child.ID;
                }
            }

            // TODO: there's an issue here with prefabs using textures that are in the archive... trying to add an icon for the prefab
            // using the resourcepath that is a ImportLib.ResourceDescriptor to an archvied image is not working here yet.
            //if (child is Keystone.Appearance.Texture)
            //{
            //    string filepath = ((Keystone.Appearance.Texture)child).ResourcePath;
            //    if (!galleryContainerTextures.SubItems.Contains(filepath))
            //    {
            //        DevComponents.DotNetBar.ButtonItem buttonItem = CreateGalleryButtonItem(filepath,
            //            Path.GetFileName(filepath), filepath, "", filepath, 32, 24, true);

            //        galleryContainerTextures.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            //        buttonItem});
            //    }
            //}
        }

        public override void Show()
        {
            base.Show();  // ViewportControls Enabled = true in call to base.Hide() 


            // Then we only need to instance all controls that might be placed
            // on various dock containers. 
            //

            // i believe the control assignments are the main thing that's not serialized that
            // we need to set.  So we have to grab a copy of the DockContainerItem's 
            // and doc.Control = whatever...
            // A way to accomplish this is to in fact have this View class create those container items
            // in the first place using pre-defined names such as 
            // evdci_  (editor view dock container item)
            // "evdci_Viewport0"
            // "evdci_Viewport1"
            // "evdci_Viewport2"
            // "evdci_Viewport3"
            // 
            mDocument.Visible = true;
            mDocument.Enabled = true;

            if (mQuickLookHostPanelInitialized == false)
            {
                //// TODO: This panel is not being implemented by floorplan workspace?
                //// TODO: all initialization of controls should be done elsewhere and not in constructor
                ////       because inheriting classes will end up calling these unnecessarily

                //if (mQuickLookHostPanel == null)
                //{
                //    mQuickLookHostPanel = new KeyEdit.GUI.QuickLookHostPanel();

                //    mQuickLookPanel = new GUI.QuickLook3(AppMain.PluginService);
                //    this.mQuickLookHostPanel.Controls.Add(mQuickLookPanel);
                //    mQuickLookPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                //}

                //// quick look panel will also have a viewport
                //if (mQuickLook3DViewportEnabled)
                //{
                //    QuickLookViewportInitialization(VIEWPORTS_COUNT);
                //}

                //((WorkspaceManager)mWorkspaceManager).DockControl(mQuickLookHostPanel, "Quick Look Host", "leftDockSiteBar", "Quick Look Host", eDockSide.Left);
                //mQuickLookHostPanelInitialized = true;
            }
            
            if (mToolBoxInitialized == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mToolBox, "Toolbox", "leftDockSiteBar", "Toolbox", eDockSide.Left);
                mToolBoxInitialized = true;
            }

            // note: we create a new asset browser so that each view can maintain it's own
            // memory of the asset browser it last used.  Game Build mode for instance will also
            // have different assetbrowser filter settings compared to regular edit mode used
            // to design Mods. (which is not available to users)
            if (mAssetBrowserDocked == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mBrowser, "Asset Browser", "leftDockSiteBar", "Asset Browser", eDockSide.Left);
                mAssetBrowserDocked = true;
            }

            
            // NOTE: The above relies on the existance of a DockContainerItem that will be restored
            // from LAYOUT.  The below relies on creation of a new dockItem.  So if we cannot
            // find a previous dockContainerItemRight for example
            if (mTreeViewDocked == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mTreeEntityBrowser, "Scene Explorer", "leftDockSiteBar", "Scene Explorer", eDockSide.Left);
                mTreeViewDocked = true;
            }
        }    

        public override void Hide()
        {
            base.Hide(); // ViewportControls Enabled = false in call to base.Hide()

        
            // loading a new definition before we remove these viewports will result in these viewports
            // being disposed.  So we remove these controls from the DockContainerItem first
            //  DockContainerItem container = FindDockContainer("docDock1");
            //System.Diagnostics.Debug.Assert(container.Control == mViewportControls[0]);
            //container.Control = null;
            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = false;
                }

            mDocument.Visible = false;
            mDocument.Enabled = false;

            // undock the mBrowser, toolbox and scene explorer
            if (mQuickLookHostPanelInitialized == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Quick Look Host", "leftDockSiteBar");
                mQuickLookHostPanelInitialized = false;
            }

            if (mToolBoxInitialized == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Toolbox", "leftDockSiteBar");
                mToolBoxInitialized = false;
            }

            if (mAssetBrowserDocked == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Asset Browser", "leftDockSiteBar");
                mAssetBrowserDocked = false;
            }

            if (mTreeViewDocked == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Scene Explorer", "leftDockSiteBar");
                mTreeViewDocked = false;
            }
        }
        #endregion
    }
}
