using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEdit.Controls
{
    public partial class ViewportsDocument : UserControl
    {
        public enum DisplayMode
        {
            None, // ONLY TO BE USED ON INITIAL CREATION OR FINAL SHUTDOWN SINCE IT CLOSES VIEWPORT0 WHICH CAN NEVER BE LOST UNTIL APP CLOSE DOWN
            Single,
            HSplit,
            VSplit,
            TripleLeft,
            TripleRight,
            Quad
        }

        public class ViewportEventArgs : EventArgs
        {
            public uint ViewportIndex;
            public Control HostControl;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="hostControl">The control which will host this viewpoint control.  Can be null.</param>
            /// <param name="vpIndex"></param>
            public ViewportEventArgs(Control hostControl, uint vpIndex)
            {
                ViewportIndex = vpIndex;
                HostControl = hostControl;
            }
        }

        public event EventHandler<ViewportEventArgs> ViewportOpening;
        public event EventHandler<ViewportEventArgs> ViewportClosing;


        public ViewportsDocument() { }

        public ViewportsDocument(string name, DisplayMode displayMode, EventHandler<ViewportEventArgs> vpOpening, EventHandler<ViewportEventArgs> vpClosing)
        {
            InitializeComponent();
            Name = name; // Name must be set after InitializeComponent() or the name will get reset 

            ViewportOpening = vpOpening;
            ViewportClosing = vpClosing;

            ConfigureLayout(displayMode);

            // open viewport 0 immediately allowing user to respond to the event and create it
            //if (ViewportOpening != null)
            //    ViewportOpening(this, new  ViewportEventArgs (splitContainer3.Panel1, 0));
        }


        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Focus();
        }


        public void ConfigureLayout(DisplayMode mode)
        {
            SuspendLayout();
            // splitcontainer1 contains our first viewport and is split vertically
            // splitcontainer2 is to the right and sits in panel 2 of splitcontainer1 and has a horizontal split
            // splitcontainer3 sits in panel1 of our splitcontainer1 and has a horizontal split
            
            splitContainer3.SuspendLayout();
            splitContainer2.SuspendLayout();
            splitContainer1.SuspendLayout();

            // open viewport index 0 for all modes except .NONE
            if (mode != DisplayMode.None)
                if (splitContainer3.Panel1.Controls.Count == 0)
                    if (ViewportOpening != null)
                        ViewportOpening(this, new ViewportEventArgs(splitContainer3.Panel1, 0));
            

            switch (mode)
            {
                case DisplayMode.None : // NONE should only be done on initialization or app shutdown
                    // close viewport index 1
                    if (splitContainer3.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // close viewport index 2
                    if (splitContainer2.Panel1.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel1, 2));

                    // close viewport index 3
                    if (splitContainer2.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel2, 3));

                    // close viewport index 0 LAST
                    if (splitContainer3.Panel1.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer3.Panel1, 0));

                    // only uses viewport 0, collapse all the rest
                    splitContainer1.Panel2Collapsed = true;
                    splitContainer3.Panel2Collapsed = true;
                    splitContainer2.Panel2Collapsed = true;
                    break;
                case DisplayMode.Single:

                    // close viewport index 1
                    if (splitContainer3.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // close viewport index 2
                    if (splitContainer2.Panel1.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel1, 2));

                    // close viewport index 3
                    if (splitContainer2.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel2, 3));

                    // only uses viewport 0, collapse all the rest
                    splitContainer1.Panel2Collapsed = true;
                    splitContainer3.Panel2Collapsed = true;
                    splitContainer2.Panel2Collapsed = true;
                    break;

                case DisplayMode.HSplit:
                case DisplayMode.VSplit:
                    if (mode == DisplayMode.VSplit)
                        splitContainer3.Orientation = System.Windows.Forms.Orientation.Vertical;
                    else
                        splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;

                    // open viewport index 1 
                    if (splitContainer3.Panel2.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // close viewport index 2
                    if (splitContainer2.Panel1.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel1, 2));

                    // close viewport index 3
                    if (splitContainer2.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel2, 3));

                    // only uses viewports 0 and 1, collapse all the rest
                    splitContainer1.Panel2Collapsed = true;
                    splitContainer3.Panel2Collapsed = false;
                    splitContainer2.Panel2Collapsed = true;
                    break;

                case DisplayMode.TripleRight: // uses indices 0, 1, 2
                    // close viewport index 3 
                    if (splitContainer2.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer2.Panel2, 3));

                    // open viewport index 1
                    if (splitContainer3.Panel2.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // open viewport index 2 
                    if (splitContainer2.Panel1.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer2.Panel1, 2));


                    splitContainer1.Panel2Collapsed = false;
                    splitContainer3.Panel2Collapsed = false;
                    splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
                    splitContainer2.Panel2Collapsed = true;
                    break;

                case DisplayMode.TripleLeft: // uses indices 0, 2, 3
                    // close viewport index 1
                    if (splitContainer3.Panel2.Controls.Count > 0)
                        if (ViewportClosing != null)
                            ViewportClosing(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // open viewport index 2 
                    if (splitContainer2.Panel1.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer2.Panel1, 2));

                    // open viewport index 3 
                    if (splitContainer2.Panel2.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer2.Panel2, 3));


                    splitContainer1.Panel2Collapsed = false;
                    splitContainer3.Panel2Collapsed = true;
                    splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
                    splitContainer2.Panel2Collapsed = false;
                    break;

                case DisplayMode.Quad:
                    // viewports 2 and 3 go onto splitContainer2 panel's 1 and 2 respectively

                    // open viewport index 1 
                    if (splitContainer3.Panel2.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer3.Panel2, 1));

                    // open viewport index 2 
                    if (splitContainer2.Panel1.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer2.Panel1, 2));

                    // open viewport index 3 
                    if (splitContainer2.Panel2.Controls.Count == 0)
                        if (ViewportOpening != null)
                            ViewportOpening(this, new ViewportEventArgs(splitContainer2.Panel2, 3));

                    
                    splitContainer1.Panel2Collapsed = false; // splitContainer1's Panel2 hosts another splitter which hosts are index 2 and 3 viewports
                    splitContainer3.Panel2Collapsed = false;
                    splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
                    splitContainer2.Panel2Collapsed = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
            splitContainer1.ResumeLayout();
            splitContainer2.ResumeLayout();
            splitContainer3.ResumeLayout();
            ResumeLayout();
        }

    }
}
