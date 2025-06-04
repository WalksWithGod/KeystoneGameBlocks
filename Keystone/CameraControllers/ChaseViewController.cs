//using System;
//using Keystone.Entities;
//using Keystone.Vehicles;
//using Keystone.Types;
//using MTV3D65;
//using System.Drawing;
//
//namespace Keystone.Cameras
//{
//    /// <summary>
//    /// ChaseViewController and OrbitViewController.  
//    /// There is no conceptual difference between the two.  A chase is typically
//    /// only limited by horizontal and vertical rotation angles.  An Orbit camera has no
//    /// limits and allows continuous 360 rotation on all axis.
//    /// </summary>
//    public class ChaseViewController : ViewpointController 
//    {
//        protected float _verticleMaxAngle;
//        protected float _verticleMinAngle;
//        protected float _angleX;
//        protected float _angleY;
//        private double _cameraLag; // 1.0 = instant updating 0.0 no updating.  Otherwise the camera will adjust to its 
//        // new position as a ratio of the movementAmount = totalDistance to travel / cameraLag
//        // of course if the player is moving non stop, then with any kind of lag the camera will eventually fall
//        // behind the player.  So the camera needs to be able to accelerate to catch up.
//        // the camera's acceleration is tricky to come up with a good value though...
//        // Like if hte player rotates, the camera must rotate faster (it has further distance to travel) or it wont keep up.
//        // i mean, i could increase the acceleration the further the camera falls behind.. :/
//
//        private float mVerticleAngle;
//        private float mHorizontalAngle;
//
//        // offsets to the camera
//        private float _maxRadius = 5000;
//        private float _minRadius = 300; // zoom can be used to travel between min and max
//        private float _currentRadius;
//        private double _polarRadius; //distance from the target
//        private double _dampening; // verticle dampening for when the camera is following the player up and down bumps and hills
//        // we dont want the camera being effected by every single pothole and divot.
//
//        // The target should be set explicitly on this ChaseViewController!  Or Viewpoint (not Viewport or Context)
//        // not on the Context.  I just finished making that change but have yet to fix it here
//        public Vector3d Target {get; set;} // Target must be set explicitly. 
//                                           // will not automatically update
//
//        public ChaseViewController(RenderingContext context)
//            : base(context)
//        {
//                    //  consider that 0 degrees is a position about the center of our actor directly in front of it.  
//            //  going counterclockwise, 90 degrees would be directly beneath it looking up
//            // 180 degrees is directly behind it
//            // 270 degrees is directly above it looking down
//            _verticleMaxAngle = 360; // higher lets us look closer to straight down.  
//            _verticleMinAngle = 0; // lower lets us look closer to straight up
//            _angleX = _verticleMinAngle; // init to min angle
//        }
//
//        public float MaxRadius { get { return _maxRadius; } set { _maxRadius = value; } }
//        public float MinRadius { get { return _minRadius; } set { _minRadius = value; } }
//
//        public override void HandleMouseLook(Point delta)
//        {
//
//
//            //    // the  y rotation of the player has to be tied to the mouse movement, else the controls will feel unresponsive
//            //    // the x rotation is for camera elevation. It will be constrained by min/max angles and also in Update by collision
//            //    if (p.MouseLook)
//            //    {
//            // TODO: we must first grab the entity's rotation directly so that we can sychronize with any rotation that was done outside of here, such as arrow keys
//            // TODO:  but im commenting out for now because of error with the player's rotation assigned by input never getting passed
//            //           along to the physics engine and thus in Simulation.Update() when that rotationmatrix is grabbed, it wipes out the rotation assigned with input
//            //horizontalAngle = (float)p.Rotation.y; // sychronize mouselook rotation with keyboard rotation
//            // TODO: actually what we should do to sychronize mouse and arrow key rotation is to have arrow keys simply
//            // call HandleKeyboardLook(); and have it also update horiztonalAngle
//            mHorizontalAngle += delta.X / 2.0f; // mouse x axis movement controls horizontal rotation about world Y axis
//            mVerticleAngle += delta.Y / 2.0f; // mouse y axis movement controls verticle rotation about world X axis
//
//            //keep the angle values between 0 and 360
//            if (mVerticleAngle < 0) mVerticleAngle += 360;
//            if (mVerticleAngle > 0) mVerticleAngle %= 360;
//            if (mHorizontalAngle < 0) mHorizontalAngle += 360;
//            if (mHorizontalAngle > 0) mHorizontalAngle %= 360;
//
//            // limit the verticle angle so we dont allow top to bottom orbiting of the player
//            mVerticleAngle = Math.Min(mVerticleAngle, _verticleMaxAngle);
//            mVerticleAngle = Math.Max(mVerticleAngle, _verticleMinAngle);
//
//            // compute YZ plane direction vector
//            Vector3d tmp = Utilities.MathHelper.VectorFrom2DHeading(mVerticleAngle);
//            Vector3d up;
//            up.x = 0;
//            up.y = tmp.z;
//            up.z = tmp.x;
//         //   mContext.Up = up;
//        }
//
//
//        //  the camera has a destination and its constantly moving to that destination 
//        // we apply the dampening and acceleration/deceleration, etc at that time. How do we get our picking though to
//        // keep the camera from going thru things?
//        public override void Update(double elapsedSeconds)
//        {
//            //http://devmaster.net/forums/topic/6630-a-camera-that-orbits-around-a-mesh-while-matching-its-roll/
//            _currentRadius += _zoomDelta;
//            _zoomDelta = 0;  // reset the zoom so when not scrolling mousewheel we dont keep incrementing using last values
//            _currentRadius = Math.Min(_currentRadius, _maxRadius);
//            _currentRadius = Math.Max(_currentRadius, _minRadius);
//
//            // note: XAxis -if not constraining elevation, the camera will eventually flip because the Up vector doesnt change.  This is why the tvcamera needs alot of changes to work as space sim cam
//            // YAxis - only when using keyboard to rotate, we'll set TurningLeft and TurningRight states in player otherwise we use the controller AngleY
//
//            Vector3d focus = Target; // so that we arent orbiting or looking at the players feet
//            //focus.y += 10;// TODO: p.Height * .75F;
//            // note: we do not use BBox center because the animation causes it to shift constantly
//
//
//            // we may add dampening during turning so that you can see parts of the sides of the actor when turning 
//            // so like, while target is turning left, apply positive dampening at a angular velocity of 20 with a max of 90.  and negative for turning right.
//            // when the stop turning, decrease the dampening til its back to 0.  Same for our chase cam.  
//            TV_3DVECTOR desiredCameraPosition =
//                CoreClient._CoreClient.Maths.MoveAroundPoint(Helpers.TVTypeConverter.ToTVVector(focus), _currentRadius, mHorizontalAngle, mVerticleAngle);
//
//            TV_3DVECTOR translation = desiredCameraPosition - Helpers.TVTypeConverter.ToTVVector(mContext.Position);
//
//            // TODO: i think this update needs to translate the Context itself.  
//            mContext.Translate(translation.x, translation.y, translation.z);
//            // TODO: with the camera being rendered at origin, i think a lookat of the origin 
//            // is wrong.  the camera space world origin should be our focus
//
//            
//            // note: camera.UpdateViewMatrix() up vector calculation
//            //       is what is causing us issues with 360 rotation of our chase camera
//            //       and recall a chase camera is basically same as a orbit camera but with no
//            //       fixed limits for the horiztonal and vertical angles.
//            focus = -mContext.Position;
//
//            mContext.LookAt = focus; //NOTE: we need a target lookat offset too
//            // because otherwise if player origin is at player's feet, we'll be look too low.  
//        }
//    }
//}
