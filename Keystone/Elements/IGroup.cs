using Keystone.Traversers;

namespace Keystone.Elements
{
    public interface IGroup : ITraversable
    {
    	string ID {get;}
    	void MoveChildOrder (string childID, bool down);
        void RemoveChildren();
        void RemoveChild(Node child);
        Node[] Children { get; }
        void GetChildIDs(string[] filteredTypes, out string[] childIDs, out string[] childTypes);
        Node FindDescendantOfType(string typeName, bool recurse);
        Node[] FindDescendantsOfType(string typeName, bool recurse);
        Node FindDescendant (System.Predicate<Keystone.Elements.Node> match);
        Node FindNodeAtDescendantIndex(int index); 
        int ChildCount { get; }
    }
}