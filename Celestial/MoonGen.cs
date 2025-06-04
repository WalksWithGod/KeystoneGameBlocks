using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Celestial
{
    public enum MoonSize
    {
        Large,
        Small
    }
    
    public class MoonGen
    {
        Random _random;

        public MoonGen (Random random)
        {
            if (random == null) throw new ArgumentNullException();
            _random = random;
        }
        
        // this sub accepts a Planet Type (Terrestial or GasGiant), its diameter and orbital radius from its Primary star
        // and produces Large Moons for a Terrestial planet or small moons (not both).  A Gas
        // Giant will be allowed to have both Large and Small moons.  If the moons are large enough
        // and have a thin atmosphere or denser, then they may even be able to support life
        public void GenerateMoons(int PlanetID, long SystemID, Core.Types.Vector3d position, WorldType worldType, 
                                  int planetDiameter, float planetOrbitalRadius, float StarAge, float StarLuminosity, 
                                  byte SpectralType, float nSnowLine, HabitalZones zone)
        {
            List<Moon> _moons = new List<Moon>();
            int moonCount = 0;
            MoonSize moonsize = MoonSize.Small ;
            
            // only create moons for terrestial planets further than 0.5 distance from it's star
            if (worldType == WorldType.Terrestial && planetOrbitalRadius >= 0.5f)
            {
                // do we have any large moons?
                moonCount = _random.Next(1, 6) - 4;
                moonCount += GetNumberOfMoonsModifier(planetDiameter, planetOrbitalRadius, worldType);
                if (moonCount > 0)
                    moonsize = MoonSize.Large;
                else
                {
                    // no large moons, do we have any small moons?
                    moonCount = _random.Next(1, 6) - 2;
                    moonCount += GetNumberOfMoonsModifier(planetDiameter, planetOrbitalRadius, worldType);
                    if (moonCount > 0)
                        moonsize = MoonSize.Small ;
                    else
                        moonCount = 0; // no moons whatsoever
                }
                
                if (moonCount > 0)
                    CreateMoons(ref _moons, worldType, planetDiameter, moonsize, moonCount, 10);
            }
            else if (worldType == WorldType.GasGiant )
            {
                // first generate the small close moons
                moonCount = _random.Next(2, 12);
                if (moonCount > 0)
                    CreateMoons(ref _moons, worldType, planetDiameter, MoonSize.Small , moonCount, 0); 

                // TODO: Here is where i would set the ring system for the planet to being
                // LIGHTRINGS,MEDIUM RINGS,HEAVYRINGS"  if the planet has 3 small close moons in this system it will have
                // light ring system,if it has 6 it will have medium, if it has 10 then it willhave rings as Heavy
                // as Saturn.  Note that rings are the equivalent of planetoidbelts around Stars but with smaller rocks and fine dust particles compared to larger rocks and even giant asteroids
                
                // Now generate the mid range to large moons and append them to the moon list
                moonCount = _random.Next(1, 6);
                moonCount += GetNumberOfMoonsModifier(planetDiameter, planetOrbitalRadius, worldType);
                if (moonCount > 0)
                    CreateMoons(ref _moons, worldType, planetDiameter, MoonSize.Large , moonCount, 2); 

                // third generate the far range small moons
                moonCount = _random.Next(1, 6);
                moonCount += GetNumberOfMoonsModifier(planetDiameter, planetOrbitalRadius, worldType);

                if (moonCount > 0)
                    // //NOTE: Im ignoring the +4 modifier that should occur when a Planetoid belt is within 3AU
                    CreateMoons(ref _moons, worldType,planetDiameter, MoonSize.Small, moonCount, 3);
            }
        }

        void CreateMoons(ref List<Moon> moons, WorldType worldType, int planetDiameter, MoonSize moonsize, int moonCount, float minOrbitalRadius)
        {
            // TODO: Temporarily commented out til i get more of this crap compiling
            //if (moons == null) throw new ArgumentNullException();

            //bool bTooClose = true;
            
            //// we have large moons
            //for (int i = 0; i < moonCount; i++)
            //{
            //    Moon newmoon = new Moon();
                
            //    if (moonsize  == MoonSize.Large )
            //        newmoon.Diameter = GetLargeMoonDiameter(planetDiameter);
            //    else
            //        newmoon.Diameter = GetSmallMoonDiameter();

            //    newmoon.OrbitalRadius = GetMoonOrbitalRadius(worldType, planetDiameter, newmoon.Diameter, 0);
            //    moons.Add(newmoon);

            //    // check to make sure there all 3 moons are not within 10 planetary radius of each other
            //    while (bTooClose)
            //    {
            //        bTooClose = false;
            //        for (int j = 0; j < moonCount - 1; j++)
            //        {
            //            if (Math.Abs(newmoon.OrbitalRadius - moons[j].OrbitalRadius) <= minOrbitalRadius)
            //            {
            //                newmoon.OrbitalRadius = GetMoonOrbitalRadius(worldType, planetDiameter, newmoon.Diameter, 0);
            //                bTooClose = true;
            //            }
            //        }
            //    }
            //} 
        }

        // this function determines how many moons are generated
        // the modifiers are different depending on whether the planet the moons will be orbiting
        // is a GasGiant or Terrestial
        int GetNumberOfMoonsModifier(int planetDiameter, float orbitalRadius, WorldType worldType)
        {
            int result  = 0;
            if (worldType == WorldType.Terrestial )
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
            else if (worldType == WorldType.GasGiant )
            {
                if (orbitalRadius < 0.5)
                    result = -5;
                else if (orbitalRadius < 0.75)
                    result = -3;
                else if (orbitalRadius < 1.5)
                    result = -1;
            }
            else
                Debug.Print ("Invalid worldType " + worldType + " in Function:GetNumberOfMoonsModifier");

            return result;
        }

        private int GetLargeMoonDiameter(int planetDiameter)
        {
            // function accepts the diameter of a planet and then generates a diameter for a large moon
            int result;
            int sizemodifier = _random.Next(2, 12) - 7;

            if (planetDiameter >= 60000)
                sizemodifier = (sizemodifier + 2);
            else if (planetDiameter >= 40000)
                sizemodifier = (sizemodifier + 1);
            else if (planetDiameter >= 10000)
                sizemodifier = sizemodifier; // change nothing
            else if (planetDiameter >= 8000)
                sizemodifier = (sizemodifier - 1);
            else if (planetDiameter >= 6000)
                sizemodifier = (sizemodifier - 2);
            else if (planetDiameter >= 4000)
                sizemodifier = (sizemodifier - 3);
            else if (planetDiameter >= 2000)
                sizemodifier = (sizemodifier - 4);
            else if (planetDiameter >= 2000)
                sizemodifier = (sizemodifier - 5);


            if (sizemodifier > 0)
                result = sizemodifier*1000 + _random.Next(1, 500);
            else
                result = _random.Next(1, 500);

            return result;
        }


        int GetSmallMoonDiameter()
        {
            int result;
            int sizemodifier = _random.Next(2, 12) - 5;
            
            if (sizemodifier > 0)
                result = (sizemodifier * 10); // //this gives us our moonsize in 10's of miles
            else
                result = _random.Next(1, 9);

            return result;
        }

        float GetMoonOrbitalRadius(WorldType worldType, int planetDiameter, int moonDiameter, byte  iFamily)
        {
            // this function returns the orbital radius for a moon based on the planet type, its diameter
            // the moons diameter and what family of moons for a gas giant it is
            return  _random.Next(1, 40);
            // todo
        }
    
    }
}
