using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeyEdit
{
    // The purpose of this class is to store the visible state of the treeview.
    // That is, if a branch is collapsed, those hidden child branches will not be saved.
    // However the collapsed state of the parent will be.
    // (will the collapsed icon be visible if there are no children added?)
    //
    // This class must also "watch" for changes such as adding/deletion of nodes that occur
    // when a different view is active.
    // So in this sense, we're actually storing the entire tree... hrm...
    // Maybe that's what we should do?  Store it but without the GUI and then change
    // the current TreeView.
    //
    // As for our method of navigating into sub-entities... i wonder if we should abandon that?
    // It's very problematic.
    //
    // 
    // 
    /// <summary>
    /// http://blog.binaryocean.com/2006/01/19/SaveTreeViewNodesExpansionCollapseStateCSAndVB.aspx
    /// public domain
    /// </summary>
    public class TreeviewState
    {
        private int RestoreTreeViewIndex;
        public object Tag;

        public void SaveTreeView(TreeView treeView, string key)
        {
            List<bool?> list = new List<bool?>();
            SaveTreeViewExpandedState(treeView.Nodes, list);
            Tag = key;

        }       
        public void RestoreTreeView(TreeView treeView, string key)
        {
            RestoreTreeViewIndex = 0;
            RestoreTreeViewExpandedState(treeView.Nodes, new List<bool?>());
        }
        

        private void SaveTreeViewExpandedState(TreeNodeCollection nodes, List<bool?> list)
        {
            foreach (TreeNode node in nodes)
            {

                //list.Add(node.IsExpanded);
                //if (node.FirstNode != null )
                //{
                //    SaveTreeViewExpandedState(node.ChildNodes, list);
                //}
            }
        }
        

        private void RestoreTreeViewExpandedState(TreeNodeCollection nodes, List<bool?> list)
        {
            foreach (TreeNode node in nodes)
            {
                if (RestoreTreeViewIndex >= list.Count) break;

                if (list[RestoreTreeViewIndex++] == true)
                    node.Expand();
                //else
                //    node.Collapse(); // redundant. they start as collapsed right?

                //if (node.. .Count > 0)
                //{
                //    RestoreTreeViewExpandedState(node.ChildNodes, list);
                //}
            }
        }
    }
}
