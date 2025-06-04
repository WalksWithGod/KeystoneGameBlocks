using System;
using Authentication;
using Lidgren.Network;
using System.Diagnostics;

namespace KeyEdit.Network
{
     /// <summary>
    /// Class to simplify client application's network handling.  This does not process game specific messages
    /// it only handles the connection / disconnection and authentication related matters.  The 
    /// client application must still poll to grab messages that have arrived 
    /// </summary>
    /// <remarks></remarks>
    public class LoopBackClient : InternetClient  
    {   
	    internal ConsoleOut mConsoleDelegate;

	    public delegate void ConsoleOut(string message);


	    public LoopBackClient(Settings.Initialization  ini, ConsoleOut consoleOutHandler) : base (ini)
	    {
		    mConsoleDelegate = consoleOutHandler;

		    // apply the configuration to the lidgren NetConfiguration object
            NetConfiguration lidgrenConfig = new NetConfiguration("KeyGameServerSvc");
            string sectionName = "lidgren";
            lidgrenConfig.SendBufferSize = mINI.settingReadInteger(sectionName, "sendbuffersize");
            lidgrenConfig.ReceiveBufferSize = mINI.settingReadInteger(sectionName, "receivebuffersize");
            lidgrenConfig.UseBufferRecycling = mINI.settingReadBool(sectionName, "enablebufferrecycling");
            lidgrenConfig.MaximumUserPayloadSize = mINI.settingReadInteger(sectionName, "maxuserpayloadsize");

            // TODO: NOTE: Game client and Game server must both be either UDP or TCP/IP
            mGameClient  = new Lidgren.Network.NetTCPClient(lidgrenConfig);
            mGameClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, mINI.settingReadBool(sectionName, "enablerejectmessages"));
            mGameClient.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, mINI.settingReadBool(sectionName, "enableconnectionapproval"));
            mGameClient.SetMessageTypeEnabled(NetMessageType.DebugMessage, mINI.settingReadBool(sectionName, "enabledebugmessages"));
            mGameClient.SetMessageTypeEnabled(Lidgren.Network.NetMessageType.VerboseDebugMessage, mINI.settingReadBool(sectionName, "enableverbosemessages"));
            

		    //lidgrenConfig.MaximumTransmissionUnit = 
		    //lidgrenConfig.Port = NETWORK_PORT

		    //lidgrenConfig.ThrottleBytesPerSecond  
		    //lidgrenConfig.TimeoutDelay = 
		    //lidgrenConfig.DisconnectLingerMaxDelay = 
		    //lidgrenConfig.HandshakeAttemptRepeatDelay = ' not used for TCP Client or Server
		    //lidgrenConfig.HandshakeAttemptsMaxCount =   ' not used for TCP Client or Server
		    //lidgrenConfig.AnswerDiscoveryRequests = False ' not used for TCP Client or Server
		    //lidgrenConfig.ResendTimeMultiplier =               ' not used for TCP Client or Server
		    //'lidgrenConfig.MaxAckWithholdTime =        ' not used for TCPClient
		    //'lidgrenConfig.PingFrequency =                 ' not used for TCPClient
		    //lidgrenConfig.MaxConnections                  ' not used for TCPClient
	    }

        public override void Update()
        {
            mGameClient.Heartbeat();
            Lidgren.Network.NetMessage[] messages = mGameClient.GetMessages();
            if (messages == null) return;

            foreach (NetMessage msg in messages)
            {
                if (msg == null) continue;
                switch (msg.Type)
                {
                    case NetMessageType.ServerDiscovered:
                        // just connect to any server found!
                        //mGameClient.Connect(msg.Buffer.ReadIPEndPoint(), "Hail from" + Environment.MachineName);
                        break;
                    case NetMessageType.ConnectionRejected:
                        Debug.WriteLine("LoopBackClient.Update() -- Connection Rejected: " + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.DebugMessage:
                    case NetMessageType.VerboseDebugMessage:
                        Debug.WriteLine("LoopBackClient.Update() - Debug:" + msg.Buffer.ReadString());
                        break;
                    case NetMessageType.StatusChanged:
                        Debug.WriteLine("LoopBackClient.Update() - Status Changed: " + mGameClient.Status + " (" + msg.Buffer.ReadString() + ")");
                        // if we're now connected, TODO: anything special to do here? Grab player profile
                        // create "Player" object
                        // load "Vehicle" object
                        break;
                    case NetMessageType.BadMessageReceived:
                        Debug.WriteLine("LoopBackClient.Update() - Bad Message Received: " + msg.Buffer.ReadString());
                        break;

                    case NetMessageType.Data:
                        // The game server sent this data 
                        ProcessUserMessage((IncomingNetMessage)msg);
                        break;
                    default:
                        Debug.WriteLine(string.Format("LoopBackClient.Update() - Unexpected message type '{0}'", msg.Type.ToString()));
                        break;
                }
            }
            //if (mDownloadManager != null)
            //{
            //    //  get the progress and update any progress bars
            //    //  get the completed file infos so we can update our scene (eg. to render a tile that's been waiting to be finished downloading)
            //    Downloader.FileDownloadTask[] completedFiles = mDownloadManager.CompletedFiles(true);
            //}
        }
    }
}
