using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    // used for walls, floors and components
    public class Prefab_Insert_Into_Structure : MessageBase
    {
        public string ParentStructureID;
        public string[] NodeIDsToUse;
        public Keystone.Types.Vector3d[] Positions;
        public Keystone.Types.Quaternion[] Rotations; 

        //public Types.Vector3i[] TileLocation; 
        //public byte[] CellRotations; // y axis rotation 0 - 360 ints only hover most things will be limited to 0 - 315 degree in 45 degree increments because our footprints mapping is simpler
        public string RelativeArchivePath;
        public string EntryPath;
        public bool[] Cancel;

        public Prefab_Insert_Into_Structure()
            : base ((int)Enumerations.InsertPrefab_Structure)
        { }

        public Prefab_Insert_Into_Structure(string resourceDescriptor)
            : this(new KeyCommon.IO.ResourceDescriptor(resourceDescriptor))
        { }

        public Prefab_Insert_Into_Structure(KeyCommon.IO.ResourceDescriptor descriptor)
            : this(descriptor.ModName, descriptor.EntryName)
        {
        }

        public Prefab_Insert_Into_Structure(string relativeArchivePath, string pathInArchive)
            : this()
        {
            RelativeArchivePath = relativeArchivePath;
            EntryPath = pathInArchive;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RelativeArchivePath = buffer.ReadString();
            EntryPath = buffer.ReadString();
            ParentStructureID = buffer.ReadString();

            uint count = buffer.ReadUInt32();
            //TileLocation = new Types.Vector3i[count];
            //CellRotations = new byte[count];
            //for (int i = 0; i < count; i++)
            //{
            //    string typename;
            //    TileLocation[i] = (Types.Vector3i)Helpers.ExtensionMethods.ReadType(buffer, out typename);
            //    CellRotations[i] = buffer.ReadByte();
            //}

            Positions = new Keystone.Types.Vector3d[count];
            Rotations = new Keystone.Types.Quaternion[count];
            for (int i = 0; i < count; i++)
            {
                string typename;
                Positions[i] = (Keystone.Types.Vector3d)Helpers.ExtensionMethods.ReadType(buffer, out typename);
                Rotations[i] = (Keystone.Types.Quaternion)Helpers.ExtensionMethods.ReadType(buffer, out typename);
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeArchivePath);
            buffer.Write(EntryPath);
            buffer.Write(ParentStructureID);

            uint count = 0;
            //if (TileLocation != null)
            //    count = (uint)TileLocation.Length;

            //buffer.Write(count);
            //for (int i = 0; i < count; i++)
            //{
            //    Helpers.ExtensionMethods.WriteType(buffer, TileLocation[i]);
            //    buffer.Write(CellRotations[i]);
            //}
            if (Positions != null)
                count = (uint)Positions.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                Helpers.ExtensionMethods.WriteType(buffer, Positions[i]);
                Helpers.ExtensionMethods.WriteType(buffer, Rotations[i]);
            }

        }
        #endregion
    }

    // used for walls, floors and components
    public class Place_Entity_In_EdgeSegment : MessageBase
    {
        public string ParentStructureID;
        public string[] NodeIDsToUse;

        public uint EdgeID;
        public Keystone.Types.Quaternion Rotation;
        //public byte Rotation; // surely rotation is irrelevant here? 
        public string RelativeArchivePath;
        public string EntryPath;
        public bool[] Cancel;

        public Place_Entity_In_EdgeSegment()
            : base ((int)Enumerations.PlaceEntity_EdgeSegment)
        { }

        public Place_Entity_In_EdgeSegment(string resourceDescriptor)
            : this(new KeyCommon.IO.ResourceDescriptor(resourceDescriptor))
        { }

        public Place_Entity_In_EdgeSegment(KeyCommon.IO.ResourceDescriptor descriptor)
            : this(descriptor.ModName, descriptor.EntryName)
        {
        }

        public Place_Entity_In_EdgeSegment(string relativeArchivePath, string pathInArchive)
            : this()
        {
            RelativeArchivePath = relativeArchivePath;
            EntryPath = pathInArchive;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RelativeArchivePath = buffer.ReadString();
            EntryPath = buffer.ReadString();
            ParentStructureID = buffer.ReadString();

            EdgeID = buffer.ReadUInt32();
            //double x, y, z, w;
            //x = buffer.ReadDouble();
            //y = buffer.ReadDouble();
            //z = buffer.ReadDouble();
            //w = buffer.ReadDouble();
            //Rotation = new Keystone.Types.Quaternion(x, y, z, w);
            string typename;
            Rotation = (Keystone.Types.Quaternion)Helpers.ExtensionMethods.ReadType(buffer, out typename);
            //Rotation = buffer.ReadByte();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeArchivePath);
            buffer.Write(EntryPath);
            buffer.Write(ParentStructureID);
            buffer.Write(EdgeID);

            //buffer.Write(Rotation.X);
            //buffer.Write(Rotation.Y);
            //buffer.Write(Rotation.Z);
            //buffer.Write(Rotation.W);
            Helpers.ExtensionMethods.WriteType(buffer, Rotation);
            //buffer.Write(Rotation);
        }
        #endregion
    }
    

}
