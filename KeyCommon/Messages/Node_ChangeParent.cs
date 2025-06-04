using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class Node_ChangeParent: MessageBase
    {
        public string NodeID;
        public string ParentID;
        public string OldParentID; // if this node is not an entity, it maybe more than one parent so we should specify

        public Node_ChangeParent()
            : base ((int)Enumerations.Node_ChangeParent) 
        {
            
        }

        public Node_ChangeParent(string nodeID, string parentID, string oldParentID) : this()
        {
            if (string.IsNullOrEmpty(nodeID) || string.IsNullOrEmpty(parentID) || string.IsNullOrEmpty(oldParentID)) throw new ArgumentNullException();
            NodeID = nodeID;
            ParentID = parentID;
            OldParentID = oldParentID;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeID = buffer.ReadString();
            ParentID = buffer.ReadString();
            OldParentID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeID);
            buffer.Write(ParentID);
            buffer.Write(OldParentID);
        }
        #endregion
    }
}
