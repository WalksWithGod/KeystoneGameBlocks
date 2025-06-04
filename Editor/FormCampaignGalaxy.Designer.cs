/*
 * Created by SharpDevelop.
 * User: Michael
 * Date: 8/29/2016
 * Time: 3:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace KeyEdit
{
	partial class FormCampaignGalaxy
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.RadioButton radioSpiral;
		private System.Windows.Forms.RadioButton radioGlobularCluster;
		private System.Windows.Forms.RadioButton radioElliptical;
		private System.Windows.Forms.RadioButton radioBarred;
		private System.Windows.Forms.RadioButton radioIrregular;
		private System.Windows.Forms.RadioButton radioPeculiar;
		private System.Windows.Forms.RadioButton radioLenticular;
		
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
            this.radioSpiral = new System.Windows.Forms.RadioButton();
            this.radioGlobularCluster = new System.Windows.Forms.RadioButton();
            this.radioElliptical = new System.Windows.Forms.RadioButton();
            this.radioBarred = new System.Windows.Forms.RadioButton();
            this.radioIrregular = new System.Windows.Forms.RadioButton();
            this.radioPeculiar = new System.Windows.Forms.RadioButton();
            this.radioLenticular = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // radioSpiral
            // 
            this.radioSpiral.Location = new System.Drawing.Point(341, 71);
            this.radioSpiral.Name = "radioSpiral";
            this.radioSpiral.Size = new System.Drawing.Size(104, 24);
            this.radioSpiral.TabIndex = 0;
            this.radioSpiral.Text = "Spiral Galaxy";
            this.radioSpiral.UseVisualStyleBackColor = true;
            // 
            // radioGlobularCluster
            // 
            this.radioGlobularCluster.Checked = true;
            this.radioGlobularCluster.Location = new System.Drawing.Point(341, 41);
            this.radioGlobularCluster.Name = "radioGlobularCluster";
            this.radioGlobularCluster.Size = new System.Drawing.Size(104, 24);
            this.radioGlobularCluster.TabIndex = 1;
            this.radioGlobularCluster.TabStop = true;
            this.radioGlobularCluster.Text = "Globula Cluster";
            this.radioGlobularCluster.UseVisualStyleBackColor = true;
            this.radioGlobularCluster.CheckedChanged += new System.EventHandler(this.radioGlobularCluster_CheckedChanged);
            // 
            // radioElliptical
            // 
            this.radioElliptical.Location = new System.Drawing.Point(341, 101);
            this.radioElliptical.Name = "radioElliptical";
            this.radioElliptical.Size = new System.Drawing.Size(104, 24);
            this.radioElliptical.TabIndex = 2;
            this.radioElliptical.Text = "Elliptical";
            this.radioElliptical.UseVisualStyleBackColor = true;
            // 
            // radioBarred
            // 
            this.radioBarred.Location = new System.Drawing.Point(341, 131);
            this.radioBarred.Name = "radioBarred";
            this.radioBarred.Size = new System.Drawing.Size(104, 24);
            this.radioBarred.TabIndex = 3;
            this.radioBarred.Text = "Barred Spiral";
            this.radioBarred.UseVisualStyleBackColor = true;
            // 
            // radioIrregular
            // 
            this.radioIrregular.Location = new System.Drawing.Point(341, 161);
            this.radioIrregular.Name = "radioIrregular";
            this.radioIrregular.Size = new System.Drawing.Size(104, 24);
            this.radioIrregular.TabIndex = 4;
            this.radioIrregular.Text = "Irregular";
            this.radioIrregular.UseVisualStyleBackColor = true;
            // 
            // radioPeculiar
            // 
            this.radioPeculiar.Location = new System.Drawing.Point(341, 191);
            this.radioPeculiar.Name = "radioPeculiar";
            this.radioPeculiar.Size = new System.Drawing.Size(104, 24);
            this.radioPeculiar.TabIndex = 5;
            this.radioPeculiar.Text = "Peduliar";
            this.radioPeculiar.UseVisualStyleBackColor = true;
            // 
            // radioLenticular
            // 
            this.radioLenticular.Location = new System.Drawing.Point(341, 221);
            this.radioLenticular.Name = "radioLenticular";
            this.radioLenticular.Size = new System.Drawing.Size(104, 24);
            this.radioLenticular.TabIndex = 6;
            this.radioLenticular.Text = "Lenticular";
            this.radioLenticular.UseVisualStyleBackColor = true;
            // 
            // FormCampaignGalaxy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.radioLenticular);
            this.Controls.Add(this.radioPeculiar);
            this.Controls.Add(this.radioIrregular);
            this.Controls.Add(this.radioBarred);
            this.Controls.Add(this.radioElliptical);
            this.Controls.Add(this.radioGlobularCluster);
            this.Controls.Add(this.radioSpiral);
            this.Name = "FormCampaignGalaxy";
            this.Size = new System.Drawing.Size(484, 543);
            this.ResumeLayout(false);

		}
	}
}
