using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class ChatMessage : MessageBase
    {

        public string Content;
        public long SenderID;
        public Scope Scope;
        public string Tag;  // contains a tableid if scope is  

        public ChatMessage() : base ((int)Enumerations.ChatMessage )
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            throw new NotImplementedException();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            throw new NotImplementedException();
        }
        #endregion
    }
}
