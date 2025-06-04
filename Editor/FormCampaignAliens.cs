using System;
using System.Windows.Forms;
using SimpleWizard;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormCampaignAliens.
	/// </summary>
	public partial class FormCampaignAliens : UserControl, IWizardPage
	{
		private bool mBusy;
		private bool mPageValid;
		private string mMessage;
		
		public FormCampaignAliens(Settings.Initialization ini)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		public UserControl Content { get {return this;} }
        public bool IsBusy { get {return mBusy;} }
        public bool PageValid { get { return mPageValid;} }
        public string ValidationMessage { get {return mMessage;} }
        
        public new void Load()
        {        	
			// TODO: hardcoded validation success for now
			mPageValid = true;
        }
        
        public void Save()
        {
        }
        
        public void Cancel()
        {
        }
	}
}
