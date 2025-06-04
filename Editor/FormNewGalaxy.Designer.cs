namespace KeyEdit
{
    partial class FormNewGalaxy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewGalaxy));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDiameterConversion = new System.Windows.Forms.Label();
            this.textRegionDiameter = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.textRegionsDeep = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.textRegionsHigh = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.textRegionsAcross = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textUniverseName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelName = new System.Windows.Forms.Label();
            this.labelNumSectors = new System.Windows.Forms.Label();
            this.labelSectorDiameter = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbCreateStarDigest = new System.Windows.Forms.CheckBox();
            this.cbEmpty = new System.Windows.Forms.CheckBox();
            this.labelSeed = new System.Windows.Forms.Label();
            this.textRandomSeed = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.textDensity = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelDensity = new System.Windows.Forms.Label();
            this.cbPlanetoidBelts = new System.Windows.Forms.CheckBox();
            this.cbMoons = new System.Windows.Forms.CheckBox();
            this.cbPlanets = new System.Windows.Forms.CheckBox();
            this.cbStars = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.labelGalaxyDimensions = new System.Windows.Forms.Label();
            this.cbSerializeEmptyZones = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(337, 443);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(70, 29);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(413, 443);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(65, 29);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDiameterConversion
            // 
            this.labelDiameterConversion.AutoSize = true;
            this.labelDiameterConversion.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDiameterConversion.Location = new System.Drawing.Point(197, 310);
            this.labelDiameterConversion.Name = "labelDiameterConversion";
            this.labelDiameterConversion.Size = new System.Drawing.Size(96, 16);
            this.labelDiameterConversion.TabIndex = 9;
            this.labelDiameterConversion.Text = "Km [ AU]  [ LY]";
            // 
            // textRegionDiameter
            // 
            // 
            // 
            // 
            this.textRegionDiameter.Border.Class = "TextBoxBorder";
            this.textRegionDiameter.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textRegionDiameter.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRegionDiameter.Location = new System.Drawing.Point(179, 86);
            this.textRegionDiameter.MaxLength = 15;
            this.textRegionDiameter.Name = "textRegionDiameter";
            this.textRegionDiameter.Size = new System.Drawing.Size(121, 22);
            this.textRegionDiameter.TabIndex = 8;
            this.textRegionDiameter.Tag = "74799000000000";
            this.textRegionDiameter.Text = "74799000000000";
            this.textRegionDiameter.TextChanged += new System.EventHandler(this.textRegionDiameter_TextChanged);
            // 
            // textRegionsDeep
            // 
            // 
            // 
            // 
            this.textRegionsDeep.Border.Class = "TextBoxBorder";
            this.textRegionsDeep.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textRegionsDeep.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRegionsDeep.Location = new System.Drawing.Point(267, 52);
            this.textRegionsDeep.MaxLength = 4;
            this.textRegionsDeep.Name = "textRegionsDeep";
            this.textRegionsDeep.Size = new System.Drawing.Size(61, 22);
            this.textRegionsDeep.TabIndex = 7;
            this.textRegionsDeep.Text = "1";
            this.textRegionsDeep.TextChanged += new System.EventHandler(this.textRegionsDeep_TextChanged);
            // 
            // textRegionsHigh
            // 
            // 
            // 
            // 
            this.textRegionsHigh.Border.Class = "TextBoxBorder";
            this.textRegionsHigh.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textRegionsHigh.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRegionsHigh.Location = new System.Drawing.Point(200, 52);
            this.textRegionsHigh.MaxLength = 4;
            this.textRegionsHigh.Name = "textRegionsHigh";
            this.textRegionsHigh.Size = new System.Drawing.Size(61, 22);
            this.textRegionsHigh.TabIndex = 6;
            this.textRegionsHigh.Text = "1";
            this.textRegionsHigh.TextChanged += new System.EventHandler(this.textRegionsHigh_TextChanged);
            // 
            // textRegionsAcross
            // 
            // 
            // 
            // 
            this.textRegionsAcross.Border.Class = "TextBoxBorder";
            this.textRegionsAcross.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textRegionsAcross.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRegionsAcross.Location = new System.Drawing.Point(133, 52);
            this.textRegionsAcross.MaxLength = 4;
            this.textRegionsAcross.Name = "textRegionsAcross";
            this.textRegionsAcross.Size = new System.Drawing.Size(61, 22);
            this.textRegionsAcross.TabIndex = 5;
            this.textRegionsAcross.Text = "1";
            this.textRegionsAcross.WordWrap = false;
            this.textRegionsAcross.TextChanged += new System.EventHandler(this.textRegionsAcross_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 299);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 98);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Star System Compositions";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(22, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = "Trinary Systems";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(22, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 16);
            this.label4.TabIndex = 13;
            this.label4.Text = "Binary Systems";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(21, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Single Star ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 389);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(194, 91);
            this.label6.TabIndex = 13;
            this.label6.Text = resources.GetString("label6.Text");
            // 
            // textUniverseName
            // 
            // 
            // 
            // 
            this.textUniverseName.Border.Class = "TextBoxBorder";
            this.textUniverseName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textUniverseName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textUniverseName.Location = new System.Drawing.Point(133, 17);
            this.textUniverseName.MaxLength = 128;
            this.textUniverseName.Name = "textUniverseName";
            this.textUniverseName.Size = new System.Drawing.Size(133, 22);
            this.textUniverseName.TabIndex = 14;
            this.textUniverseName.Text = "Campaign001";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(12, 23);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(95, 16);
            this.labelName.TabIndex = 15;
            this.labelName.Text = "Universe Name";
            // 
            // labelNumSectors
            // 
            this.labelNumSectors.AutoSize = true;
            this.labelNumSectors.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumSectors.Location = new System.Drawing.Point(12, 59);
            this.labelNumSectors.Name = "labelNumSectors";
            this.labelNumSectors.Size = new System.Drawing.Size(116, 16);
            this.labelNumSectors.TabIndex = 16;
            this.labelNumSectors.Text = "Number of Sectors";
            // 
            // labelSectorDiameter
            // 
            this.labelSectorDiameter.AutoSize = true;
            this.labelSectorDiameter.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSectorDiameter.Location = new System.Drawing.Point(12, 86);
            this.labelSectorDiameter.Name = "labelSectorDiameter";
            this.labelSectorDiameter.Size = new System.Drawing.Size(154, 16);
            this.labelSectorDiameter.TabIndex = 17;
            this.labelSectorDiameter.Text = "Sector Diameter (meters)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbCreateStarDigest);
            this.groupBox2.Controls.Add(this.cbEmpty);
            this.groupBox2.Controls.Add(this.labelSeed);
            this.groupBox2.Controls.Add(this.textRandomSeed);
            this.groupBox2.Controls.Add(this.textDensity);
            this.groupBox2.Controls.Add(this.labelDensity);
            this.groupBox2.Controls.Add(this.cbPlanetoidBelts);
            this.groupBox2.Controls.Add(this.cbMoons);
            this.groupBox2.Controls.Add(this.cbPlanets);
            this.groupBox2.Controls.Add(this.cbStars);
            this.groupBox2.Location = new System.Drawing.Point(12, 154);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(466, 139);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "What to Generate?";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // cbCreateStarDigest
            // 
            this.cbCreateStarDigest.AutoSize = true;
            this.cbCreateStarDigest.Checked = true;
            this.cbCreateStarDigest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCreateStarDigest.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCreateStarDigest.Location = new System.Drawing.Point(72, 35);
            this.cbCreateStarDigest.Name = "cbCreateStarDigest";
            this.cbCreateStarDigest.Size = new System.Drawing.Size(134, 20);
            this.cbCreateStarDigest.TabIndex = 28;
            this.cbCreateStarDigest.Text = "Create Star Digest";
            this.cbCreateStarDigest.UseVisualStyleBackColor = true;
            // 
            // cbEmpty
            // 
            this.cbEmpty.AutoSize = true;
            this.cbEmpty.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEmpty.Location = new System.Drawing.Point(8, 15);
            this.cbEmpty.Name = "cbEmpty";
            this.cbEmpty.Size = new System.Drawing.Size(147, 20);
            this.cbEmpty.TabIndex = 27;
            this.cbEmpty.Text = "Empty Regions Only";
            this.cbEmpty.UseVisualStyleBackColor = true;
            // 
            // labelSeed
            // 
            this.labelSeed.AutoSize = true;
            this.labelSeed.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSeed.Location = new System.Drawing.Point(282, 16);
            this.labelSeed.Name = "labelSeed";
            this.labelSeed.Size = new System.Drawing.Size(94, 16);
            this.labelSeed.TabIndex = 26;
            this.labelSeed.Text = "Random Seed:";
            // 
            // textRandomSeed
            // 
            // 
            // 
            // 
            this.textRandomSeed.Border.Class = "TextBoxBorder";
            this.textRandomSeed.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textRandomSeed.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRandomSeed.Location = new System.Drawing.Point(388, 10);
            this.textRandomSeed.MaxLength = 3;
            this.textRandomSeed.Name = "textRandomSeed";
            this.textRandomSeed.Size = new System.Drawing.Size(47, 22);
            this.textRandomSeed.TabIndex = 25;
            this.textRandomSeed.Text = "1";
            // 
            // textDensity
            // 
            // 
            // 
            // 
            this.textDensity.Border.Class = "TextBoxBorder";
            this.textDensity.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textDensity.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textDensity.Location = new System.Drawing.Point(235, 13);
            this.textDensity.MaxLength = 3;
            this.textDensity.Name = "textDensity";
            this.textDensity.Size = new System.Drawing.Size(47, 22);
            this.textDensity.TabIndex = 24;
            this.textDensity.Text = "20";
            // 
            // labelDensity
            // 
            this.labelDensity.AutoSize = true;
            this.labelDensity.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDensity.Location = new System.Drawing.Point(161, 15);
            this.labelDensity.Name = "labelDensity";
            this.labelDensity.Size = new System.Drawing.Size(68, 16);
            this.labelDensity.TabIndex = 23;
            this.labelDensity.Text = "Density %";
            // 
            // cbPlanetoidBelts
            // 
            this.cbPlanetoidBelts.AutoSize = true;
            this.cbPlanetoidBelts.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbPlanetoidBelts.Location = new System.Drawing.Point(8, 105);
            this.cbPlanetoidBelts.Name = "cbPlanetoidBelts";
            this.cbPlanetoidBelts.Size = new System.Drawing.Size(115, 20);
            this.cbPlanetoidBelts.TabIndex = 22;
            this.cbPlanetoidBelts.Text = "Planetoid Belts";
            this.cbPlanetoidBelts.UseVisualStyleBackColor = true;
            this.cbPlanetoidBelts.CheckedChanged += new System.EventHandler(this.cbPlanetoidBelts_CheckedChanged);
            // 
            // cbMoons
            // 
            this.cbMoons.AutoSize = true;
            this.cbMoons.Checked = true;
            this.cbMoons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMoons.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMoons.Location = new System.Drawing.Point(8, 82);
            this.cbMoons.Name = "cbMoons";
            this.cbMoons.Size = new System.Drawing.Size(66, 20);
            this.cbMoons.TabIndex = 21;
            this.cbMoons.Text = "Moons";
            this.cbMoons.UseVisualStyleBackColor = true;
            this.cbMoons.CheckedChanged += new System.EventHandler(this.cbMoons_CheckedChanged);
            // 
            // cbPlanets
            // 
            this.cbPlanets.AutoSize = true;
            this.cbPlanets.Checked = true;
            this.cbPlanets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbPlanets.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbPlanets.Location = new System.Drawing.Point(8, 58);
            this.cbPlanets.Name = "cbPlanets";
            this.cbPlanets.Size = new System.Drawing.Size(71, 20);
            this.cbPlanets.TabIndex = 20;
            this.cbPlanets.Text = "Planets";
            this.cbPlanets.UseVisualStyleBackColor = true;
            this.cbPlanets.CheckedChanged += new System.EventHandler(this.cbPlanets_CheckedChanged);
            // 
            // cbStars
            // 
            this.cbStars.AutoSize = true;
            this.cbStars.Checked = true;
            this.cbStars.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStars.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbStars.Location = new System.Drawing.Point(8, 35);
            this.cbStars.Name = "cbStars";
            this.cbStars.Size = new System.Drawing.Size(58, 20);
            this.cbStars.TabIndex = 19;
            this.cbStars.Text = "Stars";
            this.cbStars.UseVisualStyleBackColor = true;
            this.cbStars.CheckedChanged += new System.EventHandler(this.cbStars_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton2);
            this.groupBox3.Controls.Add(this.radioButton1);
            this.groupBox3.Location = new System.Drawing.Point(316, 86);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(162, 74);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Generation Mode";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(20, 44);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(95, 17);
            this.radioButton2.TabIndex = 26;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Real Star Data";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(20, 21);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(100, 17);
            this.radioButton1.TabIndex = 25;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Random Galaxy";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // labelGalaxyDimensions
            // 
            this.labelGalaxyDimensions.AutoSize = true;
            this.labelGalaxyDimensions.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGalaxyDimensions.Location = new System.Drawing.Point(302, 310);
            this.labelGalaxyDimensions.Name = "labelGalaxyDimensions";
            this.labelGalaxyDimensions.Size = new System.Drawing.Size(145, 16);
            this.labelGalaxyDimensions.TabIndex = 28;
            this.labelGalaxyDimensions.Text = "Galaxy Dimension (LY):";
            // 
            // cbSerializeEmptyZones
            // 
            this.cbSerializeEmptyZones.AutoSize = true;
            this.cbSerializeEmptyZones.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbSerializeEmptyZones.Location = new System.Drawing.Point(337, 52);
            this.cbSerializeEmptyZones.Name = "cbSerializeEmptyZones";
            this.cbSerializeEmptyZones.Size = new System.Drawing.Size(158, 20);
            this.cbSerializeEmptyZones.TabIndex = 29;
            this.cbSerializeEmptyZones.Text = "Serialize Empty Zones";
            this.cbSerializeEmptyZones.UseVisualStyleBackColor = true;
            // 
            // FormNewGalaxy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 484);
            this.Controls.Add(this.cbSerializeEmptyZones);
            this.Controls.Add(this.labelGalaxyDimensions);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.labelSectorDiameter);
            this.Controls.Add(this.labelNumSectors);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.textUniverseName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textRegionDiameter);
            this.Controls.Add(this.textRegionsDeep);
            this.Controls.Add(this.labelDiameterConversion);
            this.Controls.Add(this.textRegionsHigh);
            this.Controls.Add(this.textRegionsAcross);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewGalaxy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Galaxy Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.CheckBox cbCreateStarDigest;

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDiameterConversion;
        private DevComponents.DotNetBar.Controls.TextBoxX textRegionDiameter;
        private DevComponents.DotNetBar.Controls.TextBoxX textRegionsDeep;
        private DevComponents.DotNetBar.Controls.TextBoxX textRegionsHigh;
        private DevComponents.DotNetBar.Controls.TextBoxX textRegionsAcross;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private DevComponents.DotNetBar.Controls.TextBoxX textUniverseName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelNumSectors;
        private System.Windows.Forms.Label labelSectorDiameter;
        private System.Windows.Forms.GroupBox groupBox2;
        private DevComponents.DotNetBar.Controls.TextBoxX textDensity;
        private System.Windows.Forms.Label labelDensity;
        private System.Windows.Forms.CheckBox cbPlanetoidBelts;
        private System.Windows.Forms.CheckBox cbMoons;
        private System.Windows.Forms.CheckBox cbPlanets;
        private System.Windows.Forms.CheckBox cbStars;
        private System.Windows.Forms.Label labelSeed;
        private DevComponents.DotNetBar.Controls.TextBoxX textRandomSeed;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox cbEmpty;
        private System.Windows.Forms.Label labelGalaxyDimensions;
        private System.Windows.Forms.CheckBox cbSerializeEmptyZones;

    }
}