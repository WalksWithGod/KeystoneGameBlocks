using System;
using System.Collections.Generic;
using KeyCommon.Flags;
using KeyCommon.Traversal;
using KeyEdit.Workspaces.Tools;
using Keystone.Appearance;
using Keystone.Cameras;
using Keystone.EditTools;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Hud;
using Keystone.Portals;
using Keystone.Resource;
using Keystone.Types;

namespace KeyEdit.Workspaces.Huds
{
    public class FloorplanHud : ClientHUD
    {
        private bool mDrawIcons = false;

    	#region Tool Preview Visuals Helper Classes
    	private class SelectionToolPreview : HUDToolPreview
    	{
    		KeyEdit.Workspaces.Tools.SelectionTool mTool;
    		// TODO: if this preview managed it's own entites/models/meshes
    		//       it could be easier to manage their cleanup, but the gridentity needs to be shared by multiple preview object types
    		Interior mCelledRegion;
    		ModeledEntity mGridEntity;	
        	int mPreviousMouseOverXOffset;
        	int mPreviousMouseOverZOffset;
        	int mPreviousMouseOverColor= -1;
        	int mMouseOverAtlasIndex;
        	
        	public SelectionToolPreview (KeyEdit.Workspaces.Tools.SelectionTool tool, Interior celledRegion, ModeledEntity gridEntity, int mouseOverAtlasIndex, 
        	                             HUDAddElement_Immediate addImmediateHandler, 
        	                             HUDAddRemoveElement_Retained addRetainedHandler, 
        	                             HUDAddRemoveElement_Retained removeRetainedHandler)
        		: base (addImmediateHandler, addRetainedHandler, removeRetainedHandler )
    		{
    			
        		mTool = tool;
        		mCelledRegion = celledRegion;
    			mGridEntity = gridEntity;
    			mMouseOverAtlasIndex = mouseOverAtlasIndex ;
    		}
    		
    		// use a strategy pattern.... ?
    		public override void Preview (RenderingContext context, Keystone.Collision.PickResults pick)
    		{
    			if (pick == null || pick.HasCollided == false ) return;
    			// bounds test
	            //if (tileLocationX >= mPreviousFloorFootprint.GetLength(0)) return;
	            //if (tileLocationZ >= mPreviousFloorFootprint.GetLength(1)) return;
	
	            int cellLocationX =  pick.CellLocation.X;
	            int cellLocationZ = pick.CellLocation.Z;
	            	
                // TODO: why do I compute tileCOuntZ and reverseZ that uses it, but do not compute tileLocationX from cellLocationX * TilesPerCellX?
	            int tileCountZ = (int)mCelledRegion.TilesPerCellZ * (int)mCelledRegion.CellCountZ;
	
	            int reverseZ = tileCountZ - 1 - cellLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
	
	            // cache previous mouse over colors so we can restore in subsequent frame after the preview is rendered
	            mPreviousMouseOverColor = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixel(cellLocationX, reverseZ); 
	
	            // paint just the mouse over tile
	            mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel(cellLocationX, reverseZ, mMouseOverAtlasIndex);
	
	            mPreviousMouseOverXOffset = cellLocationX;
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

        private class WallPlacementPreview : HUDToolPreview
        {
            KeyEdit.Workspaces.Tools.WallSegmentPainter mTool;
            // TODO: if this preview managed it's own entites/models/meshes
            //       it could be easier to manage their cleanup, but the gridentity needs to be shared by multiple preview object types
            Interior mCelledRegion;


            public WallPlacementPreview(KeyEdit.Workspaces.Tools.WallSegmentPainter tool, Interior celledRegion, 
                                         HUDAddElement_Immediate addImmediateHandler,
                                         HUDAddRemoveElement_Retained addRetainedHandler,
                                         HUDAddRemoveElement_Retained removeRetainedHandler)
                : base(addImmediateHandler, addRetainedHandler, removeRetainedHandler)
            {

                mTool = tool;
                mCelledRegion = celledRegion;

            }

            public override void Preview(RenderingContext context, Keystone.Collision.PickResults pick)
            {
                // get the prevCells from the tool and scale the wall preview to match
                if (pick != null && pick.Entity is Interior)
                {
                    Interior celledRegion = (Interior)pick.Entity;
                    System.Diagnostics.Debug.Assert(celledRegion == mCelledRegion);


                    WallSegmentPainter tool = (WallSegmentPainter)mTool;

                    uint[] edges = tool.Edges;
                    if (edges == null || edges.Length < 1) return;

   
                    Vector3d position = celledRegion.GetEdgePosition(edges[0]);
                    Vector3d rotationRadians = celledRegion.GetEdgeRotation(edges[0]);

                    Vector3d scale = new Vector3d(1d, 3d, 1d);
                    if (edges.Length == 1)
                    {
                        position.y += 3d * 0.5d;
                        if (rotationRadians.y  == 0)
                            scale.x = 0.125d;
                        else
                            scale.z = 0.125d;
                        tool.Source.Translation = position;
                        
                    }
                    else
                    {
                        Vector3d position2 = celledRegion.GetEdgePosition(edges[edges.Length - 1]);
                        rotationRadians = celledRegion.GetEdgeRotation(edges[edges.Length - 1]);
                        // position determined by orientation of the edges
                        Vector3d posTemp = (position2 - position ) / 2d;
                        posTemp += position;
                        double halfHeight = 3d * 0.5d;
                        posTemp.y = position.y + halfHeight;

                        if (position2.z == position.z)
                        {
                            // vertical oriented wall
                            posTemp.z = position2.z;
                            scale.x = 0.125d;
                            scale.z *= edges.Length;// (position2.x - position.x);
                        }
                        else
                        {
                            // horizontally oriented wall
                            posTemp.x = position2.x;
                            scale.z = 0.125d;
                            scale.x *= edges.Length; // (position2.z - position.z);
                        }

                        tool.Source.Translation = posTemp;
                        
                        //scale.z = position2.z - position.z;
                       // NOTE: we no longer do Cell pruning since upgrading to a preview wall placement graphic rather than a realtime wall placement on MouseMove()
                    }


                    // if the first and last edges are not on the same column or row, we do not render
                    // or the edges from tool.Edges even generated if the rows or colums are not the same for all edges in the array?

                    // NOTE: yaw, pitch, roll ctor is broken.  Use rotationMatrix instead.
                    // tool.Source.Rotation = new Quaternion(rotationRadians.y, rotationRadians.x, rotationRadians.z);

                    Matrix tmp = Matrix.Rotation(Vector3d.Up(), 90d * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS, new Vector3d(0, 0, 0));
                    tool.Source.Rotation = new Quaternion(tmp);
                    tool.Source.Scale = scale;
                    
                    tool.Source.Visible = true;
                    System.Diagnostics.Debug.Assert (tool.Source.TVResourceIsLoaded);

                    // TODO: if the tool.Source.Model.Appearance.Material is green, turn it red for invalid placement. But when do we know it's invalid?
                    mAddImmediateHandler(celledRegion, (ModeledEntity)tool.Source, false);
                }
            }

            private bool IsEdgeHorizontal()
            {
                return true;
            }
        }

        private class TilePlacementPreview : HUDToolPreview
        {
            KeyEdit.Workspaces.Tools.InteriorSegmentPainter mTool; 
            // TODO: if this preview managed it's own entites/models/meshes
            //       it could be easier to manage their cleanup, but the gridentity needs to be shared by multiple preview object types
            Interior mCelledRegion;
            int mFloor;

            public TilePlacementPreview(KeyEdit.Workspaces.Tools.InteriorSegmentPainter tool, Interior celledRegion, int floor,
                                         HUDAddElement_Immediate addImmediateHandler,
                                         HUDAddRemoveElement_Retained addRetainedHandler,
                                         HUDAddRemoveElement_Retained removeRetainedHandler)
                : base(addImmediateHandler, addRetainedHandler, removeRetainedHandler)
            {

                mTool = tool;
                mCelledRegion = celledRegion;
                
            }

            public override void Preview(RenderingContext context, Keystone.Collision.PickResults pick)
            {

                // get the prevCells from the tool and scale the wall preview to match

                InteriorSegmentPainter tool = mTool;
                uint[] cells = tool.Cells;
                if (cells == null || cells.Length == 0) return;


                // source for this tool is a cell grid. The scale is 1,1,1
                tool.Source.Scale = new Vector3d(1, 1, 1);

                if (pick != null && pick.Entity is Interior)
                {
                    Interior celledRegion = (Interior)pick.Entity;


                    for (int i = 0; i < cells.Length; i++)
                    {
                        // TODO: collapse all cells except the ones we need
                        // TODO:
                        bool collapsed = celledRegion.IsCellCollapsed(cells[i]);
                       
                    }
                    mAddImmediateHandler(celledRegion, (ModeledEntity)tool.Source, false);
                }
            }
        }



    	private class InteriorPlacementToolPreview : PlacementPreview
    	{
    		// TODO: if this preview managed it's own entites/models/meshes
    		//       it could be easier to manage their cleanup
    		Interior mCelledRegion;
    		ModeledEntity mGridEntity;
            ModeledEntity mDirectionIndicator;
			
		    int mPreviousFootprintXOffset;
	        int mPreviousFootprintZOffset;
	        int mPreviousFootprintWidth;
	        int mPreviousFootprintHeight;
	        int[] mPreviousFootprintColors;
        	int[] mAtasIndexLookup;
        	
        	        	
        	public InteriorPlacementToolPreview (Keystone.EditTools.TransformTool tool, Interior celledRegion, ModeledEntity directionIndicator, ModeledEntity gridEntity, ModeledEntity sourceEntity, int[] atasIndexLookup,
    		                         HUDAddElement_Immediate addImmediateHandler, 
    		                         HUDAddRemoveElement_Retained addRetainedHandler, 
    		                         HUDAddRemoveElement_Retained removeRetainedHandler)
        		: base (tool, sourceEntity, addImmediateHandler, addRetainedHandler, removeRetainedHandler)
    		{

    			mCelledRegion = celledRegion;
    			mGridEntity = gridEntity;
                mDirectionIndicator = directionIndicator;
    			mAtasIndexLookup = atasIndexLookup;
    			
    			// note: base() contructor call creates our override materials
    		}
    		
            // TODO: if the mSource is the same as our target, and then we are immediate mode rendering it,
            //       is that a problem?  Because we're rendering it twice.  And further, we are changing it's appearance material color
            //       and not restoring it.
    		public override void Preview (RenderingContext context, Keystone.Collision.PickResults pick)
    		{
                System.Diagnostics.Debug.Assert(mSource.Dynamic == false);
                mSource.Translation = mTool.ComponentTranslation; // NOTE: CameraSpace is done for 3DHUD items in Hud.cs.Render() but not (yet) for 2d items.  I might change that
	            mSource.Rotation = mTool.ComponentRotation;

                // TODO: Regarding below NOTE, for InteriorTransformTool, the mSource is NOT a clone but the actual component in the Container.
                //       Maybe I should create a sepeprate Preview class for it.

                //  NOTE: recall that our mSource entity is a cloned copy of the one in the placement tool
                //  Neither of them are placed into final scene. By cloning the
                //  source in the Tool we can use our own preview material such as transparent green or red
                //	            System.Diagnostics.Debug.Assert (mSource != mTool.Source);
                bool isExistingComponent = mSource == mTool.Source;
                int[,] destFootprint = null;
                bool hasValidPlacement = mCelledRegion.IsChildPlaceableWithinInterior(mSource, mSource.Translation, mSource.Rotation, out destFootprint); // TODO: pick.TileLocation used to be AssetTool.MouseOverTile so beware if this has now changed the correct value


                // default green valid placement material
                Material materialOverride = mOverrideMaterial;
                // do we override the normal placement material with red invalid placement material instead?
                if (hasValidPlacement == false)
                    materialOverride = mInvalidPlacementMaterial;

                // For existing components that already exist in the SceneGraph
                // changing the Appearance (Material/Textures) while rendering will 
                // cause an exception in TVMesh.Render(). NOTE: actually this might
                // have been caused by a bug in Material.Create() that resulted in creation
                // of a new material every frame instead of retrieving the cached version
                // in Repository

                if (isExistingComponent == false && materialOverride != null)
                {
                    SetHudPreviewMaterial(mSource, materialOverride);
                }


                // existing components are already in the SceneGraph and do not
                // need to be added via ImmediateHandler
                if (isExistingComponent == false)
                {
                    if (pick != null && pick.Entity is Interior)
                    {
                        Interior celledRegion = (Interior)pick.Entity;

                        mAddImmediateHandler(celledRegion, mSource, false);

                        // position and draw direction indicator.  BonedEntities do not need this as they do not have a footprint
                        if (mSource.Footprint != null)
                        {
                            Vector3d translation = mDirectionIndicator.Model.Translation;
                            // compute the z offset of the direction indicator Model and allow Entity hierarchy to perform the final rotation
                            translation.z = -(mSource.Footprint.Depth / celledRegion.TilesPerCellZ * (celledRegion.CellSize.z * 0.5d));
                            mDirectionIndicator.Model.Translation = translation;
                            mDirectionIndicator.Translation = mSource.Translation;
                            mDirectionIndicator.Rotation = mSource.Rotation;
                            mAddImmediateHandler(celledRegion, mDirectionIndicator, false);
                        }
                    }
                    else
                        mAddImmediateHandler(context.Region, mSource, false); // todo: i dont think we want this for Interipr placement.  Parent must be Interior.
                }
	            

                // paint the footprint's preview drop location
                if (mSource.Footprint != null)
                {
                	int[,] footprint = mSource.Footprint.Data; 
                	
                    int atlasIndex = mAtasIndexLookup[ATLAS_INDEX_OBSTACLE];
                    if (hasValidPlacement == false)
                        atlasIndex = mAtasIndexLookup[ATLAS_INDEX_INVALID];

                    // this version of preview footprint requires tilemask grid
                    int tileCountZ = (int)mCelledRegion.TilesPerCellZ * (int)mCelledRegion.CellCountZ;
		            int tileCountX = (int)mCelledRegion.TilesPerCellX * (int)mCelledRegion.CellCountX;
		
		            int[,] rotatedFootprint = mCelledRegion.GetRotatedFootprint(footprint, mSource.Rotation);
		
		            // reverse the footprint data to map it propertly with our texture which has y =  reverse (z)
		            // TODO: i think this reversal is actually unnecessary
		            rotatedFootprint = ReverseFootprintZ(rotatedFootprint);
		
		            int width = rotatedFootprint.GetLength(0);
		            int height = rotatedFootprint.GetLength(1);
		
		            // determine the max width or height based on whether this footprint is partially out of bounds
		            int maxWidth =  width;
		            int maxHeight =+ height;
		
		            maxWidth = Math.Min(width, maxWidth);
		            maxHeight = Math.Min(height, maxHeight);
		
		            if (maxWidth < 0 || maxHeight < 0) return;
		
                    // TODO: since switching to GetTileSnapPosition() im not checking the bounds of startTileLocation and making sure they are valid
                    //       but i think if the startLocation is out of bounds of the texture, it just gracefully fails via TV3D.
		            //if (originX < 0) originX = maxWidth;
		            //if (originZ < 0) originZ = maxHeight;
		
		            // TODO: here would be nice if we could short width and/or height to deal with out of bounds partial footprints
		            // this is something we can do during preview but not final footprint apply
		            int[] colors = new int[maxWidth * maxHeight];
		
		            // fill the colors in reverse since our pixels are reversed from the data
		            for (int i = 0; i < maxWidth; i++)
		                for (int j = 0; j < maxHeight; j++)
		                {
                            // TODO: these index colors should match the ones we use
                            //       on the floorplan, plus, here we're not even using anything
                            //       but the DEFAULT index.
                            colors[j * maxWidth + i] = atlasIndex; // colors[ATLAS_INDEX_OBSTACLE];
                            if (rotatedFootprint[i, j] != 0)
                                colors[j * maxWidth + i] = atlasIndex; // colors[ATLAS_INDEX_OBSTACLE];
		
		                }

                    Vector3i startTileLocation;
                    // todo: if mSource.Translation is already at a snap position, we shouldn't alter it.  verify the values are the same before and after
                    mCelledRegion.GetTileSnapPosition(mSource.Footprint.Data, mSource.Translation, mSource.Rotation.GetComponentYRotationIndex(), out startTileLocation);

                    int reverseZ = tileCountZ - 1 - startTileLocation.Z; // - 1 because we're dealing with a 0 based index
		            reverseZ -= height - 1; // - 1 again because we want to include the current reverseZ row and not exclude it when we shift
		
		            // System.Diagnostics.Debug.WriteLine(string.Format("PreviewFootprint() - X = {0} Z = {1}", x, reverseZ));
		
		            // cache what is underneath the footprint so we can restore it when the preview is moved
		            mPreviousFootprintWidth = maxWidth;
		            mPreviousFootprintHeight = maxHeight;
		            mPreviousFootprintXOffset = startTileLocation.X;
		            mPreviousFootprintZOffset = reverseZ;
		
		            // cache block of pixel values at specified location so they can be restored next frame
		            int[] tmp = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixelArray(startTileLocation.X, reverseZ, maxWidth, maxHeight);
		            if (tmp != null)
		            {
		                mGridEntity.Model.Appearance.Layers[1].Texture.SetPixelArray(startTileLocation.X, reverseZ, maxWidth, maxHeight, colors);
		                mPreviousFootprintColors = tmp;
		            }

                    
                }
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
	        Interior mCelledRegion;
	        ModeledEntity mGridEntity;
	        int mLinkTileColor;
	        
	        public LinkPlacementToolPreview (InteriorLinkPainter tool, Interior celledRegion, ModeledEntity tileMaskGrid, int linkTileColor,
	                                        HUDAddElement_Immediate addImmediateHandler, 
    		                         HUDAddRemoveElement_Retained addRetainedHandler, 
    		                         HUDAddRemoveElement_Retained removeRetainedHandler)
	        	: base (addImmediateHandler, addRetainedHandler, removeRetainedHandler)
	        {
	        	
	        	mTool = tool;
	        	mCelledRegion = celledRegion ;
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
				if (mTool.PreviewTiles == null || mTool.PreviewTiles.Length == 0) return; 
				
				mPreviousLinkTileState = new Dictionary<uint, int> (mTool.PreviewTiles.Length);
				
	        	int tileCountZ = (int)mCelledRegion.TilesPerCellZ * (int)mCelledRegion.CellCountZ;
	        		        		        	
	        	for (int i = 0; i < mTool.PreviewTiles.Length ; i++)
	        	{
	        		uint tile = mTool.PreviewTiles[i];
	        		uint tileLocationX, tileLocationY, tileLocationZ;
	        		mCelledRegion.UnflattenTileIndex (tile, out tileLocationX, out tileLocationY, out tileLocationZ);
	
	        		int reverseZ = tileCountZ - 1 - (int)tileLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
	            
	        		// cache previous tile colors so we can restore in subsequent frame after the preview is rendered
	        		int color = mGridEntity.Model.Appearance.Layers[1].Texture.GetPixel((int)tileLocationX, reverseZ);
	        		mPreviousLinkTileState.Add (tile, color);
	        		
	        	    // update just the current preview tile
	        	    mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel((int)tileLocationX, reverseZ, mLinkTileColor);
	        	}
			} 
			
			public override void Clear()
			{
				if (mPreviousLinkTileState == null || mPreviousLinkTileState.Count == 0) return;
				int tileCountZ = (int)mCelledRegion.TilesPerCellZ * (int)mCelledRegion.CellCountZ;
				

				foreach (uint tile in mPreviousLinkTileState.Keys)
				{
					uint tileLocationX, tileLocationY, tileLocationZ;
					mCelledRegion.UnflattenTileIndex (tile, out tileLocationX, out tileLocationY, out tileLocationZ);
	
	        		int reverseZ = tileCountZ - 1 - (int)tileLocationZ; // -1 because 0 based index and tileCountZ is a 1 based count
	        		
	        		mGridEntity.Model.Appearance.Layers[1].Texture.SetPixel((int)tileLocationX, reverseZ, mPreviousLinkTileState[tile]);
				}
				
				mPreviousLinkTileState.Clear ();
			} 
    	}
		#endregion	

    	
        private FloorPlanDesignWorkspace mFloorplanWorkspace;
        private float Z_FIGHTING_EPSILON = 0.01f;

        private ModeledEntity mTileMaskGrid; // used for footprints including drawing of links (eg. power lines)
        private ModeledEntity mLegalInteriorGrid;
        private ModeledEntity mMouseTileMarker;
        private ModeledEntity mYardStick; // TODO: yardstick should be moved to InteriorWallPainterToolPreview. TODO: yardstick height should be set to wall height which is currently 3.0meters

        private const int ATLAS_INDEX_COMPONENT = 3;
        private const int ATLAS_INDEX_OBSTACLE = 7;        
        private const int ATLAS_INDEX_DEFAULT = 6;
        private const int ATLAS_INDEX_ENTRANCE_AND_EXIT = 6;
        private const int ATLAS_INDEX_VALID = 5;
        private const int ATLAS_INDEX_INVALID = 4;
        private const int ATLAS_INDEX_WALL = 7;
		private const int ATLAS_INDEX_POWERLINE = 1;
        private const int ATLAS_INDEX_ACCESSSIBLE_DEST_UPPER = 2;
        private const int ATLAS_INDEX_ACCESSSIBLE_ORIGIN_LOWER = 0;

        int[] mAtasIndexLookup = new int[8];		
		Keystone.Appearance.TextureAtlas mTileMaskAtlas;
        int mTileMaskTextureIndex;
        List<CellMap.DataFrame> mMaskChanges;
                
        int mLastInteriorCellCollapseStateHashCode = 0;


        public FloorplanHud(FloorPlanDesignWorkspace workspace) : base()
        {
            if (workspace == null) throw new ArgumentNullException();
            
            mFloorplanWorkspace = workspace;
            if (mFloorplanWorkspace.Vehicle.Interior == null)
            {
                System.Windows.Forms.MessageBox.Show("Vehicle Container does not contain an Interior.");
                throw new ArgumentException();
            }
            Interior celledRegion = (Interior)mFloorplanWorkspace.Vehicle.Interior;


            InitializePickParameters();
            
            
            celledRegion.Database.Attach(OnDatabaseChanged);
            
            // gui visualizers
            mAtasIndexLookup = InitializeAtlasIndexLookUp();
            LoadLegalInteriorGrid (celledRegion);
            LoadTileMaskGrid(celledRegion);
            LoadMouseTileMarker(celledRegion);
            LoadPrefabDirectionIndicator(celledRegion);
        }


        private void InitializePickParameters()
        {
            mPickParameters = new PickParameters[3];

            // KeyCommon.Flags.EntityFlags.ExteriorRegion
            KeyCommon.Flags.EntityAttributes excludedObjectTypes = 
                KeyCommon.Flags.EntityAttributes.Background;

            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.ContainerExterior |
                KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Interior_Region;

            // NOTE: use PickSearchType flag PickSearchType.Interior  if we want to limit our search to children of
            //       interior regions.
            PickParameters pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Tile,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[0] = pickParams;

            // selection of assets that have been placed, ignoring interior structure
            // however this makes it such that our Selection tool will not show mouse over tile
            // on the tilemask grid
            // note: we dont want to ignore the Interior because clicking onto floor or wall requires we pick interior
            //       and this allows us to bring up the overall vehicle stats/performance profiles during design mode
            //       TODO: the only concern is that picking components should always come ahead of interior
            ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.ContainerExterior; //  | KeyCommon.Flags.EntityFlags.InteriorRegion;

            pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
            	SearchType = PickCriteria.Closest | PickCriteria.Interior,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Tile | PickAccuracy.TileWithFloor, // cells with floors only! (i.e. cells that are NOT collapsed)
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[1] = pickParams;

            // placing assets onto cells with floor, but picking only against interior
            ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.ContainerExterior |
                KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Interior_Region;

            pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest | PickCriteria.FlooredCell,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Tile | PickAccuracy.TileWithFloor, // NOTE: We will pick either Cell or CellWithFloor but with greater priority on Cell's that have floors since that more fully meets our accuracy criteria
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[2] = pickParams;
        }

        #region IObserver Members
        // CellMap database changes handler 

        public void OnDatabaseChanged(CellMap database, CellMap.DataFrame dataFrame)
        {

            if (mMaskChanges == null) mMaskChanges = new List<CellMap.DataFrame>();
            mMaskChanges.Add(dataFrame);
            mTileMaskVisualizerIsDirty = true;
            // NOTE: rather than directly call UpdateTileMaskVisualizer here
            //       we just add the DataFrame to a list so we can
            //       process it during normal HUD update rather than allow the event to
            //       interrupt

        }
        #endregion


        #region Tool Events
        protected override void OnEditTool_ToolChanged(Tool currentTool)
        {
            Interior celledRegion = (Interior)mFloorplanWorkspace.Vehicle.Interior;
            
            if (currentTool == null || currentTool is SelectionTool)
            {
                mPlacementToolGraphic = mMouseTileMarker;    
            }

            // TODO: all of the following should be moved into ToolPreview's
            // change marker based on the tool in use and it's brush style.
            // the one difference though is these "preview" items (eg the yardstick)
            // use AddHUDEntity_Immediate for rendering
            if (currentTool is PlacementTool)
            {
                mPlacementToolGraphic = null; // use the mLastSourceEntry for preview mesh/actor
            }
            else if (currentTool is KeyEdit.Workspaces.Tools.WallSegmentPainter)
            {
                // TODO: load a box mesh with transparent green material.
                // TODO: if currently mousedown, resize box to fit edge locations.
                //       - the edge locations are loaded into the WallSegmentPainter
            	// TODO: this could be done as part of wallsegmentpainterpreview.
                // TODO: load this box as the previewgraphic as opposed to mPlacementToolGraphic.
                ///      We could maybe have the WallSegmentPainter hosts the previewGraphic and handle the loading of it and resizing of it
                LoadYardStick();
                mPlacementToolGraphic = mYardStick;
            }
            else if (currentTool is KeyEdit.Workspaces.Tools.TileSegmentPainter)
            {
                //LoadTileMarker(celledRegion);
                //mLastMarker = mTileMarker;
                mPlacementToolGraphic = mMouseTileMarker; // for now let's not use the big orange tile marker
            }

        }

        #endregion


        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateBeforeCull(context, elapsedSeconds);
            System.Diagnostics.Debug.Assert (context.Workspace == mFloorplanWorkspace);
            
            Interior celledRegion = (Interior)mFloorplanWorkspace.Vehicle.Interior;
            if (celledRegion == null) return; // interior not generated yet
            
            Keystone.Collision.PickResults pickResult = mFloorplanWorkspace.MouseOverItem;
            
            mPickParameters[0].FloorLevel = context.Floor;
            mPickParameters[1].FloorLevel = context.Floor;
            mPickParameters[2].FloorLevel = context.Floor;

            Keystone.EditTools.Tool currentTool = mFloorplanWorkspace.CurrentTool;

            if (mLastTool != currentTool)
            {
                OnEditTool_ToolChanged(currentTool);
                mLastTool = currentTool;
            }
            
            IHUDToolPreview currentPreviewGraphic = null;
            
            bool placementToolTileMaskVisulizerOverride = false;

            if (currentTool is PlacementTool)
            {

                PlacementTool ptool = (PlacementTool)currentTool;

                // which tool determines which hud elements we will render and determines which picking options we set
                context.PickParameters = mPickParameters[2];

                // generate and assign new preview if the current mToolPreview is not already this one 
                if (mTileMaskGrid != null && ptool.Source != null)
                {
                    if (mPreviewGraphic is InteriorPlacementToolPreview == false)
                    {
                        string cloneID = Repository.GetNewName(ptool.Source.TypeName);
                        bool delayResourceLoading = false; // we need for Entity.DomainObject to load immediately so that we can create footprint in Preview if necessary
                        ModeledEntity clone = (ModeledEntity)ptool.Source.Clone(cloneID, true, false, delayResourceLoading);

                        currentPreviewGraphic = new InteriorPlacementToolPreview(ptool, celledRegion, mDirectionIndicator, mTileMaskGrid, clone, mAtasIndexLookup,
                                                                        AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                    }
                    else
                    {
                        currentPreviewGraphic = mPreviewGraphic;
                    }
                }


                placementToolTileMaskVisulizerOverride = true;
            }
            else if (currentTool is WallSegmentPainter)
            {
                WallSegmentPainter tool = (WallSegmentPainter)currentTool;

                if (tool.Edges != null && tool.Edges.Length > 0)
                {
                    currentPreviewGraphic = new WallPlacementPreview(tool, celledRegion, AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                }
                else
                {
                    currentPreviewGraphic = mPreviewGraphic;
                }
            }
            //else if (currentTool is TileSegmentPainter) // for FLOOR tiles
            //{
            //    TileSegmentPainter painter = (TileSegmentPainter)currentTool;

            //    if (painter.Cells != null && painter.Cells.Length > 0)
            //    {
            //        currentPreviewGraphic = new TilePlacementPreview(painter, celledRegion, AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
            //    }
            //    else
            //    {
            //        currentPreviewGraphic = mPreviewGraphic;
            //    }
            //}
            else if (currentTool is InteriorSegmentPainter) // for BOUNDS_IN and FLOOR tiles
            {
                InteriorSegmentPainter painter = (InteriorSegmentPainter)currentTool;

                if (painter.Cells != null && painter.Cells.Length > 0)
                {
                    currentPreviewGraphic = new TilePlacementPreview(painter, celledRegion, context.Floor, AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                }
                else
                {
                    currentPreviewGraphic = mPreviewGraphic;
                }
            }
            else if (currentTool is InteriorTransformTool)
            {
                // switch to only allowing picking of components that are descended from the interior region
                context.PickParameters = mPickParameters[1];

                InteriorTransformTool tool = (InteriorTransformTool)currentTool;
                if (tool.mClonedSource != null)
                {
                    // render preview only if tool is currently dragging a target entity component
                    if (tool.Dragging)
                    {
                        if (mPreviewGraphic == null)
                        {
                            currentPreviewGraphic = new InteriorPlacementToolPreview(tool, celledRegion, mDirectionIndicator, mTileMaskGrid, tool.mClonedSource, mAtasIndexLookup,
                                                                      AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                        }
                        else
                        {
                            currentPreviewGraphic = mPreviewGraphic;
                        }
                    }
                    else
                    {
                        // nullpreview 
                        currentPreviewGraphic = new NullPreview();
                    }
                }
            }
            // SelectionTool is not used in FloorplanHud.  InteriorTransformTool is used instead.
            //else if (currentTool is SelectionTool)
            //{
            //    // switch to only allowing picking of components that are descended from the interior region
            //    context.PickParameters = mPickParameters[1];
            //    if (mPreviewGraphic is SelectionToolPreview == false && mTileMaskGrid != null)
            //        currentPreviewGraphic = new SelectionToolPreview((SelectionTool)currentTool, celledRegion, mTileMaskGrid, mAtasIndexLookup[1],
            //                                                  AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
            //}
            else if (currentTool is KeyEdit.Workspaces.Tools.InteriorLinkPainter)
            {

                context.PickParameters = mPickParameters[2];

                if (mPreviewGraphic is LinkPlacementToolPreview == false && mTileMaskGrid != null)
                {
                    int lookupIndex = mAtasIndexLookup[2];
                    currentPreviewGraphic = new LinkPlacementToolPreview((KeyEdit.Workspaces.Tools.InteriorLinkPainter)currentTool, celledRegion, mTileMaskGrid, lookupIndex,
                                                                 AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                }
                else
                    currentPreviewGraphic = mPreviewGraphic;

            }
            else
            {
                // nullpreview 
                currentPreviewGraphic = new NullPreview();
                context.PickParameters = mPickParameters[0]; // segment painter of some kind
                
                if (mDrawIcons)
                    // TODO: why only draw icons here?
                    DrawIcons(context);
            }

            
            if (mPreviewGraphic != currentPreviewGraphic && currentPreviewGraphic != null)
            {
            	// dispose existing and assign new if the new one is NOT null, otherwise no change has occurred
            	if (mPreviewGraphic != null)
            	{
            		mPreviewGraphic.Clear();
            		mPreviewGraphic.Dispose();
            	}
          		          	
            }

            mPreviewGraphic = currentPreviewGraphic;

            // NOTE: Order is important.  Clear previews before proceeding with update of tile mask visualizer
            // then update new previews for mouse and component's to be placed footprint last
            if (mPreviewGraphic != null)
            	mPreviewGraphic.Clear();
            

            // 3d textured grid mesh shows entire undefined area in which interior of ship can be defined
            string cellGridVisibilty = (string)this.Context.GetCustomOptionValue (null, "cell grid visibility");
            string tileGridVisibility = (string)this.Context.GetCustomOptionValue (null, "tile grid visibility");
            
            // temporary overrides while placement tool is valid to show the tilemaskgrid and hide floor tiles
            if (placementToolTileMaskVisulizerOverride) 
            {
            	cellGridVisibilty = "none";
            	tileGridVisibility = "mask visualizer";
            	//this.mFloorplanWorkspace.SetVisibleFloor (currentFloor, true);
            	// TODO: how can we temporarily disable rendering of floor and 
            	//       perhaps the things that sit ontop the floor?  maybe keep walls though?
            	//       or maybe without walls its easier to place things without having to rotate the wall?
            	//       or maybe we make the walls transparent so we can see the masks through them?
            	// 		 - maybe we can track use of the override and then will know when the override is no longer
            	//         in effect and when to restore the SetVisibleFloor to previous settings.
            }

            switch (cellGridVisibilty)
            {
                case "legal & illegal":
                    // show legal and illegal, but for illegal, we show a bordered transparent cell
                    UpdateLegalInteriorGrid(celledRegion, (uint)context.Floor, false);
                    mLegalInteriorGrid.Visible = true;
                    break;
                case "legal":
                    // show only legal areas, but if not floored, cannot support tile masks
                    UpdateLegalInteriorGrid(celledRegion, (uint)context.Floor, true);
                    mLegalInteriorGrid.Visible = true;
                    break;
                case "none":
                default:
                    mLegalInteriorGrid.Visible = false;
                    break;
            }


            switch (tileGridVisibility)
            {
        		case "mask visualizer":
	                UpdateTileMaskVisualizer(context, celledRegion, context.Floor);
                    mTileMaskGrid.Visible = true;
            		break;
            	case "none":
            	default:
                    mTileMaskGrid.Visible = false;
            		break;
            }
            
            

              // 2d grid lines may not be useful for interior floorplan view where we have other grids
//            // 2d grid lines (never shown along with the boundaries grid, but this should be set in the options
//            // Update2DGridLines();
                         

            if (mPlacementToolGraphic != null)
            {
                // yardstick or cell floor marker (i think we should make more of an outline type marker for cell floor)
                PositionMarker(celledRegion, currentTool, pickResult, mPlacementToolGraphic);
                AddHUDEntity_Immediate(celledRegion, mPlacementToolGraphic, false);

            }

            // preview for SelectionPreview performs mouse over atlas tile index change
            // so .Preview() must be called after updates to tilemaskvisualizer
            if (mPreviewGraphic != null && pickResult != null)
            	mPreviewGraphic.Preview(context, pickResult);
            
            if (mMaskChanges != null)
                mMaskChanges.Clear();

        }



        public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateAfterCull(context, elapsedSeconds);

            Interior interior = (Interior)mFloorplanWorkspace.Vehicle.Interior;
            if (interior == null) return; // interior not generated yet

#if DEBUG

            DrawWallPlacementEdgeVertices(interior, -context.GetRegionRelativeCameraPosition(interior.ID));

            // TODO: can we clone the item and set our outline shader on it?  if we don't clone it, we cannot replace the previous shader if there is one because
            //       the item is queued as Immediate in a dictionary of other items.
            if (this.mFloorplanWorkspace.MouseOverItem != null && this.mFloorplanWorkspace.MouseOverItem.Entity != null && this.mFloorplanWorkspace.MouseOverItem.Entity is Region == false)
            {
                Keystone.Types.BoundingBox cameraRelativeBox = this.mFloorplanWorkspace.MouseOverItem.Entity.BoundingBox;
                cameraRelativeBox.Translate(-context.GetRegionRelativeCameraPosition(interior.ID));
                context.Hud.AddHUDEntity_Immediate(this.mFloorplanWorkspace.Vehicle.Interior, cameraRelativeBox, Keystone.Types.Color.White.ToInt32());


                // add outline shader and render two times
                //clonedModeledEntity.Translation = offsetTranslation;
                //context.Hud.AddHUDEntity_Immediate(this.mFloorplanWorkspace.Vehicle.Interior, clonedModeledEntity, false);
            }

            // TODO: "delete" allows deletion of Interior! it should only allow deletion of the currently mouse clicked Entity
            if (this.mFloorplanWorkspace.SelectedEntity != null && this.mFloorplanWorkspace.SelectedEntity.Entity != null && this.mFloorplanWorkspace.SelectedEntity.Entity is Region == false)
            {
                Keystone.Types.BoundingBox cameraRelativeBox = this.mFloorplanWorkspace.SelectedEntity.Entity.BoundingBox;
                cameraRelativeBox.Translate(-context.GetRegionRelativeCameraPosition(interior.ID));
                context.Hud.AddHUDEntity_Immediate(this.mFloorplanWorkspace.Vehicle.Interior, cameraRelativeBox, Keystone.Types.Color.Green.ToInt32());
            }

            // draw all mutli-select entities
            // todo: we need to not multiselect entities that don't have a footprint
            if (this.mFloorplanWorkspace.CurrentTool is InteriorTransformTool)
            {
                InteriorTransformTool itransformTool = (InteriorTransformTool)this.mFloorplanWorkspace.CurrentTool;
                if (itransformTool.Selections != null && itransformTool.Selections.Length > 1)
                {
                    for (int i = 0; i < itransformTool.Selections.Length; i++)
                    {
                        Keystone.Types.BoundingBox cameraRelativeBox = itransformTool.Selections[i].BoundingBox;
                        cameraRelativeBox.Translate(-context.GetRegionRelativeCameraPosition(interior.ID));
                        context.Hud.AddHUDEntity_Immediate(this.mFloorplanWorkspace.Vehicle.Interior, cameraRelativeBox, Keystone.Types.Color.Green.ToInt32());
                    }
                }
            }

            bool drawAreas = (bool) context.GetCustomOptionValue(null, "show connectivity");

            if (drawAreas)
            {
                BoundingBox[] boxes = interior.GetConnectivityAreas(context.Floor);
                if (boxes != null)
                {
                    for (int j = 0; j < boxes.Length; j++)
                    {
                        // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                        // so we have to convert those box coords to region space
                        //  boxes[j].Translate(celledRegion.BoundingBox.Width / 2.0, 0, celledRegion.BoundingBox.Depth / 2.0);
                        // to camera space translation.. 
                        boxes[j].Translate(-context.GetRegionRelativeCameraPosition(interior.ID));
                        // boxes[j].Translate(mFloorplanWorkspace.Vehicle.Translation.x, mFloorplanWorkspace.Vehicle.Translation.y, mFloorplanWorkspace.Vehicle.Translation.z);
                        // by parenting to the relevant zone, these 2d Immediate primitvies
                        // will be rendered in their Region space.
                        AddHUDEntity_Immediate(interior, boxes[j], Keystone.Types.Color.Green.ToInt32());

                        // draw debug text for Area index value at center of Area
                        Keystone.Cameras.Viewport vp = AppMain._core.Viewports[this.Context.ID];
                        Vector3d result = vp.Project(boxes[j].Center, vp.Context.Camera.View, vp.Context.Camera.Projection, Matrix.Identity());


                        int left = (int)result.x;
                        int top = (int)result.y;
                        Keystone.Immediate_2D.Renderable2DText text = new Keystone.Immediate_2D.Renderable2DText(j.ToString(), left, top, Keystone.Types.Color.Green.ToInt32(), TextureFontID);
                        AddHUDEntity_Immediate(interior, text); 
                    }
                }
            }

            if (drawAreas)
            {
                BoundingBox[] boxes = interior.GetConnectivityPortals(context.Floor);
                if (boxes != null)
                    for (int j = 0; j < boxes.Length; j++)
                    {
                        // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                        // so we have to convert those box coords to region space
                        //boxes[j].Translate(-celledRegion.BoundingBox.Width / 2.0, 0, -celledRegion.BoundingBox.Depth / 2.0);
                        // to camera space translation
                        boxes[j].Translate(-context.GetRegionRelativeCameraPosition(interior.ID));
                        // by parenting to the relevant zone, these 2d Immediate primitvies
                        // will be rendered in their Region space.
                        AddHUDEntity_Immediate(interior, boxes[j], Keystone.Types.Color.Red.ToInt32());
                    }
            }


            // TODO: this is temporary debug code, but it doesn't work if we have multiple agents pathing.  It's just to test one agent.
            DrawPathPoints(interior, interior.GetFoundPathPoints(), -context.GetRegionRelativeCameraPosition(interior.ID));

            //DrawPathFoundAreas(interior, -context.GetRegionRelativeCameraPosition(interior.ID));

            //DrawPathFoundPortals(interior, -context.GetRegionRelativeCameraPosition(interior.ID));
#endif
        }


        // DEBUG DRAW
        private void DrawWallPlacementEdgeVertices(Interior interior, Vector3d cameraspaceOffset)
        {
            DrawTileLocationSquare(interior, interior.WallPlacementStartTileLocation, cameraspaceOffset, MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_YELLOW);
            DrawTileLocationSquare(interior, interior.WallPlacementEndTileLocation, cameraspaceOffset, MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_YELLOW);


        }

        private void DrawPathFoundAreas(Interior interior, Vector3d cameraSpaceOffset)
        {
            if (interior == null) return;
            MTV3D65.CONST_TV_COLORKEY color = MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_WHITE;

            // TODO: this is debug only.  It does not work with multiple NPCs
            BoundingBox[] boxes = interior.GetFoundAreas();

            if (boxes != null)
                for (int j = 0; j < boxes.Length; j++)
                {
                    // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                    // so we have to convert those box coords to region space
                    //boxes[j].Translate(-celledRegion.BoundingBox.Width / 2.0, 0, -celledRegion.BoundingBox.Depth / 2.0);
                    // to camera space translation
                    boxes[j].Translate(cameraSpaceOffset);
                    // by parenting to the relevant zone, these 2d Immediate primitvies
                    // will be rendered in their Region space.
                    AddHUDEntity_Immediate(interior, boxes[j], (int)color);
                }

        }


        private void DrawPathFoundPortals(Interior interior, Vector3d cameraSpaceOffset)
        {
            BoundingBox[] boxes = interior.GetFoundPathPortals(); // TODO: I should also get the path found portal indices so i can debug issues easier

            if (boxes != null)
                for (int j = 0; j < boxes.Length; j++)
                {
                    // the area boxes are built where all tiles are +x,+y,+z to an origin that is 0,0,0 
                    // so we have to convert those box coords to region space
                    //boxes[j].Translate(-celledRegion.BoundingBox.Width / 2.0, 0, -celledRegion.BoundingBox.Depth / 2.0);
                    // to camera space translation
                    boxes[j].Translate(cameraSpaceOffset);
                    // by parenting to the relevant zone, these 2d Immediate primitvies
                    // will be rendered in their Region space.
                    AddHUDEntity_Immediate(interior, boxes[j], Keystone.Types.Color.White.ToInt32());

                    // draw debug text index
                    Keystone.Cameras.Viewport vp = AppMain._core.Viewports[this.Context.ID];
                    Vector3d result = vp.Project(boxes[j].Center, vp.Context.Camera.View, vp.Context.Camera.Projection, Matrix.Identity());


                    int left = (int)result.x;
                    int top = (int)result.y;
                    int index = interior.PathFoundPortalIndices[j];
                    Keystone.Immediate_2D.Renderable2DText text = new Keystone.Immediate_2D.Renderable2DText(index.ToString(), left, top, Keystone.Types.Color.Green.ToInt32(), TextureFontID);
                    AddHUDEntity_Immediate(interior, text);


                }
        }

        private void DrawPathPoints(Interior interior, Vector3d[] points, Vector3d cameraSpaceOffset)
        {
            if (interior == null || points == null) return;

            MTV3D65.CONST_TV_COLORKEY color;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3i location = interior.TileLocationFromPoint(points[i]);

                color = MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_WHITE;
                if (i == 0 || i == points.Length - 1)
                    color = MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_MAGENTA;

                DrawTileLocationSquare(interior, location, cameraSpaceOffset, color);
            }
        }

        private void DrawTileLocationSquare(Entity parent, Vector3i location, Vector3d cameraSpaceOffset, MTV3D65.CONST_TV_COLORKEY color)
        {
            // The grid entity is not at origin, it is centered on the target component's translation
           // cameraSpaceOffset += mTileMaskGrid.Translation;


            Interior interior = (Interior)mFloorplanWorkspace.Vehicle.Interior;
            System.Diagnostics.Debug.Assert(interior == parent);
            Vector3d[] verts = interior.GetTileVertices((uint)location.X, (uint)location.Y, (uint)location.Z);

            for (int i = 0; i < verts.Length; i++)
                verts[i].y += Z_FIGHTING_EPSILON * 16f;

            // note: the lines are computed in camera space
            Line3d[] lines = new Line3d[4];
            lines[0] = new Line3d(verts[0] + cameraSpaceOffset, verts[1] + cameraSpaceOffset);
            lines[1] = new Line3d(verts[1] + cameraSpaceOffset, verts[2] + cameraSpaceOffset);
            lines[2] = new Line3d(verts[2] + cameraSpaceOffset, verts[3] + cameraSpaceOffset);
            lines[3] = new Line3d(verts[3] + cameraSpaceOffset, verts[0] + cameraSpaceOffset);



            Keystone.Immediate_2D.Renderable3DLines renderable = new Keystone.Immediate_2D.Renderable3DLines(lines, (int)color);

            AddHUDEntity_Immediate(parent, renderable);
        }

        private void DrawIcons(RenderingContext context)
        {
            Keystone.Vehicles.Vehicle vehicle = ((FloorPlanDesignWorkspace)context.Workspace).Vehicle;
            if (vehicle == null) return;

            Interior interior = (Interior)mFloorplanWorkspace.Vehicle.Interior;
            if (interior == null) return;

            Predicate<Entity> match;
            List<Entity> matchResults;

            // all entities that are within the interior and which either is a producer or consumer
            // of power user_constants.Product.Power = 5
            // userconstants dll is available to EXE just not keytone.dll but its in the scripting host i think
           
            uint power = 5;
            match = e =>
            {
                if (e.Script == null) return false;
                // TODO: we should only search for producers since those contain the list of consumers.
                //       if an entity has Consumers != null, then it must be a producer of something
                //if (e.DomainObject.Consumers == null) return false;
                //if (e.DomainObject.Consumers.ContainsKey(power) == false) return false;
                return true;
            };

            // TODO: why is the alpha screwed up for power.png sometimes?  also sometimes for hardpoint in editorhud.
            //       IS IT A DRAW ORDER THING?  Because I think our legal/illegal cell grid uses alpha and is enabled same time
            // TODO: i believe IconizeNonRecursive is rendering in 3d space and i forget why i decided to do this.
            //       im basically rendering these as Billboard3d as i recall.
            string texturepath = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\hud_engine_hardpoint1.png");
            //texturepath = System.IO.Path.Combine (AppMain.D @"E:\dev\artwork\icons\fatcow-hosting-icons-2400\16x16\lightning.png";
            texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\power.png");
            //texturepath = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\hud_fighter_vessel5.png)";
            //texturepath = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\hud_world_1.png");
            matchResults = interior.RegionNode.Query(false, match);
            
            //Color black = new Color (0, 0, 0, 255);
	        Material hardpointMaterial; // = Material.DefaultMaterials.red_fullbright;
            hardpointMaterial = Material.Create (Material.DefaultMaterials.red_fullbright);
	            
            // TODO: we need to clear icons when we switch out of "link" edit mode
            IconizeNonRecursive(context, matchResults.ToArray(), match,
                texturepath, hardpointMaterial, false,
                null, null,
                null, null,
                null);

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



        private int[] InitializeAtlasIndexLookUp()
        {
            float r = 0, g = 0, b = 0, a = 1.0f;
            int[] lookup = new int[8];
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

        // http://stackoverflow.com/questions/6893302/decode-rgb-value-to-single-float-without-bit-shift-in-glsl
		        
//        // for shaders
//        // http://aras-p.info/blog/2009/07/30/encoding-floats-to-rgba-the-final/
//        inline float4 EncodeFloatRGBA( float v ) 
//        {
//  			float4 enc = float4(1.0, 255.0, 65025.0, 160581375.0) * v;
//  			enc = frac(enc);
//  			enc -= enc.yzww * float4(1.0/255.0,1.0/255.0,1.0/255.0,0.0);
//  			return enc;
//		}
//		
//        inline float DecodeFloatRGBA( float4 rgba )
//        {
//  			return dot( rgba, float4(1.0f, 1f/255.0f, 1f/65025.0f, 1f/160581375.0f) );
//		}

		/// <summary>
        /// Based on the flags set for this tile and the tool in use, determine which atlas index to use
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private int GetAtlasLookupColor (int flags, Interior.TILE_ATTRIBUTES visualizer)
        {
        	// intialize default atlas index to return
        	int result = mAtasIndexLookup[0]; // mAtasIndexLookup[ATLAS_INDEX_DEFAULT];

       		if ((flags & (int)Interior.TILE_ATTRIBUTES.BOUNDS_IN) != 0)
       		{
       			result = mAtasIndexLookup[ATLAS_INDEX_VALID];
       			
       			if ((flags & (int)Interior.TILE_ATTRIBUTES.COMPONENT) != 0)
       				result  = mAtasIndexLookup[ATLAS_INDEX_COMPONENT];
                //if ((flags & (int)Interior.TILE_ATTRIBUTES.OBSTACLE) != 0)
                //    result = mAtasIndexLookup[ATLAS_INDEX_OBSTACLE];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.COMPONENT_TRAVERSABLE) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_DEFAULT];
       			// TODO: structure for floor is showing here as wall, it could be because they share "consumer_support" flag
       			// so that is why rather than test for != 0 or NONE we test for == EXACT
       			if ((flags & (int)Interior.TILE_ATTRIBUTES.WALL) == (int)Interior.TILE_ATTRIBUTES.WALL)
	            	result  = mAtasIndexLookup[ATLAS_INDEX_WALL];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.ENTRANCE_AND_EXIT) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_ENTRANCE_AND_EXIT];
	        	if ((flags & (int)Interior.TILE_ATTRIBUTES.LINE_POWER) != 0)
	            	result =  mAtasIndexLookup[ATLAS_INDEX_POWERLINE];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_ACCESSSIBLE_ORIGIN_LOWER];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_ACCESSSIBLE_DEST_UPPER];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_ACCESSSIBLE_ORIGIN_LOWER];
                if ((flags & (int)Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING) != 0)
                    result = mAtasIndexLookup[ATLAS_INDEX_ACCESSSIBLE_DEST_UPPER];

            }
            else 
            {
            	
            }
			// visualize boundaries and obstacles

			// visualize boundaries, obstacles but override for LINK
			
	        return result;
        }
        


        public override void Render(RenderingContext context, List<RegionPVS> regionPVS)
        {
            base.Render(context, regionPVS);
            if (context == null) return;
        }


        #region Position and Update HUD Elements
        private void PositionMarker(Interior celledRegion, Tool tool, Keystone.Collision.PickResults pickResult, ModeledEntity marker)
        {
        	if (pickResult == null) return; 
        	
            Vector3d translation = Vector3d.Zero();

            // compute position of hud entities
            if ((tool is SelectionTool || tool is TileSegmentPainter) && pickResult.FacePoints != null)
            	// mouse over marker is current marker
            	// TODO: for painting tiles, our texture should be different so we know what mode we're in
                translation = (pickResult.FacePoints[0] + pickResult.FacePoints[2]) * .5f;
            else if (tool is KeyEdit.Workspaces.Tools.WallSegmentPainter & pickResult.FacePoints != null)
            {
            	// yardstick is current marker
            	// we are assuming that the picked entity is our GRID entity otherwise .TileVertexIndex is irrelevant
            	//System.Diagnostics.Debug.Assert (pickResult is Celle
                translation = pickResult.FacePoints[pickResult.TileVertexIndex];
            }
                   
            translation.y += Z_FIGHTING_EPSILON * 16;
            if (marker != null)
            {
                marker.Translation = translation;
            }
        }

        Random mRand = new Random();
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Grid is dynamically positioned off the Root.  This works because 
        /// in design floorplan mode, the target floorplan cannot be moved from orign.
        /// We only have to raise or lower the grid to match the floor.
        /// </remarks>
        /// <param name="celledRegion"></param>
        private void UpdateLegalInteriorGrid(Interior celledRegion, uint floor, bool collapseOutBoundsCells)
        {
            if (mLegalInteriorGrid == null) return;
            // the target is a celled region and all regions are at origin of their own coordinate system
            // recall that it is only the PlayerVehicle that moves, not the interior CelledRegion
            // so this translation will always be 0,y,0 where y changes depending on floor/deck level.
            // And further, whenever we render an interior specific view, we always keep everything
            // relative to an interior at origin.  Even if the ship rotates, what we'll see is the universe
            // outside rotating about the interior.
            Vector3d translation = Vector3d.Zero(); // context.Region.Translation; // should always be origin

            translation.y += celledRegion.GetFloorHeight(floor);
            // make the grid a tiny bit lower than bottom of the cell to avoid z-fighting with a floor tile
            translation.y += Z_FIGHTING_EPSILON * 16;
            
            // only update the collapsed cell vertices when we changed floors or the designated collapsed cells have changed
            int hashCode = celledRegion.Layer_GetHashCode("boundaries");
            if (hashCode != mLastInteriorCellCollapseStateHashCode || mLegalInteriorGrid.Translation.y != translation.y)
            {
                //bool test = mRand.Next(2) == 0;
                //collapseOutBoundsCells = test;
            	UpdateCollapsableGridMeshState (celledRegion.Database, celledRegion.CellCountX, celledRegion.CellCountZ, floor, collapseOutBoundsCells,
            	                               (Keystone.Elements.Mesh3d)mLegalInteriorGrid.Model.Geometry);

	            mLastInteriorCellCollapseStateHashCode = hashCode;
            }


            mLegalInteriorGrid.Translation = translation;
            AddHUDEntity_Immediate(celledRegion, mLegalInteriorGrid, false); 
        }

        private void UpdateCollapsableGridMeshState (CellMap map, uint gridWidthX, uint gridDepthZ, uint floor, bool collapseOutBoundsCells, Mesh3d geometry)
        {
 
            for (uint i = 0; i < gridWidthX; i++)
                for (uint j = 0; j < gridDepthZ; j++)
                {
                    // the match function expects i, and j to be in Cells not Tiles
                    // so if this is for our tilemask, we need to convert i and j to Cells
                    uint cellX, cellZ;
                    cellX = i;
                    cellZ = j;

                    // HACK: if this is our tilemaskgrid (which has a width of 304 for the Morena Smuggler) then convert i & j from Tiles to Cells
                    //if (gridWidthX == 304)
                    //{
                    //	cellX = i / 16; // TODO: / 16 is for TileCountX? shouldn't use magic numbers
                    //	cellZ = j / 16;
                    //}

                    bool isDefinedInteriorCell = (bool)map.GetMapValue("boundaries", cellX, floor, cellZ);
                    
                    int atlasTileIndex = 1;
                    if (isDefinedInteriorCell == false)
                        atlasTileIndex = 0;

                    // TODO: i must be able to pass the atlas dimensions or else
                    //       we dont get proper rendering if we use this for multiple grid sizes
                    int atlasDimensionX = 2;
                    int atlasDimensionY = 1;
                    
                    Keystone.Celestial.ProceduralHelper.CellGrid_SetCellUV(gridWidthX, gridDepthZ, i, j, geometry, atlasTileIndex, atlasDimensionX, atlasDimensionY);

                    bool collapse = collapseOutBoundsCells;
                    if (isDefinedInteriorCell)
                        collapse = false;

                    //if (collapseOutBoundsCells)
                    //    System.Diagnostics.Debug.WriteLine("Collapsed " + cellX.ToString() + ", " + cellZ.ToString());
                    Keystone.Celestial.ProceduralHelper.CellGrid_CellSetCollapseState(gridWidthX, gridDepthZ, i, j, geometry, collapse);
                }
        }

        private bool mTileMaskVisualizerIsDirty = true;
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Grid is dynamically positioned off the Root.  This works because 
        /// in design floorplan mode, the target floorplan cannot be moved from orign.
        /// We only have to raise or lower the grid to match the floor.
        /// </remarks>
        /// <param name="celledRegion"></param>
        private void UpdateTileMaskVisualizer(RenderingContext context, Interior celledRegion, int currentFloor)
        {
            if (mTileMaskGrid == null) return;
            if (context.Region == null || celledRegion == null) return;

            
            // POSITION
          	// the target is a celled region and all regions are at origin of their own coordinate system
            // recall that it is only the PlayerVehicle that moves, not the interior CelledRegion
            // so this translation will always be 0,0,0.
            // And further, whenever we render an interior specific view, we always keep everything
            // relative to an interior at origin.  Even if the ship rotates, what we'll see is the universe
            // outside rotating about the interior.
            Vector3d translation = context.Region.Translation; // should always be origin
            System.Diagnostics.Debug.Assert(translation == Vector3d.Zero());

            // take into account current floor
            translation.y += celledRegion.GetFloorHeight((uint)context.Floor);
            // make the grid a tiny bit lower than bottom of the cell to avoid z-fighting with a floor tile
            translation.y += Z_FIGHTING_EPSILON * 16 ;  // disable for now since plan is to hide floor when grid is being displayed

            mTileMaskGrid.Translation = translation;
            

            System.Diagnostics.Debug.Assert(AppMain.INTERIOR_TILE_COUNT_X == celledRegion.TilesPerCellX);
            System.Diagnostics.Debug.Assert(AppMain.INTERIOR_TILE_COUNT_Z == celledRegion.TilesPerCellZ);

            int tileCountX = (int)(celledRegion.TilesPerCellX * celledRegion.CellCountX);
            int tileCountZ = (int)(celledRegion.TilesPerCellZ * celledRegion.CellCountZ);
            float gridWidth = (float)celledRegion.CellSize.x * (float)celledRegion.CellCountX;
            float gridDepth = (float)celledRegion.CellSize.z * (float)celledRegion.CellCountZ;
            
              
            // TODO: need parameters for the texel half width and half height which changes if the dimensions of the texture file changes
            // NOTE: LoadTVResourceSynchronously (mTileMaskGrid) call above guarantees shader is loaded in time before attempting to set values
            //       but even if not, i think we apply the values as soon as shader is loaded
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("TileCountX", (float)tileCountX); // value must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("TileCountZ", (float)tileCountZ); // value must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("QuadWidth", gridWidth); // value must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("QuadDepth", gridDepth); // value must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureCountX", 8.0f); // Multiple's of 2 - value must match type that is declared in shader
            mTileMaskGrid.Model.Appearance.SetShaderParameterValue("AtlasTextureCountY", 1.0f); // value must match type that is declared in shader


            // when floor changes level, we must re-grab the correct slice
            if (mTileMaskVisualizerIsDirty || currentFloor != mLastFloor)
            {
                int[, ,] data = (int[, ,])celledRegion.Layer_GetData("footprint");

                int[,] slice = new int[tileCountX, tileCountZ];
                for (int i = 0; i < tileCountX; i++)
                    for (int j = 0; j < tileCountZ; j++)
                        slice[i, j] = data[i, currentFloor, j];

                // applying the pixel array costs an additional 227 fps so we don't use this for real time update just init!
                int width = slice.GetLength(0);
                int height = slice.GetLength(1);

                int[] colors = new int[width * height];

                // TODO: this mask should depend on the tool in use
            	Interior.TILE_ATTRIBUTES visualizerMask = Interior.TILE_ATTRIBUTES.COMPONENT;
            
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        // TODO: this is wrong to reverse in this method.. data should already be passed in reversed if such a thing is necessarry
                        int jReversed = height - 1 - j; // - 1 because we're 0 based index not 1

                        colors[j * width + i] = GetAtlasLookupColor (slice[i, jReversed], visualizerMask);

                    }

                mTileMaskGrid.Model.Appearance.Layers[1].Texture.SetPixelArray(0, 0, width, height, colors);
                mLastFloor = currentFloor;
            }
            
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
            
            
            // update just changes in mask value display
            ApplyTileMaskChanges (celledRegion);


            mTileMaskVisualizerIsDirty = false;

            // Render
            AddHUDEntity_Immediate(celledRegion, mTileMaskGrid, false);
        }

        
        private void ApplyTileMaskChanges(Interior celledRegion)
        {
        	
            // get the list of changed data, but we are interested
            // only in the "footprint" layer
            if (mMaskChanges == null || mMaskChanges.Count == 0) return;

            int tileCountX = (int)(celledRegion.TilesPerCellX * celledRegion.CellCountX);
            int tileCountZ = (int)(celledRegion.TilesPerCellZ * celledRegion.CellCountZ);

            // TODO: this mask should depend on the tool in use
            Interior.TILE_ATTRIBUTES visualizerMask = Interior.TILE_ATTRIBUTES.LINE_POWER | Interior.TILE_ATTRIBUTES.COMPONENT;
            
            for (int n = 0; n < mMaskChanges.Count; n++)
            {
                CellMap.DataFrame frame = mMaskChanges[n];
                
                int[,] reverseData = ReverseFootprintZ(frame.Data);
                int width = reverseData.GetLength(0);
                int height = reverseData.GetLength(1);
                int[] colors = new int[width * height];

                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {	
                		colors[j * width + i] = GetAtlasLookupColor (reverseData[i, j], visualizerMask);
                    }

                //System.Diagnostics.Debug.WriteLine("UpdateTileMaskVisualizer() - X = {0} Y = {1}", frame.XOffset, frame.ZOffset);


                int reverseZ = (int)(tileCountZ - 1 - frame.ZOffset); // -1 because we're dealing with a 0 based index
                // we have to subtract height of the footprint because the reversal means that the top's and bottoms are reversed
                // and this means that the amount to reverse isn't just TilesPerCellZ but rather the height of the target footprint
                reverseZ -= height - 1; // - 1 again because we want to include the current reverseZ row and not exclude it when we shift

                //System.Diagnostics.Debug.WriteLine("UpdateTileMaskVisualizer() - X = {0} ReverseZ = {1}", frame.XOffset, reverseZ);
                
                mTileMaskGrid.Model.Appearance.Layers[1].Texture.SetPixelArray((int)frame.XOffset, reverseZ, width, height, colors);
            }
        }

        
        
        private void Update2DGridLines(Interior celledRegion, uint currentFloor)
        {
//            // GRID - NOTE: our grid attaches to root for EditorHud and Interior for FLoorplan
//            //        so Grid Update should be done in each overriden Hud.cs implementation
//            // NOTE: Grid.InfiniteGrid = false will move grid with camera.
//            // NOTE: if Grid.AutoScale = true the grid spacing will be based on our distance to the plane of the grid
//            Vector3d gridTranslation = Vector3d.Zero();
//            gridTranslation.y += celledRegion.GetFloorHeight(currentFloor);
//            // make the grid a tiny bit lower than bottom of the cell to avoid z-fighting with a floor tile
//            gridTranslation.y += Z_FIGHTING_EPSILON;
//            //gridTranslation.y = mFloorplanGrid.Translation.y; // mFloorPlanGrid already has zfightingepsilon added
//            // NOTE: for 2d lines our camera space offset must be calculated in the context of the interior regionPVS camera
//            // NOTE: for 3d we compute the camera space in Hud.cs.Render() but for 2d (so far) we do it before we add them to the 2d hud items.  We may change that
//            Vector3d cameraSpaceTranslation = gridTranslation;
//            if (regionPVSList.Count > 1)
//                cameraSpaceTranslation -= regionPVSList[1].RegionRelativeCameraPos;
//            Keystone.Immediate_2D.Renderable3DLines[] gridLines = Grid.Update(cameraSpaceTranslation, true); //mContext._drawAxis);
//            AddHUDEntity_Immediate(celledRegion, gridLines); 
        }
        #endregion

        private ModeledEntity mDirectionIndicator;

        #region Load HUD Elements
        private void LoadPrefabDirectionIndicator(Interior celledRegion)
        {
            if (mDirectionIndicator == null)
            {
                const bool USE_TEXTURE_ONLY = false;

                Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");
                //Keystone.Appearance.Layer layer = (Diffuse)Keystone.Resource.Repository.Create("Diffuse");
                Keystone.Elements.Mesh3d directionMesh = null;

                
                if (USE_TEXTURE_ONLY)
                {
                    //string texturePath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\arrow.png"); 
                    //Texture t = (Texture)Keystone.Resource.Repository.Create(texturePath, "Texture");
                    //layer.AddChild(t);

                    double width = celledRegion.CellSize.x;
                    double depth = celledRegion.CellSize.z;

                    // simple way to create a 1x1 quad mesh that is flat like a pancake
                    //directionMesh =
                    //    Keystone.Elements.Mesh3d.CreateFloor(t,
                    //    (float)width, (float)depth, 1, 1, 1f, 1f);


                    //appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
                    //appearance.AddChild(layer);
                }
                else
                {
                    directionMesh = (Mesh3d)Repository.Create(System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\widgets\axis_arrow.obj"), "Mesh3d");
                    directionMesh.CullMode =(int)MTV3D65.CONST_TV_CULLING.TV_DOUBLESIDED;
                }

                Keystone.Appearance.Material material = Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.blue_fullbright);
                appearance.AddChild(material);

                Model model = new Model(Repository.GetNewName(typeof(Model)));
                double angleRadians = Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * 270d;
                model.Rotation = new Quaternion(new Vector3d(1, 0, 0), angleRadians);
                double scale = 2d; // todo: may need screenspace sizing instead.
                model.Scale = new Vector3d(scale, scale, scale);

                model.AddChild(appearance);
                model.AddChild(directionMesh);

                mDirectionIndicator = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
                mDirectionIndicator.Dynamic = false;
                mDirectionIndicator.Pickable = false;
                mDirectionIndicator.Enable = true;
                mDirectionIndicator.Overlay = true; // i think this should be false, overlay is for things that should be rendered over everything else and in this case, we still want items placed on the floorplan to be ontop of these markers
                mDirectionIndicator.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mDirectionIndicator.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mDirectionIndicator.Serializable = false;
                mDirectionIndicator.AddChild(model);

                mDirectionIndicator.RenderPriority += 1;
                Repository.IncrementRef(mDirectionIndicator);
                Keystone.IO.PagerBase.QueuePageableResourceLoad(mDirectionIndicator, null, true);
            }
        }

        private void LoadMouseTileMarker(Interior celledRegion)
        {
            bool CREATE_TILE_MASK_MARKER = true;

            // note: because a new Hud.cs instance is created for every RenderingContext
            // this is one of the few times when we'd actually want to share an Entity
            // I think.  Only tricky part is with multiple viewports open and potentially
            // viewing different scenes, id have to make sure the current parent is correct
            // (eg current celledRegion) used by marker is correct and if not, set it.
            // One thing we could potentially do is make mMarker a private static var, but
            // grabbing the entity from the repository using a known name should be just as good.
            // Since these markers are shared across all RenderingContext->Hud instances
            // then they should never be unloaded until the app shuts down.
            // Actually, there is no harm in sharing the entities since the underlying
            // textures and meshes do get shared!  Let's just be consistant and never
            // share entities.  We don't have to.
            if (mMouseTileMarker == null)
            {
                // create the mesh
                double width = celledRegion.CellSize.x;
                double depth = celledRegion.CellSize.z;
                
                
                string id = null;
                Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");
                Keystone.Appearance.Layer layer =(Diffuse)Keystone.Resource.Repository.Create("Diffuse");
 				Keystone.Elements.Mesh3d markerMesh = null;
                
                if (CREATE_TILE_MASK_MARKER == false)
                {
                    string texturePath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\MouseOverTile.png"); //MouseOverTileMarker
                    //texturePath = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\hud_fighter_vessel5.png");
                    Texture t = (Texture)Keystone.Resource.Repository.Create(texturePath, "Texture");
                    layer.AddChild (t);
                    
                    // simple way to create a 1x1 quad mesh that is flat like a pancake
                    markerMesh =
                        Keystone.Elements.Mesh3d.CreateFloor(null,
                        (float)width, (float)depth, 1, 1, (int)1, (int)1);
                    

                    appearance.BlendingMode  = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;

                }
                else
                {
                    // we use 0,0 because this is a single tile grid and our startx and startz
                    // for cellgrid parameters are always CENTER coordinates and not top/left cell edge values
                    // this way the cell is constructed in model space at origin of 0,0
                    markerMesh = Keystone.Elements.Mesh3d.CreateCellGrid(
                        (float)celledRegion.CellSize.x / celledRegion.TilesPerCellX,
                        (float)celledRegion.CellSize.z / celledRegion.TilesPerCellZ,
                        celledRegion.TilesPerCellX,
                        celledRegion.TilesPerCellZ,
                        0.75f, false, null);

                    markerMesh.TextureClamping = true;


                    // we'll use a premade texture atlas
                    // Edge Artifacts - If you are using Mimpapping you will need to use GL_NEAREST_MIPMAP_LINEAR 
                    // and also duplicate the edge pixels
                    // https://developer.nvidia.com/sites/default/files/akamai/tools/files/Texture_Atlas_Whitepaper.pdf
                    // mTileMaskAtlas = Keystone.Appearance.TextureAtlas.Create(System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\masktiles.png"));
                    mTileMaskAtlas = (TextureAtlas) Keystone.Resource.Repository.Create(System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\masktiles_icons1.png"), "TextureAtlas");
                    mTileMaskAtlas.AddSubTexture(0.000000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.125000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.250000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.375000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.500000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.625000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.750000f, 0.0f, 0.0f, 0.125f, 1.0f);
                    mTileMaskAtlas.AddSubTexture(0.875000f, 0.0f, 0.0f, 0.125f, 1.0f);

                    
                    layer.AddChild(mTileMaskAtlas);
                }

                markerMesh.Name = "marker_overlay";

                

                Keystone.Appearance.Material material = Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.matte);
                appearance.AddChild(material);
                appearance.AddChild(layer);
                
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                model.AddChild(appearance);
                model.AddChild(markerMesh);
                
                mMouseTileMarker = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
                mMouseTileMarker.Dynamic = false;
                mMouseTileMarker.Pickable = false;
                mMouseTileMarker.Enable = true;
                mMouseTileMarker.Overlay = true; // i think this should be false, overlay is for things that should be rendered over everything else and in this case, we still want items placed on the floorplan to be ontop of these markers
                mMouseTileMarker.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mMouseTileMarker.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mMouseTileMarker.Serializable = false;
                mMouseTileMarker.AddChild(model);
                
                Repository.IncrementRef(mMouseTileMarker);
                Keystone.IO.PagerBase.QueuePageableResourceLoad (mMouseTileMarker, null, true);
            }
        }

        private void LoadYardStick()
        {
            if (mYardStick == null)
            {

                mYardStick = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
            	                
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                Keystone.Elements.Mesh3d yardStickMesh = (Mesh3d)Repository.Create(System.IO.Path.Combine(AppMain._core .DataPath, @"editor\widgets\axis.obj"), "Mesh3d");
                model.Scale = new Vector3d(1.0, 3.0, 1.0);

               	Keystone.Appearance.DefaultAppearance app = (DefaultAppearance)Repository.Create("DefaultAppearance");
                Keystone.Appearance.Material yellowMaterial = Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.yellow_fullbright);
                app.AddChild(yellowMaterial);

                model.AddChild(yardStickMesh);
                model.AddChild(app);
           
                mYardStick.AddChild(model);
                mYardStick.Enable = true;
                mYardStick.Pickable = false;
                mYardStick.Overlay = true; // yardstick we do want overlay = true
                mYardStick.Dynamic = false;
                //mYardStick.UseFixedScreenSpaceSize = true;
                // mYardStick.ScreenSpaceSize = .1f;
                
                mYardStick.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mYardStick.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mYardStick.Serializable = false;

                // NOTE: We increment regardless of whether we grabbed the existing copy from Repository
                // because a second instance of the placement tool may be disposing still while this
                // new placementtool instance is activated so it could result in race condition with
                // it's deref.
                // NOTE: increment the ref count to keep it from falling out of scope.
                // NOTE: We only ever have to increment just the top most entity 
                // not each child.
                Repository.IncrementRef(mYardStick);
                Keystone.IO.PagerBase.QueuePageableResourceLoad (mYardStick, null, true);
            }
        }

        /// <summary>
        /// Grid is shared for all floors and all modes (eg interior inbounds only or undefined bounds as well), 
        /// but the grid is very large and the user does not have to use up
        /// every tile when designing their floorplans.
        /// </summary>
        /// <param name="celledRegion"></param>
        private void LoadLegalInteriorGrid(Interior celledRegion)
        {
            if (mLegalInteriorGrid == null)
            {
            	// TODO: since these objects are added to Immediate, they never get EntityAttached and
            	//       since not deserialized, they never get delayResourceLoading = false so these
            	//       resources wont ever get loaded if we dont do it explicitly.
            	
                // load the necessary gui resource and entity to represent our textured floor grid

                // @"\editor\"MouseOverTile.png";
                string texturePath = System.IO.Path.Combine (AppMain._core.DataPath, "editor\\BlankTile2.bmp");
                texturePath = System.IO.Path.Combine(AppMain._core.DataPath, "editor\\FloorGridTexture.bmp"); // 2 textures wide x 1 high
                //texturePath = System.IO.Path.Combine(AppMain._core.DataPath, "editor\\TransparentTileBorder2.png"); // 1x1
                //texturePath = System.IO.Path.Combine(AppMain._core.DataPath, "editor\\BoundaryAtlas.png");  // 2 textures wide x 1 high
                //texturePath = System.IO.Path.Combine(AppMain._core.DataPath, "editor\\MouseOverTile.png");
                Model model = (Model)Repository.Create ("Model");
                DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");

                appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
                Layer layer = (Diffuse)Keystone.Resource.Repository.Create("Diffuse");
                ((Diffuse)layer).AlphaTest = true;
                ((Diffuse)layer).AlphaTestDepthWriteEnable = false;
                ((Diffuse)layer).AlphaTestRefValue = 128;
                
                Texture tex = (Texture)Keystone.Resource.Repository.Create (texturePath, "Texture");
                layer.AddChild (tex);
                appearance.AddChild(layer);
                
                // create the mesh
                // NOTE: we use Mesh3d.CreateCellGrid() instead of Mesh3d.CreateFloor() because
                // we want to be able to know the vertices layout so we can modify UV coordinates
                // based on whether a cell is out of bounds of the vehicles exterior cross section
                // or not.
                Keystone.Elements.Mesh3d gridLayoutMesh = Keystone.Elements.Mesh3d.CreateCellGrid(
                    (float)celledRegion.CellSize.x,
                    (float)celledRegion.CellSize.z,
                    celledRegion.CellCountX,
                    celledRegion.CellCountZ,
                    1.0f, false, null);

                gridLayoutMesh.TextureClamping = true; // TODO: TextureClamping should be assignable via Appearance
                gridLayoutMesh.Name = "undefined interior grid mesh"; // friendly name
                // note: the underlying meshes are shared, but the static entity is not
                // so floorplan viewport and 3d edit viewport will both have their own overlay entities
                string id = "UNDEFINED_INTERIOR_GRID_ENTITY"; // Repository.GetNewName(typeof(ModeledEntity));
                mLegalInteriorGrid = new ModeledEntity(id);
                mLegalInteriorGrid.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mLegalInteriorGrid.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mLegalInteriorGrid.Visible = true; // NOTE: setting this to visible true BEFORE we set AllEntityAttributes, will result in Visible == false!
                mLegalInteriorGrid.Dynamic = false;
                mLegalInteriorGrid.Pickable = false; // not pickable since this is just a rendering of the floor.  Our actual celledRegion does the picking
                mLegalInteriorGrid.Enable = true;
                mLegalInteriorGrid.Overlay = false; // we still want items placed on floorplan to be rendered ontop of this grid
                mLegalInteriorGrid.Serializable = false;
                
                model.AddChild(gridLayoutMesh);
                model.AddChild(appearance);
                mLegalInteriorGrid.AddChild(model);

                // raise the render priority by 1 from default so that our floor grid will always render before our exterior
                mLegalInteriorGrid.RenderPriority += 1;

                Repository.IncrementRef( mLegalInteriorGrid);
                Keystone.IO.PagerBase.QueuePageableResourceLoad (mLegalInteriorGrid, null, true);
            }
        }

        
        private int mLastFloor = -1;

        
        // NOTE: This only makes the visual representation of the tilemaskgrid
        // so that we can edit it in real time.  The underlying mask array however
        // is completely seperate and thus server side takes up only a fraction of the memory
        // this does.  It could very well be still hwoever that we are only able to have a few
        // user ships in memory, but in a single player procedural adventure simulation maybe
        // this is not a big problem... still id like to have some small level of multiplayer
        // both coop and pvp
        private void LoadTileMaskGrid(Interior celledRegion)
        {
        	int tileCountX = (int)(celledRegion.TilesPerCellX * celledRegion.CellCountX);
            int tileCountZ = (int)(celledRegion.TilesPerCellZ * celledRegion.CellCountZ);
            float gridWidth = (float)celledRegion.CellSize.x * (float)celledRegion.CellCountX;
            float gridDepth = (float)celledRegion.CellSize.z * (float)celledRegion.CellCountZ;
            
        	// one grid we can use for all view modes (eg footprints, power lines, network cables, 
            // ventilation, etc)
            if (mTileMaskGrid == null)
            {

                // NOTE: we use 1, 1 for instead of tileCountX and tileCountZ because our Atlas
                // shader will automatically divide the single large quad into tiny tiles
                Keystone.Elements.Mesh3d gridMesh = Keystone.Elements.Mesh3d.CreateCellGrid(gridWidth, gridDepth, 1, 1, 1.0f, false, null);

                gridMesh.TextureClamping = true; // // TODO: TextureClamping should be assignable via Appearance TODO: is this necessary for achieving good lines between textures?  This should be part of appearance though, set by appearance on the geometry
                gridMesh.Name = "tile mask grid mesh"; // friendly name
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                // create an apeparance and add a test texture
                string diffuse = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\masktiles_frontcolor.png"); // masktiles_mono.png"; //masktiles.png";
                diffuse = System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\masktiles_icons1.png");
                diffuse = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\masktiles_solid.png");
                string shader = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\atlas.fx");
                Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, shader, diffuse, null, null, null);
                appearance.RemoveMaterial();


                string tempTexture = Keystone.IO.XMLDatabase.GetTempFileName();

                AppMain._core.TextureFactory.SetTextureMode(MTV3D65.CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
                mTileMaskTextureIndex = AppMain._core.TextureFactory.CreateTexture(tileCountX, tileCountZ, true, tempTexture); 
                
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
                mTileMaskGrid.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mTileMaskGrid.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mTileMaskGrid.Dynamic = false;
                mTileMaskGrid.Visible = true; // NOTE: setting this to visible true BEFORE we set AllEntityAttributes, will result in Visible == false!
                mTileMaskGrid.Enable = true;
                mTileMaskGrid.Overlay = false; // we still want items placed on floorplan to be rendered ontop of this grid, or for tilemask maybe not..
                mTileMaskGrid.Serializable = false;

                model.AddChild(gridMesh);
                model.AddChild(appearance);

                mTileMaskGrid.AddChild(model);

               
                // NOTE: Floorplan's mTileMaskGrid uses Immediate NOT Retained so we must IncrementRef
                Repository.IncrementRef(mTileMaskGrid);
                Keystone.IO.PagerBase.LoadTVResource(mTileMaskGrid);
               // Keystone.IO.PagerBase.QueuePageableResource (mTileMaskGrid, null, true);
            }
        }
        #endregion

        #region IDisposeable 
        // TODO: this Dispose does not get called
        public override void Dispose()
        {
            base.Dispose();

            RemoveHUDEntity_Retained(mLegalInteriorGrid);
            RemoveHUDEntity_Retained(mYardStick);
            RemoveHUDEntity_Retained(mMouseTileMarker);
            RemoveHUDEntity_Retained(mTileMaskGrid);

            Repository.DecrementRef(mLegalInteriorGrid);
            Repository.DecrementRef(mYardStick);
            Repository.DecrementRef(mMouseTileMarker);
            Repository.DecrementRef(mTileMaskGrid);

        }
        #endregion

    }
}
