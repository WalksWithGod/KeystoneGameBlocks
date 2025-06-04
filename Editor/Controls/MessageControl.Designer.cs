namespace KeyEdit.Controls
{
    partial class MessageControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.alliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.marketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cboMailbox = new System.Windows.Forms.ToolStripComboBox();
            this.labelCurrent = new System.Windows.Forms.Label();
            this.labelSubject = new System.Windows.Forms.Label();
            this.rtbMessage = new System.Windows.Forms.RichTextBox();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonPrev = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonGoto = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.cboMailbox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(406, 27);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.toolStripSeparator1,
            this.alliesToolStripMenuItem,
            this.foesToolStripMenuItem,
            this.marketToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(45, 23);
            this.toolStripMenuItem1.Text = "Filter";
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.allToolStripMenuItem.Text = "All";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // alliesToolStripMenuItem
            // 
            this.alliesToolStripMenuItem.Name = "alliesToolStripMenuItem";
            this.alliesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.alliesToolStripMenuItem.Text = "Allies";
            // 
            // foesToolStripMenuItem
            // 
            this.foesToolStripMenuItem.Name = "foesToolStripMenuItem";
            this.foesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.foesToolStripMenuItem.Text = "Enemies";
            // 
            // marketToolStripMenuItem
            // 
            this.marketToolStripMenuItem.Name = "marketToolStripMenuItem";
            this.marketToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.marketToolStripMenuItem.Text = "Market";
            // 
            // cboMailbox
            // 
            this.cboMailbox.Items.AddRange(new object[] {
            "Inbox",
            "Drafts",
            "Sent Items"});
            this.cboMailbox.Name = "cboMailbox";
            this.cboMailbox.Size = new System.Drawing.Size(121, 23);
            this.cboMailbox.Text = "Inbox";
            // 
            // labelCurrent
            // 
            this.labelCurrent.AutoSize = true;
            this.labelCurrent.Location = new System.Drawing.Point(372, 0);
            this.labelCurrent.Name = "labelCurrent";
            this.labelCurrent.Size = new System.Drawing.Size(30, 13);
            this.labelCurrent.TabIndex = 1;
            this.labelCurrent.Text = "1 / 1";
            // 
            // labelSubject
            // 
            this.labelSubject.AutoSize = true;
            this.labelSubject.Location = new System.Drawing.Point(200, 11);
            this.labelSubject.Name = "labelSubject";
            this.labelSubject.Size = new System.Drawing.Size(65, 13);
            this.labelSubject.TabIndex = 2;
            this.labelSubject.Text = "RE: Alliance";
            // 
            // rtbMessage
            // 
            this.rtbMessage.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMessage.Location = new System.Drawing.Point(3, 27);
            this.rtbMessage.Name = "rtbMessage";
            this.rtbMessage.Size = new System.Drawing.Size(334, 248);
            this.rtbMessage.TabIndex = 3;
            this.rtbMessage.Text = "Subject: Re: Alliance\n\nHello, do you want to form an alliance?  \n\nRegards,\nLord V" +
                "ader\n";
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(344, 91);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(59, 26);
            this.buttonNext.TabIndex = 4;
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            // 
            // buttonPrev
            // 
            this.buttonPrev.Location = new System.Drawing.Point(343, 27);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Size = new System.Drawing.Size(59, 26);
            this.buttonPrev.TabIndex = 5;
            this.buttonPrev.Text = "Prev";
            this.buttonPrev.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(343, 140);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(59, 26);
            this.buttonDelete.TabIndex = 6;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            // 
            // buttonGoto
            // 
            this.buttonGoto.Location = new System.Drawing.Point(343, 59);
            this.buttonGoto.Name = "buttonGoto";
            this.buttonGoto.Size = new System.Drawing.Size(59, 26);
            this.buttonGoto.TabIndex = 7;
            this.buttonGoto.Text = "Goto";
            this.buttonGoto.UseVisualStyleBackColor = true;
            // 
            // MessageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonGoto);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonPrev);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.rtbMessage);
            this.Controls.Add(this.labelSubject);
            this.Controls.Add(this.labelCurrent);
            this.Controls.Add(this.menuStrip1);
            this.Name = "MessageControl";
            this.Size = new System.Drawing.Size(406, 288);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem alliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem foesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem marketToolStripMenuItem;
        private System.Windows.Forms.Label labelCurrent;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.RichTextBox rtbMessage;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonPrev;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonGoto;
        private System.Windows.Forms.ToolStripComboBox cboMailbox;
    }
}
