using System;
using Keystone.Elements;
using Keystone.Types;

namespace Keystone.Animation
{
    public class EllipticalAnimation : AnimationClip
    {

        public EllipticalAnimation(string id) // TODO: these should be shareable
            : base(id)                        // because i think it's the AnimationTrack which will
                                              // store active per instance data
                                              // however, i think it's complicated by the fact
                                              // Animation.cs has a target entity name specified.
        {
        }

        internal override void Update( AnimationTrack track, object target)
        {
            Celestial.World w = target as Celestial.World;
            // get the radius from the target
            if (w != null)
            {
                System.Diagnostics.Debug.Assert(w.Dynamic == false, "EllipticalAnimation.Update() -- World dynamics should be disabled for orbits using elliptical animation.");

                // June.13.2017 - elliptical code seems to work ok for perfect circles. Won't know for sure until we get our
                //                ship's orbit around a moon or planet in.
                //if (w.OrbitalEccentricity == 0)
                //{
                //    double TWO_PI = Math.PI * 2;
                //    System.Diagnostics.Debug.Assert(w.OrbitalInclination == 0);
                //    double horizontal = 0;
                //    w.Translation = Keystone.Utilities.MathHelper.MoveAroundPoint(w.Parent.Translation, w.OrbitalRadius, horizontal, 0);
                //   // return;
                //}

                // TODO: jupiter's orbital radius seems wrong
                double semiMajorAxis = w.OrbitalRadius; 

                //if (target is Node && ((Node)target).Name == "Jupiter")
                //	System.Diagnostics.Debug.WriteLine ("EllipticalAnimation.Update() - breakpoint."); // jupiter semimajor = 778547200000f;

                float period = track.Duration;
                if (period == 0f) return; // possibly a world/body with no parent to orbit (eg. something we just manually placed into scene)

                // get the eccentricity, but question is, why not just store and manage Eccentricity 
                // entirely within this animation?  problem with that is, makes it harder to get that info 
                // by player for gameplay reasons if it's not stored in the Entity world itself.  Besides
                // this way we can still provide some alternative modeling of the orbit in the future if desired.
                // TODO: i think we need to store the "age" (elapsedSeconds) in the Body and add that to OrbitalEpoch instead of track.ElapsedSeconds
                // TODO: our procedurally generated solar systems are not even computing an OrbitalEpoch... only our hardcoded Sol system is.
                // TODO: we're not updating our w.OrbitalEpoch do we have to? 
                double finalWeight = track.ElapsedSeconds + w.OrbitalEpoch; 
                double bodyOffsetAngle = GetTrueAnomaly(w.OrbitalEccentricity, period, finalWeight);
                double currRadius = CurrentRadius(semiMajorAxis, bodyOffsetAngle, w.OrbitalEccentricity);

                // since the sun is at (0,0) in our reference frame, we have easy formula for (x,y)

                // compute the translation in essentially model space
                Vector3d translation;
                translation.x = currRadius * Math.Sin(bodyOffsetAngle);
                translation.z = currRadius * Math.Cos(bodyOffsetAngle);
                translation.y = 0;


                if (w.OrbitalInclination != 0)
                {
                    // if there is an inclination, it must be applied to the translation directly
                    // NOT to just the model right? The reason is because here by directly setting
                    // the translation, this entity remains essentially
                    double radiansInclination = w.OrbitalInclination;
                    double radiansProcession = w.OrbitalProcession;

                    translation = Vector3d.TransformCoord(translation, Matrix.CreateRotationZ(radiansInclination) * Matrix.CreateRotationY(radiansProcession));
                }
                                
                // NOTE: all animation transforms are just on the child.  Children will
                // always additionally receive parent translations (and optionally rotations 
                // and scales if enabled)
                BoundTransformGroup transformable = (BoundTransformGroup)target;
                Vector3d previousTranslation = transformable.Translation;
                transformable.Translation = translation;

                
                
                // obsolete - as long as World.Dynamic == false we do not need to set LatestStepTranslation.
                //transformable.LatestStepTranslation = translation;
            }
        }


        public double CurrentRadius(double semiMajorAxis, double bodyOffsetAngle, double eccentricity)
        {
            // for ellipses there is a formula linking radius from a focus to angle and eccentricity:
            // r =  a(1 - e^2) / (1 + e cos(theta))	
            return (semiMajorAxis * (1 - eccentricity * eccentricity)) / (1 + eccentricity * Math.Cos(bodyOffsetAngle));
        }

        /// <summary>
        /// The angle between the body and the sun (in radians).
        /// The base method returns the angle for circular orbits, which is simply the 
        ///  percentage of the period completed gives the percentage of the circle covered
        ///  (therefore, by definition, angle at start of simulation = zero)				
        /// </summary>		
        public static double GetTrueAnomaly(double period, double elapsed)
        {
            // TODO: this function is essentially a keyframe using linear interpolation
            // whereas for eliptical, it's using 
            // and althoug we can compute a new position this way, i do need a velocity too so that
            // 
            return ((double)(elapsed % period) / period) * 2d * Math.PI;
        }


        /// <summary>
        /// The angle between the body and the sun (in radians).
        /// http://en.wikipedia.org/wiki/Areal_velocity
        /// </summary>
        public double GetTrueAnomaly(double eccentricity, double period, double elapsed) // returns angle between body and sun (focci)
        {
            //double E = CalcEccentricAnomaly(eccentricity, period, elapsed);
            double E = CalcEccentricAnomaly(1.0d, GetTrueAnomaly(period, elapsed), eccentricity, .00001d);

            //Solve cos(bodyAngle) = ( cos(E) - e ) / (1 - e cos(E) ) to get the body's angle with the Sun
            double bodyAngle = Math.Acos((Math.Cos(E) - eccentricity) / (1d - eccentricity * Math.Cos(E)));

            // Arccos returns a value between 0 and pi, but when M > pi (ie past halfway point) we
            // take (2pi - angle) to get the solution that lies between pi and 2pi
            double remainder = elapsed % period;
            if (remainder > period * 0.5d)
                bodyAngle = 2.0d * Math.PI - bodyAngle;

            // we return negative bodyAngle because this gets our Sol planets and moons orbiting
            // counter-clockwise.  However, i need to test how to then get worlds that should
            // orbit clockwise to do so.
            return -bodyAngle;
        }

        /// <summary>
        /// The angle between the body and the sun (in radians).
        /// http://en.wikipedia.org/wiki/Areal_velocity
        /// </summary>
        private double CalcEccentricAnomaly(double eccentricity, double period, double elapsed)
        {
            // True Anomaly in this context is the angle between the body and the sun.
            // For elliptical orbits, it's a bit tricky.  The percentage of the period completed is still a key input, but we also need
            //  to apply Kepler's equation (based on the eccentricity) to ensure that we sweep out equal areas in equal times.
            //  This equation is transcendental (ie can't be solved algebraically)
            //  so we either have to use an approximating equation or solve by a numeric method.  My implementation uses 
            //  Newton-Raphson iteration to get an excellent approximate answer (usually in 2 or 3 iterations).
            double M = GetTrueAnomaly(period, elapsed);	// the base implementation returns the "mean anomaly", which
            // assumes a circular orbit.  The rest of our work involves correcting
            // this to get the true "eccentric" anomaly
            double E, E_new, E_old = M + (eccentricity / 2d);
            const double epsilon = 0.000001;

            //Solve [ 0 = E - e sin(E) - M ] for E using Newton-Raphson: Xn+1 = Xn - [ f(Xn) / f'(Xn) ]
            // E = Eccentric Anomaly, M = Mean Anomaly
            do
            {
                E_new = E_old - (E_old - eccentricity * Math.Sin(E_old) - M) / (1d - eccentricity * Math.Cos(E_old));
                E_old = E_new;
            } while (Math.Abs(E_old - E_new) > epsilon);

            E = E_new;

            return E;
        }


        //private double CalcEccentricAnomaly(double fEccentricityGuess, double fMeanAnamoly, double fEccentricity, double fAccuracy)
        //{
        //    //Calc Ecctrentric Anomaly to specified accuracy  
        //    double fDelta;
        //    double fDeltaE;
        //    double fE;
        //    double fETmp;
        //    double fEG;

        //    fEG = fEccentricityGuess;

        //    fDelta = fEG - (fEccentricity * Math.Sin(fEG)) - fMeanAnamoly;
        //    if (Math.Abs(fDelta) > fAccuracy)
        //    {
        //        fDeltaE = (fDelta / (1.0 - (fEccentricity * Math.Cos(fEG))));
        //        fETmp = fEG - fDeltaE;
        //        fE = CalcEccentricAnomaly(fETmp, fMeanAnamoly, fEccentricity, fAccuracy);
        //    }
        //    else
        //    {
        //        fE = fEccentricityGuess;
        //    }
        //    return fE;
        //}

        private double CalcEccentricAnomaly(double fEccentricityGuess, double fMeanAnamoly, double fEccentricity, double fAccuracy)
        {
            //Calc Ecctrentric Anomaly to specified accuracy  
            double fDelta;
            double fDeltaE;
            double fE;
            double fETmp;
            double fEG;

            fEG = fEccentricityGuess;
            fE = fEccentricityGuess;

            fDelta = fEG - (fEccentricity * Math.Sin(fEG)) - fMeanAnamoly;
            if (Math.Abs(fDelta) <= fAccuracy) return fE;

            do 
            {
                fDelta = fEG - (fEccentricity * Math.Sin(fEG)) - fMeanAnamoly;
                fDeltaE = (fDelta / (1.0 - (fEccentricity * Math.Cos(fEG))));
                fEG = fEG - fDeltaE;
            } while (Math.Abs(fDelta) > fAccuracy);

            fE = fEG;
            
            return fE;
        }

        public override int KeyFrameCount
        {
            get { return 0; }
        }
    }
}
