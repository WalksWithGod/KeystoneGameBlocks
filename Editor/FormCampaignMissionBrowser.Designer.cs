namespace KeyEdit
{
    partial class FormCampaignMissionBrowser
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
            this.listCampaigns = new System.Windows.Forms.ListBox();
            this.listMissions = new System.Windows.Forms.ListBox();
            this.labelCampaigns = new System.Windows.Forms.Label();
            this.labelMissions = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxDescription = new System.Windows.Forms.GroupBox();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.groupBoxDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // listCampaigns
            // 
            this.listCampaigns.FormattingEnabled = true;
            this.listCampaigns.Location = new System.Drawing.Point(2, 29);
            this.listCampaigns.Name = "listCampaigns";
            this.listCampaigns.Size = new System.Drawing.Size(205, 329);
            this.listCampaigns.TabIndex = 0;
            this.listCampaigns.SelectedIndexChanged += new System.EventHandler(this.listCampaigns_SelectedIndexChanged);
            // 
            // listMissions
            // 
            this.listMissions.FormattingEnabled = true;
            this.listMissions.Location = new System.Drawing.Point(213, 29);
            this.listMissions.Name = "listMissions";
            this.listMissions.Size = new System.Drawing.Size(205, 329);
            this.listMissions.TabIndex = 1;
            this.listMissions.SelectedIndexChanged += new System.EventHandler(this.listMissions_SelectedIndexChanged);
            // 
            // labelCampaigns
            // 
            this.labelCampaigns.AutoSize = true;
            this.labelCampaigns.Location = new System.Drawing.Point(12, 9);
            this.labelCampaigns.Name = "labelCampaigns";
            this.labelCampaigns.Size = new System.Drawing.Size(59, 13);
            this.labelCampaigns.TabIndex = 2;
            this.labelCampaigns.Text = "Campaigns";
            // 
            // labelMissions
            // 
            this.labelMissions.AutoSize = true;
            this.labelMissions.Location = new System.Drawing.Point(221, 9);
            this.labelMissions.Name = "labelMissions";
            this.labelMissions.Size = new System.Drawing.Size(47, 13);
            this.labelMissions.TabIndex = 3;
            this.labelMissions.Text = "Missions";
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(553, 332);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(49, 26);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(608, 332);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(49, 26);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxDescription
            // 
            this.groupBoxDescription.Controls.Add(this.textDescription);
            this.groupBoxDescription.Location = new System.Drawing.Point(432, 32);
            this.groupBoxDescription.Name = "groupBoxDescription";
            this.groupBoxDescription.Size = new System.Drawing.Size(235, 294);
            this.groupBoxDescription.TabIndex = 6;
            this.groupBoxDescription.TabStop = false;
            this.groupBoxDescription.Text = "Description";
            // 
            // textDescription
            // 
            this.textDescription.Location = new System.Drawing.Point(9, 17);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(216, 271);
            this.textDescription.TabIndex = 0;
            // 
            // FormCampaignMissionBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 370);
            this.Controls.Add(this.groupBoxDescription);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelMissions);
            this.Controls.Add(this.labelCampaigns);
            this.Controls.Add(this.listMissions);
            this.Controls.Add(this.listCampaigns);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCampaignMissionBrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CampaignMissionBrowser";
            this.Load += new System.EventHandler(this.FormCampaignMissionBrowser_Load);
            this.Shown += new System.EventHandler(this.FormCampaignMissionBrowser_Shown);
            this.groupBoxDescription.ResumeLayout(false);
            this.groupBoxDescription.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listCampaigns;
        private System.Windows.Forms.ListBox listMissions;
        private System.Windows.Forms.Label labelCampaigns;
        private System.Windows.Forms.Label labelMissions;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxDescription;
        private System.Windows.Forms.TextBox textDescription;
    }
}