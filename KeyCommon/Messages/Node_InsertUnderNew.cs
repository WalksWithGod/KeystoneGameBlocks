using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;
using Settings;
using System.Collections.Generic;
using Keystone.Extensions;


namespace KeyCommon.Messages
{
    public class Node_InsertUnderNew : MessageBase // response from Node_InsertUnderNew_Request
    { 
        // this class does need to send the properties of the new created parent node
        // however, it then only needs to tell us of the original parent and the nodeToReparent
        // and then the client can directly move it.  

        public string Parent;
        public string ReparentedNode; // form child of Parent that is being re-parented to InsertedNode

        public string InsertedNode;
        public string InsertedNodeType; // need type to create this node from factory
        public Settings.PropertySpec[] InsertedNodeProperties; // Ok for propertyCount to be 0 as in case of just using default values.


        public Node_InsertUnderNew(string parent, string insertedNode, string insertedNodeType, string nodeToReparent)
            : this ()
        {
            Parent = parent;
            ReparentedNode = nodeToReparent;
            InsertedNode = insertedNode;
            InsertedNodeType = insertedNodeType;

           // Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(insertedNode);
        }

        public Node_InsertUnderNew()
            : base ((int)Enumerations.Node_InsertUnderNew)
        { }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            Parent = buffer.ReadString();
            ReparentedNode = buffer.ReadString();
            InsertedNode = buffer.ReadString();
            InsertedNodeType = buffer.ReadString();

            // NOTE: Ok for propertyCount to be 0 as in case of just using default values.
            int propertyCount = buffer.ReadInt32();

            if (propertyCount > 0)
            {
                InsertedNodeProperties = new PropertySpec[propertyCount];
                for (int i = 0; i < propertyCount; i++)
                {
                    Settings.PropertySpec spec = new Settings.PropertySpec();
                    spec.Read(buffer);
                    InsertedNodeProperties[i] = spec;
                }
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
                        
            buffer.Write(Parent);
            buffer.Write(ReparentedNode);
            buffer.Write(InsertedNode);
            buffer.Write(InsertedNodeType);


//Keystone.Elements.Node insertedNode = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(InsertedNode);

            // get the properties for this node and write them to the buffer
            int propertyCount = 0;
            if (InsertedNodeProperties != null)
                propertyCount = InsertedNodeProperties.Length;

            // NOTE: Ok for propertyCount to be 0 as in case of just using default values.

            // write the total property count for this node
            buffer.Write(propertyCount);

            // iterate through all properties and write them
            for (int i = 0; i < propertyCount; i++)
                InsertedNodeProperties[i].Write(buffer);

        }

        #endregion
    }
}
