using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Keystone.Types;

namespace KeyEdit
{
    public partial class FormPreview : Form
    {
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_SETFOCUS = 0x0007;
        private const int WM_KILLFOCUS = 0x0008;
        private const int WA_INACTIVE = 0; // Deactivated
        private const int WA_ACTIVE = 1; // Activated by some method other than a mouse click (for example, by a call to the SetActiveWindow function or by use of the keyboard interface to select the window).
        private const int WA_CLICKACTIVE = 2; // Activated by a mouse click.

        private string mModPath;
        private string mEntryPath;

        private Keystone.Entities.Entity mPreviewEntity;


        public FormPreview() 
        {
            InitializeComponent();         	
        }
        
        public FormPreview(string modPath, string entryPath) : this()
        {
            mModPath = modPath;
            mEntryPath = entryPath;
        }

        public string ModPath
        {
            get {return mModPath;}
            set {mModPath = value;}
        }
        
        public string EntryPath
        {
            get {return mEntryPath;}
            set {mEntryPath = value;}
        }
        
        public Keystone.Entities.Entity TargetEntity 
        { 
            get { return mPreviewEntity; } 
            set { mPreviewEntity = value; } 
        }
                
                
//        protected override void WndProc(ref Message m)
//        {
//            if (m.Msg == WM_SETFOCUS)
//            {
//                AppMain.ApplicationHasFocus = true;
//                System.Diagnostics.Debug.WriteLine("AppMain.AppLoop() - GAINED FOCUS");
//            }
//            else if (m.Msg == WM_KILLFOCUS)
//            {
//                AppMain.ApplicationHasFocus = false;
//                System.Diagnostics.Debug.WriteLine("AppMain.AppLoop() - LOST FOCUS");
//            }
//            else if (m.Msg == WM_ACTIVATE)
//            {
//                if (AppMain.LowWord((int)m.WParam) != WA_INACTIVE)
//                {
//                    AppMain.ApplicationHasFocus = true;
//                    System.Diagnostics.Debug.WriteLine("AppMain.AppLoop() - GAINED FOCUS");
//                }
//                else
//                {
//                    AppMain.ApplicationHasFocus = false;
//                    System.Diagnostics.Debug.WriteLine("AppMain.AppLoop() - LOST FOCUS");
//                }
//            }
//
//            base.WndProc(ref m);
//        
//        }

		protected override void OnMove(EventArgs e)
		{
			base.OnMove(e);
			AppMain.Form.WorkspaceManager.Resize ();
		}
		protected override void OnActivated(EventArgs e)
		{
			
			//base.OnActivated(e);
			AppMain.ApplicationHasFocus = true;
			System.Diagnostics.Debug.WriteLine("FormPreview.OnActivated() - GAINED FOCUS");
		} 
		
		protected override void OnDeactivate(EventArgs e)
		{
			//base.OnDeactivate(e);
			AppMain.ApplicationHasFocus = false;
			System.Diagnostics.Debug.WriteLine("FormPreview.OnDeactivate() - LOST FOCUS");
		} 
		


        private void buttonSave_Click(object sender, EventArgs e)
        {
        	System.Diagnostics.Debug.WriteLine ("FormPreview.buttonSave_Click()");
        	Save(mModPath, mEntryPath, OnScreenShot_Saved);
        }

        public class ScreenShotEventArgs : EventArgs
        {
            public string FullPath;
            public EventHandler ScreenShotSaveCompleted;

            public ScreenShotEventArgs(string fullpath, EventHandler completedHandler)
            {
                FullPath = fullpath;
                ScreenShotSaveCompleted = completedHandler;
            }
        }

        public event EventHandler<ScreenShotEventArgs> TakeScreenShot;

        public void Save(string archiveFullPath, string pathInArchive, EventHandler screenshotSaveCompleted)
        {
            buttonSave.Visible = false;
            Refresh();
            mModPath = archiveFullPath;
            mEntryPath = pathInArchive;
            string screenshotFile  = System.IO.Path.Combine (AppMain.DATA_PATH, @"previews\kgb_screen_" + DateTime.Now.ToFileTime() + ".png");
            
            if (TakeScreenShot != null)
            	// invokes RenderingContext.Screenshot() 
                TakeScreenShot(this, new ScreenShotEventArgs(screenshotFile, OnScreenShot_Saved));

        }

        private void OnScreenShot_Saved(object sender, EventArgs args)
        {
            buttonSave.Visible = true;
            string imageFile = sender as string;
            if (string.IsNullOrEmpty(imageFile) || System.IO.File.Exists(imageFile) == false)
            {
                // TODO: maybe there is a lag here between this callback and actual
                // file... i dont understand it really.  
                // actually in fact, the buttonSave is visible sometimes when it shouldnt which
                // indicates that TV is returning save before it has actually saved...
                MessageBox.Show("Preview image generation failed.");
                return;
            }

            // get a stream to the .kgbentity archive within it's parent xmldb archive
            System.IO.Stream entityStream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(mEntryPath, "", mModPath);
            Ionic.Zip.ZipFile entityZip = Ionic.Zip.ZipFile.Read(entityStream);
            entityZip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
            DateTime timeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);

            // add the temp image as "preview.png" 
            Ionic.Zip.ZipEntry e;

            if (entityZip.ContainsEntry("preview.png"))
            {
                // (overwrite any existing)
                //e = entityZip.UpdateEntry("preview.png", imageStream);
                e = entityZip.UpdateFile(imageFile, "preview.png");
            }
            else
            {
                //e = entityZip.AddEntry("preview.png", imageStream);
                e = entityZip.AddFile(imageFile);
                e.FileName = "preview.png"; // e.FileName always represents full entryname path from root.  So here obviously preview.png is going right off of root dir in archive.
            }
            
            e.LastModified = timeStamp;


            try
            {
	            // store the .kgbentity back into the mod archive or mod folder 
	            // (overwrite the existing)
	            if (mModPath.EndsWith (".zip"))
	            {
	                string tmpfile = Keystone.IO.XMLDatabase.GetTempFileName();
                    // NOTE: zip will throw exception if file already exists?? or if path doesnt exist??
                    entityZip.Save(tmpfile);
	            
		            Ionic.Zip.ZipFile xmldb = Ionic.Zip.ZipFile.Read(mModPath);
		            //xmldb.UpdateEntry(pathInArchive, entityStream);
		            // TODO: it will use the original file name and treat pathInArchive as a directory
		            string entryPathWithoutFilename = KeyCommon.IO.ArchiveIOHelper.TidyZipEntryName(System.IO.Path.GetDirectoryName(mEntryPath));
		            string entryFileNameWithoutPath = System.IO.Path.GetFileName(mEntryPath);
		            string entryFullPathAndName = System.IO.Path.Combine (entryPathWithoutFilename, entryFileNameWithoutPath);
		            if (xmldb.ContainsEntry(entryFullPathAndName))
		            {
		                xmldb.RemoveEntry(mEntryPath);
		            }
		            else throw new Exception("FormPreview.OnScreenShot() - Should always exist.");
		
		            e = xmldb.AddFile(tmpfile, entryPathWithoutFilename);
		            // NOTE: when using e.FileName for rename must actually assign the full EntryName including path.
		            e.FileName = entryFullPathAndName;
		            e.LastModified = timeStamp;
		            // TODO: save start should notify status bar
		            xmldb.Save();
	                // TODO: save complete should notify status bar
	            	System.Diagnostics.Debug.WriteLine ("FormPreview.OnScreenShot() - Successfully saved '" + entryFullPathAndName + "' to " + mModPath);
	            }
	            else 
	            {
	            	// NOTE: zip will throw exception if file already exists?? or if path doesnt exist.  This means a 0 byte file should be created
	            	// Also, interestingly, entityZip.Save() cannot be called twice with different paths?! wtf?
	            	//string testpath = @"E:\dev\c#\KeystoneGameBlocks\Data\mods\caesar\actors\testsave.kgbentity";
	            	//System.IO.File.Delete (testpath);
	            	//System.IO.FileStream fs = System.IO.File.Create (testpath);
	            	//fs.Close();
	            	//fs.Dispose();
	            	//entityZip.Save (testpath); // (mArchiveEntry);
	            	string fullPath = System.IO.Path.Combine (mModPath, mEntryPath);
	            	entityZip.Save (fullPath);
	            	// TODO: save complete should notify status bar
	            	System.Diagnostics.Debug.WriteLine ("FormPreview.OnScreenShot() - Successfully saved '" + mEntryPath);
	            }
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("FormPreview.OnScreenShot() - ERROR: Save Failed '" + ex.Message);
            }
            finally
            {
	            // stream must remain open until we have saved the entity
	            entityStream.Close();
	            entityStream.Dispose();
            	entityZip.Dispose();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose(true);
        }

    }
}
