using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amib.Threading;
using Amib.Threading.Internal;

using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;

using Keystone.Portals;
using Keystone.Resource;
using Keystone.Types;

namespace Keystone.IO
{
    // UPDATE:   Pager should really only be responsible for sorting and determing what needs to be loaded and then pass
    //           that off to the FileManager.
    //           It should then be able to cancel FileManager operations if those IPabeableBranch are no longer needed 
    public abstract class PagerBase : IDisposable
    {
        public delegate void PageCompleteCallback(Keystone.IO.ReadContext context);
    	protected object mZonePagingLock;

        protected static IWorkItemsGroup mPagerThreadPoolGroup;
        protected static bool mDisabled;
        
    	protected bool _disposed = false;
                
    	protected List<Entity> mQueuedRegionsCompleted;  // regions which were queued, loaded, and are currently empty and need to have their children loaded
        protected Dictionary<string, Entity> mLoadedRegions; // regions loaded fully including children.  Dictionary key is the name of a region
        protected List <string> mLoadedLayers;
        protected Dictionary<string, Entity> mRegionsUnloadingInProgress;

        // These two callbacks are for notifying the calling App.Exe (eg FormMain)
        // so that it can act appropriately when a particular Zone/Region is paged in.
        PageCompleteCallback EmptyRegionPageInComplete_Handler;
        PageCompleteCallback RegionChildrenPageInComplete_Handler;

        protected Scene.Scene _scene;
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="threadpool"></param>
        /// <param name="concurrency"></param>
        /// <param name="emptyRegionPageCompleteHandler"></param>
        protected PagerBase(Scene.Scene scene, SmartThreadPool threadpool, int concurrency, PageCompleteCallback emptyRegionPageCompleteHandler, PageCompleteCallback regionChildrenPageCompleteHandler)
        {
            EmptyRegionPageInComplete_Handler = emptyRegionPageCompleteHandler;
            RegionChildrenPageInComplete_Handler = regionChildrenPageCompleteHandler;

            // create a workgroup that is seperate from any other SmartThreadPool groups we have
            // going such as SceneReader and SceneWriter			
            mPagerThreadPoolGroup = threadpool.CreateWorkItemsGroup(concurrency);
            //mPagerThreadPoolGroup.WIGStartInfo.WorkItemPriority = WorkItemPriority.Highest;
            mPagerThreadPoolGroup.Name = "PagerThreadpoolGroup";
            mPagerThreadPoolGroup.OnIdle += new WorkItemsGroupIdleHandler(PagerThreadGroupIdle);
            
            mZonePagingLock = new object();
            _scene = scene;

            mLoadedRegions = new Dictionary<string, Entity>();
            mLoadedLayers = new List<string>();
            mQueuedRegionsCompleted = new List<Entity>();
            mRegionsUnloadingInProgress = new Dictionary<string, Entity>();
        }


        // note; even in a game world with one main zone, we may still have inner regions with sectors that need to be paged.
        //       the most reliable attack seems to me, is to measure the distance to other portals.
        //       We know that inner regions must always have sectors that fully encompass the region's bounds.  
        //       So we know that a portal in an outer region that points to an inner region is pointing to one of the inner region's sectors.
        //       But thats not entirely straight foward.  if i point directly to a sector, how do i know which IPabeableBranch to page?
        //       Is it that the portal identifies both the region and the sector ID within that region?
        // when it comes to portals, when im culling one thing i should probably do is just add visible sectors thru portals to a list
        // so that if there are two portals pointing to the same area, i can not add it twice if that sector is already included.
        public abstract void Update();
        
        public static bool Disabled 
        {
        	get {return mDisabled;}
        	set {mDisabled = value;}
        }
        
        // adds the read item to the _toInstantiate list
        protected void RegionPageInCompleteHandler(IWorkItemResult result)
        {
            lock (mZonePagingLock)
            {
                Keystone.IO.ReadContext rc = (Keystone.IO.ReadContext)result.State;
                if (rc.Node != null)
                {
                	mQueuedRegionsCompleted.Add((Entity)rc.Node);
                }
                else
                {
                    // NOTE: Here we can just manually instance an empty Zone which 
                    // was not stored in XMLDB to conserve disk space for very large galaxies
                    // where 99% of the galaxy is comprised of empty zones anyway.
                    if (_scene.SceneInfo.SerializeEmptyZones == false)
                    {
                        int subscriptX, subscriptY, subscriptZ;
                        ZoneRoot root = (ZoneRoot)_scene.Root;
                        ZoneRoot.GetZoneSubscriptsFromName(rc.NodeID, out subscriptX, out subscriptY, out subscriptZ);
                        float offsetX = subscriptX + root.StartX;
                        float offsetY = subscriptY + root.StartY;
                        float offsetZ = subscriptZ + root.StartZ;
                        
                        BoundingBox box = root.GetChildZoneSize();


                        uint octreeDepth = uint.Parse(Core._Core.Settings.settingRead("scene", "octreedepth"));
                        Zone z = new Zone(rc.NodeID, box, octreeDepth, subscriptX, subscriptY, subscriptZ, offsetX, offsetY, offsetZ);
                        // must flag any Zone we create here as opposed to loading from disk
                        // as an EmptyZone
                        z.Attributes |= KeyCommon.Flags.EntityAttributes.EmptyZone;


                        mQueuedRegionsCompleted.Add(z);
                    }
                    else
                        Debug.Assert(false, "PagerBase.RegionPageInCompleteHandler() - Item not instantiated after read.");
                }

                if (EmptyRegionPageInComplete_Handler != null)
                    EmptyRegionPageInComplete_Handler(rc);
            }
        }
        
        // called when entities contained by the individual Zone's are fully paged in.
        // NOTE: this refers only to the XML reading and instantiation of nodes.  It does
        // not refer to Ipageable.LoadTVResource() having been called on any relevant
        // child descendants of the Zone that was loaded.
        protected void ZoneChildrenPagingCompletedHandler (IWorkItemResult result)
        {
        	lock (mZonePagingLock)
            {
                Keystone.IO.ReadContext rc = (Keystone.IO.ReadContext)result.State;
                if (rc.Node != null)
                {
                	Portals.Region r = (Portals.Region)mLoadedRegions[rc.ParentNodeID];
                	r.Visible = true;
                	r.Enable = true;

                    if (RegionChildrenPageInComplete_Handler != null)
                        RegionChildrenPageInComplete_Handler(rc);
                }
            }
        }


        protected void RegionPageOutCompleteHandler(IWorkItemResult result)
        {
            
        }

        internal object PageOutZone(object pageableObj)
        {
            //System.Diagnostics.Debug.WriteLine ("Pager.PageOutZone - Paging out Zone '" + ((Region)pageableObj).ID + "'");
            lock (mZonePagingLock)
            {
                Region region = (Region)pageableObj;
                
                // todo: is the following necessary? if so, since this function is occuring on the main thread, maybe we can just
                // asychronously perform UnloadTVResource() but shouldn't it be recursive?  Or is this already occuring
                // after refCount == 0?
                UnloadTVResource((IPageableTVNode)region);

                // NOTE: Modifications to nodes connected to the Scene (such as adding or removing child nodes) must
                // occur on the main thread
                Debug.WriteLine("Pager.RegionPageOutCompletedHandler() - BEGIN Paging out region '" + region.ID + "'.");
                region.Parent.RemoveChild(region);

                mLoadedRegions.Remove(region.ID);
                mLoadedLayers.Remove(region.ID);
                mRegionsUnloadingInProgress.Remove(region.ID);
                Debug.WriteLine("Pager.RegionPageOutCompletedHandler() - END Paging out region '" + region.ID + "'.");
            }

            return pageableObj;
        }

        #region STATIC PAGING - I might move the following code to ImportLib instead since it's not tied to any Scene instance
        private static Dictionary<Entity, List<IPageableTVNode>> _pageableNodes =
                                                                 new Dictionary<Entity, List<IPageableTVNode>>();
#if (SERVER)
        
        internal static void QueuePageableResource(Keystone.Elements.IPageableTVNode pageable, PostExecuteWorkItemCallback postExecute)
        {
            // Filter to only load IPageable of type Scripts. 
            // For Server we DO want to load DomainObjectScripts
            // but DO NOT want to load geometry and textures and audio
            if (pageable is Keystone.Elements.DomainObjectScript)
            {
                lock (_pageableNodes) // we dont want objects added while itterating
                {

                    // neither callback can be null so if the user's postExecute is null (meaning they dont want to respond to result) then
                    // we'll just call the local one here instead.
                    if (postExecute != null)
                        mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(LoadTVResource), pageable, postExecute);
                    else
                        mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(LoadTVResource), pageable, PageableResourceLoadCompletedHandler);
                }
            }
        }
#else
        // This is mostly used for Interior data saving, and when saving a .tvm from .obj
        public static void QueuePageableResourceSave(Node node, string fullPath, PostExecuteWorkItemCallback postExecute, bool recurse = true)
        {
            if (node is IPageableTVNode)
            {
                PageableSaveInfo info = new PageableSaveInfo();
                info.node = (IPageableTVNode)node;
                info.FullPath = fullPath;
               
                QueuePageableResourceSave(info, postExecute);
            }
        }

		public static void QueuePageableResourceLoad(Node node, PostExecuteWorkItemCallback postExecute, bool recurse = true)
		{
			if (!recurse)
			{
				if (node is IPageableTVNode)			
					QueuePageableResourceLoad ((IPageableTVNode)node, postExecute);
				
				return;
			}
			else if (node is IGroup)
			{
				IGroup g = (IGroup)node;
				if (g.ChildCount > 0)
				{
					for (int i =0; i < g.ChildCount; i++)
					{
                        // recurse.  NOTE: during recurse we always use the ResourcePath and as of yet we do not pass in an array[] of alterante string paths
                        QueuePageableResourceLoad(g.Children[i], postExecute, true);
					}
				}
			}
			else 
			{
				if (node is IPageableTVNode)
					QueuePageableResourceLoad ((IPageableTVNode)node, postExecute);
			}
				
			
		}
		
        // TODO: i think the following should be apart of ImportLib.
        // TODO: The IPageableTVNode that is being paged in, if it's removed from scene tree
        // and being disposed AND it's .PageStatus = PageableNodeStatus.Loading, it must 
        // send the abort to the pager before it finishes disposing.
        //
        // TODO: i think that fundamentally the QueuePageableResource and the IPabeableBranch pager
        // are fundamentally two different things.  
        // The latter does not need to be associated with a scene, but the former does.
        internal static void QueuePageableResourceLoad(Keystone.IO.IPageableTVNode pageable, PostExecuteWorkItemCallback postExecute)
        {
            lock (_pageableNodes) // we dont want objects added while itterating
            {
                if (postExecute != null)
                    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(LoadTVResource), pageable, postExecute);
                else
                    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(LoadTVResource), pageable, PageableResourceLoadCompletedHandler);
            }
        }

        internal static void QueuePageableResourceSave(PageableSaveInfo info, PostExecuteWorkItemCallback postExecute)
        {
            lock (_pageableNodes)
            {

                if (postExecute != null)
                    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(SaveTVResource), info, postExecute);
                else
                    mPagerThreadPoolGroup.QueueWorkItem(new WorkItemCallback(SaveTVResource), info, PageableResourceSaveCompletedHandler);
            }
        }
#endif

		
        protected static void PageableResourceMaintenance()
        {
            // remove any entries that have no children or who's childrens are all PagingStatus==PageState.Loaded
            lock (_pageableNodes) // we dont want objects added while itterating
            {

            }
        }

        
        
#region unloading

		
        internal static void UnloadTVResource (Node node, bool recurse = true)
        {
        	node.Enable = false;
        	
        	// if the node has resources that can be unloaded, do so
        	if (node is IPageableTVNode)
        		UnloadTVResource((object)node);
        	
        	// It's possible for a Node to be both a IGroup and IPageableTVnode that gets processed in line above.
        	// Check for children with resources that can be unloaded via pageableNode.UnloadTVResource() and unload them
        	if (recurse && node is IGroup)
        	{
        		IGroup g = (IGroup)node;
        		
        		Node[] children = g.Children;
        		if (children != null)
        		{
        			for (int i = 0; i < children.Length; i++)
		        		UnloadTVResource (children[i], recurse);
        		}
        	}
        }

        internal static object UnloadTVResource (object pageableObj)
        {
        	IPageableTVNode pageableNode = (IPageableTVNode)pageableObj;
        	try
            {
                // TODO: need some kind of timeout var to determine if 
                // we should abort loading and go to PageableNodeStatus.Error
                lock (pageableNode.SyncRoot)
                {
                    //Trace.WriteLine("Pager.LoadTVResource() - Attempting to unload '" + pageableNode.ResourcePath + "'" + pageableNode.GetType().Name);
                    pageableNode.PageStatus = PageableNodeStatus.Unloading;

                    pageableNode.UnloadTVResource();

                    if (pageableNode.PageStatus == PageableNodeStatus.Error)
                        Trace.WriteLine("Pager.UnloadTVResource() - ERROR - '" + pageableNode.ResourcePath + "' failed to unload. ");
                    else
                    {
                        //Trace.WriteLine("Pager.UnloadTVResource() - Node '" + obj.GetType().Name +
                        //                "' resource unloading completed.");
                        pageableNode.PageStatus = PageableNodeStatus.NotLoaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Pager.UnloadTVResource() - ERROR - '" + pageableNode.GetType().Name +
                                "' resource unloading FAILED. " + ex.Message);
                pageableNode.PageStatus = PageableNodeStatus.Error;
            }
            return pageableNode;
        }
        #endregion

        #region saving
        //public static void SaveTVResource(Node node, string fullPath)
        //{
        //    // if node has resources that can be paged in, load them
        //    if (node is IPageableTVNode)
        //        SaveTVResource((object)node);
        //}

        private static object SaveTVResource(object info)
        {

            IPageableTVNode pageableNode = ((PageableSaveInfo)info).node;
            string path = ((PageableSaveInfo)info).FullPath;
            if (string.IsNullOrEmpty(path))
                path = pageableNode.ResourcePath;

            if (mDisabled) return pageableNode;
            try
            {
                // TODO: need some kind of timeout var to determine if 
                // we should abort saving and go to PageableNodeStatus.Error
                lock (pageableNode.SyncRoot)
                {
                    if (pageableNode.PageStatus == PageableNodeStatus.Loaded)
                    {
                        pageableNode.SaveTVResource(path);
                        return pageableNode;
                    }

                    if (pageableNode.PageStatus == PageableNodeStatus.Error)
                        Debug.WriteLine("Pager.SaveTVResource() - ERROR: '" + path + "' FAILED to save. ");
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Pager.SaveTVResource() - ERROR: Node '" + pageableNode.GetType().Name + "' resource saving FAILED. " + ex.Message);
                pageableNode.PageStatus = PageableNodeStatus.Error;
            }
            return pageableNode;
        }

        protected static void PageableResourceSaveCompletedHandler(IWorkItemResult result)
        {
            PageableSaveInfo info = (PageableSaveInfo)result.State;
            IPageableTVNode node = (IPageableTVNode)info.node;
            string path = info.FullPath;
            if (string.IsNullOrEmpty(path))
                path = node.ResourcePath;

            System.Diagnostics.Debug.WriteLine("PagerBase.PageableResourceSaveCompletedHandler() - " + path + " saved.");
        }

        #endregion

        public delegate void PostInteriorLoadCallback(Interior interior);
        #region loading            
        public static void LoadTVResource (Node node, bool recurse = true)
        {
        	// if node has resources that can be paged in, load them
        	if (node is IPageableTVNode)
        		LoadTVResource((object)node);
        	
        	// check for children and attempt to page in their resources via pageableNode.LoadTVResource()
        	if (recurse && node is IGroup)
        	{
        		IGroup g = (IGroup)node;
        		
        		Node[] children = g.Children;
        		if (children != null)
        			for (int i = 0; i < children.Length; i++)
		        		LoadTVResource (children[i], recurse);
        	}
        }
                
        private static object LoadTVResource(object pageableObj)
        {
            

        	IPageableTVNode pageableNode = (IPageableTVNode)pageableObj;
            if (!(pageableObj is DomainObjects.DomainObject) && mDisabled) return pageableNode;

            // NOTE: we will always load Entity Scripts even if pager is Disabled.  This is necessary because
            //       Scripts contain the CustomProperties needed for actions such as Serializing 
            //  NOTE: Interiors must always be paged in so that Interior components can be placed in them and Registered to the celldb
            //        	if (mDisabled && pageableNode is Portals.Interior == false && pageableNode is DomainObjects.DomainObject == false) return pageableNode;
            try
            {	        	
                // TODO: need some kind of timeout var to determine if 
                // we should abort loading and go to PageableNodeStatus.Error
                lock (pageableNode.SyncRoot)
                {
                    // prevent attempted loading twice of a resource that has already load or begun loading in background
                    // in the following, if i check if (pageableNode.TVResourceIsLoaded || or the rest, then if it reports true even though its not Loading or Loaded, 
                    // real-time changes to an Entity's Material will not apply. - Jan.8.2019 - hopefully removing that check doesn't break anything. 
                    if (
                        pageableNode.PageStatus == PageableNodeStatus.Loading ||
                        pageableNode.PageStatus == PageableNodeStatus.Loaded)
                    {
                    	//Debug.WriteLine ("Pager.LoadTVResource() - Resource '" + pageableNode.ResourcePath + "' already loaded or is loading.");
                    	return pageableNode;
                    }
                    //Debug.WriteLine("Pager.LoadTVResource() - Attempting to load '" + pageableNode.ResourcePath + "'" + pageableNode.GetType().Name);
                    pageableNode.PageStatus = PageableNodeStatus.Loading;
                    pageableNode.LoadTVResource();

                    if (pageableNode.PageStatus == PageableNodeStatus.Error)
                        Debug.WriteLine("Pager.LoadTVResource() - ERROR: '" + pageableNode.ResourcePath + "' FAILED to load. ");
                    else
                    {
                        //Debug.WriteLine("Pager.LoadTVResource() - Node '" + obj.GetType().Name + "' resource loading completed.");
                        pageableNode.PageStatus = PageableNodeStatus.Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Pager.LoadTVResource() - ERROR: Node '" + pageableNode.GetType().Name + "' resource loading FAILED. " + ex.Message);
                pageableNode.PageStatus = PageableNodeStatus.Error;
            }
            return pageableNode;
        }



        protected static void PageableResourceLoadCompletedHandler(IWorkItemResult result)
        {
            IPageableTVNode node = (IPageableTVNode)result.State;
            node.PageStatus = PageableNodeStatus.Loaded;

            if (node is Container)
            {

            }
        }
#endregion

        protected void PagerThreadGroupIdle(IWorkItemsGroup group)
        {
            // Trace.WriteLine("Pager.PagerThreadGroupIdle() - Theadpool Group '" + group.Name + "' idle.");
        }
        #endregion

        #region IDisposable Members
        ~PagerBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            if (mLoadedRegions != null)
            {
                foreach (Entity region in mLoadedRegions.Values)
                {
                    if (region != null)
                    {
                       // TODO: I  should probably remove this Zone from
                       //       the Root?  where is that done?
                    	region.RemoveChildren();
                       
                        region.Dispose();
                    }
                }
            }
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}