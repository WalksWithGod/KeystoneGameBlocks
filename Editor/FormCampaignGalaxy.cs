using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleWizard;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormCampaignGalaxy.
	/// </summary>
	public partial class FormCampaignGalaxy : UserControl, IWizardPage
	{
		private bool mBusy;
		private bool mPageValid;
		private string mMessage;
		
		public FormCampaignGalaxy(Settings.Initialization ini)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
					
			// TODO: hardcoded for now
			mPageValid = true; 
		}
		
		public UserControl Content { get {return this; } }
        public bool IsBusy { get {return mBusy;} }
        public bool PageValid { get { return mPageValid;} }
        public string ValidationMessage { get {return mMessage;} }
        
        public void Load()
        {
        }
        
        public void Save()
        {
        }
        
        public void Cancel()
        {
        }

        private void radioGlobularCluster_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
