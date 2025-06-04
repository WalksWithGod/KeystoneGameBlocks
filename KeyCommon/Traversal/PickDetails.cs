using System;
using Keystone.Types;

namespace KeyCommon.Traversal
{
	[Flags]
    public enum PickAccuracy: uint // can be OR'd together
    {
        None = 0,
        BoundingSphere = 1 << 1,
        BoundingBox = 1 << 2,
        Geometry = 1 << 3, // returns actual EditableMEsh, TVActor or TVMesh or TVMinimesh
        GeometryGroup = 1 << 4, // in case of TVMinimesh returns the actual element index
        Tile = 1 << 5,
        Face = 1 << 6, // face is NOT same as Tile because with "Tile" we're actually just picking 2D quad from logical grid
        TileWithFloor = 1 << 7, // Tile that has a floor assigned so that items placed on it wont fall through, requires | with Tile to be useful
        Edge = 1 << 8 | Face,   // edge required face, Tile and deck pick accuracy
        Vertex = 1 << 9 | Edge, // vertex requires edge, face, Tile and deck pick accuracy
        Bones = 1 << 10,
        Chunk = 1 << 11,
        EditableGeometry = 1 << 12,
        HitBoxes = 1 << 13,
        Any = uint.MaxValue 
    }

    //// Accuracy relates to precision and does not relate to determining what of a picked item should be included
    //public enum PickAccuracy
    //{
    //    Face = CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING,
    //    BoundingBox = CONST_TV_TESTTYPE.TV_TESTTYPE_BOUNDINGBOX,
    //    BoundingSphere = CONST_TV_TESTTYPE.TV_TESTTYPE_BOUNDINGSPHERE,
    //    HitBoxes = CONST_TV_TESTTYPE.TV_TESTTYPE_HITBOXES
    //    //CONST_TV_TESTTYPE.TV_TESTTYPE_DEFAULT 
    //}

     // how can we assign weights to search criteria?
    // if for instance i want an interior structure pick to return the uncollapsed floor over the collapsed floor
    // regardless of which is closer, then the weight for uncollapsed must always exceed any weights for closeness
    [Flags]
    public enum PickCriteria : uint
    {
        None = 0,    // This should never occur. By default PickParameters should initialize with Closest SearchType
        Closest = 1 << 0, // default behavior, "Selection" but for something like Octree thats no good since it selects the root
        
        TopLevelModel = 1 << 1, // returns the top most Model in a ComplexModel or BonedModel hierarchy
        LowLevelModel = 1 << 2, // returns the lowest model in a complex or boned model hiearchy
        Interior = 1 << 3, // pick only entities that are children of the interior
        Exterior = 1 << 4, // pick only entities that are NOT children of an interior
        // what if celledRegion is only non excluded type but we only want to pick cells that have floors and ignore
        // cells that have no floors?  keep in mind we need to do both of these so it must be optional.
        // should that be part of the primitive type to select?
        FlooredCell = 1 << 5,
        Transparent = 1 << 6,
        DeepestNested = 1 << 7,
        Last = 1 << 28, // dont really see Last being useful.
        // for chain imagine a gause riffle firing thru the closest and some dude behind him
        // you still want all of the collisions here.. 
        Chain = 1 << 29,
        All = uint.MaxValue // is this same as chain?  maybe not exactly since 
    }


    public enum PickPassType 
    {
    	Mouse = 0,
    	Collide,
    	Boundaries // portals and zone boundaries
    }
    
    public struct PickParameters
    {
        public int MouseX;
        public int MouseY;
        public string ActorDuplicateInstanceID; // same as the Entity.ID
        /// <summary>
        /// "FloorLevel" is fixed floor level and not array index.
        /// </summary>
        public int FloorLevel;
        //public bool TraversePortals; // TODO: simply set ExcludedTypes to include EntityFlags.Portals
        public PickPassType PickPassType;
        public PickCriteria SearchType;
        public KeyCommon.Flags.EntityAttributes ExcludedTypes; // 32bit mask can be or'd together.  Excluded Types and their child nodes are skipped.
        public KeyCommon.Flags.EntityAttributes IgnoredTypes;  // Ignored Types allows for children of Ignored Entitys to still be traversed
        public PickAccuracy Accuracy;
        public bool SkipBackFaces; // for accurate results this is checked to determine if backfaces should be skipped 
        public bool DepthSortResults;
        
        public double T0;
        public double T1; 
        
        public float RayThickness;  // -- a radius value -  maybe can also specify an array of Rays representing a bigger projectile 
        // or a ray with a thickness radius where the picker
        // will create extra rays to accomodate and thus return all the "first" results of each
        // or whatever searchtype is selected?
        // what about if we were using a 2d SelectionBox ?
        #if DEBUG
        public bool DebugOutput;
        #endif
    }
    
	public class PickResultsBase : IComparable<PickResultsBase>
    {
        public PickAccuracy CollidedObjectType;
        public bool HasCollided = false;
        public double Score; // search score we can use to evaluate multiple pickresults to find the best fit one to return
                          // eg. closest distance is scored 1.  If score == 1 then we return the closest
                          // other criteria 
        //public TV_COLLISIONRESULT Result;        // TODO: remove this so that this Class can be moved to KeyCommon which has no MTV3D65 dependancy

        // Matrix == the RegionMatrix used on this entity during the pick operation
        // we can use this to convert the ImpactPoint to Region space
        public Matrix Matrix;  
        
        // public int BoneID;
        public int FaceID = -1;   // Contains TileID, Mesh3d, Actor3d, or Terrain polygon index.
        public int EdgeID = -1;   // Only relevant for Tiles and is the closest edge to mouse impact point on Tile face
        public int VertexID = -1; // index of the Mesh vertex we've picked.  This index corresponds to this.VertexCoord
        public int TileVertexIndex = -1; // // Only relevant for Tiles. Index of closest corner vertex of face 
                
        public Vector3d VertexCoord; // closest vertex's world position. This value corresponds to this.VertexID (TODO: should be in local space?)
        public Vector3d ImpactPointRelativeToRayOriginRegion; // we can unproject VertexCoord and ImpactPoint and compare 2d distance to some epsilon and determine if we're close enough to snap and use the existing
        public Vector3d ImpactPointLocalSpace; 
        public Vector3d ImpactNormal; // TODO: but is tv's TV_COLLISIONRESULT impactpoint always in world space?
        
        /// <summary>
        /// Cell face coordinates not inner Tile coordinates.
        /// </summary>
        public Vector3d[] FacePoints;
        public uint[] FacePointIDs;
        public Vector3i CellLocation; // Only relevant for Interior CelledRegion Cells.
        public Vector3i TileLocation; // Only relevant for TileMap.Tiles.  Unflattened Tile coordinate indices. Corresponds to this.TileID
        public Vector3d EdgeOrigin; // Only relevant for Tiles.  Is the nearest edge origin vertex. 
        public Vector3d EdgeDest; //  Only relevant for Tiles.  Is the nearest edge destination vertex


        public string EntityID { get; protected set; }
        public string EntityTypeName { get; protected set; }

        
        // debug
        public Vector3d PickOrigin;
        public Vector3d PickEnd;  
        /// <summary>
        /// Distance from ModelSpace Ray Origin to ModelSpace ImpactPoint.
        /// </summary>
        public double DistanceSquared;

        public PickResultsBase()
        {
            DistanceSquared = double.MaxValue;
        }


        // TODO: i need items here for NearestEdge, NearestVertex,Face, etc from EditableMesh
        // 

        #region IComparable<PickResults> Members

        public int CompareTo(PickResultsBase other)
        {
        	return DistanceSquared.CompareTo (other.DistanceSquared); // Result.fDistance.CompareTo(other.Result.fDistance);
        }

        #endregion
    }
}
