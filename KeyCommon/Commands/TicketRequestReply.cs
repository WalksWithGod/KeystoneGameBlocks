using System;
using Lidgren.Network;


namespace KeyCommon.Commands
{
    //public class TicketRequestReply : NetCommandBase
    //{
    //    public byte[] AuthenticatedTicket;

    //    public TicketRequestReply()
    //    {
    //        mCommand = (int)Enumerations.Types.TicketReply;
    //    }

    //    #region IRemotableType Members
    //    public override void Read(NetBuffer buffer)
    //    {
    //        // login data
    //        int length = buffer.ReadInt32();
    //        if (length > 0)
    //            AuthenticatedTicket = buffer.ReadBytes(length);
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        int length;
    //        if (AuthenticatedTicket == null)
    //            length = 0;
    //        else
    //            length = AuthenticatedTicket.Length;

    //        buffer.Write(length);
    //        if (length > 0)
    //            buffer.Write(AuthenticatedTicket);
    //    }
    //    #endregion
    //}
}

  
