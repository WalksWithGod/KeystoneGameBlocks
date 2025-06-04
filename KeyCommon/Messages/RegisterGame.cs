using System;
using Lidgren.Network;
using KeyCommon.DatabaseEntities;

namespace KeyCommon.Messages
{
    public class RegisterGame : MessageBase 
    {
        private Game mGame;

        public RegisterGame() : base ((int)Enumerations.Simulation_Register )
        {
        }

        public RegisterGame(Game game)
            : this()
        {
            if (game == null) throw new ArgumentNullException();
            mGame = game;
        }

        public Game Game { get { return mGame; } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read (buffer);
            mGame = new Game (-1); // todo: this needs to be a proper GameID not -1
            mGame.Read(buffer);
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write (buffer);
            mGame.Write(buffer);
        }
        #endregion
        
    }
}
