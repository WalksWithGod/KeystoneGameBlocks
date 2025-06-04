using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeyEdit.Controls;

namespace KeyEdit.Workspaces
{
    public class PreviewWorkspace : WorkspaceBase3D 
    {
        internal PreviewWorkspace(string name) : base (name)
        {
        }

        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs e)
        {
            //base.OnViewportOpening(sender, e);

            if (mViewportControls[e.ViewportIndex] == null)
            {
                ViewportControl c = new ViewportEditorControl(CreateNewViewportName((int)e.ViewportIndex));
                mViewportControls[e.ViewportIndex] = c;
                if (e.HostControl != null)
                    e.HostControl.Controls.Add(mViewportControls[e.ViewportIndex]);


                ConfigureViewport(mViewportControls[e.ViewportIndex], new Huds.PreviewHud());

                ConfigurePicking(c.Viewport);
                ConfigureHUD(c.Viewport);

                c.ReadSettings(null);
            }
        }
    }
}
