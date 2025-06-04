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
using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace Lidgren.Network
{
    /// <summary> 
    /// </summary>
    public class NetUDPServer :  NetServer
    {
        private NetNATIntro m_NATIntro;
        protected long m_outstandingReads;

        public NetUDPServer(NetConfiguration config)
            : base(config, ProtocolType.Udp)
        {
            m_NATIntro = new NetNATIntro(m_messageManager, m_bufferManager);
            m_outstandingReads = 0;
        }

        public void Initialize()
        {
            if (!m_socket.IsBound)
                Bind();

            // display simulated networking conditions in debug log
            if (m_socket.Simulator.SimulatedLoss > 0.0f)
                m_messageManager.LogWrite("Simulating " + (m_socket.Simulator.SimulatedLoss * 100.0f) + "% loss");
            if (m_socket.Simulator.SimulatedMinimumLatency > 0.0f || m_socket.Simulator.SimulatedLatencyVariance > 0.0f)
                m_messageManager.LogWrite("Simulating " + ((int)(m_socket.Simulator.SimulatedMinimumLatency * 1000.0f)) + " - " + ((int)((m_socket.Simulator.SimulatedMinimumLatency + m_socket.Simulator.SimulatedLatencyVariance) * 1000.0f)) + " ms roundtrip latency");
            if (m_socket.Simulator.SimulatedDuplicateChance > 0.0f)
                m_messageManager.LogWrite("Simulating " + (m_socket.Simulator.SimulatedDuplicateChance * 100.0f) + "% chance of packet duplication");

            if (m_config.m_throttleBytesPerSecond > 0)
                m_messageManager.LogWrite("Throtting to " + m_config.m_throttleBytesPerSecond + " bytes per second");
        }

        public override void ApproveConnection(double now, NetConnectionBase conn, byte[] hailData)
        {
            base.ApproveConnection(now, conn, hailData);

            // send response; even if connected
            OutgoingNetMessage response = m_messageManager.CreateSystemMessage(NetSystemType.ConnectResponse);

            if (conn.LocalHailData != null)
                response.m_buffer.Write(conn.LocalHailData);

            conn.m_unsentMessages.Enqueue(response);
        }

        /// <summary>
        /// Provides maintenance 
        /// NOTE: this comment is out of place but I wanted to add it here so as not to forget
        /// I like the model lidgren has chosen to use internal messages rather than events.
        /// These messages sent to itself which the application can then read and process
        /// like any other i think is a good idea because events and callbacks suck in a threaded environment.
        /// Realizing this helps me understand now what these NotifyApplication messages and such are for
        /// and why they were done htis way
        /// </summary>
        public override void Heartbeat()
        {
            base.Heartbeat();
            
            //m_NATIntro.HolePunchingMaintenance(m_socket, now);
            m_NATIntro.HolePunchingMaintenance(m_socket, NetTime.Now); // TODO: having to call .Now again is bad... base already does it.  I should probably just override and re-implment completely without calling base.Heartbeat()
            OutstandingAsychronousReadMaintenance();
        }


        // only UDP needs this... and perhaps only udp servers that follow this ascyn model
        // recall taht its not absolutely necessary to do it this way, although it's ideal.  Original lidgren method is not as scaleable.
        protected void OutstandingAsychronousReadMaintenance()
        {

            // create  outstanding asychronous reads on the socket
            // to cancel pending receives, call socket.Close();
            while (m_outstandingReads < m_config.m_maxOutstandingReads)
            {
                //This method reads data into the buffer parameter, and captures the remote host endpoint 
                //from which the data is sent, as well as information about the received packet. 
                //For information on how to retrieve this endpoint, refer to EndReceiveFrom. This method is 
                //most useful if you intend to asynchronously receive connectionless datagrams from an 
                //unknown host or multiple hosts.
                //m_socket.BeginReceiveMessageFrom(buffer, offset, size, flags, ref remoteEP, callback, objState);
                // m_socket.BeginReceiveFrom(buffer, offset, size, flags, ref remoteEP, callback, objState);
                // BeginReceive is not useful because we cant get the endpoints.  Its better used for TCP sockets
                // m_socket.BeginReceive(buffer, offset, size, flags, out errorCode, callback, objState);
                NetBuffer buffer = m_bufferManager.CreateBuffer(m_config.ReceiveBufferSize);
                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs
                {
                    SocketFlags = SocketFlags.None,
                    UserToken = buffer,
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, m_config.Port) // TODO: is it correct to use m_config.Port?  I guess yes since it's UDP and we're receiving from just one port?
                };
                eventArgs.Completed += OnReadCompleted;
                eventArgs.SetBuffer(buffer.Data, 0, m_config.ReceiveBufferSize);
                //eventArgs.Offset 
                //eventArgs.ReceiveMessageFromPacketInfo 

                //eventArgs.DisconnectReuseSocket  
                //eventArgs.SendPacketsElements 
                //eventArgs.SendPacketsFlags 
                //eventArgs.SendPacketsSendSize 
                //eventArgs.LastOperation
                //eventArgs.SocketError 

                Interlocked.Increment(ref m_outstandingReads);

                if (!m_socket.Socket.ReceiveMessageFromAsync(eventArgs))
                    OnReadCompleted(m_socket.Socket, eventArgs);

                // m_socket.ReceiveFromAsync(eventArgs);
                //  m_socket.ReceiveAsync(eventArgs);
            }
        }

        private void OnReadCompleted(object obj, SocketAsyncEventArgs args)
        {
            Interlocked.Decrement(ref m_outstandingReads);
            OnUDPDataReceived(m_socket.Socket, args);
        }

        // TODO: this handles messages received from the shared UDP port.  The TCPServer wont have much use for this since
        //         there each connection should handle it's own messages.
        //         I'll have to consider this a bit more.
        // this handler is a mixed bag somewhat... it handles system stuff and a lot of that should be transparently handled
        // including connection negotiation, but what is really needed is for there to be a settings/flags to help us shape
        // the handling a bit so it works for a broad group of things without having to override the behavior elsewhere each time.
        internal override void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint)
        {
            try
            {
                double now = NetTime.Now;

                int payLen = message.m_buffer.LengthBytes;

                // NAT introduction?
                if (m_NATIntro.HandleNATIntroduction(m_socket, message))
                    return;

                // Out of band?
                if (message.m_type == NetMessageLibraryType.OutOfBand)
                {
                    // m_messageManager.HandleOBB()
                    if ((m_messageManager.EnabledMessageTypes & NetMessageType.OutOfBandData) != NetMessageType.OutOfBandData)
                        return; // drop

                    // just deliever
                    message.m_msgType = NetMessageType.OutOfBandData;
                    message.m_senderEndPoint = senderEndpoint;
                    m_messageManager.Enqueue(message);

                }

                // do we already have an established NetUDPConnection?
                if (message.m_sender == null)
                {
                    // not a connected sender; only allow System messages
                    if (message.m_type != NetMessageLibraryType.System)
                    {
                        if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                            NetMessageType.BadMessageReceived)
                        {
                            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                               "Rejecting non-system message from unconnected source: " +
                                                               message, null, message.m_senderEndPoint);
                            return;
                        }
                    }

                    // read type of system message
                    NetSystemType sysType = (NetSystemType)message.m_buffer.ReadByte();
                    switch (sysType)
                    {
                        case NetSystemType.Connect:
                            ProcessConnectRequest(message, payLen, m_socket, senderEndpoint, now);
                            return; // return and not break is important

                        case NetSystemType.ConnectionEstablished:
                            if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                NetMessageType.BadMessageReceived)
                                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                   "Connection established received from non-connection! Ignoring..." +
                                                                   senderEndpoint, null, senderEndpoint);
                            return;
                        case NetSystemType.Discovery:
                            if (m_config.AnswerDiscoveryRequests)
                                m_discovery.HandleRequest(message, senderEndpoint);
                            break;
                        case NetSystemType.DiscoveryResponse:
                            if (m_allowOutgoingConnections)
                            {
                                // NetPeer
                                IncomingNetMessage resMsg = m_discovery.HandleResponse(message, senderEndpoint);
                                if (resMsg != null)
                                {
                                    resMsg.m_senderEndPoint = senderEndpoint;
                                    m_messageManager.Enqueue(resMsg);
                                }
                            }
                            break;
                        default:
                            if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                NetMessageType.BadMessageReceived)
                                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                   "Undefined behaviour for " + this +
                                                                   " receiving system type " + sysType + ": " + message +
                                                                   " from unconnected source", null, senderEndpoint);
                            break;
                    }
                }
                else // m_sender Connection object is not null
                {
                    //m_sender.HandleConnectedReceivedMessage();

                    // ok, we already have an established  connection 
                    if (message.m_type == NetMessageLibraryType.Acknowledge)
                    {
                        // TODO: this message type should never arrive for a TCPCOnnection.  The proper way to fix this bug
                        // is for this NetServer to really be a NetUDPServer and so there's a whole class of messages that never
                        // need to be delt with such as Acks
                        ((NetUDPConnection)message.m_sender).HandleAckMessage(message);
                        return;
                    }

                    if (message.m_type == NetMessageLibraryType.System)
                    {
                        //
                        // Handle system messages from connected source
                        //
                        if (payLen < 1)
                        {
                            if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                NetMessageType.BadMessageReceived)
                                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                   "Received malformed system message; payload length less than 1 byte",
                                                                   message.m_sender, senderEndpoint);
                            return;
                        }
                        NetSystemType sysType = (NetSystemType)message.m_buffer.ReadByte();
                        switch (sysType)
                        {
                            case NetSystemType.Connect:
                            case NetSystemType.ConnectionEstablished:
                            case NetSystemType.Disconnect:
                            case NetSystemType.ConnectionRejected:
                            case NetSystemType.Ping:
                            case NetSystemType.Pong:
                            case NetSystemType.StringTableAck:
                                message.m_sender.HandleSystemMessage(message, now);
                                break;
                            case NetSystemType.ConnectResponse:
                                if (m_allowOutgoingConnections)
                                {
                                    message.m_sender.HandleSystemMessage(message, now);
                                }
                                else
                                {
                                    if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                        NetMessageType.BadMessageReceived)
                                        m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                           "Undefined behaviour for server and system type " +
                                                                           sysType, message.m_sender, senderEndpoint);
                                }
                                break;
                            case NetSystemType.Discovery:
                                // Allow discovery even if connected
                                if (m_config.AnswerDiscoveryRequests)
                                    m_discovery.HandleRequest(message, senderEndpoint);
                                break;
                            default:
                                if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                    NetMessageType.BadMessageReceived)
                                    m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                       "Undefined behaviour for server and system type " +
                                                                       sysType, message.m_sender, senderEndpoint);
                                break;
                        }
                    }
                    else // message type is not a system message, must be a user message
                        ((NetUDPConnection)message.m_sender).HandleUserMessage(message);
                }
            }
            catch (Exception ex)
            {
                throw new NetException("HandleReceivedMessage() exception", ex);
            }
        }
                

        #region callback event handlers
        protected override void OnConnectionConnected(NetConnectionBase connection)
        {
            // no need for further hole punching
            m_NATIntro.CeaseHolePunching(connection.RemoteEndpoint);
        }

        /// <summary>
        /// Callback for a connection request message
        /// </summary>
        protected virtual void HandleConnect()
        {

        }

        protected virtual void HandleConnectionEstablished()
        {

        }

        protected virtual void HandleDiscovery()
        {

        }

        protected virtual void HandleDiscoveryResponse()
        {

        }

        protected virtual void HandleDefault()
        {

        }
        #endregion

    }
}
