using System;
using System.Collections.Generic;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Elements
{
    public abstract class Group : Node, IGroup
    {
        protected List<Node> _children; // 

        public Group(string id)
            : base(id)
        {
        }

        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        #region IGroup Members
        public virtual void MoveChildOrder(string childID, bool down)
        {
            Group.MoveChildOrder(_children, childID, down);
        }

        internal void AddChild(Node child)
        {
            if (_children == null) _children = new List<Node>();

            // same node cannot be added twice 
            // NOTE: This can usually occur in loopback if we allow client to create and add children
            //       that were already created server side.  In loopback I think we should not
            //       AddChild() with supersetter.  we dont have to.  We only need to keep the nodes
            //       in the cache which already occurs upon creation in factory.
            if (_children.Contains(child))
                throw new ArgumentException("Node of type ' " + child.TypeName + "' already exists.");
            
            // ChangeSource.Child will ensure this notification is sent upwards
            // NOTE: always set flags early before any .AddParent/.Add so that if those other methods
            // result in calls to update a bounding box for example, that we then don't return here only
            // to set those flags as dirty again after they've just been updated
            SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeAdded, Keystone.Enums.ChangeSource.Child);
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
                Repository.DecrementRef(child);// decrement first
                _children.Remove(child);
                child.RemoveParent(this);
                if (_children.Count == 0) _children = null;

                // ChangeSource.Child will ensure this notification is sent upwards
                SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeRemoved, Keystone.Enums.ChangeSource.Child);
                
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

        public static void MoveChildOrder(List<Node> children, string childID, bool down)
        {
            if (children == null || children.Count <= 1) return;

            for (int i = 0; i < children.Count; i++)
                if (children[i].ID == childID)
                {
                    if (down)
                    {
                        if (i == children.Count - 1) break; // already at last possible position
                        Node swap = children[i + 1];
                        children[i + 1] = children[i];
                        children[i] = swap;
                    }
                    else
                    {
                        if (i == 0) break; // already at highest possible position
                        Node swap = children[i - 1];
                        children[i - 1] = children[i];
                        children[i] = swap;
                    }
                    break;
                }
        }
        
        public static Node GetChildofType (Node[] children, string typename)
        {
        	if (children == null) return null;
        	for (int i = 0; i < children.Length; i++)
        		if (children[i].TypeName == typename)
        			return children[i];
        	
    
        	return null;
        }

        public void GetChildIDs(string[] filteredTypes, out string[] childIDs, out string[] childNodeTypes)
        {
            Group.GetChildIDs(_children, filteredTypes, out childIDs, out childNodeTypes);
        }

        public static void GetChildIDs (List<Node> children, string[] filteredTypes, out string[] childIDs, out string[] childNodeTypes)
        {
            childIDs = null;
            childNodeTypes = null;
            int count = 0;

            if (children == null || children.Count == 0) return;

            
            childIDs = new string[children.Count];
            childNodeTypes = new string[children.Count];

            for (int i = 0; i < children.Count; i++)
            {
                //bool found = (Array.IndexOf (filteredTypes, children[i].TypeName) != -1);
                //bool found = Array.Exists(filteredTypes, s => s == children[i].TypeName);
                if (!IsFiltered(children[i].TypeName, filteredTypes))
                {
                    childIDs[count] = children[i].ID;
                    childNodeTypes[count] = children[i].TypeName;
                    count++;
                }
            }
            
            Array.Resize(ref childIDs, count);
        }


        public static Node FindDescendant(Node[] children, System.Predicate<Node> match)
        {
            if (children == null) return null;

            for (int i = 0; i < children.Length; i++)
            {
                if (match (children[i])) return children[i]; ;

                // TODO: we dont want to necessarily traverse any child that is a Group
                // until AFTER we've tried all the non group child nodes.
                // also we may not wish to traverse down to any sub-entity...
                if (children[i] is IGroup)
                {
                    if (((IGroup)children[i]).ChildCount > 0)
                    {
                        Node result = Group.FindDescendant(((IGroup)children[i]).Children, match);
                        if (result != null) return result;
                    }
                }

            }
            return null;
        }

        public static Node FindDescendantOfType(IGroup group, string typename, bool recurse)
        {
            if (group == null || group.Children == null) return null;

            for (int i = 0; i < group.ChildCount; i++)
            {
                if (recurse) throw new NotImplementedException();
                if (group.Children[i].TypeName == typename) return group.Children[i];
            }

            return null;
        }

        public static Node[] FindDescendantsOfType(IGroup group, string typename, bool recurse)
        {
            if (group == null || group.Children == null) return null;
            List<Node> results = new List<Node>();

            if (recurse) throw new NotImplementedException();
            for (int i = 0; i < group.ChildCount; i++)
            {
                if (group.Children[i].TypeName == typename)
                    results.Add(group.Children[i]);
            }
            return results.ToArray ();
        }


        /// <summary>
        /// Finds the first descendant that matches.
        /// </summary>
        public Node FindDescendant(Predicate<Keystone.Elements.Node> match)
        {
            return Group.FindDescendant(_children.ToArray(), match);
        }

        /// <summary>
        /// Finds a descendant node with a particular ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Node FindDescendant (string id)
        {
            if (_children == null) return null;

            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].ID == id) return _children[i];;

                // TODO: we dont want to necessarily traverse any child that is a Group
                // until AFTER we've tried all the non group child nodes.
                // also we may not wish to traverse down to any sub-entity...
                if (_children[i] is IGroup)
                {
                    if (((IGroup)_children[i]).ChildCount > 0)
                    {
                        Node result = ((Group)_children[i]).FindDescendant(id);
                        if (result != null) return result;
                    }
                }
               
            }
            return null;
        }

        public Node FindDescendantOfType(string typename, bool recurse)
        {
            return Group.FindDescendantOfType(this, typename, recurse);
        }

        public Node[] FindDescendantsOfType(string typename, bool recurse)
        {
            return Group.FindDescendantsOfType(this, typename, recurse);
        }

        /// <summary>
        /// Returns descendants of the start node that match the predicate function.
        /// </summary>
        /// <param name="start">IGroup node to start our search.  This node is not included in the returned results.</param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static Node[] GetDescendants(IGroup start, System.Predicate<Node> match)
        {
            Node[] children = start.Children;
            if (children == null || children.Length == 0) return null;

            List<Node> results = new List<Node>();

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null) continue;
                if (match(children[i])) results.Add(children[i]);

                if (children[i] as IGroup != null)
                {
                    if (((IGroup)children[i]).ChildCount > 0)
                        results.AddRange (GetDescendants((IGroup)children[i], match));
                }
            }

            return results.ToArray();
        }

        public Node[] GetDescendants(Type[] filteredTypes)
        {
            List<Node> results = new List<Node>();
            Group.GetDescendants(_children.ToArray(), filteredTypes, ref results);
            return results.ToArray();
        }

        public static void GetDescendants(Node[] children, Type[] filteredTypes, ref List<Node> results)
        {
            if (results == null) throw new ArgumentNullException();
            if (children == null) return;

            for (int i = 0; i < children.Length; i++)
            {
            	if (children[i] == null) continue; // shouldnt be happening but... 
                if (IsFiltered(children[i], filteredTypes)) continue;
 
                if (children[i] as IGroup != null)
                {
                    if (((IGroup)children[i]).ChildCount > 0)
                        GetDescendants(((IGroup)children[i]).Children, filteredTypes, ref results);
                }
                results.Add(children[i]);
            }
        }

        public Node FindNodeAtDescendantIndex(int index)
        {
            int startCount = 0;
            return Group.GetNodeAtDescendantIndex(this, index, ref startCount);
        }
        /// <summary>
        /// This returns a node 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static Node GetNodeAtDescendantIndex(IGroup startNode, int index, ref int startingCount)
        {
           
            for (int i = 0; i < startNode.ChildCount; i++)
            {
                startingCount++;
                if (startingCount == index) return startNode.Children[i];

                if (startNode.Children[i] is IGroup)
                {
                    Node result = GetNodeAtDescendantIndex((IGroup)startNode.Children[i], index, ref startingCount);
                    if (result != null) return result;
                }
            }

            return null;
        }

        private static bool IsFiltered(Node value, Type[] filter)
        {
            if (filter == null || filter.Length == 0) return false;

            for (int i = 0; i < filter.Length; i++)
                if (value.GetType() == filter[i]) return true;

            return false;
        }

        private static bool IsFiltered(string value, string[] filter)
        {
            if (filter == null || filter.Length == 0) return false;

            for (int i = 0; i < filter.Length; i++)
                if (value == filter[i]) return true;

            return false;
        }
        #endregion
    }
}