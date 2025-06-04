using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;
using ImageBrowserDotnet;

namespace KeyEdit.GUI
{
    public partial class AssetBrowserControl : UserControl
    {
        public event AssetClickedEventHandler OnAssetClicked;
        public event EntryRenamedEventHandler OnEntryRenamed;  // occurs on Move also
        public event QueryCustomImageForNonImageEventHandler QueryCustomImageForNonImage; // for custom handling of non image types if you want a placeholder image used for non image file

        private bool mToolbarVisible = false;
        private bool mImagePreviewEnabled = false;
        private ImageDialog m_ImageDialog;
 
        private string mModPath = null;
        private string mModName = null;
        private string mSelectedModFolder = null;
        private string mSelectedFile = null;
        private string mSelectedPath;

        private string[] mAllowedExtensions;
        private bool mShowTreeView = true;

        private const string DB_SEARCH_PATTERN = "*.zip";

        private const int ALT = 32;
        private const int CTRL = 8;
        private const int SHIFT = 4;

        public AssetBrowserControl()
        {
            InitializeComponent();

            mSelectedPath = null;

            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
            mImageBrowser.OnBrowsingAborted += mImageBrowser_OnBrowsingAborted;
            mImageBrowser.OnBrowsingCanceled += mImageBrowser_OnBrowsingCanceled;
            mImageBrowser.OnBrowsingStarted += mImageBrowser_OnBrowsingStarted;
            mImageBrowser.OnBrowsingFoundImage += mImageBrowser_OnBrowsingFoundImage;
            mImageBrowser.OnBrowsingEnded += mImageBrowser_OnBrowsingEnded;
            mImageBrowser.OnBrowsingNonImageFileFound += mImageBrowser_OnBrowsingNonImageFileFound;

            mImageBrowser.OnImageMouseDown += mImageBrowser_OnImageMouseDown;
            mImageBrowser.OnImageMouseUp += mImageBrowser_OnImageMouseUp;
            mImageBrowser.OnImageMouseMove += mImageBrowser_OnImageMouseMove;
            mImageBrowser.OnImageDoubleClicked += mImageBrowser_OnImageDoubleClicked;
            mImageBrowser.OnImageClicked += mImageBrowser_OnImageClicked;

            fileTree.ShowFiles = false; // TODO: this line should be set from it's own property in this control
            fileTree.MouseDown += fileTree_OnMouseDown;
            fileTree.MouseMove += fileTree_OnMouseMove;
            fileTree.MouseUp += fileTree_OnMouseUp;
            fileTree.NodeMouseClick += fileTree_OnNodeClick;
            fileTree.NodeMouseDoubleClick += fileTree_OnNodeDoubleClick;
            fileTree.ItemDrag += fileTree_OnItemDrag;
            fileTree.BeforeLabelEdit += fileTree_OnBeforeLabelEdit;
            fileTree.AfterLabelEdit += fileTree_OnAfterLabelEdit;
            fileTree.LabelEdit = false; // we only allow label edit manually

            ImageSize = 64; // default
        }

        public void AddRibbonControl(DevComponents.DotNetBar.BaseItem control)
        {
            ribbonBar.Items.Add(control);
        }

        public DevComponents.DotNetBar.ButtonItem AddRibbonButton(string name, string text, string tooltip, string tag, int imageIndex, System.EventHandler clickHandler)
        {
            DevComponents.DotNetBar.ButtonItem button = new DevComponents.DotNetBar.ButtonItem ();
            button.Name = name;
            button.Text = text;
            button.Tag = tag;
            button.Tooltip = tooltip;
            button.ImageIndex = imageIndex;
            button.Click += clickHandler;
            ribbonBar.Items.Add(button);
            return button;
        }

        public void RemoveRibbonButton(DevComponents.DotNetBar.ButtonItem button, System.EventHandler clickHandler)
        {
            button.Tooltip = null;
            button.Click -= clickHandler;
            ribbonBar.Items.Remove(button);
        }

        public ImageList ImageList 
        {
            get { return this.ribbonBar.Images; }
            set { this.ribbonBar.Images = value; }
        }

        public bool ToolbarVisible
        {
            get { return mToolbarVisible; }
            set { mToolbarVisible = value; }
        }

        public string[] AllowedExtensions 
        { 
            get { return mAllowedExtensions; } set { mAllowedExtensions = value; } 
        }
        
        public bool ShowTreeView
        {
            get { return mShowTreeView; }
            set
            {
                mShowTreeView = value;
                fileTree.Visible = value;
                expandableSplitter1.Visible = value;
            }
        }


        public bool RecurseSubFolders 
        { 
            get { return mImageBrowser.RecurseSubFolders; } 
            set { mImageBrowser.RecurseSubFolders = value; } 
        }
        

        public int ImageSize 
        {
            get { return mImageBrowser.ImageSize; }
            set { mImageBrowser.ImageSize = value; }
        }

        public string ModPath 
        { 
            get { return mModPath; }  
        }

        public string ModName 
        { 
            get { return mModName; } 
        }

        private bool mBoundToZipArchive;
        private bool mBoundToCustom;
        public void BindToMod(string modPath, string modName)
        {

            //sets name of new mod database
            if (string.IsNullOrEmpty(modName)) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(modPath)) throw new ArgumentNullException();

            mModName = modName;            
            mModPath = modPath;


            mImageBrowser.Initialize(mModPath, mModName);
            mBoundToZipArchive = mModName.EndsWith (".zip");
            mBoundToCustom = false;
            // fill the combo box with mods (folders _first_  AND *.zip archived mods) from the mod path
            DirectoryInfo info = new DirectoryInfo(mModPath);
            FileInfo[] files = info.GetFiles(DB_SEARCH_PATTERN, SearchOption.TopDirectoryOnly);

            DirectoryInfo[] directories = info.GetDirectories();
            
            
            if (directories != null)
            	for (int i = 0; i < directories.Length; i++)
            		// calls AddRibbonButton but that function only creates a ButtonItem and that can be used as subitem to another ButtonItem
                    buttonModDBSelect.SubItems.Add(AddRibbonButton(directories[i].Name, directories[i].Name, null, null, 0, OnModDBSelection_Click));
            
            if (files != null)
                for (int i = 0; i < files.Length; i++)
                {
                    // calls AddRibbonButton but that function only creates a ButtonItem and that can be used as subitem to another ButtonItem
                    buttonModDBSelect.SubItems.Add(AddRibbonButton(files[i].Name, files[i].Name, null, null, 0, OnModDBSelection_Click));
                }

            RefreshBrowser(System.IO.Path.Combine (mModPath, mModName));
        }

        public void BindToCustom(BrowserController atlasBrowserController)
        {
            // note: the atlasBrowserController is instanced and already has path
            //       to the atlas resource descriptor
            if (atlasBrowserController == null) throw new ArgumentNullException();
            mImageBrowser.Initialize(atlasBrowserController);
            mBoundToZipArchive = false;
            mBoundToCustom = true;
            mModPath = null;
            mSelectedPath = null;
            mSelectedFile = null;
            RefreshBrowser(null);
        }

        public string SelectedPath // entry path WITHOUT the file or the archive relative path 
        {
            get
            {
                if (fileTree != null && fileTree.SelectedNode != null)
                    mSelectedPath = fileTree.SelectedNode.FullPath;

                // TODO: the selection of a new image should set
                // the mSelectedFile 
                // TODO: and typing in the filetextbox in the assetbrowser control
                // should set the mSelectedFile
                //if (mImageBrowser.SelectedImageViewer != null)
                //    path = System.IO.Path.Combine(path, mSelectedFile);

                return mSelectedPath;
            }
            set
            {
                // should be able to change the current selected path programmatically
                // and then select that in the fileTree as well. 
                //TreeNode node = fileTree.find
                //fileTree.SelectedNode = node;
               // throw new NotImplementedException();
                mSelectedPath = value;
                if (string.IsNullOrEmpty(mModPath) || mModName == null) return;
                RefreshBrowser( System.IO.Path.Combine(mModPath, mModName));
            }
        }

        public string SelecteFile // file without path 
        {
            get { return mSelectedFile; }
            set { mSelectedFile = value; }
        }

        public void SelectMod(string modname)
        {
            //if (comboSelectModDB.Items.Count > 0)
            //    for (int i = 0; i < comboSelectModDB.Items.Count; i++)
            //        if (comboSelectModDB.Items[i] as string == modname)
            //        {
            //            comboSelectModDB.SelectedIndex = i;
            //            break;
            //        }

            foreach (DevComponents.DotNetBar.BaseItem item in buttonModDBSelect.SubItems)
                if (item is DevComponents.DotNetBar.ButtonItem)
                {
                    DevComponents.DotNetBar.ButtonItem button = item as DevComponents.DotNetBar.ButtonItem;
                    if (button.Text == modname)
                        OnModDBSelection_Click (button, null);
                }
        }


        // Refresh is called whenever the selected path or zipfile changes.
        // TODO: I think we need to refresh automatically when we import files or add/create them.
        // TODO: if we were to pass in as a path or file selection
        //       our atlas file, 
        // TODO: importing atlas should try to import all necessary files and should
        //       rename the entries in the .tai file to also now have resourceDescriptor paths
        // TODO: if we wish to initialize the BrowserAtlasController, then im not sure where
        //       we do that.
        private void RefreshBrowser(string path)
        {
        	string iconPath = System.IO.Path.Combine (AppMain.DATA_PATH, @"Editor\ClosedFolder.ICO");
        	
            if (mBoundToCustom || mBoundToZipArchive)
            {              
            	if (string.IsNullOrEmpty(mSelectedPath) == false)
                    mImageBrowser.BrowseFolder(mSelectedPath, true, mAllowedExtensions);
                
                // treeview show only allowed if bound to disk folder or zip archive and NOT
                // to custom (least not yet... we'd need new fileTree.Load() methods)
                if (mBoundToZipArchive && mShowTreeView)
                    this.fileTree.Load(path, mSelectedPath, iconPath); // start browsing at root path which is ""
            }
            else 
            {
            	if (string.IsNullOrEmpty(path) == false)
	            	this.fileTree.Load(path, mSelectedPath, iconPath); // start browsing at root path which is ""

                mImageBrowser.BrowseFolder(mSelectedPath, true, mAllowedExtensions);
            }
        }

        // TODO: should our image atlases appear in filetree?
        private void fileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null) //  && !string.IsNullOrEmpty(e.Node.FullPath)) // it's ok to browse root, there could be images there at least
            {
                this.mImageBrowser.CancelBrowsing();

                string[] previousAllowedExtensions = mAllowedExtensions;
                
                // this.buttonCancel.Enabled = true;
                
                // HACK - filter these folders based on what we're expecting to find
                switch (e.Node.Text)
               {	
                	case "meshes":
                	case "actors":
                 		mAllowedExtensions = new string[] { ".KGBENTITY", ".KGBSEGMENT" };
                 		break;
                 	default:
                 		break;
                }
                
                mImageBrowser.BrowseFolder(e.Node.FullPath, true, mAllowedExtensions);
            
                mAllowedExtensions = previousAllowedExtensions;
            }
        }

        private void OnDropDownMenu_Clicked(object sender, EventArgs e)
        {
        }
        
        private void OnModDBSelection_Click(object sender, EventArgs e)
        {
            //RelativeArchivePath = comboSelectModDB.Text;
            if (sender is DevComponents.DotNetBar.ButtonItem)
            {
                DevComponents.DotNetBar.ButtonItem button = sender as DevComponents.DotNetBar.ButtonItem;
                BindToMod (mModPath, button.Text);

                foreach (DevComponents.DotNetBar.BaseItem item in buttonModDBSelect.SubItems)
                    if (item is DevComponents.DotNetBar.ButtonItem)
                        (item as DevComponents.DotNetBar.ButtonItem).Checked = false;

                button.Checked = true;

            }
            //buttonModDBSelect.SubItems.Add();
        }


        /// <summary>
        /// Drag and drop, cut/copy & paste with Explorer sample.
        /// 
        /// Paul Tallett, 30-Apr-06 (http://blogs.msdn.com/ptallett/)
        /// 
        /// Use the code any way you like - no warrenties given.
        /// </summary>
        #region Drag and Drop
        private void SharedDragOver (object sender, DragEventArgs e)
        {

            // Determine whether file data exists in the drop data. If not, then
            // the drop effect reflects that the drop cannot occur.
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }


            // Set the effect based upon the KeyState.
            // Can't get links to work - Use of Ole1 services requiring DDE windows is disabled
            //			if ((e.KeyState & (CTRL | ALT)) == (CTRL | ALT) &&
            //				(e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) 
            //			{
            //				e.Effect = DragDropEffects.Link;
            //			}
            //			
            //			else if ((e.KeyState & ALT) == ALT && 
            //				(e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) 
            //			{
            //				e.Effect = DragDropEffects.Link;
            //
            //			} 
            //			else
            if ((e.KeyState & SHIFT) == SHIFT &&
                (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effect = DragDropEffects.Move;

            }
            else if ((e.KeyState & CTRL) == CTRL &&
                (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                // By default, the drop action should be move, if allowed.
                e.Effect = DragDropEffects.Move;

                // Implement the rather strange behaviour of explorer that if the disk
                // is different, then default to a COPY operation
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && !files[0].ToUpper().StartsWith(homeDisk) &&			// Probably better ways to do this
                (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;

            // This is an example of how to get the item under the mouse
            //       Point pt = listView1.PointToClient(new Point(e.X, e.Y));
            //       ListViewItem itemUnder = listView1.GetItemAt(pt.X, pt.Y);
        }

        private void mImageBrowser_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void mImageBrowser_DragLeave(object sender, EventArgs e)
        {

        }

        private void mImageBrowser_DragOver(object sender, DragEventArgs e)
        {
            SharedDragOver(sender, e);
        }

        private string homeFolder = "";
        private string homeDisk = "";
        private void mImageBrowser_DragDrop(object sender, DragEventArgs e)
        {

            // Can only drop files, so check
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(file);

                if (descriptor.IsArchivedResource)
                {
                    // the file was dragged from within this mod database (zip)
                }
                else
                {
                    // the file(s) were dragged from disk
                    string dest = homeFolder + "\\" + Path.GetFileName(file);
                    bool isFolder = Directory.Exists(file);
                    bool isFile = File.Exists(file);
                    if (!isFolder && !isFile)				// Ignore if it doesn't exist
                        continue;

                    try
                    {
                        switch (e.Effect)
                        {
                            case DragDropEffects.Copy:
                                if (isFile)					// TODO: Need to handle folders
                                    File.Copy(file, dest, false);
                                break;
                            case DragDropEffects.Move:
                                if (isFile)
                                    File.Move(file, dest);
                                break;
                            case DragDropEffects.Link:		// TODO: Need to handle links
                                break;
                        }
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(this, "Failed to perform the specified operation:\n\n" + ex.Message, "File operation failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                
            }

           // RefreshView();
        }

        private void fileTree_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void fileTree_DragLeave(object sender, EventArgs e)
        {

        }

        private void fileTree_DragOver(object sender, DragEventArgs e)
        {
            SharedDragOver(sender, e);
        }

        // TODO: All DragDrop requests should be processed by our FormMain
        // just as we do with the AssetClicked() event.
        // This way our main form can do things like warn the user
        // that Moving a resource may affect other entities which depend on those
        // since currently I have no code that will recurse through and find all entities
        // and update their child ref's for resources that are moved.
        private void fileTree_DragDrop(object sender, DragEventArgs e)
        {
            // Can only drop files, so check
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(file);
                if (descriptor.IsArchivedResource)
                {
                }
                else
                {
                    string dest = homeFolder + "\\" + Path.GetFileName(file);
                    bool isFolder = Directory.Exists(file);
                    bool isFile = File.Exists(file);
                    if (!isFolder && !isFile)				// Ignore if it doesn't exist
                        continue;

                    try
                    {
                        switch (e.Effect)
                        {
                            case DragDropEffects.Copy:
                                if (isFile)					// TODO: Need to handle folders
                                    File.Copy(file, dest, false);
                                break;
                            case DragDropEffects.Move:
                                if (isFile)
                                    File.Move(file, dest);
                                break;
                            case DragDropEffects.Link:		// TODO: Need to handle links
                                break;
                        }
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(this, "Failed to perform the specified operation:\n\n" + ex.Message, "File operation failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
            }

            // RefreshView();
        }

        #endregion 

        #region Tool Menu button handlers
        private void buttonNewDB_Click(object sender, EventArgs e)
        {

        }

        private void buttonOpenDB_Click(object sender, EventArgs e)
        {

        }

        private void buttonSaveDB_Click(object sender, EventArgs e)
        {

        }

        private void buttonExit_Click(object sender, EventArgs e)
        {

        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {

        }

        private void buttonCloseDB_Click(object sender, EventArgs e)
        {

        }
        #endregion 

        #region ImageBrowser Event Handlers
        private void mImageBrowser_OnBrowsingAborted(object sender, ImageBrowserEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnBrowsingAborted");
        }

        private void mImageBrowser_OnBrowsingCanceled(object sender, ImageBrowserEventArgs e)
        {
            // this.buttonCancel.Enabled = false;
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnBrowsingCanceled");
        }

        private void mImageBrowser_OnBrowsingEnded(object sender, ImageBrowserEventArgs e)
        {
            //this.buttonCancel.Enabled = false;
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnBrowsingEnded");
        }

        private void mImageBrowser_OnBrowsingFoundImage(object sender, ImageBrowserEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnBrowsingFoundImage");
        }

        private void mImageBrowser_OnBrowsingNonImageFileFound(object sender, ImageBrowserEventArgs e)
        {
            if (QueryCustomImageForNonImage != null)
            {
                string modFullPath = System.IO.Path.Combine(this.mModPath, mModName);

                Image image = QueryCustomImageForNonImage(e.ImageFilePath, modFullPath);
                if (image != null)
                    mImageBrowser.AddImage(image,e.ImageModFolder, e.ImageFilePath); // note: we still use the original file, not the placeholder
            }
        }

        private void mImageBrowser_OnBrowsingStarted(object sender, ImageBrowserEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnBrowsingStarted");
        }


        System.Diagnostics.Stopwatch mDragStartStopWatch;
        const int DRAG_START_INTERVAL = 50;

        private void mImageBrowser_OnImageMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mDragStartStopWatch = System.Diagnostics.Stopwatch.StartNew();
            }
        }

        private void mImageBrowser_OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mDragStartStopWatch.ElapsedMilliseconds >= DRAG_START_INTERVAL)
                {
                    string[] files = new string[1];
                    KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor( mModName,((ImageViewer)sender).ImagePath);
                    files[0] = descriptor.ToString();

                    DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy | DragDropEffects.Move /* | DragDropEffects.Link */);
                }
            }
        }

        private void mImageBrowser_OnImageMouseUp(object sender, MouseEventArgs e)
        {
            mDragStartStopWatch.Reset();
        }

        private void mImageBrowser_OnImageDoubleClicked(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("mImageBrowser_OnImageDoubleClicked");
        }

        private void mImageBrowser_OnImageClicked(object sender, MouseEventArgs e)
        {
            if (sender == null) return;
            ImageBrowser browser = (ImageBrowser)sender;
            ImageViewer viewer = null;
            if (browser != null)
                viewer = browser.SelectedImageViewer;


            mSelectedModFolder = viewer.ImageModFolder;
            // assign the selected file so external readers of this.SelectedFile can discover it
            mSelectedFile = viewer.Caption; 

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // no matter what the asset, we'll notify the caller what was clicked
                    // by passing a resource descriptor to the callback
                    if (OnAssetClicked != null)
                        OnAssetClicked(this, new AssetClickedEventArgs(e , mModName, mSelectedModFolder, mSelectedFile, false));

                    break;

                case MouseButtons.Right:
                    // TODO: right mouse click for things like 
                    // "Delete", "Edit" (if .css, .fx, .txt, etc)
                    if (mImagePreviewEnabled && viewer != null)
                    {
                        if (m_ImageDialog == null || m_ImageDialog.IsDisposed) m_ImageDialog = new ImageDialog();
                        if (!m_ImageDialog.Visible) m_ImageDialog.Show();

                        m_ImageDialog.SetImage(viewer.ImagePath);
                    }
                    if (OnAssetClicked != null)
                        OnAssetClicked(this, new AssetClickedEventArgs(e, mModName, mSelectedModFolder, mSelectedFile, false));

                    break;

                case MouseButtons.Middle:
                    break;

                default:
                    return;
            }
        }



        private void fileTree_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left )
                mDragStartStopWatch = System.Diagnostics.Stopwatch.StartNew();

        }

        private void fileTree_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mDragStartStopWatch.ElapsedMilliseconds >= DRAG_START_INTERVAL)
                    DoDragDrop(fileTree, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void fileTree_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (mDragStartStopWatch != null)
                mDragStartStopWatch.Reset();
        }

        private void fileTree_OnNodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        { 
        }

        private void fileTree_OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            
            mSelectedModFolder = e.Node.FullPath;

            if (OnAssetClicked != null)
                OnAssetClicked(sender, new AssetClickedEventArgs(e, mModName, mSelectedModFolder, null, false));
        }

        private void fileTree_OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (mSelectedModFolder == e.Node.FullPath)
            {
                // the same node has been clicked twice, start a label edit
                fileTree.LabelEdit = true;
                e.Node.BeginEdit();
                

            }
        }

        private void fileTree_OnItemDrag(object sender, ItemDragEventArgs e)
        {
        }

        private void fileTree_OnBeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            
        }

        private void fileTree_OnAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            fileTree.LabelEdit = false;
            if (string.IsNullOrEmpty(e.Label)) return;

            System.Diagnostics.Debug.WriteLine ("folder name = " + e.Label); 
            System.Diagnostics.Debug.WriteLine ("full path = " + e.Node.FullPath );


            
            //fileTree.LabelEdit = false;
            //e.CancelEdit = true;
            //if (e.Label == null)
            //{
            //    return;
            //}
            //ValidateLabelEditEventArgs ea = new ValidateLabelEditEventArgs(e.Label);
            //OnValidateLabelEdit(ea);
            //if (ea.Cancel == true)
            //{
            //    e.Node.Text = editedLabel;
            //    fileTree.LabelEdit = true;
            //    e.Node.BeginEdit();
            //}
            //else
            //    base.OnAfterLabelEdit(e);

        }

        public void BeginEdit()
        {
            //StartLabelEdit();
        }
		void MImageBrowserLoad(object sender, EventArgs e)
		{
	
		}
        #endregion


    }

    public class AssetClickedEventArgs : EventArgs
    {
        public AssetClickedEventArgs(MouseEventArgs mouseArgs, string modName, string modFolder, string assetName, bool isFolder)
        {
            this.MouseEventArgs = mouseArgs;
            this.ModName = modName;
            this.ModFolder = modFolder;
            this.AssetName = assetName;
            this.IsFolder = isFolder;
        }

        public MouseEventArgs MouseEventArgs;
        public bool IsFolder;
        public string ModName;
        public string ModFolder;
        public string AssetName;
    }

    public class EntryRenamedEventArgs : EventArgs
    {
        public EntryRenamedEventArgs (KeyCommon.IO.ResourceDescriptor oldResource, 
            KeyCommon.IO.ResourceDescriptor newResource,
            bool isFolder)
        {
            this.OldResource = oldResource;
            this.NewResource = newResource;
            this.IsFolder = isFolder;
        }

        public bool IsFolder;
        public KeyCommon.IO.ResourceDescriptor OldResource;
        public KeyCommon.IO.ResourceDescriptor NewResource;
    }

    public delegate Image QueryCustomImageForNonImageEventHandler (string filename, string archiveFullPath);
    public delegate void AssetClickedEventHandler(object sender, AssetClickedEventArgs e);

    public delegate void EntryRenamedEventHandler (object sender, EntryRenamedEventArgs e);
}
