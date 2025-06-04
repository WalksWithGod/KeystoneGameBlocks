using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;
using Settings;
using System.Collections.Generic;
using Keystone.Extensions;

namespace Keystone.Messages
{

    /// <summary>
    /// Command is NOT a client request to create a node(s), this Command is from server
    /// to create a particular node and any descendant nodes from it. Only the server
    /// can fill in the "id" property which the client can now use to duplicate the
    /// remote entity.  That is why
    /// the array of parents, types and properties since they refer to sub-nodes from
    /// the target.  
    /// </summary>
    /// <remarks>
    /// Clients can only request a node be Added.  Therefore this
    /// command only originates from the server and contains a binary serialized
    /// node and any of it's child nodes.
    /// TODO: Why is it not simply implied that if a "Create" comes from client
    /// that it's a request!  
    /// </remarks>
    public class Node_Create : MessageBase 
    {
        //private Elements.Node mTarget;
        public string OwnerID;
        public bool IsUserVehicle;
        public int NodeCount;
        public Settings.PropertyBag[] NodeProperties;
        public string[] DescendantNodeIDs;
        public string[] DescendantNodeTypes;    // the array of first level child types of our target node
        public string[] DescendantNodeParents;  // the array of parents.  this will make it easier to deserialize
                                                // and restore the hierarchy of these descendants under the Target node
                                                // since we dont preserve a hierarchy during deserialization and
                                                // so we cannot deduce if a node read in is a sibling child or a 
                                                // child of the last child.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentID">We pass in the parent rather than rely on target.Parent because
        /// we don't want the array of parents.  Just the specific one passed in.</param>
        public Node_Create(Elements.Node node, string parentID)
            : this()
        {
            // it's a bit strange but apparently even the initial node we are adding as a descendant
            AddDescendant(node, parentID);
        }



        public Node_Create(string targetID, string targetTypename, string parentID)
            : this()
        {
            // it's a bit strange but apparently even the initial node we are adding as a descendant
            AddDescendant(targetID, targetTypename, parentID);
        }

        public Node_Create()
            : base((int)Enumerations.Node_Create)
        {
        }

        public void AddDescendant(Elements.Node node, string parentID)
        {
            AddDescendant(node.ID, node.TypeName, parentID);

            if (node is Elements.IGroup)
            {
                Elements.IGroup group = (Elements.IGroup)node;
                Elements.Node[] children = group.Children;

                if (children != null)
                    // TODO: I think we should skip nodes like DomainObject and Shader.  
                    //       in otherwords, ANY NODE that has serializable == false
                    for (int i = 0; i < children.Length; i++)
                    {
                        AddDescendant(children[i], group.ID);
                    }
            }
        }

        public void AddDescendant(string nodeID, string nodeType, string parentID)
        {
            DescendantNodeIDs = DescendantNodeIDs.ArrayAppend(nodeID);
            DescendantNodeTypes = DescendantNodeTypes.ArrayAppend(nodeType);
            DescendantNodeParents = DescendantNodeParents.ArrayAppend(parentID);
            // NOTE: The properties are discovered and then written in the Write (buffer) method.
        }

        // obsolete: Feb.9.2013  - we cannot rely on the descendant node hierarchy to exist such as in loopback
        //            network scenario where all .AddChild() calls are done in the client command
        //            processor and not in the loopback processor.  This allows our client to work
        //            with either loopback or remote server with no modifications.
        //private void CreateLists(string targetNodeID, ref List<string> types, ref List<string> parentIDs)
        //{
        //    Keystone.Elements.Node target = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(targetNodeID);

        //    // TODO: here i should in fact be recursing our target and adding those
        //    if (target is Keystone.Elements.IGroup)
        //    {
        //        Keystone.Elements.IGroup group = (Keystone.Elements.IGroup)target;
        //        for (int i = 0; i < group.ChildCount; i++)
        //        {
        //            types.Add ( group.Children[i].TypeName);
        //            parentIDs.Add(targetNodeID);

        //            // recurse this child if it's a Group with children
        //            // add results to the lists of decendant types and descendant parents
        //            // note: the "id" fields will be saved in the read/write of each node
        //            // during serialization/deserialization of this network message.
        //            CreateLists(group.Children[i].ID, ref types, ref parentIDs);
        //        }
        //    }
        //}
        
        public string FirstType { get { return DescendantNodeTypes[0]; } }
        public string FirstParent { get { return DescendantNodeParents[0]; } }
        public string FirstID 
        { 
            get 
            {
                return DescendantNodeIDs[0];
                // obsolete: Feb.9.2013 
                //int idIndex = NodeProperties[0].Properties.IndexOf("id");
                //return (string)NodeProperties[0].Properties[idIndex].DefaultValue;
            } 
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            OwnerID = buffer.ReadString();
            IsUserVehicle = buffer.ReadBoolean();
            NodeCount = buffer.ReadInt32();
            NodeProperties = new PropertyBag[NodeCount];
            DescendantNodeIDs = new string[NodeCount];
            DescendantNodeTypes = new string[NodeCount];
            DescendantNodeParents = new string[NodeCount];

            for (int i = 0; i < NodeCount; i++)
            {
                PropertyBag bag = new PropertyBag();
                DescendantNodeIDs[i] = buffer.ReadString();
                DescendantNodeTypes[i] = buffer.ReadString();
                DescendantNodeParents[i] = buffer.ReadString();
             
                int propertyCount = buffer.ReadInt32();  // the number of properties for this specific node


                for (int j = 0; j < propertyCount; j++)
                {
                    Settings.PropertySpec spec = new Settings.PropertySpec();
                    spec.Read(buffer);
                    bag.Properties.Add(spec);
                }
                NodeProperties[i] = bag;
            }
        }

        // NON-RECURSIVE writing of the node parameter and all it's children
        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(OwnerID);
            buffer.Write(IsUserVehicle);

            NodeCount = 0;
            if (DescendantNodeIDs != null)
                NodeCount = DescendantNodeIDs.Length;

            buffer.Write(NodeCount);

            for (int i = 0; i < NodeCount; i++)
            {
                // write the node id, type and the node parent id
                // NOTE: we can rely on the descendant nodes to be instance in Repository
                // but we cannot rely that the parent/child hierarchy is constructed because
                // loopback server doesn't do this, so we explicitly assign the ids, types and parents
                // to the arrays 
                // TODO: verify it's ok to write null strings here for security purposes
                buffer.Write(DescendantNodeIDs[i]);
                buffer.Write(DescendantNodeTypes[i]);
                buffer.Write(DescendantNodeParents[i]);

                Keystone.Elements.Node node = (Keystone.Elements.Node)Resource.Repository.Get(DescendantNodeIDs[i]);
                // get the properties for this node and write them to the buffer
                PropertySpec[] properties = node.GetProperties(false);
                int length = 0;
                if (properties != null) length = properties.Length;

                // write the total property count for this node
                buffer.Write(length);

                // iterate through all properties and write them 
                for (int j = 0; j < length; j++)
                    properties[j].Write(buffer); 
            }
        }

        // obsolete: Feb.9.2013  - the following implementation assumes the parent/child hierarchy
        //                         is intact and that may not be the case such as when in loopback
        //                         where parent/child relationships are skipped since they will be done
        //                        client side and since loopback client and server share nodes.
        //public override void Write(NetBuffer buffer)
        //{
        //    base.Write(buffer);

        //    // write a placeholder int32 for our node count and return to the position
        //    // later so we can fill it with the computed value
        //    NodeCount = 0;
        //    int startPosition = buffer.LengthBits;
        //    buffer.Write(NodeCount);

        //    // call to recursive WriteNode() function to write all children and grandchildren, etc
        //    WriteNode(buffer, mTarget, FirstParent);

        //    // with NodeCount++ we now know the final node count so move back
        //    // and set
        //    int finalPosition = buffer.LengthBits;
        //    buffer.LengthBits = startPosition;
        //    buffer.Write(NodeCount);
        //    buffer.LengthBits = finalPosition;

        //}

        //// Recursive writing of the node parameter and all it's children
        //private void WriteNode(NetBuffer buffer, Elements.Node node, string parent)
        //{
        //    NodeCount++;

        //    // write the node type and the node parent id
        //    buffer.Write(node.TypeName);
        //    buffer.Write(parent);

        //    // get the properties for this node and write them to the buffer
        //    PropertySpec[] properties = node.GetProperties(false);
        //    int length = 0;
        //    if (properties != null) length = properties.Length;

        //    // write the total property count for this node
        //    buffer.Write(length);

        //    // iterate through all properties and write them
        //    for (int i = 0; i < length; i++)
        //        properties[i].Write(buffer);

        //    if (node is Elements.IGroup)
        //        if (((Elements.IGroup)node).ChildCount > 0)
        //            for (int i = 0; i < ((Elements.IGroup)node).ChildCount; i++)
        //                WriteNode(buffer, ((Elements.IGroup)node).Children[i], node.ID);
        //}
        #endregion
    }
}
