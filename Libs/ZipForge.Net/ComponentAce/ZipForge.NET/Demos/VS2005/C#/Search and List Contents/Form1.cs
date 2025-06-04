using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;

namespace Search
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
			this.ZipForge1 = new ComponentAce.Compression.ZipForge.ZipForge();
			this.label1 = new System.Windows.Forms.Label();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnName = new System.Windows.Forms.ColumnHeader();
			this.columnModified = new System.Windows.Forms.ColumnHeader();
			this.columnSize = new System.Windows.Forms.ColumnHeader();
			this.Packed = new System.Windows.Forms.ColumnHeader();
			this.columnRate = new System.Windows.Forms.ColumnHeader();
			this.columnCRC = new System.Windows.Forms.ColumnHeader();
			this.columnPath = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(151, 241);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "Search";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(319, 241);
			this.button2.Name = "button2";
			this.button2.TabIndex = 2;
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
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(462, 33);
			this.label1.TabIndex = 3;
			this.label1.Text = "This demo illustrates how to search files stored inside the archive. Source archi" +
				"ve file is located in Archive\\test.zip";
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnName,
																						this.columnModified,
																						this.columnSize,
																						this.Packed,
																						this.columnRate,
																						this.columnCRC,
																						this.columnPath});
			this.listView1.Location = new System.Drawing.Point(12, 47);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(521, 182);
			this.listView1.TabIndex = 4;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnName
			// 
			this.columnName.Text = "Name";
			this.columnName.Width = 122;
			// 
			// columnModified
			// 
			this.columnModified.Text = "Modified";
			this.columnModified.Width = 118;
			// 
			// columnSize
			// 
			this.columnSize.Text = "Size";
			// 
			// Packed
			// 
			this.Packed.Text = "Packed";
			// 
			// columnRate
			// 
			this.columnRate.Text = "Rate";
			this.columnRate.Width = 39;
			// 
			// columnCRC
			// 
			this.columnCRC.Text = "CRC";
			this.columnCRC.Width = 55;
			// 
			// columnPath
			// 
			this.columnPath.Text = "Path";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(545, 276);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "ZipForge.NET Search demo (c) ComponentAce 2007";
			this.ResumeLayout(false);

		}

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private ZipForge ZipForge1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnModified;
        private System.Windows.Forms.ColumnHeader columnSize;
        private System.Windows.Forms.ColumnHeader Packed;
        private System.Windows.Forms.ColumnHeader columnRate;
        private System.Windows.Forms.ColumnHeader columnCRC;
        private System.Windows.Forms.ColumnHeader columnPath;

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
			ZipForge1.FileName = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\Archive\\test.zip";
			ZipForge1.OpenArchive();			
			ArchiveItem archiveItem = new ArchiveItem();
			if (ZipForge1.FindFirst("*.*", ref archiveItem))
			{
				do
				{
					ListViewItem listItem = new ListViewItem();
					listItem.Text = archiveItem.FileName;
					ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem(listItem, 
						archiveItem.LastWriteTime.ToShortDateString() + " " +
						archiveItem.LastWriteTime.ToShortTimeString());
					listItem.SubItems.Add(subItem);
					subItem = new ListViewItem.ListViewSubItem(listItem,
						archiveItem.UncompressedSize.ToString());
					listItem.SubItems.Add(subItem);                    
					subItem = new ListViewItem.ListViewSubItem(listItem,
						archiveItem.CompressedSize.ToString());
					listItem.SubItems.Add(subItem);
					subItem = new ListViewItem.ListViewSubItem(listItem,
						archiveItem.CompressionRate.ToString());
					listItem.SubItems.Add(subItem);
					subItem = new ListViewItem.ListViewSubItem(listItem,
						((uint)archiveItem.CRC).ToString());
					listItem.SubItems.Add(subItem);
					subItem = new ListViewItem.ListViewSubItem(listItem,
						archiveItem.StoredPath);
					listItem.SubItems.Add(subItem);                    
					listView1.Items.Add(listItem);
				}
				while (ZipForge1.FindNext(ref archiveItem));
			}
			ZipForge1.CloseArchive();
 
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
