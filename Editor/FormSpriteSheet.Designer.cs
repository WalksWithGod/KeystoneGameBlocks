namespace KeyEdit
{
    partial class FormSpriteSheet
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
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.cboTextureWidth = new System.Windows.Forms.ComboBox();
            this.labelTextureWidth = new System.Windows.Forms.Label();
            this.labelTextureHeight = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lableOutputPath = new System.Windows.Forms.Label();
            this.buttonBrowsePath = new System.Windows.Forms.Button();
            this.checkAlpha = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(399, 465);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(128, 51);
            this.buttonGenerate.TabIndex = 0;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.button1_Click);
            // 
            // cboTextureWidth
            // 
            this.cboTextureWidth.FormattingEnabled = true;
            this.cboTextureWidth.Items.AddRange(new object[] {
            "256",
            "512",
            "1024",
            "2048",
            "4096"});
            this.cboTextureWidth.Location = new System.Drawing.Point(114, 54);
            this.cboTextureWidth.Name = "cboTextureWidth";
            this.cboTextureWidth.Size = new System.Drawing.Size(148, 21);
            this.cboTextureWidth.TabIndex = 1;
            this.cboTextureWidth.Text = "4096";
            // 
            // labelTextureWidth
            // 
            this.labelTextureWidth.AutoSize = true;
            this.labelTextureWidth.Location = new System.Drawing.Point(11, 54);
            this.labelTextureWidth.Name = "labelTextureWidth";
            this.labelTextureWidth.Size = new System.Drawing.Size(77, 13);
            this.labelTextureWidth.TabIndex = 2;
            this.labelTextureWidth.Text = "Texture Width:";
            // 
            // labelTextureHeight
            // 
            this.labelTextureHeight.AutoSize = true;
            this.labelTextureHeight.Location = new System.Drawing.Point(11, 81);
            this.labelTextureHeight.Name = "labelTextureHeight";
            this.labelTextureHeight.Size = new System.Drawing.Size(80, 13);
            this.labelTextureHeight.TabIndex = 4;
            this.labelTextureHeight.Text = "Texture Height:";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "256",
            "512",
            "1024",
            "2048",
            "4096"});
            this.comboBox1.Location = new System.Drawing.Point(114, 81);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(148, 21);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.Text = "4096";
            // 
            // lableOutputPath
            // 
            this.lableOutputPath.AutoSize = true;
            this.lableOutputPath.Location = new System.Drawing.Point(10, 21);
            this.lableOutputPath.Name = "lableOutputPath";
            this.lableOutputPath.Size = new System.Drawing.Size(70, 13);
            this.lableOutputPath.TabIndex = 5;
            this.lableOutputPath.Text = "Output Path::";
            // 
            // buttonBrowsePath
            // 
            this.buttonBrowsePath.Location = new System.Drawing.Point(86, 12);
            this.buttonBrowsePath.Name = "buttonBrowsePath";
            this.buttonBrowsePath.Size = new System.Drawing.Size(473, 22);
            this.buttonBrowsePath.TabIndex = 6;
            this.buttonBrowsePath.Text = "...";
            this.buttonBrowsePath.UseVisualStyleBackColor = true;
            // 
            // checkAlpha
            // 
            this.checkAlpha.AutoSize = true;
            this.checkAlpha.Location = new System.Drawing.Point(114, 118);
            this.checkAlpha.Name = "checkAlpha";
            this.checkAlpha.Size = new System.Drawing.Size(82, 17);
            this.checkAlpha.TabIndex = 7;
            this.checkAlpha.Text = "Alpha Mask";
            this.checkAlpha.UseVisualStyleBackColor = true;
            // 
            // FormSpriteSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 533);
            this.Controls.Add(this.checkAlpha);
            this.Controls.Add(this.buttonBrowsePath);
            this.Controls.Add(this.lableOutputPath);
            this.Controls.Add(this.labelTextureHeight);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.labelTextureWidth);
            this.Controls.Add(this.cboTextureWidth);
            this.Controls.Add(this.buttonGenerate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSpriteSheet";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FormSpriteSheet";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ComboBox cboTextureWidth;
        private System.Windows.Forms.Label labelTextureWidth;
        private System.Windows.Forms.Label labelTextureHeight;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label lableOutputPath;
        private System.Windows.Forms.Button buttonBrowsePath;
        private System.Windows.Forms.CheckBox checkAlpha;
    }
}