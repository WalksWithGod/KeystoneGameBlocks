using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Keystone.Scene;
using Keystone.Simulation;

namespace Keystone.Controllers
{
    // used for controlling the System via user input.
    // Console commands, ESC, F1-F12 are some of the types
    // this controller will respond too.
    // NOTE: 
    public class SystemController : InputController
    {
        private Core _core;


        public SystemController(Core core, string bindsFilePath) : base(bindsFilePath )
        {
            if (core == null) throw new ArgumentNullException();
            _core = core;

            // registering system commands is easy.  Simply
            // pass the command and the function name that will handle the command
            // or you can load it from a script file
            mConsole.Register (KW_BIND, bindkey);
            mConsole.Register("shutdown", Shutdown);
            mConsole.Register("fullscreen", Fullscreen);
        }


        public void RunCommand(string command)
        {
            try
            {
                _interpreter.Interpret(command, -1);
            }
            catch
            {
                Trace.WriteLine("SystemController.RunCommand() - ERROR: Invalid command '" + command + "'.");
            }
        }

        #region Commands

        public void Fullscreen(object[] args)
        {
            if (args.Length != 1) // actually has two arguments if there's debug symbol attached
            {
                Trace.WriteLine("SystemController.Fullscreen() - Invalid arguments.");
                return;
            }
            CoreClient._CoreClient.Graphics.Fullscreen = (bool)args[0];
        }

        public void Shutdown(object[] args)
        {
            
        }

        #endregion
    }
}