using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormNewScene.
	/// </summary>
	public partial class FormNewScene : Form
	{
		public FormNewScene()
		{
			InitializeComponent();
			this.DialogResult = DialogResult.Cancel;
		}
		
		public string SceneName { get { return textSceneName.Text; } }
		public float Diameter { get { return float.Parse(textRegionWidth.Text ); } }
		
		
		void ButtonOKClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;	
		}
		
		void ButtonCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
		
		void RadioButton2CheckedChanged(object sender, EventArgs e)
		{
			this.textRegionWidth.Text = AppMain.REGION_DIAMETER.ToString();
		}
		
		void RadioButton1CheckedChanged(object sender, EventArgs e)
		{
			this.textRegionWidth.Text = "40000";
		}
	}
}
