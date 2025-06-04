using System;
using System.Windows.Forms;
using System.Drawing;

namespace ImageBrowserDotnet
{
    public class BrowserFlowLayoutPanel : FlowLayoutPanel
    {
        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
        }
    }
}
