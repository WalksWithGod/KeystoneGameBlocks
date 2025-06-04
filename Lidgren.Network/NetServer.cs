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
	/// A server which can accept connections from multiple NetClients
	/// </summary>
	public abstract class NetServer : NetBase
	{
		protected List<NetConnectionBase> m_connections;
		protected Dictionary<string, NetConnectionBase> m_connectionLookup;
        
		/// <summary>
		/// Creates a new NetServer
		/// </summary>
		protected NetServer(NetConfiguration config, ProtocolType protocol)
			: base(config, protocol)
		{
			m_connections = new List<NetConnectionBase>();
			m_connectionLookup = new Dictionary<string, NetConnectionBase>();

		}

        /// <summary>
        /// Gets which port this netbase instance listens on, or -1 if it's not listening.
        /// </summary>
        public int ListenPort
        {
            get
            {
                return m_socket.ListenPort;
            }
        }

        /// <summary>
        /// Is the instance listening on the socket?
        /// </summary>
        public bool IsListening { get { return m_socket.IsBound; } } // m_isBound; } }
        /// <summary>
        /// Gets a copy of the list of connections
        /// </summary>
        public List<NetConnectionBase> Connections
        {
            get
            {
                lock (m_connections)
                {
                    return new List<NetConnectionBase>(m_connections);
                }
            }
        }

        public void Bind()
        {
            Bind(IPAddress.Any, m_config.Port);
        }

        public void Bind(string ipaddress, int port)
        {
            IPAddress address = NetUtility.Resolve(ipaddress);
            if (address == null)
                throw new NetException("Unable to resolve host");
            Bind(address, port);
        }

        /// <summary>
        /// Called to bind to socket
        /// </summary>
        public void Bind(IPAddress ipaddress, int port)
        {
            try
            {
                m_socket.Bind(ipaddress, port);
                m_messageManager.LogWrite("Listening on " + m_socket.Socket.LocalEndPoint);
            }
            catch (Exception ex)
            { 
                // TODO: sometimes when app crashes the port will stay open... and we wont be able to re-open
                string error = "Unable to Bind to port " + ex.Message;
                m_messageManager.LogWrite(error);
                throw new NetException(error);
            }

            m_shutdownComplete = false;
            m_shutdownRequested = false;
            return;
        }

        #region callback event handlers
        protected virtual void OnConnectionConnected(NetConnectionBase connection)
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
        

        public override NetConnectionBase GetConnection(IPEndPoint remoteEndpoint)
        {
            lock (m_connections)
            {
                NetConnectionBase retval;
                if (m_connectionLookup == null) m_connectionLookup = new Dictionary<string, NetConnectionBase>();
                if (m_connectionLookup.TryGetValue(remoteEndpoint.ToString(), out retval))
                    return retval;
                return null;
            }
        }

        //internal bool CreateNewConnection(NetSocket socketToUseForConnection, NetMessageManager messageManager, NetConfiguration config, IPEndPoint remoteEndpoint, byte[] localHailData, byte[] remoteHailData, out NetConnectionBase connection)
        //{
        //    lock (m_connections)
        //    {
        //        // if the connection already exists, we must return the existing connection and return false
        //        NetConnectionBase conn;

        //        if (m_connectionLookup != null)
        //        {
        //            if (m_connectionLookup.ContainsKey(remoteEndpoint.ToString()))
        //            {
        //                connection = m_connectionLookup[remoteEndpoint.ToString()];
        //                return false;
        //            }
        //        }

        //        if (socketToUseForConnection.Protocol == ProtocolType.Udp)
        //            conn = new NetUDPConnection(socketToUseForConnection, messageManager, config, remoteEndpoint, localHailData, remoteHailData);
        //        else if (socketToUseForConnection.Protocol == ProtocolType.Tcp)
        //            conn = new NetTCPConnection(socketToUseForConnection, messageManager, config, remoteEndpoint, localHailData, remoteHailData);
        //        else
        //            throw new Exception("Invalid protocol type.");

        //        conn.ConnectionEventHandler += OnConnectionConnected;

        //        //we have to add the connection as soon as it's created or else it'll jsut get created again.  
        //        if (m_connections == null)
        //            m_connections = new List<NetConnectionBase>();
        //        m_connections.Add(conn);

        //        if (m_connectionLookup == null) m_connectionLookup = new Dictionary<string, NetConnectionBase>();
        //        m_connectionLookup.Add(remoteEndpoint.ToString(), conn);

        //        connection = conn;
        //        return true;
        //    }
        //}

        private void RejectConnection(IPEndPoint senderEndpoint, NetMessageType messageType)
        {
 
        }

        internal void ProcessConnectRequest(IncomingNetMessage message, int payLen, NetSocket socketToUseForConnection,  IPEndPoint senderEndpoint, double now)
        {
            // if we're already processing a connection attempt from this endpoint, return immediately.
            // if not, we'll add a placeholder until we can create the NetConnection object
            lock (m_connectionLookup)
            {
                if (m_connectionLookup.ContainsKey (senderEndpoint.ToString ()))
                    return;
                else
                    m_connectionLookup.Add(senderEndpoint.ToString(), null); // place holder 
            }

            m_messageManager.LogVerbose("NetServer:ProcessConnectRequest() - Request received from " + senderEndpoint);

            // max connections reached?
            if (m_connections.Count >= m_config.m_maxConnections)
            {
                if ((m_messageManager.EnabledMessageTypes & NetMessageType.ConnectionRejected) ==
                    NetMessageType.ConnectionRejected)
                    m_messageManager.NotifyApplication(NetMessageType.ConnectionRejected,
                                                       "NetServer:ProcessConnectRequest() - Server full; rejecting connect from " +
                                                       senderEndpoint, null, senderEndpoint);
                lock (m_connectionLookup)
                {
                    m_connectionLookup.Remove (senderEndpoint.ToString ());
                }
                return;
            }

            if (payLen < 4)
            {
                if ((m_messageManager.EnabledMessageTypes & NetMessageType.BadMessageReceived) ==
                    NetMessageType.BadMessageReceived)
                    m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                       "NetServer:ProcessConnectRequest() - Malformed Connect message received from " +
                                                       senderEndpoint, null, senderEndpoint);
                lock (m_connectionLookup)
                {
                    m_connectionLookup.Remove (senderEndpoint.ToString ());
                }
                return;
            }

            // check app ident
            if (!NetConnectionBase.ReadAndValidateAppIdent(message, m_config.ApplicationIdentifier, m_messageManager)) 
            {
                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                       "NetServer:ProcessConnectRequest() - Client did not send correct Application Identifier " +
                                                       senderEndpoint, null, senderEndpoint);
                lock (m_connectionLookup)
                {
                    m_connectionLookup.Remove (senderEndpoint.ToString ());
                }
                return;
            }
            if (!NetConnectionBase.ReadAndValidateIdentifier(message, socketToUseForConnection.RandomIdentifier, m_messageManager)) 
            {
                m_messageManager.NotifyApplication(NetMessageType.BadMessageReceived,
                                                       "NetServer:ProcessConnectRequest() - Client did not send correct Random Identifier " +
                                                       senderEndpoint, null, senderEndpoint);
                lock (m_connectionLookup)
                {
                    m_connectionLookup.Remove (senderEndpoint.ToString ());
                }
                return;
            }

            
            // if we made it this far, then create a connection.  
            byte[] hailData = NetConnectionBase.ReadHailData(message);

            // TODO: we must prevent creation of a new connection if there's already a pending connection for this senderEndpoint
            // in progress.  Last time we tried to implement that, we broke stuff so take care...
            // The reason why this bug exists is because its assumed that once you've created a connection object with this senderEndpoint
            // that future messages from this will result in a connection object being found so the m_sender in the message is filled
            // and thus this part of the code never gets reached twice, but that's not happening, that's because
            // there is a period between when this connection is being created, and another incoming connect is already inside the 
            // DataReceived() which is where m_sender is set via GetConnection()
            m_messageManager.LogWrite("NetServer:ProcessConnectRequest() - New connection: " + senderEndpoint);
            NetConnectionBase conn;

            

            if (socketToUseForConnection.Protocol == ProtocolType.Udp)
                conn = new NetUDPConnection(socketToUseForConnection, m_messageManager, m_config, senderEndpoint, null, hailData);
            else if (socketToUseForConnection.Protocol == ProtocolType.Tcp)
                conn = new NetTCPConnection(socketToUseForConnection, m_messageManager, m_config, senderEndpoint, null, hailData);
            else
                throw new Exception("NetServer:ProcessConnectRequest() - Invalid protocol type.");

            // replace the placeholder null connection with the created one
            lock (m_connectionLookup)
            {
                m_connectionLookup[senderEndpoint.ToString()] = conn;
            }

            conn.ConnectionEventHandler += OnConnectionConnected;

            // if we first need to get approval for all connections, then the connection will get added by the host apps explict call to ApproveConnection()
            if ((m_messageManager.EnabledMessageTypes & NetMessageType.ConnectionApproval) ==  NetMessageType.ConnectionApproval)
            {
                // Ask application if this connection is allowed to proceed
                IncomingNetMessage app = m_messageManager.CreateIncomingMessage();
                app.m_msgType = NetMessageType.ConnectionApproval;
                if (hailData != null)
                    app.m_buffer.Write(hailData);
                app.m_senderEndPoint = senderEndpoint;
                app.m_sender = conn;
                conn.m_approved = false;
                // Don't add connection; it's done by the application
                // enqueue as message for the host application to handle where it must call NetServer.ApproveConnection() to approve it
                m_messageManager.Enqueue(app);
                return;
            }

            //  no approval from the host application needed, we can approve right away by ourself.
            ApproveConnection(now, conn, null);
        }

        /// <summary>
        /// Approves the connection and sends any (already set) local hail data
        /// </summary>
        public void ApproveConnection(NetConnectionBase conn)
        {
            ApproveConnection(NetTime.Now, conn, null);
        }

        /// <summary>
        /// Approves the connection and sents/sends local hail data provided
        /// </summary>
        public void ApproveConnection(NetConnectionBase conn, byte[] hailData)
        {
            if (conn.m_approved == true)
                throw new NetException("NetServer.ApproveConnection() - Connection is already approved!");

            ApproveConnection(NetTime.Now, conn, hailData);
        }

        public virtual void ApproveConnection(double now, NetConnectionBase conn, byte[] hailData)
        {
            lock (conn)
            {
                if (conn.m_approved) return;
                conn.m_approved = true;
                
                if (m_config.BannedIPAddresses != null)
                    // TODO: perhaps more sophisticated evaluations are required such as wildcards instead of just "Contains" an exact match
                    if (m_config.BannedIPAddresses.Contains(conn.RemoteEndpoint.Address.ToString()))
                    {
                        conn.m_approved = false;
                        conn.Disconnect("NetServer.ApproveConnection() - Banned IP Address", 0, false, true);
                        // note: here since .Disconnect() is called, maintenance will remove from both m_connections and m_connectionLookup
                    }

                conn.m_handshakeInitiated = now;
                conn.m_localHailData = hailData;  // store the hail data in case we need to re-send guaranteed connect response

                conn.SetStatus(NetConnectionStatus.Connecting, "NetServer.ApproveConnection() - Connecting");
                m_messageManager.LogWrite("NetServer.ApproveConnection() - Adding connection " + conn);

                lock (m_connections)
                    m_connections.Add(conn);                
            }
        }

        /// <summary>
        /// Broadcasts a message to all Connected and Approved connections not in the excludeList
        /// utilizing a single NetBuffer for the m_sendBuffer of each connection.  
        /// NOTE: This function should be used with care because it is possible that while you are preparing
        /// to Broadcast and even while you are itterating the m_connections below, new connections
        /// may be being added resulting in a packet being sent to a connection not originally intended.
        /// The short version is, if it's not critical that a message be sent only to the people in the connection
        /// list at the time Broadcast() is called, then use it.  Otherwise use Groupcast() with an explicity list of 
        /// connections to use.
        /// </summary>
        /// <param name="remoteable"></param>
        /// <param name="broadcastGroupID"></param>
        /// <param name="excludeList"></param>
        public void Broadcast(IRemotableType remoteable, int broadcastGroupID,  NetConnectionBase[] excludeList)
        {
            NetBuffer buffer = m_bufferManager.CreateBuffer();
            buffer.Write(remoteable.Type);
            remoteable.Write(buffer);

            bool excluded = false;

            // perhaps one issue with this Broadcast is that if new connections are bieng added to m_connections
            // then even though we've sychronized, we wind up sending to the new connections 
            foreach (NetConnectionBase conn in m_connections)
            {
                if (conn.m_broadcastGroup == broadcastGroupID)
                {
                    if (excludeList != null)
                        for (int i = 0; i < excludeList.Length; i++)
                        {
                            if (excludeList[i] == conn)
                            {
                                excluded = true;
                                break;
                            }
                        }

                    if (!excluded) //&& conn.Status == NetConnectionStatus.Connecting)
                        if (conn.m_approved)
                            conn.SendMessage(buffer, remoteable.Channel);

                    excluded = false;
                }
            }
        }

        public void Broadcast(IRemotableType remoteable, NetConnectionBase[] excludeList)
        {
            Broadcast(remoteable, 0, excludeList);
            //NetBuffer buffer = m_bufferManager.CreateBuffer();
            //buffer.Write(remoteable.ID);
            //remoteable.Write(buffer);

            //bool excluded = false;

            //// perhaps one issue with this Broadcast is that if new connections are bieng added to m_connections
            //// then even though we've sychronized, we wind up sending to the new connections 
            //foreach (NetConnectionBase conn in m_connections)
            //{
            //    if (excludeList != null)
            //        for (int i = 0; i < excludeList.Length; i++)
            //        {
            //            if (excludeList[i] != conn) continue;
            //            excluded = true;
            //            break;
            //        }

            //    if (!excluded ) //&& conn.Status == NetConnectionStatus.Connecting)
            //        if (conn.m_approved)
            //            conn.SendMessage(buffer, remoteable.Channel);

            //    excluded = false;
            //}
        }

        public void Broadcast(IRemotableType remoteable, int broadcastGroupID)
        {
            Broadcast(remoteable, broadcastGroupID, null);
        }

        public void Broadcast(IRemotableType remoteable)
        {
            Broadcast(remoteable, null);
        }
        
        /// <summary>
        /// Sends a message to all Connected and Approved connections in the groupList
        /// </summary>
        /// <param name="remoteable"></param>
        /// <param name="groupList"></param>
        public void Groupcast (IRemotableType remoteable, NetConnectionBase[] groupList)
        {
            NetBuffer buffer = m_bufferManager.CreateBuffer();
            buffer.Write(remoteable.Type);
            remoteable.Write(buffer);

            if (groupList == null) return;
            foreach (NetConnectionBase conn in groupList)
            {
                //if (conn.Status == NetConnectionStatus.Connecting) // TODO: fix .Status since the state doesnt go to .Connected and stays at .Connecting
                    if (conn != null && conn.m_approved)
                        conn.SendMessage(buffer, remoteable.Channel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="now"></param>
        protected void ConnectionMaintenance(double now)
        {
            if (m_connections == null) return;
            lock (m_connections) 
            {
                // find new dead connections 
                List<NetConnectionBase> deadConnections = null;
                foreach (NetConnectionBase conn in m_connections)
                {
                    // Hypno - added misbehaving client check
                    if ((conn.m_status != NetConnectionStatus.Disconnected &&
                        (conn.Statistics.GetBadMessagesReceived( false) < m_config.MaxBadMessages
                        && conn.Statistics.GetBadBytesReceived(false) < m_config.MaxBadBytesReceived)))

                        conn.Heartbeat(now);
                    else
                    {
                        if (conn.m_status != NetConnectionStatus.Disconnected )
                            conn.Disconnect ("Too many bad packets.", 0, false, true);

                        if (deadConnections == null)
                            deadConnections = new List<NetConnectionBase>();
                        deadConnections.Add(conn);
                        continue;
                    }
                }

                // remove dead connections
                if (deadConnections != null)
                {
                    foreach (NetConnectionBase conn in deadConnections)
                    {
                        m_connections.Remove(conn);
                        m_connectionLookup.Remove(conn.RemoteEndpoint.ToString());
                        m_messageManager.LogWrite("Removing dead connection ", conn);
                    }
                }
            }
        }

		/// <summary>
		/// Reads and sends messages from the network
		/// </summary>
		public override void Heartbeat()
		{
		    base.Heartbeat();
            
            // process application posted internal messages

            // note: incoming messages from outside the application (e.g. from a server or client)
            // should be processed automatically in the asychronous recieve event?  Or no? 
            // I think i decided to just queue those messages and process them later right?  yes im nearly positive
            // the reason was because our server app like our AuthenticationService might want to
            // multithread that using a seperate pool.  Why not let the asychreceivefrom threads do that?
            // Well actaully part of that reasoning was for versions of the server that also must
            // run a simulation loop so we wouldn't want these threads that called back on completion
            // to process messages in a way tha tmade it difficult to have consistent temporal simulation for all clients.
            // Also perhaps for our authentication most of those threads would block since we wouldn't have
            // enough database connections, so this way at least we can store the messages and only process
            // them in batches every itteration as we drain the queu
            
            m_messageManager.SendOutgoing(m_heartbeatCounter.LastCount);
            ConnectionMaintenance(m_heartbeatCounter.LastCount);
		}

       
	
		internal override void HandleConnectionForciblyClosed(NetConnectionBase connection, SocketException sex)
		{
			if (connection != null)
				connection.Disconnect("Connection forcibly closed", 0, false, false);
			return;
		}

		protected override void PerformShutdown(string reason)
		{
			foreach (NetConnectionBase conn in m_connections)
				if (conn.m_status != NetConnectionStatus.Disconnected)
					conn.Disconnect(reason, 0, true, true);
			base.PerformShutdown(reason);
		}
	}
}
