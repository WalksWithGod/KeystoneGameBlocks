using System;
using KeyCommon.Traversal;
using Keystone.Cameras;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.Events;
using Keystone.Types;

namespace Keystone.EditTools
{
    public enum PrimitiveType
    {
        Vertex,
        Edge,
        Face
    }
    public struct PrimitiveInfo
    {
        public uint ID;
        public PrimitiveType Type;
        public Vector3d Center;
        public Vector3d[] OriginalPosition;
        public Vector3d[] Position;
    }

    /// <summary>
    /// UPDATE: A tool is merely a collection of eventhandlers for a complex group of controls.
    /// We may keep Move/Scale/Rotate as intrinsic tools to keystone, but most tools should be
    /// implemented by the User in the EXE app side of things and not in keystone.dll.  
    /// 
    /// UIController which ManipulatorController inherits is also irrelevant.
    /// 
    /// OLD:
    /// A tool is an object that essentially takes ownership of mouse and keyboard input
    /// when the tool is activated. Unlike a "Control" however, a Tool acts like a
    /// strategy pattern set in the overall Controller.cs to change how input is handled
    /// for the current viewport.  For instance a deckplan segment "paint" tool that
    /// paints as long as the mouse is down versus a component place tool that drops once on
    /// each mouse down.
    /// </summary>
    public abstract class Tool : ITool, IDisposable
    {
        protected Keystone.Network.NetworkClientBase mNetClient;

        protected Controls.Control mControl;
        protected Keystone.Events.IInputCapture mActiveControl;
        protected Entities.Entity mSource;

        protected bool mIsActive;
        protected bool mHasInputCapture;
        public Vector3d MousePosition3D;
        protected System.Drawing.Point mMouseStart;
        protected System.Drawing.Point mMouseEnd;
        protected PickResults mPickResult;
        protected PickResults mStartPickResults;

        protected EditDataStructures.EditableMesh _mesh;
        protected Cameras.Viewport _viewport;

        protected bool _disposed = false;

              
        public Tool (Keystone.Network.NetworkClientBase netClient)
        {
            if (netClient == null) throw new ArgumentNullException();
            mNetClient = netClient;
        }
        
        
        public Cameras.Viewport CurrentViewport
        { 
        	get { return _viewport; } 
        	set { _viewport = value; } 
        }

        public Entity Source 
        { 
        	get { return mSource; } 
        	set { mSource = value; } 
        }

        /// <summary>
        /// HUD Visual
        /// </summary>
 		public Controls.Control Control  
        { 
        	get { return mControl; } 
        }
 
        //public EditDataStructures.EditableMesh Target { get { return _mesh; } set { _mesh = value; } }
        
        public PickResults PickResult 
        { 
        	get { return mPickResult; } 
        }

        public virtual PickResults Pick(Viewport vp, int viewportRelativeX, int viewportRelativeY)
        {
        	// default pick parameters are from RenderingContext
        	return Pick (vp, viewportRelativeX, viewportRelativeY, vp.Context.PickParameters);             	
        }
        
        /// <summary>
        /// Picking which is viewport specific.
        /// </summary>
        /// <param name="vp"></param>
        /// <param name="viewportRelativeX"></param>
        /// <param name="viewportRelativeY"></param>
        /// <returns></returns>
        public virtual PickResults Pick(Viewport vp, int viewportRelativeX, int viewportRelativeY, PickParameters parameters)
        {
            PickResults result = new PickResults();
            if (vp != null)
            {

                try
                {
                    parameters.MouseX = viewportRelativeX;
                    parameters.MouseY = viewportRelativeY;
                    result = vp.Context.Pick(viewportRelativeX, viewportRelativeY, parameters);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Tool.Pick() - Pick exception." + ex.Message);
                }

                if (result != null && result.HasCollided)
                {
                    // TODO: the below is all for verifying i think face level picking of a TVMesh 
                    // I think this code could be moved into TVMesh and potentially TVActor and then this info
                    // plugged right intot he PickResults struct
                    //if (results[0].Result.bHasCollided == false) return null;
                    //ModelBase model = ((ModeledEntity)results[0].Entity).Model;
                    //Mesh3d mesh = (Mesh3d)model.Geometry;
                    //TVMesh tvmesh = Core._CoreClient.Globals.GetMeshFromID( mesh.TVIndex);

                    //// get the triangle
                    //int index = results[0].Result.iFaceindex;
                    //int v1 = 0;
                    //int v2 = 0;
                    //int v3 = 0;
                    //int group = 0;
                    //tvmesh.GetTriangleInfo(index, ref v1, ref v2, ref v3, ref group);
                    //float x1 = 0;
                    //float y1 = 0;
                    //float z1 = 0;
                    //float x2 = 0;
                    //float y2 = 0;
                    //float z2 = 0;
                    //float x3 = 0;
                    //float y3 = 0;
                    //float z3 = 0;
                    //float dummy = 0;
                    //int intdummy = 0;
                    //tvmesh.GetVertex(v1, ref x1, ref y1, ref z1, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref intdummy);
                    //tvmesh.GetVertex(v2, ref x2, ref y2, ref z2, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref intdummy);
                    //tvmesh.GetVertex(v3, ref x3, ref y3, ref z3, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref dummy, ref intdummy);
                    //Triangle tri = new Triangle(new Vector3d(x1, y1, z1), new Vector3d(x2, y2, z2), new Vector3d(x3, y3, z3));
                    //double u, v, w;
                    //// i think the problem here in part is our triangle points are in model space and need to be in world space

                    //Vector3d collisionPoint = new Vector3d(results[0].Result.vCollisionImpact);
                    //Utilities.MathHelper.BarycentricCoordinate(tri, collisionPoint , out u, out v, out w);
                }
            }
            return result;
        }


        public virtual void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouse = args as MouseEventArgs;
            if (mouse == null) return; // KeyboardEventArgs ?

            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(mouse.Viewport, mouse.ViewportRelativePosition.X, mouse.ViewportRelativePosition.Y);


            switch (type)
            {
                // this mousedown currently only handles editablemesh geometry moving.  Currently there's code
                // in the PositioningManip to translate an Entity
                case EventType.MouseDown:
                    // TODO: should we filter left button here? or in EditController?
                    if (mPickResult.HasCollided && mouse.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS)
                    {
                        //if (mPickResult.CollidedObjectType == CollidedObjectType.Geometry)
                        //{
                        //    throw new Exception();
                        //}
                        //else  // it's a regular entity, so we just show the move widget
                        {
                            if (mIsActive)
                            {
                                // gizmo is already activated, so test if we've hit a manipulator control box to initiate Positioning
                                if (mPickResult.Entity is Keystone.Events.IInputCapture)
                                {
                                    // TODO: Here i need to create a new type of args
                                    //       that includes the specific sub-model that was clicked
                                    //       so that the Control can have more specific knowledge about
                                    //       what part of it was clicked and can respond appropriately
                                    Keystone.Events.IInputCapture ctrl = (Keystone.Events.IInputCapture)mPickResult.Entity;
                                    ctrl.HandleEvent(type, new InputCaptureEventArgs(mouse, mPickResult)); // this control is not the root Control most likely however it will call
                                    // the event handler inside PositioningManip.cs and there we can
                                    // set _manipulator.HasInputCapture and test for that and
                                    mActiveControl = ctrl;

                                    mHasInputCapture = true;
                                }
                            }
                            else
                            {
                                // TODO: prevent changing of the target entity by user if flag is set
                                Activate(mouse.Viewport.Context.Scene, mPickResult.Entity);
                            }
                        }
                    }
                    else  // pickResult.HasCollided = false
                    {
                        if (mPickResult == null || mPickResult.Entity == null || mPickResult.Entity != mSource)
                        {
                            // TODO: prevent de-activation by user
                            DeActivate();
                            return;
                        }
                    }
                    // System.Diagnostics.Trace.WriteLine("MoveTool Mouse Down...");
                    break;

                case EventType.MouseMove:

                    // TODO: i have to get the movement down properly before i can think about plotting points and adding them....
                    //          in sketchup i noticed that edges of 3d objects can only move perpendicular to their alignment which helps
                    //          individual vertices can move on the 2 axis parallel with the screen.
                    //  And while this moving is occuring, the alignment snapping and guide lines are drawn...  this will be tricky.  Clearly
                    //  This occurs dynamically and should be computed by the MoveTool itself... that means the moveTool must query
                    //  the Mesh to determine what other edges/verts/faces to possibly align to and snap to.  Same goes for edge
                    // halfway points 
                    // TODO: and when are the debug.draw's done for that?  Currently I use the Scene.MouseOverItem which is an EditableMesh
                    // of course and i draw the edgeID.   I could compute the lines and send those as a drawlist in Scene so that
                    // when the scene.render occurs any lines added can be drawn.  But maybe it's better to have the mesh itself render any
                    // face/edge/vertex differently based on cached last pick results
                    //System.Diagnostics.Trace.WriteLine("MoveTool Mouse Move...");
                    Vector3d mouseProjectedCoords = (Vector3d)((MouseEventArgs)args).UnprojectedCameraSpacePosition;

                    if (mPickResult.CollidedObjectType == PickAccuracy.EditableGeometry)
                    {
                        throw new Exception();
                    }
                    else // regular entity and NOT an editablemesh
                    {
                        // if the actual manipulator's pickable Control (one of it's parts) has inputCapture, we pass the event straight throught to it
                        if (mHasInputCapture)
                        {
                            mActiveControl.HandleEvent(EventType.MouseMove, new InputCaptureEventArgs((MouseEventArgs)args, mPickResult));
                            // if the manipulator still has capture return, otherwise it's just been deactivated (e.g. MouseUp event occurred)
                            if (mHasInputCapture) return;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    // compute final positions for all and create a command that will be able to undo \redo the action 
                    // i could move the primitive back to it's original position and then compute a cumulative translation
                    // and use that in TMove() and undo/redo is easy... 
                    // is there a way to create the move 
                    // TODO: the primitive's values are in world coords and need to be in local.  That's why
                    // translating was preferred.  Now if there is no scale, we could translate but i think a better idea is to 
                    // do all the editing in model space... that just requires converting the mouse pick to model space
                    // by removing the Entity's translation (assuming scaling is never allowed on EditableMesh :((

                    if (mIsActive)
                    {
                        // if the control has input capture, then we've just completed a move operation?  
                        if (mHasInputCapture)
                        {
                            mActiveControl.HandleEvent(type, new InputCaptureEventArgs(mouse, mPickResult));
                            // verify after the active control processes the event, input capture in the manipulator is relinquished
                            // but we won't de-activate because we do want the move widget still visible

                            mHasInputCapture = false;


                        }
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Adds the widget control hosted by the Tool as a child to the source Entity
        /// that was selected in the scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        protected virtual void Activate(Scene.Scene scene, Entities.Entity entity)
        {
            System.Diagnostics.Debug.Assert(entity.Scene == scene);

            // make sure that we can't activate the widget on itself
            if (entity == mControl) return;
            if (entity == null) throw new Exception();

            mSource = entity;

            mIsActive = true;
            mControl.Enable = true;
        }

        protected virtual void DeActivate()
        {
            //mSource = null; // IMPORTANT: We do NOT null mSource here!  That must only occur on dispose
            mIsActive = false;

            if (mControl != null) // placement tool doesn't use a control widget
                mControl.Enable = false;
        }

        #region IDisposable Members
        ~Tool()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
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
            if (mControl != null)
                Resource.Repository.DecrementRef(mControl);

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
