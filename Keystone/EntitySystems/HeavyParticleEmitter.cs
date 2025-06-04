using System;
using KeyCommon.Simulation;
using Keystone.Entities;
using Keystone.EntitySystems.Emitters;
using Keystone.EntitySystems.Modifiers;
using Keystone.Extensions;
using Keystone.Types;
using Keystone.Utilities;

namespace Keystone.EntitySystems
{
	
	/// <summary>
	/// A "Heavy Particle" is a unit within a HeavyParticleSystem which uses an array of light weight Entity proxies
	/// to represent many entities either as a LOD system or as a group proxy for what would otherwise be a
	/// large number of fat/bloated Entity nodes.  
	/// 
	/// Unlike a normal "particle" in a lightweight particle system, these particles if desired, can interact with the world
	/// through collisions, physics and scripted behavior including production/consumption.
	/// </summary>
	internal abstract class HeavyParticleEmitter : ModeledEntity // , Keystone.Simulation.IEntitySystem
	{
		protected Particle[] Particles; // TODO: would it be better to have particles sorted by dictionary of Zones here? rather than each particle have a Region reference?
		                                //       but if using dictionary, how would simulation update() handle particles crossing zone boundaries though?  It would seem to need
		                                //       access to traversal object and entire scene to find portals and such.
		                                //       AND IF THAT IS THE CASE, then it's better to have one of these emitter's PER ZONE.  And we can begin to think of them less as emitters
		                                //       and more of digests.  But then there is the problem of having to have all of them open per zone for server side simulation.
		                                //       I do think the nature of a "Zone" as a self-contained sub-section of the game universe is to have it's own Emitters for the particles 
		                                //       it controls.
		                                //       AND IF IT IS PER ZONE, threading becomes much easier too once we handle the cross region cases.
		protected Modifier[] Modifiers; // TODO: what if i also wanted to add a Constraints[] such as a minimum Y-axis value to keep something from appearing to fall through ground?
		protected Production[] Production;

		// tracking vars
		protected double mLastRelease;
		protected int mActiveParticleCount;
		
		// emitter config vars
		public int MaxParticles;
		public int QuantityReleased; // how many will be released on each trigger
		public float ReleaseInterval; // used for some types of non-physical (non production) decorative emitters to prevent over saturation and killing of frame rate
		public bool AxialBillboard; 
		public bool CollisionEnabled; 
		
		// particle config vars
		public float Lifespan;
		public Vector3d InitialOffset; // in a single HeavyParticleEffect, some of the emitters may need to be offset from the others.
		public Vector3d InitialImpulse; // impulse can be thought of as a one time acceleration that is applied to Velocity only on release
		public VariableDouble InitialSpeed;
		public VariableVector3d InitialScale;
		public VariableVector3d InitialRotation; // for normal (non-axial) billboards, z-axis rotation is only value used
		
		public VariableColor InitialColor;
		public float InitialOpacity;
		
		internal HeavyParticleEmitter(string id) : base (id)
		{
			this.Serializable = false;
			System.Diagnostics.Debug.WriteLine ("HeavyParticleEmitter.ctor() - '" + id + "' created.  Serializable == false.");
			
			InitialColor = new VariableColor();
			InitialColor.Value = new Color(1.0f, 1.0f, 1.0f,1.0f);
			InitialColor.Variation = new Color (0.0f, 0.0f, 0.0f, 0.0f);
			
			InitialScale = new VariableVector3d();
			InitialScale.Value = new Vector3d(1,1,1);
		}
		
		// Each Particle can be in it's own Region.  Here we allow class like Model.cs during Render() to grab
		// information regarding each Particle's coordinate system
		public virtual Keystone.Portals.Region GetRegion (uint particleIndex)
		{
			return Particles[particleIndex].Region;
		}
		
		public virtual Vector3d GetPosition (uint particleIndex)
		{
			return Particles[particleIndex].Position;
		}
		
		public virtual void SetParameter (string parameterName, object value)
		{
			switch (parameterName)
			{
				case "max_particles":
					Particles = new Particle[(int)value];
					MaxParticles = (int)value;
					break;
				case "collision_enable":
					CollisionEnabled = (bool)value;
					break;
				case "axial_billboard":
					AxialBillboard = (bool)value;
					break;
				case "production":
					Production = (KeyCommon.Simulation.Production[])value;
					break;
				case "quantity_released":
					QuantityReleased = (int)value;
					break;
				case "release_interval":
					throw new NotImplementedException();
					break;

				case "lifespan":
					Lifespan = (float)value;
					break;
				case "initial_offset":
					InitialOffset = (Vector3d)value;
					break;
				case "initial_speed":
					InitialSpeed = (VariableDouble)value;
					break;
				case "initial_impulse":
					InitialImpulse = (Vector3d)value;
					break;
				case "initial_scale":
					InitialScale = (VariableVector3d)value;
					break;
				case "initial_rotation":
					InitialRotation  = (VariableVector3d)value;
					break;
				case "initial_color":
					InitialColor = (VariableColor)value;
					break;
				case "initial_opacity":
					InitialOpacity = (float)value;
					break;
				default:
					break;
			}
		}
		
		public virtual void SetModifier (string modifierName, object startValue, object endValue)
		{
			Modifier modifier = null;
			
			// TODO: verify an existing modifier does not already exist, or if it does, perhaps we replace it
			// with these new start/end values
			
			switch (modifierName)
			{
				case "rotation_modifier":
					modifier = new RotationModifier ((float)startValue);
					break;
				case "rotation_rate_modifier":
					modifier = new RotationRateModifier ((float)startValue, (float)endValue);
					break;
				case "scale_modifier":
					modifier = new ScaleModifier ((Vector3d)startValue, (Vector3d)endValue);
					break;
				case "damping_modifier":
					modifier = new DampingModifier ((float)startValue);
					break;
				
				case "color_modifier":
					modifier = new ColorModifier ((Color)startValue, (Color)endValue);
					break;
					
				case "opacity_modifier":
					modifier = new OpacityModifier((float)startValue, (float)endValue);
					break;
			}
			
			if (modifier != null)
				Modifiers = Modifiers.ArrayAppend(modifier);
		}
		
		
	
		
		// NOTE: Particles are always emitted as "fire and forget" which means
		// they are not attached to things and used like jet pack plumes and such.
		// But they can be triggered externally
		public void Trigger (Entity entity, double elapsedSeconds, Vector3d triggerPosition, Vector3d triggerVelocity)
		{
			// if (mInitialized == false) throw new Exception ();
            if (this.Enable == false)
                return;

            // Bail if the Emitter is still in its cool down period...
            double totalElapsedSeconds = entity.Scene.Simulation.GameTime.TotalElapsedSeconds;
            if (totalElapsedSeconds - this.mLastRelease < this.ReleaseInterval)
                return;
    
            // Add the Emitter offset vector to the trigger position...
            Vector3d emiterPosition = triggerPosition + this.InitialOffset;
            
            int previousCount = mActiveParticleCount;
            for (int i = previousCount; i < previousCount + this.QuantityReleased; i++)
            {
                if (i < this.MaxParticles)
                {
                    Particle particle;
                    particle.Alive 		= true;
        			particle.Owner 		= entity;         // entity which emitted the particle 
					particle.Region 	= entity.Region;  // if this is a LookupIndex as opposed to a string dictionary Key, then the API itself can easily provide us this info.
                    particle.Age        = 0f;
                    particle.Triggered  = totalElapsedSeconds;
                    particle.Color      = this.InitialColor.Sample();
                    particle.Scale      = this.InitialScale.Sample();

                    
                    particle.Velocity   = this.InitialImpulse + triggerVelocity;
                    if (AxialBillboard)
                    	particle.Rotation = Vector3d.Normalize(particle.Velocity);
                    else
	                    particle.Rotation = InitialRotation.Sample();
                    
                    double releaseSpeed = this.InitialSpeed.Sample();
			
                    Vector3d offset, forceDirection;
                    // different emitter shapes (eg circle, line, etc) produce different
					// offsets and forces on release of their respective particles
                    this.GenerateReleaseOffsetAndForce(out offset, out forceDirection);

                    // TODO: using IndexedGeometry, the camera regionRelativeCameraPosition needs to be subtracted
                    //       I think?  It's what we do during Cull() 
                    // Vector3d regionRelativeCameraPosition = 

                    // TODO: wait, isn't the cameraSpace offset supplied in Model.Render() ? Because 
                    // during ActivateParticle we are AddInstance() in regionSpace which is what we want..
                    // TODO: so we also I think want seperate Model's to host each particle of same Region....
                    // rather than a single particle[] array that then stores a .Region property
                    // TODO: but where if at all, is culling occurring because it appears that the instancedGeometry primitives
                    // from the particle are not calling Model.Render() unless the camera is oriented a certain way... 
                    
                    particle.Position   = emiterPosition + offset ; // - regionRelativeCameraPosition; <-- TODO: compute from particle.Region and camera.Region, but actually not here, in Update()
					particle.PreviousPosition = particle.Position;

                    //particle.Rotate(this.InitialRotation.Sample());
                    
                    // Calculate the acceleration of the particle using the force vector and the release speed...
					particle.Acceleration = forceDirection * releaseSpeed;
			
                    // this.OnParticleReleased(ref *particle);
                    try 
                    {
                    	Particles[i] = particle;
                    	ActivateParticle (i);
                    }
                    catch (Exception ex)
                    {
                    	System.Diagnostics.Debug.WriteLine (ex.Message);
                    	
                    }
                    mActiveParticleCount++;
                    if (this is AnimatedTextureHPEmitter)
                    	System.Diagnostics.Debug.WriteLine ("mActiveParticleCount++ = " + mActiveParticleCount.ToString());
                }
                else
                {
                    // max particles reached, can't release any more
                    break;
                }
            }

            mLastRelease = entity.Scene.Simulation.GameTime.TotalElapsedSeconds;
		}
		
		
		public override void Update(double elapsedSeconds)
		{
			// base.Update() is required for ModeledEntity scripts, behaviors and animations
			base.Update(elapsedSeconds); 
			
			if (Particles == null) return;

			// July.2.2015 - switching to InstancedGeometry.cs from Minimesh - Clear Instances
			if (this.Model.Geometry is Keystone.Elements.InstancedBillboard)
				this.Model.ClearVisibleInstances();
			
        	double totalElapsedSeconds = this.Scene.Simulation.GameTime.TotalElapsedSeconds;
        	
			for (int i = 0; i < Particles.Length; i++)
            {
        		if (Particles[i].Alive == false) continue;
        		float actualAge = (float)(totalElapsedSeconds - Particles[i].Triggered);
                Particles[i].Age = actualAge / Lifespan;
                if (actualAge >= this.Lifespan)
                { 	
					Particles[i].Alive = false;
					RetireParticle(i);
					mActiveParticleCount--;
					if (this is AnimatedTextureHPEmitter)
						System.Diagnostics.Debug.WriteLine ("mActiveParticleCount = " + mActiveParticleCount.ToString());
                } 
			}
			
			try
			{
				for (int i = 0; i < Particles.Length; i++)
            	{
        			if (Particles[i].Alive == false) continue;
        		
	        		// Position is always relative to a particle's current region.   This means we do
	        		// track particles as they cross zone boundaries or portal boundaries and then switch
	        		// their regions and update their positions. For fast moving particles, we may even have to do this
	        		// across multiple such boundaries as a particle goes in one side and out the other.
	        		Particles[i].PreviousPosition = Particles[i].Position;
	        		// euler integration of acceleration
	        		Particles[i].Velocity += Particles[i].Acceleration * elapsedSeconds;

	        		// euler integration of velocity
//	        		double length;
//	        		Vector3d direction = Vector3d.Normalize (Particles[i].Velocity, out length);
//	        		double distanceTraveled = length * elapsedSeconds;
//	        		Particles[i].Position += direction * distanceTraveled;
	        		// TODO: if velocity changes direction (some winding missile), then not updating Rotation below will affect axial billboarding
//        		    if (AxialBillboard)
//		        		Particles[i].Rotation = direction; // for axial billboards, rotation field holds direction

	        		Particles[i].Position += Particles[i].Velocity * elapsedSeconds;
	        		// TODO: need boundary tests here to move/pass particles from one Region to next
	        		// TODO: what about global positions instead that then get converted to camera relative during render?
	        
				}
				
				if (Modifiers != null)
					for (int i = 0; i < Modifiers.Length; i++)
					{
						Modifiers[i].Process (Particles, elapsedSeconds);
					}
				
				#if DEBUG
				int aliveParticles = 0;
				for (int i = 0; i <Particles.Length; i++)
					if (Particles[i].Alive)
						aliveParticles++;
								
				//System.Diagnostics.Debug.WriteLine ("Alive particles = " + aliveParticles.ToString());
				#endif 
				
				// collissions and minimesh update
				for (int i = 0; i < Particles.Length; i++)
            	{
        			if (Particles[i].Alive == false) continue;
        				        		
	        		double length;
	        		Vector3d direction = Vector3d.Normalize (Particles[i].Velocity, out length);
	        		double distanceTraveled = length * elapsedSeconds;
	        		
	        		if (CollisionEnabled)
		        		// for axial billboards, minimeshgeometry rotation array element must hold axial direction
		        		DoCollisions(i, Particles[i].PreviousPosition, direction, distanceTraveled, elapsedSeconds);
	        		
	        		// TODO: perhaps UpdateParticle() should be a single loop for update all particles that are alive instead of embedded in collission loop
	        		// TODO: UpdateParticle is now call to graphical and so specific Billboard override functions can be used... 
	        		// instead of inheriting with new derived HeavyParticleEmitter.cs type
	        		// DoCollisions() may kill particle so test .Alive again before UpdateParticle()
	        		if (Particles[i].Alive) // TODO: && IsClientSide
		        		UpdateParticle(i);
				}	
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ("HeavyParticleEmitter.Update() - ERROR: " + ex.Message);
			}	
		} 
	
		// point emitter 
		protected virtual void GenerateReleaseOffsetAndForce (out Vector3d offset, out Vector3d force)
		{
			// different emitter shapes (eg circle, line, etc) produce different offsets and forces
			// but this is the default behavior
            offset = Vector3d.Zero();
			force = RandomHelper.RandomVector();
		}
				
		protected virtual void ActivateParticle (int particleIndex)
		{
		}
		
		protected virtual void UpdateParticle (int particleIndex)
		{
		}
		
		protected virtual void RetireParticle(int particleIndex)
		{
		}
		
		protected virtual void DoCollisions(int particleIndex,Vector3d previousPosition, Vector3d direction, double distanceTraveled, double elapsedSeconds)
		{
		}
	}
}
