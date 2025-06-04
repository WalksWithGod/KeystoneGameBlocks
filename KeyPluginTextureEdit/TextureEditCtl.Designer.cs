namespace KeyPluginTextureEdit
{
    partial class TextureEditCtl
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
            this.labelResourcePath = new System.Windows.Forms.Label();
            this.nudTileU = new System.Windows.Forms.NumericUpDown();
            this.nudTileV = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelResourceStatus = new System.Windows.Forms.Label();
            this.buttonConvert = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudTileU)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTileV)).BeginInit();
            this.SuspendLayout();
            // 
            // labelResourcePath
            // 
            this.labelResourcePath.AutoSize = true;
            this.labelResourcePath.Location = new System.Drawing.Point(55, 121);
            this.labelResourcePath.Name = "labelResourcePath";
            this.labelResourcePath.Size = new System.Drawing.Size(73, 13);
            this.labelResourcePath.TabIndex = 0;
            this.labelResourcePath.Text = "TextureEditCtl";
            // 
            // nudTileU
            // 
            this.nudTileU.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileU.Location = new System.Drawing.Point(56, 24);
            this.nudTileU.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileU.Name = "nudTileU";
            this.nudTileU.Size = new System.Drawing.Size(60, 20);
            this.nudTileU.TabIndex = 1;
            this.nudTileU.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileU.ValueChanged += new System.EventHandler(this.nudTileU_ValueChanged);
            // 
            // nudTileV
            // 
            this.nudTileV.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileV.Location = new System.Drawing.Point(56, 50);
            this.nudTileV.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileV.Name = "nudTileV";
            this.nudTileV.Size = new System.Drawing.Size(60, 20);
            this.nudTileV.TabIndex = 2;
            this.nudTileV.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTileV.ValueChanged += new System.EventHandler(this.nudTileV_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tile U:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Tile V:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Path\\\\";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(144, 28);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(68, 26);
            this.buttonBrowse.TabIndex = 6;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(375, 28);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(68, 26);
            this.buttonExport.TabIndex = 7;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(141, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "TV Factory Index:";
            // 
            // labelResourceStatus
            // 
            this.labelResourceStatus.AutoSize = true;
            this.labelResourceStatus.Location = new System.Drawing.Point(238, 57);
            this.labelResourceStatus.Name = "labelResourceStatus";
            this.labelResourceStatus.Size = new System.Drawing.Size(107, 13);
            this.labelResourceStatus.TabIndex = 9;
            this.labelResourceStatus.Text = "(resource not loaded)";
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(292, 28);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(68, 26);
            this.buttonConvert.TabIndex = 10;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // TextureEditCtl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.labelResourceStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nudTileV);
            this.Controls.Add(this.nudTileU);
            this.Controls.Add(this.labelResourcePath);
            this.Name = "TextureEditCtl";
            this.Size = new System.Drawing.Size(511, 150);
            ((System.ComponentModel.ISupportInitialize)(this.nudTileU)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTileV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelResourcePath;
        private System.Windows.Forms.NumericUpDown nudTileU;
        private System.Windows.Forms.NumericUpDown nudTileV;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelResourceStatus;
        private System.Windows.Forms.Button buttonConvert;
    }
}
