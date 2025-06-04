using System;
using System.Collections.Generic;
using System.Text;

namespace Keystone.IO
{
    public enum PageableNodeStatus : int
    {        
        NotLoaded = 0,
        Unloading,
        Loading,
        LoadedStandby,
        Loaded,
        Error
    }


    /// <summary>
    /// Pageable nodes _must_ reference a TV3D Resource such as a mesh, actor, particle system file, terrain data/heightmap, or texture that 
    /// must be loaded specifically by TV3D from disk.  
    /// </summary>
    internal interface IPageableTVNode
    {
        // index is not guaranteed to be unique across nodes types.  It is strictly for TV3D factory indices
        // so that for instance, we can retreive the mesh index
        int TVIndex { get; }
        bool TVResourceIsLoaded { get; }
        string ResourcePath { get; set; }
        PageableNodeStatus PageStatus { get; set; }
        // when starting to page we can lock _enabled, set it to false if its true, page it in, then if it was true return to true, then unlock
        // NOTE: resource loading is not -just- file loading but also tv instantiation into TVScene
        void LoadTVResource();
        void UnloadTVResource();
        void SaveTVResource(string filepath);
        object SyncRoot { get ; }

    }

    internal class PageableSaveInfo
    {
        public IPageableTVNode node { get; set; }
        public string FullPath { get; set; } // normally we use IPageableTVNode.ResourcePath but sometimes we need ability to save to a different path.  This path overrides that if string.IsNullOrEmpty == false
    }

    //private struct PageEntry
    //{
    //    private int Priority;// perhaps a way of not needing to sort anything if instead priority can just be used when adding to the list and also it can be modified

    //    // PageEntryType _type;  // some types it seems makes no sense to store as attributes when creation of the actual node will take up less memory and be very fast.
    //    // e.g.  a Light node.  Perhaps its just certain aspects of paging like Meshes, Soundfiles, Hulldata, textures that are slow...
    //    // so what do we do?
    //    private NodeAttributeGroup _attributes;
    //    private Vector3d _position;
    //    private float _radius;// keep in mind that ultimately our server needs to load hulls and bounding boxes because it wont be loading meshes.  So this data is needed in the xml archive
    //    private float _distanceToCameraSquared;
    //    private Node _instantiatedNode;
    //    private PageState _status;
    //    private IWorkItemResult _workItemResult; // most for 

    //    public void Cancel()
    //    {
    //        if (_workItemResult != null && !_workItemResult.IsCompleted && !_workItemResult.IsCanceled)
    //            _workItemResult.Cancel();
    //    }

    //    public double DistanceToCameraSquared(Vector3d cameraPosition)
    //    {
    //        // if not instantiated, we have to compute it.
    //        // if it is instantiated and added to the scene, then determining if it needs to be unloaded is a matter of checking the distanceSquared already set 
    //        // during render traversal.
    //        return Vector3d.GetDistance3dSquared(cameraPosition, _position);
    //    }
    //}

    //private class PageInfo
    //{
    //    private string _name;
    //    private IWorkItemResult _workItemResult; // only good fo
    //    private Dictionary<string, PageEntry> _entries;
    //}
}