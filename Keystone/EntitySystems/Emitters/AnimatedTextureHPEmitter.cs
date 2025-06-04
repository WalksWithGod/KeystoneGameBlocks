using System;
using Keystone.Types;

namespace Keystone.EntitySystems.Emitters 
{
	/// <summary>
	/// Description of AnimatedTextureHPEmitter.
	/// </summary>
	internal class AnimatedTextureHPEmitter : HeavyParticleEmitter
	{
		// private Keystone.Traversers.SphereCollider mSphereCollider;
		
		public AnimatedTextureHPEmitter(string id, string explosionTextureAtlasPath) : base (id)
		{
			RefCount = 0;
			MaxParticles = 100;
			Particles = null;
			Lifespan = 1.5f;
		}
		
		private int FindFreeIndex()
		{
			if (mSelector.ChildCount > 0)
				for (int index = 0; index < mSelector.ChildCount; index++)
					if (mSelector.Children[index].Enable == false)
						return index;
			
			return -1;
		}
				
		protected override void ActivateParticle(int particleIndex)
		{
			if (mSelector == null)
			{
				Keystone.Elements.ModelSequence sequence = (Keystone.Elements.ModelSequence)Keystone.Resource.Repository.Create (typeof(Keystone.Elements.ModelSequence).Name);
	            // TODO: this ExplosionES is derived from ModeledEntity ultimately and is added to Root of scene, however
	            //       it is flagged NotSerializbble by it's immediate base class ParticleEntitySystem. 
	            //       However, maybe it would be better to not have it derived from ModeledEntity and instead have our IEntitySystems as something else...
	            //       something that is more simulation and which can inject visuals into the scene... 
				this.AddChild (sequence);
			}

			int modelCount = mSelector.ChildCount;
			 
			if (particleIndex > modelCount - 1)
			{
	            System.Diagnostics.Debug.Assert (particleIndex == modelCount);
				if (mSelector.ChildCount != MaxParticles)
				{
		            Keystone.Elements.Model newModel = new Keystone.Elements.Model(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model)));
		            newModel.Enable = false; // always start disabled since this is pre-loading 
		            
		            // AnimationName and TargetName both only need to be unique under the entity it's added to

		            string uniqueAnimationName = "explosion_animation_" + modelCount.ToString();
		            string uniqueTargetName = "explosion_appearance_" + modelCount.ToString();
		            
		            // NOTE: we pass in model to InitExplosion() so that if we want to call multiple times
		            // with different  models, we can fill up a ModelSelector with them
		            // Creates a non-minimesh TVBillboard explosion using HLSL SPRITE SHEET ANIMATION
		            Keystone.Celestial.ProceduralHelper.InitExplosion(this, newModel, uniqueAnimationName, uniqueTargetName, this.Lifespan);
		                          
		            // TV Particle System
		            //Keystone.Celestial.ProceduralHelper.InitiTVParticleSystem(this, model, uniqueAnimationName, uniqueTargetName, this.Lifespan);
		                        
		            mSelector.AddChild (newModel);
				}
			}
		
            
			
			// TODO: bounding box for ParticleES culling is fubar
			Keystone.Elements.Model model = (Keystone.Elements.Model)mSelector.Children[particleIndex];
											
			model.Enable = true;
			// TODO: as we do with lasers, this position must be set in region relative to
			// camera region and then in cameraspace.  perhaps in Model.cs.Render() we can
			// also get it to make the transform if entity is ParticleEntitySystem
			model.Translation = Particles[particleIndex].Position;

			// TODO: how do we handle tvparticlesystem here in order to Reset() and
			//       enable?  Do we allow model.Enable() to cycle down and reset relevant Geometry?
			this.Animations.Play (particleIndex);
		} 
		
		protected override void RetireParticle(int particleIndex)
		{
			base.RetireParticle(particleIndex);
			
			// TODO: an event here for each derived Emitter type would allow us to not
			// have to change the core tests above
            // when the lifespan is reached, we disable relevant Model under 
       		// ModelSelector but we leave attached because we can re-use it
			mSelector.Children[particleIndex].Enable = false;
			this.Animations.Stop(particleIndex);
		
			//System.Diagnostics.Debug.WriteLine ("ExplosionHPEmitter.Update() - Particle removed at index '" + i + "'");	
		}
	}
}
