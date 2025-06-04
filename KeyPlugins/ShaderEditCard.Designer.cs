namespace KeyPlugins
{
    partial class ShaderEditCard
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
            this.labelEntry = new DevComponents.DotNetBar.LabelX();
            this.labelMod = new DevComponents.DotNetBar.LabelX();
            this.propgridParameters = new System.Windows.Forms.PropertyGrid();
            this.groupPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupPanel
            // 
            this.groupPanel.Controls.Add(this.propgridParameters);
            this.groupPanel.Controls.Add(this.labelEntry);
            this.groupPanel.Controls.Add(this.labelMod);
            this.groupPanel.Size = new System.Drawing.Size(226, 268);
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
            this.groupPanel.Style.CornerDiameter = 4;
            this.groupPanel.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanel.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.groupPanel.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.groupPanel.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonClose
            // 
            this.buttonClose.FlatAppearance.BorderSize = 0;
            this.buttonClose.Location = new System.Drawing.Point(201, 0);
            this.buttonClose.Size = new System.Drawing.Size(25, 15);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.FlatAppearance.BorderSize = 0;
            this.buttonBrowse.Location = new System.Drawing.Point(183, 0);
            this.buttonBrowse.Size = new System.Drawing.Size(23, 15);
            // 
            // labelEntry
            // 
            // 
            // 
            // 
            this.labelEntry.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelEntry.Location = new System.Drawing.Point(8, 180);
            this.labelEntry.Name = "labelEntry";
            this.labelEntry.Size = new System.Drawing.Size(200, 18);
            this.labelEntry.TabIndex = 37;
            this.labelEntry.Text = "Entry:";
            this.labelEntry.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.labelEntry.WordWrap = true;
            // 
            // labelMod
            // 
            // 
            // 
            // 
            this.labelMod.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelMod.Location = new System.Drawing.Point(8, 204);
            this.labelMod.Name = "labelMod";
            this.labelMod.Size = new System.Drawing.Size(200, 18);
            this.labelMod.TabIndex = 36;
            this.labelMod.Text = "Archive:";
            this.labelMod.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.labelMod.WordWrap = true;
            // 
            // propgridParameters
            // 
            this.propgridParameters.HelpVisible = false;
            this.propgridParameters.Location = new System.Drawing.Point(8, 3);
            this.propgridParameters.Name = "propgridParameters";
            this.propgridParameters.Size = new System.Drawing.Size(200, 171);
            this.propgridParameters.TabIndex = 38;
            this.propgridParameters.ToolbarVisible = false;
            this.propgridParameters.SelectedGridItemChanged += propgridParameters_SelectedGridItemChanged;
            // 
            // ShaderEditCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ShaderEditCard";
            this.Size = new System.Drawing.Size(226, 268);
            this.groupPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelEntry;
        private DevComponents.DotNetBar.LabelX labelMod;
        private System.Windows.Forms.PropertyGrid propgridParameters;
    }
}
