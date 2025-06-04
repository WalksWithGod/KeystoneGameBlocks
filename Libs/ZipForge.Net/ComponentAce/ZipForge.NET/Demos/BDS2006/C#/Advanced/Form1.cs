using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.ZipForge;
using ComponentAce.Compression.Archiver;
using System.IO;

namespace Advanced
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
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.ZipForge1 = new ComponentAce.Compression.ZipForge.ZipForge();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(384, 55);
			this.label1.TabIndex = 0;
			this.label1.Text = "This demo illustrates how to handle ZipForge archives. Source files are locat" +
				"ed in Source folder. Destination archive file will be located in Archive\\test.zi" +
				"p. Files will be extracted to folders Dest and Dest1.";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(140, 78);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "Start";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(247, 78);
			this.button2.Name = "button2";
			this.button2.TabIndex = 3;
			this.button2.Text = "Exit";
			this.button2.Click += new System.EventHandler(this.button2_Click);
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
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(463, 109);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "ZipForge.NET Advanced Demo (c) ComponentAce 2007";
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private ZipForge ZipForge1;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			string DemoFolder = Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\";
			Directory.SetCurrentDirectory(DemoFolder);             

			// clear Dest folder
			if (Directory.Exists("Dest"))
				Directory.Delete("Dest", true);
			Directory.CreateDirectory("Dest");
            
			ZipForge1.FileName = "Archive\\test.zip";
            
			File.Copy("Source\\1.txt", "Source1\\2.txt", true);
			File.Copy("Source\\uMain.pas", "Source1\\2.pas", true);
			File.Copy("Source\\dummy.mp3", "Source1\\dummy2.mp3", true);
			Directory.CreateDirectory("Source1\\33.txt");
			// Create a new archive file
			ZipForge1.OpenArchive(FileMode.Create);
			// Let's encrypt all files
			ZipForge1.Password = "The password";
			// Set path to folder with some text files to BaseDir
			ZipForge1.BaseDir = "Source";
			// Do not compress MPEG3 files
			ZipForge1.NoCompressionMasks.Add("*.mp3");
			// Add all files and directories from Source excluding text files to the archive
			ZipForge1.AddFiles("*.*", FileAttributes.Archive| 
				FileAttributes.Normal |
				FileAttributes.Directory, "*.txt");
			// Set path to destination folder            
			ZipForge1.BaseDir = "Dest";
			// Extract all files and directories from the archive to BaseDir
			// After extracting directory Dest should contain all files from folder
			// Source excluding *.txt files
			ZipForge1.ExtractFiles("*.*");
			// Use full path
			ZipForge1.Options.StorePath = StorePathMode.FullPath;
			// Set path to destination folder
			ZipForge1.BaseDir = "Source1";
			// Move all text files from Source1 to the archive
			// After moving directory Source1 should not contain any text files
			ZipForge1.MoveFiles("*.txt", FileAttributes.Normal | FileAttributes.Archive);
			// Set path to current drive
			ZipForge1.BaseDir = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
			// Overwrite all files
			ZipForge1.Options.Overwrite = OverwriteMode.Always;
			// Update all files excluding 1???.t* from Source1
			ZipForge1.UpdateFiles(DemoFolder + "\\Source1\\*.*", FileAttributes.Archive | FileAttributes.Normal,
				"2???.t*");
			// Set temporary directory
			ZipForge1.TempDir = "Temp";
			// Test all files and directories in the archive
			try
			{
				ZipForge1.TestFiles("*.*");
			}
			catch
			{
				MessageBox.Show("Archive is corrupted");
			}

			// Use full path
			ZipForge1.Options.StorePath = StorePathMode.RelativePath;
			ZipForge1.BaseDir = "Dest1";
			// Extract all files to Dest1
			ZipForge1.ExtractFiles("*.*");
			// Close the archive
			ZipForge1.CloseArchive();
			MessageBox.Show("All files were added and extracted successfully.");
 
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
