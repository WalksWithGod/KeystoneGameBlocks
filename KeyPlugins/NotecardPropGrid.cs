using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeyPlugins
{
    /// <summary>
    /// Currently used by ShaderEditCard, RigidBodyEditCard and EmitterCard
    /// </summary>
    public class NotecardPropGrid : NotecardBase
    {
        public PropertyGrid propertyGrid;

        //public EventHandler Settings.PropertySpec PropertyChanged (Settings.PropertySpec property);
        public delegate void PropertyValueChanged(string nodeID, Settings.PropertySpec spec);
        protected System.Collections.Hashtable mPropertyTable;
        public EventHandler OnResourceChanged;
        public PropertyValueChanged OnPropertyValueChanged;
        public string mNodeID;

        public NotecardPropGrid()
        {
            InitializeComponent();
        }
        protected NotecardPropGrid(string nodeID)
        {
            mNodeID = nodeID;
            InitializeComponent();
        }

        public void SetGridProperties(Settings.PropertySpec[] parameters)
        {
            // clear the property grid
            propertyGrid.SelectedObject = null;

            if (parameters == null || parameters.Length == 0) return;

            mPropertyTable = new System.Collections.Hashtable();
            Settings.PropertyBag bag = new Settings.PropertyBag();
            bag.GetValue += GetPropertyValue;
            bag.SetValue += SetPropertyValue;

            // TODO: we must define TypeEditors if we want to be able to edit things
            // like Vector3d and Matrix

            for (int i = 0; i < parameters.Length; i++)
            {
                mPropertyTable[parameters[i].Name] = parameters[i].DefaultValue;
                // https://learn.microsoft.com/en-us/dotnet/api/system.drawing.design.uitypeeditor?view=windowsdesktop-8.0
                if (parameters[i].Name.Contains("color"))
                    // HACK - checking for string.Contains("Color") is a toal hack
                    parameters[i].EditorTypeName = typeof(ColorTypeEditor).AssemblyQualifiedName;
            }
            bag.Properties.AddRange(parameters);
            
            propertyGrid.SelectedObject = bag;
        }

        #region Grid GUI Editing methods
        private void GetPropertyValue(object sender, Settings.PropertySpecEventArgs e)
        {
            e.Value = mPropertyTable[e.Property.Name];
        }

        /// <summary>
        /// Called when user changes the property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SetPropertyValue(object sender, Settings.PropertySpecEventArgs e)
        {
            mPropertyTable[e.Property.Name] = e.Value;
            e.Property.DefaultValue = e.Value;
            OnPropertyValueChanged(mNodeID, e.Property);
            //PropertyChanged.Invoke(sender , e);
        }


        protected static void ExpandGroup(PropertyGrid propertyGrid, string groupName)
        {
            GridItem root = propertyGrid.SelectedGridItem;
            //Get the parent 
            while (root.Parent != null)
                root = root.Parent;

            if (root != null)
            {
                foreach (GridItem g in root.GridItems)
                {
                    if (g.GridItemType == GridItemType.Category && g.Label == groupName)
                    {
                        g.Expanded = true;
                        break;
                    }
                }
            }
        }

        protected void propgridParameters_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            //ExpandGroup(propgridParameters, e.NewSelection.Label);
            //propgridParameters.CollapseAllGridItems();
            e.NewSelection.Expanded = true;
        }
        #endregion

        private void InitializeComponent()
        {
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.groupPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupPanel
            // 
            this.groupPanel.Controls.Add(this.propertyGrid);
            this.groupPanel.Size = new System.Drawing.Size(231, 240);
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
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(225, 219);
            this.propertyGrid.TabIndex = 0;
            // 
            // NotecardPropGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "NotecardPropGrid";
            this.Size = new System.Drawing.Size(231, 240);
            this.groupPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
