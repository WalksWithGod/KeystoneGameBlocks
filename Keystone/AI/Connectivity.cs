using System;
using Keystone.Extensions;
using Keystone.Types;

namespace Keystone.AI
{
	// http://www.ai-blog.net/archives/000152.html
	// http://www.gamasutra.com/view/feature/3000/gdc_2002_polygon_soup_for_the_.php
	// http://www.gamasutra.com/blogs/SvenBergstrom/20140109/208374/Pathing_Excursions__more_natural_paths.php?print=1
	/// <summary>
	/// Creates Areas composed of Tiles along with the list of adjacent areas.
	/// Open tiles such as "air" (eg. no terrain or structure flooring) are considered
	/// connecting. Only blocking obstacles are considered to prevent connectivity.
	/// Or perhaps we'll use a lambda evaluation function that can be taylored to the type of
	/// connectivity we're interested in discovering.
	/// </summary>
	internal class Connectivity : Keystone.TileMap.IMapLayerGridObserver
	{
		internal enum GraphState 
		{
			Dirty,     // must be rebuilt
			Building,  // is buing rebuilt
			Ready      // up to date
		}
		
		private enum AXIS
		{
			X,
			Y,
			Z
		}
							
		internal struct AreaBuilderNodeFast
	    {
			// NOTE: this struct is only used during Runtime Area building in the mCalcGrid.  
	        public int AreaID;	        
	        public TraversalState  Status; // node has either 0 not evaluated, 1 open evaluated, 2 closed evaluated status.  
	    }
		
		/// <summary>
		/// A bounding box defined in terms of contiguously connected tiles.  These
		/// define contiguous volumes of space and do not necessarily indicate
		/// traversability by any particular NPC.  Areas are always
        /// boxes and thus convex polygons.
		/// </summary>
		internal struct Area 
		{
			public int ID;    // == flattened (MinX,MinY,MinZ)
			public int MinX; 
			public int MinY; 
			public int MinZ;
			
			public int MaxX; 
			public int MaxY; 
			public int MaxZ;
			
			public int[] Portals; // index into Connectivity.Portals list

			public bool Contains (int x, int y, int z)
			{
				return x >= MinX && x <= MaxX && 
						y >= MinY && y <= MaxY && 
						z >= MinZ && z <= MaxZ;					
			}
			
			public void AddPortal(int portalID)
			{
				Portals = Portals.ArrayAppend (portalID); // TODO: profile, this could be slow.  Experiement with turning Portals into a List<>
			}
			
			public int LocatePortal (int x, int y, int z, AreaPortal[] graphPortals)
			{
				if (Portals == null) return -1;
				
				for (int i = 0; i < Portals.Length; i++)
					if ((graphPortals[Portals[i]].Location[0].X == x && 
					    graphPortals[Portals[i]].Location[0].Y == y &&
					    graphPortals[Portals[i]].Location[0].Z == z) || 
					    
					    (graphPortals[Portals[i]].Location[1].X == x &&
					    graphPortals[Portals[i]].Location[1].Y == y &&
					    graphPortals[Portals[i]].Location[1].Z == z))
					    
						return Portals[i];
					    	            		
        		return -1;
			}
		}

		internal struct AreaPortal
		{
			internal enum PortleFlags
			{
				None = 0,
				ZoneBoundary = 1 << 0,
				TwoWay = 1 << 1
			
			}
			
			// Location[0] and Location[1] form at the 2D portal between Areas.  
			public Vector3i[] Location;    // x,y,z tile locations on -/+ sides of the Area boundary
					
			
			public int[] RegionID; 
			public int[] AreaID; // array of 2 areaID's. lower ID is always at subscript[0]
			public PortleFlags Flags;    // Zone Boundary, 1way/2way, ladder, teleport, elevator, etc...
			

			public AreaPortal (int startTileX, int startTileY, int startTileZ, 
			                   int destTileX, int destTileY, int destTileZ,
			                   int startZoneID, int destinationZoneID, 
			                   int startingArea, int destinationArea)
			{
				
				RegionID = new int[2];
				RegionID[0] = startZoneID;
				RegionID[1] = destinationZoneID;
				
				AreaID = new int[2];
				AreaID[0] = startingArea;
				AreaID[1] = destinationArea;
				
				Location = new Vector3i[2];
				Location[0] = new  Vector3i(startTileX, startTileY, startTileZ);
				Location[1] = new Vector3i(destTileX, destTileY, destTileZ);
				
				Flags = 0;
			}
			
			public bool ZoneBoundary
	        {
	            get { return (Flags & PortleFlags.ZoneBoundary) == PortleFlags.ZoneBoundary; }
	            set 
	            {
	                if (value)
	                    Flags |= PortleFlags.ZoneBoundary;
	                else
	                    Flags &= ~PortleFlags.ZoneBoundary;
	            }
	        }
			
			public bool TwoWay
	        {
	            get { return (Flags & PortleFlags.TwoWay) == PortleFlags.TwoWay; }
	            set 
	            {
	                if (value)
	                    Flags |= PortleFlags.TwoWay;
	                else
	                    Flags &= ~PortleFlags.TwoWay;
	            }
	        }
		}
		
		
		// if all these could be tracked per Graph
		internal struct Graph
		{
			public int RegionID;              // can also be though of as GraphID
			public int LevelCount;
			public bool IsDirty;
			public GraphState mState;
			public Area[] Areas;
			public AreaPortal[] Portals;
			
			
			/// <summary>
			/// Locate adjacent Area in the AXIS direction specified.  If that Area is in a different Zone, 
			/// find that Zone's x,y,z offset and flatten it and pass it in out parameter.
			/// </summary>
			/// <param name="currentZoneID"></param>
			/// <param name="areas"></param>
			/// <param name="tileID"></param>
			/// <param name="axis"></param>
			/// <param name="gridSizeX"></param>
			/// <param name="gridSizeY"></param>
			/// <param name="gridSizeZ"></param>
			/// <param name="destinationZoneID"></param>
			/// <returns></returns>
			internal int LocateAreaContainingTile (int tileX, int tileY, int tileZ)
			{
				Area[] areas = this.Areas;
				if (areas == null || areas.Length == 0) 
					return -1;
	
				
				// find area if any, that contains this new x,y,z
				// TODO: what if the area is in an adjacent Zone?  We can't return a simple area index
				//       since the indices between zones can be the same.  So... what are we doing here?
				//
	
				// if x, y, z is within bounds of this zone, then one area should contain this tileID			
				for (int i = 0; i < areas.Length; i++)
					if (areas[i].Contains(tileX, tileY, tileZ))
					{
						return i;
					}
				
				// area might not exist for this tile.  eg. a terrain/structure might be taking up this entire tile
				return -1;
			}
		}
		
		// NOTE: mGraphs will be of size of entire world, however, not all Zone subscripts
		// will have instanced Graph elements.  
		private Graph[,] mGraphs;
		

		
		internal Connectivity(int width, int depth)
		{
			// intially, the graphs need to be sized to match the world size... 
			mGraphs = new Connectivity.Graph[width, depth];
		}
		
		internal Graph? this[int x, int z]
		{
			get 
			{
				if (mGraphs == null) return null;
				
				if (x > mGraphs.GetUpperBound (0) ||
				    z > mGraphs.GetUpperBound(1)) return null;
				
				return mGraphs[x, z];
			}
			
		}
		
		
		internal void NotifyOnStructureLoaded(TileMap.StructureVoxels structure, 
		                                    int layerSubscriptX, int layerSubscriptZ, 
		                                    int tileCountX, int tileCountZ)
		{
		                                    	
			mGraphs[layerSubscriptX, layerSubscriptZ] = 
				InitializeGraph (structure, layerSubscriptX, layerSubscriptZ, tileCountX, structure.mLevels.Length, tileCountZ);
		
		}
		
		#region IMapLayerObserver interface	        
        public void NotifyOnDatabaseCreated(TileMap.Structure structure, string layerName, int[,] data, 
		                                    int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ, 
		                                    int tileCountX, int tileCountZ, 
		                                    float width, float depth)
        {
    	    // initialize a Graph for this structure?
    	    // TODO: problem here is, how do we know when all levels for a particular structure are loaded?
    	    //       because that's when we really want to create the graph

    	    // TODO: when a new layer is added/removed by user during editing, should also recompute connectivity graph?
    	    				

			
			
        }
		
		
		// IMapLayerObserver.NotifyOnDatabaseChanged
        public void NotifyOnDatabaseChanged(string layerName, 
		                                    int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ,
		                                    int x, int z, int value)
        {
			
			
			// TODO: can we prevent the re-gen of Areas during series of changes and instead update only after?
			//       maybe here instead we can se g.Dirty flag instead?  
			switch (layerName)
			{
				case "obstacles":
					
					// TODO: prehaps each Graph can track certain data for us so we dont have to grab layer.TileCount and such
					// after the graph has been initially created?
					
					// update the Graph for this structure, but not when deserialization operation is occurring
					Graph g = mGraphs[layerSubscriptX, layerSubscriptZ];
					g.IsDirty = true;
									
					// update the graph value type in array 
					mGraphs[layerSubscriptX, layerSubscriptZ] = g;

					break;
				default:
					break;
			}
        }
		
		public void NotifyOnDatabaseDestroyed(int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ)
		{
		}
		#endregion
		
		public BroadPhasePathFindResults GraphFind (int layerSubscriptX, int layerSubscriptZ, Vector3i start,
		                      int destinationSubscriptX, int destinationSubscriptZ, Vector3i destination)
		{
			// find diff between start and end zones
			int diffX = destinationSubscriptX - layerSubscriptX ;
			int diffZ = destinationSubscriptZ - layerSubscriptZ;
			
			int stepX = diffX < 0 ? -1 : 1;
			int stepZ = diffZ < 0 ? -1 : 1;
    
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int subscriptX = i;
					int subscriptZ = j;
					
					Graph graph  = mGraphs[subscriptX, subscriptZ];
					//int[] gridData = Keystone.Scene.Scene.CopyGridData (structure, (uint)tileCountX, (uint)tileCountZ, (uint)tileCountY);
	      			//int[] gridData = Keystone.Scene.Scene.CopyGridData (structure, (uint)layer.TileCountX, (uint)layer.TileCountZ, (uint)g.LevelCount);
			
	    			int[] gridData = Core._Core.MapGrid.CopyData (TileMap.LayerType.obstacles, subscriptX, subscriptZ);
					
	    			// TODO: hard coded 63?? does this prevent 32x32x32 maps? i need to refresh my memory of this code first
	    			TileMap.MapLayer layer = Core._Core.MapGrid[subscriptX, 63, subscriptZ][(int)TileMap.LayerType.obstacles];
						    		
					graph.Areas = GenerateAreas (graph.RegionID, gridData, (int)layer.TileCountX , graph.LevelCount, (int)layer.TileCountZ);

					mGraphs[subscriptX, subscriptZ] = graph;
				}
			}
			
			
			// Portal Creation must occur _AFTER_ building areas for all available Zones?
			// how does that work with paging?
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int subscriptX = i;
					int subscriptZ = j;
					
					CreatePortals(mGraphs[subscriptX, subscriptZ].RegionID, 32, 2, 32, 3, 1, 3);	
					mGraphs[subscriptX, subscriptZ].IsDirty = false;	
				}
			}
			
			// TODO: i think portals between two adjacents needs to be RE-COMPUTED
			//       when one adjacent is loaded before the other!
			
			
			// flatten
			Graph[] tmp = new Connectivity.Graph[3 * 3 * 1];
			for (uint x = 0; x < 3; x++)
				for (uint z = 0; z < 3; z++)
				{
					uint index = Keystone.Utilities.MathHelper.FlattenCoordinate (x, 0, z, 3, 3);
					tmp[index] = mGraphs[x, z];
				}
			PathingBroadPhase broadphase = new PathingBroadPhase (tmp, 3, 1, 3);
			PathFindParameters parameters = new PathFindParameters();
			parameters.SearchLimit = 2000;
            parameters.HEstimate = 2; //1;
            
			BroadPhasePathFindResults results = broadphase.Find (mGraphs[layerSubscriptX, layerSubscriptZ], 
			                 mGraphs[destinationSubscriptX, destinationSubscriptZ], 
			                 start, destination, parameters);
			
			return results;
		}

		// whenever a Structure is fully loaded, we should initialize the grid
		// and from then on, whenever changes to maplayers?
		private Graph InitializeGraph(TileMap.StructureVoxels structure, int layerSubscriptX, int layerSubscriptZ, int tileCountX, int tileCountY, int tileCountZ)
		{								
			Graph graph = mGraphs[layerSubscriptX, layerSubscriptZ]; // new Connectivity.Graph();
	      	// graph = new Graph();
	      	graph.LevelCount = tileCountY;
	      	graph.IsDirty = true;
			graph.RegionID = (int)Utilities.MathHelper.FlattenIndex (layerSubscriptX, layerSubscriptZ, 3);
			
			return graph;
		}

				// TODO: how do we store areas (connectivity maps)?
		//       - if we store by array, then we store array Count, then for each Area we store MinX, MinY, MinZ, MaxX, MaxY, MaxZ.
		//         32bit * 6 per Area if we use ints.  
		//       - we could store this in bitmaps too if we want.
		//      But what about the Portal from one area to the next?  Storing it is PortalCount, then array of flattened indices to the MinX,Y,Z tiles.	
		//      	- note: each Area stores it's own Portal.  That means all links are 1 way from start Area to adjacent.  This way adjacents
		//          don't share Portal and so you can create areas that are traversable one-way only such as in a 2 story drop.
		//		   - well first of all, we should first simply track adjacent (connected) areas and not traversability itself!  Traversability
		//           is a further refinement that is dependant on Pathing.  For instance, a giant lake area connects to a flat land area
		//           but whether a unit can traverse from one to next is whether it can swim/float or vice versa operate on land or air.
		internal Area[] GenerateAreas(int zoneID, int[] grid, int tileCountX, int tileCountY, int tileCountZ)
		{
			System.Collections.Generic.List <Area> results = new System.Collections.Generic.List<Connectivity.Area>();
			Area[] areas;
			
			// create a 3d grid that we'll use to calculate our areas.  
			AreaBuilderNodeFast[] calcGrid = new AreaBuilderNodeFast[tileCountX * tileCountY * tileCountZ];
			// NOTE: no priority queue needed here.  We want the order of the queue to be FIFO.
			System.Collections.Generic.Queue<int> openQueue = new System.Collections.Generic.Queue<int>();
			
			                
			// TODO: predicate function eventually will be passed in to this Build() function so that we
			//       can build different connectivity maps based on different AI unit capabilities (eg jet pack, vs swim, vs tracked)
			Func<int[], int, bool> fConnectivityFunction = (int[] tileWeights, int tileID) =>
				{ 
					
					// if the weight of the grid makes it unpassable, we'll consider the path blocked		
					if (tileWeights[tileID] >= Pathing.TILE_UNPASSABLE) return false;
					
					return true;
				};
				
				
			bool stop = false;
			int currentTileID       =  0; // flatten (0,0,0) == 0
			uint currentX            = 0;
        	uint currentY            = 0;
        	uint currentZ            = 0;
        	
        	
			openQueue.Enqueue (currentTileID);
			
               
			// Area Creation - single zone only
			// -----------------------
			while(openQueue.Count > 0 && !stop)
			{
				bool areaOpen = true;
				
				currentTileID = openQueue.Dequeue();
				//Is it marked Closed? means this tile was already added to an area.
                if (calcGrid[currentTileID].Status == TraversalState.Closed)
                {
                	continue;
                }
                
				// unflatten tile index to X,Y,Z coordinates
                Utilities.MathHelper.UnflattenIndex ((uint)currentTileID, (uint)tileCountX, (uint)tileCountZ, out currentX, out currentY, out currentZ);
                 
                Area currentArea;
                currentArea.Portals = null;
                //currentArea.ID = currentTileID; // lowest tile (-x,-y,-z) within the area flattened is the area ID
 				currentArea.ID = results.Count;
 				
                currentArea.MinX = currentArea.MaxX = (int)currentX;
                currentArea.MinY = currentArea.MaxY = (int)currentY;
                currentArea.MinZ = currentArea.MaxZ = (int)currentZ;
				
                // is this first tile of new Area OOB or UNPASSABLE itself?
				bool expansionInitialSuccess = fConnectivityFunction(grid, currentTileID);
				if (expansionInitialSuccess ==false)
				{
					// get one adjacent tile from each of +X, +Y, +Z axis
					int[] adjacentTiles = null;
					if (currentX < tileCountX - 1)
						adjacentTiles = GetAxisExpansionTiles(AXIS.X, (int)currentX + 1, (int)currentY, (int)currentZ, (int)currentY, (int)currentZ, tileCountX, tileCountZ);
					if (currentY < tileCountY - 1)
						adjacentTiles = adjacentTiles.ArrayAppendRange (GetAxisExpansionTiles(AXIS.Y, (int)currentY + 1, (int)currentX, (int)currentZ, (int)currentX, (int)currentZ, tileCountX, tileCountZ));
					if (currentZ < tileCountZ - 1)
						adjacentTiles = adjacentTiles.ArrayAppendRange (GetAxisExpansionTiles(AXIS.Z, (int)currentZ + 1, (int)currentX, (int)currentY, (int)currentX, (int)currentY, tileCountX, tileCountZ));
					
					// enqueu and thus marks as OPEN and enqueues adjacent open list 
					for (int i = 0; i < adjacentTiles.Length; i++)
					{
						// never enqueue twice or if it's already closed
						if (calcGrid[adjacentTiles[i]].Status == TraversalState.Closed || calcGrid[adjacentTiles[i]].Status == TraversalState.Open) continue;
						
						openQueue.Enqueue (adjacentTiles[i]);
						calcGrid[adjacentTiles[i]].Status = TraversalState.Open;
						calcGrid[adjacentTiles[i]].AreaID = -1;
					}
					// mark the current as closed
					calcGrid[currentTileID].Status = TraversalState.Closed;
					continue;
				}
				
				while (areaOpen)
				{
					// NOTE: we try to expand along each axis in secession in order to keep
					//       dimensions perfectly box like. 
					bool expansionSuccessX = false;
					
					// +X Axis expansion evaluation 
					if (currentArea.MaxX < tileCountX - 1)
					{
						// find all IN BOUND tiles within the expanded direction.  
						int[] tiles = GetAxisExpansionTiles(AXIS.X, currentArea.MaxX + 1, currentArea.MinY, currentArea.MinZ, currentArea.MaxY, currentArea.MaxZ, tileCountX, tileCountZ);
						// _all_ must pass fTraversalTest() for the expansion to succeed.
						expansionSuccessX = Expand (openQueue, grid, calcGrid, tiles, currentArea.ID, fConnectivityFunction);
						if (expansionSuccessX)
							currentArea.MaxX++;				
					
						// if it fails, we still need to enqueue those 
					}
					
					bool expansionSuccessY = false;
					// +Y Axis expansion evaluation
					if (currentArea.MaxY < tileCountY - 1)
					{
						// find all IN BOUND tiles within the expanded direction.  
						int[] tiles = GetAxisExpansionTiles(AXIS.Y, currentArea.MaxY + 1, currentArea.MinX, currentArea.MinZ, currentArea.MaxX, currentArea.MaxZ, tileCountX, tileCountZ);
						// _all_ must pass fTraversalTest() for the expansion to succeed.
						expansionSuccessY = Expand (openQueue, grid, calcGrid, tiles, currentArea.ID, fConnectivityFunction);
						if (expansionSuccessY)
							currentArea.MaxY++;
					}
					
					bool expansionSuccessZ = false;
					// +Z Axis expansion evaluation
					if (currentArea.MaxZ < tileCountZ - 1)
					{
						// find all IN BOUND tiles within the expanded direction.  
						int[] tiles = GetAxisExpansionTiles(AXIS.Z, currentArea.MaxZ + 1, currentArea.MinX, currentArea.MinY, currentArea.MaxX, currentArea.MaxY, tileCountX, tileCountZ);
						// _all_ must pass fTraversalTest() for the expansion to succeed.
						expansionSuccessZ = Expand (openQueue, grid, calcGrid, tiles, currentArea.ID, fConnectivityFunction);
						if (expansionSuccessZ)
							currentArea.MaxZ++;
					}
					
					// if expansion on all 3 axis fail this iteration, close this area even if
					// only the currentTileID is the only tile in it
					if (!expansionSuccessX && !expansionSuccessY && !expansionSuccessZ)
					{
						areaOpen = false;
					}
					
					calcGrid[currentTileID].Status = TraversalState.Closed;
				} // end inner while
				
 				
 				// add Area to results and start on next Area if there are still open nodes
 				results.Add (currentArea);
				
				
			} // end outer while
			
			areas = results.ToArray();
			System.Diagnostics.Debug.Assert (areas != null && areas.Length > 0);
			
			return areas;
		}
		
		private const int ZONE_IS_OUT_OF_WORLD_BOUNDS = -1;
		private void CreatePortals(int zoneID, int tileCountX, int tileCountY, int tileCountZ, int regionCountX, int regionCountY, int regionCountZ)
		{
			int zoneX, zoneZ;
			Utilities.MathHelper.UnflattenIndex ((uint)zoneID, (uint)regionCountX, out zoneX, out zoneZ);
			
			TraversalState[] areaVisitedStates = new TraversalState[mGraphs[zoneX, zoneZ].Areas.Length];
			System.Collections.Generic.Queue <int> areaQueue = new System.Collections.Generic.Queue<int>();
						
			
			areaQueue.Enqueue (0);
			// flag areaIndex as "closed" so that we do not try to queue it again
			areaVisitedStates[0] = TraversalState.Closed;
					
			const int faceCount = 3;
			while (areaQueue.Count > 0)
			{
				int areaIndex = areaQueue.Dequeue();				
				Area[] currentAreas = mGraphs[zoneX, zoneZ].Areas;
				
				// create all portals first for each face of each area
				// they are indexed by an ID into portal list.
				for (int i = 0; i < faceCount; i++)
				{
					int[] faceTiles = GetFaceTiles (currentAreas[areaIndex], (AXIS)i, tileCountX, tileCountY, tileCountZ);
					if (faceTiles != null) 
					{
						// find zone along this adjacent axis to see if we've crossed zone boundaries
						int destZoneX, destZoneY, destZoneZ;
						int destinationZoneID =  LocateZone (zoneX, zoneZ, (uint)currentAreas[areaIndex].MaxX, 1, (uint)currentAreas[areaIndex].MaxZ, (AXIS)i, 
						                                     tileCountX, tileCountY, tileCountZ, 
						                                     regionCountX, regionCountY, regionCountZ, 
						                                     out destZoneX, out destZoneZ);
						// out of world bounds
						if (destinationZoneID == ZONE_IS_OUT_OF_WORLD_BOUNDS) continue;
						

						// NOTE: areas may be null if that graph has not been loaded yet
						// destAreas are all available areas in the destination Zone.
						Area[] destZoneAreas = mGraphs[destZoneX, destZoneZ].Areas;						
						if (destZoneAreas == null) continue;
						
						
						for (int j = 0; j < faceTiles.Length; j++)
						{
							uint startTileX, startTileY, startTileZ;
							uint destTileX, destTileY, destTileZ;
							
							Utilities.MathHelper.UnflattenIndex  ((uint)faceTiles[j], (uint)tileCountX,(uint)tileCountZ, out startTileX, out startTileY, out startTileZ);
							
							// find the adjacent tile in along the AXIS direction specified and the Zone it's in.
							int adjacentTileIndex = LocateAdjacentTile (startTileX, startTileY, startTileZ, (AXIS)i, 
							                                            tileCountX, tileCountY, tileCountZ, 
							                                           out destTileX, out destTileY, out destTileZ);
							
							
							// find the area in the AXIS direction across from this tile 
							// and connect via portal							
							int adjacentAreaIndex = mGraphs[destZoneX, destZoneZ].LocateAreaContainingTile ((int)destTileX, (int)destTileY, (int)destTileZ);
							// if there is no area, we need no portal
							if (adjacentAreaIndex == -1) continue;

			
							AreaPortal portal = new AreaPortal ((int)startTileX, (int)startTileY, (int)startTileZ, 
							                                    (int)destTileX, (int)destTileY, (int)destTileZ, 
							                                    zoneID, destinationZoneID, 
							                                    currentAreas[areaIndex].ID, destZoneAreas[adjacentAreaIndex].ID);
							
							// add portal to BOTH the source and destination areas
							if (zoneID != destinationZoneID)
							{
								// since the start & dest zone's are different we'll add a unique 1-way portal to each
								// since this way we can page in one zone without the need for the other to be available
								// immediately
								portal.ZoneBoundary = true;
								portal.TwoWay = true;
								mGraphs[zoneX, zoneZ].Portals = mGraphs[zoneX, zoneZ].Portals.ArrayAppend (portal);
								int portalID = mGraphs[zoneX, zoneZ].Portals.Length - 1;
								currentAreas[areaIndex].AddPortal (portalID);
								
								AreaPortal destZonePortal = new AreaPortal ((int)destTileX, (int)destTileY, (int)destTileZ,
																 (int)startTileX, (int)startTileY, (int)startTileZ,								                                            
							                                     destinationZoneID, zoneID,  // note: dest and zone are reversed here
							                                     destZoneAreas[adjacentAreaIndex].ID, currentAreas[areaIndex].ID);
								
								destZonePortal.ZoneBoundary = true;
								destZonePortal.TwoWay = true;
								
								// TODO: Zone portals can easily be encoded in a 32x4 (or Nx4) sized image
								//       we intrinsically know which border is which and which direction and thus
								//       which adjacent tile in the adjacent zone.  the only information each tile needs to contain
								//       are the src AreaID and target AreaID.  But this means zone connectivity at all 4 edges
								//       can be looked up without having to load any interior areas or portals which are can be
								//       of arbitrary sizes and locations
								// TODO: Furthermore, a SECOND TYPE of connectivity instead of storing src and target AreaIDs 
								//       can indicate which cardinal adjacent and diagonal zones can be reached from it.
					            //       using just 8 flags.  we dont need to store zone ids since those can be inferred								
								
								mGraphs[destZoneX, destZoneZ].Portals = mGraphs[destZoneX, destZoneZ].Portals.ArrayAppend (destZonePortal);
								int destZonePortalID = mGraphs[destZoneX, destZoneZ].Portals.Length - 1;
								destZoneAreas[adjacentAreaIndex].AddPortal(destZonePortalID);

								// NOTE: we reassign areas to relevant graph indices since Area is value type
								// so changes to it do not automatically update copies in mGraph[]
								mGraphs[zoneX, zoneZ].Areas = currentAreas;
								mGraphs[destZoneX, destZoneZ].Areas = destZoneAreas;
							}
							else 
							{
								// current and dest zone are same so only need to work with currentAreas[]
								portal.ZoneBoundary = false;
								portal.TwoWay = false;
								mGraphs[zoneX, zoneZ].Portals = mGraphs[zoneX, zoneZ].Portals.ArrayAppend(portal);
								int portalID = mGraphs[zoneX, zoneZ].Portals.Length - 1;
								currentAreas[areaIndex].AddPortal (portalID);
								currentAreas[adjacentAreaIndex].AddPortal(portalID);
								
								mGraphs[zoneX, zoneZ].Areas = currentAreas;	
							}
							
							
							// add the adjacent areas to queue if they are not in closed list
							// but _ONLY_ if the adjacent is in this same zone!  We will handle
							// resuming of portal creation in other zones some other way TODO: unless we
							// decide to recurse across borders ultimately and to stop if PORTALS_DIRTY flag == false
							if (destinationZoneID == zoneID)
							{
								if (areaVisitedStates[adjacentAreaIndex] == TraversalState.Closed) continue;
								areaQueue.Enqueue(adjacentAreaIndex);
								
								// set adjacent status to closed so we dont add it more than once within this loop through faceTiles
								areaVisitedStates[adjacentAreaIndex] = TraversalState.Closed;
							}
						}
					}
					
					// merge all portals for this face of area that connect to the same dest area
					// TODO: but don't we have to ensure the portals are adjacent to each other?  
					//       maybe we wont even merge them at first.  					
					// area.MergePortals((AXIS)i);
				}
			}
		}
		
		private int LocateZone (int zoneX, int zoneZ,
		                        uint tileX, uint tileY, uint tileZ, AXIS axis,
		                        int gridSizeX, int gridSizeY, int gridSizeZ, 
		                        int worldSizeX, int worldSizeY, int worldSizeZ, 
		                        out int destZoneX, out int destZoneZ)
		{
			int gridOffsetX = 0;
			int gridOffsetY = 0;
			int gridOffsetZ = 0;
			
			switch (axis)
			{
				case AXIS.X:
					tileX++;
					if (tileX >= (uint)gridSizeX)
					{
						tileX -= (uint)gridSizeX;
						gridOffsetX++;
					}
					break;
				case AXIS.Y:
					tileY++;
					if (tileY >= (uint)gridSizeY)
					{
						tileY -= (uint)gridSizeY;
						System.Diagnostics.Debug.Assert (worldSizeY == 1, "Zone based world maps must always have worldSizeY == 1");
						// for Y axis, raises level but does not change Zone which is fixed at 1 zone high for land based maps.
						//gridOffsetY++;
					}
					break;
				case AXIS.Z:
					tileZ++;
					if (tileZ >= (uint)gridSizeZ)
					{
						tileZ -= (uint)gridSizeZ;
						gridOffsetZ++;
						
					}
					break;
				default:
					throw new Exception ();
			}
			
			
			destZoneX = zoneX + gridOffsetX;
			destZoneZ = zoneZ + gridOffsetZ;
			
			if (destZoneX >= worldSizeX || destZoneZ >= worldSizeZ)
				return ZONE_IS_OUT_OF_WORLD_BOUNDS;
									
			// we always use 2D Flatten because Zone based world maps must always have worldSizeY == 1
			int zoneID = (int)Utilities.MathHelper.FlattenIndex (destZoneX, destZoneZ, (uint)worldSizeX);
			return zoneID;
			
		}
		
		
		
		private int LocateAdjacentTile (uint tileX, uint tileY, uint tileZ, 
		                                AXIS axis, 
		                                int gridSizeX, int gridSizeY, int gridSizeZ, 
		                                out uint adjacentTileX, out uint adjacentTileY, out uint adjacentTileZ)
		{

			switch (axis)
			{
				case AXIS.X:
					tileX++;
					if (tileX >= (uint)gridSizeX)
						tileX -= (uint)gridSizeX;
					break;					
				case AXIS.Y:
					tileY++;
					if (tileY >= (uint)gridSizeY)
						tileY -= (uint)gridSizeY;
					break;
				case AXIS.Z:
					tileZ++;
					if (tileZ >= (uint)gridSizeZ)
						tileZ -= (uint)gridSizeZ;
					break;
				default:
					throw new Exception ();
			}
			
			
			adjacentTileX = tileX;
			adjacentTileY = tileY;
			adjacentTileZ = tileZ;
			
			int adjacentTileID = (int)Utilities.MathHelper.FlattenCoordinate (tileX, tileY, tileZ, (uint)gridSizeX, (uint)gridSizeZ);
			
			return adjacentTileID;
		}
				
		private int[] GetFaceTiles(Area area, AXIS axis, int gridSizeX, int gridSizeY, int gridSizeZ)
		{
			
			int[] tiles;
			int width = 1 + area.MaxX - area.MinX;
			int height = 1 + area.MaxY - area.MinY;
			int depth = 1 + area.MaxZ - area.MinZ;
			
			int count = 0;
			
			switch (axis)
			{
				case AXIS.X:	
					// if the adjacent tiles along X axis will be OOB with respect to this Zone they
					// can still be in bounds with respect to adjacent Zone and so we do not
					// try to filter tiles 
					tiles = new int[height * depth];
					
					for (uint i = 0; i < height; i++)
						for (uint j = 0; j < depth; j++)
							tiles[count++] = (int)Utilities.MathHelper.FlattenCoordinate ((uint)area.MaxX, (uint)area.MinY + i, (uint)area.MinZ + j, (uint)gridSizeX, (uint)gridSizeZ);
							
					break;
				case AXIS.Y:
					// if the adjacent tiles along Y axis will be OOBwith respect to this Zone they
					// can still be in bounds with respect to adjacent Zone and so we do not
					// try to filter tiles 
					tiles = new int[width * depth];
					
					for (uint i = 0; i < width; i++)
						for (uint j = 0; j < depth; j++)
							tiles[count++] = (int)Utilities.MathHelper.FlattenCoordinate ((uint)area.MinX + i, (uint)area.MaxY, (uint)area.MinZ + j, (uint)gridSizeX, (uint)gridSizeZ);
					
					break;
				case AXIS.Z:
					// if the adjacent tiles along Z axis will be OOBwith respect to this Zone they
					// can still be in bounds with respect to adjacent Zone and so we do not
					// try to filter tiles 
					tiles = new int[width * height];
					
					for (uint i = 0; i < width; i++)
						for (uint j = 0; j < height; j++)
							tiles[count++] = (int)Utilities.MathHelper.FlattenCoordinate ((uint)area.MinX + i, (uint)area.MinY + j, (uint)area.MaxZ, (uint)gridSizeX, (uint)gridSizeZ);
					
					break;
					
				default:
					throw new Exception("We should never have any other axis type.  NOTE: There should never be a need for negative axis directions either.");
			}
			
			return tiles;
			
		}
		
		private int[] GetAxisExpansionTiles(AXIS axis, int axisValue, int minA, int minB, int maxA, int maxB, int gridSizeX, int gridSizeZ)
		{
			System.Diagnostics.Debug.Assert (axisValue >= 0 && minA >= 0 && maxA >= 0);
			System.Diagnostics.Debug.Assert (minA <= maxA && minB <= maxB);
			
			int size = (maxB - minB + 1) * (maxA - minA + 1);
			int[] results = new int[size];
			int count = 0;
	
			for (uint a = (uint)minA; a <= (uint)maxA; a++)
				for (uint b = (uint)minB; b <= (uint)maxB; b++)
					// out of bounds tiles are guaranteed to never occur
					// so long as the parameters passed in are all within bounds
					switch (axis)
					{
						case AXIS.X:
							results[count++] = (int)Utilities.MathHelper.FlattenCoordinate ((uint)axisValue, a, b, (uint)gridSizeX, (uint)gridSizeZ);
							break;
						case AXIS.Y:
							results[count++] = (int)Utilities.MathHelper.FlattenCoordinate (a, (uint)axisValue, b, (uint)gridSizeX, (uint)gridSizeZ);
							break;
						case AXIS.Z:
							results[count++] = (int)Utilities.MathHelper.FlattenCoordinate (a, b, (uint)axisValue, (uint)gridSizeX, (uint)gridSizeZ);
							break;
					}

			return results;
		}
		
		private bool Expand (System.Collections.Generic.Queue<int> openQueue, int[]grid, AreaBuilderNodeFast[] calcGrid, int[] tiles, int areaID, Func<int[], int, bool> fTraversalTest)
		{
			bool allSuccess = true;
			bool[] success = new bool[tiles.Length];
						
			// to expand the Area box, all tiles must pass fTraversalTest()
			for (int i = 0; i < tiles.Length; i++) 
			{

				int adjacentTileID = tiles[i];
				if (calcGrid[adjacentTileID ].Status == TraversalState.Closed) 
					success[i] = false;
				else
					success[i] =  fTraversalTest(grid, adjacentTileID);
				
				if (success[i] == false)
				{
					if (allSuccess == true) allSuccess = false;
				}
			}
			
			// if all tiles pass, mark all as "closed" and return TRUE
			if (allSuccess)
			{
				for (int i = 0; i < tiles.Length; i++)
				{
					int adjacentTileID = tiles[i];
					// successful, we mark as "closed" all evaluated tiles
					// mark as closed
					calcGrid[adjacentTileID].Status = TraversalState.Closed;
					calcGrid[adjacentTileID].AreaID = areaID;
				}
				return true;
			}
			
			// if any single tiles fail, we cannot expand.  We mark ONLY 
			// the ones that passed as Open, but we still return FALSE			

			for (int i = 0; i < tiles.Length; i++) 
			{
				int adjacentTileID = tiles[i];
//				// mark every adjacent in this axis' expansion direction as Open 
//				if (success[i])
//				{
					// do not enqueue it again if it's closed or if it's already in the openQueue list	
					if (calcGrid[adjacentTileID].Status == TraversalState.Closed || calcGrid[adjacentTileID].Status == TraversalState.Open) continue;
				
					calcGrid[adjacentTileID].Status = TraversalState.Open;
					calcGrid[adjacentTileID].AreaID = -1;

					openQueue.Enqueue (adjacentTileID);
//				}
//				else  
//					// it failed because it's either already closed or its weight is UNPASSABLE.
//					// however, unpassable still means we need to be able to traverse it's adjacents!
//					// otherwise we stall.
//					calcGrid[adjacentTileID].Status = TraversalState.Closed;
			}

			
			return false;
		}
		
	}
}
