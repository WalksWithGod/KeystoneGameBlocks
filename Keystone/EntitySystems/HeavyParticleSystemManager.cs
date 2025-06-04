using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.EntitySystems;
using Keystone.EntitySystems.Emitters;
using Keystone.FX;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.EntitySystems
{
    // Keystone\Celestial\ProceduralHelper\CreateSmoothCircle(string id, int segments, float radius)
    // - uses mesh3d built with linestrip primitive and
    // @"pool\shaders\VolumeLines.fx";  
    // google axial billboard shader
    
    // to create thick connected list of lines.  trianglestrip seems to not work on tvmesh.
    // I think using a single mesh where we move verts is not the best idea, but the key is avoiding
    // to have to compute each vertex position each frame on CPU and to instead only modify verts during
    // create/destroy of a laser particle.
    
    // // Keystone.Simulation\StarDigest.cs
    //

    public class HeavyParticleSystemManager : FXBase 
    {
        // a list of the currently active ParticleEntitySystem fx objects.  The elements they manage 
		// (eg. lasers, explosions, etc) are not part of the scene graph.
        // They are rendered as Immediate Mode 3d objects relative to the current camera Region.
        
        
        // Or, the \\E:\dev\c#\KeystoneGameBlocks\Data\pool\shaders\Particles\XNA ParticleEffect.fx
        // seems to be better than computing position every frame for each particle. 
        // You modify the vertex for each particle when it's created so that the vertex position is
        // POSITION0
        // NORMAL0 = velocity
        // COLOR0 =  random
        // TEXCOORD0 == time the particle has lived.
        
        
        // but the question is, that is for vertex particles (point sprites), what about quad particles?  Our VolumeLines shader
        // does do the axial rotation for us though, we'd have to determine how to handle particle aspect.
        
        // I suspect that other FX like exhaut trails, exhaut plumes, explosions and all other "elemental" type fx
        // that need to be animated, faded over time, etc., should not be added/inserted into the scene constantly
        // but be managed by this FX class 
        // we should also use a cache where we can register certain types of fx so they can be easily shared.
        // the underlying meshes and textures too

        Dictionary<int, HeavyParticleEffect> mEffects;
        
    	// TODO: can we render each particle in camera space without losing precision of first converting it to global space?
    	//       - we want to eliminate the big regional offsets before we start with the entity's own regional position or model space offsets
    		
    	// TODO: when attempting to do collisions, wont we have to convert each particle to it's current region space?
    	//       - our ray picker does do cross region model space picking... but it starts with a ray's own region position
    	//         so I think each laser should be in region space coords
        	
		// TODO: is HeavyParticleSystemManager host to many many different kinds of lasers? colors, sizes, etc?
		//       or just one basic type?  Will it host arrays of mMinimesh[], mTexture[] mBillboard[]?
        public HeavyParticleSystemManager()
        {  
        	System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.ctor() - Instantiated.");
        	_semantic = FX_SEMANTICS.FX_CAMERA_BUBBLE;
        }
         
        
    	// AppMain.Simulation.RegisterProducer(this);
    	// - npc.css  -> InitializeEntity() -> FXAPI.Sound_Register(soundResource);
    	//									  FXAPI.ParticleSystem_Register (particleResource, PARTICLE_TYPE);
    	//                                    FXAPI.ParticleSystem_SetParameter(particleResource, "lifetime", 0);
    	//                                    FXAPI.ParticleSystem_SetParameter(particleResource, "length", 0);
    	//                                    FXAPI.ParticleSystem_SetParameter(particleResource, "width", 0);
    	 
		// - npc.css  -> OnUpdate ()  ->  FXAPI.ParticleSystem_EmitParticle("laser01.png", entityID, position, direction, scale, speed);
		//                               {
		//                                    // find this particle system inside collection
		//                                    bool result = mParticleSystems.TryGetValue ("laser01.png", out system);
		//	                                  
		//                               }
		//							 ->  FXAPI.Emit_Sound ("laser.wav");
		// 

		//                           KeyEdit.Simulation.Simulation.Update->UpdateProductio() 
		//                             grabs all producers from each entity, but we want is to be able to
		//                             have this EntitySystem also be queried I suppose?  Because here we have
		//                             products that were produced (lasers) but need to be simulated for a small time
		//                             to determine if these products will be consumed by any other entities.			
		
		
		// factory style method - note: caller VisualFXAPI.cs always adds to "root" for parent
		// TODO: root parent means we need to calc region offset as well as camera space offset
		//       Problem here is in naming conventions, an "effect" here means particle fx but what we want it to mean is
		//       more like, a "mini-entity" and for SystemManager to be like a digest/mini-entity system
		//       and one where entitites can display a visual when close enuf to camera.. when in region that is close enuf
		//       to camera - effectors/receptors
		public int CreateEffect (string name, string parentID)
		{
			System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.CreateEffect() - '" + name + "'");

			// TODO: the assetplacementTool is giving us an entity not yet connected to scene by the time this CreateEffect is called in the entity's script
			//       so 
			// a) should we be creating an effect during InitializeEntity()
			// b) should entities have InitializeEntity() called only after they are parented to another Entity or Region.
			Keystone.Entities.Entity parent = (Keystone.Entities.Entity)Repository.Get (parentID);
			parent = parent.Region;
			if (parent == null) throw new Exception ("HeavyParticleSystemManager.CreateEffect() - parent is null.");
			
			
			if (mEffects == null) mEffects = new Dictionary<int, HeavyParticleEffect>();
			
			HeavyParticleEffect existingEffect = null;
			int index = 0;
			foreach (HeavyParticleEffect effect in mEffects.Values)
			{
				if (effect.Name == name)
				{
					existingEffect = effect;
					break;
				}
				index++;
			}
			
        	if (existingEffect != null)
        	{
        		existingEffect.RefCount++;
        		return index;
        	}
        	
        	string id = Repository.GetNewName (typeof(HeavyParticleEffect));
        	HeavyParticleEffect newEffect = new HeavyParticleEffect(id);
        	newEffect.Name = name;
        	
        	index = mEffects.Count;
            mEffects.Add (index, newEffect);
            
            // for this system to render, it must be added to scene
            parent.AddChild (newEffect);
            return index;
		}
		
		// factory method
		// TODO: perhaps per Region as well as per effectID
		public int CreateEmitter (int effectID, int emitterType, int maxParticles, float lifeSpan, float interval, int quantityReleased, string texturePath)
		{	  
			System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.CreateEmitter() - '" + texturePath + "'");	

			if (mEffects == null || mEffects.ContainsKey (effectID) == false) throw new ArgumentOutOfRangeException();

			HeavyParticleEffect parent = mEffects[effectID];
						
        	// if the HeavyParticleEmitter does not already exist, it will be added 
        	// it's refCount will start at 1.  If it already exists, the refCount will be incremented.

        	// TODO: for emitters of same "name" we're not sharing
        	
        	string id = Repository.GetNewName (typeof(HeavyParticleEmitter) );
        	
        	// TODO: temporary switch based on emitterType specified
        	HeavyParticleEmitter newEmitter = null;
        	switch (emitterType)
        	{
        		case 0: 
        			// hlsl animated texture explosions
        			newEmitter = new AnimatedTextureHPEmitter(id, texturePath); 
        			break;
        		case 1:
        			// minimesh NON-AXIAL billboards ( free rotation or y-axis)
        			newEmitter = new BillboardHPEmitter(id, texturePath); 
        			newEmitter.AxialBillboard = false;
        			break;
        		case 2:
        			// minmesh AXIAL billboards
        			newEmitter = new BillboardHPEmitter(id, texturePath); 
        			newEmitter.AxialBillboard = true;
        			break;
        		case 3:
        			// instancedGeometry w/ AXIAL rotation
        			// HACK - recycle same emitter 
        			// TODO: we should recycle emitters whenever the effect refCount > 0
        			//       because we know the same effect uses same emitters... else
        			//       the effect should have a different name.
        			if (parent.Children != null)
        				return 0;
        			
        			newEmitter = new InstancedBillboardHPEmitter (id, texturePath);
        			newEmitter.AxialBillboard = true;
        			break;
        		case 4:
        			newEmitter = new InstancedBillboardHPEmitter (id, texturePath);
        			newEmitter.AxialBillboard = false;
        			break;
        		default:
        			return -1;
        	}
        	
			newEmitter.Lifespan = lifeSpan;
			newEmitter.QuantityReleased = quantityReleased;
			newEmitter.ReleaseInterval = interval ;
			newEmitter.RefCount = 1;
			newEmitter.SetParameter ("max_particles", maxParticles);

			// load any Entity script resource
			newEmitter.LoadTVResource();
			
            // for this system to render, it must be added to scene
            parent.AddChild (newEmitter);
            
            int emitterID = parent.ChildCount - 1;
            return emitterID;
		}
		
		public void SetParticleEmitterParameter (int effectID, int emitterID, string parameterName, object value)
		{
			//System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.SetParticleSystemParameter() - '" + id + "'");	
			if (mEffects == null || mEffects.ContainsKey (effectID) == false) throw new ArgumentOutOfRangeException();

			HeavyParticleEffect effect = mEffects[effectID];
			HeavyParticleEmitter emitter = (HeavyParticleEmitter)effect.Children[emitterID];
			
			emitter.SetParameter (parameterName, value);
		}
		
		public void SetParticleEmitterModifierr (int effectID, int emitterID, string modifierName, object startValue, object endValue)
		{
			//System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.SetParticleSystemParameter() - '" + id + "'");	
			if (mEffects == null || mEffects.ContainsKey (effectID) == false) throw new ArgumentOutOfRangeException();

			HeavyParticleEffect effect = mEffects[effectID];
			HeavyParticleEmitter emitter = (HeavyParticleEmitter)effect.Children[emitterID];
			
			emitter.SetModifier  (modifierName, startValue, endValue);
		}
		
		public void TriggerEffect (int effectID, Entities.Entity entity, double gameTime, Vector3d triggerPosition, Vector3d triggerVelocity)
		{
			//System.Diagnostics.Debug.WriteLine ("HeavyParticleSystemManager.EmitParticle() - '" + id + "'");	
			HeavyParticleEffect existingEffect;
			bool result = mEffects.TryGetValue (effectID, out existingEffect);
			if (result == false) throw new ArgumentOutOfRangeException ("HeavyParticleSystemManager.TriggerEffect() - ERROR: Effect '" + effectID.ToString() + "' not found.");
        		
			// TODO: how/when do we assign the Region and Owner?  If we assign
			//       an owner on Registration, then Region is likely particle.Owner.Region
			//       but, the problem is... our damn Entity system doesn't have nice indices 
			//       directly into the lookup that we can use.  It'd be great if we did... even
			//       if those indices were temporary and only client/machine specific... 
			//       like minimesh element indices.  The problem is that, managing these indices 
			//       ... they exist for lifetime of Entity... but we can't have one entity reference
			//       the index of another or it will be lost.  It's great for entity scripts though... since 
			//       they can only run because their entities are alive.  To an extent, Region's (or ancestral parents) should be safe too
			//       if their indices are referenced by their children since the region can't be destroyed while it
			//       still has children.  But being able to just store these indices for entitites and for 
			//       flyweight entities... would save us much trouble as our scenes grow in complexity and as we
			//       make more API calls from our entity scripts.     			     
			existingEffect.Trigger (entity, gameTime, triggerPosition, triggerVelocity);
		}
	
		public override void Update(double elapsedSeconds, Keystone.Cameras.RenderingContext context)
		{
			if (mEffects == null) return;
			
			// we do have all Zone's loaded, but not their contents.  Should i have seperate HPSM per zone/region? 
	
			
			// TODO: i believe here we are updating every single entity multiple times because
			// each HeavyParticleEmitter within each HeavyParticleEffect is derived from 
			// ModeledEntities and added to the scene!
			// If this were a "real" FX system where none of the entities were necessarily
			// connected to the scene itself, then we would need to update here
			// TODO: for now we will comment out the Update() since each HeavyParticleEmitter will
			// have it's update() called by Simulation.cs
			// TODO: if we wanted to do culling here, we'd have to prevent the normal Simulation.Update() from
			//       updating these particles... 
			
			// NOTE: the following is incorrect. Trigger is for external emitting of particle effects
			// by something like Entity script.  
			foreach (HeavyParticleEffect effect in mEffects.Values)
			{
				//effect.Trigger(elapsedSeconds);
				//effect.Update (elapsedSeconds);
			}
        }
        
		public override void Render(Keystone.Cameras.RenderingContext context)
		{
			if (mEffects == null) return;
			
//			foreach (HeavyParticleEffect effect in mEffects.Values)
//			{
//				effect.Render(elapsedSeconds);
//				//	system.Model.Render (context, this, 
//				// context must render our geometry so they get injected in proper root regionPVS
//				// mMinimesh.Render 
//			}	
		} 
    }
}
