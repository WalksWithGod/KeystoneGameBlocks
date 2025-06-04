//using System.Collections.Generic;
//using System.Diagnostics;
//using Keystone.Cameras;
//using Keystone.Collision;
//using Keystone.Entities;
//using Keystone.FX;
//using Keystone.Traversers;
//using Keystone.Types;
//using KeyCommon.DatabaseEntities;
//
//namespace Keystone.Scene
//{
//    // NOTE: Hrm.  I had this inheriting FXBase because when Scene elements move, I wanted the Scene to be notifiied in case it needs to
//    // move that object within the scene to a different sector or octree node.  Or is there another mechanism we want to use for that?
//    // Hrm... but if i do do something whereby Update() will handle this sort of thing when its sending those translation commands to the
//    // the scene nodes, then it brings into question the Notify() model in general for FXBase... i mean, the Update() here can
//    // check for subscriptions and notify the FX directly?  Meh. I dunno.  Need to contemplate this.  
//    // On the one hand, it's only Shadows that really utilized subscribers the most... and Water generally doesnt move at runtime except in edit mode.
//    // 
//    public class ClientScene : Scene
//    {   
//
//        internal ClientScene(string name) : base(name) { }
//
//        public ClientScene(SceneManagerBase sceneManager, string name, Keystone.Simulation.ISimulation simulation, EntityAdded entityAddedHandler, EntityRemoved entityRemovedHandler) 
//            : base(sceneManager, name, simulation, entityAddedHandler, entityRemovedHandler)
//        {
//            
//            
//            // TODO: So, in order to handle some GUI element's updates for like progress meters
//            // we'll need constants for some SCENE_EVENT_LOAD_PROGRESS or something that can be
//            // be used to "register" GUI element's appearance to those events...
//            // Consider my keybinder, there I use a script where the "constant" function to bind too
//            // is referred to via an alias that is "hooked" at runtime.  So I think a similar
//            // function can exist for GUI where we'll have some hard coded routines, but the GUI elements
//            // can "bind" to those routines when they are loaded.
//            // There are other types of handlers we can specify too that GUI elements can bind too.
//            // NetworkIn/Out events so we can update any labels.
//            // Paging/Load events so we can update any debug progress meters and such.
//
//            // So one question is, should the GUI map directly to those handlers in say Network.Events or Reader.Events, Loader.Events?
//            //      
//        }
//
//        public override void Open(string filename)
//        {
//            // open scene db before creating pager so we can pass reference to the db
//            base.Open(filename);
//            
//            // create pager prior to loading the scene so that it's available
//            // note: seperate pager per scene because a pager is about determine which
//            // zones to load.  However, resource paging is done by static methods and thus is
//            // independant of any particular scene.
//            _pager = new IO.ClientPager(this, Core._Core.ThreadPool, 1);
//            
//                           			
//            base.Load();
//        }
//
//
//        #region IDisposable Members
//        protected override void DisposeManagedResources()
//        {
//            base.DisposeManagedResources();
//        }
//        #endregion
//    }
//}