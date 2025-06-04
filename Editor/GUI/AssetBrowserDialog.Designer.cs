namespace KeyEdit.GUI
{
    partial class AssetBrowserDialog
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textFilename = new System.Windows.Forms.TextBox();
            this.labelFilename = new System.Windows.Forms.Label();
            this.assetBrowserControl = new KeyEdit.GUI.AssetBrowserControl();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(583, 534);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(88, 30);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(489, 534);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 30);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textFilename
            // 
            this.textFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textFilename.Location = new System.Drawing.Point(70, 540);
            this.textFilename.Name = "textFilename";
            this.textFilename.Size = new System.Drawing.Size(256, 20);
            this.textFilename.TabIndex = 6;
            this.textFilename.TextChanged += new System.EventHandler(this.textFilename_TextChanged);
            // 
            // labelFilename
            // 
            this.labelFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFilename.AutoSize = true;
            this.labelFilename.Location = new System.Drawing.Point(12, 543);
            this.labelFilename.Name = "labelFilename";
            this.labelFilename.Size = new System.Drawing.Size(52, 13);
            this.labelFilename.TabIndex = 7;
            this.labelFilename.Text = "Filename:";
            // 
            // assetBrowserControl
            // 
            this.assetBrowserControl.AllowedExtensions = null;
            this.assetBrowserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.assetBrowserControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assetBrowserControl.ImageList = null;
            this.assetBrowserControl.ImageSize = 64;
            this.assetBrowserControl.Location = new System.Drawing.Point(-2, -2);
            this.assetBrowserControl.MinimumSize = new System.Drawing.Size(400, 2);
            this.assetBrowserControl.Name = "assetBrowserControl";
            this.assetBrowserControl.RecurseSubFolders = false;
            this.assetBrowserControl.SelectedPath = null;
            this.assetBrowserControl.SelecteFile = "";
            this.assetBrowserControl.ShowTreeView = true;
            this.assetBrowserControl.Size = new System.Drawing.Size(685, 520);
            this.assetBrowserControl.TabIndex = 8;
            this.assetBrowserControl.ToolbarVisible = false;
            // 
            // AssetBrowserDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 571);
            this.Controls.Add(this.assetBrowserControl);
            this.Controls.Add(this.labelFilename);
            this.Controls.Add(this.textFilename);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssetBrowserDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Save As...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textFilename;
        private System.Windows.Forms.Label labelFilename;
        private AssetBrowserControl assetBrowserControl;

    }
}