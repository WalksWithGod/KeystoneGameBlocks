using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    // this is actually more like Geometry_Add to an existing Model.  We do not copy and save any geometry, textures of shaders
    public class Geometry_Add : MessageBase
    {
        public string EntityID; // only needed for Actors that have BonedAnimation[] clips
        public string ModelID;          
        public string ResourcePath;// relative path 

        public bool InteriorContainer;
        public bool LoadXFileAsActor = false; // TODO: need GUI checkbox to indicate if this .x file should be loaded as boned actor
        public bool LoadTextures;
        public bool LoadMaterials;


        public Geometry_Add() : base((int)Enumerations.Geometry_Add)
        { }

        public Geometry_Add(string entityID, string modelID, string resourcePath, bool loadTextures, bool loadMaterials, bool loadXFileAsActor = false)
            : this()
        {
            EntityID = entityID;
            ModelID = modelID;
            ResourcePath = resourcePath;
            LoadTextures = loadTextures;
            LoadMaterials = loadMaterials;
            LoadXFileAsActor = loadXFileAsActor;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            EntityID = buffer.ReadString();
            ModelID = buffer.ReadString();
            ResourcePath = buffer.ReadString();
            
            LoadTextures = buffer.ReadBoolean();
            LoadMaterials = buffer.ReadBoolean();
            LoadXFileAsActor = buffer.ReadBoolean();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(EntityID);
            buffer.Write(ModelID);
            buffer.Write(ResourcePath);
            
            buffer.Write(LoadTextures);
            buffer.Write(LoadMaterials);
            buffer.Write(LoadXFileAsActor);
        }
        #endregion
    }
}
