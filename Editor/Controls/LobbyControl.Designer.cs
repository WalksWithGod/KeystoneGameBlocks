namespace KeyEdit
{
    partial class LobbyControl
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
            this.label3 = new System.Windows.Forms.Label();
            this.listviewGames = new System.Windows.Forms.ListView();
            this.headerCountry = new System.Windows.Forms.ColumnHeader();
            this.headerName = new System.Windows.Forms.ColumnHeader();
            this.headerIP = new System.Windows.Forms.ColumnHeader();
            this.headerPing = new System.Windows.Forms.ColumnHeader();
            this.headerPrivate = new System.Windows.Forms.ColumnHeader();
            this.headerPlayerCount = new System.Windows.Forms.ColumnHeader();
            this.headerMap = new System.Windows.Forms.ColumnHeader();
            this.headerGame = new System.Windows.Forms.ColumnHeader();
            this.headerGameType = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-146, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 27;
            this.label3.Text = "Filter";
            // 
            // listviewGames
            // 
            this.listviewGames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerCountry,
            this.headerName,
            this.headerIP,
            this.headerPing,
            this.headerPrivate,
            this.headerPlayerCount,
            this.headerMap,
            this.headerGame,
            this.headerGameType});
            this.listviewGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listviewGames.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listviewGames.FullRowSelect = true;
            this.listviewGames.GridLines = true;
            this.listviewGames.Location = new System.Drawing.Point(0, 0);
            this.listviewGames.MultiSelect = false;
            this.listviewGames.Name = "listviewGames";
            this.listviewGames.Size = new System.Drawing.Size(743, 442);
            this.listviewGames.TabIndex = 28;
            this.listviewGames.UseCompatibleStateImageBehavior = false;
            this.listviewGames.View = System.Windows.Forms.View.Details;
            // 
            // headerCountry
            // 
            this.headerCountry.DisplayIndex = 8;
            this.headerCountry.Text = "Country";
            // 
            // headerName
            // 
            this.headerName.DisplayIndex = 0;
            this.headerName.Text = "Name";
            this.headerName.Width = 145;
            // 
            // headerIP
            // 
            this.headerIP.DisplayIndex = 1;
            this.headerIP.Text = "Address";
            this.headerIP.Width = 126;
            // 
            // headerPing
            // 
            this.headerPing.DisplayIndex = 2;
            this.headerPing.Text = "Ping";
            this.headerPing.Width = 43;
            // 
            // headerPrivate
            // 
            this.headerPrivate.DisplayIndex = 3;
            this.headerPrivate.Text = "Priv";
            this.headerPrivate.Width = 39;
            // 
            // headerPlayerCount
            // 
            this.headerPlayerCount.DisplayIndex = 4;
            this.headerPlayerCount.Text = "# Players";
            this.headerPlayerCount.Width = 81;
            // 
            // headerMap
            // 
            this.headerMap.DisplayIndex = 5;
            this.headerMap.Text = "Map";
            this.headerMap.Width = 109;
            // 
            // headerGame
            // 
            this.headerGame.DisplayIndex = 6;
            this.headerGame.Text = "Game";
            this.headerGame.Width = 73;
            // 
            // headerGameType
            // 
            this.headerGameType.DisplayIndex = 7;
            this.headerGameType.Text = "GameType";
            this.headerGameType.Width = 83;
            // 
            // LobbyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listviewGames);
            this.Controls.Add(this.label3);
            this.Name = "LobbyControl";
            this.Size = new System.Drawing.Size(743, 442);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listviewGames;
        private System.Windows.Forms.ColumnHeader headerName;
        private System.Windows.Forms.ColumnHeader headerIP;
        private System.Windows.Forms.ColumnHeader headerPing;
        private System.Windows.Forms.ColumnHeader headerPrivate;
        private System.Windows.Forms.ColumnHeader headerPlayerCount;
        private System.Windows.Forms.ColumnHeader headerMap;
        private System.Windows.Forms.ColumnHeader headerGame;
        private System.Windows.Forms.ColumnHeader headerGameType;
        private System.Windows.Forms.ColumnHeader headerCountry;
    }
}
