namespace IgNS.Controls
{
    partial class IgSpinEdit
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
            this.components = new System.ComponentModel.Container();
            this.btnIncr = new System.Windows.Forms.NumericUpDown();
            this.btnVal = new System.Windows.Forms.NumericUpDown();
            this.edVal = new System.Windows.Forms.TextBox();
            this.hint = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.btnIncr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnVal)).BeginInit();
            this.SuspendLayout();
            // 
            // btnIncr
            // 
            this.btnIncr.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnIncr.Location = new System.Drawing.Point(80, 2);
            this.btnIncr.Margin = new System.Windows.Forms.Padding(1);
            this.btnIncr.Maximum = new decimal(new int[] {
            -1304428544,
            434162106,
            542,
            0});
            this.btnIncr.MaximumSize = new System.Drawing.Size(18, 0);
            this.btnIncr.Minimum = new decimal(new int[] {
            -1304428544,
            434162106,
            542,
            -2147483648});
            this.btnIncr.Name = "btnIncr";
            this.btnIncr.Size = new System.Drawing.Size(18, 20);
            this.btnIncr.TabIndex = 1;
            this.btnIncr.Enter += new System.EventHandler(this.btnIncr_Enter);
            this.btnIncr.ValueChanged += new System.EventHandler(this.btnIncr_ValueChanged);
            this.btnIncr.Leave += new System.EventHandler(this.btnVal_Leave);
            // 
            // btnVal
            // 
            this.btnVal.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnVal.ForeColor = System.Drawing.SystemColors.WindowText;
            this.btnVal.Location = new System.Drawing.Point(60, 2);
            this.btnVal.Margin = new System.Windows.Forms.Padding(1);
            this.btnVal.Maximum = new decimal(new int[] {
            -1304428544,
            434162106,
            542,
            0});
            this.btnVal.MaximumSize = new System.Drawing.Size(20, 0);
            this.btnVal.Minimum = new decimal(new int[] {
            -1304428544,
            434162106,
            542,
            -2147483648});
            this.btnVal.Name = "btnVal";
            this.btnVal.Size = new System.Drawing.Size(20, 20);
            this.btnVal.TabIndex = 0;
            this.hint.SetToolTip(this.btnVal, "Value Up/Down");
            this.btnVal.Enter += new System.EventHandler(this.btnVal_Enter);
            this.btnVal.ValueChanged += new System.EventHandler(this.btnVal_ValueChanged);
            this.btnVal.Leave += new System.EventHandler(this.btnVal_Leave);
            // 
            // edVal
            // 
            this.edVal.BackColor = System.Drawing.SystemColors.Window;
            this.edVal.Cursor = System.Windows.Forms.Cursors.Default;
            this.edVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.edVal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.edVal.Location = new System.Drawing.Point(2, 2);
            this.edVal.Margin = new System.Windows.Forms.Padding(1);
            this.edVal.Name = "edVal";
            this.edVal.ReadOnly = true;
            this.edVal.Size = new System.Drawing.Size(58, 20);
            this.edVal.TabIndex = 2;
            this.edVal.TabStop = false;
            this.edVal.Text = "0.0";
            this.edVal.Enter += new System.EventHandler(this.edVal_Enter);
            this.edVal.Click += new System.EventHandler(this.edVal_Click);
            this.edVal.SizeChanged += new System.EventHandler(this.edVal_SizeChanged);
            // 
            // hint
            // 
            this.hint.ShowAlways = true;
            // 
            // IgSpinEdit
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.edVal);
            this.Controls.Add(this.btnVal);
            this.Controls.Add(this.btnIncr);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "IgSpinEdit";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(100, 24);
            this.SizeChanged += new System.EventHandler(this.edVal_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.btnIncr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnVal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown btnIncr;
        private System.Windows.Forms.NumericUpDown btnVal;
        private System.Windows.Forms.TextBox edVal;
        private System.Windows.Forms.ToolTip hint;
    }
}
