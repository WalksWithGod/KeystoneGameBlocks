namespace KeyEdit
{
    partial class FormNewVehicle
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
            this.cboCellCountX = new System.Windows.Forms.ComboBox();
            this.cboCellCountZ = new System.Windows.Forms.ComboBox();
            this.cboCellCountY = new System.Windows.Forms.ComboBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelDecks = new System.Windows.Forms.Label();
            this.labelLength = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelMaxTechlevel = new System.Windows.Forms.Label();
            this.cboTechLevel = new System.Windows.Forms.ComboBox();
            this.labelMaxCost = new System.Windows.Forms.Label();
            this.spinMaxCost = new IgNS.Controls.IgSpinEdit();
            this.textMeshResource = new System.Windows.Forms.TextBox();
            this.buttonBrowseAsset = new System.Windows.Forms.Button();
            this.labelDeckHeight = new System.Windows.Forms.Label();
            this.cboDeckHeight = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.spinMaxCost)).BeginInit();
            this.SuspendLayout();
            // 
            // cboCellCountX
            // 
            this.cboCellCountX.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboCellCountX.FormattingEnabled = true;
            this.cboCellCountX.Location = new System.Drawing.Point(128, 157);
            this.cboCellCountX.Margin = new System.Windows.Forms.Padding(4);
            this.cboCellCountX.Name = "cboCellCountX";
            this.cboCellCountX.Size = new System.Drawing.Size(114, 26);
            this.cboCellCountX.TabIndex = 0;
            this.cboCellCountX.Text = "5";
            // 
            // cboCellCountZ
            // 
            this.cboCellCountZ.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboCellCountZ.FormattingEnabled = true;
            this.cboCellCountZ.Location = new System.Drawing.Point(127, 192);
            this.cboCellCountZ.Margin = new System.Windows.Forms.Padding(4);
            this.cboCellCountZ.Name = "cboCellCountZ";
            this.cboCellCountZ.Size = new System.Drawing.Size(114, 26);
            this.cboCellCountZ.TabIndex = 1;
            this.cboCellCountZ.Text = "10";
            // 
            // cboCellCountY
            // 
            this.cboCellCountY.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboCellCountY.FormattingEnabled = true;
            this.cboCellCountY.Location = new System.Drawing.Point(127, 228);
            this.cboCellCountY.Margin = new System.Windows.Forms.Padding(4);
            this.cboCellCountY.Name = "cboCellCountY";
            this.cboCellCountY.Size = new System.Drawing.Size(114, 26);
            this.cboCellCountY.TabIndex = 2;
            this.cboCellCountY.Text = "2";
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOK.Location = new System.Drawing.Point(404, 225);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(103, 54);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWidth.Location = new System.Drawing.Point(13, 161);
            this.labelWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(107, 17);
            this.labelWidth.TabIndex = 4;
            this.labelWidth.Text = "Tiles Wide:";
            // 
            // labelDecks
            // 
            this.labelDecks.AutoSize = true;
            this.labelDecks.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDecks.Location = new System.Drawing.Point(13, 232);
            this.labelDecks.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDecks.Name = "labelDecks";
            this.labelDecks.Size = new System.Drawing.Size(80, 17);
            this.labelDecks.TabIndex = 5;
            this.labelDecks.Text = "# Decks:";
            // 
            // labelLength
            // 
            this.labelLength.AutoSize = true;
            this.labelLength.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLength.Location = new System.Drawing.Point(12, 196);
            this.labelLength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLength.Name = "labelLength";
            this.labelLength.Size = new System.Drawing.Size(107, 17);
            this.labelLength.TabIndex = 6;
            this.labelLength.Text = "Tiles Long:";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(294, 225);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(103, 54);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelMaxTechlevel
            // 
            this.labelMaxTechlevel.AutoSize = true;
            this.labelMaxTechlevel.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMaxTechlevel.Location = new System.Drawing.Point(5, 32);
            this.labelMaxTechlevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMaxTechlevel.Name = "labelMaxTechlevel";
            this.labelMaxTechlevel.Size = new System.Drawing.Size(89, 17);
            this.labelMaxTechlevel.TabIndex = 9;
            this.labelMaxTechlevel.Text = "Max Tech:";
            // 
            // cboTechLevel
            // 
            this.cboTechLevel.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboTechLevel.FormattingEnabled = true;
            this.cboTechLevel.Items.AddRange(new object[] {
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20"});
            this.cboTechLevel.Location = new System.Drawing.Point(102, 28);
            this.cboTechLevel.Margin = new System.Windows.Forms.Padding(4);
            this.cboTechLevel.Name = "cboTechLevel";
            this.cboTechLevel.Size = new System.Drawing.Size(114, 26);
            this.cboTechLevel.TabIndex = 8;
            this.cboTechLevel.Text = "8";
            // 
            // labelMaxCost
            // 
            this.labelMaxCost.AutoSize = true;
            this.labelMaxCost.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMaxCost.Location = new System.Drawing.Point(217, 36);
            this.labelMaxCost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMaxCost.Name = "labelMaxCost";
            this.labelMaxCost.Size = new System.Drawing.Size(89, 17);
            this.labelMaxCost.TabIndex = 11;
            this.labelMaxCost.Text = "Max Cost:";
            // 
            // spinMaxCost
            // 
            this.spinMaxCost.ExternalUpdate = false;
            this.spinMaxCost.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinMaxCost.FormatString = "{0:C}";
            this.spinMaxCost.Increament = 1;
            this.spinMaxCost.IncrementMax = 100000000;
            this.spinMaxCost.IncrementMin = 0.1;
            this.spinMaxCost.IncrementVisible = true;
            this.spinMaxCost.Location = new System.Drawing.Point(310, 28);
            this.spinMaxCost.Margin = new System.Windows.Forms.Padding(0);
            this.spinMaxCost.Name = "spinMaxCost";
            this.spinMaxCost.Padding = new System.Windows.Forms.Padding(3);
            this.spinMaxCost.Pow2Increment = false;
            this.spinMaxCost.Size = new System.Drawing.Size(197, 30);
            this.spinMaxCost.TabIndex = 12;
            this.spinMaxCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.spinMaxCost.Value = 10000000;
            this.spinMaxCost.ValueAsHex = false;
            this.spinMaxCost.ValueAsInt = true;
            this.spinMaxCost.ValueBackColor = System.Drawing.SystemColors.Window;
            this.spinMaxCost.ValueMax = 1000000000000;
            this.spinMaxCost.ValueMin = 1000;
            // 
            // textMeshResource
            // 
            this.textMeshResource.Location = new System.Drawing.Point(12, 77);
            this.textMeshResource.Name = "textMeshResource";
            this.textMeshResource.Size = new System.Drawing.Size(388, 27);
            this.textMeshResource.TabIndex = 13;
            // 
            // buttonBrowseAsset
            // 
            this.buttonBrowseAsset.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBrowseAsset.Location = new System.Drawing.Point(422, 77);
            this.buttonBrowseAsset.Margin = new System.Windows.Forms.Padding(4);
            this.buttonBrowseAsset.Name = "buttonBrowseAsset";
            this.buttonBrowseAsset.Size = new System.Drawing.Size(85, 25);
            this.buttonBrowseAsset.TabIndex = 14;
            this.buttonBrowseAsset.Text = "Browse...";
            this.buttonBrowseAsset.UseVisualStyleBackColor = true;
            this.buttonBrowseAsset.Click += new System.EventHandler(this.buttonBrowseAsset_Click);
            // 
            // labelDeckHeight
            // 
            this.labelDeckHeight.AutoSize = true;
            this.labelDeckHeight.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeckHeight.Location = new System.Drawing.Point(13, 125);
            this.labelDeckHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDeckHeight.Name = "labelDeckHeight";
            this.labelDeckHeight.Size = new System.Drawing.Size(116, 17);
            this.labelDeckHeight.TabIndex = 16;
            this.labelDeckHeight.Text = "Deck Height:";
            // 
            // cboDeckHeight
            // 
            this.cboDeckHeight.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDeckHeight.FormattingEnabled = true;
            this.cboDeckHeight.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2",
            "2.5",
            "3",
            "3.5",
            "4",
            "4.5",
            "5",
            "5.5",
            "6",
            "6.5",
            "7",
            "7.5",
            "8",
            "8.5",
            "9",
            "9.5",
            "10"});
            this.cboDeckHeight.Location = new System.Drawing.Point(128, 121);
            this.cboDeckHeight.Margin = new System.Windows.Forms.Padding(4);
            this.cboDeckHeight.Name = "cboDeckHeight";
            this.cboDeckHeight.Size = new System.Drawing.Size(114, 26);
            this.cboDeckHeight.TabIndex = 15;
            this.cboDeckHeight.Text = "2";
            // 
            // FormNewVehicle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 289);
            this.Controls.Add(this.labelDeckHeight);
            this.Controls.Add(this.cboDeckHeight);
            this.Controls.Add(this.buttonBrowseAsset);
            this.Controls.Add(this.textMeshResource);
            this.Controls.Add(this.spinMaxCost);
            this.Controls.Add(this.labelMaxCost);
            this.Controls.Add(this.labelMaxTechlevel);
            this.Controls.Add(this.cboTechLevel);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelLength);
            this.Controls.Add(this.labelDecks);
            this.Controls.Add(this.labelWidth);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.cboCellCountY);
            this.Controls.Add(this.cboCellCountZ);
            this.Controls.Add(this.cboCellCountX);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormNewVehicle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Vehicle Settings";
            ((System.ComponentModel.ISupportInitialize)(this.spinMaxCost)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboCellCountX;
        private System.Windows.Forms.ComboBox cboCellCountZ;
        private System.Windows.Forms.ComboBox cboCellCountY;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelDecks;
        private System.Windows.Forms.Label labelLength;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelMaxTechlevel;
        private System.Windows.Forms.ComboBox cboTechLevel;
        private System.Windows.Forms.Label labelMaxCost;
        private IgNS.Controls.IgSpinEdit spinMaxCost;
        private System.Windows.Forms.TextBox textMeshResource;
        private System.Windows.Forms.Button buttonBrowseAsset;
        private System.Windows.Forms.Label labelDeckHeight;
        private System.Windows.Forms.ComboBox cboDeckHeight;
    }
}