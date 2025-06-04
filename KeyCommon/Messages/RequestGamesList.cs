using System;
using Lidgren.Network;


namespace KeyCommon.Messages
{
    public class RequestGamesList : MessageBase
    {
        // public int[] Filters;
        // filter_type
        //       none
        //       region
        //       ping
        //       password
        //       mapname
        //       empty
        //       full
        //       
        public RequestGamesList() : base ((int)Enumerations.RequestGamesList )
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            //Filters = buffer.ReadInt();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            //buffer.Write((int)Filters);
        }
        #endregion
    }
}
