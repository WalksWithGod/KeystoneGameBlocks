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
    internal delegate void InteriorPathFinderDebugHandler(int fromX, int fromY, int x, int y, int z, PathFinderNodeType type, int totalCost, int cost);
    #endregion


    // FOR AI PATHING SEE 
    //  E:\dev\_projects\_AI\PathFinder_source
    // AND
    //	Keystone.Portals.CellMap
    //			DoLinkSearch
    //			FindLinks
    //			BreadthFirstSearch

    internal enum MOVEMENT_STATE : int
    {
        Normal = 0,
        Walking_Up_Stairs = 1,
        Walking_Down_Stairs = 2,
        Climbing_Up_Ladder = 3,
        Climbing_Down_Ladder = 4,
        Walking_Through_Door = 5,
        Entering_Elevator = 6,
        Existing_Elevator = 7,
        Riding_Elevator = 8,
        Walking_On_Traversable_Component = 9
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct InteriorPathFinderNodeFast
    { 
        public double FitnessWeight;  // (f) accumulated cost + estimated cost (heuristic is added cost based on distance of this node to the goal).  This value is used by Compare() method.
        public double Weight;         // (g) accumulated cost to reach this node beginning at starting node
        public int H;              // (h) estimated cost
        public int Floor;
        public ConnectivityInterior.Area Area;
        public int AreaIndex;
        public int PortalIndex; // portal index within the current Area
        public int PreviousPortalIndex; // portal index within the Previous Area
        public Types.Vector3d Position;
        public ConnectivityInterior.Area PreviousArea;
        public int PreviousAreaIndex;
        public TraversalState Status; // node has either 0 not evaluated, 1 open evaluated, 2 closed evaluated status.  
        public Entities.Entity Entity;
        public MOVEMENT_STATE MovementState;
    }

    internal struct InteriorPathFindResults
    {
        public List<InteriorPathFinderNodeFast> FoundNodes;
        public bool Found;
        public bool SearchCountExceeded;
        public long CompletedTime;
        public bool DebugProgress;  // show path finding process
        public bool DebugFoundPath; // draw found path
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
    internal class PathingInterior
    {

        #region Inner Classes
        internal class CompareInteriorPFNodeMatrix : IComparer<InteriorPathFinderNodeFast>
        {
            // initialize with entire matrix
            public CompareInteriorPFNodeMatrix()
            {
            }

            public int Compare(InteriorPathFinderNodeFast a, InteriorPathFinderNodeFast b)
            {

                if (a.FitnessWeight > b.FitnessWeight)
                    return 1;
                else if (a.FitnessWeight < b.FitnessWeight)
                    return -1;

                return 0;
            }
        }
        #endregion

        #region Events
        public event InteriorPathFinderDebugHandler PathFinderDebug;
        #endregion


        // TODO: i think this grid should become an array of 2D arrays so that we have an array by altitude of
        //       2D x/z plane collision data layers.
        //       - the reason is, we want to reference the 2D array without 
        public PathingInterior()
        {

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
        
        public InteriorPathFindResults Find(Portals.Interior interior, Keystone.Entities.Entity npc, ConnectivityInterior.Area[][] areas,
                                              Keystone.Types.Vector3i start, Keystone.Types.Vector3i end, PathFindParameters parameters)
        {
            
            // NOTE: the A* assumes that the areas and portals were based on available movement
            // type of the unit.  Otherwise if the CG is generated for units that CAN jump across
            // 1 tile wide gaps but the unit we A* for against that CG cannot, that unit will 
            // fail to jump when required and the pathing will break
            InteriorPathFinderNodeFast[][] calcGrid = null;
            PriorityQueueB<InteriorPathFinderNodeFast> openQueue = null;
            List<InteriorPathFinderNodeFast> closeList = null;
            int closeNodeCounter = 0;

            double heuristic = 0;
            int xAxisChangeAmount = 0;
            int yAxisChangeAmount = 0;
            double newG = 0;

            // TODO: this assignment of "start" is not the currentPortalLocaction.  It's just a tile location
            //       of which we can find the current Area starting location.
            Types.Vector3i currentPortalLocation = start;
            Types.Vector3d vecCurrentPortalLocation = interior.GetTileCenter(currentPortalLocation);
            Types.Vector3d vecEnd = interior.GetTileCenter(end);

            bool stop = false;
            int currentPortalIndex = -1;

            calcGrid = new InteriorPathFinderNodeFast[areas.GetLength(0)][];
            openQueue = new PriorityQueueB<InteriorPathFinderNodeFast>(new CompareInteriorPFNodeMatrix());
            closeList = new List<InteriorPathFinderNodeFast>();
            //openQueue.Clear();
            //closeList.Clear();

            InteriorPathFindResults results;
            results.SearchCountExceeded = false;
            results.FoundNodes = null;
            results.CompletedTime = -1;
            results.DebugFoundPath = false;
            results.DebugProgress = false;
            results.Found = false;

#if DEBUGON
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

		    if (mDebugProgress && PathFinderDebug != null)
		        PathFinderDebug (0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
		    if (mDebugProgress && PathFinderDebug != null)
		        PathFinderDebug (0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
#endif


            // initialize calcGrid[][] jagged array
            int floorCount = areas.GetLength(0);
            calcGrid = new InteriorPathFinderNodeFast[floorCount][];
            for (int i = 0; i < floorCount; i++)
            {
                int areasCount = 0;
                if (areas[i] != null)
                {
                    areasCount = areas[i].Length;
                    calcGrid[i] = new InteriorPathFinderNodeFast[areasCount];
                    for (int j = 0; j < areasCount; j++)
                    {
                        calcGrid[i][j].AreaIndex = j; 
                        calcGrid[i][j].Floor = i;
                        calcGrid[i][j].Weight = 0; // initialize to 1 // March.10.2020 - is it necessary to init to 1?  0 seems to work the same
                    }
                }
            }

            try
            {
                Keystone.AI.ConnectivityInterior.Area currentArea = interior.LocateAreaContainingTile(start.X, start.Y, start.Z);
                Keystone.AI.ConnectivityInterior.Area endArea = interior.LocateAreaContainingTile(end.X, end.Y, end.Z);

                if (currentArea.Index == -1 || endArea.Index == -1) return results;

                calcGrid[currentArea.Floor][currentArea.Index].Area = currentArea;

                int currentFloor = start.Y;
                int endFloor = end.Y;
                System.Diagnostics.Debug.Assert(currentFloor == currentArea.Floor);
                System.Diagnostics.Debug.Assert(endFloor == endArea.Floor);

                // NOTE: area.Index is a subscript within the current floor jagged array.  This means
                // indices across floors can be the same. So we must always use Area.Index with floorIndex to pick the 
                // correct Area.
                int startAreaIndex = currentArea.Index; 
                int endAreaIndex = endArea.Index;
                int currentAreaIndex = startAreaIndex;
                currentPortalIndex = -1;
                ConnectivityInterior.AreaPortal[] portals = currentArea.Portals;
                int portalCount = portals == null ? 0 : portals.Length;
                // TODO: i dont think this should be initialized to 0. but -1 as we do above
                if (portalCount > 0)
                    currentPortalIndex = 0;

                // prime the queue with a node representing our starting Area
                // TODO: this is a struct, we should skip "new" call and just populate the members to fully instantiate it
                InteriorPathFinderNodeFast node = new InteriorPathFinderNodeFast();
                node.Position = vecCurrentPortalLocation;
                node.Area = currentArea;
                node.Floor = start.Y;
                node.PortalIndex = currentPortalIndex;
                openQueue.Push(node);

                
                //System.Diagnostics.Debug.WriteLine("PATHING.FIND() - Start Index = " + startAreaIndex + " End Index = " + endAreaIndex);

                while (openQueue.Count > 0 && !stop)
                {
                    node = openQueue.Pop();

                    currentAreaIndex = node.Area.Index;
                    currentFloor = node.Floor;
                    currentPortalIndex = node.PortalIndex;
                    currentArea = areas[currentFloor][currentAreaIndex];
                    calcGrid[currentFloor][currentAreaIndex].Area = currentArea;
                    // TODO: here we can't just assign "i" because it just results in the last index always being used right?
                    //       we need the PortalIndex from the node we've dequeued
                    calcGrid[currentFloor][currentAreaIndex].PortalIndex = currentPortalIndex;

                    //Is it in closed list? means this node was already processed
                    if (calcGrid[currentFloor][currentAreaIndex].Status == TraversalState.Closed)
                    {
                        continue;
                    }

                    // because the priority queue has us dequeing the best scoring path so far
                    // if we are at the desired location, then we know we can abort further tests now
                    // we've reached the end location.  exit the while loop
                    if (currentFloor == endFloor && currentAreaIndex == endAreaIndex)
                    {
                        calcGrid[endFloor][endAreaIndex].Status = TraversalState.Found;
                        calcGrid[endFloor][endAreaIndex].Area = currentArea;

                        UpdateMovementState(calcGrid, endFloor, endAreaIndex);

                        results.Found = true;
                        //System.Diagnostics.Debug.WriteLine ("PATHING: Found tile {0},{1},{2}.", currentX, currentY, currentZ);
                        break;
                    }

                    if (closeNodeCounter > parameters.SearchLimit)
                    {
                        System.Diagnostics.Debug.Assert(results.Found == false);
                        results.SearchCountExceeded = true;
#if DEBUGON
                        results.CompletedTime = stopWatch.ElapsedTicks;
#endif
                        System.Diagnostics.Debug.WriteLine(string.Format("PATHING: Search LIMIT {0} reached.", parameters.SearchLimit));
                        return results;
                    }


                    portals = currentArea.Portals;
                    portalCount = portals == null ? 0 : portals.Length;
                    vecCurrentPortalLocation = node.Position;

#if DEBUG
                    if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Stairs lower landing found");
                    }
                    else if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Stairs upper landing found.");
                    }
                    else if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING) != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Ladder lower landing found.");
                    }
                    else if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING) != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Ladder Upper landing found.");
                    }
#endif
                    // calculate traversal cost to each portal (Not to each portal's destination areas which is incorrect way to think about problem!)
                    //http://csharpexamples.com/fast-image-processing-c/
                    // "parallel-for" loop opportunity here?
                    for (int i = 0; i < portalCount; i++)
                    {
                        int destAreaSubscript = 0;
                        // TODO: the AreaIndex here could be on a floor above or below.  The only way
                        //       to know that is to compare the Floor level.  Does this mean we need for
                        //       portal's to hold the floor level of both the origin and destination subscripts?
                        //       And consider that the following test could be wrong where currentAreaIndex == destAreaSubscript
                        //       but on different floors so we MUST check the floor also before determining which destAreaSubscript to use
                        if (currentFloor == currentArea.Portals[i].MinMax[0].Y && currentArea.Portals[i].AreaIndex[destAreaSubscript] == currentAreaIndex) 
                            destAreaSubscript =  1;


                        // TODO: these subscripts are an issue.  We cannot assume destAreaSubscript is always 1.  We need to be 
                        //       picking the opposite subscript from the currentArea we're in.
                        int nextFloor =  currentArea.Portals[i].MinMax[destAreaSubscript].Y;
                        int nextAreaIndex = currentArea.Portals[i].AreaIndex[destAreaSubscript]; // is destAreaSubscript correct here for AreaIndex?


#if DEBUG
                        //if (currentAreaIndex == 10)
                        //{
                        //    nextFloor = currentArea.Portals[i].MinMax[1].Y;
                        //    nextAreaIndex = currentArea.Portals[i].AreaIndex[1]; // is destAreaSubscript correct here for AreaIndex?
                       // }

                       // if (nextFloor == 2)
                        //    System.Diagnostics.Debug.WriteLine("Found upper landing");
#endif

                        int nextPortal = -1;
                        if (areas[nextFloor].Length <= nextAreaIndex || areas[nextFloor][nextAreaIndex].Portals == null)
                            nextPortal = -1;
                        else
                            nextPortal = GetPortalContainingTile(areas[nextFloor][nextAreaIndex].Portals, currentArea.Portals[i].MinMax[destAreaSubscript]); // currentArea.Portals[i].Center);


                        if (nextPortal == -1 || currentArea.Contains (currentArea.Portals[i].MinMax[destAreaSubscript].X, currentArea.Portals[i].MinMax[destAreaSubscript].Z))
                        {
                            nextPortal = GetPortalContainingTile(areas[nextFloor][nextAreaIndex].Portals, currentArea.Portals[i].MinMax[destAreaSubscript]);
                            if (nextPortal == -1)
                                continue;
                        }

                        System.Diagnostics.Debug.Assert(nextPortal != -1);
                        // TODO: could i just look up the index here  nfrom currentArea.Portals[i].AreaIndex(destAreaSubscript];
                        //       and using the nextFloor index to limit our search?

                        
                        if ((areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.WALL) != 0 ||
                            (areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.COMPONENT) != 0 ||
                            (areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.COMPONENT_STACKABLE) != 0)
                            continue;

                        if ((areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.FLOOR) == 0)
                            continue;

#if DEBUG
                        //if (nextFloor == 2)
                        //    System.Diagnostics.Debug.WriteLine("Upper level Area found.");

                        //if ((areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Stairs Lower landing found.");
                        //}
                        //else if ((areas[nextFloor][nextAreaIndex].TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Stairs Upper landing found.");
                        //}
                        //else if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING) != 0)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Ladder lower landing found.");
                        //}
                        //else if ((currentArea.TileValue & (int)Portals.Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING) != 0)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Ladde Upper landing found.");
                        //}

#endif 

                        // TODO: portals have min/max values for MinMax[] and we are just selecting either min or max, not center! This is screwing up the heuristic
                        // TODO: That is why nextTilePosition isn't really used for our Manhattan search.  It's only used for the heuristics we haven't updated yet.
                        Keystone.Types.Vector3i nextTilePosition = new Types.Vector3i(); //  // currentArea.Portals[i].NearestTile[destAreaSubscript];
                                                                                         // currentArea.Portals[i].Center; 

                        // skip this portal if both conditions are true:
                        // A) subscript[1] is where we are right now and subscript[0] is where we were before that.
                        // B) subscript[0] is where we are right now and subscript[1] is where we were before that 
                        // If A) fails but B) passes, we can switch to using subscript[0]
                        //if (currentArea.Index == nextAreaIndex && currentAreaIndex == nextAreaIndex)
                        //{
                        //    const int subscript = 0;
                        //    nextAreaIndex = currentArea.Portals[i].AreaIndex[subscript];
                        //    //    nextTilePosition = currentArea.Portals[i].MinMax[subscript]; // TODO: this should be taking a tile location that is nearest to the end tile location.

                        //}

                          Keystone.Types.Vector3d vecNextPosition = GetNearestTile(interior, currentArea.Portals[i].MinMax[0], currentArea.Portals[i].MinMax[1], (uint)destAreaSubscript, vecCurrentPortalLocation, vecEnd); // interior.GetTileCenter(nextTilePosition);
                       // Keystone.Types.Vector3d vecNextPosition = GetNearestTile(interior, areas[nextFloor][nextAreaIndex].Portals[nextPortal].MinMax[0], areas[nextFloor][nextAreaIndex].Portals[nextPortal].MinMax[1], (uint)destAreaSubscript, vecCurrentPortalLocation, vecEnd); // interior.GetTileCenter(nextTilePosition);

                        // NOTE: no need to world bounds check the Area, if it was generated it must be in bound.


                        // TODO: I think in theory, the weight used for adjacent should be the weight of all tiles from current tile to this adjacent
                        //       but we don't know exactly which path to that portal we'll take...  we could take an average perhaps
                        // accumulate parent's accumulated weight with adjacent's weight (no diagonal penalties for connectivity graph)
                        double distance = (vecNextPosition - vecCurrentPortalLocation).Length;
                        //System.Diagnostics.Debug.Assert(distance > 0.0d);
                        newG = distance;
                        newG += calcGrid[currentFloor][currentAreaIndex].Weight;
                       // System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - New Portal '" + nextPortal.ToString() + "' Weight = " + newG.ToString() + " to Area Index = "  + nextAreaIndex.ToString());
                        
                        
                        if (calcGrid[nextFloor][nextAreaIndex].Status == TraversalState.Open || calcGrid[nextFloor][nextAreaIndex].Status == TraversalState.Closed)
                        { 
                            //if (distance == 0)
                            //{
                             //   // if the distance == 0, it could be algo trying to go back to previous portal since all our portals are 1 way
                             //   System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - weight == 0 - SKIPPING portal");
                             //   continue;
                            //}

                            // this adjacent has accumulated less weight during a previous evaluation so we skip this evaluation
                            // since we'll use results of previous evaluation instead since that route was better (lower cost)
                            // NOTE: node status starts off as Unvisited so if .Status has reached either .Open or .Closed status, then it's weight MUST
                            //       have been set by now. 
                            if (calcGrid[nextFloor][nextAreaIndex].Weight <= newG)
                            {
                                //System.Diagnostics.Debug.WriteLine ("PATHING: Skipping PREVIOUSLY EVALUATED adjacent {0},{1},{2}.", adjacentX, adjacentY, adjacentZ);
                                continue;
                            }
                        }

                        // if this is a special component such as Door or Elevator/Lift, confirm that this NPC has access to it.
                        // if there is no access, set the heuristic value to maximum to indicate untraversable
                        // TODO: privileges is a bit a problem.  It's not really inherent to the engine, it's more specific to gameplay
                        // yet here we are in path.Find() and we do need to know if a door or elevator component is accessible.
                        // Is there a way to make that scriptable?  Where we have the npc object passed to the component's script to
                        // determine there if accessibility exists?
                        //object p = npc.GetCustomPropertyValue("privileges");
                        //     string componentID = interior.FindComponent(vecNextPosition).ID;
                        // todo: does using the component require us to implement accessbility script to every component?
                        //       That's why it's probably best for interior.Execute() to be used and it can determine what component exists
                        //       in an Area we are testing, and whether the npc has access to that Area.
                        // todo: the problem is, how do we know which tile to check for Component?  Especially since some tiles are 
                        //       assigned to components even when they aren't touching it.  But wait, we "occupy" those tiles with a 
                        //       reference to the Entity anyway right?
                        // todo: could this reaaaaally slow down our pathing code?  If we had the information within the connectivity graph (areas and portals)
                        //       this would not be a big concern.
                        // TODO: Should we be uing currentArea here or "nextArea"?
                        ConnectivityInterior.Area nextArea = areas[nextFloor][nextAreaIndex];
                        int tileValue = nextArea.TileValue;
                        if ((tileValue & (int)Portals.Interior.TILE_ATTRIBUTES.ENTRANCE_AND_EXIT) != 0) // door
                        {
                            // todo: should we pass in the end Vector3i location so we know which deck we're trying to reach and whether
                            //       in the case of an elevator, does it go to that floor?
                            bool traversable = false;
                            // todo: do i test against the door component directly, or do i make call to interior.cs scripted function?
                            Entities.Entity door = (Entities.Entity)interior.Layer_GetValue("floorentity", (uint)nextArea.MinX, (uint)nextArea.Floor, (uint)nextArea.MinZ);
                            System.Diagnostics.Debug.Assert(door != null);
                            System.Diagnostics.Debug.Assert(door.Script != null);
                            // todo: for a door, traversability is either "ok" or "not ok."  If the door is damaged and unpassable, the same FALSE score is returned just as if the NPC didn't have appropriate security privileges to enter that door.
                            // TODO: and can't we just "continue" the loop if the traversable function returns False?  We don't need to go forward with the heuristic at all.
                            // todo: and how do we determine if we are exiting vs entering a "room"?  one side of the door is next to one room, and the other side, another.
                            //       So in a sense, its the room that has the security clearance privilege value, or do we have user during design time, assign the values to 2 custom properties covering both side.
                            //       I think we need to have the user design time edit the privilege values for the door.  For simplicity, maybe we don't have different traversable values for
                            //       "entering" vs "exiting" and so if a crew member somehow enters without privileges, they can't exit.
                            traversable = (bool)door.Execute("IsAccessible", new object[] { door.ID, npc.ID });

                            // TODO: how do we handle closed doors and waiting for doors to "open" 

                            // TODO: how do we handle "locked" doors that need to be unlocked first.  Are all doors automatically locked when "Closed"?

                            // TODO: when the NPC script is pathing, when we reach this Area that has a door, we need to "use" the door first. (eg. open the door)
                            //       which means employing any necessary animation on both the npc and the door.
                            System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - Door entrance and exit Area = " + nextArea.Index);
                            if (!traversable)
                            {
                                // TODO: does this skip the entire Area as it should or just this current tile?  we already know the entire Area contains the same tileValue so no need to test every tile in the Area.
                                throw new Exception("PathingInterior.Find() - Door not traversable by this NPC");
                                continue;
                            }
                            else
                            {
                                calcGrid[nextFloor][nextAreaIndex].Entity = door;
                            }
                        }


                        // compute a heuristic weight based on the distance to end goal and the selected formula to use
                        switch (parameters.Formula)
                        {
                            default:
                            case HeuristicFormula.Manhattan:
                                //heuristic = parameters.HEstimate * (Math.Abs(nextTilePosition.X - end.X) +
                                //                                    Math.Abs(nextTilePosition.Z - end.Z) +
                                //                                    Math.Abs(nextTilePosition.Y - end.Y));
                                heuristic = (double)parameters.HEstimate * (vecNextPosition - vecEnd).Length;
                                // TODO: all of the below should use the vecNextPosition and vecEnd and not TileLocation which are Vector3i
                                break;
                            case HeuristicFormula.MaxDXDY:
                                heuristic = parameters.HEstimate * (Math.Max(Math.Abs(nextTilePosition.X - end.X), Math.Abs(nextTilePosition.Z - end.Z)));
                                break;
                            case HeuristicFormula.DiagonalShortCut:
                                int h_diagonal = Math.Min(Math.Abs(nextTilePosition.X - end.X), Math.Abs(nextTilePosition.Z - end.Z));
                                int h_straight = (Math.Abs(nextTilePosition.X - end.X) + Math.Abs(nextTilePosition.Z - end.Z));
                                heuristic = (parameters.HEstimate * 2) * h_diagonal + parameters.HEstimate * (h_straight - 2 * h_diagonal);
                                break;
                            case HeuristicFormula.Euclidean:
                                heuristic = (int)(parameters.HEstimate * Math.Sqrt(Math.Pow((nextTilePosition.Z - end.X), 2) + Math.Pow((nextTilePosition.Z - end.Z), 2)));
                                break;
                            case HeuristicFormula.EuclideanNoSQR:
                                heuristic = (int)(parameters.HEstimate * (Math.Pow((nextTilePosition.X - end.X), 2) + Math.Pow((nextTilePosition.Z - end.Z), 2)));
                                break;
                            case HeuristicFormula.Custom1:
                                //                            System.Drawing.Point dxy       = new System.Drawing.Point(Math.Abs(end.X - adjacentX), Math.Abs(end.Z - adjacentZ));
                                //                            int Orthogonal  = Math.Abs(dxy.X - dxy.Z);
                                //                            int Diagonal    = Math.Abs(((dxy.X + dxy.Z) - Orthogonal) / 2);
                                //                            heuristic = parameters.HEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Z);
                                heuristic = 0; // TEMP HACK WHILE System.Drawing.Point dxy above is commented out since we should probably switch to Vector3i for consistancy
                                break;
                        }
                        //if (parameters.TieBreaker)
                        //{
                        //    int dx1 = currentPortalLocation.X - end.X;
                        //    int dz1 = currentPortalLocation.Z - end.Z;
                        //    int dy1 = currentPortalLocation.Y - end.Y;
                        //    int dx2 = start.X - end.X;
                        //    int dz2 = start.Z - end.Z;
                        //    int dy2 = start.Y - end.Y;
                        //    int cross = Math.Abs(dx1 * dz2 - dx2 * dz1);
                        //    heuristic = (int)(heuristic + cross * 0.001);
                        //}

                        //System.Diagnostics.Debug.Assert(newG > 0);

                        calcGrid[nextFloor][nextAreaIndex].Weight = newG;
                        calcGrid[nextFloor][nextAreaIndex].FitnessWeight = newG + heuristic;
                        calcGrid[nextFloor][nextAreaIndex].Area = areas[nextFloor][nextAreaIndex];
                        calcGrid[nextFloor][nextAreaIndex].PortalIndex = nextPortal; // i;
                        calcGrid[nextFloor][nextAreaIndex].Floor = nextFloor; // TODO: is this correct? SHouldn't it be the currentFloor and the Property renamed to PreviousFloor?
                        calcGrid[nextFloor][nextAreaIndex].PreviousPortalIndex = currentPortalIndex; // currentPortalLocation;
                        calcGrid[nextFloor][nextAreaIndex].PreviousArea = currentArea;
                        //calcGrid[nextFloor][nextAreaIndex].Location = nextTilePosition;

                        UpdateMovementState(calcGrid, nextFloor, nextAreaIndex);


                        //System.Diagnostics.Debug.WriteLine ("PATHING: Weight {0} - for adjacent {1},{2},{3}.", calcGrid[adjacentLocationID].FitnessWeight, adjacentX, adjacentY, adjacentZ);


                        //It is faster if we leave the open node in the priority queue
                        //When it is removed, it will be already closed, it will be ignored automatically
                        //if (tmpGrid[newLocation].Status == TraversalState.Open)
                        //{
                        //    //int removeX   = newLocation & gridXMinus1;
                        //    //int removeY   = newLocation >> gridYLog2;
                        //    mOpen.RemoveLocation(newLocation);
                        //}

                        //if (tmpGrid[newLocation].Status != TraversalState.Open)
                        //{
                        // add this adjacent to priority queue for evaluation
                        // NOTE: the FitnessWeight is used as the priority value?  but it's not here is it and shouldn't it?
                        node = new InteriorPathFinderNodeFast();
                        node.PortalIndex = nextPortal; // i;
                        node.Position = vecNextPosition;
                        node.Floor = nextFloor;
                        node.Area = areas[nextFloor][nextAreaIndex];
                        node.FitnessWeight = calcGrid[nextFloor][nextAreaIndex].FitnessWeight;
                        node.Entity = calcGrid[nextFloor][nextAreaIndex].Entity;

                        openQueue.Push(node);
                        //}

                        // set this adjacent node status to "evaluate"
                        calcGrid[nextFloor][nextAreaIndex].Status = TraversalState.Open;
                        
                    } // end for

                    closeNodeCounter++;
                    if (currentPortalIndex != -1)
                        calcGrid[currentFloor][currentAreaIndex].Status = TraversalState.Closed;

                } // end while
#if DEBUGON
                results.CompletedTime = stopWatch.ElapsedTicks;
#endif
                if (results.Found)
                {
                    closeList.Clear();

                    // start with final found node (end location node) and work backwards
                    InteriorPathFinderNodeFast fNodeTmp = calcGrid[endFloor][endAreaIndex];
                    InteriorPathFinderNodeFast foundNode = fNodeTmp;
                    int loopCount = 0;

                    //if (foundNode.Floor != start.Y || foundNode.AreaIndex != startAreaIndex)
                    ////if (foundNode.Area.Index != calcGrid[start.Y][startAreaIndex].Area.Index)
                    //{
                        // iterate through all found nodes that make up the path in
                        // reverse order using the portal IDs.  
                        while (foundNode.Floor != start.Y || foundNode.AreaIndex != startAreaIndex)
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

                            // move pointer
                            fNodeTmp = calcGrid[foundNode.PreviousArea.Floor][foundNode.PreviousArea.Index];

                            //System.Diagnostics.Debug.Assert (foundNode.Location == previousLocation);
                            //foundNode.Location  = previousLocation;                
                            foundNode = fNodeTmp;
#if DEBUG
                            loopCount++;
                            if (loopCount > parameters.SearchLimit)
                                System.Diagnostics.Debug.WriteLine("PathingInterior.Find() -- bad loop");
#endif
                        }
                    //}
               //     closeList.Insert(0, foundNode); // Feb.17.2020 - stop adding the Area that contains our start vector3d to the list of found nodes.  Is this bad?  Should we instead just prevent the ConstructWaypoints from using the first node?
#if DEBUGON
		            if (mDebugFoundPath && PathFinderDebug != null)
		                PathFinderDebug(foundNode.Previous.X, foundNode.Previous.Y, foundNode.Previous.Z, 
		            	                foundNode.Location.X, foundNode.Location.Y, foundNode.Location.Z, 
		            	                PathFinderNodeType.Path, foundNode.F, foundNode.G);
#endif

                    results.FoundNodes = closeList;


#if DEBUG
                    // or should we append the weights to results.Weights
                    // when storing weights, are we storing them by portal or by Area?  If by Area, then that means we can walk through non-optimal portal since any portal of the Area can change the weight
                    // todo: when searching, we should skip portals of an Area that do not connect to the currentArea in the search.  Are we doing that?
              //      GetWeights(calcGrid);
#endif
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PathingInterior.Find() - " + ex.Message);
            }


            return results;
        }

        private void UpdateMovementState(InteriorPathFinderNodeFast[][]grid, int floor, int areaIndex)
        {
            int tileValue = grid[floor][areaIndex].Area.TileValue;
            int previousFLoor = grid[floor][areaIndex].PreviousArea.Floor;
            int previousIndex = grid[floor][areaIndex].PreviousArea.Index;
            if (previousIndex == -1) return;

            if (grid[previousFLoor].Length == 0) return;

            int previousTileValue = grid[previousFLoor][previousIndex].Area.TileValue;
            int temp = grid[floor][areaIndex].PreviousArea.TileValue;
            System.Diagnostics.Debug.Assert(previousTileValue == temp);

            if ((tileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
            {
                // were we previously on COMPONENT_TRAVERSABLE?  This would indicate we have come downstairs
                // and future points until we reach a different MOVEMENT_STATE should be clamped to this floor
                //grid[floor][areaIndex].MovementState = MOVEMENT_STATE.Walking_Up_Stairs;
                
            }
            else if ((tileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
            {
                // were we previously on COMPONENT_TRAVERSABLE?  This would indicate we just came UPstairs.
            }
            else if ((tileValue & (int)Portals.Interior.TILE_ATTRIBUTES.COMPONENT_TRAVERSABLE) != 0)
            {
                // are we headed upstairs or downstairs?  This lets us know where to clamp the waypoints leading up to this change of MOVEMENT_STATE as well as the following waypoints.  Or is this necessary since we will appear on either an up/down landing and can check then?
                // so we check if previousTileValue is STAIR_LOWER_LANDING or STAIR_UPPER_LANDING
                if ((previousTileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
                {
                    // TODO: wait, shouldnt the current state be Walking_Up_Stairs and the following
                    //       state should be "Reached_Up_Stairs" landing

                    grid[floor][areaIndex].MovementState = MOVEMENT_STATE.Walking_Up_Stairs;
                    //grid[previousFLoor][previousIndex].MovementState = MOVEMENT_STATE.Walking_Up_Stairs;
                }
                else if ((previousTileValue & (int)Portals.Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
                {
                    // TODO: wait, shouldnt the current state be Walking_Up_Stairs and the following
                    //       state should be "Reached_Down_Stairs" landing
                    grid[floor][areaIndex].MovementState = MOVEMENT_STATE.Walking_Down_Stairs;
                    // we need to know if the tileValue of the next waypooint if walking up or down stairs so we can
                    // determine if our intermediate goal is centering the LANDING.
                    //grid[previousFLoor][previousIndex].MovementState = MOVEMENT_STATE.Walking_Down_Stairs;           
                }
                else
                {
                    // NOTE: I don't think this should ever occur.
                    grid[previousFLoor][previousIndex].MovementState = MOVEMENT_STATE.Normal;
                }
            }
            else
            {
                // we're just walking on the current floor and Y values during steering should be clamped to current floor
            }
        }

        private int GetPortalContainingTile(ConnectivityInterior.AreaPortal[] portals, Types.Vector3i location)
        {
            int result = -1;
            if (portals == null) return result;

            for (int i = 0; i < portals.Length; i++)
                if (portals[i].Contains(location))
                    return i;

            return result;
        }

        /// <summary>
        /// Finds the point within a portal bounding box closest to the passed in point
        /// </summary>
        /// <param name="interior"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="destinationSubscript"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <remarks>this only works in 2D XZ plane</remarks>
        private Types.Vector3d GetNearestTile(Portals.Interior interior, Types.Vector3i min, Types.Vector3i max, uint destinationSubscript, Types.Vector3d point, Types.Vector3d end)
        {
            // make sure we are taking min/max tile index values from the destination Area
            if (destinationSubscript == 0)
            {
                if (min.X + 1 == max.X)
                    max.X = min.X;
                else
                    max.Z = min.Z;
            }
            else
            {
                if (min.Z + 1 == max.Z)
                    min.Z = max.Z;
                else
                    min.X = max.X;
            }

            Types.Vector3d vecMin = interior.GetTileCenter(min);
            Types.Vector3d vecMax = interior.GetTileCenter(max);

            var dx = Math.Max(vecMin.x - point.x, point.x - vecMax.x);
            var dz = Math.Max(vecMin.z - point.z, point.z - vecMax.z);

            Types.Vector3d result;
            result.x = dx;
            result.y = vecMin.y; // min.Y;
            result.z = dz;

            
            Types.BoundingBox box = new Types.BoundingBox (vecMin, vecMax);
            result = box.Center;
            result.y = vecMin.y;
  //          return result;

            // TODO: I think using "end" here is not correct.  It needs an "dest" location that is closest to next waypoint such as door or ladder
            result = Utilities.MathHelper.ClosestPointTo(box, end);

     
            // distance = (point - result).Length();
            return result;
        }
    }
}
