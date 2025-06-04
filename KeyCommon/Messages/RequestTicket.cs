using System;
using System.Collections.Generic;


namespace KeyCommon.Messages
{
    public class RequestTicket : MessageBase 
    {
        public string ServiceName;  // synoymous with the user name for the account


        public RequestTicket()
            : base ((int)Enumerations.RequestTicket)
        { }

        #region IRemotableType Members
        public override Lidgren.Network.NetChannel Channel
        {
            get { return Lidgren.Network.NetChannel.ReliableInOrder1; }
        }
        public override void Read(Lidgren.Network.NetBuffer buffer)
        {
            base.Read(buffer);
            ServiceName = buffer.ReadString();
        }

        public override void Write(Lidgren.Network.NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ServiceName);
        }
        #endregion
    }
}
