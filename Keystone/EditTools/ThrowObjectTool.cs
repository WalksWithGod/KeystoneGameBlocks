//using System;
//using Keystone.Cameras;
//using Keystone.Collision;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Events;
//using Keystone.Types;

//namespace Keystone.EditTools
//{
//    // TODO: replace the ThrowObjectTool with simply the drop object tool, allow us to set the entity to drop
//    // and to set the velocity.
//   // ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.ThrowObjectTool(0.0);
//    // crap, once we've imported this into a Zip and now want to load the mesh or actor
//    // into the scene along with those textures, we have to TextureFactory.ForceTextureLoading = true;
//    // and then grab the names off the groups.  Sylvain says this works even if no textures are ever loaded.

//    // TODO: ok, so now we've got a resource (say a mesh or actor)in the archive and we want
//    // to drag and drop it into the scene at the viewport position...
//    // This is no longer importing but rather creating a new modeled entity with the specified geometry and
//    // 
//    // as far as insides of ships go, the camera must be in the same frame where you're editing.
//    // Thus to edit a ship its useful to be in deckplan mode which forces the context to be on just the interior
//    // http://www.youtube.com/watch?v=Az6X0Vi9hH4
//    // in gamebryo i like how the object is loaded, then you can click to set it down.
//    // Well for space games i think a good idea is to wait for the object to be loaded, then when loaded
//    // change the cursor to a x for "no good" or "green" for good and allow the user to click once to set that entity
//    // and even copies of that entity.  Since the entity will have been loaded, and if there's no "surface" to set it down upon
//    // then we'll place the model at a distance where it takes up maybe 25% of the screenspace so it's always fully visible.
//    // So placing a giant saturn will place it on each "click".
//    // This is a good way to do it rather than just popping it into the world!  So simple.

//    // This class is a Tool, not a Keystone.Command and the model or entity that we are going to "throw" 
//    // into the world must already have been loaded prior to initializing this Tool.  
//    public class ThrowObjectTool : Tool
//    {
//        private bool mShowCustomCursor;
//        private Vector3d _velocity;
//        private const float _speed = 1000;
//        // and if it's a model, then we create a new entity for it first. 
//        // otherwise if it's an entity, we keep that entity.
//        // Then once we have an entity we .Clone() it when we throw it to place a unique one.
//        private Entity mLastResult;
//        private Scene.ClientScene mScene;
//        private Keystone.Scene.SceneInfo _info;

//        System.IO.Stream stream;
//        Keystone.IO.XMLDatabase xmldatabase;

//        public ThrowObjectTool (Scene.ClientScene scene, string archivePath, string prefabPathInArchive)
//        {
//            if (scene == null) throw new ArgumentNullException();
//            mScene = scene;
//            stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromArchive(prefabPathInArchive, "", archivePath);
//            xmldatabase = new Keystone.IO.XMLDatabase();
//            _info = xmldatabase.Open(stream);

//            // TODO: i think a default Root should be added so we can temporarily show
//            // the entity with high alpha on all it's materials
//            LoadNewEntity();

            
//        }

//        ~ThrowObjectTool()
//        {
//            if (mLastResult != null)
//            {
//                // the last of our mLastResults is always unconnected to the scene
//                // so to properly remove it, we should increment/decrement so any children
//                // unconnected will also get removed
//                Resource.Repository.IncrementRef(null, mLastResult);
//                Resource.Repository.DecrementRef(null, mLastResult);
//            }
//            xmldatabase.Dispose();
//            stream.Close();
//            stream.Dispose();
//            _info = null;
//        }

//        /// <summary>
//        /// NOTE: If the prefab is an entity instead of a model, then rather 
//        /// than "clone" the entity each time we want to place a new instance
//        /// using newEntity = oldEntity.Clone()
//        /// all we really have to do is just re-read the xml for it.  That is fast enough
//        /// operation and the slow operation of loading any underlying actor/mesh geometry
//        /// is done just once and we re-use those copies in the Repository automatically.
//        /// </summary>
//        private void LoadNewEntity()
//        {
            
//            // NOTE: clone entities arg == true in ReadSychronous() call
//            // This partricular xmldatabase has only one model or one entity in it so name and parent name can be ""
//            Keystone.Elements.Node node = xmldatabase.ReadSynchronous(_info.FirstNodeTypeName,"", "", true, true, false);
            
//            if (node is Entity)
//            {
//                // this entity is already cloned for us during serialization in the xmldatabase.ReadSynchronous() call
//                mLastResult = (Entity)node;
//            }
//            else
//            {
//                throw new Exception("Node not of valid Entity type.");
//            }
//            // we want to create a copy of the Appearance but with transparency
//            // We do not alter the existing, we instead make a copy that has transparency
//            // this way the instances we place wont be transparent but the new one to be
//            // placed will.
//            //MakeTranparent();

//            double boundingRadius = mLastResult.BoundingBox.Radius;
          
//            this.Entity = mLastResult;
//        }

//        public override void HandleEvent(EventType type, EventArgs args)
//        {
//            MouseEventArgs mouseArgs = (MouseEventArgs)args;
//            PickResults pickResult = (PickResults)(mouseArgs).Data;
            
//            _viewport = mouseArgs.Viewport;
//            if (_viewport == null) return;

//            RenderingContext currentContext = _viewport.Context;
//            Entity parentEntity = currentContext.Region;
//            if (parentEntity == null) return;

//            double targetRadius = 0; 
            
//            try 
//            { 
//                targetRadius = mLastResult.BoundingBox.Radius;
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.Assert (true, "need to track down why sometimes the bounding box is screwed...");
//            }

//            if (targetRadius == float.MaxValue * .5f)
//                targetRadius = 1; // the boundingbox is not initialized yet
//                                  // so far this seems to occur with .obj's that are loaded.
//            else if (targetRadius > _viewport.Context.Camera.Far)
//                targetRadius = _viewport.Context.Camera.Far * .5f;

//            Vector3d scale = currentContext.LookAt * currentContext.GetZoomToFitDistance(targetRadius);
//            // TODO: now we must also limit this position to any bounds of the region
//            // that the item is being placed so we do need information about the "drop" point
//           // System.Diagnostics.Debug.Assert();
//            // TODO: One way we could achieve that is similar to with the CellPainter where we only
//            // even allow the CellPainter tool to function if the parent entity is CelledRegion
//            // in the pickResults arg.  Otherwise we exit early.  We can do the same with 
//            // the throwobjecttool if we make sure that a Region parent is set in the PickResult only if
//            // the mouse is in bounds.
//            switch (type)
//            {
//                // this mousedown can(?) handles editablemesh geometry moving.  Currently there's code
//                // for a manipulator to translate an Entity
//                case EventType.MouseDown:
//                    if (mouseArgs.Button == Enums.MOUSE_BUTTONS.XAXIS ) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
//                    {
//                        _viewport = mouseArgs.Viewport;
//                        if (CurrentViewport == null)
//                        {
//                            System.Diagnostics.Debug.WriteLine("No viewport selected");
//                            return;
//                        }

                        
//                       // _velocity = Vector3d.Normalize(currentContext.LookAt - currentContext.Position) * _speed;
                        
//                        try
//                        {
                            
//                            // TODO: computing the translation needs to depend on several factors
//                            // 1) if in a celled region, then we assume placing either
//                            //    on floor, on wall (like decoration or window or door)
//                            //    or ontop of another entity (eg. table)
//                            // 2) if on a terrained region, then we track terrain and so
//                            //    we dont have to compute  adistance to place the entity in view
//                            //    we only have to place the entity's x,z at the terrain point
//                            //    that the mouse picks against
//                            // 3) if in open space, a region with no terrain or celledregion
//                            //    THEN we want to place the entity on the x/z plane but totally 
//                            //    in view
                            
//                            mLastResult.Translation = currentContext.Position + scale;
//                            parentEntity.AddChild(mLastResult);
//                            System.Diagnostics.Debug.WriteLine("ThrowObjectTool - Item Placed.");
//                            // save starting at parent since the parent now has a new child
//                         //   Core._Core.SceneManager.ActiveSimulation.Scene.XMLDB.Write(mLastResult.Parent, true, AddNodeCompleted);

//                            // prepare a new entity after having inserted the last one in the scene
//                            LoadNewEntity();
//                        }
//                        catch (Exception ex)
//                        {
//                            // if the command execute fails we still want to end the input capture
//                            System.Diagnostics.Debug.WriteLine("ThrowObjectTool - Error -" + ex.Message);

//                        }
//                        finally
//                        {
//                            // TODO: This i believe is probably false, we don't
//                            // need to stop input capture do we?
//                            //_hasInputCapture = false;
//                            //DeActivate();
                            
//                        }
//                    }
//                    break;
//                case EventType.MouseEnter :
//                    System.Diagnostics.Debug.WriteLine("ThrowObjectTool Mouse Enter...");
                    
//                    // from here can we introduce a mesh into the scene and have it render?
//                    // I dont see why not...  all we have to do is NOT save the scene
//                    // after doing it

//                    // and then on Leave we remove the mesh
//                    //
//                    // but i know when we first start this tool, it's only after our target
//                    // mesh has been loaded.
//                    break;
//                case EventType.MouseMove:
//                    if (parentEntity is Portals.CelledRegion)
//                    {
//                        // TODO: parentEntity should be selected from the PickResult just as i do
//                        // with CelledRegion!  that will solve the below problem
//                        // --
//                        // problem here is parentEntity is reference from currentContext.Region
//                        // which is basically the region where the camera is and that's not necessary
//                        // what we want.  We want the region that is the focus for editing
//                        // not the one the camera currently resides in.


//                        // 1) Or maybe that's exactly what i should do, put the camera into the
//                        // context of that Interior region!
//                        //  - I'm liking this option more than option 2
//                        //  - The rules we have for a camera moving within a region
//                        //    needs to be different when in the Zone's vs an interior CelledRegion
//                        //    right?  If we're set to be in an interior and in edit, we should
//                        //    limit the position of our isometric camera as well as it's distance
//                        //    from it's target
//                        //      - in fact, what i should have is a camera mode that forces
//                        //      only an interior view centric and if no interiors available
//                        //      it's grayed out or something
//                        // 2) OR don't rely on CurrentContext.Region and instead pass that 
//                        // CelledRegion in along with any fitler for the 
//                        // current "floor" to edit and along with a check
//                        // to prevent that floor value from possibly being lower or higher
//                        // than the max or min floor in the assigned CelledRegion

//                        // for now rather than a mesh, lets just draw a single verticle line
//                        // that snaps to the grid point and when we've gone from one to the next
//                        // we'll add a wall segment there

//                        // maybe we should use a seperate tool entirely to place
//                        // walls and floors and floorplan components?
//                    }
//                    // are we in orthographic view?
//                    // switch (_viewport.Context.ProjectionType)
//                    // {
//                    //      case ProjectionType.Orthographic:
//                    //          // is this a deck plan?
//                    //          // is the mouse down?
//                    //          // are we defining a rubber band area to drop
//                    //          // deck segments?
//                    // }
//                    //
                    
//                    // - i think the drop tool should first wait for the object to be loaded
//                    //   and I think it technically is, our drop cursor doesnt appear until then
//                    //   So how do we do this?
//                    // 
//                    // - This tool is created by the Editor
//                    //
//                    // checking the settings of the CoordinteGrid, we can determine if we've 
//                    // moved
//                    //_viewport.Context.Grid.HighlightCell (x,z); 
//                    // mLastGrid // a Point that is in int's to determine the current grid cell
//                    //           // the point CAN contain negative values because this is a coorinate grid
//                    //           // with 0,0 as origin
//                    //           // The center of each grid cell is easy to compute
//                    //           // 
//                    // 
//                    //pickResult.ImpactPoint
//                    //System.Diagnostics.Debug.WriteLine("ThrowObjectTool Mouse Move...");
//                    break;

//                case EventType.MouseUp:
//                    System.Diagnostics.Debug.WriteLine("ThrowObjectTool.HandleEvent() - Mouse Up.");
//                    break;
//                case EventType.MouseLeave :  // TODO: verify this event occurs when we switch tools
//                    System.Diagnostics.Debug.WriteLine("ThrowObjectTool.HandleEvent() - Mouse Leave.");
//                    break;

//                default:
//                    break;
//            }
//        }

//        private void AddNodeCompleted(Amib.Threading.IWorkItemResult result)
//        {
//            // the result.State object is always set as the Command object that was used.
//            // from that Command object's type, we can determine what members we want to check.
//            // For instance ImportEntityBase has a member Entity (change to Entities[] ?) and so
//            // all imported entities will appear in that list
//            if (result.Exception == null)
//            {
//                System.Diagnostics.Trace.WriteLine("Command completed successfully.", result.State.GetType().Name);

//                return; // todo; no physics for now, we're just adding entities to the scene

//                ////EntityBase parent = ((Commands.ImportEntityBase)result.State).Parent;
//                //// we don't want or need to write a thrower entity we've imported

//                //// set up the physics for this entity
//                //mLastResult.PhysicsBody = new JigLibX.Physics.Body();
//                //mLastResult.PhysicsBody.Immovable = false;
//                //mLastResult.PhysicsBody.Mass = 1;
//                //mLastResult.PhysicsBody.CollisionSkin = new JigLibX.Collision.CollisionSkin(mLastResult.PhysicsBody);
//                //float x = (float)((Commands.ImportStaticEntity)result.State).Position.x;
//                //float y = (float)((Commands.ImportStaticEntity)result.State).Position.y;
//                //float z = (float)((Commands.ImportStaticEntity)result.State).Position.z;

//                //mLastResult.PhysicsBody.SetPosition(x, y, z);

//                //mLastResult.PhysicsBody.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Sphere(0, 0.5f, 0, 1), new JigLibX.Collision.MaterialProperties(0.1f, 0.5f, 0.5f));
//                //mLastResult.PhysicsBody.SetVelocity((float)_velocity.x, (float)_velocity.y, (float)_velocity.z);
//                //mLastResult.PhysicsBody.EnableBody();
//                //// wire up collision events?
//                ////mLastResult.PhysicsBody.CollisionSkin.callbackFn
//            }
//            else
//                System.Diagnostics.Trace.WriteLine("Command failed.", result.State.GetType().Name);

//        }
//    }
//}
