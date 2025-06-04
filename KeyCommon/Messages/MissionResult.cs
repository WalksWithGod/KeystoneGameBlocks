using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class MissionResult : MessageBase
    {

        public bool Success;
        

        public MissionResult() : base((int)Enumerations.MissionResult)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            Success = buffer.ReadBoolean();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(Success);
        }
        #endregion
    }
}
