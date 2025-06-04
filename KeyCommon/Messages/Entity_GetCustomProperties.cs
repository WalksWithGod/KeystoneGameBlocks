using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Client request to server to receive custom property information
    /// about Entity.
    /// </summary>
    public class Entity_GetCustomProperties : MessageBase
    {
        public string SceneName;
        public string EntityID;
        public string EntityTypeName;

        // we can reply back with Node_Create
        public Entity_GetCustomProperties()
            : base((int)Enumerations.Entity_GetCustomProperties) 
        {
        }

        // note: entityTypeName is supplied because we often want all of the properties from an entity
        // even if it's still in the xmldatabase. 
        public Entity_GetCustomProperties(string sceneName, string entityID, string entityTypeName)
            : this()
        {
            SceneName = sceneName;
            EntityID = entityID;
            EntityTypeName = entityTypeName;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            SceneName = buffer.ReadString();
            EntityID = buffer.ReadString();
            EntityTypeName = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(SceneName);
            buffer.Write(EntityID);
            buffer.Write(EntityTypeName);
        }
        #endregion
    }

    /// <summary>
    /// Reply from Server to Client after the server has received a Entity_GetCustomProperties
    /// message.
    /// </summary>
    public class Entity_SetCustomProperties : MessageBase
    {
        public string SceneName;
        public string EntityID;
        public string EntityTypeName;

        private List<Settings.PropertySpec> mCustomPropertySpecs;

        public Entity_SetCustomProperties()
            : base((int)Enumerations.Entity_SetCustomProperties)
        {
            mCustomPropertySpecs = new List<Settings.PropertySpec>();
        }

        public Entity_SetCustomProperties(Settings.PropertySpec[] properties)
            : base((int)Enumerations.Entity_SetCustomProperties)
        {
            mCustomPropertySpecs = new List<Settings.PropertySpec>();
            for (int i =0 ; i < properties.Length ; i++)
                mCustomPropertySpecs.Add (properties[i]);
        }

        public void Add(Settings.PropertySpec spec)
        {
            mCustomPropertySpecs.Add(spec);
        }

        public Settings.PropertySpec[] CustomProperties { get { if (mCustomPropertySpecs == null) return null; return mCustomPropertySpecs.ToArray(); } }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            SceneName = buffer.ReadString();
            EntityID = buffer.ReadString();
            EntityTypeName = buffer.ReadString();
            int count = buffer.ReadInt32();

            mCustomPropertySpecs = new List<Settings.PropertySpec>();
            for (int i = 0; i < count; i++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mCustomPropertySpecs.Add(spec);
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(SceneName);
            buffer.Write(EntityID);
            buffer.Write(EntityTypeName);

            int count = 0;
            if (mCustomPropertySpecs != null)
                count = mCustomPropertySpecs.Count;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                mCustomPropertySpecs[i].Write(buffer);
            }
        }
        #endregion
    }
}
