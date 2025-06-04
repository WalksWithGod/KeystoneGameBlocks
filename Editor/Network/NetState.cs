using System;


namespace KeyEdit.Network
{
    public class NetState
    {
        protected int mEnteredStateTick;

        protected NetState ()
        {
            mEnteredStateTick = Environment.TickCount;
        }

        public virtual void Execute(INetManager  client)
        {
 
        }

        
    }
}
