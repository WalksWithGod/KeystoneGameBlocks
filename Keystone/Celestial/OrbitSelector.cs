using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Celestial
{
    public class OrbitSelector
    {
        private Random _rand;
        public List<OrbitalZoneInfo> OribtalZones = new List<OrbitalZoneInfo>();

        public struct OrbitInfo
        {
            //public World World;
            public int OrbitID;
            public HabitalZones Zone;
            public Body ParentBody;
            public OrbitalZoneInfo OrbitalZoneInfo;
            //public WorldType WorldType;
            //public BiosphereType BiosphereType;
            public WorldType WorldType;

            public double OrbitalRadius;

            // TODO: I think ZoneType is now in the OrbitalZoneInfo
            //public byte ZoneType; // this is mostly just for generation data.  Zone type is for determining habital or not zone


            public OrbitInfo(WorldType type, OrbitalZoneInfo info, HabitalZones zone, int orbitID, double orbitalRadius)
            {
                //World = planet;
                WorldType = type;
                //BiosphereType = bioType;
                Zone = zone;
                OrbitID = orbitID;
                ParentBody = info.Body;
                OrbitalZoneInfo = info;
                OrbitalRadius = orbitalRadius;
            }

            public void SetWorldType(WorldType type)
            {
                WorldType = type;
            }
        }

        public class OrbitalZoneInfo
        {
            public Body Body;
            public double InnerLimit; // in AU
            public double OuterLimit; // in AU
            public double LifeZoneInnerEdge; // in AU
            public double LifeZoneOuterEdge; // in AU
            public double SnowLine; // in AU
            public double ForbiddenZoneInnerEdge;
            public double ForbiddenZoneOuterEdge;
            public float SquareRootofLuminosity;
            public float BaseOrbitalRadius;
            public double BodeConstant;
            public double MinimumSeperation;

            public Random _rand;

            private OrbitalZoneInfo(Random rand)
            {
                if (rand == null) throw new ArgumentNullException();
                _rand = rand;
            }

            // this is private because you will never pass two stars externally like this, it will always be the parent
            private OrbitalZoneInfo(Star[] stars, Random rand)
                : this(rand)
            {
                double combinedMass = 0;
                float combinedLuminosity = 0;

                foreach (Star s in stars)
                {
                    combinedMass += s.MassKg;
                    combinedLuminosity += s.Luminosity;
                }

                GetZoneInfo(stars[0].Radius, stars[0].OrbitalRadius, combinedMass, combinedLuminosity);
            }

            public OrbitalZoneInfo(StellarSystem system, Random rand)
                : this(rand)
            {
                Body = system;

                // TODO: if this system contains only stars, we can call the private method that accepts an array of stars

                // TODO: if it contains multiple sub-systems or a cobmination of stars and subsystems, we need to handle that

            }

            public OrbitalZoneInfo(Star star, Random rand)
                : this(rand)
            {
                Body = star;
                GetZoneInfo(star.Radius, star.OrbitalRadius, star.MassKg, star.Luminosity);
            }

            private void GetZoneInfo(double starRadius, double orbitalRadius, double mass, float luminosity)
            {
                Debug.Assert(orbitalRadius > starRadius, "OrbitSelector.GetZoneInfo() - Orbital Radius cannot be less than Star Radius");

                // since we use the square root of luminosity alot, lets pre-calculate it
                if (luminosity == 0)
                {
                    // do a divide by zero check
                    SquareRootofLuminosity = 1;
                    Debug.WriteLine("Warning: Divide by zero in sub GeneratePlanets");
                }
                else
                    SquareRootofLuminosity = (float)Math.Sqrt(luminosity);

                // TODO: I need to verify that MinimumSeperation also takes into account the worlds radius.
                //       and perhaps even a bit greater padding because right now, some inner planets seem way too close to the star
                MinimumSeperation = orbitalRadius;

                // First get the inner limit distances.  It is the bigger of two possible values.
                InnerLimit = (mass / Temp.SOL_MASS_KILOGRAMS * 0.2) * Temp.AU_TO_METERS;
                InnerLimit = Math.Max(InnerLimit, (0.0088 * SquareRootofLuminosity * Temp.AU_TO_METERS));
                InnerLimit = Math.Max(InnerLimit, starRadius + starRadius * .2);
                Debug.Assert(InnerLimit > starRadius);

                // NOTE: outer limit has a minimum value of 10AU
                // TODO: limit the max outer limit to a size that will keep it inside the radius
                // of a region when taking account any cumulative center offsets of the entire system within that region
                OuterLimit = Math.Max(10d * Temp.AU_TO_METERS, (40d * Temp.AU_TO_METERS * mass / Temp.SOL_MASS_KILOGRAMS));
                SnowLine = (5d * Temp.AU_TO_METERS * SquareRootofLuminosity);

                // life zone inner and outer edges
                LifeZoneInnerEdge = Temp.AU_TO_METERS * 0.95 * SquareRootofLuminosity;
                LifeZoneOuterEdge = Temp.AU_TO_METERS * 1.3 * SquareRootofLuminosity;

                // forbidden zone inner and outer edges
                ForbiddenZoneInnerEdge = Temp.AU_TO_METERS * orbitalRadius / 3d;
                ForbiddenZoneOuterEdge = Temp.AU_TO_METERS * orbitalRadius * 3d;

                // Now we can start to calculate our Orbital Zones
                //   First get our BaseOrbitalRadius distance
                BaseOrbitalRadius = (float)(_rand.Next(2, 7) / 2d * InnerLimit);

                BodeConstant = GetBodeConstant();
            }


            private double GetBodeConstant()
            {
                float temp = (float)_rand.NextDouble();
                if (temp <= 0.333)
                    temp = 0.3f;
                else if (temp <= 0.666)
                    temp = 0.35f;
                else
                    temp = 0.4f;

                return temp * Temp.AU_TO_METERS;
            }
        }



        public OrbitSelector(Random rand)
        {
            if (rand == null) throw new ArgumentNullException();
            _rand = rand;
        }

        /// <summary>
        /// Generates the OrbitalZoneInformation for each star and/or sub-system that will have worlds orbiting it
        /// </summary>
        /// <param name="system"></param>
        public void Apply(StellarSystem system)
        {
            uint starCount = system.StarCount;
            uint systemCount = system.SubSystemCount;
            if (starCount + systemCount == 0) return;

            if (starCount + systemCount == 1 && HasPlanets())
            {
                if (system.Children[0] is Star)
                    OribtalZones.Add(new OrbitalZoneInfo((Star)system.Children[0], _rand));
                else
                    // recurse and determine if the child stars of this subsystem will have seperate planets or share the system's planets
                    Apply((StellarSystem)system.Children[0]);
            }
            else
            {
                Debug.Assert(starCount + systemCount == 2);

                // if the two children are close enough then the planets will orbit both and be added as children to the parent System.
                if (system.Radius < 5)
                {
                    if (HasPlanets()) OribtalZones.Add(new OrbitalZoneInfo(system, _rand));
                }
                else if (starCount == 2)
                {
                    // each child star will have it's own planets
                    if (HasPlanets())
                        OribtalZones.Add(new OrbitalZoneInfo((Star)system.Children[0], _rand));
                    if (HasPlanets())
                        OribtalZones.Add(new OrbitalZoneInfo((Star)system.Children[1], _rand));
                }
                // here we will recurse and thus a single system may have more than one OrbitalZoneInfo added to the final list
                else if (systemCount == 2)
                {
                    // recurse each system to determine if their children will have seperate planets or not
                    Apply((StellarSystem)system.Children[0]);
                    // TODO: PlacePlanets under this child using an orbitalZone class that has the info 
                    Apply((StellarSystem)system.Children[1]);
                }
            }
        }

        public void Apply(Star star)
        {
        }

        // TODO: we might have to pass a Combined mass here!!!  i cant just remove star and world.  I could pass in "system" and then enable true
        // the "use combined" bool arg?  or i could pass an array of stars
        public OrbitInfo[] GenerateOrbits(OrbitalZoneInfo zoneInfo)
        {
            // This sub routine is called after each star system is made and will develop all the
            //   planets within that star system
            // NOTE:  The argument MaxSeperation and MinSeperation will be -1 if this is not a multiple star system

            List<OrbitInfo> orbitData = new List<OrbitInfo>();

            HabitalZones zone;
            WorldType worldType = WorldType.Terrestial; // can be initialized to anything except GasGiant!

            // Finally we can start creating planets.
            double orbitalRadius = zoneInfo.BaseOrbitalRadius;
            int iBCmod = 1;
            int orbitID = 1;

            // Now create our planets at the appropriate planetary orbits
            while (orbitalRadius < zoneInfo.OuterLimit)
            {
                // determine which Zone we are in
                if (orbitalRadius < zoneInfo.LifeZoneInnerEdge)
                    zone = HabitalZones.Inner;
                else if (orbitalRadius <= zoneInfo.LifeZoneOuterEdge)
                    zone = HabitalZones.Life;
                else if (orbitalRadius <= zoneInfo.SnowLine)
                    zone = HabitalZones.Middle;
                else
                    zone = HabitalZones.Outer;

                // check to see if we are in a forbidden zone
                if (orbitalRadius >= zoneInfo.ForbiddenZoneInnerEdge && orbitalRadius <= zoneInfo.ForbiddenZoneOuterEdge)
                    zone = HabitalZones.Forbidden;

                // if we are not in a forbidden zone then we can create a planet here 
                if (zone != HabitalZones.Forbidden)
                {

                    //  Decide if this is a gas giant by rolling 3d against the Zone we are in
                    double randomvalue = _rand.NextDouble();
                    switch (zone)
                    {
                        case HabitalZones.Inner:
                            if (randomvalue <= 0.0625)
                                worldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Life:
                            if (randomvalue <= 0.125)
                                worldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Middle:
                            if (randomvalue <= 0.3125)
                                worldType = WorldType.GasGiant;
                            break;
                        case HabitalZones.Outer:
                            if (randomvalue <= 0.75)
                                worldType = WorldType.GasGiant;
                            break;
                        default:
                            throw new Exception("Invalid habital zone");
                    }

                    // and add it to the temporarly list as well
                    orbitData.Add(new OrbitInfo(worldType, zoneInfo, zone, orbitID, orbitalRadius));
                }

                // move to the next orbital location
                orbitalRadius += (zoneInfo.BodeConstant * iBCmod);
                iBCmod *= 2;
                orbitID++; // should orbitID increment even for 
                // the sequence for orbital radius is InnerLimit, Innerlimit + nBodeConstant, Innerlimit + 2*nbodeConstant, InnerLimit + 4*nBodeConstant, InnerLimit + 8*nBodeConstant, InnerLimit + 16*nBodeConstant, etc
            }

            // now that we have ALL our gas giants assigned, determine
            // which of the previously created orbits are terrestial or planetoidbelts
            for (int i = 0; i < orbitData.Count; i++)
            {
                if (orbitData[i].WorldType != WorldType.GasGiant)
                {
                    // if the next planet out is a gas giant the odds are increased that this will be a planetoid belt
                    if (i + 1 < orbitData.Count)
                    {
                        if (orbitData[i + 1].WorldType == WorldType.GasGiant)
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

                    orbitData[i].SetWorldType(worldType);
                }
            }
            return orbitData.ToArray();
        }

        private bool HasPlanets()
        {
            // //this routine determines whether this star or system center will have planets orbiting it
            // TODO:
            return true;
        }
    }
}