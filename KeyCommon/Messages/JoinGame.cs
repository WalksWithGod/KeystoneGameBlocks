using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class JoinGame : Messages.MessageBase 
    {
        public string HostName; // the user name of the host account hosting this game
        public string UserName; // the name of the user wishing to join

        public JoinGame() : base ((int)Enumerations.Simulation_Join)
        {
        }

        public JoinGame(string hostName, string userName) : this()
        {
            HostName = hostName;
            UserName = userName;
        }

        
        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            HostName = buffer.ReadString();
            UserName = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(HostName);
            buffer.Write(UserName);
        }
        #endregion
    }
}
