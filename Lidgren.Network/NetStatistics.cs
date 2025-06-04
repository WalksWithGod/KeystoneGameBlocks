using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Network
{
    /// <summary>
    /// Statistics for a NetBase derived class.  The difference between NetConnectionStatistics and NetStatistics is that
    /// NetConnectionStatistics is for each individual connection and NetStatistics is a particular Socket.  With UDP
    /// NetStatistics serves as the cumulative stats for all connections using that socket.
    /// </summary>
    public sealed class NetStatistics
    {
        private double m_startTimestamp;

        private long m_packetsSent;
        private long m_packetsReceived;
        private long m_bytesSent;
        private long m_bytesReceived;

        private long m_simDropped;

        /// <summary>
        /// Gets the number of packets received
        /// </summary>
        public long PacketsReceived { get { return m_packetsReceived; } }

        /// <summary>
        /// Gets the number of packets sent
        /// </summary>
        public long PacketsSent { get { return m_packetsSent; } }

        /// <summary>
        /// Gets the number of bytes received
        /// </summary>
        public long BytesReceived { get { return m_bytesReceived; } }

        /// <summary>
        /// Gets the number of artificially dropped packets
        /// </summary>
        public long SimulatedDroppedPackets { get { return m_simDropped; } }

        /// <summary>
        /// Gets the number of bytes sent
        /// </summary>
        public long BytesSent { get { return m_bytesSent; } }

        /// <summary>
        /// Gets the number of packets sent per second
        /// </summary>
        public float GetPacketsSentPerSecond(double now)
        {
            return (float)((double)m_packetsSent / (now - m_startTimestamp));
        }

        /// <summary>
        /// Gets the number of bytes sent per second
        /// </summary>
        public float GetBytesSentPerSecond(double now)
        {
            return (float)((double)m_bytesSent / (now - m_startTimestamp));
        }

        /// <summary>
        /// Gets the number of packets received per second
        /// </summary>
        public float GetPacketsReceivedPerSecond(double now)
        {
            return (float)((double)m_packetsReceived / (now - m_startTimestamp));
        }

        /// <summary>
        /// Gets the number of bytes received per second
        /// </summary>
        public float GetBytesReceivedPerSecond(double now)
        {
            return (float)((double)m_bytesReceived / (now - m_startTimestamp));
        }

        public NetStatistics()
        {
            Reset();
        }

        /// <summary>
        /// Resets all statistics, including starting timestamp
        /// </summary>
#if !USE_RELEASE_STATISTICS
        [Conditional("DEBUG")]
#endif
        public void Reset()
        {
            m_startTimestamp = NetTime.Now;
            m_packetsSent = 0;
            m_packetsReceived = 0;
            m_bytesSent = 0;
            m_bytesReceived = 0;
        }

#if !USE_RELEASE_STATISTICS
        [Conditional("DEBUG")]
#endif
        public void CountPacketSent(int numBytes)
        {
            m_packetsSent++;
            m_bytesSent += numBytes;
        }

#if !USE_RELEASE_STATISTICS
        [Conditional("DEBUG")]
#endif
        public void CountPacketReceived(int numBytes)
        {
            m_packetsReceived++;
            m_bytesReceived += numBytes;
        }

#if !USE_RELEASE_STATISTICS
        [Conditional("DEBUG")]
#endif
        public void CountSimulatedDroppedPacket()
        {
            m_simDropped++;
        }
    }
}
