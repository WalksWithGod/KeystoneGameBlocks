//using System;
//using System.Diagnostics;
//using Amib.Threading;
//using Keystone.Appearance;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Enum;
//using Keystone.IO;
//using Keystone.Types;
//using MTV3D65;

//namespace KeyCommon.Messages
//{
//    public enum PhysicsDemo
//    {
//        Wall,
//        Incoming,
//        LotsOfSpheres,
//        MultiPendulum,
//        Pendulum,
//        Catapult,
//        Plinko,
//        Cubes,
//        BounceAndFriction
//    }

//    public class ImportPhysicsDemo : ImportEntityBase
//    {
//        public bool LoadTextures = true;
//        public bool LoadMaterials = true;
//        private Entity _parent;
//        private Vector3d _localPosition;
//        private Mesh3d[] _resources;
//        private PhysicsDemo _demo;
//        private DefaultAppearance _app;
//        private bool _writeToLibrary;
//        private string _libraryFileName;
//        Random _random = new Random();

//        public ImportPhysicsDemo(PhysicsDemo demo, Entity parent, Vector3d localPosition, bool writeToLibrary, string libraryFileName,
//                                  PostExecuteWorkItemCallback completionCB)
//            : base(completionCB)
//        {
//            if (parent == null) throw new ArgumentNullException();
//            _parent = parent;
//            _localPosition = localPosition;
//            NodeType = typeof(Mesh3d);
//            EntityType = typeof(ModeledEntity);
//            _writeToLibrary = writeToLibrary;
//            _libraryFileName = libraryFileName;
//            _demo = demo;
//        }

//        public override object Execute()
//        {
//            // validate filename and reserve the library file name if necessary
//            _state = CommandState.ExecuteCompleted;

//            // these three lines i suspect should be handled automatically in the command.Execute() method
//            // and if it's an asychronous command, it should queue the work item
//            WorkItemInfo item = new WorkItemInfo();
//            item.PostExecuteWorkItemCallback = new PostExecuteWorkItemCallback(AsychronousJobCompletedHandler);
//            CommandProcessor.ThreadedWorkQueue.QueueWorkItem(item, new WorkItemCallback(Worker), this);

//            return null;
//        }

//        // TODO:
//        public override void UnExecute()
//        {
//            if (_state == CommandState.ExecuteCompleted)
//            {
//                //_parent.RemoveChild(_entity);

//                _state = CommandState.ExecuteCompleted;
//            }
//            //    // call to remove should handle derefercing the node and such
//            //    Core._CoreClient.SceneManager.CurrentScene.Remove(Model);

//            //    Core._CoreClient.Simulation.RemoveEntity(Entity);
//        }

//        private void AsychronousJobCompletedHandler(IWorkItemResult result)
//        {
//            //TODO: fix FileManager.SuspendWrite = true; // TODO: this method of on/off is lame... need something more robust?
//            float defaultMargin = 0.04f; // space.simulationSettings.defaultMargin;
//           // _resource.FrustumCullMode = FrustumCullMode.custom;
//            ModeledEntity newEntity;
//            Physics.Primitives.CollisionPrimitive primitve;

//            //switch (_demo)
//            //{
//            //    case PhysicsDemo.Wall :
                    
//            //        // the floor
//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[1]);

//            //        newEntity = new StaticEntity("floor", Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(0, -.5, 0);
                    
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.isPhysicallySimulated = false;
//            //     //   primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //        _parent.AddChild(newEntity);

//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[0]);

//            //        int width = 10;
//            //        int height = 10;
//            //        float blockWidth = 2;
//            //        float blockHeight = 1;
//            //        float blockLength = 1;
                    
//            //        float xSpacing = blockWidth + defaultMargin * 2;
//            //        float ySpacing = blockHeight + defaultMargin * 2;
//            //        for (int i = 0; i < width; i++)
//            //        {
//            //            for (int j = 0; j < height; j++)
//            //            {
//            //                newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //                newEntity.Enable = true;
//            //                double x = i * xSpacing + .5f * blockWidth * (j % 2) - width * xSpacing * .5f;
//            //                double y = blockHeight * .5f + defaultMargin * 2 + j * ySpacing;
//            //                double z = 0;

//            //                newEntity.Translation = new Vector3d(x, y, z);
                            
//            //                newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                newEntity.PhysicsBody.mass = 1;
//            //                newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //             //   primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //                _parent.AddChild(newEntity);
//            //            }
//            //        }
//            //        break;

//            //    #region Incoming
//            //    case PhysicsDemo.Incoming:
//            //        // the platform
//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[0]);
//            //        newEntity = new StaticEntity("floor", Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(0,0,0);
                    
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.isPhysicallySimulated = false;
//            //        //primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //        _parent.AddChild(newEntity);

//            //        //the smasher sphere
//            //        SimpleModel Model2 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//            //        Model2.AddChild(_app);
//            //        ((SimpleModel)Model2).AddChild(_resources[3]); // sphere
//            //        newEntity = new StaticEntity("sphere", Model2);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(0, 150, 0);
                    
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.mass = 20;
//            //        newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //        //primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //        _parent.AddChild(newEntity);

//            //        //Build the stack... 1st block type
//            //        SimpleModel Model3 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//            //        Model3.AddChild(_app);
//            //        ((SimpleModel)Model3).AddChild(_resources[2]);

//            //        // second block type
//            //        SimpleModel Model4 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.blue));
//            //        Model4.AddChild(_app);
//            //        ((SimpleModel)Model4).AddChild(_resources[1]);

//            //        for (int k = 1; k < 11; k++)
//            //        {
//            //            float y = (1.0f + defaultMargin * 2.0f) * k;
//            //            if (k % 2 == 1)
//            //            {
//            //                newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model4);
//            //                newEntity.Enable = true;
//            //                newEntity.Translation = new Vector3d(-3, y, 0);
//            //                newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                newEntity.PhysicsBody.mass = 1;
//            //                newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //                //primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //                _parent.AddChild(newEntity);
//            //                // TODO: above is failing on internal Initialize because the SceneNode from which world BoundingBox is grabbed
//            //                // isn't computed... this is problematic.  
//            //                // and when an entity is moved, the primitive also needs to be removed/added and re-inited at least
//            //                // with the bounding volume updates... but maybe the key is that when
//            //                // certain things like scale/bounding volume/position change, those will re-init what they need
//            //                // also when PhysicsEnabled flag changes... this is part of the IPhysicsEntity i think.
//            //                // well it seems the Space.Add() pretty much require a collision primitive be set at that point... 
//            //                // so i think our solution is that if an entity is not yet added to the Scene, then it can't be added to the simulation
//            //                // however we should still be able to set a primitive, and we only update parts of the primitive when it's add/remove
//            //                // in scene is done or moved to a different region.

//            //                newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model4);
//            //                newEntity.Enable = true;
//            //                newEntity.Translation = new Vector3d(3, y, 0);
//            //                newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                newEntity.PhysicsBody.mass = 1;
//            //                newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //                //primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //                _parent.AddChild(newEntity);
//            //            }
//            //            else
//            //            {
//            //                newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model3);
//            //                newEntity.Enable = true;
//            //                newEntity.Translation = new Vector3d(0, y, -3);
//            //                newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                newEntity.PhysicsBody.mass = 1;
//            //                newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //                //primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //                _parent.AddChild(newEntity);

//            //                newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model3);
//            //                newEntity.Enable = true;
//            //                newEntity.Translation = new Vector3d(0, y, 3);
//            //                newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                newEntity.PhysicsBody.mass = 1;
//            //                newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //               // primitve = new Physics.Primitives.BoxPrimitive(newEntity.PhysicsBody, newEntity.BoundingBox);
//            //                _parent.AddChild(newEntity);
//            //            }
//            //        }
//            //        break;
//            //    #endregion
//            //    #region lots of spheres
//            //    case PhysicsDemo.LotsOfSpheres :

//            //        // the platform
//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[0]);
//            //        newEntity = new StaticEntity("floor", Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(0, 0, 0);
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.isPhysicallySimulated = false;
//            //        _parent.AddChild(newEntity);

                    
//            //        // the spheres
//            //        SimpleModel Model5 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//            //        Model5.AddChild(_app);
//            //        ((SimpleModel)Model5).AddChild(_resources[1]); // sphere

//            //        int numColumns = 7;
//            //        int numRows = 7;
//            //        int numHigh = 7;
//            //        xSpacing = 2.09f;
//            //        ySpacing = 2.08f;
//            //        float zSpacing = 2.09f;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {
//            //                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model5);
//            //                    newEntity.Enable = true;
//            //                    float x = 0 + xSpacing * i - (numRows - 1) * xSpacing / 2f;
//            //                    float y = 1.58f + k * (ySpacing);
//            //                    float z = 2 + zSpacing * j - (numColumns - 1) * zSpacing / 2f;
//            //                    newEntity.Translation = new Vector3d(x, y, z);
//            //                    newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //                    newEntity.PhysicsBody.mass = 1;
//            //                    newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //                    _parent.AddChild(newEntity);
//            //                   // primitve = new Physics.Primitives.SpherePrimitive(newEntity.PhysicsBody);

//            //                }
//            //        break;
//            //    #endregion
//            //    case PhysicsDemo.Catapult :

//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[2]);

//            //        newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(0, -.5, 0);
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.isPhysicallySimulated = false;
//            //        _parent.AddChild(newEntity);

//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.blue));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[1]);

//            //        newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(-2.2, .6, 0);
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.mass = 1;
//            //        newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //        _parent.AddChild(newEntity);

//            //        Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//            //        _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//            //        _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//            //        Model.AddChild(_app);
//            //        ((SimpleModel)Model).AddChild(_resources[0]);

//            //        //newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //        //newEntity.Enable = true;
//            //        //newEntity.Translation = new Vector3d(0, 1.7, 0);
//            //        //newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        //newEntity.PhysicsBody.mass = 1;
//            //        //newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //        //_parent.AddChild(newEntity);

//            //        //newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //        //newEntity.Enable = true;
//            //        //newEntity.Translation = new Vector3d(0, 3.5, 0);
//            //        //newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        //newEntity.PhysicsBody.mass = 1;
//            //        //newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //        //_parent.AddChild(newEntity);

//            //        newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//            //        newEntity.Enable = true;
//            //        newEntity.Translation = new Vector3d(-3.5d, 10d, 0);
//            //        newEntity.PhysicsBody = new Physics.PhysicsBody(newEntity);
//            //        newEntity.PhysicsBody.mass = 10;
//            //        newEntity.PhysicsBody.isPhysicallySimulated = true;
//            //        _parent.AddChild(newEntity);
//            //        break;
//            //    default :
//            //        throw new Exception();

//            //}

//            //TODO: fix FileManager.SuspendWrite = false;      // TODO: this method of on/off is lame... need something more robust?
//            // TODO: allow the _completionCB.Invoke to handle the saving
//            // FileManager.WriteNewNode(_parent.Parent, true); // TODO: writes shouldnt be done automaticall when the item is added to the scene correct?

//            // TODO: however should a seperate Entity save directly to the library be done?
//            if (_writeToLibrary)
//            {

//            }

//            _completionCB.Invoke(result);
//            _state = CommandState.ExecuteCompleted;
//        }

//        protected object Worker(object obj)
//        {
//            string classname = NodeType.ToString();


//            try
//            {
//                // TODO: i think Import should always assume that the user does not want to duplicate an existing resource from the same path
//                //       and so a different name is chosen.  or what?
//                // create the entity
//                // TODO: create seperate command to instantiate the entity and add it to the Simulation
//                //      note: the main impetus for a seperate load model is so that we can immediately add the IEntity
//                //      to the scene and get Notification of that, so if the networking demands we spawn something, it doesnt have to wait.
//                //

//                // create the model
//                if (classname == typeof(Mesh3d).ToString())
//                {
//                    switch (_demo)
//                    {
//                        #region Wall
//                        case PhysicsDemo.Wall :
//                            _resources = new Mesh3d [2];
//                            _resources[0] = Mesh3d.CreateBox (2,1,1);
//                            _resources [1] = Mesh3d.CreateBox(50, 1, 50);
//                            break;
//                        #endregion

//                        case PhysicsDemo.Incoming :
//                            _resources = new Mesh3d [4];
//                            _resources[0] = Mesh3d.CreateBox(10, 1, 10);
//                            _resources[1] = Mesh3d.CreateBox (1,1,7);
//                            _resources [2] = Mesh3d.CreateBox(7, 1, 1);
//                            _resources [3] = Mesh3d.CreateSphere(3, 10, 10, false);
//                            break;
//                        case PhysicsDemo.LotsOfSpheres:
//                            _resources = new Mesh3d[2];
//                            _resources[0] = Mesh3d.CreateBox(70, 1, 70);
//                            _resources[1] = Mesh3d.CreateSphere(1, 10, 10, false);
//                            break;

//                        case PhysicsDemo.Catapult :
//                            _resources = new Mesh3d[3];
//                            _resources[0] = Mesh3d.CreateBox(1, 1, 1);
//                            _resources[1] = Mesh3d.CreateBox(5, .7f, 1);
//                            _resources[2] = Mesh3d.CreateBox(3, .5f, 2);
//                            break;

//                        default:
//                            throw new Exception ();
//                    }
//                }
//                else if (classname == typeof(Actor3d).ToString())
//                {
//                }
//                else if (classname == typeof(Terrain).ToString())
//                {
//                }
//                //else if (classname == typeof(Minimesh).ToString())
//                //{
//                //}
//                else if (classname == typeof(Billboard).ToString())
//                {
//                }
//                else
//                {
//                    // TODO: need a custom type so we can 
//                    throw new Exception(string.Format("Unsupported node type '{0}'", classname));
//                }
//            }
//            // TODO: we have to unroll anything we've done here if the command fails we want it to fail completely, not partially
//            catch (Exception ex)
//            {
//                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
//                Trace.WriteLine(string.Format("Importer.Instantiate() -- Error creating node '{0}'.", classname));
//            }

//            return null;
//        }
//    }


//}

//            //switch (sim)
//            //{
//            //    
//            //    #region Various Things Doing Stuff
//            //    case 3:

//            //        //Create a stack of spheres to sit atop the cone.
//            //        for (int k = 0; k < 15; k++)
//            //        {
//            //            toAddSphere = new Sphere(new Vector3(0, 2 * k + 14, 0f), .5f, 1f);
//            //            space.add(toAddSphere);
//            //        }
//            //        //Add the supporting, floaty cone.  Note the high linear damping.
//            //        toAddCone = new Cone(new Vector3(0, 12, 0), 2f, 2f, 100);
//            //        toAddCone.linearDamping = .9999f;
//            //        space.add(toAddCone);
//            //        toAddCylinder = new Cylinder(new Vector3(2, 70, 0), 4f, 1.2f, 100);
//            //        toAddCylinder.linearDamping = .73f;
//            //        toAddCylinder.angularDamping = .4f;
//            //        space.add(toAddCylinder);
//            //        space.add(new Box(new Vector3(0, -.5f, 0), 30, 1f, 30));
//            //        //Now for some random, circularly placed capsules to complete the scene.
//            //        for (double k = 0; k < Math.PI * 2; k += Math.PI / 6)
//            //        {
//            //            space.add(new Capsule(new Vector3((float)Math.Cos(k) * 7, 2, (float)Math.Sin(k) * 7), 2, 1, 20));
//            //        }
//            //        for (double k = 0; k < Math.PI * 2; k += Math.PI / 16)
//            //        {
//            //            space.add(new Capsule(new Vector3((float)Math.Cos(k) * 12, 1, (float)Math.Sin(k) * 12), 2, 1));
//            //        }
//            //        //Toss a sphere from the left to knock the spherestack over.
//            //        //Note that if the velocity of the sphere is increased, using the default setting of useContinuousDetectionAgainstKinematics = false,
//            //        //the collision can be missed.  It is set to false by default to improve the appearance of simulations; 
//            //        //infinite mass bodies stopping (cutting motion off to create a contact point) for less than infinite mass bodies can look a bit odd.
//            //        Sphere staticSphere = new Sphere(new Vector3(-240, 14, 0), 2);
//            //        staticSphere.linearVelocity = new Vector3(100, 0, 0);
//            //        space.add(staticSphere);

//            //        //Long poles benefit from having a more accurate model of angular momentum.  The following methods are slightly slower than the default nonconservative/symplectic euler methods.
//            //        space.simulationSettings.conserveAngularMomentum = true;
//            //        space.simulationSettings.useRK4AngularIntegration = true; //turning off RK4 while conservation is on will make objects tend to spin around the axis of least resistance in an odd way.
//            //        //Create some long poles.
//            //        for (int k = 0; k < 5; k++)
//            //        {
//            //            space.add(new Box(new Vector3(0, 6 + 2 * k, 7), 10, .2f, .2f, 20f));
//            //        }

//            //        camera.position = new Vector3(0, 6, 30);

//            //        break;
//            //    #endregion
//            //    #region Terrain
//            //    case 4:

//            //        //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
//            //        //Setup the heights of the terrain.
//            //        int xLength = 256;
//            //        int yLength = 256;

//            //        xSpacing = 8f;
//            //        ySpacing = 8f;
//            //        float[,] heights = new float[xLength, yLength];
//            //        float x, y;
//            //        for (int i = 0; i < xLength; i++)
//            //        {
//            //            for (int j = 0; j < yLength; j++)
//            //            {
//            //                x = i - xLength / 2;
//            //                y = j - yLength / 2;
//            //                //heights[i, j] = (float)(x * y / 1000f);
//            //                heights[i, j] = (float)(10 * (Math.Sin(x / 8) + Math.Sin(y / 8)));
//            //                //heights[i, j] = 3 * (float)Math.Sin(x * y / 100f);
//            //                //heights[i, j] = (x * x * x * y - y * y * y * x) / 1000f;

//            //            }
//            //        }
//            //        //Create the terrain.
//            //        Terrain terrain = new Terrain(new Vector3(-xLength * xSpacing / 2, 0, -yLength * ySpacing / 2));
//            //        terrain.setData(heights, QuadFormats.lowerLeftUpperRight, xSpacing, ySpacing);
//            //        space.add(terrain);
//            //        terrain.useFaceNormalWithinAngle = (float)Math.PI / 2; //This terrain shape is very flat, so the triangle normals can be safely used in every case.
//            //        //Generally, it is fine to use the triangle normals, but if the terrain has extremely sharp cliffs and features you may notice
//            //        //some 'stickiness' or other more severe problems.  Triangles can be set to use the normal contact normals always by setting the terrain.tryToUseTriangleNormals to false.
//            //        //The benefit of using the triangles' normals is smoother behavior for sliding objects at the boundary between triangles.

//            //        for (int i = 0; i < 3; i++)
//            //        {
//            //            for (int j = 0; j < 6; j++)
//            //            {
//            //                for (int k = 0; k < 3; k++)
//            //                {
//            //                    space.add(new Box(new Vector3(0 + i * 4, 100 + j * 7, 0 + k * 4), 2 + i * j * k, 2 + i * j * k, 2 + i * j * k, 4 + 20 * i * j * k));
//            //                }
//            //            }
//            //        }

//            //        DisplayTerrain dispTerrain = new DisplayTerrain(terrain, graphics);
//            //        entityRenderer.addDisplayObject(dispTerrain);

//            //        camera.position = new Vector3(0, 30, 20);
//            //        break;
//            //    #endregion

//            //    #region Multi-pendulum
//            //    case 6:
//            //        //Create the blocks in the pendulum.
//            //        Box link1 = new Box(new Vector3(3, 5, 0), 1.1f, 1.1f, 1.1f, 2);
//            //        Box link2 = new Box(new Vector3(6, 5, 0), 1.1f, 1.1f, 1.1f, 2);
//            //        space.add(link1);
//            //        space.add(link2);
//            //        Box ground = new Box(new Vector3(3, -3, 0), 20, 1, 10);
//            //        space.add(ground);
//            //        //Connect them together.
//            //        space.add(new BallSocketJoint(link1, link2, (link1.centerPosition + link2.centerPosition) / 2, 0f, 1f));
//            //        space.add(new BallSocketJoint(link1, ground, new Vector3(0, 5, 0), 0f, 1f));
//            //        //Create a target stack.
//            //        for (int k = 0; k < 4; k++)
//            //        {
//            //            toAddBox = new Box(new Vector3(-2f, -1f + 2 * k, 0), 1.5f, 1.5f, 1.5f, 1);
//            //            space.add(toAddBox);
//            //        }
//            //        camera.position = new Vector3(0, 2, 20);
//            //        break;
//            //    #endregion

//            //    #region Plinko
//            //    case 8:
//            //        //Drops a bunch of spheres down a plinko-style machine.


//            //        space.simulationSettings.gravity = new Vector3(0, -4, 10);
//            //        //Create the container.
//            //        space.add(new Box(new Vector3(0, -.5f, 0), 21f, 1f, 50));
//            //        space.add(new Box(new Vector3(-10, .5f, 0), 1f, 1f, 50));
//            //        space.add(new Box(new Vector3(10, .5f, 0), 1f, 1f, 50));
//            //        space.add(new Box(new Vector3(0, .5f, -24.5f), 19f, 1f, 1));
//            //        space.add(new Box(new Vector3(0, .5f, 24.5f), 19f, 1f, 1));

//            //        //Create some obstacles.
//            //        for (int i = 0; i < 4; i++)
//            //        {
//            //            for (int j = 0; j < 8; j++)
//            //            {
//            //                space.add(new Cylinder(new Vector3(-7.5f + i * 5, .5f, -20 + j * 6), 1f, .8f));
//            //            }
//            //        }
//            //        //Create some more obstacles, offset a bit.
//            //        for (int i = 0; i < 3; i++)
//            //        {
//            //            for (int j = 0; j < 7; j++)
//            //            {
//            //                space.add(new Cylinder(new Vector3(-5f + i * 5, .5f, -17 + j * 6), 1f, .8f));
//            //            }
//            //        }
//            //        //Create the spheres.
//            //        for (int i = 0; i < 15; i++)
//            //        {
//            //            toAddSphere = new Sphere(new Vector3(-7f + i * 1, .5f, -22), .3f, .5f);
//            //            space.add(toAddSphere);
//            //            toAddSphere = new Sphere(new Vector3(-7f + i * 1, .5f, -20), .3f, .5f);
//            //            space.add(toAddSphere);

//            //        }
//            //        camera.position = new Vector3(0, 25, 25);
//            //        camera.pitch = (float)Math.PI / -4;
//            //        break;
//            //    #endregion
//            //    #region Force Tracking/Lifespan
//            //    case 9:
//            //        //Create a little stack.
//            //        for (int k = 0; k < 5; k++)
//            //        {
//            //            toAddBox = new Box(new Vector3(0 + 0f * k, 2f + k * 1.5f, 0f + 0f * k), 1f, 1f, 1f, 1);
//            //            space.add(toAddBox);
//            //        }
//            //        //Ram the stack with a rocket-cone!
//            //        toAddCone = new Cone(new Vector3(5, 3f, 0), 2, .5f, 4);
//            //        Force force = new Force(toAddCone.centerPosition + new Vector3(-.06f, 0, 0), new Vector3(-0, 9.81f * toAddCone.mass, 0), 3);
//            //        force.isTrackingTarget = true;
//            //        toAddCone.applyForce(force);
//            //        space.add(toAddCone);
//            //        space.add(new Box(new Vector3(0, -.5f, 0), 10f, 1f, 10));
//            //        camera.position = new Vector3(0, 2, 20);
//            //        break;
//            //    #endregion
//            //    #region Bowl o' Cubes
//            //    case 10:
//            //        //Dump a bunch of boxes in a bowl.  Nothing particularly amazing here, just lots of boxes in a big bowl.

//            //        //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
//            //        //Setup the heights of the terrain.
//            //        xLength = 128;
//            //        yLength = 128;

//            //        xSpacing = 2f;
//            //        ySpacing = 2f;
//            //        heights = new float[xLength, yLength];
//            //        for (int i = 0; i < xLength; i++)
//            //        {
//            //            for (int j = 0; j < yLength; j++)
//            //            {
//            //                x = i - xLength / 2;
//            //                y = j - yLength / 2;
//            //                heights[i, j] = (float)Math.Pow(1.2 * Math.Sqrt(x * x + y * y), 2);
//            //                //heights[i, j] = -1f / (x * x + y * y);
//            //                //heights[i, j] = (float)(x * y / 100f);
//            //                //heights[i, j] = (float)(10 * (Math.Sin(x / 8f) + Math.Sin(y / 8f)));
//            //                //heights[i, j] = 3 * (float)Math.Sin(x * y / 100f);
//            //                //heights[i, j] = (x * x * x * y - y * y * y * x) / 1000f;

//            //            }
//            //        }
//            //        //Create the terrain.
//            //        terrain = new Terrain(new Vector3(-xLength * xSpacing / 2, 0, -yLength * ySpacing / 2));
//            //        terrain.setData(heights, QuadFormats.lowerLeftUpperRight, xSpacing, ySpacing);
//            //        space.add(terrain);

//            //        for (int i = 0; i < 5; i++)
//            //        {
//            //            for (int j = 0; j < 5; j++)
//            //            {
//            //                for (int k = 0; k < 5; k++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(-5 + i * 2, 50 + j * 4, -5 + k * 2), 1, 1, 1, 15);
//            //                    toAddBox.linearDamping = .5f;
//            //                    space.add(toAddBox);
//            //                }
//            //            }
//            //        }


//            //        dispTerrain = new DisplayTerrain(terrain, graphics);
//            //        entityRenderer.addDisplayObject(dispTerrain);
//            //        camera.position = new Vector3(5, 50, 5);
//            //        camera.pitch = (float)Math.PI / -2.2f;
//            //        break;
//            //    #endregion
//            //    #region bounciness/friction
//            //    case 11:
//            //        //The combine method changes how the properties of each entity in a collision are factored together.
//            //        //Change them around a bit to alter the simulation.
//            //        space.simulationSettings.frictionCombineMethod = PropertyCombineMethod.min; //Defaults to average
//            //        space.simulationSettings.bouncinessCombineMethod = PropertyCombineMethod.max;
//            //        //Create the ground
//            //        toAddBox = new Box(new Vector3(0, -.5f, 0), 20f, 1f, 20f);
//            //        space.add(toAddBox);
//            //        //Bouncy balls 
//            //        toAddSphere = new Sphere(new Vector3(-8, 10, 0), 1, 1);
//            //        toAddSphere.bounciness = 1;
//            //        space.add(toAddSphere);
//            //        toAddSphere = new Sphere(new Vector3(-5, 10, 0), 1, 1);
//            //        toAddSphere.bounciness = .5f;
//            //        space.add(toAddSphere);
//            //        toAddSphere = new Sphere(new Vector3(-2, 10, 0), 1, 1);
//            //        toAddSphere.bounciness = .25f;
//            //        space.add(toAddSphere);
//            //        //Slide-y boxes
//            //        toAddBox = new Box(new Vector3(9, 1, 9), 1, 1, 1, 1);
//            //        toAddBox.dynamicFriction = 0;
//            //        toAddBox.linearVelocity = new Vector3(-5, 0, 0);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(9, 1, 5), 1, 1, 1, 1);
//            //        toAddBox.dynamicFriction = .05f;
//            //        toAddBox.linearVelocity = new Vector3(-5, 0, 0);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(9, 1, -5), 1, 1, 1, 1);
//            //        toAddBox.dynamicFriction = .2f;
//            //        toAddBox.linearVelocity = new Vector3(-5, 0, 0);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(9, 1, -9), 1, 1, 1, 1);
//            //        toAddBox.dynamicFriction = .5f;
//            //        toAddBox.linearVelocity = new Vector3(-5, 0, 0);
//            //        space.add(toAddBox);
//            //        camera.position = new Vector3(0, 2, 30);
//            //        break;
//            //    #endregion
//            //    #region Colosseum
//            //    case 12:


//            //        double angle;
//            //        int numBoxesPerRing = 6;
//            //        blockWidth = 2;
//            //        blockHeight = 2;
//            //        blockLength = 10;
//            //        float radius = 25;
//            //        ySpacing = blockHeight + space.simulationSettings.defaultMargin * 2;

//            //        space.add(new Box(new Vector3(0, -(blockHeight + space.simulationSettings.defaultMargin * 2), 0), 60, 2f, 60));
//            //        for (int i = 0; i < 10; i++)
//            //        {
//            //            for (double k = 0; k < Math.PI * 2; k += Math.PI / numBoxesPerRing)
//            //            {
//            //                if (i % 2 == 0)
//            //                {
//            //                    angle = k;
//            //                    toAddBox = new Box(new Vector3((float)Math.Cos(Math.PI + angle) * radius, i * ySpacing, (float)Math.Sin(angle) * radius), blockWidth, blockHeight, blockLength, 20);
//            //                    toAddBox.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Up, (float)angle);
//            //                    space.add(toAddBox);
//            //                }
//            //                else
//            //                {
//            //                    angle = k + Math.PI / (numBoxesPerRing * 2);
//            //                    toAddBox = new Box(new Vector3((float)Math.Cos(Math.PI + angle) * radius, i * ySpacing, (float)Math.Sin(angle) * radius), blockWidth, blockHeight, blockLength, 20);
//            //                    toAddBox.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Up, (float)angle);
//            //                    space.add(toAddBox);
//            //                }
//            //            }
//            //        }
//            //        camera.position = new Vector3(0, 2, 2);
//            //        break;
//            //    #endregion
//            //    #region Collision Filtering
//            //    case 13:
//            //        //Collision filters are represented by bit values in a 64-bit int.  Could also use the nonCollidableEntities field to establish pairwise intangibility.
//            //        toAddBox = new Box(new Vector3(0, -.5f, 0), 50, 1, 50);
//            //        space.add(toAddBox);
//            //        //Set up two stacks which go through each other
//            //        for (int k = 0; k < 10; k++)
//            //        {
//            //            toAddBox = new Box(new Vector3(-4 + .1f * k, .5f + space.simulationSettings.defaultMargin + k * (1 + space.simulationSettings.defaultMargin * 2), 0), 1f, 1f, 1f, 10);
//            //            toAddBox.collisionFilter = 1;
//            //            space.add(toAddBox);
//            //            toAddBox = new Box(new Vector3(4 - .1f * k, .5f + space.simulationSettings.defaultMargin + k * (1 + space.simulationSettings.defaultMargin * 2), 0), 1f, 1f, 1f, 10);
//            //            toAddBox.collisionFilter = 2;
//            //            space.add(toAddBox);
//            //        }
//            //        //Add another two boxes which ignore each other using the nonCollidableEntities method, but still collide with the stacks.
//            //        toAddBox = new Box(new Vector3(1, 3, 0), 1f, 4f, 2f, 10);
//            //        Box toAddBox2 = new Box(new Vector3(-1, 3, 0), 1f, 4f, 2f, 15);
//            //        toAddBox.nonCollidableEntities.Add(toAddBox2);
//            //        space.add(toAddBox);
//            //        space.add(toAddBox2);
//            //        camera.position = new Vector3(0, 6, 20);
//            //        break;
//            //    #endregion
//            //    #region Newton's Cradle
//            //    case 14:
//            //        //Create the block which the spheres hang on.
//            //        toAddBox = new Box(new Vector3(0, 5f, 0), 6f, 1f, 3f);
//            //        space.add(toAddBox);

//            //        //The rest of the objects should not be arbitrarily clamped.
//            //        space.simulationSettings.linearVelocityClamping = 0;
//            //        space.simulationSettings.angularVelocityClamping = 0;
//            //        float spacingBetweenBalls = .1f;
//            //        //Create the spheres
//            //        for (int k = 0; k < 6; k++)
//            //        {
//            //            if (k == 5)
//            //                toAddSphere = new Sphere(new Vector3(-3 + k * (1 + spacingBetweenBalls + space.simulationSettings.defaultMargin * 2) + 5f, 5, 0), .49f, 1);
//            //            else
//            //                toAddSphere = new Sphere(new Vector3(-3 + k * (1 + spacingBetweenBalls + space.simulationSettings.defaultMargin * 2), 0, 0), .49f, 1);
//            //            toAddSphere.bounciness = 1f;
//            //            toAddSphere.angularDamping = 0; //Damping the rotation can slow down the balls.
//            //            space.add(toAddSphere);
//            //            space.simulationSettings.penetrationRecoveryStiffness = 0;
//            //            space.add(new BallSocketJoint(toAddBox, toAddSphere, new Vector3(-3 + k * (1 + spacingBetweenBalls + space.simulationSettings.defaultMargin * 2), 5f, 1), .01f, .1f));
//            //            space.add(new BallSocketJoint(toAddBox, toAddSphere, new Vector3(-3 + k * (1 + spacingBetweenBalls + space.simulationSettings.defaultMargin * 2), 5f, -1), .01f, .1f));

//            //        }
//            //        camera.position = new Vector3(0, 2, 20);
//            //        break;
//            //    #endregion
//            //    #region Bowling
//            //    case 15:
//            //        //Create the alley
//            //        space.add(new Box(new Vector3(0, -1.5f, 0), 5f, 1f, 40f));
//            //        //How about a game of tenpin?
//            //        for (int k = 0; k < 4; k++)
//            //        {
//            //            toAddCylinder = new Cylinder(new Vector3(-1.5f + k * 1, .5f, -19), 1.4f, .3f, 1);
//            //            space.add(toAddCylinder);
//            //        }
//            //        for (int k = 0; k < 3; k++)
//            //        {
//            //            toAddCylinder = new Cylinder(new Vector3(-1f + k * 1, .5f, -17.5f), 1.4f, .3f, 1);
//            //            space.add(toAddCylinder);
//            //        }
//            //        for (int k = 0; k < 2; k++)
//            //        {
//            //            toAddCylinder = new Cylinder(new Vector3(-.5f + k * 1, .5f, -16), 1.4f, .3f, 1);
//            //            space.add(toAddCylinder);
//            //        }
//            //        toAddCylinder = new Cylinder(new Vector3(0, .5f, -14.5f), 1.4f, .3f, 1);
//            //        space.add(toAddCylinder);

//            //        //Uncomment if you don't want to knock the pins down yourself!
//            //        //Sphere bowlingBall = new Sphere(new Vector3(0, .5f, 19), .5f, 20);
//            //        //bowlingBall.linearMomentum = new Vector3(0, 0, -400);
//            //        //space.add(bowlingBall);
//            //        camera.position = new Vector3(0, 2, 20);
//            //        break;
//            //    #endregion
//            //    #region Pyramid
//            //    case 16:
//            //        float boxSize = 2f;
//            //        int boxCount = 20;
//            //        space.add(new Box(new Vector3(0, -.5f, 0), boxCount * (boxSize + space.simulationSettings.defaultMargin * 2) + 20, 1, Math.Min(50, boxCount * (boxSize + space.simulationSettings.defaultMargin * 2))));
//            //        for (int i = 0; i < boxCount; i++)
//            //        {
//            //            for (int j = 0; j < boxCount - i; j++)
//            //            {
//            //                space.add(new Box(new Vector3(-((boxCount - 1) * space.simulationSettings.defaultMargin / 2 + boxCount * boxSize / 2) + (boxSize / 2 + space.simulationSettings.defaultMargin) * i + j * (boxSize + space.simulationSettings.defaultMargin * 2),
//            //                                              ((boxSize / 2) + space.simulationSettings.defaultMargin * 1) + i * (boxSize + space.simulationSettings.defaultMargin * 2),
//            //                                              0),
//            //                                              boxSize, boxSize, boxSize, 20));
//            //            }
//            //        }
//            //        //Down here are the 'destructors' used to blow up the pyramid.  One set is physically simulated, the other kinematic.
//            //        /*
//            //        Sphere pow = new Sphere(new Vector3(-25, 5, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        pow = new Sphere(new Vector3(-15, 10, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        pow = new Sphere(new Vector3(-5, 15, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        pow = new Sphere(new Vector3(5, 15, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        pow = new Sphere(new Vector3(15, 10, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        pow = new Sphere(new Vector3(25, 5, 70), 2, 10);
//            //        pow.linearMomentum = new Vector3(0, 100, -1000);
//            //        space.add(pow);
//            //        */
//            //        /*
//            //        staticSphere = new Sphere(new Vector3(-40, 40, 120), 4);
//            //        staticSphere.linearVelocity = new Vector3(0, 0, -70);
//            //        space.add(staticSphere);
//            //        staticSphere = new Sphere(new Vector3(-20, 40, 120), 9);
//            //        staticSphere.linearVelocity = new Vector3(0, 0, -85);
//            //        space.add(staticSphere);
//            //        staticSphere = new Sphere(new Vector3(0, 40, 120), 14);
//            //        staticSphere.linearVelocity = new Vector3(0, 0, -100);
//            //        space.add(staticSphere);
//            //        staticSphere = new Sphere(new Vector3(20, 40, 120), 9);
//            //        staticSphere.linearVelocity = new Vector3(0, 0, -85);
//            //        space.add(staticSphere);
//            //        staticSphere = new Sphere(new Vector3(40, 40, 120), 4);
//            //        staticSphere.linearVelocity = new Vector3(0, 0, -70);
//            //        space.add(staticSphere);
//            //        */

//            //        camera.position = new Vector3(-boxCount * boxSize, 2, boxCount * boxSize);
//            //        camera.yaw = (float)Math.PI / -4f;
//            //        camera.pitch = (float)Math.PI / 9f;
//            //        break;
//            //    #endregion
//            //    #region Compound Bodies
//            //    case 17:
//            //        //Build the first body
//            //        CompoundBody cb1 = new CompoundBody();
//            //        Sphere a = new Sphere(new Vector3(0, 1, 0), .5f, 15);
//            //        Cone b = new Cone(new Vector3(1, 1, 0), 2f, .5f, 15);
//            //        Sphere c = new Sphere(new Vector3(-1, 1, 0), .5f, 15);
//            //        cb1.addBody(a);
//            //        cb1.addBody(b);
//            //        cb1.addBody(c);


//            //        //Build the second body
//            //        CompoundBody cb2 = new CompoundBody();
//            //        Box d = new Box(new Vector3(0, 3, 0), 1, 1, 1, 2);
//            //        Box f = new Box(new Vector3(1, 3.5f, 0), 1, 1, 1, 2);
//            //        cb2.addBody(d);
//            //        cb2.addBody(f);
                    
//            //        //Build the third Braum's-fry style body
//            //        CompoundBody cb3 = new CompoundBody();
//            //        for (int k = 0; k < 7; k++)
//            //        {
//            //            cb3.addBody(new Box(new Vector3(-4 + k * .7f, 2 + .7f * k, 2 + k * .2f), 1, 1, 1, 15));
//            //        }

//            //        //Add them all to the space
//            //        space.add(cb3);
//            //        space.add(cb2);
//            //        space.add(cb1);

//            //        space.add(new Box(new Vector3(0, -.5f, 0), 10, 1, 10));
//            //        camera.position = new Vector3(0, 3, 15);
//            //        break;
//            //    #endregion
//            //    #region Spaceship
//            //    case 18:
//            //        //Build the ship
//            //        Cylinder shipFuselage = new Cylinder(new Vector3(0, 5, 0), 3, .7f, 4);
//            //        Cone shipNose = new Cone(new Vector3(0, 7.00f, 0), 2, .7f, 2);
//            //        Box shipWing = new Box(new Vector3(0, 5, 0), 5f, 2, .2f, 3);
//            //        Cone shipThrusters = new Cone(new Vector3(0, 3.25f, 0), 1, .5f, 1);
//            //        CompoundBody ship = new CompoundBody();
//            //        ship.addBody(shipFuselage);
//            //        ship.addBody(shipNose);
//            //        ship.addBody(shipWing);
//            //        ship.addBody(shipThrusters);

//            //        //Setup the launch pad and ramp
//            //        toAddBox = new Box(new Vector3(10, 4, 0), 26, 1f, 6);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(32, 7.8f, 0), 20, 1, 6);
//            //        toAddBox.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Forward, -(float)Math.PI / 8);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(32, 8.8f, -3.5f), 20, 1, 1);
//            //        toAddBox.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Forward, -(float)Math.PI / 8);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(32, 8.8f, 3.5f), 20, 1, 1);
//            //        toAddBox.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Forward, -(float)Math.PI / 8);
//            //        space.add(toAddBox);
//            //        toAddBox = new Box(new Vector3(-2.75f, 5.5f, 0), .5f, 2f, 3);
//            //        space.add(toAddBox);

//            //        //Blast-off!
//            //        ship.angularDamping = .99f; //Helps keep the rocket on track for a little while longer :D
//            //        Force thruster = new Force(new Vector3(0, 5, 0), new Vector3(0, 300, 0));
//            //        thruster.isTrackingTarget = true;
//            //        ship.applyForce(thruster);
//            //        ship.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 2) * Quaternion.CreateFromAxisAngle(Vector3.Forward, (float)Math.PI / 2);
//            //        space.add(ship);

//            //        camera.position = new Vector3(-14, 12, 25);
//            //        camera.yaw = (float)Math.PI / -4;
//            //        break;
//            //    #endregion
//            //    #region Jenga
//            //    case 19:
//            //        space.remove(kapow);
//            //        //Have to shrink the ball a little to make it fit between jenga tower blocks.
//            //        kapow = new Sphere(new Vector3(-11000, 0, 0), .4f, 20);
//            //        kapow.collisionMargin = .2f;
//            //        space.add(kapow);
//            //        //space.activateDynamicIterationCount(30, 20, 30);
//            //        int numBlocksTall = 18; //How many 'stories' tall.
//            //        blockWidth = 4; //Total width/length of the tower.
//            //        float heightPerBlock = 1.333f;
//            //        for (int i = 0; i < numBlocksTall; i++)
//            //        {
//            //            if (i % 2 == 0)
//            //            {
//            //                for (int j = 0; j < 3; j++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(j * (blockWidth / 3 + space.simulationSettings.defaultMargin * 2) - blockWidth / 3f - space.simulationSettings.defaultMargin * 2, heightPerBlock / 2 + space.simulationSettings.defaultMargin * 2 + i * (heightPerBlock + space.simulationSettings.defaultMargin * 2), 0), blockWidth / 3, heightPerBlock, blockWidth, 2 * numBlocksTall + 1 - 2 * i);
//            //                    space.add(toAddBox);
//            //                }
//            //            }
//            //            else
//            //            {
//            //                for (int j = 0; j < 3; j++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(0, heightPerBlock / 2 + space.simulationSettings.defaultMargin * 2 + (i) * (heightPerBlock + space.simulationSettings.defaultMargin * 2), j * (blockWidth / 3 + space.simulationSettings.defaultMargin * 2) - blockWidth / 3f - space.simulationSettings.defaultMargin * 2), blockWidth, heightPerBlock, blockWidth / 3, 2 * numBlocksTall + 1 - 2 * i);
//            //                    space.add(toAddBox);
//            //                }
//            //            }
//            //        }
//            //        space.add(new Box(new Vector3(0, -.5f, 0), 70, 1, 70));
//            //        camera.position = new Vector3(0, 8, 32);
//            //        break;
//            //    #endregion
//            //    #region Rest State Stress Test
//            //    case 20:
//            //        //Create a bunch of blocks.
//            //        int zOffset = 5;
//            //        numRows = 1;
//            //        numColumns = 40;
//            //        for (int i = 0; i < numRows; i++)
//            //        {
//            //            for (int j = 0; j < numColumns; j++)
//            //            {
//            //                for (int k = 1; k <= 7; k++)
//            //                {
//            //                    if (k % 2 == 1)
//            //                    {
//            //                        toAddBox = new Box(new Vector3(j * 10 + -3, j * 10 + 3 * k, i * 10 + zOffset), 1, 1, 7, 20);
//            //                        toAddBox.linearDamping = .9f;
//            //                        toAddBox.angularDamping = .9f;
//            //                        space.add(toAddBox);
//            //                        toAddBox = new Box(new Vector3(j * 10 + 3, j * 10 + 3 * k, i * 10 + zOffset), 1, 1, 7, 20);
//            //                        toAddBox.linearDamping = .9f;
//            //                        toAddBox.angularDamping = .9f;
//            //                        space.add(toAddBox);
//            //                    }
//            //                    else
//            //                    {
//            //                        toAddBox = new Box(new Vector3(j * 10 + 0, j * 10 + 3 * k, i * 10 + zOffset - 3), 7, 1, 1, 20);
//            //                        toAddBox.linearDamping = .9f;
//            //                        toAddBox.angularDamping = .9f;
//            //                        space.add(toAddBox);
//            //                        toAddBox = new Box(new Vector3(j * 10 + 0, j * 10 + 3 * k, i * 10 + zOffset + 3), 7, 1, 1, 20);
//            //                        toAddBox.linearDamping = .9f;
//            //                        toAddBox.angularDamping = .9f;
//            //                        space.add(toAddBox);
//            //                    }
//            //                }
//            //                space.add(new Box(new Vector3(10 * j, -.5f, i * 10 + zOffset), 10, 1f, 10));
//            //            }
//            //        }

//            //        camera.position = new Vector3(-30, 5, 25);
//            //        camera.yaw = (float)Math.PI / -3;
//            //        camera.pitch = -(float)Math.PI / -12;
//            //        break;
//            //    #endregion
//            //    #region Bridge
//            //    case 21:
//            //        //Form a long chain of cubes, connected by ball socket joints.

//            //        float baseHeight = 85;
//            //        float linkSeparation = 2f;
//            //        int numLinks = 200;
//            //        float xOffset = -(numLinks + 1) * linkSeparation / 2;
//            //        Box link;
//            //        Box previousLink = null;
//            //        ground = new Box(new Vector3(0, -3, 0), 10, 10, 10);
//            //        space.add(ground);
//            //        for (int k = 0; k < numLinks; k++)
//            //        {
//            //            link = new Box(new Vector3(xOffset + linkSeparation * (k + 1), baseHeight, 0), 1.1f, 1.1f, 1.1f, 1);
//            //            link.linearDamping = .6f;
//            //            space.add(link);
//            //            if (k == 0)
//            //                space.add(new BallSocketJoint(link, ground, new Vector3(xOffset, baseHeight, 0), .05f, .2f));
//            //            else if (k == numLinks - 1)
//            //            {
//            //                space.add(new BallSocketJoint(link, previousLink, (link.centerPosition + previousLink.centerPosition) / 2, .05f, .2f));
//            //                space.add(new BallSocketJoint(link, ground, new Vector3(xOffset + (numLinks + 1) * linkSeparation, baseHeight, 0), .05f, .2f));
//            //            }
//            //            else
//            //                space.add(new BallSocketJoint(link, previousLink, (link.centerPosition + previousLink.centerPosition) / 2, .05f, .2f));
//            //            previousLink = link;
//            //        }


//            //        camera.position = new Vector3(-180, 70, 300);
//            //        camera.yaw = MathHelper.ToRadians(-24);
//            //        camera.pitch = MathHelper.ToRadians(-5);
//            //        break;
//            //    #endregion
//            //    #region More Constraints
//            //    case 22:

//            //        //Simulating a hinge (1DOF) between objects using ball socket joints (could use Distance constraints as well):
//            //        ground = new Box(new Vector3(0, -3, 0), 40, 1, 40);
//            //        space.add(ground);
//            //        Box hingePart1 = new Box(new Vector3(-2, 10, 5), 1, 1, 1, 10);
//            //        Box hingePart2 = new Box(new Vector3(2, 10, 5), 1, 1, 1, 10);
//            //        space.add(hingePart1);
//            //        space.add(hingePart2);
//            //        space.add(new BallSocketJoint(hingePart1, hingePart2, new Vector3(0, 10, 4), 0, .2f));
//            //        space.add(new BallSocketJoint(hingePart1, hingePart2, new Vector3(0, 10, 6), 0, .2f));

//            //        //Simulating a weld (0DOF) between objects (could use Distance constraints as well):
//            //        Box weldPart1 = new Box(new Vector3(-1, 15, 5), 1, 1, 1, 10);
//            //        Box weldPart2 = new Box(new Vector3(1, 15, 5), 1, 1, 1, 10);
//            //        space.add(weldPart1);
//            //        space.add(weldPart2);
//            //        space.add(new BallSocketJoint(weldPart1, weldPart2, new Vector3(0, 15, 4), .01f, .2f));
//            //        space.add(new BallSocketJoint(weldPart1, weldPart2, new Vector3(0, 15, 6), .01f, .2f));
//            //        space.add(new BallSocketJoint(weldPart1, weldPart2, new Vector3(0, 16, 5), .01f, .2f));

//            //        //Hinge connected to the space:
//            //        Box loneHingePart = new Box(new Vector3(5, 10, 0), 1, 1, 1, 1);
//            //        space.add(loneHingePart);
//            //        space.add(new BallSocketJoint(null, loneHingePart, new Vector3(0, 10, -1), 0, .2f));
//            //        space.add(new BallSocketJoint(null, loneHingePart, new Vector3(0, 10, 1), 0, .2f));


//            //        //Point on Line anchored to the space:
//            //        /* If the ground and pointOnLinePart1 are switched in the constraint definition, it can be seen how the axis is anchored to the first
//            //         * object.  If pointOnLinePart1 comes first, the small slope error caused by the physically simulated nature of the object will cause it
//            //         * to slide downwards and accelerate.
//            //         */
//            //        Box pointOnLineBox = new Box(new Vector3(-10f, 10, 3), 1, 1, 1, 50);
//            //        space.add(pointOnLineBox);
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(-10, 10, 0), new Vector3(1, 0, 0), 0f, .2f));

//            //        //Breakable Ball-socket Joint
//            //        Box breakablePart1 = new Box(new Vector3(8, 2, 8), 1, 1, 1, 10);
//            //        space.add(breakablePart1);
//            //        space.add(new BallSocketJoint(null, breakablePart1, new Vector3(8, 4, 8), 0, .2f, 50));

//            //        //Prismatic/Slider Joint
//            //        /* By default, this setup has a single, linear degree of freedom.  By removing the last constraint, a rotational degree of freedom is added,
//            //         * allowing the block to spin on the axis defined by the span between the remaining joints' axes.
//            //         */
//            //        pointOnLineBox = new Box(new Vector3(0f, 10, 10), 2, 2, 2, 50);
//            //        space.add(pointOnLineBox);
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 10, 9), new Vector3(1, 0, 0), .01f, .2f));
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 10, 11), new Vector3(1, 0, 0), .01f, .2f));
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 11, 10), new Vector3(1, 0, 0), .01f, .2f));

//            //        //Breakable Slider Joint
//            //        pointOnLineBox = new Box(new Vector3(0f, 10, 15), 1, 1, 1, 50);
//            //        space.add(pointOnLineBox);
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 10, 14), new Vector3(1, 0, 0), .01f, .2f, 100));
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 10, 16), new Vector3(1, 0, 0), .01f, .2f, 100));
//            //        space.add(new PointOnLineJoint(null, pointOnLineBox, new Vector3(0, 11, 15), new Vector3(1, 0, 0), .01f, .2f, 100));

//            //        //Springs: Create a lattice of springs to hold the boxes together.
//            //        Box springPart1 = new Box(new Vector3(-10, 0, 10), 2, 1, 2, 7);
//            //        Box springPart2 = new Box(new Vector3(-10, 4, 10), 2, 1, 2, 7);
//            //        space.add(springPart1);
//            //        space.add(springPart2);
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 9), new Vector3(-11, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 9), new Vector3(-9, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 9), new Vector3(-11, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 11), new Vector3(-9, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 11), new Vector3(-11, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 11), new Vector3(-9, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 11), new Vector3(-11, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 11), new Vector3(-9, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-9, .5f, 11), new Vector3(-9, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 9), new Vector3(-11, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 9), new Vector3(-11, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 11), new Vector3(-11, 3.5f, 9), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(springPart1, springPart2, new Vector3(-11, .5f, 11), new Vector3(-11, 3.5f, 11), 120, .5f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        //Distance Constraints: Create another similar lattice for comparison to springs.
//            //        Box distancePart1 = new Box(new Vector3(10, 0, 10), 2, 1, 2, 2);
//            //        Box distancePart2 = new Box(new Vector3(10, 4, 10), 2, 1, 2, 2);
//            //        space.add(distancePart1);
//            //        space.add(distancePart2);
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 9), new Vector3(11, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 9), new Vector3(9, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 9), new Vector3(11, 3.5f, 9), 0f, .3f, float.MaxValue));

//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 11), new Vector3(9, 3.5f, 11), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 11), new Vector3(11, 3.5f, 11), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 11), new Vector3(9, 3.5f, 11), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 11), new Vector3(11, 3.5f, 11), 0f, .3f, float.MaxValue));

//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, 11), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 11), new Vector3(9, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(9, .5f, 11), new Vector3(9, 3.5f, 11), 0f, .3f, float.MaxValue));

//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 9), new Vector3(11, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 9), new Vector3(11, 3.5f, 11), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 11), new Vector3(11, 3.5f, 9), 0f, .3f, float.MaxValue));
//            //        space.add(new DistanceConstraint(distancePart1, distancePart2, new Vector3(11, .5f, 11), new Vector3(11, 3.5f, 11), 0f, .3f, float.MaxValue));

//            //        //DistanceRange Constraints: Yet another similar lattice for comparison.
//            //        Box distanceRangePart1 = new Box(new Vector3(0, 0, 15), 2, 1, 2, 2);
//            //        Box distanceRangePart2 = new Box(new Vector3(0, 4, 15), 2, 1, 2, 2);
//            //        space.add(distanceRangePart1);
//            //        space.add(distanceRangePart2);
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 14), new Vector3(-1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 14), new Vector3(1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 14), new Vector3(-1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 14), new Vector3(1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));

//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 16), new Vector3(-1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 16), new Vector3(1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 16), new Vector3(-1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 16), new Vector3(1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));

//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 14), new Vector3(-1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 16), new Vector3(-1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 14), new Vector3(-1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(-1, .5f, 16), new Vector3(-1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));

//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 14), new Vector3(1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 16), new Vector3(1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 14), new Vector3(1, 3.5f, 16), 1, 3, 0, .3f, float.MaxValue));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart2, new Vector3(1, .5f, 16), new Vector3(1, 3.5f, 14), 1, 3, 0, .3f, float.MaxValue));

//            //        //"Intersecting Spheres" allowed movement volume, created from two distance-range constraints.  Make it breakable, too.
//            //        distanceRangePart1 = new Box(new Vector3(-7, 10, -10), 1, 1, 1);
//            //        distanceRangePart2 = new Box(new Vector3(7, 10, -10), 1, 1, 1);
//            //        Sphere distanceRangePart3 = new Sphere(new Vector3(0, 10, -10), 1, 3);
//            //        space.add(distanceRangePart1);
//            //        space.add(distanceRangePart2);
//            //        space.add(distanceRangePart3);
//            //        space.add(new DistanceRangeConstraint(distanceRangePart1, distanceRangePart3, new Vector3(-6.5f, 10, -10), new Vector3(0, 10, -10), 2, 12, 0, .2f, 100));
//            //        space.add(new DistanceRangeConstraint(distanceRangePart2, distanceRangePart3, new Vector3(6.5f, 10, -10), new Vector3(0, 10, -10), 2, 12, 0, .2f, 100));

//            //        camera.position = new Vector3(0, 7, 35);
//            //        break;
//            //    #endregion
//            //    #region BroadPhase Stress Test
//            //    case 23:
//            //        //Make a fatter kapow sphere.
//            //        space.remove(kapow);
//            //        kapow = new Sphere(new Vector3(11000, 0, 0), 1f, 1000);
//            //        kapow.collisionMargin = .5f;
//            //        space.add(kapow);
//            //        space.simulationSettings.iterations = 1; //Essentially no sustained contacts, so don't need to worry about accuracy.
//            //        space.simulationSettings.gravity = Vector3.Zero;
//            //        numColumns = 15;
//            //        numRows = 15;
//            //        numHigh = 15;
//            //        float separation = 3;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {


//            //                    toAddBox = new Box(new Vector3(separation * i, k * separation, separation * j), 1, 1, 1, 1);
//            //                    toAddBox.bounciness = 1; //Superbouncy boxes help propagate shock waves.
//            //                    toAddBox.angularDamping = 0f; //It looks cooler if boxes don't slowly stop spinning!
//            //                    //toAddBox.isTangible = false;
//            //                    space.add(toAddBox);


//            //                }

//            //        camera.position = new Vector3(0, 3, -10);
//            //        camera.yaw = -(float)Math.PI;
//            //        break;
//            //    #endregion
//            //    #region Ball Pit
//            //    case 24:
//            //        //Create and drop the balls into the pit.
//            //        numColumns = 3;
//            //        numRows = 3;
//            //        numHigh = 7;
//            //        separation = 5f;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {


//            //                    toAddSphere = new Sphere(new Vector3(separation * i - numRows * separation / 2f, 25 + k * separation, separation * j - numColumns * separation / 2f), 2, 1);
//            //                    toAddSphere.collisionMargin = .4f;
//            //                    space.add(toAddSphere);


//            //                }



//            //        //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
//            //        //Setup the heights of the terrain.
//            //        xLength = 48;
//            //        yLength = 48;

//            //        xSpacing = 3;
//            //        ySpacing = 3;
//            //        heights = new float[xLength, yLength];

//            //        for (int i = 0; i < xLength; i++)
//            //        {
//            //            for (int j = 0; j < yLength; j++)
//            //            {
//            //                x = i - xLength / 2;
//            //                y = j - yLength / 2;
//            //                heights[i, j] = (float)Math.Min(20, Math.Pow(.8 * Math.Sqrt(x * x + y * y), 2));
//            //                //heights[i, j] = (float)(x * y / 1000f);
//            //                //heights[i, j] = (float)(10 * (Math.Sin(x / 8f) + Math.Sin(y / 8f)));
//            //                //heights[i, j] = 3 * (float)Math.Sin(x * y / 100f);
//            //                //heights[i, j] = (x * x * x * y - y * y * y * x) / 1000f;

//            //            }
//            //        }
//            //        //Create the terrain.
//            //        terrain = new Terrain(new Vector3(-xLength * xSpacing / 2, 0, -yLength * ySpacing / 2));
//            //        terrain.setData(heights, QuadFormats.lowerLeftUpperRight, xSpacing, ySpacing);
//            //        space.add(terrain);


//            //        dispTerrain = new DisplayTerrain(terrain, graphics);
//            //        entityRenderer.addDisplayObject(dispTerrain);

//            //        //Put some boxes around the edge for shoving purposes.
//            //        for (double k = 0; k < Math.PI * 1.99; k += Math.PI / 12)
//            //        {
//            //            space.add(new Box(new Vector3((float)Math.Cos(k) * 25, 22, (float)Math.Sin(k) * 25), 2, 4, 2, 50));
//            //        }

//            //        camera.position = new Vector3(0, 30, 20);
//            //        break;
//            //    #endregion
//            //    #region Buoyancy
//            //    case 25:
//            //        List<Vector3[]> tris = new List<Vector3[]>();
//            //        float basinWidth = 100;
//            //        float basinLength = 100;
//            //        float basinHeight = 16;
//            //        float waterHeight = 15;

//            //        //Remember, the triangles composing the surface need to be coplanar with the surface.  In this case, this means they have the same height.
//            //        tris.Add(new Vector3[] { new Vector3(-basinWidth / 2, waterHeight, -basinLength / 2), new Vector3(basinWidth / 2, waterHeight, -basinLength / 2), new Vector3(-basinWidth / 2, waterHeight, basinLength / 2) });
//            //        tris.Add(new Vector3[] { new Vector3(-basinWidth / 2, waterHeight, basinLength / 2), new Vector3(basinWidth / 2, waterHeight, -basinLength / 2), new Vector3(basinWidth / 2, waterHeight, basinLength / 2) });
//            //        FluidVolume fluid = new FluidVolume(new Vector3(0, waterHeight, 0), Vector3.Up, tris, waterHeight, 1f, 8, .7f, .7f);
//            //        //fluid.flowDirection = Vector3.Right;
//            //        //fluid.flowForce = 80;
//            //        //fluid.maxFlowSpeed = 50;
//            //        space.add(fluid);
//            //        entityRenderer.addDisplayObject(new DisplayFluid(fluid, graphics, true, false));
//            //        //Create the container.
//            //        space.add(new Box(new Vector3(0, 0, 0), basinWidth, 1, basinLength));
//            //        space.add(new Box(new Vector3(-basinWidth / 2 - .5f, basinHeight / 2 - .5f, 0), 1, basinHeight, basinLength));
//            //        space.add(new Box(new Vector3(basinWidth / 2 + .5f, basinHeight / 2 - .5f, 0), 1, basinHeight, basinLength));
//            //        space.add(new Box(new Vector3(0, basinHeight / 2 - .5f, -basinLength / 2 - .5f), basinWidth + 2, basinHeight, 1));
//            //        space.add(new Box(new Vector3(0, basinHeight / 2 - .5f, basinLength / 2 + .5f), basinWidth + 2, basinHeight, 1));
//            //        Random random = new Random();

//            //        //Create a bunch of random blocks.
//            //        /*for (int k = 0; k < 1; k++)
//            //        {
//            //            toAddBox = new Box(new Vector3(random.Next((int)basinWidth) - basinWidth / 2f, 30 + (.1f) * k, random.Next((int)basinLength) - basinLength / 2f), 2, 4, 2, 12);
//            //            toAddBox.collisionMargin = .2f;
//            //            toAddBox.allowedPenetration = .1f;
//            //            toAddBox.orientationQuaternion = Quaternion.Normalize(new Quaternion((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));
//            //            space.add(toAddBox);
//            //        }*/

//            //        //Create a tiled floor.
//            //        for (int i = 0; i < 9; i++)
//            //        {
//            //            for (int j = 0; j < 1; j++)
//            //            {
//            //                for (int k = 0; k < 9; k++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(-basinWidth / 2f + 10.1f * (i + 1), 15 + .01f * j, -basinLength / 2f + 10.1f * (k + 1)), 10, 5, 10, 400);
//            //                    toAddBox.dynamicFriction = .6f; //Makes it a little easier to turn while driving.
//            //                    space.add(toAddBox);
//            //                }
//            //            }
//            //        }


//            //        //Create a bunch o' spheres and dump them into the water.
//            //        /*for (int k = 0; k < 80; k++)
//            //        {
//            //            toAddSphere = new Sphere(new Vector3(-48 + k * 1f, 12 + 4 * k, (float)random.Next(-15, 15)), 2, 27);
//            //            space.add(toAddSphere);
//            //        }*/


//            //        camera.position = new Vector3(0, waterHeight + 5, -15);
//            //        camera.yaw = (float)Math.PI;
//            //        break;
//            //    #endregion
//            //    #region Tornado
//            //    case 26:
//            //        Entity forceFieldShape = new Box(new Vector3(-60, 60, 0), 80, 150, 80);
//            //        //Get the tornado moving.  Note the use of 'internal' linear velocity; since this object isn't handled by the space it won't have its buffered velocity set.
//            //        forceFieldShape.internalLinearVelocity = new Vector3(5, 0, 0);
//            //        Tornado tornado = new Tornado(forceFieldShape, new Vector3(0, 1, 0), 150, false, 140, 20, 1000, 100, 10, 1000, 40, 30);
//            //        space.add(tornado); //Note how the forceFieldShape entity was not added to the space; it is instead handled by the Tornado itself.

//            //        //Create the unfortunate box-like citizens about to be hit by the tornado.
//            //        numColumns = 10;
//            //        numRows = 10;
//            //        numHigh = 1;
//            //        separation = 1.5f;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {


//            //                    toAddBox = new Box(new Vector3(separation * i - numRows * separation / 2, 5 + k * separation, separation * j - numColumns * separation / 2), 1, 1, 1, 10);
//            //                    space.add(toAddBox);


//            //                }

//            //        //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
//            //        //Setup the heights of the terrain.
//            //        xLength = 256;
//            //        yLength = 256;

//            //        xSpacing = 8f;
//            //        ySpacing = 8f;
//            //        heights = new float[xLength, yLength];
//            //        for (int i = 0; i < xLength; i++)
//            //        {
//            //            for (int j = 0; j < yLength; j++)
//            //            {
//            //                x = i - xLength / 2;
//            //                y = j - yLength / 2;
//            //                //heights[i, j] = (float)Math.Pow(1.2 * Math.Sqrt(x * x + y * y), 2);
//            //                //heights[i, j] = -1f / (x * x + y * y);
//            //                //heights[i, j] = (float)(x * y / 100f);
//            //                heights[i, j] = (float)(5 * (Math.Sin(x / 8f) + Math.Sin(y / 8f)));
//            //                //heights[i, j] = 3 * (float)Math.Sin(x * y / 100f);
//            //                //heights[i, j] = (x * x * x * y - y * y * y * x) / 1000f;

//            //            }
//            //        }
//            //        //Create the terrain.
//            //        terrain = new Terrain(new Vector3(-xLength * xSpacing / 2, 0, -yLength * ySpacing / 2));
//            //        terrain.setData(heights, QuadFormats.lowerLeftUpperRight, xSpacing, ySpacing);
//            //        space.add(terrain);

//            //        entityRenderer.addDisplayObject(new DisplayTerrain(terrain, graphics));

//            //        camera.position = new Vector3(0, 5, 60);
//            //        break;
//            //    #endregion
//            //    #region Gravitational Fields
//            //    case 27:
//            //        space.simulationSettings.gravity = Vector3.Zero;

//            //        forceFieldShape = new Sphere(new Vector3(0, 20, 0), 1000);
//            //        //The effective mass of the field, assuming 6.673*10^-11 as G, would be 10,000,000,000,000 kilograms.
//            //        GravitationalField field = new GravitationalField(forceFieldShape, 667.3f, 1000);
//            //        space.add(field);
//            //        space.add(new Sphere(new Vector3(0, 20, 0), 30));

//            //        //Drop the "meteorites" on the planet.
//            //        numColumns = 4;
//            //        numRows = 4;
//            //        numHigh = 4;
//            //        separation = 3f;
//            //        space.simulationSettings.defaultMargin = .2f;
//            //        space.simulationSettings.defaultAllowedPenetration = .1f;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {


//            //                    toAddBox = new Box(new Vector3(separation * i - numRows * separation / 2, 70 + k * separation, separation * j - numColumns * separation / 2), 1f, 1f, 1f, 1);
//            //                    space.add(toAddBox);


//            //                }
//            //        camera.position = new Vector3(0, 20, 70);
//            //        break;
//            //    #endregion
//            //    #region Playground
//            //    case 28:
//            //        //Load in mesh data and create the group.
//            //        StaticTriangleGroup.StaticTriangleGroupVertex[] staticTriangleVertices;
//            //        int[] staticTriangleIndices;
//            //        //This load method wraps the TriangleMesh.getVerticesAndIndicesFromModel method to output vertices of type StaticTriangleGroupVertex instead of TriangleMeshVertex or simply Vector3.
//            //        //
//            //        StaticTriangleGroup.getVerticesAndIndicesFromModel(playgroundModel, out staticTriangleVertices, out staticTriangleIndices);
//            //        TriangleMesh mesh = new TriangleMesh(staticTriangleVertices, staticTriangleIndices, 0);
//            //        StaticTriangleGroup group = new StaticTriangleGroup(mesh);
//            //        group.friction = .6f; //Makes driving a bit easier.


//            //        space.add(group);
//            //        //Modify the group's world matrix to rotate it around (or transform it in general).
//            //        group.worldMatrix = Matrix.CreateFromYawPitchRoll((float)Math.PI, 0, 0) * Matrix.CreateTranslation(0, -10, 0);
//            //        //Dump some boxes on top of it for fun.
//            //        numColumns = 8;
//            //        numRows = 8;
//            //        numHigh = 1;
//            //        separation = 17;
//            //        DisplayModel displayBox;
//            //        for (int i = 0; i < numRows; i++)
//            //            for (int j = 0; j < numColumns; j++)
//            //                for (int k = 0; k < numHigh; k++)
//            //                {


//            //                    toAddBox = new Box(new Vector3(separation * i - numRows * separation / 2, 40.00f + k * separation, separation * j - numColumns * separation / 2), 1f, 1f, 1f, 15);
//            //                    space.add(toAddBox);
//            //                    //Give the boxes a non-standard model.
//            //                    displayBox = new DisplayModel(boxModel, toAddBox);
//            //                    entityRenderer.displayModels.Add(displayBox);
//            //                    toAddBox.tag = "noDisplayObject"; //Prevent a normal DisplayObject from being created for this entity later.


//            //                }
//            //        DisplayModel groupDisplayModel = new DisplayModel(playgroundModel); //Give the model itself to the display system.
//            //        groupDisplayModel.worldMatrix = group.worldMatrix;
//            //        entityRenderer.displayModels.Add(groupDisplayModel);
//            //        camera.position = new Vector3(0, 10, 40);
//            //        break;
//            //    #endregion
//            //    #region Fancy Shapes
//            //    case 29:
//            //        List<Vector3> points = new List<Vector3>();
                    
//            //        //Setup a random distribution in a cube and compute a convex hull.
//            //        random = new Random();
//            //        for (int k = 0; k < 40; k++)
//            //        {
//            //            points.Add(new Vector3(3 * (float)random.NextDouble(), 7 + 5 * (float)random.NextDouble(), 3 * (float)random.NextDouble()));
//            //        }
//            //        ConvexHull convexHull = new ConvexHull(points, 10);

//            //        space.add(convexHull);
                    
                  
//            //        points.Clear();

//            //        //Create another random distribution, but this time with more points.
//            //        points.Clear();
//            //        for (int k = 0; k < 400; k++)
//            //        {
//            //            points.Add(new Vector3(4 + 3 * (float)random.NextDouble(), 7 + 3 * (float)random.NextDouble(), 3 * (float)random.NextDouble()));
//            //        }
//            //        convexHull = new ConvexHull(points, 50);
//            //        space.add(convexHull);
                    

//            //        //Minkowski Sums are fancy 'combinations' of objects, where the result is the sum of the individual points making up shapes.
//            //        //Think of it as sweeping one shape around and through another; a sphere and a box would produce a rounded-edge box.
//            //        //More examples can be seen below.  Note in the third example how a minkowski sum is based on another minkowski sum.
//            //        space.add(new MinkowskiSum(new Vector3(0, 0, 0), new Box(Vector3.Zero, 1, 1, 1), new Cone(Vector3.Zero, 2, 2), 100));
//            //        space.add(new MinkowskiSum(new Vector3(0, 3, 0), new Cone(Vector3.Zero, 1f, 1), new Triangle(Vector3.Zero, Vector3.Right, Vector3.Forward), 100));
//            //        space.add(new MinkowskiSum(new Vector3(0, 6, 0), new MinkowskiSum(new Vector3(10, 0, 0), new Cone(Vector3.Zero, 1f, 1), new Triangle(Vector3.Zero, Vector3.Up, Vector3.Forward * 2)), new Box(Vector3.Zero, 1, 1, 1), 50));
//            //        space.add(new MinkowskiSum(new Vector3(0, 12, 0), new Cylinder(Vector3.Zero, 1, 2), new Triangle(new Vector3(1, 1, 1), new Vector3(-2, 0, 0), new Vector3(0, -1, 0)), 50));

//            //        //Wrapped objects use an implicit convex hull around a set of shapes.

//            //        //Oblique cone:
//            //        List<Entity> cone = new List<Entity>();
//            //        Cylinder coneBase = new Cylinder(new Vector3(0, 0, 0), 0, 1f);
//            //        Sphere conePoint = new Sphere(new Vector3(1, 2, 0), 0f);
//            //        cone.Add(coneBase);
//            //        cone.Add(conePoint);
//            //        space.add(new WrappedBody(new Vector3(-3, 0, 0), cone, 50));

//            //        //Rather odd shape:
//            //        List<Entity> oddShape = new List<Entity>();
//            //        Sphere bottom = new Sphere(new Vector3(-2, 2, 0), 2f);
//            //        Cylinder middle = new Cylinder(new Vector3(-2, 3, 0), 0, 3);
//            //        middle.orientationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 6);
//            //        Sphere top = new Sphere(new Vector3(-2, 4, 0), 1f);
//            //        oddShape.Add(bottom);
//            //        oddShape.Add(middle);
//            //        oddShape.Add(top);
//            //        space.add(new WrappedBody(new Vector3(-3, 4, 0), oddShape, 100));

                    


//            //        space.add(new Box(new Vector3(0, -10, 0), 70, 5, 70));
//            //        camera.position = new Vector3(0, 0, 30);
//            //        break;
//            //    #endregion
//            //    #region Fish in a Barrel
//            //    case 30:
//            //        camera.position = new Vector3(0, 7, 30);

//            //        Box detector = new Box(new Vector3(0, 0, 0), 1.5f, 1.5f, 1.5f);
//            //        detector.isDetector = true;
//            //        Box acceptedTrigger = new Box(new Vector3(5, 0, 0), 1.6f, .7f, .4f, 1);

//            //        detector.tag = "noDisplayObject";
//            //        acceptedTrigger.tag = "noDisplayObject";
//            //        space.add(detector);
//            //        space.add(acceptedTrigger);
//            //        entityRenderer.displayModels.Add(new DisplayModel(barrelAndPlatform, detector));
//            //        entityRenderer.displayModels.Add(new DisplayModel(fish, acceptedTrigger));

//            //        StaticTriangleGroup.getVerticesAndIndicesFromModel(barrelAndPlatform, out staticTriangleVertices, out staticTriangleIndices);
//            //        //Note that the final 'margin' parameter is optional, but can be used to specify a collision margin on triangles in the static triangle group.
//            //        mesh = new TriangleMesh(staticTriangleVertices, staticTriangleIndices, 0);
//            //        StaticTriangleGroup fishDepositoryMesh = new StaticTriangleGroup(mesh);
//            //        space.add(fishDepositoryMesh);


//            //        EntityEventListenerDemo30 listener = new EntityEventListenerDemo30(detector, acceptedTrigger);

//            //        break;
//            //    #endregion
//            //    #region Earthquake!
//            //    case 31:

//            //        ground = new Box(new Vector3(0, 0, 0), 50f, 1f, 50f);
//            //        space.add(ground);
//            //        //Springs: Create a lattice of springs to hold the boxes together.
//            //        Box platform = new Box(new Vector3(0, 4, 0), 18, 1, 18, 400);
//            //        platform.dynamicFriction = .8f;

//            //        space.add(platform);

//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, -9), new Vector3(9, 3.5f, -9), 3000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, -9), new Vector3(-9, 3.5f, -9), 3000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, -9), new Vector3(9, 3.5f, -9), 3000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, -9), new Vector3(-9, 3.5f, -9), 3000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, 9), new Vector3(-9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, 9), new Vector3(9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, -9), new Vector3(9, 3.5f, -9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, -9), new Vector3(9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, -9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(9, .5f, 9), new Vector3(9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, -9), new Vector3(-9, 3.5f, -9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, -9), new Vector3(-9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, -9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));
//            //        space.add(new Spring(ground, platform, new Vector3(-9, .5f, 9), new Vector3(-9, 3.5f, 9), 6000, .0f, float.MaxValue, float.MaxValue, float.MaxValue));

//            //        numBlocksTall = 10; //How many 'stories' tall.
//            //        blockWidth = 4; //Total width/length of the tower.
//            //        heightPerBlock = 1.333f;
//            //        for (int i = 0; i < numBlocksTall; i++)
//            //        {
//            //            if (i % 2 == 0)
//            //            {
//            //                for (int j = 0; j < 3; j++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(j * (blockWidth / 3 + space.simulationSettings.defaultMargin * 2) - blockWidth / 3f - space.simulationSettings.defaultMargin * 2, 5 + heightPerBlock / 2 + space.simulationSettings.defaultMargin * 2 + i * (heightPerBlock + space.simulationSettings.defaultMargin * 2), 0), blockWidth / 3, heightPerBlock, blockWidth, 3 * numBlocksTall + 1 - 2 * i);
//            //                    space.add(toAddBox);
//            //                    toAddBox.dynamicFriction = .8f;
//            //                }
//            //            }
//            //            else
//            //            {
//            //                for (int j = 0; j < 3; j++)
//            //                {
//            //                    toAddBox = new Box(new Vector3(0, 5 + heightPerBlock / 2 + space.simulationSettings.defaultMargin * 2 + (i) * (heightPerBlock + space.simulationSettings.defaultMargin * 2), j * (blockWidth / 3 + space.simulationSettings.defaultMargin * 2) - blockWidth / 3f - space.simulationSettings.defaultMargin * 2), blockWidth, heightPerBlock, blockWidth / 3, 3 * numBlocksTall + 1 - 2 * i);
//            //                    space.add(toAddBox);
//            //                    toAddBox.dynamicFriction = .8f;
//            //                }
//            //            }
//            //        }

//            //        camera.position = new Vector3(0, 7, 30);

//            //        break;
//            //    #endregion
//            //    #region Detector Volume
//            //    case 32:

//            //        TriangleMeshVertex[] modelVertices;
//            //        int[] modelIndices;
//            //        TriangleMesh.getVerticesAndIndicesFromModel(tubeModel, out modelVertices, out modelIndices);
//            //        DetectorVolume detectorVolume = new DetectorVolume(new TriangleMesh(modelVertices, modelIndices));
//            //        space.add(detectorVolume);


//            //        DisplayModel detectorModel = new DisplayModel(tubeModel);
//            //        detectorModel.worldMatrix = detectorVolume.triangleMesh.worldMatrix;
//            //        entityRenderer.displayModels.Add(detectorModel);


//            //        toAddBox = new Box(new Vector3(0, -10, 0), 1, 1, 1);
//            //        toAddBox.tag = "noDisplayObject";
//            //        DisplayBox displayTestBox = new DisplayBox(toAddBox, graphics, true, true);
//            //        entityRenderer.addDisplayObject(displayTestBox, 0);
//            //        toAddBox.linearVelocity = new Vector3(0, 1, 0);
//            //        space.add(toAddBox); //Create the ground


//            //        DetectorVolumeEventListenerDemo32 volumeListener = new DetectorVolumeEventListenerDemo32(detectorVolume, displayTestBox);


//            //        camera.position = new Vector3(0, 0, 22);



//            //        break;
//            //    #endregion