using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;

namespace KeyPluginTextureEdit
{
    public partial class TextureEditCtl : BasePluginCtl 
    {

        public TextureEditCtl(IPluginHost host) : base(host)
        {
            InitializeComponent();
            mName = "Keystone Texture Editor Plugin";
            mTargetTypename = "Texture";
            mDescription = "A control for configuring textures";
        }

        public override void SelectTarget(string id, string typeName)
        {
            mTargetNodeID = id;
            mTargetTypename = typeName;

            throw new NotImplementedException();
        }

        private void nudTileV_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudTileU_ValueChanged(object sender, EventArgs e)
        {

        }

        private void buttonExport_Click(object sender, EventArgs e)
        {

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {

        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {

        }
    }
}
