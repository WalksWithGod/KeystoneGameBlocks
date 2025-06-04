//using System;
//using Keystone.Appearance;
//using Amib.Threading;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Portals;
//using Keystone.Resource;
//using Keystone.Types;
//using Keystone.Vehicles;
//using JigLibX.Physics;
//using JigLibX.Collision;
//using JigLibX.Physics;
//using System.Diagnostics;
//using Keystone.Enum;
//using Keystone.IO;
//using Keystone.Modeler;

//namespace Keystone.Commands
//{
//    // Command wise for creating vehicles there's two kinds,
//    // 1) a new vehicle that is being imported during design time (File\Import )
//    //      a) generating the deckplans from an exterior mesh
//    //          http://www.gamedev.net/community/forums/topic.asp?topic_id=353691
//    //      b) generating the exterior model form the array of deck meshes
//    // 2) an existing vehicle on file as someVehicle.xml or that is streamed across the wire
//    //      (Prefabs \ Vehicles or network)
//    public class NewVehicle : ImportGeometry  
//    {

//        public NewVehicle(string fullpath, string relativeModFolderPath, bool loadTextures, bool loadMaterials,  bool loadXFileAsActor)
//            : base(fullpath , relativeModFolderPath , loadTextures , loadMaterials ,  loadXFileAsActor )
//        {
//        }
//            // note: physics for our exterior objects is no good because we lose our precision since JigLibX uses floats
//            //Entity.PhysicsBody = new JigLibX.Physics.Body();
//            //Entity.PhysicsBody.Immovable = false;
//            //Entity.PhysicsBody.CollisionSkin = new CollisionSkin(Entity.PhysicsBody);

//            //float mass = 10000;
//            //Entity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box((float)_localPosition.x, (float)_localPosition.y, (float)_localPosition.z, (float)Model.BoundingBox.Width, (float)Model.BoundingBox.Height, (float)Model.BoundingBox.Depth), new MaterialProperties(0.1f, 0.5f, 0.5f));
//            //Entity.PhysicsBody.MoveTo((float)_localPosition.x, (float)_localPosition.y, (float)_localPosition.z);
//            //Entity.PhysicsBody.Initialize(mass);



//    }
//}
