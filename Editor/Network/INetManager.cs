using System;
using Lidgren.Network;

namespace KeyEdit.Network
{
    public interface INetManager
    {
        Lidgren.Network.NetClient NetClient { get; }
                        
        long UserID { get; }
        string UserName { get; }
        string Password { get; }

        string ServerAddress { get; set; }
        int ServerPort { get; set; }

        //NetState State { get; set; }
        bool AutoReconnect { get; set; }
        int MaxRetries { get; set; }
        int CurrentRetryAttempt { get; }


        void CommandProc(IRemotableType type);
        void SendCommand(IRemotableType command);
        void Connect();
        void Disconnect();
        void Update();

    }
}
