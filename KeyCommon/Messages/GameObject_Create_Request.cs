using System;
using KeyCommon.Messages;
using System.Collections.Generic;
using KeyCommon.Helpers;
using Lidgren.Network;
using KeyCommon.DatabaseEntities;

namespace KeyCommon.Messages
{
    // Unlike the other "requests" which should be renamed I think 
    // this "request" is mostly to receive a GUID and so does not
    // include a list of  properties that should be set on the created node.
    public class GameObject_Create_Request : MessageBase
    {
        public string GameObjectType;
        public string OwnerID;
        // an Entity Create Request supplies a path to an entity xml prefab
        // however GameObjects have no "definition" to select from.  Instead
        // we allow this class to GetProperties/SetProperties
        // along with our buffer extension methods for converting those to NetBuffer
        public Settings.PropertySpec[] Properties;

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public GameObject_Create_Request(string nodeType, string parentID)
            : this()
        {
            GameObjectType = nodeType;
            OwnerID = parentID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public GameObject_Create_Request(string nodeType, string parentID, Settings.PropertySpec[] properties)
            : this()
        {
            GameObjectType = nodeType;
            OwnerID = parentID;
            Properties = properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="parentID"></param>
        /// <param name="resourcePath">Optional resource path to use when loading an IPageableResource type</param>
        public GameObject_Create_Request(string nodeType, string parentID, GameObject gameObject)
            : this()
        {
            if (gameObject == null) throw new ArgumentNullException();
            GameObjectType = nodeType;
            OwnerID = parentID;
            Properties = gameObject.GetProperties(false);
        }

        public GameObject_Create_Request()
            : base ((int)Enumerations.GameObject_Create_Request)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            GameObjectType = buffer.ReadString();
            OwnerID = buffer.ReadString();

            int count = buffer.ReadInt32();

            if (count > 0)
            {
                Properties = new Settings.PropertySpec[count];
                for (int i = 0; i < count; i++)
                {
                    Settings.PropertySpec spec = new Settings.PropertySpec();
                    spec.Read(buffer);
                    Properties[i] = spec;
                }
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(GameObjectType);
            buffer.Write(OwnerID);

            int count = 0;
            if (Properties != null)
                count = Properties.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                Properties[i].Write(buffer);
            }

        }
        #endregion
    }
}
