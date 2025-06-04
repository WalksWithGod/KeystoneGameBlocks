using System;
using Keystone.Interfaces;
using System.Collections.Generic;
using Keystone.Devices;
using System.Diagnostics;
using System.Drawing;
using Keystone.Enums;
using Keystone.Cameras;

namespace Keystone.Controllers
{
    public abstract class InputControllerBase :  IDisposable, IObserver
    {

        protected struct BOUND_KEY
        {
            public Microsoft.DirectX.DirectInput.Key keyCode;
            public byte keyCodeCombo;
            // SHIFT, CTL, ALT in addition to the keycode. NOTE: Depending on context, these may stand alone in addition to combos.
            // also some will have to be defautl SYSTEM keys like ALT+ENTER for fullscreen/windowed toggle (ALT+TAB is supported in windows and we couldnt disable that if we wanted to i dont think. Least not easily)
            public string keyName; // string representation of the key, "a" "A" "SHIFT" "leftarrow" etc
            public string routineName;
        }

        protected Dictionary<string, BOUND_KEY> _bindCollection;
        protected Keyboard _keyboard;
        protected Mouse _mouse;
        // TODO: are these values polled somewhere?
        protected Microsoft.DirectX.DirectInput.Key _currentKey;
        protected bool _buttonPressed;

        protected Interpreter _interpreter;
        protected Keystone.CmdConsole.Console mConsole;

       // protected Workspaces.IWorkspace3D mWorkspace;
        protected Cameras.RenderingContext mSelectedContext;

       


		// http://docs.unity3d.com/Documentation/Components/class-InputManager.html
        public InputControllerBase()
        {
            _bindCollection = new Dictionary<string, BOUND_KEY>();
            
            mConsole = new Keystone.CmdConsole.Console();
            _interpreter = new Interpreter(mConsole, this);
        }

       // public Workspaces.IWorkspace3D Workspace { get { return mWorkspace; } }

        public Keystone.Cameras.RenderingContext SelectedContext { get { return mSelectedContext; } }
        
        #region IObserver Members
        // Keyboard and Mouse both notify this controller via its Update method for each buffered key or
        //mouse button state change since the previous frame.
        // Then a giant select case is used to convert the key or button code into
        // the ascii name used in the bind dictionary.  
        public virtual bool HandleUpdate(ISubject subject)
        {
            // TODO: this input handler has some problems namely that
            // it must be overriden if a control like a textbox has inputCapture and thus
            // text needs to be sent to it rather than "executed" as a bind
            if (subject is Keyboard)
            {
                _keyboard = (Keyboard)subject;
                return HandleKeyboard(_keyboard);
            }
            else if (subject is Mouse)
            {
                _mouse = (Mouse)subject;
                return HandleMouseButtons(_mouse);
            }
            return false;
        }
        #endregion

        protected virtual bool HandleKeyboard(Keyboard keyboard)
        {
            _buttonPressed = (Keyboard.KEYSTATE.KS_DOWN == keyboard.currentKeyState) ? true : false;

            _currentKey = keyboard.CurrentKey;
            string key = _currentKey.ToString();
            //DirectInput returns localized names.  
            //Note that if you, for example, use a German keyboard with a French version of Windows, 
            //DirectInput will give you the French names for the keys.
            //Thus, in order to provide localized controls for our project, we simply
            // need to provide localized configuration scripts.
            if (_buttonPressed) 
            {
                // NavPlacement tool needs to respond to CTRL keys for Altitude changes in waypoints
                if (mSelectedContext != null && mSelectedContext.Workspace.CurrentTool != null)
                    //mWorkspace.CurrentTool.HandleEvent (Events.EventType.KeyboardKeyUp, )
                    mSelectedContext.Workspace.ToolKeyDown(key);
                //mSelectedContext.mViewController.SetAxisStateChange (key, _buttonPressed, null);
                return ExecuteBind(key, true, true);
            }

            // TODO: this sucks.  Mouse must be over viewport for these key events to be
            // passed to the mViewController.
            // How do I resolve this?  
            // Does the current tool need to be assigned to the InputController?
            // NavPlacement tool needs to respond to CTRL keys for Altitude changes in waypoints
               if (mSelectedContext != null && mSelectedContext.Workspace.CurrentTool != null)
                //mWorkspace.CurrentTool.HandleEvent (Events.EventType.KeyboardKeyUp, )
                mSelectedContext.Workspace.ToolKeyUp(key);

            //mSelectedContext.mViewController.SetAxisStateChange(key, _buttonPressed, null);
            return ExecuteBind(key, false, true);
        }

        protected virtual bool HandleMouseButtons(Mouse mouse)
        {
            // find the selected viewport in this workspace
            // _buttonPressed is used to maintain a capture of the current context as the start of the mouse down
            // the problem however is when using keyboard keys for movements for example and then moving the mouse
            // off the viewport, we only track the lost of the mouse button pressed and seem to forget we already
            // have capture from keyboard still.. likewise we may have multiple keyboard keys and multiple mouse
            // buttons so we cannot just respond to the first one to be unpressed and then attempt
            // a re-select of context which may result in null with mouse having left the viewport area.
            // what if we incremented buttonCount? problem there is it could be messy trying to keep a proper count
            if (_buttonPressed == false || mSelectedContext == null)
            {
                Viewport selectedViewport =
                    Viewport.GetMouseOverViewport(mouse.ScreenPosition.X, mouse.ScreenPosition.Y);

                if (selectedViewport == null || selectedViewport.Context == null ||
                    selectedViewport.Context.Camera == null)
                {
                    mSelectedContext = null;
                    return false;
                }
                
                // only change current context if a previous button does not have capture
                mSelectedContext = selectedViewport.Context;
            }

            mSelectedContext.ViewpointController.MouseScreenPosition = mouse.ScreenPosition;
            string mousebutton = Mouse.GetMouseButtonName(mouse.currentButton, mouse.scrollAmount);

            if (mousebutton == "XY-AXIS") // mouse movement 
            {
            	//return HandleMouseMove(mouse.PositionDelta);
            	// TODO: or what if value is mouse?
            	try {
                    // TODO: we're effectively subverting the use of the Binded functions... 
                    // For version 1.0, maybe that's ok.  We kinda screwed up but don't want to fix it
                    // at this point.  Still, for Keyboard keys, we don't have a mSelectedContext that is
                    // guaranteed to be not null because keys can be pressed even when mouse is not over viewport.
            		mSelectedContext.mViewController.SetAxisStateChange ("mousemove", true, mouse.PositionDelta); 
            	}
            	catch (Exception ex )
            	{
            		System.Diagnostics.Debug.WriteLine ("InputControllerBase.HandleMouseButton() " + ex.Message );
            	}
            	return true;
            }
			        

            _buttonPressed = mouse.currentButtonIsPressed;            
            if (mouse.currentButton == (int)MOUSE_BUTTONS.WHEEL)
            {    // x movement, y movement and wheel are never pressed
                //return ExecuteBind(mousebutton, _buttonPressed, false);
                // TODO: how come we cant execute the bind with a value?
                // Why can't the selected tool for a given active workspace
                // have ExecuteBind() work for it too?
                if (_bindCollection.ContainsKey (mousebutton))
                {
                	string axisName = _bindCollection[mousebutton].routineName;
                	mSelectedContext.mViewController.SetAxisStateChange (axisName, true, mouse.scrollAmount);
                }
                return true;
            }
            
            return ExecuteBind(mousebutton, _buttonPressed, true);
        }

        
        internal void HandleAxisStateChange (string axisName, object args)
        {
        	if (mSelectedContext != null)
        	{	// convert the +/- in the axisName to a bool
        		bool pressed = true;
        		if (IsPlusMinusRoutineName (axisName))
        		{
                    // todo: mousepan is useful during interior view, but during exterior view, we want it to be serve as a movement destination point for our ship.
                    //       How do we handle that particularly when both viewports are open at the same time?
        			pressed = axisName.Substring(0, 1) == "+";
        			// TODO: what if i filtered out the reserved axis names (eg. Bind, Use, Select, and others that were not Viewpoint related
        			//       and distributed them to the appropriate handlers here rather than in ViewpointController? The ViewpointController itself
        			//       is nearly unnecessary at this point.
        			mSelectedContext.ViewpointController.SetAxisStateChange (axisName.Substring(1), pressed, args);
        		}
        		else	
        			mSelectedContext.ViewpointController.SetAxisStateChange (axisName, pressed, args);
        	}
        }
        
        /// <summary>
        /// Takes keyboard/mouse event and finds a corresponding keybind to determine
        /// the axis it's associated with. Axis in this context means a friendly input
        /// state such as "IsFiring" or "Selecting" or "Zooming" or "Panning".
        /// TODO: normally inside _interpreter.executeScript it ends in
        /// mConsole.Execute(sTokens[0], args)  which invokes a function delegate
        /// that was assigned during creation of the InputController but instead
        /// we just need to invoke a delegate in the InputController which will
		/// pass the axis name and state to our ViewpointController.HandleAxisChangeState(string axisName, object value)        
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="bIsPressed"></param>
        /// <param name="isButton"></param>
        /// <returns></returns>
        protected bool ExecuteBind(string sKey, bool bIsPressed, bool isButton)
        {
            sKey = sKey.ToUpper();
            if (_bindCollection.ContainsKey(sKey))
            {
                //Debug.Print("firstPersonController.ExecuteBind() -- Key = " + sKey + "  Bound to '" +
                //            _bindCollection[sKey].routineName + "'.");
                // TODO: next determine if its bound to a +/- action
                // if so, then based on keystate, determine which one to call
                string axisName = _bindCollection[sKey].routineName;
                
                // If this is a button and this button responds to both PRESSED and RELEASED states
                if ((isButton) && (IsPlusMinusRoutineName(axisName)))
                {
                    if (bIsPressed)
                        _interpreter.executeScript(axisName);
                    else // button was released so add the "-" to the name so the script knows
                        _interpreter.executeScript("-" + axisName.Substring(1));
                }
                else
                    _interpreter.executeScript(axisName);

                return true;
            }
            else
            {
                Trace.WriteLine ("'" + sKey + "' is not bound.");
                return false;
            }
        }
        
        internal bool HasBind (string name)
        {
        	return _bindCollection.ContainsKey (name);
        }

        // Keys that respond to BOTH pressed and released events are bound
        // by AXIS names that begin with +.  
        // Keys that respond only to a pressed state have no + in front of them.
        // 
        // the + stands for a key pressed and so the action beginning
        // the - stands for the key released and the action ending.
        // The idea here is that pressing and holding attack (+attack)
        // can result in multiple rapid fire dpeneding on the type of weapon in hand.
        // But for mousewheel, there really is no begin/end scroll in either direction.
        // its just mwheelup and mwheeldown.  
        protected bool IsPlusMinusRoutineName(string sName)
        {
            int i = sName.IndexOf("+");
            if (i == 0) return true;
            
            int j = sName.IndexOf("-");
            return j == 0;  // if either are 0, it indicates a plus or a minus is the first character and will return true
        }

        // =======================================================================
        // Delegates for generic support functions 
        // PURPOSE: 
        // =======================================================================
        
        protected bool IsValidBind(string keyname, string routineName)
        {
            if (IsPlusMinusRoutineName(routineName))
                if (routineName.IndexOf("-") >= 0) return false;  // a "-" prefix is not valid for the "bind" command.

            return (IsValidKey(keyname));
            
        }

        protected bool IsValidKey(string name)
        {
            return true; // TODO: implement this
        }
        /// <summary>
        /// The syntax for the bind command in our control configuration scripts is "bind {0} {1}" 
        /// where {0} is the name of a key or mouse button or wheel and 
        /// where {1} is the intrinsic or aliased routine name. 
        /// RoutineName arg is always "name" or "+name" and NEVER "-name".
        /// The inclusion of the "+" prefix means that this routine should be treated as a repeatable action
        /// for as long as the key/button is pressed, and on release 
        /// A "-" prefix is not allowed because obviously a "+" prefix implies that a single key's press and release action
        /// is bound to a single repeatable action to connote it's begin and end states.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected void bindkey(object[] args)
        {
            // TODO: I don't necessarily need these generic array of args for every function
            // do i?
            Trace.Assert(args.Length == 3, "Invalid arguments passed to bindkey()");
            Trace.Assert(IsValidBind(((string) args[0]).ToUpper(), ((string) args[1])));

            BOUND_KEY boundKey = new BOUND_KEY
            {
                routineName = ((string)args[1]),
                keyName = ((string)args[0]).ToUpper()
            };

            _bindCollection.Add(boundKey.keyName, boundKey);
        }

        // "ALIAS" is a command that effectively says "register the following user function (aka: scriptlet) 
        // These are added to the _scripts collection in the interpreter
        // An alias command has only 2 arguments. First is the name of the scriptlet.  Second is a string field that can contain a semicolon
        // delimited list of other commands which can be any mix of user commands and intrinsic.
        // NOTE: ALIASing using the name of an existing scriptlet replaces the existing one with the current one
        protected void registerAlias(object[] args)
        {
            Trace.Assert(args.Length == 3);
            Interpreter.SCRIPTLET scriptlet;
            Interpreter.DEBUG_SYMBOL symbol;

            // NOTE: I should make it have 3 arguments with the 3rd being a SYMBOL which contains the line number and line position.
            //       This could work elegantly when executing/validating scripts too since the line number can be passed via the stack
            //       and line position can also be deduced.
            try
            {
                symbol = (Interpreter.DEBUG_SYMBOL) args[2];
                scriptlet.symbol = symbol;
                scriptlet.script = (string) args[1];

                _interpreter.registerAlias((string) args[0], scriptlet);

            }
            catch
            {
                Debug.Print("controllerFirstPerson:registerAlias() -- Error registering user alias.");
            }
        }

        // Echo's are executed in real time.  They are not added to the script
        protected void echo(object[] args)
        {
            Trace.Assert(args.Length == 2);

        }

        protected void dev(object[] args)
        {
            // 0 = disable developer debug info in the console
            // 1 = enable developer debug info in the console

        }

        protected bool _disposed = false;
        
        #region IDisposable Members
        ~InputControllerBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(this.GetType ().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}