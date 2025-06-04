using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using System.Diagnostics;
using Keystone.Cameras;
using KeyEdit.Controls;
using Keystone.Scene;
using System.Windows.Forms;
using Keystone.Controllers;
using Keystone.Workspaces;

namespace KeyEdit.Workspaces
{
    /// <summary>
    /// Workspace Manager.  
    /// </summary>
    public class WorkspaceManager : Keystone.Workspaces.IWorkspaceManager 
    {
        public delegate bool DocumentClosingEventHandler(System.Windows.Forms.Control control);
        private DocumentClosingEventHandler mOnDocumentClosing;

        private FormMainBase mForm;
        private Settings.Initialization _iniFile;
        private IWorkspace mCurrentWorkspace; // todo: with multiple documents and workspace allowed to be open, this var no longer makes sense. All we care about is which viewports have mouse and keyboard focus
        private Dictionary<string, IWorkspace> mWorkspaces;
        protected Dictionary<string, Control> mDocuments; // all documents
        protected int m_DocumentCount = 0;
        protected int m_UniqueBarCount = 0;

        private DotNetBarManager mBarManager;

        protected string mLayoutFilePath;
        protected string mDefinitionFilePath;

        public KeyEdit.GUI.AssetClickedEventHandler AssetSelected;
        delegate void AssignControlDelegate(DockContainerItem container, Control ctl);

        InputControllerBase mIOController;
        

        public WorkspaceManager(FormMainBase frm, 
                                DotNetBarManager barManager, 
                                Settings.Initialization ini, 
                                string definitionpath, 
                                string layoutfilepath, 
                                DocumentClosingEventHandler documentClosingHandler)
        {
            if (frm == null || barManager == null || ini == null) throw new ArgumentNullException();
            
            mOnDocumentClosing = documentClosingHandler;
            mForm = frm;
            mBarManager = barManager;
            _iniFile = ini;
            // can be null, but ideally should not
            mLayoutFilePath = layoutfilepath;
            mDefinitionFilePath = definitionpath;

            mWorkspaces = new Dictionary<string, IWorkspace>();

            // NOTE: this uses a SINGLE bind configuration for ALL views open (eg. editor, simulation, floorplan view, etc)
            // NOTE: default bindings are stored in Properties.Resources.Resources.resx 
            string bindsFilePath = Settings.Initialization.GetConfigFilePath(AppMain.ConfigFolderPath, "binds_editor.config", Properties.Resources.binds_editor);
            mIOController = new InputController(bindsFilePath);
            AppMain._core.Mouse.Attach(mIOController);
            AppMain._core.Keyboard.Attach(mIOController);
        }

        ~WorkspaceManager()
        {
            AppMain._core.Mouse.Detach(mIOController);
            AppMain._core.Keyboard.Detach(mIOController);
        }

        public Form Form { get { return mForm; } }

        public string DefaultLayout { get { return KeyEdit.Properties.Resources.editor_layout; } }
        
        public string DefaultDefinition { get { return KeyEdit.Properties.Resources.editor_definition; } }

        public void Add(IWorkspace workspace, Scene scene)
        {
        	try 
        	{
        		mWorkspaces.Add(workspace.Name, workspace);
        		
        		

        	}
        	catch (Exception ex)
        	{
        		Debug.WriteLine ("WorkspaceManager.Add() - ERROR: " + ex.Message);
        	}
        }
        	

        public void Remove(IWorkspace workspace)
        {
            Remove(workspace.Name);
        }

        public void Remove(string name)
        {
            IWorkspace workspace = mWorkspaces[name];
            mWorkspaces.Remove(name);
            workspace.UnConfigure();
        }

        public IWorkspace GetWorkspace(string name)
        {
            IWorkspace ws;
            if (mWorkspaces.TryGetValue(name, out ws))
                return ws;

            return null;
        }

        // TODO: this property needs to be removed.  We should not be accessing
        // the current WorkSpace from here but rather from the document tab.
        // And actually, with docking side by side for instance, we can have multiple workspaces active at a time.
        // Although, only one 3DWorkspace can have mouse and keyboard "focus"
        public IWorkspace CurrentWorkspace
        {
            get { return mCurrentWorkspace; }
            set { mCurrentWorkspace = value; }
        }

        public void SaveLayout()
        {
            // todo: this must work with all active workspaces not just the old notion of there being a "current" workspace
            SaveCurrentLayout(mCurrentWorkspace);
        }

        public void SaveCurrentLayout(IWorkspace workspace)
        {
            if (workspace != null)
            {
                if (string.IsNullOrEmpty(mLayoutFilePath) == false)
                    mBarManager.SaveLayout(mLayoutFilePath);

                if (string.IsNullOrEmpty(mDefinitionFilePath) == false)
                    mBarManager.SaveDefinition(mDefinitionFilePath);

                // TODO: is this correct still to have unconfigure here?
                // TODO: I have no idea why we unconfigure on SaveCurrentLayout here! 
                // instead we either unload the workspace and have it unconfigure it's viewports
                // or we just call ws.SaveLayout(iniFile)
                //if (mWorkspaces != null)
                //    foreach (IWorkspace ws in mWorkspaces.Values)
                //        if (ws is WorkspaceBase)
                //        {
                //            if (((WorkspaceBase)ws).ViewportControls != null)
                //                foreach (ViewportControl vc in ((WorkspaceBase)ws).ViewportControls)
                //                    UnConfigureViewport(vc);
                //        }
            }
        }

        // TODO: i think once again i need to have each workspace use a seperate layout
        //       and although i can cache the actual controls, i must unload the containers
        //       as i switch back and forth between workspaces.
        //      This way whenever i save or load a layout, it is ONLY for the current workspace.
        // TODO: and then when restoring our Controls on the various Containers, we shoudl only
        // restore the ones to the containers that are found so as to restore the last configuration
        // fully.  users can then manually re-enable any that are not open or restore from 
        // default if things get screwed up.
        // TODO: plugin window can be a "Events" log viewer that is filtered automatically depending
        //       on the workspace and upon which entity is selected.  Teh default always goes back
        //       to the ship's.
        public void LoadLayout()
        {
            bool definitionLoaded = false;
            try
            {
                //string path = System.IO.Path.Combine(AppMain._core.ConfigPath, "layouts\\editor.definition");
                //if (!string.IsNullOrEmpty(path))
                //    mBarManager.LoadDefinition(path);
                
               
                if (!string.IsNullOrEmpty(mDefinitionFilePath))
                    mBarManager.LoadDefinition(mDefinitionFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WorkspaceManager.LoadLayout() - ERROR: " + ex.Message);
                // load the default layout that's embedded in the resx
                // NOTE: Since DotNetBarManager.LoadLayout expects a file path
                // we'll have to create a temp file.  If for some reason we're unable to 
                // create a temp file, then we have to popup a message about either
                // insufficient disk space in temp path, or no temp path or access denied
                string tempFilePath = Keystone.IO.XMLDatabase.GetTempFileName();
                System.IO.File.AppendAllText(tempFilePath, DefaultDefinition, System.Text.Encoding.UTF8);
                mBarManager.LoadDefinition(tempFilePath);
                definitionLoaded = true;
                mBarManager.SuspendLayout = true;
            }

            try
            {
                mBarManager.SuspendLayout = true; // suspends layout for all bars.  Call this again since we've loaded a new definition

                if (definitionLoaded && !string.IsNullOrEmpty(mLayoutFilePath))
                    mBarManager.LoadLayout(mLayoutFilePath);

            }
            catch (Exception ex)
            {
                // load the default layout that's embedded in the resx
                // NOTE: Since DotNetBarManager.LoadLayout expects a file path
                // we'll have to create a temp file.  If for some reason we're unable to 
                // create a temp file, then we have to popup a message about either
                // insufficient disk space in temp path, or no temp path or access denied
                string tempFilePath = Keystone.IO.XMLDatabase.GetTempFileName();
                System.IO.File.AppendAllText(tempFilePath, DefaultLayout, System.Text.Encoding.UTF8);
                mBarManager.LoadLayout(tempFilePath);
            }
            finally
            {

                // http://www.devcomponents.com/kb/questions.php?questionid=67
                // "Since Visual Studio sometimes changes the docking order inadvertently
                // it might be useful to always set the Z-Order from code on startup.
                // The Z-Order of the dock-sites is also saved and restored with the layout
                // returned by DotNetBarManager.LayoutDefinition property.
                // Therefore, if this functionality is used the Z-Order should be set to the 
                // desired state after loading the Layout.

                // mBarManager.TopDockSite.SendToBack(); // aka dockSite1
                // mBarManager.BottomDockSite.SendToBack(); // aka dockSite2
                mBarManager.LeftDockSite.BringToFront();  // aka dockSite3
                mBarManager.RightDockSite.BringToFront(); // aka dockSite4
                mBarManager.ToolbarTopDockSite.SendToBack(); // aka dockSite5
                mBarManager.ToolbarBottomDockSite.SendToBack(); // aka dockSite6
                // mBarManager.ToolbarLeftDockSite.SendToBack(); // aka dockSite7
                // mBarManager.ToolbarRighttDockSite.SendToBack(); // aka dockSite8
                mBarManager.FillDockSite.BringToFront(); // aka dockSite9

                foreach (Bar bar in mBarManager.Bars)
                    bar.RecalcLayout();

                Bar documentBar = mBarManager.Bars["barDocumentDockBar"];
                documentBar.LayoutType = eLayoutType.DockContainer;
                documentBar.AlwaysDisplayDockTab = true;
                documentBar.DockTabClosing += DocumentBar_DockTabClosing;
                documentBar.BarDock += DocumentBar_OnDock;
                documentBar.BarUndock += DocumentBar_OnUnDock;
                documentBar.ControlAdded += DocumentBar_OnControlAdded;
                documentBar.ControlRemoved += DocumentBar_OnControlRemoved;
                documentBar.DockTabChange += documentBar_TabChanged;
                mBarManager.ItemClick += dotNetBarManager_ItemClick;
                //documentBar.AcceptDropItems = false;
                System.Diagnostics.Debug.WriteLine("WorkspaceManager.LoadLayout() - bar count = " + mBarManager.Bars.Count.ToString());
                mBarManager.SuspendLayout = false;
            } 
        }

        private void DocumentBar_OnDock(object sender, EventArgs e)
        {
            if (sender is Control)
            {
                Control ctl = (Control)sender;

                System.Diagnostics.Debug.WriteLine("DocumentBar_OnDock() + " + sender.GetType().Name + " " + ctl.Name + " " + ctl.Text + " visible = " + ctl.Visible);
            }
            //mWorkspaces.Count;
            //mWorkspaces[i].Show();

            // as the mouse moves, we must constantly determine which viewport we are over so that we can attach/detach the mouse and keyboard processing for a given viewport seemlessly.
            // we should have a way to visually see which viewport is the interactive viewport
        }

        private void DocumentBar_OnUnDock(object sender, EventArgs e)
        {
            if (sender is Control)
            {
                Control ctl = (Control)sender;

                System.Diagnostics.Debug.WriteLine("DocumentBar_OnUnDock() + " + sender.GetType().Name + " " + ctl.Name + " " + ctl.Text + " visible = " + ctl.Visible);
            }

            // todo: we need to prevent docking of 3d document controls from docking anywhere except the "barDocumentDockBar" bar.
        }

        private void DocumentBar_OnControlAdded(object sender, EventArgs e)
        {
            
            if (sender is Control)
            {
                Control ctl = (Control)sender;
                 
                System.Diagnostics.Debug.WriteLine("DocumentBar_OnControlAdded() + " + sender.GetType().Name + " " + ctl.Name + " " + ctl.Text + " visible = " + ctl.Visible);
            }    

        }

        private void DocumentBar_OnControlRemoved(object sender, EventArgs e)
        {

            if (sender is Control)
            {
                Control ctl = (Control)sender;
                System.Diagnostics.Debug.WriteLine("DocumentBar_OnControlRemoved() + " + sender.GetType().Name + " " + ctl.Name + " "  + ctl.Text + " visible = " + ctl.Visible);

                if (ctl.Name == "barDocumentDockBar")
                {

                }
                
                
            }
        }


        //// TODO: the following will load  a new control as a Document tab
        //// but it doesn't seem to care which Workspace it's assigned. but this is ok
        // TODO: Only workspace manager should track document tabs!!!!  NOT individual workspaces!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        public void CreateWorkspaceDocumentTab(System.Windows.Forms.Control control, EventHandler gotFocus)
        {

            if (control != null)
            {
                DockContainerItem dock = new DockContainerItem(); //mCurrentView.DocumentCount);
                dock.Control = control;
                dock.Text = control.Name;
                dock.Visible = true;
                dock.GotFocus += gotFocus;
                // NOTE: DockStyle.Fill screws up the toolbar for some reason.  Do not USE!
                // control.Dock = System.Windows.Forms.DockStyle.Fill;

                Bar bar = FindBar("barDocumentDockBar");  

                bar.Items.Add(dock);
                if (!bar.Visible)
                    bar.Visible = true;

                bar.SelectedDockTab = bar.Items.IndexOf(dock);
                bar.RecalcLayout();
            }
        }

        private void AssignControl(DockContainerItem container, Control ctl)
        {
            container.Control = ctl;

            
            //ctl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            //ctl.Dock = DockStyle.Fill; // Fill seems to be unnecessary and in fact screws things up
        }

        public Bar FindBar(string name)
        {
            Bar result = mBarManager.Bars[name];
            
            return result;
        }

        public Bar CreateBar(string name, eDockSide dockSide)
        {
            Bar bar = new Bar(name); // initialize with Caption


            bar.Name = name;
            bar.DockSide = dockSide;
            bar.LayoutType = eLayoutType.TaskList ;
            bar.GrabHandleStyle = eGrabHandleStyle.Caption;
            bar.Stretch = true;

            mBarManager.Bars.Add(bar);

            Form.Invoke(new DockBarDelegate(DockBar), new object[] { bar, dockSide});
            

            return bar;
        }

        delegate void DockBarDelegate(Bar bar, eDockSide side);

        private void DockBar(Bar bar, eDockSide side)
        {
            mBarManager.Dock (bar, side);
            //bar.SelectedDockTab = bar.Items.IndexOf(dock);
        }

        public DockContainerItem FindDockContainer(string name)
        {
            
            object[] items = mBarManager.GetItems(name).ToArray();

            if (items == null || items.Length == 0) return null;

            // our naming must be setup so that there's only ever one item of a given name.
            System.Diagnostics.Debug.Assert(items.Length == 1);
            System.Diagnostics.Debug.Assert(items[0] is DockContainerItem);
            return (DockContainerItem)items[0];

        }

        public void UnDockControl(string containerName, string barName)
        {
            DockContainerItem container;
            Bar bar = FindBar(barName);
            if (bar == null) throw new ArgumentException();

            container = FindDockContainer(containerName);

            if (container == null) throw new ArgumentException();

            container.Control = null; 
            bar.Items.Remove(container);

            bar.RecalcLayout();
        }

        private delegate void AssignCtl (DockContainerItem container, Control c2);
        public DockContainerItem DockControl(Control ctl, string containerName, string barName, string text, eDockSide dockSide)
        {
            DockContainerItem container;
            Bar bar = FindBar(barName); 

            // containers are deserialized automatically if they are saved
            // this is problematic if we have multiple open workspaces and then
            // when starting app we load only one, there will be containers available
            // that are not relevant to that workspace...  
            // a) we could close them.....   really all we want restored are bars right?
            //   not the containers?  
            //    - but on the other hand, we do want to restore containers that are relevant
            //    We just dont want to restore those containers which are NOT relevant.
            // b) we must have different save configurations for each workspace...
            //    how do we do that since they share the same bars?
            container = FindDockContainer(containerName);

            // if container is null, then we have to create it.  
            if (container == null)
            {
                container = new DockContainerItem(containerName);
                
                // if the bar is not available, we must create it.  
                if (bar == null)
                {
                    // create bar and dock it to specified side
                    bar = CreateBar(barName, dockSide);
                }

                // add this container
                bar.Items.Add(container);

                // is it possible to order the containers?
                // i suspect we'd have to remove them and re-add them to the order we want
                container.Text = text; // is the name that appears on the tab! And if we forgot to assign .Text property it will be "" on the tab
                bar.Text = text; // this is the text that appears at the top and should change as we switch tabs to that of the current container
            }
            else
            {
                Debug.Assert(bar != null); // if the bar doesnt exist, then the container shouldnt either and thus it will have been created before reaching here

                bar.DockTabChange += bar_TabChanged;
                container.Text = text; // is the name that appears on the tab! And if we forgot to assign .Text property it will be "" on the tab
                bar.Text = text; // this is the text that appears at the top and should change as we switch tabs to that of the current container
                if (container.Control != ctl)
                    container.Control = null;
                else return container;
            }


            // http://blog.quantumbitdesigns.com/2008/07/22/delegatemarshaler-replace-controlinvokerequired-and-controlinvoke/
            // now add our browser to the control
            // obsolete? i think since now the caller to .Show already does the invoke required test
            // container.Invoke(new AssignControlDelegate(AssignControl), new object[] { container, ctl });
            if (container.InvokeRequired)
            { 
                // Jan.25.2024 - Hypnotron: Don't know why Invoke is suddenly required.... it never was before after all these years.  Started after i addded the mUnserTypieID to Entity.cs properties and moved COMPONENT_TYPE to game01.Enums from user_constants.css
                container.Invoke(new AssignCtl(AssignControl), new object[] { container, ctl });

            }
            else
            {
                AssignControl(container, ctl);
            }

           

            container.Visible = true;
            
            Debug.Assert(bar.Items.Contains(container));
            //bar.BarType = eBarType.DockWindow;
            //bar.AlwaysDisplayDockTab = true;
            container.RecalcSize();
            bar.RecalcLayout();
            return container;
        }

        // The folks at DevComponents recommends not searching for Bars because
        // those are transitory and the user can drag and drop stuff and who knows 
        // where it'll end up..  It's better to search for the DockContainerItem
        // which are like panels.


        // It does not recommend querying for a specific bar name first and then iterating through just it's items
        // because bars are transitory but the dockCOntainerItems are not (they remain the same
        // when they are docked and when they are floated.)
        // So, this view needs a method to get a dockContainerItem
        // http://www.devcomponents.com/kb/questions.php?questionid=87
        // TODO: how does save/load of layouts relate to workspaces?

        // DockSite
        //      .DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
        //      or
        //      .DocumentDockContainer = new DevComponents.DotNetBar.DocumentBarContainer();
        //      then add a Bar to it
        //
        //      .DocumentDockContainer.Documents.Add (barDocumentDockBar); <-- Can add multiple bars which will appear like nested bars
        //
        //      then to the bars themselves must add DockContainerItem  (not to be confused with DocumentDockContainer)
        // 
        // barDocumentDockBar.Add (containerItem) <-- CAN ADD MULTIPLE CONTAINERS which will show up as TABSTRIP Page items
        // or
        // barDocumentDockBar.Controls.Add(containerItem);
        // then finally to the containerItem
        // we can add controls
        // 

        // - there is only one dotnetbar manager, i think that alone requires us to
        //   only have 1 layout
        // - workspaces are either visible/hidden but generally their layouts should be viewed
        //   as always live?
        //      - we can hide/show them when applicable, but when saving they are still 
        //      saved even when hidden and they are restored when hidden too.
        // - perhaps our layouts define what docks/bars are active, but here we can control
        //   restoration to those docs and bars?  

        // TODO: hiding the entire container works.  
        // so a "dockContainerItem" represents an entire page.  So next test will be
        // splitting a bar horizontally
        #region Shared control management - including Viewports
        internal Bar GetFirstDocumentBar()
        {
            foreach (Bar b in mBarManager.Bars)
            {
                if (b.DockSide == eDockSide.Document && b.Visible)
                    return b;
            }
            return null;
        }
        #endregion 

        // todo: this should be more like SetFocus() and occur on mousemove perhaps
        public void ChangeWorkspace(string workspaceName, bool savePreviousLayout)
        {
            // no need to change if it's the same as current
            if (mCurrentWorkspace != null && mCurrentWorkspace.Name == workspaceName) return;
            if (!mWorkspaces.ContainsKey(workspaceName)) return;

            // TODO: I think this entire method should be deleted.  There is no need to attach/detach the mouse since that is handled
            //       by determining the mouse over viewpoint code.  The only thing we have to do is to test if a document is visible before rendering to any of it's viewports. 
            //       And the visible test only really occurs when multiple documents are assigned to the same document bar.
            IWorkspace previousView = mCurrentWorkspace;
            mCurrentWorkspace = mWorkspaces[workspaceName];

            // TODO: the problem with trying to hide previous workspace is that the previous workspace might still be visible and just docked along side the new workspace
            ////if (previousView != null)
            ////{

            ////    if (savePreviousLayout) SaveCurrentLayout(previousView);
            ////    if (mForm.InvokeRequired)
            ////        mForm.Invoke((System.Windows.Forms.MethodInvoker)delegate { previousView.Hide(); });
            ////    else
            ////        previousView.Hide();
            ////    // todo:  when detecting mouse picking , i definetly do need to take into account zorder for overlapped (undocked) viewports. 
            ////    //        In fact, it is the zorder issue that is preventing the floorplan view from receiving mouse input when docked ontop the Main Viewer document.
            ////    //        While im at it, i should probably add a .Visible property to the control (window) as well.
            ////}
            
            

            // NOTE: the call to workspace.Show() sets the workspace.IsActive = true
       // TODO: I think below is obsolete because we can have multiple documents/workspaces open at once including some docked and undocked 
            if (mForm.InvokeRequired)
                mForm.Invoke((System.Windows.Forms.MethodInvoker)delegate { mCurrentWorkspace.Show(); });
            else
                mCurrentWorkspace.Show(); // todo: i dont think we need this either.  The form activates on load and all we need to do is determine if it's visible and can render or receive "Focus"
        }



        public void Resize()
        {
            if (mWorkspaces != null)
                foreach (IWorkspace ws in mWorkspaces.Values)
                    ws.Resize();

            foreach (Bar b in mBarManager.Bars)
                b.RecalcLayout();
        }

        private void HideDocumentTabs(bool value)
        {
            foreach (Bar b in mBarManager.Bars)
            {
                if (b.DockSide == eDockSide.Document && b.Visible)
                    b.AlwaysDisplayDockTab = !value;
            }
        }

        // KeyScript.Delegates.EntityDelegates.SpawnEventHandler mOnSpawn;
        //http://www.codeplex.com/wikipage?ProjectName=Dynamic  // dynamic reflection & dynamic code generation
        #region Dotnet Document Bar Events
        private void documentBar_TabChanged(object sender, DevComponents.DotNetBar.DockTabChangeEventArgs e)
        {

            switch (e.NewTab.Text)
            {
                default:
                    System.Diagnostics.Debug.WriteLine("WorkspaceManager.documentBar_TabChanged() - Tab changed to '" + e.NewTab.Text + "'.");
                    ChangeWorkspace(e.NewTab.Text, false);

                    // i need to consolidate these things
                    // document tab name
                    // game.name  - a game id is unique to every game ever played
                    // simulation.name
                    // scene.name - a scene is independant of game.  
                    // scene.filename 

                    break;
            }
        }

        /// <summary>
        /// Event handler for when a new tab is selected on a Bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_TabChanged(object sender, DevComponents.DotNetBar.DockTabChangeEventArgs e)
        {
            if (sender is Bar && e.NewTab != null)
            {
                // update the text to match the newly selected tab
                Bar b = (Bar)sender;
                b.Text = e.NewTab.Text;
            }
        }

        private void dotNetBarManager_ItemClick(object sender, System.EventArgs e)
        {
            //    BaseItem item = sender as BaseItem;
            //    if (item == null) return;

            //    if (item.Name == "buttonNew")
            //    {
            //        //CreateNewDocument(3);
            //    }
            //        // Activate the form
            //   // else if (item.Name == "window_list")
            //       //((Form)item.Tag).Activate();
            //   // else if (item == bThemes)
            //        //EnableThemes(bThemes);
            //    else if (item.GlobalName == buttonTextColor.GlobalName && mViewManager.ViewportControls != null)
            //    {
            //        _backColor = new Keystone.Types.Color(((ColorPickerDropDown)item).SelectedColor.R,
            //            ((ColorPickerDropDown)item).SelectedColor.G,
            //            ((ColorPickerDropDown)item).SelectedColor.B,
            //            ((ColorPickerDropDown)item).SelectedColor.A);

            //        _core.SetBackGroundColor(_backColor);
            //    }
            //    //else if (activedocument != null)
            //    //{
            //    //    // Pass command to the active document
            //    //    // Note the usage of GlobalName property! Since same command can be found on both menus and toolbars, for example Bold
            //    //    // you have to give two different names through Name property to these two instances. However, you can and should
            //    //    // give them same GlobalName. GlobalName will ensure that properties set on one instance are replicated to all
            //    //    // objects with the same GlobalName. You can read more about Global Items feature in help file.
            //    //    activedocument.ExecuteCommand(item.GlobalName, null);
            //    //}
        }


        private void DocumentBar_DockTabClosing(object sender, DevComponents.DotNetBar.DockTabClosingEventArgs e)
        {
            //DialogResult r = MessageBox.Show("Document '" + e.DockContainerItem.Text + " is about to close. Close it?", "Document Docking", MessageBoxButtons.YesNo);
            //if (r == DialogResult.No)
            //    e.Cancel = true;

            // In this event you save any changes to the active document or cancel the closing...
            // e.DockContainerItem returns the reference to the item being closed
            // Set e.Cancel to true to cancel the closing
            // Set e.RemoveDockTab to true to automatically remove the closed tab from Bar.Items collection
            Bar b = (Bar)sender;
            DockContainerItem item = e.DockContainerItem;

            bool cancel = false;

            if (mOnDocumentClosing != null)
            {
                cancel = mOnDocumentClosing.Invoke(item.Control);
            }
            e.RemoveDockTab = !cancel;
            e.Cancel = cancel;

            // NOTE: Wrong, we never remove the DocumentBar or else we wont be able to re-load a new scene
            //if (!e.Cancel )
            //    if (b.Items.Count == 1) // Remove bar if last item is closed...
            //        mBarManager.Bars.Remove(b);
        }
        #endregion

        /// <summary>
        /// Notifies each workspace that a node has been created. NOTE: This is not just nodes added to the Scene but rather any node that has been created via a Node_Create.cs message received from the server.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="childID"></param>
        /// <param name="childTypeName"></param>
        public void NotifyNodeAdded(string parentID, string childID, string childTypeName)
        {
            if (mWorkspaces != null && mWorkspaces.Count > 0)
                foreach (IWorkspace ws in mWorkspaces.Values)
                {
                    ws.OnNodeAdded(parentID, childID, childTypeName);
                }
        }

        public void NotifyNodeRemoved(string parentID, string childID, string childTypeName)
        {
            if (mWorkspaces != null && mWorkspaces.Count > 0)
                foreach (IWorkspace ws in mWorkspaces.Values)
                {
                    ws.OnNodeRemoved(parentID, childID, childTypeName);
                }

        }
        // OBSOLETE - ViewpointController refactor now has ws.OnEntitySelected() invoked for the specific workspace in use
        //        // note: this is only ever called by the InputController when an entity is clicked in
        //        //       the 3d scene.  Treview selected entities on the other hand directly call the
        //        //       OnSelected for the workspace it is associated with.
        //        // TODO: this scene should be only necessary to help us filter which workspaces
        //        // to send to since each workspace is 
        //        internal void Entity_OnSelected(RenderingContext context, System.Drawing.Point vpRelativeMousePos, Keystone.Collision.PickResults pickResults)
        //        {
        //            Scene scene = context.Scene;
        //            // NOTE: We call .OnEntitySelected for NULL selection as well as non null so
        //            // that the huds and workspaces can clean up gui options for when no entity selection exists
        //
        //             if (mWorkspaces != null && mWorkspaces.Count > 0)
        //                foreach (IWorkspace ws in mWorkspaces.Values)
        //                {
        //                    // TODO: rather than send to every workspace, shoudlnt it only
        //                    // be to the current workspace because we want other workspaces
        //                    // to be contextual.  Afterall, not every workspace will allow
        //                    // selection of all entity types.  Nav screen for example is about
        //                    // selecting nav locations and switching worksapces, selecting a different entity
        //                    // then going back to Nav should nto alter that.
        //                    // TODO: but that also means that when updates occur to an entity that
        //                    //       is selected in any particular workspace, modifications to those
        //                    //       entities should be relayed to those workspaces.
        //                    ws.OnEntitySelected(context, vpRelativeMousePos, pickResults);
        //                }
        //
        //            if (pickResults.Entity != null)
        //                System.Diagnostics.Trace.WriteLine(
        //                    string.Format("WorkspaceManager.EntitySelected() - {0} {1}.",
        //                    pickResults.Entity.TypeName,  
        //                    pickResults.EntityID));
        //            else
        //                System.Diagnostics.Trace.WriteLine("WorkspaceManager.EntitySelected() - NONE");
        //            
        //        }
        //
        //        internal void Entity_OnMouseOver(RenderingContext context, System.Drawing.Point vpRelativeMousePos, Keystone.Collision.PickResults pickResults)
        //        {
        //            // Trace.WriteLine(string.Format("WorkspaceManager.EntityMouseOver - Entity {0}.", pickResults.Entity));
        //            // TODO: ideally id rather highlight the entity somehow... either change bounding box color slightly
        //            // if bounding volumes are enabled, or change material color slightly to be brighter
        //            // or if group select, change the group to highlighted color
        //        }
    }
}
