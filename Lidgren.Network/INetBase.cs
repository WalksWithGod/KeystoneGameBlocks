using System.Net;

namespace Lidgren.Network
{
    internal interface INetBase
    {

        NetBuffer ScratchBuffer { get; }
        void SendSingleUnreliableSystemMessage(NetSystemType tp,
			NetBuffer data,
			IPEndPoint remoteEP,
			bool useBroadcast);
        void QueueSingleUnreliableSystemMessage(NetSystemType tp,
			NetBuffer data,
			IPEndPoint remoteEP,
			bool useBroadcast);
    }
}
