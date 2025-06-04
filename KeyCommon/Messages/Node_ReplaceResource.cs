using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class Node_ReplaceResource : MessageBase
    {
    
        public string ParentID;
        public string OldResourceID;
        public string NewResourceID;
        public string TypeName; // the type of node we will create

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public Node_ReplaceResource(string oldResourceID, string newResourceID, string newTypeName, string parentID)  : this()
        {
            ParentID = parentID;
            OldResourceID = oldResourceID ;
            NewResourceID = newResourceID ;
            TypeName = newTypeName;
        }


        public Node_ReplaceResource()
            : base ((int)Enumerations.Node_ReplaceResource)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ParentID = buffer.ReadString();
            OldResourceID = buffer.ReadString();
            NewResourceID = buffer.ReadString();
            TypeName = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ParentID);
            buffer.Write(OldResourceID);
            buffer.Write(NewResourceID);
            buffer.Write(TypeName);
        }
        #endregion
    }
}
