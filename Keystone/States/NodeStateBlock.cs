using System;
using System.Collections.Generic;


namespace Keystone.States
{
    /// <summary>
    /// A NodeStateBlock's primary purpose is to host an array of states associated with a single target node
    /// </summary>
    public class NodeStateBlock
    {
              int mTarget;
              private uint mStateCount;
              private NodeState[] mStates;
          

              public void Add (NodeState state) {}
              public void Apply(object target) {}

              public uint StateCount { get { return mStateCount; } }
              public NodeState[] States { get { return mStates; } }
    }
}
