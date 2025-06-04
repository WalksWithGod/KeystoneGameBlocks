using System;
using Keystone.Extensions;

namespace Keystone.TileMap
{
	
	struct PatternCondition 
	{
		public TileDirections Direction;
		public TileTypes Type;
	}
    
	struct PatternRule 
	{
		public int TileIndex;
		public int TileRotation; // 0, 90, 180, 270
		public int[] AtlasTextureRowColumn; // [0]Row, [1]Column
		public PatternCondition[] Conditions;
		
		public PatternRule (int tileIndex, int tileRotation)
		{
			TileIndex = tileIndex;
			TileRotation = tileRotation;
			Conditions = null;
			AtlasTextureRowColumn = new int[2];
		}
		
		public void Add (PatternCondition condition)
		{
			Conditions = Conditions.ArrayAppend (condition);
		}
	}
	
	// rule based system http://www.squidi.net/three/entry.php?id=166 for auto-tiling
	struct Pattern 
	{
		public TileTypes TileType;
		public int DefaultIndex;
		public PatternRule[] Rules;
		
		public void Add (PatternRule r)
		{
			Rules = Rules.ArrayAppend (r);
		}
		
		public PatternRule Query (int[] adjacentSegmentTypes)
		{
	    	// convert from int to Structure.TileTypes
	    	TileTypes[] tmp = new TileMap.TileTypes[adjacentSegmentTypes.Length];
	    	for (int i = 0; i < tmp.Length; i++)
	    		tmp[i] = (TileTypes)adjacentSegmentTypes[i];
	    	
	    	return Query (tmp);
		}
		// the current TileType is that of this Pattern.  
		// iterate through Rules and based on adjacent Types deterimine
		// appropriate modelLookup index
		public PatternRule Query (TileTypes[] adjacents)
		{
			if (Rules == null) return new PatternRule (DefaultIndex, 0);
			
			// iterate rules to find one that best fits and return that sub-model index.
			for (int i = 0; i < Rules.Length; i++)
			{
				// first rule that matches we exit
				// - first get the map for this rule
				//   - compare the map against the actual adjacents values
				PatternCondition[] conditions = Rules[i].Conditions;
				if (conditions == null) continue;
				
				for (int j = 0; j < conditions.Length; j++)
				{
					TileDirections dir = conditions[j].Direction;
										
					if (adjacents[(int)dir] != conditions[j].Type)
						// break inner for{} and resume next Rule in outer for{}
						break;
					
					// Rule has passed since this was last condition and we're still here
					// so return that PatternRule
					if (j == conditions.Length - 1)
					{
						if (dir == TileDirections.U)
						{
							//System.Diagnostics.Debug.WriteLine ("Up...");
						}
						return Rules[i];
					}
				}
			}
			
			// still here, then return the default PatternRule 
			return new PatternRule (DefaultIndex, 0);
		}
	}
}
