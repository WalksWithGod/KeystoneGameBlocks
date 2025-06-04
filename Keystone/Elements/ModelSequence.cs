using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Traversers;


namespace Keystone.Elements
{
    /// <summary>
    /// The Select() for a sequence returns all child models or combined results 
    /// of child.Select() of all children.
    /// </summary>
    /// <remarks>
    /// ModelSequence like it's derived ModelSelector cannot be shared because
    /// they themselves contain state inform (being derived from Transformable) 
    /// and they also host Model's which also contain per-instance state.
    /// </remarks>
    public class ModelSequence : ModelSelector 
    {
        public ModelSequence(string id)
            : base(id)
        {
            Shareable = false; // nodes derived from Transformable can not be shared
            this.mStyle = SelectorNodeStyle.Sequence;
        }

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

    }
}
