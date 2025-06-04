using System;

namespace Keystone.EditTools
{
    public class Eraser : Tool
    {
        public Eraser(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {}

        public override void HandleEvent(Keystone.Events.EventType type, EventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
