using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;

namespace KeyPluginLODMeshEdit
{
    public partial class LODMeshEditCtl : BasePluginCtl
    {

        public LODMeshEditCtl(IPluginHost host)
            : base(host)
        {
            InitializeComponent();
            mName = "Keystone LOD Mesh Editor Plugin";
            mTargetTypename = "LODSwitch";
            mDescription = "A control for configuring geometry LOD levels";
        }

        //        private void menuItemLODExporter_Click(object sender, EventArgs e)
        //        {
        //            // pop up tool so user can set a LOD percentage as well as perhaps have option to add the LOD to the Model
        //            // and set a LOD switch distance

        //        }

        //        private void LODExport()
        //        {
        //            //TEST TO SEE IF DIRECTX EXPORT WORKS
        //            MeshTool tool = new MeshTool(_core.Scene, _core.MaterialFactory, _core.TextureFactory);
        //            TVMesh lodResult = tool.CreateLOD(@"E:\dev\artwork\dominus_art_public_domain\actors\scale_Renzok_static.x", true, true, .21f, "test1");
        //            string tmppath = @"E:\dev\vb.net\test\AGT3rdPersonTV3DDemo\Data\Actors\testdump\lod.tvm.";
        //            lodResult.SaveTVM(tmppath);
        //            //END TEST
        //        }

        public override void SelectTarget(string id, string typeName)
        {
            mTargetNodeID = id;
            mTargetTypename = typeName;

            throw new NotImplementedException();
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
