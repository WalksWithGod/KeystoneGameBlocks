//using System;
//using System.Drawing;
//using Keystone.Types;
//
//namespace Keystone.Cameras
//{
//    public class EditorViewController : ViewpointController 
//    {
//        private Vector3d mPan;
//
//        private Point mouseStart;
//        private Point mouseEnd;
//        private bool mSmoothRotation = true;
//        private float _mouseLookSpeed = .5f; // TODO: this should be configurable like "cam_speed"
//
//
//
//        public EditorViewController(RenderingContext context)
//            : base(context) 
//        {
//            mPan = Vector3d.Zero();
//        }
//
//
//        public bool SmoothRotation { get { return mSmoothRotation;  } set { mSmoothRotation = value; } }
//        public float Speed { get { return (float)mContext.GetCustomOptionValue(null, "cam_speed"); } }
//        
//        public double PanX { get { return mPan.x; } set { mPan.x = value; } }
//        public double PanY { get { return mPan.y; } set { mPan.y = value; } }
//        public double PanZ { get { return mPan.z; } set { mPan.z = value; } }
//
//        public Vector3d Direction
//        {
//            get { return mPan; }
//        }
//
//
//
//		// https://gist.github.com/JISyed/5017805
//        public void BeginMousePanning(int mouseX, int mouseY)
//        {
//            int x, y;
//            mContext.Viewport.ScreenToViewport(mouseX, mouseY, out x, out y);
//            mouseStart.X = x;
//            mouseStart.Y = y;
//            mouseEnd = mouseStart;
//        }
//
//        public override void HandleMousePanning(System.Drawing.Point mouseViewportRelativePosition)
//        {
//
//            //System.Diagnostics.Debug.WriteLine("mouse panning " + mouseViewportRelativePosition.ToString());
//
//            mouseStart = mouseEnd;
//            mouseEnd = mouseViewportRelativePosition;
//
//
//            Vector3d planeNormal = Vector3d.Up ();
//            switch (mContext.ViewType)
//            {
//                case Viewport.ViewType.Top:
//                    planeNormal = new Vector3d(0, 1, 0);
//                    break;
//                case Viewport.ViewType.Bottom:
//                    planeNormal = new Vector3d(0, -1, 0);
//                    break;
//
//                case Viewport.ViewType.Left:
//                    planeNormal = new Vector3d(-1, 0, 0);
//                    break;
//                case Viewport.ViewType.Right:
//                    planeNormal = new Vector3d(1, 0, 0);
//                    break;
//
//                case Viewport.ViewType.Front:
//                    planeNormal = new Vector3d(0, 0, -1);
//                    break;
//                case Viewport.ViewType.Back:
//                    planeNormal = new Vector3d(0, 0, 1);
//                    break;
//
//            }
//            // TODO: for deckplans, the planeDistance should be the current deck's
//            // y coordinate component right?  But 0 seems to be working for all zoom heights
//            // although perhaps that's because the 0 distance we hardcoded now has the grid
//            // plane always at 0!  So when we get into multilevel deckplans, we will have
//            // some grids that are higher or lower then the center primary deck.
//            double planeDistance = 0; // -mContext.Position.y;
//            //planeDistance = -mContext.Position.y;
//            Plane p = new Plane(planeNormal, planeDistance); // GetPlane(mSelectedAxes);
//
//            double hitDistanceStart, hitDistanceEnd;
//            Ray start_ray, end_ray;
//
//            // cast rays into the scene from the mouse start and end points and
//            // intersect the pick rays with the dual axis plane we want to move along
//
//            // if either of the intersections is invalid then bail out as it would
//            // be impossible to calculate the difference
//            if (!mContext.Viewport.Pick_PlaneStartEndRays(mContext.Camera.View, mContext.Camera.Projection, mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//            {
//                //System.Diagnostics.Debug.WriteLine("error...");
//                return;
//            }
//            // obtain the intersection points of each ray with the dual axis plane
//            Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//            Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);
//
//            // TODO: i believe perhaps the above mouse tracking should be done always
//            // not just for panning... except we may only want the "end_pos" and can/maybe
//            // write a Viewport.GetPickRay() for just one instead of both
//            // Part of the problem with trying to mouse pick the way we are is... well
//            // we're trying to select a "tile" when that tile doesnt exist as any pickable
//            // entity and so we try to just keep the concept of a tile purely conceptual
//            // Second, we dont want to create an isometric editor or do we?
//            // We could have it such that for our deck, a deck is comprised of a specific
//            // layout of segments that are all equal sized (tile sized) and cannot overlap or
//            // occupy the same space.  Then, components can exist as decorators to those deckplan
//            // "tiles."  These tiles later help us with pathing and our diffusion grid (collaborative diffusion)
//            // So I think perhaps it starts with a concept of an entity that is a FloorPlan
//            // that has properties like tileWidth, tileDepth and becomes our diffusion grid
//            // as well as pickable.  
//            // So like FPSCreator, maybe we could try having a 1x1 meter tile that is then directly
//            // pickable load up by default as "empty" and these get flagged as inivisible during game
//            // but not during edit.
//            // So in this context I think when selecting the current tile, what we'd actually
//            // be doing is using the Picker to then pick the specific "tile" within the deckplan
//            // we're currently looking at.
//            // Also actually, to reduce memory from having to have a ton of "SceneNodes" we can
//            // use special versions of both the SceneNode and the DeckEntity such that the tiles
//            // are not typical entity children but are now completely black box owned by the DeckEntity
//            // and are not children at all and thus don't require seperate "SceneNodes"  
//            // whether we do this on a per room basis or not is not important now, the key point is
//            // we will have room for optimizations and so first goal should be to simply create
//            // the FloorPlanEntity that has tiles and for now uses minimeshes to render all
//            // the same single tile.  Then we can change the material color for mouse over 
//            // and mouse selection.
//
//            // calculate the difference between the intersection points
//            Vector3d difference = end_pos - start_pos;
//            //difference.y = 0;
//            mContext.Translate (-difference);
//
//        }
//
//        // TODO: this occurs if +mouselook axis state is set and XY-AXIS delta event has occurred
//        //       which shows up to us as a +mousemove.  The behaviors that then run will have access 
//        //       to those states.  Also behaviors can enable/disable states that other behaviors in a sequence
//        //       can access  anyways, this is the plan for now... we'll see how this turns out!
//        public override void HandleMouseLook(Point delta)
//        {
//            // TODO: lowering mouseLookSpeed definetly smooths the mouse
//            // and we could also use a frame rate elapsed * _mouseLookSpeed to dampen it across framerates
//            // but maybe the main thing to do is to get rid of _mouseLookSpeed altogether
//            //_mouseLookSpeed = 0.1f;
//
//            // orthogonal cameras can use rotations so long as they are not about thier designated axis
//           
//   //         mHorizontalAngle = Utilities.MathHelper.WrapAngle(mContext.Rotation.y + (delta.X * _mouseLookSpeed)); // mouse x axis movement controls horizontal rotation about world Y axis 
//   //         mVerticalAngle = Utilities.MathHelper.WrapAngle(mContext.Rotation.x + (delta.Y * _mouseLookSpeed)); // mouse y axis movement controls verticle rotation about world X axis
//
//   
//   			// TODO: the delta value can be stored as a MouseVerticleAxis and MouseHorizontalAxis value
//   			//       and then our "Update" can actually compute the angles and store their states
//   			
//   			// and where "HandleMouseLook" is a more generic HandleAxisChange() or i dunno
//   			// there's still fair bit of complexity here and id hate to start ripping all this out
//   			// without knowing all the ramifications
//            mHorizontalAngle = Utilities.MathHelper.WrapAngle(mHorizontalAngle + (delta.X * _mouseLookSpeed)); // mouse x axis movement controls horizontal rotation about world Y axis 
//            mVerticalAngle = Utilities.MathHelper.WrapAngle(mVerticalAngle + (delta.Y * _mouseLookSpeed)); // mouse y axis movement controls verticle rotation about world X axis
//           
//        }
//
//
//        private Vector3d mLastTargetTranslation;
//
//        
//        
//        #region Shaking Camera - Camera Modififer - along with "Smoothing" I think these camera fx should be classes that we attach so that adding new ones is easy
//        // http://xnaessentials.com/archive/2011/04/26/shake-that-camera.aspx
//        // http://gamedev.stackexchange.com/questions/1828/realistic-camera-screen-shake-from-explosion
//        
//        // We only need one Random object no matter how many Cameras we have
//		private static readonly Random mRandomShakeVal = new Random();
//		
//		// Are we shaking?
//		private bool mShaking = true;
//		
//		// The maximum magnitude of our shake offset
//		private double mShakeMaxMagnitude = 0.1d;
//		
//		// The total duration of the current shake
//		private double mShakeDuration = 0;
//		
//		// A timer that determines how far into our shake we are
//		private double mShakeTimer;
//		
//		// The shake offset vector
//		private Vector3d mShakeOffset;
//
//        /// <summary>
//		/// Shakes the camera with a specific magnitude and duration.
//		/// </summary>
//		/// <param name="magnitude">The largest magnitude to apply to the shake.</param>
//		/// <param name="duration">The length of time (in seconds) for which the shake should occur.</param>
//		public void Shake(float magnitude, float duration)
//		{
//			if (duration < 0d) throw new ArgumentOutOfRangeException ();
//			if (magnitude < 0d) throw new ArgumentOutOfRangeException();
//			
//		    // We're now shaking
//		    mShaking = true;
//		
//		    // Store our magnitude and duration
//		    mShakeMaxMagnitude = magnitude;
//		    mShakeDuration = duration;
//		    
//		    // Reset our timer
//		    mShakeTimer = 0d;
//		}
//
//		/// <summary>
//		/// Helper to generate a random double in the range of [-1, 1].
//		/// </summary>
//		private double NextShakeValue()
//		{
//		    return mRandomShakeVal.NextDouble() * 2d - 1d;
//		}
//		#endregion  
//
//        public override void Update(double elapsedSeconds)
//        {
//        	// TODO: hack until we rename Update2() -> Update() 
//        	// for the near term, it will contain the following code but will first compute all the vars
//        	// it needs from all state changes that have occurred.
//        	Update2(elapsedSeconds);  return;
//        	
//            //RotateOrbit();
//            //return; 
//
//            // HANDLES MOUSE LOOK AND KEYBOARD MOVEMENT OF EDITOR CAMERA
//            Vector3d translation;
//            translation.x = 0;
//            translation.y = 0;
//            translation.z = 0;
//            
//            // perspective camera movement 
//            double speedThisFrame = elapsedSeconds * Speed;
//
//            double fA1 = (mHorizontalAngle / 360.0 * Math.PI * 2.0);
//            double fA2 = (mHorizontalAngle + 90.0) / 360.0 * Math.PI * 2.0;
//
//            translation.z = (Math.Cos(fA1) * speedThisFrame * Direction.x) + (Math.Cos(fA2) * speedThisFrame * Direction.z);
//            translation.x = (Math.Sin(fA1) * speedThisFrame * Direction.x) + (Math.Sin(fA2) * speedThisFrame * Direction.z);
//            translation.y = speedThisFrame * Direction.y;
//
//
//
//            // shake timer management
//			if (mShaking)			
//			{
//				// TODO: if viewpoint script version of this and we delete EditorViewController
//				// then we can query Viewpoint.Context.Target or something and see if it's had an explosion
//				translation += Shake (elapsedSeconds);
//			}
//           
//          
//			// TODO: if this is just about a "Viewpoint" and if each Viewpoint is associated with a specific RenderingContext (cloning the configs of the ones defined in SceneInfo or 
//			//       encountered in scene) then yes we can totally script it and get rid of EditorViewController and we can do
//			// if (this.Animations != null && this.Animations.Current != null)
//			//    // if fly-to then check if finished
//			//    // not finished then ignore mouselook and keyboard translations for now!
//			
//            // this call occurs which leads to update of the frustum planes since it will set "Camera.Position" which results in _frustum.Update
//            mContext.Translate(translation);
//
//
////            if (mSmoothRotation) // smootBehavior
////            {
////                Vector3d currentRotation = Quaternion. mContext.Rotation;
////                // TODO: our increment value is .1 which is 1/10 of the way towards our goal every
////                //       frame which makes our smoothing behave differently at different frame rates
////                //       i need to test this to see if this is actually a good thing since it'll 
////                //       appear to smooth more with worse frame rate or whether we want some
////                //       frame rate independant smooth rate so the smoothing is consistant
////                //       note: an increment of 0.1 seems to suggest we never actually reach our goal
////                //       we just get closer and closer.  perhaps this is good too as it prevents
////                //       any snapping.  And so long as currentRotation and targetRotation are not
////                //       the same value, it will always compute a closer new angle 
////                double smoothing = 0.3; // higher the value the less the smoothing, but less sense of mouse lag
////                // TODO: can we first implement this to only set a Quat on the viewpoint orientation?
////                double angleX = Utilities.MathHelper.LerpAngle(currentRotation.x, mVerticalAngle, smoothing);
////                double angleY = Utilities.MathHelper.LerpAngle(currentRotation.y, mHorizontalAngle, smoothing);
////                mContext.SetRotation(angleX, angleY, 0);
////            }
////            else
//                mContext.SetRotation(mVerticalAngle, mHorizontalAngle, 0);
//           
//        }
//        
//        
//        private Vector3d Chase (double elapsedSeconds)
//        {
//        	return Vector3d.Zero();
//        }
//        
//        // TODO: can we make this a behavior or animation easily? i think it should be our first test
//        //       and we neednt change much
//        private Vector3d FlyTo (double elapsedSeconds, Vector3d start, Vector3d destination)
//        {
//        	
//
////        	mFlyTimer += elapsedSeconds ;
////        	
////        	if (mFlyTimer >= mFlyDuration)
////        	{
////        		mFlying = false;
////        		mFlyTimer = mFlyDuration;
////        		return Vector3d.Zero();
////        	}
////        	else 
////        	{
////        		// Compute our progress in a [0, 1] range
////			   double progress = mShakeTimer / mShakeDuration;
////					 
////			 
////			   // Generate a new offset vector with three random values and our magnitude
////			   mFlyOffset = Vector3d.LerpSmoothStep (start, destination, mFlyTimer, mFlyDuration);
////			
////			   // If we're shaking, add our offset to our position and target
////			   return mFlyOffset;
////        	}
//			return Vector3d.Zero (); // temp return val
//        }
//        
//        // http://pastebin.com/4uqqM2Nv
//        private Vector3d PathFollow (double elapsedSEconds)
//        {
//        	return Vector3d.Zero();
//        }
//        
//        private Vector3d JumpTo (Entities.Entity target, float screenSpace)
//        {
//        	return Vector3d.Zero();
//        }
//        
//        private Vector3d Shake (double elapsedSeconds)
//        {
//        	// Move our timer ahead based on the elapsed time
//		   	mShakeTimer += elapsedSeconds;
//			
//		    // If we're at the max duration, we're not going to be shaking anymore
//		    if (mShakeTimer >= mShakeDuration)
//		    {
//		        mShaking = false;
//		    	mShakeTimer = mShakeDuration;
//		    	return Vector3d.Zero();
//		   	}
//			else 
//			{
//			   // Compute our progress in a [0, 1] range
//			   double progress = mShakeTimer / mShakeDuration;
//					 
//			   // Compute our magnitude based on our maximum value and our progress. This causes
//			   // the shake to reduce in magnitude as time moves on, giving us a smooth transition
//			   // back to being stationary. We use progress * progress to have a non-linear fall 
//			   // off of our magnitude. We could switch that with just progress if we want a linear 
//			   // fall off.
//			   double magnitude = mShakeMaxMagnitude * (1d - (progress * progress));
//			 
//			   // Generate a new offset vector with three random values and our magnitude
//			   mShakeOffset = new Vector3d( NextShakeValue(), NextShakeValue(), NextShakeValue()) * magnitude;
//			
//			   // If we're shaking, add our offset to our position and target
//			   return mShakeOffset;
//			  // lookat += mShakeOffset; // TODO: i dont think we need to modify lookAt here in our implementation because we work directly with a Rotation and not a LookAT.
//			  //                         // and to the extent that we use LookAt, it is to compute the Rotation
//			}	
//        }
//        
//                
//        // NOTE: Orbit is a input based mouse orbit rotator it is not an automatic orbit script based on elapsed time 
//        //       the orbit point is based on input changes so in other words, its not an animation
//        /// <summary>
//        /// Rotates the camera for orbit behavior. Rotations are either about
//        /// the camera's local y axis or the orbit target's y axis. The property
//        /// PreferTargetYAxisOrbiting controls which rotation method to use.
//        /// </summary>
//        /// <param name="headingDegrees">Y axis rotation angle.</param>
//        /// <param name="pitchDegrees">X axis rotation angle.</param>
//        /// <param name="rollDegrees">Z axis rotation angle.</param>
//        private void Orbit()
//        {
//            //double heading = Utilities.MathHelper.DEGREES_TO_RADIANS * mHorizontalAngle;
//            //double pitch = Utilities.MathHelper.DEGREES_TO_RADIANS * mVerticalAngle;
//            
//            ////if (preferTargetYAxisOrbiting)
//            ////{
//            ////    Quaternion rotation = Quaternion.Identity();
//
//            ////    if (heading != 0.0f)
//            ////    {
//            ////        rotation = new Quaternion(ref targetYAxis, heading);
//            ////        orientation = Quaternion.Concatenate(ref rotation, ref orientation);
//            ////    }
//
//            ////    if (pitch != 0.0f)
//            ////    {
//            ////        rotation = Quaternion.CreateFromAxisAngle(ref WORLD_X_AXIS, pitch);
//            ////        orientation = Quaternion.Concatenate(ref orientation, ref rotation);
//            ////    }
//            ////}
//            ////else
//            ////{
//            //    double roll = 0; //  Utilities.MathHelper.DEGREES_TO_RADIANS* rollDegrees;
//            //    //Quaternion rotation = new Quaternion (heading,  pitch, roll);
//            //    //Quaternion orientation = Quaternion.Concatenate(ref orientation, ref rotation);
//
//
//            //    mContext.SetRotation(heading, pitch, roll);
//
//
//            //    TV_3DVECTOR desiredCameraPosition =
//            //        CoreClient._CoreClient.Maths.MoveAroundPoint(Helpers.TVTypeConverter.ToTVVector(focus), _currentRadius, mHorizontalAngle, mVerticleAngle);
//
//            //    TV_3DVECTOR translation = desiredCameraPosition - Helpers.TVTypeConverter.ToTVVector(mContext.Position);
//
//            //    // TODO: i think this update needs to translate the Context itself.  
//            //    mContext.Translate(translation.x, translation.y, translation.z);
//            //    mContext.LookAt = focus;
//
//            //    // we must compute a translation based on 
//            //    //mContext.Translation = targetPosition + zAxis * orbitOffsetLength;
//            //    // where the zAxis is extracted from the orientation 
//
//            ////}
//        }
//    }
//}
