using System;
using System.Windows.Forms;

namespace Keystone.Workspaces
{
    public interface IWorkspaceManager
    {
        Form Form { get;}

        void Add(IWorkspace workspace, Keystone.Scene.Scene scene);
        void Remove(IWorkspace workspace);
        void Remove(string name);
        IWorkspace GetWorkspace(string name);

        // TODO: this property needs to be removed.  We should not be accessing
        // the current WorkSpace from here but rather from the document tab
        IWorkspace CurrentWorkspace { get; set; }

        void ChangeWorkspace(string workspaceName, bool savePreviousLayout);
        void LoadLayout();

        void CreateWorkspaceDocumentTab(System.Windows.Forms.Control control, EventHandler gotFocus);

    }
}
