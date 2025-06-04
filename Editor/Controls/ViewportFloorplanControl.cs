using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.Editors;
using Keystone.Resource;
using Keystone.Portals;
using DevComponents.DotNetBar;

namespace KeyEdit.Controls
{
    public partial class ViewportFloorplanControl : ViewportControl
    {
        KeyEdit.Workspaces.FloorPlanDesignWorkspace mFloorPlanWorkspace;
        Keystone.Scene.Scene mScene;
        KeyEdit.Workspaces.FloorPlanDesignWorkspace.CategoryChangedHandler mCategoryChangedHandler;

        const int VEHICLE_STATE_HAS_INTERIOR = 2;
        const int VEHICLE_STATE_NO_INTERIOR = 1;


        public ViewportFloorplanControl() : base()
        {
            InitializeComponent();
        }

        public ViewportFloorplanControl(string name, 
            KeyEdit.Workspaces.FloorPlanDesignWorkspace floorPlanWorkspace, 
            Keystone.Scene.Scene scene, 
            KeyEdit.Workspaces.FloorPlanDesignWorkspace.CategoryChangedHandler categoryChangedHandler)
            : base(name)
        {
            if (floorPlanWorkspace == null || scene == null) throw new ArgumentNullException();

            InitializeComponent();

            mFloorPlanWorkspace = floorPlanWorkspace;
            mScene = scene;
            mCategoryChangedHandler = categoryChangedHandler;

            Keystone.Vehicles.Vehicle vehicle = mFloorPlanWorkspace.Vehicle;

            this.Refresh();
        }

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			ConfigureControlState ();
		}



        private void ConfigureControlState()
        {
        	Keystone.Vehicles.Vehicle vehicle = mFloorPlanWorkspace.Vehicle;
        	
            int state = -1;
            if (vehicle != null)
            {
                if (vehicle.Interior == null)
                    state = VEHICLE_STATE_NO_INTERIOR;
                else
                    state = VEHICLE_STATE_HAS_INTERIOR;
            }
            
            // TODO: if it's the same vehicle as last time, no need to reset the current floor
            numLevel.Minimum = 1;


            // Disable Arrow Keys for Floor Up/Down control and
            // Disable Text Entry for Floor Up/Down control
            numLevel.ReadOnly = true;
            numLevel.InterceptArrowKeys = false;
            ControlCollection controls = numLevel.Controls;
            //controls[1].Cursor = Cursors.Default;
            ////controls[1].Enabled = false;
            //System.Diagnostics.Debug.WriteLine (controls[1].GetType().Name) ;
            System.Windows.Forms.TextBox t = (System.Windows.Forms.TextBox)controls[1];
            //t.ReadOnly = false;
            //t.TabStop = false;
            //t.Cursor = Cursors.Default;
            t.Enabled = false; // seems to be only real way to do get rid of the ibeam caret without win32 api hidecaret
            //t.ForeColor = SystemColors.ControlText;
            //t.BackColor = SystemColors.AppWorkspace;

            //object o = numLevel.Cursor; 



            switch (state)
            {
                
                case (VEHICLE_STATE_NO_INTERIOR): // vehicle selected BUT interior is NOT generated
                    //buttonCreateFloor.Visible = false;
                    //buttonCreateFloor.Enabled = false;
                    buttonItemOpacityDropDown.Visible = false;
                    buttonItemOpacityDropDown.Enabled = false;
                    labelCurrentFloor.Visible = false;
                    labelCurrentFloor.Enabled = false;
                    buttonShowWallsMode.Visible = false;
                    buttonShowWallsMode.Enabled = false;
                    itemContainerFloorUpDown.Visible = false;
                    itemContainerFloorUpDown.Enabled = false;
                    numLevel.Value = this.Viewport.Context.Floor;
                    break;
                case (VEHICLE_STATE_HAS_INTERIOR): // vehicle selected AND interior IS generated
                default:
                    //buttonCreateFloor.Visible = true;
                    //buttonCreateFloor.Enabled = true;

                    buttonItemOpacityDropDown.Visible = true;
                    buttonItemOpacityDropDown.Enabled = true;
                    labelCurrentFloor.Visible = true;
                    labelCurrentFloor.Enabled = true;
                    buttonShowWallsMode.Visible = true;
                    buttonShowWallsMode.Enabled = true;
                    itemContainerFloorUpDown.Visible = true;
                    itemContainerFloorUpDown.Enabled = true;

                    // initialize the numLevel drop down with the number of floors in the interior
                    uint cellsLayers = ((Keystone.Portals.Interior)vehicle.Interior).CellCountY;

                    if (numLevel.Value > cellsLayers) numLevel.Value = cellsLayers;
                    numLevel.Maximum = cellsLayers;
                    // convert from internal 0 based floor to the 1 based floor that is set on the up/down control
                    int floor = this.Viewport.Context.Floor + 1;
                    numLevel.Value =floor;

                    // configure the overlay grid
                    ConfigureGrid(((Interior)vehicle.Interior).CellCountX,
                        ((Interior)vehicle.Interior).CellCountZ,
                        (float)((Interior)vehicle.Interior).CellSize.x,
                        (float)((Interior)vehicle.Interior).CellSize.z);

                    sliderItemOpacity.Value = (int)((float)this.Viewport.Context.GetCustomOptionValue(null, "exterior vehicle opacity") * 100f);
                    break;
            }

            this.Refresh();
        }

        //        private void VehicleSelect()
        //        {
        //            // TODO: this entire drop down should not be obsolete
        //            //       we now assign the Vehicle to the FloorplanWorkspace upon
        //            //       creation of the Workspace.  
        //            //  HOEVER, we so still need to have the ViewportFloorplanControl GUI state
        //            //  configured properly
        //            //Keystone.Entities.Entity vehicle = GetSelectedEntity();
        //            Keystone.Vehicles.Vehicle vehicle =
        //                ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
        //
        //            if (vehicle == null) 
        //            {
        //                throw new ArgumentNullException();
        //            }
        //            else if (vehicle is Keystone.Vehicles.Vehicle == false) 
        //                throw new Exception("ViewportFloorplanControl.cboVehicleSelect_Click() - unexpected type.");
        //
        //
        //            if (vehicle.Interior == null)
        //            {
        //                if (this.Viewport.Context.Viewpoint != null)
        //                    this.Viewport.Context.UnBind(this.Viewport.Context.Viewpoint);
        //
        //                ConfigureControlState(VEHICLE_STATE_NO_INTERIOR);
        //            }
        //            else
        //            {
        //                //this.Viewport.Context.Viewpoint.MoveTo ((Keystone.Portals.CelledRegion)mScene.GetResource(container.Interior.ID));
        //                // TODO: here if we've already created a vp for a particular vehicle interior
        //                //       we dont need to create it again OR we need to do a better job of removing it from
        //                //        Repository because deleting the interior celledregion to create a new attempts to
        //                //        recreate the viewpoint and causes Repository key alreadyexists exception
        //                // TODO: i dont like binding here from the control.  But im not sure where would be 
        //                // better place to bind the viewpoint at the moment
        //                Keystone.Elements.Viewpoint vp;
        //                string key = "viewpoint_floorplan_" + vehicle.Region.ID;
        //                vp = (Keystone.Elements.Viewpoint)Repository.Get (key);
        //                if (vp == null)
        //                    vp = new Keystone.Elements.Viewpoint(key, vehicle.Region.ID);
        //
        //                this.Viewport.Context.Bind(vp);
        //
        //                ConfigureControlState(VEHICLE_STATE_HAS_INTERIOR);
        //            }
        //
        //
        //            this.Refresh();
        //        }



        private void ConfigureGrid(uint columnCount, uint rowCount, float columSpacing, float rowSpacing)
        {
            // grid
            this.Viewport.Context.Hud.Grid.Enable = true;
            this.Viewport.Context.Hud.Grid.AutoScale = false;
            this.Viewport.Context.Hud.Grid.UseFarFrustum = false;
            this.Viewport.Context.Hud.Grid.InfiniteGrid = false;
            this.Viewport.Context.Hud.Grid.DrawInnerRows = false;
            this.Viewport.Context.Hud.Grid.InnerRowCount = 0;
            this.Viewport.Context.Hud.Grid.InnerColumnCount = 0;
            this.Viewport.Context.Hud.Grid.OuterRowCount = rowCount;
            this.Viewport.Context.Hud.Grid.OuterColumnCount = columnCount;
            this.Viewport.Context.Hud.Grid.RowSpacing = rowSpacing;
            this.Viewport.Context.Hud.Grid.ColumnSpacing = columSpacing;

            this.Viewport.Context.Hud.Grid.OuterColor = new Keystone.Types.Color(0.6f, 0.4f, 0.2f, 0.5f);
            this.Viewport.Context.Hud.Grid.OuterColor = Keystone.Types.Color.LightYellow;

        }

        //private void buttonCreateFloor_Click(object sender, EventArgs e)
        //{
        //    Keystone.Entities.Entity entity = GetSelectedEntity();

        //    if (entity == null)
        //    {
        //        MessageBox.Show("Entity not found.  Verify the entity has not been unloaded from the scene.");
        //        return;
        //    }


        //    // if no interior exists yet, exit
        //    Keystone.Entities.Container container = (Keystone.Entities.Container)entity;
        //    if (container.Interior == null)
        //    {
        //        MessageBox.Show("Vehicle must first contain an interior.");
        //        return;
        //    }


        //    // validate floor height is in acceptable range
        //    Keystone.Types.BoundingBox bounds = container.BoundingBox;

        //    float height = 1.0f;            

        //    AddFloor(container, height);                
            
        //}
 
        // TODO: technically right now, we should be in boundary painting stage
        // and not be showing ANY floors, just the grid.  This also means we are creating cells
        // within CelledRegion but not any segments.
        private void numLevel_ValueChanged(object sender, EventArgs args)
        {
            int floor = (int)numLevel.Value;
            floor -= 1; // convert to 0 based floor index
            this.Viewport.Context.Floor = floor;

            bool disableAllStructures = false;// "legal & illegal" != (string)this.Viewport.Context.GetCustomOptionValue(null, "cell grid visibility");
            this.mFloorPlanWorkspace.SetVisibleFloor (this.Viewport.Context, floor, disableAllStructures);
        }
                

        private void buttonShowFPS_Click(object sender, EventArgs e)
        {
            buttonShowFPS.Checked = !buttonShowFPS.Checked;
            this.Viewport.Context.ShowFPS = buttonShowFPS.Checked;
        }

        private void buttonShowProfiler_Click(object sender, EventArgs e)
        {
            buttonShowProfiler.Checked = !buttonShowProfiler.Checked;
            this.Viewport.Context.ShowTVProfiler = buttonShowProfiler.Checked;
        }

        private void buttonLabels_Click(object sender, EventArgs e)
        {
            buttonLabels.Checked = !buttonLabels.Checked;
            this.Viewport.Context.ShowEntityLabels = buttonLabels.Checked;
        }

        private void buttonShowBoundingBoxes_Click(object sender, EventArgs e)
        {
            buttonShowBoundingBoxes.Checked = !buttonShowBoundingBoxes.Checked;
            this.Viewport.Context.ShowEntityBoundingBoxes = buttonShowBoundingBoxes.Checked;
        }

        private void buttonSelectionMode_Click(object sender, EventArgs e)
        {
            this.Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.InteriorSelectionTransformTool);
            this.Viewport.Context.SetCustomOptionValue(null, "cell grid visibility", "none");
            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", false);
            
            this.mFloorPlanWorkspace.SetVisibleFloor(this.Viewport.Context, this.Viewport.Context.Floor, false);
            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler(null);
        }

        private void buttonPaintInteriorBounds_Click(object sender, EventArgs e)
        {
            KeyEdit.Workspaces.Tools.InteriorSegmentPainter painter = 
                new KeyEdit.Workspaces.Tools.InteriorSegmentPainter(AppMain.mNetClient);
            painter.SetValue ("boundaries",true); // inbounds
            this.Viewport.Context.Workspace.CurrentTool = painter;

            // TODO: what if i added a watch here rather than having to then call .SetVisibleFloor it would know
            //       when this option had changed
            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", true);
            this.Viewport.Context.SetCustomOptionValue(null, "cell grid visibility", "legal & illegal");
            this.mFloorPlanWorkspace.SetVisibleFloor (this.Viewport.Context, this.Viewport.Context.Floor, true);

            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler(null); // no atlas browsing in ImageBrowser required for bounds painting
        }        

        private void buttonEraseInteriorBounds_Click(object sender, EventArgs e)
        {
            KeyEdit.Workspaces.Tools.InteriorSegmentPainter painter = 
                new KeyEdit.Workspaces.Tools.InteriorSegmentPainter(AppMain.mNetClient);
            painter.SetValue ("boundaries", false); // out of bounds
            this.Viewport.Context.Workspace.CurrentTool = painter;

            // TODO: what if i added a watch here rather than having to then call .SetVisibleFloor it would know
            //       when this option had changed
            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", true);
            this.Viewport.Context.SetCustomOptionValue(null, "cell grid visibility", "legal & illegal");
            this.mFloorPlanWorkspace.SetVisibleFloor (this.Viewport.Context, this.Viewport.Context.Floor, true);
            
            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler(null); // no atlas browsing in ImageBrowser required for bounds painting
        }

        private void buttonPaintFloor_Click(object sender, EventArgs e)
        {
            KeyEdit.Workspaces.Tools.TileSegmentPainter painter = 
                new KeyEdit.Workspaces.Tools.TileSegmentPainter(AppMain.mNetClient);

            EdgeStyle style = new EdgeStyle();

            // floor using atlas texture index 4
            style.FloorAtlasIndex = 4; 
            // hardcode the footprint for this SegmentStyle
            int mask = (int)Keystone.Portals.Interior.TILE_ATTRIBUTES.FLOOR;
            style.BottomLeftFootprint = new int[,] 
                           {
                           {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},  
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask}};

            // ceiling using atlas texture index 0
            style.CeilingAtlasIndex = 0; 
            mask = (int)Keystone.Portals.Interior.TILE_ATTRIBUTES.CEILING;
            style.TopRightFootprint = new int[,]
            {
                           {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},  
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask}};

            // TODO: I think LayerNames should not be arrays. 
            painter.SetValue ("tile style", style); // creates a floor/ceiling segment and assigns texture atlas index 
            this.Viewport.Context.Workspace.CurrentTool = painter;

            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", true);
            this.Viewport.Context.SetCustomOptionValue(null, "cell grid visibility", "legal");
            this.mFloorPlanWorkspace.SetVisibleFloor (this.Viewport.Context, this.Viewport.Context.Floor, false);

            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler("\\entities\\structure\\floors");
        }

        private void buttonEraseFloor_Click(object sender, EventArgs e)
        {
            KeyEdit.Workspaces.Tools.TileSegmentPainter painter = 
                new KeyEdit.Workspaces.Tools.TileSegmentPainter(AppMain.mNetClient);
            EdgeStyle style = new EdgeStyle();
            style.StyleID = -1;

            style.FloorAtlasIndex = -1; 
            int mask = (int)Keystone.Portals.Interior.TILE_ATTRIBUTES.FLOOR;
            style.BottomLeftFootprint = new int[,] 
                           {
                           {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},  
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask}};

            style.CeilingAtlasIndex = -1; 
            mask = (int)Keystone.Portals.Interior.TILE_ATTRIBUTES.CEILING;
            style.TopRightFootprint = new int[,]
            {
                           {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},  
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask},
						   {mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask,mask}};

            // "erasing" floor or ceiling texture removes floor/ceiling segment and does not simply revert to some default texture
            painter.SetValue ( "tile style" ,style); 
            this.Viewport.Context.Workspace.CurrentTool = painter;

            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", true);
            this.Viewport.Context.SetCustomOptionValue(null, "cell grid visibility", "legal");
            this.mFloorPlanWorkspace.SetVisibleFloor (this.Viewport.Context, this.Viewport.Context.Floor, false);

            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler("\\entities\\structure\\floors");
        }

        /// <summary>
        /// TODO: these should not be hard coded.  They should be components in the asset browser
        /// but how do you delete?  right mouse drag instead of left?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPaintWalls_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
                ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
            if (vehicle == null || vehicle.Interior == null)
                MessageBox.Show("A vehicle with interior must first be selected.", "No Vehicle Selected.");

            KeyEdit.Workspaces.Tools.WallSegmentPainter painter =
                new KeyEdit.Workspaces.Tools.WallSegmentPainter(AppMain.mNetClient);

            // TODO: here we need to be able to set an EdgeStyle on the brush
            EdgeStyle edgeStyle = new EdgeStyle();

            // TODO: for changes to how we apply walls, all we really need style wise is a tileWidth and color
            //       and from that we can check adjacents and come up with proper scaling for the minimesh element.
            //       Though i think we can have seperate colors for the double sided wall (each wall segment is actually 2 wall segments
            //       positioned on either side of the edge.

            // prefabs contain model selector with multiple models 
            //edgeStyle.PrefabPathTopRight = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\entities\structure\walls\");
            //edgeStyle.PrefabPathTopRight += "wall.kgbentity";
            edgeStyle.FloorAtlasIndex = -1; // null; //TODO: this should just use the texture within the prefab.  System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\textures\walls\wall00.png"); // TODO: Texture application not working
            edgeStyle.CeilingAtlasIndex = -1;//  null; // edgeStyle.BottomLeftTexturePath;

            // TODO: I don't think i use TopRight anymore since now walls are just one prefab and not double sided.
            edgeStyle.Prefab = @"caesar\entities\structure\walls\wall.kgbentity";
            //edgeStyle.PrefabPathTopRight = null;

            // todo; using prefab, we must compute the footprint of the selected models we use on the edge or corner.

            // TODO: I think LayerNames should not be arrays. 
            // NOTE: wall edge footprint is generated dyanmically in Interior.GetEdgeFootprint() and is not saved in the cellmap as a result.
            painter.SetValue("wall style", edgeStyle);

            this.Viewport.Context.Workspace.CurrentTool = painter;

                              
            // show the wall styles in the asset browser.  These styles allow us to
            // select interior and exterior styles to use on the wall segment.
            // A default is used if none are selected
            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler("\\entities\\structure\\walls");

        }

          
        // TODO: right mouse click should remove walls and left mouse click should add.
        // TODO: wall.kgbentity should be picked from the asset browser and any asset in the "Walls" category
        //       should use the WallSegmentPainter tool to place items.
        private void buttonEraseWalls_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
                ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
            if (vehicle == null || vehicle.Interior == null)
                MessageBox.Show("A vehicle with interior must first be selected.", "No Vehicle Selected.");

            KeyEdit.Workspaces.Tools.WallSegmentPainter painter =
                new KeyEdit.Workspaces.Tools.WallSegmentPainter(AppMain.mNetClient);

            // TODO: here we need to be able to set a SegmentStyle on the brush 
            EdgeStyle edgeStyle = new EdgeStyle();
            edgeStyle.StyleID = -1;

            
            // TODO: I think LayerNames should not be arrays. 
            painter.SetValue("wall style", edgeStyle); // erases a wall 

            this.Viewport.Context.Workspace.CurrentTool = painter;

            // show the wall styles in the asset browser.  These styles allow us to
            // select interior and exterior styles to use on the wall segment.
            // A default is used if none are selected
            if (mCategoryChangedHandler != null)
                mCategoryChangedHandler("\\entities\\structure\\walls");

        }

        ///// <summary>
        ///// TODO: these should not be hard coded.  They should be components in the asset browser
        ///// but how do you delete?  right mouse drag instead of left?
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void buttonPaintWalls_Click(object sender, EventArgs e)
        //{
        //    Keystone.Vehicles.Vehicle vehicle = 
        //        ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
        //    if (vehicle == null || vehicle.Interior == null) 
        //        MessageBox.Show("A vehicle with interior must first be selected.", "No Vehicle Selected.");

        //    KeyEdit.Workspaces.Tools.WallSegmentPainter painter = 
        //        new KeyEdit.Workspaces.Tools.WallSegmentPainter(AppMain.mNetClient);

        //    // TODO:: SegmentStyle2 where i use a mesh from the asset browser that
        //    //        points to wall segment with ModelSelector nodes.

        //    // TODO: here we need to be able to set a SegmentStyle on the brush
        //    SegmentStyle style = new SegmentStyle();

        //    // TODO: for changes to how we apply walls, all we really need style wise is a tileWidth and color
        //    //       and from that we can check adjacents and come up with proper scaling for the minimesh element.
        //    //       Though i think we can have seperate colors for the double sided wall (each wall segment is actually 2 wall segments
        //    //       positioned on either side of the edge.

        //    // default test mesh to clone
        //    style.BottomLeftTexturePath = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\textures\W_g_ALL_01_D2.dds");
        //    style.BottomLeftMeshPath = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\meshes\");
        //    style.BottomLeftMeshPath = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\FPSCreator_Data\meshbank\scifi\moonbase\rooms\control_room\");
        //    style.BottomLeftMeshPath += "wall_ALL_a.X"; // "wall_1_x_3.obj";

        //    style.BottomLeftTexturePath = ""; // System.IO.Path.Combine(AppMain.MOD_PATH, null);
        //    style.BottomLeftMeshPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\meshes\structures\");
        //    style.BottomLeftMeshPath += "wall.obj";

        //    // TODO: for structures like walls, we should auto-generate the footprint within CelledRegion since
        //    //       footprints for inner and outer walls (as well as walls with doors) can be different depending on adjacents
        //    //       Also if we support variable width walls, then those footprints will auto-generate and apply to tilemask appropriately.
        //    // hardcode the footprint for this SegmentStyle
        //    int mask = (int)Keystone.Portals.Interior.TILEMASK_FLAGS.STRUCTURE_WALL;
        //    style.BottomLeftFootprint = new int[,] // left wall model is default footprint. for top,bottom, and right walls we apply a rotation to the footprint
        //                   {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask}, // corner bit is set, and when adding adjacent walls we test for this bit and allow placement anyway 
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask}}; // corner bit is set, and when adding adjacent walls we test for this bit and allow placement anyway

        //    // TODO: we need to dynamically compute the size of the wall because if it's an outer wall, they are potentially longer
        //    //       than the inner wall depending on adjacents.

        //    // TopRightt default wall model uses same footprint because it's using same mesh
        //    // but we apply a rotation to the footprint that varies based on wall edge orientation
        //    style.TopRightTexturePath = null; // System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\textures\W_g_MID_01_D2.dds");
        //    style.TopRightMeshPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\meshes\structures\");
        //    style.TopRightMeshPath += "wall.obj";
        //    style.TopRightFootprint = style.BottomLeftFootprint;

        //    // TODO: I think LayerNames should not be arrays. 
        //    painter.SetValue("wall style", style); // creates a wall 

        //    this.Viewport.Context.Workspace.CurrentTool = painter;


        //    // show the wall styles in the asset browser.  These styles allow us to
        //    // select interior and exterior styles to use on the wall segment.
        //    // A default is used if none are selected
        //    if (mCategoryChangedHandler != null)
        //        mCategoryChangedHandler("\\entities\\structure\\walls");

        //}

        //private void buttonEraseWalls_Click(object sender, EventArgs e)
        //{
        //    Keystone.Vehicles.Vehicle vehicle = 
        //        ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
        //    if (vehicle == null || vehicle.Interior == null)
        //        MessageBox.Show("A vehicle with interior must first be selected.", "No Vehicle Selected.");

        //    KeyEdit.Workspaces.Tools.WallSegmentPainter painter = 
        //        new KeyEdit.Workspaces.Tools.WallSegmentPainter(AppMain.mNetClient);

        //    // TODO: here we need to be able to set a SegmentStyle on the brush
        //    EdgeStyle style = new EdgeStyle();
        //    style.StyleID = -1; // StyleID defaults to 0, setting to -1 indicates we wish to delete or make null
        //    //mesh path null or empty means delete this segment
        //    style.BottomLeftTexturePath = "";
        //    style.BottomLeftMeshPath = "";

        //    // hardcode the footprint for this SegmentStyle
        //    // TODO: we do need the footprints in order to bitwise unapply these footprints!
        //    int mask = (int)Keystone.Portals.Interior.TILEMASK_FLAGS.STRUCTURE_WALL;
        //    style.BottomLeftFootprint = new int[,] // left wall model is default footprint. for top,bottom, and right walls we apply a rotation to the footprint
        //                   {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // corner bit not set 
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,mask},
        // {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}}; // corner bit not set

        //    //mesh path null or empty means delete this segment
        //    style.TopRightTexturePath = "";
        //    style.TopRightMeshPath = "";
        //    // TopRightt default wall model uses same footprint because it's using same mesh
        //    // but we apply a rotation to the footprint that varies based on wall edge orientation

        //    style.TopRightFootprint = style.BottomLeftFootprint;

        //    // TODO: I think LayerNames should not be arrays. 
        //    painter.SetValue ("wall style", style); // erases a wall 

        //    this.Viewport.Context.Workspace.CurrentTool = painter;

        //    // show the wall styles in the asset browser.  These styles allow us to
        //    // select interior and exterior styles to use on the wall segment.
        //    // A default is used if none are selected
        //    if (mCategoryChangedHandler != null)
        //        mCategoryChangedHandler("\\entities\\structure\\walls");

        //}

        // TODO: need more grid optiosn
        private void buttonShowGrid_Click(object sender, EventArgs e)
        {
            bool value = !buttonShowGrid.Checked;
            buttonShowGrid.Checked = value;
            this.Viewport.Context.ShowLineGrid = buttonShowGrid.Checked;
        // TODO: let's expierment with automatic selection for now
        //    this.Viewport.Context.SetCustomOptionValue (null, "show tilemask visualizer", value);
        //    this.Viewport.Context.SetCustomOptionValue(null, "show undefined interior grid", value);
        //    this.Viewport.Context.SetCustomOptionValue(null, "show defined interior grid", value);
        }

        private void buttonView_Click(object sender, EventArgs e)
        {

        }

        
        private void sliderOpacity_ValueChanged(object sender, EventArgs e)
        {
            this.Viewport.Context.SetCustomOptionValue(null, "exterior vehicle opacity", (float)sliderItemOpacity.Value / 100f);
        }

        private void buttonShowWallsDown_Click(object sender, EventArgs e)
        {
            this.Viewport.Context.SetCustomOptionValue(null, "wall show mode", 0); // down
        }

        private void buttonShowWallsCutAway_Click(object sender, EventArgs e)
        {
            this.Viewport.Context.SetCustomOptionValue(null, "wall show mode", 1); // cutaway
        }

        private void buttonShowWallsUp_Click(object sender, EventArgs e)
        {
            this.Viewport.Context.SetCustomOptionValue(null, "wall show mode", 2); // up
        }

        private void buttonShowCurrentFloorOnly_Click(object sender, EventArgs e)
        {
            int floor = (int)numLevel.Value;
            floor -= 1; // convert to 0 based floor index
            this.Viewport.Context.Floor = floor;

            bool disableAllStructures = "legal & illegal" != (string)this.Viewport.Context.GetCustomOptionValue(null, "cell grid visibility");
            
            buttonItemShowCurrentFloorOnly.Checked = !buttonItemShowCurrentFloorOnly.Checked;
            this.Viewport.Context.SetCustomOptionValue(null, "show current floor only", buttonItemShowCurrentFloorOnly.Checked);

            this.mFloorPlanWorkspace.SetVisibleFloor(this.Viewport.Context, floor, disableAllStructures);
        }
        

        private void buttonGenerateConnectivity_Click(object sender, EventArgs e)
        {
            //Keystone.Types.Vector3i[] pathPoints = Interior.PathFind(start, end);
            Keystone.Vehicles.Vehicle vehicle =
               ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
            if (vehicle == null || vehicle.Interior == null) return;

            ((Interior)vehicle.Interior).UpdateConnectivity();
        }

        private void buttonFillGaps_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
                ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
            if (vehicle == null || vehicle.Interior == null) return;



            ((Interior)vehicle.Interior).FillGaps();
        }

        private void buttonDrawLine_Click(object sender, EventArgs e)
        {
            ButtonItem item = (ButtonItem)sender;

            string linkName = "none";

            switch (item.Name)
            {
                case "buttonDrawPowerLine":
                    linkName = "powerlink";
                    break;
                case "buttonDrawFuelLine":
                    linkName = "fuellink";
                    break;
                case "buttonDrawNetworkLine":
                    linkName = "netlink";
                    break;
                case "buttonDrawVentLine":
                    linkName = "ventlink";
                    break;
                case "buttonDrawPlumbingLine":
                    linkName = "plumbinglink";
                    break;
                case "buttonDrawMechanicalLinkage":
                    linkName = "mechanicallink";
                    break;
            }


            Keystone.Vehicles.Vehicle vehicle =
                ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;
            if (vehicle == null || vehicle.Interior == null)
                MessageBox.Show("A vehicle with interior must first be selected.", "No Vehicle Selected.");

            // very much like our footprint painter
            KeyEdit.Workspaces.Tools.InteriorLinkPainter painter =
                new KeyEdit.Workspaces.Tools.InteriorLinkPainter(AppMain.mNetClient);

            bool enable = true; // how do we select which tool we want to erase?  toggle key for erase mode?

            // TODO: hud needs to switch to showing the overlay for this link 

            // TODO: drawing links must check for obstacles
            //       can links cross each other?
            // if right mouse click could be used to toggle draw/erase we could allow
            // erasing to be done on different layers regarldess of whether links of differnet types intersected
            painter.SetValue(linkName, enable);

            this.Viewport.Context.Workspace.CurrentTool = painter;
        }

        private void buttonEraseLine_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void buttonShowWallsMode_Click(object sender, EventArgs e)
        {

        }

        // TODO: we need to be able to dinstinguish between saving the prefab interior vs a game runtime instance of the Interior.
        // TODO: maybe we can do both? but actually during runtime playing, we wouldnt want that.
        // TODO: I could check AppMain.Simulation = true to see if we are in just a regular scene or a new universe simulation and
        //       then i would know if we were editing as prefab or instanced entity with interior
        private void buttonSaveInterior_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
               ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;

            FormMain.SaveResource(vehicle);
            // TODO: we should also have ability to save the .kgbentity itself if we add components and want them saved to the prefab
            // TODO: we should also have ability to "save as" and pick a new prefab name rather than overwrite existing

            // TODO: this should be done in background thread initiated from FormMain.Commands.ProcessCompletedCommandQueue() when modifying the interior structure
            //
            // save changes to interior db of the prefab.  We're already saving to the live Vehicle
            // NOTE: this does not update the actual prefab archive with any new components added to the interior of the vehicle.
            //       this is strictly for the cell database which includes inbounds, tile segments and wall segments.
            // TODO: So how do we save user made changes to a seperate "prefab" instance that can be loaded at runtime automatically when the Scene is saved?
            

            //if (vehicle == null || vehicle.Interior == null) return;

            //string prefab = (string)vehicle.Interior.GetProperty ("datapath", false).DefaultValue;
            //string datapath = System.IO.Path.ChangeExtension(prefab, ".interior");

            //if (AppMain._core.ArcadeEnabled)
            //{
                
            //}
            //{
                
                
            //}
            //Keystone.IO.ClientPager.QueuePageableResourceSave((Interior)vehicle.Interior, datapath, null, false);

            //Interior interior = (Interior)vehicle.Interior;
            //interior.SaveTVResource(dbpath);
        }

        private void labelCurrentFloor_Click(object sender, EventArgs e)
        {

        }

        private void buttonShowConnectivity_Click(object sender, EventArgs e)
        {
            buttonShowConnectivity.Checked = !buttonShowConnectivity.Checked;
            this.Viewport.Context.SetCustomOptionValue(null, "show connectivity", buttonShowConnectivity.Checked);

        }

        // NOTE: This is temporary code to be used during development to reload the crew after having made modifications to the BonedActor prefabs (eg. adding new animations)
        private void buttonCrew_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonCrewDeleteAll_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
               ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;

            if (vehicle == null || vehicle.Interior == null) return;


            DialogResult result = MessageBox.Show("Delete existing crew?", "Delete existing crew and regenerate?", MessageBoxButtons.YesNoCancel);
            if (result != DialogResult.Yes) return;


            Database.AppDatabaseHelper.CharacterRecord[] characters = Database.AppDatabaseHelper.GetCharacterRecords(vehicle.Interior.ID);
            if (characters == null || characters.Length == 0) return;

            string[] targetIDs = new string[characters.Length];

            // we don't need to suspend the simulation because the actual removal of the nodes from the scene occurs on main thread in ProcessCompletedCommandQueue()
            for (int i = 0; i < characters.Length; i++)
            {
                targetIDs[i] = characters[i].ID;

                if (this.mFloorPlanWorkspace.SelectedEntity != null && this.mFloorPlanWorkspace.SelectedEntity.Entity != null && this.mFloorPlanWorkspace.SelectedEntity.Entity.ID == characters[i].ID)
                    this.mFloorPlanWorkspace.SelectedEntity = null;
                
            }

            KeyCommon.Messages.Node_Remove removeNode = new KeyCommon.Messages.Node_Remove();
            removeNode.mParentID = vehicle.Interior.ID;
            removeNode.mTargetIDs = targetIDs;
            removeNode.SetFlag(KeyCommon.Messages.Flags.SourceIsClient);
            // TODO: currently removing seems to fail at removing from Octree Octant. // dec.18.2022 - is this still true?  i think i fixed that some time ago
            AppMain.mNetClient.SendMessage(removeNode);

            
        }

        private void buttonCrewGenerate_Click(object sender, EventArgs e)
        {
            Keystone.Vehicles.Vehicle vehicle =
              ((KeyEdit.Workspaces.FloorPlanDesignWorkspace)this.Viewport.Context.Workspace).Vehicle;

            if (vehicle == null || vehicle.Interior == null) return; // vehicle or vehicle.Interior may not be paged in yet, or perhaps was destroyed.

            // todo: does crew already exist? do we append with starting seed = AppMain._core.Seed + count.

            // do we issue a new command to generate crew?  This is unique because there also exists a character reference and not just a BonedEntity 
            // based on a prefab. How do we send that character data?  if we're basically saying its all procedural, then server just needs to include the seed used for that character.
            // Hmm.  Otherwise, it would have to be like mCustomPropertyData that would need to be serialized to the client.  We currently do not do this even when initial generation of new universe.
            // The same holds true for star and world generation.  We currently don't serialize the unique data, we just load it from the scene xml, which works only because we use loopback and share the scene xml data and there is no need to send it over the wire.
            // We could still potentially just use NetBuffer to serialize the records... but keep them out of KeyCommon.DatabaseEntities.  Stick with Game01.dll
            KeyCommon.Messages.Simulation_GenerateCharacters genCrew = new KeyCommon.Messages.Simulation_GenerateCharacters(); // todo: This maybe should just be GenerateCharacters() since we will need to generate for space stations, enemy vehicles, planets, etc.
            genCrew.Quantity = 100;
            genCrew.ParentID = vehicle.Interior.ID;
            AppMain.mNetClient.SendMessage(genCrew); // TODO: upon receiving this command, server should create the characters and Entities, and then serialize them to the client... but we can maybe postpone this until v2.0
                                                     // serializing mCustomProperty data is significant.
                                                     // TODO: initially, lets have the server generate the crew and send spawn commands to the client.
                                                     // this makes sense since the player shouldn't be able to send a Simulation_Spawn command and have the server just do as it's told.

            //// todo: when spawninmy vehicles, dont we need to be able to spawn it's crew as well?

            //// todo: generate the new crew _using the same seed_ and same quantity as when generating the Universe.
            ////       todo: or i could popup a "Quantity" dialog and use that value.  There's no reason it needs to be the same quantity as before.
            ////AppMain._core.Seed;

            //Database.AppDatabaseHelper.CharacterRecord[] characters = Database.AppDatabaseHelper.GetCharacterRecords(vehicle.Interior.ID);
            //if (characters != null  && characters.Length > 0) return;

            //if (characters == null || characters.Length == 0) return;
            //for (int i = 0; i < characters.Length; i++)
            //{
            //    KeyCommon.Messages.Simulation_Spawn spawn = new KeyCommon.Messages.Simulation_Spawn();
            //    spawn.EntitySaveRelativePath = characters[i].RelativeEntitySavePath;
            //    spawn.Translation = characters[i].Translation;
            //    spawn.ParentID = vehicle.Interior.ID;
            //    spawn.CellDBData = null;
            //    spawn.EntityFileData = null;
            //    spawn.UserName = AppMain._core.Settings.settingRead("network", "username");
            //    // todo: can we append a spawn.CustomData here and serialize it? Or do we simply serialize the mCustomProperties and use mCustomData only for AI/BehaviorTree context
            //    // todo: also keep in mind that our mCustomProperties[] can contain objects so long as the objects know how to serialize/deserialize themselves from the mCustomPropertyValues persist string.
            //    //       maybe we keep the mCustomData to store Character object, and link a resource file similar to how we link scripts. hm.
            //    // todo: i think our first attempt should be at using mCustomProperties and implementing serialization/deserialization of the persist string.  Or at worst, we make the mCustomData inherit from Node.cs and then
            //    //       it uses a seperate serialization persist string.  We can still also store this data in the save.db
            //    AppMain.mNetClient.SendMessage(spawn);
            //}
        }

        private void buttonAlignTops_Click(object sender, EventArgs e)
        {
            if (this.mFloorPlanWorkspace.CurrentTool != null && this.mFloorPlanWorkspace.CurrentTool is Workspaces.Tools.InteriorTransformTool)
            {
                Workspaces.Tools.InteriorTransformTool tool = (Workspaces.Tools.InteriorTransformTool)this.mFloorPlanWorkspace.CurrentTool;
                if (tool.Selections != null && tool.Selections.Length > 0)
                    tool.AlignSelectedComponents(Workspaces.Tools.InteriorTransformTool.Alignments.Tops);
            }
        }

        private void buttonAlignBottoms_Click(object sender, EventArgs e)
        {
            if (this.mFloorPlanWorkspace.CurrentTool != null && this.mFloorPlanWorkspace.CurrentTool is Workspaces.Tools.InteriorTransformTool)
            {
                Workspaces.Tools.InteriorTransformTool tool = (Workspaces.Tools.InteriorTransformTool)this.mFloorPlanWorkspace.CurrentTool;
                if (tool.Selections != null && tool.Selections.Length > 0)
                    tool.AlignSelectedComponents(Workspaces.Tools.InteriorTransformTool.Alignments.Bottoms);
            }
        }

        private void buttonAlignLefts_Click(object sender, EventArgs e)
        {
            if (this.mFloorPlanWorkspace.CurrentTool != null && this.mFloorPlanWorkspace.CurrentTool is Workspaces.Tools.InteriorTransformTool)
            {
                Workspaces.Tools.InteriorTransformTool tool = (Workspaces.Tools.InteriorTransformTool)this.mFloorPlanWorkspace.CurrentTool;
                if (tool.Selections != null && tool.Selections.Length > 0)
                    tool.AlignSelectedComponents(Workspaces.Tools.InteriorTransformTool.Alignments.Lefts);
            }
        }

        private void buttonAlignRights_Click(object sender, EventArgs e)
        {
            if (this.mFloorPlanWorkspace.CurrentTool != null && this.mFloorPlanWorkspace.CurrentTool is Workspaces.Tools.InteriorTransformTool)
            {
                Workspaces.Tools.InteriorTransformTool tool = (Workspaces.Tools.InteriorTransformTool)this.mFloorPlanWorkspace.CurrentTool;
                if (tool.Selections != null && tool.Selections.Length > 0)
                    tool.AlignSelectedComponents(Workspaces.Tools.InteriorTransformTool.Alignments.Rights);
            }
        }

        private void buttonAlignCenters_Click(object sender, EventArgs e)
        {
            if (this.mFloorPlanWorkspace.CurrentTool != null && this.mFloorPlanWorkspace.CurrentTool is Workspaces.Tools.InteriorTransformTool)
            {
                Workspaces.Tools.InteriorTransformTool tool = (Workspaces.Tools.InteriorTransformTool)this.mFloorPlanWorkspace.CurrentTool;
                if (tool.Selections != null && tool.Selections.Length > 0)
                    tool.AlignSelectedComponents(Workspaces.Tools.InteriorTransformTool.Alignments.Centers);
            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            pictureBox.Focus();
        }





        // TODO: add a button save option for updating the entire prefab, but without having to
        //       bring up the Save dialog.  It should also re-use the existing (if available) preview.png
    }
}