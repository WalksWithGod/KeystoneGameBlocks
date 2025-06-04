using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class Node_RenameResource : MessageBase
    {
        public string ParentID;
        public string OldResourceID;
        public string NewResourceID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public Node_RenameResource(string oldResourceID, string newResourceID, string parentID) : this()
        {
            ParentID = parentID;
            OldResourceID = oldResourceID;
            NewResourceID = newResourceID;
        }


        public Node_RenameResource()
            : base((int)Enumerations.Node_RenameResource)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ParentID = buffer.ReadString();
            OldResourceID = buffer.ReadString();
            NewResourceID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ParentID);
            buffer.Write(OldResourceID);
            buffer.Write(NewResourceID);
        }
        #endregion
    }
}
