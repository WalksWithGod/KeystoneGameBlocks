using System;
using System.Collections.Generic;
using Keystone.Extensions;
using Keystone.Types;

namespace Keystone.TileMap
{
	// indexes into layer array within each layer
	internal enum LayerType : int
 	{
		obstacles = 0,
		terrain = 1,           // terrain needs adjacency for autotile
		terrain_style,
		structure,         // walls need adjacency for autotile
		structure_style,
	 	
//		
//			components        // components & structure cannot occupy same footprint areas
//			component_style  // how does this work?
//			biomes / flora & fauna // https://speciesdevblog.wordpress.com/category/xna/ also see notes in keystone notes.txt
//				
//			links (power, elecric, plumbing, network, weapon links, control linkages, vents)
//			damage
	 }	
	
	//	-	
	// TOOD: do we diffrentiate between a Wall and a Terrain structure?
		
	// when our structure's Read their "layout" MapLayer data, they should be grabbing them from here
	// but this MapGrid should be instantiating them and reading the pixels into memory.
	
	// - perhaps we can use a cursor for where the camera is
	// - MapGrid should be able to load/unload layers.
	// - MapLayer data can be manipulated without ever loading any visuals.
	
	
	// indexes into segment selector nodes under each level child node
	enum TileTypes : int // these values correspond to Segment array indices.  When we write these to BITMAP we increment by 1. Similarly when reading from BITMAP we decrement because the BITMAP data is 1 based.
	{
		TT_OOB = -1,  // out of bounds of the map itself
		TT_EMPTY = 0,
		TT_FLOOR = 1,
		TT_WALL = 2,
		TT_TERRAIN = 3
		
		// TODO: do i want to store an elevation in here?  If first TT were 65k possible types
	    //       then we'd still have another 65k for elevation and of course at most we'd need ~5 types: flat, quarter, half, 3/4, full
		//       for instance, flat, half, full where a full elevated ROCK
		//       tile would be an unpassable obstacle, but would be a flat
		//       traversable area on the layer above
			//   NOTE: I dont think we can add them.. i think elevation shoudl be written to a seperate member of a TileType struct
			//   that is a union of Type and Elevation 
			//   - then when we do AutoTiling, we can check elevation... so this way, our upper layer does NOT actually need any
			//   data written if it is in fact empty.  
	}
	
	// TileFlags [walkable, sloped, etc] our footprints will provide more detailed info about obstacles and walkability such as speed modifiers?
	// or perhaps TileElevation belongs along with our footprint obstacles
	enum TileHeight
	{
		TE_FLAT = 0,
		TE_QUARTER = 1,
		TE_HALF = 2,
		TE_THREE_QUARTERS = 3,
		TE_FULL = 4,
		TE_RAMP = 5 // TODO: what about ramps that connect one elevation to another?
	}

	// mapping of the int Types of TileTypes enum to string Names
	//static string[] TileTypeNames = new string[] {"outofbounds", "empty", "floor", "wall"};

	// http://en.wikipedia.org/wiki/Points_of_the_compass
	enum TileDirections : int
	{		
		// lower level
		D_SW =    0,
		D_S =     1,
		D_SE =    2,
		D_W =     3,
		D =       4,  // Down
		D_E =     5,
		D_NW =    6,
		D_N =     7,
		D_NE =    8,
		
		// middle level
		SW =      9,
		S =      10,  // South
		SE =     11,
		W =      12,  // West 
		Center = 13,
		E =      14,  // East
		NW =     15,
		N =      16,  // North
		NE =     17,
		
		// upper level
		U_SW =   18,
		U_S =    19,
		U_SE =   20,
		U_W =    21,
		U =      22,  // Up
		U_E =    23,
		U_NW =   24,
		U_N =    25,
		U_NE =   26

//		Center = 13,
//		S = 12,  // South
//		N = 14,  // North
//		W = 4,  // West 
//		E = 22,  // East
//		
//		NE = 23,
//		SE = 21,
//		SW = 3,
//		NW = 5,
//		
//		// upper level
//		U = 16,  // Up
//		U_S = 15,
//		U_N = 17,
//		U_W = 7,
//		U_E = 25,
//		
//		U_NE = 26,
//		U_SE = 24,
//		U_SW = 6,
//		U_NW = 8,
//		
//		// lower level
//		D = 10,  // Down
//		D_S = 9,
//		D_N = 11,
//		D_W = 1,
//		D_E = 19,
//		
//		D_NE = 20,
//		D_SE = 18,
//		D_SW = 0,
//		D_NW = 2
		
//			Center = 0,
//			S = 1 << 0,  // South
//			N = 1 << 1,  // North
//			W = 1 << 4,  // West 
//			E = 1 << 5,  // East
//			
//			NE = N | E,
//			SE = S | E,
//			SW = S | W,
//			NW = N | W,
//			
//			// upper level
//			U = 1 << 2,  // Up
//			U_S = U | S,
//			U_N = U | N,
//			U_W = U | W,
//			U_E = U | E,
//			
//			U_NE = U | NE,
//			U_SE = U | SE,
//			U_SW = U | SW,
//			U_NW = U | NW,
//			
//			// lower level
//			D = 1 << 3,  // Down
//			D_S = D | S,
//			D_N = D | N,
//			D_W = D | W,
//			D_E = D | E,
//			
//			D_NE = D | NE,
//			D_SE = D | SE,
//			D_SW = D | SW,
//			D_NW = D | NW
		
	}

	
	internal struct TileInfo
	{
		public int ID; // flattened x,y,z
		// public int LevelIndex; // not floor, but 0 - N level index.  maybe it should be floorLevel though instead since it's implementation non-specific
		//public int ElementID; // minimesh element
		public int SegmentID;       // aka "StructureID" or "TerrainTypeID" everything from Terrain Types to Wall types (eg Steel Wall vs Wooden Wall)
		public int ModelID;         // specific model based on adjacent model's
		public Vector3d Position;
		public int RotationDegrees; // degrees (we need to then convert to values of  0, 1, 2 or 3 for shader)
	}
}
