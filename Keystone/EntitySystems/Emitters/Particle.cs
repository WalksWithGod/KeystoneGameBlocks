using System;
using Keystone.Types;

namespace Keystone.EntitySystems.Emitters
{
	/// <summary>
	/// LaserParticle is a type of physical partical that can interact
	/// with Entities in the world.  This is in contrast to purely cosmetic
	/// particles that only serve the aesthetics of the game and not the gameplay.
	/// </summary>
    internal struct Particle 
    {
    	// TODO: what if some particles are just full blown Entities that we pool so can easily/quickly initialize from cache
    	//       - recall that most of our particles do have simulation effects and are more than decoration.
    	
    	
    	// TODO: recall how perhaps Radar scans and such may work too.. starting with 
    	// scripted emission of a "radar" transmission particle that when consumed, 
    	// collider sends owner it's "contact" info
    	
    	// particle data here is just physical data and production for simulation.
    	// The EntitySystem holds the minimesh and textures.
    	// TODO: having references inside our struct winds up boxing the entire struct doesn't it?
    	// TODO: what about sorting particles into buckets by Owner and Region instead?  
    	//			- would that help with collisions by limiting search for colliders  
    	//			  to current region and maybe some adjacents?
    	//			- it adds maintenance though for these relatively short lived flyweight entities
    	//          - i think it really depends on just how many particles ill have... 
    	//            and how far they can travel... aka how many regions do we have to search for collisions
    	//
    	public Entities.Entity Owner;  // Entity owner, or what if each entity that is emitting, has it's own LaserParticleSystem 
    	public Portals.Region Region;  // we cache Region because Owner.Region may change frame to frame.
    	
    	
    	// Region
    	//	  ParticleEffect[]
    	//		- owner
    	//		- ParticleEmitter[]
    	//			- Particles[]
    	//          - Model -> Geometry 
    	//
    	
    	// when a particle crosses a zone boundary, we will simply check the "region" the particle is in, and perform region relative collisions and such.
    	// this requires we update the particle's position coordinate when the particle.region changes.
    	
    	// public int PhysicsComponentIndex;   <-- what if all particles could be updated more easily if vars like Scale, Position, Velocity could be held in a single large
    	//                                     array of records so that physics and collissions can be done in a cache efficient manner
    	//                                     the records could be divided by Region, the only real issue is, dealing with non Zone style regions
    	//                                     and getting something like a laser to travel from zone, through some door, into a ship that is on the ground
    	//                                     but which has it's own coordinate system, and then out the other side back into adjacent zone.  The way we've always
    	//                                     intended to do this is to check for portal collision with ray, then traverse through the portal to have the ray changed
    	//                                     to the new coordinate system.  
    	//                                            - entities crossing boundaries was supposed to be similar... though we have the added ability I think to check tiles
    	//                                     entities are entering for portals and have it so that portals attached to nested regions, must register with the exterior region.
    	//                                     so NPCs can navigate by examining the obstacle grid and can find portals to enter structures.
    	//                                            - but what about ray picking through a 3d tilemap?  i havent even thought about how we do that yet..
    	//                                     i mean, we're used to picking top down 2d tile edges when painting walls and terrain, but how do we have a laser traveling sideways hit a wall?  I think
    	//                                     if we project the 3d positions into 2d , we can reduce the number of 3d collision tests substantially...
    	//                                     - ULTIMATELY, we will need to just build and build on a system...
    	
    	public Vector3d Scale;     // size of graphic
    	public Vector3d PreviousPosition;
    	public Vector3d Position;  // region relative position
    	public Vector3d Rotation;  // minimeshes use Vector3d rotation
    	public Color Color;
    	
    	public Vector3d Velocity;  // velocity (speed * heading) is also axis we use for axial billboarding effect
    	public Vector3d Acceleration;
    	public double Triggered; 
        public float Age;     // how long this particle has been alive
        public bool Alive;         // state - inactive particles are not removed, they're just ignored
        
        
        // 1 - hash code so our scripts can register and then subsequentally access via that index
        // 2 - for now we will delay trying to sort particles by Region and Owner.
        //      - although i think we could potentially still use the same TVMinimeshes
        // 3 - could we have a system for lightweight/flyweight entities to all be handled 
        //     (as far as movement, boundary crossing, etc) handled by a base IEntitySystem so that
        //     any flyweight entity could benefit... particles, star digests, virtual starship convoys, merchants, etc)
        
        
        // do we need Owner above? Production already contains SourceEntityID which we could probably
        // assign actual Entity reference to since we know it
        //void T()
        //{
        //	Production[0].SourceEntityID
        //  System.Diagnostics.Debug.Assert (Production[0].SourceEntityID == Owner.ID);
        // Production[0].DistributionMode
		// Production[0].DistributionFilterFunc = 		
        // Production[0].SearchPrimitive 
        //}
        
        public void Rotate (double rotation)
        {
        	// for typcial NON-AXIAL Billboards this is always z rotation
        	// for AXIAL Billboards, rotation is direction of the billboard (eg bullet or laser heading)
        }
    }
   
    
//    /// <summary>
//    /// 
//    /// </summary>]
//    /// <remarks>
//    /// - Can divide the individual particle updates into threadable tasks
//    /// http://software.intel.com/en-us/blogs/2011/02/18/building-a-highly-scalable-3d-particle-system/
//    /// - Can implement full physics on each particle
//    /// - Can script individual particle behavior for more heavy particles like lasers, bullets
//    /// - Can re-use any minimesh shader for particles so that shadowing, lighting, deferred paths
//    /// for particles is exactly the same as for any minimesh.
//    /// </remarks>
//    /// 
//    public class Particle // decorative particles only
//    {
//        // http://code.google.com/p/py-lepton/wiki/Overview
//        // http://code.google.com/p/py-lepton/source/browse/trunk/examples/splode.py
//
//
//        // http://unity3d.com/support/documentation/Components/comp-ParticlesLegacy.html
//        //
//        // one of the key aspects of a particle is that
//        // each individual particle only has one texture
//        // and one group and thus can be rendered with a minimesh or InstancedGeometry.
//        // Using minimesh also allows us to re-use shaders for minimeshes.
//
//
//        // pointsprite can also be used and then rendered with a TVMesh 
//        // 
//    
//
//        // since every particle is not a full blown "entity" but rather a sub-entity
//        // of an overall Entity that is the System itself... 
//        // we can make each individual particle very lightweight.
//        // However, we can still retain (if we want) a more heavyweight particle
//        // that can utilize physics, and individual scripting
//
//        //
//    }
//
//    /// <summary>
//    /// Heavy particles taht can be individually tracked.  Unlike most scifi games where
//    /// lasers are fired with reckless abandon, weapons in our simulation are designed to have
//    /// devestating effects on an individual basis, but also to have relatively slow rates of fire.
//    /// So it's quality and not quantity in our sim as far as energy weapons are concerned.
//    /// </summary>
//    class PhysicalParticle
//    {
//        // these particles are spawned by the server and recreated on the client.
//
//
//    }
}
