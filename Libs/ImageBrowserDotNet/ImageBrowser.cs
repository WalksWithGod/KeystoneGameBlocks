using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Ionic.Zip;

namespace ImageBrowserDotnet
{
    public partial class ImageBrowser : UserControl
    {
        public event ImageBrowserEventHandler OnBrowsingStarted;
        public event ImageBrowserEventHandler OnBrowsingFoundImage;
        public event ImageBrowserEventHandler OnBrowsingEnded;
        public event ImageBrowserEventHandler OnBrowsingAborted;
        public event ImageBrowserEventHandler OnBrowsingCanceled;
        public event ImageBrowserEventHandler OnBrowsingNonImageFileFound;

        public event MouseEventHandler OnImageClicked;
        public event MouseEventHandler OnImageDoubleClicked;
        public event MouseEventHandler OnImageMouseDown;
        public event MouseEventHandler OnImageMouseUp;
        public event MouseEventHandler OnImageMouseMove;


        delegate void DelegateAddImage(Image image, string imageModFolder, string imagePath);
        private DelegateAddImage m_AddImageDelegate;

        // browsing events

        // individual image events
        public event ImageViewerEventHandler OnImageSizeChanged; 


        private BrowserController m_Controller;
        private ImageDialog m_ImageDialog;
        private ImageViewer m_SelectedImageViewer;
        private bool mImageViewerEnabled = true;
        private bool mInitialized = false;
        private bool mRecurseSubFolders = false;
        private int mImageSize;

        public ImageBrowser()
        {
            InitializeComponent();


            // create the large image preview dialog
            m_ImageDialog = new ImageDialog();
            m_AddImageDelegate = new DelegateAddImage(this.AddImage);
        }

        public int ImageSize 
        { 
            get { return mImageSize; } 
            set 
            {
                if (OnImageSizeChanged != null)
                    OnImageSizeChanged(this, new ImageViewerEventArgs(value));
                mImageSize = value; 
            } 
        }
        public bool ImageViewerEnabled { get { return mImageViewerEnabled; } set { mImageViewerEnabled = value; } }
        public bool RecurseSubFolders
        {
            get { return mRecurseSubFolders; }
            set 
            { 
                if (mInitialized )
                    m_Controller.RecurseSubFolders = value;

                mRecurseSubFolders = value; 
            }
        }

        public void Initialize(string modsPath, string modName) 
        {
            if (string.IsNullOrEmpty(modName))
                throw new ArgumentNullException("ImageBrowser.Initialize() - Invalid mod name."); 

            string fullPath= System.IO.Path.Combine (modsPath, modName);
            System.IO.FileInfo info = new System.IO.FileInfo( fullPath);

            if (modName.EndsWith (".zip"))
            {
	            if (info == null ||
	                info.Length == 0 ||
	                info.Exists == false)
	                throw new System.IO.FileLoadException("ImageBrowser.Initialize() - Invalid zip file.");
	
	            // initialize with a zip file crawler
	            Initialize(new BrowserZipController(modsPath, modName));
            }
            else
            {
				// regular folder on disk browsing
            	Initialize(new BrowserController(modsPath, modName));
            }
        }

        public void Initialize(BrowserController controller)
        {
            if (mInitialized)
            {
                // release events from previous controller
                m_Controller.OnStart -= m_Controller_OnStart;
                m_Controller.OnAdd -= m_Controller_OnAdd;
                m_Controller.OnEnd -= m_Controller_OnEnd;
                m_Controller = null;
                mInitialized = false;
            }

            // wire up events to notify us when completed.  We will then
            // pass these events back to the caller using the caller provided event handlers
            m_Controller = controller;
            m_Controller.OnStart += new ImageBrowserEventHandler(m_Controller_OnStart);
            m_Controller.OnAdd += new ImageBrowserEventHandler(m_Controller_OnAdd);
            m_Controller.OnEnd += new ImageBrowserEventHandler(m_Controller_OnEnd);

            mInitialized = true;
            RecurseSubFolders = mRecurseSubFolders; // make sure existing value gets applied to new controller
        }

        /// <summary>
        /// Calls a controller which implements threaded loading of the image files
        /// so that the calling app is not blocked whilst all images are loaded.
        /// </summary>
        /// <param name="folderPath"></param>
        public void BrowseFolder(string folderPath, bool clearExistingImages, string[] allowedExtensions)
        {
            if (!mInitialized) throw new Exception("ImageBrowser.BrowseFolder() - Not initialized.");			
            if (clearExistingImages)
            {
	            foreach (Control c in this.flowLayoutPanelMain.Controls)
	              c.Dispose();

	            this.flowLayoutPanelMain.Controls.Clear();
            }
            m_Controller.BrowseFolder (folderPath, allowedExtensions );
        }

        public void BrowseFolder(string folderPath, bool clearExistingImages)
        {
            BrowseFolder(folderPath, clearExistingImages, null);
        }

        public void CancelBrowsing()
        {
            m_Controller.CancelScanning = true;
        }

        #region Thumbnail Controller event handlers
        private void m_Controller_OnStart(object sender, ImageBrowserEventArgs e)
        {
            if (OnBrowsingStarted != null)
                OnBrowsingStarted.Invoke(this, new ImageBrowserEventArgs(null, null));
        }

        private void m_Controller_OnAdd(object sender, ImageBrowserEventArgs e)
        {
            if (e.IsImageFile == false)
            {
            	if (this.OnBrowsingNonImageFileFound != null)
	                this.OnBrowsingNonImageFileFound(sender, e);
            }
            else 
                this.AddImage((Image)sender, e.ImageModFolder, e.ImageFilePath);
    
            
            this.Invalidate();

            if (OnBrowsingFoundImage != null)
                OnBrowsingFoundImage.Invoke(this, new ImageBrowserEventArgs(e.ImageModFolder, e.ImageFilePath));
        }

        private void m_Controller_OnEnd(object sender, ImageBrowserEventArgs e)
        {
            // thread safe
            if (this.InvokeRequired)
            {
                // re-invoke so that next time it recurses here, no invoke required and 
                // the "else" code will get executed
                this.Invoke(new ImageBrowserEventHandler(m_Controller_OnEnd), sender, e);
            }
            else
            {
                // notify calling app 
                if (OnBrowsingEnded  != null)
                    OnBrowsingEnded.Invoke(this, new ImageBrowserEventArgs (null, null));
            }
        }
        #endregion



        // TODO: this filename can be either a filename or an archive entry name
        // need to have overloads... hrm...
        public void AddImage(Image image, string modFolder, string path)
        {

            // thread safe
            if (this.InvokeRequired)
            {
                this.Invoke(m_AddImageDelegate, image, modFolder, path);
            }
            else
            {
                int size = ImageSize;
                
                ImageViewer imageViewer = new ImageViewer();
                imageViewer.Dock = DockStyle.Bottom;
                imageViewer.Font = this.Font;
                imageViewer.ShowCaption = false;
               // imageViewer.LoadImage(imageFilename, 256, 256);
                imageViewer.Image = image;
                imageViewer.ImageModFolder = modFolder;
                imageViewer.ImagePath = path;
                imageViewer.Width = size;
                imageViewer.Height = size;
                imageViewer.IsThumbnail = true;
                imageViewer.MouseClick += imageViewer_MouseClick;
                imageViewer.MouseDoubleClick += imageViewer_MouseDoubleClick;
                imageViewer.MouseDown += imageViewer_MouseDown;
                imageViewer.MouseUp += imageViewer_MouseUp;
                imageViewer.MouseMove += imageViewer_MouseMove;
                imageViewer.Visible = true;
                imageViewer.Enabled = true;

                // note: we add each new image viewer's ImageSizeChange() method
                // to the list of event handlers responding to the change in size here
                if (this.OnImageSizeChanged != null)
                    this.OnImageSizeChanged += imageViewer.ImageSizeChanged;

                this.flowLayoutPanelMain.Enabled = true;
                this.flowLayoutPanelMain.Visible = true;
                this.flowLayoutPanelMain.Controls.Add(imageViewer);
            }
        }


        private void imageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (OnImageMouseMove != null)
                OnImageMouseMove(sender, e);
        }

        private void imageViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (OnImageMouseDown != null)
                OnImageMouseDown(sender, e);
        }

        private void imageViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (OnImageMouseUp != null)
                OnImageMouseUp(sender, e);
        }


        private void imageViewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (OnImageDoubleClicked != null)
                OnImageDoubleClicked(sender, e);
        }

        private void imageViewer_MouseClick(object sender, MouseEventArgs e)
        {
            // disable any previous image viewer
            // note: image viewer is just a single thumbnail viewer which is used
            // both in the browser (using many individual instances) to display all 
            // thumbnails and a single instance in the preview dialog to display
            if (m_SelectedImageViewer != null)
            {
                m_SelectedImageViewer.IsActive = false;
            }

            // activate current
            m_SelectedImageViewer = (ImageViewer)sender;
            m_SelectedImageViewer.IsActive = true;

            // notify the calling application
            if (OnImageClicked != null)
            {
                OnImageClicked.Invoke(this, e);
            }
        }

        public ImageViewer SelectedImageViewer { get { return m_SelectedImageViewer; } }

        public ImageViewer GetImageAt(int x, int y)
        {
            return null;
        }
    }
}
