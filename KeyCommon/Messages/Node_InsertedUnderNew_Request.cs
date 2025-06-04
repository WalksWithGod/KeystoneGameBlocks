using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Moves an existing node from it's current parent to a new parent that will be created.
    /// </summary>
    public class Node_InsertUnderNew_Request : MessageBase
    {
        public string NodeType;
        public string ParentID; // current parent
        public string NodeID;   // new parent?

        /// <summary>
        /// Moves an existing node from it's current parent to a new parent that will be created.
        /// </summary>
        /// <param name="NodeType">The typename of the new node that will be created. It must
        /// be of type IGroup so that the existing nodeID can be moved under it.</param>
        /// <param name="parentID">The parent which the existing NodeID will be removed and
        /// to which the new node will be added as child instead.</param>
        /// <param name="nodeID">The existing node which will be added under the new node.</param>
        public Node_InsertUnderNew_Request(string nodeType, string parentID, string nodeID)
            : this()
        {
            NodeType = nodeType;
            ParentID = parentID;
            NodeID = nodeID;
        }
        
        public Node_InsertUnderNew_Request()
            : base ((int)Enumerations.Node_InsertUnderNew_Request)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeType = buffer.ReadString();
            ParentID = buffer.ReadString();
            NodeID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeType);
            buffer.Write(ParentID);
            buffer.Write(NodeID);
        }
        #endregion
    }
}
