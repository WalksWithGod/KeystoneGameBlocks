//using System;
//using System.Collections.Generic;
//using Keystone.IO;
//using Keystone.Scene;
//using System.Diagnostics;
//using Amib.Threading;
//using Keystone.Celestial;
//using Keystone.Portals ;
//using Keystone.Types;
//using Keystone.Resource;
//using KeyCommon.Commands;

// OBSOLETE - Commands now moved outside of Commands and into thread safe worker functions in FormMain.Commands
//namespace Keystone.Commands
//{
//    public class GenerateUniverse : NetCommandBase
//    {
//        private const float MINIMUM_CLUSTER_DENSITY = .1f;

//        private UniverseCreationParams mParams;
//        private GenerateStellarSystem _systemGen;
//        private GenerateWorld _worldGen;
//        private GenerateMoon _moonGen;

//        private Root mRoot;
//        private List<StellarSystem> _systems = new List<StellarSystem>();
//        private Random _random;
//        private int _systemCount = 0;
//        private int _seed;
//        private string mFileName;
//        private string mName;
//        private bool mCreateEmptyUniverse;
//        public SceneInfo mSceneInfo;

//        public GenerateUniverse(int seed, string name,  uint regionsAcross,
//                         uint regionsHigh, uint regionsDeep,
//                         float regionDiameterX, float regionDiameterY, float regionDiameterZ, 
//                        bool createEmptyUniverse,
//                         uint octreeDepth)
//            : base(Enumerations.GenerateUniverse)
//        {
//            _seed = seed;
//            _random = new Random(_seed);
//            _systemGen = new GenerateStellarSystem(_random);
//            _worldGen = new GenerateWorld(_random);
//            _moonGen = new GenerateMoon(_random);
//            mName = name;
//            mCreateEmptyUniverse = createEmptyUniverse;
//            mRoot = new Root("root", regionsAcross, regionsHigh, regionsDeep,
//                                           regionDiameterX, regionDiameterY, regionDiameterZ, 0);

//            // TODO: the params should be passed in
//            mParams = new UniverseCreationParams(1.0f, 10);
//        }

//        public string FileName { get { return mFileName; } }
//        public override void Execute()
//        {
//            _state = KeyCommon.Commands.State.ExecuteProcessing;
//            if (UnExecutePath)
//                this.Execute(new WorkItemCallback(UnExecuteWorker), new PostExecuteWorkItemCallback(ExecuteCompletedHandler));
//            else
//                this.Execute(new WorkItemCallback(ExecuteWorker), new PostExecuteWorkItemCallback(ExecuteCompletedHandler));
//        }

//        public override void EndExecute()
//        {

//            if (UnExecutePath)
//            {

//            }
//            else
//            { 

//            }

//            base.EndExecute();

//        }

//        private object UnExecuteWorker(object state)
//        {
//            throw new NotImplementedException();
//        }

//        private object ExecuteWorker(object state)
//        {
//            //try
//            //{

//            //// TODO: new strategy, first we gen the stellar systems with the stars
//            //// we modify the code to be able to generate orbits for a system and it's stars on demand.
//            //// Now there is one consideration, the idea of our gen was to be able to write to disk immediately
//            //// but, what we're going to try instead is when each system completes, we'll call a local callback here
//            //// and then we'll gen the orbits and worlds here.  Why?  Because I dont want "world gen"  and moon gen
//            //// inside of GenerateStellarSystems

//            ////if (parameters._clusterDensity  <= MINIMUM_CLUSTER_DENSITY)
//            ////    throw new ArgumentOutOfRangeException(string.Format("Cluster density must be > {0}",
//            ////                                                        MINIMUM_CLUSTER_DENSITY));
//            ////if (parameters._clusterDensity > 1.0f) parameters._clusterDensity = 1.0f;

//            //    mFileName = GetTempFile();
//            //    XMLDatabase xmldb = new XMLDatabase();
//            //    xmldb.Create(mFileName);
//            //    Trace.WriteLine("New scene created at temporary location " + mFileName);
//            //    SceneWriter writer = new SceneWriter(xmldb, Core._Core.ThreadPool, null, null);
                
//            //    mSceneInfo = new SceneInfo(mFileName, new string[] { "Root" });
//            //    // nodes are added to the Repository when they are constructed but only ref count is incremented
//            //    // if it's manually done or added to the scene.  Here we do it manually so that on DecrementRef it
//            //    // will automatically be removed from Repository
//            //    Repository.IncrementRef(null, mSceneInfo);
//            //    Repository.DecrementRef(null, mSceneInfo);

//            //    writer.WriteSychronous(mSceneInfo, false);

//            //    // nodes are added to the Repository when they are constructed but only ref count is incremented
//            //    // if it's manually done or added to the scene.  Here we do it manually so that on DecrementRef it
//            //    // will automatically be removed from Repository
//            //    Repository.IncrementRef(null, mRoot);
//            //    Repository.DecrementRef(null, mRoot);

//            //    writer.WriteSychronous(mRoot, false);

//            //    // first create and write to disk the regions we will need to pass to the universe gen
//            //    Region[, ,] regions = Region.Create(mRoot, writer);

//            //    // this routine essentially creates a cube shaped galaxy.  It can be made
//            //    // to be more irregular by haing the Z level go to rnd(min) to rnd(maxDiameter)
//            //    // it takes diameter of our galaxy and then begings to plot stars
//            //    // Basically it starts at coordinates 1,1,1 and then goes to 1,1,2 then 1,1,3, etc
//            //    // til it eventually reaches Diameter,Diameter,Diameter
//            //    // so the maximum number of stars we can have in our system is Diameter^3
//            //    // The actual diameter of our galaxy in lightyears is the MinimumSystemSeperation * Diameter.
//            //    // So if the minimum seperation (as set by the user) is 2 light years then
//            //    // our diamter in light years is 2 * Diameter.  So if the diameter is 10 then our cube is
//            //    // actually 20 light years across and can hold at most 1000 (or 10^3) star systems.
//            //    // Incidentally, Generating 1000 star systems as detailed as we are doing will take quite
//            //    // a while, but fortunately its jsut for creating the initial map.

//            //    // //note that the user is setting the minimum distance between systems in LightYears
//            //    //   but we calculate all of our locations in AU so we must convert
//            //    StellarSystem newsystem;
//            //    int width = regions.GetUpperBound(0);
//            //    int height = regions.GetUpperBound(1);
//            //    int depth = regions.GetUpperBound(2);

//            //    for (int i = 0; i <= width; i++)
//            //    {
//            //        for (int j = 0; j <= height; j++)
//            //        {
//            //            for (int k = 0; k <= depth; k++)
//            //            {
//            //                if (!mCreateEmptyUniverse)
//            //                {
//            //                    // systems gets added and written to disk and then the region is unloaded and we move to the next
//            //                    if (ShouldSystemGoHere(mParams._clusterDensity))
//            //                    {
//            //                        // determine how many stars we should have in the system
//            //                        uint starCount = GetNumberofStars();
//            //                        newsystem = _systemGen.GenerateSystem(i * mParams.MinimumSystemSeperation,
//            //                                                            j * mParams.MinimumSystemSeperation,
//            //                                                              k * mParams.MinimumSystemSeperation, starCount);
//            //                        newsystem.Translation = new Vector3d(i, j, k);
//            //                        OnSystemGenerated(newsystem);
//            //                        regions[i, j, k].AddChild(newsystem);
//            //                    }
//            //                    else
//            //                    {
//            //                        newsystem = null;
//            //                    }
//            //                }
//            //                writer.WriteSychronous (regions[i, j, k], false);
//            //                 regions[i, j, k].RemoveChildren();
//            //                 Repository.Remove(regions[i, j, k]);
//            //                 //Repository.IncrementRef(null, regions[i, j, k]); // ok to use null because we never added these regions to any parent entity
//            //                 //Repository.DecrementRef(null, regions[i, j, k]);
//            //            }
//            //        }
//            //    }

//            //    xmldb.SaveAllChanges();
//            //    xmldb.Dispose();

//            //    _state = KeyCommon.Commands.State.ExecuteProcessing;
//            //    return null;
//            //}
//            //catch (Exception ex)
//            //{
//            //    _state = KeyCommon.Commands.State.ExecuteError;
//            //    return null;
//            //}
//                return null;
//        }


//        /// this routine determines if a new system is created
//        ///  at a particular point in space.  It uses the
//        ///  Density setting to determine the odds that a system is created
//        ///  or not.  The higher the Density, the better the chance that a system
//        ///  will be created.
//        private bool ShouldSystemGoHere(float clusterDensity)
//        {
//            if (_random.NextDouble() <= clusterDensity)
//            {
//                _systemCount++;
//                return true;
//            }
//            return false;
//        }

//        /// this function returns the number of companion stars are created based
//        /// on the users settings.  Possible outcomes are 1, 2, 3 or 4 (for now)
//        private uint GetNumberofStars()
//        {
//            double d = _random.NextDouble()*100;
//            uint result = 0;

//            //  Generate random byte between 1 and 100.
//            if (d <= mParams._percentSingleStarSystems)
//                result = 1;
//            else if (d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems)
//                result = 2;
//            else if (d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem)
//                result = 3;
//            else
//            {
//                result = 4;
//                Debug.Assert(d >= 0 &&
//                             d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem +
//                                  mParams._percentQuadrupleStarSystems);
//                Debug.Assert(mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem +
//                             mParams._percentQuadrupleStarSystems == 100f);
//            }
//            return result;
//        }

//        /// <summary>
//        /// After each system is created and before the region is written to xml, this is called to generate the orbital
//        /// info for worlds that will be in the system.
//        /// </summary>
//        /// <param name="system"></param>
//        private void OnSystemGenerated(StellarSystem system)
//        {
//            if (!mParams.GeneratePlanets) return;

//            // generate positions for worlds around stars and the star system 
//            // note: zoneGenerator just generates the habitable/forbidden/inner/outer zones for the star system
//            OrbitSelector zoneGenerator = new OrbitSelector(_random);
//            zoneGenerator.Apply(system);

//            List<OrbitSelector.OrbitInfo> orbits = new List<OrbitSelector.OrbitInfo>();
//            // note: orbitGenerator just generates the orbits and the types of planets for each star systems based on it's orbtial zone information
//            foreach (OrbitSelector.OrbitalZoneInfo zoneInfo in zoneGenerator.OribtalZones)
//                orbits.AddRange(zoneGenerator.GenerateOrbits(zoneInfo));


//            // flesh out the full world statistics for each planet
//            foreach (OrbitSelector.OrbitInfo orbit in orbits )
//            {
//                Planet newplanet = new Planet(orbit.ParentBody.GetFreeChildName());

//                // make sure this translation fits within the current region's radius.  Ideally the entire planet
//                // should fit within the bounds of the system 
//                newplanet.Translation = new Vector3d(orbit.OrbitalRadius, 0, 0);
//                // TODO: System.Diagnostics.Trace.Assert(PlanetFitsEntirelyInSystem(newplanet));
//                // TODO: eventually compute more sophisticated positions instead of linear along x axis
//                newplanet.OrbitalRadius = orbit.OrbitalRadius;
//                newplanet.WorldType = orbit.WorldType;

//                // Add the world directly to whatever system or star it's been created under
//                if (orbit.ParentBody is StellarSystem)
//                    ((StellarSystem)orbit.ParentBody).AddChild(newplanet);
//                else
//                    orbit.ParentBody.AddChild(newplanet);



//                if (orbit.WorldType == WorldType.PlanetoidBelt )
//                {
//                    //Celestial.PlanetoidBelt belt = new Celestial.PlanetoidBelt();
//                    // TODO: create a PlanetoidField object that inherits Region perhaps and then uses our new
//                    // system of "circled covered wagon formation" of boxes which we'll be procedurally generated during rendering
//                    // i.e no need to store every asteroid.  This will mitigate some of the performance annoyance with saving regions to disk
//                    ProceduralHelper.InitAsteroidField (newplanet , 100000);
//                }
//                else 
//                {
//                    if (orbit.ParentBody is Star)
//                    {
//                        Star star = (Star)orbit.ParentBody;
//                        _worldGen.ComputeWorldStatistics(newplanet, star.Age, star.Luminosity, star.LuminosityClass,
//                                            star.SpectralType, star.SpectralSubType, 
//                                            orbit, orbit.OrbitID, orbit.Zone);

//                    }
//                    else // starsystem
//                    {
//                        // TODO: temp hack hardcoded values
//                        SPECTRAL_TYPE spectralType = SPECTRAL_TYPE.M;
//                        SPECTRAL_SUB_TYPE spectralSubType = SPECTRAL_SUB_TYPE.SubType_5 ;
//                        LUMINOSITY highestLuminosityClass = LUMINOSITY.WHITEDWARF_D;
//                        float oldestAge = 0;
//                        //  multistar systems usually have stars of same age since they usually form together, exceptions are when stars capture other stars that formed seperately.  there are rules for exceptions for very old 10billion+ year systems too but i dont know if i implemented those

//                        float combinedLuminosity = 0;
                        
//                        for (int j = 0; j < ((StellarSystem)orbit.ParentBody).StarCount; j++)
//                        {
//                            combinedLuminosity += ((StellarSystem)orbit.ParentBody).Stars[j].Luminosity;

//                            highestLuminosityClass =
//                                highestLuminosityClass.CompareTo(((StellarSystem)orbit.ParentBody).Stars[j].Luminosity) > 0
//                                    ?
//                                        highestLuminosityClass
//                                    : ((StellarSystem)orbit.ParentBody).Stars[j].LuminosityClass;
//                        }
//                        // TODO:  read the rules and figure out what to use for spectraltype and subtype and
//                        // verify im handling luminosity correctly with combined and highest for class
//                        _worldGen.ComputeWorldStatistics(newplanet, oldestAge, combinedLuminosity, highestLuminosityClass, 
//                                 spectralType, spectralSubType,
//                                orbit, orbit.OrbitID, orbit.Zone);
//                    }
//                    ProceduralHelper.InitPlanet(newplanet, orbit.WorldType == WorldType.Terrestial, orbit.WorldType == WorldType.Terrestial);
//                }
//                //// at this stage we Generate Moons for this planet
//                //Moon[] moons = _moonGen.GenerateMoons(worlds[i], zones[i]);

//                //foreach (Moon m in moons)
//                //{
//                //    _worldGen.ComputeWorldStatistics(newsystem.Children, m);
//                //    worlds[i].World.AddChild(m); // TODO: when adding these moons to the proper parent, the parent child relationships need to configure themselve
//                //}
//            }
//        }
//    }
//}
////// //First clear our Systems, Stars and Worlds tables
////db.Execute; "DELETE * FROM StarSystems";
////db.Execute; "DELETE * FROM Stars";
////db.Execute; "DELETE * FROM Worlds";
////db.Execute; "DELETE * FROM Moons";

////for (int i = 1; (i <= UBound(arrSystems)); i++) 
////{
////    db.Execute; ("INSERT INTO StarSystems(SystemID,Name,xPos,yPos,zPos) VALUES (" 
////                + (arrSystems[i].ID + ("," + (''' 
////                + (arrSystems[i].Name + (''' + ("," 
////                + (arrSystems[i].xPos + (", " 
////                + (arrSystems[i].yPos + (", " 
////                + (arrSystems[i].zPos + " )"))))))))))));
////    DoEvents;
////}
////for (int i = 1; (i <= UBound(arrStars)); i++) 
////{
////    db.Execute;
////    (@"INSERT INTO Stars(StarID,Name,SystemID, xPos,yPos,zPos, DistanceFromSystemCenter,LuminosityClass, SpectralType, SpectralSubType, orbitalEccentricity, MinSeperation, MaxSeperation,Temperature,Luminosity, Mass, Radius, Age, InnerLimitDistance,OuterLimitDistance,LifeZoneInnerEdge,LifeZoneOuterEdge,SnowLine)" + (" VALUES (" 
////                + (arrStars[i].ID + ("," + (''' 
////                + (arrSystems[i].Name + (''' + ("," 
////                + (arrStars[i].SystemID + ("," 
////                + (arrStars[i].xPos + (", " 
////                + (arrStars[i].yPos + (", " 
////                + (arrStars[i].zPos + (", " 
////                + (arrStars[i].DistanceFromSystemCenter + (", " 
////                + (arrStars[i].LuminosityClass + (", " 
////                + (arrStars[i].SpectralType + (", " 
////                + (arrStars[i].SpectralSubType + (", " 
////                + (arrStars[i].OrbitalEccentricity + (", " 
////                + (arrStars[i].MinSeperation + (", " 
////                + (arrStars[i].MaxSeperation + (", " 
////                + (arrStars[i].Temperature + (", " 
////                + (arrStars[i].Luminosity + (", " 
////                + (arrStars[i].Mass + (", " 
////                + (arrStars[i].Radius + (", " 
////                + (arrStars[i].Age + (", " 
////                + (arrStars[i].InnerLimitDistance + (", " 
////                + (arrStars[i].OuterLimitDistance + (", " 
////                + (arrStars[i].LifeZoneInnerEdge + (", " 
////                + (arrStars[i].LifeZoneOuterEdge + (", " 
////                + (arrStars[i].SnowLine + ")")))))))))))))))))))))))))))))))))))))))))))))))));
////    DoEvents;
////}
////for (int i = 1; (i <= UBound(arrPlanets)); i++) 
////{
////    db.Execute;
////    (@"INSERT INTO Worlds(WorldID,Name,WorldType,StarID,SystemID,xPos,yPos,zPos, ZoneType,Size , BiosphereType, Diameter, DENSITY, Mass, SurfaceGravity, OrbitalRadius, OrbitalEccentricity, OrbitalPeriod, AtmospherePressure, Hydrographics, NativeEcosphere, AtmosphereComposition, PrimaryContaminant, Albedo, GreenHouseFactor, AverageSurfaceTemperature ) VALUES (" 
////                + (arrPlanets[i].ID + ("," + (''' 
////                + (arrPlanets[i].Name + (''' + ("," 
////                + (arrPlanets[i].WorldType + (", " 
////                + (arrPlanets[i].StarID + (", " 
////                + (arrPlanets[i].SystemID + ("," 
////                + (arrPlanets[i].xPos + (", " 
////                + (arrPlanets[i].yPos + (", " 
////                + (arrPlanets[i].zPos + ("," 
////                + (arrPlanets[i].ZoneType + ("," 
////                + (arrPlanets[i].Size + ("," 
////                + (arrPlanets[i].BiosphereType + ("," 
////                + (arrPlanets[i].Diameter + ("," 
////                + (arrPlanets[i].DENSITY + ("," 
////                + (arrPlanets[i].Mass + ("," 
////                + (arrPlanets[i].SurfaceGravity + ("," 
////                + (arrPlanets[i].OrbitalRadius + ("," 
////                + (arrPlanets[i].OrbitalEccentricity + ("," 
////                + (arrPlanets[i].OrbitalPeriod + ("," 
////                + (arrPlanets[i].AtmospherePressure + ("," 
////                + (arrPlanets[i].Hydrographics + ("," 
////                + (arrPlanets[i].NativeEcosphere + ("," 
////                + (arrPlanets[i].AtmosphereComposition + ("," 
////                + (arrPlanets[i].PrimaryContaminant + ("," 
////                + (arrPlanets[i].Albedo + ("," 
////                + (arrPlanets[i].GreenHouseFactor + ("," 
////                + (arrPlanets[i].AverageSurfaceTemperature + " )"))))))))))))))))))))))))))))))))))))))))))))))))))))));
////    DoEvents;
////}
////for (int i = 1; (i <= UBound(arrMoons)); i++) 
////{
////    db.Execute;
////    (@"INSERT INTO Moons(MoonID,Name,PlanetID,SystemID,xPos,yPos,zPos, ZoneType,Size,MoonType, MoonClass, Diameter, DENSITY, Mass, SurfaceGravity, OrbitalRadius, OrbitalEccentricity, OrbitalPeriod, AtmospherePressure, Hydrographics, NativeEcosphere, AtmosphereComposition, PrimaryContaminant, Albedo, GreenHouseFactor, AverageSurfaceTemperature ) VALUES (" 
////                + (arrMoons[i].ID + ("," + (''' 
////                + (arrMoons[i].Name + (''' + (", " 
////                + (arrMoons[i].PlanetID + (", " 
////                + (arrMoons[i].SystemID + ("," 
////                + (arrMoons[i].xPos + (", " 
////                + (arrMoons[i].yPos + (", " 
////                + (arrMoons[i].zPos + ("," 
////                + (arrMoons[i].ZoneType + ("," 
////                + (arrMoons[i].Size + ("," 
////                + (arrMoons[i].MoonType + ("," 
////                + (arrMoons[i].MoonClass + ("," 
////                + (arrMoons[i].Diameter + ("," 
////                + (arrMoons[i].DENSITY + ("," 
////                + (arrMoons[i].Mass + ("," 
////                + (arrMoons[i].SurfaceGravity + ("," 
////                + (arrMoons[i].OrbitalRadius + ("," 
////                + (arrMoons[i].OrbitalEccentricity + ("," 
////                + (arrMoons[i].OrbitalPeriod + ("," 
////                + (arrMoons[i].AtmospherePressure + ("," 
////                + (arrMoons[i].Hydrographics + ("," 
////                + (arrMoons[i].NativeEcosphere + ("," 
////                + (arrMoons[i].AtmosphereComposition + ("," 
////                + (arrMoons[i].PrimaryContaminant + ("," 
////                + (arrMoons[i].Albedo + ("," 
////                + (arrMoons[i].GreenHouseFactor + ("," 
////                + (arrMoons[i].AverageSurfaceTemperature + " )"))))))))))))))))))))))))))))))))))))))))))))))))))))));
////    Debug.Print;
////    (arrMoons[i].ID + (", " + (''' 
////                + (arrMoons[i].Name + (''' + (", " 
////                + (arrMoons[i].PlanetID + (", " 
////                + (arrMoons[i].SystemID + (", " 
////                + (arrMoons[i].xPos + (", " 
////                + (arrMoons[i].yPos + (", " 
////                + (arrMoons[i].zPos + (", " 
////                + (arrMoons[i].ZoneType + (", " 
////                + (arrMoons[i].Size + (", " 
////                + (arrMoons[i].MoonType + (", " 
////                + (arrMoons[i].MoonClass + (", " + arrMoons[i].Diameter))))))))))))))))))))))));
////    ("" + ("," 
////                + (arrMoons[i].DENSITY + ("," 
////                + (arrMoons[i].Mass + ("," 
////                + (arrMoons[i].SurfaceGravity + ("," 
////                + (arrMoons[i].OrbitalRadius + ("," 
////                + (arrMoons[i].OrbitalEccentricity + ("," 
////                + (arrMoons[i].OrbitalPeriod + ("," 
////                + (arrMoons[i].AtmospherePressure + ("," 
////                + (arrMoons[i].Hydrographics + ("," 
////                + (arrMoons[i].NativeEcosphere + ("," 
////                + (arrMoons[i].AtmosphereComposition + ("," 
////                + (arrMoons[i].PrimaryContaminant + ("," 
////                + (arrMoons[i].Albedo + ("," 
////                + (arrMoons[i].GreenHouseFactor + ("," + arrMoons[i].AverageSurfaceTemperature))))))))))))))))))))))))))));
////}