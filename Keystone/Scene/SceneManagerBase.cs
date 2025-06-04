using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Types;
using Keystone.Resource;
using KeyCommon.DatabaseEntities;
using KeyCommon;
using Lidgren.Network;
using Keystone.Workspaces;
using System.IO;

namespace Keystone.Scene
{
    /// <summary>
    /// Only one SceneManager can exist which contains all graphs which are rendered together as a single logical "scene"
    /// Because of the nature of how RenderBeforeClear, Render, PostRender work, only one logical scene can exist.
    /// There would be no easy way to do two seperate renders where a second SceneManager also needs to RenderBeforeClear 
    ///  That's why I think the best approach is to simply Add the second "scene" as just another graph that exists in 
    /// a seperate "layer" and to include logic to handle the additional transitioning to/rendering of layers of graphs in addition to a list of graphs
    /// </summary>
    public abstract class SceneManagerBase : IDisposable
    {
        protected NetServer mServer;
        protected bool _disposed;
        internal Dictionary<string, Scene > mScenes;

        // note: serverscenemanager uses a list of all players regardless of which scene it's in.
        // but client doesn't really need mPlayers in the SceneManager does it?
        protected Dictionary<string, Player> mPlayers;
        protected Core mCore;


        public SceneManagerBase()
        {
            mScenes = new Dictionary<string, Scene>();
            mPlayers = new Dictionary<string, Player>();
        }

        ~SceneManagerBase()
        {
            Dispose(false);
        }

        public Scene[] Scenes
        {
            get
            {
            	if (mScenes == null || mScenes.Count == 0) return null;
            	
                Scene[] results = new Scene[mScenes.Count];
                int i = 0;
                foreach (Scene scene in mScenes.Values)
                {
                    results[i] = scene;
                    i++;
                }
                return results;
            }
        }

        public Scene GetScene (string id)
        {
        	foreach (Scene scene in mScenes.Values)
            {
        		if (scene.ID == id) return scene;
            }
        	
        	return null;
        }
        
        public static string CreateRootNodeName (string typename, string modName)
        {
            return Repository.GetNewName(typename);
        	string DELIMITER = ",";
        	return typename + DELIMITER + modName;
        }
                
        // TODO: isnt this moved to FormMain.Commands?
        // it's used in fact in Worker_GenerateNewScene  but also in ProcessCompletedCommandQueue for Preview_Save 
        public bool CreateNewSceneDatabase(string relativeFolderPath, float diameterX, float diameterY, float diameterZ, out string startingRegionID)
        {
        	
            XMLDatabase xmldb = new XMLDatabase();
            SceneInfo info = xmldb.Create(SceneType.SingleRegion, relativeFolderPath, typeof(Keystone.Portals.Root).Name);

            Vector3d min = new Vector3d(-diameterX * .5f, -diameterY * .5f, -diameterZ * .5f);
            Vector3d max = new Vector3d(diameterX * .5f, diameterY * .5f, diameterZ * .5f);
            uint octreeDepth = 3;

            string rootNodeName = CreateRootNodeName(typeof(Keystone.Portals.Root).Name, relativeFolderPath);
            startingRegionID = rootNodeName;

            //Keystone.Portals.Root r = new Keystone.Portals.Root(rootNodeName, 1, 1, 1, (float)(max.x - min.x), (float)(max.y - min.y), (float)(max.z - min.z), octreeDepth);
            Keystone.Portals.Root r = new Keystone.Portals.Root(rootNodeName, (float)(max.x - min.x), (float)(max.y - min.y), (float)(max.z - min.z), octreeDepth);

            // add a default directional light - Dec.1.2022
            Keystone.Lights.DirectionalLight light = Celestial.LightsHelper.LoadDirectionalLight(diameterX * 0.5f);
            Traversers.SuperSetter setter = new Traversers.SuperSetter(r);
            setter.Apply(light);

            xmldb.WriteSychronous(r, true, true, false);

            // TODO: so we want to have Viewpoint's added to the Scene, but here, we dont even have the scene created yet...
            // that's lame...   and if we make Scene a Node which our SceneInfo can list as FirstTypename, then
            // we need to be able to have Scene created through "Repository.Create()"  but currently that's not possible
            // because Scene also requires Sim and Event Handlers to be passed in... as well as an appropriate IO.Pager type.
            // Well, also consider that our Scene we're trying to turn into a Root... we already have a Root.  So it begs the question
            // why aren't Viewpoints added to Root and why too aren't EntitySystems which i think 
            // - but our SceneInfo which can be used to load Preview, also dont have a Root... viewpoints are added to the SceneInfo again
            //   but there's no reason as we know that a Viewpoint can't be child to an Entity... so all of this is to decide where to put
            // IEntitySystem... 
            string id = Repository.GetNewName(typeof(Viewpoint));
            Viewpoint vp = Viewpoint.Create(id, r.ID); // NOTE: r.ID will be the startingRegionID

            // viewpoints added to SceneInfo's must always be serializable because they were never cloned
            vp.Serializable = true;
            info.AddChild (vp);
            
            xmldb.WriteSychronous(info, true, false, false);
            xmldb.SaveAllChanges();
            xmldb.Dispose();

            info.RemoveChild(vp);
         //   Repository.IncrementRef(vp);
         //   Repository.DecrementRef(vp);

            // The IncrementRef/DecrementRef aren't needed because the Root node is created using New and not Repository.Create()
            //Repository.IncrementRef(r);
            //Repository.DecrementRef(r);

            // The IncrementRef/DecrementRef aren't needed because ??
            //Repository.IncrementRef(info);
            //Repository.DecrementRef(info);
            return true;
        }
                        
        public bool CreateNewSceneDatabase(string relativeFolderPath, string modName,
                                           SceneType type,
                                           uint regionsAcross, uint regionsHigh, uint regionsDeep,
                                           float regionDiameterX, float regionDiameterY, float regionDiameterZ,
                                           bool serializeEmptyZones,
                                           uint octreeDepth,
                                           uint structureLevelsHigh,
                                           out Keystone.Portals.ZoneRoot root,
                                           out Keystone.Scene.SceneInfo info,
                                           out Keystone.IO.XMLDatabase xmldb)
        {
        	         
        	string typename = typeof(Keystone.Portals.ZoneRoot).Name;
            string rootNodeName = CreateRootNodeName (typename, modName);
                      	
        	xmldb = new XMLDatabase();
        	info = xmldb.Create(type, relativeFolderPath, typename);
            info.SerializeEmptyZones = serializeEmptyZones ;


            root = new Keystone.Portals.ZoneRoot(rootNodeName, regionsAcross, regionsHigh, regionsDeep,
                                        regionDiameterX, regionDiameterY, regionDiameterZ, 0, structureLevelsHigh); //0 octree depth for root, but non root zones can use octree depth

            // TODO: Sept.3.2016 - why two viewpoints here? is it for Navigation at root node?
            // ANSWER: There are two viewpoints because one is for the Root and one is for the specific Zone.
            //         NOTE: These viewpoints have Serializable = true but are only added to SceneInfo and are NOT actually added to the Scene.  
            //         Then when a RenderingContext is initialized, we .Clone() one of these viewpoints with Serializable = false and is assigned
            //         to context.Viewpoint.  But even then it is NOT added to the Scene because the Region it is located in may not be paged in.
            //         It is only during ClientPager that the context.Viewpoint is added to a Region.
            //         
            //       Why not just insert a Viewpoint when going to Navigation workspace and then removing it when done?
            //		 And why are both viewpoints appearing under Zone0,0,0.  ZoneRoot has no viewport showing in the treeview
            // We will create two Viewpoints here 
            // 1) A Viewpoint at the ZoneRoot origin
            string id = Repository.GetNewName(typeof(Viewpoint));
            Viewpoint vp = Viewpoint.Create(id, root.ID);
            // viewpoints added to SceneInfo's must always be serializable because they were never cloned
            vp.Serializable = true;
            info.AddChild(vp);
                 
            double x = 0;
            if (regionsAcross % 2 == 0)
                x = -regionDiameterX / 2d;
            double y = 0;
            if (regionsHigh % 2 == 0)
                y = -regionDiameterY / 2d;
            double z = 0;
            if (regionsDeep % 2 == 0)
                z = -regionDiameterZ / 2d;

            // 2) A Viewpoint in the Zone that contains this global center coordinate.
            // Zone Paging can only occur when the user's current viewpoint is located
            // in a Zone not ZoneRoot.
            int centerZoneX, centerZoneY, centerZoneZ;
            root.GetZoneCenterSubscripts(out centerZoneX, out centerZoneY, out centerZoneZ);

            string startingRegion = root.GetZoneName (centerZoneX, centerZoneY, centerZoneZ);
            id = Repository.GetNewName(typeof(Viewpoint));
            vp = Viewpoint.Create(id, startingRegion);
            vp.StartingTranslation = new Vector3d (x, y, z);
            // viewpoints added to SceneInfo's must always be serializable because they were never cloned
            vp.Serializable = true;
            info.AddChild (vp);

			return true;
        }


        public virtual Scene Open(string folderName, Keystone.Simulation.ISimulation sim, 
                                  PagerBase.PageCompleteCallback emptyRegionPageCompleteHandler, 
                                  PagerBase.PageCompleteCallback regionChildrenPageCompleteHandler)
        {
            try
            {
                Scene scene;

                if (this is ClientSceneManager)              	
                	scene = Scene.CreateClientScene(this, folderName, sim, emptyRegionPageCompleteHandler, regionChildrenPageCompleteHandler);
                else
                    scene = Scene.CreateServerScene(this, folderName, sim, emptyRegionPageCompleteHandler, regionChildrenPageCompleteHandler);

                //mScenes.Add(game.mName, scene);
             //   mScenes.Add(fullFolderPath, scene); // TODO: what if the filename changes? don't allow filename changes
                return scene;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SceneManagerBase.Load() - ERROR loading simulation '" + folderName + "' " + ex.Message);
                return null;
            }
        }


        public Game[] GetGames()
        {
            if ((mScenes == null || mScenes.Count == 0))
            {
                return null;
            }

            Game[] games = new Game[mScenes.Count];
            int i = 0;
            foreach (Scene scene in mScenes.Values)
            {
                games[i] = scene.Simulation.Game;
                i++;
            }
            return games;
        }

        public Game GetGame(string accountName)
        {
            // TODO: accountName do not match the Simulation or Scene name... hrm... 
            // til then, we're simply going to loop and search the actual game hosted name

            foreach (Scene scene in mScenes.Values)
                if (scene.Simulation.Game.Host.Name == accountName)
                    return scene.Simulation.Game;

            return null; 
        }
        

        public void UnloadAllSimulations()
        {
            foreach (Scene scene in mScenes.Values)
            {
                scene.Unload();
                scene.Dispose();
            }

            mScenes.Clear();
        }

        public void Unload(string name)
        {
            mScenes[name].Unload();
            mScenes[name].Dispose();
            mScenes.Remove(name);
        }


        public abstract void Update(Keystone.Simulation.GameTime gameTime);
       


        //Private mAI As AI  // or would we have multiple AI agents, one instance for each player that was taken over by a full AI?
        //// it'd be easier to multithread it that way perhaps... 

        //// TODO: mPlayers should correspond to slots.  Keep in mind that AI can assist all players for when turns are running while they are just
        //// allowing their pre-set orders to carry out. (unless there is a way for users to tell the game to temporitly automanage everything
        //// if they're going out of town for a few days or something.
        ////  But that's not really an AI is it?  It's just following the orders the user has queued up?
        ////  Only when a player officially abandons the game or misses some X turns will they be
        //// completely replaced and so those mPlayers slots will need to be marked since we dont need to send network updates
        //// to replaced players.  We could use a type of NullConnectionInfo or something.
        //// If a player tries to REJOIN a game they abandoned or have been replaced by full control AI, they cant because in the Players
        //// database, their status is marked as "forfeit" and "forfeit_reason" is either "missed_max_consecutive_turns", "missed_max_turns", "explicit_forfeit"

        //// TODO:  this dictionary contains a mix of Humans and AI players.  That is, not all can be associated
        //// with a ConnectionInfo object?  How does mUsers here related to SimHost mPlayers (of integer, ConnectionInfo) ?


        //    // for games where only registered players can join the match, retreive the list of those players
        // and then initialize their online status = false
        //    ClientServerCommon.Player[] p =
        //       (ClientServerCommon.Player[])mHost.mGamesStorageContext.RetreiveList(GetType(ClientServerCommon.Player).Name, "game_id", DbType.String, game.mID);
        //    if (p != null) 
        // {
        //       for (int i = 0; i < p.Length; i++)
        //            p[i].mOnline = false;
        //        
        //      }
        //    }

        //Public Property ActiveUsers() As User()
        //    Get
        //        'mGame.mSettings.mRules.GameType()

        //    End Get
        //    Set(ByVal value As User())

        //    End Set


        /// <summary>
        /// Dispatches the command to the appropriate game object (since a server can host more than one game) 
        /// based on the player sending the message
        /// Based on the sender, we send this message to the Simulation this user is part of
        /// </summary>
        /// <param name="message"></param>
        public void MessageProc(IncomingNetMessage message)
        {
            KeyCommon.Messages.Enumerations  command = (KeyCommon.Messages.Enumerations)message.Buffer.ReadInt32();

            Lidgren.Network.NetConnectionBase connection = message.m_sender;
            Player player = (Player)connection.Tag;

            // 
            mScenes[player.GameName].Simulation.UserMessageReceived(connection, message.Buffer);

            switch (command)
            {

            //    case KeyCommon.Messages.Enumerations.ChatMessage:
            //        KeyCommon.Commands.ChatMessage chat = new KeyCommon.Commands.ChatMessage();
            //        chat.Read(message.Buffer);
            //        Console.WriteLine("Chat message sent {" + chat.Content + "}");

            //        // manually set the chat.SenderID to match the connectionID.  We don't trust the original sender to have set this properly
            //        chat.SenderID = player.PrimaryKey;
            //        //NOTE: Broadcast shouldn't be used unless you truly intend to send to every user on the entire server, and not just this SImHost
            //        // and if you do, do that, make sure that the clients either have a list of all the users so the SenderID can be looked up
            //        // or have the client ignore the SenderID and perhaps have the server append the username instead so no lookup required
            //        //mHost.mServer.Broadcast(chat, New Lidgren.Network.NetConnectionBase() {connection})
            //        // notify all users
            //        Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mPlayers.Count];
            //        int i = 0;
            //        foreach (Player p in mPlayers.Values)
            //        {
            //            connections[i] = (NetConnectionBase)p.Tag;
            //            i++;
            //        }
            //        // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //        mServer.Groupcast(chat, connections);
            //        break;

            //    // client explicitly requesting authorization to download files
            //    case KeyCommon.Messages.Enumerations.FileDownloadRequest:
            //        //KeyCommon.Commands.FileDownloadTicketRequest request ;
            //        //request.Read(message.Buffer);

            //        //// TODO: verify the user is allowed to download the requested files.  For my part
            //        //// i just have to show that the server is capable of creating the authorization packet and sending it to the client
            //        //// so i'm just going to use the tag without verifying the files requsted are valid for this particular user
            //        //string tag = request.FileIDs;

            //        //// TODO: the webserver's host data should come from the config file with perhaps an alternate mirror address if the first is down
            //        //string mFileServerName  = "ProjectEvoFileServer1";
            //        //string mFileServerPassword  = "FileServerPassword1";
            //        //string mFileServerAddress  = "http://192.168.1.64/secure.asp";
            //        //int mFileServerPort  = 80;
            //        //const int FILE_DOWNLOAD_TIMEOUT = int.MaxValue;
            //        //byte[] sessionKey = player.mSessionKey;
            //        //string userip = "192.168.1.65"; //  connection.RemoteEndpoint.Address.ToString ' this is problematic if we connect to a local game server and then try to authenticate with remote file server, the ip will not match
            //        //Authentication.Reply reply = new Authentication.Reply(userip, player.mName, sessionKey, mFileServerName, mFileServerPassword, FILE_DOWNLOAD_TIMEOUT, tag);

            //        //KeyCommon.FileDownloadAuthorization authorization = new KeyCommon.FileDownloadAuthorization() ;
            //        //authorization.ReplyTicket = reply.ToBytes();
            //        //authorization.Host = mFileServerAddress;
            //        //authorization.Port = mFileServerPort;
            //        //authorization.FileName = "download.zip";  //TODO: this needs to match what the client should have it named 

            //        //connection.SendMessage(authorization, NetChannel.ReliableUnordered);
            //        break;

            //    // Appearance
            //    // case KeyCommon.Messages.Enumerations.UnitAppeared

            //    //case KeyCommon.Messages.Enumerations.CityAppeared

            //    // case KeyCommon.Messages.Enumerations.ImprovementAppeared

            //    // Commands
            //    //case KeyCommon.Messages.MoveUnit:
            //    //    Console.WriteLine("MoveUnit requested.");
            //    //    break;


            //    case KeyCommon.Messages.Enumerations.RetreiveUserList:
            //        Console.WriteLine("Retreive User List requested."); //TODO: this doesnt belong here does it?
            //        break;

                default:
                    Console.WriteLine("Unexpected game command from User.");
                    break;
            }
        }



        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            UnloadAllSimulations();
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("scene is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}