using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;
using Settings;

namespace KeyCommon.Messages
{
    public class Node_Remove : MessageBase 
    {
        public string[] mTargetIDs;
        public string mParentID;

        public Node_Remove(string targetID, string parentID)
            : base ((int)Enumerations.Node_Remove)
        {
            mTargetIDs = new string[] { targetID };
            mParentID = parentID;
        }

        public Node_Remove(string[] targetIDs, string parentID)
            : base ((int)Enumerations.Node_Remove)
        {
            mTargetIDs = targetIDs;
            mParentID = parentID;
        }

        public Node_Remove()
            : base ((int)Enumerations.Node_Remove)
        { }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            int count = buffer.ReadInt32();
            if (count <= 0 || count >= Int32.MaxValue) return;

            mTargetIDs = new string[count];
            for (int i = 0; i < count; i++)
                mTargetIDs[i] = buffer.ReadString();

            mParentID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(mTargetIDs.Length);

            for (int i = 0; i < mTargetIDs.Length; i++)
                buffer.Write(mTargetIDs[i]);

            buffer.Write(mParentID);
        }
        #endregion
    }
}
