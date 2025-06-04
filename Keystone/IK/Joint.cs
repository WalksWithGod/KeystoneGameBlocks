using System;
using Keystone.Elements;
using Keystone.Types;


namespace Keystone.IK
{
    // joints are really just constraints applied to a link.
    // 
    public abstract class Joint : Node
    {
        protected string mName;
        protected Vector3d mPosition;

        public Joint(string id) : base(id) { }

        public void SetName(string name)
        {
            mName = name;
        }

        #region ITraverser Members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            throw new NotImplementedException();
        }
        #endregion

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
    }
}
