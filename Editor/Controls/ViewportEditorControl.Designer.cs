using System.Windows.Forms;

namespace KeyEdit
{
    partial class ViewportEditorControl
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

        #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewportEditorControl));
            this.buttonPlay = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPlayMission = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSelect = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTranslate = new DevComponents.DotNetBar.ButtonItem();
            this.buttonRotate = new DevComponents.DotNetBar.ButtonItem();
            this.buttonScale = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemProjection = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemPerspective = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemOrthographic = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTop = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemBottom = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemLeft = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemRight = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemFront = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemBack = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemFree = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemIsometric = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScaling = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx10 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx100 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx1000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSpeed = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem10meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem100meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem10000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem100000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem10000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem100000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem299792458meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1000000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem10000000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1000000000000meterps = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemZoom = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX10 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX100 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX1000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX10000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX100000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX1000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX10000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX100000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX1000000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX10000000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX100000000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemX1000000000000 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTimeScalingx4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem5 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem11 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem12 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemZoomExtents = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTextColor = new DevComponents.DotNetBar.ColorPickerDropDown();
            this.buttonView = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowAxisIndicator = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowLabels = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowGrid = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowCelestialGrid = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowOrbitLines = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowMotionField = new DevComponents.DotNetBar.ButtonItem();
            this.labelDebugSeperator = new DevComponents.DotNetBar.LabelItem();
            this.buttonShowFPS = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowCullingStats = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowLineProfiler = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowTVProfiler = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowBoundingBoxes = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShowPathingDebugInfo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemOrbit = new DevComponents.DotNetBar.ButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBox.Location = new System.Drawing.Point(0, 41);
            this.pictureBox.Size = new System.Drawing.Size(608, 407);
            // 
            // ribbonBar
            // 
            this.ribbonBar.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar.ContainerControlProcessDialogKey = true;
            this.ribbonBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbonBar.DragDropSupport = true;
            this.ribbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonSelect,
            this.buttonTranslate,
            this.buttonRotate,
            this.buttonScale,
            this.buttonView,
            this.buttonItemProjection,
            this.buttonItemZoomExtents,
            this.buttonItemOrbit,
            this.buttonPlayMission,
            this.buttonPlay});
            this.ribbonBar.Location = new System.Drawing.Point(0, 0);
            this.ribbonBar.Name = "ribbonBar";
            this.ribbonBar.ResizeItemsToFit = false;
            this.ribbonBar.Size = new System.Drawing.Size(608, 37);
            this.ribbonBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar.TabIndex = 1;
            // 
            // 
            // 
            this.ribbonBar.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar.TitleVisible = false;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Image = ((System.Drawing.Image)(resources.GetObject("buttonPlay.Image")));
            this.buttonPlay.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonPlay.ImagePaddingHorizontal = 0;
            this.buttonPlay.ImagePaddingVertical = 0;
            this.buttonPlay.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.RibbonWordWrap = false;
            this.buttonPlay.SubItemsExpandWidth = 14;
            this.buttonPlay.Text = "buttonPlay";
            this.buttonPlay.Tooltip = "Play";
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonPlayMission
            // 
            this.buttonPlayMission.Image = ((System.Drawing.Image)(resources.GetObject("buttonPlayMission.Image")));
            this.buttonPlayMission.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonPlayMission.ImagePaddingHorizontal = 0;
            this.buttonPlayMission.ImagePaddingVertical = 0;
            this.buttonPlayMission.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonPlayMission.Name = "buttonPlayMission";
            this.buttonPlayMission.RibbonWordWrap = false;
            this.buttonPlayMission.SubItemsExpandWidth = 14;
            this.buttonPlayMission.Text = "buttonPlayMission";
            this.buttonPlayMission.Tooltip = "Play Mission";
            this.buttonPlayMission.Click += new System.EventHandler(this.buttonPlayMission_Click);
            // 
            // buttonSelect
            // 
            this.buttonSelect.Image = global::KeyEdit.Properties.Resources.Selection_Mode;
            this.buttonSelect.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonSelect.ImagePaddingHorizontal = 0;
            this.buttonSelect.ImagePaddingVertical = 0;
            this.buttonSelect.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.RibbonWordWrap = false;
            this.buttonSelect.SubItemsExpandWidth = 14;
            this.buttonSelect.Text = "buttonSelect";
            this.buttonSelect.Tooltip = "Select";
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Image = global::KeyEdit.Properties.Resources.Turn_On_Move_Manipulator;
            this.buttonTranslate.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonTranslate.ImagePaddingHorizontal = 0;
            this.buttonTranslate.ImagePaddingVertical = 0;
            this.buttonTranslate.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.RibbonWordWrap = false;
            this.buttonTranslate.SubItemsExpandWidth = 14;
            this.buttonTranslate.Text = "buttonTranslate";
            this.buttonTranslate.Tooltip = "Translate";
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // buttonRotate
            // 
            this.buttonRotate.Image = global::KeyEdit.Properties.Resources.Turn_On_Rotation_Manipulator;
            this.buttonRotate.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonRotate.ImagePaddingHorizontal = 0;
            this.buttonRotate.ImagePaddingVertical = 0;
            this.buttonRotate.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonRotate.Name = "buttonRotate";
            this.buttonRotate.RibbonWordWrap = false;
            this.buttonRotate.SubItemsExpandWidth = 14;
            this.buttonRotate.Text = "buttonRotate";
            this.buttonRotate.Tooltip = "Rotate";
            this.buttonRotate.Click += new System.EventHandler(this.buttonRotate_Click);
            // 
            // buttonScale
            // 
            this.buttonScale.Image = global::KeyEdit.Properties.Resources.Turn_On_Scale_Manipulator;
            this.buttonScale.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonScale.ImagePaddingHorizontal = 0;
            this.buttonScale.ImagePaddingVertical = 0;
            this.buttonScale.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonScale.Name = "buttonScale";
            this.buttonScale.RibbonWordWrap = false;
            this.buttonScale.SubItemsExpandWidth = 14;
            this.buttonScale.Text = "buttonScale";
            this.buttonScale.Tooltip = "Scale";
            this.buttonScale.Click += new System.EventHandler(this.buttonScale_Click);
            // 
            // buttonItemProjection
            // 
            this.buttonItemProjection.AutoExpandOnClick = true;
            this.buttonItemProjection.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemProjection.Image = global::KeyEdit.Properties.Resources.Add_Camera;
            this.buttonItemProjection.ImageFixedSize = new System.Drawing.Size(32, 32);
            this.buttonItemProjection.ImagePaddingHorizontal = 0;
            this.buttonItemProjection.ImagePaddingVertical = 0;
            this.buttonItemProjection.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItemProjection.Name = "buttonItemProjection";
            this.buttonItemProjection.RibbonWordWrap = false;
            this.buttonItemProjection.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemPerspective,
            this.buttonItemOrthographic,
            this.buttonItemIsometric,
            this.buttonItemTimeScaling,
            this.buttonItemSpeed,
            this.buttonItemZoom});
            this.buttonItemProjection.SubItemsExpandWidth = 14;
            this.buttonItemProjection.Tooltip = "View";
            // 
            // buttonItemPerspective
            // 
            this.buttonItemPerspective.Name = "buttonItemPerspective";
            this.buttonItemPerspective.Text = "Perspective";
            this.buttonItemPerspective.Click += new System.EventHandler(this.toolStripButtonProjection_Click);
            // 
            // buttonItemOrthographic
            // 
            this.buttonItemOrthographic.Name = "buttonItemOrthographic";
            this.buttonItemOrthographic.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemTop,
            this.buttonItemBottom,
            this.buttonItemLeft,
            this.buttonItemRight,
            this.buttonItemFront,
            this.buttonItemBack,
            this.buttonItemFree});
            this.buttonItemOrthographic.Text = "Orthographic";
            this.buttonItemOrthographic.Click += new System.EventHandler(this.toolStripButtonProjection_Click);
            // 
            // buttonItemTop
            // 
            this.buttonItemTop.Name = "buttonItemTop";
            this.buttonItemTop.Text = "Top";
            this.buttonItemTop.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemBottom
            // 
            this.buttonItemBottom.Name = "buttonItemBottom";
            this.buttonItemBottom.Text = "Bottom";
            this.buttonItemBottom.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemLeft
            // 
            this.buttonItemLeft.Name = "buttonItemLeft";
            this.buttonItemLeft.Text = "Left";
            this.buttonItemLeft.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemRight
            // 
            this.buttonItemRight.Name = "buttonItemRight";
            this.buttonItemRight.Text = "Right";
            this.buttonItemRight.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemFront
            // 
            this.buttonItemFront.Name = "buttonItemFront";
            this.buttonItemFront.Text = "Front";
            this.buttonItemFront.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemBack
            // 
            this.buttonItemBack.Name = "buttonItemBack";
            this.buttonItemBack.Text = "Back";
            this.buttonItemBack.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemFree
            // 
            this.buttonItemFree.Name = "buttonItemFree";
            this.buttonItemFree.Text = "Free";
            this.buttonItemFree.Click += new System.EventHandler(this.toolStripButtonView_Click);
            // 
            // buttonItemIsometric
            // 
            this.buttonItemIsometric.Name = "buttonItemIsometric";
            this.buttonItemIsometric.Text = "Isometric";
            this.buttonItemIsometric.Click += new System.EventHandler(this.toolStripButtonProjection_Click);
            // 
            // buttonItemTimeScaling
            // 
            this.buttonItemTimeScaling.AutoExpandOnClick = true;
            this.buttonItemTimeScaling.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemTimeScaling.Image = global::KeyEdit.Properties.Resources.zoom_icon_32;
            this.buttonItemTimeScaling.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItemTimeScaling.ImagePaddingHorizontal = 5;
            this.buttonItemTimeScaling.ImagePaddingVertical = 0;
            this.buttonItemTimeScaling.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItemTimeScaling.Name = "buttonItemTimeScaling";
            this.buttonItemTimeScaling.RibbonWordWrap = false;
            this.buttonItemTimeScaling.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemTimeScalingx1,
            this.buttonItemTimeScalingx2,
            this.buttonItemTimeScalingx10,
            this.buttonItemTimeScalingx100,
            this.buttonItemTimeScalingx1000});
            this.buttonItemTimeScaling.SubItemsExpandWidth = 14;
            this.buttonItemTimeScaling.Text = "Time Scaling x1";
            this.buttonItemTimeScaling.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemTimeScalingx1
            // 
            this.buttonItemTimeScalingx1.Name = "buttonItemTimeScalingx1";
            this.buttonItemTimeScalingx1.Text = "Time Scaling x1";
            this.buttonItemTimeScalingx1.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemTimeScalingx2
            // 
            this.buttonItemTimeScalingx2.Name = "buttonItemTimeScalingx2";
            this.buttonItemTimeScalingx2.Text = "Time Scaling x2";
            this.buttonItemTimeScalingx2.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemTimeScalingx10
            // 
            this.buttonItemTimeScalingx10.Name = "buttonItemTimeScalingx10";
            this.buttonItemTimeScalingx10.Text = "Time Scaling x10";
            this.buttonItemTimeScalingx10.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemTimeScalingx100
            // 
            this.buttonItemTimeScalingx100.Name = "buttonItemTimeScalingx100";
            this.buttonItemTimeScalingx100.Text = "Time Scaling x100";
            this.buttonItemTimeScalingx100.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemTimeScalingx1000
            // 
            this.buttonItemTimeScalingx1000.Name = "buttonItemTimeScalingx1000";
            this.buttonItemTimeScalingx1000.Text = "Time Scaling x1000";
            this.buttonItemTimeScalingx1000.Click += new System.EventHandler(this.buttonItemTimeScaling_Click);
            // 
            // buttonItemSpeed
            // 
            this.buttonItemSpeed.AutoExpandOnClick = true;
            this.buttonItemSpeed.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSpeed.Image = global::KeyEdit.Properties.Resources.dashboard_icon_32;
            this.buttonItemSpeed.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItemSpeed.ImagePaddingHorizontal = 5;
            this.buttonItemSpeed.ImagePaddingVertical = 0;
            this.buttonItemSpeed.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItemSpeed.Name = "buttonItemSpeed";
            this.buttonItemSpeed.RibbonWordWrap = false;
            this.buttonItemSpeed.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem1meterps,
            this.buttonItem10meterps,
            this.buttonItem100meterps,
            this.buttonItem1000meterps,
            this.buttonItem10000meterps,
            this.buttonItem100000meterps,
            this.buttonItem1000000meterps,
            this.buttonItem10000000meterps,
            this.buttonItem100000000meterps,
            this.buttonItem299792458meterps,
            this.buttonItem1000000000meterps,
            this.buttonItem10000000000meterps,
            this.buttonItem1000000000000meterps});
            this.buttonItemSpeed.SubItemsExpandWidth = 14;
            this.buttonItemSpeed.Text = "1 m/s";
            // 
            // buttonItem1meterps
            // 
            this.buttonItem1meterps.Name = "buttonItem1meterps";
            this.buttonItem1meterps.Text = "1 m/s";
            this.buttonItem1meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem10meterps
            // 
            this.buttonItem10meterps.Name = "buttonItem10meterps";
            this.buttonItem10meterps.Text = "10 m/s";
            this.buttonItem10meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem100meterps
            // 
            this.buttonItem100meterps.Name = "buttonItem100meterps";
            this.buttonItem100meterps.Text = "100 m/s";
            this.buttonItem100meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem1000meterps
            // 
            this.buttonItem1000meterps.Name = "buttonItem1000meterps";
            this.buttonItem1000meterps.Text = "1,000 m/s";
            this.buttonItem1000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem10000meterps
            // 
            this.buttonItem10000meterps.Name = "buttonItem10000meterps";
            this.buttonItem10000meterps.Text = "10,000 m/s";
            this.buttonItem10000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem100000meterps
            // 
            this.buttonItem100000meterps.Name = "buttonItem100000meterps";
            this.buttonItem100000meterps.Text = "100,000 m/s";
            this.buttonItem100000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem1000000meterps
            // 
            this.buttonItem1000000meterps.Name = "buttonItem1000000meterps";
            this.buttonItem1000000meterps.Text = "1,000,000 m/s";
            this.buttonItem1000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem10000000meterps
            // 
            this.buttonItem10000000meterps.Name = "buttonItem10000000meterps";
            this.buttonItem10000000meterps.Text = "10,000,000 m/s";
            this.buttonItem10000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem100000000meterps
            // 
            this.buttonItem100000000meterps.Name = "buttonItem100000000meterps";
            this.buttonItem100000000meterps.Text = "100,000,000 m/s";
            this.buttonItem100000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem299792458meterps
            // 
            this.buttonItem299792458meterps.Name = "buttonItem299792458meterps";
            this.buttonItem299792458meterps.Text = "299,792,458 m/s";
            this.buttonItem299792458meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem1000000000meterps
            // 
            this.buttonItem1000000000meterps.Name = "buttonItem1000000000meterps";
            this.buttonItem1000000000meterps.Text = "1,000,000,000 m/s";
            this.buttonItem1000000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem10000000000meterps
            // 
            this.buttonItem10000000000meterps.Name = "buttonItem10000000000meterps";
            this.buttonItem10000000000meterps.Text = "10,000,000,000 m/s";
            this.buttonItem10000000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItem1000000000000meterps
            // 
            this.buttonItem1000000000000meterps.Name = "buttonItem1000000000000meterps";
            this.buttonItem1000000000000meterps.Text = "1,000,000,000,000 m/s";
            this.buttonItem1000000000000meterps.Click += new System.EventHandler(this.buttonItemSpeed_Click);
            // 
            // buttonItemZoom
            // 
            this.buttonItemZoom.AutoExpandOnClick = true;
            this.buttonItemZoom.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemZoom.Image = global::KeyEdit.Properties.Resources.zoom_icon_32;
            this.buttonItemZoom.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItemZoom.ImagePaddingHorizontal = 5;
            this.buttonItemZoom.ImagePaddingVertical = 0;
            this.buttonItemZoom.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItemZoom.Name = "buttonItemZoom";
            this.buttonItemZoom.RibbonWordWrap = false;
            this.buttonItemZoom.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemX1,
            this.buttonItemX10,
            this.buttonItemX100,
            this.buttonItemX1000,
            this.buttonItemX10000,
            this.buttonItemX100000,
            this.buttonItemX1000000,
            this.buttonItemX10000000,
            this.buttonItemX100000000,
            this.buttonItemX1000000000,
            this.buttonItemX10000000000,
            this.buttonItemX100000000000,
            this.buttonItemX1000000000000});
            this.buttonItemZoom.SubItemsExpandWidth = 14;
            this.buttonItemZoom.Text = "x1";
            this.buttonItemZoom.Click += new System.EventHandler(this.ButtonItemZoomClick);
            // 
            // buttonItemX1
            // 
            this.buttonItemX1.Name = "buttonItemX1";
            this.buttonItemX1.Text = "x1";
            this.buttonItemX1.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX10
            // 
            this.buttonItemX10.Name = "buttonItemX10";
            this.buttonItemX10.Text = "x10";
            this.buttonItemX10.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX100
            // 
            this.buttonItemX100.Name = "buttonItemX100";
            this.buttonItemX100.Text = "x100";
            this.buttonItemX100.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX1000
            // 
            this.buttonItemX1000.Name = "buttonItemX1000";
            this.buttonItemX1000.Text = "x1,000";
            this.buttonItemX1000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX10000
            // 
            this.buttonItemX10000.Name = "buttonItemX10000";
            this.buttonItemX10000.Text = "x10,000";
            this.buttonItemX10000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX100000
            // 
            this.buttonItemX100000.Name = "buttonItemX100000";
            this.buttonItemX100000.Text = "x100,000";
            this.buttonItemX100000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX1000000
            // 
            this.buttonItemX1000000.Name = "buttonItemX1000000";
            this.buttonItemX1000000.Text = "x1,000,000";
            this.buttonItemX1000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX10000000
            // 
            this.buttonItemX10000000.Name = "buttonItemX10000000";
            this.buttonItemX10000000.Text = "x10,000,000";
            this.buttonItemX10000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX100000000
            // 
            this.buttonItemX100000000.Name = "buttonItemX100000000";
            this.buttonItemX100000000.Text = "x100,000,000";
            this.buttonItemX100000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX1000000000
            // 
            this.buttonItemX1000000000.Name = "buttonItemX1000000000";
            this.buttonItemX1000000000.Text = "x1,000,000,000";
            this.buttonItemX1000000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX10000000000
            // 
            this.buttonItemX10000000000.Name = "buttonItemX10000000000";
            this.buttonItemX10000000000.Text = "x10,000,000,000";
            this.buttonItemX10000000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX100000000000
            // 
            this.buttonItemX100000000000.Name = "buttonItemX100000000000";
            this.buttonItemX100000000000.Text = "x100,000,000,000";
            this.buttonItemX100000000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemX1000000000000
            // 
            this.buttonItemX1000000000000.Name = "buttonItemX1000000000000";
            this.buttonItemX1000000000000.Text = "x1,000,000,000,000";
            this.buttonItemX1000000000000.Click += new System.EventHandler(this.buttonItemZoom_Click);
            // 
            // buttonItemTimeScalingx4
            // 
            this.buttonItemTimeScalingx4.Name = "buttonItemTimeScalingx4";
            // 
            // buttonItem5
            // 
            this.buttonItem5.Image = global::KeyEdit.Properties.Resources.AlignToGridHS;
            this.buttonItem5.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItem5.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItem5.Name = "buttonItem5";
            this.buttonItem5.RibbonWordWrap = false;
            this.buttonItem5.SubItemsExpandWidth = 14;
            this.buttonItem5.Text = "buttonItem5";
            // 
            // buttonItem4
            // 
            this.buttonItem4.Image = global::KeyEdit.Properties.Resources.AlignToGridHS;
            this.buttonItem4.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItem4.ImagePaddingHorizontal = 0;
            this.buttonItem4.ImagePaddingVertical = 0;
            this.buttonItem4.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.RibbonWordWrap = false;
            this.buttonItem4.SubItemsExpandWidth = 14;
            this.buttonItem4.Text = "buttonItem4";
            // 
            // buttonItem11
            // 
            this.buttonItem11.AutoExpandOnClick = true;
            this.buttonItem11.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem11.Image = global::KeyEdit.Properties.Resources.AlignToGridHS;
            this.buttonItem11.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItem11.ImagePaddingHorizontal = 0;
            this.buttonItem11.ImagePaddingVertical = 0;
            this.buttonItem11.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItem11.Name = "buttonItem11";
            this.buttonItem11.RibbonWordWrap = false;
            this.buttonItem11.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem12});
            this.buttonItem11.SubItemsExpandWidth = 14;
            this.buttonItem11.Text = "Perspective";
            // 
            // buttonItem12
            // 
            this.buttonItem12.Name = "buttonItem12";
            this.buttonItem12.Text = "buttonItem2";
            // 
            // buttonItemZoomExtents
            // 
            this.buttonItemZoomExtents.FixedSize = new System.Drawing.Size(32, 32);
            this.buttonItemZoomExtents.Image = global::KeyEdit.Properties.Resources.Zoom_View;
            this.buttonItemZoomExtents.Name = "buttonItemZoomExtents";
            this.buttonItemZoomExtents.SubItemsExpandWidth = 14;
            this.buttonItemZoomExtents.Tooltip = "Zoom Extents";
            this.buttonItemZoomExtents.Click += new System.EventHandler(this.buttonItemZoomExtents_Click);
            // 
            // buttonTextColor
            // 
            this.buttonTextColor.Image = ((System.Drawing.Image)(resources.GetObject("buttonTextColor.Image")));
            this.buttonTextColor.Name = "buttonTextColor";
            this.buttonTextColor.SelectedColorImageRectangle = new System.Drawing.Rectangle(0, 13, 16, 3);
            this.buttonTextColor.Text = "Text &Color";
            this.buttonTextColor.SelectedColorChanged += new System.EventHandler(this.buttonTextColor_SelectedColorChanged);
            // 
            // buttonView
            // 
            this.buttonView.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonView.Image = global::KeyEdit.Properties.Resources.Show_Heads_Up_Display;
            this.buttonView.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonView.Name = "buttonView";
            this.buttonView.RibbonWordWrap = false;
            this.buttonView.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonTextColor,
            this.buttonShowAxisIndicator,
            this.buttonShowLabels,
            this.buttonShowGrid,
            this.buttonShowCelestialGrid,
            this.buttonShowOrbitLines,
            this.buttonShowMotionField,
            this.labelDebugSeperator,
            this.buttonShowFPS,
            this.buttonShowCullingStats,
            this.buttonShowLineProfiler,
            this.buttonShowTVProfiler,
            this.buttonShowBoundingBoxes,
            this.buttonShowPathingDebugInfo});
            this.buttonView.SubItemsExpandWidth = 14;
            this.buttonView.Tooltip = "HUD Items";
            // 
            // buttonShowAxisIndicator
            // 
            this.buttonShowAxisIndicator.Name = "buttonShowAxisIndicator";
            this.buttonShowAxisIndicator.Text = "Axis Indicator";
            this.buttonShowAxisIndicator.Click += new System.EventHandler(this.ButtonItemShowAxisIndicatorClick);
            // 
            // buttonShowLabels
            // 
            this.buttonShowLabels.Name = "buttonShowLabels";
            this.buttonShowLabels.Text = "Labels";
            this.buttonShowLabels.Click += new System.EventHandler(this.buttonLabels_Click);
            // 
            // buttonShowGrid
            // 
            this.buttonShowGrid.Name = "buttonShowGrid";
            this.buttonShowGrid.Text = "Grid";
            this.buttonShowGrid.Click += new System.EventHandler(this.buttonShowGrid_Click);
            // 
            // buttonShowCelestialGrid
            // 
            this.buttonShowCelestialGrid.Name = "buttonShowCelestialGrid";
            this.buttonShowCelestialGrid.Text = "Celestial Grid";
            this.buttonShowCelestialGrid.Click += new System.EventHandler(this.buttonItemCelestialGrid_Click);
            // 
            // buttonShowOrbitLines
            // 
            this.buttonShowOrbitLines.Name = "buttonShowOrbitLines";
            this.buttonShowOrbitLines.Text = "Orbit Lines";
            this.buttonShowOrbitLines.Click += new System.EventHandler(this.ButtonItemOrbitLinesClick);
            // 
            // buttonShowMotionField
            // 
            this.buttonShowMotionField.Name = "buttonShowMotionField";
            this.buttonShowMotionField.Text = "Motion Field";
            this.buttonShowMotionField.Click += new System.EventHandler(this.ButtonItemMotionField_Click);
            // 
            // labelDebugSeperator
            // 
            this.labelDebugSeperator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(238)))));
            this.labelDebugSeperator.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelDebugSeperator.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.labelDebugSeperator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(21)))), ((int)(((byte)(110)))));
            this.labelDebugSeperator.Name = "labelDebugSeperator";
            this.labelDebugSeperator.PaddingBottom = 1;
            this.labelDebugSeperator.PaddingLeft = 10;
            this.labelDebugSeperator.PaddingTop = 1;
            this.labelDebugSeperator.SingleLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.labelDebugSeperator.Text = "Debug";
            // 
            // buttonShowFPS
            // 
            this.buttonShowFPS.Name = "buttonShowFPS";
            this.buttonShowFPS.Text = "Frames Per Second";
            this.buttonShowFPS.Click += new System.EventHandler(this.buttonShowFPS_Click);
            // 
            // buttonShowCullingStats
            // 
            this.buttonShowCullingStats.Name = "buttonShowCullingStats";
            this.buttonShowCullingStats.Text = "Culling Stats";
            this.buttonShowCullingStats.Click += new System.EventHandler(this.buttonShowCullingStats_Click);
            // 
            // buttonShowLineProfiler
            // 
            this.buttonShowLineProfiler.Name = "buttonShowLineProfiler";
            this.buttonShowLineProfiler.Text = "Line Profiler";
            this.buttonShowLineProfiler.Click += new System.EventHandler(this.buttonShowLineProfiler_Click);
            // 
            // buttonShowTVProfiler
            // 
            this.buttonShowTVProfiler.Name = "buttonShowTVProfiler";
            this.buttonShowTVProfiler.Text = "TV Profiler";
            this.buttonShowTVProfiler.Click += new System.EventHandler(this.buttonShowTVProfiler_Click);
            // 
            // buttonShowBoundingBoxes
            // 
            this.buttonShowBoundingBoxes.Name = "buttonShowBoundingBoxes";
            this.buttonShowBoundingBoxes.Text = "Bounding Boxes";
            this.buttonShowBoundingBoxes.Click += new System.EventHandler(this.buttonShowBoundingBoxes_Click);
            // 
            // buttonShowPathingDebugInfo
            // 
            this.buttonShowPathingDebugInfo.Name = "buttonShowPathingDebugInfo";
            this.buttonShowPathingDebugInfo.Text = "Pathing Debug Info";
            this.buttonShowPathingDebugInfo.Click += new System.EventHandler(this.ButtonItemShowPathingDebugInfoClick);
            // 
            // buttonItemOrbit
            // 
            this.buttonItemOrbit.FixedSize = new System.Drawing.Size(32, 32);
            this.buttonItemOrbit.Image = global::KeyEdit.Properties.Resources.Zoom_View;
            this.buttonItemOrbit.Name = "buttonItemOrbit";
            this.buttonItemOrbit.SubItemsExpandWidth = 14;
            this.buttonItemOrbit.Tooltip = "Zoom Extents";
            this.buttonItemOrbit.Click += new System.EventHandler(this.ButtonItemOrbitClick);
            // 
            // ViewportEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.CausesValidation = false;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ViewportEditorControl";
            this.Size = new System.Drawing.Size(608, 448);
            this.Controls.SetChildIndex(this.ribbonBar, 0);
            this.Controls.SetChildIndex(this.pictureBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

		}
		private DevComponents.DotNetBar.ButtonItem buttonShowOrbitLines;
		private DevComponents.DotNetBar.ButtonItem buttonShowMotionField;
		private DevComponents.DotNetBar.ButtonItem buttonItemOrbit;
		private DevComponents.DotNetBar.ButtonItem buttonShowAxisIndicator;
		private DevComponents.DotNetBar.ButtonItem buttonShowPathingDebugInfo;
		private DevComponents.DotNetBar.LabelItem labelDebugSeperator;
		private DevComponents.DotNetBar.ButtonItem buttonShowCelestialGrid;
		#endregion

        private DevComponents.DotNetBar.ButtonItem buttonItem5;
        private DevComponents.DotNetBar.ButtonItem buttonItemProjection;
        private DevComponents.DotNetBar.ButtonItem buttonItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItemPerspective;
        private DevComponents.DotNetBar.ButtonItem buttonItemTop;
        private DevComponents.DotNetBar.ButtonItem buttonItemSpeed;
        private DevComponents.DotNetBar.ButtonItem buttonItem1000000000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItemZoom;

        private DevComponents.DotNetBar.ButtonItem buttonItem11;
        private DevComponents.DotNetBar.ButtonItem buttonItem12;
        private DevComponents.DotNetBar.ButtonItem buttonItemX1;
        private DevComponents.DotNetBar.ButtonItem buttonItemX10;
        private DevComponents.DotNetBar.ButtonItem buttonItemX100;
        private DevComponents.DotNetBar.ButtonItem buttonItemX1000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX10000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX100000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX1000000;

        private DevComponents.DotNetBar.ButtonItem buttonItemX10000000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX100000000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX1000000000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX10000000000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX100000000000;
        private DevComponents.DotNetBar.ButtonItem buttonItemX1000000000000;
        
        private DevComponents.DotNetBar.ButtonItem buttonItemTimeScaling;
        private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx1;
		private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx2;
		private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx4;
		private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx10;
		private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx100;
		private DevComponents.DotNetBar.ButtonItem buttonItemTimeScalingx1000;
		
        private DevComponents.DotNetBar.ButtonItem buttonItem1meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem10meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem100meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem1000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem10000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem100000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem1000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItemOrthographic;
        private DevComponents.DotNetBar.ButtonItem buttonItemIsometric;
        private DevComponents.DotNetBar.ButtonItem buttonItemFree;
        private DevComponents.DotNetBar.ButtonItem buttonItemBottom;
        private DevComponents.DotNetBar.ButtonItem buttonItemLeft;
        private DevComponents.DotNetBar.ButtonItem buttonItemRight;
        private DevComponents.DotNetBar.ButtonItem buttonItemFront;
        private DevComponents.DotNetBar.ButtonItem buttonItemBack;
        private DevComponents.DotNetBar.ButtonItem buttonTranslate;
        private DevComponents.DotNetBar.ButtonItem buttonRotate;
        private DevComponents.DotNetBar.ButtonItem buttonScale;
        private DevComponents.DotNetBar.ButtonItem buttonSelect;
        private DevComponents.DotNetBar.ButtonItem buttonPauseResume;
        private DevComponents.DotNetBar.ButtonItem buttonPlayMission;
        private DevComponents.DotNetBar.ButtonItem buttonItemZoomExtents;
        private DevComponents.DotNetBar.ButtonItem buttonItem10000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem100000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem1000000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem10000000000meterps;
        private DevComponents.DotNetBar.ButtonItem buttonItem299792458meterps;
        private DevComponents.DotNetBar.ColorPickerDropDown buttonTextColor;
        private DevComponents.DotNetBar.ButtonItem buttonView;
        private DevComponents.DotNetBar.ButtonItem buttonShowFPS;
        private DevComponents.DotNetBar.ButtonItem buttonShowLabels;
        private DevComponents.DotNetBar.ButtonItem buttonShowBoundingBoxes;
        private DevComponents.DotNetBar.ButtonItem buttonShowGrid;
		private DevComponents.DotNetBar.ButtonItem buttonShowLineProfiler;
        private DevComponents.DotNetBar.ButtonItem buttonShowCullingStats;
        private DevComponents.DotNetBar.ButtonItem buttonShowTVProfiler;
        private DevComponents.DotNetBar.ButtonItem buttonPlay;
    }
}