using System;
using System.Collections.Generic;
using Keystone.Events;

namespace Sculptor.Tools
{
    public abstract class Tool : ITool
    {
        
        public abstract void HandleEvent(EventType type, EventArgs args);

    }
}
