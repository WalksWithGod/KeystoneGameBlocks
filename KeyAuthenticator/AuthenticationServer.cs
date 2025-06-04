using System;
using System.Collections.Generic;
using KeyServices;
using System.Diagnostics;
using System.ServiceProcess;
using Lidgren.Network;
using System.Data;
using KeyCommon.DatabaseEntities;

namespace KeyAuthenticator
{ 
    /// <summary>
    /// The authentication server service usese the Authentication library and the Lidgren library
    /// and authenticates services and clients.  The real Kerberos does use UDP by default
    /// and has the ability to failover to TCP for a client that is having problems.  The benefit of 
    /// UDP is that the server doesnt have to maintain tons of sockets and it can handle many many
    /// requests very fast.  The beauty of kerberos is you don't need an established connection for security anyway.
    /// </summary>
    public class AuthenticationServer : KeyServiceBase
    {
        private NetUDPServer mUDPServer;
        private NetTCPServer mTCPServer;
    
        private Authentication.Server mAuthServer;

        //private Server mAuthenticationHelper;

        private KeyServerCommon.SQLStorageContext mAuthStorageContext;

          
        // ini related vars
        private string _configFile, _configPath;
        public string BASE_PATH = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\..\\..\\..\\";
        private const string COMPANY_NAME = "SciFiCommand";
        private const string PRODUCT_NAME = "KGBAuthenticationServer.config";
        private string CONFIG_DEFAULTS_FILENAME = "KGBAuthenticationServer_defaults.config";
        private Settings.Initialization _ini;
  

        private int mPort;
        private string mAddress;
        private string mServicePassword = "";
        // TODO: should read the service name from file.
        private string mServiceName = ""; // network service name and the windows service name will keep the same


        private const int TICKET_EXPIRATION = 15000;  // 15 seconds.  Our strategy is that authentications are done before connecting to every service
        private long _elapsed;
        private long _tick;
        private bool _isInitialized;
        private bool _isRunning;
        private long _itterations;
        


        /// <summary>
        /// Initialize with a database path to a postgres database
        /// </summary>
        public AuthenticationServer ()
        {
            //Services = new List<Service >();
            // create a new timespan object with a default of 1 seconds delay.
            m_delay = new TimeSpan(0, 0, 0,0, 0);
        }
        
        ~AuthenticationServer()
        {
            mAuthStorageContext.Dispose();
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
            _configPath = Settings.Initialization.CreateApplicationDataFolder(COMPANY_NAME);
            _configFile = Settings.Initialization.CreateApplicationConfigFile(COMPANY_NAME, PRODUCT_NAME, false);
                        
            _tick = Stopwatch.GetTimestamp();

            if (!Initialize()) // load settings
                return;

            mAuthServer = new Authentication.Server();

            // http://msdn.microsoft.com/en-us/magazine/cc163356.aspx
            // set up our network server
           
            string sectionName = "network";

            mServiceName = _ini.settingRead(sectionName, "servicename");
            mServicePassword = _ini.settingRead(sectionName, "servicepassword");
            this.ServiceName = mServiceName;

            NetConfiguration lidgrenConfig = new NetConfiguration(this.ServiceName);
            mAddress = _ini.settingRead(sectionName, "address"); // 0.0.0.0 = IPAddress.Any' and allows to listen from LAN or anywhere
            mPort = _ini.settingReadInteger(sectionName, "port");

            lidgrenConfig.Port = mPort;
            sectionName = "lidgren";
            lidgrenConfig.MaxConnections = _ini.settingReadInteger (sectionName, "maxconnections");
            lidgrenConfig.MaxOutstandingAccepts = _ini.settingReadInteger(sectionName, "maxoutstandingaccepts");
            lidgrenConfig.SendBufferSize = _ini.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = _ini.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.MaximumUserPayloadSize = _ini.settingReadInteger(sectionName, "maxuserpayloadsize");
            lidgrenConfig.UseBufferRecycling = _ini.settingReadBool(sectionName, "enablebufferrecycling");

                        
            mUDPServer = new NetUDPServer(lidgrenConfig);
            mUDPServer.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, _ini.settingReadBool(sectionName, "enablerejectmessages"));
            mUDPServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, _ini.settingReadBool(sectionName, "enableconnectionapproval"));
            mUDPServer.SetMessageTypeEnabled(NetMessageType.DebugMessage, _ini.settingReadBool(sectionName, "enabledebugmessages"));
            mUDPServer.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, _ini.settingReadBool(sectionName, "enableverbosemessages"));

            mTCPServer = new NetTCPServer(lidgrenConfig);
            mTCPServer.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, _ini.settingReadBool(sectionName, "enablerejectmessages"));
            mTCPServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, _ini.settingReadBool(sectionName, "enableconnectionapproval"));
            mTCPServer.SetMessageTypeEnabled(NetMessageType.DebugMessage, _ini.settingReadBool(sectionName, "enabledebugmessages"));
            mTCPServer.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, _ini.settingReadBool(sectionName, "enableverbosemessages"));
            
            sectionName = "authenticationdatabase";
            // create a provider to the Authentication Database "ProjectEvoAuth" for use by our authentication helper
            KeyServerCommon.PostgresProvider provider = new KeyServerCommon.PostgresProvider(
                _ini.settingRead(sectionName, "databasename"),
                _ini.settingRead(sectionName, "address"),
                _ini.settingReadInteger (sectionName, "port"),
                _ini.settingRead(sectionName, "username"),
                _ini.settingRead(sectionName, "password"));

            mAuthStorageContext = new KeyServerCommon.SQLStorageContext(provider);

            
                  
            // set up our cache of user/passwords so we might skip
            // some database calls.
        
            // -check the suspended property on user

            // -should we additionally store a text log for each authentication record?

            //   - similarly for the master server eventually where maybe we want to log which servers are popular and 
            //    having the most players and which game types.  We also may want to deal with more detailed player
            //    logs on our official competitive servers.  we'll see...

           
            //  start listening for connections
            mUDPServer.Initialize();
            // TODO: these cant listen/bind on same port
          //  mTCPServer.Bind(mAddress, mPort);

            // if ((mTCPServer.IsListening))  // <-- assuming we have both a backup tcp server for authentications... id ont really see the point in that tho if our actual game requires UDP
            
            Console.WriteLine("Server initialized.");
            // once base.OnStart() is called, this method never completes until the project ends.
            // i'm not sure why that is since the execute is done on a seperate thread.
            // but i believe before we get to that thread, we do want to set up our completion ports and asych readers
            // and then use the .execute() for house keeping functions
			base.OnStart( args );        
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override int Execute() 
        {
            long currentTick = Stopwatch.GetTimestamp();
            _elapsed = currentTick - _tick;
            _tick = currentTick;
			// for right now we'll just log a message in the Application message log to let us know that
			// our service is working
			//System.Diagnostics.EventLog.WriteEntry(ServiceName, ServiceName + "::Execute()");
   //         Console.WriteLine("{0}::Execute() - Itterations = {1} - Elapsed = {2}", ServiceName, _itterations, _elapsed);

            //_itterations += 1; // with the console line seems we are averaging ~7,000 itterations per second and i suspect 5x that if we remove the console line
		    
            mUDPServer.Heartbeat(); //<-- authentication service doesn't need heartbeat because IOCP style processing?  Maybe
            // not really true considering that we might have internal messages to from the networking library that therefore
            // wont get processed by a completion thread

            // check our frequency... we may be running at a fixed hertz to prevent 100% cpu usage
            // if we're running throttled, check out frequency to prevent 100% cpu usage
            // or does using IOCP to only respond to completions solve this already.  Thats really how a 
            // authentication server should do it anyway.  I think the following parallel for way is for
            // a type of server that needs to also simulate as well as process commands. 
            // thus this authentication server should only use Execute() for housekeeping


		   NetMessage[] messages = mUDPServer.GetMessages();
            if (messages != null && messages.Length > 0)
            {
                //   use threadpool to process the received messages
                foreach (NetMessage msg in messages)
                {
                    bool success = System.Threading.ThreadPool.QueueUserWorkItem(MessageProc, msg);
                    if (!success) throw new Exception("failed to queue user work item");
                }
            }


            //mTCPServer.Heartbeat();
            //messages = mTCPServer.GetMessages();
            //if (messages != null && messages.Length > 0)
            //{
            //    //   and then use a threadpool to process them.  Each thread independantly can call _network.SendMessage(OutgoingMessage);
            //    // http://msdn.microsoft.com/en-us/library/3dasc8as(VS.80).aspx
            //    foreach (NetMessage msg in messages)
            //    {
            //        bool success = System.Threading.ThreadPool.QueueUserWorkItem(MessageProc, msg);
            //        if (!success) throw new Exception("failed to queue user work item");
            //    }
            //}
               
            return 0;
		}

        /// <summary>
        /// Thread safe method that processes incoming messages that were not processed automatically by the Lidgren system\internal message handler.
        /// </summary>
        /// <param name="message"></param>
        private void MessageProc (object message)
        {
            if (message == null) return;
            IncomingNetMessage msg = (IncomingNetMessage)message;
            string ip = "";
            if (msg.m_senderEndPoint != null)
                ip = msg.m_senderEndPoint.Address.ToString();

            try
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
                switch  (msg.Type)
                {
                    case (NetMessageType.ConnectionApproval):
                        KeyCommon.Messages.Enumerations cmd = (KeyCommon.Messages.Enumerations)msg.Buffer.ReadInt32();
                        if (cmd == KeyCommon.Messages.Enumerations.AuthenticationLogin)
                        {
                            Authentication.AuthenticatedTicket reply = null;
                            
                            Lidgren.Network.NetConnectionBase connection = ((IncomingNetMessage)msg).m_sender;
                            //Debug.Assert(ip = connection.RemoteEndpoint.ToString)
                            KeyCommon.Messages.AuthenticationLogin login = new KeyCommon.Messages.AuthenticationLogin();
                            // TODO: there is no error checking whatsoever here on REadByte() and such.  Could easily have null exceptions everywhere
                            login.Read(msg.Buffer);

                            //Debug.Assert(msg.Buffer.Position == msg.Buffer.LengthBits - 7);
                            bool authSucceeded = false;

                            // TODO: add a cache for recently disconnected clients so we can check there first and possibly avoid db retreival
                            // cache not needed for servers which have low frequency of connect/disconnect cycling <-- or maybe just add the cache in the SQLStorageContext for EntityDefinitions
                            User user = (User)mAuthStorageContext.Retreive(typeof(User), "name", DbType.String, login.Name);
                            if (user !=null)
                            {
                                // TODO: the following Authenticate() call should check for duplicates and trying to add dupe to dictionary crashes
                                // should we allow dupes and if so, we'd have to include a address and/or port id ...
                                authSucceeded = Authenticate(user, ip, login.HashedPassword, ref reply);
                                if (authSucceeded )
                                {
                                    byte[] replyData = null;
                                    if (reply != null) replyData = reply.ToBytes();
                                    connection.Tag = user;
                                    mUDPServer.ApproveConnection(NetTime.Now, connection, replyData);
                                }
                            }
                                                        

                            if (!authSucceeded)
                            {
                                Console.WriteLine("Authentication failed.");
                            }
                        }
                        else
                        {
                            // unsupported command.  Log\flag\track? determine if user is attacker/abuser
                            // we could have seperate "alerts" log for these sorts of things
                            // i think we also do want a dedicated log for connect/disconnects/ticketrequests
                            // we should create a \logs\ folder in appdata as well
                            Console.WriteLine("Unsupported command from un-authenticated client.");
                        }
                        break;
                    case (NetMessageType.Data):
                        ProcessUserMessages((IncomingNetMessage)msg);
                        break;   
                
                    case (NetMessageType.StatusChanged):
                        switch (((IncomingNetMessage)msg).m_sender.Status)
                        {
                            case NetConnectionStatus.Disconnected:
                               
                                break;
                            default:
                                Console.WriteLine("Connection " + ((IncomingNetMessage)msg).m_sender.ToString() + " status changed - " + msg.Buffer.ReadString());
                                break;
                        }
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Console.WriteLine(msg.Buffer.ReadString());
                        break;
                    case NetMessageType.BadMessageReceived:
                        Console.WriteLine("Bad Message Received: " + msg.Buffer.ReadString());
                        break;
                    default:
                        Console.WriteLine(string.Format ("Unexpected message type '{0}'", msg.Type.ToString ()));
                        break;
                }
                #region "old"
               
                //            byte[] reply = null;
                //            try
                //            {
                //                // if there is still more data in the buffer, then it must contain the name of another service for which a ticket is requested
                //                if ((msg.Buffer.Position) < msg.Buffer.LengthBits)
                //                {
                //                    string requestedServiceTicket = msg.Buffer.ReadString();
                //                    reply = GetTicket(ip, name, password, requestedServiceTicket);
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine("Error creating ticket. " +  ex.Message);
                //            }

           
                //        if (AuthenticateUser(name, hashedPassword, ref password))
                //        {
                //            Console.WriteLine("User authentication successful.");
                //            // get a ticket that will allow us to authenticate with the master server and get game server listings
                //            byte[] reply = GetTicket(ip, name, password, "KeyMasterSvc");
                //            // set the reply in the connection's outgoing hail data
                //            Debug.Assert(reply != null && reply.Length > 0);
                //            mUDPServer.ApproveConnection(NetTime.Now, ((IncomingNetMessage) msg).m_sender, reply);
                //        }
                //    }
                //    else
                //    {
          
                //else if (msg.Type == NetMessageType.Data)
                //{
                //    // we verify that this user is in fact already authenticated and is allowed to request service tickets by
                //    // verifying their remote endpoint has a connection 
                //    // TODO: the above verify might not be necessary if we maintain a cache of last x authenticated users
                //    // so we can simply check the cache and not track connections or requery the db
                //    NetConnectionBase connection = mUDPServer.GetConnection(((IncomingNetMessage) msg).m_senderEndPoint);
                //    int command = msg.Buffer.ReadInt32();
                //    if (connection != null && connection.Status == NetConnectionStatus.Connected)
                //    {
                //        if (command == COMMAND_REQUEST_TICKET)
                //        {
                //            // besides connection approval, the only message this server handles are ticket requests
                //            // if this is not a ticket request, the command is invalid and we should 
                //            // - log?
                //            // - track in db how many potential abuses this user is doing
                //            // - ban user / ip if this is a pattern
                //            // otherwise it's a valid ticket request packet, read the service name and 
                //            //string serviceName = msg.Buffer.ReadString(); // TODO: get actual service name and remove hardcoded "KeyMasterSvc"
                //            // get the service name and create a ticket
                //            byte[] replyData = GetTicket(ip, name, password, "KeyMasterSvc");
                //            if (replyData != null)
                //                connection.SendMessage(replyData, NetChannel.ReliableUnordered, true);
                //        }
           
                //    }
                //}

#endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("MessageProc() - Exception:" + ex.Message);
                return ;
            }
        }


        private void ProcessUserMessages(IncomingNetMessage msg)
        {
            const int timeout = 60000;

            // TODO: this also handles messages like user's joining/quiting which are internal messages 
            // CommandProc is specifically reserved for incoming user commands/remoteabletypes
            KeyCommon.Messages.Enumerations command = (KeyCommon.Messages.Enumerations)msg.Buffer.ReadInt32();

            switch (command)
            {
                case KeyCommon.Messages.Enumerations.RequestTicket:

                    KeyCommon.Messages.RequestTicket request = new KeyCommon.Messages.RequestTicket();
                    request.Read(msg.Buffer);
                    string serviceName="";
                    string servicePassword="";
                    string applicantName="";
                    string applicantPassword="";

                    serviceName = request.ServiceName;
                    // TODO: perform db query to grab the password
                    servicePassword = "password";
                    if (string.IsNullOrEmpty(servicePassword))
                    {
                        Console.WriteLine("Unsupported service.  Cannot construct ticket.");
                        return;
                    }
                    User user = (User)msg.m_sender.Tag;
                    applicantName = user.Name;
                    applicantPassword = user.Password;

                  
                    Authentication.AuthenticatedTicket ticket = new Authentication.AuthenticatedTicket(
                        msg.m_sender.RemoteEndpoint.Address.ToString(),
                        applicantName, applicantPassword, serviceName, servicePassword, timeout, "");


                    KeyCommon.Messages.TicketRequestReply ticketRequestReply = new KeyCommon.Messages.TicketRequestReply();
                    ticketRequestReply.AuthenticatedTicket  = ticket.ToBytes();
                    msg.m_sender.SendMessage(ticketRequestReply, NetChannel.ReliableInOrder1);
                    break;
                //case KeyCommon.Messages.Enumerations.GetTicket:
                //    // TODO: send a ticket to the user for the service they requested

                //    break;
                default:
                    Console.WriteLine("Unknown message received.");
                    break;
                // we need to dispatch this message to the appropriate game server for hosted game server configuration
                //If (Not USE_DISTRIBUTED_HOSTS) Then
                //    mSimHost.Process(command, msg)

                //Else ' or to the appropriate MSMQ for distributed game server configuration


                //End If
            }
        }


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


        public bool Authenticate( User user, string ip , string hashedPassword, ref Authentication.AuthenticatedTicket reply)
        {   
            bool result = false;
            // note: passed in user is guaranteed to be non null and have a valid user.mID in the database
            KeyServerCommon.AuthenticationRecord history = new KeyServerCommon.AuthenticationRecord(user.ID);
            history.UserID = user.ID;
            history.IPAddress = user.Name;
            history.Succeeded = false;
            history.Date = DateTime.Now;

            if (mAuthServer.Authenticate(user.Password, hashedPassword)) 
            {          
                Console.WriteLine("Authentication granted for '" + user.Name +"'");
                // select * from  'users' where 'name' = user.mName order by oid desc limit 1;
                // Desc = from the bottom up, limit 1 = just one record.
                result = true;
            }

            history.Succeeded = result;
            //mAuthStorageContext.Store(history); // log the authentication in th users_history table
            return result;
        }

        // obsolete now that we've merged services and users into a single user db
        //public bool Authenticate(KeyCommon.Entities.Host host, string ip, string hashedPassword, ref Authentication.AuthenticatedTicket reply)
        //{
        //    if (mAuthServer.Authenticate(host.Password, hashedPassword)) 
        //    {
        //        // note: hosts authentication history not tracked.
        //        Console.WriteLine("Host authentication granted.");
        //        return true;
        //    }
        //    return false;
        //}

        //private Authentication.Reply GetTicket(string userIP, string nickname, string userPassword, string serviceName)
        //{
        //    mAuthServer.CreateTicketForService
        //    // if the user is successfully authenticated, then we create a session key for the service they want to interact with
        //    // we'll return the sessionkey, service ip, service port plus a ticket back to the user.  The first 3 parts
        //    // are for the user and we'll be encrypted in the user's password.  The ticket will be encrypted using the service's password.
        //    // Then when the user attempt to connect to that service, the service will decrypt the ticket and then use the session key
        //    // to decrypt the user's information (the authenticator) and compare it with the information it has in the ticket.
        //    // so indeed, Service.Authenticate()  method is different than Server.Authenticate();

        //    // for now lets here create a ticket so the authenticated user can connect to a master server
        //    // in order to be able to retreive a list of game servers
        //    // tickets are solely for the use of Services and only the Authenticator or TicketGrantingService can create/issue them.

        //    int expiration = int.MaxValue;

        //    string servicePassword = "";
        //    if (!GetPassword("services", serviceName, ref servicePassword))
        //        return null;

        //    Authentication.Reply reply = new Authentication.Reply(userIP, nickname, userPassword, serviceName, servicePassword, expiration, "");

        //    // note that our reply per the notes at the end of http://web.mit.edu/Kerberos/dialogue.html#scene4
        //    // does not encrypt the enitre reply using the user's key but only their copy of the sessionkey.
        //    return reply;
        //}
    }
}
