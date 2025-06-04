using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{

    public class Transfer_Stream : MessageBase
    {
        public byte[] mData;
        public int mLength;

        public Transfer_Stream() : base ((int)Enumerations.StreamTransfer)
        {

        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            mLength = buffer.ReadInt32();
            mData = buffer.ReadBytes(mLength);
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(mLength);
            buffer.Write(mData);


        }
        #endregion
    }
}
