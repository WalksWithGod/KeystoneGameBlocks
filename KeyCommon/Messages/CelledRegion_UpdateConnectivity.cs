using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    class CelledRegion_UpdateConnectivity : MessageBase
    {
        public string CelledRegionID;


        public CelledRegion_UpdateConnectivity()
            : base ((int)Enumerations.CelledRegion_UpdateConnectivity)
        { }

        public CelledRegion_UpdateConnectivity(string interiorID) : this()
        {
            CelledRegionID = interiorID;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            CelledRegionID = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(CelledRegionID);
        }
        #endregion
    }
}
