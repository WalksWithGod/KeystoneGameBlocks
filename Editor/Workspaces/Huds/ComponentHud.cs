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
    public class ComponentHud : Keystone.Hud.Hud
    {
        private ComponentDesignWorkspace mComponentWorkspace;
        private float Z_FIGHTING_EPSILON = 0.01f;

        // mFootprintMesh is shared across all Huds
        // in this workspace so we use a static var
        private static ModeledEntity mFootprintMesh;
        private static bool mChangeVisualizationDimensions = true;

        int mLastLayerHashCode = 0;
        
        // these var values are iniitialized in the ctor from the workspace
        private uint mTileCountX = 0;
        private uint mTileCountZ = 0;
      

        public ComponentHud(ComponentDesignWorkspace workspace)
            : base()
        {
            if (workspace == null) throw new ArgumentNullException();

            mComponentWorkspace = workspace;
            mChangeVisualizationDimensions = true;

            // these footprint dimensions must be multiples of 2 on each side (perhaps with one small exception, 1x1 is ok since it fits neatly in one tile)
            mTileCountX = workspace.FootPrintWidth;
            mTileCountZ = workspace.FootPrintHeight;
            if (mTileCountX == 0 || mTileCountZ == 0)
            {
                mTileCountX = mTileCountZ = 0;
            }
            else
            {
                // make sure tileCount on both axis are multiples of 2
                if (mTileCountX % 2 != 0) mTileCountX++;
                if (mTileCountZ % 2 != 0) mTileCountZ++;
            }

            SetFootprintVisualizationSize(mTileCountX, mTileCountZ);

            InitializePickParameters();
            
            CreateAtlasLookup();
        }

        
        public ModeledEntity TileVisualization 
        { 
            get { return mFootprintMesh; } 
        }

        
        private void InitializePickParameters()
        {
            mPickParameters = new PickParameters[2];

            // "excluded" types will skip WITHOUT TRAVERSING the skipped entity's children
            KeyCommon.Flags.EntityAttributes excludedObjectTypes = EntityAttributes.AllEntityAttributes_Except_HUD;
            // "ignored" will skip but WILL TRAVERSE skipped entity's children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes = EntityAttributes.AllEntityAttributes_Except_HUD;

            PickParameters pickParams = new PickParameters
            {
          		T0 = AppMain.MINIMUM_PICK_DISTANCE,
	            T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Face,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[0] = pickParams;

            //  note: we do not exclude HUD when a footprint painter/eraser tool is active

            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            excludedObjectTypes = EntityAttributes.AllEntityAttributes_Except_HUD;
            ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.Region | KeyCommon.Flags.EntityAttributes.ContainerExterior;

            pickParams = new PickParameters
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

            mPickParameters[1] = pickParams;
        }

        #region Tool Events
        protected override void OnEditTool_ToolChanged(Keystone.EditTools.Tool currentTool)
        {
            if (currentTool == null || currentTool is SelectionTool)
            {
                mPlacementToolGraphic = null;
            }
        }

        #endregion


        public override VisibleItem? Iconize(VisibleItem item, double iconScale)
        {
            // item.entity, item.model, item.cameraSpacePosition, item.DistanceToCameraSq
            return null;
        }

        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateBeforeCull(context, elapsedSeconds);

            Keystone.Collision.PickResults pickResult = context.Workspace.MouseOverItem;
            Keystone.EditTools.Tool currentTool = context.Workspace.CurrentTool;

            if (mLastTool != currentTool)
            {
                OnEditTool_ToolChanged(currentTool);
                mLastTool = currentTool;
                
            }
            
            IHUDToolPreview currentPreviewGraphic = null;
            // TODO: do we need to prevent any tool except for footprint painting?
            //  TODO: or will component editor eventually allow for other things like
            //        assigning weapons and backpacks and such on actors
            if (currentTool is PlacementTool)
            {
                // since tools haven't changed, mLastTool must also be placement tool
                PlacementTool ptool = (PlacementTool)currentTool;
                
                if (mPreviewGraphic is PlacementPreview == false )
                {
	                string cloneID = Repository.GetNewName(ptool.Source.TypeName);
            		ModeledEntity clone = (ModeledEntity)ptool.Source.Clone(cloneID, true, false);
                	currentPreviewGraphic = new PlacementPreview (ptool, clone, AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                }
            }
            else if (currentTool is KeyEdit.Workspaces.Tools.SelectionTool || currentTool is KeyEdit.Workspaces.Tools.ComponentFootprintPainter)
            {
            	// TODO: THIS CODE NEEDS TO BE PART OF A PREVIEW OBJECT
                if (currentTool is KeyEdit.Workspaces.Tools.ComponentFootprintPainter)
                    // re-enable picking of the component's footprint grid
                    context.PickParameters = mPickParameters[1]; ;


                CellFootprint fp = this.mComponentWorkspace.Component.Footprint;

                if (fp != null && fp.Data != null)
                {
                    int width = fp.Data.GetLength(0);
                    int height = fp.Data.GetLength(1);

                    int[] colors = new int[width * height];

                    for (int i = 0; i < width; i++)
                        for (int j = 0; j < height; j++)
                        {
                            // TODO: this is wrong to reverse in this method.. data should already be passed in reversed if such a thing is necessarry
                            int jReversed = height - 1 - j; // - 1 because we're 0 based index not 1

                            colors[j * width + i] = mAtlasLookup[0];
                            // TODO: don't we need to select the mAtlasLookup[] index based on what kind of footprint data is set in the flags?
                            // TODO: we want to show only the layer we are currently working on in the component workspace.
                            // TODO: what layers does a hatch use?  Is it an obstacle that prevents other obstacles from being placed on top of it? Yes.
                            //       is it an accessible component that allows traversal up/down even though it is an obstacle? yes.
                            if (fp.Data[i, jReversed] != 0)
                            {
                                // depending on the footprint bit we are looking at in the ComponentDesignWorspace, select appropriate atlas lookup index
                                switch (mComponentWorkspace.CurrentLayer)
                                {
                                    case (int)Interior.TILE_ATTRIBUTES.WALL:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.WALL) != 0)
                                            colors[j * width + i] = mAtlasLookup[4];
                                        break;
                                    // OBSOLETE - we no longer have OBSTACLE flag. We use just WALL and COMPONENT to define non-traversable areas
                                    //case (int)Interior.TILE_ATTRIBUTES.OBSTACLE:
                                    //    if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.OBSTACLE) != 0)
                                    //        colors[j * width + i] = mAtlasLookup[4];
                                    //    break;
                                    case (int)Interior.TILE_ATTRIBUTES.COMPONENT:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.COMPONENT) != 0)
                                            colors[j * width + i] = mAtlasLookup[5];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.COMPONENT_TRAVERSABLE:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.COMPONENT_TRAVERSABLE) != 0)
                                            colors[j * width + i] = mAtlasLookup[4];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.ENTRANCE_AND_EXIT:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.ENTRANCE_AND_EXIT) != 0)
                                            colors[j * width + i] = mAtlasLookup[3];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.STAIR_LOWER_LANDING) != 0)
                                            colors[j * width + i] = mAtlasLookup[1];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.STAIR_UPPER_LANDING) != 0)
                                            colors[j * width + i] = mAtlasLookup[2];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.LADDER_LOWER_LANDING) != 0)
                                            colors[j * width + i] = mAtlasLookup[1];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING:
                                        if ((fp.Data[i, jReversed] & (int)Interior.TILE_ATTRIBUTES.LADDER_UPPER_LANDING) != 0)
                                            colors[j * width + i] = mAtlasLookup[2];
                                        break;
                                    case (int)Interior.TILE_ATTRIBUTES.NONE:
                                    default:
                                        colors[j * width + i] = mAtlasLookup[0];
                                        break;
                                }
                            }
                        }
                    // assign the color indices to the lookup texture (Layers[1]) used by atlas.fx shader
                    if (mFootprintMesh != null)
	                    mFootprintMesh.Model.Appearance.Layers[1].Texture.SetPixelArray(0, 0, width, height, colors);

                    
                }
            }
			else
            {
            	// nullpreview 
        		currentPreviewGraphic = new NullPreview ();
			}

            // persistant footprint data which we'll save out to the component's DomainObject.Properties["footprint"]
            Entity component = mComponentWorkspace.Component;

            // TODO: we should track last changes in the footprint via hashcode perhaps
            string encodedData = (string)component.GetProperty("footprint", false).DefaultValue;
            CellFootprint footprint = null;
            if (string.IsNullOrEmpty (encodedData))
            {
                // TODO: the following code should never hit because
                // when first opening the ComponentDesignWorkspace we test for null Footprint
                // and create default 16x16
                encodedData = CellFootprint.Encode(new int[mTileCountX, mTileCountZ]);
                // to edit we must have a footprint to use
                // TODO: when we change dimensions must must send a command and the stored footprint should be updated
                // and when we test hashcodes we should know this.
                Settings.PropertySpec property = new Settings.PropertySpec("footprint", typeof(string).Name, (object)encodedData);
                component.SetProperties(new Settings.PropertySpec[] { property });
            }
            else
            {
                footprint = CellFootprint.Create (encodedData);
                // match up our vars to the actual data
                uint tmpX = (uint)footprint.Data.GetLength(0);
                uint tmpZ = (uint)footprint.Data.GetLength(1);
                if (mTileCountX != tmpX || mTileCountZ != tmpZ)
                    mChangeVisualizationDimensions = true;

                mTileCountX = tmpX;
                mTileCountZ = tmpZ;

#if DEBUG
                // TEMP: TODO: Hack to eliminate old enum 1 << 14 from footprint data. we just need to resave the component for changes to take effect.
                //       MUSVE RE-SAVE THE PREFAB AFTER LOADING IT INTO THE COMPONENTEDITORWORKSPACE
                CellFootprint fp = this.mComponentWorkspace.Component.Footprint;

                for (int i = 0; i < tmpX; i++)
                    for (int j = 0; j < tmpZ; j++)
                    {
                        if ((fp.Data[i, j] & 1 << 14) != 0)
                            fp.Data[i, j] &= ~(1 << 14);
                    }

                
                encodedData = CellFootprint.Encode (fp.Data);
                // to edit we must have a footprint to use
                // TODO: when we change dimensions must must send a command and the stored footprint should be updated
                // and when we test hashcodes we should know this.
                Settings.PropertySpec property = new Settings.PropertySpec("footprint", typeof(string).Name, (object) encodedData);
                component.SetProperties(new Settings.PropertySpec[] { property });
                this.mComponentWorkspace.Component.SetProperties(new Settings.PropertySpec[] { property });
                // END TEMP HACK
#endif
            }



                LoadFootprintGrid(mTileCountX, mTileCountZ);
            PositionTileMaskGrid(context, component);
            
            
            if (pickResult == null || pickResult.CollidedObjectType == PickAccuracy.None )
            {
            }
            else 
            {
//                // mouse over tile rollover
//                float impactPointX = (float)pickResult.ImpactPointLocalSpace.x;
//                float impactPointZ = (float)pickResult.ImpactPointLocalSpace.z;
//
//                // convert the local space float impact points to int footprint subscripts
//                int pixelX, pixelZ;
//                Keystone.Celestial.ProceduralHelper.MapImpactPointToPixelCoordinate(
//                                                    mGridWidth, mGridDepth,
//                                                    (uint)mTileCountX, (uint)mTileCountZ,
//                                                    impactPointX, impactPointZ,
//                                                    out pixelX, out pixelZ);
//
//                // NOTE: we should track the last mouse and undo it rather than rely on full
//                //       re-write of the footprint data
//                mTileMaskTextureLayer.Texture.SetPixel(pixelX, pixelZ, mMouseOverColor);

            }
            
            if (mPreviewGraphic != currentPreviewGraphic && currentPreviewGraphic != null)
            {
            	// dispose existing and assign new if the new one is NOT null, otherwise no change has occurred
            	if (mPreviewGraphic != null)
            	{
            		mPreviewGraphic.Clear();
            		mPreviewGraphic.Dispose();
            	}
          		mPreviewGraphic = currentPreviewGraphic;            	
            }
            
            if (mPreviewGraphic != null)
            	mPreviewGraphic.Clear();
            
            if (mPreviewGraphic != null)
                	mPreviewGraphic.Preview(context, pickResult);

			 if (mPlacementToolGraphic != null)
            {
                PositionMarker(currentTool, pickResult, mPlacementToolGraphic);
                AddHUDEntity_Immediate(component.Region, mPlacementToolGraphic, false);
            }
        }

        public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateAfterCull(context, elapsedSeconds);

            Keystone.Collision.PickResults pickResult = context.Workspace.MouseOverItem;
            Keystone.EditTools.Tool currentTool = context.Workspace.CurrentTool;

            if (currentTool.PickResult != null)
            {
                // are we over the grid?
                if (currentTool.PickResult.Entity == mFootprintMesh)
                {
                    // TODO: using a shared HUD, we can't assign different -Context.Position offset to each individual hud!
                    //       I think we need to find a way to share just the mFootPrintGridEntity
                    //       Either that, or we need to update each version of that entity on footprint paint/erase events
                    Vector3d mousePosition = currentTool.PickResult.ImpactPointLocalSpace;
                    DrawMouseLocationSquare(mComponentWorkspace.Component.Region, mousePosition, -Context.Position);
                }
            }
        }


        private void DrawMouseLocationSquare(Entity parent, Vector3d hitpoint, Vector3d cameraSpaceOffset)
        {
            float gridWidth = mComponentWorkspace.TileWidth * mTileCountX;
            float gridDepth = mComponentWorkspace.TileHeight * mTileCountZ;
            int x, z;
            Keystone.Celestial.ProceduralHelper.MapImpactPointToTileCoordinate(gridWidth, gridDepth, mTileCountX, mTileCountZ, (float)hitpoint.x, (float)hitpoint.z, out x, out z);
            // The grid entity is not at origin, it is centered on the target component's translation
            cameraSpaceOffset += mFootprintMesh.Translation;

            // TODO: tileWidth and tileDepth should be directly held in vars, not calculated here
            float tileWidth = mComponentWorkspace.TileWidth;
            float tileHeight = mComponentWorkspace.TileHeight;
            

            float left = -gridWidth / 2f + (x * tileWidth);
            float bottom = -gridDepth / 2f + (z * tileHeight);
            float right = left + tileWidth;
            float top = bottom + tileHeight;

            float y = Z_FIGHTING_EPSILON * 1f;
            // note: the lines are computed in camera space
            Line3d[] lines = new Line3d[4];
            lines[0] = new Line3d(new Vector3d (left, y, bottom) + cameraSpaceOffset, new Vector3d(left, y, top) + cameraSpaceOffset);
            lines[1] = new Line3d(new Vector3d(left, y, top) + cameraSpaceOffset, new Vector3d(right, y, top) + cameraSpaceOffset);
            lines[2] = new Line3d(new Vector3d(right, y, top) + cameraSpaceOffset, new Vector3d(right, y, bottom) + cameraSpaceOffset);
            lines[3] = new Line3d(new Vector3d(right, y, bottom) + cameraSpaceOffset, new Vector3d(left, y, bottom) + cameraSpaceOffset);

            MTV3D65.CONST_TV_COLORKEY color = MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_RED;


            Keystone.Immediate_2D.Renderable3DLines renderable = new Keystone.Immediate_2D.Renderable3DLines(lines, (int)color);

            AddHUDEntity_Immediate(parent, renderable);
        }

        private Vector3d GetMarkerModelSpaceImpactPoint(float cellSizeX, float cellSizeZ, uint cellCountX, uint cellCountZ, float startX, float startZ, Vector3d impactPoint)
        {
            // convert the impact point which is in celledRegion space into a single tile's space?
            //       - find the center of this tile and subtract it from the impactPoint
            uint j = (uint)(((cellSizeX * cellCountX / 2f) + impactPoint.x) / cellSizeX);
            if (j >= cellCountX) j = cellCountX - 1;
            uint k = (uint)(((cellSizeZ * cellCountZ / 2f) + impactPoint.z) / cellSizeZ);
            if (k >= cellCountZ) k = cellCountZ - 1;


            float tileCenterX = (startX * cellSizeX) + (j * cellSizeX);
            float tileCenterZ = (startZ * cellSizeZ) + (k * cellSizeZ);
            // if this converted impactPoint is then no longer in the bounds of a tile in model space
            // then the impactpoint is not in this tile at all.
            impactPoint.x -= tileCenterX;
            impactPoint.z -= tileCenterZ;
            return impactPoint;
        }

        public override void Render(RenderingContext context, List<RegionPVS> regionPVS)
        {
            base.Render(context, regionPVS);
            if (context == null) return;


        }

        #region Position and Update HUD Elements
        private void PositionMarker(Tool tool, Keystone.Collision.PickResults pickResult, ModeledEntity marker)
        {
            Vector3d translation = Vector3d.Zero();

            // compute position of hud entities
            if (tool is SelectionTool & pickResult.FacePoints != null)
                translation = (pickResult.FacePoints[0] + pickResult.FacePoints[2]) * .5f;
            else 
            {
            	throw new Exception ("Unexpected tool type.  What are we positioning marker with respect to?");
            }

            // TODO: i think ideally, a marker should be wrapped into some type of struct
            // and have a delegate assigned to update it and render it.  
            // 

            // TODO: in theory we might have an array of markers depending on the type
            // and state of the marker operation....
            // TODO: are these actually would be an array of Source entities!  So that does 
            // make a difference in terms of implementation...
            //
            // for starters, id like to have the wall align properly 
            // so in a sense, our "sources" may need to be an array with a stipulation that
            // must all be of same type, same parent, just different positions...

            translation.y += Z_FIGHTING_EPSILON * 16;
            if (marker != null)
            {
                marker.Translation = translation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Grid is dynamically positioned at same derived translation as parent entity
        /// because ultimately it's added to HUD3DRoot and not the parent so it cannot
        /// inherent the proper translation from the parent.
        /// Grid is not at Origin since the Entity Component it aligns with is not
        /// necessarily at origin.
        /// </remarks>
        /// <param name="celledRegion"></param>
        private void PositionTileMaskGrid(RenderingContext context, Entity parent)
        {
            if (mFootprintMesh == null) return;

            Vector3d translation = parent.DerivedTranslation;

            // make the grid a tiny bit lower than bottom of the cell to avoid z-fighting with a floor tile
            translation.y += Z_FIGHTING_EPSILON;

            mFootprintMesh.Translation = translation;
            // NOTE: This mTileMaskGrid is added to HUD as retained element so no immediate add here
        }
        #endregion

        int[] mAtlasLookup;
        int mMouseOverColor;
        private void CreateAtlasLookup()
        {
            float r = 0, g = 0, b = 0, a = 1.0f;
            // there are 8 textures in our atlas texture
            mAtlasLookup = new int[8];
            for (int i = 0; i < 8; i++)
            {
                r = i / 255f;
                mAtlasLookup[i] = AppMain._core.Globals.RGBA(r, g, b, a);
            }

            
            // mouse over highlight
            r = 1f / 255f;
            g = 0;
            b = 0;
            a = 1.0f;
            mMouseOverColor = AppMain._core.Globals.RGBA(r, g, b, a);
        }

        public void SetFootprintVisualizationSize(uint width, uint height)
        {
            mChangeVisualizationDimensions = true;

            mTileCountX = width;
            mTileCountZ = height;
        }

        #region Load HUD Elements
        static Keystone.Appearance.TextureAtlas mTileMaskAtlas = 
        	(TextureAtlas)Keystone.Resource.Repository.Create(System.IO.Path.Combine(AppMain.DATA_PATH, @"editor\masktiles.png"), "TextureAtlas");

        static Layer mTileMaskTextureLayer;

        // NOTE: This only makes the visual representation of the tilemaskgrid
        // so that we can edit it in real time.  The underlying mask array however
        // is completely seperate and thus server side takes up only a fraction of the memory
        // this does.  It could very well be still hwoever that we are only able to have a few
        // user ships in memory, but in a single player procedural adventure simulation maybe
        // this is not a big problem... still id like to have some small level of multiplayer
        // both co-op and pvp
        private void LoadFootprintGrid(uint tileCountX, uint tileCountZ)
        {
           
            float gridWidth = mComponentWorkspace.TileWidth * tileCountX;
            float gridDepth = mComponentWorkspace.TileHeight * tileCountZ;

            if (mFootprintMesh == null)
            {
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                // create an appearance and add a test texture
                string diffuse = System.IO.Path.Combine (AppMain.DATA_PATH, @"editor\masktiles_solid.png"); // masktiles_frontcolor.png // masktiles_mono.png"; //masktiles.png";
                string shaderPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\atlas.fx");

                Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, shaderPath, diffuse, null, null, null);
                appearance.RemoveMaterial();

                //appearance.AddChild(TileMaskGrid_LoadTexture(cellCountX, cellCountZ));
                
                // NOTE: There is one of these for EACH Hud used by the workspace which
                // for our ComponentEditor is currently 3 viewports
                string id = Repository.GetNewName(typeof(ModeledEntity));
                mFootprintMesh = new ModeledEntity(id);
                mFootprintMesh.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
                mFootprintMesh.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
                mFootprintMesh.Visible = true; // NOTE: setting this to visible true BEFORE we set AllEntityAttributes, will result in Visible == false!
                mFootprintMesh.Dynamic = false;
                mFootprintMesh.Enable = true;
                mFootprintMesh.Overlay = false; // we still want items placed on floorplan to be rendered ontop of this grid, or for tilemask maybe not..
                mFootprintMesh.Serializable = false;
                model.AddChild(appearance);
               // model.AddChild(TileMaskGrid_LoadMesh(gridWidth, gridDepth));
                mFootprintMesh.AddChild(model);

                Keystone.IO.PagerBase.LoadTVResource(model, true);
                // add as retained so that we can Mouse Pick it
                AddHudEntity_Retained(mFootprintMesh);
                // note: We do NOT IncrementRef if we use Retained  
                //Repository.IncrementRef(mTileMaskGrid); 
            }

            if (mChangeVisualizationDimensions)
            {
                mTileCountX = tileCountX;
                mTileCountZ = tileCountZ;

                if (mTileMaskTextureLayer != null) // first run it will be null
                {
                    mFootprintMesh.Model.Appearance.RemoveChild(mTileMaskTextureLayer);
                    //System.Diagnostics.Debug.WriteLine("ComponentHud.LoadTexture - Removing @ " + mTileMaskTextureLayer.Texture.TVIndex.ToString());
                }

                // TODO: Nov.24.2017 - I recently stopped sharing the ComponentHud, is that a mistake?  Should I go back to ensuring i share the one hud?
                // as long as we share this hud with all 3 viewports, there is no worry of creating seperate
                // textures here
                mTileMaskTextureLayer = TileMaskGrid_LoadTexture((int)mTileCountX, (int)mTileCountZ);
                mFootprintMesh.Model.Appearance.AddChild(mTileMaskTextureLayer);

                mFootprintMesh.Model.Appearance.SetShaderParameterValue("TileCountX", (float)mTileCountX); // value must match type that is declared in shader
                mFootprintMesh.Model.Appearance.SetShaderParameterValue("TileCountZ", (float)mTileCountZ); // value must match type that is declared in shader
                mFootprintMesh.Model.Appearance.SetShaderParameterValue("QuadWidth", gridWidth); // value must match type that is declared in shader
                mFootprintMesh.Model.Appearance.SetShaderParameterValue("QuadDepth", gridDepth); // value must match type that is declared in shader


                // are we changing the resolution?  If so we need to unload the Mesh3d 
                // for this grid and make a new one.  
                Geometry geometry = mFootprintMesh.Model.Geometry;
                if (geometry != null) // first run it will be null
                    mFootprintMesh.Model.RemoveChild(geometry); // this will unload it from Repository
                // NOTE: This geometry is shared by 3 different viewports! Conceivably we could share
                //       a HUD between them assigned by ComponentDesignWorkspace, but shouldn't be required
                //       and I don't think saves us much.

                // we only want to create the geometry here. 
                // NOTE: here we always use 1, 1 for numCellsX and numCellsZ because our Atlas Shader
                //       will draw the inner cells for us in just a single 4 vertice quad mesh
                geometry = TileMaskGrid_LoadMesh(gridWidth, gridDepth);
                mFootprintMesh.Model.AddChild(geometry);


                mChangeVisualizationDimensions = false;
            }
        }

        private Keystone.Elements.Mesh3d TileMaskGrid_LoadMesh(float gridWidth, float gridDepth)
        {
            // NOTE: here we always use 1, 1 for numCellsX and numCellsZ because our Atlas Shader
            //       will draw the inner cells for us in just a single 4 vertice quad mesh
            Keystone.Elements.Mesh3d gridMesh = Keystone.Elements.Mesh3d.CreateCellGrid(
                                        gridWidth, gridDepth, 1, 1, 1.0f, false, null);

            gridMesh.TextureClamping = true; // // TODO: TextureClamping should be assignable via Appearance TODO: is this necessary for achieving good lines between textures?  This should be part of appearance though, set by appearance on the geometry
            gridMesh.Name = "tileMaskGridMesh";     
            return gridMesh ;
        }

        private Keystone.Appearance.Layer TileMaskGrid_LoadTexture(int cellCountX, int cellCountZ)
        {
            string tempTexture = Keystone.IO.XMLDatabase.GetTempFileName();

            AppMain._core.TextureFactory.SetTextureMode(MTV3D65.CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
            // Create a texture that is of the pixel dimensions we need for this grid so we have 1:1 mapping 
            int index = AppMain._core.TextureFactory.CreateTexture(cellCountX, cellCountZ, false, tempTexture);
            System.Diagnostics.Debug.WriteLine("ComponentHud.LoadTexture - Loading @ " + index.ToString());
            //AppMain._core.TextureFactory.SaveTexture(rsTextureIndex, tempTexture);
            // TODO: is there a reason why we're using normal map here?
            Keystone.Appearance.NormalMap n = (NormalMap)Keystone.Resource.Repository.Create("NormalMap");
            Keystone.Appearance.Texture tex = Texture.Create (index); //  (Texture)Keystone.Resource.Repository.Create (?, "Texture");
            n.AddChild (tex);

                
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
            return n;
        }
        #endregion

        #region IDisposeable
        // TODO: this Dispose does not get called
        public override void Dispose()
        {
            base.Dispose();
            RemoveHUDEntity_Retained(mFootprintMesh);
            Repository.DecrementRef(mFootprintMesh);
        }
        #endregion
    }
}
