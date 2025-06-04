using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using KeyEdit.Controls;
using Keystone.Cameras;
using Keystone.Types;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Portals;
using Keystone.EditTools;
using KeyCommon.Traversal;

namespace KeyEdit.Workspaces
{
    public class ComponentDesignWorkspace : EditorWorkspace  
    {
        private Entity mComponent;      // our scene connected Component.  We replace the BlendingModes and Materials to use a transparent Material and BlendingMode here and restore them when exiting the workspace.
        //private Entity mComponentClone; // used to cache the original blendingModes and materials. This component is not connected to the Scene and thus isn't rendered. 
        //private Huds.ComponentHud mSharedHud;
        private List<Keystone.Appearance.Material> mMaterials;
        private List<Keystone.Appearance.GroupAttribute> mAppearances;
        private float[] mOriginalOpacities;
        private MTV3D65.CONST_TV_BLENDINGMODE[] mOriginalBlendingModes; // Appearance blending modes

        public uint FootPrintWidth = 16; // for components, these can be arbitrary dimensions > 0
        public uint FootPrintHeight = 16; // for components, these can be arbitrary dimensions > 0
        internal float TileWidth = 2.5f / 16f;
        internal float TileHeight = 2.5f / 16f;
        
        // TODO: I should debug draw the width and depth on lower left of viewport so we know what it is after changing them
        public float GridWidth;
        public float GridDepth;

        // TODO: i need an interface were we can assign TILEMASK_FLAGS. not every component is floor mounted.
        private Interior.TILE_ATTRIBUTES mFlags = Interior.TILE_ATTRIBUTES.COMPONENT;
        private BrushSize mBrushSize;

        // todo: shouldn't this get passed in the Interior so we know the tile width and cell sizes?
        // and how do i restrict placement to just 16x16 full cells with no overlap in between  other cells? for things like lifts and ladders
        public ComponentDesignWorkspace(string name, Entity target, float tileWidth, float tileDepth) 
            : base(name)
        {
            if (target == null) throw new ArgumentNullException();

            mComponent = target;

            mDisplayMode = KeyEdit.Controls.ViewportsDocument.DisplayMode.TripleLeft ;

            System.Diagnostics.Debug.Assert(tileWidth > 0 && tileDepth > 0);
            TileWidth = tileWidth;
            TileHeight = tileDepth;
            // NOTE: we don't share the HUD because per viewport 
            // Immediate2D calls need to be offset per camera position
            // and that can't be done from a shared hud.  However
            // we do share the static var mFootPrintGridEntity so that
            // painting to it from any viewport appears in all viewports.
            //mSharedHud = new Huds.ComponentHud(this);

            // default is 16x16 tiles but can be increased/decreased in Component Editor workspace
            FootPrintWidth = 16;
            FootPrintHeight = 16;

            CacheMaterialsAndBlendingModes(mComponent);
        }

        
        private void InitializeFootprint()
        {
            if (mComponent.Footprint == null)
            {
                ResizeEntityFootprint(FootPrintWidth, FootPrintHeight);

                // TODO: need an X button on toolbar to delete footprint and
                // close the workspace prompting user "Delete the Footprint for this Component and close Footprint Editor?"

            }
            else
            {
                // set the initial state of the footprint 
                FootPrintWidth = (uint)mComponent.Footprint.Width;
                FootPrintHeight = (uint)mComponent.Footprint.Depth;

                // ensure footprints are always in multiple 2 (unless they are 0)
                if (FootPrintWidth % 2 != 0)
                    FootPrintWidth++;

                if (FootPrintHeight % 2 != 0)
                    FootPrintHeight++;
                
                InitializeFootprintSize(FootPrintWidth, FootPrintHeight);
            }
        }

        private void InitializeFootprintSize(uint width, uint height)
        {
            FootPrintWidth = width;
            FootPrintHeight = height;
            GridWidth = FootPrintWidth * TileWidth;
            GridDepth = FootPrintHeight * TileHeight;

            //mSharedHud.SetFootprintVisualizationSize(FootPrintWidth, FootPrintHeight);
            // NOTE: we use a shared hud across all viewports so no need to iterate
            // and update footprint size on seperate huds.
            // NOTE: we initially tried using a shared hud, but then viewport specific
            // Immediate2D renders that need to be offset independantly based on each
            // viewports' camera position failed. So we're back to independant Huds.
            if (mViewportControls != null)
                foreach (ViewportControl c in mViewportControls)
                {
                    if (c != null)
                    {
                        Huds.ComponentHud hud = (Huds.ComponentHud)c.Viewport.Context.Hud;
                        hud.SetFootprintVisualizationSize(FootPrintWidth, FootPrintHeight);
                    }
                }
        }

        // NOTE: HUD is responsible for actual rendering of the 
        private void ResizeEntityFootprint(uint width, uint height)
        {
            InitializeFootprintSize(width, height);

            // footprints are created as resources similar to Entity Scripts
            // but instead of a resource path, the "footprint" property
            // stored in Entity is a string that holds the compressed
            // footprint resource.  Thus these footprints can easily be shared
            // because the resulting string is always same for identical painted
            // footprints
            KeyCommon.Messages.Node_ChangeProperty createFootPrint =
                new KeyCommon.Messages.Node_ChangeProperty(mComponent.ID);

            // TODO: actually null string will cause the footprint to be deleted
            // so we dont need to send this at all, but it's useful for reminding us
            // that a footprint is only stored as a property of Entity but then it is shared
            // by naming convention where all footprints with the same name have the same
            // layout and painted tiles.
            string encodedData = CellFootprint.Encode(width, height);
#if DEBUG
            int[,] testDecompress = CellFootprint.Decode(encodedData);
#endif
            createFootPrint.Add("footprint", typeof(string).Name, encodedData);

            AppMain.mNetClient.SendMessage(createFootPrint);
        }

        // todo: when this workspace become active again, i could always re-create this cache
        // in case the original prefab was modified in the Editor workspace
        private void CacheMaterialsAndBlendingModes(Keystone.Entities.Entity entity)
        {
            mMaterials = new List<Keystone.Appearance.Material>();
            mAppearances = new List<Keystone.Appearance.GroupAttribute>();

            Predicate<Keystone.Elements.Node> match = (Keystone.Elements.Node node) => { return (node is Keystone.Appearance.Material || node is Keystone.Appearance.GroupAttribute); };

            Keystone.Elements.Node[] nodes = Keystone.Scene.Scene.GetDescendants((Keystone.Elements.IGroup)entity, match);

            if (nodes == null) return;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] is Keystone.Appearance.Material)
                {
                    mMaterials.Add((Keystone.Appearance.Material)nodes[i]);
                }
                else if (nodes[i] is Keystone.Appearance.GroupAttribute)
                {
                    mAppearances.Add((Keystone.Appearance.GroupAttribute)nodes[i]);
                }
                else throw new Exception("Matches should not return nodes that are not of type Material or GroupAttribute");
            }

            if (mMaterials.Count > 0)
            {
                mOriginalOpacities = new float[mMaterials.Count];
                for (int i = 0; i < mMaterials.Count; i++)
                    mOriginalOpacities[i] = mMaterials[i].Opacity;
            }

            if (mAppearances.Count > 0)
            {
                mOriginalBlendingModes = new MTV3D65.CONST_TV_BLENDINGMODE[mAppearances.Count];
                for (int i = 0; i < mAppearances.Count; i++)
                    mOriginalBlendingModes[i] = mAppearances[i].BlendingMode;
            }
        }

        public Entity Component
        { 
            get { return mComponent; } 
        }

        public ModeledEntity Visualization 
        { 
            get
            {
                return null;
                //return mHud.TileVisualization;
            } 
        }
        
        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {  
            //base.Configure(manager, scene); // do not configure here or it will duplicate

            if (manager == null) throw new ArgumentNullException("ComponentDesignWorkspace.Configre() - WorkspaceManager parameter is NULL.");
            if (scene == null) throw new ArgumentNullException();
            mScene = scene;
            mWorkspaceManager = manager;

           // mWorkspaceManager.LoadLayout();

            // NOTE: dim the mViewportControls array before mDocument creation 
            // NOTE: We use FOUR viewports even though we'll only utilize 3 but which 3
            //       subscripts vary depending if we are trip view LEFT or trip view RIGHT
            mViewportControls = new KeyEdit.Controls.ViewportControl[4];

            // create our viewports document.  only need 1 to hold all 4 viewportControls
            mDocument = new KeyEdit.Controls.ViewportsDocumentWithToolbar(this.Name, mDisplayMode, 
                OnViewportOpening, OnViewportClosing);


            InitializeFootprint();

            WireDocumentEvents();
			mDocument.Resize += base.OnDocumentResized;
			
            BuildBrushMenu();
            // init mFlags after default Brush Menu state has been set in BuildBrushMenu()
            mFlags = Interior.TILE_ATTRIBUTES.COMPONENT;

            ConfigureDefaultBrushSettings();

            // ConfigureLayout will initialize the viewportcontrols but msut be called 
            // AFTER .CreateWorkspaceDocumentTab()
            //((KeyEdit.Controls.ViewportsDocumentWithToolbar)mDocument).ConfigureLayout(mDisplayMode);

            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, OnGotFocus);


            Show();
            this.Resize();
        }

        DevComponents.DotNetBar.SliderItem sliderItemOpacity;

        private void WireDocumentEvents()
        {
            KeyEdit.Controls.ViewportsDocumentWithToolbar doc = (KeyEdit.Controls.ViewportsDocumentWithToolbar)mDocument;

            string floodFillIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\largeImage.png");
            string incrementWidthIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\incrementwidth.png");
            string decrementWidthIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\decrementwidth.png");
            

            string incrementHeightIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\manualImage.png"); //size_vertical.png";
            string decrementHeightIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\flattenImage.png");
            string eraserIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\selectImage.png"); //draw_eraser.png";
            string drawIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\paintImage.png"); //draw_calligraphic.png";
            string opacityIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\alphaImage.png");//\color_gradient.png";

            System.Drawing.Image image = System.Drawing.Image.FromFile(floodFillIcon);
            doc.AddToolbarButton("buttonFillFootprint", image, OnButtonFillFootprint_Click);
            

            image = System.Drawing.Image.FromFile(drawIcon);
            doc.AddToolbarButton("buttonPaintFootprint", image, OnButtonPaintFootprint_Click);
            image = System.Drawing.Image.FromFile(eraserIcon);
            doc.AddToolbarButton("buttonEraseFootprint", image, OnButtonEraseFootprint_Click);

            image = System.Drawing.Image.FromFile(incrementWidthIcon);
            doc.AddToolbarButton("buttonIncrementFootprintWidth", image, OnButtonIncrementFootprintWidth);
            image = System.Drawing.Image.FromFile(decrementWidthIcon);
            doc.AddToolbarButton("buttonDecrementFootprintWidth", image, OnButtonDecrementFootprintWidth);

            image = System.Drawing.Image.FromFile(incrementHeightIcon);
            doc.AddToolbarButton("buttonIncrementFootprintHeight", image, OnButtonIncrementFootprintHeight);
            image = System.Drawing.Image.FromFile(decrementHeightIcon);
            doc.AddToolbarButton("buttonDecrementFootprintHeight", image, OnButtonDecrementFootprintHeight);

            // transparency slider so that the component does not obscure the footprint
            image = System.Drawing.Image.FromFile(opacityIcon);
            DevComponents.DotNetBar.ButtonItem buttonItemOpacityDropDown =
                doc.AddToolbarButton("buttonItemOpacityDropDown", image, OnButtonIncrementFootprintHeight);
            buttonItemOpacityDropDown.SubItemsExpandWidth = 14;

            sliderItemOpacity = new DevComponents.DotNetBar.SliderItem();
            sliderItemOpacity.SliderOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            sliderItemOpacity.Value = 30;
            sliderItemOpacity.ValueChanged += new System.EventHandler(this.sliderOpacity_ValueChanged);

            // add slider to dropdown
            buttonItemOpacityDropDown.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
                                                        sliderItemOpacity});
        }


        private void ConfigureDefaultBrushSettings()
        {
            // TODO: we need to map colors to the flags and we need to redo flags in TILEMASK_FLAGS so that they are simpler without so many combined | flag elements.
            //       We do not want to use the flag bits themselves to determine color.  That
            //       will result in unclear visualizations.
            mBrushSize = BrushSize.Size_1x1;
        }

        #region Brush Menus
        private void BuildBrushMenu()
        {
            // TODO: eww, hardcoded paths
            KeyEdit.Controls.ViewportsDocumentWithToolbar doc = (KeyEdit.Controls.ViewportsDocumentWithToolbar)mDocument;
            string drawIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\squareImage.png");// cog.png";
            System.Drawing.Image image = System.Drawing.Image.FromFile(drawIcon);
            ButtonItem EditMenu = doc.AddToolbarButton("buttonEditMenu", image, OnButtonEditMenu_Click);
            ButtonItem child = doc.AddToolbarButton(EditMenu, "buttonClearAllData", image, OnButtonClearData_Click);
            child.Text = "&Clear All Data";
            child = doc.AddToolbarButton(EditMenu, "buttonSearchReplace", image, OnButtonSearchReplace_Click);
            child.Text = "Search &Replace";


            drawIcon = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\icons\terrainedit\squareImage.png"); //draw_calligraphic.png";
            image = System.Drawing.Image.FromFile(drawIcon);

            // button dropdown to select brush size
            ButtonItem brushSizeSelectMenu = doc.AddToolbarButton("buttonBrushSizeSelect", image, OnButtonBrushSizeSelect_Click);
            child = doc.AddToolbarButton(brushSizeSelectMenu, "buttonSelectBrush1x1", image, OnButtonBrushSizeSelect_Click); 
            child.Text = "1x1";
            child.Checked = true;
            

            child = doc.AddToolbarButton(brushSizeSelectMenu, "buttonSelectBrush2x2", image, OnButtonBrushSizeSelect_Click);
            child.Text = "2x2";
            child = doc.AddToolbarButton(brushSizeSelectMenu, "buttonSelectBrush4x4", image, OnButtonBrushSizeSelect_Click);
            child.Text = "4x4";

            // button dropdown for selecting bitflag layer to edit
            ButtonItem brushSelectMenu = doc.AddToolbarButton("buttonBrushSelect", image, OnButtonBrushSelect_Click);
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectFloorComponent", image, OnButtonFlagSelect_Click);
            child.Text = "Floor Component";
            child.Checked = false;

            //child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectObstacle", image, OnButtonFlagSelect_Click);
            //child.Text = "Obstacle";

            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectTraversableComponent", image, OnButtonFlagSelect_Click);
            child.Text = "Traversable Component";

            // stairs
            child = doc.AddToolbarButton (brushSelectMenu, "buttonSelectStairsLowerLanding", image, OnButtonFlagSelect_Click);
            child.Text = "Stair Lower Landing";
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectStairsUpperLanding", image, OnButtonFlagSelect_Click);
            child.Text = "Stair Upper Landing";

            // doors
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectAccessibleDoor", image, OnButtonFlagSelect_Click);
            child.Text = "Doorway";
            // ladder
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectLadderLowerLanding", image, OnButtonFlagSelect_Click);
            child.Text = "Ladder Lower Landing";
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectLadderUpperLanding", image, OnButtonFlagSelect_Click);
            child.Text = "Ladder Upper Landing";

            // doors
            //child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectAccessibleDoor", image, OnButtonFlagSelect_Click);
            //child.Text = "Access Door";

            // door & elevator access // NOTE: I think for now, a seperate door flag is not needed. Doors and Elevators use same flags.
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectEnterExitTriggerArea", image, OnButtonFlagSelect_Click);
            child.Text = "Enter Exit Trigger Area";

            // structural support
            child = doc.AddToolbarButton(brushSelectMenu, "buttonSelectSupportConsumer", image, OnButtonFlagSelect_Click);
            child.Text = "Support Required";
            
        }

        private void OnButtonEditMenu_Click(object sender, EventArgs e)
        {
        }

        private void OnButtonSearchReplace_Click(object sender, EventArgs e)
        {
 
        }

        private void sliderOpacity_ValueChanged(object sender, EventArgs e)
        {
            mViewportControls[0].Viewport.Context.SetCustomOptionValue(null, "component opacity", (float)sliderItemOpacity.Value / 100f);
        }


        private void OnButtonBrushSizeSelect_Click(object sender, EventArgs e)
        {
            ButtonItem item = (ButtonItem)sender;
            switch (item.Name)
            {
                case "buttonSelectBrush1x1":
                    mBrushSize = BrushSize.Size_1x1;
                    break;
                case "buttonSelectBrush2x2":
                    mBrushSize = BrushSize.Size_2x2;
                    break;
                case "buttonSelectBrush4x4":
                    mBrushSize = BrushSize.Size_4x4;
                    break;
                default:
                    break;
            }

            ConfigurePaintBrush();
        }

        private void OnButtonBrushSelect_Click(object sender, EventArgs e)
        {
        }

        public int CurrentLayer; // corresponds to bit position within the bitflag
        private void OnButtonFlagSelect_Click(object sender, EventArgs e)
        {
            // NOTE: the actual colors used is assigned in KeyEdit.Workspaces.Huds.ComponentHud.CreateAtlasLookup().
            //       But it uses an index into an atlas texture.
            ButtonItem item = (ButtonItem)sender;
            bool isChecked = !item.Checked;
            item.Checked = isChecked;

            Interior.TILE_ATTRIBUTES flag = Interior.TILE_ATTRIBUTES.NONE;

            // deselect checked from every buttonItem in the dropdown except the current
            KeyEdit.Controls.ViewportsDocumentWithToolbar doc = (KeyEdit.Controls.ViewportsDocumentWithToolbar)mDocument;
            ButtonItem menuItem = doc.GetButtonItem("buttonBrushSelect");
            foreach (ButtonItem button in menuItem.SubItems)
            {
                if (button == item) continue;
                
                button.Checked = false;
            }

            switch (item.Name)
            {
                // OBSOLETE - we no longer use OBSTACLE - we use just WALL and COMPONENT for determining traversable areas
                //case "buttonSelectObstacle":
                //    flag = Interior.TILE_ATTRIBUTES.OBSTACLE;
                //    break;
                case "buttonSelectFloorComponent":
                    flag = Interior.TILE_ATTRIBUTES.COMPONENT;
                    break;
                // TODO: this is meant to be components attached to the wall (and not just a TILE_ATTRIBUTES.WALL), but we won't have any in version 1.0
                case "buttonSelectWallComponent":
                    flag = Interior.TILE_ATTRIBUTES.WALL;
                    break;
                //case "buttonSelectCeilingComponent":
                //    flag = Interior.TILEMASK_FLAGS.COMPONENT_CEILING_MOUNTED;
                //    break;
                case "buttonSelectTraversableComponent":
                    flag = Interior.TILE_ATTRIBUTES.COMPONENT_TRAVERSABLE;
                    break;
                case "buttonSelectStairsLowerLanding":
                    flag = Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING;
                    break;
                case "buttonSelectStairsUpperLanding":
                    flag = Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING;
                    break;
                case "buttonSelectLadderLowerLanding":
                    flag = Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING;
                    break;
                case "buttonSelectLadderUpperLanding":
                    flag = Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING;
                    break;
                case "buttonSelectEnterExitTriggerArea":
                    flag = Interior.TILE_ATTRIBUTES.ENTRANCE_AND_EXIT;
                    break;
                //case "buttonSelectAccessibleDoor":
                //    flag = Interior.TILE_ATTRIBUTES.ACCESSIBLE_THROUGH;
                //    break;
                //case "buttonSelectAttributePourous":
                //    flag = Interior.TILE_ATTRIBUTES.OBSTACLE_ATTRIBUTE_POUROUS;
                //    break;
                //case "buttonSelectAttributeTransparent":
                //    flag = Interior.TILE_ATTRIBUTES.OBSTACLE_ATTRIBUTE_TRANSPARENT;
                //    break;

                case "buttonSelectSupportProvider":
                    //flag = Interior.TILEMASK_FLAGS.SUPPORT_PROVIDER_VERTICAL;
                    break;
                default:
                    break;
            }

            CurrentLayer = (int)flag;

            // // NOTE: for less complexity, we're only setting one current flag at a time and we are not combining them. 
            // So we set initial mFlags to NONE
            mFlags = Interior.TILE_ATTRIBUTES.NONE;
            if (item.Checked)
                mFlags |= flag;
            else
                mFlags &= ~flag;

            ConfigurePaintBrush();
        }
        #endregion

        private void OnButtonFillFootprint_Click(object sender, EventArgs e)
        {
            int[,] data = mComponent.Footprint.Data;

            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    data[i, j] |= (int)mFlags;
                }

            CellFootprint fp = CellFootprint.Create(data);
            string footprintID = fp.ID; // encoded footprint is stored in the .ID.  We don't need to keep the actual CellFootprint node loaded

            Keystone.Resource.Repository.IncrementRef(fp);
            Keystone.Resource.Repository.DecrementRef(fp);

            mComponent.SetProperty("footprint", typeof(string), footprintID);

            // when we send the footprint, we only send the encoded string  because this is how footprints
            // are saved in Entity and then entity only creates a Footprint instance based on decoding the string.
            KeyCommon.Messages.Node_ChangeProperty changeProperty = new KeyCommon.Messages.Node_ChangeProperty(mComponent.ID);
            changeProperty.Add("footprint", typeof(string).Name, footprintID);
            //changeProperty.NodeID = mComponent.ID;

            AppMain.mNetClient.SendMessage(changeProperty);
        }

        private void OnButtonClearData_Click(object sender, EventArgs e)
        {
            ConfigurePaintBrush();
            ((KeyEdit.Workspaces.Tools.ComponentFootprintPainter)this.CurrentTool)
                .ClearData();

        }

        private void OnButtonPaintFootprint_Click(object sender, EventArgs e)
        {
            mApplyFlags = true;
            ConfigurePaintBrush();
        }

        bool mApplyFlags = true; // false for erasing the current selected brush flags
        private void OnButtonEraseFootprint_Click(object sender, EventArgs e)
        {   
            mApplyFlags = false;
            ConfigurePaintBrush();
        }

        private void ConfigurePaintBrush()
        {
            if (this.CurrentTool == null || this.CurrentTool is KeyEdit.Workspaces.Tools.ComponentFootprintPainter == false)
            {
                // NOTE: the actual painting is done by the ComponentFootprintPainter as it handles
                //       mouse button and movement events.  Here we are only configuring the values
                //       that this ComponentFootprintPainter is using.
                KeyEdit.Workspaces.Tools.ComponentFootprintPainter painter =
                               new KeyEdit.Workspaces.Tools.ComponentFootprintPainter(AppMain.mNetClient, this);

                this.CurrentTool = painter;
            }
            
            ((KeyEdit.Workspaces.Tools.ComponentFootprintPainter)this.CurrentTool)
                .SetValue("footprint", mFlags, mBrushSize, mApplyFlags);
        }

        private const uint FOOTPRINT_INCREMENT = 2;
        private void OnButtonIncrementFootprintWidth(object sender, EventArgs e)
        {
            FootPrintWidth += FOOTPRINT_INCREMENT;
            ResizeEntityFootprint(FootPrintWidth, FootPrintHeight);      
        }

        private void OnButtonDecrementFootprintWidth(object sender, EventArgs e)
        {
            FootPrintWidth -= FOOTPRINT_INCREMENT;
            if (FootPrintWidth <= 0)
                FootPrintWidth = 2; // TODO: should allow for 0 size which means no footprint

            ResizeEntityFootprint(FootPrintWidth, FootPrintHeight);
        }

        private void OnButtonIncrementFootprintHeight(object sender, EventArgs e)
        {
            FootPrintHeight += FOOTPRINT_INCREMENT;
            ResizeEntityFootprint(FootPrintWidth, FootPrintHeight);
        }

        private void OnButtonDecrementFootprintHeight(object sender, EventArgs e)
        {
            FootPrintHeight -= FOOTPRINT_INCREMENT;
            if (FootPrintHeight <= 0)
                FootPrintHeight = 2; // TODO: should allow for 0 size which means no footprint

            ResizeEntityFootprint(FootPrintWidth, FootPrintHeight);
        }


        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs e)
        {
            if (mViewportControls[e.ViewportIndex] == null)
            {
                ViewportControl c = 
                    new ViewportEditorControl(CreateNewViewportName((int)e.ViewportIndex));
                mViewportControls[e.ViewportIndex] = c;
                e.HostControl.Controls.Add(c);

                // NOTE: we cannot share HUDs between viewports because
                // camera space rendering of Immediate2D items will fail
                Huds.ComponentHud hud = new Huds.ComponentHud(this);
                ConfigureViewport(mViewportControls[e.ViewportIndex], hud);

                c.Viewport.BackColor = Color.RoyalBlue;
                c.Viewport.Context.CullingStart += OnCullingStart;
                c.Viewport.Context.CullingComplete += OnCullingCompleted;
                c.Viewport.Context.RenderingStart += OnRenderingStart;
                c.Viewport.Context.RenderingComplete += OnRenderingCompleted;

                // TODO: depending on left trip view or right trip view, ViewportIndex 0 or 1 is used
                // LEFT uses 0, 2, 3 RIGHT uses 0, 1, 2
                if (e.ViewportIndex == 0)
                {
                    c.Viewport.Context.ViewType = Viewport.ViewType.Free;
                    c.Viewport.Context.ProjectionType = Viewport.ProjectionType.Perspective;
                }
                else if (e.ViewportIndex == 1) // enabled only during RIGHT
                {
                    c.Viewport.Context.ViewType = Viewport.ViewType.Top;
                    c.Viewport.Context.ProjectionType = Viewport.ProjectionType.Orthographic;
                }
                else if (e.ViewportIndex == 2)
                {
                    c.Viewport.Context.ViewType = Viewport.ViewType.Front;
                    c.Viewport.Context.ProjectionType = Viewport.ProjectionType.Orthographic;
                    // TODO: customize the view controller's zoom multiplier and min / max zooms
                    //       and set the initial starting zoom
                }
                else if (e.ViewportIndex == 3) // enabled only during LEFT
                {
                    c.Viewport.Context.ViewType = Viewport.ViewType.Top;
                    c.Viewport.Context.ProjectionType = Viewport.ProjectionType.Orthographic;
                }

				
				// hud options
                c.Viewport.Context.AddCustomOption("", "component opacity", typeof(float).Name, 0.60f);

                // we use the main toolbar of the ViewportsDocumentWithToolbar
                // and not the individual toolbars of each ViewportControl
                c.ShowToolbar = false;

                ConfigurePicking(c.Viewport);
                ConfigureHUD(c.Viewport );
                

                // Viewpoints and ViewpointBehaviors
                // NOTE: is component editing dependant on server? if not viewpoints can have any name right?
                // Bind all viewports to a cloned default configured viewpoint
                string uniqueKey = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Entities.Viewpoint));
                uniqueKey = "viewpoint_Component_" + mComponent.ID + uniqueKey;

                Viewpoint vp = Viewpoint.Create(uniqueKey, mComponent.Region.ID);
                // TODO: added serializable = false on June.26.2014 - is correct?  
                vp.Serializable = false;

                Repository.IncrementRef(vp);
                // TODO: are we ever releasing these viewpoints when the workspace is closed?
                // TODO: should the act of binding incrementref?


                c.Viewport.Context.Bind(vp);

                Keystone.Behavior.Composites.Composite behaviorTree = CreateVPRootSelector();
                vp.AddChild(behaviorTree);
                mViewpointsInitialized = true;

            }
        }

        protected override void ConfigurePicking(Keystone.Cameras.Viewport viewport)
        {
            // set RenderingContext PickParameters appropriate for ComponentDesign window viewports
            
            // NOTE: Accuracy is used to determine the precision to use when testing if a hit occurs or not. 
            // (eg bounding box versus per face to determine hit)
            PickAccuracy accuracy = PickAccuracy.Tile; 

            // skip traversing of everything except the grid which has the HUD attribute set
            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes excludedObjectTypes = KeyCommon.Flags.EntityAttributes.AllEntityAttributes & ~KeyCommon.Flags.EntityAttributes.HUD;

            KeyCommon.Flags.EntityAttributes ignoredObjectTypes = KeyCommon.Flags.EntityAttributes.AllEntityAttributes & ~KeyCommon.Flags.EntityAttributes.HUD;

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

        /// <summary>
        /// RenderingContext specific rendering state overrides can be applied here
        /// to part of the scene or the entire scene.  for instance, we can temporarily make
        /// the exterior of a vehicle transparent for just one particular RenderingContext.
        /// </summary>
        /// <param name="context"></param>
        private void OnCullingStart(RenderingContext context)
        {

            // is a component target set?
            if (mComponent == null || mComponent is ModeledEntity == false) return;

            object tmpOpacity = context.GetCustomOptionValue(null, "component opacity");
            if (tmpOpacity == null)
                tmpOpacity = 0.5f;

            AssignTransparentMaterials((float)tmpOpacity);

            string s = FootPrintWidth.ToString() + "w x " + FootPrintHeight.ToString() + "h";
            int color = Keystone.Types.Color.Red.ToInt32();
            int left = 5;
            int top = context.Viewport.Height - 20;
            Keystone.Immediate_2D.Renderable2DText text = new Keystone.Immediate_2D.Renderable2DText(s, left, top, color, context.TextureFontID);
            context.Hud.AddHUDEntity_Immediate(context.Region, text);
        }

        private void AssignTransparentMaterials(float opacity)
        {
            for (int i = 0; i < mMaterials.Count; i++)
                mMaterials[i].Opacity = opacity;

            for (int i = 0; i < mAppearances.Count; i++)
                mAppearances[i].BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
        }

        private void RestoreTransparency()
        {
            for (int i = 0; i < mMaterials.Count; i++)
                mMaterials[i].Opacity = mOriginalOpacities[i];

            for (int i = 0; i < mAppearances.Count; i++)
                mAppearances[i].BlendingMode = mOriginalBlendingModes[i];
        }

        private void OnCullingCompleted(RenderingContext context)
        {

        }

        private void OnRenderingStart(RenderingContext context)
        {
        }


        private void OnRenderingCompleted(RenderingContext context)
        {
            // must cleanup any outstanding overrides that were.... ?? huh?
            if (mComponent == null || mComponent is ModeledEntity == false) return;

            ModeledEntity entity = (ModeledEntity)mComponent;
            if (entity == null) return;
            RestoreTransparency();
        }


        #region IWorkspace Members
        public override void Show()
        {
            //base.Show(); // NOTE: Do NOT call base.Show() as we do not want a new Scene Explorer treeview
            //             // We do need a new AssetBrowser though but it must be a custom one with different
            //             // filters.   We will want to hide the existing and show this when in this workspace active.


            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = true;
                }

            mDocument.Visible = true;
            mDocument.Enabled = true;

			mIsActive = true;
        }

        public override void Hide()
        {
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

			mIsActive = false;
        }
        #endregion
    }
}
