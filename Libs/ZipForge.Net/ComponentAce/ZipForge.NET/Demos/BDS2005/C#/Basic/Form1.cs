using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.ZipForge;
using ComponentAce.Compression.Archiver;
using System.IO;

namespace Basic
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
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.ZipForge1 = new ComponentAce.Compression.ZipForge.ZipForge();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(111, 102);
			this.button1.Name = "button1";
			this.button1.TabIndex = 0;
			this.button1.Text = "Start";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.button2.Location = new System.Drawing.Point(225, 102);
			this.button2.Name = "button2";
			this.button2.TabIndex = 1;
			this.button2.Text = "Exit";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(37, 63);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(335, 23);
			this.progressBar1.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(34, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(338, 49);
			this.label1.TabIndex = 3;
			this.label1.Text = "This demo illustrates how to add files and extract files using ZipForge. Sour" +
				"ce files are located in Source folder. Destination archive file will be located " +
				"in Archive\\test.zip. Files will be extracted to folder Dest.";
			// 
			// ZipForge1
			// 
			this.ZipForge1.Active = false;
			this.ZipForge1.BaseDir = "";
			this.ZipForge1.FileName = null;
			this.ZipForge1.CompressionLevel = ComponentAce.Compression.Archiver.CompressionLevel.Fastest;
			this.ZipForge1.CompressionMode = ((System.Byte)(1));
			this.ZipForge1.FileName = null;
			this.ZipForge1.FileName = null;
			this.ZipForge1.ExtractCorruptedFiles = false;
			this.ZipForge1.FileName = null;
			this.ZipForge1.InMemory = false;
			this.ZipForge1.OpenCorruptedArchives = true;
			this.ZipForge1.Password = "";
			this.ZipForge1.SFXStub = null;
			this.ZipForge1.SpanningMode = ComponentAce.Compression.Archiver.SpanningMode.None;
			this.ZipForge1.TempDir = null;
			this.ZipForge1.OnOverallProgress += new ComponentAce.Compression.Archiver.BaseArchiver.OnOverallProgressDelegate(this.ZipForge1_OnOverallProgress);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(411, 128);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "ZipForge.NET basic demo (c) ComponentAce 2007";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label1;
		private ZipForge ZipForge1;                

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
			string DemoFolder = Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\";
			// Set archive file name
			ZipForge1.FileName = DemoFolder + "Archive\\test.zip";
			// Create a new archive file
			ZipForge1.OpenArchive(FileMode.Create);
			// Set path to folder with the files to archive
			ZipForge1.BaseDir = DemoFolder + "\\Source";
			// Add all files and directories from the source folder to the archive
			ZipForge1.AddFiles("*.*");
			// Set path to the destination folder
			ZipForge1.BaseDir = DemoFolder + "\\Dest";
			// extract all files in archive
			ZipForge1.ExtractFiles("*.*");
			// Close archive
			ZipForge1.CloseArchive();
			MessageBox.Show("All files were added and extracted successfully.");
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void ZipForge1_OnOverallProgress(object sender, double progress, ProcessOperation operation, ProgressPhase progressPhase, ref bool cancel)
		{
			progressBar1.Value = (int)progress;
		}
	}
}
