using System;
using System.Diagnostics;
using Keystone.Types;

namespace Keystone.Celestial
{
    public class GenerateWorld
    {
        private Random _random;
        private static double G = 6.67428E-11;
        

        // fill the world type matrix. NOTE earthlike is not included at this stage
        private BiosphereType[,] arrBiosphereType = 
            new BiosphereType[,]
            {
                {
                    BiosphereType.Hostile_SG, BiosphereType.Hostile_SG,
                    BiosphereType.Hostile_SG, BiosphereType.Hostile_SG
                },
                {
                    BiosphereType.Greenhouse, BiosphereType.Ocean,
                    BiosphereType.Hostile_N, BiosphereType.Hostile_A
                },
                {
                    BiosphereType.Desert, BiosphereType.Desert,
                    BiosphereType.Desert, BiosphereType.Hostile_A
                },
                {
                    BiosphereType.Rockball, BiosphereType.Rockball,
                    BiosphereType.Rockball, BiosphereType.IcyRockball
                },
                {
                    BiosphereType.Rockball, BiosphereType.Rockball,
                    BiosphereType.Rockball, BiosphereType.IcyRockball
                }
            };

        public GenerateWorld(Random random)
        {
            if (random == null) throw new ArgumentNullException();
            _random = random;
        }

        // also see below link on procedural asteroids!  In this way asteroids are simply a seed and a position/rotation/scale
        // http://local.wasp.uwa.edu.au/~pbourke/modelling_rendering/asteroid/
        //To create a bulge an "impact" center is chosen at random on the surface of the current asteroid. All vertices within a chosen radius are perturbed normal to the surface, the degree of perturbation falls off linearly with distance away from the center of impact. The user controls the radius range, depth range, and the number of impacts. This seemed to give sufficient control over the final shape. Planet type objects are created with small shallow bulges (with respect to the initial sphere). More extreme cases are formed as the radius and depth are increased. 

        // TODO: the below needs / should be integrated with the single function for computing world statistics.
        //       The below code represents the only real differences between the seperate Moon implementation and the Planet implementation 
        //// now we can get the stats for the large moons to see if they can support life
        //    foreach (Moon m in _moons)
        //    {
        //        position.x += m.OrbitalRadius; // for simplicity (TODO: temporary),they will also be in line
        //        m.Translation = position;  // we know our moons will orbit in the same plane
        //        m.WorldType = WorldType.Terrestial;
        //        m.Zone = zone; // TODO: zone will match the parent planet's zone 

        //        m.Density = GetPlanetDensity(WorldGen.WorldType.Terrestial, m.Diameter, m.Zone, StarAge);
        //        m.Mass = GetPlanetMass(m.DENSITY, m.Diameter);
        //        m.SurfaceGravity = GetPlanetSurfaceGravity(m.Mass, m.Diameter);

        // }


        // TODO: Star mass must be passed in to compute Oribtal Period
        public void ComputeWorldStatistics(World w, float starAge, float combinedLuminosity, LUMINOSITY highestLuminosityClass,
                                           SPECTRAL_TYPE specType, SPECTRAL_SUB_TYPE specSubType,
                                           OrbitSelector.OrbitInfo worldInfo, HabitalZones Zone)
        {

            // use the combined luminosity if multiple stars in array. or should we just pass the "System" which would have properties to get the combined
            // values?  i should pass in the combined luminosity if its a system with multiple stars and the planets orbit the entire system.
            // For the ages and such, do we use the oldest or youngest?
            if (w.WorldType == WorldType.PlanetoidBelt) throw new Exception ();
            
                
            w.Diameter = GetPlanetDiameter(w.WorldType, Zone, worldInfo.OrbitID, w.OrbitalRadius,
                                           worldInfo.OrbitalZoneInfo.SnowLine,
                                           worldInfo.OrbitalZoneInfo.BodeConstant, highestLuminosityClass, specType,
                                           specSubType);
            w.Density = GetPlanetDensity(w.WorldType, w.Diameter, Zone, starAge);
            w.MassKg = GetPlanetMassKg(w.Density, w.Diameter);
            w.SurfaceGravity = GetPlanetSurfaceGravity(w.MassKg, w.Diameter);


            // now we can get the orbital stats for the planet and moons
            double maxSeperation, minSeperation;
            GenerateOrbitInfo(w, Temp.SOL_MASS_KILOGRAMS,  _random, out maxSeperation , out minSeperation);

            // GetTidalForce
            // GetLocalCalender
            // GetAxialTilt


            // the following are only done on Terrestial worlds and moons (if desired)
            if (w.WorldType == WorldType.Terrestial)
            {
                WorldSize size = GetWorldSize(w.OrbitalRadius, combinedLuminosity, (uint) w.MassKg, (uint) w.Diameter,
                                              Zone);


                w.Biosphere.BiosphereType = GetBiosphereType(size, Zone);
                w.Biosphere.Atmosphere.Pressure = GetPlanetAtmospherePressure(w.Biosphere.BiosphereType, size,
                                                                              w.SurfaceGravity);
                w.Biosphere.OceanCoverage = GetPlanetHydropraphics(w.Biosphere.BiosphereType, size,
                                                                   w.Biosphere.Atmosphere.Pressure, Zone,
                                                                   w.OrbitalRadius,
                                                                   specType, worldInfo.OrbitalZoneInfo.SnowLine);
                // % of ocean coverage
                w.Biosphere.Life = GetNativeEcosphere(w.Biosphere.BiosphereType, w.Biosphere.OceanCoverage, starAge);
                // (what type of life exists)
                // note that if the NativeEcosphere is Metazoa or higher than we change it to BiosphereType.Earthlike else we change it to BiosphereType.Hostile_N
                if (w.Biosphere.Life >= Life.Metazoa)
                    w.Biosphere.BiosphereType = BiosphereType.Earthlike;
                else
                    w.Biosphere.BiosphereType = BiosphereType.Hostile_N;

                // w.AtmosphereComposition = GetPlanetAtmosphereComposition
                // GetPrimaryContaminant
                w.Biosphere.Albedo = GetPlanetAlbedo(w.Biosphere.BiosphereType, w.Biosphere.OceanCoverage);
                w.Biosphere.Atmosphere.GreenHouseFactor = GetPlanetGreenhouseFactor(w.Biosphere.BiosphereType,
                                                                                    w.Biosphere.Atmosphere.Pressure,
                                                                                    w.SurfaceGravity);
                w.Biosphere.SurfaceTemperature = GetPlanetAverageSurfaceTemperature(combinedLuminosity,
                                                                                    w.OrbitalRadius,
                                                                                    w.Biosphere.Albedo,
                                                                                    w.Biosphere.Atmosphere.
                                                                                        GreenHouseFactor);
                
               
            }
        }


        public static void GenerateOrbitInfo(Celestial.Body body, double parentBodyMassKg, Random random, out double maxSeperation, out double minSeperation)
        {
            body.OrbitalEccentricity = (float)GetEccentricty(random, body.OrbitalRadius, out maxSeperation, out minSeperation);
            // TODO: temp using fixed sol mass
            body.OrbitalPeriod = (float)GetOrbitalPeriod(parentBodyMassKg, body.OrbitalRadius);
            if (body.OrbitalPeriod == 0)
                System.Diagnostics.Debug.WriteLine("GenerateWorld.ComputeWorldStatistics() - ERROR: Failed to compute orbital period.");
            body.OrbitalEpoch = body.OrbitalPeriod * random.NextDouble();
            body.OrbitalInclination = (float)GetOrbitalInclination(body.MassKg, body.OrbitalRadius);
            body.OrbitalProcession = (float)(random.NextDouble() * (Math.PI * 2d));

            // June.21.2017 - for v1.0, all orbits are perfect circles and no longer use Orbit animations.  So they aren't actually orbits anymore.
            //                Thus we do not rely on EllipticalAnimation and orbitalPeriod to find the current translation of the world.
            double randomAngle = 2.0d * Math.PI * random.NextDouble();
            double x = body.OrbitalRadius * Math.Cos(randomAngle);
            double z = body.OrbitalRadius * Math.Sin(randomAngle);
            body.Translation = new Vector3d(x, 0, z); // new Vector3d(body.OrbitalRadius, 0, 0);
        }

        /// <summary>
        /// Planet diameter in meters
        /// </summary>
        /// <param name="worldType"></param>
        /// <param name="zone"></param>
        /// <param name="OrbitID"></param>
        /// <param name="OrbitalRadius"></param>
        /// <param name="SnowLine"></param>
        /// <param name="BodeConstant"></param>
        /// <param name="LuminosityClass"></param>
        /// <param name="SpectralType"></param>
        /// <param name="SubType"></param>
        /// <returns></returns>
        private int GetPlanetDiameter(WorldType worldType, HabitalZones zone, int OrbitID, double OrbitalRadius,
                                      double SnowLine, double BodeConstant,
                                      LUMINOSITY LuminosityClass, SPECTRAL_TYPE SpectralType, SPECTRAL_SUB_TYPE SubType)
        {
            // all constants are in meters
            // the comments next to these constants are the gurps values.  we're changing them to meters but using round values
            const int minimum_gasgiant_diameter = 40000000;  // min gas giant diameter is 25,000 miles
            const int minimum_terrestial_diameter = 2000000;  //  min terrestial diameter is 1,000 miles
            const int terrestial_primary_mod = 2000000; // 1000 miles
            const int gasgiant_primary_mod = 10000000; // 5000 miles
            const int terrestial_variability_mod = 200000; // 100 miles
            const int gasgiant_variability_mod = 1000000; // 500 miles

            // Diamaters are created based primarily on the type of planet and its orbital distance
            int result = 0;
            int i = _random.Next(2, 12);
            // modifiers for orbit location and proximity to star
            if (OrbitID == 1)
                i -= 4;
            else if (zone == HabitalZones.Inner)
                i -= 2;
            else if (zone == HabitalZones.Outer && OrbitalRadius <= SnowLine + BodeConstant)
                i += 6;
            else if (zone == HabitalZones.Outer && OrbitalRadius <= SnowLine + BodeConstant*2)
                i += 4;
            //else if (zone != HabitalZones.Outer)
            // unmodified

            // modifiers for star type
            if (LuminosityClass == LUMINOSITY.DWARF_V)
            {
                if (SpectralType == SPECTRAL_TYPE.M && SubType <= SPECTRAL_SUB_TYPE.SubType_4)
                    i--;
                else if (SpectralType == SPECTRAL_TYPE.M && SubType > SPECTRAL_SUB_TYPE.SubType_4)
                    i -= 2;
            }
            // compute actual diameter based on planet type
            if (worldType == WorldType.Terrestial)
            {
                result = (i * terrestial_primary_mod);
                result = (result + (_random.Next(0, 7) * terrestial_variability_mod)); // TODO: this is supposed to be 2d-7 * 100 but we use 0 - 7
                result = Math.Max(minimum_terrestial_diameter, result); 
            }
            else if (worldType == WorldType.GasGiant)
            {
                result = (i * gasgiant_primary_mod);
                result = (result + (_random.Next(0, 7) * gasgiant_variability_mod));
                result = Math.Max(minimum_gasgiant_diameter, result);
            }
            else
                Debug.Assert(worldType != WorldType.PlanetoidBelt);

            return result;
        }

        
        // Determines density in grams per cubic centimeter of the planet
        private float GetPlanetDensity(WorldType worldType, float Diameter, HabitalZones zone, float StarAge)
        {
            float modifier;
            float result = 0;
            if (worldType == WorldType.Terrestial)
            {
                modifier = _random.Next(3, 18);
                modifier = modifier - (StarAge/0.5f);
                    // NOTE: This assumes billions!! We are dividing by 500 million or .5 billion
                modifier /= 10; // retain fractions here
                // now get the base value
                if (Diameter < 3000)
                {
                    if (zone >= HabitalZones.Outer)
                        result = 2.3f;
                    else
                        result = 3.2f;
                }
                else if (Diameter < 6000)// TODO: these diameter values need to be converted to meters
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
                    if (zone >= HabitalZones.Outer)
                        result = 1.9f;
                    else
                        result = 5.9f;
                }

                // now add our modifier and retvals and make sure we are at least minimum 1.3
                result = (float) Math.Max(1.3, (modifier + result));
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

                // TODO: custom rule: add a fudge factor so the results have variation
                //float fudge = _random.NextDouble()
            }
            return result;
        }

        #region Mass and Size
        // TODO: get the tidal force    // Note: Not this version
        // TODO: get the rotation period  // Note: Not this version
        // TODO: get the local calender   // Note: Not this version
        // TODO: get the Axial tilt     // Note: Not this version
        private WorldSize GetWorldSize(double OrbitalRadius, float luminosity, uint mass, uint diameter,
                                       HabitalZones zone)
        {
            // this function is used to determine what size of world this is.
            // First get the correction factor to correct the size parameter
            // to account for the energy is receives from its primary sun.  the closer
            // a planet is to its primary, it is warmer and the molecules in its upper
            // atmosphere are more likely to attain escape velocity.
            // This correction factor which will give us greater accuracy in producing realistic worlds
            //  C = square root of R / fourth root of L
            double c;
            double p;
            WorldSize result;

            Debug.Assert(luminosity != 0);
            c = Math.Pow(OrbitalRadius, 1/2)/Math.Pow(luminosity, 1/4);

            p = (7.93*mass)/(diameter/1000);
            p = (p*c);
            // now we can get our size class
            if (p <= 0.13)
                result = WorldSize.Tiny;
            else if (p <= 0.24)
                result = WorldSize.VerySmall;
            else if (p <= 0.38)
                result = WorldSize.Small;
            else if (p <= 1.73)
                result = WorldSize.Standard;
            else
                result = WorldSize.Large;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="density">grams per cubic centimeter</param>
        /// <param name="diameter"></param>
        /// <returns>mass in kilograms</returns>
        private double GetPlanetMassKg(double densityGramsCM3, double radiusMeters)
        {
            // volume of a sphere is 4pi*r^3/3
            double volume = 4d / 3d * Math.PI * radiusMeters * radiusMeters * radiusMeters;

            // convert density from grams/cm^3  to kg/m3
            double densitykg = densityGramsCM3 * 1000d;

            return volume * densityGramsCM3;
        }

        // returns the gravity in units of earth gravity
        private float GetPlanetSurfaceGravity(double Mass, float Diameter)
        {
            Diameter /= 1000f;
            //  diamater must be in 1000's of miles
            if (Diameter != 0)
                return (float)((62.9f * Mass) / (Diameter * Diameter));
            else
                return 0;
        }
#endregion


        #region Orbital Functions
        private static double GetEccentricty(Random random, double semimajoraxisMeters, out double maxSeperation, out double minSeperation)
        {

            // TODO: 70% of planets will have eccentricty less than 0.1
            // the remaining will have orbits typically from .1 to .25 and with maybe some
            // tiny fraction .25 to .35
            double eccentricity = 0; //  June.12.2017 - for v1 keep things simple and keep all orbits perfectly circular. // random.Next(1, 35) / 100d;
            maxSeperation = (1d + eccentricity) * semimajoraxisMeters;
            minSeperation = (1d - eccentricity) * semimajoraxisMeters; ;

           
            return eccentricity;
        }

        /// <summary>
        /// Orbital period in seconds.
        /// </summary>
        /// <param name="fociMass">Mass of either primary star or the combined mass
        /// if this orbit is around both binaries</param>
        /// <param name="radiusMeters"></param>
        /// <returns></returns>
        public static double GetOrbitalPeriod(double fociMass, double radiusMeters)
        {
            const double SECONDS_IN_YEAR = 31556926;
            // T = 2.0 * PI * SQRT(radiusMeters^3 / G * M);
            //
            // T is the orbital period in seconds (26.5 hrs = 95,400 seconds)
            //    - - 1 earth year = 31,556,926 seconds;
            // G is Newtons constant, 6.673e-11
            // M is the mass of the Sun, 1.9891e30 kg
            // G * M = 132725970000000000000
            //
            // 2pi = 6.28318531;
            // radiusMeters = semi-major axis = for earth 149597892000d meters
            // radiusMeters * radiusMeters * radiusMeters = 5.008433312797295104985488889449e+44
            // sol mass = 1.989e30 kg 
            double radiusKilometers = radiusMeters / 1000d;
            double periodSeconds = Utilities.MathHelper.TWO_PI * Math.Sqrt(Math.Pow(radiusKilometers, 3d) / G * fociMass);

            periodSeconds = Math.Sqrt((4 * Utilities.MathHelper.TWO_PI * Math.Pow(radiusMeters, 3d)) / (G * fociMass));
            //System.Diagnostics.Debug.WriteLine("Orbital period = " + periodSeconds / SECONDS_IN_YEAR + " years");

            return periodSeconds;
        }

        /// <summary>
        /// Returns orbital inclination above the plane of the system in radians.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="semimajoraxis"></param>
        /// <returns>Angle in radians.</returns>
        public static double GetOrbitalInclination(double mass, double semimajoraxis)
        {
            return 0; // June.12.2017 - force all orbits to be co-planar with galactic plane.  This is simpler.  For v2.0 maybe we can add this again.


            // the further out the orbit the greater the potential inclination

            // the smaller the mass, the greater the multiplier of the inclination
            double distanceMultiplier = semimajoraxis / 149597892000d; // earth semimajoraxis
            double massMultiplier =  5.9742e24 / mass; // earth mass


            double result = (distanceMultiplier * massMultiplier) / 10d;
            return result * Utilities.MathHelper.DEGREES_TO_RADIANS;
        }

        // The speed (v) of a satellite in circular orbit is:
        // v = SQRT(G * M / r)
        // where G is the universal gravitational constant (6.6726 E-11 N m2 kg-2), 
        // M is the mass of the combined planet/satellite system (Earth's mass is 5.972 E24 kg), 
        // and r is the radius of the orbit measured from the planet's center.  "SQRT" means "square root".
        /// <summary>
        /// Orbital velocity in meters per second
        /// </summary>
        /// <param name="mass1">mass in kilograms</param>
        /// <param name="mass2">mass in kilograms</param>
        /// <param name="radiusMeters">radius in meters</param>
        /// <returns></returns>
        public static double GetOrbitalVelocity(double mass1, double mass2, double radiusMeters)
        {
            //return Math.Sqrt(G * (mass1 + mass2) * radiusMeters);
            return Math.Sqrt((G * (mass1 + mass2)) / radiusMeters);
        }



        // Using these values gives the speed in meters per second.
        // The period (P) of a satellite in circular orbit is the orbit's circumference
        // divided by the satellite's speed:
        //      P = 2 * pi * rMeters / v 
        /// <summary>
        /// Orbital period in seconds
        /// </summary>
        /// <param name="radiusMeters">radius in meters</param>
        /// <param name="velocity">velocity in meters per second</param>
        /// <returns>Orbital period in seconds.</returns>
        //public static double GetOrbitalPeriod(double radiusMeters, double velocity)
        //{
        //    // convert radius to AU 
        //    double AU = radiusMeters * METERS_TO_AU;


        //    return Utilities.MathHelper.TWO_PI * AU / velocity;
        //}

        //public static double GetOrbitalRadius(double velocity)
        //{
        // r= cube root(T^2 * GM / 4pi^2)

        // T is the orbital period in seconds (26.5 hrs = 95,400 seconds)
        // G is Newtons constant, 6.673 x 10^-11
        // M is the mass of the Earth, 5.9742 x 10^24 kg

        //}

        // returns result in m/s^2
        // gravity effect is always dependant on distance.  That is why typically the only gravity
        // people care about is surface gravity.  Otherwise for something in orbit, we need to compute a different gravity value
        public static double GetSurfaceGravity(Body body)
        {
            return (G * body.MassKg) / Math.Pow((body.Radius), 2);
        }
        #endregion 

        private BiosphereType GetBiosphereType(WorldSize Size, HabitalZones Zone)
        {
            // after determinine World Size in the "GetWorldSize" function
            // we call this to determine the class of world such as BiosphereType.Hostile_A, BiosphereType.Desert,BiosphereType.Rockball, etc
            return arrBiosphereType[(int) Size, (int) Zone];
        }

        private Atmosphere.ATMOSPHERIC_PRESSURE GetPlanetAtmospherePressure(BiosphereType biosphereType,
                                                                            WorldSize SizeClass, float SurfaceGravity)
        {
            // this function returns the atmosphere pressure.
            Atmosphere.ATMOSPHERIC_PRESSURE result = Atmosphere.ATMOSPHERIC_PRESSURE.None;

            if (SizeClass == WorldSize.Tiny)
                result = Atmosphere.ATMOSPHERIC_PRESSURE.Thin;
            else if (SizeClass == WorldSize.VerySmall)
                result = Atmosphere.ATMOSPHERIC_PRESSURE.Trace;
            else if (biosphereType == BiosphereType.Greenhouse)
                result = Atmosphere.ATMOSPHERIC_PRESSURE.SuperDense;
            else
            {
                //  for all other worlds we will calculate the atmosphere pressure
                float Temp = _random.Next(3, 18)*(0.1f*SurfaceGravity);
                if (Temp < 0.5)
                    result = Atmosphere.ATMOSPHERIC_PRESSURE.VeryThin;
                else if (Temp < 0.8)
                    result = Atmosphere.ATMOSPHERIC_PRESSURE.Thin;
                else if (Temp < 1.2)
                    result = Atmosphere.ATMOSPHERIC_PRESSURE.Standard;
                else if (Temp < 1.5)
                    result = Atmosphere.ATMOSPHERIC_PRESSURE.Dense;
                else if (Temp >= 1.5)
                    result = Atmosphere.ATMOSPHERIC_PRESSURE.VeryDense;
            }
            return result;
        }


        private int GetPlanetHydropraphics(BiosphereType biosphereType, WorldSize SizeClass,
                                           Atmosphere.ATMOSPHERIC_PRESSURE AtmospherePressure, HabitalZones zone,
                                           double OrbitalRadius, SPECTRAL_TYPE SpectralType, double SnowLine)
        {
            // //returns an INTEGER for the Hydrographics  of the planet (aka % of ocean coverage)
            int RetVal;
            if (SizeClass >= WorldSize.Small && AtmospherePressure >= Atmosphere.ATMOSPHERIC_PRESSURE.VeryThin &&
                zone != HabitalZones.Inner && OrbitalRadius < SnowLine*3)
            {
                RetVal = _random.Next(2, 10);
                // //now apply modifiers for star spectral type
                if (SpectralType == SPECTRAL_TYPE.M)
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
                    if (zone == HabitalZones.Life)
                        RetVal = (RetVal - 8);
                    else if (zone == HabitalZones.Middle)
                        RetVal = (RetVal - 6);
                }
                else if (biosphereType == BiosphereType.Hostile_SG || biosphereType == BiosphereType.Hostile_N ||
                         biosphereType == BiosphereType.Hostile_A)
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

        private Life GetNativeEcosphere(BiosphereType BiosphereType, int Hydrographics, float StarAge)
        {
            // get the Native Ecosphere (what type of life exists)
            int i = 0;
            Life result = Life.None;

            if (Hydrographics > 0)
            {
                i = _random.Next(2, 12);
                i = (int) result + (int) (StarAge/0.5f);
                if (BiosphereType == BiosphereType.Ocean)
                    i += 2;
            }

            // now determine our Native Ecosphere
            if (i <= 13)
            {
                result = Life.None;
            }
            else if (i <= 16)
            {
                result = Life.Protozoa;
            }
            else if (i == 17)
            {
                result = Life.Metazoa;
            }
            else if (i == 18)
            {
                result = Life.SimpleAnimals;
            }
            else if (i >= 19)
            {
                result = Life.ComplexAnimals;
            }
            return result;
        }

        private void GetPlanetAtmosphereComposition()
        {
        }


        private float GetPlanetAlbedo(BiosphereType BiosphereType, int Hydrographics)
        {
            // This returns a SINGLE between 0 and 1 representing the amount of stellar energy that is reflected
            // away from the planets surface.  For instance, planets with total cloud coverage will have
            // a value closer to 1 indicating that ALOT of sunlight is reflected back into space resulting
            // in cooler AverageSurfaceTemperature.  An albedo closer to 0 ndicates it absorbs more stellar energy
            float result = 0;
            float i;
            i = _random.Next(3, 18)*0.01f;
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
            Debug.Assert(result <= 1);
            return result;
        }

        private float GetPlanetGreenhouseFactor(BiosphereType BiosphereType,
                                                Atmosphere.ATMOSPHERIC_PRESSURE AtmospherePressure, float SurfaceGravity)
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
                    Debug.WriteLine("Planet Class Not Listed in function:GetPlanetGreenhouseFactor");
                    break;
            }
            // //now perform our calculation
            if (SurfaceGravity != 0)
                result *= (int) AtmospherePressure/SurfaceGravity;

            return result;
        }

        private float GetPlanetAverageSurfaceTemperature(float Luminosity, double OrbitalRadius, float Albedo,
                                                         float GreenHouseFactor)
        {
            // //returns single representing the Climate Type in Farenheit.
            float blackbodyTemp;
            float A = 1 - Albedo;
            float result;
            // //first we must determine the blackbody temperation
            blackbodyTemp = 278*(float) (Math.Pow(Luminosity, 1/4)/Math.Pow(OrbitalRadius, 1/2));
            result = blackbodyTemp*(float) ((Math.Pow(A, 1/4)*(1 + GreenHouseFactor)));

            return 1.8f*result - 460;
        }

        // also read the following  http://www.gamedev.net/community/forums/topic.asp?topic_id=502657
        // ive added libnoise.net to my solution folder at \Libnoise.NET  it's MIT license
        // the src is here http://www.codeplex.com/LibNoiseDotNet
        // from http://orbiter.dansteph.com/?news=79  F:\Temp\planettexturegen\Planet Editor
        // Planet Texture Generator by Quick_Nick
        // uses libnoise from 
//        1.How to use the program

//Note: Before using the program please choose the default options that you would like to have ( Options -> Default Options ).

//1.1. Parameters category. Here you can modify the values of different parameters that will be used for planet generation and select the resolution level.

//1.2. Light parameters. Here you can disable/enable lightning, the light angles and light parameters.

//1.3. Gradients. Here you can enter color gradients to specify the way the generated texture will be colored. The first value represents the altitude ( please keep this within [-1,1] ). The following tree values represent the red, green and blue component of the light.
//    To add a gradient first enter a value in the value box then press "Add Point" and select the desired color. If you made a mistake you can delete a gradient using the "Delete point" button.


//    After you have entered the desired parameters you can generate the planet by pressing the "Generate" button. Each planet has a specific seed which you can modify. If you want the seed to change upon every generation check the "Random" checkbox.
//    If you want to quickly change the planet parameters push the "Randomize" button.

//2.Some things you should keep in mind

//2.1. Keep Octave count within the [1,30] range.
//2.2. Have a value entered in every textbox.
//2.3. For the 8192x4096 resolution you will receive and error. Just press cancel. The planet texture was generated so you can still use "Save as .bmp".
//2.4. Have at least two gradients with the values -1 and 1. The associated color components do not matter.
//2.5. Do not have gradients that have exacly the same value.


//3.Legal stuff

//This program uses :
//1.Libnoise library and the Noiseutils library. Both can be found at : http://libnoise.sourceforge.net/
//2.wxWidgets cross-platform GUI library. It can be found at : http://www.wxwidgets.org/

//The program is released under the General Public License.
//The program was made in Visual Studio 2005 Express Edition but it could work on other versions. If you want to compile the source you will need the Libnoise, Noiseutils and wxWidgets libraries. Include noiseutils.h and noiseutils.cpp in the project.

//(c) Iancu Adrian Robert
//    iaro2004@yahoo.com
        private void GenerateTexture()
        {
            //        //TODO: dalea default
            //#include <stdlib.h>
            //#include <process.h>
            //#include <string.h>


            ////TODO: dale libnoise-ului
            //#include "noise/noise.h"
            //#include "noiseutils.h"
            //using namespace noise;

            //// dale wx-ului
            //#include "wx/wxprec.h"
            //#ifndef WX_PRECOMP
            //       #include "wx/wx.h"
            //#endif
            //#include "wx/image.h"
            //#include "wx/colordlg.h"
            //#include "wx/process.h"

            //// dale mele
            //#include "pled.h"


            ////wxFrame *frame;

            //IMPLEMENT_APP(pled)

            //// <=> cu main() in programe normale
            //bool pled::OnInit()
            //{

            ////fereastra principala. Poate avea status bar,toolbar & alte abureli.

            //wxFrame *frame = new Frame( "Planet Editor", wxPoint(0,0), wxSize(1024,768),
            //wxDEFAULT_FRAME_STYLE & ~ (wxMAXIMIZE_BOX)// |( wxRESIZE_BORDER )
            //                          );
            //frame->Show(TRUE);
            //SetTopWindow(frame);


            //return true;

            //}

            ////constructoru la clasa frame
            //Frame::Frame(const wxString& title, const wxPoint& pos, const wxSize& size,long style)
            //: wxFrame((wxFrame *)NULL, -1, title, pos, size,style)
            //{


            //FILE *f;
            //f=fopen("config.txt","r");

            //if (f != NULL)
            //{
            //    //fscanf(f,"%s\n",&orbpath);
            //    //if (orbpath != NULL) orbiter_path.Printf("%s",orbpath);
            //    fscanf(f,"%d\n",&def2);
            //    fscanf(f,"%d\n",&def31);
            //    fscanf(f,"%d\n",&def32);
            //    fscanf(f,"%d\n",&def4);
            //}

            //fclose(f);


            //wxMenu *file = new wxMenu;				//meniu efectiv ( in cazu asta e File )
            //file->Append( IDSaveBmp, "Save as .bmp" );
            //file->Append( IDSaveTex, "Save as .tex" );
            //file->Append( ID_Quit,   "Exit" );			//exit

            //wxMenu *options = new wxMenu;			
            //options->Append( IDDefault, "Default settings" );

            //wxMenu *help = new wxMenu;		
            //help->Append( IDReadme, "Readme" );
            //help->Append( IDAbout , "About " );


            //wxMenuBar *menuBar = new wxMenuBar;		//declara bara de meniuri
            //menuBar->Append( file   , "&File" );		//prinde File in bara de meniuri
            //menuBar->Append( options, "&Options" );
            //menuBar->Append( help   , "&Help" );
            //SetMenuBar( menuBar );

            ////deocamdata nu trebe
            ////CreateStatusBar();			//bara aia de jos care nu ajuta la nimic
            ////SetStatusText( "?" );

            //panel = new wxPanel(this,-1);	//panou principal, thisu = framu = parintele

            ////incarca bitmap-u...
            //bitmap = new wxBitmap(_T("splash.bmp"), wxBITMAP_TYPE_BMP);

            ////ceva vodoo pt resize
            //image1 = bitmap->ConvertToImage();
            //image1.Rescale(1000,500);
            //delete bitmap;
            //bitmap = new wxBitmap(image1,-1);

            ////adauga canvas ( numa pe dasta se poate desena ), panza & culori
            ////vezi OnPaint [ metoda a Canvas ]
            //canvas = new Canvas( panel, wxID_ANY, wxPoint(8,208),
            //                    wxSize(bitmap->GetWidth(),bitmap->GetHeight()) );


            ////DE AICI INCEP BUTOANE, LISTE & TRAPE SECRETE

            //// adauga un buton cu ID-u..., pozitie, marime ....
            ///*
            //button1 = new wxButton(panel, ID_button1, "buton1",wxPoint(128,8), wxSize(64,32), 0);

            //(void)new wxStaticBox( panel, wxID_ANY, _T("&Box around combobox"),
            //                           wxPoint(5, 5), wxSize(150, 100));
            //*/

            ////prima categorie - ...
            //generate  = new wxButton(panel, IDgenerate, "GENERATE ",wxPoint(8,32 ), wxSize(128,48),0);
            //randomize = new wxButton(panel, IDrand    , "RANDOMIZE",wxPoint(8,96 ), wxSize(128,48), 0);
            //ii_random = new wxCheckBox( panel, wxID_ANY, _T("&Random"),   wxPoint(8,160)   );

            //(void)  new wxStaticText(panel, wxID_ANY, _T("Seed   "),wxPoint(12,180 ));
            //seed  = new wxTextCtrl(panel, wxID_ANY, _T("184296 "),wxPoint(48,180 ),wxSize(96,20));


            ////a 2-a categorie - parametri al modulului generator
            //(void)new wxStaticBox( panel, wxID_ANY, _T(" &Parameters "),wxPoint(154,32),wxSize(384,162));

            //(void)new wxStaticText(panel, wxID_ANY, _T("Frequency   "),wxPoint(164,64 ));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Lacunarity  "),wxPoint(164,96 ));
            ////(void)new wxStaticText(panel, wxID_ANY, _T("Stretch     "),wxPoint(330,96 ));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Octave Count"),wxPoint(164,128));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Persistence "),wxPoint(164,160));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Resolution  "),wxPoint(330,64 ));


            //if (def2 == 1)
            //{
            //frequency   = new wxTextCtrl(panel, wxID_ANY, _T(" 1   "),wxPoint(256,64 ),wxSize(64,20));
            //lacunarity  = new wxTextCtrl(panel, wxID_ANY, _T(" 2   "),wxPoint(256,96 ),wxSize(64,20));
            ////stretch     = new wxTextCtrl(panel, wxID_ANY, _T(" 1   "),wxPoint(400,96 ),wxSize(64,20));
            //octave      = new wxTextCtrl(panel, wxID_ANY, _T(" 5.5 "),wxPoint(256,128),wxSize(64,20));
            //persistence = new wxTextCtrl(panel, wxID_ANY, _T(" 0.5 "),wxPoint(256,160),wxSize(64,20));
            //}
            //else
            //{
            //frequency   = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(256,64 ),wxSize(64,20));
            //lacunarity  = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(256,96 ),wxSize(64,20));
            ////stretch   = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(400,96 ),wxSize(64,20));
            //octave      = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(256,128),wxSize(64,20));
            //persistence = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(256,160),wxSize(64,20));
            //}

            //wxString rezuri[] =
            //    {
            //        _T("   64 x 64"),
            //        _T(" 128 x 128"),
            //        _T(" 256 x 256"),
            //        _T(" 512 x 256"),
            //        _T("1024 x 512"),
            //        _T("2048 x 1024"),
            //        _T("4096 x 2048"),
            //        _T("8192 x 4096")
            //    };
            //rezolutie	= new wxComboBox(panel,wxID_ANY, _T(""),
            //                  wxPoint(400,64), wxSize(96, wxDefaultCoord),
            //                              8, rezuri,
            //                              wxCB_READONLY | wxPROCESS_ENTER);
            //rezolutie->SetValue("1024 x 512");

            ////a 3-a categorie - SA FIE LUMINA
            //(void)new wxStaticBox( panel, wxID_ANY, _T(" &Light "),wxPoint(540,32),wxSize(256,162));

            //(void)new wxStaticText(panel, wxID_ANY, _T("Brightness "),wxPoint(550,64 ));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Color      "),wxPoint(550,96 ));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Contrast   "),wxPoint(550,128));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Intensity  "),wxPoint(550,160));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Azimuth    "),wxPoint(660,128));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Elevation  "),wxPoint(660,160));

            //ni_light = new wxCheckBox( panel, wxID_ANY, _T("&Disable light"),   wxPoint(705,64 )   );
            //if (def31 == 0) ni_light->SetValue(true);

            //if (def32 == 1)
            //{
            //brightness  = new wxTextCtrl(panel, wxID_ANY, _T("1.25 "),wxPoint(605,64 ),wxSize(48,20));
            //colorR      = new wxTextCtrl(panel, wxID_ANY, _T("255 "),wxPoint(605,92 ),wxSize(40,20));
            //colorG      = new wxTextCtrl(panel, wxID_ANY, _T("255 "),wxPoint(647,92 ),wxSize(40,20));
            //colorB      = new wxTextCtrl(panel, wxID_ANY, _T("255 "),wxPoint(689,92 ),wxSize(40,20));
            //contrast    = new wxTextCtrl(panel, wxID_ANY, _T("4.0 "),wxPoint(605,128),wxSize(48,20));
            //intensity   = new wxTextCtrl(panel, wxID_ANY, _T("1.0 "),wxPoint(605,160),wxSize(48,20));

            //azimuth  = new wxSlider  ( panel, wxID_ANY, 0, 0, 360,wxPoint(710,112),wxSize(80,40),
            //                             wxSL_AUTOTICKS | wxSL_LABELS);
            //azimuth->SetTickFreq(45, 0); 
            //azimuth->SetValue(45);
            //elevation  = new wxSlider  ( panel, wxID_ANY, 0, 0, 90,wxPoint(710,151),wxSize(80,40),
            //                             wxSL_AUTOTICKS | wxSL_LABELS);
            //elevation->SetTickFreq(45, 0); 
            //elevation->SetValue(70);
            //}
            //else
            //{
            //brightness  = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(605,64 ),wxSize(48,20));
            //colorR      = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(605,92 ),wxSize(40,20));
            //colorG      = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(647,92 ),wxSize(40,20));
            //colorB      = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(689,92 ),wxSize(40,20));
            //contrast    = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(605,128),wxSize(48,20));
            //intensity   = new wxTextCtrl(panel, wxID_ANY, _T(""),wxPoint(605,160),wxSize(48,20));

            //azimuth  = new wxSlider  ( panel, wxID_ANY, 0, 0, 360,wxPoint(710,112),wxSize(80,40),
            //                             wxSL_AUTOTICKS | wxSL_LABELS);
            //azimuth->SetTickFreq(45, 0); 
            //elevation  = new wxSlider  ( panel, wxID_ANY, 0, 0, 90,wxPoint(710,151),wxSize(80,40),
            //                             wxSL_AUTOTICKS | wxSL_LABELS);
            //elevation->SetTickFreq(45, 0); 
            //}


            ////a 4-a categorie - gradientii de culoare, lumini si umbre
            //(void)new wxStaticBox( panel, wxID_ANY, _T(" &Gradients "),wxPoint(798,32),wxSize(215,162));
            //(void)new wxStaticText(panel, wxID_ANY, _T("Value "),wxPoint(800,64 ));
            //val = new wxTextCtrl(panel, wxID_ANY, _T("1.0 "),wxPoint(832,64 ),wxSize(48,20));
            //add = new wxButton(panel, IDadd, "Add point   ",wxPoint(800,96  ), wxSize(80,36),0);
            //del = new wxButton(panel, IDdel, "Delete point",wxPoint(800,144 ), wxSize(80,36),0);
            //box = new wxListBox( panel, wxID_ANY,wxPoint(884,50), wxSize(124,130),0,NULL,wxLB_SORT );


            //if (def4 == 1)
            //{
            //box->Append( _T("-1 0 0 128") );
            //box->Append( _T("-0.25 0 0 255") );
            //box->Append( _T("0 0 128 255") );
            //box->Append( _T("0.0625 240 240 64") );
            //box->Append( _T("0.125 32 160 0") );
            //box->Append( _T("0.375 224 224 0") );
            //box->Append( _T("0.75 128 128 128") );
            //box->Append( _T("1 255 255 255") );
            //nr_gradienti = 8;
            //}
            //else nr_gradienti =0;


            //}


            ////din meniu

            ////file
            //void Frame::OnSaveBmp(wxCommandEvent& WXUNUSED(event))
            //{
            //wxFileDialog sbmp
            //                 (
            //                    this,
            //                    _T("Save as bitmap"),
            //                    wxEmptyString,
            //                    wxEmptyString,
            //                    _T("Bitmap (*.bmp)|*.bmp"),
            //                    wxSAVE|wxOVERWRITE_PROMPT
            //                 );

            //    //sbmp.SetDirectory(wxGetHomeDir());
            //    sbmp.SetFilterIndex(1);

            //    if (sbmp.ShowModal() == wxID_OK)
            //    {

            //        wxString path=_T("cmd /Q /c move tmp.bmp \"");
            //        path.Append(sbmp.GetPath());
            //        path.Append("\"");
            //        wxExecute(path);
            //    }

            //}
            //void Frame::OnSaveTex(wxCommandEvent& WXUNUSED(event))
            //{
            //wxMessageBox("Not implemented yet","Save as .tex", wxOK | wxICON_INFORMATION, this);
            //}
            //void Frame::OnQuit(wxCommandEvent& WXUNUSED(event))		//iesire
            //{
            //Close(TRUE);
            //}


            ////options
            //void Frame::OnDefault(wxCommandEvent& WXUNUSED(event))
            //{
            //FILE *f;
            //f=fopen("config.txt","w");


            ////directoru la orbiter
            ///*
            //wxDirDialog odir(this, _T("Please choose the Orbiter directory"),"",wxDEFAULT_DIALOG_STYLE);

            //    if (odir.ShowModal() == wxID_OK)
            //    {
            //        fprintf(f,"%s\n",odir.GetPath().c_str());
            //    }
            //*/
            ////valori default pentru parametri planeta
            //wxMessageDialog def2( NULL, _T("Add default values for the planet parameters ?"),
            //        _T("Planet parameters"), wxNO_DEFAULT|wxYES_NO|wxICON_QUESTION);

            //    switch ( def2.ShowModal() )
            //    {
            //        case wxID_YES: fprintf(f,"1\n");break;

            //        case wxID_NO:  fprintf(f,"0\n");break;
            //            break;

            //        default: fprintf(f,"0\n");break;

            //    }
            //// sa fie lumina?
            //wxMessageDialog def31( NULL, _T("Enable light by default ?"),
            //        _T("Light"), wxNO_DEFAULT|wxYES_NO|wxICON_QUESTION);

            //    switch ( def31.ShowModal() )
            //    {
            //        case wxID_YES: fprintf(f,"1\n");break;

            //        case wxID_NO:  fprintf(f,"0\n");break;
            //            break;

            //        default: fprintf(f,"0\n");break;

            //    }
            ////valori default pentru parametrii luminii
            //wxMessageDialog def32( NULL, _T("Add default values for the light parameters ?"),
            //        _T("Light parameters"), wxNO_DEFAULT|wxYES_NO|wxICON_QUESTION);

            //    switch ( def32.ShowModal() )
            //    {
            //        case wxID_YES: fprintf(f,"1\n");break;

            //        case wxID_NO:  fprintf(f,"0\n");break;
            //            break;

            //        default: fprintf(f,"0\n");break;

            //    }
            ////sa fie gradienti by default?
            //wxMessageDialog def4( NULL, _T("Add some default gradients?"),
            //        _T("Gradients"), wxNO_DEFAULT|wxYES_NO|wxICON_QUESTION);

            //    switch ( def4.ShowModal() )
            //    {
            //        case wxID_YES: fprintf(f,"1\n");break;

            //        case wxID_NO:  fprintf(f,"0\n");break;
            //            break;

            //        default: fprintf(f,"0\n");break;

            //    }

            //fclose(f);

            //}
            ////help
            //void Frame::OnReadme(wxCommandEvent& WXUNUSED(event))
            //{
            //wxExecute("notepad readme.txt");
            //}
            //void Frame::OnAbout(wxCommandEvent& WXUNUSED(event))
            //{
            //    wxMessageBox("                  Planet Editor version 1.0 \n\nSend comments and suggestions to : iaro2004@yahoo.com","About", wxOK | wxICON_INFORMATION, this);
            //}


            ////din program
            //void Frame::OnClik(wxCommandEvent& WXUNUSED(event))	//etest
            //{
            //str = new wxString();
            //float var1=0;
            //long  var2=0;

            //module::Perlin perlin;


            ////Pt frecventa
            //*str = frequency->GetValue();
            //var1 = wxAtof( (*str) );
            //perlin.SetFrequency(var1);

            ////Pt lacunaritate
            //*str = lacunarity->GetValue();
            //var1 = wxAtof( (*str) );
            //perlin.SetLacunarity(var1);


            ////Pt octave
            //*str = octave->GetValue();
            //var1 = wxAtof( (*str) );
            //perlin.SetOctaveCount(var1);

            ////Pt persistenta
            //*str = persistence->GetValue();
            //var1 = wxAtof( (*str) );
            //perlin.SetPersistence(var1);

            ////Pt seed
            //if (ii_random->GetValue() == false )
            //    {
            //        *str =seed->GetValue();
            //         str->ToLong(&var2);
            //         perlin.SetSeed(var2);
            //    }
            //else	 perlin.SetSeed(rand()%65535);

            ////================================= detalii in plus pt continente ===================

            ////============================ / the rabit jumps over the lazy fox ===================


            //  utils::NoiseMap heightMap;
            //  utils::NoiseMapBuilderSphere heightMapBuilder;
            //  //heightMapBuilder.SetSourceModule (final);
            //    heightMapBuilder.SetSourceModule (perlin);
            //  //heightMapBuilder.SetSourceModule (detalii);
            //  //heightMapBuilder.SetSourceModule (masca);
            //  //heightMapBuilder.SetSourceModule (scale);

            //  heightMapBuilder.SetDestNoiseMap (heightMap);

            ////selectare rezolutie
            ////nu mere cu switchuri

            //  heightMapBuilder.SetDestSize (1024,512);
            //  if ( rezolutie->GetValue()== "   64 x 64"  ) heightMapBuilder.SetDestSize (64  ,64  );
            //  else 
            //  if ( rezolutie->GetValue()== " 128 x 128"  ) heightMapBuilder.SetDestSize (128 ,128 );
            //  else 
            //  if ( rezolutie->GetValue()== " 256 x 256"  ) heightMapBuilder.SetDestSize (256 ,256 );
            //  else 
            //  if ( rezolutie->GetValue()== " 512 x 256"  ) heightMapBuilder.SetDestSize (512 ,256 );
            //  else 
            //  if ( rezolutie->GetValue()== "1024 x 512"  ) heightMapBuilder.SetDestSize (1024,512 );
            //  else 
            //  if ( rezolutie->GetValue()== "2048 x 1024" ) heightMapBuilder.SetDestSize (2048,1024);
            //  else 
            //  if ( rezolutie->GetValue()== "4096 x 2048" ) heightMapBuilder.SetDestSize (4096,2048);
            //  else 
            //  if ( rezolutie->GetValue()== "8192 x 4096" ) heightMapBuilder.SetDestSize (8192,4096);


            //  heightMapBuilder.SetBounds (-90.0, 90.0, -180.0, 180.0);
            //  heightMapBuilder.Build ();

            //  utils::RendererImage renderer;
            //  utils::Image image;
            //  renderer.SetSourceNoiseMap (heightMap);
            //  renderer.SetDestImage (image);
            //  renderer.ClearGradient ();


            //double valoare=0;
            //long   r=0,g=0,b=0;
            //wxString selection,sel;

            //for (int i=0;i<nr_gradienti;i++)
            //{

            //selection = box->GetString(i);

            //sel = selection.BeforeFirst(' ');
            //valoare = wxAtof(sel);
            ////wxMessageBox(sel,"Enter titlu here.", wxOK | wxICON_INFORMATION, this);

            //selection = selection.AfterFirst(' ');
            //sel = selection.BeforeFirst(' ');
            //sel.ToLong(&r);
            ////wxMessageBox(sel,"Enter titlu here.", wxOK | wxICON_INFORMATION, this);

            //selection = selection.AfterFirst(' ');
            //sel = selection.BeforeFirst(' ');
            //sel.ToLong(&g);
            ////wxMessageBox(sel,"Enter titlu here.", wxOK | wxICON_INFORMATION, this);

            //selection = selection.AfterFirst(' ');
            //selection.ToLong(&b);
            ////wxMessageBox(selection,"Enter titlu here.", wxOK | wxICON_INFORMATION, this);

            //renderer.AddGradientPoint (valoare, utils::Color (r,g,b, 255));

            //}


            ////lumina , se iau valorile din categoria 3 si se amesteca bine
            ////brightness colorR colorG colorB contrast intensity ni_light azimuth elevation

            //if (ni_light->GetValue() == false)
            //{
            //    renderer.EnableLight ();

            //    //Pt brightness
            //    renderer.SetLightBrightness(wxAtof(brightness->GetValue()));

            //    //Pt culoare
            //    renderer.SetLightColor(utils::Color (
            //                                    wxAtof( colorR->GetValue() ),
            //                                    wxAtof( colorB->GetValue() ),
            //                                    wxAtof( colorG->GetValue() ),
            //                                    255));
            //    //Pt contrast
            //    renderer.SetLightContrast(wxAtof(contrast->GetValue()));

            //    //Pt intensitate
            //    renderer.SetLightIntensity(wxAtof(intensity->GetValue()));

            //    //Pt azimuth
            //    renderer.SetLightAzimuth(azimuth->GetValue());

            //    //Pt azimuth
            //    renderer.SetLightElev   (elevation->GetValue());
            //    }


            //  renderer.Render ();
            //  utils::WriterBMP writer;
            //  writer.SetSourceImage (image);
            //  writer.SetDestFilename ("tmp.bmp");
            //  writer.WriteDestFile ();

            //sel.Clear();
            //selection.Clear();
            //delete str;

            //delete bitmap;
            //bitmap = new wxBitmap(_T("tmp.bmp"), wxBITMAP_TYPE_BMP);

            ////ceva vodoo pt resize
            //image1 = bitmap->ConvertToImage();
            //image1.Rescale(1000,500);
            //delete bitmap;
            //bitmap = new wxBitmap(image1,-1);

            ////canvas->Redraw_();
            //Refresh();
            //Update();

            //}

            //void Frame::OnRand(wxCommandEvent& WXUNUSED(event))
            //{
            //float f= 0.5 + ((double)rand() / ((double)(RAND_MAX)+(double)(1))) * 2.71;
            //frequency->SetValue(  wxString::Format(wxT("%02f"),f)   );

            //float l= 0.5 + ((double)rand() / ((double)(RAND_MAX)+(double)(1))) * 3;
            //lacunarity->SetValue(  wxString::Format(wxT("%02f"),l)   );

            //float o= 1 + ((double)rand() / ((double)(RAND_MAX)+(double)(1))) * 20.71;
            //octave->SetValue(  wxString::Format(wxT("%02f"),o)   );

            //float p= 0.25 + ((double)rand() / ((double)(RAND_MAX)+(double)(1))) * 0.65;
            //persistence->SetValue(  wxString::Format(wxT("%02f"),p)   );

            //float s=  ((double)rand() / ((double)(RAND_MAX)+(double)(1))) * 4294836225;
            //seed->SetValue(  wxString::Format(wxT("%02f"),s)   );

            //}

            //void Frame::OnAdd(wxCommandEvent& WXUNUSED(event))	//etest
            //{
            //date_culoare = new wxColourData(); 
            //date_culoare->SetChooseFull(true);
            //culoare = new wxColour();

            //wxColourData test;

            //picker = new wxColourDialog(this,date_culoare);
            //picker->SetTitle(_T("Choose the color value"));

            //if (picker->ShowModal() == wxID_OK)
            //    {
            //       str = new wxString();
            //       *str +=val->GetValue();
            //       *date_culoare = picker->GetColourData();
            //       *culoare = date_culoare->GetColour();
            //      (*str) += wxString::Format(wxT(" %02d %02d %02d"),
            //                          culoare->Red(),culoare->Green(),culoare->Blue());
            //       box->Append( (*str) );
            //       nr_gradienti++;
            //       delete str;	   
            //    } 

            //delete date_culoare;
            //delete culoare;
            //delete picker;

            //}

            //void Frame::OnDel(wxCommandEvent& WXUNUSED(event))	//etest
            //{
            //int idu;

            //idu = box->GetSelection();
            //    if ( idu != wxNOT_FOUND ) { box->Delete( idu ); nr_gradienti--; }
            //    else wxMessageBox("No gradient selected","", wxOK | wxICON_INFORMATION, this);

            //}


            ////constructoru pentru Canvas
            //Canvas::Canvas(wxWindow *parent,wxWindowID id, const wxPoint& pos, const wxSize& size):
            // wxScrolledWindow(parent, wxID_ANY, pos, size)
            //{
            //}

            //// Functia de redesenare
            //void Canvas::OnPaint(wxPaintEvent& WXUNUSED(event))
            //{

            //    wxPaintDC dc(this);

            //    if ( bitmap->Ok() )
            //    {
            //        wxMemoryDC memDC;
            //        if ( bitmap->GetPalette() )
            //        {
            //            memDC.SetPalette(*bitmap->GetPalette());
            //            dc.SetPalette	(*bitmap->GetPalette());
            //        }
            //        memDC.SelectObject(*bitmap);

            //        // blit, non-transparent ( ii false la sfarsit )
            //        dc.Blit(0, 0, bitmap->GetWidth(), bitmap->GetHeight(), & memDC, 0, 0, wxCOPY, false);
            //        memDC.SelectObject(wxNullBitmap);

            //        // daca-mi trebe
            //        //dc.DrawText(_T("Text scris peste bitmap"), 65, 65);
            //    }

            //}
        }
    }
}