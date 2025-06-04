using System;

namespace Keystone.Events
{
    public interface IEventHandler  // can handle game event triggers
    {
        void HandleEvent(EventType et, InputCaptureEventArgs args);
    }

    public class InputCaptureEventArgs : EventArgs
    {
        public Keystone.Collision.PickResults PickResults;
        public MouseEventArgs MouseEventArgs;

        public InputCaptureEventArgs(MouseEventArgs margs, Keystone.Collision.PickResults pr)
        {
            MouseEventArgs = margs;
            PickResults = pr;
        }
    }

    // TODO: why not simply name IControl ?
    public interface IInputCapture : IEventHandler // for input specific events.  
    {
        /// <summary>
        /// 
        /// </summary>
        bool InputCaptureEnable { get; set; }
        bool HasInputCapture { get; }

        event EventHandler KeyboardCancel;
        event EventHandler MouseMove;
        event EventHandler MouseEnter;
        event EventHandler MouseLeave;
        event EventHandler MouseDown;
        event EventHandler MouseUp;
        event EventHandler MouseClick;
        event EventHandler MouseDrag;
    }
}
