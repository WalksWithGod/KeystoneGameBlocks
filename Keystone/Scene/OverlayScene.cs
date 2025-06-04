//using System;
//using System.Collections.Generic;
//using Keystone.Portals;
//using Keystone.Resource;
//using Keystone.Elements;


//namespace Keystone.Scene
//{
//    // OBSOLETE: A Hud with a root element that we attach all hud items is better than 
//    //           trying to have multiple scenes running within a SINGLE rendering context. 
//    //           It is already perfectly ok to have seperate scenes for seperate rendering contexts.
//    // 
//    public class OverlayScene  : ClientScene 
//    {


//        ClientScene mPrimaryScene;
//        // TODO: create custom derived HUD implementation in nav workspace for
//        //       nav workspace that will render icons for planets and ships and draw orbital
//        //       indicators 
//        // TODO: culler.cull passes this scene but must be given a seperate
//        //       regionPVS to use for OverlayRegionPVS
//        //       TODO: Why not create our own RegionPVS during Cull of the Overlay!?
//        //
//        // TODO: test picking of HUD elements
//        //
//        public OverlayScene(string name, ClientScene primaryScene)
//            : base(primaryScene.SceneManager, name, null, null, null)
//        {
//            // TODO: what on earth is the need to have a primaryScene passed in at all?
//            //       seems completely irrelevant.
//            if (primaryScene == null) throw new ArgumentNullException();


//            mPrimaryScene = primaryScene;

//            // overlay scene does not load from a file and there is no paging.
//            // therefore we will create the initial root node and it's regionNode

//            // no simulation either
//            // entities that need to update may still have update functions
//            // written in Hud.cs, but the scene itself is not simulating physics
//            // or anything

//            _root = new Root(_name);

//            // must call this manually on root to get it's RegionNode created so that
//            // child entities can then have their scenenode's attached under it
//            //       _root.SetChangeFlags(Enums.ChangeStates.EntityAttached, Keystone.Enums.ChangeSource.Self);
//            NodeAttached(_root);

//            Repository.IncrementRef(_root);

//            _sceneLoaded = true;
//        }

//        public override void Update(double elapsedSeconds)
//        {
//            // do nothing // base.Update(elapsedSeconds);
//            // no paging is necessary for an overlay scene because
//            // it updates based on the state of the primary scene

//        }

//    }
//}
