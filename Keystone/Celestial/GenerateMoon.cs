using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Keystone.Celestial
{
    public enum MoonSize
    {
        Large,
        Small
    }

    public class GenerateMoon
    {
        private Random _random;

        public GenerateMoon(Random random)
        {
            if (random == null) throw new ArgumentNullException();
            _random = random;
        }

        // this sub accepts a Planet Type (Terrestial or GasGiant), its diameter and orbital radius from its Primary star
        // and determines if Large Moons for a Terrestial planet or small moons (not both) should be created.
        // A Gas
        // Giant will be allowed to have both Large and Small moons.  If the moons are large enough
        // and have a thin atmosphere or denser, then they may even be able to support life
        public World[] GenerateMoons(World parentWorld, OrbitSelector.OrbitInfo parentWorldOrbit, float StarAge,
                                  float StarLuminosity,
                                  byte SpectralType, float nSnowLine, HabitalZones zone)
        {
            List<World> _moons = new List<World>();
            int moonCount = 0;
            MoonSize moonsize = MoonSize.Small;
            string[] moonNames = null;

            // only create moons for terrestial planets further than 0.5AU distance from it's star
            if (parentWorldOrbit.WorldType == WorldType.Terrestial && parentWorld.OrbitalRadius >= 0.5f * Temp.AU_TO_METERS)
            {
                // do we have any large moons?
                moonCount = _random.Next(1, 6) - 4;
                moonCount += GetNumberOfMoonsModifier(parentWorld.Diameter, (float)parentWorld.OrbitalRadius, parentWorldOrbit.WorldType);
                if (moonCount > 0)
                    moonsize = MoonSize.Large;
                else
                {
                    // no large moons, do we have any small moons?
                    moonCount = _random.Next(1, 6) - 2;
                    moonCount += GetNumberOfMoonsModifier(parentWorld.Diameter, (float)parentWorld.OrbitalRadius, parentWorldOrbit.WorldType);
                    if (moonCount > 0)
                        moonsize = MoonSize.Small;
                    else
                        moonCount = 0; // no moons whatsoever
                }

                moonNames = GetMoonNames(parentWorld.Name, 0, moonCount);
                
                if (moonCount > 0)
                    _moons.AddRange(PlaceMoons(parentWorld, parentWorldOrbit.WorldType, moonsize, moonCount, moonNames, 10));
            }
            else if (parentWorldOrbit.WorldType == WorldType.GasGiant)
            {
                int totalMoons = 0;
                // first generate the small close moons
                moonCount = _random.Next(2, 12);
                moonNames = GetMoonNames(parentWorld.Name, totalMoons, moonCount);
                totalMoons += moonCount;

                if (moonCount > 0)
                    _moons.AddRange(PlaceMoons(parentWorld, parentWorldOrbit.WorldType, MoonSize.Small, moonCount, moonNames, 0));

                // TODO: Here is where i would set the ring system for the planet to being
                // LIGHTRINGS,MEDIUM RINGS,HEAVYRINGS"  if the planet has 3 small close moons in this system it will have
                // light ring system,if it has 6 it will have medium, if it has 10 then it willhave rings as Heavy
                // as Saturn.  Note that rings are the equivalent of planetoidbelts around Stars but with smaller rocks and fine dust particles compared to larger rocks and even giant asteroids

                // Now generate the mid range to large moons and append them to the moon list
                moonCount = _random.Next(1, 6);
                moonCount += GetNumberOfMoonsModifier(parentWorld.Diameter, (float)parentWorld.OrbitalRadius, parentWorldOrbit.WorldType);
                moonNames = GetMoonNames(parentWorld.Name, totalMoons, moonCount);
                totalMoons += moonCount;

                if (moonCount > 0)
                    _moons.AddRange(PlaceMoons(parentWorld, parentWorldOrbit.WorldType, MoonSize.Large, moonCount, moonNames, 2));

                // third generate the far range small moons
                moonCount = _random.Next(1, 6);
                moonCount += GetNumberOfMoonsModifier(parentWorld.Diameter, (float)parentWorld.OrbitalRadius, parentWorldOrbit.WorldType);
                moonNames = GetMoonNames(parentWorld.Name, totalMoons, moonCount);
                totalMoons += moonCount;

                if (moonCount > 0)
                    // //NOTE: Im ignoring the +4 modifier that should occur when a Planetoid belt is within 3AU
                    _moons.AddRange(PlaceMoons(parentWorld, parentWorldOrbit.WorldType, MoonSize.Small, moonCount, moonNames, 3));
            }

            if (moonCount > 0)
                return _moons.ToArray();
            return null;
        }

        private string[] GetMoonNames(string parentWorldID, int startIndex, int count)
        {
            string[] results = new string[count];
            for (int i = 0; i < count; i++)
                results[i] = parentWorldID + " " + (startIndex + i).ToString();

            return results;

        }
        // called after the size of moons has been determined
        private World[] PlaceMoons(World world, WorldType worldType, MoonSize moonsize, int moonCount, string[] moonNames, float minOrbitalRadius)
        {
            System.Diagnostics.Debug.Assert(moonCount == moonNames.Length);
            World[] moons = new World[moonCount];
            double previousRadius = 0;

            // we have large moons
            for (int i = 0; i < moonCount; i++)
            {
            	string id = Resource.Repository.GetNewName(typeof(World));
                World newmoon = new World(id);
                newmoon.Name = moonNames[i];
                
                if (moonsize == MoonSize.Large)
                    newmoon.Diameter = GetLargeMoonDiameter(world.Diameter);
                else
                    newmoon.Diameter = GetSmallMoonDiameter();

                newmoon.OrbitalRadius = GetMoonOrbitalRadius(worldType, moonsize, world.Diameter, newmoon.Diameter, 0, previousRadius);
                previousRadius = newmoon.OrbitalRadius;
                moons[i] = newmoon;
            }
            
            return moons;
        }

        // this function determines how many moons are generated
        // the modifiers are different depending on whether the planet the moons will be orbiting
        // is a GasGiant or Terrestial
        private int GetNumberOfMoonsModifier(float planetDiameter, float orbitalRadius, WorldType worldType)
        {
            int result = 0;
            if (worldType == WorldType.Terrestial)
            {
                if (orbitalRadius < 0.75)
                    result = -3;
                else if (orbitalRadius >= 0.75 && orbitalRadius <= 1.5)
                    result = -1;
                else if (planetDiameter > 9000)
                    result = 1;
                else if (planetDiameter >= 2000 && planetDiameter < 4000)
                    result = -1;
                else if (planetDiameter < 2000)
                    result = -2;
            }
            else if (worldType == WorldType.GasGiant)
            {
                if (orbitalRadius < 0.5)
                    result = -5;
                else if (orbitalRadius < 0.75)
                    result = -3;
                else if (orbitalRadius < 1.5)
                    result = -1;
            }
            else
                Debug.Print("Invalid worldType " + worldType + " in Function:GetNumberOfMoonsModifier");

            return result;
        }

        private float GetLargeMoonDiameter(float planetDiameter)
        {
            // function accepts the diameter of a planet and then generates a diameter for a large moon
            int result;
            int sizemodifier = _random.Next(2, 12) - 7;
            double MILES_TO_METERS = 1609.34;


            if (planetDiameter >= 60000 * MILES_TO_METERS) // 60000 miles
                sizemodifier = (sizemodifier + 2);
            else if (planetDiameter >= 40000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier + 1);
            else if (planetDiameter >= 10000 * MILES_TO_METERS)
                sizemodifier = sizemodifier; // change nothing
            else if (planetDiameter >= 8000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier - 1);
            else if (planetDiameter >= 6000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier - 2);
            else if (planetDiameter >= 4000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier - 3);
            else if (planetDiameter >= 2000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier - 4);
            else if (planetDiameter >= 2000 * MILES_TO_METERS)
                sizemodifier = (sizemodifier - 5);


            if (sizemodifier > 0)
                result = sizemodifier * 1000 + _random.Next(1, 500);
            else
                result = _random.Next(1, 500);

            return result * (float)MILES_TO_METERS;
        }


        private float GetSmallMoonDiameter()
        {
            int result;
            float MILES_TO_METERS = 1609.34f;

            int sizemodifier = _random.Next(2, 12) - 5;

            if (sizemodifier > 0)
                result = (sizemodifier*10); // //this gives us our moonsize in 10's of miles
            else
                result = _random.Next(1, 9);

            return result * MILES_TO_METERS;
        }

        // page 60
        private float GetMoonOrbitalRadius(WorldType parentWorldType, MoonSize moonSize, float planetDiameter, float moonDiameter, byte iFamily, double minimumRadius)
        {
            // this function returns the orbital radius for a moon based on the planet type, its diameter
            // the moons diameter and what family of moons for a gas giant it is

            if (parentWorldType == WorldType.Terrestial)
            {
                
                switch (moonSize)
                {
                    case MoonSize.Large:

                        // if minimum raidus > 0, then our next radius must be between 10 planetary radius greater
                        return _random.Next(10, 80) * planetDiameter; 

                        break;
                    case MoonSize.Small:
                        // terrestial planet's small moon orbit between 2 and 7 planetary radii
                        return _random.Next(2, 7) * planetDiameter;
                        break;
                    default:
                        throw new Exception();
                }
            }
            else if (parentWorldType == WorldType.GasGiant)
            {
                switch (moonSize)
                {
                    case MoonSize.Large :
                        return _random.Next(6, 30) * planetDiameter; // second family of large medium range moons
                        break;
                    case MoonSize.Small :
                        return _random.Next(2, 5) * planetDiameter; // first family of close small moons
                        return _random.Next(40, 250) * planetDiameter; // last family of close far moons
                        break;
                }
            }
            else
                throw new Exception("No moons in planetary belts!");

            return 0;
            // check to make sure there that large moons are not within 10 planetary radius of each other
            //while (bTooClose)
            //{
            //    bTooClose = false;
            //    for (int j = 0; j < moonCount; j++)
            //    {
            //        if (Math.Abs(newmoon.OrbitalRadius - moons[j].OrbitalRadius) <= minOrbitalRadius)
            //        {
            //            newmoon.OrbitalRadius = GetMoonOrbitalRadius(world.WorldType, world.Diameter,
            //                                                         newmoon.Diameter, 0);
            //            bTooClose = true;
            //        }
            //    }
            //}

           
            // todo
        }
    }
}