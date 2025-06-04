using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;


namespace KeyEdit.Workspaces
{
    public partial class ShipSystemsWorkspace : WorkspaceBase // TODO: will i have need for a 3d view of floorplan?  If not IWorkspace is probably better to implement than WorkspaceBase
    {
        private bool mNavBarDocked = false;
        private System.Windows.Forms.Control mNavBar;

        public ShipSystemsWorkspace(string name) : base (name)
        {
            IntializeNavBar();
        }


        #region IWorkspace Members
        public override void Show()
        {
            base.Show();

            if (mNavBarDocked == false)
            {
                // NOTE: This toolbox has it's own unique name "FloorPlanToolbox"
                ((WorkspaceManager)mWorkspaceManager).DockControl(mNavBar, "Crew Management", "leftDockSiteBar", "Crew Management", eDockSide.Left);
                mNavBarDocked = true;
            }
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion
    }
}
