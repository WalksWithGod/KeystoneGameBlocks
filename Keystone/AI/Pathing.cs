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
	// 	Modified by Hypnotron for use with 3D tile maps - Nov.2014
	#endregion

//#define DEBUGON

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Keystone.AI
{
	
    #region Delegates
    internal delegate void PathFinderDebugHandler(int fromX, int fromY, int x, int y, int z, PathFinderNodeType type, int totalCost, int cost);
    #endregion
    

	// FOR AI PATHING SEE 
	//  E:\dev\_projects\_AI\PathFinder_source
	// AND
	//	Keystone.Portals.CellMap
	//			DoLinkSearch
	//			FindLinks
	//			BreadthFirstSearch
	
		
    [StructLayout(LayoutKind.Sequential, Pack=1)] 
    internal struct PathFinderNodeFast
    {
		// NOTE: this struct is only used during Runtime path finding in the mCalcGrid.  The original obstacle weight is 
		// stored as a single int32 in the mDataGrid
        public int     FitnessWeight;  // (f) accumulated cost + estimated cost (heuristic is added cost based on distance of this node to the goal).  This value is used by Compare() method.
        public int     Weight;         // (g) accumulated cost to reach this node beginning at starting node
        // public int H;               // (h) estimated cost
        public int     X;  // Node Location X
        public int     Y;  // Node location Y
        public int     Z;  // Node location Z
        public int  PX; // X location of node previously traversed
        public int  PY; // Y location of node previously traversed
        public int  PZ; // Z location of node previously traversed
        public TraversalState    Status; // node has either 0 not evaluated, 1 open evaluated, 2 closed evaluated status.  
    }
	    
	
	internal struct PathFindResults
	{
		public List<PathFinderNodeFast>        FoundNodes;
		public bool                            Found;
		public bool                            SearchCountExceeded;
		public long                            CompletedTime;
        public bool                            DebugProgress;  // show path finding process
        public bool                            DebugFoundPath; // draw found path
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
	internal class Pathing
	{

    	internal const int TILE_UNPASSABLE = 66;
    	internal const int TILE_EMPTY =      0;
        	
        #region Inner Classes
        internal class ComparePFNodeMatrix : IComparer<int>
        {
            PathFinderNodeFast[] mMatrix;

            // initialize with entire matrix
            public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
            {
                mMatrix = matrix;
            }

            // compare two different matrix elements at index a and index b
            public int Compare(int a, int b)
            {
                if (mMatrix[a].FitnessWeight > mMatrix[b].FitnessWeight)
                    return 1;
                else if (mMatrix[a].FitnessWeight < mMatrix[b].FitnessWeight)
                    return -1;
                return 0;
            }
        }
        #endregion
        
		#region Events
    	public event PathFinderDebugHandler PathFinderDebug;
    	#endregion
       
        private int[]                        mGrid                   = null;
        private int                          mGridSizeX              = 0;
        private int                          mGridSizeY              = 0;
        private int                          mGridSizeZ              = 0;
                
        // direction array allows us to iterate in a single loop rather than nested loops
        // for each of x,y and z axis
    	// NOTE: the order here does not matter at all.  They simply contain all of the
    	//       26 x,y,z adjacent directions (skipping the starting node at 0,0,0
//        private sbyte[,]   mDirection = new sbyte[26,3]{{0,0,-1} , {1,0,0}, {0,0,1}, {-1,0,0},    // y = 0 FOUR 2D cardinal neighbors (does not include the center our starting node)
//    												   {1,0,-1}, {1,0,1}, {-1,0,1}, {-1,0,-1},   // y = 0, FOUR 2D diagonal neighbors
//    												   {0,-1,-1}, {1,-1,0}, {0,-1,0}, {0,-1,1}, {-1,-1,0}, // y = -1, FOUR lower cardinal neighbors _includes_ center 
//    												   {1,-1,-1}, {1,-1,1}, {-1,-1,1}, {-1,-1,-1},         // y = -1, FOUR lower diagonals
//    												   {0,1,-1} , {1,1,0}, {0,1,0}, {0,1,1}, {-1,1,0},    // y = +1, FOUR upper cardinal neighbors _includes_ center 
//    												   {1,1,-1}, {1,1,1}, {-1,1,1}, {-1,1,-1}             // y = +1, FOUR upper diagonals
//    													};

    	private sbyte[,]   mDirection = new sbyte[26,3]{{0,0,-1} , {1,0,0}, {0,0,1}, {-1,0,0}, {0,-1,0}, {0,1,0},   // y = 0 SIX 2D cardinal neighbors including UP and DOWN (does not include the center our starting node)
    												   {0,-1,-1}, {1,-1,0}, {0,-1,1}, {-1,-1,0},    // y = -1, FOUR lower cardinal neighbors (stair descending)
    												   {0,1,-1} , {1,1,0}, {0,1,1}, {-1,1,0},       // y = +1, FOUR upper cardinal neighbors (stair climbing)
													   {1,0,-1}, {1,0,1}, {-1,0,1}, {-1,0,-1},      // y = 0, FOUR diagonal neighbors    												   
       												   {1,-1,-1}, {1,-1,1}, {-1,-1,1}, {-1,-1,-1},  // y = -1, FOUR lower diagonals   // note: diagonal hill climbing is allowed, but steep climbing walls should be restricted to cardinal upper/lower adjacents
    												   {1,1,-1}, {1,1,1}, {-1,1,1}, {-1,1,-1}       // y = +1, FOUR upper diagonals
    													};
    	

        // TODO: i think this grid should become an array of 2D arrays so that we have an array by altitude of
        //       2D x/z plane collision data layers.
        //       - the reason is, we want to reference the 2D array without 
        public Pathing (int[] grid, int gridSizeX, int gridSizeY, int gridSizeZ)
        {
        	// Keystone.Portals.Structure.TILEMASK_FLAGS
        	// TODO: byte vs int for footprint?  before i used footprint to store flag data and not weight data
        	//       but maybe weight data is just a better way to do this?  i had flags for whether something was
        	//       structural support, whether a wall was pourous, half-height, transparent, whether obstacle was
        	//       wall mounted, floor, ceiling, whether something was ladder or stairs, fuel line, etc.  So in other words
        	//       it was bitflag data held in a layer that represented in 2D data, what was there in the world so AI could act
        	//       server side using just the data layer.
        	//       The question is now, can't we simply use a different layer for that other information?  Consider now how we
        	//       store terrain_layout and terrain_style in seperate bitmaps with 
        	//       terrain_layout.png -> TileType ID (8 bits)
        	//       terrain_style.png -> entity/segment index(8bits), sub-model index(8bits), minimesh element index(16bits)
        	//       structure_layout.png
        	//       structure_style.png
        	//       component_layout.png
        	//       component_style.png <-- same type of component can look different (eg metal trash can vs plastic recycle bin)
        	//       obstacles.png
        	//
        	// weights - should be considered for traversing cost and not for game specific costs like hazardousness? )
        	//	- 0 - NO TERRAIN - air / toxic/hazardous air (temperature/radiation/poison/etc) / vaccuum 
        	//		 - requires pathparameters of +canfly or +jumpjet if moving greater than +1 air tile in a row?
        	//	- 1 - NORMAL TERRAIN - cleared roads, sand, short grass, gravel, etc
        	//	- 2 - MEDIUM TERRAIN - ice, plowed soil, thick snow, uncleared roads, tall grass
        	//	- 5 - closed door (with key) <-- door dynamically modifies it's contribution to cost when closed vs open state 
        	//                                   - and perhaps a "key" can somehow offset the cost if the current traversal parameters supplies a
        	//                                     modifier for a certain obstacle's weight contribution so that the weight can be offset.
        	//                                     e.g. security guard has key to a door and during A*, he can ignore the door but will still need to
        	//                                     run open animation when reaching that tile.
        	//
        	// obstacle weight, obstacle material (for sound), 
        	//  - 6 - stairs 
        	//  - 7 - ladder, rope
        	//  - 10 - low fence, barricade,
        	//  - 15 - DIFFICULT TERRAIN - swamp, thick mud, thick brush, thicket, volcanic rock, (1/3 walk/crawl)  <-- done via vegetation layer? or a brush that allows us to change the top segment used.
        	//  - 20 - steep slope 
        	//  -     - IMPOSSIBLE TERRAIN - 
        	//  - 25 - requires swimming 
        	//  - 100 - closed door (no key but door is breakable)
        	//  - 128 - deadly liquid (lava, acid, poison)
        	//  - 255 - impassable (does not mean the object cannot be destroyed by digging, explosives, etc)
        	
        
        	
			// NOTE: tiles also can give attack bonus and armor bonus such as a heavily forested tile vs open plains tile
			// 
			// should tile attribute and weight (movement cost) be combined?  i somehow think they need to be seperate.
			// because the weight value would need to be modified at runtime during pathing to allow
			// an empty tile to be traversable.  By keeping them seperate we simply can add a PathFindParameter
			// to allow processing of "Air" tiles or to restrict to "Solid" tiles or "Water" for ships, etc. 
			// And the weights for ground vs water vs air can all be kept seperate so that you can have a amphibuous
			// unit still select to swim rather than walk around since the weight wont be so high in an effort to discourage
			// a land unit. 
			//
			// http://archive.gamedev.net/archive/reference/articles/article728.html - Tile Based Games FAQ 
			// tile type (what about the degrees of type? - well normally that's what "weight" is for for a type of unit..)
			// 0 - empty  { pure vacuum -> trace atmosphere} (living unit, under it's own control, cannot normally traverse this without protection and propulsion)
			// 1 - air    {earthlike -> poisonous} (living unit, under it's own control, needs to be able to fly or jump to traverse this tile)
			// 2 - liquid {water -> lava and puddle depth -> ocean depth} 
			// 3 - solid  {mud -> ice -> rock -> pavement -> grass}
			// 
			// 
			// for instance, can our obstacle map contain walkability info
			// - where 0 = empty  (not necessarily pure vacuum, can be air.  but solid vs water vs air or less)
			// - 64 can be deep water
			// - 65 - solid ground
        	
        	
        	//  how do the above terrain, structures and component layers affect the weights?
        	//  - a rule that says whenever ANY terrain layer segment or structure layer segment is added
        	//    if it's full height, it activates the layer above it with the weight for it's base difficulty, and
        	//    for it's own layer, it assigns impassable.
        	//	  - any layer with obstacle value of 0 is "air" or "vaccuum" and cannot be stood upon... players fall through.
        	//      if they aren't flying.  OR... 
			//    - *** SOLUTION: The current layer is the +1 and the NPC is always standing at the very top of that layer such that their feet and thus
			//      the entire NPC is regarded as being in layer 1 whilst the NPC's body extends up into Layer 2.
			//			- Layer 0 is regarded as IMPOSSIBLE since it either does not exist or there is no hole to get to it.
        	//		- YES.  This makes sense!  And because we're standing at heighest point of the current layer, it means when our character
        	//        ray casts to find height, they are doing so against the layer they are ON, not a layer beneath them.  
        	//     - QUESTION: pathfinding to layers above and below?  We can implement different costs to climbing up vs climbing down in our search parameters based on situation.
        	//              - maybe a ladder/stairs will decrease the cost to switch a level significantly.  To do this, we could have a seperate layer that specified the costs
        	//                to go up and down with some layers impassable and others very low cost due to existance of stairs and or stairs. As an NPC path found, when they arrived
        	//                at point where they are changing from tile at elevation i to tile at elevation i+1, then they could test to see what kind of accessibility component was there
        	//                and use it (elevator, escalator, stairs, ladder, rope, etc)
        	//				- placing a TERRAIN can set a VERTICAL traveral cost to current layer, but perhaps also set HORIZONTAL traversal costs
        	//				  to layers below or above perhaps with different costs for up vs down.  This makes sense and is not
        	//              a special case.  Structures can do the same thing and in fact, structures seem no different than terrain. Just different auto tile rules. 
        	//                  - interior structures can exist only in entirely interior structure scaled zones.  The paths from exterior to interior must
        	//                    occur through portals which can be formed dynamically as structures are destroyed.  
        	//         http://gamedev.stackexchange.com/questions/46392/efficient-path-finding-on-2d-tile-based-multilevel-map     <-- MULTI-LEVEL (UP/DOWN) some suggestions on broadphase 
        	//         initial search using connectivity graph and such.
        	
        	//	- when we add a terrain, it sets +1 to the layer above it?  it adds +255 to it's own layer.
        	//		- but what if that terrain is supposed to be 
        	//		- must each seperate MODEL in a segment have it's own Footprint?  I think yes.  This means
        	//        perhaps in our Appearance plugin perhaps we can see the footprint panel and edit it.  Then in footprint editor
        	//        we can cycle through the selector node to different segment versions and create unique footprints.
        	//        - anytime a model changes (not just the segment!) we must unapply previous footprint and then apply new.
        	
        	//  - are they cumulative?
        	//  How do we deal with world bounds, areas with no terrain/structure to stand on, areas we can stand, air (jumping vs air travel)
        	
        	
        	
        	
			// UNIT FORMATIONS
			// - http://www.youtube.com/watch?v=5KxO4SDU1hY
			//   The "Leader - Follow" approach used in youtube vid link above performs path.find() for leader and having rest of units move relative to that unit.
			//   It has problems as we've seen in games like Total War.
			//   You get some sub-units that fail to get through doors or narrow alleys properly.
        	//   - the reason is simple... if those units are trying to follow, then they'll not really be using any pathing of their own and instead will have to
        	//   use some limited obstacle avoidance but not every frame per unit for performance reasons.
        	//	 - we see "steering" as ways to try and avoid the issues where following units lose proximity to their units but these steering behaviors eventually seem
        	//   to break down.  Perhaps one thing the steering should do is try to steer towards leader's path if the distance from leader starts to get too great.  
        	//  The nasa link below uses steering to prevent collisions between dynamic moving agents. (eg crowd collision avoidance) by better solving for non colliding 
        	//  accelerations for entities to accumulation next frame.
        	// http://www.youtube.com/watch?v=Hc6kng5A8lQ
        	// - Starcraft 2 also uses steering and a single waypoint but probably also allows individual units to initiate a path.find() if it starts to get too far from it's leader.
        	//   - so the state of the art is combining steering behaviors individually and using special flocking behavior rules to create formations... supplimenting this behavior with
        	// unique per unit.path finds if they get too lost.
        	
        	// TODO: we don't need constructor if we just pass the following to Find() also.  The key is to avoid having to recreate the grid[] array from tile.Data so
        	//       we should try to find a way to keep that .Data as same layout as we need for Pathing.Find() and for things like Connectivity.Generate()
        	mGrid = grid;
        	
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
        public PathFindResults Find(Keystone.Types.Vector3i start, Keystone.Types.Vector3i end, PathFindParameters parameters)
        {

			PathFinderNodeFast[]         calcGrid            = null;
        	PriorityQueueB<int>          openQueue           = null;
        	List<PathFinderNodeFast>     closeList           = null;
        	int                          closeNodeCounter    = 0;
        	
        	int                          heuristic           = 0;
	        int                          xAxisChangeAmount   = 0;
	        int                          yAxisChangeAmount   = 0;
        	int                          newG                = 0;
        	
        	int                          currentLocationID   = 0;
        	int                          adjacentLocationID  = 0;
        	int                          endLocationID       = 0;
        	
        	int                          currentX            = 0;
        	int                          currentY            = 0;
        	int                          currentZ            = 0;
        	int                          adjacentX           = 0;
        	int                          adjacentY           = 0;
        	int                          adjacentZ           = 0;
        	
        	bool                         stop                = false;
        	
        	
        	calcGrid = new PathFinderNodeFast[mGridSizeX * mGridSizeY * mGridSizeZ]; 
        	openQueue   = new PriorityQueueB<int>(new ComparePFNodeMatrix(calcGrid));
        	closeList   = new List<PathFinderNodeFast>();
            //openQueue.Clear();
            //closeList.Clear();
            
        	PathFindResults results;
        	results.SearchCountExceeded = false;
        	results.FoundNodes = null;
        	results.CompletedTime = -1;
        	results.DebugFoundPath = false;
        	results.DebugProgress = false;
			results.Found = false;
        
			#if DEBUGON
        	System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
			#endif

            #if DEBUGON
            if (mDebugProgress && PathFinderDebug != null)
                PathFinderDebug(0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
            if (mDebugProgress && PathFinderDebug != null)
                PathFinderDebug(0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
            #endif

            // flatten start & end location X,Y coords into flat index
            currentLocationID               = Flatten (start.X, start.Y, start.Z);
            endLocationID                   = Flatten (end.X, end.Y, end.Z);
            
            calcGrid[currentLocationID].Weight         = 0;
            calcGrid[currentLocationID].FitnessWeight  = parameters.HEstimate;
            calcGrid[currentLocationID].PX             = start.X;
            calcGrid[currentLocationID].PY             = start.Y;
            calcGrid[currentLocationID].PZ             = start.Z;
            calcGrid[currentLocationID].Status         = TraversalState.Open;

            openQueue.Push(currentLocationID);
            while(openQueue.Count > 0 && !stop)
            {
                currentLocationID    = openQueue.Pop();

                //Is it in closed list? means this node was already processed
                if (calcGrid[currentLocationID].Status == TraversalState.Closed)
                {
                	continue;
                }
                
                // unflatten location index to X,Y,Z coordinates
                Unflatten (currentLocationID, out currentX, out currentY, out currentZ);
                
                #if DEBUGON
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, currentLocationID & mGridSizeX_Minus_1, currentLocationID >> mGridZLog2, PathFinderNodeType.Current, -1, -1);
                #endif

                
                // we've reached the end location.  exit the while loop
                // because the priority queue has us dequeing the best scoring path so far
                // if we are at the desired location, then we know we can abort further tests now
                if (currentLocationID == endLocationID)
                {
                    calcGrid[currentLocationID].Status = TraversalState.Closed;
                    results.Found = true;
                    System.Diagnostics.Debug.WriteLine ("Pathing.Find() - Found tile {0},{1},{2}.", currentX, currentY, currentZ);
                    break;
                }

                if (closeNodeCounter > parameters.SearchLimit)
                {
                	System.Diagnostics.Debug.Assert (results.Found == false);
                	results.SearchCountExceeded = true;
    				#if DEBUGON
                    results.CompletedTime = stopWatch.ElapsedTicks;
                    #endif
                    System.Diagnostics.Debug.WriteLine ("Pathing.Find() - Search LIMIT reached.");
                    return results;
                }

                if (parameters.PunishHorizontalChangeInDirection)
                    xAxisChangeAmount = (currentX - calcGrid[currentLocationID].PX); 
                
                if (parameters.PunishVerticalChangeInDirection)
                    yAxisChangeAmount = (currentY - calcGrid[currentLocationID].PY);
                

                int CARDINAL_DIRECTION_COUNT = 6;
                int CARDINAL_COUNT_PLUS_VERTICAL = 14;
                int TOTAL_ADJACENT_COUNT = 26;
                int DIAGONAL_START_INDEX = 14;
                int adjacentCount = parameters.AllowVerticalMovement ? CARDINAL_COUNT_PLUS_VERTICAL : CARDINAL_DIRECTION_COUNT;
                // TODO: AllowVerticalMovement should NOT automatically enable Diagonals but that's exactly what it does!
                //       TODO: the trick will be to use different arrays for our mDirection and then perhaps simply allowing
				//             traversal of all elements?  hrm.. let's see
				//             - cardinals, diagonals current floor, cardinals upper floor, diagonals upper floor, cardinals lower floor, diagonals bottom floor				
                adjacentCount = parameters.AllowDiagonalMovement ? TOTAL_ADJACENT_COUNT : adjacentCount;

                //Lets calculate each adjacent's traversal cost
                //http://csharpexamples.com/fast-image-processing-c/
                // "parallel-for" loop opportunity here?
                for (int i = 0; i < adjacentCount; i++)
                {
                    adjacentX = currentX + mDirection[i, 0];
                    adjacentY = currentY + mDirection[i, 1]; 
                    adjacentZ = currentZ + mDirection[i, 2];
                    
                    // this particular adjacent of the current tile does not exist as it would be out of bounds
                    if (adjacentX >= mGridSizeX || adjacentZ >= mGridSizeZ || adjacentY >= mGridSizeY ||
                       adjacentX < 0 || adjacentY < 0 || adjacentZ < 0)
                    {
                        //System.Diagnostics.Debug.WriteLine ("PATHING: Skipping OOB adjacent {0},{1},{2}.", adjacentX, adjacentY, adjacentZ);
                    	continue;
                    }
                    
	                // flatten to index
                    adjacentLocationID  = Flatten(adjacentX, adjacentY, adjacentZ); 
                    
                    // empty tile reached (eg. air, or a hole).  Is it above something we can stand on?
                    if (mGrid[adjacentLocationID] == TILE_EMPTY || mGrid[adjacentLocationID] >= TILE_UNPASSABLE)
                    {
                        //System.Diagnostics.Debug.WriteLine ("Pathing.Find() - Skipping EMPTY or UNPASSABLE adjacent {0},{1},{2}.", adjacentX, adjacentY, adjacentZ);
                    	continue;
                    }
                    // accumulate parent's accumulated weight with adjacent's weight plus any penalty modifier
                    if (parameters.PenalizeDiagonalMovement && i >= DIAGONAL_START_INDEX)
                    	// if this is a diagonal and we are increasing cost to move diagonal
                        newG = calcGrid[currentLocationID].Weight + (int) (mGrid[adjacentLocationID] * parameters.DIAGONAL_MOVEMENT_PENALTY_MULTIPLIER);
                    else
                    	// not a diagonal so we just use data layer weight for this tile
                        newG = calcGrid[currentLocationID].Weight + mGrid[adjacentLocationID];

                    if (parameters.PunishHorizontalChangeInDirection) // NOTE: diagonals incur both the X axis and Y axis change penalties
                    {
                        if ((adjacentX - currentX) != 0)
                        {
                        	// moving to this adjacent would incur an X-Axis change of direction penalty
                        	// (X-Axis did NOT change previous, but moving to this adjacent would)
                            if (xAxisChangeAmount == 0)
                                newG += Math.Abs(adjacentX - end.X) + Math.Abs(adjacentZ - end.Z);
                        }
                        if ((adjacentZ - currentZ) != 0)
                        {
                        	// moving to this adjacent would incur a Y-Axis change of direction penalty
                        	// (X-Axis DID change previous, moving to this adjacent would now change Z-Axis instead)
                            if (xAxisChangeAmount != 0)
                                newG += Math.Abs(adjacentX - end.X) + Math.Abs(adjacentZ - end.Z);
                        }
                    }
                    if (parameters.PunishVerticalChangeInDirection)
                    {
                		// TODO: if the unit can fly or jump really well, then the +Y modifier should
                		//       reduce the weight for an above tile that is just "air" but at the same time
                		//       what if the distance over air is too large?  how do we restrict to just an appropriate
                		//       jump distances?
                        // Y-Axis change in direction penalty
                        if ((adjacentY - currentY) != 0)
                        	if (yAxisChangeAmount != 0)
                        		newG += Math.Abs (adjacentY - end.Y);
                    }

                    // has this adjacent already been evaluated?  
                    if (calcGrid[adjacentLocationID].Status == TraversalState.Open || calcGrid[adjacentLocationID].Status == TraversalState.Closed)
                    {
                        // this adjacent has accumulated less weight during a previous evaluation so we skip this evaluation
                        // since we'll use results of previous evaluation instead since that route was better (lower cost)
                        // NOTE: node status starts off as Unvisited so if .Status has reached either .Open or .Closed status, then it's weight MUST
                        //       have been set by now. 
                        if (calcGrid[adjacentLocationID].Weight <= newG)
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
                    		heuristic = parameters.HEstimate * (Math.Abs(adjacentX - end.X) + Math.Abs(adjacentZ - end.Z) + Math.Abs(adjacentY - end.Y));
                            break;
                        case HeuristicFormula.MaxDXDY:
                            heuristic = parameters.HEstimate * (Math.Max(Math.Abs(adjacentX - end.X), Math.Abs(adjacentZ - end.Z)));
                            break;
                        case HeuristicFormula.DiagonalShortCut:
                            int h_diagonal  = Math.Min(Math.Abs(adjacentX - end.X), Math.Abs(adjacentZ - end.Z));
                            int h_straight  = (Math.Abs(adjacentX - end.X) + Math.Abs(adjacentZ - end.Z));
                            heuristic = (parameters.HEstimate * 2) * h_diagonal + parameters.HEstimate * (h_straight - 2 * h_diagonal);
                            break;
                        case HeuristicFormula.Euclidean:
                            heuristic = (int) (parameters.HEstimate * Math.Sqrt(Math.Pow((adjacentZ - end.X) , 2) + Math.Pow((adjacentZ - end.Z), 2)));
                            break;
                        case HeuristicFormula.EuclideanNoSQR:
                            heuristic = (int) (parameters.HEstimate * (Math.Pow((adjacentX - end.X) , 2) + Math.Pow((adjacentZ - end.Z), 2)));
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
                        int dx1 = currentX - end.X;
                        int dz1 = currentZ - end.Z;
                        int dx2 = start.X - end.X;
                        int dz2 = start.Z - end.Z;
                        int cross = Math.Abs(dx1 * dz2 - dx2 * dz1);
                        heuristic = (int) (heuristic + cross * 0.001);
                    }
                    
                    calcGrid[adjacentLocationID].Weight  = newG;
                    calcGrid[adjacentLocationID].FitnessWeight = newG + heuristic;
                    calcGrid[adjacentLocationID].PX      = currentX;
                    calcGrid[adjacentLocationID].PY      = currentY;
                    calcGrid[adjacentLocationID].PZ      = currentZ;
                    
                    
                    //System.Diagnostics.Debug.WriteLine ("PATHING: Weight {0} - for adjacent {1},{2},{3}.", calcGrid[adjacentLocationID].FitnessWeight, adjacentX, adjacentY, adjacentZ);
                    
                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(currentX, currentY, adjacentX, adjacentY, PathFinderNodeType.Open, calcGrid[adjacentLocationID].FitnessWeight, calcGrid[adjacentLocationID].Weight);
                    #endif

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
                        openQueue.Push(adjacentLocationID);
                    //}
                    
                    // set this adjacent node status to "evaluate"
                    calcGrid[adjacentLocationID].Status = TraversalState.Open;
                } // end for

                closeNodeCounter++;
                // set node status to "already evaluated"
                calcGrid[currentLocationID].Status = TraversalState.Closed;

                #if DEBUGON
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, 0, currentX, currentY, currentZ, PathFinderNodeType.Close, calcGrid[currentLocationID].FitnessWeight, calcGrid[currentLocationID].Weight);
                #endif
            } // end while

            #if DEBUGON
            results.CompletedTime = stopWatch.ElapsedTicks;
            #endif
            
            if (results.Found)
            {
                closeList.Clear();
                
                // start with end point location and work backwards
                int posX = end.X;
                int posY = end.Y;
                int posZ = end.Z;

                // get copy of the end location node
                PathFinderNodeFast fNodeTmp = calcGrid[endLocationID];
                PathFinderNodeFast foundNode;
                foundNode.FitnessWeight  = fNodeTmp.FitnessWeight;
                foundNode.Weight  = fNodeTmp.Weight;
                
                foundNode.PX = fNodeTmp.PX; //previous node X location
                foundNode.PY = fNodeTmp.PY; //previous node Y location
                foundNode.PZ = fNodeTmp.PZ; //previous node Z location
                foundNode.X  = end.X;
                foundNode.Y  = end.Y;
                foundNode.Z  = end.Z;
				
                foundNode.Status = TraversalState.Found;
				
                // iterate through all found nodes that make up the path in
				// reverse order using the PX,PY coordinates of parents to find their flattened mCalcGrid index
                while(foundNode.X != foundNode.PX || foundNode.Y != foundNode.PY || foundNode.Z != foundNode.PZ)
                {
                	// NOTE: we use Insert instead of Add() since we are iterating in reverse order
                	// but we want the resulting waypoints sorted first to last order
                    closeList.Insert (0, foundNode);
                    #if DEBUGON
                    if (mDebugFoundPath && PathFinderDebug != null)
                        PathFinderDebug(foundNode.PX, foundNode.PY, foundNode.PZ, foundNode.X, foundNode.Y, foundNode.Z, PathFinderNodeType.Path, foundNode.F, foundNode.G);
                    #endif
                    posX = foundNode.PX;
                    posY = foundNode.PY;
                    posZ = foundNode.PZ;
                    
                    // move pointer
                    fNodeTmp = calcGrid[Flatten(posX, posY, posZ)];
                    
                    foundNode.FitnessWeight  = fNodeTmp.FitnessWeight;
                    foundNode.Weight  = fNodeTmp.Weight;
                    
                    foundNode.PX = fNodeTmp.PX;
                    foundNode.PY = fNodeTmp.PY;
                    foundNode.PZ = fNodeTmp.PZ;
                    foundNode.X  = posX;
                    foundNode.Y  = posY;
                    foundNode.Z  = posZ;
                    
                    foundNode.Status = fNodeTmp.Status;
                } 

                closeList.Insert(0, foundNode);
                #if DEBUGON
                if (mDebugFoundPath && PathFinderDebug != null)
                    PathFinderDebug(foundNode.PX, foundNode.PY, foundNode.PZ, foundNode.X, foundNode.Y, foundNode.Z, PathFinderNodeType.Path, foundNode.F, foundNode.G);
                #endif

                results.FoundNodes = closeList;
            }
            #if DEBUG
            else 
            {
            	System.Diagnostics.Debug.WriteLine ("Pathing.Find() - Path not found.");
            }
            #endif

            return results;
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
