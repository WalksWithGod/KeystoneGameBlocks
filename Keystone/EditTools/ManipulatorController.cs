//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using Keystone.Appearance;
//using Keystone.Cameras;
//using Keystone.Controls;
//using Keystone.EditOperations;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Events;
//using Keystone.Resource;
//using Keystone.Types;
//using MTV3D65;
//using System.Collections.Generic;

//namespace Keystone.Controllers
//{
//    // TODO: why have the tool and the manipulator control seperate?  Why not
//    // have the various move tools inhert from a base that has all of these?
//    // I'm just not sure the point of a seperate manipulator....
//    // UPDATE:  Actually now looking at UIController I see it's because 
//    // the idea was that the controller is a GUI controller and could be seperate
//    // from any "tool".  So you could have an elevator button in your game and have
//    // a UIController assigned to it when the button has input capture.
//    //
//    // Thus the role of the "tool" is to manage the insertion of this gui control and 
//    // it's controller into the scene.  I think the only thng I should change is move
//    // the control itself into the Tool and then to dynamically associate the different
//    // controllers to that GUI based on move/translation/rotation 
//    /// <summary>
//    /// ManipulatorController is a UIController that hosts event handlers for any associated GUI entity controls.
//    /// So it's two tasks are event handling and (for now at least) loading the Control and wiring up those events.
//    /// </summary>
//    public class ManipulatorController : UIController
//    {
//        // TODO: This should be read from config
//        public readonly float SCREENSPACE_CONSTANT = .1f; // 10% of the dimensions x,y of screen shoudl our widget be 

//        protected Entity _target;
//        // April.26.2011 - the entire concept of a disconnected anchor is incompatible with our
//        // entity design which states only one Entity parent can exist per Entity.  For an Anchor
//        // to work, there must be a time where the widget is attached to both the anchor and the target
//        // because to remove it from one first without adding it to the other
//        // would decrease the ref count to 0 and remove it from repository.
//        //protected Keystone.Portals.Region mAnchor; // a disconnected anchor node that is never added to the scene but which keeps a node anchored to the scene so it's never completely dereferenced
//        protected Viewport mCurrentViewport;

//        protected VectorSpace mVectorSpace;
//        protected AxisFlags mSelectedAxes;
//        protected TransformationMode mActiveMode;
//        protected delegate void ManipFunction();

//        protected System.Drawing.Point mouseDelta, mouseStart, mouseEnd;
//        //protected System.Drawing.Point mLastMousePosition;
//        protected ManipFunctions mManipFunctions; 
        
//        // TODO: i think snapping is maybe not too useful.  sketchup doesnt do it
//        //private bool _enableHorizontalSnapping = true;
//        //private int _horizontalSnapValue = 10; // 10 units it will snap
//        //private int _horizontalSnapTolerance = 3; // when dragging within 3 units of snapvalue, it will snap rounding up or down

//        /// <summary>
//        /// Defines the current axes on which a manipulator is operating
//        /// </summary>
//        [Flags]
//        public enum AxisFlags : int
//        {
//            None = 0,

//            X = (0x1 << 0),
//            Y = (0x1 << 1),
//            Z = (0x1 << 2),

//            XY = X | Y,
//            YX = Y | X,
//            XZ = X | Z,
//            ZX = Z | X,
//            YZ = Y | Z,
//            ZY = Z | Y,

//            XYZ = X | Y | Z,

//            All = XYZ
//        }

//        /// <summary>
//        /// Defines the directions of an axis
//        /// </summary>
//        [Flags]
//        public enum AxisDirections
//        {
//            Positive = (0x1 << 0),      // Positive direction of an axis
//            Negative = (0x1 << 1),      // Negative direction of an axis

//            All = Positive | Negative   // Both positive and negative directions
//        }

//        /// <summary>
//        /// Defines the transformation mode of a manipulator
//        /// </summary>
//        [Flags]
//        public enum TransformationMode
//        {
//            None,                               // No manipulation mode

//            TranslationAxis = (0x1 << 0),        // Manipulating the position of an object along an axis
//            TranslationPlane = (0x1 << 1),       // Manipulating the position of an object along a plane
//            Rotation = (0x1 << 2),               // Manipulating the orientation of an object around an axis
//            ScaleAxis = (0x1 << 3),              // Manipulating the scale of an object on an axis
//            ScalePlane = (0x1 << 4),             // Manipulating the scale of an object on a plane (two-axes)
//            ScaleUniform = (0x1 << 5)            // Manipulating the scale of an object uniformly
//        }

//        /// <summary>
//        /// Defines the vector space in which a manipulator is operating
//        /// </summary>
//        public enum VectorSpace
//        {
//            World,              // Manipulating with world space basis vectors
//            Local               // Manipulating with local space basis vectors
//        }


//        protected class ManipFunctions : Dictionary<TransformationMode, Dictionary<AxisFlags, ManipFunction>>
//        {
//            public ManipFunctions()
//            {
//                foreach (TransformationMode mode in System.Enum.GetValues(typeof(TransformationMode)))
//                    this[mode] = new Dictionary<AxisFlags, ManipFunction>();
//            }
//        }

//        // TODO: this should never LoadVisuals on it's own.
//        // Instead, the Tools should be created and then wire themselves up
//        // to these 
//        public ManipulatorController()
//        {
//            mManipFunctions = new ManipFunctions();
//            CongigurePositioningManipulator();
//            ConfigureScalingManipulator();
//            ConfigureRotatingManipulator();


//            LoadVisuals();
//            WireEvents();
//        }
        
//        // TODO: the main reason to have a Tool.cs encapsulating a Control
//        // is that the single tool can handle all events of a complex control.
//        // It can wire each child control's events to be handled
//        // to methods in a single place where the entire complex control is treated
//        // as a single control.  That is the function of the Tool.  
//        // The question is, can we not just do that with the root
//        // control of a complex control? 
//        private void WireEvents()
//        {
//            // TODO: i should move the origin mesh into an actual sub-Control and not just as a mesh directly added to Children array
//            //((Control)_manipulator.Children[0])  // origin mesh.  Meshes aren't Controls obviously.  The origin entity is actually the manipulator itself
//            //((Control)_manipulator).MouseEnter += OnMouseEnter;
//            //((Control)_manipulator).MouseLeave += OnMouseLeave;
//            //((Control)_manipulator).MouseDown += OnMouseDown;
//            //((Control)_manipulator).MouseUp += OnMouseUp;
//            //((Control)_manipulator).MouseClick += OnMouseClick;
//            //((Control)_manipulator).MouseDrag += OnMouseDrag;
//            // POSITION
//            //////////////////////////////////////////////////////////////////////////////////////
//            // x entity arrow
//            ((Control)_control.Children[0]).MouseEnter += OnMouseEnter;
//            ((Control)_control.Children[0]).MouseLeave += OnMouseLeave;
//            ((Control)_control.Children[0]).MouseDown += entityXPosition_OnMouseEnter;
//            ((Control)_control.Children[0]).MouseUp += OnMouseUp;
//            ((Control)_control.Children[0]).MouseClick += OnMouseClick;
//            ((Control)_control.Children[0]).MouseDrag += OnMouseDrag;

//            // y entity arrow
//            ((Control)_control.Children[1]).MouseEnter += OnMouseEnter;
//            ((Control)_control.Children[1]).MouseLeave += OnMouseLeave;
//            ((Control)_control.Children[1]).MouseDown += entityYPosition_OnMouseEnter;
//            ((Control)_control.Children[1]).MouseUp += OnMouseUp;
//            ((Control)_control.Children[1]).MouseClick += OnMouseClick;
//            ((Control)_control.Children[1]).MouseDrag += OnMouseDrag;

//            // z entity arrow
//            ((Control)_control.Children[2]).MouseEnter += OnMouseEnter;
//            ((Control)_control.Children[2]).MouseLeave += OnMouseLeave;
//            ((Control)_control.Children[2]).MouseDown += entityZPosition_OnMouseEnter;
//            ((Control)_control.Children[2]).MouseUp += OnMouseUp;
//            ((Control)_control.Children[2]).MouseClick += OnMouseClick;
//            ((Control)_control.Children[2]).MouseDrag += OnMouseDrag;

//            for (int i = 3; i < _control.ChildCount; i++)
//            {
//                ((Control)_control.Children[i]).MouseEnter += OnMouseEnter;
//                ((Control)_control.Children[i]).MouseLeave += OnMouseLeave;
//                ((Control)_control.Children[i]).MouseUp += OnMouseUp;
//                ((Control)_control.Children[i]).MouseClick += OnMouseClick;
//                ((Control)_control.Children[i]).MouseDrag += OnMouseDrag;
//            }
            

//            // SCALE
//            //////////////////////////////////////////////////////////////////////////////////////
//            ((Control)_control.Children[6]).MouseDown += entityXScale_OnMouseEnter;
//            ((Control)_control.Children[7]).MouseDown += entityYScale_OnMouseEnter;
//            ((Control)_control.Children[8]).MouseDown += entityZScale_OnMouseEnter;


//            // ROTATION
//            //////////////////////////////////////////////////////////////////////////////////////
//            ((Control)_control.Children[12]).MouseDown += entityXRotate_OnMouseEnter;
//            ((Control)_control.Children[13]).MouseDown += entityYRotate_OnMouseEnter;
//            ((Control)_control.Children[14]).MouseDown += entityZRotate_OnMouseEnter;
//        }

//        public void ShowMove()
//        {
//            // Position
//            ((ModeledEntity)_control.Children[0]).Enable = true;  // position arrowx
//            ((ModeledEntity)_control.Children[1]).Enable = true;  // Y 
//            ((ModeledEntity)_control.Children[2]).Enable = true;  // Z
//            ((ModeledEntity)_control.Children[3]).Enable = true;  // axis line X
//            ((ModeledEntity)_control.Children[4]).Enable = true;  // Y
//            ((ModeledEntity)_control.Children[5]).Enable = true;  // Z

//            // Scale
//            ((ModeledEntity)_control.Children[6]).Enable = false;  // scale tab
//            ((ModeledEntity)_control.Children[7]).Enable = false;
//            ((ModeledEntity)_control.Children[8]).Enable = false;
//            ((ModeledEntity)_control.Children[9]).Enable = false;  // axis line
//            ((ModeledEntity)_control.Children[10]).Enable = false;
//            ((ModeledEntity)_control.Children[11]).Enable = false;

//            // Rotation
//            ((ModeledEntity)_control.Children[12]).Enable = false;  // circle axis
//            ((ModeledEntity)_control.Children[13]).Enable = false;
//            ((ModeledEntity)_control.Children[14]).Enable = false;

//        }

//        public void ShowScale()
//        {
//            // Position
//            ((ModeledEntity)_control.Children[0]).Enable = false;  // position arrow
//            ((ModeledEntity)_control.Children[1]).Enable = false;
//            ((ModeledEntity)_control.Children[2]).Enable = false;
//            ((ModeledEntity)_control.Children[3]).Enable = false;  // axis line
//            ((ModeledEntity)_control.Children[4]).Enable = false;
//            ((ModeledEntity)_control.Children[5]).Enable = false;

//            // Scale
//            ((ModeledEntity)_control.Children[6]).Enable = true;  // scale tab
//            ((ModeledEntity)_control.Children[7]).Enable = true;
//            ((ModeledEntity)_control.Children[8]).Enable = true;
//            ((ModeledEntity)_control.Children[9]).Enable = true;  // axis line
//            ((ModeledEntity)_control.Children[10]).Enable = true;
//            ((ModeledEntity)_control.Children[11]).Enable = true;

//            // Rotation
//            ((ModeledEntity)_control.Children[12]).Enable = false;  // circle axis
//            ((ModeledEntity)_control.Children[13]).Enable = false;
//            ((ModeledEntity)_control.Children[14]).Enable = false;
//        }

//        public void ShowRotate()
//        {
//            mActiveMode = TransformationMode.Rotation;
//            // Position
//            ((ModeledEntity)_control.Children[0]).Enable = false;  // position arrow
//            ((ModeledEntity)_control.Children[1]).Enable = false;
//            ((ModeledEntity)_control.Children[2]).Enable = false;
//            ((ModeledEntity)_control.Children[3]).Enable = false;  // axis line
//            ((ModeledEntity)_control.Children[4]).Enable = false;
//            ((ModeledEntity)_control.Children[5]).Enable = false;

//            // Scale
//            ((ModeledEntity)_control.Children[6]).Enable = false;  // scale tab
//            ((ModeledEntity)_control.Children[7]).Enable = false;
//            ((ModeledEntity)_control.Children[8]).Enable = false;
//            ((ModeledEntity)_control.Children[9]).Enable = false;  // axis line
//            ((ModeledEntity)_control.Children[10]).Enable = false;
//            ((ModeledEntity)_control.Children[11]).Enable = false;

//            // Rotation
//            ((ModeledEntity)_control.Children[12]).Enable = true;  // circle axis
//            ((ModeledEntity)_control.Children[13]).Enable = true;
//            ((ModeledEntity)_control.Children[14]).Enable = true;
//        }

//        // an editor needs to load and unload gizmo for editing
//        private void LoadVisuals()
//        {
//            _control = EditTools.Widgets.LoadWidget();

//            // increment the ref count to keep it from falling out of scope.
//            // NOTE: We only ever have to increment just the top most entity 
//            // not each child.
//            Repository.IncrementRef(null, _control);
//            // NOTE: here we only set this flag on the root Control, not the children.
//            // the children entities will automatically gain their parents scaling.
//            _control.UseFixedScreenSpaceSize = true;
//            _control.ScreenSpaceSize = .2f;
//        }

        
//        /// <summary>
//        /// Adds the edit gizmo control to the scene so that it's visible.
//        /// </summary>
//        /// <param name="scene"></param>
//        public virtual void Activate(Scene.ClientScene scene)
//        {

//            _control.Enable = true;

//            // note: manipulator is removed from it's parent which decrements ref count.  That's why
//            // the manipulator's ref count is artificially raised +1 on creation so it doesnt get disposed by the Repository
//            if (_target != null )
//            {
//                // when translating an entity, we're actually litterally moving it so we can handle
//                // boundary checking so the actual translation amount 
//                _control.Translation = _target.Translation;

//                // add to the target's parent //TODO: should be add to target's same region
//                _target.Parent.AddChild(_control);

                
//                //if (_control.Parent != null)

//                //    _control.MoveTo(_target.Parent);
//                //else
//                //{
//                //    _target.Parent.AddChild(_control);
//                //}
//            }
//            else // target is null, remove the manipulator
//            {
               
//            }
//        }

//        /// <summary>
//        /// Removes the manipulator gizmo control from the scene so it no longer renders
//        /// But because we set the refcount artificially one higher, it will not go out of scope 
//        /// upon being removed from the scene.
//        /// </summary>
//        public virtual void Deactivate()
//        {
//            _control.Enable = false;                    

//            if (_control.Parent != null && _target != null &&  _control.Parent == _target.Parent)
//            {
//                _control.Parent.RemoveChild(_control);
//                // reverse the translation to put the control back at origin
//                _control.Translation -= _control.Translation;
//            }

//            _target = null;
//        }

//        public bool IsActive 
//        { 
//            get
//            {
//                if (_target == null)
//                {
//                    Trace.Assert(_control.Enable == false);
//                }
//                return (_target != null);
//            } 
//        }

//        public bool HasInputCapture
//        {
//            get
//            {
//                if (_activeChildControl == null) return false;
//                return _activeChildControl.HasInputCapture;
//            }
//        }

//        public Entity Target
//        {
//            get { return _target; }
//            set
//            {
//                _target = value;
//                _control.Enable = _target != null;
//            }
//        }

//        #region EventHandlers
//        protected void entityXPosition_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.TranslationAxis ;
//            mSelectedAxes = AxisFlags.X;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        protected void entityYPosition_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.TranslationAxis;
//            mSelectedAxes = AxisFlags.Y;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        protected void entityZPosition_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.TranslationAxis;
//            mSelectedAxes = AxisFlags.Z;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        // scale
//        protected void entityXScale_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.ScaleAxis  ;
//            mSelectedAxes = AxisFlags.X;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        protected void entityYScale_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.ScaleAxis;
//            mSelectedAxes = AxisFlags.Y;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        protected void entityZScale_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.ScaleAxis;
//            mSelectedAxes = AxisFlags.Z;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }

//        // rotate
//        protected void entityXRotate_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.Rotation;
//            mSelectedAxes = AxisFlags.X;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }
//        protected void entityYRotate_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.Rotation;
//            mSelectedAxes = AxisFlags.Y;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }
//        protected void entityZRotate_OnMouseEnter(object sender, EventArgs args)
//        {
//            mActiveMode = TransformationMode.Rotation;
//            mSelectedAxes = AxisFlags.Z;

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;
//        }
        

//        protected virtual void OnMouseEnter(object sender, EventArgs args)
//        {
//            // set the transform/axis mode

//            // TODO: here we might do something like change the material color of the control to it's RollOver state color
//         //   Trace.WriteLine("Controller.OnMouseEnter - " + ((EntityBase)sender).ID);

//            // change the material to roll over

//        }

//        protected virtual void OnMouseLeave(object sender, EventArgs args)
//        {
//            //Trace.WriteLine("Controller.OnMouseLeave - " + ((EntityBase)sender).ID);

//            // change the material's back to default
//        }


//        protected virtual void OnMouseDown(object sender, EventArgs args)
//        {
//            Trace.WriteLine("Controller.OnMouseDown - " + ((Entity)sender).ID);

//            mCurrentViewport = ((MouseEventArgs)args).Viewport;
//           // mouseDelta = new System.Drawing.Point(mLastMousePosition.X - ((MouseEventArgs)args).ViewportRelativePosition.X, mLastMousePosition.Y - ((MouseEventArgs)args).ViewportRelativePosition.Y);

//            Debug.WriteLine("Mouse delta = " + mouseDelta.ToString());

//            // swap the end with last start, and new start with current
//            mouseStart = mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;

//            // if no change, return after we've updated the mouseStart and mouseEnd
//            if (mouseDelta.X == 0 && mouseDelta.Y == 0) return;


//            //((ModeledEntity)_control.Children[1]).Enable = false;
//            //((ModeledEntity)_control.Children[2]).Enable = false;
//            //((ModeledEntity)_control.Children[3]).Enable = false;

//        }

//        protected virtual void OnMouseUp(object sender, EventArgs args)
//        {
//            Trace.WriteLine("Controller.OnMouseUp - " + ((Entity)sender).ID);
                           
//            // TODO: uncomment after switch to ICommand2

                        
//            //MoveOp op = new RotateOp(_target, _targetStartPosition, _target.Translation);
//            //Core._Core.CommandProcessor.Execute(op);

//            _activeChildControl = null;

//            //RotateOp op = new RotateOp(_target, _targetStartPosition, _target.Translation);
//            //Core._Core.CommandProcessor.Execute(op);

//            _activeChildControl = null;
//            //((ModeledEntity)_control.Children[0]).Enable = true;
//           // ((ModeledEntity)_control.Children[1]).Enable = true;
//           // ((ModeledEntity)_control.Children[2]).Enable = true;
        
//        }

//        protected virtual void OnMouseClick(object sender, EventArgs args)
//        {
//            Trace.WriteLine("Controller.OnMouseClick - " + ((Entity)sender).ID);
//        }

//        // TODO: this works well for orthogonal views, but i dont think for perspective.
//        protected virtual void OnMouseDrag(object sender, EventArgs args)
//        {
//            //Trace.WriteLine("Controller.OnMouseDrag - " + ((EntityBase)sender).ID);
//            mCurrentViewport = ((MouseEventArgs)args).Viewport;
//            mouseDelta = new System.Drawing.Point(mouseEnd.X - ((MouseEventArgs)args).ViewportRelativePosition.X, mouseEnd.Y - ((MouseEventArgs)args).ViewportRelativePosition.Y);
//            mouseStart = mouseEnd;
//            mouseEnd = ((MouseEventArgs)args).ViewportRelativePosition;

//            mManipFunctions[mActiveMode][mSelectedAxes]();
//        }
//        #endregion

//        // TODO: this should/could be static method within ManipulatorControl 
//        // which inherits Control.cs  and is a type of Control that is for translation/scale/rotation
//        // the host control.
//        #region Delegates Configuration
//        private void CongigurePositioningManipulator()
//        {

//            mManipFunctions[TransformationMode.TranslationPlane][AxisFlags.X | AxisFlags.Y]
//            = mManipFunctions[TransformationMode.TranslationPlane][AxisFlags.X | AxisFlags.Z]
//            = mManipFunctions[TransformationMode.TranslationPlane][AxisFlags.Y | AxisFlags.Z]
//                = delegate()
//                {
//                    // get the plane representing the two selected axes
//                    Plane p = GetPlane(mSelectedAxes);

//                    double hitDistanceStart, hitDistanceEnd;
//                    Ray start_ray, end_ray;

//                    // cast rays into the scene from the mouse start and end points and
//                    // intersect the pick rays with the dual axis plane we want to move along

//                    // if either of the intersections is invalid then bail out as it would
//                    // be impossible to calculate the difference
//                    if (!mCurrentViewport.GetStartEndPlanePickRays(mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//                        return;

//                    // obtain the intersection points of each ray with the dual axis plane
//                    Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//                    Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

//                    // calculate the difference between the intersection points
//                    Vector3d difference = end_pos - start_pos;

//                    // TODO: commenting out the frustum test for now
//                    //// obtain the current view frustum using the camera's view and projection matrices
//                    //BoundingFrustum frust = new BoundingFrustum(ViewMatrix * ProjectionMatrix);

//                    //// if the new translation is within the current camera frustum, then add the difference
//                    //// to the current transformation's translation component
//                    //if (frust.Contains(mTransform.Translation + diff) == ContainmentType.Contains)
//                    _target.Translation += difference;
//                    _control.Translation = _target.Translation;
//                };

//            mManipFunctions[TransformationMode.TranslationAxis][AxisFlags.X]
//                = mManipFunctions[TransformationMode.TranslationAxis][AxisFlags.Y]
//                = mManipFunctions[TransformationMode.TranslationAxis][AxisFlags.Z]
//                = delegate()
//                {
//                    if (_target == null) return;
//                    bool useTV = true;

//                    // get the unit version of the seclected axis
//                    // NOTE: The unit axis accrues values from muliple axis if applicable such as when
//                    // moving by both X and Z plane at same time or any other plane.
//                    Vector3d axis = GetUnitAxis(mSelectedAxes);

//                    // we need to project using the translation component of the current
//                    // ITransform in order to obtain a projected unit axis originating
//                    // from the transform's position

//                    Vector3d start_position;
//                    Vector3d end_position;
//                    Vector3d screen_direction;
//                    Vector3d difference;


//                    // TODO: it would be nice to pass in things like the axis, start/end mouse positions, viewport, 

//                    if (!useTV)
//                    {
//                        Matrix translation = Matrix.Translation(_target.Translation - mCurrentViewport.Context.Position);
//                        translation = Matrix.Identity();

//                        // project the origin onto the screen at the transform's position
//                        start_position = mCurrentViewport.Project(new Vector3d(), translation);
//                        // project the unit axis onto the screen at the transform's position
//                        end_position = mCurrentViewport.Project(axis, translation);
//                        start_position.z = 0;
//                        end_position.z = 0;

//                        // calculate the normalized direction vector of the unit axis in screen space
//                        screen_direction = Vector3d.Normalize(end_position - start_position);

//                        // calculate the projected mouse delta along the screen direction vector
//                        end_position = start_position + (screen_direction * (Vector3d.DotProduct(new Vector3d(mouseDelta.X, mouseDelta.Y, 0), screen_direction)));

//                        //difference = end_position - start_position;

//                        // unproject the screen points back into world space using the translation transform
//                        // to get the world space start and end positions in regard to the mouse delta along
//                        // the mouse direction vector
//                        start_position = mCurrentViewport.UnProject(start_position.x, start_position.y, start_position.z, translation);
//                        end_position = mCurrentViewport.UnProject(end_position.x, end_position.y, end_position.z, translation);
//                    }
//                    else// use tv's versions of Project/Unproject because mine are faulty
//                    {
//                        // project the origin onto the screen at the transform's position
//                        start_position = mCurrentViewport.Project(new Vector3d() + _target.Translation - mCurrentViewport.Context.Position);
//                        // project the unit axis onto the screen at the transform's position
//                        end_position = mCurrentViewport.Project(axis + _target.Translation - mCurrentViewport.Context.Position);
//                        start_position.z = 0;
//                        end_position.z = 0;

//                        // calculate the normalized direction vector of the unit axis in screen space
//                        screen_direction = Vector3d.Normalize(end_position - start_position);
//                        // calculate the projected mouse delta along the screen direction vector
//                        end_position = start_position +
//                            (screen_direction * (Vector3d.DotProduct(new Vector3d(mouseDelta.X, mouseDelta.Y, 0), screen_direction)));

//                        //difference = end_position - start_position;

//                        double desiredDistance = (_target.Translation - mCurrentViewport.Context.Position).Length;

//                        start_position = mCurrentViewport.UnProject(start_position.x, start_position.y, desiredDistance);
//                        end_position = mCurrentViewport.UnProject(end_position.x, end_position.y, desiredDistance);
//                    }

//                    // calculate the difference vector between the world space start and end points
//                    difference = end_position - start_position;

//                    _target.Translation += difference;
//                    _control.Translation = _target.Translation;
//                };
//        }

//        // http://www.youtube.com/watch?v=QI-7vSFB9j4
//        private void ConfigureRotatingManipulator()
//        {
//            mManipFunctions[TransformationMode.Rotation][AxisFlags.X]
//                = mManipFunctions[TransformationMode.Rotation][AxisFlags.Y]
//                = mManipFunctions[TransformationMode.Rotation][AxisFlags.Z]
//                = delegate()
//                {
//                    // get the plane perpendicular to the rotation axis, transformed by
//                    // the current world matrix (or in our case camera space matrix)
//                    Vector3d distanceToTarget = _target.Translation - mCurrentViewport.Context.Position;
//                    Plane p = GetPlane(mSelectedAxes);

//                    double hitDistanceStart, hitDistanceEnd;
//                    Ray start_ray, end_ray;
//                    if (!mCurrentViewport.GetStartEndPlanePickRays(mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//                        return;

//                    // calculate the intersection position of each ray on the plane
//                    Vector3d start_position = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//                    Vector3d end_position = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

//                    // get the direction vectors of the rotation origin to the start and end points
//                    Vector3d origin_to_start = Vector3d.Normalize(start_position - distanceToTarget);
//                    Vector3d origin_to_end = Vector3d.Normalize(end_position - distanceToTarget);

//                    Vector3d rotation_axis = GetUnitAxis(mSelectedAxes);

//                    // calculate cross products of the direction vectors with the rotation axis
//                    Vector3d rot_cross_start = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_start));
//                    Vector3d rot_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_end));

//                    // calculate the cross product of the above start and end cross products
//                    Vector3d start_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rot_cross_start, rot_cross_end));

//                    // dot the two direction vectors and get the arccos of the dot product to get
//                    // the angle between them, then multiply it by the sign of the dot product
//                    // of the derived cross product calculated above to obtain the direction
//                    // by which we should rotate with the angle
//                    double dot = Vector3d.DotProduct(origin_to_start, origin_to_end);
//                    double rotation_angle = Math.Acos(dot)
//                        * Math.Sign(Vector3d.DotProduct(rotation_axis, start_cross_end));

//                    // create a normalized quaternion representing the rotation from the start to end points
//                    Quaternion rot = Quaternion.Normalize(new Quaternion(rotation_axis, rotation_angle));

//                    if (double.IsNaN(rot.X) || double.IsNaN(rot.Y) || double.IsNaN(rot.Z) || double.IsNaN(rot.W))
//                        return;

//                    _target.Rotation = rot * _target.Rotation;

//                    if (mVectorSpace == VectorSpace.Local)
//                        _control.Rotation = _target.Rotation;
//                };
//        }

//        private void ConfigureScalingManipulator()
//        {
//            // all single-axis scaling will use the same manip function
//            mManipFunctions[TransformationMode.ScaleAxis][AxisFlags.X]
//                = mManipFunctions[TransformationMode.ScaleAxis][AxisFlags.Y]
//                = mManipFunctions[TransformationMode.ScaleAxis][AxisFlags.Z]
//                    = delegate()
//                    {
//                        if (_target == null) return;

//                        // get the axis for the component being scaled
//                        Vector3d axis = GetUnitAxis(mSelectedAxes);

//                        // get a translation matrix on which the projection of the above axis
//                        // will be based
//                        Matrix translation = Matrix.Translation(_target.Translation - mCurrentViewport.Context.Position);
//                        translation = Matrix.Identity();

//                        // project the axis into screen space
//                        Vector3d p0 = mCurrentViewport.Project(new Vector3d(), translation);
//                        Vector3d p1 = mCurrentViewport.Project(axis, translation);

//                        // disregard the z component for 2D calculations
//                        p0.z = p1.z = 0;

//                        // Vector3d versions of the mouse input positions
//                        Vector3d ps = new Vector3d(mouseStart.X, mouseStart.Y, 0);
//                        Vector3d pe = new Vector3d(mouseEnd.X, mouseEnd.Y, 0);

//                        // calculate the axis vector and vectors from the translation point
//                        // to each of the mouse positions
//                        Vector3d v0 = p1 - p0;
//                        Vector3d vs = ps - p0;
//                        Vector3d ve = pe - p0;

//                        // project both mouse positions onto the axis vector and calculate
//                        // their scalars
//                        double proj_s = Math.Abs(Vector3d.DotProduct(vs, v0) / v0.Length);
//                        double proj_e = Math.Abs(Vector3d.DotProduct(ve, v0) / v0.Length);

//                        // find the ratio between the projected scalar values
//                        Vector3d scale = _target.Scale;
//                        double ratio = (proj_e / proj_s);

//                        // scale the appropriate axis by the ratio
//                        switch (mSelectedAxes)
//                        {
//                            case AxisFlags.X:
//                                scale.x *= ratio;
//                                break;

//                            case AxisFlags.Y:
//                                scale.y *= ratio;
//                                break;

//                            case AxisFlags.Z:
//                                scale.z *= ratio;
//                                break;
//                        }

//                        // clamp each component of the new scale to a sane value
//                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
//                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
//                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

//                        // scale the transform
//                        _target.Scale = scale;
//                    };


//            // all dual-axis scaling will use the same manip function
//            mManipFunctions[TransformationMode.ScalePlane][AxisFlags.X | AxisFlags.Y]
//                = mManipFunctions[TransformationMode.ScalePlane][AxisFlags.X | AxisFlags.Z]
//                = mManipFunctions[TransformationMode.ScalePlane][AxisFlags.Y | AxisFlags.Z]
//                    = delegate()
//                    {
//                        // get the plane that corresponds to the axes on which we are performing the scale
//                        Plane p = GetPlane(mSelectedAxes);


//                        double hitDistanceStart, hitDistanceEnd;
//                        Ray start_ray, end_ray;
//                        if (!mCurrentViewport.GetStartEndPlanePickRays(mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//                            return;

//                        // calculate the intersection points of each ray along the plane
//                        Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//                        Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

//                        // find the vectors from the transform's position to each intersection point
//                        Vector3d start_to_pos = start_pos - _target.Translation;
//                        Vector3d end_to_pos = end_pos - _target.Translation;



//                        // get the lengths of both of these vectors and find the ratio between them
//                        double start_len = start_to_pos.Length;
//                        double end_len = end_to_pos.Length;

//                        Vector3d scale = _target.Scale;
//                        double ratio = (start_len == 0)
//                            ? (1)
//                            : (end_len / start_len);

//                        // scale the selected components by the ratio
//                        if ((mSelectedAxes & AxisFlags.X) == AxisFlags.X)
//                            scale.x *= ratio;
//                        if ((mSelectedAxes & AxisFlags.Y) == AxisFlags.Y)
//                            scale.y *= ratio;
//                        if ((mSelectedAxes & AxisFlags.Z) == AxisFlags.Z)
//                            scale.z *= ratio;

//                        // clamp each component of the new scale to a sane value
//                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
//                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
//                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

//                        // scale the transform
//                        _target.Scale = scale;
//                    };

//            // Uniform scaling
//            mManipFunctions[TransformationMode.ScaleUniform][AxisFlags.X | AxisFlags.Y | AxisFlags.Z]
//                    = delegate()
//                    {
//                        // get the direction of the transformation's position to the camera position
//                        Vector3d pos_to_cam = mCurrentViewport.Context.Position - _target.Translation;
//                        //= mCurrentViewport.Context.Camera.Inverse).Translation - _target.Translation;

//                        // normalize the direction for use in plane construction
//                        if (pos_to_cam.x != 0 || pos_to_cam.y != 0 || pos_to_cam.z != 0)
//                            pos_to_cam.Normalize();

//                        // create a plane with the normal calculated above that passes through
//                        // the transform's position
//                        Plane p = new Plane(pos_to_cam, 0);
//                        p.Transform(Matrix.Translation(_target.Translation));


//                        double hitDistanceStart, hitDistanceEnd;
//                        Ray start_ray, end_ray;
//                        if (!mCurrentViewport.GetStartEndPlanePickRays(mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//                            return;

//                        // calculate the intersection points of each ray along the plane
//                        Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//                        Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

//                        // find the vectors from the transform's position to each intersection point
//                        Vector3d start_to_pos = start_pos - _target.Translation;
//                        Vector3d end_to_pos = end_pos - _target.Translation;




//                        // get the lengths of both of these vectors and find the ratio between them
//                        double start_len = start_to_pos.Length;
//                        double end_len = end_to_pos.Length;

//                        Vector3d scale = _target.Scale;
//                        double ratio = (start_len == 0)
//                            ? (1)
//                            : (end_len / start_len);

//                        // multiply the scale uniformly by the ratio of the start and end vector lengths
//                        scale *= ratio;

//                        // clamp each component of the new scale to a sane value
//                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
//                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
//                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

//                        // scale the transform
//                        _target.Scale = scale;
//                    };
//        }
//        #endregion 

//        /// <summary>
//        /// Utility function that returns the origin plane whose normal is perpendicular to the 
//        /// vectors of the specified axes if multiple axes are specified, or the plane
//        /// whose normal is the unit vector of the specified axis if a single axis is specified
//        /// </summary>
//        /// <param name="axis">The axes for which to retrieve the corresponding plane</param>
//        /// <returns>The origin plane that corresponds to the specified axes</returns>
//        protected Plane GetPlane(AxisFlags axis)
//        {
//            Vector3d normal = new Vector3d ();

//            switch (axis)
//            {
//                case AxisFlags.X:
//                case AxisFlags.Y | AxisFlags.Z:
//                    normal = new Vector3d (1,0,0);
//                    break;

//                case AxisFlags.Y:
//                case AxisFlags.X | AxisFlags.Z:
//                    normal= new Vector3d (0,1,0);
//                    break;

//                case AxisFlags.Z:
//                case AxisFlags.X | AxisFlags.Y:
//                    normal= new Vector3d (0,0,1);
//                    break;
//            }

//            if (_target == null)
//                return new Plane (new Vector3d (), normal);

//            Vector3d translation = _target.Translation - mCurrentViewport.Context.Position;
//            Plane p = new Plane(new Vector3d(), normal);
//            if (mVectorSpace == VectorSpace.Local)
//                p.Transform(new Matrix(_target.Rotation) * Matrix.Translation(translation));
//            else
//                p.Transform(Matrix.Translation(translation));

//            return p;
//        }


//        /// <summary>
//        /// Utility function that returns the unit axis in Vector3 format that corresponds to the 
//        /// specified axes, oriented based on the vector space of the manipulator
//        /// </summary>
//        /// <param name="axis">The axes for which to retrieve the corresponding unit axis</param>
//        /// <returns>The unit axis that corresponds to the specified axes</returns>
//        protected Vector3d GetUnitAxis(AxisFlags axes)
//        {
//            Vector3d unit;
//            unit.x = 0;
//            unit.y = 0;
//            unit.z = 0;

//            // note these are NOT if / else blocks.  Execution falls through and each successive flag can
//            // potentially be true when multiple axis are ORd together
//            if ((axes & AxisFlags.X) == AxisFlags.X)
//                unit.x += 1;
//            if ((axes & AxisFlags.Y) == AxisFlags.Y)
//                unit.y += 1;
//            if ((axes & AxisFlags.Z) == AxisFlags.Z)
//                unit.z += 1;

//            if (unit.x != 0 || unit.y != 0 || unit.z != 0)
//                unit.Normalize();

//            // in local vector space, rotate the axis with the transform's
//            // rotation component, otherwise return the axis in its default
//            // form for world vector space
//            unit = ((mVectorSpace == VectorSpace.Local)
//                        && (_target  != null))
//                ? (Vector3d.TransformNormal(unit, _target.Rotation))
//                : (unit);

//            return unit;
//        }




//        #region IDisposable Members
//        private bool _disposed = false;
//        ~ManipulatorController()
//        {
//            // Do not re-create Dispose clean-up code here.
//            // Calling Dispose(false) here is optimal in terms of
//            // readability and maintainability.
//            Dispose(false);
//        }

//        public override void Dispose()
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
//            // TODO: manipulator should not be allowed in the treeview at all
//            // remove the artificial ref count we add to the control to keep it alive (prevent Repository from deleting it)
//            if (_control != null)
//                Repository.DecrementRef(null, _control);

//        }

//        protected virtual void DisposeUnmanagedResources()
//        {
//        }

//        protected void CheckDisposed()
//        {
//            if (IsDisposed) throw new ObjectDisposedException(this.GetType ().ToString () + " is disposed.");
//        }

//        protected bool IsDisposed
//        {
//            get { return _disposed; }
//        }
//        #endregion
//    }
//}
