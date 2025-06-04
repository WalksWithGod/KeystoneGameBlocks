using System;


namespace KeyCommon.Traversal
{
    // April.14.2019 - obsolete.  We now use Entity.CustomData for AIBlackboard data
    //                 and if Entity.CustomData == null, then we know it hasnt been initialized in the main Entity.Script.
    public class BehaviorContext
    {
        public BehaviorContext(string entityID)
        {
            EntityID = entityID;
        }

        public bool Initialized;
        public string EntityID { get; }

    }
}


//// AKA: BehaviorKnowledge
////
////IBehaviorKnowledge
////  Stimulus\Events
////  OtherEntityKnowledge (persistant knowlwedge about detected enemies, allies, etc)
////      - last seen, last known position
////  ActionHistoryKnoweldge
////      - last melee action, last action time (needed to determine cooldowns)
////     
////  LastTreeTraversalStateKnowledge
////
//// in the Halo article on gamasutra, they also try to cache info that speeds up
//// the relevancy check on nodes by tagging some parts of the tree as disabled
//// for instance if an entity is driving a vehicle, they wont be able to
//// consider some parts of the tree like "run to" until they exit the vehicle
//// thus rather than have to constantly test those nodes just to find out
//// it can't run because it's in a car, it can flag those nodes immediately 
//// as disabled.  This seems complicated though and requires that the designer
//// of the behavior graph add some form of "trigger" action that will disable
//// certain parts of the tree via the IBehaviorKnowledge memory blocks
////
////
//// IStimuli // an interface entites use to track the stimulii acting on it
//// so the scripts can query to help decide which path in the tree to take
//// 
//// IBehaviorMemory // an interface entities use so that behavior tree nodes can
////                 // share data with otehr nodes during traversal. Storing this in the Entity
////                 // makes it available to the behaviortree.
////                 // the alternative is to store these in the traverser object
////                 // however that's not good since the scripts dont have a way
////                 // to reference that traversal object... unless perhaps
////                 // IBehaviorMemory is set to reference the one used by the traverser
////
////         - i think for BehaviorMemory we should use the entity.CustomData AI Blackboard and not a CustomProperty ("userdata", data); defined in script
////
////
////  I've always been a bit confused on how active behaviors work... it seems
////  behavior tree descriptions ive read say that unlike a state machine where you traverse
////  it top to bottom each time is not done in a behavior tree, that active behaviors
////  can be resumed next traversal and im not sure how that's done exactly but 
////  I think it does make sense that if say you are currently running a script that is
////  "move to waypoint target" that next frame you'd want to re-perform that behavior until
////  it was completed.... unless an event/stimulus causes us to re-evaluate a new path
////  such as "taking fire.." or "waypoint target destroyed..."
////  Not sure how that works exactly... 
////
////  Seems yes that what we need to store as part of our IBehaviorKnowledge
////  is our lastBehavior, and the last tree path 
////  and i think an interesting way to store this is with stack of indices[]
////  that correspond to the children indices of the last path.
////  
//public class BehaviorContext
//{
//    public bool Initialized;
//    private Behavior mRoot;
//    // TODO: we have access to the Entity so we have access to "userdata" AI blackboard.  We can make the blackboard object intrinsic rather than a custom property perhaps?
//    public Entities.Entity Entity { get; set; }

//    // OBSOLETE - now using Entity.CustomData
//    //public KeyCommon.Data.UserData Knowledge; // TODO: replace with Entity.CustomData

//    // supposed to be used to maintain the state of each child node on a per entity basis, but i dont like this... imgoing to not use it for now til i understand more
//    // I like that the state is tracked to an extent, but i dont like how it's done with recursive storage of state nodes.   It should be flat
//    // However it is correct that the node state is contextual to the entity it's running on.  However, i think i will ignore Tree state tracking for now since i dont need it
//    //private BehaviorNodeState mBehaviorState; 
//    //mLastRunBehavior; // <-- obsolete - replaced with BehaviorNodeState ?


//    internal BehaviorContext(Entities.Entity entity, Behavior root)
//    {
//        //mBehaviorState = new BehaviorNodeState();
//        mRoot = root;
//        Entity = entity;
//        System.Diagnostics.Debug.Assert(Entity.Behavior == root);

//        // OBSOLETE - now using Entity.CustomData
//        //Knowledge = new UserData ();

//        // for now we wont bother trying to store node state
//        // Initialize(mBehaviorState, mRoot);
//    }


//    // recursive <-- TODO: i think this is fubar.  For starters, we aren't even calling Initialize() in the ctor.  It's commented out.  
//    //           but we are adding these "childStates"
//    //           which seem very unlike a behavior tree... behavior trees are more like DAG screengraphs
//    //           where as FSMs are like flow charts and never start back at root each tick
//    //           unless their flow happens to take them there.
//    internal void Initialize(BehaviorNodeState nodeState, Behavior node)
//    {
//        if (nodeState == null || node == null) throw new ArgumentNullException();
//        BehaviorNodeState childState;

//        if (node is Composites.Composite)
//            if (((Composites.Composite)node).Children != null)
//                for (int i = 0; i < ((Composites.Composite)node).Children.Length; i++)
//                {
//                    childState = new BehaviorNodeState();
//                    nodeState.AddChild(childState);
//                    Initialize(childState, (Behavior)((Composites.Composite)node).Children[i]);
//                }
//            else
//            {
//                childState = new BehaviorNodeState();
//                nodeState.AddChild(childState);
//            }
//    }
//}