/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 6/8/2014
 * Time: 9:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace KeyEdit
{
	partial class FormNewTerrainScene
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.cbSerializeEmptyZones = new System.Windows.Forms.CheckBox();
			this.labelSectorWidth = new System.Windows.Forms.Label();
			this.labelNumSectors = new System.Windows.Forms.Label();
			this.labelName = new System.Windows.Forms.Label();
			this.textSceneName = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.textRegionsDeep = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.textRegionsHigh = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.textRegionsAcross = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.cbAddWater = new System.Windows.Forms.CheckBox();
			this.labelWaterLevel = new System.Windows.Forms.Label();
			this.txtWaterLevel = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.cbAddTerrainGeometry = new System.Windows.Forms.CheckBox();
			this.labelSectorDepth = new System.Windows.Forms.Label();
			this.labelInfoCityBlock = new System.Windows.Forms.Label();
			this.lblTerrainPrecision = new System.Windows.Forms.Label();
			this.textBoxTerrainPrecision = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.labelSectorHeight = new System.Windows.Forms.Label();
			this.lblSectorResolution = new System.Windows.Forms.Label();
			this.lblTileSize = new System.Windows.Forms.Label();
			this.textTileSizeZ = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.textTileSizeY = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.textTileSizeX = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.lblWorldHeight = new System.Windows.Forms.Label();
			this.lblWorldDepth = new System.Windows.Forms.Label();
			this.lblWorldWidth = new System.Windows.Forms.Label();
			this.labelOctreeDepth = new System.Windows.Forms.Label();
			this.lblTerrainDepth = new System.Windows.Forms.Label();
			this.cboSectorResolution = new System.Windows.Forms.ComboBox();
			this.cboTerrainTileCountY = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// cbSerializeEmptyZones
			// 
			this.cbSerializeEmptyZones.AutoSize = true;
			this.cbSerializeEmptyZones.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbSerializeEmptyZones.Location = new System.Drawing.Point(332, 39);
			this.cbSerializeEmptyZones.Name = "cbSerializeEmptyZones";
			this.cbSerializeEmptyZones.Size = new System.Drawing.Size(168, 20);
			this.cbSerializeEmptyZones.TabIndex = 38;
			this.cbSerializeEmptyZones.Text = "Serialize Empty Sectors";
			this.cbSerializeEmptyZones.UseVisualStyleBackColor = true;
			// 
			// labelSectorWidth
			// 
			this.labelSectorWidth.AutoSize = true;
			this.labelSectorWidth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSectorWidth.Location = new System.Drawing.Point(9, 156);
			this.labelSectorWidth.Name = "labelSectorWidth";
			this.labelSectorWidth.Size = new System.Drawing.Size(126, 16);
			this.labelSectorWidth.TabIndex = 37;
			this.labelSectorWidth.Text = "Zone Width (meters)";
			// 
			// labelNumSectors
			// 
			this.labelNumSectors.AutoSize = true;
			this.labelNumSectors.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNumSectors.Location = new System.Drawing.Point(9, 46);
			this.labelNumSectors.Name = "labelNumSectors";
			this.labelNumSectors.Size = new System.Drawing.Size(116, 16);
			this.labelNumSectors.TabIndex = 36;
			this.labelNumSectors.Text = "Number of Sectors";
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelName.Location = new System.Drawing.Point(7, 10);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(83, 16);
			this.labelName.TabIndex = 35;
			this.labelName.Text = "Scene Name";
			// 
			// textSceneName
			// 
			// 
			// 
			// 
			this.textSceneName.Border.Class = "TextBoxBorder";
			this.textSceneName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textSceneName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textSceneName.Location = new System.Drawing.Point(128, 4);
			this.textSceneName.MaxLength = 128;
			this.textSceneName.Name = "textSceneName";
			this.textSceneName.Size = new System.Drawing.Size(195, 22);
			this.textSceneName.TabIndex = 34;
			this.textSceneName.Text = "MultiZoneLandBasedScene001";
			// 
			// textRegionsDeep
			// 
			// 
			// 
			// 
			this.textRegionsDeep.Border.Class = "TextBoxBorder";
			this.textRegionsDeep.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textRegionsDeep.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textRegionsDeep.Location = new System.Drawing.Point(262, 39);
			this.textRegionsDeep.MaxLength = 4;
			this.textRegionsDeep.Name = "textRegionsDeep";
			this.textRegionsDeep.Size = new System.Drawing.Size(61, 22);
			this.textRegionsDeep.TabIndex = 32;
			this.textRegionsDeep.Text = "3";
			this.textRegionsDeep.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// textRegionsHigh
			// 
			// 
			// 
			// 
			this.textRegionsHigh.Border.Class = "TextBoxBorder";
			this.textRegionsHigh.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textRegionsHigh.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textRegionsHigh.Location = new System.Drawing.Point(195, 39);
			this.textRegionsHigh.MaxLength = 4;
			this.textRegionsHigh.Name = "textRegionsHigh";
			this.textRegionsHigh.Size = new System.Drawing.Size(61, 22);
			this.textRegionsHigh.TabIndex = 31;
			this.textRegionsHigh.Text = "1";
			this.textRegionsHigh.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// textRegionsAcross
			// 
			// 
			// 
			// 
			this.textRegionsAcross.Border.Class = "TextBoxBorder";
			this.textRegionsAcross.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textRegionsAcross.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textRegionsAcross.Location = new System.Drawing.Point(128, 39);
			this.textRegionsAcross.MaxLength = 4;
			this.textRegionsAcross.Name = "textRegionsAcross";
			this.textRegionsAcross.Size = new System.Drawing.Size(61, 22);
			this.textRegionsAcross.TabIndex = 30;
			this.textRegionsAcross.Text = "3";
			this.textRegionsAcross.WordWrap = false;
			this.textRegionsAcross.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(422, 423);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(65, 29);
			this.buttonOK.TabIndex = 40;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOKClick);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(346, 423);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(70, 29);
			this.buttonCancel.TabIndex = 39;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
			// 
			// cbAddWater
			// 
			this.cbAddWater.AutoSize = true;
			this.cbAddWater.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbAddWater.Location = new System.Drawing.Point(214, 334);
			this.cbAddWater.Name = "cbAddWater";
			this.cbAddWater.Size = new System.Drawing.Size(89, 20);
			this.cbAddWater.TabIndex = 41;
			this.cbAddWater.Text = "Add Water";
			this.cbAddWater.UseVisualStyleBackColor = true;
			// 
			// labelWaterLevel
			// 
			this.labelWaterLevel.AutoSize = true;
			this.labelWaterLevel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelWaterLevel.Location = new System.Drawing.Point(214, 357);
			this.labelWaterLevel.Name = "labelWaterLevel";
			this.labelWaterLevel.Size = new System.Drawing.Size(128, 16);
			this.labelWaterLevel.TabIndex = 43;
			this.labelWaterLevel.Text = "Water Level (meters)";
			// 
			// txtWaterLevel
			// 
			// 
			// 
			// 
			this.txtWaterLevel.Border.Class = "TextBoxBorder";
			this.txtWaterLevel.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.txtWaterLevel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtWaterLevel.Location = new System.Drawing.Point(214, 376);
			this.txtWaterLevel.MaxLength = 15;
			this.txtWaterLevel.Name = "txtWaterLevel";
			this.txtWaterLevel.Size = new System.Drawing.Size(121, 22);
			this.txtWaterLevel.TabIndex = 42;
			this.txtWaterLevel.Tag = "";
			this.txtWaterLevel.Text = "1000";
			// 
			// cbAddTerrainGeometry
			// 
			this.cbAddTerrainGeometry.AutoSize = true;
			this.cbAddTerrainGeometry.Checked = true;
			this.cbAddTerrainGeometry.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbAddTerrainGeometry.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbAddTerrainGeometry.Location = new System.Drawing.Point(9, 334);
			this.cbAddTerrainGeometry.Name = "cbAddTerrainGeometry";
			this.cbAddTerrainGeometry.Size = new System.Drawing.Size(197, 20);
			this.cbAddTerrainGeometry.TabIndex = 44;
			this.cbAddTerrainGeometry.Text = "Add Default Terrain Geometry";
			this.cbAddTerrainGeometry.UseVisualStyleBackColor = true;
			// 
			// labelSectorDepth
			// 
			this.labelSectorDepth.AutoSize = true;
			this.labelSectorDepth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSectorDepth.Location = new System.Drawing.Point(9, 210);
			this.labelSectorDepth.Name = "labelSectorDepth";
			this.labelSectorDepth.Size = new System.Drawing.Size(126, 16);
			this.labelSectorDepth.TabIndex = 46;
			this.labelSectorDepth.Text = "Zone Depth (meters)";
			// 
			// labelInfoCityBlock
			// 
			this.labelInfoCityBlock.AutoSize = true;
			this.labelInfoCityBlock.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInfoCityBlock.Location = new System.Drawing.Point(319, 156);
			this.labelInfoCityBlock.Name = "labelInfoCityBlock";
			this.labelInfoCityBlock.Size = new System.Drawing.Size(168, 64);
			this.labelInfoCityBlock.TabIndex = 49;
			this.labelInfoCityBlock.Text = "Common City Block sizes:\r\n75m x 75m, 100m x 100m, \r\n200m x 100m, \r\n120m x120m";
			// 
			// lblTerrainPrecision
			// 
			this.lblTerrainPrecision.AutoSize = true;
			this.lblTerrainPrecision.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTerrainPrecision.Location = new System.Drawing.Point(25, 357);
			this.lblTerrainPrecision.Name = "lblTerrainPrecision";
			this.lblTerrainPrecision.Size = new System.Drawing.Size(104, 16);
			this.lblTerrainPrecision.TabIndex = 51;
			this.lblTerrainPrecision.Text = "Terrain Precision";
			// 
			// textBoxTerrainPrecision
			// 
			// 
			// 
			// 
			this.textBoxTerrainPrecision.Border.Class = "TextBoxBorder";
			this.textBoxTerrainPrecision.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textBoxTerrainPrecision.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxTerrainPrecision.Location = new System.Drawing.Point(25, 376);
			this.textBoxTerrainPrecision.MaxLength = 15;
			this.textBoxTerrainPrecision.Name = "textBoxTerrainPrecision";
			this.textBoxTerrainPrecision.Size = new System.Drawing.Size(121, 22);
			this.textBoxTerrainPrecision.TabIndex = 50;
			this.textBoxTerrainPrecision.Tag = "";
			this.textBoxTerrainPrecision.Text = "Best";
			// 
			// labelSectorHeight
			// 
			this.labelSectorHeight.AutoSize = true;
			this.labelSectorHeight.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSectorHeight.Location = new System.Drawing.Point(9, 182);
			this.labelSectorHeight.Name = "labelSectorHeight";
			this.labelSectorHeight.Size = new System.Drawing.Size(129, 16);
			this.labelSectorHeight.TabIndex = 53;
			this.labelSectorHeight.Text = "Zone Height (meters)";
			// 
			// lblSectorResolution
			// 
			this.lblSectorResolution.AutoSize = true;
			this.lblSectorResolution.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSectorResolution.Location = new System.Drawing.Point(8, 101);
			this.lblSectorResolution.Name = "lblSectorResolution";
			this.lblSectorResolution.Size = new System.Drawing.Size(111, 16);
			this.lblSectorResolution.TabIndex = 57;
			this.lblSectorResolution.Text = "Sector Resolution";
			// 
			// lblTileSize
			// 
			this.lblTileSize.AutoSize = true;
			this.lblTileSize.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTileSize.Location = new System.Drawing.Point(10, 72);
			this.lblTileSize.Name = "lblTileSize";
			this.lblTileSize.Size = new System.Drawing.Size(58, 16);
			this.lblTileSize.TabIndex = 61;
			this.lblTileSize.Text = "Tile Size";
			// 
			// textTileSizeZ
			// 
			// 
			// 
			// 
			this.textTileSizeZ.Border.Class = "TextBoxBorder";
			this.textTileSizeZ.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textTileSizeZ.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textTileSizeZ.Location = new System.Drawing.Point(262, 67);
			this.textTileSizeZ.MaxLength = 4;
			this.textTileSizeZ.Name = "textTileSizeZ";
			this.textTileSizeZ.Size = new System.Drawing.Size(61, 22);
			this.textTileSizeZ.TabIndex = 60;
			this.textTileSizeZ.Text = "2.5";
			this.textTileSizeZ.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// textTileSizeY
			// 
			// 
			// 
			// 
			this.textTileSizeY.Border.Class = "TextBoxBorder";
			this.textTileSizeY.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textTileSizeY.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textTileSizeY.Location = new System.Drawing.Point(195, 67);
			this.textTileSizeY.MaxLength = 4;
			this.textTileSizeY.Name = "textTileSizeY";
			this.textTileSizeY.Size = new System.Drawing.Size(61, 22);
			this.textTileSizeY.TabIndex = 59;
			this.textTileSizeY.Text = "3.0";
			this.textTileSizeY.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// textTileSizeX
			// 
			// 
			// 
			// 
			this.textTileSizeX.Border.Class = "TextBoxBorder";
			this.textTileSizeX.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textTileSizeX.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textTileSizeX.Location = new System.Drawing.Point(128, 67);
			this.textTileSizeX.MaxLength = 4;
			this.textTileSizeX.Name = "textTileSizeX";
			this.textTileSizeX.Size = new System.Drawing.Size(61, 22);
			this.textTileSizeX.TabIndex = 58;
			this.textTileSizeX.Text = "2.5";
			this.textTileSizeX.WordWrap = false;
			this.textTileSizeX.TextChanged += new System.EventHandler(this.TextFieldChanged);
			// 
			// lblWorldHeight
			// 
			this.lblWorldHeight.AutoSize = true;
			this.lblWorldHeight.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorldHeight.Location = new System.Drawing.Point(9, 269);
			this.lblWorldHeight.Name = "lblWorldHeight";
			this.lblWorldHeight.Size = new System.Drawing.Size(135, 16);
			this.lblWorldHeight.TabIndex = 64;
			this.lblWorldHeight.Text = "World Height (meters)";
			// 
			// lblWorldDepth
			// 
			this.lblWorldDepth.AutoSize = true;
			this.lblWorldDepth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorldDepth.Location = new System.Drawing.Point(9, 297);
			this.lblWorldDepth.Name = "lblWorldDepth";
			this.lblWorldDepth.Size = new System.Drawing.Size(132, 16);
			this.lblWorldDepth.TabIndex = 63;
			this.lblWorldDepth.Text = "World Depth (meters)";
			// 
			// lblWorldWidth
			// 
			this.lblWorldWidth.AutoSize = true;
			this.lblWorldWidth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorldWidth.Location = new System.Drawing.Point(9, 243);
			this.lblWorldWidth.Name = "lblWorldWidth";
			this.lblWorldWidth.Size = new System.Drawing.Size(132, 16);
			this.lblWorldWidth.TabIndex = 62;
			this.lblWorldWidth.Text = "World Width (meters)";
			// 
			// labelOctreeDepth
			// 
			this.labelOctreeDepth.AutoSize = true;
			this.labelOctreeDepth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOctreeDepth.Location = new System.Drawing.Point(196, 101);
			this.labelOctreeDepth.Name = "labelOctreeDepth";
			this.labelOctreeDepth.Size = new System.Drawing.Size(108, 16);
			this.labelOctreeDepth.TabIndex = 65;
			this.labelOctreeDepth.Text = "Octree Depth = 5";
			// 
			// lblTerrainDepth
			// 
			this.lblTerrainDepth.AutoSize = true;
			this.lblTerrainDepth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTerrainDepth.Location = new System.Drawing.Point(10, 129);
			this.lblTerrainDepth.Name = "lblTerrainDepth";
			this.lblTerrainDepth.Size = new System.Drawing.Size(84, 16);
			this.lblTerrainDepth.TabIndex = 67;
			this.lblTerrainDepth.Text = "Terrain Depth";
			// 
			// cboSectorResolution
			// 
			this.cboSectorResolution.DisplayMember = "2";
			this.cboSectorResolution.FormattingEnabled = true;
			this.cboSectorResolution.Items.AddRange(new object[] {
			"8",
			"16",
			"32",
			"64",
			"128",
			"256",
			"512",
			"1024"});
			this.cboSectorResolution.Location = new System.Drawing.Point(128, 98);
			this.cboSectorResolution.Name = "cboSectorResolution";
			this.cboSectorResolution.Size = new System.Drawing.Size(61, 21);
			this.cboSectorResolution.TabIndex = 69;
			this.cboSectorResolution.Text = "32";
			this.cboSectorResolution.SelectedIndexChanged += new System.EventHandler(this.CboSectorResolutionSelectedIndexChanged);
			// 
			// cboTerrainTileCountY
			// 
			this.cboTerrainTileCountY.FormattingEnabled = true;
			this.cboTerrainTileCountY.Items.AddRange(new object[] {
			"8",
			"16",
			"32",
			"64",
			"128",
			"256",
			"512"});
			this.cboTerrainTileCountY.Location = new System.Drawing.Point(128, 124);
			this.cboTerrainTileCountY.Name = "cboTerrainTileCountY";
			this.cboTerrainTileCountY.Size = new System.Drawing.Size(61, 21);
			this.cboTerrainTileCountY.TabIndex = 70;
			this.cboTerrainTileCountY.Text = "32";
			// 
			// FormNewTerrainScene
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(499, 460);
			this.Controls.Add(this.cboTerrainTileCountY);
			this.Controls.Add(this.cboSectorResolution);
			this.Controls.Add(this.lblTerrainDepth);
			this.Controls.Add(this.labelOctreeDepth);
			this.Controls.Add(this.lblWorldHeight);
			this.Controls.Add(this.lblWorldDepth);
			this.Controls.Add(this.lblWorldWidth);
			this.Controls.Add(this.lblTileSize);
			this.Controls.Add(this.textTileSizeZ);
			this.Controls.Add(this.textTileSizeY);
			this.Controls.Add(this.textTileSizeX);
			this.Controls.Add(this.lblSectorResolution);
			this.Controls.Add(this.labelSectorHeight);
			this.Controls.Add(this.lblTerrainPrecision);
			this.Controls.Add(this.textBoxTerrainPrecision);
			this.Controls.Add(this.labelInfoCityBlock);
			this.Controls.Add(this.labelSectorDepth);
			this.Controls.Add(this.cbAddTerrainGeometry);
			this.Controls.Add(this.labelWaterLevel);
			this.Controls.Add(this.txtWaterLevel);
			this.Controls.Add(this.cbAddWater);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.cbSerializeEmptyZones);
			this.Controls.Add(this.labelSectorWidth);
			this.Controls.Add(this.labelNumSectors);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.textSceneName);
			this.Controls.Add(this.textRegionsDeep);
			this.Controls.Add(this.textRegionsHigh);
			this.Controls.Add(this.textRegionsAcross);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormNewTerrainScene";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FormNewTerrainScene";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Label lblWorldWidth;
		private System.Windows.Forms.Label lblWorldDepth;
		private System.Windows.Forms.Label lblWorldHeight;
		private DevComponents.DotNetBar.Controls.TextBoxX textTileSizeX;
		private DevComponents.DotNetBar.Controls.TextBoxX textTileSizeY;
		private DevComponents.DotNetBar.Controls.TextBoxX textTileSizeZ;
		private System.Windows.Forms.Label lblTileSize;
		private System.Windows.Forms.Label lblSectorResolution;
		private DevComponents.DotNetBar.Controls.TextBoxX textBoxTerrainPrecision;
		private System.Windows.Forms.Label lblTerrainPrecision;
		private System.Windows.Forms.Label labelInfoCityBlock;
		private System.Windows.Forms.Label labelSectorHeight;
		private System.Windows.Forms.Label labelSectorDepth;
		private System.Windows.Forms.CheckBox cbAddTerrainGeometry;
		private DevComponents.DotNetBar.Controls.TextBoxX txtWaterLevel;
		private System.Windows.Forms.Label labelWaterLevel;
		private System.Windows.Forms.CheckBox cbAddWater;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private DevComponents.DotNetBar.Controls.TextBoxX textRegionsAcross;
		private DevComponents.DotNetBar.Controls.TextBoxX textRegionsHigh;
		private DevComponents.DotNetBar.Controls.TextBoxX textRegionsDeep;
		private DevComponents.DotNetBar.Controls.TextBoxX textSceneName;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelNumSectors;
		private System.Windows.Forms.Label labelSectorWidth;
		private System.Windows.Forms.CheckBox cbSerializeEmptyZones;
		private System.Windows.Forms.Label labelOctreeDepth;
		private System.Windows.Forms.Label lblTerrainDepth;
		private System.Windows.Forms.ComboBox cboSectorResolution;
		private System.Windows.Forms.ComboBox cboTerrainTileCountY;
	}
}
