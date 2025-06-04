using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ImageBrowserDotnet;
using Ionic.Zip;

namespace KeyEdit.GUI
{
    /// <summary>
    /// Dialog is used for "Save As" path and file selection as well as
    /// "Open" for changing resources like textures.
    /// </summary>
    public partial class AssetBrowserDialog : Form
    {
        private bool mIsSaveAsDialog; 

        /// <summary>
        /// A GUI for managing assets in mod databases. (aka zip archives)
        /// This GUI is part of KeyPlugins project because we want it to be available
        /// for utilities that are offline and which cannot depend on existance of 
        /// mHost plugin scripting interface to be active.
        /// </summary>
        public AssetBrowserDialog(string modPath, string relativeArchivePath) : this (modPath, relativeArchivePath, null)
        {
        }

        public AssetBrowserDialog(string modPath, string relativeArchivePath, string[] allowedExtensions)
        {
            InitializeComponent();

            assetBrowserControl.ShowTreeView = true;
            assetBrowserControl.AllowedExtensions = allowedExtensions;
            assetBrowserControl.RecurseSubFolders = false; // we do not want the imagebrowser recursing!
            assetBrowserControl.ImageSize = 64;

            // TODO: this should not assume a zip archive, but a modname and if it ends in .zip then it's an archive
            //       otherwise it's a folder
            
            // ModPath needs to be set after recurse = false or it will immediately start
            // searching before we have a chance to set .RecurseSubFolders = false
            assetBrowserControl.BindToMod (modPath, relativeArchivePath);

           // assetBrowserControl.BindToCustom ();
            
            assetBrowserControl.OnAssetClicked += AssetSelected;
            assetBrowserControl.QueryCustomImageForNonImage += Workspaces.EditorWorkspace.OnNonImageFileFound;
            //assetBrowserControl.tree.LabelEdit = false; // we only allow label edit manually

        }

        public string ModName { get { return assetBrowserControl.ModName; } }

        public string ModDBSelectedEntry 
        { 
            get { return assetBrowserControl.SelecteFile; }
            set { assetBrowserControl.SelecteFile = value; }
        }


        #region Filter button handlers
        #endregion

        public DialogResult ShowOpenDialog()
        {
            // hide the toolbar
            assetBrowserControl.ToolbarVisible = false;

            mIsSaveAsDialog = false;

            labelFilename.Hide();
            textFilename.Hide();

            return base.ShowDialog();
        }

        public DialogResult ShowSaveAsDialog()
        {

            // hide the toolbar
            assetBrowserControl.ToolbarVisible = false;

            mIsSaveAsDialog = true;

            labelFilename.Show();
            textFilename.Show();

            return base.ShowDialog();
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            // TODO: the error here is, when manually modifying the textFilename.Text
            // it will no longer match the image.caption of the selected image
            // but yet, that selected image is what we're using to come up with the final selection
            //
            // the proper fix is to update a SelectedFile as well as have the SelectedPath
            // and then combine the two.
            if (string.IsNullOrEmpty (assetBrowserControl.SelectedPath) ||
                string.IsNullOrEmpty (assetBrowserControl.SelecteFile)) 
            	return;
            
            string entry = System.IO.Path.Combine (assetBrowserControl.SelectedPath, assetBrowserControl.SelecteFile);
            string extension = System.IO.Path.GetExtension(entry);
            if (string.IsNullOrEmpty(extension))
            {
                entry += ".kgbentity";
            }
            if (mIsSaveAsDialog)
            {
                if (IsValidFilename(entry))
                {
                    // note: since filename will be merged with selectedpath
                    // the filename itself should  not contain any part of a path
                    // so no ":" or no "/" or "\" for instance
                    // and should not end in "."  since that's not a valid end extension
                    
                    // TODO: test for overwrite of existing entry or file.
                    // if is a zip archive mod, otherwise it's an HDD mod folder
                    
                    string fullModPath = System.IO.Path.Combine(AppMain.MOD_PATH, ModName.ToLower());
                    bool exists = false;
                    // test for overwrite of existing entry.
                    if (fullModPath.EndsWith (".zip"))
                    {
	                    if (KeyCommon.IO.ArchiveIOHelper.EntryExists(fullModPath, entry))
	                        exists = true;
                    }
                    else // test for overwrite of existing file
                    {
                    	// entry will already contain mod name so find filename using MOD_PATH
                    	string filename = Path.Combine (AppMain.MOD_PATH, entry);
                    	if (File.Exists (filename))
                    		exists = true;
                    }
                    
                    if (exists)
                    {
                    	// prompt overwrite
                        DialogResult result = MessageBox.Show("A file with this same name already exists.  Do you wish to overwrite it?", "Confirm overwrite existing file?", MessageBoxButtons.YesNo);
                        if (result != DialogResult.Yes)
                        {
                            this.DialogResult = DialogResult.Cancel;
                            return; // return without closing the SaveAs asset browser
                        }
                    }
                    // so here rather than get the mSelectedEntry from the mImageBrowser, we get it
                    // from the selected node path in the tree and the user entered filename
                    ModDBSelectedEntry = entry;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                    return; // return without closing the SaveAs asset browser
                }
            }
            else
            {
                // verify that some file is selected by the open dialog
                ModDBSelectedEntry = entry;
            }

            this.DialogResult = DialogResult.OK;

            this.Close(); // all is good, return and close
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        bool IsValidFilename(string testName) 
        { 
            string regexString = "[" + System.Text.RegularExpressions.Regex.Escape(Path.GetInvalidPathChars().ToString()) + "]"; 
            System.Text.RegularExpressions.Regex containsABadCharacter = new System.Text.RegularExpressions.Regex(regexString); 
         
            if (containsABadCharacter.IsMatch(testName) )
            { 
                return false; 
            } 
         
            // Check for drive 
            string pathRoot = Path.GetPathRoot(testName); 
            //if (Directory.GetLogicalDrives().Contains(pathRoot)) 
            //{ 
            //    // etc 
            //} 
         
            // other checks for UNC, drive-path format, etc 
         
            return true; 
        }



        private void AssetSelected(object sender, KeyEdit.GUI.AssetClickedEventArgs e)
        {
            switch (e.MouseEventArgs.Button)
            {
                case MouseButtons.Right:
                    //AssetRightMouseClick(sender, e);
                    break;
                case MouseButtons.Left:
                    AssetLeftMouseClick(sender, e);
                    break;
                case MouseButtons.Middle:
                default:
                    break;
            }
        }

        private void AssetLeftMouseClick(object sender, KeyEdit.GUI.AssetClickedEventArgs e)
        {
        	if (string.IsNullOrEmpty (e.AssetName)) return;
            string assetName = e.AssetName;
            string path = System.IO.Path.Combine (e.ModFolder, e.AssetName);

            // TODO: if clicking folder, we only need "e.AssetName" but if clicking image on right panel, i think this breaks
            textFilename.Text = e.AssetName; //path;
        }

        private void textFilename_TextChanged(object sender, EventArgs e)
        {
            if (sender == textFilename)
                this.assetBrowserControl.SelecteFile = textFilename.Text;
        }
    }
}
