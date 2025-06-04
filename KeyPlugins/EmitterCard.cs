using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyPlugins
{
    public partial class EmitterCard : NotecardPropGrid
    {
        public int mEmitterIndex;

        public EmitterCard() : base()
        {
        }
        public EmitterCard(string nodeID, int emitterIndex) : base (nodeID)
        {
            mEmitterIndex = emitterIndex;

            
            Settings.PropertySpec[] properties = null;
            this.SetGridProperties(properties);
        }

        private void InitializeComponent()
        {
            this.groupPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Size = new System.Drawing.Size(225, 259);
            // 
            // groupPanel
            // 
            this.groupPanel.Size = new System.Drawing.Size(231, 280);
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
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.FlatAppearance.BorderSize = 0;
            // 
            // EmitterCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "EmitterCard";
            this.Size = new System.Drawing.Size(231, 280);
            this.groupPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
