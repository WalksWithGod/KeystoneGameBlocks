namespace ThumbnailViewerDemo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.fileSystemTreeView1 = new FileExplorerTreeDotnet.FileSystemTreeView();
            this.mImageBrowser = new ImageBrowserDotnet.ImageBrowser();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.trackBarSize = new System.Windows.Forms.TrackBar();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.labelThumbnailSize = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(-1, 57);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.fileSystemTreeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mImageBrowser);
            this.splitContainer1.Size = new System.Drawing.Size(656, 445);
            this.splitContainer1.SplitterDistance = 218;
            this.splitContainer1.TabIndex = 0;
            // 
            // fileSystemTreeView1
            // 
            this.fileSystemTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileSystemTreeView1.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileSystemTreeView1.ImageIndex = 0;
            this.fileSystemTreeView1.Location = new System.Drawing.Point(0, 0);
            this.fileSystemTreeView1.Name = "fileSystemTreeView1";
            this.fileSystemTreeView1.SelectedImageIndex = 0;
            this.fileSystemTreeView1.ShowFiles = true;
            this.fileSystemTreeView1.Size = new System.Drawing.Size(218, 445);
            this.fileSystemTreeView1.TabIndex = 1;
            this.fileSystemTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileSystemTreeView1_AfterSelect);
            // 
            // mImageBrowser
            // 
            this.mImageBrowser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mImageBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mImageBrowser.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mImageBrowser.ImageSize = 64;
            this.mImageBrowser.ImageViewerEnabled = true;
            this.mImageBrowser.Location = new System.Drawing.Point(0, 0);
            this.mImageBrowser.Name = "mImageBrowser";
            this.mImageBrowser.RecurseSubFolders = false;
            this.mImageBrowser.Size = new System.Drawing.Size(434, 445);
            this.mImageBrowser.TabIndex = 0;
            this.mImageBrowser.OnImageClicked += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnImageClicked);
            this.mImageBrowser.OnBrowsingCanceled += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnBrowsingCanceled);
            this.mImageBrowser.OnBrowsingFoundImage += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnBrowsingFoundImage);
            this.mImageBrowser.OnBrowsingStarted += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnBrowsingStarted);
            this.mImageBrowser.OnBrowsingEnded += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnBrowsingEnded);
            this.mImageBrowser.OnBrowsingAborted += new ImageBrowserDotnet.ImageBrowserEventHandler(this.mImageBrowser_OnBrowsingAborted);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(222, 20);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(79, 27);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // trackBarSize
            // 
            this.trackBarSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarSize.LargeChange = 1;
            this.trackBarSize.Location = new System.Drawing.Point(448, 6);
            this.trackBarSize.Name = "trackBarSize";
            this.trackBarSize.Size = new System.Drawing.Size(203, 45);
            this.trackBarSize.TabIndex = 2;
            this.trackBarSize.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarSize.ValueChanged += new System.EventHandler(this.trackBarSize_ValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(-1, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(204, 26);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.Text = "common\\\\resources.zip";
            // 
            // labelThumbnailSize
            // 
            this.labelThumbnailSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelThumbnailSize.AutoSize = true;
            this.labelThumbnailSize.Location = new System.Drawing.Point(356, 23);
            this.labelThumbnailSize.Name = "labelThumbnailSize";
            this.labelThumbnailSize.Size = new System.Drawing.Size(86, 18);
            this.labelThumbnailSize.TabIndex = 4;
            this.labelThumbnailSize.Text = "Image Size:";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(652, 498);
            this.Controls.Add(this.labelThumbnailSize);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.trackBarSize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Text = "Asset Browser";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label labelThumbnailSize;

        private ImageBrowserDotnet.ImageBrowser mImageBrowser;
        private FileExplorerTreeDotnet.FileSystemTreeView fileSystemTreeView1;
    }
}

