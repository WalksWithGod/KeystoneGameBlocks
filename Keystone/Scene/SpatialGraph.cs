//using System;
//using System.Collections.Generic;
//using Keystone.Cameras;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Portals;
//using Keystone.Resource;
//using Keystone.Traversers;
//using Keystone.Types;

//namespace Keystone.Scene
//{
//    public class SpatialGraph
//    {
//        public RegionNode _root;
//        //   private ITraverser _culler;
//        private Traversers.Picker _picker;
//        private Locator _locator;
//        private SceneBase _scene;

//        private delegate void EntityMovedCallback(Entity entity);

//        private delegate void EntityDestroyedCallback(Entity entity);

//        private SpatialGraph(SceneBase  scene)
//        {
//            if (scene == null) throw new ArgumentNullException();
//            _scene = scene;

//            //_culler = new Culler();
//            _picker = new Traversers.Picker();
//            _locator = new Locator(_root);
//        }

//        public SpatialGraph(SceneBase scene, Region rootRegion)
//            : this(scene)
//        {
//            _root = new RegionNode(rootRegion);
//            rootRegion.SceneNode = _root;
//        }

//        // simulation update that focuses specifically on movement and response.
//        // right now when an entity moves, it set's changeflags on its SceneNode and the SceneNode's bounding volume is dirty
//        // and results in sceneNode UpdateBoundingVolume, but what should be happening also is the SpatialGraph should
//        // determine if a SceneNode has crossed a boundary and whether it needs to move parents or whether it's straddling
//        // and needs an additonal node or whether its now fully crossed into another region and can have one or more SceneNode's
//        // removed.
//        // This is why in Simulation i had entities flag when they were moved and we itterated all entities.  Here the option is the same
//        // itterate all SceneNodes and update the ones that have been moved, or notify directly and add to a list of all that have changed
//        // and then just itterate the list... hrm...
//        public void Update (float elapsed)
//        {
//            // parallel for possible?  How would you do a parallel collision code?  Seems to me that collision must recurse
//            // after responses per itteration because in that time delta multiple collisions can have occurred.  Read the ebook 
//            // Real-Time Collision detection.  One thng it does is use the broadphase to identify clusters of objects that are seperated 
//            // and will only interact with other objects in their own cluster.  There we can apply threading to the different clusters.
//            // As for broadphase itself, we have some help by the fact we have seperate Regions and we will know what objects
//            // are definetly in their own region vs straddling multiple.
//            foreach (IResource resource in Repository.Items)
//            {
//                if (resource is SceneNode)
//                {
//                    //if (((SceneNode)resource).Enable )
//                    //{
//                        // has the sceneNode changed
//                        if (((SceneNode)resource).ChangeFlags != Enums.ChangeStates.None)
//                        {
//                            // is the node still in the world bounds

//                            // are we still entirely in one region (update all regionNodes first?)

//                                // if yes, do we have to delete any previous extra scene nodes?

//                                // if no, we have to create additional sceneNodes

//                            // did we cross a portal? sweep test required?

//                        }
//                    //}
//                }
//            }
//        }

//        private void EntityAddedHandler (Entity entity)
//        {
            
//        }

//        private void EntityMovedHandler (Entity entity)
//        {
            
//        }
//        private void EntityMoveHandler(Entity entity)
//        {
//        }

//        private void EntityDestroyedHandler(Entity entity)
//        {
//        }



//        //// note: all regions have their own local coordinate system so an offset is needed during traversal
//        //public SceneNode CreateSceneNode(EntityBase entity)
//        //{
//        //    SceneNode host;
//        //    if (entity is Region)
//        //    {
//        //        if (((Region) entity).IsOctreePartition)
//        //            host = new OctreeHost((Region) entity);
//        //        else
//        //            host = new RegionNode((Region) entity);
//        //    }
//        //    else
//        //    {
//        //        host = new EntityNode(entity);
//        //    }
//        //    //region.EntityMoved = new EntityMovedCallback(EntityMoveHandler);
//        //    //region.EntityDestroyed = new EntityDestroyedCallback(EntityDestroyedHandler);

//        //    return host;
//        //}

//        //internal Region FindCameraRegion(Vector3d position)
//        //{
//        //    Region found = FindRegion(position);
//        //    // if for some reason the camera timing gets messed up, the camera may end up way out of bounds and found will == null. In that case start at root
//        //    if (found == null) found = _root.Region;

//        //    // for interior regions, for purposes of portal rendering we always start in the camera's curent sector if it exists. Otherwise we must start at root
//        //    if (! (found is Interior)) found = _root.Region;
//        //    return found;
//        //}

//        //// Attempts to find a region which contains the position.
//        //public Region FindRegion(Vector3d position)
//        //{
//        //    return FindRegion(position, _root.Region);
//        //}


//        //public Region FindRegion(Vector3d position, Region start)
//        //{
//        //    if (start != null && start.RegionNode != null)
//        //    {
//        //        Locator locator = new Locator(_root);
//        //        // the root is not really a pure sector and is the only one that can completely overlap other sectors. <-- TODO: this might change as we allow smaller ships inside bigger ships 
//        //        // so let's ignore that one and test if we've entered a "real" sector
//        //        // TODO: there's a problem with our Sectors though. If Sectors are not regions then they dont get returned here
//        //        //       and when we're looking for a sector to start portal culling all we get is the sector's parent.
//        //        // TODO: the other issue is when we "search" we want the SceneNode because it contains a world bounding box...
//        //        //       yet ive been resistant to that idea.  But the SceneNode is faster because it allows for skipping of entire child branches quickly
//        //        //       There shouldnt be a problem with regions that are not connected into the SpatialGraph because they're simply not active
//        //        locator.Search(start.RegionNode, position);
//        //        return locator.Result.Region;
//        //    }

//        //    // still here so return the root region as default
//        //    return _root.Region;
//        //}

        

//        //internal Region[] GetAdjacents(int x, int y, int z)
//        //{
//        //    // TODO: we're not going to worry about optimizing at this point, just get it working.
//        //    bool north, south, east, west, front, back;
//        //    north = south = east = west = front = back = false;

//        //    // verify the x,y,z is in bounds and add that zone first
//        //    if (InBounds(x, y, z))
//        //    {
//        //        List<Region> regions = new List<Region>();

//        //        // add the current zone
//        //        //regionNames.Add(ref regionNames, x,y,z);
//        //        AddRegion(ref regions, x, y, z);


//        //        // add the zone immediately to the west
//        //        if (InBounds(x - 1, y, z)) AddRegion(ref regions, x - 1, y, z);
//        //        // add the zone immediately to the east
//        //        if (InBounds(x + 1, y, z)) AddRegion(ref regions, x + 1, y, z);
//        //        // add the zone immediately to the south
//        //        if (InBounds(x, y - 1, z)) AddRegion(ref regions, x, y - 1, z);
//        //        // add the zone immediately to the north
//        //        if (InBounds(x, y + 1, z)) AddRegion(ref regions, x, y + 1, z);
//        //        // add the zone immediately in front
//        //        if (InBounds(x, y, z - 1)) AddRegion(ref regions, x, y, z - 1);
//        //        // add the zone immediately behind
//        //        if (InBounds(x, y, z + 1)) AddRegion(ref regions, x, y, z + 1);


//        //        // for the non cardinal (diagonal) directions
//        //        if (InBounds(x + 1, y + 1, z)) AddRegion(ref regions, x + 1, y + 1, z); // +y (+x)
//        //        if (InBounds(x + 1, y - 1, z)) AddRegion(ref regions, x + 1, y - 1, z); // -y (+x) 
//        //        if (InBounds(x + 1, y, z - 1)) AddRegion(ref regions, x + 1, y, z - 1); // -z (+x)  
//        //        if (InBounds(x + 1, y, z + 1)) AddRegion(ref regions, x + 1, y, z + 1); // +z (+x)  

//        //        if (InBounds(x - 1, y + 1, z)) AddRegion(ref regions, x - 1, y + 1, z); // -x (+y)
//        //        if (InBounds(x - 1, y - 1, z)) AddRegion(ref regions, x - 1, y - 1, z); // -x (-y) 
//        //        if (InBounds(x - 1, y, z + 1)) AddRegion(ref regions, x - 1, y, z + 1); // +z (-x)  
//        //        if (InBounds(x - 1, y, z - 1)) AddRegion(ref regions, x - 1, y, z - 1); // -z (-x)  

//        //        if (InBounds(x, y + 1, z + 1)) AddRegion(ref regions, x, y + 1, z + 1); // +y (+z)
//        //        if (InBounds(x, y + 1, z - 1)) AddRegion(ref regions, x, y + 1, z - 1); // +y (-z) 
//        //        if (InBounds(x, y - 1, z + 1)) AddRegion(ref regions, x, y - 1, z + 1); // -y (+z)  
//        //        if (InBounds(x, y - 1, z - 1)) AddRegion(ref regions, x, y - 1, z - 1); // -y (-z)  

//        //        // the furthest corner diagonals
//        //        if (InBounds(x + 1, y + 1, z + 1)) AddRegion(ref regions, x + 1, y + 1, z + 1); // +x (+y) +z
//        //        if (InBounds(x + 1, y + 1, z - 1)) AddRegion(ref regions, x + 1, y + 1, z - 1); // +x (+y) -z
//        //        if (InBounds(x + 1, y - 1, z + 1)) AddRegion(ref regions, x + 1, y - 1, z + 1); // +x (-y) +z
//        //        if (InBounds(x + 1, y - 1, z - 1)) AddRegion(ref regions, x + 1, y - 1, z - 1); // +x (-y) -z 

//        //        if (InBounds(x - 1, y + 1, z + 1)) AddRegion(ref regions, x - 1, y + 1, z + 1); // -x (+y) +z
//        //        if (InBounds(x - 1, y + 1, z - 1)) AddRegion(ref regions, x - 1, y + 1, z - 1); // -x (+y) -z
//        //        if (InBounds(x - 1, y - 1, z + 1)) AddRegion(ref regions, x - 1, y - 1, z + 1); // -x (-y) +z
//        //        if (InBounds(x - 1, y - 1, z - 1)) AddRegion(ref regions, x - 1, y - 1, z - 1); // -x (-y) -z 

//        //        return regions.ToArray ();
//        //    }
//        //    return null;
//        //}

//        //// TODO: how do we deal with zone's within zones like inside of ships?  does that have to follow portal rules where the camera must traverse
//        //// to those regions unless we "jump" to a specific region in which case we already know implicitly where the camera is placed?
//        //// what if i go to the "zones are connected by portals" method instead?  the downside is there i would need allot of portals loaded
//        //// for each zone off the main zone to know which ones to load, but then there is no traversal from the main zone, it requires jumps
//        //internal List<string> GetAdjacentZoneNames(int x, int y, int z)
//        //{
//        //    bool north, south, east, west, front, back;
//        //    north = south = east = west = front = back = false;

//        //    // verify the x,y,z is in bounds and add that zone first
//        //    if (InBounds(x, y, z))
//        //    {
//        //        List<string> regionNames = new List<string>();

//        //        // add the current zone
//        //        AddRegionName(ref regionNames, x, y, z);

//        //        // add the zone immediately to the west
//        //        if (InBounds(x - 1, y, z)) AddRegionName(ref regionNames, x - 1, y, z);
//        //        // add the zone immediately to the east
//        //        if (InBounds(x + 1, y, z)) AddRegionName(ref regionNames, x + 1, y, z);
//        //        // add the zone immediately to the south
//        //        if (InBounds(x, y - 1, z)) AddRegionName(ref regionNames, x, y - 1, z);
//        //        // add the zone immediately to the north
//        //        if (InBounds(x, y + 1, z)) AddRegionName(ref regionNames, x, y + 1, z);
//        //        // add the zone immediately in front
//        //        if (InBounds(x, y, z - 1)) AddRegionName(ref regionNames, x, y, z - 1);
//        //        // add the zone immediately behind
//        //        if (InBounds(x, y, z + 1)) AddRegionName(ref regionNames, x, y, z + 1);


//        //        // for the non cardinal (diagonal) directions
//        //        if (InBounds(x + 1, y + 1, z)) AddRegionName(ref regionNames, x + 1, y + 1, z); // +y (+x)
//        //        if (InBounds(x + 1, y - 1, z)) AddRegionName(ref regionNames, x + 1, y - 1, z); // -y (+x) 
//        //        if (InBounds(x + 1, y, z - 1)) AddRegionName(ref regionNames, x + 1, y, z - 1); // -z (+x)  
//        //        if (InBounds(x + 1, y, z + 1)) AddRegionName(ref regionNames, x + 1, y, z + 1); // +z (+x)  

//        //        if (InBounds(x - 1, y + 1, z)) AddRegionName(ref regionNames, x - 1, y + 1, z); // -x (+y)
//        //        if (InBounds(x - 1, y - 1, z)) AddRegionName(ref regionNames, x - 1, y - 1, z); // -x (-y) 
//        //        if (InBounds(x - 1, y, z + 1)) AddRegionName(ref regionNames, x - 1, y, z + 1); // +z (-x)  
//        //        if (InBounds(x - 1, y, z - 1)) AddRegionName(ref regionNames, x - 1, y, z - 1); // -z (-x)  

//        //        if (InBounds(x, y + 1, z + 1)) AddRegionName(ref regionNames, x, y + 1, z + 1); // +y (+z)
//        //        if (InBounds(x, y + 1, z - 1)) AddRegionName(ref regionNames, x, y + 1, z - 1); // +y (-z) 
//        //        if (InBounds(x, y - 1, z + 1)) AddRegionName(ref regionNames, x, y - 1, z + 1); // -y (+z)  
//        //        if (InBounds(x, y - 1, z - 1)) AddRegionName(ref regionNames, x, y - 1, z - 1); // -y (-z)  

//        //        // the furthest corner diagonals
//        //        if (InBounds(x + 1, y + 1, z + 1)) AddRegionName(ref regionNames, x + 1, y + 1, z + 1); // +x (+y) +z
//        //        if (InBounds(x + 1, y + 1, z - 1)) AddRegionName(ref regionNames, x + 1, y + 1, z - 1); // +x (+y) -z
//        //        if (InBounds(x + 1, y - 1, z + 1)) AddRegionName(ref regionNames, x + 1, y - 1, z + 1); // +x (-y) +z
//        //        if (InBounds(x + 1, y - 1, z - 1)) AddRegionName(ref regionNames, x + 1, y - 1, z - 1); // +x (-y) -z 

//        //        if (InBounds(x - 1, y + 1, z + 1)) AddRegionName(ref regionNames, x - 1, y + 1, z + 1); // -x (+y) +z
//        //        if (InBounds(x - 1, y + 1, z - 1)) AddRegionName(ref regionNames, x - 1, y + 1, z - 1); // -x (+y) -z
//        //        if (InBounds(x - 1, y - 1, z + 1)) AddRegionName(ref regionNames, x - 1, y - 1, z + 1); // -x (-y) +z
//        //        if (InBounds(x - 1, y - 1, z - 1)) AddRegionName(ref regionNames, x - 1, y - 1, z - 1); // -x (-y) -z 

//        //        return regionNames;
//        //    }
//        //    return null;
//        //}

//        //private void AddRegion(ref List<Region> regions, int x, int y, int z)
//        //{
//        //    regions.Add((Region)Repository.Get (string.Format(Region.PREFIX + "{0},{1},{2}", x, y, z)));
//        //}

//        //private void AddRegionName(ref List<string> regionNames, int x, int y, int z)
//        //{
//        //    regionNames.Add(string.Format(Region.PREFIX + "{0},{1},{2}", x, y, z));
//        //}

//        //private  bool InBounds(int x, int y, int z)
//        //{
//        //    //return (x >= _scene.SceneInfo.StartX && x <= _scene.SceneInfo.StopX &&
//        //    //        y >= _scene.SceneInfo.StartY && y <= _scene.SceneInfo.StopY &&
//        //    //        z >= _scene.SceneInfo.StartZ && z <= _scene.SceneInfo.StopZ);

//        //    return (x >= _scene.Root.StartX && x <= _scene.Root.StopX &&
//        //            y >= _scene.Root.StartY && y <= _scene.Root.StopY &&
//        //            z >= _scene.Root.StartZ && z <= _scene.Root.StopZ);
//        //}
//    }
//}