using System;
using System.Diagnostics;
using Core.Types;

namespace Celestial
{
    public class SystemGen
    {
        private Random _random;
        private int _seed;
        private StarGen _starGen;
        private WorldGen _worldGen;
        

        public SystemGen (Random random)
        {
            // todo: I think passing a seed to each is best than the Random object because 
            // it's best if we can know what seed was used to build a particular system so we can recreate it.
            _random = random;
            _starGen = new StarGen(_random);
            _worldGen = new WorldGen(_random);
        }
        
        public System GenerateSystem (float xPos, float yPos, float zPos, uint starCount)
        {
            return GenerateSystem(GetSystemName(), xPos, yPos, zPos, starCount);
        }
        
        // star count is the total number of starts in the system. If you want an empty Zone, just don't add a star system to it.  
        public System GenerateSystem(string name, float xPos, float yPos, float zPos, uint starCount)
        {
            const int PRIMARY = 0;
            if (starCount < 1 || starCount > 4)
                throw new ArgumentOutOfRangeException("Cannot have less than 1 or more than 4 stars in a star system.");

            System newsystem = CreateSystem(name, new Vector3d(xPos, yPos, zPos), starCount);
            newsystem.Translation = new Vector3d(xPos, yPos, zPos); // todo: might want to add a bit of a small +/- 50 AU variance to each of these

            // NOTE: We can continue to start with getting the luminosity and spectral types first for our primary 
            // and all other stars in the system will be based off of the primary's stats
            bool primaryFound = false;
            Star primary = null;
            foreach (Body body in newsystem.Children )
            {
                if (body is Star && !primaryFound )
                {
                    primaryFound = true;
                    primary = (Star) newsystem.Children[PRIMARY];
                    _starGen.ComputePrimaryStatistics(primary);
                }
                if (body is System)
                {
                    if (!primaryFound && starCount == 4 )
                    {
                        primary = (Star) ((System) newsystem.Children[0]).Children[0];
                        _starGen.ComputePrimaryStatistics(primary);
                        primaryFound = true;
                    }
                    if (!primaryFound) throw new Exception("Primary star must exist as first child in any system.");
                    foreach (Body companion in ((System) body).Children )
                        _starGen.ComputeCompanionStatistics((Star)companion, primary);
                }
            }
            
            // now we must configure the distances between the subsystems and between the stars 
            PositionSystemComponents(newsystem);
            BodyPositioner positioner = new BodyPositioner(_random);
            positioner.Apply(newsystem);

            // position and place planets about the systems and stars in the overall system
            PlanetPlacer placer = new PlanetPlacer(_random);
            placer.Apply(newsystem);
            PlanetPlacer.WorldInfo[] worlds = placer.Worlds;
            
            //// flesh out the full world statistics for each planet
            //for(int i =0; i < worlds.Length ; i++)
            //{
            //    _worldGen.ComputeWorldStatistics(stars, worlds[i]);
            //    // at this stage we Generate Moons for this planet
            //    Moon[] moons = MoonGen.GenerateMoons(worlds[i], zones[i]);

            //    foreach (Moon m in moons)
            //    {
            //        _worldGen.ComputeWorldStatistics(stars, m);
            //        worlds[i].AddChild(m); // todo: when adding these moons to the proper parent, the parent child relationships need to configure themselve
            //    }
            //}
            return newsystem;
        }


        private System CreateSystem(string name, Vector3d position, uint starCount)
        {
            // todo: verify name is unique?
            
            System newsystem = new System(name);
            newsystem.Translation = position; // todo: might want to add a bit of a small +/- 50 AU variance to each of these

            // select a system configuration.  All star systems are made up of any combination of 
            // urnary, binary and trinary systems.  So a 6 star system could be any combination of those 3 types 
            // Once we have the system configured, then we can determine the positions, orbits and masses.  But by doing all this first,
            // we can simplify calculating of the planetary zone calculations.
            // I could make an array of configurations to choose from...
            // All systems must be of close enough range that planets can only be formed around all stars in the system.
            // Thus if you want planets around each star in a binary configuration, you must configure two urnary systems that are far apart.
            // all seperate "systems" by definition then must be of far away ranges.  So this way all planets of a system are just that... system
            // and sub-system based and never star based.
            // So the only remaining question is how do we handle >3 subsystems so that they all orbit the center of the system properly in a way
            // that looks stable?  Well the easy answer is to still limit all multiple systems as being as part of an overall binary or trinary system.
            // so we have two urnary's acting as binaries to one another, which in turn both act as a single urnary to another combined urnary system
            // to make a binary.  So it's hierarchical in this sense.  So how do we elegantly represent this?
            // actually let's just limit to 1 and 2?  
            // http://www.atlasoftheuniverse.com/orbits.html
            // shows we can have some interesting possibilities with just 1 and 2 star subsystems.
            

            // We can allow a max of 4 stars.  Such a configuration would result in 4 stars all of similar mass.
            // generate a configuration based on the star count.  Are configurations can be
            // 1 star
            // 1 star, 1 star (far)
            // 2 stars (close)
            // 1 star, 2 stars (far, close)
            // 2 stars, 2 stars (close, close)

            // entities have matrices so an entity that is child to another has its local matrix multipled by it's parents.
            // so moons orbit a body.  planet's orbit a system which inherits from Body.  
            
           
            switch (starCount)
            {
                case 1:
                    newsystem.AddChild(new Star(newsystem.GetFreeChildName()));
                    break;
                case 2:
                    newsystem.AddChild(new Star(newsystem.GetFreeChildName()));
                    newsystem.AddChild(new Star(newsystem.GetFreeChildName()));
                    break;
                case 3:
                    // notice how in a trinary system, the parent system still has just two children.
                    // the single star at [0], and the subsystem at [1]
                    newsystem.AddChild(new Star(newsystem.GetFreeChildName()));
                    System subsystem = new System(newsystem.Name + " " + starCount.ToString ());
                    newsystem.AddChild(subsystem);
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));
                    break;
                default:
                    subsystem = new System(newsystem.Name + " " + starCount.ToString() + " A");
                    newsystem.AddChild(subsystem);
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));

                    subsystem = new System(newsystem.Name + " " + starCount.ToString() + " B");
                    newsystem.AddChild(subsystem);
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));
                    subsystem.AddChild(new Star(subsystem.GetFreeChildName()));
                    
                    Debug.Assert(starCount == 4);
                    break;
            }
            return newsystem;
        }
        
        // todo: i think we should customize the way we build binary+ systems by basically
        // determining how many we have in advance and then determining the positions and things after we know how many we have.
        private void PositionSystemComponents(System system)
        {
            
            
            // remember that planets CAN orbit either or both stars within a single subsystem or the center of the entire subsystem.
            // To orbit a star, the planet is added as a child of that star.  To orbit the system, the planet is added as another child of the system.
            // HOWEVER- the problem with the above is that originally I didn't intend for Planets to be "children" to stars or for Moons to be "children"
            // to planets because I just wanted a soup of bodies all children of the System itself.  But what I forgot I think is that
            // for moon's to orbit planets that orbit stars or systems, then we need the hierarchical transforms to work and that requires 
            // moons to be children of planets and such. In fact we can just think of them as "systems", "starsystems", "planettarysystems" 
            // and "sattellitesystems"
            
            // For spacecraft however, we will be using nBody so we wont have these child/parent based
            // orbital positions for them
            //
            // On the other hand, if I merely compute the center of orbit for a planet or moon, we can put them in the soup of the system.
            //
            //
            // Finding positions I think will be easy.  We need a recursive function that FindPosition() recursively such that we only find the 
            // position of the current Body after we find the position of it's children and so forth.  Then we can set the Diameter of the entire
            // child systems on the way back up.
            
            // 1) child stars or subsystems within a system, orbit the same center of their parent system but are on opposite sides of each other.
            //    The main thing we calculate is their seperation distance which is basically their orbitalRadius * 2 since each have the same oribtalRadius
            //
            // so if we have > 2 stars remaining unpositioned in the system
            
            // compute orbit and position information
            //if (system.ChildCount == 1)
            //{
            //    // since we have no companions, make the single star at the exact center of the system
            //    stars[PRIMARY].Translation = system.Translation; // todo: i think this should be 0,0,0 since we'll be Adding the parent's offset to get our world position
            //    stars[PRIMARY].OrbitalRadius = 0;
            //    // if there is only a primary star and no companions
            //    stars[PRIMARY].OrbitalEccentricity = 0; // it's at the center of it's system and orbits the center of the universe
            //}
            //else // companion count is at least 1
            //{
            //    // we do have companions.  Lets figure out their exact X,Y,Z positions
            //    Distance = GetOrbitalSeperationAU(false);
            //    stars[PRIMARY].Translation = new Vector3d(system.Translation.x + GetOrbitalSeperationAU(false),
            //            system.Translation.y + GetOrbitalSeperationAU(false),
            //            system.Translation.z); // note our z position is same as center of system

            //    // get our positions for our Companion stars
            //    if (starCount == 2)
            //    {
            //        // if only 1 companion, the star is opposite of our Primary star
            //        stars[1].OrbitalRadius = Distance;
            //        stars[1].Translation = GetCompanionXYZCoords(system.Translation, stars[PRIMARY].Translation, Distance, 180, false);
            //    }
            //    if (starCount == 3)
            //    {
            //        // then all 3 stars are all 120degrees off center
            //        // get our distance from the center of the system for Companion1
            //        stars[1].OrbitalRadius = Distance;
            //        stars[1].Translation = GetCompanionXYZCoords(system.Translation, stars[PRIMARY].Translation, Distance, 120, false);
            //        // compute a new distance for the second companion
            //        Distance = GetOrbitalSeperationAU(true);
            //        stars[2].OrbitalRadius = Distance;
            //        // true is used for 2nd companion stars to trigger the +6 modifier for 3rd star in trinary systems
            //        stars[2].Translation = GetCompanionXYZCoords(system.Translation, stars[PRIMARY].Translation, Distance, 120, true);
            //    }
            //}


            //// calculate the orbital eccentricty and minimum seperations for stars in the system.  This is used to determine planetary orbits
            //if (starCount > 1)
            //{
            //    // in our loop, we start at the first companion index which is '1'
            //    for (int i = 1; i < starCount; i++)
            //    {
            //        stars[i].OrbitalEccentricity = GetOrbitalEccentricity(stars[i].OrbitalRadius);
            //        // find the minimum and maximum seperation between the Primary and each companion
            //        if (starCount == 2)
            //        {
            //            stars[i].MinSeperation =
            //                GetMinimumSeperation(stars[i].OrbitalEccentricity, stars[PRIMARY].OrbitalEccentricity,
            //                                     stars[i].OrbitalRadius,
            //                                     stars[PRIMARY].OrbitalRadius);
            //            stars[i].MaxSeperation =
            //                GetMaximumSeperation(stars[i].OrbitalEccentricity, stars[PRIMARY].OrbitalEccentricity,
            //                                     stars[i].OrbitalRadius,
            //                                     stars[PRIMARY].OrbitalRadius);
            //            stars[PRIMARY].MinSeperation = stars[i].MinSeperation;
            //            stars[PRIMARY].MaxSeperation = stars[i].MaxSeperation;
            //        }
            //        else if (starCount == 3)
            //        {
            //            stars[i].MinSeperation =
            //                GetMinimumSeperation(stars[i].OrbitalEccentricity, stars[PRIMARY].OrbitalEccentricity,
            //                                     stars[i].OrbitalRadius,
            //                                     stars[PRIMARY].OrbitalRadius);
            //            stars[i].MaxSeperation =
            //                GetMaximumSeperation(stars[i].OrbitalEccentricity, stars[PRIMARY].OrbitalEccentricity,
            //                                     stars[i].OrbitalRadius,
            //                                     stars[PRIMARY].OrbitalRadius);
            //            // the minimum seperation for the primary needs to be the smallest between its two companions
            //            stars[PRIMARY].MinSeperation = Math.Min(stars[PRIMARY].MinSeperation, stars[i].MinSeperation);
            //            // the maximum seperation needs to be the larger of the two companions
            //            stars[PRIMARY].MaxSeperation = Math.Max(stars[PRIMARY].MaxSeperation, stars[i].MaxSeperation);
            //        }
            //    }
            //}
        }
       

        private string GetSystemName()
        {
            // //This function generates made up names for star systems
            string result = "";
            // //generate a random number between 4 and 10
            int i = _random.Next(4, 10);
            for (int j = 1; j <= i; j++)
            {
                // now generate a random letter from the alphabet
                int k = _random.Next(65, 90);
                result = result + (char) k;
            }
            return result;
        }

        

        //// todo: when we add a companion we need to actually recompute an orbit for the primary star because
        //// right now we assume the center of the system is the primary star and any companions orbit that. 
        //// what we really want is to orbit the center of mass for the system.
        //private Vector3d GetCompanionXYZCoords(Vector3d SystemPosition, Vector3d PrimaryStarPosition,
        //                                        float Distance, float Theta,
        //                                  bool IsSecondCompanion)
        //{
        //    // //this function calculates and Creates the X,Y and Z positions for the 2 companion stars in a Trinary system
        //    //   based on the Center of the System, the location of the Primary Star and the angle of seperation Theta
        //    Vector3d direction;
        //    Vector3d result;
        //    // the vector from our the center of our system to our primary star
        //    Vector3d u;
        //    // the vector to our 2nd companion
        //    double sine, cosine;
        //    const double PI = Math.PI;

        //    if (IsSecondCompanion)
        //        Theta *= (float)(PI / 180) * -1;
        //    else
        //        Theta *= (float)(PI / 180); // convert degrees to radians

        //    // get the vector of our system center to our primary star
        //    direction = PrimaryStarPosition - SystemPosition;

        //    // R(theta) = [ cos(theta) , -sin(theta) ]
        //    //            [ sin(theta) ,  cos(theta) ]
        //    cosine = Math.Cos(Theta);
        //    sine = Math.Sin(Theta);
        //    // u is just w rotated by theta.  To get this we have u = R(theta) * w
        //    u.x = (direction.x * cosine) + (direction.y * (sine * -1));
        //    u.y = (direction.x * sine) + (direction.y * cosine);

        //    // The actual Companion position = SystemCenter + Distance * u
        //    result = SystemPosition + Distance * u;
        //    result.z = PrimaryStarPosition.z;  // z position is the same

        //    return result;
        //}


        float GetMinimumSeperation(float CompanionEccentricity, float PrimaryEccentricity, float companionOrbitalRadius, float primaryOrbitalRadius)
        {
            // //Minimum seperation between the Primary and its Companion.
            //   NOTE: in systems with 2 companions, the Primary's minimum seperation is the smaller of the two companions
            return ((1 - CompanionEccentricity + PrimaryEccentricity) * companionOrbitalRadius + primaryOrbitalRadius);
        }

        float GetMaximumSeperation(float CompanionEccentricity, float PrimaryEccentricity, float companionOrbitalRadius, float primaryOrbitalRadius)
        {
            // //Maximum seperation between the Primary and its Companion.
            //   NOTE: in systems with 2 companions, the Primary's maximum seperation is the smaller of the two companions
            return (((1 + CompanionEccentricity + PrimaryEccentricity) * companionOrbitalRadius) + primaryOrbitalRadius);
        } 
    }
}