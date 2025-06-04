//#define TVKEYBOARD = true
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Interfaces;
using Microsoft.DirectX.DirectInput;

namespace Keystone.Devices
{
    public class Keyboard : DirectInputDevice
    {


        #region Use WM_CHAR messages for text
        /*
Posted 02 December 2001 - 12:52 PM

DO NOT use DirectInput to get text input. Don't use WM_KEYUP or WM_KEYDOWN messages either!!

DirectInput treats a keyboard like a big joypad with ~102 buttons on it. The virtual key codes from the WM_KEY* messages just tell you the state of a single key.

The reasons ?:
1. Certain european languages have accented characters (e.g. φυςστ) for which there aren''t enough keys on a keyboard to represent each with its own key. On those keyboards, they have a key which represents the type of accent without the letter, you press this key first, THEN the letter. 
So for example to get an accented character on a Danish keyboard you press 2 keys, one after the other - NOT together!

2. The codes in DirectInput and the virtual key codes map to the US keyboard layout - on a German keyboard for example what ASCII code would you display for DIK_Y ? if you say "Y", you''d be wrong!! - a German Keyboard has a layout of QWERTZ rather than QWERTY. Pressing Z gives a DIK_Y... The French keyboard is even more scary!. This is because DInput is just saying "this button was pressed" - it doesn''t care what the sticker on the keyboard might say something different.

3. On top of that, the user may have chosen to use a different keymap with their keyboard. For example I once did some work in Denmark - although the machines I was using had Danish keyboards, I loaded a UK keymap because that''s what I''m used to. DInput and the VK_ codes ignores any soft keymappings loaded.

4. In languages with many more characters than a keyboard supports such as Japanese, Chinese, Korean etc, Windows comes with an IME ("Input Method Editor") where characters can be entered in a few ways - one is spelling the word phonetically where a single character might be the result of say 4 keypresses. Another way of using the IME is to select characters from an on-screen menu. DInput and VK_ codes once again go in below all this.

5. Likewise there are some alternative keyboard types for disabled people with software key translation. Also emerging speech-to-text input etc etc etc...


So what''s the solution to perfect text input on ANY keyboard type in any territory etc ? quite simple really - when you need text input, release any exclusive mode you have over the keyboard, put a TranslateMessage() in your message pump and properly translated ASCII (or UNICODE) characters will be sent to you with a WM_CHAR message. When you need to go back to game style input, re-acquire the keyboard and use DInput again. 

--
Simon O''''Connor
Creative Asylum Ltd

    */
        #endregion

        //Private _repeatDelay As Int32 = 300 '300ms = 1/3 of a second
        //Private _repeatRate As Int32 = 30 ' when a key is pressed, it will fire keyPress events at no more than once ever 30ms.  Alternatively, this repeatRate could also be set as the thread update interval which has the same effect by forcing the polling frequency to an upper threshhold.  In fact this is probably how this should be implemented.
        // on the other hand, we dont need to make a thread for this class _IF_ we only thread the rendering instead.  That makes more sense.  
        // so here we'd use _repeatRate as a limit on when we actually "process" the keybuffer
        // WAIT: on the other hand what if you want to have a repeatRate = 0 so that it doesnt repeat.  These should in fact then be seperate
        //WAIT: If we use a different mechanism (WM_CHAR message processing) for typing into a console or chat mode,
        // then why do we care about "repeat" rate and repeat delay?  Those dont really come into play unless
        // i directly support those mechanisms.  Of course the downside to doing that is now we have to worry about
        // multilanguage crap (like how multiple keys are pressed to generate a character in Japanese) 
        // instead of letting windows handle it for us.


        // TODO: eventually i want to try testing against other languages espeiclaly asian
        // so control panel\regional and language options\languages tab --> then click the bottom check box to install Asian languages.
        public enum KEYSTATE
        {
            KS_UP = 0,
            KS_DOWN = 1
        }



        private KEYSTATE _currentKeyState; // used by both

#if TVKEYBOARD
        Private MTV3D65.TVInputEngine _tvKeyboard; 
        Private MTV3D65.TV_KEYDATA[] _keyBuffer = new  TV_KEYDATA[ MINIMUM_BUFFER_SIZE]; 
        Private MTV3D65.TV_KEYDATA _currentKey;
#else

        private Key _currentKey;
        private Char mCurrentWMChar;

#endif
        // frequency crap and repeat delay shit should be apart of the GUIController or EntityController class
        //Private _pollFrequencyThreshold As Int32 = 0  ' 0 default guarantees polling every frame
        private int _lastPollTime;
        private int _lastKeyEventTime;
        private bool _windowsKeyEnabled = true;


        #region ISubject Members
        public override void Notify()
        {
            if (_observers == null || _observers.Count == 0) return;

            foreach (IObserver o in _observers)
            {
                if (o.HandleUpdate(this))
                {
                    // maintain list of pressed keys so we can send KS_UP events for them
                    // if app loses focus and we want to clean up key states properly
                    if (Keyboard.KEYSTATE.KS_DOWN == currentKeyState)
                        mKeysPressed.Add(_currentKey);
                    else
                    {
                        if (mKeysPressed.Contains(_currentKey))
                            mKeysPressed.Remove(_currentKey);
                    }
                    break;
                }
            }
        }

        #endregion

        //constructor
        public Keyboard(IntPtr hwnd, bool windowsKeyEnabled)
        {
            _windowsKeyEnabled = windowsKeyEnabled;
            Handle = hwnd;
            mKeysPressed = new List<Key>(256);

#if TVKEYBOARD 
        _tvKeyboard.Initialize( true ,  false);
#else
            try
            {
                _directInputDevice = new Device(SystemGuid.Keyboard);
                _directInputDevice.SetDataFormat(DeviceDataFormat.Keyboard);
                _directInputDevice.Properties.BufferSize = _bufferSize;
                SetCooperativeLevel();
                Acquire();
            }
            catch (Exception)
            {
                _deviceAcquired = false;
            }
#endif
        }


        public bool WindowsKeyEnabled
        {
            get { return _windowsKeyEnabled; }
            set
            {
#if !TVKEYBOARD
                if (value != _windowsKeyEnabled)
                {
                    if (_deviceAcquired)
                    {
                        _directInputDevice.Unacquire();
                        _deviceAcquired = false;
                    }
                }
#endif
                _windowsKeyEnabled = value;
            }
        }


        public KEYSTATE currentKeyState
        {
            get { return _currentKeyState; }
        }

#if TVKEYBOARD
        public MTV3D65.TV_KEYDATA currentKey
        get{
            return _currentKey;
        }
    }
#else
        public Char CurrentWMChar { get { return mCurrentWMChar; } }
        public Key CurrentKey
        {
            get { return _currentKey; }
        }
#endif

#if !TVKEYBOARD
        private void SetCooperativeLevel()
        {
        	CooperativeLevelFlags f = CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Background;
            if (!_windowsKeyEnabled)
                //f = f | CooperativeLevelFlags.NoWindowsKey //<-- only valid for Exclusive Or Foreground i think?
            
                //_frm.Show();
                _directInputDevice.SetCooperativeLevel(_hWnd, f);
        }

        protected override void Acquire()
        {
            try
            {
                _directInputDevice.Acquire();
                _deviceAcquired = true;
            }
            catch
            {
                _deviceAcquired = false;
            }
        }
#endif
        object mWMCharLock = new object();
        const int MAX_WM_CHAR_COUNT = 256;
        char[] mWMCharBuffer = new char[MAX_WM_CHAR_COUNT];
        int mWMCharCount = 0;
        public Char[] WMCharacters 
        {
            get
            {
                lock (mWMCharLock)
                {
                    if (mWMCharCount == 0) return null;
                    Char[] result = new Char[mWMCharCount];
                    Array.Copy(mWMCharBuffer, result, mWMCharCount);
                    return result;
                }
            }
        }
        public void OnWindowsCharReceived(char c)
        {
            lock (mWMCharLock)
            {
                if (mWMCharCount < MAX_WM_CHAR_COUNT)
                {
                    mWMCharBuffer[mWMCharCount++] = c;
                }
            }
        }

        public override void Clear()
        {
            _directInputDevice.GetBufferedData(); // dont need to save it

            lock (mWMCharLock)
            {
                for (int i = 0; i < MAX_WM_CHAR_COUNT; i++)
                    mWMCharBuffer[i] = (char)0;

                mWMCharCount = 0;
            }
        }


        
        private List<Key> mKeysPressed;
        public override void ForceKeyReleases()
        {
            if (mKeysPressed.Count > 0)
            {
                for (int i = 0; i < mKeysPressed.Count; i++)
                {
                    _currentKey = mKeysPressed[i];
                    _currentKeyState = KEYSTATE.KS_UP;
                    Notify();
                }
                mKeysPressed.Clear();
            }
        }

        // NOTE: This function is for getting real time status of keyboard state
        // and should NOT be used for character input.  This is especially true
        // for non english keyboards where characters require more than one keystroke
        // to enter.  For normal keyboard text input, just process WM_CHAR.
        // Although its not as fast, it doesnt really have to be since when you are entering
        // text, its not performance critical point in the game.
        // TODO: i beleive the above is incorrect since here we arent checking the
        // state per frame, we are checking buffered data which includes any keys
        // pressed since the last check.
        public override void Update()
        {
#if TVKEYBOARD 
        int keyCount 
        _tvKeyboard.GetKeyBuffer(_keyBuffer, keyCount);
        for (int i = 0; i < keyCount; i++)
            _currentKey = _keyBuffer[i];
            _keyBuffer(i).Key.ToString();
            if ( _currentKey.Pressed <> 0 )
                        _currentKeyState = KEYSTATE.KS_DOWN;
            else
                _currentKeyState = KEYSTATE.KS_UP;
            
            notify();
        }
#else
            if (_deviceAcquired == false) Acquire();
            try
            {
                _lastPollTime = Environment.TickCount;
                _buffer = _directInputDevice.GetBufferedData();

                if (_buffer != null)
                {
                    _lastKeyEventTime = Environment.TickCount;
                    for (int i = 0; i < _buffer.Count; i++)
                    {
                        var keyInfo = _buffer[i];
                        //BIT8 = (keyInfo.Data & 0x1) 
                        //BIT7 = (keyInfo.Data & 0x2) 
                        //BIT6 = (keyInfo.Data & 0x4) 
                        //BIT5 = (keyInfo.Data & 0x8) 
                        //BIT4 = (keyInfo.Data & 0x10)
                        //BIT3 = (keyInfo.Data & 0x20) 
                        //BIT2 = (keyInfo.Data & 0x40)
                        //BIT1 = (keyInfo.Data & 0x80) 
                        _currentKey = (Key) (keyInfo.Offset);
                        if ((keyInfo.Data & 0x80) != 0)
                            _currentKeyState = KEYSTATE.KS_DOWN;
                        else
                            _currentKeyState = KEYSTATE.KS_UP;

                        _timeStamp = keyInfo.TimeStamp;
                        Notify();
                    }
                    //TODO: so our job here should be to only generate notifications when there is a CHANGE in a key's up/down state and thats it.
                    // BUT how do you track any particular key's multiple up/down cycling?  Since these are state changes wrt to the previous
                    // key but not wrt to the state kept since this function was last called. 
                    // The easy solution is to update the currentstate for that key everytime and then call notify one key at a time.
                    // Then when we are looping in here, we compare next keyevent to its "currentstate" and can tell its changed.

                    // so it'd loop call notify and in our notify we'd call every observers.update(me)
                    // in the observers.update(me)  we'd 
                    // call keyboard.getKeyEvent which would return the keycode and whether its down or up one key at a time.

                    // Umm.. by definetion isnt the BufferedDataCollection ONLY going to be filled with CHANGES in any key's state?
                    //I mean, its not gonna generate an event if a key was already down adn is still down right?  


                    // What about combo keys?  Do first person shooters have like ALT+ or SHIFT+ or CTL+ keys?  i dont think they do.
                    // If you did want to handle combo keys that used alt,shift or ctl, then first you need to realize that
                    // the ALT or SHIFT or CTL keys must be pressed FIRST.
                    // So you can always check the keyDOWN events and determine if a combo key is also down
                    // and then maybe set a combo = true flag so the observer can get that.

                    // Thankfully with a decent frame rate, not many events will be generated per frame... maybe only 1 event per 10 frames 
                    // if running at 60fps.
                }
                lock (mWMCharLock)
                {
                    if (mWMCharCount > 0)
                    {
                        for (int i = 0; i < mWMCharCount; i++)
                        {
                            mCurrentWMChar = mWMCharBuffer[i]; //
                            // problem is, we don't want to Notify() here if the controller
                            // is not wanting WM_CHAR but just dx keys and vice versa.  Otherwise
                            // we are essentially notifying twice as much as we need.
                            // it would be nice if we could have a seperate notify for observers
                            // that want these
                            Notify();
                        }
                        mWMCharCount = 0; // clear
                    }
                }
            }
            catch
            {
                Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Module.Name +
                                System.Reflection.MethodBase.GetCurrentMethod().Name);
                _deviceAcquired = false;
            }

#endif
        }
    }
}