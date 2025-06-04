using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    public class CommandSuccess : MessageBase
    {
        protected long mReferencedCommand; // id of the command that was successful


        public CommandSuccess()
            : base((int)Enumerations.CommandSuccess)
        {
        }

        public CommandSuccess(long referencedCommand)
            : this()
        {
            mReferencedCommand = referencedCommand;
        }

        public long ReferencedCommand { get { return mReferencedCommand; } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            mReferencedCommand = buffer.ReadInt32();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(mReferencedCommand);
        }
        #endregion

    }
}
