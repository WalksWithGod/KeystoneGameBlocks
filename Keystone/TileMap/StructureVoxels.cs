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
    /// things like FloorHeight and Level of a structure that the Entity (such as NPC) is at.
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
    public class StructureVoxels : Keystone.Entities.ModeledEntity
    {
        // these three vars (MINIMUM_FLOOR_LEVEL, MAXIMUM_FLOOR_LEVEL, FloorHeight) are configurable via SetProperties()
        private int MINIMUM_FLOOR_LEVEL = -63;
        private int MAXIMUM_FLOOR_LEVEL = 63;
        public double FloorHeight = 3.0d; // NOTE: keep as double or precision can screw up and our meshes have seams // 3.0 (halfsize = 1.5 )
                                          // dimension wise, don't think Minecraft, think more Legend of Grimrock.  In other words, rectangular and not square.
                                          // pathfinding weight modulus... 
        private const int PATH_WEIGHT_MODULUS = 63; // 0 - 63 is WALKABLE, 64+ is NON WALKABLE
        private uint mInitialMinimeshCapacity = 1024 * 64;  // i believe this must always be at least 1 or higher but i think we'll always use multiple of 2

        // TODO: use Vector3f Size; var instead of 3 seperate references
        internal float mWidth { get; set; }  // size along X axis (width) in world units of structure
        internal float mDepth { get; set; }   // size along Z axis in world units of this structure
        internal float mHeight { get; set; }   // size along Y axis in world units of this structure

        // NOTE: mSegmentLookupPaths and mStructureMinimeshes can be shared between all levels so are not
        // a member of that struct as mMapLayerNames, mMapLayerPaths and mMapLayers
        // NOTE2: since mModelLookupPaths are serialized by Structure.cs we can control the order in which
        // these referenced entities/segments are loaded... particularly that they can be loaded before 
        // we try to read and restore MapLayer data from bitmap storage.
        private string[] mSegmentLookupPaths;

        // collection of prefab resource descriptors
        private System.Collections.Generic.Dictionary<string, Keystone.Entities.ModeledEntity> ReferencedSegments;

        private int mMaxNumLevels;
        internal VoxelStructureLevel[] mLevels;
        internal Octree.StaticOctree<TileInfo> mOctree;


        internal StructureVoxels(string id) : base(id)
        {
            SetEntityAttributesValue((int)KeyCommon.Flags.EntityAttributes.Structure, true);
            mOctree = new Keystone.Octree.StaticOctree<TileInfo>();
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[7 + tmp.Length];
            tmp.CopyTo(properties, 7);


            properties[0] = new Settings.PropertySpec("modellookuppaths", typeof(string[]).Name);
            properties[1] = new Settings.PropertySpec("maplevels", typeof(string).Name);
            properties[2] = new Settings.PropertySpec("minfloor", typeof(int).Name);
            properties[3] = new Settings.PropertySpec("maxfloor", typeof(int).Name);
            properties[4] = new Settings.PropertySpec("floorheight", typeof(double).Name);
            properties[5] = new Settings.PropertySpec("maxfloorcount", typeof(int).Name);
            properties[6] = new Settings.PropertySpec("octreedepth", typeof(uint).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mSegmentLookupPaths;
                properties[1].DefaultValue = mMapLevelsPersistString;
                properties[2].DefaultValue = MINIMUM_FLOOR_LEVEL;
                properties[3].DefaultValue = MAXIMUM_FLOOR_LEVEL;
                properties[4].DefaultValue = FloorHeight;
                properties[5].DefaultValue = mMaxNumLevels;
                properties[6].DefaultValue = mOctree.MaxDepth;
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
                    case "maxfloorcount":
                        mMaxNumLevels = (int)properties[i].DefaultValue;
                        break;
                    case "minfloor":
                        MINIMUM_FLOOR_LEVEL = (int)properties[i].DefaultValue;
                        break;
                    case "maxfloor":
                        MAXIMUM_FLOOR_LEVEL = (int)properties[i].DefaultValue;
                        break;
                    case "floorheight":
                        FloorHeight = (double)properties[i].DefaultValue;
                        break;
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
                    case "octreedepth":
                        // Depth 1 = 2 octants along each axis (2^1)
                        // Depth 2 = 4 octants along each axis (2^2)
                        // Depth 3 = 8 octants along each axis (2^3)
                        // Depth 4 = 16 octants along each axis (2^4)
                        // Depth 5 = 32 octants along each axis (2^5)
                        // Depth 6 = 64 octants along each axis (2^6)
                        // NOTE: MaxDepth for StaticOctree _MUST_ be set such that they fully encompass #tiles on each axis 
                        // So for instance, for 32x32x32 tile Zones, the Depth must be set to 5.  That's 2^5 = 32.
                        mOctree.MaxDepth = (uint)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

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

            // first child is a dummy ModelSelector representing SegmentIndex == 0 (tileType == EMPTY)
            id = Resource.Repository.GetNewName(typeof(ModelSelector));
            ModelSelector emptyModelSelector = (ModelSelector)Resource.Repository.Create(id, "ModelSelector");
            rootSegmentSelector.AddChild(emptyModelSelector);
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


            // generate Level of Detail models
            // - to use smallest number of hull points, we should take advantage of the post-auto-tiled
            //   data...


            // after map data is loaded, update page status for minimeshes.
            // NOTE: i don't understand why this should help stop loading crashes
            // but it does.  But in theory, these minimeshes shouldn't be rendering anyway
            // since they aren't yet ultimately connnectd to the scene
            for (int k = 0; k < mStructureMinimeshes.Length; k++)
            {
                mStructureMinimeshes[k].PageStatus = IO.PageableNodeStatus.Loaded;
            }

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

            if (mScene.mConnectivityGraph != null)
                mScene.mConnectivityGraph.NotifyOnStructureLoaded(this, subscriptX, subscriptZ, 32, 32); // TODO: hardcoded 32, 32 no good!

            System.Diagnostics.Debug.WriteLine("TileMap.Structure.LoadTVResource() - LOADING " + this.ID + " COMPLETED.");
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
            foreach (VoxelStructureLevel level in mLevels)
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
            foreach (VoxelStructureLevel level in mLevels)
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


        private string PersistFloorLevels(VoxelStructureLevel[] levels)
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
            if (ReferencedSegments.ContainsKey(segmentPath.ToLower())) return;

            // NOTE: we only add to mSegmentLookupPath's inside this public method where our Tool
            // is trying to place a new segment.        	
            System.Diagnostics.Debug.Assert(mSegmentLookupPaths.ArrayContains(segmentPath) == false);
            mSegmentLookupPaths = mSegmentLookupPaths.ArrayAppend(segmentPath);

            LoadSegmentLookup(segmentPath);

            for (int k = 0; k < mStructureMinimeshes.Length; k++)
            {
                mStructureMinimeshes[k].PageStatus = IO.PageableNodeStatus.Loaded;
            }
        }

        private Keystone.Entities.ModeledEntity LoadSegmentLookup(string segmentPath)
        {
            // skip if this is already loaded
            if (ReferencedSegments.ContainsKey(segmentPath.ToLower()) == true) return ReferencedSegments[segmentPath.ToLower()];

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(segmentPath.ToLower());
            string archiveFullPath = Keystone.Core.FullNodePath(""); // TODO: hack hardcoding empty string here

            System.IO.Stream stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(descriptor.EntryName, null, archiveFullPath);
            Keystone.IO.XMLDatabaseCompressed xmldb = new Keystone.IO.XMLDatabaseCompressed();
            Keystone.Scene.SceneInfo info = xmldb.Open(stream);

            bool clone = true;
            bool recurse = true;
            string[] clonedEntityIDs = null; // TODO: these may need to be assigned by server in the COmmand
            // this ModeledEntity segment will typically have as it's first child a ModelSequence node.
            Keystone.Entities.ModeledEntity segment = (Keystone.Entities.ModeledEntity)xmldb.ReadSynchronous(info.FirstNodeTypeName, null, recurse, clone, clonedEntityIDs, false, false);
            stream.Dispose();
            xmldb.Dispose();

            if (segment.Translation != Vector3d.Zero())
            {
                // NOTE: Very important these segments are centered.  When I save these prefabs
                //System.Diagnostics.Debug.WriteLine ("Structure.LoadSegmentLookup() - WARNING: Segment '" + segmentPath + "' translation is not 0,0,0.");
                segment.Translation = Vector3d.Zero();
            }

            ReferencedSegments.Add(segmentPath.ToLower(), segment);
            Resource.Repository.IncrementRef(segment);


            // March.26.2105 - segment visuals are now per structure, not per structureLevel.  The memory reqts for per
            // structureLevel were too great and increased load times dramatically.  So the first this.Children[0] is
            // no longer a rootLevelSelector, it is a segmentSelector that will select between the different segment packs
            // such as dirt terrain segment, castle wall segment, etc.
            ModelSelector segmentSelector = (ModelSelector)this.Children[0];

            // Clone first child of the segment which is the ModelSelector node
            ModelSelector modelSelector = (ModelSelector)segment.Children[0];
            string id = Keystone.Resource.Repository.GetNewName(typeof(ModelSelector));
            ModelSelector clonedModelSelector = (ModelSelector)modelSelector.Clone(id, true, false, false);

            // the ReferencedSegments has .Mesh3d Geometry nodes and we need to replace all of those with MinimeshGeometry			
            // for every model that makes up the Segment, ensure a MinimeshGeometry has been created	
            InstancedGeometry[] minimeshes = this.CreateInstancedGeometryRepresentations(clonedModelSelector, mInitialMinimeshCapacity, FloorHeight);
            mStructureMinimeshes = mStructureMinimeshes.ArrayAppendRange(minimeshes);

            // ordering of every modelSelector under our segmentSelector must match the ReferencedSegments ordering
            // but recall that at index == 0 represents EMPTY segment index and so contains a ModelSelector with no children.
            segmentSelector.AddChild(clonedModelSelector);

            return segment;
        }

        // array of minimeshes is only used for being able to assign pagestatus = IO.PageableNodeStatus.Loaded  
        // 
        InstancedGeometry[] mStructureMinimeshes;

        public InstancedGeometry[] CreateInstancedGeometryRepresentations(ModelSelector segmentModelSelector, uint initialMinimeshCapacity, double floorHeight)
        {
            // Length - 1 because first Model of every Segment's model selector is null model since often times
            // a segment that is hidden by other geometry need not be rendered and so null variant is used.
            InstancedGeometry[] instancedGeometries = new InstancedGeometry[segmentModelSelector.Children.Length - 1];

            // recall any segment can have multiple visual styles or auto-tile versions we must loop through
            // NOTE: again, loop starts at 1 since index 0 is null model.
            for (uint i = 1; i < segmentModelSelector.Children.Length; i++)
            {
                Model model = segmentModelSelector.Select(i);

                // when each StructureLevel had seperate minimeshes, we computed diferent heights based on level.  
                // but since we switched (permanently? not sure yet) to one set of minimeshes used by all 
                // StructureLevels within a structure we use height = 0.0
                double height = 0.0; // this.FloorHeight * floorHeight;

                instancedGeometries[i - 1] = Celestial.ProceduralHelper.CreateInstancedGeometryRepresentations(initialMinimeshCapacity, model, (float)height);


                // TEMP HACK - ASSIGN INSTANCE SHADER
                string shaderPath = @"caesar\shaders\PSSM\pssm.fx";
                // TEMP HACK - Load + Assign Texture
                string texturePath = null; // @"E:\dev\c#\KeystoneGameBlocks\Data\pool\shaders\GrassTop.dds";
                if (model.Appearance.Layers != null)
                {
                    texturePath = model.Appearance.Layers[0].Texture.ID;
                }
                Appearance.Appearance appearance = Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, null, null, null);
                appearance.AddDefine("TERRAIN_ATLAS_TEXTURING", null);

                appearance.AddChild(model.Appearance.Material);
                model.RemoveChild(model.Appearance);
                // NOTE: here we assign new Appearance _AFTER_ we've created the InstancedGeometry above and replaced the Mesh3d with the InstancedGeometry
                // otherwise Model.UpdateShaderDefines() will set incorrect GEOMETRY_***** type.
                model.AddChild(appearance);

                // NOTE: for now we do not recycle these minimeshes so we DO NOT NEED to artificially increment them.
                //       Each minimesh will then reach refCount == 0 when their respective Zone & Structure are paged out.
                //       In the future however, I might keep the minimeshes in a pool so that we don't ever have to unload/load new instances.
                // Resource.Repository.IncrementRef (minimeshes[i - 1]);
            }

            return instancedGeometries;
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

            string filename = string.Format("{0}{1}{2}.layerdata", locationX, LAYERPATH_DELIMIETER,
                                                                          locationZ);
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

            mOctree.BoundingBox = r.BoundingBox;
        }

        public override void AddChild(Keystone.Entities.Entity child)
        {
            // TODO: but what about static items like rocks or trees?
            //       and what about the "physical" updates of those things when terrain is destroyed at run-time or when
            //       river paths need to change along with changes in structure?
            //       even NPCs may be destroyed for instance on a cave-in.  Do we just use events
            //       to handle movement of those entities that are affected by changes in structure?
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

                // bounds testing - determine if we've reached max or min allowed levels to build structure/terrain on
                // NOTE: we allow creation of level to max level, but further below we do not allow structure/terrain
                // to be placed on the maximum level
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


            VoxelStructureLevel level = mLevels[levelIndex];

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
                    // IMPORTANT: Feb.9.2016 - we do not  allow structure/terrain to be placed in top most level. Top
                    //            most is reserved for entities to walk on or be placed. Pathing
                    //            also will break if we try to path on topmost level terrain structure.
                    if (floorLevel >= MAXIMUM_FLOOR_LEVEL - 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Structure.Layer_SetValue() - Maximum terrain/structure build height reached.");
                        return;
                    }

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
                VoxelStructureLevel level = mLevels[levelIndex];

                int segmentIndex = pixel.B;

                // TODO: can we always assume that the style layer for terrain or structure is always layer.Index + 1
                int layerIndex = FindLayerIndex(levelIndex, layer.Name);

                // grab a reference to the STYLE layer for this LAYOUT layer so that we can notify it
                MapLayer styleLayer = level.mMapLayers[layerIndex + 1]; // style is always next index from "layout"

                // segment.Execute ("Apply", new object[] { this.ID, layer, styleLayer, x, z, segmentIndex, mAutoTileAdjacents});
                // NOTE: "AutoTile" is recursive so we can't just return a value here... it must respond to changed values of adjacents
                //segment.Execute("AutoTile", new object[] { layer, styleLayer, x, z, segmentIndex, mAutoTileAdjacents});

                // the segment's sub-model (visual style) is determiend from pattern rules.  
                // TODO: These rules may be loaded from scipt but currently are hardcoded
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
            // TODO: this zone is null when deserializing structure... 
            Keystone.Portals.Zone zone = (Keystone.Portals.Zone)this.Parent;
            Keystone.Portals.ZoneRoot zoneRoot = (Keystone.Portals.ZoneRoot)zone.Parent;

            System.Diagnostics.Debug.Assert(
               zone.ArraySubscript[1] == 0, "zone.ArraySubscript[1] must always be 0 because the Y Axis of our universe must never be > 1 Zone high when using Structures inside Zones.");

            int offsetX = zone.ArraySubscript[0];
            // NOTE: offsetY must take into account the relative floorLevel of this, as well as the gorund floor index of our entire game world
            int offsetY = zone.ArraySubscript[1] + (int)zoneRoot.StructureGroundFloorIndex + floorLevel;
            int offsetZ = zone.ArraySubscript[2];

            VoxelStructureLevel level = this.InsertLevel(offsetX, offsetY, offsetZ, floorLevel);

            MapLayer layer;

            if (mDeserializationMode == false)
                layer = Core._Core.MapGrid.Create(layerName, offsetX, offsetY, offsetZ, floorLevel, mWidth, (float)FloorHeight, mDepth);

            // always bind events whether deserialization mode or not
            layer = Core._Core.MapGrid.Bind(layerName, offsetX, offsetY, offsetZ, MapLayer_OnValueChanged, MapLayer_OnValidate);

            // because level is a struct, we modify at array because otherwise we're modifying a value type
            // which wont affect the element in array
            level.AddMapLayer(layer);
        }

        /// <summary>
        /// Insert level.  Can insert ahead of first level but lowest array index is always 0 (obviously).
        /// NOTE: cannot insert arbitrarily between first and last elements.  It must be append or insert before first.
        /// </summary>
        /// <param name="locationX"></param>
        /// <param name="locationY"></param>
        /// <param name="locationZ"></param>
        /// <param name="floorLevel"></param>
        /// <returns></returns>
        private VoxelStructureLevel InsertLevel(int locationX, int locationY, int locationZ, int floorLevel)
        {
            VoxelStructureLevel level;
            int levelIndex = this.FindLevelIndex(floorLevel);
            if (levelIndex == -1)
            {
                // create a new level - insert below, or append above depending on floorLevel
                // NOTE: We do NOT support inserting BETWEEN floorLevels!
                level = new VoxelStructureLevel(locationX, locationY, locationZ, floorLevel);

                if (mLevels == null)
                {
                    levelIndex = 0;
                    mLevels = mLevels.ArrayAppend(level);
                }
                else if (floorLevel > mLevels[mLevels.Length - 1].FloorLevel)
                {
                    // this floorLevel is > current top floor level
                    levelIndex = mLevels.Length;
                    mLevels = mLevels.ArrayAppend(level);
                }
                else
                {
                    // this floorLevel is lower than lowest existing 
                    levelIndex = 0;
                    mLevels = mLevels.ArrayInsertAt(level, (uint)levelIndex);
                }


                //System.Diagnostics.Debug.Assert (levelIndex == rootLevelSelector.ChildCount - 1, "StructureLevel indices and rootLevelSelector's children should have sychronized indices");

            }
            else level = mLevels[levelIndex];

            return level;
        }

        private string GetSegmentName(StructureVoxels structure, int segmentIndex)
        {
            // find the appropriate Segment

            // TODO: Verify this works and that all segmentIndex values in all Levels within the same Structure are always
            // in the same order no matter when new Segment's are added to Lookup  and no matter when new Levels are added to Structure.
            string name = mSegmentLookupPaths[segmentIndex - 1]; // subtract 1 because segmentIndex 0 represents null/empty tile segment.
            return name;
        }

        private StructureVoxels FindStructure(int subscriptX, int subscriptY, int subscriptZ)
        {
            // we could perhaps deduce the name and then just find the structure from 
            // Repository.  

            // we force subscript 0 since we don't want MapLayer's subscript which is actually 0-64 or 0-128 even)
            string structureID = GetStructureID(subscriptX, 0, subscriptZ);
            StructureVoxels result = (StructureVoxels)Keystone.Resource.Repository.Get(structureID);
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

        private int FindLevelIndex(int floorLevel)
        {
            if (mLevels == null) return -1;

            for (int j = 0; j < mLevels.Length; j++)
                if (mLevels[j].FloorLevel == floorLevel)
                    return j;

            return -1;
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

        private int FindLayerIndex(int levelIndex, string layerName)
        {
            if (mLevels == null) throw new ArgumentOutOfRangeException("Level does not exist."); // return -1;

            VoxelStructureLevel level = mLevels[levelIndex];

            if (level.mMapLayers == null) throw new ArgumentOutOfRangeException("Layer does not exist.");

            for (int i = 0; i < level.mMapLayers.Length; i++)
                if (level.mMapLayers[i].Name == layerName)
                    return i;

            return -1;
        }

        public Vector3i TileLocationFromPoint(Vector3d point)
        {
            Vector3i result;
            // find the Y index for floor containing point.Y
            result.X = result.Y = result.Z = 0;


            result = mLevels[result.Y].mMapLayers[(int)TileMap.LayerType.obstacles].TileLocationFromPoint(point);

            return result;
        }

        #region Pattern
        Pattern[] mPatterns;

        private Pattern[] CreatePatterns()
        {
            // TODO: being able to parse a pattern from a string would be nice

            // create maps for this rule for creating a dirt wall
            // TODO: model 0 = null model - has no appearance or geometry
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
            r.TileRotation = 270;
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
            r.TileRotation = 0;
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

            // if adjacent to just a East wall, use W END connector model
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
            // and a NULL Model type is _not_ the same as a NULL Segment type.  A NULL Segment means no segment, but a null model
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
                int[] atlasTextureRowColumn = selectedRule.AtlasTextureRowColumn;

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

                UpdateTileStyle(layoutLayer, styleLayer, x, z, newPixel, tileRotation, atlasTextureRowColumn);

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

                        // recurse
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

            string previousSegmentName = GetSegmentName(this, previousSegmentIndex);
            Entities.ModeledEntity previousSegment = ReferencedSegments[previousSegmentName];
            int[,] footprintData = new int[,] { { PATH_WEIGHT_MODULUS + 1 } }; // previousSegment.Footprint.Data;

            int footprintWidth = footprintData.GetLength(0);
            int footprintDepth = footprintData.GetLength(1);

            for (int i = 0; i < footprintWidth; i++)
                for (int j = 0; j < footprintDepth; j++)
                {
                    int weight = footprintData[i, j];
                    // when modWeight == true, we are modifying an upper layer
                    // and so the full weight is not applied, only a modulus thereof
                    if (modWeight)
                        weight %= PATH_WEIGHT_MODULUS;

                    int scaledX = x;
                    int scaledZ = z;
                    int existingValue = obstacleLayer.GetMapValue(scaledX + i, scaledZ + j);
                    int decrementedValue = existingValue - weight;
                    obstacleLayer.SetMapValue(scaledX + i, scaledZ + j, decrementedValue);
                }
        }

        private void UpdateObstacleMap(MapLayer obstacleLayer, int x, int z, Pixel newPixel, int tileRotation, bool modWeight = false)
        {
            if (obstacleLayer == null) return;

            int segmentIndex = newPixel.B;

            // NOTE: segmentIndex = 0 clears that segment from the data layer (eg digs a hole)
            //       so there is no footprint data to add to obstacle map
            if (segmentIndex == 0) return;

            string segmentName = GetSegmentName(this, segmentIndex);
            Entities.ModeledEntity segment = ReferencedSegments[segmentName];
            int[,] footprintData = new int[,] { { PATH_WEIGHT_MODULUS + 1 } }; // segment.Footprint.Data; // TODO: footprint data must be gained through 


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
                        weight %= PATH_WEIGHT_MODULUS;

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

        private Vector3d GetTilePosition(MapLayer layer, int x, int y, int z)
        {
            // compute minimesh element POSITION represented by pixel map layer pixel
            double zoneWidth = mWidth; //(double)parentRegion.BoundingBox.Width;
            double zoneDepth = mDepth; // (double)parentRegion.BoundingBox.Depth;
            double zoneHeight = mHeight;

            double zoneLeft = -(zoneWidth / 2d); // zone left edge is -halfwidth
            double zoneBack = -(zoneDepth / 2d); // zone bottom edge -halfDepth
            double zoneBottom = -(zoneHeight / 2d);

            double tileSizeX = zoneWidth / (double)layer.TileCountX;
            double tileSizeY = layer.TileSize.y; // zoneHeight / mMaxNumLevels;
            double tileSizeZ = zoneDepth / (double)layer.TileCountZ;

            System.Diagnostics.Debug.Assert(tileSizeX == (double)layer.TileSize.x);
            System.Diagnostics.Debug.Assert(tileSizeZ == (double)layer.TileSize.z);

            Vector3d position;
            position.x = x * tileSizeX;
            position.y = y * tileSizeY;
            position.z = z * tileSizeZ;

            double halfTileWidth = tileSizeX / 2d;
            double halfTileDepth = tileSizeZ / 2d;
            double halfTileHeight = tileSizeY / 2d;

            // TODO: this calc seems to work for even numbered tiles (eg. 32 x 32 x 32) per zone, and I think 
            //       odd numbers can't be allowed because they wont fit into Octree leaf nodes perfectly.
            position.x += zoneLeft + halfTileWidth;
            // NOTE: unlike x and z which are 0 based indices into array of MapLayer, y arg passed in is actual 'y' index level so eg -15 to 16 for 32 resolution map layer
            position.y += halfTileHeight; // zoneBottom + halfTileHeight;
            position.z += zoneBack + halfTileDepth;

            return position;
        }

        private uint CreateStyleElement(Elements.MinimeshGeometry minimeshGeometry, Vector3d position, int rotationDegrees, int tileID)
        {
            // once we have the model, we can find it's geometry's dimensions 
            // so we can scale it's minimesh element representation to fit within a single tile
            // TODO: actually we should never have to do that.  We should ensure the meshes are made to exact size of tile.
            // I think sometimes we try to be too flexible with data inputs... data inputs as a rule should be of a certain specification
            // because enabling flexibility for variable inputs is just asking for trouble
            //float meshWidth = (float)minimeshGeometry.Mesh3d.BoundingBox.Width;
            //float meshDepth = (float)minimeshGeometry.Mesh3d.BoundingBox.Depth;
            float scale = 1.001f; // .001 increase in scale seems to be only way to get rid of seams between tiles even when they are perfectly sized. // tileSizeX / meshDepth; //meshWidth; 
            Vector3d scaleVec;
            scaleVec.x = scaleVec.y = scaleVec.z = scale;

            // NOTE: we don't use the model.Rotation because we want the Pattern Rule to determine Rotation of a Tile
            // so that we only need one Model and thus one MinimeshGeometry instance to represent all rotations of a single geometry
            // file.
            Vector3d rotation = Vector3d.Zero();
            rotation.y = (double)rotationDegrees;
            // TODO: it's critical these elementIndices don't change for the duration of this tile's current style
            //       TODO: April.10.2015 - this may change if we go with immediate rendering using octree visibility of each tile
            //        rather than retained minimesh elements.
            int elementColor = 0;
            int tag = tileID;
            uint minimeshElementIndex = minimeshGeometry.AddElement(position, scaleVec, rotation,
                                                                     elementColor, tag);
            return minimeshElementIndex;
        }


        // TODO: unfortunately, a lot of this visual appearance stuff dealing with minimeshes is just tied directly into Structure rather than
        //       into some kind of "Model" to help seperate entity logic from visual display logic (i.e. MVC).  
        //       I think that the Octree for the visual model should for now exist here also and contain a TileInfo for each
        //       segment that has a visible model.  If the model is a null model or the segment is null, then the entry is removed from the octree.
        //       - in this way, we allow for AutoTile to help us to minimize the number of leaf nodes in our octree by not adding null segment/models
        //       to the octree.
        private void UpdateTileStyle(MapLayer layoutLayer, MapLayer styleLayer, int x, int z, TileMap.Pixel newPixel, int rotationDegrees, int[] atlasTextureRowColumn)
        {
            // set new style and set new footprint\obstacle data
            // NOTE: segmentIndex can be 0 if we've deleted this segment.
            if (newPixel.B > 0 && newPixel.G > 0) // B == segmentIndex, G == modelIndex (modelIndex 0 == null model so skip that one)
            {
                int floorLevel = layoutLayer.FloorLevel;

                // NOTE: we only need access to the Structure here because we intend to Add a Minimesh element and because
                // sometimes the structure we want is on an adjacent Zone from 'this' which we've reached through AutoTile recursion of adjacents
                // TODO: using the layoutLayer.SubscriptX/Y/Z, can I grab this Structure from a multidimensional array?
                StructureVoxels currentStructure = FindStructure(layoutLayer.SubscriptX, layoutLayer.SubscriptY, layoutLayer.SubscriptZ);
                // when the level is added, any referenced segments must be cloned for each Level
                VoxelStructureLevel level = currentStructure.InsertLevel(x, layoutLayer.SubscriptY, z, floorLevel);

                // TODO: we need to ensure SegmentSelector node's under our RootSelector node are sychronized by levelIndex used on mLevels[]
                int levelIndex = currentStructure.FindLevelIndex(floorLevel);


                // update graphical	            
                Vector3d position = GetTilePosition(layoutLayer, x, floorLevel, z);
                int tileID = (int)layoutLayer.FlattenIndex(x, z);
                //	            MinimeshGeometry minimeshGeometry = GetMinimesh(currentStructure, newPixel.B, newPixel.G);
                //	            uint minimeshElementIndex = CreateStyleElement (minimeshGeometry, position, rotationDegrees, tileID);
                newPixel.High = 1; // (ushort)minimeshElementIndex;

                // update spatial
                TileInfo data;
                data.ID = tileID;
                data.SegmentID = newPixel.B;
                data.ModelID = newPixel.G;
                data.RotationDegrees = rotationDegrees;
                data.Position = position;

                // add TileInfo to octree
                currentStructure.mOctree.Add(data, position);
            }

            // update data layer
            styleLayer.SetMapValue(x, z, newPixel.ARGB);
        }

        private void ClearExistingStyle(MapLayer styleLayer, int x, int z)
        {
            int previousValue = styleLayer.GetMapValue(x, z);
            if (previousValue == -1) return; // there is no previous style set

            int floorLevel = styleLayer.FloorLevel;

            // OBSOLETE - June.8.2015 - since switching to InstancedGeometry.cs no more need to remove minimesh element            
            //			Pixel previousPixel = Pixel.GetPixel(previousValue);
            //	
            //			// the first 8 bits contains the segmentIndex thus 256 possible segments (including 1 empty)
            //			int segmentIndex = previousPixel.B;
            //			
            //			// the second 8 bits contains the modelIndex thus 256 possible tile-able models per segments
            //			int modelIndex = previousPixel.G;
            //					
            //			// the remaining 16 bits contains the minimeshElement index thus 65k minimesh elements max per MinimeshGeometry
            //			int minimeshElementIndex = previousPixel.High;
            //            
            //            // NOTE: we only need access to the Structure here because we intend to remove a Minimesh element and because
            //            // sometimes the structure we want is on an adjacent Zone from 'this' which we've reached through AutoTile recursion of adjacents
            StructureVoxels currentStructure = FindStructure(styleLayer.SubscriptX, styleLayer.SubscriptY, styleLayer.SubscriptZ);
            //            
            //            // NOTE: the levelIndex for a particular floorLevel is not necessarily the same across all Zones/Structures because
            //            // they may have been added in a different order and / or may not contain same number of floors.  This is why
            //            // this call to FindLevelIndex() is dependant on the particular structure it's called on.
            //        	int levelIndex = currentStructure.FindLevelIndex (floorLevel);
            //	            
            //            InstancedGeometry instancedGeometry = GetMinimesh (currentStructure, segmentIndex, modelIndex );
            //               
            //			// note: this removes the element by disabling it and flagging it's index as free.
            //			// So we do not otherwise compact the minimesh though we should be able to
            //			// by truncating all interal arrays up to the last enabled minimesh element.			
            //			minimeshGeometry.RemoveElement ((uint)minimeshElementIndex);

            // remove TileInfo from octree - TODO: we need a position, but can we also compute an offset in ints for octree?
            // the depth i think is always the same right?
            Vector3d position = GetTilePosition(styleLayer, x, floorLevel, z);
            currentStructure.mOctree.Remove(position);
        }

        private static MinimeshGeometry GetMinimesh(StructureVoxels structure, int segmentIndex, int modelIndex)
        {
            Model model = GetModel(structure, segmentIndex, modelIndex);
            MinimeshGeometry minimeshGeometry = (MinimeshGeometry)model.Geometry;

            return minimeshGeometry;
        }

        internal static Model GetModel(StructureVoxels structure, int segmentIndex, int modelIndex)
        {
            // find the appropriate Segment
            ModelSelector segmentSelector = (ModelSelector)structure.Children[0];

            // select the appropriate model and grab the MinimeshGeometry reference 
            ModelSelector modelSelector = (ModelSelector)segmentSelector.Children[segmentIndex];
            Model model = modelSelector.Select((uint)modelIndex);
            return model;
        }

        internal void ClearAllInstances()
        {
            ModelSelector segmentSelector = (ModelSelector)this.Children[0];

            for (int i = 0; i < segmentSelector.ChildCount; i++)
            {
                ModelSelector modelSelector = (ModelSelector)segmentSelector.Children[i];
                for (int j = 0; j < modelSelector.ChildCount; j++)
                    if (modelSelector.Children[j] != null)
                    {
                        Model model = (Model)modelSelector.Children[j];
                        if (model.Geometry != null)
                            model.ClearVisibleInstances();
                    }
            }
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

            // NOTE: even we wanted to test whether we hit anywhere on a Structure regardless of
            // which specific tile, it would be a bad way to do things because we cannot assume
            // that tile geometry exists everywhere on a Structure plane.  That is why Structure Ray Picking
            // requires PickAccuracy.Tile as well.
            if ((parameters.Accuracy & PickAccuracy.Tile) == PickAccuracy.Tile)
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

        #region IBoundVolume Members
        // April.28.2015 - if we use static octree and dynamic culling of octree with immediate mode style rendering, then
        // we must override UpdateBoundVolume to use octree bbox
        protected override void UpdateBoundVolume()
        {
            if (mOctree == null || mOctree.BoundingBox == null) return;

            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty | Enums.ChangeStates.BoundingBox_TranslatedOnly);

            mBox = mOctree.BoundingBox;


            mSphere = new BoundingSphere(mBox);
            double radius = mSphere.Radius;

            // TODO: with regards to planets, we could hardcode this such that
            // it's always visible within half region range for worlds, and 1/8th region range for moons
            // the problem is, in order to render even the icon for the world, it must pass
            // the cull test.   however, if we to treat it properly like a HUD item, we would
            // only find the worlds we detected via sensors (including visual sensors (telescopes and eyeballs))
            // In this way, the world would cull, but still show up in the hud.
            // This way the hud would independantly query for the nearby worlds and generate
            // proxies for them.
            // 
            // http://astrobob.areavoices.com/2012/01/05/what-would-the-sun-look-like-from-jupiter-or-pluto/
            // 30 arc minutes equal 1/2 degree or the diameter of the sun and moon. 
            double angleDegrees = 0.5d;
            double angleRadians = Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * angleDegrees;
            // http://www.bautforum.com/showthread.php/106931-Calculating-angular-size
            // sin(angle/2) = radius/(distance+radius)
            _maxVisibleDistanceSq = radius / System.Math.Sin(angleRadians * .5); // / Radius;
            _maxVisibleDistanceSq *= _maxVisibleDistanceSq;
        }
        #endregion

        #region IDisposable
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            // structure was added internally so must be disposed here
            if (ReferencedSegments != null)
            {
                string[] keys = new string[ReferencedSegments.Count];
                ReferencedSegments.Keys.CopyTo(keys, 0);

                foreach (string segmentKey in keys)
                {
                    Resource.Repository.DecrementRef(ReferencedSegments[segmentKey]);
                    ReferencedSegments.Remove(segmentKey);
                }
            }

            if (mLevels == null) return;

            for (int i = 0; i < this.mLevels.Length; i++)
                mLevels[i].Dispose();


            //System.Diagnostics.Debug.WriteLine("Structure.DisposeUnmanagedResources() - Structure Disposed Successfully.");
        }
        #endregion
    }
}
