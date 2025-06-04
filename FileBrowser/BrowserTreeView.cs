using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ShellDll;

namespace FileBrowser
{
    /// <summary>
    /// This is the TreeView used in the Browser control
    /// </summary>

    public class BrowserTreeView : TreeView
    {
            private BrowserTreeSorter sorter;

            public BrowserTreeView()
            {
                HandleCreated += new EventHandler(BrowserTreeView_HandleCreated);

                sorter = new BrowserTreeSorter();
            }

            #region Override

            #endregion

            #region Events

            /// <summary>
            /// Once the handle is created we can assign the image list to the TreeView
            /// </summary>
            void BrowserTreeView_HandleCreated(object sender, EventArgs e)
            {
                ShellImageList.SetSmallImageList(this);
            }
            #endregion

            #region Public
            public bool GetTreeNode(ShellItem shellItem, out TreeNode treeNode)
            {
                List<ShellItem> pathList = new List<ShellItem>();

                while (shellItem.ParentItem != null)
                {
                    pathList.Add(shellItem);
                    shellItem = shellItem.ParentItem;
                }
                pathList.Add(shellItem);

                pathList.Reverse();

                treeNode = Nodes[0];
                for (int i = 1; i < pathList.Count; i++)
                {
                    bool found = false;
                    foreach (TreeNode node in treeNode.Nodes)
                    {
                        if (node.Tag != null && node.Tag.Equals(pathList[i]))
                        {
                            treeNode = node;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        treeNode = null;
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// This method will check whether a TreeNode is a parent of another TreeNode
            /// </summary>
            /// <param name="parent">The parent TreeNode</param>
            /// <param name="child">The child TreeNode</param>
            /// <returns>true if the parent is a parent of the child, false otherwise</returns>
            public bool IsParentNode(TreeNode parent, TreeNode child)
            {
                TreeNode current = child;
                while (current.Parent != null)
                {
                    if (current.Parent.Equals(parent))
                        return true;

                    current = current.Parent;
                }
                return false;
            }

            /// <summary>
            /// This method will check whether a TreeNode is a parent of another TreeNode
            /// </summary>
            /// <param name="parent">The parent TreeNode</param>
            /// <param name="child">The child TreeNode</param>
            /// <param name="path">If the parent is indeed a parent of the child, this will be a path of
            /// TreeNodes from the parent to the child including both parent and child</param>
            /// <returns>true if the parent is a parent of the child, false otherwise</returns>
            public bool IsParentNode(TreeNode parent, TreeNode child, out TreeNode[] path)
            {
                List<TreeNode> pathList = new List<TreeNode>();

                TreeNode current = child;
                while (current.Parent != null)
                {
                    pathList.Add(current);
                    if (current.Parent.Equals(parent))
                    {
                        pathList.Add(parent);
                        pathList.Reverse();
                        path = pathList.ToArray();
                        return true;
                    }

                    current = current.Parent;
                }

                path = null;
                return false;
            }

            public void SetSorting(bool sorting)
            {
                if (sorting)
                    this.TreeViewNodeSorter = sorter;
                else
                    this.TreeViewNodeSorter = null;
            }
            #endregion
    }
}
