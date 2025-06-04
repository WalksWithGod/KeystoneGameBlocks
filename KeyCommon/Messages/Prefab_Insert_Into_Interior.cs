using System;
using KeyCommon.Messages;
using Lidgren.Network;
using Keystone.Types;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Used for placing Components within Cells or Edges (eg: doors, railings, etc)
    /// </summary>
    public class Prefab_Insert_Into_Interior : MessageBase
    {
        public uint Index; // TODO: the Index of the cells or edges to be modified should be an array.  For ComponentType.EdgeComponent, this is the EdgeID
        public ComponentType ComponentType;
        public Vector3d Position;
        public Quaternion Rotation;
       
        public string ParentID;
        public string[] NodeIDsToUse;

        public string ModName;
        public string EntryPath; // todo: we are not sychronizing IDs between server and client.  

        public bool Cancel;


        public Prefab_Insert_Into_Interior(string resourceDescriptor)
            : this(new KeyCommon.IO.ResourceDescriptor(resourceDescriptor))
        { }

        public Prefab_Insert_Into_Interior(KeyCommon.IO.ResourceDescriptor descriptor)
            : this(descriptor.ModName, descriptor.EntryName)
        {
        }

        public Prefab_Insert_Into_Interior(string modName, string entryName)
            : this()
        {
            ModName = modName;
            EntryPath = entryName;
        }

        public Prefab_Insert_Into_Interior()
            : base ((int)Enumerations.InsertPrefab_Interior) 
        { }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModName = buffer.ReadString();
            EntryPath = buffer.ReadString();
            ParentID = buffer.ReadString();
            Index = buffer.ReadUInt32();
            Position.x = buffer.ReadDouble();
            Position.y = buffer.ReadDouble();
            Position.z = buffer.ReadDouble();
            Rotation = new Quaternion();
            Rotation.X = buffer.ReadDouble();
            Rotation.Y = buffer.ReadDouble();
            Rotation.Z = buffer.ReadDouble();
            Rotation.W = buffer.ReadDouble();
            ComponentType = (ComponentType)buffer.ReadByte();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModName);
            buffer.Write(EntryPath);
            buffer.Write(ParentID);
            buffer.Write(Index);
            buffer.Write(Position.x);
            buffer.Write(Position.y);
            buffer.Write(Position.z);
            buffer.Write(Rotation.X);
            buffer.Write(Rotation.Y);
            buffer.Write(Rotation.Z);
            buffer.Write(Rotation.W);
            buffer.Write((byte)ComponentType);

        }
        #endregion
    }

}
