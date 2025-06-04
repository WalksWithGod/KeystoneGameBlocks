using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Celestial
{
    public class StarSeperationSelector 
    {
        private Stack<Body> _stars = new Stack<Body>();
        private Stack<StellarSystem> _systems = new Stack<StellarSystem>();
        private Stack<float> _diameters = new Stack<float>();
        private Random _rand;
        private const double MAX_TOTAL_RADIUS = 250; // 250 AU will be max radius of combined stars orbits in the system
        public StarSeperationSelector(Random rand)
        {
            if (rand == null) throw new ArgumentNullException();
            _rand = rand;
        }

        public void Apply(StellarSystem parentSystem, double maxSeperation, out double result)
        {

            Body firstBody = (Body)parentSystem.Children[0];
            Body companion = null;
            Debug.Assert(parentSystem.Children.Length <= 2);  // no one sub-system in v1.0 can have more than two stars

            // totalRadii defines minimum seperation
            double totalRadii = GetTotalRadii(firstBody) + GetTotalRadii(companion);
            // get the seperation and clamp it to be the lesser of maxSeperation and total radii 
            double seperation = Math.Min (GetOrbitalSeperationMeters(0) + totalRadii, maxSeperation);
            Debug.Assert (seperation > totalRadii );

            if (parentSystem.Children.Length > 1)
            {
                companion = (Body)parentSystem.Children[1];
            }

            double computedSeperation;
            // Seperation is dependant on whether we're seperating two stars or one star and one system
            // or two systems
            if (firstBody is StellarSystem)
            {
                //take half of the supplied offset and recurse
                Apply((StellarSystem)firstBody, maxSeperation * 0.5f, out computedSeperation);
            }
            else // it's a star
            {
                // TODO: Root StellarSystems have no oribital period or eccentricity or anything.
                // They may have a center translation offset however.  
                // Stars and child StellarSystems then have a 
                // orbital radius to that StellarSystem and must still fit within bounds
                // of the StellarSystem's Zone.

                //
                // TODO: Remember that for child Entity nodes, translation is relative
                //       to parent!  This simplifies our calculations greatly.
                // TODO: once we have orbital radius, we can compute a position based on it's
                // current % of it's orbital period
                //w.OrbitalEccentricity = (float)GetEccentricty(w.OrbitalRadius, out maxSeperation, out minSeperation);
                //// TODO: temp using fixed sol mass
                //w.OrbitalPeriod = (float)GetOrbitalPeriod(Temp.SOL_MASS_KILOGRAMS, w.OrbitalRadius);
                //if (w.OrbitalPeriod == 0)
                //    System.Diagnostics.Debug.WriteLine("GenerateWorld.ComputeWorldStatistics() - Failed to compute orbital period.");
                //w.OrbitalEpoch = w.OrbitalPeriod * _random.NextDouble();
                //w.OrbitalInclination = (float)GetOrbitalInclination(w.Mass, w.OrbitalRadius);
                //w.OrbitalProcession = (float)(_random.NextDouble() * (Math.PI * 2d));

                firstBody.Translation = Vector3d.Zero(); // June.9.2017 - NOTE: all stars are at origin now and each Zone only ever contains 1 star.  (No multibody systems)// new Vector3d(seperation , 0, 0);
                // TODO: if this is a star in a system with other stars, then shouldn't the orbitalradius
                // be based on the center of the system and the mass of the stars and thus their distances
                // to center of mass of system?
                firstBody.OrbitalRadius = seperation;
                // now recurse to the companion if any
                if (companion != null)
                {
                    if (companion is Star)
                        // if the companion is a star, we use the full seperation we computed
                        // TODO: but is seperation now half since half was sibling? and do we
                        // have to change the firstBody's oribtal radius to be half the seperation?
                        companion.Translation = new Vector3d(seperation, 0, 0);
                    else
                    {
                        // child stellar system still has a seperation from the primary root system's center
                        companion.Translation = new Vector3d(seperation, 0, 0); 
                        Apply((StellarSystem)companion, maxSeperation * 0.5f, out computedSeperation);
                    }
                    // else we use 50% of it... but then we also have to ensure
                    // that the firstBody we already set a translation to is at least twice the distance away as the
                    // distance between the two stars in the company stellar system.. urgh...

                    // so really what should happen is the companion stellar system should be determined first then
                    // afterwards an overall translation offset applied to both it's stars once we determine that translation offset
                    // based on it's size.  This is the only way.  That "computedSeperation" afterall is reported back
                    // and that's the point of having it.  We need to know it.

                    // Ok, so now what about the last case of two stellar systems both with two stars?

                    // same deal really, we need to compute the seperations of both first
                    // using no translation offset but just 50%
                }  
            }

            result = seperation;
        }

        private double GetTotalRadii(Body body)
        {
            double result = 0;
            if (body == null) return 0;

            if (body is Star)
                result = body.Radius;
            else if (body is StellarSystem)
                result = GetTotalRadii ((Body)body.Children[0]) +  GetTotalRadii ((Body)body.Children[1]);

            return result;
        }

        public void Apply(StellarSystem system)
        {
            Star primary = system.Primary;
            if (primary == null) throw new ArgumentOutOfRangeException("StarSeperationSelector.Apply(StellarSystem) -- No primary star found'");
            double seperation = GetOrbitalSeperationMeters((uint) system.ChildCount  - 1)*2;
            if (_systems.Count > 0)
            {
                // keep re-rolling a seperation for a multi-star system to ensure our minimum seperation between the child star or
                // child sub-system is no less than 1/4 the parent's diameter
                // TODO: fix this error with diameter of the system being 0
                if (_systems.Peek().Diameter > 0)
                {
                    while (seperation > _systems.Peek().Diameter * .25)
                        seperation = GetOrbitalSeperationMeters((uint)_systems.Count - 1) * 2;

                    system.Diameter = (float)seperation;
                }
            }

            _systems.Push(system);

            // compute all the child subsystem's positions and seperations first
            foreach (Body body in system.Children)
            {
                if (body is StellarSystem)
                {
                    // recursion
                    Apply((StellarSystem) body);


                    // now position any child systems.  if there was a child star already, it's position will be set and we can
                    // place the child system opposite it.  If not, we just place it like we would a child star
                    Star[] childstars = system.Stars;
                    if (childstars != null && childstars.Length > 0)
                    {
                        body.Translation = childstars[0].Translation * -1;
                        // TODO: CRAP.  I think i need to include it's parent's position offset...
                        // or perhaps only for subsystems??  ugh... wish i finished this when it was fresh in my mind
                        // NOTE: the eccentricty is not the same as the inclination
                        body.OrbitalEccentricity = 0f;
                        //              position = childsars[0].Translation * -1 + _systems.Peek().Translation; ?
                        body.OrbitalRadius = childstars[0].OrbitalRadius;
                        //TODO: min seperation! ack   
                    }
                    else if (body is Star) // it's a star
                    {
                        body.Translation = Vector3d.Zero(); // June.9.2017 - all stars at origin. // Utilities.RandomHelper.Random() * _systems.Peek().Diameter * .5f;
                    }
                    else
                        throw new ArgumentOutOfRangeException("StarSeperationSelector.Apply(StellarSystem) -- Unexepcted child '" + body.TypeName + "'");
                }
            }


            uint starCount = system.StarCount;
            if (starCount > 0)
            {
                Star[] childstars = system.Stars;

                foreach (Star star in childstars)
                {
                    // apply same orbital radius to all children
                    star.OrbitalRadius = (float) _systems.Peek().Radius;
                    star.OrbitalEccentricity = 0;
                }
                // pick a random point at that distance for the first child star that is at half the diameter of the parent system
                childstars[0].Translation = Utilities.RandomHelper.RandomUnitSphere() * _systems.Peek().Diameter*.5f;

                // if there are more than one children, place them at opposite ends
                if (starCount > 1)
                {
                    childstars[1].Translation = childstars[1].Translation * -1;
                    Debug.Assert(starCount == 2);
                }
            }
            _systems.Pop();
        }


        private double GetOrbitalSeperationMeters(uint depth)
        {
            // this function determines how far from the center of the system a star is.
            float x = (float) _rand.NextDouble();
            float y;

            // if the star is the 2nd companion star in a Trinary star system,
            // each depth down into the system hierarchy will have a smaller and smaller seperation
            float depthModifer = 0.3333f * depth;
            x -= depthModifer;

            if (x <= 0.3333)
                y = 0.025f;
            else if (x <= 0.4999)
                y = 0.25f;
            else if (x <= 0.611)
                y = 1;
            else if (x <= 0.7776)
                y = 5;
            else
                y = 25;

            // add some variation
            y *= _rand.Next(2, 12);
            

            return y * Temp.AU_TO_METERS;
        }

        // TODO: i think we should customize the way we build binary+ systems by basically
        // determining how many we have in advance and then determining the positions and things after we know how many we have.
        private void PositionSystemComponents(StellarSystem system)
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
            //    stars[PRIMARY].Translation = system.Translation; // TODO: i think this should be 0,0,0 since we'll be Adding the parent's offset to get our world position
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


        // TODO: when we add a companion we need to actually recompute an orbit for the primary star because
        // right now we assume the center of the system is the primary star and any companions orbit that. 
        // what we really want is to orbit the center of mass for the system.
        private Vector3d GetCompanionXYZCoords(Vector3d SystemPosition, Vector3d PrimaryStarPosition,
                                                float Distance, float Theta,
                                          bool IsSecondCompanion)
        {
            // //this function calculates and Creates the X,Y and Z positions for the 2 companion stars in a Trinary system
            //   based on the Center of the System, the location of the Primary Star and the angle of seperation Theta
            Vector3d direction;
            Vector3d result;
            // the vector from our the center of our system to our primary star
            Vector3d u;
            // the vector to our 2nd companion
            double sine, cosine;
            const double PI = Math.PI;

            if (IsSecondCompanion)
                Theta *= (float)(PI / 180f) * -1f;
            else
                Theta *= (float)(PI / 180f); // convert degrees to radians

            // get the vector of our system center to our primary star
            direction = PrimaryStarPosition - SystemPosition;

            // R(theta) = [ cos(theta) , -sin(theta) ]
            //            [ sin(theta) ,  cos(theta) ]
            cosine = Math.Cos(Theta);
            sine = Math.Sin(Theta);
            // u is just w rotated by theta.  To get this we have u = R(theta) * w
            u.x = (direction.x * cosine) + (direction.y * (sine * -1f));
            u.y = (direction.x * sine) + (direction.y * cosine);
            u.z = 0f;

            // The actual Companion position = SystemCenter + Distance * u
            result = SystemPosition + Distance * u;
            result.z = PrimaryStarPosition.z;  // z position is the same

            return result;
        }


        private float GetMinimumSeperation(float CompanionEccentricity, float PrimaryEccentricity,
                                           float companionOrbitalRadius, float primaryOrbitalRadius)
        {
            // //Minimum seperation between the Primary and its Companion.
            //   NOTE: in systems with 2 companions, the Primary's minimum seperation is the smaller of the two companions
            return ((1 - CompanionEccentricity + PrimaryEccentricity) * companionOrbitalRadius + primaryOrbitalRadius);
        }

        private float GetMaximumSeperation(float CompanionEccentricity, float PrimaryEccentricity,
                                           float companionOrbitalRadius, float primaryOrbitalRadius)
        {
            // //Maximum seperation between the Primary and its Companion.
            //   NOTE: in systems with 2 companions, the Primary's maximum seperation is the smaller of the two companions
            return (((1 + CompanionEccentricity + PrimaryEccentricity) * companionOrbitalRadius) + primaryOrbitalRadius);
        }
     
    }
}