using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Ionic.Zip;
using ImageBrowserDotnet;

namespace ThumbnailViewerDemo
{
    public partial class MainForm : Form
    {
        private ImageDialog m_ImageDialog;
        private string mZipFilePath = @"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\resources.zip";
        
        
        public MainForm()
        {
            InitializeComponent();

            this.buttonCancel.Enabled = false;

            

    //        ZipFile zip = new ZipFile(mZipFilePath);
    //        mImageBrowser.Initialize(mZipFilePath);
    //        this.fileSystemTreeView1.Load(zip, ""); // start browsing at root path which is ""

            string path = @"c:\";
            mImageBrowser.Initialize();
            mImageBrowser.RecurseSubFolders = true;
            this.fileSystemTreeView1.ShowFiles = false;
            this.fileSystemTreeView1.Load(path);

            // create the large image preview dialog
            m_ImageDialog = new ImageDialog();


            
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.mImageBrowser.CancelBrowsing();
        }

       
        private void trackBarSize_ValueChanged(object sender, EventArgs e)
        {
            mImageBrowser.ImageSize = (64 * (this.trackBarSize.Value + 1));   
        }

        private void fileSystemTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && !string.IsNullOrEmpty (e.Node.FullPath) )
            {
                this.mImageBrowser.CancelBrowsing();

                this.buttonCancel.Enabled = true;

                mImageBrowser.BrowseFolder(e.Node.FullPath, true);
            }
        }


        #region ImageBrowser Event Handlers

        private void mImageBrowser_OnBrowsingAborted(object sender, ImageBrowserEventArgs e)
        {

        }

        private void mImageBrowser_OnBrowsingCanceled(object sender, ImageBrowserEventArgs e)
        {
            this.buttonCancel.Enabled = false;
        }

        private void mImageBrowser_OnBrowsingEnded(object sender, ImageBrowserEventArgs e)
        {
            this.buttonCancel.Enabled = false;
        }

        private void mImageBrowser_OnBrowsingFoundImage(object sender, ImageBrowserEventArgs e)
        {

        }

        private void mImageBrowser_OnBrowsingStarted(object sender, ImageBrowserEventArgs e)
        {

        }

        private void mImageBrowser_OnImageClicked(object sender, ImageBrowserEventArgs e)
        {
            ImageViewer viewer = (ImageViewer)sender;
            if (m_ImageDialog.IsDisposed) m_ImageDialog = new ImageDialog();
            if (!m_ImageDialog.Visible) m_ImageDialog.Show();

            m_ImageDialog.SetImage(viewer.ImageLocation);
        }
        #endregion
    }


}