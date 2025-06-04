using System;

namespace KeyCommon.Traversal
{

	[Flags]
    public enum CullAccuracy: uint // can be OR'd together
    {
        None = 0,
        BoundingSphere = 1 << 1,
        BoundingBox = 1 << 2,
        
        Tile = 1 << 3,
        Face = 1 << 4, // face is NOT same as Tile because with "Tile" we're actually just picking 2D quad from logical grid
        Chunk = 1 << 5,
        Any = uint.MaxValue 
    }

    public enum CullPassType 
    {
    	Normal = 0,
    	Occlussion,
    	DetectionCone,   // entity sensor detection using cone primitive (sensor can mean sight, motion sensor, radar, etc)
    	DetectionSphere  // entity sensor detection using sphere primitive (smell, sound,)
    }
	    
    public enum FrustumType 
    {
    	Planes,
    	Cone,
    	Sphere
    }
    
	public struct CullParameters
	{
	    public FrustumType FrustumType; 
        public CullPassType CullPassType;
        public bool HardwareOcclussionEnabled; 
        public CullAccuracy Accuracy;
        // obsolete - Feb.22.2017 - using Included and Allowed instead
        //public KeyCommon.Flags.EntityAttributes IgnoredAttributes; // 32bit mask can be or'd together.  Ignored nodes skipped, BUT their children ARE traversed.        
        // NOTE: now we can more flexibily exclude entire interiors for certain cull passes
        //public KeyCommon.Flags.EntityAttributes ExcludedAttributes;  // 32bit mask can be or'd together.  Excluded nodes AND THEIR CHILDREN are skipped.
        public KeyCommon.Flags.EntityAttributes IncludedAttributes;
        public KeyCommon.Flags.EntityAttributes AllowedAttributes;
        public int FloorLevel;
	}
	
}
