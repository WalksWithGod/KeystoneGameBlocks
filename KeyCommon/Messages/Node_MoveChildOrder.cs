using System;
using KeyCommon.Messages;
using Lidgren.Network;


namespace KeyCommon.Messages
{
    public class Node_MoveChildOrder : MessageBase
    {
        public bool Down;
        public string ParentID; 
        public string NodeID;   


        public Node_MoveChildOrder(string parentID, string childID, bool down)
            : this()
        {
            Down = down;
            ParentID = parentID;
            NodeID = childID;
        }


        public Node_MoveChildOrder()
            : base ((int)Enumerations.Node_MoveChildOrder)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            Down = buffer.ReadBoolean();
            ParentID = buffer.ReadString();
            NodeID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(Down);
            buffer.Write(ParentID);
            buffer.Write(NodeID);
        }
        #endregion
    }
}
