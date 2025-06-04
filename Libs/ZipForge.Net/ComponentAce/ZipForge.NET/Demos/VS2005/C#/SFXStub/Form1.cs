using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;

namespace SFXStub
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;		
		private Button button1;
		private Button button2;
		private Button button3;		
		private FolderBrowserDialog folderBrowserDialog1;
		private Label label1;
		private System.Windows.Forms.CheckBox checkBox1;
		private TextBox textBox1;


		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(14, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(320, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select destination folder";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(16, 32);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(320, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "";
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(344, 32);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "Browse...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(128, 96);
			this.button2.Name = "button2";
			this.button2.TabIndex = 3;
			this.button2.Text = "Extract";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(224, 96);
			this.button3.Name = "button3";
			this.button3.TabIndex = 4;
			this.button3.Text = "Exit";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(16, 64);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(176, 16);
			this.checkBox1.TabIndex = 5;
			this.checkBox1.Text = "Overwrite existing files";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(424, 133);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "ZipForge.NET SFX  stub demo (C) ComponentAce 2007";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				this.textBox1.Text = this.folderBrowserDialog1.SelectedPath;
			}

		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			
			ZipForge ZipForge1 = new ZipForge();			
			ZipForge1.FileName = Application.ExecutablePath;
			ZipForge1.OpenArchive(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			ZipForge1.BaseDir = this.textBox1.Text;
			if (checkBox1.Checked)
				ZipForge1.Options.Overwrite = OverwriteMode.Always;
			else
				ZipForge1.Options.Overwrite = OverwriteMode.Never;            			
			ZipForge1.ExtractFiles("*.*");
			ZipForge1.CloseArchive();
			System.Windows.Forms.MessageBox.Show("All files were extracted successfully");
		
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void ZipForge1_OnFileProgress(object sender, string fileName, double progress, 
			ProcessOperation operation, ProgressPhase progressPhase, ref bool cancel)
		{
			
		}

		private void folderBrowserDialog1_HelpRequest(object sender, System.EventArgs e)
		{
		
		}
	}
}
