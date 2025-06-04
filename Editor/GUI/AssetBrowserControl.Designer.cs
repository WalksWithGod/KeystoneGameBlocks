namespace KeyEdit.GUI
{
    partial class AssetBrowserControl
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
        	this.styleManager1 = new DevComponents.DotNetBar.StyleManager(this.components);
        	this.fileTree = new FileExplorerTreeDotnet.FileSystemTreeView();
        	this.mImageBrowser = new ImageBrowserDotnet.ImageBrowser();
        	this.ribbonBar = new DevComponents.DotNetBar.RibbonBar();
        	this.buttonModDBSelect = new DevComponents.DotNetBar.ButtonItem();
        	this.buttonCreateNewModDB = new DevComponents.DotNetBar.ButtonItem();
        	this.expandableSplitter1 = new DevComponents.DotNetBar.ExpandableSplitter();
        	this.panelEx1 = new DevComponents.DotNetBar.PanelEx();
        	this.panelEx1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// styleManager1
        	// 
        	this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2010Blue;
        	this.styleManager1.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154))))));
        	// 
        	// fileTree
        	// 
        	this.fileTree.AllowDrop = true;
        	this.fileTree.Dock = System.Windows.Forms.DockStyle.Left;
        	this.fileTree.ImageIndex = 0;
        	this.fileTree.Location = new System.Drawing.Point(0, 26);
        	this.fileTree.Name = "fileTree";
        	this.fileTree.SelectedImageIndex = 0;
        	this.fileTree.ShowFiles = true;
        	this.fileTree.Size = new System.Drawing.Size(102, 560);
        	this.fileTree.TabIndex = 1;
        	this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_AfterSelect);
        	this.fileTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.fileTree_DragDrop);
        	this.fileTree.DragEnter += new System.Windows.Forms.DragEventHandler(this.fileTree_DragEnter);
        	this.fileTree.DragOver += new System.Windows.Forms.DragEventHandler(this.fileTree_DragOver);
        	this.fileTree.DragLeave += new System.EventHandler(this.fileTree_DragLeave);
        	// 
        	// mImageBrowser
        	// 
        	this.mImageBrowser.AllowDrop = true;
        	this.mImageBrowser.BackColor = System.Drawing.SystemColors.Control;
        	this.mImageBrowser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        	this.mImageBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.mImageBrowser.ImageSize = 64;
        	this.mImageBrowser.ImageViewerEnabled = true;
        	this.mImageBrowser.Location = new System.Drawing.Point(0, 0);
        	this.mImageBrowser.Name = "mImageBrowser";
        	this.mImageBrowser.RecurseSubFolders = false;
        	this.mImageBrowser.Size = new System.Drawing.Size(189, 560);
        	this.mImageBrowser.TabIndex = 0;
        	this.mImageBrowser.Load += new System.EventHandler(this.MImageBrowserLoad);
        	this.mImageBrowser.DragDrop += new System.Windows.Forms.DragEventHandler(this.mImageBrowser_DragDrop);
        	this.mImageBrowser.DragEnter += new System.Windows.Forms.DragEventHandler(this.mImageBrowser_DragEnter);
        	this.mImageBrowser.DragOver += new System.Windows.Forms.DragEventHandler(this.mImageBrowser_DragOver);
        	this.mImageBrowser.DragLeave += new System.EventHandler(this.mImageBrowser_DragLeave);
        	// 
        	// ribbonBar
        	// 
        	this.ribbonBar.AutoOverflowEnabled = true;
        	this.ribbonBar.AutoScroll = true;
        	// 
        	// 
        	// 
        	this.ribbonBar.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	// 
        	// 
        	// 
        	this.ribbonBar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.ribbonBar.ContainerControlProcessDialogKey = true;
        	this.ribbonBar.Dock = System.Windows.Forms.DockStyle.Top;
        	this.ribbonBar.DragDropSupport = true;
        	this.ribbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
			this.buttonModDBSelect});
        	this.ribbonBar.Location = new System.Drawing.Point(0, 0);
        	this.ribbonBar.Name = "ribbonBar";
        	this.ribbonBar.Size = new System.Drawing.Size(301, 26);
        	this.ribbonBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
        	this.ribbonBar.TabIndex = 9;
        	// 
        	// 
        	// 
        	this.ribbonBar.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.ribbonBar.TitleStyle.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
        	// 
        	// 
        	// 
        	this.ribbonBar.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
        	this.ribbonBar.TitleVisible = false;
        	// 
        	// buttonModDBSelect
        	// 
        	this.buttonModDBSelect.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
        	this.buttonModDBSelect.Image = global::KeyEdit.Properties.Resources.db_icon_16;
        	this.buttonModDBSelect.Name = "buttonModDBSelect";
        	this.buttonModDBSelect.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
			this.buttonCreateNewModDB});
        	this.buttonModDBSelect.SubItemsExpandWidth = 14;
        	// 
        	// buttonCreateNewModDB
        	// 
        	this.buttonCreateNewModDB.Name = "buttonCreateNewModDB";
        	this.buttonCreateNewModDB.Text = "Create New Mod";
        	// 
        	// expandableSplitter1
        	// 
        	this.expandableSplitter1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(108)))), ((int)(((byte)(122)))));
        	this.expandableSplitter1.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.expandableSplitter1.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
        	this.expandableSplitter1.ExpandableControl = this.fileTree;
        	this.expandableSplitter1.ExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(108)))), ((int)(((byte)(122)))));
        	this.expandableSplitter1.ExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.expandableSplitter1.ExpandLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(57)))), ((int)(((byte)(120)))));
        	this.expandableSplitter1.ExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
        	this.expandableSplitter1.GripDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(57)))), ((int)(((byte)(120)))));
        	this.expandableSplitter1.GripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
        	this.expandableSplitter1.GripLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(232)))), ((int)(((byte)(246)))));
        	this.expandableSplitter1.GripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
        	this.expandableSplitter1.HotBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(200)))), ((int)(((byte)(103)))));
        	this.expandableSplitter1.HotBackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(226)))), ((int)(((byte)(135)))));
        	this.expandableSplitter1.HotBackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground2;
        	this.expandableSplitter1.HotBackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground;
        	this.expandableSplitter1.HotExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(108)))), ((int)(((byte)(122)))));
        	this.expandableSplitter1.HotExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.expandableSplitter1.HotExpandLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(57)))), ((int)(((byte)(120)))));
        	this.expandableSplitter1.HotExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
        	this.expandableSplitter1.HotGripDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(108)))), ((int)(((byte)(122)))));
        	this.expandableSplitter1.HotGripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.expandableSplitter1.HotGripLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(232)))), ((int)(((byte)(246)))));
        	this.expandableSplitter1.HotGripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
        	this.expandableSplitter1.Location = new System.Drawing.Point(102, 26);
        	this.expandableSplitter1.Name = "expandableSplitter1";
        	this.expandableSplitter1.Size = new System.Drawing.Size(10, 560);
        	this.expandableSplitter1.Style = DevComponents.DotNetBar.eSplitterStyle.Office2007;
        	this.expandableSplitter1.TabIndex = 10;
        	this.expandableSplitter1.TabStop = false;
        	// 
        	// panelEx1
        	// 
        	this.panelEx1.CanvasColor = System.Drawing.SystemColors.Control;
        	this.panelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
        	this.panelEx1.Controls.Add(this.mImageBrowser);
        	this.panelEx1.DisabledBackColor = System.Drawing.Color.Empty;
        	this.panelEx1.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.panelEx1.Location = new System.Drawing.Point(112, 26);
        	this.panelEx1.Name = "panelEx1";
        	this.panelEx1.Size = new System.Drawing.Size(189, 560);
        	this.panelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
        	this.panelEx1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
        	this.panelEx1.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
        	this.panelEx1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
        	this.panelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
        	this.panelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
        	this.panelEx1.Style.GradientAngle = 90;
        	this.panelEx1.TabIndex = 11;
        	this.panelEx1.Text = "panelEx1";
        	// 
        	// AssetBrowserControl
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.Controls.Add(this.panelEx1);
        	this.Controls.Add(this.expandableSplitter1);
        	this.Controls.Add(this.fileTree);
        	this.Controls.Add(this.ribbonBar);
        	this.Name = "AssetBrowserControl";
        	this.Size = new System.Drawing.Size(301, 586);
        	this.panelEx1.ResumeLayout(false);
        	this.ResumeLayout(false);

        }

        #endregion
        private DevComponents.DotNetBar.MenuPanel menuMODDBSelect;
        private DevComponents.DotNetBar.StyleManager styleManager1;
        private FileExplorerTreeDotnet.FileSystemTreeView fileTree;
        private ImageBrowserDotnet.ImageBrowser mImageBrowser;
        private DevComponents.DotNetBar.RibbonBar ribbonBar;
        private DevComponents.DotNetBar.ExpandableSplitter expandableSplitter1;
        private DevComponents.DotNetBar.PanelEx panelEx1;
        private DevComponents.DotNetBar.ButtonItem buttonModDBSelect;
        private DevComponents.DotNetBar.ButtonItem buttonCreateNewModDB;
    }
}
