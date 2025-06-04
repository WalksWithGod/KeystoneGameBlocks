using System;

namespace Lidgren.Network
{
	public sealed partial class NetUDPConnection
	{
		/// <summary>
		/// Disapprove the connection, rejecting it.
		/// </summary>
		public void Disapprove(string reason)
		{
			if (m_approved == true)
				throw new NetException("Connection is already approved!");

            // send connectionrejected
            NetBuffer buf = new NetBuffer(reason);
            m_messageManager.QueueSingleUnreliableSystemMessage(
                NetSystemType.ConnectionRejected,
                buf,
                m_remoteEndPoint,
                false
            );

            m_requestDisconnect = true;
            m_requestLinger = 0.0f;
            m_requestSendGoodbye = !string.IsNullOrEmpty(reason);
            m_futureDisconnectReason = reason;
		}
	}
}
