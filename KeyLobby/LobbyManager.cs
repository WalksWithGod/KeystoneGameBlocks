using System;
using Authentication;
using KeyCommon;
using KeyServerCommon;
using Lidgren.Network;
using System.Collections.Generic;
using KeyCommon.DatabaseEntities;
using KeyCommon.Commands;
using System.Threading;
using System.Diagnostics;
using Game01.GameObjects;

namespace KeyLobby
{
    public class LobbyManager
    {
        
        private const int MAX_SIMULTANEOUS_GAMES = 1;
        private KeyServerCommon.SQLStorageContext mStorageContext;
        private NetServer mServer;

        //private List<KeyCommon.Entities.Host> mServers;
        private Dictionary<string, KeyCommon.DatabaseEntities.User> mUsers;
        private Dictionary<string, Game> mGames;

        private Dictionary<long, Table> mTables;
        // list of registering tables.  Once a game starts, the table is transferred to the "Games" table in the database

        private int mNextTableID;


        public LobbyManager(NetServer server, KeyServerCommon.SQLStorageContext storageContex)
        {
            if ((server == null || storageContex == null)) throw new ArgumentNullException();
            mServer = server;
            mStorageContext = storageContex;

            //mServers = new List<KeyCommon.Entities.Host>();
            mUsers = new Dictionary<string, KeyCommon.DatabaseEntities.User>();
            mGames = new Dictionary<string, Game>();
            mTables = new Dictionary<long, Table>();
            mLastCreatedTable = Environment.TickCount;

            mLastClosedTable = Environment.TickCount;
        }

        private int GetNextTableID()
        {
            return Interlocked.Increment(ref mNextTableID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="conn"></param>
        /// <param name="broadcastGroupID">Since server clients and player clients connections are kept in the same array, we can differentiate them by setting different braodcastGroupIDs</param>
        /// <remarks></remarks>
        public void AddUser(KeyCommon.DatabaseEntities.User user, NetConnectionBase conn, int broadcastGroupID)
        {
            lock (mUsers)
            {
                conn.Tag = user;
                conn.BroadcastGroupID = broadcastGroupID;
                user.Tag = conn;
                mUsers.Add(user.Name, user);
                OnUserJoined(user);
            }
        }

        public void RemoveUser(KeyCommon.DatabaseEntities.User user)
        {
            lock (mUsers)
            {
                if (mUsers != null)
                {
                    OnUserLeft(user);
                    mUsers.Remove(user.Name);
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="host"></param>
        ///// <param name="conn"></param>
        ///// <param name="interfaces" >contains a comma seperated values of ip:port that the server is listening on</param>
        ///// <param name="broadcastGroupID">Since server clients and player clients connections are kept in the same array, we can differentiate them by setting different braodcastGroupIDs</param>
        ///// <remarks></remarks>
        //public void AddServer(KeyCommon.Entities.Host host, NetConnectionBase conn, string loginTag, int broadcastGroupID)
        //{
        //    lock (mServers)
        //    {
        //        conn.Tag = host;
        //        conn.BroadcastGroupID = broadcastGroupID;
        //        host.Port = KeyCommon.Commands.Login.ParsePort(loginTag);
        //        string csvInterfaces = KeyCommon.Commands.Login.ParseInterfaces(loginTag);
        //        host.EndPoints = Lidgren.Network.IPAddressWithMask.Parse(csvInterfaces);
        //        host.Tag = conn;

        //        mServers.Add(host);
        //        OnServerJoined(host);
        //    }
        //}


        //public void RemoveServer(KeyCommon.Entities.Host host)
        //{
        //    lock (mServers)
        //    {
        //        if (mServers != null)
        //        {
        //            OnServerLeft(host);
        //            mServers.Remove(host);
        //        }
        //    }
        //}


        //private void OnServerJoined(KeyCommon.Entities.Host host)
        //{
        //    // this and ServerLeft would only be important if we had some other external application that needed to be notified
        //    // primarily something like a load balancer perhaps, i don't know.  Maybe in the future we'll have a need for
        //    // the file server, a web page updater, or who knows what to be notied and if so, here's where we'd do it.
        //}

        //private void OnServerLeft(KeyCommon.Entities.Host host)
        //{

        //}

        private void OnUserJoined(KeyCommon.DatabaseEntities.User user)
        {
            KeyCommon.Messages.UserStatusChanged userJoined = new KeyCommon.Messages.UserStatusChanged((int)KeyCommon.Messages.Enumerations.UserJoined);
            userJoined.UserName = user.Name;
            userJoined.UserID = user.ID;

            NetConnectionBase connection = (NetConnectionBase)user.Tag;

            // we endeavor to make sure that we only need to send the entire user list to any particular user ONCE
            // and from there out, only join/parts of individuals.  Thus we sychronize access to adding/removing of mUsers
            KeyCommon.Messages.UserList userList = new KeyCommon.Messages.UserList();
            userList.Scope = KeyCommon.Scope.Global;
            // add the user to the top of the list so that the client will be able to get their server assigned UserID first as opposed to last
            // which is what would happen if we added it in the forloop below
            userList.AddUser(user.Name, user.ID);
            if (mUsers != null)
            {
                if (mUsers.Count > 0)
                {
                    foreach (var item in mUsers.Values)
                    {
                        // dont add the joined user to the user list twice. They were already added as first
                        if (item.Name != user.Name)
                        {
                            //userList.AddUser(item.mName, item.PrimaryKey);
                        }
                    }

                    // TODO: no more do we automatically send this list of existing users.  Now client must ask for it
                    //         To ensure client does not dos us by constantly asking, we'll track certain requests by the user and 
                    //          the frequency and if they abuse, we can disconnect and possibly suspend access for that username via
                    //          a cache of banned ips in lidgren.
                    //          Also note that since our "user" list can contain users as well as services, it makes no sense to automatically
                    //          send userlists to some types of users.  
                    // notify new user of the existing users
                    //Console.WriteLine("Sending userlist to user " + connection.ToString());
                    //connection.SendMessage(userList, userList.Channel);
                }
            }

            //// notify existing users of the new user
            //Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
            //int i = 0;
            //foreach (KeyCommon.Entities.User u in mUsers.Values)
            //{
            //    if (object.ReferenceEquals(u.Tag, connection)) continue;
            //    connections[i] = (NetConnectionBase)u.Tag;
            //    i += 1;
            //}
            //// see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //mServer.Groupcast(userJoined, connections);

            //// notify new user of existing tables
            //KeyCommon.Commands.TableList tableList = new KeyCommon.Commands.TableList();
            //if (mTables != null)
            //{
            //    if (mTables.Count > 0)
            //    {
            //        foreach (Table t in mTables.Values)
            //        {
            //            tableList.AddTable(t);
            //        }
            //        Console.WriteLine("Sending tablelist to user " + connection.ToString());
            //        connection.SendMessage(tableList, tableList.Channel);
            //    }
            //}
        }

        private void OnUserLeft(KeyCommon.DatabaseEntities.User user)
        {
            KeyCommon.Messages.UserStatusChanged userLeft = new KeyCommon.Messages.UserStatusChanged((int)KeyCommon.Messages.Enumerations.UserLeft);
            userLeft.UserName = user.Name;
            userLeft.UserID = user.ID;

            // unregister from any tables we might be in
            lock (mTables)
            {
                if (mTables != null)
                {
                    if (mTables.Count > 0)
                    {
                        //foreach (var table in mTables.Values)
                        //{
                        //    if (table.ContainsUser(user.PrimaryKey))
                        //    {
                        //        // TODO: if the user is the owner of the table, then the entire table must be closed
                        //        //          because that user was supposed to host... unless we transfer the table ownership...
                        //        KeyCommon.Commands.LeaveTable leave = new KeyCommon.Commands.LeaveTable((int)Enumerations.Types.UnRegisterTable);
                        //        leave.TableID = table.PrimaryKey;
                        //        leave.UserID = user.PrimaryKey;
                        //        RemoveUserFromTable(leave);
                        //        // actual removal performed AFTER call to RemoveUserFromTable
                        //        table.RemoveUser(user.PrimaryKey);
                        //    }
                        //}
                    }
                }
            }

            // TODO: if this user is hosting a game, then the loss of thier connection should NOT necessarily
            // result in the game being terminated IF that game is a persistant game.  However
            // lobby mostly just uses UDP so having lots of connections shouldn't be a problem and those servers should 
            // be sending udp game updates so we can provide users with updated info to browse.hrm...


            // TODO: we no longer notify all users, we'll only ever notify people at the same registration window or
            // people on friend lists.
            //// notify all users
            //Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
            //int i = 0;
            //foreach (KeyCommon.Entities.User u in mUsers.Values)
            //{
            //    connections[i] = (NetConnectionBase)u.Tag;
            //    i += 1;
            //}
            //// see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //mServer.Groupcast(userLeft, connections);
        }


        public void RegisterGame(Lidgren.Network.IncomingNetMessage msg)
        {
            KeyCommon.Messages.RegisterGame register = new KeyCommon.Messages.RegisterGame();
            register.Read (msg.Buffer);
            
            //GameConfig is really a "Game" entity i havent renamed yet and havent added the ConfigParameters array property to
            Game game = register.Game;

            bool success = false;
            
            // verify only one registered game per user is allowed (our own servers which can host multiple games on a single
            // instance of the gameserver.exe simply uses multiple registration keys so each game has a unique one to get around
            // what would otherwise be a limitation, but how does that work?  It means those game servers do need
            // seperate connections to the server yes?  I would say so...  if that's the case, then it is ok to tie
            // connections here to specific games. 
            KeyCommon.DatabaseEntities.User user = (KeyCommon.DatabaseEntities.User)msg.m_sender.Tag;

            if (user != null)
            {
                // verify this request to host a game is specifying the same userID as this connection
                if (user.Name  == game.Host.Name)
                {
                    lock (mGames)
                    {
                        // verify this user is NOT already hosting a game
                        if (!mGames.ContainsKey(user.Name))
                        {
                            // verify the internet IP of the primary game.Host.EndPoint matches the sender's IP
                            // We care because we don't want some hacker to write  a program that can authorize games for 
                            // all sorts of IP's that don't match his own.  This way spoofing his IP wont work because users
                            // will only be able to connect to a gameserver on that IP.
                            //if (msg.m_sender.RemoteEndpoint.Address.ToString ().Equals ( game.Host.Name))
                            //{

                                int primaryKey = 0; 
                                 
                                // TODO: add the game to the Games database grabbing the primary key as well
                                // since the game is hosted by a user account, we can search for games in the DB by that field
                                // to see if it already exists as a persistant game.... else if it's an old completed or expired 
                                // game we will then add a new one
                                //mStorageContext.Store(game);
                                //primaryKey = game.PrimaryKey; // after storing the game, the primary key is set in the entity

                                //game.ID = primaryKey;

                                // generate a session key for this game.  This key will be used by the game to 
                                // authenticate user connections
                                //string sessionKey = System.Text.Encoding.UTF8.GetString(PasswordGenerator.GenerateRandomPassword(9, 32));
                                //game.SessionKey = sessionKey;

                                // the above logic is faulty, i cant generate the session here.  Maybe hte user
                                // should contact the authentication server directly to get a ticket for a particular game server
                                // just as it did when it requested a ticket for the lobby.  Then we can wait for the game
                                // server to notify us when the user has joined the game and we can update our db.
                                // So here perhaps, our "JoinGame" is authorized and we only need to verify that the user
                                // is not playing in too many games or something.  So lobby's responsibility is limited to that
                                // and has nothing to do with mediating authentication?  And possibly, the only way we regulate that
                            // is through communication with the host by telling it "hey, this user is already playing in x games"

                            // BUT BUT BUT, how then if in the case of setting up a match at a table, do you limit the joiners
                            // of the game by only those players who were at the table?  
                            // i think the game itself is notified of the player that is going to be joining as part of the
                            // agreed upon final configuration and only those players in that list can join.

                                // we can finally add the game to our list of games
                                mGames.Add(user.Name , game);
                                OnGameRegistered(game);
                                success = true;
                                
                            //}
                        }
                        else  // this game already exists.  Maybe it's completed or timed out and needs to be removed?
                        {
                            // TODO: add checks to see if this game is stale and should be removed and this new request put in it's place

                        }
                    }
                }
            }

            KeyCommon.Messages.MessageBase response;
            // TODO: maybe a generic CommandSucess  with a command sequenceID used
            if (success) // send reply success along with the friendly name of the game and the database id being used
            {
                response = new KeyCommon.Messages.CommandSuccess(register.Type);
                Console.WriteLine("Game '" + game.mName + "' registered successfully.");
            }
            else   // send reply of access denied
            {
                response = new KeyCommon.Messages.CommandFail(register.Type);
                Console.WriteLine("Game '" + game.mName + "' registration failed.");
            }

            msg.m_sender.SendMessage(response, NetChannel.ReliableUnordered);
        }

        private void OnGameRegistered(Game game)
        {
            // TODO: for now im using just a GameConfig but eventually this should be just renamed a "Game"
            // that contains a Config, Parameters, Host info, and the ability to extract a summary vai game.GetSummary()
            // what do we do here?
        }

        /// <summary>
        /// A temporary simplistic method to send the game summary to a client so that
        /// we can connect to our furball server
        /// </summary>
        /// <param name="msg"></param>
        public void QueryGames(Lidgren.Network.IncomingNetMessage msg)
        {
            // TODO: eventually our query must have various filters and such
            KeyCommon.Messages.RequestGamesList query = new KeyCommon.Messages.RequestGamesList();
            query.Read(msg.Buffer);

            // compile a query response
            KeyCommon.Messages.GameSummaryList list = new KeyCommon.Messages.GameSummaryList();

            foreach (Game g in mGames.Values)
                list.AddSummary(g.GetSummary ());


            msg.m_sender.SendMessage(list, NetChannel.ReliableUnordered);
        }

        /// <summary>
        /// The player does not send a "JoinGame" message to the Lobby.  Rather the game host 
        /// sends the notification here to the lobby.
        /// </summary>
        /// <param name="msg"></param>
        public void JoinGame(Lidgren.Network.IncomingNetMessage msg)
        {
            
            KeyCommon.Messages.JoinGame join = new KeyCommon.Messages.JoinGame();
            join.Read(msg.Buffer);

            string key = join.HostName;

            // get the user based on the message sender

            // TODO: is user hosting a game?  If so, then can they only play in the game they are hosting?


            // TODO: eventually we should use database but now we'll just use the dictionary
            // verify user is not already in another game

            // verify game exists
            if (mGames.ContainsKey(key))
            {
                Game game = mGames[key];
                // lock this game and verify game is not full and that user is not already in it and trying to join twice
                //if (!game.IsFull() && !game.ContainsUser (username))
                //{
                    // with thread lock on game still in place, reserve this spot on that game for the user
                    // and wait til we get a ticket from the authentication server that will allow the user to 
                    // join that specific game.

                    // put the user into a pending ticket received list 

                    // the reason we need to get a ticket from the authentication server is that the authenticator
                    // for this ticket will only be accessible by the gameserver and furthermore, this lobby will not have to
                    // get the password for that server which would mean that the password is sent encrypted using our
                    // own password.  Well, why not do it that way?
                //}
            }

            //Table table = mTables[userReadyStatus.TableID];

            //// verify this message is from the same user 
            //if (((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey == userReadyStatus.UserID)
            //{
            //    // verify the game they wish to join exists 
            //    Game game = mStorageContext.Retreive ();

            //    // we don't trust the user so we verify this user is not aleady in a game
            //    if (userNotAlreadyInGame)
            //    {
            
            //        // if game is not full
            //        if (gameIsNotFull(game))
            //        {
            
            //            // contact the authentication server and get a ticket for this user to join that specific server
            //            // TODO: 
            //            string servicePassword = System.Text.Encoding.UTF8.GetString(PasswordGenerator.GenerateRandomPassword(9, 32));


            //            // send the server the game info including it's password so it can spawn the game and start accepting users
            //            ((Lidgren.Network.NetConnectionBase)found.Tag).SendMessage(createdGame, NetChannel.ReliableUnordered);
            //            Console.WriteLine("sending CreateGame to game server...");

            //            // TODO: here we could/should wait for confirmation from the game server that the game has been created
            //            // so we know when we can start creating the player objects, storing them
            //            // and then sending the players the game obj data

            //            // clear the servicer password because we dont want to send this to clients
            //            createdGame.Game.mHost.Password = "";
            //            createdGame.Game.mPassword = "";

            //            // add a "player" record in the "players" table for this player in that game and set their status as "active"

            //                KeyCommon.Entities.User user = mUsers[id];

            //                Player player = new Player();
            //                player.GameID = game.PrimaryKey;
            //                Debug.Assert(id == user.PrimaryKey);
            //                player.PrimaryKey = id;
            //                player.mName = user.mName;
            //                mStorageContext.Store(player);
            //            
            //            // generate a ticket for each client using that password 
            //            // send the player's the createGame packet AFTER the game and players have been added to the games db
            //            foreach (long id in table.UserIDs)
            //            {
            //                KeyCommon.Entities.User user = mUsers[id];
            //                Lidgren.Network.NetConnectionBase connection = (Lidgren.Network.NetConnectionBase)user.Tag;

            //                // filter the game server's available endpoints to just the first ip that the user can connect to

            //                IPAddressWithMask result = Lidgren.Network.NetUtility.FindFirstReachableEndpoint(connection.RemoteEndpoint.Address, createdGame.Game.mHost.EndPoints);
            //                createdGame.Game.mHost.EndPoints = new IPAddressWithMask[] { result };

            //                AuthenticatedTicket reply = new AuthenticatedTicket(connection.RemoteEndpoint.Address.ToString(), user.mName, user.mPassword, game.PrimaryKey.ToString(), servicePassword, int.MaxValue, "");

            //                //  include the ticket and send to the current user
            //                createdGame.Data = reply.ToBytes();
            //                connection.SendMessage(createdGame, NetChannel.ReliableUnordered);
            //            }
            //        }
            //    }
            //}

        }

        public void JoinGameTicketReceived()
        {
 
        }

        private void TableCreated(Table table)
        {
            //lock (mUsers)
            //{
            //    // NOTE: If a user has joined at the same time a new table has joined, this table's ID could be broadcasted
            //    // to that user an instant prior to the user receiving the full table list which will by then contain this table as well
            //    // so the client must be prepared to ignore duplicates.
            //    // send the table info to all users
            //    Game01.Messages.TableStatusChanged tableCreated = new Game01.Messages.TableStatusChanged();
            //    tableCreated.Table = table;
            //    tableCreated.Status = Game01.GameObjects.TableStatus.Created;

            //    Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
            //    int i = 0;
            //    foreach (var u in mUsers.Values)
            //    {
            //        connections[i] = (NetConnectionBase)u.Tag;
            //        i += 1;
            //    }
            //    // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //    mServer.Groupcast(tableCreated, connections);
            //}
        }

        public void TableClosed(Table table)
        {
            //lock (mUsers)
            //{
            //    // notify users that this table is closed so that they can remove it from their list of available tables
            //    Game01.Messages.TableStatusChanged tableClosed = new Game01.Messages.TableStatusChanged();
            //    tableClosed.Table = table;
            //    tableClosed.Status = Game01.GameObjects.TableStatus.Created;

            //    Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mUsers.Count];
            //    int i = 0;
            //    foreach (var u in mUsers.Values)
            //    {
            //        connections[i] = (NetConnectionBase)u.Tag;
            //        i += 1;
            //    }
            //    // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //    mServer.Groupcast(tableClosed, connections);
            //}
        }

        public bool RegisterTable(Lidgren.Network.IncomingNetMessage msg)
        {

            //KeyCommon.Commands.TableRegistration register = new KeyCommon.Commands.TableRegistration((int)Enumerations.Types.RegisterTable);
            //register.Read(msg.Buffer);

            //// verify this message is from the same user
            //KeyCommon.Entities.User user = (KeyCommon.Entities.User)msg.m_sender.Tag;
            //if (user.PrimaryKey == register.UserID)
            //{
            //    if (UserCanJoinTable(user))
            //    {
            //        lock (mTables)
            //        {

            //            // verify the register.tableID is valid
            //            if (mTables.ContainsKey(register.TableID))
            //            {

            //                Table table = mTables[register.TableID];
            //                if (!table.IsFull)
            //                {
            //                    //   add them to the table and notify users
            //                    table.AddUser(user.PrimaryKey, user.mName);

            //                    UserList userList = new UserList();
            //                    userList.Scope = Enumerations.Scope.Local;

            //                    for (int i = 0; i < table.UserIDs.Length; i++)
            //                    {
            //                        long id = table.UserIDs[i];
            //                        userList.AddUser(mUsers[id].mName, id);
            //                    }

            //                    // notify the existing users  that the new user has joined.  We pass -1 because on Register
            //                    // because we do want the new user to receive the register request back as a confirmation packet
            //                    NetConnectionBase[] connections = GetGroupCastConnections(table.UserIDs);
            //                    mServer.Groupcast(register, connections);

            //                    // notify the new user of the existing users at the table AFTER the new user has received the Register confirmation packet
            //                    msg.m_sender.SendMessage(userList, NetChannel.ReliableUnordered);
            //                    return true;
            //                }
            //            }
            //        }

            //    }
            //}
            // if we haven't returned yet, then the user is not allowed to register to this ttable.  
            // notify user table is full or they are already playing in another game or registered at another table
            return false;
        }

        public bool UnregisterTable(Lidgren.Network.IncomingNetMessage msg)
        {
            // TODO: synclock changes or queries against the table

            //KeyCommon.Commands.TableRegistration unregister = new KeyCommon.Commands.TableRegistration((int)Enumerations.Types.UnRegisterTable);
            //unregister.Read(msg.Buffer);
            //lock (mTables)
            //{
            //    if (((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey == unregister.UserID)
            //    {
            //        return RemoveUserFromTable(unregister);
            //    }
            //}
            return false;
        }

        private bool RemoveUserFromTable(KeyCommon.Messages.LeaveTable leave)
        {
            //// verify this message is from the same user ' TODO: or we can always use this userID instead.  keep todo til we resolve how we want to handle this in a global way
            //// remove the user from the table
            //Table table = mTables[leave.TableID];
            //// TODO: last minute check that the table is not already "launching" into game
            //// has the game already started?
            //// if not, unregister them
            //// if yes, too late.  They can still quit the game prior to the end of the first turn without penalty
            //long[] previousIDs = leave.UserIDs;
            //table.RemoveUser(leave.UserID);

            //// notify users at this table that a user has unregistered.  Note that we are echo'ing back to the user who unregistered
            //// and their client will use that to confirm they've been removed and to do internal housekeeping (removing their own id from their local instance of the table)
            //NetConnectionBase[] connections = GetGroupCastConnections(previousIDs);

            //mServer.Groupcast(leave, connections);
            return true;
        }

        public void SetUserReadyStatus(Lidgren.Network.IncomingNetMessage msg)
        {
            //KeyCommon.Commands.ReadyStatusChanged userReadyStatus = new KeyCommon.Commands.ReadyStatusChanged();
            //userReadyStatus.Read(msg.Buffer);

            //Table table = mTables[userReadyStatus.TableID];

            //// verify this message is from the same user 
            //if (((KeyCommon.Entities.User)msg.m_sender.Tag).PrimaryKey == userReadyStatus.UserID)
            //{
            //    // we don't trust the user so we verify this user is really at this table
            //    if (table.ContainsUser(userReadyStatus.UserID))
            //    {
            //        table.SetUserReadyStatus(userReadyStatus.UserID, userReadyStatus.IsReady);
            //        // notify everyone at the table of the user's change in ready state
            //        mServer.Groupcast(userReadyStatus, GetGroupCastConnections(table.UserIDs));

            //        // if everyone at the table has readystatus = true
            //        if (table.IsReady)
            //        {
            //            //we find the server with lowest number of games 
            //            Host found = default(Host);
            //            foreach (Host server in mServers)
            //            {
            //                if (found == null) found = server;
            //                if (server.Games == null)
            //                {
            //                    found = server;
            //                    // this server is empty, let's just add the new game here
            //                    break; // TODO: might not be correct. Was : Exit For
            //                }
            //                if (server.Games.Length  < found.Games.Length)
            //                {
            //                    found = server;
            //                }
            //            }

            //            // there are no available servers
            //            // TODO: users should be notified and/or we should retry to find a game server for x interval up to some maxattempts value?
            //            if ((found == null)) return;

            //            // generate a new password for the new game that users in the lobby wanting to connect to will
            //            // have to use to authenticate with the game server.  This ensures that only properly registered users
            //            // who are using the lobby can connect to a server that is registered with the lobby.
            //            string servicePassword = System.Text.Encoding.UTF8.GetString(PasswordGenerator.GenerateRandomPassword(9, 32));

            //            // store the game info and password in the "games" table of the ProjectEvoGames database
            //            Game game = new Game(table, servicePassword);
            //            game.mServerName = found.Name;
            //            mStorageContext.Store(game);

            //            KeyCommon.Commands.CreateGame createdGame = new KeyCommon.Commands.CreateGame();
            //            game.mHost = found.Clone();// we want a copy, not a reference since we have to change some properties
            //            createdGame.Game = game;

            //            // send the server the game info including it's password so it can spawn the game and start accepting users
            //            ((Lidgren.Network.NetConnectionBase)found.Tag).SendMessage(createdGame, NetChannel.ReliableUnordered);
            //            Console.WriteLine("sending CreateGame to game server...");

            //            // TODO: here we could/should wait for confirmation from the game server that the game has been created
            //            // so we know when we can start creating the player objects, storing them
            //            // and then sending the players the game obj data

            //            // clear the servicer password because we dont want to send this to clients
            //            createdGame.Game.mHost.Password = "";
            //            createdGame.Game.mPassword = "";

            //            // add a "player" record in the "players" table for each player in that game and set their status as "active"
            //            foreach (long id in table.UserIDs)
            //            {
            //                KeyCommon.Entities.User user = mUsers[id];

            //                Player player = new Player();
            //                player.GameID = game.PrimaryKey;
            //                Debug.Assert(id == user.PrimaryKey);
            //                player.PrimaryKey = id;
            //                player.mName = user.mName;

            //               //player.mFaction = new ActiveFaction();
            //               // player.mFaction.BaseFaction = new Faction();
            //                // TODO: the faction name should be set at the Table as users pick which faction they want to be and their leader's name
            //               // player.mFaction.LeaderName = "CANT_BE_NULL_IN_DB";
            //                mStorageContext.Store(player);
            //            }
            //            // generate a ticket for each client using that password 
            //            // send the player's the createGame packet AFTER the game and players have been added to the games db
            //            foreach (long id in table.UserIDs)
            //            {
            //                KeyCommon.Entities.User user = mUsers[id];
            //                Lidgren.Network.NetConnectionBase connection = (Lidgren.Network.NetConnectionBase)user.Tag;

            //                // filter the game server's available endpoints to just the first ip that the user can connect to

            //                IPAddressWithMask result = Lidgren.Network.NetUtility.FindFirstReachableEndpoint(connection.RemoteEndpoint.Address, createdGame.Game.mHost.EndPoints);
            //                createdGame.Game.mHost.EndPoints = new IPAddressWithMask[] { result };

            //                AuthenticatedTicket reply = new AuthenticatedTicket(connection.RemoteEndpoint.Address.ToString(), user.mName, user.mPassword, game.PrimaryKey.ToString(), servicePassword, int.MaxValue, "");

            //                //  include the ticket and send to the current user
            //                createdGame.Data = reply.ToBytes();
            //                connection.SendMessage(createdGame, NetChannel.ReliableUnordered);
            //            }
            //        }
            //    }
            //}
        }

        private bool UserCanJoinTable(KeyCommon.DatabaseEntities.User user)
        {
            // TODO: verify the user is allowed to register for any game
            // - user is already at this table
            // - table is closed/inprogress or similar
            // - user isnt already playing in too many games
            // - user isnt already sitting at too many other tables
            return true;
        }



        public void ForwardChat(Lidgren.Network.IncomingNetMessage msg)
        {
            KeyCommon.Messages.ChatMessage chat = new KeyCommon.Messages.ChatMessage();
            chat.Read(msg.Buffer);

            // verify this message is from the same user 
            chat.SenderID = ((KeyCommon.DatabaseEntities.User)msg.m_sender.Tag).ID;
            // reset the UserID not relying on the user to have set this properly

            Console.WriteLine("Forwarding chat: " + chat.Content);

            switch (chat.Scope)
            {
                case KeyCommon.Scope.Global:
                    mServer.Broadcast(chat, new Lidgren.Network.NetConnectionBase[] { msg.m_sender });

                    break;
                case KeyCommon.Scope.Local :
                    //chat.Tag 'in this context, Tag is the table's primary key in string form
                    // only broadcast to users in that table 

                    long tableID = 0;
                    if (long.TryParse((string)chat.Tag, out tableID))
                    {
                        NetConnectionBase[] connections = GetGroupCastConnections(GetFilteredUsers(mTables[tableID].UserIDs, new string[] { chat.Name }));
                        mServer.Groupcast(chat, connections);
                    }

                    break;
                case KeyCommon.Scope.Private:
                    break;
                // tag in this context is the userID
                // TODO:
            }
        }

        private string[] GetFilteredUsers(string[] userID, string[] excludedUser)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < userID.Length ; i++)
            {
                string id = userID[i];
                bool excluded = false;
                for (int j = 0; j < excludedUser.Length ; j++)
                {
                    if (id == excludedUser[j])
                    {
                        excluded = true;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }

                if (!excluded)
                {
                    result.Add(id);
                }
                excluded = false;
            }

            return result.ToArray();
        }

        private NetConnectionBase[] GetGroupCastConnections(string[] userID)
        {
            if (userID == null || userID.Length == 0) return null;

            NetConnectionBase[] connections = new NetConnectionBase[userID.Length];

            for (int i = 0; i < userID.Length ; i++)
            {
                connections[i] = (NetConnectionBase )( mUsers[userID[i]]).Tag;
            }

            return connections;
        }

        

        private int mLastCreatedTable;
        private int mLastClosedTable;
        private Random mRandom = new Random(Environment.TickCount);
        private const int REMOVE_TABLE_INTERVAL = 10000;
        private const int CREATE_TABLE_INTERVAL = 5000;

        public void Update(long elapsed)
        {
            long tick =  Stopwatch.GetTimestamp();
            // run's maintenance on existing tables, creates new tables when needed, etc

            // we need either a set of heuristics to determine what type of table to create and/or we need for users to be able to
            // submit "votes" for games to create.  A way of handling "votes" is basically just going off the "waiting list" length like 
            // the online poker room model
            // 
            // Now what i  really don tlike about server side only game creation is certain aspects of the games settings arent configurable
            // 

            //' temp: remove a random empty table
            //If (tick - mLastClosedTable > REMOVE_TABLE_INTERVAL) Then
            //    If (mTables.Count > 1) Then
            //        DebugRemoveTable()
            //    End If
            //End If

            // temp: create a table up to max number of 10 tables
            //if (tick - mLastCreatedTable > CREATE_TABLE_INTERVAL)
            //{
            //    if (mTables.Count < 10)
            //    {
            //        DebugCreateTable();
            //    }

            //}
        }

        private void DebugRemoveTable()
        {
            Table removedTable = null;
            lock (mTables)
            {

                var id = mRandom.Next(0, mTables.Count - 1);
                if (mTables[id].UserIDs.Length > 0)
                {
                    removedTable = mTables[id];
                    throw new NotImplementedException();
                }
                //mTables.Removeat(removedTable)
                mLastClosedTable = Environment.TickCount;
            }

            if (removedTable != null)
            {
                TableClosed(removedTable);
            }
        }

        private void DebugCreateTable()
        {
        //    mLastCreatedTable = Environment.TickCount;

        //    long id = GetNextTableID(); 
        //    Table newTable = new Table(id);
         

        //    newTable.Name = System.Guid.NewGuid().ToString("N");
        //    // just a way to get a unique string.  "N" format returns numbers only with no hypens
        //    List<GameConfigParameter> newSettings = new List<GameConfigParameter>();

        //    newTable.Settings = newSettings;

        //    lock (mTables)
        //    {
        //        mTables.Add(newTable.ID, newTable);

        //        mLastCreatedTable = Environment.TickCount;
        //    }

        //    TableCreated(newTable);
        }

    }

}
