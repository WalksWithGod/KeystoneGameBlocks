using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPlugins
{
    partial class BaseEntityPlugin
    {
        private void buttonAddBehaviorTree_Click(object sender, EventArgs e)
        {
            // NOTE: we should be adding a file with extension .kgbbehavior  not a .css script.
            string fileDialogFilter = "Behaviors|*.kgbbehavior";
            BrowseNewResource("Behavior", mTargetNodeID, fileDialogFilter);
        }

        private void buttonRemoveBehaviorTree_Click(object sender, EventArgs e)
        {
            string behaviorTreeNodeID = superTabItem6.Tag.ToString();
            mHost.Node_Remove(behaviorTreeNodeID, mTargetNodeID);
            ClearBehaviorPanel();
        }

        private void ClearBehaviorPanel()
        {
            treeBehavior.Nodes.Clear();
            buttonRemoveBehaviorTree.Hide();
            buttonAddBehaviorTree.Show();
        }

        // TODO: this doesn't get called if there is no DomainObject
        // attached to an entity, however it's not clearing the .SelectedObject either 
        // of the previous entity.
        // second, when adding a new domainobject, this doesnt get called to now reflect
        // the DO that now exists. (animationset and behaviortree does same thing)

        private void PopulateBehaviorPanel(string parentID, string childID, string typeName)
        {
            buttonAddBehaviorTree.Visible = false;
            buttonRemoveBehaviorTree.Top = buttonAddBehaviorTree.Top;
            buttonRemoveBehaviorTree.Visible = true;
            

            treeBehavior.NodeMouseClick += OnTreeBehavior_NodeClick;

            TreeNode rootNode = KeyPlugins.StaticControls.GenerateTreeBranch(mHost, treeBehavior, null, parentID, childID, typeName, null, true, null);
            if (rootNode == null)
            {
                System.Diagnostics.Debug.WriteLine("Unexpected null tree node...");
                return;
            }
        }

        private void OnTreeBehavior_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string parentNodeID = null;
            if (e.Node.Parent != null)
                parentNodeID = (string)e.Node.Parent.Name;
            else // this is root Sequence or Selector and so the current entity is parent
                parentNodeID = TargetID;

            // TODO: we now use BehaviorTreeEditorWorkspace and no longer allow editing of behavior trees in the plugin
            // TODO: if anything, if we click on any nodes and want to edit, it should open the BehaviorTreeEditorWorkspace
            //KeyPlugins.StaticControls.OnTreeBehavior_NodeClick(sender, e);

        }
    }
}
