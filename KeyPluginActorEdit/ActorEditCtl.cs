using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using KeyPlugins;
using System.Windows.Forms;

namespace KeyPluginActorEdit
{
    // Weapon.PerformEvent["OnHolster"];
    //
    // todo: i think the main aspects of actor edit are the
    // different bones and animation ranges.  I dont know if it'd be
    // better to have animations managed at the BonedModel level...
    // but bones i think definetly
    //  - bone naming
    //  - 
    //  is there a 1:1 mapping of bones to groups?  i dont think so?
    // A BonedENTITY though can have attach points that aren't just bones
    // (shouldn't a regular modeled entity too?)
    // Well attachpoints have names, eg "holster" but maybe it's better to do
    // WeaponEntity.PerformEvent (holster)  where the weapon knows where to holster
    // itself given a particular actor?  Is that bad though because every time you
    // add a new actor, you have to edit every weapon and set the new holster point?
    // But maybe that's better than having to edit every actor each time you add a new
    // weapon to specify the holster point for a specific weapon?  What's worse or
    // what makes better programatic sense?
    //
    // - Use our Grid Control for the Bones
    // - or maybe a treeview to show hierarchical nature of them
    public partial class ActorEditCtl : BasePluginCtl 
    {
        public ActorEditCtl(IPluginHost host)
            : base(host)
        {
            InitializeComponent();
            mName = "Keystone Actor3d Editor Plugin";
            mTargetTypename = "Actor3d";
            mDescription = "A control for configuring boned geometry";
        }

        public override void SelectTarget(string id, string typeName)
        {
            mTargetNodeID = id;
            mTargetTypename = typeName;

            //throw new NotImplementedException();
            //Vector3d vector = (Vector3d)mHost.GetNodeProperty(id, "position");
            //numPositionX.Value = (decimal)vector.x;
            //numPositionY.Value = (decimal)vector.y;
            //numPositionZ.Value = (decimal)vector.z;

            //vector = (Vector3d)mHost.GetNodeProperty(id, "scale");
            //numScaleX.Value = (decimal)vector.x;
            //numScaleY.Value = (decimal)vector.y;
            //numScaleZ.Value = (decimal)vector.z;

            //Quaternion quat = (Quaternion)mHost.GetNodeProperty(id, "rotation");
            //Vector3d eulers = quat.GetEulerAngles();

            //numRotX.Value = (decimal)(Keystone.Math2.Math2.RADIANS_TO_DEGREES * eulers.x);
            //numRotY.Value = (decimal)(Keystone.Math2.Math2.RADIANS_TO_DEGREES * eulers.y);
            //numRotZ.Value = (decimal)(Keystone.Math2.Math2.RADIANS_TO_DEGREES * eulers.z);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // collidable, castsShadow, additiveShadows, selfShadows
            // visible, overlay, enable, enableAlphatest, alphaTestDepthBufferWriteEnable
            //mHost.ChangeNodeProperty("", 0, BitConverter.GetBytes(checkBox1.Checked));
        }

        public override ContextMenuStrip GetContextMenu(string resourceID, string parentID, Point location)
        {
            ContextMenuStrip menu = base.GetContextMenu(resourceID, parentID, location);

            ToolStripSeparator seperator = new ToolStripSeparator();
            menu.Items.Add(seperator);

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Save As...");
            menuItem.Name = resourceID;
            menuItem.Tag = parentID;
            menuItem.Click += new EventHandler(SaveAs_Click);
            menu.Items.Add(menuItem);

            return menu;
        }

        private void SaveAs_Click(object sender, EventArgs e)
        {
          //  mHost.SendMessage(((ToolStripMenuItem)sender).Tag.ToString(), 6, null);
        }
    }
}
