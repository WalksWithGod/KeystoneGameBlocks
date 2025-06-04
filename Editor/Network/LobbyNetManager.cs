//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Lidgren.Network ;

//namespace KeyEdit.Network
//{
//    public class LobbyNetManager : INetManager
//    {
//        public delegate void MessageReceivedEvent (Lidgren.Network.IRemotableType command);
//        public delegate void ConsoleMessageEvent (string message); 

//        private Settings.Initialization mINI;
               
//        // Events <-- no direct modification of the GUI.  
//        public ConsoleMessageEvent ConsoleMessageHandler;
//        public MessageReceivedEvent UserJoined;
//        public MessageReceivedEvent UserLeft;
//        // UserListReceived
//        // UserJoined
//        // UserLeft

//        public MessageReceivedEvent TableAdded;
//        public MessageReceivedEvent TableRemoved;
//        // TableStatusChanged
//        // TableListReceived

//        public MessageReceivedEvent ChatReceived;
//        // ChatReceived
//        public MessageReceivedEvent CreateGame;
//        // CreateGame
//        public MessageReceivedEvent RegisterTable;
//        // RegisterTable
//        public MessageReceivedEvent UnRegisterTable;
//        // UnRegisterTable
//        public MessageReceivedEvent ReadyStatusChanged;
//        // ReadyStatusChanged

//        // INetManager local variables
//        private long mUserID;
//        private string mUserName ;
//        private string mPassword;
//        private string mServerAddress;
//        private int mServerPort;
//        private bool mAutoReconnect;  // attempts to reconnect if lost connection
//        private int mMaxRetries;
//        private int mCurrentTries;
//        private bool mIsConnectionLost;
//        private NetState mState;
//        private NetClient mNetClient;


//        //public LobbyManager(Settings.Initialization ini)
//        //{
//        //    if (ini == null) throw new ArgumentNullException();
//        //    mINI = ini;
//        //}

//        public LobbyNetManager (Settings.Initialization  ini)
//        {
//            if (ini == null) throw new ArgumentNullException();
//            mINI = ini;
            
//            mCurrentTries = 0;

//            mServerAddress = mINI.settingRead("network", "lobbyaddress");
//            mServerPort = mINI.settingReadInteger("network", "lobbyport");

//            // apply the configuration to the lidgren NetConfiguration object
//            Lidgren.Network.NetConfiguration lidgrenConfig = new Lidgren.Network.NetConfiguration("KeystoneClient");
//            // we use the name of the system we're connecting to
//            lidgrenConfig.ReceiveBufferSize = mINI.settingReadInteger("network", "receivebuffersize");
//            lidgrenConfig.SendBufferSize = mINI.settingReadInteger("network", "sendbuffersize");
//            lidgrenConfig.UseBufferRecycling = mINI.settingReadBool("network", "enablebufferrecycling");

//            //lidgrenConfig.MaximumTransmissionUnit = 
//            //lidgrenConfig.Port = NETWORK_PORT

//            //lidgrenConfig.ThrottleBytesPerSecond  
//            //lidgrenConfig.TimeoutDelay = 
//            //lidgrenConfig.DisconnectLingerMaxDelay = 
//            //lidgrenConfig.HandshakeAttemptRepeatDelay = ' not used for TCP Client or Server
//            //lidgrenConfig.HandshakeAttemptsMaxCount =   ' not used for TCP Client or Server
//            //lidgrenConfig.AnswerDiscoveryRequests = False ' not used for TCP Client or Server
//            //lidgrenConfig.ResendTimeMultiplier =               ' not used for TCP Client or Server
//            //'lidgrenConfig.MaxAckWithholdTime =        ' not used for TCPClient
//            //'lidgrenConfig.PingFrequency =                 ' not used for TCPClient
//            //lidgrenConfig.MaxConnections                  ' not used for TCPClient

//            mState = new Authenticating();
//            mNetClient = new Lidgren.Network.NetTCPClient(lidgrenConfig);
//        }

//        ~LobbyNetManager()
//        {
 
//        }

//        #region INetManager Members
//        public Lidgren.Network.NetClient  NetClient
//        {
//            get {return mNetClient; }
//        }

//        public long  UserID { get { return mUserID; }}
//        public string  UserName { get { return mUserName; }}
//        public string  Password {get { throw new NotImplementedException(); }}

//        public string  ServerAddress
//        {
//              get {return mServerAddress;}
//              set {mServerAddress = value;}
//        }

//        public int  ServerPort
//        {
//              get {return mServerPort;}
//              set {mServerPort = value; }
//        }

//        public NetState  State
//        {
//              get { return mState;}
//              set {mState = value;}
//        }

//        public bool  AutoReconnect
//        {
//              get {return mAutoReconnect;}
//              set {mAutoReconnect = value;}
//        }

//        public int  MaxRetries
//        {
//              get { return mMaxRetries; }
//              set {mMaxRetries = value;}
//        }

//        public int  CurrentRetryAttempt
//        {
//            get { return  mCurrentTries; }
//        }

//        public void  SendCommand(Lidgren.Network.IRemotableType command)
//        {
//            mNetClient.SendMessage(command);
//        }

//        public void  Connect()
//        {
//            if (mNetClient.Status == NetConnectionStatus.Connected || mNetClient.Status == NetConnectionStatus.Connecting)
//            {
//                // TODO: i should probably have a "Disconnecting" status option
//                // and to then linger for x time to ensure we completely disconnection 
//                // prior to trying to reconnect
//                mNetClient.Disconnect("reconnecting...");
//            }

//            // TODO: need to verify address and then update .config after changes

//            mState = new ConnectingToLobby();
//        }

//        public void  Disconnect()
//        {
//            throw new NotImplementedException();
//        }

//        public void  Update()
//        {
//            mNetClient.Heartbeat();
//            mState.Execute(this);

//            Lidgren.Network.NetMessage[] messages = mNetClient.GetMessages();

//            if (messages != null && messages.Length > 0)
//            {
//                for (int i = 0; i < messages.Length; i++)
//                {
//                    MessageProc(messages[i]);
//                }
//            }
//        }

//        private void MessageProc(NetMessage message)
//        {
//            switch (message.Type)
//            {
//                case NetMessageType.Data:
//                    //  read first byte and determine the command and have the command deserialize itself
//                    // here we could add a loop 
//                    List<IRemotableType>receivedTypes = new List<IRemotableType>();
//                    while (message.Buffer.LengthUnread > 0)
//                    {
//                        int command = message.Buffer.ReadInt32();
//                        IRemotableType newType = KeyCommon.RemotableFactory.GetRemotableType(command, message.Buffer);
//                        if (newType == null)
//                        // something went wrong trying to read that newType
//                        // We can elect to exit this entire MessageProc() and thus
//                        // treat every IRemotableType in this NetMessage as corrupt, or we can 
//                        // skip this one and attempt to read the rest.
//                        {
//                            return; 
//                        }
//                    }

//                    for (int i = 0; i < receivedTypes.Count; i++)
//                        CommandProc(receivedTypes[i]);

//                    break;
                
//                case NetMessageType.DebugMessage:
//                    //ConsoleMessage(message.Buffer.ReadString());
//                    break;
//                case NetMessageType.StatusChanged:
//                    //ConsoleMessage(message.Buffer.ReadString());
//                    break;
//                default:
//                    //ConsoleMessage("Unexpected message type...");
//                    break;
//            }
//        }


//        /// <summary>
//        /// Handles commands from the Lobby/Master server ONLY.  
//        /// </summary>
//        /// <param name="command"></param>
//        /// <param name="message"></param>
//        public void CommandProc(IRemotableType type)
//        {
//            KeyCommon.Messages.Enumerations command = (KeyCommon.Messages.Enumerations) type.Type;
//            switch (command)
//            {
//                // There is a concern that there would be too many users in a lobby to have to send every user
//                // Well the way that card rooms deal with this is to show a list of lobbies and have a max users on each
//                // and then before select a game, users must join a particular lobby.
//                // The good idea here is that the lobby server can be set up to host only a particular set of games
//                // like rated 1v1 lobby, unrated 1v1, PersistantInstantAction, 
//                case KeyCommon.Messages.Enumerations.RetreiveUserList:
//                    KeyCommon.Commands.UserList userList = (KeyCommon.Commands.UserList)type;
//                    if (UserJoined != null) UserJoined(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.ChatMessage:
//                    KeyCommon.Commands.ChatMessage chatMessage = (KeyCommon.Commands.ChatMessage)type;
//                    if (ChatReceived != null) ChatReceived(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.CreateGame:
//                    KeyCommon.Commands.CreateGame createGame = (KeyCommon.Commands.CreateGame)type;
//                    if (CreateGame != null) CreateGame(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.RegisterTable:
//                    //  another user has joined a table we are currently registered to
//                    KeyCommon.Commands.TableRegistration register = (KeyCommon.Commands.TableRegistration)type;
//                    if (RegisterTable != null) RegisterTable(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.UnRegisterTable:
//                    //  a user has left  a table we are currently registerd to
//                    KeyCommon.Commands.TableRegistration unRegister = (KeyCommon.Commands.TableRegistration)type;
//                    if (UnRegisterTable != null) UnRegisterTable(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.UserJoined:
//                    if (UserJoined != null) UserJoined(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.UserLeft:
//                    KeyCommon.Commands.UserStatusChanged userstatus = (KeyCommon.Commands.UserStatusChanged)type;
//                    if (UserLeft != null) UserLeft(type);
//                    break;

//                case KeyCommon.Messages.Enumerations.TableStatusChanged:
//                    KeyCommon.Commands.TableStatusChanged table = (KeyCommon.Commands.TableStatusChanged)type;
                    
//                    if (table.Status == KeyCommon.Messages.Enumerations.TableStatus.Created)
//                    {
//                        if (TableAdded != null) TableAdded(type);
//                    }
//                    else if (table.Status == KeyCommon.Messages.Enumerations.TableStatus.Closed)
//                    {
//                        if (TableRemoved != null) TableRemoved(type);
//                    }
//                   // else if (table.Status == KeyCommon.Messages.Enumerations.TableStatus.ParameterChanged)
//                   // {
//                       // if (TableParameterChanged != null) TableParameterChanged(type);
//                   //}
//                    else
//                    {
//                        Debug.Assert(false, "Unexpected table status...");
//                    }
//                    break;
//                case KeyCommon.Messages.Enumerations.RetreiveTableList:
//                    KeyCommon.Commands.TableList tableList = (KeyCommon.Commands.TableList)type;
//                    // TODO: The following should no longer be possible since TableList type will be removed and
//                    // sending of Entities will be done via SendMessage (myArray, channel, true)
//                    //if (tableList.Tables != null)
//                    //{
//                    //    for (int i = 0; i < tableList.Tables.Count; i++)
//                    //    {
//                    //        listviewGames.Items.Add(tableList.Tables[i].PrimaryKey.ToString(), tableList.Tables[i].Name, 0);
//                    //        mTables.Add(tableList.Tables[i].PrimaryKey, tableList.Tables[i]);
//                    //    }
//                    //}
//                    //ConsoleMessage("table list...");
//                    break;
//                case KeyCommon.Messages.Enumerations.TableReadyStatusChanged:
//                    KeyCommon.Commands.ReadyStatusChanged readystatus = (KeyCommon.Commands.ReadyStatusChanged)type;
//                    if (ReadyStatusChanged != null) ReadyStatusChanged(type);
//                    break;
//                default:
//                    //ConsoleMessage("Unexpected command type...");
//                    break;
//            }
//        }
//         #endregion


//    }
//}
