using System;
using System.Collections.Generic;
using Amib.Threading;

namespace Keystone.IO
{
    public class ServerPager : PagerBase 
    {

        public ServerPager(Scene.Scene scene, SmartThreadPool threadPool, int concurrency, PageCompleteCallback emptyRegionPageCompleteHandler, PageCompleteCallback regionChildrenPageCompleteHandler)
            : base(scene, threadPool, concurrency, emptyRegionPageCompleteHandler, regionChildrenPageCompleteHandler)
        {

            string[] zoneNames = ((Portals.ZoneRoot)_scene.Root).GetAllZoneNames();
            string rootName = ((Portals.ZoneRoot)_scene.Root).ID;

            for (int i = 0; i < zoneNames.Length; i++)
            {
                scene.XMLDB.Read(typeof(Keystone.Portals.Region).Name, zoneNames[i], rootName, true, false, null, RegionPageInCompleteHandler);
            }
        }

        public override void Update()
        {
            // TODO: since Core.Pager is now not scene specific but used by any scene
            // by passing in a scene instance to this Update() we will use the passed in scene
            // and not any _scene or xmldb specified here.  We will use Scene.Database instead
            foreach (Keystone.Portals.Region r in mQueuedRegionsCompleted)
            {
                Traversers.SuperSetter super = new Traversers.SuperSetter(_scene.Root);
                try 
                {
                	r.Visible = false;
                	r.Enable = false;
                	super.Apply(r);
                }
                catch (Exception ex)
                {
                	System.Diagnostics.Debug.WriteLine ("ServerPager.Update() - ERROR - Cannot add child node to parent type. " + ex.Message);
                }
                mLoadedRegions[r.ID] = r;
            }
            mQueuedRegionsCompleted.Clear();

        }
    }
}
