using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyPlugins
{
	/// <summary>
	/// Description of StaticControls.
	/// </summary>
	public class StaticControls
	{
		public static DialogResult YesNoBox (string title, string promptText)
        {

            DialogResult dialogResult = MessageBox.Show(title, promptText, MessageBoxButtons.YesNo);
            return dialogResult;

        }

        public static DialogResult InputBox(string title, string promptText, ref string inputText)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = inputText;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(System.Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            inputText = textBox.Text;
            return dialogResult;
        }
            
        public static DialogResult ProgressBox(string title, string promptText, ref int progressValue)
        {
            Form form = new Form();
            Label label = new Label();
            ProgressBar progressBar = new ProgressBar();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            progressBar.Value = progressValue;

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            progressBar.SetBounds(12, 36, 372, 20);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            progressBar.Anchor = progressBar.Anchor | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, progressBar, buttonCancel });
            form.ClientSize = new Size(System.Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            progressValue = progressBar.Value;
            return dialogResult;
        }


        public delegate void NodeCreated(TreeView tvw, TreeNode parentNode, TreeNode childNode, string childTypeName);


        public static TreeNode GenerateTreeBranch(IPluginHost host, TreeView treeview, TreeNode parentNode, string parentID, string childID, string childTypename, string[] filteredTypes, bool recurse, NodeCreated nodeCreatedCallBack)
        {
            TreeNode childNode = CreateTreeNode(parentID, childID, childTypename);

            if (childNode == null)
            {
                System.Diagnostics.Debug.WriteLine("Unexpected null child tree node...");
                return null;
            }
            // TODO: every node that is created, we want to inform the IPluginHost that we want
            // to be notified if that node changes.
            childNode.Text = childTypename + ", " + childID;

            if (parentNode == null)
            {
                // add as root
                treeview.Nodes.Add(childNode);
            }
            else
            {
                // we don't want to display GUID ID
                if (childTypename != "Script")
                    childNode.Text = childTypename;

                parentNode.Nodes.Add(childNode);
                parentNode.Expand();
                childNode.Expand();
            }

            if (nodeCreatedCallBack != null)
                nodeCreatedCallBack(treeview, parentNode, childNode, childTypename);

            if (recurse)
            {
                // get the child nodes and add them to the tree and recurse them
                string[] childIDs;
                string[] childTypes;
                host.Node_GetChildrenInfo(childID, filteredTypes, out childIDs, out childTypes);

                if (childIDs != null && childIDs.Length > 0)
                {
                    for (int i = 0; i < childIDs.Length; i++)
                    {
                        string nodeType = childTypes[i];
                        string nodeID = childIDs[i];

                        // TODO: if this is the fullTreeview and
                        // it's a child Entity we should add but dont recurse
                        // if the user clicks the child entity it will set it as active in plugin?
                        // we can decide later..
                        TreeNode treeNode = GenerateTreeBranch(host, treeview, childNode, childID, nodeID, nodeType, filteredTypes, recurse, nodeCreatedCallBack);
                        if (treeNode == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Unexpected null tree node...");
                            continue;
                        }
                    }
                }
            }

            return childNode;
        }



        private static TreeNode CreateTreeNode(string parentID, string nodeID, string typeName)
        {
            TreeNode treeNode = new TreeNode();
            treeNode.Name = nodeID;
            treeNode.Text = typeName;
            treeNode.Tag = parentID;
            treeNode.Expand();
            return treeNode;
        }



    }
}
