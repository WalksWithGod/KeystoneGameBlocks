using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;

namespace KeyEdit.Workspaces 
{
    public class SimWorkspace : WorkspaceBase
    {

        public SimWorkspace(string name)
            : base(name)
        { 
        }

        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            base.Configure(manager, scene);
            throw new NotImplementedException();
        }

        public override void UnConfigure()
        {
            base.UnConfigure();
            throw new NotImplementedException();
        }

        #region IView Members

        public override void Show()
        {
            base.Show();
            throw new NotImplementedException();
        }

        public override void Hide()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
