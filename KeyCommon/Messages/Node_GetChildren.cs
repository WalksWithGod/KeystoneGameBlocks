using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class Node_GetChildren : MessageBase
    {
        public string NodeID;
        private string[] mChildIDs;
        private string[] mChildTypes;

        public Node_GetChildren()
            : base ((int)Enumerations.NodeGetState)
        {
        }

        public string[] ChildIDs { get { return mChildIDs; } }
        public string[] ChildTypes { get { return mChildTypes;} }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeID = buffer.ReadString();

            int childCount = buffer.ReadInt32();
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    mChildIDs[i] = buffer.ReadString();
                    mChildTypes[i] = buffer.ReadString();
                }
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeID);

            int childCount = 0;
            if (mChildIDs == null || mChildIDs.Length == 0)
            {
                buffer.Write(childCount);
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                buffer.Write(mChildIDs[i]);
                buffer.Write(mChildTypes[i]);
            }
            
        }
        #endregion
    }
}
