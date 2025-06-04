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
    /// Trying to detemine what to call this class that i'm still working on pairing down.  I was thinking of 
    /// renaming it to NetHost but that's not really accurate since a client uses it too.  I think 
    /// the fact that a client or server can inherit this or use this does make sense for it to be something
    /// generic like NetNetwork or NetHost.  
    /// </summary>
    public abstract class NetBase : IDisposable
    {
        internal NetDiscovery m_discovery;
        protected NetFrequencyCounter m_heartbeatCounter;
        internal NetConfiguration m_config;
        internal NetBufferManager m_bufferManager;
        internal NetMessageManager m_messageManager;
        internal NetSocket m_socket;
        private ProtocolType m_protocol;
        protected bool m_shutdownRequested;
        protected string m_shutdownReason;
        protected bool m_shutdownComplete;
        protected bool m_allowOutgoingConnections; // used by NetPeer
        private object getMessages = new object();


        public NetBase(NetConfiguration config, ProtocolType protocol)
        {
            Debug.Assert(config != null, "Config must not be null");
            if (string.IsNullOrEmpty(config.ApplicationIdentifier))
                throw new ArgumentException("Must set ApplicationIdentifier in NetConfiguration!");

            m_protocol = protocol; 
            m_config = config;
            m_bufferManager = new NetBufferManager(m_config);
            m_socket = NetSocket.Create(m_config.SendBufferSize, m_config.ReceiveBufferSize, protocol);
            m_messageManager = new NetMessageManager(m_config, m_socket);
            m_heartbeatCounter = new NetFrequencyCounter(3.0f); // don't know why 3 is used
        }


        /// <summary>
        /// Gets the configuration for this NetBase instance
        /// </summary>
        public NetConfiguration Configuration { get { return m_config; } }

        public abstract NetConnectionBase GetConnection(IPEndPoint remoteEndpoint);

        // this should rather be handled with a delegate
        // internal virtual void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint) { }
        internal virtual void HandleConnectionForciblyClosed(NetConnectionBase connection, SocketException sex) { }


        /// <summary>
        /// Enables or disables a particular type of message
        /// </summary>
        public void SetMessageTypeEnabled(NetMessageType type, bool enabled)
        {
            if (enabled)
            {
#if DEBUG
#else
				// TODO: re-enable before ship, but for now testing release builds we still have debug messages being sent we havent disabled yet
//				if ((type | NetMessageType.DebugMessage) == NetMessageType.DebugMessage)
//					throw new NetException("Not possible to enable Debug messages in a Release build!");
//				if ((type | NetMessageType.VerboseDebugMessage) == NetMessageType.VerboseDebugMessage)
//					throw new NetException("Not possible to enable VerboseDebug messages in a Release build!");
#endif
                m_messageManager.EnabledMessageTypes |= type;
            }
            else
            {
                m_messageManager.EnabledMessageTypes &= (~type);
            }
        }

        /// <summary>
        /// Provides maintenance 
        /// </summary>
        public virtual void Heartbeat()
        {
            double now = NetTime.Now;
            m_heartbeatCounter.Count(now);

            if (!m_socket.IsBound) return;

            if (m_shutdownRequested)
            {
                PerformShutdown(m_shutdownReason);
                return;
            }
        }
       
        /// <summary>
        /// Override this to process a received NetBuffer on the networking thread 
        /// note: This can be problematic, only use this if you know what you are doing
        /// </summary>
        public virtual void ProcessReceived(NetBuffer buffer)
        {
        }

        internal abstract void HandleReceivedMessage(IncomingNetMessage message, IPEndPoint senderEndpoint);

        internal void InitiateTCPReceive(NetConnectionBase conn, bool preserveBuffer, int offset)
        {

            NetSocket netSocket = conn.m_socket;
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.UserToken = conn;

            if (!preserveBuffer)
            {
                netSocket.m_sendBuffer.Reset();
                Debug.Assert(offset == 0);
                offset = 0; // make sure if preserveBuffer==false, value is 0
            }

            e.SetBuffer(netSocket.m_sendBuffer.Data, offset, netSocket.m_sendBuffer.Data.Length - offset);
            //e.BufferList // only for send/receiv ops, not accept
            e.Completed += OnTCPDataReceived;
            e.DisconnectReuseSocket = true;
            e.RemoteEndPoint = netSocket.Socket.RemoteEndPoint;

            if (!netSocket.Socket.ReceiveAsync(e)) // returns false if the operation succeeds immediately
                OnTCPDataReceived(netSocket.Socket, e);
        }

        protected virtual void OnTCPDataReceived(object obj, SocketAsyncEventArgs args)
        {
            // grab the buffer object and set it's lengthbits 
            NetTCPConnection conn = (NetTCPConnection)args.UserToken;
            NetSocket netSocket = conn.m_socket;
            Socket socket = (Socket)obj;

            uint bytesReceived = (uint)args.BytesTransferred;
            args.SetBuffer(null, 0, 0);
            args.Dispose(); //memleak
            
            if (bytesReceived == 0)
            {
                // indicates graceful closesure of the remote socket (TCP only)
                conn.Disconnect("Client disconnected gracefully.", 0, false, true);
                return;
            }

            uint totalBytes = conn.mPartialMessageByteCount + bytesReceived;
            conn.mPartialMessageByteCount = totalBytes;

            netSocket.m_sendBuffer.LengthBits = (int)conn.mPartialMessageByteCount * 8; // (int)bytesReceived * 8;
            netSocket.m_sendBuffer.m_readPosition = 0;
            Debug.Assert(conn.mPartialMessageByteCount > 0); // not less than 0

            // i think the problem we're having here is that mPartialMessageByteCount can be bigger than m_sendBuffer length
            // so this test although it usually works, isnt always correct to allow a ReadFrom to take place.  So the question is
            // what is it about the way we're tracking existing bytes + received bytes that could possibly make the partial count
            // more than our buffer's length?
            while (conn.mPartialMessageByteCount > 0)
            {
                // if there's not enough bytes available to read the payload, break
                if (conn.mPartialMessageByteCount < 4) // 3 byte header + at least 1 byte for variable length payload length
                    break;
                // advance over the header
                netSocket.m_sendBuffer.Position += 3 * 8; // 3 bytes * 8 bits.  Position is in bits

                // try to read the payload length
                uint maxCount = conn.mPartialMessageByteCount - 3; // 4 means we can read a full 32 payload length, or values ranging from 3 - 1 means potentially we can read the paylen, but not necessarily so we "TryReadVariableUint32" to see
                uint payLen, actualCount;
                payLen = 0;
                actualCount = 0;

                try
                {
                    // ok, seems to me the issue here is that m_sendBuffer length can potentially be smaller
                    // than .mPartialMessageByteCount 
                    if (!netSocket.m_sendBuffer.TryReadVariableUInt32(maxCount, out payLen, out actualCount))
                    {
                        if (maxCount >= 4)
                        {
                            // bad message (see FormatException below)
                            conn.mPartialMessageByteCount = 0;
                            totalBytes = 0;
                            conn.Statistics.CountBadPacketReceived((int)totalBytes, NetTime.Now);
                        }
                        // else if maxCount < 4, then we hav eno chance at being able to read the paylen, so break and we'll catch it next receipt
                        // we leave the partial and total byte counters alone 
                        break;
                    }
                    else
                    {
                        // make sure the user isnt trying to send more data than is allowed in a single message
                        if (payLen > m_config.m_maximumUserPayloadSize - 7)
                        // minus 7 bytes because our max lidgren header is 3 bytes + a max possible of 4 for holding the payLen value)
                        {
                            Debug.Assert(false, "m_config.m_maximumUserPayloadSize exceeded!  PayLen = " + payLen.ToString());  // keep this assert during testing.  You may need increase the m_maximumUserPayload value for your particular game 
                            conn.mPartialMessageByteCount = 0;
                            totalBytes = 0;
                            conn.Statistics.CountBadPacketReceived((int)totalBytes, NetTime.Now);
                            break;
                        }
                        else
                        {
                            Debug.Assert(payLen > 0);
                        }
                    }
                }
                catch (FormatException ex)
                {
                      // then this must be a bad packet, we will attempt to recover by resetting the counters and hopefully the next receive
                      // will be the start of a new message and not a fragment.  It's best we can do because without the ability to read a proper
                      // PayLen, we can't know how much more of the message might be remaining.
                      conn.mPartialMessageByteCount = 0;
                      totalBytes = 0;
                      conn.Statistics.CountBadPacketReceived((int)totalBytes, NetTime.Now); 
                }
                catch (AccessViolationException ex)
                {
                    // we might still be able to get the payLen when more data arrives
                    break;
                }
                
                uint bytesNeeded = payLen + actualCount + 3; // header amount must be added to total
                // can we read the full payload and thus advance to start of next message?
                if (bytesNeeded > conn.mPartialMessageByteCount)
                    break;

                netSocket.m_sendBuffer.Position += (int)payLen * 8; // position is in bits, not bytes
                // if we're still in the loop, the full message (header + payload) was readable, subtract total bytesNeeded and advance
                // note that we only ever subtract the full message length from header to payload. 
                conn.mPartialMessageByteCount -= bytesNeeded;
            }

            netSocket.m_sendBuffer.m_readPosition = 0;
            int completedMessagesBytes = (int)(totalBytes - conn.mPartialMessageByteCount);
            if (completedMessagesBytes > 0)
            {
                netSocket.m_sendBuffer.m_bitLength = completedMessagesBytes * 8;
                // DataReceived() always copies the contents out of the buffer prior to returning so we are free to keep using m_sendBuffer on our next socket read call
                DataReceived(netSocket.m_sendBuffer, completedMessagesBytes, (IPEndPoint)netSocket.Socket.RemoteEndPoint);
                // note: where is the Lidgren code for parsing out the seperate commands\events\messages contained in a combo packet?
                // The answer is that it's all in teh byte stream seperated by the same type of packet header.  The only difference
                // is whether it arrived in one datagram or two, and if you concatenated the two they'd be identicle.  So with TCP/IP it's the same
                // deal.  If we can add more data to a single SendAscyn() call, fine.
            }
            if (conn.mPartialMessageByteCount > 0)
            {
                // shift the remaining buffer after the DataReceived op has completed over to the 0 position by a count of "bytesToProcess"
                // and initiate the receive with an offset value so we dont overwrite existing data
                Buffer.BlockCopy(netSocket.m_sendBuffer.Data, completedMessagesBytes, netSocket.m_sendBuffer.Data, 0, (int)conn.mPartialMessageByteCount);
                Array.Clear(netSocket.m_sendBuffer.Data, (int)conn.mPartialMessageByteCount, completedMessagesBytes);
                InitiateTCPReceive(conn, true, (int)conn.mPartialMessageByteCount);
                return;
            }
          
            // initiate another receive
            InitiateTCPReceive(conn, false, 0);
        }
    
        protected virtual void OnUDPDataReceived(object obj, SocketAsyncEventArgs args)
        {
            Socket socket = (Socket)obj;
            
            // grab the buffer object and set it's lengthbits 
            NetBuffer buffer = (NetBuffer)args.UserToken;
            IPEndPoint ipsender = (IPEndPoint)args.RemoteEndPoint;
            int bytesReceived = args.BytesTransferred;
            args.SetBuffer(null, 0, 0);
            args.Dispose(); //memleak

            if (bytesReceived > 0)
            {
                buffer.LengthBits = bytesReceived * 8;
                
#if DEBUG
                try
                {
                    m_socket.Simulator.SendDelayedPackets(NetTime.Now);
                }
                catch (SocketException sex)
                {
                    if (sex.ErrorCode == 10054)
                    {
                        // forcibly closed; but m_senderRemote is unreliable, we can't trust it!
                        NetConnectionBase conn = GetConnection(ipsender);
                        HandleConnectionForciblyClosed(conn, sex);
                        return;
                    }
                }
#endif
                DataReceived(buffer, bytesReceived, ipsender);
            }
            m_bufferManager.RecycleBuffer(buffer);
        }

        /// <summary>
        /// Accepts a buffer (typically a socket buffer) containing FULL AND COMPLETED messages
        /// that have already been validated in that regard by OnTCPDataReceived or OnUDPDataReceived
        /// and copies the data OUT into a new buffer owned by a NetMessage.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesReceived"></param>
        /// <param name="endpoint"></param>
        protected void DataReceived(NetBuffer buffer, int bytesReceived, IPEndPoint endpoint)
        {
            //try
            //{
                if (bytesReceived < 1) return;

                NetConnectionBase sender = GetConnection(endpoint);
                if (sender != null)
                    sender.m_statistics.CountPacketReceived(bytesReceived, m_heartbeatCounter.LastCount);

                // create messages from packet.  Note that multiple messages can be read
                // by simply looping and resuming reads from end of previous message
                while (buffer.Position < buffer.LengthBits)
                {
                    int beginPosition = buffer.Position;

                    // read message header
                    IncomingNetMessage msg = m_messageManager.CreateIncomingMessage();
                    msg.m_sender = sender;
                    if (msg.ReadFrom(buffer, endpoint))  // buffer overflow prevention is done in msg.ReadFrom()  
                    {
                        // statistics
                        if (sender != null)
                            sender.m_statistics.CountMessageReceived(msg.m_type, msg.m_sequenceChannel,
                                                                     (buffer.Position - beginPosition) / 8,
                                                                     m_heartbeatCounter.LastCount);

                        // handle message
                        HandleReceivedMessage(msg, endpoint);
#if DEBUG
                        if (buffer.Position < buffer.LengthBits)
                            m_messageManager.NotifyApplication(NetMessageType.DebugMessage, "Multiple messages received in single packet", sender);
#endif
                    }
                }
            //}
            //catch (Exception ex)
            //{
                // TODO: really should never hit an exception in this event because we shoudl be handling malformed packets as a matter of course and not as an "exception"
                // TODO: exceptions for malformed data in a high performance server are mutually exclusive because it means a DoS attacker just needs to spam malformed packets and
                // all the excpetions thrown/handled will bring the server to its knees fast.
//#if DEBUG
                //throw new NetException("ReadPacket() exception", ex);
//#endif 
                //}
        }

        /// NOTE:
        /// I like the model lidgren has chosen to use internal messages rather than events.
        /// These messages sent to itself which the application can then read and process
        /// like any other i think is a good idea because events and callbacks suck in a threaded environment.
        /// Realizing this helps explain what the NotifyApplication messages are for
        /// and why they were done this way
        /// <summary>
        /// We will communicate with the library through messages
        /// Just as the library can comunicate with the app via messages
        /// That is, there are system messages and application messages from other users for instance.
        /// System Messages that are posted are processed first every Update() cycle.  We will
        /// manage state here, but it'll be up to the user to crank the pump and retrieve
        /// messages like Connected or Disconnected.  What we'll do here is simpler things when we're in
        /// a certain state like "Connecting" by trying to do hole punching and discovery if in server list mode.
        /// </summary>
        /// <param name="input"></param>
        public void PostMessage(string input)
        {
            NetBuffer sendBuffer = new NetBuffer();
            sendBuffer.Write(input);
        }

        public void PostMessage(byte[] input)
        {
            NetBuffer sendBuffer = new NetBuffer();
            sendBuffer.Write(input);
        }


        public NetMessage[] GetMessages()
        {
            IncomingNetMessage msg = null;
            lock (getMessages) //(m_messageManager.m_receivedMessages)
            {
                int count = m_messageManager.m_receivedMessages.Count;
                if (count == 0) return null; // Hypnotron Sept.23.2011

                NetMessage[] messages = new NetMessage[count];

                for (int i = 0; i < count; i++)
                {
                    NetMessage tmp = m_messageManager.m_receivedMessages.Dequeue();
                    System.Diagnostics.Debug.Assert(tmp != null);
#if DEBUG
                    //if (tmp == null) continue;
                    if (tmp.m_buffer == null)
                        throw new NetException("Ouch, no data!");
                    if (tmp.m_buffer.Position != 0)
                        throw new NetException("Ouch, stale data!");
#endif
                    messages[i] = tmp;
                }
                return messages;
            }
        }

        public NetMessage ReadMessage()
        {
            IncomingNetMessage msg;
            lock (getMessages) //(m_messageManager.m_receivedMessages)
                msg = m_messageManager.m_receivedMessages.Dequeue();

            if (msg == null) return null;


#if DEBUG
            if (msg.m_buffer == null)
                throw new NetException("Ouch, no data!");
            if (msg.m_buffer.Position != 0)
                throw new NetException("Ouch, stale data!");
#endif

            return msg;
        }

        /*
		/// <summary>
		/// Read any received message in any connection queue
		/// </summary>
		public NetBuffer ReadMessage(out NetConnectionBase sender)
		{
			if (m_receivedMessages.Count < 1)
			{
				sender = null;
				return null;
			}

			NetMessage msg = m_receivedMessages.Dequeue();
			sender = msg.m_sender;

			NetBuffer retval = msg.m_buffer;
			msg.m_buffer = null;
			m_messagePool.Push(msg);

			Debug.Assert(retval.Position == 0);

			return retval;
		}
		*/

        /// <summary>
        /// Read received messages only for connections specified in the onlyFor param 
        /// </summary>
        public bool ReadMessage(
            NetBuffer intoBuffer,
            List<NetConnectionBase> onlyFor,
            bool includeNullConnectionMessages,
            out NetMessageType type,
            out NetConnectionBase sender)
        {
            sender = null;
            type = NetMessageType.None;

            if (m_messageManager.m_receivedMessages.Count < 1)
            {
                return false;
            }

            IncomingNetMessage msg = null;
            lock (m_messageManager.m_receivedMessages)
            {
                int sz = m_messageManager.m_receivedMessages.Count;
                for (int i = 0; i < sz; i++)
                {
                    msg = m_messageManager.m_receivedMessages.Peek(i);
                    if (msg != null)
                    {
                        if ((msg.m_sender == null && includeNullConnectionMessages) ||
                            onlyFor.Contains(msg.m_sender))
                        {
                            m_messageManager.m_receivedMessages.Dequeue(i);
                            break;
                        }
                        msg = null;
                    }
                }
            }

            if (msg == null)
            {
                return false;
            }

            sender = msg.m_sender;

            m_messageManager.RecycleMessage(intoBuffer, msg, ref type);
            return true;
        }

        /// <summary>
        /// Read any received message in any connection queue
        /// </summary>
        public bool ReadMessage(NetBuffer intoBuffer, out NetMessageType type, out NetConnectionBase sender)
        {
            IPEndPoint senderEndPoint;
            return ReadMessage(intoBuffer, out type, out sender, out senderEndPoint);
        }

        /// <summary>
        /// Read any received message in any connection queue
        /// </summary>
        public bool ReadMessage(NetBuffer intoBuffer, out NetMessageType type, out NetConnectionBase sender, out IPEndPoint senderEndPoint)
        {

            if (intoBuffer == null)
#if DEBUG
                throw new ArgumentNullException("intoBuffer");
#else
            {
                sender = null;
                type = NetMessageType.None;
                senderEndPoint = null; ;
                return false;
            }
#endif 

            sender = null;
            type = NetMessageType.None;
            senderEndPoint = null;

            IncomingNetMessage msg;
            lock (m_messageManager.m_receivedMessages)
                msg = m_messageManager.m_receivedMessages.Dequeue();

            if (msg == null)  return false;
            
#if DEBUG
            if (msg.m_buffer == null)
                throw new NetException("Ouch, no data!");
            if (msg.m_buffer.Position != 0)
                throw new NetException("Ouch, stale data!");
#endif

            senderEndPoint = msg.m_senderEndPoint;
            sender = msg.m_sender;

            m_messageManager.RecycleMessage(intoBuffer, msg, ref type);
            return true;
        }

        /// <summary>
        /// Initiates a shutdown
        /// </summary>
        public void Shutdown(string reason)
        {
            m_messageManager.LogWrite("Shutdown initiated (" + reason + ")");
            m_shutdownRequested = true;
            m_shutdownReason = reason;
        }

        protected virtual void PerformShutdown(string reason)
        {
            m_messageManager.LogWrite("Performing shutdown (" + reason + ")");
            m_socket.ShutDown();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NetBase()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Unless we're already shut down, this is the equivalent of killing the process
            m_shutdownComplete = true;

            if (disposing)
            {
                m_socket.Dispose();
            }
        }
    }
}
