using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

using MindFusion.Drawing;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;


namespace KeyEdit.Controls
{
    public partial class BehaviorTreeEditorControl : UserControl
    {
        private EventHandler mOnSaveHandler;
        private EventHandler mOnOpenHandler;
        private EventHandler mOnNewHandler;

        Keystone.Behavior.Behavior mRootBehavior;
        string mModName;

        public BehaviorTreeEditorControl(EventHandler onNew, EventHandler onOpen, EventHandler onSave)
        {
            InitializeComponent();

            mOnSaveHandler = onSave;

            // create the "behaviors" path under the modpath if it doesn't exist
            mOnNewHandler = onNew;
            mOnOpenHandler = onOpen;
            mOnSaveHandler = onSave;
        }

        public void Clear()
        {
            treeView.Nodes.Clear();
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }


        private void buttonNew_Click(object sender, EventArgs e)
        {
            if (mOnNewHandler != null)
                mOnNewHandler(sender, e);
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            if (mOnOpenHandler != null)
                mOnOpenHandler(sender, e);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (mOnSaveHandler != null)
                mOnSaveHandler(sender, e);
        }

        
    }
}
