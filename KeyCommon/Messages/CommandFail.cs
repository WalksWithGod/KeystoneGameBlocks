using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class CommandFail : MessageBase
    {
        protected long mReferencedCommand; // id of the command that failed
        protected int mFailureCode;

        public CommandFail() : base ((int)Enumerations.CommandFail  )
        {
        }

        public CommandFail(long referencedCommand)
            : this()
        {
            mReferencedCommand = referencedCommand;
        }

        public long ReferencedCommand { get { return mReferencedCommand; } }
        public int Code { get { return mFailureCode; } set { mFailureCode = value; } }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            mReferencedCommand = buffer.ReadInt32();
            mFailureCode = buffer.ReadInt32();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(mReferencedCommand);
            buffer.Write(mFailureCode);
        }
        #endregion

    }
}
