
using System;

namespace Keystone.EntitySystems.Emitters
{
	/// <summary>
	/// 2-D Billboard shockwave
	/// </summary>
	internal class ShockwaveHPEmitter : HeavyParticleEmitter
	{
		public ShockwaveHPEmitter(int emitterID, string shockwaveTexturePath) : base (emitterID.ToString())
		{
		}
		
		
		private void Load()
		{          
            Keystone.Elements.Model model = new Keystone.Elements.Model(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model)));
            model.Enable = false; // always start disabled since this is pre-loading 
            
            // AnimationName and TargetName both only need to be unique under the entity it's added to
            int modelCount = 0;
            if (mSelector.Children != null)
            	modelCount = mSelector.Children.Length ;
            
            string uniqueAnimationName = "explosion_animation_" + modelCount.ToString();
            string uniqueTargetName = "explosion_appearance_" + modelCount.ToString();
            
            // NOTE: we pass in model to InitExplosion() so that if we want to call multiple times
            // with different  models, we can fill up a ModelSelector with them
             // Creates a non-minimesh TVBillboard explosion using HLSL SPRITE SHEET ANIMATION
             Keystone.Celestial.ProceduralHelper.InitExplosion(this, model, uniqueAnimationName, uniqueTargetName, this.Lifespan);
            //Keystone.Celestial.ProceduralHelper.InitiTVParticleSystem(this, model, uniqueAnimationName, uniqueTargetName, this.Lifespan);
                        
            mSelector.AddChild (model);
		}
	}
}
