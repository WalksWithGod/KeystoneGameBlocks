using System;

namespace Keystone.Events
{
    public interface IEventConsumer
    {
        void ConsumeEvent(IEvent ievent);
        void OnEventConsumed(IEvent ievent);

    }
}
