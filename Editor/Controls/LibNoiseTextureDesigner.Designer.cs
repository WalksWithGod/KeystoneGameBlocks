namespace KeyEdit.Controls
{
    partial class LibNoiseTextureDesigner
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LibNoiseTextureDesigner));
            this.diagramView = new MindFusion.Diagramming.Diagram();
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.diagramView1 = new MindFusion.Diagramming.WinForms.DiagramView();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // images
            // 
            this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
            this.images.TransparentColor = System.Drawing.Color.LightBlue;
            this.images.Images.SetKeyName(0, "datatype_float.png");
            this.images.Images.SetKeyName(1, "1.png");
            this.images.Images.SetKeyName(2, "table.png");
            this.images.Images.SetKeyName(3, "datatype_int.png");
            this.images.Images.SetKeyName(4, "");
            this.images.Images.SetKeyName(5, "");
            this.images.Images.SetKeyName(6, "");
            this.images.Images.SetKeyName(7, "");
            this.images.Images.SetKeyName(8, "");
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.diagramView1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.pictureBox);
            this.splitContainer.Size = new System.Drawing.Size(1070, 588);
            this.splitContainer.SplitterDistance = 297;
            this.splitContainer.TabIndex = 1;
            // 
            // diagramView1
            // 
            this.diagramView1.Behavior = MindFusion.Diagramming.Behavior.LinkShapes;
            this.diagramView1.ControlHandlesStyle = MindFusion.Diagramming.HandlesStyle.HatchHandles;
            this.diagramView1.ControlMouseAction = MindFusion.Diagramming.ControlMouseAction.SelectNode;
            this.diagramView1.DelKeyAction = MindFusion.Diagramming.DelKeyAction.DeleteSelectedItems;
            this.diagramView1.Diagram = this.diagramView;
            this.diagramView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagramView1.Location = new System.Drawing.Point(0, 0);
            this.diagramView1.MiddleButtonActions = MindFusion.Diagramming.MouseButtonActions.None;
            this.diagramView1.ModificationStart = MindFusion.Diagramming.ModificationStart.SelectedOnly;
            this.diagramView1.Name = "diagramView1";
            this.diagramView1.RightButtonActions = MindFusion.Diagramming.MouseButtonActions.Cancel;
            this.diagramView1.Size = new System.Drawing.Size(1070, 297);
            this.diagramView1.TabIndex = 1;
            this.diagramView1.Text = "diagramView";
            // 
            // pictureBox
            // 
            this.pictureBox.AccessibleName = "Generated Texture";
            this.pictureBox.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(3, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(1063, 280);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 2;
            this.pictureBox.TabStop = false;
            // 
            // LibNoiseTextureDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "LibNoiseTextureDesigner";
            this.Size = new System.Drawing.Size(1070, 588);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MindFusion.Diagramming.Diagram diagramView;
        internal System.Windows.Forms.ImageList images;
        private System.Windows.Forms.SplitContainer splitContainer;
        private MindFusion.Diagramming.WinForms.DiagramView diagramView1;
        private System.Windows.Forms.PictureBox pictureBox;
    }
}
