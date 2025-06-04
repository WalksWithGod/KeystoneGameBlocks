using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormImport : Form
    {
        private string mRelativeModFolderPath;
        private string mSourceFilePath;


        public FormImport(string sourceFilePath)
        {
            InitializeComponent();

            mSourceFilePath = sourceFilePath;
            
        }

        public string RelativeModFolderPath { get { return mRelativeModFolderPath; } }
        public bool LoadTextures { get { return checkBoxImportTextures.Checked; } }
        public bool LoadMaterials { get { return checkBoxImportMaterials.Checked; } }
        public bool ConvertToTVMorTVA { get { return checkBoxConvertToTVM.Checked; } }
        public bool ImportXFileAsActor { get { return checkBoxImportAsActor.Checked; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            labelSrcPath.Text = mSourceFilePath;
       
            mRelativeModFolderPath = AppMain.MOD_PATH;
            textBoxFileName.Text = Path.GetFileNameWithoutExtension(mSourceFilePath) + ".prefab";

            // fill the mod drop down
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(AppMain.DATA_PATH, "mods"));
            foreach (DirectoryInfo info in dirInfo.GetDirectories())
            {
                comboBoxMod.Items.Add(info.Name);
            }
            if (comboBoxMod.Items.Count > 0)
                comboBoxMod.SelectedIndex = 0;


            // fill the tree starting at the mod path and filtering out all but the \\resources sub folder and it's children
            

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!Settings.Initialization.IsValidFileName(textBoxFileName.Text)) return;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonCreateNew_Click(object sender, EventArgs e)
        {
            string result = "";
            if (KeyPlugins.StaticControls.InputBox("New mod folder", "Enter mod folder name:", ref result) == DialogResult.OK)
            {
                if (Settings.Initialization.ValidFolder(AppMain.MOD_PATH + result))
                {
                    Directory.CreateDirectory(AppMain.MOD_PATH + result);
                    int index = comboBoxMod.Items.Add(result);
                    comboBoxMod.SelectedIndex = index;
                }
                else
                    MessageBox.Show("Invalid character in folder name.");
            }
        }

        private void buttonCreateSubFolder_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("No folder selected.  Please select a folder in the tree to add a child folder to.");
                return;
            }

            string result = "";
            if (KeyPlugins.StaticControls.InputBox("New child folder", "Enter child folder name:", ref result) == DialogResult.OK)
            {
                if (Settings.Initialization.ValidFolder(AppMain.MOD_PATH + "resources\\" + comboBoxMod.Text + "\\" + result))
                {
                    Directory.CreateDirectory(AppMain.MOD_PATH + comboBoxMod.Text + "\\" + result);
                    TreeNode newNode = new TreeNode (result);
                    treeView1.SelectedNode.Nodes.Add(newNode );
                    treeView1.SelectedNode = newNode;
                }
                else
                    MessageBox.Show("Mod folder already exists.");
            }
        }


        private void textBoxFileName_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {

        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = treeView1.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                treeView1.SelectedNode = node;
                treeView1_AfterSelect(null, new TreeViewEventArgs(node));
            }
        }

        private void treeView1_AfterSelect (object sender, TreeViewEventArgs e )
        {
            System.Diagnostics.Debug.Assert (e.Node == treeView1.SelectedNode );
            if (treeView1.SelectedNode != null)
            {
                TreeNode current = treeView1.SelectedNode ;
            
                string subPath = current.Text;
                while (current.Parent != null)
                {
                    current = current.Parent ;
                    subPath = current.Text + "\\" + subPath ;
                }
                mRelativeModFolderPath = string.Format("\\{0}\\resources\\" + subPath, comboBoxMod.Text);
                labelDestPath.Text = "Dest Path: " + mRelativeModFolderPath + "\\" + textBoxFileName.Text;
            }
        }

        private void comboBoxMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            mRelativeModFolderPath = string.Format ("\\{0}\\resources\\", comboBoxMod.Text);
            labelDestPath.Text = "Dest Path: " + mRelativeModFolderPath + "\\" + textBoxFileName.Text; ;

            // populate the treeview
            treeView1.Nodes.Clear();
            PopulateTreeWithChildFolders(null, new DirectoryInfo(Path.Combine(AppMain.MOD_PATH, comboBoxMod.Text + "\\resources\\")));
            treeView1.ExpandAll();
        }

        private void PopulateTreeWithChildFolders(TreeNode parent, DirectoryInfo root)
        {
            // Now find all the subdirectories under this directory.
            foreach (System.IO.DirectoryInfo dirInfo in root.GetDirectories())
            {
                TreeNode node = new TreeNode(dirInfo.Name);
                if (parent != null)
                    parent.Nodes.Add(node);
                else
                    treeView1.Nodes.Add(node);

                // Resursive call for each subdirectory.
                PopulateTreeWithChildFolders(node, dirInfo);
            }
        }
    }         
}
