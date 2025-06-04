using System;
using System.Net.Sockets;
using System.Threading;

namespace Lidgren.Network
{
    internal class NetTCPConnection : NetConnectionBase
    {
        internal uint mPartialMessageByteCount;

        /// <summary>
        /// A TCP Connection.  Note that depending on the type of NetClient or NetServer that connections are being managed
        /// you will be able to instantiate either UDP Connections or NetTCPConnections but never both in the same NetClient or NetServer.
        /// However, any particular application using this networking library CAN have multiple NetClients or NetServers running each independantly
        /// using either UDP or TCP connections.  Thus its possible to have a TCPClient for email and file transfer and a UDP client for
        /// real time game state packets.
        /// </summary>
        /// <param name="owner">A unique TCP Socket instance for this connection only.</param>
        /// <param name="messageManager"></param>
        /// <param name="config"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="localHailData"></param>
        /// <param name="remoteHailData"></param>
        public NetTCPConnection(NetSocket owner, NetMessageManager messageManager, NetConfiguration config,
            System.Net.IPEndPoint remoteEndPoint, byte[] localHailData, byte[] remoteHailData)
            : base(owner, messageManager, config, remoteEndPoint, localHailData, remoteHailData)
        {

        }

        /// <summary>
        /// Unlike the NetUDPConnection, here we must explicitly tell the socket to connect
        /// </summary>
        internal override void Connect()
        {
            base.Connect();

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = m_remoteEndPoint;
            e.Completed += ConnectCompleted;
            
       
            // outgoingnetmessage automates the construction of the header
            OutgoingNetMessage outMsg = m_messageManager.CreateSystemMessage(NetSystemType.Connect);
            outMsg.Buffer.Write(m_config.ApplicationIdentifier);
            outMsg.Buffer.Write(m_socket.RandomIdentifier);
            if (m_localHailData != null && m_localHailData.Length > 0)
                outMsg.Buffer.Write(m_localHailData);

            // create a buffer for the asychronous connect socket
            //NetBuffer buffer = NetBuffer.Create();
            NetBuffer buffer = new NetBuffer();
            outMsg.Encode(buffer);  // 

            // NOTE: when creating new TCP messages from stream, we can also keep calling
            // incomingMessage.Encode()  to append new data to an existing message until it's completed

          //  e.BufferList 
            e.SetBuffer(buffer.Data, 0, buffer.LengthBytes);

            // explicit connect attempt
            if (!m_socket.Socket.ConnectAsync(e))
            {
                ConnectCompleted(m_socket, e);
            }
        }

        private void ConnectCompleted(object obj, SocketAsyncEventArgs args)
        {
            // TODO: need a better switch statement and error handling here
            // don't just assume everything worked like a charm... always check for socket errors on IO calls
            if (args.SocketError == SocketError.ConnectionRefused) // occurs if the socket isnt even open on the host computer
            {}
            else if (args.SocketError == SocketError.Success)
            {
                System.Diagnostics.Debug.WriteLine("NetTCPConnection.ConnectCompleted().");
                m_socket.m_isBound = true;
                // note:  the following must be commented out.  Connected status change must occur when the Server 
                // approves the connection
                // SetStatus(NetConnectionStatus.Connected, "Connected."); // this line must remain commented out
            }

            args.SetBuffer(null, 0, 0);
            args.Dispose();
        }

        /// <summary>
        /// TCPConnections don't need a heartbeat because there is no reliability code that needs to run.  But we'll override
        /// to ensure that no unnecessary or inappropriate code is actually performed by the base class.
        /// </summary>
        /// <param name="now"></param>
        internal override void Heartbeat(double now)
        {
           //base.Heartbeat(now);

            // we do need to use Ping/Pongs for TCP
            if (m_status == NetConnectionStatus.Connected)
            {
                // send ping?
                //CheckPing(now);
            }

        }

        internal void SendSocketData ()
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.UserToken = this;
            e.RemoteEndPoint = m_socket.Socket.RemoteEndPoint;
            e.SetBuffer(m_socket.m_sendBuffer.Data, 0, m_socket.m_sendBuffer.LengthBytes);

            if (!m_socket.Socket.SendAsync(e))
                OnSendCompleted(m_socket.Socket, e);
        }

        internal override void SendMessage(NetBuffer data, NetChannel channel, NetBuffer receiptData, bool isLibraryThread)
        {
            if (data == m_socket.m_sendBuffer)
                throw new ArgumentException("Data buffer cannot be the same as socket's send buffer!");
                // TODO: i need a version of SendMessage that sends whatever data is in the sendBuffer directly
                // this version here would take an outgoing system message into a user message and that's incorrect!
                        
            OutgoingNetMessage msg = m_messageManager.CreateOutgoingMessage();
            msg.m_msgType = NetMessageType.Data;
            msg.m_type = NetMessageLibraryType.User;
#if DEBUG  // Hypnotron Sept.22.2011 - Lenn is getting strange empty IncomingNetMessages
            if (data == null)
                System.Diagnostics.Debug.WriteLine("WARNING!  Sending an empty data buffer");

            System.Diagnostics.Trace.Assert(data != null, "WARNING!  Sending an empty data buffer");
#endif
            msg.m_buffer = data;
            msg.m_buffer.m_refCount++; // it could have been sent earlier also
            msg.m_numSent = 0;
            msg.m_nextResend = double.MaxValue;
            msg.m_sequenceChannel = channel;
            msg.m_sequenceNumber = 0;
            msg.m_receiptData = receiptData;
            msg.m_buffer.m_readPosition = 0;

            // encode will create a proper header into our outgoing sendBuffer
            NetBuffer sendBuffer = new NetBuffer(); // m_socket.m_sendBuffer;  DO NOT use m_socket.m_sendBuffer. During threaded sends you will wind up replacing the data in the buffer before it's finished sending and that will result in lots of corrupted data on the client side
            sendBuffer.Reset();
            msg.Encode(sendBuffer);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.UserToken = this;
            e.RemoteEndPoint = m_socket.Socket.RemoteEndPoint;
            e.SetBuffer(sendBuffer.Data, 0, sendBuffer.LengthBytes);
            e.Completed += OnSendCompleted;

            if (!m_socket.Socket.SendAsync(e))
                OnSendCompleted(m_socket.Socket, e);
        }

   
        internal void HandleReceivedMessage(IncomingNetMessage message)
        { 
        }
    }
}
