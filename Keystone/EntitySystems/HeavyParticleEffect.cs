using System;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Extensions;
using Keystone.Types;

namespace Keystone.EntitySystems
{
	internal class HeavyParticleEffect : ModeledEntity
	{
		HeavyParticleEmitter[] mEmitters;
		
		public HeavyParticleEffect(string id) : base (id)
		{
			this.Serializable = false;
			System.Diagnostics.Debug.WriteLine ("HeavyParticleEffect.ctor() - '" + id + "' created.  Serializable == false.");
		}
	
		public void AddChild(HeavyParticleEmitter emitter)
		{
			base.AddChild (emitter);
			
			mEmitters = mEmitters.ArrayAppend (emitter);
		}
		
		public override void RemoveChild(Node child)
		{
			if (child is HeavyParticleEmitter == false) throw new ArgumentOutOfRangeException();
			if (mEmitters.ArrayContains ((HeavyParticleEmitter)child) == false) throw new ArgumentOutOfRangeException();
			
			base.RemoveChild (child);
            mEmitters = mEmitters.ArrayRemove ((HeavyParticleEmitter)child);
		}
		
		public void Trigger(Entity entity, double elapsedSeconds, Vector3d triggerPosition)
		{
			this.Trigger (entity, elapsedSeconds, triggerPosition, Vector3d.Zero());
		} 
		
		public void Trigger(Entity entity, double elapsedSeconds, Vector3d triggerPosition, Vector3d triggerVelocity)
		{
			if (mEmitters == null) return;
			
			// TODO: we are creating and adding a new emitter to the same single effect each time we initialize an NPC
			// TODO: but for legitimate unique emitters within a single effect, the for loop is valid and needed
			//for (int i = 0; i < mEmitters.Length; i++)
			//{
				mEmitters[0].Trigger (entity, elapsedSeconds, triggerPosition, triggerVelocity);
			//}
		} 
		
		public override void Update(double elapsedSeconds)
		{
			if (mEmitters == null) return;
			
			//for (int i = 0; i < mEmitters.Length; i++)
			//{
				// TODO: see TODO notes above for Trigger... same problem
				mEmitters[0].Update (elapsedSeconds);
			//}
		} 
	}
}
