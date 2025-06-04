using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using IgNS.Controls;

namespace IgSpinEditTest
{
    public partial class TestForm : Form
    {
        IgSpinEdit currCtrl;
        double currVal;
        Rectangle selection;
        Control selectedCtrl = null;

        public TestForm()
        {
            InitializeComponent();

            // You may set custom tooltip
            spedAsInt.SetToolTip("Attention: \"ExternalUpdate\" is initially set for this SpinEdit");

            // Background - feel free to throw this out :)
            Bitmap img = new Bitmap(ClientRectangle.Width, ClientRectangle.Height, CreateGraphics());
            Graphics gr = Graphics.FromImage(img);
            using (
                LinearGradientBrush brush =
                    new LinearGradientBrush(
                        ClientRectangle,
                        Color.LightSkyBlue, //PaleGreen,
                        Color.AliceBlue, 
                        LinearGradientMode.Horizontal))
            {
                brush.SetSigmaBellShape(0.5f);
                gr.FillRectangle(brush, ClientRectangle);
                img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            this.BackgroundImage = img;
        }

        private void spedWide_ValueChanging(object sender, IgNS.Controls.IgSpinEditChangedArgs e)
        {
            currCtrl = (IgSpinEdit)sender;
            currVal = e.Value;
            
            edNewVal.Text = e.Value.ToString();

            e.Cancel = chbxCancel.Checked;
        }

        private void btnExternUpdate_Click(object sender, EventArgs e)
        {
            currCtrl.Value = currVal;
        }

        private void spedWide_Click(object sender, EventArgs e)
        {
            // Set object for property editor
            propGrid.SelectedObject = sender;
            tmrRefresh.Enabled = true;

            // Selection
            selectedCtrl = (Control)sender;
            selection = new Rectangle(selectedCtrl.Left - 4, 
                                                              selectedCtrl.Top - 4, 
                                                              selectedCtrl.Width + 8, 
                                                              selectedCtrl.Height + 8);
            pnl.Refresh();
        }

        private void TestForm_Paint(object sender, PaintEventArgs e)
        {
            if (selectedCtrl != null)
            {
                Graphics gr = pnl.CreateGraphics();
                Pen pen = new Pen(Color.Black);
                gr.DrawRectangle(pen, selection);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            propGrid.Refresh();
        }

        private void propGrid_Enter(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = false;
        }

        private void propGrid_Leave(object sender, EventArgs e)
        {
            if (propGrid.SelectedObject != null)
                tmrRefresh.Enabled = true;
        }

        private void TestForm_Load(object sender, EventArgs e)
        {

        }
    }
}