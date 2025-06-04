namespace KeyEdit
{
    partial class FormPleaseWait
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
            this.pictureWaitImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureWaitImage)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureWaitImage
            // 
            this.pictureWaitImage.Image = global::KeyEdit.Properties.Resources.bar_circle;
            this.pictureWaitImage.Location = new System.Drawing.Point(116, 27);
            this.pictureWaitImage.Name = "pictureWaitImage";
            this.pictureWaitImage.Size = new System.Drawing.Size(50, 48);
            this.pictureWaitImage.TabIndex = 0;
            this.pictureWaitImage.TabStop = false;
            // 
            // FormPleaseWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 102);
            this.ControlBox = false;
            this.Controls.Add(this.pictureWaitImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormPleaseWait";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Please Wait";
            ((System.ComponentModel.ISupportInitialize)(this.pictureWaitImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureWaitImage;
    }
}