using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShellDll;
using ColumnHeader=System.Windows.Forms.ColumnHeader;
using ControlStyles=System.Windows.Forms.ControlStyles;
using DrawListViewColumnHeaderEventArgs=System.Windows.Forms.DrawListViewColumnHeaderEventArgs;
using DrawListViewColumnHeaderEventHandler=System.Windows.Forms.DrawListViewColumnHeaderEventHandler;
using DrawListViewItemEventArgs=System.Windows.Forms.DrawListViewItemEventArgs;
using DrawListViewItemEventHandler=System.Windows.Forms.DrawListViewItemEventHandler;
using DrawListViewSubItemEventArgs=System.Windows.Forms.DrawListViewSubItemEventArgs;
using DrawListViewSubItemEventHandler=System.Windows.Forms.DrawListViewSubItemEventHandler;
using ListView=System.Windows.Forms.ListView;
using ListViewAlignment=System.Windows.Forms.ListViewAlignment;
using ListViewItemSelectionChangedEventArgs=System.Windows.Forms.ListViewItemSelectionChangedEventArgs;
using Message=System.Windows.Forms.Message;

namespace FileBrowser
{
    /// <summary>
    /// This is the ListView used in the Browser control
    /// </summary>
    public class BrowserListView : ListView
    {
        #region Fields

        // The arraylist to store the order by which ListViewItems has been selected
        private ArrayList selectedOrder;

        private System.Windows.Forms.ContextMenu columnHeaderContextMenu;
        private bool suspendHeaderContextMenu;
        private int columnHeight = 0;

        private BrowserListSorter sorter;

        #endregion

        public BrowserListView()
        {
            OwnerDraw = true;

            HandleCreated += new EventHandler(BrowserListView_HandleCreated);
            selectedOrder = new ArrayList();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            DrawItem += new DrawListViewItemEventHandler(BrowserListView_DrawItem);
            DrawSubItem += new DrawListViewSubItemEventHandler(BrowserListView_DrawSubItem);
            DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(BrowserListView_DrawColumnHeader);

            this.Alignment = ListViewAlignment.Left;
            sorter = new BrowserListSorter();
        }

        #region Owner Draw

        void BrowserListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        void BrowserListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        void BrowserListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            columnHeight = e.Bounds.Height;
        }

        #endregion

        #region Override

        public new System.Windows.Forms.View View
        {
            get
            {
                return base.View;
            }
            set
            {
                base.View = value;

                if (value == View.Details)
                {
                    foreach (ColumnHeader col in Columns)
                        if (col.Width == 0)
                            col.Width = 120;
                }
            }
        }

        protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
                selectedOrder.Insert(0, e.Item);
            else
                selectedOrder.Remove(e.Item);

            base.OnItemSelectionChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (this.View == View.Details && columnHeaderContextMenu != null &&
                (int)m.Msg == (int)ShellAPI.WM.CONTEXTMENU)
            {
                if (suspendHeaderContextMenu)
                    suspendHeaderContextMenu = false;
                else
                {
                    int x = (int)ShellHelper.LoWord(m.LParam);
                    int y = (int)ShellHelper.HiWord(m.LParam);
                    Point clientPoint = PointToClient(new Point(x, y));

                    if (clientPoint.Y <= columnHeight)
                        columnHeaderContextMenu.Show(this, clientPoint);
                }

                return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Events

        /// <summary>
        /// Once the handle is created we can assign the image lists to the ListView
        /// </summary>
        void BrowserListView_HandleCreated(object sender, EventArgs e)
        {
            ShellImageList.SetSmallImageList(this);
            ShellImageList.SetLargeImageList(this);
        }

        #endregion

        #region Public

        [Browsable(false)]
        public ArrayList SelectedOrder
        {
            get { return selectedOrder; }
        }

        [Browsable(false)]
        public bool SuspendHeaderContextMenu
        {
            get { return suspendHeaderContextMenu; }
            set { suspendHeaderContextMenu = value; }
        }

        [Browsable(true)]
        public ContextMenu ColumnHeaderContextMenu
        {
            get { return columnHeaderContextMenu; }
            set { columnHeaderContextMenu = value; }
        }

        public void SetSorting(bool sorting)
        {
            if (sorting)
                this.ListViewItemSorter = sorter;
            else
                this.ListViewItemSorter = null;
        }

        public void ClearSelections()
        {
            selectedOrder.Clear();
            selectedOrder.Capacity = 0;
        }

        public bool GetListItem(ShellItem shellItem, out ListViewItem listItem)
        {
            listItem = null;

            foreach (ListViewItem item in Items)
            {
                if (shellItem.Equals(item.Tag))
                {
                    listItem = item;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
