using System;
using Keystone.Entities;
using Keystone.Vehicles;
using Keystone.Types;
using MTV3D65;
using System.Drawing;

namespace Keystone.Cameras
{
    public class ChaseCameraController : CameraController 
    {
        protected float _verticleMaxAngle;
        protected float _verticleMinAngle;
        protected float _angleX;
        protected float _angleY;
        private double _cameraLag; // 1.0 = instant updating 0.0 no updating.  Otherwise the camera will adjust to its 
        // new position as a ratio of the movementAmount = totalDistance to travel / cameraLag
        // of course if the player is moving non stop, then with any kind of lag the camera will eventually fall
        // behind the player.  So the camera needs to be able to accelerate to catch up.
        // the camera's acceleration is tricky to come up with a good value though...
        // Like if hte player rotates, the camera must rotate faster (it has further distance to travel) or it wont keep up.
        // i mean, i could increase the acceleration the further the camera falls behind.. :/


        // offsets to the camera
        private float _maxRadius = 5000;
        private float _minRadius = 300; // zoom can be used to travel between min and max
        private float _currentRadius;
        private double _polarRadius; //distance from the target
        private double _dampening; // verticle dampening for when the camera is following the player up and down bumps and hills
        // we dont want the camera being effected by every single pothole and divot.


        public ChaseCameraController(Camera camera)
            : base(camera)
        {
                    //  consider that 0 degrees is a position about the center of our actor directly in front of it.  
            //  going counterclockwise, 90 degrees would be directly beneath it looking up
            // 180 degrees is directly behind it
            // 270 degrees is directly above it looking down
            _verticleMaxAngle = 260; // higher lets us look closer to straight down.  
            _verticleMinAngle = 150; // lower lets us look closer to straight up
            _angleX = _verticleMinAngle; // init to min angle
        }

        private float mVerticleAngle;
        private float mHorizontalAngle;
        public override void HandleMouseLook(Point delta)
        {
            EntityBase p = ((ThirdPersonCamera)mCamera).Target;

            //    // the  y rotation of the player has to be tied to the mouse movement, else the controls will feel unresponsive
            //    // the x rotation is for camera elevation. It will be constrained by min/max angles and also in Update by collision
            //    if (p.MouseLook)
            //    {
            // todo: we must first grab the entity's rotation directly so that we can sychronize with any rotation that was done outside of here, such as arrow keys
            // todo:  but im commenting out for now because of error with the player's rotation assigned by input never getting passed
            //           along to the physics engine and thus in Simulation.Update() when that rotationmatrix is grabbed, it wipes out the rotation assigned with input
            //horizontalAngle = (float)p.Rotation.y; // sychronize mouselook rotation with keyboard rotation
            // todo: actually what we should do to sychronize mouse and arrow key rotation is to have arrow keys simply
            // call HandleKeyboardLook(); and have it also update horiztonalAngle
            mHorizontalAngle += delta.X / 2.0f; // mouse x axis movement controls horizontal rotation about world Y axis
            mVerticleAngle += delta.Y / 2.0f; // mouse y axis movement controls verticle rotation about world X axis

            //keep the angle values between 0 and 360
            if (mVerticleAngle < 0) mVerticleAngle += 360;
            if (mVerticleAngle > 0) mVerticleAngle %= 360;
            if (mHorizontalAngle < 0) mHorizontalAngle += 360;
            if (mHorizontalAngle > 0) mHorizontalAngle %= 360;

            // limit the verticle angle so we dont allow top to bottom orbiting of the player
            mVerticleAngle = Math.Min(mVerticleAngle, _verticleMaxAngle);
            mVerticleAngle = Math.Max(mVerticleAngle, _verticleMinAngle);
        }


        //  the camera has a destination and its constantly moving to that destination 
        // we apply the dampening and acceleration/deceleration, etc at that time. How do we get our picking though to
        // keep the camera from going thru things?
        public override void Update(float elapsed)
        {
            if (((ThirdPersonCamera)mCamera).Target == null) return;

            _currentRadius += _zoomDelta;
            _currentRadius = Math.Min(_currentRadius, _maxRadius);
            _currentRadius = Math.Max(_currentRadius, _minRadius);

            // note: XAxis -if not constraining elevation, the camera will eventually flip because the Up vector doesnt change.  This is why the tvcamera needs alot of changes to work as space sim cam
            // YAxis - only when using keyboard to rotate, we'll set TurningLeft and TurningRight states in player otherwise we use the controller AngleY

            EntityBase  p = ((ThirdPersonCamera)mCamera).Target;

            Vector3d focus = p.Translation; // so that we arent orbiting or looking at the players feet
            focus.y += 10;// todo: p.Height * .75F;
            // note: we do not use BBox center because the animation causes it to shift constantly

            // we may add dampening during turning so that you can see parts of the sides of the actor when turning 
            // so like, while target is turning left, apply positive dampening at a angular velocity of 20 with a max of 90.  and negative for turning right.
            // when the stop turning, decrease the dampening til its back to 0.  Same for our chase cam.  
            TV_3DVECTOR desiredCameraPosition =
                CoreClient._CoreClient.Maths.MoveAroundPoint(Helpers.TVTypeConverter.ToTVVector(focus), _currentRadius, mHorizontalAngle, mVerticleAngle);

            TV_3DVECTOR translation = desiredCameraPosition - Helpers.TVTypeConverter.ToTVVector(mCamera.Position);

            mCamera.TranslateCamera(translation.x, translation.y, translation.z);
            mCamera.LookAt = focus; //NOTE: we need a target lookat offset too
            // because otherwise if player origin is at player's feet, we'll be look too low.  
        }
    }
}
