using System;
using System.Collections.Generic;
using Amib.Threading;
using Keystone.Cameras;
using Keystone.Commands;
using Keystone.Octree;

namespace Keystone.IO
{
    public class ClientPager : PagerBase 
    {
    	// All Ranges are in whole Zones and DOES NOT COUNT current camera (x,y,z) zone as part of range.
    	// Thus a range of '1' returns current zone AND all immediate adjacents.  A range of '0'
    	// returns the camera's current zone only.
    	// TODO: these should be got from App.Settings
    	private uint ZONE_VISIBLE_RANGE = 1; // 5; // use 5,6,7 for terrain.  2,3,4 for space
    	private uint ZONE_PAGE_IN_RANGE = 2; //6;
    	private uint ZONE_PAGE_OUT_RANGE = 3; // 7;
    	

    	private List<string> _lastAdjacents;
        


        public ClientPager(Scene.Scene scene, SmartThreadPool threadPool, int concurrency, PageCompleteCallback emptyRegionPageCompleteHandler, PageCompleteCallback regionChildrenPageCompleteHandler)
            : base(scene, threadPool, concurrency, emptyRegionPageCompleteHandler, regionChildrenPageCompleteHandler)
        {

//        	CoreClient._CoreClient.Settings.settingReadInteger ("performance", "zone visible range");
//        	CoreClient._CoreClient.Settings.settingReadInteger ("performance", "zone pagein range");
//        	CoreClient._CoreClient.Settings.settingReadInteger ("performance", "zone pageout range");

            if (scene == null) throw new ArgumentNullException ();
            
            System.Diagnostics.Debug.Assert (ZONE_VISIBLE_RANGE <= ZONE_PAGE_IN_RANGE, "ClientPager.ctor() - Visible range must be less than or equal to range at which zones are paged in.");
            System.Diagnostics.Debug.Assert (ZONE_PAGE_IN_RANGE <= ZONE_PAGE_OUT_RANGE, "ClientPager.ctor() - Page IN range must be less than or equal to Page OUT range.");
        }

        // for auto-tile to work properly at boundaries, layer data for non-paged in zones at +1 zone distance further out must be made available
        private uint LAYER_PAGE_IN_RANGE {get {return ZONE_PAGE_IN_RANGE + 1;}}
        
        // TODO: if a scene is unloaded, all queued pages for that scene need to be aborted?
        /// <summary>
        /// Update to determine if any other region's need to be passed in.
        /// Every scene is stored in a single XMLDB.  Resources being loaded
        /// from mod db's is an entirely different type of resource db loading.
        /// </summary>
        /// <param name="scene"></param>
        public override void Update()
        {
        	List<string> layerPageInRange = new List<string>();
            List<string> zonesPageInRange = new List<string>();
			List<string> zonesVisibleRange = new List<string>();
            List<string> zonesPageOutRange = new List<string>();

            // TODO: here for each Viewport we are determining which Regions to
            // page in or page out, but I don't think we are handling cases where
            // one context.Viewpoint results in a particular zone being paged in
            // and another context.Viewpoint later in the loop, resulting in that
            // same zone to be paged out.  In other words, we don't do any kind of
            // reference counting or comparison of zone id's in the "zonesPageInRange"
            // and the "zonesPageOutRange"
            foreach (Viewport vp in CoreClient._CoreClient.Viewports.Values)
            {
            	// Following line MUST NOT skip if (!vp.Enabled) because that
				// could be just for a document tab that does not have focus.  
				// Then when switching documents, and in a multiZone scene, all 
				// of it's adjacents will have been paged out and they wont be 
				// paged back in again in time to prevent error in Repository.IncrementRef()
                // if (!vp.Enabled) continue;   
                RenderingContext context = vp.Context;
                if (context == null) continue;
 
                Keystone.Entities.Viewpoint viewpoint = context.Viewpoint ;
                if (viewpoint == null) continue;

                if (viewpoint.Region == null)
                {
                    // Add the viewpoint to the entity it's attached to
                    // TODO: note, here im using viewpoint.StartRegionID but AttachedToEntityID should 
                    // be what we use... and the viewpoint should be added to it...
                    // TODO: unless those are different types of Viewpoints... hrm...
                    // or are done at runtime?
                    // TODO: and should we be looping through viewpoints for one that's enabled?
                    // what if user dies and respawns and server wants to give them a new viewpoint?
                    // hrm... 
                    // in vrml
                    // http://accad.osu.edu/~pgerstma/class/vnv/resources/info/AnnotatedVrmlRef/ch3-353.htm
                    // they use a stack system where we can have for instance the starting node as first
                    // on stack, then when traversing if encountering another Viewpoint, that Viewpoint
                    // gets pushed to top and the context gets reparented to that viewpoint...
                    // hrm..
                    // perhaps we do put some Viewpoints inline if they are ones that are not starting viewpoints
                    // which must start in regions.  Then we put potential starting viewpoints in SceneInfo
                    // and server can randomly assign users to them.  And then SceneInfo.Current
                    // can always be that starting one.  So no looping through viewpoints in SceneInfo
                    // just check _scene.CurrentViewpoint
                    Resource.IResource res = Resource.Repository.Get(viewpoint.StartingRegionID);
                    if (res == null)
                    	// Add this Region to be paged in immediately since our viewpoint is currently
                    	// without an instantiated Region!
                        zonesPageInRange.Add(viewpoint.StartingRegionID);
                    else
                    {
                        if (res is Keystone.Portals.Region)
                        {
                        	// starting region has been paged in.  Add viewpoint to it
                        	// and initilaize it's translation to starting translation
                            ((Portals.Region)res).AddChild(viewpoint);
                            viewpoint.Translate (viewpoint.StartingTranslation); 
                        }
                        else throw new Exception("CliengPager.Update() - ERROR: Unsupported StartingRegion type... should we add support?");
                    }
                }
                else
                {
                    // if the Viewpoint is connected ultimatley to a Zone
                    // then load the adjacents to this region 
                    if (viewpoint.Region is Portals.Root)
                    {
                        // do nothing
                    }
                    else if (viewpoint.Region is Portals.Interior)
                    {
                        // do nothing, this celledregion is already paged in
                        // and there's no adjacents ever right?
                        // note: Portals.Structure is not same as TileMap.Structure
                    }
                    else
                    {
                    	// TODO: I could also test if the last viewpoint.Region's have changed
                    	//       and then I don't even have to get zone names, although I do think 
                    	//       after going from .StartingRegionID above to viewpoint.Region != null
                    	//       requires we go through this top part of the loop at least twice 
                    	//       
                        Portals.Zone currentZone = (Portals.Zone)viewpoint.Region; //.GetZone();
                        if (currentZone != null)
                        {
                        	// get the names of zones within range of the current
                        	// so we can page them in by name.
                        	// NOTE: we can set as .visible = false after loading ones
                        	// that are further away.  
                            string[] range = currentZone.GetAdjacentZoneNames(ZONE_PAGE_IN_RANGE);
                            
                            if (range != null)
                            	// append
                                zonesPageInRange.AddRange(range);
                            
                            // layer's page in distance is always +1 of zone_page_in_range 
                            // so that autotile data is available in time
                            range = currentZone.GetAdjacentZoneNames (LAYER_PAGE_IN_RANGE);
                            if (range != null)
                            	layerPageInRange.AddRange(range);
                            
                             
                            // zone's that are within visible range 
                            range = currentZone.GetAdjacentZoneNames (ZONE_VISIBLE_RANGE);
                            if (range != null)
                            	zonesVisibleRange.AddRange(range);
                           
                        }
                    }
                }
            } // end foreach

            // we use a seperate foreach to find zones to page out because
            // this way the above foreach is able to take into account all viewports in all contexts
            // and so below we have full list of exempted zones before we start
            // trying to add any to zonesPageOutRange
            foreach (Viewport vp in CoreClient._CoreClient.Viewports.Values)
            {
                RenderingContext context = vp.Context;
                if (context == null) continue;

                Keystone.Entities.Viewpoint viewpoint = context.Viewpoint;
                if (viewpoint == null) continue;

                if (viewpoint.Region is Portals.Root)
                {
                    // do nothing
                }
                else if (viewpoint.Region is Portals.Interior)
                {
                    // do nothing, this celledregion is already paged in
                    // and there's no adjacents ever right?
                    // note: Portals.Structure is not same as TileMap.Structure
                }
                else
                {
                    Portals.Zone currentZone = (Portals.Zone)viewpoint.Region; //.GetZone();
                    if (currentZone != null)
                    {
                        // a zone gets paged IN at a certain distance, but we want
                        // it's page OUT distance to be a little further than that so
                        // that we dont have a hard invisible pageIN/pageOUT line that 
                        // the user can quickly hop across causing pager to thrash back
                        // and forth.  
                        string[] range = currentZone.GetAdjacentZoneNamesBeyondRange(ZONE_PAGE_OUT_RANGE);
                        if (range != null)
                        {
                            for (int i = 0; i < range.Length; i++)
                            {
                                if (zonesPageInRange.Contains(range[i])) continue;
                                if (zonesVisibleRange.Contains(range[i])) continue;
                                zonesPageOutRange.Add(range[i]);
                            }
                        }
                        // TODO: in future we may need to traverse through portals
                        //       to find Zones in range
                    }
                }
            }


            // NOTE: We can have multiple scenes loaded such as when a preview prefab window is open. 
            // So it's important we don't attempt to page in when the _scene for this ClientPager has no
            // ZoneRoot
            if (this._scene.Root is Portals.ZoneRoot == false) 
                return;
            
            // Page In Zones that are in visible range but which are not currently loaded
            lock (mZonePagingLock)
            {
            	// if adjacents HAVE CHANGED since last Update(), find the ones we need to page in
                if (!AdacentsEqual(zonesPageInRange, _lastAdjacents))
                {
                	// FOR NON-EXTERIOR SPACE ZONES (i.e. StructureLevelsHigh > 0) we 
                	// load in MapLayers for all Zones we'll be paging in PLUS ONE further zone out which will
                	// not be visibly represented, but needed for proper AutoTiling of the outermost visible zones.
					// NOTE: We load all the MapLayers of all Structures before we page in the Zones and their Structure 
                	// entites so that adjacent data will be available for all our Structures so AutoTiling performs correctly. 
                	if (this._scene.Root is Portals.ZoneRoot && ((Portals.ZoneRoot)this._scene.Root).StructureLevelsHigh > 0)
            	    {
	                	for (int i = 0; i < layerPageInRange.Count; i++)
	                    {
	                        if (!mLoadedLayers.Contains(layerPageInRange[i]))
	                        {
	                        	// ZoneRoot is guaranteed to be loaded prior to calling this since it's loaded by default during inital loading of Scene
	                        	LoadZoneMapLayers ((Portals.ZoneRoot)this._scene.Root, layerPageInRange[i]);
	                        }
	                	}
            	    }
                    // load Zone's themselves - if a dictionary key doesnt exist, then that Zone has not been loaded.
                    for (int i = 0; i < zonesPageInRange.Count; i++)
                    {
                        if (!mLoadedRegions.ContainsKey(zonesPageInRange[i]))
                        {
                            // NOTE: SmartThreadPool does not support delegate chaining so we can only assign one handler
                            PostExecuteWorkItemCallback cb = RegionPageInCompleteHandler;
                            PageInZoneAsychronously(zonesPageInRange[i], cb);
                        }
                    }
                }
                
                
                // any paged in zone that is not Enabled and Visible and which is now in VisibleRange
                // we must enable.  Any that is beyond Visible range we will Disable
                if (zonesVisibleRange  != null)
	                foreach (string key in mLoadedRegions.Keys)
    	            {
                		bool show = zonesVisibleRange.Contains(key);
                		if (mLoadedRegions[key] == null) continue;
                			
            			mLoadedRegions[key].Enable = show;
            			mLoadedRegions[key].Visible = show;
    	            } 
                
                
                // The previous automatically will load with the help of sorting, those nodes which are closest to the camera (once we add the sorting code)
                // However, we also need to UNLOAD from back to front those items that are in pages that arent being unloaded but where other nodes within those Regions
                // are being unloaded because they are now too far. In other words, even though Regions are the first loaded and the last to be unloaded, the items they
                // contain are also loaded and unloaded by distance.
                // An easy way for unloading is to unload things that are outside of the bounding radius and to also unload them from back to front.
                // The question i think is, do we try to do all that stuff here or do we also try to use the SceneManager and a special traversal?
                // 1) we have to read in all region data but we dont have to load any that are beyond the "load" distance

                // Begin unloading, removal from scene, and then removal of region key in _pageTable of any entries in the _pageTable
                // that are NOT in the currentAdjacents
                // TODO: we also need to unload any that may also be in our instantiated list (since we add those to the list so that we can update to the sceneManager all at once)

                List<string> regionsToUnload = new List<string>();
                if (zonesPageOutRange != null)
	                foreach (string key in mLoadedRegions.Keys)
    	            {
	                	if (mRegionsUnloadingInProgress.ContainsKey (key)) continue;
    
	                	if (zonesPageOutRange.Contains(key))
    	            	{
	                    	// TODO: the list of adjacents for regions to unload should be +1 further distance away
	                    	//       but if right AT the distance to unload, and they are loaded, they should just be
	                    	//       enable=false or visible=false (since we want simulation on them enabled potentially, just not rendered or pickable)
	                    	
	                        // TODO: verify that if this page is now no longer in the list of adjacents then all nodes within that region
	                        // must also be unloaded already.  
	                        // TODO: but this raises a question about how move-able static geometry is handled.  If something crosses a boundary
	                        //       then this data _must_ not be simply viewed as tied to that previous zone.  Further, when that data moves doesnt it mean
	                        //       the file data needs to be updated?  Bottom line is what we do know is that after we've read in the data, 
	                        //       we still have to treat it as something perhaps moveable even when its not acually instantiated (e.g if the server sends us some
	                        //       update about the entity with that same GUID)
	                        System.Diagnostics.Debug.Assert (regionsToUnload.Contains (key) == false, "ClientPager.Update() - ERROR: Do not attempt to unload same region twice.");
    	                    regionsToUnload.Add(key);    	            		
    	                }
    	            }

			
				// Page Out Zones that have fallen out of visible range
                foreach (string key in regionsToUnload)
                {
                    if (mLoadedRegions[key] != null)
                    {
                        // TODO: The complication with removing regions completely is that
                        // we may still have AI and worlds on the clients in zones beyond the players 
                        // visibility that we still must simulate.  We should be able to unload RESOURCES
                        // (textures, geometry, particle sysystems, etc) but we should not necessarily
                        // remove entities themselves.
                        // However, in the case that we did want to do so, unloading zones completely should work
                        // and making sure it works should make all our code generally more robust
                        // TODO: also, in the event i do completely unload some Zones, then they must be simulated
                        // using a "digest" method of some kind where the simulation engine switches to
                        // one that is more statistical and less real-time agent based.  Here we would still
                        // have individual entities, but their states would be computed more as a 
                        // top down SYSTEMS simulation rather than bottom up AGENT simulation.
                        Keystone.Portals.Zone zone = (Keystone.Portals.Zone)Keystone.Resource.Repository.Get(key);
                        System.Diagnostics.Debug.Assert (mLoadedRegions[key] == zone, "ClientPager.Update() - ERROR: Entity in Repository does not match mLoadedRegions[key].");
                        
                        System.Diagnostics.Debug.Assert (zone != null, "ClientPager.Update() - ERROR: Cannot unload Zone that does not exist in Repository");

                        PageOutZoneAsychronously(zone, RegionPageOutCompleteHandler);
                    }
                }
                regionsToUnload.Clear();
                
                
				// Asychronous, recursive read OF THE CHILDREN of zones that have completed loading
                foreach (Keystone.Portals.Region r in mQueuedRegionsCompleted)
                {   	
                    Traversers.SuperSetter super = new Traversers.SuperSetter(_scene.Root);
                    try 
                    {
                    	//mark zone as enable = false and visible = false until it's children are done loading
   	                	r.Visible = false;
   	                	r.Enable = false;
   	                	    
	                   	// note: Add region to Scene before asychronous read of children begins
	                   	// or else there will be a race condition to create any ISpatialNode within
	                   	// the RegionNode that will be created for this Zone and any children (eg TileMap.Structure)
	                   	// whos own EntityNode must be inserted into the ISpatialNode
                    	super.Apply(r);	                	
                    }
                    catch (Exception ex)
                    {
                    	System.Diagnostics.Debug.WriteLine ("ClientPager.Update() - ERROR - Cannot add child node to parent type. " + ex.Message);
                    }


                    // Asychronous, recursive read OF THE CHILDREN of this zone.  NOTE: this only applies for 
                    // instantiation of Node and does not directly call LoadTVResource() on any descendant node.
                    // After the node is deserialized, a call to .AddChild() occurs which triggers Repository to call
                    // IO.PagerBase.QueuePageableResource()
                    string regionID = r.ID;
                    _scene.XMLDB.Read(r.TypeName, regionID, true, false, null, ZoneChildrenPagingCompletedHandler);
                    mLoadedRegions[regionID] = r; // assign to the reserved key, the actual reference to the loaded region
                    //if (regionID == "Zone,8,0,5")
                    //    System.Diagnostics.Debug.WriteLine("ClientPager.Update " + regionID);

                }
                mQueuedRegionsCompleted.Clear();


                _lastAdjacents = zonesPageInRange;
            } // end lock (mZonePagingLock)
        }

    	/// <summary>
    	/// Loads all map layers for the named zone BEFORE the zone has been paged in.
    	/// </summary>
    	/// <remarks>
        /// NOTE: We load all the MapLayer data of all Structures before we page in the Zones and their Structures
    	/// so that adjacent data will be available for all our Structures so AutoTiling performs correctly. 
    	/// </remarks>
    	/// <param name="zoneName"></param>
        protected void LoadZoneMapLayers (Keystone.Portals.ZoneRoot zoneRoot, string zoneName)
        {
        	// TODO: WARNING: The only reason this method exists here in ClientPager.cs is because we need to load
        	//       map layer data BEFORE we load in the structures for each zone in order for each structure to AutoTile
        	//       properly with adjacent zones that might not be paged in yet.
        	int subscriptX, subscriptY, subscriptZ;
        	
        	// NOTE: since the zone is not even paged in yet, we must read the
        	//       map layer data from disk.
        	Keystone.Portals.ZoneRoot.GetZoneSubscriptsFromName (zoneName, out subscriptX, out subscriptY, out subscriptZ);
            	
        	string path = Keystone.TileMap.Structure.GetLayersDataPath (subscriptX, subscriptZ);

            // TODO: this parse code is redundant from inside of Structure.
            // read all the text and parse it
            if (System.IO.File.Exists(path) == false) return; // TODO: for tvlandscape terrain, the path does not exist. i need to refresh my memory on how this used to work... is this LoadZoneMapLayers only for Structure based Terrain?

        	string persistString = System.IO.File.ReadAllText (path);
        	
			char DELIMITER = ',';  // TODO: use global constant for this delim
			string[] csv = persistString.Split(DELIMITER); 
			
			int index = 0;
			int numLevels = int.Parse(csv[index++]);
			
			if (numLevels > 0)
			{
				float floorWidth = float.Parse (csv[index++]);
				double floorHeight = double.Parse (csv[index++]);
				float floorDepth = float.Parse (csv[index++]);
			
				for (int i = 0; i < numLevels; i++)
				{	
					int floorLevel = int.Parse (csv[index++]);
					int numLayers = int.Parse(csv[index++]);
					
					for (int j  = 0; j < numLayers; j++)
					{
						string layerName = csv[index++];
						
						// Load the map layers					
						// NOTE: our ZoneRoot node must have already been instantiated prior to trying to load any MapLayers
		        		Keystone.TileMap.MapLayer layer = Core._Core.MapGrid.Create (layerName,
	    	                            subscriptX, 
	    	                            subscriptY + floorLevel + (int)zoneRoot.StructureGroundFloorIndex, 
	    	                            subscriptZ,
	    	                            floorLevel, 
	    	                            floorWidth, (float)floorHeight, floorDepth); // floorHeight is perLevel not per Structure
					}
				}    
			}
			
			mLoadedLayers.Add (zoneName);
        }
        

        private void PageInZoneAsychronously(string zoneID, PostExecuteWorkItemCallback postExecute)
        {
        	mLoadedRegions.Add(zoneID, null);
            //System.Diagnostics.Debug.WriteLine ("ClientPager.PageInZoneAsychronously() - Loading zone " + zoneID);
            
            // TODO: i've added the property FileManager.EnableWrite = false; but imagine we are in editor and scrolling and
            // paging in data while adding something new at the same time?  how do we make sure that gets saved but the paged stuff doesnt?
          
            // It has to be a Zone typeName because non multizoned maps don't have adjacents right?
            
            // TODO: if this required branch is currently being paged OUT then we must wait before we can begin
            //       paging it in.  we should try to load in other zones instead and setup a timeout value
            //       by which to throw exception if it still hasn't paged out so that we can start paging it in again
        #if DEBUG
            Keystone.Elements.Node node = (Keystone.Elements.Node)Keystone.Resource.Repository.Get (zoneID);
            System.Diagnostics.Debug.Assert (node == null, "Node we're paging in should already be null.  " +
                                             "If not, make sure Zone nodes are NEVER serialized as children to ZoneRoot! " +
                                             "Zone children should be paged in here and never be allowed to deserialize as XML children off ZoneRoot." +
                                             "Or if that is not the problem..." +
                                             "If not and ResourceStatus of that node is in process of paging out... " +
                                             "we will have to wait til it's fully unloaded before we can start to page it in right?");
                                             
        #endif
            
            // Asychronous backround read the zones but DO NOT recurse their children yet.  
            _scene.XMLDB.Read("Zone", zoneID, "", false, false, null, postExecute);
        }

        // WARNING: This actually CANNOT (yet) be done asynchronously because we are modifying the scene directly
        // which we cannot allow.  So for now, we comment out the QueueWorkItem() and just call PageOutZone() directly
        private void PageOutZoneAsychronously(Portals.Region region, PostExecuteWorkItemCallback postExecute)
        {
            region.Enable = false;
        	mRegionsUnloadingInProgress.Add (region.ID, region);

            // Execute paging out of Region from main thread.  This is required
            // because we are directly modifying the Scene by removing Nodes.
            PageOutZone(region);

            // NOTE: The problem below is the postExecute callbacks are called on the worker thread and meanwhile
            // the Scene.Update() can occur on Entities that are only partially paged out (eg. removed from Repository but not
            // deactivated from Scene.ActiveEntities

            //if (postExecute != null)
            //    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(PageOutZone), region, postExecute);
            //else
            //    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(PageOutZone), region, RegionPageOutCompleteHandler);        	
        }

        private bool AdacentsEqual(List<string> list1, List<string> list2)
        {
            if (list1 == null || list2 == null) return false;

            if (list1 == null && list2 == null) return true;

            if (list1.Count != list2.Count) return false;
            if (list1.Count == 0) return true;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i]) return false;
            }

            // still here, must be equal
            return true;
        }

    }
}
