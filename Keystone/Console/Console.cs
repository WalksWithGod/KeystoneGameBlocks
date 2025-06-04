using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.CmdConsole
{
    public class Console
    {
        // TODO: this is unnecessary it seems.   i can have interpreter call "AxisChanged"
        // that doesnt execute any function directly... instead relies on behavior scripts
        // to respond to state
        private Dictionary<string, Action<object[]>> _functionTable;
        
		//private Dictionary <string, 
        public Console()
        {
            _functionTable = new Dictionary<string, Action<object[]>>();
        }

        public Dictionary<string, Action<object[]>> Functions
        {
            get {return _functionTable; }
        }

        internal bool AxisIsRegistered(string name)
        {
        	return _functionTable.ContainsKey (name);
        }
        
        public void Register (string name, Action<object[]> function)
        {
            if (_functionTable.ContainsKey(name))
                _functionTable.Remove(name);

            _functionTable.Add(name, function);
        }
        
        public void UnRegisterFunction(string name)
        {
            if (_functionTable.ContainsKey(name))
                _functionTable.Remove(name);
        }
        
        

        public void Execute (string name)
        {
            if (_functionTable.ContainsKey(name))
            	if (_functionTable[name] != null) // if the function this item is pointing to is not set, dont try to invoke it
                	_functionTable[name](null);
        }

        public void Execute(string name, object[] args)
        {
        	// until we move the "BIND" command to somewhere, we still need to execute functions from functiontable
            if (_functionTable.ContainsKey(name))
                if (_functionTable[name] != null) // if the function this item is pointing to is not set, dont try to invoke it
                	_functionTable[name](args);
            
            // here all we want to do is not execute anything but
            // set axis states
            
        }

    }
}
