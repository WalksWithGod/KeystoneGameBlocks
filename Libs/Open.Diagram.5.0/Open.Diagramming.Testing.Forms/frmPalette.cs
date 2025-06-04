using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Open.Diagramming;
using Open.Diagramming.Forms;

namespace Open.Diagramming.Testing
{
    public partial class frmPalette : Form
    {
        public frmPalette()
        {
            InitializeComponent();

            palette1.AddStencil(Singleton.Instance.GetStencil(typeof(BasicStencil)));

        }

        private void palette1_Load(object sender, EventArgs e)
        {

        }
    }
}