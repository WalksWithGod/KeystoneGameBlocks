using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class NotifyPlugin_ProcessEventQueue : MessageBase
    {
        public string NodeID;
        public string Typename;


        public NotifyPlugin_ProcessEventQueue()
            : base ((int)Enumerations.NotifyPlugin_ProcessEventQueue)
        {
        }



        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            // note: currently im not even using this Node_GetProperty command
            // because my plugins are retreiving properties from nodes that are in the local
            // cache
            base.Read(buffer);
            NodeID = buffer.ReadString();
            Typename = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            // note: currently im not even using this Node_GetProperty command
            // because my plugins are retreiving properties from nodes that are in the local
            // cache
            base.Write(buffer);
            buffer.Write(NodeID);
            buffer.Write(Typename);


        }
        #endregion
    }

}
