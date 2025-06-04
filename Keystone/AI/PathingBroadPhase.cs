	#region Attribution
	//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
	//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
	//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
	//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
	//  REMAINS UNCHANGED.
	//
	//  Email:  gustavo_franco@hotmail.com
	//
	//  Copyright (C) 2006 Franco, Gustavo 
	// 
	//  Modified by Hypnotron for Broadphase Pathing using Connectivity Graph - Dec.2014
	#endregion

//#define DEBUGON
	
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Keystone.AI
{
	
    #region Delegates
    internal delegate void BroadPhasePathFinderDebugHandler(int fromX, int fromY, int x, int y, int z, PathFinderNodeType type, int totalCost, int cost);
    #endregion
    

	// FOR AI PATHING SEE 
	//  E:\dev\_projects\_AI\PathFinder_source
	// AND
	//	Keystone.Portals.CellMap
	//			DoLinkSearch
	//			FindLinks
	//			BreadthFirstSearch
	

    [StructLayout(LayoutKind.Sequential, Pack=1)] 
    internal struct BroadPhasePathFinderNodeFast
    {
		// NOTE: this struct is only used during Runtime path finding in the mCalcGrid.  The original obstacle weight is 
		// stored as a single int32 in the mDataGrid
        public int     FitnessWeight;  // (f) accumulated cost + estimated cost (heuristic is added cost based on distance of this node to the goal).  This value is used by Compare() method.
        public int     Weight;         // (g) accumulated cost to reach this node beginning at starting node
        // public int H;               // (h) estimated cost
        public int  GraphID; 
        public int ID;          // Portal ID
        public Keystone.Types.Vector3i Location;    // Portal Location  <-- for portal traversal, X,Y,Z is only important for the calcgrid to compare distances

        public int  PGraphID; 
        public int PID;
        public Keystone.Types.Vector3i  Previous;         // location of Portal previously traversed
        public TraversalState    Status; // node has either 0 not evaluated, 1 open evaluated, 2 closed evaluated status.  
    }
	  
	internal struct BroadPhasePathFindResults
	{
		public List<BroadPhasePathFinderNodeFast>  FoundNodes;
		public bool                                Found;
		public bool                                SearchCountExceeded;
		public long                                CompletedTime;
        public bool                                DebugProgress;  // show path finding process
        public bool                                DebugFoundPath; // draw found path
	}
	
	
	/// <summary>
	/// A* Path Finding.
	/// It is also worth mentioning that while the path will go around obstacles they will not stop it entirely.
	/// If we leave no other option, or if going around an obstacle is even more expensive than crossing it, A* will
	/// create a path through high cost terrain.
	/// 
	/// If we want to add strictly impassable terrain we will need to prevent A* from considering that node entirely. 
	/// Depending on our game it might also be helpful to stop our path finder if it cannot find a path less than 
	/// some maximum cost. This would both prevent the path finder from running for an extended period of time and 
	/// from returning incredibly long or expensive paths.
	/// http://theory.stanford.edu/~amitp/GameProgramming/MapRepresentations.html
	/// </summary>
	internal class PathingBroadPhase
	{
   	
        #region Inner Classes
        internal class CompareBroadPhasePFNodeMatrix : IComparer<PortalIdentifier>
        {
        	BroadPhasePathFinderNodeFast[][] mMatrix;

            // initialize with entire matrix
            public CompareBroadPhasePFNodeMatrix(BroadPhasePathFinderNodeFast[][] matrix)
            {
                mMatrix = matrix;
            }

            // compare two different matrix elements at index a and index b
            public int Compare(PortalIdentifier a, PortalIdentifier b)
            {
            	if (mMatrix[a.GraphID][a.PortalID].FitnessWeight > mMatrix[b.GraphID][b.PortalID].FitnessWeight)
                    return 1;
            	else if (mMatrix[a.GraphID][a.PortalID].FitnessWeight < mMatrix[b.GraphID][b.PortalID].FitnessWeight)
                    return -1;
                return 0;
            }
        }
        
        internal class PortalIdentifier 
        {
        	public int GraphID;
        	public int PortalID;
        	public int AreaID;
        }
        #endregion
        
		#region Events
    	public event BroadPhasePathFinderDebugHandler PathFinderDebug;
    	#endregion
       
        private AI.Connectivity.Graph[]      mGraphs                 = null;
        private int                          mGridSizeX              = 0;
        private int                          mGridSizeY              = 0;
        private int                          mGridSizeZ              = 0;
                


        // TODO: i think this grid should become an array of 2D arrays so that we have an array by altitude of
        //       2D x/z plane collision data layers.
        //       - the reason is, we want to reference the 2D array without 
        public PathingBroadPhase (AI.Connectivity.Graph[] graphs, int gridSizeX, int gridSizeY, int gridSizeZ)
        {
        	
        	mGraphs = graphs;
        	
        	mGridSizeX = gridSizeX; 
			mGridSizeY = gridSizeY; 
			mGridSizeZ = gridSizeZ; 
        }
        
        // thread safe - the path finder instance is attached to a paged-in Zone however
        // in order to be thread safe, we cannot share mCalcGrid.  But it should be no real problem to
        // instance it every call.  
        // - also, if multiple calls to Find() within same Zone occurs such that multiple threads are entering 
        // how do we ensure that the underlying grid cannot change inbetween these calls?  Further, the time it takes for
        // the entity to execute the path, the map must not change then either.  Perhaps a trigger can fire if a tile changes
        // such that it blocks a path for an NPC for outstanding found paths not yet completed.. then an option to recalc can be done.
        // The event perhaps gives user option to re-send that unit to a different area instead.
		  // http://www.youtube.com/watch?v=ajdzxsL_NIE <- Dynamic Path Finding in Kynapse AI
		  // http://www.youtube.com/watch?v=nGC_kBCoHYc <-- dynamic 500ms real-time A* pathing with smoothing
		  // http://www.youtube.com/watch?v=a3sdyw6kGI0  <-- simulated city life demo withno prescripted events
		public BroadPhasePathFindResults Find(Keystone.AI.Connectivity.Graph startGraph, Keystone.AI.Connectivity.Graph endGraph, 
		                                      Keystone.Types.Vector3i start, Keystone.Types.Vector3i end, PathFindParameters parameters)
		{
			// NOTE: the A* assumes that the areas and portals were based on available movement
			// type of the unit.  Otherwise if the CG is generated for units that CAN jump across
			// 1 tile wide gaps but the unit we A* for against that CG cannot, that unit will 
			// fail to jump when required and the pathing will break
		  	BroadPhasePathFinderNodeFast[][]         calcGrid            = null;
			PriorityQueueB<PortalIdentifier>       openQueue           = null;
			List<BroadPhasePathFinderNodeFast>     closeList           = null;
			int                          closeNodeCounter    = 0;
			
			int                          heuristic           = 0;
		    int                          xAxisChangeAmount   = 0;
		    int                          yAxisChangeAmount   = 0;
			int                          newG                = 0;
			

			int                          endLocationID       = 0;
			
			Types.Vector3i               relativeZoneOffset;
			Types.Vector3i               currentPortalLocation = start;
	        Types.Vector3i 				 adjacentTilePosition;

			bool                         stop                = false;
			
			int                          TILES_PER_SIDE = 16; // todo: should this now be 16?
			const int                    PRIMER         = -1;
			int                          currentPortalID     = PRIMER;
						
			calcGrid = new BroadPhasePathFinderNodeFast[mGridSizeX * mGridSizeY * mGridSizeZ][];
			openQueue   = new PriorityQueueB<PortalIdentifier>(new CompareBroadPhasePFNodeMatrix(calcGrid));
			closeList   = new List<BroadPhasePathFinderNodeFast>();
		    //openQueue.Clear();
		    //closeList.Clear();
		    
			BroadPhasePathFindResults results;
			results.SearchCountExceeded = false;
			results.FoundNodes = null;
			results.CompletedTime = -1;
			results.DebugFoundPath = false;
			results.DebugProgress = false;
			results.Found = false;
		
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		    stopWatch.Start();
		
		    #if DEBUGON
		    if (mDebugProgress && PathFinderDebug != null)
		        PathFinderDebug (0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
		    if (mDebugProgress && PathFinderDebug != null)
		        PathFinderDebug (0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
		    #endif
		
		
		    // initialize calcGrid[][] jagged array
		    for (int x = 0; x < mGridSizeX; x++) 
		    	for (int y = 0; y < mGridSizeY; y++)
		    		for (int z = 0; z < mGridSizeZ; z++)
		    		{
		    			int index = Flatten (x, y, z);
		    			int count = mGraphs[index].Portals != null ? mGraphs[index].Portals.Length : 0;
		    			calcGrid[index] = new BroadPhasePathFinderNodeFast[count];
		    			
		    			for (int j = 0; j < count; j++)
		    				calcGrid[index][j].Weight = 1; // initialize to 1
		    		}
		    		   
			
			try 
			{
			    AI.Connectivity.Graph currentGraph = startGraph;
		    	int currentGraphID = currentGraph.RegionID;
		    	int endGraphID = endGraph.RegionID; 
		    
				Keystone.Types.Vector3i startZoneOffset;
				Unflatten (currentGraphID, out startZoneOffset.X, out startZoneOffset.Y, out startZoneOffset.Z);
				Keystone.Types.Vector3i endZoneOffset;
				Unflatten (endGraphID, out endZoneOffset.X, out endZoneOffset.Y, out endZoneOffset.Z);
				
				Keystone.Types.Vector3i relativeEnd = TILES_PER_SIDE * (endZoneOffset - startZoneOffset) + end;
			
		        Keystone.AI.Connectivity.Area currentArea = currentGraph.Areas[startGraph.LocateAreaContainingTile(start.X, start.Y, start.Z)];
		        Keystone.AI.Connectivity.Area endArea = endGraph.Areas[endGraph.LocateAreaContainingTile(end.X, end.Y, end.Z)];
		
		        int startAreaID = currentArea.ID;
		        int endAreaID = endArea.ID;
		        int currentAreaID = startAreaID;	
		        	
		        int[] portals = currentArea.Portals;
		        int portalCount = portals == null ? 0 : portals.Length;
		        
		        // prime the queue with an non existant portal in the center of our starting area.  
		        PortalIdentifier ident = new PathingBroadPhase.PortalIdentifier();
	            ident.PortalID = currentPortalID;
	            ident.GraphID = startGraph.RegionID;
	            ident.AreaID = startAreaID;
	        	openQueue.Push (ident);
		            
	        	
		        while(openQueue.Count > 0 && !stop)
		        {
		        	ident = openQueue.Pop();
		        	currentGraphID = ident.GraphID;
		            currentAreaID = ident.AreaID;
		            currentPortalID = ident.PortalID;
		            		            
		            currentGraph = mGraphs[currentGraphID];
		            currentArea = currentGraph.Areas[currentAreaID];
		            
		            //Is it in closed list? means this node was already processed
		            if (currentPortalID != PRIMER)
			            if (calcGrid[currentGraphID][currentPortalID].Status == TraversalState.Closed)
			            {
			            	continue;
			            }	           	
		
		    		Keystone.Types.Vector3i currentZoneOffset;
		    		Unflatten (currentGraphID, out currentZoneOffset.X, out currentZoneOffset.Y, out currentZoneOffset.Z);
		    		relativeZoneOffset = currentZoneOffset - startZoneOffset;
		
		            if (currentPortalID != PRIMER)
		            {
			            int subscript = FindPortalDirectionSubscript(currentArea, currentGraph.Portals[currentPortalID]);
			            // unflatten tile location (TILES_PER_SIDE x TILES_PER_SIDE grid size) of this portal to X,Y coordinates
			            currentPortalLocation = TILES_PER_SIDE * relativeZoneOffset + currentGraph.Portals[currentPortalID].Location[subscript];
			            
			            // because the priority queue has us dequeing the best scoring path so far
			            // if we are at the desired location, then we know we can abort further tests now
			            // we've reached the end location.  exit the while loop
			            if (currentGraphID == endGraphID && currentAreaID == endAreaID)
			            {
			                endLocationID = currentPortalID;
			                calcGrid[currentGraphID][endLocationID].Status = TraversalState.Found;
			                results.Found = true;
			                //System.Diagnostics.Debug.WriteLine ("PATHING: Found tile {0},{1},{2}.", currentX, currentY, currentZ);
			                break;
			            }
			
			            if (closeNodeCounter > parameters.SearchLimit)
			            {
			            	System.Diagnostics.Debug.Assert (results.Found == false);
			            	results.SearchCountExceeded = true;
			                results.CompletedTime = stopWatch.ElapsedTicks;
			                System.Diagnostics.Debug.WriteLine (string.Format("PATHING: Search LIMIT {0} reached.", parameters.SearchLimit));
			                return results;
			            }
			
			            if (parameters.PunishHorizontalChangeInDirection)
			                xAxisChangeAmount = (currentPortalLocation.X - calcGrid[currentGraphID][currentPortalID].Previous.X); 
			            
			            if (parameters.PunishVerticalChangeInDirection)
			                yAxisChangeAmount = (currentPortalLocation.Y - calcGrid[currentGraphID][currentPortalID].Previous.Y);
		            }
		            
		            portals = currentArea.Portals;	
		            portalCount = portals == null ? 0 : portals.Length;	            
		            	
		            // calculate traversal cost to each portal (Not to each portal's destination areas which is incorrect way to think about problem!)
		            //http://csharpexamples.com/fast-image-processing-c/
		            // "parallel-for" loop opportunity here?
		            for (int i = 0; i < portalCount; i++)
		            {   
		            	int portalID = portals[i];
		            	int destPortalID = portalID;
		            	
		            	int subscript = 1;
		            	int destGraphID = currentGraph.Portals[portalID].RegionID[subscript];
		            	int destAreaID = currentGraph.Portals[portalID].AreaID[subscript]; 	 
		            	// NOTE: even if we've crossed a zone boundary, the tile position at Location[subscript]
		            	// is because Area and Portal creation take zone boundaries into account
		            	adjacentTilePosition = currentGraph.Portals[portalID].Location[subscript];

		            	
		                // skip this portal if both conditions are true:
		                // A) subscript[1] is where are right now and subscript[0] is where we were before that.
		                // B) subscript[0] is where we are right now and subscript[1] is where we were before that 
		                // If A) fails but B) passes, we can switch to using subscript[0]
		                if (currentGraphID == destGraphID && currentAreaID == destAreaID)
		                {
//	                    	if (currentGraphID == currentGraph.Portals[portalID].RegionID[0] && currentAreaID == currentGraph.Portals[portalID].AreaID[0])
//	                    		continue;
//	                    	else 
//	                    	{
	            			subscript = 0;
	                		destGraphID = currentGraph.Portals[portalID].RegionID[subscript];
	            			destAreaID = currentGraph.Portals[portalID].AreaID[subscript]; 
			            	// NOTE: even if we've crossed a zone boundary, the tile position at Location[subscript]
		            		// is because Area and Portal creation take zone boundaries into account
	            			adjacentTilePosition = currentGraph.Portals[portalID].Location[subscript];
//	                    	}
		                }
		                
		            	// does this portal cross a zone boundary portal? 
		            	if ( currentGraph.Portals[portalID].ZoneBoundary)
		            	{
		            		Unflatten (destGraphID, out currentZoneOffset.X, out currentZoneOffset.Y, out currentZoneOffset.Z);
		            		
		            		relativeZoneOffset = currentZoneOffset - startZoneOffset;
		            		
		            		// we need to update the portalID to match the portal index for this graph
		            		destPortalID = mGraphs[destGraphID].Areas[destAreaID].LocatePortal(adjacentTilePosition.X, 
    		                                                                                   adjacentTilePosition.Y, 
    		                                                                                   adjacentTilePosition.Z,
    		                                                                                   mGraphs[destGraphID].Portals);
		            	}

		                // convert to position relative to start tile
		                // relative tile position is used to compute heuristic fitness weight
		                Keystone.Types.Vector3i relativeTilePosition = adjacentTilePosition + TILES_PER_SIDE * relativeZoneOffset;
		                
		                // NOTE: no need to world bounds check the area, if it was generated it must be in bound in some zone.

		                // accumulate parent's accumulated weight with adjacent's weight (no diagonal penalties for connectivity graph)
		                // TODO: I think in theory, the weight used for adjacent should be the weight of all tiles from current tile to this adjacent
		                //       but we don't know exactly which path to that portal we'll take...  we could take an average perhaps
		                newG = System.Math.Abs (relativeTilePosition.X - currentPortalLocation.X) + System.Math.Abs(relativeTilePosition.Z - currentPortalLocation.Z);
		                if (currentPortalID != PRIMER)
			                newG += calcGrid[currentGraphID][currentPortalID].Weight;
		                		                	
										
		            	// for portals since their count is arbitary (unlike adjacent tiles)
		            	// we can only filter based on allowed parameters as we iterate and not
		            	// based on the number of tiles in a mDirection[] lookup 
		                if (parameters.PunishHorizontalChangeInDirection) // NOTE: diagonals incur both the X axis and Y axis change penalties
		                {
		                    if ((relativeTilePosition.X - currentPortalLocation.X) != 0)
		                    {
		                    	// moving to this adjacent would incur an X-Axis change of direction penalty
		                    	// (X-Axis did NOT change previous, but moving to this adjacent would)
		                        if (xAxisChangeAmount == 0)
		                            newG += Math.Abs(relativeTilePosition.X - relativeEnd.X) + Math.Abs(relativeTilePosition.Z - relativeEnd.Z);
		                    }
		                    if ((relativeTilePosition.Z - currentPortalLocation.Z) != 0)
		                    {
		                    	// moving to this adjacent would incur a Y-Axis change of direction penalty
		                    	// (X-Axis DID change previous, moving to this adjacent would now change Z-Axis instead)
		                        if (xAxisChangeAmount != 0)
		                            newG += Math.Abs(relativeTilePosition.X - relativeEnd.X) + Math.Abs(relativeTilePosition.Z - relativeEnd.Z);
		                    }
		                }
		                if (parameters.PunishVerticalChangeInDirection)
		                {
		                    // Y-Axis change in direction penalty
		                    if ((relativeTilePosition.Y - currentPortalLocation.Y) != 0)
		                    	if (yAxisChangeAmount != 0)
		                    		newG += Math.Abs (relativeTilePosition.Y - relativeEnd.Y);
		                }
		
		                // has this adjacent already been evaluated?  
						if ( currentGraph.Portals[portalID].ZoneBoundary)
						{
							if (calcGrid[destGraphID][destPortalID].Status == TraversalState.Open || calcGrid[destGraphID][destPortalID].Status == TraversalState.Closed)
								if (calcGrid[destGraphID][destPortalID].Weight <= newG)
									continue;
						}							
		                else if (calcGrid[currentGraphID][portalID].Status == TraversalState.Open || calcGrid[currentGraphID][portalID].Status == TraversalState.Closed)
		                {
		                    // this adjacent has accumulated less weight during a previous evaluation so we skip this evaluation
		                    // since we'll use results of previous evaluation instead since that route was better (lower cost)
		                    // NOTE: node status starts off as Unvisited so if .Status has reached either .Open or .Closed status, then it's weight MUST
		                    //       have been set by now. 
		                    if (calcGrid[currentGraphID][portalID].Weight <= newG)
		                    {
		                        //System.Diagnostics.Debug.WriteLine ("PATHING: Skipping PREVIOUSLY EVALUATED adjacent {0},{1},{2}.", adjacentX, adjacentY, adjacentZ);
		                    	continue;
		                    }
		                }

		            
		                // compute a heuristic weight based on the distance to end goal and the selected formula to use
		                switch(parameters.Formula)
		                {
		                    default:
		                    case HeuristicFormula.Manhattan:
		                		heuristic = parameters.HEstimate * (Math.Abs(relativeTilePosition.X - relativeEnd.X) + 
		                		                                    Math.Abs(relativeTilePosition.Z - relativeEnd.Z) + 
		                		                                    Math.Abs(relativeTilePosition.Y - relativeEnd.Y));
		                        break;
		                    case HeuristicFormula.MaxDXDY:
		                        heuristic = parameters.HEstimate * (Math.Max(Math.Abs(relativeTilePosition.X - relativeEnd.X), Math.Abs(relativeTilePosition.Z - relativeEnd.Z)));
		                        break;
		                    case HeuristicFormula.DiagonalShortCut:
		                        int h_diagonal  = Math.Min(Math.Abs(relativeTilePosition.X - relativeEnd.X), Math.Abs(relativeTilePosition.Z - relativeEnd.Z));
		                        int h_straight  = (Math.Abs(relativeTilePosition.X - relativeEnd.X) + Math.Abs(relativeTilePosition.Z - relativeEnd.Z));
		                        heuristic = (parameters.HEstimate * 2) * h_diagonal + parameters.HEstimate * (h_straight - 2 * h_diagonal);
		                        break;
		                    case HeuristicFormula.Euclidean:
		                        heuristic = (int) (parameters.HEstimate * Math.Sqrt(Math.Pow((relativeTilePosition.Z - relativeEnd.X) , 2) + Math.Pow((relativeTilePosition.Z - relativeEnd.Z), 2)));
		                        break;
		                    case HeuristicFormula.EuclideanNoSQR:
		                        heuristic = (int) (parameters.HEstimate * (Math.Pow((relativeTilePosition.X - relativeEnd.X) , 2) + Math.Pow((relativeTilePosition.Z - relativeEnd.Z), 2)));
		                        break;
		                    case HeuristicFormula.Custom1:
	//                            System.Drawing.Point dxy       = new System.Drawing.Point(Math.Abs(end.X - adjacentX), Math.Abs(end.Z - adjacentZ));
	//                            int Orthogonal  = Math.Abs(dxy.X - dxy.Z);
	//                            int Diagonal    = Math.Abs(((dxy.X + dxy.Z) - Orthogonal) / 2);
	//                            heuristic = parameters.HEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Z);
								heuristic = 0; // TEMP HACK WHILE System.Drawing.Point dxy above is commented out since we should probably switch to Vector3i for consistancy
		                        break;
		                }
		                if (parameters.TieBreaker)
		                {
		                    int dx1 = currentPortalLocation.X - relativeEnd.X;
		                    int dz1 = currentPortalLocation.Z - relativeEnd.Z;
		                    int dx2 = start.X - relativeEnd.X;
		                    int dz2 = start.Z - relativeEnd.Z;
		                    int cross = Math.Abs(dx1 * dz2 - dx2 * dz1);
		                    heuristic = (int) (heuristic + cross * 0.001);
		                }
		                
		                System.Diagnostics.Debug.Assert (newG > 0);
		                
		                calcGrid[currentGraphID][portalID].Weight  = newG;
		                calcGrid[currentGraphID][portalID].FitnessWeight = newG + heuristic;
		                calcGrid[currentGraphID][portalID].GraphID = destGraphID;
		                calcGrid[currentGraphID][portalID].PGraphID = currentGraphID;
		                calcGrid[currentGraphID][portalID].ID 		= destPortalID;
		               	calcGrid[currentGraphID][portalID].PID 		= currentPortalID;
		                calcGrid[currentGraphID][portalID].Previous = currentPortalLocation;
		                calcGrid[currentGraphID][portalID].Location = adjacentTilePosition;
		                
		            	// portals at zone boundaries must always have their corresponding adjacent in
		            	// the bordering zone updated IN TANDEM.
		                if ( currentGraph.Portals[portalID].ZoneBoundary)
		                {
		                	calcGrid[destGraphID][destPortalID] = calcGrid[currentGraphID][portalID];
		                	calcGrid[destGraphID][destPortalID].GraphID = destGraphID;
		                	calcGrid[destGraphID][destPortalID].ID = destPortalID;
		                }
		                //System.Diagnostics.Debug.WriteLine ("PATHING: Weight {0} - for adjacent {1},{2},{3}.", calcGrid[adjacentLocationID].FitnessWeight, adjacentX, adjacentY, adjacentZ);
		                
		
		                //It is faster if we leave the open node in the priority queue
		                //When it is removed, it will be already closed, it will be ignored automatically
		                //if (tmpGrid[newLocation].Status == 1)
		                //{
		                //    //int removeX   = newLocation & gridXMinus1;
		                //    //int removeY   = newLocation >> gridYLog2;
		                //    mOpen.RemoveLocation(newLocation);
		                //}
		
		                //if (tmpGrid[newLocation].Status != 1)
		                //{
		                	// add this adjacent to priority queue for evaluation
		                	// NOTE: the FitnessWeight is used as the priority value?  but it's not here is it and shouldn't it?
		                	ident = new PathingBroadPhase.PortalIdentifier();
				            ident.PortalID = destPortalID;
				            ident.GraphID = destGraphID;
				            ident.AreaID = destAreaID;
		                    openQueue.Push(ident);
		                //}
		                
		                // set this adjacent node status to "evaluate"
		                calcGrid[currentGraphID][portalID].Status = TraversalState.Open;
		                if ( currentGraph.Portals[portalID].ZoneBoundary)
		                	// also set the adjacent counterpart in next graph
		                	calcGrid[destGraphID][destPortalID].Status = TraversalState.Open;
		               	
		            } // end for
		
		            closeNodeCounter++;
		            if (currentPortalID != -1)
			        	calcGrid[currentGraphID][currentPortalID].Status = TraversalState.Closed;
		
		        } // end while
		
		        results.CompletedTime = stopWatch.ElapsedTicks;
		        if (results.Found)
		        {
		            closeList.Clear();
		            
		            // start with final found node (end location node) and work backwards
		            BroadPhasePathFinderNodeFast fNodeTmp = calcGrid[endGraphID][endLocationID];
		            BroadPhasePathFinderNodeFast foundNode = fNodeTmp;
		            
		            // iterate through all found nodes that make up the path in
					// reverse order using the portal IDs.  
		            while(foundNode.PID != -1)
		            {
		            	// NOTE: we use Insert instead of Add() since we are iterating in reverse order
		            	// but we want the resulting waypoints sorted first to last order
		                closeList.Insert(0, foundNode);
		                #if DEBUGON
		                if (mDebugFoundPath && PathFinderDebug != null)
		                    PathFinderDebug(foundNode.Previous.X, foundNode.Previous.Y, foundNode.Previous.Z, 
		                	                foundNode.Location.X, foundNode.Location.Y, foundNode.LocationZ,
		                	                PathFinderNodeType.Path, foundNode.F, foundNode.G);
		                #endif
		                
		                
		                Types.Vector3i previousLocation = foundNode.Previous;
		                // move pointer
		                fNodeTmp = calcGrid[foundNode.PGraphID][foundNode.PID]; 
		                //System.Diagnostics.Debug.Assert (foundNode.Location == previousLocation);
		                //foundNode.Location  = previousLocation;                
		                foundNode = fNodeTmp;
		            } 
		
		            closeList.Insert(0, foundNode);
		            #if DEBUGON
		            if (mDebugFoundPath && PathFinderDebug != null)
		                PathFinderDebug(foundNode.Previous.X, foundNode.Previous.Y, foundNode.Previous.Z, 
		            	                foundNode.Location.X, foundNode.Location.Y, foundNode.Location.Z, 
		            	                PathFinderNodeType.Path, foundNode.F, foundNode.G);
		            #endif
		
		            results.FoundNodes = closeList;
		        }
		  	}
		  	catch (Exception ex)
		  	{
		  		System.Diagnostics.Debug.WriteLine (ex.Message);
		  	}
		  	
		  	
		    return results;
		  }
        
		  private Keystone.Types.Vector3i FindPortalPosition (Connectivity.Area area, Connectivity.AreaPortal portal, bool adjacentArea)
		  {
	        	Keystone.Types.Vector3i position;
	        	
		  		position.X = portal.Location[0].X;
	        	position.Y = portal.Location[0].Y;
	        	position.Z = portal.Location[0].Z;
	        	
	        	if (area.Contains (position.X, position.Y, position.Z) && adjacentArea == false)
	        		return position;
	        	else 
	        	{
	        		position.X = portal.Location[1].X;
	        		position.Y = portal.Location[1].Y;
	        		position.Z = portal.Location[1].Z;
	        	}
	        	
	        	return position;
		  }
		  
		private int FindPortalDirectionSubscript(Connectivity.Area area, Connectivity.AreaPortal portal)
		{
			if (area.Contains (portal.Location[0].X, portal.Location[0].Y, portal.Location[0].Z))
			    return 0;
			    
			return 1;
		}
		  
		private void Unflatten(int index, out int x, out int y, out int z)
        {     
            uint testX, testY, testZ;
            Utilities.MathHelper.UnflattenIndex ((uint)index, (uint)mGridSizeX, (uint)mGridSizeZ, out testX, out testY, out testZ);
            x = (int)testX;
            y = (int)testY;
            z = (int)testZ;
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="x">Row Index, not a real x coord</param>
		/// <param name="z">Column Index, not a real z coord</param>
		/// <returns></returns>
        private int Flatten (int x, int y, int z)
        {
            int index = (int)Utilities.MathHelper.FlattenCoordinate ((uint)x, (uint)y, (uint)z, (uint)mGridSizeX, (uint)mGridSizeZ);
            return index;   
        }
	}
}
