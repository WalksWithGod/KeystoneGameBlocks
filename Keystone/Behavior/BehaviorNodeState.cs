using System;
using KeyCommon.Traversal;

namespace Keystone.Behavior
{
    internal enum BehaviorFlags : byte
    {
        None = 0,
        Enabled = 1 << 0,
        Activated = 1 << 1,  // indicates the node is part of the selected path last iteration through the behavior tree
        All = Byte.MaxValue
    }

    // http://altdevblogaday.org/2011/04/24/data-oriented-streams-spring-behavior-trees/
    // note how in the above article it talks about creating a flat array of tokens
    // rather than any nested structure as we use here.
    // Hrm...
    internal class BehaviorNodeState
    {
        // TODO: remove codeplex BehaviorTree from keystone solution... i dont use it afterall.
        private BehaviorResult mLastResult; 
        private BehaviorFlags mFlags;
        private BehaviorNodeState[] mChildren; // TODO: i forget, but why do we have child node states?  isn't this just more a traversal state?
        // private Parameters[] mParameters;   TODO: parameters must be part of the state since they can't be persisted by the Behavior node
                                               // because those nodes are designed to be free of state info so that they can be shared

        // indicates whether the current node was already activated last tick and thus
        // can be used to determine if OnEnter needs to be called and whether
        // if de-activated in the current turn, OnExit needs to be invoked
        public bool IsActivated
        {
            get { return ((mFlags & BehaviorFlags.Activated) == BehaviorFlags.Activated); }
            set { mFlags |= BehaviorFlags.Activated; }
        }

        public bool Enabled
        {
            get { return ((mFlags & BehaviorFlags.Enabled) == BehaviorFlags.Enabled); }
            set { mFlags |= BehaviorFlags.Enabled; }
        }

        public BehaviorResult LastResult
        {
            get { return mLastResult; }
        }

        public BehaviorNodeState[] Children
        {
            get { return mChildren; }
        }

        public void AddChild(BehaviorNodeState child)
        {
            int length = 0;
            if (mChildren != null)
                length = mChildren.Length;

            BehaviorNodeState[] tmp = new BehaviorNodeState[length + 1];
            
            if (length > 0)
                mChildren.CopyTo (tmp, 0);

            tmp[length] = child;
            mChildren = tmp;
        }

        public void RemoveChild()
        {
 
        }
    }
}
