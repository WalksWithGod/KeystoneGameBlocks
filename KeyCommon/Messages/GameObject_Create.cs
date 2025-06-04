using System;
using KeyCommon.Messages ;
using Lidgren.Network;
using KeyCommon.DatabaseEntities;
using Settings;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class GameObject_Create : MessageBase 
    {
        string mOwner;
        long mGameObjectID;
        string mGameObjectTypeName;

        PropertySpec[] mProperties;

        public GameObject_Create(string owner, KeyCommon.DatabaseEntities.GameObject gameObject) : this ()
        {
            mOwner = owner;
            mProperties = gameObject.GetProperties (false);
            mGameObjectID = gameObject.ID;
            mGameObjectTypeName = gameObject.TypeName;
        }

        public GameObject_Create()
            : base ((int)Enumerations.GameObject_Create)
        { 
        }


        public string Owner { get { return mOwner; } }

        public long GameObjectID { get { return mGameObjectID; } }

        public string GameObjectTypeName { get { return mGameObjectTypeName; } }

        public PropertySpec[] Properties { get { return mProperties; } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            // for client to recreate this gameobject we need type, id, property count
            // and then type and value of each property
            mGameObjectID = buffer.ReadInt64();
            mGameObjectTypeName = buffer.ReadString();
            mOwner = buffer.ReadString();


            int propertyCount = buffer.ReadInt32();
            if (propertyCount == 0) return;

            mProperties = new  PropertySpec[propertyCount];

            for (int j = 0; j < propertyCount; j++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mProperties[j] = spec;
            }

        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(mGameObjectID);
            buffer.Write(mGameObjectTypeName);
            buffer.Write(mOwner);

            
            int length = 0;
            if (mProperties != null) length = mProperties.Length;

            // write the total property count for this node
            buffer.Write(length);

            // iterate through all properties and write them
            for (int i = 0; i < length; i++)
                mProperties[i].Write(buffer);

        }
        #endregion

    }
}
