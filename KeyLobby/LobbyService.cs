using System;
using System.Collections.Generic;
using System.Threading;
using Authentication;
using KeyCommon;
using KeyServices;
using Lidgren.Network;
using System.Diagnostics;
using KeyCommon.DatabaseEntities;

namespace KeyLobby
{
    /// <summary>
    /// Keystone Master Server uses a lidgren netclient to authenticate with a Keystone Authentication Server
    /// and once authenticated, starts a Lidgren NetServer to accept game servers.  GameServers must
    /// use a valid ticket, and then it can post periodic state updates to the masterserver
    /// </summary>
    class LobbyService : KeyServiceBase
    {
        private const int DEFAULT_BROADCAST_GROUP = 1;
        private string _configPath;
        private string _configFile;
        private const string COMPANY_NAME = "SciFiCommand";
        private const string PRODUCT_NAME = "KGBLobby.config";

        private Stopwatch mStopWatch;
        private long mElapsedMilliseconds;
        private Settings.Initialization _ini;
        private KeyServerCommon.SQLStorageContext mGamesStorageContext;
        private LobbyManager mLobbyManager;

        
        private NetUDPClient m_authenticationClient;
        private NetUDPServer m_server;
        private NetTCPServer mTCPServer;
        private Algorithm m_hashAlgorithm = Algorithm.SHA256;
        private string mServiceName = "";
        private string mServicePassword  = "";
        private int m_servicePort = 2021;
        private string m_localIP = "127.0.0.1"; //"192.168.1.65";

        private string m_authenticationServiceName = "";
        private string m_authenticationServerIP = "127.0.0.1"; // "71.112.229.84"  // "192.168.1.64"
        private int m_authenticationPort = 2020;
        private bool m_authenticated = false;
        

        private Dictionary < System.Net.IPEndPoint , GameServerInfo> m_gameServers;

      
        /// <summary>
        /// In terms of our Authentication system, the Master server also serves the purpose of granting
        /// tickets for users to be able to login and play any game server that has registered with the master server.
        /// Thus when a user finds a game in the master server list that they want to play, they will under the hood
        /// first request a ticket from the master server to play on that host and then when they receive the ticket
        /// they will be able to initiate a login to that server.
        /// </summary>
        public LobbyService ()
        {
            _configPath = Settings.Initialization.CreateApplicationDataFolder(COMPANY_NAME);
            _configFile = Settings.Initialization.CreateApplicationConfigFile(COMPANY_NAME, PRODUCT_NAME, false);


            if (!Initialize()) // load settings
                return;

            string sectionName = "network";
            mServiceName = _ini.settingRead(sectionName, "servicename");
            mServicePassword = _ini.settingRead(sectionName, "servicepassword");
            m_authenticationServiceName = _ini.settingRead(sectionName, "authenticationservicename");

            m_gameServers = new Dictionary<System.Net.IPEndPoint, GameServerInfo>();
            //Services = new List<Service >();
            this.ServiceName = mServiceName;
            // create a new timespan object with a default of 1 seconds delay.
            m_delay = new TimeSpan(0, 0, 0,0, 0);

            StartAuthenticationClient();
        }


        ~LobbyService()
        {
            mGamesStorageContext.Dispose();
        }


        #if (DEBUG)
        public void DebugStartEntryPoint(string[] args)
        {
            OnStart(args);
        }
        #endif

        /// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
        protected override void OnStart(string[] args)
        {                       
            // once base.OnStart() is called, this method never completes until the project ends.
            // i'm not sure why that is since the execute is done on a seperate thread.
            // but i believe before we get to that thread, we do want to set up our completion ports and asych readers
            // and then use the .execute() for house keeping functions
            base.OnStart(args);

        }

        private void StartAuthenticationClient()
        {
            // create an authentication client with a default configuration
            NetConfiguration config = new NetConfiguration(m_authenticationServiceName);
            m_authenticationClient = new NetUDPClient(config);

            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
            m_authenticationClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, true);

            //NetBuffer buffer = NetBuffer.Create();
            KeyCommon.Messages.AuthenticationLogin login = new KeyCommon.Messages.AuthenticationLogin();
            login.Name = mServiceName;
            login.HashedPassword = Hash.ComputeHash(mServicePassword, m_hashAlgorithm);

            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID);
            login.Write(buffer);

            //buffer.Write((int)KeyCommon.Enumerations.Types.AuthenticateService);
            //buffer.Write(this.ServiceName);
            //buffer.Write(Hash.ComputeHash(mServicePassword, m_hashAlgorithm));

            m_authenticationClient.Connect(m_authenticationServerIP, m_authenticationPort, buffer.Data);
        }
        /// <summary>
        /// After the lobby has successfully authenticated itself, this call to StartLobby() will occur and we can start accepting
        /// user connections and game server connections and requests.
        /// </summary>
        private void StartLobby()
        {
            // note: once authenticated, a master server does not need any service ticket
            m_authenticated = true;
            NetConfiguration config = new NetConfiguration(mServiceName);
            config.Port = m_servicePort;
            m_server = new NetUDPServer(config);
            m_server.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
            m_server.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            m_server.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
            m_server.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, true);
            m_server.Bind();


            mStopWatch = Stopwatch.StartNew();

            string sectionName = "gamesdatabase";
            // for storing Games and Players, we use a provider to the Games Database "ProjectEvoGames"
            KeyServerCommon.PostgresProvider provider = new KeyServerCommon.PostgresProvider(
                _ini.settingRead(sectionName, "databasename"),
                _ini.settingRead(sectionName, "address"),
                _ini.settingReadInteger(sectionName, "port"),
                _ini.settingRead(sectionName, "username"),
                _ini.settingRead(sectionName, "password"));

            mGamesStorageContext = new KeyServerCommon.SQLStorageContext(provider);
            mLobbyManager = new LobbyManager(m_server, mGamesStorageContext);
        }

        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        protected override int Execute()
        {
            NetMessage[] messages;

            // Keep trying to authenticate this lobby with the Authentication Server while not successfully authenticated
            if (!m_authenticated)
            {
                // unlike our lobby for Evo, this lobby must first authenticate with an external authentication server
                // just like a game server or a client must.  It cannot start the lobby until it has authenticated.
                AuthenticationClientTick();
            }
            else
            {

                AuthenticationClientTick(); // we need to keep the authentication heartbeat after approval connection until
                                                      // the connect response is received by the authentication server.
                LobbyServerTick ();
            }
               
            return 0;
        }


        private void AuthenticationClientTick()
        {
            m_authenticationClient.Heartbeat(); // we'll use this thread to pump instead of having seperate internal client thread
            NetMessage[] messages = m_authenticationClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        m_authenticationClient.Connect(msg.Buffer.ReadIPEndPoint(), "Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Console.WriteLine("Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Console.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Console.WriteLine("New status: " + m_authenticationClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // note: Lobby does not need a ticket to connect to any other service.
                        if (m_authenticationClient.Status == NetConnectionStatus.Connected)
                        {
                            StartLobby();
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Console.WriteLine("Bad Message Received: " + msg.Buffer.ReadString());
                        break;
                   
                    case NetMessageType.Data:
                        // The authentication server sent this data.  A lobby will never expect any data here though right?...
                        string data = msg.Buffer.ReadString();
                        Console.WriteLine(data);
                        break;
                    default:
                        Console.WriteLine(string.Format("Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }

        private void LobbyServerTick()
        {
            // pump netserver until shutdown
            m_server.Heartbeat();
            NetMessage[] messages = m_server.GetMessages();
            if (messages == null ) return;

            foreach (NetMessage msg in messages)
            {
                // TODO: for the few rare tcp connections that for some reason couldnt use udp, we may eventually tweak things here
                // as far as whether we approve or not, whether we just accept and let the user send a packet after approved not within the approved
                // but one thing we should change is 
                // a) users should time out if idle.  That value for the few rare tcp authents should be 60 seconds max i think
                // b) our lobby server will change to not send all global users, but it will only accept TCP connections from one instance
                //     of a user and the timeout value for a user should be 15 minutes perhaps.
                // stay with the poker model.  pokerstars certainly is able to manage 10's of thousands of tcp connections.  however it limits
                // chat to just those at a table.  We should do the same.  Tables and private channels (e.g friends lists like steam)
                // ladder server <-- game matches are auto assigned when you are logged in.
                // tournament servers 
                // 
                switch (msg.Type )
                {
                    case NetMessageType.ConnectionApproval:
                
                        KeyCommon.Messages.Enumerations  cmd = (KeyCommon.Messages.Enumerations)msg.Buffer.ReadInt32();
                        if (cmd == KeyCommon.Messages.Enumerations.ServiceLogin)
                        {

                            string ip = ((IncomingNetMessage)msg).m_senderEndPoint.Address.ToString();
                            Lidgren.Network.NetConnectionBase connection = ((IncomingNetMessage)msg).m_sender;
                            //Debug.Assert(ip = connection.RemoteEndpoint.ToString)
                            KeyCommon.Messages.ServiceLogin login = new KeyCommon.Messages.ServiceLogin();

                            try
                            {
                                login.Read(msg.Buffer); // read is in try/catch block
                                
                                AuthenticatedLogin auth = new AuthenticatedLogin(login.AuthLogin,  mServicePassword);
                                                               
                                if (auth.Ticket != null && auth.Ticket.IsAuthentic(login.Name, ip, mServiceName))
                                {
                                    // TODO: this command to register a game should be completely seperate
                                    //          and then once it's registered, if it's a persistant free join server, it'll give game status updates
                                    //          via UDP.  Similarly the listentable for when registering should be provided there
                                    //          and not here in the lobby.
                                   
                                    // note: unlike Evo, our lobby server only has access to the game server but the authenticated login
                                    // contains the userID we need.
                                    KeyCommon.DatabaseEntities.User user = new KeyCommon.DatabaseEntities.User(-1); // TODO: this needs to be proper User.ID not -1
                                    Debug.Assert(auth.UserName == login.Name);
                                    user.Name = auth.UserName;
                                    //user.PrimaryKey = auth.UserPrimaryKey;
                                    mLobbyManager.AddUser(user, connection, DEFAULT_BROADCAST_GROUP);
                                    m_server.ApproveConnection(connection);
                                    Console.WriteLine("LobbService.LobbyServerTick():ConnectionApproval - Client '" + auth.UserName + "' authentication SUCCESS.");
                                }
                                else
                                    Console.WriteLine("LobbService.LobbyServerTick():ConnectionApproval - Client '" + auth.UserName + "'  authentication FAILED.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("LobbService.LobbyServerTick():ConnectionApproval - Exception " + ex.Message);
                            }
                        }
                        else
                        {
                            // unsupported command.  Log\flag\track? determine if user is attacker/abuser
                            // we could have seperate "alerts" log for these sorts of things
                            // i think we also do want a dedicated log for connect/disconnects/ticketrequests
                            // we should create a \logs\ folder in appdata as well
                            Console.WriteLine("LobbService.LobbyServerTick():ConnectionApproval - Unsupported command from un-authenticated client.");
                        }
                        break;

                    case NetMessageType.Data:
                        ProcessMessage((IncomingNetMessage)msg);
                        break;
                
                    case NetMessageType.StatusChanged:
                
                        switch (((IncomingNetMessage)msg).m_sender.Status)
                        {
                            case NetConnectionStatus.Disconnected:
                                if (mLobbyManager != null)
                                {
                                    NetConnectionBase connection = ((IncomingNetMessage)msg).m_sender;
                                    if (connection.Tag is KeyCommon.DatabaseEntities.User)
                                    {
                                        mLobbyManager.RemoveUser((KeyCommon.DatabaseEntities.User)connection.Tag);
                                        
                                    }
                                    
                                    else
                                    {
                                        Debug.Assert(false);
                                    }
                                }
                                break;
                            default:
                                Console.WriteLine("LobbService.LobbyServerTick():StatusChanged - Connection " + ((IncomingNetMessage)msg).m_sender.ToString() + msg.Buffer.ReadString());
                                break;
                        }
                        break;
                    case NetMessageType.DebugMessage :
                    case NetMessageType.VerboseDebugMessage:
                        Console.WriteLine(msg.Buffer.ReadString());
                        break;

                    case NetMessageType.BadMessageReceived:
                        Console.WriteLine("LobbService.LobbyServerTick():Bad Message Received - " + msg.Buffer.ReadString());
                        break;
                    default:
                        Console.WriteLine(string.Format("LobbService.LobbyServerTick():Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }

                mElapsedMilliseconds = mStopWatch.ElapsedMilliseconds - mElapsedMilliseconds;
                mLobbyManager.Update(mElapsedMilliseconds);
            }
            
        }
        


        private void ProcessMessage(IncomingNetMessage msg)
        {
            int command = msg.Buffer.ReadInt32();

            switch (command)
            {
                // if it's a lobby chat, echo it to all users?  
                case (int)KeyCommon.Messages.Enumerations.ChatMessage:
                    mLobbyManager.ForwardChat(msg);
                    break;

                case (int)KeyCommon.Messages.Enumerations.QueryGames: // returns tables and games based on specification of query
                    break;

                case (int)KeyCommon.Messages.Enumerations.RequestGamesList:
                    mLobbyManager.QueryGames(msg);
                    break;

                case (int)KeyCommon.Messages.Enumerations.Simulation_Register:  // for registering a game that doesnt first need a table
                    // TODO: reply with a password for use by users who wish to join it from the lobby.
                    //         a client trying to resume will still need to authenticate, then contact the lobby
                    //         then request the ticket for that game using any new password
                    // verify the user can only register one game or one table per authentication account.  Our own servers will be 
                    // able to use multiple accounts.
                    mLobbyManager.RegisterGame(msg);
                    // registerGame.BeginExecute (mLobbyManager.RegisterGame,  completionHandler...)
                    // results = registerGame.Execute (mLobbyManager.RegisterGame)
                    // if (registerGame.State == State.Success)
                    //       msg.Sender.SendMessage (new CommandSuccessful)...
                    // In a sense here, we still have our messages, that's part of the issue here... we're sending Messages
                    // not "Commands"  and so that's part of our disconnect in our thinking.  A message and then the processessing
                    // of those messages get handled by different functions
                    // Some messages which want to change a state of an entity will need to be validated.
                    // Some messages which want to call some method of an entity, there those entities will have
                    // their own validations in them... those can be loaded from script more easily that way
                    // Since i already have code for loading an entity script and assigning it.  The only thing ihavent done
                    // is save those scripts in the entity xml so they can be re-loaded automatically during paging.

                    // Similarly for validation, say changing the power output on an engine, server side can load any validation scripts
                    // but client can use same entity object and not do any validation on changes sent to it from the serve
                    break;

                case (int)KeyCommon.Messages.Enumerations.Simulation_Join:
                    mLobbyManager.JoinGame(msg);
                    break;




                // if it's a "Register" command, verify the user meets the 
                case (int)KeyCommon.Messages.Enumerations.Table_Create: 
                    Console.WriteLine("'Register' request  received.");
                    mLobbyManager.RegisterTable(msg);
                    break;

                case (int)KeyCommon.Messages.Enumerations.Table_Join:
                    break;

                case (int)KeyCommon.Messages.Enumerations.Table_Leave:


                case (int)KeyCommon.Messages.Enumerations.Table_Close:
                    Console.WriteLine("'Unregister' received.");
                    mLobbyManager.UnregisterTable(msg);
                //  if it's a "GameCompleted" command from a GameServer where the stats are then reported. Maybe not actually
                // reported to the lobby...
                    break;
                case (int)KeyCommon.Messages.Enumerations.Table_ReadyStatusChanged:
                    mLobbyManager.SetUserReadyStatus(msg);
                    break;



                default:
                    Console.WriteLine("Unexpected user command id '" + command.ToString ()  + "' received .");
                    break;
            }
        }

        //private void ProcessMessage(object message)
        //{
        //    NetMessage msg = (NetMessage)message;

        //    if (msg.Type == NetMessageType.DebugMessage)
        //    {
        //        Console.WriteLine("Debug: " + msg.Buffer.ReadString());
        //        return;
        //    }

        //    // approval for when Gameservers are trying to register with this Master server using an authenticated login packet
        //    // created from a ticket and session key received from the authentication server
        //    if (msg.Type == NetMessageType.ConnectionApproval)
        //    {
        //        Console.WriteLine(msg.Buffer.Data.ToString ());
        //        int command = msg.Buffer.ReadInt32();
        //        switch (command)
        //        {
        //            case 4: // authenticated login from GameServer to register on this Lobby 
        //                {
                          
        //                    System.Net.IPEndPoint endpoint = ((IncomingNetMessage) msg).m_senderEndPoint;
        //                    string ip = endpoint.Address.ToString();

        //                    try
        //                    {
        //                        // grab the server's own reported username which we'll then compare with that contained in the decrypted logon
        //                        string serverName = msg.Buffer.ReadString();

        //                        // read the rest of the data
        //                        byte[] login = msg.Buffer.ReadBytesRemaining();

        //                        // grab the authenticated login and verify it's validity
        //                        AuthenticatedLogin decryptedLogin = new AuthenticatedLogin(login, mServicePassword);

        //                        // only this serivce can decrypt the authenticator which will contain the user's login name and ip address
        //                        // which must match the one the user sends, and the ip endpoint from the message
        //                        if (decryptedLogin.Ticket.IsAuthentic(serverName, ip, mServiceName))// TODO: verify whether I pass in this ip or the one from decryptedLogin!  Been over 3 years since i first coded this and i'm not too happy with the commenting or the variable naming.. way too confusing trying to remember what this is all about
        //                        {

        //                            // TODO: verify this game server is not already running?  
        //                            // m_gameServers.TryGetvalue(endpoint, info);

        //                            // connection is approved and we will now accept status updates from this server
        //                            m_server.ApproveConnection(NetTime.Now, ((IncomingNetMessage) msg).m_sender, null);
                                    
        //                            KeyCommon.Entities.GameServerInfo info = new KeyCommon.Entities.GameServerInfo();
        //                            info.Name = decryptedLogin.UserName;
        //                            info.IP = ip;
        //                            info.Port = endpoint.Port;
        //                            //info.Type = GameType.Demo_1_0;
        //                            // TODO: i dont believe im removing game servers for removed connections
        //                            m_gameServers.Add(endpoint, info);
        //                        }
        //                        else
        //                        {
        //                            // TODO: not authentic.  log this?  I don't believe there's anything else we' do eh?
        //                            Console.WriteLine("Authentication failed:  Invalid Ticket" + ip);
        //                        }
        //                    }
        //                    catch
        //                    {
        //                        // authentication of ticket failed. log?
        //                        Console.WriteLine("Authentication failed:  Could not decrypt login" + ip);
        //                    }
        //                    break;
        //                }
        //            case 5: // retreive game server list (can use filters via arguments such as by region) 
        //                {
        //                    // these are attached to approval's but they 
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    else if (msg.Type == NetMessageType.Data)
        //    {
        //        // we verify that this user is in fact already authenticated and is allowed to request service tickets by
        //        // verify their remote endpoint has a connection 
        //        System.Net.IPEndPoint endpoint = ((IncomingNetMessage) msg).m_senderEndPoint;
        //        NetConnectionBase connection = m_server.GetConnection(((IncomingNetMessage)msg).m_senderEndPoint);
        //        if (connection != null && connection.Status == NetConnectionStatus.Connected)
        //        {
        //            // accept status update or other notification from a registered gameserver
        //            int command = msg.Buffer.ReadInt32();
        //            switch (command)
        //            {
        //                case (int)KeyCommon.Enumerations.Types.GameServerInfo: // gameserver is sending a standard status update packet, so update the entry
        //                    {
        //                        KeyCommon.Entities.GameServerInfo info = FindGameServer(endpoint);
        //                        if (info == null)
        //                        {
        //                            // TODO: log this attempt at sending status update for unauthenticated server?
        //                          // return;
        //                        }
        //                        // verify that this information is infact coming from the server we think it is?
        //                        // although what would be the point of putting up fake info using spoofed ip?
        //                        // DoS is about it...
        //                        // TODO: so should we really verify authentication everytime we receive a status update?
        //                        // probably not.  Most important is to just track the request rate and to both throtttle the request rate
        //                        // (ignore repeated requests if interval is within some delta) and 
        //                        // ignore the ip altogether if they seem to be abusing

        //                        // read the data and update the info
        //                        IRemotableType newInfo = RemotableFactory.GetRemotableType (command, msg.Buffer);
        //                        if (newInfo == null) break;

        //                        info = (KeyCommon.Entities.GameServerInfo)newInfo;
        //                        Console.WriteLine("Received status for server '{0}' at {1}:{2}" , info.Name , info.IP, info.Port);
        //                        break;
        //                    }
        //                default:
        //                    break;
        //            }

        //            // - game state update 
        //            // - a shutdown notification
        //            // 
        //        }
        //    }
        //}

        //private KeyCommon.Entities.GameServerInfo FindGameServer(System.Net.IPEndPoint endpoint)
        //{
        //    KeyCommon.Entities.GameServerInfo info;
        //    if (m_gameServers.TryGetValue( endpoint, out info ))
        //        return info;

        //    return null;
        //}


        private bool Initialize()
        {
            Settings.Initialization ini = null;
            try
            {
                // attempt to load settings from config settings
                _ini = Settings.Initialization.Load(_configFile);
            }
            catch
            {
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
    }
}
