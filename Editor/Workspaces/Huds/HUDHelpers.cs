using System;
using System.Collections.Generic;
using KeyCommon.Flags;
using KeyEdit.Workspaces.Tools;
using Keystone.Appearance;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Extensions;
using Keystone.Hud;
using Keystone.Resource;
using Keystone.TileMap;
using Keystone.Types;

namespace KeyEdit.Workspaces.Huds
{
	/// <summary>
	/// Description of HUDHelpers.
	/// </summary>
	public class HUDHelpers
	{

        #region Tool Preview Visuals Helper Classes
    	private class SelectionToolPreview : HUDToolPreview
    	{
    		KeyEdit.Workspaces.Tools.SelectionTool mTool;
    		// TODO: if this preview managed it's own entites/models/meshes
    		//       it could be easier to manage their cleanup, but the gridentity needs to be shared by multiple preview object types
    		ModeledEntity mGridEntity;	
        	int mPreviousMouseOverXOffset;
        	int mPreviousMouseOverZOffset;
        	int mPreviousMouseOverColor= -1;
        	int mMouseOverAtlasIndex;
        	
        	public SelectionToolPreview (KeyEdit.Workspaces.Tools.SelectionTool tool, 
        	                             ModeledEntity gridEntity, 
        	                             int mouseOverAtlasIndex,
        	                             HUDAddElement_Immediate addImmediateHandler, 
        	                             HUDAddRemoveElement_Retained addRetainedHandler, 
        	                             HUDAddRemoveElement_Retained removeRetainedHandler)
        		: base (addImmediateHandler, addRetainedHandler, removeRetainedHandler )
    		{
    			
        		mTool = tool;
    			mGridEntity = gridEntity;
    			mMouseOverAtlasIndex = mouseOverAtlasIndex ;
    		}
    		
    		// use a strategy pattern.... ?
    		public override void Preview (RenderingContext context, Keystone.Collision.PickResults pick)
    		{
    			if (pick == null || pick.HasCollided == false ) return;
    			if (pick.Entity == null) return;
    			
    			// if the picked entity is a Structure, then we can find the obstacle layer
    			if (pick.Entity is Keystone.TileMap.Structure == false) return;
    			
    			Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)pick.Entity;
    			
    			// bounds test
	            //if (tileLocationX >= mPreviousFloorFootprint.GetLength(0)) return;
	            //if (tileLocationZ >= mPreviousFloorFootprint.GetLength(1)) return;
	
	            int tileLocationX =  pick.TileLocation.X;
	            int tileLocationZ = pick.TileLocation.Z;
	            int floorLevel = 0;
	            // convert mouse tile location to bitmap pixel location since rendering the pixels as texture
	            // causes the z axis to be flipped longitudinally.  If we never render them, then reversing the z is never necessary
	            // TODO: but how can we get the layer dimensions from structure without requiring KeyEdit.exe to have access to internal MapLayer class.
	            int tileCountX, tileCountZ; 
	           
	            structure.Layer_GetDimensions(floorLevel, "obstacles", out tileCountX, out tileCountZ);
	
	            int reverseZ = tileCountZ - 1 - tileLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
	
	            // cache previous mouse over colors so we can restore/Clear() in subsequent frame after the preview is rendered
	            mPreviousMouseOverColor = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixel(tileLocationX, reverseZ); 
	
	            // paint just the mouse over tile
	            mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel(tileLocationX, reverseZ, mMouseOverAtlasIndex);
	
	            mPreviousMouseOverXOffset = tileLocationX;
	            mPreviousMouseOverZOffset = reverseZ;
    		}
    		    		
    		public override void Clear ()
    		{
    			if (mPreviousMouseOverColor == -1) return;
	            mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel(mPreviousMouseOverXOffset,
                                                                      mPreviousMouseOverZOffset, 
                                                                      mPreviousMouseOverColor);
    			mPreviousMouseOverColor = -1;
    			
    		}
    	}
    	
        private class InteriorPlacementToolPreview : PlacementPreview
    	{
    		// TODO: if this preview managed it's own entites/models/meshes
    		//       it could be easier to manage their cleanup, however passing in the
    		//       gridEntity means it can be shared between various other preview types
    		//       for different tools (eg pathing tool, link painting, etc)
    		ModeledEntity mGridEntity;	
			
			
		    int mPreviousFootprintXOffset;
	        int mPreviousFootprintZOffset;
	        int mPreviousFootprintWidth;
	        int mPreviousFootprintHeight;
	        int[] mPreviousFootprintColors;
        	int[] mAtasIndexLookup;
        	
        	        	
        	public InteriorPlacementToolPreview (Keystone.EditTools.PlacementTool tool, 
        	                                     ModeledEntity gridEntity, 
        	                                     ModeledEntity sourceEntity, 
        	                                     int[] atasIndexLookup,
    		                         			HUDAddElement_Immediate addImmediateHandler, 
    		                         			HUDAddRemoveElement_Retained addRetainedHandler, 
    		                         			HUDAddRemoveElement_Retained removeRetainedHandler)
        		: base (tool, sourceEntity, addImmediateHandler, addRetainedHandler, removeRetainedHandler)
    		{

    			mGridEntity = gridEntity;
    			mAtasIndexLookup = atasIndexLookup;
    			
    			// note: base() contructor call creates our override materials
    		}
    		

    		public override void Preview (RenderingContext context, Keystone.Collision.PickResults pick)
    		{	
//    			// TODO: can some of this just be done in the Terrain shader when we enable "show obstacle mask" shader parameter?
//    			
//    			// TODO: i think during preview, we should always check the target in the pick result
//    			//       rather than having it fixed to celledRegion since we need to work across multiple zones
//    			//       and if the target is not a valid placement type (eg is component and not structure floor\terrain) 
//    			//       then we show red "invalid position" material
//    			
//    			if (pick.HasCollided == false || pick.Entity == null) return;
//    		
//    			if (pick.Entity is Keystone.TileMap.Structure == false) return;
//    			
//    			Keystone.TileMap.Structure  structure = (Keystone.TileMap.Structure)pick.Entity;
//    			
//    			// 1) we want to draw the preview footprint on the tilemask grid at the mouse pick location on this structure
//    			// if we're on terrain or floor structure, we will show them there.  If it's 
//    			// 2) we want to draw using data from the obstacle layer  
//    			// 3) 
//    			
//    			mSource.Translation = mTool.ComponentTranslation; // NOTE: CameraSpace is done for 3DHUD items in Hud.cs.Render() but not (yet) for 2d items.  I might change that
//	            System.Diagnostics.Debug.Assert(mSource.Dynamic == false);
//	            mSource.Rotation = mTool.ComponentRotation;
//	            
//	            //  NOTE: recall that our mSource entity is a cloned copy of the one in the placement tool
//    			//  Neither of them are placed into final scene. By cloning the
//	            //  source in the Tool we can use our own preview material such as transparent green or red
//	            System.Diagnostics.Debug.Assert (mSource != mTool.Source);
//	            
//	            byte componentRotationIndex = mTool.ComponentRotation.GetComponentYRotationIndex();
//                
//                bool hasValidPlacement = structure.IsChildPlaceableWithinInterior(mSource, pick.TileLocation, componentRotationIndex); // TODO: pick.TileLocation used to be AssetTool.MouseOverTile so beware if this has now changed the correct value
//	            
//	            Material materialOverride = mOverrideMaterial;
//	            // do we override the normal placement material with invalid placement material instead?
//	            if (hasValidPlacement == false)
//	                materialOverride = mInvalidPlacementMaterial;
//	
//	            if (materialOverride != null)
//	            {
//	                SetHudPreviewMaterial(mSource, materialOverride);
//	            }
//                    
//
//	            if (pick != null && pick.Entity is Structure)
//	            {
//	            	Structure celledRegion = (Structure)pick.Entity;
//	            	
//	            	mAddImmediateHandler(celledRegion, mSource, false);
//            	}
//	            else 
//	            	mAddImmediateHandler (context.Region, mSource, false);
//
//	            
//
//                // paint the footprint's preview drop location
//                if (mSource.Footprint != null)
//                {
//                	int[,] footprint = mSource.Footprint.Data; 
//                	
//                    int atlasIndex = mAtasIndexLookup[ATLAS_INDEX_VALID];
//                    if (hasValidPlacement == false)
//                        atlasIndex = mAtasIndexLookup[ATLAS_INDEX_INVALID];
//
//                    // this version of preview footprint requires tilemask grid
//                    int tileCountZ = (int)structure.TileCountZ;
//		            int tileCountX = (int)structure.TileCountX;
//		
//		            int[,] rotatedFootprint = TileDataVisualizer.GetRotatedFootprint(footprint, componentRotationIndex);
//		
//		            // reverse the footprint data to map it propertly with our texture which has y =  reverse (z)
//		            // TODO: i think this reversal is actually unnecessary
//		            rotatedFootprint = TileDataVisualizer.ReverseFootprintZ(rotatedFootprint);
//		
//		            int width = rotatedFootprint.GetLength(0);
//		            int height = rotatedFootprint.GetLength(1);
//		
//		            int originX, originZ;
//		            structure.GetDestinationOriginFromTileLocation(pick.TileLocation, footprint, out originX, out originZ);
//		
//		            // determine the max width or height based on whether this footprint is partially out of bounds
//		            int maxWidth =  width;
//		            int maxHeight =+ height;
//		
//		            maxWidth = Math.Min(width, maxWidth);
//		            maxHeight = Math.Min(height, maxHeight);
//		
//		            if (maxWidth < 0 || maxHeight < 0) return;
//		
//		            //if (originX < 0) originX = maxWidth;
//		            //if (originZ < 0) originZ = maxHeight;
//		
//		            // TODO: here would be nice if we could short width and/or height to deal with out of bounds partial footprints
//		            // this is something we can do during preview but not final footprint apply
//		            int[] colors = new int[maxWidth * maxHeight];
//		
//		            // fill the colors in reverse since our pixels are reversed from the data
//		            for (int i = 0; i < maxWidth; i++)
//		                for (int j = 0; j < maxHeight; j++)
//		                {
//		                    colors[j * maxWidth + i] = ATLAS_INDEX_DEFAULT;
//		                    if (rotatedFootprint[i, j] != 0)
//		                        colors[j * maxWidth + i] = atlasIndex;
//		
//		                }
//		
//		            int reverseZ = tileCountZ - 1 - originZ; // - 1 because we're dealing with a 0 based index
//		            reverseZ -= height - 1; // - 1 again because we want to include the current reverseZ row and not exclude it when we shift
//		
//		            // System.Diagnostics.Debug.WriteLine(string.Format("PreviewFootprint() - X = {0} Z = {1}", x, reverseZ));
//		
//		            // cache what is underneath the footprint so we can restore it when the preview is moved
//		            mPreviousFootprintWidth = maxWidth;
//		            mPreviousFootprintHeight = maxHeight;
//		            mPreviousFootprintXOffset = originX;
//		            mPreviousFootprintZOffset = reverseZ;
//		
//		            // cache block of pixel values at specified location so they can be restored
//		            int[] tmp = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixelArray(originX, reverseZ, maxWidth, maxHeight);
//		            if (tmp != null)
//		            {
//		                mGridEntity.Model.Appearance.Layers[1].Texture.SetPixelArray(originX, reverseZ, maxWidth, maxHeight, colors);
//		                mPreviousFootprintColors = tmp;
//		            }
//                }
    		}
    		    		
    		public override void Clear ()
    		{
    			if (mPreviousFootprintColors == null) return;

            	mGridEntity.Model.Appearance.Layers[1].Texture.SetPixelArray(mPreviousFootprintXOffset, mPreviousFootprintZOffset,
                                                                            mPreviousFootprintWidth, mPreviousFootprintHeight, 
                                                                            mPreviousFootprintColors);
            	mPreviousFootprintColors = null;
    			
    		}

    	}
    	
    	private class LinkPlacementToolPreview : HUDToolPreview 
    	{
    		InteriorLinkPainter mTool;
	        Dictionary <uint, int> mPreviousLinkTileState;

	        ModeledEntity mGridEntity;
	        int mLinkTileColor;
	        
	        public LinkPlacementToolPreview (InteriorLinkPainter tool, ModeledEntity tileMaskGrid, int linkTileColor,
	                                        HUDAddElement_Immediate addImmediateHandler, 
    		                         HUDAddRemoveElement_Retained addRetainedHandler, 
    		                         HUDAddRemoveElement_Retained removeRetainedHandler)
	        	: base (addImmediateHandler, addRetainedHandler, removeRetainedHandler)
	        {
	        	
	        	mTool = tool;
	        	mGridEntity = tileMaskGrid;
	        	mLinkTileColor = linkTileColor;
	        }
	        
	        
	        // TODO: when previewing, i really should disable floor tiles.
	        //       - and/or should raise the tiles above floor height so they are visible above floor
	        //		- make them transparent, currently they are not
	        //      - get our auto-gen footprint working again and rendering 
	        //      	- ability to reposition a power plant
	        //			- ability to scale it and have the footprint re-scale
			public override  void Preview(RenderingContext context, Keystone.Collision.PickResults pick)
			{
//				if (mTool.PreviewTiles == null || mTool.PreviewTiles.Length == 0) return; 
//				
//				mPreviousLinkTileState = new Dictionary<uint, int> (mTool.PreviewTiles.Length);
//				
//				// TODO: here we ned to cache as previousStructure, previousMouse, etc so we can clear
//				//       the preview from last frame
//				
//	        	int tileCountZ = (int)mCelledRegion.TileCountZ;
//	        		        
//				// TODO: mTool.PreviewTiles must be of the same picked structure and not a different zone's structure	        	
//	        	for (int i = 0; i < mTool.PreviewTiles.Length ; i++)
//	        	{
//	        		uint tile = mTool.PreviewTiles[i];
//	        		uint tileLocationX, tileLocationY, tileLocationZ;
//	        		mCelledRegion.UnflattenTileIndex (tile, out tileLocationX, out tileLocationY, out tileLocationZ);
//	
//	        		int reverseZ = tileCountZ - 1 - (int)tileLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
//	            
//	        		// cache previous tile colors so we can restore in subsequent frame after the preview is rendered
//	        		int color = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixel((int)tileLocationX, reverseZ);
//	        		mPreviousLinkTileState.Add (tile, color);
//	        		
//	        	    // update just the current preview tile
//	        	    mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel((int)tileLocationX, reverseZ, mLinkTileColor);
//	        	}
			} 
			
			public override void Clear()
			{
//				if (mPreviousLinkTileState == null || mPreviousLinkTileState.Count == 0) return;
//				int tileCountZ = (int)mCelledRegion.TileCountZ;
//				
//
//				foreach (uint tile in mPreviousLinkTileState.Keys)
//				{
//					uint tileLocationX, tileLocationY, tileLocationZ;
//					mCelledRegion.UnflattenTileIndex (tile, out tileLocationX, out tileLocationY, out tileLocationZ);
//	
//	        		int reverseZ = tileCountZ - 1 - (int)tileLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
//	        		
//	        		mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel((int)tileLocationX, reverseZ, mPreviousLinkTileState[tile]);
//				}
//				
//				mPreviousLinkTileState.Clear ();
			} 
    	}
        #endregion
        
	}

    public class TileDataVisualizer 
    {

        private struct DataFrame
        {
            public uint XOffset;   // note: there is no YOffset, since each Level represented by a TileDataVisualizer already is inherently specifying it's YOffset
            public uint ZOffset;
            public int Data;
        }
   

        string mLayerName;
        int mTileCountX;
        int mTileCountZ;
        float mGridWidth;
        float mGridDepth;
        
		int mLayerSubscriptX;
		int mLayerSubscriptY;
		int mLayerSubscriptZ;
		
        private Keystone.TileMap.Structure mStructure;
    	private ModeledEntity mTileMaskGrid; // used for footprints including drawing of links (eg. power lines)
		// TODO: we can use an atlas that is a bit more like a gradiant with higher values
		//       going from gray to black but with discrete tile colors, not a continuous gradianet
		Keystone.Appearance.TextureAtlas mTileMaskAtlas;
		int[] mAtasIndexLookup;
        int mTileMaskTextureIndex;
        // used to cache changes to a specific map layer between Update() calls.
        // this way we can apply changes to our HUD tilemaskgrid during Update() and keep
        // good seperation between input and scene modifications
        List<DataFrame> mMaskChanges;
        
        public TileDataVisualizer (Keystone.TileMap.Structure structure, string layerName, int[,] data, int layerSubscriptX, int layerSubscriptY, int layerSubscriptZ, int tileCountX, int tileCountZ, float gridWidth, float gridDepth)
        {
        	mLayerName = layerName;
        	
        	int lookupSize = 256;
        	mAtasIndexLookup = InitializeAtlasIndexLookUp(lookupSize);
        	
        	mStructure = structure;
        	mTileCountX = tileCountX;
        	mTileCountZ = tileCountZ;
        	mGridWidth = gridWidth;
        	mGridDepth = gridDepth;
        	
        	mLayerSubscriptX = layerSubscriptX;
        	mLayerSubscriptY = layerSubscriptY;
        	mLayerSubscriptZ = layerSubscriptZ;
        	
        	string geometryNoShareTag = "visualization mesh: " + Keystone.TileMap.Structure.GetStructureID (layerSubscriptX, layerSubscriptY, layerSubscriptZ);
        	mTileMaskGrid = LoadTileMaskGrid( tileCountX, tileCountZ, gridWidth, gridDepth, geometryNoShareTag);
        	
        	// initialize the grid's texture with the Layer data.  We could grab it from MapGrid[,,]
            // applying the pixel array costs an additional 227 fps so we don't use this for real time update just init!
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            System.Diagnostics.Debug.Assert (width == mTileCountX);
            System.Diagnostics.Debug.Assert (height == mTileCountZ);
            
            int[] colors = new int[width * height];


            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    // TODO: this is wrong to reverse in this method.. data should already be passed in reversed if such a thing is necessarry
                    int jReversed = height - 1 - j; // - 1 because we're 0 based index not 1

                    colors[j * width + i] = GetAtlasLookupColor ((byte)data[i, jReversed]);
                }

            mTileMaskGrid.Model.Appearance.Layers[1].Texture.SetPixelArray(0, 0, width, height, colors);
            
            
//            // Collapsed Cells Update - only update collapsed cell vertices when we changed floors or the designated collapsed cells have changed
//            int hashCode = celledRegion.Layer_GetHashCode("boundaries");
//            if (hashCode != mLastTileMaskVisualizerCellCollapseStateHashCode || mTileMaskGrid.Translation.y != translation.y)
//            {
//            	// Our tilemask visualizer is built of a single 4 vertex quad combined with a SetPixelArray()
//            	// texture that stores our atlas texture lookup indices for our shader.
//           		// UpdateCollapsableGridMeshState (); <-- WRONG.
//           		
//           		// thus if we want to collapse a part of the tilemask, we simply want to assign a -1 and that
//           		// will instruct the shader to not draw anything there.
//           		
//            	mLastTileMaskVisualizerCellCollapseStateHashCode = hashCode ;
//            }
        }
        
        public ModeledEntity Visualization {get {return mTileMaskGrid;}}
            
        public Keystone.TileMap.Structure Parent {get {return mStructure;}}

        public int LayerSubscriptX {get {return mLayerSubscriptX;}}
        public int LayerSubscriptY {get {return mLayerSubscriptY;}}
        public int LayerSubscriptZ {get {return mLayerSubscriptZ;}}
        
        #region IObserver Members
        // Tile data layer changes handler 

        public void AddChange(int x, int z, int value)
        {
        	DataFrame frame = new DataFrame();
            frame.Data = value; // footprint;
            frame.XOffset = (uint)x;
            frame.ZOffset = (uint)z;
            

            if (mMaskChanges == null) mMaskChanges = new List<DataFrame>();
            mMaskChanges.Add(frame);
            // NOTE: rather than directly call UpdateTileMaskVisualizer here
            //       we just add the DataFrame to a list so we can
            //       process it during normal HUD update rather than allow the event to
            //       interrupt

        }
        #endregion
        
		/// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Grid is dynamically positioned off the Root.  This works because 
        /// in design floorplan mode, the target floorplan cannot be moved from orign.
        /// We only have to raise or lower the grid to match the floor.
        /// </remarks>
        /// <param name="celledRegion"></param>
        public void Update(Vector3d position)
        {
            if (mTileMaskGrid == null) return;
            
            float Z_FIGHTING_EPSILON = float.Epsilon * 2;
            // POSITION
          	// the target is a celled region and all regions are at origin of their own coordinate system
            // recall that it is only the PlayerVehicle that moves, not the interior CelledRegion
            // so this translation will always be 0,0,0.
            // And further, whenever we render an interior specific view, we always keep everything
            // relative to an interior at origin.  Even if the ship rotates, what we'll see is the universe
            // outside rotating about the interior.
            Vector3d translation = position; // should always be origin
            
            // take into account current floor selected in the toolbar/menu/settings
            translation.y += Z_FIGHTING_EPSILON; // NOTE: ground floor we stand on starts at -2.8 so that the tops are at 0.0f; // TODO: hardcoded hack //mStructure.GetFloorHeight((uint)context.Floor);
            // make the grid a tiny bit lower than bottom of the cell to avoid z-fighting with a floor tile
    //        translation.y += Z_FIGHTING_EPSILON ;  // disable for now since plan is to hide floor when grid is being displayed

            mTileMaskGrid.Translation = translation;
          
            // TODO: need parameters for the texel half width and half height which changes if the dimensions of the texture file changes
            // NOTE: LoadTVResourceSynchronously (mTileMaskGrid) call above guarantees shader is loaded in time before attempting to set values
            //       but even if not, i think we apply the values as soon as shader is loaded
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("TileCountX", (float)mTileCountX); // value type must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("TileCountZ", (float)mTileCountZ); // value type must match type that is declared in shader
            // NOTE: AtlasTextureCountX and AtlasTextureCountY are #of atlas tiles along each dimension and these images within the atlas can be > 1 pixel wide and high!
            //       but they should be multiples of 2.
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureCountX", 8.0f); // Multiple's of 2 - value type must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureCountY", 1.0f); // value type must match type that is declared in shader
			mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureWidth", 128.0f); // value type must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureHeight", 16.0f); // value type must match type that is declared in shader

            // update just changes in mask value display
            ApplyChanges (mLayerName, mTileCountX, mTileCountZ); 
        }

        
        /// <summary>
        /// Changes the underlying pixels in the changed rectangular area of the bitmap 
        /// that the grid uses as it's texture.
        /// </summary>
        /// <param name="structure"></param>
        internal void ApplyChanges(string layerName, int tileCountX, int tileCountZ)
        {
        	
            // get the list of changed data, but we are interested
            // only in the "footprint" layer
            if (mMaskChanges == null || mMaskChanges.Count == 0) return;

            //structure.Layer_GetValue (mLayerIndex, layerName, tileID);
            
            
            // TODO: our HUD tileMaskGrids.  We can have one grid for each
            //       zone we are over.  This way we only have to change the
            //       ones that actually change and not all of them.
            for (int n = 0; n < mMaskChanges.Count; n++)
            {
                DataFrame frame = mMaskChanges[n];
                
                if (frame.Data is int)
                {
                	int reverseZ = (int)(tileCountZ - 1 - frame.ZOffset); // -1 because we're dealing with a 0 based index
                	int color = GetAtlasLookupColor ((byte)frame.Data);
	                mTileMaskGrid.Model.Appearance.Layers[1].Texture.SetPixel((int)frame.XOffset, reverseZ, color);
                }
//                else if (frame.Data is int[])
//                {
//	                int[,] reverseData = ReverseFootprintZ(frame.Data);
//	                int width = reverseData.GetLength(0);
//	                int height = reverseData.GetLength(1);
//	                int[] colors = new int[width * height];
//	
//	                for (int i = 0; i < width; i++)
//	                    for (int j = 0; j < height; j++)
//	                    {	
//	                		colors[j * width + i] = GetAtlasLookupColor ((byte)reverseData[i, j]);
//	                    }
//	
//	                //System.Diagnostics.Debug.WriteLine("UpdateTileMaskVisualizer() - X = {0} Y = {1}", frame.XOffset, frame.ZOffset);
//	
//	
//	                int reverseZ = (int)(tileCountZ - 1 - frame.ZOffset); // -1 because we're dealing with a 0 based index
//	                // we have to subtract height of the footprint because the reversal means that the top's and bottoms are reversed
//	                // and this means that the amount to reverse isn't just TilesPerCellZ but rather the height of the target footprint
//	                reverseZ -= height - 1; // - 1 again because we want to include the current reverseZ row and not exclude it when we shift
//	
//	                //System.Diagnostics.Debug.WriteLine("UpdateTileMaskVisualizer() - X = {0} ReverseZ = {1}", frame.XOffset, reverseZ);
//	                
//	                mTileMaskGrid.Model.Appearance.Layers[1].Texture.SetPixelArray((int)frame.XOffset, reverseZ, width, height, colors);
//                }
            }
            
            mMaskChanges.Clear();
        }
        
        internal static int[,] ReverseFootprintZ(int[,] footprint)
        {
            if (footprint == null) throw new ArgumentNullException();

            int width = footprint.GetLength(0);
            int height = footprint.GetLength(1);

            int[,] result = new int[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    int reverseJ = height - 1 - j; // -1 because we're dealing with a 0 based index
                    result[i, j] = footprint[i, reverseJ];
                }

            return result;
        }
        
        // http://gamedev.stackexchange.com/questions/38118/best-way-to-mask-2d-sprites-in-xna
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFootprint">The already rotated, footprint of the component we wish to place.</param>
        /// <param name="destFootprint">The footprint or portion of footprint that we will compare with source footprint.
        /// The dest footprint's dimensions must be equal to or greater than the respective dimensions of the source and 
        /// must already be aligned to same coordinate space</param>
        /// <param name="mask"></param>
        /// <returns></returns>
        private bool FootprintCollides(int[,] sourceFootprint, int[,] destFootprint)
        {
            int sourceWidth = sourceFootprint.GetLength(0);
            int sourceHeight = sourceFootprint.GetLength(1);
            int destWidth = destFootprint.GetLength(0);
            int destHeight = destFootprint.GetLength(1);

            // if any index of the source with or without the offset applied will be out of bounds
            // within the destination, return as collision == true.
            if (sourceWidth > destWidth || sourceHeight > destHeight) return true;

            for (int x = 0; x < sourceWidth; x++)
                for (int z = 0; z < sourceHeight; z++)
                {
                    // here we test if the source and dest have any flags the same, if so that is a collision
                    // since two components cannot set the same flags on the destination
                    if ((sourceFootprint[x, z] & destFootprint[x, z]) != 0)
                        return true;
                }

            return false;
        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sourceFootprint">The already rotated, footprint of the component we wish to place.</param>
//        private static bool FootprintCollides(int x, int y, int z, int[,] sourceFootprint)
//        {
//            if (sourceFootprint == null) return false;
//            int sourceWidth = sourceFootprint.GetLength(0);
//            int sourceHeight = sourceFootprint.GetLength(1);
//
//            if (sourceWidth == 0 || sourceHeight == 0) return false;
//
//            // footpring with and height must be even divisible by 2 
//            // warning: wait this is not (yet) necessarily so when we are rotating!
//            //if (sourceWidth % 2 != 0 && sourceHeight % 2 != 0) throw new ArgumentOutOfRangeException();
//
//            if (mCellMap.TilesAreInBounds(x, y, z, sourceWidth, sourceHeight) == false) return true;
//
//            int[,] destFootprint = mCellMap.GetFootprint((uint)x, (uint)y, (uint)z, (uint)sourceWidth, (uint)sourceHeight);
//
//            bool result = FootprintCollides(sourceFootprint, destFootprint);
//            //System.Diagnostics.Debug.WriteLine("CelledRegion.IsChildPlaceable() - " + result.ToString());
//
//            return result;
//        }

        public static int[,] GetRotatedFootprint(int[,] sourceFootprint, byte rotation)
        {
            switch (rotation)
            {
                case 32:  // 45 degrees
                    sourceFootprint = ArrayExtensions.RotateTheta(sourceFootprint, Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * 45d);
                    break;
                case 64:  // 90 degrees 
                    sourceFootprint = ArrayExtensions.Rotate90(sourceFootprint);
                    break;
                case 96:  // 135 degrees
                    sourceFootprint = ArrayExtensions.RotateTheta(sourceFootprint, Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * 135d);
                    break;
                case 128:  // 180 degrees 
                    sourceFootprint = ArrayExtensions.Rotate180(sourceFootprint);
                    break;
                case 160: // 225 degrees
                    sourceFootprint = ArrayExtensions.RotateTheta(sourceFootprint, Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * 225d);
                    break;
                case 192: // 270 degrees
                    sourceFootprint = ArrayExtensions.Rotate270(sourceFootprint);
                    break;
                case 224: // 315 degrees
                    sourceFootprint = ArrayExtensions.RotateTheta(sourceFootprint, Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * 315d);
                    break;
                case 0:   // 0 or 360 degrees
                default:
                    break;
            }

            return sourceFootprint;
        }
        
        private int[] InitializeAtlasIndexLookUp(int size)
        {
        	
            float r = 0, g = 0, b = 0, a = 1.0f;
            int[] lookup = new int[size];
            for (int i = 0; i < lookup.Length; i++)
            {
                r = i / 255f;
                if (i == 0)
                	a = 0.0f;
                else a = 1.0f; // if using alpha, make sure the texture we create has an alpha channel
                
                lookup[i] = AppMain._core.Globals.RGBA(r, g, b, a);
            }

            return lookup;
        }
                
		/// <summary>
        /// Based on the flags set for this tile and the tool in use, determine which atlas index to use
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private int GetAtlasLookupColor (byte weight)
        {
        	int result = mAtasIndexLookup[weight]; 
			
	        return result;
        }
        
        // NOTE: This only makes the visual representation of the tilemaskgrid
        // so that we can edit it in real time.  The underlying mask array however
        // is completely seperate and thus server side takes up only a fraction of the memory
        // this does.  It could very well be still hwoever that we are only able to have a few
        // user ships in memory, but in a single player procedural adventure simulation maybe
        // this is not a big problem... still id like to have some small level of multiplayer
        // both coop and pvp
        internal ModeledEntity LoadTileMaskGrid(int tileCountX, int tileCountZ, float gridWidth, float gridDepth, string geometryNoShareTag)
        {       	
        	// one grid we can use for all view modes (eg footprints, power lines, network cables, 
            // ventilation, etc)

            // NOTE: we use 1, 1 for instead of tileCountX and tileCountZ because our Atlas
            // shader will automatically divide the single large quad into tiny tiles
            Keystone.Elements.Mesh3d gridMesh = Keystone.Elements.Mesh3d.CreateCellGrid(gridWidth, gridDepth, 1, 1, 1.0f, false, geometryNoShareTag);

            gridMesh.TextureClamping = true; // // TODO: TextureClamping should be assignable via Appearance TODO: is this necessary for achieving good lines between textures?  This should be part of appearance though, set by appearance on the geometry
            gridMesh.Name = "tile mask grid mesh"; // friendly name
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            // create an apeparance and add a test texture
            string diffuse = System.IO.Path.Combine(AppMain.DATA_PATH, @"\editor\masktiles_frontcolor.png"); // masktiles_mono.png"; //masktiles.png"; // Grayscale_8bits_palette.png // colourReplaceRamp.jpg
            diffuse = System.IO.Path.Combine(AppMain.DATA_PATH, @"\editor\masktiles_icons1.png"); 
            diffuse = System.IO.Path.Combine (AppMain.DATA_PATH,  @"editor\masktiles_solid.png");
            //diffuse = @"E:\dev\c#\KeystoneGameBlocks\Data\editor\colorlookup.png"; // colourReplaceRamp.jpg";
            string shader = @"caesar\shaders\atlas.fx";
            Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, shader, diffuse, "", "", "");
            appearance.RemoveMaterial();

            string tempTexture = Keystone.IO.XMLDatabase.GetTempFileName();
            // create unique texture that we wont be sharing
            AppMain._core.TextureFactory.SetTextureMode(MTV3D65.CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
            mTileMaskTextureIndex = AppMain._core.TextureFactory.CreateTexture(tileCountX, tileCountZ, false, tempTexture); 
            
            // normal map slot will contain the diffuse actually, the atlas lookup is set in diffuse slot!  i should swap those.
            //AppMain._core.TextureFactory.SaveTexture(rsTextureIndex, tempTexture);
            Keystone.Appearance.NormalMap n = (NormalMap)Keystone.Resource.Repository.Create("NormalMap");
            Keystone.Appearance.Texture tex = Texture.Create (mTileMaskTextureIndex); // (Texture)Keystone.Resource.Repository.Create (?, "Texture");
            n.AddChild (tex);
            appearance.AddChild(n);


            //AtlasTextureTools.AtlasRecord[] records = AtlasTextureTools.AtlasParser.Parse(@"F:\Downloads\half_tiles_textures\floor_textures_atlas.png.tai");
            //Keystone.Appearance.TextureAtlas atlas =
            //    Keystone.Appearance.TextureAtlas.Create(
            //        @"F:\Downloads\half_tiles_textures\floor_textures_atlas.png0.dds",
            //        records);
            
            //// we'll use a premade texture atlas
            //// Edge Artifacts - If you are using Mimpapping you will need to use GL_NEAREST_MIPMAP_LINEAR 
            //// and also duplicate the edge pixels
            //// https://developer.nvidia.com/sites/default/files/akamai/tools/files/Texture_Atlas_Whitepaper.pdf
            //mTileMaskAtlas = Keystone.Appearance.TextureAtlas.Create(@"E:\dev\c#\KeystoneGameBlocks\Data\editor\masktiles.png");
            //mTileMaskAtlas.AddSubTexture(0.000000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.125000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.250000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.375000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.500000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.625000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.750000f, 0.0f, 0.0f, 0.125f, 1.0f);
            //mTileMaskAtlas.AddSubTexture(0.875000f, 0.0f, 0.0f, 0.125f, 1.0f);

            //Appearance.Material mat = Appearance.Material.Create(Appearance.Material.DefaultMaterials.red_fullbright);
            //appearance.AddChild(mat);
            // NOTE: id's should always be unique in every Entity created in any HUD because
            //       Huds are PER VIEWPORT and our workspace may have multiple HUDs
            string id = Repository.GetNewName(typeof(ModeledEntity));
            mTileMaskGrid = new ModeledEntity(id);
            mTileMaskGrid.Dynamic = false;
            mTileMaskGrid.Enable = true;
            mTileMaskGrid.Overlay = false; // we still want items placed on floorplan to be rendered ontop of this grid, or for tilemask maybe not..
            mTileMaskGrid.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
            mTileMaskGrid.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
            mTileMaskGrid.Serializable = false;
            model.AddChild(appearance);
            model.AddChild(gridMesh);
            mTileMaskGrid.AddChild(model);

           
            // NOTE: Floorplan's mTileMaskGrid uses Immediate NOT Retained so we must IncrementRef
            Repository.IncrementRef(mTileMaskGrid);
            Keystone.IO.PagerBase.QueuePageableResourceLoad (mTileMaskGrid, null, true);
            
            return mTileMaskGrid;
        }
        
        public void Dispose()
        {
        	if (mTileMaskGrid != null)
	        	Repository.DecrementRef (mTileMaskGrid);
        }
	}
}
