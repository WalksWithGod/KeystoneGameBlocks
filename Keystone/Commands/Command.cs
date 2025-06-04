using System;
using Amib.Threading;
using KeyCommon.Messages;

namespace Keystone.Commands
{

    public enum State
    {
        Ready,
        ExecuteError,

        ExecuteProcessing,

        ExecuteCompleted,

        //UnExecuteError,
        //UnExecuteProcessing,
        //UnExecuteCompleted
    }


    public class Command
    {
        protected MessageBase mMessage;
        protected MessageBase mUndo;

        protected WorkItemCallback _workerFunction;
        protected PostExecuteWorkItemCallback _completionCB;
        protected ICommandProgress _progress;
        protected State _state = State.Ready;
        public object WorkerProduct;  // when worker finishes executing it can store any product here
                                      // and then in the Completion() method that WorkerProduct can be 
                                      // handled further if necessary.
    

         /// <summary>
         /// commandID is required for proper deserialization over the network.  
         /// Since this is an abstract class, commandID is always passed by the derived class.
         /// </summary>
         /// <param name="commandID"></param>
         public  Command(MessageBase message)
         {
             mMessage = message;
             //Name = mMessage.Name;
             _state = State.Ready;
         }

         public bool CanUndo { get { return mMessage.CanUndo; } }

         public MessageBase Message { get { return mMessage; } }
         public MessageBase Undo { get { return mUndo; } }


         #region ICommand Members
         public State State
         {
             get { return _state; }
             set { _state = value; }
         }

         public WorkItemCallback Worker
         {
             get { return _workerFunction ; }
         }

         public PostExecuteWorkItemCallback Completion
         {
             get { return _completionCB; }
         }

         public ICommandProgress Progress
         {
             get { return _progress; }
         }


         public void BeginExecute(WorkItemCallback workerFunction, PostExecuteWorkItemCallback completionHandler)
         {
             BeginExecute(workerFunction, completionHandler, null);
         }

         public void BeginExecute(WorkItemCallback workerFunction, PostExecuteWorkItemCallback completionHandler, ICommandProgress progress)
         {
             _completionCB = completionHandler;
             _workerFunction = workerFunction;
             _progress = progress;

             if (_state == State.Ready)
             {
                 if (_progress != null) _progress.Begin();

                 WorkItemInfo item = new WorkItemInfo();
                // item.WorkItemPriority = WorkItemPriority.Highest;
                 item.PostExecuteWorkItemCallback = new PostExecuteWorkItemCallback(ExecuteCompletedHandler);
                 Core._Core.CommandProcessor.QueueWorkItem(item, _workerFunction, this);

             }
             else
             {
                 _state = State.ExecuteError;
                 System.Diagnostics.Trace.WriteLine(string.Format("CommandBase.BeginExecute() -- state '{0}' not ready.  Cannot queue command.{1}", _state.ToString(), this.GetType().Name));
                 // TODO: need to create a new WorkItemResult so we can invoke the callback immediately
                 //          since otherwise there is no way the CommandProcessor will ever be able to find out this command failed.
                 //_completionCB.Invoke(new work
                 // TODO: need a way to cancel execution for things like canceling generation of connectivity in Interior
                

             }
         }


         public virtual void EndExecute()
         {
             _state = State.ExecuteCompleted;
             if (_progress != null) _progress.End();
         }

         protected void ExecuteCompletedHandler(IWorkItemResult result)
         {
             if (_completionCB != null)
                 _completionCB.Invoke(result);
         }
         #endregion

     }
}
