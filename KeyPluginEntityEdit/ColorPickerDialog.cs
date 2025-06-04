using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyPluginEntityEdit
{
    public partial class ColorPickerDialog : Form
    {
        public System.Drawing.Color ColorInitial;

        public ColorPickerDialog()
        {
            InitializeComponent();
        }
         
        public Color SelectedColor
        {
            get { return colorPickerCtrl1.SelectedColor; }
            set { colorPickerCtrl1.SelectedColor = value; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
