using System;
using Keystone.Types;

namespace Keystone.Events
{
    public enum EventType
    {
        MouseMove,
        MouseEnter,
        MouseLeave,
        MouseDown,
        MouseUp,
        MouseWheel,
        MouseLeftClick,
        MouseRightClick,
        MouseDoubleClick,
        KeyboardKeyDown,
        KeyboardKeyUp,
        KeyboardKeyPress,
        KeyboardWMChar,
        KeyboardCancel
    }

    public class MouseEventArgs : EventArgs
    {
        public System.Drawing.Point ViewportRelativePosition; // aka mouse 2d position
        public Vector3d UnprojectedCameraSpacePosition;                  // in camera space
        public Enums.MOUSE_BUTTONS Button;
        public Cameras.Viewport Viewport;
        public object Data;
    }

    public class KeyboardEventArgs : EventArgs
    {
        public string Key;
        public bool IsPressed;
        public object Data;
    }
}
