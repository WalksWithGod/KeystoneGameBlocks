using System;
using System.Diagnostics;
using System.Windows.Forms;
using Keystone.Resource;
using Silver.UI;
using KeyEdit.Controls;

namespace KeyEdit.Workspaces
{
	public abstract class WorkspaceBase3D : WorkspaceBase, Keystone.Workspaces.IWorkspace3D
	{
		// display mode holds the configuration of Viewports in our ViewportsDocument
        protected KeyEdit.Controls.ViewportsDocument.DisplayMode mDisplayMode;

		protected Keystone.EditTools.Tool mCurrentTool;
        protected Keystone.EditTools.Tool mDefaultTool;
        protected Keystone.Entities.Entity mSelectedEntity;
        protected string mSelectedEntityID;
        protected string mSelectedEntityTypeName;
        
        protected bool mViewpointsInitialized = false;
        
        protected bool mQuickLookHostPanelDocked = false;
        protected bool mQuickLookHostPanelInitialized = false;
        protected bool mPluginPanelInitialized = false;

        protected System.Windows.Forms.Control mQuickLookHostPanel;
        //protected KeyEdit.GUI.QuickLook3 mQuickLookPanel;
        
		protected delegate void PluginSelectDelegate(KeyPlugins.IPlugin plugin, string resourceID, string resourceTypeName, Keystone.Collision.PickResults pickResults);
        protected KeyPlugins.AvailablePlugin mCurrentPlugin;

        public KeyPlugins.AvailablePlugin CurrentPlugin { get { return mCurrentPlugin; } }
        
        
        internal WorkspaceBase3D(string name) : base (name)
        {
        	SelectTool (Keystone.EditTools.ToolType.Select);
        	
            mDisplayMode = KeyEdit.Controls.ViewportsDocument.DisplayMode.Single;

            mDoubleClickTimer = new System.Timers.Timer();
            mDoubleClickTimer.Interval = 100;
            mDoubleClickTimer.Elapsed += ClickTimer_Tick;
        }
        
                      
        protected virtual void OnGotFocus(Object sender, EventArgs e)
        {
        	System.Diagnostics.Debug.WriteLine(this.GetType().Name + ".GotFocus() - ");
        }
                
        
        // TODO: the main problem with SelectTool is that in game, user should be 
        // able to select a tool for input capture in game such as elevator button or 
        // some lever, valve, airlock control, etc.  But that sort of tool selection
        // then needs to come via Picking as opposed to manually setting via a viewport toolbar
        // But this is ok... the real problem then is, if in game users Pick a Control.cs instance
        // whereas here, we assign a Tool... what is difference between the two then again?  Hrm...
        // actually I think a seperate Tool distinguishing is ok. Tools are somewhat unique in that
        // they are used to modify and edit the game world.  In game controls are just responding 
        // to events and running the triggered functions which can be scripts or delegates.
        // TODO: below is mostly the default tools for Editor.  But we need a way to assign
        // a CurrentTool from runtime.
        public virtual void SelectTool(Keystone.EditTools.ToolType toolType)
        {
        	switch (toolType)
           	{
                    // TODO: shouldn't the CurrentTool be per context as well?
                    // ugh... viewports complicates things
               case Keystone.EditTools.ToolType.Select :
                   CurrentTool = new KeyEdit.Workspaces.Tools.SelectionTool(AppMain.mNetClient);
                    break;
               case Keystone.EditTools.ToolType.InteriorSelectionTransformTool:
                    CurrentTool = new Tools.InteriorTransformTool(AppMain.mNetClient);
                    break;
               case Keystone.EditTools.ToolType.NavPointPlacer:
                    CurrentTool = new KeyEdit.Workspaces.Tools.NavPointPlacer(AppMain.mNetClient);
                    break;
               case Keystone.EditTools.ToolType.Position :
                   CurrentTool = new Keystone.EditTools.MoveTool(AppMain.mNetClient);
                   break;

               case Keystone.EditTools.ToolType.Rotate :
                   CurrentTool = new Keystone.EditTools.RotateTool(AppMain.mNetClient);
                   break;

               case Keystone.EditTools.ToolType.Scale :
                   CurrentTool = new Keystone.EditTools.ScaleTool(AppMain.mNetClient);
                   break;
                
                case Keystone.EditTools.ToolType.HingeEdit:
                   CurrentTool = new Keystone.EditTools.HingeTool(AppMain.mNetClient); 
                   break;
               default:
                   break;
           }

        }

        public virtual Keystone.EditTools.Tool CurrentTool 
        {
            get { return mCurrentTool; }
            set
            {
                if (mCurrentTool != null && mCurrentTool != value)
                {
                    mCurrentTool.Dispose();
                    mCurrentTool = null;
                }

                mCurrentTool = value;

                // TODO: And here we want perhaps some events to be assignable by the Controller
                // so that the Tool can invoke them
                // 1 - OnSnap (when an item being moved hits a snap point)
                // 2 - When the mouse over grid cell has changed so the grid can be
                //     ugh but wait... isnt the point of using a Tool so i can keep a lot of 
                //     that crap code in there?  I mean sure for specific generic functions like
                //     Adding an object to a scene, sure...
                //     And for that, maybe it should be simply an event that the KeyEditor has to set
                //     and respond to....
                // - OnSelect <-- when mouse is down and user is dragging, when the cell changes
                //            then an OnSelect event is generated again and continues so that this tool
                //            can be used as a paintbrush 
                //            NOTE: normally this could be configured to generate based on 
                //            different grid cell dimensions so granular or less so depending on
                //            grid size, but it's the tool itself that determines when
                //            grid location has changed.
                //  OK, so how about this, the only code i need to implement now that is 
                //  effectively new is determining which grid cell we're over, and notifying
                //  Grid cell changed and then allowing the calling app to highlight that cell
                //  Differently.
                //  ACTUALLY, a GREAT architectural plus for us is RenderingContext is what
                //  actually handles our "DrawGrid" function as well as having the grid dimension
                // values to use which are easily accessible from within our Tool


                // update the mouse cursor.  //note: when switching the Viewport layout after having set edittool, 
                // ViewManager.ConfigureViewport() handles setting new viewports cursors and backcolor to match the primary viewports settings
                // TODO: i think currenttool should maybe belong in workspace or renderingcontext... hrm...

//                if (mCurrentTool == null || mCurrentTool is SelectionTool)
//
//                    foreach (Viewport vp in AppMain._core.Viewports.Values)
//                        vp.Cursor = System.Windows.Forms.Cursors.Default;
//                else
//                    foreach (Viewport vp in AppMain._core.Viewports.Values)
//                        vp.Cursor = System.Windows.Forms.Cursors.Cross;
            }
        }

        // LEFT BEGIN
        public void ToolMouseBeginLeftButtonSelect (Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition)
        {
        	Keystone.Events.MouseEventArgs arg = vp.GetMouseHitInfo(mouseScreenPosition.X, mouseScreenPosition.Y);
            arg.Button = Keystone.Enums.MOUSE_BUTTONS.LEFT;

            if (mCurrentTool != null)
            {
                mCurrentTool.HandleEvent(Keystone.Events.EventType.MouseDown, arg);
                arg.Data = mCurrentTool.PickResult;
                if (arg.Data == null) return;
                mCurrentTool.PickResult.Context = vp.Context;
                mCurrentTool.PickResult.vpRelativeMousePos = arg.ViewportRelativePosition;
                
                // invoke the handler in formmain or whatever calling app  
                OnEntitySelected (mCurrentTool.PickResult);              
            }
        }
        
        // LEFT END
        public void ToolMouseEndLeftButtonSelect (Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition)
        {
        	Keystone.Events.MouseEventArgs arg = vp.GetMouseHitInfo(mouseScreenPosition.X, mouseScreenPosition.Y);
            arg.Button = Keystone.Enums.MOUSE_BUTTONS.LEFT;

            Keystone.Collision.PickResults pickResult = new Keystone.Collision.PickResults();
            pickResult.Context = vp.Context;
            arg.Data = pickResult;
            // TODO: the need to check for input capture confuses me...  why should the tool
            // not always have the input capture?  i forget my reasoning here... 
            // Isn't the tool always active and needs mouse event input changes so long as it's
            // active?  Do i need to just delete the .HasInputCapture() property from Tools since
            // it's irrelevant?
            if (mCurrentTool != null ) //&& mCurrentTool.HasInputCapture())
            {
                mCurrentTool.HandleEvent(Keystone.Events.EventType.MouseUp, arg);
               
            }
        }

        // RIGHT begin
        public void ToolMouseBeginRightButtonSelect(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition)
        {
            Keystone.Events.MouseEventArgs arg = vp.GetMouseHitInfo(mouseScreenPosition.X, mouseScreenPosition.Y);
            arg.Button = Keystone.Enums.MOUSE_BUTTONS.RIGHT;

            if (mCurrentTool != null)
            {
                mCurrentTool.HandleEvent(Keystone.Events.EventType.MouseDown, arg);
                arg.Data = mCurrentTool.PickResult;

                mCurrentTool.PickResult.Context = vp.Context;
                mCurrentTool.PickResult.vpRelativeMousePos = arg.ViewportRelativePosition;

                // invoke the handler in formmain or whatever calling app  
                OnEntitySelected(mCurrentTool.PickResult);
            }
        }

        // RIGHT end
        public void ToolMouseEndRightButtonSelect(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition)
        {
            Keystone.Events.MouseEventArgs arg = vp.GetMouseHitInfo(mouseScreenPosition.X, mouseScreenPosition.Y);
            arg.Button = Keystone.Enums.MOUSE_BUTTONS.RIGHT;

            Keystone.Collision.PickResults pickResult = new Keystone.Collision.PickResults();
            pickResult.Context = vp.Context;
            arg.Data = pickResult;
            // TODO: the need to check for input capture confuses me...  why should the tool
            // not always have the input capture?  i forget my reasoning here... 
            // Isn't the tool always active and needs mouse event input changes so long as it's
            // active?  Do i need to just delete the .HasInputCapture() property from Tools since
            // it's irrelevant?
            if (mCurrentTool != null) //&& mCurrentTool.HasInputCapture())
            {
                mCurrentTool.HandleEvent(Keystone.Events.EventType.MouseUp, arg);

            }
        }

        public void ToolMouseMove(Keystone.Cameras.Viewport vp, System.Drawing.Point mouseScreenPosition )
        {
        	Keystone.Events.MouseEventArgs arg = vp.GetMouseHitInfo(mouseScreenPosition.X, mouseScreenPosition.Y);

            // NOTE: The above if/else falls through and we continue to then test for tool input
            if (mCurrentTool == null) return; //&& mCurrentTool.HasInputCapture())
            
            mCurrentTool.HandleEvent(Keystone.Events.EventType.MouseMove, arg);
            vp.Context.Workspace.MouseOverItem = mCurrentTool.PickResult;

            if (mCurrentTool.PickResult == null) return;

            mCurrentTool.PickResult.Context = vp.Context;
            mCurrentTool.PickResult.vpRelativeMousePos = arg.ViewportRelativePosition;
            if (mCurrentTool.PickResult != null && mCurrentTool.PickResult.Entity != null)
                // TODO: why not just mContext.MouseOver (arg.ViewportRelativePosition, mCurrentTool.PickResult);
             	// there's no point in having these handlers here is there?  
                OnEntityMouseOver(mCurrentTool.PickResult);
            
        }

        public void ToolKeyDown(string key)
        {
            System.Diagnostics.Debug.WriteLine("key down = " + key);
            Keystone.Events.KeyboardEventArgs args = new Keystone.Events.KeyboardEventArgs();
            args.Key = key;
            args.IsPressed = true;
            mCurrentTool.HandleEvent(Keystone.Events.EventType.KeyboardKeyDown, args);
        }

        public void ToolKeyUp(string key)
        {
            System.Diagnostics.Debug.WriteLine("key up = " + key);
            Keystone.Events.KeyboardEventArgs args = new Keystone.Events.KeyboardEventArgs();
            args.Key = key;
            args.IsPressed = false;
            mCurrentTool.HandleEvent(Keystone.Events.EventType.KeyboardKeyUp, args);
        }

        public void ToolCancel()
        {
        	if (mCurrentTool != null)
                // send cancel to the tool so it can forward to any GUI elements that have input capture
                mCurrentTool.HandleEvent(Keystone.Events.EventType.KeyboardCancel, null);

            // switch back to the default selection tool
            SelectTool (Keystone.EditTools.ToolType.Select);
        }

        // TODO: how do we also track mSelectedEntity in HUD which also wants to be 
        //       notified of changes in selection.  HUD.OnEntitlySelected invoke for each viewport control?

        System.Timers.Timer mDoubleClickTimer;
        DateTime mLastMouseClick;
        float mDoubleClickInterval = 250f;
        int mMilliseconds = 0;
        bool mIsFirstClick = true;
        bool mIsDoubleClick = false;
        string mLastClickID;
        public override void OnEntitySelected(Keystone.Collision.PickResults pickResult)
        { 
        	base.OnEntitySelected (pickResult);
            mPickResult = pickResult;        	
        	
        	// determine if click or double click
    		if (mIsFirstClick)
    		{
    			mIsFirstClick = false;
                mDoubleClickTimer.Start();
            }
    		else 
    		{
                if (mMilliseconds < SystemInformation.DoubleClickTime)
                {
                    // double Click, reset
                    mIsFirstClick = false;
                    mIsDoubleClick = true;
                }
    		}
        	
        	mLastClickID = mPickResult.EntityID;
        }

        void ClickTimer_Tick(object sender, EventArgs e)
        {
            mMilliseconds += 100;
            if (mMilliseconds >= SystemInformation.DoubleClickTime)
            {
                mDoubleClickTimer.Stop();

                if (mIsDoubleClick)
                {
                    DoDoubleClick();
                }
                else
                {
                    DoSingleClick();
                }

                mIsFirstClick = true;
                mIsDoubleClick = false;
                mMilliseconds = 0;
            }
        }

        private void DoDoubleClick()
        {
            // fire appropriate hud events
            // if scene treeview node is selected, Context will be null
            if (mPickResult.Context != null)
                mPickResult.Context.Hud.OnEntityDoubleClicked(mPickResult);
        }

        private void DoSingleClick()
        {
            if (mPickResult == null) return;
            // single click action
            // TODO: Does this occur on RIGHT MOUSE CLICK too?  I suspect not only because then i think
            //       mouse look occurs
            // TODO: We do not want all entities to modify mSelectedEntity.
            //       Hud Controls when they are mouse picked for instance, should not modify the existing Workspace3D.mSelectedEntity.
            Keystone.Entities.Entity selectedEntity = mPickResult.Entity;

            if (selectedEntity is Keystone.Entities.IEntityProxy)
            {
                Keystone.Entities.IEntityProxy proxy = selectedEntity as Keystone.Entities.IEntityProxy;
                if (proxy.ReferencedEntity != null)
                {
                    selectedEntity = proxy.ReferencedEntity;
                    mPickResult.SetEntity(selectedEntity);
                }
            }
            else if (selectedEntity is Keystone.Simulation.IEntitySystem)
            {
                // Digest or IEntitySystem
                throw new NotImplementedException();
            }
            // NOTE: It's important that the pickResult.SetEntity() is of type IEntitySystem
            //       so that we know that the pickResult.EntityID and EntityTypeName are for referenced Entities
            //       that might need to be paged in.
            else if (selectedEntity is Keystone.Elements.Background3D || selectedEntity is Keystone.Entities.ModeledEntity)
            {
                Keystone.Entities.ModeledEntity modeledEntity = (Keystone.Entities.ModeledEntity)selectedEntity;
                if (modeledEntity.Model == null && modeledEntity.SelectModel(0) != null)
                {
                    // we clicked a ModelSequence
                    Keystone.Elements.Model model = modeledEntity.SelectModel(0);

                }
                else if (modeledEntity.Model != null && modeledEntity.Model.Geometry is Keystone.Elements.MinimeshGeometry)
                {
                    int index = mPickResult.FaceID;
                    Debug.Assert(index >= 0);

                    // grab all Star records
                    Database.AppDatabaseHelper.StarRecord[] records = Database.AppDatabaseHelper.GetStarRecords();
                    Debug.Assert(records != null && records.Length > 0);
                    Debug.Assert(index < records.Length);

                    Database.AppDatabaseHelper.StarRecord record = records[index];

                    // TODO: if we don't sychrnously page in the entity here, then we screw up the Plugin notification and QuickLook panel notifications.

                    // NOTE: picker.cs.UpdatePickResults() is where
                    if (index == -1 || index >= records.Length) return; // TODO: use assert for starIndex >= RecordCount after i fix bug
                                                                        // TODO: why is a world showing up as the PickResult typename? Only occurs after going to systemmap with planets
                    string selectedStarID = record.ID;
                    string typename = "Star";
                    const bool recurse = true;
                    const bool clone = false;
                    // System.Diagnostics.Debug.Assert(mPickResult.EntityTypeName == "Star");
                    // Nov.10.2016 - NOTE: We do not call Repository.Create() because
                    //               that call does not result in the Entity being paged in.  It just creates a new Entity
                    //               or grabs cached copy.  
                    //selectedEntity = (Keystone.Entities.Entity)Repository.Create (record.ID, record.TypeName);
                    selectedEntity = (Keystone.Entities.Entity)this.ViewportControls[0].Context.Scene.XMLDB.ReadSynchronous(typename, selectedStarID, recurse, clone, null, true, false);

                    Keystone.IO.PagerBase.LoadTVResource(selectedEntity, false);
                    mPickResult.SetEntity(selectedEntity);
                }
            }

            // OBSOLETE - selectedEntity should never be null.  If it's an IEntitySystem then selectedEntity should reference that IEntitySystem
            //			if (selectedEntity == null && string.IsNullOrEmpty(mPickResult.EntityID) == false)
            //			{
            //				// Repository.Create() does not page in Entity!  It just creates it or grabs existing from cache
            //				selectedEntity = (Keystone.Entities.Entity)Repository.Create (mPickResult.EntityID, mPickResult.EntityTypeName);
            //				Keystone.IO.PagerBase.LoadTVResource (selectedEntity, false);
            //				mPickResult.SetEntity (selectedEntity);
            //			}

            // Dec.28.2022 - the below IncrementRef() and DecrementRef() is not the way to go. If an Entity is paged out
            //               we should merely adapt and clear the mPickResult.SelectedEntity.  As is, by IncrementRef()
            //               we are preventing the Entity or Region from reaching a refcount == 0 and being completely removed
            //               when desired.
            //// prevent any selected entity from being paged out completely by raising it's ref count
            //if (mSelectedEntity != selectedEntity && selectedEntity != null)
            //    // increment new selected before decrement previous below
            //    try
            //    {
            //        Repository.IncrementRef(selectedEntity);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Workspace3D.OnEntitySelected() - ERROR:" + ex.Message);
            //    }

            //// only decrease the ref count if a different NON NULL entity has been selected
            //if (mSelectedEntity != null && mSelectedEntity != selectedEntity)
            //    try
            //    {
            //        // TODO: Shouldn't we also decrementref the last mSelectedEntity when this workspace closes?
            //        Repository.DecrementRef(mSelectedEntity);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Workspace3D.OnEntitySelected() - " + ex.Message);
            //    }


            // TODO: how do we indicate to quicklook panel that target data is being retrieved from db?			
            mSelectedEntity = selectedEntity;

            if (selectedEntity is Keystone.Controls.Control)
            {
                // mPickResult gets passed on to the Contextualize and NotifyPlugin calls
                mPickResult.SetEntity(null);
                // NOTE: We do not alter this.Selected   because we want it to
                // maintain whatever entity was selected previously
                // if it's a control for a door keypad as opposed to a 2d HUD menu we may want to make exception
            }
            // NOTE: we want to be able to select Background3D from SceneExplorer but not from Viewport
            //else if (selectedEntity is Keystone.Elements.Background3D)
            //{
            //    mPickResult.SetEntity(null);
            //    this.SelectedEntity = null;
            //}
            else
                this.SelectedEntity = mPickResult;


            // TODO: here, ContextualizeToolbar and ContextualizeHUDs should be grabbed from the Entity's Script if it has
            //       a contextualized effect on the Current Tool/Toolbar/HUD/Menu system.  It's extremely crappy to try and just do that 
            //       with overrides here and with hardcoded if/else based on the entity type.  So for instance, by default
            //       clicking on some Entity type may contextualize the tool to be a right mouse click "MOve To" command tool.
            //       It may also result in a command menu on the toolbar that has "Patrol, Attack, Defend, etc" RTS style pathing + AI commands.

            // TODO: perhaps something like the following call will allow us to use scripting API to then modify the
            //       tool, toolbar, menu, plugin, etc from script rather than from EXE code.
            // previousPickResult.Entity.Execute("on_selection_lost", workspace.Name, context.Name);
            if (this.SelectedEntity != null && this.SelectedEntity.Entity != null && this.SelectedEntity.Context != null)
                this.SelectedEntity.Entity.Execute("OnSelected", new object[] { this.SelectedEntity.Entity.ID, this.Name, this.SelectedEntity.Context.ID });

            if (mPickResult == null) return;

            if (string.IsNullOrEmpty(mPickResult.EntityID) == false)
            {
                // TODO: if we notify plugin of an entity that is not in Repository, then it will not load
                // Can we implement a mechanism to load that target entity in the workspace as selected and incrementref it
                // and decrementref it when we change?  wouldnt that be easiest?
                KeyCommon.Messages.NotifyPlugin_NodeSelected internalMessage = new KeyCommon.Messages.NotifyPlugin_NodeSelected();
                internalMessage.NodeID = mPickResult.EntityID;
                internalMessage.Typename = mPickResult.EntityTypeName;
                AppMain.mNetClient.SendMessage(internalMessage);
                //NotifyPlugin(mPickResult);
            }

            // if scene treeview node is selected, Context will be null
            if (mPickResult != null && mPickResult.Context != null && mPickResult.Context.Hud != null)
                mPickResult.Context.Hud.OnEntityClicked(mPickResult);
        }


        public static string GetLabelText(string name, string typeName)
        {
            string result = typeName;
            if (string.IsNullOrEmpty(name)) return result;

            if (name != result)
            {
                if (name.ToUpper() != result.ToUpper())
                    result += ", " + name;
            }
            return result;
        }

        private DevComponents.DotNetBar.DockContainerItem mPluginContainer;

        public virtual void NotifyPlugin()
        {
            // TODO: Feb.23.2017 - I think our plugin isn't loading and that is also preventing
            //                     the quicklookpanel from loading
            KeyPlugins.AvailablePlugin plugin;
            string containerText = null;

            if (AppMain._core.SceneManager.Scenes[0].Simulation.CurrentMission != null && AppMain._core.SceneManager.Scenes[0].Simulation.CurrentMission.Enable)
            {
                plugin = AppMain.PluginService.SelectPlugin("C&C", null);
                containerText = "Command && Control";

            }
            else if (mCurrentPlugin != null && mCurrentPlugin.Instance.Name == "Command & Control")
            {
                // null and undock this plugin
                //((WorkspaceManager)mWorkspaceManager).UnDockControl("plugin container", "plugin bar");
                ((WorkspaceManager)mWorkspaceManager).DockControl(null, "plugin container", "plugin bar", containerText, DevComponents.DotNetBar.eDockSide.Bottom);

                mCurrentPlugin = null;
                return;
            }
            else
            {
                if (mPickResult == null || mPickResult.Entity == null) return;
                plugin = AppMain.PluginService.SelectPlugin(null, "Entity");
                containerText = mPickResult.Entity.Name;
                containerText = GetLabelText(containerText, mPickResult.EntityTypeName); // + " - " + mPickResult.EntityID;
                                                                                         //container.Parent.Text = container.Text;
            }

            // when the IPlugin activates, it'll request updated state information
            if (plugin == null)
            {
                System.Diagnostics.Debug.WriteLine("WorkspaceBase3D.NotifyPlugin() - ERROR: Plugin not found!");
                MessageBox.Show("Plugin not found!");
            }
            else
            {

                mPluginContainer =
                    ((WorkspaceManager)mWorkspaceManager).DockControl(plugin.Instance.MainInterface, "plugin container", "plugin bar", containerText, DevComponents.DotNetBar.eDockSide.Bottom);

                string selectedID;
                string selectedTypeName;

                // mPickResult.Entity is currently only for EntityEdit plugin. C&C uses AppMain.mLocal
                if (mPickResult != null && mPickResult.Entity != null)
                {
                    selectedID = mPickResult.Entity.ID;
                    selectedTypeName = mPickResult.Entity.TypeName;
                }
                else
                {
                    selectedID = AppMain.mPlayerControlledEntity.ID;
                    selectedTypeName = AppMain.mPlayerControlledEntity.TypeName;
                }

                if (plugin != mCurrentPlugin)
                {
                    mCurrentPlugin = plugin;
                    
                    // TODO: a system to register specific types of notifications to a plugin
                    // based on the plugin's list of supported types.  or a way for the plugin
                    // itself to register for specific types of notifications on demand.
                    // TODO: fix           mCurrentPlugin.Instance.EditResourceRequest += Plugin_OnEditResource;
                }
                // if it's the same plugin as last time, see if it's also the same
                // resource being used for the data.  If so then no need to plugin.SelectTarget
                else if (plugin.Instance.TargetID == selectedID)
                {
                }



                try
                {
                    plugin.Instance.Host.PluginChangesSuspended = true;
                    if (plugin.Instance.MainInterface.InvokeRequired)
                    {
                        plugin.Instance.MainInterface.Invoke(new PluginSelectDelegate(PluginSelectInvoke), new object[] { plugin.Instance, selectedID, selectedTypeName, mPickResult});
                    }
                    else
                    {
                        PluginSelectInvoke(plugin.Instance, selectedID, selectedTypeName, mPickResult);
                    }
                }
                catch (NotImplementedException ex)
                {
                    // typical if user is developing a plugin and hasnt debugged it, just ignore.
                    Debug.WriteLine("Workspace3D.OnEntitySelected() - ERROR:" + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Workspace3D.OnEntitySelected() - ERROR:" + ex.Message);
                }
                finally
                {
                    // never rely on the plugin to un-suspend changes.  One plugin writer may forget
                    // and then they will be able to break everyone elses plugin by never unsuspending
                    plugin.Instance.Host.PluginChangesSuspended = false;
                }
            }
        }
        
        /// <summary>
        /// Required if InvokeRequired when attempting to access plugin outside of main GUI thread.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="entityID"></param>
        /// <param name="entityTypeName"></param>
        private void PluginSelectInvoke(KeyPlugins.IPlugin plugin, string selectedID, string entityTypeName, Keystone.Collision.PickResults pick)
        {
            // TODO: This is the only workspace that calls the select plugin!
            //       This is revealing a flaw... that each workspace should be able to independantly
            //       access the plugin panel to put up it's own relevant plugins specific to the
            //       current workspace.
            //       HOWEVER: The editorworkspace should continue to update the existing plugin but
            //       this workspace if applicable, should find it's own plugins and display those
            //       on dockcontainer tabs that only this workspace is responsible for.
            mSelectedEntityID = selectedID;
            mSelectedEntityTypeName = entityTypeName;
            // TODO: this assert will fail if selecting from treeview and another entity is selected in simulation
            // NOTE: these asserts will also fail if the mSelectedEntityID is a referenced entity of pickResult.Entity as IEntitySystem
//            System.Diagnostics.Debug.Assert(mPickResult.Entity.ID == mSelectedEntityID);
//            System.Diagnostics.Debug.Assert(mPickResult.Entity.TypeName == mSelectedEntityTypeName);

            plugin.SelectTarget(mScene.Name, selectedID, entityTypeName);

            // TODO: let's say for example that the quick look bar is part of the plugin
            //       and can be selected from the plugin (maybe the plugin returns the quicklookbar
            //       to be used in fact...) then we can update that panel if it's used by that
            //       workspace, otherwise we can null it easily.
            //       However, the workspace area itself it seems... well... why is it too not
            //       defined by the plugin?  it seems as if all of the gui should be plugin-ish
            //       since the different workspaces are all app specific mostly.  
            //       For now, we wont put it in the plugin, but we will make the quicklookpanel
            //       a workspace default that doesnt have to be used.
            //       TODO: WAIT, what if we want to cache quick look panels so that we can go 
            //       backwards to last one... perhaps in a Stack<> where we push/pop quick look
            //       panels.  One way to do that would be to have a shared stack between all
            //       workspaces or something... ugh.  for now we wont worry about that either.
            //       Because maybe all we need for that is a list of last visited entities.


            QuickLookPanelSelect((PluginHost.EditorHost)plugin.Host, selectedID, entityTypeName, pick);
        }
        
        // TODO: why not just renmae this to "EntitySelect"
        // and we need to allow notifictaions to be sent to the workspace
        // so that updates to entities that are selected can be used to update
        // the GUI layed out by this workspace
        public override void QuickLookPanelSelect(KeyEdit.PluginHost.EditorHost scriptingHost, string entityID, string entityTypeName, Keystone.Collision.PickResults pick)
        {
            if (mQuickLookHostPanel == null) return;


            string[] childIDs;
            string[] childNodeTypes;
            scriptingHost.Node_GetChildrenInfo(entityID, null, out childIDs, out childNodeTypes);

            string domainObjID = "";

            if (childNodeTypes != null)
            {
                for (int i = 0; i < childNodeTypes.Length; i++)
                    if (childNodeTypes[i] == "DomainObject")
                    {
                        domainObjID = childIDs[i];
                        break;
                    }
            }
            
            try 
            {
            	//mQuickLookPanel.Select(mScene.Name, entityID, entityTypeName, pick);
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine("Workspace3D.QuickLookPanelSelect() - ERROR:" + ex.Message);
            }
        }
        
        protected virtual void ConfigurePicking(Keystone.Cameras.Viewport viewport)
        {
        }
        
        protected virtual void ConfigureHUD(Keystone.Cameras.Viewport viewport)
        {
        }
        
        protected virtual void OnViewportOpening(object sender, ViewportsDocument.ViewportEventArgs e)
        {
            if (mViewportControls[e.ViewportIndex] == null)
            {
                ViewportControl c = new ViewportEditorControl(CreateNewViewportName((int)e.ViewportIndex));
                mViewportControls[e.ViewportIndex] = c;
                if (e.HostControl != null)
	                e.HostControl.Controls.Add(mViewportControls[e.ViewportIndex]);
    

                ConfigureViewport(mViewportControls[e.ViewportIndex], new Huds.EditorHud());

                // wire cull being/completed events in case we want a chance to modify before and after
                // TODO: perhaps the HUD can also subscribe to these events with it's own methods?
                c.Viewport.Context.CullingStart += OnCullingStart;
                c.Viewport.Context.CullingComplete += OnCullingCompleted;
                c.Viewport.Context.RenderingStart += OnRenderingStart;
                c.Viewport.Context.RenderingComplete += OnRenderingCompleted;

                ConfigurePicking(c.Viewport);
                ConfigureHUD(c.Viewport);
                
                c.ReadSettings(null);
            }
        }

        protected void OnViewportClosing(object sender, ViewportsDocument.ViewportEventArgs e)
        {
            if (mViewportControls[e.ViewportIndex] != null)
            {
            	if (e.HostControl != null)
	                e.HostControl.Controls.Remove(mViewportControls[e.ViewportIndex]);
    
            	UnConfigureViewport(mViewportControls[e.ViewportIndex]);
                mViewportControls[e.ViewportIndex] = null;
            }
        }

        /// <summary>
        /// Called when unloading the entire workspace.
        /// </summary>
        public override void UnConfigure()
        {
            base.UnConfigure();

            SetDisplayMode(ViewportsDocument.DisplayMode.None);
        }

        /// <summary>
        /// Prior to calling this, the calling formmain should suspend layout/resize events
        /// </summary>
        /// <param name="mode"></param>
        public void SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode mode)
        {
            KeyEdit.Controls.ViewportsDocument vpdocument = (KeyEdit.Controls.ViewportsDocument)mDocument;

            vpdocument.ConfigureLayout(mode);

            // show is required here to activate this tab properly and Enable any newly added viewports
            if (mDocument.InvokeRequired)
                mDocument.Invoke((System.Windows.Forms.MethodInvoker)delegate { Show(); });
            else
                Show();
        }



        protected virtual void ConfigureViewport(KeyEdit.Controls.ViewportControl vpControl, Keystone.Hud.Hud hud)
        {
            // the TVViewport is created only after the viewport Forms are fully loaded on the GUI.
            // note: this method is called everytime a new scene is loaded even with viewports already
            // loaded from a previous scene.  This is because a RenderingContext which is created here
            // is always tied to a specific scene and cannot be untied.  Thus the RendreingContext
            // must be destroyed and recreated if a new scene is loaded.

            Keystone.Cameras.Viewport vp  = Keystone.Cameras.Viewport.Create(vpControl, vpControl.Name, vpControl.RenderHandle);
            

            Keystone.Cameras.RenderingContext context = 
                new Keystone.Cameras.RenderingContext(this, mScene, vp, 
                
                    AppMain.NEARPLANE, AppMain.FARPLANE, 
                    AppMain.NEARPLANE_LARGE , AppMain.FARPLANE_LARGE,
                    AppMain.MAX_VISIBLE_DISTANCE,
                    AppMain.FOV * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS);

            context.Hud = hud; // can be null


            // viewport settings can be restored after InitScene() 
            // NOTE: i really prefer dealing with ini file here and not propertybags.  Propertybag is good for the propertygrid filling
            // without needing a special class for each type... but that's it.  Everything has it's place.
            // TODO: the following is "ok" except i think restoring of ini values should be done in a seperate class perhaps...
            // And likewise, something like CommandProcessor should have menu commands and such sent to it for centralized processing
            // so that any changes are easily updated to the ini and so you dont have ini update code everywhere 
            //vpControl.ReadSettings(_iniFile);
            context.ViewType = vpControl.View;
            context.ProjectionType = vpControl.Projection;
            context.TraverseInteriorsAlways = false;
            context.TraverseInteriorsThroughPortals = true;
            
            vpControl.Context = context;
          

            // set RenderingContext options appropriate for whether to use config file graphics
            // settings or to override and force those specific for editor viewports
            vpControl.Dock = System.Windows.Forms.DockStyle.Fill;
            vpControl.ShowToolbar = true;
        }

        public void UnConfigureViewport(KeyEdit.Controls.ViewportControl vpControl)
        {
            if (vpControl != null)
            {
                //vpControl.WriteSettings(_iniFile);
                if (vpControl.Context != null)
                {
                    if (vpControl.Context != null)
                    {
                        vpControl.Context.Dispose ();
                        vpControl.Context = null;
                    }
                }
            }
        }

        protected string CreateNewViewportName(int index)
        {
            // TODO: when we no longer depend on viewport0 name used by some helper placement functions
            // that don't use EntityPlacer, we can delete this hack to make sure first viewport is always
            // named viewport0
            if (AppMain._core.Viewports.ContainsKey("viewport0") == false)
                return "viewport0";

            return mName + "_viewport" + index.ToString();
        }


        /// <summary>
        /// RenderingContext specific rendering state overrides can be applied here
        /// to part of the scene or the entire scene.  for instance, we can temporarily make
        /// the exterior of a vehicle transparent for just one particular RenderingContext.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void OnCullingStart(Keystone.Cameras.RenderingContext context)
        {

        }
        protected virtual void OnCullingCompleted(Keystone.Cameras.RenderingContext context)
        {

        }

        protected virtual void OnRenderingStart(Keystone.Cameras.RenderingContext context)
        {

        }

        protected virtual void OnRenderingCompleted(Keystone.Cameras.RenderingContext context)
        {
            
        }


    }
}
