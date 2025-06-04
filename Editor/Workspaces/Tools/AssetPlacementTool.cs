using System;
using System.Collections.Generic;
using Keystone.EditTools;
using Keystone.Entities;
using Keystone.Collision;
using Keystone.Events;
using Keystone.Types;
using Keystone.Portals;
using Keystone.Cameras;
using Keystone.CSG;
using Keystone.Utilities;
using Keystone.Extensions;
using KeyCommon.Flags ;

namespace KeyEdit.Workspaces.Tools
{
    // as far as insides of ships go, the camera must be in the same frame where you're editing.
    // Thus to edit a ship its useful to be in deckplan mode which forces the context to be on just the interior
    // http://www.youtube.com/watch?v=Az6X0Vi9hH4
    // in gamebryo i like how the object is loaded, then you can click to set it down.
    // Well for space games i think a good idea is to wait for the object to be loaded, then when loaded
    // change the cursor to a x for "no good" or "green" for good and allow the user to click once to set that entity
    // and even copies of that entity.  Since the entity will have been loaded, and if there's no "surface" to set it down upon
    // then we'll place the model at a distance where it takes up maybe 25% of the screenspace so it's always fully visible.
    // So placing a giant saturn will place it on each "click".
    // This is a good way to do it rather than just popping it into the world!  So simple.

    // This class is a Tool, not a Keystone.Command or Message and the Entity that we are going to place 
    // we will load ourselves if it's not loaded and control any wait dialog here since it is here
    // where we will have our completion delegate to handle asychronous loading of Entity's 
    public class AssetPlacementTool : PlacementTool
    {
        private System.Windows.Forms.Form mWaitDialog;
        private bool mShowWaitDialog;
        private bool mShowCustomCursor;


        string _prefabPathInArchive;
        string _relativeArchivePath;
        int[,] mCollisionFootprint;
        bool mPaintOperationInProgress = false;
        bool mComponentDropOperationInProgress = false;

        public bool ShowPreview = true;
        public EntityAttributes IgnoredParentTypes = KeyCommon.Flags.EntityAttributes.None;
        public EntityAttributes ExcludedParentTypes = KeyCommon.Flags.EntityAttributes.Background;
        public PlacementMode PlacementMode;
        public KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION paintOperation;
        public Vector3i StartTile;
        public Vector3i MouseOverTile;

        protected uint[] mPrevTiles;
        private byte[] mWallRotations; // 0 - 359 y axis rotation mapped to 0 - 255




        public AssetPlacementTool(Keystone.Network.NetworkClientBase netClient, string relativePath, uint brushStyle, System.Windows.Forms.Form waitDialog)
            : this(netClient, null, relativePath, brushStyle, waitDialog)
        {
        }

        public AssetPlacementTool(Keystone.Network.NetworkClientBase netClient, string relativeArchivePath, string prefabPathInArchive, uint brushStyle, System.Windows.Forms.Form waitDialog)
            : base(netClient)
        {
            _prefabPathInArchive = prefabPathInArchive;
            _relativeArchivePath = relativeArchivePath;

            mWaitDialog = waitDialog;

            // note: the mSource entity is never added to the scene.  In fact, it is is only used as a reference
            // for making cloned copies that will be used by Hud.cs and placement.  Hud.cs clone because
            // it needs to override the Material to show valid/invalid placement options.  
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_relativeArchivePath, _prefabPathInArchive);
            // NOTE: LoadPreviewEntity() does not load any scripts or Interior regions...  just geometry and appearance
            mSource = LoadPreviewEntity(descriptor);
            if (mSource is Keystone.Vehicles.Vehicle || mSource is Container)
            {
                mSource.RemoveChild(((Container)mSource).Interior);
            }

            mSourceEntry = mSource.ID;
            
            if (mSource == null) throw new Exception("AssetPlacementTool.ctor() - Could not load source entity.");
            if (mSource.Footprint != null)
                mCollisionFootprint = mSource.Footprint.Data; // CreateCollisionFootprint();


            ComponentRotation = new Quaternion();

            // TODO: for brush styles that rely on taking up an entire cell rather than just
            //       any sub-tile, how do we do this if we dont load the component's script?
            //       Maybe we should load the script and initialize it?
            _brushStyle = brushStyle;
            if (mSource.Script != null)
            //	// TODO: executing script prior to initializing entity should not be allowed.
            //	//       we should directly set the brush style from code when we know what
            //  //       tool icon was selected or if direct asset library selection, then 
            //	//		assign BRUSH_SINGLE_DROP.  So this means if the current library
            // //       tab is for "Hatches" or "Doors" then we know the brush type is for
            // //       entire Cells or Wall edges.  That could be feasible to avoid loading the script. hrm.
            // //       We could just require the brushstyle to be passed in as an argument.
            // //       After much debate, it seems if the script is loaded, then we SHOULD be able
            // //       to call certain functions such as QueryPlacementBrushType which does not 
            // //       require Initialization of the script itself.  Then moving QueryCellPlacement
            // //       to ship_interior.css (the parent instead of in the child) solves issues
            // //       related to child script not being initialized properly.
                _brushStyle = (uint)mSource.Execute("QueryPlacementBrushType", null);
        }


        public uint[] CellIndices { get { return mPrevTiles; } }

        public byte[] Rotations { get { return mWallRotations; } }


        /// <summary>
        /// Loads a reference copy which HUD and placement can use to make clones.  Hud.cs for instance
        /// will make a clone so that it can change the Material to display valid/invalid placement highlights.
        /// </summary>
        /// <remarks>
        /// XMLDB.ReadSychronous is used to ensure the entire entity is loaded fully 
        /// including scripts compiled, geometry, everything.
        /// </remarks>
        private Entity LoadPreviewEntity(KeyCommon.IO.ResourceDescriptor descriptor)
        {
            if (mWaitDialog != null)
                mWaitDialog.Show();

            System.IO.Stream stream = null;
            Keystone.IO.XMLDatabaseCompressed xmldb_c = new Keystone.IO.XMLDatabaseCompressed();
            Keystone.Scene.SceneInfo info = xmldb_c.Open(descriptor, out stream);

            // NOTE: clone entities arg == false in ReadSychronous() call.
            bool delayIPageableLoading = true; // we don't want to recursively page in all  nodes.  This is important for Interior regions too.
            bool recursiveLoadChildren = true;
            bool cloneEntities = true;
            string[] clonedEntityIDs = null;
            // TODO: do we need multiple root level prefabs in the xmldb or should we just pass in an arry of prefabs for things like ladders with railings and a hatch on the upper level?
            Keystone.Elements.Node node = xmldb_c.ReadSynchronous(info.FirstNodeTypeName, null, recursiveLoadChildren, cloneEntities, clonedEntityIDs, delayIPageableLoading, false);

            if (stream != null)
            {
                // NOTE: stream must remain open until after ReadSynchronous call above
                stream.Close();
                stream.Dispose();
            }

            xmldb_c.Dispose();

            if (!(node is Entity))
                throw new Exception("AssetPlacementTool.LoadPreviewEntity() - ERROR: Node not of valid Entity type.");

            ModeledEntity e = (ModeledEntity)node; // node must always be modeled entity
            System.Diagnostics.Debug.Assert(e != null);

            e.CollisionEnable = false;
            e.Visible = true;
            e.Pickable = false;
            e.Dynamic = false; // no physics step is used
            e.InheritRotation = false;
            e.InheritScale = true; // <-- The preview entity SHOULD inherit scale of the world it's being placed into right?
            e.Attributes |= KeyCommon.Flags.EntityAttributes.HUD;

            // set up the prefab link (i dont suppose there is a better place to do this?)
            e.SRC = descriptor.ToString();

            // sychronous loading
            // August.22.2017 - we do not want to recurse and load Interior Region's for asset preview
            // But we DO need any available entity script to load.`1
            Keystone.IO.PagerBase.LoadTVResource(e, false);
            // TODO: I think we do need to load the script afterall.  This does make creating the preview
            //       entity slower, but we do need to be able to query the type of entity this is so that
            //       the parent entity that this child is added to, knows how to place it. e.g a door within 
            //       an interior.
            //       TODO: I think the key is that we remember that a preview entity which is NOT added
            //       the the scene (it's just rendered by HUD) cannot be initialized and so certain functions
            //       within the script cannot be called.  However, some can and that's ok.. such as QueryCellPlacementType
            Keystone.Elements.Model[] models = e.SelectModel(Keystone.Elements.SelectionMode.Render, 0);

            if (models != null)
            {
                for (int i = 0; i < models.Length; i++)
                {
                    Keystone.Appearance.Appearance appearance = models[i].Appearance;
                    if (appearance != null)
                    {
                        // Set Preview LightingMode to MANAGED just for preview placement
                        appearance.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
                        Keystone.IO.PagerBase.LoadTVResource(appearance, true);
                    }
                    Keystone.Elements.Geometry geometry = models[i].Geometry;
                    Keystone.IO.PagerBase.LoadTVResource(geometry, false);
                }
            }
            if (mWaitDialog != null)
                mWaitDialog.Hide();

            return (Entity)node;
        }


        public override void HandleEvent(EventType type, EventArgs args)
        {
            // keyboard related event (args will be NULL and cannot be cast to (MouseEventArgs)
            if (type == EventType.KeyboardCancel)
            {
                // TODO: i don't think ESC to cancel is working to default back to SelectionTool
                // on cancel - force a MouseUp event
                type = EventType.MouseUp;
                return;
            }

            KeyboardEventArgs keyArgs = args as KeyboardEventArgs;
            if (keyArgs != null)
            {
                switch ((keyArgs.Key))
                {
                    case "Escape":
                        CancelPlacement();
                        this._viewport.Context.Workspace.ToolCancel();
                        return;
                        break;
                    default:
                        return;
                }
            }

            MouseEventArgs mouseArgs = args as MouseEventArgs;
            if (mouseArgs == null) return;

            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;

            //System.Diagnostics.Debug.WriteLine ("AssetPlacementTool.HandleEvent() - MouseX = " + mouseArgs.ViewportRelativePosition.X + " MouseY = " + mouseArgs.ViewportRelativePosition.Y);

            if (type == EventType.MouseUp)
            {
                // painting operations in progress must always be allowed to cancel
                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - Mouse Up.");
                if (mPaintOperationInProgress)
                    TilePaint_MouseUp();
                else if (mComponentDropOperationInProgress)
                {
                    Region currentRegion = null;
                    if (mPickResult.Entity is Region)
                        // NOTE: Interior is of type Region
                        currentRegion = (Region)mPickResult.Entity;
                    else
                        currentRegion = mPickResult.Entity.Region;
                                        

                    switch (_brushStyle)
                    {
                        case BRUSH_DOOR:
                            DoorPlacer_MouseUp((Interior)currentRegion);
                            break;
                        case BRUSH_SINGLE_DROP:
                        default:
                            ComponentPlacer_MouseUp(currentRegion.ID);
                            break;
                    }
                }
                else
                    DefaultExterior_MouseUp();
            }
            else // MouseDown, MouseMove, MouseLeave
            {
                // NOTE: normally the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters, but here
                //       we want this tool to also allow Tile picking per Deck
                KeyCommon.Traversal.PickParameters pickParameters = mouseArgs.Viewport.Context.PickParameters;
                pickParameters.FloorLevel = mouseArgs.Viewport.Context.Floor; //<- HACK to dynamically select the floor.  This should be done by the FloorplanDesignWorkspace // int.MinValue; // search all floors 

                mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y, pickParameters);
                //mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y, mouseArgs.Viewport.Context.PickParameters);

                _viewport = mouseArgs.Viewport;

                if (_viewport == null)
                {
                    System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - No viewport selected");
                    return;
                }

                // determine the parentEntity based on pickResult
                // do not just assume it's currentContextRegion
                RenderingContext currentContext = _viewport.Context;
                Region currentRegion = null;

                Interior pickedInterior = null;
                Keystone.TileMap.Structure pickedStructure = null;
                bool structureRequred = false;

                // TODO: if regions are not excluded entity type, then ray start pick region should always  be returned as default pickresult
                //       this way when in space and just trying to place an entity in space and not on terrain, we will have a target
                if (mPickResult.Entity == null)
                {
                    System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - No Entity picked.");
                    return;
                }


                //System.Diagnostics.Debug.WriteLine ("AssetPlacementTool.HandleEvent() - Picked Entity is - " + mPickResult.Entity.TypeName + " - " + DateTime.Now.ToString());

                if (mPickResult.Entity is Region)
                    // NOTE: Interior is of type Region
                    currentRegion = (Region)mPickResult.Entity;
                else
                    currentRegion = mPickResult.Entity.Region;

                // children[0] will contain the viewpoint since it's the currentContext!  but what i should do is simply
                // bool hasStructure; and iterate
                if (RegionHasStructure(currentRegion))
                {
                    structureRequred = true;
                }


                // NOTE: Currently there are too many problems accidentally placing new entities as children to Background3d entities
                // Lights, Stars, Moons, etc to allow use of the mouse over target.  Maybe if we let the user
                // hold down SHIFT key first so that it's explicit and not accidental.
                //parentEntity = mPickResult.Entity;

                // did we pick a structure? TODO: should we enforce picking of structure automatically switches to 
                // picking of the Parent Region instead UNLESS we are painting terrain/structural items (eg walls)?
                if (mPickResult.Entity is Keystone.TileMap.Structure)
                    // use impact point instead of mouse pick vector (what about if ALT is pressed?)
                    pickedStructure = (Keystone.TileMap.Structure)mPickResult.Entity;
                else if (mPickResult.Entity is Interior)
                {
                    pickedInterior = (Interior)mPickResult.Entity;
                }
                else if (structureRequred) return;



                // TODO: parentEntity should not change if an operation is in progress
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // but as you can see above, that's exactly what im doing... MUST FIX
                // but WAIT - it must be allowed to change for structures when tile painting across zone boundaries
                if (currentRegion == null) return;

                switch (type)
                {
                    // this mousedown can(?) handles editablemesh geometry moving.  Currently there's code
                    // for a manipulator to translate an Entity

                    case EventType.MouseDown:
                        try
                        {
                            if (pickedStructure != null)
                            {
                                // TODO: fix below to support dropping components into Structure
                                switch (_brushStyle)
                                {
                                    case BRUSH_FLOOR_TILE:
                                        TilePaint_MouseDown(pickedStructure, mouseArgs.Button);
                                        break;
                                    
                                    case BRUSH_EDGE_SEGMENT:
                                        WallPlacer_MouseDown(pickedStructure, mouseArgs.Button);
                                        break;
                                    case BRUSH_SINGLE_DROP:
                                        ComponentPlacer_MouseDown(pickedStructure.Region.ID, mouseArgs);
                                        break;
                                    //       case BRUSH_EDGE_SINGLE_SEGMENT:
                                    //           //DoorPlacer_MouseDown();
                                    //            break;
                                    //       case BRUSH_CSG_MESH:
                                    //           //DoorPlacer_MouseDown();
                                    //           break;
                                    default:
                                        throw new Exception("AssetPlacementTool.HandleEvent() - Invalid Brush Type");
                                }
                            }
                            else if (pickedInterior != null)
                            {
                                 switch (_brushStyle)
                                 {
                                    case BRUSH_DOOR:  // arent there several types of doors? Crew quarter doors which are more like firefly doors, and then sliding bulkhead doors which are more like trek but require walls to both left and right of the door segment.
                                        DoorPlacer_MouseDown();
                                        break;
                                    case BRUSH_LIFT:
                                    case BRUSH_LADDER:
                                    case BRUSH_STAIRS:
                                    case BRUSH_HATCH: // do i need a seperate brush type here?  todo: do hatches only exist when there is a ladder? and so are apart of a more complex entity prefab structure?
                                                      //                   // TODO: where do i signal that the floor & ceiling need to be collapsed on placement?
                                                      //                   // Or for a door, where do i signal that the wall needs to be replaced?
                                                      //                   // Also, hatches can only be placed where there is a floor and doors can only be placed
                                                      //                   // where there is a wall.  The ship Interior object or it's script perhaps based on brush type?  Unlike with
                                                      //                   // the placeable components, the Interior's script is loaded and can do validation. hrm.
                                        ComponentPlacer_MouseDown(pickedInterior.ID, mouseArgs);
                                        break;
                                    case BRUSH_SINGLE_DROP:
                                        ComponentPlacer_MouseDown(pickedInterior.ID, mouseArgs);
                                        break;
                                    default:
                                        throw new Exception("AssetPlacementTool.HandleEvent() - Invalid Brush Type");
                                 }
                            }
                            else
                                DefaultExterior_MouseDown(currentContext, currentRegion, MousePosition3D);
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - Error -" + ex.Message);

                        }
                        finally
                        {
                            // TODO: This i believe is probably false, we don't
                            // need to stop input capture do we?
                            //_hasInputCapture = false;
                            //DeActivate();

                        }
                        break;
                    case EventType.MouseEnter:
                        System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - MouseEnter...");
                        break;
                    case EventType.MouseMove:
                        // if (mStartPickResults == null) return; // don't return immediately because for door placer
                        // for instance, on mousemove we want to be able to compute
                        // potential drop locations even if we don't actually decide on them
                        // but our Hud.cs can use the potential locations to draw preview visual

                        if (pickedStructure != null)
                        {
                            switch (_brushStyle)
                            {
                                case BRUSH_FLOOR_TILE:
                                    TilePaint_MouseMove(pickedStructure);
                                    break;
                                case BRUSH_EDGE_SEGMENT:
                                    WallPlacer_MouseMove(pickedStructure);
                                    break;
                                case BRUSH_SINGLE_DROP:
                                    // determine rotation of the Hud visual aid
                                    ComponentPlacer_MouseMove(mouseArgs);
                                    break;
                                //                            case BRUSH_EDGE_SINGLE_SEGMENT:
                                //                                //DoorPlacer_MouseMove(pickedStructure);
                                //                                break;
                                //                            case BRUSH_CSG_MESH:
                                //                                //DoorPlacer_MouseMove(pickedStructure);
                                //                                break;
                                default:
                                    throw new Exception("AssetPlacementTool.HandleEvent() - Invalid Brush Type");
                            }
                        }
                        else if (pickedInterior != null)
                        {
                            switch (_brushStyle)
                            {
                                case BRUSH_DOOR:
                                    DoorPlacer_MouseMove(pickedInterior);
                                    break;
                                case BRUSH_HATCH:
                                    ComponentPlacer_MouseMove(mouseArgs);
                                    break;

                                default:
                                    ComponentPlacer_MouseMove(mouseArgs);
                                    break;
                            }

                           
                        }
                        else
                        {
                            UpdatePreviewEntityTransform(_viewport.Context, (ModeledEntity)mSource);
                        }

                        // are we in orthographic view?
                        // switch (_viewport.Context.ProjectionType)
                        // {
                        //      case ProjectionType.Orthographic:
                        //          // is this a deck plan?
                        //          // is the mouse down?
                        //          // are we defining a rubber band area to drop
                        //          // an entire room of floorplan tiles?
                        // }
                        //


                        // 
                        //pickResult.ImpactPoint
                        //System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - Mouse Move...");
                        break;



                    case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                        System.Diagnostics.Debug.WriteLine("AssetPlacementTool.HandleEvent() - Mouse Leave.");

                        break;

                    default:
                        break;
                }
            }
        }

        private bool RegionHasStructure(Region region)
        {
            if (region.Children == null) return false;

            for (int i = 0; i < region.Children.Length; i++)
                if (region.Children[i] is Keystone.TileMap.Structure)
                    return true;

            return false;
        }

       
        #region Tile Painting
        protected object mValue;
        public string LayerName;
        /// <summary>
        /// The value here is simply a path to the segment entity.  
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="index"></param>
        public void SetValue(string layerName, string segmentPath)
        {
            LayerName = layerName;
            mValue = segmentPath;
        }

        public string GetValue()
        {
            if (mValue == null) return null;
            return (string)mValue;
        }

        protected virtual void TilePaint_MouseDown(Keystone.TileMap.Structure structure, Keystone.Enums.MOUSE_BUTTONS button)
        {
            mStartPickResults = mPickResult;


            // determine Add or Remove operation
            if (button == Keystone.Enums.MOUSE_BUTTONS.XAXIS)
            {
                //Operation = KeyCommon.Messages.CelledRegion_PaintCell.PAINT_OPERATION.ADD;

                // CTRL key pressed toggles delete mode?


            }


            // TODO: if the lower level contains a floor, then we might not have to add a model to upper level but we do need to assign
            //       footprint data there.  And im not sure how that's done through the tool, it should be done through
            //       entity.
            //       HOWEVER, if you consider "dig" where we're dealing with floor of [current level] and model in [current level - 1] there too we must
            //       know to deal with both.  Perhaps this is the script of that entity?
            //       - because knowing how to just add extra paint calls here rather than the single call, would be nice... that is just do it here
            //       and then the keystone.dll doesn't care about those sorts of details 
            int floorLevel;
            if (BoundsCheckPaint(structure, out floorLevel) == false) return;

            // NOTE: start and end .TileLocation are same on initial MouseDown event.
            // Retreive 2D list of tiles.
            uint[] tileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, mStartPickResults.TileLocation);
            if (tileIndices == null) return;

            // TODO: client must never deal in terms of "TileIndex" because that value changes...
            //       TileLocation.Y should therefore always contain FloorLevel which is absolute
            //       When digging, levelIndex = 0 at MouseDown may no longer point to same level
            //       if another level was inserted to be the new lower floorlevel at index 0.            
            System.Diagnostics.Debug.WriteLine("TilePaint_MouseDown() - TileIndices count = " + tileIndices.Length.ToString());


            mPaintOperationInProgress = true;
            // as far as command logic goes, first we need to pick a spot on TERRAIN
            // LAYOUT layer that has a NON EMPTY segment set that we will be "digging out."
            // Then our command clear's that segment AND also adds the current terrain segment
            // set in the tool to the below layer IF there is no layer beneath.  Otherwise we
            // use the existing layer
            PaintTerrainTiles(structure.ID, floorLevel, this.LayerName, tileIndices);
        }

        private bool BoundsCheckPaint(Keystone.TileMap.Structure structure, out int floorLevel)
        {
            bool result = false;
            // we need to ensure we stay on same level when we start a paint operation.  
            // no hill climbing or digging.  And if there's no tiles there that can be modified
            // so be it.  But we cannot change levels.
            if (mPickResult.TileLocation.Y != mStartPickResults.TileLocation.Y)
                mPickResult.TileLocation.Y = mStartPickResults.TileLocation.Y;

            // level index must never change from start level index
            floorLevel = mPickResult.TileLocation.Y;
            System.Diagnostics.Debug.Assert(floorLevel == mStartPickResults.TileLocation.Y, "AssetPlacementTool.TilePaint_MouseMove() - ERROR: Level Index has changed during operatoin!");

            if (paintOperation == KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.ADD)
            {
                // if ADD we always paint the level above the layer that is mouse picked.
                result = floorLevel < structure.FloorLevelMaximum();
                floorLevel++;
            }
            else if (paintOperation == KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.REMOVE)
            {
                // No need to change levelIndex.  We will be affecting the level actually picked
                // NOTE: Prevent digging below world minimum bounds.  
                // structure.Layer_SetValue() does bounds checking too, but client side,
                // we should not allow sending of useless command.
                result = floorLevel >= structure.FloorLevelMinimum();
            }

            return result;
        }

        protected virtual void TilePaint_MouseMove(Keystone.TileMap.Structure structure)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null) return;

            System.Diagnostics.Debug.Assert(mPaintOperationInProgress == true);

            int floorLevel;
            if (BoundsCheckPaint(structure, out floorLevel) == false) return;


            // if we've crossed a zone boundary, we'll need seperate calls to find tileindices
            // that lay between start/stop mouse locations
            if (mStartPickResults.EntityID != mPickResult.EntityID)
            {
                // our mouse has crossed zone boundaries
                // we need to create 2 seperate calls to GetTileList 
                // with adjusted start/end faceIDs 
                System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.MouseMove() - Mouse crossing zone boundary.");

                Keystone.TileMap.Structure startStructure = (Keystone.TileMap.Structure)mStartPickResults.Entity;
                Keystone.TileMap.Structure endStructure = (Keystone.TileMap.Structure)mPickResult.Entity;

                Vector3i startBorderLocation = mStartPickResults.TileLocation;
                Vector3i endBorderLocation = mPickResult.TileLocation;

                // which axis have we crossed? And from which direction? small to large or large to small?
                if (mStartPickResults.TileLocation.X == mPickResult.TileLocation.X)
                {
                    if (mStartPickResults.TileLocation.Z > mPickResult.TileLocation.Z)
                    {
                        // TODO: hardcoded 31? is that pixel width?
                        startBorderLocation.Z = 31; // layerWidth - 1; // minus one because zero based indices
                        endBorderLocation.Z = 0;
                    }
                    else
                    {
                        startBorderLocation.Z = 0;
                        endBorderLocation.Z = 31; // layerWidth - 1; // minus one because zero based indices 
                    }
                }
                else
                {
                    if (mStartPickResults.TileLocation.X > mPickResult.TileLocation.X)
                    {
                        startBorderLocation.X = 31; // layerHeight - 1; // minus one because zero based indices
                        endBorderLocation.X = 0;
                    }
                    else
                    {
                        startBorderLocation.X = 0;
                        endBorderLocation.X = 31; // layerHeight - 1; // minus one because zero based indices
                    }
                }

                uint[] startTileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, startBorderLocation);
                uint[] endTileIndices = structure.GetTileList(this.LayerName, endBorderLocation, mPickResult.TileLocation);
                if (startTileIndices == null || endTileIndices == null) return;

                PaintTerrainTiles(startStructure.ID, floorLevel, this.LayerName, startTileIndices);
                PaintTerrainTiles(endStructure.ID, floorLevel, this.LayerName, endTileIndices);

                // we must now update mStartPickResult... perhaps we should use a stack so we can tell subsequently whether we've crossed one or more zones
                // TODO: we should cache the Y value so we never change it
                mStartPickResults = mPickResult;
            }
            else
            {
                uint[] tileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, mPickResult.TileLocation);
                if (tileIndices == null) return;

                PaintTerrainTiles(structure.ID, floorLevel, this.LayerName, tileIndices);
            }
        }


        protected virtual void TilePaint_MouseUp()
        {
            mStartPickResults = null;
            mPrevTiles = null; // clear on mouse up
            mPaintOperationInProgress = false;
        }

        /// <summary>
        /// Find subset array of indices within tileIndices that do not already exist previously
        /// </summary>
        /// <param name="cellIndices"></param>
        /// <returns></returns>
        protected uint[] PruneTiles(uint[] newIndices, uint[] previousIndices)
        {
            List<uint> temp = new List<uint>();

            //prune those cells that have already been painted previously
            for (int i = 0; i < newIndices.Length; i++)
                if (previousIndices.ArrayContains(newIndices[i]) == false)
                    temp.Add(newIndices[i]);

            if (temp.Count == 0) return null;
            return temp.ToArray();
        }

        /// <summary>
        /// Called By WallSegmentPainter and TileSegmentPainter
        /// </summary>
        /// <param name="celledRegionID"></param>
        /// <param name="cellIndices"></param>
        protected virtual void PaintTerrainTiles(string structureID, int floorLevel, string layerName, uint[] tileIndices)
        {
            // TODO: below assert fails if .KGBSEGMENT extension is not used
            System.Diagnostics.Debug.Assert(mValue != null, "AutoTileSegmentPainter.PaintTiles() - Null value not allowed.  A command for deletion of wall or floor should use a default empty SegmentStyle or -1 respectively.");
            System.Diagnostics.Debug.Assert(tileIndices.Length > 0);


            // note: in the case of walls, tileIndices are actually individual edge indices
            // and a single edge index represents an edge defined by 2 verts
            if (tileIndices.Length < 1) return;

            // TODO: we should prune out those areas where a floor can't go
            // this is mainly useful for when changing just the texture, color or mesh but not
            // changing whether there is a floor tile or not already there.
            // I mean, i have some rules perhaps regarding frame strength and how many tiles can
            // span before a pillar or brace or wall or bulkhead is encountered.  But that could be
            // future stuff.
            uint[] prunedTiles = PruneTiles(tileIndices, mPrevTiles);
            if (prunedTiles == null || prunedTiles.Length == 0) return;

            mPrevTiles = mPrevTiles.ArrayAppendRange(prunedTiles);

            //for (int i = 0; i < temp.Count; i++)
            //    System.Diagnostics.Debug.WriteLine("cell[" + i.ToString() + "] = " + temp[i].ToString());

            // LayerNames will indicate if we are painting "tile style" or "wall style"
            // TODO: August.27.2017 - need to add new case for TileMapStructure.PaintCell()
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.TileMapStructure_PaintCell();


            // TODO: we have a problem here if we allow tiling to occur across multiple layers (eg a ground layer and a hill layer)
            //        tileIndices below CAN contain our y layer value though so maybe it's ok.  BUT WE MUST ENSURE THAT 
            //        the proper tests are occurring to ensure all tiles are valid.
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).Indices = prunedTiles;
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).LayerName = layerName;
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).Operation = paintOperation;
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).FloorLevel = floorLevel;
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).PaintValue = mValue;
            ((KeyCommon.Messages.TileMapStructure_PaintCell)mNetworkMessage).ParentStructureID = structureID;

            mNetClient.SendMessage(mNetworkMessage);
        }

        /// <summary>
        /// Called By WallSegmentPainter and TileSegmentPainter
        /// </summary>
        /// <param name="celledRegionID"></param>
        /// <param name="cellIndices"></param>
        protected virtual void PaintInteriorTiles(string structureID, int floorLevel, string layerName, uint[] tileIndices)
        {
            // TODO: below assert fails if .KGBSEGMENT extension is not used
            System.Diagnostics.Debug.Assert(mValue != null, "AutoTileSegmentPainter.PaintTiles() - Null value not allowed.  A command for deletion of wall or floor should use a default empty SegmentStyle or -1 respectively.");
            System.Diagnostics.Debug.Assert(tileIndices.Length > 0);


            // note: in the case of walls, tileIndices are actually individual edge indices
            // and a single edge index represents an edge defined by 2 verts
            if (tileIndices.Length < 1) return;

            // TODO: we should prune out those areas where a floor can't go
            // this is mainly useful for when changing just the texture, color or mesh but not
            // changing whether there is a floor tile or not already there.
            // I mean, i have some rules perhaps regarding frame strength and how many tiles can
            // span before a pillar or brace or wall or bulkhead is encountered.  But that could be
            // future stuff.
            uint[] prunedTiles = PruneTiles(tileIndices, mPrevTiles);
            if (prunedTiles == null || prunedTiles.Length == 0) return;

            mPrevTiles = mPrevTiles.ArrayAppendRange(prunedTiles);

            //for (int i = 0; i < temp.Count; i++)
            //    System.Diagnostics.Debug.WriteLine("cell[" + i.ToString() + "] = " + temp[i].ToString());

            // LayerNames will indicate if we are painting "tile style" or "wall style"
            // TODO: August.27.2017 - need to add new case for TileMapStructure.PaintCell()
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.PaintCellOperation();


            // TODO: we have a problem here if we allow tiling to occur across multiple layers (eg a ground layer and a hill layer)
            //        tileIndices below CAN contain our y layer value though so maybe it's ok.  BUT WE MUST ENSURE THAT 
            //        the proper tests are occurring to ensure all tiles are valid.
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).Indices = prunedTiles;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).LayerName = layerName;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).Operation = paintOperation;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).FloorLevel = floorLevel;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).PaintValue = mValue;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).ParentCelledRegionID = structureID;

            mNetClient.SendMessage(mNetworkMessage);
        }
        #endregion


        #region Wall Placer
        private void WallPlacer_MouseDown(Keystone.TileMap.Structure structure, Keystone.Enums.MOUSE_BUTTONS button)
        {
            mStartPickResults = mPickResult;


            // determine Add or Remove operation
            if (button == Keystone.Enums.MOUSE_BUTTONS.XAXIS)
            {

                //Operation = KeyCommon.Messages.CelledRegion_PaintCell.PAINT_OPERATION.ADD;

                // CTRL key pressed toggles delete mode?


            }


            // TODO: if the lower level contains a floor, then we might not have to add a model to upper level but we do need to assign
            //       footprint data there.  And im not sure how that's done through the tool, it should be done through
            //       entity.
            //       HOWEVER, if you consider "dig" where we're dealing with floor of [current level] and model in [current level - 1] there too we must
            //       know to deal with both.  Perhaps this is the script of that entity?
            //       - because knowing how to just add extra paint calls here rather than the single call, would be nice... that is just do it here
            //       and then the keystone.dll doesn't care about those sorts of details 
            int floorLevel;
            if (BoundsCheckPaint(structure, out floorLevel) == false) return;

            // NOTE: start and end .TileLocation are same on initial MouseDown event.
            // Retreive 2D list of tiles.
            uint[] tileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, mStartPickResults.TileLocation);
            if (tileIndices == null) return;

            // TODO: client must never deal in terms of "TileIndex" because that value changes...
            //       TileLocation.Y should therefore always contain FloorLevel which is absolute
            //       When digging, levelIndex = 0 at MouseDown may no longer point to same level
            //       if another level was inserted to be the new lower floorlevel at index 0.            
            System.Diagnostics.Debug.WriteLine("WallPlacer_MouseDown() - TileIndices count = " + tileIndices.Length.ToString());


            mPaintOperationInProgress = true;
            // as far as command logic goes, first we need to pick a spot on TERRAIN
            // LAYOUT layer that has a NON EMPTY segment set that we will be "digging out."
            // Then our command clear's that segment AND also adds the current terrain segment
            // set in the tool to the below layer IF there is no layer beneath.  Otherwise we
            // use the existing layer
            PaintInteriorTiles(structure.ID, floorLevel, this.LayerName, tileIndices);
        }


        private void WallPlacer_MouseUp(Keystone.TileMap.Structure structure)
        {
            mStartPickResults = null;
            mPrevTiles = null; // clear on mouse up
            mPaintOperationInProgress = false;
        }


        private void WallPlacer_MouseMove(Keystone.TileMap.Structure structure)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null) return;

            System.Diagnostics.Debug.Assert(mPaintOperationInProgress == true);

            int floorLevel;
            if (BoundsCheckPaint(structure, out floorLevel) == false) return;

            // TODO: we need to constrain to one axis either X or Z
            //       also need to constrain to one Y altitude.

            // TODO: need to rotate the wall 90 degrees when dragging along Z axis
            //       But first need to know what the default 0 rotation axis is

            // if we've crossed a zone boundary, we'll need seperate calls to find tileindices
            // that lay between start/stop mouse locations
            if (mStartPickResults.EntityID != mPickResult.EntityID)
            {
                // our mouse has crossed zone boundaries
                // we need to create 2 seperate calls to GetTileList 
                // with adjusted start/end faceIDs 
                System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.WallPlacer_MouseMove() - Mouse crossing zone boundary.");

                Keystone.TileMap.Structure startStructure = (Keystone.TileMap.Structure)mStartPickResults.Entity;
                Keystone.TileMap.Structure endStructure = (Keystone.TileMap.Structure)mPickResult.Entity;

                Vector3i startBorderLocation = mStartPickResults.TileLocation;
                Vector3i endBorderLocation = mPickResult.TileLocation;

                // which axis have we crossed? And from which direction? small to large or large to small?
                if (mStartPickResults.TileLocation.X == mPickResult.TileLocation.X)
                {
                    if (mStartPickResults.TileLocation.Z > mPickResult.TileLocation.Z)
                    {
                        startBorderLocation.Z = 31; // layerWidth - 1; // minus one because zero based indices
                        endBorderLocation.Z = 0;
                    }
                    else
                    {
                        startBorderLocation.Z = 0;
                        endBorderLocation.Z = 31; // layerWidth - 1; // minus one because zero based indices 
                    }
                }
                else
                {
                    if (mStartPickResults.TileLocation.X > mPickResult.TileLocation.X)
                    {
                        startBorderLocation.X = 31; // layerHeight - 1; // minus one because zero based indices
                        endBorderLocation.X = 0;
                    }
                    else
                    {
                        startBorderLocation.X = 0;
                        endBorderLocation.X = 31; // layerHeight - 1; // minus one because zero based indices
                    }
                }

                uint[] startTileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, startBorderLocation);
                uint[] endTileIndices = structure.GetTileList(this.LayerName, endBorderLocation, mPickResult.TileLocation);
                if (startTileIndices == null || endTileIndices == null) return;

                PaintInteriorTiles(startStructure.ID, floorLevel, this.LayerName, startTileIndices);
                PaintInteriorTiles(endStructure.ID, floorLevel, this.LayerName, endTileIndices);

                // we must now update mStartPickResult... perhaps we should use a stack so we can tell subsequently whether we've crossed one or more zones
                // TODO: we should cache the Y value so we never change it
                mStartPickResults = mPickResult;
            }
            else
            {
                // constrain the destination tile location based on the greater mouse direction along X or Z axis
                // assign rotation array to PaintTIles() if we're constrained to Z axis

                Vector3i constrainedTileLocation = mPickResult.TileLocation;
                constrainedTileLocation.X = mStartPickResults.TileLocation.X;

                uint[] tileIndices = structure.GetTileList(this.LayerName, mStartPickResults.TileLocation, constrainedTileLocation);
                if (tileIndices == null) return;

                PaintInteriorTiles(structure.ID, floorLevel, this.LayerName, tileIndices);
            }
        }

        #endregion


        #region Component Placer
        private void ComponentPlacer_MouseDown(string regionID, MouseEventArgs args)
        {
            mComponentDropOperationInProgress = true;
            mMouseStart = mMouseEnd = args.ViewportRelativePosition;

            // TODO: where do we autogenerate footprints?  here?
            mStartPickResults = mPickResult;
            StartTile = mPickResult.TileLocation;

            mManipulatorFunctions = new ManipFunctions2();
            UpdatePreviewEntityTransform(_viewport.Context, (ModeledEntity)mSource);

        }

        private void ComponentPlacer_MouseMove(MouseEventArgs args)
        {
            // NOTE: the preview entity _IS_ translated here for INTERIOR celled region because
            //       mStartPickResults indicates a MouseDown cached pick target.  But for 
            //       typical MouseMove of a compoonent we are trying to place within the Interior
            //       mStartPickResults will be null.        
            if (mStartPickResults == null && mPickResult != null) // mouse down has not occurred yet
            {
                if (PickResult.Entity is Keystone.TileMap.Structure)
                {
                }



                UpdatePreviewEntityTransform(_viewport.Context, (ModeledEntity)mSource);
                //System.Diagnostics.Debug.Assert(mPickResult.Entity is Interior, "CellLocation is being used here, TileMap.Structure requires TileLocation- that's a TODO item");
                MouseOverTile = mPickResult.CellLocation;
            } 
            else // mouse down is pressed
            {
                // we no longer want to move the entity with the mouse, instead we want to rotate it
                mMouseStart = mMouseEnd;
                mMouseEnd = args.ViewportRelativePosition;

                ComponentRotation = UpdatePreviewEntityRotation();
                // NOTE: notice how we do not update MouseOverTile when in component rotation mode
            }
        }

        private void ComponentPlacer_MouseUp(string regionID)
        {
            if (mComponentDropOperationInProgress)
            {
                PlaceEntity(regionID);
                mComponentDropOperationInProgress = false;
                mStartPickResults = null;

                // NOTE: we reset rotation but not translation because preview is still active
                ComponentRotation = new Quaternion();
                // ComponentTranslation = Vector3d.Zero();
                mManipulatorFunctions = null;
                return;
            }
        }

        private void CancelPlacement()
        {
            if (mSource != null)    
                Keystone.Resource.Repository.DecrementRef(mSource);

            mSource = null;
        }

        private void PlaceEntity(string regionID)
        {
            // non boned entities must be raised so that their centered y is off the floor
            // TODO: the following is not true because it depeneds on the component itself
            // and it's script.
            //if (mSource is BonedEntity == false)
            //    ComponentTranslation.y += mSource.Height * .5d;

            //       byte componentRotationIndex = ComponentRotation.GetComponentYRotationIndex();

            // TODO: if we want to prevent storing of TileLocation, then we should use impactPoint here
            // and the rotation should be rounded to a rotation index when component is added to interior.
            // similarly the tilelocation should be computed from the impact point

            // NOTE: TileMap.Structure is like a Terrain.  It is not spatial and it is not hierarchical so
            //       although we do MOUSE PICK against it to find a tile location, we do not AddChild() to it.
            //       Instead we add to the Zone that contains the Structure. 
            //       TODO: but how does this effect the updating of the obstacle map and other map layers?  

            // TODO:  I need to handle the snapping of the ComponentTranslation such that
            // for odd number tiles along an axis, we use center of that tile and for
            // even number tiles, we use center of the entire footprint for that axis.

            //PlaceEntityInStructure(regionID, ComponentTranslation, ComponentRotation);
            // TODO: July.7.2015 - the above line was uncommented and the below commented, but the below seems 
            // to work as it always had.  Why was the above uncommented?  Need to do a diff from before all this crazyiness started.
            // TODO: i think the below works for placing actors and meshes, but the above works for raising/lowering terrain?!  Verify.
            if (mPickResult.Entity is Keystone.TileMap.Structure)
                PlaceEntityInStructure(mPickResult.Entity.ID, mStartPickResults.ImpactPointLocalSpace, ComponentRotation);
            else if (mPickResult.Entity is Keystone.Portals.Interior)
                PlaceEntityInInterior(regionID, ComponentTranslation, ComponentRotation, KeyCommon.Messages.ComponentType.Common); // mStartPickResults.ImpactPointLocalSpace, ComponentRotation);
        }
        #endregion

        #region DoorPlacer
        private void DoorPlacer_MouseDown()
        {
            mStartPickResults = mPickResult;
            mComponentDropOperationInProgress = true;
        }


        private void DoorPlacer_MouseUp(Interior celledRegion)
        {
            mStartPickResults = null;
            System.Diagnostics.Debug.Assert(mPickResult.Entity == celledRegion);
            if (mSource == null) return;

            PlaceEntityInInterior(celledRegion.ID, ComponentTranslation, ComponentRotation, KeyCommon.Messages.ComponentType.EdgeComponent);

            mPrevTiles = null; // clear on mouse up
            mWallRotations = null;
            mComponentDropOperationInProgress = false;
        }


        private void DoorPlacer_MouseMove(Interior interior)
        {
            // TODO: doors should snap to nearest walls.  If there are no walls on an edge do we prevent placement?
            if (mSource != null && mPickResult.Entity != null)
            {
                Vector3d translation = Vector3d.Zero();
                Quaternion rotation = new Quaternion();

                if (mPickResult.Entity is Interior && mPickResult.FaceID >= 0)
                {
                    if (interior.Script == null) throw new Exception();
                    // NOTE: QueryCellPlacement is called on the Interior.cs script and not on the component's script we're placing into the Interior.
                    Vector3d[] vecs = (Vector3d[])interior.Execute("QueryCellPlacement", new object[] { mSource.ID, interior.ID, mPickResult.ImpactPointLocalSpace, (byte)mPickResult.CellLocation.Y });

                    System.Diagnostics.Debug.Assert(interior == (Interior)mPickResult.Entity);
                    
                    translation = vecs[0];
                    ComponentScale = vecs[1]; 
                    if (vecs.Length == 3)
                        rotation = new Quaternion(vecs[2].y * MathHelper.DEGREES_TO_RADIANS, 0, 0);
                }

                ComponentTranslation = translation;
                ComponentRotation = rotation;

                MouseOverTile = mPickResult.CellLocation;
            }
        }
        #endregion

        private bool UseTerrainAutoHeightPlacement(Entity pickedEntity)
        {
            bool terrainPlacement = false;
            if (pickedEntity != null)
                if (pickedEntity as Keystone.TileMap.Structure != null)
                    terrainPlacement = true;
                else
                    terrainPlacement = pickedEntity.GetEntityFlagValue("terrain");

            return terrainPlacement;
        }

        // OBSOLETE - maybe useful at some other point, but now the collisionFootprint is not used.  
        // create a footprint used for collisions that is +1 in size on each of the 4 sides
        // this only needs to be done once when mSource is loaded.
        private int[,] CreateCollisionFootprint()
        {
            int[,] collisionFootprint;
            int[,] footprint;

            if (mSource.Footprint == null) return null;

            footprint = mSource.Footprint.Data;

            // NOTE: below is obsolete - this is not how w do alignment snapping.  This entire function is basically useless.  We just need mSource.Footprint.Data
            collisionFootprint = new int[footprint.GetLength(0) + 2, footprint.GetLength(1) + 2];

            for (int i = 0; i < footprint.GetLength(0); i++)
            {
                for (int j = 0; j < footprint.GetLength(1); j++)
                {
                    if ((footprint[i, j] & (int)Interior.TILE_ATTRIBUTES.COMPONENT) != 0)
                    {
                        collisionFootprint[i - 1, j + 1] = footprint[i, j];
                        collisionFootprint[i - 1, j - 1] = footprint[i, j];
                        collisionFootprint[i + 1, j + 1] = footprint[i, j];
                        collisionFootprint[i + 1, j - 1] = footprint[i, j];
                    }
                }
            }

            return collisionFootprint;
        }


        private void GetCollisionDepthMinMax(int[,] destFootprint, int[,] componentFootprint, out int minX, out int minY, out int maxX, out int maxY)
        {
            if (destFootprint == null || componentFootprint == null) throw new ArgumentNullException();

            maxX = maxY = minX = minY = 0;

            int width = componentFootprint.GetLength(0);
            int depth = componentFootprint.GetLength(1);
            int halfWidth = componentFootprint.GetLength(0) / 2;
            int halfDepth = componentFootprint.GetLength(1) / 2;

            // left side of componentFootprint depth test
            for (int i = 0; i < halfWidth; i++)
                for (int j = 0; j < depth; j++)
                {
                    if ((componentFootprint[i, j] & destFootprint[i,j]) != 0)
                    {
                        minX++;
                        break;
                    }
                }

            // right side
            if (minX == 0)
                for (int i = width - 1; i > halfWidth; i--)
                    for (int j = 0; j < depth; j++)
                    {
                        if ((componentFootprint[i, j] & destFootprint[i, j]) != 0)
                        {
                            maxX++;
                            break;
                        }
                    }

            // bottom side
            for (int j = 0; j < halfDepth; j++)
                for (int i = 0; i < width; i++)
                {
                    if ((componentFootprint[i, j] & destFootprint[i, j]) != 0)
                    {
                        minY++;
                        break;
                    }
                }

            // top side
            if (minY == 0)
                for (int j = depth - 1; j > halfDepth; j--)
                    for (int i = 0; i < width; i++)
                    {
                        if ((componentFootprint[i, j] & destFootprint[i, j]) != 0)
                        {
                            maxY++;
                            break;
                        }
                    }
        }


        private bool PassesCollisionTest(Interior interior, Vector3d position, Quaternion rotation, out double x, out double z )
        {
            // TODO: if the entity contains and Actor3d with no footprint, we cant place it!
            //       Do actors need footprints?  It would mean everytime an actor moves, it must
            //       remove and update its footprint in the tilemask grid.  That in turn would cause
            //       Area generation to recompute.  But we dont want to be able to place crew
            //       on obstacles and with no footprint, there's no way to prevent that.  Should we
            //       even place crew from AssetPlacementTool?  Crew should just be added at runtime
            //       Maybe they can have a footprint, but which does not require regenerating areas and portals?
            //       AFterall, their footprints wont be "obstacles" but rather "actors" so should not require
            //       area and portal regeneration.  I could use a blank footprint.
            x = position.x;
            z = position.z;

            if (mCollisionFootprint == null) return true;

            // NOTE: rotation of footprint is performed within the call to IsChildPlaceableWithinInterior
            int[,] destFootprint;
            
            bool isPlaceable = interior.IsChildPlaceableWithinInterior(mCollisionFootprint, position, rotation, out destFootprint);
            if (isPlaceable) return true;
            return isPlaceable;

            // NOTE: rotation of component footprint is performed within the call to GetCollisionFootprint
            // returns a result footprint where collisions between component and destination occur.. perhaps as out parameter the startTileLocation as well
            Vector3i startTileLocation;
            int[,] collisionFootprint = interior.GetCollisionFootprint(mCollisionFootprint, position, rotation, out startTileLocation);

            int minX, minZ, maxX, maxZ;
            // If there is no collision min and max remain at 0.
            GetCollisionDepthMinMax(destFootprint, collisionFootprint, out minX, out minZ, out maxX, out maxZ);

            if (minZ > 0)
                z = position.z + (minZ * interior.TileSize.z);
            else
                if (maxZ > 0) z = position.z - (maxZ * interior.TileSize.z);

            if (minX > 0)
                x = position.x + (minX * interior.TileSize.x);
            else 
                if (maxX > 0) x = position.x - (maxX * interior.TileSize.x);

            

            return isPlaceable;
        }

        private const int mSnapAlignInterval = 2;
        // NOTE: Hud mSourceEntity is a cloned copy of this Entity.  Cloning within Hud.cs is important so that
        //       we can show a preview version that has a different Red Appearance and still allow
        //       placement tool to actually place a normal version of the entity.
        private void UpdatePreviewEntityTransform(RenderingContext context, ModeledEntity previewEntity)
        {
            Vector3d translation = Vector3d.Zero();
            Quaternion rotation = new Quaternion();

            // NOTE: This impactPoint is relative to the region/zone where the pick ray originated 
            // and so is NOT necessarily relative to the region that was picked.
            Vector3d impactPoint = mPickResult.ImpactPointRelativeToRayOriginRegion;

            bool terrainPlacement = UseTerrainAutoHeightPlacement(mPickResult.Entity);

            Interior interior = mPickResult.Entity as Interior;
            if (interior != null && mPickResult.FaceID >= 0)
            {
                // NOTE: Entity Script should always be null because we only load
                //       geometry for the mSource preview entity. This is by design.
                if (previewEntity.Script == null)
                {
                    if (terrainPlacement)
                        translation = impactPoint;
                    else
                    {
                        // TODO: ImpactPointLocalSpace works but ImpactPointRelativeToRayOriginRegion which we use
                        //       during outdoor scenes to place entities in any paged in terrain Zone does not.
                        //       I guess the question is, why aren't ImpactPointLocalSpace and ImpactPointRelativeToRayOriginRegion
                        //       not identicle for cases such as Interior celledRegion of a starship?
                        // TODO: this snaps the component position to the center of a Cell's TILE.  
                        // Cell dimensions are only used for Edge segments or floor/ceiling segments and hatches.
                        if (_brushStyle == BRUSH_SINGLE_DROP)
                        {
                            impactPoint = mPickResult.ImpactPointLocalSpace;
                            //translation = impactPoint; // interior.TileCenterPositionFromPoint(impactPoint);
                            // snap to the center of the cell
                            translation = (mPickResult.FacePoints[0] + mPickResult.FacePoints[2]) * .5f;
                        }
                        else if (_brushStyle == BRUSH_HATCH)
                        {
                            // snap to the center of the cell
                            translation = (mPickResult.FacePoints[0] + mPickResult.FacePoints[2]) * .5f; // NOTE: we also have pickResult.CellLocation but those are cell indices not actual 3D coords as is FacePoints
                            ComponentTranslation = translation;
                        }
                    }
                }
                else // the interior.cs instance has a script we can query for component placement within it
                {
                    // Here we call placement calculation function on the interior.cs instance script

                    // That's what should be occurring in QueryCellPlacement() call further below based on the child entity's brushStyle
                    Vector3d position = mPickResult.ImpactPointLocalSpace;
                    // TODO: why is it the _brushStyle isn't set properly when preview mSource entity is loaded?  I think its because
                    //       we delay loading of the script
                    // we should pass the brushstyle to QueryCellPlacement rather than have the interor.cs script query the EntityAPI to find out right?
                    // if the mSource does not have a script, we can pass default "0" style.
                    System.Diagnostics.Debug.Assert(previewEntity == mSource);
                    _brushStyle = (uint)previewEntity.Execute("QueryPlacementBrushType", null);

                    byte cellRotation = 0;
                    if (Rotations != null) cellRotation = Rotations[0];
                    // NOTE: This is calling the _interior_ entity's script NOT the _mSource_ (previewEntity) entity.
                    // Should this be the official way we handle placement within the interior?  
                    // I think it does make sense and is better than calling QueryCellPlacement on each child entity when
                    // that child entity doesn't know about the environment it's being placed and whether its even allowed to be placed there.
                    Vector3d[] vecs = (Vector3d[])interior.Execute("QueryCellPlacement", new object[] { previewEntity.ID, interior.ID, position, cellRotation });
                    translation = vecs[0];

                    //entity.Scale = vecs[1]; // we don't typically want to modify the scale of a prefab do we? We want to set the model scales properly in the saved prefab so that the Entity.Scale can be 1,1,1
                    if (vecs.Length == 3)
                        rotation = new Quaternion(vecs[2].y * MathHelper.DEGREES_TO_RADIANS,
                                                    vecs[2].x * MathHelper.DEGREES_TO_RADIANS,
                                                    vecs[2].z * MathHelper.DEGREES_TO_RADIANS);

                    double x, z;
                    bool passes = PassesCollisionTest(interior, translation, rotation, out x, out z);
                    if (!passes)
                    {
                        // NOTE: The min/max values returned are in snap positions
                        translation.x = x;
                        translation.z = z;
                    }
                }
            }
            else // we're not placing inside an Interior
            {
                // TODO: if we're in FloorPlanWorkspace we should not allow placing outside of interior
                //       in other words we should avoid the falling call
                if (terrainPlacement)
                    translation = impactPoint;
                else
                    translation = context.GetNonCelledPerfectFitPlacementPosition(mSource, MousePosition3D);
            }



            if (PlacementMode == PlacementMode.UseExistingPreviewEntityPosition)
            {
                translation = previewEntity.Translation;
            }
            ComponentTranslation = translation;
            ComponentRotation = rotation;
        }

        /// <summary>
        /// Use our Keystone.EditTools.ManipulatorFunctions.cs to find the rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion UpdatePreviewEntityRotation()
        {
            // mManipulatorFunctions[TransformationMode.Rotation][AxisFlags.Y](mSource, _viewport, mouseStart, mouseEnd, AxisFlags.Y, VectorSpace.World);
            // return mSource.Rotation;
            Quaternion result; // = mSource.Rotation;

           // result = Keystone.EditTools.RotationFunctions.Rotate(mSource, _viewport, mMouseStart, mMouseEnd, AxisFlags.Y, VectorSpace.World);
            result = RotationFunctions.RotateYAxis(mSource, mMouseStart, mMouseEnd);
            mSource.Rotation = result; // subsequent calls to Rotate require the initial rotation be correct
            return result;
        }

        private void DefaultExterior_MouseDown(RenderingContext context, Entity region, Vector3d mousePosition3D)
        {
            Vector3d translation = Vector3d.Zero();
            Entity parent = region;

            if (PlacementMode == PlacementMode.MouseHitLocation)
            {
                // NOTE: This impactPoint is relative to the region/zone where the pick ray originated 
                // and so is NOT necessarily relative to the region that was picked.
                Vector3d impactPoint = mPickResult.ImpactPointRelativeToRayOriginRegion;

                bool placementOnTerrain = UseTerrainAutoHeightPlacement(mPickResult.Entity);

                if (placementOnTerrain)
                    // we are placing prefab at the x,z coordinate where mouse has intersected terrain.
                    // NOTE: currently for placement on terrain, the placed asset is still parent of the Region and in region space coords.
                    translation = impactPoint;
                else if (mPickResult.Entity is Root == false && mPickResult.Entity is Zone == false && mPickResult.Entity is Entity)
                {
                    // we are placing the prefab at the impactpoint of the targeted ModeledEntity 
                    // todo: do we need to modify the parentEntity to reflect we are setting the pickResult.Entity as the parent?
                    // to test this, we can move the Vehicle for instance, and see if the hardpoints move with it.  We may need to
                    // set the NOT relative impact point and then set the target parent.
                    parent = mPickResult.Entity;
                    translation = mPickResult.ImpactPointLocalSpace;
                }
                else
                    // we are placing prefab floating in space in front of camera 
                    translation = context.GetNonCelledPerfectFitPlacementPosition(mSource, mousePosition3D);

                //if (parentEntity is Region == false)
                //    // if something is mousepicked and we're 
                //    // attempting to attach as sub-child entity (TODO: im fairly certain the translation for parenting is computed totally wrong)
                //    translation += mPickResult.ImpactPointRelativeToRayOriginRegion;
            }

            // we must re-send this message everytime we wish to place another instance of this prefab
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Prefab_Load(_relativeArchivePath, _prefabPathInArchive);
            ((KeyCommon.Messages.Prefab_Load)mNetworkMessage).ParentID = parent.ID;
            ((KeyCommon.Messages.Prefab_Load)mNetworkMessage).Position = translation;
            ((KeyCommon.Messages.Prefab_Load)mNetworkMessage).GenerateIDs = true;
            ((KeyCommon.Messages.Prefab_Load)mNetworkMessage).Recurse = true;
            ((KeyCommon.Messages.Prefab_Load)mNetworkMessage).DelayResourceLoading = false; // this needs to be false because scripts need to be loaded It's only textures and geometry that can be skipped

            // note: we must send this as a command and then actually addchild
            // in the response from the server because Entity's need their
            // ID's / GUID's computed on server.
            // Other benefit is we have common point for XMLDB saving when loading
            // new entities
            // however the main concern is that we are sending strings for the path
            // of archive and path to entry in archive and then the server has to
            // send back same thing for our callback handler to process...
            // is there an elegant way to avoid having to round trip this data?
            // Well one way potentially could be to initially have our available archives
            // in an array and then to specify the archive via an index and 
            // same for our file entry...
            // TODO: We wont worry about sending the strings for now... we can fix this
            // at some later date.

            mNetClient.SendMessage(mNetworkMessage);
        }

        private void DefaultExterior_MouseUp()
        {
        }
        
        /// <summary>
         /// OBSOLETE? This method is never used. Doors are placed by PlaceEntityInInterior() - // Places entities (eg. Doors, Windows) onto structural segments (Walls) of the Interior. 
         /// </summary>
         /// <param name="celledRegionID"></param>
         /// <param name="edgeID"></param>
         /// <param name="rotation"></param>
        private void PlaceEntityInSegment(string celledRegionID, uint edgeID, Quaternion rotation)
        {
            Interior celledRegion = (Interior)mPickResult.Entity;
            System.Diagnostics.Debug.Assert(celledRegion.ID == celledRegionID);

            if (mSource.Footprint == null) return; // doors should always have footprint right?
            Vector3d position;
            Vector3i tileLocation;
            int[,] footprint = null;
            // TODO: if mSource contains a ModelSelector, how do we know which Model to select so that we can query it's footprint?
            //       I think the idea is that we use a single Entity footprint for all models. So a door or hatch with two half models under
            //      a ModelSequence just use the overall Entity footprint. I think this is fine, but what we should NEVER do for 1.0 is
            //      allow components to have nested Entities and this way we don't have to deal with nested footprints either.
            if (mSource.Footprint != null) footprint = mSource.Footprint.Data;
            bool isPlaceable = celledRegion.IsEdgeComponentPlaceable(footprint, edgeID, out position, out tileLocation);

            if (isPlaceable == false) return;

            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Place_Entity_In_EdgeSegment(_relativeArchivePath, _prefabPathInArchive);

            // note: for placing entities, for now we only support one at a time but the overload exists for multiple
            ((KeyCommon.Messages.Place_Entity_In_EdgeSegment)mNetworkMessage).EdgeID = edgeID;
            ((KeyCommon.Messages.Place_Entity_In_EdgeSegment)mNetworkMessage).Rotation = rotation;
            ((KeyCommon.Messages.Place_Entity_In_EdgeSegment)mNetworkMessage).ParentStructureID = celledRegionID;

            mNetClient.SendMessage(mNetworkMessage);
        }

        private void PlaceEntityInInterior(string interiorID, Keystone.Types.Vector3d position, Quaternion rotation, KeyCommon.Messages.ComponentType componentType)
        {
            Keystone.Portals.Interior interior = (Keystone.Portals.Interior)mPickResult.Entity;

            System.Diagnostics.Debug.Assert(interior.ID == interiorID);

            
            int[,] footprint = null;
            // TODO: if the entity contains and Actor3d with no footprint, we cant place it!
            //       Do actors need footprints?  It would mean everytime an actor moves, it must
            //       remove and update its footprint in the tilemask grid.  That in turn would cause
            //       Area generation to recompute.  But we dont want to be able to place crew
            //       on obstacles and with no footprint, there's no way to prevent that.  Should we
            //       even place crew from AssetPlacementTool?  Crew should just be added at runtime
            //       Maybe they can have a footprint, but which does not require regenerating areas and portals?
            //       AFterall, their footprints wont be "obstacles" but rather "actors" so should not require
            //       area and portal regeneration.  I could use a blank footprint.

            System.Diagnostics.Debug.Assert(mSource.Footprint != null);
      		if (mSource.Footprint != null) footprint = mSource.Footprint.Data ;

            // NOTE: rotation of footprint is performed within the call to IsChildPlaceableWithinInterior
            int[,] destFootprint;
            bool isPlaceable = interior.IsChildPlaceableWithinInterior(footprint, position, rotation, out destFootprint);

            if (isPlaceable == false) 
            {
            	System.Diagnostics.Debug.WriteLine ("AssetPlacementTool.PlaceEntityInInterior() - NOT PLACEABLE");
            	// TODO: this preview does not update the original on disk!  thus when command goes over wire
            	//        we end up reloading from the disk version that has no footprint set and we do another
            	//        placemenet validity test for security and that fails.  so what option now?
            	//        - further, how does that affect resizing these autogen components?!
            	return;
            }


            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Prefab_Insert_Into_Interior(_relativeArchivePath, _prefabPathInArchive);

            ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).ComponentType = componentType ; // eg EdgeComponent {doors, window'ed wall sections, railings}
            if (componentType == KeyCommon.Messages.ComponentType.EdgeComponent)
                ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).Index = (uint)mPickResult.EdgeID;
            else
                ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).Index = (uint)mPickResult.FaceID;

            // note: for placing entities, for now we only support one at a time but the overload exists for multiple
            // TODO: server should validate position is snapped to correct location
            ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).Position = position;
            ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).Rotation = rotation;
            ((KeyCommon.Messages.Prefab_Insert_Into_Interior)mNetworkMessage).ParentID = interiorID;

            mNetClient.SendMessage(mNetworkMessage);
        }
        /// <summary>
        /// This only places Entities and NOT Wall or Floor structural segments.  
        /// (see KeyCommon.Messages.CelledRegion_Paint for structural segment painting)
        /// </summary>
        /// <param name="structureID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        private void PlaceEntityInStructure(string structureID, Keystone.Types.Vector3d position, Quaternion rotation)
        {
            Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)mPickResult.Entity;
            System.Diagnostics.Debug.Assert(structure.ID == structureID);

            int[,] footprint = null;
        		if (mSource.Footprint != null) footprint = mSource.Footprint.Data ;
        		
        		// TODO: could i add a IsChildPlaceableWithinInterior that does footprint test for us?
            bool isPlaceable = true; // structure.IsChildPlaceable(footprint, position, rotation);

            if (isPlaceable == false) 
            {
            	System.Diagnostics.Debug.WriteLine ("AssetPlacementTool.PlaceEntityInStructure() - NOT PLACEABLE");
            	// TODO: this preview does not update the original on disk!  thus when command goes over wire
            	//        we end up reloading from the disk version that has no footprint set and we do another
            	//        placemenet validity test for security and that fails.  so what option now?
            	//        - further, how does that affect resizing these autogen components?!
            	return;
            }


            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Prefab_Insert_Into_Structure(_relativeArchivePath, _prefabPathInArchive);

            // note: for placing entities, for now we only support one at a time but the overload exists for multiple
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).Positions = new Keystone.Types.Vector3d[] { position };
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).Rotations = new Quaternion[] { rotation };
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).ParentStructureID = structureID;

            mNetClient.SendMessage(mNetworkMessage);
        }

        /// <summary>
        /// Places a copy of a prefab at every cell specified using the specified rotations.
        /// </summary>
        /// <param name="structureID"></param>
        /// <param name="positions"></param>
        /// <param name="rotations"></param>
        private void PlaceEntityInStructure(string structureID, Keystone.Types.Vector3d[] positions, Quaternion[] rotations)
        {
            if (positions == null || rotations == null) throw new ArgumentNullException();
            if (positions.Length != rotations.Length) throw new ArgumentOutOfRangeException();

            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Prefab_Insert_Into_Structure(_relativeArchivePath, _prefabPathInArchive);
            
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).Positions = positions;
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).Rotations = rotations;
            ((KeyCommon.Messages.Prefab_Insert_Into_Structure)mNetworkMessage).ParentStructureID = structureID;

            mNetClient.SendMessage(mNetworkMessage);
        }

        protected override void DeActivate()
        {
            base.DeActivate();

            // NOTE: Hud.cs is responsible for drawing the preview... how do we get
            // hud.cs to remove after the tool is deactivated?

            mWallRotations = null;
            mPrevTiles = null;
            mPickResult = null;
        }

        #region IDisposable Members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            DeActivate();

            if (mWaitDialog != null)
                mWaitDialog.Close();

            // NOTE: The IncrementRef() and DecrementRef() are done in the base class
            //if (mSource != null)
            //{
            //    Keystone.Resource.Repository.IncrementRef(mSource);
            //    Keystone.Resource.Repository.DecrementRef(mSource);
            //}
        }
        #endregion
    }
}
