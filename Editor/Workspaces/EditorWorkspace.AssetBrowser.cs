using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;


namespace KeyEdit.Workspaces
{
    public partial class EditorWorkspace
    {
        protected virtual void InitializeAssetBrowser()
        {
            mBrowser = new KeyEdit.GUI.AssetBrowserControl();

            mBrowser.ShowTreeView = true;
            mBrowser.RecurseSubFolders = false; // we do not want the imagebrowser recursing!
            mBrowser.AllowedExtensions = null;
            mBrowser.ImageSize = 64;

            // ModPath needs to be set after recurse = false or it will immediately start
            // searching before we have a chance to set .RecurseSubFolders = false
            mBrowser.BindToMod ( AppMain.MOD_PATH, AppMain.ModName);

            mBrowser.OnAssetClicked += AssetSelected;
            mBrowser.OnEntryRenamed += EntryRenamed;
            mBrowser.QueryCustomImageForNonImage += OnNonImageFileFound;

            mBrowser.AddRibbonButton("Preview", "Preview", "Preview", null, 1, OnShowPreview_Click);
        }

        internal static System.Drawing.Image OnNonImageFileFound(string filename, string archivePath)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            System.Drawing.Image img = null;

            switch (Path.GetExtension(filename).ToUpper())
            {
            	case ".KGBENTITY":
                case ".KGBSEGMENT":
                    try 
                    {
                    	// open the kgbentity archive from within the existing archive
                        // TODO: stream will be null if the archive is already opening by another process (eg 7zip)
	                    Stream stream  = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(filename, "", archivePath);
	                    Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(stream);
	                    if (zip.ContainsEntry("preview.png"))
	                    {
	                        MemoryStream mem = new System.IO.MemoryStream();
	                        zip["preview.png"].Extract(mem);
	                        mem.Seek(0, System.IO.SeekOrigin.Begin);
	                        img = System.Drawing.Image.FromStream(mem);
	                    }
	                    // TODO: grab the description from the SceneInfo
	                    //		 and assign it to the image tag.... or assign to an out parameter
	                              
	                    stream.Dispose();
	                    zip.Dispose();
                    }
                    catch (Exception ex)
                    {
                    	// if the kgbentity or kgbsegment is corrupt, ignore adding preview image to imagelist
                    	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.AssetBrowser.OnNonImageFileFound() - ERROR: " + ex.Message);
                    }
                    // load place holder instead
                    if (img == null)
                    	img = System.Drawing.Image.FromFile(System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\icons\wireframe.png"));
                    break;
                case ".TXT":
                //break;
                case ".FX":
                //break;
                case ".CSS":
                    img = System.Drawing.Image.FromFile(System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\icons\code.png")); //csscript128.jpg");
                    break;

                case ".MTL":
                    break;

                case ".OBJ":
                case ".X":
                case ".TVA":
                case ".TVM":
                    img = System.Drawing.Image.FromFile(System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\icons\wireframe.png"));
                    break;
                default:
                    // anything else we don't recognize we skip (eg html, hlp, .xml)
                    break;
            }

            return img;
        }

        protected void EntryRenamed(object sender, EventArgs e)
        {

            if ((sender is ToolStripMenuItem) == false) return;
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((ToolStripMenuItem)sender).Name);
            string relativeZipPath = descriptor.ModName;
            string pathInArchive = descriptor.EntryName;

            // we should not allow rename of treeview node directly but instead pop up an input box?
            string newPathInArchive = "";
            KeyCommon.Messages.MessageBase message = new KeyCommon.Messages.Archive_RenameEntry(relativeZipPath, pathInArchive, newPathInArchive);

            if (message != null)
                AppMain.mNetClient.SendMessage(message);
        }

        protected void AssetSelected(object sender, KeyEdit.GUI.AssetClickedEventArgs e)
        {
            switch (e.MouseEventArgs.Button)
            {
                case MouseButtons.Right:
                    AssetRightMouseClick(sender, e);
                    break;
                case MouseButtons.Left:
                    AssetLeftMouseClick(sender, e);
                    break;
                case MouseButtons.Middle:
                default:
                    break;
            }
        }

        protected void AssetLeftMouseClick(object sender, KeyEdit.GUI.AssetClickedEventArgs e)
        {
            if (AppMain._core.SceneManager == null ||
                AppMain._core.SceneManager.Scenes == null ||
                AppMain._core.SceneManager.Scenes.Length == 0) return;


            if (string.IsNullOrEmpty (e.AssetName)) return;
            string assetName = e.AssetName;
            string path = Path.Combine (e.ModFolder, assetName);


            // based on mod folder determine the brush style
            // it is the responsibility of modders to place their entities under the correct mod folder
            // so that the brush style is appropriate (eg a hatch requires a brush that edits entire cells only and not individual tiles)
            uint brushStyle = (uint)Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP;
            int stringlen = e.ModFolder.Length;
            int firstSlash = e.ModFolder.IndexOf("\\", 0) + 1;
            string subFolder =  e.ModFolder.Substring(firstSlash, stringlen - firstSlash);
            switch (subFolder.ToLower())
            {
                case "entities\\structure\\walls":
                    brushStyle = Keystone.EditTools.PlacementTool.BRUSH_EDGE_SEGMENT;
                    LoadSegmentPainterTool(path, assetName);
                    return;
                    break;
                case "entities\\structure\\hatches":
                    brushStyle = Keystone.EditTools.PlacementTool.BRUSH_HATCH;
                    break;
                case "entities\\structure\\doors":
                    brushStyle = Keystone.EditTools.PlacementTool.BRUSH_DOOR;
                    break;
                case "entities\\accomodations":
                    brushStyle = Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP;
                    break;
                default:
                    brushStyle = (uint)Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP;
                    break;
            }

            


            switch (Path.GetExtension (path).ToUpper())
            {
                case ".KGBSEGMENT":

                case ".KGBENTITY":// represents a compressed archive (xmldb) within the overall xmldb

                    // the primary problem here is that the viewport rendering is not
                    // threaded.
                    FormPleaseWait waitDialog = new FormPleaseWait();
                    waitDialog.Owner = this.mWorkspaceManager.Form;
                    waitDialog.Show(this.mWorkspaceManager.Form);

                    // TODO: here, the selected asset when loaded should be placed
                    //       into our Repository and somehow flagged as a prefab that exists
                    //       and manages links to entities that are using it but otherwise
                    //       is not used in the scene.  The prefab is also used by the AssetPlacementTool
                    // TODO: when saving an entity that references a Prefab, that entity then should be loaded
                    //       when reading the 'DEF' attribute 
                    KeyEdit.Workspaces.Tools.AssetPlacementTool placer = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                    (
                            AppMain.mNetClient,
                            path, 
                            brushStyle,
                            null
                    );
                    waitDialog.Close();

                    if (Path.GetExtension (path).ToUpper() == ".KGBSEGMENT")
                    	placer.SetValue ("layout", assetName);
                    
                    this.CurrentTool = placer;
                    break;

//                case ".KGBSEGMENT":
//                    
//                    if (AppMain._core.SceneManager == null ||
//                        AppMain._core.SceneManager.Scenes == null ||
//                        AppMain._core.SceneManager.Scenes.Length == 0) return;
//                    
//                    KeyEdit.Workspaces.Tools.AutoTileSegmentPainter painter = new KeyEdit.Workspaces.Tools.AutoTileSegmentPainter 
//                	(
//						AppMain.mNetClient,
//						resource
//                	);
//                    
//                    painter.SetValue ("layout", resource.ToString());
//                    this.CurrentTool = painter;
//                    break;
                
               	case ".X":
                case ".TVM":
                case ".OBJ":
                case ".TVA":
                    //ICommandProgress progress = new FormPleaseWait();

                    //  ICommand2 command = new ImportGeometry(relativeZipFilePath, pathInArchive, true, true, false);
                    //  command.BeginExecute(CommandCompleted, progress);

                    //  ((FormPleaseWait)progress).ShowDialog(this);

                    // interface ICommandProgress
                    // {
                    //      // the ImportAsset command itself can host the zip reading.  
                    //      // and perhaps we can move the zip function out to the command itself
                    //      // so that we can continue to pass the final string to the load function?
                    //      // then inside the command we receive any zip update which can in turn
                    //      // update the ICommandProgress where we iterate and if the Mesh loading has started
                    //      // we can switch to that progress based on an estimate milliseconds it should take
                    //      // based on the size of the file in bytes.
                    //      // Then we can call ICommandProgress.Update()  to set the new values

                    // }
                    break;
                case ".CSS":
                case ".FX":
                    break;
                case ".TA_INDEX":
                case ".TAI": // nvidia texture atlas index file
                    if (this.CurrentTool is KeyEdit.Workspaces.Tools.InteriorSegmentPainter)
                    {
                        KeyEdit.Workspaces.Tools.TileSegmentPainter floorPainter =
                            (KeyEdit.Workspaces.Tools.TileSegmentPainter)this.CurrentTool;

                        // note: for atlas sub-textures, RelativePathToArchive contains
                        //       a full ResourceDescriptor that includes a path to archive if relevant
                        //       as well as the name of the .TAI file.
                        //       PathInArchive then contains either disk file name or since there is
                        //       no actual zip entry for this dynamically generated sub-texture 
                        //       that is being displayed in the ImageBrowser, it contains 
                        //       a friendly name.Index
                        // extract the atlas index from "pathInArchive"
                        throw new NotImplementedException();
//                        string[] splitString = assetName.Split(new char[] { '.'});
//                        int length = splitString.Length;
//                        // second to last element will contain our atlas index
//                        int index = int.Parse (splitString[length - 2]);
//
//                        Keystone.Portals.SegmentStyle style = floorPainter.GetValue();
//                        if (style == null) break;
//                        style.BottomLeftTexturePath = index.ToString ();
//                        style.TopRightTexturePath = index.ToString();
//                        floorPainter.SetValue("tile style", style);
                    }
                    break;
                default:
                    break;

            }
        }

        private void LoadSegmentPainterTool(string path, string assetName)
        {
            // the primary problem here is that the viewport rendering is not
            // threaded.
            FormPleaseWait waitDialog = new FormPleaseWait();
            waitDialog.Owner = this.mWorkspaceManager.Form;
            waitDialog.Show(this.mWorkspaceManager.Form);

            if (this.mWorkspaceManager.CurrentWorkspace is FloorPlanDesignWorkspace == false)
            {

                // TODO: here, the selected asset when loaded should be placed
                //       into our Repository and somehow flagged as a prefab that exists
                //       and manages links to entities that are using it but otherwise
                //       is not used in the scene.  The prefab is also used by the AssetPlacementTool
                // TODO: when saving an entity that references a Prefab, that entity then should be loaded
                //       when reading the 'DEF' attribute 
                KeyEdit.Workspaces.Tools.AssetPlacementTool placer = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                (
                        AppMain.mNetClient,
                        path,
                        Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
                        null
                );
                waitDialog.Close();

                if (Path.GetExtension(path).ToUpper() == ".KGBSEGMENT")
                    placer.SetValue("layout", assetName);

                this.CurrentTool = placer;
                return;
            }
            KeyEdit.Workspaces.Tools.WallSegmentPainter painter =
                new KeyEdit.Workspaces.Tools.WallSegmentPainter(AppMain.mNetClient);

            Keystone.Portals.EdgeStyle edgeStyle = new Keystone.Portals.EdgeStyle();


            // prefabs contain model selector with multiple models 
            edgeStyle.Prefab = path;
            edgeStyle.FloorAtlasIndex = -1; // null; // TODO: this should just use the texture in the prefab. System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\textures\walls\wall00.png"); // TODO: Texture application not working
            edgeStyle.CeilingAtlasIndex = -1; // null; // edgeStyle.BottomLeftTexturePath;

            // todo; using prefab, we must compute the footprint of the selected models we use on the edge or corner.

            // TODO: I think LayerNames should not be arrays. 
            painter.SetValue("wall style", edgeStyle);
            waitDialog.Close();

            this.CurrentTool = painter;

        }

        private void AssetRightMouseClick(object sender, KeyEdit.GUI.AssetClickedEventArgs e)
        {
            if ((sender is Control) == false) return;
            System.Windows.Forms.ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;

            string assetName = e.AssetName;
            string path = e.ModFolder;
            if (string.IsNullOrEmpty (assetName) == false)
            	path = Path.Combine (e.ModFolder, assetName);
            
            path = Path.Combine (e.ModName, path);
            string ext = Path.GetExtension(path).ToUpper();


            // New -->  Folder
            //          Script
            //          Shader
            //          Text File

            item = new ToolStripMenuItem("New Folder");
            item.Name = path;
            item.Click += menuNewFolder_Click;
            menu.Items.Add(item);

            item = new ToolStripMenuItem("New Text File"); // automatically creates text file and prompts for save path / name
            item.Name = path;
            item.Click += menuNewTextFile_Click;
            menu.Items.Add(item);

            menu.Items.Add(new ToolStripSeparator());

            // Import --> Existing file adds it to archive but makes no attempt at creating an Entity or Mesh3d/Actor3d/Texture IResource node.
            item = new ToolStripMenuItem("Import File");
            item.Name = path;
            item.Click += menuImportEntity_Click;
            menu.Items.Add(item);

            menu.Items.Add(new ToolStripSeparator());
            // Import --> Entity
            item = new ToolStripMenuItem("Import as Entity");
            item.Name = path;
            item.Click += menuImportEntity_Click;
            menu.Items.Add(item);

            // Import --> Container (exterior with interior)
            item = new ToolStripMenuItem("Import as Container");
            item.Name = path;
            item.Click += menuImportEntity_Click;
            menu.Items.Add(item);

            // Export --> Extracts a copy
            // Convert --> converts to a new extension within same folder
            // --

            menu.Items.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem("Cut");
            item.Name = path;
            item.Click += mnuCut_Click;
            menu.Items.Add(item);

            item = new ToolStripMenuItem("Copy");
            item.Name = path;
            item.Click += mnuCopy_Click;
            menu.Items.Add(item);

            item = new ToolStripMenuItem("Paste"); // (grays out if clipboard is empty)
            item.Name = path;
            item.Click += mnuPaste_Click;
            menu.Items.Add(item);

            menu.Items.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem("Delete");
            item.Name = path;
            item.Click += menuDelete_Click;
            menu.Items.Add(item);

            item = new ToolStripMenuItem("Rename"); // rename on resource requires potentially all targets to rename their ref id's for this node
            item.Name = path;
            item.Click += mnuRename_Click;
            menu.Items.Add(item);

            // Properties



            if (!string.IsNullOrEmpty(ext))
            {
                switch (ext.ToUpper())
                {
                    case ".FX":
                    case ".CSS":

                        item = new ToolStripMenuItem("Edit");
                        item.Name = path;
                        item.Click += EditMenu_OnClick;
                        menu.Items.Add(item);
                        break;

                    default:
                        break;
                }
            }

            // convert the screen space coord to form relative
            //Point formRelativePosition = PointToClient(e.MouseEventArgs.Location);
            menu.Show((Control)sender, e.MouseEventArgs.Location);
        }

        /// <summary>
        /// Occurs when gallery item like .CSS or .FX is right mouse clicked and then "Edit" is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditMenu_OnClick(object sender, EventArgs e)
        {
            KeyCommon.IO.ResourceDescriptor descriptor;

            if (sender is System.Windows.Forms.ToolStripMenuItem)
                descriptor = new KeyCommon.IO.ResourceDescriptor(((System.Windows.Forms.ToolStripMenuItem)sender).Name);
            else
                descriptor = new KeyCommon.IO.ResourceDescriptor(((MenuItem)sender).Name);

            CodeEditorWorkspace codeEditor = new CodeEditorWorkspace(descriptor.ToString());

            mWorkspaceManager.Add(codeEditor, null);
            mWorkspaceManager.ChangeWorkspace(codeEditor.Name, false);
        }

     
        private void menuImportEntity_Click(object sender, EventArgs e)
        {
            if ((sender is ToolStripMenuItem) == false) return;

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((ToolStripMenuItem)sender).Name);
            string modName = descriptor.ModName;
            string entryPath = descriptor.EntryName;

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = AppMain.DATA_PATH;

            // TODO: since our Import can contextually load anything, we we may eventually
            // add seperate buttons for only geometry, only whatever for filtering sake but

            //openFile.Filter = ALL_GEOMETRY_FILE_FILTER;
            //openFile.FilterIndex = DEFAULT_FILTER_INDEX;

            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                KeyCommon.Messages.MessageBase message = null;

                string sourceFile = openFile.FileName;
                string extension = Path.GetExtension(sourceFile).ToUpper();

                switch (extension)
                {
                    // meshes are supported
                    case ".OBJ":
                    case ".X":
                    case ".TVA":
                    case ".TVM":

                        bool convertObjToTVM = false;
                        bool loadXFileAsActor = false;
                        bool loadTextures = true;
                        bool loadMaterials = true;
                        // Archive_AddGeometry actually does import the geometry
                        // as an Entity.  Should modify name of this to reflect that

                        // TODO: prompt for user to specify a filename to use for the KGBEntity
                        // xml that will be created.  Although the .x/.tvm/.obj will get imported
                        // with it's original name to the specified path, we will be able to specify
                        // the .kgbentity filename at least.  

                        const string PREFAB_EXTENSION = ".kgbentity";
                        string inputResult = System.IO.Path.GetFileNameWithoutExtension(sourceFile) + PREFAB_EXTENSION;
                        // TODO: wait, this is the new .x/.tvm/.obj filename, not the Entity.
                        // Hrm... how can we make entity creation vs mesh importation more intuitive
                        // because clearly i forgot myself what was going on... what was being imported.
                        // 
                        if (extension == ".X")
                        {
                            if (KeyPlugins.StaticControls.YesNoBox("Import .X as Boned Actor?", "Import as Actor?") == DialogResult.Yes)
                            {
                                loadXFileAsActor = true;
                            }
                        }

                        if (KeyPlugins.StaticControls.InputBox("Enter file name for new Entity...", "Entity file name:", ref inputResult) == DialogResult.OK)
                        {
                            // NOTE: isInteriorContainer is ignored for BonedActors
                            bool isInteriorContainer = ((ToolStripMenuItem)sender).Text == "Import as Container";

                            message = new KeyCommon.Messages.Archive_AddGeometry(modName,
                                sourceFile, entryPath, inputResult, loadTextures, loadMaterials, isInteriorContainer);

                            ((KeyCommon.Messages.Archive_AddGeometry)message).LoadXFileAsActor = loadXFileAsActor;
                        }
                        break;
                    // materials are valid
                    case ".MTL":
                    // image textures are supported
                    case ".DDS":
                    case ".PNG":
                    case ".BMP":
                    case ".JPG":
                    case ".GIF":
                    case ".TGA":
                    // scripts are supported
                    case ".CSS":
                    case ".FX":
                    case ".TXT":

                        string[] sourceFiles = new string[1] { sourceFile };
                        string[] targetPaths = new string[1] { entryPath };
                        string[] newNames = new string[1] { Path.GetFileName(sourceFile) };

                        message = new KeyCommon.Messages.Archive_AddFiles(modName, sourceFiles, targetPaths, newNames);

                        break;

                    default:
                        MessageBox.Show(string.Format("Unsupported file type {0}.", extension.ToUpper()));
                        break;
                }

                if (message != null)
                    AppMain.mNetClient.SendMessage(message);

            }
        }

        private void menuNewTextFile_Click(object sender, EventArgs e)
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((ToolStripMenuItem)sender).Name);
            string relativeZipPath = descriptor.ModName;
            string pathInArchive = descriptor.EntryName;

            string newFileName = "";
            DialogResult result = KeyPlugins.StaticControls.InputBox("Enter name of new file:", "New File Name?", ref newFileName);

            if (string.IsNullOrEmpty(newFileName)) return;

            KeyCommon.Messages.MessageBase message = new KeyCommon.Messages.Archive_AddFiles(relativeZipPath, null, new string[] { pathInArchive }, new string[] {newFileName});
            if (message != null)
                AppMain.mNetClient.SendMessage(message);
        }

        private void menuNewFolder_Click(object sender, EventArgs e)
        {

            if ((sender is ToolStripMenuItem) == false) return;

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((ToolStripMenuItem)sender).Name);
            string relativeZipPath = descriptor.ModName;
            string pathInArchive = descriptor.EntryName;

            string newFolderName = null;
            DialogResult result = KeyPlugins.StaticControls.InputBox("Enter name of new folder:", "New Folder Name?", ref newFolderName);

            // TODO: should we prevent users from adding "/" or "\" characters or "." etc
            // or any invalid folder character and only allow one folder at a time
            // and not multiple like "firstfolder\subfolder\anothersubfolder"
            if (result == DialogResult.Cancel) return;
            if (string.IsNullOrEmpty (newFolderName)) return;

            pathInArchive = System.IO.Path.Combine(pathInArchive, newFolderName);
            // TODO: we should check for existing folder name first
            // TODO: we should validate the folder name string 

            KeyCommon.Messages.MessageBase message = new KeyCommon.Messages.Archive_AddFolder(relativeZipPath, pathInArchive);

            if (message != null)
                AppMain.mNetClient.SendMessage(message);
        }


        private void menuDelete_Click(object sender, EventArgs e)
        {
            if ((sender is ToolStripMenuItem) == false) return;

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((ToolStripMenuItem)sender).Name);
            string relativeZipPath = descriptor.ModName;
            string pathInArchive = descriptor.EntryName;


            KeyCommon.Messages.MessageBase message = new KeyCommon.Messages.Archive_DeleteEntry(relativeZipPath, pathInArchive);

            // TODO: perhaps the FormMain.Commands upon receipt of confirmation
            // should then notify every workspace to remove this node
            // from the archive

            if (message != null)
                AppMain.mNetClient.SendMessage(message);

        }

        private void mnuCut_Click(object sender, EventArgs e)
        {
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
        }
        private void mnuPaste_Click(object sender, EventArgs e)
        {
        }

        private void mnuRename_Click(object sender, EventArgs e)
        {
            // TODO: rename is complicated. It can ruin the linkages of assets to entities
            //       prefabs stored in the mod db.  
            // TODO: rename must also surely send a message to the server so it can sychronize
            //       the rename.
            this.mBrowser.BeginEdit();

        }

        System.Windows.Forms.Form mPreviewForm;

        // TODO: the following is broken.
        // This old preview implementations is never tied to a PreviewWorkspace and thus
        // never instantiates a viewportControl for the form.  This got broken when 
        // I implemented Preview to be a seperate workspace so that I could have
        // unique input controls
        internal void OnShowPreview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mBrowser.SelecteFile))
            {
                MessageBox.Show("You must first select an entity asset in order to launch the preview window.", "No entity asset selected.");
                return;
            }
            string entry = System.IO.Path.Combine(mBrowser.SelectedPath, mBrowser.SelecteFile);
            // todo: i dont think the below combine is necessary... we just need mBrowser.ModPath;
            string fullModPath = System.IO.Path.Combine(mBrowser.ModPath, mBrowser.ModName);

            // TODO: note we dont create a seperate workspace for this FormPreview... does it work?
            mPreviewForm = new FormPreview(fullModPath, entry);
            
            // TODO: i had major problems last time with instancing some GUI from the threadpool threads
            // we MUST not ever do that.  It results in all kinds of weird hangs and stopping of the message pump.
            // I think this mPreview form is now doing something like that perhaps. Or it could be something else
            // but keep this todo reminder here until we resolve that. Off hand, this should be on
            // the proper update thread because it's triggered by response to button press
            // windows message to call this Preview(object sender, EventArgs e) event
            System.Diagnostics.Debug.Assert(mWorkspaceManager.Form == this.Manager.Form);
            mPreviewForm.ShowDialog(this.Manager.Form);
            mPreviewForm.Dispose();
            mPreviewForm = null;
        }
    }
}
