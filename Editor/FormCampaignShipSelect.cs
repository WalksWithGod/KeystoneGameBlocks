using System;
using System.Drawing;
using System.Windows.Forms;
using ImageBrowserDotnet;
using SimpleWizard;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormCampaignShipSelect.
	/// </summary>
	public partial class FormCampaignShipSelect : UserControl, IWizardPage
	{
		private bool mBusy;
		private bool mPageValid;
		private string mMessage;
		
		public FormCampaignShipSelect(Settings.Initialization ini)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
		}
		
		public UserControl Content { get {return this;} }
        public bool IsBusy { get {return mBusy;} }
        public bool PageValid { get { return mPageValid;} }
        public string ValidationMessage { get {return mMessage;} }
        
        public new void Load()
        {
        	FillImageBrowser();
        	
			// TODO: hardcoded validation success for now
			mPageValid = true;
        }
        
        public void Save()
        {
        }
        
        public void Cancel()
        {
        }
        
    	void FillImageBrowser()
    	{
    		// TODO: load preview images from \\modname\\meshes\\vehicles
			//		 - associate a description with each vehicle?
			//			- we can do this manually by modifying the SceneInfo.Description
			//			and resaving the prefab archive
			string modName = "caesar";
			imageBrowser.OnBrowsingNonImageFileFound += ImageBrowser_OnBrowsingNonImageFileFound;
			imageBrowser.Initialize(AppMain.MOD_PATH, modName);
			string vehiclesFolderPath = System.IO.Path.Combine (AppMain.MOD_PATH, "caesar\\meshes\\vehicles");
			string[] allowedExtensions = new string[]{".KGBENTITY", ".KGBSEGMENT"};
			imageBrowser.BrowseFolder  (vehiclesFolderPath, false, allowedExtensions);
    	}

    	void ImageBrowser_OnBrowsingNonImageFileFound(object sender, ImageBrowserEventArgs e)
        {
            string modFullPath = System.IO.Path.Combine(AppMain.MOD_PATH, "caesar\\meshes\\vehicles");

            Image image = Workspaces.EditorWorkspace.OnNonImageFileFound (e.ImageFilePath, modFullPath);
            if (image != null)
            {
            	string filePath = @"D:\dev\c#\KeystoneGameBlocks\Data\mods\caesar\meshes\Vehicles\Blinds_Cloth_Horizontal.png";
            	image = Image.FromFile (filePath);
                imageBrowser.AddImage(image, e.ImageModFolder, e.ImageFilePath); // note: we still use the original file, not the placeholder
                System.Diagnostics.Debug.WriteLine ("FormCampaignShipSelect.ImageBrowser_OnBrowsingNonImageFileFound() - " + e.ImageFilePath);
            }
    	}
    	
	}
}
