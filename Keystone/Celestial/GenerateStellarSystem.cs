using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Types;
using Settings;

namespace Keystone.Celestial
{
    public class StellarSystemGenerator
    {
        private Random _random;
        private GenerateStar _starGen;
        Keystone.Utilities.MarkovNameGenerator mNameGenerator;

        //http://www.astronomynotes.com/solfluf/s11.htm
        // All the planets' orbits lie roughly in the same plane. 
        // The Sun's rotational equator lies nearly in this plane. 
        // Planetary orbits are slightly elliptical, very nearly circular. 
        public StellarSystemGenerator(int seed)
        {
            // TODO: I think passing a seed to each is best than the Random object because 
            // it's best if we can know what seed was used to build a particular system so we can recreate it.
            _random = new Random(seed);
            _starGen = new GenerateStar(_random);
            
            int minLength = 2;
            int order = 1;
            List<string> samples = new List<string>(){"Mercury", "Venus", "Earth", "Jupiter", "Io", "Europa", "Saturn", "Uranus", "Pluto", "Neptune"};
            
            
         	// TODO: add option to precache some "used" names for when we generate universes based on actual
			//       near star data         	
            mNameGenerator = new Keystone.Utilities.MarkovNameGenerator (samples, order, minLength, seed);
            //Utilities.RandomNameHelper.GenerateStarName()
        }

        public StellarSystem GenerateSystem(uint starCount)
        {
            return GenerateSystem(mNameGenerator.NextName, starCount);
        }

        // star count is the total number of starts in the system. If you want an empty Zone, just don't add a star system to it.  
        public StellarSystem GenerateSystem(string name, uint starCount)
        {
            const int PRIMARY = 0;
            if (starCount < 1 || starCount > 4)
                throw new ArgumentOutOfRangeException("StellarSystemGenerator.GenerateSystem() - Cannot have less than 1 or more than 4 stars in a star system.");

            
            // NOTE: We can continue to start with getting the luminosity and spectral types first for our primary 
            // and all other stars in the system will be based off of the primary's stats
            bool primaryFound = false;
            Star primary = null;
            starCount = 1; // TODO: hardcoding star count for version 1.0.
            StellarSystem newsystem = PopulateSystem(name, starCount);

            //  After iterating through these child stars and child sub-systems
            // Positioner will compute the positions of these new children.
            foreach (Body body in newsystem.Children)
            {
                if (body is Star )
                {
                    if (!primaryFound)
                    {
                        primary = (Star)newsystem.Children[PRIMARY];
                        // these statistics are strictly the physical properties of the stars and does not compute
                        // anything yet related to potential planetary orbits or habital zones
                        _starGen.ComputePrimaryStatistics(primary);
                        ProceduralHelper.InitStarVisuals(primary);
                        primaryFound = true;
                    }
                    else
                    {
                        _starGen.ComputeCompanionStatistics((Star)body, primary);
                        ProceduralHelper.InitStarVisuals((Star)body);
                    }
                }
                else if (body is StellarSystem)
                {
                    if (!primaryFound && starCount == 4)
                    {
                        primary = (Star) ((StellarSystem) newsystem.Children[0]).Children[0];
                        // these statistics are strictly the physical properties of the stars and does not compute
                        // anything yet related to potential planetary orbits or habital zones
                        _starGen.ComputePrimaryStatistics(primary);
                        ProceduralHelper.InitStarVisuals(primary);
                        primaryFound = true;
                    }
                    if (!primaryFound) throw new Exception("Primary star must exist as first child in any system.");
                    // we only allow one depth of child stellar systems within a parent stellar system so now we'll
                    // traverse the children of this child stellar system
                    foreach (Body companion in ((StellarSystem) body).Children)
                    {
                        if (companion == primary) continue; // we've already computed stats for the primary
                        // these statistics are strictly the physical properties of the stars and does not compute
                        // anything yet related to potential planetary orbits or habital zones
                        _starGen.ComputeCompanionStatistics((Star) companion, primary);
                        ProceduralHelper.InitStarVisuals ((Star) companion);
                    }
                }
            }


            // add a bit of a small +/- 50 AU variance to each of root StellarSystem variably off center
            // with respect to Zone region parent.
            // June.5.2017 - for v1.0 just limit star Translations to 0,0,0 instead of +-50 AU offset on each axis
            int randomOffsetX = 0; // _random.Next(-50, 50);
            int randomOffsetY = 0; // _random.Next(-50, 50);
            int randomOffsetZ = 0; // _random.Next(-50, 50);

            // applying an offset to the root system will hierarchically offset everything else
            newsystem.Translation = new Vector3d(randomOffsetX * Celestial.Temp.AU_TO_METERS,
                                                 randomOffsetY * Celestial.Temp.AU_TO_METERS,
                                                 randomOffsetZ * Celestial.Temp.AU_TO_METERS);

            // now we must configure the distances between the subsystems and between the stars 
            StarSeperationSelector positioner = new StarSeperationSelector(_random);

            double computedSeperation;
            // TODO: we need to ensure maxSeperation is less than some fraction of a zone's size
            // i think right now we're at 500 AU zone region size by default
            double maxSeperation = 250 * Celestial.Temp.AU_TO_METERS;
            positioner.Apply(newsystem, maxSeperation, out computedSeperation);
            return newsystem;
        }


        private StellarSystem PopulateSystem(string name, uint starCount)
        {
            // TODO: verify name is unique?
            string id = Keystone.Resource.Repository.GetNewName (typeof (StellarSystem));
            StellarSystem newsystem = new StellarSystem(id);
			newsystem.Name = name;
            
			// TODO: might want to add a bit of a small +/- 50 AU variance to each of these

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



            Star star;
            switch (starCount)
            {
                case 1:
            		id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = newsystem.GetFreeChildName ();
                    newsystem.AddChild(star);
                    break;
                case 2:
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = newsystem.GetFreeChildName ();
                    newsystem.AddChild(star);
                    
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = newsystem.GetFreeChildName ();
                    newsystem.AddChild(star);
                    break;
                case 3:
                    // notice how in a trinary system, the parent system still has just two children.
                    // the single star at [0], and the subsystem at [1]
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = newsystem.GetFreeChildName ();
                    newsystem.AddChild(star);
                    
                    id = Resource.Repository.GetNewName (typeof (StellarSystem));
                    StellarSystem subsystem = new StellarSystem(id);
                    subsystem.Name = newsystem.Name + " " + starCount.ToString();
                    
                    newsystem.AddChild(subsystem);
                    
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);
                    
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);
                    break;
                default:
                    // subsystem A
                    id = Resource.Repository.GetNewName (typeof (StellarSystem));
                    subsystem = new StellarSystem(id);
                    subsystem.Name = newsystem.Name + " " + starCount.ToString() + " A";
                    newsystem.AddChild(subsystem);
                    
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);

                    // subsystem B
                    id = Resource.Repository.GetNewName (typeof (StellarSystem));
                    subsystem = new StellarSystem(id);
                    subsystem.Name = newsystem.Name + " " + starCount.ToString() + " B";
                    newsystem.AddChild(subsystem);
                    
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);
                    id = Resource.Repository.GetNewName (typeof (Star));
            		star = new Star (id);
            		star.Name = subsystem.GetFreeChildName ();
                    subsystem.AddChild(star);

                    Debug.Assert(starCount == 4);
                    break;
            }
            return newsystem;
        }
    }
}