using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Keystone.Commands;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Types ;
using Keystone.Appearance;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Scene;
using Keystone.Celestial;
using Keystone.Portals;
using Keystone.Traversers;
using Keystone.Workspaces;
using Lidgren.Network;
using System.Collections.Generic;


namespace KeyEdit
{
    partial class FormMain : FormMainBase
    {
        private Dictionary<long, KeyCommon.Messages.MessageBase> mUnconfirmedCommands = new Dictionary<long, KeyCommon.Messages.MessageBase>();

        private Stack<KeyCommon.Messages.MessageBase> _redo = new Stack<KeyCommon.Messages.MessageBase>();
        private Stack<KeyCommon.Messages.MessageBase> _undo = new Stack<KeyCommon.Messages.MessageBase>();

        //public void UnDo()
        //{
        //    if (_undo.Count > 0)
        //    {
        //        // TODO: shouldn't undo/redo first have to send the message over the network?  And then with the arrival of the mesage
        //        // the command is queued dynamically... so here really i think we should only be queuing actual MessageBase's...
        //        Keystone.Commands.Command command = _undo.Pop();
        //        command.BeginExecute (CommandCompleted); // TODO: internally, unexecute and execute should check it's workitem to see if the underlying
        //        // command has finished from a previous execute or unexcute function
        //        // TODO: the push should come in the completed handler when the undo has completed.
        //    }
        //}

        //public void ReDo()
        //{
        //    if (_redo.Count > 0)
        //    {
        //        Keystone.Commands.Command command = _redo.Pop();
        //        command.BeginExecute(CommandCompleted);
        //    }
        //}

        public void UnDo()
        {
            if (_undo.Count > 0)
            {
                // TODO: the following _redo and _undo should NOT contain the
                // Message but the Command that wraps the message.  The Command should then
                // already directly contain links to both the Do and Undo message handlers
                // for that message types.
                // so until we fix this throw NotImplementException
                throw new NotImplementedException();

                // TODO: shouldn't undo/redo first have to send the message over the network?  And then with the arrival of the mesage
                // the command is queued dynamically... so here really i think we should only be queuing actual MessageBase's...
                //KeyCommon.Messages.MessageBase message = _undo.Pop();
                
                // now then, to execute this message it depends on the type and we must dynamically determine which
                // worker function to call based on that message type.  This is why this code should be written
                // in FormMain as it is here...
               // ExecuteMessage(message);
                
                // TODO: the push should come in the completed handler when the undo has completed.
            }
        }

        public void ReDo()
        {
            if (_redo.Count > 0)
            {
                // TODO: the following _redo and _undo should NOT contain the
                // Message but the Command that wraps the message.  The Command should then
                // already directly contain links to both the Do and Undo message handlers
                // for that message types.
                // so until we fix this throw NotImplementException
                throw new NotImplementedException();
                //KeyCommon.Messages.MessageBase message = _redo.Pop();
                //ExecuteMessage (message);
            }
        }

        /// <summary>
        /// Redo should be cleared whenever an edit occurs that was not done by Undo()
        /// </summary>
        public void ClearReDo()
        {
            _redo.Clear();
        }

        public void ClearUnDo()
        {
            _undo.Clear();
        }


        // called for new simple scenes and prefabs.  multi-region scenes use different method
        private object Worker_GenerateNewScene(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Scene_New newScene = (KeyCommon.Messages.Scene_New)cmd.Message;

            try
            {
                // try/catch here so that any exceptions are caught and we can set state to error and allow completion callback to 
                // be invoked

                // create the xmldb which will store this scene's data
                //System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(System.IO.Path.Combine(AppMain.SCENES_PATH, newScene.FolderName)));
                AppMain.CURRENT_SCENE_NAME = newScene.FolderName;
                string sceneName = newScene.FolderName;
                CreateSceneDirectory(sceneName);
               
                bool result = AppMain._core.SceneManager.CreateNewSceneDatabase(newScene.FolderName, newScene.RegionDiameterX, newScene.RegionDiameterY, newScene.RegionDiameterZ, out mStartingRegionID);

                // May.1.2017 - obsolete with changes to how user/client "joins" a new scene
                //cmd.WorkerProduct = SceneLoad(newScene.FolderName);
                cmd.WorkerProduct = state;
                return state;
            }
            catch (Exception ex)
            {
                return state;
            }
        }

        // Floorplan Design vs Active Game Object
        // a) If this is just a pure design and not a real active game object then
        //    the server creates this new vehicle into an inactive scene.
        //    This way it can still run validation checks during it's design.
        // b) If this is an active floorplan, then there is no distinction on the server.  It
        //    is just as any other game object.
        // c) However on client, if it's active, it still gets it's own seperate scene but is shared
        //    with the main scene.
        // d) Vehicle designs are stored in seperate XMLDB's though because they can be shared
        //    between games so long as the same MODs are used.
        //
        private object Worker_GenerateNewFloorplan(object state)
        {
            

            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Floorplan_New newFloorplan = (KeyCommon.Messages.Floorplan_New)cmd.Message;

            try
            {
                // try/catch here so that any exceptions are caught and we can set state to error and allow completion callback to 
                // be invoked

                // create the xmldb which will store this scene's data
                uint octreeDepth = 3;

                // some basic boundary tests
                uint cellsAcross = newFloorplan.CellCountX;
                if (cellsAcross < 1) cellsAcross = 1;

                uint cellsDeep = newFloorplan.CellCountZ;
                if (cellsDeep < 1) cellsDeep = 1;

                uint cellsLayers = newFloorplan.CellCountY;
                if (cellsLayers < 1) cellsLayers = 1;

                float cellWidth = newFloorplan.CellWidth;
                if (cellWidth < 1) cellWidth = 1;

                float cellHeight = newFloorplan.CellHeight;
                if (cellHeight < 1) cellHeight = 1;

                float cellDepth = newFloorplan.CellDepth;
                if (cellDepth < 1) cellDepth = 1;

                XMLDatabase xmldb = new XMLDatabase();
                SceneInfo info = xmldb.Create(Keystone.Scene.SceneType.SingleRegion, newFloorplan.FolderName, typeof(Keystone.Portals.Root).Name);

                // NOTE: we want our region taht contains the floor plan to be double the size
                // so that our camera can zoom out enough.  Thus we do not use * .5f;  when computing
                // our min and max vectors
                Vector3d min = new Vector3d(-newFloorplan.CellCountX * newFloorplan.CellWidth,
                    -newFloorplan.CellCountY * newFloorplan.CellHeight,
                    -newFloorplan.CellCountZ * newFloorplan.CellDepth);
                Vector3d max = new Vector3d(newFloorplan.CellCountX * newFloorplan.CellWidth,
                    newFloorplan.CellCountY * newFloorplan.CellHeight,
                    newFloorplan.CellCountZ * newFloorplan.CellDepth);

                System.Diagnostics.Debug.Assert (System.IO.Directory.Exists (newFloorplan.FolderName));
                string sceneName = System.IO.Path.GetFileName ( newFloorplan.FolderName );
                CreateSceneDirectory (sceneName);
                
                string nodeName = SceneManagerBase.CreateRootNodeName (typeof(Keystone.Portals.Root).Name, sceneName);
                Keystone.Portals.Root r = new Keystone.Portals.Root(nodeName, (float)(max.x - min.x), (float)(max.y - min.y), (float)(max.z - min.z), octreeDepth);


                bool useOctree = false;
                bool useCelledRegion = true;
                string regionName = "";
                Keystone.Portals.Region interior;

                
                // NOTE: if you want the interior to "move" you must never translate the region itself.
                // Regions are fixed and can never be scaled, rotated, translated no matter what type of derived
                // Region entity it is!
                // For a ship interior to move, you must add that interior Region to a Container.cs instance
                // such as a Vehicle
                string name = Repository.GetNewName(typeof(Keystone.Vehicles.Vehicle));
                Keystone.Vehicles.Vehicle veh = new Keystone.Vehicles.Vehicle(name);
                if (useOctree)
                {
                    throw new Exception("Interiors don't use octrees");
                    regionName = Repository.GetNewName(typeof(Keystone.Portals.Region));
                    BoundingBox box = new BoundingBox(new Vector3d(), cellsAcross * cellsDeep);
                    interior = new Keystone.Portals.Region(regionName, box, 6);
                }
                else if (useCelledRegion)
                {
                    // NOTE: Interior is a region but with the CelledRegion option
                    // an array of celled space is created that is primarily for portals and AI hint/diffusion layers
                    regionName = Repository.GetNewName(typeof(Keystone.Portals.Interior));
                    interior = (Keystone.Portals.Region)Repository.Create (regionName, "Region");

                    interior.AddChild (
                        new Keystone.Portals.Interior(regionName,
                            cellWidth, cellHeight, cellDepth,
                            cellsAcross, cellsLayers, cellsDeep));
                }
                else
                {
                    // TODO: how much faster is it without celledregion?  but with more realistic
                    // floorplan and not just wall-less flag field
                    regionName = Repository.GetNewName(typeof(Keystone.Portals.Region));
                    interior = (Keystone.Portals.Region)Repository.Create (regionName, "Region");
                }

                
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                Keystone.Appearance.DefaultAppearance app;
                Keystone.Elements.Mesh3d exteriorMesh = null;


                // obsolete - saving of exterior box not required.  Floorplans now require
                // a valid mesh to be passed in
                exteriorMesh = Keystone.Elements.Mesh3d.CreateBox(cellsAcross * cellWidth, cellsLayers * cellHeight, cellsDeep * cellDepth);
                //if (!System.IO.File.Exists(exteriorMesh.ID))
                //{
                //    exteriorMesh.SaveTVResource(exteriorMesh.ID);
                //}

                if (exteriorMesh != null)
                {
                    Keystone.ImportLib.GetMeshTexturesAndMaterials(exteriorMesh, out app);
                    if (app != null)
                        model.AddChild(app);
                }

                model.AddChild(exteriorMesh);
                veh.AddChild (model);
                // 1) Start a new floor plan design - Uses a unique temporary scene
                //         that will only have this PlayerVehicle design as a first level child.
                //         - This allows us to edit offline without being connected.
                //         - Once in floorplan design mode, only limited Game Objects can be added.
                //           - eg. no stars,moons, etc... only those objects that are for buidling decks.
                //
                //  a) dialog pops up specifying dimensions
                //  b) PlayerVehicle create command is sent with an Interior with specified dimensions
                //  c) Server creates the Vehicle but does not add it to the Scene as an active game object.
                //     It is just a design.
                //      - but do we not need for the Scene object to match?  I somewhat don't like
                //       having things handled differently on client and on server.
                //     Because on client, the design is handled as a seperate scene entirely.
                //     What if we have a "FloorplanDesigns" scene on both client and server
                //     But on client, only local designs can be shown and also, only one active 
                //     design at a time.  The rest will be invisible.  But server side, there's never
                //     any rendering.  
                //     - but... what if i want to have different design rules for different games when
                //     a server is hosting multiple games?  
                //     Ideally here we'd still want our floorplan designer to have vehicles as a
                //     sub-scene of the main scene.
                //     -  Well in a sense, the PlayerVehicle itself is a sub-scene and the Interior
                //     is an entirely seperate region logically contained within the PlayerVehicle.
                //     - Normally a seperate "Simulation" object is created for each Scene, would we
                //     need seperate sims for each Vehicle interior then?
                //     - Maybe we can share the same scene, but with a seperate RenderingContext
                //     that specifies to only render a specific interior.
                // IMPORTANT: An active design is not same as active game object.
                //  - the active design gets updated as a prefab
                //  - the game object does not, only in the scene xmldb
                // 2) Open an existing floor plan design
                //
                // 3) Refit an active interior
                // 4) View an active interior and re-route power, security access
                veh.Pickable = false;
                veh.Visible = false;

                // NOTE: currently our repository will increment ref the interior
                // which triggers the scene treeview "Add Node Handler()" if the interior
                // is added to the veh before the veh itself is added to the Region.
                // Thus the interior will not have a parent in the treeview.  So its best
                // to add the interior after the exterior veh is added to the Region.
                // I'm not sure of any way to fix this.  This does not occur during deserialization
                // because all entities are spawned and added to Scene before their children.
                r.AddChild(veh);
                veh.AddChild(interior);

                xmldb.WriteSychronous(r, true, true, false);

                name = Repository.GetNewName(typeof(Viewpoint));
                Viewpoint vp = Viewpoint.Create(name, r.ID);
                info.AddChild (vp);
                
                xmldb.WriteSychronous(info, true, false, false);
                xmldb.SaveAllChanges();
                xmldb.Dispose();

                // NOTE: we no longer load the scene from here, but in the main thread on ProcessCompletedCommandQueue();
                // TODO: note in SceneLoad() we should first check if a scene is already loaded
                cmd.WorkerProduct = state; // SceneLoad(newFloorplan.FolderName);
                return state;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FormMain.Worker_GenrateFloorplan() " + ex.Message);
                return state;
            }
        }

        private object Worker_GenerateNewTerrainScene (object state)
        {
        	Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Scene_NewTerrain newTerrainScene = (KeyCommon.Messages.Scene_NewTerrain)cmd.Message;

            uint octreeDepth = uint.Parse (AppMain._core.Settings.settingRead("scene", "octreedepth"));

            try
            {
            	System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                    
				// TODO: we should get the root node name from the returned newUniverse message from server.
				//       Then we can create the child zones ourselves because those are named based off the root's name
				System.Diagnostics.Debug.Assert (System.IO.Directory.Exists(Path.Combine (AppMain.SCENES_PATH, newTerrainScene.FolderName )));
	            string sceneName = System.IO.Path.GetFileName (newTerrainScene.FolderName);
                AppMain.CURRENT_SCENE_NAME = sceneName;
                CreateSceneDirectory(sceneName);
                
                Keystone.Portals.ZoneRoot root; 
                Keystone.Scene.SceneInfo info;
                Keystone.IO.XMLDatabase xmldb;
  
                uint structureLevelsHigh = newTerrainScene.RegionResolutionY ;
                AppMain._core.SceneManager.CreateNewSceneDatabase (newTerrainScene.FolderName, sceneName,
                                                                 Keystone.Scene.SceneType.MultiRegionTerrainVoxels,
                                                                  newTerrainScene.RegionsAcross, newTerrainScene.RegionsHigh, newTerrainScene.RegionsDeep,
                                                                  newTerrainScene.RegionDiameterX, newTerrainScene.RegionDiameterY, newTerrainScene.RegionDiameterZ,
                                                                  newTerrainScene.SerializeEmptyZones, 
                                                                  newTerrainScene.OctreeDepth, 
                                                                  structureLevelsHigh,
                                                                  out root,
                                                                  out info,
                                                                  out xmldb);
                                                                             	
                // create and write to disk the regions we will need to pass to the universe gen
                Keystone.Portals.Zone[, ,] regions = null;
                if (info.SerializeEmptyZones)
                    regions = Keystone.Portals.Zone.Create(root, octreeDepth, xmldb);

                
                Keystone.Lights.DirectionalLight light = LightsHelper.LoadDirectionalLight(AppMain.REGION_DIAMETER * 0.5f);
                SuperSetter setter = new SuperSetter(root);
                setter.Apply(light);

                // Add Skydome
                SkyHelper.AddGradientSky(root, AppMain.FARPLANE - 1000d, AppMain.FARPLANE);

                // this routine essentially creates a cube shaped galaxy.  It can be made
                // to be more irregular by haing the Z level go to rnd(min) to rnd(maxDiameter)
                // it takes diameter of our galaxy and then begings to plot stars
                // Basically it starts at coordinates 1,1,1 and then goes to 1,1,2 then 1,1,3, etc
                // til it eventually reaches Diameter,Diameter,Diameter
                // so the maximum number of stars we can have in our system is Diameter^3
                // The actual diameter of our galaxy in lightyears is the MinimumSystemSeperation * Diameter.
                // So if the minimum seperation (as set by the user) is 2 light years then
                // our diamter in light years is 2 * Diameter.  So if the diameter is 10 then our cube is
                // actually 20 light years across and can hold at most 1000 (or 10^3) star systems.
                // Incidentally, Generating 1000 star systems as detailed as we are doing will take quite
                // a while, but fortunately its jsut for creating the initial map.

                // //note that the user is setting the minimum distance between systems in LightYears
                //   but we calculate all of our locations in AU so we must convert
                int regionsAcross = (int)root.RegionsAcross;
                int regionsHigh = (int)root.RegionsHigh;
                int regionsDeep = (int)root.RegionsDeep;

                int totalZoneCount = regionsAcross * regionsHigh * regionsDeep;

                
                System.Diagnostics.Debug.WriteLine("-- {0} -- ZONE GENERATION BEGINNING", regionsAcross * regionsHigh * regionsDeep);
                System.Diagnostics.Debug.Assert (regionsHigh == 1, "Worker_GenerateNewTerrainScene() - Y Zone Count must == 1.  No other value allowed. StructureLevels are used for height changes in the map.");
                for (int x = 0; x < regionsAcross; x++)
                {
                    for (int y = 0; y < regionsHigh; y++)
                    {
                        for (int z = 0; z < regionsDeep; z++)
                        {

                            Zone zone = null;
                            if (info.SerializeEmptyZones)
                                zone = regions[x, y, z];

                            // terrain entities gets added and written to disk and then the region is unloaded and we move to the next
                            if ((newTerrainScene.Mode & KeyCommon.Messages.Scene_NewTerrain.CreationMode.DefaultTerrain) == KeyCommon.Messages.Scene_NewTerrain.CreationMode.DefaultTerrain)
                            {
                                // if we are NOT serializing empty zones, the current Zone will not have
								// been created above and so will be NULL here.  So we will need to create the zone instance 
                                // BEFORE WE CAN ADD THE TERRAIN PATCH TO IT
                                if (info.SerializeEmptyZones == false)
                                {
                                    // NOTE: Here even though client side we are creating the child zone off root
                                    //       we can compute the child's name because it's based off the root Zone's name
                                    string name = root.GetZoneName(x, y, z);

                                    BoundingBox box = root.GetChildZoneSize();
                                    // position offset multiplier
                                    float offsetX = root.StartX + x;
                                    float offsetY = root.StartY + y;
                                    float offsetZ = root.StartZ + z;

                                    zone = new Zone(name, box, octreeDepth, x, y, z, offsetX, offsetY, offsetZ);
                                }
                                
                                bool useStructure = true;
                                bool useVoxels = false;
                                // Mesh based terrain using a tile based structure
                                if (useStructure)
                                {
                                    Keystone.TileMap.Structure structure = GenerateIsometricTerrain(x, y, z, newTerrainScene.RegionDiameterX, newTerrainScene.RegionDiameterZ);
                                    zone.AddChild(structure);

                                }
                                else if (useVoxels)
                                {
                                    Keystone.TileMap.StructureVoxels voxelStructure = GenerateVoxelTerrain(x, y, z, newTerrainScene.TileSizeY, newTerrainScene.MinimumFloor, newTerrainScene.MaximumFloor, (int)newTerrainScene.TerrainTileCountY, (int)newTerrainScene.OctreeDepth);
                                    zone.AddChild(voxelStructure);
                                }
                                else
                                {
                                    ModeledEntity tvterrain = GenerateTVLandscapeTerrain();
                                    zone.AddChild(tvterrain);
                                }
                                xmldb.WriteSychronous(zone, true, false, false);
                            }
                            
                            
                            // now we can write this region which has a fully created star (or empty) system with planets and moons
                            if (info.SerializeEmptyZones == true && zone.ChildCount == 0)
                                xmldb.WriteSychronous(zone, true, false, false); // must not increment/decrement 
                            
                            // TODO: removeChildren here can screw up the scene whilst IPageable resources
                            // that depend on this branch are being unloaded and removed from Repository!
                            // so how to fix this aspect of IPageableNode
                            // TODO: I should be able to put some kind of "abort" status on it?
                            // but how to ensure i cover all cases?

                            if (zone != null) // empty zones with serializeemptyzones == false will result in zone == null here so test for it
                            {
                                // Dec.6.2012 - zone.RemoveChildren() is wrong.
                                //      in fact tests show that zone.RemoveChildren() is not required 
                                // 		so long as IncrementRef/DecrementRef removes from cache and will trigger cascade of RemoveChild 
                                // 		as all parent refcounts == 0.
                                // zone.RemoveChildren();  // remove the structure if it exists
                                //Repository.Remove(zone);
                                
                                Repository.IncrementRef(zone); // artificially raise refcount to 1
                                Repository.DecrementRef(zone); // force refcount back to 0 and this will trigger removal from cache and cascade to children
                                System.Diagnostics.Debug.Assert (zone.RefCount == 0, "FormMain.Commands.Worker_GenerateNewTerrain() - RefCount != 0.");
                            }
                            //System.Diagnostics.Debug.WriteLine("ZONES REMAINING = " + (--totalZoneCount).ToString());
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("-- {0} -- ZONES CREATED", regionsAcross * regionsHigh * regionsDeep);

                // TODO: this is misleading... we think zone with name 0,0,0 is the center but its not. It's simply the first
                // zone subscript!  We need a GetCenterZone or something.. similarly, when we add the Sol system, we should place it
                // in the center zone, NOT zone 0,0,0
                mStartingRegionID = root.GetZoneName(0, 0, 0);

                // NOTE: ZoneRoot does not actually add any Zone's as children here.  Otherwise they would be deserialized automatically
                // when we actually want the Pager to handle loading and unloading of child Zones to the ZoneRoot.
                xmldb.WriteSychronous(root, true, true, false);
									
                xmldb.WriteSychronous(info, true, false, false);
                
                // TODO: is there a way to make our "xmldb" not use zip but keep file mode
                //       but still of course keep the different xml files for each?  And also what about an option
                //       where each Zone can have seperate xml files?  but we'd still want some resources shared across zones...
                //       so Entities wont be shared, but geometry and texture and materials will
                xmldb.SaveAllChanges();
                xmldb.Dispose();

                // NOTE: Node.ctor adds to Repository with refcount == 0, however the call above to xmldb.Create() already does IncrementRef and DecrementRef on the SceneInfo
                //       node to remove it from Repository.
                //Repository.IncrementRef(info); 
                //Repository.DecrementRef(info); 
                // however we do need to remove StarDigest and Viewpoints
                info.RemoveChildren();
                
                stopWatch.Stop();
                    System.Diagnostics.Trace.WriteLine(string.Format("Terrain Scene generated in {0} seconds", stopWatch.Elapsed.TotalSeconds));


                // NOTE: we no longer load the scene from here, but in the main thread on ProcessCompletedCommandQueue();
                // TODO: note in SceneLoad() we should first check if a scene is already loaded
                cmd.WorkerProduct = state; // SceneLoad(newTerrainScene.FolderName);
                return state;
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine("Worker_GenerateNewTerrain() - ERROR - " + ex.Message);
                return state;
            }
        }

        private Keystone.TileMap.StructureVoxels GenerateVoxelTerrain(int x, int y, int z, double voxelSizeY, int minFloor, int maxFloor, int maxFloorCount, int octreeDepth)
        {
            // NOTE: structure ID MUST be based on zone ID 
            string id = Keystone.TileMap.StructureVoxels.GetStructureID(x, y, z);
            Keystone.TileMap.StructureVoxels voxelStructure = (Keystone.TileMap.StructureVoxels)Repository.Create(id, "StructureVoxels");
            voxelStructure.SetProperty("floorheight", typeof(double), voxelSizeY);
            voxelStructure.SetProperty("minfloor", typeof(int), minFloor);
            voxelStructure.SetProperty("maxfloor", typeof(int), maxFloor);
            voxelStructure.SetProperty("maxfloorcount", typeof(int), maxFloorCount);
            voxelStructure.SetProperty("octreedepth", typeof(uint), octreeDepth);



            // ----------------------------------------------
            // DEBUG TEMP - GENERATE LEVEL VARS
            //int zoneTileWidth = (int)newTerrainScene.RegionResolutionX;
            //int zoneTileDepth = (int)newTerrainScene.RegionResolutionZ;
            //double floorHeight = newTerrainScene.TileSizeY;
            //string persistDimensions = newTerrainScene.RegionDiameterX.ToString() + "," +
            //                           floorHeight.ToString() + "," +
            //                           newTerrainScene.RegionDiameterZ.ToString() + ",";
            string levelsPersistString;
            int numLevels;


            return voxelStructure;
        }


        private Keystone.TileMap.Structure GenerateIsometricTerrain(int x, int y, int z, float regionDiameterX, float regionDiameterZ)
        {
            // Zone will have a visible tile based structure such as floors, walls and ceilings.
            // The structure is regarded as part of the Region and not as seperate entities.
            // This is for performance (rendering and memory consumption) primarily.
            // Since having seperate entities for every Tile in the 

            // NOTE: structure ID MUST be based on zone ID 
            string id = Keystone.TileMap.Structure.GetStructureID(x, y, z);
            Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)Repository.Create(id, "Structure");
            //structure.SetProperty("floorheight", typeof(double), (double)newTerrainScene.TileSizeY);
            //structure.SetProperty("minfloor", typeof(int), newTerrainScene.MinimumFloor);
            //structure.SetProperty("maxfloor", typeof(int), newTerrainScene.MaximumFloor);
            //structure.SetProperty("maxfloorcount", typeof(int), (int)newTerrainScene.TerrainTileCountY);



            // ----------------------------------------------
            // DEBUG TEMP - GENERATE LEVEL VARS
            int zoneTileWidth = 32; // (int)newTerrainScene.RegionResolutionX;
            int zoneTileDepth = 32; // (int)newTerrainScene.RegionResolutionZ;
            double floorHeight = 2.82983f; // newTerrainScene.TileSizeY;
            string persistDimensions = regionDiameterX.ToString() + "," +
                                       floorHeight.ToString() + "," +
                                       regionDiameterZ.ToString() + ",";
            string levelsPersistString;
            int numLevels;

            // ----------------------------------------------
            // DEBUG TEMP - GENERATE DEFAULT STRUCTURE LEVELS
            // Index 0 = Floor Level -1 = underground
            // Index 1 = Floor Level 0 = above ground
            // Index 2 = Floor Level 1 = air above 
            //	- when placing items upon Level2 (and on top of Level 1) a new air Level gets added
            //    as Level 3, and Level 2 items will write to obstacle layer of Level 1
            // since we deduce filepaths from layer name, all we need is the name of the layer and it's level
            string[] layerNames = new string[] { "obstacles", "layout", "style" };
            const int TERRAIN_SEGMENT_INDEX = 1;
            byte segmentIndex = TERRAIN_SEGMENT_INDEX;

            for (int floorLevel = -1; floorLevel <= 0; floorLevel++)
            {
                // begin temp: create these bitmaps and save to disk.... seed with random values from 0 - N for now
                for (int n = 0; n < layerNames.Length; n++)
                {
                    int initializationValue = 0;
                    if (layerNames[n] == "obstacles")
                    {
                        initializationValue = 0; // segments placed on this LEVEL affect obstacle map of the level BELOW it!
                    }
                    else if (layerNames[n] == "layout")
                    {
                        if (floorLevel == -1)
                            initializationValue = segmentIndex;
                        else
                            initializationValue = 0; // 0 == null empty segment
                    }
                    else if (layerNames[n] == "style") // "style"
                    {
                        // style has to be discovered during autotile
                        initializationValue = -1;
                    }

                    ProceduralHelper.InitializeMapLayerBitmap(layerNames[n], floorLevel, x, z, zoneTileWidth, zoneTileDepth, initializationValue);
                }
            }
            numLevels = 2;
            // format: numLevels, worldDimensions, {floorLevel, numLayers, layerNames{}}
            levelsPersistString = numLevels + "," + persistDimensions + "-1,3,obstacles,layout,style,0,3,obstacles,layout,style";
            //						        	// END TEMP - GENERATE DEFAULT STRUCTURE LEVELS
            // ----------------------------------------------

            //// BEGIN TEMP - GENERATE PROCEDURAL BASED LEVEL DATA
            //// ----------------------------------------------

            //// - for this zone, determine range of floor levels we need to generate based on altitude
            ////   of the terrain. (NOTE: subterranian caverns not generated yet)
            ////   - note: unlike above where each level has same initializationValue, here
            ////     visible levels will have varying initialization values for each x,z tile location based on
            ////     whether terrain exists there or not.
            //int seed = 0;
            //int minFloorLevel, maxFloorLevel;
            //numLevels = ProceduralHelper.GenerateMapLayerBitmap(seed, 
            //                                                    x,  z,
            //                                                    zoneTileWidth, zoneTileDepth, 
            //                                                    structureLevelsHigh, newTerrainScene.MinimumFloor, newTerrainScene.MaximumFloor - 1, 
            //                                                    out minFloorLevel, out maxFloorLevel);

            //levelsPersistString = numLevels + "," + persistDimensions;

            //string delimitedText = null;
            //for (int i = minFloorLevel; i <= maxFloorLevel; i++)
            //{
            //	if (string.IsNullOrEmpty(delimitedText) == false)
            //		delimitedText +=",";

            //	delimitedText += i + ",3,obstacles,layout,style";
            //}

            //levelsPersistString += delimitedText;
            //// ----------------------------------------------
            //// END TEMP - GENERATE PROCEDURAL BASED LEVEL DATA


            // TODO: this persist string after structure.SetProperty ("maplevels"...) is being ignored.  Actually, I think it's being
            //       overwritten by another persist string that is computed because no actual Levels and Layers are created being added to Structure!  
            //       What we're trying to do is generate the save file without having to load the level and serialize the xml.
            //       So the question then is, can we override the overwriting of the persist string so that we do not need to use
            //       the hackish "persistPath" file.
            //       WAIT: The other reason we use persistPath is so we can load MapLayer's in Pager without needing to load in Zones
            //       or their Structures and that way when we do load structures, the AutoTile will work across Zones because the MapLayer
            //       will be loaded already so we can see what types of segments exist in relevant adjacent tiles across zone boundaries.
            structure.SetProperty("maplevels", typeof(string), levelsPersistString); // <- is being overwritten by computed persist string during structure.PersistFloorLevels()

            // We need to update the actual persist file with this string for the current structure because
            // assigning "maplevels" property above is not working.
            string persistPath = Keystone.TileMap.Structure.GetLayersDataPath(x, z);
            System.IO.File.WriteAllText(persistPath, levelsPersistString);

            // hard coded default dirt segment
            string[] modelLookupPaths = new string[] { AppMain.ModName + @"\meshes\terrain\dirt.kgbsegment" };
            // Add a default segment to go with the default floor i've painted in the layout above
            structure.SetProperty("modellookuppaths", typeof(string[]), modelLookupPaths);

            // domain objects (aka entity scripts) are assigned via entity.ResourcePath
            //        	string scriptPath = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\scripts\tile_structure.css";
            //        	structure.ResourcePath = scriptPath;

            return structure;
        }

        private ModeledEntity GenerateTVLandscapeTerrain()
        {
            // TVLandscape based terrain
            // create terrain Entity and add to zone
            // TODO: terrain entity names should be generated by server and sent back to client to use
            string terrainEntityID = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity newTerrain = new ModeledEntity(terrainEntityID);
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            string geometryID = Repository.GetNewName(typeof(Terrain));
            Terrain terrainGeometry = (Terrain)Repository.Create(geometryID, "Terrain");
            terrainGeometry.SetProperty("heightmap", typeof(string), null); // empty default terrain 
                                                                            // force loading of terrainGeometry resource since this is already in Worker thread
                                                                            // TODO: i don't think loading of the terrain is necessary here since it actually doesn't need to be rendered here.
                                                                            //       that will occur when this generated terrain scene XML is read in and rebuilt.
                                                                            //terrainGeometry.LoadTVResource ();

            // splatting appearance
            string appearanceID = Repository.GetNewName(typeof(SplatAppearance));
            SplatAppearance appearance = new SplatAppearance(appearanceID);
            // we'll use single group that is same for all chunks
            Material material = Material.Create(Material.DefaultMaterials.matte);
            appearance.AddChild(material);

            // TODO: this path is just a resource path to find textures, it's not a mod path at all
            string path = System.IO.Path.Combine(AppMain._core.ModsPath, "terrain");

            string texturePath1 = System.IO.Path.Combine(path, "grass1.png");
            string texturePath2 = System.IO.Path.Combine(path, "rock 6.png");
            string texturePath3 = System.IO.Path.Combine(path, "dirt 1.png");
            string texturePath4 = System.IO.Path.Combine(path, "snow 1.png");
            string alphaPath = "";     // we will be autogenerating the values contained in our alpha map using our AutoUpdateOpacityMap() method


            Keystone.Appearance.SplatAlpha splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
            Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(texturePath1, "Texture");
            tex.TextureType = Texture.TEXTURETYPE.Default;
            splatLayer.AddChild(tex);
            appearance.AddChild(splatLayer);
            //appearance.AddDefine("DIFFUSEMAP", null);

            splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
            tex = (Texture)Keystone.Resource.Repository.Create(texturePath2, "Texture");
            tex.TextureType = Texture.TEXTURETYPE.Default;
            splatLayer.AddChild(tex);
            appearance.AddChild(splatLayer);

            splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
            tex = (Texture)Keystone.Resource.Repository.Create(texturePath3, "Texture");
            tex.TextureType = Texture.TEXTURETYPE.Default;
            splatLayer.AddChild(tex);
            appearance.AddChild(splatLayer);

            splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
            tex = (Texture)Keystone.Resource.Repository.Create(texturePath4, "Texture");
            tex.TextureType = Texture.TEXTURETYPE.Default;
            splatLayer.AddChild(tex);
            appearance.AddChild(splatLayer);


            model.AddChild(appearance);
            model.AddChild(terrainGeometry);
            newTerrain.AddChild(model);


            return newTerrain;
        }

        private object Worker_GenerateNewUniverse(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Scene_NewUniverse newUniverse = (KeyCommon.Messages.Scene_NewUniverse)cmd.Message;

            try
            { 
                // obsolete - GenerateNewUniverse() now occurs server side and the relevant Zones are sent to client via a series of Spawn() messages.
                //            OR can we just send the Zone xml.  It would have the proper EntityIDs already for us.  WE could use a new command SpawnZone perhaps
                //GenerateNewUniverse(newUniverse);
                // April.23.2017 - obsolete.  server now sends a command to load the scene when player
                //                 initiates a "join_request" command
                //cmd.WorkerProduct = SceneLoad(newUniverse.FolderName);
                return state;
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine("Worker_GenerateNewUniverse() - ERROR - " + ex.Message);
                return state;
            }
            finally 
            {
            	ClientPager.Disabled = false;
            }
        }

        // NOTE: Generating a new scene is NOT the same thing as LOADING a scene. For instance, no scene is added to scenemanager when generating
        private void GenerateNewUniverse(KeyCommon.Messages.Scene_NewUniverse newUniverse)
        {
            // TODO: i should be using seperate seed for each stellar system, star, world, asteroid field rather than
            // passing a _random.  This is because we cannot create the individual system, star, world, etc without having
            // a seed for each.  
            // TODO: so i believe to do this is to use the _random created with the first seed, to actually generate a seed to use
            // in the next call.  This way that seed value can be stored, and this way that star,world,etc can be restored from just
            // the initial seed value.
            Keystone.Utilities.XXHash hash = new Keystone.Utilities.XXHash(newUniverse.RandomSeed);
            Random _random = new Random(newUniverse.RandomSeed); // new Random((int)hash.GetHash(new int[] { 0, 0, 0 }));
            // NOTE: we generate all star systems and stars because they're needed for starmap and navigation screen
            StellarSystemGenerator _systemGen = new StellarSystemGenerator(newUniverse.RandomSeed);
            // TODO: generate worlds and moons only as needed. page in and out as required, but i think actually, this is what Zones are for.
            GenerateWorld _worldGen = new GenerateWorld(_random);
            GenerateMoon _moonGen = new GenerateMoon(_random);
            List<StellarSystem> _systems = new List<StellarSystem>();

            uint octreeDepth = uint.Parse(AppMain._core.Settings.settingRead("scene", "octreedepth"));

            try
            {
                // Feb.29.2024 - Disabling the pager does NOT prevent loading of Entity scritps since we need to have Scripts loaded if we want to save CustomProperties
                 ClientPager.Disabled = true;

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                // TODO: we should get the root node name from the returned newUniverse message from server.
                //       Then we can create the child zones ourselves because those are named based off the root's name
                System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(System.IO.Path.Combine(AppMain.SCENES_PATH, newUniverse.FolderName)));
                string sceneName = System.IO.Path.GetFileName(newUniverse.FolderName);
                AppMain.CURRENT_SCENE_NAME = sceneName;

                Keystone.Portals.ZoneRoot root;
                Keystone.Scene.SceneInfo info;
                Keystone.IO.XMLDatabase xmldb;

                // Create new Scene XML DB and Viewpoints
                AppMain._core.SceneManager.CreateNewSceneDatabase(newUniverse.FolderName, sceneName,
                                                                  Keystone.Scene.SceneType.MultiReginSpaceStarsAndWorlds,
                                                                  newUniverse.RegionsAcross, newUniverse.RegionsHigh, newUniverse.RegionsDeep,
                                                                  newUniverse.RegionDiameterX, newUniverse.RegionDiameterY, newUniverse.RegionDiameterZ,
                                                                  newUniverse.SerializeEmptyZones,
                                                                  0,
                                                                  0,
                                                                  out root,
                                                                  out info,
                                                                 out xmldb);

                PagerBase.Disabled = true;

                if (newUniverse.CreateStarDigest) // todo: add checkbox for GenerateStarfield
                {
                    int[] starCount = new int[] { 5000, 1000, 100 };
                    int[] colors = new int[]{Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32()};

                    float variance = 1.0f;
                    //float[] spriteSize = new float[] {500 + (500 * (float)rand.NextDouble()),
                    //            500 + (500 * (float)rand.NextDouble()),
                    //            500 + (500 * (float)rand.NextDouble())};
                    float[] spriteSize = new float[] { 250, 500, 1000 };

                    float radius = 90000;

                    string[] texture = new string[] { @"caesar\Shaders\Planet\stardx7.png", @"caesar\Shaders\Planet\stardx7.png", @"caesar\Shaders\Planet\stardx7.png" }; // star2.dds";
                    string fieldName = "starfield_" + Repository.GetNewName(typeof(Entity)); // "starfield1"  // random name means we should be able to produce more than one

                    Entity field = Keystone.Celestial.ProceduralHelper.CreateRandomStarField(fieldName, texture, radius, starCount, spriteSize, colors);
                    field.Name = "starfield";
                    field.Translation = Vector3d.Zero(); // TODO: isn't this pos irrelevant as it follows camera?

                    root.AddChild(field);
                }

                // create and write to disk the regions we will need to pass to the universe gen
                Keystone.Portals.Zone[,,] regions = null;
                if (info.SerializeEmptyZones)
                    regions = Keystone.Portals.Zone.Create(root, octreeDepth, xmldb);

                // this routine essentially creates a cube shaped galaxy.  It can be made
                // to be more irregular by haing the Z level go to rnd(min) to rnd(maxDiameter)
                // it takes diameter of our galaxy and then begins to plot stars
                // Basically it starts at coordinates 1,1,1 and then goes to 1,1,2 then 1,1,3, etc
                // til it eventually reaches Diameter,Diameter,Diameter
                // so the maximum number of stars we can have in our system is Diameter^3
                // The actual diameter of our galaxy in lightyears is the MinimumSystemSeperation * Diameter.
                // So if the minimum seperation (as set by the user) is 2 light years then
                // our diamter in light years is 2 * Diameter.  So if the diameter is 10 then our cube is
                // actually 20 light years across and can hold at most 1000 (or 10^3) star systems.
                // Incidentally, Generating 1000 star systems as detailed as we are doing will take quite
                // a while, but fortunately its jsut for creating the initial map.

                // //note that the user is setting the minimum distance between systems in LightYears
                //   but we calculate all of our locations in AU so we must convert
                int width = (int)root.RegionsAcross;
                int height = (int)root.RegionsHigh;
                int depth = (int)root.RegionsDeep;

                int totalZoneCount = width * height * depth;
                                
                System.Diagnostics.Debug.WriteLine("-- {0} -- ZONE GENERATION BEGINNING", width * height * depth);
                

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        for (int k = 0; k < depth; k++)
                        {
                            Zone zone = null;
                            if (info.SerializeEmptyZones)
                                zone = regions[i, j, k]; 

                            if (newUniverse.Mode != KeyCommon.Messages.Scene_NewUniverse.CreationMode.Empty)
                            {
                                KeyCommon.Messages.UniverseCreationParams celestialParams = newUniverse.mParams;
                                // systems gets added and written to disk and then the region is unloaded and we move to the next
                                if (totalZoneCount == 1 || ShouldSystemGoHere(celestialParams.ClusterDensity, _random))
                                {
                                    // TODO: for empty Zones, shouldn't we still have a default DirectionalLight? or we could add one when the Zone is paged in.
                                    // if we are NOT serializing empty zones, the current Zone will not have
                                    // been created above and so be NULL here. So we will need to create the zone instance obviously
                                    // BEFORE WE CAN ADD A STELLAR SYSTEM TO IT
                                    if (info.SerializeEmptyZones == false)
                                    {
                                        // NOTE: Here even though client side we are creating the child zone off root
                                        //       we can compute the child's name because it's based off the root Zone's name
                                        string name = root.GetZoneName(i, j, k);

                                        BoundingBox box = root.GetChildZoneSize();
                                        float offsetX = root.StartX + i;
                                        float offsetY = root.StartY + j;
                                        float offsetZ = root.StartZ + k;

                                        zone = new Zone(name, box, octreeDepth, i, j, k, offsetX, offsetY, offsetZ);
                                    }

                                    StellarSystem newsystem;
                                    // TODO: we should have a flag in the newUniverse command for AddSolSystem = true
                                   
                                    int centerX, centerY, centerZ;
                                    root.GetZoneCenterSubscripts(out centerX, out centerY, out centerZ);
                                    // todo: we should add a default viewpoint to the SceneInfo pointing to this zone as well.
                                    if (totalZoneCount == 1 || (i == centerX && j == centerY && k == centerZ))
                                        newsystem = Keystone.Celestial.ProceduralHelper.GenerateSolSystem(new Vector3d(0, 0, 0), newUniverse.RandomSeed);
                                    else
                                    {
                                        // create new stellar system and add to zone
                                        uint starCount = GetNumberofStars(celestialParams, _random);
                                        newsystem = _systemGen.GenerateSystem(starCount);


                                        // - flesh out the solar system if this is not a bare bones universe with stars only
                                        if (celestialParams.GeneratePlanets)
                                            GenerateWorldsForSystem(newsystem, _worldGen, _random, celestialParams.GenerateMoons, celestialParams.GeneratePlanetoidBelts);
                                    }
                                    zone.AddChild(newsystem);

                                    // only create db star record AFTER system is added to zone or globaltranslations will be incorrect
                                    if (newUniverse.CreateStarDigest)
                                    {
                                        CreateStarSystemDatabaseRecords(newsystem);
                                    }

                                    // Nov.5.2016 - WriteSychronous() is very slow.  It is the bottleneck during universe creation
                                    // TODO: Maybe faster if I can do one XML file per Zone or 
                                    //       switch to sqlite.  First i should try the one XML file per Zone though... and Stars 
                                    //       and worlds should be inline.  The other problem is that textures, models are all in one file still.
                                    //       so even if we break up the Zones, they still wind up pointing to other bloated XML files. In other words
                                    //       if instead we could have a seperate FOLDER for each Zone, and then have the XML in those folders be just
                                    //       for entities and resources in that Zone.  In effect, we're talking about a seperate XMLDB for each Zone.
                                    //       The problem there is, shared resources are no longer shared properly. Is that a problem?


                                    xmldb.WriteSychronous(zone, true, false, false);
                                }
                            }
                            // now we can write this region which has a fully created star (or empty) system with planets and moons
                            if (info.SerializeEmptyZones == true && zone.ChildCount == 0)
                                xmldb.WriteSychronous(zone, true, false, false); // must not increment/decrement 
                            // TODO: removeChildren here can screw up the scene whilst IPageable resources
                            // that depend on this branch are being unloaded and removed from Repository!
                            // so how to fix this aspect of IPageableNode
                            // TODO: I should be able to put some kind of "abort" status on it?
                            // but how to ensure i cover all cases?

                            if (zone != null) // empty zones with serializeemptyzones == false will result in zone == null here so test for it
                            {
                                // Dec.6.2012 - zone.RemoveChildren() is wrong.
                                //      in fact tests show that zone.RemoveChildren() is not required 
                                // 		so long as IncrementRef/DecrementRef removes from cache and will trigger cascade of RemoveChild 
                                // 		as all parent refcounts == 0.
                                //zone.RemoveChildren();  
                                //Repository.Remove(zone);
                                Repository.IncrementRef(zone); // artificially raise refcount to 1
                                Repository.DecrementRef(zone); // force refcount back to 0 and this will trigger removal from cache and cascade to children
                            }

                            //System.Diagnostics.Debug.WriteLine("ZONES REMAINING = " + (--totalZoneCount).ToString());
                        }
                    }
                }

                // NOTE: Disabling paging for resources (except for DomainObject scripts for now until we start persisting to .save)
                //       But even with disabling paging of most resources, the generation is slow because the xmldb.WriteSynchronous() is taking
                //       ~30 seconds whereas the actual galaxy generation for 5x1x5 galaxy takes just 2 seconds.
                PagerBase.Disabled = false;

                System.Diagnostics.Debug.WriteLine("-- {0} -- ZONES CREATED", width * height * depth);


                // NOTE: ZoneRoot does not actually add any Zone's as children here.  Otherwise they would be deserialized automatically
                // when we actually want the Pager to handle loading and unloading of child Zones to the ZoneRoot.
                xmldb.WriteSychronous(root, true, true, false);

                xmldb.WriteSychronous(info, true, false, false);
                xmldb.SaveAllChanges();
                xmldb.Dispose();

                // NOTE: Node.ctor adds to Repository with refcount == 0, however the call above to xmldb.Create() already does IncrementRef and DecrementRef on the SceneInfo
                //       node to remove it from Repository.
                //Repository.IncrementRef(info); 
                //Repository.DecrementRef(info); 
                // however we do need to remove StarDigest and Viewpoints
                info.RemoveChildren();

                stopWatch.Stop();
                System.Diagnostics.Trace.WriteLine(string.Format("Universe generated in {0} seconds", stopWatch.Elapsed.TotalSeconds));
                
            }
            catch (Exception ex)
            {
            }
        }


        private void CreateStarSystemDatabaseRecords(StellarSystem system)
        {
            // In SQLite3 opening the db also creates the file if necessary
            // NOTE: GetConnection() returns an opened database connection 
            System.Data.SQLite.SQLiteConnection conn = Database.AppDatabaseHelper.GetConnection();


            // using a single transaction allows MUCH faster inserts
            using (var transaction = conn.BeginTransaction())
            {
                for (int n = 0; n < system.StarCount; n++)
                {
                    Database.AppDatabaseHelper.CreateStarRecord(system.Stars[n], conn);

                    for (int m = 0; m < system.Stars[n].ChildCount; m++)
                    {
                        World planet = system.Stars[n].Children[m] as World;
                        if (planet != null)
                        {
                            Database.AppDatabaseHelper.CreateWorldRecord(planet, conn);

                            for (int p = 0; p < planet.ChildCount; p++)
                            {
                                World moon = planet.Children[p] as World;
                                if (moon != null)
                                {
                                    Database.AppDatabaseHelper.CreateWorldRecord(moon, conn);
                                }
                            }
                        }
                    }
                }
                transaction.Commit();
            }
            conn.Close();
        }

        // crew and world generation belong in game01.dll
        private void GenerateCrew(string interiorID, int crewCount, int seed)
        {
            if (crewCount <= 0) return;

            // todo:  Maybe we can use a callback from .CreateCharacters() to then load the models?
            // todo: i need a ratio for the department each member will be assigned and i dont even know yet what full list of departments there will be. 
            // todo: need to construct a chain of command
            // todo: our behaviorContext can be assigned to mCustomData as well if its not already
            Game01.GameObjects.Character[] characters = Game01.ProcGen.CreateCharacters(crewCount, _core.Seed);
            System.Diagnostics.Debug.Assert(crewCount == characters.Length);
            BonedEntity[] bonedEntities = new BonedEntity[crewCount];

            string[] relativePaths = new string[crewCount];
            string[] malePrefabs = new string[] { "caesar\\actors\\colonel-x.kgbentity" };
            string[] femalePrefabs = new string[] { "caesar\\actors\\aiko_physics.kgbentity" };


            // NOTE: using parallel.for() breaks SceneReader which is not thread safe particularly when it comes to shared behavior tree nodes
            //System.Threading.Tasks.Parallel.For(0, crewCount, i =>
            //{
            //    string[] prefabs = malePrefabs;

            //    if (characters[i].Gender == 1)
            //        prefabs = femalePrefabs;
            //    // todo: when i have multiple male and female models, i may need to know their rank and department to determine which model to use
            //    Random random = new Random(seed + i);
            //    bonedEntities[i] = GenerateCrewModel(prefabs, random); // todo: pass in characters[i] so we have access to more data about this crew member to determine the models to use
            //    bonedEntities[i].CustomData = new KeyCommon.Data.UserData();
            //    // todo: same should be done for celestial bodies.  Celestial should be merged into game01.dll and all propertiies for it should be assigned by the script
            //    bonedEntities[i].CustomData.SetObject("character", characters[i]);
            //    bonedEntities[i].Name = characters[i].FirstName + " " + characters[i].LastName;
            //    relativePaths[i] = AppMain.CURRENT_SCENE_NAME + "\\" + bonedEntities[i].ID + ".kgbentity";
            //});

            for(int i = 0; i < crewCount; i++)
            {
                string[] prefabs = malePrefabs;

                if (characters[i].Gender == 1)
                    prefabs = femalePrefabs;
                // todo: when i have multiple male and female models, i may need to know their rank and department to determine which model to use
                Random random = new Random(seed + i);
                bonedEntities[i] = GenerateBonedEntity(prefabs, random); // todo: pass in characters[i] so we have access to more data about this crew member to determine the models to use
                // todo: i think i need to add a random seed counter to all Entities
                bonedEntities[i].BlackboardData = new KeyCommon.Data.UserData();

                // todo: same should be done for celestial bodies.  Celestial should be merged into game01.dll and all propertiies for it should be assigned by the script
                bonedEntities[i].BlackboardData.SetObject("character", characters[i]);
                bonedEntities[i].Name = characters[i].FirstName + " " + characters[i].LastName;
                relativePaths[i] = AppMain.CURRENT_SCENE_NAME + "\\" + bonedEntities[i].ID + ".kgbentity";

                // set custom properties for Station operators. Grab highest ranking Characters for starters
            }


            // todo: server ultimately in real client/server configuration, needs to be able to send Character info over and the client can store it how it wishes.  
            //       So GameObjects may need to implement NetBuffer read/write
            // todo: database needs to accomodate storing/retreiving Game01.GameObjects.Character
            // todo: i believe the bonedEntities[i].ID is the primary key we are using and so when we delete bonedEntities, we know which record to delete.
            Database.AppDatabaseHelper.CreateCharacterRecords(bonedEntities, interiorID, relativePaths);

            for (int i = 0; i < crewCount; i++)
            {
                bonedEntities[i].SRC = null;
                bonedEntities[i].SetFlagValue("forceserializeseperate", true);
                Scene.WriteEntity(bonedEntities[i], true);
                // NOTE: we do not add these BonedEntities to the Vehicle.Interior.
                // when vehicle is successfully spawned, then server can start spawning the Crew
            }
        }

        // we are only generating IDs here, we dont need scripts or any other resource
        private BonedEntity GenerateBonedEntity(string[] prefabs, Random random)
        {
            int index = random.Next(prefabs.Length);
            string relativePath = prefabs[index];
               
            string fullPath = Path.Combine(AppMain.MOD_PATH, relativePath);
            bool delayResourceLoading = true; // we are only generating IDs here, we dont need scripts or any other resource
            bool generateIDs = true;
            BonedEntity entity = (BonedEntity)LoadEntity(fullPath, relativePath, generateIDs, true, delayResourceLoading, null, new Vector3d()); // NOTE: initial (eg: first run after generation) crew translations are calculated in Loopback upon Interior region load completed.  We need Interior loaded in order to find the unoccupied FLOOR flags.

            return entity;
        }

        private bool CrewPositionsNeedInitialization(Database.AppDatabaseHelper.CharacterRecord[] characters)
        {
            if (characters == null || characters.Length == 0) return false;

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Translation != new Vector3d())
                    return false;
            }

            return true;
        }

        private Vector3d[] PositionCrew(string parentID, int count)
        {
            if (string.IsNullOrEmpty(parentID)) return null;

            Keystone.Portals.Interior interior = (Keystone.Portals.Interior)Repository.Get(parentID);
            System.Diagnostics.Debug.Assert(interior.TVResourceIsLoaded);

            //Keystone.Portals.Interior.TILE_ATTRIBUTES.COMPONENT;

            Vector3d[] positions = new Vector3d[count];
            int flag = (int)Keystone.Portals.Interior.TILE_ATTRIBUTES.FLOOR;
            uint[] cells = interior.GetCellList(0, (interior.CellCountX * interior.CellCountY * interior.CellCountZ) - 1, flag);

            if (cells == null || cells.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("PositionCrew() - Failed to find any available FLOOR cells");
                return null;
            }
            // todo: this random is not using a seed
            Random random = new Random();
            // we already have an OUT_OF_BOUNDS flag and TILEMASK.FLOOR i think.  We would have to find only FLOOR and no other OBSTACLE flags on them
            bool[] occupied = new bool[cells.Length]; // of the pruned cells, flag the ones that are already occupied with an actor
            for (uint i = 0; i < positions.Length; i++)
            {
                uint cellIndex = (uint)random.Next(0, cells.Length);
                while (occupied[cellIndex] == true)
                {
                    cellIndex = (uint)random.Next(0, cells.Length);
                }

                occupied[cellIndex] = true;
                uint cellID = cells[cellIndex];
                positions[i] = interior.GetCellCenter(cellID);
                positions[i].y = positions[i].y - (interior.CellSize.y / 2d);
            }

            return positions;

        }
        
        //private object Worker_LoadScene_Request(object state)
        //{
        //    Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
        //    KeyCommon.Messages.Scene_Load_Request loadSceneRequest = (KeyCommon.Messages.Scene_Load_Request)cmd.Message;
        //    bool approved = loadSceneRequest.Approved;
        //    return state;
        //}

        //private object Worker_LoadScene(object state)
        //{
        //    // nothing to do here for loopback on server side.  
        //    // Just let it go to CommandCompleted where we can 
        //    // actually load the scene
        //    Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
        //    KeyCommon.Messages.Scene_Load loadScene = (KeyCommon.Messages.Scene_Load)cmd.Message;
        //    string sceneName = loadScene.FolderName;
        //    CreateSceneDirectory(sceneName);
        //    //mStartingRegionID = loadScene.StartingRegionID; - obsolete - this is now set during SimJoinApproved() 
        //    cmd.WorkerProduct = SceneLoad(loadScene.FolderName);
        //    return state;
        //}

        private object Worker_SimulationJoin(object state) // this worker executes after we receive a Sim_Join command from the server. Note: receiving that message from server denotes that we have been approved
        {

            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Simulation_Join simJoin = (KeyCommon.Messages.Simulation_Join)cmd.Message;
            string saveName = simJoin.FolderName;
            mStartingRegionID = simJoin.RegionID;
  //          AppMain.mLocalVehicleID = simJoin.VehicleID;

           
           // cmd.WorkerProduct = SimulationJoin(simJoin.FolderName);
           
            // first send the user a command to load the scene.  When the player
            // get the Scene_Load command, it needs to wait for the startingregionid to load
            // before initiating "Spawn".  Perhaps we can add username,passowrd to the Scene_Load_Request
            // so server can verify user before allowing them to load the scene? 
            // TODO: should we pass the user's vehicle here?  How do we load other vehicles in other zones?  We should probably tell server we have completed loading of the Zone and need list of entities to spawn?
            // TODO: what is the difference between components inside the vehicle and crew?  do we only "spawn" vehicles and crew?  The main difference is they are moveable entities.
            // TODO: we need to make sure that when the user's or AI's vehicle crosses zone boundaries, we don't try to load it again
 //            SceneLoadRequest(simJoin.FolderName, mStartingRegionID);

            CreateSceneDirectory(simJoin.FolderName);
            //mStartingRegionID = loadScene.StartingRegionID; - obsolete - this is now set during SimJoinApproved() 
            cmd.WorkerProduct = SceneLoad(simJoin.FolderName);
            return state;
        }

        private void CreateSaveDirectory(string sceneName)
        {
            AppMain.CURRENT_SCENE_NAME = sceneName;
            string currentSaveFolderPath = Path.Combine(AppMain.SAVES_PATH, AppMain.CURRENT_SCENE_NAME);

            if (System.IO.Directory.Exists(currentSaveFolderPath) == false)
                try
                {
                    System.IO.Directory.CreateDirectory(currentSaveFolderPath);
                    
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("FormMain.CreateSaveDirectories() - " + ex.Message);
                    throw ex;
                }
        }

        private void CreateSceneDirectory(string sceneName)
        {
        	string currentScenePath = System.IO.Path.Combine(_core.ScenesPath, sceneName);
            string relativeLayerDataPath = System.IO.Path.Combine (sceneName, _core.RelativeLayerDataPath);
            string fullLayerDataPath = System.IO.Path.Combine (_core.ScenesPath, relativeLayerDataPath);
            
            _core.Scene_Name = sceneName;
            
            if (System.IO.Directory.Exists (fullLayerDataPath) == false)
            	try
                {
                	System.IO.Directory.CreateDirectory (fullLayerDataPath);
                }
                catch (Exception ex)
                {
                	System.Diagnostics.Debug.WriteLine ("FormMain.CreateSceneDirectories() - " + ex.Message);
                	throw ex;
                }
        }

        /// this routine determines if a new system should be created
        ///  at a particular region in space.  It uses the
        ///  Density setting to determine the odds that a system is created
        ///  or not.  The higher the Density, the better the chance that a system
        ///  will be created in that region
        private bool ShouldSystemGoHere(float clusterDensity, Random rand)
        {
            return (rand.NextDouble() <= clusterDensity);
        }

        /// this function returns the number of companion stars are created based
        /// on the users settings.  Possible outcomes are 1, 2, 3 or 4 (for now)
        private uint GetNumberofStars(KeyCommon.Messages.UniverseCreationParams mParams, Random rand)
        {
            double d = rand.NextDouble() * 100;
            uint result = 0;

            //  Generate random byte between 1 and 100.
            if (d <= mParams._percentSingleStarSystems)
                result = 1;
            else if (d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems)
                result = 2;
            else if (d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem)
                result = 3;
            else
            {
                result = 4;
                Debug.Assert(d >= 0 &&
                             d <= mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem +
                                  mParams._percentQuadrupleStarSystems);
                Debug.Assert(mParams._percentSingleStarSystems + mParams._percentBinaryStarSystems + mParams._percentTrinarySystem +
                             mParams._percentQuadrupleStarSystems == 100f);
            }
            return result;
        }

        
        /// <summary>
        /// After each system is created and before the region is written to xml, this is called to generate the orbital
        /// info for worlds that will be in the system.
        /// </summary>
        /// <param name="system"></param>
        private void GenerateWorldsForSystem(StellarSystem system, GenerateWorld worldGen, Random rand, bool moonGenerationEnabled, bool planetoidBeltGenerationEnabled)
        {
            // generate positions for worlds around stars and the star system 
            // note: zoneGenerator just generates the habitable/forbidden/inner/outer zones for the star system
            OrbitSelector zoneGenerator = new OrbitSelector(rand);
            zoneGenerator.Apply(system);

            List<OrbitSelector.OrbitInfo> orbits = new List<OrbitSelector.OrbitInfo>();
            // note: orbitGenerator just generates the orbits and the types of planets for each star systems based on it's orbtial zone information
            foreach (OrbitSelector.OrbitalZoneInfo zoneInfo in zoneGenerator.OribtalZones)
                orbits.AddRange(zoneGenerator.GenerateOrbits(zoneInfo));


            // flesh out the full world statistics for each planet
            foreach (OrbitSelector.OrbitInfo orbit in orbits)
            {
            	string id = Repository.GetNewName (typeof(World));
            	World newplanet = new World(id);
				newplanet.Name = orbit.ParentBody.GetFreeChildName();
				// TODO: is this generating unique orbit's based on Bode's Law or is it
                //       just creating random orbits?  Because for moons, it seems there 
                //       are too many bunched together.
                // NOTE: Remember child Entity.Translation is always relative to parent Entity. This simplifies calculation.
                // make sure this translation fits within the current region's radius.  Ideally the entire planet
                // and it's orbit should fit within the bounds of the Zone 
                newplanet.Translation = new Vector3d(orbit.OrbitalRadius, 0, 0);
                // TODO: System.Diagnostics.Trace.Assert(PlanetFitsEntirelyInSystem(newplanet));
                // TODO: eventually compute more sophisticated positions instead of linear along x axis
                //       note: this is now fixed by selecting start epoch in orbit animation
                newplanet.OrbitalRadius = orbit.OrbitalRadius;
                newplanet.WorldType = orbit.WorldType;

                // NOTE: if planetoidBeltGenerationEnabled == false, we'll replace the belt with a planet
                if (orbit.WorldType == WorldType.PlanetoidBelt && planetoidBeltGenerationEnabled)
                {
                    int asteroidCount = 1000;
                    //Celestial.PlanetoidBelt belt = new Celestial.PlanetoidBelt();
                    // TODO: create a PlanetoidField object that inherits Region perhaps and then uses our new
                    // system of "circled covered wagon formation" of boxes which we'll be procedurally generated during rendering
                    // i.e no need to store every asteroid.  This will mitigate some of the performance annoyance with saving regions to disk
                    // TODO: newPlanet in this case should actually be thought of as a "field"
                    // and perhaps rather than a world at all, we should pass in an OctreeRegion
                    ProceduralHelper.InitAsteroidField(newplanet, (float)orbit.OrbitalRadius, asteroidCount);
                }
                else
                {
                    if (orbit.ParentBody is Star)
                    {
                        Star star = (Star)orbit.ParentBody;
                        worldGen.ComputeWorldStatistics(newplanet, star.Age, star.Luminosity, star.LuminosityClass,
                                            star.SpectralType, star.SpectralSubType,
                                            orbit, orbit.Zone);

                        if (moonGenerationEnabled)
                        {
                            // generate moons using SINGLE star stats
                            Keystone.Celestial.GenerateMoon moonGen = new GenerateMoon(rand);
                            World[] moons = moonGen.GenerateMoons(newplanet, orbit, star.Age, star.Luminosity, (byte)star.LuminosityClass,
                                (float)orbit.OrbitalZoneInfo.SnowLine, orbit.Zone);

                            if (moons != null)
                                for (int i = 0; i < moons.Length; i++)
                                {
                                    worldGen.ComputeWorldStatistics(moons[i], star.Age, star.Luminosity, star.LuminosityClass,
                                                    star.SpectralType, star.SpectralSubType,
                                                    orbit, orbit.Zone);

                                    // create visual model of moon and add moon as child to world
                                    ProceduralHelper.InitWorldVisuals(newplanet, moons[i], true, false, false, false);
                                }
                        }
                    }
                    else // starsystem
                    {
                        // TODO: temp hack hardcoded values
                        SPECTRAL_TYPE spectralType = SPECTRAL_TYPE.M;
                        SPECTRAL_SUB_TYPE spectralSubType = SPECTRAL_SUB_TYPE.SubType_5;
                        LUMINOSITY highestLuminosityClass = LUMINOSITY.WHITEDWARF_D;
                        float oldestAge = 0;
                        //  multistar systems usually have stars of same age since they usually form together, exceptions are when stars capture other stars that formed seperately.  there are rules for exceptions for very old 10billion+ year systems too but i dont know if i implemented those

                        float combinedLuminosity = 0;

                        for (int j = 0; j < ((StellarSystem)orbit.ParentBody).StarCount; j++)
                        {
                            combinedLuminosity += ((StellarSystem)orbit.ParentBody).Stars[j].Luminosity;

                            highestLuminosityClass =
                                highestLuminosityClass.CompareTo(((StellarSystem)orbit.ParentBody).Stars[j].Luminosity) > 0
                                    ?
                                        highestLuminosityClass
                                    : ((StellarSystem)orbit.ParentBody).Stars[j].LuminosityClass;
                        }
                        // TODO:  read the rules and figure out what to use for spectraltype and subtype and
                        // verify im handling luminosity correctly with combined and highest for class
                        worldGen.ComputeWorldStatistics(newplanet, oldestAge, combinedLuminosity, highestLuminosityClass,
                                 spectralType, spectralSubType,
                                orbit, orbit.Zone);

                        // generate moons using COMBINED star stats
                        if (moonGenerationEnabled)
                        {
                            Keystone.Celestial.GenerateMoon moonGen = new GenerateMoon(rand);
                            World[] moons = moonGen.GenerateMoons(newplanet, orbit, oldestAge, combinedLuminosity, (byte)highestLuminosityClass,
                                (float)orbit.OrbitalZoneInfo.SnowLine, orbit.Zone);

                            if (moons != null)
                                for (int i = 0; i < moons.Length; i++)
                                {
                                    worldGen.ComputeWorldStatistics(moons[i], oldestAge, combinedLuminosity, highestLuminosityClass,
                                                    spectralType, spectralSubType,
                                                    orbit, orbit.Zone);

                                    // create visual model of moon and add moon as child to world
                                    ProceduralHelper.InitWorldVisuals(newplanet, moons[i], true, false, false, false);
                                }
                        }
                    }

                    // create visual model of planet
                    ProceduralHelper.InitWorldVisuals(orbit.ParentBody, newplanet, orbit.WorldType == WorldType.Terrestial, orbit.WorldType != WorldType.Terrestial, true, true);
                    //Debug.WriteLine (orbit.WorldType.ToString() + " planet '" + newplanet.ID +"' placed at position " + newplanet.Translation.ToString () + " has radius of " + newplanet.Radius.ToString ());
                }


            } // end for
        }

        
        #region Archive Related
        private object Worker_ArchiveRenameEntry(object state)
        {
             Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Archive_RenameEntry renameFile = (KeyCommon.Messages.Archive_RenameEntry)cmd.Message;

            if (string.IsNullOrEmpty (renameFile.ZipEntry) || string.IsNullOrEmpty (renameFile.RelativeZipFilePath)) return state;

            Ionic.Zip.ZipFile zip = null;
            try
            {
                string zipFullPath = System.IO.Path.Combine(_core.ModsPath, renameFile.RelativeZipFilePath);
                zip = new Ionic.Zip.ZipFile(zipFullPath);

                KeyCommon.IO.ArchiveIOHelper.RenameZipEntry(zip, renameFile.ZipEntry, renameFile.NewEntryName);

                zip.Save();
                Trace.WriteLine(string.Format("Worker_ArchiveRenameEntry() -- Successfully renamed {0} from archive '{1}'.", renameFile.ZipEntry, renameFile.RelativeZipFilePath));
                return state;
            }
            catch (Exception ex)
            {
                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
                Trace.WriteLine(string.Format("Worker_ArchiveRenameEntry() -- Error renaming file from archive '{0}'. {1}.", renameFile.RelativeZipFilePath, ex.Message));
                return state;
            }
            finally
            {
                if (zip != null) zip.Dispose();
            }
            return state;
        }

        private object Worker_ArchiveAddFolder(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Archive_AddFolder addFolder = (KeyCommon.Messages.Archive_AddFolder)cmd.Message;

            Ionic.Zip.ZipFile zip = null;
            try
            {
                string zipFullPath = System.IO.Path.Combine(AppMain.MOD_PATH, addFolder.ModName);
                zip = new Ionic.Zip.ZipFile(zipFullPath);
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;

                DateTime timeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);

                if (zip.ContainsEntry(addFolder.FolderNameToAdd))
                {
                    System.Diagnostics.Debug.WriteLine("Worker_ArchiveAddFolder() - Folder already exists in zip");
                    return state;
                }
                Ionic.Zip.ZipEntry e = zip.AddDirectoryByName(addFolder.FolderNameToAdd);
                //ZipEntry e = zip.AddDirectory(mDirectoryName, addFolder.TargetPath); // this requires that a file directory actually exists.  Not what we want.
                e.LastModified = timeStamp;

                zip.Save();
                return state;
            }
            catch (Exception ex)
            {
                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
                Trace.WriteLine(string.Format("AddDirectoryToArchive.ExecuteWorker() -- Error adding directory to archive '{0}'. {1}.", addFolder.ModName, ex.Message));
                return state;
            }
            finally
            {
                if (zip != null) zip.Dispose();
            }
            return state;
        }

        private object Worker_ModImportFile(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Archive_AddFiles addFiles = (KeyCommon.Messages.Archive_AddFiles)cmd.Message;

            KeyCommon.IO.ArchiveIOHelper.AddFilesToMod(AppMain.MOD_PATH, 
                    addFiles.ModName, 
                    addFiles.SourceFiles, 
                    addFiles.EntryDestinationPaths, 
                    addFiles.EntryNewFilenames);
            return state;
        }

        private object Worker_ModImportGeometryAsEntity(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Archive_AddGeometry addGeometry = (KeyCommon.Messages.Archive_AddGeometry)cmd.Message;

            // TODO: this does not remove the item from the Repository after the obj is imported to the archive
            try
            {
                string ext = System.IO.Path.GetExtension(addGeometry.SourceFilePath);
                bool isActor = ext.ToUpper() == Keystone.ImportLib.TVACTOR_EXTENSION;
                if (!isActor)
                    // TODO: if .x file and boned, we can't tell from just extension alone! Perhaps require ModelView to convert them all to TVA first.
                    isActor = (addGeometry.LoadXFileAsActor && ext.ToUpper() == Keystone.ImportLib.XFILE_EXTENSION);

                //if (if (ext.ToUpper() == ImportLib.WAVEFRONTOBJ_EXTENSION && _openAsEditableMesh)
                //    //_resource = MeshEditable.Create(FilePath, FilePath, LoadTextures, LoadMaterials);
                //    _resource = Keystone.EditDataStructures.EditableMesh.Create(mPathInArchive, mPathInArchive, LoadTextures, LoadMaterials);

                // Mesh3d.Create can accept either a 
                // for now we'll always load the mesh or actor
                System.Collections.Generic.List<string> sourceFiles = new System.Collections.Generic.List<string>();
                System.Collections.Generic.List<string> targetPaths = new System.Collections.Generic.List<string>();

                // add the geometry source file itself to be included into the mod archive
                sourceFiles.Add(addGeometry.SourceFilePath);
                string entityDestinationPath = System.IO.Path.Combine(addGeometry.ModName, addGeometry.EntryDestinationPath);
                targetPaths.Add(entityDestinationPath.Replace ("entities", "meshes"));

                Keystone.Animation.BonedAnimation[] clips;
                Keystone.Elements.Geometry geometry;
                Keystone.Appearance.DefaultAppearance appearance = null;
                Keystone.Entities.ModeledEntity entity;
                Model model = new Model(Repository.GetNewName(typeof(Model)));

                KeyCommon.IO.ResourceDescriptor resource = new KeyCommon.IO.ResourceDescriptor(addGeometry.SourceFilePath);
                if (isActor) // TODO: if .x file and boned, we can't tell from just extension alone! Perhaps require ModelView to convert them all to TVA first.
                {
                	entity = (BonedEntity)Repository.Create (Repository.GetNewName(typeof(BonedEntity)), "BonedEntity");
                    geometry = Actor3d.Create(resource.ToString(), MTV3D65.CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, addGeometry.LoadTextures, addGeometry.LoadMaterials, out appearance);
                    model.AddChild(geometry);
                    string id = Repository.GetNewName(typeof(Keystone.Animation.Animation));
                    Keystone.Animation.Animation animation = new Keystone.Animation.Animation(id);
                   
                    // animation nodes from Actor3d ARE shareable as long as all duplicates have same
                    // animations with same names / indices
                    clips = ((Actor3d)geometry).GetAnimations();
                    // Keystone.Animation.AnimationController animSet = null;
                    //// Automatically add AnimationSet and intrinsic Animations if exists
                    //if (animations != null && animations.Length > 0)
                    //{
                    //    string animSetName = Repository.GetNewName (typeof (Keystone.Animation.AnimationController));
                    //    animSet = new Keystone.Animation.AnimationController(animSetName);

                    //    for (int i = 0; i < animations.Length; i++)
                    //        animSet.AddChild (animations[i]);
                    //}
                    
                    //if (animSet != null)
                    //    entity.AddChild(animSet);

                    // NOTE: the above is when AnimationSet was a Group node.  Now it's AnimationController
                    // and is not a Node, just a helper object within Entities.  So
                    // animations now ALL get added directly to Entities
                    for (int i = 0; i < clips.Length; i++)
                    {
                        // BonedAnimations should still have a direct reference to the
                        // Actor3d it references... at least the friendly name so it can be restored
                        // when the Actor3d resource is added/paged in
                        clips[i].TargetName = model.Name;

                        animation.AddChild(clips[i]);
                    }
                    entity.AddChild(animation);
                }
                else
                {
                    if (addGeometry.InteriorContainer)
                    {
                        entity = new Keystone.Vehicles.Vehicle(Repository.GetNewName(typeof(Keystone.Vehicles.Vehicle)));
                    }
                    else
                        entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
                    
                    // TDO: this is bad. this call to Mesh3d.GetAppearance effectively means we are parsing and loading the mesh twice
                    appearance = Mesh3d.GetAppearance (resource.ToString()); // , addGeometry.LoadTextures, addGeometry.LoadMaterials, out appearance
                    geometry = (Mesh3d)Repository.Create(resource.ToString(), "Mesh3d");
                    model.AddChild(geometry);
                }


                
                if (appearance != null) // appearance may be null if loadmaterials + loadtextures = false or none exist whatsoever in the geometry file
                    model.AddChild(appearance);

                 // NOTE: Prior to saving as prefab in our mod archive, the resourcePath for Mesh3d must point to the new
                //         location within this archive.
                // NOTE: we only change the ResourcePath _AFTER_ entity.AddChild(model), since this geometry was initially created with a 
                //          disk resource path not an archive resource path. 
                entity.AddChild(model); // <-- Only change ResourcePath from original src path on drive to archive entry path AFTER this is done.

                string foldername = System.IO.Path.GetDirectoryName(addGeometry.SourceFilePath);
                // filename is the .x/.tvm/.tva/.obj file NOT the .kgbentity xml file!!!
                string filename = System.IO.Path.GetFileName(addGeometry.SourceFilePath);
                // todo: should geometry, textures, and materials be saved to \\meshes\\  \\textures\\ \\materials\\ relative folders?
                // todo: i need a xml material format and materials need a pageable resource file
                
                string geometryDestinationPath = entityDestinationPath.Replace ("entities", "meshes");
                string texturesDestinationPath = entityDestinationPath.Replace("entities", "textures"); ;
                string materialsDestinationPath = entityDestinationPath.Replace("entities", "materials"); 
                // todo: this seems problematic as setting the Geometry.ResourcePath is same as changing it's ID
                geometry.ResourcePath = System.IO.Path.Combine(geometryDestinationPath, filename).ToString();
				
                // Now we need to add associated texture files and .obj mtl files to the relative path of the main .kgbmodel
                // we're currently not supporting renaming any of these files so it's best for user to always add a to a new sub directory
                // just for this new model to avoid any inadvertant existing file conflicts/overwrites in the archive
                if (addGeometry.LoadTextures || addGeometry.LoadMaterials)
                {
                    if (ext.ToUpper() == ".OBJ")
                    {
                        // TODO: hrm, to create our .kgbmodel requires that we have a Model instance so maybe we do have to 
                        // load the geometry so we can then add the appearance and hierarchically save the .xml.
                        // for .obj's we dont have to create the mesh at all, we just need to grab any material lib names
                        // and texture file names.  Instead of Mesh3d.Create which creates a .TVM a well, we'll only just directly call
                        // ParseWaveFrontObj
                        string[] materials;
                        string[] textures;
                        Keystone.Loaders.WavefrontObjLoader.ParseWaveFrontObj(addGeometry.SourceFilePath, false, true, out materials, out textures);
                        if (materials != null)
                            for (int i = 0; i < materials.Length; i++)
                            {
                                sourceFiles.Add(System.IO.Path.Combine(foldername, materials[i]));
                                targetPaths.Add(materialsDestinationPath);
                            }

                        if (textures != null)
                            for (int i = 0; i < textures.Length; i++)
                            {
                                sourceFiles.Add(System.IO.Path.Combine(foldername, textures[i]));
                                targetPaths.Add(texturesDestinationPath);
                            }

                        // OBJ - Texture Resource Renaming <-- hackish but works
                        if (appearance != null)
                        {
                            System.Collections.Generic.List<Node> attributes2 = new System.Collections.Generic.List<Node>();
                            // we want to get the actual texture file names if this is an .x or .tva/.tvm file
                            if (appearance.Children != null)
                            foreach (Keystone.Elements.Node child in appearance.Children)
                            {
                                if (child is Keystone.Appearance.GroupAttribute)
                                {
                                	Node[] children = ((IGroup)child).Children;
                                	if (children == null) continue;
                                	for  (int i = 0; i < children.Length; i++)
                                    {
                                		Node attribute = children[i];
                                        if (attribute is Layer)
                                        {
                                            if (!attributes2.Contains(attribute))
                                            {
                                                KeyCommon.IO.ResourceDescriptor attributeRes = new KeyCommon.IO.ResourceDescriptor(((Layer)attribute).Texture.ResourcePath);
                                                string path = System.IO.Path.Combine(foldername, attributeRes.FileName);

                                                attributes2.Add(attribute);

                                                // NOTE: since this texture is being added to the specified archive, the resourcePath must point
                                                // to the new location within this archive. 
                                                string textureFilename = attributeRes.FileName;
                                                string newResourcePath = System.IO.Path.Combine(texturesDestinationPath, textureFilename).ToString();

                                                // clone, rename, and replace in hierarchy
                                                Texture texture = ((Layer)attribute).Texture;
                                                // NOTE: we pass neverShare = true to force a new node to be created otherwise it passes back same shareable texture node
                                                // NOTE; this is confusing. Perhaps the newResourcePath does not euqal the existging texture's retail path 
                                                Texture clone = (Texture)texture.Clone (newResourcePath, true, true, true);
                                                ((IGroup)attribute).RemoveChild (texture);
                                                ((Keystone.Appearance.Layer)attribute).AddChild (clone);
                                            }
                                        }
                                    }
                                }
                            }
                            attributes2.Clear();
                        }
                    }
                    else
                    {
                        // TVM/X/TVA - Texture Resource Renaming AND source/target file adds <-- hackish but works
                        if (appearance != null)
                        {
                            System.Collections.Generic.List<Node> attributes = new System.Collections.Generic.List<Node>();
                            // we want to get the actual texture file names if this is an .x or .tva/.tvm file
                            foreach (Keystone.Elements.Node child in appearance.Children)
                            {
                                if (child is Keystone.Appearance.GroupAttribute)
                                {
                            		Node[] children = ((IGroup)child).Children;
                                	if (children == null) continue;
                                	for  (int i = 0; i < children.Length; i++)
                                    {
                                		Node attribute = children[i];
                                        if (attribute is Layer)
                                        {
                                        
                                            if (!attributes.Contains(attribute))
                                            {
                                                KeyCommon.IO.ResourceDescriptor attributeRes = new KeyCommon.IO.ResourceDescriptor(((Layer)attribute).Texture.ResourcePath);
                                                string path = System.IO.Path.Combine(foldername, attributeRes.FileName);
                                                
                                                attributes.Add(attribute);
                                                sourceFiles.Add(path);
                                                targetPaths.Add(texturesDestinationPath);

                                                string textureFilename = attributeRes.FileName;

                                                // NOTE: since this texture is being added to the specified archive, the resourcePath must point
                                                // to the new location within this archive.
                                                // TODO: the resourcepath is updated, but the "id" is not.  For all IPageableResource the
                                                // id should match the resource path.
                                                string newResourcePath = System.IO.Path.Combine(texturesDestinationPath, textureFilename).ToString();

                                                try 
                                                {
                                                	// clone because the resource path we want to use, is not the same as the resource path of the current Texture? rename, and replace in hierarchy
	                                                Texture texture = ((Layer)attribute).Texture;
	                                                // NOTE: we pass neverShare = true to force a new node to be created otherwise it passes back same shareable texture node
	                                                Texture clone = (Texture)texture.Clone (newResourcePath, true, true, true);
	                                                ((IGroup)attribute).RemoveChild (texture);
	                                                ((Keystone.Appearance.Layer)attribute).AddChild (clone);
                                                }
                                                catch (Exception ex)
                                                {
                                                	System.Diagnostics.Debug.WriteLine ("Worker_ArchiveAddGeometry() - " + ex.Message + " Cannot rename resource. Skipping.");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            attributes.Clear();
                        }
                    }
                }
                else
                {
                    // no loading of materials or textures so we'll only be storing the xml prefab itself
                }

                if (addGeometry.InteriorContainer)
                {
                    uint quadtreeDepth = 6;
                    // TODO: the interior's entity ID should match that from the server.  I think since we
                    //       name the interior based on the exterior entityID it should be ok.  We just need
                    //       for the exterior's ID to match the server's exterior ID.
                    
                    Keystone.IO.PagerBase.LoadTVResource(geometry); // force load of geometry so that our subsequenct Entity.BoundingBox call is correct.
                    string interiorID = addGeometry.EntryNewFilename;
                    interiorID = Path.ChangeExtension(interiorID, ".interior");
                    AddInterior((Keystone.Entities.Container)entity, interiorID, quadtreeDepth, entityDestinationPath);
                }

                // save the .KGBEntity  file first by writing the xml to a temp file, then adding that to the archive
                // at the mDestPathInArchive 
                // NOTE: the filename of the temp file is the final end part of the filename (without the directories) because
                // then our mFileNames array does not need to be used.
                string entityXMLPath = Keystone.IO.XMLDatabase.GetTempFileName();
                entityXMLPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), addGeometry.EntryNewFilename);

                if (System.IO.File.Exists(entityXMLPath))
                    System.IO.File.Delete(entityXMLPath);

                Keystone.IO.XMLDatabaseCompressed xmldb = new Keystone.IO.XMLDatabaseCompressed();
                xmldb.Create(Keystone.Scene.SceneType.Prefab, entityXMLPath, entity.TypeName);
                xmldb.WriteSychronous(entity, true, false, false);
                xmldb.SaveAllChanges();
                xmldb.Dispose();

                sourceFiles.Add(entityXMLPath);
                targetPaths.Add(entityDestinationPath);

                KeyCommon.IO.ArchiveIOHelper.AddFilesToMod(AppMain.MOD_PATH, 
										                        addGeometry.ModName, 
										                        sourceFiles.ToArray(), 
										                        targetPaths.ToArray(), null);


                cmd.WorkerProduct = entity;
                return state;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddGeometryToArchive.cs () - " + ex.Message);
                return state;
            }
        }

        /// <summary>
        /// Adds Geometry (does not "Import" which involves copying files to various paths) to an existing Scene connected Model.  
        /// This call is made from within the Plugin.  Usually only used during Edit mode.
        /// It uses the existing Entity and Model and only creates new DefaultAppearance, GroupAttributes and thne Geometry itself (eg Mesh3d, Actor3d, ParticleSystem).
        /// For Actors, it does require the Entity to be passed in so we can assign BonedAnimation[] clips
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private object Worker_Geometry_Add(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometry_Add addGeometry = (KeyCommon.Messages.Geometry_Add)cmd.Message;

            // we pass in the EntityID too because if it's an Actor, we need to load BonedAnimation[] clips which are stored in the Entity and not the Model
            ModeledEntity entity = (ModeledEntity)Repository.Get(addGeometry.EntityID);
            Model model = (Model)Repository.Get(addGeometry.ModelID);

            // removal of the existing appearance is done in the main thread in ProcessCommandCompleted
            // model.RemoveChild(model.Appearance);
            // todo: for Actors, we should probably remove any existing Animation from Entity in  ProcessCommandCompleted

            // TODO: this does not remove the item from the Repository after the obj is imported to the archive
            try
            {
                
                string ext = System.IO.Path.GetExtension(addGeometry.ResourcePath);
                bool isActor = ext.ToUpper() == Keystone.ImportLib.TVACTOR_EXTENSION;
                if (!isActor)
                    isActor = (addGeometry.LoadXFileAsActor && ext.ToUpper() == Keystone.ImportLib.XFILE_EXTENSION);


                //if (if (ext.ToUpper() == ImportLib.WAVEFRONTOBJ_EXTENSION && _openAsEditableMesh)
                //    //_resource = MeshEditable.Create(FilePath, FilePath, LoadTextures, LoadMaterials);
                //    _resource = Keystone.EditDataStructures.EditableMesh.Create(mPathInArchive, mPathInArchive, LoadTextures, LoadMaterials);


                Keystone.Animation.BonedAnimation[] clips;
                Keystone.Elements.Geometry geometry;
                Keystone.Appearance.DefaultAppearance appearance = null;
                Keystone.Animation.Animation animation = null;

                KeyCommon.IO.ResourceDescriptor resource = new KeyCommon.IO.ResourceDescriptor(addGeometry.ResourcePath);
                if (isActor) // TODO: if .x file and boned, we can't tell from just extension alone! Perhaps require ModelView to convert them all to TVA first.
                {
                    string id = Repository.GetNewName(typeof(Keystone.Animation.Animation));
                    animation = new Keystone.Animation.Animation(id);

                    geometry = Actor3d.Create(resource.ToString(), MTV3D65.CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, addGeometry.LoadTextures, addGeometry.LoadMaterials, out appearance);

                    // animation nodes from Actor3d ARE shareable as long as all duplicates have same
                    // animations with same names / indices
                    clips = ((Actor3d)geometry).GetAnimations();
                    // Keystone.Animation.AnimationController animSet = null;
                    //// Automatically add AnimationSet and intrinsic Animations if exists
                    //if (animations != null && animations.Length > 0)
                    //{
                    //    string animSetName = Repository.GetNewName (typeof (Keystone.Animation.AnimationController));
                    //    animSet = new Keystone.Animation.AnimationController(animSetName);

                    //    for (int i = 0; i < animations.Length; i++)
                    //        animSet.AddChild (animations[i]);
                    //}

                    //if (animSet != null)
                    //    entity.AddChild(animSet);

                    // NOTE: the above is when AnimationSet was a Group node.  Now it's AnimationController
                    // and is not a Node, just a helper object within Entities.  So
                    // animations now ALL get added directly to Entities
                    for (int i = 0; i < clips.Length; i++)
                    {
                        // BonedAnimations should still have a direct reference to the
                        // Actor3d it references... at least the friendly name so it can be restored
                        // when the Actor3d resource is added/paged in
                        clips[i].TargetName = model.Name;

                        animation.AddChild(clips[i]);
                    }
                    // entity.AddChild(animation); // TODO: this should be set in the workerproduct
                }
                else if (ext.ToUpper() == (".TVP"))
                {
                    geometry = (ParticleSystem)Repository.Create(resource.ToString(), "ParticleSystem");
                    // TODO: are we creating the duplicates properly if we dont call geometry.LoadTVResource() directly?
                    PagerBase.LoadTVResource(geometry, true);

                    // todo: we need to load any NiminMesh Emitters. Does particleSystem.LoadTVResource() do that?

                    ParticleSystem ps = (ParticleSystem)geometry;

                    appearance = ps.CreateAppearance();
                                       
                    PagerBase.LoadTVResource(appearance, true);
                }
                //else if (addGeometry.IsBillboard) // TODO
                //{
                //    geometry = (Billboard)Repository.Create(resource.ToString(), "Billboard");
                //}
                else // Mesh3d
                {
                    // TDO: this is bad. this call to Mesh3d.GetAppearance effectively means we are parsing and loading the mesh twice
                    appearance = Mesh3d.GetAppearance(resource.ToString()); // , addGeometry.LoadTextures, addGeometry.LoadMaterials, out appearance
                    geometry = (Mesh3d)Repository.Create(resource.ToString(), "Mesh3d");
                    PagerBase.LoadTVResource(geometry, true);
                }
                                

                Node[] products;
                if (geometry is Actor3d)
                    products = new Node[5];
                else products = new Node[4];

                products[0] = entity;
                products[1] = model;
                products[2] = appearance;
                products[3] = geometry;
                if (geometry is Actor3d)
                    products[4] = animation;

                cmd.WorkerProduct = products;

                return state;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FormMain.Commands.Worker_Geometry_Add() - " + ex.Message);
                return state;
            }
        }

        private object Worker_GeometryCreateGroup(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometry_CreateGroup create = (KeyCommon.Messages.Geometry_CreateGroup)cmd.Message;

            Model model = (Model)Repository.Get(create.ModelID);
            ParticleSystem ps = (ParticleSystem)Repository.Get(create.GeometryNodeID);
            if (ps == null) throw new ArgumentOutOfRangeException("Currently the only geometry that supports this is ParticleSystem.  Will add more as needed");
            System.Diagnostics.Debug.Assert(model.Geometry == ps);

            List<object> results = new List<object>();
            if (create.GroupClass == 0)
                results.Add(model);

            results.Add(ps);

            if (create.GroupClass == 0) // GroupClaas 0 == emitter
            {
                DefaultAppearance appearance = (DefaultAppearance)model.Appearance;
                if (appearance == null)
                {
                    appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");
                    results.Add(appearance);
                }

                int emitterCount = ps.EmitterCount;
                if (appearance.GroupAttributesCount != emitterCount) throw new Exception("GroupAttributes should exist 1:1 for every emitter.");

                // create a new attribute since in ProcessCommandCompleted() we will be adding a new Emitter
                GroupAttribute attrib = (GroupAttribute)Repository.Create("GroupAttribute");
                results.Add(attrib);

                if (!string.IsNullOrEmpty(create.MeshPath) && create.GroupType == 2) // this is a minimesh emitter
                {
                    MTV3D65.TVMesh tvmesh = _core.Scene.CreateMeshBuilder(create.GroupName);
                    string fullPath = System.IO.Path.Combine(AppMain.MOD_PATH, create.MeshPath);
                    tvmesh.LoadTVM(fullPath);
                    // mesh.LoadXFile();
                    //Keystone.Loaders.WavefrontObjLoader.ObjToTVM(obj)= new Keystone.Loaders.WaveFrontObj(path);

                    MTV3D65.TVMiniMesh tvmini = _core.Scene.CreateMiniMesh(128); // IMPORTANT: TV3D requires that the minimesh is created with at least as many meshes as will be used by max particlecount

                    tvmini.CreateFromMesh(tvmesh);
                    results.Add(tvmesh);
                    results.Add(tvmini);
                }
            }

            cmd.WorkerProduct = results.ToArray();
            return state;
        }

        private object Worker_GeometryRemoveGroup(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometry_RemoveGroup remove = (KeyCommon.Messages.Geometry_RemoveGroup)cmd.Message;
            ParticleSystem ps = (ParticleSystem)Repository.Get(remove.GeometryNodeID);
            if (ps == null) throw new ArgumentOutOfRangeException("Currently the only geometry that supports this is ParticleSystem.  Will add more as needed");

            Model model = (Model)Repository.Get(remove.ModelID);
            System.Diagnostics.Debug.Assert(model.Geometry == ps);

            List<Node> results = new List<Node>();
            if (remove.GroupClass == 0)
                results.Add(model);

            results.Add(ps);

            if (remove.GroupClass == 0)
            {
                DefaultAppearance appearance = (DefaultAppearance)model.Appearance;
                if (appearance == null)
                {
                    appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");
                    results.Add(appearance);
                }

                int emitterCount = ps.EmitterCount;
                if (appearance.GroupAttributesCount != emitterCount) throw new Exception("GroupAttributes should exist 1:1 for every emitter.");

                // create a new attribute since in ProcessCommandCompleted() we will be adding a new Emitter
                GroupAttribute attrib = (GroupAttribute)appearance.Children[remove.GroupIndex];
                results.Add(attrib);
            }

            cmd.WorkerProduct = results.ToArray();
            return state;
        }

        private object Worker_GeometryChangeGroupProperty(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometrty_ChangeGroupProperty change = (KeyCommon.Messages.Geometrty_ChangeGroupProperty)cmd.Message;
            ParticleSystem ps = (ParticleSystem)Repository.Get(change.GeometryNodeID);
            if (ps == null) throw new ArgumentOutOfRangeException("Currently the only geometry that supports this is ParticleSystem.  Will add more as needed");
            cmd.WorkerProduct = ps;
            return state;
        }

        private object Worker_Geometry_ResetTransform(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometry_ResetTransform reset = (KeyCommon.Messages.Geometry_ResetTransform)cmd.Message;

            Keystone.Elements.Geometry geometry = (Keystone.Elements.Geometry)Keystone.Resource.Repository.Get(reset.GeometryID);

            // note: actor3d does not support SetVertex so there is no way for us to modify the vert positions
            if (geometry is Keystone.Elements.Mesh3d)
            {
                Keystone.Elements.Mesh3d mesh = (Keystone.Elements.Mesh3d)geometry;
                mesh.ResetTransform(reset.Transform);

                string tempFile = Keystone.IO.XMLDatabase.GetTempFileName();
                mesh.SaveTVResource(tempFile);

                // TODO: July.25.2020 - if the mesh was an .obj and we have converted it to .tvm, we must update the Models.xml and Meshes.xml in the prefab!  
                //       If we don't do this and we open the prefab again, it will still point to the unreset .obj file instead of the resetted .tvm.
                //       For now, we're manually updating these files within the prefab with 7zip and XML editor.
                // TOOD: Can we accomplish this by removing the geometry child node and adding in a new one and then saving the entire prefab?
                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(reset.GeometryID);
                string filename = System.IO.Path.GetFileName(descriptor.EntryName);
                string path = System.IO.Path.GetDirectoryName(descriptor.EntryName);
                string extension = System.IO.Path.GetExtension(filename);
                if (extension.ToUpper() != ".TVM")
                    filename = System.IO.Path.GetFileNameWithoutExtension(filename) + ".tvm";

                KeyCommon.IO.ArchiveIOHelper.AddFilesToMod(_core.ModsPath,
                        descriptor.ModName,
                        new string[] { tempFile },
                        new string[] { path },
                        new string[] { filename });
            }
            return state;
        }
        // NOTE: This must only be allowed on Prefabs and not SavedEntities
        // when launching floorplan, user can iterate through list of found Containers.
        // if no interior exists (at any stage of completion) the user is presented a menu
        // item to generate interior.
        // 1) Upon clicking, they are given a dialog that yeilds some basic stats about the exterior mesh
        // and then asks 
        //   a) how many decks (and it will compute the deck height)
        //   b) the deck height and it will compute how many decks 
        //   -  allows selection of 2 - 6 meter deck height 
        //   - Textures will tile at 2 meter increments and scale inbeteen.
        //   - 6 meters allows for floorplans of buildings and space stations.  Especially warehouses for instance
        // that can be constructed of 2 floors each 6 meters high but with no ceiling for the first floor 
        // thus providing a continguous space and a place lots can be stored.
        // 2) Allows for importing of a new container by popping up asset dialog and allow selection
        //    of the exterior mesh?
        //    - or we need some otehr way to convert an existing Entity into one that is a Container.
        //    - we could do that via an option in the floorplan toolbar when selecting an Entity
        //    to view the floorplan of... to allow conversion to container and then generation of interior.
        // 3) I think the above is a good start and better than alternatives.  We can tweak from there
        //    as need be.
        private void AddInterior(Keystone.Entities.Container container, string interiorID, uint quadtreeDepth, string relativeDestinationPath)
        {

            // launch dialog to assist in generation of basic interior
            FormNewInterior newInterior = new FormNewInterior();

            System.Windows.Forms.DialogResult result = newInterior.ShowDialog();


            // validate floor height is in acceptable range

            // validate the Container entity's exterior mesh is 
            // of appropriate size for a floorplan.  If it's too small
            // it may only be allowed as a fighter/bomber

            // TODOO: 
            // TODO: the vehicle's exterior mesh is not loaded?  We're not getting proper container.BoundingBox values
            const uint OVERLAP = 2; // ensure there is 1 out of bounds cell on BOTH sides of the Interior floors.  No walls can be placed on the outer edge of these out of bounds cells.  TODO: we need to enforce that
            Vector3d cellSize = newInterior.CellSize;
            // verify the mesh is loading, then verify the Container object calculates it's boundingbox properly
            Keystone.Types.BoundingBox bounds = container.BoundingBox;
            uint cellsAcross = (uint)(bounds.Width / cellSize.x) + OVERLAP;
            uint cellsLayers = (uint)(bounds.Height / cellSize.y);
            if (cellsLayers == 0) cellsLayers = 1; // minimum of one for models with low ceilings

            uint cellsDeep = (uint)(bounds.Depth / cellSize.z) + OVERLAP;


            // TODO: "decks" can be labeled as spacing decks that can be used to hold
            // access tubes/ventilation shafts?
            // 1) Any half deck is built as a normal deck wwhere we specify it's height as smaller
            //    So that is how any crawlspace would be made as being a short height deck 
            // 2) shafts within walls would be constructed as sandwiched between walls.
            //    so you could create extra thick bulkheads and then have one part within the walls
            //    not be solid but hollow for a special access route should it be needed.
            //    There is no need to change how our decks are layed out otherwise.
            //    The only thing we need is a way to specify the height of the walls of the deck and
            //    the start (aka how thick the floor is)
            //
            // NOTE: from now on, creating a new vehicle consists of 
            // 1) selecting a normal modeled entity
            // 2) in plugin, click the floorplan tab and enable floorplan creation

            // 3) click Add Deck from the floorplan plugin tab and 
            //    generate floorplan
            //    where you will be prompted 
            //      a) for whether the first deck will be at lowest z or lowest y value
            //      b) for a height between -y and +y for each
            //    new deck you wish to add.  This y value will represent center height of that
            //    deck/floor
            // ?) When do we specify the thickness of the deck floor?
            // ?) How do we specify thickness of ceiling of final top deck?
            // 4) specify the height of the deck.  
            //      a) allow options for user to be assisted with coming up for values by
            //      making sure that an above floor starts at height of the below floor.
            //      b) ensure that decks are built from bottom floor up? or top down?
            // 
            // 5) internally we compute a cross section at the y height of the deck
            //    and use this polygon to determine which cells will be available for 
            //    plotting deck design 
            // 6) if the exterior geometry is removed, the entire deckplan will be destroyed as well.
            // 7) create a version of this code that can validate each deck based on cross sections
            //    after the fact so that submitted designs can be checked.


            Keystone.Elements.Mesh3d exteriorMesh = (Keystone.Elements.Mesh3d)container.Model.Geometry;

            int crc32 = 0; // TODO: we must get crc32 of the Mesh3d object
            // that does crc32 of all verts + scale of that mesh and model
            // and entity!  (actually model and entity scales must be 1,1,1)
            // we do not support scaling of the geometry for security reasons???


            // we only disable picking of exterior or rendering of exterior in the floorplan view
            // by setting options on the Context and it's pickparameters
            container.Pickable = true;
            container.Visible = true;

            /////////////////////////////////////////////////////////////////////////////
            // INTERIOR 
            /////////////////////////////////////////////////////////////////////////////
            // based on exterior mesh bounding volume and the cell size, compute cellsacross/layers/deep
            
            Keystone.Portals.Interior interior =
                new Keystone.Portals.Interior(interiorID, cellSize,
                            cellsAcross, cellsLayers, cellsDeep, quadtreeDepth);

            interior.SetProperty("datapath", typeof(string), relativeDestinationPath + "\\" + interiorID);

            // todo: script path should be customizable
            string scriptPath = @"caesar\scripts_entities\ship_interior.css";
            Keystone.Celestial.ProceduralHelper.MakeDomainObject(interior, scriptPath);

            // orientation of interior will always be origin with 0 rotation.  With "front" of exterior facing positive Z
            // this will have the effect of the lower ID cell indices having the highest Z values and the cells
            // at "back" of exterior vehicle having lowest Z values.  
            container.AddChild(interior);

            //  interior.CreateMask("boundaries", 0);
            //  interior.CreateMask("floors", 0);

            // add a default directional light.  Interior does not use the Zone's star light  - Dec.1.2022
            // todo: is the correct light being used?  also, the Interior light should be placed at rroot in QuadtreeCollection and not in a single Quadtree child node. 
            // todo: and how do reactors and shuttlecraft that take up multiple floors get placed in the QuadtreeCollection?
            float range = (float)(cellsAcross * cellSize.x);
            range = (float)Math.Max(range, cellsDeep * cellSize.z);
            Keystone.Lights.DirectionalLight light = Keystone.Celestial.LightsHelper.LoadDirectionalLight(range);
            Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(interior);
            setter.Apply(light);


            // obsolete for 1.0 - no more auto generated boundaries
            //// autogenerate all floors based on bounds of mesh and a fixed height of 2 meters (or 3?)
            //// TODO: maybe let's try to get our floorplan view based on an autogenerated
            //// interior of the yorktown...
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //Keystone.Types.Polygon[] crossSections = new Keystone.Types.Polygon[cellsLayers];
            //for (uint i = 0; i < cellsLayers; i++)
            //{
            //    double height = interior.GetFloorHeight(i);

            //    watch.Start();
            //    // NOTE: CreateCrossSection takes into account any scaling of the Model or Entity
            //    //       so that should be done to each crossSection[n]
            //    // TODO: its also taking into account rotation and that's wrong since we are conducting
            //    // the cross section tests at origin with 0 rotation
            //    Keystone.Types.Matrix transform = vehicle.Model.RegionMatrix;
            //    transform.M41 = transform.M42 = transform.M43 = 0.0; // remove translation from the matrix
            //    crossSections[i] = Keystone.Elements.Mesh3d.CreateCrossSection(exteriorMesh, (float)height, (float)cellSize.y);
            //    crossSections[i] = crossSections[i].Transform(transform);
            //    watch.Stop();
            //    // TODO: for each cross section, we need to test each tile in each celledregion
            //    // to see if that tile is INSIDE of the cross section and if so, set a flag indicating
            //    // it has a "floor" and thus allowing user to place interior components on it. by default
            //    // all tiles are out of bounds.
            //    int outOfBoundsCount = 0;
            //    for (uint j = 0; j < interior.CellCountX; j++)
            //        for (uint k = 0; k < interior.CellCountZ; k++)
            //        {

            //            Keystone.Types.Vector3d[] tileVertices = interior.GetTileVertices(j, i, k); // j, i, k is x,y,z order
            //            for (int n = 0; n < tileVertices.Length; n++)
            //                tileVertices[n].y += cellSize.y *.5;

            //            // is this tile entirely inside the bounds of the polygon?
            //            if (crossSections[i].ContainsPoints(tileVertices))
            //            // set all 16x16 footprint of this tile to FLOOR
            //            {
            //                uint tileStartX = j * 16;
            //                uint tileStartZ = k * 16;

            //                for (int x = 0; x < 16; x++)
            //                    for (int z = 0; z < 16; z++)
            //                    {
            //                        // mTileMask is y, x, z index order
            //                        interior.mTileMask[i, tileStartX + x, tileStartZ + z] |= 1 << 0; // TILEMASKFLAGS_FLOOR;
            //                    }
            //            }
            //            else
            //            {
            //                System.Diagnostics.Debug.WriteLine("Tile is out of cross section boundaries.");
            //                outOfBoundsCount++;
            //            }
            //        }

            //    System.Diagnostics.Debug.WriteLine(string.Format("{0} of {1} tiles out of bounds", outOfBoundsCount, interior.CellCountZ * interior.CellCountX));
            //    // TODO: then we should save this vehicle and verfy we can reload it because
            //    // this process is too slow to have to do everytime i want to test.
            //    System.Diagnostics.Debug.WriteLine(string.Format ("Cross Section {0} of {1} completed in {2} seconds.",i+1, cellsLayers, watch.Elapsed.TotalSeconds));
            //    watch.Reset();
            //}

            // IMPORTANT REMINDERS:
            // 1) Floor/Ceiling thickness is simply regulated by increasing the y height of a floor
            //    or ceiling tile.
            // 2) Vertical lift tubes or stairwell or "Jefferies tubes"  can be built manually out of a single
            //    tile that has walls around it to form a tube.  Doors/hatches can exist on each level
            //    and can contain either a ladder or an electric lift.  The idea is that these tubes
            //    can allow access through damaged parts of the ship where pressureization has failed
            //    while being protected by the pressurized tube itself.
            //    Sections of the tube can be sealed off with irises to act as airlocks.
            // 3) Horizontal ventilation shafts style tubes can be built similarly using a special
            //    type of floor tile or ceiling tile that is hollow.  Thus the tile contains flags
            //    that indicate it can be stepped on like a normal floor tile but also can contain
            //    a volume.
            // 4) The overall deck height is fixed for every single deck.  This compromise however
            //    will still allow us to replicate any type of starship interior.

            // 5) In this sims, you could simply see down through to any floor if there was no ceiling
            //   or floor tiles in the way and it would continue until you eventually did hit one.
            // 6) any area that has a full floor is considered a room that extends upwards until
            //   it meets a full ceiling.  If there is a NON door/access hole in the ceiling then that 
            //   space through the ceiling becomes apart of the room below it.
            //
            // I could convert a "CelledRegion" into a "CelledArea" and then enforce these are strictly
            // 2d width x depth but which because they match the width depth of anything above or below it
            // they can still compute traversal upstairs or downstairs
            // Doing this we can do two things
            // 1) we can save a bit of memory for decks that aren't all as wide as the widest deck
            // 2) we can more easily join subassemblies that have different altitudes and such
            //    so that subassemblies can allow for different sectors like in a huge babylon 5 station
            //


        }

        //        private void AddFloor(Keystone.Entities.Container vehicle, float height)
        //        {
        //            string interiorID = vehicle.ID + "_interor";
        //            Keystone.Portals.CelledRegion interior = (Keystone.Portals.CelledRegion)Repository.Get(interiorID);

        ////            // OBSOLETE - No need to create a cross section any longer.  We will manually determine
        ////            // in bounds and out of bounds interior tiles.
        ////            // NOTE: CreateCrossSection takes into account any scaling of the Model or Entity
        ////            //       so that should be done to each crossSection[n]
        ////            // TODO: its also taking into account rotation and that's wrong since we are conducting
        ////            // the cross section tests at origin with 0 rotation
        ////            Keystone.Types.Matrix transform = vehicle.Model.RegionMatrix;
        ////            transform.M41 = transform.M42 = transform.M43 = 0.0; // remove translation from the matrix
        ////            float stepSize = 1.0f;
        ////            Keystone.Types.Polygon crossSection = Keystone.Elements.Mesh3d.CreateCrossSection((Keystone.Elements.Mesh3d)vehicle.Model.Geometry, (float)height, stepSize);
        ////            crossSection = crossSection.Transform(transform);
        ////            // END OBSOLETE CROSS SECTION 

        //            // cross section above should be obsolete since we no longer care to automate
        //            // creation of inbounds/out of bounds tiles.  we will do it manually and we want
        //            // to be able to do it with a placement tool "bounds" brush

        //            // saving this vehicle in Morena_Full shoudl also be able to load the floor layers
        //            // when we assign decks to specific layer indices.  But all inbound/out of bounds should exist
        //            // for every possible layer even though only few decks will actually be created at runtime

        //            // is there a difference between a floor being designated to a layer
        //            // and assigning the inbounds/outofbounds of a floor?  Well yes.  A layer never
        //            // has to be a floor, but it must always have bounds flagged before floors can be added
        //            // because in the future, users will want to remodel and change locations of floors.
        //            // 
        //            // TODO: perhaps our "Interior" floors mesh is a single ModelSequence with a Model for each
        //            // floor rather than Floor entities being added.  So how about this.  How about prelimirily
        //            // when we "create floor" below, it adds a new grid to the Interior.ModelSequence
        //            Keystone.Entities.Entity floor = interior.CreateFloor(0);
        //        }


    // TODO: this does not work with folder based file system
    private object Worker_ArchiveDeleteFile(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Archive_DeleteEntry delFiles = (KeyCommon.Messages.Archive_DeleteEntry)cmd.Message;

            if (delFiles.ZipEntries == null || delFiles.ZipEntries.Length == 0) return state;

            Ionic.Zip.ZipFile zip = null;
            try
            {
                string zipFullPath = System.IO.Path.Combine(_core.ModsPath, delFiles.RelativeZipFilePath);
                zip = new Ionic.Zip.ZipFile(zipFullPath);

                DateTime timeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                        

                bool isFolder = false; // bool.Parse((string)item.Tag);

                for (int i = 0; i < delFiles.ZipEntries.Length; i++)
                {
                    if (KeyCommon.IO.ArchiveIOHelper.IsFolder(delFiles.ZipEntries[i]))
                    {
                        string[] files = KeyCommon.IO.ArchiveIOHelper.SelectEntryNamesRecursively(zip, delFiles.ZipEntries[i]);
                        for (int j = 0; j < files.Length; j++)
                            zip.RemoveEntry(files[j]);

                        // NOTE: SelectEntriesRecursively DOES include the starting folder so no need to delete it seperately
                    }
                    else
                    {
                        zip.RemoveEntry(delFiles.ZipEntries[i]);
                    }
                }
                   
                zip.Save();
                Trace.WriteLine(string.Format("Worker_ArchiveDeleteFile() -- Successfully deleted {0} from archive '{1}'.", delFiles.ZipEntries[0], delFiles.RelativeZipFilePath));
                return state;
            }
            catch (Exception ex)
            {
                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
                Trace.WriteLine(string.Format("Worker_ArchiveDeleteFile() -- Error removing files from archive '{0}'. {1}.", delFiles.RelativeZipFilePath, ex.Message));
                return state;
            }
            finally
            {
                if (zip != null) zip.Dispose();
            }
        }
        #endregion

        private object Worker_Geometry_Save(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Geometry_Save saveGeometry = (KeyCommon.Messages.Geometry_Save)cmd.Message;

            string savePath = Path.Combine(AppMain.MOD_PATH, saveGeometry.NewRelativePath);

            Geometry geometry = (Geometry)Repository.Get(saveGeometry.CurrentRelativePath); // RelativeResourcePath is the same as the NodeID
            geometry.SaveTVResource(savePath);

            return state;
        }

        private object Worker_PrefabSave(object state)
        {
            // maybe all of this then eventually goes through our various API System? 
            // that modders have access to for scripting?
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Prefab_Save savePrefab = (KeyCommon.Messages.Prefab_Save)cmd.Message;

            string savePath = Keystone.IO.XMLDatabase.GetTempFileName();

            string path = Path.Combine(AppMain.MOD_PATH, savePrefab.EntryPath);
                
            if (path.ToLower().EndsWith (".zip") == false)
            {
                savePath = path; // Path.Combine (path, savePrefab.EntryPath);
            	//savePath = Path.Combine (savePath, savePrefab.EntryName);
            }

            Keystone.Elements.Node target = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(savePrefab.NodeID);


            if (target != null)
            {
                // create a new XMLDB to store this prefab
                Keystone.IO.XMLDatabaseCompressed xmldb = new Keystone.IO.XMLDatabaseCompressed();
                xmldb.Create(Keystone.Scene.SceneType.Prefab, savePath, target.TypeName);

                // modify the "ID" value to match the archive relative path + entry name so that when this prefab is ref'd
                // in a scene XMLD by an entity instance, the scene reader will know where to find the prefab
                // so it can be loaded.  This does mean that only saved prefabs can be ref'd by other instances.
                string entryPath = System.IO.Path.Combine (savePrefab.EntryPath, savePrefab.EntryName);
                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(savePrefab.EntryPath);
                // upon saving, we must now make the live instance into the clone 
                // because we do not wish for the live instance to have it's server generated ID modified.
                // So instead the clone becomes our prefab and is assigned as SRC to the live instance 
                bool delayResourceLoading = true;
                // todo: i think neverShare = true should never be used.  if a node is shareable and IPageable than the id and the resourcepath are the same thing
                //       and thus must be shared.
                bool recurse = true;
                bool neverShare = false;
                Node prefab = target.Clone(descriptor.ToString(), recurse, neverShare, delayResourceLoading);
                target.SRC = descriptor.ToString();
                prefab.SRC = null;  // prefab root node must not have SRC set or we'll get circular errors 
                                    // which really blows up if we are overwiting to our prevous resourceDesriptor location!

                if (prefab is Container)
                {
                    // save the interior database to same path the prefab resides
                    Interior interior = ((Container)prefab).Interior as Interior;
                    // todo: test re-saving of a re-saved prefab and whether it finds the correct datapath
                    // todo: test loading of campaign with new default vehicle
                    string existingPath = (string)((Container)target).Interior.GetProperty("datapath", false).DefaultValue;
                    string existingFullPath = Path.Combine(AppMain.MOD_PATH, existingPath); // todo: this should be MOD_PATH right?

                    string relativeInteriorDBPath = savePrefab.EntryPath;
                    relativeInteriorDBPath = System.IO.Path.ChangeExtension(relativeInteriorDBPath, ".interior");

                    Settings.PropertySpec dbPathSpec = new Settings.PropertySpec("datapath", typeof(string).Name, (object)relativeInteriorDBPath); // note: its important to cast relativeInteriorDBPath string as an object so the proper constructor gets called.
                    interior.SetProperties(new Settings.PropertySpec[] { dbPathSpec });


                    string fullInteriorDBPath = AppMain.MOD_PATH;
                    fullInteriorDBPath = Path.Combine (fullInteriorDBPath, relativeInteriorDBPath);
                    if (File.Exists(fullInteriorDBPath))
                    {
                        File.Delete(fullInteriorDBPath);
                    }

                    // NOTE: The .interior file may not have been created yet so if its not there, don't copy it.
                    if (File.Exists(existingFullPath))
                        System.IO.File.Copy(existingFullPath, fullInteriorDBPath);
                    else
                        ((Container)target).Interior.SaveTVResource(fullInteriorDBPath);


                }
                // TODO: how do we ensure that the prefab gets unloaded properly when we shut down the scene?
                //       if it is added to a special Prefab list in the Repository then that's easy, otherwise
                //       we dont know what in the list is prefab and what is not.
                // TODO: is there a way to save meshes required by this prefab here?
                //       and then, doesnt that mean the resource paths need to be modified
                //       to point to this prefab.. ugh.  
                // I think not.  prefab only has the .xml for the entity and resources
                // must relate back to meshes stored on disk such as in mod folder \\caesar\\meshes\\test.tvm
                // So somewhere during save, we must be allowed to pick save of the interior
                // meshes? or we auto do it based on where the prefab is saved? or are these meshes
                // saved hierarchically within the interior region.xml within the vehicle prefab archive?
                // TODO: i need to test the above, if i save a prefab vehicle with interior and add a component
                // to the interior, will that component be saved to the interior region xml within the prefab?
                // TODO: and when deserializing, do the components update the footprint grid?  I think they should because
                //       they are going through celledRegion.RegisterEntity? or are they?
                xmldb.WriteSychronous(prefab, true, false, false); // we dont incrementdecrement because unlike generating a scene or universe, the target node already exists in our scene and we want to keep it
                xmldb.SaveAllChanges();
                xmldb.Dispose();

                System.Diagnostics.Debug.WriteLine("FormMain.Commands() - Worker_PrefabSave() - Prefab database created.");

                // if using zip archive mod, must still add .kgbentity to the archive mod db. 
                if (path.EndsWith (".zip"))
                {
	                KeyCommon.IO.ArchiveIOHelper.AddFilesToMod(_core.ModsPath, 
	                        savePrefab.ModName, 
	                        new string[] { savePath },
	                    	new string[] { savePrefab.EntryPath },
	                    	new string[] { savePrefab.EntryName });
                }
                
                System.Diagnostics.Debug.WriteLine("FormMain.Commands() - Worker_PrefabSave() - Prefab database added to mod database.");
            }

            return state;
        }

        private object Worker_NodeCreate(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            Keystone.Messages.Node_Create create = (Keystone.Messages.Node_Create)cmd.Message;


            // server is telling client to create a node and any children it may have
            if (create.NodeCount > 0)
                for (int i = 0; i < create.NodeCount; i++)
                {
                    // NOTE: in loopback the server has already created the node in the repository
                    // Fortunately our Factory.Create checks that the node exists and will return 
                    // existing if it does.
                    Node node = Keystone.Resource.Repository.Create(create.DescendantNodeIDs[i], create.DescendantNodeTypes[i]);
                    // NOTE: we're not assigning Custom Properties to Entities here.  Is this ok?
                    node.SetProperties(create.NodeProperties[i].Properties.ToArray());

                    string parent = create.DescendantNodeParents[i];
                    Node parentNode = (Node)Keystone.Resource.Repository.Get(parent);
                    if (!(parentNode is Keystone.Elements.IGroup))
                        throw new Exception("FormMain.Commands.Worker_NodeCreate() - Command is invalid, i should test validity first.  This usually occurs when the object in plugin is not the same as the one you think it is.");
                    // TODO: MAKE SURE YOU ARE DRAGGING ONTO GEOMETRY SWITCH OR LOD
                    // TODO: need try/catch to invalidate and continue, not error and break

                    // TODO: is it always ok to call LoadTVResourceSynchronously() here?  do we never need child nodes added first?  AND this must happen before
                    // adding to parent because shader require  they are loaded before being added to appearance
                    PagerBase.LoadTVResource(node, true);

                    if (i == 0)
                    {
                        // NOTE: We do not assign the top most node to a parent.  That is done in
                        // ProcessCompletedCommandQueue() because adding to actual scene must always be done on main thread
                        cmd.WorkerProduct = node;
                    }
                    else
                    {
                        Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parentNode);
                        // TODO: should only assign to parent that is already in Scene in our single threaded CommandCompleted method 
                        setter.Apply(node);
                    }
                    if (node is Keystone.Vehicles.Vehicle)
                    {
                        Debug.WriteLine("FormMain.Commands.Worker_NodeCreate() - Creating Vehicle '" + node.ID + "'.");
                    }

                    // obsolete: Feb.9.2013 - descendant ids are now stored in an array just like 
                    // descendant types and parents. 
                    //int idIndex = create.NodeProperties[i].Properties.IndexOf("id");
                    //if (idIndex >= 0)
                    //{
                    //    string id = (string)create.NodeProperties[i].Properties[idIndex].DefaultValue;
                    //    Node n = Keystone.Resource.Repository.Create(id, create.DescendantNodeTypes[i]);
                    //    n.SetProperties(create.NodeProperties[i].Properties.ToArray());                    

                    //    // TODO: in loopback the server has already created the node in the repository
                    //    // Fortunately our Factory.Create checks that the node exists and will return 
                    //    // existing if it does.
                    //    string parent = create.DescendantNodeParents[i];
                    //    Node parentNode = (Node)Keystone.Resource.Repository.Get(parent);
                    //    if (!(parentNode is Keystone.Elements.IGroup))
                    //        throw new Exception("Command is invalid, i should test validity first.  This usually occurs when the object in plugin is not the same as the one you think it is.");
                    //    // TODO: MAKE SURE YOU ARE DRAGGING ONTO GEOMETRYSWITCH OR LOD
                    //    // TODO: need try/catch to invalidate and continue, not error and break
                    //    // TODO: I'm pretty sure the above setter can in loopback enviornment
                    //    //       wind up trying to add children to nodes they are alread children of
                    //    //       - this for loop iterate through a flat list of nodes which are potentially
                    //    //       hierarchical in nature.  we only serialize them in a flat list.  
                    //    //  TODO: surely the way we setter.Apply(n) here should take into account if a given
                    //    // child already exists under a given parent and then maybe skip it and then continue
                    //    // to iterate?
                    //    Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parentNode);
                    //    setter.Apply(n);

                    //    // - TODO: we still need to verify that touching the scene here with SuperSetter addchild
                    //    // is what we want... that this is threadsafe and this is where we want to iterate 
                    //    // through all received messages and update the scenegraph all at once rather than
                    //    // interupt willy nilly whenever a message arrives.
                    //    //  Ithink our Completion function is where we want to do that
                    //}
                }

            if (create.IsUserVehicle)
            {
                AppMain.mPlayerControlledEntityID = create.DescendantNodeIDs[0];
            }
            return state;
        }

        /// <summary>
        /// Spawn specifically from a file in the SAVES_PATH
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private object Worker_Spawn(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Simulation_Spawn spawn = (KeyCommon.Messages.Simulation_Spawn)cmd.Message;

            // todo: don't we need to know if this a \\saves\\ or \\modname\\   This command shouldn't be named Prefab_Insert, but 
            // more general and able to add prefabs using server generatedIDs or saved kgbentities using the IDs inside the saved .kgbentities
            string fullPath = System.IO.Path.Combine(AppMain.MOD_PATH, spawn.EntitySaveRelativePath);
            //todo: the bools needs to be set in the Simulation_Spawn command. Or maybe not... these are client settings.
            bool generateIDs = false;
            
            // todo: this needs to use the IDs generated by the server!
            Entity entity = LoadEntity(fullPath, spawn.EntitySaveRelativePath, generateIDs, true, false, spawn.NodeIDsToUse, spawn.Translation);
            entity.SRC = null; // spawn command only occurs during ArcadeEnabled =true, so SRC must be null.  If it's not null, when we save the scene and try to reload it later, it will fail with infinite loop

            // TODO: Simulation_Spawn and Prefab_Load are basically identicle.  We only need one.
            cmd.WorkerProduct = entity;
            return state;
        }

        // unlike Node_Create this specifically loads a KGBENTITY from a mod db
        // and so no Properties[] spec is supplied.
        private object Worker_Prefab_Insert(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Prefab_Load addPrefab = (KeyCommon.Messages.Prefab_Load)cmd.Message;

            bool load = true;
            string[] nodeIDsToUse = addPrefab.NodeIDsToUse;

            if (addPrefab.IsFragmented)
            {
                load = false;
                CommandFragmentState fragmentState;

                if (addPrefab.Index > 0)
                {
                    bool success = mFragmentedCommands.TryGetValue(addPrefab.Guid, out fragmentState);

                    if (success)
                    {
                        // merge the data to the existing fragmentState
                        fragmentState.Command.MergeFragment(addPrefab);
                        fragmentState.ElementsReceived += addPrefab.NodeIDsToUse.Length;
                        mFragmentedCommands[addPrefab.Guid] = fragmentState; // CommandFragmentState is a struct not a reference type so we need to assign the updated value

                        if (fragmentState.ElementsReceived == addPrefab.Length)
                        {
                            // we are done.  load the Entity
                            nodeIDsToUse = ((KeyCommon.Messages.Prefab_Load)fragmentState.Command).NodeIDsToUse;
                            load = true;
                        }
                    }
                    else
                        throw new Exception("FormMain.Commands.Worker_Prefab_Insert() - Cannot find fragmentstate");
                }
                else
                {
                    fragmentState.Command = addPrefab;
                    fragmentState.ElementsReceived = nodeIDsToUse.Length;
                    mFragmentedCommands.Add(addPrefab.Guid, fragmentState);
                }
            }


            if (load)
            {
                // todo: don't we need to know if this a \\saves\\ or \\modname\\   This command shouldn't be named Prefab_Insert, but 
                // more general and able to add prefabs using server generatedIDs or saved kgbentities using the IDs inside the saved .kgbentities
                string fullPath = System.IO.Path.Combine(AppMain.MOD_PATH, addPrefab.RelativeArchivePath);
                fullPath = System.IO.Path.Combine(fullPath, addPrefab.EntryPath);

                Node prefab = null;
                if (fullPath.ToLower().EndsWith(".kgbbehavior"))
                {
                    prefab = Keystone.ImportLib.Load(fullPath, false, true, true, nodeIDsToUse);
                }
                else
                    // NOTE: Since LoadEntity() is already being called in this worker thread, typically it's ok to NOT delay resource loading
                    // however, this will cause a failure to load .interior database for Containers\Vehicle Interiors because we will not have
                    // yet had a chance to change the Interior's "datapath" to point to the correct location.
                    prefab = LoadEntity(fullPath, addPrefab.RelativeArchivePath, false, addPrefab.Recurse, addPrefab.DelayResourceLoading, nodeIDsToUse, addPrefab.Position);

#if DEBUG
                System.Collections.Generic.Queue<string> validate = new System.Collections.Generic.Queue<string>();
                GetNonShareableNodeIDs(prefab, ref validate);
                string[] comparison = validate.ToArray();
                for (int i = 0; i < nodeIDsToUse.Length; i++)
                    if (nodeIDsToUse[i] != comparison[i]) throw new Exception();
#endif 

                // TODO: if this cmd.IsUserVehicle = true, we need to setup the main 3dWorkspace and Viewpoint <-- I think this is obsolete
                //addPrefab.Cancel = cancel;
                // NOTE: assigning WorkerProduct in this "if (load)" block ensures we do not assign WorkerProduct if the fragmented command is not fully assembled
                cmd.WorkerProduct = prefab;
            }

            return state;
        }

        private object Worker_Prefab_Insert_Interior(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Prefab_Insert_Into_Interior insertIntoCell = (KeyCommon.Messages.Prefab_Insert_Into_Interior)cmd.Message;
            Keystone.Portals.Interior interior = (Interior)Repository.Get(insertIntoCell.ParentID);

            Keystone.Entities.Entity parentEntity = (Keystone.Entities.Entity)Repository.Get(insertIntoCell.ParentID);

            Entity product;
            bool cancel = false;

            // todo: don't we need to know if this a \\saves\\ or \\modname\\   This command shouldn't be named Prefab_Insert, but 
            // more general and able to add prefabs using server generatedIDs or saved kgbentities using the IDs inside the saved .kgbentities
            string fullPath = System.IO.Path.Combine(AppMain.MOD_PATH, insertIntoCell.ModName);
            fullPath = Path.Combine(fullPath, insertIntoCell.EntryPath);
            // NOTE: clone entities arg == true in Load() call
            // This partricular xmldatabase has only one model or one entity in it so name and parent name can be ""
            Keystone.Entities.ModeledEntity child = (ModeledEntity)Keystone.ImportLib.Load(fullPath, true, true, false, insertIntoCell.NodeIDsToUse);
            // load any entity script now during this worker thread
            Keystone.IO.PagerBase.LoadTVResource(child, true);

            // set the prefab link.  Only caller of clone/deserialize/readsychrnous should
            // assign prefab links because sometimes when we clone or deserialize we don't want to
            if (AppMain._core.ArcadeEnabled)
                child.SRC = null;
            else
                child.SRC = new KeyCommon.IO.ResourceDescriptor(insertIntoCell.ModName, insertIntoCell.EntryPath).ToString();

            // make sure default child transform is identity
            child.Translation = Vector3d.Zero();
            child.Rotation = new Quaternion();
            // child.Scale we allow to be cloned.

            // TODO: I think interior.IsChildPlaceableWithinInterior() validation should occur in loopback not here in client processing.
            // NOTE: Vector3i tileLocation and startTileLocation are internal to keystone.dll and not accessible here. We only pass in Vector3d position.
            int[,] destFootprint = null;
            bool isPlaceable = interior.IsChildPlaceableWithinInterior(child, insertIntoCell.Position, insertIntoCell.Rotation, out destFootprint);
            if (isPlaceable == false)
                cancel = true; // todo: if cancel is true we should increment and decrement the child right?

            // TODO: also some Components need to be restricted to _only_ being EntityPlacer.cs 
            // allowed to be placed inside CelledRegion interiors and not outside of ships.
            child.Translation = insertIntoCell.Position;
            child.LatestStepTranslation = child.Translation;
            child.Rotation = insertIntoCell.Rotation;

            product = child;

            insertIntoCell.Cancel = cancel;
            cmd.WorkerProduct = product;
            return state;
        }

        private object Worker_Prefab_Insert_Into_Structure(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Prefab_Insert_Into_Structure insertIntoStructure = (KeyCommon.Messages.Prefab_Insert_Into_Structure)cmd.Message;

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(insertIntoStructure.RelativeArchivePath, insertIntoStructure.EntryPath);

            // todo: don't we need to know if this a \\saves\\ or \\modname\\   This command shouldn't be named Prefab_Insert, but 
            // more general and able to add prefabs using server generatedIDs or saved kgbentities using the IDs inside the saved .kgbentities
            string fullPath = Path.Combine(AppMain.MOD_PATH, insertIntoStructure.RelativeArchivePath);
            fullPath = Path.Combine(fullPath, insertIntoStructure.EntryPath);

            Keystone.Entities.Entity parentEntity = (Keystone.Entities.Entity)Repository.Get(insertIntoStructure.ParentStructureID);

            Entity[] products = new Entity[insertIntoStructure.Positions.Length];
            bool[] cancel = new bool[insertIntoStructure.Positions.Length];

            // inserts the prefab multiple times in different positions and rotations.  This is why once we've read the prefab, we can just clone it using the specified IDs
            for (int i = 0; i < insertIntoStructure.Positions.Length; i++)
            {
                // clones a new entity each time
                // NOTE: clone entities arg == true in ReadSychronous() call
                // This partricular xmldatabase has only one model or one entity in it so name and parent name can be ""
                Keystone.Entities.ModeledEntity child = (ModeledEntity)Keystone.ImportLib.Load(fullPath, true, true, false, insertIntoStructure.NodeIDsToUse); // TODO: We need seperate NodeIDsToUse for each instance being inserted into the Structure
                // load any entity script now during this worker thread
                Keystone.IO.PagerBase.LoadTVResource(child, true);

                // set the prefab link.  Only caller of clone/deserialize/readsychrnous should
                // assign prefab links because sometimes when we clone or deserialize we don't want to
                child.SRC = descriptor.ToString();

                // make sure default child transform is identity
                child.Translation = Vector3d.Zero();
                child.Rotation = new Quaternion();
                // child.Scale we allow to be cloned.

                // TODO: is this resulting in a footprint that is just empty because we havent loaded fully yet?
                //       it shouldn't be though...
                bool isPlaceable = true; // structure.IsChildPlaceableWithinStructure(child, insertIntoCell.Positions[i], insertIntoCell.Rotations[i]);
                if (isPlaceable == false)
                    cancel[i] = true; // TODO: should we cancel entire operation if just one of these isPlaceale fails?


                // TODO: also some Components need to be restricted to _only_ being EntityPlacer.cs 
                // allowed to be placed inside CelledRegion interiors and not outside of ships.
                child.Translation = insertIntoStructure.Positions[i];
                child.LatestStepTranslation = child.Translation;
                child.Rotation = insertIntoStructure.Rotations[i];

                products[i] = child;
            }

            insertIntoStructure.Cancel = cancel;
            cmd.WorkerProduct = products;
            return state;
        }

        private object Worker_Prefab_Insert_EdgeSegment(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Place_Entity_In_EdgeSegment placeIntoSegment = (KeyCommon.Messages.Place_Entity_In_EdgeSegment)cmd.Message;

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(placeIntoSegment.RelativeArchivePath, placeIntoSegment.EntryPath);
            string fullPath = Path.Combine(AppMain.MOD_PATH, placeIntoSegment.RelativeArchivePath);
            fullPath = Path.Combine(fullPath, placeIntoSegment.EntryPath);
            Keystone.Portals.Interior interior = (Keystone.Portals.Interior)Repository.Get(placeIntoSegment.ParentStructureID);

            // right now we only support one entity being placed into edge segment at a time
            Entity[] products = new Entity[1];
            bool[] cancel = new bool[1];

            // NOTE: clone entities arg == true in ReadSychronous() call
            Keystone.Entities.Entity child = (Entity)Keystone.ImportLib.Load(fullPath, true, true, true, placeIntoSegment.NodeIDsToUse);

            // load any script now during this worker thread
            Keystone.IO.PagerBase.LoadTVResource(child, true);

            // set the prefab link.  Only caller of clone/deserialize/readsychrnous should
            // assign prefab links because sometimes when we clone or deserialize we don't want to
            child.SRC = descriptor.ToString();

            // TODO: a door has other reqts to determine IsChildPlaceable how can we run
            //       that script?
            //       e.g child.Execute ("IsPlaceable", args);
            Vector3d position;
            Vector3i tileLocation;
            bool isPlaceable = interior.IsEdgeComponentPlaceable(child.Footprint.Data, placeIntoSegment.EdgeID, out position, out tileLocation);
            if (isPlaceable == false)
                cancel[0] = true;

            child.Translation = position;
            child.Rotation = placeIntoSegment.Rotation;

            products[0] = child;


            placeIntoSegment.Cancel = cancel;
            cmd.WorkerProduct = products;

            return state;
        }

        private object Worker_Terrain_Paint(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Terrain_Paint paintTerrain = (KeyCommon.Messages.Terrain_Paint)cmd.Message;

            Keystone.Elements.Terrain terrain = (Keystone.Elements.Terrain)Repository.Get(paintTerrain.TerrainID);

            if (paintTerrain.Vertices != null)
                for (int i = 0; i < paintTerrain.Vertices.Length; i++)
                    terrain.SetHeight((float)paintTerrain.Vertices[i].x, (float)paintTerrain.Vertices[i].z, (float)paintTerrain.Vertices[i].y);


            //terrain.FlushHeightChanges();

            return state;
        }

        private object Worker_TileMapStructure_Paint(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.TileMapStructure_PaintCell paintCell = (KeyCommon.Messages.TileMapStructure_PaintCell)cmd.Message;

            //Keystone.Portals.CelledRegion celledRegion = (Keystone.Portals.CelledRegion)Repository.Get(paintCell.ParentCelledRegionID);

            // TODO: is there a way to lock cells that we are looking to modify
            //       so that another thread can't attempt the same thing before we've
            //       had a chance to update those cells?
            //       TODO: or i could lock the entire cellmap...  i think that's probably our best
            //       bet


            if (paintCell.Indices == null || paintCell.Indices.Length <= 0)
                return state;

            // verify all indices are unique
            for (int i = 0; i < paintCell.Indices.Length; i++)
                for (int j = 0; j < paintCell.Indices.Length; j++)
                {
                    if (j == i) continue;
                    if (paintCell.Indices[i] == paintCell.Indices[j])
                        return state;
                }

            bool[] cancel = new bool[paintCell.Indices.Length];

            object value = paintCell.PaintValue;
            //paintCell.WorkerResults = new MinimeshMap[paintCell.Indices.Length];
            cmd.WorkerProduct = new MinimeshMap[paintCell.Indices.Length];

            for (int i = 0; i < paintCell.Indices.Length; i++)
            {
                uint index = paintCell.Indices[i];
                switch (paintCell.LayerName)
                {
                    case "obstacles":
                        throw new NotImplementedException();
                        break;

                    case "layout": // TODO: rename "structure_layout"
                        cancel[i] = false;
                        if (i == 0)
                        {
                            Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)Repository.Get(paintCell.ParentStructureID);
                            string segmentPath = (string)paintCell.PaintValue;

                            // TODO: where do we define other aspects of this segment?  It should be in the segment entity itself in ReferencedSegments?
                            //	     - it's footprint/traversal cost,  

                            // while in worker thread, load this segment, create InstancedGeometry representation and add it to ReferencedSegments and mSegmentLookupPath
                            structure.AddLookupSegment(segmentPath);
                        }
                        break;

                    default:
                        {
                            System.Diagnostics.Debug.WriteLine("FormMainCommands.Worker_CelledRegion_Paint() - Unexpected layer name " + paintCell.LayerName);
                            break;
                        }
                }

            }

            paintCell.Cancel = cancel;

            // TODO: here we do NOT actually modify scene... we verify
            //       placement is valid and load load any geometry and scripts
            //       and since we dont modify the scene, there's nothing to save.
            //       modification occurs in CommandCompleted and then save should be
            //       sent to pager where all saves of celledregion are serialized to a single
            //       background thread for i/o
            //       TODO: hte main problem right now i think is validating placement if threaded
            //             can result in two or more items being validated but then one or more
            //             becoming invalid after the commandcomplete does actual placement
            //         we could make a temporary footprint that we modify and then do all tests
            //         against and modify it along the way, then we are done we update the entire
            //         original at once.  
            //         - if we do this, then we also need to prevent other calls to cell paint from
            //         occuring until this one is done.
            //         - we also need to make sure that a command such as this if for some reason 
            //         using UDP is always sequenced and never allowed to occur out of order. 
            //         - we must also ensure that a short subsequent operation done in thread and
            //         finishes before the first cannot be executed as long as the outstanding 
            //         operation hasnt completed.
            //       

            return state;
        }

        /// <summary>
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <remarks>
        /// Validation of painting occurs here because validation of COMMANDS
        /// always occurs in our Command Processor.
        /// </remarks>
        private object Worker_CelledRegion_Paint(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.PaintCellOperation paintCell = (KeyCommon.Messages.PaintCellOperation)cmd.Message;

            //Keystone.Portals.CelledRegion celledRegion = (Keystone.Portals.CelledRegion)Repository.Get(paintCell.ParentCelledRegionID);

            // TODO: is there a way to lock cells that we are looking to modify
            //       so that another thread can't attempt the same thing before we've
            //       had a chance to update those cells?
            //       TODO: or i could lock the entire cellmap...  i think that's probably our best
            //       bet


            if (paintCell.Indices == null || paintCell.Indices.Length <= 0)
                return state;

            // verify all indices are unique
            for (int i = 0; i < paintCell.Indices.Length; i++)
                for (int j = 0; j < paintCell.Indices.Length; j++)
                {
                    if (j == i) continue;
                    if (paintCell.Indices[i] == paintCell.Indices[j])
                        return state;
                }

            bool[] cancel = new bool[paintCell.Indices.Length];

            object value = paintCell.PaintValue;
            EdgeStyle[] styles = new EdgeStyle[paintCell.Indices.Length];

            Keystone.Portals.Interior interior = (Keystone.Portals.Interior)Repository.Get(paintCell.ParentCelledRegionID);

            for (int i = 0; i < paintCell.Indices.Length; i++)
            {
                uint index = paintCell.Indices[i];
                switch (paintCell.LayerName)
                {
                    case "obstacles":
                        throw new NotImplementedException();
                        break;

                    case "layout": // TODO: rename "structure_layout"
                        cancel[i] = false;
                        if (i == 0)
                        {

                            string segmentPath = (string)paintCell.PaintValue;

                            // TODO: where do we define other aspects of this segment?  It should be in the segment entity itself in ReferencedSegments?
                            //	     - it's footprint/traversal cost,  

                            // while in worker thread, load this segment, create InstancedGeometry representation and add it to ReferencedSegments and mSegmentLookupPath
                            // celledRegion.AddLookupSegment (segmentPath);
                        }
                        break;
                    case "boundaries":
                        break;
                    case "footprint":
                        // verify tile is in designated bounds which is not same as cell being in interior bound
                        cancel[i] = false; // TODO: temp false until i get this working !(bool)celledRegion.Layer_GetValue("footprint", index);
                        break;
                    case "powerlink":
                        // verify tile is in designated bounds which is not same as simply being in the interior bounds
                        cancel[i] = false; // TODO: temp false until i get this working !(bool)celledRegion.Layer_GetValue("footprint", index);
                        break;
                    case "tile style":
                        // we only allow cells to have texture if they are in bounds.  If it is inbounds, then
                        // this cell will NOT be null in the mCells array.  We can check by getting value of "boundaries" which
                        // will return TRUE if inbounds.
                        cancel[i] = !(bool)interior.Layer_GetValue("boundaries", index);
                        break;

                    case "wall style":
                        {
                            // Validation code mostly and a call to preload necessary models but to 
                            // not apply them to the scene yet
                            EdgeStyle style = (EdgeStyle)value;
                            // NOTE: even with style == -1, if IsSegmentPlaceable == false, then
                            // it's ok to cancel because it's out of bounds and there is no existing wall there to delete.

                            // NOTE: here in the Worker thread, we page in the Meshes / Minimeshes but do not yet add
                            //       to the selected Edge.  Adding occurs in the Interior.cs script which calls EntityAPI.CellMap_SetEdgeSegmentStyle()
                            // TODO: style here should contain link to the Wall prefab?
                            // TODO: should style even be necessary?  For 1.0 will only allow 
                            //       one type of wall style throughout the entire ship?
                            //       Does this impact things like railings and fences that we might want to use?
                            //       And what if we want some interior walls (they can still be single mesh walls) to have different
                            //       walls compared to an exterior wall?
                            //       So right now, yes or no... do we allow double sided walls?  We could still have the corner pieces
                            //       be of a single fixed type.  Afterall CORNER PIECES touching two different wall styles on either end
                            //       which CORNER PIECE would we use?  Using just a single structural corner mesh is just easier.
                            //       Railings and Fences straddling edges taking up tiles along both side of the edge fits with
                            //       using just a single thick wall as opposed to double-sided.
                            //       Or what if, we do allow changes in color of the walls only... never changes to Prefab used.
                            bool loaded = false;
                            if (style.StyleID != -1)
                            {
                                // NOTE: we only pass in the index (edgeID) because we can use it to determine the
                                //       floor level which is currently used to create seperate MinimeshGeometry nodes for different
                                //       floors of the ship
                                loaded = interior.CreateEdgeSegmentStyle(index, style);

                            }

                            //  in this case "index" is an EdgeID.  IsSegmentPlaceable can NEVER tell us if a Segment is deletable
                            //  because footprint collision test would return not placeable and there'd be no way to distinguish that
                            // reason for not being placeable with any other kind such as one where no existing walls exists for 
                            // other footprint collision reasons for all we know, or whether edge is out of bounds, etc.
                            if (loaded && interior.IsSegmentPlaceable(style, index) == false)
                            {
                                // TODO: IsSegmentPlaceable() needs to know which segment based on adjacents needs to be applied doesn't it?
                                //       Otherwise how does it compute the correct footprint to test against the existing data layer?

                                System.Diagnostics.Debug.WriteLine("FormMainCommands.Worker_CelledRegion_Paint() - Segment Not Placeable.");
                                cancel[i] = true;
                                continue;
                            }

                            // assign the results to our message so the commandcompleted can update the scene visuals
                            styles[i] = style;
                            break;
                        }
                    default:
                        {
                            System.Diagnostics.Debug.WriteLine("FormMainCommands.Worker_CelledRegion_Paint() - Unexpected layer name " + paintCell.LayerName);
                            break;
                        }
                }

            }

            paintCell.Cancel = cancel;
            cmd.WorkerProduct = styles;
            // TODO: here we do NOT actually modify scene... we verify
            //       placement is valid and load load any geometry and scripts
            //       and since we dont modify the scene, there's nothing to save.
            //       modification occurs in CommandCompleted and then save should be
            //       sent to pager where all saves of celledregion are serialized to a single
            //       background thread for i/o
            //       TODO: hte main problem right now i think is validating placement if threaded
            //             can result in two or more items being validated but then one or more
            //             becoming invalid after the commandcomplete does actual placement
            //         we could make a temporary footprint that we modify and then do all tests
            //         against and modify it along the way, then we are done we update the entire
            //         original at once.  
            //         - if we do this, then we also need to prevent other calls to cell paint from
            //         occuring until this one is done.
            //         - we also need to make sure that a command such as this if for some reason 
            //         using UDP is always sequenced and never allowed to occur out of order. 
            //         - we must also ensure that a short subsequent operation done in thread and
            //         finishes before the first cannot be executed as long as the outstanding 
            //         operation hasnt completed.
            //       

            return state;

        }

        // obsolete - walls, floors, any entity all use common Worker_CelledRegion_Paint() method now
        //    private object Worker_Place_Wall_Into_CelledRegion(object state)
        //    {
        //        Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
        //        KeyCommon.Messages.Place_Wall_Into_CelledRegion placeWalls = (KeyCommon.Messages.Place_Wall_Into_CelledRegion)cmd.Message;

        //        string ext = System.IO.Path.GetExtension(placeWalls.PathInArchive);

        //        KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(placeWalls.RelativeArchivePath, placeWalls.PathInArchive);
        //        string resourcePath = descriptor.ToString();

        //        // NOTE: clone entities arg == true in ReadSychronous() call
        //        // This partricular xmldatabase has only one model or one entity in it so name and parent name can be ""
        //        Keystone.IO.XMLDatabase xmldatabase = new Keystone.IO.XMLDatabase();
        //        System.IO.Stream stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromArchive(
        //            placeWalls.PathInArchive, "", placeWalls.RelativeArchivePath);
        //        // TODO: GetStreamFromArchive(guid, "", guid); // use GUIDs and a hash table of database and entries to keep network packet small (no need for string paths)

        //        // Here the child is fully deserialized including any Scripts
        //        Keystone.Scene.SceneInfo info = xmldatabase.Open(stream);
        //        Keystone.Entities.Entity child;

        //        Keystone.Portals.CelledRegion parent = (Keystone.Portals.CelledRegion)Repository.Get(placeWalls.ParentCelledRegionID);

        //        for (int i = 0; i < placeWalls.EdgeIndices.Length; i++)
        //        {
        //            // clones a new wall entity each time
        //            child = (Entity)xmldatabase.ReadSynchronous(info.FirstNodeTypeName, "", "", true, true, false);


        //            // TODO: also some Components need to be restricted to _only_ being EntityPlacer.cs 
        //            // allowed to be placed inside CelledRegion interiors and not outside of ships.

        //            // TODO: but what about saving prefabs automatically?  like under what situation do i
        //            // change the underlying mesh3d and that updates the prefab and all entities in the xmldb
        //            // that uses it?
        //            // TODO: here we should query the child's flags which will have been initialized
        //            // the any DomainObjectScripts assigned to it...
        //            // These flags can also tell us whether to compute a position based on a wall or floor
        //            // Actually why not just have the worker here only assign and have all this computation
        //            // done in andvanced by the EntityPlacer then in AddChild our only real responsibility
        //            // is to query child/run scripts to modify layers?
        //// TODO: this should be done in the saved entity, NOT here.. perhaps make UseInstancing internal
        //            // like most of our properties should be... TODO: should not be be able to
        //            // set this property here from FormMain!... even though ultimately this particular
        //            // command handler function will be serverside.
        //          ((ModeledEntity)child).UseInstancing = true;

        //            // from .EdgeIndices we should be able to determine the rotation needed
        //            // for a wall... should the placement tool already figure that out?
        //            // TODO: in fact, edgeindices should be used to compute rotation
        //            // and then we use the footprint of the entity's domainobject 

        //          //child.CellIndex = placeWalls.CellIndices[i];
        //          //child.CellRotation = placeWalls.Rotations[i];
        //          //child.CellOffset =  placeWalls.CellOffset[i];
        //           // child.CellIndex = placeWalls.EdgeIndices[i];
        //            parent.AddChild(child);
        //        }
        //        return state;
        //    }

        // obsolete - in favor of the version above that also adds it to parent
        //private object Worker_PrefabLoad(object state)
        //{
        //    Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
        //    KeyCommon.Messages.Prefab_Load addPrefab = (KeyCommon.Messages.Prefab_Load)cmd.Message;

        //    string ext = System.IO.Path.GetExtension(addPrefab.PathInArchive);

        //    KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(addPrefab.RelativeArchivePath, addPrefab.PathInArchive);
        //    string resourcePath = descriptor.ToString();

        //    // the following stream retreived is expected to a be a zip within a zip (i.e .kgbentity) file.
        //    // TODO: check for nulls and invalid files, wrong versions and such

        //    // create wait dialog until the Thrower tool is instantiated.
        //    // since the Thrower tool will intitially read in ultimately any underlying mesh/actor geometry
        //    FormPleaseWait waitDialog = new FormPleaseWait();
        //    waitDialog.Show();

        //    // TODO: I should be initially loading this entity here so that the throwobjecttool will
        //    // only be initially created after the underlying prefab has been loaded
        //    Keystone.EditTools.ThrowObjectTool thrower = new Keystone.EditTools.ThrowObjectTool(
        //        (Keystone.Scene.ClientScene)AppMain._core.SceneManager.ActiveSimulation.Scene ,
        //        Keystone.Core.FullArchivePath(descriptor.RelativePathToArchive), addPrefab.PathInArchive);

        //    ((EditController)AppMain._core.CurrentIOController).CurrentTool = thrower;

        //    waitDialog.Close();
        //    return state;
        //}

        //private object Worker_CelledRegion_Paint_OBSOLETE(object state)
        //{
        //    Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
        //    KeyCommon.Messages.CelledRegion_Paint paintCell = (KeyCommon.Messages.CelledRegion_Paint)cmd.Message;

        //    Keystone.Portals.CelledRegion celledRegion = (Keystone.Portals.CelledRegion)Repository.Get(paintCell.ParentCelledRegionID);

        //    object value = paintCell.PaintValue;
        //    for (int i = 0; i < paintCell.CellIndices.Length; i++)
        //    {
        //        switch (paintCell.LayerName)
        //        {
        //            case "wall style":
        //                {
        //                    uint index = paintCell.CellIndices[i];
        //                    celledRegion.Layer_SetValue(paintCell.LayerName, index, value);
        //                    break;
        //                }
        //            default:
        //                {
        //                    uint index = paintCell.CellIndices[i];
        //                    celledRegion.Layer_SetValue(paintCell.LayerName, index, value);
        //                    break;
        //                }
        //        }

        //    }

        //    // save the cell data 
        //    celledRegion.SaveTVResource(null); // ok to pass null here because celldatabase is computed to use a naming convention

        //    return state;
        //}

        /// <summary>
        /// Load's an entity from either a prefab during insert in non simulation scene building, or a saved entity during a Simulation_Spawn call. 
        /// This function should always be called from a worker thread so will occur in the background.
        /// This function is NOT called during normal scene deserialization.
        /// </summary>
        /// <param name="fullpath"></param>
        /// <param name="isSavedEntity"></param>
        /// <param name="generateIDs"></param>
        /// <param name="recurse"></param>
        /// <param name="delayResourceLoading"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        private Entity LoadEntity(string fullpath, string relativePath, bool generateIDs, bool recurse, bool delayResourceLoading, string[] nodeIDsToUse, Vector3d translation)
        {
            //delayResourceLoading = false;
            //bool delay = true;
            Entity entity = Keystone.ImportLib.Load(fullpath, generateIDs, recurse, delayResourceLoading, nodeIDsToUse) as Entity;

            System.Diagnostics.Debug.Assert(entity != null);

            // if this is a Container, copy the prefab's cellDB to the correct Scenes\\CurrentSceneName\\ folder.
            // NOTE: This is not the same as a "Spawn" command which only occurs during Simulation and not just prefab or floorplan designing.
            // During "Simulation_Spawn" the .interior file and .kgbentity for the Container should already exist in the \\Saves\\ folder.  todo: verify this
            if (entity is Container)
            {
                Interior interior = ((Container)entity).Interior as Interior;
                if (interior != null)
                {
                    // NOTE: its ok that the datapath was originally loaded from a prefab for instance, but for this instance
                    // we now need to rename and copy the datapath so that we don't overwrite the existing prefab's Inteiror data file.
                    string originalRelativeDBPath = (string)interior.GetProperty("datapath", false).DefaultValue;
                    string originalFullPath = System.IO.Path.Combine(AppMain.MOD_PATH, originalRelativeDBPath);
                    Debug.WriteLine("FormMain.Commands.LoadEntity() - Interior dbpath = " + originalRelativeDBPath);
                    Debug.WriteLine("FormMain.Commands.LoadEntity() - Interior resource loaded = " + interior.TVResourceIsLoaded.ToString());
                    // Feb.29.2024 - the following seems wrong for simple scenes. The new saved path of the .interior file doesn't match the expected path when Interior.LoadTVResource() is performed
                    // Interior may not be loaded if delayResourceLoading == true, but if Interior is not null, the datapath should be set and available to reassign to this new instance
   
                    string newRelativePath = Path.Combine(AppMain.CURRENT_SCENE_NAME, entity.ID + ".interior");
                                        
                    // copy the relativePath to the new relativePath for this instance
                    // TODO: destinationpath needs to vary based on Core.SimulationEnabled.  Actually i dont think so because the spawned Container and it's .interior file will already be in the SAVE_PATH. TODO: verify this 
                    string newFullPath = System.IO.Path.Combine(AppMain.SCENES_PATH, newRelativePath);

                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(newFullPath);
                    fileInfo.Directory.Create();

                    if (System.IO.File.Exists(originalFullPath))
                    {
                        if (System.IO.File.Exists(newFullPath))
                        System.IO.File.Delete(newFullPath);

                        System.IO.File.Copy(originalFullPath, newFullPath);
                    }
                    else
                        System.IO.File.Create(newFullPath);

                    interior.SetProperty("datapath", typeof(string), newRelativePath);
                }
            }


            // load any entity script now during this worker thread
            if (!delayResourceLoading)
                PagerBase.LoadTVResource(entity, true);

            // set the prefab link.  Only caller of clone/deserialize/readsychrnous should
            // assign prefab links because sometimes when we clone or deserialize we don't want to
            //KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(addPrefab.RelativeArchivePath, addPrefab.EntryPath);
            entity.SRC = relativePath; // descriptor.ToString();

            ((Entity)entity).Translation = translation;
            ((Entity)entity).LatestStepTranslation = translation;


            return entity;
        }

        private void SaveInterior()
        {
        }


        void PositionVehicle(Keystone.Vehicles.Vehicle vehicle)
        {
            Database.AppDatabaseHelper.StarRecord[] starRecords = Database.AppDatabaseHelper.GetStarRecords();
            Database.AppDatabaseHelper.WorldRecord[] worldRecords = Database.AppDatabaseHelper.GetWorldRecords(starRecords[0].ID);

            // TODO: the star and world need to be paged in or otherwise we can't get the current positions of the worlds.
            double smass = starRecords[0].Mass;
            double sradius = starRecords[0].Radius;
            double wmass = worldRecords[0].Mass;
            double wradius = worldRecords[0].Radius;
            double woradius = worldRecords[0].OrbitalRadius;

            //Star star = (Star)Repository.Get(starRecords[0].ID);
            //            World w = (World)Repository.Get(worldRecords[0].ID);

            // June.20.2017 - orbital animations feature cut (postponed til version 2.0)
            //          w.Animations.Play(0, true);
            //          w.Animations.Update(w, 0);
            // TODO: this worldRecords[0].Translation is all wrong, wtf?  it's outside of the zone boundaries.
            //       I think it's because worldRecords[0].Translation is global translation.  I think it is!  We only want the Region space translation, so we'll need to add those to the record.
            Vector3d worldPosition = worldRecords[0].Translation; // w.Translation; // TODO: for moon this wont work since we need RegionTranslation but for planet, Translation and RegionTranslation are the same thing.
            double altitude = wradius + 1000000;

            Vector3d dir = Vector3d.Normalize(worldPosition); // can just normalize because it's dir to star and we know that star is at origin
            Vector3d vehicleTranslation = worldPosition + dir * altitude;
            // TODO: On even numbered zone's across, height, depth the coord 0,0,0 is far away from camera starting point! 
            //       This is why we need to choose relative region position and parent that is Zone and not ZoneRoot (Or a position relative to a Star or World)
            vehicle.Translation = vehicleTranslation; // vehicleTranslation; //  Vector3d.Zero(); // TODO: use proper position here based on orbit

            Vector3d basisVector = Vector3d.Up();// w.RegionMatrix.Right;

            Vector3d tangent = Vector3d.Normalize(Vector3d.CrossProduct(-dir, basisVector));
            //tangent = Vector3d.CrossProduct(w.RegionMatrix.Up, tangent);
            //double worldVelocity = GenerateWorld.GetOrbitalVelocity(smass, 0, woradius);
            //worldVelocity = Keystone.Celestial.Temp.GetCircularOrbitVelocity(smass + wmass, woradius);
            //worldVelocity = Keystone.Animation.EllipticalAnimation.GetTrueAnomaly(w.OrbitalPeriod, 1d);

            // TODO: I don't know if this is working because it seems the star's gravity pull is pulling us
            //       through the planet and onto the star.  It seems perhaps we need something like gravity wells
            //       where gravity emitting bodies only affect vehicles within their well. Let's revisit this issue
            //       once we get the tangent vector solved correctly and see if our starship can at least orbit
            //       a few times before the orbit is destabilized by the Star's gravity pull.
            // TODO: I could test it by just removing that from the Star script... see if we can orbit this planet.
            double velocity = Keystone.Celestial.GenerateWorld.GetOrbitalVelocity(wmass, 0, altitude);
            //velocity += worldVelocity;
            // is v= Math.Sqr(GMr)
            // - retrieve world and moon records and assign moon as parentID?  No.  That is only if we want
            //   hierarchical position of our ship and we're still trying to use absolute region positions based on gravitation
            // - compute velocity for ship around moon including hierarchical velocities for worlds around stars
            //      - find tangent vector velocities for each and add them
            vehicle.Velocity = tangent * velocity;

            //float altitude = 10000000000f;
            double semiMajorAxis = altitude;
            ////string vehiclePath = @"caesar\\meshes\\vehicles\\uesn_yorktown.kgbentity";
            ////// TODO: there are issues with loading .obj files.  Perhaps if i switched from AddVertex to SetGeometry it would solve the problem?
            //////vehiclePath = @"caesar\\meshes\\vehicles\\morena smuggler\\morena1.kgbentity";
            ////   ModeledEntity vehicle = CreateVehicle (vehiclePath, region, star, altitude);

            ////   // TODO: 
            ////   double G = Keystone.Celestial.Temp.GRAVCONST;

            ////   //TODO: does this yeild KM/s or M/s? we want M/s.
            ////   double velocityMetersPS = Math.Sqrt((2d * G * star.MassKg / altitude) - (G * star.MassKg / semiMajorAxis));
            ////   // velocityMetersPS *= 1000;

            ////   vehicle.Velocity = new Vector3d (velocityMetersPS, 0, 0 );
            ////   //transformable.AngularVelocity = ;



            // sychronous loading
            //            Keystone.IO.PagerBase.LoadTVResource (vehicle);

            //// vehicle's are not hierarchically tied to worlds, they are Region relative
            //// so translation to a world should apply cumulative translation of Sol system hierarchy
            //// WARNING: vehicle starting too close to planet (such as inside planet!) will cause physics to go crazy.
            //vehicle.Translation = body.GlobalTranslation + new Vector3d(0, 0, body.Radius + altitude);

            //region.AddChild(vehicle);

            //// compute starting velocity
            //vehicle.Force += body.Velocity;
            //// TODO: how can we test this with 1 star and 1 computed velocity vector
            ////       and then distance to star should be constant for a nearly perfect starting velocity vector
            ////       at close distance.  

            //// TODO: can we delay 1 frame so that our orbital animation can play at least once to set the
            ////       orbits at their starting positions so we know where to place this vehicle and then 
            ////       the starting velocity to assign?

            //// actual velocity is cumulative velocity of world velocity and relative orbital velocity
            //// - velocity of a world is vector length of previous position and current
            ////   - but can we get it from the eliptical animation itself

            // assign this vehicle as chase camera follow target
        }



        private object Worker_EntityMove(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Entity_Move move = (KeyCommon.Messages.Entity_Move)cmd.Message;

            // typically, this Worker is only for movement within an Interior hosted Entity
            // (otherwise, we would simply translate the Entity)
            // THis is because
            // to simulate a Move behavior, we RemoveChild() and AddChild() the node back to 
            // the same parent, but that must be done on the main thread

            Keystone.Entities.Entity[] children = new Entity[move.mTargetIDs.Length];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = (Keystone.Entities.Entity)Repository.Get(move.mTargetIDs[i]);
                // todo: should we just Translate by the diff of existing children[i].Translation and move.Positions[i]?
                //       i think if we just .Translate() we dont need to remove from the parent and re-add. 
                //       But to update the TileMapGrid in Interior, don't we have to remove and then re-add the Component?
                children[i].Translation = move.Positions[i];
            }

            cmd.WorkerProduct = children;
            return state;
        }
     

        //private void PhysicsHelper(Entity entity)
        //{
            ////EntityBase parent = ((Commands.ImportEntityBase)result.State).Parent;
            //// we don't want or need to write a thrower entity we've imported

            //// set up the physics for this entity
            //entity.PhysicsBody = new JigLibX.Physics.Body();
            //entity.PhysicsBody.Immovable = false;
            //entity.PhysicsBody.Mass = 1;
            //entity.PhysicsBody.CollisionSkin = new JigLibX.Collision.CollisionSkin(mLastResult.PhysicsBody);
            //float x = (float)((Commands.ImportStaticEntity)result.State).Position.x;
            //float y = (float)((Commands.ImportStaticEntity)result.State).Position.y;
            //float z = (float)((Commands.ImportStaticEntity)result.State).Position.z;

            //entity.PhysicsBody.SetPosition(x, y, z);

            //entity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Sphere(0, 0.5f, 0, 1), new JigLibX.Collision.MaterialProperties(0.1f, 0.5f, 0.5f));
            //entity.PhysicsBody.SetVelocity((float)_velocity.x, (float)_velocity.y, (float)_velocity.z);
            //entity.PhysicsBody.EnableBody();
            //// wire up collision events?
            ////entity.PhysicsBody.CollisionSkin.callbackFn
        //}

        

        private object Worker_NodeGetChildren(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_GetChildren getChildren = (KeyCommon.Messages.Node_GetChildren)cmd.Message;
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(getChildren.NodeID);


            return state;
        }

        // todo: actual removal from scene needs to occur in ProcessCommandCompleted()
        private object Worker_NodeRemove(object state)
        {

            // WARNING: When debugging and hitting DELETE key to modify code
            // this will trigger and attemp to delete the currently selected Entity!
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_Remove remove = (KeyCommon.Messages.Node_Remove)cmd.Message;
            
            string[] nodesToRemove = remove.mTargetIDs;
            string p = remove.mParentID;
                        
            Keystone.Elements.IGroup parentGroup = Keystone.Resource.Repository.Get(p) as Keystone.Elements.IGroup;

            if (parentGroup == null) throw new ArgumentNullException("FormMain.Commands.Worker_NodeRemove() - no parent found");

            Node[] children = new Node[nodesToRemove.Length];
            List<string> arcadeEntities = new List<string>();
            for (int i = 0; i < nodesToRemove.Length; i++)
            {
                children[i] = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(nodesToRemove[i]);
                
                if (parentGroup is Keystone.Appearance.Appearance && children[i] is Keystone.Shaders.Shader)
                {
                    ((Keystone.Appearance.Appearance)parentGroup).ResourcePath = null;
                }

                if (children[i] as Entity != null)
                {
                    Entity childEntity = (Entity)children[i];
                    childEntity.Activate(false); // todo: perhaps this line can go in the worker too
                    if (childEntity is BonedEntity) // todo: there should be a "dynamic" flag we can check on Entity to see if it's a dynamic entity that is not stored in the xml but seperately as a prefab in the \\SAVES\\ path
                    {
                        if (AppMain._core.ArcadeEnabled)
                        {
                            // NOTE: This worker should work even if we are just single mouse picking and deleting a BonedEntity
                            Scene.DeleteEntityFile(childEntity.ID);
                            arcadeEntities.Add(childEntity.ID);

                            // todo: are we removing mChangedNodes Entities in Simulation? we can check the entity.RefCount == 0 and skip processing them and just remove them from the activelist.
                            //       There shouldn't be any race conditions because actual removal of Entity occurs in ProcessCommandCompletedQueue() which is on the main thread
                        }
                    }
                }


                //Rules[] brokenRules = parent.RemoveChild(child);
                // if (brokenRules == null || brokenRules.Length == ) // then we're valid
                //{
                //
                //}
                // get the rules for this type of message?
                //msg.CreateRules();
                //if (msg.Validate())
                //{

                //}
                // do NOT allow removal of some node types
                // - root
                // - fixed child regions of root(eg zones)
                // - intrinsic animations 
                // - any AppearanceGroup because those are intrinsic
                //   - though it'd be nice if in plugin we could merge groups, save the new tvm/tva
                //     and then replace the old geometry with the new in the AppMain.ModName
                //
                // WHERE IS OUR VALIDATION RULES FOR OUR BUSINESS OBJECTS?
                //

            }

            if (arcadeEntities != null && arcadeEntities.Count > 0)
                Database.AppDatabaseHelper.DeleteCharacterRecords(arcadeEntities.ToArray());

            cmd.WorkerProduct = children;
            return state;
        }

        

        private object Worker_NodeMoveOrder(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_MoveChildOrder move = (KeyCommon.Messages.Node_MoveChildOrder)cmd.Message;

            string parentMoveChildOrder = move.ParentID;
            string childMoveChildOrder = move.NodeID;
            bool down = move.Down;

            IGroup parent = (IGroup)(Node)Repository.Get(parentMoveChildOrder);
            // note: this is mostly relevant for DefaultAppearnace as it applies to
            //       mesh Group rendering order,
            //       although node re-ordering should be ok for any IGroup node type but for now
            //       there is no relevance for doing so.
// TODO: should only occur in out single threaded CommandCompleted             
            parent.MoveChildOrder(childMoveChildOrder, down);
            return state;
        }

        private object Worker_InsertUnderNew(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_InsertUnderNew insert = (KeyCommon.Messages.Node_InsertUnderNew)cmd.Message;

            Node node = Keystone.Resource.Repository.Create(insert.InsertedNode, insert.InsertedNodeType);
            System.Diagnostics.Debug.Assert(node is Keystone.Elements.IGroup);

            // add the previous under the new node
            Node previous = (Node)Repository.Get(insert.ReparentedNode);
            IGroup parent = (IGroup)(Node)Repository.Get(insert.Parent);

            // TODO: The following supersetter addchild in loopback results in the client Form.Commands
            // message processor to be unable to add the child itself because it's already
            // added.  However, we cannot construct the "Node_Create" node unless the hierarchy
            // is built.. and so that hierarchy can be re-assembled by client Form.Commands.
            // So one solution is to remove child for everything we addchild for but then
            // we're unable to prevent them from refcount == 0 and removed.
            // The other option is to handle all this here and dont allow client form.commands
            // to process anything when in loopback...just let the server do it all...

            // after adding the previous under the new node, we can remove it from it's old parent
            // NOTE: in loopback we do NOT remove here, the client will when we notify it to do so below
            // TODO: upon removing previous, 

            // remove the model from the parent Entity but first increment ref it so
            // that it's not accidentally deleted from Repository
            Repository.IncrementRef(previous);
            parent.RemoveChild(previous);

            // add the new ModelSelector to the parent entity
            SuperSetter setter = new SuperSetter((Node)parent);
            setter.Apply(node);

            // add the model to the new ModelSelector
            setter = new SuperSetter(node);
            // TODO: should only occur in out single threaded CommandCompleted 
            setter.Apply(previous);
            // decrement the artificial increment after we've added the model to a node that is 
            // ultimately attached to the main scene (i.e. don't DecrementRef simply after adding to
            // the ModelSelector if the ModelSelector itself is not connected to the scene yet)
            Repository.DecrementRef(previous);
            return state;
        }

        private object Worker_NodeRenameResource(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_RenameResource rename = (KeyCommon.Messages.Node_RenameResource)cmd.Message;

            Keystone.Elements.Node oldChild = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(rename.OldResourceID);
            Keystone.Elements.IGroup parent = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(rename.ParentID);


            cmd.WorkerProduct = new Node[] { (Node)parent, oldChild };

            return state;
        }

        /// <summary>
        /// This worker only deals with replacing of resources (textures, meshes) because
        /// their ID is always the same as their resource path descriptor which means
        /// the client can specify the change without first needing a create node request
        /// which returns a server generated GUID so that nodes are always sychronized.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private object Worker_NodeReplaceResource(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_ReplaceResource replace = (KeyCommon.Messages.Node_ReplaceResource)cmd.Message;


            
            // NOTE: remove/setter of children should occur in ProcessCompleted.  Why?  Because in attempting to remove a child now while rendering is occurring
            //       can cause issues if for example a material added to a TVMesh\TVActor\etc is removed, it's refcount goes to 0 and then
            //       is destroyed, but is still assigned to TVMesh and then during render tv access violation occurs because the material is destroyed
            
          
            //cmd.WorkerProduct = ;
            return state;
        }

        private object Worker_NodeChangeParent(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_ChangeParent change = (KeyCommon.Messages.Node_ChangeParent)cmd.Message;
            Keystone.Elements.Node child = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(change.NodeID);
            Keystone.Elements.Node parent = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(change.ParentID);
            Keystone.Elements.IGroup oldParent = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(change.OldParentID);

            Repository.IncrementRef(child);

            if (child as Transform != null)
            {
                Transform newParent = (Transform)parent;
                Transform old = (Transform)oldParent;

                // NOTE: Make sure child object has inherit rotation enabled in flags at least 
                Matrix result = ((Transform)child).GlobalMatrix * Matrix.Inverse(newParent.GlobalMatrix);
                //result = ((Transform)child).GlobalMatrix * Matrix.Inverse(newParent.GlobalMatrix);

                // must switch parents before setting translation or scale because
                // order of change state flags is important and detach + attach must occur first
                oldParent.RemoveChild(child);
                Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(newParent);
                setter.Apply(child);

                // the above result assumes inherit scale since it takes into account full matrix
                ((Transform)child).Translation = new Vector3d(result.M41, result.M42, result.M43);
                ((Transform)child).Scale = new Vector3d(result.M11, result.M22, result.M33);
            }
            else
            {
                oldParent.RemoveChild(child);
                Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parent);
                // TODO: should only occur in out single threaded CommandCompleted 
                setter.Apply(child);
            }
            
            Repository.DecrementRef(child);
            return state;
        }

        // todo: i think changes to the nodes need to occur in ProcessCommandCompleted()
        //       here all i can really do is find and cache the target Node 
        private object Worker_NodeChangeFlag(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_ChangeFlag change = (KeyCommon.Messages.Node_ChangeFlag)cmd.Message;
            IResource res = Keystone.Resource.Repository.Get(change.NodeID);

            cmd.WorkerProduct = res;
            return state;
        }


        private object Worker_NodeChangeProperty(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_ChangeProperty change = (KeyCommon.Messages.Node_ChangeProperty)cmd.Message;
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(change.NodeID);


            cmd.WorkerProduct = node;

            return state;
        }

        private object Worker_NodeGetProperty(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Node_GetProperty change = (KeyCommon.Messages.Node_GetProperty)cmd.Message;
            // note: currently not used and maybe not needed for getters
            // since we may only ever need to "get" property values directly
            // from nodes and so no network command gets sent and thus there's nothing
            // to be processed by threaded worker
            return state;
        }

        private object Worker_EntitySetCustomProperties(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Entity_SetCustomProperties set = (KeyCommon.Messages.Entity_SetCustomProperties)cmd.Message;
            // TODO: try catches nowhere to be found... or is it built into our threaded workers?


            // if entity is in repository, 
            Node node = (Node)Keystone.Resource.Repository.Get(set.EntityID);
            bool loadedFromDB = node == null;
            // pull the node from the xmldb.  NOTE: that the client has 
            // this node in the xmldb is implied because for the client to request the
            // properties in the first place, they would have to have known the entityID!
            if (loadedFromDB)
            {
                XMLDatabase xmldb = new XMLDatabase();
                SceneInfo info = xmldb.Open(set.SceneName, true);
                bool clone = false;
                bool recurse = true;
                bool delayLoading = false; // this is already a worker thread so 
                                           // we dont want to delay loading of the DomainObject however the
                                           // mesh or bavhior tree we are uninterested in... hrm...
                // we must recurse to load the domainObject otherwise there is no way to gain access
                // to custom properties.  But this also means the domainobject must be DESERIALIZED
                node = (Node)xmldb.ReadSynchronous(set.EntityTypeName, set.EntityID, recurse, clone, null, true, false);
            }

            int[] errorCodes;
            // apply the properties // TODO: should only occur in out single threaded CommandCompleted 
            ((Entity)node).SetCustomPropertyValues(set.CustomProperties, false, true, out errorCodes);

            // TODO: notify all workspaces
            

            return state;
        }

        // this has to be 2 halves, a request and a 
        // authorization result (yes, no)
        // this is the essence of our attempt to reliably validate user commands
        // requests will get run thru validation script and if any one command fails
        // they should all fail.

        private object Worker_EntityChangeCustomProperty(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Entity_ChangeCustomPropertyValue change = (KeyCommon.Messages.Entity_ChangeCustomPropertyValue)cmd.Message;
            // TODO: try catches nowhere to be found... or is it built into our threaded workers?

            Node node = (Node)Keystone.Resource.Repository.Get(change.EntityID);


            if (node is Entity)
            {
                Entity entity = (Entity)node;
               // NOTE: application of the values occurs in ProcessCommandCompleted()
                // entity.SetCustomPropertyValues(change.CustomProperties, false, true, out errorCodes);
            }
            else if (node is GroupAttribute)
                // TODO: This must occur in ProcessCommandCompleted()
                //        The reason we use the same command is because shader parameters are stored in GroupAttribute the same way as Entity.CustomProperties
                ((GroupAttribute)node).SetShaderParameterValues(change.CustomProperties);
            else
                throw new Exception("Worker_EntityChangeCustomProperty() - Unexpected type '" + node.TypeName + "'");


            cmd.WorkerProduct = node;

            

            return state;
        }

        private object Worker_EntityChangeShaderParameter(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Shader_ChangeParameterValue change = (KeyCommon.Messages.Shader_ChangeParameterValue)cmd.Message;
            // TODO: try catches nowhere to be found... or is it built into our threaded workers?
            // TODO: what if it's not an entity node and cast is illegal?
            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)Keystone.Resource.Repository.Get(change.AppearanceID);

            // TODO: the actual apply of these properties to the node
            // should not occur in threaded workers, only in single threaded ocmmandcompleted?
            int[] errorCodes = null;
            // TODO: should only occur in out single threaded CommandCompleted 
            entity.SetCustomPropertyValues(change.ShaderParameters, false, true, out errorCodes);

            return state;
        }


        private object Worker_GameObject_ChangeProperties(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.GameObject_ChangeProperties changeProperties = (KeyCommon.Messages.GameObject_ChangeProperties)cmd.Message;

            Entity owner = (Entity)Repository.Get(changeProperties.Owner);

            switch (changeProperties.GameObjectTypeName)
            {
                case "TacticalState":
                    break;
                case "HelmState":
                    break;

                // todo: i think NavPoint shouldn't be a gameobject, but rather a member of HelmState and we should add ability to Read/Write via NetBuffer all GameObjects
                case "NavPoint":
                    Game01.GameObjects.NavPoint[] navPoints = (Game01.GameObjects.NavPoint[])owner.GetCustomPropertyValue("navpoints");

                    int index = GetNavPointArrayIndexFromRowID(changeProperties.GameObjectIndex, navPoints);
                    if (index >= 0)
                    {
                        navPoints[index].SetProperties(changeProperties.Properties);
                        cmd.WorkerProduct = navPoints;
                    }
                    break;

                default:
                    break;
            }
            

            return state;
        }

        private int GetNavPointArrayIndexFromRowID(int rowID, Game01.GameObjects.NavPoint[] navPoints)
        {
            int index = -1;
            for (int i = 0; i < navPoints.Length; i++)
            {
                if (navPoints[i].RowID == rowID)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private object Worker_Task_Create (object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            Game01.Messages.Task_Create create = (Game01.Messages.Task_Create)cmd.Message;


            // add the task to the client side task table
            Database.AppDatabaseHelper.CreateTaskRecordClient(create.Order, Database.AppDatabaseHelper.GetConnection());

            // client entities can't poll the task table in the db for new tasks.  
            // Here we know exactly when the task comes in.  We deduce which station it's assigned to
            // based on the "task" field.  

            // so let's say we get a "helm" related task.  
            // - what if the client is already executing a helm task?
            // - how do we assign the new task to the helm station?
            //   - what if we set a flag in custom property so the Script knows when to poll the db?
            // - how do we assign the new task if it's a future task, one that should only
            //   execute after previous queued tasks are completed?
            // - how do we resume tasks when the player Exits and then restarts sim? Presumeably we 
            //   load up all current tasks when loading the sim.
            // - how do we update the task status when for instance, going from Executing to Completed?


            // In the ProcessCompletedCommandQueue() we can...
            // find the correct station entity and then queue the task to a custom property within that entity
            // we can find the correct station by search by name of children of the Order.OwnerID (vehicleID)

            // add model-less entity to Vehicle with name #STATION_HELM
            //      - how should we add a model-less entity to the vehicle?  Ideally via plugin editor
            //        This way we can modify and save the Vehicle to prefab easily.  Or via right mouse click on Vehicle->Add Empty Entity
            //  - add script to our helm entity
            //  - add custom property task_queue (or is it a list)
            //
            return state;
    }

        private object Worker_GameObject_Create(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.GameObject_Create create = (KeyCommon.Messages.GameObject_Create)cmd.Message;
            
            // TODO: does this mean that we can only create gameobjects for Entities that are instantiated?
            //       In other words, for Entities that are in paged in Zones/Regions?
            Entity owner = (Entity)Repository.Get (create.Owner);
            string type = create.GameObjectTypeName;

            // TODO: we don't necessarily need to create a new GameObject such as a NavPoint
            //       we need to be able to modify a specific NavPoint GameObject as well
            //       as append.  What commands do we need to implement to fascilitate this?
            KeyCommon.DatabaseEntities.GameObject gameObject = Game01.GameObjects.Factory.Create(create.GameObjectID, type);
            gameObject.SetProperties(create.Properties);
                        
            // TODO: should these all be in "userdata" now and not in .GetCustomPropertyValue()? 
            //       recall userdata since it's runtime data does not need to be serialized
            // TODO: why are these AI pathing vars set here?
            // TODO: a command for Worker_GameObject_Replace()?
            //       What about other game object types?
            KeyCommon.Data.UserData data = (KeyCommon.Data.UserData)owner.GetCustomPropertyValue("userdata");
            if (data != null)
            {
                // TODO: I believe this sort of thing should be done in the Entity's script
                //data.SetVector3dArray ("navpoints", create.
                data.SetBool("ai_path_in_progress", true);
                data.SetInteger("ai_current_nav", 0);
                data.SetInteger("ai_current_path", 0);
                data.SetBool("ai_wander", false);
            }

            // what if we just move a waypoint and not append new or replace all of them?  
            // we need to identify the changed waypoint and update just that entry in the navpoints array.
            // if (create.AppendWaypoint) // <-- TODO: not yet implemented this branch path
            // {
    	 			// get existing waypoints 
    	        	// TODO: maybe i should include in the GameObject_Create the custom property value name
		           	//  that the game object is stored on within the owner vehicle      
        	    	// Game01.GameObjects.NavPoint[] navPoints = (Game01.GameObjects.NavPoint[])owner.GetCustomPropertyValue("navpoints");

            		// // add this new waypoint to the array
            		// waypoints = waypoints.ArrayAppend((Game01.GameObjects.Waypoint)gameObject);
            // }
			// else 
			// {
            		// REPLACE custom property "navpoints" array with new one (rather than append) 
            		// that contains the mouse picked navPoint specified in the gameObject
            		Game01.GameObjects.NavPoint navPoint = (Game01.GameObjects.NavPoint)gameObject;
            		Game01.GameObjects.NavPoint[] navPoints = new Game01.GameObjects.NavPoint[] {navPoint};
            // }


            // TODO: update of the value should occur in Update() thread so we are not updating these when anther thread is using them
            cmd.WorkerProduct = navPoints;

            Settings.PropertySpec property = new Settings.PropertySpec("navpoints", typeof(Game01.GameObjects.NavPoint[]), "private", navPoints);
            int[] brokenCodes;
            owner.SetCustomPropertyValues(new Settings.PropertySpec[] { property }, false, false, out brokenCodes);

            return state;
        }

        private object Worker_MissionResult(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.MissionResult result = (KeyCommon.Messages.MissionResult)cmd.Message;

            if (result.Success)
            {
            }
            else
            {

            }

            return state;
        }


        private object Worker_CommandSucceeded(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.CommandSuccess success = (KeyCommon.Messages.CommandSuccess)cmd.Message;
            Amib.Threading.WorkItemCallback cb = null;

            KeyCommon.Messages.MessageBase msg = mUnconfirmedCommands[success.ReferencedCommand];
            mUnconfirmedCommands.Remove(success.ReferencedCommand);

            // actually need to get the cb to use here...
            // 

            // note: here we do NOT queue the command for execution, this worker function is already
            // being handled by a worker thread so we want to execute the now confirmed
            // commands execution function on this same thread immediately


            Keystone.Commands.Command unconfirmedCmd = new Keystone.Commands.Command(msg);
            CreateMessage((int)msg.CommandID, out cb);
            cb(unconfirmedCmd);

            // this return then allow us to use the CommandCompleted of the original success and still get a IWorkItemResult
            // note the unconfirmedCmd will be stored in IWorkResult.Result and NOT IWorkResult.State
            return unconfirmedCmd;
        }

        private object Worker_CommandFailed(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.CommandFail fail = (KeyCommon.Messages.CommandFail)cmd.Message;

            KeyCommon.Messages.MessageBase msg = mUnconfirmedCommands[fail.ReferencedCommand];
            mUnconfirmedCommands.Remove(fail.ReferencedCommand);

            return state;
        }

        string mFileParentID;
       
        private Dictionary<string, FileFragmentState> mFileFragmentStates;

        private struct FileFragmentState
        {
            public string FullPath;
            public long FileSize;

            public FileStream mFileStream;
            public int mFileReceivedBytes;
        }

        private Dictionary<string, CommandFragmentState> mFragmentedCommands = new Dictionary<string, CommandFragmentState>();
        private struct CommandFragmentState
        {
            public KeyCommon.Messages.IFragmentable Command;
            public int ElementsReceived; // NOTE: elements not "packets."  Each command packet can contain multiple number of elements

        }


        // NOTE: Only one .kgbentity file transfer can occur at a time
        // todo: ideally we'd use a seperate thread and client connection to server for file and stream transfers
        private object Worker_File_Transfer(object state)
        {
            lock (FileTransferLock)
            {
                Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
                KeyCommon.Messages.Transfer_Entity_File fileTransferCommand = (KeyCommon.Messages.Transfer_Entity_File)cmd.Message;
                FileFragmentState fragmentState;

                // server increments the FragmentIndex
                if (fileTransferCommand.IsFragmented && fileTransferCommand.FragmentIndex > 0)
                {
                    // grab the FileFragmentState for this existing transfer
                    fragmentState = mFileFragmentStates[fileTransferCommand.Guid];

                    fragmentState.mFileStream.Write(fileTransferCommand.Data, 0, fileTransferCommand.DataSize);
                    fragmentState.mFileReceivedBytes += fileTransferCommand.DataSize;
                }
                else
                {
                    fragmentState = new FileFragmentState();
                  
                    fragmentState.FullPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileTransferCommand.mRootNodeID) + ".kgbentity";
                    fragmentState.FileSize = fileTransferCommand.FileLength;
                    fragmentState.mFileStream = new FileStream(fragmentState.FullPath, FileMode.Create, FileAccess.Write, FileShare.None); //, (int)mFileSize, false);
                    fragmentState.mFileReceivedBytes = fileTransferCommand.DataSize;

                    if (mFileFragmentStates == null) mFileFragmentStates = new Dictionary<string, FileFragmentState>();

                    mFileFragmentStates.Add(fileTransferCommand.Guid, fragmentState);

                   
                    mFileParentID = fileTransferCommand.mParentID;
                    // todo: we need to know if this is a modpath, saves path or scenes path or temp even
                    // todo:  the temp file  should be saved to the \\saves  path using the current SceneName (eg Campaign001)


                    fragmentState.mFileStream.Write(fileTransferCommand.Data, 0, fileTransferCommand.DataSize);


                    bool isContainer = (fileTransferCommand.mRootNodeTypename == "Vehicle" || fileTransferCommand.mRootNodeTypename == "Container");
                    
                    // NOTE: we load the vehicle in UpdateLoopback() because we want the server to establish the node IDs and that the client will 
                    //       then import without cloning. Remember that a Vehicle or Container can contains A LOT of crew, furniture, components, etc
                    //       and so sending them as a single file is better than trying to send a hiearchy of NodeState
                    // TODO: Wait, don't we need to copy this file from temp path to \\saves along with the .interior  ?
                    if (isContainer)
                    {
                        // copy the interior to the \\saves path
                        string interiorOriginalFullPath = Path.Combine(_core.ModsPath, fileTransferCommand.mPrefabRelativePath);
                        interiorOriginalFullPath = Path.ChangeExtension(interiorOriginalFullPath, ".interior");
                        string destinationFullPath = Path.Combine(_core.SavesPath, fileTransferCommand.mNewRelativeDataPath);
                        File.Copy(interiorOriginalFullPath, destinationFullPath);
                    }

                    // copy the temp prefab to the \\saves path
                    string fullSavesFolderPath = Path.Combine(_core.SavesPath, fileTransferCommand.mNewRelativeDataPath);
                    Path.ChangeExtension(fullSavesFolderPath, ".kgbentity");
                    File.Copy(fragmentState.FullPath, fullSavesFolderPath);
                }


                // we've received the entire file.  Flush and then close the stream and load the completed .kgbentity file
                if (fragmentState.mFileReceivedBytes >= fragmentState.FileSize)
                {
                    fragmentState.mFileStream.Flush();
                    fragmentState.mFileStream.Close();
                    fragmentState.mFileStream.Dispose();
                    fragmentState.mFileStream = null;

                    bool generateIDs = false;
                    bool recurse = true;
                    // TODO: this is a worker thread, shouldn't we NEVER delayResourceLoading during worker threads?
                    bool delayResourceLoading = true; // NOTE: we especially want to delay loading of .Interior before we have a chance to update the dbpath property to point to a new coopy of the .interior db file
                                                      //       but i'm not sure since this worker is in background thread if it will prevent resource loading of the queued resources.
                                                      // TODO: WARNING - we don't even know if the interior components will get placed in the tilemask grid properly if the interior db isn't loaded first!!!!
                                                      //       So what we could do is have the server set the updated db path prior to saving the prefab to temp file and sending it
                                                      //       And then here we copy the .interior to the new name.  The vehicle or container relativePrefabPath must be assigned to the Command we process here
                                                      // TODO: i also need to verify that during deserialization, if adding components to the vehicle.Interior before its LoadTVResource() occurs, if this prevents
                                                      //       those components from having their footprints applied to the interior tilemask grid.
                                                      // TODO: I should add an assert in inteior.AddChild(entity) if TVResourceIsLoaded == false


                    // NOTE: The server has generated this .kgbentity file with the nodeIDs we must use. 
                    // so clone = false and we pass null for "NodeIDsToUse" parameters in ImportLib.Load()
                    Entity entity = (Keystone.Entities.Entity)Keystone.ImportLib.Load(fragmentState.FullPath, generateIDs, recurse, delayResourceLoading, null);
                    cmd.WorkerProduct = entity;
                }
                return state;

            }
        }

        private object FileTransferLock = new object();


        private object Worker_NotifyPlugin_NodeSelected(object state)
        {
            return state;
        }

        private object Worker_NotifyPlugin_ProcessEventQueue(object state)
        {
            return state;
        }

        private object Worker_Stream_Transfer(object state)
        {
            Keystone.Commands.Command cmd = (Keystone.Commands.Command)state;
            KeyCommon.Messages.Transfer_Stream stream = (KeyCommon.Messages.Transfer_Stream)cmd.Message;


            return state;
        }

        private KeyCommon.Messages.MessageBase CreateMessage(int commandID, out Amib.Threading.WorkItemCallback cb)
        {
            KeyCommon.Messages.MessageBase msg;

            if (commandID > (int)KeyCommon.Messages.Enumerations.UserMessages)
            {
                switch ((Game01.Enums.UserMessage)commandID)
                {
                    case Game01.Enums.UserMessage.Game_AttackResults:

                        msg = new Game01.Messages.AttackResults();
                        cb = null;
                        break;

                    default:
                         msg = null;
                        cb = null; ;

                        Debug.WriteLine("FormClient.UserMessageReceived() - Unsupported message type '" + commandID.ToString() + "'.");
                        break;
                }

                return msg;
            }


            switch ((KeyCommon.Messages.Enumerations)commandID)
            {
                case KeyCommon.Messages.Enumerations.CommandSuccess:
                    msg = new KeyCommon.Messages.CommandSuccess();
                    cb = Worker_CommandSucceeded;
                    break;
                case KeyCommon.Messages.Enumerations.CommandFail:
                    msg = new KeyCommon.Messages.CommandFail();
                    cb = Worker_CommandFailed;
                    break;
                case KeyCommon.Messages.Enumerations.MissionResult:
                    msg = new KeyCommon.Messages.MissionResult();
                    cb = Worker_MissionResult;
                    break;
                case KeyCommon.Messages.Enumerations.NotifyPlugin_NodeSelected:
                    msg = new KeyCommon.Messages.NotifyPlugin_NodeSelected();
                    cb = Worker_NotifyPlugin_NodeSelected;
                    break;
                case KeyCommon.Messages.Enumerations.NotifyPlugin_ProcessEventQueue:
                    msg = new KeyCommon.Messages.NotifyPlugin_ProcessEventQueue();
                    cb = Worker_NotifyPlugin_ProcessEventQueue;
                    break;
                case KeyCommon.Messages.Enumerations.TransferEntityFile:
                    msg = new KeyCommon.Messages.Transfer_Entity_File();
                    cb = Worker_File_Transfer;
                    System.Diagnostics.Debug.WriteLine("File Transfer Command Received");
                    break;

                
                //case KeyCommon.Messages.Enumerations.LoadScene:
                //	mOpenSceneInProgress = true;
                //    msg = new KeyCommon.Messages.Scene_Load();
                //    cb = Worker_LoadScene;
                //    break;
                case KeyCommon.Messages.Enumerations.Simulation_Join:
                    msg = new KeyCommon.Messages.Simulation_Join();
                    cb = Worker_SimulationJoin;
                    break;
               case KeyCommon.Messages.Enumerations.NewScene:
                    msg = new KeyCommon.Messages.Scene_New();
                    cb = Worker_GenerateNewScene;
                    break;
                case KeyCommon.Messages.Enumerations.NewFloorplan:
                    msg = new KeyCommon.Messages.Floorplan_New();
                    cb = Worker_GenerateNewFloorplan;
                    break;
                case KeyCommon.Messages.Enumerations.NewUniverse:
                    msg = new KeyCommon.Messages.Scene_NewUniverse();
                    cb = Worker_GenerateNewUniverse;
                    break;
                case KeyCommon.Messages.Enumerations.NewTerrainScene:
                    msg = new KeyCommon.Messages.Scene_NewTerrain();
                    cb = Worker_GenerateNewTerrainScene;
                    break;
                    
                case KeyCommon.Messages.Enumerations.Node_Remove:
                    msg = new KeyCommon.Messages.Node_Remove();
                    cb = Worker_NodeRemove;
                    break;
                case KeyCommon.Messages.Enumerations.Node_Create:
                    msg = new Keystone.Messages.Node_Create();
                    cb = Worker_NodeCreate;
                    break;
                case KeyCommon.Messages.Enumerations.Node_MoveChildOrder:
                    msg = new KeyCommon.Messages.Node_MoveChildOrder();
                    cb = Worker_NodeMoveOrder;
                    break;
                case KeyCommon.Messages.Enumerations.Node_InsertUnderNew:
                    msg = new KeyCommon.Messages.Node_InsertUnderNew();
                    cb = Worker_InsertUnderNew;
                    break;
                case KeyCommon.Messages.Enumerations.Node_RenameResource:
                    msg = new KeyCommon.Messages.Node_RenameResource();
                    cb = Worker_NodeRenameResource;
                    break;
                case KeyCommon.Messages.Enumerations.Node_ReplaceResource:
                    msg = new KeyCommon.Messages.Node_ReplaceResource();
                    cb = Worker_NodeReplaceResource;
                    break;
                case KeyCommon.Messages.Enumerations.Node_ChangeParent:
                    msg = new KeyCommon.Messages.Node_ChangeParent();
                    cb = Worker_NodeChangeParent;
                    break;
                case KeyCommon.Messages.Enumerations.NodeChangeState:
                    msg = new KeyCommon.Messages.Node_ChangeProperty();
                    cb = Worker_NodeChangeProperty;
                    break;
                case KeyCommon.Messages.Enumerations.Geometry_ChangeProperty:
                    msg = new KeyCommon.Messages.Geometrty_ChangeGroupProperty();
                    cb = Worker_GeometryChangeGroupProperty;
                    break;
                case KeyCommon.Messages.Enumerations.NodeGetState:
                    msg = new KeyCommon.Messages.Node_GetProperty();
                    cb = Worker_NodeGetProperty;
                    break;
                case KeyCommon.Messages.Enumerations.Entity_Move:
                    msg = new KeyCommon.Messages.Entity_Move();
                    cb = Worker_EntityMove;
                    break;
                // Entity_GetCustomProperties - only required on Server (eg LoopbackServer.cs)
                //case KeyCommon.Messages.Enumerations.Entity_GetCustomProperties :
                //    break;
                case KeyCommon.Messages.Enumerations.Entity_SetCustomProperties:
                    msg = new KeyCommon.Messages.Entity_SetCustomProperties();
                    cb = Worker_EntitySetCustomProperties;
                    break;
                case KeyCommon.Messages.Enumerations.Entity_ChangeCustomPropertyValue:
                    msg = new KeyCommon.Messages.Entity_ChangeCustomPropertyValue();
                    cb = Worker_EntityChangeCustomProperty;
                    break;
                // case KeyCommon.Messages.Enumerations.NodeChangeShaderParameter:
                // Worker_EntityChangeShaderParameter
                case KeyCommon.Messages.Enumerations.Geometry_CreateGroup:
                    msg = new KeyCommon.Messages.Geometry_CreateGroup();
                    cb = Worker_GeometryCreateGroup;
                    break;
                case KeyCommon.Messages.Enumerations.Geometry_RemoveGroup:
                    msg = new KeyCommon.Messages.Geometry_RemoveGroup();
                    cb = Worker_GeometryRemoveGroup;
                    break;
                case KeyCommon.Messages.Enumerations.Geometry_ResetTransform:
                    msg = new KeyCommon.Messages.Geometry_ResetTransform();
                    cb = Worker_Geometry_ResetTransform;
                    break;
                case KeyCommon.Messages.Enumerations.NodeChangeFlag:
                    msg = new KeyCommon.Messages.Node_ChangeFlag();
                    cb = Worker_NodeChangeFlag;
                    break;
                // OBSOLETE - users should use Node_Create() instead
                //case KeyCommon.Messages.Enumerations.AddLight:
                //    msg = new KeyCommon.Messages.Scene_LoadLight();
                //    cb = Worker_AddLight;
                //    break;
                case KeyCommon.Messages.Enumerations.Task_Create:
                    msg = new Game01.Messages.Task_Create();
                    cb = Worker_Task_Create;
                    break;
                case KeyCommon.Messages.Enumerations.GameObject_Create:
                    msg = new KeyCommon.Messages.GameObject_Create();
                    cb = Worker_GameObject_Create;
                    break;
                case KeyCommon.Messages.Enumerations.GameObject_ChangeProperties:
                    msg = new KeyCommon.Messages.GameObject_ChangeProperties();
                    cb = Worker_GameObject_ChangeProperties;
                    break;
                case KeyCommon.Messages.Enumerations.DeleteFileFromArchive:
                    msg = new KeyCommon.Messages.Archive_DeleteEntry();
                    cb = Worker_ArchiveDeleteFile;
                    break;
                case KeyCommon.Messages.Enumerations.RenameEntryInArchive:
                    msg = new KeyCommon.Messages.Archive_RenameEntry();
                    cb = Worker_ArchiveRenameEntry;
                    break;
                case KeyCommon.Messages.Enumerations.AddFolderToArchive:
                    msg = new KeyCommon.Messages.Archive_AddFolder();
                    cb = Worker_ArchiveAddFolder;
                    break;
                case KeyCommon.Messages.Enumerations.AddFileToArchive:
                    msg = new KeyCommon.Messages.Archive_AddFiles();
                    cb = Worker_ModImportFile;
                    break;
                case KeyCommon.Messages.Enumerations.Geometry_Add:
                    msg = new KeyCommon.Messages.Geometry_Add();
                    cb = Worker_Geometry_Add;
                    break;
                case KeyCommon.Messages.Enumerations.AddGeometryToArchive:
                    msg = new KeyCommon.Messages.Archive_AddGeometry();
                    cb = Worker_ModImportGeometryAsEntity;
                    break;
                case KeyCommon.Messages.Enumerations.InsertPrefab_Interior:
                    msg = new KeyCommon.Messages.Prefab_Insert_Into_Interior();
                    cb = Worker_Prefab_Insert_Interior;
                    break;
                case KeyCommon.Messages.Enumerations.InsertPrefab_Structure:
                    msg = new KeyCommon.Messages.Prefab_Insert_Into_Structure();
                    cb = Worker_Prefab_Insert_Into_Structure;
                    break;
                case KeyCommon.Messages.Enumerations.PlaceEntity_EdgeSegment :
                    msg = new KeyCommon.Messages.Place_Entity_In_EdgeSegment();
                    cb = Worker_Prefab_Insert_EdgeSegment;
                    break;
                    
                    
                case KeyCommon.Messages.Enumerations.Terrain_Paint:
                    msg = new KeyCommon.Messages.Terrain_Paint();
                    cb = Worker_Terrain_Paint;
                    break;
                case KeyCommon.Messages.Enumerations.CelledRegion_PaintCell:
                    msg = new KeyCommon.Messages.PaintCellOperation();
                    cb = Worker_CelledRegion_Paint;
                    break;
                case KeyCommon.Messages.Enumerations.TileMapStructure_PaintCell:
                    msg = new KeyCommon.Messages.TileMapStructure_PaintCell();
                    cb = Worker_TileMapStructure_Paint;
                    break;
                //case KeyCommon.Messages.Enumerations.PlaceWall_CelledRegion:
                //    msg = new KeyCommon.Messages.Place_Wall_Into_CelledRegion();
                //    break;
                //case KeyCommon.Messages.Enumerations.CelledRegion_PaintLink:
                //    msg = new KeyCommon.Messages.CelledRegion_PaintLink();
                //    cb = Worker_CelledRegion_Link;
                //    break;
                case KeyCommon.Messages.Enumerations.PrefabLoad:
                    msg = new KeyCommon.Messages.Prefab_Load();
                    cb = Worker_Prefab_Insert;
                    break;
                case KeyCommon.Messages.Enumerations.PrefabSave:
                    msg = new KeyCommon.Messages.Prefab_Save();
                    cb = Worker_PrefabSave;
                    break;
                case KeyCommon.Messages.Enumerations.GeometrySave:
                    msg = new KeyCommon.Messages.Geometry_Save();
                    cb = Worker_Geometry_Save;
                    break;
                case KeyCommon.Messages.Enumerations.Simulation_Spawn:
                    msg = new KeyCommon.Messages.Simulation_Spawn();
                    cb = Worker_Spawn;
                    break;
                default:
                    msg = null;
                    cb = null; ;
                   
                    Debug.WriteLine("FormClient.UserMessageReceived() - Unsupported message type '" + ((KeyCommon.Messages.Enumerations)commandID).ToString() + "'.");
                    break;
            }

            return msg;
        }

        public void UserMessageSending(KeyCommon.Messages.MessageBase message)
        {
            message.SetFlag(KeyCommon.Messages.Flags.SourceIsClient);

            if (message.HasFlag(KeyCommon.Messages.Flags.ConfirmationRequired))
            {
                mUnconfirmedCommands.Add(message.UUID, message);
            }
        }

        //    // ICOmmands do have a Error command state we check.  So ICommand's themselves have the results.
        //    // Consider a SetTranslation()  when would a collision test be done?
        //    // Ideally immediately after the attempt so we can check for collide?  How do we integrate that with JigLibX
        //      // Well in Edit mode there's no physics when SetTranslation
        //     // but at arcade runtime, when translating a player we compute a final position then in Simulation.Update() we
        //    // check collision?  Maybe game uses SetMove() and not just SetTranslation()

        // TODO: in our current code. loopback sends this to here because we've specified it as the handler for
        // user data messages, however we should send the Connection too and then we can derive the Player object
        // as well as the 

       
        const int DATA_TYPE_MESSAGE = 0;
        const int DATA_TYPE_FILE_DATA = 1;
        const int DATA_TYPE_FILE_STREAM = 2;
        const int DATA_TYPE_CHUNK = 3;

        private void ServerMessageReceivedGameSpecific(int commandID, NetChannel channel, NetBuffer buffer)
        {
            // typically here we just want to apply the server response. This occurs on the main thread and doesn't require us to use worker functions.
            Game01.Enums.UserMessage command = (Game01.Enums.UserMessage)commandID;


            switch (command)
            {
                case Game01.Enums.UserMessage.Game_AttackResults:
                    // todo: attack results need to change properties on the target EntityID.
                    // these property changes can then be handled by the script and initiate any requisite FX
                    // the loopback server should be able to verify that a weapon isAimed and valid for firing at a specified targetID

                    // todo: here we apply the custom property changes and those in turn trigger events within the script
                    Game01.Messages.AttackResults results = new Game01.Messages.AttackResults();
                    results.Read(buffer);


                    // todo: applying of hit results should be done in main thread and I think here since we dont assign a callback to handle the message, is the main thread
                    if (results.Malfunction || results.Hit)
                    {
                        Entity weapon = (Entity)Repository.Get(results.WeaponID);
                        for (int i = 0; i < results.Results.Length; i++)
                        {
                            int[] dummy;
                            Entity target = (Entity)Repository.Get(results.Results[i].EntityID);
                            // todo: perhaps apply wear & tear on the weapon after each shot and based on the craftsmanship
                            if (results.Hit)
                            {
                                switch (weapon.UserTypeID)
                                {
                                    case (int)Game01.Enums.COMPONENT_TYPE.WEAPON_LASER:
                                        string beamID = AppMain.mScriptingHost.EntityAPI.FindDescendantByName(weapon.ID, "beam");
                                        Entity beam = (Entity)Repository.Get(beamID);

                                        // compute a better beam length using ray cast.  
                                        // This also allows us to find a better ImpactPoint for rendering VisualFX like particlesystems and texture cycles
                                        Vector3d rayDirection = Vector3d.Normalize (weapon.DerivedRotation.Forward()); // beam.DerivedRotation.Forward()
                                        Ray r = new Ray(beam.DerivedTranslation, rayDirection);

                                        BoundingBox box = target.SceneNode.BoundingBox;
                                        double d1, d2;
                                        // TODO: our weapon is firing when the target is out of range.  I think that is the real problem
                                        //       with the failed assertion.
                                        // TODO: the intersect range set below matters!  In a test,  i had a vehicle attempting
                                        //       to fire at another vehicle in another zone and the below box.Intersects() will fail Assertion
                                        double beamRange = AppMain.FARPLANE; // (double)beam.GetCustomPropertyValue("range");
                                        bool intersects = box.Intersects(r, 0.1d, beamRange, out d1, out d2);
                                        System.Diagnostics.Debug.Assert(intersects == true);
                                        double length = Math.Min (d1, d2); //  results.DistanceToTarget
                                        beam.SetCustomPropertyValue("length", length, false, false, out dummy);
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            int hitpoints = (int)target.GetCustomPropertyValue("hitpoints");
                            int damage = results.Results[i].Damage;
                            hitpoints -= damage;
                            bool runRule = true;
                            bool raiseEvent = true;
                            target.SetCustomPropertyValue("hitpoints", hitpoints, runRule, raiseEvent, out dummy);
                        }
                    }
                    else
                    {
                        // todo: set the beam entity's range to max range for that weapon

                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Create appropriate message and then wire up the correct execution callback functions based on the message type.
        /// Since our execution is decoupled from the command and only assigned after the message
        /// is created and when the command is created, we can have different versions of the callbacks
        /// depending on whether it's the client or server responding to the message
        /// </summary>
        /// <param name="commandID"></param>
        /// <param name="channel"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="buffer"></param>
        private void UserMessageReceived(int commandID, NetChannel channel, NetBuffer buffer)
        {

            if (commandID > (int)KeyCommon.Messages.Enumerations.UserMessages)
            {
                this.ServerMessageReceivedGameSpecific(commandID, channel, buffer);
                return;
            }
            // based on the command received, wire up the command worker function and completion functions
            Amib.Threading.WorkItemCallback cb = null;
            KeyCommon.Messages.MessageBase msg = CreateMessage(commandID, out cb);

            FormPleaseWait progress = new FormPleaseWait();

            // TODO: if this message needs to be confirmed first (determined by the type of message)
            // a flag that indicates it's unconfirmed, then we must postpone msg.Read(buffer) 
            // and instead store the buffer, store the msg, store the callback function
            // and then if confirmed, we construct the cmd and simply invoke the callback
            // thusly:  cb(cmd);
            // since we'll already be processing the confirmation message in a worker thread 
            // there is no need to queue it. 

            if (msg == null) return;
            msg.Read(buffer);

            Keystone.Commands.Command cmd = new Keystone.Commands.Command(msg);

            // .BeginExecute results in this command being enqueued to SmartThreadPool and so will not
            // execute on this current thread.
            cmd.BeginExecute(cb, CommandCompleted, progress); // TODO: we should pass an undo here too
                                                                //if (progress != null)
                                                                //    progress.ShowDialog(this);

        }

        /// <summary>
        /// This function is called prior to updating the scene (see AppMain.MainLoop()), 
        /// advancing the simulation and rendering it.
        /// Thus we can modify the scene without any worry about interupting the scene 
        /// while it's rendering.  This is ideally where all modifications of the scene
        /// should occur.  We should never directly modify properties on scene node objects
        /// and instead only modify via our changeproperty commands and other commands which
        /// eventually result in the final scene modifications here (TODO: although i think that's
        /// not 100% true yet, currently our worker threads are still making some modifications and
        /// aren't just caching the computation results and letting those results get applied here...)
        /// We can also potentially thread this with the Parallel Task extensions.
        /// </summary>
        /// <remarks>
        /// In SINGLE THREADED mode, this function occurs on the main GUI thread.
		/// In MULTI-THREADED mode, this function occurs on dedicated GAMELOOP thread which is _not_ the same as GUI thread.
        /// </remarks>
        internal override void ProcessCompletedCommandQueue()
        {
            Keystone.Commands.Command command = null;

            Amib.Threading.IWorkItemResult[] completedResults;
            lock (mCompletedQueueLock)
            {
                if (mCompletedCommands == null || mCompletedCommands.Count == 0) return;

                completedResults = mCompletedCommands.ToArray();
                mCompletedCommands.Clear();
            }

            // TODO: when we get .net 4.0\vs.net 2010 we can try to Parallel.ForEach () 
            // http://blogs.lessthandot.com/index.php/Architect/EnterpriseArchitecture/visual-studio-2010-concurrency-profiling
            foreach (Amib.Threading.IWorkItemResult result in completedResults)
            {
                if (result.Exception == null)
                {
                    // .Result != .State - it's very important to remember this since CommandSuccess 
                    // messages will have as .Result the inner command that was executed
                    command = (Keystone.Commands.Command)result.Result; 
                    KeyCommon.Messages.MessageBase message = command.Message;
                    object workerProduct = command.WorkerProduct;

                    if (command.State == State.ExecuteError)
                    {
                        Debug.WriteLine("FormMain.Commands.ProcessCompletedCommandQueue() - Error executing command '" + command.GetType().Name);
                        // nothing to handle here, nothing to push on stack
                        // TODO: if a command errors and sets this state, it should've already unrolled anything it did
                        command.EndExecute(); // we still call endexecute so any cleanup code (such as hiding of a progress dialog) can occur
                        return;
                    }

                    command.EndExecute();

                    // recheck for ExecuteError after .EndExecute()
                    if (command.State == State.ExecuteError)
                        return;


                    if (command.CanUndo)
                    {
                        if (command.State == State.ExecuteCompleted)
                        {
                            if (command.Undo != null)
                            {
                                _undo.Push(command.Undo);
                                // TODO: send an event when undo empty
                                // or not empty?
                                // buttonItemUndo.Enabled = true;
                            }
                            else
                            {
                                _redo.Push(command.Message);
                                // TODO: send an event when _redo empty
                                // or not empty?
                                //buttonItemRedo.Enabled = true;
                            }
                        }
                    }

                    command.State = State.Ready;

                    // be somewhat nice if AddNode was the only kind and that it used a delegate to specify which
                    // worker to use rather than inheritance.  But that would complicate deserialization of commands from the net.
                    // But maybe we should use a switch( nodeType) and then make calls to static methods based on type
                    // such as Model.Create ()  which handles a lot of our boilerplate model creation code.  or Light.Create()
                    // etc.  HOWEVER, keep in mind that having seperate Command implementations is also good for serialization
                    // over the wire as it simplifies our Read() Write() methods.  
                    //     if (message is AddNode || message is RemoveNode)
                    //     //if (command is EditCommand)  // whats the difference between an EditCommand, a SpawnCommand
                    //         // or some other form of "AddLight" command or something?  Some circumstances the server may require
                    //         // a light be spawned but not for it to be written to the scene?
                    //         // certainly you could have a scenario where someone is modifying the scene whilst it's being played by others?
                    //         // As in real-time world building... comets or asteroids destroied persistently, moons, planets, stars destroyed?
                    //         // who knows?  So there needs to be a better way than just differentiating by 'EditCommand'...
                    //         // well, perhaps simply... we explicitly state that the thing we want added to the scene should get written to the
                    //         // scene.  Then we can verify the permissions that the write is allowed.  Based on both who sent the command
                    //         // and perhaps the context/appropriateness of the command given the situation.  But...
                    //         // if the caller is setting flags, then what's to stop them from using inappropriate flags?

                    //         // Ok, first clarification, permissions are set on the target thing.  Such as a file, or in our case
                    //         // the scene database, or perhaps a user's own vehicle.  Permissions are checked against the request
                    //         // against the target object based on the access parameters of the target object.  So basically permissions
                    //         // are not set in the ICommand object.  That is obviously a security. problem.  Thus, SceneInfo perhaps 
                    //         // might contain access levels based on whether it's Edit, Runtime, etc... plus various other permission flags
                    //         // such as for instance whether all Editing of the scene is disabled... or whether Moderation is enabled
                    //         // and chat gets auto-filtered, etc.
                    //     {
                    //         // validate permissions for write
                    ////         if (command.WriteChanges)  // WriteChanges bool is a user request stored in bitflag.  The Write still needs to be checked for permissions against the SceneBase.Security object
                    //         {                                       // in this way AddLight can be used to add a light to the scene, or a temporary one that is spawned.

                    //             // TODO: the connectionID is not associated with the command.. how did we do that in Evo?
                    //             //         in server environment we do need to verify the sender to confirm write access
                    //             // well it would have access to Lidgren.Network.NetConnectionBase  sender;
                    //             // maybe we can just attach that to the command itself.  After the command is deserialized
                    //             // we set it's Sender reference to the connection.  We can put this off for now, it's a relatively minor change
                    //             // regardless of when we implement it.  But i think it could be useful for the command object itself to 
                    //             // have a reference to it's sender.
                    ////             Core._Core.SceneManager.ActiveScene.XMLDB.Write(command.Parent); // all Add/Remove operation require writes start at parent node.
                    //                                                          // nodes that edit just one node (as long as no children or added/removed) can be written without
                    //                                                          // recursing.  
                    //         }
                    //     }


                    //    Trace.WriteLine("Command completed successfully.", result.State.GetType().Name);

                    //although, we ideally want to be able to execute multiple queues by max (ideal) threacount
                    // but at the same time, we dont want to have issues with synchronization
                    // And at the same time, we want to preserve serialization of the applictaion of commands even as some are
                    // completed at different times.  That im not 100% sure how to deal with easily.  We could/should probably
                    // ignore that for now *sigh*
                    // Well... we do know that as far as execution goes, we have a situation where our Commands actually carry out
                    // functions.  Some can perform asychronously out of order and then rely on a subsequent command here to
                    // run any other commands in sequence.  (like adding things to the scene)
                    // So it seems to me, it's a matter of sequencing.  Indeed, we can queue all commands at once and those will execute
                    // but we cannot apply those changes to the scene out of order.  So it's a lot like TCP.  They can complete (arrive)
                    // in any order, but those aspects of the command that actually apply the results of the computation to the scene
                    // must be queued in order until they can all be applied at once serially.  So we can create nodes, load resources
                    // but then applying those to the tree must be reserved.  How does that work along with undo?


                    // if an entity was imported, we need to add it to the browser
                    if (message is KeyCommon.Messages.MissionResult)
                    {
                        KeyCommon.Messages.MissionResult missionResult = (KeyCommon.Messages.MissionResult)message;
                        if (missionResult.Success)
                        {
                            // todo: since this messagebox is being shown on main thread, will it stop the game loop?
                            System.Windows.Forms.MessageBox.Show("Mission Success!", "Mission Success", System.Windows.Forms.MessageBoxButtons.OK);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Mission Failed!", "Mission Failed.", System.Windows.Forms.MessageBoxButtons.OK);
                        }
                    }
                    else if (message is KeyCommon.Messages.NotifyPlugin_NodeSelected)
                    {
                        KeyCommon.Messages.NotifyPlugin_NodeSelected selected = (KeyCommon.Messages.NotifyPlugin_NodeSelected)message;
                        System.Diagnostics.Debug.WriteLine("Notifying plugin " + selected.Typename + " with id = " + selected.NodeID);

                        Workspaces.WorkspaceBase3D workspace3d = mWorkspaceManager.CurrentWorkspace as Workspaces.WorkspaceBase3D;
                        if (workspace3d == null) return;

                        workspace3d.NotifyPlugin();

                        return;
                    }
                    else if (message is KeyCommon.Messages.NotifyPlugin_ProcessEventQueue)
                    {
                        KeyPlugins.AvailablePlugin plugin = AppMain.PluginService.SelectPlugin("Editor", "Entity");
                        plugin.Instance.ProcessEventQueue();
                    }

                    else if (message is KeyCommon.Messages.Scene_New)
                    {
                        KeyCommon.Messages.Scene_New newScene = (KeyCommon.Messages.Scene_New)message;

                        string username = AppMain._core.Settings.settingRead("network", "username");
                        string password = AppMain._core.Settings.settingRead("network", "password");

                        SimulationJoinRequest(username, password, newScene.FolderName, "", false);

                        // what does the save folder location look like and how do we delete it?
                        // SceneLoadRequest(newScene.FolderName, mStartingRegionID);


                        // NOTE: the below gets set in this if/else block during handling of message Keystone.Messages.Scene_Load
                        // march.2.2024 - moved from scene and after processs completed so that the scene is fully loaded
                        //mScene = (Scene)command.WorkerProduct;
                        //this.mScene.Simulation.Running = true;

                        return;
                    }
                    else if (message is KeyCommon.Messages.Scene_NewTerrain)
                    {
                        KeyCommon.Messages.Scene_NewTerrain newTerrainScene = (KeyCommon.Messages.Scene_NewTerrain)message;
                        string username = AppMain._core.Settings.settingRead("network", "username");
                        string password = AppMain._core.Settings.settingRead("network", "password");
                        SimulationJoinRequest(username, password, newTerrainScene.FolderName, "", false);

                        //SceneLoadRequest(newTerrainScene.FolderName, mStartingRegionID);

                        // march.2.2024 - moved from scene and after processs completed so that the scene is fully loaded
                        //SceneLoadRequest(newTerrainScene.FolderName, mStartingRegionID);

                        //mScene = (Scene)command.WorkerProduct;
                        //this.mScene.Simulation.Running = true;
                        return;
                    }
                    else if (message is KeyCommon.Messages.Scene_NewUniverse)
                    {
                        // todo: shouldn't empty Zones without a Star, have a default directional light?
                        KeyCommon.Messages.Scene_NewUniverse newUniverse = (KeyCommon.Messages.Scene_NewUniverse)message;

                        string username = AppMain._core.Settings.settingRead("network", "username");
                        string password = AppMain._core.Settings.settingRead("network", "password");

                        // TODO: why doesn't this first trigger a Scene_Load?
                        SimulationJoinRequest(username, password, newUniverse.FolderName, "", false);
                        // what does the save folder location look like and how do we delete it?
                        //SimulationJoin(newUniverse.FolderName, false);
                        // march.2.2024 - moved from scene and after processs completed so that the scene is fully loaded
                        // NOTE: the following are set in this if/else block under  KeyCommon.Messages.Scene_Load completed
                        //mScene = (Scene)command.WorkerProduct;
                        //this.mScene.Simulation.Running = true;
                        return;
                    }
                    else if (message is KeyCommon.Messages.Floorplan_New)
                    {
                        KeyCommon.Messages.Floorplan_New newFloorplan = (KeyCommon.Messages.Floorplan_New)message;

                        /// wait,arent' we already authenticating with the auth server?
                        string username = AppMain._core.Settings.settingRead("network", "username");
                        string password = AppMain._core.Settings.settingRead("network", "password");

                        SimulationJoinRequest(username, password, newFloorplan.FolderName, "", false);

                        // march.2.2024 - moved from scene and after processs completed so that the scene is fully loaded
                        mScene = (Scene)command.WorkerProduct;
                        this.mScene.Simulation.Running = true;
                        return;
                    }
                    //// TODO: I think Scene_Load_Request message should only be handled by loopback and then a Scene_Load() command should be set to client for handling
                    //// Scene_Load_Request called when loading existing scene. 
                    //// In fact, i think all code paths do that now.  This Scene_Load_Request processing is obsolete.
                    //else if (message is KeyCommon.Messages.Scene_Load_Request)
                    //{
                    //    KeyCommon.Messages.Scene_Load_Request sceneRequest = (KeyCommon.Messages.Scene_Load_Request)message;
                    //    // the problem here is when loading a Single Region scene that was
                    //    // started initially as "edit mode" but then on Load we set 
                    //    // edit mode = false.  For our purposes, we want all single region scenes
                    //    // to load as edit mode = true since for release version, only our mutli-region games
                    //    // have edit mode = false.
                    //    //sceneRequest.
                    //    //SimulationJoin(sceneRequest.FolderName, false);
                    //    return;
                    //}
                    //else if (message is KeyCommon.Messages.Scene_Load)
                    //{
                    //    // NOTE: This message is received from server AFTER we call SimulationJoinRequest()
                    //    // This is because we rely on the server to tell the client the startingRegionID
                    //    // so that the client knows what parts of the scene to page in.
                    //    KeyCommon.Messages.Scene_Load load = (KeyCommon.Messages.Scene_Load)message;
                    //    mScene = (Scene)command.WorkerProduct;
                    //    mOpenSceneInProgress = false;
                    //    // march.2.2024 - moved from scene and after processs completed so that the scene is fully 

                    //    // TODO: ACTUALLY, the server should send the clients their respective starting regions
                    //    // todo: eventually i think we need to get the mission data from the server/loopback
                    //    // todo: if there is a mission to load, we need to grab the player's spawnpoint regionID and set that as the startingRegion
                    //    // perhaps we can do Mission mission = new Mission();
                    //    // mission.Load (missionPath);
                    //    // CurrentMission = mission;
                    //    // if (CurrentMission == null)
                    //    // {
                    //    //      if (MultiZoneScene) mStartingRegion = "Zone,0,0,0";
                    //    // }
                    //    // else 
                    //    // {
                    //    //      // get the player spawnpoint which must exist
                    //    //      mStartingRegionID = CurrentMission.PlayerSpawnPoint.Region;
                    //    // }
                    //    // todo: then, we can load mission data such as spawnpoints into the scene

                    //    // In true multiplayer, missions are loaded by the server and are updated and processed by the server's
                    //    // simulation loop.  However, for loopback, the client needs to do this.  Loopback can potentially
                    //    // load a mission to determine a startingRegion, but it cannot assign the mission it loads to the Simulation.CurrentMission
                    //    // because the client's scene will not have been loaded yet (since client needs to know the startingRegion before
                    //    // it can load the scene!)
                    //    if (mIsLoopback)
                    //        LoadMission(load.FolderName, load.MissionName);

                    //    this.mScene.Simulation.Running = true;
                    //    return;
                    //}

                    else if (message is KeyCommon.Messages.Simulation_Join) // join has been approved on server side and we get this message client side
                    {
                        KeyCommon.Messages.Simulation_Join join = (KeyCommon.Messages.Simulation_Join)message;
                        // NOTE: below is no longer used.  After we receive this message we wait for
                        //       Scene_Load command from server.
                        //SimulationJoin(join.FolderName, false);
                        // this.mScene.Simulation.Running = true;

                        mScene = (Scene)command.WorkerProduct;
                        mOpenSceneInProgress = false;

                        if (mIsLoopback)
                            this.mScene.Simulation.LoadMission(join.FolderName, join.MissionName);

                        this.mScene.Simulation.Running = true;
                        return;

                    }
                    else if (message as KeyCommon.Messages.Transfer_Entity_File != null)
                    {
                        KeyCommon.Messages.Transfer_Entity_File transfer = (KeyCommon.Messages.Transfer_Entity_File)message;
                        Node node = (Node)command.WorkerProduct;

                        if (node != null)
                        {
                            Node parentNode = (Node)Repository.Get(mFileParentID);
                            SuperSetter setter = new SuperSetter(parentNode);
                            setter.Apply(node);
                        }
                    }
                    else if (message as KeyCommon.Messages.Prefab_Load != null)
                    {
                        KeyCommon.Messages.Prefab_Load load = (KeyCommon.Messages.Prefab_Load)message;
                        Keystone.Entities.Entity parent = (Keystone.Entities.Entity)Repository.Get(load.ParentID);
                        Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parent);

                        if (workerProduct != null) // workerProduct will be null if Prefab_Load is fragmented and not fully assembled in worker
                        {
                            if (workerProduct is Keystone.Behavior.Behavior)
                            {
                                Keystone.Behavior.Behavior behavior = (Keystone.Behavior.Behavior)workerProduct;
                                setter.Apply(behavior);
                            }
                            else if (workerProduct is Entity)
                            {
                                Entity entity = (Entity)workerProduct;
                                setter.Apply(entity);
                            }

                            // todo: do we want/need to save here?
                            SaveNode(load.ParentID);
                        }
                    }
                    else if (message as KeyCommon.Messages.Simulation_Spawn != null)
                    {
                        KeyCommon.Messages.Simulation_Spawn spawn = (KeyCommon.Messages.Simulation_Spawn)message;
                        Entity parent = (Entity)Repository.Get(spawn.ParentID);
                        Entity child = (Entity)workerProduct;
                        SuperSetter setter = new SuperSetter(parent);
                        setter.Apply(child);

                        if (child is Container)
                        {
                            Container container = (Container)child;
                            if (container.Interior != null)
                            {
                                // inform the server that we are ready to receive spawns of dynamic Entities such as Crew
                                KeyCommon.Messages.RegionPageComplete complete = new KeyCommon.Messages.RegionPageComplete();
                                complete.RegionID = container.Interior.ID;

                                SendNetMessage(complete);
                            }
                        }
                        // NOTE: we do NOT save the parent because spawned Entities are saved seperately and are not considered part of the static scene
                        //SaveNode(spawn.ParentID);
                    }
                    else if (message is KeyCommon.Messages.Prefab_Insert_Into_Interior)
                    {
                        KeyCommon.Messages.Prefab_Insert_Into_Interior insert = (KeyCommon.Messages.Prefab_Insert_Into_Interior)message;
                        Keystone.Entities.Entity parent = (Keystone.Entities.Entity)Repository.Get(insert.ParentID);

                        System.Diagnostics.Debug.WriteLine("ProcessCompletedCommandQueue() Inserting into cell " + insert.Index);
                        if (insert.ComponentType == KeyCommon.Messages.ComponentType.EdgeComponent)
                        {
                            // if there is a wall or existing EdgeSegment, delete it by setting StyleID = -1 and then Applying it
                            Interior interior = (Interior)parent;
                            uint edgeID = insert.Index;
                            EdgeStyle style = new EdgeStyle();
                            style.StyleID = -1;
                            interior.ApplyEdgeSegmentStyle(edgeID, style);
                        }

                        Entity child = (Entity)workerProduct;
                        if (child != null)
                        {
                            // todo: verify the loopback is validating the placement. It's not... we are doing validation in the worker and that's wwrong.
                            // todo: i should be using SuperSetter
                            if (insert.Cancel == false)
                                parent.AddChild(child);

                            SaveNode(insert.ParentID);
                        }
                    }
                    else if (message is KeyCommon.Messages.Prefab_Insert_Into_Structure) // eg. chairs, reactors, engines
                    {
                        KeyCommon.Messages.Prefab_Insert_Into_Structure insert = (KeyCommon.Messages.Prefab_Insert_Into_Structure)message;
                        Keystone.Entities.Entity parent = (Keystone.Entities.Entity)Repository.Get(insert.ParentStructureID);


                        Entity[] children = (Entity[])workerProduct;
                        if (children != null)
                        {
                            for (int i = 0; i < children.Length; i++)
                                if (insert.Cancel[i] == false)
                                    parent.AddChild(children[i]);

                            // save outside of the loop
                            SaveNode(insert.ParentStructureID);
                        }
                    }
                    else if (message as KeyCommon.Messages.Place_Entity_In_EdgeSegment != null) // eg. doors
                    {
                        KeyCommon.Messages.Place_Entity_In_EdgeSegment place = (KeyCommon.Messages.Place_Entity_In_EdgeSegment)message;
                        Keystone.Portals.Interior parent = (Keystone.Portals.Interior)Repository.Get(place.ParentStructureID);

                        // NOTE: RegisterComponent results on "OnAddedToParent" which will allow the door entity
                        //       to do things like register itself to EdgeSegment's dictionary as well as configure
                        //       CSG/stencil linkages with the underlying wall segment.
                        Entity[] children = (Entity[])workerProduct;
                        if (children != null)
                        {
                            for (int i = 0; i < children.Length; i++)
                                if (place.Cancel[i] == false)
                                    parent.AddChild(children[i]);

                            // save outside of the loop
                            SaveNode(place.ParentStructureID);
                        }
                    }
                    else if (message as KeyCommon.Messages.TileMapStructure_PaintCell != null) // eg. exterior away team terrain, floors, walls)
                    {
                        KeyCommon.Messages.TileMapStructure_PaintCell paintTile = (KeyCommon.Messages.TileMapStructure_PaintCell)message;
                        Node node = (Node)Repository.Get(paintTile.ParentStructureID);
                        Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)node;

                        // TODO: my .obj loader might need to reverse the normal when loading or dirt.kgbsegment. Its fine for the voxels, but not for the dirt

                        bool saveNode = false;
                        bool saveResource = false;

                        for (int i = 0; i < paintTile.Indices.Length; i++)
                        {
                            // skip this index if it was flagged as canceled by our Worker 
                            // typically it would be canceled if it failed an out of bounds check 
                            if (paintTile.Cancel[i]) continue;

                            switch (paintTile.LayerName)
                            {
                                case "obstacles":

                                    break;
                                case "layout":
                                    {
                                        string segmentPath = (string)paintTile.PaintValue;
                                        int floorLevel = paintTile.FloorLevel;

                                        if (paintTile.Operation == KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.ADD)
                                        {
                                            structure.Layer_SetValue(floorLevel, paintTile.LayerName, paintTile.Indices[i], segmentPath);
                                        }
                                        else
                                        {
                                            // else if digging, we want to Layer_SetValue() an EMPTY value for the LevelIndex
                                            // that was mouse picked.  We should rely on the Layer_SetValue() to run any script that would also
                                            // add a new lower level if necessary to avoid punching through map.
                                            structure.Layer_SetValue(floorLevel, paintTile.LayerName, paintTile.Indices[i], null); // <-- send null and not segmentPath
                                                                                                                                   // a negative levelIndex will result in a new floor being added beneath the lowest
                                            structure.Layer_SetValue(--floorLevel, paintTile.LayerName, paintTile.Indices[i], segmentPath);
                                        }

                                        saveResource = true;
                                        saveNode = true; // TODO: tiles are not child components/entities, do we even need to save the structure after this?
                                        break;
                                    }
                            }
                        }

                        // save outside of the loop
                        // TODO: should be done in background thread

                        if (saveResource)
                            //structure.SaveTVResource(null);
                            Keystone.IO.ClientPager.QueuePageableResourceSave(structure, null, null, false);
                        if (saveNode)
                            SaveNode(structure);
                    }
                    else if (message is KeyCommon.Messages.PaintCellOperation) // eg. interior floors, walls
                    {
                        KeyCommon.Messages.PaintCellOperation paintCell = (KeyCommon.Messages.PaintCellOperation)message;
                        Node node = (Node)Repository.Get(paintCell.ParentCelledRegionID);
                        Interior interior = (Interior)node;

                        bool saveNode = false;
                        bool saveResource = false;
                        bool updateConnectivity = false;
                        int connectivityLayer = -1;

                        Keystone.Portals.EdgeStyle[] workerResults = (Keystone.Portals.EdgeStyle[])command.WorkerProduct;

                        for (int i = 0; i < paintCell.Indices.Length; i++)
                        {
                            // skip this index if it was flagged as canceled by our Worker 
                            // typically it would be canceled if it failed an out of bounds check 
                            // TODO: for some tiles, we should cancel all if even one element is Cancel[i] = true;
                            if (paintCell.Cancel[i]) continue;

                            switch (paintCell.LayerName)
                            {
                                case "boundaries":
                                    interior.Layer_SetValue(paintCell.LayerName, paintCell.Indices[i], (bool)paintCell.PaintValue);
                                    saveResource = true;
                                    // note: we do not save entire celledRegion for "boundaries" changes as no entity has been added/removed from interior
                                    saveNode = false;
                                    break;
                                case "powerlink":
                                    // TODO: is there a way to send these calls all at once?  for links doing this piecemeal slows down our ability to update link connections
                                    // between producers and consumers as it it done for each index
                                    // TODO: powerlink and all links, should just be in footprint but using different bitflags
                                    interior.Layer_SetValue(paintCell.LayerName, paintCell.Indices[i], (bool)paintCell.PaintValue);
                                    saveResource = true;
                                    // note: we do not save entire celledRegion for "powerlink" changes as no entity has been added/removed from interior
                                    saveNode = false;
                                    break;
                                case "tile style":
                                    {
                                        EdgeStyle style = (EdgeStyle)paintCell.PaintValue;
                                        interior.Layer_SetValue(paintCell.LayerName, paintCell.Indices[i], style);
                                        saveResource = true;
                                        saveNode = false; // tiles are not child components/entities so we do not need to serialize the Interior or Vehicle
                                        break;
                                    }
                                case "wall style":
                                    {
                                        updateConnectivity = true;
                                        // if (style.StyleID == -1)
                                        //{

                                        // TODO: And I think from there, i should only call inteior.Layer_SetValue()
                                        // even if that style is -1 for deleting an edge. 

                                        // DeleteWallModelView() should be a private method
                                        //    map = interior.DeleteWallModelView(paintCell.Indices[i], style);
                                        //}


                                        EdgeStyle style = (EdgeStyle)paintCell.PaintValue;

                                        // TODO: Layer_SetValue() calls a script OnCelledRegion_DataLayerValue_Changed()
                                        //       but the question is, what is the purpose of this?
                                        //       Well a key benefit is, by activating it it allows us to
                                        //       run code during real-time user paint the same way as during deserialization
                                        //       where's Worker() functions dont run and only the script is run.

                                        // Mesh creation and validation has already occurred in Worker_CelledRegion_Paint().  
                                        // Here we simply apply the style's visuals to the celledRegion
                                        // maps is needed by EntityAPI.CellMap_SetEdgeSegmentStyle
                                        // whereas "SegmentStyle" is used to dynamically generate a footprint based on the mesh dimensions
                                        style = workerResults[i]; //paintCell.WorkerResults[i]; // results generated earlier by Worker_CelledRegionPaint

                                        interior.Layer_SetValue(paintCell.LayerName, paintCell.Indices[i], style);
                                        saveResource = true;
                                        saveNode = false; // walls are not child components/entities so there is no need to serialize the Interior or Vehicle
                                        break;
                                    }
                            }
                        }

                        if (updateConnectivity)
                        {
                            // TODO: This absolutely cannot occur here.  It's taking too long when trying to build up and design our interior floorplans.  
                            //       Perhaps we add a toolbar button to "compile" interior after we're done laying it out in floorplan editor?
                            // TODO: we should queue this for threaded processing
                            // TODO: also our wall placing doesn't allow us to draw many walls in a row and so we end up having to place them one at a time
                            //       with UpdateConnectivity() being called each time.
                            // TODO: in the short term, i should add a toolbar icon to refresh connectivity rather than queuing and canceling, but I do like
                            //       the idea of generating connectivity in a background thread.  But i think with our command processor only allowing concurency of 1
                            //       means that while we are generating connectivity, no other commands can be performed.
                            //_core.CommandProcessor.QueueWorkItem();
                            // CelledRegion_UpdateConnectivity cmd = new CelledRegion_UpdateConnectivity();
                            //   interior.UpdateConnectivity();
                        }

                        // save outside of the loop and should this be done in background thread?
                        if (saveResource)
                        {
                            SaveResource(interior);


                        }

                        if (saveNode)
                            // TODO: background thread needed here too?  Maybe not.  I think we want save of XML on main thread and only when exiting the game or hitting a save point?
                            SaveNode(interior);
                    }
                    else if (message as KeyCommon.Messages.Node_Remove != null)
                    {
                        KeyCommon.Messages.Node_Remove remove = (KeyCommon.Messages.Node_Remove)message;
                        IGroup parent = (IGroup)Repository.Get(remove.mParentID);
                        Node[] nodes = (Node[])workerProduct;

                        for (int i = 0; i < nodes.Length; i++)
                        {
                            // if this is loopback, then the command will have come from client via plugin (aka ourselves - but not through one of our scripts) 
                            // and so we dont want to notify the plugin since that would be circular 
                            if (!remove.HasFlag(KeyCommon.Messages.Flags.SourceIsClient))
                                NotifyRemove(parent.ID, nodes[i].ID, nodes[i].TypeName);

                            // todo: remove any outstanding notifications on this entity in SceneBase.mChangedNodes

                            // NOTE: actual removal from scene occurs here in main thread
                            parent.RemoveChild(nodes[i]); // scenenode deletion is handled in Simulation.cs 


                            // notify all workspaces that a node has been removed
                            if (this.InvokeRequired)
                                this.Invoke((System.Windows.Forms.MethodInvoker)delegate
                                {
                                    mWorkspaceManager.NotifyNodeRemoved(parent.ID, nodes[i].ID, nodes[i].TypeName);
                                });
                            else
                                mWorkspaceManager.NotifyNodeRemoved(parent.ID, nodes[i].ID, nodes[i].TypeName);



#if DEBUG
                            if (nodes[i] as Entity != null)
                            {
                                Entity childEntity = (Entity)nodes[i];
                                Debug.Assert(childEntity.Parent == null);
                                Debug.Assert(childEntity.RefCount == 0);
                            }
                            // NOTE: If the node[i] is a TVResource node, upon refcount == 0 it will DisposeManagedResources() 
                            // and call relevant TVFactory.Delete* methods.
#endif
                        }
                        // TODO: why is save here and not in the worker?
                        // im not saying it should be saved in the worker, im just wondering why?
                        // is one reason because this ProcessCompleted() occurs on mainthread
                        // and so this way we can guarantee that saves are all serialized and
                        // there's no threading issues to worry about.
                        // Isn't the better way to serialize saves simply to have seperate 
                        // threadpool groups for our xmldb's with each of their groups only
                        // allowing 1 thread?
                        // TODO: i dont think we should save here at all.  save should come at "save points" perhaps
                        //       or upon app shutdown.  however, we do have issue of zone paging in out... zones would need to be saved before page out?
                        //       updates to "dynamic\thinking entities" within those Zones should be done against a "digest" or "entity system" version or upon their database entries.
                        //                       
                        SaveNode(remove.mParentID);

                    }
                    else if (message as Keystone.Messages.Node_Create != null)
                    {
                        // TODO: eventually we should verify that the created node actually even
                        // is connected to the plugin's current entity.... for now we'll just always
                        // have the event fire which updates the plugin gui.
                        Keystone.Messages.Node_Create create = (Keystone.Messages.Node_Create)message;

                        Node node = (Node)command.WorkerProduct;
                        Node parentNode = (Node)Repository.Get(create.DescendantNodeParents[0]);
                        SuperSetter setter = new SuperSetter(parentNode);
                        setter.Apply(node);

                        if (create.NodeCount == 0) return;

                        // todo: notify all workspaces that a node has been Created
                        if (this.InvokeRequired)
                            this.Invoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                mWorkspaceManager.NotifyNodeAdded(parentNode.ID, node.ID, node.TypeName);
                            });
                        else
                            mWorkspaceManager.NotifyNodeAdded(parentNode.ID, node.ID, node.TypeName);


                        // NOTE: We only have to save the first node, the loop is not necessary

                        //for (int i = 0; i < create.NodeCount; i++)
                        //{
                        // TODO: Here i'm saving the entity to the scene if it's Serializable == true... problem is
                        //       Vehicle does not have Serializble == false.  Also not testing if edit mode or if
                        //       the entity is dynamic object hosted only in the sqlite db.
                        // TODO: verify we can save prefabs when Serializable == false (do we temporarily set Serializable = true?)
                        SaveNode(node); // SaveNode(create.DescendantNodeParents[i]);

                        if (!create.HasFlag(KeyCommon.Messages.Flags.SourceIsClient))
                        {
                            int idIndex = create.NodeProperties[0].Properties.IndexOf("id");
                            string nodeID = (string)create.NodeProperties[0].Properties[idIndex].DefaultValue;

                            if (this.InvokeRequired)
                                this.Invoke((System.Windows.Forms.MethodInvoker)delegate { NotifyAdd(create.DescendantNodeParents[0], nodeID, create.DescendantNodeTypes[0]); });
                            else
                                NotifyAdd(create.DescendantNodeParents[0], nodeID, create.DescendantNodeTypes[0]);
                            //      int idIndex = create.NodeProperties[i].Properties.IndexOf("id");
                            //      string nodeID = (string)create.NodeProperties[i].Properties[idIndex].DefaultValue;

                            //       if (this.InvokeRequired)
                            //  		this.Invoke((System.Windows.Forms.MethodInvoker)delegate { NotifyAdd(create.DescendantNodeParents[i], nodeID, create.DescendantNodeTypes[i]); });
                            //else
                            //      	NotifyAdd(create.DescendantNodeParents[i], nodeID, create.DescendantNodeTypes[i]);
                        }
                        //}
                    }
                    else if (message as KeyCommon.Messages.Node_InsertUnderNew != null)
                    {
                        KeyCommon.Messages.Node_InsertUnderNew insert = (KeyCommon.Messages.Node_InsertUnderNew)message;
                        //TODO:
                        // SaveNode();
                        // NotifyAdd();

                    }
                    else if (message as KeyCommon.Messages.Node_ChangeParent != null)
                    {
                        KeyCommon.Messages.Node_ChangeParent changePar = (KeyCommon.Messages.Node_ChangeParent)message;

                        SaveNode(changePar.OldParentID); // we must save BOTH seperately
                        SaveNode(changePar.ParentID);    // or we must find a common parent!

                        Node node = (Node)Repository.Get(changePar.NodeID);
                        NotifyMove(changePar.OldParentID, changePar.ParentID, changePar.NodeID, node.TypeName);
                    }
                    else if (message as KeyCommon.Messages.Node_RenameResource != null)
                    {
                        KeyCommon.Messages.Node_RenameResource renameRes = (KeyCommon.Messages.Node_RenameResource)message;
                        Node parent = ((Node[])workerProduct)[0];
                        Node resource = ((Node[])workerProduct)[1];

                        // NOTE: if we just change the ID, then all parent nodes that reference this resource will be affected.
                        //       thus we assert that the refcount for the resource == 1.
                        System.Diagnostics.Debug.Assert(Repository.Get(renameRes.NewResourceID) == null);
                        System.Diagnostics.Debug.Assert(Repository.Get(renameRes.OldResourceID).RefCount == 1);

                        // NOTE: the source pagestatus should not change after changing it's id/resourceID

                        // NOTE: animation assignments should not be affected since they use the friendly name of the node.
                        Repository.RenameResource(resource, renameRes.NewResourceID);

                    }
                    else if (message as KeyCommon.Messages.Node_ReplaceResource != null)
                    {
                        KeyCommon.Messages.Node_ReplaceResource replaceRes = (KeyCommon.Messages.Node_ReplaceResource)message;

                        // todo: have the following nodes assigned to workerProduct in worker thread
                        Keystone.Elements.Node oldChild = (Keystone.Elements.Node)Keystone.Resource.Repository.Get(replaceRes.OldResourceID);
                        Keystone.Elements.IGroup parent = (Keystone.Elements.IGroup)Keystone.Resource.Repository.Get(replaceRes.ParentID);


                        Keystone.Elements.Node newChild =
                            Keystone.Resource.Repository.Create(replaceRes.NewResourceID, replaceRes.TypeName);

                        if (oldChild != null)
                            parent.RemoveChild(oldChild);
                        else
                            Debug.WriteLine("FormMain.Commands.ProcessCompletedCommandQueue() - No existing child to replace.  Continuing on and adding new child");

                        Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter((Node)parent);
                        // TODO: should only occur in out single threaded CommandCompleted 
                        setter.Apply(newChild);

                        // TODO: i think the SourceIsClient is irrelevant. Client commands
                        // still should notify to trigger the update the the GUI which otherwise
                        // is not allowed until server turns a client request into a real result
                        if (!replaceRes.HasFlag(KeyCommon.Messages.Flags.SourceIsClient))
                            NotifyPluginOfPropertyChange(replaceRes.NewResourceID, replaceRes.TypeName);

                        // note: changing a replaced resource DOES require the parent 
                        // to be saved because replacement of a resource node means add/remove
                        // is done under the hood.
                        SaveNode(replaceRes.ParentID);
                    }
                    else if (message is KeyCommon.Messages.Node_ChangeFlag)
                    {
                        KeyCommon.Messages.Node_ChangeFlag change = (KeyCommon.Messages.Node_ChangeFlag)message;
                        Node node = (Node)workerProduct;

                        if (change.FlagType == "Node")
                        {
                            foreach (string key in change.Flags.Keys)
                                node.SetFlagValue(key, change.Flags[key]);
                        }
                        else if (change.FlagType == "Model")
                        {
                            Keystone.Elements.Model model = (Keystone.Elements.Model)node;
                            foreach (string key in change.Flags.Keys)
                                model.SetFlagValue(key, change.Flags[key]);
                        }
                        else if (change.FlagType == "Entity")
                        {
                            Keystone.Entities.Entity entity = (Keystone.Entities.Entity)node;
                            foreach (string key in change.Flags.Keys)
                            {
                                bool previousFlag = entity.GetEntityFlagValue(key);
                                // NOTE: set the new flag prior to calling ManageGeneratedViewpoints()
                                entity.SetEntityFlagValue(key, change.Flags[key]);

                                if (key == "hasviewpoint")
                                {
                                    if (AppMain._core.Scene_Mode == Keystone.Core.SceneMode.EditScene)
                                    {
                                        if (previousFlag != change.Flags[key])
                                            AppMain._core.SceneManager.Scenes[0].ManageGeneratedViewpoints(entity, true, false, false);
                                    }
                                    //else change.Flags.Remove(key); // <-- can't remove during for loop
                                }

                                if (key == "playercontrolled")
                                {
                                    if (AppMain.mPlayerControlledEntityID != entity.ID)
                                    {
                                        Entity previous = (Keystone.Entities.Entity)Repository.Get(AppMain.mPlayerControlledEntityID);
                                        if (previous != null)
                                            previous.SetEntityFlagValue(key, false);
                                        AppMain.mPlayerControlledEntityID = entity.ID;
                                        AppMain.mPlayerControlledEntity = entity;
                                    }
                                }
                            }
                        }

                        SaveNode(node);
                        NotifyPluginOfPropertyChange(node.ID, node.TypeName);
                       
                    }
                    else if (message is KeyCommon.Messages.Node_ChangeProperty)
                    {
                        // TODO: here this needs to be a Node_ChangeProperty_Confirmation
                        // and allow us to then reference the original Node_ChangePropertyRequest
                        // to actually apply the changes here.
                        // A confirmation requires a way to identify the original... 
                        // how does the UDP part of lidgren deal with those acks?
                        // obviously it's part of the header... but..
                        //


                        KeyCommon.Messages.Node_ChangeProperty changeProp = (KeyCommon.Messages.Node_ChangeProperty)message;
                        Node node = (Node)workerProduct;

                        string typeName = node.TypeName;

#if DEBUG
                        if (typeName == "Material")
                        {
                            Material mat = (Material)Repository.Get(changeProp.NodeID);
                            //mat.Apply();
                        }
#endif
                        // TODO: if this is an Entity, we want to be able to potentially validate and respond to events
                        // via scripted event handlers.  We may also potentially want to validate.  That requires we
                        // identify if node is entity and then call the overload.  Ideally we make all
                        // versions of SetProperties allow for the event handlers and rules validation.
                        // but, then if the base calls Validate() or tries to invoke event handler
                        // how do we prevent that in the derived? especially since we want to validate
                        // prior to setting, and then raise event after value has been updated
                        int[] broken;

                        //node.SetProperties(changeProp.Properties);
                        for (int i = 0; i < changeProp.Properties.Length; i++)
                        {
                            object newValue = null;
                            object currentValue = node.GetProperty(changeProp.Properties[i].Name, false).DefaultValue;
                            switch (changeProp.Operations[i])
                            {
                                case KeyCommon.Simulation.PropertyOperation.Add:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.AddArrayElement(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, newValue);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Remove:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.RemoveArrayElement(changeProp.Properties[i].TypeName, currentValue); //, changeProp.Properties[i].DefaultValue);
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, newValue);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Union:// union performs a merge that updates any existing but appends no duplicates
                                    newValue = KeyCommon.Helpers.ExtensionMethods.MergeArrayElements(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, newValue);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Increment:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.IncrementNumeric(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, newValue);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Decrement:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.DecrementNumeric(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, newValue);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Replace:
                                default:
                                    node.SetProperty(changeProp.Properties[i].Name, changeProp.Properties[i].TypeName, changeProp.Properties[i].DefaultValue);
                                    break;
                            }
                        }

                        // TODO: verify all our scripts all change properties via a command
                        // and not directly.  If scripts can directly change nodes it screws things up.
                        // I'm pretty sure as it is, the scripts must call editorhost.API which 
                        // goes through our command system here.
                        //
                        // So consider animations even, how does our actor currentframe get sent?
                        //What if we had the plugin control it when play/pause/advance/reverse at
                        // the animation tab?  Our plugin would need some kind of timer though..
                        // Maybe for these things a way to go is to do a type of variable
                        // "Watch" and use a timer or a sort of Watch_Frequency

                        if (!changeProp.HasFlag(KeyCommon.Messages.Flags.SourceIsClient))
                            NotifyPluginOfPropertyChange(changeProp.NodeID, typeName);

                        // note: to change a property only, we dont need to start by
                        // saving the parent since the parent should already be saved
                        // with this node as a child.  Therefore we only need to save this
                        // specific node that has had it's property changed
                        if (changeProp.HasFlag(KeyCommon.Messages.Flags.SourceIsClient) || !changeProp.HasFlag(KeyCommon.Messages.Flags.SourceIsClientScript))
                            SaveNode(changeProp.NodeID);
                    }
                    else if (message as KeyCommon.Messages.Entity_ChangeCustomPropertyValue != null)
                    {
                        KeyCommon.Messages.Entity_ChangeCustomPropertyValue changeCustom = (KeyCommon.Messages.Entity_ChangeCustomPropertyValue)message;
                        if (changeCustom.CustomProperties != null && changeCustom.CustomProperties.Length > 0)
                        {
                            Node node = (Node)workerProduct;

                            string typeName = node.TypeName;

                            // TODO: move this code to the loopback processor and then pass a version of this message with all PropertyOperations.Replace
                            if (node is Entity)
                            {
                                Entity entity = (Entity)node;
                                int[] errorCodes = null;

                                for (int i = 0; i < changeCustom.CustomProperties.Length; i++)
                                {
                                    object newValue = null;
                                    object currentValue = entity.GetCustomPropertyValue(changeCustom.CustomProperties[i].Name);
                                    switch (changeCustom.Operations[i])
                                    {
                                        case KeyCommon.Simulation.PropertyOperation.Add:
                                            newValue = KeyCommon.Helpers.ExtensionMethods.AddArrayElement(changeCustom.CustomProperties[i].TypeName, currentValue, changeCustom.CustomProperties[i].DefaultValue);
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, newValue, true, true, out errorCodes);
                                            break;
                                        case KeyCommon.Simulation.PropertyOperation.Remove:
                                            newValue = KeyCommon.Helpers.ExtensionMethods.RemoveArrayElement(changeCustom.CustomProperties[i].TypeName, currentValue); // changeCustom.CustomProperties[i].DefaultValue);
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, newValue, true, true, out errorCodes);
                                            break;
                                        case KeyCommon.Simulation.PropertyOperation.Union: // union performs a merge that updates any existing but appends no duplicates
                                            newValue = KeyCommon.Helpers.ExtensionMethods.MergeArrayElements(changeCustom.CustomProperties[i].TypeName, currentValue, changeCustom.CustomProperties[i].DefaultValue);
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, newValue, true, true, out errorCodes);
                                            break;
                                        case KeyCommon.Simulation.PropertyOperation.Increment:
                                            newValue = KeyCommon.Helpers.ExtensionMethods.IncrementNumeric(changeCustom.CustomProperties[i].TypeName, currentValue, changeCustom.CustomProperties[i].DefaultValue);
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, newValue, true, true, out errorCodes);
                                            break;
                                        case KeyCommon.Simulation.PropertyOperation.Decrement:
                                            newValue = KeyCommon.Helpers.ExtensionMethods.DecrementNumeric(changeCustom.CustomProperties[i].TypeName, currentValue, changeCustom.CustomProperties[i].DefaultValue);
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, newValue, true, true, out errorCodes);
                                            break;
                                        case KeyCommon.Simulation.PropertyOperation.Replace:
                                        default:
                                            entity.SetCustomPropertyValue(changeCustom.CustomProperties[i].Name, changeCustom.CustomProperties[i].DefaultValue, true, true, out errorCodes);
                                            break;
                                    }
                                }


                                // if success, send a success response
                                if (errorCodes != null && errorCodes.Length > 0)
                                {
                                    System.Diagnostics.Debug.Assert(errorCodes != null && errorCodes.Length > 0);
                                    for (int i = 0; i < errorCodes.Length; i++)
                                    {
                                        string errorDescr = entity.Script.GetError(errorCodes[i]);
                                        Debug.WriteLine("Validation failed for property '" + changeCustom.CustomProperties[i].Name + "' - " + errorDescr);
                                    }
                                }
                            }

                            if (changeCustom.HasFlag(KeyCommon.Messages.Flags.SourceIsClient) || !changeCustom.HasFlag(KeyCommon.Messages.Flags.SourceIsClientScript))
                            {
                                SaveNode(changeCustom.EntityID);
                                NotifyCustomPropertyChange(changeCustom.EntityID, typeName);
                            }
                        }
                    }
                    else if (message is KeyCommon.Messages.Geometry_CreateGroup)
                    {
                        KeyCommon.Messages.Geometry_CreateGroup createGroup = (KeyCommon.Messages.Geometry_CreateGroup)message;
                        object[] objects = (object[])workerProduct;

                        // NOTE: The Entity, Model and ParticleSystem already exists in the Scene.  Here we are applying 
                        //       the Emitter or Attractor and any Appearance and GroupAttribute
                        Model model = (Model)objects[0];
                        ParticleSystem ps = (ParticleSystem)objects[1];
                        // here we should add the emitter to the ParticleSystem and the GroupAttribute to the Model

                        if (createGroup.GroupClass == 1) // attractor
                        {
                            ps.AddAttractor(createGroup.GroupName);
                        }
                        else // add a GroupAttribute and an Emitter
                        {
                            System.Diagnostics.Debug.Assert(createGroup.MaxParticles > 0);
                            MTV3D65.TVMesh tvmesh;
                            MTV3D65.TVMiniMesh tvmini;
                            if (createGroup.GroupType == 2) // Minimesh
                            {
                                tvmesh = (MTV3D65.TVMesh)objects[4];
                                tvmini = (MTV3D65.TVMiniMesh)objects[5];
                                ps.AddEmitter(createGroup.Name, (MTV3D65.CONST_TV_EMITTERTYPE)createGroup.GroupType, createGroup.MaxParticles, tvmini, tvmesh, createGroup.MeshPath);
                            }
                            else
                                ps.AddEmitter(createGroup.Name, (MTV3D65.CONST_TV_EMITTERTYPE)createGroup.GroupType, createGroup.MaxParticles);

                            // todo: these are stored in the objects[] and do not need to be created here, just applied to the Model
                            DefaultAppearance appearance = model.Appearance as DefaultAppearance;
                            if (appearance == null)
                            {
                                appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");
                                model.AddChild(appearance);
                            }

                            GroupAttribute ga = (GroupAttribute)Repository.Create("GroupAttribute");
                            appearance.AddChild(ga);




                            // todo: it might be nice to have an overloaded AddChild() where we can insert at a specific index so that it matches the emitter's groupIndex
                            // todo: or we re-order the GroupAttributes as the user re-orders emitters. This should work since adding GroupAttributes is only done when adding an Emitter
                            // todo: also when adding a Geometry ParticleSystem under a Model, it should be noted to the user that any existing GroupAttributes will be removed and instead
                            //       be managed by adding/removing of emitters
                            // todo: in fact, we shouldn't even be tracking GroupIndex of the emitter, only the array position.  otherwise, if we delete an emitter we need to re-name all the existing emitters potentially.
                            // so all we need to do is correlate emitter to GroupAttribute position.  So im better off letting users provide a "Name" for the emitter and not tracking its index at all.  we can derive
                            // its position in code by iterating through the parent treenode's children


                        }
                    }
                    else if (message is KeyCommon.Messages.Geometry_RemoveGroup)
                    {
                        KeyCommon.Messages.Geometry_RemoveGroup removeGroup = (KeyCommon.Messages.Geometry_RemoveGroup)message;
                        // for  emitters we remove the emitter and the groupAttribute from the Model
                        Node[] nodes = (Node[])workerProduct;
                        Model model = (Model)nodes[0];
                        ParticleSystem node = (ParticleSystem)nodes[1];
                        if (removeGroup.GroupClass == 0)
                            node.RemoveEmitter(removeGroup.GroupIndex);
                        else
                            node.RemoveAttractor(removeGroup.GroupIndex);
                    }
                    else if (message is KeyCommon.Messages.Geometrty_ChangeGroupProperty)
                    {
                        // TODO: here this needs to be a Node_ChangeProperty_Confirmation
                        // and allow us to then reference the original Node_ChangePropertyRequest
                        // to actually apply the changes here.
                        // A confirmation requires a way to identify the original... 
                        // how does the UDP part of lidgren deal with those acks?
                        // obviously it's part of the header... but..
                        //


                        KeyCommon.Messages.Geometrty_ChangeGroupProperty changeProp = (KeyCommon.Messages.Geometrty_ChangeGroupProperty)message;
                        ParticleSystem geometry = (ParticleSystem)workerProduct;

                        string typeName = geometry.TypeName;

                        // TODO: if this is an Entity, we want to be able to potentially validate and respond to events
                        // via scripted event handlers.  We may also potentially want to validate.  That requires we
                        // identify if node is entity and then call the overload.  Ideally we make all
                        // versions of SetProperties allow for the event handlers and rules validation.
                        // but, then if the base calls Validate() or tries to invoke event handler
                        // how do we prevent that in the derived? especially since we want to validate
                        // prior to setting, and then raise event after value has been updated
                        int[] broken;

                        //node.SetProperties(changeProp.Properties);
                        for (int i = 0; i < changeProp.Properties.Length; i++)
                        {
                            object newValue = null;
                            object currentValue = geometry.GetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, changeProp.GroupClass);
                            switch (changeProp.Operations[i])
                            {
                                // NOTE: Primarily Add and Remove are for adding KeyFrames
                                case KeyCommon.Simulation.PropertyOperation.Add:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.AddArrayElement(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, newValue, changeProp.GroupClass);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Remove:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.RemoveArrayElement(changeProp.Properties[i].TypeName, currentValue); //, changeProp.Properties[i].DefaultValue);
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, newValue, changeProp.GroupClass);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Union:// union performs a merge that updates any existing but appends no duplicates
                                    newValue = KeyCommon.Helpers.ExtensionMethods.MergeArrayElements(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, newValue, changeProp.GroupClass);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Increment:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.IncrementNumeric(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, newValue, changeProp.GroupClass);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Decrement:
                                    newValue = KeyCommon.Helpers.ExtensionMethods.DecrementNumeric(changeProp.Properties[i].TypeName, currentValue, changeProp.Properties[i].DefaultValue);
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, newValue, changeProp.GroupClass);
                                    break;
                                case KeyCommon.Simulation.PropertyOperation.Replace:
                                default:
                                    geometry.SetProperty(changeProp.GroupIndex, changeProp.Properties[i].Name, changeProp.Properties[i].DefaultValue, changeProp.GroupClass);
                                    break;
                            }

                        }

                        // TODO: verify all our scripts all change properties via a command
                        // and not directly.  If scripts can directly change nodes it screws things up.
                        // I'm pretty sure as it is, the scripts must call editorhost.API which 
                        // goes through our command system here.
                        //
                        // So consider animations even, how does our actor currentframe get sent?
                        //What if we had the plugin control it when play/pause/advance/reverse at
                        // the animation tab?  Our plugin would need some kind of timer though..
                        // Maybe for these things a way to go is to do a type of variable
                        // "Watch" and use a timer or a sort of Watch_Frequency

                        if (!changeProp.HasFlag(KeyCommon.Messages.Flags.SourceIsClient))
                            NotifyPluginOfPropertyChange(changeProp.GeometryNodeID, typeName);

                        // note: to change a property only, we dont need to start by
                        // saving the parent since the parent should already be saved
                        // with this node as a child.  Therefore we only need to save this
                        // specific node that has had it's property changed
                        //                        if (!changeProp.HasFlag(KeyCommon.Messages.Flags.SourceIsClientScript))
                        //                           SaveNode(changeProp.NodeID);
                    }
                    else if (message as KeyCommon.Messages.Geometry_Add != null)
                    {
                        KeyCommon.Messages.Geometry_Add importGeometry = (KeyCommon.Messages.Geometry_Add)message;

                        // the workerProduct should contain our existing Model and a new DefaultAppearance (with or without any children) and a new Geometry node. Add these to the Model.
                        // The model is already connected to the scene so we don't AddChld() the appearance and geometry until here, in the main thread
                        ModeledEntity entity = (ModeledEntity)((Node[])workerProduct)[0];
                        Model model = (Model)((Node[])workerProduct)[1];
                        Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)((Node[])workerProduct)[2];
                        Geometry geometry = (Geometry)((Node[])workerProduct)[3];
                        if (model.Appearance != null)
                            model.RemoveChild(model.Appearance);

                        if (appearance != null)
                            model.AddChild(appearance);

                        model.AddChild(geometry);

                        // Add any boned animation to Entity hosting Actor3d
                        if (geometry is Actor3d)
                            if (((Node[])workerProduct)[4] != null)
                                entity.AddChild((Keystone.Animation.Animation)((Node[])workerProduct)[4]);

                    }
                    else if (message as KeyCommon.Messages.Archive_AddGeometry != null)
                    {
                        KeyCommon.Messages.Archive_AddGeometry addGeometry = (KeyCommon.Messages.Archive_AddGeometry)message;
                        string modPath = System.IO.Path.Combine(AppMain.MOD_PATH, addGeometry.ModName);
                        string entry = System.IO.Path.Combine(addGeometry.EntryDestinationPath, addGeometry.EntryNewFilename);


                        string workspaceName = "WORKSPACE" + entry;

                        Node node = (Node)command.WorkerProduct;
                        if (node is Container)
                        {
                            SaveResource(((Container)node).Interior);
                        }
                        Repository.IncrementRef(node);
                        Repository.DecrementRef(node);

                        ShowPreview(modPath, entry);

                    }
                    else if (message as KeyCommon.Messages.Prefab_Save != null) // ProcessCompletedCommandQueue()
                    {
                        KeyCommon.Messages.Prefab_Save prefabSave = (KeyCommon.Messages.Prefab_Save)message;

                        // if current modPath is an archive
                        string modPath = AppMain.MOD_PATH; //, prefabSave.ModName;
                        string path = prefabSave.EntryPath;

                        if (modPath.EndsWith(".zip"))
                        {
                            path = System.IO.Path.Combine(prefabSave.EntryPath, prefabSave.EntryName);
                        }
                        else
                        // else it's save directly to desired mod folder on HDD
                        {
                            //path = System.IO.Path.Combine(modPath, prefabSave.EntryPath);
                        }

                        // NOTE: Worker_SavePrefab does the actual saving of the .kgbentity/.kgbsegment

                        //Entity entity = Repository.Get ();

                        ShowPreview(modPath, path);

                    }
                    else if (message is KeyCommon.Messages.Entity_Move)
                    {

                        if (AppMain.mLoopbackServer == null)
                        {
                            KeyCommon.Messages.Entity_Move move = (KeyCommon.Messages.Entity_Move)message;
                            Interior parent = (Interior)Repository.Get(move.mParentID);

                            Entity[] children = (Entity[])workerProduct;
                            // If this message reaches the client we know that it has been validated so all we have to do is remove and re-add it with updated Translation and only if not using loopback since loopback will have already done this

                            for (int i = 0; i < children.Length; i++)
                            {
                                // make sure the children do not fall out of scope on call to RemoveChild
                                // todo: make sure the InteriorTransformTool does NOT decrement or increment... thats for the loopback/server and here to do.
                                Repository.IncrementRef(children[i]);
                                parent.RemoveChild(children[i]);

                                children[i].Translation = move.Positions[i];
                                // we re-add the child to the ISpatialNode parent.SceneNode
                                // NOTE: we don't need the setter because parent is already cast as Interior since this command is only for Interiors
                                //Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parent);
                                //setter.Apply(children[i]);
                                parent.AddChild(children[i]);
                                Repository.DecrementRef(children[i]);

                            }

                            SaveNode(move.mParentID);
                        }


                    }
                    else if (message as KeyCommon.Messages.GameObject_ChangeProperties != null)
                    {
                        KeyCommon.Messages.GameObject_ChangeProperties goChangeProperties = (KeyCommon.Messages.GameObject_ChangeProperties)message;
                        Entity entity = (Entity)Repository.Get(goChangeProperties.Owner);

                        // note: this is client side processing so any Rules checks should have been done in the loopback server and then
                        //       the command forwarded to the client.exe here.
                        // lets focus on "contacts[]" which should be part of TacticalState.  This suggest that a CrewStation_Tactical.css needs to exist and assigned to an Entity in the vehicle.Interior. This can then host the specific properties we need and there's no need for a GameObject implementation
                        // they can assigned to private non serializable properties, but for the public vars, this has the advantage that they can be inspected in the GUI
                        // this means we don't need GameObject implementations for our main station states... only for Tasks/Orders perhaps? This is basically what Unity does.  The main issue is having to Query() down the hierarchy to find things like "CrewStation_Tactical"KeyCommon.Simulation.PropertyOperation[] operations = new  KeyCommon.Simulation.PropertyOperation[1];

                        switch (goChangeProperties.GameObjectTypeName) // todo: this should just use GameType 
                        {
                            // todo: i think NavPoint is not a gameobject.. it should be part of HelmState and the scripts should be able to directly 
                            // modify its members and then send a simple ChangeProperties to it that contains nodeIDs, operationID and the propertyspecs
                            // from there we should simply be able to do HelmState.SetProperties(specs) 
                            // todo: really, we dont even need Set/GetProperties because they are Game01.GameObjects and known by exe and scripts. The only real purpose would be to assign them to propertyGrid for inspection
                            // i shouldn't need a switch statement below
                            // todo: it needs to be sent as a gameobject over the wire
                            case "NavPoint":
                                Game01.GameObjects.NavPoint[] navPoints = (Game01.GameObjects.NavPoint[])workerProduct; // workerProduct should be HelmState which can contain an array of NavPoint
                                int[] brokenCode;
                                // todo: this shouldn't use the entity.SetCustomPropertyValue but rather the gameObject itself right?
                                entity.SetCustomPropertyValue("navpoints", navPoints, true, true, out brokenCode);

                                if (brokenCode == null)
                                {
                                    // update this waypoint in the database
                                    int index = GetNavPointArrayIndexFromRowID(goChangeProperties.GameObjectIndex, navPoints);
                                    System.Diagnostics.Debug.Assert(index >= 0);
                                    Database.AppDatabaseHelper.Waypoint_UpdateRecord(goChangeProperties.Owner, goChangeProperties.GameObjectIndex, navPoints[index]);
                                }
                                break;
                            default:
                                break;
                        }

                    }

                }
                else
                    Debug.WriteLine("Command failed.", result.Exception.ToString());
            }  // end foreach

            // AFter we've iterated through all outstanding completed commands now perhaps is best time 
            // to save the actual XMLDocuments to disk/zip whereas before we simply edited the Documents in memory.  

        }



        /// <summary>
        /// SceneLoad is called by worker thread.
        /// </summary>
        /// <param name="folderName"></param>
        private Scene SceneLoad(string folderName)
        { 
            Keystone.Simulation.ISimulation sim = CreateSimulation(folderName);
            AppMain.CURRENT_SCENE_NAME = folderName;
            
            Scene resultScene = _core.SceneManager.Open(folderName, sim, OnEmptyRegionPageComplete, OnRegionChildrenPageComplete);
            mScene = resultScene;
            IWorkspace workSpace = LoadWorkspace(mScene, PRIMARY_WORKSPACE_NAME, mStartingRegionID);
            mScene.mEntityAddedHandler += workSpace.OnEntityAdded;
            mScene.mEntityRemovedHandler += workSpace.OnEntityRemoved;

            mScene.Load();

            // WorkspaceManager.Add() will call Workspace.Configure which will configure a default Viewpoint.
            // TODO: should we not Configure on .Add?  Where else can we Configure the workspace?  Maybe
            // the workspace can configure itself upon first EntityAdded?  What about workspaces that get loaded
            // later and don't receive all the EntityAdded events?  Would be better if Configure occurred
            // when a Scene was passed into the Constructor.  Then depending on the Workspace, we can
            // iterate Scene.Root and add entity's that exist as well as ones that are paged in.  Or if we can
            // wait until the pager thread exits. 
            mWorkspaceManager.Add(workSpace, mScene);


            if (InvokeRequired)
                Invoke((System.Windows.Forms.MethodInvoker)delegate { workSpace.Configure(mWorkspaceManager, mScene); });
            else
                workSpace.Configure(mWorkspaceManager, mScene);

            this.Text = TITLE_KGB + " - " + folderName;
            //this.Text = TITLE_SFC + " - " + folderName;
            return resultScene;
        }

        //private void SceneLoadRequest(string folderName, string startingRegionID)
        //{
        //    KeyCommon.Messages.Scene_Load load = new KeyCommon.Messages.Scene_Load();
        //    load.FolderName = folderName;
        //    load.StartingRegionID = startingRegionID;
        //    load.UnloadPreviousScene = true;
        //    SendNetMessage(load);
        //}

        
        private void SimulationJoinRequest(string username, string password, string folderName, string missionName, bool resume)
        {
            KeyCommon.Messages.Simulation_Join_Request request = new KeyCommon.Messages.Simulation_Join_Request();

            request.FolderName = folderName;
            request.UserName = username;
            request.Password = password;

            SendNetMessage(request);
        }

        private IWorkspace LoadWorkspace(Scene scene, string workspaceName, string regionID)
        {
            // TODO:
            // TODO: user's ship selection included in Scene_NewUniverse?
            //		 - where is user's ship referenced so that it's available to all workspaces?
            //				-Workspace.Selected.Entity 
            //		- Can we spawn the player's vehicle and then "FlyTo" instantly.  If that requires traversing
            //		   through multiple Zones instantly, will it break?  In other words, do we need to traverse
            //			with flyto one zone at a time.  FlyTo might not be a good option.  We might just want to
            //			manually place the viewpoint in the same sector as our Vehicle and then "chase" it.		
            //		- How is the player vehicle inserted currently?  I think when we instantiate Sol System it gets
            //		  added.
            //			- why is the vehicle invisible? BECAUSE GAME IS PAUSED! UNPAUSE (GREEN ARROW IN TOOLBAR)
            //			- the shader needs to be fixed. oversaturated (specular?)
            //
            // 		- can i generate a system with Sol system at Zone0,0,0?  And with vehicle orbiting earth?
            //		- can i generate other systems using real star data and then just generating planets for them?
            //			- we need option to load local star data from file and adjust the star positions to fit in middle of Zones.
            //		- first priority is getting vehicle in and viewpoint "chase"ing it
            //			- we need a new EditorWorkspace.ViewpointBehaviors.CreateVP******() for our tactical workspace's viewpoint
            //		- we need a way to place predesigned systems such as Sol, into the generation.  If stellarsystem already exists
            //		  for that region, we skip generation.  
            //			- this also must work with real star data we parse and use to create new sectors.
            //			- but first, let's just work on placing the vehicle in orbit around any planet we've generated
            //				- once we have that, we can focus on binding the viewpoint to chase it
            //				- recall it's the SceneInfo that stores Viewpoints such as a starting viewpoint
            //				  and we shouldn't be creating those viewpoints then in SceneManager.CreateNewSceneDatabase()
            //      

            //		- Scene.PlayerVehicle?
            //			- in multiplayer, how would the various player vehicles be referenced?
            //				- it would need to be tied to a Player object stored where?  Simulation.cs? Scene? 
            //		- do we deserialize the Vehicle here?  If so we need to know when player vehicles are created.
            //			- this could be done in Scene.EntityAttached
            //
            //			- that's why adding the vehicles (spawning them) might be better after loading the scene.
            //			- what about NPC vehicles?
            //			- is there a distinction between Player object and it's scene node ModeledEntity?
            //				- also a distinction between Player.Vehicle and Player.Stats (Player.User.Stats or Player.UserStats)
            //				- doesn't the DomainObject give us the distinction we're looking for?  Or do we still need a 
            //				  "player" object that references the ModeledEntity/Vehicle.
            //			- do we need a Player spawn point?
            //
            //		- EditorWorkspace.PlayerVehicle or .Player
            //			- if PlayerVehicle is in WorkspaceBase and must be assigned to relevant workspace types (not to RenderPreview for instance, but for 
            //			Nav and Editor yes.
            //		- bound like a Viewpoint?  It would then need to be bound to every RenderingContext
            //			- and don't we want to be able to bind a viewpoint to the vehicle? 
            //				- and that needs to be done on scene load right.  
            //				loading a saved scene and generating a new universe with selected vehicle should use same code
            // 		- Do we use EntityNodes and Entities for ship internal components?
            //		- we need to prevent multiple player vehicles Entities in the scene graph. eg. if hacker tries to edit to the xml to have multiple user vehicles.
            //		- we must bind the player vehicle when it's found.  But how do we find the player vehicle from a multi-zone scene Database?
            //			- for viewpoints, we use the SceneInfo.Viewpoint to find a starting viewpoint from which to page Zones in.
            //			- because that Zone needs to be the one that has a Viewpoint for the player so it gets paged in.
            //			- look at the "RenderingContext.Viewpoint_MoveTo" Vehicle code and see how it binds viewpoint to vehicle. Actually
            //			this does not seem to bind... i think the Workspaces.EditorWorkspace.ViewpointBehaviors.cs does			


            // TODO: this command when received by client is confirmation of OK to generate the scene
            //       and then Simulation_Join with valid username and password.  
            //       If a first time join, we also include the selected Vehicle prefab (server validates legality of selection)
            //
            //      
            // TODO: RESUMING PLAY STEPS SINGLE PLAYER is different from MULTIPLAYER because 
            //       we connect to loopback immediately and do not determine which host to connect to based on a most recent host.
            // - connect to network
            // - select from Most Recent list an existing Scene file?  
            // - we're already connected to loopback! 
            //      - for multiplayer you'd expect to NOW connect and then issue a Simulation_Join command.
            //        to join a currently running simulation on the server. (OR TO START A SIMULATION THAT WAS PAUSED FOR ALL PLAYERS)
            //       - the server would send a universe generation SEED and the client would generate the universe locally.
            //          - the server would then send according to sensor readings, notifications of contacts.
            //          - the server would then send according to sensor readings and ship's database planetary info such as cities, factories, mines, etc.
            //   - but for now, what should we do?
            //      - player requests join and passes in homeworld selection and race selection and ship selection?
            //   - right now we do have the Scene loaded on client so all we need to send is a join with ship selection.
            //     the server can then issue a spawn command for that ship (or if the user fails authentication, bounces the client but on loopback that is skipped)
            //     - so let's go with this.  Client on Generate_NewUniverse sends Simulation_Join command with ship selection.
            //       server will find a random planet and place the ship in orbit above it and send those results to the client via a Spawn command.
            //


            // TODO: What's a better place to instantiate and Add and Configure workspaces?


            // TODO: but what we need to do is use a Viewpoint that is attached to player's vehicle.
            //       this means we need to delay configuration of the Workspace until the player's vehicle
            //       is loaded.  So how do we do that?  We can perhaps delay creation of the workspace until
            //       a Vehicle Node is created.  Should we perhaps use a seperate command for Vehicle creation
            //       rather than the typical CreateNode?

            IWorkspace workSpace;

            switch (workspaceName)
            {
                case PRIMARY_WORKSPACE_NAME:
                    workSpace = new Workspaces.EditorWorkspace(workspaceName);
                    break;
                case "Tactical":
                    workSpace = new Workspaces.TacticalWorkspace(workspaceName);
                    ((Workspaces.TacticalWorkspace)workSpace).mStartingRegionID = regionID; // TODO: this actually just needs to be vehicle.Region.ID as the Tactical workspace may not be loaded until user chooses to
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                    break;
            }

            return workSpace;
        }


        // Here we can react based on Zone or Region of a particular id loading successfully.
        private void OnEmptyRegionPageComplete(Keystone.IO.ReadContext context)
        {
            
        }

        private Scene mScene; // todo: delete this
        private string mStartingRegionID; // todo: delete this
        // here we can react based on all children scene elements within a particular Zone or Region being loaded successfully
        private void OnRegionChildrenPageComplete(Keystone.IO.ReadContext context)
        {
            
            if (context.Node == null) return;

            if (context.Node is Region)
            {  
                //       Are vehicles not saved to the db?  What about the components within the vehicle and the crew?
                //       What about NPC controlled Vehicles?  What if i put each Vehicle's xml in their own directory named by the Vehicle's guid id and uncompressed xml?
                //       It may also be worth considering putting every Zone in its own directory too.
                // TODO: Do we need an event for when vehicles are paged in?
                // TODO: lets incrementally do this... start with just using the proper "prefab" .kgbentity for the vehicle path below and proper UserName
                // - and for now, dont mind that the xml is all in one xml decompressed db
                // - Some basic assumptions
                //   - the server and client both have same "map."  This could be client having sent to server the seed and ratios required and Zone layout etc.
                //   - server has blueprint (prefab) for all player ships.  The initial ship is selected during "new campaign" and then user edits are updated as they occur.  Server can then validate user ships on resume campaign.
                // - when resumming, a spawn point built into the map does not help us.  How do we handle spawning player vehicles and avatars in this case?
                //   - for multi-zone scenes, we need to know immediately which zone the player's vehicle is in.  This guides are thinking on how to implement "resume."
                // - what if i loaded Vehicles first? I'd have to wait to parent them to their respective regions...
                // - what if i added a "player" node to SceneInfo?  But that would mean our SceneInfo wouldn't match the servers which would have to include all players.
                // - saw a Unity video where the author treated the player object as it's own scene and it loads the player first, then in script it adds it to the SceneNamanager.
                //   - this parallels the purpose of our SceneInfo object.  It would have to be different on each client and on the server.
                // - so question is, how do i handle different client and scene SceneInfo.xml objects?  Each client in multiplayer would have a different vehicle id to which a viewpoint would be attached right?
                //     - in fact, the server will often be serving each player custom xml info at runtime and not just during "Join."  This way each client does not need to know what is in every Zone.  They can get that info when entering adjacents.
                //   - maybe on Join, the server sends back a clientSceneInfo.xml?

                KeyCommon.Messages.RegionPageComplete pageCompleted = new KeyCommon.Messages.RegionPageComplete();
                pageCompleted.RegionID = context.Node.ID;

                // inform the server that a Region / Zone has loaded and that we are ready for  relevant Spawn messages
                SendNetMessage(pageCompleted);
                // send server "RegionLoadComplete message and server will send a Spawn command for the player's Vehicle.  Then when the vehicle (and it's Interior) is loaded, we will do same to initiate spawning of the Crew.
                // The user's vehicle is special in that we assign Viewpoint to it
                
            }
        }

        

        private static Keystone.Entities.Entity DeserializePreviewComponent(string archiveFullPath, string pathInArchive)
        {
            if (string.IsNullOrEmpty(archiveFullPath) ||
                string.IsNullOrEmpty(pathInArchive)) return null;

            string extension = System.IO.Path.GetExtension(pathInArchive).ToUpper();
            switch (extension)
            {
            	case ".KGBSEGMENT":
                case ".KGBENTITY":// represents a compressed archive (xmldb) within the overall xmldb

                    // open an entity from a zip archive mod db or mod folder path on harddrive
                    System.IO.Stream stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(pathInArchive, "", archiveFullPath);
                    if (stream == null) throw new Exception("FormMain.Commands.DeserializePreviewComponent() - ERROR: zip entry not found.");
                    
                    Keystone.IO.XMLDatabaseCompressed database = new Keystone.IO.XMLDatabaseCompressed();
                    Keystone.Scene.SceneInfo info = database.Open(stream);

                    // NOTE: we delayTVResourceLoading because we only LoadTVResource for the exterior geometry.  This also prevents us from loading Container Vehicle Interior
                    bool delayTVResourceLoading = true;
                    Keystone.Elements.Node node = database.ReadSynchronous(info.FirstNodeTypeName, null, true, true, null, delayTVResourceLoading, false);
                    if (!(node is Keystone.Entities.Entity))
                        throw new Exception("FormMain.Commands.DeserializePreviewComponent() - Node not of valid Entity type.");
                    
                    Debug.Assert(node is Entity, "DeserializePreviewComponent() - Node is not an Entity.");
                    if (node is Entity)
                    {
                        // TODO: This isn't compiling or creating the Entity script
                        if (node is Keystone.Vehicles.Vehicle || node is Container)
                        {
                            // There is no need to have an interior for a Vehicle or Container's Interior.  Preview only shows Exterior
                            Container c = (Container)node;
                            c.RemoveChild(c.Interior);
                        }
                        if (node is ModeledEntity)
                        {
                            ModeledEntity entity = (ModeledEntity)node;
                            Model[] models = entity.SelectModel(SelectionMode.Render, 0);
                            if (models != null)
                            {
                                for (int i = 0; i < models.Length; i++)
                                {
                                    Keystone.Appearance.Appearance appearance = models[i].Appearance;
                                    Keystone.IO.PagerBase.LoadTVResource(appearance, true);
                                    Keystone.Elements.Geometry geometry = models[i].Geometry;
                                    Keystone.IO.PagerBase.LoadTVResource(geometry, false);
                                }
                            }
                        }
                    }
                    
                    stream.Close();
                    stream.Dispose();
                    database.Dispose();
                    return (Keystone.Entities.Entity)node;

                default:
                    return null;

            }
        }

        private void ShowPreview (string modPath, string entryName)
        {
            // TODO: note: here the closing event of the previewform is not hooked
            //       so i'm not sure if we can switch back to the previous workspace!
            // TODO: and actually, where am i even creating the formPreview workspace?
            //       is it because I havent tested importing geometry in a while?
            
			string previewSceneDBPath = System.IO.Path.GetTempPath() ; // TODO: i added this tempfile for fullpath after tweaking things.  Is this correct to use a tempfile here?

            string regionID;
			if (AppMain._core.SceneManager.CreateNewSceneDatabase(previewSceneDBPath, 10000f, 10000f, 10000f, out regionID) == false)
            	throw new Exception ("FormMain.Commands.ShowPreview() - Error creating scene database.");

			if (modPath.EndsWith(".zip"))
			{
                // todo: this is obsolete - we don't place prefabs into zipped mods anymore. 
            	entryName =  KeyCommon.IO.ArchiveIOHelper.TidyZipEntryName(entryName);
			}
            // EditorWorkspace or most others will attempt to configure the control as a document tab
            // on the main form. We must therefore use a custom 'RenderPreviewWorkspace' where we can
            // have it configure to the form we pass into it's constructor.
            string workspaceName = "PREVIEW:" + entryName;
            string sceneName = "PREVIEW:" + entryName;

            AppMain.Form.Invoke(new System.Windows.Forms.MethodInvoker(delegate()
            	{ 
            		AppMain.PreviewForm = new FormPreview(null, null);
            	}));

            
            Keystone.Simulation.ISimulation sim = KeyEdit.FormMain.CreateSimulation(sceneName);
            Scene scene = AppMain._core.SceneManager.Open(previewSceneDBPath, sim, null, null);
            scene.Load();
            
            // TODO: we should not be deserializing from archive, but from either a tmp src or
            //       ...how in the heck was this even working when using zip archive? because the kgbentity
            //       to be deserialized isn't even saved yet... yet it was working wasn't it?
			//       kgbentity - imported, added to archive, then you could save it... how did the
			//       modifications get added to the archived version? Were mods saved during all updates?
			//       - if we save the entity first here, then it's loadable for preview and a re-save that includes the preview image
			
            // reads preview component from archive and adds to scene
            Entity previewEntity = DeserializePreviewComponent(AppMain.MOD_PATH, entryName);
            // NOTE: scene.Root already contains a default directionallight       	
            //Keystone.Lights.DirectionalLight light = LightsHelper.LoadDirectionalLight(AppMain.REGION_DIAMETER * 0.5f);

            Keystone.Entities.Entity parent = (Keystone.Entities.Entity)scene.Root;
            Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parent);
            //setter.Apply(light);
            setter.Apply(previewEntity);

            // NOTE: for our AssetBrowser Preview 
            // FloorPlanDesignWorkspace.InitializeAssetBrowser() and
            // EditorWorksapce.AssetBrowser.cs.OnShowPreview_Click() it does NOT
            // use a seperate workspace...  those are broken and MUST BE FIXED.
            // they were never fixed after switching
            // to use of workspace that configures the viewportcontrol.
            // TODO: this is wrong workspace for showing prefab? or is it ok to share?
            Workspaces.RenderPreviewWorkspace workSpace = new Workspaces.RenderPreviewWorkspace(workspaceName,
                    									AppMain.PreviewForm, mWorkspaceManager.CurrentWorkspace.Name);
            mWorkspaceManager.Add(workSpace, scene);
            workSpace.Configure(mWorkspaceManager, scene);
            mWorkspaceManager.ChangeWorkspace(workSpace.Name, false);

            scene.Simulation.Running = true;
            // TODO: this call to context.MoveTo() might be resulting in a deadlock 
            // because inside it, it moves the camera via .SetMatrix() and unfortunately that is potentially
            // occurring at any random time during rendering which is obviously bad.
            // context.MoveTo() should result in the creation of a move command at the very least
            // 
            workSpace.ViewportControls[0].Context.Viewpoint_Orbit(previewEntity, .9f);

            // ProcessCompletedCommand() occurs on gameloop thread and showing this
            // FormPreview will block that thread if it's not spawned on the Windows Form thread.
            // For some reason I seem unable to do that...
            AppMain.Form.ShowPreview(previewEntity, modPath, entryName);
        }
        

        internal static Keystone.Simulation.ISimulation CreateSimulation(string name)
        {
            // TODO: this call should be done in a seperate thread and upon complettion then we should RegisterGame
            KeyCommon.DatabaseEntities.Game game = new KeyCommon.DatabaseEntities.Game(name);

            System.Diagnostics.Debug.WriteLine("FormMain.Commands.CreateSimulation() - Simulationed " + game.mName + " loaded.");
            Keystone.Simulation.ISimulation sim = new Simulation(0.0f, game);
            return sim;
        }

        private void NotifyMove(string oldParentID, string newParentID, string childID, string childTypename)
        {
            KeyPlugins.AvailablePlugin plugin = GetCurrentPlugin(PRIMARY_WORKSPACE_NAME);
            if (plugin != null && plugin.Instance != null)
            {
                if (childID == plugin.Instance.TargetID || ChildNodeIsDescendant(plugin.Instance.TargetID, childID))
                {
                    ((KeyEdit.PluginHost.EditorHost)AppMain.PluginService).NotifyNodeMoved(oldParentID, newParentID, childID, childTypename);
                }
            }
        }

        private void NotifyAdd(string parentID, string childID, string childTypename)
        {
            KeyPlugins.AvailablePlugin plugin = GetCurrentPlugin(PRIMARY_WORKSPACE_NAME);
            if (plugin != null && plugin.Instance != null)
            {
                if (childID == plugin.Instance.TargetID || ChildNodeIsDescendant(plugin.Instance.TargetID, childID))
                {
                    ((KeyEdit.PluginHost.EditorHost)AppMain.PluginService).NotifyNodeAdded(parentID, childID, childTypename);
                }
            }
        }

        private KeyPlugins.AvailablePlugin GetCurrentPlugin (string workspace)
        {
            return  ((KeyEdit.Workspaces.EditorWorkspace)mWorkspaceManager.GetWorkspace(workspace)).CurrentPlugin;
        }

        private void NotifyRemove(string parentID, string childID, string childTypename)
        {
            KeyPlugins.AvailablePlugin plugin = GetCurrentPlugin(PRIMARY_WORKSPACE_NAME);

            if (plugin != null && plugin.Instance != null)
            {
                if (childID == plugin.Instance.TargetID || ChildNodeIsDescendant(plugin.Instance.TargetID, childID))
                {
                    ((KeyEdit.PluginHost.EditorHost)AppMain.PluginService).NotifyNodeRemoved(parentID, childID, childTypename);
                }
            }
        }

        // TODO: this should expand to include the propertyspec holding the new value
        private void NotifyCustomPropertyChange(string nodeID, string nodeTypename)
        {
            KeyPlugins.AvailablePlugin plugin = GetCurrentPlugin(PRIMARY_WORKSPACE_NAME);

            // has to be a custom property defined in a shared DomainObject but stored in an Entity
            if (plugin != null && plugin.Instance != null)
            {
                if (nodeID == plugin.Instance.TargetID || ChildNodeIsDescendant(plugin.Instance.TargetID, nodeID))
                {
                    ((KeyEdit.PluginHost.EditorHost)AppMain.PluginService).NotifyNodeCustomPropertyChanged(nodeID, nodeTypename);
                }
            }
        }

        private void NotifyPluginOfPropertyChange(string nodeID, string nodeTypename)
        {
            KeyPlugins.AvailablePlugin plugin = GetCurrentPlugin(PRIMARY_WORKSPACE_NAME);
            if (plugin != null && plugin.Instance != null)
            {
                // does the modified node match the target entity of the plugin OR any 
                // of it's descendants?  If so, then we need to notify the plugin of 
                // the modification so that it can update it's GUI.
                if (nodeID == plugin.Instance.TargetID || ChildNodeIsDescendant(plugin.Instance.TargetID, nodeID))
                {
                    ((KeyEdit.PluginHost.EditorHost)AppMain.PluginService).NotifyNodePropertyChanged(nodeID, nodeTypename);
                }
            }
        }

        private bool ChildNodeIsDescendant(string elderNodeID, string nodeToCompareID)
        {
            Node nodeToCompare = (Node)Repository.Get(nodeToCompareID);
            if (nodeToCompare == null) throw new ArgumentOutOfRangeException();
            
            return nodeToCompare.IsDescendantOf (elderNodeID);
        }
                

        internal void SaveNode(string entityID)
        {
            Node target = (Node)Repository.Get(entityID);
            SaveNode(target);
        }

        bool mOpenSceneInProgress = false;
        object mSaveNodeLock = new object();

        internal static void SaveResource(Node node)
        {
            Node target = node;

            if (target is Container)
                target = ((Container)target).Interior;

            // TODO: when editing the prefab of the Interior, am i updating the .interior within the MOD_PATH? The path i need to use is the same path as the vehicle .kgbentity
            //       - i have access to interior Entity below, so maybe i can get the parent.ID for the Container. Hrm, but that doesnt get us the prefab filename.  
            // TODO: i shouldn't really have to save anything until the user manually clicks "save" or when exiting the simulation. Everything is in memory, there's no need to save (unless there's a crash)
            string relativeDataPath = (string)target.GetProperty("datapath", false).DefaultValue;
            string fullDataPath;
            if (AppMain._core.Scene_Mode == Keystone.Core.SceneMode.EditScene)
                fullDataPath = System.IO.Path.Combine(AppMain.SCENES_PATH, relativeDataPath);
            else if (AppMain._core.Scene_Mode == Keystone.Core.SceneMode.Simulation)
                fullDataPath = System.IO.Path.Combine(AppMain.SAVES_PATH, relativeDataPath);
            else
                fullDataPath = System.IO.Path.Combine(AppMain.MOD_PATH, relativeDataPath);

            // if in MOD editing mode, this should update the main prefab's .interior bin file.  Otherwise
            // it should update the instance .interior.  I need to sort this out because nowhere am I saving the entire Vehicle instance and reloading it. I'm always just starting off by loading the prefab
            //     string prefab = interior.Parent.SRC;
            //     string prefabDBPath = System.IO.Path.ChangeExtension(prefab, ".interior");
            //     fullDBPath = prefabDBPath;
            // TODO: rename FloorplanWorkspace toolbar button to "save prefab" if in mod mode.  There needs to be a way to know when we are in Mod mode versus normal runtime edit mode.
            // saves to the live action instance of the Vehicle, not the prefab
            // - what we need to find out is, if we save the entire scene, will the vehicle be loaded with modifications of Structure intact?  and will it auto assign player's vehicle toe FloorplanWorkspace?
            
            Keystone.IO.ClientPager.QueuePageableResourceSave(target, fullDataPath, null, false);

                // TODO: What does the "Save" button in the Floorplan Editor toolbar do? Ah, it actually saves the prefab whereas this is saving the runtime version of the ship.
                // TODO: verify if i save the scene (not just the prefab) that it loads the vehicle with all edits intact.
                // TOOD: how would this work in multiplayer as far as sending the Command to the NON loopback server?
                // TODO: wait a minute, is this a message or a command?
            
        }

        // todo: we really do need to differentiate between saving the parent when a child is added or removed
        //       versus saving the node that changed (eg a property was changed)

        // asychronous save.  Why did i make it asychronous and called from the CommandCompleted
        // and not sychronous in the worker thread function?
        // TODO: we are saving nodes that are still in process of being loaded as they were added
        // and that means we can wind up saving a node that is not even fully deserialized yet
        // and thus we overwrite the data we are still trying to deserialize with the un-serialized
        // default values for various fields!  
        internal void SaveNode(Node node)
        {
        	// do not save when node creation and adding is due to loading of a scene or when a Mission is loaded.
            // TODO: or maybe we should just check if NOT in EDIT mode
        	// TODO: what about when paging in new Zones, does SaveNode get triggered when adding those nodes to the scene?
        	if (mOpenSceneInProgress || AppMain._core.ArcadeEnabled) // AppMain._core.Scene_Mode != Keystone.Core.SceneMode.EditScene) //AppMain._core.SceneManager.Scenes[0].Simulation.CurrentMission != null)
	        	return;


        	// Feb.14.2016 -because we need to temporarily disable Serialization of child Zones
        	// we need to sychronize threading between Zones and ZoneRoot
            // TODO: Maybe we can avoid this by using sqlite db as primary store with
            //       stars and worlds and vehicles all loaded as prefabs.
        	if (node as Zone != null || node as ZoneRoot != null)
        		System.Threading.Monitor.Enter (mSaveNodeLock);
        	
	        	
	            // TODO: make sure SaveNode is called when in Edit mode and when a change occurs
	            // in all cases.
	            bool saveOnWrite = true;
	            bool recurseChildren = true;
	            
	            
	            // HACK: saving the ZoneRoot would normally involve saving all the child zones as
				//       XML child nodes under ZoneRoot which WE MUST NEVER DO since we handle paging in of 
				//       Zone children in a specially.   For now, we temporarily set seerializable = false
				//       for zone children of zone root
				if (node is ZoneRoot)
				{
					//startingRegion = (Region)entity;
					// disaable serialization for Zone children of ZoneRoot
					// NOTE: PROBLEM WITH BELOW IS, the XMLDB.Write() call occurs ASYNCHRONOUSLY so we cannot
					// re-enable serialization until OnNodeWriteComplete and then hope that there's
					// no race condition in between!  we need a better way to prevent serialization of just Zone children type
					// on those occassions we are serializing ZoneRoot
					IGroup group = (IGroup)node;
					foreach (Node child in group.Children)
						if (child is Zone)
							child.Serializable = false;
				}

            // TODO: I think this entire function belongs in Scene.cs
            //       if the Scene has any need to manage itself as a result of this Save() call it should be handled by the scene
            //       A good example is managing automatically generated SpawnPoints

            // TODO: our test is, generate a universe that is 3x3.
            //       add a vehicle to a zone other than zone,0,0,0 
            //       set the vehicle's "hasViewpoint" flag == true.
            //       re-open the scene and see if we start at the zone the vehicle was placed in
            //if (node is Entity)
            // {
            //  Entity e = (Entity)node;
            //  // i suppose first we could delete all auto-managed Viewpoints and just create new ones under Scenes[0].XMLDB.Info
            //  // note: we need to then explicitly write the changed Info via a XMLDB.Write() call
            //  // _core.SceneManager.Scenes[0].XMLDB.Write(info, recurseChildren, saveOnWrite, OnNodeWriteComplete);

            //  // the idea is that we only need to manage Viewpoints if 
            //  // 0) we are in EDIT mode.  "HasViewpoint" cannot be changed during "ARCADE"
            //  // 1) the node that changed its property is an Entity and the flag being changed was "HasViewpoint"
            //  // 2) a node with "hasViewpoint" flag is removed during EDIT mode so we need to remove the Viewpoint for it
            //  //    NOTE: during ARCADE any change to an Entity must never result in the Scene being saved (overwritten)
            //  //          We only change the .saves data
            // }

            // TODO: do a search for all hard coded .Scenes[0] and remove them
            System.Diagnostics.Debug.Assert(mScene == _core.SceneManager.Scenes[0]);
	        _core.SceneManager.Scenes[0].XMLDB.Write(node, recurseChildren, saveOnWrite, OnNodeWriteComplete);
        }

        


        private void OnNodeWriteComplete(Amib.Threading.IWorkItemResult result)
        {
            Keystone.IO.WriteContext context = (Keystone.IO.WriteContext)result.State;
            System.Diagnostics.Debug.WriteLine("FormMain.Commands() - Written Node '" + context.Node.TypeName + "'");
            
			if (context.Node is ZoneRoot)
			{
				// re-enable serialization for Zone children of ZoneRoot
				IGroup group = (IGroup)context.Node;
				foreach (Node child in group.Children)
					if (child is Zone)
						child.Serializable = true;
			
				// Feb.14.2016 - Monitor.Exits added
				//System.Threading.Monitor.Exit (mSaveNodeLock);
			}
			//else if (context.Node is Zone)
                // TODO: I think if ZoneRoot Exits first then the subsequent child Zone will fail below
				//System.Threading.Monitor.Exit (mSaveNodeLock);

        
        }
    }
}
