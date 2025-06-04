
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Authentication;
using Keystone;
using Keystone.IO;
using Keystone.Scene;
using Keystone.Simulation;
using KeyServices;
using Lidgren.Network;
using Settings;
using KeyCommon.DatabaseEntities;

namespace KeyGameServer
{
    /// <summary>
    /// Game Server must authenticate and then it can "register" with a master server.
    /// Once registered, users over the internet can connect to it.  Unregistered game servers
    /// can run, but game clients must connect to them directly via IP address.  Further,
    /// clients playing on unregistered game servers will not have their stats reported.  Only
    /// registered game servers upload results to the stats tracking web server.
    /// </summary>
    public class GameServerService : KeyServiceBase
    {
         
        // Unlike master servers that are usually just 1 (at least in a region of the world) there can be
        // many game servers so the "username" is not the service name, but is like the password
        // the user's registered name.  Only localhost game servers do not need a username or apssword since
        // they dont have to authenticate

        private NetUDPClient mAuthenticationClient;
        private NetUDPClient mLobbyClient;
        private NetUDPServer mGameServer;
        private KeyServerCommon.SQLStorageContext mGamesStorageContext;              
        
        // authentication server vars
        private Algorithm m_hashAlgorithm = Algorithm.SHA256;
        private string m_authenticationServiceName = "";
        private string m_authenticationServerAddress ;
        private int m_authenticationPort ;
        private bool m_authenticated = false;

        // lobby server vars
        private string mLobbyServiceName = "";
        private string mLobbyServiceAddress;
        private int mLobbyServicePort;
        private const int MASTER_SERVER_STATUS_UPDATE_FREQ = 5000; // 5 seconds
        private int m_lastMasterServerStatusUpdate = 0;
        private bool mLoggedIntoLobby = false; 


        private string m_serverName = "Test Server 1"; // unique to every game server
        private string mListenTable;
        private int mPort;
        private string mAddress;
        private string mServicePassword = "password";
        private string mServiceName = "KGBGameServer"; // network service name, not the windows service name

        // init related vars
        private Settings.Initialization _ini;
        private string CONFIG_DEFAULTS_FILENAME = "KGBGameServer_defaults.config";
        private string _configFile, _configPath;
        public readonly string BASE_PATH = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\..\\..\\..\\";
        //public string BASE_PATH = @"C:\Documents and Settings\Hypnotron\My Documents\dev\c#\KeystoneGameBlocks\"; //System.Application.StartupPath + "\\..\\..\\..\\";
        public string DATA_PATH = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\..\\..\\..\\", "data\\");
        public string MODS_PATH = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\..\\..\\..\\", "data\\mods\\");
        private const string COMPANY_NAME = "SciFiCommand";
        private const string PRODUCT_NAME = "KGBGameServer.config";

        private string mModName = @"caesar"; //common.zip";

        private bool mGameServerRunning = false;
        private bool mGameRegistered = false;
        private long m_lastGameUpdate;
        private long m_updateElapsed;
        private AuthenticatedTicket mAuthenticatedLobbyTicket;  // a seperate ticket is needed for every game that is hosted on this server
                                                                                        // this is something i can implement later

        private Keystone.Core _core;
        private ServerSceneManager  mSceneManager;
        //private GameServerInfo _gameInfo;
        private static string DEFAULT_MAP = "smallgalaxy.zip";

        public GameServerService ()
        {
            //Services = new List<Service >();
            this.ServiceName = mServiceName;
            // delay is the rate between calls to Execute made by this window's service
            // create a new timespan object with a default of 1 seconds delay.
            m_delay = new TimeSpan(0, 0, 0,0, 0);
        }

    #if (DEBUG)
        public void DebugStartEntryPoint(string[] args)
        {
            OnStart(args);
        }
   #endif


        private bool Initialize()
        {
            Console.WriteLine("Initialize()");
            Settings.Initialization ini = null;
            try
            {
                Console.WriteLine("Try loading ini...");
                // attempt to load settings from config settings
                _ini = Settings.Initialization.Load(_configFile);
            }
            catch
            {
                Console.WriteLine("Ini catch 1");
                return false;
            }

            // load the default from the resx and save it to the app settings config using the _configFile path
            if (_ini == null)
            {
                try
                {
                    // create a new ini from the default values file
                    // that file should be read only or added as a resource.
                    // TODO: grab from resx
                    //  ini = Settings.Initialization.Load();
                    SaveSettings(_configFile);
                }
                catch
                {
                    Console.WriteLine("Ini catch 2");
                    return false;
                }
            }
            if (_ini == null) throw new ArgumentNullException();
            return true;
        }

        private void SaveSettings(string filename)
        {
            Settings.Initialization.Save(filename, _ini);
        }

        /// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Starting...");
            _configPath = Settings.Initialization.CreateApplicationDataFolder(COMPANY_NAME);
            _configFile = Settings.Initialization.CreateApplicationConfigFile(COMPANY_NAME, PRODUCT_NAME, false);
            

            if (!Initialize()) // load settings
                return;

            string sectionName = "network";
            mServiceName = _ini.settingRead(sectionName, "servicename");
            Console.WriteLine("Service name = " + mServiceName);
            this.ServiceName = mServiceName;

            m_authenticated = false;
            mLoggedIntoLobby = false;
            mGameServerRunning = false;

            
            StartAuthenticationClient();     

            // once base.OnStart() is called, this method never completes until the project ends.
            // i'm not sure why that is since the execute is done on a seperate thread.
            // but i believe before we get to that thread, we do want to set up our completion ports and asych readers
            // and then use the .execute() for house keeping functions
            base.OnStart(args);    
        }

        private void StartAuthenticationClient()
        {
            Console.WriteLine("starting authentication client.");
            string sectionName = "network";
            m_authenticationServerAddress = _ini.settingRead(sectionName, "authenticationaddress");
            m_authenticationPort = _ini.settingReadInteger(sectionName, "authenticationport");
            m_authenticationServiceName = _ini.settingRead(sectionName, "authenticationservicename");

            NetConfiguration lidgrenConfig = new NetConfiguration(m_authenticationServiceName);
            sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = _ini.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = _ini.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.UseBufferRecycling = _ini.settingReadBool(sectionName, "enablebufferrecycling");
            
            mAuthenticationClient = new NetUDPClient(lidgrenConfig);
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, _ini.settingReadBool(sectionName, "enablerejectmessages"));
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, _ini.settingReadBool(sectionName, "enableconnectionapproval"));
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, _ini.settingReadBool(sectionName, "enabledebugmessages"));
            mAuthenticationClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, _ini.settingReadBool(sectionName, "enableverbosemessages"));


            KeyCommon.Messages.AuthenticationLogin login = new KeyCommon.Messages.AuthenticationLogin();
            login.Name = mServiceName;
            login.HashedPassword = Hash.ComputeHash(mServicePassword, Authentication.Algorithm.SHA256);


            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID);
            login.Write(buffer);

            Console.WriteLine("connecting to authentication server.");
            mAuthenticationClient.Connect(m_authenticationServerAddress, m_authenticationPort, buffer.Data);
        }

        private void StartLobbyClient()
        {
            m_authenticated = true;
            string sectionName = "network";
            mLobbyServiceAddress = _ini.settingRead(sectionName, "lobbyaddress");
            mLobbyServicePort = _ini.settingReadInteger(sectionName, "lobbyport");
            mLobbyServiceName = _ini.settingRead(sectionName, "lobbyservicename");
            
            // create a lobby client with a default configuration
            NetConfiguration lidgrenConfig = new NetConfiguration(mLobbyServiceName);
            sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = _ini.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = _ini.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.UseBufferRecycling = _ini.settingReadBool(sectionName, "enablebufferrecycling");

            mLobbyClient = new NetUDPClient(lidgrenConfig);
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, _ini.settingReadBool(sectionName, "enablerejectmessages"));
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, _ini.settingReadBool(sectionName, "enableconnectionapproval"));
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, _ini.settingReadBool(sectionName, "enabledebugmessages"));
            mLobbyClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, _ini.settingReadBool(sectionName, "enableverbosemessages"));

            AuthenticatedLogin authenticatedLogin = new AuthenticatedLogin (mServiceName , mAuthenticatedLobbyTicket.SessionKey,  mAuthenticatedLobbyTicket.TicketData);
            KeyCommon.Messages.ServiceLogin login = new KeyCommon.Messages.ServiceLogin(mServiceName, "kgblobby");
            login.AuthLogin = authenticatedLogin.ToBytes();
                        

            // to the lobby we include our mAuthenticatedTicket directly in the hail data
            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID);
            login.Write(buffer);
            
            mLobbyClient.Connect(mLobbyServiceAddress, mLobbyServicePort, buffer.Data);
        }
        
		private Keystone.Simulation.GameTime mGameTime;
        private void StartGameServer()
        {
            mLoggedIntoLobby = true;
            NetConfiguration lidgrenConfig = new NetConfiguration(this.ServiceName);
            string sectionName = "network";
            mListenTable = _ini.settingRead(sectionName, "listentable");
            mServicePassword = _ini.settingRead(sectionName, "servicepassword");

            IPAddressWithMask[] endpoints = IPAddressWithMask.Parse(mListenTable);
            if (endpoints.Length > 1) 
                // if there are multiple endpoints we want to listen to, then we have to bind to IPAddress.Any (0.0.0.0)
                // which lets us listen simultaneously from LAN or Internet or anywhere
                mAddress = System.Net.IPAddress.Any.ToString();
            else
                mAddress = endpoints[0].Address.ToString();
            //mAddress = _ini.settingRead(sectionName, "address"); // TODO: i believe this "address" section can be deleted since we use "listentable" now
            
            mPort = _ini.settingReadInteger(sectionName, "port");
        

            lidgrenConfig.Port = mPort;
            sectionName = "lidgren";
            lidgrenConfig.MaxConnections = _ini.settingReadInteger(sectionName, "maxconnections");
            lidgrenConfig.MaxOutstandingAccepts = _ini.settingReadInteger(sectionName, "maxoutstandingaccepts");
            lidgrenConfig.SendBufferSize = _ini.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = _ini.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.MaximumUserPayloadSize = _ini.settingReadInteger(sectionName, "maxuserpayloadsize");
            lidgrenConfig.UseBufferRecycling = _ini.settingReadBool(sectionName, "enablebufferrecycling");


            // create the servers which will listen and handle user connections
            //mGameServer = new NetTCPServer(lidgrenConfig);
            mGameServer = new NetUDPServer(lidgrenConfig);
            mGameServer.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, _ini.settingReadBool(sectionName, "enablerejectmessages"));
            mGameServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, _ini.settingReadBool(sectionName, "enableconnectionapproval"));
            mGameServer.SetMessageTypeEnabled(NetMessageType.DebugMessage, _ini.settingReadBool(sectionName, "enabledebugmessages"));
            mGameServer.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, _ini.settingReadBool(sectionName, "enableverbosemessages"));


            sectionName = "gamesdatabase";
            // for storing Games and Players, we use a provider to the Games Database "ProjectEvoGames"
            KeyServerCommon.PostgresProvider provider = new KeyServerCommon.PostgresProvider(
                _ini.settingRead(sectionName, "databasename"),
                _ini.settingRead(sectionName, "address"),
                _ini.settingReadInteger(sectionName, "port"),
                _ini.settingRead(sectionName, "username"),
                _ini.settingRead(sectionName, "password"));

            mGamesStorageContext = new KeyServerCommon.SQLStorageContext(provider);

            //   - similarly for the master server eventually where maybe we want to log which servers are popular and 
            //    having the most players and which game types.  We also may want to deal with more detailed player
            //    logs on our official competitive servers.  we'll see...


            mGameTime = new GameTime(1.0f);
            m_updateElapsed = Stopwatch.GetTimestamp();

            //  start listening for connections
            if (mGameServer is NetUDPServer )
                mGameServer.Initialize();
            else
                mGameServer.Bind(mAddress, mPort);

            //TODO: player ships will be in xml and stored directly in the database as a .prefab (no images or geometry data.  The gameserver
            // itself will have those in the mod's path
            string _appDataPath = Initialization.CreateApplicationDataFolder(COMPANY_NAME);
            string SAVE_PATH = System.IO.Path.Combine(_appDataPath, "saves");
            throw new NotImplementedException("Current save name must be set at some point or the xml deserialization will not be able to find .kgbentities in the save path");
            _core = new Core(_ini, BASE_PATH , DATA_PATH , MODS_PATH, SAVE_PATH);
            mSceneManager  = new ServerSceneManager ( _core , mGameServer);

            mGameServerRunning = true;
            Console.WriteLine("Network initialized.");

            
            // Load a game or start a new default game to be hosted on this gameserver?

            // for alpha, we just want to register a single hosted furball server running on our primary furball map
            // which consists of the Sol system, several colonies and stations and with the constant objective of
            // taking over the stations and gaining control of them and any colonies.
            // (nukes are banned and allowed ship types are fixed for alpha as well).

            // Yes a hacked gameserver can just ignore any client ticket, however since those clients arent registered
            // the gameserver will not be able to have the reported stats for those clients added to the main player stats db
            // Furthermore, if the gameserver registers any match tables automatically, then clients who notify th elobb
            // that they want to join an available ladder game or some such, can get automatically added to the
            // registration table for that game... that can be the basis for some auto match making functionality although
            // maybe those users who notify they want to play, the players with better connections and cpu get set as
            // the host and they end up registering the game.

            Keystone.Workspaces.IWorkspace dummy = null;

            string map = "simple.zip";
            Game game = new Game(new Host(mListenTable, mServicePassword));
            game.Host.Name = mServiceName;
            game.mName = "24/7 Furball Test Server";
            //game.Description = "";
            game.Map = map;
            game.mPassword = ""; // friendly passwrod to join game, not host account password

            // TODO: this call should be done in a seperate thread and upon complettion then we should RegisterGame

            string gameName = "";
            if (game != null) gameName = game.mName;

            System.Diagnostics.Debug.WriteLine("SceneBase.Ctor() - Simulationed " + gameName + " loaded.");
            Keystone.Simulation.ISimulation sim = new Simulation(0.0f, game);

           Scene scene = mSceneManager.Load(  mListenTable, mServiceName,  mServicePassword, map, sim);       
            
            RegisterGameWithLobby(scene.Simulation.Game);

        }

        private void RequestLobbyTicket()
        {
            KeyCommon.Messages.RequestTicket request = new KeyCommon.Messages.RequestTicket();
            request.ServiceName = "kgblobby"; 
            mAuthenticationClient.SendMessage(request);
        }

        /// <summary>
        /// Register an existing game with the Lobby so that clients will see it and be able to join it
        /// </summary>
        private void RegisterGameWithLobby(Game game)
        {

            //Lidgren.Network.IPAddressWithMask[] endpoints = Lidgren.Network.IPAddressWithMask.Parse(netconfig.NetworkConfigElement.ListenTable);
            //if (endpoints == null || endpoints.Length < 1) 
            // throw new Exception("No valid endpoints can be parsed from ListenTable.");



            // get a game summary object... wait, as far as the lobby goes, shouldnt the full config be sent along with
            // any subsequent changes?  Only the client should get preliminary summaries with ability to query individual servers
            // for more details

            //KeyCommon.Entities.GameSummary summary = gameConfig.GetSummary();

            KeyCommon.Messages.RegisterGame registration = new KeyCommon.Messages.RegisterGame(game);

            // after sending, we await the registration successful response 
            // The only proper way is for the clients to request their ticket for this game from the authentication server
            // and perhaps for the lobby to only send the game server the list of user names who registered at the table
            mLobbyClient.SendMessage(registration);
        }

        private void OnGameRegistered()
        {
            string name = mServiceName;
            Console.WriteLine("Scene Registered.");
            mSceneManager.GetGame(name).Registered = true;
            mGameRegistered = true;
        }

        private void OnGameRegistrationFailed()
        {
            Console.WriteLine("Scene Registration FAILED.  Unloading scene.");
            string name = mServiceName;
            mSceneManager.GetGame(name).Registered = false;
        }

        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        protected override int Execute()
        {
            NetMessage[] messages;

            // pump authentication client until we've fully authenticated
            if (!m_authenticated )
            {
                AuthenticationClientTick();
            }
            else if (mAuthenticatedLobbyTicket == null)
            {
                AuthenticationClientTick();
            }
            else
            {
                AuthenticationClientTick(); // we need to keep the authentication heartbeat after approval connection until
                // the connect response is received by the authentication server.  Need to find a better way around that...
                LobbyClientTick();
            }

            if (mGameServerRunning)
            {
                LobbyClientTick();  // need to periodically send updates to lobby
                GameServerTick();

                long time = Stopwatch.GetTimestamp();
	            long elapsed = time - m_updateElapsed;
	            //m_lastMasterServerStatusUpdate += elapsed;
	            m_updateElapsed = time;
	            mGameTime.Update (elapsed); // TODO: shouldn't this be in seconds
                mSceneManager.Update(mGameTime);
                // for each hosted persistant furball game, grab a gameStatus object and send that to the lobby
                // if (mGameRegistered)
                //  {
                //// send periodic status updates to the lobby via our UDPLobby connection
                //if (m_lastMasterServerStatusUpdate >= MASTER_SERVER_STATUS_UPDATE_FREQ)
                //{
                //    m_lastMasterServerStatusUpdate = 0;
                //    // we report status updates to the master server using our ticket
                //    // the master server will maintain a connection object.
                //    //  if a master server does not get an update for a game server within some interval
                //    // it will be delisted and that connection object will be closed and the master server
                //    // will need to re-authenticate to get access again.

                //    mLobbyClient.SendMessage(_gameInfo);
                //    //mLobbyClient.SendMessage("TestServer, This is a test server.");
                //    Console.WriteLine("Sending status update to master server.");
                //   }
                //}
            }
            return 0;
        }

        private void AuthenticationClientTick()
        {
            mAuthenticationClient.Heartbeat(); 
            NetMessage[] messages = mAuthenticationClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        mAuthenticationClient.Connect(msg.Buffer.ReadIPEndPoint(), "Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Console.WriteLine("Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Console.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Console.WriteLine("New status: " + mAuthenticationClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // if we're now connected, gab the ticket and then set our flag
                        if (mAuthenticationClient.Status == NetConnectionStatus.Connected)
                        {
                            RequestLobbyTicket();
                            
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Console.WriteLine("Bad Message Received: " + msg.Buffer.ReadString());
                        // TODO: check the statistics on this connection
                        if (msg is Lidgren.Network.IncomingNetMessage)
                        {
                            NetConnectionBase userConnection = ((Lidgren.Network.IncomingNetMessage)msg).m_sender;
                            // if you're not letting Lidgren automatically handle badpacket
                            // allowance based disconnects, you can do it here
                            if (userConnection.Statistics.BadPacketsReceived > 100)
                            {
                                // TODO: add that .Disconnect option that allows application
                                // to tell lidgren server to disconnect a specific user
                                //mGameServer.Disconnect (userConnection, "Too many bad packets... suspicious...");
                                //userConnection.Disconnect(, 0.0);
                                throw new NotImplementedException("TODO: this needs to exist in authentication and master server too");
                            }
                        }
                        break;
                   
                    case NetMessageType.Data:
                        // The authentication server sent this data which is probably our LobbyServer ticket reply
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Console.WriteLine(string.Format("Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }

        private void LobbyClientTick()
        {
            mLobbyClient.Heartbeat(); 
            NetMessage[] messages = mLobbyClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        mLobbyClient.Connect(msg.Buffer.ReadIPEndPoint(), "Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Console.WriteLine("Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Console.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Console.WriteLine("New status: " + mLobbyClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        if (mLobbyClient.Status == NetConnectionStatus.Connected)
                        {
                            // start up a furball game server
                            StartGameServer();
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Console.WriteLine("Bad Message Received: " + msg.Buffer.ReadString());
                        break;

                    case NetMessageType.Data:
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Console.WriteLine(string.Format("Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }

        private void GameServerTick()
        {
            
            

            // pump netserver until shutdown.  NetServer can accept new clients
            // as well as run simulation
            // As for clients, their authentication procedure is slightly different in that
            // when they go to connect to this gameserver, first they must contact the authentication
            // server and get an authentication ticket specific for that game server.  
            // it's done transparently and it's done for each new gameserver they try to connect to.  
            // Ticket expiration is variable and the specific value we use can be determined later.  
            // A few checks we'll need though is to always require re-authorization if the user's IP 
            // changes within the expiration window.

            mGameServer.Heartbeat();
            NetMessage[] messages = mGameServer.GetMessages();
            if (messages != null && messages.Length > 0)
            {
                foreach (NetMessage msg in messages)
                {
                    switch (msg.Type )
                    {
                    // our game server processes received messages as they arrive
                    // but it runs the simulation at a fixed frequency
                    // approval for when clients are trying to connect with this GameServer
                        case NetMessageType.ConnectionApproval:
                    
                            // read int32 that will verify this is an authenticated login attempt
                            int command = msg.Buffer.ReadInt32();
                            Debug.Assert(command == (int)KeyCommon.Messages.Enumerations.ServiceLogin);

                            IncomingNetMessage incomingMsg = (IncomingNetMessage)msg;
                            string ip = incomingMsg.m_senderEndPoint.Address.ToString();
                            KeyCommon.Messages.ServiceLogin login = new KeyCommon.Messages.ServiceLogin();
                            login.Read(msg.Buffer);

                            // find the specific game hosted here this user is trying to connect to since only that specific
                            // game account can decrypt the authenticated login
                            Game game = mSceneManager.GetGame(login.ServiceName);  
                            if (game != null)
                            {
                                try
                                {
                                    AuthenticatedLogin auth = new AuthenticatedLogin(login.AuthLogin, game.Host.Password);
                                    //  must match the one the user sends, and the ip endpoint from the message
                                    if (auth.Ticket != null && 
                                        auth.UserName == login.Name && 
                                        auth.Ticket.IsAuthentic(auth.UserName, ip, game.Host.Name))
                                    {
                                        mGameServer.ApproveConnection(((IncomingNetMessage)msg).m_sender);
                                        Console.WriteLine("Client authentication SUCCESS.");
                                        mSceneManager.AddPlayer(login.Name, auth.Ticket.SessionKey, ((IncomingNetMessage)msg).m_sender, game.Host.Name);
                                    }
                                    else
                                        Console.WriteLine("Client authentication FAILED.");
                                }
                                catch (Exception ex) 
                                { 
                                    Console.WriteLine("Client authentication FAILED." + ex.Message); 
                                }
                            }
                            else
                            {
                                //  the game does not exist on this server
                                Console.WriteLine("Client refused because the game '" + login.ServiceName + "' is not hosted on this gameserver.");
                            }
                            break;
                       case NetMessageType.StatusChanged:
                            NetConnectionBase connection = ((IncomingNetMessage)msg).m_sender;

                            switch (((IncomingNetMessage)msg).m_sender.Status)
                            {
                                case NetConnectionStatus.Connected :
                                    Player player = (Player)connection.Tag;
                                    KeyCommon.Messages.Scene_Load loadScene = new KeyCommon.Messages.Scene_Load();
                                    string fullPath = _core.ScenesPath + player.GameName;
                                    throw new NotImplementedException ("The above fullPath is wrong.  i think i need to combine datapath with GameName need to fix that.");
                                    loadScene.FolderName = fullPath;
                                    Game g = mSceneManager.GetGame(player.GameName);
                                    loadScene.FolderName = g.Map;

                                    connection.SendMessage(loadScene, NetChannel.ReliableInOrder1);

                                    //// send the user a notice to spawn a vehicle and also add that vehicle here
                                    //KeyCommon.Messages.Simulation_Spawn spawn = new KeyCommon.Messages.Simulation_Spawn();
                                    //spawn.ID = "v1"; // hard coded for now
                                    //spawn.ParentID = "root";
                                    //string entryName = "meshes/vehicles/b5_hyperion/hyperion.kgbmodel";
                                    //spawn.Resource  = new KeyCommon.IO.ResourceDescriptor(mModName, entryName);
                                    throw new NotImplementedException();
                                    //connection.SendMessage(spawn, NetChannel.ReliableInOrder1);
                                    break;
                                case NetConnectionStatus.Disconnected:

                                    if (mSceneManager != null)
                                    {
                                        mSceneManager.RemovePlayer((Player)connection.Tag);
                                    }
                                    break;

                                default:
                                    Console.WriteLine(msg.Buffer.ReadString());
                                    break;
                            }
                            break;
                        case NetMessageType.Data:
                            mSceneManager.MessageProc((Lidgren.Network.IncomingNetMessage)msg);
                            break;
                    
                        case NetMessageType.DebugMessage :
                        case NetMessageType.VerboseDebugMessage:
                            Console.WriteLine("Debug: " + msg.Buffer.ReadString());
                            break;
                        case NetMessageType.BadMessageReceived:
                            // can occur during handshake if the appident is incorrect
                            Console.WriteLine("Bad Message Received: " + msg.Buffer.ReadString());
                            break;
                        default:
                            Console.WriteLine("Unexpected message type from User");
                            break;
                    }
                }
            }
        }

        private void ProcessUserMessage(IncomingNetMessage message)
        {

            int command = message.Buffer.ReadInt32();

            switch (command)
            {
                case (int)KeyCommon.Messages.Enumerations.TicketReply :
                    try
                    {
                        KeyCommon.Messages.TicketRequestReply ticketRequestReply = new KeyCommon.Messages.TicketRequestReply();
                        ticketRequestReply.Read(message.Buffer);
                        mAuthenticatedLobbyTicket = new AuthenticatedTicket(ticketRequestReply.AuthenticatedTicket, mServicePassword);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not validated received Ticket.");
                    }
                    if (mAuthenticatedLobbyTicket != null)
                    {
                        StartLobbyClient();
                    }
                    break;
                case  (int)KeyCommon.Messages.Enumerations.CommandSuccess :
                    OnGameRegistered();
                    break;
                case (int)KeyCommon.Messages.Enumerations.CommandFail :
                    OnGameRegistrationFailed();
                    break;
                //case (int)KeyCommon.Commands.j
                default:
                    break;
            }

        }
    }
}
