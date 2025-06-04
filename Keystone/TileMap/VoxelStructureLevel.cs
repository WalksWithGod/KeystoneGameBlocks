using System;
using Keystone.Elements;
using Keystone.Extensions;

namespace Keystone.TileMap
{
	/// <summary>
	/// VoxelStructureLevel is 1 slice of a 3-D tilemap.  Each structureLevel contains an array of 2-D maplayers.
	/// </summary>
	internal class VoxelStructureLevel 
	{
		
		public MapLayer[] mMapLayers;
		public readonly int LocationX;
		public readonly int LocationY;    // array index
		public readonly int LocationZ;
		public readonly int FloorLevel;   // FloorLevel is not the same as the array index this StructureLevel is placed.
		                         // This is because the array order can change as new StructureLevels are added or deleted
		                         // from the Structure and by tracking the FloorIndex seperately, a floor designed to be at
		                         // "ground level" will always be at ground level no matter how many new floors we insert
		                         // above or below it or how many we delete.
		                         		
	
		public VoxelStructureLevel(int locationX,  int locationY, int locationZ, int floorLevel)
		{
			LocationX = locationX;
			LocationY = locationY;
			LocationZ =  locationZ;
			
			FloorLevel = floorLevel;
		}
		
		public void AddMapLayer (MapLayer layer)
		{
			mMapLayers = mMapLayers.ArrayAppend (layer);
		}
		
		
		// TODO: i added incrementRef above and now i think i also need to decrementref and then to ensure
		// that we do in fact cause the minimeshes disposedmanagedresources to fire
		internal void Dispose()
		{

    		// TODO: Doesn't MapGrid.Destroy() screw up ClientPager.LoadZoneMapLayers() where we want to load mapdata
    		// that is +1 further out from our page_in value so that auto-tiling works at boundaries?
    		// In other  words, i dont think our page-out distance can be less than or equal to layer-page-in distance
    		Core._Core.MapGrid.Destroy (LocationX, LocationY, LocationZ);
		}
	}
}
