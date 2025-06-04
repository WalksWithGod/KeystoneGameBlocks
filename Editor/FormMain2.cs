using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Keystone.Cameras;
using Keystone.Commands;
using Keystone.Controllers;
using Keystone.Devices;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Scene;
using Keystone.Types;
using Lidgren.Network;
using MTV3D65;
using System.Collections.Generic;
using DevComponents.DotNetBar;

namespace KeyEdit
{
    public partial class FormMain2 : DevComponents.DotNetBar.Office2007RibbonForm
    {
        private enum EditMode
        {
            FloorPlanDesign,
            Universe,
            ComponentDesign,
            SimpleComposition  // in composition, you can only drag and drop and rotate/scale/position objects.  No real-time editing.
        }

        private struct SceneState
        {
            public bool Changed;
            public string FileName;
            public bool IsUntitled;
            // while in floor plan edit mode, you cannot change the primary perspective from top down x/z orthographic  projection
            // you actually can leave floor plan edito mode, but you will then no longer be able to modify walls, floors or ceilings. (except for placing components like doors and windows)
            public EditMode Type;
        }

        private const string OPEN_ZIP_FILTER = "zip files (*.zip)|*.zip|All files (*.*)|*.*";
        private const string ACTOR_GEOMETRY_FILE_FILTER = "x files (*.x)|*.x| tva files (*.tva)|*.tva| All files (*.*)|*.*";
        private const string STATIC_GEOMETRY_FILE_FILTER = "x files (*.x)|*.x| tvm files (*.tvm)|*.tvm|obj files (*.obj)|*.obj|all supported files (*.x; *.tvm;*.obj)|*.x; *.tvm;*.obj|All files (*.*)|*.*";
        private const string EDITABLE_MESH_FILE_FILTER = "obj files (*.obj)|*.obj";
        private const int DEFAULT_FILTER_INDEX = 4;
        private const string TEXTURE_FILE_FILTER = "bmp files (*.bmp)|*.bmp|png files (*.png)|*.png|dds files (*.dds)|*.dds|jpg files (*.jpg)|*.jpg |gif files (*.gif)|*.gif |tga files (*.tga)|*.tga|All supported files (*.bmp; *.png;*.dds; *.jpg; *.gif; *.tga)|*.bmp; *.png;*.dds;*.jpg;*.gif;*.tga|All files (*.*)|*.*";
        private const int TEXTURE_FILTER_INDEX = 7;

        private const string EDITOR_LAYOUT_FILENAME = "layouts\\editor.layout";
        private const string EDITOR_DEFINITION_FILENAME = "layouts\\editor.definition";
        private const string LOBBY_LAYOUT_FILENAME = "layouts\\lobby.layout";
        private const string LOBBY_DEFINITION_FILENAME = "layouts\\lobby.definition";
        private const string LOGIN_LAYOUT_FILENAME = "layouts\\login.layout";
        private const string LOGIN_DEFINITION_FILENAME = "layouts\\login.definition";
        private bool m_bSaveLayout = true;

        private Keystone.Types.Color _backColor = new Keystone.Types.Color(0.0f, 0.3f, 0.9f, 1.0f);
                    //_core.SetBackGroundColor(1.0f, 1.0f, 1.0f, 1.0f); // white
            //_core.SetBackgroundColor(0.75f, 0.75f, 0.75f, 1.0f); // light gray
            // _core.SetBackgroundColor(0.8f, 0.8f, 0.8f, 1.0f); // lighter gray
            //_core.SetBackGroundColor(0.3f, 0.3f, 0.3f, 1.0f); // dark gray
            //_core.SetBackGroundColor(0.0f, 0.0f, 0.0f, 1.0f); // black
            //_core.SetBackGroundColor(0.0f, 0.3f, 0.9f, 1.0f); // blue

        private SceneState _state;
        public KeyEdit.Network.Client _client;

        private Settings.Initialization _iniFile;
        

        private Keystone.CoreClient _core;
        private Views.ViewManager mViewManager;
        private Views.IView mLobbyView;  // todo: i might not need to maintain reference to any Views here, just switch them via mViewManager.ChangeView(key);
        private Views.IView mEditorView;
        private Controls.LibNoiseTextureDesigner mLibNoiseTextureDesigner;

        public FormMain2(  Keystone.CoreClient core, Settings.Initialization ini)
        {
            if (core == null || ini == null) throw new ArgumentNullException();

            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            _iniFile = ini;
            _core = core;

            LoadPlugins();
        }
                           
        // MainForm_Load is called on Application.Run(_window) and AFTER the Form's constructor.
        protected override void OnLoad(EventArgs e)
        {
            //if (m_output != null && m_output.Visible )
            //    m_outputConsole.Init(m_output, true, false);
            AppMain.LobbyNetManager = new KeyEdit.Network.LobbyNetManager(_iniFile);


            mViewManager = new KeyEdit.Views.ViewManager(this, dotNetBarManager, _iniFile);

            // we'll use the form's handle to init our graphics, but we'll only render to secondary viewports.  
            // This means that our ViewManager doesnt even have to retain the array of viewports and do some juggling act.
            // Instead, we just need to remember that cameras have viewports assigned to them, and each Scene instance
            // has Cameras added to them.  So only the active scene being rendered uses only the cameras added to it.
            // So our Intro screen can use an entirely seperate Scene with it's own camera and viewport assigned to it.
            IntPtr handle = this.Handle;
            GraphicsDevice graphics = new GraphicsDevice(handle);
            graphics.SwitchWindow();
            AppMain._core.Initialize(graphics);


            string layoutFile = Path.Combine(_core.ConfigPath, LOBBY_LAYOUT_FILENAME);
            string definitionFile = Path.Combine(_core.ConfigPath, LOBBY_DEFINITION_FILENAME);
            //mLobbyView = new Views.LobbyView("lobby", definitionFile, layoutFile, AppMain.LobbyNetManager, dotNetBarManager, mViewManager);
            //mLobbyView.HideDocumentTabs = false;
            //mViewManager.Add(mLobbyView);
            
            //layoutFile = Path.Combine(_core.ConfigPath, LOGIN_LAYOUT_FILENAME);
            //definitionFile = Path.Combine(_core.ConfigPath, LOGIN_DEFINITION_FILENAME);
            //Views.LoginView loginView = new Views.LoginView("login", definitionFile, layoutFile, dotNetBarManager, mViewManager);
            //loginView.HideDocumentTabs = true;
            //mViewManager.Add(loginView);

            layoutFile = Path.Combine(_core.ConfigPath, EDITOR_LAYOUT_FILENAME);
            definitionFile = Path.Combine(_core.ConfigPath, EDITOR_DEFINITION_FILENAME);
            mEditorView = new Views.EditorView("editor", definitionFile, layoutFile, dotNetBarManager, mViewManager);
            mEditorView.HideDocumentTabs = true;
            mViewManager.Add(mEditorView);

            // the texture gen can only be switched to while in the Edit mode and so is a subset of Edit where it adds
            // an extra tab to the main Edit dockView.  So really, i dont know if it needs a layoutFile or definitionFile
            // still working on this as far as how we handle all the different views.  Maybe we have to always use some
            // layoutfile and as far as making sure we can only switch to it from editview, we can check the mViewManager.ActiveView and
            // verify it's "editor".  If i were to have a concept of "sub-views" than i should have the EditView itself
            // internally handle the change to a specific sub-view within it's own "Show()"
            // Actually now that I think about it, i'm thinking more and more that the difference between a sub-view is that
            // it shouldnt use a view at all, it's really just the adding of a document view.  Other views would be
            // BehaviorTreeView, ScriptingView, ShipDesignView
            // So i think maybe there are two rules of thumbs about views
            //  1) Views change the ribbonbar
            //  2)  Views change the names and numbers of the docking bars
            //  Otherwise so called "sub-views" dont change any of that, they just are more contextual as to some tabs being added
            //  to the main document view and to the dock panels
            //  Although, the primary advantage to "views" is that i can prevent users from ever hiding and customizing to the poitn of
            //  them accidentally losing some parts of the display that are needed to fully use a particular view.  Ideally i dont want
            // them to have too much customizeability that they can hide/lose stuff...and drag and drop them to places they can't
            // restore.  I guess that's the main fear.  Hiding isnt so bad if they can hit View\Show Properties or Show Explorer, etc
            // but i dont want them being able to get rid of a bar completely.
            //  I can still have a concept of a "SubView" class and that way switching back to one view, can restore the proper 
            // subView as well.... in theory, the layout including these other tabs added, will be saved to the definition and layout
            // inbetween switching views so it'll be automatic.
            //layoutFile = Path.Combine(_core.ConfigPath, TEXTUREGEN_LAYOUT_FILENAME);
            //definitionFile = Path.Combine(_core.ConfigPath, TEXTUREGEN_DEFINITION_FILENAME);
            //Views.ProceduralTextureGenView mTextureGenView = new Views.ProceduralTextureGenView("texturegen", definitionFile, layoutFile, dotNetBarManager, mViewManager);
            //mTextureGenView.HideDocumentTabs = false;
            //mViewManager.Add(mTextureGenView);

            SwitchController(typeof(EditController).Name);
            mViewManager.ChangeView("editor", false);
            //mViewManager.ChangeView("login", false);
                        
            // scene manager to hold each of our scenes
            _core.SceneManager = new ClientSceneManager("untitled");

            // intro scene starts with our scripted intro scene
            // note that every scene has it's own cameras (and thus viewport controls) associated with them 
          //  ClientScene introScene = AppMain.LoadScene (_core.DataPath + "\\mods\\intro\\default.kgbscene");
            
            // init the intro scene to use a single viewport, with no controller since its a scripted scene


            // editor starts with a blank untitled scene
            buttonItemNewSingleRegion_Click(null, null);
            
            // loopback server is used whenever we do NOT connect to an external server.  Loopback server is default
            // todo: a call to ConnectToServer should ideally start loopback or swap to any other if a current server is already set
            AppMain.mLoopbackServer = new Keystone.Network.LoopbackServer();
            // create the client and connect it to the loopback server
            AppMain._networkClient = new KeyEdit.Network.Client2(_iniFile , null);
            AppMain._networkClient.ConnectToLoopback("127.0.0.1", 2022);
           
            // rendering will start as soon as a scene is loaded
            // Hook the application's idle event and launch the main form.
            Application.Idle += AppMain.OnApplicationIdle;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _core.SceneManager.Scenes[0].Simulation.IsRunning = false;
            base.OnClosing(e);
            if (m_bSaveLayout)
            {
                mViewManager.SaveCurrentLayout();
                Settings.Initialization.Save(Path.Combine(_core.ConfigPath, AppMain.CONFIG_FILENAME), _iniFile);
            }
            Hide();
            AppMain.Plugins.ClosePlugins();
            _core.Shutdown(); // shut down and unload scene before the form which our engine is initialized too is removed
        }

        private void InitScene(ClientScene scene) //, ViewportControl[] vpControls )
        {
            ViewportControl[] vpControls = mViewManager.ViewportControls;
            if (vpControls == null || vpControls.Length == 0)throw new ArgumentNullException ();


 
            // Cameras are created when a scene is loaded.
            // Cameras are destroyed when a scene is destroyed/unloaded.
            // Cameras therefore must always be re-assigned to viewports everytime a new scene is loaded.
            for (int i = 0; i < vpControls.Length; i++)
            {

                if (vpControls[i] == null) continue;

                mViewManager.ConfigureViewport(scene, vpControls[i]); 
                // todo: might be able to delete below if 1 edit controller for all viewports works out ok. 
                // The reason is because now instead of having a camera assigned to a Controller as before
                // we now have the Controller in it's Update() grabbing the selectedViewport.Camera and using that

                //See notes 
                // above the Update() function in the EditController.cs
                //EditController controller = new EditController("editcontroller", m_formViewports[i].Viewport.Camera);
                //controller.RegisterGizmoController(_core.SceneManager.ManipulatorController);
                //_core.AddIOController( controller);  
            }
            
            Pager.ResourceAddedCallback = ResourceAddedToRepository;
        }

        // todo: this needs to be more specific to a viewport scheme and not just generic for any scenario.  
        // I think for our VehicleController, we should just assume a single viewport and so we just need to replace that EditorCamera
        // with a ThirdPersonCamera
        private void SwitchController(string typeName)
        {
            ClientController controller;
            _core.RemoveAllIOControllers();

            // todo: you know, i was thinking of using a single controller for all viewports but consider the case where i want
            // a 3rdp erson controller for primary viewport and then a nav viewport that is panable and zoomable like the EditController
            // and consider another viewport where it's 3rd person of another ship and with different rotation and zoom settings...
            // here clearly IOControllers are effectively per Viewport
            switch (typeName)
            {
                case "EditController":
                    controller = new EditController(EntityMouseOver , EntitySelected);
            
                    // set the default tool
                    ((EditController)controller).CurrentEditTool = new Keystone.EditTools.SelectionTool();
                    break;
                case "VehicleController":
                    EntityBase veh = null;

                    // find the target by name or search for IControllable 
                    veh = _core.SceneManager.ActiveScene.FindEntityByType(typeof(Keystone.Vehicles.PlayerVehicle).Name);

                    controller = new EditController(EntityMouseOver, EntitySelected);
                    controller.Target = veh;
                    //controller = new VehicleController(veh);

                    ThirdPersonCamera cam = (ThirdPersonCamera)((ClientScene)_core.SceneManager.ActiveScene).CreateThirdPersonCamera
                        (AppMain.NEARPLANE , AppMain.FARPLANE , AppMain.FOV , false, true, false );
                    ((ClientScene)_core.SceneManager.ActiveScene).AddCamera(cam);

                    cam.Target = veh;
                    // remove the existing cam from our viewport and replace with the new thirdPersonCam and configure it
                    //mViewManager.UnConfigureViewport(mViewManager.ViewportControls[0]);
                   // mViewManager.ConfigureViewport((ClientScene)_core.SceneManager.ActiveScene, mViewManager.ViewportControls[0]);

                    ((ClientScene)_core.SceneManager.ActiveScene).RemoveCamera(mViewManager.ViewportControls[0].Viewport.Camera);
                    mViewManager.ViewportControls[0].Viewport.Camera = cam;
                    ChaseCameraController chaseController = new ChaseCameraController(cam);
                    break;

                case "ThirdPersonController":
                  
                    // this view we (for now) wont switch to a new View or add a new control
                    // we'll simply find a IControllable entity and set that as the target for a 3rd person
                    // camera.

                    EntityBase target = null;

                    // find the target by name or search for IControllable 
                    string id = "";
                    target = _core.SceneManager.ActiveScene.FindEntityByType(typeof(Keystone.Entities.PlayerCharacter).Name);
                    // _core.SceneManager.ActiveScene.GetEntity(id);
                    // todo: need to replace the cameras with ThirdPersonCamera
                    controller = new CharacterController(target);
                    break;

                default:
                    throw new NotImplementedException ();
            }

            _core.AddIOController(controller);
            _core.CurrentIOController = controller;
        }

        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new FormAbout();
            formAbout.ShowDialog(this);
        }


        private void menuItemExit_Click(object sender, EventArgs e)
        {
            if (QuerySceneUnload())
                Close();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            menuItemExit_Click(sender, e);
        }

        private void buttonItemSave_Click(object sender, EventArgs e)
        {
            QuerySave();
        }

        private void buttonItemSaveAs_Click(object sender, EventArgs e)
        {
            string filename = SaveAs();
            if (!string.IsNullOrEmpty(filename))
                Save(filename);
        }

        
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.InitialDirectory = AppMain.DATA_PATH;
            openFile.Filter = OPEN_ZIP_FILTER;
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string fullpath = openFile.FileName;

                LoadScene(fullpath);
            }
        }

        private void buttonItemNewFloorPlan_Click(object sender, EventArgs e)
        {
            NewScene();
            _state.Type = EditMode.FloorPlanDesign;
        }
        
        private void buttonItemNewSingleRegion_Click(object sender, EventArgs e)
        {
            NewScene();
            _state.Type = EditMode.SimpleComposition;
        }

        // used for celestial galaxy.  Perhaps use the Control Panel option dialog instead with a different set of options
        private void buttonItemNewMultiRegionScene_Click(object sender, EventArgs e)
        {
            FormNewGalaxy newGalaxy = new FormNewGalaxy();
            newGalaxy.ShowDialog(this);
         
            if (newGalaxy.DialogResult == DialogResult.OK )
            {
                if (QuerySceneUnload())
                {
                    uint across = AppMain.REGIONS_ACROSS;
                    float diameter = AppMain.REGION_DIAMETER;
                   // _state.FileName = _core.SceneManager.BeginCreateUniverse(across, across, across, diameter, diameter, diameter, OnUniverseCreated);

                    GenerateUniverse command = new GenerateUniverse(0, "test", across, across, across, diameter, diameter, diameter, 0, CommandCompleted);
                    _core.CommandProcessor.EnQueue(command);
                    Trace.Assert(!string.IsNullOrEmpty(_state.FileName));
                    _state.Type = EditMode.Universe;
                }
            }
        }

        private ClientScene NewScene()
        {
            if (QuerySceneUnload())
            {
                ClientScene newScene;
                float diameter = AppMain.SIMPLE_SCENE_DIAMETER;
                newScene = ((ClientSceneManager)_core.SceneManager).NewScene("untitled", diameter, diameter, diameter, EntityCreated, EntityChanged, EntityDeleted);
                _state.FileName = newScene.Name;
                _state.Changed = false;
                _state.IsUntitled = true;
                InitScene(newScene);
                return newScene;
            }
            return null;
        }

        private ClientScene LoadScene(string fullpath)
        {
            try
            {
                string fileName = Path.GetFileName(fullpath);
                if (QuerySceneUnload())
                {
                    ClientScene newScene = ((ClientSceneManager)_core.SceneManager).Load(fullpath, EntityCreated, EntityChanged, EntityDeleted);
                    _state.FileName = fullpath;
                    _state.Changed = false;
                    _state.IsUntitled = false;
                    InitScene(newScene);
                    return newScene;
                }
                return null;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return null;
            }
        }                

        // returns true if user elects to EITHER save or willfully NOT save
        // returns false if user CANCELS
        private bool QuerySceneUnload()
        {
            if (AppMain._core.SceneManager != null)
            {
                // does the current scene need to be saved?
                if (_state.Changed)
                {
                    DialogResult result = MessageBox.Show(
                        "The current scene has not been saved since it was last modified.  Do you wish to save now?",
                        "Save current scene?", MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Cancel) return false;
                    if (result == DialogResult.No)
                    {
                        UnloadScene(false, ""); // unload the existing scene without saving
                    }
                    else
                    {
                        if (_state.IsUntitled)
                        {
                            // bring up the save as dialog
                            string filename = SaveAs();

                            if (string.IsNullOrEmpty(filename))
                                return false; // user canceled at the SaveAs dialog so abort Unload of existing scene
                            else
                                UnloadScene(true, filename);
                        }
                        // if the existing scene is already titled (meaning its been saved once or loaded from file without "new" file) then just save it
                        else
                        {
                            UnloadScene(true, _state.FileName);
                        }
                    }
                }
                else
                {
                    // we can unload it safely without saving
                    UnloadScene(false, _state.FileName);
                }
            }
            _state.IsUntitled = true;
            _state.Changed = false;
            return true;
        }

        private void UnloadScene(bool save, string filename)
        {
            if (save)
            {
                if (!String.IsNullOrEmpty(filename))
                {
                    Save(filename);
                }
                else
                    Debug.WriteLine("FormMain.UnloadScene() -- Invalid filename."); // only minimal filechecks here.  otherwise its up to client app to check that Return False
            }

            _core.SceneManager.Unload(filename);
            _core.SceneManager.UnloadAllScenes();
            ClearDockedWindows();
        }

        private void ClearDockedWindows()
        {
            treeEntityBrowser.Nodes.Clear();
            // todo: properties.Clear();
        }

        //        //private void OnSceneUnloaded()
        //        //{
        //        //    scene.RemoveViewport(m_cameraViewports[0]);
        //        //}

        private bool QuerySave()
        {
            // does the current scene need to be saved?
            if (_state.Changed)
            {
                DialogResult result = MessageBox.Show(
                    "The current scene has not been saved since it was last modified.  Do you wish to save now?",
                    "Save current scene?", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Cancel) return false;
                if (result != DialogResult.No)
                {
                    if (_state.FileName == "untitled")
                    {
                        // bring up the save as dialog
                        string filename = SaveAs();

                        if (string.IsNullOrEmpty(filename))
                            return false; // user canceled at the SaveAs dialog so abort Unload of existing scene

                        Save(filename);
                    }
                    // if the existing scene is already titled (meaning its been saved once or loaded from file without "new" file) then just save it
                    else
                    {
                        Save(_state.FileName);
                    }
                }
            }
            return true;
        }

        private string SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = AppMain.DATA_PATH;
            saveFileDialog.Filter = OPEN_ZIP_FILTER;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                return saveFileDialog.FileName;

            return null;
        }

        private void Save(string filename)
        {
            _core.SceneManager.Save(filename);
            _state.FileName = filename;
            _state.Changed = false;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Delete)
            {
                //// todo: DeleteEntity was never uncommented after last refactor
                //ICommand deleteSelected = new Keystone.Commands.DeleteEntity();
                //QueueCommand(deleteSelected);
            }
        }

        protected override void OnResize(EventArgs e)
        {
           // mViewManager.CurrentView.re
            base.OnResize(e);
        }
        private void buttonItem55_Click(object sender, EventArgs e)
        {

            if (mViewManager.CurrentView.Name == "login")
                mViewManager.ChangeView("editor", true);
            else
                mViewManager.ChangeView("login", true);
        }

        private void ribbonTabItemVehicles_Click(object sender, EventArgs e)
        {
            SwitchController(typeof(VehicleController).Name);

            // todo: we've now switched the controller but we also have to disable writes to the file manager
            // todo: ideally what would happen is our Vehicle tab would only exist in our Game mode version of the RibbonBar
            //         but for now to save time from having to create the code to manage bar switching, which is i think what we should do
            //         We should have a seperate Bar 
            //          In fact what i could do is embed the RIbbon bar in a custom control then ican design it the way i want
            //         and swap out the custom control on the mainform for a differently designed one
            Keystone.IO.FileManager.DisableWrite = true;
        }

        private void ribbonTabItemProceduralTexture_Click(object sender, EventArgs e)
        {
            Bar bar = dotNetBarManager.Bars["barDocumentDockBar"];
            DockContainerItem dock = new DockContainerItem();
            if (mLibNoiseTextureDesigner == null)
            {
                mLibNoiseTextureDesigner = new Controls.LibNoiseTextureDesigner();

                dock.Control = mLibNoiseTextureDesigner;
                dock.Text = "Procedural Texture";
                dock.Visible = true;
                bar.Items.Add(dock);

                bar.AlwaysDisplayDockTab = true;
            }
        }

        private void configureNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormNetwork networkDialog = new FormNetwork();
            networkDialog.Username = _iniFile.settingRead("network", "username");
            networkDialog.Password = _iniFile.settingRead("network", "password");
            networkDialog.ServerAddress = _iniFile.settingRead("network", "authenticationaddress");
            networkDialog.ServerPort = _iniFile.settingReadInteger("network", "authenticationport");

            if (networkDialog.ShowDialog() == DialogResult.OK)
            {
                _iniFile.settingWrite("network", "authenticationaddress", networkDialog.ServerAddress);
                _iniFile.settingWrite("network", "authenticationport", networkDialog.ServerPort.ToString());
                _iniFile.settingWrite("network", "username", networkDialog.Username);
                _iniFile.settingWrite("network", "password", networkDialog.Password);
                _iniFile.Save();
            }
            networkDialog.Close();
        }

        private void lobbyToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // is the key server configured? 
            //ShowNetworkTab();
            return;

            // can we authenticate?
            // 
            if (AppMain._networkClient == null)
            {
                string username = _iniFile.settingRead("network", "username");
                string password = _iniFile.settingRead("network", "password");
                AppMain._networkClient = new KeyEdit.Network.Client2(_iniFile, null);
            }
            else if (AppMain._networkClient.Status == KeyEdit.Network.Client2.NetworkStatus.Connected)
            {
                // if we're already connected, warn the user that this will disconnect them
                if (DialogResult.OK == MessageBox.Show("Disconnect from current network game?",
                                    "You are currently connected to a network game.  Clicking 'Ok' will disconnect you from that session.  Click 'cancel' to remain in your current network game.",
                                    MessageBoxButtons.OKCancel))
                {
                    AppMain._networkClient.Disconnect();
                }
                else // user canceled
                {
                    return;
                }
            }

            //_iniFile.settingWrite("network", "authenticationaddress", networkDialog.ServerAddress);
            //_iniFile.settingWrite("network", "authenticationport", networkDialog.ServerPort.ToString());

            // now we can attempt connection to the selected game server using that ticket
            //_core.Network.Connect(config.GameServerAddress, config.GameServerPort);


            // initate to grab the map data from the game server

            // load the map data as normal? (LoadScene(path))?
        }

        private void buttonItemDisplayMode(object sender, EventArgs e)
        {
            if (mViewManager.CurrentView.Name != "editor") return;

            switch (((DevComponents.DotNetBar.BaseItem)sender).Name)
            {
                case "buttonItemDisplaySingleViewport":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.Single);
                    break;

                case "buttonItemDisplayVSplit":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.VSplit);
                    break;

                case "buttonItemDisplayHSplit":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.HSplit);
                    break;

                case "buttonItemDisplayTripleLeft":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.TripleLeft);
                    break;

                case "buttonItemDisplayTripleRight":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.TripleRight);
                    break;

                case "buttonItemDisplayQuad":
                    ((Views.EditorView)mViewManager.CurrentView).SetDisplayMode(Views.EditorView.DisplayMode.Quad);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
 
        private void OnToolboxItemSelected(object sender, EventArgs e)
        {

            if (mViewManager.CurrentView.Name != "editor") return;
            string name = ((DevComponents.DotNetBar.BaseItem)sender).Name;
            Trace.WriteLine("Item selected " + name);

            switch (name)
            {
                case "buttonSelectTool": // default case which means no editing tool is set
                    ((EditController) AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.SelectionTool();
                    break;
                case "buttonMoveTool":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.MoveTool();
                    break;
                case "buttonRotateTool":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.RotateTool();
                    break;
                case "buttonScaleTool":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.ScaleTool();
                    //((EditController)AppMain._core.CurrentIOController).RegisterGizmoController(new ScalingManipulator());
                    //((EditController) AppMain._core.CurrentIOController).CurrentEditTool = null; // maybe selection pointer?
                    break;
                case "buttonLineTool":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.LineTool();
                    break;
                case "buttonRectangleTool":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.RectangleTool();
                    break;
                //case "buttonCircleTool":
                //    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.CircleTool();
                //    break;
                //case "buttonPolygonTool":
                //    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.PolygonTool();
                //    break;
                case "buttonThrowProjectile":
                    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.ThrowObjectTool();
                    break;
                default:
                    throw new NotImplementedException  ("Toolbox tool not implemented");
            }
        }


        private void OnPhysicsDemoSelected(object sender, EventArgs e)
        {

            if (mViewManager.CurrentView.Name != "editor") return;
            string name = ((DevComponents.DotNetBar.BaseItem)sender).Name;
            Trace.WriteLine("Physics Demo - " + name);

            // here we can bring up a dialog perhaps to 
            switch (name)
            {
                case "buttonWallDemo":
                    break;

                case "buttonIncomingDemo":
                    break;

                case "buttonSpheresDemo":
                    break;

                case "buttonCatapultDemo":
                    break;

                case "buttonPendulumDemo":
                    break;

                case "buttonPlinkoDemo":
                    break;
                case "buttonVehicleDemo":
                    break;
                case "buttonSandboxDemo":
                    break;
                default:
                    throw new NotImplementedException("Toolbox tool not implemented");
            }

            if (_core.CurrentIOController is EditController)
            {
                // todo: i think ideally we want for the gui to send commands and not directly interface with core as below
                // todo: maybe inside the EditController .Import actually queues commands
                ((EditController)_core.CurrentIOController).Import("", name, CommandCompleted);
            }
        }

        /// <summary>
        /// Add a mesh based entity to the scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemImportGeometry_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = AppMain.DATA_PATH;

            switch (((DevComponents.DotNetBar.BaseItem )sender).Name)
            {
                case "buttonImportVehicle":
                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
                    break;
                case "buttonImportActor":
                case "menuItemImportMultipartActor":
                    openFile.Filter = ACTOR_GEOMETRY_FILE_FILTER;
                    break;
                case "buttonItemImportMesh":
                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
                    break;
                case "buttonItemImportEditableMesh":
                    openFile.Filter = EDITABLE_MESH_FILE_FILTER;
                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
                    break;
                case "buttonImportMinimesh": 
                    openFile.Filter = STATIC_GEOMETRY_FILE_FILTER;
                    openFile.FilterIndex = DEFAULT_FILTER_INDEX;
                    break;
                case "buttonImportBillboard":
                    openFile.Filter = TEXTURE_FILE_FILTER;
                    openFile.FilterIndex = TEXTURE_FILTER_INDEX;
                    break;

                case "buttonItemStarfield":
                    openFile.Filter = TEXTURE_FILE_FILTER;
                    openFile.FilterIndex = TEXTURE_FILTER_INDEX;
                    break;
            }
            
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string fullName = openFile.FileName;
                string fileName = Path.GetFileName(fullName);

                if (_core.CurrentIOController is EditController)
                {
                    // todo: i think ideally we want for the gui to send commands and not directly interface with core as below
                    // todo: maybe inside the EditController .Import actually queues commands
                    ((EditController)_core.CurrentIOController).Import(fullName, ((DevComponents.DotNetBar.BaseItem)sender).Name, CommandCompleted);
                }
            }
        }

        private void buttonNewStar_Click(object sender, EventArgs e)
        {
            // USES IMPORT COMMANDS


            // pops up dialog where you can set some parameters
            //  - binary, trinary, + in the future
            // - 
            // add's a point light
            
            //
        }

        private void buttonNewWorld_Click(object sender, EventArgs e)
        {
            // type (gas, earthlike, rockball, ice, etc)

            // rings


        }

        private void buttonNewAsteroidBelt_Click(object sender, EventArgs e)
        {

        }

        private void buttonNewComet_Click(object sender, EventArgs e)
        {

        }

        private void buttonSpherePrimitive_Click(object sender, EventArgs e)
        {

        }

        private void buttonBoxPrimitive_Click(object sender, EventArgs e)
        {

        }

        // todo: add capsule, add 3 and 4 sided pyramids
        private void buttonCylinderPrimitive_Click(object sender, EventArgs e)
        {

        }

        private void buttonTeapotPrimitive_Click(object sender, EventArgs e)
        {

        }

        private void buttonTorusPrimitive_Click(object sender, EventArgs e)
        {

        }

        private void buttonPyramidPrimitive_Click(object sender, EventArgs e)
        {

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

         private void buttonTextColor_SelectedColorPreview(object sender, ColorPreviewEventArgs e)
        {
            BaseItem item = sender as BaseItem;
            _core.SetBackGroundColor(e.Color);
        }

        private void buttonTextColor_SelectedColorChanged(object sender, EventArgs e)
        {

            BaseItem item = sender as BaseItem;

            _backColor = new Keystone.Types.Color(((ColorPickerDropDown)item).SelectedColor.R,
                    ((ColorPickerDropDown)item).SelectedColor.G,
                    ((ColorPickerDropDown)item).SelectedColor.B,
                    ((ColorPickerDropDown)item).SelectedColor.A);

            _core.SetBackGroundColor(_backColor);

            for (int i = 0; i < mViewManager.CurrentView.ViewportControls.Length; i++)
                if (mViewManager.CurrentView.ViewportControls[i] != null)
                {
                    string sectionName = mViewManager.CurrentView.ViewportControls[i].Name;
                    _iniFile.settingWrite(sectionName, "backcolor", mViewManager.CurrentView.ViewportControls[i].Viewport.BackColor.ToString());
                }
        }

        private void buttonItemUndo_Click(object sender, EventArgs e)
        {
            _core.CommandProcessor.UnDo();
        }

        private void buttonItemRedo_Click(object sender, EventArgs e)
        {
            _core.CommandProcessor.ReDo();
        }

        private void buttonCut_Click(object sender, EventArgs e)
        {

        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {

        }

        private void buttonItemPaste_Click(object sender, EventArgs e)
        {

        }


        //        private void clearUndoHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        //        {

        //        }

        //        private void clearClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        //        {

        //        }


        //        private void menuItemLODExporter_Click(object sender, EventArgs e)
        //        {
        //            // pop up tool so user can set a LOD percentage as well as perhaps have option to add the LOD to the Model
        //            // and set a LOD switch distance

        //        }

        //        private void LODExport()
        //        {
        //            //TEST TO SEE IF DIRECTX EXPORT WORKS
        //            MeshTool tool = new MeshTool(_core.Scene, _core.MaterialFactory, _core.TextureFactory);
        //            TVMesh lodResult = tool.CreateLOD(@"E:\dev\artwork\dominus_art_public_domain\actors\scale_Renzok_static.x", true, true, .21f, "test1");
        //            string tmppath = @"E:\dev\vb.net\test\AGT3rdPersonTV3DDemo\Data\Actors\testdump\lod.tvm.";
        //            lodResult.SaveTVM(tmppath);
        //            //END TEST
        //        }

        private void buttonItemSolSystem_Click(object sender, EventArgs e)
        {

            // Options for scaling the models
            // - apply a scale during traversal to the model just as we apply a scale to the Region during traversal
            //      -   to avoid constant recomputing of bounding box, we should apply the scale to the model's that are loaded
            //          and the ones that are subsequently loaded when in "nav" mode.
            // - we use proxy models (using LOD style switching) for rendering scaled up models, billboards, icons. (icons which 
            //        can have a fixed screenspace size)
            // - what about an LOD switch or a NAV Switch that applies a scaling?  A scaling node of sorts... the question then would be
            // how do you get that scaling node to act more like a permanent toggle so that it doesnt have to scale every frame...
            // thats the problem with Transform nodes in general.  
            // Our current system of scaling the Region allows us to avoid touching the actual entities, but the only way to scale
            // distance down and model's up is to scale Region for distance, and then entity or models for the worlds and ships.
            // - I can render them as Imposters and then scale the imposters up instead!  This way when i render them to a
            // rendersurface, I can render them to 256x256 or whatever and then blit them at whatever actual scale i want.
            // So when in "navigation mode" stars, worlds, ships,s tations, etc will get rendered with the imposter system and the
            // imposter system will have a scale on it.
            //      - BUT HOW DO I HANDLE PICKING THEN?  HUD-ified icons would need a seperate 2d gui handler


            // sol
            buttonItemStar_Click(null, null);

            // mercury  orbital radius 57,910,000,000 meters,  diameter 4,880,000 meters
            Keystone.Celestial.Planet world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 57910000000);
            world.Diameter = 4880000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            // venus  orbital radius 108,200,000,000 meters, diameter 12,103,600 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 108200000000);
            world.Diameter = 12103600;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            // earth  149,600,000,000 meters, diameter 12,756,300 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 149600000000);
            world.Diameter = 12756300;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            // mars 227,940,000,000 meters, diameter 6,794,000 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 227940000000);
            world.Diameter = 6794000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            // asteroid belt 1000 km across


            // jupitor 778,330,000,000 meters, diameter 142,984,000 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 778330000000);
            world.Diameter = 142984000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, false);
            _core.PrimaryCamera.Region.AddChild(world);

            // saturn  1,429,400,000,000 meters, diameter 120,536,000 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 1429400000000);
            world.Diameter = 120536000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, false);
            _core.PrimaryCamera.Region.AddChild(world);

            // uranus 2,870,990,000,000 meters, diameter 51,118,000 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 2870990000000);
            world.Diameter = 51118000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, false);
            _core.PrimaryCamera.Region.AddChild(world);

            // neptune 4,504,000,000,000 meters, diameter 49,532,000 meters 
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d(0, 0, 4504000000000);
            world.Diameter = 49532000;
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, false);
            _core.PrimaryCamera.Region.AddChild(world);

            // pluto 5,913,520,000,000 meters, diameter 2,274,000 meters
            world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = new Vector3d (0,0, 5913520000000);
            world.Diameter = 2274000; 
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            // kupier belt  (pluto is a kupier belt object at the inner edge of the kupier belt
            // appears to be huge radius like all the way out to 500 au+

            // oort cloud 30 trillion km from Sun

        }

        private void buttonItemStar_Click(object sender, EventArgs e)
        {
            FileManager.SuspendWrite = true;

            Keystone.Celestial.Star star = new Keystone.Celestial.Star(Repository.GetNewName(typeof(Keystone.Celestial.Star)));
            star.Translation = _core.PrimaryCamera.Position;
            star.Diameter = 1392000000; // sun diameter
            Keystone.Celestial.ProceduralHelper.InitStar(star);
            _core.PrimaryCamera.Region.AddChild(star);

            FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            FileManager.SuspendWrite = false;
        }

        private void buttonItemBluePlanet_Click(object sender, EventArgs e)
        {
            FileManager.SuspendWrite = true;

            Keystone.Celestial.Planet world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = _core.PrimaryCamera.Position;
            world.Diameter = 12756200; // earth diameter
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, true);
            _core.PrimaryCamera.Region.AddChild(world);

            FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            FileManager.SuspendWrite = false;
        }


        private void buttonItemGasPlanet_Click(object sender, EventArgs e)
        {
            FileManager.SuspendWrite = true;

            Keystone.Celestial.Planet world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Translation = _core.PrimaryCamera.Position;
            world.Diameter = 142984000;//jupiter diameter
            Keystone.Celestial.ProceduralHelper.InitPlanet(world, false);
            _core.PrimaryCamera.Region.AddChild(world);
            
            FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            FileManager.SuspendWrite = false;
        }

        private void buttonItemMoon_Click(object sender, EventArgs e)
        {
            FileManager.SuspendWrite = true;
            
            Keystone.Celestial.Moon world = new Keystone.Celestial.Moon(Repository.GetNewName(typeof(Keystone.Celestial.Moon)));
            world.Diameter = 3476000; //moon's diameter
            world.Translation = _core.PrimaryCamera.Position;
            Keystone.Celestial.ProceduralHelper.InitMoon(world);
            _core.PrimaryCamera.Region.AddChild(world);

            FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            FileManager.SuspendWrite = false;
        }
        private void buttonItemPlanetoidField_Click(object sender, EventArgs e)
        {
            FileManager.SuspendWrite = true;

            Keystone.Celestial.Planet world = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            world.Diameter = 100000;
            world.Translation = _core.PrimaryCamera.Position;
            Keystone.Celestial.ProceduralHelper.InitAsteroidField(world, 1000);
            _core.PrimaryCamera.Region.AddChild(world);

            FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            FileManager.SuspendWrite = false;
        }

        private void buttonItemDustField_Click(object sender, EventArgs e)
        {

        }

        // be nice if i could just select the FX and modify them after they've been created.  That way when i'm
        // hitting the various Generate buttons i dont have to deal with a box asking what radius, how many elements, etc
        // it can just start with a default and tehn you can modify it easily.
        private void buttonItemStarfield_Click(object sender, EventArgs e)
        {
            // for the starfield effect, we simply track which region the user is in, where any 
            // planetoid belts in that region are, and then determine if the player is in that field
            // by testing a min/max radius, and their height above and below the plane of that belt.
            // this is something easily done by frame. While player is in the field, we track
            // an array of these "particles" and maintain them in a bubble of x radius around the player
            menuItemImportGeometry_Click(sender, e);
        }

        private void buttonItemStarFieldNoTexture_Click(object sender, EventArgs e)
        {
            ((EditController)_core.CurrentIOController).Import("", ((DevComponents.DotNetBar.BaseItem)sender).Name, CommandCompleted);
        }

        private void buttonItemSkybox_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Add a light bsed entity to the scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonItemDirectionalLight_Click(object sender, EventArgs e)
        {
            Keystone.Portals.Region region = _core.PrimaryCamera.Region; // _core.SceneManager.ActiveScene.FindSector(_core.PrimaryCamera.Position);

            // camera position is always in current region coords
            Vector3d pos = _core.PrimaryCamera.Position;

            Keystone.Commands.ICommand addlight = new AddLight(region, pos, new Vector3d(0.5f, -0.5f, 0.8f));
            QueueCommand(addlight);

        }

        private void buttonItemPointLight_Click(object sender, EventArgs e)
        {
            Keystone.Portals.Region region = _core.PrimaryCamera.Region; // _core.SceneManager.ActiveScene.FindSector(_core.PrimaryCamera.Position);

            // camera position is always in current region coords
            Vector3d pos = _core.PrimaryCamera.Position;

            Keystone.Commands.ICommand addlight = new AddLight(region, pos, new Vector3d(0.5f, -0.5f, 0.8f));
            QueueCommand(addlight);
        }

        //        private void mnuItemTestSmokeTrail_Click(object sender, EventArgs e)
        //        {
        //            // we need to load in a ship, and then be able to set a speed on it and have it move
        //            // right now the fields we require only exist in Player and NPC which implement ISTeerable
        //            // So maybe start by ImportPlayer 
        //            // allow us to give some basic movement commands to the ship to change it's course


        //            // load a smoke trail object and tie it to the selected ship such that new smoke segments in the circular queue
        //            // will follow the ship's position

        //            // the smoke should be able to turn on/off with engines (acceleration) and not just velocity

        //            // smoke trail length is a parameter along with fade rate
        //            // 

        private void buttonIncreaseVelocity_Click(object sender, EventArgs e)
        {
            ((EditController)_core.CurrentIOController).ProcessCommand("velocity increase 1000");

        }

        private void buttonDecreaseVelocity_Click(object sender, EventArgs e)
        {
            ((VehicleController)_core.CurrentIOController).ProcessCommand("velocity decrease 1000");
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            ((VehicleController)_core.CurrentIOController).ProcessCommand("velocity stop");
        }

        // temp button to place a portal of hardcoded points in a Vehicle's region (so that it moves relative with the vehicle)
        // and which connects back to the root region
        private void buttonItem45_Click(object sender, EventArgs e)
        {
            Vector3d[] coords = new Vector3d[4];
            Vector3d position = _core.PrimaryCamera.Position;
            Keystone.Portals.Region destination = _core.SceneManager.ActiveScene.Root.Region;

            // find the first vehicle for now and we'll add the portal to the interiorRegion entity of that Vehicle
            // 
            Keystone.Portals.Region parent = _core.SceneManager.ActiveScene.Root.Region;

            Keystone.Commands.ICommand command = new CreatePortal(parent , destination, coords, position, CommandCompleted);

           _core.CommandProcessor.EnQueue (command);
        }

        // temp button to place an occluder of hardcoded dimensions, position and rotation
        // or maybe actually what we want is to be able to set a flag on a static entity that it's an occluder
        // so we could just load a static mesh per normal, then set it's IsOccluder flag
        private void buttonItem46_Click(object sender, EventArgs e)
        {

        }


        #region ProceduralTexture Buttons
        private void buttonItemGenerateTexture_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.Generate();
        }

        // note:  blind's screenshot
        // http://www.smithbower.com/old/pics/sc_08.png
        private void buttonItemSphereMap_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.CreateEntityNode("Sphere Mapper", "Spherical texture mapping algorithm.",
            new string[] { "Module", "Palette", "MinX", "MinY", "MaxX", "MaxY", "Width", "Height" },
            new string[] { "module", "palette", "int", "int", "int", "int", "int", "int" },
            new bool[] { true, false, false, false, false, false, false, false },
            new string[] { "out" },
            new string[] { "texture" });
        }

        private void buttonPT_PlaneMap_Click(object sender, EventArgs e)
        {

        }

        private void buttonPT_CylinderMap_Click(object sender, EventArgs e)
        {

        }

        private void buttonPT_Select_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.CreateEntityNode("Select", "Select.",
            new string[] { "ControlModule", "SourceModule1", "SourceModule2", "LowerBound", "UpperBound", "EdgeFalloff" },
            new string[] { "module", "module", "module", "float", "float", "float" },
            new bool[] { true, true, true, false, false, false },
            new string[] { "out" },
            new string[] { "module" });
        }

        private void buttonPT_ScaleOutput_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.CreateEntityNode("Scale Output", "Scale output.",
            new string[] { "Module", "Scale" },
            new string[] { "module", "float" },
            new bool[] { true, false },
            new string[] { "out" },
            new string[] { "module" });
        }

        private void buttonItemScaleBiasOutput_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.CreateEntityNode("Scale Bias Output", "Scale bias output.",
            new string[] { "Module", "Scale", "Bias"},
            new string[] { "module", "float", "float"},
            new bool[] { true, false, false},
            new string[] { "out" },
            new string[] { "module" });
        }


        // Integer
        private void buttonItemInt_Click(object sender, EventArgs e)
        {
            mLibNoiseTextureDesigner.CreateValueTypeNode ("Integer", "Positive whole numbers starting at 0.",
             
               
            new string[] { "value"},
            new string[] { "2" },
            new string[] { "int" });

            KeyEdit.Controls.LibNoiseTextureDesigner.Parameter p = new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter("value", "int", false);
            p.Value = "1";
            p.EditInPlace = true;
            p.AssignmentRequired = false;

            mLibNoiseTextureDesigner.CreateValueTypeNode("Integer", "Positive whole numbers starting at 0.", 
                 new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter[] { p});
        }

        // Float
        private void buttonItemFloat_Click(object sender, EventArgs e)
        {
            KeyEdit.Controls.LibNoiseTextureDesigner.Parameter p = new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter("value", "float", false);
            p.Value = "1.0";
            p.EditInPlace = true;
            p.AssignmentRequired = false;

            mLibNoiseTextureDesigner.CreateValueTypeNode("Float", "Floating point numbers greater or equal to 0.0",
                 new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter[] { p });
        }

        private void buttonPT_Perlin_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Perlin", "Noise using the original perlin noise algorithm.");
        }

        private void buttonPT_FastBillow_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Fast Billow", "Billow using the fast billow algorithm.");
        }

        private void buttonPT_Billow_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Billow", "Billow using the original billow algorithm.");
        }

        private void buttonPT_FastNoise_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Fast Noise", "Noise using the fast perlin noise algorithm.");
        }

        private void CreateIPersistenceNoiseEntity(string name, string description)
        {
            mLibNoiseTextureDesigner.CreateEntityNode(name, description,
                       new string[] { "Seed", "Frequency", "Lacunarity", "OctaveCount", "Persistence", "NoiseQuality" },
                       new string[] { "int", "float", "float", "int", "float", "noisequality" },
                       new bool[] { false, false, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }

        private void buttonPT_FastRidgedMF_Click(object sender, EventArgs e)
        {
            CreateINoiseEntity("Fast Ridged Multifractal", "Ridged multifractal using the fast algorithm.");
        }

        private void buttonPT_RidgedMF_Click(object sender, EventArgs e)
        {
            CreateINoiseEntity("Ridged Multifractal", "Ridged multifractal using the original algorithm.");
        }

        private void CreateINoiseEntity(string name, string description)
        {
            mLibNoiseTextureDesigner.CreateEntityNode(name, description,
                       new string[] { "Seed", "Frequency", "Lacunarity", "OctaveCount", "NoiseQuality" },
                       new string[] { "int", "float", "float", "int", "noisequality" },
                       new bool[] { false, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }


        private void buttonPT_FastTurbulence_Click(object sender, EventArgs e)
        {
            CreateITurublence("Fast Turbulence", "Fast turbulence algorithm.");
        }

        private void buttonPT_Turbulence_Click(object sender, EventArgs e)
        {
            CreateITurublence("Turbulence", "Original turbulence algorithm");
        }

        private void CreateITurublence(string name, string description)
        {
            mLibNoiseTextureDesigner.CreateEntityNode(name, description,
                       new string[] { "Module", "Seed", "Frequency", "Power", "Roughness" },
                       new string[] { "module", "int", "float", "float", "int" },
                       new bool[] { true, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }


        private void buttonPT_Voronoi_Click(object sender, EventArgs e)
        {

        }


        private void buttonPT_Checkerboard_Click(object sender, EventArgs e)
        {

        }
        #endregion


        #region KeyStone Commands
        private void QueueCommand(Keystone.Commands.ICommand command)
        {
            _core.CommandProcessor.EnQueue(command);
        }

        private void CommandCompleted(Amib.Threading.IWorkItemResult result)
        {
            if (result.Exception == null)
            {
                Trace.WriteLine("Command completed successfully.", result.State.GetType().Name);
                EntityBase entity = null;

                // if an entity was imported, we need to add it to the browser
                if (result.State is GenerateUniverse)
                {
                    FileManager.DisableWrite = false;
                    GenerateUniverse gen =  (GenerateUniverse)result.State;
                    

                    FileManager.WriteNewNode(gen.mSceneInfo, true);

                    FileManager.Save(); // todo: this is saved in a temporary archive file and that's bad because we only save what's changed.  We never do a top to bottom save because most of the scene wont necessarily even be paged in.
                    FileManager.CLose();
                    FileManager.DisableWrite = true;
                    gen.mSceneInfo.Dispose();

                    LoadScene(gen.FileName);
                    return;
                }
                else if (result.State is CreatePortal)
                {
                    System.Diagnostics.Debug.WriteLine("Portal created.");
                }
                else if (result.State is ImportStaticEntity)
                {
                    entity = ((ImportStaticEntity)result.State).Entity;
                }
                else if (result.State is NewVehicle)
                {
                    // todo: normally a NewVehicle would not start with any Interior Mesh or Portals
                    // or the interior or exterior would be generated to match the bounds of the other

                    // only user vehicles or unowned vehicles\stations that are being boarded will load interiors
                    Vector3d[] points = new Vector3d[4];

                    entity = ((NewVehicle)result.State).Entity;

                    // rotate the ship 180 degrees for testing
                    entity.Rotation = new Vector3d(0, 180, 0);
                    // We'll start with interior and exterior on same coordinate system
                    // at origin in the world and just testing our previous portal culling.
                    // Once we get that re-enabled, we'll be able to add the transform through the portal

                    // - add an Interior region
                    Keystone.Portals.Interior interior = new Keystone.Portals.Interior("Interior_" + entity.ID);
                    

                    // - load and add a mesh to the interior region that will have our floors, ceilings and walls as viewed from inside
                    //   and most importantly, the hole that we'll look thru to the exterior
                    string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Test Frigate\test_frigate_interior.obj";
                    //Keystone.Appearance.DefaultAppearance app;
                    //Keystone.Elements.Mesh3d resource = Keystone.Elements.Mesh3d.Create(path, path, true, true, out app);
                    //Keystone.Elements.SimpleModel model = new Keystone.Elements.SimpleModel(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.SimpleModel)));
                    //if (app == null)
                    //{
                    //    app = new Keystone.Appearance.DefaultAppearance(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Appearance.DefaultAppearance)));
                    //    app.AddChild(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.matte));
                    //    app.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
                    //}
                    //model.AddChild(app);
                    //model.AddChild(resource);
                    ////StaticEntity interiorModeledEntity = new StaticEntity(Keystone.Resource.Repository.GetNewName(typeof(StaticEntity)), model);;
                    //StaticEntity interiorModeledEntity = new StaticEntity("StaticEntity_InteriorModeledEntity", model); ;
                    StaticEntity interiorModeledEntity = CreateStaticEntity(path);
                    interior.AddChild (interiorModeledEntity);

                    // add the portal to the Interior region that will look to the exterior
                    points[0] = new Vector3d(54.6024, 7.09593, -2.50019);  // top left viewed from interior
                    points[1] = new Vector3d(54.6024, 7.09593, 2.49981);  // top right viewed from interior
                    points[2] = new Vector3d(54.6024, 4.59593, 2.49981); // bottom right viewed from interior
                    points[3] = new Vector3d(54.6024, 4.59593, -2.50019);  // bottom left viewed from interior
                    
                  //  Keystone.Portals.Portal interiorPortal = new Keystone.Portals.Portal( "interiorPortal", _core.SceneManager.ActiveScene.Root, points);
                  //  interior.AddChild(interiorPortal);

                    // add a portal to the Vehicle that will look to the interior region
                    points[0] = new Vector3d(54.6024, 7.09593, 2.49981);  // top left viewed from exterior
                    points[1] = new Vector3d(54.6024, 7.09593, -2.50019);  // top right viewed from exterior
                    points[2] = new Vector3d(54.6024, 4.59593, -2.50019);  // bottom right
                    points[3] = new Vector3d(54.6024, 4.59593, 2.49981); // bottom left

                    Keystone.Portals.Portal exteriorPortal = new Keystone.Portals.Portal( "exteriorPortal", interior, points);

                    // finally add the interior to the Entity after the interior has geometry added to it elses it's bounds is null
                    // todo: small dilemna, i dont want to traverse Interior's during culling as children of the Vehicle but only through
                    // exterior portals if we're outside the vehicle, or as a starting node if we're inside the vehicle.  The reason
                    // i do still want it as a child of the Vehicle though is so we can get world transformation info from the vehicle
                    // so that we can apply the transform during rendering of the geometry.  NOTE: culling and picking we can
                    // transform the camera and mouse pick vectors to the object's local space first.
                    // 
                    entity.AddChild(interior); 
                    entity.AddChild(exteriorPortal);

                    // - add a chair mesh too
                    path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Test Frigate\component_objs\galcap-[jasonduskey5-15.11.2009-e1527]_tslocator_gmdc.obj";
                    StaticEntity captainsChair = CreateStaticEntity(path);
                    interior.AddChild(captainsChair);
                    //   - chair mesh cures +1 fatigue/exhaustion every 5 minutes up to maximum
                    //      - chair does not cure sleepiness
                    // - bed/bunk cures +1 sleep every 5 minutes up to maximum.  After 72 hours with little or no sleep
                    //   npc's and player too, will start to lose reaction time and make mistakes and even collapse.

                    // set the camera into this interior region (todo: our camera bounds check must be adaptable for the region it's in)

                    //
                    // todo: temp set this up to use iimposter rendering
            //        entity.Subscribe(((Keystone.Scene.ClientSceneManager)_core.SceneManager).FXProviders[(int)Keystone.FX.FX_SEMANTICS.FX_IMPOSTER]);
            
                }
                else if (result.State is ImportActor)
                {
                    entity = ((ImportActor)result.State).Entity;
                }
                else if (result.State is AddLight)
                {
                    entity = ((AddLight)result.State).Entity;
                }
                else if (result.State is ImportBillboard)
                {
                    entity = ((ImportBillboard)result.State).Entity;
                }
                
                if (entity != null)
                    ResourceAddedToRepository(entity);

            }
            else
                Trace.WriteLine("Command failed.", result.State.GetType().Name);

        }

        private StaticEntity CreateStaticEntity(string path)
        {
            
            Keystone.Appearance.DefaultAppearance app;
            Keystone.Elements.Mesh3d resource = Keystone.Elements.Mesh3d.Create(path, path, true, true, out app);
            Keystone.Elements.SimpleModel model = new Keystone.Elements.SimpleModel(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.SimpleModel)));
            if (app == null)
            {
                app = new Keystone.Appearance.DefaultAppearance(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Appearance.DefaultAppearance)));
                app.AddChild(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.matte));
                app.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
            }
            model.AddChild(app);
            model.AddChild(resource);
            StaticEntity entity = new StaticEntity(Keystone.Resource.Repository.GetNewName(typeof(StaticEntity)), model);;
            return entity;     
        }

        private static void OnSceneUnloadComplete(object sender)
        {

        }

        private void EntitySelected(SceneBase scene, EntityBase entity)
        {
//            //// update all related dialogs to reflect the current selection and available options for modifying this entity
//            //PropertyTable properties = CreateProxy(entity);

            if (entity != null)
                Trace.WriteLine(string.Format("Entity {0} selected.", entity.ID));
//            //if (entity.Name == "StaticEntity1")
//            //{
//            //    // todo: this sword needs to be added to boneID = 11 // "upper_torso"
//            //    entity.Rotation = new Vector3d(0, -100, 0);
//            //}

            propertyGrid.SelectedObject = entity;

        }

        private void EntityMouseOver(SceneBase scene, EntityBase entity)
        {
            Trace.WriteLine(string.Format("Entity {0} -> mouse over.", entity.ID));
        }
        
//        private BonedEntity npc1;
        private void EntityCreated(SceneBase scene, EntityBase entity)
        {
//            // todo:  populate Entity explorer
//            //Trace.WriteLine(string.Format("Entity {0} spawned.", entity.Name));
//            if (entity.ID == "NPC1")
//            {
//                npc1 = (BonedEntity)entity;
//            }
//            if (entity.ID == "NPC2")
//            {
//                // let's attach this entity to skeleton which we know is NPC1 for testing purposes
//                npc1.AddChild((BonedEntity)entity);
//            }
        }

        private void EntityChanged(SceneBase scene, EntityBase entity)
        {
 
        }

        private void EntityDeleted(SceneBase scene, EntityBase entity)
        {
            // if this entity is selected in the property grid, set grid's selectedobject to null
        }
        
        private void ResourceAddedToRepository(IResource resource)
        {
            AddTreeItem item = AddTreeItemHandler;
            if (treeEntityBrowser.InvokeRequired)
                treeEntityBrowser.BeginInvoke(item, resource);
            else
                AddTreeItemHandler(resource);
        }


        private delegate void AddTreeItem(IResource resource);
        private delegate void RemoveTreeItem(string key);

        // todo: rename to AddTreeEntityHandler perhaps... since we'll need seperate AddTreeEntityComponentHandler
        private void AddTreeItemHandler(IResource resource)
        {
            try
            {
                if (resource is EntityBase)
                {
                    EntityBase ent = (EntityBase)resource;
                    TreeNode newNode;

                    if (ent.Parent != null)
                    {
                        // search starting at root node collection
                        TreeNode[] nodes = treeEntityBrowser.Nodes.Find(ent.Parent.ID, true);
                        if (nodes.Length > 0)
                        {
                            nodes[0].Nodes.Add(ent.ID, ent.TypeName + " " + ent.ID);
                            // since this only contains entities, there should only ever be just one node with this key found
                            Trace.Assert(nodes.Length == 1, "More than one entity instance found in treeview.  Entities should only ever have one instance!");
                        }
                        else
                            treeEntityBrowser.Nodes.Add(  resource.ID, resource.TypeName + " " + resource.ID); // temp
                    }
                    else
                        treeEntityBrowser.Nodes.Add(resource.ID, resource.TypeName + " " + resource.ID);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Error adding node {0} to treeview." + ex.Message, resource.TypeName));
            }
        }

        /// <summary>
        /// Double click causes us to act like we're switching URLs or following a link to the internals of that Entity.
        /// We can later use the "back or up" commands to go back to the Entity treeview.   Using two seperate treeviews
        /// will make it easier to maintain our state when going between the Entity and it's Internals.  To simplify things we
        /// should not worry about a "history" stack or any such thing.  All we need is a single "Forward" button when an 
        /// Entity is highlighted and a "Back" image to take the place on this toggle button when viewing an Entity's internal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeEntityBrowser_NodeMouseDoubleClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);

            if (treeEntityBrowser.SelectedNode != null)
            {
               
                if (treeEntityBrowser.SelectedNode.Name == "..")
                {
                    // clear the treeview and move to the previous treeState 
                    treeEntityBrowser.Nodes.Clear();
                    // Display a wait cursor while the TreeNodes are being created.
                    Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                    // Suppress repainting the TreeView until all the objects have been created.
                    treeEntityBrowser.BeginUpdate();

                    TreeviewState state = mTreeStates.Pop();

                    AddResourceToTree (_core.SceneManager.ActiveScene.GetResource((string)state.Tag));
                    SelectPlugin(_core.SceneManager.ActiveScene.GetResource((string)state.Tag));

                    state.RestoreTreeView (treeEntityBrowser, "" );

                    // todo: now there's a slight issue here... what happens if
                    // we somehow add or delete new nodes to the scene such that when we
                    // revert back, nodes in the cached state do not match those in the actual scene
                    // Well i think for robustness this answers itself, we should not store the actual nodes, but just the parent
                    // since it should be impossible to delete a parent without being on that node in the tree 
                    // further, when trying to restore the expanded state, if a particular nodes in the expanded list does not match
                    // any node in the tree, we skip it
                   // treeEntityBrowser.node

                    // todo: if we do delete a parent, we should clear the stack and return to the root
                    treeEntityBrowser.EndUpdate ();
                    Cursor.Current = System.Windows.Forms.Cursors.Default;
                }
                else
                {
                    // double click is always preceded by NodeMouseClick (single click event) so there is no need to set the Selected
                    // entity in SceneManager.ActiveScene.Selected or to set the property grid again.
                    IResource resource = _core.SceneManager.ActiveScene.GetResource(treeEntityBrowser.SelectedNode.Name);
                    AddResourceToTree (resource);
                    SelectPlugin(resource);
                }
            }
        }

        /// <summary>
        /// If an EntityBase is being added, this Clears the tree and adds a new Entity resource as root node of the Treeview
        /// </summary>
        /// <param name="resourceID"></param>
        private void AddResourceToTree(IResource resource)
        {
            
            if (resource != null)
            {
                // if we're already on this resource, just return
                if (mCurrentResource == resource) return;
                mCurrentResource = resource;

                // save the tree state, push it onto the stack and then 
                // clear the treeview and repopulate it with the selected entities
                if (resource is EntityBase)
                {
                    TreeviewState state = new TreeviewState();
                    string parentID = "";
                    if (((EntityBase)resource).Parent != null)
                        parentID = ((EntityBase)resource).Parent.ID;

                    state.SaveTreeView(treeEntityBrowser, parentID);
                    mTreeStates.Push(state);

                    treeEntityBrowser.Nodes.Clear();

                    // if this is not the root region, add as first tree node "..." for returning to the previous parent
                    if (resource.ID != "root")
                        treeEntityBrowser.Nodes.Add("..", "..");

                    // add the entity itself
                    TreeNode node = treeEntityBrowser.Nodes.Add(resource.ID, resource.TypeName + " " + resource.ID);

                    // Add all the entity components as well as all recursive child entities.  But only our
                    // primary selected Entity will have it's sub-components loaded.  Child entities will appear just as if we
                    // were still in the main Entity browser mode
                    AddSubNodes((EntityBase)resource, node, false);

                }
            }
        }

        private KeyPlugins.AvailablePlugin SelectPlugin(IResource resource)
        {
            KeyPlugins.AvailablePlugin plugin = null;

            if (resource != null)
            {
                if (resource is Keystone.Entities.EntityBase )
                {
                    plugin = SelectPlugin ("Entity");
                }
                // display the plugin for the appropriate type
                //else if (resource is JigLibX.Physics.Body)
                //{
                //}
                else if (resource is Keystone.AI.Behavior.Behavior)
                {
                }
                else if (resource is Keystone.Elements.ModelBase)
                {
                }
                else if (resource is Keystone.Elements.Geometry)
                {
                    if (resource is Keystone.Elements.Mesh3d)
                    {
                        plugin = SelectPlugin(resource.GetType().Name);
                    }
                    else if (resource is Keystone.Elements.Actor3d)
                    { 
                    }
                    else if (resource is Keystone.Entities.Terrain )
                    {
                    }
                    else if (resource is Keystone.Elements.ParticleSystem )
                    {
                    }
                    else if (resource is Keystone.Elements.Emitter )
                    {
                    }
                    else if (resource is Keystone.Elements.Attractor)
                    {
                    }


                }
                else if (resource is Keystone.Animation.ActorAnimation) // todo this should be general IAnimation first and then we can check subtypes?
                {
                }
                else if (resource is Appearance)
                {

                }
                // GroupAttribute - Added as children of Appearance nodes.  A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
                else if (resource is Keystone.Appearance.GroupAttribute)
                {
                    // when selecting a specific GroupAttribute, we should highlight in the 3d viewport that selected part of the mesh
                    plugin = SelectPlugin("GroupAttribute");
                }
                else if (resource is Keystone.Shaders.Shader)
                {

                }
                else if (resource is Keystone.Appearance.Texture)
                {
                    plugin = SelectPlugin("Texture");
                }
                else if (resource is Keystone.Appearance.Material)
                {
                    // add the Material editor to the smart context sensitive plugin dock pane
                    plugin = SelectPlugin(resource.GetType().Name);

                }
                

                // when the IPlugin activates, it'll request updated state information
                if (plugin != null)
                    ((PluginHost.EditorHost)plugin.Instance.Host).Selected = resource;
            }

            return plugin;
        }

        private void AddSubNodes(Keystone.Elements.IGroup group, TreeNode parentNode, bool recurseChildEntities)
        {
            if (group.Children != null)
                for (int i = 0; i < group.Children.Length; i++)
                {
                    TreeNode subParent = parentNode.Nodes.Add(group.Children[i].ID, group.Children[i].TypeName + " " + group.Children[i].ID);
                    // now recurse all child Groups except Entities if prohibited
                    if (group.Children[i] is Keystone.Elements.IGroup)
                    {
                        if (group.Children[i] is EntityBase && !recurseChildEntities)
                            continue;

                        AddSubNodes((Keystone.Elements.IGroup)group.Children[i], subParent, recurseChildEntities);
                    }
                }
        }

        private IResource mCurrentResource;
        private System.Collections.Generic.Stack<TreeviewState> mTreeStates = new Stack<TreeviewState>();
        private void treeEntityBrowser_NodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);
            
            if (treeEntityBrowser.SelectedNode != null)
            {
                IResource resource = _core.SceneManager.ActiveScene.GetResource(treeEntityBrowser.SelectedNode.Name);
                if (resource != null) SelectPlugin(resource);

                ((ClientScene)_core.SceneManager.ActiveScene).Selected = _core.SceneManager.ActiveScene.GetEntity(treeEntityBrowser.SelectedNode.Name);
                propertyGrid.SelectedObject = _core.SceneManager.ActiveScene.GetEntity(treeEntityBrowser.SelectedNode.Name);
            }
        }
        
        /// <summary>
        /// Right Mouse Click Context Menu Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeEntityBrowser_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) return;

            treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);

            if (treeEntityBrowser.SelectedNode != null)
            {
                // based on the node type, select the context menu to use
                IResource resource = _core.SceneManager.ActiveScene.GetResource(treeEntityBrowser.SelectedNode.Name);
                if (resource != null)
                    SelectContextMenu(resource, e.Location);
                                
                //propertyGrid.SelectedObject = _core.SceneManager.ActiveScene.GetEntity( treeEntityBrowser.SelectedNode.Name);
            }
        }

        private void SelectContextMenu(IResource resource, Point location)
        {
            ContextMenuStrip menu = null;

            // todo: lets see if we can't just grab the right click menu from the plugin
            KeyPlugins.AvailablePlugin plugin = SelectPlugin(resource);
            if (plugin != null && plugin.Instance != null)
                menu = plugin.Instance.GetContextMenu(resource.ID, location);

            // EntityMenu
            // LODGeometry Menu
            // GroupAttribute Menu (add material, add texture) <-- layers too
            
            //if (resource != null)
            //{
            //    if (resource is Keystone.Entities.EntityBase)
            //    {
            //        contextMenuEntityBrowser.Show(treeEntityBrowser, location);
            //    }
            //    // display the plugin for the appropriate type
            //    //else if (resource is JigLibX.Physics.Body)
            //    //{
            //    //}
            //    else if (resource is Keystone.AI.Behavior.Behavior)
            //    {
            //    }
            //    else if (resource is Keystone.Elements.ModelBase)
            //    {
            //    }
            //    else if (resource is Keystone.Elements.Geometry)
            //    {
            //        if (resource is Keystone.Elements.Mesh3d)
            //        {

            //        }
            //        else if (resource is Keystone.Elements.Actor3d)
            //        {
            //        }
            //        else if (resource is Keystone.Entities.Terrain)
            //        {
            //        }
            //        else if (resource is Keystone.Elements.ParticleSystem)
            //        {
            //        }
            //        else if (resource is Keystone.Elements.Emitter)
            //        {
            //        }
            //        else if (resource is Keystone.Elements.Attractor)
            //        {
            //        }


            //    }
            //    else if (resource is Keystone.Animation.ActorAnimation) // todo this should be general IAnimation first and then we can check subtypes?
            //    {
            //    }
            //    else if (resource is Appearance)
            //    {

            //    }
            //    // GroupAttribute - Added as children of Appearance nodes.  A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
            //    else if (resource is Keystone.Appearance.GroupAttribute)
            //    {
            //        // when selecting a specific GroupAttribute, we should highlight in the 3d viewport that selected part of the mesh

            //    }
            //    else if (resource is Keystone.Shaders.Shader)
            //    {

            //    }
            //    else if (resource is Keystone.Appearance.Texture)
            //    {

            //    }
            //    else if (resource is Keystone.Appearance.Material)
            //    {



            //    }


            //}

            if (menu != null)
            {
                menu.Show(treeEntityBrowser, location);
            }
        }



        private void ResourceRemovedFromRepository(string resourceGuid)
        {
            RemoveTreeItem item = RemoveTreeItemHandler;
                    if (treeEntityBrowser.InvokeRequired)
                        treeEntityBrowser.BeginInvoke(item, resourceGuid);
            else
                RemoveTreeItemHandler(resourceGuid);
        }

        private void RemoveTreeItemHandler(string guid)
        {
            try
            {
                        TreeNode[] nodes = treeEntityBrowser.Nodes.Find(guid, true);
                if (nodes.Length > 0)
                            treeEntityBrowser.Nodes[nodes[0].Index].Remove();
                   
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Error removing node {0} to treeview." + ex.Message, guid));
            }   
        }
#endregion


        #region Plugin Specific
        private void LoadPlugins()
        {
            //Call the find plugins routine, to search in our Plugins Folder
            AppMain.Plugins.FindPlugins(AppMain.BASE_PATH + @"\Plugins");

            //Add each plugin to the treeview
            //foreach (KeyPlugins.AvailablePlugin pluginOn in AppMain.Plugins.AvailablePlugins.Values)
            //{
            //    TreeNode newNode = new TreeNode(pluginOn.Instance.Name);
            //    this.tvwPlugins.Nodes.Add(newNode);
            //    newNode = null;
            //}
        }

        /// <summary>
        /// Based on the selected node typename from the treeEntityBrowser, select the appropriate plugin
        /// to handle the editing of this node.
        /// </summary>
        /// <param name="nodeTypename"></param>
        private KeyPlugins.AvailablePlugin SelectPlugin(string nodeTypename)
        {
            if (!string.IsNullOrEmpty (nodeTypename))
            {
                //Get the selected Plugin
                KeyPlugins.AvailablePlugin selectedPlugin;
                if (AppMain.Plugins.AvailablePlugins.TryGetValue(nodeTypename, out selectedPlugin))
                {
                    // for debugging, create a string for the dock panel that will contain the plugin information
                    string pluginDescription = selectedPlugin.Instance.Name + 
                        "(" + selectedPlugin.Instance.Version + ")" + 
                        "By: " + selectedPlugin.Instance.Author + 
                        selectedPlugin.Instance.Description;

                    //Remove the previous plugin from the plugin dock panel
                    //Note: this only affects visuals.. doesn't close the instance of the plugin
                    //this.barRightDockBar.Controls.Clear();
                    this.panelDockContainerRight.Controls.Clear();
                    //Set the dockstyle of the plugin to fill, to fill up the space provided
                    selectedPlugin.Instance.MainInterface.Dock = DockStyle.Fill;

                    // add the usercontrol to the plugin dock panel.
                    //this.barRightDockBar.Controls.Add(selectedPlugin.Instance.MainInterface);
                    this.panelDockContainerRight.Controls.Add(selectedPlugin.Instance.MainInterface);

                    // todo: de-activate the previous plugin so it doesnt receive events

                    // assign the node type this plugin will modify and also so that changes to this node
                    // will result in the plugin being notified (but only when the changes are made outside of hte plugin)
                    // I think one way to accomplish this without having to require every node type to have some xtra glue
                    // is to simply monitor the commandprocessor since we will restrict modification to any node
                    // via Commands.  No direct editing of properties via the property grid.

                    // so it can update it's display with any new values
                   // selectedPlugin.Register(resource);
                    return selectedPlugin;
                }
            }
            return null;
        }
        #endregion 



        //        private void toolBar_ButtonClick(object sender, ToolStripItemClickedEventArgs e)
//        {
//            if (e.ClickedItem == toolBarButtonNew)
//                menuItemNew_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonOpen)
//                menuItemOpen_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonSolutionExplorer)
//                menuItemSolutionExplorer_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonPropertyWindow)
//                menuItemPropertyWindow_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonToolbox)
//                menuItemToolbox_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonOutputWindow)
//                menuItemOutputWindow_Click(null, null);
//            else if (e.ClickedItem == toolBarButtonTaskList)
//                menuItemTaskList_Click(null, null);
//            else if (e.ClickedItem == toolbarButtonVertexSelect) // selection type
//            {
                
//            }
//            else if (e.ClickedItem == toolbarButtonEdgeSelect )
//            {
                
//            }
//            else if (e.ClickedItem == toolbarButtonFaceSelect)
//            {}
//            else if (e.ClickedItem == toolbarButtonObjectSelect)
//            {}
//        }

//        //// the preview viewport is different because it does not render the exact same scene as the main
//        //// it is only rendering a single target and thus is rendering it's own unique scene that essentially contains just a light and a single 
//        //// entity, model, texture, particle system, etc
//        //private void Preview_HandleCreated(object sender, EventArgs args)
//        //{
//        //    Viewport vp = new Viewport(((FormViewport)sender).Text, ((FormViewport)sender).Handle, false);
//        //    vp.Camera = new PreviewCamera(1, 5000, 60, false, true, false, false);
//        //    ((FormViewport)sender).Viewport = vp;
//        //    _core.SetBackGroundColor(0.0f, 0.0f, 0.0f, 1.0f); // todo: this is hackish.  we need a more common way of setting the backcolor
//        //}

    }
}
