//using System;
//using Authentication;
//using KeyCommon;
//using KeyServerCommon;
//using Lidgren.Network;
//using System.Collections.Generic;
//using KeyCommon.Entities;
//using KeyCommon.Commands;
//using System.Threading;
//using System.Diagnostics;

//namespace KeyAuthenticator
//{
//    public class LobbyManager
//    {
//        private const int MAX_SIMULTANEOUS_GAMES = 1;
//        private KeyServerCommon.SQLStorageContext mStorageContext;
//        private NetServer mServer;

//        private List<KeyCommon.Entities.Host> mServers;
//        private Dictionary<long, KeyCommon.Entities.User> mUsers;
//        private Dictionary<long, KeyCommon.Entities.Table> mTables;
//        private Dictionary<long, KeyCommon.Entities.Game> mGames;

//        // list of registering tables.  Once a game starts, the table is transferred to the "Games" table in the database

//        private int mNextTableID;


//        public LobbyManager(NetServer server, KeyServerCommon.SQLStorageContext storageContex)
//        {
//            if ((server == null || storageContex == null)) throw new ArgumentNullException();
//            mServer = server;
//            mStorageContext = storageContex;

//            mServers = new List<KeyCommon.Entities.Host>();
//            mUsers = new Dictionary<long, KeyCommon.Entities.User>();
//            mTables = new Dictionary<long, Table>();
//            mLastCreatedTable = Environment.TickCount;

//            mLastClosedTable = Environment.TickCount;
//        }

//        private int GetNextTableID()
//        {
//            return Interlocked.Increment(ref mNextTableID);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="conn"></param>
//        /// <param name="broadcastGroupID">Since server clients and player clients connections are kept in the same array, we can differentiate them by setting different braodcastGroupIDs</param>
//        /// <remarks></remarks>
//        public void AddUser(KeyCommon.Entities.User user, NetConnectionBase conn, int broadcastGroupID)
//        {
//            lock (mUsers)
//            {
//                conn.Tag = user;
//                conn.BroadcastGroupID = broadcastGroupID;
//                user.Tag = conn;
//                mUsers.Add(user.PrimaryKey, user);
//                OnUserJoined(user);
//            }
//        }

//        public void RemoveUser(KeyCommon.Entities.User user)
//        {
//            lock (mUsers)
//            {
//                if (mUsers != null)
//                {
//                    OnUserLeft(user);
//                    mUsers.Remove(user.PrimaryKey);
//                }
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="host"></param>
//        /// <param name="conn"></param>
//        /// <param name="interfaces" >contains a comma seperated values of ip:port that the server is listening on</param>
//        /// <param name="broadcastGroupID">Since server clients and player clients connections are kept in the same array, we can differentiate them by setting different braodcastGroupIDs</param>
//        /// <remarks></remarks>
//        public void AddServer(KeyCommon.Entities.Host host, NetConnectionBase conn, string loginTag, int broadcastGroupID)
//        {
//            lock (mServers)
//            {
//                conn.Tag = host;
//                conn.BroadcastGroupID = broadcastGroupID;
//                host.Port = KeyCommon.Commands.Login.ParsePort(loginTag);
//                string csvInterfaces = KeyCommon.Commands.Login.ParseInterfaces(loginTag);
//                host.EndPoints = Lidgren.Network.IPAddressWithMask.Parse(csvInterfaces);
//                host.Tag = conn;

//                mServers.Add(host);
//                OnServerJoined(host);
//            }
//        }

//        public void RemoveServer(KeyCommon.Entities.Host host)
//        {
//            lock (mServers)
//            {
//                if (mServers != null)
//                {
//                    OnServerLeft(host);
//                    mServers.Remove(host);
//                }
//            }
//        }


//        private void OnServerJoined(KeyCommon.Entities.Host host)
//        {
//            // this and ServerLeft would only be important if we had some other external application that needed to be notified
//            // primarily something like a load balancer perhaps, i don't know.  Maybe in the future we'll have a need for
//            // the file server, a web page updater, or who knows what to be notied and if so, here's where we'd do it.
//        }

//        private void OnServerLeft(KeyCommon.Entities.Host host)
//        {

//        }

//        private void OnUserJoined(KeyCommon.Entities.User user)
//        {
//            KeyCommon.Commands.UserStatusChanged userJoined = new KeyCommon.Commands.UserStatusChanged((int)KeyCommon.Messages.Enumerations.UserJoined);
//            userJoined.UserName = user.mName;
//            userJoined.UserID = user.PrimaryKey;

//            NetConnectionBase connection = (NetConnectionBase)user.Tag;

//            // we endeavor to make sure that we only need to send the entire user list to any particular user ONCE
//            // and from there out, only join/parts of individuals.  Thus we sychronize access to adding/removing of mUsers
//            KeyCommon.Commands.UserList userList = new KeyCommon.Commands.UserList();
//            userList.Scope = KeyCommon.Messages.Enumerations.Scope.Global;
//            // add the user to the top of the list so that the client will be able to get their server assigned UserID first as opposed to last
//            // which is what would happen if we added it in the forloop below
//            userList.AddUser(user.mName, user.PrimaryKey);
//            if (mUsers != null)
//            {
//                if (mUsers.Count > 0)
//                {
//                    foreach (var item in mUsers.Values)
//                    {
//                        // dont add the joined user to the user list twice. They were already added as first
//                        if (item.mName != user.mName)
//                        {
//                            userList.AddUser(item.mName, item.PrimaryKey);
//                        }
//                    }

//                    // notify new user of the existing users
//                    Console.WriteLine("Sending userlist to user " + connection.ToString());
//                    connection.SendMessage(userList, userList.Channel);
//                }
//            }

//            // notify existing users of the new user
//            Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
//            int i = 0;
//            foreach (KeyCommon.Entities.User u in mUsers.Values)
//            {
//                if (object.ReferenceEquals(u.Tag, connection)) continue;
//                connections[i] = (NetConnectionBase)u.Tag;
//                i += 1;
//            }
//            // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//            mServer.Groupcast(userJoined, connections);

//            // notify new user of existing tables
//            KeyCommon.Commands.TableList tableList = new KeyCommon.Commands.TableList();
//            if (mTables != null)
//            {
//                if (mTables.Count > 0)
//                {
//                    foreach (Table t in mTables.Values)
//                    {
//                        tableList.AddTable(t);
//                    }
//                    Console.WriteLine("Sending tablelist to user " + connection.ToString());
//                    connection.SendMessage(tableList, tableList.Channel);
//                }
//            }
//        }

//        private void OnUserLeft(KeyCommon.Entities.User user)
//        {
//            KeyCommon.Commands.UserStatusChanged userLeft = new KeyCommon.Commands.UserStatusChanged((int)KeyCommon.Messages.Enumerations.UserLeft);
//            userLeft.UserName = user.mName;
//            userLeft.UserID = user.PrimaryKey;

//            // unregister from any tables we might be in
//            lock (mTables)
//            {
//                if (mTables != null)
//                {
//                    if (mTables.Count > 0)
//                    {
//                        foreach (var table in mTables.Values)
//                        {
//                            if (table.ContainsUser(user.PrimaryKey))
//                            {
//                                KeyCommon.Commands.TableRegistration unregister = new KeyCommon.Commands.TableRegistration((int)Enumerations.Types.UnRegisterTable);
//                                unregister.TableID = table.PrimaryKey;
//                                unregister.UserID = user.PrimaryKey;
//                                RemoveUserFromTable(unregister);
//                                // actual removal performed AFTER call to RemoveUserFromTable
//                                table.RemoveUser(user.PrimaryKey);
//                            }
//                        }
//                    }
//                }
//            }

//            // notify all users
//            Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
//            int i = 0;
//            foreach (KeyCommon.Entities.User u in mUsers.Values)
//            {
//                connections[i] = (NetConnectionBase)u.Tag;
//                i += 1;
//            }
//            // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//            mServer.Groupcast(userLeft, connections);
//        }

//        private void TableCreated(KeyCommon.Entities.Table table)
//        {
//            lock (mUsers)
//            {
//                // NOTE: If a user has joined at the same time a new table has joined, this table's ID could be broadcasted
//                // to that user an instant prior to the user receiving the full table list which will by then contain this table as well
//                // so the client must be prepared to ignore duplicates.
//                // send the table info to all users
//                KeyCommon.Commands.TableStatusChanged tableCreated = new KeyCommon.Commands.TableStatusChanged();
//                tableCreated.Table = table;
//                tableCreated.Status = KeyCommon.Messages.Enumerations.TableStatus.Created;

//                Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
//                int i = 0;
//                foreach (KeyCommon.Entities.User u in mUsers.Values)
//                {
//                    connections[i] = (NetConnectionBase)u.Tag;
//                    i += 1;
//                }
//                // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//                mServer.Groupcast(tableCreated, connections);
//            }
//        }

//        public void TableClosed(KeyCommon.Entities.Table table)
//        {
//            lock (mUsers)
//            {
//                // notify users that this table is closed so that they can remove it from their list of available tables
//                KeyCommon.Commands.TableStatusChanged tableClosed = new KeyCommon.Commands.TableStatusChanged();
//                tableClosed.Table = table;
//                tableClosed.Status = KeyCommon.Messages.Enumerations.TableStatus.Created;

//                Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
//                int i = 0;
//                foreach (KeyCommon.Entities.User u in mUsers.Values)
//                {
//                    connections[i] = (NetConnectionBase)u.Tag;
//                    i += 1;
//                }
//                // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//                mServer.Groupcast(tableClosed, connections);
//            }
//        }

//        public bool RegisterTable(Lidgren.Network.IncomingNetMessage msg)
//        {

//            KeyCommon.Commands.TableRegistration register = new KeyCommon.Commands.TableRegistration((int)Enumerations.Types.RegisterTable);
//            register.Read(msg.Buffer);

//            // verify this message is from the same user
//            KeyCommon.Entities.User user = (KeyCommon.Entities.User)msg.m_sender.Tag;
//            if (user.PrimaryKey == register.UserID)
//            {
//                if (UserCanJoinTable(user))
//                {
//                    lock (mTables)
//                    {

//                        // verify the register.tableID is valid
//                        if (mTables.ContainsKey(register.TableID))
//                        {

//                            Table table = mTables[register.TableID];
//                            if (!table.IsFull)
//                            {
//                                //   add them to the table and notify users
//                                table.AddUser(user.PrimaryKey, user.mName);

//                                UserList userList = new UserList();
//                                userList.Scope = Enumerations.Scope.Local;

//                                for (int i = 0; i < table.UserIDs.Length; i++)
//                                {
//                                    long id = table.UserIDs[i];
//                                    userList.AddUser(mUsers[id].mName, id);
//                                }

//                                // notify the existing users  that the new user has joined.  We pass -1 because on Register
//                                // because we do want the new user to receive the register request back as a confirmation packet
//                                NetConnectionBase[] connections = GetGroupCastConnections(table.UserIDs);
//                                mServer.Groupcast(register, connections);

//                                // notify the new user of the existing users at the table AFTER the new user has received the Register confirmation packet
//                                msg.m_sender.SendMessage(userList, NetChannel.ReliableUnordered);
//                                return true;
//                            }
//                        }
//                    }

//                }
//            }
//            // if we haven't returned yet, then the user is not allowed to register to this ttable.  
//            // notify user table is full or they are already playing in another game or registered at another table
//            return false;
//        }

//        public bool UnregisterTable(Lidgren.Network.IncomingNetMessage msg)
//        {
//            // TODO: synclock changes or queries against the table

//            KeyCommon.Commands.TableRegistration unregister = new KeyCommon.Commands.TableRegistration((int)Enumerations.Types.UnRegisterTable);
//            unregister.Read(msg.Buffer);
//            lock (mTables)
//            {
//                if (((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey == unregister.UserID)
//                {
//                    return RemoveUserFromTable(unregister);
//                }
//            }
//            return false;
//        }

//        private bool RemoveUserFromTable(KeyCommon.Commands.TableRegistration unregister)
//        {
//            // verify this message is from the same user ' TODO: or we can always use this userID instead.  keep todo til we resolve how we want to handle this in a global way
//            // remove the user from the table
//            Table table = mTables[unregister.TableID];
//            // TODO: last minute check that the table is not already "launching" into game
//            // has the game already started?
//            // if not, unregister them
//            // if yes, too late.  They can still quit the game prior to the end of the first turn without penalty
//            long[] previousIDs = table.UserIDs;
//            table.RemoveUser(unregister.UserID);

//            // notify users at this table that a user has unregistered.  Note that we are echo'ing back to the user who unregistered
//            // and their client will use that to confirm they've been removed and to do internal housekeeping (removing their own id from their local instance of the table)
//            NetConnectionBase[] connections = GetGroupCastConnections(previousIDs);

//            mServer.Groupcast(unregister, connections);
//            return true;
//        }

//        public void SetUserReadyStatus(Lidgren.Network.IncomingNetMessage msg)
//        {

//            //KeyCommon.Commands.ReadyStatusChanged userReadyStatus = new KeyCommon.Commands.ReadyStatusChanged();
//            //userReadyStatus.Read(msg.Buffer);

//            //Table table = mTables[userReadyStatus.TableID];

//            //// verify this message is from the same user 
//            //if (((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey == userReadyStatus.UserID)
//            //{
//            //    // we don't trust the user so we verify this user is really at this table
//            //    if (table.ContainsUser(userReadyStatus.UserID))
//            //    {
//            //        table.SetUserReadyStatus(userReadyStatus.UserID, userReadyStatus.IsReady);
//            //        // notify everyone at the table of the user's change in ready state
//            //        mServer.Groupcast(userReadyStatus, GetGroupCastConnections(table.UserIDs));

//            //        // if everyone at the table has readystatus = true
//            //        if (table.IsReady)
//            //        {
//            //            //we find the server with lowest number of games 
//            //            Host found = default(Host);
//            //            foreach (Host server in mServers)
//            //            {
//            //                if (found == null) found = server;
//            //                if (server.Games == null)
//            //                {
//            //                    found = server;
//            //                    // this server is empty, let's just add the new game here
//            //                    break; // TODO: might not be correct. Was : Exit For
//            //                }
//            //                if (server.Games.Length  < found.Games.Length)
//            //                {
//            //                    found = server;
//            //                }
//            //            }

//            //            // there are no available servers
//            //            // TODO: users should be notified and/or we should retry to find a game server for x interval up to some maxattempts value?
//            //            if ((found == null)) return;

//            //            // generate a new password for a new game  
//            //            string servicePassword = System.Text.Encoding.UTF8.GetString(PasswordGenerator.GenerateRandomPassword(9, 32));

//            //            // store the game info and password in the "games" table of the ProjectEvoGames database
//            //            Game game = new Game(table, servicePassword);
//            //            game.mServerName = found.Name;
//            //            mStorageContext.Store(game);

//            //            KeyCommon.Commands.CreateGame createdGame = new KeyCommon.Commands.CreateGame();
//            //            game.mHost = found.Clone();// we want a copy, not a reference since we have to change some properties
//            //            createdGame.Game = game;

//            //            // send the server the game info including it's password so it can spawn the game and start accepting users
//            //            ((Lidgren.Network.NetConnectionBase)found.Tag).SendMessage(createdGame, NetChannel.ReliableUnordered);
//            //            Console.WriteLine("sending CreateGame to game server...");

//            //            // TODO: here we could/should wait for confirmation from the game server that the game has been created
//            //            // so we know when we can start creating the player objects, storing them
//            //            // and then sending the players the game obj data

//            //            // clear the servicer password because we dont want to send this to clients
//            //            createdGame.Game.mHost.Password = "";
//            //            createdGame.Game.mPassword = "";

//            //            // add a "player" record in the "players" table for each player in that game and set their status as "active"
//            //            foreach (long id in table.UserIDs)
//            //            {
//            //                KeyCommon.Entities.User user = mUsers[id];

//            //                Player player = new Player();
//            //                player.GameID = game.PrimaryKey;
//            //                Debug.Assert(id == user.PrimaryKey);
//            //                player.PrimaryKey = id;
//            //                player.mName = user.mName;

//            //               //player.mFaction = new ActiveFaction();
//            //               // player.mFaction.BaseFaction = new Faction();
//            //                // TODO: the faction name should be set at the Table as users pick which faction they want to be and their leader's name
//            //               // player.mFaction.LeaderName = "CANT_BE_NULL_IN_DB";
//            //                mStorageContext.Store(player);
//            //            }
//            //            // generate a ticket for each client using that password 
//            //            // send the player's the createGame packet AFTER the game and players have been added to the games db
//            //            foreach (long id in table.UserIDs)
//            //            {
//            //                KeyCommon.Entities.User user = mUsers[id];
//            //                Lidgren.Network.NetConnectionBase connection = (Lidgren.Network.NetConnectionBase)user.Tag;

//            //                // filter the game server's available endpoints to just the first ip that the user can connect to

//            //                IPAddressWithMask result = Lidgren.Network.NetUtility.FindFirstReachableEndpoint(connection.RemoteEndpoint.Address, createdGame.Game.mHost.EndPoints);
//            //                createdGame.Game.mHost.EndPoints = new IPAddressWithMask[] { result };

//            //                AuthenticatedTicket reply = new AuthenticatedTicket(connection.RemoteEndpoint.Address.ToString(), user.mName, user.mPassword, game.PrimaryKey.ToString(), servicePassword, int.MaxValue, "");

//            //                //  include the ticket and send to the current user
//            //                createdGame.Data = reply.ToBytes();
//            //                connection.SendMessage(createdGame, NetChannel.ReliableUnordered);
//            //            }
//            //        }
//            //    }
//            //}
//        }

//        private bool UserCanJoinTable(KeyCommon.Entities.User user)
//        {
//            // TODO: verify the user is allowed to register for any game
//            // - user is already at this table
//            // - table is closed/inprogress or similar
//            // - user isnt already playing in too many games
//            // - user isnt already sitting at too many other tables
//            return true;
//        }

//        public void ForwardChat(Lidgren.Network.IncomingNetMessage msg)
//        {
//            KeyCommon.Commands.ChatMessage chat = new KeyCommon.Commands.ChatMessage();
//            chat.Read(msg.Buffer);

//            // verify this message is from the same user 
//            chat.SenderID = ((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey;
//            // reset the UserID not relying on the user to have set this properly

//            Console.WriteLine("Forwarding chat: " + chat.Content);

//            switch (chat.Scope)
//            {
//                case KeyCommon.Messages.Enumerations.Scope.Global:
//                    mServer.Broadcast(chat, new Lidgren.Network.NetConnectionBase[] { msg.m_sender });

//                    break;
//                case KeyCommon.Messages.Enumerations.Scope.Local :
//                    //chat.Tag 'in this context, Tag is the table's primary key in string form
//                    // only broadcast to users in that table 

//                    long tableID = 0;
//                    if (long.TryParse((string)chat.Tag, out tableID))
//                    {
//                        NetConnectionBase[] connections = GetGroupCastConnections(GetFilteredUsers(mTables[tableID].UserIDs, new long[] { chat.SenderID }));
//                        mServer.Groupcast(chat, connections);
//                    }

//                    break;
//                case KeyCommon.Messages.Enumerations.Scope.Private:
//                    break;
//                // tag in this context is the userID
//                // TODO:
//            }
//        }

//        private long[] GetFilteredUsers(long[] userID, long[] excludedUser)
//        {
//            List<long> result = new List<long>();

//            for (int i = 0; i < userID.Length ; i++)
//            {
//                long id = userID[i];
//                bool excluded = false;
//                for (int j = 0; j < excludedUser.Length ; j++)
//                {
//                    if (id == excludedUser[j])
//                    {
//                        excluded = true;
//                        break; // TODO: might not be correct. Was : Exit For
//                    }
//                }

//                if (!excluded)
//                {
//                    result.Add(id);
//                }
//                excluded = false;
//            }

//            return result.ToArray();
//        }

//        private NetConnectionBase[] GetGroupCastConnections(long[] userID)
//        {
//            if (userID == null || userID.Length == 0) return null;

//            NetConnectionBase[] connections = new NetConnectionBase[userID.Length];

//            for (int i = 0; i < userID.Length ; i++)
//            {
//                connections[i] = (NetConnectionBase )( mUsers[userID[i]]).Tag;
//            }

//            return connections;
//        }

        

//        private int mLastCreatedTable;
//        private int mLastClosedTable;
//        private Random mRandom = new Random(Environment.TickCount);
//        private const int REMOVE_TABLE_INTERVAL = 10000;
//        private const int CREATE_TABLE_INTERVAL = 5000;

//        public void Update(long elapsed)
//        {
//            long tick =  Stopwatch.GetTimestamp();
//            // run's maintenance on existing tables, creates new tables when needed, etc

//            // we need either a set of heuristics to determine what type of table to create and/or we need for users to be able to
//            // submit "votes" for games to create.  A way of handling "votes" is basically just going off the "waiting list" length like 
//            // the online poker room model
//            // 
//            // Now what i  really don tlike about server side only game creation is certain aspects of the games settings arent configurable
//            // 

//            //' temp: remove a random empty table
//            //If (tick - mLastClosedTable > REMOVE_TABLE_INTERVAL) Then
//            //    If (mTables.Count > 1) Then
//            //        DebugRemoveTable()
//            //    End If
//            //End If

//            // temp: create a table up to max number of 10 tables
//            //if (tick - mLastCreatedTable > CREATE_TABLE_INTERVAL)
//            //{
//            //    if (mTables.Count < 10)
//            //    {
//            //        DebugCreateTable();
//            //    }

//            //}
//        }

//        private void DebugRemoveTable()
//        {
//            Table removedTable = null;
//            lock (mTables)
//            {

//                var id = mRandom.Next(0, mTables.Count - 1);
//                if (mTables[id].UserIDs.Length > 0)
//                {
//                    removedTable = mTables[id];
//                    throw new NotImplementedException();
//                }
//                //mTables.Removeat(removedTable)
//                mLastClosedTable = Environment.TickCount;
//            }

//            if (removedTable != null)
//            {
//                TableClosed(removedTable);
//            }
//        }

//        private void DebugCreateTable()
//        {
//            mLastCreatedTable = Environment.TickCount;

//            Table newTable = new Table();
//            newTable.PrimaryKey = GetNextTableID();

//            newTable.Name = System.Guid.NewGuid().ToString("N");
//            // just a way to get a unique string.  "N" format returns numbers only with no hypens
//            List<GameConfigParameter> newSettings = new List<GameConfigParameter>();

//            newTable.Settings = newSettings;

//            lock (mTables)
//            {
//                mTables.Add(newTable.PrimaryKey, newTable);

//                mLastCreatedTable = Environment.TickCount;
//            }

//            TableCreated(newTable);
//        }

//    }

//}
