using System;
using System.Collections.Generic;
using System.Text;

namespace Keystone.Events
{
    /// <summary>
    /// A repeatable event is one where holding down of a button results in automatic re-firing / handling of the event
    /// until a release/stop of the event occurs.  Thus a repeatable event contains private vars related to controlling
    /// the automatic repetition of the event including any conditions that stop the event such as running out of ammo.
    /// </summary>
    public abstract class RepeatableEvent : IEventHandler
    {


        #region IEventHandler Members

        public void HandleEvent(EventType et, InputCaptureEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
