using System;
using Keystone.Entities;

namespace Keystone.Events
{
    class GunFireEventHandler : RepeatableEvent
    {
        private Entity _owner;

        GunFireEventHandler(Entity owner, string animation_file, string sound_file)
        {
            if (owner == null) throw new ArgumentNullException();
            _owner = owner;
            //fire_animation = Animation.Create(animation_file);
            //fire_sound = Sound.Create(sound_file);
        }

        // This one would likely be defined in RepeatableAction
        public void HandleEvent(Event ievent)
        {
            // IsInCooldown is a property that this event knows to exist any owner because all owners must be of type Gun
            //if (_owner.IsInCooldown) return;

            //_owner.IsInCooldown = true;
            //cooldown_timer = action_cooldown;
            //_owner.AnimationHandler.play(fire_animation);
            //_owner.SoundHandler.play(fire_sound);

            //// where do we write the code to compute trajectory and send netpacket?
            //// maybe we can raise another event in the owner?
            //// where BulletFired is derived from abstract class NetworkAwareEvent
            //// we could fill in things like vector and target here so that when we raise the completion
            //// event the owner can read those properties?
            //_owner.OnEventConsumed(this);
        }
    }
}
