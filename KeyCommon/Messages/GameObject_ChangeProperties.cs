using KeyCommon.Messages;
using Lidgren.Network;
using Settings;
using System;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class GameObject_ChangeProperties : MessageBase
    {
        string mOwner;
        long mGameObjectID;
        string mGameObjectTypeName;
        int mGameObjectIndex;     // some Entity gameObjects are stored as an array such as NavPoints.  This index tells us which array element we want to changeProperties for
        PropertySpec[] mProperties;

        public GameObject_ChangeProperties(string owner, KeyCommon.DatabaseEntities.GameObject gameObject) : this ()
        {
            mOwner = owner;
            mProperties = gameObject.GetProperties(false);
            mGameObjectID = gameObject.ID;
            mGameObjectTypeName = gameObject.TypeName;
            mGameObjectIndex = -1;
        }

        public GameObject_ChangeProperties()
            : base ((int)Enumerations.GameObject_ChangeProperties)
        {
        }


        public string Owner { get { return mOwner; } }

        public long GameObjectID { get { return mGameObjectID; } }

        public string GameObjectTypeName { get { return mGameObjectTypeName; } }

        public int GameObjectIndex { get { return mGameObjectIndex;} set { mGameObjectIndex = value; } }

        public PropertySpec[] Properties { get { return mProperties; } }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            // for client to recreate this gameobject we need type, id, property count
            // and then type and value of each property
            mGameObjectID = buffer.ReadInt64();
            mGameObjectTypeName = buffer.ReadString();
            mGameObjectIndex = buffer.ReadInt32();
            mOwner = buffer.ReadString();


            int propertyCount = buffer.ReadInt32();
            if (propertyCount == 0) return;

            mProperties = new PropertySpec[propertyCount];

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
            buffer.Write(mGameObjectIndex);
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