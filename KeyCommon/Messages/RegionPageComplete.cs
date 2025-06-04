using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Server is notified by Client that a Region has been paged in completely and Spawning of dynamic Entities can begin.
    /// </summary>
    public class RegionPageComplete : MessageBase
    {
        public string RegionID;
        
        public RegionPageComplete() : base ((int)Enumerations.RegionPageComplete)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RegionID = buffer.ReadString();
            
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RegionID);

        }
        #endregion
    }
}
