using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Lidgren.Network
{
    internal class NetSocket : NetPoolItem
    {
        private static NetSocketPool m_socketPool;
        private Socket m_socket;
        internal NetBuffer m_sendBuffer;
        internal NetBuffer m_scratchBuffer;
        private NetStatistics m_statistics;
        private NetSimulator m_simulator;
        private int m_sendBufferSize;
        private int m_receiveBufferSize;
        private IPAddress  m_address;
        private int m_port;
        private ProtocolType m_protocol;
        internal bool m_isBound;
        private object m_bindLock;
        internal byte[] m_randomIdentifier;
        
        public delegate void ExceptionEvent(NetSocket owner, string message);
        public delegate void ClosedEvent(NetSocket owner);

        public EventHandler<SocketAsyncEventArgs > SendCompletedHandler;

        public ExceptionEvent SocketExceptionEventHandler;
        public ClosedEvent SocketClosedEventHandler;


        private NetSocket(int sendBufferSize, int receiveBufferSize, ProtocolType protocol)
        {
            m_sendBuffer = new NetBuffer(sendBufferSize);

            Init(sendBufferSize, receiveBufferSize, protocol);
        }

        public static NetSocket Create(int sendBufferSize, int receiveBufferSize, ProtocolType protocol)
        {
            return new NetSocket(sendBufferSize, receiveBufferSize, protocol);
        }

        private void Init(int sendBufferSize, int receiveBufferSize, ProtocolType protocol)
        {
            m_randomIdentifier = new byte[8];
            NetRandom.Instance.NextBytes(m_randomIdentifier);
            m_sendBufferSize = sendBufferSize;
            m_receiveBufferSize = receiveBufferSize;
            m_sendBuffer = new NetBuffer(sendBufferSize);
            m_statistics = new NetStatistics();
            m_simulator = new NetSimulator(this, m_statistics);
            m_scratchBuffer = new NetBuffer(32);

            m_bindLock = new object();
            m_protocol = protocol;

            if (m_protocol == ProtocolType.Tcp)
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = false
                };
                try
                {
                    m_socket.NoDelay = true; // .NoDelay has no affect on UDP so we'll put it here only.  
                }
                catch (SocketException  ex)
                {
                    Debug.WriteLine("NetSocket.Init() - " + ex.Message + " Errorcode = " + ex.ErrorCode.ToString());
                }
            }
            else if (m_protocol == ProtocolType.Udp)
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    Blocking = false
                };
            }
            else
                throw new Exception("NetSocket.Init() - Unsupported protocol");

        }

        public ProtocolType Protocol { get { return m_protocol; } }
        public Socket Socket { get { return m_socket; }}

        public NetStatistics Statistics { get { return m_statistics; } }

        public NetSimulator Simulator { get { return m_simulator; } }

        public NetBuffer ScratchBuffer { get { return m_scratchBuffer; } }

        public bool IsBound { get { return m_isBound; } }

        public byte[] RandomIdentifier { get { return m_randomIdentifier; } }
        /// <summary>
        /// Gets which port this netbase instance listens on, or -1 if it's not listening.
        /// </summary>
        public int ListenPort
        {
            get
            {
                if (m_isBound)
                    return (m_socket.LocalEndPoint as IPEndPoint).Port;
                return -1;
            }
        }

        private void Bind()
        {
            Bind(m_port);
        }
        public void Bind(int port)
        {
            Bind(IPAddress.Any, port);
        }

        public void Bind (IPAddress ipAddress , int port)
        {
            if (m_isBound) return;
            m_port = port;
            m_address = ipAddress;

            EndPoint ep = new IPEndPoint(m_address, m_port);

            m_socket.Bind(ep);

            // TODO: i tend to think that if we're using the AcceptAsync and ReceiveAsync with SocketAsyncEventArgs
            // where we are setting the buffer's explicitly, that these socket options arent necessary
            // but perhaps the reason it's done for UDP is there's just one socket doing all the sending/receiving
            // TODO: verify that if we convert to using the asychronous accept and receivefrom that we can just supply our own receive buffer 
            // and not worry about this socket option atall (least not receive) Need to research about the socket's internal bufffer vs app buffer
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, m_receiveBufferSize);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, m_sendBufferSize);

            try
            {
                if (m_protocol == ProtocolType.Udp)
                { }
                else if (m_protocol == ProtocolType.Tcp)
                {
                    // TODO: im not 100% sure how to determine a good value for backlog
                    // i think we want it as small as possible for the server's demands so that DoS attacks are mitigated
                    int mListenBacklog = 16; // we'll make it 16 for now, but thisshould be in m_config
                    m_socket.Listen(mListenBacklog);
                }
                else
                    throw new Exception("Protocol not supported.");

            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    throw new NetException("Failed to bind to port " + m_port + " - Address already in use!", sex);
                throw;
            }
            catch (Exception ex)
            {
                throw new NetException("Failed to bind to port " + m_port, ex);
            }
            
            m_isBound = true;
            m_statistics.Reset();
        }

        internal bool AcceptAsync(SocketAsyncEventArgs e)
        {
            if (!m_socket.IsBound) throw new Exception("Socket not bound.");

            switch (e.SocketError)
            {
                case SocketError.Success :
                    return m_socket.AcceptAsync(e);
                    
                default:
                    throw new Exception("Socket accept error.");
            }
            
        }

        ///// <summary>
        ///// In order to have TCP transparently detect connection/disconnection for us, we need to set a keep alive on each accept socket.
        ///// NOTE: Apparently TCP keep alive behaves differently on different versions of windows + .net framework version.  I'm hoping
        ///// latest .net always allows you to set individual keep alive options per socket and that it works on all versions of windows
        ///// that support .net 3.5.  
        ///// For instance, older versions of windows and i think CE as well, you have to set registry value which affects _all_ sockets
        ///// HKEY_LOCAL_MACHINE\Comm\Tcpip\Parms
        ///// with the default keep alive interval set to 2 hours!
        ///// http://msdn.microsoft.com/en-us/library/bb736550(VS.85).aspx
        ///// NOTE: There it says "SIO_KEEPALIVE_VALS is supported on Windows 2000 and later."
        ///// NOTE: Based on my research, it seems that the only proper way is to not rely on KeepAlive at all
        ///// but to have your own simple ping/pong and if either client or server hasnt received a ping or pong in x interval
        ///// to assume the connection has terminated.
        ///// </summary>
        ///// <param name="socket"></param>
        ///// <param name="keepaliveTime"></param>
        ///// <param name="keepaliveInterval"></param>
        //private void SetTcpKeepAlive(Socket socket, uint keepaliveTime, uint keepaliveInterval)
        //{
        //    /* the native structure
        //    struct tcp_keepalive {
        //    ULONG onoff;
        //    ULONG keepalivetime;
        //    ULONG keepaliveinterval;
        //    };
        //    */

        //    // marshal the equivalent of the native structure into a byte array
        //    uint dummy = 0;
        //    byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
        //    BitConverter.GetBytes((uint)(keepaliveTime)).CopyTo(inOptionValues, 0);
        //    BitConverter.GetBytes((uint)keepaliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
        //    BitConverter.GetBytes((uint)keepaliveInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);

        //    // write SIO_VALS to Socket IOControl
        //    socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        //}

        /// <summary>
        /// Send a single, out-of-band unreliable message
        /// </summary>
        internal void DoSendOutOfBandMessage(NetBuffer data, IPEndPoint recipient)
        {
            m_sendBuffer.Reset();

            // message type and channel
            m_sendBuffer.Write((byte)((int)NetMessageLibraryType.OutOfBand | ((int)NetChannel.Unreliable << 3)));
            m_sendBuffer.Write((ushort)0);

            // payload length; variable byte encoded
            if (data == null)
            {
                m_sendBuffer.WriteVariableUInt32((uint)0);
            }
            else
            {
                int dataLen = data.LengthBytes;
                m_sendBuffer.WriteVariableUInt32((uint)(dataLen));
                m_sendBuffer.Write(data.Data, 0, dataLen);
            }

            SendPacket(recipient);

            // unreliable; we can recycle this immediately
            //if (ExceptionEvent != m_bufferManager.RecycleBuffer(data);
        }

        internal void SendSingleUnreliableSystemMessage(NetSystemType tp, NetBuffer data, IPEndPoint remoteEP, bool useBroadcast)
        {
            //m_messageManager.SendSingleUnreliableSystemMessage(tp, data, remoteEP, useBroadcast);
            // packet number
            m_sendBuffer.Reset();

            // message type and channel
            m_sendBuffer.Write((byte)((int)NetMessageLibraryType.System | ((int)NetChannel.Unreliable << 3)));
            m_sendBuffer.Write((ushort)0);

            // payload length; variable byte encoded
            if (data == null)
            {
                m_sendBuffer.WriteVariableUInt32((uint)1);
                m_sendBuffer.Write((byte)tp);
            }
            else
            {
                int dataLen = data.LengthBytes;
                m_sendBuffer.WriteVariableUInt32((uint)(dataLen + 1));
                m_sendBuffer.Write((byte)tp);
                m_sendBuffer.Write(data.Data, 0, dataLen);
            }

            if (useBroadcast)
            {
                bool wasSSL = m_simulator.SuppressSimulatedLag;
                try
                {
                    m_simulator.SuppressSimulatedLag = true;
                    m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                    SendPacket(remoteEP);
                }
                finally
                {
                    m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
                    m_simulator.SuppressSimulatedLag = wasSSL;
                }
            }
            else
            {
                SendPacket(remoteEP);
            }
        }

        /// <summary>
        /// Pushes a single packet onto the wire from m_sendBuffer
        /// </summary>
        internal void SendPacket(IPEndPoint remoteEP)
        {
            SendPacket(m_sendBuffer.Data, m_sendBuffer.LengthBytes, remoteEP);
        }

        /// <summary>
        /// Immediately pushes a single packet onto the wire using the supplied data 
        /// </summary>
        internal void SendPacket(byte[] data, int length, IPEndPoint remoteEP)
        {
            if (length <= 0 || length > m_sendBufferSize)
            {
                string str = "Invalid packet size; Must be between 1 and NetConfiguration.SendBufferSize - Invalid value: " + length;
                //TODO: event for socketException for logging purposes
                if (SocketExceptionEventHandler != null) SocketExceptionEventHandler.Invoke(this, str); // m_messageManager.LogWrite(str);
                throw new NetException(str);
            }

            // for client's Binding to the remote endpoint is typically not called explicitly, so it's done here
            if (!m_isBound) Bind();

#if DEBUG
            if (!m_simulator.SuppressSimulatedLag)
            {
                bool send = m_simulator.SimulatedSendPacket(data, length, remoteEP);
                if (!send)
                {
                    m_statistics.CountPacketSent(length);
                    return;
                }
            }
#endif

            // note: this is only used for UDP.  TCPConnection sends are done via
            // NetTCPConnection.SendMessage() which calls socket.SendAsync(e)
            //m_socket.SendTo(data, 0, length, SocketFlags.None, remoteEP);
            //int bytesSent = m_socket.SendTo(data, 0, length, SocketFlags.None, remoteEP);


            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += SendCompletedHandler;
            e.UserToken = this;
            e.RemoteEndPoint = remoteEP;
            e.DisconnectReuseSocket = true;
            e.SetBuffer(data, 0, length);    // TODO: could it be an issue with the ping/pong occuring in a seperate thread and screwing up the buffer?
            //m_sendBuffer.Write(data);
            //e.SetBuffer(m_sendBuffer.Data, 0, length);
            // m_socket.NoDelay = true; not valid for UDP
            if (!m_socket.SendToAsync(e))
                SendCompletedHandler.Invoke(m_socket, e);
        }

        public void ShutDown()
        {
            lock (m_bindLock)
            {
                try
                {
                    if (m_socket != null)
                    {
#if DEBUG
                        // just send all delayed packets; since we won't have the possibility to do it after socket is closed
                        m_simulator.SendDelayedPackets(NetTime.Now + m_simulator.SimulatedMinimumLatency + m_simulator.SimulatedLatencyVariance + 1000.0);
#endif
                        m_socket.Shutdown(SocketShutdown.Receive);
                        m_socket.Close(2);
                    }
                }
                finally
                {
                    m_socket = null;
                    m_isBound = false;
                }
           
                // these should result in callbacks which will then handle any logging
                if (SocketClosedEventHandler != null) SocketClosedEventHandler.Invoke(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NetSocket()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Unless we're already shut down, this is the equivalent of killing the process
            m_isBound = false;
            if (disposing)
            {
                if (m_socket != null)
                {
                    m_socket.Close();
                    m_socket = null;
                }
            }
        }
    }
}
