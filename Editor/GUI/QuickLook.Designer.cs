namespace KeyEdit.GUI
{
    public partial class QuickLook
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
            this.grid = new SourceGrid.Grid();
            this.htmlPanel1 = new HtmlRenderer.HtmlPanel();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.BackColor = System.Drawing.SystemColors.ControlLight;
            this.grid.EnableSort = false;
            this.grid.Location = new System.Drawing.Point(0, 423);
            this.grid.Name = "grid";
            this.grid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.grid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.grid.Size = new System.Drawing.Size(236, 55);
            this.grid.TabIndex = 0;
            this.grid.TabStop = true;
            this.grid.ToolTipText = "";
            // 
            // htmlPanel1
            // 
            this.htmlPanel1.AutoScroll = true;
            this.htmlPanel1.AutoScrollMinSize = new System.Drawing.Size(227, 17);
            this.htmlPanel1.AvoidGeometryAntialias = false;
            this.htmlPanel1.AvoidImagesLateLoading = false;
            this.htmlPanel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.htmlPanel1.BaseStylesheet = null;
            this.htmlPanel1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.htmlPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlPanel1.Location = new System.Drawing.Point(0, 0);
            this.htmlPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.htmlPanel1.Name = "htmlPanel1";
            this.htmlPanel1.Size = new System.Drawing.Size(227, 478);
            this.htmlPanel1.TabIndex = 1;
            this.htmlPanel1.Text = "htmlPanel1";
            // 
            // QuickLook
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.htmlPanel1);
            this.Controls.Add(this.grid);
            this.Name = "QuickLook";
            this.Size = new System.Drawing.Size(227, 478);
            this.ResumeLayout(false);

        }

        #endregion

        protected SourceGrid.Grid grid;
        private HtmlRenderer.HtmlPanel htmlPanel1;

    }
}
