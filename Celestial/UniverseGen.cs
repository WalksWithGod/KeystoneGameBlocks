using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using Core.Portals;
using Core.Resource;

namespace Celestial
{
    public class UniverseGen
    {
        private const float MINIMUM_CLUSTER_DENSITY = .1f;
        private float _clusterDensity = 0.75f;
        private float _percentSingleStarSystems = 50f;
        private float _percentBinaryStarSystems = 33f;
        private float _percentTrinarySystem = 12.5f;
        private float _percentQuadrupleStarSystems = 4.5f;
        
        private bool _useRealisticSpectralTypeRatios;
        private float mvarPercentSupergiantI;
        private float mvarPercentGiantII;
        private float mvarPercentGiantIII;
        private float mvarPercentSubgiantIV;
        private float mvarPercentDwarfV;
        private float mvarPercentSubdwarfVI;
        private float mvarPercentWhiteDwarfD;
        private float mvarPercentPulsarP;
        private float mvarPercentNeutronStarN;
        private float mvarPercentBlackHoleB;

        private SystemGen _systemGen;
        private List<System> _systems = new List<System> ();
        private Random _random;
        private int _systemCount = 0;
        private int _seed;

        public delegate void SystemGenerationCompleteHandler(object sender);
        public delegate void SystemGeneratedHandler(object sender, SystemGeneratedEventArgs e);
        
        
        public class SystemGeneratedEventArgs
        {
            public System System;
            public int PositionX;
            public int PositionY;
            public int PositionZ;
            
            // todo: i believe the system's position is a relative position as a System is an entity
            // therefore these eventarg's should return WorldX, WorldY, WorldZ of the region which can be computed directly from
            // i,j,k and the system minimumSystemSeperation.  But all we should return is just the i,j,k because we can also create
            // just one system at a time if we want and return it and that obviously wont result in i,j,k values although for those
            // it's probably ok to not use the threading...
        }
        
        public UniverseGen (int seed)
        {
            _seed = seed;
            _random = new Random(_seed);
            _systemGen = new SystemGen(_random);
        }
        
        // todo: create more overloads where user can force a luminosity class, spectral class, spectral subtypes, etc.
        public System CreateSystem(float positionX, float positionY, float positionZ)
        {
            uint numberofStars = GetNumberofStars();
            return CreateSystem(numberofStars, positionX, positionY, positionZ);
        }
        
        public System CreateSystem(uint numberofStars, float positionX, float positionY, float positionZ) 
        {
            return _systemGen.GenerateSystem(positionX, positionY, positionZ, numberofStars);
        }

        public object CreateUniverse (object obj)
        {
            if (!(obj is UniverseCreationParams)) throw new ArgumentOutOfRangeException();
            
            CreateUniverse(((UniverseCreationParams) obj).Regions,
                               ((UniverseCreationParams)obj).ClusterDensity,
                               ((UniverseCreationParams)obj).MinimumSystemSeperation,
                               ((UniverseCreationParams)obj).SystemGeneratedCallback,
                               ((UniverseCreationParams)obj).SystemGenerationCompleteCallback);
            return null;
        }
        
        public struct UniverseCreationParams
        {
            public Region[,,] Regions;
            public float ClusterDensity;
            public float MinimumSystemSeperation;
            public SystemGeneratedHandler SystemGeneratedCallback;
            public SystemGenerationCompleteHandler SystemGenerationCompleteCallback;

            public UniverseCreationParams(Region[, ,] regions, float clusterDensity, float minimumSeperation, SystemGeneratedHandler sgCB, SystemGenerationCompleteHandler sgcCB)
            {
                Regions = regions;
                ClusterDensity = clusterDensity;
                MinimumSystemSeperation = minimumSeperation;
                SystemGeneratedCallback = sgCB;
                SystemGenerationCompleteCallback = sgcCB;
            }
        }
        
        public void CreateUniverse(Region[,,] regions, float clusterDensity, float minimumSystemSeperation, SystemGeneratedHandler sgCB, SystemGenerationCompleteHandler sgcCB) 
        {

            if (clusterDensity <= MINIMUM_CLUSTER_DENSITY) throw new ArgumentOutOfRangeException(string.Format("Cluster density must be > {0}", MINIMUM_CLUSTER_DENSITY));
            if (clusterDensity > 1.0f) clusterDensity = 1.0f;

            SystemGeneratedHandler OnSystemGenerated = sgCB;
            SystemGenerationCompleteHandler OnSystemGenerationComplete = sgcCB ;
            _clusterDensity = clusterDensity;
            
            // this routine essentially creates a cube shaped galaxy.  It can be made
            // to be more irregular by haing the Z level go to rnd(min) to rnd(maxDiameter)
            // it takes diameter of our galaxy and then begings to plot stars
            // Basically it starts at coordinates 1,1,1 and then goes to 1,1,2 then 1,1,3, etc
            // til it eventually reaches Diameter,Diameter,Diameter
            // so the maximum number of stars we can have in our system is Diameter^3
            // The actual diameter of our galaxy in lightyears is the MinimumSystemSeperation * Diameter.
            // So if the minimum seperation (as set by the user) is 2 light years then
            // our diamter in light years is 2 * Diameter.  So if the diameter is 10 then our cube is
            // actually 20 light years across and can hold at most 1000 (or 10^3) star systems.
            // Incidentally, Generating 1000 star systems as detailed as we are doing will take quite
            // a while, but fortunately its jsut for creating the initial map.

            // //note that the user is setting the minimum distance between systems in LightYears
            //   but we calculate all of our locations in AU so we must convert
            System newsystem;
            int width = regions.GetUpperBound(0);
            int height = regions.GetUpperBound(1);
            int depth = regions.GetUpperBound(2);

            for (int i = 0; i <= width; i++)
            {
                for (int j = 0; j <= height; j++)
                {
                    for (int k = 0; k <= depth; k++) 
                    {
                        if (ShouldSystemGoHere(clusterDensity))
                         {
                             // determine how many stars we should have in the system
                            uint starCount = GetNumberofStars() ;
                             
                             // todo: here rather than add systems to a list, we should create the Regions, save them to disk
                             // and record the filenames and array positions in a SceneInfo struct that is returned.
                             // todo: in .GenerateSystem i dont believe im properly computing the system's boundingbox, but should it
                             //       be a moving box that updates with planet/star positions?  
                             newsystem = _systemGen.GenerateSystem(i * minimumSystemSeperation, j * minimumSystemSeperation,
                                                                     k * minimumSystemSeperation, starCount);
                             regions[i, j, k].AddChild(newsystem);
                         }
                        else
                        {
                            newsystem = null;
                        }
                         SystemGeneratedEventArgs args = new SystemGeneratedEventArgs ();
                         args.System = newsystem;
                         args.PositionX = i;
                         args.PositionY = j;
                         args.PositionZ = k;
                         if (OnSystemGenerated != null) OnSystemGenerated.Invoke(this, args);
                    }
                }
            }

            if (OnSystemGenerationComplete != null) OnSystemGenerationComplete.Invoke(this);
        }

        /// this routine determines if a new system is created
        ///  at a particular point in space.  It uses the
        ///  Density setting to determine the odds that a system is created
        ///  or not.  The higher the Density, the better the chance that a system
        ///  will be created.
        private bool ShouldSystemGoHere(float clusterDensity)
        {
            if (_random.NextDouble() <= clusterDensity)
            {
                _systemCount++;
                return true;
            }
            return false;
        }

        /// this function returns the number of companion stars are created based
        /// on the users settings.  Possible outcomes are 1, 2, 3 or 4 (for now)
        private uint GetNumberofStars()
        {
            double d = _random.NextDouble() * 100;
            uint result = 0;

            //  Generate random byte between 1 and 100.
            if (d <= _percentSingleStarSystems)
                result = 1;
            else if (d <= _percentSingleStarSystems + _percentBinaryStarSystems)
                result = 2;
            else if (d <= _percentSingleStarSystems + _percentBinaryStarSystems + _percentTrinarySystem)
                result = 3;
            else
            {
                result = 4;
                Debug.Assert(d >= 0 && d <= _percentSingleStarSystems + _percentBinaryStarSystems + _percentTrinarySystem +
                             _percentQuadrupleStarSystems);
                Debug.Assert(_percentSingleStarSystems + _percentBinaryStarSystems + _percentTrinarySystem +
                             _percentQuadrupleStarSystems == 100f);
            }
            return result;
        }
    }
    //// //First clear our Systems, Stars and Worlds tables
    //db.Execute; "DELETE * FROM StarSystems";
    //db.Execute; "DELETE * FROM Stars";
    //db.Execute; "DELETE * FROM Worlds";
    //db.Execute; "DELETE * FROM Moons";

    //for (int i = 1; (i <= UBound(arrSystems)); i++) 
    //{
    //    db.Execute; ("INSERT INTO StarSystems(SystemID,Name,xPos,yPos,zPos) VALUES (" 
    //                + (arrSystems[i].ID + ("," + (''' 
    //                + (arrSystems[i].Name + (''' + ("," 
    //                + (arrSystems[i].xPos + (", " 
    //                + (arrSystems[i].yPos + (", " 
    //                + (arrSystems[i].zPos + " )"))))))))))));
    //    DoEvents;
    //}
    //for (int i = 1; (i <= UBound(arrStars)); i++) 
    //{
    //    db.Execute;
    //    (@"INSERT INTO Stars(StarID,Name,SystemID, xPos,yPos,zPos, DistanceFromSystemCenter,LuminosityClass, SpectralType, SpectralSubType, orbitalEccentricity, MinSeperation, MaxSeperation,Temperature,Luminosity, Mass, Radius, Age, InnerLimitDistance,OuterLimitDistance,LifeZoneInnerEdge,LifeZoneOuterEdge,SnowLine)" + (" VALUES (" 
    //                + (arrStars[i].ID + ("," + (''' 
    //                + (arrSystems[i].Name + (''' + ("," 
    //                + (arrStars[i].SystemID + ("," 
    //                + (arrStars[i].xPos + (", " 
    //                + (arrStars[i].yPos + (", " 
    //                + (arrStars[i].zPos + (", " 
    //                + (arrStars[i].DistanceFromSystemCenter + (", " 
    //                + (arrStars[i].LuminosityClass + (", " 
    //                + (arrStars[i].SpectralType + (", " 
    //                + (arrStars[i].SpectralSubType + (", " 
    //                + (arrStars[i].OrbitalEccentricity + (", " 
    //                + (arrStars[i].MinSeperation + (", " 
    //                + (arrStars[i].MaxSeperation + (", " 
    //                + (arrStars[i].Temperature + (", " 
    //                + (arrStars[i].Luminosity + (", " 
    //                + (arrStars[i].Mass + (", " 
    //                + (arrStars[i].Radius + (", " 
    //                + (arrStars[i].Age + (", " 
    //                + (arrStars[i].InnerLimitDistance + (", " 
    //                + (arrStars[i].OuterLimitDistance + (", " 
    //                + (arrStars[i].LifeZoneInnerEdge + (", " 
    //                + (arrStars[i].LifeZoneOuterEdge + (", " 
    //                + (arrStars[i].SnowLine + ")")))))))))))))))))))))))))))))))))))))))))))))))));
    //    DoEvents;
    //}
    //for (int i = 1; (i <= UBound(arrPlanets)); i++) 
    //{
    //    db.Execute;
    //    (@"INSERT INTO Worlds(WorldID,Name,WorldType,StarID,SystemID,xPos,yPos,zPos, ZoneType,Size , BiosphereType, Diameter, DENSITY, Mass, SurfaceGravity, OrbitalRadius, OrbitalEccentricity, OrbitalPeriod, AtmospherePressure, Hydrographics, NativeEcosphere, AtmosphereComposition, PrimaryContaminant, Albedo, GreenHouseFactor, AverageSurfaceTemperature ) VALUES (" 
    //                + (arrPlanets[i].ID + ("," + (''' 
    //                + (arrPlanets[i].Name + (''' + ("," 
    //                + (arrPlanets[i].WorldType + (", " 
    //                + (arrPlanets[i].StarID + (", " 
    //                + (arrPlanets[i].SystemID + ("," 
    //                + (arrPlanets[i].xPos + (", " 
    //                + (arrPlanets[i].yPos + (", " 
    //                + (arrPlanets[i].zPos + ("," 
    //                + (arrPlanets[i].ZoneType + ("," 
    //                + (arrPlanets[i].Size + ("," 
    //                + (arrPlanets[i].BiosphereType + ("," 
    //                + (arrPlanets[i].Diameter + ("," 
    //                + (arrPlanets[i].DENSITY + ("," 
    //                + (arrPlanets[i].Mass + ("," 
    //                + (arrPlanets[i].SurfaceGravity + ("," 
    //                + (arrPlanets[i].OrbitalRadius + ("," 
    //                + (arrPlanets[i].OrbitalEccentricity + ("," 
    //                + (arrPlanets[i].OrbitalPeriod + ("," 
    //                + (arrPlanets[i].AtmospherePressure + ("," 
    //                + (arrPlanets[i].Hydrographics + ("," 
    //                + (arrPlanets[i].NativeEcosphere + ("," 
    //                + (arrPlanets[i].AtmosphereComposition + ("," 
    //                + (arrPlanets[i].PrimaryContaminant + ("," 
    //                + (arrPlanets[i].Albedo + ("," 
    //                + (arrPlanets[i].GreenHouseFactor + ("," 
    //                + (arrPlanets[i].AverageSurfaceTemperature + " )"))))))))))))))))))))))))))))))))))))))))))))))))))))));
    //    DoEvents;
    //}
    //for (int i = 1; (i <= UBound(arrMoons)); i++) 
    //{
    //    db.Execute;
    //    (@"INSERT INTO Moons(MoonID,Name,PlanetID,SystemID,xPos,yPos,zPos, ZoneType,Size,MoonType, MoonClass, Diameter, DENSITY, Mass, SurfaceGravity, OrbitalRadius, OrbitalEccentricity, OrbitalPeriod, AtmospherePressure, Hydrographics, NativeEcosphere, AtmosphereComposition, PrimaryContaminant, Albedo, GreenHouseFactor, AverageSurfaceTemperature ) VALUES (" 
    //                + (arrMoons[i].ID + ("," + (''' 
    //                + (arrMoons[i].Name + (''' + (", " 
    //                + (arrMoons[i].PlanetID + (", " 
    //                + (arrMoons[i].SystemID + ("," 
    //                + (arrMoons[i].xPos + (", " 
    //                + (arrMoons[i].yPos + (", " 
    //                + (arrMoons[i].zPos + ("," 
    //                + (arrMoons[i].ZoneType + ("," 
    //                + (arrMoons[i].Size + ("," 
    //                + (arrMoons[i].MoonType + ("," 
    //                + (arrMoons[i].MoonClass + ("," 
    //                + (arrMoons[i].Diameter + ("," 
    //                + (arrMoons[i].DENSITY + ("," 
    //                + (arrMoons[i].Mass + ("," 
    //                + (arrMoons[i].SurfaceGravity + ("," 
    //                + (arrMoons[i].OrbitalRadius + ("," 
    //                + (arrMoons[i].OrbitalEccentricity + ("," 
    //                + (arrMoons[i].OrbitalPeriod + ("," 
    //                + (arrMoons[i].AtmospherePressure + ("," 
    //                + (arrMoons[i].Hydrographics + ("," 
    //                + (arrMoons[i].NativeEcosphere + ("," 
    //                + (arrMoons[i].AtmosphereComposition + ("," 
    //                + (arrMoons[i].PrimaryContaminant + ("," 
    //                + (arrMoons[i].Albedo + ("," 
    //                + (arrMoons[i].GreenHouseFactor + ("," 
    //                + (arrMoons[i].AverageSurfaceTemperature + " )"))))))))))))))))))))))))))))))))))))))))))))))))))))));
    //    Debug.Print;
    //    (arrMoons[i].ID + (", " + (''' 
    //                + (arrMoons[i].Name + (''' + (", " 
    //                + (arrMoons[i].PlanetID + (", " 
    //                + (arrMoons[i].SystemID + (", " 
    //                + (arrMoons[i].xPos + (", " 
    //                + (arrMoons[i].yPos + (", " 
    //                + (arrMoons[i].zPos + (", " 
    //                + (arrMoons[i].ZoneType + (", " 
    //                + (arrMoons[i].Size + (", " 
    //                + (arrMoons[i].MoonType + (", " 
    //                + (arrMoons[i].MoonClass + (", " + arrMoons[i].Diameter))))))))))))))))))))))));
    //    ("" + ("," 
    //                + (arrMoons[i].DENSITY + ("," 
    //                + (arrMoons[i].Mass + ("," 
    //                + (arrMoons[i].SurfaceGravity + ("," 
    //                + (arrMoons[i].OrbitalRadius + ("," 
    //                + (arrMoons[i].OrbitalEccentricity + ("," 
    //                + (arrMoons[i].OrbitalPeriod + ("," 
    //                + (arrMoons[i].AtmospherePressure + ("," 
    //                + (arrMoons[i].Hydrographics + ("," 
    //                + (arrMoons[i].NativeEcosphere + ("," 
    //                + (arrMoons[i].AtmosphereComposition + ("," 
    //                + (arrMoons[i].PrimaryContaminant + ("," 
    //                + (arrMoons[i].Albedo + ("," 
    //                + (arrMoons[i].GreenHouseFactor + ("," + arrMoons[i].AverageSurfaceTemperature))))))))))))))))))))))))))));
    //}
}
