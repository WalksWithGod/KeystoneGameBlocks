using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;

namespace TransactionSystem
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
			this.Label1 = new System.Windows.Forms.Label();
			this.Start = new System.Windows.Forms.Button();
			this.Exit = new System.Windows.Forms.Button();
			this.ZipForge1 = new ComponentAce.Compression.ZipForge.ZipForge();
			this.SuspendLayout();
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(10, 14);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(457, 40);
			this.Label1.TabIndex = 3;
			this.Label1.Text = "This demo illustrates how to use a transaction system. Source files are located i" +
				"n Source and Source1 folder. Destination archive file will be located in Archive" +
				"\\test.zip. Files will be extracted to folder Dest.";
			// 
			// Start
			// 
			this.Start.BackColor = System.Drawing.SystemColors.Control;
			this.Start.Location = new System.Drawing.Point(122, 70);
			this.Start.Name = "Start";
			this.Start.Size = new System.Drawing.Size(75, 25);
			this.Start.TabIndex = 2;
			this.Start.Text = "Start";
			this.Start.Click += new System.EventHandler(this.Start_Click);
			// 
			// Exit
			// 
			this.Exit.BackColor = System.Drawing.SystemColors.Control;
			this.Exit.Location = new System.Drawing.Point(242, 70);
			this.Exit.Name = "Exit";
			this.Exit.Size = new System.Drawing.Size(75, 25);
			this.Exit.TabIndex = 4;
			this.Exit.Text = "Exit";
			this.Exit.Click += new System.EventHandler(this.Exit_Click);
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
			this.ClientSize = new System.Drawing.Size(477, 109);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Start);
			this.Controls.Add(this.Exit);
			this.Name = "Form1";
			this.Text = "ZipForge.NET Transaction System demo (c) ComponentAce 2007";
			this.ResumeLayout(false);

		}
		#endregion

		public System.Windows.Forms.Label Label1;
		public System.Windows.Forms.Button Start;
		public System.Windows.Forms.Button Exit;
		private ZipForge ZipForge1;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Start_Click(object sender, System.EventArgs e)
		{
			string DemoPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\";
			ZipForge1.FileName = DemoPath + "Archive\\test.zip";
			// Create a new archive file            
			ZipForge1.OpenArchive(System.IO.FileMode.Create);
			// Start a transaction
			ZipForge1.BeginUpdate();
			// Set path to folder with some HTML files to BaseDir
			ZipForge1.BaseDir = DemoPath + "Source";
			// Add all files from Source folder to the archive
			try
			{
				ZipForge1.AddFiles("*.*");
			}
			catch
			{
				// If errors occurs rollback transaction. All modifications will be cancelled.            
				ZipForge1.CancelUpdate();
				// Close archive and exit current procedure                
				ZipForge1.CloseArchive();
				MessageBox.Show("Error adding all files");
				return;
			}
			// Set path to folder with some HTML files to BaseDir            
			ZipForge1.BaseDir = DemoPath + "Source1\\";
			// Add all HTML files from Source1 folder to the archive
			try
			{
				ZipForge1.AddFiles("*.htm*");
			}
			catch
			{
				// If errors occurs rollback transaction. All modifications will be cancelled.                
				ZipForge1.CancelUpdate();
				// Close archive and exit current procedure                
				ZipForge1.CloseArchive();
				MessageBox.Show("Error adding html files");
				return;
			}
			// Commit a transaction. All modifications will be saved.            
			ZipForge1.EndUpdate();
			// Set path to destination folder            
			ZipForge1.BaseDir = DemoPath + "Dest";
			// Extract all files            
			ZipForge1.ExtractFiles("*.*");
			// Close the archive            
			ZipForge1.CloseArchive();
			MessageBox.Show("All files were added and extracted successfully.");
 
		}

		private void Exit_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
