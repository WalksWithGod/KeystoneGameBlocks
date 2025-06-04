using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEdit.Controls
{
    // this version of the ViewportsDocument has a toolbar and would be used
    // with viewports controls that have their own independant toolbars DISABLED
    public partial class ViewportsDocumentWithToolbar : ViewportsDocument
    {

        public ViewportsDocumentWithToolbar() : base() { }

        public ViewportsDocumentWithToolbar(string name, DisplayMode displayMode, EventHandler<ViewportEventArgs> vpOpening, EventHandler<ViewportEventArgs> vpClosing)
            : base(name, displayMode, vpOpening, vpClosing)
        {
            InitializeComponent();
            Name = name; // Name must be set after InitializeComponent() or the name will get reset 
        }

        public DevComponents.DotNetBar.ButtonItem GetButtonItem(string name)
        {
            DevComponents.DotNetBar.ButtonItem result = ribbonBar.Items[name] as DevComponents.DotNetBar.ButtonItem;
            return result;
        }

        public DevComponents.DotNetBar.ButtonItem AddToolbarButton(string name, Image image, EventHandler buttonClick)
        {
            DevComponents.DotNetBar.ButtonItem item = new DevComponents.DotNetBar.ButtonItem();
            item.Name = name;
            item.Image = image;
            item.Tooltip = name;
            item.Click += buttonClick;
            ribbonBar.Items.Add(item);
            return item;
        }

        public DevComponents.DotNetBar.ButtonItem AddToolbarButton(DevComponents.DotNetBar.ButtonItem parent, string name, Image image, EventHandler buttonClick)
        {
            DevComponents.DotNetBar.ButtonItem item = new DevComponents.DotNetBar.ButtonItem();
            item.Name = name;
            item.Image = image;
            item.Tooltip = name;
            item.Click += buttonClick;
            ribbonBar.Items.Add(item);

            parent.SubItems.Add(item);
            return item;
        }

    }
}
