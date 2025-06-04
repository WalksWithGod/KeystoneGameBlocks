using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Resource;
using Keystone.Traversers;


namespace Keystone.Behavior.Composites
{
    /// <summary>
    /// Behavior Tree composite node type.
    /// </summary>
    public abstract class Composite : Behavior , IGroup 
    {
        protected List<Node> _children;

        public Composite(string id) : base (id)
        {
            
        }

        #region IGroup Members
        public  void AddChild(Behavior child)
        {
            if (_children == null) _children = new List<Node>();

            // same node cannot be added twice
            if (_children.Contains(child))
                throw new ArgumentException("Keystone.Behavior.Composites.AddChild() - Node with id ' " + child.ID + "' already exists.");
            
            child.AddParent(this);
            _children.Add(child);
        }

        public virtual void RemoveChildren()
        {
            while (_children != null)
                RemoveChild(_children[0]);
        }

        public virtual void RemoveChild(Node child)
        {
            try
            {
                // NOTE: There is no recursive call of child.RemoveChildren()  INSTEAD
                // we call child.RemoveParent(this)  and only from there would it call .RemoveChildren() on it'self 
                // if it had no more parents and was truely detached from the scene.  This is why it works!
                // see Node.cs.RemoveParent for more details.
                Repository.DecrementRef(child);// decrement first because 
                _children.Remove(child);
                child.RemoveParent(this);
                if (_children.Count == 0) _children = null;
            }
            catch
            {
            }
        }

        public Node[] Children
        {
            get
            {
                if (_children == null) return null;
                return _children.ToArray();
            }
        }

        public virtual int ChildCount
        {
            get
            {
                if (_children == null) return 0;
                return _children.Count;
            }
        }

        public virtual void MoveChildOrder(string childID, bool down)
        {
            Group.MoveChildOrder(_children, childID, down);
        }

        public void GetChildIDs(string[] filteredTypes, out string[] childIDs, out string[] childNodeTypes)
        {
            Group.GetChildIDs(_children, filteredTypes, out childIDs, out childNodeTypes);
        }

        /// <summary>
        /// Finds the first descendant that matches.
        /// </summary>
        public Node FindDescendant(Predicate<Keystone.Elements.Node> match)
        {
            return Group.FindDescendant(_children.ToArray(), match);
        }

        public Node FindNodeAtDescendantIndex(int index)
        {
            int startCount = 0;
            return Group.GetNodeAtDescendantIndex(this, index, ref startCount);
        }

        public Node FindDescendantOfType(string typename, bool recurse)
        {
            return Group.FindDescendantOfType(this, typename, recurse);
        }

        public Node[] FindDescendantsOfType(string typename, bool recurse)
        {
            return Group.FindDescendantsOfType(this, typename, recurse);
        }
        #endregion
    }
}
