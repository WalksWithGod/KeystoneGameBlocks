using Open.Diagramming;
using Open.Diagramming.Forms;

namespace Open.Diagramming.Testing
{
    partial class frmPath
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
            this.diagram1 = new Open.Diagramming.Forms.Diagram();
            this.SuspendLayout();
            // 
            // diagram1
            // 
            this.diagram1.AutoScroll = true;
            this.diagram1.AutoScrollMinSize = new System.Drawing.Size(540, 540);
            this.diagram1.BackColor = System.Drawing.SystemColors.Window;
            this.diagram1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagram1.DragElement = null;
            this.diagram1.Feedback = true;
            this.diagram1.GridColor = System.Drawing.Color.Silver;
            this.diagram1.GridSize = new System.Drawing.Size(20, 20);
            this.diagram1.Location = new System.Drawing.Point(0, 0);
            this.diagram1.Name = "diagram1";
            this.diagram1.Size = new System.Drawing.Size(740, 551);
            this.diagram1.TabIndex = 0;
            this.diagram1.Zoom = 100F;
            // 
            // frmPath
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 551);
            this.Controls.Add(this.diagram1);
            this.Name = "frmPath";
            this.Text = "Port Path";
            this.ResumeLayout(false);

        }

        #endregion

        private Diagram diagram1;

    }
}

