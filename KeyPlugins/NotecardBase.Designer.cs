namespace KeyPlugins
{
    partial class NotecardBase
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotecardBase));
        	this.groupPanel = new DevComponents.DotNetBar.Controls.GroupPanel();
        	this.buttonClose = new System.Windows.Forms.Button();
        	this.buttonBrowse = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// groupPanel
        	// 
        	this.groupPanel.CanvasColor = System.Drawing.SystemColors.Control;
        	this.groupPanel.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
        	this.groupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.groupPanel.Location = new System.Drawing.Point(0, 0);
        	this.groupPanel.Name = "groupPanel";
        	this.groupPanel.Size = new System.Drawing.Size(231, 334);
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
        	this.groupPanel.TabIndex = 2;
        	this.groupPanel.Text = "groupPanel4";
        	// 
        	// buttonClose
        	// 
        	this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonClose.BackgroundImage")));
        	this.buttonClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.buttonClose.FlatAppearance.BorderSize = 0;
        	this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonClose.Location = new System.Drawing.Point(209, 0);
        	this.buttonClose.Name = "buttonClose";
        	this.buttonClose.Size = new System.Drawing.Size(12, 12);
        	this.buttonClose.TabIndex = 1;
        	this.buttonClose.UseVisualStyleBackColor = true;
        	this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
        	// 
        	// buttonBrowse
        	// 
        	this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonBrowse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonBrowse.BackgroundImage")));
        	this.buttonBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.buttonBrowse.FlatAppearance.BorderSize = 0;
        	this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonBrowse.Location = new System.Drawing.Point(185, 0);
        	this.buttonBrowse.Name = "buttonBrowse";
        	this.buttonBrowse.Size = new System.Drawing.Size(18, 12);
        	this.buttonBrowse.TabIndex = 4;
        	this.buttonBrowse.UseVisualStyleBackColor = true;
        	this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowseClick);
        	// 
        	// NotecardBase
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.Controls.Add(this.buttonBrowse);
        	this.Controls.Add(this.buttonClose);
        	this.Controls.Add(this.groupPanel);
        	this.Name = "NotecardBase";
        	this.Size = new System.Drawing.Size(231, 334);
        	this.ResumeLayout(false);
        }
        #endregion

        protected DevComponents.DotNetBar.Controls.GroupPanel groupPanel;
        protected System.Windows.Forms.Button buttonClose;
        protected System.Windows.Forms.Button buttonBrowse; 
    }
}
