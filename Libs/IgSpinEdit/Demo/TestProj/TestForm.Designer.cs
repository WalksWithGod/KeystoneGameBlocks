namespace IgSpinEditTest
{
    partial class TestForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.propGrid = new System.Windows.Forms.PropertyGrid();
            this.pnl = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.grbxExtUpdate = new System.Windows.Forms.GroupBox();
            this.chbxCancel = new System.Windows.Forms.CheckBox();
            this.btnExternUpdate = new System.Windows.Forms.Button();
            this.edNewVal = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.spedPow2 = new IgNS.Controls.IgSpinEdit();
            this.spedAsHex = new IgNS.Controls.IgSpinEdit();
            this.spedAsInt = new IgNS.Controls.IgSpinEdit();
            this.spedWide = new IgNS.Controls.IgSpinEdit();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnl.SuspendLayout();
            this.grbxExtUpdate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spedPow2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedAsHex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedAsInt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedWide)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.propGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnl, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(550, 296);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // propGrid
            // 
            this.propGrid.BackColor = System.Drawing.SystemColors.Info;
            this.propGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propGrid.HelpBackColor = System.Drawing.SystemColors.Info;
            this.propGrid.Location = new System.Drawing.Point(203, 3);
            this.propGrid.Name = "propGrid";
            this.propGrid.Size = new System.Drawing.Size(344, 290);
            this.propGrid.TabIndex = 26;
            this.propGrid.Enter += new System.EventHandler(this.propGrid_Enter);
            this.propGrid.Leave += new System.EventHandler(this.propGrid_Leave);
            // 
            // pnl
            // 
            this.pnl.BackColor = System.Drawing.Color.Transparent;
            this.pnl.Controls.Add(this.label4);
            this.pnl.Controls.Add(this.spedPow2);
            this.pnl.Controls.Add(this.label3);
            this.pnl.Controls.Add(this.spedAsHex);
            this.pnl.Controls.Add(this.label2);
            this.pnl.Controls.Add(this.spedAsInt);
            this.pnl.Controls.Add(this.label1);
            this.pnl.Controls.Add(this.grbxExtUpdate);
            this.pnl.Controls.Add(this.spedWide);
            this.pnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl.Location = new System.Drawing.Point(3, 3);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(194, 290);
            this.pnl.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(73, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Power2 Value";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(40, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 27;
            this.label3.Text = "Value as Hex";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(40, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Value as Int";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(8, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "Wide range Value";
            // 
            // grbxExtUpdate
            // 
            this.grbxExtUpdate.BackColor = System.Drawing.Color.Transparent;
            this.grbxExtUpdate.Controls.Add(this.chbxCancel);
            this.grbxExtUpdate.Controls.Add(this.btnExternUpdate);
            this.grbxExtUpdate.Controls.Add(this.edNewVal);
            this.grbxExtUpdate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grbxExtUpdate.Location = new System.Drawing.Point(0, 223);
            this.grbxExtUpdate.Name = "grbxExtUpdate";
            this.grbxExtUpdate.Size = new System.Drawing.Size(194, 67);
            this.grbxExtUpdate.TabIndex = 22;
            this.grbxExtUpdate.TabStop = false;
            this.grbxExtUpdate.Text = "External Update";
            // 
            // chbxCancel
            // 
            this.chbxCancel.AutoSize = true;
            this.chbxCancel.Location = new System.Drawing.Point(6, 45);
            this.chbxCancel.Name = "chbxCancel";
            this.chbxCancel.Size = new System.Drawing.Size(128, 17);
            this.chbxCancel.TabIndex = 7;
            this.chbxCancel.Text = "Cancel Value change";
            this.chbxCancel.UseVisualStyleBackColor = true;
            // 
            // btnExternUpdate
            // 
            this.btnExternUpdate.Location = new System.Drawing.Point(123, 19);
            this.btnExternUpdate.Margin = new System.Windows.Forms.Padding(1);
            this.btnExternUpdate.Name = "btnExternUpdate";
            this.btnExternUpdate.Size = new System.Drawing.Size(61, 20);
            this.btnExternUpdate.TabIndex = 6;
            this.btnExternUpdate.Text = "Update";
            this.toolTip1.SetToolTip(this.btnExternUpdate, "If ExternalUpdate is set for current SpinEdit use button to complete external upd" +
                    "ate.");
            this.btnExternUpdate.UseVisualStyleBackColor = true;
            this.btnExternUpdate.Click += new System.EventHandler(this.btnExternUpdate_Click);
            // 
            // edNewVal
            // 
            this.edNewVal.BackColor = System.Drawing.SystemColors.Window;
            this.edNewVal.Location = new System.Drawing.Point(6, 19);
            this.edNewVal.Name = "edNewVal";
            this.edNewVal.ReadOnly = true;
            this.edNewVal.Size = new System.Drawing.Size(113, 20);
            this.edNewVal.TabIndex = 5;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 300;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // spedPow2
            // 
            this.spedPow2.AutoSize = true;
            this.spedPow2.BackColor = System.Drawing.Color.Transparent;
            this.spedPow2.ExternalUpdate = false;
            this.spedPow2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.spedPow2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.spedPow2.FormatString = "{0}";
            this.spedPow2.Increament = 1;
            this.spedPow2.IncrementMax = 1;
            this.spedPow2.IncrementMin = 1;
            this.spedPow2.IncrementVisible = false;
            this.spedPow2.Location = new System.Drawing.Point(76, 176);
            this.spedPow2.Margin = new System.Windows.Forms.Padding(0);
            this.spedPow2.Name = "spedPow2";
            this.spedPow2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spedPow2.Pow2Increment = true;
            this.spedPow2.Size = new System.Drawing.Size(90, 26);
            this.spedPow2.TabIndex = 4;
            this.spedPow2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.spedPow2.Value = 32;
            this.spedPow2.ValueAsHex = true;
            this.spedPow2.ValueAsInt = true;
            this.spedPow2.ValueBackColor = System.Drawing.Color.LightCyan;
            this.spedPow2.ValueMax = 65536;
            this.spedPow2.ValueMin = 1;
            this.spedPow2.Click += new System.EventHandler(this.spedWide_Click);
            this.spedPow2.ValueChanging += new IgNS.Controls.IgSpinEditChanged(this.spedWide_ValueChanging);
            // 
            // spedAsHex
            // 
            this.spedAsHex.AutoSize = true;
            this.spedAsHex.BackColor = System.Drawing.Color.Transparent;
            this.spedAsHex.ExternalUpdate = false;
            this.spedAsHex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.spedAsHex.ForeColor = System.Drawing.SystemColors.ControlText;
            this.spedAsHex.FormatString = "0x{0:X8}";
            this.spedAsHex.Increament = 1;
            this.spedAsHex.IncrementMax = 268435456;
            this.spedAsHex.IncrementMin = 1;
            this.spedAsHex.IncrementVisible = true;
            this.spedAsHex.Location = new System.Drawing.Point(43, 123);
            this.spedAsHex.Margin = new System.Windows.Forms.Padding(0);
            this.spedAsHex.Name = "spedAsHex";
            this.spedAsHex.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spedAsHex.Pow2Increment = false;
            this.spedAsHex.Size = new System.Drawing.Size(141, 26);
            this.spedAsHex.TabIndex = 2;
            this.spedAsHex.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.spedAsHex.Value = 123456789;
            this.spedAsHex.ValueAsHex = true;
            this.spedAsHex.ValueAsInt = true;
            this.spedAsHex.ValueBackColor = System.Drawing.SystemColors.Info;
            this.spedAsHex.ValueMax = 4294967296;
            this.spedAsHex.ValueMin = 0;
            this.spedAsHex.Click += new System.EventHandler(this.spedWide_Click);
            this.spedAsHex.ValueChanging += new IgNS.Controls.IgSpinEditChanged(this.spedWide_ValueChanging);
            // 
            // spedAsInt
            // 
            this.spedAsInt.AutoSize = true;
            this.spedAsInt.BackColor = System.Drawing.Color.Transparent;
            this.spedAsInt.ExternalUpdate = true;
            this.spedAsInt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.spedAsInt.ForeColor = System.Drawing.SystemColors.ControlText;
            this.spedAsInt.FormatString = "Score: {0}";
            this.spedAsInt.Increament = 1;
            this.spedAsInt.IncrementMax = 100000;
            this.spedAsInt.IncrementMin = 1;
            this.spedAsInt.IncrementVisible = true;
            this.spedAsInt.Location = new System.Drawing.Point(43, 70);
            this.spedAsInt.Margin = new System.Windows.Forms.Padding(0);
            this.spedAsInt.Name = "spedAsInt";
            this.spedAsInt.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spedAsInt.Pow2Increment = false;
            this.spedAsInt.Size = new System.Drawing.Size(141, 26);
            this.spedAsInt.TabIndex = 1;
            this.spedAsInt.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.toolTip1.SetToolTip(this.spedAsInt, "!!! Set by default to ExternalUpdate");
            this.spedAsInt.Value = 987;
            this.spedAsInt.ValueAsHex = false;
            this.spedAsInt.ValueAsInt = true;
            this.spedAsInt.ValueBackColor = System.Drawing.Color.PapayaWhip;
            this.spedAsInt.ValueMax = 1000000;
            this.spedAsInt.ValueMin = 0;
            this.spedAsInt.Click += new System.EventHandler(this.spedWide_Click);
            this.spedAsInt.ValueChanging += new IgNS.Controls.IgSpinEditChanged(this.spedWide_ValueChanging);
            // 
            // spedWide
            // 
            this.spedWide.AutoSize = true;
            this.spedWide.BackColor = System.Drawing.Color.Transparent;
            this.spedWide.ExternalUpdate = false;
            this.spedWide.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.spedWide.ForeColor = System.Drawing.SystemColors.ControlText;
            this.spedWide.FormatString = "{0:0.000000} MHz";
            this.spedWide.Increament = 1;
            this.spedWide.IncrementMax = 1000;
            this.spedWide.IncrementMin = 1E-06;
            this.spedWide.IncrementVisible = true;
            this.spedWide.Location = new System.Drawing.Point(6, 17);
            this.spedWide.Margin = new System.Windows.Forms.Padding(0);
            this.spedWide.Name = "spedWide";
            this.spedWide.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spedWide.Pow2Increment = false;
            this.spedWide.Size = new System.Drawing.Size(178, 26);
            this.spedWide.TabIndex = 0;
            this.spedWide.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.spedWide.Value = 4000.9999;
            this.spedWide.ValueAsHex = false;
            this.spedWide.ValueAsInt = false;
            this.spedWide.ValueBackColor = System.Drawing.SystemColors.Window;
            this.spedWide.ValueMax = 10000;
            this.spedWide.ValueMin = 0;
            this.spedWide.Click += new System.EventHandler(this.spedWide_Click);
            this.spedWide.ValueChanging += new IgNS.Controls.IgSpinEditChanged(this.spedWide_ValueChanging);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(550, 296);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestForm";
            this.Text = " Test Form (Click on SpinEdit control and play with properties)";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.TestForm_Paint);
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnl.ResumeLayout(false);
            this.pnl.PerformLayout();
            this.grbxExtUpdate.ResumeLayout(false);
            this.grbxExtUpdate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spedPow2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedAsHex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedAsInt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spedWide)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnl;
        private IgNS.Controls.IgSpinEdit spedWide;
        private System.Windows.Forms.PropertyGrid propGrid;
        private System.Windows.Forms.GroupBox grbxExtUpdate;
        private System.Windows.Forms.CheckBox chbxCancel;
        private System.Windows.Forms.Button btnExternUpdate;
        private System.Windows.Forms.TextBox edNewVal;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private IgNS.Controls.IgSpinEdit spedPow2;
        private System.Windows.Forms.Label label3;
        private IgNS.Controls.IgSpinEdit spedAsHex;
        private System.Windows.Forms.Label label2;
        private IgNS.Controls.IgSpinEdit spedAsInt;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer tmrRefresh;
    }
}