//#define TVMOUSE
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keystone.Enums;
using Keystone.Interfaces;
using Microsoft.DirectX.DirectInput;

namespace Keystone.Devices
{
    
    public abstract class DirectInputDevice : ISubject
    {
        protected List<IObserver> _observers = new List<IObserver>();
        protected Device _directInputDevice;
        protected bool _deviceAcquired = false;

        private const int MINIMUM_BUFFER_SIZE = 256;
        protected int _bufferSize = MINIMUM_BUFFER_SIZE;
        protected int _timeStamp;

        protected BufferedDataCollection _buffer;
        protected IntPtr _hWnd;

        #region ISubject Members
        public void Attach(IObserver observer)
        {
            // TODO: determine if ths is WM_CHAR observer and place in
            // the proper list of observers
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public virtual void Notify()
        {
            foreach (IObserver o in _observers)
            {
                if (o.HandleUpdate(this))
                {
                    
                    break;
                }
            }
        }
        #endregion

        public IntPtr Handle
        {
            get { return _hWnd; }
            set
            {
                if (IntPtr.Zero.Equals(value)) throw new ArgumentNullException();
                _hWnd = value;
            }
        }


        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = Math.Min(value, _bufferSize); }
        }

        /// <summary>
        /// System Time in milliseconds
        /// </summary>
        public int TimeStamp { get { return _timeStamp; } }

        public virtual void Update()
        { }

        protected virtual void Acquire()
        { 
        }

        public virtual void Clear()
        { 
        }

        public virtual void ForceKeyReleases()
        { 
        }

        public void Dispose()
        {
            _directInputDevice.Unacquire();
            if (!(_directInputDevice == null)) _directInputDevice.Dispose();
            _directInputDevice = null;
        }
    }
    //mouse class; handles relative/absolute mouse coordinates and button states
    // TODO: id like to improve this class to use delegates for when the state
    // detects thigns like mouse down, mouse up, mouse click, mouse double click
    // its lame to use public vars all over the place like this
// Also consider how this would work in multithreaded client too. Singleton needed for this class?

    public class Mouse : DirectInputDevice
    {
        

#if TVMOUSE 
    Private _tvMouse As New MTV3D65.TVInputEngine
    Private _currentMouseState As MTV3D65.TV_KEYDATA
    Private _currentButton As Int32
    Private _currentButtons(0 To 3) As Boolean
    Private _previousButtons(0 To 3) As Boolean

#else

        //private Microsoft.DirectX.DirectInput.MouseState _currentMouseState; //current mousestate

        private int _currentButton; // Microsoft.DirectX.DirectInput.Button
#endif


        private Point _positionDelta;
        private Point _screenPosition;
        private bool _hasMouseWheel;
        private int _wheelGranularity = 1;
        private int _currentWheelScrollAmount;
        private bool _currentButtonIsPressed;


        // absolute mouse coords
        private int _originX;
        private int _originY;
        private int _originZ;

        private bool _axisMode;
        private bool _restrictMouseToWindow = true;

        private Rectangle _windowBounds;
        private double _mouseSensitivity = 1.0F;
        private const double MIN_SENSITIVITY = 0.1F;
        private const double MAX_SENSITIVITY = 5.0F;



        //If you’re playing with DirectX.DirectInput and the keyboard make sure you set 
        //SetCooperativeLevelFlags to Non-Exclusive and Background while running from within 
        //Visual Studios otherwise you’ll get Microsoft.DirectX.DirectInput.OtherApplicationHasPriorityException 
        //when you try to acquire control of the input device.  Visual Studios appears to keep exclusive control 
        //even if set to Release Mode.
        public Mouse(IntPtr hwnd, bool axisModeAbsolute, bool restrictMouseToWindow)
        {
            //_frm = frm; // set form first since resctrict mouse window checks that its set first
            this.Handle = hwnd;
            _restrictMouseToWindow = restrictMouseToWindow;
            mMouseButtonsPressed = new List<int>(256);
#if TVMOUSE 
        _tvMouse.Initialize(true, true);

#else
            _directInputDevice = new Device(SystemGuid.Mouse);
            _directInputDevice.SetDataFormat(DeviceDataFormat.Mouse);
            _directInputDevice.Properties.BufferSize = _bufferSize;

            _wheelGranularity = _directInputDevice.Properties.GetGranularity(ParameterHow.ByOffset, 8);
            // granularity of hweel 
            if (_wheelGranularity > 0) _hasMouseWheel = true;

            setCooperativeLevel( CoreClient._CoreClient.Graphics.Fullscreen);
            axisAbsolute = axisModeAbsolute;
            Acquire(); // must aquire BEFORE form is shown?
            
            _screenPosition =
                    new Point(Cursor.Position.X, Cursor.Position.Y);
#endif
        }

        public override void Notify()
        {
            foreach (IObserver o in _observers)
            {
                if (o == null) continue;
                if (o.HandleUpdate(this))
                {
                    // maintain list of pressed buttons so we can send KS_UP events for them
                    // if app loses focus and we want to clean up key states properly
                    if (_currentButton == (int)MOUSE_BUTTONS.LEFT ||
                        _currentButton == (int)MOUSE_BUTTONS.MIDDLE ||
                        _currentButton == (int)MOUSE_BUTTONS.RIGHT ||
                        _currentButton == (int)MOUSE_BUTTONS.BUTTON4 ||
                        _currentButton == (int)MOUSE_BUTTONS.BUTTON5 || 
                        _currentButton == (int)MOUSE_BUTTONS.BUTTON6)
                    {

                        if (_currentButtonIsPressed)
                            mMouseButtonsPressed.Add(_currentButton);
                        else
                        {
                            if (mMouseButtonsPressed.Contains(_currentButton))
                                mMouseButtonsPressed.Remove(_currentButton);
                        }
                    }
                    break;
                }
            }
        }

        public static string GetMouseButtonName(int button, int scroll)
        {
            switch (button)
            {
                case (int)MOUSE_BUTTONS.XAXIS:
                case (int)MOUSE_BUTTONS.YAXIS:
                    return "XY-axis".ToUpper();
                case (int)MOUSE_BUTTONS.LEFT:
                    return "mouse1".ToUpper();
                case (int)MOUSE_BUTTONS.MIDDLE:
                    return "mouse2".ToUpper();
                case (int)MOUSE_BUTTONS.RIGHT:
                    return "mouse3".ToUpper();
                case (int)MOUSE_BUTTONS.BUTTON4:
                    return "mouse4".ToUpper();
                case (int)MOUSE_BUTTONS.BUTTON5:
                    return "mouse5".ToUpper();
                case (int)MOUSE_BUTTONS.BUTTON6:
                    return "mouse6".ToUpper();
                case (int)MOUSE_BUTTONS.WHEEL:
                    if (scroll > 0)
                        return "mwheelup".ToUpper();
                    else
                        return "mwheeldown".ToUpper();
                //Note: our mouse buffered data only gives us the amount of movement in up/down scroll and
                // does not provide seperate enumberations based on direction
                default:
                    break;
            }
            return "";
        }

        public int currentButton
        {
            get { return _currentButton; }
        }

        public bool currentButtonIsPressed
        {
            get { return _currentButtonIsPressed; }
        }

        public int scrollAmount
        {
            get { return _currentWheelScrollAmount; }
        }


        public Point PositionDelta
        {
            get { return _positionDelta; }
        }

        public Point ScreenPosition
        {
            get
            {
                while (!(_deviceAcquired))
                    Acquire();

                // NOTE: trying to grab the seperate x,y from _directinputMouseState.CurrentMouseState.X and Y results in the state being cleared 
                //after the first X is read.  So we read only from the _currentMouseState variable which is ensured to be updated every tick
                //_mousePosition.X = _currentMouseState.X;
                //_mousePosition.Y = _currentMouseState.Y;

                return _screenPosition;
            }
        }

//        private Microsoft.DirectX.DirectInput.MouseState mouseState
//        {
//            get
//            {
//#if TVMOUSE 
//            _tvMouse.GetMouseState(_mousePosition.X, _mousePosition.Y, _currentButtons(0), _currentButtons(1), _currentButtons(2), _currentButtons(3), _currentWheelScroll)
//#else
//                _currentMouseState = _directInputDevice.CurrentMouseState;
//                return _currentMouseState;
//#endif
//            }
//        }

        //results in a change in the setCooperativeLevel to foreground and exclusive 
        // note with foreground, you must provide your own mouse cursor.  
        // TODO: that means we need to pass in an icon or something?
        // and draw that into our scene?
        public bool restrictMouseToWindow
        {
            get { return _restrictMouseToWindow; }
            set
            {
                if (_hWnd == IntPtr.Zero)
                    // cannot restrict to window if no form is specified
                    _restrictMouseToWindow = false;
                else
                    _restrictMouseToWindow = value;
            }
        }

        private bool axisAbsolute
        {
            get { return _axisMode; }
            set
            {
#if TVMOUSE

#else
                // this gives us relative units as opposed to absolute position from the origin
                // in a first person shooter, generally you'd want to use .AxisModeAbsolute = False
                // since all you care about is the amount of movement in each axis.
                _axisMode = value;
                _directInputDevice.Properties.AxisModeAbsolute = _axisMode;
#endif
            }
        }

        public double mouseSensitivity
        {
            get { return _mouseSensitivity; }
            set
            {
                if (value < MIN_SENSITIVITY) value = MIN_SENSITIVITY;
                else if (value < MAX_SENSITIVITY) value = MAX_SENSITIVITY;
                _mouseSensitivity = value;
            }
        }


#if !TVMOUSE


        private void setCooperativeLevel(bool foreground)
        {
            try
            {
                CooperativeLevelFlags f;

                // foreground + exclusive = ok.  background + exclusive = not supported
                // foreground + nonexclusive=ok

                if (foreground)
                    f = CooperativeLevelFlags.Foreground;
                else
                    f = CooperativeLevelFlags.Background;
                               

                if (!foreground)
                    f = f | CooperativeLevelFlags.NonExclusive;
                else if (_restrictMouseToWindow)
                    f = f | CooperativeLevelFlags.Exclusive;
                else
                    f = f | CooperativeLevelFlags.NonExclusive;

                _directInputDevice.SetCooperativeLevel(_hWnd, f);
            }
            catch
            {
            }
        }

        protected override void Acquire()
        {
            try
            {
                setCooperativeLevel(CoreClient._CoreClient.Graphics.Fullscreen);
                _directInputDevice.Acquire();

                //TODO: i could change this depending on our .AxisModeAbsolute setting

                // but in say a RTS or a game editor where you have a moving mouse cursor (or say at a menu screen in a 
                // first person shootr) i could be easier to define the initial mouse coordinates
                // as 0,0 by using offsets
                _originX = _directInputDevice.CurrentMouseState.X;
                _originY = _directInputDevice.CurrentMouseState.Y;
                _originZ = _directInputDevice.CurrentMouseState.Z;
                _deviceAcquired = true;
            }
            catch (AcquiredException ex)
            {
                _deviceAcquired = true;
            }
            catch
            {
                _deviceAcquired = false;
            }
        }
#endif

        public override void Clear()
        {
            _directInputDevice.GetBufferedData(); // dont need to save it
        }

        private List<int> mMouseButtonsPressed;
        public override void ForceKeyReleases()
        {
            if (mMouseButtonsPressed.Count > 0)
            {
                for (int i = 0; i < mMouseButtonsPressed.Count; i++)
                {
                    _currentButton  = mMouseButtonsPressed[i];
                    _currentButtonIsPressed = false;
                    Notify();
                }
                mMouseButtonsPressed.Clear();
            }
        }

        //retrieves the mouse state.  Primarily this is for button clicks since a change in mouse button state results in an notify
        // to observers
        public override void Update()
        {
#if TVMOUSE 
    try{
            _tvMouse.GetMouseState(_mousePosition.X, _mousePosition.Y, _currentButtons(0), _currentButtons(1), _currentButtons(2))
            for (int i =0; i < 4; i++){
                if ( _currentButtons(i) <> _previousButtons(i)); // TODO: perhaps the problem is it winds up being a reference copy! Not a value copy
                    _currentButton = i;
                    _currentButtonState = _currentButtons(i); //  true = down, false = up
                    notify();

                    _previousButtons(i) = _currentButtons(i);
                }
            }
        }
        Catch Exception ex{
            Debug.Print("deviceMouse:tick() -- " + ex.Message);
        }
#else
            if (_deviceAcquired)
            {
                // screen position used mostly for picking, otherwise we use the _positionDelta 
                Point lastScreenPos = _screenPosition;
                _screenPosition =
                    new Point(Cursor.Position.X, Cursor.Position.Y);
                try
                {
                    BufferedDataCollection bufferedData = _directInputDevice.GetBufferedData();

                    // reset accumulation variables
                    _currentButton = 0;
                    _currentWheelScrollAmount = 0;
                    _positionDelta.X = 0;
                    _positionDelta.Y = 0;


                    if (!(bufferedData == null))
                    {
                        for (int i = 0; i < bufferedData.Count; i++)
                        {
                            var mouseInfo = bufferedData[i];
                            //Dim button As Microsoft.DirectX.DirectInput.Mouse = CType(mouseInfo.Offset, Microsoft.DirectX.DirectInput.Mouse)

                            //For axis input, if the device is in relative axis mode, the relative axis motion is reported. 
                            //If the device is in absolute axis mode, the absolute axis coordinate is reported.
                            //--- TODO: Does this mean that axis input and button info are always seperate?  I think it is.
                            //For button input, only the low byte of DwData is significant. 
                            // The high bit of the low byte is set if the button was pressed; it is clear if the button was released.
                            // I believe AND &H80 is checking that high bit of the low byte
                            //Select Case button
                            //    Case Mouse.Button0, Mouse.Button1, Mouse.Button2, Mouse.Button3, Mouse.Button4, Mouse.Button5, _
                            //     Mouse.Button6, Mouse.Button7

                            //        If CBool(mouseInfo.Data And 0x80) Then
                            //            _currentButtonIsPressed = True
                            //        Else
                            //            _currentButtonIsPressed = False
                            //        End If
                            //    Case Mouse.Wheel
                            //        Debug.Print("Wheel rolling")
                            //    Case Mouse.YAxisAB
                            //        Debug.Print("Top to Bottom movement")
                            //    Case Mouse.XAxisAB

                            //        Debug.Print("Left to Right movement")
                            //End Select
                            switch (mouseInfo.Offset)
                            {
                                    // TODO: these we should store in a seperate buffer so that in the end we notify
                                    // and we've got them all already and it doesnt interefere with accumulating scrollwheel and x/y axis movement
                                case (int) MOUSE_BUTTONS.LEFT:
                                case (int) MOUSE_BUTTONS.MIDDLE:
                                case (int) MOUSE_BUTTONS.RIGHT:
                                    {
                                        //BIT8 = (keyInfo.Data & 0x1) 
                                        //BIT7 = (keyInfo.Data & 0x2) 
                                        //BIT6 = (keyInfo.Data & 0x4) 
                                        //BIT5 = (keyInfo.Data & 0x8) 
                                        //BIT4 = (keyInfo.Data & 0x10)
                                        //BIT3 = (keyInfo.Data & 0x20) 
                                        //BIT2 = (keyInfo.Data & 0x40)
                                        //BIT1 = (keyInfo.Data & 0x80) 
                                        if ((mouseInfo.Data & 0x80) != 0)
                                            _currentButtonIsPressed = true;
                                        else
                                            _currentButtonIsPressed = false;

                                        _currentButton = mouseInfo.Offset;
                                        _timeStamp = mouseInfo.TimeStamp;
                                        try {
                                        	Notify();
                                        }
                                        catch (Exception ex)
                                        {
                                        	 	System.Diagnostics.Debug.WriteLine ("Mouse.Update() - ERROR: " + ex.Message);
                                        }
                                        break;
                                    }
                                case (int) MOUSE_BUTTONS.XAXIS:
                                case (int) MOUSE_BUTTONS.YAXIS:
                                    {
                                        // value gets larger as you go down and to the right
                                        // which i guess makes sense if you consider that 0,0 is top/left in windows.
                                        // in Absolute mode this number accumulates from forever it seems.  The numbers get extremely large.
                                        // so we'd have to initialize and subtract the initial value to get relative.  No point, just set
                                        // axis mode to NOT absolute
                                        if (mouseInfo.Offset == 0) //x axis
                                            _positionDelta.X += mouseInfo.Data;
                                        else
                                            _positionDelta.Y += mouseInfo.Data;
                                        break;
                                    }
                                case (int) MOUSE_BUTTONS.WHEEL:
                                    {
                                        // accumulate all the values between frames before notifying
                                        _currentWheelScrollAmount += (mouseInfo.Data/_wheelGranularity);
                                        break;
                                    }
                                default:
                                    {
                                        //Debug.Print("mouse offset = " + mouseInfo.Offset);
                                        //// String.Format("0x{0:X}", mouseInfo.Offset));
                                        //Debug.Print("button pressed data = " + mouseInfo.ButtonPressedData);
                                        ////String.Format("0x{0:X}", )); 
                                        //Debug.Print("Data = " + mouseInfo.Data);
                                        break;
                                    }
                            }
                            // if we have any accumulated x,y axis scroll info, set current to MOUSEMOVE and notify
                            //TODO: if we want to support absolute position wrt an origin, does it work buffered?  do we want to care?
                            //If (_mousePosition.X <> 0) OrElse (_mousePosition.Y <> 0) Then
                            //    _currentButton = -1
                            //    notify()
                            //End If
                            // if we have any accumulated mouse scroll info, set current = 8 and notify
                            //  NOTE: we do not notify scrolling until all scrolling that  have been performed since hte last frame are accumulated
                        }
                        if (_currentWheelScrollAmount != 0)
                        {
                            _currentButton = (int) MOUSE_BUTTONS.WHEEL;
                            Notify();
                        }
                        // accumulate and notify seperately for wheel and for x/y axxis movement
                        if ((_positionDelta.X != 0) || (_positionDelta.Y != 0))
                        {
                            _currentButton = (int) MOUSE_BUTTONS.XAXIS;
                            // TODO: can we confirm that _positionDelta = lastScreenPos - currentScreenPos?  doesnt seem to work
//                            int x = lastScreenPos.X - _screenPosition.X;
//                            int y = lastScreenPos.Y - _screenPosition.Y;
//                            Point deltaTest = new Point (x,y);
//                            System.Diagnostics.Debug.Assert (deltaTest.Equals (_positionDelta));
                            Notify();
                        }
                    }
                }
                catch (Exception ex)
                {
                	System.Diagnostics.Debug.WriteLine ("Mouse.Update() - " + ex.Message);
                    _deviceAcquired = false;
                }
#endif


                //    // relative units the mouse has moved since the last time
                //    // "mickeys" are the smallest measurable movements of a given device. (uh mickey mouse? Thats from the DX documentaiton. I didnt make that up!)
                //    // _mouseSensitivity is used to scale up or down the amount of movement depending on user preference
                //    MovementX = _currentMouseState.X * _mouseSensitivity;
                //    MovementY = _currentMouseState.Y * _mouseSensitivity;
                //    MovementZ = _currentMouseState.Z;

                //    mX = (int)(mX + MovementX);
                //    mY = (int)(mY + MovementY);

                //    if ((MovementX != 0 ) || (MovementY != 0) {
                //        _bMouseMoved = true;
                //        System.Diagnostics.Trace.WriteLine("Mouse moved");
                //    }else
                //        _bMouseMoved = false;


                //    // with AxisModeAbsolute = True we would map the 
                //    if (mX < 0 ) mX = 0;
                //    if (mY < 0 ) mY = 0;

                //    // restrict position info to screen boundaries if in exclusive mode
                //    if ( _bRestrictMouseToWindow ) {
                //        if (mX > _frm.Width) mX = _frm.Width;
                //        if ( mY > _frm.Height ) mY = _frm.Height;
                //    }
                // }
                //catch{
                //    //TODO: i can replace this with reflection code to get proper module name and function name.  This way
                //    // its always corrrect regardless of how we re-organize our code and change modules/functions names
                //    errhandler("mouse.vb", "GetMouseState")
                // }
#if TVMOUSE 
            }

#else
                if (!_deviceAcquired) Acquire();
            }
#endif
        }
    }
}