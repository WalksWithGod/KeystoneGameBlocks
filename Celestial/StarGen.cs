using System;
using Core.Types;

namespace Celestial
{
    public class StarGen
    {

        
        private Random _random;
        private float _percentTypeM, _percentTypeK, _percentTypeG, _percentTypeF, _percentTypeA, _percentTypeB, _percentTypeO;
        
        private float _percentSupergiantI, _percentGiantII, _percentGiantIII, _percentSubgiantIV,
                      _percentDwarfV, _percentSubdwarfVI, _percentWhiteDwarfD, _percentPulsarP,
                      _percentNeutronStarN, _percentBlackHoleB;
        
        
        //Constants for the Default (Realistic) Luminosity Class Ratios. These must equal 100% when added together
        const float RATIO_SUPERGIANT_I = 0.3f;
        const float RATIO_GIANT_II = 0.7f;
        const float RATIO_GIANT_III = 3.25f;
        const float RATIO_SUBGIANT_IV = 3f;
        const float RATIO_DWARF_V = 68.75f; // mainsequence
        const float RATIO_SUBDWARF_VI = 1;
        const float RATIO_WHITEDWARF_D = 24;
        const float RATIO_PULSAR_P = 1;
        const float RATIO_NEUTRONSTAR_N = 1;
        const float RATIO_BLACKHOLE_B = 1;

        //constants for the Default (realistic) Spectral Type ratios
        const float RATIO_TYPE_O = 0.4f;
        const float RATIO_TYPE_B = 0.6f;
        const float RATIO_TYPE_A = 11;
        const float RATIO_TYPE_F = 11;
        const float RATIO_TYPE_G = 12;
        const float RATIO_TYPE_K = 15;
        const float RATIO_TYPE_M = 50;
        
        
        private float[][,] _luminosities = new float[6][,];
        private float[][,] _masses = new float[6][,];
        int[,] _temperatures = new int[,] {{58000, 56000, 54000,51000, 48000, 45000,42000,39000,36000, 33000},
                                         {30000,27000, 24000,21000, 18000, 15000, 14150, 13250, 12000, 10750},
                                          {9500, 9200, 8950, 8700, 8450, 8200, 8000, 7800, 7600,7400},
                                          {7200, 7050, 6850, 6700, 6550,6400,6330,6225,6150,6075},
                                          {6000, 5960, 5920, 5880, 5840,5800,5700,5600,5500,5400},
                                          {5300,5120,4900,4750,4575,4400,4300,4200,4100,4000},
                                           {3900, 3750,3620,3470,3350,3200,3000,2800,2600,2400}};


        
        public StarGen (Random random)
        {
            if (random == null) throw new ArgumentNullException();
            _random = random;

            InitMainSequenceLuminosities();
            InitMainSequenceMassMatrices();
            
            // assign our defaults since for now we don't accept 
            _percentTypeM = RATIO_TYPE_M;
            _percentTypeK = RATIO_TYPE_K;
            _percentTypeG = RATIO_TYPE_G;
            _percentTypeF = RATIO_TYPE_F;
            _percentTypeA = RATIO_TYPE_A;
            _percentTypeB = RATIO_TYPE_B;
            _percentTypeO = RATIO_TYPE_O;
                       
            _percentSupergiantI = RATIO_SUPERGIANT_I;
            _percentGiantII = RATIO_GIANT_II;
            _percentGiantIII = RATIO_GIANT_III;
            _percentSubgiantIV = RATIO_SUBGIANT_IV;
            _percentDwarfV = RATIO_DWARF_V;
            _percentSubdwarfVI = RATIO_SUBDWARF_VI;
            _percentWhiteDwarfD = RATIO_WHITEDWARF_D;
            _percentPulsarP = RATIO_PULSAR_P;
            _percentNeutronStarN = RATIO_NEUTRONSTAR_N;
            _percentBlackHoleB = RATIO_BLACKHOLE_B;
            
        }
        
        public void ComputePrimaryStatistics(Star newstar)
        {
            newstar.LuminosityClass = GetLuminosityClass();
            newstar.SpectralType = GetSpectralType(newstar.LuminosityClass);
            newstar.SpectralSubType = GetSpectralSubType(newstar.SpectralType);
           
            // the following stats are all based on having got the above however it's also possible to simply
            // start with the mass and then determine a luminosity and spectral types 
            newstar.Temperature = GetStarTemperature(newstar.LuminosityClass, newstar.SpectralType, newstar.SpectralSubType);
            newstar.Luminosity = GetStarLuminosity(newstar.LuminosityClass, newstar.SpectralType, newstar.SpectralSubType);
            newstar.Mass = GetStarMass(newstar.LuminosityClass, newstar.SpectralType, newstar.SpectralSubType);
            newstar.Diameter = GetStarDiameterAU(newstar.Luminosity, newstar.Temperature);
            // arrStars(b).StableLifeSpan = GetStarStableLifespan(arrStars(b).Mass, arrStars(b ).Luminosity)
            newstar.Age = GetStarAge(newstar.Age);
        }
        
        public void ComputeCompanionStatistics(Star companion, Star primary)
        {
            // companions of a white dwarf are also automatically white dwarfs
            if (primary.LuminosityClass == LUMINOSITY.WHITEDWARF_D)
                companion.LuminosityClass = LUMINOSITY.WHITEDWARF_D;
            else
                companion.LuminosityClass = GetCompanionLuminosityClass(primary.LuminosityClass);
            
            
            companion.SpectralType =
                GetCompanionSpectralType(companion.LuminosityClass, primary.LuminosityClass,
                                         primary.SpectralType);
            companion.SpectralSubType =
                GetCompanionSpectralSubType(companion.LuminosityClass, primary.LuminosityClass,
                                            companion.SpectralType, primary.SpectralType,
                                            primary.SpectralSubType);
            companion.Temperature =
                GetStarTemperature(companion.LuminosityClass, companion.SpectralType,
                                   companion.SpectralSubType);
            companion.Luminosity =
                GetStarLuminosity(companion.LuminosityClass, companion.SpectralType,
                                  companion.SpectralSubType);
            companion.Mass =
                GetStarMass(companion.LuminosityClass, companion.SpectralType,
                            companion.SpectralSubType);
            companion.Diameter = GetStarDiameterAU(companion.Luminosity, companion.Temperature);
            // arrStars(b + l).StableLifeSpan = GetStarStableLifespan(arrStars(b + l).Mass, arrStars(b + l).Luminosity)
            companion.Age = GetStarAge(companion.Age);
        }

        // this function returns the Luminosity class for a created star.  It uses the values in
        // mvarPercentSuperGiantI - mvarPercentBlackHoles to influence what types of systems are created.
        public LUMINOSITY GetLuminosityClass()
        {
            LUMINOSITY result = LUMINOSITY.DWARF_V;
            float i = _random.Next(1, 100);
            
            //  Generate random single between 1 and 100.
            if (i <= _percentSupergiantI)
                result = LUMINOSITY.SUPERGIANT_I;
            else if (i <= (_percentSupergiantI + _percentGiantII))
                result = LUMINOSITY.GIANT_II;
            else if (i <= (_percentSupergiantI + _percentGiantII + _percentGiantIII))
                result = LUMINOSITY.GIANT_III;
            else if (i <= (_percentSupergiantI + _percentGiantII + _percentGiantIII + _percentSubgiantIV))
                result = LUMINOSITY.SUBGIANT_IV;
            else if (i <= (_percentSupergiantI + _percentGiantII + _percentGiantIII + _percentSubgiantIV + _percentDwarfV))
                result = LUMINOSITY.DWARF_V;
            else if (i <= (_percentSupergiantI + _percentGiantII + _percentGiantIII + _percentSubgiantIV
                        + _percentDwarfV + _percentSubdwarfVI))
            {
                result = LUMINOSITY.SUBDWARF_VI;
            }
            else if (i <= (_percentSupergiantI+ _percentGiantII+ _percentGiantIII+ _percentSubgiantIV+ _percentDwarfV
                        + _percentSubdwarfVI + _percentWhiteDwarfD))
            {
                result = LUMINOSITY.WHITEDWARF_D;
            }
            else if (i<= (_percentSupergiantI+ _percentGiantII+ _percentGiantIII+ _percentSubgiantIV+ _percentDwarfV
                        + _percentSubdwarfVI+ _percentWhiteDwarfD + _percentPulsarP))
            {
                result = LUMINOSITY.PULSAR_P;
            }
            else if (i<= (_percentSupergiantI+ _percentGiantII+ _percentGiantIII+ _percentSubgiantIV+ _percentDwarfV
                        + _percentSubdwarfVI+ _percentWhiteDwarfD+ _percentPulsarP + _percentNeutronStarN))
            {
                result = LUMINOSITY.NEUTRON_N ;
            }
            else if (i<= (_percentSupergiantI+ _percentGiantII+ _percentGiantIII+ _percentSubgiantIV+ _percentDwarfV
                        + _percentSubdwarfVI+ _percentWhiteDwarfD+ _percentPulsarP+ _percentNeutronStarN + _percentBlackHoleB))
            {
                result = LUMINOSITY.BLACKHOLE_B;
            }
            return result;
        }

        LUMINOSITY GetCompanionLuminosityClass(LUMINOSITY PrimaryLuminosityClass)
        {
            // since its important that companion stars are either the SAME or LOWER luminosity class
            // as the Primary star in that system, we will calculate their luminosity class in this
            // dedicated function instead of using the same one that we use to calc the Primary's luminosity
            LUMINOSITY result;
            float n;
            if (PrimaryLuminosityClass == LUMINOSITY.WHITEDWARF_D)
                result = LUMINOSITY.WHITEDWARF_D;
            else
            {
                n = (float)_random.NextDouble();
                if (n <= 0.666)
                    result = PrimaryLuminosityClass;
                else if (n <= 0.166)
                    result = (PrimaryLuminosityClass + 1);
                else
                    result = (PrimaryLuminosityClass + 2);

            }
            // check to make sure we havent fallen off the scale of luminosity types
            // and if so then it becomes a white dwarf automatically
            if (result > LUMINOSITY.SUBDWARF_VI)
                result = LUMINOSITY.WHITEDWARF_D;

            return result;
        }

        SPECTRAL_TYPE  GetSpectralType(LUMINOSITY LuminosityClass)
        {
            // This function returns the Spectral Type of a star
            // based on the stars Luminosity_Class
            // Modifier is a percent value to determine likelihood of the star being
            // more earthlike
            SPECTRAL_TYPE result;
            // First determine if we are dealing with a black hole, white dwarf, pulsar or neutron star
            switch (LuminosityClass)
            {
                case LUMINOSITY.WHITEDWARF_D:
                case LUMINOSITY.PULSAR_P:
                case LUMINOSITY.NEUTRON_N:
                case LUMINOSITY.BLACKHOLE_B:
                    return SPECTRAL_TYPE.SPECIAL ;
            }
            // since we are not a special star (i.e. neutron, pulsar, white dwarf or blackhole)
            // we can continue and get the SpectralType
            int i = _random.Next(1, 100);
            //  Generate random single between 1 and 100.
            if (i <= _percentTypeM )
                result = SPECTRAL_TYPE.M;
            else if (i <= _percentTypeM + _percentTypeK)
                result = SPECTRAL_TYPE.K;
            else if (i <= _percentTypeM + _percentTypeK + _percentTypeG)
                result = SPECTRAL_TYPE.G;
            else if (i <= _percentTypeM + _percentTypeK + _percentTypeG + _percentTypeF)
                result = SPECTRAL_TYPE.F;
            else if (i <= _percentTypeM + _percentTypeK + _percentTypeG + _percentTypeF + _percentTypeA)
                result = SPECTRAL_TYPE.A;
            else if (i <= _percentTypeM + _percentTypeK + _percentTypeG + _percentTypeF + _percentTypeA + _percentTypeB)
                result = SPECTRAL_TYPE.B;
            else
                result = SPECTRAL_TYPE.O;
                // todo: i think only main sequence and supergiants can be type O
                // B types might have similar restrictions

            return result;
            
        }

        SPECTRAL_TYPE GetCompanionSpectralType(LUMINOSITY LuminosityClass, LUMINOSITY PrimaryLuminosityClass, SPECTRAL_TYPE PrimarySpectralType)
        {
            // if the companion is the same Luminosity as the parent, then the likelihood of them being the
            // same spectral type is fairly high.  I
            // HOWEVER, if the Luminosity classes between the companion and Primary are different
            // then  we can calculate their spectral type just as if they were primary stars themselves
            float n;
            SPECTRAL_TYPE result;
            if (LuminosityClass == PrimaryLuminosityClass)
            {
                n = (float)_random.NextDouble();
                if (n <= 0.5)
                    result = PrimarySpectralType;
                else if (n <= 0.666)
                    result = (PrimarySpectralType + 1);
                else if (n <= 0.833)
                    result = (PrimarySpectralType + 2);
                else
                    result = (PrimarySpectralType + 3);

                // check to see that we havent fallen off the scale
                if (result >= SPECTRAL_TYPE.M )
                    result = SPECTRAL_TYPE.M;
            }
            else
            {
                //  since the primary and companion are of different luminosity class
                //  we can use GetSpectralType function to find its spectral type
                result = GetSpectralType(LuminosityClass);
            }
            return result;
        }

        SPECTRAL_SUB_TYPE GetSpectralSubType(SPECTRAL_TYPE SpectralType)
        {
            // Spectral Subtype is a measure of a stars brightness.
            // The range is 0 thru 9 with 0 being brighter than 9
            int i;
            // //0 is the hottest in a class and 9 is the coolest
            // O0 stars are the hottest stars there is
            // //there are no restrictions in generating subtypes EXCEPT that there
            //  are no O stars with subtype 4 or earlier or any type of O star of class III
            if (SpectralType == SPECTRAL_TYPE.O)
                // there are no O4, O3,O2,O1,or O0 stars
                i = _random.Next(5, 9); // generate value between 5 and 9
            else
                i = _random.Next(0, 9); //  Generate random value between 0 and 9.

            return  (SPECTRAL_SUB_TYPE) i;
        }

        SPECTRAL_SUB_TYPE GetCompanionSpectralSubType(LUMINOSITY LuminosityClass, LUMINOSITY PrimaryLuminosityClass, 
                                                      SPECTRAL_TYPE SpectralType, SPECTRAL_TYPE PrimarySpectralType, 
                                                      SPECTRAL_SUB_TYPE PrimarySpectralSubType)
        {
            // if the Primary and Companion are of the same Luminosity class AND spectral type, then
            // the companion cannot be of a HIGHER spectral SUB type.
            SPECTRAL_SUB_TYPE result = GetSpectralSubType(SpectralType);
            
            // check to see if the Companion and Primary are of the same luminosity class and spectral type
            if (LuminosityClass == PrimaryLuminosityClass && SpectralType == PrimarySpectralType)
            {
                // since they are of the same luminosity class and spectraltype, then we
                // make sure our companion is not of a higher subtype then the primary
                // REMEMBER than 0 is HIGHER (and hotter) than 9 (coolest) when talking about star sub types
                if (result < PrimarySpectralSubType)
                    result = PrimarySpectralSubType;
            }
            return result;
        }
        
        // returns result in m/s^2
        // gravity effect is always dependant on distance.  That is why typically the only gravity
        // people care about is surface gravity.  Otherwise for something in orbit, we need to compute a different gravity value
        double  GetSurfaceGravity ( Body body )
        {
            const double G = 6.67428E-11;
            return G * body.Mass / Math.Pow((body.Diameter/2), 2);
        }
        
        // this presumes a perfectly circular orbit
        double GetOrbitalVelocity( Body body1, Body body2)
        {
            const double G = 6.67428E-11;
            double r = Vector3d.GetDistance3d(body1.Translation, body2.Translation);
            return  Math.Sqrt(G * (body1.Mass  + body2.Mass ) /  Math.Pow(r,2));
            
        }
        
        // distance in AU 
        float GetOrbitalEccentricity(float distance)
        {
            return 1.0f; // for now simply use perfect circle.  
            
            // this function determines the orbital eccentricty of the star or how
            // far from being a perfect circle its orbit is.  The closer to 0 the number the
            // more perfect the circle.  The closer to 1 the more eliptical
            float result;
            // First check to see if the star's distance is 0
            if (distance == 0)
                return 0;

            float n = (float) _random.NextDouble();
            // apply modifiers based on distance
            if (distance < 0.06)
                // in AU
                n = (n - 0.4f);
            else if (distance < 0.5)
                n = (n - 0.25f);
            else if (distance <= 2)
                n = (n - 0.125f);
            // //now find our actual eccentricity
            if (n <= 0.0625)
                result = 0.05f;
            else if (n <= 0.125)
                result = 0.1f;
            else if (n <= 0.19)
                result = 0.2f;
            else if(n <= 0.25)
                result = 0.3f;
            else if (n <= 0.375)
                result = 0.4f;
            else if (n <= 0.5875)
                result = 0.5f;
            else if (n <= 0.7125)
                result = 0.6f;
            else if (n <= 0.8375)
                result = 0.7f;
            else if (n <= 0.9)
                result = 0.8f;
            else if (n <= 0.9625)
                result = 0.9f;
            else
                result = 0.95f;
            
            return result;
        }

        

        int GetStarTemperature(LUMINOSITY LuminosityClass, SPECTRAL_TYPE SpectralType, SPECTRAL_SUB_TYPE SubType)
        {
            // This function calculates the effective temperature in Kelvin, of a star
            // based on its Luminosity Class, Spectral Type and Spectral SubType
            int result;
            
            switch (LuminosityClass)
            {
                case LUMINOSITY.WHITEDWARF_D:
                    result = _random.Next(4000, 8500); // these will be anywhere between 8500 and 4,000 Kelvin
                    break;
                case LUMINOSITY.NEUTRON_N:
                    result = _random.Next(2000, 4000); // these will not be that hot since neutron stars are only a few miles across
                    break;
                case LUMINOSITY.PULSAR_P:
                    result = _random.Next(3000, 4500); // these are advanced stage neutron stars so maybe a little hotter?
                    break;
                case LUMINOSITY.BLACKHOLE_B:
                    result = 0; // since this radiates 0 energy, there is no heat?
                    break;
                default:
                    result = _temperatures [(int)SpectralType, (int) SubType];
                    // these are 0 based arrays and SpectralType goes from 0 to 6.  No need to subtract from SubType since its both 0 -9 in and outside of the array
                    break;
            }
            return result;
        }

        float GetStarLuminosity(LUMINOSITY LuminosityClass, SPECTRAL_TYPE SpectralType, SPECTRAL_SUB_TYPE SubType)
        {
            float result;
            switch (LuminosityClass)
            {
                case LUMINOSITY.WHITEDWARF_D:
                    result = _random.Next(4000, 8500);
                    break;
                case LUMINOSITY.NEUTRON_N:
                    result = _random.Next(2000, 4000);
                    break;
                case LUMINOSITY.PULSAR_P:
                    result = _random.Next(3000, 4500);
                    break;
                case LUMINOSITY.BLACKHOLE_B:
                    result = 0.0001f;
                    break;
                case LUMINOSITY.SUPERGIANT_I:
                case LUMINOSITY.GIANT_II:
                case LUMINOSITY.GIANT_III:
                case LUMINOSITY.SUBGIANT_IV:
                case LUMINOSITY.DWARF_V:
                    result = _luminosities[(int)LuminosityClass][(int)SpectralType, (int)SubType];
                    break;
                default: //LUMINOSITY.SUBDWARF_VI:
                    result = _luminosities[(int)LuminosityClass][(int)SpectralType, (int)SubType];
                    break;
            }
            result *= (_random.Next(85, 105) / 100);
            return result;
        }

        uint GetStarDiameterAU(float luminosity, int temperature)
        {
            // This function accepts the Luminosity of the star
            // and the Effective temperature and computes the
            // diameter of the star in Astronomical Units (AU)
            // Almost all main-sequence (Class V) stars will be a fraction of an AU
            // in diameter, while giant stars may be MUCH larger
            // Do a quick divide by zero check on lum
            if (luminosity == 0)
                luminosity = 1;
            
            if (temperature == 0)
                temperature = 1;

            return (uint)(309000 * (Math.Pow(luminosity , 1/2) / (temperature * temperature)));

        }

        uint GetStarMass(LUMINOSITY luminosityClass, SPECTRAL_TYPE spectralType, SPECTRAL_SUB_TYPE subType)
        {
            // This function accepts the luminosity, spectral type and subtype and determines the Mass
            float result;
            switch (luminosityClass)
            {
                case LUMINOSITY.WHITEDWARF_D:
                    result = _random.Next(4000, 8500);
                    break;
                case LUMINOSITY.NEUTRON_N:
                    result = _random.Next(2000, 4000);
                    break;
                case LUMINOSITY.PULSAR_P:
                    result = _random.Next(3000, 4500);
                    break;
                case LUMINOSITY.BLACKHOLE_B:
                    result = 0.0001f;
                    break;
                case LUMINOSITY.SUPERGIANT_I:
                case LUMINOSITY.GIANT_II:
                case LUMINOSITY.GIANT_III:
                case LUMINOSITY.SUBGIANT_IV:
                case LUMINOSITY.DWARF_V:
                    result = _masses[(int)luminosityClass][(int)spectralType , (int)subType];
                    break;

                default: // LUMINOSITY.SUBDWARF_VI:
                    result = _masses[(int)luminosityClass][(int)spectralType , (int)subType];
                    break;
            }
            result *= _random.Next(85, 105) / 100f;
            return (uint)result;
        }

        float GetStarStableLifespan(int mass, float luminosity)
        {
            // This function accepts the mass and luminosity of a star
            // and from it calculates the Stable Lifespan of that star
            return (10 * mass) / luminosity;
        }

        float GetStarAge(float stableLifespan)
        {
            // This function takes the Stars Stable Lifespan and computes a statistically random age for that star in billions of years
            int baseage;
            int i = _random.Next(1, 100);
            const float modifier = 0.5f;
            
            // generate number between 1 and 100
            if (i <= 25)
                baseage = 0;
            else if (i <= 50)
                baseage = 3;
            else if (i <= 75)
                baseage = 6;
            else if (i <= 94)
                baseage = 9;
            else
                baseage = 12;

            i = _random.Next(1, 5); // generate number between 1 and 5
            return baseage + (i * modifier);
        }

        // create one for each of the 6 main sequence Luminosity Class
        private void InitMainSequenceLuminosities()
        {
            _luminosities[0] = new float[,]
                {
                    {1647400, 1537920, 1428440, 1318960, 1209480, 1100000, 990520, 881040, 771560, 662080},
                    {552600, 443120, 333640, 224160, 114680, 52000, 48600, 45200, 41800, 38400},
                    {35000, 35000, 35000, 35000, 35000, 35000, 34250, 33500, 32750, 32000},
                    {32000, 32000, 32000, 32000, 32000, 32000, 31700, 31400, 31000, 30500},
                    {30000, 29800, 29600, 29400, 29200, 29000, 29000, 29000, 29000, 29000},
                    {29000, 30000, 32000, 34000, 36000, 38000, 38750, 39250, 40000, 40500},
                    {41000, 90000, 150000, 220000, 260000, 300000, 375000, 425000, 500000, 575000},
                };

            // Luminsoity Classes for Giants_II
            _luminosities[1] = new float[,]
                {
                    {164740, 153792, 142844, 131896, 120948, 110000, 99052, 88104, 77156, 66208},
                    {55260, 44312, 33364, 22416, 11468, 52000, 48600, 45200, 41800, 3840},
                    { 3500, 3500, 3500, 3500, 3500, 3500, 3425, 3350, 3275, 3200},
                    {3200, 3200, 3200, 3200, 3200, 3200, 3170, 3140, 3100, 3050},
                    {3000, 2980, 2960, 2940, 2920, 2900, 2900, 2900, 2900, 2900},
                    {2900, 3000, 3200, 3400, 3600, 3800, 3875, 3925, 4000, 4050},
                    { 4100, 9000, 15000, 22000, 26000, 30000, 37500, 42500, 50000, 57500}};


            //    Luminosity Classes for Giants_III
            _luminosities[2] = new float[,]{
             {449100, 419280, 389460, 359640, 329820, 300000, 270180, 240360, 210540, 180720}, 
             {150900, 121080, 91260, 61440, 31620, 1800, 1461.2f, 1122.4f, 783.6f, 444.8f}, 
             {106, 93.4f, 80.8f, 68.2f, 55.6f, 43, 38.4f, 33.8f, 29.2f, 24.6f},
             { 20, 19.4f, 18.8f, 18.2f, 17.6f, 17, 20.4f, 23.8f, 27.2f, 30.6f},
             { 34, 35.8f, 37.6f, 39.4f, 41.2f, 43, 46.4f, 49.8f, 53.2f, 56.6f},
             { 60, 92, 124, 156, 188, 220, 245, 270, 295, 315},
             { 330, 450, 570, 690, 810, 930, 986, 1043, 1100, 1157}
                };

            //    Luminosity Classes for SubGiants IV
            _luminosities[3] = new float[,]{
                { 44910, 41928, 38946, 35964, 32982, 30000, 27018, 24036, 21054, 18072},
                {15090, 12108, 9126, 6144, 3162, 180f, 146.12f, 112.24f, 78.36f, 44.48f},
                {10.6f, 9.34f, 8.08f, 6.82f, 5.56f, 4.3f, 3.84f, 3.38f, 2.92f, 2.46f},
                { 2f, 1.94f, 1.88f, 1.82f, 1.76f, 1.7f, 2.04f, 2.38f, 2.72f, 3.06f},
                {3.4f, 3.8f, 3.6f, 3.4f, 4.2f, 4, 4.4f, 4.8f, 5.2f, 5.6f},
                { 6, 9, 12, 15, 18, 22, 24, 27, 29, 31},
                { 33, 45, 57, 69, 81, 93, 98, 104, 110, 115}
                };

            //    Luminosity Classes for Main Sequence V
            _luminosities[4] = new float[,]{
            { 1184585, 1105668, 1026751, 947834, 868917, 790000, 711083, 632166, 553249, 474332}
            , {395415, 316498, 237581, 158664, 79747, 830, 674.8f, 519.6f, 364.4f, 209.2f}
            , {54, 46, 38, 30, 22, 14, 12.5f, 11, 9.5f, 8}
            , {6.5f, 5.78f, 5.06f, 4.34f, 3.62f, 2.9f, 2.62f, 2.34f, 2.06f, 1.78f}
            ,{ 1.5f, 1.358f, 1.216f, 1.074f, 0.932f, 0.79f, 0.716f, 0.642f, 0.568f, 0.494f}
            , {0.42f, 0.366f, 0.312f, 0.258f, 0.204f, 0.15f, 0.1326f, 0.1152f, 0.0978f, 0.0804f}
            , {0.063f, 0.0526f, 0.0422f, 0.0318f, 0.0214f, 0.011f, 0.0077f, 0.0043f, 0.001f, 0.0005f}
                };

            //    Luminosity Classes for Subdwarfs VI
            _luminosities[5] = new float[,]
                {
                    {118458, 110566, 102675, 94783, 86891, 79000, 71108, 63216, 55324, 47433},
                    { 39541, 31649, 23758, 15866, 7974, 83, 67.8f, 51.6f, 36.4f, 20.2f},
                    { 5, 4, 3, 3, 2, 1, 1.5f, 1, 0.95f, 0.8f},
                    { 0.65f, 0.578f, 0.506f, 0.434f, 0.362f, 0.29f, 0.262f, 0.234f, 0.206f, 0.178f},
                    { 0.15f, 0.1358f, 0.1216f, 0.1074f, 0.0932f, 0.079f, 0.0716f, 0.0642f, 0.0568f, 0.0494f},
                    {0.042f, 0.0366f, 0.0312f, 0.0258f, 0.0204f, 0.015f, 0.01326f, 0.01152f, 0.00978f, 0.00804f},
                    { 0.0063f, 0.00526f, 0.00422f, 0.00318f, 0.00214f, 0.0011f, 0.00077f, 0.00043f, 0.0001f, 0.00005f}
                };
        }

        private void InitMainSequenceMassMatrices()
        {
            _masses[0] = new float[,]
            {
                { 76, 75, 74, 73, 72, 70, 99, 88, 77, 66}
                , {55, 44, 33, 22, 11, 52, 48, 12, 12, 12}
                , {12, 12, 12, 12, 12, 12, 12, 12, 12, 12}
                , {12, 12, 12, 12, 12, 12, 12, 12, 12, 12}
                , {12, 12, 12, 12, 12, 12, 12, 12, 12, 12}
                , {12, 12, 12, 12, 12, 12, 12, 12, 12, 12}
                , {12, 12, 15, 22, 26 , 19, 20.5f, 22.25f, 23, 24}
            };
            //Masses for Giants_II
            _masses[1] = new float[,]
            {
                    {15, 15, 14, 13, 12, 11, 9, 8, 7, 6}
                    , {5, 4, 3, 2, 1, 5, 4, 4, 4, 3}
                    , {3, 3, 3, 3, 3, 3, 3, 3, 3, 3}
                    , {3, 3, 3, 3, 3, 3, 3, 3, 3, 3}
                    , {3, 2, 2, 2, 2, 2, 2, 2, 2, 2}
                    , {2, 3, 3, 3, 3, 3, 3, 3, 4, 4}
                    , {4, 9, 1f, 2.2f, 2.6f, 3f, 3.75f, 4.25f, 5, 5.7f}
            };

            //Masses for Giants_III
            _masses[2] = new float[,]
                {
                    {9, 8, 7.8f, 7.6f, 7.2f, 7, 6.3f, 6.1f, 5.4f, 5}
                        , {4, 2, 2, 2, 2, 2, 2.2f, 2.4f, 2.6f, 2.8f}
                        , {2, 2.4f, 2.8f, 2.2f, 1.6f, 2, 1.4f, 1.8f, 1.2f, 1.6f}
                        , {1, 1.4f, 1.8f, 1.2f, 1.6f, 1, 1.4f, 1.8f, 1.2f, 1.6f}
                        , {1, 1.8f, 1.6f, 1.4f, 1.2f, 2, 2.4f, 2.8f, 2.2f, 2.6f}
                        , {2, 2, 2, 1, 1, 1, 1, 1, 1, 1}
                        , {1, 1, 1, 2, 2, 2, 2, 2, 2, 2}
                };

            //Masses for SubGiants IV
            _masses[3] = new float[,]
                {
                        { 20, 19, 18, 18, 18, 12, 12, 12, 12, 12}
                        , {12, 12, 12, 12, 11, 11, 11.12f, 11.24f, 11.36f, 1.48f}
                        , {11.6f, 9.34f, 8.08f, 6.82f, 5.56f, 4.3f, 3.84f, 3.38f, 2.92f, 2.46f}
                        , {2f, 1.94f, 1.88f, 1.82f, 1.76f, 1.7f, 2.04f, 2.38f, 2.72f, 3.06f}
                        , {3.4f, 3.8f, 3.6f, 3.4f, 4.2f, 4, 4.4f, 4.8f, 5.2f, 5.6f}
                        , {6, 9, 0.12f, 0.15f, 0.18f, 0.22f, 0.24f, 0.27f, 0.29f, 0.31f}
                        , {0.33f, 0.45f, 0.57f, 0.69f, 0.81f, 0.93f, 0.98f, 1.04f, 1.1f, 1.15f}
                };
            //Masses for Main Sequence V
            _masses[4] = new float[,]
                {
                        { 35, 34, 34, 34, 33, 33, 33, 32, 32, 32}
                        , {31, 31, 31, 31, 31, 30, 30.8f, 30.6f, 30.4f, 30.2f}
                        , {5, 5, 5, 5, 5, 5, 5.5f, 5, 9.5f, 2}
                        , {2.5f, 2.78f, 2.06f, 2.34f, 2.62f, 2.9f, 1.62f, 1.34f, 1.06f, 1.78f}
                        , {1.5f, 1.358f, 1.216f, 1.074f, 0.932f, 0.79f, 0.716f, 0.642f, 0.568f, 0.494f}
                        , {0.42f, 0.366f, 0.312f, 0.258f, 0.204f, 0.15f, 0.1326f, 0.1152f, 0.978f, 0.804f}
                        , {0.63f, 0.526f, 0.422f, 0.318f, 0.214f, 0.1f, 0.77f, 0.43f, 0.1f, 0.063f}
                };

            //Masses for Subdwarfs VI
            _masses[5] = new float[,]
                {
                       { 5, 1, 1, 2, 1, 2, 1, 3, 2, 3}
                        , {3, 3, 2, 1, 4, 3, 7.8f, 1.6f, 6.4f, 0.2f}
                        , {5, 4, 3, 3, 2, 1, 1.5f, 1, 0.95f, 0.8f}
                        , {0.65f, 0.578f, 0.506f, 0.434f, 0.362f, 0.29f, 0.262f, 0.234f, 0.206f, 0.178f}
                        , {0.15f, 0.1358f, 0.1216f, 0.1074f, 0.0932f, 0.079f, 0.0716f, 0.0642f, 0.0568f, 0.0494f}
                        , {0.042f, 0.0366f, 0.0312f, 0.0258f, 0.0204f, 0.015f, 0.01326f, 0.0152f, 0.0978f, 0.0804f}
                        , {0.063f, 0.0526f, 0.0422f, 0.0318f, 0.0214f, 0.011f, 0.0077f, 0.0043f, 0.001f, 0.005f}
                };
        }
    }
}
