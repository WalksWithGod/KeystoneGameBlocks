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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace Lidgren.Network
{
    /// <summary>
    /// A TCP server which can accept connections from multiple NetTCPClients
    /// </summary>
    public class NetTCPServer : NetServer
    {
        private NetSocketPool m_socketPool;
        private int m_outstandingAccepts;

        /// <summary>
        /// Creates a new NetUDPServer
        /// </summary>
        public NetTCPServer(NetConfiguration config)
            : base(config, ProtocolType.Tcp)
        {
            m_socketPool = new NetSocketPool(5000);
            m_outstandingAccepts = 0;
        }

        #region callback event handlers
        private void OnConnectionConnected(NetConnectionBase connection)
        {
           
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

        protected virtual void HandleDefault()
        {

        }
        #endregion


        /// <summary>
        /// Reads and sends messages from the network
        /// </summary>
        public override void Heartbeat()
        {
            // TODO: currently the base class heartbeat calls connectionmaintenance for udp ascych reads which is incompatible
            base.Heartbeat();
            ConnectionMaintenance();
        }

        private void ConnectionMaintenance()
        {
            // TODO: maintenance should run at an interval, not every itteration?  

            // ensure we have enough ready connections in the pool
            // TODO: for now we wont use a pool and we'll just create them on the fly
            while (m_outstandingAccepts < m_config.m_maxOutstandingAccepts)
            {
                // TODO: this should actually make sure that the m_connections are active clients and ignore admins, spectators, etc?
                // this check is probably a bad idea, but the thinking is you dont want to have max outstanding connections once the game is 
                // totally full becuase the server becomes more succeptible to DoS
               // if (m_outstandingAccepts > (m_config.MaxConnections - m_connections.Count)) break;
  
                InitiateAccept();
            }
        }

        /// <summary>
        /// AcceptConnection is called when socket.AcceptAsync(e) completes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private void AcceptConnection(object obj, SocketAsyncEventArgs args)
        {
            Interlocked.Decrement(ref m_outstandingAccepts);
            int bytesReceived = args.BytesTransferred;
            if (bytesReceived <= 0)
            {
                // NOTE: Since we often will have pending "accepts" since we always want some pending so we can accept new
                // client connections, however that means that we'll see the following in console/debug.  It's not a bug at all.  
                // It's just part of graceful shutdown of those pending accepts.
                string message = "AcceptConnection completed but with 0 data.  Ignoring connection attempt";
                m_messageManager.LogVerbose(message);
                // TODO: shouldn't we be actively rejecting the connection here or is it ok to let it just go unhandled?
                // i think it's fine to ignore it because there is no Connection object made at this point
                args.SetBuffer(null, 0, 0);
                args.Dispose();

                return; // no message exists and via Lidgren, we require at least a "Connect" system message to be included}
            }

            // grab the buffer object and set it's lengthbits 
            NetSocket netSocket = (NetSocket)args.UserToken;
            NetBuffer buffer = netSocket.m_sendBuffer;
            buffer.LengthBits = bytesReceived * 8;
            SocketError err = args.SocketError;
            args.SetBuffer(null, 0, 0);
            args.Dispose(); // memory leak if this is not done

            switch (err)
            {
                case (SocketError.Success ):

                    // TODO: verify that our TCPClient can "Connect" with just a Connect system message and no haildata included if ConnectionApproval = false
                    //          if (m_messageManager.EnabledMessageTypes & NetMessageType.ConnectionApproval)

                    // TODO: we shoudl parse to see if there is more than one message in the incoming buffer?  No i think we should limit
                    // to just one as part of our protocol. I think for TCP/IP the data that comes with "accept" arrives in a single "packet" (TODO: verify this)
                    IncomingNetMessage msg = m_messageManager.CreateIncomingMessage();
                    if (!msg.ReadFrom(buffer, null))
                    {
                        m_messageManager.LogVerbose("NetTCPServer:AcceptConnection() - Invalid incoming message.  Ignoring connection attempt");
                        return; // invalid message, return without accepting connection
                    }
                    buffer.Reset(); // reset the buffer for this socket now that it's read into the message // note: Reset() does not re-init the buffer, only resets the position counters

                    NetSystemType systemType = (NetSystemType)msg.Buffer.ReadByte();
                    // Hypnotron Sept.1.2011
                    // The old Debug.Assert (systemType == NetSystemType.Connect)
                    // evaluates to False every now and then in Lenn's Brokerage server even when
                    // no client is trying to connect.  So somehow some bit of data is being sent
                    // that the AcceptEx call thinks is a connect request.  Seems to me 
                    // it could be a port probe/hack of some sort... in other words
                    // a connection attempt NOT by any of our clients. 
                    // so instead of the old Debug.Assert, we'll test the systemType and
                    // return after notifying the user code for the server app if it's 
                    // incorrect because there's no reason AT ALL the normal
                    // client would create a connect packet incorrectly!
                    // Debug.Assert(systemType == NetSystemType.Connect);
                    if (systemType != NetSystemType.Connect)
                    {
                        m_messageManager.NotifyApplication(NetMessageType.ConnectionRejected, msg.Buffer, null, msg.m_senderEndPoint);
                        return;
                    }
                        

                    ProcessConnectRequest(msg, bytesReceived, netSocket, (IPEndPoint)netSocket.Socket.RemoteEndPoint, NetTime.Now);
                    return;
                
                default:
                    m_messageManager.NotifyApplication(NetMessageType.ConnectionRejected, buffer, null);
                    break;
            }
        }

        public override void ApproveConnection(double now, NetConnectionBase conn, byte[] hailData)
        {
            base.ApproveConnection(now, conn, hailData);

            // send the connection response directly on the socket, whereas UDP we use the global socket
            OutgoingNetMessage response = m_messageManager.CreateSystemMessage(NetSystemType.ConnectResponse);
            if (conn.LocalHailData != null)
                response.m_buffer.Write(conn.LocalHailData);


            response.Encode(conn.m_socket.m_sendBuffer);
            
            ((NetTCPConnection) conn).SendSocketData();

            InitiateReceive(conn);
        }

        /// <summary>
        /// Re-use the passed in SocketAsyncEventArgs
        /// </summary>
        /// <param name="e"></param>
        private void InitiateAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref m_outstandingAccepts);
            // I think I prefer that buffer's not be attached/associated to the socket permanently.  It's best to just
            // do that here in mainteance and in data reads and such.  TODO: in fact, we'll see if with TCP we can even
            // eventually remove the send buffer since we'll just be using the application buffer and we avoid having extra copies
            // from app buffer to socket buffer.
            NetSocket netSocket = NetSocket.Create(m_config.m_sendBufferSize, m_config.m_receiveBufferSize, ProtocolType.Tcp);
            
            e.UserToken = netSocket;
            e.SetBuffer(netSocket.m_sendBuffer.Data, 0, netSocket.m_sendBuffer.Data.Length);
            e.AcceptSocket = netSocket.Socket;
            e.Completed += AcceptConnection; 

            if (!m_socket.AcceptAsync(e)) // returns false if the operation succeeds immediately
                AcceptConnection(e.AcceptSocket, e);
        }

        private void InitiateAccept()
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            InitiateAccept(e);
        }

        internal void InitiateReceive(NetConnectionBase conn)
        {
            InitiateTCPReceive(conn, false, 0);
        }

        // this handler is a mixed bag somewhat... it handles system stuff and a lot of that should be transparently handled
        // including connection negotiation, but what is really needed is for there to be a settings/flags to help us shape
        // the handling a bit so it works for a broad group of things without having to override the behavior elsewhere each time.
        internal override void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint)
        {
            try
            {
                double now = NetTime.Now;

                int payLen = message.m_buffer.LengthBytes;

                // m_sender is a NetTCPConnection object and will never be null for TCP, although the status may be .Disconnected or .Connecting, etc
                // because the m_sender is established in the AcceptConnection AcceptEx completion handler.  Thus we have to do 
                // the connect response sending there
                if (message.m_sender == null) 
                {
                    Debug.Assert(false, "Send should never be null for NetTCPConnection!!!"); 
                    //this entire block should be deleted and just the return kept.  
                    return;
                   #region NotNeededForTCP
                    //// not a connected sender; only allow System messages
                    //if (message.m_type != NetMessageLibraryType.System)
                    //{
                    //    if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                    //        NetMessageType.BadMessageReceived)
                    //    {
                    //        m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                    //                                           "Rejecting non-system message from unconnected source: " +
                    //                                           message, null, message.m_senderEndPoint);
                    //        return;
                    //    }
                    //}
                    //
                    //// read type of system message
                    //NetSystemType sysType = (NetSystemType)message.m_buffer.ReadByte();
                    //switch (sysType)
                    //{
                    //    case NetSystemType.Connect:
                    //        Debug.Assert(false, "This line should never be reached...  ProcessConnectRequest is called from this.AcceptConnection()");
                    //        //ProcessConnectRequest(message, payLen, m_socket, senderEndpoint, now);
                    //        return; // return and not break is important 
                    //    case NetSystemType.ConnectionEstablished:
                    //        if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                    //            NetMessageType.BadMessageReceived)
                    //            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                    //                                               "Connection established received from non-connection! Ignoring..." +
                    //                                               senderEndpoint, null, senderEndpoint);
                    //        return;
                    //    default:
                    //        if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                    //            NetMessageType.BadMessageReceived)
                    //            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                    //                                               "Undefined behaviour for " + this +
                    //                                               " receiving system type " + sysType + ": " + message +
                    //                                               " from unconnected source", null, senderEndpoint);
                    //        break;
                    //}
                   #endregion
                }
                else // m_sender is not null so there is a connection associated with this message
                {
                
                    // Handle system messages from connected source
                    if (message.m_type == NetMessageLibraryType.System)
                    {
                        if (payLen < 1)
                        {
                            if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                NetMessageType.BadMessageReceived)
                                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                   "Received malformed system message; payload length less than 1 byte",
                                                                   null, senderEndpoint);
                            return;
                        }
                        // any system message received while connected is either ignored (depending on EnabledMessageTypes policy)
                        // or notified to the application for logging purposes but is _NOT_ 
                        NetSystemType sysType = (NetSystemType)message.m_buffer.ReadByte();
                        switch (sysType)
                        {
                            case NetSystemType.Connect:
                            case NetSystemType.ConnectionEstablished:
                            case NetSystemType.Ping:
                            case NetSystemType.Pong:
                            case NetSystemType.Disconnect:
                            case NetSystemType.ConnectionRejected:
                            case NetSystemType.StringTableAck:
                            case NetSystemType.ConnectResponse:
                            case NetSystemType.Discovery:

                            default:
                                if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                    NetMessageType.BadMessageReceived)
                                {
                                    m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                                       "Undefined behaviour for server and system type " +
                                                                       sysType, message.m_sender, senderEndpoint);

                                    message.m_sender.Statistics.CountBadMessageReceived(message.m_type, message.m_sequenceChannel, payLen, now);
                                }
                                break;
                        }
                    }
                    else if (message.m_type == NetMessageLibraryType.User)
                    {
                        // TODO: HandleUserMessage should be virtual with UDP connection handling reliability and TCP not
                        // not a system message but a user message
                        // first we allow the connection to handle the message to verify
                        // sequencing, reliability, ordering, etc.  If all that checks out
                        // finally the application can respond to the message via an event
                        // or by storing these messages into a global queue or MSMQ.
                        // Currently we have this occuring at the bottom of NetUDPConnection.Fragmentation.cs - AcceptMessage()
                        // TODO: temporarily disabled to get to compile and 
                        // added the following exception instead to remind us.                
                        //message.m_sender.HandleUserMessage(message);
                        // instead of m_sender.HandleUserMessage() which for UDP handles sequencing of ordered + reliable packets and such
                        // we need to go straight .Enqueue(msg) skipping even to m_sender.AcceptMessage() 
                        // because .AcceptMessage() is for combining fragmented datagrams, but with TCP, we must create full packets
                        // in DataReceived() and for DataReceived() to not forward to HandleReceivedMessage() (this very sub) until it has a full message
                        m_messageManager.Enqueue(message); // for now i think we can get away with calling .Enqueue since our packets are going to be
                        // small and shouldnt be fragmented

                    }
                    else
                    {
                        if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                                    NetMessageType.BadMessageReceived)
                        {
                            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                               "Undefined behaviour for server and message type " +
                                                               message.m_type, message.m_sender, senderEndpoint);

                            message.m_sender.Statistics.CountBadMessageReceived(message.m_type, message.m_sequenceChannel, payLen, now);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
                Debug.WriteLine ("NetTCPServer.HandleReceivedMessage() - " + ex.Message + " +++++ Stack trace = " +  ex.StackTrace );
            }
        }
    }
}
