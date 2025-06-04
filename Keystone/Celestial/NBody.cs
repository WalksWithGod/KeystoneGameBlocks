using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Celestial
{
    class NBody
    {
    }
}

// I think trying to implement n-body for planets is insane.   I should only be doing this for craft.
// However the thing I wondered about was "how then do you get a moon to orbit a planet orbiting a star 
// with a fixed path?  You can't really.   but, that doesn't mean the solution is to 
// use full blown nbody physics integration.  Not at all, the solution is to simply use hierarchical animation!
// 

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
namespace DirectX
{    
    // http://www.physicsforums.com/showthread.php?t=262733
    // http://www.gamedev.net/topic/481263-c-particle-to-particle-gravity/
    public class Newton    
    {        
        // Newton v1.1        
        // Author: Mykel Ne'Oa Ikon        
        // Description: This class is a functional n-body system        
        //it requires a DirectX Based project that is setup to the point that a blank scene        
        // is being drawn. You can then invoke the class and use the functions        
        //        
        // Add_NBody - adds a n-body to the system        
        // Delete_NBody - deletes a n-body from the system        
        // Toggle - by passing true(On) or false (Off) you can cause the particle to be affected by other
        // particles gravity or not, other particles will still be affected by the off particle       
        Int32 Max = 500; // This is the maximum number of particles we can have through newton               
        Int32 N = 0; // This is the particle counter, when we add a particle this increments, and decrements when we delete it        
        n_body[] newton = new n_body[1000]; // Our array for our particle structure        
        float GC = 6.673e-11f; // Our gravitational constant, .00000000006.673        
        float MC = 1e+22f; // We multiply this by our radius, double squared.                
        float GD = 1000000000000; // our galactic distance, makes the force seem like we're at large
                

         public struct n_body        
        {            
            public Mesh nmesh;            
            public float ncutoff;            
            public Vector3 nposition;            
            public Vector3 nacceleration;            
            public Material nmaterial;            
            public float nradius;            
            public float nmass;            
            public bool ngravity;        
        }


        // This method allows us to add a particle        
        public void Add_NBody(Mesh mesh, Vector3 position, Vector3 acceleration, Color diffuse, float radius)        
        {            
            if (N < Max)            
            {                                
                newton[N].nradius = radius; // Pass the radius                
                newton[N].nmass = (radius * radius) * MC; // Turn the radius into mass                
                newton[N].ncutoff = (radius * radius) + radius;// * (newton[i].nradius + newton[i].nradius); 

                // Set the gravity cutoff distance                
                newton[N].nmesh = mesh; // Set the mesh                
                newton[N].nposition = position; // Set the mesh position                
                newton[N].nacceleration += acceleration; // Define the acceleration                
                newton[N].nmaterial.Diffuse = diffuse; // Set the diffuse color                
                newton[N].ngravity = true; // Turn the particle on                
                N += 1; // Increase our particle counter            
            }        
        }        
        
        public void Add_NBody(Mesh mesh, Vector3 position, Vector3 acceleration, Color diffuse, float radius, Boolean status)        
        {           
            if (N < Max)            
            {                
                newton[N].nradius = radius; // Pass the radius                
                newton[N].nmass = (radius * radius) * MC; // Turn the radius into mass                
                newton[N].ncutoff = (radius * radius) + radius;// * (newton[i].nradius + newton[i].nradius); 
                // Set the gravity cutoff distance                newton[N].nmesh = mesh; 
                // Set the mesh                
                newton[N].nposition = position; 
                // Set the mesh position                
                newton[N].nacceleration += acceleration; // Define the acceleration                
                newton[N].nmaterial.Diffuse = diffuse; // Set the diffuse color                
                newton[N].ngravity = status; // Turn the particle on                
                N += 1; // Increase our particle counter            
            }        
        } 
       
        // This method allows us to delete a particle        
        public void Delete_NBody(Int32 ndex)        
        {            
            if (N > 0) // Providing we have items to work with            
            {                
                for (Int32 x = ndex; x <= N; x++) // loop through each one at the position we want to delete                
                {                    newton[x] = newton[x + 1]; // And copy the one ahead of it                
                }                
                N -= 1; // Decrement our counter            
            }        
        }
        
        // This method allows us to turn off/on a particle by passing off(False) or on(True) to it        
        public void Toggle(Int32 ndex, Boolean status)        
        {            
            newton[ndex].ngravity = status;        
        }
        
                // This method renders our particles        
        public void Render(Device device)       
        {            
            for (Int32 i = 0; i < N; i++) // Go through each particle            
            {                
                if (newton[i].nmass != 0) // Check to make sure it isn't blank                
                {                    
                    device.Transform.World = Matrix.Translation(newton[i].nposition); // Position our particle                    
                    device.Material = newton[i].nmaterial; // Apply its material                    
                    newton[i].nmesh.DrawSubset(0); // Draw it                
                }            
            }        
        } 

        
        // This method calculates the gravitation on each particle, then updates the position   
        // http://wiki.vdrift.net/Numerical_Integration#Basic_Verlet.2FVelocity_Verlet
        // It's shown that for underdamped simulations 
        // Oscillating systems such as planetary orbits are underdamped. 4
        // modified verlet is most stable.
        public void Update()        
        {            
            #region process gravity            
            if (N > 0) // If we have particles to process            
            {                
                for (Int32 i = 0; i < N; i++) // Loop through our particles and update the positions                
                {                    
                    newton[i].nposition += newton[i].nacceleration;
                
                }                
                for (Int32 i = 0; i < N; i++) // Go though our particles                
                {                    
                    // When we're ready, on this line we will move the position + acceleration bit in the loop above                    
                    for (Int32 ii = 0; ii < N; ii++) // Go through our particles                    
                    {                        
                        if (i != ii) // If the focus(i) and target(ii) particles aren't the same                        
                        {                           
                            if (newton[i].ngravity != false) // If our particle can calculate gravity                            
                            {                                
                                float d = v3dist(newton[i].nposition, newton[ii].nposition); // Get the distance between them                                
                                if (v3collision(i,ii, d) == false) // If we're greater than the cutoff distance                                
                                {                                        
                                    float g = v3grav(newton[ii].nmass, newton[i].nposition, newton[ii].nposition); // Get the gravitational force                                        
                                    Vector3 vd = v3direction(newton[i].nposition, newton[ii].nposition, d); // get a vector representing a normalized direction to our target                                       
                                    newton[i].nacceleration += new Vector3(vd.X * g, vd.Y * g, vd.Z * g); // calculate our acceleration                                
                                }                             
                            }                        
                        }                                            
                    }                
                }            
            }            
            #endregion

            // Modified velocity verlet
            // note:  state.x = position,  state.a = acceleration, state.v = velocity
            //----------------------------------------------------------------------------------
            //if (!have_oldaccel)
            //oldaccel = system.GetAcceleration(state)
            // state.x += state.v*dt + 0.5*oldaccel*dt*dt;
            // state.v += 0.5*oldaccel*dt;
            // real a = system.GetAcceleration(state);
            // state.v += 0.5*a*dt;
             
            // oldaccel = a;
            // have_oldaccel = true;

        }        
              
        
        // This method calculates the gravity        
        public float v3grav(float mass, Vector3 object1, Vector3 object2)        
        {            
            // this function will calculate true gravity between 2 objects            
            float dist = (float)v3dist(object1, object2); // Get the distance between the 2 points            
            float gf = (GC * mass); // Get the gravitational force            
            return gf / (dist * GD); // Return the gravitational force after dividing it by r2 and scaling it        
        }        
        
        // This method calculates a scalar vector toward our target        
        public Vector3 v3direction(Vector3 object1, Vector3 object2, float d)        
        {           
            float x = (object1.X - object2.X); // Get our x distance           
            float y = (object1.Y - object2.Y); // Get our y distance            
            float z = (object1.Z - object2.Z); // Get our z distance            
            return new Vector3(-(x / d), -(y / d), -(z / d)); // Return our vector        
        }        
        
        // Get the distance between 2 points        
        public float v3dist(Vector3 object1, Vector3 object2)       
        {            
            Double x = (object1.X - object2.X) * (object1.X - object2.X); // Get X            
            Double y = (object1.Y - object2.Y) * (object1.Y - object2.Y); // Get Y            
            Double z = (object1.Z - object2.Z) * (object1.Z - object2.Z); // Get Z            
            float dist = (float)(x + y + z); // Get distance            
            return dist; // Return distance        
        }        
        
        // Returns true if we have a collision with the two particles in question using sphere bound detection        
        public Boolean v3collision(Int32 object1, Int32 object2, float dist)        
        {                        
            float collisiondistance = newton[object1].ncutoff + newton[object2].ncutoff; // The collision distance is the sum of both particles cutoffs, while not exact it works            
            if (dist <= collisiondistance) // If we're closer than our colision distance, we're colliding            
            {                
                return true;  // return true            
            }            
            else            
            {                
                return false; // return false            
            }        
        }        
    }
}