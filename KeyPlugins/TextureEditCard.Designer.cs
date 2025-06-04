namespace KeyPlugins
{
    partial class TextureEditCard
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
        	this.labelScaleU = new DevComponents.DotNetBar.LabelX();
        	this.labelScaleV = new DevComponents.DotNetBar.LabelX();
        	this.labelOffsetU = new DevComponents.DotNetBar.LabelX();
        	this.labelOffsetV = new DevComponents.DotNetBar.LabelX();
        	this.labelRotation = new DevComponents.DotNetBar.LabelX();
        	this.picTexture = new System.Windows.Forms.PictureBox();
        	this.numScaleU = new System.Windows.Forms.NumericUpDown();
        	this.numScaleV = new System.Windows.Forms.NumericUpDown();
        	this.numOffsetU = new System.Windows.Forms.NumericUpDown();
        	this.numOffsetV = new System.Windows.Forms.NumericUpDown();
        	this.numRotation = new System.Windows.Forms.NumericUpDown();
        	this.labelMod = new DevComponents.DotNetBar.LabelX();
        	this.labelEntry = new DevComponents.DotNetBar.LabelX();
        	this.labelDimensions = new DevComponents.DotNetBar.LabelX();
        	this.cbAlphaTest = new System.Windows.Forms.CheckBox();
        	this.groupPanel.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.picTexture)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numScaleU)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numScaleV)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numOffsetU)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numOffsetV)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numRotation)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// groupPanel
        	// 
        	this.groupPanel.Controls.Add(this.cbAlphaTest);
        	this.groupPanel.Controls.Add(this.labelDimensions);
        	this.groupPanel.Controls.Add(this.labelEntry);
        	this.groupPanel.Controls.Add(this.labelMod);
        	this.groupPanel.Controls.Add(this.numRotation);
        	this.groupPanel.Controls.Add(this.numOffsetV);
        	this.groupPanel.Controls.Add(this.numOffsetU);
        	this.groupPanel.Controls.Add(this.numScaleV);
        	this.groupPanel.Controls.Add(this.numScaleU);
        	this.groupPanel.Controls.Add(this.picTexture);
        	this.groupPanel.Controls.Add(this.labelRotation);
        	this.groupPanel.Controls.Add(this.labelOffsetV);
        	this.groupPanel.Controls.Add(this.labelOffsetU);
        	this.groupPanel.Controls.Add(this.labelScaleV);
        	this.groupPanel.Controls.Add(this.labelScaleU);
        	this.groupPanel.Size = new System.Drawing.Size(243, 207);
        	// 
        	// 
        	// 
        	this.groupPanel.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
        	this.groupPanel.Style.BackColorGradientAngle = 90;
        	this.groupPanel.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
        	this.groupPanel.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
        	this.groupPanel.Style.BorderBottomWidth = 1;
        	this.groupPanel.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.groupPanel.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
        	this.groupPanel.Style.BorderLeftWidth = 1;
        	this.groupPanel.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
        	this.groupPanel.Style.BorderRightWidth = 1;
        	this.groupPanel.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
        	this.groupPanel.Style.BorderTopWidth = 1;
        	this.groupPanel.Style.Class = "";
        	this.groupPanel.Style.CornerDiameter = 4;
        	this.groupPanel.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
        	this.groupPanel.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
        	this.groupPanel.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
        	this.groupPanel.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
        	// 
        	// 
        	// 
        	this.groupPanel.StyleMouseDown.Class = "";
        	this.groupPanel.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	// 
        	// 
        	// 
        	this.groupPanel.StyleMouseOver.Class = "";
        	this.groupPanel.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.groupPanel.Click += new System.EventHandler(this.GroupPanelClick);
        	// 
        	// buttonClose
        	// 
        	this.buttonClose.FlatAppearance.BorderSize = 0;
        	this.buttonClose.Location = new System.Drawing.Point(220, 0);
        	this.buttonClose.Size = new System.Drawing.Size(23, 20);
        	// 
        	// buttonBrowse
        	// 
        	this.buttonBrowse.FlatAppearance.BorderSize = 0;
        	this.buttonBrowse.Location = new System.Drawing.Point(202, 0);
        	this.buttonBrowse.Size = new System.Drawing.Size(21, 20);
        	this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowseClick);
        	// 
        	// labelScaleU
        	// 
        	// 
        	// 
        	// 
        	this.labelScaleU.BackgroundStyle.Class = "";
        	this.labelScaleU.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelScaleU.Location = new System.Drawing.Point(148, 3);
        	this.labelScaleU.Name = "labelScaleU";
        	this.labelScaleU.Size = new System.Drawing.Size(43, 17);
        	this.labelScaleU.TabIndex = 13;
        	this.labelScaleU.Text = "ScaleU";
        	// 
        	// labelScaleV
        	// 
        	// 
        	// 
        	// 
        	this.labelScaleV.BackgroundStyle.Class = "";
        	this.labelScaleV.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelScaleV.Location = new System.Drawing.Point(148, 23);
        	this.labelScaleV.Name = "labelScaleV";
        	this.labelScaleV.Size = new System.Drawing.Size(43, 17);
        	this.labelScaleV.TabIndex = 14;
        	this.labelScaleV.Text = "ScaleV";
        	// 
        	// labelOffsetU
        	// 
        	// 
        	// 
        	// 
        	this.labelOffsetU.BackgroundStyle.Class = "";
        	this.labelOffsetU.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelOffsetU.Location = new System.Drawing.Point(148, 43);
        	this.labelOffsetU.Name = "labelOffsetU";
        	this.labelOffsetU.Size = new System.Drawing.Size(43, 17);
        	this.labelOffsetU.TabIndex = 15;
        	this.labelOffsetU.Text = "OffsetU";
        	// 
        	// labelOffsetV
        	// 
        	// 
        	// 
        	// 
        	this.labelOffsetV.BackgroundStyle.Class = "";
        	this.labelOffsetV.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelOffsetV.Location = new System.Drawing.Point(148, 63);
        	this.labelOffsetV.Name = "labelOffsetV";
        	this.labelOffsetV.Size = new System.Drawing.Size(43, 17);
        	this.labelOffsetV.TabIndex = 16;
        	this.labelOffsetV.Text = "OffsetV";
        	// 
        	// labelRotation
        	// 
        	// 
        	// 
        	// 
        	this.labelRotation.BackgroundStyle.Class = "";
        	this.labelRotation.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelRotation.Location = new System.Drawing.Point(148, 83);
        	this.labelRotation.Name = "labelRotation";
        	this.labelRotation.Size = new System.Drawing.Size(43, 17);
        	this.labelRotation.TabIndex = 17;
        	this.labelRotation.Text = "Rotation";
        	// 
        	// picTexture
        	// 
        	this.picTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.picTexture.Location = new System.Drawing.Point(3, 3);
        	this.picTexture.Name = "picTexture";
        	this.picTexture.Size = new System.Drawing.Size(100, 64);
        	this.picTexture.TabIndex = 18;
        	this.picTexture.TabStop = false;
        	// 
        	// numScaleU
        	// 
        	this.numScaleU.Increment = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleU.Location = new System.Drawing.Point(191, 0);
        	this.numScaleU.Minimum = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleU.Name = "numScaleU";
        	this.numScaleU.Size = new System.Drawing.Size(42, 20);
        	this.numScaleU.TabIndex = 21;
        	this.numScaleU.Value = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleU.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
        	// 
        	// numScaleV
        	// 
        	this.numScaleV.Increment = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleV.Location = new System.Drawing.Point(191, 20);
        	this.numScaleV.Minimum = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleV.Name = "numScaleV";
        	this.numScaleV.Size = new System.Drawing.Size(42, 20);
        	this.numScaleV.TabIndex = 22;
        	this.numScaleV.Value = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numScaleV.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
        	// 
        	// numOffsetU
        	// 
        	this.numOffsetU.Increment = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numOffsetU.Location = new System.Drawing.Point(191, 40);
        	this.numOffsetU.Maximum = new decimal(new int[] {
        	        	        	10,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numOffsetU.Name = "numOffsetU";
        	this.numOffsetU.Size = new System.Drawing.Size(42, 20);
        	this.numOffsetU.TabIndex = 23;
        	this.numOffsetU.ValueChanged += new System.EventHandler(this.numOffset_ValueChanged);
        	// 
        	// numOffsetV
        	// 
        	this.numOffsetV.Increment = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numOffsetV.Location = new System.Drawing.Point(191, 60);
        	this.numOffsetV.Maximum = new decimal(new int[] {
        	        	        	10,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numOffsetV.Name = "numOffsetV";
        	this.numOffsetV.Size = new System.Drawing.Size(42, 20);
        	this.numOffsetV.TabIndex = 24;
        	this.numOffsetV.ValueChanged += new System.EventHandler(this.numOffset_ValueChanged);
        	// 
        	// numRotation
        	// 
        	this.numRotation.Increment = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	65536});
        	this.numRotation.Location = new System.Drawing.Point(191, 80);
        	this.numRotation.Maximum = new decimal(new int[] {
        	        	        	360,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.numRotation.Name = "numRotation";
        	this.numRotation.Size = new System.Drawing.Size(42, 20);
        	this.numRotation.TabIndex = 25;
        	this.numRotation.Value = new decimal(new int[] {
        	        	        	1,
        	        	        	0,
        	        	        	0,
        	        	        	131072});
        	this.numRotation.ValueChanged += new System.EventHandler(this.numRotation_ValueChanged);
        	// 
        	// labelMod
        	// 
        	// 
        	// 
        	// 
        	this.labelMod.BackgroundStyle.Class = "";
        	this.labelMod.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelMod.Location = new System.Drawing.Point(3, 130);
        	this.labelMod.Name = "labelMod";
        	this.labelMod.Size = new System.Drawing.Size(230, 18);
        	this.labelMod.TabIndex = 28;
        	this.labelMod.Text = "Archive:";
        	this.labelMod.TextLineAlignment = System.Drawing.StringAlignment.Near;
        	this.labelMod.WordWrap = true;
        	// 
        	// labelEntry
        	// 
        	// 
        	// 
        	// 
        	this.labelEntry.BackgroundStyle.Class = "";
        	this.labelEntry.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelEntry.Location = new System.Drawing.Point(3, 106);
        	this.labelEntry.Name = "labelEntry";
        	this.labelEntry.Size = new System.Drawing.Size(230, 18);
        	this.labelEntry.TabIndex = 29;
        	this.labelEntry.Text = "Entry:";
        	this.labelEntry.TextLineAlignment = System.Drawing.StringAlignment.Near;
        	this.labelEntry.WordWrap = true;
        	// 
        	// labelDimensions
        	// 
        	// 
        	// 
        	// 
        	this.labelDimensions.BackgroundStyle.Class = "";
        	this.labelDimensions.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelDimensions.Location = new System.Drawing.Point(97, 3);
        	this.labelDimensions.Name = "labelDimensions";
        	this.labelDimensions.Size = new System.Drawing.Size(49, 64);
        	this.labelDimensions.TabIndex = 32;
        	this.labelDimensions.Text = "W: 512\r\nH: 512\r\nBpp: 32";
        	// 
        	// cbAlphaTest
        	// 
        	this.cbAlphaTest.Location = new System.Drawing.Point(3, 73);
        	this.cbAlphaTest.Name = "cbAlphaTest";
        	this.cbAlphaTest.Size = new System.Drawing.Size(104, 24);
        	this.cbAlphaTest.TabIndex = 33;
        	this.cbAlphaTest.Text = "Alpha Test ";
        	this.cbAlphaTest.UseVisualStyleBackColor = true;
        	this.cbAlphaTest.CheckedChanged += new System.EventHandler(this.CbAlphaTestCheckedChanged);
        	// 
        	// TextureEditCard
        	// 
        	this.AllowDrop = true;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Name = "TextureEditCard";
        	this.Size = new System.Drawing.Size(243, 207);
        	this.groupPanel.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.picTexture)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numScaleU)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numScaleV)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numOffsetU)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numOffsetV)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numRotation)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.CheckBox cbAlphaTest;

        #endregion

        private DevComponents.DotNetBar.LabelX labelScaleU;
        private System.Windows.Forms.PictureBox picTexture;
        private DevComponents.DotNetBar.LabelX labelRotation;
        private DevComponents.DotNetBar.LabelX labelOffsetV;
        private DevComponents.DotNetBar.LabelX labelOffsetU;
        private DevComponents.DotNetBar.LabelX labelScaleV;
        private System.Windows.Forms.NumericUpDown numRotation;
        private System.Windows.Forms.NumericUpDown numOffsetV;
        private System.Windows.Forms.NumericUpDown numOffsetU;
        private System.Windows.Forms.NumericUpDown numScaleV;
        private System.Windows.Forms.NumericUpDown numScaleU;
        private DevComponents.DotNetBar.LabelX labelEntry;
        private DevComponents.DotNetBar.LabelX labelMod;
        private DevComponents.DotNetBar.LabelX labelDimensions;

    }
}
