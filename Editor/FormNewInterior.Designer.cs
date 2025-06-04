namespace KeyEdit
{
    partial class FormNewInterior
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelCellWidth = new System.Windows.Forms.Label();
            this.labelCellHeight = new System.Windows.Forms.Label();
            this.labelCellDepth = new System.Windows.Forms.Label();
            this.numCellWidth = new System.Windows.Forms.NumericUpDown();
            this.numCellHeight = new System.Windows.Forms.NumericUpDown();
            this.numCellDepth = new System.Windows.Forms.NumericUpDown();
            this.numQuadtreeDepth = new System.Windows.Forms.NumericUpDown();
            this.labelQuadtreeDepth = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCellWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCellHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCellDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuadtreeDepth)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(91, 126);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(73, 37);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(170, 126);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(73, 37);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelCellWidth
            // 
            this.labelCellWidth.AutoSize = true;
            this.labelCellWidth.Location = new System.Drawing.Point(12, 31);
            this.labelCellWidth.Name = "labelCellWidth";
            this.labelCellWidth.Size = new System.Drawing.Size(58, 13);
            this.labelCellWidth.TabIndex = 2;
            this.labelCellWidth.Text = "Cell Width:";
            // 
            // labelCellHeight
            // 
            this.labelCellHeight.AutoSize = true;
            this.labelCellHeight.Location = new System.Drawing.Point(12, 60);
            this.labelCellHeight.Name = "labelCellHeight";
            this.labelCellHeight.Size = new System.Drawing.Size(61, 13);
            this.labelCellHeight.TabIndex = 3;
            this.labelCellHeight.Text = "Cell Height:";
            // 
            // labelCellDepth
            // 
            this.labelCellDepth.AutoSize = true;
            this.labelCellDepth.Location = new System.Drawing.Point(12, 87);
            this.labelCellDepth.Name = "labelCellDepth";
            this.labelCellDepth.Size = new System.Drawing.Size(59, 13);
            this.labelCellDepth.TabIndex = 4;
            this.labelCellDepth.Text = "Cell Depth:";
            // 
            // numCellWidth
            // 
            this.numCellWidth.DecimalPlaces = 2;
            this.numCellWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numCellWidth.Location = new System.Drawing.Point(91, 24);
            this.numCellWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numCellWidth.Name = "numCellWidth";
            this.numCellWidth.Size = new System.Drawing.Size(76, 20);
            this.numCellWidth.TabIndex = 5;
            this.numCellWidth.Value = new decimal(new int[] {
            25,
            0,
            0,
            65536});
            this.numCellWidth.ValueChanged += new System.EventHandler(this.numCellWidth_ValueChanged);
            // 
            // numCellHeight
            // 
            this.numCellHeight.DecimalPlaces = 2;
            this.numCellHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numCellHeight.Location = new System.Drawing.Point(91, 53);
            this.numCellHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numCellHeight.Name = "numCellHeight";
            this.numCellHeight.Size = new System.Drawing.Size(76, 20);
            this.numCellHeight.TabIndex = 6;
            this.numCellHeight.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // numCellDepth
            // 
            this.numCellDepth.DecimalPlaces = 2;
            this.numCellDepth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numCellDepth.Location = new System.Drawing.Point(91, 80);
            this.numCellDepth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numCellDepth.Name = "numCellDepth";
            this.numCellDepth.Size = new System.Drawing.Size(76, 20);
            this.numCellDepth.TabIndex = 7;
            this.numCellDepth.Value = new decimal(new int[] {
            25,
            0,
            0,
            65536});
            // 
            // numQuadtreeDepth
            // 
            this.numQuadtreeDepth.Location = new System.Drawing.Point(265, 24);
            this.numQuadtreeDepth.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numQuadtreeDepth.Name = "numQuadtreeDepth";
            this.numQuadtreeDepth.Size = new System.Drawing.Size(51, 20);
            this.numQuadtreeDepth.TabIndex = 9;
            this.numQuadtreeDepth.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // labelQuadtreeDepth
            // 
            this.labelQuadtreeDepth.AutoSize = true;
            this.labelQuadtreeDepth.Location = new System.Drawing.Point(173, 31);
            this.labelQuadtreeDepth.Name = "labelQuadtreeDepth";
            this.labelQuadtreeDepth.Size = new System.Drawing.Size(86, 13);
            this.labelQuadtreeDepth.TabIndex = 8;
            this.labelQuadtreeDepth.Text = "Quadtree Depth:";
            // 
            // FormNewInterior
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 176);
            this.Controls.Add(this.numQuadtreeDepth);
            this.Controls.Add(this.labelQuadtreeDepth);
            this.Controls.Add(this.numCellDepth);
            this.Controls.Add(this.numCellHeight);
            this.Controls.Add(this.numCellWidth);
            this.Controls.Add(this.labelCellDepth);
            this.Controls.Add(this.labelCellHeight);
            this.Controls.Add(this.labelCellWidth);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormNewInterior";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Interior";
            ((System.ComponentModel.ISupportInitialize)(this.numCellWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCellHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCellDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuadtreeDepth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelCellWidth;
        private System.Windows.Forms.Label labelCellHeight;
        private System.Windows.Forms.Label labelCellDepth;
        private System.Windows.Forms.NumericUpDown numCellWidth;
        private System.Windows.Forms.NumericUpDown numCellHeight;
        private System.Windows.Forms.NumericUpDown numCellDepth;
        private System.Windows.Forms.NumericUpDown numQuadtreeDepth;
        private System.Windows.Forms.Label labelQuadtreeDepth;
    }
}