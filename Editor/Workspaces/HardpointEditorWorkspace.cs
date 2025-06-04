using System;
using System.Diagnostics;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using KeyCommon.Traversal;
using KeyEdit.Controls;
using Keystone.Resource;
using Silver.UI;
using Keystone.Entities;
using Keystone.Types;
using Keystone.Elements;
using Keystone.Appearance;

namespace KeyEdit.Workspaces
{
    /// <summary>
    /// This workspace is very similar to the main EditorWorkspace but uses an orbit viewpoint behavior and the component browser only shows
    /// "hardpoints" as placeable.  This workspace is only available when in "modding" mode and is not used during "refit" mode at game time.
    /// </summary>
    public partial class HardpointEditorWorkspace : WorkspaceBase3D
    {
        #region constants
        private const int TOOLBOX_ID_HARDPOINT = 1;
        #endregion

        uint VIEWPORTS_COUNT = 1;

        // gui controls used in this workspace
        private TreeView mTreeHardpointsBrowser;
        private Silver.UI.ToolBox mToolBox; //hardpoint types
        bool mTreeViewDocked = false;
        bool mToolBoxInitialized = false;

        Keystone.Vehicles.Vehicle mVehicle;
        Keystone.Entities.Entity[] mHardpoints;


        public HardpointEditorWorkspace(string name, Keystone.Vehicles.Vehicle vehicle)
            : base(name)
        {
            if (vehicle == null) throw new ArgumentNullException();
            mVehicle = vehicle;


            // or do we click View->Hardpoint Editor with the selected Vehicle set similar to how we do View->Floorplan Designer
            // - yes, we will follow this path and just use this specific workspace.
            // - when not in modding mode, we hide the view->menu item


            // todo: i need tooltip to show over assetbrowser images because ive disabled the caption. or need to update the image preview icon inside the prefabs
            // TODO: I don't need an assetbrowser, just toolbox that has the hardpoint types listed.  Treeview is same as EditorWorkspace
            //       but it only shows the hardpoints installed under the vehicle.
            InitializeToolBox();
            // InitializeTreeview();
        }

        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            try
            {
                // NOTE: Dont call base.Configure because it'll instantiates a viewportcontrol we dont want here
                //base.Configure(manager); 

                if (manager == null) throw new ArgumentNullException("HardpointEditorWorkspace.Configre() - ERROR: WorkspaceManager parameter is NULL.");
                if (scene == null) throw new ArgumentNullException("HardpointEditorWorkspace.Configre() - ERROR: scene cannot be NULL.");

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


                InitializeViewpoint(mVehicle.Region.ID);
                AttachVehicle(mVehicle);

                if (mDocument.InvokeRequired)
                    mDocument.Invoke((System.Windows.Forms.MethodInvoker)delegate { Show(); });
                else
                    Show();


                this.Resize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("HardpointEditorWorkspace.Configure() - ERROR: " + ex.Message);
            }
        }


        protected override void ConfigureHUD(Keystone.Cameras.Viewport viewport)
        {
            if (viewport != null)
                if (viewport.Context.Hud != null)
                {
                    // space our rows and colums so that entire grid is same width and depth as a Zone
                    viewport.Context.Hud.Grid.InfiniteGrid = false;
                    viewport.Context.Hud.Grid.UseFarFrustum = false;
                    viewport.Context.Hud.Grid.AutoScale = false;
                    viewport.Context.Hud.Grid.DrawAxis = false;
                    viewport.Context.Hud.Grid.OuterRowCount = 40;
                    viewport.Context.Hud.Grid.RowSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterRowCount);
                    viewport.Context.Hud.Grid.OuterColumnCount = 40; // todo: all these row and column counts and spacing should be read from Settings
                    viewport.Context.Hud.Grid.ColumnSpacing = 1f; // (int)(mScaledZoneDiameter / mViewportControls[0].Viewport.Context.Hud.Grid.OuterColumnCount);

                    viewport.Context.Hud.Grid.InnerColor = new Keystone.Types.Color(45, 57, 86, 255); // todo: colors needs to be read from Settings
                    Keystone.Types.Color outerColor = new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey 
                    viewport.Context.Hud.Grid.OuterColor = outerColor; // new Keystone.Types.Color(69, 83, 114, 255);


                    viewport.Context.AddCustomOption(null, "show axis indicator", typeof(bool).Name, true);
                    viewport.Context.AddCustomOption(null, "show hardpoint icons", typeof(bool).Name, true);
                }


            //// TODO: i should  always read these values from ini 
            //// and do so via vpControl.ReadSettings (iniFile)
            //viewport.Cursor = AppMain._core.PrimaryContext.Viewport.Cursor;
            //viewport.BackColor = AppMain._core.PrimaryContext.Viewport.BackColor;
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
            //mViewportControls[0].Viewport.Context.Viewpoint.CustomData.SetDouble("orbit_radius_min", 125d);
            // this.mTacticalPlugin.Instance.SelectTarget(vehicle.Scene.Name, vehicle.ID, vehicle.TypeName);
        }

        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs args)
        {

            if (mViewportControls[args.ViewportIndex] == null)
            {
                ViewportEditorControl c =
                    new ViewportEditorControl(CreateNewViewportName((int)args.ViewportIndex));
                c.ShowToolbar = true;
                mViewportControls[args.ViewportIndex] = c;
                args.HostControl.Controls.Add(c);


                ConfigureViewport(c, new Huds.HardpointsHud(this));

                // "cam_speed" must always be set


                // Bind() to viewpoint which will give us the camera control functionality we need
                // TODO: i do not like binding viewpoint here.  I should be able to bind it and rebind it
                //       as I please elsewhere.

                ConfigureHUD(c.Viewport);
                ConfigurePicking(c.Viewport);
            }


        }

        protected override void ConfigurePicking(Keystone.Cameras.Viewport viewport)
        {
            // set RenderingContext PickParameters appropriate for viewport.Context


            // NOTE: Accuracy is used to determine the precision to use when testing if a hit occurs or not. 
            // (eg bounding box versus per face to determine hit)
            PickAccuracy accuracy = PickAccuracy.Face; // PickAccuracy.Geometry |
            //PickAccuracy.Vertex |
            //PickAccuracy.EditableGeometry;

            // skip traversing of interiors without recursing children
            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.HUD | KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Structure;

            // TODO: pickparameters should be set by the Hud for each RenderingContext 

            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.Region | KeyCommon.Flags.EntityAttributes.Root;

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

        private void InitializeViewpoint(string startingRegionID)
        {
            // server doesn't care about viewpoints (only the vehicle that it's bound to) we just generate a viewpoint here.
            //
            string id = Repository.GetNewName(typeof(Keystone.Entities.Viewpoint));
            Keystone.Entities.Viewpoint vp = (Keystone.Entities.Viewpoint)Repository.Create(id, "Viewpoint");
            vp.Serializable = false;

            Keystone.Behavior.Composites.Composite behavior = EditorWorkspace.CreateOrbitOnlyBehavior();
            vp.StartingRegionID = startingRegionID;
            vp.AddChild(behavior);
            vp.Update(0); // HACK - call Update(0) to initialize Viewpoint.CustomData

            mViewpointsInitialized = true;

            // bind to the context
            mViewportControls[0].Viewport.Context.Bind(vp);
            mViewportControls[0].Viewport.Context.Viewpoint.BlackboardData.SetString("focus_entity_id", mVehicle.ID);

            //          System.Diagnostics.Debug.Assert(AppMain.mLocalVehicleID == vehicle.ID);
            System.Diagnostics.Debug.Assert(vp == mViewportControls[0].Viewport.Context.Viewpoint);

        }

        private void InitializeToolBox()
        {
            mToolBox = new Silver.UI.ToolBox();
            mToolBox.AllowDrop = false; // can drag items off, but can't drop things onto
            mToolBox.BackColor = System.Drawing.SystemColors.Control;
            mToolBox.Dock = System.Windows.Forms.DockStyle.Fill;
            mToolBox.ItemHeight = 20;
            mToolBox.ItemHoverColor = System.Drawing.Color.BurlyWood;
            mToolBox.ItemNormalColor = System.Drawing.SystemColors.Control;
            mToolBox.ItemSelectedColor = System.Drawing.Color.Linen;
            mToolBox.ItemSpacing = 1;
            mToolBox.Location = new System.Drawing.Point(0, 0);
            mToolBox.Name = "_hardpoints_toolBox";
            //mToolBox.Size = new System.Drawing.Size(208, 405);
            mToolBox.TabHeight = 18;
            //mToolBox.SetImageList(GetImage("ToolBox_Small.bmp"), new System.Drawing.Size(16, 16), System.Drawing.Color.Magenta, true);
            //mToolBox.SetImageList(GetImage("ToolBox_Large.bmp"), new System.Drawing.Size(32, 32), System.Drawing.Color.Magenta, false);

            //mToolBox.RenameFinished += new RenameFinishedHandler(ToolBox_RenameFinished);
            //mToolBox.TabSelectionChanged += new TabSelectionChangedHandler(ToolBox_TabSelectionChanged);
            //mToolBox.ItemSelectionChanged += new ItemSelectionChangedHandler(ToolBox_ItemSelectionChanged);
            //mToolBox.TabMouseUp += new TabMouseEventHandler(ToolBox_TabMouseUp);
            //            mToolBox.ItemMouseUp += new ItemMouseEventHandler(ToolBox_ItemMouseUp);
            //mToolBox.OnDeSerializeObject += new XmlSerializerHandler(ToolBox_OnDeSerializeObject);
            //mToolBox.OnSerializeObject += new XmlSerializerHandler(ToolBox_OnSerializeObject);
            //mToolBox.ItemKeyPress += new ItemKeyPressEventHandler(ToolBox_ItemKeyPress);

            mToolBox.ItemMouseDown += ToolboxItem_Clicked;

            mToolBox.DeleteAllTabs(false);
            bool allowDrag = true;


            // Hardpoints tab
            int aiTabs = mToolBox.AddTab("Hardpoints", -1);
            mToolBox[aiTabs].Deletable = false;
            mToolBox[aiTabs].Renamable = false;
            mToolBox[aiTabs].Movable = false;

            mToolBox[aiTabs].View = Silver.UI.ViewMode.List;
            mToolBox[aiTabs].AddItem("hardpoint", 0, 0, true, TOOLBOX_ID_HARDPOINT);

        }

        private void ToolboxItem_Clicked(object sender, EventArgs e)
        {
            if (sender is ToolBoxItem == false) return;
            if (((ToolBoxItem)sender).Object is int == false)
            {
                System.Diagnostics.Debug.WriteLine("HardpointEditorWorkspace.ToolboxItem_Clicked() - ERROR: itemID has unexpected type.");
                return;
            }

            int itemID = (int)((ToolBoxItem)sender).Object;
            switch (itemID)
            {

                case TOOLBOX_ID_HARDPOINT:
                    {
                        // todo: how do we save the hardpoints?  Is it part of the vehicle prefab?  I think it should be and same goes for the Entities mounted on the hardpoints.
                        // lets take this in steps.  click the object, and we create a new hardpoint from a hardpoint prefab with script, and have it follow the mouse and show red when invalid placement, green for valid placement.
                        AddHardpoint();
                        break;
                    }

                default:
                    break;
            }
        }

        #region hardpoints
        private void AddHardpoint()
        {
            // todo: in the toolbox add "single" enging emount and "dual" engine mount
            // this assigns the hardpoint to a transform tool.  
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            Mesh3d mesh = Mesh3d.CreateSphere(2.5f, 25, 25, false);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);
            Material material = Material.Create(Material.DefaultMaterials.green);
            appearance.RemoveMaterial();
            appearance.AddChild(material);
            

            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);
            entity.Name = "hardpoint";
            entity.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Hardpoint, true);

            ActivateBrush(entity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }
        #endregion

        #region Drag Drop from Toolbox to Workspace 
        private void dragContainer_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
        }

        private void dragContainer_DragLeave(object sender, System.EventArgs e)
        {
        }

        private void dragContainer_QueryContinueDrag(object sender, System.Windows.Forms.QueryContinueDragEventArgs e)
        {
        }


        private void dragContainer_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void dragContainer_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            ToolBoxItem dragItem = null;
            string strItem = "";

            if (e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
            {
                dragItem = e.Data.GetData(typeof(Silver.UI.ToolBoxItem)) as ToolBoxItem;

                if (null != dragItem && null != dragItem.Object)
                {
                    strItem = dragItem.Object.ToString();
                    ToolboxItem_Dropped((KeyEdit.Controls.ViewportControl)sender, dragItem.Object, e.X, e.Y);

                    //mToolBox.Focus();
                }
            }
        }
        #endregion

        private void ToolboxItem_Dropped(KeyEdit.Controls.ViewportControl ctl, object itemID, int mouseX, int mouseY)
        {
            // note: this function is not necessary for hardpoints. They are not dragged and dropped from Toolbox. instead we initialize it on mouse click and then assign placement tool with preview.
            return;

        }

        /// <summary>
        /// Add's Entities to Scene at runtime using networked message so that
		/// scene modifications only occur during update thread and never from
		/// Client.exe GUI thread.		Attempting to add entities directly to scene
		/// via parent.AddChild(entity) from Client.exe GUI thread will result in random
		/// crashes.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentTypeNameFilter"></param>
        /// <param name="overridePickedPosition"</param> 
        private void ActivateBrush(Entity entity, uint brushStyle, KeyCommon.Flags.EntityAttributes ignoredTypesFilter, KeyCommon.Flags.EntityAttributes excludedParentTypesFilter, bool overridePickedPosition)
        {
            // TODO: since we are using TEMP files to hold these prefabs, the problem is they wont be available
            // when trying to re-load a saved scene!  Therefore, the options are to 
            // a) save these to a prefab cache instead that gets associated with that particular Scene so that they are
            //    per-scene specific.
            // b) store all resources (eg meshes, textures, etc) required by these prefabs, in the common.zip or in some common
            // prefab_cache.zip 

            string prefabFilePath = Keystone.IO.XMLDatabase.GetTempFileName();
            prefabFilePath += ".kgbentity"; // MUST have .kgbentity or .kgbsegment extension to be recognized as non-nested-archive prefab database

            // TODO: temporarily lets just createa a sphere "hardpoint.kgbentity" ModeledEntity prefab and use that path
            // - we need a script for it? something that adds custom properties like "orientation", "location", "half-turret/full-turret", size, etc
            // todo: can we also place things like airlock doors, cargo doors, torpedo doors, docking ports, shuttle bay doors, etc, using the same technique?
            //       The doors themselves can be scripted entities that can open and close and we can snap their positions to align with interior decks.
            //       The cut outs for when the doors/bays are opeend really should be done on the exterior hull by a modeller and then limited to 3 meter intervals.
            //       Maybe google search blender cutting holes or something... and we want to be able to save the cutout for use as the exterior animated door / bay modeledEntity.
            //       Maybe this is a suitable problem for stencil buffer and punches through the geometry.
            Keystone.IO.XMLDatabaseCompressed xmldb = new Keystone.IO.XMLDatabaseCompressed();
            xmldb.Create(Keystone.Scene.SceneType.Prefab, prefabFilePath, entity.TypeName);
            xmldb.WriteSychronous(entity, true, true, false);
            xmldb.SaveAllChanges();
            xmldb.Dispose();


            string path = prefabFilePath;
            
            
            // TODO: can we pre-cache all toolbox items in a \prefabcache\ folder so they are ready to use quickly and dont have to be
            //       written then read back in
            KeyEdit.Workspaces.Tools.AssetPlacementTool placementTool = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                (
                    AppMain.mNetClient,
                    path,
                    brushStyle,
                    null
                );

            // TODO: can we pass a filter of parent types that are legal?  
            // TODO: im not using the below filters within the placement tool
            placementTool.IgnoredParentTypes = ignoredTypesFilter;
            placementTool.ExcludedParentTypes = excludedParentTypesFilter;
            //if (overridePickedPosition)
            //	placementTool.PlacementMode = Keystone.EditTools.PlacementMode.UseExistingPreviewEntityPosition;


            this.CurrentTool = placementTool;


        }

        public override void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            // todo: add to treeview if its a hardpoint
            return;
            throw new NotImplementedException();
        }

        public override void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            // todo: remove from treeview if its a hardpoint
            return;
            throw new NotImplementedException();
        }

        public override void Show()
        {
            base.Show();  // ViewportControls Enabled = true in call to base.Hide() 

            mDocument.Visible = true;
            mDocument.Enabled = true;

            

            if (mToolBoxInitialized == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mToolBox, "Toolbox", "leftDockSiteBar", "Toolbox", eDockSide.Left);
                mToolBoxInitialized = true;
            }

            
            // NOTE: The above relies on the existance of a DockContainerItem that will be restored
            // from LAYOUT.  The below relies on creation of a new dockItem.  So if we cannot
            // find a previous dockContainerItemRight for example
            if (mTreeViewDocked == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mTreeHardpointsBrowser, "Hardpoints Explorer", "leftDockSiteBar", "Hardpoints", eDockSide.Left);
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
            if (mToolBoxInitialized == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Hardpoints", "leftDockSiteBar");
                mToolBoxInitialized = false;
            }


            if (mTreeViewDocked == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Hardpoints Explorer", "leftDockSiteBar");
                mTreeViewDocked = false;
            }
        }
    }
}
