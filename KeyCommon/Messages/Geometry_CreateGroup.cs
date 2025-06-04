using Lidgren.Network;


namespace KeyCommon.Messages
{

    public class Geometry_CreateGroup : MessageBase
    {
        /// <summary>
        /// ModelID is needed to inform which Appearance to add a GroupAttribute to 
        /// Technically have the GeometryNodeID isn't necessary if we know the ModelID 
        /// but whatever.
        /// </summary>
        public string ModelID;
        public string GeometryNodeID;
        public int GroupType;  // CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE = 0, CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD = 1, CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH = 2
        public int GroupIndex;
        public int GroupClass; // 0 == emitter, 1 == attractor
        public int MaxParticles; // used for GroupClass = 0 or emitter
        public string GroupName;
        public string MeshPath; // only used if GroupClass == 0 && GroupType = MINIMESH
        public Geometry_CreateGroup(string geometryID)
            : this()
        {
            GeometryNodeID = geometryID;
        }

        public Geometry_CreateGroup() : base((int)Enumerations.Geometry_CreateGroup)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModelID = buffer.ReadString();
            GeometryNodeID = buffer.ReadString();
            GroupType = buffer.ReadInt32();
            GroupIndex = buffer.ReadInt32();
            GroupClass = buffer.ReadInt32();
            MaxParticles = buffer.ReadInt32();
            GroupName = buffer.ReadString();
            MeshPath = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModelID);
            buffer.Write(GeometryNodeID);
            buffer.Write(GroupType);
            buffer.Write(GroupIndex);
            buffer.Write(GroupClass);
            buffer.Write(MaxParticles);
            buffer.Write(GroupName);
            buffer.Write(MeshPath);
        }
        #endregion
    }
}
