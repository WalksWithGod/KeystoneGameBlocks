using System;

namespace KeyCommon.DatabaseEntities
{
    public class Player 
    {
        public enum PlayerStatus : byte
        {
            Spectator,
            Active,
            Forfeit,
            Abandoned,
            Completed   // is this needed?  If the Game.Status is completed then obviously the player is not active for this game
        }

        public string mName;
        public long GameID; // if this server hosts multiple games, indicates the proper ID
        public string GameName;
        public PlayerStatus mStatus;
        public bool mOnline;
        //public bool mHuman  // the mID for this a non human would be -1?  Or maybe better to have AI inherit Player and then to add an extra property Player.Behavior 
        //public ActiveFaction mFaction; 
        public byte[] mSessionKey;
        public object Tag; // not serialized in IRemotableType or stored in DB.  This tag holds the Connection object used by this player on the GameServer

    }
}
