using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class LeaveTable : Messages.MessageBase  
    {
        public long TableID;
        public long[] UserIDs;  // an array?  is this so we can notify other users to remove multiple users potentially from a table they are at?

        public LeaveTable()
            : base ((int)Enumerations.Table_Leave)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

        }
        #endregion
    }
}
