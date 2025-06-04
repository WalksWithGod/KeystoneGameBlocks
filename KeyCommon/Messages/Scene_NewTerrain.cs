using System;
using Lidgren.Network;
using KeyCommon.Messages;
using System.Diagnostics;
using Amib.Threading;
using Keystone.Types;
using System.Collections.Generic;

namespace KeyCommon.Messages
{
    public class Scene_NewTerrain : MessageBase
    {
    	[Flags]
        public enum CreationMode : int
        {
            None = 0,
            MultiZone = 1 << 0,
            DefaultTerrain = 1 << 1,
            Water = 1 << 2,
            DefaultTerrain_Water = 1 << 3,
            SerializeEmptyZones = 1 << 4
            	
        }

        // TODO: why not just add "multizone" as part of CreationMode flags
        //       - along with o
        public string FolderName;

        public uint RegionsAcross;
        public uint RegionsHigh;
        public uint RegionsDeep;
        
        public uint RegionResolutionX;
        public uint RegionResolutionY;
        public uint RegionResolutionZ;
        
        public uint TerrainTileCountY;
        
        public uint OctreeDepth;
        
        public float RegionDiameterX;
        public float RegionDiameterY;
        public float RegionDiameterZ;


        public float TileSizeX;
        public float TileSizeY;
        public float TileSizeZ;
        
        public int MinimumFloor;
        public int MaximumFloor;
        
        public bool SerializeEmptyZones;
                
        public int RandomSeed;
        public CreationMode Mode;

        public UniverseCreationParams mParams;

        //change: result is now stored in command.WorkerProduct;
        //public Scene.Scene ResultScene; // this is only a result after scene is loaded
        // and can be queried in the CommandCompleted threadpool 
        // callback

        public Scene_NewTerrain()
            : base ((int)Enumerations.NewTerrainScene)
        { }

        public Scene_NewTerrain(string folderName,  
                                uint regionsAcross, uint regionsHigh, uint regionsDeep,
                                float tileSizeX, float tileSizeY, float tileSizeZ,
                                uint regionResolutionX, uint regionResolutionY, uint regionResolutionZ,
                                uint terrainTileCountY, int minimumFloor, int maximumFloor,
                        CreationMode mode, uint octreeDepth, bool serializeEmptyZones)
            : this()
        {
            FolderName = folderName;

            OctreeDepth = octreeDepth;
            RegionsAcross = regionsAcross;
            RegionsHigh = regionsHigh;
            RegionsDeep = regionsDeep;

            RegionResolutionX = regionResolutionX;
            RegionResolutionY = regionResolutionY;
            RegionResolutionZ = regionResolutionZ;
            
            TerrainTileCountY = terrainTileCountY;
            MinimumFloor = minimumFloor;
            MaximumFloor = maximumFloor;
            
            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
            TileSizeZ = tileSizeZ;
            
            RegionDiameterX = RegionResolutionX * TileSizeX;
            RegionDiameterY = RegionResolutionY * TileSizeY;
            RegionDiameterZ = RegionResolutionZ * TileSizeZ;
            
            SerializeEmptyZones = serializeEmptyZones;

            Mode = mode;
            
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RandomSeed = buffer.ReadInt32();
            FolderName = buffer.ReadString();

            TileSizeX = buffer.ReadFloat();
            TileSizeY = buffer.ReadFloat();
            TileSizeZ = buffer.ReadFloat();

            RegionsAcross = buffer.ReadUInt32();
            RegionsHigh = buffer.ReadUInt32();
            RegionsDeep = buffer.ReadUInt32();

            RegionResolutionX = buffer.ReadUInt32();
            RegionResolutionY = buffer.ReadUInt32();
            RegionResolutionZ = buffer.ReadUInt32();
            
            TerrainTileCountY = buffer.ReadUInt32();
            
            MinimumFloor = buffer.ReadInt32();
            MaximumFloor = buffer.ReadInt32();
            
            RegionDiameterX = RegionResolutionX * TileSizeX;
            RegionDiameterY = RegionResolutionY * TileSizeY;
            RegionDiameterZ = RegionResolutionZ * TileSizeZ;
            
            OctreeDepth = buffer.ReadUInt32();
            SerializeEmptyZones = buffer.ReadBoolean();

            Mode = (CreationMode) buffer.ReadInt32();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RandomSeed);
            buffer.Write(FolderName);

            buffer.Write(TileSizeX);
            buffer.Write(TileSizeY);
            buffer.Write(TileSizeZ);

            buffer.Write(RegionsAcross);
            buffer.Write(RegionsHigh);
            buffer.Write(RegionsDeep);

            buffer.Write(RegionResolutionX);
            buffer.Write(RegionResolutionY);
            buffer.Write(RegionResolutionZ);
            
            buffer.Write (TerrainTileCountY);
            
            buffer.Write(MinimumFloor);
            buffer.Write(MaximumFloor);
            	
            buffer.Write (OctreeDepth);
            buffer.Write(SerializeEmptyZones);

            buffer.Write((int)Mode);
        }
        #endregion
    }
}
