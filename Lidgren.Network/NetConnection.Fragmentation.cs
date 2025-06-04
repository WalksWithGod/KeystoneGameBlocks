using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lidgren.Network
{
	internal sealed class FragmentedMessage
	{
		public int TotalFragments;
		public int FragmentsReceived;
		public int ChunkSize;
		public int BitLength;
		public byte[] Data;
	}

	public abstract partial class NetConnectionBase
	{
		protected ushort m_nextSendFragmentId;

		/// <summary>
		/// Identifier : Complete byte array
		/// </summary>
		private Dictionary<int, FragmentedMessage> m_fragments;

		protected void InitializeFragmentation()
		{
			m_fragments = new Dictionary<int, FragmentedMessage>();
			m_nextSendFragmentId = 1;
		}

		/// <summary>
		/// Called when a message should be released to the application
		/// </summary>
		protected void AcceptMessage(IncomingNetMessage msg)
		{
			if (msg.m_type == NetMessageLibraryType.UserFragmented)
			{
				// parse
				int id = msg.m_buffer.ReadUInt16();
				int number = (int)msg.m_buffer.ReadVariableUInt32(); // 0 to total-1
				int total = (int)msg.m_buffer.ReadVariableUInt32();

				int bytePtr = msg.m_buffer.Position / 8;
				int payloadLen = msg.m_buffer.LengthBytes - bytePtr;

				FragmentedMessage fmsg;
				if (!m_fragments.TryGetValue(id, out fmsg))
				{
					fmsg = new FragmentedMessage();
					fmsg.TotalFragments = total;
					fmsg.FragmentsReceived = 0;
					fmsg.ChunkSize = payloadLen;
					fmsg.Data = new byte[payloadLen * total];
					m_fragments[id] = fmsg;
				}

				// insert this fragment
				Array.Copy(
					msg.m_buffer.Data,
					bytePtr,
					fmsg.Data,
					number * fmsg.ChunkSize,
					payloadLen
				);

				fmsg.BitLength += (msg.m_buffer.m_bitLength - msg.m_buffer.Position);
				fmsg.FragmentsReceived++;

				m_messageManager.LogVerbose("Fragment " + id + " - " + (number+1) + "/" + total + " received; chunksize " + fmsg.ChunkSize + " this size " + payloadLen, this);

				if (fmsg.FragmentsReceived < fmsg.TotalFragments)
				{
					// Not yet complete
					return;
				}

				// Done! Release it as a complete message
			    NetBuffer buf = new NetBuffer(fmsg.Data);

				//int bitLen = (fmsg.TotalFragments - 1) * fmsg.ChunkSize * 8;
				//bitLen += msg.m_buffer.m_bitLength - (bytePtr * 8);

			    m_fragments.Remove(id);

				// reuse "msg"
                m_messageManager.LogVerbose("All fragments received; complete length is " + buf.LengthBytes, this);
				msg.m_buffer = buf;
			}

			// release
            m_messageManager.LogVerbose("Accepted " + msg, this);

			// Debug.Assert(msg.m_type == NetMessageLibraryType.User);

            // TODO: an event here
			// do custom handling on networking thread
		//	asyncbase.ProcessReceived(msg.m_buffer);
            m_messageManager.Enqueue(msg);
		}
	}
}
