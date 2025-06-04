using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;

namespace Spanning
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button createSplittingButton;
		private System.Windows.Forms.Button createSpanningButton;
		private System.Windows.Forms.Button btnNewArchiveFile;
		private System.Windows.Forms.Button btnSourceFiles;
		private System.Windows.Forms.TextBox tbNewArchiveFile;
		private System.Windows.Forms.TextBox tbSourceFiles;
		private System.Windows.Forms.Button btnExtractTo;
		private System.Windows.Forms.Button btnArchiveFile;
		private System.Windows.Forms.TextBox tbExtractTo;
		private System.Windows.Forms.TextBox tbArchiveFile;
		private System.Windows.Forms.OpenFileDialog ofdSourceFiles;
		private System.Windows.Forms.SaveFileDialog sfdNewArchiveFile;
		private System.Windows.Forms.ProgressBar pbProgress;
		private System.Windows.Forms.OpenFileDialog ofdArchiveFile;
		private System.Windows.Forms.FolderBrowserDialog fbdExtractTo;
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.createSplittingButton = new System.Windows.Forms.Button();
			this.createSpanningButton = new System.Windows.Forms.Button();
			this.btnNewArchiveFile = new System.Windows.Forms.Button();
			this.btnSourceFiles = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.tbNewArchiveFile = new System.Windows.Forms.TextBox();
			this.tbSourceFiles = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.button7 = new System.Windows.Forms.Button();
			this.btnExtractTo = new System.Windows.Forms.Button();
			this.btnArchiveFile = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tbExtractTo = new System.Windows.Forms.TextBox();
			this.tbArchiveFile = new System.Windows.Forms.TextBox();
			this.pbProgress = new System.Windows.Forms.ProgressBar();
			this.ofdSourceFiles = new System.Windows.Forms.OpenFileDialog();
			this.sfdNewArchiveFile = new System.Windows.Forms.SaveFileDialog();
			this.ofdArchiveFile = new System.Windows.Forms.OpenFileDialog();
			this.fbdExtractTo = new System.Windows.Forms.FolderBrowserDialog();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(416, 208);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.createSplittingButton);
			this.tabPage1.Controls.Add(this.createSpanningButton);
			this.tabPage1.Controls.Add(this.btnNewArchiveFile);
			this.tabPage1.Controls.Add(this.btnSourceFiles);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.tbNewArchiveFile);
			this.tabPage1.Controls.Add(this.tbSourceFiles);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(408, 182);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Create archive";
			// 
			// createSplittingButton
			// 
			this.createSplittingButton.Location = new System.Drawing.Point(40, 120);
			this.createSplittingButton.Name = "createSplittingButton";
			this.createSplittingButton.Size = new System.Drawing.Size(144, 23);
			this.createSplittingButton.TabIndex = 7;
			this.createSplittingButton.Text = "Create splitting archive";
			this.createSplittingButton.Click += new System.EventHandler(this.createArchive_Click);
			// 
			// createSpanningButton
			// 
			this.createSpanningButton.Location = new System.Drawing.Point(192, 120);
			this.createSpanningButton.Name = "createSpanningButton";
			this.createSpanningButton.Size = new System.Drawing.Size(152, 23);
			this.createSpanningButton.TabIndex = 6;
			this.createSpanningButton.Text = "Create spanning archive";
			this.createSpanningButton.Click += new System.EventHandler(this.createArchive_Click);
			// 
			// btnNewArchiveFile
			// 
			this.btnNewArchiveFile.Location = new System.Drawing.Point(312, 72);
			this.btnNewArchiveFile.Name = "btnNewArchiveFile";
			this.btnNewArchiveFile.TabIndex = 5;
			this.btnNewArchiveFile.Text = "Browse...";
			this.btnNewArchiveFile.Click += new System.EventHandler(this.btnNewArchiveFile_Click);
			// 
			// btnSourceFiles
			// 
			this.btnSourceFiles.Location = new System.Drawing.Point(312, 24);
			this.btnSourceFiles.Name = "btnSourceFiles";
			this.btnSourceFiles.TabIndex = 4;
			this.btnSourceFiles.Text = "Browse...";
			this.btnSourceFiles.Click += new System.EventHandler(this.btnSourceFiles_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Archive file";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Source files";
			// 
			// tbNewArchiveFile
			// 
			this.tbNewArchiveFile.Location = new System.Drawing.Point(80, 72);
			this.tbNewArchiveFile.Name = "tbNewArchiveFile";
			this.tbNewArchiveFile.Size = new System.Drawing.Size(216, 20);
			this.tbNewArchiveFile.TabIndex = 1;
			this.tbNewArchiveFile.Text = "";
			// 
			// tbSourceFiles
			// 
			this.tbSourceFiles.Location = new System.Drawing.Point(80, 24);
			this.tbSourceFiles.Name = "tbSourceFiles";
			this.tbSourceFiles.Size = new System.Drawing.Size(216, 20);
			this.tbSourceFiles.TabIndex = 0;
			this.tbSourceFiles.Text = "";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.button7);
			this.tabPage2.Controls.Add(this.btnExtractTo);
			this.tabPage2.Controls.Add(this.btnArchiveFile);
			this.tabPage2.Controls.Add(this.label3);
			this.tabPage2.Controls.Add(this.label4);
			this.tabPage2.Controls.Add(this.tbExtractTo);
			this.tabPage2.Controls.Add(this.tbArchiveFile);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(408, 182);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Extract files from archive";
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(128, 120);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(128, 24);
			this.button7.TabIndex = 12;
			this.button7.Text = "Extract files";
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// btnExtractTo
			// 
			this.btnExtractTo.Location = new System.Drawing.Point(312, 72);
			this.btnExtractTo.Name = "btnExtractTo";
			this.btnExtractTo.TabIndex = 11;
			this.btnExtractTo.Text = "Browse...";
			this.btnExtractTo.Click += new System.EventHandler(this.btnExtractTo_Click);
			// 
			// btnArchiveFile
			// 
			this.btnArchiveFile.Location = new System.Drawing.Point(312, 24);
			this.btnArchiveFile.Name = "btnArchiveFile";
			this.btnArchiveFile.TabIndex = 10;
			this.btnArchiveFile.Text = "Browse...";
			this.btnArchiveFile.Click += new System.EventHandler(this.btnArchiveFile_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 16);
			this.label3.TabIndex = 9;
			this.label3.Text = "Extract to";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 32);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 16);
			this.label4.TabIndex = 8;
			this.label4.Text = "Archive file";
			// 
			// tbExtractTo
			// 
			this.tbExtractTo.Location = new System.Drawing.Point(80, 72);
			this.tbExtractTo.Name = "tbExtractTo";
			this.tbExtractTo.Size = new System.Drawing.Size(216, 20);
			this.tbExtractTo.TabIndex = 7;
			this.tbExtractTo.Text = "";
			// 
			// tbArchiveFile
			// 
			this.tbArchiveFile.Location = new System.Drawing.Point(80, 24);
			this.tbArchiveFile.Name = "tbArchiveFile";
			this.tbArchiveFile.Size = new System.Drawing.Size(216, 20);
			this.tbArchiveFile.TabIndex = 6;
			this.tbArchiveFile.Text = "";
			// 
			// pbProgress
			// 
			this.pbProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pbProgress.Location = new System.Drawing.Point(0, 205);
			this.pbProgress.Name = "pbProgress";
			this.pbProgress.Size = new System.Drawing.Size(416, 16);
			this.pbProgress.TabIndex = 1;
			// 
			// sfdNewArchiveFile
			// 
			this.sfdNewArchiveFile.DefaultExt = "*.zip";
			this.sfdNewArchiveFile.Filter = "ZIP archives|*.zip";
			// 
			// ofdArchiveFile
			// 
			this.ofdArchiveFile.Filter = "ZipForge archives|*.zip";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 221);
			this.Controls.Add(this.pbProgress);
			this.Controls.Add(this.tabControl1);
			this.Name = "Form1";
			this.Text = "Spanning demo (C) ComponentAce 2007";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
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



		private void button7_Click(object sender, System.EventArgs e)
		{
			// create ZipForge object			
			ZipForge ZipForge1 = new ZipForge();			
			// set file name
			ZipForge1.FileName = tbArchiveFile.Text;
			// open archive
			ZipForge1.OpenArchive();
			// set base directory
			ZipForge1.BaseDir = tbExtractTo.Text;
			// set OnOverallProgress event
			ZipForge1.OnOverallProgress += new 
BaseArchiver.OnOverallProgressDelegate(zf_OnOverallProgress);
			// extract all files
			ZipForge1.ExtractFiles("*.*");
			// close archive
			ZipForge1.CloseArchive();           		
		}


		private void createArchive_Click(object sender, System.EventArgs e)
		{
			// create ZipForge object
			
			ZipForge ZipForge1 = new ZipForge();
			// set OnOverallProgress event
			ZipForge1.OnOverallProgress += new 
BaseArchiver.OnOverallProgressDelegate(zf_OnOverallProgress);

			// specify spanning mode
			if ((sender as Button).Name == createSpanningButton.Name)
                ZipForge1.SpanningMode = SpanningMode.Spanning;
			else
				ZipForge1.SpanningMode = SpanningMode.Splitting;
			// set volume size to 1.44 Mb			
			ZipForge1.SpanningOptions.VolumeSize = VolumeSize.Disk1_44MB;
			
			// code to set custom volume size:
			// ZipForge1.SpanningOptions.VolumeSize = VolumeSize.Custom;
			// ZipForge1.SpanningOptions.CustomVolumeSize = 1024*1024; // 1Mb 
            
			// set archive file 
			ZipForge1.FileName = tbNewArchiveFile.Text;
			// create a new archive
			ZipForge1.OpenArchive(System.IO.FileMode.Create);
			// set base directory to the folder containing selected file
			ZipForge1.BaseDir = System.IO.Path.GetDirectoryName(tbSourceFiles.Text);
			// add file(s)
			ZipForge1.AddFiles(tbSourceFiles.Text);
			// close archive
			ZipForge1.CloseArchive(); 
		}

		private void btnSourceFiles_Click(object sender, System.EventArgs e)
		{
			if (ofdSourceFiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				tbSourceFiles.Text = ofdSourceFiles.FileName;
		}

		private void btnNewArchiveFile_Click(object sender, System.EventArgs e)
		{
			if (sfdNewArchiveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
				tbNewArchiveFile.Text = sfdNewArchiveFile.FileName;
		}
		

		private void btnArchiveFile_Click(object sender, System.EventArgs e)
		{
			if (ofdArchiveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				tbArchiveFile.Text = ofdArchiveFile.FileName;
		}

		private void btnExtractTo_Click(object sender, System.EventArgs e)
		{
			if (fbdExtractTo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
 				tbExtractTo.Text = fbdExtractTo.SelectedPath;
		}

		private void zf_OnOverallProgress(object sender, double progress, 
			ProcessOperation operation, ProgressPhase progressPhase, ref bool cancel)
		{
			pbProgress.Value = (int)progress;
		}
	}
}
