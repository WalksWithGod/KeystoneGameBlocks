//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Keystone.Cameras;
//using Keystone.Collision;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.FX;
//using Keystone.IO;
//using Keystone.Portals;
//using Keystone.Resource;
//using Keystone.Traversers;
//using Keystone.Types;
//using MTV3D65;

//namespace Keystone.Scene
//{
//    // NOTE: Hrm.  I had this inheriting FXBase because when SceneManager elements move, I wanted the SceneManager to be notifiied in case it needs to
//    // move that object within the scene to a different sector or octree node.  Or is there another mechanism we want to use for that?
//    // Hrm... but if i do do something whereby Update() will handle this sort of thing when its sending those translation commands to the
//    // the scene nodes, then it brings into question the Notify() model in general for FXBase... i mean, the Update() here can
//    // check for subscriptions and notify the FX directly?  Meh. I dunno.  Need to contemplate this.  
//    // On the one hand, it's only Shadows that really utilized subscribers the most... and Water generally doesnt move at runtime except in edit mode.
//    // 

//    /// <summary>
//    /// Only one SceneManager can exist which contains all graphs which are rendered together as a single logical "scene"
//    /// Because of the nature of how RenderBeforeClear, Render, PostRender work, only one logical scene can exist.
//    /// There would be no easy way to do two seperate renders where a second SceneManager also needs to RenderBeforeClear 
//    ///  That's why I think the best approach is to simply Add the second "scene" as just another graph that exists in 
//    /// a seperate "layer" and to include logic to handle the additional transitioning to/rendering of layers of graphs in addition to a list of graphs
//    /// </summary>
//    public class SceneManager2 : IDisposable
//    {
//        public delegate void EntitySpawned(EntityBase entity);
//        public delegate void EntityChanged(EntityBase entity);
//        public delegate void EntityDestroyed(EntityBase entity);
//        public delegate void EntitySelected(EntityBase entity);
//        public delegate void EntityMouseOver(EntityBase entity);

//        private EntitySpawned _entitySpawned;
//        private EntityChanged _entityChanged;
//        private EntityDestroyed _entityDestroyed;
//        private EntitySelected _entitySelected;
//        private EntityMouseOver _entityMouseOver;

//        // a root region that hosts an unbounded Region always exists because its required for portals which need to link to the root region
//        private Region _root;

//        // private RegionNode _rootNode; // TODO: i suspect i dont need a _rootNode because for a root region the bounding volume is already in world coords
//        // and for a series of "rootRegions" acting as Zones, those will use a dynamic translation during cull.
//        // given that, and when rendering we can determine neighboring regions by simple math
//        public static List<Region> _regions = new List<Region>(); // TODO: for now lets just store htem all here
        
//        public const string FIRST_CHILD = "firstchild"; // 
//        private IFXProvider[] _fxProviders = new IFXProvider[System.Enum.GetNames(typeof (FX_SEMANTICS)).Length];
//        private Pager _pager;

//        private readonly string _name;
//        private bool _disposed;
//        private bool _sceneLoaded = false;
        
//        private EntityBase _selected;
//        private SceneInfo _sceneInfo;
//        private SpatialGraph _spatialGraph;
//        private SceneNodeManager _sceneNodeManager = new SceneNodeManager();
//        private ITraverser _drawer;
//        private Traversers.Picker _picker;


//        private List<Scene> _scenes = new List<Scene>( );

//        public SceneManager2(string name)
//        {
//            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
//            _name = name;
//            _drawer = new Draw();
//            _picker = new Traversers.Picker();


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

//            // So one question is, should the GUI map directly to those handlers in say Network.Events or Reader.Events, Loader.Events?
//            //      
//        }

//        ~SceneManager2()
//        {
//            Dispose(false);
//        }


//        public string Name
//        {
//            get { return _name; }
//        }

//        public Region Root
//        {
//            get { return _root; }
//        }

//        public bool SceneLoaded
//        {
//            get { return _sceneLoaded; }
//        }

//        public SceneInfo SceneInfo
//        {
//            get { return _sceneInfo; }
//        }

//        public SpatialGraph SpatialGraph
//        {
//            get { return _spatialGraph; }
//        }

//        public EntityBase Selected
//        {
//            set { _selected = value; }
//            get { return _selected; }
//        }

//        public EntitySpawned EntitySpawnedCallback
//        {
//            get { return _entitySpawned; }
//            set { _entitySpawned = value; }
//        }

//        public EntityDestroyed EntityDestroyedCallback
//        {
//            get { return _entityDestroyed; }
//            set { _entityDestroyed = value; }
//        }

//        public EntityChanged EntityChangedCallback
//        {
//            get { return _entityChanged; }
//            set { _entityChanged = value; }
//        }

//        public EntitySelected EntitySelectedCallback
//        {
//            get { return _entitySelected; }
//            set { _entitySelected = value; }
//        }

//        public EntityMouseOver EntityMouseOverCallback
//        {
//            get { return _entityMouseOver; }
//            set { _entityMouseOver = value; }
//        }


//        // save's the current scene
//        public void Save(string filepath)
//        {
//            //TODO: fix FileManager.SaveAs(filepath);
//        }

//        /// <summary>
//        /// Initiates a mouse pick into the scene.  Alternatively there is
//        /// TVMaths.GetMousePickVectors (x, z, ref nearStart, ref farEnd)
//        /// </summary>
//        /// <param name="vp"></param>
//        /// <param name="mouseX">Client mouse position X value.</param>
//        /// <param name="mouseY">Client mouse position Y value</param>
//        public PickResults[] Pick(Viewport vp, int mouseX, int mouseY)
//        {
//            // project mouse coords to 3d and create a ray in the direction of the camera
//            Vector3d v, rayDir;
//            TV_3DMATRIX view = vp.Camera.Matrix;
//            TV_3DMATRIX projection = vp.Camera.ProjectionMatrix;

//            //Convert the point's coords from screen space (0 to wholeViewport width/height) to proj space (-1 to 1)
//            // NOTE: Height and Width are actual client dimensions and not window dimensions.
//            v.x = (((2.0f*mouseX)/vp.TVViewport.GetWidth()) - 1)/projection.m11;
//            v.y = -(((2.0f*mouseY)/vp.TVViewport.GetHeight()) - 1)/projection.m22;
//            v.z = 1.0f;

//            //Engine.Core.Maths.TVMatrixInverse(ref m,ref det, view); // only required if getting the view from device.  TV3D already provides the inverse
//            // Transform the screen space pick ray into 3D space
//            rayDir.x = v.x*view.m11 + v.y*view.m21 + v.z*view.m31;
//            rayDir.y = v.x*view.m12 + v.y*view.m22 + v.z*view.m32;
//            rayDir.z = v.x*view.m13 + v.y*view.m23 + v.z*view.m33;
//            // TODO: if we wanted the far plane hit point, we'd get the distance and scale the rayDir by that amount
//            //       and then add that to the v vector.
//            //      
//            // TODO: alternatively there is 
//            //Core._CoreClient.Maths.GetMousePickVectors (mouseX, mouseY, ref nearStart, ref farEnd)
//            return Pick(vp.Camera, vp.Camera.Position, rayDir);
//        }

//        // camera is required because we need access to camera.Region so we can translate the ray during picking
//        public PickResults[] Pick(Camera camera, Vector3d start, Vector3d direction)
//        {
//            Ray r = new Ray(start, direction);
//            PickResults[] results = null;

//            results = _picker.Pick(camera, _root.RegionNode, r, PickFilter.All, PickAccuracy.Point, true);
//            return results;
//        }


//        public void Load(string filename)
//        {
//            try
//            {
//                //TODO: fix FileManager.Open(filename);
//                SceneInfo sceneInfo = (SceneInfo) FileManager.Load(typeof (SceneInfo).Name, FIRST_CHILD, "");
//                Load(sceneInfo);
//                Core._CoreClient.Simulation = new Simulation.Simulation(9.0f, this);
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine("Error loading scene from archive. " + ex.Message);
//            }
//        }

//        // assumes that the archive is already open and all we need is the SceneInfo and then to start paging in regions
//        private void Load(SceneInfo sceneInfo)
//        {
//            _sceneInfo = sceneInfo;
//            Repository.IncrementRef(_sceneInfo);

//            _sceneLoaded = true;

//            Vector3d min = new Vector3d(float.MinValue*.5f, float.MinValue*.5f, float.MinValue*.5f);
//            Vector3d max = new Vector3d(float.MaxValue*.5f, float.MaxValue*.5f, float.MaxValue*.5f);

//            BoundingBox box = new BoundingBox(min, max);
//            _root = new Region("root", box, false, _sceneInfo.OctreeNodeDepth);
//            Repository.IncrementRef(_root);
//            _spatialGraph = new SpatialGraph(_sceneInfo, _root);
//            //TODO: fix FileManager.EnableWrite = false;
//            _pager = new Pager(Core._CoreClient.ThreadPool, 1);
//        }

//        public void Unload()
//        {
//            if (!_sceneLoaded) return;
//            FileManager.EnableWrite = true;
//            _sceneLoaded = false;
//            //note: when a node is disposed, it's reference count should be down to 0 at this point
//            // because when the ref count reaches 0, there should be no more references to the node
//            // unless they are held by the original caller and they shouldnt be.  
//            //
//            // IMPORTANT: Thus the Repository will call .Dispose() on the object and the 
//            //  overriden DisposeManaged/UnmangedResoruces should call an appropriate tv....Destroy() on any TV3D resource
//            // thus if there are any lingering references, they will be invalid so it is the Creator's responsibility to manually
//            // increment the reference count of this object if they dont want it to be disposed.

//            //But there's no way to enforce
//            // that.  In normal file save/load this can't happen, but when the user manually imports/creates from a static load procedure
//            // then there is no way to guarantee that so we leave those resources loaded.  There's no harm in that when we unload a scene
//            // only when we want to shut down the engine completely.


//            // root node and rootRegion dont have parents so instead of calling RemoveEntity() we'll special case it here
//            _root.RemoveChildren();
//            Repository.DecrementRef(_sceneInfo);
//            Repository.DecrementRef(_root);
//            _root.Dispose();
//            _sceneInfo.Dispose();
//            _root = null;
//            _sceneInfo = null;

//            // TODO: there maybe some objects that are instanced by the pager but never added to the scene and thus a call to
//            // _root.RemoveChildren() has no impact.  So when shutting down we need to lock the pager, cancel any existing jobs,
//            // and then dispose of all regions that hasnt been added and that should result in all of them being recursively removed as well.
//            _pager.Dispose();
//            _pager = null;
//        }

//        #region Add

//        // Having a single MoveEntity here is ok, but we would need overloads because sometimes
//        // sceneNode's need to be added/removed or modified and because sceneNode creation is not alike
//        // for all types.  E.g. interior region on a ship or building has different sceneNode reqts
//        // however having this called so we can raise an event in the application seems beneficial.  It's mostly
//        // we dont want the nuts and bolts of moving there... and maybe neither with scenenodes
//        public void EntityMoved (EntityBase child, EntityBase newParent)
//        {
//            // TODO: these events EntityAdded and EntityMoved should NOT be dealing with sceneNodes.
//            // here i need to be able to remove the child's scene node from it's previous parent but i cant?
//            child.SceneNode.Parent.RemoveChild(child.SceneNode);
//            SceneNode parentSN = newParent.SceneNode;
//            var tmpChild = child;

//            // TODO: SceneNode creation isn't always done here... sometimes the SceneNode is actually saved
//            // and loaded from disk like other scene elements!  So this complicates things.
//            if (parentSN != null)
//            {
//                SceneNode sn;
//                if (tmpChild is Region)
//                    sn = new RegionNode((Region)tmpChild);
//                else
//                    sn = new EntityNode(tmpChild);

//                parentSN.AddChild(sn);

//                // recurse child entities as these will need to have their sceneNode's updated to point
//                // to their parent's new sceneNode...
//                // but note there is no need to call .MoveTo() on children because their parent hasnt changed at all
//                // however, we dont really want to create new sceneNode's for the children, just move them...
//                if (tmpChild.Children != null)
//                    foreach (Node n in tmpChild.Children)
//                    {
//                        if (n is EntityBase)
//                            EntityMoved((EntityBase)n, tmpChild);
//                    }
//            }

//        }

//        /// NOTE: You cannot add Enties directly to the SpatialGraph.  
//        /// This is a fundamental concept that must be understood.  Entities represent Hierarchical 
//        /// relationships and not spatial ones.  If you don't know which hierarchical relationship
//        /// a child entity should have, you can't expect the program to figure it out.  
//        /// Therefore, you MUST know in advance which Entity you wish
//        /// to add a child entity to and then add that child directly to that parent entity. 
//        /// Now, you can pick into the spatial graph to return a desired entity (e.g. a region)
//        /// but then you will entity.AddChild(entity) and that parent entity will be responsible for inserting
//        /// that child into the SceneManager.  When we first determined we'd be seperating our entities from our
//        /// spatial graph, this was all apart of that fundamental design decision of how it _should_ work.  This is
//        /// desired behavior even though after taking weeks off coding sometimes, I forget these facts.  But the fact is
//        /// remembering this simplifies going foward because the restraints it imposes, points the correct path foward.
//        /// Thus, when adding a child entity to a parent, we must pass it the child's relative position to the parent's origin.
//        /// This too is by design and we keep forgetting.  Child entities position property is always in relative coords
//        /// with respect to the parent.  However, the entity.SceneNode.Translation is always in worldcoordinates. This is why
//        /// our SceneNode's position vars will need to use doubles.
//        internal void EntityAdded(EntityBase parent, EntityBase child)
//        {
//            //_sceneNodeManager.SetParent(parent);
//            //_sceneNodeManager.Apply(child);
//            if (child.RefCount > 1) return; // a ref count > 1 means it's already attached to the SceneGraph and is being Moved from one parent to another 

//            var tmpChild = child;
//            // TODO: since im not yet using _sceneNodeManager to do all scenenode management for spatial graph, right now when i page in a region
//            // its resulting in the Region getting a sceneNode but not it's StarSystems or Stars or Planets
//            SceneNode parentSN = parent.SceneNode;
//            if (parentSN != null)
//                // test for null because unconnected entity parents will not have a sceneNode so children will be unconnected as well.
//            {
//                SceneNode sn;
//                if (tmpChild is Region)
//                    sn = new RegionNode((Region) tmpChild);
//                else
//                    sn = new EntityNode(tmpChild);

//                parentSN.AddChild(sn);

//                // recurse child entities until until all have scenenodes and are into the spatial graph?  One easy way is to
//                // just call EntityAdded again with the relevant parent and child and to abort the entitySpawned.Invoke(child) if the entity is not yet added to the graph
//                if (tmpChild.Children != null)
//                    foreach (Node n in tmpChild.Children)
//                    {
//                        if (n is EntityBase)
//                            EntityAdded(tmpChild, (EntityBase) n);
//                    }
//            }
//            else return;

//            // TODO: some types like Players need to be in a seperate _players array?  not sure... 
//            // e.g.
//            //if (entity is Player)
//            //    _players.Add((Player)entity);
//            // not sure what to do about "position" because Directional doesnt have one, but to simulate sunlight
//            // i think maybe we should use a direction and even an artificial range to allow us to define the area
//            // and create a "box" and to maybe help us determine if when indoors when we are able to see light.
//            // And recall, even that is maybe not 100% enough because we dont want indoor meshes lit by sunlight
//            // it would be light coming through the walls.  So i think we need to turn lights on/off during the render
//            // So perhaps categorically, when rendering indoors, turn off the outdoor lights.  Maybe that's the deal.
//            //
//            // _root.Insert(l);  //_injector.Inject(l); // TODO: adds the light to the octree.  At runtime, the Renderer traversal
//            // should manage the indoor / outdoor light stack
//            //  _lights.Add(l);
//            //l.Enable = true;
//            _entitySpawned.Invoke(child);
//            // insert is called internally by container entities like Regions when they've added a child entity to them
//            // and now need for this child to be represented in the scene.  NOTE: we have a 1:1 relationship with a parent region and it's spatial
//            // regionNode representation.  That is why we can recurse backward through the parent Entities 
//            // ack - but consider a moon orbiting a planet?  
//            // ack2 - what about things being carried by a player that end up straddling a portal?
//            // ack3 - what about a tank barrel that is part of an overall tank, and so doesnt have a fixed bound "region" because when the turret 
//            //       swivels that bounds changes.  This is part of why the spatial graph is / should be independant of the parentEntity childEntity relationship.
//            //       right?  But the tank turret and tracks and body are 3 sub-entities of a "tank" entity that itself has no bounds.  And those three parts
//            //       are siblings and child of that entity container object.  Spatially an overall regionNode could be created to represent the overall
//            //       entity and it will have 3 entitynode's underneath it.
//            // answer?  I think maybe the answer is based on which "region" something is in.  An entity attached as a child (e.g. sword in player's hand)
//            //          is ultimately recursed upward til we find the region it's in.  Then in the spatailgraph it's added as a child to the RegionNode
//            //          that hosts that Region.  However while recursing backwards, we accumulate the matrices of every parent along the way.

//            // TODO: for the very short term, we wont worry about the above until we get multiple zone's loaded, a planet or so in and an empty ship.
//            //       with all bounding boxes, lazy dirty updates, and movement across zones and such working.
//            //
//        }


//        public void UnloadEntity(string id)
//        {
//            EntityBase ent = (EntityBase) Repository.Get(id);
//            UnloadEntity(ent);
//        }

//        public void UnloadEntity(EntityBase entity)
//        {
//            //Trace.WriteLine("Unload Entity " + entity.ID);
//            if (entity.Parent != null && entity.Parent.SceneNode != null && entity.SceneNode != null)
//                entity.Parent.SceneNode.RemoveChild(entity.SceneNode);

//            if (entity.SceneNode != null)
//            {
//                entity.SceneNode.RemoveChildren();
//                entity.SceneNode = null;
//            }

//            if (entity.Parent != null)
//                entity.Parent.RemoveChild(entity);

//            // TODO: usubscribe from any providers and arrays or it will never fall out of scope
//        }

//        // Conceptually delete is meant to be done during Edit mode for making changes to the scene that then need to be saved.
//        // It's not intended to be used during gameplay when an entity is killed and needs to be removed from the game at runtime!
//        // Frankly a ICommand should be used to do this and that ICOmmnad should then call the RemoveEntity to deal with the common tasks
//        // of both Removal and EditDeletion
//        public void Delete(EntityBase entity)
//        {
//            UnloadEntity(entity);
//            FileManager.WriteNewNode(entity.Parent);
//            // parent node gets rewritten this time with the former child now removed

//            // TODO: I dont see why we are not recursing here especially now that we've added a SceneNode to which
//            // i think we should write a traverser for unloading them all and handling any ref counts.
//            // the same could be done frankly for the "AddChild() and then our code for ref inc/dec is all contained


//            //if (CurrentTarget == p)
//            //    CurrentTarget = null;

//            // remove from any Zone

//            // remove from entity list

//            // notify simulation
//            _entityDestroyed.Invoke(entity);
//        }

//        // remove signifies actually removing the entity from the simulation completely including the game database.
//        // Disabling is what we do when we merely want to kill an entity in game.  However, we might want to actually remove IF 
//        // a spawn generator is creating these temporary entities that are never intended to remain in the world however those perhaps
//        // we should never call FileManager.WriteNewNode on in the first place?
//        public void DisableEntity(EntityBase ent)
//        {
//        }

//        public void Add(IFXProvider p)
//        {
//            int semanticID = (int) p.Semantic;
//            if (_fxProviders[semanticID] != null)
//                throw new ArgumentException(
//                    "Scene.Add (IFXProvider) - Provider at this semantic slot already occupited.  It must first be removed.");

//            if (p.Semantic == FX_SEMANTICS.FX_NONE)
//                throw new ArgumentException("Scene.Add (IFXProvider) - Provider Semantic not set.  Cannot add Provider.");
//            // TODO: not sure how to swap though because we do want to register the proper scene elements with 
//            // the new semantic.  We might need a traversal and the individual elements might need a sort of bitflag
//            // indicating which semantics they should be registered too.
//            // also some elements like lights i think might not need these... though actually maybe they do because
//            // consider a pointlight may not necessarily be used as a shadowmap volume.  So we would need the semantics bitflag.
//            _fxProviders[(int) p.Semantic] = p;
//        }

//        #endregion

//        // Attempts to find a region which contains the position.
//        public Region FindSector(Vector3d position)
//        {
//            return _spatialGraph.FindRegion(position);
//        }


//        //// cull requests can take place from camera positions that are different than our main position so find the sector for the camera's position
//        //public Stack<Node> CullRequest(Camera camera)
//        //{
//        //    //Culler tmpCuller = new Culler();
//        //    //tmpCuller.Clear(camera);

//        //    //_spatialGraph.FindCameraRegion(camera.Position).Traverse(tmpCuller);
//        //    //return tmpCuller.NodeStack;

//        //    Stack<Node> result = _spatialGraph.CullFrom(camera);
//        //    return result;
//        //}

//        public void BeginUpdate(Vector3d position)
//        {
//        }

//        public void EndUpdate(object callback)
//        {
//        }

//        public void Update(float elapsed)
//        {
//            _pager.Update(); // TODO: pager must have viewports register/unregister so it can make proper determination

           
//            // TODO: having elapsed for anything except the simulation seems wrong because
//            //       if we dont render (minimized) but are still running the simulation, then
//            //        the elapsed since it's shared with _simulation's will be too small.  we'd need to track seperate elapses right?
//            //        but really, i shouldnt have to track elapsed.  Anything that should need elapsed should be updated via _simulation right?
//            //        Well from the looks of things, elapsed is only needed for the Editor camera and for some FX.Update() (e.g. updating waves )
//            //        and can quite possibly just use a seperate elapsed counter that always resets when suspended so there's never any "jump"

//            foreach (Viewport vp in Core._CoreClient.Viewports.Values)
//                Update(vp, elapsed);
//        }

//        // Update() is called by AppMain in the main loop.  Since eventually we want _pager.Update to just be called once and not per vp
//        // we should just have a seperate Update() that only get's called once and can do _pager.Update() and then
//        // which will also call the following Update(vp, elapsed) per viewport as needed
//        private void Update(Viewport vp, float elapsed)
//        {
//            Core._CoreClient.Engine.SetViewport(vp.TVViewport);
//            Core._CoreClient.Engine.SetCamera(vp.Camera.TVCamera);
//            vp.Camera.Update(elapsed);

//            // primarily culls the graph // TODO: note that paging currently breaks if multiple viewports are used!!!
//            // TODO: also note that update doesnt need to occur for every viewport....
//            Update(elapsed, vp);
//            // copy the visible list from the traverser
//            // note: any shadow map culling has been done too at this point?


//            //if (DepthofField) DoF.DrawDepthOfField();

//            RenderBeforeClear(vp.Camera);
//            vp.Context.Clear();
//            Clear();

//            //ps.Render(); // seems if you dont render this last it gets hiden behind everything
//            //  _waterFall.Update();
//            //  _waterFall.Render();

//            // DebugDraw.DrawAxisIndicator(new Vector3d( 0,0,0), null, 0);
//            //_occlusionFrustum.Render();

//            //if (DepthofField) DoF.UpdateDepthOfField();

//            //foreach (EDGE[] seg in TerrainGridBuilder._segments)
//            //{
//            //    DebugDraw.DrawLines(seg, CONST_TV_COLORKEY.TV_COLORKEY_RED);
//            //}
//            Render(vp);
//            DebugDraw.Clear();
//        }


//        private void Update(double elapsed, Viewport vp)
//        {
//            //_visible = _spatialGraph.CullFrom(camera);
//            //_visible = vp.Context.Culler;
//            vp.Context.Culler(); // culls and updates debug text
//            vp.Context.Update();
//            // get and output the culling statistics
//            //UpdateDebugText(camera);

//            // update all the FX Managers running in the scene
//            foreach (IFXProvider fx in _fxProviders)
//                if (fx != null) fx.Update((int) elapsed, vp.Camera);
//        }

//        private void RenderBeforeClear(Camera camera)
//        {
//            // TODO: do I need HACK because when water and such modifies the camera, the subsequent calls to CulLDrawer.Clear() would then start using 
//            // that incorrect position to Translate models??
//            foreach (IFXProvider fx in _fxProviders)
//                if (fx != null) fx.PreRender(camera);
//        }

//        private void Clear()
//        {
//            Core._CoreClient.Engine.Clear();
//        }

//        // RenderScene() is the method for drawing the visible items.
//        // It's also the method that is invoked directly by FX's such as reflecting Water
//        // to render the reflection.  As such, we have to test 
//        // TODO: not meant to be a "public" method but for right now during init in Engine.CS i need to refer to this
//        // method for the callback method for some of the FX
//        public void RenderScene(Viewport vp)
//        {
//            // we only render sky here
//            foreach (IFXProvider fx in _fxProviders)
//                if (fx != null && fx.Semantic == FX_SEMANTICS.FX_SKY)
//                    fx.Render(vp.Camera);


//            // draw the scene // TODO: We need to use a culler that is specific to the camera since this method gets invoked by FX doing things such as reflection
//            ((Draw) _drawer).Render(vp.Camera, vp.Context.VisibleNodes);

//            // draw those scene elements that should be drawn after the rest of the scene has been drawn
//            foreach (IFXProvider fx in _fxProviders)
//            {
//                // Some FX's "Render()" method (e.g Water) must only be rendered in the final scene render.
//                // Is there a better way for detecting these types?  In effect, any "reflection" FX cant do this.
//                // but that's why Water's dont override the fx.Render() method.
//                if (fx != null && fx.Semantic != FX_SEMANTICS.FX_SKY && fx.Semantic != FX_SEMANTICS.FX_WATER_LAKE &&
//                    fx.Semantic != FX_SEMANTICS.FX_WATER_OCEAN)
//                    fx.Render(vp.Camera);
//            }
//        }

//        private void Render(Viewport vp)
//        {
//            RenderScene(vp);

//            // render post FX such as Bloom
//            foreach (IFXProvider fx in _fxProviders)
//            {
//                // NOTE: because Bloom uses the 2d fullscreen quad draw command it must be the very last
//                // 2d operation or it will get wiped out.  Water is also rendered here because the actual water meshes are only rendered once
//                if (fx != null) fx.PostRender(vp.Camera);
//            }

//            DebugDraw.CommitDisplayList();
//            DebugDraw.CommitTextList();

//            // interestingly, the way AccurateTimeElapsed works is, it takes the time between frameRenderN - frameRenderN-1
//            // this means the following debug line = 0 first time because there is no frameRenderN or frameRenderN-1, 
//            // however the second time around, the value is filled.  This must be because when the engine is initialized
//            // the frameRenderN = Now 
//            // Anyways, this isnt more accurate than me calling Environement.GetTickCOunt() just once at the same place everytime.
//            // The difference is, TV's version is immune from changing every subsequent call between frames.
//            // 
//            //Trace.WriteLine(string.Format("Accurate time elapsed == {0}", Engine.Core.Engine.AccurateTimeElapsed()));
//            Core._CoreClient.Engine.RenderToScreen();
//        }

//        #region IDisposable Members
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        // pass true if managed resources should be disposed. Otherwise false.
//        private void Dispose(bool disposing)
//        {
//            if (!IsDisposed)
//            {
//                if (disposing)
//                {
//                    DisposeManagedResources();
//                    _disposed = true;
//                }
//                DisposeUnmanagedResources();
//            }
//        }

//        protected virtual void DisposeManagedResources()
//        {
//            // call to unload removes all regions\graphs\entities
//            Unload();

//            Array.Clear(_fxProviders, 0, _fxProviders.Length);
//            // TODO: Should these be apart of scene then or apart of our core engine?

//            // now destroy the rest of the scene manager objets
//            _drawer = null;
//            _picker = null;
//            //_players = null;
//            //_staticEntities = null;
//        }

//        protected virtual void DisposeUnmanagedResources()
//        {
//        }

//        protected void CheckDisposed()
//        {
//            if (IsDisposed) throw new ObjectDisposedException("scene is disposed.");
//        }

//        protected bool IsDisposed
//        {
//            get { return _disposed; }
//        }
//        #endregion
//    }
//}