namespace KeyEdit
{
    partial class FormSaves
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
            this.textDescription = new System.Windows.Forms.TextBox();
            this.groupBoxDescription = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelSaves = new System.Windows.Forms.Label();
            this.listSaves = new System.Windows.Forms.ListBox();
            this.groupBoxDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // textDescription
            // 
            this.textDescription.Location = new System.Drawing.Point(9, 17);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(216, 271);
            this.textDescription.TabIndex = 0;
            // 
            // groupBoxDescription
            // 
            this.groupBoxDescription.Controls.Add(this.textDescription);
            this.groupBoxDescription.Location = new System.Drawing.Point(231, 32);
            this.groupBoxDescription.Name = "groupBoxDescription";
            this.groupBoxDescription.Size = new System.Drawing.Size(235, 294);
            this.groupBoxDescription.TabIndex = 11;
            this.groupBoxDescription.TabStop = false;
            this.groupBoxDescription.Text = "Description";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(407, 332);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(49, 26);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(352, 332);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(49, 26);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelSaves
            // 
            this.labelSaves.AutoSize = true;
            this.labelSaves.Location = new System.Drawing.Point(20, 9);
            this.labelSaves.Name = "labelSaves";
            this.labelSaves.Size = new System.Drawing.Size(37, 13);
            this.labelSaves.TabIndex = 8;
            this.labelSaves.Text = "Saves";
            // 
            // listSaves
            // 
            this.listSaves.FormattingEnabled = true;
            this.listSaves.Location = new System.Drawing.Point(12, 29);
            this.listSaves.Name = "listSaves";
            this.listSaves.Size = new System.Drawing.Size(213, 381);
            this.listSaves.TabIndex = 7;
            // 
            // FormSaves
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 421);
            this.Controls.Add(this.groupBoxDescription);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelSaves);
            this.Controls.Add(this.listSaves);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSaves";
            this.Text = "FormSaves";
            this.groupBoxDescription.ResumeLayout(false);
            this.groupBoxDescription.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.GroupBox groupBoxDescription;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelSaves;
        private System.Windows.Forms.ListBox listSaves;
    }
}