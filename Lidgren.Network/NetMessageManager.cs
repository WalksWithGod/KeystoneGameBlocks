using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;

namespace Lidgren.Network
{
    /// <summary>
    /// The MessageManager lies at the interface between library and application.  Received completed and ordered
    /// messages are dequeued from the MessageManager and have the connection related to each message referenced
    /// and out going messages from the application to clients goes into the MessageManager to be dispatched by the library.
    /// This is a proper configuration that will work regardless of whether we're using UDP or whether we expand the library
    /// to also support 1:1 tcp conneections
    /// </summary>
    internal class NetMessageManager
    {
        // we could make most of these static.  The only thing that worries me about
        // having seperate NetMessageManager's for each Connection is that to process
        // the messages, you're not processing them in order of receipt or the time they were added to the queue
        // instead you're processing all of one connection's messages first and then itterating.  
        // Is that a bad thing though?   Also consider how a threaded queue would need to work..
        // of course the other question is... why queue at all?  Well we did sort of solve the idea of why 
        // queuing and
        private NetBufferManager m_bufferManager;
        internal NetMessageType m_enabledMessageTypes;
        
        internal NetQueue<IncomingNetMessage> m_receivedMessages;

        private NetQueue<NetBuffer> m_unsentOutOfBandMessages;
        private NetQueue<IPEndPoint> m_unsentOutOfBandRecipients;
        private Queue<SUSystemMessage> m_susmQueue;
        private NetSocket m_socket;

        internal NetMessageManager(NetConfiguration config, NetSocket socket)
        {
            if (config == null || socket == null) throw new ArgumentNullException();
            
            m_bufferManager = new NetBufferManager( config); // <-- should be a singleton
            m_socket = socket;

            // default enabled message types
            m_enabledMessageTypes =
                NetMessageType.Data | NetMessageType.StatusChanged | NetMessageType.ServerDiscovered |
                NetMessageType.DebugMessage | NetMessageType.Receipt;


            m_receivedMessages = new NetQueue<IncomingNetMessage>(4);
            m_unsentOutOfBandMessages = new NetQueue<NetBuffer>();
            m_unsentOutOfBandRecipients = new NetQueue<System.Net.IPEndPoint>();
            m_susmQueue = new Queue<SUSystemMessage>();
        }


        /// <summary>
        /// Gets or sets what types of messages are delievered to the client
        /// </summary>
        internal NetMessageType EnabledMessageTypes { get { return m_enabledMessageTypes; } set { m_enabledMessageTypes = value; } }

        internal void Enqueue(SUSystemMessage susm)
        {
            lock (m_susmQueue)
                m_susmQueue.Enqueue(susm);
        }
        internal void Enqueue(IncomingNetMessage msg)
        {
            lock (m_receivedMessages)
                m_receivedMessages.Enqueue(msg);
        }

        /// <summary>
        /// Here there is a single queue, but how do we know which connection
        /// and therefore which socket to use?  If we tried to pass in a socket or something
        /// then we'd still have to determine if a particular message deqeued is meant for that socket.
        /// Thus it'd be better if the message had the connection associated with it... however
        /// even that isnt completely valid because sometimes when a connection isnt yet established
        /// we still need to send data using just an endpoint and the passed in socket.  In effect
        /// SendOutgoing here really requires a UDP socket is passed in that can be shared for all outgoing.
        /// Thus as a starting point, we know that a socket and a messageManager should be 1:1
        /// Connections on the other hand can be associated with a socket and thus the connection
        /// can determine the messagemanager through socket.MessageManager.
        /// But... and this is a big but... this puts us back into the position of having split queues
        /// for when the application wants to send out, we'd need to determine which socket/messagemanager/connection
        /// to use.  
        /// All of the above can be simplified if i just acknowledge that this UDP setup just uses one UDP socket
        /// for everything.  Forget about TCP/IP altogether and the future case of wanting a similar model... seriously.
        /// Let's just assume the MessageManager is using a single socket reference.
        /// Hrm, perhaps there's still a chance if for a NetTCPConnection.cs message manager behaves differently.  For such
        /// connections there's really no need to have any central message store because we're not funneling through a single
        /// connection.
        /// </summary>
        public void SendOutgoing(double now)
        {

            SendQueuedSystemMessages();
            SendQueuedOOBMessages();

            try
            {
#if DEBUG
                m_socket.Simulator.SendDelayedPackets(now);
#endif
            }
            catch{}

        }

        internal void SendQueuedSystemMessages()
        {
            // Send queued system messages
            if (m_susmQueue.Count > 0)
            {
                lock (m_susmQueue)
                {
                    while (m_susmQueue.Count > 0)
                    {
                        SUSystemMessage su = m_susmQueue.Dequeue();
                        
                        m_socket.SendSingleUnreliableSystemMessage(su.Type, su.Data, su.Destination, su.UseBroadcast);
                    }
                }
            }
        }

        internal void SendQueuedOOBMessages()
        {
            // Send out-of-band messages
            if (m_unsentOutOfBandMessages.Count > 0)
            {
                lock (m_unsentOutOfBandMessages)
                {
                    while (m_unsentOutOfBandMessages.Count > 0)
                    {
                        NetBuffer buf = m_unsentOutOfBandMessages.Dequeue();
                        System.Net.IPEndPoint ep = m_unsentOutOfBandRecipients.Dequeue();
                        m_socket.DoSendOutOfBandMessage(buf, ep);
                    }
                }
            }
        }

        //
        // recycle NetMessage object and NetBuffer
        // 
        internal void RecycleMessage(NetBuffer intoBuffer, NetMessage msg, ref NetMessageType type)
        {
            // This swaps the buffer of the message
            // with that of the intoBuffer so that the calling application gets the message data.  The idea
            // is that we can avoid an allocation.  Not only do we NOT have to copy the data to a new buffer
            // that is returned via function, but even if instead of copying we were to just return the msg buffer
            // itself, we'd still have to have the calling application release that message back to th emessage 
            // cache or else it would have to be reallocated.
            // TODO: But what i find corny is that why not just keep the msg itself and just recycle those and use
            // more of them?  Why use a seperate intoBuffer and have out params for NetUDPConnection and such?

            intoBuffer.Tag = msg.m_buffer.Tag; //TODO: Tag doesnt seem to be used for anything
            NetBuffer tmpBuffer = msg.m_buffer;

            msg.m_buffer = null;
            type = msg.m_msgType;

            // swap content of buffers
            byte[] tmp = intoBuffer.Data;
            intoBuffer.Data = tmpBuffer.Data;
            if (tmp == null)
                tmp = new byte[8]; // application must have lost it somehow
            tmpBuffer.Data = tmp;

            // set correct values for returning value (ignore the other, it's being recycled anyway)
            intoBuffer.m_bitLength = tmpBuffer.m_bitLength;
            intoBuffer.m_readPosition = 0;

            // m_messagePool.Push(msg);

            RecycleBuffer(tmpBuffer);
        }

        public NetBuffer CreateBuffer()
        {
            return m_bufferManager.CreateBuffer();
        }

        public void RecycleBuffer(NetBuffer buffer)
        {
            m_bufferManager.RecycleBuffer(buffer);
        }
        /// <summary>
        /// Creates an incoming net message
        /// </summary>
        internal IncomingNetMessage CreateIncomingMessage()
        {
            // no recycling for messages
            IncomingNetMessage msg = new IncomingNetMessage();
            msg.m_msgType = NetMessageType.Data;
            msg.m_buffer = CreateBuffer();
            return msg;
        }
        /// <summary>
        /// Creates an outgoing net message
        /// </summary>
        internal OutgoingNetMessage CreateOutgoingMessage()
        {
            // no recycling for messages
            OutgoingNetMessage msg = new OutgoingNetMessage();
            msg.m_sequenceNumber = -1;
            msg.m_numSent = 0;
            msg.m_nextResend = double.MaxValue;
            msg.m_msgType = NetMessageType.Data;
            msg.m_buffer = CreateBuffer();
            return msg;
        }

        internal OutgoingNetMessage CreateSystemMessage(NetSystemType systemType)
        {
            OutgoingNetMessage msg = CreateOutgoingMessage();
            msg.m_type = NetMessageLibraryType.System;
            msg.m_sequenceChannel = NetChannel.Unreliable;
            msg.m_sequenceNumber = 0;
            msg.m_buffer.Write((byte)systemType);
            return msg;
        }

        /// <summary>
        /// Emit receipt event (this seems to only be called from NetUDPConnection.Reliability.cs HandleAckMessage())
        /// I fact i think below NetMessageType.Receipt stands for NetMessage.AckReceipt
        /// </summary>
        internal void FireReceipt(NetUDPConnection connection, NetBuffer receiptData)
        {
            if ((m_enabledMessageTypes & NetMessageType.Receipt) != NetMessageType.Receipt)
                return; // disabled

            IncomingNetMessage msg = CreateIncomingMessage();
            msg.m_sender = connection;
            msg.m_msgType = NetMessageType.Receipt;
            msg.m_buffer = receiptData;

            lock (m_receivedMessages)
                m_receivedMessages.Enqueue(msg);
        }



        /// <summary>
        /// Send a single, out-of-band unreliable message
        /// </summary>
        public void SendOutOfBandMessage(NetBuffer data, IPEndPoint recipient)
        {
            lock (m_unsentOutOfBandMessages)
            {
                m_unsentOutOfBandMessages.Enqueue(data);
                m_unsentOutOfBandRecipients.Enqueue(recipient);
            }
        }

        /// <summary>
        /// Send a NAT introduction messages to one and two, allowing them to connect
        /// </summary>
        public void SendNATIntroduction(
            IPEndPoint one,
            IPEndPoint two
        )
        {
            NetBuffer toOne = m_bufferManager.CreateBuffer();
            toOne.Write(two);
            QueueSingleUnreliableSystemMessage(NetSystemType.NatIntroduction, toOne, one, false);

            NetBuffer toTwo = m_bufferManager.CreateBuffer();
            toTwo.Write(one);
            QueueSingleUnreliableSystemMessage(NetSystemType.NatIntroduction, toTwo, two, false);
        }

        /// <summary>
        /// Send a NAT introduction messages to ONE about contacting TWO
        /// </summary>
        public void SendSingleNATIntroduction(
            IPEndPoint one,
            IPEndPoint two
        )
        {
            NetBuffer toOne = m_bufferManager.CreateBuffer();
            toOne.Write(two);
            QueueSingleUnreliableSystemMessage(NetSystemType.NatIntroduction, toOne, one, false);
        }

        // this only seems to be used for queing OUTGOING unreliable system messages only
        internal void QueueSingleUnreliableSystemMessage(NetSystemType tp, NetBuffer data, IPEndPoint remoteEP, bool useBroadcast)
        {
            SUSystemMessage susm = new SUSystemMessage();
            susm.Type = tp;
            susm.Data = data;
            susm.Destination = remoteEP;
            susm.UseBroadcast = useBroadcast;

            Enqueue(susm);
        }

        [Conditional("DEBUG")]
        internal void LogWrite(string message)
        {
            if ((EnabledMessageTypes & NetMessageType.DebugMessage) != NetMessageType.DebugMessage)
                return; // disabled

            NotifyApplication(NetMessageType.DebugMessage, message, null); //sender);
        }

        [Conditional("DEBUG")]
        internal void LogVerbose(string message)
        {
            if ((EnabledMessageTypes & NetMessageType.VerboseDebugMessage) != NetMessageType.VerboseDebugMessage)
                return; // disabled

            NotifyApplication(NetMessageType.VerboseDebugMessage, message, null); //sender);
        }

        [Conditional("DEBUG")]
        internal void LogWrite(string message, NetConnectionBase connection)
        {
            if ((EnabledMessageTypes & NetMessageType.DebugMessage) != NetMessageType.DebugMessage)
                return; // disabled

            NotifyApplication(NetMessageType.DebugMessage, message, connection);
        }

        [Conditional("DEBUG")]
        internal void LogVerbose(string message, NetConnectionBase connection)
        {
            if ((EnabledMessageTypes & NetMessageType.VerboseDebugMessage) != NetMessageType.VerboseDebugMessage)
                return; // disabled

            NotifyApplication(NetMessageType.VerboseDebugMessage, message, connection);
        }

        internal void NotifyApplication(NetMessageType tp, string message, NetConnectionBase conn)
        {
            NetBuffer buf = new NetBuffer(message); // TODO: use new NetBuffer() until after we fix m_bufferManager.CreateBuffer(message);
            NotifyApplication(tp, buf, conn);
        }

        internal void NotifyApplication(NetMessageType tp, string message, NetConnectionBase conn, IPEndPoint ep)
        {
            NetBuffer buf = new NetBuffer(message); // TODO: use new NetBuffer() until after we fix m_bufferManager.CreateBuffer(message);
            NotifyApplication(tp, buf, conn, ep);
        }

        internal void NotifyApplication(NetMessageType tp, NetBuffer buffer, NetConnectionBase conn)
        {
            NotifyApplication(tp, buffer, conn, null);
        }

        internal void NotifyApplication(NetMessageType tp, NetBuffer buffer, NetConnectionBase conn, IPEndPoint ep)
        {
            IncomingNetMessage msg = new IncomingNetMessage();
            msg.m_buffer = buffer;
            msg.m_msgType = tp;
            msg.m_sender = conn;
            msg.m_senderEndPoint = ep;

            Enqueue(msg);
        }

        /// <summary>
        /// Notify application that a connection changed status
        /// </summary>
        internal void NotifyStatusChange(NetConnectionBase connection, string reason)
        {
            if ((EnabledMessageTypes & NetMessageType.StatusChanged) != NetMessageType.StatusChanged)
                return; // disabled
            NotifyApplication(NetMessageType.StatusChanged, reason, connection);
        }

        /*
        internal void BroadcastUnconnectedMessage(NetBuffer data, int port)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (!m_isBound)
                Start();

            m_sendBuffer.Reset();

            // message type, netchannel and sequence number
            m_sendBuffer.Write((byte)((int)NetMessageLibraryType.System | ((int)NetChannel.Unreliable << 3)));
            m_sendBuffer.Write((ushort)0);

            // payload length
            int len = data.LengthBytes;
            m_sendBuffer.WriteVariableUInt32((uint)len);

            // copy payload
            m_sendBuffer.Write(data.Data, 0, len);

            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);

            try
            {

                m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                int bytesSent = m_socket.SendTo(m_sendBuffer.Data, 0, m_sendBuffer.LengthBytes, SocketFlags.None, broadcastEndpoint);
                if (bytesSent > 0)
                    m_statistics.CountPacketSent(bytesSent);
                LogVerbose("Bytes broadcasted: " + bytesSent);
                return;
            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.WouldBlock)
                {
#if DEBUG
                    // send buffer overflow?
                    LogWrite("SocketException.WouldBlock thrown during sending; send buffer overflow? Increase buffer using NetAppConfiguration.SendBufferSize");
                    throw new NetException("SocketException.WouldBlock thrown during sending; send buffer overflow? Increase buffer using NetConfiguration.SendBufferSize", sex);
#else
                    return;
#endif
                }

                if (sex.SocketErrorCode == SocketError.ConnectionReset ||
                    sex.SocketErrorCode == SocketError.ConnectionRefused ||
                    sex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    LogWrite("Remote socket forcefully closed: " + sex.SocketErrorCode);
                    // TODO: notify connection somehow
                    return;
                }

                throw;
            }
            finally
            {
                m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
            }
        }
        */

        
    }
}
