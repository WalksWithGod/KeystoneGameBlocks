using System;
using System.Collections.Generic;
using System.Net;

namespace Lidgren.Network
{
    internal class NetNATIntro
    {
        // TODO: i think maybe i could just make HandleNATIntroduction and such as a NetNATIntroduction class that can then be shared
        internal List<IPEndPoint> m_holePunches;
        private double m_lastHolePunch;
        private NetBufferManager m_bufferManager;
        private NetMessageManager m_messageManager;

        public NetNATIntro(NetMessageManager messageManager, NetBufferManager bufferManager)
        {
            if (messageManager == null) throw new ArgumentNullException();
            if (bufferManager == null) throw new ArgumentNullException();

            m_bufferManager = bufferManager;
            m_messageManager = messageManager;
        }

        public void HolePunchingMaintenance(NetSocket socket, double now)
        {
            // hole punching
            if (m_holePunches != null)
            {
                if (now > m_lastHolePunch + NetConstants.HolePunchingFrequency)
                {
                    if (m_holePunches.Count < 0)
                    {
                        m_holePunches = null;
                    }
                    else
                    {
                        IPEndPoint dest = m_holePunches[0];
                        m_holePunches.RemoveAt(0);
                        m_messageManager.NotifyApplication(NetMessageType.DebugMessage, "Sending hole punch to " + dest, null);
                        NetUDPConnection.SendPing(socket, dest, now);
                        if (m_holePunches.Count < 1)
                            m_holePunches = null;
                        m_lastHolePunch = now;
                    }
                }
            }
        }


        /// <summary>
        /// Stop any udp hole punching in progress towards ep
        /// </summary>
        public void CeaseHolePunching(IPEndPoint ep)
        {
            if (ep == null)
                return;

            int hc = ep.GetHashCode();
            if (m_holePunches != null)
            {
                for (int i = 0; i < m_holePunches.Count; )
                {
                    if (m_holePunches[i] != null && m_holePunches[i].GetHashCode() == hc)
                    {
                        m_messageManager.LogVerbose("Ceasing hole punching to " + m_holePunches[i]);
                        m_holePunches.RemoveAt(i);
                    }
                    else
                        i++;
                }
                if (m_holePunches.Count < 1)
                    m_holePunches = null;
            }
        }

        /// <summary>
        /// Returns true if message should be dropped
        /// See http://en.wikipedia.org/wiki/UDP_hole_punching
        /// for explanation on UDP hole punching.  Requires 3rd party to get working according to Mith.
        /// and then gave this url for paper explaining how to do it without a 3rd party
        /// http://www.brynosaurus.com/pub/net/p2pnat/
        /// Just bookmarking for now because it's sidetracking and i dont care for now...
        /// </summary>
        public bool HandleNATIntroduction(NetSocket socket, IncomingNetMessage message)
        {
            if (message.m_type == NetMessageLibraryType.System)
            {
                if (message.m_buffer.LengthBytes > 4 && message.m_buffer.PeekByte() == (byte)NetSystemType.NatIntroduction)
                {
                    if ((m_messageManager.EnabledMessageTypes & NetMessageType.NATIntroduction) != NetMessageType.NATIntroduction)
                        return true; // drop
                    try
                    {
                        message.m_buffer.ReadByte(); // step past system type byte
                        IPEndPoint presented = message.m_buffer.ReadIPEndPoint();

                        m_messageManager.LogVerbose("Received NATIntroduction to " + presented + "; sending punching ping...");

                        double now = NetTime.Now;
                        NetUDPConnection.SendPing(socket, presented, now);

                        if (m_holePunches == null)
                            m_holePunches = new List<IPEndPoint>();

                        for (int i = 0; i < 5; i++)
                            m_holePunches.Add(new IPEndPoint(presented.Address, presented.Port));

                        NetBuffer info = m_bufferManager.CreateBuffer();
                        info.Write(presented);
                        m_messageManager.NotifyApplication(NetMessageType.NATIntroduction, info, message.m_sender, message.m_senderEndPoint);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "Bad NAT introduction message received: " + ex.Message, message.m_sender, message.m_senderEndPoint);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
