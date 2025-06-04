using System;
using System.Drawing;
using System.Threading;
using System.IO;
using Ionic.Zip;

namespace ImageBrowserDotnet
{
    /// <summary>
    /// Class can be inherited by calling application to create novel
    /// Controllers for walking/displaying various types of content. (not just image thumbnails)
    /// </summary>
    public class BrowserController
    {
        protected string mModsPath;
		protected string mModName;
                
        public event ImageBrowserEventHandler OnStart;
        public event ImageBrowserEventHandler OnAdd;
        public event ImageBrowserEventHandler OnEnd;

        protected bool m_CancelScanning;
        protected string[] mAllowedFileTypes;  
        protected string[] mFolderFilter;    // this should be EXCLUDED folders
        protected bool mRecurseSubFolders;

        protected Thread mThread;
        private static readonly object mCancelScanningLock = new object();



        public BrowserController(string modsPath, string modName)
        {
        	mModsPath = modsPath;
        	mModName = modName;
            mRecurseSubFolders = true;
        }

        public bool RecurseSubFolders
        {
            get { return mRecurseSubFolders; }
            set { mRecurseSubFolders = value; }
        }

        public bool CancelScanning
        {
            get
            {
                lock (mCancelScanningLock)
                {
                    return m_CancelScanning;
                }
            }
            set
            {
                lock (mCancelScanningLock)
                {
                    m_CancelScanning = value;

                    if (m_CancelScanning == true)
                    {
                        if (mThread != null)
                        {
                            if (mThread.IsAlive)
                            {
                                // Put the Main thread to sleep for 1 millisecond to allow oThread
                                // to do some work:
                                Thread.Sleep(1);

                                // Request that oThread be stopped
                                mThread.Abort();

                                // Wait until oThread finishes. Join also has overloads
                                // that take a millisecond interval or a TimeSpan object.
                                mThread.Join();
                                mThread = null;
                            }
                        }
                    }
                }
            }
        }


        public void BrowseFolder(string folderPath)
        {
            BrowseFolder(folderPath, null);
        }

        public void BrowseFolder(string folderPath, string[] allowedExtensions)
        {
            mAllowedFileTypes = allowedExtensions;
            CancelScanning = false;
            mThread = new Thread(new ParameterizedThreadStart(AddFolder));
            mThread.IsBackground = true;
            
            mThread.Start(folderPath);
        }
                
        protected virtual void AddFolder(object folderPath)
        {
            if (folderPath == null) return;

            string path = (string)folderPath;
            if (path.StartsWith("\\"))
                path = path.Remove(0, 1);

        	path = Path.Combine (mModName, path);
            
            if (this.OnStart != null)
            {
                this.OnStart(this, new ImageBrowserEventArgs(null, null));
            }

            this.AddFolderIntern(path); // <--
            
            
            if (this.OnEnd != null)
            {
                this.OnEnd(this, new ImageBrowserEventArgs(null, null));
            }

            CancelScanning = false;
        }

        protected virtual void AddFolderIntern(string folderPath)
        {
            // todo: i should prevent this function from being executed by more than one thread at a time... see if that fixes the double entry bug
            if (m_CancelScanning) mThread.Abort();

            string[] files = null;
            // not using AllDirectories
            try
            {
                // catch exception for trying to access any secure files
                string fullPath = System.IO.Path.Combine (mModsPath, folderPath);
                files = Directory.GetFiles(fullPath);
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("ImageBrowserDotNet.BrowserController.AddFolderIntern() - " + ex.Message);
            }

            if (files != null && files.Length > 0)
            {
                foreach (string file in files)
                {
                    if (m_CancelScanning) mThread.Abort();
                    if (IsFileTypeFiltered(file)) continue;
                    Image img = null;

                    if (IsValidImageFormat (file))
                    {
                        try
                        {
                            // TODO: in a custom thumbnailController, we can 
                            // use an alt image for certain file types.
                            // The surrogate images should be provided along with the list
                            // of extension types they are to be used on
                            // .kgbentity
                            // .fx
                            // .css
                            // .x, .tvm, .obj, (.tva)
                            // (any non image use question mark or just skip)
                            // .dds
                            img = Image.FromFile(file); // this fails for DDS and TGA formats.  Convert to PNG 
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                    else 
                    {
                        // .kgbentity
                        // .fx
                        // .css
                        // .x, .tvm, .obj, (.tva)
                        // (any non image use question mark or just skip)
                        InvokeNonImageFound(folderPath, file);
                    }

                    if (img != null)
                    {
                        InvokeAdd (img, folderPath, file);
                        // img.Dispose();// cannot dispose image while we are using it
                    }
                }
            }

            if (mRecurseSubFolders)
            {
                string[] directories = null;
                // not using AllDirectories
                try
                {
                    // catch exception for trying to access any secure folders (eg recycle.bin)
                    directories = Directory.GetDirectories(folderPath);
                }
                catch { }

                if (directories != null && directories.Length >  0)
                {
                    foreach (string dir in directories)
                    {
                        if (m_CancelScanning) mThread.Abort();

                        if (IsFolderFiltered(dir)) continue;

                        // recurse next child directory
                        AddFolderIntern(dir);
                    }
                }
            }
        }

        protected void InvokeNonImageFound(string folder, string filename)
        {
            if (OnAdd != null)
                OnAdd.Invoke(null, new ImageBrowserEventArgs(folder, filename, false));
        }

        protected void InvokeAdd(Image image, string folder, string filename) 
        {
            if (OnAdd != null)
                OnAdd.Invoke(image, new ImageBrowserEventArgs(folder, filename));
        }

        protected bool IsValidImageFormat(string file)
        {
            string extension = Path.GetExtension(file).ToUpper();
            if (string.IsNullOrEmpty(extension)) return false;

            switch (extension)
            {
                // image textures are supported
                case ".DDS":
                case ".PNG":
                case ".BMP":
                case ".JPG":
                case ".GIF":
                case ".TGA":
                    return true;

                // materials are not valid
                case ".MTL":
                // meshes and scripts not valid image files
                case ".OBJ":
                case ".X":
                case ".TVA":
                case ".TVM":
               
                // scripts are supported
                case ".CSS":
                case ".FX":
                case ".TXT":

                    return false;


                default:
                   // MessageBox.Show(string.Format ("Unsupported file type {0}.", extension.ToUpper()));
                    break;
                
            }

            return false;
        }
        /// <summary>
        /// Compares file extension with list of filtered types and skips
        /// if it is NOT in the list
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected bool IsFileTypeFiltered(string file)
        {
            if (mAllowedFileTypes == null || mAllowedFileTypes.Length == 0) return false;

            string ext = Path.GetExtension (file).ToUpper ();
            if (string.IsNullOrEmpty(ext)) return true;

            for (int i = 0; i < mAllowedFileTypes.Length; i++)
                if (ext == mAllowedFileTypes[i].ToUpper()) return false;

            return true;
        }

        /// <summary>
        /// We filter certain dir names such as names that begin with
        /// "$" or "__" which might denote administrative folders
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        protected bool IsFolderFiltered(string folder)
        {
            if (mFolderFilter == null || mFolderFilter.Length == 0) return false;
            //TODO:
            return false;
        }
    }
}
