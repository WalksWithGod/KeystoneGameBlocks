using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleWizard;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormCampaignNew.
	/// </summary>
	public partial class FormCampaignNew : WizardHost
	{
		public FormCampaignNew(Settings.Initialization ini)
		{
			// - settings need to be shared by all wizard pages
			//	- OnWizardCompleted() needs to validate those settings
			//	  - player ship
			//		- player starting world
			//		- viewpoint tied to ship
			//	- galaxy settings
			//	- alien race settings (# of races, etc)
			//	
			//	- each page needs to validate before being allowed to advance to next?
			//	  - that's why the mPageIsValid var exists.
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			this.WizardCompleted += OnWizardCompleted;

		}

		void FormCampaignNewLoad(object sender, EventArgs e)
		{
	
		}
		
		void OnWizardCompleted()
		{
			System.Diagnostics.Debug.WriteLine ("FormCampaignNew:OnWizardCompleted()");
			
	
		}

	}
}
