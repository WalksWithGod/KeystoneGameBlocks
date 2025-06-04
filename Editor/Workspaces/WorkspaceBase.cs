using System;
using System.Collections.Generic;
using DevComponents.DotNetBar ;
using KeyEdit.Controls;
using Keystone.Scene;
using System.Windows.Forms;
using System.IO;
using Keystone.Controllers;

namespace KeyEdit.Workspaces
{	
    // App.exe's document tabs represnt a "View" (as in MVC pattern).
    // Now in fact, if you look at Visual Studio, switching tabs from Start Page tab to a 
    // typical class code view tab, some menu and toolbar options disable and even disappear/reappear
    // In fact, same for the "Properties" page which shows as a Tab!  The entire toolbar changes.
    // This is our key!  The tab provides the context for the rest of the visible menus/toolbars
    // etc!
    // NOTE: VS.NET will simply turn a panel that is not useful for a current view completely blank.
    // That way even though some otehr panel gets focus, if the user tries to manually switch to it
    // there will be no controls visible to manipulate.
    public abstract class WorkspaceBase : Keystone.Workspaces.IWorkspace
    {
        protected Keystone.Workspaces.IWorkspaceManager mWorkspaceManager;
        protected string mName;
        protected string mCaption;
		protected bool mIsActive;
		
        protected Control mDocument;
        protected ViewportControl[] mViewportControls; // the rendering context for each ViewportControl is accessible individually through each vpc
        
        protected Keystone.Scene.Scene mScene; // a workspace must be tied to a scene.
                                                   // when the scene is unloaded, so are all Workspaces for that scene

        protected InputControllerBase mIOController; // one IO controller shared by all ViewportControls used in this Workspace. By design and good.
        protected Keystone.Collision.PickResults mPickResult;
        protected Keystone.Collision.PickResults _mouseOverItem;
        

        internal WorkspaceBase(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            mName = name; // name must be set before mViewManager.Add 

           
        }

        // TODO: why dont i pass the manager and scene to the constructor?  Why here?
        public virtual void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            if (manager == null) throw new ArgumentNullException("WorkspaceBase.Configre() - ERROR: No ViewManager set.");
            mWorkspaceManager = manager;

            if (scene == null) throw new ArgumentNullException();
            mScene = scene; 
        }

        public virtual void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - ERROR: No ViewManager set.");
        }

        #region IWorkspace Members
        public string Name { get { return mName; }}

        public bool IsActive {get {return mIsActive;}}
        
        public Keystone.Workspaces.IWorkspaceManager Manager { get { return mWorkspaceManager; } }

        
        public Scene Scene { get { return mScene; } }

        public Control Document { get { return mDocument; } }

        public ViewportControl[] ViewportControls { get { return mViewportControls; } set { mViewportControls = value; } }
        
        public InputControllerBase IOController { get { return mIOController; } }


        // The selected PickResult from any RenderingContext associated with
        // this workspace.  There can only be one selected PickResult per workspace at a time
        // which is good and by design.  Any renderingcontext that is open in that workspace
        // can be used to select PickResults.  Alternatively, empowered gui's such as treeviews
        // or listviews etc can be used.
        public Keystone.Collision.PickResults SelectedEntity
        {
            set { mPickResult = value; }
            get { return mPickResult; }
        }

        public Keystone.Collision.PickResults MouseOverItem
        {
            set { _mouseOverItem = value; }
            get { return _mouseOverItem; }
        }
        

        public virtual void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public virtual void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public virtual void OnEntitySelected(Keystone.Collision.PickResults pickResult)
        { 
        	mPickResult = pickResult;
        	
        }

        public virtual void OnEntityMouseOver(Keystone.Collision.PickResults pickResult)
        { 
        	_mouseOverItem = pickResult;
        }
        
        public virtual void OnEntityDeleteKeyPress ()
        {
            // If in treeviewBrowser an entity is selected, that entity should be selected in the
            // scene as well.  When a viewport loses focus it's selected entity should be ignored
            // automatically as unfocussed viewports should not handle user input.
            // but how do we get the mSelectedContext to be the correct one when multiple viewports are open?
            // In terms of delete, the context is important because in theory, we can have seperate scenes
            // assigned to each context.  The treeview is specific to a scene obviously... it must be
            // specific to a workspace though also.  I think every context within a workspace MUST
            // represent the same scene.  In order to represent different scenes, you must 
            // use different Workspaces.
            // TODO: it could also be that this entire class becomes merged application side
            // as EditorWorkspace.Controller.cs partial class.
            if (this.SelectedEntity != null && this.SelectedEntity.Entity != null)
            {
                // keydown can only ever be used to manipulated Entities.  Entities are the only
                // nodes accessible to the user through the viewport.  Otherwise if the treeview is focussed
                // then other nodes can be used and the parent id is obvious as the node's parent key value.

                // send a message, to delete this node
                KeyCommon.Messages.Node_Remove removeNode = new KeyCommon.Messages.Node_Remove();
                removeNode.mParentID = this.SelectedEntity.Entity.Parent.ID;
                removeNode.mTargetIDs = new string[] { this.SelectedEntity.Entity.ID };

                removeNode.SetFlag(KeyCommon.Messages.Flags.SourceIsClient);
                // TODO: i need to check all workspaces and remove , now im just doing the current
                this.SelectedEntity = null;

                // TODO: currently removing seems to fail at removing from Octree Octant
                AppMain.mNetClient.SendMessage(removeNode);
            }
        }

        public virtual void OnNodeAdded(string parentID, string childID, string childTypeName)
        {
        }
        public virtual void OnNodeRemoved(string parentID, string childID, string childTypeName)
        {
        }

        public virtual void QuickLookPanelSelect(KeyEdit.PluginHost.EditorHost scriptingHost, string entityID, string entityTypeName, Keystone.Collision.PickResults pick)
        {
            throw new NotImplementedException();
        }


        protected void OnDocumentResized(object sender, EventArgs e)
        {
            Resize();
        }

        public virtual void Resize()
        {
            if (this.ViewportControls != null)
                foreach (ViewportControl vc in this.ViewportControls)
                    if (vc != null)
                        vc.ResizeViewport();   
        }

        public virtual void Show()
        {
            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                        vc.Enabled = true;
                }
            
            mIsActive = true;
        }

        public virtual void Hide()
        {
            
            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                        vc.Enabled = false;
                }
            
            mIsActive = false;
        }
        #endregion

    }
}
