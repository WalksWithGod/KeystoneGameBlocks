using System;
using KeyCommon.Traversal;
using Keystone.Collision;
using Keystone.Elements;
using Keystone.Extensions;
using Keystone.Types;

namespace Keystone.TileMap
{
    /// <summary>
    /// A TileMap.Structure should be considered to be like Terrain - a complex modeled
    /// entity consisting solely of _structure_ and NOT actually itself being a CONTAINER
    /// such as a Zone or Region.  Thus entities should not exist inside of "Structure" but rather
    /// they exist inside of the Zone or Region and then we can raycast within the Structure to find
    /// things like FloorHeightt and Level of a structure that the Entity (such as NPC) is at.
    /// 
    /// Thus we can have different types of Structure such as a more traditional Chunked Terrain
    /// by swapping in that type rather than TileMap based structure.
    /// </summary>
    /// <remarks>	
    /// TODO 1: I still don't like how the MapLayer stuff is tied in with the Visual Model.  Why aren't those
    ///         seperate?  I think it's something i should still eventually do because the two really are independant
    ///         of each other and should remain so.
    /// TODO 2: but what about static items like rocks or trees?
    /// and what about the "physical" updates of those things when terrain is destroyed at run-time or when
    /// river paths need to change along with changes in structure?
    ///	even NPCs may be destroyed for instance on a cave-in.  Do we just use events 
    /// to handle movement of those entities that are affected by changes in structure?
    /// </remarks>
    public class Structure : Keystone.Entities.ModeledEntity
    {
        // TODO: should not be hardcoding these values
        internal int MINIMUM_FLOOR_LEVEL = -63;
        internal int MAXIMUM_FLOOR_LEVEL = 63;

        uint initialMinimeshCapacity = 8;  // i believe this must always be at least 1 or higher but i think we'll always use multiple of 2
                                           // TODO: FloorHeight remove hardcode.  it should match size of our wall heights... perhaps initially assigned from Settings
        public double FloorHeight = 2.82983f; // NOTE: keep as double or precision can screw up and our meshes have seams

        internal float mWidth { get; set; }  // size along X axis (width) in world units of structure
        internal float mDepth { get; set; }   // size along Z axis in world units of this structure
        internal float mHeight { get; set; }   // size along Y axis in world units of this structure


        // NOTE: mModelLookupPaths and mStructureMinimeshes can be shared between all levels so are not
        // a member of that struct as mMapLayerNames, mMapLayerPaths and mMapLayers
        // NOTE2: since mModelLookupPaths are serialized by Structure.cs we can control the order in which
        // these referenced entities/segments are loaded... particularly that they can be loaded before 
        // we try to read and restore MapLayer data from bitmap storage.
        private string[] mSegmentLookupPaths;

        // collection of prefab resource descriptors
        System.Collections.Generic.Dictionary<string, Keystone.Entities.ModeledEntity> ReferencedSegments;

        internal StructureLevel[] mLevels;

        internal Structure(string id) : base(id)
        {
        }

        #region ITraverse
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        #region IResource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
            tmp.CopyTo(properties, 2);


            properties[0] = new Settings.PropertySpec("modellookuppaths", typeof(string[]).Name);
            properties[1] = new Settings.PropertySpec("maplevels", typeof(string).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mSegmentLookupPaths;
                properties[1].DefaultValue = mMapLevelsPersistString;
            }

            return properties;
        }

        private string mMapLevelsPersistString;
        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].DefaultValue == null) continue;
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "maplevels":
                        // TODO: if this is a CSV of floorLevels numbers, we can instantiate 
                        // our mLevels[] with appropriate number.  Then if we've also got
                        // info stored on the map layer names and paths, we can continue
                        // loading.
                        mMapLevelsPersistString = (string)properties[i].DefaultValue;

                        break;
                    case "modellookuppaths":
                        mSegmentLookupPaths = (string[])properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        #region Naming Conventions
        public static readonly char PREFIX_DELIMIETER = '_';
        public static readonly string LAYERPATH_DELIMIETER = ",";
        public static readonly string SENTINEL = "$";

        public static string GetStructureID(int subscriptX, int subscriptY, int subscriptZ)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                 "structure", LAYERPATH_DELIMIETER,
                                 subscriptX, LAYERPATH_DELIMIETER,
                                 subscriptY, LAYERPATH_DELIMIETER,
                                 subscriptZ, LAYERPATH_DELIMIETER);
        }


        public static string GetStructureElementPrefix(string structureID, Type t)
        {
            return SENTINEL + PREFIX_DELIMIETER +
                structureID + PREFIX_DELIMIETER +
                t.Name;
        }
        public static string GetStructureElementPrefix(string structureID, string description, Type nodeType)
        {
            return GetStructureElementPrefix(structureID, nodeType)
                + PREFIX_DELIMIETER +
                description;
        }

        public static string GetStructureElementPrefix(string structureID, string description, Type nodeType, int index)
        {
            return GetStructureElementPrefix(structureID, description, nodeType)
                + PREFIX_DELIMIETER +
                index.ToString();
        }


        public static string GetLayerPath(int locationX, int locationZ, int level, string layerName)
        {
            //Core._Core.SceneManager.Scenes[0].Name  // Name here is filename
            //Core._Core.SceneManager.Scenes[0].ModName // Mod name
            //Core._Core.SceneManager.Scenes[0].RelativeZoneDataPath = "zones//"

            string sceneName = Core._Core.Scene_Name;
            string relativeLayerDataPath = System.IO.Path.Combine(sceneName, Core._Core.RelativeLayerDataPath);

            string path = System.IO.Path.Combine(Core._Core.ScenesPath, relativeLayerDataPath);

            string filename = string.Format("{0}{1}{2}{3}{4}{5}{6}.bmp", locationX, LAYERPATH_DELIMIETER,
                                                                            locationZ, LAYERPATH_DELIMIETER,
                                                                               level, LAYERPATH_DELIMIETER,
                                                                               layerName);
            path = System.IO.Path.Combine(path, filename);
            return path;
        }

        public static string GetLayersDataPath(int locationX, int locationZ)
        {
            string sceneName = Core._Core.Scene_Name;
            string relativeLayerDataPath = System.IO.Path.Combine(sceneName, Core._Core.RelativeLayerDataPath);

            string path = System.IO.Path.Combine(Core._Core.ScenesPath, relativeLayerDataPath);

            string filename = string.Format("{0}{1}{2}.layerdata", locationX, LAYERPATH_DELIMIETER, locationZ);
            path = System.IO.Path.Combine(path, filename);
            return path;
        }
        #endregion

        #region IGroup
        public override void AddParent(IGroup parent)
        {
            base.AddParent(parent);

            Keystone.Portals.Region r = (Keystone.Portals.Region)parent;
            mWidth = (float)r.BoundingBox.Width;
            mDepth = (float)r.BoundingBox.Depth;
            mHeight = (float)r.BoundingBox.Height;

        }

        public override void AddChild(Keystone.Entities.Entity child)
        {
            // TODO: but what about static items like rocks or trees?
            //       and what about the "physical" updates of those things when terrain is destroyed at run-time or when
            //       river paths need to change along with changes in structure?
            //       even NPCs may be destroyed for instance on a cave-in.  Do we just use events
            //       to handle movement of those entities that are affected by changes in structure?
            // TODO: maybe Structure should be a type of Region just as Interior.cs is and inherit Region not ModeledEntity.
            //       This works fine for Interior and really they are the same thing.
            //       Or Structure is considered to be more like a Geometry.cs node like TVLandscape/Terrain.cs.  In the terrain case
            //       entities placed ontop of it are part of the Region that the Terrain exists in.  So then we still need to mousepick the terrain and then 
            //       AddChild to the parent region.  The parent Region can contain our octree.
            //       But again, Interior.cs does not behave like this.  What's the difference?  Clearly TVLandscape based Terrain is not a Region though. Hmm.
          
            throw new Exception("TileMap.Structure should be viewed as Terrain and not as a Container or Region or Zone.  Entities should be added to a TileMap.Structure's parent Region and not to the TileMap.Structure itself."); // 
        }
        #endregion

        public uint LevelCount
        {
            get
            {
                if (mLevels == null) return 0;
                return (uint)mLevels.Length;
            }
        }

        public int[] GetActiveFloorLevels()
        {
            if (mLevels == null) return null;

            int[] results = new int[mLevels.Length];

            for (int i = 0; i < results.Length; i++)
                results[i] = mLevels[i].FloorLevel;

            return results;
        }

        public void Layer_GetDimensions(int floorLevel, string layerName, out int tileCountX, out int tileCountZ)
        {
            int levelIndex = FindLevelIndex(floorLevel);
            tileCountX = mLevels[levelIndex].mMapLayers[0].mTileData.Width;
            tileCountZ = mLevels[levelIndex].mMapLayers[0].mTileData.Height;
        }

        public int[,] Layer_GetData(int floorLevel, string layerName)
        {
            // TODO: we need a good way to grab all of this data as an int[] and/or int[,] 
            //       Pixels byte where each byte is either r, g, b, or a if applicable depending on color depth of the bitmap 
            // we'll copy what our autotile does in the short term by building the array here using loop of getpixel
            int levelIndex = FindLevelIndex(floorLevel);
            int layerIndex = this.FindLayerIndex(levelIndex, layerName);
            LockBitmap bmp = mLevels[levelIndex].mMapLayers[layerIndex].mTileData;

            int width = bmp.Width;
            int height = bmp.Height;
            int[,] result = new int[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    result[i, j] = bmp.GetPixel(i, j).ARGB;
                }

            return result;
        }

        public object Layer_GetValue(int floorLevel, string layerName, uint tileID)
        {
            int levelIndex = FindLevelIndex(floorLevel);
            throw new NotImplementedException();
            return null;
        }

        public void Layer_SetValue(int floorLevel, string layerName, uint tileID, object value)
        {
            // no validation.  Validation is more app centric (eg CommandProcessor)
            // and script centric and should not be hard coded here.            
            int levelIndex = FindLevelIndex(floorLevel);
            //System.Diagnostics.Debug.WriteLine ("Structure.Layer_SetValue() - Level Index = " + levelIndex.ToString());

            if (value == null)
            {
                // This is a delete operation.  If we're out of range of existing level indices
                // we just abort
                if (levelIndex < 0 || levelIndex >= mLevels.Length)
                    return;


                // TODO: if after the delete operation, the entire level is made clear of any Terrain or Structural data, do we delete the level and all it's layers?
                // including supporting layers like obstacles, style?
            }

            // If the layer does not exist, then we need to create it
            if ((levelIndex < 0 || levelIndex >= mLevels.Length) && layerName == "layout")
            {

                // if we've reached max levels above or below, we abort
                if (MINIMUM_FLOOR_LEVEL == floorLevel ||
                    MAXIMUM_FLOOR_LEVEL == floorLevel)
                    return;

                //            	// find either next lowest floorID or next highest depending if we are trying to insert
                //            	// below the lowest index or above the highest
                //            	if (value == null) // null value is dig operation
                //            		floorLevel--; 
                //            	else 
                //            		floorLevel++;

                AddLayer(floorLevel, "obstacles");
                AddLayer(floorLevel, "layout");
                AddLayer(floorLevel, "style");

                levelIndex = this.FindLevelIndex(floorLevel);
            }


            StructureLevel level = mLevels[levelIndex];

            // TODO: remove hardcode layer index (int)LayerType.terrain and determine it based on
            //       layerName i think right?
            MapLayer layer = level.mMapLayers[(int)LayerType.terrain];

            // NOTE: since each layer can have it's own independant resolution, the resulting x,z are 
            //       specific to just that layer. However, for the same name layer on different StructureLevels
            //       i'll probably enforce they all be the same at least within the same Structure.
            int x, z;
            layer.UnflattenIndex(tileID, out x, out z);

            switch (layerName)
            {
                case "obstacles":
                    break;
                case "layout":
                    int segmentIndex = 0;

                    if (value != null)
                    {
                        string segmentPath = ((string)value).ToLower();
                        if (ReferencedSegments == null) throw new Exception();

                        int i = 0;

                        if (string.IsNullOrEmpty(segmentPath))
                            segmentIndex = 0;
                        else
                        {
                            // Find correct ModelSelector index for this segmentPath within this Structure
                            // NOTE: index will start at 1 because index 0 in our ModelSelector.Children[] 
                            // is reserved for EMPTY segment
                            i = 1;
                            foreach (string key in ReferencedSegments.Keys)
                            {
                                if (key == segmentPath)
                                {
                                    segmentIndex = i;
                                    break;
                                }
                                i++;
                            }
                        }
                        if (segmentIndex == -1) throw new Exception();
                    }

                    layer.SetMapValue(x, z, segmentIndex);
                    break;
                case "style":
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Structure.Layer_SetValue() - Unsupported layer " + layerName);
                    break;
            }
        }


        private void MapLayer_OnValidate(MapLayer layer, int x, int z, int value, out bool cancel)
        {
            cancel = false;

            // if we are deserializing (as opposed to making live editor changes) we don't validate
            if (mDeserializationMode) return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        private void MapLayer_OnValueChanged(MapLayer layer, int x, int z, int value)
        {
            Pixel pixel = Pixel.GetPixel(value);

            System.Diagnostics.Debug.Assert(layer != null);

            // raised from MapLayer.SetValue() and called from Layer_SetValue and PropogateChangeFlags

            // segment properties (double sided, thickness, etc)
            // segment appearance (atlas texture index)


            if (layer.Name == "obstacles")
            {
                // NOTE: because of the way AutoTile() has to run recursively
                // and because the footprint depends on which sub-tile is chosen,
                // we call UpdateObstacleMap() from inside of AutoTile() when the "layout" 
                // has changed.
            }
            else if (layer.Name == "style")
            {
            }
            else if (layer.Name == "layout")
            {
                int floor = layer.FloorLevel;
                int levelIndex = this.FindLevelIndex(floor);
                StructureLevel level = mLevels[levelIndex];

                int segmentIndex = pixel.B;

                // TODO: can we always assume that the style layer for terrain or structure is always layer.Index + 1
                int layerIndex = FindLayerIndex(levelIndex, layer.Name);

                // grab a reference to the STYLE layer for this LAYOUT layer so that we can notify it
                MapLayer styleLayer = level.mMapLayers[layerIndex + 1]; // style is always next index from "layout"

                // segment.Execute ("Apply", new object[] { this.ID, layer, styleLayer, x, z, segmentIndex, mAutoTileAdjacents});
                // NOTE: "AutoTile" is recursive so we can't just return a value here... it must respond to changed values of adjacents
                //segment.Execute("AutoTile", new object[] { layer, styleLayer, x, z, segmentIndex, mAutoTileAdjacents});

                // the segment's sub-model (visual style) is determiend from pattern rules.  TODO: These rules may be loaded from scipt but currently are hardcoded
                // TODO: when/where do we save changes to the MapLayer files?
                bool updateObstacleLayer = true;
                AutoTile(layer, styleLayer, x, z, segmentIndex, updateObstacleLayer, mDeserializationMode == false, mDeserializationMode);

                // TODO: are we computing bounding box of Structure properly?
                //       also seems an issue with Structure being re-inserted at proper depth after it's minis have loaded
                SetChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Self);
            }
        }


        public void AddLayer(int floorLevel, string layerName)
        {
            // TODO: we need to ensure we are inserting these levels in to mLevels array
            //       at appropriate indices
            StructureLevel level = this.AddLevel(floorLevel);

            // TODO: this zone is null when deserializing structure... 
            Keystone.Portals.Zone zone = (Keystone.Portals.Zone)this.Parent;
            Keystone.Portals.ZoneRoot zoneRoot = (Keystone.Portals.ZoneRoot)zone.Parent;

            System.Diagnostics.Debug.Assert(
               zone.ArraySubscript[1] == 0, "zone.ArraySubscript[1] must always be 0 because the Y Axis of our universe must never be > 1 Zone high when using Structures inside Zones.");

            int offsetX = zone.ArraySubscript[0];
            // NOTE: offsetY must take into account the relative floorLevel of this, as well as the gorund floor index of our entire game world
            int offsetY = zone.ArraySubscript[1] + (int)zoneRoot.StructureGroundFloorIndex + floorLevel;
            int offsetZ = zone.ArraySubscript[2];


            MapLayer layer;

            if (mDeserializationMode == false)
                layer = Core._Core.MapGrid.Create(layerName, offsetX, offsetY, offsetZ, floorLevel, mWidth, (float)FloorHeight, mDepth);

            // always bind events whether deserialization mode or not
            layer = Core._Core.MapGrid.Bind(layerName, offsetX, offsetY, offsetZ, MapLayer_OnValueChanged, MapLayer_OnValidate);

            // because level is a struct, we modify at array because otherwise we're modifying a value type
            // which wont affect the element in array
            level.AddMapLayer(layer);
        }

        private StructureLevel AddLevel(int floorLevel)
        {
            // if the StructureLevel didnt exist we must also add a Segment Selector as child to the rootSelector
            ModelSelector rootLevelSelector = (ModelSelector)this.Children[0];
            ModelSelector segmentSelector = (ModelSelector)Resource.Repository.Create("ModelSelector");

            StructureLevel level;
            int levelIndex = this.FindLevelIndex(floorLevel);
            if (levelIndex == -1)
            {
                // create a new level - insert below, or append above depending on floorLevel
                // NOTE: We do NOT support inserting BETWEEN floorLevels!
                level = new StructureLevel(floorLevel);

                if (mLevels == null)
                {
                    levelIndex = 0;
                    mLevels = mLevels.ArrayAppend(level);
                    rootLevelSelector.AddChild(segmentSelector);
                }
                else if (floorLevel > mLevels[mLevels.Length - 1].FloorLevel)
                {
                    // this floorLevel is > current top floor level
                    levelIndex = mLevels.Length;
                    mLevels = mLevels.ArrayAppend(level);
                    rootLevelSelector.AddChild(segmentSelector);
                }
                else
                {
                    // this floorLevel is lower than lowest existing 
                    levelIndex = 0;
                    mLevels = mLevels.ArrayInsertAt(level, (uint)levelIndex);
                    // _MUST_ insert the child ahead of all others if we are inserting 
                    // a new floor beneath current lowest
                    rootLevelSelector.InsertChild(segmentSelector);
                }

                // the first Segment under each Level is an empty Segment that represents TileType.TT_EMPTY
                ModelSelector emptySegment = (ModelSelector)Resource.Repository.Create("ModelSelector");
                segmentSelector.AddChild(emptySegment);

                //System.Diagnostics.Debug.Assert (levelIndex == rootLevelSelector.ChildCount - 1, "StructureLevel indices and rootLevelSelector's children should have sychronized indices");

                // clone the ReferencedSegments and add them to this new level
                foreach (Keystone.Entities.Entity segment in ReferencedSegments.Values)
                {
                    // first child is a script
                    // second child of the segment is the ModelSelector which is what we clone
                    ModelSelector modelSelector = (ModelSelector)segment.Children[1];

                    string id = Keystone.Resource.Repository.GetNewName(typeof(ModelSelector));
                    ModelSelector clonedModelSelector = (ModelSelector)modelSelector.Clone(id, true, false, false);

                    // the referenced segments has .Mesh3d Geometry nodes and we need to replace all of those with MinimeshGeometry			
                    // for every model that makes up the Segment, ensure a MinimeshGeometry has been created	
                    level.CreateMinimeshRepresentations(clonedModelSelector, initialMinimeshCapacity, FloorHeight);

                    segmentSelector.AddChild(clonedModelSelector);
                }
            }
            else level = mLevels[levelIndex];

            return level;
        }

        private string GetSegmentName(Structure structure, int segmentIndex)
        {
            // find the appropriate Segment

            // TODO: Verify this works and that all segmentIndex values in all Levels within the same Structure are always
            // in the same order no matter when new Segment's are added to Lookup  and no matter when new Levels are added to Structure.
            string name = mSegmentLookupPaths[segmentIndex - 1]; // subtract 1 because segmentIndex 0 represents null/empty tile segment.
            return name;
        }

        internal int FindLevelIndex(double y)
        {
            // find the level that contains the y coordinate value
            if (mLevels == null) return -1;

            for (int i = 0; i < mLevels.Length; i++)
            {
                int level = mLevels[i].mMapLayers[0].FloorLevel;
                double levelHeight = level * this.FloorHeight;
                // TODO: Feb.9.2016 - this fails on the top most level.  I think it's a case of
                // no extra empty level being added above the last level that contains actual geometry.
                // pathfinding on levels (<= mLevels.Length - 1) works fine.
                // TODO: the fix is during map generation and map editing
                //       DO NOT ALLOW "structure" geometry (i.e. terrain blocks) to be placed in top most level.
                //       That level should be reserved for placing entities.
                if (levelHeight >= y)
                    return i;
            }

            return -1;
        }

        private Structure FindStructure(int subscriptX, int subscriptY, int subscriptZ)
        {
            // we could perhaps deduce the name and then just find the structure from 
            // Repository.  

            // we force subscript 0 since we don't want MapLayer's subscript which is actually 0-64 or 0-128 even)
            string structureID = GetStructureID(subscriptX, 0, subscriptZ);
            Structure result = (Structure)Keystone.Resource.Repository.Get(structureID);
            return result;
        }

        public int FloorLevel(int levelIndex)
        {
            if (mLevels == null) throw new Exception();

            if (levelIndex < 0 || levelIndex > mLevels.Length - 1)
                throw new ArgumentOutOfRangeException();

            return mLevels[levelIndex].FloorLevel;
        }

        public int FloorLevelMinimum()
        {
            return MINIMUM_FLOOR_LEVEL;
        }

        public int FloorLevelMaximum()
        {
            return MAXIMUM_FLOOR_LEVEL;
        }

        public int FindLevelIndex(int floorLevel)
        {
            if (mLevels == null) return -1;

            for (int j = 0; j < mLevels.Length; j++)
                if (mLevels[j].FloorLevel == floorLevel)
                    return j;

            return -1;
        }

        private int FindLayerIndex(int levelIndex, string layerName)
        {
            if (mLevels == null) throw new ArgumentOutOfRangeException("Level does not exist."); // return -1;

            StructureLevel level = mLevels[levelIndex];

            if (level.mMapLayers == null) throw new ArgumentOutOfRangeException("Layer does not exist.");

            for (int i = 0; i < level.mMapLayers.Length; i++)
                if (level.mMapLayers[i].Name == layerName)
                    return i;

            return -1;
        }

        #region Pattern
        Pattern[] mPatterns;

        private Pattern[] CreatePatterns()
        {
            // todo: being able to parse a pattern from a string would be nice

            // create maps for this rule for creating a dirt wall
            // todo: model 0 = null model - has no appearance or geometry
            //
            // model 0 = floor cap
            // model 1 = exterior corner piece (SW)
            //           exterior corner piece (NW)  (90 y-axis rotation)
            //           exterior corner piece (NE)  (180 y-axis rotation)
            //           exterior corner piece (SE)  (270 y-axis rotation)
            //

            // three way junction
            // four way junction
            // 
            Pattern[] patterns = new TileMap.Pattern[3];

            // pattern for empty tile
            patterns[0] = new TileMap.Pattern();
            patterns[0].TileType = TileTypes.TT_EMPTY;
            patterns[0].DefaultIndex = 0;
            patterns[0].Rules = null;

            // pattern for terrain
            patterns[1] = new TileMap.Pattern();
            patterns[1].TileType = TileTypes.TT_FLOOR;
            patterns[1].DefaultIndex = 1;
            patterns[1].Rules = null;

            // pattern for wall structure
            patterns[2] = new TileMap.Pattern();
            patterns[2].TileType = TileTypes.TT_WALL;
            patterns[2].DefaultIndex = 6;
            patterns[2].Rules = null;

            PatternRule r;
            PatternCondition con;

            // NOTE: ORDER OF FOLLOWING PatternRules IS IMPORTANT.
            //       Rules must be added in order of most conditions to least
            //       because the first rule we encounter that passes is the
            //       one that is returned!
            // --------------------------------

            // LEVEL -1 RULES (these include tests for tiletypes in upper level)
            // if all upper adjacents are EMPTY, use FLOOR END CAP MODEL
            //			r = new TileMap.PatternRule ();
            //			r.TileIndex = 2;
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_E;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_N;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_NE;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_NW;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_S;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_SE;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_SW;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_W;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);

            // if .U is EMPTY and any cardinal neighbor 

            // TODO: can i logical OR the TileTypes we're looking for together
            //       so that we can have multiple allowed Types?

            // if all upper adjacents are FLOOR except for .U, use CONCAVE HOLE MODEL,
            //			r = new TileMap.PatternRule ();
            //			r.TileIndex = 2;
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U;
            //			con.Type = TileTypes.TT_EMPTY;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_E;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_N;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_NE;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_NW;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_S;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_SE;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_SW;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			con = new TileMap.PatternCondition ();
            //			con.Direction = TileDirections.U_W;
            //			con.Type = TileTypes.TT_FLOOR;
            //			r.Add (con);
            //			
            //			patterns[1].Add (r);

            // LEVEL 0 RULES
            // if adjacent to nothing in planar cardinal directions, use 4-Sided Column
            r = new TileMap.PatternRule();
            r.TileIndex = 3;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_EMPTY;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_EMPTY;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_EMPTY;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_EMPTY;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to North, South, East, West, and Top, use NULL model 
            r = new TileMap.PatternRule();
            r.TileIndex = 0;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.U;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to North, South, East and West wall, use N/S/E/W  TOP CAP MODEL
            r = new TileMap.PatternRule();
            r.TileIndex = 1;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);


            // if adjacent to E,W and N wall, use a inner wall model (90 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 8; // how to use variants indexed 8 - 11?
            r.TileRotation = 90;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to E,W, and S wall, use inner wall model (270 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 8; // how to use variants indexed 8 - 11?
            r.TileRotation = 270;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to N,S, and E wall, use inner wall model (180 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 8; // how to use variants indexed 8 - 11?
            r.TileRotation = 180;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to N,S, and W wall, use inner wall model (0 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 8; // how to use variants indexed 8 - 11?
            r.TileRotation = 0;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);


            // if adjacent to both a North and South wall, use N/S connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 5;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);


            // if adjacent to both a East and West wall, use E/W connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 5;
            r.TileRotation = 270;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to a North and East wall, use 1 sided corner model (90 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 7;
            r.TileRotation = 90;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to a North and West wall, use 1 sided corner model (0 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 7;
            r.TileRotation = 0;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to a South and West wall, use 1 sided corner model (270 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 7;
            r.TileRotation = 270;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to a South and East wall, use 1 sided corner model (180 degree rotation)
            r = new TileMap.PatternRule();
            r.TileIndex = 7;
            r.TileRotation = 180;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);


            // if adjacent to just a South wall, use N END connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 4;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.S;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to just a North wall, use S END connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 4;
            r.TileRotation = 180;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.N;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to just a East well, use W END connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 4;
            r.TileRotation = 270;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.E;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);

            // if adjacent to just a West wall, use E END connector model
            r = new TileMap.PatternRule();
            r.TileIndex = 4;
            r.TileRotation = 90;
            con = new TileMap.PatternCondition();
            con.Direction = TileDirections.W;
            con.Type = TileTypes.TT_FLOOR;
            r.Add(con);
            patterns[1].Add(r);



            // if the tile below is "unfilled" then it's a hole and we use NO model here (TileIndex == 0 which is our null model)
            // and a NULL Model type is _not_ the same as a NULL Segment type.  A NULL Segment means no segment, buta  null model
            // still means that the segment is still being used there, there's just no need to render a model there.
            // Thus even if no model is rendered, the footprint obstacle data is still there.
            return patterns;
        }
        #endregion

        #region AutoTile
        private bool mDeserializationMode = false;

        private void AutoTile(MapLayer layoutLayer, MapLayer styleLayer, int x, int z, int segmentIndex, bool updateUpperObstacleLayer, bool recurse, bool deserializationMode)
        {
            try
            {
                // NOTE: segmentIndex correlates to a ModelSelector node child and NOT to a Model node.
                //       The Model node is selected dynamically based on pattern rule.
                TileTypes current = (TileTypes)segmentIndex;
                if (current == TileTypes.TT_OOB)
                    return; // out of bounds tiles are skipped, but what if this is a zone boundary and not a world boundary? it should return     				

                // all structures share same ZoneRoot
                Keystone.Portals.ZoneRoot zoneRoot = (Keystone.Portals.ZoneRoot)this.Region.Parent;

                int offsetX = layoutLayer.SubscriptX;
                int offsetY = layoutLayer.SubscriptY;
                int offsetZ = layoutLayer.SubscriptZ;

                System.Drawing.Point[] wrappedTiles;
                MapLayer[][] wrappedLayers;
                int[] adjacentSegments = Core._Core.MapGrid.GetAdjacentTileTypes(LayerType.terrain, offsetX, offsetY, offsetZ, x, z, out wrappedTiles, out wrappedLayers);

                if (mPatterns == null)
                    mPatterns = CreatePatterns();

                System.Diagnostics.Debug.Assert(segmentIndex == (int)mPatterns[segmentIndex].TileType, "TileMap.Structure.AutoTile() - Patterns should be stored by same indexing as their TileType");
                PatternRule selectedRule = mPatterns[segmentIndex].Query(adjacentSegments);
                int modelIndex = selectedRule.TileIndex;
                int tileRotation = selectedRule.TileRotation;

                // TileMap.Pixel Member Info
                // use the B to set segmentIndex value
                // use the G to set modelIndex value
                // use the High (RA) to hold minimeshElement index value

                // prepare new pixel for "STYLE" MapLayer with new model and minimesh indices
                TileMap.Pixel newPixel = TileMap.Pixel.GetPixel(0, 0, (byte)modelIndex, (byte)segmentIndex);

                // RETURN EARLY BEFORE STYLE IS UPDATED & ADJACENT RECURSION - if (previousStyle == new)
                // return immediately and simply re-use existing element
                // No need to recursively auto-tile either
                int previousValue = styleLayer.GetMapValue(x, z);
                TileMap.Pixel previousPixel = TileMap.Pixel.GetPixel(previousValue);
                if (deserializationMode == false && previousPixel.Low == newPixel.Low)
                    return;

                // TODO: and perhaps the only thing our script needs is the Pattern().  
                //       - it doesn't even need any special script for getting footprint right?
                //		- although, not every component AutoTiles but in that case the default pattern can be 
                //        to place the one available model type... even if the script .Pattern returns null
                //        when null, default behavior can be to use model 0. (model 0 is empty for those that have many, but
                //        for components that have only 1 model, then model 0 is used)
                // TODO: UpdateTileStyle perhaps can be a callback function from script to code
                // Since AutoTile() is recursive (just for immediate adjacents of initial tile), we must clear existing STYLE within this AutoTile function.  
                // using "STYLE" MapLayer we clear any minimesh element represented by previous value of this pixel 
                if (deserializationMode == false && previousPixel.B != 0 && previousPixel.G >= 0)
                    // NOTE: we always clear when not in deserialization mode, and that is not the same thing as
                    // recurse = true
                    ClearExistingStyle(styleLayer, x, z);

                UpdateTileStyle(layoutLayer, styleLayer, x, z, newPixel, tileRotation);

                // update obstacle layer for the same levelIndex as the layoutLayer.  
                int levelIndex = FindLevelIndex(layoutLayer.FloorLevel);
                System.Diagnostics.Debug.Assert(levelIndex >= 0);


                // TODO: if this is an adjacent from the recursive call to AutoTile() and is also the ordinal TOP adjacent
                //       then we should UpdateObstacleMap for it by +X where X is a traversability weight of the 
                //       tile below it.  In fact, it would be nice if we could pass the "weight" value in the call
                //       so it can be varied?  The other "footprint" value is different right?  It defines visibility/blocking/cover
                //       - and in fact, we do not want UpdateObstacleMap to EVER be called on any adjacent.  We want to immediately
                //       just call UpdateObstacleMap() for the initial tile and it's top adjacent... and that's it.  
                // TODO: the footprint is between 0 - 63 for walkable tiles
                //       however this footprint is added to above layer AND it's added to the current layer.  Thus if the total weight > 64, then it's unwalkable.
                //       since now we know it's actually blocking.
                //       - unity pro 3d tile map editor has nice way of allowing you to select between some block types with varying 1/8 increment values in heights for 
                //       4 corner vertices of the top cap block.  In reality, these are just different block meshes for each corner height scenario, but it does allow for greater control
                // 	TODO: - there's still the question of how to handle ladders... does a ladder mitigate the "unwalkable" weight of a wall it is against?
                //		 and if so, how does the NPC know there is a ladder on that tile when it gets there?  it has to check a weight flag if it sees that the Y levels are different
                //	     and it has no natural ability to fly up there or drop down there safely.
                // http://archive.gamedev.net/archive/reference/articles/article728.html	
                MapLayer obstacleLayer = this.mLevels[levelIndex].mMapLayers[(int)LayerType.obstacles];
                if (deserializationMode == false && previousPixel.ARGB != -1 && previousPixel.B != 0)
                    // clear obstacle data from PREVIOUS segment <-- TODO: is this the correct obstacle layer? shouldn't it be the one for layer above? is it?
                    ClearObstacleMap(obstacleLayer, x, z, previousPixel, tileRotation);


                // NOTE: it is vital in our 3-D implementation of a tilemap for the weight to be ADDED (to whatever existing weight)
                // in the obstacle map, to both the current level's obstacle layer AND the layer above! <-- TODO: why both?
                // TODO: when a new layer is added, we must Append the obstacle data to it!  I think a way to fix this is to _always_
                //       ensure there is always at least one level above the highest one.
                if (segmentIndex > 0)
                    UpdateObstacleMap(obstacleLayer, x, z, newPixel, tileRotation);

                // collision map data is written to the layer above
                if (updateUpperObstacleLayer)
                {
                    // from the perspective of this current tile, the upper is always
                    // current tile index but in the level above (levelIndex + 1)
                    int upperLevelIndex = levelIndex + 1;
                    if (upperLevelIndex < this.mLevels.Length)
                    {
                        MapLayer upperObstacleLayer = this.mLevels[upperLevelIndex].mMapLayers[(int)LayerType.obstacles];

                        // use previous pixel's segment and footprint of lower floor to CLEAR from upperObstacleLayer
                        if (deserializationMode == false && previousPixel.ARGB != -1 && previousPixel.B != 0)
                            ClearObstacleMap(upperObstacleLayer, x, z, previousPixel, tileRotation, true);

                        // use current pixel's segment and footprint of lower floor to apply to upperObstacleLayer
                        UpdateObstacleMap(upperObstacleLayer, x, z, newPixel, tileRotation, true);
                    }
                }


                // Recursive AutoTile() for adjacents
                // If this tile's model has changed during runtime (as opposed to during loading saved data from disk)
                // then we must AutoTile() it's adjacents so they can select new models to fit with this changed neighbor  
                if (recurse)
                {
                    for (int i = 0; i < adjacentSegments.Length; i++)
                    {
                        bool updateUpperObstacles = false;
                        //  second pancake of the 9x9 rubik cube and are the tiles that require
                        //  for the adjacents above to have the same weights added to their obstacle layers. 
                        //  The problem is, the upper layers will usually have no model there at all.. and so
                        //  no footprint of it's own... how do we get the tile footprint beneath it to be used?
                        switch (i)
                        {
                            // for pathfinding obstacle map, obstacle data from entity written to layout
                            // of this level must be written to obstacle data on above layer
                            // NOTE: TileDirections.Center gets processed during original AutoTile()
                            // call by passing arg updateUpperObstacleLayer = true.
                            case (int)TileDirections.S:
                            case (int)TileDirections.N:
                            case (int)TileDirections.W:
                            case (int)TileDirections.E:
                            case (int)TileDirections.NE:
                            case (int)TileDirections.SE:
                            case (int)TileDirections.SW:
                            case (int)TileDirections.NW:
                                updateUpperObstacles = true;
                                break;
                        }

                        // TileDirections.Center is NOT an adjacent, but rather THIS VERY TILE which was 
                        // processed already in original call to AutoTile() So we must skip it.
                        if (i == (int)TileDirections.Center) continue;

                        // NOTE: we pass 'false' so we don't further recurse adjacents of the current adjacent.
                        // we're only interested in recursing the adjacents of the initial tile of the original AutoTile() call.
                        if (adjacentSegments[i] == (int)TileTypes.TT_EMPTY) continue;

                        AutoTile(wrappedLayers[i][(int)LayerType.terrain],
                                  wrappedLayers[i][(int)LayerType.terrain_style],
                                  wrappedTiles[i].X,
                                  wrappedTiles[i].Y,
                                  adjacentSegments[i],
                                  updateUpperObstacles,
                                  false, deserializationMode);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TileMap.Structure.AutoTile() - AutoTile failed. " + ex.Message);
            }
        }
        #endregion


        private void ClearObstacleMap(MapLayer obstacleLayer, int x, int z, Pixel previousPixel, int tileRotation, bool modWeight = false)
        {
            if (obstacleLayer == null) return;

            int previousSegmentIndex = previousPixel.B;
            int previousModel = previousPixel.G;
            System.Diagnostics.Debug.Assert(previousSegmentIndex > 0);
            System.Diagnostics.Debug.Assert(previousModel > 0); // model index 0 is NULL model and we should just return since nothing to clear on obstacle map

            string previousSegmentName = GetSegmentName(this, previousSegmentIndex);
            Entities.ModeledEntity previousSegment = ReferencedSegments[previousSegmentName];
            int[,] footprintData = new int[,] { { WEIGHT_MODULUS + 1 } }; // previousSegment.Footprint.Data;


            int footprintWidth = footprintData.GetLength(0);
            int footprintDepth = footprintData.GetLength(1);


            for (int i = 0; i < footprintWidth; i++)
                for (int j = 0; j < footprintDepth; j++)
                {
                    int weight = footprintData[i, j];
                    // when modWeight == true, we are modifying an upper layer
                    // and so the full weight is not applied, only a modulus thereof
                    if (modWeight)
                        weight %= WEIGHT_MODULUS;

                    int scaledX = x;
                    int scaledZ = z;
                    int existingValue = obstacleLayer.GetMapValue(scaledX + i, scaledZ + j);
                    int decrementedValue = existingValue - weight;
                    obstacleLayer.SetMapValue(scaledX + i, scaledZ + j, decrementedValue);
                }

        }

        private const int WEIGHT_MODULUS = 63; // 0 - 63 is WALKABLE, 64+ is NON WALKABLE

        private void UpdateObstacleMap(MapLayer obstacleLayer, int x, int z, Pixel newPixel, int tileRotation, bool modWeight = false)
        {
            if (obstacleLayer == null) return;

            int segmentIndex = newPixel.B;

            // NOTE: segmentIndex = 0 clears that segment from the data layer (eg digs a hole)
            //       so there is no footprint data to add to obstacle map
            if (segmentIndex == 0) return;

            string segmentName = GetSegmentName(this, segmentIndex);
            Entities.ModeledEntity segment = ReferencedSegments[segmentName];
            int[,] footprintData = new int[,] { { WEIGHT_MODULUS + 1 } }; // segment.Footprint.Data; // TODO: footprint data must be gained through 


            int footprintWidth = footprintData.GetLength(0);
            int footprintDepth = footprintData.GetLength(1);


            // INCREMENT the existing weight value on the obstacle map
            for (int i = 0; i < footprintWidth; i++)
                for (int j = 0; j < footprintDepth; j++)
                {
                    int weight = footprintData[i, j];
                    // when modWeight == true, we are modifying an upper layer
                    // and so the full weight is not applied, only a modulus thereof
                    if (modWeight)
                        weight %= WEIGHT_MODULUS;

                    // TODO: Scaling Potential? -  our terrain layout resolution may be 32x32 per zone and the
                    //      obstacle layer might be 128x128 (scale = x4 per axis) or 256x256 (x8 per axis)
                    int scaledX = x;
                    int scaledZ = z;
                    // NOTE: we always append weights to the obstacle map.
                    int existingValue = obstacleLayer.GetMapValue(scaledX + i, scaledZ + j);
                    int incrementedValue = existingValue + weight;
                    obstacleLayer.SetMapValue(scaledX + i, scaledZ + j, incrementedValue);
                }

        }

        private void UpdateTileStyle(MapLayer layoutLayer, MapLayer styleLayer, int x, int z, TileMap.Pixel newPixel, int tileRotation)
        {
            // set new style and set new footprint\obstacle data
            // NOTE: segmentIndex can be 0 if we've deleted this segment.
            if (newPixel.B > 0 && newPixel.G > 0) // B == segmentIndex, G == modelIndex (modelIndex 0 == null model so skip that one)
            {
                int floorLevel = layoutLayer.FloorLevel;

                // NOTE: we only need access to Structure here because we intend to Add a new Minimesh Element
                // TODO: using the layoutLayer.SubscriptX/Y/Z, can I grab this Structure from a multidimensional array?
                Structure currentStructure = FindStructure(layoutLayer.SubscriptX, layoutLayer.SubscriptY, layoutLayer.SubscriptZ);
                // when the level is added, any referenced segments must be cloned for each Level
                StructureLevel level = currentStructure.AddLevel(floorLevel);

                // TODO: we need to ensure SegmentSelector node's under our RootSelector node are sychronized by levelIndex used on mLevels[]
                int levelIndex = currentStructure.FindLevelIndex(floorLevel);

                MinimeshGeometry minimeshGeometry = GetMinimesh(currentStructure, levelIndex, newPixel.B, newPixel.G);

                uint minimeshElementIndex = CreateStyleElement(layoutLayer, x, z, minimeshGeometry, tileRotation);
                newPixel.High = (ushort)minimeshElementIndex;
            }

            styleLayer.SetMapValue(x, z, newPixel.ARGB);
        }

        private void ClearExistingStyle(MapLayer styleLayer, int x, int z)
        {
            int previousValue = styleLayer.GetMapValue(x, z);
            if (previousValue == -1) return; // there is no previous style set

            Pixel previousPixel = Pixel.GetPixel(previousValue);

            // the first 8 bits contains the segmentIndex thus 256 possible segments (including 1 empty)
            int segmentIndex = previousPixel.B;

            // the second 8 bits contains the modelIndex thus 256 possible tile-able models per segments
            int modelIndex = previousPixel.G;

            // the remaining 16 bits contains the minimeshElement index thus 65k minimesh elements max per MinimeshGeometry
            int minimeshElementIndex = previousPixel.High;

            int floorLevel = styleLayer.FloorLevel;

            // NOTE: we only need access to the Structure here because we intend to remove a Minimesh element.
            Structure currentStructure = FindStructure(styleLayer.SubscriptX, styleLayer.SubscriptY, styleLayer.SubscriptZ);

            // NOTE: the levelIndex for a particular floorLevel is not necessarily the same across all Zones/Structures because
            // they may have been added in a different order and / or may not contain same number of floors.  This is why
            // this call to FindLevelIndex() is dependant on the particular structure it's called on.
            int levelIndex = currentStructure.FindLevelIndex(floorLevel);

            MinimeshGeometry minimeshGeometry = GetMinimesh(currentStructure, levelIndex, segmentIndex, modelIndex);

            // note: this removes the element by disabling it and flagging it's index as free.
            // So we do not otherwise compact the minimesh though we should be able to
            // by truncating all interal arrays up to the last enabled minimesh element.			
            minimeshGeometry.RemoveElement((uint)minimeshElementIndex);
        }

        private uint CreateStyleElement(MapLayer layer, int x, int z, Elements.MinimeshGeometry minimeshGeometry, int rotationDegrees)
        {
            // compute minimesh element POSITION represented by pixel map layer pixel
            float zoneWidth = mWidth; //(float)parentRegion.BoundingBox.Width;
            float zoneDepth = mDepth; // (float)parentRegion.BoundingBox.Depth;
            float zoneLeft = -(zoneWidth / 2f); // zone left edge is -halfwidth
            float zoneBottom = -(zoneDepth / 2f); // zone bottom edge -halfDepth

            float tileSizeX = zoneWidth / (float)layer.TileCountX;
            float tileSizeZ = zoneDepth / (float)layer.TileCountZ;

            System.Diagnostics.Debug.Assert(tileSizeX == (float)layer.TileSize.x);
            System.Diagnostics.Debug.Assert(tileSizeZ == (float)layer.TileSize.z);

            float positionX = x * tileSizeX;
            float positionY = 0;
            float positionZ = z * tileSizeZ;

            float halfTileWidth = tileSizeX / 2f;
            float halfTileDepth = tileSizeZ / 2f;

            positionX += zoneLeft + halfTileWidth;
            positionZ += zoneBottom + halfTileDepth;

            // once we have the model, we can find it's geometry's dimensions 
            // so we can scale it's minimesh element representation to fit within a single tile
            float meshWidth = (float)minimeshGeometry.Mesh3d.BoundingBox.Width;
            float meshDepth = (float)minimeshGeometry.Mesh3d.BoundingBox.Depth;
            float scale = 1.001f; // tileSizeX / meshDepth; //meshWidth; 

            // NOTE: we don't use the model.Rotation anymore because we want the Pattern Rule to determine Rotation of a Tile
            // so that we only need one Model and thus one MinimeshGeometry instance to represent all rotations of a single geometry
            // file.
            Vector3d rotation = Vector3d.Zero(); // model.Rotation.GetEulerAngles(true);
            rotation.y = (float)rotationDegrees;
            // TODO: it's critical these elementIndices don't change for the duration of this tile's current style
            //       
            int elementColor = 0;
            int tag = (int)layer.FlattenIndex(x, z);
            uint minimeshElementIndex = minimeshGeometry.AddElement(positionX, positionY, positionZ,
                                                                     scale, scale, scale,
                                                                     (float)rotation.x, (float)rotation.y, (float)rotation.z,
                                                                     elementColor,
                                                                     tag);
            return minimeshElementIndex;
        }

        private static MinimeshGeometry GetMinimesh(Structure structure, int levelIndex, int segmentIndex, int modelIndex)
        {
            // find the appropriate Segment
            ModelSelector rootSegmentSelector = (ModelSelector)structure.Children[0];
            ModelSelector segmentSelector = (ModelSelector)rootSegmentSelector.Children[levelIndex];

            // select the appropriate model and grab the MinimeshGeometry reference 
            ModelSelector modelSelector = (ModelSelector)segmentSelector.Children[segmentIndex];
            Model model = modelSelector.Select((uint)modelIndex);
            MinimeshGeometry minimeshGeometry = (MinimeshGeometry)model.Geometry;

            return minimeshGeometry;
        }

        internal void UnflattenIndex(string layerName, int levelIndex, uint index, out int x, out int z)
        {
            int layerIndex = FindLayerIndex(levelIndex, layerName);
            mLevels[levelIndex].mMapLayers[layerIndex].UnflattenIndex(index, out x, out z);
        }


        /// <summary>
        /// Retrieves 2D list of tile indices defined by the rectangular bounds of startTileLocation.X/Z and endTileLocation.X/Z
        /// ignoring .Y values.
        /// </summary>
        /// <remarks>
        /// The startTileLocation and endTileLocation must exist within a single zone.  If you want to get list of tiles
        /// across multiple zones, you must find the seperate bounds for each zone and make multiple calls to GetTileList()
        /// </remarks>
        /// <param name="layerName"></param>
        /// <param name="startTileLocation"></param>
        /// <param name="endTileLocation"></param>
        /// <returns></returns>
        public uint[] GetTileList(string layerName, Vector3i startTileLocation, Vector3i endTileLocation)
        {
            int levelIndex = FindLevelIndex(startTileLocation.Y);
            int layerIndex = FindLayerIndex(levelIndex, layerName);

            uint startTileIndex = mLevels[levelIndex].mMapLayers[layerIndex].FlattenIndex(startTileLocation.X, startTileLocation.Z);
            uint endTileIndex = mLevels[levelIndex].mMapLayers[layerIndex].FlattenIndex(endTileLocation.X, endTileLocation.Z);

            return mLevels[levelIndex].mMapLayers[layerIndex].GetTileList(startTileIndex, endTileIndex);
        }

        #region IPageableTVNode Members
        public override void LoadTVResource()
        {
            System.Diagnostics.Debug.Assert(Parent != null);

            // following call to base.LoadTVResource() will load any domain object entity script before we start to load in the data layers
            base.LoadTVResource();

            ReferencedSegments = new System.Collections.Generic.Dictionary<string, Keystone.Entities.ModeledEntity>();

            // Create our Root Selector
            string id = "$" + this.ID + ':' + "root selector";
            ModelSelector rootSegmentSelector = (ModelSelector)Resource.Repository.Create(id, "ModelSelector");
            rootSegmentSelector.Enable = false;

            // entire branch is not serializble to scene database
            rootSegmentSelector.Serializable = false;
            this.AddChild(rootSegmentSelector);


            // load Entity Segments that we've deserialized 
            for (int k = 0; k < mSegmentLookupPaths.Length; k++)
            {
                LoadSegmentLookup(mSegmentLookupPaths[k]);
            }

            mDeserializationMode = true;

            LoadMapData();
            mDeserializationMode = false;
            rootSegmentSelector.Enable = true;

            // make sure this is written to disk as well
            // TODO: but this is something that should occur whenever we add layers.  Keep in mind 
            //		 tile painting (thus adding/removing layers) occurs via a worker command so that minimesh is
            //       updated in seperate thread during tool based sculpting
            PersistFloorLevels(mLevels);


            // when entire structure.LoadTVResource() is done, we want to have scene.mConnectivityGraph notified
            // so that we can generate the Graph for it.  How do we then generate portals from that?  How do we know for instance
            // when the last of multiple queues Structures have been loaded?  Or does it matter?  Do we always just make the Portals for
            // zones that are to the +x and +y and +z dimensions?  But if a Zone at x = 0, z = 0 comes in _after_ the Zone at x = 1, z = 0
            // then no portals will be generated between those two Zones.  I could perhaps set flags for each mGraph that will indicate
            // whether portals are dirty for any particular adjacent zone?

            string zoneName = this.Parent.ID;
            int subscriptX, subscriptY, subscriptZ;
            Keystone.Portals.ZoneRoot.GetZoneSubscriptsFromName(zoneName, out subscriptX, out subscriptY, out subscriptZ);

            // TODO: fix this notification for not just voxels but for isometric structure as well
           // if (mScene.mConnectivityGraph != null)
           //     mScene.mConnectivityGraph.NotifyOnStructureLoaded(this, subscriptX, subscriptZ, 32, 32);
        }


        // NOTE: Pager loads MapLayer's ahead of Structure.LoadTVResource() and thus ahead of this call 
        //       and so AutoTile will have adjacent MapLayer data available in order to compute models properly
        //       at Zone boundaries. TODO: however, i still need to verify that I'm always loading all of the 
        //       necessary MapLayers before the call to Structure.LoadTVResource() occurs
        private void LoadMapData()
        {
            // load the level data from the persist string
            LoadFloorLevels(mMapLevelsPersistString);

            if (mLevels == null) return;

            // Initialize() all layers first before Reading and loading\restoring the data.
            // This is because some layers during Read() will depend on adjacent layers being available for modification
            foreach (StructureLevel level in mLevels)
            {
                if (level.mMapLayers == null || level.mMapLayers.Length == 0) return;

                for (int i = 0; i < level.mMapLayers.Length; i++)
                {
                    string name = level.mMapLayers[i].Name;
                    string path = level.mMapLayers[i].Path;
                    System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false && string.IsNullOrEmpty(path) == false);

                    level.mMapLayers[i].Initialize();
                }
            }

            // Read _after_ all layers have first been opened above since some layers
            // do not get read directly, but are used by other layers during deserialization
            // Read() _only_ layouts.  Style and Obstacle maps will get computed and written during load of layout.
            foreach (StructureLevel level in mLevels)
            {
                level.mMapLayers[(int)LayerType.terrain].Read();
            }
        }

        // parse the floor levels stored in the xml properties
        private void LoadFloorLevels(string persistString)
        {
            char DELIMITER = ',';  // TODO: use global constant for this delim
            string[] csv = persistString.Split(DELIMITER);
            int index = 0;

            int numLevels = int.Parse(csv[index++]);

            if (numLevels > 0)
            {

                float floorWidth = float.Parse(csv[index++]);
                double floorHeight = double.Parse(csv[index++]);
                float floorDepth = float.Parse(csv[index++]);

                for (int i = 0; i < numLevels; i++)
                {
                    int floorLevel = int.Parse(csv[index++]);
                    int numLayers = int.Parse(csv[index++]);

                    for (int j = 0; j < numLayers; j++)
                    {
                        string layerName = csv[index++];
                        AddLayer(floorLevel, layerName);
                    }
                }
            }
        }

        private string PersistFloorLevels(StructureLevel[] levels)
        {
            string DELIM = ",";
            string persistString;

            int numLevels = 0;
            if (levels != null)
                numLevels = levels.Length;

            persistString = numLevels.ToString();

            if (numLevels > 0)
            {
                persistString += DELIM + mWidth.ToString();
                persistString += DELIM + FloorHeight.ToString();
                persistString += DELIM + mDepth.ToString();
            }

            for (int i = 0; i < numLevels; i++)
            {
                // floor level
                persistString += DELIM + levels[i].FloorLevel.ToString();

                // number of layers
                int numLayers = levels[i].mMapLayers.Length;

                persistString += DELIM + numLayers.ToString();

                // layer names 
                for (int j = 0; j < numLayers; j++)
                {
                    persistString += DELIM + levels[i].mMapLayers[j].Name;
                }
            }

            // write to disk
            string zoneName = this.Parent.ID;
            int subscriptX, subscriptY, subscriptZ;
            Keystone.Portals.ZoneRoot.GetZoneSubscriptsFromName(zoneName, out subscriptX, out subscriptY, out subscriptZ);
            string path = GetLayersDataPath(subscriptX, subscriptZ);
            System.IO.File.WriteAllText(path, persistString);

            return persistString;
        }

        public void AddLookupSegment(string segmentPath)
        {
            if (string.IsNullOrEmpty(segmentPath)) return;

            // skip if this is already loaded
            if (ReferencedSegments.ContainsKey(segmentPath.ToLower()) == true) return;

            Entities.ModeledEntity segment = LoadSegmentLookup(segmentPath);

            // NOTE: we only add to mSegmentLookupPath's inside this public method where our Tool
            // is trying to place a new segment.        	
            System.Diagnostics.Debug.Assert(mSegmentLookupPaths.ArrayContains(segmentPath) == false);
            mSegmentLookupPaths.ArrayAppend(segmentPath);

            // for each level already instantiated, this _NEW_ segment must be added under
            // every SegmentSelector for every Level!
            if (mLevels != null)
            {
                // TODO: Dec.2.2014 - below has never been tested yet since we only have terrain segment and havent tried
                //       adding in a new segment like castle wall _after_ we've already loaded a few levels 
                ModelSelector rootLevelSelector = (ModelSelector)this.Children[0];

                for (int i = 0; i < mLevels.Length; i++)
                {
                    // gain reference to the segmentSelector for each floor level
                    ModelSelector segmentSelector = (ModelSelector)rootLevelSelector.Children[i];

                    // Clone first child of the segment which is the ModelSelector node
                    ModelSelector modelSelector = (ModelSelector)segment.Children[0];
                    string id = Keystone.Resource.Repository.GetNewName(typeof(ModelSelector));
                    ModelSelector clonedModelSelector = (ModelSelector)modelSelector.Clone(id, true, false, false);

                    
                    // the referenced segments has .Mesh3d Geometry nodes and we need to replace all of those with MinimeshGeometry			
                    // for every model that makes up the Segment, ensure a MinimeshGeometry has been created	
                    mLevels[i].CreateMinimeshRepresentations(clonedModelSelector, initialMinimeshCapacity, FloorHeight);

                    // every segmentSelector for every Level within this Structure, _must_ have each cloned Segment
                    // in the same index location within their respective segmentSelector's.Children[] right?
                    segmentSelector.AddChild(clonedModelSelector);
                }
            }
        }


        private Keystone.Entities.ModeledEntity LoadSegmentLookup(string segmentPath)
        {
            // skip if this is already loaded
            if (ReferencedSegments.ContainsKey(segmentPath.ToLower()) == true) return ReferencedSegments[segmentPath.ToLower()];

            string archiveFullPath = Keystone.Core.FullNodePath(segmentPath);
            
            bool clone = true;
            bool recurse = true;
            bool delay = false;
            // this ModeledEntity segment will typically have as it's first child a ModelSequence node.
            Keystone.Entities.ModeledEntity segment = Keystone.ImportLib.Load(archiveFullPath, clone, recurse, delay, null) as Entities.ModeledEntity;

            if (segment.Translation != Vector3d.Zero())
            {
                // NOTE: Very important these segments are centered.  When I save these prefabs, i should be enforcing 0,0,0 origin too.
                System.Diagnostics.Debug.WriteLine("Structure.LoadSegmentLookups() - WARNING: Segment '" + segmentPath + "' translation is not 0,0,0.");
                segment.Translation = Vector3d.Zero();
            }

            // HACK - set segment flag on Entity since we dont have GUI plugin option to modify that flag yet
            segment.SetEntityFlagValue("terrain", true);

            ReferencedSegments.Add(segmentPath.ToLower(), segment);
            Resource.Repository.IncrementRef(segment); // todo: during dispose, we need to decrement ref this lookup segment

            return segment;
        }

        // NOTE: this is the only place where Save's to MapLayers occurs and it gets called
        // from FormMain.Commands.CommandCompleted() after tile painting operation is completed.
        public override void SaveTVResource(string filepath)
        {
            if (mLevels == null || mLevels.Length == 0) return;

            for (int i = 0; i < mLevels.Length; i++)
                if (mLevels[i].mMapLayers != null)
                    for (int j = 0; j < mLevels[i].mMapLayers.Length; j++)
                        mLevels[i].mMapLayers[j].Write();
        }

        #endregion


        /// <summary>
        /// ModelSpace Ray Collision of Structure.
        /// Seperate call to this method must be made for each level we wish to test with
        /// levelIndex set in parameters.LevelIndex
        /// That is currently done by Picker.cs.DoStructureTest()
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="regionMatrix"></param>
        /// <param name="parameters">parameters.LevelIndex specifies which level we will test against.</param>
        /// <returns></returns>
        public override Keystone.Collision.PickResults Collide(Keystone.Types.Vector3d start, Keystone.Types.Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // NOTE: see Picker.cs.DoStructureTest()
            // NOTE: see Picker.cs.DoCelledRegionInteriorTest()
            PickResults pickResult = new PickResults();
            pickResult.HasCollided = false;

            if (this.PageStatus != Keystone.IO.PageableNodeStatus.Loaded) return pickResult;

            int floorLevel = parameters.FloorLevel;
            int levelIndex = FindLevelIndex(floorLevel);

            if (levelIndex < 0)
                throw new ArgumentOutOfRangeException("Structure.Collide() - parameter.FloorLevel '" + parameters.FloorLevel.ToString() + "' out of range.");

            // does the pick parameters specify we should search floors?
            if ((parameters.Accuracy & PickAccuracy.Tile) == PickAccuracy.Tile || (parameters.Accuracy & PickAccuracy.Face) == PickAccuracy.Face)
            {
                Vector3d dir = Vector3d.Normalize(end - start);
                Ray r = new Ray(start, dir);

                // NOTE: we pick as if the height of the floor is the TOP of the floor, not the bottom. 
                // So even LevelIndex 0 with a FloorLevel of -1, we'll treat height as FloorLevel + 1 so that
                // we are picking the top since visually that is what the users mouse will be doing.  
                double floorSizeY = FloorHeight;

                double floorHeight = (floorLevel + 1) * floorSizeY;

                // NOTE: translate the ray origin to be in model space with respect to the floor height.
                //       this is necessary because although the ray is passed in
                //       in modelspace for the overall structure, it's not modelspace with respect to each individual level. So we will
                //       move the origin by -floorHeight for that level.
                r.Origin -= new Vector3d(0, floorHeight, 0);

                // NOTE: the hard coded mLevels[0] index WOULD BE ok _IF_ we are only interested in doing a 2D pick against the layer type of specified name
                //       and we already know that all layer types of a particular name have exact same resolution regardless of level and thus would return
                //       the exact same picked tile ID/tile location.  
                //       HOWEVER, we also need to test that layer to see if it's occupied by any type other than TileType.TT_EMPTY.  Thus we must
                //       use mLevels[levelIndex]

                // TODO: that said, we've hardcoded layer name "layout" when it should be passed in because sometimes we might 
                // want to pick structure's other than terrain such as housing interiors which may have a finer resolution than outside terrain.       
                int layerIndex = FindLayerIndex(levelIndex, "layout");
                pickResult = mLevels[levelIndex].mMapLayers[layerIndex].Collide(r, parameters);
                pickResult.ImpactPointLocalSpace.y += floorHeight;
            }

            // NOTE: pickResult.TileLocation.Y will contain the paramters.LevelIndex
            return pickResult;
        }

        public Vector3i TileLocationFromPoint (Vector3d point)
        {
            return new Vector3i(); // TODO: 
        }

        #region IDisposable
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            // structure was added internally so much be disposed here
            if (ReferencedSegments != null)
                foreach (string segmentKey in ReferencedSegments.Keys)
                {
                    Resource.Repository.DecrementRef(ReferencedSegments[segmentKey]);
                    ReferencedSegments.Remove(segmentKey);

                }
        }
        #endregion
    }
}
