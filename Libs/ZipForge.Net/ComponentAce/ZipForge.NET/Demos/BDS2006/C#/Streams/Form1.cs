using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using ComponentAce.Compression.ZipForge;
using ComponentAce.Compression.Archiver;

namespace Streams
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
			this.ZipForge1 = new ZipForge();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(13, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(509, 42);
			this.label1.TabIndex = 0;
			this.label1.Text = "This demo shows how to create and open an archive in a stream, and how to add and" +
				" exctract archived files to streams.";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(139, 59);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "Start";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(305, 59);
			this.button2.Name = "button2";
			this.button2.TabIndex = 2;
			this.button2.Text = "Exit";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// ZipForge1
			// 
			this.ZipForge1.Active = false;
			this.ZipForge1.BaseDir = "";
			this.ZipForge1.CompressionLevel = CompressionLevel.Fastest;
			this.ZipForge1.CompressionMode = ((System.Byte)(1));
			this.ZipForge1.ExtractCorruptedFiles = false;
			this.ZipForge1.FileName = null;
			this.ZipForge1.InMemory = false;
			this.ZipForge1.OpenCorruptedArchives = true;
			this.ZipForge1.Password = "";
			this.ZipForge1.SFXStub = null;
			this.ZipForge1.SpanningMode = SpanningMode.None;
			this.ZipForge1.TempDir = null;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(519, 94);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "ZipForge.NET Streams demo (c) ComponentAce 2007";
			this.ResumeLayout(false);

		}
		#endregion

		
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private 
ZipForge ZipForge1;
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
			System.IO.MemoryStream archiveStream = new System.IO.MemoryStream();
			System.IO.MemoryStream itemStream = new System.IO.MemoryStream();
			// Create archive in stream
			ZipForge1.OpenArchive(archiveStream, true);
			// Add item from stream            
			itemStream.Write(new byte[] {1,2,3,4}, 0, 4);
			ZipForge1.AddFromStream("testfile.txt", itemStream);
			// Close arhive
			ZipForge1.CloseArchive();
			// Open archive in stream
			ZipForge1.OpenArchive(archiveStream, false);
			// extract item to stream
			itemStream = null;
			itemStream = new System.IO.MemoryStream();
			ZipForge1.ExtractToStream("testfile.txt", itemStream);
			// Close archive
			ZipForge1.CloseArchive();
			MessageBox.Show("All files were added and extracted successfully");
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
