using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using System.Diagnostics;

namespace KeyEdit.Workspaces
{
    /// <summary>
    /// Login view is just before the "intro" view, which is similar but
    /// doesnt have the login boxes.  The Login view is often skipped once a user
    /// has first created an account and logged in and selected the two "remember"  boxes.
    /// Then on future starts the game starts at the Intro view where they can elect to
    /// continue an ongoing multiplayer campaign, create a new character, etc.
    /// The intro scene will show their current character, their rank, medals/awards, and just a bunch of stats
    /// that are downloaded at startup.
    /// that will effectively keep the player tied into the netwrok even if they are playing single
    /// player campaigns.
    /// </summary>
    public class LoginWorkspace : EditorWorkspace 
    {
        internal Controls.Login mLoginControl ;

        // TODO: commented out until we get the LoginNetManager working.  
        // I'm thinking we do want to keep our authentication server seperate from everything else.
        //public LoginView(string name,string definitionpath, string layoutfilepath, KeyEdit.Network.LoginNetManager loginManager, DotNetBarManager barManager)
        //    : base(name, definitionpath, layoutfilepath, barManager)
        //{

        //    if (loginManager == null) throw new ArgumentNullException();

        //    mLoginControl = new KeyEdit.Controls.Login();
        //}

        public LoginWorkspace(string name)
            : base(name)
        {

        }

        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            base.Configure(manager, scene);
            //mLoginControl = new KeyEdit.Controls.Login();
            mViewportControls = new KeyEdit.Controls.ViewportControl[1];

            mViewportControls[0] = new ViewportEditorControl(CreateNewViewportName(0)); // the issue with these viewport indices right now is that the ini file doesnt sub-categorize viewports by the View which is something i could /should do
            ConfigureViewport(mViewportControls[0], null);
            // TODO: dock style?  for a viewportcontrol directly onto dock we should not
            // but for viewport added to viewport document we should.
            // it's the differerence between EditorWorkspace and FloorplanWorkspace.
            // wrong dock style and the toolbar gets screwed up
            mViewportControls[0].ShowToolbar = false;
            //mHideDocumentTabs = true;
            //mHideRibbonTabItems = true;
            //mHideRibbonTabGroups = true;
        }

        public override void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - No ViewManager set.");

        }

        public override void Show()
        {
            base.Show();

            DockContainerItem container = ((WorkspaceManager)mWorkspaceManager).FindDockContainer("docDock1");
            //Debug.Assert(container.Control != mLoginControl);
            Debug.Assert(container.Control != mViewportControls[0]);
            container.Control = mViewportControls[0]; // mLoginControl;
            //mLoginControl.Visible = true;
            mViewportControls[0].Visible = true;

            //mViewManager.HideDocumentTabs();
            //mViewManager.ShowDocumentTabs();

            //mViewManager.HideRibbonTabGroups();
            //mViewManager.ShowRibbonTabGroups();

            //mViewManager.HideTabItems();
            //mViewManager.ShowTabItems();



            // show the relevant toolbar tabs - Connect, Options, Help
        }

        public override void Hide()
        {
            DockContainerItem container = ((WorkspaceManager)mWorkspaceManager).FindDockContainer("docDock1");
            //Debug.Assert(container.Control == mLoginControl);
            Debug.Assert(container.Control == mViewportControls[0]);
            container.Control = null;
           // mLoginControl.Visible = false;
            
            mViewportControls[0].Visible = false;
        }
    }
}
