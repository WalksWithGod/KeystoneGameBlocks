using System;

namespace Keystone.Collision
{
	/// <summary>
	/// Description of IBroadphaseItem.
	/// </summary>
	internal interface IBroadphaseItem
	{
        Keystone.Types.BoundingSphere BoundingBox { get; }
        bool IsStaticOrInactive{ get; }
	}
	
	
    internal class BroadphasePair
    {
        /// <summary>
        /// The first body.
        /// </summary>
        public IBroadphaseItem Entity1;
        /// <summary>
        /// The second body.
        /// </summary>
        public IBroadphaseItem Entity2;

        /// <summary>
        /// A resource pool of Pairs.
        /// </summary>
        //public static ResourcePool<BroadphasePair> Pool = new ResourcePool<BroadphasePair>();
    }
    
    
}


