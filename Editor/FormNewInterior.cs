using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormNewInterior : Form
    {
        public Keystone.Types.Vector3d CellSize;
        public uint QuadtreeDepth  = 0;

        public FormNewInterior()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            CellSize.x = (double)numCellWidth.Value;
            CellSize.y = (double)numCellHeight.Value;
            CellSize.z = (double)numCellDepth.Value;
            QuadtreeDepth = (uint)numQuadtreeDepth.Value;

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void numCellWidth_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
