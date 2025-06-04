namespace KeyEdit.Controls
{
    partial class ViewportNavigationControl
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
            this.buttonSelect = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.labelSelectedContainer = new DevComponents.DotNetBar.LabelItem();
            this.cboVehicleSelect = new DevComponents.DotNetBar.ComboBoxItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(0, 28);
            this.pictureBox.Size = new System.Drawing.Size(775, 455);
            // 
            // ribbonBar
            // 
            // 
            // 
            // 
            this.ribbonBar.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonSelect,
            this.buttonItem1,
            this.labelSelectedContainer,
            this.cboVehicleSelect});
            this.ribbonBar.Size = new System.Drawing.Size(775, 28);
            // 
            // 
            // 
            this.ribbonBar.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Image = global::KeyEdit.Properties.Resources.round_arrow_left_icon_16;
            this.buttonSelect.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonSelect.ImagePaddingHorizontal = 0;
            this.buttonSelect.ImagePaddingVertical = 0;
            this.buttonSelect.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.RibbonWordWrap = false;
            this.buttonSelect.SubItemsExpandWidth = 14;
            this.buttonSelect.Text = "buttonSelect";
            this.buttonSelect.Tooltip = "Select";
            this.buttonSelect.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonItem1
            // 
            this.buttonItem1.Image = global::KeyEdit.Properties.Resources.round_arrow_right_icon_16;
            this.buttonItem1.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonItem1.ImagePaddingHorizontal = 0;
            this.buttonItem1.ImagePaddingVertical = 0;
            this.buttonItem1.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.RibbonWordWrap = false;
            this.buttonItem1.SubItemsExpandWidth = 14;
            this.buttonItem1.Text = "buttonSelect";
            this.buttonItem1.Tooltip = "Select";
            this.buttonItem1.Click += new System.EventHandler(this.buttonForward_Click);
            // 
            // labelSelectedContainer
            // 
            this.labelSelectedContainer.Name = "labelSelectedContainer";
            this.labelSelectedContainer.Text = "Selected:";
            // 
            // cboVehicleSelect
            // 
            this.cboVehicleSelect.ComboWidth = 128;
            this.cboVehicleSelect.DropDownHeight = 106;
            this.cboVehicleSelect.DropDownWidth = 128;
            this.cboVehicleSelect.Name = "cboVehicleSelect";
            this.cboVehicleSelect.Click += new System.EventHandler(this.cboVehicleSelect_Click);
            // 
            // ViewportNavigationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ViewportNavigationControl";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonItem buttonSelect;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.LabelItem labelSelectedContainer;
        private DevComponents.DotNetBar.ComboBoxItem cboVehicleSelect;
    }
}
