using System;
using System.Runtime.InteropServices;


namespace Keystone.Devices
{
    // TODO: read this again!!!! http://blog.ngedit.com/2005/06/13/whats-broken-in-the-wm_keydownwm_char-input-model/
    //  this could cause us problems :(   need to think about this
    // and this  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/keyboardinput/aboutkeyboardinput.asp

    // TODO: The below snippet from Zaknafein requires a 256 byte array "state" passed into the ToAsciiEx function.
    //       SO what i need to do is build my own KBState class that contains a ToArray() method that outputs the bytes
    //       and have my keyboard device class update that myself.  This will be trivial since i already know the keyup/keydown events for 
    //       every key and so all i need then is when getting the buffered input, to also store the buffered characters for that frame
    //       too and so when the IOController's Notify is fired, after the keyboard device has Notified in its internal loop
    //       all of the buffered keys individually, i can provide a final notify for the entire text and set a flag that
    //       "endofBuffer" is reached so we know we can grab it.  Alternatively, we can grab them one at a time too as they are available.
    //       This just means that sometimes thee wont be any printable character, but sometimes there will be.  Thats probably the better method.
    /// <summary>
    /// Public domain code by Zaknafein
    /// http://www.truevision3d.com/forums/old_forums/old_topic-t11510.0.html
    /// The information class for text handling (buffered input) and gets the ASCII value equivalent
    /// of a directx scancode regardless of keyboard layout.
    /// </summary>
    public class KeyboardState
    {
        private string _text;
        private bool _return, _backspace;

        private static IntPtr _keyboardLayout;
        //Microsoft.DirectX.DirectInput.KeyboardState _state;
        private static byte[] _state;

        #region Externs

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        private static extern int ToAsciiEx(uint uVirtKey, uint uScanCode, byte[] lpKeyState, ushort[] lpChar,
                                            uint uFlags, IntPtr dwhkl);

        #endregion

        // TODO: convert code to work with just our buffered input.
        //public KeyboardState(TV_KEYDATA[] pressedKeys, int number)
        //{
        //    StaticInitialization();

        //    _text = "";
        //    // -- Interpretate the keys
        //    for (int i = 0; i < number; i++)
        //    {
        //        if (pressedKeys[i].Pressed == 1)
        //        {
        //            // Differentiate backspace and return from the rest
        //            Microsoft.DirectX.DirectInput.Key scancode = (Microsoft.DirectX.DirectInput.Key)pressedKeys[i].Key;
        //            if (scancode == Microsoft.DirectX.DirectInput.Key.Return)
        //            {
        //                _return = true;
        //            }
        //            else if (scancode == Microsoft.DirectX.DirectInput.Key.BackSpace )
        //            {
        //                _backspace = true;
        //            }
        //            else
        //            {
        //                // Transfer to ASCII
        //                char c = ScanToASCII(scancode);
        //                if (c != (char)0)
        //                    _text += c;
        //            }
        //        }
        //    }
        //}

        public void Clear()
        {
            // TODO: clear the states of all bytes in the _state array    
        }

        public string BufferedText
        {
            get { return _text; }
        }

        public bool IsEmpty
        {
            get { return _text.Equals("") && !_return && !_backspace; }
        }

        public bool BackspacePressed
        {
            get { return _backspace; }
        }

        public bool ReturnPressed
        {
            get { return _return; }
        }

        private static void StaticInitialization()
        {
            if (_keyboardLayout == IntPtr.Zero)
            {
                _keyboardLayout = GetKeyboardLayout((uint) 0);
            }
            if (_state == null)
            {
                _state = new byte[256];
            }
        }

        private static char ScanToASCII(Microsoft.DirectX.DirectInput.Key scancode)
        {
            if (!GetKeyboardState(_state))
                return (char) 0;

            uint virtualKey = MapVirtualKeyEx((uint) scancode, (uint) 1, _keyboardLayout);
            ushort[] result = new ushort[2];
            ToAsciiEx(virtualKey, (uint) scancode, _state, result, (uint) 0, _keyboardLayout);
            return (char) result[0];
        }
    }
}