using System;

namespace Keystone.Commands
{
    public enum CommandState
    {
        Ready,
        ExecuteError,
        
        ExecuteProcessing,

        ExecuteCompleted,

        //UnExecuteError,
        //UnExecuteProcessing,
        //UnExecuteCompleted
    }
}
