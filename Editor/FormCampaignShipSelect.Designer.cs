/*
 * Created by SharpDevelop.
 * User: Michael
 * Date: 8/29/2016
 * Time: 3:56 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace KeyEdit
{
	partial class FormCampaignShipSelect
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.GroupBox groupBoxDescription;
		private System.Windows.Forms.Label labelDescription;
		private ImageBrowserDotnet.ImageBrowser imageBrowser;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.groupBoxDescription = new System.Windows.Forms.GroupBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.imageBrowser = new ImageBrowserDotnet.ImageBrowser();
			this.groupBoxDescription.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(318, 437);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(104, 24);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.Text = "checkBox1";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// groupBoxDescription
			// 
			this.groupBoxDescription.Controls.Add(this.labelDescription);
			this.groupBoxDescription.Location = new System.Drawing.Point(219, 3);
			this.groupBoxDescription.Name = "groupBoxDescription";
			this.groupBoxDescription.Size = new System.Drawing.Size(371, 428);
			this.groupBoxDescription.TabIndex = 1;
			this.groupBoxDescription.TabStop = false;
			this.groupBoxDescription.Text = "Description";
			// 
			// labelDescription
			// 
			this.labelDescription.Location = new System.Drawing.Point(6, 26);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(359, 399);
			this.labelDescription.TabIndex = 0;
			this.labelDescription.Text = "Ship Description here...";
			// 
			// imageBrowser
			// 
			this.imageBrowser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.imageBrowser.ImageSize = 0;
			this.imageBrowser.ImageViewerEnabled = true;
			this.imageBrowser.Location = new System.Drawing.Point(3, 3);
			this.imageBrowser.Name = "imageBrowser";
			this.imageBrowser.RecurseSubFolders = false;
			this.imageBrowser.Size = new System.Drawing.Size(210, 458);
			this.imageBrowser.TabIndex = 2;
			// 
			// FormCampaignShipSelect
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.imageBrowser);
			this.Controls.Add(this.groupBoxDescription);
			this.Controls.Add(this.checkBox1);
			this.Name = "FormCampaignShipSelect";
			this.Size = new System.Drawing.Size(598, 469);
			this.groupBoxDescription.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
