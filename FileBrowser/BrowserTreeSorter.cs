using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ShellDll;

namespace FileBrowser
{
    /// <summary>
    /// This class is used to sort the TreeNodes in the BrowserTreeView
    /// </summary>
    public class BrowserTreeSorter : IComparer
    {
        #region IComparer Members

        /// <summary>
        /// This method will compare the ShellItems of the TreeNodes to determine the return value for
        /// comparing the TreeNodes.
        /// </summary>
        public int Compare(object x, object y)
        {
            TreeNode nodeX = x as TreeNode;
            TreeNode nodeY = y as TreeNode;

            if (nodeX.Tag != null && nodeY.Tag != null)
                return ((ShellItem)nodeX.Tag).CompareTo(nodeY.Tag);
            else if (nodeX.Tag != null)
                return 1;
            else if (nodeY.Tag != null)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
