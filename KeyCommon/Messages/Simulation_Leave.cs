using System;
using Lidgren.Network;
using Keystone.Types;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Simulation_Leave : MessageBase
    {
        // we use username + password for Leave because we want to avoid
        // hacked clients sending false leave commands for other users.
        public string UserName;
        public string Password;

        public Simulation_Leave() : base ((int)Enumerations.Simulation_Leave)
        {
        }



        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            UserName = buffer.ReadString();
            Password = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(UserName);
            buffer.Write(Password);
        }
        #endregion
    }
}
