using System;
using Keystone.Types;
using MTV3D65;

namespace Keystone.AI
{
    /// <summary>
    /// http://www.shiffman.net/teaching/nature/steering/
    /// </summary>
    public class Steering
    {
        /// <summary>
        /// If we use fixed timesteps and a limit to maximum velocity, we should be able to
        /// avoid situations where we overshoot our targets in such a way that is unrecoverable.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector3d Steer(Vector3d agentPosition, Vector3d targetPosition, Vector3d agentVelocity, double maxSpeed, double maxForce)
        {
            return Steer(agentPosition, targetPosition, agentVelocity, maxSpeed, maxForce, 0f);
        }

        // TODO: Avoidance and such... find out what members to add to ISteerable for avoidance behavior
        // we might need to include NavNet stuff here?
        // as well as the other agents so we can determine collision with others


        //Arrival — Arrival can be achieved in an identical fashion as seeking, 
        //only instead of pursuing a target at maximum velocity, 
        //the object slows down as it approaches the destination. One solution
        //for implementing this behavior is to modify the 
        //steering vector calculation as follows (note in the above examples for this week,
        // the steering method receives a true 
        //or false flag to indicate whether it should apply the distance based damping or not). 
        // Here, the magnitude of the “desired”
        //vector shrinks as the object approaches the destination. 
        //For a full explanation of “arrival”, visit: http://www.red3d.com/cwr/steer/Arrival.html.
        // Steer() -- A method that calculates a steering vector towards a target
        //Steering Vector = “Desired Vector” minus “Velocity” where “desired vector” is defined 
        //as the vector pointing from the object’s location directly towards the target
        //Our method receives a location vector (”target”) and returns a force vector (”steer”) as below:
        // A method that calculates a steering vector towards a target
        public static Vector3d Steer(Vector3d agentPosition, Vector3d targetPosition, Vector3d agentVelocity, double maxSpeed, double maxForce, double slowDownStartDistance)
        {
        	// WARNING: slowDownStartDistance really should be a speed that allows us to reduce our velocity in sufficient time to stop!
        	//          or else we WILL OVERSHOOT.  This steer does not otherwise apply breaking as long as distance > 0 and no slowDownStartDistance specified!!!
            if (slowDownStartDistance < 0) slowDownStartDistance = 0;
            	
            // The steering vector
            Vector3d steer = Vector3d.Zero(); 
            Vector3d desired = targetPosition - agentPosition; // A vector pointing from the location to the target
            double distance;
            desired = Vector3d.Normalize(desired, out distance); // Distance from the target is the magnitude of the vector

            if (distance - float.Epsilon < 0) return steer;
        
            // calc steering 
            // Two options for desired vector magnitude (1 -- based on distance, 2 -- maxspeed)
            if (distance < slowDownStartDistance)
                // This damping is somewhat arbitrary
            	desired = desired * (maxSpeed * (distance / slowDownStartDistance));    
            else
                // give desired a magnitude determined by maxspeed
                desired = desired * maxSpeed;

            // Steering (aka the new acceleration) = Desired minus velocity
            steer = desired - agentVelocity;

            //if (steer.y != 0)
            //    System.Diagnostics.Debug.WriteLine("AI.Steer() - y axis drift");
            
            // Limit to maximum steering force. 
            steer = Vector3d.Limit(steer, maxForce); 
            return steer;
        }

        // TODO: Wander should be a Behavior node
        //Wandering — Reynolds’ method for wandering is a bit more complex. It involves steering towards a random
        //point on a circle projected at a given length in front of the the object. Normally, we think of wandering 
        //as applying a random steering vector each frame of animation. Reynolds solution is more sophisticated as it 
        //implements an ordered wandering where the steering at one moment is related to the previous one (note the conceptual 
        //similarity here to what perlin noise achieves for us). 
        //For a full explanation, visit: http://www.red3d.com/cwr/steer/Wander.html.
        public static Vector3d Wander(Vector3d agentPosition, Vector3d agentVelocity, double wanderTheta, double maxSpeed, double maxForce, out double resultTheta)
        {
        	// steer_wander_radius
        	// steer_wander_diameter
        	// steer_wander_change_rate
        	// steer_wander_theta
        	
            double wanderR = 16.0f; // Radius for our "wander circle" //TODO: these values should all be apart of the script file for this NPC
            double wanderD = 60.0f; // Distance for our "wander circle" projected out in front of the agent
            // NOTE: halfChangeRateDegrees is amount of angular change in direction that will occur on next call to Wander.  
            int changeRateDegrees = 30; // TODO: change rate should be variable along with wanderRadius and wanderDistance
            int halfChangeRate = changeRateDegrees / 2;
            
            // accumulate wander angle change with random change 
            wanderTheta += Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * Utilities.RandomHelper.RandomNumber(-halfChangeRate, halfChangeRate); 
			resultTheta = wanderTheta;
            
            // Now we have to calculate the new location to steer towards on the wander circle
            Vector3d circleloc = Vector3d.Normalize(agentVelocity); // normalize velocity to get heading
            circleloc *= wanderD; // Multiply by distance 
            circleloc += agentPosition; // Make it relative to agent's location

            Vector3d circleOffSet; 
            circleOffSet.x = wanderR * Math.Cos(wanderTheta);
            circleOffSet.y = 0;
            circleOffSet.z = wanderR * Math.Sin(wanderTheta);

            Vector3d targetPosition = circleloc + circleOffSet;

            // TODO: when do we check for obstacles
            return Steer(agentPosition, targetPosition, agentVelocity, maxSpeed, maxForce, 0); // Steer towards it

            // Render wandering circle, etc.
            // #if (DEBUG_DRAW_STEERING)
            // {
            // 		if (drawwandercircle) drawWanderStuff(loc, circleloc, target, wanderR);
            // }
        }

        // turret animation tests!  awesome
        // http://www.youtube.com/watch?v=L8pzvPA-gU4&NR=1
        // http://www.youtube.com/watch?v=ihB_8rao93U&NR=1
        // http://www.youtube.com/watch?v=_bQBFB-N0iw&NR=1
        // http://www.youtube.com/watch?v=TfRhQO5PcuQ&feature=related <-- Unity3d turrets... seems Unity is similar to what i can do in mind.. but my ui i think is easier
        // http://www.youtube.com/watch?v=4DDfzuHSxhc&feature=related
        // http://www.youtube.com/watch?v=MG0f_py6G4c&NR=1&feature=fvwp <-- space freighter for infinity
        
        /// <summary>
        /// 2D Y-Axis rotation with Up Vector = (0,1,0)
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Quaternion RotateTo(Vector3d direction)
        {		
        	// TODO: rotate amount is not clamped by a max rotationSpeed
        	
			// compute the actors rotation based on our velocity vector (only works for ground based games).
			// To make the character not "snap" to a rotation we
			// will want to turn the character prior to steering them.  In effect, test if the angle between the two is > say .5 at X distance
			// where if the X distance is very close, we will rotate more but if they are far enough away, this will normalize on its own.
	
			// NOTE: this is fine for rotating in a land based game, but for a 6DoF spacesim
			// scroll down to Vuli's post here http://www.truevision3d.com/phpBB2/viewtopic.php?t=10009&postdays=0&postorder=asc&start=0 
	
			
			// or maybe Vuli's version which doesnt use Quaternions isnt as simple as 
			// the RotateTo() ive added in Steering.cs
			// 2-D y-axis rotation (yaw) from direction vector
			double headingDegrees = Utilities.MathHelper.Heading2DDegrees(direction);
			Quaternion rotation = new Quaternion (headingDegrees * Utilities.MathHelper.DEGREES_TO_RADIANS, 0, 0); // y, x, z (yaw, pitch, roll)
			return Quaternion.Normalize (rotation);
        }
        
        /// <summary>
        /// 2D Y-Axis rotation with Up Vector = (0,1,0)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="rotationSpeed"></param>
        /// <param name="elapsedSeconds"></param>
        /// <returns></returns>
		public static Quaternion RotateTo(Vector3d v1, Vector3d v2, double rotationSpeed, double elapsedSeconds)
        {
            // http://www.gamedev.net/community/forums/topic.asp?topic_id=397044&whichpage=1&#2632461
            // Whats wrong with a straight vector to vector interpolation?
            // For a start he's wanting to rotate an orientation matrix so that its heading vector 
            // looks at a certain point in space (the intruder). Interpolating the heading vector 
            // is one thing but what do you do with the rest of the matrix? 
            // This type of vector interpolation only realy works for vectors which are already closely
            // aligned anyway since you're effectively interpolating across a line where as the vector 
            // would sweep out an arc. Since the length of the vector changes across the interpolation 
            // line this leads to uneven rotation, and in the worse case, when you want to get to a vector 
            // which is the exact opposite, results in zero rotation or instantaneous flipping. 
            // This is why its much better to rotate, or even better still do it with quaternions.

            //Vector3d result = new Vector3d();
            //if (time < 0 || time > 1.0f) throw new ArgumentOutOfRangeException();
            //Core._CoreClient.Maths.TVVec3Lerp(ref result, v1, v2, Clamp(rotationSpeed * elapsed)); 

            //// Generate a rotation that will rotate from pointing along v1 to pointing along v2.
            //double angleRadians = Math.Acos(Vector3d.DotProduct(v1, v2) / (v1.Length * v2.Length));
            //Vector3d perpVector = Vector3d.CrossProduct(v1, v2);
            //double angleDegrees = angleRadians  * 180 / Math.PI;

            double tmpPerFrameSpeed = rotationSpeed * elapsedSeconds;
            // TODO: it would be better to claimp the rotationSpeed passed in
            tmpPerFrameSpeed = Utilities.MathHelper.Clamp(tmpPerFrameSpeed, 0.0d, 1.0d); 
            Vector3d heading  = Vector3d.Lerp(v1, v2, tmpPerFrameSpeed);
            Vector3d axis = Vector3d.CrossProduct(v1, v2);
            //axis = playerMatrix.GetY(); 

            float angleDegrees = (float) Utilities.MathHelper.Heading2DDegrees(heading);

            Quaternion result = new Quaternion (axis, angleDegrees);
            return result;
            
        }
    }
}