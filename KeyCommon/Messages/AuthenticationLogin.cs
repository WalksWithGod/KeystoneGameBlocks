using System;
using System.Collections.Generic;


namespace KeyCommon.Messages
{
    public class AuthenticationLogin : MessageBase 
    {
        public string Name;
        public string HashedPassword; 


        public AuthenticationLogin() : base ((int)Enumerations.AuthenticationLogin )
        {
        }
        



        #region IRemotableType Members
        public override Lidgren.Network.NetChannel Channel
        {
            get { return Lidgren.Network.NetChannel.ReliableInOrder1; }
        }
        public override void Read(Lidgren.Network.NetBuffer buffer)
        {
            base.Read(buffer);

            Name = buffer.ReadString();
            HashedPassword = buffer.ReadString();
        }

        public override void Write(Lidgren.Network.NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(Name);
            buffer.Write(HashedPassword);           

        }
        #endregion
    }
}
