using System;
using System.Collections.Generic;
using System.Threading;
using Amib.Threading;
using KeyCommon.Commands;

namespace Keystone.Commands
{
    // 1)
    // This CommandProcessor can handle Commands that are Asychronous or Sychronous.  Both types are Enqueued the same way.
    // Even though one command that is loading something may take long, no other command will execute until it is finished.
    // Thus command execution is always done sequentially... or at least the results are applied to the scene sequentially.
    // When we have the ability to execute more than one thread via Parallel.For (ideally) we will be able to do more to
    // try and precompute commands in preparation of applying them all quickly.
    // However, the key point is simply that commands are executed chronoligically even if some are processed asychronously.
    //
    //
    // This is why an asynchronous job should never be allowed to modify the scene... because it will not know
    // if it is a parallel job that has completed or whether it is the first. 
    // perhaps a better solution is to have asychronous commands complete and then inernal commands which are not added to the redo/undo
    // stacks handle the rest this way there is still a 1:1 mapping of user commands to results on screen.
    // when the command is added to the redo/undo stacks while it is still asychronously working, attempt to undo can cancel the task first
    // if possible and then handle undo... the command thus must e able to maintain a reference to its IWorkItem.  I dunno yet, but
    // we can start and change as required.

    // 2)???
    // The other key point to understand with respect to this CommandProcessor is that commands are enqueued so that 
    // all outstanding results can be applied to the SceneGraph at once in order to minimize locking of the SceneGraph
    // The problem is however that some commands such as moving a character occur in our Player classes and produces a Result
    // rather than a ICommand.  So when we edit a propertygrid, there seem to be two aspects... there's the 1) TryChange
    // which will verify the legality of the change, and the 2) IResult ApplyChange
    // Or perhaps this is not the right way to view this?  Perhaps the ICommand will process and will either succeed or fail.
    // Hrm...  a result would then have to occur _after_ the scene is locked and the attempt to carry out the command and apply it to 
    // scene worked or not.  In this regards, ICommands are basically dumb commands that dont have to actually compute anything
    // such as Player.Update()  .  This is why my original thought was to enqueue commands so that these NON cpu intensive tasks can
    // lock the scene and apply all the commands at once, and then unlock the scene and then raise the results for each.
    // But this is complicated by things such as Collision where a single command may result in other commands and a cyclical collisionResult
    // process to find the final destination of something.  
    //
    // Note that when network says a new player or npc has spawned, that player/npc is added instantly and then the paging
    // of the actual data can be done in the pager and sothe commandprocessor is never blocked.  CommandProcessor should be for
    // immediate commands really.  TODO: i think that's one thing i havent done right yet in my Import commands... the screen locks
    // while waiting for the geometry/tetxures and stuff to load and that should be offloaded to the pager.
    //
    // Input Thread + SimulationUpdate + Short Job Processing Thread + Scene Locking and applying job results to the scene
    // Long Job Processing Thread (Paging / Loading)
    // Render Thread
    public class CommandProcessor
    {
        private static int _lastID = int.MinValue;
        private const int REQUIRED_CONCURRENCY_OF_ONE = 1;
        internal static IWorkItemsGroup ThreadedWorkQueue;

        public delegate void CommandExecutionProgress(out int written, out int remaining);
        public delegate void AsychronousJobCompleted(Command command);

        private SmartThreadPool _threadpool;

        private List< IWorkItemResult> _outstandingWork;
        private object _outstandingWorkLock;


        public CommandProcessor(SmartThreadPool threadpool)
        {
            if (threadpool == null) throw new ArgumentNullException();
            _threadpool = threadpool;

            // we're going to configure this queue such that it will run on a single thread and thus process new commands seqentially
            _outstandingWork = new List<IWorkItemResult>();
            _outstandingWorkLock = new object();
            ThreadedWorkQueue = _threadpool.CreateWorkItemsGroup( REQUIRED_CONCURRENCY_OF_ONE); // for now we require serialized command processing in the threadpool
            ThreadedWorkQueue.Name = "CommandProcessorWorkQueue";
            ThreadedWorkQueue.OnIdle += new WorkItemsGroupIdleHandler(CommandProcessorThreadGroupIdle);
        }

        public void QueueWorkItem(WorkItemInfo itemInfo, WorkItemCallback workerFunction, Command command)
        {
            IWorkItemResult result = ThreadedWorkQueue.QueueWorkItem(itemInfo, workerFunction, command);
            lock (_outstandingWorkLock)
            {
                _outstandingWork.Add(result);
            }

        }

        /// <summary>
        /// This cancels _all_ workItems that are queued.  I would need to modify the ThreadQueue to cancel only specific workitems that are either in progress or enqueued.
        /// </summary>
        public void CancelWorkItem()
        {
            System.Diagnostics.Debug.Assert(_outstandingWork.Count <= 1, "Ideally we should only have one queued item at a time, but there's no guarantee.  This assert is just a sanity check.  It will probably not be true all the time.");
            ThreadedWorkQueue.Cancel();
        }

        /// <summary>
        /// Must be called by CommandCompleted handler to remove the outstanding item
        /// </summary>
        /// <param name="result"></param>
        public void RemoveWorkItemResult(IWorkItemResult result)
        {
            lock (_outstandingWorkLock)
            {
                _outstandingWork.Remove(result);
            }
        }

        public void UpdateProgress()
        {
            lock (_outstandingWorkLock)
            {
                foreach (IWorkItemResult result in _outstandingWork)
                {
                    //ICommand2 command = (ICommand2)result.State;
                    //if (command.Progress != null)
                    //    command.Progress.Update();
                }
            }
        }

        public static int NextID
        {
            get
            {
                // TODO: this is probably bad.  maybe better to put all this in a locked section
                // TODO: im not sure this works well in network but the idea is that a SequenceID for each
                // command will allow clients to apply commands that finish asychronously out of order, in order.
                Interlocked.Increment(ref _lastID);
                Interlocked.CompareExchange(ref _lastID, int.MinValue, int.MaxValue);
                return _lastID;
            }
        }


        private void CommandProcessorThreadGroupIdle(IWorkItemsGroup group)
        {
           // System.Diagnostics.Trace.WriteLine("Theadpool Group '" + group.Name + "' idle.");
        }
    }
}