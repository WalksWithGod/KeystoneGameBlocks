//using System;
//using System.Diagnostics;
//using Amib.Threading;
//using Keystone.Appearance;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Enum;
//using Keystone.IO;
//using Keystone.Types;
//using JigLibX.Collision;
//using MTV3D65;

//namespace Keystone.Commands
//{
//    //public enum PhysicsDemo
//    //{
//    //    Wall,
//    //    Incoming,
//    //    LotsOfSpheres,
//    //    Pendulum,
//    //    Catapult,
//    //    Plinko,
//    //    Cubes,
//    //    BounceAndFriction
//    //}

//    public class ImportJigLibPhysicsDemo : ImportEntityBase
//    {
//        public bool LoadTextures = true;
//        public bool LoadMaterials = true;
//        private EntityBase _parent;
//        private Vector3d _localPosition;
//        private Mesh3d[] _resources;
//        private PhysicsDemo _demo;
//        private DefaultAppearance _app;
//        private bool _writeToLibrary;
//        private string _libraryFileName;
//        Random _random = new Random();

//        public ImportJigLibPhysicsDemo(PhysicsDemo demo, EntityBase parent, Vector3d localPosition, bool writeToLibrary, string libraryFileName,
//                                  PostExecuteWorkItemCallback completionCB)
//            : base(completionCB)
//        {
//            if (parent == null) throw new ArgumentNullException();
//            _parent = parent;
//            Position = _localPosition = localPosition;
//            NodeType = typeof(Mesh3d);
//            EntityType = typeof(StaticEntity);
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
//            // _resource.FrustumCullMode = FrustumCullMode.custom;
//            StaticEntity newEntity;

//            switch (_demo)
//            {
//                case PhysicsDemo.Wall:

//                    // the floor
//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[1]);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, -.5, 0);

//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.Immovable = true;
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);

//                    float mass = 10000;                    
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(-25, -.5f, -25, 50,1,50), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.MoveTo((float)0, (float)-0.5, (float)0);
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[0]);

//                    int width = 10;
//                    int height = 10;
//                    float blockWidth = 2;
//                    float blockHeight = 1;
//                    float blockLength = 1;

//                    mass = 1;
//                    float xSpacing = blockWidth + defaultMargin * 2;
//                    float ySpacing = blockHeight + defaultMargin * 2;
//                    for (int i = 0; i < width; i++)
//                    {
//                        for (int j = 0; j < height; j++)
//                        {
//                            newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                            newEntity.Enable = true;
//                            double x = i * xSpacing + .5f * blockWidth * (j % 2) - width * xSpacing * .5f;
//                            double y = blockHeight * .5f + defaultMargin * 2 + j * ySpacing;
//                            double z = 0;

//                            newEntity.Translation = new Vector3d(x, y, z);

//                            newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                            newEntity.PhysicsBody.MoveTo((float)x, (float)y, (float)z);
//                            newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                            newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth , blockHeight , blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                            newEntity.PhysicsBody.Initialize(mass);
//                            _parent.AddChild(newEntity);
//                        }
//                    }
//                    break;

//                #region Incoming
//                case PhysicsDemo.Incoming:
//                    // the platform
//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[0]);
//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, 0, 0);

//                    blockWidth = 10;
//                    blockHeight = 1;
//                    blockLength = 10;
//                    mass = 10000;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(0, 0,0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth, blockHeight, blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Immovable = true;
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    //the smasher sphere
//                    SimpleModel Model2 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//                    Model2.AddChild(_app);
//                    ((SimpleModel)Model2).AddChild(_resources[3]); // sphere
//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model2);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, 150, 0);

//                    mass = 20;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(0, 150,0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Sphere( 3 * -.5f, 3 * -.5f, 3 * -.5f , 3 ), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    //Build the stack... 1st block type
//                    SimpleModel Model3 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//                    Model3.AddChild(_app);
//                    ((SimpleModel)Model3).AddChild(_resources[2]);

//                    // second block type
//                    SimpleModel Model4 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.blue));
//                    Model4.AddChild(_app);
//                    ((SimpleModel)Model4).AddChild(_resources[1]);

                    
//                    for (int k = 1; k < 11; k++)
//                    {
//                        float y = (1.0f + defaultMargin * 2.0f) * k;
//                        if (k % 2 == 1)
//                        {
//                            blockWidth = 1;
//                            blockHeight = 1;
//                            blockLength = 7;
//                            newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model4);
//                            newEntity.Enable = true;
//                            newEntity.Translation = new Vector3d(-3, y, 0);
//                            mass = 1;
//                            newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                            newEntity.PhysicsBody.MoveTo(-3, y, 0);
//                            newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                            newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth, blockHeight, blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                            newEntity.PhysicsBody.Initialize(mass);
//                            _parent.AddChild(newEntity);
//                            // TODO: above is failing on internal Initialize because the SceneNode from which world BoundingBox is grabbed
//                            // isn't computed... this is problematic.  
//                            // and when an entity is moved, the primitive also needs to be removed/added and re-inited at least
//                            // with the bounding volume updates... but maybe the key is that when
//                            // certain things like scale/bounding volume/position change, those will re-init what they need
//                            // also when PhysicsEnabled flag changes... this is part of the IPhysicsEntity i think.
//                            // well it seems the Space.Add() pretty much require a collision primitive be set at that point... 
//                            // so i think our solution is that if an entity is not yet added to the Scene, then it can't be added to the simulation
//                            // however we should still be able to set a primitive, and we only update parts of the primitive when it's add/remove
//                            // in scene is done or moved to a different region.

//                            newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model4);
//                            newEntity.Enable = true;
//                            newEntity.Translation = new Vector3d(3, y, 0);
//                            mass = 1;
//                            newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                            newEntity.PhysicsBody.MoveTo(3, y, 0);
//                            newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                            newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth, blockHeight, blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                            newEntity.PhysicsBody.Initialize(mass);
//                            _parent.AddChild(newEntity);
//                        }
//                        else
//                        {
//                            blockWidth = 7;
//                            blockHeight = 1;
//                            blockLength = 1;
//                            newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model3);
//                            newEntity.Enable = true;
//                            newEntity.Translation = new Vector3d(0, y, -3);
//                            mass = 1;
//                            newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                            newEntity.PhysicsBody.MoveTo(0, y, -3);
//                            newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                            newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth, blockHeight, blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                            newEntity.PhysicsBody.Initialize(mass);
//                            _parent.AddChild(newEntity);

//                            newEntity = new StaticEntity("block" + Resource.Repository.GetNewName(typeof(StaticEntity)), Model3);
//                            newEntity.Enable = true;
//                            newEntity.Translation = new Vector3d(0, y, 3);
//                           mass = 1;
//                        newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                        newEntity.PhysicsBody.MoveTo(0,y,3);
//                        newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                        newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(blockWidth * -.5f, blockHeight * -.5f, blockLength * -.5f, blockWidth, blockHeight, blockLength), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                        newEntity.PhysicsBody.Initialize(mass);
//                            _parent.AddChild(newEntity);
//                        }
//                    }
//                    break;
//                #endregion
//                #region lots of spheres
//                case PhysicsDemo.LotsOfSpheres:

//                    // the platform
//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[0]);
//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, 0, 0);
//                    mass = 10000;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.Immovable = true;
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    // this position is the "min" corner it seems to me such that the center position is always 0,0,0 in model space
//                    // this seems kind of retarded to me.  
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(70 * -.5f, 1 * -.5f, 70 * -.5f, 70, 1, 70), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);


//                    // the spheres
//                    SimpleModel Model5 = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//                    Model5.AddChild(_app);
//                    ((SimpleModel)Model5).AddChild(_resources[1]); // sphere

//                    int numColumns = 7;
//                    int numRows = 7;
//                    int numHigh = 7;
//                    xSpacing = 2.09f;
//                    ySpacing = 2.08f;
//                    float zSpacing = 2.09f;
//                    for (int i = 0; i < numRows; i++)
//                        for (int j = 0; j < numColumns; j++)
//                            for (int k = 0; k < numHigh; k++)
//                            {
//                                newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model5);
//                                newEntity.Enable = true;
//                                float x = 0 + xSpacing * i - (numRows - 1) * xSpacing / 2f;
//                                float y = 1.58f + k * (ySpacing);
//                                float z = 2 + zSpacing * j - (numColumns - 1) * zSpacing / 2f;
//                                newEntity.Translation = new Vector3d(x, y, z);
//                                mass = 1;
//                                newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                                newEntity.PhysicsBody.MoveTo(x, y, z);
//                                newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                                newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Sphere(1 * -.5f, 1 * -.5f, 1 * -.5f, 1), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                                newEntity.PhysicsBody.Initialize(mass);
//                                _parent.AddChild(newEntity);
                                
//                            }
//                    break;
//                #endregion
//                #region Catapult
//                case PhysicsDemo.Catapult:

//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.green));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[2]);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, -.5, 0);
//                    mass = 10000;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(0, -.5f, 0);
//                    newEntity.PhysicsBody.Immovable = true;
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(3 * -.5f, .5f * -.5f, 2 * -.5f, 3, .5f, 2), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.blue));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[1]);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(-1, .6f, 0);
//                    mass = 1;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(-1f, .6f, 0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(5 * -.5f, .7f * -.5f, 1 * -.5f, 5, .7f,1), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    Model = new SimpleModel(Resource.Repository.GetNewName(typeof(SimpleModel)));
//                    _app = new DefaultAppearance(Resource.Repository.GetNewName(typeof(DefaultAppearance)));
//                    _app.AddChild(Material.Create(Material.DefaultMaterials.red));
//                    Model.AddChild(_app);
//                    ((SimpleModel)Model).AddChild(_resources[0]);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, 1.7f, 0);
//                    mass = 1;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(0, 1.7f,0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(1 * -.5f, 1 * -.5f, 1 * -.5f, 1,1,1), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(0, 3.5f, 0);
//                    mass = 1;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(0, 3.5f, 0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(1 * -.5f, 1 * -.5f, 1 * -.5f, 1,1,1), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);

//                    newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), Model);
//                    newEntity.Enable = true;
//                    newEntity.Translation = new Vector3d(-2.5d, 10d, 0);
//                    mass = 20;
//                    newEntity.PhysicsBody = new JigLibX.Physics.Body();
//                    newEntity.PhysicsBody.MoveTo(-2.5f, 10, 0);
//                    newEntity.PhysicsBody.CollisionSkin = new CollisionSkin(newEntity.PhysicsBody);
//                    newEntity.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(1 * -.5f, 1 * -.5f, 1 * -.5f, 1,1,1), new MaterialProperties(0.1f, 0.5f, 0.5f));
//                    newEntity.PhysicsBody.Initialize(mass);
//                    _parent.AddChild(newEntity);
//                    break;
//                #endregion
//                #region Multi-pendulum
//                //case PhysicsDemo.MultiPendulum:
//                //    //Create the blocks in the pendulum.
//                //    Box link1 = new Box(new Vector3(3, 5, 0), 1.1f, 1.1f, 1.1f, 2);
//                //    Box link2 = new Box(new Vector3(6, 5, 0), 1.1f, 1.1f, 1.1f, 2);
//                //    space.add(link1);
//                //    space.add(link2);
//                //    Box ground = new Box(new Vector3(3, -3, 0), 20, 1, 10);
//                //    space.add(ground);
//                //    //Connect them together.
//                //    JigLibX.Physics.
//                //    space.add(new BallSocketJoint(link1, link2, (link1.centerPosition + link2.centerPosition) / 2, 0f, 1f));
//                //    space.add(new BallSocketJoint(link1, ground, new Vector3(0, 5, 0), 0f, 1f));
//                //    //Create a target stack.
//                //    for (int k = 0; k < 4; k++)
//                //    {
//                //        toAddBox = new Box(new Vector3(-2f, -1f + 2 * k, 0), 1.5f, 1.5f, 1.5f, 1);
//                //        space.add(toAddBox);
//                //    }
//                //    camera.position = new Vector3(0, 2, 20);
//                //    break;
//                #endregion
//                #region Plinko
//                case PhysicsDemo.Plinko:
//                    ////Drops a bunch of spheres down a plinko-style machine.


//                    //space.simulationSettings.gravity = new Vector3(0, -4, 10);
//                    ////Create the container.
//                    //space.add(new Box(new Vector3(0, -.5f, 0), 21f, 1f, 50));
//                    //space.add(new Box(new Vector3(-10, .5f, 0), 1f, 1f, 50));
//                    //space.add(new Box(new Vector3(10, .5f, 0), 1f, 1f, 50));
//                    //space.add(new Box(new Vector3(0, .5f, -24.5f), 19f, 1f, 1));
//                    //space.add(new Box(new Vector3(0, .5f, 24.5f), 19f, 1f, 1));

//                    ////Create some obstacles.
//                    //for (int i = 0; i < 4; i++)
//                    //{
//                    //    for (int j = 0; j < 8; j++)
//                    //    {
//                    //        space.add(new Cylinder(new Vector3(-7.5f + i * 5, .5f, -20 + j * 6), 1f, .8f));
//                    //    }
//                    //}
//                    ////Create some more obstacles, offset a bit.
//                    //for (int i = 0; i < 3; i++)
//                    //{
//                    //    for (int j = 0; j < 7; j++)
//                    //    {
//                    //        space.add(new Cylinder(new Vector3(-5f + i * 5, .5f, -17 + j * 6), 1f, .8f));
//                    //    }
//                    //}
//                    ////Create the spheres.
//                    //for (int i = 0; i < 15; i++)
//                    //{
//                    //    toAddSphere = new Sphere(new Vector3(-7f + i * 1, .5f, -22), .3f, .5f);
//                    //    space.add(toAddSphere);
//                    //    toAddSphere = new Sphere(new Vector3(-7f + i * 1, .5f, -20), .3f, .5f);
//                    //    space.add(toAddSphere);

//                    //}
//                    //camera.position = new Vector3(0, 25, 25);
//                    //camera.pitch = (float)Math.PI / -4;
//                    break;
//                #endregion
//                default:
//                    throw new Exception();

//            }

//            //TODO: fix FileManager.SuspendWrite = false;      // TODO: this method of on/off is lame... need something more robust?
//            // TODO: allow the _completionCB.Invoke to handle the saving
//            //FileManager.WriteNewNode(_parent.Parent,true); // TODO: writes shouldnt be done automaticall when the item is added to the scene correct?

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
//                        case PhysicsDemo.Wall:
//                            _resources = new Mesh3d[2];
//                            _resources[0] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 2, 1, 1);
//                            _resources[1] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 50, 1, 50);
//                            break;
//                        #endregion

//                        case PhysicsDemo.Incoming:
//                            _resources = new Mesh3d[4];
//                            _resources[0] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 10, 1, 10);
//                            _resources[1] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 1, 1, 7);
//                            _resources[2] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 7, 1, 1);
//                            _resources[3] = Mesh3d.CreateSphere(Resource.Repository.GetNewName(typeof(Mesh3d)), 3, 10, 10, false);
//                            break;
//                        case PhysicsDemo.LotsOfSpheres:
//                            _resources = new Mesh3d[2];
//                            _resources[0] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 70, 1, 70);
//                            _resources[1] = Mesh3d.CreateSphere(Resource.Repository.GetNewName(typeof(Mesh3d)), 1, 10, 10, false);
//                            break;

//                        case PhysicsDemo.Catapult:
//                            _resources = new Mesh3d[3];
//                            _resources[0] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 1, 1, 1);
//                            _resources[1] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 5, .7f, 1);
//                            _resources[2] = Mesh3d.CreateBox(Resource.Repository.GetNewName(typeof(Mesh3d)), 3, .5f, 2);
//                            break;
                            
//                        default:
//                            throw new Exception();
//                    }
//                }
//                else if (classname == typeof(Actor3d).ToString())
//                {
//                }
//                else if (classname == typeof(Terrain).ToString())
//                {
//                }
//                else if (classname == typeof(Minimesh).ToString())
//                {
//                }
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
