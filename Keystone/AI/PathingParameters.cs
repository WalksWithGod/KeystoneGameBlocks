/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 12/18/2014
 * Time: 10:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Keystone.AI
{

	internal struct PathFindParameters
	{
		public double DIAGONAL_MOVEMENT_PENALTY_MULTIPLIER;
        public HeuristicFormula                Formula;
        public bool                            AllowDiagonalMovement; 
        public bool                            AllowVerticalMovement;
        // public bool                         mPenalizeVerticalMovement; // going up/down stairs should normally be penalized to represent slower speed one must use to go up/down stairs/ladders/etc?
        public int                             HEstimate;
        public bool                            PunishVerticalChangeInDirection;
        public bool                            PunishHorizontalChangeInDirection;
        public bool                            TieBreaker;
        public bool                            PenalizeDiagonalMovement;
        public int                             SearchLimit;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="searchLimit"></param>
        /// <param name="heuristicEstimate"> the higher this value, the fewer nodes will be examined but the less optimal the path will be.  if heuristicEstimate == 0 then A* is equivalent to Dijkstra</param>
        /// <param name="allowDiagonalMovement"></param>
        /// <param name="allowVerticalMovement"></param>
        /// <param name="punishHorizontalChangeDirection"></param>
        /// <param name="punishVerticalChangeDirection"></param>
        /// <param name="tieBreaker"></param>
        /// <param name="penalizeDiagonalMovement"></param>
        public PathFindParameters (HeuristicFormula formula, int searchLimit = 2000, int heuristicEstimate = 2,  
                                   bool allowDiagonalMovement = false, bool allowVerticalMovement = true,  
                                   bool punishHorizontalChangeDirection = false, bool punishVerticalChangeDirection = true,
                                   bool tieBreaker = false, bool penalizeDiagonalMovement = false)
        {
        	Formula = formula;
        	HEstimate = heuristicEstimate; // the higher this value, the fewer nodes will be examined but the less optimal the path will be.  if heuristicEstimate == 0 then A* is equivalent to Dijkstra
        	SearchLimit = searchLimit;
        	AllowDiagonalMovement = allowDiagonalMovement;
        	AllowVerticalMovement = allowVerticalMovement;
        	PunishHorizontalChangeInDirection = punishHorizontalChangeDirection;
        	PunishVerticalChangeInDirection = punishVerticalChangeDirection;
        	TieBreaker = tieBreaker;
        	PenalizeDiagonalMovement = penalizeDiagonalMovement;
        	
        	DIAGONAL_MOVEMENT_PENALTY_MULTIPLIER = 2.41;
        }
	}
}
