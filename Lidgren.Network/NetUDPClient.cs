using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Lidgren.Network
{
    public class NetUDPClient : NetClient 
    {
        public NetUDPClient(NetConfiguration config)
            : base(config, ProtocolType.Udp)
        {
        }
        
        /// <summary>
        /// Connects to the specified remote endpoint; passing hailData to the server
        /// </summary>
        public override void Connect(IPEndPoint remoteEndpoint, byte[] hailData)
        {
            m_connectEndpoint = remoteEndpoint;
            m_localHailData = hailData;

            if (m_serverConnection != null)
            {
                m_serverConnection.Disconnect("New connect", 0, m_serverConnection.Status == NetConnectionStatus.Connected, true);
                if (m_serverConnection.RemoteEndpoint.Equals(m_connectEndpoint))
                    m_serverConnection = new NetUDPConnection(m_socket, m_messageManager, m_config, m_connectEndpoint, m_localHailData, null);
            }
            else
            {
                m_serverConnection = new NetUDPConnection(m_socket, m_messageManager, m_config, m_connectEndpoint, m_localHailData, null);
            }

            // connect
            m_serverConnection.Connect();

            m_connectEndpoint = null;
            m_localHailData = null;
        }

        public override void Heartbeat()
        {
            base.Heartbeat();
            
            // discovery - TODO: this is disabled and m_discovery is currently not initialized so you'll get a null reference if you uncomment.
           // m_discovery.Heartbeat(m_heartbeatCounter.LastCount);
        }

        internal override void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint)
        {
            //LogWrite("NetClient received message " + message);
            double now = NetTime.Now;

            int payLen = message.m_buffer.LengthBytes;

            // Discovery response?
            if (message.m_type == NetMessageLibraryType.System && payLen > 0)
            {
                NetSystemType sysType = (NetSystemType)message.m_buffer.PeekByte();

                // NAT introduction?
                if (m_NATIntro.HandleNATIntroduction(m_socket, message))
                    return;

                if (sysType == NetSystemType.DiscoveryResponse)
                {
                    message.m_buffer.ReadByte(); // step past system type byte
                    IncomingNetMessage resMsg = m_discovery.HandleResponse(message, senderEndpoint);
                    if (resMsg != null)
                    {
                        resMsg.m_senderEndPoint = senderEndpoint;
                        m_messageManager.Enqueue(resMsg);
                    }
                    return;
                }
            }

            // Out of band?
            if (message.m_type == NetMessageLibraryType.OutOfBand)
            {
                if ((m_messageManager.m_enabledMessageTypes & NetMessageType.OutOfBandData) != NetMessageType.OutOfBandData)
                    return; // drop

                // just deliever
                message.m_msgType = NetMessageType.OutOfBandData;
                message.m_senderEndPoint = senderEndpoint;
                m_messageManager.Enqueue(message);
                return;
            }

            if (message.m_sender != m_serverConnection && m_serverConnection != null)
                return; // don't talk to strange senders after this

            if (message.m_type == NetMessageLibraryType.Acknowledge)
            {
                ((NetUDPConnection)m_serverConnection).HandleAckMessage(message);
                return;
            }

            // Handle system types
            if (message.m_type == NetMessageLibraryType.System)
            {
                if (payLen < 1)
                {
                    if ((m_messageManager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                        m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "Received malformed system message: " + message, m_serverConnection, senderEndpoint);
                    return;
                }
                NetSystemType sysType = (NetSystemType)message.m_buffer.Data[0];
                switch (sysType)
                {
                    case NetSystemType.ConnectResponse:
                    case NetSystemType.Ping:
                    case NetSystemType.Pong:
                    case NetSystemType.Disconnect:
                    case NetSystemType.ConnectionRejected:
                    case NetSystemType.StringTableAck:
                        if (m_serverConnection != null)
                            m_serverConnection.HandleSystemMessage(message, now);
                        return;
                    case NetSystemType.Connect:
                    case NetSystemType.ConnectionEstablished:
                    case NetSystemType.Discovery:
                    case NetSystemType.Error:
                    default:
                        if ((m_messageManager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "Undefined behaviour for client and " + sysType, m_serverConnection, senderEndpoint);
                        return;
                }
            }

            Debug.Assert(
                message.m_type == NetMessageLibraryType.User ||
                message.m_type == NetMessageLibraryType.UserFragmented
            );

            if (m_serverConnection.Status == NetConnectionStatus.Connecting)
            {
                // lost connectresponse packet?
                // Emulate it; 
                m_messageManager.LogVerbose("Received user message before ConnectResponse; emulating ConnectResponse...", m_serverConnection);
                IncomingNetMessage emuMsg = m_messageManager.CreateIncomingMessage();
                emuMsg.m_type = NetMessageLibraryType.System;
                emuMsg.m_buffer.Reset();
                emuMsg.m_buffer.Write((byte)NetSystemType.ConnectResponse);
                m_serverConnection.HandleSystemMessage(emuMsg, now);

                // ... and proceed to pick up user message
            }

            // add to pick-up queue
            ((NetUDPConnection)m_serverConnection).HandleUserMessage(message);
        }

    }
}
