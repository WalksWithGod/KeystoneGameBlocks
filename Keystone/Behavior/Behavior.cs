using System;
using KeyCommon.Traversal;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Traversers;
using Keystone.Resource;

namespace Keystone.Behavior
{
	// TODO: Links here
    #region Notes and LInks
    // Brainiac visual behavior tree designer
	//   http://brainiac.codeplex.com/
    
    // Goal Oriented Action Planning 
	// http://martindevans.me/heist-game/2013/05/22/Trees-Are-Well-Behaved/
	// - seems to me that GOAP is mostly a matter of using a tree to create a plan from a goal
	//   and then storing that plan and staying to it until the plan is invalidated or completed.
	//   - so it seems not really different from BT but rather uses the BT in a way as to create plans
	//     and then to cache that plan as state 
	//	- then when traversing when formulating a new plan, we can traverse the tree until we get to
	//    the plan we are executing. So in many ways GOAP is part of how we cache and use node state
	//    to resume a part of a branch rather than recomputing from the start. 
	//    - so i dont think GOAP is new.  its just an implementation of a BT.
	//
	// 
	// 
	// http://www.altdevblogaday.com/2011/02/24/introduction-to-behavior-trees/
	
	// http://freesdk.crydev.net/display/SDKDOC5/Create+a+simple+new+AI+Behavior+from+scratch
    // NOTE: the talk about deferred actions reminds me very much of our
    //       Renderer use of buckets to batch items that are similar where 
    //       we cull and defer and then batch render later
    //       Indeed, if we can run the tree using an AgentState block of memory that
    //       stores the AI state and last run state, then defer the actual actions 
    //       we can be very efficient with making decisions thru behavior trees!
    //       So as we have Context.Cull()  we could have Scene.DecideBehaviors() and then
    //       Scene.ExecuteBehaviors()

    // NOTE: the below article "data oriented streams spring behavior trees" also reminds me of the
    //       token type structure used in our keybind system.  Indeed, a behavior tree looks very much like
    //       a function call graph and of course it can be flattened perhaps into a structure that is 
    //       very memory efficient as it creates a sort of byte code that we then just interpret.
    //       "To put it bluntly: behavior trees and an actor’s traversal state and action requests are completely
    //       represented in data and the data is stored in arrays. No pointers, just indices are necessary 
    //       to traverse a behavior stream."

    // https://github.com/listentorick/UnityBehaviorLibrary
    //
    // https://skill.codeplex.com/
    //
    // http://www.altdevblogaday.com/2011/04/24/data-oriented-streams-spring-behavior-trees/

    // Output – harvesting behavior trees
    // A naive behavior tree implementation would execute leaf node actions during its
    // traversal and, as actions like pathfinding, environment sensing, or animation generation
    // can be very data intensive, would repeatedly trash caches on the way. Running these 
    // actions has side effects on the game state – but these side effects are hard to grasp as
    // output. Also, as behavior trees are general modeling tools, it isn’t clear what kind of 
    // actions will be used by the behavior tree creator – and what kind of data will be accessed.
    //
    // Deferred actions and action request streams
    // Therefore, actions shouldn’t be called on visiting a leaf node of the behavior tree but
    // deferred to run later in associated systems that can organize their data for optimal 
    // (parallel) processing, e.g. via batching. Deferring means to collect action commands or 
    // action requests without needing to handle the data actions operate on immediately – one
    // source of cache misses defeated.
    // 
    // These action request collections – typically streams aka arrays of commands – can be 
    // analyzed and sorted to batch them to their respective systems later on. If we know which 
    // resources, e.g. parts of an animation skeleton, are affected by actions, we can also benefit
    // from the sorting by detecting possible clashes where different actions want to manipulate 
    // the same resource. Awareness is the first step to appropriately treat such collisions.
    //
    // Deferring has downsides – selection of an action to run and action execution are separated in 
    // time and in code. This complicates debugging. And – it adds lag. During traversal of an
    // action emitting node the final action result state isn’t immediately known. Instead of the 
    // final behavior result state, node traversal returns the running state – an action has been 
    // requested – it can be understood to already run without an end-result yet. The action might 
    // finish before the next tree traversal process or later. On finishing, the result state, e.g.
    // success or fail, etc., needs to be made accessible to the action node, so the traversal 
    // process can react to it during the next traversal.

    // http://www.altdevblogaday.com/2011/07/09/data-oriented-behavior-tree-overview/
    // - "By analyzing the static shape of a behavior tree it should be possible to determine the 
    // max number of action and decider states active at once. The necessary storage buffers are 
    // therefore pre-allocated."

    // http://angryant.com/behave/documentation/

    // http://stackoverflow.com/questions/9330928/create-c-sharp-parser-ast-tree-cil-and-run-it-in-virtual-machine
    // http://www.codeproject.com/Articles/43176/How-to-create-your-own-virtual-machine
    // http://www.codeproject.com/Articles/61924/How-to-create-your-own-virtual-machine-Part-2

    // https://github.com/btodoroff/Encephal16
    
    // data oriented behavior trees
    // http://www.smu.edu/~/media/Site/guildhall/Documents/Theses/Delmer_Stephan_Thesis_Final.ashx
            
    // http://forums.tigsource.com/index.php?topic=34856.msg921122#msg921122  <-- good description of implementation for Games of Edan
    // 
    // 
    // http://www.gamasutra.com/blogs/MatthewKlingensmith/20130907/199787/Overview_of_Motion_Planning.php
    //
    // UNDERSTANDING BEHAVIOR TREES
    // http://aigamedev.com/open/articles/bt-overview/
    //_______________________________

    // ANTI-OBJECTS
    //_______________________________
    // A MUST RE_READ ARTCILE ON ANTI_OBJECTS AND AI  BEFORE WE REALLY START CODING OUR AI <--------------------
    // http://en.wikipedia.org/wiki/Antiobjects  <-----------------------------------
    //    - and what about anti-objects for computing sensors, targeting and such?
    //      - i could divide each Solar System which is 74,799,000,000,000 (74.799 trillion meters!!!) into
    //      ugh... even if 1 million meters cube, that'd be like 75 megabytes x 3 per system.
    //      But as far as sighting and detection, this would naturally take care of occlusion, and such... except
    //      where planets and things orbiting need to move through cells would be annoying.
    //      We would need a fast system for determine which cell something was in without tracking references.... cuz that 
    //      owuld be a nightmare.
    //
    //   Although at the macro-AI level for strategic decision making, using each StellarSystem's region itself as our anti-object
    // strategic layer would be great.
    //
    //      -  also anti-objects for computing line of sight, communications messages and intercepting them.
    //  - for fighter squadrons i did actually once have the idea of having them fly in a moving grid centered around
    //    a particular "leader" 
    //  -  What about the use of this to determine what things are lit?  Diffusion of actual light?
    //  - How to parallelize?  Multi-threaded Error Diffusion by Shameem Akhter and Jason Roberts  @ 11:01 EDT | May 24, 2010 
    //      http://www.drdobbs.com/go-parallel/article/showArticle.jhtml;jsessionid=CFAJVCT30Q3WPQE1GHRSKH4ATMY32JVN?articleID=225000075&pgno=2


    // article on why navigation meshes beat waypoints
    // http://www.ai-blog.net/archives/000152.html

    // Handling Complexity in the Halo 2 AI
    // http://www.gamasutra.com/gdc2005/features/20050311/isla_01.shtml

    // Crysis
    // http://aarmstrong.org/notes/paris-2009/coordinating-agents-with-behaviour-trees

    // ending the HFSM tyrranny
    // http://www.ai-blog.net/archives/000072.html

    // Using Resource Allocators to Synchronize Behaviors
    // http://aigamedev.com/open/articles/allocator/

    // http://aigamedev.com/open/articles/event-handling-purposeful-behaviors/

    // https://aigamedev.com/open/articles/popular-behavior-tree-design/

    // AI Planning course pdfs
    // http://www.cs.umd.edu/~nau/cmsc722/

    // Recast and Detour open source navigation mesh and pathfinding library in c++
    // http://code.google.com/p/recastnavigation/

    // http://aigamedev.com/insider/presentations/behavior-trees/

    // the main page that links behavior trees with an overarching AI strategy
    // http://aigamedev.com/architecture/hierarchical-logic-multi-threading/
    //http://www.behaviorengineering.org/index.php?option=com_content&task=view&id=23&Itemid=33

    // the more interesting behavior tree specific vids
    //AI Behavior trees might be an interesting way away from HFSM
    //http://aigamedev.com/videos/behavior-trees-part1/
    //http://aigamedev.com/videos/behavior-trees-part2/
    // http://aigamedev.com/videos/behavior-trees-part3/   <-- this third part when it talks about squad AI problems, its great
    // as well as the part just before that when it talks about behaviors naturally being able to work together automatically
    // and that's great because it actually seems to fit the way the brain works better than a FSM.

    // parallel nodes for concurrency
    // https://aigamedev.com/open/articles/parallel/

    // What would a Behavior Tree editor look like?
    // http://aigamedev.com/open/articles/behavior-tree-editor-example/ 

    //        MIGHT BE NICE TO IMPLEMENT BEHAVIOR TREES AFTER I GET MY SHIP FLOOR PLAN BUILT AND GET SOME SIMPLE NPC'S IN THERE RIGHT AWAY.  
    //<Hypnotron> ill have to spend some time thinking about behavior trees at some point... there's definetly some attractive qualities
    //<Hypnotron> and having worked with scenegraphs before its easier to understand than fsm's (for me at least)
    //<Hypnotron> and as the speaker in the vid said, its easy to get up and started with some simple trees and just incrementally develop your AI on an as needed basis
    //<Hypnotron> i think im sold on it

    // brainac open src behavior tree editor in c#  !!!
    // http://www.codeplex.com/brainiac

    // seems Unity has a Behavior tree ditor called Behave
    // http://eej.dk/angryant/wp-content/themes/angryant/images/BehaveCompiler.png
    // and  aTUTORIAL for how to create a behavior in Unity
    // http://www.arges-systems.com/articles/2/behavior-trees-in-unity-with-behave

    //Seems to me the concept of a behavior tree is basically a lot like a Scene Graph and scenegraph traversal.  
    //Two key concepts stuck with me.
    //  - 1 - I think its important that we implement our fleets/armies of NPC AI as actually being apart of a single organism
    //    such that individual NPCs are just sort of the actionable parts of a bigger organism so that we just need one behavior tree.
    //  - 2 - I'm confused in that it seems behavior trees like a scene cull traversal, could require a lot of traversal time.  But I guess its supposed to go really fast.  
    //<Hypnotron> behavior trees are interesting... it seems 
    //analgous to a scene graph
    //<Hypnotron> in a traditional scene graph you traverse and collect state information as you go
    //<Hypnotron> and taht results in what you render
    //<Hypnotron> or in this case it results in various npc actions
    //<Hypnotron> so in terms of understanding why a "behavior tree" is not a HFSM in disguise, i think
    //the difference is in an HFSM you usually reach a dead end and that's the part 

    //that contains all the actions you perform
    //<Hypnotron> and in a behavior tree, just like in a scenegraph, when you reach a leaf node you dont 
    //stop, you then back up and go on to the next sibling

    // a good comment from the ai gamedev.com site
    // Regarding the difference of subsumption architectures and BTs: as far as I can remember 
    // subsumption architectures are layered. The lowest level reacts to direct events and 
    // executes direct actions. Higher levels get reports from lower levels (what is going on 
    // but no requests to do something as lower levels don't really know about higher levels,
    // just to send data somewhere up the layer chain) and ask the lower level to execute specific
    // actions based on the decision process of the higher level. For example the lower level might 
    // be able to do one step while a higher level tries to move to a specific location which needs 
    //lots of steps.
    //The lower level also can ignore higher level requests/commands, for example to prevent a collision.
    //The higher level is mainly concerned with high level decision making and not with the nitty 
    //gritty details of action execution done by the lower levels.

    //The lowest level is updated most often while higher levels are updated with lower frequencies.

    //A variation of this is an Orwellian State Machine (from Igor Borovikov), which replaces the layers
    //by a tree structure similar to BTs but the data flowing between the tree nodes are messages
    //and not status tokens like in the BT Alex develops (as far as I know).

    //A BT is quite different from a subsumption architecture. First there aren't direct layers but a
    // tree. Information flowing in the tree is:
    //1) traversal and therefore execution and evaluation of nodes and
    //2) status reports by the nodes which flow from the nodes up to their parents.

    //A BT is traversed (or might be traversed this way - there can for sure be variations of this idea) 
    // until a node returns "running". If the BT is updated the next time it isn't traversed from
    // the root until the last "running" node is found but the last "running" node is called 
    // directly. Therefore, the last state of the BT traversal is the next state to start the 
    // evaluation again.
    //This idea is complicated by having parallel nodes - so a tree evaluation needs a mechanism to 
    // store all "running" or active nodes of the last tree traversal and to work on them in 
    // the order the tree traversal dictates to react to conditions that might cancel "running" 
    // nodes that might be traversed later on.
    //As far as I understand Alex blog (I haven't looked into the Game::AI++ code but am about to do it) 
    // he implements this mechanism (and fights call-stack depth with it, too) by modelling each
    // node as a task that gets inserted into a task scheduler. If you traverse a BT each nodes 
    // task is added to a scheduler, the scheduler runs the task. A task might finish as 
    // "completed" and then might enqueue its parents node task into the scheduler which then 
    // is executed by the scheduler.

    //For example an action below a sequence is "completed" and enqueues the selector task right behind
    // it into the scheduler queue. The scheduler then runs the selector tasks which enqeues 
    // the next action in the sequence it controls, and so on.

    //"running" tasks/nodes remain in the scheduler (while completed nodes/tasks are removed) and are
    // executed in the next frame when calling the BT evaluation. These "active" or "running"
    // tasks/nodes therefore represent which nodes to evaluate the next time the BT is called.

    //Well, this is how far I understand Alex's BTs, I hope that Alex or other people that know how 
    // BTs work chime in and point out the errors and how BTs really work internally.

    //Cheers,
    //Bjoern 


    // Shared behavior trees i think should not be re-instanced for every agent.  Instead, we rely on
    // simply passing in a new "state" object (e.g the Entity reference) which is used for that entity
    // The entity itself can maintain a Behavior.State to track BehaviorResults like BehaviorResult.IsRunning??  
    // the above idea is a total WIP.  
    // UPDATE TO above about sharing trees, I think this is OUT.  AFter reading about Spore's BT
    // at http://chrishecker.com/My_Liner_Notes_for_Spore/Spore_Behavior_Tree_Docs
    // it seems clear that because the active path in the tree is specific to each agent, that the tree itself does
    // hold some "state" information and thus needs to be shared.  Further, there's no good way to cache
    // that currently active path (or is there?) in the agent itself so that the tree can be shared by many agents.
    // Since a tree is a DAG, there's no way to traverse back to the right parents to the root.  If in the future we have a mechanism to
    // dynamically update the BT in real time, then that could screw up any sort of scheme we employ to trace through a cached numeric
    // path we've set in the agent's state.  Ahhh... but wait... if the tree is a DAG then that would actually screw up how active path is
    // saved as well.  I think we really do need a way to know the path and I suppose keep the tree static at runtime... no dynamic updates of the tree.
    // I think actually it's the traverser itself that has to remember the path and that traverser is also the Entityt's state object.
    // A traverser can abort early but stay at it's current state.... or actaully maybe a traverser is _not_ also the State object because
    // we might need multiple traversers per agent.  If the active path is stored in one traverser by simply "freezing" it in it's place
    // then we would use a second traverser next game tick to determine if a higher priority behavior must occur and thus abort the one that
    // is in progress.  If our traversers are lean and mean and light weight, this shouldnt be a problem.  
    // Actually what we're going to do is have a single traverser but maintain the last traverser state via an array of uint that will serve as indices
    // for the child to visit.  so it int[] activePath  = new int[]{0,2,2,1};  
    // then this would indicate root, 2nd child of root, then the 2nd child of that, then the 1st child of that
    // So our objective then should be to 
    // TODO: Should read http://www.altdevblogaday.com/2011/04/24/data-oriented-streams-spring-behavior-trees/
    // http://www.altdevblogaday.com/2011/02/24/introduction-to-behavior-trees/
    // http://www.altdevblogaday.com/2011/03/10/shocker-naive-object-oriented-behavior-tree-isnt-data-oriented/

    /// <summary>
    /// There are two main ways to thread our behavior tree.  If each AI has it's instance of the behavior tree
    /// then when itterating over the AI agents list, we can have multiple behavior trees being evaluated at a time
    /// thus speeding up the itteration.  
    /// This is suitable for games where there are lots of individual agents acting autonomously.  But what about a 
    /// planning AI for managing a theater map in a game like Civ or for coordinating many individual units in a tactical game
    /// like Total War?  Here the Behavior tree for the "planner" AI must be able to decompose it's traversal so that it can
    /// be parallelized for faster decision making resolution.  How can we accomodate both?  
    /// 
    /// If the state used by this planner is self-contained and does not use using blocking procedures on shared variables,
    /// then the search can be parallelized by duplicating the state for every parallel search of a sub-graph.
    /// 
    /// HTN planners are very similar to behavior trees, except that behavior trees do less planning and focus more on 
    /// reactive monitoring and control — also known as reactive planning. What’s useful about behavior trees in the context 
    /// of multi-threading is that they are structured as latent procedures that execute over time, and the whole logic is 
    /// built to deal with any-time behavior.
    /// 
    /// Task Schedulers

    /// The final ingredient in the mix is a task scheduler. This technique makes it easy to implement things like behavior trees 
    /// in an event driven way, such that little time is wasted polling and querying tasks, instead there’s a nice callback mechanism 
    /// that helps parent tasks figure out when the child computation is done. (My article in AI Wisdom 4 shows how this can be done.)
    /// </summary>
    /// 
    /// // http://sandervanrossen.blogspot.com/2009/06/behavior-tree-navigation-meshes.html
    ///     // - behavior script runs and as AI for instance it says
    //   string[] targets = SpatialAPI.GetPrioritizedTargets(args, filter); //args is enemy types, filters, etc
    //   string[] turrets = VehicleAPI.GetWeapons(vehicleID, filter); // filter is weapon types (lasers, missiles)
    //  
    //     AssignTargetsToAvailableTurrets  // any turrets not destroyed or unpwoered are available
    //       Until TargetLock Lost or TargetDestroyed
    //          ConvertTargetPostionToTurret'sLocalSpace
    //          (Matrix targetMatrixInReferenceSpace = EntityAPI.GetEntitySpace (referenceEntity, targetEntity);
    //          RotateToAcquire
    //              EntityAPI.SetTurretTarget (turretID, targetMatrixInReferenceSpace);
    //          FireWhenGoodEnoughAimAchieved
    // 
    
    
            // TODO:  you DO NOT want to store state in the tree.
            //       to me the tree is a machine that you put state INTO IT and can update STATE. It should not hold state
            //       that is not relevant only to management of the tree itself. (eg flag for which child is next to iterate)
            //       - so i think Behavior should most definetly be a shared thing and from an Entity's perspective the root
            //       behavior should be something we can reference and swap in/out.  Any edit of tree always effects 
            //       all other entities that are using it.  So it's important to always re-save trees that you wish to 
            //       change without affecting other entities that use it.
            // http://www.altdevblogaday.com/2011/04/24/data-oriented-streams-spring-behavior-trees/
            // INDEED we see that state really is something we pass in to a tree
            // Traversal state
            // On closer inspection, each behavior tree traversal produces an actor specific traversal state that 
            // affects the next traversal, e.g. which sequence child node to re-visit next because it returned a 
            // running state. The actor traversal state is an output to feed back as input for the next update.        
        //Deferred Actions / Batching - similar to how we cull and generate a render list
        //http://altdevblogaday.org/2011/04/24/data-oriented-streams-spring-behavior-trees/
        //quote from above url "actions shouldn’t be called on visiting a leaf node of the
        //behavior tree but deferred to run later in associated systems that can organize 
        //their data for optimal (parallel) processing, e.g. via batching. Deferring means 
        //to collect action commands or action requests without needing to handle the data 
        //actions operate on immediately – one source of cache misses defeated"
        
        		//
        //      or would we rather http://stackoverflow.com/questions/4241824/creating-an-ai-behavior-tree-in-c-sharp-how
        // implement something like Rafe's solution in the above link where he basically just links functions together
        // or Elliot Wood's here  http://stackoverflow.com/questions/10357305/game-ai-behavior-trees
    #endregion
    /// <summary>
    /// Standard Behavior base class.
    /// 
    /// NOTE: Just like any other non-entity node, Behaviors can be shared
    /// but changing a parameter in one shared cases, changes that behavior in others.
    /// Adding a child in one case adds it to the other shared cases obviously.
    /// Thus when editing behavior trees keep in mind that shared trees may cause
    /// unexpected yet correct behavior.
    /// 
    /// Since you cannot drag and drop behaviors between entities, the only way to
    /// share them easily is by copying the entity or by storing the tree as a .Behavior
    /// xml file in the assets browser and dragging and dropping it as a complete or subtree.
    /// 
    /// When designing a game it is important to share trees as much as possible so try to
    /// create unique behaviors by having different entity data for the .Script action's to 
    /// use.
    /// </summary>
    public abstract class Behavior : Node
    {
    	
        protected Behavior(string id) : base (id)
        {
            Shareable = true;
        }

        
        public abstract BehaviorResult Perform(Entity entity, double elapsedSeconds);

        
    #region ITraverser Members
        // Using ITraverser objects to traverse behavior nodes is mostly for
        // save/load.  For executing behaviors however we just call Behavior.Perform()
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
#endregion

        
        // TODO: what does a behavior node serialize?  Mostly it's just the hierarchy and typenames and script node paths.
		// Do we really want to inherit rather bulky "Node" type?  
		// -> This image shows some ideas on what gets saved (also saved in KGB PICS)
		// http://4.bp.blogspot.com/-9upi-woL75M/TyovHNfHAII/AAAAAAAAAN8/YDb0g_s71CM/s1600/behavior_trees_2.png
		// - is the serialized behaviortree and the actual code the same thing?
        //      - the AIBlackboardData should get saved to the Entity's main script and deserialized during Initialize(entityID) call of that script
		// - serializing\deserializing behaviors that are using delegates?
		//		- the articles ive found re: trees like TreeSharp by Apoc which serializes behavior trees
		//      is that every behavior node type that is created, becomes a unique type.  You can re-use nodes
		//      of course, but Actions\Delegates don't save you from never having to create more than the basic node types.
		//      Instead you are constantly creating derived types.  Data is pointed to using resx ids or direct strings
		//		in the case of dialogue trees.
		//      - so in terms of what this looks like in my scene format, i think it results in typenames that
		//      may be defined exe side and so our Repository.Create() needs a way to see if the EXE can create the derived type.
		//		otherwise, i think there's probably no actual properties to "get" or "set".  The tree becomes completely flat in
		//      this regards.  The tree is data and execution in one.  The only saving grace is that its generated for you...
		//      - to the extent some data is dynamically discovered, its still done through code queries to entity states and data maps\blackboards\resx stores.
		//		- ill have to research this but in short run, im going to avoid it and stick with viewpoints that i generate on the fly
		//      through hardcoded instantiating.
		//      - If this Entire tree is generated to CODE, then we can simply compile the whole thing... like if our Brainiac implementation actually let us
		//      write the code, but the problem is, debugging would be a nightmare.
		//      - but this starts to look like a nightmare to compose behaviors... 
		//   http://stackoverflow.com/questions/4241824/creating-an-ai-behavior-tree-in-c-sharp-how
        #region IResource Members
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="specOnly">True returns the properties without any values assigned</param>
        ///// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
        //    tmp.CopyTo(properties, 2);

        // TODO: perhaps here these can read/write various parameters
            // such as a "wait" parameter that accepts a value in milliseconds
            // or a "vector3d target" parameter for a Move behavior node
            // and a float for speed
        //    properties[0] = new Settings.PropertySpec("entityflags", _entityFlags.GetType());
        //    properties[1] = new Settings.PropertySpec("attachedtoboneid", AttachedToBoneID.GetType());

        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = _entityFlags;
        //        properties[1].DefaultValue = AttachedToBoneID;
        //    }

        //    return properties;
        //}

        //// TODO: this should return any broken rules 
        //public override void SetProperties(Settings.PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "entityflags":
        //                _entityFlags = (EntityFlags)properties[i].DefaultValue;
        //                break;
        //            case "attachedtoboneid":
        //                AttachedToBoneID = (int)properties[i].DefaultValue;
        //                break;
        //        }
        //    }
        //}
        #endregion
    }
}
