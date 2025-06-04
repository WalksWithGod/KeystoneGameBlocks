namespace KeyPluginCnC
{
    partial class EntityPluginCnC
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.stOfficerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tacticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scienceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setCourseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableTargetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attackTargetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interceptTargetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.communicationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.surveyWorldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtbComms = new System.Windows.Forms.RichTextBox();
            this.dockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stOfficerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(605, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // stOfficerToolStripMenuItem
            // 
            this.stOfficerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helmToolStripMenuItem,
            this.tacticalToolStripMenuItem,
            this.communicationsToolStripMenuItem,
            this.scienceToolStripMenuItem});
            this.stOfficerToolStripMenuItem.Name = "stOfficerToolStripMenuItem";
            this.stOfficerToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.stOfficerToolStripMenuItem.Text = "C && C";
            // 
            // helmToolStripMenuItem
            // 
            this.helmToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setCourseToolStripMenuItem,
            this.interceptTargetToolStripMenuItem,
            this.dockToolStripMenuItem});
            this.helmToolStripMenuItem.Name = "helmToolStripMenuItem";
            this.helmToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.helmToolStripMenuItem.Text = "Helm";
            // 
            // tacticalToolStripMenuItem
            // 
            this.tacticalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scanToolStripMenuItem,
            this.disableTargetToolStripMenuItem,
            this.attackTargetToolStripMenuItem});
            this.tacticalToolStripMenuItem.Name = "tacticalToolStripMenuItem";
            this.tacticalToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.tacticalToolStripMenuItem.Text = "Tactical";
            // 
            // scienceToolStripMenuItem
            // 
            this.scienceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.surveyWorldToolStripMenuItem});
            this.scienceToolStripMenuItem.Name = "scienceToolStripMenuItem";
            this.scienceToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.scienceToolStripMenuItem.Text = "Science";
            // 
            // setCourseToolStripMenuItem
            // 
            this.setCourseToolStripMenuItem.Name = "setCourseToolStripMenuItem";
            this.setCourseToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.setCourseToolStripMenuItem.Text = "Set Course";
            this.setCourseToolStripMenuItem.Click += new System.EventHandler(this.setCourseToolStripMenuItem_Click);
            // 
            // disableTargetToolStripMenuItem
            // 
            this.disableTargetToolStripMenuItem.Name = "disableTargetToolStripMenuItem";
            this.disableTargetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.disableTargetToolStripMenuItem.Text = "Disable Target";
            // 
            // attackTargetToolStripMenuItem
            // 
            this.attackTargetToolStripMenuItem.Name = "attackTargetToolStripMenuItem";
            this.attackTargetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.attackTargetToolStripMenuItem.Text = "Attack Target";
            this.attackTargetToolStripMenuItem.Click += new System.EventHandler(this.attackTargetToolStripMenuItem_Click);
            // 
            // interceptTargetToolStripMenuItem
            // 
            this.interceptTargetToolStripMenuItem.Name = "interceptTargetToolStripMenuItem";
            this.interceptTargetToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.interceptTargetToolStripMenuItem.Text = "Intercept Target";
            // 
            // scanToolStripMenuItem
            // 
            this.scanToolStripMenuItem.Name = "scanToolStripMenuItem";
            this.scanToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.scanToolStripMenuItem.Text = "Scan";
            this.scanToolStripMenuItem.Click += new System.EventHandler(this.scanToolStripMenuItem_Click);
            // 
            // communicationsToolStripMenuItem
            // 
            this.communicationsToolStripMenuItem.Name = "communicationsToolStripMenuItem";
            this.communicationsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.communicationsToolStripMenuItem.Text = "Communications";
            // 
            // surveyWorldToolStripMenuItem
            // 
            this.surveyWorldToolStripMenuItem.Name = "surveyWorldToolStripMenuItem";
            this.surveyWorldToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.surveyWorldToolStripMenuItem.Text = "Survey World";
            // 
            // rtbComms
            // 
            this.rtbComms.Location = new System.Drawing.Point(3, 124);
            this.rtbComms.Name = "rtbComms";
            this.rtbComms.Size = new System.Drawing.Size(599, 154);
            this.rtbComms.TabIndex = 3;
            this.rtbComms.Text = "";
            // 
            // dockToolStripMenuItem
            // 
            this.dockToolStripMenuItem.Name = "dockToolStripMenuItem";
            this.dockToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.dockToolStripMenuItem.Text = "Dock";
            // 
            // EntityPluginCnC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rtbComms);
            this.Controls.Add(this.menuStrip1);
            this.Name = "EntityPluginCnC";
            this.Size = new System.Drawing.Size(605, 281);
            this.Load += new System.EventHandler(this.EntityPluginCnC_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem stOfficerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tacticalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scienceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setCourseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem interceptTargetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableTargetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attackTargetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem communicationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem surveyWorldToolStripMenuItem;
        private System.Windows.Forms.RichTextBox rtbComms;
        private System.Windows.Forms.ToolStripMenuItem dockToolStripMenuItem;
    }
}

