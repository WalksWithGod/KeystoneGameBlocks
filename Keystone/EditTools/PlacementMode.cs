using System;


namespace Keystone.EditTools
{
    /// <summary>
    /// Placement mode describes the state of the Tool and exist irrespective
    /// of what type of Entity is attempting to be placed.  If an entity is incompatible
    /// with the placement mode, then a "not allowed" GUI "X" invalid grpahic or message
    /// of some kind should be generated.
    /// </summary>
    [Flags]
    public enum PlacementMode
    {
    	MouseHitLocation = 0,
    	UseExistingPreviewEntityPosition = 1 << 0,
    	
    	StructureTile = 1 << 1,
    	StructureEdge = 1 << 2,
    	
        EXTERIOR_GENERAL, // <-- default case
        EXTERIOR_BUILD, // <-- attach weapons to hardpoints
        // I think that we must be in either 
        INTERIOR_BUILD, // <-- allows editing of walls, floorplan layout
        INTERIOR_EDIT // <-- allows some interior walls to be removed, btu mostly only allows refits and remodels
        //       - so any component we select, if we're in Interior Build/Edit the component
        //         must be queried via Validate perhaps to discover if this component can be
        //         used at all in this mode, and if so, then what type of placement rules does it use
        //         e.g wall placement v floor tile placement v door v non-resizable component vs resizeable component
        //
    }
    
    
            	//
        	// enum PlacementMode
			// {        	
        	// - zero
        	// - existingPreviewEntityTranslation
        	// - water
        	//  MouseHitTerrain, 
        	// 	CenterFOV 
        	// }
}
