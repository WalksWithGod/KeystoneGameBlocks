using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class UserList : MessageBase
    {
        public Scope Scope;
        public Dictionary<long, string> Users;

        public UserList()
            : base ((int)Enumerations.RetreiveUserList)
        {
            Users = new Dictionary<long, string>();
        }

        public void AddUser(string username, long userID)
        {
            Users.Add(userID, username);
        }
        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            throw new NotImplementedException();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            throw new NotImplementedException();
        }
        #endregion
    }
}
