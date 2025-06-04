using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    // TODO: obsolete?  
    public class Place_Wall_Into_CelledRegion : MessageBase
    {
        public string ParentCelledRegionID;

        public uint[] EdgeIndices;  // for walls, edge indices alone determine position and rotation
        public string RelativeArchivePath;
        public string PathInArchive;


        public Place_Wall_Into_CelledRegion()
            : base((int)Enumerations.PlaceWall_CelledRegion)
        { }

        public Place_Wall_Into_CelledRegion(string resourceDescriptor)
            : this(new KeyCommon.IO.ResourceDescriptor(resourceDescriptor))
        { }

        public Place_Wall_Into_CelledRegion(KeyCommon.IO.ResourceDescriptor descriptor)
            : this(descriptor.ModName, descriptor.EntryName)
        {
        }

        public Place_Wall_Into_CelledRegion(string relativeArchivePath, string pathInArchive)
            : this()
        {
            RelativeArchivePath = relativeArchivePath;
            PathInArchive = pathInArchive;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RelativeArchivePath = buffer.ReadString();
            PathInArchive = buffer.ReadString();
            ParentCelledRegionID = buffer.ReadString();

            uint count = buffer.ReadUInt32();
            EdgeIndices = new uint[count];
            for (int i = 0; i < count; i++)
                EdgeIndices[i] = buffer.ReadUInt32();

            //ComponentType = (ComponentType)buffer.ReadByte();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeArchivePath);
            buffer.Write(PathInArchive);
            buffer.Write(ParentCelledRegionID);

            uint count = 0;
            if (EdgeIndices != null)
                count = (uint)EdgeIndices.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
                buffer.Write(EdgeIndices[i]);

            //buffer.Write((byte)ComponentType);

        }
        #endregion
    }
}
