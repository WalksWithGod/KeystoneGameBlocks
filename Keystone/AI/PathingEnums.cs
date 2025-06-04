/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 12/18/2014
 * Time: 10:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Keystone.AI
{

    internal enum PathFinderNodeType
    {
        Start   = 1,
        End     = 2,
        Open    = 4,
        Close   = 8,
        Current = 16,
        Path    = 32
    }
	    
    internal enum HeuristicFormula
    {
    	// Manhattan is usually best for pure cartesian grids combined with 
    	// the tiebreaker option.
		// Manhattan (or Taxicab) distance only permits traveling along 
		// the x and y axes, so the distance between (0, 0) and (1, 1) is 
		// 2 from traveling along the X axis 1 unit and then up the Y 
		// axis 1 unit, and not the hypotenuse, which would be sqrt(2)/2. 
		// It's so named because in a city you're bound to the streets. 
		// You can't cut diagonally through a building to go north/south 
		// one block and east/west one block.
        Manhattan           = 1,
		// Chebyshev (aka: MaxDXDY)- Chebyshev distance considers the movement on both axis
		// and takes the maximum between the two differences. This 
		// heuristic can be paired with the 8-directional neighbor.<br /> 
		// To visualize the usefulness of this heuristic imagine a
		// claw machine. While you're moving in one direction you can  
		// simultaneously move in the other direction at no cost in time 
		// (unless you move more in this direction, which makes this the max).
		// Sometimes in poorly programmed video games you notice that your 
		// character can move faster on the diagonal. This is because the 
		// motion is working in Chebyshev-space, but the world map you're 
		// running around on is in Euclidean space. Say the character has 
		// a set speed of one unit per update cycle. The up and the left 
		// buttons are pressed on the controller simultaneously, so the 
		// character moves up one unit and left one unit. The effect on 
		// the screen in Euclidean space is that the character gets a 
		// speed boost. The character should have been limited to one unit 
		// of motion, but by moving in two dimensions at once, the 
		// character traveled sqrt(2)/2 units in one update. In Chebyshev 
		// space these movements are considered equal, and the discrepancy 
		// between the Euclidean space of the game world and the Chebyshev 
		// space of the poorly chosen input scheme betray this. 
        MaxDXDY             = 2,
		//	Diagonal distance is a perfect pair with the 8-directional 
		//  neighbor selection scheme. It performs a perfect measure in  
		//  distance when movement is restricted entirely to horizontal,  
		//  vertical, and diagonal motion.
        DiagonalShortCut    = 3,
        // This is normal distance calculated by the Pythagorean formula: 
        // d = sqrt((a_x-b_x)^2+(a_y-b_y)^2)
        Euclidean           = 4,
		// This heuristic is an optimized version of the 
		// Euclidean heuristic. It is the normal distance formula, but 
		// without taking the square root. Because the distance is only 
		// used to compare, comparing the squares of two values still 
		// works to determine which path is shorter. Is does weight the 
		// from-cost (g) much less than the to-cost (h) because the from-
		// cost is added up incrementally while the to-cost is calculated 
		// in one big piece. This is caused because the distance between 
		// the start and the cursor is many very small steps squared 
		// individually before being added up (often just 1, which is not 
		// increased by being squared at all) so the Euclidean squared 
		// heuristic is much greedier about getting to its goal. 
		// Sometimes this imbalance will cause it to select a less 
		// optimal path closer to the start in favor of a better path 
		// closer to the goal.
        EuclideanNoSQR      = 5,
        Custom1             = 6
    }
    
    		
    internal enum TraversalState : byte
	{
		Unvisited = 0,
		Open = 1,
		Closed = 2, // closed nodes weight will never be re-evaluated
		Found = 3
	}
}
