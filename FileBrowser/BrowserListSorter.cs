using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ShellDll;

namespace FileBrowser
{
    /// <summary>
    /// This class is used to sort the ListViewItems in the BrowserListView
    /// </summary>
    internal class BrowserListSorter : IComparer
    {
        #region IComparer Members

        /// <summary>
        /// This method will compare the ShellItems of the ListViewItems to determine the return value for
        /// comparing the ListViewItems.
        /// </summary>
        public int Compare(object x, object y)
        {
            ListViewItem itemX = x as ListViewItem;
            ListViewItem itemY = y as ListViewItem;

            if (itemX.Tag != null && itemY.Tag != null)
                return ((ShellItem)itemX.Tag).CompareTo(itemY.Tag);
            else if (itemX.Tag != null)
                return 1;
            else if (itemY.Tag != null)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
