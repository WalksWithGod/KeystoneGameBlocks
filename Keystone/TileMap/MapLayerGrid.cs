using System;


namespace Keystone.TileMap
{
	internal interface IMapLayerGridObserver
	{
		void NotifyOnDatabaseCreated(TileMap.Structure structure, string layerName, int[,] data, 
		                             int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ, 
		                             int tileCountX, int tileCountZ, float width, float depth);
		void NotifyOnDatabaseChanged(string layerName, 
		                             int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ,
		                             int x, int z, int value);
		void NotifyOnDatabaseDestroyed(int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ);
	}
	
	/// <summary>
	/// MapGrid manages the low level MapLayer objects in a way that maintains
	/// adjacencies to one another automatically.  This will be useful for 
	/// AI as well.
	/// NOTE: We may even be able to hold multiple resolutions for certain MapGrid instances
	/// so that one instance is for AI and where the resolution per zone is just 1x1 and a list
	/// of entities.
	/// </summary>
	internal class MapLayerGrid
	{
		
		// 3,450 x 128 x 3,840  = 1.8 Gigabytes of space just to hold the MapLayer references for a world map the size of Great Britain
		// 128 x 128 x 128      = 2 Megabytes of space just to hold the MapLayer references for a world map the size of Jersey.
		//                   			- that's not too bad... 
		// (DaggerFall allocates 3.4 megabytes for storing objects which all objects that are in memory use. But here we're talking just
		// for the map data.  Now for a larger world (eg Great Britain sized) then we could simply connect more of our 128x128x128 Grid's together.
		// It would be 30x1x30  (900 total small grids required.  900 x 2 MB = 1.8 Gigabytes just for the zone placeholder data on disk although empty zones could be skipped?)
		// The boundaries between super-regions can be constructed in such a way as to make the swapping out of one zone and the next invisible... 
		// eg. huge mountain borders, wide ocean channels, vast flat deserts, etc
		
		// TODO: has the concept of "layer" removed need to have any more than 1 zone high along Y axis?
		//       I think yes.  I think we should for this type express Levels within a Zone as layers within same zone
		//       and not stacking of multiple Zones.  Now ultimatley it's a distinction which may not matter too much
		//       as only our Pager really cares i think because if we switched to all Zones, we'd be doing away with the idea
		//       of "StructureLayers" so that our MapLayer[,,] mGrid still mapped altitude wise to just one layering device (StructureLevels or Zones but not both)
		//       and for paging purposes, breaking levels down to Zones by such small layers seems wrong.
		//       - suppose if we really found a need, we could create seperate MapGrid's for when we extend beyond 128 and now have a new zone above or below.
		
		Portals.ZoneRoot mZoneRoot;
		
		// MapLayer elements at any subscript CAN be null or perhaps their internal bmp data can be unloaded.  
        // Most elements in the sizeY range WILL be null!  
		// All elements must share same resolution or there is no way to match up adjacent tiles across boundaries                    
		MapLayer[,,][] mGrid;
							

		/// <summary>
		/// 
		/// </summary>
		/// <param name="zoneRoot">There is no point in using MapGrid outside of multi-zone simulations.</param>
		/// <param name="sizeX"></param>
		/// <param name="sizeY"></param>
		/// <param name="sizeZ"></param>
		public MapLayerGrid (Portals.ZoneRoot zoneRoot, int sizeX, int sizeY, int sizeZ)
		{
			mGrid = new MapLayer[sizeX, sizeY, sizeZ][];
			mZoneRoot = zoneRoot; 
		}
		
		public MapLayer[] this[int x, int y, int z]
		{
			get 
			{
				//int layerType = GetLayerType(layerName); 
				return mGrid[x, y, z]; // [layerType];
			}
		}
			
		public MapLayer this[string layerName, int x, int y, int z]
		{
			get 
			{
				int layerType = GetLayerType(layerName); 
				return mGrid[x,y,z][layerType];
			}
		}
		
        #region ISubject Members
        private System.Collections.Generic.List <IMapLayerGridObserver> mObservers;
        // a HUD item instead of updating every frame, can accumulate all changes
        // between Update() and then apply them at once and only when there are changes
        // since modifying the tilemaskgrid is expensive.
        public void Attach (IMapLayerGridObserver observer)
        {
        	if (mObservers == null) mObservers = new System.Collections.Generic.List<IMapLayerGridObserver>();
            mObservers.Add(observer);
            
            // when initially attaching, all existing MapLayers need to result in 
            // observer.NotifyOnDatabaseCreated(layer.Name, layer.SubscriptX, layer.SubscriptY, layer.SubscriptZ); 
        }
        
        public void Detach(IMapLayerGridObserver observer)
        {
            mObservers.Remove(observer);
            if (mObservers.Count == 0) mObservers = null;
        }

        
        
        // called during this.Bind()
        internal void NotifyBind(MapLayer layer) 
        {            
            if (mObservers == null || mObservers.Count == 0) return;
            
        	string structureID = TileMap.Structure.GetStructureID (layer.SubscriptX, 0, layer.SubscriptZ);
        	TileMap.Structure structure = (TileMap.Structure)Keystone.Resource.Repository.Get (structureID);
            	

        	
            foreach (IMapLayerGridObserver observer in mObservers)
            {
            	int tileCountX = (int)layer.TileCountX;
            	int tileCountZ = (int) layer.TileCountZ;
            	float width = (float)layer.TileSize.x * tileCountX;
            	float depth = (float)layer.TileSize.z * tileCountZ;
            	
            	int[,] data = new int[layer.mTileData.Width, layer.mTileData.Height]; 
            	
            	for (int i = 0; i < layer.mTileData.Width; i++)
            		for (int j = 0; j < layer.mTileData.Height; j++)
            			data[i,j] = layer.GetMapValue (i, j);
            	

            	observer.NotifyOnDatabaseCreated(structure, layer.Name, data, layer.SubscriptX, layer.SubscriptY, layer.SubscriptZ, tileCountX, tileCountZ, width, depth);
            }
            
        }
        
        // called by MapLayer.SetMapValue() when one tile has changed so that we can
        // relay to the observers HUD and Connectivity
        internal void NotifyChange(MapLayer layer, int x, int z, int value) 
        {            
            if (mObservers == null) return;
            foreach (IMapLayerGridObserver observer in mObservers)
            {
                observer.NotifyOnDatabaseChanged(layer.Name, layer.SubscriptX, layer.SubscriptY, layer.SubscriptZ, x, z, value);
            }
            
            // TODO: should connectivity be updated here as observer or from within MapLayer itself?
        }
        
        // called by MapLayer.SetMapValue() when a rectangular block of tiles has changed   
        internal void NotifyChange (MapLayer layer, int x, int z, int[,] value)
        {
        	int width = value.GetLength(0);
        	int depth = value.GetLength(1);
        	
        	
        }
           
		// called by MapLayer.SetMapValue() when a rectangular block of tiles has changed        
        internal void NotifyChange (MapLayer layer, int x, int z, int stride, int[] value)
        {
        	//mObservers
        }
        #endregion
        
		public MapLayer Bind (string layerName, int locationX, int locationY, int locationZ,
		                      MapLayer.OnMapLayerChanged onChangedHandler, MapLayer.OnMapLayerValidateChange onValidateHandler)
		{
			System.Diagnostics.Debug.Assert (locationX >= 0 && locationX <= mGrid.GetUpperBound(0));
			System.Diagnostics.Debug.Assert (locationY >= 0 && locationY <= mGrid.GetUpperBound(1));
			System.Diagnostics.Debug.Assert (locationZ >= 0 && locationZ <= mGrid.GetUpperBound(2));
			
			int layerType = GetLayerType (layerName);
            
			MapLayer layer = mGrid[locationX, locationY, locationZ][layerType];
			
			if (layer == null) throw new ArgumentNullException("MapLayer.Bind() - ERROR: Specified MapLayer not set.");
			
			layer.mOnChangedHandler = onChangedHandler;
			layer.mOnValidateHandler = onValidateHandler;
			
			NotifyBind (layer);
			return layer;
		}
		
		public MapLayer Create (string layerName, 
		                        int locationX, int locationY, int locationZ,
		                        int floorLevel, float floorWidth, float floorHeight, float floorDepth)
		{
			System.Diagnostics.Debug.Assert (mZoneRoot.StartY == 0 && mZoneRoot.StopY  == 0, "Zone's Y Axis is limited to 1-D.  To achieve 3-D we use multiple levels such as a skyscraper");

			System.Diagnostics.Debug.Assert (locationX >= 0 && locationX <= mGrid.GetUpperBound(0));
			System.Diagnostics.Debug.Assert (locationY >= 0 && locationY <= mGrid.GetUpperBound(1));
			System.Diagnostics.Debug.Assert (locationZ >= 0 && locationZ <= mGrid.GetUpperBound(2));
			
			// locationX start from 0 to ZoneCountX;
			// locationY start from 0 to ZoneCountY;
			// locationZ start from 0 to ZoneCountZ;
					
			// to find the layers world offset 
			float offsetX = mZoneRoot.StartX + locationX;
            float offsetY = mZoneRoot.StartY + locationY;
            float offsetZ = mZoneRoot.StartZ + locationZ;
            
            // to find the center position of the zone in world space
            double layerWorldCenterX = offsetX * mZoneRoot.RegionDiameterX;
            double layerWorldCenterY = offsetY * mZoneRoot.RegionDiameterY;
            double layerWorldCenterZ = offsetZ * mZoneRoot.RegionDiameterZ;
            

            // if no layers for this grid location jagged array exists, first create the jagged array 
            if (mGrid[locationX, locationY, locationZ] == null)
	            mGrid[locationX, locationY, locationZ] = new MapLayer[6];
            
            
            int layerType = GetLayerType(layerName);


            // verify this layer is not already created.  we should explicitly destroy before creating replacement layer
            System.Diagnostics.Debug.Assert (mGrid[locationX, locationY, locationZ][layerType] == null, "MapLayerGrid.Create() - ERROR: Grid already exists.");
            
            string layerPath = Structure.GetLayerPath (locationX, locationZ, floorLevel, layerName);
            
            MapLayer layer = new  MapLayer (layerName, layerPath,
                                            locationX, locationY, locationZ, 
                                            floorLevel,
                                            floorWidth, floorHeight, floorDepth);
            
            
            // TODO: WARNING!!!!!!!!!!!!!! <-- changing default values will not get written if we refuse to always rewrite file instead of skipping if it already exists!!!!!!!!!!!!!!!!!!!!!!!!!! 
			// if the storage file does not exist, create it
			int initializationValue = 0;
			if (layerName == "obstacles")
			{
				initializationValue = 0; // segments placed on this LEVEL affect obstacle map of the level BELOW it!
			}
			else if (layerName == "layout")
			{
				initializationValue = 0; // segment Index
			}
			else if (layerName == "style") // "style"
			{
				// style has to be discovered during autotile
				initializationValue = -1; // model index
			}
        			
			Keystone.Celestial.ProceduralHelper.InitializeMapLayerBitmap(layerName, floorLevel, locationX, locationZ, 32, 32, initializationValue);
			
			layer.Initialize ();
			
			
            mGrid[locationX, locationY, locationZ][layerType] = layer;
            return layer;
		}
        
        internal void Destroy (int locationX, int locationY, int locationZ)
        {
        	// TODO: need a synclock between destroy and create so we cannot do both.
        	System.Diagnostics.Debug.Assert (mGrid[locationX, locationY, locationZ] != null);
        	
        	if (mGrid[locationX, locationY, locationZ].Length == 0) return;
        	
        	for (int i = 0; i < mGrid[locationX, locationY, locationZ].Length; i++)
        	{
        		// NOTE: some zones do not have all map layers present! recall that we have
        		// 6 slots of maplayers and not all are applicable to each zone
        		if (mGrid[locationX, locationY, locationZ][i] == null) continue;
        		mGrid[locationX, locationY, locationZ][i].Dispose();
        	}
        	
        	mGrid[locationX, locationY, locationZ] = null;
        	
        	foreach (IMapLayerGridObserver observer in mObservers)
            {
        		observer.NotifyOnDatabaseDestroyed(locationX, locationY, locationZ);
        	}
        }
        
        
        internal int[] CopyData (LayerType layerType, int locationX, int locationZ)
        {
        	
        	// get the layer and it's resolution
        	int levelCount = 0;
        	int startLevelIndex = 0;
        	
        	int yLength = mGrid.GetLength(1);
        	
        	bool foundStart = false;
        	// the first Level of map data does not being at y = 0 necessarily.  
        	// it may actually start at halfway between min/max thus representing sea level
        	// so iterate until we find first non null layer
        	for (int y = 0; y < yLength; y++)
        	{	
        		if (foundStart == false && mGrid[locationX, y, locationZ] != null)
        		{
        			foundStart = true;
        			startLevelIndex = y;
        		}
        		if (foundStart)
        		{
        			if (mGrid[locationX, y, locationZ] == null)
        				break;
        			else 
        				levelCount++;
        		}
        	}
        	
        	
			MapLayer layer = mGrid[locationX, startLevelIndex, locationZ][(int)layerType];
			uint tileCountX = layer.TileCountX;
			uint tileCountZ = layer.TileCountZ;
			
        	int[] grid = new int[tileCountX * tileCountZ * levelCount];
        	
        	for (uint y = 0; y < levelCount; y++)
			{
				layer = mGrid[locationX, startLevelIndex + y, locationZ][(int)layerType];
				
				for (uint x = 0; x < tileCountX; x++)
					for (uint z = 0; z < tileCountZ; z++)				
					{
						int index2D = (int)Utilities.MathHelper.FlattenIndex ((int)x, (int)z, tileCountX);
						// TODO: we really _must_ find a way to grab all this data
						//       by just passing a reference to the array so we dont have to copy
						// Buffer.BlockCopy ();
						//layer.Data;
						
						// NOTE: indices into individual map layers use 2D flattened index values
						int weight = layer.GetMapValue ((uint)index2D);						
						// NOTE: a weight (traversal cost) value of 0 is an empty hole in the map that cannot be traversed

						// NOTE: indices into a combined grid for entire structure (and all of it's floor levels) uses 3D flattened index values
						int index3D = (int)Utilities.MathHelper.FlattenCoordinate (x, y, z, tileCountX, tileCountZ);
						grid[index3D] = weight;
					}
			}
			return grid;
        }
		
		internal int[] GetAdjacentTileTypes (LayerType layerType, 
		                                     int locationX, int locationY, int locationZ, 
		                                     int tileX, int tileZ, 
		                                     out System.Drawing.Point[] wrappedTiles, out MapLayer[][] wrappedLayers)
		{	
			 // 9+9+9 adjacents = 27
			const int ADJ_COUNT = 27; 			
			// get the layer and it's resolution
			MapLayer layer = mGrid[locationX, locationY, locationZ][(int)layerType];
			int sizeX = (int)layer.TileCountX;
			int sizeZ = (int)layer.TileCountZ;
			
			int gridSizeX = mGrid.GetUpperBound (0) + 1;
			int gridSizeY = mGrid.GetUpperBound (1) + 1;
			int gridSizeZ = mGrid.GetUpperBound (2) + 1;

						
			// if our tile is on a boundary, then the adjacent on that side will cross over
			// NOTE: 64 is 2^6 which is what the below forloop covers
			MapLayer[][] lookup = new MapLayer[64][];
			
			// since we cannot lookup into multi-dimensional array
			// we'll construct a 1 dimensional lookup table that maps
			// to our mGrid[,,] layers
			for (int y = - 1; y <= 1; y++)			
				for (int z = - 1; z <= 1; z++)
					for (int x = - 1; x <= 1; x++)
					{
						// check layer location against world boundaries
						if (LayerInBounds (locationX + x, locationY + y, locationZ + z, gridSizeX, gridSizeY, gridSizeZ))
						{
							int index = 0;
							// there are 64 possible combinations of x,y,z axis locations
							index |= (x < 0) ? 1 << 0 : 0;
							index |= (x > 0) ? 1 << 1 : 0;
							index |= (z < 0) ? 1 << 2 : 0;
							index |= (z > 0) ? 1 << 3 : 0;
							index |= (y < 0) ? 1 << 4 : 0;
							index |= (y > 0) ? 1 << 5 : 0;

									
							bool loaded = mGrid[locationX + x, locationY + y, locationZ + z] != null;
							if (!loaded) 
							{
								// do we want to temporarily load this layer which may be paged out?
								// we don't have to store it in mGrid[] but can store it in a tempary array
								// as well as our lookup[] so that we can unload them after this call..
								//string layerPath = Structure.GetLayerPath (locationX, locationY, locationZ, layer.mLevel.FloorLevel, layer.Name, layer.mStructure.mRelativeLayerDataPath);
            					// TODO: here we just want the bitmap data so we can read it..
								
							}
							
							// store this layer in lookup at index
							lookup[index] = mGrid[locationX + x, locationY + y, locationZ + z];
						}
			}
			
			
			int[] indices = new int[ADJ_COUNT];
			wrappedTiles = new System.Drawing.Point[ADJ_COUNT];
			wrappedLayers = new MapLayer[ADJ_COUNT][];
			int count = 0;
			// a second lookup now 	 
			for (int y = locationY -1; y <= locationY + 1; y++)			
				for (int z = tileZ - 1; z <= tileZ + 1; z++)
					for (int x = tileX - 1; x <= tileX + 1; x++)
					{
				#if DEBUG
						if (tileX == x && tileZ == z && y == locationY)
						{
							// this is the current tile, not an adjacent
							// but we must fill this position in the array because
							// our TileDirections enums assume 9x9x9 array
							// this will always occur at count == 13. (14th element, TileDirections.Center)
							System.Diagnostics.Debug.Assert (count == (int)TileDirections.Center);
						}
				#endif
				
					// produces indices in the following 3x3x3 layout
					// These values are synced to "TileDirections" enum.
					//	 _____  _____ ____
					//	/ 24  /  25 /  26 /
					//	----- ----- -----
					//	 _____  _____ ____
					//	/ 21  /  22 /  23 /   <-- upper 3x3 grid
					//	----- ----- -----
					//	 _____  _____ ____
					//	/ 18  /  19 /  20 /
					//	----- ----- -----
					
					//	 _____  _____ ____
					//	/ 15  /  16 /  17 /
					//	----- ----- -----
					//	 _____  _____ ____
					//	/ 12  /  13 /  14 /   <-- middle 3x3 grid
					//	----- ----- -----
					//	 _____  _____ ____
					//	/  9  /  10 /  11 /
					//	----- ----- -----
					
					//	 _____  _____ ____
					//	/  6  /  7  /  8  /
					//	----- ----- -----
					//	 _____  _____ ____
					//	/  3  /  4  /  5  /   <-- lower 3x3 grid
					//	----- ----- -----
					//	 _____  _____ ____
					//	/  0  /  1  /  2  /
					//	----- ----- -----	
						int index = 0;
						// compute index. there are 64 possible combinations of x,y,z axis locations
						index |= (x < 0)          ? 1 << 0 : 0;
						index |= (x >= sizeX)     ? 1 << 1 : 0;
						index |= (z < 0)          ? 1 << 2 : 0;
						index |= (z >= sizeZ)     ? 1 << 3 : 0;
						index |= (y < locationY)          ? 1 << 4 : 0;
						index |= (y > locationY) ? 1 << 5 : 0;
						indices[count] = index;
						
						// wrap tiles where necessary
						System.Drawing.Point p = new System.Drawing.Point (x, z);
						System.Diagnostics.Debug.Assert (z > -2 && z < sizeZ + 1);
						System.Diagnostics.Debug.Assert (x > -2 && x < sizeX + 1);
						// wrap position value for adjacents in neighboring zones
						// TODO: why am i not wrapping position for Y value?
						if (z == -1)    p.Y = sizeZ - 1; 
						if (z == sizeZ) p.Y = 0;
						if (x == -1)    p.X = sizeX - 1;
						if (x == sizeX) p.X = 0;
						
						wrappedTiles[count++] = p;
					}
			

			int[] adjacents = new int[ADJ_COUNT];  
			for (int i = 0; i < indices.Length; i++)
			{
				MapLayer[] lyrs = lookup[indices[i]];
				wrappedLayers[i] = lyrs;

				if (lyrs == null) continue; // layer was out of bounds if null
				
				if (lyrs[(int)layerType] == null) continue;
											
				System.Drawing.Point p = wrappedTiles[i];				
				// finally assign the TileType uising the appropriate layer for this adjacent tile
				adjacents[i] = lyrs[(int)layerType].GetMapValue (p.X, p.Y);
			}
						
			return adjacents ;
		}
		
		bool AllAdjacentsInBounds (int tileX, int tileZ, int sizeX, int sizeZ)
		{
			return true;
			throw new NotImplementedException ();
			//return TileInBounds (tileX, tileZ, );
		}
		
		
		bool LayerInBounds (int locationX, int locationY, int locationZ, int sizeX, int sizeY, int sizeZ)
		{
			if (locationX < 0 || locationY < 0 || locationZ < 0 || locationX >= sizeX || locationY >= sizeY || locationZ >= sizeZ)
				return false;
			
			return true;
		}
		
		int GetLayerType (string layerName)
		{
			int layerType = 0; // obstacles
            
			if (layerName == "obstacles")
				layerType = 0;
            else if (layerName == "layout")
            	layerType = 1;
            else if (layerName == "style")
            	layerType = 2;
            else throw new NotImplementedException ();
            
            return layerType;
		}
		
#region IDisposable Members
		private bool _disposed = false;
        ~MapLayerGrid()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            //note: when a node is disposed, it's reference count should be down to 0 at this point
            // because when the ref count reaches 0, there should be no more references to the node
            // unless they are held by the original caller and they shouldnt be.  
            //
            // IMPORTANT: Thus the Repository will call .Dispose() on the object and the 
            //  overriden DisposeManaged/UnmangedResoruces should call an appropriate tv....Destroy() on any TV3D resource
            // thus if there are any lingering references, they will be invalid so it is the Creator's responsibility to manually
            // increment the reference count of this object if they dont want it to be disposed.

            //But there's no way to enforce
            // that.  In normal file save/load this can't happen, but when the user manually imports/creates from a static load procedure
            // then there is no way to guarantee that so we leave those resources loaded.  There's no harm in that when we unload a scene
            // only when we want to shut down the engine completely.


            // Note: Remove() of the node is what will decremenet the reference count
         
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
        	if (IsDisposed) throw new ObjectDisposedException(GetType().Name + " is already disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion

	}
}
