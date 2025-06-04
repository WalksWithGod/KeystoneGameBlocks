using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Lidgren.Network
{
    public class NetTCPClient : NetClient
    {
        private int mOutstandingReceives = 0;

        public NetTCPClient(NetConfiguration config)
            : base(config, ProtocolType.Tcp)
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
                m_serverConnection.Disconnect("NetTCPClient.Connect() - New connect", 0, m_serverConnection.Status == NetConnectionStatus.Connected, true);
                
                // Hypno Feb.16.2010 - Not sure why this is here.  Makes no sense.
                //if (m_serverConnection.RemoteEndpoint.Equals(m_connectEndpoint))
                //    m_serverConnection = new NetTCPConnection(m_socket, m_messageManager, m_config, m_connectEndpoint, m_localHailData, null);
            }
            
            m_serverConnection = new NetTCPConnection(m_socket, m_messageManager, m_config, m_connectEndpoint, m_localHailData, null);
            
            // connect here actually calls the overloaded NetTCPConnection.Connect() but if vs.net tends to take you to the base.Connect()
            m_serverConnection.Connect();
            m_connectEndpoint = null;
            m_localHailData = null;
        }

        public override void Disconnect(string message)
        { 
            try
            {
                m_serverConnection.m_socket.Socket.Shutdown(SocketShutdown.Both); // makes sure remaining data is flushed.
                m_serverConnection.m_socket.Socket.Close(0); // releases unamanged and managed resources.  Do not re-cycle socket after this.
                // m_serverConnection.m_socket.Socket.Disconnect(true); <-- don't use this.  For some reason it blocks and causes problems
                m_serverConnection.SetStatus(NetConnectionStatus.Disconnected, message);
            }
            catch (SocketException ex)
            {
                Debug.Assert (false, "NetTCPClient.Disconnect() - " + ex.Message + "Error code = " + ex.ErrorCode.ToString ());
            }
        }

        public override void Heartbeat()
        {
            // if (m_serverConnection != null) m_serverConnection.Heartbeat(NetTime.Now); // TODO: verify if we're forgetting to have ConnectionResponse/Established... cuz i think its still getting stuck on "Connecting" and not transitioning to "Connected" on the server side for NetConnectionStatus
            // TODO:  hackish just to get the _initial_ read on the connected socket going
            if (mOutstandingReceives == 0)
            {
                if (m_serverConnection != null)
                    //if (m_serverConnection.Status == NetConnectionStatus.Connecting || m_serverConnection.Status == NetConnectionStatus.Connected)
                    //if (m_serverConnection.m_socket.Socket.IsBound )
                    if (m_serverConnection.m_socket.Socket.Connected )
                    {
                        mOutstandingReceives++;
                        InitiateTCPReceive(m_serverConnection, false, 0);
                        Debug.WriteLine("NetTCPClient.Heartbeat() - Initial IOCP receive initiated...");
                    }
            }
        }

        internal override void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint)
        {
            //LogWrite("NetClient received message " + message);
            double now = NetTime.Now;

            int payLen = message.m_buffer.LengthBytes;

            // TODO: this should be impossible with TCP, but no harm leaving it
            if (message.m_sender != m_serverConnection && m_serverConnection != null)
                return; // don't talk to strange senders after this


            // TCP does not deal with NAT or peer discovery packets so any system message received should be connection related
            if (message.m_type == NetMessageLibraryType.System)
            {
                if (payLen < 1)
                {
                    if ((m_messageManager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                        m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "NetTCPClient.HandleReceivedMessage() -Received malformed system message: " + message, m_serverConnection, senderEndpoint);
                    return;
                }
                NetSystemType sysType = (NetSystemType)message.m_buffer.Data[0];
                Debug.WriteLine ("NetTCPClient.HandleReceivedMessage() - System Message Type - " + sysType.GetType().Name);
                switch (sysType)
                {
                    case NetSystemType.ConnectResponse:
                    case NetSystemType.ConnectionEstablished:
                    case NetSystemType.Ping:
                    case NetSystemType.Pong:
                    case NetSystemType.Disconnect:
                    case NetSystemType.ConnectionRejected:
                        message.m_sender.HandleSystemMessage(message, now);
                        break;
                        
                    case NetSystemType.Connect: // NetTCPClient can't handle Connect messages
                    case NetSystemType.Error:
                    default:
                        // since we're already connected, any system message that is received is effectively a bad message
                        if ((m_messageManager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "NetTCPClient.HandleReceivedMessage() -Undefined behaviour for client and " + sysType, m_serverConnection, senderEndpoint);
                        break;
                }
            }
            else if (message.m_type == NetMessageLibraryType.User) // no NetMessageLibraryType.UserFragmented type for TCP
            {
                // add to pick-up queue
                m_messageManager.Enqueue(message);
            }
            else
            {
                Debug.Assert((message.m_type == NetMessageLibraryType.None && message.m_buffer.m_bitLength ==0) || message.m_type == NetMessageLibraryType.User ||message.m_type == NetMessageLibraryType.UserFragmented);
                if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                    NetMessageType.BadMessageReceived)
                    m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                       "NetTCPClient.HandleReceivedMessage() -Undefined behaviour for client and message type " +
                                                       message.m_type, null, senderEndpoint);
            }
        }
    }
}
