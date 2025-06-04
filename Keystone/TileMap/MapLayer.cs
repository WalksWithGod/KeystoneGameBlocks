using System;
using System.Collections.Generic;
using KeyCommon.Traversal;
using Keystone.CSG;
using Keystone.Elements;
using Keystone.Types;

namespace Keystone.TileMap
{
	
	internal interface IMapLayer : IDisposable
	{
		string FilePath {get;}
		float Width {get;}         // width must be less than or equal to Structure.Width
		float Height {get;}        // height must be less than or equal to Structure.Height
		object GetValue(int x, int z);
    	void SetValue(int x, int z, object value);
    	
    	
    	void Read ();
    	void Write();
	}
			
	/// <summary>
	/// 2-Dimensional map of tile data.
	/// </summary>
	/// <remarks>
	/// 2.0 version of a tiled map structure.  Our first implementation was a rather bloated CelledRegion.cs which combined 
	/// too many different concepts into a single structure.
	/// The other problem with it was, it didn't fully exploit the strengths of a tilemap's method of storing data in 
	/// effecient arrays and instead wanted to try to store data in dictionaries as well.
	/// </remarks>
	internal class MapLayer  : IDisposable
	{
		public delegate void OnMapLayerChanged(MapLayer sender, int x, int z, int value);
		public delegate void OnMapLayerValidateChange (MapLayer sender, int x, int z, int value, out bool cancel);
		
		internal OnMapLayerChanged mOnChangedHandler;
		internal OnMapLayerValidateChange mOnValidateHandler;

		
		internal LockBitmap mTileData;   	
		private string mTileDataFilePath;
        
		string mName;
		
		// the array subscripts
		public int SubscriptX;
		public int SubscriptY;
		public int SubscriptZ;
		
		uint mTileCountX;
		uint mTileCountZ;
		
		int mFloorLevel;
		
		float mFloorWidth;
		float mFloorDepth;
		
		Vector3d mTileSize;
		
		internal MapLayer (string name, string path, int subscriptX, int subscriptY, int subscriptZ, int floorLevel, float floorWidth, float floorHeight, float floorDepth)
		{
			if (string.IsNullOrEmpty (name) || string.IsNullOrEmpty (path)) throw new ArgumentNullException ();
							
			mName = name;
			mTileDataFilePath = path;

			SubscriptX = subscriptX;
			SubscriptY = subscriptY;
			SubscriptZ = subscriptZ;

			mFloorLevel = floorLevel ;
			
			mFloorWidth = floorWidth;
			mFloorDepth = floorDepth;
			mTileSize.y = floorHeight;
		}
		

		public string Name {get {return mName; }} // eg. "terrain_layout", "terrain_style", "structure_layout" "structure_style", etc.
		
		public int FloorLevel {get {return mFloorLevel;}}
		
		public string Path { get {return mTileDataFilePath; }}
		
        /// <summary>
        /// TileStartX contains the LOCAL SPACE center position.X of the smallest position.X tile on the axis
        /// The actual left and right tile bounds then become
        /// TileStartX * offsetX +/- sizeX where +/- depending on which side of origin we want
        /// </summary>
        public float TileStartX { get; private set; }
        /// <summary>
        /// TileStartZ contains the LOCAL SPACE center position.Z of the smallest position.Y tile on the axis
        /// The actual front and back tile bounds then become
        /// TileStartZ * offsetZ +/- sizeZ where +/- depending on which side of origin we want
        /// </summary>
        public float TileStartZ { get; private set; }
        public float TileStopX { get; private set; }
        public float TileStopZ { get; private set; }
        
        /// <summary>
        /// TileSize is set after the underling bitmap data Open() call
        /// </summary>
		public Vector3d TileSize 
		{
			get {return mTileSize;}
		}
		
		
		public uint TileCountX 
		{
			get {return mTileCountX;}
		}
		
		public uint TileCountZ
		{
			get {return mTileCountZ;}
		}
		
		protected bool IsLittleEndian 
		{
			get { return BitConverter.IsLittleEndian;}
		}
		
				
		public virtual int GetMapValue(uint index)
		{
			int x, z;
			UnflattenIndex (index, out x, out z);
			return GetMapValue (x, z);
		}
		
		public virtual int GetMapValue(int x, int z)
		{
			// if out of bounds return -1
	    	if (x < 0 || x >= mTileCountX || z < 0 || z >= mTileCountZ) return -1;
	    	
			// NOTE: tile coordinates need to be remapped to pixel coordinates
			// since bitmap stores 0,0 at top left and our tiles start at bottom left
			// which is lowest x and lowest z values
			int pixelZ = z; // Math.Abs(z - (int)mTileCountZ + 1);
				    	
			Pixel color = mTileData.GetPixel (x, pixelZ);
			
			return color.ARGB;
		}
		
		public virtual void SetMapValue(uint index, int value)
		{		            
            int x, z;
            UnflattenIndex (index, out x, out z);
            SetMapValue(x, z, value);
		}
		
		public virtual void SetMapValue(int x, int z, int value)
		{
			bool cancel;
			mOnValidateHandler (this, x, z, value, out cancel);		
			if (cancel) return;
		
			Pixel color = Pixel.GetPixel(value); 
			
			// NOTE: tile coordinates need to be remapped to pixel coordinates
			// since bitmap stores 0,0 at top left and our tiles start at bottom left
			// which is lowest x and lowest z values
			int pixelZ = z; // (int)mTileCountZ - z - 1;
			
			mTileData.SetPixel (x, pixelZ, color);

			// TODO: MapGrid.NotifyChange() will occur even when the SetMapValue() is occuring due to
			//       deserialization of saved file.  We dont want to notify observers every time this happens
			//       in that case.  During deserialization or during what amounts to be a bulk Change() operation
			//       we only want to NotifyChange() after the entire operation has completed.
			//       
			Core._Core.MapGrid.NotifyChange (this, x, z, value);
			mOnChangedHandler (this, x, z, value);
		}
		
		
#region Tile Management
        internal bool TileIsInBounds(int x, int z)
        {
            if (x < 0 || z < 0 || x >= mTileCountX || 
                z >= mTileCountZ ) return false;
            return true;
        }
		
        internal bool TilesAreInBounds(int x, int z, int width, int height)
    	{
            int maxDestIndexX = x + width - 1;
            int maxDestIndexZ = z + height - 1;

            if (x < 0 || z < 0 ||
                maxDestIndexX >= mTileCountX ||
                maxDestIndexZ >= mTileCountZ )
                return false;

            return true;
        }
        
        internal bool PositionIsInBounds (Vector3d position)
        {
        	Vector3d halfWidth = mTileSize * 0.5d;
        	halfWidth.x *= mTileCountX;
        	halfWidth.z *= mTileCountZ;
        	
        	return  position.x >= -halfWidth.x && position.x <= halfWidth.x &&
                    position.z >= -halfWidth.z && position.z <= halfWidth.x;	
        }
        
		public void UnflattenIndex(uint index, out int x, out int z)
        {
			Utilities.MathHelper.UnflattenIndex (index, mTileCountX, out x, out z);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x">Row Index, not a real x coord</param>
		/// <param name="z">Column Index, not a real z coord</param>
		/// <returns></returns>
        public uint FlattenIndex (int x, int z)
        {
        	return Utilities.MathHelper.FlattenIndex (x, z, mTileCountX);
        }
		
		public Vector3i TileLocationFromPoint(Vector3d coord)
        {
            return TileLocationFromPoint(coord.x, coord.z);
        }
				
        // TODO: if floor is just vector.y where the .y is not actually the floor but
        // a point in 3d space which looks to find the floor, then this will be wrong!
        // TODO: however from now on we should really always be returning real points in 3d interior space
        //      and not using a special "floor" value that is a corruption of our entity.Translation value
        // and which is just plain old confusing.  stick to one simple real coordinate system
        // todo; i need to verify the above
        // WARNING: TileLocation is dependant on the MapLayer's resolution and different layers
        //          have different resolutions.   So a "TileLocation" is not a fixed thing like a
        //          cartesian coordinate.
        public Vector3i TileLocationFromPoint(double x, double z)
        {
            Vector3i tileLocation;
            tileLocation.Y = 0;
            TileLocationFromPoint (x, z, out tileLocation.X, out tileLocation.Z);

            return tileLocation;
        }

        public void TileLocationFromPoint(double positionX, double positionZ, out int tileX, out int tileZ)
        {
            // NOTE: no "reversal" of Z is needed because our cells are not stored in an image, but in an array
            tileX = (int)(((mTileSize.x * TileCountX / 2f) + positionX) / mTileSize.x);
            if (tileX >= TileCountX) 
            	tileX = (int)TileCountX - 1;
            tileZ = (int)(((mTileSize.z * TileCountZ / 2f) + positionZ) / mTileSize.z);
            if (tileZ >= TileCountZ) 
            	tileZ = (int)TileCountZ - 1;
            
        #if DEBUG 
            // verify MapImpactPointToTileCoordinate() computes same result
            int x, z;
           
            Keystone.Celestial.ProceduralHelper.MapImpactPointToTileCoordinate(
                (float)TileSize.x * TileCountX,
                (float)TileSize.z * TileCountZ,
                TileCountX,
                TileCountZ,
                (float)positionX, (float)positionZ,
                out x, out z);
            
            System.Diagnostics.Debug.Assert (tileX == x && tileZ == z, "MapLayer.TileLocationFromPoint() - Tile locations do not match.");
        #endif
        }
        
        
		public Vector3d PointFromTileLocation (int x, int y, int z)
		{
			Vector3d result;
			// the found node coordinates are 0 based array indices.  convert them to region space coordinates
			double tileCenterX = (TileStartX * mTileSize.x) + (x * mTileSize.x);
            double tileCenterZ = (TileStartZ * mTileSize.z) + (z * mTileSize.z);
                
			result.x = tileCenterX;
			double floorHeight = this.mFloorLevel * mTileSize.y;
			result.y = floorHeight; 
			result.z = tileCenterZ;
			
			return result;
		}
		
		/// <summary>
        /// generates the flood filled rectangular list of cell indices
        /// given a start and end tile index
        /// </summary>
        /// <param name="startCellIndex"></param>
        /// <param name="endCellIndex"></param>
        /// <returns></returns>
        public uint[] GetTileList(uint startTileIndex, uint endTileIndex)
        {
            // I think these should be generated dynamically 
            int startX, startZ, stopX, stopZ;
            UnflattenIndex(startTileIndex, out startX, out startZ);
            UnflattenIndex(endTileIndex, out stopX, out stopZ);

            //System.Diagnostics.Debug.WriteLine ("TileX Start = " + startX.ToString() + "   TileZ Start = " + startZ.ToString());
            // recall the unflattened stop/x/y/z and start/x/y/z are 0 based on TileCount's 
            // not cell x,y,z coordinates
            if (TileIsInBounds(startX, startZ) && TileIsInBounds(stopX, stopZ))
            {
                if (startTileIndex == endTileIndex) return new uint[] { startTileIndex };

                // reverse the order so that start indices are always lower than the end indices
                if (startX > stopX)
                {
                    Utilities.MathHelper.Swap (ref startX, ref stopX);
                }
                if (startZ > stopZ)
                {
                    Utilities.MathHelper.Swap (ref startZ, ref stopZ);
                }

                uint[] result = new uint[(stopX - startX + 1) * (stopZ - startZ + 1)];
                uint count = 0;
                for (int i = startX; i <= stopX; i++)
                    for (int j = startZ; j <= stopZ; j++)
                    {
                        result[count] = FlattenIndex(i, j);
                        count++;
                    }

                return result;
            }
 
            return null;
        }
        
        

        internal int GetVertexID(uint tileIndex, uint tileCorner)
        {
            uint vertexCountX = TileCountX + 1;
            uint vertexCountZ = TileCountZ + 1;
            uint tilesPerDeck = TileCountX * TileCountZ;
            uint verticesPerDeck = vertexCountX * vertexCountZ;
            uint currentDeck = tileIndex / tilesPerDeck;
            uint deckSpaceCellID = tileIndex - (tilesPerDeck * currentDeck);
            uint vertexID = deckSpaceCellID + (verticesPerDeck * currentDeck); // (cellIndex + 1) / CellCountX + cellIndex;
            uint cellRow = deckSpaceCellID / TileCountX;


            //if (cellCorner == 0) // do nothing
            if (tileCorner == 1 ||tileCorner == 2)
            {
                vertexID += TileCountX + tileCorner;
        
            }
            else if (tileCorner == 3)
                vertexID++;

           
            return (int)(vertexID + cellRow );
        }

        private int FindClosestEdge(Line3d[] edges, Vector3d intersectionPoint)
        {
            double distance = double.MaxValue;
            int closestEdgeID = -1;
            Vector3d closestPoint;
            if (edges.Length > 0)
            {
                for (int j = 0; j < edges.Length; j++)
                {
                    Vector3d point, o, d;
                    o = edges[j].Point[0];
                    d = edges[j].Point[1];
                    
                    double dist = Line3d.DistanceSquared(o, d,
                                                         intersectionPoint, out point);
                    if (dist < distance)
                    {
                        distance = dist;
                        closestPoint = point;
                        closestEdgeID = j;
                    }
                }
            }
            //// the below i beleive is actually finding the cloest endpoint on the cloesest edge
            //// the closestPoint is not necessarily an end point of this edge
            //// it's just the closest perpendicular to the line
            //// Actually, even this is not true.  The closest vertex is not even necessarily on this edge!
            //// finding the nearest edge to the impact point does not necessarily mean that the closest 
            //// vertex also lies on that closest edge.  In the case of a triangle that is very wide at it's base but with a short
            //// peak at center top, you could have an impact point closest to the bottom edge but with closest vertex at peak.
            //// So the following code is incorrect really... What we really need is a new routine to find closest vert
            //// were we test dist and dist2 and then get the intersectionPoint and then compare those in the above closest edge
            //// search loop.  To avoid having to do these loops twice, keep in mind that typically you're in edge or vert selection mode
            //// so you'd only do one or the other.  Verify in sketchup how that works
            ////result.iNearestVertex =;
            //double dist1 = Vector3d.GetDistance3dSquared(origin, intersectionPoint);
            //double dist2 = Vector3d.GetDistance3dSquared(dest, intersectionPoint);
            //if (dist1 < dist2)
            //{
            //    coord = origin;
            // //   result.VertexIndex = (int)edges[iclosestEdge].Origin.ID;
            //}
            //else
            //{
            //    coord = dest;
            // //   result.VertexIndex = (int)edges[iclosestEdge].Destination.ID;
            //}

            return closestEdgeID;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelSpaceRay"></param>
        /// <param name="parameters">
        /// The Ray passed in MUST be in the model space of this level.  Typically this is done by
        /// having Structure.Collide() translate the resulting ray by the mTileSize.Y
        /// </param>
        /// <returns></returns>
        public Keystone.Collision.PickResults Collide(Keystone.Types.Ray modelSpaceRay, KeyCommon.Traversal.PickParameters parameters)
        {
            // if the selected floor is picked, then we can deduce which
            // tile was picked instead of iterating through every tile on the floor
            // like we used to do in the old version of this function.
            Keystone.Collision.PickResults pickResult = new Keystone.Collision.PickResults();
            pickResult.HasCollided = false;
            
			const double RAY_SCALE = 10000000; // picking an interior assumes camera will be relatively close to it and that the interior is not "huge"

            Vector3d halfDimension = mTileSize * .5;

            // NOTE: we only do these collisions in 2D space so disregard y for both halfDimension and center cell y
			const double ZERO = 0;
			halfDimension.y = 0; 


			// create points for quad positioned at origin for our lookup values
            Vector3d[] floorPoints = new Vector3d[4];


            // NOTE: Default DirectX winding order is CLOCKWISE vertices for
            // front facing.  XNA also uses clockwise for front facing.
            // based on current cell, compute the polypoints of the cell
            // THUS 
            // 1 ___ 2
            // |    |
            // 0 ___ 3
            // is our layout
            floorPoints[0].x = TileStartX * mTileSize.x;
            floorPoints[0].y = ZERO;
            floorPoints[0].z = TileStartZ * mTileSize.z;
            // -halfDimension will translate to the bottom corner 
            // of the cell (ie vertex 0 of the floor face)
            floorPoints[0] -= halfDimension;


            floorPoints[1].x = floorPoints[0].x;
            floorPoints[1].y = floorPoints[0].y;
            floorPoints[1].z = floorPoints[0].z + (mTileSize.z * TileCountZ);

            floorPoints[2].x = floorPoints[0].x + (mTileSize.x * TileCountX);
            floorPoints[2].y = floorPoints[0].y;
            floorPoints[2].z = floorPoints[1].z;

            floorPoints[3].x = floorPoints[2].x;
            floorPoints[3].y = floorPoints[0].y;
            floorPoints[3].z = floorPoints[0].z;

            // polygon.Intersect because it provides an intersectionPoint
            // which ineed to determine closest vertex and closest edge
            Vector3d intersectionPoint;
            bool hit = Polygon.Intersects(modelSpaceRay, RAY_SCALE, floorPoints, false, out intersectionPoint);
            // TODO: the resulting intersectionPoint might be slightly out of bounds... if it is
            //       we should override the hit = true and set it to false instead
            
            if (hit && PositionIsInBounds(intersectionPoint))
            {
                //System.Diagnostics.Debug.WriteLine(string.Format("{0}, {1}, {2} impact point.", intersectionPoint.x, intersectionPoint.y, intersectionPoint.z));

                // all collisions are done in ModelSpace.
                if ((parameters.Accuracy & PickAccuracy.Tile) == PickAccuracy.Tile || (parameters.Accuracy & PickAccuracy.Face) == PickAccuracy.Face)
                {
                    pickResult.HasCollided = true;     
                    pickResult.CollidedObjectType = parameters.Accuracy;

                    int x, y, z;
                    // convert the impact point to i, k, 0 based indices
                    this.TileLocationFromPoint (intersectionPoint.x, intersectionPoint.z, out x, out z);
                                        
                    // if tile x, y, z is out of bounds of max possible interior dimensions this should NOT report a hit
                    // this is the only filter we do here, rest is responsibility of Picker.cs
                    if (TileIsInBounds(x, z) == false)
                        return pickResult;

                    // here the polygon is hit, but if this tile's segment type is TT_EMPTY then no collision occurs
                    Pixel pixel = Pixel.GetPixel (GetMapValue (x, z));
                    byte segmentIndex = (byte)TileTypes.TT_EMPTY; // TODO: segmentIndex types should be in search parameters
                    if (pixel.B == segmentIndex)
                    {
                    	pickResult.HasCollided = false;
                    	return pickResult;
                    }

                    // flatten the x, z to a tileIndex so we can store this in our PickResults
                    uint flattenedTileIndex = FlattenIndex(x, z); 
					pickResult.FaceID = (int)flattenedTileIndex;
					
                    Vector3i tileLocation;
                    tileLocation.X = (int)x;
                    tileLocation.Y = int.MinValue; // caller must set this, MapLayer only does 2D 
                    tileLocation.Z = (int)z;
                    pickResult.TileLocation = tileLocation;
#if DEBUG
                    int testX, testZ;
		            // verify unflatted produces same results
                    UnflattenIndex(flattenedTileIndex, out testX, out testZ);
                    // note: k does in fact == z 
                    // likewise i == y and j == x
                    System.Diagnostics.Debug.Assert(x == testX && z == testZ);
#endif
                 
                    // get the polypoints of this tile
                    Vector3d[] polyPoints = new Vector3d[4]; // four because we want to be able to pick any part of quad, not just triangle
                    float tileCenterX = (TileStartX * (float)mTileSize.x) + (x * (float)mTileSize.x);
                    float tileCenterZ = (TileStartZ * (float)mTileSize.z) + (z * (float)mTileSize.z);

                    // NOTE: Default DirectX winding order is CLOCKWISE vertices for
                    // front facing.  XNA also uses clockwise for front facing.
                    // based on current cell, compute the polypoints of the cell
                    // THUS 
                    // 1 ___ 2
                    // |    |
                    // 0 ___ 3
                    // is our layout
                    polyPoints[0].x = tileCenterX;
                    polyPoints[0].y = ZERO;
                    polyPoints[0].z = tileCenterZ;
                    // -halfDimension will translate to the bottom corner 
                    // of the cell (ie vertex 0 of the floor face)
                    polyPoints[0] -= halfDimension;

                    polyPoints[1].x = polyPoints[0].x;
                    polyPoints[1].y = polyPoints[0].y;
                    polyPoints[1].z = polyPoints[0].z + mTileSize.z;
                    // NOTE: no need to add offset since we're using polyPoints[0]'s values
                    // which already takes those into account
                    //polyPoints[1] += mCellOriginOffset;

                    polyPoints[2].x = polyPoints[0].x + mTileSize.x;
                    polyPoints[2].y = polyPoints[0].y;
                    polyPoints[2].z = polyPoints[1].z;
                    // NOTE: no need to add offset since we're using 0 and 1's values
                    // which already takes those into account
                    //polyPoints[2] += mCellOriginOffset;

                    polyPoints[3].x = polyPoints[2].x;
                    polyPoints[3].y = polyPoints[0].y;
                    polyPoints[3].z = polyPoints[0].z;
                    // NOTE: no need to use offset since we're using 0 and 2's values
                    // which already takes those into account
                    //polyPoints[3] += mCellOriginOffset;

                    // use the impact point to find the closest corner
                    uint closestCornerIndex = 0;
                    double dist = double.MaxValue;
                    for (uint n = 0; n < polyPoints.Length; n++)
                    {
                        //
                        double newDist = Vector3d.GetDistance3dSquared(intersectionPoint, polyPoints[n]);
                        if (newDist < dist)
                        {
                            closestCornerIndex = n;
                            dist = newDist;
                        }
                    }

                    pickResult.TileVertexIndex = (int)closestCornerIndex;
                    pickResult.VertexID = GetVertexID(flattenedTileIndex, closestCornerIndex);

                    // compute the closest edge
                    // TODO: if nearest the center, then it's center edge
                    // NOTE: there is only one center edge and it has one
                    // of two orientations. This is elegant as it allows us
                    // still 1:1 edges everywhere and not some special case
                    // couple that both travel through center of the tile.
                    // either corner 0 to 2 or 1 to 3 (lowest corner id always
                    // first)
                    Line3d[] edges = new Line3d[4];
                    edges[0] = new Line3d(polyPoints[0], polyPoints[1]);
                    edges[1] = new Line3d(polyPoints[1], polyPoints[2]);
                    edges[2] = new Line3d(polyPoints[2], polyPoints[3]);
                    edges[3] = new Line3d(polyPoints[3], polyPoints[0]);

                    int edgeIndex = FindClosestEdge(edges, intersectionPoint);
                    
                    TileEdge.EdgeOrientation orientation;
                    orientation = edgeIndex == 0 || edgeIndex == 2 ? TileEdge.EdgeOrientation.Vertical : TileEdge.EdgeOrientation.Horizontal ;
                    
                    uint originID, destID;
                    switch (edgeIndex)
                    {
                        case 0:
                            originID = (uint)GetVertexID(flattenedTileIndex, 0);
                            destID = (uint)GetVertexID(flattenedTileIndex, 1);
                            break;
                        case 1:
                            originID = (uint)GetVertexID(flattenedTileIndex, 1);
                            destID = (uint)GetVertexID(flattenedTileIndex, 2);
                            break;
                        case 2:
                            originID = (uint)GetVertexID(flattenedTileIndex, 2);
                            destID = (uint)GetVertexID(flattenedTileIndex, 3);
                            break;
                        default:
                            originID = (uint)GetVertexID(flattenedTileIndex, 3);
                            destID = (uint)GetVertexID(flattenedTileIndex, 0);
                            break;
                    }

                    if (destID < originID)
                    {
                        uint t = destID;
                        destID = originID;
                        originID = t;
                    }
                    
                    TileEdge tmp = TileEdge.CreateTileEdge(originID, destID, mTileCountX, TileCountZ, orientation);
                    pickResult.EdgeID = (int)tmp.ID;
                    pickResult.EdgeOrigin = edges[edgeIndex].Point[0];
                    pickResult.EdgeDest = edges[edgeIndex].Point[1];
                    pickResult.FacePoints = polyPoints;

                    // since the collision is done in local space, the intersectionPoint should be already also
                    pickResult.ImpactPointLocalSpace = intersectionPoint;
                    pickResult.ImpactNormal = -modelSpaceRay.Direction;

                    // BEGIN - Perform 3D Pass to resolve wall vs floor collision
                    // - is there a wall on any of the 4 edges?  if not, set EdgeID = -1
                    // TODO: this method of picking will not fix the problem unless the camera angle is 
                    // fixed at a relatively steep angle.  The closer to deck horizon the camera gets,
                    // the more walls we can pick before every intersecting the floor itself with our pick ray.
                    List<Model> models = new List<Model>();
                    for (int i = 0; i < 4; i++)
                    {
                    //	  originID = (uint)GetVertexID(flattenedCellArrayIndex, i);
                    //    int n =  i < 3 ? i + 1 : 0;
                    //    destID = (uint)GetVertexID(flattenedCellArrayIndex, n);	
                    //    orientation = i == 0 || i == 2 ? CellEdge.EdgeOrientation.Vertical : CellEdge.EdgeOrientation.Horizontal ;
                    // 
                    //	  tmp = CellEdge.CreateEdge(originID, destID, CellCountX, CellCountY, CellCountZ, orientation);
                    //    
                    //	  MinimeshMap existingMap;
                    //	  if (mEdgeModelMap.TryGetValue(edgeID, out existingMap))
                    //    {
                    //			// recall mEdgeModelMap is MinimeshMap instance and contains references to Models and Geometry
                    //			// whereas Segment only contains a SegmentStyle that has resourceIDs only, no object refs.  Here
                    //          // either can be used to determine if a wall is at that location
                    //			
                    //			// add to list of models in pickResult so that Picker.cs traverser can perform 
                    //			if (existingMap.BottomLeftModel != null)
                    //				models.Add (existingMap.BottomLeftModel);
                    //			if (existingMap.TopRightModel != null)
                    //				models.Add (existingMap.TopRightModel);
                    //			
                	//	  }
                    }
                    //
                   	// - our HUD can add a second instance of the renderable to the pipeline making this one x% larger and
                   	//   rendered before the scene, OR we can modify the Material to make the selected wall brighter
                   	// 		- TODO: good time to make a "Selected" HUD material changer for our SelectionTool
                   	//  - http://www.gamedev.net/topic/280596-how-to-render-highlight-model-with-edges-on-it/
                   	// - http://rbwhitaker.wikidot.com/toon-shader
                   	// - http://gamedev.stackexchange.com/questions/34652/outline-object-effect
                   	// - http://www.flipcode.com/archives/Object_Outlining.shtml <-- uses stencil buffer
                   	// http://www.codeproject.com/Articles/94817/Pixel-Shader-for-Edge-Detection-and-Cartoon-Effect  - sobel filter
                   	// - http://www.gamedev.net/topic/524102-d3d10-single-pass-outline-rendering/#entry4402201 - single pass shader option using inner lines
                    // END - Perform 3D Pass 
                }

                //System.Diagnostics.Debug.WriteLine ("MapLayer.Collide() - Pick Distance: "+ _lastPickResult.DistanceSquared.ToString ());
                //System.Diagnostics.Debug.WriteLine("MapLayer.Collide() - Closest EdgeID: " + tmp.ID.ToString());
                //System.Diagnostics.Debug.WriteLine("MapLayer.Collide() - Face: " + pickResult.FaceID.ToString());                
            }
            
            return pickResult;
        }
		#endregion

					
#region Map Data IO
		internal void Initialize ()
		{
			if (string.IsNullOrEmpty (mTileDataFilePath) || System.IO.File.Exists (mTileDataFilePath) == false)
				throw new System.IO.FileNotFoundException("MapLayer.Initialize() - '" + mTileDataFilePath + "' not found.");
			
            mTileData = new LockBitmap  (mTileDataFilePath);
            
            
            mTileCountX = (uint)mTileData.Width;
            mTileCountZ = (uint)mTileData.Height;
            
            // each MapLayer can have a tile size that is independant of other MapLayers within that Structure
            mTileSize.x = mFloorWidth  / (float)mTileCountX;
            mTileSize.z = mFloorDepth / (float)mTileCountZ;
            
            // initialize start/stop
            float start, stop;
            // Start and Stop's contain the LOCAL SPACE center position offsets for the start and end cells
            // on those respective axis.  The actual cell bounds then become
            // StartX * offsetX +/- sizeX where +/- depending on which side of origin we want
            // see Mesh3d.CreateCellGrid() for a version that calcs cell boundaries
            Portals.ZoneRoot.GetStartStop(out start, out stop, TileCountX);
            TileStartX = start;
            TileStopX = stop;

            Portals.ZoneRoot.GetStartStop(out start, out stop, TileCountZ);
            TileStartZ = start;
            TileStopZ = stop;
                        
            mHashCodeDirty = true;
		}
		
		internal void Read ()	
		{
			if (mTileData == null || mTileData.Bitmap == null) 
				throw new System.Exception("MapLayer.Read() - '" + mTileDataFilePath + "' Map Layer is not Open.  Call .Open() first.");;

			
            System.Diagnostics.Debug.Assert (mTileCountX == mTileData.Width && mTileCountZ == mTileData.Height);
			// iterate through the bitmap using TILE coordinates.
			// the call to GetMapValue() will convert to proper bitmap value
			// invoke mOnChangedHandler as if we were painting data
            // in real time rather than deserializing
            for (int x = 0; x < mTileCountX; x++)
            	for (int z = 0; z < mTileCountZ; z++)
            	{
	            	int argb = GetMapValue (x, z);
                	mOnChangedHandler (this,  x, z, argb);
            	}
		}
		
	
		internal void Write()
        {
			if (string.IsNullOrEmpty(mTileDataFilePath)) throw new Exception();
			if (mTileData == null || mTileData.Bitmap == null) throw new Exception ();

            mTileData.Save();
            
        }

#endregion


		const int BITS_PER_INT32 = 32;
		int mHashCode;
		bool mHashCodeDirty = true;
		
        public override int GetHashCode()
        {
        	if (!mHashCodeDirty) return mHashCode;

    		mHashCodeDirty = false;
        
			// http://stackoverflow.com/questions/6832139/gethashcode-from-booleans-only
            unchecked // unchecked will skip overflow check
            {
                mHashCode = 17; // start with prime
				const int prime_multiplier = 23;

                for (uint i = 0; i < mTileCountX; i++)
                    for (uint k = 0; k < mTileCountZ; k++)
                    {
                        int elementHashValue = 0;
                        int data = mTileData.GetPixel((int)i, (int)k).ARGB;
                        
                        // test all 32 bits in the int
                        int bit = 0;  
                        for (int n = 0; n < BITS_PER_INT32; n++)
                        {
                            if ((data & bit) == bit)
                            {
                                elementHashValue = 1;
                                mHashCode = mHashCode * prime_multiplier + elementHashValue;
                            }
                            bit = 1 << n; 
                        }
                    }
            }
            return mHashCode;
        }
        
	    #region IDisposable
	    public void Dispose()
	    {
	    	if (mTileData == null) return;
	    	
	    	mTileData.Dispose();
	    	mTileData = null;
	    		
	    }
	    #endregion
	}
}
