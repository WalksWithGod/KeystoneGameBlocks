using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeyEdit.Controls;

namespace KeyEdit.Workspaces
{
    public class RenderPreviewWorkspace : EditorWorkspace 
    {
        private string mPreviousWorkspace;
        System.Windows.Forms.Form mRenderPreviewForm;
        Keystone.Types.Color mBackColor;

        public RenderPreviewWorkspace(string name, System.Windows.Forms.Form target, string lastWorkspaceName) 
            : base(name)
        {
            if (target == null) throw new ArgumentNullException();
            mRenderPreviewForm = target;
            mBackColor = new Keystone.Types.Color(165, 191, 225, 255);
            mPreviousWorkspace = lastWorkspaceName;
        }


        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            //base.Configure(manager); // NOTE: Dont call base.Configure because it'll instatiates a viewportcontrol we dont want here

            if (manager == null) throw new ArgumentNullException("RenderPreviewWorkspace.Configre() - WorkspaceManager parameter is NULL.");
            if (scene == null) throw new ArgumentNullException();

            mWorkspaceManager = manager;
            mScene = scene;

            // NOTE: dim the mViewportControls array before mDocument creation 
            mViewportControls = new ViewportControl[1];

            // create our viewports document.  only need 1 to hold all 4 viewportControls
            mDocument = new KeyEdit.Controls.ViewportsDocument(this.Name, KeyEdit.Controls.ViewportsDocument.DisplayMode.Single, OnViewportOpening, OnViewportClosing);
			mDocument.Resize += base.OnDocumentResized;


            //mDocument.SendToBack(); <-- used to fix control zorder issues
            // IMPORTANT: Here instead of .CreateWorkspaceDocumentTab() which automatically adds a document tab
            // to our main application window, we will use the form passed into the constructor
            //mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, GotFocus);
            mRenderPreviewForm.Controls.Add(mDocument);
            ((FormPreview)mRenderPreviewForm).TakeScreenShot += FormPreview_OnScreenShot;
            ((FormPreview)mRenderPreviewForm).Move+=  FormPreview_OnMove;
            ((FormPreview)mRenderPreviewForm).Shown += FormPreview_OnShown;
            ((FormPreview)mRenderPreviewForm).Closing += FormPreview_OnClosing; 

            mDocument.Dock = System.Windows.Forms.DockStyle.Fill;
            mDocument.Enabled = true; // preview window is immediately always enabled and visible since Show/Hide are only called for Document tabs 
            mDocument.Visible = true; // preview window is immediately always enabled and visible since Show/Hide are only called for Document tabs

            System.Diagnostics.Debug.WriteLine ("RenderPreviewWorkspace.Configre() - Preparing to Initialize Viewpoints.");
            // Clone new viewpoints from the default viewpoint supplied in scene info
            if (mViewpointsInitialized == false)
            	InitializeViewpoints();
            
            // Bind all viewports to a cloned default configured viewpoint
            BindViewpoints();


            // SetDisplayMode() in EditorWorkspace calls .Show
            // but here we dont call that so we directly call .Show.
            // Show() is required here to activate/enable/set visible the viewport controls properly right away.. or is it? Actually
            // the only reason Show() is not needed in the following case is because usually the edit tab
            // is visible when floorplan tab is created and we must explicilty click the floorplan tab
            // to switch which results in a call to .Show()
            // the caller should do it then from formmain that initiated creation of floorplan view

            if (mRenderPreviewForm.InvokeRequired)
                // TODO: I think call to Configure should already use invoker?  because i think we have
                // perhaps a problem with our viewport documents being created on wrong thread?
                mRenderPreviewForm.Invoke((System.Windows.Forms.MethodInvoker)delegate { Show(); });
            else
            	// THis is NOT showing the form, this is just calling the workspace "Show" method
                Show();

            this.Resize();
        }


        protected override void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs e)
        {
            if (mViewportControls[e.ViewportIndex] == null)
            {
                ViewportControl c = new ViewportControl();
                mViewportControls[e.ViewportIndex] = c;
                e.HostControl.Controls.Add(mViewportControls[e.ViewportIndex]);

                ConfigureViewport(mViewportControls[e.ViewportIndex], null);
                // no toolbar on this viewport control
                mViewportControls[e.ViewportIndex].ShowToolbar = false;

				// hud options
                c.Viewport.Context.ShowFPS = false;
                c.Viewport.Context.Viewport.BackColor = Keystone.Types.Color.RoyalBlue;
            //if (mContext.Hud != null && mContext.Hud.Grid != null)
            //    mContext.ShowGrid = false;
                //c.Viewport.Context.ShowEntityBoundingBoxes = true;
                //c.Viewport.Context.ShowCullingStats = true;
                //c.Viewport.Context.ShowTVProfiler = true;
                //c.Viewport.Context.ShowLineGrid = true;
            }
        }

        private void FormPreview_OnScreenShot(object sender, KeyEdit.FormPreview.ScreenShotEventArgs e)
        {
            mViewportControls[0].Viewport.Context.Screenshot(e.FullPath, e.ScreenShotSaveCompleted);
        }

        private void FormPreview_OnShown(object sender, EventArgs e)
        {
            mWorkspaceManager.CurrentWorkspace.Resize();
        }

        private void FormPreview_OnMove(object sender, EventArgs e)
        {
            mWorkspaceManager.CurrentWorkspace.Resize();
        }

        private void FormPreview_OnClosing(object sender, EventArgs e)
        {
            mWorkspaceManager.ChangeWorkspace(mPreviousWorkspace, false);
            mWorkspaceManager.Remove(this);
           
        }

        public override void Show()
        {
            //base.Show();

            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = true;
                }

            mDocument.Visible = true;
            mDocument.Enabled = true;
            mIsActive = true;
        }

        public override void Hide()
        {
            //base.Hide();

            if (mViewportControls != null)
                foreach (ViewportControl vc in mViewportControls)
                {
                    if (vc == null) continue;
                    vc.Enabled = false;
                }

            mDocument.Visible = false;
            mDocument.Enabled = false;
            mIsActive = false;
        }
    }
}
