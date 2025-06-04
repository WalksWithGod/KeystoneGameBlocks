using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class UserStatusChanged : MessageBase
    {
        public int mStatusID;
        public long UserID;
        public long TableID;
        public string UserName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">Register or Unregister</param>
        public UserStatusChanged(int command) : base ((int)Enumerations.UserStatusChanged)
        {
            mStatusID = command;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            mStatusID = buffer.ReadInt32();
            throw new NotImplementedException();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(mStatusID);
            throw new NotImplementedException();
        }
        #endregion
    }
}
