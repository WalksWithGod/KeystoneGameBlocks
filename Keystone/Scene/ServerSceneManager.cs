using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Entities;
using KeyCommon.DatabaseEntities;
using Lidgren.Network;

namespace Keystone.Scene
{
    /// <summary>
    /// Only one SceneManager can exist which contains all graphs which are rendered together as a single logical "scene"
    /// Because of the nature of how RenderBeforeClear, Render, PostRender work, only one logical scene can exist.
    /// There would be no easy way to do two seperate renders where a second SceneManager also needs to RenderBeforeClear 
    ///  That's why I think the best approach is to simply Add the second "scene" as just another graph that exists 
    ///  off the root node and then just write logic to handle transition fx and enable\disable of scenes.
    ///  (for instance a transition can start with a full screen catpure of the previous scene just prior to disabling it, then fading it out
    ///  as you now render the newly enabled scene instead. ) the additional transitioning to/rendering of layers of graphs in addition to a list of graphs
    /// </summary>
    public class ServerSceneManager : SceneManagerBase 
    {

        public ServerSceneManager(Core core, NetServer netServer)
            : base()
        {
            mCore = core;
            core.SceneManager = this;
        }

        public Scene Load(string listenTable, string serviceName, string servicePassword, string map, Keystone.Simulation.ISimulation sim)
        {
            Scene scene = Open( sim.Game.Map, sim, null, null);
            return scene;
        }

        /// <summary>
        /// Find the player in the player's list and set their mOnline status = true, and associate the connection to their .Tag
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="sessionKey"></param>
        /// <param name="connection"></param>
        /// <param name="gameID"></param>
        /// <remarks></remarks>
        public void AddPlayer(string playerName, byte[] sessionKey, NetConnectionBase connection, string gameHostAccountName)
        {

            Simulation.ISimulation sim = mScenes[gameHostAccountName].Simulation; // GameManagers[gameHostAccountName].Game;

            Player player; // = (Player)mGamesStorageContext.Retreive(typeof(Player).Name, "name", DbType.String, playerName);
            player = new Player();
            player.GameName = gameHostAccountName;
            player.mName = playerName;
            player.Tag = connection;
            player.mSessionKey = sessionKey; // used only for file downloads from the webserver
            connection.Tag = player;

            // TODO: i should verify this player isn't already in a second game on this server?  if so
            // that shouldn't be done in "AddPlayer" but during connection approval?
            mPlayers.Add(player.mName, player);
            sim.AddPlayer(player);
        }


        public void RemovePlayer(Player player)
        {
            mScenes[player.GameName].Simulation.RemovePlayer(player);
            mPlayers.Remove(player.mName);
            player.mOnline = false;
            player.Tag = null;
        }

        public override void Update(Keystone.Simulation.GameTime gameTime)
        {
            // server's can host multiple simulations(games)
            if (mScenes != null && mScenes.Count > 0)
            {
                foreach (Scene scene in mScenes.Values)
                {
                    if (scene.Simulation != null && scene.Simulation.Running)
                    {
                        scene.Simulation.Update(gameTime);
                    }
                    
                    // sim may be paused by the graphics may still be rendering?
                    scene.Update(gameTime.ElapsedSeconds);
                }
            }
            //if (mSimulations != null && mSimulations.Count > 0)
            //{
            //    foreach (Simulation.Simulation sim in mSimulations.Values)
            //    {
            //        if (sim.IsRunning)
            //        {
            //            sim.Update(elapsedMilliseconds);
            //        }
            //        // sim may be paused by the graphics may still be rendering?
            //        sim.Scene.Update(elapsedMilliseconds);
            //    }
            //}
        }

    }
}