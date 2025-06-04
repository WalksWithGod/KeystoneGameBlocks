//using System;
//using System.Collections.Generic;
//using Lidgren.Network;
//using KeyCommon.Entities;

//namespace KeyGameServer
//{
//    public class SimHost 
//    {
//        private Keystone.Core mCore;
//        private string BASE_PATH;
//        private string DATA_PATH;
//        private string MODS_PATH;
        

//        //internal NetServer mServer;
//        // originally if only our servers could host games, then we used the actual database ID as dictionary key
//        // but now im thinking we should simply use the user account name which must be 1:1 unique for every hosted
//        // game on a single gameserver.exe instance since end users can also host games
//        //private Dictionary<string, Keystone.Scene.SceneManagerBase> GameManagers;

//        private Keystone.Scene.ServerSceneManager SceneManager; 
//        private Dictionary<string, Game> Games;
//        private Dictionary<string, KeyCommon.Entities.Player> mPlayers;

//        private const int TIME_STEP = 100;

//        public SimHost(NetServer netServer, string basePath, string dataPath, string modsPath, string configPath)
//        {
//           // if ((netServer == null)) throw new ArgumentNullException();
//            BASE_PATH = basePath;
//            DATA_PATH = dataPath;
//            MODS_PATH = modsPath;

//            mCore = new Keystone.Core(basePath, dataPath , modsPath , configPath);

//            SceneManager = new Keystone.Scene.ServerSceneManager(mCore);


//            //mServer = netServer;
//            Games = new Dictionary<string, Game>();
//            //GameManagers = new Dictionary<string, Keystone.Scene.SceneManagerBase>();
//            mPlayers = new Dictionary<string, KeyCommon.Entities.Player>();
//        }

//        public Keystone.Core Core { get { return mCore; } }

//        public KeyCommon.Entities.Game[] GetGames()
//        {
//            if ((Games == null || Games.Count  == 0))
//            {
//                return null;
//            }

//            KeyCommon.Entities.Game[] games = new KeyCommon.Entities.Game[Games.Count];
//            int i = 0;
//            foreach (Game g in Games.Values)
//            {
//                games[i] = g;
//                i++;
//            }

         
//            //for (int i = 0; i < GameManagers.Count; i++)
//            //{
//            //    games[i] = GameManagers[i].GameConfig;
//            //}
//            return games;
//        }

//        public KeyCommon.Entities.Game GetGame(string name)
//        {
//            Game g;
//            Games.TryGetValue(name, out g);
//            return g;
//        }

//        public Game CreateNewGame( string listenTable, string serviceName, string servicePassword, string map)
//        {

//            Game game = new KeyCommon.Entities.Game(new KeyCommon.Entities.Host(listenTable, servicePassword));
//            game.Host.Name = serviceName;
//            game.mName = "24/7 Furball Test Server";
//            //game.Description = "";
//            game.Map = map;
//            game.mPassword = servicePassword;

//            // TODO: this call should be done in a seperate thread and upon complettion then we should RegisterGame
//            Keystone.Simulation.Simulation sim = mCore.SceneManager.Load(System.IO.Path.Combine(MODS_PATH, game.Map));
//            Console.WriteLine("New Game Scene loaded '" + game.Map + "'");

//            AddGame(game, false);
//            return game;
//        }

//        /// <summary>
//        /// Loads a game and all of the player objects in that game.  
//        /// Note: Players are loaded before the player actually connects to this server.
//        /// The only thing we do when a player actually connects is to Player.mOnline = true
//        /// </summary>
//        /// <param name="game"></param>
//        /// <param name="isExisting"></param>
//        /// <remarks></remarks>
//        public void AddGame(Game game, bool isExisting)
//        {
//            // TODO: in theory no duplicates should occur but you never know.  We should not add and simply
//            // resume (i.e. dont crash)
//            // TODO: verify this particular game isnt already hosted here, and if it is, just ignore it? Might have to 
//            // update the db to remove the server_name for the assigned game so that the lobby maintenance can see that
//            // the game is unassigned and possibly has some error...
//            // however if for some reason it's a duplicate, maybe first verify that all the assigned players match the ones 
//            // we have set in that game, then if we ignore it, there's no problem whatsoever, we might only have to check the db
//            // and verify that the game's status is set as ok.

//           // mGames.Add(game.mID, new Game(this, game, true, TIME_STEP));
//           // GameManagers.Add(manager.Game.Host.Name, manager);
//            Games.Add(game.Host.Name, game);
//        }

        

//        public void Update(long elapsed)
//        {

//            // here we could use parallel library to update every hosted game simultaneously
//            // TODO: if the turns are occurring automatically at fixed intervals, we should configure the intervals to overlap
//            // so we have the fewest number of updates occuring at a time.
//            //foreach (Keystone.Scene.SceneManagerBase sceneManager in Games.Values)
//            //{
//            //    sceneManager.Update((int)elapsed);

//            //}
//        }


//        /// <summary>
//        /// Dispatches the command to the appropriate game object (since a server can host more than one game) 
//        /// based on the player sending the message
//        /// </summary>
//        public void MessageProc(Lidgren.Network.IncomingNetMessage message)
//        {

//            Player player = (Player)message.m_sender.Tag;

//            System.Diagnostics.Debug.Assert(player != null);
//            // this should be impossible since we assigned the Tag's ourselves and is not dependant on any client action
//            // same as above, should be impossible for player.mGameID to result in an invalid key in mSimulations dictionary


//            // another possibility is instead of hosting all games for all users who connect to this server,
//            // we have games\simulations running independantly
//            // on a bunch of different machines with this SimHost just being a connection manager and using something like MSMQ for message
//            // passing, then we dispatch this message to the appropriate game server for hosted game server configuration
//            //If (Not USE_DISTRIBUTED_HOSTS) Then
//            //    mSimulations(player.mGameID).MessageProc(message)
//            //Else ' or to the appropriate MSMQ for distributed game server configuration
//            //    mMSMQ.QueueMessage(player.mGameID, message) 
//            //End If

//            // TODO: I think ideally i want this SimHost (which will be converted to be ServerSceneManager) to then
//            //  select the appropriate Keystone.Simulation.Simulation  to direct this message to.  Then our 
//            // Simulations should be overriden by client and our message handling written there on the app side instead of in
//            // FormBase/FormMain/FormClient
//            //Games[player.GameName].MessageProc(message);
//        }
//    }
//}
