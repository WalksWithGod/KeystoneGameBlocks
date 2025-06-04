namespace KeyPlugins
{
    partial class VectorEditCard
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
        	this.labelX = new DevComponents.DotNetBar.LabelX();
        	this.labelY = new DevComponents.DotNetBar.LabelX();
        	this.labelZ = new DevComponents.DotNetBar.LabelX();
        	this.cbSnap = new DevComponents.DotNetBar.Controls.CheckBoxX();
        	this.labelX6 = new DevComponents.DotNetBar.LabelX();
        	this.numX = new IgNS.Controls.IgSpinEdit();
        	this.numY = new IgNS.Controls.IgSpinEdit();
        	this.numZ = new IgNS.Controls.IgSpinEdit();
        	this.igSpinEdit3 = new IgNS.Controls.IgSpinEdit();
        	this.groupPanel.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.numZ)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.igSpinEdit3)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// groupPanel
        	// 
        	this.groupPanel.Controls.Add(this.igSpinEdit3);
        	this.groupPanel.Controls.Add(this.numZ);
        	this.groupPanel.Controls.Add(this.numY);
        	this.groupPanel.Controls.Add(this.numX);
        	this.groupPanel.Controls.Add(this.labelX6);
        	this.groupPanel.Controls.Add(this.cbSnap);
        	this.groupPanel.Controls.Add(this.labelZ);
        	this.groupPanel.Controls.Add(this.labelY);
        	this.groupPanel.Controls.Add(this.labelX);
        	this.groupPanel.Size = new System.Drawing.Size(281, 262);
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
        	// 
        	// buttonClose
        	// 
        	this.buttonClose.FlatAppearance.BorderSize = 0;
        	this.buttonClose.Size = new System.Drawing.Size(24, 23);
        	// 
        	// buttonBrowse
        	// 
        	this.buttonBrowse.FlatAppearance.BorderSize = 0;
        	// 
        	// labelX
        	// 
        	// 
        	// 
        	// 
        	this.labelX.BackgroundStyle.Class = "";
        	this.labelX.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelX.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelX.Location = new System.Drawing.Point(3, 44);
        	this.labelX.Name = "labelX";
        	this.labelX.Size = new System.Drawing.Size(23, 19);
        	this.labelX.TabIndex = 1;
        	this.labelX.Text = "X ";
        	// 
        	// labelY
        	// 
        	// 
        	// 
        	// 
        	this.labelY.BackgroundStyle.Class = "";
        	this.labelY.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelY.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelY.Location = new System.Drawing.Point(4, 89);
        	this.labelY.Name = "labelY";
        	this.labelY.Size = new System.Drawing.Size(23, 19);
        	this.labelY.TabIndex = 3;
        	this.labelY.Text = "Y";
        	// 
        	// labelZ
        	// 
        	// 
        	// 
        	// 
        	this.labelZ.BackgroundStyle.Class = "";
        	this.labelZ.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelZ.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelZ.Location = new System.Drawing.Point(4, 138);
        	this.labelZ.Name = "labelZ";
        	this.labelZ.Size = new System.Drawing.Size(23, 19);
        	this.labelZ.TabIndex = 5;
        	this.labelZ.Text = "Z";
        	// 
        	// cbSnap
        	// 
        	// 
        	// 
        	// 
        	this.cbSnap.BackgroundStyle.Class = "";
        	this.cbSnap.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.cbSnap.Location = new System.Drawing.Point(4, 177);
        	this.cbSnap.Name = "cbSnap";
        	this.cbSnap.Size = new System.Drawing.Size(100, 22);
        	this.cbSnap.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
        	this.cbSnap.TabIndex = 6;
        	this.cbSnap.Text = "Snap to Grid";
        	this.cbSnap.CheckedChanged += new System.EventHandler(this.cbSnap_CheckedChanged);
        	// 
        	// labelX6
        	// 
        	// 
        	// 
        	// 
        	this.labelX6.BackgroundStyle.Class = "";
        	this.labelX6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.labelX6.Location = new System.Drawing.Point(110, 180);
        	this.labelX6.Name = "labelX6";
        	this.labelX6.Size = new System.Drawing.Size(70, 19);
        	this.labelX6.TabIndex = 10;
        	this.labelX6.Text = "Grid Spacing";
        	// 
        	// numX
        	// 
        	this.numX.ExternalUpdate = false;
        	this.numX.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.numX.FormatString = "{0}";
        	this.numX.Increament = 1D;
        	this.numX.IncrementMax = 1000000000D;
        	this.numX.IncrementMin = 0.001D;
        	this.numX.IncrementVisible = true;
        	this.numX.Location = new System.Drawing.Point(30, 29);
        	this.numX.Margin = new System.Windows.Forms.Padding(0);
        	this.numX.Name = "numX";
        	this.numX.Padding = new System.Windows.Forms.Padding(2);
        	this.numX.Pow2Increment = false;
        	this.numX.Size = new System.Drawing.Size(227, 35);
        	this.numX.TabIndex = 12;
        	this.numX.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
        	this.numX.Value = 0D;
        	this.numX.ValueAsHex = false;
        	this.numX.ValueAsInt = false;
        	this.numX.ValueBackColor = System.Drawing.SystemColors.Window;
        	this.numX.ValueMax = 1.7976931348623157E+308D;
        	this.numX.ValueMin = -1.7976931348623157E+308D;
        	this.numX.ValueChanged += new System.EventHandler(this.numX_ValueChanged);
        	// 
        	// numY
        	// 
        	this.numY.ExternalUpdate = false;
        	this.numY.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.numY.FormatString = "{0}";
        	this.numY.Increament = 1D;
        	this.numY.IncrementMax = 1000000000D;
        	this.numY.IncrementMin = 0.001D;
        	this.numY.IncrementVisible = true;
        	this.numY.Location = new System.Drawing.Point(30, 73);
        	this.numY.Margin = new System.Windows.Forms.Padding(0);
        	this.numY.Name = "numY";
        	this.numY.Padding = new System.Windows.Forms.Padding(2);
        	this.numY.Pow2Increment = false;
        	this.numY.Size = new System.Drawing.Size(227, 35);
        	this.numY.TabIndex = 13;
        	this.numY.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
        	this.numY.Value = 0D;
        	this.numY.ValueAsHex = false;
        	this.numY.ValueAsInt = false;
        	this.numY.ValueBackColor = System.Drawing.SystemColors.Window;
        	this.numY.ValueMax = 1.7976931348623157E+308D;
        	this.numY.ValueMin = -1.7976931348623157E+308D;
        	this.numY.ValueChanged += new System.EventHandler(this.numY_ValueChanged);
        	// 
        	// numZ
        	// 
        	this.numZ.ExternalUpdate = false;
        	this.numZ.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.numZ.FormatString = "{0}";
        	this.numZ.Increament = 1D;
        	this.numZ.IncrementMax = 1000000000D;
        	this.numZ.IncrementMin = 0.001D;
        	this.numZ.IncrementVisible = true;
        	this.numZ.Location = new System.Drawing.Point(30, 122);
        	this.numZ.Margin = new System.Windows.Forms.Padding(0);
        	this.numZ.Name = "numZ";
        	this.numZ.Padding = new System.Windows.Forms.Padding(2);
        	this.numZ.Pow2Increment = false;
        	this.numZ.Size = new System.Drawing.Size(227, 35);
        	this.numZ.TabIndex = 14;
        	this.numZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
        	this.numZ.Value = 0D;
        	this.numZ.ValueAsHex = false;
        	this.numZ.ValueAsInt = false;
        	this.numZ.ValueBackColor = System.Drawing.SystemColors.Window;
        	this.numZ.ValueMax = 1.7976931348623157E+308D;
        	this.numZ.ValueMin = -1.7976931348623157E+308D;
        	this.numZ.ValueChanged += new System.EventHandler(this.numZ_ValueChanged);
        	// 
        	// igSpinEdit3
        	// 
        	this.igSpinEdit3.ExternalUpdate = false;
        	this.igSpinEdit3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.igSpinEdit3.FormatString = "{0}";
        	this.igSpinEdit3.Increament = 1D;
        	this.igSpinEdit3.IncrementMax = 1000000000D;
        	this.igSpinEdit3.IncrementMin = 0.001D;
        	this.igSpinEdit3.IncrementVisible = true;
        	this.igSpinEdit3.Location = new System.Drawing.Point(183, 169);
        	this.igSpinEdit3.Margin = new System.Windows.Forms.Padding(0);
        	this.igSpinEdit3.Name = "igSpinEdit3";
        	this.igSpinEdit3.Padding = new System.Windows.Forms.Padding(2);
        	this.igSpinEdit3.Pow2Increment = false;
        	this.igSpinEdit3.Size = new System.Drawing.Size(74, 35);
        	this.igSpinEdit3.TabIndex = 15;
        	this.igSpinEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
        	this.igSpinEdit3.Value = 0D;
        	this.igSpinEdit3.ValueAsHex = false;
        	this.igSpinEdit3.ValueAsInt = false;
        	this.igSpinEdit3.ValueBackColor = System.Drawing.SystemColors.Window;
        	this.igSpinEdit3.ValueMax = 1.7976931348623157E+308D;
        	this.igSpinEdit3.ValueMin = -1.7976931348623157E+308D;
        	// 
        	// VectorEditCard
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Name = "VectorEditCard";
        	this.Size = new System.Drawing.Size(281, 262);
        	this.groupPanel.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.numZ)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.igSpinEdit3)).EndInit();
        	this.ResumeLayout(false);
        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelZ;
        private DevComponents.DotNetBar.LabelX labelY;
        private DevComponents.DotNetBar.LabelX labelX;
        private DevComponents.DotNetBar.Controls.CheckBoxX cbSnap;
        private DevComponents.DotNetBar.LabelX labelX6;
        private IgNS.Controls.IgSpinEdit numX;
        private IgNS.Controls.IgSpinEdit numZ;
        private IgNS.Controls.IgSpinEdit numY;
        private IgNS.Controls.IgSpinEdit igSpinEdit3;
    }
}
