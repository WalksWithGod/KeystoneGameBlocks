using System;
using KeyCommon.Traversal;

namespace Keystone.Behavior.Actions
{
    // TODO: How do i get these derived classes into the BehaviorTree plugin editor GUI?  If they were scripted, i could simply list the files available in a scripts\behaviors\ folder
    /// <summary>
    /// Compute a path from current Entity translation, to a random translation within the bounds of the ship interior.
    /// </summary>
    public class Action_RandomPath : Action
    {
        public Action_RandomPath(string id)
            : base(id)
        {

        }

        public Action_RandomPath(string id, Func<Entities.Entity, double, BehaviorResult> action) : this (id)
        {
            if (action == null) throw new Exception("Action.ctor() - Action is null.");
            mAction = action;
        }

        public override BehaviorResult Perform(Entities.Entity entity, double elapsedSeconds)
        {
            if (entity == null)
                return BehaviorResult.Error_Script_Invalid_Arguments;

            if (Validate(entity, elapsedSeconds) == BehaviorResult.Fail)
                return BehaviorResult.Fail;

            if (mAction == null) throw new Exception("Action.Perform() - Action is null.");

            return mAction(entity, elapsedSeconds);
        }
    }
}
