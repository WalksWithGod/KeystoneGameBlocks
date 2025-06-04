namespace KeyPluginEntityEdit2
{
    partial class PlaybackControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaybackControl));
            this.buttonPlay = new System.Windows.Forms.Button();
            this.slider = new DevComponents.DotNetBar.Controls.Slider();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonFrameReverse = new System.Windows.Forms.Button();
            this.buttonFrameAdvance = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonPlay
            // 
            this.buttonPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPlay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonPlay.BackgroundImage")));
            this.buttonPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonPlay.FlatAppearance.BorderSize = 0;
            this.buttonPlay.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.buttonPlay.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlay.Location = new System.Drawing.Point(120, 1);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(39, 34);
            this.buttonPlay.TabIndex = 0;
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // slider
            // 
            this.slider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // 
            // 
            this.slider.BackgroundStyle.Class = "";
            this.slider.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.slider.DecreaseTooltip = "Reverse one frame";
            this.slider.FocusCuesEnabled = false;
            this.slider.IncreaseTooltip = "Advance one frame";
            this.slider.LabelPosition = DevComponents.DotNetBar.eSliderLabelPosition.Bottom;
            this.slider.Location = new System.Drawing.Point(204, 6);
            this.slider.Name = "slider";
            this.slider.Size = new System.Drawing.Size(268, 34);
            this.slider.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.slider.TabIndex = 1;
            this.slider.Text = "Frame 0/0";
            this.slider.Value = 0;
            this.slider.ValueChanged += new System.EventHandler(this.slider_ValueChanged);
            // 
            // buttonPause
            // 
            this.buttonPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPause.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonPause.BackgroundImage")));
            this.buttonPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonPause.FlatAppearance.BorderSize = 0;
            this.buttonPause.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.buttonPause.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPause.Location = new System.Drawing.Point(81, 1);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(39, 34);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonStop.BackgroundImage")));
            this.buttonStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonStop.FlatAppearance.BorderSize = 0;
            this.buttonStop.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.buttonStop.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Location = new System.Drawing.Point(42, 1);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(39, 34);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonFrameReverse
            // 
            this.buttonFrameReverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFrameReverse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonFrameReverse.BackgroundImage")));
            this.buttonFrameReverse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonFrameReverse.FlatAppearance.BorderSize = 0;
            this.buttonFrameReverse.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.buttonFrameReverse.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonFrameReverse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFrameReverse.Location = new System.Drawing.Point(3, 1);
            this.buttonFrameReverse.Name = "buttonFrameReverse";
            this.buttonFrameReverse.Size = new System.Drawing.Size(39, 34);
            this.buttonFrameReverse.TabIndex = 4;
            this.buttonFrameReverse.UseVisualStyleBackColor = true;
            this.buttonFrameReverse.Click += new System.EventHandler(this.buttonFrameReverse_Click);
            // 
            // buttonFrameAdvance
            // 
            this.buttonFrameAdvance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFrameAdvance.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonFrameAdvance.BackgroundImage")));
            this.buttonFrameAdvance.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonFrameAdvance.FlatAppearance.BorderSize = 0;
            this.buttonFrameAdvance.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.buttonFrameAdvance.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonFrameAdvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFrameAdvance.Location = new System.Drawing.Point(159, 1);
            this.buttonFrameAdvance.Name = "buttonFrameAdvance";
            this.buttonFrameAdvance.Size = new System.Drawing.Size(39, 34);
            this.buttonFrameAdvance.TabIndex = 5;
            this.buttonFrameAdvance.UseVisualStyleBackColor = true;
            this.buttonFrameAdvance.Click += new System.EventHandler(this.buttonFrameAdvance_Click);
            // 
            // PlaybackControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.buttonFrameAdvance);
            this.Controls.Add(this.buttonFrameReverse);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.slider);
            this.Name = "PlaybackControl";
            this.Size = new System.Drawing.Size(475, 40);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonPlay;
        private DevComponents.DotNetBar.Controls.Slider slider;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonFrameReverse;
        private System.Windows.Forms.Button buttonFrameAdvance;
    }
}
