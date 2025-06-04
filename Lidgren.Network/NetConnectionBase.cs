using System;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;

namespace Lidgren.Network
{
    /// <summary>
    /// Represents a connection between this host and a remote endpoint
    /// </summary>
    [DebuggerDisplay("RemoteEndpoint = {m_remoteEndPoint}")]
    public abstract partial class NetConnectionBase
    {
        internal NetSocket m_socket;
        internal IPEndPoint m_remoteEndPoint;
        internal NetMessageManager m_messageManager;
        internal NetQueue<OutgoingNetMessage> m_unsentMessages;
        internal NetQueue<OutgoingNetMessage> m_lockedUnsentMessages;
        internal int m_broadcastGroup;

        internal NetConnectionStatus m_status;
        internal byte[] m_localHailData; // outgoing hail data
        protected byte[] m_remoteHailData; // incoming hail data (with connect or response)
        protected double m_ackWithholdingStarted;
        protected float m_throttleDebt;

        internal bool m_approved;

        protected bool m_requestDisconnect;
        protected float m_requestLinger;
        protected bool m_requestSendGoodbye;
        protected double m_futureClose;
        protected string m_futureDisconnectReason;

        protected bool m_isInitiator; // if true: we sent Connect; if false: we received Connect
        internal double m_handshakeInitiated;
        protected int m_handshakeAttempts;

        protected NetConfiguration m_config;
        public delegate void ConnectionConnectedEvent(NetConnectionBase connection);
       
        public ConnectionConnectedEvent ConnectionEventHandler;
        internal NetConnectionStatistics m_statistics;
        protected double m_lastSentUnsentMessages;

        internal NetConnectionBase(NetSocket owner, NetMessageManager messageManager, NetConfiguration config, IPEndPoint remoteEndPoint, byte[] localHailData, byte[] remoteHailData)
        {
            m_socket = owner;

            m_socket.SendCompletedHandler += OnSendCompleted;

            m_remoteEndPoint = remoteEndPoint;
            m_localHailData = localHailData;
            m_remoteHailData = remoteHailData;
            m_futureClose = double.MaxValue;
            m_config = config;
            m_throttleDebt = m_config.ThrottleBytesPerSecond; // slower start
            // unsent messages are for when the library is adding a message
            m_unsentMessages = new NetQueue<OutgoingNetMessage>(6);
            // locked unsent messages is for when the calling application is adding a message
            m_lockedUnsentMessages = new NetQueue<OutgoingNetMessage>(3);
            m_messageManager = messageManager;
            m_statistics = new NetConnectionStatistics(this, 1.0f);
            m_status = NetConnectionStatus.Connecting; // to prevent immediate removal on heartbeat thread
            m_broadcastGroup = 0;

            InitializeStringTable();
            //InitializeCongestionControl(32);
        }

        /// <summary>
        /// Gets the statistics object for this connection
        /// </summary>
        public NetConnectionStatistics Statistics { get { return m_statistics; } }

        /// <summary>
        /// Gets or sets the multicast broadcast group id.  -1 is the default
        /// </summary>
        public int BroadcastGroupID { get { return m_broadcastGroup; } set { m_broadcastGroup = value; } }

        /// <summary>
        /// Gets or sets local (outgoing) hail data
        /// </summary>
        public byte[] LocalHailData { get { return m_localHailData; } set { m_localHailData = value; } }

        /// <summary>
        /// Gets remote (incoming) hail data, if available
        /// </summary>
        public byte[] RemoteHailData { get { return m_remoteHailData; } }

        /// <summary>
        /// Remote endpoint for this connection
        /// </summary>
        public IPEndPoint RemoteEndpoint { get { return m_remoteEndPoint; } }

        /// <summary>
        /// For application use
        /// </summary>
        public object Tag;

        /// <summary>
        /// Gets the status of the connection
        /// </summary>
        public NetConnectionStatus Status { get { return m_status; } }

        /// <summary>
        /// Used only by NetClient's to connect to a server.  
        /// </summary>
        internal virtual void Connect()
        {
            m_isInitiator = true;
            m_handshakeInitiated = NetTime.Now;
            m_futureClose = double.MaxValue;
            m_futureDisconnectReason = null;

            m_messageManager.LogVerbose("Sending Connect request to " + m_remoteEndPoint, this);
            
            SetStatus(NetConnectionStatus.Connecting, "Connecting");
        }

        // these "queues" really should result in immediate sending for outgoing.  Only
        // incoming should be queued for when we are ready to process them
        internal void Enqueue(OutgoingNetMessage msg)
        {
            lock (m_unsentMessages)
                m_unsentMessages.Enqueue(msg);
        }

        /// <summary>
        /// Acks are pushed to the front of the queue
        /// </summary>
        /// <param name="msg"></param>
        internal void EnqueueFirst(OutgoingNetMessage msg)
        {
            m_unsentMessages.EnqueueFirst(msg);
        }

        internal void LockedEnqueue(OutgoingNetMessage msg)
        {
            // typically messages that are being sent by the application
            lock (m_lockedUnsentMessages)
                m_lockedUnsentMessages.Enqueue(msg);
        }

        internal void DrainLockedUnset()
        {
            // drain messages from application into main unsent list
            lock (m_lockedUnsentMessages)
            {
                OutgoingNetMessage lm;
                while ((lm = m_lockedUnsentMessages.Dequeue()) != null)
                    Enqueue(lm);
            }
        }

        internal void SetStatus(NetConnectionStatus status, string reason)
        {
            // Connecting status is given to NetUDPConnection at startup, so we must treat it differently
            if (m_status == status && status != NetConnectionStatus.Connecting)
                return;

            m_messageManager.LogWrite("New connection status: " + status + " (" + reason + ")");
            NetConnectionStatus oldStatus = m_status;
            m_status = status;

            if (m_status == NetConnectionStatus.Connected)
            {
                if (ConnectionEventHandler != null) ConnectionEventHandler.Invoke(this);

            }
            m_messageManager.NotifyStatusChange(this, reason);
        }

        internal virtual void Heartbeat(double now)
        {
            if (m_status == NetConnectionStatus.Disconnected)
                return;

            //CongestionHeartbeat(now);
            DrainLockedUnset();

            if (m_status == NetConnectionStatus.Connecting)
            {
                if (now - m_handshakeInitiated > m_config.HandshakeAttemptRepeatDelay)
                {
                    if (m_handshakeAttempts >= m_config.HandshakeAttemptsMaxCount)
                    {
                        Disconnect("No answer from remote host", 0, false, true);
                        return;
                    }
                    m_handshakeAttempts++;
                    if (m_isInitiator) // if this is the initiating application
                    {
                        m_messageManager.LogWrite("Re-sending Connect", this);
                        Connect();
                    }
                    else // this is the listening application
                    {
                        m_messageManager.LogWrite("Re-sending ConnectResponse", this);
                        m_handshakeInitiated = now;

                        OutgoingNetMessage response = m_messageManager.CreateSystemMessage(NetSystemType.ConnectResponse);
                        if (m_localHailData != null)
                            response.m_buffer.Write(m_localHailData);

                        Enqueue(response);
                    }
                }
            }
            else if (m_status == NetConnectionStatus.Connected)
            {
                // send ping?
                CheckPing(now);
            }

            if (m_requestDisconnect)
                InitiateUDPDisconnect();

            if (now > m_futureClose)
                FinalizeDisconnect();
        }


        internal virtual void HandleSystemMessage(IncomingNetMessage msg, double now)
        {
            msg.m_buffer.Position = 0;
            NetSystemType sysType = (NetSystemType)msg.m_buffer.ReadByte();
            OutgoingNetMessage response = null;
            switch (sysType)
            {
                case NetSystemType.Disconnect:
                    if (m_status == NetConnectionStatus.Disconnected)
                    {
                        m_messageManager.NotifyApplication(NetMessageType.StatusChanged, "Disconnected.", this, msg.m_senderEndPoint);
                        return;
                    }
                    Disconnect(msg.m_buffer.ReadString(), 0.75f + ((float)m_currentAvgRoundtrip * 4), false, false);
                    break;
                case NetSystemType.ConnectionRejected:
                    // TODO: event
                    m_messageManager.NotifyApplication(NetMessageType.ConnectionRejected, msg.m_buffer.ReadString(), msg.m_sender, msg.m_senderEndPoint);
                    break;
                case NetSystemType.Connect:
                    // if this connection object already exists and we're still getting a Connect message then   
                    // ConnectReponse must have been lost
                    if (!ReadAndValidateAppIdent(msg, m_config.ApplicationIdentifier, m_messageManager)) return;
                    if (!ReadAndValidateIdentifier(msg, m_socket.RandomIdentifier, m_messageManager)) return;
                    
                    m_remoteHailData = ReadHailData(msg);


                    // finalize disconnect if it's in process
                    if (m_status == NetConnectionStatus.Disconnecting)
                        FinalizeDisconnect();

                    // send response; even though we're already connected
                    response = m_messageManager.CreateSystemMessage(NetSystemType.ConnectResponse);
                    if (m_localHailData != null) response.m_buffer.Write(m_localHailData);

                    Enqueue(response);

                    break;
                case NetSystemType.ConnectResponse:
                    if (m_status != NetConnectionStatus.Connecting || m_status != NetConnectionStatus.Connected)
                    {
                        m_messageManager.LogWrite("Received connection response but we're not connecting...", this);
                        //return;
                    }

                    m_remoteHailData = ReadHailData(msg);
                    m_messageManager.LogWrite("Received connection response but we're not connecting...", this);
                    // Send connectionestablished
                    response = m_messageManager.CreateSystemMessage(NetSystemType.ConnectionEstablished);
                    if (m_localHailData != null) response.m_buffer.Write(m_localHailData);

                    Enqueue(response);

                    // send first ping 250ms after connected
                    m_lastSentPing = now - m_config.PingFrequency + 0.1 + (NetRandom.Instance.NextFloat() * 0.25f);
                    m_statistics.Reset();
                    SetInitialAveragePing(now - m_handshakeInitiated);
                    SetStatus(NetConnectionStatus.Connected, "Connected");
                    break;
                case NetSystemType.ConnectionEstablished:
                    if (m_status != NetConnectionStatus.Connecting)
                    {
                        if ((m_messageManager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                            m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived, "Received connection response but we're not connecting...", this, msg.m_senderEndPoint);
                        return;
                    }

                    // read hail data
                    if (m_remoteHailData == null)
                    {
                        m_remoteHailData = ReadHailData(msg);
                    }

                    // send first ping 100-350ms after connected
                    m_lastSentPing = now - m_config.PingFrequency + 0.1 + (NetRandom.Instance.NextFloat() * 0.25f);
                    m_statistics.Reset();
                    SetInitialAveragePing(now - m_handshakeInitiated);
                    SetStatus(NetConnectionStatus.Connected, "Connected");
                    break;
                case NetSystemType.Ping:
                    // also accepted as ConnectionEstablished
                    if (m_isInitiator == false && m_status == NetConnectionStatus.Connecting)
                    {
                        m_messageManager.LogWrite("Received ping; interpreted as ConnectionEstablished", this);
                        m_statistics.Reset();
                        SetInitialAveragePing(now - m_handshakeInitiated);
                        SetStatus(NetConnectionStatus.Connected, "Connected");
                    }

                    //LogWrite("Received ping; sending pong...");
                    SendPong(m_socket, m_remoteEndPoint, now);
                    break;
                case NetSystemType.Pong:
                    double twoWayLatency = now - m_lastSentPing;
                    if (twoWayLatency < 0)
                        break;
                    ReceivedPong(twoWayLatency, msg);
                    break;
                case NetSystemType.StringTableAck:
                    ushort val = msg.m_buffer.ReadUInt16();
                    StringTableAcknowledgeReceived(val);
                    break;
                default:
                    m_messageManager.LogWrite("Undefined behaviour in NetUDPConnection for system message " + sysType, this);
                    break;
            }
        }

        internal static bool ReadAndValidateAppIdent(IncomingNetMessage msg, string identifier, NetMessageManager manager)
        {
            string appIdent = "";
            appIdent = msg.m_buffer.ReadString();

            if (appIdent != identifier)
            {
                if ((manager.m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
                    manager.NotifyApplication(NetMessageType.BadMessageReceived, "Connect for different application identification received: " + appIdent, null, msg.m_senderEndPoint);
                return false;
            }
            return true;
        }

        internal static bool ReadAndValidateIdentifier(IncomingNetMessage msg, byte[] identifier, NetMessageManager manager)
        {
            // read random identifer (Hypno - i'm not too keen on this.  How do we know it doesnt result in lots of collisions?)
            if (msg.m_buffer.LengthUnreadBytes < 8) return false; // Hypnotron Sept.1.2011 - added line
            byte[] rnd = msg.m_buffer.ReadBytes(8);
            if (NetUtility.CompareElements(rnd, identifier))
            {
                // don't allow self-connect
                if ((manager.m_enabledMessageTypes & NetMessageType.ConnectionRejected) == NetMessageType.ConnectionRejected)
                    manager.NotifyApplication(NetMessageType.ConnectionRejected, "Connection to self not allowed", null, msg.m_senderEndPoint);
                return false;
            }
            return true;
        }

        internal static byte[] ReadHailData(IncomingNetMessage msg)
        {
            return msg.m_buffer.ReadBytesRemaining();
        }

        public void SendMessage(IRemotableType remoteable, NetChannel channel)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(remoteable.Type);
            remoteable.Write(buffer);
            SendMessage(buffer, channel);
        }

        public void SendMessage(byte[] data, NetChannel channel, bool isLibraryThread)
        {
            NetBuffer buffer = new NetBuffer(data);
            SendMessage(buffer, channel, null, isLibraryThread);
        }

        /// <summary>
        /// Queue message for sending
        /// </summary>
        public void SendMessage(NetBuffer data, NetChannel channel)
        {
            SendMessage(data, channel, null, false);
        }

        /// <summary>
        /// Queue a reliable message for sending. When it has arrived ReceiptReceived will be fired on owning NetBase, and the ReceiptEventArgs will contain the object passed to this method.
        /// </summary>
        public void SendMessage(NetBuffer data, NetChannel channel, NetBuffer receiptData)
        {
            SendMessage(data, channel, receiptData, false);
        }

        ///////////////////////////////////////
        // THIS IS FOR UDP SENDS ONLY.  TCP packets SendMessage() is OVERRIDEN IN NetTCPConnection
        // TODO: Use this with TRUE isLibraryThread for internal sendings (acks etc)
        // Ultimately when using UDP outgoing messages are queued 
        // NetBase.SendMessage() -> NetConnectionBase.SendMessage() and then finally during heartbeat
        // the queued unsent messages are sent inside of NetUDPConnection.Reliability
        // I think we can keep NetClient.SendMessage() but we need to then override the following
        // in NetTCPConnection
        internal virtual void SendMessage(NetBuffer data, NetChannel channel, NetBuffer receiptData, bool isLibraryThread)
        {
            if (m_status != NetConnectionStatus.Connected)
#if DEBUG
                throw new NetException("Status must be Connected to send messages");
#else
            return;
#endif

            if (data.LengthBytes > m_config.m_maximumTransmissionUnit)
            {
                //
                // Fragmented message
                //
                int dataLen = data.LengthBytes;
                int chunkSize = m_config.m_maximumTransmissionUnit - 10; // header
                int numFragments = dataLen / chunkSize;
                if (chunkSize * numFragments < dataLen)
                    numFragments++;

                ushort fragId = m_nextSendFragmentId++;

                for (int i = 0; i < numFragments; i++)
                {
                    OutgoingNetMessage fmsg = m_messageManager.CreateOutgoingMessage();
                    fmsg.m_type = NetMessageLibraryType.UserFragmented;
                    fmsg.m_msgType = NetMessageType.Data;

                    NetBuffer fragBuf = m_messageManager.CreateBuffer();
                    fragBuf.Write(fragId);
                    fragBuf.WriteVariableUInt32((uint)i);
                    fragBuf.WriteVariableUInt32((uint)numFragments);

                    if (i < numFragments - 1)
                    {
                        // normal fragment
                        fragBuf.Write(data.Data, i * chunkSize, chunkSize);
                    }
                    else
                    {
                        // last fragment
                        int bitsInLast = data.LengthBits - (chunkSize * (numFragments - 1) * 8);
                        int bytesInLast = dataLen - (chunkSize * (numFragments - 1));
                        fragBuf.Write(data.Data, i * chunkSize, bytesInLast);

                        // add receipt only to last message
                        fmsg.m_receiptData = receiptData;
                    }
                    fmsg.m_buffer = fragBuf;
                    fmsg.m_buffer.m_refCount = 1; // since it's just been created

                    fmsg.m_numSent = 0;
                    fmsg.m_nextResend = double.MaxValue;
                    fmsg.m_sequenceChannel = channel;
                    fmsg.m_sequenceNumber = -1;

                    if (isLibraryThread)
                    {
                        // typically messages like system messages and acks that get automatically handling by the library
                        // but im thinking there really shouldnt be this issue because sends should ocur immediately.  Why
                        // are the outgoing library messages queued at all?
                        Enqueue(fmsg);
                    }
                    else
                    {
                        LockedEnqueue(fmsg);
                    }
                }

                // TODO: recycle the original, unfragmented data
                return;
            }

            //
            // Normal, unfragmented, message
            //
            OutgoingNetMessage msg = m_messageManager.CreateOutgoingMessage();
            msg.m_msgType = NetMessageType.Data;
            msg.m_type = NetMessageLibraryType.User;
            msg.m_buffer = data;
            msg.m_buffer.m_refCount++; // it could have been sent earlier also
            msg.m_numSent = 0;
            msg.m_nextResend = double.MaxValue;
            msg.m_sequenceChannel = channel;
            msg.m_sequenceNumber = -1;
            msg.m_receiptData = receiptData;

            if (isLibraryThread)
            {
                Enqueue(msg);
            }
            else
            {
                LockedEnqueue(msg);
            }
        }

        protected void OnSendCompleted(object obj, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            int bytesSent = e.BytesTransferred;
            m_statistics.CountPacketSent(bytesSent);
            SocketError err = e.SocketError;
            e.SetBuffer(null, 0, 0);
          e.Dispose(); //memleak

            switch (err)
            {
                case SocketError.Success:
#if DEBUG || USE_RELEASE_STATISTICS
                    if (!m_socket.Simulator.SuppressSimulatedLag)
                        m_statistics.CountPacketSent(bytesSent);
#endif
                    //m_socket.Statistics.
                    break;

                case SocketError.WouldBlock :
                case SocketError.IOPending :
                case SocketError.NoBufferSpaceAvailable :
#if DEBUG
                    // send buffer overflow?
                    //const string str = "SocketException.WouldBlock thrown during sending; send buffer overflow? Increase buffer using NetAppConfiguration.SendBufferSize";
                    //if (SocketExceptionEventHandler != null) SocketExceptionEventHandler.Invoke(this, str);
                    //throw new NetException(str, e.SocketError);
#else
                    
                    // we need to retry the send after waiting a while for the buffer to hopefully decrease
                    // but how?  do we Thread.Sleep(100) and just sit here waiting and then retry?

#endif
                    m_messageManager.NotifyApplication(NetMessageType.VerboseDebugMessage, "NetConnectionBase.OnSend() - Socket IO Error", this);
                    break;

                case SocketError.ConnectionReset:
                case SocketError.ConnectionRefused:
                case SocketError.ConnectionAborted:
                case SocketError.TimedOut:
                default:
                    
                    // the socket connection is lost, set m_requestDisconnect = true to notify ConnectionBase.Heartbeat() to disconnect which results in
                    // SetStatus() called which then notifies the host application so any client disconnect maintenace code can be performed
                    string reason = "Connection lost."; // no need to send a "Goodbye" packet to the client because they are already gone
                    // setting m_requestDisconnect = true will result in the NetServer being notified via maintenance routine via Connection.Heartbeat()
                    m_requestDisconnect = true;
                    m_requestLinger = 0.0f;
                    m_requestSendGoodbye = false ; // !string.IsNullOrEmpty(reason);
                    m_futureDisconnectReason = reason;
                    m_messageManager.NotifyApplication(NetMessageType.VerboseDebugMessage, reason, this);
                    break;
            }
        }

        /// <summary>
        /// Notify application that a connection changed status
        /// </summary>
        internal void NotifyStatusChange(NetConnectionBase connection, string reason)
        {
            if ((m_messageManager.EnabledMessageTypes & NetMessageType.StatusChanged) != NetMessageType.StatusChanged)
                return; // disabled
            m_messageManager.NotifyApplication(NetMessageType.StatusChanged, reason, connection);
        }

        /// <summary>
        /// Disconnects from remote host; lingering for 'lingerSeconds' to allow packets in transit to arrive
        /// </summary>
        public void Disconnect(string reason, float lingerSeconds)
        {
            throw new Exception("Verify this is never called by a client.  If so, then we should make this internal and only allow a NetServer.DisconnectClient() call to call it");
                
            Disconnect(reason, lingerSeconds, true, false);
        }

        internal void Disconnect(string reason, float lingerSeconds, bool sendGoodbye, bool forceRightNow)
        {
            if (m_status == NetConnectionStatus.Disconnected)
                return;

            m_futureDisconnectReason = reason;
            m_requestLinger = lingerSeconds;
            m_requestSendGoodbye = sendGoodbye;
            m_requestDisconnect = true;

            if (forceRightNow)
                InitiateUDPDisconnect();
        }

        /// <summary>
        /// UDP Disconnect.  TCP uses explicit disconnect when it overrides NetClient.Disconnect()
        /// </summary>
        private void InitiateUDPDisconnect()
        {
            if (m_requestSendGoodbye)
            {
                NetBuffer scratch = m_socket.m_scratchBuffer;
                scratch.Reset();
                scratch.Write(string.IsNullOrEmpty(m_futureDisconnectReason) ? "" : m_futureDisconnectReason);
                m_socket.SendSingleUnreliableSystemMessage(
                    NetSystemType.Disconnect,
                    scratch,
                    m_remoteEndPoint,
                    false
                );
            }

            if (m_requestLinger <= 0)
            {
                SetStatus(NetConnectionStatus.Disconnected, m_futureDisconnectReason);
                FinalizeDisconnect();
                m_futureClose = double.MaxValue;
            }
            else
            {
                SetStatus(NetConnectionStatus.Disconnecting, m_futureDisconnectReason);
                m_futureClose = NetTime.Now + m_requestLinger;
            }

            m_requestDisconnect = false;
        }

        protected virtual void FinalizeDisconnect()
        {
            SetStatus(NetConnectionStatus.Disconnected, m_futureDisconnectReason);
        }

        public override string ToString()
        {
            return "[" + this.GetType().Name  + m_remoteEndPoint.ToString() + " / " + m_status + "]";
        }
    }
}
