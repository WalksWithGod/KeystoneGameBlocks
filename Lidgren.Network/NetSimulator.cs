using System;
using System.Collections.Generic;
using System.Net;

namespace Lidgren.Network
{
    internal class NetSimulator
    {
        private NetStatistics m_statistics;
        private NetSocket m_socket;
        private float m_simulatedLoss;
        private float m_simulatedMinimumLatency;
        private float m_simulatedLatencyVariance;
        private float m_simulatedDuplicateChance;

 
        /// <summary>
        /// Simulates chance for a packet to become lost in transit; 0.0f means no packets; 1.0f means all packets are lost
        /// </summary>
        public float SimulatedLoss { get { return m_simulatedLoss; } set { m_simulatedLoss = value; } }

        /// <summary>
        /// Simulates chance for a packet to become duplicated in transit; 0.0f means no packets; 1.0f means all packets are duplicated
        /// </summary>
        public float SimulatedDuplicateChance { get { return m_simulatedDuplicateChance; } set { m_simulatedDuplicateChance = value; } }

        /// <summary>
        /// Simulates minimum two-way latency, ie. roundtrip (in seconds) of outgoing packets
        /// </summary>
        public float SimulatedMinimumLatency { get { return m_simulatedMinimumLatency; } set { m_simulatedMinimumLatency = value; } }

        /// <summary>
        /// Simulates maximum amount of random variance (in seconds) in roundtrip latency added to the MinimumLatency
        /// </summary>
        public float SimulatedLatencyVariance { get { return m_simulatedLatencyVariance; } set { m_simulatedLatencyVariance = value; } }

        private List<DelayedPacket> m_delayedPackets = new List<DelayedPacket>();
        private bool m_suppressSimulatedLag;
        private List<DelayedPacket> m_removeDelayedPackets = new List<DelayedPacket>();

        public bool SuppressSimulatedLag { get { return m_suppressSimulatedLag; } set { m_suppressSimulatedLag = value; } }

        /// <summary>
        /// Simulates bad outgoing networking conditions - similar settings should be used on both server and client
        /// </summary>
        /// <param name="lossChance">0.0 means no packets dropped; 1.0 means all packets dropped</param>
        /// <param name="duplicateChance">0.0 means no packets duplicated; 1.0f means all packets duplicated</param>
        /// <param name="minimumLatency">the minimum roundtrip time in seconds</param>
        /// <param name="latencyVariance">the maximum variance in roundtrip time (randomly added on top of minimum latency)</param>
        public NetSimulator(
            float lossChance,
            float duplicateChance,
            float minimumLatency,
            float latencyVariance)
        {
            m_simulatedLoss = lossChance;
            m_simulatedDuplicateChance = duplicateChance;
            m_simulatedMinimumLatency = minimumLatency;
            m_simulatedLatencyVariance = latencyVariance;
        }

        internal NetSimulator(NetSocket net, NetStatistics statistics)
        {
            if (net == null) throw new ArgumentNullException();
            if (statistics == null) throw new ArgumentNullException();
            m_socket = net;
            m_statistics = statistics;
        }

        /// <summary>
        /// returns true if packet should be sent by calling code
        /// </summary>
        internal bool SimulatedSendPacket(byte[] data, int length, IPEndPoint remoteEP)
        {
            if (m_simulatedLoss > 0.0f)
            {
                if (NetRandom.Instance.NextFloat() < m_simulatedLoss)
                {
                    // packet was lost!
                    // LogWrite("Faking lost packet...");
                    m_statistics.CountSimulatedDroppedPacket();
                    return false;
                }
            }

            if (m_simulatedDuplicateChance > 0.0f)
            {
                if (NetRandom.Instance.NextFloat() < m_simulatedDuplicateChance)
                {
                    // duplicate; send one now, one after max variance
                    float first = m_simulatedMinimumLatency;
                    float second = (m_simulatedLatencyVariance > 0.0f ? m_simulatedLatencyVariance : 0.01f); // min 10 ms

                    DelayPacket(data, length, remoteEP, second);

                    if (first <= 0.0f)
                        return true; // send first right away using regular code

                    DelayPacket(data, length, remoteEP, first);
                    return false; // delay both copies
                }
            }

            if (m_simulatedMinimumLatency > 0.0f || m_simulatedLatencyVariance > 0.0f)
            {
                DelayPacket(
                    data, length, remoteEP,
                    (m_simulatedMinimumLatency * 0.5f) +
                    (m_simulatedLatencyVariance * NetRandom.Instance.NextFloat() * 0.5f)
                );
                return false;
            }

            // just send
            return true;
        }

        internal void DelayPacket(byte[] data, int length, IPEndPoint remoteEP, float delay)
        {
            DelayedPacket pk = new DelayedPacket();
            pk.Data = new byte[length];
            Buffer.BlockCopy(data, 0, pk.Data, 0, length);
            pk.Recipient = remoteEP;

            double now = NetTime.Now;
            pk.DelayedUntil = now + delay;
            m_delayedPackets.Add(pk);
        }

        internal void SendDelayedPackets(double now)
        {
            if (m_delayedPackets.Count < 1)
                return;

            m_removeDelayedPackets.Clear();
            foreach (DelayedPacket pk in m_delayedPackets)
            {
                if (now >= pk.DelayedUntil)
                {
                    m_suppressSimulatedLag = true;
                    m_socket.SendPacket(pk.Data, pk.Data.Length, pk.Recipient);
                    m_suppressSimulatedLag = false;
                    m_removeDelayedPackets.Add(pk);
                }
            }
            foreach (DelayedPacket pk in m_removeDelayedPackets)
                m_delayedPackets.Remove(pk);
        }
    }
}
