//using System;
//using KeyCommon.DatabaseEntities;
//
//namespace Keystone.Scene
//{
//    public class ServerScene : Scene
//    {
//
//        public ServerScene(SceneManagerBase sceneManager, string name, Keystone.Simulation.ISimulation simulation, Keystone.Scene.Scene.EntityAdded entityAddedHandler, Keystone.Scene.Scene.EntityRemoved entityRemovedHandler)
//            : base(sceneManager, name, simulation, entityAddedHandler, entityRemovedHandler) { }
//
//
//        public override void Open(string filename)
//        {
//
//            // open scene db before creating pager so we can pass reference to the db
//            base.Open(filename);
//
//            // create pager prior to loading scene so that it's immediately available for 
//            // background paging
//            _pager = new IO.ServerPager(this, Core._Core.ThreadPool, 1);
//
//            base.Load();
//        }
//    }
//}
