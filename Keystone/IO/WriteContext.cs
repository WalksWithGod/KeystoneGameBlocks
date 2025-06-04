using System.Collections.Generic;
using Amib.Threading;
using Keystone.Elements;

namespace Keystone.IO
{
    public class WriteContext
    {
        public string SingleDocumentSavePath; // user specifiies path for saving Prefabs or other partial nodes and not hte entire tree
        public System.IO.Stream SingleDocumentStream; // NOTE: if using stream, SaveOnWrite will not occur.

        public bool SaveOnWrite = false;
        public PostExecuteWorkItemCallback WriteCompletedCallback;
        public bool IsAsychronousWrite = false;

        public Node Node;
        public Stack<Node> Parents = new Stack<Node>();
        public Stack<string> XMLParentPath = new Stack<string>(); // XMLPath of this node including itself
        public Stack<Node> StartingPrefabNode = new Stack<Node>();
        public bool Recurse; // we do not always want to recursive write because
                             // we may only have loaded part of the Entity branch such as
                             // Entity and DOmainObject only and to save recursively would
                             // effectively remove all the other children types.
    }
}