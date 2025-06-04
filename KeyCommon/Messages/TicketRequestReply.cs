using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyCommon.Messages
{
    public class TicketRequestReply : MessageBase 
    {
        public byte[] AuthenticatedTicket;

        public TicketRequestReply() : base ((int)Enumerations.TicketReply) { }


        #region IRemotableType Members
        public override Lidgren.Network.NetChannel Channel
        {
            get { return Lidgren.Network.NetChannel.ReliableInOrder1; }
        }
        public override void Read(Lidgren.Network.NetBuffer buffer)
        {
            base.Read(buffer);
            int length = buffer.ReadInt32();
                    if (length > 0)
                        AuthenticatedTicket = buffer.ReadBytes(length);
        }

        public override void Write(Lidgren.Network.NetBuffer buffer)
        {
            base.Write(buffer);
            int length;
                    if (AuthenticatedTicket == null)
                        length = 0;
                    else
                        length = AuthenticatedTicket.Length;

                    buffer.Write(length);
                    if (length > 0)
                        buffer.Write(AuthenticatedTicket);
        }
        #endregion
    }
}
