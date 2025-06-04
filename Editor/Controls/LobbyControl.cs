using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using Lidgren.Network;
using KeyCommon.DatabaseEntities;
using KeyCommon.Commands;
using KeyCommon;

namespace KeyEdit
{
    /// <summary>
    /// User interface for the LobbyManager
    /// </summary>
    public partial class LobbyControl : UserControl
    {

        public LobbyControl()
        {
            InitializeComponent();

            //         listviewGames.View = View.LargeIcon;
            //         listviewGames.LargeImageList = imageList1;
            //         listviewGames.MultiSelect = false;
        }

        public void AddItem (ListViewItem item)
        {
           
            listviewGames.Items.Add(item);
        }

        public void RemoveItem()
        {
        }

        public bool ContainsItem(string name)
        {
            return listviewGames.Items.ContainsKey (name);
        }

        protected override void OnResize(EventArgs e)
        {
            //listviewGames.Width = Width - this.Padding.Horizontal;
            //listviewGames.Height = Height - this.Padding.Vertical - 5 - comboBox1.Bottom;
            base.OnResize(e);
        }
    }
}
