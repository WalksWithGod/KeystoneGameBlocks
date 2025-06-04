using System;
using Lidgren.Network;
using KeyCommon.Messages;
using System.Diagnostics;
using Amib.Threading;
using Keystone.Types;

using System.Collections.Generic;

namespace KeyCommon.Messages
{
    public struct UniverseCreationParams
    {
        private const float MINIMUM_CLUSTER_DENSITY = .1f;
        public float ClusterDensity;

        public float _percentSingleStarSystems;
        public float _percentBinaryStarSystems;
        public float _percentTrinarySystem;
        public float _percentQuadrupleStarSystems;

        public bool _useRealisticSpectralTypeRatios;
        public float mvarPercentSupergiantI;
        public float mvarPercentGiantII;
        public float mvarPercentGiantIII;
        public float mvarPercentSubgiantIV;
        public float mvarPercentDwarfV;
        public float mvarPercentSubdwarfVI;
        public float mvarPercentWhiteDwarfD;
        public float mvarPercentPulsarP;
        public float mvarPercentNeutronStarN;
        public float mvarPercentBlackHoleB;

        // TODO: add vars and/or code as necessary to make sure that things like a star's age is kept within a range if
        //         more habitable planets are desired so that more such planets will be created (due to the cascading affect it'll have
        //         on star's spectral classes and thus habitable zones)
        public bool GeneratePlanets;
        public bool GenerateMoons;
        public bool GeneratePlanetoidBelts;

        public UniverseCreationParams(float clusterDensity)
        {
            if (clusterDensity < MINIMUM_CLUSTER_DENSITY) clusterDensity = MINIMUM_CLUSTER_DENSITY;
            if (clusterDensity > 1.0f) clusterDensity = 1.0f;


            ClusterDensity = clusterDensity;


            _percentSingleStarSystems = 50f;
            _percentBinaryStarSystems = 33f;
            _percentTrinarySystem = 12.5f;
            _percentQuadrupleStarSystems = 4.5f;

            _useRealisticSpectralTypeRatios = true;
            mvarPercentSupergiantI = 0;
            mvarPercentGiantII = 0;
            mvarPercentGiantIII = 0;
            mvarPercentSubgiantIV = 0;
            mvarPercentDwarfV = 0;
            mvarPercentSubdwarfVI = 0;
            mvarPercentWhiteDwarfD = 0;
            mvarPercentPulsarP = 0;
            mvarPercentNeutronStarN = 0;
            mvarPercentBlackHoleB = 0;

            GeneratePlanets = true;
            GenerateMoons = true;
            GeneratePlanetoidBelts = true;
        }

        #region IRemotableType Members
        public void Read(NetBuffer buffer)
        {
            ClusterDensity = buffer.ReadFloat();

            _percentSingleStarSystems = buffer.ReadFloat();
            _percentBinaryStarSystems = buffer.ReadFloat();
            _percentTrinarySystem = buffer.ReadFloat();
            _percentQuadrupleStarSystems = buffer.ReadFloat();

            GeneratePlanets = buffer.ReadBoolean();
            GeneratePlanetoidBelts = buffer.ReadBoolean();
            GenerateMoons = buffer.ReadBoolean();

        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(ClusterDensity);

            buffer.Write(_percentSingleStarSystems);
            buffer.Write(_percentBinaryStarSystems);
            buffer.Write(_percentTrinarySystem);
            buffer.Write(_percentQuadrupleStarSystems);

            buffer.Write(GeneratePlanets);
            buffer.Write(GeneratePlanetoidBelts);
            buffer.Write(GenerateMoons);
        }
        #endregion 
    }

    public class Scene_NewUniverse : MessageBase
    {
    	[Flags]
        public enum CreationMode : int
        {
            Empty = 0,
            Stars = 1 << 0,
            Planets = 1 << 1,
            Moons = 1 << 2,
            PlanetoidBelts = 1 << 3
        }

        public string FolderName;
        public string VehiclePrefabRelativePath;
		
        public uint RegionsAcross;
        public uint RegionsHigh;
        public uint RegionsDeep;
        public float RegionDiameterX;
        public float RegionDiameterY;
        public float RegionDiameterZ;

        public bool SerializeEmptyZones;
        public bool CreateStarDigest;
        
        public int RandomSeed;
        public CreationMode Mode;

        public UniverseCreationParams mParams;

        // this is only a result after scene is loaded and can
		// be queried in the CommandCompleted threadpool callback.
       
        // change; the result is now saved in command.WorkerProduct;
        // public Scene.Scene ResultScene; 


        public Scene_NewUniverse()
            : base ((int)Enumerations.NewUniverse)
        { }

        public Scene_NewUniverse(int randomSeed, string folderName, uint regionsAcross,
                         uint regionsHigh, uint regionsDeep,
                         float regionDiameterX, float regionDiameterY, float regionDiameterZ,
                        CreationMode mode, float galaxyDensity, uint octreeDepth, bool serializeEmptyZones, bool generateStarDigest, string vehicle)
            : this()
        {
            FolderName = folderName;
            RandomSeed = randomSeed;

            RegionDiameterX = regionDiameterX;
            RegionDiameterY = regionDiameterY;
            RegionDiameterZ = regionDiameterZ;

            RegionsAcross = regionsAcross;
            RegionsHigh = regionsHigh;
            RegionsDeep = regionsDeep;

            SerializeEmptyZones = serializeEmptyZones;
			CreateStarDigest = generateStarDigest;
            VehiclePrefabRelativePath = vehicle;
            Mode = mode;

            // TODO: the params should be passed into constructor
            mParams = new UniverseCreationParams(galaxyDensity); 
            mParams.GenerateMoons = (mode & CreationMode.Moons) != 0;
            mParams.GeneratePlanets = (mode & CreationMode.Planets) != 0;
            mParams.GeneratePlanetoidBelts  = (mode & CreationMode.PlanetoidBelts) != 0;
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RandomSeed = buffer.ReadInt32();

            FolderName = buffer.ReadString();
            
            RegionDiameterX = buffer.ReadFloat();
            RegionDiameterY = buffer.ReadFloat();
            RegionDiameterZ = buffer.ReadFloat();

            RegionsAcross = buffer.ReadUInt32();
            RegionsHigh = buffer.ReadUInt32();
            RegionsDeep = buffer.ReadUInt32();

            SerializeEmptyZones = buffer.ReadBoolean();
			CreateStarDigest = buffer.ReadBoolean();	

            Mode = (CreationMode) buffer.ReadInt32();
            mParams = new UniverseCreationParams();
            mParams.Read(buffer);
            
            VehiclePrefabRelativePath = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RandomSeed);

            buffer.Write(FolderName);

            buffer.Write(RegionDiameterX);
            buffer.Write(RegionDiameterY);
            buffer.Write(RegionDiameterZ);

            buffer.Write(RegionsAcross);
            buffer.Write(RegionsHigh);
            buffer.Write(RegionsDeep);

            buffer.Write(SerializeEmptyZones);
			buffer.Write(CreateStarDigest);

            buffer.Write((int)Mode);
            mParams.Write(buffer);
        	
            buffer.Write(VehiclePrefabRelativePath);
        }
        #endregion
    }
}
