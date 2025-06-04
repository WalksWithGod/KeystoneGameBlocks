namespace KeyEdit
{
    partial class FormCampaignMissionEditor
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listAvailableObjectives = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonAddObjective = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listObjectives = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonRemoveObjective = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(685, 529);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(49, 26);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(630, 529);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(49, 26);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // listAvailableObjectives
            // 
            this.listAvailableObjectives.FormattingEnabled = true;
            this.listAvailableObjectives.Items.AddRange(new object[] {
            "none",
            "destroy",
            "transport"});
            this.listAvailableObjectives.Location = new System.Drawing.Point(12, 90);
            this.listAvailableObjectives.Name = "listAvailableObjectives";
            this.listAvailableObjectives.Size = new System.Drawing.Size(211, 433);
            this.listAvailableObjectives.TabIndex = 8;
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(458, 90);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(288, 433);
            this.listBox2.TabIndex = 9;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(211, 20);
            this.textBox1.TabIndex = 10;
            // 
            // buttonAddObjective
            // 
            this.buttonAddObjective.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAddObjective.Location = new System.Drawing.Point(15, 529);
            this.buttonAddObjective.Name = "buttonAddObjective";
            this.buttonAddObjective.Size = new System.Drawing.Size(91, 26);
            this.buttonAddObjective.TabIndex = 11;
            this.buttonAddObjective.Text = "Add Objective";
            this.buttonAddObjective.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(455, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Entity IDs:";
            // 
            // listObjectives
            // 
            this.listObjectives.FormattingEnabled = true;
            this.listObjectives.Location = new System.Drawing.Point(229, 90);
            this.listObjectives.Name = "listObjectives";
            this.listObjectives.Size = new System.Drawing.Size(223, 433);
            this.listObjectives.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(226, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Mission Objectives:";
            // 
            // buttonRemoveObjective
            // 
            this.buttonRemoveObjective.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRemoveObjective.Location = new System.Drawing.Point(229, 529);
            this.buttonRemoveObjective.Name = "buttonRemoveObjective";
            this.buttonRemoveObjective.Size = new System.Drawing.Size(106, 26);
            this.buttonRemoveObjective.TabIndex = 15;
            this.buttonRemoveObjective.Text = "Remove Objective";
            this.buttonRemoveObjective.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Mission Name:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Available Objectives:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(306, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Player ID:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(365, 32);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(211, 20);
            this.textBox2.TabIndex = 19;
            // 
            // FormCampaignMissionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 564);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonRemoveObjective);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listObjectives);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonAddObjective);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listAvailableObjectives);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "FormCampaignMissionEditor";
            this.Text = "FormCampaignMissionEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListBox listAvailableObjectives;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonAddObjective;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listObjectives;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRemoveObjective;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox2;
    }
}