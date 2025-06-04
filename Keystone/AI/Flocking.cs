using System.Collections;
using Keystone.Types;
using MTV3D65;

namespace Keystone.AI
{
    /// <summary>
    /// Group entity behavior methods.
    /// </summary>
    public class Flock
    {
		private double MAX_FORCE = 20;
    	private System.Collections.Generic.List<Keystone.Entities.Entity> mAgents; // An arraylist for all agents

        public Flock()
        {
        	mAgents = new System.Collections.Generic.List<Keystone.Entities.Entity> ();
        }


        public void AddAgent(Entities.Entity agent)
        {
            mAgents.Add(agent);
        }

        // we peform the following algorithm:
        // Compute the sum of all locations of all boids within 100 pixels
        // Compute the average location (i.e. divide by total neighboring agents)
        // Compute the steering vector towards the average location
        // With the above method, along with two more similar ones for separation and alignment, we now have all the elements for flocking, i.e. take the results of the three rules, weight them appropriately, and accumulate them together in the object’s acceleration.

        
        public void Update()
        {
            for (int i = 0; i < mAgents.Count; i++)
            {
                Entities.Entity agent = mAgents[i];
                KeyCommon.Data.UserData userdata = (KeyCommon.Data.UserData)agent.GetCustomPropertyValue ("userdata"); // agent.CustomData.GetDouble ("steer_wander_theta"); // agent.WanderTheta
            
            	double maxSpeed = userdata.GetDouble ("max_speed"); // agent.MaxSpeed; TODO: max speed can be variable based on damage
            	double maxForce = userdata.GetDouble ("max_force"); // agent.MaxForce; TODO: max force can be variable based on damage
            
                Vector3d acceleration = DoFlocking(agent.Translation, agent.Velocity, agent.Acceleration, maxSpeed, maxForce, mAgents); // Passing the entire list of agents to each agent individually
            	agent.Acceleration = acceleration;
            }
        }

        // We accumulate a new acceleration each time based on three rules
        private Vector3d DoFlocking(Vector3d agentPosition, Vector3d agentVelocity, Vector3d agentAcceleration, double maxSpeed, double maxForce, System.Collections.Generic.List<Keystone.Entities.Entity> flock)
        {
            Vector3d sep = SeparateFrom(agentPosition, agentVelocity, maxSpeed, maxForce, flock); // Separation
            Vector3d ali = AlignWith(agentPosition, agentVelocity, maxSpeed, maxForce, flock); // Alignment
            Vector3d coh = Cohesion(agentPosition, agentVelocity, maxSpeed, maxForce, flock); // Cohesion

            // Arbitrarily weight these forces
            sep *= 2.0f;
            ali *= 1.0f;
            coh *= 1.0f;

            // Add the force vectors to acceleration
            agentAcceleration += sep;
            agentAcceleration += ali;
            agentAcceleration += coh;
        
            return agentAcceleration;
        }

        // Cohesion
        // For the average location (i.e. center) of all nearby agent, calculate steering vector towards that location
        private Vector3d Cohesion(Vector3d agentPosition, Vector3d agentVelocity, double maxSpeed, double maxForce, System.Collections.Generic.List<Keystone.Entities.Entity> flock)
        {
            double neighbordist = 50.0f;
            Vector3d sum = Vector3d.Zero(); // Start with empty vector to accumulate all locations
            int count = 0;
            for (int i = 0; i < flock.Count; i++)
            {
                Entities.Entity other = flock[i];
                double d = Vector3d.GetDistance3d(agentPosition, other.Translation);
                if (d > 0 && d < neighbordist)
                {
                    sum += other.Translation;
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= (double) count;
                return global::Keystone.AI.Steering.Steer(agentPosition, sum, agentVelocity, maxSpeed, maxForce, 0);
            }
            return sum;
        }

        // Separation
        // Method checks for nearby agents and steers away
        private Vector3d SeparateFrom(Vector3d agentPosition, Vector3d agentVelocity, double maxSpeed, double maxForce, System.Collections.Generic.List<Keystone.Entities.Entity> flock)
        {
            double desiredseparationSq = 25.0d * 25d;
            Vector3d sum = Vector3d.Zero();

            int count = 0;
            // For every agent in the system, check if it's too close
            for (int i = 0; i < flock.Count; i++)
            {
                Entities.Entity other = flock[i];
                double d = Vector3d.GetDistance3dSquared(agentPosition, other.Translation);
                // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                if ((d > 0) && (d < desiredseparationSq))
                {
                    // Calculate vector pointing away from neighbor
                    Vector3d diff = agentPosition - other.Translation;
                    Vector3d.Normalize(diff);
                    diff /= d; // Weight by distance
                    sum += diff;
                    count++; // Keep track of how many
                }
            }
            // Average -- divide by how many
            if (count > 0)
            {
                sum /= (double) count;
            }
            return sum;
        }

        // Alignment
        // For every nearby agent in the system, calculate the average velocity
        private Vector3d AlignWith(Vector3d agentPosition, Vector3d agentVelocity, double maxSpeed, double maxForce, System.Collections.Generic.List<Keystone.Entities.Entity> flock)
        {
            double neighbordistSq = 50.0f*50.0f;
            Vector3d sum = Vector3d.Zero();
            int count = 0;
            for (int i = 0; i < flock.Count; i++)
            {
                Entities.Entity other = flock[i];
                double d = Vector3d.GetDistance3dSquared(agentPosition, other.Translation);
                if ((d > 0) && (d < neighbordistSq))
                {
                    sum += other.Velocity;
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= (double) count;
                sum = Vector3d.Limit(sum, MAX_FORCE);
            }
            return sum;
        }

        

    }
}