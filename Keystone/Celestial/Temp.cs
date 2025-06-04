using System;

namespace Keystone.Celestial
{
    public class Temp
    {
        // TODO: i think the max for any dimension should be 100 000 000 000 000 meters = 668.458134 Astronomical Units 
        // in our companion star calcs, the max distance of a companion star is 50AU * 12 = 600 AU.
        // page 49
        // a decimal datatype would definetly be large enough as it can hold over 8.40646793e12 light years!

        //  NOTE Max visilbe range should be PER object based on its screenspace, not a general precomputed value
        //   public static float max_visible_range = farplane*100000; // translates into 1 billion meters

        //'Private _translation As Vector3d 
        //'Private _rotation As Rotation

		public const double GRAVCONST = 6.67408E-11;
        public  const double SOL_MASS_KILOGRAMS = 1.98892E+30d;

        //  If you would accelerate to the 99,99% of the speed light in just 1 second, you would experience a G-force of aprox. 30,600,000 g's

        private float _diamter; // sun is 1,390,000,000 meters
        private float _surfaceTemperature; // sun is 5800 K


     

        
        // Decimal variables can hold values up to 7.9228162514264337593543950335E+28,  29 digits of precision
        //       decLightYear = 9424667196000000; // meters 1 LY uses 16 significant digits. 
        //       Decimals could easily support a universe with 1/10 meter resolution that is thousands of lightyears across
        //       at 1 tvunit = 1 meter, decomal datatype would allow for a universe over 8.40646793e12 light years across!
        // Single variables can hold up to 3.4028235E+38,             7 digits of precision
        // Double variables can hold up to 1.79769313486231570E+308.  15 digits of precision
        //       a double would still be very good for 1/10 meter resolution of a light year.
        //
        //       Consider a ship at 1000 light years from the origin and thus 9424667196000000000 meters away 
        //       To find the distance of this ship from a ship that's in a sector that is only 50 light years away
        //       it's not a big problem to have very rough results do to rounding.  You'll still be within +/- hundred thousand kilometers which is very tiny
        //       from that distance.
        //       However, thats still not good enough because even if you have a ship at  1 light year away from another and in a different region
        //       you must first subtract the root sector's distances from each other, then the two ship's relative positions from each other and then add
        //       those two seperate results together so as not to lose the accuracy that is needed from this relaively close range.
        //       So when computing such things, when we go up the stack, we add position information.  So a character on board shipX where the shipX is 
        //       has its own internal region and coordinate system, then we add the characters position with the ship's position.  Then as we go up to 
        //       the root region, we add all those positions as well.  Then when we go down the regions to the entity we are trying to find the distance from
        //       we start subtracting positions.  This way we don't lose precision because we start subtracting the largest numbers first and these largest numbers
        //       will tend to be whole numbers ending in lots of zeros.
        //       Of course this way is simplest but since we still dont subtract the very largest as the first operation we can lose some precision.  The best way
        //       around that is to make our sectors smaller than 1 LY (63,000 AU) and make it say 
        //       The relative position of the 

        private static decimal decLightYear = 9424667196000000;
                        // meters 1 LY uses 16 significant digits.  9,424,667,196,000,000

        private decimal OneThousandLightYears = 9424667196000000000; //
        // but the problem with decimals is  arithemetic on them is PROHBITIVIELY extremely slow even for addition and subtraction.
        // Lets face it, the point of using decimals would be to
        // keep a single contingious coordinate system.  This means that if you're handling orbits, then on the server at least you're also operating math directly against those
        // decimal values .  This can be pretty slow and practically speaking it limits the number of moving objects in the game otherwise updating their positions
        // in realtime becomes impossible.  Well, on the server we "could" also use zones and local single precision coordinates.
        // the thing to remember is when working in the local coordinate system, start with the world coords since they are at highest precision
        // and then translate them by the local coordinate systems distance from the origin and that will greatly reduce the size of these numbers and bring
        // them within single precision range.

        //decimal decLightSecond  = 299792458.0;
        private float sngLightSecond = 299792448.0F; // ~300 million meters or 300 thousand kilometers

        // Even if i try to tally up how much "stuff" would use decimals and how much would be ok to do calcs with doubles, i think
        // thinking about it just starts to impact the design of the game.  The only major downside to a grid system with seperate coordinate systems
        // is for client side a) updating coordinates for meshes in neighboring sectors that are visible from the current sector, and b) having to re-update
        // the coordinate system when crossing over the boundary.  
        private double dbl = 20000000000000.012D;

        public static decimal METERS_TO_LY = 1 / decLightYear;

        //meters (1AU)  ~63,000 AU = 1LY   // ~150000000000F;// 150 billion meters 
        // note: earth loks like tiny dot from 4billion miles away
        // http://en.wikipedia.org/wiki/Image:PaleBlueDot.jpg
        private static double earthDistanceToSun = 149597892000.0D;
        public static double AU_TO_METERS = earthDistanceToSun; //1.495979E+9F; //1.495979E+11F; // ~500 light seconds away (8.33 light minutes)
        public static double METERS_TO_AU = 1 / AU_TO_METERS;

        private float earthDiameter = 1.27563E+7F; // 12,756,300 meters
        private float moonDiameter = 3476000; // 3476000 meters
        private float moonOrbitalRadius = 384500000; // 384.5 million meters meters

        // note how the plutoDistance here ending in .2 has 15 significant digits. This is maximum precision of double. Rounding errors occur after the 15th.
        // case in point, try adding a 1 and instead of .21 it rounds crazy to .211
        // so we should limit a "sector" size to 99999999999999.0  meters (668.458134 AU) and then we still get 1/10 of a meter precision for positioning. 
        // Hmm.. these make for small sectors where an entire system may not fit properly.  But considre that if we do
        // decide to simply translate everything in our spatial graph for when for instance a ship moves and we translate all its contents to world coords
        // then we need more precision to properly represent the position of things.  Ideally we want at least 1/100 precision for things inside the ship
        // and ideally at least 1/1000
        private float maxDiamter = 7.4799E13f; // 74799000000000 // 500 AU // 
 
        private double plutoDistance = 5900000000000.2D;
                       //meters note if you try to store that in a single, you lose the .2 and it turns to 5.9E+13!  39.5AU

        // average 5.9 billion kilometers or 5.9 trillion meters. (light takes 5.5 hours to reach pluto)
        // here with doubles, we can use meters to have a solar system with a radius of pluto's distance
        //                             5909116734000
        private float plutoDistanceSingle = 5.9E+13F; 

        //Note that at its CLOSEST distance to the sun, Pluto is 35 Astronomical Units (AU) from the sun. That is, 
        //Pluto is 35 TIMES the distance from the Earth to the Sun away from the sun. 
        //The distance is 4,425 million km and the farthest distance (aphelion) is 7,375 million km.

        //Because light and all electromagnetic radiation falls off with the SQUARE of the distance, 
        //the sun's power is 1 / (35 * 35) or 1 / 1225 as strong on Pluto as it is on the Earth.
        //So, any increase of the sun's energy causing a THREE-FOLD (3-FOLD) increase in atmospheric 
        //temperature on Pluto would be 1,225 TIMES GREATER on the Earth.

        //MPJ - helpful info, although i'm not sure i agree with his conclusion because he assumes
        // earth and pluto are equally able to absorb solar radiation.      


        // 9,460,530,000,000,000 ' meters.   NOTE: Here we are at the bounds of the precision for a system
        private double dblLightYear = 9.46053E+15D;
        private float sngLightYear = 9.46053E+15F;

        //' PRoblem with Doubles:  Even for sectors, using doubles kinda sucks because things like BoundingBox uses singles for bounds and all of that
        //' code needs to be converted down to singles anyway.  So since that has to be done, maybe store real in decimal and then stuff into single.
        //' We'll still get good enough resolution for things far away, and when things are close, we'll get better.
        //' The only thing stopping us with singles is _if_ they arent good enough for even being near the earth / moon system up close.
        //' Specifically i mean zbuffer and texturing.  However, this may still all be solved with the moving origin.

        //' The moving origin advantage.  The camera is always at 0,0,0 and so what happens is, the values within the matrices are all made smaller
        //' thus giving us back more room for precision.  The thing is though, these valeus need to always be adjusted from the original values.


        //' CONCLUSION: What the above is telling us is that for very large galaxies with life size planet sizes and star sizes, etc, then we have to
        //' compute each sector's position data as if it were the only one.  Then we can compute its "real" coord in continguous space
        //' via adding / subtracting the offset within the universe grid.   The whole purpose of doing this is that we get really good precision
        //' for positions of objects and we only lose precision on the calculations between values of different systems
        //' but this is ok as precision matters less when in one system and guaging say distance data to a planet in another system.  Here rounding and loss
        //' of precision is ok.  For instance if the didstance to AlphaCentauri's PRime Star shows as being equal distance away from us as us to a planet orbiting
        //' alpha centauri Prime.  It's ok that both values are the same because we know that once we get closer, we'll start gaining precision.

        //' so when we create our universe, divided up by sectors, each sector will have an X,Y,Z integer offset and we'll know how much each integer
        //' offset translates into.  Thus we'll use static methods for getting accurate distance calcs.


        //' we could cache the real Translation in Vector3Dec and use LocalPosition otherwise.  
        //' The stars and well everything naturally needs to update when you cross over sector boundaries (use the fuzzy boundary code so it doesnt 
        //' toggle at one line but has a inner/outer switch mechanism).  
        //' 
        //' NOTE: in this gamasutra article
        //' http://www.gamasutra.com/features/20020712/oneil_01.htm
        //' he explains that you'll still have Zbuffer problems for moons/planets that are still far away.  To solve this, he either brings the
        //' planet closer but scales it down so it appears to still be just as far away, or he switches it to a billboard by rendering it to
        //' a backbuffer and rendering the resulting 2d texture instead of the 3d image.  The cool concept here though is that its done at runtime
        //' dynamically and doesnt rely on any pre-made imposters that wont look right once the LOD switches.


        /// <summary>
        /// Based on the desired orbitalRadius, compute the orbital velocity for a circular orbit
        /// </summary>
        /// <param name="massKg"></param>
        /// <param name="orbitalRadius"></param>
        /// <returns></returns>
        public static double GetCircularOrbitVelocity(double massKg, double orbitalRadius)
        {
            double u = Temp.GRAVCONST * massKg;
            double semimajoraxis = orbitalRadius; // for perfect circles, radius and semimajoraxis of the ellipse are one and the same


            double velocity = Math.Sqrt(u / orbitalRadius);
            //double velocity = Math.Sqrt(u * (2d / orbitalRadius - 1d / semimajoraxis));
            return velocity;
        }

        /// <summary>
        /// Geostationary Orbital Radius is the radius that maintains orbit over a particular position above a axial rotating world.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static double GetGeostationaryOrbitRadius(Celestial.Body body)
        {
            double massKg = body.MassKg;
            // also known as "spin" or "length of day" in seconds
            // In our procedural generator, we can probably randomly select a rate of "spin" from within some range.
            double rotationalPeriod = 24d * 3600d;

            double radius =  Math.Sqrt (Temp.GRAVCONST * massKg / rotationalPeriod);

            radius = Math.Max(radius, body.Radius);
            return radius;
        } 
    }
}