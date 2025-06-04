using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Amib.Threading;
using Keystone.Appearance;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Portals;
using Keystone.Scene;
using KeyCommon.Helpers;


namespace Keystone.IO
{
    public class SceneWriter : IDisposable
    {
        // basically can be used to notify everytime a node is written including number of nodes written and number remaining.
        public delegate void WriteProgress(out int written, out int remaining);

        public delegate void WriteCallBack(Node obj);

        private int _nodeCount;

        private SmartThreadPool _threadpool;
        // we want all writes to be done through a single group that has a concurrency of 1.  Thus
        // our threadpool can also handle the work of other groups at the same time, but as for Writes, only one threaded
        // write at a time is possible.
        private object mGroupLock;
        IWorkItemsGroup mSceneWriterThreadPoolGroup; // TODO: should an xmldb reader/writer share a group?  I think maybe so!
        private XMLDatabase mXMLDB;

        private bool _disableWrite, _suspendWrite;
        private int mTimeOut = 60000;
        private const int REQUIRED_CONCURRENCY_OF_ONE = 1;
        private const int REQUIRED_THREAD_COUNT = 1;

        private bool mForceWriteToSingleXMLFile = false;
        private bool mDisableNodeState = true; // Hypnotron - Jan.13.2024 - NodeState deserialization is failing during arcade mode (specifically actor animations and rigiidbody nodes) so we will just disable it completely.  Its not necessary for v1.0


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmldb">Can be null.  If so, then all writes occur to a single XML file, not an archive of xml documents for each typename</param>
        /// <param name="threadpool"></param>
        /// <param name="cb"></param>
        /// <param name="progresscb"></param>
        public SceneWriter(XMLDatabase xmldb, SmartThreadPool threadpool, WriteCallBack cb,
                           WriteProgress progresscb)
        {
            if (threadpool == null) throw new ArgumentNullException();

            _threadpool = threadpool;
            mGroupLock = new object();


            mSceneWriterThreadPoolGroup = _threadpool.CreateWorkItemsGroup(REQUIRED_CONCURRENCY_OF_ONE);
            mSceneWriterThreadPoolGroup.Name = "xmldb_SceneWriterThreadpoolGroup";
            mSceneWriterThreadPoolGroup.OnIdle += new WorkItemsGroupIdleHandler(SceneWriterThreadGroupIdle);

            mXMLDB = xmldb;
            if (mXMLDB == null) mForceWriteToSingleXMLFile = true;
        }

        public bool SuspendWrite
        {
            get { return _suspendWrite; }
            set { _suspendWrite = value; }
        }

        public bool DisableWrite
        {
            get { return _disableWrite; }
            set { _disableWrite = value; }
        }

        public void WriteSychronous(Node node, bool recursivelyWriteChildren, bool saveOnWrite)
        {
            WriteContext context = new WriteContext();
            context.SaveOnWrite = saveOnWrite;
            context.Node = node;
            context.Recurse = recursivelyWriteChildren;

            WriteNodeWorker(context);
        }

        /// <summary>
        /// This can be called to write a single xml file without the need of instantiating an XMLDatabase 
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="node"></param>
        /// <param name="recursivelyWriteChildren"></param>
        public void WriteSychronous(string xmlFilePath, Node node, bool recursivelyWriteChildren)
        {
            mForceWriteToSingleXMLFile = true;

            WriteContext context = new WriteContext();
            context.SaveOnWrite = true;
            context.Node = node;
            context.Recurse = recursivelyWriteChildren;
            context.SingleDocumentSavePath = xmlFilePath;

            WriteNodeWorker(context);
        }

        /// <summary>
        /// This can be called to write a single memory stream of a single all inline xml document with no need of instantiating an XMLDatabase
        /// </summary>
        /// <param name="node"></param>
        /// <param name="recursivelyWriteChildren"></param>
        /// <param name="stream"></param>
        public void WriteSychronous(Node node, bool recursivelyWriteChildren, out Stream stream)
        {
            mForceWriteToSingleXMLFile = true;

            WriteContext context = new WriteContext();
            context.SaveOnWrite = false;
            context.Node = node;
            context.Recurse = recursivelyWriteChildren;
            context.SingleDocumentSavePath = null;
            context.SingleDocumentStream = new MemoryStream(); // if stream is not null, then SingleDocumentSavePath will never be used.
            
            WriteNodeWorker(context);
            stream = context.SingleDocumentStream;
        }

        // NOTE: asynchronous writes like this are rarely used because the WriteSychrnous() is already being called from a Worker thread in FormMain.Commands.cs or FormMain.LoopbackServer.cs
        public void Write(string xmlFilePath, Node node, PostExecuteWorkItemCallback writeCompletedHandler)
        {
            WriteContext context = new WriteContext();
            context.SaveOnWrite = true;
            context.SingleDocumentSavePath = xmlFilePath;
            context.Node = node;
            context.WriteCompletedCallback = writeCompletedHandler;
            mForceWriteToSingleXMLFile = true;
            Write(context);
        }

        /// <summary>
        ///  Saves a node to the default archive.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="saveOnWrite"></param>
        public void Write(Node node, bool recursivelyWriteChildren, bool saveOnWrite, PostExecuteWorkItemCallback writeCompletedHandler)
        {
            if (_disableWrite || _suspendWrite) return;

            if (!mXMLDB.IsInlineType(node))
            {
                WriteContext context = new WriteContext();
                context.SaveOnWrite = saveOnWrite;
                context.Node = node;
                context.Recurse = recursivelyWriteChildren;
                context.WriteCompletedCallback = writeCompletedHandler;
                Write(context);
            }
        }

        public void Write(WriteContext context)
        {
            lock (mGroupLock)
            {
                AsychWrite(mSceneWriterThreadPoolGroup, context);
            }
        }


        private void SceneWriterThreadGroupIdle(IWorkItemsGroup group)
        {
            Trace.WriteLine("Theadpool Group '" + group.Name + "' idle.");

            // TODO: set ProgressCallBack for this Group.Name to 100% 

        }

        private void AsychWrite(IWorkItemsGroup group, WriteContext context)
        {
            group.QueueWorkItem(new WorkItemCallback(WriteNodeWorker), context,
                                context.WriteCompletedCallback);
        }

        private object WriteNodeWorker(object obj)
        {
            WriteContext context = null;
            try
            {
                context = (WriteContext)obj;
                // NOTE: returning the document is relatively useless here because the call to WriteDocument()
                // is recursive if context.Recurse == false and results in multiple xml documents being written.
                // If we want to have a Stream output option, then we should set mForceWriteToSingleXMLFile = true 
                // and possibly context.SingleDocumentSavePath or context.SingleDocumentStream being non null.  and then document.Save(context.SingleDocumentStream) 
                XmlDocument document = WriteDocument(context);
                if (context.SingleDocumentStream != null)
                    document.Save(context.SingleDocumentStream);

                else if (context.SaveOnWrite)
                    mXMLDB.SaveAllChanges(); 

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                //Trace.WriteLine(string.Format("SceneWriter.Write() -- Writing node '{0}'", context.Node.ID));
                //debug: shell the file so it opens in notepad...
                //Process p = new Process();
                //p.StartInfo.FileName = _archiver.GetTempFile(context.Node.TypeName);
                //p.Start();
            }
            return null;
        }

        private XmlDocument WriteDocument(WriteContext context)
        {
            XmlDocument document = null;
            try
            {
                // NOTE: Must always use child.TypeName and NOT child.GetType().Name because we have
                // custom Typename for Generic types (eg Generic Interpolation Animation type)
                string typename = context.Node.TypeName;


                if (mXMLDB != null)
                {
                    document = mXMLDB.GetDocument(typename, true);
                }
                else
                    document = XmlHelper.CreateXmlDocument("root");

                if (document == null)
                    System.Diagnostics.Trace.Assert(document != null, "SceneWriter.WriteDocument() - Could not find document for type '" + typename + "'");

                BuildXMLOutput(document, context); // recursive-ish
                return document;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }


        private bool mDisableSourceReferencing = true; // Hypnotron - Dec.14.2024 - saving nested prefabs (eg BonedEntitiy inside Vehicle.Interior) is broken and the new vehicle prefab that contains them wont load

        /// <summary>
        /// Performs the actual writing to our XmlDocument.  The principle design allows for the following.
        /// 
        /// You can indpendandly delete, modify, add a node to any document without having to rebuild the contents
        /// of the entire document by hand as would normally be required with XmlWriter
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private object BuildXMLOutput(XmlDocument document, WriteContext context)
        {
            Node currentSceneNode = context.Node;
            XmlNode xmlParent;

            try
            {
                if (context.Parents == null || context.Parents.Count == 0 || context.Parents.Peek() == null)
                {
                    xmlParent = document.DocumentElement;
                    Debug.Assert(xmlParent != null);
                }
                else
                {
                    string parentPath = context.XMLParentPath.Peek();
                    xmlParent = XmlHelper.SelectNode(document.DocumentElement, parentPath, Core.ATTRIB_ID, context.Parents.Peek().ID);
                    Trace.Assert(xmlParent != null);
                }


                // is the very first root node of this scene or prefab, a prefab?
                if (context.StartingPrefabNode.Count == 0)
                {
                    bool isPrefab = string.IsNullOrEmpty(currentSceneNode.SRC) == false;
                    if (mDisableSourceReferencing) // TODO: this fixes an issue with nested prefabs saving.  We simply tream them as regular nodes and don't attempt to maintain a reference SRC prefab.
                    {
                        if (isPrefab)
                            currentSceneNode.SRC = null;

                        isPrefab = false; 
                    }
                    // note: we pop the stack at the end of this function 
                    if (isPrefab)
                        context.StartingPrefabNode.Push(currentSceneNode);
                }

                // write or refresh an XML element for this scene node
                XmlNode xmlElement = null;
                bool skipWritingOfThisNodeAndItsChildren = (currentSceneNode.Serializable == false) ||
                    (currentSceneNode.GetFlagValue((byte)Node.NodeFlags.ForceSerializeSeperate) == true && context.Parents.Count != 0);

                if (!skipWritingOfThisNodeAndItsChildren)
                {
                    if (context.StartingPrefabNode.Count > 0)
                    {
                        // if we are writing NodeStates and the element type is a 
                        // NON shareable node like Mesh3d, Actor3d, Texture, Material, DomainObject, 
                        // we should return null immediately rather than continue and call WriteNodeElement
                        // for NodeStates that are irrelevant.
                        if (currentSceneNode.Shareable == true)
                            skipWritingOfThisNodeAndItsChildren = true; // TODO: this is too late.  Should we move this higher up to the test where we determine if a node is skippable?  Well i tried that and then Geometry for BoneActors wouldn't load
                        else if (mDisableNodeState == false)
                            xmlElement = WriteNodeStateElement(document, xmlParent, currentSceneNode, context.StartingPrefabNode.Peek());
                        else
                            xmlElement = WriteNodeElement(document, xmlParent, currentSceneNode);
                    }
                    else
                        xmlElement = WriteNodeElement(document, xmlParent, currentSceneNode);

                    // Feb.14.2016 - must remove all existing child nodes or some deleted or moved scene nodes
                    // will still show up under this element.
                    xmlElement.RemoveAll();

                    // Now that we have the selected node, we need to create all the rest of the attributes
                    // for this node based on the type.
                    WriteNodeProperties(xmlElement, currentSceneNode);

                    if (context.XMLParentPath.Count == 0 || context.XMLParentPath.Peek() == null)
                        context.XMLParentPath.Push("//" + xmlElement.Name);
                    else
                        context.XMLParentPath.Push(context.XMLParentPath.Peek() + "//" + xmlElement.Name);

                    //if (currentSceneNode is Behavior.Behavior)
                    //   System.Diagnostics.Debug.WriteLine("SceneWriter.BuildXMLOutput() - " + currentSceneNode.TypeName);

                    // if recursing enabled and if this node has children, cast it to IGroup and recurse and save the child nodes
                    if (context.Recurse == true && skipWritingOfThisNodeAndItsChildren == false)
                    {
                        IGroup group = null;
                        if (currentSceneNode is IGroup) group = (IGroup)currentSceneNode;

                        if (group != null && group.Children != null && group.Children.Length > 0)
                        {
                            foreach (Node childSceneNode in group.Children)
                            {
                                //if (childSceneNode is Celestial.StellarSystem)
                                //    System.Diagnostics.Debug.WriteLine("SceneWriter.BuildXMLOutput - Child Encountered '" 
                                //        + childSceneNode.TypeName);
                                // starting nodes of prefab instance can be ref'd in seperate xml table document so let's determine if that 
                                // is applicable now for this child. We must not already be on a prefab node so check count for 0
                                // and if we're not already inside a prefab and this currrent child is, then we are just starting
                                // and can potentially see if we can write it to it's own table (eg Models.xml or Entities.xml)
                                bool inlineStartingPrefabInstance =
                                            string.IsNullOrEmpty(childSceneNode.SRC) == false &&
                                            context.StartingPrefabNode.Count == 0;


                               //if (childSceneNode is Behavior.Behavior)
                               // {
                               //     //inlineStartingPrefabInstance = true;
                               //     System.Diagnostics.Debug.WriteLine("SceneWriter.BuildXMLOutput() - " + childSceneNode.TypeName);
                                //}
                                context.Node = childSceneNode;
                                
                                // skip any child node that is set to ignore scene save
                                // or
                                // if we are in prefab mode, and this is a shareable node, skip without recursing
                                // because there is no useful state information to save for shared nodes within a prefab INSTANCE
                                if (childSceneNode.Serializable == false ||
                                    (context.StartingPrefabNode.Count > 0 && childSceneNode.Shareable))
                                {
                                    //System.Diagnostics.Debug.WriteLine("SceneWriter.BuildOutput() - Skipping save of 'Serializable == false' flagged node:");
                                    //System.Diagnostics.Debug.WriteLine("SceneWriter.BuildOutput() - '" + childSceneNode.TypeName + "' id '" + childSceneNode.ID + "'");
                                    context.Parents.Push(null); // push a null so we can pop without altering the stack
                                }
                                else if (mForceWriteToSingleXMLFile == false && (inlineStartingPrefabInstance || mXMLDB.IsInlineType(childSceneNode) == false))
                                {
                                    // determine if the "ref" node we need already exists, if not create
                                    // NOTE: Must always use child.TypeName and NOT child.GetType().Name because we have
                                    // custom Typename for Generic types (eg Generic Interpolation Animation type)
                                    if (childSceneNode.GetFlagValue((byte)Node.NodeFlags.ForceSerializeSeperate) != true)
                                    {
                                        XmlNode refNode = XmlHelper.SelectNode(xmlElement, childSceneNode.TypeName, Core.ATTRIB_REF, childSceneNode.ID);

                                        // NOTE: isRef and isSrc can never both be true due to the way if "ref" to a dedicated
                                        // table is required, then the "src" will not be used until inside that dedicated table
                                        if (refNode == null)
                                        {
                                            // create new if none already exists
                                            refNode = document.CreateElement(childSceneNode.TypeName);
                                            XmlHelper.CreateAttribute(refNode, Core.ATTRIB_REF, childSceneNode.ID);
                                            xmlElement.AppendChild(refNode);
                                        }
                                    }
                                    // recurse to the new document to continue writing this branch.  But since this is a new
                                    // "table" document, push a null as the previous parent
                                    context.Parents.Push(null);

                                    // likewise to pushing null for parents, we must now push null for XMLParentPath  
                                    // since we're now start of a new document
                                    context.XMLParentPath.Push(null);

                                    // RECURSE to seperate document (a ref document eg Models.xml vs Entities.xml)
                                    XmlDocument doc = WriteDocument(context);
                                    
                                    context.XMLParentPath.Pop();
                                }
                                else // write inline current document
                                {
                                    // we keep the current document and just call BuildOutput directly
                                    // these node's will be added as children to the current selected xml node
                                    // Thus, the parent node must be supplied
                                    context.Parents.Push(currentSceneNode);

                                    try
                                    {
                                        // RECURSE within the current document
                                        BuildXMLOutput(document, context);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(ex.Message);
                                    }
                                }

                                // restore the stack
                                context.Node = currentSceneNode;
                                context.Parents.Pop();
                            }
                        }
                        context.XMLParentPath.Pop();
                    }
                }
	
	            if (context.StartingPrefabNode.Count > 0 && context.StartingPrefabNode.Peek() == currentSceneNode) 
	            {
	                context.StartingPrefabNode.Pop();
	            }
	            return null;
            }
            catch (Exception ex)
            {
                throw new Exception("SceneWriter.BuildXMLOutput() - " + ex.Message);
            }
        }

        private XmlNode WriteNodeStateElement(XmlDocument document, XmlNode root, Node sceneNode, Node startingPrefabNode)
        {
            XmlNode selected = XmlHelper.SelectNode(root, sceneNode.TypeName, Core.ATTRIB_ID, sceneNode.ID);
            int descendantIndex = 0;
            bool isStartingNodeOfPrefab = startingPrefabNode == sceneNode;

            if (isStartingNodeOfPrefab == false)
            {
                // compute the descendant index of this node
                bool found = Node.GetNodeAncestryIndex((IGroup)startingPrefabNode, sceneNode, ref descendantIndex);
                if (found == false) throw new Exception ();
            }

            // if this Element doesn't exist, we will be creating it and thus inserting a
            // previously unsaved node rather than updating existing
            if (selected == null)
            {
                if (isStartingNodeOfPrefab)
                {
                    // starting node write "src" instead of "id"
                    // NOTE: we don't use NodeState for the starting node.  We need the 
                    // typename of the actual node to be correct
                    selected = document.CreateElement(sceneNode.TypeName);
                    XmlHelper.CreateAttribute(selected, Core.ATTRIB_SRC, sceneNode.SRC);
                }
                else
                {
                    string nodeStateTypeName = typeof(NodeState).Name;
                    // our initial call to XmlHelper.SelectNode() was using the original typename
                    // but now we must re-select to see if there is a NodeState already so that we 
                    // are not rewriting it everytime we re-save the scene!
                    selected = XmlHelper.SelectNode(root, nodeStateTypeName, Core.ATTRIB_ID, sceneNode.ID);

                    // non starting nodes of a prefab just use NodeStates
                    if (selected == null)
                        selected = document.CreateElement(nodeStateTypeName);
                    XmlHelper.CreateAttribute(selected, Core.ATTRIB_ID, sceneNode.ID);
                }

                //CreateElement does not automatically add this node to the Document so we must insert it.
                root.AppendChild(selected);
            }
            else
            {
                if (isStartingNodeOfPrefab == false)
                {
                    // removed the previous selected because it is a SceneNode and
                    // we need it to have NodeState typenmae
                    root.RemoveChild(selected);

                    // check for an existing NodeState typename for this "id"
                    // TODO: we should verify/assert that the index and referenceTypename
                    // attributes for this existing NodeState are correct
                    string nodeStateTypeName = typeof(NodeState).Name;
                    // our initial call to XmlHelper.SelectNode() was using the original typename
                    // but now we must re-select to see if there is a NodeState already so that we 
                    // are not rewriting it everytime we re-save the scene!
                    selected = XmlHelper.SelectNode(root, nodeStateTypeName, Core.ATTRIB_ID, sceneNode.ID);
                    if (selected == null)
                        selected = document.CreateElement(nodeStateTypeName);
                    else
                        selected.Attributes.RemoveAll();
                }
                else
                    selected.Attributes.RemoveAll();


                XmlHelper.CreateAttribute(selected, Core.ATTRIB_ID, sceneNode.ID);
            }


            if (isStartingNodeOfPrefab == false)
            {
                WriteNodeProperty(selected, NodeState.ATTRIBUTE_REF_INDEX, descendantIndex.ToString());
                WriteNodeProperty(selected, NodeState.ATTRIBUTE_REF_TYPENAME, sceneNode.TypeName);
            }
            return selected;
        }

        private XmlNode WriteNodeElement(XmlDocument document, XmlNode root, Node sceneNode)
        {
            // NOTE: Must always use sceneNode.TypeName and NOT child.GetType().Name because we have
            // custom Typename for Generic types (eg Generic Interpolation Animation type)
            // find the node with the desired scenenode's name in the document
            XmlNode selected = XmlHelper.SelectNode(root, sceneNode.TypeName, Core.ATTRIB_ID, sceneNode.ID);
            // if this Element doesn't exist, we will be creating it and thus inserting a
            // previously unsaved node rather than updating existing
            if (selected == null)
            {
                selected = document.CreateElement(sceneNode.TypeName);
                XmlHelper.CreateAttribute(selected, Core.ATTRIB_ID, sceneNode.ID);

                //CreateElement does not automatically add this node to the Document so we must insert it.
                root.AppendChild(selected);
            }
            else
            {
                // remove attributes since we prefer to rebuild from scratch since
                // this node may have been stale existing that needs updating particularly
                // if this xml document was a previous save we've reopened and loaded
                selected.Attributes.RemoveAll();
                XmlHelper.CreateAttribute(selected, Core.ATTRIB_ID, sceneNode.ID);
            }

            return selected;
        }

        /// <summary>
        /// Writes new xml attributes by appending to the xmlNode without removing existing.
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="sceneNode"></param>
        private void WriteNodeProperties(XmlNode xmlNode, Node sceneNode)
        {
            Settings.PropertySpec[] specs = sceneNode.GetProperties(false);

            if (specs != null)
                for (int i = 0; i < specs.Length; i++)
                    if (specs[i] != null)
                        specs[i].WriteXMLAttribute(xmlNode);
        }

        private void WriteNodeProperty(XmlNode xmlNode, string attributeName, string attributeValue)
        {
            Keystone.IO.XmlHelper.CreateAttribute(xmlNode, attributeName, attributeValue);
        }

        //private void NumberNodes(IGroup parent, int counter)
        //{

        //    foreach (Node child in parent.Children)
        //    {
        //        NodeState state = new NodeState(counter);
        //        if (child is IGroup)
        //            NumberNodes((IGroup)child, counter);
        //    }
        //}

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}