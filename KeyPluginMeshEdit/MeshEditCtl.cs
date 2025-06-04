using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;

namespace KeyPluginMeshEdit
{
    public partial class MeshEditCtl : BasePluginCtl
    {

        public MeshEditCtl(IPluginHost host)
            : base(host)
        {
            InitializeComponent();
            mName = "Keystone Mesh3d Editor Plugin";
            mTargetTypename = "Mesh3d";
            mDescription = "A control for configuring scene level geometry";
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

        private void checkAllowTransparency_CheckedChanged(object sender, EventArgs e)
        {
            // most of the below is for Entities.  
            // enable, visible(hidden), collidable, castsShadow, additiveShadows, 
            // selfShadows, overlay,  enableAlphatest, alphaTestDepthBufferWriteEnable
            
            
            // We need to carefully consider what options are for Meshes.  This really should
            // be pretty much Geometry related.
            // In fact i think the "transparency" option in TV.Mesh (SetBlendingMode...) 
            // should be something that is set on the fly in response to Material setting
            // in addition to being set direclty via Mesh.SetBlendingMode because
            // for some shaders where that mode isnt done in the shader, you'll want it set
            // in the mesh.
          
            
            //mHost.ChangeNodeProperty("", 0, BitConverter.GetBytes(checkAllowTransparency_CheckedChanged.Checked));
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
         //   mHost.SendMessage(((ToolStripMenuItem)sender).Tag.ToString(), 6, null);
        }
    }
}
