using System;
using System.Diagnostics;

namespace Celestial
{
    public class WorldGen
    {

        private Random _random;
        // fill the world type matrix. NOTE earthlike is not included at this stage
        BiosphereType[,] arrBiosphereType = new BiosphereType[,] {
                 {BiosphereType.Hostile_SG, BiosphereType.Hostile_SG, BiosphereType.Hostile_SG, BiosphereType.Hostile_SG},
                 {BiosphereType.Greenhouse, BiosphereType.Ocean, BiosphereType.Hostile_N, BiosphereType.Hostile_A},
                 { BiosphereType.Desert, BiosphereType.Desert, BiosphereType.Desert, BiosphereType.Hostile_A},
                 {BiosphereType.Rockball, BiosphereType.Rockball, BiosphereType.Rockball, BiosphereType.IcyRockball},
                 {BiosphereType.Rockball, BiosphereType.Rockball, BiosphereType.Rockball, BiosphereType.IcyRockball}};
        
        public  WorldGen(Random random)
        {
            if (random == null) throw new ArgumentNullException();
            _random = random;

       }

        // todo: the below needs / should be integrated with the single function for computing world statistics.
        //       The below code represents the only real differences between the seperate Moon implementation and the Planet implementation 
        //// now we can get the stats for the large moons to see if they can support life
        //    foreach (Moon m in _moons)
        //    {
        //        position.x += m.OrbitalRadius; // for simplicity (todo: temporary),they will also be in line
        //        m.Translation = position;  // we know our moons will orbit in the same plane
        //        m.WorldType = WorldType.Terrestial;
        //        m.Zone = zone; // todo: zone will match the parent planet's zone 
                
        //        m.Density = GetPlanetDensity(WorldGen.WorldType.Terrestial, m.Diameter, m.Zone, StarAge);
        //        m.Mass = GetPlanetMass(m.DENSITY, m.Diameter);
        //        m.SurfaceGravity = GetPlanetSurfaceGravity(m.Mass, m.Diameter);
        
        // }
        
        public void ComputeWorldStatistics(Star[] star, PlanetPlacer.WorldInfo worldInfo, int OrbitID, HabitalZones Zone)
        {
            if (star == null || star.Length == 0) throw new ArgumentNullException();
            
            World w = worldInfo.World;
            SPECTRAL_TYPE specType = SPECTRAL_TYPE.G; // star.SpectralType;
            SPECTRAL_SUB_TYPE specSubType = SPECTRAL_SUB_TYPE.SubType_3; // star.SpectralSubType;
            LUMINOSITY lc = LUMINOSITY.DWARF_V; // star.LuminosityClass;
            uint luminosity = 10000; // star.Luminosity;
            float age = 5;// star.Age;
            WorldSize size = WorldSize.Standard; // w.Size;
            
            // use the combined luminosity if multiple stars in array.  For the ages and such, do we use the oldest or youngest?
            
            // Do this for all planets EXCEPT Planetoid belts
            if (w.WorldType != WorldType.PlanetoidBelt)
                {
                    w.Diameter = GetPlanetDiameter(w.WorldType, Zone, OrbitID, w.OrbitalRadius, worldInfo.OrbitalZoneInfo.SnowLine,
                                                   worldInfo.OrbitalZoneInfo.BodeConstant, lc, specType, specSubType);
                    w.Density = GetPlanetDensity(w.WorldType, w.Diameter, Zone, age);
                    w.Mass = GetPlanetMass(w.Density, w.Diameter);
                    w.SurfaceGravity = GetPlanetSurfaceGravity(w.Mass, w.Diameter);
                    // the following are only done on Terrestial worlds and moons (if desired)
                    if (w.WorldType == WorldType.Terrestial)
                    {
                        size = GetWorldSize(w.OrbitalRadius, luminosity, (uint)w.Mass, (uint)w.Diameter, Zone);
                        
                        w.Biosphere.BiosphereType = GetBiosphereType(size, Zone);
                        w.Biosphere.Atmosphere.Pressure = GetPlanetAtmospherePressure(w.Biosphere.BiosphereType, size, w.SurfaceGravity);
                        w.Biosphere.OceanCoverage = GetPlanetHydropraphics(w.Biosphere.BiosphereType, size, 
                                                                           w.Biosphere.Atmosphere.Pressure, Zone, w.OrbitalRadius,
                                                                           specType, worldInfo.OrbitalZoneInfo.SnowLine);
                        // % of ocean coverage
                        w.Biosphere.Life = GetNativeEcosphere(w.Biosphere.BiosphereType, w.Biosphere.OceanCoverage, age);
                        // (what type of life exists)
                        // note that if the NativeEcosphere is Metazoa or higher than we change it to BiosphereType.Earthlike else we change it to BiosphereType.Hostile_N
                        if (w.Biosphere.Life >= Life.Metazoa)
                            w.Biosphere.BiosphereType = BiosphereType.Earthlike;
                        else
                            w.Biosphere.BiosphereType = BiosphereType.Hostile_N;

                        // w.AtmosphereComposition = GetPlanetAtmosphereComposition
                        // GetPrimaryContaminant
                        w.Biosphere.Albedo = GetPlanetAlbedo(w.Biosphere.BiosphereType, w.Biosphere.OceanCoverage);
                        w.Biosphere.Atmosphere.GreenHouseFactor = GetPlanetGreenhouseFactor(w.Biosphere.BiosphereType, w.Biosphere.Atmosphere.Pressure, w.SurfaceGravity);
                        w.Biosphere.SurfaceTemperature = GetPlanetAverageSurfaceTemperature(luminosity, w.OrbitalRadius, w.Biosphere.Albedo, w.Biosphere.Atmosphere.GreenHouseFactor);
                    }
                    // now we can get the orbital stats for the planet and moons
                    // w.OrbitalEccentricity = GetPlanetOrbitalEccentricity
                    // w.OrbitalPeriod = GetPlanetOrbitalPeriod
                    // GetTidalForce
                    // GetRotationPeriod
                    // GetLocalCalender
                    // GetAxialTilt
                }
                else
                {
                    // if its a planetoid belt, we probably need to do something like
                    // create a bunch of rocks and place them in orbit? That or dont really
                    // support them
                }
        }
        
        int GetPlanetDiameter(WorldType  worldType, HabitalZones  zone, int OrbitID, float OrbitalRadius, float SnowLine, float BodeConstant,
                               LUMINOSITY LuminosityClass, SPECTRAL_TYPE SpectralType, SPECTRAL_SUB_TYPE SubType)
        {
            // Diamaters are created based primarily on the type of planet and its orbital distance
            int result  = 0;
            int i = _random.Next(2, 12);
            // modifiers for orbit location and proximity to star
            if (OrbitID == 1)
                i -= 4;
            else if (zone == HabitalZones.Inner )
                i -= 2;
            else if (zone == HabitalZones.Outer  && OrbitalRadius <= SnowLine + BodeConstant)
                i += 6;
            else if (zone == HabitalZones.Outer && OrbitalRadius <= SnowLine + BodeConstant * 2)
                i += 4;
            //else if (zone != HabitalZones.Outer)
                // unmodified

            // modifiers for star type
            if (LuminosityClass == LUMINOSITY.DWARF_V )
            {
                if (SpectralType == SPECTRAL_TYPE.M && SubType <= SPECTRAL_SUB_TYPE.SubType_4)
                    i--;
                else if (SpectralType == SPECTRAL_TYPE.M && SubType > SPECTRAL_SUB_TYPE.SubType_4)
                    i -= 2;
            }
            // compute actual diameter based on planet type
            if (worldType == WorldType.Terrestial)
            {
                result = (i * 1000);
                result = (result + (_random.Next(0, 7) * 100)); // todo: this is supposed to be 2d-7 * 100 but we use 0 - 7
                result = Math.Max(1000, result);  // make sure our min value is 1000
            }
            else if (worldType == WorldType.GasGiant)
            {
                result = (i * 5000);
                result = (result + (_random.Next(0, 7) * 100));
                result = Math.Max (25000, result);
            }
            else
                Debug.Assert(worldType != WorldType.PlanetoidBelt);

            return result;
        }

        // Determines density in grams per cubic centimeter of the planet
        float GetPlanetDensity(WorldType worldType, float Diameter, HabitalZones zone, float StarAge)
        {
            float modifier;
            float result = 0;
            if (worldType == WorldType.Terrestial)
            {
                modifier = _random.Next(3, 18);
                modifier = modifier - (StarAge / 0.5f); // NOTE: This assumes billions!! We are dividing by 500 million or .5 billion
                modifier /= 10; // retain fractions here
                // now get the base value
                if (Diameter < 3000)
                {
                    if (zone >= HabitalZones.Outer)
                        result = 2.3f;
                    else
                        result = 3.2f;
                }
                else if (Diameter < 6000)
                {
                    if (zone >= HabitalZones.Outer)
                        result = 1.6f;
                    else
                        result = 4.4f;
                }
                else if (Diameter < 9000)
                {
                    if (zone >= HabitalZones.Outer)
                        result = 1.8f;
                    else
                        result = 5.3f;
                }
                else if (Diameter >= 9000)
                {
                    if (zone >= HabitalZones.Outer )
                        result = 1.9f;
                    else
                        result = 5.9f;
                }

                // now add our modifier and retvals and make sure we are at least minimum 1.3
                result = (float)Math.Max(1.3, (modifier + result));
            }
            else if (worldType == WorldType.GasGiant)
            {
                // now get our density for gas giants.  Its very straight forward.  Just look
                // up the diameter and give it the density that corresponds
                if (Diameter < 40000)
                    result = 1.4f;
                else if (Diameter < 60000)
                    result = 1;
                else if (Diameter < 80000)
                    result = 0.7f;
                else if (Diameter < 85000)
                    result = 1;
                else if (Diameter >= 85000)
                    result = 1.4f;
                
                // todo: custom rule: add a fudge factor so the results have variation
                //float fudge = _random.NextDouble()
            }
            return result;
        }


        float GetPlanetMass(float DENSITY, float Diameter)
        {
            // //returns the planets mass in units of earth mass
            float RetVal;
            float d = (Diameter / 1000);
            // diameter in thousands of miles. save this to a single or else we can get overflow errors
            RetVal = (DENSITY * d * d * d) / 2750;
            return RetVal;
        }

        // returns the gravity in units of earth gravity
        float GetPlanetSurfaceGravity(float Mass, float Diameter)
        {
            Diameter /= 1000;
            //  diamater must be in 1000's of miles
            if (Diameter != 0)
                return (62.9f * Mass) / (Diameter * Diameter);
            else
                return 0;
        }

        void GetPlanetOrbitalEccentricity()
        {
            float MaxSeperation;
            float MinSeperation;
            // //first calc the min and max seperation
        }

        void GetPlanetOrbitalPeriod()
        {
        }

        // //get the tidal force
        // Note: Not this version
        // //get the rotation period
        // Note: Not this version
        // //get the local calender
        // Note: Not this version
        // //get the Axial tilt
        // Note: Not this version
        WorldSize GetWorldSize(float OrbitalRadius, uint luminosity, uint mass, uint diameter, HabitalZones  zone)
        {
            // this function is used to determine what size of world this is.
            // First get the correction factor to correct the size parameter
            // to account for the energy is receives from its primary sun.  the closer
            // a planet is to its primary, it is warmer and the molecules in its upper
            // atmosphere are more likely to attain escape velocity.
            // This correction factor which will give us greater accuracy in producing realistic worlds
            //  C = square root of R / fourth root of L
            double  c;
            double p;
            WorldSize result;

            Debug.Assert(luminosity != 0);
            c = Math.Pow (OrbitalRadius , 1/2)  / Math.Pow (luminosity , 1/4);

            p = (7.93 * mass) / (diameter / 1000);
            p = (p * c);
            // now we can get our size class
            if (p <= 0.13)
                result = WorldSize.Tiny ;
            else if (p <= 0.24)
                result = WorldSize.VerySmall ;
            else if (p <= 0.38)
                result = WorldSize.Small ;
            else if (p <= 1.73)
                result = WorldSize.Standard ;
            else
                result = WorldSize.Large ;
            return result;

        }

        BiosphereType GetBiosphereType(WorldSize Size, HabitalZones  Zone)
        {
            // after determinine World Size in the "GetWorldSize" function
            // we call this to determine the class of world such as BiosphereType.Hostile_A, BiosphereType.Desert,BiosphereType.Rockball, etc
            return arrBiosphereType[(int)Size,(int) Zone];
        }

        Pressure GetPlanetAtmospherePressure(BiosphereType biosphereType, WorldSize  SizeClass, float SurfaceGravity)
        {
            // this function returns the atmosphere pressure.
            Pressure result = Pressure.None ;

            if (SizeClass == WorldSize.Tiny )
                result = Pressure.Thin ;
            else if (SizeClass == WorldSize.VerySmall )
                result = Pressure.Trace;
            else if (biosphereType == BiosphereType.Greenhouse)
                result = Pressure.SuperDense ;
            else
            {
                //  for all other worlds we will calculate the atmosphere pressure
                float Temp = _random.Next(3, 18) * (0.1f * SurfaceGravity);
                if (Temp < 0.5)
                    result = Pressure.VeryThin ;
                else if (Temp < 0.8)
                    result = Pressure.Thin ;
                else if (Temp < 1.2)
                    result = Pressure.Standard ;
                else if (Temp < 1.5)
                    result = Pressure.Dense ;
                else if (Temp >= 1.5)
                    result = Pressure.VeryDense ;
            }
            return result;
        }


        int GetPlanetHydropraphics(BiosphereType biosphereType, WorldSize  SizeClass, Pressure AtmospherePressure, HabitalZones zone, float OrbitalRadius, SPECTRAL_TYPE SpectralType, float SnowLine)
        {
            // //returns an INTEGER for the Hydrographics  of the planet (aka % of ocean coverage)
            int RetVal;
            if (SizeClass >= WorldSize.Small  && AtmospherePressure >= Pressure.VeryThin && zone != HabitalZones.Inner && OrbitalRadius < SnowLine * 3)
            {
                RetVal = _random.Next(2, 10);
                // //now apply modifiers for star spectral type
                if (SpectralType == SPECTRAL_TYPE.M )
                    RetVal = (RetVal + 2);
                else if (SpectralType == SPECTRAL_TYPE.K)
                    RetVal = (RetVal + 1);
                else if (SpectralType == SPECTRAL_TYPE.F)
                    RetVal = (RetVal - 1);
                else if (SpectralType == SPECTRAL_TYPE.A)
                    RetVal = (RetVal - 2);

                // //modifiers for world class and zone
                if (biosphereType == BiosphereType.Desert)
                {
                    if (zone == HabitalZones.Life )
                        RetVal = (RetVal - 8);
                    else if (zone == HabitalZones.Middle )
                        RetVal = (RetVal - 6);
                }
                else if (biosphereType == BiosphereType.Hostile_SG || biosphereType == BiosphereType.Hostile_N || biosphereType == BiosphereType.Hostile_A)
                {
                    RetVal = (RetVal - 2);
                }
                // now apply some variation to the value
                RetVal = RetVal + _random.Next(-5, 5);
                // make sure the value is no more than 100 or no less than 0
                if (RetVal < 0)
                    RetVal = 0;
                else if (RetVal > 100)
                    RetVal = 100;
            }
            else
                RetVal = 0;

            return RetVal;
        }

        Life GetNativeEcosphere(BiosphereType BiosphereType, int Hydrographics, float StarAge)
        {
            // get the Native Ecosphere (what type of life exists)
            int i = 0;
            Life result = Life.None ;
            
            if (Hydrographics > 0)
            {
                i = _random.Next(2, 12);
                i = (int)result + (int)(StarAge / 0.5f);
                if (BiosphereType == BiosphereType.Ocean)
                    i += 2;
            }

            // now determine our Native Ecosphere
            if (i <= 13)
            {
                result = Life.None ;
            }
            else if (i <= 16)
            {
                result = Life.Protozoa ;
            }
            else if (i == 17)
            {
                result = Life.Metazoa ;
            }
            else if (i == 18)
            {
                result = Life.SimpleAnimals ;
            }
            else if (i >= 19)
            {
                result = Life.ComplexAnimals ;
            }
            return result;
        }

        void GetPlanetAtmosphereComposition()
        {
        }


        float GetPlanetAlbedo(BiosphereType BiosphereType, int Hydrographics)
        {
            // This returns a SINGLE between 0 and 1 representing the amount of stellar energy that is reflected
            // away from the planets surface.  For instance, planets with total cloud coverage will have
            // a value closer to 1 indicating that ALOT of sunlight is reflected back into space resulting
            // in cooler AverageSurfaceTemperature.  An albedo closer to 0 ndicates it absorbs more stellar energy
            float result= 0;
            float i;
            i = _random.Next(3, 18) * 0.01f;
            // now find our base value depending on our planet class
            switch (BiosphereType)
            {
                case BiosphereType.Hostile_SG:
                    result = 0.5f;
                    break;
                case BiosphereType.Hostile_N:
                    result = 0.2f;
                    break;
                case BiosphereType.Hostile_A:
                    result = 0.5f;
                    break;
                case BiosphereType.Desert:
                    result = 0.02f;
                    break;
                case BiosphereType.Rockball:
                    result = 0.02f;
                    break;
                case BiosphereType.IcyRockball:
                    result = 0.45f;
                    break;
                case BiosphereType.Earthlike:
                    if (Hydrographics < 30)
                        result = 0.02f;
                    else if (Hydrographics < 60)
                        result = 0.1f;
                    else if (Hydrographics < 90)
                        result = 0.2f;
                    else if (Hydrographics >= 90)
                        result = 0.28f;

                    break;
                default:
                    Debug.WriteLine("Planet Class Not Listed in function:GetPlanetAlbedo");
                    break;
            }
            // //now add our base value to our random value
            result += i;
            Debug.Assert (result <= 1);
            return result;
        }

        float GetPlanetGreenhouseFactor(BiosphereType  BiosphereType, Pressure AtmospherePressure, float SurfaceGravity)
        {
            // this returns a single representing how much of the heat that is retained by the
            // planets atmosphere
            float result = 0;
            
            // now find our base value depending on our planet class
            switch (BiosphereType)
            {
                case BiosphereType.Hostile_SG:
                    result = 0.2f;
                    break;
                case BiosphereType.Hostile_N:
                    result = 0.2f;
                    break;
                case BiosphereType.Hostile_A:
                    result = 0.2f;
                    break;
                case BiosphereType.Desert:
                    result = 0.15f;
                    break;
                case BiosphereType.Rockball:
                    result = 0;
                    // TODO: # ... Warning!!! not translated
                    break;
                case BiosphereType.IcyRockball:
                    result = 0;
                    // TODO: # ... Warning!!! not translated
                    break;
                case BiosphereType.Earthlike:
                    result = 0.15f;
                    break;
                default:
                    Debug.WriteLine  ("Planet Class Not Listed in function:GetPlanetGreenhouseFactor");
                    break;
            }
            // //now perform our calculation
            if (SurfaceGravity != 0)
                result *= (int)AtmospherePressure / SurfaceGravity;

            return result;
        }

        float GetPlanetAverageSurfaceTemperature(float Luminosity, float OrbitalRadius, float Albedo, float GreenHouseFactor)
        {
            // //returns single representing the Climate Type in Farenheit.
            float blackbodyTemp;
            float A = 1 - Albedo;
            float result;
            // //first we must determine the blackbody temperation
            blackbodyTemp  = 278 * (float) (Math.Pow(Luminosity, 1/4) / Math.Pow(OrbitalRadius ,1/2));
            result = blackbodyTemp * (float)((Math.Pow (A , 1/4) * (1 + GreenHouseFactor)));

            return 1.8f * result - 460;
        }
    }
}
