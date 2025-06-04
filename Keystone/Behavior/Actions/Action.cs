using System;
using KeyCommon.Traversal;

namespace Keystone.Behavior.Actions
{
    // only action's have Execute()
    // only action's have Enter\Exit actions that can be called 
    // - mostly useful for scripts since enter/exit actions for hardcoded
    // derived types of Actions would be kind of lame
    // In short the purpose is largely for scripts or at least for strategy pattern
    // of plugging in anonymous methods or some such
    public class Action  : Behavior 
    {
    	protected Func <Keystone.Entities.Entity, double, BehaviorResult> mAction;
    	
        public Action(string id)
            : base(id)
        {
            Shareable = true;
        }

        public Action (string id, Func <Keystone.Entities.Entity, double, BehaviorResult> action) : this (id)
        {
        	if (action == null) throw new Exception ("Action.ctor() - Action anonymous function is null.");
        	mAction = action;
        }
        
        // TODO: why isn't Perform and Execute the same thing?
        public override BehaviorResult Perform(Entities.Entity entity, double elapsedSeconds)
        {
            if (entity == null)
                return BehaviorResult.Error_Script_Invalid_Arguments;

            if (Validate (entity, elapsedSeconds) == BehaviorResult.Fail)
            	return BehaviorResult.Fail;
            
            if (mAction == null) throw new Exception ("Action.Perform() - Action is null.");
            
            return mAction (entity, elapsedSeconds);
        }


        /// <summary>
        /// Runs an evaluation method, delegate or script to determine if this Behavior
        /// should be run or skipped so that potential siblings can be selected instead.
        /// Validate() is analogous to Decide() or Preconditions() in other BehaviorTree implimentations.
        /// </summary>
        /// <param name="EntityID"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <returns></returns>
        protected virtual BehaviorResult Validate(Entities.Entity entity, double elapsedSeconds)
        {
            return BehaviorResult.Success; 
        }

        protected virtual BehaviorResult Execute(Entities.Entity entity, double elapsedSeconds)
        {
            return BehaviorResult.Success;
        }

        protected virtual BehaviorResult Enter(Entities.Entity entity, double elapsedSeconds)
        {
            return BehaviorResult.Success; 
        }

        protected virtual BehaviorResult Exit(Entities.Entity entity, double elapsedSeconds)
        {
            return BehaviorResult.Success;
        }
    }
}
