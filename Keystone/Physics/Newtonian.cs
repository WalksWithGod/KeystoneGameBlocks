using System;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.Physics
{
    public class Newtonian
    {
        // pseudo n-body, not real n-body
        // parallel
        // unsafe version
        // fixed time step
        // uses leapfrog integration
        // http://www.ademiller.com/blogs/tech/2009/03/c-optimization-revisited-part-1-algorithms-and-bill-clinton/
        // http://en.wikipedia.org/wiki/Leapfrog_integration
        // leapfrog is stable and reversable and thus i think good for networked simulations and vcr replay functions
        // double mTimeStep;

        //Dictionary<string, Influences> mInfluenceMap;

#if UNSAFE
#endif
        // SIMULATION - LEVEL OF DETAILS
        // for AI craft that is beyond any fully paged in sectors client side and assuming single player
        // game but where we also want to simulate the universe still,
        // we use a seperate branch of code that completely ignores physical simulation
        // and switches to an analytical resolution of battles, commerce updates, etc.
        // - ie we just roll the dice
        // - ie. a ship can have a vague "position" and then based on some mission objective
        //   such as this ship is trading and going to land on a planet, we can just compute whether
        //   it's landed or not based solely on an ETA countdown.
        // - LOD for simulation.
        //      - but how do we merge position simulation then with mission tasks?  That is sort of
        //        a tainting of the physical lod simulation which should not need to know such things.
        //      - we could simplify that by having the logic simulation compute destination coordinates
        //        and then the physical simulation can advance the position simply and ignore heavy physics
        //        calcs.  Thus the physics doesn't need an ETA var to track.  It just will move the item
        //        to it's destination.  Then the logic sim can wait for that arrival event and perform a new task.
        //  - how does this relate to fixed time step viewing of a ship in another system as spectator?
        //    i think such viewing cannot be allowed.  if it was never properly simulated to begin with
        //    then it cannot be viewed because even if we were to use seed values and then try to simulate
        //    it as replay for first time, the simulated results may not match the rolled dice version results.
        //
        // celestial bodies need to register for this specific type of physical updating
        // - fixed orbits - scripted?  how does physics vs scripted position updates fit?
        //                 - seems to me it's the same difference as logic for AI steering
        //                 - vs physical responses
        //
        // - axial rotations
        // - cloud rotations
        // - sphere to sphere collisions against ships
        // 

        // typically, physics libraries update positions
        // with the expectations of collisions... 

        // Actually we do not do n-body at all
        // we only integrate every ship with every celestial body (greater than minimum size which 
        // excludes most asteroids)
        // in a sector or range of sectors.
        // Thus, a given ship will only interact with n bodies, but the ship itself
        // will not affect those bodies themselves.  


        // StarSystem.GetGravityAffectors() // how do we track which systems bodies a ship should be affected by?
        //  - does each ship track the system's it's affected by then?
        //  - or perhaps we can compute it every frame based on it's current region?


        // StarSystem.GetGravityEmitters() // results are cached and only recomputed if bodies are added/removed
        // ship's position is now only integrated using those bodies

        // for each ship, we will thread it's execution as a task
        // in the future when we upgrade, we can use parallel for instead

        private const double GRAVCONST = 6.67428E-11;

        //// Kinetic energy (Joules) = 1/2 * mass * velocity^2 (mass in kg, velocity in m/s)
        //// Kinetic energy (kilotons) = (KE in Joules) / (4.185e12 joules/KT)
        ////float GetKEJoules(C3DObject &obj)		{ return 500000.0f * m_fMass * (m_vVelocity-obj.m_vVelocity).MagnitudeSquared(); }
        ////float GetKEKilotons(C3DObject &obj)		{ return 1.19474e-7f * m_fMass * (m_vVelocity-obj.m_vVelocity).MagnitudeSquared(); }


        // june.30.2012
        // check outfollowing code from gamedev
        //http://www.gamedev.net/topic/481263-c-particle-to-particle-gravity/ 


        // http://en.wikipedia.org/wiki/Gravitational_constant
        //
        // s_p_oneil@hotmail.com 
        // Copyright (c) 2000, Sean O'Neil 
        // All rights reserved. 
        public static Vector3d GravityVector(Vector3d positionMassiveBody, Vector3d positionTinyBody, double massMassiveBody)
        {
            // TODO: how can we here get the pull of ALL relevant gravity emitters?  Remember we want to use GravityProduction and GravityConsumption (our vehicles) with a Region distribution filter
            //       combine those and only then pass to Accelerate() 
            Vector3d direction = positionMassiveBody - positionTinyBody;

            // get a vector representing a normalized direction to our target
            double length;
            direction = Vector3d.Normalize(direction, out length);
            Vector3d acceleration = direction * GravityPull(length, massMassiveBody); /// Math.Sqrt(fDistSquared);
            return acceleration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fDistSquared"></param>
        /// <param name="mass"></param>
        /// <returns>Meters per second</returns>
        private static double GravityPull(double distance, double mass)
        {
            double distanceSquared = distance * distance;
            double pull = GRAVCONST * (mass / distanceSquared);
            return pull;
        }

        // http://www.scirra.com/tutorials/67/delta-time-and-framerate-independence/page-1
        // http://www.gamedev.net/reference/programming/features/verlet/
        // http://stackoverflow.com/questions/655843/orbital-mechanics
        // http://www.universesandbox.com/
        // http://gafferongames.com/game-physics/integration-basics/
        //http://lonesock.net/article/verlet.html
        //http://www.youtube.com/watch?v=s-8VPZUW9yI

        // https://julesjacobs.com/2019/03/15/leapfrog-verlet.html
        // https://en.wikipedia.org/wiki/Verlet_integration
        /// <summary>
        /// 
        /// </summary>
        /// <param name="forceDelta">This is actually the delta of forces acting on the entity</param>
        /// <param name="currentTranslation"></param>
        /// <param name="currentAcceleration">Assumes that the acceleration is calculated properly from all available engine thrust and taking into account ship mass.</param>
        /// <param name="currentVelocity"></param>
        /// <param name="elapsedSeconds"></param>
        /// <param name="resistance">0.0 - 1.0f represents percentage drag such as atmospheric or nebulae</param>
        public static void LeapFrogVerlet(Vector3d forceDelta, ref Vector3d currentTranslation,
                                            ref Vector3d currentAcceleration, ref Vector3d currentVelocity,
                                            double elapsedSeconds, float resistance)
        {
            // TODO: For our acceleration force, Consumption_Thrust in Ship.cs must properly 
            //       convert thrust to acceleration force.  Currently I'm just directly using
            //       thrust as acceleration.  That's ok for testing for now.
            double halfElapsed = elapsedSeconds * 0.5d;
            // integrate previous "currentAccleration" into Velocity
            currentVelocity += forceDelta * halfElapsed;
            // integrate velocity into Translation
            currentTranslation += currentVelocity * elapsedSeconds;

            // apply forceDelta and integrate new "currentAcceleration" value into velocity
            // when this is done, we will have combined half the previous and have the current into
            // our velocity.  This is why it's called LeapFrog since it combines previous with current
            currentAcceleration = forceDelta; // * elapsedSeconds;  // TODO: Feb.25.2015 - ensure "* elapsedSeconds" is correct
            currentVelocity += currentAcceleration * halfElapsed;


            // TODO: resistance if used should be consumed just like acceleration.  
            // and so passed in like Entity.Force, Entity.Resistance which assumes always resistance
            // in the direction of travel.
            // as well as angular rotation
            //
            //    //if(resistance > DELTA)
            //    //    currentVelocity *= 1.0f - resistance * elapsedSeconds;

            //    System.Diagnostics.Debug.WriteLine("Speed = " +
            //        (entity.Velocity.Length / elapsedSeconds).ToString() + "Accel = " +
            //        (entity.Acceleration.Length / elapsedSeconds).ToString());
        }



        #region RK4 (Runge-Kutta)  (i might have velocity and accelerations mixed up in State and Derivative...
        //struct State
        //{
        //    float x;
        //    float v;
        //};

        //struct Derivative
        //{
        //    float dx;
        //    float dv;
        //};

        //State interpolate(const State &previous, const State &current, float alpha)
        //{
        //    State state;
        //    state.x = current.x*alpha + previous.x*(1-alpha);
        //    state.v = current.v*alpha + previous.v*(1-alpha);
        //    return state;
        //}

        //float acceleration(const State &state, float t)
        //{
        //    const float k = 10;
        //    const float b = 1;
        //    return - k*state.x - b*state.v;
        //}

        //Derivative evaluate(const State &initial, float t)
        //{
        //    Derivative output;
        //    output.dx = initial.v;
        //    output.dv = acceleration(initial, t);
        //    return output;
        //}

        //Derivative evaluate(const State &initial, float t, float dt, const Derivative &d)
        //{
        //    State state;
        //    state.x = initial.x + d.dx*dt;
        //    state.v = initial.v + d.dv*dt;
        //    Derivative output;
        //    output.dx = state.v;
        //    output.dv = acceleration(state, t+dt);
        //    return output;
        //}

        //void integrate(State &state, float t, float dt)
        //{
        //    Derivative a = evaluate(state, t);
        //    Derivative b = evaluate(state, t, dt*0.5f, a);
        //    Derivative c = evaluate(state, t, dt*0.5f, b);
        //    Derivative d = evaluate(state, t, dt, c);

        //    const float dxdt = 1.0f/6.0f * (a.dx + 2.0f*(b.dx + c.dx) + d.dx);
        //    const float dvdt = 1.0f/6.0f * (a.dv + 2.0f*(b.dv + c.dv) + d.dv);

        //    state.x = state.x + dxdt*dt;
        //    state.v = state.v + dvdt*dt;
        //}
        public struct State
        {
            public Vector3d Position;
            //public Quaternion Rotation;
            public Vector3d Velocity;
        }

        struct Derivative
        {
            public Vector3d Velocity;
            public Vector3d Acceleration;
        }

        static Vector3d acceleration(State state, float t)
        {
            const float k = 10;
            const float b = 1;
            return -k * state.Position - b * state.Velocity;
        }

        static Derivative evaluate(State initial, float t)
        {
            Derivative output;
            output.Velocity = initial.Velocity;
            output.Acceleration = acceleration(initial, t);
            return output;
        }

        static Derivative evaluate(State initial, float t, float dt, Derivative d)
        {
            State state;
            state.Position = initial.Position + d.Velocity * dt;
            state.Velocity = initial.Velocity + d.Acceleration * dt;
            Derivative output;
            output.Velocity = state.Velocity;
            output.Acceleration = acceleration(state, t + dt);
            return output;
        }

        public static void Integrate(ref State state, float t, float dt)
        {
            Derivative a = evaluate(state, t);
            Derivative b = evaluate(state, t, dt * 0.5f, a);
            Derivative c = evaluate(state, t, dt * 0.5f, b);
            Derivative d = evaluate(state, t, dt, c);

            Vector3d dxdt = 1.0f / 6.0f * (a.Velocity + 2.0f * (b.Velocity + c.Velocity) + d.Velocity);
            Vector3d dvdt = 1.0f / 6.0f * (a.Acceleration + 2.0f * (b.Acceleration + c.Acceleration) + d.Acceleration);

            state.Position = state.Position + dxdt * dt;
            state.Velocity = state.Velocity + dvdt * dt;
        }
    #endregion


        // an alternative approach 
        // https://gamedev.stackexchange.com/questions/189727/rotating-quaternion-smoothly-with-acceleration-and-deceleration
        //
        //        // Aurora 4x is interesting game that has tons of GUI elements.  I like the top row of icons which is better than menu dropdowns
        //        // https://www.gamedev.net/forums/topic/702839-spaceship-controls-autopilot/?page=2
        //        // find the new rotation

            /// <summary>
            /// Computes a new rotation after applying an angular velocity.
            /// </summary>
            /// <param name="angularVelocity">entity.AngularVelocity</param>
            /// <param name="currentRotation">Typically should be entity.DerivedRotation</param>
            /// <param name="elapsedSeconds"></param>
            /// <returns></returns>
        public static Quaternion RotationAfterAngularVelocity(Vector3d angularVelocity, Quaternion currentRotation, double elapsedSeconds)
        {
            // todo: force needs to take intou account the mass of the vehicle

            double length = angularVelocity.Length;
            if (length < 1E-6d)
                return currentRotation;  // Otherwise we'll have division by zero when trying to normalize it later on

            // Convert the rotation half vector to quaternion. The following 4 lines are very similar to CreateFromAxisAngle method.
            double half = length * 0.5d;
            double sin = Math.Sin(half);
            double cos = Math.Cos(half);
            // Instead of normalizing the axis, we multiply W component by the length of it. This method normalizes result in the end.
            Quaternion rotation = new Quaternion(angularVelocity.x * sin, angularVelocity.y * sin, angularVelocity.z * sin, length * cos);

            rotation = Quaternion.Normalize(rotation);

            rotation *= currentRotation;
            rotation = Quaternion.Normalize(rotation);

            // The following line is not required, only useful for people. Computers are fine with 2 different quaternion representations of each possible rotation.
            if (rotation.W < 0) rotation = Quaternion.Negate(rotation);


            return rotation;
        }


        // todo: find the index of the closest waypoint and use it to assign "heading" and "destination" to the crew_station_helm.css
        // todo: since we are following waypoints, the key is to not treat each individual waypoint as a final destination since we need to start turning for the next waypoint before reach the current
        // todo: likewise, there is no flip maneuver
        // todo: the distance between adjacent waypoints can be used to determine a max velocity 

        // https://gamedev.stackexchange.com/questions/75137/spaceship-acceleration-for-following-waypoints

        // https://gamedev.stackexchange.com/questions/201569/how-to-navigate-a-moving-2d-spaceship-under-newtonian-mechanics-to-a-point-in-sp

        // https://gamedev.stackexchange.com/questions/189707/moving-ship-to-new-destination-when-already-having-a-velocity

        // https://gamedev.stackexchange.com/questions/189727/rotating-quaternion-smoothly-with-acceleration-and-deceleration

        // https://gamedev.stackexchange.com/questions/75137/spaceship-acceleration-for-following-waypoints

        // https://www.google.com/search?client=firefox-b-1-e&q=following+waypoints+newtonian+physics

        // https://gamedev.stackexchange.com/questions/201569/how-to-navigate-a-moving-2d-spaceship-under-newtonian-mechanics-to-a-point-in-sp

        // todo: we also need to assign a max velocity when traversing the waypoints

        // todo: set the helm state to transition to orbit from any previous maneuver.
        //       It should then travel to the closet waypoint and begin following from there and if "loop == true" then after reaching last waypoint, restart at index 0
        // after having done so, the orbit waypoints should show up in the hud
        // todo: the hud should grab the helm Entity after it grabs the Vehicle

        // todo: in the future when we want to transition from travel_to to orbit, we simply move to the nearest waypoint and begin following the waypoints

        // todo: i think if either the vehicle or target's acceleration or velocity changes, we need to recompute the heading
        // TODO: this should be easy to test as all it requires is a target aim position which we can set in our existing Vehicle_Intercept() as a heading and not change our scripts code at all.
        // https://www.reddit.com/r/Unity3D/comments/ccby9z/quadratic_target_interception_code_in_comments/
        // https://www.reddit.com/r/Unity3D/comments/ccby9z/quadratic_target_interception_code_in_comments/
        // todo: i think the problem with this function for vehicles as opposed to missiles or other projectils is that
        //       vehicle will intercept but then quickly overshoot and just keep going. What we want is to
        //       intercept and then match velocity and acceleration if applicable
        // todo: i could perhaps conceptulalize waypoints as "moving" toward the next waypoint as the vehicle gets closer



        //All in world space! Gets point you have to aim to
        //NOTE: this will break with infinite speed projectiles!
        //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
        public static Vector3d GetTargetLeadingPositionQuadratic(Vector3d launchPosition, Vector3d launcherVelocity, Vector3d projectileAcceleration, Vector3d targetPosition, Vector3d targetVelocity, Vector3d targetAcceleration, double distance, double projectileSpeed, int iterations)
        {
            Vector3d pT = targetPosition - launchPosition;
            Vector3d vT = targetVelocity - launcherVelocity;
            Vector3d aT = targetAcceleration;
            double s = projectileSpeed;
            Vector3d aP = projectileAcceleration;

            Vector3d accel = aT - aP;

            //time to target guess
            double guess = distance / s;

            if (iterations > 0)
            {
                //quartic coefficients
                double a = Vector3d.DotProduct(accel, accel) * 0.25d;
                double b = Vector3d.DotProduct(accel, vT);
                double c = Vector3d.DotProduct(accel, pT) + Vector3d.DotProduct(vT, vT) - s * s;
                double d = 2f * Vector3d.DotProduct(vT, pT);
                double e = Vector3d.DotProduct(pT, pT);

                //solve with newton
                double finalGuess = SolveQuarticNewton(guess, iterations, a, b, c, d, e);

                //use first guess if negative or zero
                if (finalGuess > 0d)
                {
                    guess = finalGuess;
                }
            }

            Vector3d travel = pT + vT * guess + 0.5f * aT * guess * guess;

            return launchPosition + travel;
        }

       


        // todo: lets try deriving the 2d heading based on the result
        //All in world space! Gets launch velocity you have to aim to
        //NOTE: this will break with infinite speed projectiles!
        //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
        public static Vector3d GetTargetLeadingVelocityQuadratic(Vector3d launchPosition, Vector3d launcherVelocity, Vector3d projectileAcceleration, Vector3d targetPosition, Vector3d targetVelocity, Vector3d targetAcceleration, double distance, double projectileSpeed, int iterations)
        {
            Vector3d pT = targetPosition - launchPosition;
            Vector3d vT = targetVelocity - launcherVelocity;
            Vector3d aT = targetAcceleration;
            double s = projectileSpeed;
            Vector3d aP = projectileAcceleration;

            Vector3d accel = aT - aP;

            //time to target guess
            double guess = distance / s;

            if (iterations > 0)
            {
                //quartic coefficients
                double a = Vector3d.DotProduct(accel, accel) * 0.25d;
                double b = Vector3d.DotProduct(accel, vT);
                double c = Vector3d.DotProduct(accel, pT) + Vector3d.DotProduct(vT, vT) - s * s;
                double d = 2f * Vector3d.DotProduct(vT, pT);
                double e = Vector3d.DotProduct(pT, pT);

                //solve with newton
                double finalGuess = SolveQuarticNewton(guess, iterations, a, b, c, d, e);

                //use first guess if negative or zero
                if (finalGuess > 0d)
                {
                    guess = finalGuess;
                }
            }

            Vector3d travel = pT + vT * guess + 0.5f * aT * guess * guess;

            Vector3d launchVelocity = travel / guess - 0.5f * aP * guess;

            return launchVelocity;
        }

        static double SolveQuarticNewton(double guess, int iterations, double a, double b, double c, double d, double e)
        {
            for (int i = 0; i < iterations; i++)
            {
                guess = guess - EvalQuartic(guess, a, b, c, d, e) / EvalQuarticDerivative(guess, a, b, c, d);
            }
            return guess;
        }

        static double EvalQuartic(double t, double a, double b, double c, double d, double e)
        {
            return a * t * t * t * t + b * t * t * t + c * t * t + d * t + e;
        }

        static double EvalQuarticDerivative(double t, double a, double b, double c, double d)
        {
            return 4f * a * t * t * t + 3f * b * t * t + 2f * c * t + d;
        }

        // https://www.google.com/search?client=firefox-b-1-e&q=newtonian+intercept+match+velocity

        // https://stackoverflow.com/questions/7278636/re-space-physics-for-missiles-spaceships
        // https://www.gregegan.net/ORTHOGONAL/E3/SoftInterception.html

        // https://www.codeproject.com/Articles/990452/Interception-of-Two-Moving-Objects-in-D-Space
        // https://stackoverflow.com/questions/55691355/accelerate-decelerate-towards-moving-target-and-hitting-it
        // https://forum.unity.com/threads/intercept-prediction-in-a-2d-game-and-teh-maths.379212/


        public void Update()
        {
            // how do we handle LOD?

            // TODO: also what about the Reptron Emission design pattern?
            // how is gravity emitted by our celestial bodies and received
            // by receptors on targets?

            // could we have a more generic solution as far as the physics updating goes
            // if we supply the celestial bodies with an gravity particle emitter
            // that flies and affects all things within it's sphere of influence
            // each frame that have gravity receptors attached to them?
            // http://www.systemshock.org/index.php?topic=2305.0
            // recall that system shock's ShockEd is essentially same engine for Thief DromEd
            //
            // the Key to using Receptrons and Emitters is that we dont have to have these seperate
            // systems like an NBody updater that things need to "register" and "unregister" too.
            // Instead we could have just one such updater that can track all relationships
            // between receptrons and emitters by spatial relationship the same way.
            // Thus radar emissions or gravity emissions are all tracked the same.
            // - same with thrust emitters
            //      - emission output is always credited to the top most assembly in the vehicle heirarchy
            //          - only exception is it will stop at assembly if it's connection type to 
            //          the parent is not static.. (eg like a dyanmic chain link)
            // - a star emits heat which causes damage as a product of distance
            //   thus if you're ship is far enough, zero damage will result.
            //    
            // So all of these examples are about how to design the architecture
            // to update these emitters and receivers... how to cache the influences
            // how to discover influences and when to recompute them
            // what hertz to call Apply() on constant emitters upon Receptors
            // - and how to deal with diffusion within celledregions
            // - what about potential line of sight blocking of some emissions
        }
    }
}
