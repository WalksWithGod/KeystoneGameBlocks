//using System;
//using System.Collections.Generic;
//using Lidgren.Network;

//namespace KeyEdit.Network
//{
//    public abstract class NetworkClientBase
//    {
//        public enum NetworkStatus
//        {
//            ConnectRequest,
//            Authenticating,
//            Authenticated,

//            Connected,
//            Connecting,
//            Disconnected
//        }

//        protected Lidgren.Network.NetClient mGameClient;
//        protected string mGameServerName;
//        protected string mGameServerAddress = "localhost";
//        protected int mGameServerPort = 0;

//        protected Settings.Initialization mINI;
//        protected int mUserID;
//        protected string mUserName;
//        protected string mPassword;

//        protected NetworkStatus mStatus;
//        protected int mMaxAuthenticationTries = 4;
//        protected int mMaxConnectTries = 2;
//        protected int mCurrentAuthenticationAttempts;
//        protected int mCurrentConnectAttempts;

//        // in a multi-edit environment could we potentially have a need to specify the sender of the message? 
//        // let's not worry about that for now.  we'll assume single player is single player.
//        public delegate void UserMessageReceived(int command, NetBuffer buffer);
//        public UserMessageReceived UserMessageReceivedHandler; 

//        public NetworkClientBase (Settings.Initialization ini)
//        {
//            mINI = ini;
//            string sectionName = "network";
//            mUserName = mINI.settingRead(sectionName, "username");
//            mPassword = mINI.settingRead(sectionName, "password");

//            mStatus = NetworkStatus.Disconnected;
//            mCurrentAuthenticationAttempts = 0;
//            mCurrentConnectAttempts = 0;
//        }

//        public NetworkStatus Status { get { return mStatus; } }

//        public virtual void ConnectTo(string host, int port)
//        {
//            System.Diagnostics.Debug.WriteLine("NetworkClientBase.ConnectTo() - Connecting to " + host + " at port " + port.ToString());
//            mGameClient.Connect(host, port);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <remarks></remarks>
//        public void Disconnect()
//        {
//            mStatus = NetworkStatus.Disconnected;
//        }

//        public abstract void Update();



//        public void SendMessage(IRemotableType command)
//        {
//            mGameClient.SendMessage(command);
//        }


//        protected virtual void ProcessUserMessage(IncomingNetMessage message)
//        {

//            int command = message.Buffer.ReadInt32();
           
//            switch (command)
//            {
                   
//                    // TODO: here instead of sending the message we just send command and buffer
//                    //         is this ok?  Is there no direct peer to peer communications for instance where
//                    //         we'd need to know a user's sender endpoint to verify?  or all coms just go thru server
//                    //         is probably best because then we know the server verified the sender's authenticity
//                default:
//                    if (UserMessageReceivedHandler != null)
//                        UserMessageReceivedHandler.Invoke(command, message.Buffer);
//                    else
//                        System.Diagnostics.Debug.WriteLine("ProcessUserMessage() - Unsupported message type '" + command.ToString() + "'.");

//                    break;
//            }
//        }
//    }
//}
