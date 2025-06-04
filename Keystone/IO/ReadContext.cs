using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Amib.Threading;
using Keystone.Elements;
using Keystone.Extensions;

namespace Keystone.IO
{
    public class ReadContext
    {
        private XMLDatabase mXMLDB;

        public string ParentTypeName;
        public string ParentNodeID;
        public Stack<Node> Parents = new Stack<Node>();
        public Node PrefabInstanceStartingNode;

        public string NodeID;  // the id field of the child node we are looking to find.
        public string TypeName; // the TypeName of the child node we are looking to find.
        public bool DelayTVResourceLoading;
        public bool GenerateIDs; // used when we don't want to use the Node IDs for non-shareable Nodes that are in the xml
        public Queue<string> CachedIDs = new Queue<string>();
        public bool RecursivelyLoadChildren; 

        public string XmlFilePath; // used when we want to load a prefab rather than from archived scene file
        public Stack<XmlNode> XmlParentNodes = new Stack<XmlNode>();
        public Node[] Nodes;
        // TODO: maybe this should be a stack so our SceneReader can pop() them off the stack as each Entity is deserialized from prefab
        public Queue<string> NodeIDsToUse = new Queue<string>(); // when not using the id's from non-shareable Nodes from the xml itself.   
        
        public PostExecuteWorkItemCallback ReadCompletedCallback;


        public ReadContext(XMLDatabase xmldb, string typename, string nodeID, string parentNodeID, PostExecuteWorkItemCallback postReadCB)
        {
            TypeName = typename; // can be null when we wish to retreive all child nodes under the specified parentNodeName
            NodeID = nodeID;     // can be null when we wish to retreive all child nodes under the specified parentNodeName
            ParentNodeID = parentNodeID;
            ReadCompletedCallback = postReadCB;

            mXMLDB = xmldb;
        }

        public ReadContext(string xmlFilePath,  PostExecuteWorkItemCallback postReadCB)
        {

            XmlFilePath = xmlFilePath;
            NodeID = "";
            TypeName = "";
            ParentNodeID = "";
            ReadCompletedCallback = postReadCB;
        }

        public Node Node 
        {
            get 
            {
                if (Nodes == null) return null;
                return Nodes[0];
            }
        }
        public void AddNode(Node node)
        {
            Nodes = Nodes.ArrayAppend(node);
        }

        public XmlDocument SelectXMLDocument(string typename)
        {
            // what if the archiver stayed with the Context.  That could solve some issues...
            // TODO: This should load a document from disk if it's uncompressed as well...
            return mXMLDB.GetDocument(typename, false);
        }
    }
}