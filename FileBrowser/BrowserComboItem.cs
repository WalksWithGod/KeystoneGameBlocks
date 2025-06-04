using System.Drawing;
using ShellDll;

namespace FileBrowser
{
    /// <summary>
    /// This class represents the items that can be used for the BrowserComboBox
    /// </summary>
    internal class BrowserComboItem
    {
        // The ShellItem that goes with the ComboItem
        private ShellItem shellItem;

        // The indent that goes with the ComboItem
        private int indent;

        // The Icon that has to be drawn for this ComboItem
        private Icon image;

        public BrowserComboItem(ShellItem shellItem, int indent)
        {
            this.shellItem = shellItem;
            this.indent = indent;
            this.image = ShellImageList.GetIcon(shellItem.ImageIndex, true);
        }

        #region Properties
        public ShellItem ShellItem { get { return shellItem; } }
        public int Indent { get { return indent; } }
        public Icon Image { get { return image; } }
        public string Text { get { return ShellItem.Text; } }
        #endregion

        public override string ToString()
        {
            return ShellItem.Path;
        }
    }
}