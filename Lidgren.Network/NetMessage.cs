using System;
using System.Diagnostics;
using System.Net;

namespace Lidgren.Network
{

    internal sealed class SUSystemMessage
    {
        public NetSystemType Type;
        public NetBuffer Data;
        public IPEndPoint Destination;
        public bool UseBroadcast;
    }

    public abstract class NetMessage
    {
        internal NetMessageType m_msgType;

        internal NetMessageLibraryType m_type;
        internal NetChannel m_sequenceChannel;
        internal int m_sequenceNumber = -1; // for fragmented messages
        protected const int BITS_PER_BYTE = 8; 

        internal NetBuffer m_buffer;

        public NetMessage()
        {
            m_msgType = NetMessageType.Data;
        }

        public NetMessageType Type { get { return m_msgType; }
        }

        public NetChannel Channel { get { return m_sequenceChannel; } }

        // TODO: why is this property int yet reading of the sequence number from buffer reads uint16?
        // NOTE: THis is not an incremental id for each message. This is an incremented ID for messages sent
        protected int SequenceNumber { get { return m_sequenceNumber; } }

        public NetBuffer Buffer {get { return m_buffer;}}
    }

    /// <summary>
    /// Copy's message in socket buffer to a new buffer owned solely by this object.
    /// Thus socket buffer's are always re-usable and never need to be re-allocated.
    /// TODO: in the future potentially we will see if here instead we can just read the header
    /// and then immediately grab a IRemotableType and have it read from the buffer immediately
    /// so that eliminates one copy.
    /// </summary>
	public sealed class IncomingNetMessage : NetMessage
	{
		public NetConnectionBase m_sender;
        public IPEndPoint m_senderEndPoint; // because when a connection is not yet established, an endpoint is still referenced with the message
        
		/// <summary>
		/// Read this message from the packet buffer.  
        /// This call assumes it will be able to read a full packet regardless of UDP or TCP. 
		/// </summary>
		/// <returns>new read pointer position</returns>
		internal bool ReadFrom(NetBuffer source, IPEndPoint endpoint) // Hypno - Nov.13.2009. Changed to a bool to indicate success/fail. Added payload length validity checks
		{
            
			m_senderEndPoint = endpoint;
            int startPosition = source.m_readPosition;

            // is there enough unread data in the buffer to read the 3 byte header + at least 1 byte paylen?
            if (source.LengthUnread >= (4 * BITS_PER_BYTE)) // Hypno - payload is variable Uint32, the actual amount read will be 1 - 4 bytes (8 - 32 bits)
            {
                // read 3 byte header.  This is the very first 3 bytes to ever arrive and always seperates every single message
                byte header = source.ReadByte();
                m_type = (NetMessageLibraryType)(header & 7);
                m_sequenceChannel = (NetChannel)(header >> 3);
                m_sequenceNumber = source.ReadUInt16(); // TODO: this reads a ushort but m_sequenceNumber is int32... is that wrong? will this eventually result in overflows or something ?

                uint payLen;
                uint actualBytesRead;
                // read 1 to 4 byte payload length
                // note: OnTCPDataReceived has already verified the paylen is correct before this ReadFrom() is ever called
                // however for OnUDPDataReceived it is not checked until here.
                if (source.TryReadVariableUInt32((uint)source.LengthUnread / BITS_PER_BYTE, out payLen, out actualBytesRead))
                {
                    // verify payLen wont result in a read past the end of the buffer (buffer overrun)
                    if (payLen * BITS_PER_BYTE <= source.LengthUnread)
                    {
                        if (payLen > 0)
                        {
                            // copy payload into message's own m_buffer
                            m_buffer.EnsureBufferSize((int)payLen * BITS_PER_BYTE);
                            source.ReadBytes(m_buffer.Data, 0, (int)payLen);
                            m_buffer.Reset(0, (int)payLen * BITS_PER_BYTE);
                            return true;
                        }
                        // empty payload.  This shouldnt happen but we can recover by doing nothing.  So we'll see if the next packet they send is ok.
                        m_buffer.Reset(0, 0);
#if DEBUG
                        System.Diagnostics.Trace.Assert(false, "WARNING: Error in sending application.  They just sent us an IRemotableType that has no user data in it. Only lidgren packet header.");
#endif
                        return true; 
                    }
#if DEBUG
                    else
                    {
                        Debug.WriteLine("Bad packet inner"); // for debug breakpoints only
                    }
#endif
                }
#if DEBUG
                else
                {
                    Debug.WriteLine("Could not read payload length"); // for debug breakpoints only
                }
#endif
            }
          
            // still here? Then this is a bad UDP packet (note: TCP packets are already verified good in OnTCPDataReceived so we know this is UDP)
            // So, read to the end of the buffer to move the position marker so we might try to continue and read the next seperate message
            source.ReadBytes(source.LengthUnread / BITS_PER_BYTE); 
            if (m_sender != null) m_sender.Statistics.CountBadPacketReceived(source.m_readPosition - startPosition, NetTime.Now); //if (m_sender != null) m_sender.Statistics.CountBadPacketReceived(buffer.LengthBytes, NetTime.Now); 
            return false;
		}

		public override string ToString()
		{
			if (m_type == NetMessageLibraryType.System)
				return "[Incoming " + (NetSystemType)m_buffer.Data[0] + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";

			return "[Incoming " + m_type + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";
		}
	}

	internal sealed class OutgoingNetMessage : NetMessage
	{
		internal int m_numSent;
		internal double m_nextResend;
		internal NetBuffer m_receiptData; // used to append Acks of previous messages.  not needed for TCP/IP

		internal void Encode(NetBuffer intoBuffer)
		{
			Debug.Assert(m_sequenceNumber != -1);

			// message type, netchannel and sequence number
			intoBuffer.Write((byte)((int)m_type | ((int)m_sequenceChannel << 3)));
			intoBuffer.Write((ushort)m_sequenceNumber);

			// payload length
			uint len = (uint)m_buffer.LengthBytes;
			intoBuffer.WriteVariableUInt32(len);

			// copy payload
			intoBuffer.Write(m_buffer.Data, 0, (int)len);

			return;
		}

		public override string ToString()
		{
			if (m_type == NetMessageLibraryType.System)
				return "[Outgoing " + (NetSystemType)m_buffer.Data[0] + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";

			return "[Outgoing " + m_type + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";
		}
	}
}
