using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Keystone.Devices;
using Keystone.Entities;
using Keystone.Enums;
using Keystone.Events;
using Keystone.Interfaces;
using Keystone.Collision;
using Keystone.Cameras;
using Keystone.Types;

namespace Keystone.Controllers
{
    // Client IOControllers are very much like the Strategy design pattern and for our purposes they do two things:
    // 1) They accept and respond to keyboard and mouse input by binding intrinsic functions
    //     to the various key/mouse input buttons and movements.  
    // 2) Provide an interface for a specific type of game client to implement functions that are specific to the game
    //    and thus can effectively enable/disable features such as Importing items, mouse picking, etc by overriding the controller
    //    and re-implemnting methods
    //     a) you can have a controller that supports the editing of deckplans and low level modeling
    //         and another type that is only good for actual game play related commands
    // http://docs.unity3d.com/Documentation/Components/class-InputManager.html
    public class InputController : InputControllerBase
    {

        // bind should be a native interpreted command understood by our virtual machine to allow it to change binds at runtime?
        protected const string KW_BIND = "bind";
        private const string KW_ECHO = "echo";
        private const string KW_ALIAS = "alias";

        
       public InputController(string bindsFilePath) : base()
            // note: mouse and keyboard are subjects being observed via IObserver and are set during Update() below
        {
            // NOTE: binding the release to the same method for pressed works now
            // because none of the scripts call each other... its strictly single key bound
            // actions.  But when you try to call +attack and -attack directly from script
            // then the _buttonPressed var wont be updated and cant be relied on.
            // generic support functions used by a first person controller

            // custom keybinds are always app.EXE side in a custom IOController
            mConsole.Register (KW_BIND, bindkey);
            mConsole.Register(KW_ECHO, echo);
            mConsole.Register(KW_ALIAS, registerAlias);

           // TODO: Don't we need to Unregister functions when we Dispose
           // so that another InputController can register it's own functions for
           // those binds?
           
            // TODO: All of these should be registered by the Workspace
            // for our Editor.  That puts this stuff in AppMain/KeyEdit
            // and not in Keystone.dll

            // TODO: eventually i might just remove this double +/- and simply hold that
            // if no + is appended then the action is treated as a being and end all in one.
            // if a + is appended to the bind then it's treated as begin until keyup.
            // but - action is never expressed as "bindable"  you cant legally have "bind w -attack" 
            // that makes no sense.  That is why i originally did things that way though.


            
			// TODO: these should be thought of as registerd axis now OR we should register the axis as RegisterAxis()
			//       and we can still also use RegisterFunction where we bind directly to a function rather than 
			//       send the axis change state to ViewpointController
            // TODO: if i replace with dictionary<string, Axes>
            //       then on keypress, i get the axis and set it's state and allow 
            //       our scripts to query them and respond rather than let the event trigger which ends up just setting a hardcoded state var anyway
            // April.12.2017 - I'm not even using these registered functions.  They are all null!
            //                 Since my game is not a FPS or 3rd Person action game, I don't really need them.
            //                 Instead, I will just pass the commands through to the Workspace.CurrentTool so that
            //                 behavior can be modified based on input (eg. when lctrl or rctrl keys are pressed
            //                 navpoint placer switches from XZ plane movement to Y axis movement for navpoint altitude.
            mConsole.Register("delete", null);
            mConsole.Register("+leftclick", null);
            mConsole.Register("-leftclick", null);
            mConsole.Register("+rightclick", null);
            mConsole.Register("-rightclick", null);
            mConsole.Register("+cancel", null);
            mConsole.Register("+use", null);
            mConsole.Register("+mouselook", null);
            mConsole.Register("-mouselook", null);
            mConsole.Register("+mousepan", null);
            mConsole.Register("-mousepan", null);
            mConsole.Register("zoomin", null);
            mConsole.Register("zoomout", null);
            mConsole.Register("+forward", null);
            mConsole.Register("-forward", null);
            mConsole.Register("+backward", null);
            mConsole.Register("-backward", null);
            mConsole.Register("+panleft", null);
            mConsole.Register("-panleft", null);
            mConsole.Register("+panright", null);
            mConsole.Register("-panright", null);
            mConsole.Register("+rotateleft", null);
            mConsole.Register("-rotateleft", null);
            mConsole.Register("+rotateright", null);
            mConsole.Register("-rotateright", null);
            mConsole.Register("+control", null);
            mConsole.Register("+panup", null);
            mConsole.Register("-panup", null);
            mConsole.Register("+pandown", null);
            mConsole.Register("-pandown", null);
            

            _interpreter.scriptLoad(bindsFilePath, false, false);
        }

		
        
       #region IDisposable Members
       protected override void DisposeManagedResources()
       {
           base.DisposeManagedResources();

            // TODO: shouldn't our custom Workspaces do this and
           // not Keystone.dll ?
           mConsole.UnRegisterFunction(KW_BIND);
           mConsole.UnRegisterFunction(KW_ECHO);
           mConsole.UnRegisterFunction(KW_ALIAS);

       }
       #endregion
    }
}