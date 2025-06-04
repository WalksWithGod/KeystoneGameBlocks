//using System;
//using System.ComponentModel;
//using System.Drawing;
//using System.Windows.Forms;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Windows.Forms;
//using Keystone.Cameras;
//using Keystone.Commands;
//using Keystone.Controllers;
//using Keystone.Devices;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Types;
//using Keystone.Appearance;
//using Keystone.IO;
//using Keystone.Resource;
//using Keystone.Scene;
//using Keystone.Types;
//using Lidgren.Network;
//using MTV3D65;
//using System.Collections.Generic;
//using DevComponents.DotNetBar;
//using Keystone.Celestial;
//using Ionic.Zip;
//namespace KeyEdit
//{
//    partial class FormMain : FormMainBase
//    {
//        #region Archive Gallery Manipulation
//        private void buttonCreateNewArchiveFolder_Click(object sender, EventArgs e)
//        {
//            string folderName = "";
//            DialogResult result = InputBox("New Folder Name", "Enter the name of this folder.", ref folderName);

//            if (result == DialogResult.OK)
//            {
//                folderName = (string)galleryAssets.Tag + "/" + folderName;
//                KeyCommon.Messages.Archive_AddFolder message = new KeyCommon.Messages.Archive_AddFolder(
//                    AppMain.ModName, folderName);
//                AppMain.mNetClient.SendMessage(message);
//            }
//        }

//        private void buttonDeleteArchiveItem_Click(object sender, EventArgs e)
//        {
//            if ((sender is MenuItem) == false) return;

//            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(((MenuItem)sender).Name);
//            string zipFilePath = Path.Combine(AppMain._core.ModsPath, descriptor.RelativePathToArchive);
//            string[] files = new string[1];

//            DialogResult result = MessageBox.Show("Do you really wish to delete the selected file or folder?", "Confirm Delete?", MessageBoxButtons.YesNo);

//            if (result == DialogResult.Yes)
//            {
//                KeyCommon.Messages.Archive_DeleteEntry delete = new KeyCommon.Messages.Archive_DeleteEntry();
//                delete.RelativeZipFilePath = descriptor.RelativePathToArchive;
//                delete.ZipEntries = files;
//                AppMain.mNetClient.SendMessage(delete);
//            }
//        }

//        // TODO: this is about to be obsolete now that we have new AssetBrowser and
//        // no more gallery
//        private void buttonImportFile_Click(object sender, EventArgs e)
//        {
//            if (sender == null) return;
//            string senderName = ((DevComponents.DotNetBar.BaseItem)sender).Name;
//            string senderTag = (string)((DevComponents.DotNetBar.BaseItem)sender).Tag;

//            OpenFileDialog openFile = new OpenFileDialog();
//            openFile.InitialDirectory = AppMain.DATA_PATH;

//            // TODO: since our Import can contextually load anything, we we may eventually
//            // add seperate buttons for only geometry, only whatever for filtering sake but

//            //openFile.Filter = ALL_GEOMETRY_FILE_FILTER;
//            //openFile.FilterIndex = DEFAULT_FILTER_INDEX;

//            openFile.RestoreDirectory = true;

//            if (openFile.ShowDialog() == DialogResult.OK)
//            {
//                KeyCommon.Messages.MessageBase message = null;

//                // parse the sender tag for the zip archive, and the path within that archive
//                string zipPath = AppMain.ModName;
//                string pathInArchive = (string)galleryAssets.Tag; // "meshes";

//                string sourceFile = openFile.FileName;
//                string extension = Path.GetExtension(sourceFile).ToUpper();

//                switch (extension)
//                {
//                    // meshes are supported
//                    case ".OBJ":
//                    case ".X":
//                    case ".TVA":
//                    case ".TVM":

//                        bool convertObjToTVM = false;
//                        bool loadXFileAsActor = false;
//                        bool loadTextures = true;
//                        bool loadMaterials = true;
//                        message = new KeyCommon.Messages.Archive_AddGeometry(zipPath, sourceFile, pathInArchive, "", loadTextures, loadMaterials);
//                        break;
//                    // materials are valid
//                    case ".MTL":
//                    // image textures are supported
//                    case ".DDS":
//                    case ".PNG":
//                    case ".BMP":
//                    case ".JPG":
//                    case ".GIF":
//                    case ".TGA":
//                    // scripts are supported
//                    case ".CSS":
//                    case ".FX":
//                    case ".TXT":

//                        string[] sourceFiles = new string[1] { sourceFile };
//                        string[] targetPaths = new string[1] { pathInArchive };
//                        string[] newNames = new string[1] { Path.GetFileName(sourceFile) };

//                        message = new KeyCommon.Messages.Archive_AddFiles(zipPath, sourceFiles, targetPaths, newNames);

//                        break;

//                    default:
//                        MessageBox.Show(string.Format("Unsupported file type {0}.", extension.ToUpper()));
//                        break;
//                }

//                if (message != null)
//                    AppMain.mNetClient.SendMessage(message);
//            }
//        }



//        private void buttonNewScript_Click(object sender, EventArgs e)
//        {
//            // popup "Enter script name" file.  We will append .css to the end of the name if it's not already
//            string scriptName = "";
//            DialogResult result = InputBox("New Shader or Script Name", "Enter the name of this script.", ref scriptName);
//            if (result == DialogResult.OK)
//            {
//                // if invalid extension, we will also prompt
//                string ext = Path.GetExtension(scriptName);
//                if (string.IsNullOrEmpty(ext) || (ext.ToUpper() != ".CSS" && ext.ToUpper() != ".FX"))
//                {
//                    MessageBox.Show("Invalid extension.  Script's added to module must end with .css extension.  Shader's added to module must end with .fx extension.");
//                    return;
//                }

//                // archive path is the current gallery path BUT DO NOT add the filename if you want it to be different.  That targetpath
//                // doesnt work that way.  To change the filename you then must change the entry's "entry.Filename" property prior to zip.Save
//                string pathInArchive = (string)galleryAssets.Tag;

//                // create a temp file for our script so that AddFilesToArchive will have a valid source path
//                string sourcePath = Keystone.IO.XMLDatabase.GetTempFileName();
//                StreamWriter sw = File.CreateText(sourcePath);

//                string[] sourceFiles = new string[1] { sourcePath };
//                string[] targetPaths = new string[1] { pathInArchive };
//                string[] newNames = new string[1] { scriptName };

//                KeyCommon.Messages.Archive_AddFiles addFiles = new KeyCommon.Messages.Archive_AddFiles();
//                addFiles.RelativeZipFilePath = AppMain.ModName;
//                addFiles.SourceFiles = sourceFiles;
//                addFiles.EntryDestinationPaths = targetPaths;
//                addFiles.EntryNewFilenames = newNames;

//                AppMain.mNetClient.SendMessage(addFiles);
//            }
//        }

//        /// <summary>
//        /// Add a mesh based entity to the archive
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void menuItemImportGeometry_Click(object sender, EventArgs e)
//        {
//            if (sender == null) return;
//            string senderName = ((DevComponents.DotNetBar.BaseItem)sender).Name;
//            string senderTag = (string)((DevComponents.DotNetBar.BaseItem)sender).Tag;

//            OpenFileDialog openFile = new OpenFileDialog();
//            openFile.InitialDirectory = AppMain.DATA_PATH;

//            switch (senderName)
//            {
//                case "buttonImportVehicle":
//                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
//                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
//                    break;
//                case "buttonImportActor":
//                case "menuItemImportMultipartActor":
//                    openFile.Filter = ACTOR_GEOMETRY_FILE_FILTER;
//                    break;
//                case "buttonItemImportMesh":
//                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
//                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
//                    break;
//                case "buttonItemImportEditableMesh":
//                    openFile.Filter = EDITABLE_MESH_FILE_FILTER;
//                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
//                    break;
//                case "buttonImportMinimesh":
//                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
//                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
//                    break;
//                case "buttonImportBillboard":
//                    openFile.Filter = TEXTURE_FILE_FILTER;
//                    openFile.FilterIndex = TEXTURE_FILTER_INDEX;
//                    break;

//                case "buttonItemStarfield":
//                    openFile.Filter = TEXTURE_FILE_FILTER;
//                    openFile.FilterIndex = TEXTURE_FILTER_INDEX;
//                    break;
//            }

//            openFile.RestoreDirectory = true;

//            if (openFile.ShowDialog() == DialogResult.OK)
//            {
//                // parse the sender tag for the zip archive, and the path within that archive
//                string[] results = senderTag.Split(',');
//                string zipPath = results[0];
//                string pathInArchive = results[1];

//                string sourcePath = openFile.FileName;

//                string[] sourceFiles = new string[1];
//                string[] targetPaths = new string[1];

//                sourceFiles[0] = sourcePath;
//                targetPaths[0] = pathInArchive;
//                //TODO: for actor and meshes, we should create a seperate command that can instance the mesh/actor and allow for user to
//                // possibly import the textures.  That's nothing we can do here with AddFilesToArchive.  We should have a dedicated 
//                // command for adding actors and meshes to the assets archive.


//                // TODO: AddFilesToArchive maybe rename to "ImportAssets" and then we know that it's always to the AppMain.ModName?
//                //          ICommand2 command = new AddFilesToArchive(zipPath, sourceFiles, targetPaths);
//                //          command.BeginExecute(CommandCompleted);

//                return;


//                // import button is pressed within the context of a resource path in the Zip archive.
//                // TODO: if a resource of the same name already exists, user must be prompted if they wish to overwrite
//                // the existing, rename the import and any textures, or cancel the import  
//            }
//        }
//        #endregion


//        // http://stackoverflow.com/questions/29177/dodragdrop-and-mouseup
//        private void Gallery_OnDragEnter(object sender, DragEventArgs drgevent)
//        {
//            System.Diagnostics.Debug.WriteLine("galleryAsset.OnDragEnter() " + drgevent.ToString());
//            drgevent.Effect = DragDropEffects.None;

//            if (ribbonControl.SelectedRibbonTabItem != null)
//            {
//                RibbonTabItem tab = ribbonControl.SelectedRibbonTabItem;
//                if (tab == ribbonTabItemPrimitives)
//                {
//                    object o = drgevent.Data.ToString();
//                    KeyPlugins.DragDropContext node = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));
//                    // TODO: if this is not the correct node type, then
//                    if (node == null)
//                        return;
//                    else
//                    {
//                        // test if we are exactly over the tab and not on the ribbonbar
//                        Rectangle rect = tab.Panel.ClientRectangle;
//                        rect = tab.Panel.RectangleToScreen(rect);
//                        if (rect.Contains(Cursor.Position))
//                            // if the data is valid for drop, set the effect to the allowed effect
//                            drgevent.Effect = drgevent.AllowedEffect;
//                    }
//                }
//            }
//        }

//        private void Gallery_OnDragOver(object sender, DragEventArgs drgevent)
//        {
//            System.Diagnostics.Debug.WriteLine("galleryAsset.OnDragOver() " + drgevent.ToString());
//            KeyPlugins.DragDropContext node = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));

//            if (node != null && node.EntityID != null)
//            {
//                System.Diagnostics.Debug.WriteLine("galleryAsset.OnDragOver() " + drgevent.ToString());
//            }

//        }

//        private void Gallery_OnDragDrop(object sender, DragEventArgs drgevent)
//        {
//            System.Diagnostics.Debug.WriteLine("galleryAsset.OnDragDrop() " + drgevent.ToString());
//            KeyPlugins.DragDropContext node = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));

//            if (node != null && node.EntityID != null)
//            {
//                string prefabName = "";
//                DialogResult result = FormMain.InputBox("Enter a name for this new prefab.", "Please enter a new name for this prefab.", ref prefabName);

//                if (result == DialogResult.OK)
//                {
//                    // TODO: verify the name is ok
//                    if (System.IO.Path.GetExtension(prefabName) == "")
//                        prefabName += ".kgbentity";

//                    // finally issue command to store the prefab into the current mod database
//                    KeyCommon.Messages.Prefab_Save save = new KeyCommon.Messages.Prefab_Save();
//                    save.RelativeZipFilePath = AppMain.ModName;
//                    save.NodeID = node.EntityID;
//                    save.EntryPath = galleryAssets.Tag.ToString();
//                    save.EntryName = prefabName;

//                    AppMain.mNetClient.SendMessage(save);
//                }
//            }
//        }

//        private void Gallery_OnDragLeave(object sender, EventArgs e)
//        {
//            System.Diagnostics.Debug.WriteLine("galleryAsset.OnDragLeave() " + e.ToString());
//        }

//        private void ShowGalleryRightMouseClickMenu(ButtonItem sender, Control container, MouseEventArgs e)
//        {
//            ContextMenu menu = new ContextMenu();
//            MenuItem item = new MenuItem("Delete", buttonDeleteArchiveItem_Click);

//            // New -->  Folder
//            //          Script
//            //          Shader
//            //          Text File
//            //          
//            // Import --> Entity
//            // Export --> Extracts a copy
//            // Convert --> converts to a new extension within same folder
//            // --
//            // Cut
//            // Copy
//            // Paste (grays out if clipboard is empty)
//            // --
//            // Delete
//            // Rename
//            // Properties

//            item.Tag = sender;
//            menu.MenuItems.Add(item);

//            string senderName = sender.Name;
//            string ext = Path.GetExtension(senderName);

//            if (!string.IsNullOrEmpty(ext))
//            {
//                switch (ext.ToUpper())
//                {
//                    case ".FX":
//                    case ".CSS":

//                        item = new MenuItem("Edit", EditMenu_OnClick);
//                        item.Tag = sender;
//                        menu.MenuItems.Add(item);
//                        break;

//                    default:
//                        break;
//                }
//            }

//            menu.Show(container, e.Location);
//        }

//        private void assetGalleryItemButton_MouseDown(object sender, MouseEventArgs e)
//        {
//            EndDrag();

//            if (e.Button == MouseButtons.Left)
//                // when clicking on a folder, i dont want to autocollapse, but when clicking on an item, perhaps i do
//                QueryDrag(sender, e);
//            else
//            {
//                // show a popup menu
//                ShowGalleryRightMouseClickMenu((ButtonItem)sender, (Control)((ButtonItem)sender).ContainerControl, e);
//            }
//        }

//        private void assetGalleryItemButton_MouseUp(object sender, MouseEventArgs e)
//        {
//            if (e.Button == MouseButtons.Left)
//                EndDrag();
//        }


//        private void assetGalleryItemButton_Click(object sender, EventArgs e)
//        {
//            // note: this only fires on left mouse click.
//            EndDrag();

//            ButtonItem buttonItem = (ButtonItem)sender;
//            DevComponents.DotNetBar.Events.EventSourceArgs args = (DevComponents.DotNetBar.Events.EventSourceArgs)e;
//            string relativeZipFilePath = AppMain.ModName;

//            if (!bool.Parse((string)buttonItem.Tag)) // this is an item and not a folder
//            {
//                string pathInArchive = buttonItem.Name;
//                string ext = Path.GetExtension(pathInArchive);

//                switch (ext.ToUpper())
//                {
//                    case ".KGBENTITY":// represents a compressed archive (xmldb) within the overall xmldb
//                        FormPleaseWait waitDialog = new FormPleaseWait();
//                        waitDialog.Owner = this;
//                        waitDialog.Show(this);
//                        // is there a  way to load this tool in a seperate thread and then
//                        // 
//                        Keystone.EditTools.PlacementTool placer = new Keystone.EditTools.PlacementTool(
//                            AppMain.mNetClient,
//                        Keystone.Core.FullArchivePath(relativeZipFilePath), pathInArchive, null);
//                        waitDialog.Close();

//                        ((EditController)AppMain._core.CurrentIOController).CurrentTool = placer;


//                        // obsolete - because we want our CellPainter tool and PlaceObject
//                        // tools to send the commands to load and place objects
//                        //   KeyCommon.Messages.Prefab_Load msg = new KeyCommon.Messages.Prefab_Load();
//                        //   msg.PathInArchive = pathInArchive;
//                        //   msg.RelativeArchivePath = relativeZipFilePath;
//                        //   AppMain.mNetClient.SendMessage(msg);
//                        break;

//                    case ".X":
//                    case ".TVM":
//                    case ".OBJ":
//                    case ".TVA":
//                        ICommandProgress progress = new FormPleaseWait();

//                        //  ICommand2 command = new ImportGeometry(relativeZipFilePath, pathInArchive, true, true, false);
//                        //  command.BeginExecute(CommandCompleted, progress);

//                        //  ((FormPleaseWait)progress).ShowDialog(this);

//                        // interface ICommandProgress
//                        // {
//                        //      // the ImportAsset command itself can host the zip reading.  
//                        //      // and perhaps we can move the zip function out to the command itself
//                        //      // so that we can continue to pass the final string to the load function?
//                        //      // then inside the command we receive any zip update which can in turn
//                        //      // update the ICommandProgress where we iterate and if the Mesh loading has started
//                        //      // we can switch to that progress based on an estimate milliseconds it should take
//                        //      // based on the size of the file in bytes.
//                        //      // Then we can call ICommandProgress.Update()  to set the new values

//                        // }
//                        break;
//                    case ".CSS":
//                    case ".FX":
//                        break;

//                    default:
//                        break;
//                }


//            }
//        }

//        private void assetGalleryItemButton_MouseMove(object sender, MouseEventArgs e)
//        {
//            if (e.Button == MouseButtons.Left)
//            {
//                QueryDrag(sender, e);
//            }

//            // If the user drops the mouse over a valid viewport then we go through with the loading
//            // In fact what we can do is use a default transparent sphere marker to indcate where the object is being placed
//            // and hopefully it loads in the background quickly and the actual can be rendered in.
//            // THe other aspect is that it's up to the user to switch camera angles and camera position (i.e. inside of a ship instead outside)
//            // to place something easier.  They can switch to top down for instance.
//            // 


//        }

//        private void assetGalleryFolderButton_MouseDown(object sender, MouseEventArgs e)
//        {
//            if (e.Button == MouseButtons.Right)
//            {
//                // show a popup menu
//                ShowGalleryRightMouseClickMenu((ButtonItem)sender, (Control)((ButtonItem)sender).ContainerControl, e);
//            }
//        }

//        private void assetGalleryFolderButton_Click(object sender, EventArgs e)
//        {
//            ButtonItem buttonItem = (ButtonItem)sender;
//            DevComponents.DotNetBar.Events.EventSourceArgs args = (DevComponents.DotNetBar.Events.EventSourceArgs)e;

//            string zipFilePath = Path.Combine(AppMain._core.ModsPath, AppMain.ModName);


//            // if the last character is "/" this is a folder
//            if (bool.Parse((string)buttonItem.Tag))
//            {
//                if (buttonItem.Name.EndsWith(@"/"))
//                {
//                    string zipEntryName = Path.GetDirectoryName(buttonItem.Name);
//                    bool folder = bool.Parse((string)buttonItem.Tag);

//                    UpdateGallery(galleryAssets, zipFilePath, zipEntryName, true);
//                }
//                else
//                {
//                    UpdateGallery(galleryAssets, zipFilePath, buttonItem.Name, true);
//                }
//            }
//        }

//        //private void InitGallery()
//        //{
//        //    ribbonControl.AllowDrop = true;
//        //    ribbonControl.DragEnter += Gallery_OnDragEnter;
//        //    ribbonControl.DragDrop += Gallery_OnDragDrop;
//        //    ribbonControl.DragOver += Gallery_OnDragOver;
//        //    ribbonControl.DragLeave += Gallery_OnDragLeave;

//        //    galleryContainerPrefabBrowse.AutoCollapseOnClick = false;
//        //    string relativePath = "mods";
//        //    DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(AppMain.DATA_PATH, relativePath));
//        //    galleryContainerPrefabBrowse.Tag = relativePath;
//        //    PopulatePrefabGallery(dirInfo);
//        //}



//        //private void buttonItemPrefab_Click(object sender, EventArgs e)
//        //{

//        //    int length;
//        //    string relativePath;
//        //    string filename = "";

//        //    // update the relative path
//        //    if (((ButtonItem)sender).Name == "..")
//        //    {
//        //        length = galleryContainerPrefabBrowse.Tag.ToString().LastIndexOf("\\");
//        //        relativePath = galleryContainerPrefabBrowse.Tag.ToString().Substring(0, length);
//        //    }
//        //    else
//        //    {
//        //        length = ((ButtonItem)sender).Name.LastIndexOf("\\");
//        //        relativePath = galleryContainerPrefabBrowse.Tag.ToString() + "\\" + ((ButtonItem)sender).Name; // ((ButtonItem)sender).Name.Substring(0, length);

//        //        // TODO: below should be obsolete because now we just use full sender).Name above... verify all these paths look proper tho in the gallery at runtime
//        //        // TODO: actually it's not correct.  The browsing skips a directory each time you go down the hierarchy.  This is a bug
//        //        // with the concept itself of using the dirs as group names.  This could be a problem... i need to resolve this sooner rather
//        //        // than later to see if this gallery will be useful for prefab selection
//        //        //if (((ButtonItem)sender).Name.Length > length)
//        //        //{
//        //        //    Debug.WriteLine(((ButtonItem)sender).Name);
//        //        //  //  filename = ((ButtonItem)sender).Name.Substring(length + 1);
//        //        //}
//        //    }


//        //    string fullpath = Path.Combine(AppMain.DATA_PATH, relativePath);
//        //    fullpath = Path.Combine(fullpath, filename);
//        //    Debug.WriteLine(fullpath);
//        //    bool isFolder = bool.Parse(((ButtonItem)sender).Tag.ToString());

//        //    if (isFolder)
//        //    {
//        //        Cursor previous = Cursor.Current;
//        //        Cursor.Current = Cursors.WaitCursor;
//        //        galleryContainerPrefabBrowse.SuspendLayout = true;
//        //        galleryContainerPrefabBrowse.GalleryGroups.Clear();
//        //        galleryContainerPrefabBrowse.SubItems.Clear();

//        //        galleryContainerPrefabBrowse.Tag = relativePath;
//        //        PopulatePrefabGallery(new DirectoryInfo(fullpath));
//        //        galleryContainerPrefabBrowse.SuspendLayout = false;
//        //        galleryContainerPrefabBrowse.PopupGallery();
//        //        galleryContainerPrefabBrowse.PopupGallery();
//        //        Cursor.Current = previous;
//        //    }
//        //    else
//        //    {

//        //        if (!string.IsNullOrEmpty(fullpath))
//        //        {

//        //            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
//        //            byte[] value = encoding.GetBytes(fullpath);

//        //            // TODO: no world coordinate is specified
//        //            AppMain.PluginService.SendMessage(AppMain._core.Viewports["viewport0"].Context.Region.ID, 9, value);
//        //        }
//        //    }
//        //}

//        //private void PopulatePrefabGallery(DirectoryInfo dirInfo)
//        //{
//        //    List<DirectoryInfo> childDirs = new List<DirectoryInfo>();
//        //    List<FileInfo> childFiles = new List<FileInfo>();
//        //    List<ButtonItem> buttons = new List<ButtonItem>();
//        //    GalleryGroup newGroup;
//        //    int groupIndex = 0;
//        //    DevComponents.DotNetBar.ButtonItem buttonItem;

//        //    int thumbWidth = 64;
//        //    int thumbHeight = 48;
//        //    bool isFolder = true;

//        //    string currrentlyBrowsingPath = galleryContainerPrefabBrowse.Tag.ToString();
//        //    int length = currrentlyBrowsingPath.LastIndexOf("\\");

//        //    // if this is not the root we add a group titled "back to \mods"
//        //    // and an icon named ".."
//        //    DirectoryInfo tmp = new DirectoryInfo(Path.Combine(AppMain.DATA_PATH, "mods"));
//        //    if (dirInfo.FullName != tmp.FullName)
//        //    {
//        //        string backToPath = currrentlyBrowsingPath.Substring(0, length);

//        //        newGroup = CreateGalleryGroup("galleryGroup_" + groupIndex.ToString(),
//        //            groupIndex,
//        //            string.Format("<b>{0}</b>", "back to \\" + backToPath));

//        //        galleryContainerPrefabBrowse.GalleryGroups.Add(newGroup);
//        //        groupIndex++;

//        //        buttonItem = CreateGalleryButtonItem("..", "..", "back to parent folder", isFolder.ToString(),
//        //           Path.Combine(AppMain.DATA_PATH, "editor\\icons\\prev_folder.png"), thumbWidth, thumbHeight,
//        //            true);
//        //        buttonItem.Click += buttonItemPrefab_Click;

//        //        galleryContainerPrefabBrowse.SubItems.Add(buttonItem);
//        //        galleryContainerPrefabBrowse.SetGalleryGroup(buttonItem, newGroup);
//        //    }

//        //    foreach (DirectoryInfo info in dirInfo.GetDirectories())
//        //    {
//        //        // add each directory as a galleryGroup 
//        //        newGroup = CreateGalleryGroup("galleryGroup_" + groupIndex.ToString(),
//        //            groupIndex,
//        //            string.Format("<b>{0}</b>", galleryContainerPrefabBrowse.Tag.ToString() + "\\" + info.Name));

//        //        galleryContainerPrefabBrowse.GalleryGroups.Add(newGroup);
//        //        groupIndex++;

//        //        isFolder = true;
//        //        // add the child directories of this current directory as a button Item using a folder icon
//        //        foreach (DirectoryInfo childDir in info.GetDirectories())
//        //        {
//        //            buttonItem = CreateGalleryButtonItem(info.Name + "\\" + childDir.Name,
//        //               childDir.Name, childDir.Name, isFolder.ToString(),
//        //           Path.Combine(AppMain.DATA_PATH, "editor\\icons\\folder.jpg"), thumbWidth, thumbHeight,
//        //           true);
//        //            buttonItem.Click += buttonItemPrefab_Click;
//        //            buttons.Add(buttonItem);
//        //        }

//        //        isFolder = false;
//        //        // add the child files of this directory as items in this gallerygroup using their default icon if available, else the default "no image" icon
//        //        foreach (FileInfo childFile in info.GetFiles())
//        //        {
//        //            buttonItem = CreateGalleryButtonItem(info.Name + "\\" + childFile.Name,
//        //               childFile.Name, childFile.Name, isFolder.ToString(),
//        //           @"E:\dev\c#\KeystoneGameBlocks\Data\pool\prefabs\vehicles\sagitarius.prefab.png",
//        //           thumbWidth, thumbHeight,
//        //           true);
//        //            buttonItem.Click += buttonItemPrefab_Click;
//        //            buttons.Add(buttonItem);
//        //        }

//        //        // add all these buttons to the current galleryGroup then resume to next galleryGroup
//        //        galleryContainerPrefabBrowse.SubItems.AddRange(buttons.ToArray());

//        //        // assign all these buttons to the current group
//        //        foreach (ButtonItem currentButton in buttons)
//        //            galleryContainerPrefabBrowse.SetGalleryGroup(currentButton, newGroup);

//        //        buttons.Clear();
//        //    }

//        //    galleryContainerPrefabBrowse.SuspendLayout = true;
//        //}


//        /// <summary>
//        /// Handles galleries for assets, prefabs, etc
//        /// </summary>
//        /// <param name="gallery"></param>
//        private void UpdateGallery(GalleryContainer gallery, string zipFilePath, string zipEntryName, bool isFolder)
//        {
//            //Keystone.IO.SceneArchiver archiver;  // archiver is for xmldb, not generic archive manipulation so we use DotNetZip

//            if (File.Exists(zipFilePath))
//            {
//                int groupIndex = 0;
//                ZipFile zip = null;
//                Cursor previous = Cursor.Current;
//                Cursor.Current = Cursors.WaitCursor;
//                int thumbWidth = 64;
//                int thumbHeight = 48;

//                try
//                {
//                    gallery.SuspendLayout = true;
//                    gallery.GalleryGroups.Clear();
//                    gallery.SubItems.Clear();

//                    // don't call PopupGalleryItems.Clear().  This actually removes the bottom buttons like "Import Mesh.." or "Search Online..."
//                    // gallery.PopupGalleryItems.Clear(); 

//                    zip = new ZipFile(zipFilePath);
//                    if (zip != null)
//                    {
//                        // dir seperators  in the search param use DOS style backslashes NOT forward slashes.
//                        string directoryPathInArchive = zipEntryName;
//                        buttonItemImport.Tag = zipFilePath + "," + directoryPathInArchive;
//                        // first child directories are ones that contain one "/" located as last character in string

//                        // http://wyupdate.googlecode.com/svn/trunk/Zip/FileSelector.cs
//                        //No slash in the pattern implicitly means recurse, which means compare to
//                        // filename only, not full path.
//                        // "type = D"  returns only folders  
//                        // "type = F" returns only files
//                        // "*\\" returns all directories (recursive included) and no files
//                        string searchParam = "*\\";
//                        string caption = string.Format("<b>{0}</b>", "folders in " + zipEntryName);

//                        GalleryGroup newGroup = CreateGalleryGroup("galleryGroup_" + groupIndex.ToString(), groupIndex, caption);
//                        gallery.GalleryGroups.Add(newGroup);
//                        groupIndex++;

//                        // add up one directory button
//                        if (!string.IsNullOrEmpty(zipEntryName))
//                        {
//                            ButtonItem buttonItem = CreateGalleryButtonItem(Path.GetDirectoryName(zipEntryName),
//                                       "..", "back to parent folder", isFolder.ToString(),
//                                   Path.Combine(AppMain.DATA_PATH, "editor\\icons\\prev_folder.png"),
//                                   thumbWidth, thumbHeight,
//                                   true);

//                            buttonItem.Click += assetGalleryFolderButton_Click;
//                            buttonItem.CanCustomize = false; // don't want our right mouse click replaced with customize menu popup
//                            gallery.SubItems.Add(buttonItem);
//                            gallery.SetGalleryGroup(buttonItem, newGroup);
//                        }

//                        gallery.Tag = directoryPathInArchive;

//                        // and rest of directories
//                        PopulateGalleryGroup(newGroup, zip, directoryPathInArchive,
//                            gallery, thumbWidth, thumbHeight, assetGalleryFolderButton_Click, assetGalleryFolderButton_MouseDown, null, null, true);


//                        // select and add files
//                        searchParam = "*.*";
//                        caption = string.Format("<b>{0}</b>", "files in " + zipEntryName);

//                        newGroup = CreateGalleryGroup("galleryGroup_" + groupIndex.ToString(), groupIndex, caption);
//                        gallery.GalleryGroups.Add(newGroup);

//                        PopulateGalleryGroup(newGroup, zip, directoryPathInArchive,
//                            gallery, thumbWidth, thumbHeight, assetGalleryItemButton_Click, assetGalleryItemButton_MouseDown,
//                            assetGalleryItemButton_MouseMove, assetGalleryItemButton_MouseUp, false);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Debug.WriteLine(ex.Message);
//                }
//                finally
//                {
//                    if (zip != null) zip.Dispose();
//                    gallery.SuspendLayout = false;
//                    gallery.PopupGallery();
//                    Cursor.Current = previous;
//                }
//            }
//        }

//        // //traverseDirectoryReparsePoints is only for reading from the ntfs reparse mount points and such (logical folders)
//        private GalleryGroup PopulateGalleryGroup(GalleryGroup newGroup, ZipFile zip, string directoryPathInArchive,
//            GalleryContainer gallery, int thumbWidth, int thumbHeight,
//            EventHandler buttonClickHandler, MouseEventHandler mouseDownHandler, MouseEventHandler mouseMoveHandler, MouseEventHandler mouseUpHandler,
//            bool selectDirectories)
//        {
//            List<ButtonItem> buttons = new List<ButtonItem>();
//            string[] entries;

//            if (selectDirectories)
//                entries = KeyCommon.IO.ArchiveIOHelper.SelectDirectories(zip, directoryPathInArchive);
//            else
//                entries = KeyCommon.IO.ArchiveIOHelper.SelectFiles(zip, directoryPathInArchive);


//            foreach (string entry in entries)
//            {
//                string iconPath = "";
//                if (selectDirectories)
//                {
//                    iconPath = Path.Combine(AppMain.DATA_PATH, "editor\\icons\\folder.jpg");
//                }
//                else
//                {
//                    // TODO: if one of our standard icons for a type is NOT found, use default embedded resource icon.  
//                    //          But for .prefab rendered icons, if they are not available use the default "no image available" icon
//                    //  Thus for modding, we should always first look for icon on disk rather than embedded default
//                    // iconPath = @"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\prefabs\vehicles\sagitarius.prefab.png";
//                    if (!selectDirectories) // if it's a file
//                    {
//                        string extension = Path.GetExtension(entry);
//                        switch (extension.ToUpper())
//                        {
//                            case ".CSS":
//                                iconPath = Path.Combine(AppMain.DATA_PATH, "editor\\icons\\csscript128.jpg");
//                                break;
//                            case ".X":
//                            case ".OBJ":
//                            case ".TVM":
//                                iconPath = Path.Combine(AppMain.DATA_PATH, "editor\\icons\\mesh.png");
//                                break;
//                            default:
//                                iconPath = Path.Combine(AppMain.DATA_PATH, "editor\\icons\\unknown.png");
//                                break;
//                        }
//                    }
//                }

//                // add each entry as a child to the current group
//                string entryName = entry;
//                entryName = entryName.Replace("/", @"\");
//                if (selectDirectories)
//                {
//                    // remove any trailing "\" or "/"
//                    if (entryName.EndsWith("\\"))
//                        entryName = entryName.Substring(0, entryName.Length - 1);

//                    // now find the last "\" if any which will indicate this is a child dir under other folders
//                    int index = entryName.LastIndexOf("\\");

//                    if (index >= 0)
//                        entryName = entryName.Substring(index + 1, entryName.Length - index - 1);
//                }
//                else
//                {
//                    entryName = Path.GetFileName(entry);
//                }

//                DevComponents.DotNetBar.ButtonItem buttonItem = CreateGalleryButtonItem(entry,
//                   entryName, entry, selectDirectories.ToString(),
//               iconPath, thumbWidth, thumbHeight, true);
//                buttonItem.CanCustomize = false; // don't want our right mouse click replaced with customize menu popup
//                buttonItem.Click += buttonClickHandler;
//                buttonItem.MouseDown += mouseDownHandler;
//                buttonItem.MouseMove += mouseMoveHandler;
//                buttonItem.MouseUp += mouseUpHandler;

//                buttons.Add(buttonItem);
//            }

//            if (buttons != null && buttons.Count > 0)
//            {
//                gallery.SubItems.AddRange(buttons.ToArray());

//                // assign all these buttons to the current group
//                foreach (ButtonItem currentButton in buttons)
//                    gallery.SetGalleryGroup(currentButton, newGroup);

//                buttons.Clear();
//            }

//            return newGroup;
//        }




//        private ButtonItem CreateGalleryButtonItem(string name, string text, string tooltip, string tag,
//            string imagePath, int imageWidth, int imageHeight,
//            bool autoCollapse)
//        {
//            string iconPath;
//            Image fullSizeImage = null;

//            DevComponents.DotNetBar.ButtonItem buttonItem =
//                new DevComponents.DotNetBar.ButtonItem(name);

//            buttonItem.Text = text;
//            buttonItem.Tooltip = tooltip;
//            buttonItem.Tag = tag;

//            buttonItem.ButtonStyle = eButtonStyle.ImageAndText;
//            buttonItem.ImagePosition = eImagePosition.Top;

//            buttonItem.Size = new Size(imageWidth, imageHeight);
//            buttonItem.ImageFixedSize = new Size(imageWidth, imageHeight);
//            buttonItem.AutoCollapseOnClick = autoCollapse; // oddly, this has to be enabled on the buttons and not the gallery itself for the popup to stay activated

//            iconPath = imagePath;

//            FileStream fs = null;
//            try
//            {
//                // incase the thumbnails are too large for some reason we'll make sure they are scaled down
//                //fullSizeImage = Image.FromFile(iconPath);
//                //buttonItem.Image = fullSizeImage.GetThumbnailImage(imageWidth, imageHeight, null, IntPtr.Zero);

//                fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
//                fullSizeImage = Image.FromStream(fs, true, true);
//                buttonItem.Image = fullSizeImage.GetThumbnailImage(imageWidth, imageHeight, null, IntPtr.Zero);
//            }
//            catch (Exception ex)
//            {
//                // TODO: load a placeholder image instead
//                // probably out of memory exception if the image is too large
//            }
//            finally
//            {
//                if (fullSizeImage != null)
//                    fullSizeImage.Dispose();

//                if (fs != null)
//                {
//                    fs.Close();
//                    fs.Dispose();
//                }
//            }
//            return buttonItem;
//        }




//        private GalleryGroup CreateGalleryGroup(string name, int displayOrderIndex, string text)
//        {
//            GalleryGroup newGroup = new GalleryGroup();
//            newGroup.Name = name;
//            newGroup.DisplayOrder = displayOrderIndex;
//            newGroup.Text = text;

//            return newGroup;
//        }

//    }
//}
