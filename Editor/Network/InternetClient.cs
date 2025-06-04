using System;
using System.Collections.Generic;
using Lidgren.Network;
using Authentication;
using System.Diagnostics;

namespace KeyEdit.Network
{
    // application side custom implementation of the Keystone.Network.NetworkClientBase
    public class InternetClient : Keystone.Network.NetworkClientBase 
    {
        private enum ClientState
        {
            Terminating,
            Root,
            GettingTicket,
            Authentication_Connected,
            Authentication_NotConnected,
            Authentication_Connecting,
            Authentication_WaitingForTicket,
            Authentication_Waiting_For_Game_Ticket,
            Lobby_Connected,
            Lobby_NotConnected,
            Lobby_Connecting,
            Lobby_Disconnecting,
            Game_NotConnected,
            Game_Connecting,
            Game_Connected,
            Game_Disconnecting,
            Game_Disconnected
        }

        private enum ClientTrigger
        {
            Cancel,
            TimeOut,
            FatalError,
            TerminationComplete,
            Tick,
            Disconnect,
            Authentication_RequestTicket,
            Authentication_Connect,
            Authentication_Disconnect,
            Authentication_Connected,
            Authentication_Disconnected,
            Authentication_TicketReceived,
            Lobby_Connect,
            Lobby_Disconnect,
            Lobby_Connected,
            Lobby_Disconnected,
            Game_Connect,
            Game_Disconnect,
            Game_Connected,
            Game_Disconnected
        }

        private enum GameState
        {
            Connected,
            WaitingForConfig,  // receiving map details, user ship data, match rules/config
            Loading,
            Starting,             // for match games, waiting for all players to signal they are "ready"
            Started,
            Spectating,
            Spawning,
            
            Dead,
            GameComplete,   // for non persistent servers with match games that have a clear win/loss game like Starcraft 
            Disconnected
        }

        private enum GameTriggers
        {
            Spawn,
            ReadyToSpawn,  // after game config received, and the world is loaded and user is ready to spawn and play
            End,
            Die
        }
       

        Stateless.StateMachine<GameState, GameTriggers> mGameStateMachine;
        Stateless.StateMachine<ClientState, ClientTrigger> mConnectionStateMachine;
        Stateless.StateMachine<ClientState, ClientTrigger>.TriggerWithParameters<AuthenticatedTicket> mTicketReceivedTrigger;


        private Lidgren.Network.NetClient mAuthenticationClient;
        private Lidgren.Network.NetClient mLobbyClient;
        internal Authentication.Algorithm mAuthenticationHashAlgo;
        internal int mLobbyRefreshInterval = 5000;

        private string mAuthenticationServiceName;
        internal string mAuthenticationServerAddress = "localhost";
        internal int mAuthenticationServerPort = 0;
        internal string mLobbyServiceName;
        internal string mLobbyServiceAddress = "localhost";
        internal int mLobbyServicePort = 0;

        protected AuthenticatedTicket mAuthenticatedLobbyTicket;
        protected AuthenticatedTicket mGameTicket; 


        public InternetClient(Settings.Initialization ini) : base (ini)
        {
            InitializeConnectionStateMachine();

            InitializeAuthenticationClient();
            InitializeLobbyClient();
        }

        #region Public Methods
        public void ConnectToLobby()
        {
            // NOTE: .State will report the top level state in a current hierarchy
            // to test for the actual current substate, we must use IsInState(ClientState.Lobby_NotConnected)
            if (mConnectionStateMachine.IsInState(ClientState.Lobby_NotConnected))

                mConnectionStateMachine.Fire(ClientTrigger.Lobby_Connect);
            else if (mConnectionStateMachine.State == ClientState.Game_Connected)
            {
                // disconnect from game first?
                // mConnectionStateMachine.Fire 

                // if we're already connected to the game we can't connect to lobby?
                // TODO: lots of checks to do here...
            }
            else
            {
                Debug.WriteLine("InternetClient.ConnectToLobby() - else");
            }
        }

        public void ConnectToGame(string serviceName, string address, int port)
        {
            mGameServerName = serviceName;
            mGameServerPort = port = 2022;
            mGameServerAddress = address = "127.0.0.1";
            //mConnectionStateMachine.Fire(ClientTrigger.Game_Connect, serviceName);
            mConnectionStateMachine.Fire(ClientTrigger.Game_Connect);
        }
        #endregion 



        private void InitializeAuthenticationClient()
        {
            Debug.WriteLine("InternetClient.InitializeAuthenticationClient() - Starting Authentication Client.");
            
            mAuthenticationHashAlgo = Authentication.Algorithm.SHA256;
            mCurrentAuthenticationAttempts = 0;
            mCurrentConnectAttempts = 0;

            string sectionName = "network";
            mAuthenticationServerAddress = mINI.settingRead(sectionName, "authenticationaddress");
            mAuthenticationServerPort = mINI.settingReadInteger(sectionName, "authenticationport");
            mAuthenticationServiceName = mINI.settingRead(sectionName, "authenticationservicename");

            NetConfiguration lidgrenConfig = new NetConfiguration(mAuthenticationServiceName);
            sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = mINI.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = mINI.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.UseBufferRecycling = mINI.settingReadBool(sectionName, "enablebufferrecycling");

            mAuthenticationClient = new NetUDPClient(lidgrenConfig);
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, mINI.settingReadBool(sectionName, "enablerejectmessages"));
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, mINI.settingReadBool(sectionName, "enableconnectionapproval"));
            mAuthenticationClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, mINI.settingReadBool(sectionName, "enabledebugmessages"));
            mAuthenticationClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, mINI.settingReadBool(sectionName, "enableverbosemessages"));
        }

        private void InitializeLobbyClient()
        {
            string sectionName = "network";
            mLobbyServiceAddress = mINI.settingRead(sectionName, "lobbyaddress");
            mLobbyServicePort = mINI.settingReadInteger(sectionName, "lobbyport");
            mLobbyServiceName = mINI.settingRead(sectionName, "lobbyservicename");

            // create a lobby client with a default configuration
            NetConfiguration lidgrenConfig = new NetConfiguration(mLobbyServiceName);
            sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = mINI.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = mINI.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.UseBufferRecycling = mINI.settingReadBool(sectionName, "enablebufferrecycling");

            mLobbyClient = new NetUDPClient(lidgrenConfig);
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, mINI.settingReadBool(sectionName, "enablerejectmessages"));
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, mINI.settingReadBool(sectionName, "enableconnectionapproval"));
            mLobbyClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, mINI.settingReadBool(sectionName, "enabledebugmessages"));
            mLobbyClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, mINI.settingReadBool(sectionName, "enableverbosemessages"));

            //lidgrenConfig.MaximumTransmissionUnit = 
            //lidgrenConfig.Port = NETWORK_PORT

            //lidgrenConfig.ThrottleBytesPerSecond  
            //lidgrenConfig.TimeoutDelay = 
            //lidgrenConfig.DisconnectLingerMaxDelay = 
            //lidgrenConfig.HandshakeAttemptRepeatDelay = ' not used for TCP Client or Server
            //lidgrenConfig.HandshakeAttemptsMaxCount =   ' not used for TCP Client or Server
            //lidgrenConfig.AnswerDiscoveryRequests = False ' not used for TCP Client or Server
            //lidgrenConfig.ResendTimeMultiplier =               ' not used for TCP Client or Server
            //'lidgrenConfig.MaxAckWithholdTime =        ' not used for TCPClient
            //'lidgrenConfig.PingFrequency =                 ' not used for TCPClient
            //lidgrenConfig.MaxConnections                  ' not used for TCPClient
        }

        protected void InitializeGameClient(string lidgrenAppIdent)
        {
            // create a lobby client with a default configuration
            NetConfiguration lidgrenConfig = new NetConfiguration(lidgrenAppIdent); // mGameServerName is the app ident and must be game specific
            string sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = mINI.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = mINI.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.MaximumUserPayloadSize = mINI.settingReadInteger(sectionName, "maxuserpayloadsize");

            lidgrenConfig.UseBufferRecycling = mINI.settingReadBool(sectionName, "enablebufferrecycling");

            // TODO: NOTE: Game client and Game server must both be either UDP or TCP/IP
            mGameClient = new NetUDPClient(lidgrenConfig);
            mGameClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, mINI.settingReadBool(sectionName, "enablerejectmessages"));
            mGameClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, mINI.settingReadBool(sectionName, "enableconnectionapproval"));
            mGameClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, mINI.settingReadBool(sectionName, "enabledebugmessages"));
            mGameClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, mINI.settingReadBool(sectionName, "enableverbosemessages"));
            

            Debug.WriteLine("InternetClient.InitializeGameClient()");
        }


        /// <summary>
        /// There are two types of states we concern ourselves with.  States which are transitioned to by
        /// external (user button clicks) triggers and states which are transitioned to by internal (network messages
        /// received) triggers.  That's it.  Keep this in mind when designing state machines and it will be easier.  If there
        /// is no trigger, there is no state change.
        /// </summary>
        private void InitializeConnectionStateMachine()
        {
            mConnectionStateMachine = new Stateless.StateMachine<ClientState, ClientTrigger>(ClientState.Lobby_NotConnected);
            mTicketReceivedTrigger = mConnectionStateMachine.SetTriggerParameters<AuthenticatedTicket>(ClientTrigger.Authentication_TicketReceived);


            mConnectionStateMachine.Configure(ClientState.Terminating)
                .Permit(ClientTrigger.TerminationComplete, ClientState.Lobby_NotConnected);

            // the root state serves primarilty to handle cancels, errors, disconnects from any other state
            mConnectionStateMachine.Configure(ClientState.Root)
               // .OnEntry(() => TickTest())  // will get called for all child states as well every .Tick trigger event
                //.OnEntryFrom <ClientState>(()=>TickTest())
                //.PermitReentry(ClientTrigger.Tick)  // <-- Can't use this or the state machine will end up in this Root superstate and not in the substate after execution of this trigger
                                                                   //   however to use this, this .Root state must never ever be a final state.  In fact ANY superstate should never be a final resting state 
                                                                   // That goes for substates with their own substates too!.  We must always end only
                                                                    // only the leaf most substates.
                .Permit (ClientTrigger.Tick,  mConnectionStateMachine.State )
                .PermitDynamic(ClientTrigger.Game_Connect, () => Select_Game_Connect()) // needs to be in root for joining favorites 
                .Permit (ClientTrigger.Cancel, ClientState.Terminating)
                .Permit(ClientTrigger.FatalError, ClientState.Terminating);  // cleanup, then return to Lobby_NotConnected state

                // LOBBY - NOT CONNECTED 
                mConnectionStateMachine.Configure(ClientState.Lobby_NotConnected)
                    .SubstateOf(ClientState.Root)
                    .OnEntry(() => OnEntry_Lobby_NotConnected())
                    .PermitDynamic(ClientTrigger.Lobby_Connect, ()=> Select_Lobby_Connect());

                // LOBBY - CONNECTING
                mConnectionStateMachine.Configure(ClientState.Lobby_Connecting)
                    .SubstateOf(ClientState.Root)
                    .OnEntryFrom(mTicketReceivedTrigger, ticket => OnEntry_Lobby_Connecting(ticket))
                    .Permit(ClientTrigger.Lobby_Disconnect, ClientState.Lobby_Disconnecting)
                    .Permit(ClientTrigger.Lobby_Connected, ClientState.Lobby_Connected);

                // LOBBY - CONNECTED
                mConnectionStateMachine.Configure(ClientState.Lobby_Connected)
                    .SubstateOf(ClientState.Root)
                    .OnEntry (()=> OnEntry_Lobby_Connected())
                    .Permit(ClientTrigger.Lobby_Disconnect, ClientState.Lobby_NotConnected);

                mConnectionStateMachine.Configure(ClientState.Lobby_Disconnecting)
                    .SubstateOf(ClientState.Root)
                    .Permit(ClientTrigger.Lobby_Disconnected, ClientState.Lobby_NotConnected);

            // IMPORTANT: To simplify this, i think i need to keep this state machine seperate from 
            //                    the GameServer connection state.  Then there is no issue of which state we go to
            //                    from any substate
            // below now we would have some internal states such as 
            // 1 connecting to authentication server
            // 2 trigger.AuthenticationServer_Connected
            // 3 State - waiting for ticket (for lobby)
            // 4 Trigger - TimeOut or Error or TicketReceived
            // 5 State - back to Lobby_NotConnected (should do cleanup verify all disconnected here?)
            //              or Connecting to Lobby and sending Ticket
            //              trigger - Connection Established or Failed
            //                  - then perhaps trying to get a new ticket instead and try again to connect to lobby

                        
               // needs a ticket, access the authentication server
            //    mConnectionStateMachine.Configure(ClientState.NeedTicket)
            //        .SubstateOf(ClientState.Lobby_Connecting)
            //        .Permit (ClientTrigger.GetTicket, ClientState.GettingTicket);

            //    mConnectionStateMachine.Configure(ClientState.HaveTicket)
            //        .SubstateOf(ClientState.Lobby_Connecting);

            // AUTHENTICATION - NOT CONNECTED
            mConnectionStateMachine.Configure(ClientState.Authentication_NotConnected)
                .SubstateOf(ClientState.Root)
                .Permit(ClientTrigger.Authentication_Connect, ClientState.Authentication_Connecting);

            // AUTHENTICATION - CONNECTING
            mConnectionStateMachine.Configure(ClientState.Authentication_Connecting)
                .SubstateOf(ClientState.Root)
                .OnEntry (()=> OnEntry_Authentication_Connecting())
                .Permit(ClientTrigger.Authentication_Connected, ClientState.Authentication_Connected);
                
                
            // AUTHENTICATION - CONNECTED
            mConnectionStateMachine.Configure (ClientState.Authentication_Connected )
                .SubstateOf (ClientState.Root)
                .OnEntry (()=> OnEntry_Authentication_Connected())
                .Permit (ClientTrigger.Authentication_RequestTicket, ClientState.Authentication_WaitingForTicket);

           
            // AUTHENTICATION - WAITING FOR LOBBY TICKET
            mConnectionStateMachine.Configure(ClientState.Authentication_WaitingForTicket)
                .SubstateOf(ClientState.Root)
                .OnEntry(() => OnEntry_Authentication_WaitingForTicket("kgblobby"))            
                .Permit(ClientTrigger.Authentication_TicketReceived, ClientState.Lobby_Connecting);
                
            // AUTHENTICATION - WAITING FOR GAME SERVER TICKET
            mConnectionStateMachine.Configure(ClientState.Authentication_Waiting_For_Game_Ticket)
                .SubstateOf(ClientState.Root)
                .OnEntry(() => OnEntry_Authentication_WaitingForTicket(mGameServerName))
                .Permit(ClientTrigger.Authentication_TicketReceived, ClientState.Game_Connecting);
                           


             // GAME - NOT CONNECTED 
             mConnectionStateMachine.Configure(ClientState.Game_NotConnected)
                 .SubstateOf(ClientState.Root)
                 .OnEntry(() => OnEntry_Game_NotConnected())
                 .PermitDynamic(ClientTrigger.Game_Connect, () => Select_Game_Connect());

             // GAME - CONNECTING
             mConnectionStateMachine.Configure(ClientState.Game_Connecting)
                 .SubstateOf(ClientState.Root)
                 .OnEntryFrom(mTicketReceivedTrigger, ticket => OnEntry_Game_Connecting(ticket))
                 .Permit(ClientTrigger.Game_Disconnect, ClientState.Game_Disconnecting)
                 .Permit(ClientTrigger.Game_Connected, ClientState.Game_Connected);

             // GAME - CONNECTED
             mConnectionStateMachine.Configure(ClientState.Game_Connected)
                 .SubstateOf(ClientState.Root)
                 .OnEntry(() => OnEntry_Game_Connected())
                 .Permit(ClientTrigger.Game_Disconnect, ClientState.Game_NotConnected);
        }

        private void InitializeGameStateMachine()
        {
            mGameStateMachine  = new Stateless.StateMachine<GameState, GameTriggers>(GameState.Connected);
        }

        private void OnEntry_Authentication_Connecting()
        {
            if (mAuthenticationClient == null)
                InitializeAuthenticationClient();

            if (mAuthenticationClient.Status != NetConnectionStatus.Connected)
                AuthenticationServer_Login();  // on connection approval in messageproc we'll trigger authentication connected
            // but... if we have two seperate state machines, how do we know which state
            // machine to make the call against in order to trigger for a lobby or a gameserver ticket?
            // Maybe the key is more likely how do we know for what service we've received a ticket reply?  We could add that
            // property to the ticketreply
            // As for the state machines itself, we can use a single variable mStateMachine and then swap out the machines 
            // stored in that variable so we're always using just a single one.... at this point then probably could just merge
            // into set of unique states where the trigger names are the same but the effect is different because it's applied
            // on seperate states
        }

        private void OnEntry_Authentication_Connected()
        {
            mConnectionStateMachine.Fire(ClientTrigger.Authentication_RequestTicket);
        }

        private void OnEntry_Authentication_WaitingForTicket(string serviceName)
        {
            Debug.WriteLine("InternetClient.OnEntry_Authentication_WaitingForTicket() - Sending Ticket Request.");
            KeyCommon.Messages.RequestTicket request = new KeyCommon.Messages.RequestTicket();
            request.ServiceName = serviceName;
            mAuthenticationClient.SendMessage(request);
        }


        private ClientState Select_Lobby_Connect()
        {
            if (mAuthenticatedLobbyTicket != null) // and not expired
            {
                // we already have the ticket
                return ClientState.Lobby_Connecting;

                // reset our timeout
            }
            else  // we need to download a ticket from authentication server
            {
                if (mAuthenticationClient.Status == NetConnectionStatus.Connected)
                {
                    // already connected we can just issue command to request ticket
                    return ClientState.Authentication_WaitingForTicket;
                }
                else
                { 
                    // we first need to login before we can request ticket as we login

                    return ClientState.Authentication_Connecting;
                }
            }
        }

        private void OnEntry_Lobby_NotConnected()
        {
            if (mAuthenticationClient != null)
                mAuthenticationClient.Disconnect("");

            if (mLobbyClient != null)
                mLobbyClient.Disconnect("");

            // TODO: would we maybe still be connected to a lobby when in a game?  such as for private messenging?
            //if (mGameClient != null)
            //    mGameClient.Disconnect();
        }

        private void OnEntry_Lobby_Connecting(AuthenticatedTicket ticket)
        {
            mAuthenticatedLobbyTicket = ticket;
            LobbyServer_Login();
        }

        private void OnEntry_Lobby_Connected()
        {
            // toggle our "Connect" button to "Disconnect"
            // if the the lobby control is not visible show it

            KeyCommon.Messages.RequestGamesList request = new KeyCommon.Messages.RequestGamesList();

            mLobbyClient.SendMessage(request);

            // allow a Chat panel to be accessible?  Here users can log into a seperate chat server and chat?
            // perhaps also the lobby gets placed as part of the overall Communications tab that is also in the game
            // and this Comms tab gets added (made visible) and eventually when joining a game the other tabs
            // are made visible.  But the Comms tab is shared
            // IM, Chat (local chat for locations in worlds)
            //      - maybe even a MUD style to it.. but rather not.  mainly tho i'd just need to
            //        go ultra low reqts for any type of 3d lounge\cantina\heart of the city\etc
        }

        private ClientState Select_Game_Connect()
        {
            if (mGameTicket != null) // and not expired
            {
                // we already have the ticket
                return ClientState.Game_Connecting;

                // reset our timeout
            }
            else  // we need to download a ticket from authentication server
            {
                if (mAuthenticationClient.Status == NetConnectionStatus.Connected)
                {
                    // already connected we can just issue command to request ticket
                    return ClientState.Authentication_Waiting_For_Game_Ticket;
                }
                else
                {
                    // we first need to login before we can request ticket as we login

                    return ClientState.Authentication_Connecting;
                }
            }
        }


        private void OnEntry_Game_NotConnected()
        {

        }

        private void OnEntry_Game_Connecting(AuthenticatedTicket ticket)
        {
            if (mGameClient == null) InitializeGameClient(mGameServerName );
            mGameTicket = ticket;
            Debug.Assert(mGameTicket != null);
            Debug.WriteLine("InternetClient.OnEntry_Game_Connecting() - Game ticket received");
            GameServer_Login();
        }

        /// <summary>
        /// State OnEntry handler upon successful authentication with game server.  
        /// </summary>
        private void OnEntry_Game_Connected()
        {
            // at this point now we know we need server details so we know which map to load
            // we need ability to cancel and load a second map if the map changes whilst were loading
            // we need to notify the server when we're ready to spawn (server can potentially time us out after x seconds)
            // 
            // we need to switch from our network interface to our game interface
            // we need to send and get approval for our ship selection and crew compliments and such
            // we need to receive a spawn point with our parameters for ship, crew, staring positions and such

            // then just regular updates.... so... now we're going away from connection state to game states so
            // a second state machine is in order perhaps
        }


        private void AuthenticationServer_Login()
        {
            KeyCommon.Messages.AuthenticationLogin login = new KeyCommon.Messages.AuthenticationLogin();
            login.Name = mUserName;
            login.HashedPassword = Authentication.Hash.ComputeHash(mPassword , Authentication.Algorithm.SHA256);
            
            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID);
            login.Write(buffer);

            mAuthenticationClient.Connect(mAuthenticationServerAddress, mAuthenticationServerPort, buffer.Data);
        }

        private void LobbyServer_Login()
        {
            Debug.WriteLine("InternetClient.LobbyServer_Login() - Starting Lobby Client.");

            AuthenticatedLogin authenticatedLogin = new AuthenticatedLogin(mUserName, mAuthenticatedLobbyTicket.SessionKey, mAuthenticatedLobbyTicket.TicketData);
            KeyCommon.Messages.ServiceLogin login = new KeyCommon.Messages.ServiceLogin();

            // to the lobby we include our mAuthenticatedTicket directly in the hail data
            login.AuthLogin = authenticatedLogin.ToBytes();
            login.Name = mUserName;
            login.ServiceName = "kgblobby";

            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID );
            login.Write(buffer);

            mLobbyClient.Connect(mLobbyServiceAddress, mLobbyServicePort, buffer.Data);
        }

        private void GameServer_Login()
        {

            Debug.WriteLine("InternetClient.GameServer_Login() - Starting Game Client.");

            AuthenticatedLogin authenticatedLogin = new AuthenticatedLogin(mUserName, mGameTicket.SessionKey, mGameTicket.TicketData);
            KeyCommon.Messages.ServiceLogin login = new KeyCommon.Messages.ServiceLogin();

            // to the game server we include our mGameTicket directly in the hail data
            login.AuthLogin = authenticatedLogin.ToBytes();
            login.Name = mUserName;
            login.ServiceName = mGameServerName;

            NetBuffer buffer = new NetBuffer();
            buffer.Write((int)login.CommandID);
            login.Write(buffer);           

            mGameClient.Connect(mGameServerAddress, mGameServerPort, buffer.Data);
        }


        public override void Update()
        {
            //mConnectionStateMachine.Fire(ClientTrigger.Tick);

            AuthenticationClientUpdate();
            LobbyClientUpdate();
            GameClientUpdate();
        }

        private void AuthenticationClientUpdate()
        {
            if (mAuthenticationClient == null || mAuthenticationClient.Status == NetConnectionStatus.Disconnected) return;

            mAuthenticationClient.Heartbeat();
            NetMessage[] messages = mAuthenticationClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        mAuthenticationClient.Connect(msg.Buffer.ReadIPEndPoint(), "InternetClient.AuthenticationClientUpdate() - Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Debug.WriteLine("InternetClient.AuthenticationClientUpdate() - Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Debug.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Debug.WriteLine("InternetClient.AuthenticationClientUpdate() - New status: " + mAuthenticationClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // if we're now connected, gab the ticket and then set our flag
                        if (mAuthenticationClient.Status == NetConnectionStatus.Connected)
                        {
                            mConnectionStateMachine.Fire(ClientTrigger.Authentication_Connected);
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Debug.WriteLine("InternetClient.AuthenticationClientUpdate() - Bad Message Received: " + msg.Buffer.ReadString());
                        break;

                    case NetMessageType.Data:
                        // The authentication server sent this data which is probably our LobbyServer ticket reply
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Debug.WriteLine(string.Format("InternetClient.AuthenticationClientUpdate() - Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }

        private void LobbyClientUpdate()
        {
            // note: if testing on local host and the lobby server app is not currently running, we'll get an errror 
            // in the .Heartbeat() method.
            if (mLobbyClient == null || mLobbyClient.Status == NetConnectionStatus.Disconnected) return;

            mLobbyClient.Heartbeat();
            NetMessage[] messages = mLobbyClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        mLobbyClient.Connect(msg.Buffer.ReadIPEndPoint(), "InternetClient.LobbyClientUpdate() - Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Debug.WriteLine("InternetClient.LobbyClientUpdate() - Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Debug.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Debug.WriteLine("InternetClient.LobbyClientUpdate() - New status: " + mLobbyClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // if we're now connected, gab the ticket and then set our flag
                        if (mLobbyClient.Status == NetConnectionStatus.Connected)
                        {
                            mConnectionStateMachine.Fire(ClientTrigger.Lobby_Connected);  
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Debug.WriteLine("InternetClient.LobbyClientUpdate() - Bad Message Received: " + msg.Buffer.ReadString());
                        break;

                    case NetMessageType.Data:
                        // The lobby server sent this data 
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Debug.WriteLine(string.Format("InternetClient.LobbyClientUpdate() - Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }

        private void GameClientUpdate()
        {
            if (mGameClient == null || mGameClient.Status == NetConnectionStatus.Disconnected) return;

            mGameClient.Heartbeat();
            NetMessage[] messages = mGameClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        mGameClient.Connect(msg.Buffer.ReadIPEndPoint(), "InternetClient.GameClientUpdate() - Hail from: " + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Debug.WriteLine("InternetClient.GameClientUpdate() - Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Debug.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Debug.WriteLine("InternetClient.GameClientUpdate() - New status: " + mGameClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // if we're now connected, gab the ticket and then set our flag
                        if (mGameClient.Status == NetConnectionStatus.Connected)
                        {
                            mConnectionStateMachine.Fire(ClientTrigger.Game_Connected);
                        }
                        break;
                    case NetMessageType.BadMessageReceived:
                        Debug.WriteLine("InternetClient.GameClientUpdate() - Bad Message Received: " + msg.Buffer.ReadString());
                        break;

                    case NetMessageType.Data:
                        // The game server sent this data 
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Debug.WriteLine(string.Format("InternetClient.GameClientUpdate() - Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
        }
        

        protected override void ProcessUserMessage(IncomingNetMessage message)
        {
            int command = message.Buffer.ReadInt32();
            NetConnectionBase connection = message.m_sender ;

            //message.m_sender.        
            
            switch (command)
            {
                    // TODO: how do we know for which service this ticket is in reply too?
                case (int)KeyCommon.Messages.Enumerations.TicketReply:
                    AuthenticatedTicket ticket = null;
                    try
                    {
                        
                        Debug.WriteLine("InternetClient.ProcessUserMessage() - TicketReply received.");
                        KeyCommon.Messages.TicketRequestReply ticketRequestReply = new KeyCommon.Messages.TicketRequestReply();
                        ticketRequestReply.Read(message.Buffer);
                        ticket = new AuthenticatedTicket(ticketRequestReply.AuthenticatedTicket, mPassword);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("InternetClient.ProcessUserMessage() - Could not validated received Ticket.");
                        // TODO: might as well have an event here where we send a Authentication_TicketInvalid trigger
                    }
                    if (ticket != null)
                    {
                        Debug.WriteLine("InternetClient.ProcessUserMessage() - TicketReply validated.");
                        mConnectionStateMachine.Fire(mTicketReceivedTrigger,  ticket);
                        //mConnectionStateMachine.Fire <AuthenticatedTicket >(
                    }
                    break;

                default:
                    // I'm thinking of having our SceneManager route these messages to the appropriate Simulation
                    if (UserMessageReceivedHandler != null)
                        UserMessageReceivedHandler.Invoke(command,message.Channel, message.Buffer);
                    else 
                        Debug.WriteLine("InternetClient.ProcessUserMessage() - Unsupported message type '" + command.ToString () + "'.");
    
                    break;
            }
        }
    }
}
