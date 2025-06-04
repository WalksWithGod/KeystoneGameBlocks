using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers; // Extension Methods for PropertySpec.Read/Write
using System.Collections.Generic;

namespace KeyCommon.Messages
{
    // Unlike the other "requests" which should be renamed I think 
    // this "request" is mostly to receive a GUID and so does not
    // include a list of  properties that should be set on the created node.
    // TODO: why am I not sending the properties I wish to apply to the node if
    //       the request is granted?  I mean why do i have to wait to send a Node_ChangeProperties?
    //       i cant easily do that because it seperates the intent and its no longer trivial to know which create request
    //       desired which properties to be applied
    //
    // TODO: notice here i'm not recursively adding descendant nodes
    public class Node_Create_Request : MessageBase
    {
        public string NodeTypename;
        public string ParentID;
        public string ResourcePath;

        private List<Settings.PropertySpec> mPropertySpecs; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public Node_Create_Request(string nodeType, string resourcePath, string parentID)  : this()
        {
            NodeTypename = nodeType;
            ParentID = parentID;
            ResourcePath = resourcePath;
        }

        
        public Node_Create_Request(string nodeType, string parentID)
            : this(nodeType, "", parentID)
        {
        }

        public Node_Create_Request (string nodeType, string parentID, Settings.PropertySpec[] specs) : this (nodeType, parentID)
        {
        	Add (specs);
        }
        
        public Node_Create_Request()
            : base ((int)Enumerations.Node_Create_Request)
        {
        }

        public Settings.PropertySpec[] Properties 
        { 
        	get 
        	{
        		if (mPropertySpecs == null) return null; 
        		return mPropertySpecs.ToArray(); 
        	}
        }
        

        public void Add(string propertyName, string typeName, object value)
        {
            Add(new Settings.PropertySpec(propertyName, typeName, value));
        }

        public void Add(Settings.PropertySpec spec)
        {
            if (mPropertySpecs == null) mPropertySpecs = new List<Settings.PropertySpec>();
            mPropertySpecs.Add(spec);
        }
        
        public void Add(Settings.PropertySpec[] specs)
        {
        	if (mPropertySpecs == null) mPropertySpecs = new List<Settings.PropertySpec>();
            mPropertySpecs.AddRange(specs);
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeTypename = buffer.ReadString();
            ParentID = buffer.ReadString();
            ResourcePath = buffer.ReadString();
        
            // properties
            int count = buffer.ReadInt32();

            if (count == 0) return;
            mPropertySpecs = new List<Settings.PropertySpec>();
            for (int i = 0; i < count; i++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mPropertySpecs.Add(spec);
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeTypename);
            buffer.Write(ParentID);
            buffer.Write(ResourcePath);

            // properties
            int count = 0;
            if (mPropertySpecs != null)
                count = mPropertySpecs.Count;

            buffer.Write(count);
            if (count > 0)
                for (int i = 0; i < count; i++)
                {
                    mPropertySpecs[i].Write(buffer);
                }
        }
        #endregion
    }
}
