namespace KeyEdit.Controls
{
    partial class ViewportFloorplanControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewportFloorplanControl));
            this.labelCurrentFloor = new DevComponents.DotNetBar.LabelItem();
            this.comboItem1 = new DevComponents.Editors.ComboItem();
            this.comboItem2 = new DevComponents.Editors.ComboItem();
            this.comboItem3 = new DevComponents.Editors.ComboItem();
            this.comboItem4 = new DevComponents.Editors.ComboItem();
            this.buttonView = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowFPS = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowProfiler = new DevComponents.DotNetBar.ButtonItem();
            this.buttonLabels = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowBoundingBoxes = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowConnectivity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemShowCurrentFloorOnly = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSelectionMode = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPaintInteriorBounds = new DevComponents.DotNetBar.ButtonItem();
            this.buttonEraseInteriorBounds = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPaintFloor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonEraseFloor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowGrid = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPaintWalls = new DevComponents.DotNetBar.ButtonItem();
            this.buttonEraseWalls = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemOpacityDropDown = new DevComponents.DotNetBar.ButtonItem();
            this.sliderItemOpacity = new DevComponents.DotNetBar.SliderItem();
            this.itemContainerFloorUpDown = new DevComponents.DotNetBar.ItemContainer();
            this.controlContainerItem1 = new DevComponents.DotNetBar.ControlContainerItem();
            this.numLevel = new DevAge.Windows.Forms.DevAgeNumericUpDown();
            this.buttonShowWallsMode = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowWallsDown = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowWallsCutAway = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowWallsUp = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawPowerLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawFuelLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawNetworkLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawVentLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawPlumbingLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDrawMechanicalLinkage = new DevComponents.DotNetBar.ButtonItem();
            this.buttonEraseLine = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSaveInterior = new DevComponents.DotNetBar.ButtonItem();
            this.buttonGenerateConnectivity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFillGaps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCrew = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCrewDeleteAll = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCrewGenerate = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlign = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlignTops = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlignBottoms = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlignLefts = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlignRights = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAlignCenters = new DevComponents.DotNetBar.ButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.ribbonBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Size = new System.Drawing.Size(713, 445);
            this.pictureBox.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            // 
            // ribbonBar
            // 
            // 
            // 
            // 
            this.ribbonBar.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar.Controls.Add(this.numLevel);
            this.ribbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonSaveInterior,
            this.buttonSelectionMode,
            this.buttonPaintInteriorBounds,
            this.buttonEraseInteriorBounds,
            this.buttonPaintFloor,
            this.buttonEraseFloor,
            this.buttonPaintWalls,
            this.buttonEraseWalls,
            this.buttonItem1,
            this.buttonShowGrid,
            this.buttonView,
            this.buttonShowWallsMode,
            this.labelCurrentFloor,
            this.itemContainerFloorUpDown,
            this.buttonItemOpacityDropDown,
            this.buttonGenerateConnectivity,
            this.buttonFillGaps,
            this.buttonCrew,
            this.buttonAlign});
            this.ribbonBar.Size = new System.Drawing.Size(713, 26);
            // 
            // 
            // 
            this.ribbonBar.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // labelCurrentFloor
            // 
            this.labelCurrentFloor.Name = "labelCurrentFloor";
            this.labelCurrentFloor.Text = "Floor:";
            this.labelCurrentFloor.Click += new System.EventHandler(this.labelCurrentFloor_Click);
            // 
            // comboItem1
            // 
            this.comboItem1.Text = "1";
            // 
            // comboItem2
            // 
            this.comboItem2.Text = "2";
            // 
            // comboItem3
            // 
            this.comboItem3.Text = "3";
            // 
            // comboItem4
            // 
            this.comboItem4.Text = "4";
            // 
            // buttonView
            // 
            this.buttonView.Name = "buttonView";
            this.buttonView.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonShowFPS,
            this.buttonShowProfiler,
            this.buttonLabels,
            this.buttonShowBoundingBoxes,
            this.buttonShowConnectivity,
            this.buttonItemShowCurrentFloorOnly});
            this.buttonView.SubItemsExpandWidth = 14;
            this.buttonView.Text = "View";
            this.buttonView.Click += new System.EventHandler(this.buttonView_Click);
            // 
            // buttonShowFPS
            // 
            this.buttonShowFPS.Name = "buttonShowFPS";
            this.buttonShowFPS.Text = "Frames Per Second";
            this.buttonShowFPS.Click += new System.EventHandler(this.buttonShowFPS_Click);
            // 
            // buttonShowProfiler
            // 
            this.buttonShowProfiler.Name = "buttonShowProfiler";
            this.buttonShowProfiler.Text = "Profiler";
            this.buttonShowProfiler.Click += new System.EventHandler(this.buttonShowProfiler_Click);
            // 
            // buttonLabels
            // 
            this.buttonLabels.Name = "buttonLabels";
            this.buttonLabels.Text = "Labels";
            this.buttonLabels.Click += new System.EventHandler(this.buttonLabels_Click);
            // 
            // buttonShowBoundingBoxes
            // 
            this.buttonShowBoundingBoxes.Name = "buttonShowBoundingBoxes";
            this.buttonShowBoundingBoxes.Text = "Bounding Boxes";
            this.buttonShowBoundingBoxes.Click += new System.EventHandler(this.buttonShowBoundingBoxes_Click);
            // 
            // buttonShowConnectivity
            // 
            this.buttonShowConnectivity.Name = "buttonShowConnectivity";
            this.buttonShowConnectivity.Text = "Show Connectivity";
            this.buttonShowConnectivity.Click += new System.EventHandler(this.buttonShowConnectivity_Click);
            // 
            // buttonItemShowCurrentFloorOnly
            // 
            this.buttonItemShowCurrentFloorOnly.Name = "buttonItemShowCurrentFloorOnly";
            this.buttonItemShowCurrentFloorOnly.Text = "Show Current Floor Only";
            this.buttonItemShowCurrentFloorOnly.Click += new System.EventHandler(this.buttonShowCurrentFloorOnly_Click);
            // 
            // buttonSelectionMode
            // 
            this.buttonSelectionMode.Image = ((System.Drawing.Image)(resources.GetObject("buttonSelectionMode.Image")));
            this.buttonSelectionMode.Name = "buttonSelectionMode";
            this.buttonSelectionMode.SubItemsExpandWidth = 14;
            this.buttonSelectionMode.Tooltip = "Default Cursor";
            this.buttonSelectionMode.Click += new System.EventHandler(this.buttonSelectionMode_Click);
            // 
            // buttonPaintInteriorBounds
            // 
            this.buttonPaintInteriorBounds.Image = ((System.Drawing.Image)(resources.GetObject("buttonPaintInteriorBounds.Image")));
            this.buttonPaintInteriorBounds.Name = "buttonPaintInteriorBounds";
            this.buttonPaintInteriorBounds.SubItemsExpandWidth = 14;
            this.buttonPaintInteriorBounds.Tooltip = "Paint Interior Bounds";
            this.buttonPaintInteriorBounds.Click += new System.EventHandler(this.buttonPaintInteriorBounds_Click);
            // 
            // buttonEraseInteriorBounds
            // 
            this.buttonEraseInteriorBounds.Image = ((System.Drawing.Image)(resources.GetObject("buttonEraseInteriorBounds.Image")));
            this.buttonEraseInteriorBounds.Name = "buttonEraseInteriorBounds";
            this.buttonEraseInteriorBounds.SubItemsExpandWidth = 14;
            this.buttonEraseInteriorBounds.Tooltip = "Remove Interior Bounds";
            this.buttonEraseInteriorBounds.Click += new System.EventHandler(this.buttonEraseInteriorBounds_Click);
            // 
            // buttonPaintFloor
            // 
            this.buttonPaintFloor.Image = ((System.Drawing.Image)(resources.GetObject("buttonPaintFloor.Image")));
            this.buttonPaintFloor.Name = "buttonPaintFloor";
            this.buttonPaintFloor.SubItemsExpandWidth = 14;
            this.buttonPaintFloor.Tooltip = "Paint Floor Tile";
            this.buttonPaintFloor.Click += new System.EventHandler(this.buttonPaintFloor_Click);
            // 
            // buttonEraseFloor
            // 
            this.buttonEraseFloor.Image = ((System.Drawing.Image)(resources.GetObject("buttonEraseFloor.Image")));
            this.buttonEraseFloor.Name = "buttonEraseFloor";
            this.buttonEraseFloor.SubItemsExpandWidth = 14;
            this.buttonEraseFloor.Tooltip = "Remove Floor Tile";
            this.buttonEraseFloor.Click += new System.EventHandler(this.buttonEraseFloor_Click);
            // 
            // buttonShowGrid
            // 
            this.buttonShowGrid.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowGrid.Image")));
            this.buttonShowGrid.Name = "buttonShowGrid";
            this.buttonShowGrid.SubItemsExpandWidth = 14;
            this.buttonShowGrid.Tooltip = "Paint Interior Bounds";
            this.buttonShowGrid.Click += new System.EventHandler(this.buttonShowGrid_Click);
            // 
            // buttonPaintWalls
            // 
            this.buttonPaintWalls.Image = ((System.Drawing.Image)(resources.GetObject("buttonPaintWalls.Image")));
            this.buttonPaintWalls.Name = "buttonPaintWalls";
            this.buttonPaintWalls.SubItemsExpandWidth = 14;
            this.buttonPaintWalls.Tooltip = "Paint Walls";
            this.buttonPaintWalls.Click += new System.EventHandler(this.buttonPaintWalls_Click);
            // 
            // buttonEraseWalls
            // 
            this.buttonEraseWalls.Image = ((System.Drawing.Image)(resources.GetObject("buttonEraseWalls.Image")));
            this.buttonEraseWalls.Name = "buttonEraseWalls";
            this.buttonEraseWalls.SubItemsExpandWidth = 14;
            this.buttonEraseWalls.Tooltip = "Remove Walls";
            this.buttonEraseWalls.Click += new System.EventHandler(this.buttonEraseWalls_Click);
            // 
            // buttonItemOpacityDropDown
            // 
            this.buttonItemOpacityDropDown.Name = "buttonItemOpacityDropDown";
            this.buttonItemOpacityDropDown.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.sliderItemOpacity});
            this.buttonItemOpacityDropDown.SubItemsExpandWidth = 14;
            this.buttonItemOpacityDropDown.Text = "Opacity";
            // 
            // sliderItemOpacity
            // 
            this.sliderItemOpacity.LabelPosition = DevComponents.DotNetBar.eSliderLabelPosition.Bottom;
            this.sliderItemOpacity.Name = "sliderItemOpacity";
            this.sliderItemOpacity.SliderOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.sliderItemOpacity.Value = 30;
            this.sliderItemOpacity.ValueChanged += new System.EventHandler(this.sliderOpacity_ValueChanged);
            // 
            // itemContainerFloorUpDown
            // 
            // 
            // 
            // 
            this.itemContainerFloorUpDown.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainerFloorUpDown.Name = "itemContainerFloorUpDown";
            this.itemContainerFloorUpDown.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.controlContainerItem1});
            // 
            // 
            // 
            this.itemContainerFloorUpDown.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // controlContainerItem1
            // 
            this.controlContainerItem1.AllowItemResize = false;
            this.controlContainerItem1.Control = this.numLevel;
            this.controlContainerItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.controlContainerItem1.Name = "controlContainerItem1";
            // 
            // numLevel
            // 
            this.numLevel.Location = new System.Drawing.Point(395, 3);
            this.numLevel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLevel.Name = "numLevel";
            this.numLevel.Size = new System.Drawing.Size(35, 20);
            this.numLevel.TabIndex = 10001;
            this.numLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLevel.ValueChanged += new System.EventHandler(this.numLevel_ValueChanged);
            // 
            // buttonShowWallsMode
            // 
            this.buttonShowWallsMode.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowWallsMode.Image")));
            this.buttonShowWallsMode.Name = "buttonShowWallsMode";
            this.buttonShowWallsMode.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonShowWallsDown,
            this.buttonShowWallsCutAway,
            this.buttonShowWallsUp});
            this.buttonShowWallsMode.SubItemsExpandWidth = 14;
            this.buttonShowWallsMode.Tooltip = "Show Walls Mode";
            this.buttonShowWallsMode.Click += new System.EventHandler(this.buttonShowWallsMode_Click);
            // 
            // buttonShowWallsDown
            // 
            this.buttonShowWallsDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowWallsDown.Image")));
            this.buttonShowWallsDown.Name = "buttonShowWallsDown";
            this.buttonShowWallsDown.SubItemsExpandWidth = 14;
            this.buttonShowWallsDown.Text = "Walls Down";
            this.buttonShowWallsDown.Tooltip = "Show Walls Down";
            this.buttonShowWallsDown.Click += new System.EventHandler(this.buttonShowWallsDown_Click);
            // 
            // buttonShowWallsCutAway
            // 
            this.buttonShowWallsCutAway.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowWallsCutAway.Image")));
            this.buttonShowWallsCutAway.Name = "buttonShowWallsCutAway";
            this.buttonShowWallsCutAway.SubItemsExpandWidth = 14;
            this.buttonShowWallsCutAway.Text = "Walls Cut Away";
            this.buttonShowWallsCutAway.Tooltip = "Show Walls Cut Away";
            this.buttonShowWallsCutAway.Click += new System.EventHandler(this.buttonShowWallsCutAway_Click);
            // 
            // buttonShowWallsUp
            // 
            this.buttonShowWallsUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowWallsUp.Image")));
            this.buttonShowWallsUp.Name = "buttonShowWallsUp";
            this.buttonShowWallsUp.SubItemsExpandWidth = 14;
            this.buttonShowWallsUp.Text = "Walls Up";
            this.buttonShowWallsUp.Tooltip = "Show Walls Up";
            this.buttonShowWallsUp.Click += new System.EventHandler(this.buttonShowWallsUp_Click);
            // 
            // buttonItem1
            // 
            this.buttonItem1.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem1.Image")));
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonDrawPowerLine,
            this.buttonDrawFuelLine,
            this.buttonDrawNetworkLine,
            this.buttonDrawVentLine,
            this.buttonDrawPlumbingLine,
            this.buttonDrawMechanicalLinkage,
            this.buttonEraseLine});
            this.buttonItem1.Text = "buttonItem1";
            // 
            // buttonDrawPowerLine
            // 
            this.buttonDrawPowerLine.Image = ((System.Drawing.Image)(resources.GetObject("buttonDrawPowerLine.Image")));
            this.buttonDrawPowerLine.Name = "buttonDrawPowerLine";
            this.buttonDrawPowerLine.Text = "Draw Power Lines";
            this.buttonDrawPowerLine.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonDrawFuelLine
            // 
            this.buttonDrawFuelLine.Name = "buttonDrawFuelLine";
            this.buttonDrawFuelLine.Text = "Draw Fuel Lines";
            this.buttonDrawFuelLine.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonDrawNetworkLine
            // 
            this.buttonDrawNetworkLine.Name = "buttonDrawNetworkLine";
            this.buttonDrawNetworkLine.Text = "Draw Computer Network";
            this.buttonDrawNetworkLine.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonDrawVentLine
            // 
            this.buttonDrawVentLine.Name = "buttonDrawVentLine";
            this.buttonDrawVentLine.Text = "Draw Ventilation Lines";
            this.buttonDrawVentLine.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonDrawPlumbingLine
            // 
            this.buttonDrawPlumbingLine.Name = "buttonDrawPlumbingLine";
            this.buttonDrawPlumbingLine.Text = "Draw Plumbing Lines";
            this.buttonDrawPlumbingLine.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonDrawMechanicalLinkage
            // 
            this.buttonDrawMechanicalLinkage.Name = "buttonDrawMechanicalLinkage";
            this.buttonDrawMechanicalLinkage.Text = "Draw Mechanical Linkage";
            this.buttonDrawMechanicalLinkage.Click += new System.EventHandler(this.buttonDrawLine_Click);
            // 
            // buttonEraseLine
            // 
            this.buttonEraseLine.Image = ((System.Drawing.Image)(resources.GetObject("buttonEraseLine.Image")));
            this.buttonEraseLine.Name = "buttonEraseLine";
            this.buttonEraseLine.Text = "Line Eraser";
            this.buttonEraseLine.Click += new System.EventHandler(this.buttonEraseLine_Click);
            // 
            // buttonSaveInterior
            // 
            this.buttonSaveInterior.Name = "buttonSaveInterior";
            this.buttonSaveInterior.Text = "Save";
            this.buttonSaveInterior.Click += new System.EventHandler(this.buttonSaveInterior_Click);
            // 
            // buttonGenerateConnectivity
            // 
            this.buttonGenerateConnectivity.Name = "buttonGenerateConnectivity";
            this.buttonGenerateConnectivity.SubItemsExpandWidth = 14;
            this.buttonGenerateConnectivity.Text = "Connectivity";
            this.buttonGenerateConnectivity.Click += new System.EventHandler(this.buttonGenerateConnectivity_Click);
            // 
            // buttonFillGaps
            // 
            this.buttonFillGaps.Name = "buttonFillGaps";
            this.buttonFillGaps.SubItemsExpandWidth = 14;
            this.buttonFillGaps.Text = "Fill Gaps";
            this.buttonFillGaps.Click += new System.EventHandler(this.buttonFillGaps_Click);
            // 
            // buttonCrew
            // 
            this.buttonCrew.Name = "buttonCrew";
            this.buttonCrew.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonCrewDeleteAll,
            this.buttonCrewGenerate});
            this.buttonCrew.SubItemsExpandWidth = 14;
            this.buttonCrew.Text = "Crew";
            this.buttonCrew.Click += new System.EventHandler(this.buttonCrew_Click);
            // 
            // buttonCrewDeleteAll
            // 
            this.buttonCrewDeleteAll.Name = "buttonCrewDeleteAll";
            this.buttonCrewDeleteAll.Text = "Delete All";
            this.buttonCrewDeleteAll.Click += new System.EventHandler(this.buttonCrewDeleteAll_Click);
            // 
            // buttonCrewGenerate
            // 
            this.buttonCrewGenerate.Name = "buttonCrewGenerate";
            this.buttonCrewGenerate.Text = "Generate";
            this.buttonCrewGenerate.Click += new System.EventHandler(this.buttonCrewGenerate_Click);
            // 
            // buttonAlign
            // 
            this.buttonAlign.Name = "buttonAlign";
            this.buttonAlign.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonAlignTops,
            this.buttonAlignBottoms,
            this.buttonAlignLefts,
            this.buttonAlignRights,
            this.buttonAlignCenters});
            this.buttonAlign.SubItemsExpandWidth = 14;
            this.buttonAlign.Text = "Align";
            // 
            // buttonAlignTops
            // 
            this.buttonAlignTops.Name = "buttonAlignTops";
            this.buttonAlignTops.Text = "Tops";
            this.buttonAlignTops.Click += new System.EventHandler(this.buttonAlignTops_Click);
            // 
            // buttonAlignBottoms
            // 
            this.buttonAlignBottoms.Name = "buttonAlignBottoms";
            this.buttonAlignBottoms.Text = "Bottoms";
            this.buttonAlignBottoms.Click += new System.EventHandler(this.buttonAlignBottoms_Click);
            // 
            // buttonAlignLefts
            // 
            this.buttonAlignLefts.Name = "buttonAlignLefts";
            this.buttonAlignLefts.Text = "Lefts";
            this.buttonAlignLefts.Click += new System.EventHandler(this.buttonAlignLefts_Click);
            // 
            // buttonAlignRights
            // 
            this.buttonAlignRights.Name = "buttonAlignRights";
            this.buttonAlignRights.Text = "Rights";
            this.buttonAlignRights.Click += new System.EventHandler(this.buttonAlignRights_Click);
            // 
            // buttonAlignCenters
            // 
            this.buttonAlignCenters.Name = "buttonAlignCenters";
            this.buttonAlignCenters.Text = "Centers";
            this.buttonAlignCenters.Click += new System.EventHandler(this.buttonAlignCenters_Click);
            // 
            // ViewportFloorplanControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ViewportFloorplanControl";
            this.Size = new System.Drawing.Size(713, 471);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ribbonBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numLevel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelItem labelCurrentFloor;
        private DevComponents.Editors.ComboItem comboItem1;
        private DevComponents.Editors.ComboItem comboItem2;
        private DevComponents.Editors.ComboItem comboItem3;
        private DevComponents.Editors.ComboItem comboItem4;
        private DevComponents.DotNetBar.ButtonItem buttonView;
        private DevComponents.DotNetBar.ButtonItem buttonShowBoundingBoxes;
        private DevComponents.DotNetBar.ButtonItem buttonLabels;
        private DevComponents.DotNetBar.ButtonItem buttonShowFPS;
        private DevComponents.DotNetBar.ButtonItem buttonShowProfiler;
        private DevComponents.DotNetBar.ButtonItem buttonItemShowCurrentFloorOnly;
        private DevComponents.DotNetBar.ButtonItem buttonSelectionMode;
        private DevComponents.DotNetBar.ButtonItem buttonPaintInteriorBounds;
        private DevComponents.DotNetBar.ButtonItem buttonEraseInteriorBounds;
        private DevComponents.DotNetBar.ButtonItem buttonPaintFloor;
        private DevComponents.DotNetBar.ButtonItem buttonEraseFloor;
        private DevComponents.DotNetBar.ButtonItem buttonShowGrid;
        private DevComponents.DotNetBar.ButtonItem buttonPaintWalls;
        private DevComponents.DotNetBar.ButtonItem buttonEraseWalls;
        private DevComponents.DotNetBar.ButtonItem buttonItemOpacityDropDown;
        private DevComponents.DotNetBar.SliderItem sliderItemOpacity;
        private DevAge.Windows.Forms.DevAgeNumericUpDown numLevel;
        private DevComponents.DotNetBar.ItemContainer itemContainerFloorUpDown;
        private DevComponents.DotNetBar.ControlContainerItem controlContainerItem1;
        private DevComponents.DotNetBar.ButtonItem buttonShowWallsMode;
        private DevComponents.DotNetBar.ButtonItem buttonShowWallsDown;
        private DevComponents.DotNetBar.ButtonItem buttonShowWallsCutAway;
        private DevComponents.DotNetBar.ButtonItem buttonShowWallsUp;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem buttonDrawPowerLine;
        private DevComponents.DotNetBar.ButtonItem buttonDrawFuelLine;
        private DevComponents.DotNetBar.ButtonItem buttonDrawNetworkLine;
        private DevComponents.DotNetBar.ButtonItem buttonDrawVentLine;
        private DevComponents.DotNetBar.ButtonItem buttonDrawPlumbingLine;
        private DevComponents.DotNetBar.ButtonItem buttonEraseLine;
        private DevComponents.DotNetBar.ButtonItem buttonDrawMechanicalLinkage;
        private DevComponents.DotNetBar.ButtonItem buttonSaveInterior;
        private DevComponents.DotNetBar.ButtonItem buttonGenerateConnectivity;
        private DevComponents.DotNetBar.ButtonItem buttonShowConnectivity;
        private DevComponents.DotNetBar.ButtonItem buttonFillGaps;
        private DevComponents.DotNetBar.ButtonItem buttonCrew;
        private DevComponents.DotNetBar.ButtonItem buttonCrewDeleteAll;
        private DevComponents.DotNetBar.ButtonItem buttonCrewGenerate;
        private DevComponents.DotNetBar.ButtonItem buttonAlign;
        private DevComponents.DotNetBar.ButtonItem buttonAlignTops;
        private DevComponents.DotNetBar.ButtonItem buttonAlignBottoms;
        private DevComponents.DotNetBar.ButtonItem buttonAlignLefts;
        private DevComponents.DotNetBar.ButtonItem buttonAlignRights;
        private DevComponents.DotNetBar.ButtonItem buttonAlignCenters;
    }
}
