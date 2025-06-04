using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormNewVehicle : Form
    {

        float CellWidth = 1.0f;
        float CellHeight = 1.0f;
        float CellDepth = 1.0f;
        public string ExteriorMeshPath;
        public uint CellCountX = 1;
        public uint CellCountY = 1;
        public uint CellCountZ = 1;
        public decimal MaxCost = 1000000000;
        public uint MaxTechLevel = 8;

        public FormNewVehicle(float cellWidth, float cellHeight, float cellDepth)
        {
            InitializeComponent();

            CellWidth = cellWidth;
            CellHeight = cellHeight;
            CellDepth = cellDepth;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            CellCountX = uint.Parse(cboCellCountX.Text);
            CellCountY = uint.Parse(cboCellCountY.Text);
            CellCountZ = uint.Parse(cboCellCountZ.Text);

            MaxCost = (decimal)spinMaxCost.Value;
            MaxTechLevel = uint.Parse(cboTechLevel.Text);

            if (CellCountX < 1 || CellCountY < 1 || CellCountZ < 1)
            {
                MessageBox.Show("Ship deck layout must be at least 1 length x 1 width x 1 decks");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonBrowseAsset_Click(object sender, EventArgs e)
        {
            string modPath = AppMain.MOD_PATH;
            string modName = AppMain.ModName;
            KeyEdit.GUI.AssetBrowserDialog dialog = 
                new KeyEdit.GUI.AssetBrowserDialog(modPath, modName, new string[] {".KGBENTITY"});
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                textMeshResource.Text = dialog.ModDBSelectedEntry;
            }
        }
    }
}
