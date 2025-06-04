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

namespace Lidgren.Network
{
	/// <summary>
	/// Configuration for a NetBase derived class
	/// </summary>
	public sealed class NetConfiguration
	{
		internal int m_port;
		internal string m_appIdentifier;
		internal int m_receiveBufferSize, m_sendBufferSize;
		internal int m_maxConnections;
		internal int m_maximumTransmissionUnit;
        internal int m_maximumUserPayloadSize;
		internal float m_pingFrequency;
		internal float m_timeoutDelay;
		internal int m_handshakeAttemptsMaxCount;
		internal float m_handshakeAttemptRepeatDelay;
		internal float m_resendTimeMultiplier;
		internal float m_maxAckWithholdTime;
		internal float m_disconnectLingerMaxDelay;
		internal float m_throttleBytesPerSecond;
		internal bool m_useBufferRecycling;
		internal bool m_answerDiscoveryRequests;
	    internal int m_bufferManagerTimeout;
	    internal int m_maxOutstandingAccepts;
	    internal int m_maxOutstandingReads;

        internal List<string> m_bannedIPAddresses;

        // misbehaving remote connections
        internal int m_maxBadMessages;     // TODO: maybe for these bad messages / bytes per hour is better?
        internal int m_maxBadBytesReceived;

		/// <summary>
		/// Gets or sets the string identifying this particular application; distinquishing it from other Lidgren based applications. Ie. this needs to be the same on client and server.
		/// </summary>
		public string ApplicationIdentifier { get { return m_appIdentifier; } set { m_appIdentifier = value; } }

        public List<string> BannedIPAddresses { get { return m_bannedIPAddresses; } set { m_bannedIPAddresses = value; } }

		/// <summary>
		/// Gets or sets the local port to bind to
		/// </summary>
		public int Port
		{
			get { return m_port; }
			set
			{
				m_port = value;
				//if (m_port > 0 && m_port < 1024)
				//	m_owner.LogWrite("Warning: Ports 1-1023 are reserved");
			}
		}

		public int ReceiveBufferSize { get { return m_receiveBufferSize; } set { m_receiveBufferSize = value; } }
		public int SendBufferSize { get { return m_sendBufferSize; } set { m_sendBufferSize = value; } }

		/// <summary>
		/// Gets or sets if buffer recycling is used
		/// </summary>
		public bool UseBufferRecycling
		{
			get { return m_useBufferRecycling; }
			set { m_useBufferRecycling = value; }
		}

		/// <summary>
		/// Gets or sets how many simultaneous connections a NetServer can have
		/// </summary>
		public int MaxConnections { get { return m_maxConnections; } set { m_maxConnections = value; } }

        /// <summary>
        /// Gets or sets how many simultaneous outstanding Accept Connection handlers a NetTCPServer (TCP only) can have
        /// </summary>
        public int MaxOutstandingAccepts { get { return m_maxOutstandingAccepts; } set { m_maxOutstandingAccepts = value; } }

        /// <summary>
        /// Gets or sets how many simultaneous outstanding Asychronous Reads a UDPServer (UDP only) can have
        /// </summary>
        public int MaxOutstandingReads { get { return m_maxOutstandingReads; } set { m_maxOutstandingReads = value; } }

        /// <summary>
		/// Gets or sets how many bad messages a remote connection can send before it's forceably disconnected
		/// </summary>
        public int MaxBadMessages { get { return m_maxBadMessages; } set { m_maxBadMessages = value; } }

        /// <summary>
		/// Gets or sets how many bad bytes a remote connection can send before it's forceably disconnected
		/// </summary>
        public int MaxBadBytesReceived { get { return m_maxBadBytesReceived; } set { m_maxBadBytesReceived = value; } }


		/// <summary>
		/// Gets or sets how many bytes can maximally be sent using a single packet
		/// </summary>
		public int MaximumTransmissionUnit { get { return m_maximumTransmissionUnit; } set { m_maximumTransmissionUnit = value; } }

        /// <summary>
        /// Gets or sets how many bytes a user can send in a single packet.  We can use this to prevent buffer overflows.
        /// This is for TCP only since a TCP stream can go on forever whereas UDP packets are discrete and thus are never larger than
        /// the m_receiveBufferSize 
        /// </summary>
        public int MaximumUserPayloadSize {
            get { return m_maximumUserPayloadSize; }
            set { m_maximumUserPayloadSize = value; } }


		/// <summary>
		/// Gets or sets the number of seconds between pings
		/// </summary>
		public float PingFrequency { get { return m_pingFrequency; } set { m_pingFrequency = value; } }

		/// <summary>
		/// Gets or sets the time in seconds before a connection times out when no answer is received from remote host
		/// </summary>
		public float TimeoutDelay { get { return m_timeoutDelay; } set { m_timeoutDelay = value; } }

		/// <summary>
		/// Gets or sets the maximum number of attempts to connect to the remote host
		/// </summary>
		public int HandshakeAttemptsMaxCount { get { return m_handshakeAttemptsMaxCount; } set { m_handshakeAttemptsMaxCount = value; } }

		/// <summary>
		/// Gets or sets the number of seconds between handshake attempts
		/// </summary>
		public float HandshakeAttemptRepeatDelay { get { return m_handshakeAttemptRepeatDelay; } set { m_handshakeAttemptRepeatDelay = value; } }

		/// <summary>
		/// Gets or sets the multiplier for resend times; increase to resend packets less often
		/// </summary>
		public float ResendTimeMultiplier { get { return m_resendTimeMultiplier; } set { m_resendTimeMultiplier = value; } }

		/// <summary>
		/// Gets or sets the amount of time, in multiple of current average roundtrip time,
		/// that acknowledges waits for other data to piggyback on before sending them explicitly
		/// </summary>
		public float MaxAckWithholdTime { get { return m_maxAckWithholdTime; } set { m_maxAckWithholdTime = value; } }

		/// <summary>
		/// Gets or sets the number of seconds allowed for a disconnecting connection to clean up (resends, acks)
		/// </summary>
		public float DisconnectLingerMaxDelay { get { return m_disconnectLingerMaxDelay; } set { m_disconnectLingerMaxDelay = value; } }

		/// <summary>
		/// Gets or sets if a NetServer/NetPeer should answer discovery requests
		/// </summary>
		public bool AnswerDiscoveryRequests { get { return m_answerDiscoveryRequests; } set { m_answerDiscoveryRequests = value; } }

        /// <summary>
        /// Gets or sets the timeout for NetBuffer's being maintained in the NetBufferManager pool
        /// </summary>
        public int BufferManagerTimeout { get { return m_bufferManagerTimeout; } set { m_bufferManagerTimeout = value; } }


		/// <summary>
		/// Gets or sets the amount of bytes allowed to be sent per second; set to 0 for no throttling
		/// </summary>
		public float ThrottleBytesPerSecond
		{
			get { return m_throttleBytesPerSecond; }
			set
			{
				m_throttleBytesPerSecond = value;
				//if (m_throttleBytesPerSecond < m_maximumTransmissionUnit)
				//	LogWrite("Warning: Throttling lower than MTU!");
			}
		}

		public NetConfiguration(string appIdentifier)
		{
			m_appIdentifier = appIdentifier;
			m_port = 0;

            m_receiveBufferSize = (ushort.MaxValue * 2) + 1; // uShort.MaxValue * 2 + 1 == 131071; 
			m_sendBufferSize = 131071; // 128KB - 1 
            m_maximumTransmissionUnit = 1459; // note: there is no check but maximumTransmissionUnit should be smaller than receive and send buffer sizes
            m_maximumUserPayloadSize = 16384; // this value should be set less than the m_receiveBufferSize
			m_maxConnections = 32;
			
			m_pingFrequency = 3.0f;
			m_timeoutDelay = 300.0f; // 300 seconds == 5 minute time out
			m_handshakeAttemptsMaxCount = 5;
			m_handshakeAttemptRepeatDelay = 2.5f;
			m_maxAckWithholdTime = 0.5f; // one half RT wait before sending explicit ack
			m_disconnectLingerMaxDelay = 3.0f;
			m_resendTimeMultiplier = 1.1f;
			m_answerDiscoveryRequests = true;
			m_useBufferRecycling = false; // TODO: enable this when sure there's no threading issues
		    m_bufferManagerTimeout = 5000;
		    m_maxOutstandingAccepts = 8;
		    m_maxOutstandingReads = 32;
            m_maxBadMessages = 100;
            m_maxBadBytesReceived = 1000000;
            m_bannedIPAddresses = null; // nothing banned by default
		}
	}
}
