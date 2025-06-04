using Lidgren.Network;

namespace KeyCommon.Messages
{

    public class Geometry_RemoveGroup : MessageBase
    {
        /// <summary>
        /// ModelID is needed to inform which Appearance to REMOVE a GroupAttribute from.
        /// Technically have the GeometryNodeID isn't necessary if we know the ModelID 
        /// but whatever.
        /// </summary>
        public string ModelID;
        public string GeometryNodeID;
        public int GroupIndex;
        public int GroupClass;


        public Geometry_RemoveGroup(string geometryID)
            : this()
        {
            GeometryNodeID = geometryID;
        }

        public Geometry_RemoveGroup() : base((int)Enumerations.Geometry_RemoveGroup)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModelID = buffer.ReadString();
            GeometryNodeID = buffer.ReadString();
            GroupIndex = buffer.ReadInt32();
            GroupClass = buffer.ReadInt32();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModelID);
            buffer.Write(GeometryNodeID);
            buffer.Write(GroupIndex);
            buffer.Write(GroupClass);
        }
        #endregion
    }
}
