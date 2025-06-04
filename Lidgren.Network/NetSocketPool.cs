using System;
using System.Collections.Generic;

namespace Lidgren.Network
{
    internal class NetSocketPool : NetObjectPool <NetSocket>
    {
        public NetSocketPool(int expirationTime) : base(expirationTime)
        {
        }

        protected override NetSocket Create()
        {
            throw new NotImplementedException();
        }

        protected override bool Validate(NetSocket obj, object context)
        {
            throw new NotImplementedException();
        }



    }
}
