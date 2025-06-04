using System;
using System.Collections.Generic;
using System.Threading;
using Authentication;
using KeyCommon;
using KeyCommon.DatabaseEntities;
using KeyServices;
using Lidgren.Network;

namespace KeyMaster
{
    /// <summary>
    /// Keystone Master Server uses a lidgren netclient to authenticate with a Keystone Authentication Server
    /// and once authenticated, starts a Lidgren NetServer to accept game servers.  GameServers must
    /// use a valid ticket, and then it can post periodic state updates to the masterserver
    /// </summary>
    class MasterServerService : KeyServiceBase
    {
        private NetUDPClient m_authenticationClient;
        private NetUDPServer m_server;
        private NetTCPServer mTCPServer;
        private Algorithm m_hashAlgorithm = Algorithm.SHA256;
        private const string m_serviceName = "KeyMasterSvc";
        private const string m_servicePassword  = "KeyMasterSvc_Passw0rd";
        private const int m_servicePort = 2021;
        private string m_localIP = "127.0.0.1"; //"192.168.1.65";

        private const string m_authenticationServiceName = "KeyAuthenticationSvc";
        private string m_authenticationServerIP = "127.0.0.1"; // "71.112.229.84"  // "192.168.1.64"
        private int m_authenticationPort = 2020;
        private bool m_authenticated = false;
        private const int CMD_ID_AUTHENTICATE_SERVICE = 1;

        private Dictionary < System.Net.IPEndPoint , GameServerInfo> m_gameServers;

      
        /// <summary>
        /// In terms of our Authentication system, the Master server also serves the purpose of granting
        /// tickets for users to be able to login and play any game server that has registered with the master server.
        /// Thus when a user finds a game in the master server list that they want to play, they will under the hood
        /// first request a ticket from the master server to play on that host and then when they receive the ticket
        /// they will be able to initiate a login to that server.
        /// </summary>
        public MasterServerService ()
        {
            m_gameServers = new Dictionary<System.Net.IPEndPoint, GameServerInfo>();
            //Services = new List<Service >();
            this.ServiceName = m_serviceName;
            // create a new timespan object with a default of 1 seconds delay.
            m_delay = new TimeSpan(0, 0, 0,0, 0);

            // create an authentication client with a default configuration
            NetConfiguration config = new NetConfiguration(m_authenticationServiceName);
            m_authenticationClient = new NetUDPClient(config);

            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            m_authenticationClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
            m_authenticationClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, true);

            //NetBuffer buffer = NetBuffer.Create();
            NetBuffer buffer = new NetBuffer();
            buffer.Write(CMD_ID_AUTHENTICATE_SERVICE);
            buffer.Write(this.ServiceName);
            buffer.Write(Hash.ComputeHash(m_servicePassword, m_hashAlgorithm));

            m_authenticationClient.Connect(m_authenticationServerIP, m_authenticationPort, buffer.Data);
        }


        ~MasterServerService()
        {
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

        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        protected override int Execute()
        {
            NetMessage[] messages;

            // pump authentication client until we've fully authenticated
            if (!m_authenticated)
            {
                m_authenticationClient.Heartbeat();
                // we'll use this thread to pump instead of having seperate internal client thread
                messages = m_authenticationClient.GetMessages();

                foreach (NetMessage msg in messages)
                {
                    if (msg.Type == NetMessageType.DebugMessage)
                    {
                        Console.WriteLine("DebugMessage: " + msg.Buffer.ReadString());
                        continue;
                    }

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
                            // if we're now connected, gab the ticket and then set our flag
                            if (m_authenticationClient.Status == NetConnectionStatus.Connected)
                            {
                                // note: once authenticated, a master server does not need any service ticket
                                m_authenticated = true;
                                NetConfiguration config = new NetConfiguration(m_serviceName);
                                config.Port = m_servicePort;
                                m_server = new NetUDPServer(config);
                                m_server.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
                                m_server.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
                                m_server.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
                                m_server.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, true);
                                m_server.Bind();
                            }
                            break;
                        case NetMessageType.Data:
                            // The server sent this data!
                            string data = msg.Buffer.ReadString();
                            Console.WriteLine(data);
                            break;
                    }
                }
            }
            else
            {
                // pump netserver until shutdown
                m_server.Heartbeat();
                messages = m_server.GetMessages();

                if (messages != null && messages.Length > 0)
                {
                    //   and then use a threadpool to process them.  Each thread independantly can call _network.SendMessage(OutgoingMessage);
                    //  
                    // http://msdn.microsoft.com/en-us/library/3dasc8as(VS.80).aspx
                    foreach (NetMessage msg in messages)
                    {
                        bool success = ThreadPool.QueueUserWorkItem(ProcessMessage, msg);
                        if (!success) throw new Exception("failed to queue user work item");
                    }
                }
                
            }
            return 0;
        }

        private void ProcessMessage(object message)
        {
            NetMessage msg = (NetMessage)message;

            if (msg.Type == NetMessageType.DebugMessage)
            {
                Console.WriteLine("Debug: " + msg.Buffer.ReadString());
                return;
            }

            // approval for when Gameservers are trying to register with this Master server using an authenticated login packet
            // created from a ticket and session key received from the authentication server
            if (msg.Type == NetMessageType.ConnectionApproval)
            {
                Console.WriteLine(msg.Buffer.Data.ToString ());
                int command = msg.Buffer.ReadInt32();
                switch (command)
                {
                    case 4: // authenticated login from GameServer to register on this MasterServer 
                        {
                          
                            System.Net.IPEndPoint endpoint = ((IncomingNetMessage) msg).m_senderEndPoint;
                            string ip = endpoint.Address.ToString();

                            try
                            {
                                // grab the user's own reported username which we'll then compare with that contained in the decrypted logon
                                string serverName = msg.Buffer.ReadString();

                                // read the rest of the data
                                byte[] login = msg.Buffer.ReadBytesRemaining();

                                // grab the authenticated login and verify it's validity
                                AuthenticatedLogin decryptedLogin = new AuthenticatedLogin(login, m_servicePassword);

                                // only this serivce can decrypt the authenticator which will contain the user's login name and ip address
                                // which must match the one the user sends, and the ip endpoint from the message
                                if (decryptedLogin.Ticket.IsAuthentic(serverName, ip, m_serviceName))// TODO: verify whether I pass in this ip or the one from decryptedLogin!  Been over 3 years since i first coded this and i'm not too happy with the commenting or the variable naming.. way too confusing trying to remember what this is all about
                                {

                                    // TODO: verify this game server is not already running?  
                                    // m_gameServers.TryGetvalue(endpoint, info);

                                    // connection is approved and we will now accept status updates from this server
                                    m_server.ApproveConnection(NetTime.Now, ((IncomingNetMessage) msg).m_sender, null);
                                    GameServerInfo info = new GameServerInfo();
                                    info.Name = decryptedLogin.UserName;
                                    info.IP = ip;
                                    info.Port = endpoint.Port;
                                    //info.Type = GameType.Demo_1_0;
                                    // TODO: i dont believe im removing game servers for removed connections
                                    m_gameServers.Add(endpoint, info);
                                }
                                else
                                {
                                    // TODO: not authentic.  log this?  I don't believe there's anything else we' do eh?
                                    Console.WriteLine("Authentication failed:  Invalid Ticket" + ip);
                                }
                            }
                            catch
                            {
                                // authentication of ticket failed. log?
                                Console.WriteLine("Authentication failed:  Could not decrypt login" + ip);
                            }
                            break;
                        }
                    case 5: // retreive game server list (can use filters via arguments such as by region) 
                        {
                            // these are attached to approval's but they 
                            break;
                        }
                    default:
                        break;
                }
            }
            else if (msg.Type == NetMessageType.Data)
            {
                // we verify that this user is in fact already authenticated and is allowed to request service tickets by
                // verify their remote endpoint has a connection 
                System.Net.IPEndPoint endpoint = ((IncomingNetMessage) msg).m_senderEndPoint;
                NetConnectionBase connection = m_server.GetConnection(((IncomingNetMessage)msg).m_senderEndPoint);
                if (connection != null && connection.Status == NetConnectionStatus.Connected)
                {
                    // accept status update or other notification from a registered gameserver
                    int command = msg.Buffer.ReadInt32();
                    switch (command)
                    {
                        case (int)KeyCommon.Messages.Enumerations.GameServerInfo: // gameserver is sending a standard status update packet, so update the entry
                            {
                                GameServerInfo info = FindGameServer(endpoint);
                                if (info == null)
                                {
                                    // TODO: log this attempt at sending status update for unauthenticated server?
                                  // return;
                                }
                                // verify that this information is infact coming from the server we think it is?
                                // although what would be the point of putting up fake info using spoofed ip?
                                // DoS is about it...
                                // TODO: so should we really verify authentication everytime we receive a status update?
                                // probably not.  Most important is to just track the request rate and to both throtttle the request rate
                                // (ignore repeated requests if interval is within some delta) and 
                                // ignore the ip altogether if they seem to be abusing

                                // read the data and update the info
                                IRemotableType newInfo = new KeyCommon.DatabaseEntities.GameServerInfo();
                               
                                info = (GameServerInfo)newInfo;
                                info.Read(msg.Buffer);
                                Console.WriteLine("Received status for server '{0}' at {1}:{2}" , info.Name , info.IP, info.Port);
                                break;
                            }
                        default:
                            break;
                    }

                    // - game state update 
                    // - a shutdown notification
                    // 
                }
            }
        }

        private GameServerInfo FindGameServer(System.Net.IPEndPoint endpoint)
        {
            GameServerInfo info;
            if (m_gameServers.TryGetValue( endpoint, out info ))
                return info;

            return null;
        }
    }
}
