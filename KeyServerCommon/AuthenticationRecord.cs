using System;

namespace KeyServerCommon
{
    public class AuthenticationRecord : KeyCommon.DatabaseEntities.GameObject
    {
        public long UserID ;
        public string IPAddress ;
        public bool Succeeded ;
        public DateTime Date ;

        public AuthenticationRecord(long userID) : base(userID)
        {
        }

    #region Entity members
        public virtual void Read(Lidgren.Network.NetBuffer buffer)
        {
            mID = buffer.ReadInt64();
        }

        public virtual void Write(Lidgren.Network.NetBuffer buffer)
        {
            buffer.Write(mID);
        }
        #endregion
    }
}
