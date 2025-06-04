using System;


namespace KeyCommon.Simulation
{
    public enum DistributionType : byte
    {
        
        Self = 0,
        Parent,
        Container,  // the owning space station, ship, fighter, missile, vehicle, building, etc
        List,
        Region,    // triggers affect around all entities in region
        Primitive, // triggers cull using primitive centered around production location
		Collision, // triggers Ray collision and where result collided entity is potential 'consumer' of this product.
        
        Diffusion
    }

}
