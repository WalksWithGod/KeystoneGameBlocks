using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lidgren.Network;
using System.Diagnostics;
using DevComponents.DotNetBar;
using System.IO;
using Keystone.Workspaces;
using Keystone.Scene;
using KeyCommon.DatabaseEntities;

namespace KeyEdit
{
    public partial class FormClient : FormMainBase
    {

        private const string EDITOR_LAYOUT_FILENAME = "layouts\\editor.layout";
        private const string EDITOR_DEFINITION_FILENAME = "layouts\\editor.definition";

        private Dictionary<int, GameSummary> mGames; 


        public FormClient() : base() 
        { 
            InitializeComponent();
            ribbonControl = ribbonControl1; 
        }

        public FormClient(Keystone.CoreClient core) : base (core)
        {
            InitializeComponent();
            ribbonControl = ribbonControl1 ; 

            // on startup only the network tab is visble
            // TODO: on startup, now maybe popup control panel (aka options)
            // and allow user to configure network password (cdkey) and such there
            // network ribbon tab may be obsolete... in fact im sure it is. 
            ribbonTabNetwork.Visible = true;
            ribbonTabCnC.Visible = false;
            ribbonTabOperations.Visible = false;
            ribbonTabSecurity.Visible = false;
            ribbonTabComms.Visible = false;
            ribbonTabDatabase.Visible = false;
            ribbonTabManifest.Visible = false;
            ribbonTabMedical.Visible = false;
            ribbonTabPersonnel.Visible = false;

            ribbonTabNetwork.Select();
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string layoutFile = Path.Combine(AppMain.ConfigFolderPath, EDITOR_LAYOUT_FILENAME);
            string definitionFile = Path.Combine(AppMain.ConfigFolderPath, EDITOR_DEFINITION_FILENAME);

            mWorkspaceManager = new KeyEdit.Workspaces.WorkspaceManager(this, dotNetBarManager1, _core.Settings, definitionFile, layoutFile, OnDocumentTabClosing);
            //mViewManager.AssetSelected = AssetSelected;
            
            // we'll use the form's handle to init our graphics, but we'll only render to secondary viewports.  
            // This means that our ViewManager doesnt even have to retain the array of viewports and do some juggling act.
            // Instead, we just need to remember that cameras have viewports assigned to them, and each Scene instance
            // has Cameras added to them.  So only the active scene being rendered uses only the cameras added to it.
            // So our Intro screen can use an entirely seperate Scene with it's own camera and viewport assigned to it.
            IntPtr handle = this.Handle;
            Keystone.Devices.GraphicsDevice graphics = new Keystone.Devices.GraphicsDevice(handle);
            graphics.SwitchWindow();
            AppMain._core.Initialize(graphics);

            Keystone.Scene.ClientSceneManager sm = new Keystone.Scene.ClientSceneManager(_core);


            AppMain.mNetClient = new KeyEdit.Network.InternetClient(_core.Settings);
            AppMain.mNetClient.UserMessageReceivedHandler = UserMessageReceived;
            AppMain.mNetClient.UserMessageSendingHandler = UserMessageSending;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
             base.OnClosing(e);

        }

        private bool OnDocumentTabClosing (System.Windows.Forms.Control control)
        {
            //if (control is
            throw new NotImplementedException();
            return false;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            AppMain.mNetClient.ConnectToLobby();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            // refreshing should be automatic, lets make this more automagic like battlenet
        }

        private void buttonJoinHost_Click(object sender, EventArgs e)
        {
            ListViewItem item = null;
            if (lobbyControl1.SelectedItems != null && lobbyControl1.SelectedItems.Count > 0)
                item = lobbyControl1.SelectedItems[0];

            if (item == null) return;

            AppMain.mNetClient.ConnectToGame(item.Text, item.SubItems[0].Text, 2022);
        }


        private void OnConnectedToGameServer()
        {
            ribbonBar1.SuspendLayout();
            ribbonTabNetwork.Visible = false;
            ribbonTabCnC.Visible = true;
            ribbonTabOperations.Visible = true;
            ribbonTabSecurity.Visible = true;
            ribbonTabComms.Visible = true;
            ribbonTabDatabase.Visible = true;
            ribbonTabManifest.Visible = true;
            ribbonTabMedical.Visible = true;
            ribbonTabPersonnel.Visible = true;

            ribbonTabNetwork.Select();

            ribbonBar1.ResumeLayout();

            // TODO: hide the server's tab and show the 3d tab for the tactical C&C view

            // TODO: so our "view" switching between our various tabs needs to be bulletproofed and simplified
            // The key is that the viewmanager is used to restore window layouts...  so it exists clearly only on the client exe
            // not in keystone or such 
            // And we have to really do a proper job of dealing with cases where the user repositions a window... and restoring those
            // states.
            //mViewManager.ChangeView("c&c", false);
        }

        private void buttonHostDetails_Click(object sender, EventArgs e)
        {
            float diameter = AppMain.SIMPLE_SCENE_DIAMETER; 
            // i kind of prefer this to be automatic when selecting a server in the list
            KeyCommon.Messages.Scene_New message = new KeyCommon.Messages.Scene_New("test", diameter, diameter, diameter);

            SendNetMessage(message);
        }

      


        // if we receive joingameapproval, we'll have a ticket and host details we use to connect to that game server
        // then after connection, the server needs to send us ship info we can use to cache our visuals
        // so we can spawn entities and start playing.

        private void OnConnectionToGameServerSuccess()
        {
            // remove the server browser and start the 3d and load the map

            // after we've loaded the map and the scene is initialized, we need to notify the 
            // server we're ready to receive state?  this is part of what we need to figure out in our game server handshaking
            // protocol
            // 

            // In theory, what we want is for network commands to all just go thru the regular Command Processor...
            // via    SomeCommand.BeginExecute(handler);
            // But here, some of these commands are doing client side only graphics related things which our server shouldnt do
            // and shouldnt need to reference, so how do we do this?

            // perhaps in the short term what we'll do is convert netcommands to the asychronous commands 
            // and just execute them.   This way a client can call client versions of those commands and a server can call server
            // versions...
            //Keystone.Commands.GenerateScene generateScene = new Keystone.Commands.GenerateScene();
        }

        private void UserMessageSending(KeyCommon.Messages.MessageBase message)
        {
 
        }

        // essentially a giant factory to create the correct message
        private void UserMessageReceived(int commandID, NetChannel channel, NetBuffer buffer)
        {
            KeyCommon.Messages.MessageBase msg = null;
            Amib.Threading.WorkItemCallback cb = null;

            // essentially a giant factory
            switch ((KeyCommon.Messages.Enumerations)commandID)
            {
                   
                case KeyCommon.Messages.Enumerations.GameSummaryList:
                    msg = new KeyCommon.Messages.GameSummaryList();
                    cb = Worker_GameSummaryList;
                    break;
                
                case KeyCommon.Messages.Enumerations.NewScene:
                    msg = new KeyCommon.Messages.Scene_New();
                    break;
                //case KeyCommon.Messages.Enumerations.LoadScene :
                //    msg = new KeyCommon.Messages.Scene_Load();
                //    cb = Worker_LoadScene;
                //    break;
                case KeyCommon.Messages.Enumerations.Simulation_Spawn:
                    msg = new KeyCommon.Messages.Simulation_Spawn();
                    cb = Worker_SpawnEntity;
                    break;
                case KeyCommon.Messages.Enumerations.CommandSuccess:
                    // TODO: temporary for telling us that our join game succeeded.
                    msg = new KeyCommon.Messages.CommandSuccess();
                    break; 
                default:
                    break;
            }

            if (msg == null) return;

            msg.Read(buffer);
            
            Keystone.Commands.Command cmd = new Keystone.Commands.Command(msg);
            // TODO: cmd should also wire up the Undo callback.
            cmd.BeginExecute(cb, CommandCompleted, null);
        }


        private object Worker_LoadScene(object state)
        {
            throw new NotImplementedException();

            //Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            //KeyCommon.Messages.Scene_Load  loadScene = (KeyCommon.Messages.Scene_Load)cmd.Message;

            ////Repository.ResourceAddedCallback = SceneNodeAddedHandler;
            ////Repository.ResourceRemovedCallback  = SceneNodeRemovedHandler;
            //// TODO: in our regular FormMain, we only call LoadScene() from the
            //// commandcompleted which runs on main update thread and not here!
            //// this should be fixed to work the same way.
            //IWorkspace workSpace = new Workspaces.EditorWorkspace(PRIMARY_WORKSPACE_NAME);
            //Keystone.Simulation.ISimulation sim = KeyEdit.FormMain.CreateSimulation(loadScene.FolderName);
            //Scene scene = (Scene)_core.SceneManager.Load( loadScene.FolderName, sim, OnEmptyRegionPageComplete, OnRegionChildenPageComplete);
            
            //mWorkspaceManager.Add(workSpace, scene);
            ////mWorkspaceManager.ChangeWorkspace(workSpace.Name, false);

            //// must set the new state after LoadScene()
            ////_core.Scene_Type = SceneType.;
            //_core.Scene_FileName = loadScene.FolderName;

            //// TODO: the following view creation should occur next but havent finished implementing
            //// just copy and pasted the old code that was in the constructor
            //// but now these views are only created after the scene is loaded since
            //// views are tied to the scene they "view"

            ////string layoutFile = System.IO.Path.Combine(_core.ConfigPath, CNC_LAYOUT_FILENAME);
            ////string definitionFile = System.IO.Path.Combine(_core.ConfigPath, CNC_DEFINITION_FILENAME);
            //////Views.CNCView newView = new Views.CNCView("c&c", definitionFile, layoutFile, dotNetBarManager1, mViewManager);
            ////Views.EditorView newView = new KeyEdit.Views.EditorView("c&c", definitionFile, layoutFile, dotNetBarManager1, mViewManager);
            ////mViewManager.Add(newView);

            ////// TODO: add views for security, personnel, manifests, etc

            //////SwitchController(typeof(EditController).Name);
            //mWorkspaceManager.ChangeWorkspace("c&c", false);
            return state;
        }

        private void OnEmptyRegionPageComplete(Keystone.IO.ReadContext context)
        {
        }

        private void OnRegionChildenPageComplete(Keystone.IO.ReadContext context)
        {
        }


        private object Worker_SpawnEntity(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Simulation_Spawn spawn = (KeyCommon.Messages.Simulation_Spawn)cmd.Message;

            Keystone.Entities.Entity entity = null; // TODO LoadEntity(); // TODO:
            Keystone.Entities.Entity parent ; //= _core.SceneManager.ActiveSimulation.Scene.Root;
            parent = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(spawn.ParentID);

            // Below is obsolete - we should simply be calling LoadEntity()
            //KeyCommon.IO.ResourceDescriptor desc = spawn.Resource;

            //// the following stream retreived is expected to a be a zip within a zip (as in a .kgbentity) file.
            //System.IO.Stream stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(desc.EntryName, "", Keystone.Core.FullModPath(desc.ModName ));
            //Keystone.IO.XMLDatabaseCompressed xmldatabase = new Keystone.IO.XMLDatabaseCompressed();

            //xmldatabase.Open(stream);
            //// TODO: who closes the stream?

            //// TODO: check for nulls and invalid files, wrong versions and such
            //Keystone.IO.SceneReader reader = new Keystone.IO.SceneReader(xmldatabase, _core.ThreadPool);
            //// it's an archived prefab so we know the first node is our info node.
            //Keystone.Scene.SceneInfo info = (Keystone.Scene.SceneInfo)reader.ReadSynchronous("SceneInfo", "").Node;
            //// nodes including this temporary SceneInfo are added to the Repository when they are constructed but
            //// ref count is still at 0 until it's added to the scene.  If we manually increment ref count to 1, then decrement it, it will be automatically
            //// removed from the Repository. 
            //Keystone.Resource.Repository.IncrementRef(info);
            //Keystone.Resource.Repository.DecrementRef(info); // ensure the temporary SceneInfo that was created gets removed from repository    

            //string[] clonedEntityIDs = null; // TODO: these need to be assigned by server in the Command
            //Keystone.IO.ReadContext context = reader.ReadSynchronous(info.FirstNodeTypeName, "", "", true, true, clonedEntityIDs, true);
            //stream.Dispose();
            //xmldatabase.Dispose();

            //// also switch controllers to follow this entity
            //Keystone.Entities.Entity entity = (Keystone.Entities.Entity)context.Node;
            //// TODO: the parentID is specified in the Spawn command isn't it?  It should be required!
            //// and maybe if it's null, it means Root by default, but i dont think we should ever allow it to be null.
            //// unless we dynamically want to compute the parent based on a position



            ////     entity = new StaticEntity ("vehicle", import.Model); //TODO: temp, we use "vehicle" so we can discriminate in StaticEntity.Update() so only the out vehicle entity updates and not interior as well (the interior already inherits the parents rotation and doesnt need to rotate additionally)
            //// TODO: we dont necessary add to the camera, perhaps we specified a parent via RMK or based on
            //// the coordinate, we must find the container (possibly ship interior) where this belongs
            //entity.Translation = spawn.Position;
            // TODO: we should use super setter for this... or call a function that will in turn call that for us
            parent.AddChild(entity);


            // OBSOLETE - Below is not how we do things.  Instead a Workspace such as a 3rdPersonArcade
            //            workspace should be created and it wll instance the proper IOController for itself
            //            and through it user can control their vehicle
            //Keystone.Controllers.InputController controller = null;
            //_core.RemoveAllIOControllers();
            //_core.Viewports["viewport0"].Context.ChangeToChase();
            //// note that a context's View Controller is not the same as the main client application Client Controller
           
            //// a type of third person controller for controlling client applications where a Vehicle is the main avatar
            //controller = new Keystone.Controllers.VehicleController(entity);

            //_core.AddIOController(controller);
            //_core.CurrentIOController = controller;

            return state;
        }

        private object Worker_GameSummaryList(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.DatabaseEntities.GameSummary[] list = ((KeyCommon.Messages.GameSummaryList)cmd.Message).List;

            if (list == null) return null;

            for (int i = 0; i < list.Length; i++)
            {
                string name = list[i].ServerName;
                if (string.IsNullOrEmpty(name)) continue;

                if (lobbyControl1.Items.ContainsKey(name)) continue; 
 
                // add the list items to the listview
                string[] row = {list[i].ListenTable,"", "0/16",  list[i].Map, "Saga", "Furball"};
                
                ListViewItem item = new ListViewItem(name);
                item.SubItems.AddRange(row);
                lobbyControl1.Items.Add (item);
            }
            // how do we remove items from this list?
            //  - client must refresh their search?
            return state;
        }


        /// <summary>
        /// This function is called at the bottom of our game loop while in the network client and we can do all
        /// gui updates at once such as 
        /// </summary>
        internal override void ProcessCompletedCommandQueue()
        {
            Keystone.Commands.Command command = null;
            if (mCompletedCommands == null) return;

            Amib.Threading.IWorkItemResult[] completedResults;
            lock (mCompletedCommands)
            {
                completedResults = mCompletedCommands.ToArray();
                mCompletedCommands.Clear();
            }

            // TODO: when we get .net 4.0\vs.net 2010 we can try to Parallel.ForEach () 
            // http://blogs.lessthandot.com/index.php/Architect/EnterpriseArchitecture/visual-studio-2010-concurrency-profiling
            foreach (Amib.Threading.IWorkItemResult result in completedResults)
            {
                if (result.Exception == null)
                {
                    command = (Keystone.Commands.Command)result.State;
                    KeyCommon.Messages.MessageBase message = command.Message;

                    if (command.State == Keystone.Commands.State.ExecuteError)
                    {
                        Debug.WriteLine("FormClient.Commands.ProcessCompletedCommandQueue() - Error executing command '" + command.GetType().Name);
                        // nothing to handle here, nothing to push on stack
                        // TODO: if a command errors and sets this state, it should've already unrolled anything it did
                        command.EndExecute(); // we still call endexecute so any cleanup code (such as hiding of a progress dialog) can occur
                        return;
                    }

                    command.EndExecute();

                    // recheck for ExecuteError after .EndExecute()
                    if (command.State == Keystone.Commands.State.ExecuteError)
                        return;

                    command.State = Keystone.Commands.State.Ready;

                  
                    // TODO: all of the below belond in the EndExecute of each respective command
                    //Keystone.Commands.ICommand command = (Keystone.Commands.ICommand) result.State;
                    Trace.WriteLine("FormClient - Command completed successfully.", result.State.GetType().Name);
                    
                    //EntityBase entity = null;

                    // OBSOLETE - lights now created with Node_Create_Request and receiving Node_Create that will
                    //            contain a server created "id" for that light
                    //if (message is KeyCommon.Messages.Scene_LoadLight)
                    //{
                    //    //entity = ((KeyCommon.Messages.AddLight)message).Entity;
                    //    //// Keystone.Traversers.SuperSetter super = new Keystone.Traversers.SuperSetter(((AddLight )result.State).Parent));
                    //    //// super.Apply(entity);
                    //    //AppMain._core.PrimaryContext.Region.AddChild(entity);
                    //    //_core.SceneManager.ActiveScene.XMLDB.Write(entity.Parent, true, OnNodeWriteComplete);
                    //}
                    //else if (message is ImportBillboard)
                    //{
                    //    entity = ((ImportBillboard)message).Entity;
                    //}

                }
                else
                    Trace.WriteLine("Command failed.", command.Message.GetType().Name);
            }  // end foreach

            // AFter we've iterated through all outstanding completed commands now perhaps is best time 
            // to save the actual XMLDocuments to disk/zip whereas before we simply edited the Documents in memory.  

        }

        private void ribbonTabCnC_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabNetwork_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabSecurity_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabDatabase_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabPersonnel_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabManifest_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabMedical_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabComms_Click(object sender, EventArgs e)
        {

        }

        private void ribbonTabOperations_Click(object sender, EventArgs e)
        {

        }

    }
}
