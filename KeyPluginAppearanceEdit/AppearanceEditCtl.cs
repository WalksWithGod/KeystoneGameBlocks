using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;

namespace KeyPluginAppearanceEdit
{
    public partial class AppearanceEditCtl : BasePluginCtl
    {

        public AppearanceEditCtl(IPluginHost host)
            : base(host)
        {
            InitializeComponent();
            mName = "Keystone Appearance Editor Plugin";
            mTargetTypename = "Appearance";
            mDescription = "A control for configuring general appearance";
        }

        public override void SelectTarget(string id, string typeName)
        {
            mTargetNodeID = id;
            mTargetTypename = typeName;

            throw new NotImplementedException();
        }

        public override ContextMenuStrip GetContextMenu(string resourceID, string parentID, Point location)
        {
            ContextMenuStrip menu = base.GetContextMenu(resourceID, parentID, location);

            // a group attribute cannot be deleted because it is associated with a group in tvactor or tvmesh.
            // a Group can be deleted if the mesh group is somehow merged or removed.  Maybe we could have
            // a "Merge Group" or "Delete Group" and when that occurs, make a call to the underlying Mesh3d or Actor3d
            // to merge or delete the group.
            menu.Items[0].Enabled = false;

            ToolStripSeparator seperator = new ToolStripSeparator();
            menu.Items.Add(seperator);

            return menu;
        }

        private void AddMaterial_Click(object sender, EventArgs e)
        {
          //  mHost.SendMessage(((ToolStripMenuItem)sender).Tag.ToString(), 200, null);
        }
    }
}