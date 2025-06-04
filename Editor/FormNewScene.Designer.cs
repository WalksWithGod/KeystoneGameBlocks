/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/18/2015
 * Time: 9:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace KeyEdit
{
	partial class FormNewScene
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.labelSectorWidth = new System.Windows.Forms.Label();
			this.labelName = new System.Windows.Forms.Label();
			this.textSceneName = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textRegionWidth = new DevComponents.DotNetBar.Controls.TextBoxX();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(352, 106);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(65, 29);
			this.buttonOK.TabIndex = 46;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOKClick);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(276, 106);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(70, 29);
			this.buttonCancel.TabIndex = 45;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
			// 
			// labelSectorWidth
			// 
			this.labelSectorWidth.AutoSize = true;
			this.labelSectorWidth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSectorWidth.Location = new System.Drawing.Point(17, 51);
			this.labelSectorWidth.Name = "labelSectorWidth";
			this.labelSectorWidth.Size = new System.Drawing.Size(112, 16);
			this.labelSectorWidth.TabIndex = 44;
			this.labelSectorWidth.Text = "Diameter (meters)";
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelName.Location = new System.Drawing.Point(17, 18);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(83, 16);
			this.labelName.TabIndex = 43;
			this.labelName.Text = "Scene Name";
			// 
			// textSceneName
			// 
			// 
			// 
			// 
			this.textSceneName.Border.Class = "TextBoxBorder";
			this.textSceneName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textSceneName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textSceneName.Location = new System.Drawing.Point(138, 12);
			this.textSceneName.MaxLength = 128;
			this.textSceneName.Name = "textSceneName";
			this.textSceneName.Size = new System.Drawing.Size(142, 22);
			this.textSceneName.TabIndex = 42;
			this.textSceneName.Text = "SimpleScene001";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textRegionWidth);
			this.groupBox1.Controls.Add(this.radioButton2);
			this.groupBox1.Controls.Add(this.radioButton1);
			this.groupBox1.Location = new System.Drawing.Point(126, 40);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(269, 60);
			this.groupBox1.TabIndex = 48;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// textRegionWidth
			// 
			// 
			// 
			// 
			this.textRegionWidth.Border.Class = "TextBoxBorder";
			this.textRegionWidth.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
			this.textRegionWidth.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textRegionWidth.Location = new System.Drawing.Point(21, 21);
			this.textRegionWidth.MaxLength = 15;
			this.textRegionWidth.Name = "textRegionWidth";
			this.textRegionWidth.Size = new System.Drawing.Size(142, 22);
			this.textRegionWidth.TabIndex = 50;
			this.textRegionWidth.Tag = "";
			this.textRegionWidth.Text = "40000";
			// 
			// radioButton2
			// 
			this.radioButton2.Location = new System.Drawing.Point(188, 37);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(75, 25);
			this.radioButton2.TabIndex = 49;
			this.radioButton2.Text = "Space";
			this.radioButton2.UseVisualStyleBackColor = true;
			this.radioButton2.CheckedChanged += new System.EventHandler(this.RadioButton2CheckedChanged);
			// 
			// radioButton1
			// 
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(188, 11);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(54, 25);
			this.radioButton1.TabIndex = 48;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Land";
			this.radioButton1.UseVisualStyleBackColor = true;
			this.radioButton1.CheckedChanged += new System.EventHandler(this.RadioButton1CheckedChanged);
			// 
			// FormNewScene
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(429, 147);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.labelSectorWidth);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.textSceneName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormNewScene";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Scene";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.GroupBox groupBox1;
		private DevComponents.DotNetBar.Controls.TextBoxX textRegionWidth;
		private DevComponents.DotNetBar.Controls.TextBoxX textSceneName;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelSectorWidth;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
	}
}
