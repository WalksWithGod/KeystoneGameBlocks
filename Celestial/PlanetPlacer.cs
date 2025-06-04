using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Elements;
using Core.Entities;
using Core.Traversers;
using Core.Types;

namespace Celestial
{
    
    public class PlanetPlacer : ITraverser
    {
        public struct WorldInfo
        {
            public World World;
            public int OrbitID;
            public HabitalZones Zone;
            public Body ParentBody;
            public OrbitalZoneInfo OrbitalZoneInfo;

            public WorldInfo(World planet, OrbitalZoneInfo info, HabitalZones zone, int orbitID)
            {
                World = planet;
                Zone = zone;
                OrbitID = orbitID;
                ParentBody = info.Body ;
                OrbitalZoneInfo = info;
            }
        }
        
        public class OrbitalZoneInfo
        {
            public Body Body;
            public float InnerLimit; // in AU
            public float OuterLimit; // in AU
            public float LifeZoneInnerEdge; // in AU
            public float LifeZoneOuterEdge; // in AU
            public float SnowLine; // in AU
            public float ForbiddenZoneInnerEdge;
            public float ForbiddenZoneOuterEdge;
            public float SquareRootofLuminosity;
            public float BaseOrbitalRadius;
            public float BodeConstant;
            public float MinimumSeperation;
            
            public Random _rand;
           
            private OrbitalZoneInfo(Random rand)
            {
                if (rand == null) throw new ArgumentNullException();
                _rand = rand;
            }

            public OrbitalZoneInfo(System system, Random rand): this(rand)
            {
                Body = system;
            }
            
            // this is private because you will never pass two stars externally like this, it will always be the parent
            private OrbitalZoneInfo (Star[] stars, Random rand) : this(rand)
            {
                float combinedMass = 0;
                float combinedLuminosity = 0;

                foreach (Star s in stars)
                {
                    combinedMass += s.Mass;
                    combinedLuminosity += s.Luminosity;
                }

                GetZoneInfo(stars[0].OrbitalRadius, combinedMass, combinedLuminosity);
            }

            public OrbitalZoneInfo(Star star, Random rand): this(rand)
            {
                Body = star;
                GetZoneInfo(star.OrbitalRadius, star.Mass, star.Luminosity);
            }
            
            private void GetZoneInfo(float orbitalRadius, float mass, float luminosity)
            {
                // since we use the square root of luminosity alot, lets pre-calculate it
                if (luminosity == 0)
                {
                    // do a divide by zero check
                    SquareRootofLuminosity = 1;
                    Debug.Print("Warning: Divide by zero in sub GeneratePlanets");
                }
                else
                    SquareRootofLuminosity = (float)Math.Pow(luminosity, 2);

                MinimumSeperation = orbitalRadius;
                
                // First get the inner limit distances.  It is the bigger of two possible values.
                InnerLimit = mass * 0.2f;
                InnerLimit = Math.Max(InnerLimit, (0.0088f * SquareRootofLuminosity));

                // NOTE: outer limit has a minimum value of 10AU
                OuterLimit = Math.Max(10, (40 * mass));
                SnowLine = (5 * SquareRootofLuminosity);

                // life zone inner and outer edges
                LifeZoneInnerEdge = 0.95f * SquareRootofLuminosity;
                LifeZoneOuterEdge = 1.3f * SquareRootofLuminosity;

                // forbidden zone inner and outer edges
                ForbiddenZoneInnerEdge = orbitalRadius / 3;
                ForbiddenZoneOuterEdge = orbitalRadius * 3;

                // Now we can start to calculate our Orbital Zones
                //   First get our BaseOrbitalRadius distance
                BaseOrbitalRadius = _rand.Next(10, 35) / 100 * InnerLimit;

                BodeConstant = GetBodeConstant();    
            }

            
            float GetBodeConstant()
            {
                float temp = (float)_rand.NextDouble();
                if (temp <= 0.333)
                    temp = 0.3f;
                else if (temp <= 0.666)
                    temp = 0.35f;
                else
                    temp = 0.4f;

                return temp;
            }
        }

        Random _rand;
        public WorldInfo[] Worlds;
        
        public PlanetPlacer (Random rand)
        {
            if (rand == null) throw new ArgumentNullException();
            _rand = rand;
        }
        #region IEntityTraverser Members
        public void Apply(EntityBase entity)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        public void Apply(System system)
        {
            uint starCount = system.StarCount;
            uint systemCount = system.SubSystemCount;
            if (starCount + systemCount == 0) return;

            if (starCount + systemCount == 1 && HasPlanets ())
            {
                if (system.Children[0] is Star)
                    PlacePlanets(new OrbitalZoneInfo((Star)system.Children[0], _rand));
                else
                    // recurse and determine if the child stars of this subsystem will have seperate planets or share the system's planets
                    Apply((System)system.Children[0]);
            }
            else
            {
                Debug.Assert(starCount + systemCount == 2);

                // if the two children are close enough then the planets will orbit both and be added as children to the parent System.
                if (system.Diameter * .5 < 5)
                {
                    if (HasPlanets()) PlacePlanets(new OrbitalZoneInfo(system, _rand));
                }
                else if (starCount == 2)
                {
                    // each child star will have it's own planets
                    if (HasPlanets()) PlacePlanets(new OrbitalZoneInfo((Star)system.Children[0], _rand));
                    if (HasPlanets()) PlacePlanets(new OrbitalZoneInfo((Star)system.Children[1], _rand)); 
                }
                else if (systemCount == 2)
                {
                    // recurse each system to determine if their children will have seperate planets or not
                    Apply((System)system.Children[0]); // todo: PlacePlanets under this child using an orbitalZone class that has the info 
                    Apply((System)system.Children[1]);
                }
            }
        }
        
        public void Apply (Star star)
        {
            
        }

        // todo: we might have to pass a Combined mass here!!!  i cant just remove star and world.  I could pass in "system" and then enable true
        // the "use combined" bool arg?  or i could pass an array of stars
        //private WorldInfo[] PlacePlanets(OrbitalZoneInfo zoneInfo)
        private void PlacePlanets(OrbitalZoneInfo zoneInfo)
        {
            // This sub routine is called after each star system is made and will develop all the
            //   planets within that star system
            // NOTE:  The argument MaxSeperation and MinSeperation will be -1 if this is not a multiple star system

            List<WorldInfo> worlds = new List<WorldInfo>(); // a temporary list of all created worlds so we can itterate them easily
            
            HabitalZones zone;
            WorldType worldType;

            // Finally we can start creating planets.
            float location = zoneInfo.BaseOrbitalRadius;
            int iBCmod = 1; 
            int orbitID = 1;
            
            // Now create our planets at the appropriate planetary orbits
            while (location < zoneInfo.OuterLimit)
            {
                // determine which Zone we are in
                if (location < zoneInfo.LifeZoneInnerEdge)
                    zone = HabitalZones.Inner;
                else if (location <= zoneInfo.LifeZoneOuterEdge)
                    zone = HabitalZones.Life;
                else if (location <= zoneInfo.SnowLine)
                    zone = HabitalZones.Middle;
                else
                    zone = HabitalZones.Outer;

                // check to see if we are in a forbidden zone
                if (location >= zoneInfo.ForbiddenZoneInnerEdge && location <= zoneInfo.ForbiddenZoneOuterEdge)
                    zone = HabitalZones.Forbidden;

                // if we are not in a forbidden zone then we can create a planet here 
                if (zone != HabitalZones.Forbidden)
                {
                    Planet newplanet = new Planet(zoneInfo.Body.GetFreeChildName());
                    
                    //  Decide if this is a gas giant by rolling 3d against the Zone we are in
                    double randomvalue = _rand.NextDouble();
                    switch (zone)
                    {
                        case HabitalZones.Inner:
                            if (randomvalue <= 0.0625)
                                newplanet.WorldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Life:
                            if (randomvalue <= 0.125)
                                newplanet.WorldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Middle:
                            if (randomvalue <= 0.3125)
                                newplanet.WorldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Outer:
                            if (randomvalue <= 0.75)
                                newplanet.WorldType = WorldType.GasGiant;
                            break;
                    }

                    newplanet.Translation = new Vector3d(location, 0, 0); // todo: eventually compute more sophisticated positions instead of linear along x axis
                    newplanet.OrbitalRadius = location;
                    // add the world directly to whatever system or star it's been created under
                    if (zoneInfo.Body is System)
                        ((System)zoneInfo.Body).AddChild(newplanet);
                    else
                        zoneInfo.Body.AddChild(newplanet);
                        
                    // and add it to the temporarly list as well
                    worlds.Add(new WorldInfo(newplanet, zoneInfo , zone, orbitID));
                }
                
                // move to the next orbital location
                location += (zoneInfo.BodeConstant * iBCmod);
                iBCmod *= 2;
                orbitID++; // should orbitID increment even for 
                // the sequence for orbital radius is InnerLimit, Innerlimit + nBodeConstant, Innerlimit + 2*nbodeConstant, InnerLimit + 4*nBodeConstant, InnerLimit + 8*nBodeConstant, InnerLimit + 16*nBodeConstant, etc
            }

            // now that we have ALL our gas giants assigned, determine
            // which of the previously created planets are terrestial or planetoidbelts
            for (int i = 0; i < worlds.Count; i++)
            {
                if (worlds[i].World.WorldType != WorldType.GasGiant)
                {
                    // if the next planet out is a gas giant the odds are increased that this will be a planetoid belt
                    if (i + 1 < worlds.Count)
                    {
                        if (worlds[i + 1].World.WorldType == WorldType.GasGiant)
                        {
                            if (_rand.NextDouble() <= 0.8125)
                                worldType = WorldType.PlanetoidBelt;
                            else
                                worldType = WorldType.Terrestial;
                        }
                        else
                        {
                            // use the normal odds
                            if (_rand.NextDouble() <= 0.25)
                                worldType = WorldType.PlanetoidBelt;
                            else
                                worldType = WorldType.Terrestial;
                        }
                    }
                    //  if we have a companion star, then odds of planetoid belt in last orbit are increased
                    else if (zoneInfo.MinimumSeperation != -1)
                    {
                        if (_rand.NextDouble() <= 0.625)
                            worldType = WorldType.PlanetoidBelt;
                        else
                            worldType = WorldType.Terrestial;
                    }
                    else if (_rand.NextDouble() <= 0.25)
                        worldType = WorldType.PlanetoidBelt;
                    else
                        worldType = WorldType.Terrestial;

                    
                    worlds[i].World.WorldType = worldType;
                }

            }
         //   return worlds.ToArray();
        }

        private bool HasPlanets()
        {
            // //this routine determines whether this star or system center will have planets orbiting it
            // todo:
            return true;
        }

        #region ITraverser Members

        public void Apply(Node scene)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(SceneNode sn)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(OctreeHost host)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(Core.Octree.OctreeNode o)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(EntityNode en)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(RegionNode rn)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(ModeledEntity entity)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(Core.Portals.Portal p)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Portals.Region sector)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        
        

        public void Apply(Terrain terrain)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(ModelBase model)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(LODSwitch lod)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Geometry element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Lights.DirectionalLight directionalLight)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
