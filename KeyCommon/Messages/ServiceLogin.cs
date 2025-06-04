using System;
using System.Collections.Generic;


namespace KeyCommon.Messages
{
    public class ServiceLogin : MessageBase 
    {
        public string Name;  // name of the account that wants to log in to a service
        public string ServiceName; 
        public byte[] AuthLogin;
       

        public ServiceLogin()
            : base ((int)Enumerations.ServiceLogin)
        { }

        public ServiceLogin(string userName, string serviceName)
            : this()
        {
            Name = userName;
            ServiceName = serviceName;
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
            ServiceName = buffer.ReadString();

            // login data
            int length = buffer.ReadInt32();
            if (length > 0)
                AuthLogin = buffer.ReadBytes(length);
        }

        public override void Write(Lidgren.Network.NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(Name);
            buffer.Write(ServiceName);
            // login length and data
            int length;
            if (AuthLogin == null)
                length = 0;
            else
                length = AuthLogin.Length;

            buffer.Write(length);
            if (length > 0)
                buffer.Write(AuthLogin);

        }
        #endregion
    }
}
