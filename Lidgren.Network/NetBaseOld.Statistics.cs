/* Copyright (c) 2008 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
//#define USE_RELEASE_STATISTICS

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// Statistics per NetBase instance
	/// </summary>
	public partial class NetBaseOld
	{
		internal NetStatistics m_statistics;

		/// <summary>
		/// Statistics for this NetBase instance
		/// </summary>
		public NetStatistics Statistics { get { return m_statistics; } }

#if DEBUG || USE_RELEASE_STATISTICS
		public string GetStatisticsString(NetUDPConnection connection)
		{
			double now = NetTime.Now;
			string retval =
				"--- Application wide statistics ---" + Environment.NewLine +
				"Heartbeats: " + this.HeartbeatAverageFrequency + "/sec" + Environment.NewLine + 
				"Packets sent: " + m_statistics.PacketsSent + " (" + m_statistics.GetPacketsSentPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
				"Bytes sent: " + m_statistics.BytesSent + " (" + m_statistics.GetBytesSentPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
				"Packets received: " + m_statistics.PacketsReceived + " (" + m_statistics.GetPacketsReceivedPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
				"Bytes received: " + m_statistics.BytesReceived + " (" + m_statistics.GetBytesReceivedPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine;

			if (m_simulatedLoss > 0.0f)
				retval = retval +
					"Simulated dropped packets: " + m_statistics.SimulatedDroppedPackets + Environment.NewLine +
					"Simulated delayed packets: " + m_delayedPackets.Count + Environment.NewLine
				;

			if (connection != null)
			{
				NetConnectionStatistics connStats = connection.Statistics;
				retval += Environment.NewLine +
					"--- Connection wide statistics ---" + Environment.NewLine +
					"Status: " + connection.Status + Environment.NewLine +

					"Received -----" + Environment.NewLine +
					"Messages: " + connStats.GetMessagesReceived(true) + " (" + connStats.GetMessagesReceivedPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
					"  User/type: " +
					connStats.GetUserUnreliableReceived() + "/" +
					connStats.GetUserSequencedReceived() + "/" +
					connStats.GetUserReliableUnorderedReceived() + "/" +
					connStats.GetUserOrderedReceived() + Environment.NewLine +
					"Packets: " + connStats.PacketsReceived + Environment.NewLine +
					"Bytes: " + connStats.GetBytesReceived(true) + " (" + connStats.GetBytesReceivedPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
					"Acks: " + connStats.AcknowledgesReceived + Environment.NewLine +
					Environment.NewLine +

					"Sent ------" + Environment.NewLine +
					"Messages: " + connStats.GetMessagesSent(true) +
					" (excl. resends " + (connStats.GetMessagesSent(true) - connStats.MessagesResent) + ")" + Environment.NewLine +
					"  User/type: " +
					connStats.GetUserUnreliableSent() + "/" +
					connStats.GetUserSequencedSent() + "/" +
					connStats.GetUserReliableUnorderedSent() + "/" +
					connStats.GetUserOrderedSent() + Environment.NewLine +

					"Packets: " + connStats.PacketsSent + Environment.NewLine +
					"Bytes: " + connStats.GetBytesSent(true) + " (" + connStats.GetBytesSentPerSecond(now).ToString("N1") + "/sec)" + Environment.NewLine +
					"Acks: " + connStats.AcknowledgesSent + Environment.NewLine +
					Environment.NewLine +
					"Resent: " + connStats.MessagesResent + Environment.NewLine +
					"Duplicates: " + connStats.DuplicateMessagesRejected + Environment.NewLine +
					"Dropped sequenced: " + connStats.SequencedMessagesRejected + Environment.NewLine +
					Environment.NewLine +
					"Unsent messages: " + connection.m_unsentMessages.Count + Environment.NewLine + 
		// TODO: temporarily commented out next two lines wich deal with UDPConnection stored and withheld messages
        // TCPConnection has no need for these but until i properly update the stats classes, im just disabling those properties
        //			"Stored messages: " + connStats.CurrentlyStoredMessagesCount + Environment.NewLine +
		//			"Withheld messages: " + connStats.CurrentlyWithheldMessagesCount + Environment.NewLine +
					"Average roundtrip: " + (int)(connection.AverageRoundtripTime * 1000) + " ms" + Environment.NewLine
					//"Window: " + connection.GetEstimatedPacketsOnWire() + " of " + connection.m_congestionWindowSize
				;
			}
			return retval;
		}
#else
		public string GetStatisticsString(NetConnectionBase connection)
		{
			return "(Statistics not available in Release build)";
		}
#endif
	}
}
