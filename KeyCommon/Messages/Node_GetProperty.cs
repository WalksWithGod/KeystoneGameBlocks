using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class Node_GetProperty : MessageBase
    {
        public string NodeID;
        public string PropertyName;

        private Settings.PropertySpec mPropertySpec; // filled as result

        // todo: we need to implement functionality that prevents this from being sent over the wire and instead goes straight to client side worker and processing
        public Node_GetProperty()
            : base ((int)Enumerations.NodeGetState)
        {
        }

        public Settings.PropertySpec Property { get{ return mPropertySpec; } }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            // note: currently im not even using this Node_GetProperty command
            // because my plugins are retreiving properties from nodes that are in the local
            // cache
            base.Read(buffer);
            NodeID = buffer.ReadString();

            mPropertySpec = new Settings.PropertySpec();
            mPropertySpec.Read(buffer);
            PropertyName = mPropertySpec.Name;
        }

        public override void Write(NetBuffer buffer)
        {
            // note: currently im not even using this Node_GetProperty command
            // because my plugins are retreiving properties from nodes that are in the local
            // cache
            base.Write(buffer);
            buffer.Write(NodeID);

            mPropertySpec.Write(buffer);
        }
        #endregion
    }
}
