using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;

namespace MakeSFX
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;
		internal System.Windows.Forms.Button Button3;
		internal System.Windows.Forms.Button Button2;
		internal System.Windows.Forms.Button Button1;
		internal System.Windows.Forms.TextBox TextBox2;
		internal System.Windows.Forms.TextBox TextBox1;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.OpenFileDialog OpenFileDialog1;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.OpenFileDialog OpenFileDialog2;
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
			this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.Button3 = new System.Windows.Forms.Button();
			this.Button2 = new System.Windows.Forms.Button();
			this.Button1 = new System.Windows.Forms.Button();
			this.TextBox2 = new System.Windows.Forms.TextBox();
			this.TextBox1 = new System.Windows.Forms.TextBox();
			this.Label1 = new System.Windows.Forms.Label();
			this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.Label2 = new System.Windows.Forms.Label();
			this.OpenFileDialog2 = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// SaveFileDialog1
			// 
			this.SaveFileDialog1.Filter = "Applications | *.exe";
			// 
			// Button3
			// 
			this.Button3.Location = new System.Drawing.Point(176, 96);
			this.Button3.Name = "Button3";
			this.Button3.Size = new System.Drawing.Size(160, 24);
			this.Button3.TabIndex = 13;
			this.Button3.Text = "Make SFX...";
			this.Button3.Click += new System.EventHandler(this.Button3_Click);
			// 
			// Button2
			// 
			this.Button2.Location = new System.Drawing.Point(408, 56);
			this.Button2.Name = "Button2";
			this.Button2.Size = new System.Drawing.Size(64, 21);
			this.Button2.TabIndex = 12;
			this.Button2.Text = "Browse...";
			this.Button2.Click += new System.EventHandler(this.Button2_Click);
			// 
			// Button1
			// 
			this.Button1.Location = new System.Drawing.Point(408, 24);
			this.Button1.Name = "Button1";
			this.Button1.Size = new System.Drawing.Size(65, 21);
			this.Button1.TabIndex = 11;
			this.Button1.Text = "Browse...";
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			// 
			// TextBox2
			// 
			this.TextBox2.Location = new System.Drawing.Point(128, 56);
			this.TextBox2.Name = "TextBox2";
			this.TextBox2.Size = new System.Drawing.Size(272, 20);
			this.TextBox2.TabIndex = 10;
			this.TextBox2.Text = "";
			// 
			// TextBox1
			// 
			this.TextBox1.Location = new System.Drawing.Point(128, 24);
			this.TextBox1.Name = "TextBox1";
			this.TextBox1.Size = new System.Drawing.Size(272, 20);
			this.TextBox1.TabIndex = 9;
			this.TextBox1.Text = "";
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(16, 24);
			this.Label1.Name = "Label1";
			this.Label1.TabIndex = 7;
			this.Label1.Text = "Archive file name";
			// 
			// OpenFileDialog1
			// 
			this.OpenFileDialog1.DefaultExt = "*.zip";
			this.OpenFileDialog1.Filter = "ZipForge archives | *.zip";
			// 
			// Label2
			// 
			this.Label2.Location = new System.Drawing.Point(16, 64);
			this.Label2.Name = "Label2";
			this.Label2.TabIndex = 8;
			this.Label2.Text = "SFX stub file name";
			// 
			// OpenFileDialog2
			// 
			this.OpenFileDialog2.DefaultExt = "*.exe";
			this.OpenFileDialog2.Filter = "Applications | *.exe";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(504, 125);
			this.Controls.Add(this.TextBox2);
			this.Controls.Add(this.TextBox1);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Button3);
			this.Controls.Add(this.Button2);
			this.Controls.Add(this.Button1);
			this.Name = "Form1";
			this.Text = "MakeSFX demo (C) ComponentAce 2007";
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

		private void Button1_Click(object sender, System.EventArgs e)
		{
			if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
				TextBox1.Text = OpenFileDialog1.FileName;
			}
							   

		}

		private void Button2_Click(object sender, System.EventArgs e)
		{
			if (OpenFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TextBox2.Text = OpenFileDialog2.FileName;
			}
		}

		private void Button3_Click(object sender, System.EventArgs e)
		{
			if (SaveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
			{				
				ZipForge ZipForge1 = new ZipForge();				
				ZipForge1.FileName = TextBox1.Text;
				ZipForge1.SFXStub = TextBox2.Text;
				ZipForge1.MakeSFX(SaveFileDialog1.FileName);
			}

		}
	}
}
