using System;
using System.Windows.Forms;
using Keystone.Resource;

namespace Keystone.Workspaces
{
    public interface IWorkspace
    {
        void OnEntityAdded(Entities.Entity parent, Entities.Entity child);
        void OnEntityRemoved(Entities.Entity parent, Entities.Entity child);
        void OnEntitySelected(Keystone.Collision.PickResults pickResults);

        void OnEntityDeleteKeyPress ();

        void OnNodeAdded(string parentID, string childID, string childTypeName);
        void OnNodeRemoved(string parentID, string childID, string childTypeName);


        string Name { get; }
        bool IsActive {get;}
        Control Document { get; }
        Workspaces.IWorkspaceManager Manager { get; }
        
        
        void Configure(IWorkspaceManager manager, Keystone.Scene.Scene scene);
        void UnConfigure();

        void Resize();
        void Show();
        void Hide();

        
        
       // Keystone.Controllers.InputControllerBase IOController {get;}

        Keystone.Collision.PickResults SelectedEntity { get; set; }

        Keystone.Collision.PickResults MouseOverItem { get; set; }


    }
    
    public interface IWorkspace3D : IWorkspace 
    {
    	Keystone.EditTools.Tool CurrentTool {get;set;}
    	void SelectTool (Keystone.EditTools.ToolType toolType);


        void ToolMouseBeginLeftButtonSelect(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition);
        void ToolMouseEndLeftButtonSelect(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition);
        void ToolMouseBeginRightButtonSelect (Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition);
    	void ToolMouseEndRightButtonSelect (Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition);
        

        void ToolMouseMove(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition );
    	void ToolCancel();
        void ToolKeyDown(string key);
        void ToolKeyUp(string key);
        //Keystone.Scene.SceneBase Scene { get; }
        //ViewportControl[] ViewportControls { get; set; }
    }
}
