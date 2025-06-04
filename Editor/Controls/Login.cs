using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit.Controls
{
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }


        protected override void OnResize(EventArgs e)
        {
           // listviewGames.Width = Width - this.Padding.Horizontal;
           // listviewGames.Height = Height - this.Padding.Vertical - 5 - comboBox1.Bottom;
            base.OnResize(e);
        }
    }
}
