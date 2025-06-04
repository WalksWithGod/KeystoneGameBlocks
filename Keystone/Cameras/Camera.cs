using System;
using System.Collections.Generic;
using Keystone.Celestial;

using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Cameras
{
    public class Camera
    {
        protected Viewport _viewport;
        protected TVCamera _tvcamera;

        protected Vector3d _origin = Vector3d.Zero();
        protected Vector3d _position;
        protected Vector3d mScale;   // camera scaling is for perspective zoom to scale entire scene easily without modifying scene graph
        protected Vector3d _lookat;
        private Quaternion mRotation = new Quaternion ();
        protected float _zoom = 1f;
        
        private Matrix _view = Types.Matrix.Identity();
        protected Matrix _invViewMatrix = Types.Matrix.Identity();
        protected Matrix _projection;
        protected Matrix _rotationmatrix;
        protected double _aspectRatio = 1;
        private bool mIsOrthographic;
               
        protected float _near, _far, _fovDegrees, _fovRadians;


        public Camera(float near, float far, float fovDegrees, bool enableSphereCull, bool enablePlanesCull,
                      bool enableConeCull)
        {
        	
        	_tvcamera = CoreClient._CoreClient.CameraFactory.CreateCamera();
            mIsOrthographic = false;
            SetViewFrustum(fovDegrees, near, far, enableSphereCull, enablePlanesCull, enableConeCull);
            _zoom = 1f;
            mScale = new Vector3d (_zoom, _zoom, _zoom);
            _view = Matrix.Identity();
        }

        // http://www.codeguru.com/cpp/w-d/dislog/win32/article.php/c8279 <-- quat camera for flight sims simulators
        // need to read this primer on orthographic projections
        //http://www.codeguru.com/Cpp/misc/misc/math/article.php/c10123__2/
        //http://www.truevision3d.com/forums/tv3d_sdk_65/nonsquare_isometric_camera-t13994.0.html;msg96749#msg96749
        // the zoom value defines how many world units are seen vertically by the camera along its basis vectors.
        public Camera(float zoom, float near, float far)
        {
            _tvcamera = CoreClient._CoreClient.CameraFactory.CreateCamera();
            SetViewIsometric(zoom, near, far, false, true, false);
            _zoom = 1f;
            mScale = new Vector3d (_zoom, _zoom, _zoom);
            _view = Matrix.Identity();
        }

        public Viewport Viewport
        {
            get { return _viewport; } 
            set 
            {
                _viewport = value;
                Init();
            }
        }

        public float Far { get { return _far; } }
        public float Near { get { return _near; } }
        public float FOVRadians { get { return _fovRadians; } }


        public TVCamera TVCamera
        {
            get { return _tvcamera; }
        }

        public double AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                if (_aspectRatio == value) return;
                _aspectRatio = value;
                _tvcamera.SetCustomAspectRatio((float)value);
                Init(); 
            }
        }

        //http://www.truevision3d.com/forums/tv3d_sdk_65/nonsquare_isometric_camera-t13994.0.html;msg96749#msg96749
        // the zoom value defines how many world units are seen vertically by the camera along its basis vectors.
        // Zoom == 1 means that viewport width worth of world units are shown horizontally
        // Zoom == 2 means taht viewport width x 2 worth of world units are shown horizontally
        // Zoom == .5 means that viewport width x .5 worth of world units are shown.
        public float Zoom 
        { 
            get { return _zoom;  } 
            set 
            { 
                _zoom = value;
                Init();
            } 
        }

        public Vector3d Scale 
        {
        	get 
        	{
        		return mScale ;
        	}
        	set 
        	{
        		mScale = value;
        		
        	}
        }
        
        public void SetRotation(double x, double y, double z)
        {
        	// 6DoF camera by utilizing quaternions and by not constraining up() vector which is the x axis (pitch)
        	// http://xboxforums.create.msdn.com/forums/p/26070/142339.aspx
        	Quaternion yaw = Yaw (y * Utilities.MathHelper.DEGREES_TO_RADIANS);
		    Quaternion pitch = Pitch (x * Utilities.MathHelper.DEGREES_TO_RADIANS);
     		Quaternion roll = Roll (z * Utilities.MathHelper.DEGREES_TO_RADIANS);
     
     		// set property, not direct var so we can initiate recalc of rotation matrix 
     		Rotation = yaw * pitch * roll;		
        
     		// TODO: can't we just call ctor of quat and get same answer?
     		Vector3d ypw = new Vector3d (y * Utilities.MathHelper.DEGREES_TO_RADIANS, x * Utilities.MathHelper.DEGREES_TO_RADIANS, z * Utilities.MathHelper.DEGREES_TO_RADIANS);
     		System.Diagnostics.Debug.Assert (ypw.Equals (Rotation));
     		
        }

        public Quaternion Yaw(double radians)
        {
            if (radians != 0.0d)
            {
            	Quaternion q = new Quaternion(Vector3d.Up(), radians);
                return q;
            }
            return new Quaternion();
        }

        public Quaternion Pitch(double radians)
        {
            if (radians != 0.0d)
            {
            	Quaternion q = new Quaternion(Vector3d.Right(), radians);
                return q;
            }
            return new Quaternion();
        }

        public Quaternion Roll(double radians)
        {
            if (radians != 0.0d)
            {
            	Quaternion q = new Quaternion(Vector3d.Forward(), radians);
                return q;
            }
            return new Quaternion();
        }

        // http://xboxforums.create.msdn.com/forums/p/26070/142339.aspx
        public Quaternion Rotation
        {
        	get {return mRotation;}
        	set 
        	{
        		// if (mRotation.Equals(value)) return;
                mRotation = value;
                // TV3D's rotation order for component rotation matrices is X * Y * Z then * T for viewmatrix
                // but for component quats its yaw * pitch * roll or y * x * z
                _rotationmatrix =  new Matrix (mRotation); // TODO: i think this is wrong? viewmatrix rotation matrix from what is different than just from quat?
                                                           // however, if this was wrong, we'd not have things in proper location and they are. just culling is off
                // TODO: temp rotationmatrix using old vectors to compare with quat computed rotationmatrix
//                Matrix altMatrix = // TV3D's rotation order is X * Y * Z then * T for viewmatrix
//                	 Matrix.CreateRotationX(vecrotation.x * Utilities.MathHelper.DEGREES_TO_RADIANS)
//                       * Matrix.CreateRotationY(vecrotation.y * Utilities.MathHelper.DEGREES_TO_RADIANS) * Matrix.CreateRotationZ(vecrotation.z * Utilities.MathHelper.DEGREES_TO_RADIANS)  ;
//                
//                _rotationmatrix = altMatrix ;
				_invViewMatrix =  _rotationmatrix ; // *Matrix.Translation(_position); // with origin space rendering, _invViewMatrix is same as _rotationMatrix (assuming there's no scaling needed in the final view matrix)

                TV_3DMATRIX tmpView = Helpers.TVTypeConverter.CreateTVMatrix();
                float det = 0;
                // TODO: I do now have maybe a working Matrix.Inverse of my own... so maybe i can replace the following? well i dont trust mine for now so use TV's
                CoreClient._CoreClient.Maths.TVMatrixInverse(ref tmpView, ref det, Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix));
                _view = Helpers.TVTypeConverter.FromTVMatrix(tmpView);
                _tvcamera.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix));               

                Init();
        	}
        }
        
// obsolete - switched from vector3d to quaternion
//        /// <summary>
//        /// Creates a new view matrix based on yaw, pitch, roll of the camera
//        /// </summary>
//        public Vector3d Rotation
//        {
//            get { return _rotation; }
//            set
//            {
//                // if (_rotation.Equals(value)) return;
//                _rotation = value;
//                // TV3D's rotation order is X * Y * Z then * T for viewmatrix
//                _rotationmatrix =  Matrix.RotationX(_rotation.x * Utilities.MathHelper.DEGREES_TO_RADIANS)
//                       * Matrix.RotationY(_rotation.y * Utilities.MathHelper.DEGREES_TO_RADIANS) * Matrix.RotationZ(_rotation.z * Utilities.MathHelper.DEGREES_TO_RADIANS)  ;
//
//                //TV_3DQUATERNION quat = new TV_3DQUATERNION();
//                //CoreClient._CoreClient.Maths.TVQuaternionRotationYawPitchRoll(ref quat, (float)_rotation.y, (float)_rotation.x, (float)_rotation.z);
//                //TV_3DMATRIX rotationTVMat = new TV_3DMATRIX();
//                //CoreClient._CoreClient.Maths.TVConvertQuaternionToMatrix(ref rotationTVMat, quat);
//                //_rotationmatrix = Helpers.TVTypeConverter.FromTVMatrix(rotationTVMat);
//                
//                
//                _invViewMatrix = _rotationmatrix; // *Matrix.Translation(_position); // with origin space rendering, _invViewMatrix is same as _rotationMatrix (assuming there's no scaling needed in the final view matrix)
//
//                TV_3DMATRIX tmpView = Helpers.TVTypeConverter.CreateTVMatrix();
//                float det = 0;
//                // TODO: I do now have maybe a working Matrix.Inverse of my own... so maybe i can replace the following?
//                CoreClient._CoreClient.Maths.TVMatrixInverse(ref tmpView, ref det, Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix));
//                _view = Helpers.TVTypeConverter.FromTVMatrix(tmpView);
//                _tvcamera.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix));               
//
//                Init();
//            }
//        }

        public void SetLookAt(double x, double y, double z)
        {
            Vector3d result;
            result.x = x;
            result.y = y;
            result.z = z;
            LookAt = result;
        }


        public Vector3d LookAt
        {
            get { return _lookat; }
            internal set
            {
            
                if (_lookat.Equals(value)) return;
                _lookat = value;
                
                // cached previous values can be restored
                // TODO: temp hack to use tv's setlookat to compute rotation from lookat since our UpdateViewMatrix doesnt seem to work properly
                _tvcamera.SetLookAt((float)value.x, (float)value.y, (float)value.z);
                // quick hack because SetLookAt is typically set before i translate origin, but grabbing and setting the resulting rotation results in the rotation being constant regardless of translation of origin
                // TODO: however, it could be that our normal mouse rotation is overriding this setting and so we should be changing the rotation of the
                // editor controller and not the camera directly.
                TV_3DVECTOR rot = _tvcamera.GetRotation();
                // TODO: these rot values must be in radians. I think this is now broken as of Jan.31.2014 after switching from Vector3d to quaternion rotation
                //       because call to SetRotation now assumes degrees are passed in.  Now who knows, maybe this is passing degrees.  need to verify either way
                System.Diagnostics.Debug.Assert (false, "Camera.LookAt() - verify setter rot.x rot.y rot.z are in degrees.");
                SetRotation(rot.x, rot.y, rot.z); 
               // UpdateViewMatrix();
               // Init();
            }
        }

        public Matrix RotationMatrix { get { return _rotationmatrix; } }
        public Matrix View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;

                TV_3DMATRIX inv = Helpers.TVTypeConverter.CreateTVMatrix();
                float det = 0;
                // TODO: one day i need to write my own matrix inverse that works in all cases including jiglibx physics
                CoreClient._CoreClient.Maths.TVMatrixInverse(ref inv, ref det, Helpers.TVTypeConverter.ToTVMatrix(_view));
                _tvcamera.SetMatrix(inv);
              
                _invViewMatrix = Helpers.TVTypeConverter.FromTVMatrix(inv);

                Init();
            }
        }

        public Matrix InverseView
        {
            get { return _invViewMatrix; }
            set
            {
                // if (_invViewMatrix.Equals(value)) return;
                _invViewMatrix = value;
                _tvcamera.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix));

                float det = 0;
                TV_3DMATRIX inv = Helpers.TVTypeConverter.ToTVMatrix(_invViewMatrix);
                TV_3DMATRIX view = Helpers.TVTypeConverter.CreateTVMatrix();
                // TODO: replace with my own matrix inverse?
                CoreClient._CoreClient.Maths.TVMatrixInverse(ref view, ref det, inv);
                _view = Helpers.TVTypeConverter.FromTVMatrix(view);
                
                Init();
            }
        }

        public Matrix Projection
        {
            get
            {
                if (_viewport == null) return Matrix.Identity();
                //return _projection;

                Matrix proj;
                // TODO: I am recalcing every frame and not caching and only recomputing
                //        if dirty.
                // recalc  if dirty.  Dirty is when viewport height or width changes, or any of the fov, near or far changes.
                // Or the zoom for Orthographic cameras
                if (mIsOrthographic)
                {
                    // if auto compute distanceToOrthoPlane, then use half height of the combined 
                    // bounding volumes of all visible items last frame
                    // otherwise use the fixed distance
                    proj = Types.Matrix.ScaledOrthoMatrix(_zoom, _viewport.Width,
                        _viewport.Height, _near, _far);
                }
                else
                {
                    proj = Types.Matrix.PerspectiveFOVLH(_near, _far, _fovRadians, _viewport.Width, _viewport.Height, ref _aspectRatio);
                }

                // call setter so that .SetCustomProjection is called
                Projection = proj;
                return proj;
            }
            set
            {
                // this is typically used when we want to create a projection that is not based on the actual viewport height and width
                // Right now its only used by FXImposter and FXPlanetAtmosphere for generating the billboard planet (actually a planet imposter)
                _projection = value;
                
                // _tvcamera can be null when RenderingContext is first created. TODO: need to fix that
                if (_tvcamera != null)
	                // always have to set in actual tvcamera because it's required to render with proper projection. Dur!
                	_tvcamera.SetCustomProjection(Helpers.TVTypeConverter.ToTVMatrix(_projection));

                // the following is only to get the fov.
                // TODO: There has to be an easy way to compute the FOV without calling tvcamera.GetViewFrustum
                // TODO: I think with zoom == 1.0, fov == 45 degrees so 
                // fov = (45 * zoom) * Utilities.MathHelper.DEGREES_TO_RADIANS;  ??? not 100% sure yet
                float fovDegrees, far, near;
                fovDegrees = far = near = 0;
                _tvcamera.GetViewFrustum(ref fovDegrees, ref far, ref near);
                _fovDegrees = fovDegrees;
                _fovRadians = _fovDegrees * (float)Utilities.MathHelper.DEGREES_TO_RADIANS;
                _near = near;
                _far = far;
                // from http://forums.cgsociety.org/archive/index.php/t-725538.html
                // Ortho camera FOVs depend on the target distance. Other than that, it's the usual trigonometry bits.
            	// I.e. if you have a target 100 units away from the camera and you want the camera to cover, horizontally, 100 units as well, then imagine the full horizontal FOV as two right triangles with the target distance as the adjacent length and the desired view in units, divided by two, as the opposite length. 
            	// halfHorizontalFOV = atan(opposite/adjacent) = atan (50/0/100.0) = atan (0.5) = 26.5651
            	//  Then to cover the full horizontal FOV, simply multiply by two.

            }
        }

        public Matrix ViewProjection
        {
        	get 
        	{
        		return View * Projection;
        	}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="near"></param>
        /// <param name="farplane"></param>
        /// <param name="enableSphereCull"></param>
        /// <param name="enablePlanesCull"></param>
        /// <param name="enableConeCull"></param>
        public void SetViewIsometric(float zoom, float near, float far,  bool enableSphereCull, bool enablePlanesCull, bool enableConeCull)
        {
            // must update our near and far
            _near = near;
            _far = far;

            // OBSOLETE - .SetViewIsometric is no longer used.
            // I now in the Projection property in this class, compute a new
            // ScaledOrthoMatrix upon changes to zoom
           
            _tvcamera.SetViewIsometric(zoom,far, near);
            mIsOrthographic = true;
            Init();
        }

        public void SetViewFrustum(float fovDegrees, float near, float far, bool enableSphereCull, bool enablePlanesCull, bool enableConeCull)
        {
            _near = near;
            _far = far;
            _fovDegrees = fovDegrees;
            _fovRadians =  fovDegrees * (float) Utilities.MathHelper.DEGREES_TO_RADIANS;
            // May.2.2012 
            // TODO: this _one_ initial call to tvcamera.SetViewFrustum seems vital for picking at the moment.  I'm not sure why...
            // Perhaps the .SetViewFrustum call initializes some required variables within tvCamera?
            _tvcamera.SetViewFrustum(fovDegrees, _far, _near);

            mIsOrthographic = false;
            Init();
        }

        private void Init()
        {
            if (_viewport == null) return;
            // note: do not want to get position and rotation from single precision _tvCamera.Get****()  that just overwrites our doubles!!!
            //  _position = new Vector3d(_tvcamera.GetPosition());
            // _rotation = new Vector3d(_tvcamera.GetLookAt());
            _lookat = Helpers.TVTypeConverter.FromTVVector(_tvcamera.GetLookAt());

            if (mIsOrthographic)
                Projection = Types.Matrix.ScaledOrthoMatrix(_zoom, _viewport.Width,
                            _viewport.Height, _near, _far);
            else
                Projection = Types.Matrix.PerspectiveFOVLH(_near, _far, _fovRadians, 
                            _viewport.Width, _viewport.Height, ref _aspectRatio);
        }

        public void GetBasisVectors(ref Vector3d front, ref Vector3d up, ref Vector3d right)
        {
            TV_3DVECTOR tvfront = new TV_3DVECTOR(0, 0, 0);
            TV_3DVECTOR tvup = new TV_3DVECTOR(0, 0, 0);
            TV_3DVECTOR tvright = new TV_3DVECTOR(0, 0, 0);
            _tvcamera.GetBasisVectors(ref tvfront, ref tvup, ref tvright);
            front.x = tvfront.x;
            front.y = tvfront.y;
            front.z = tvfront.z;
            up.x = tvup.x;
            up.y = tvup.y;
            up.z = tvup.z;
            right.x = tvright.x;
            right.y = tvright.y;
            right.z = tvright.z;
        }



        // TODO: what we should do is when the LookAt is set, we return
        // not a fixed var but a position + (dir);
        public Vector3d Up
        {
            get { return _view.Up; } // used for billboarding
        }

        private void UpdateViewMatrix()
        {
            TV_3DVECTOR tvfront;
            tvfront.x = 0;
            tvfront.y = 0;
            tvfront.z = 0;
            TV_3DVECTOR tvup;
            tvup.x = 0;
            tvup.y = 0;
            tvup.z = 0;
            TV_3DVECTOR tvright;
            tvright.x = 0;
            tvright.y = 0;
            tvright.z = 0;
            // we only care to get the up vector.
            _tvcamera.GetBasisVectors(ref tvfront, ref tvup, ref tvright);

            Vector3d up;
            up.x = tvup.x;
            up.y = tvup.y;
            up.z = tvup.z;
            // up = Vector3d.Up(); TODO: for nav galaxy view orbit cam, constraining to Vector3d.Up() fixes the camera rolling
            // but why is the camera rolling in the first place?
            // I do know that by not constraining the Y axis, im able to do a sommersault 
            // with camera and thus do not experience the Y flip when going past straight up or down look

            System.Diagnostics.Debug.Assert (Vector3d.Zero().Equals (_position));
            _view = Matrix.CreateLookAt (Vector3d.Zero(), LookAt, Up); // TODO: not i'm passing Up property and not "up" basic vector retreived here
           
            
            TV_3DMATRIX inv = default(TV_3DMATRIX);
            float det = 0;
            // TODO: one day i need to write my own matrix inverse that works in all cases including jiglibx physics
            // TODO: actually i do have a View Matrix inverse working right?  
            CoreClient._CoreClient.Maths.TVMatrixInverse(ref inv, ref det, Helpers.TVTypeConverter.ToTVMatrix(_view));
            _tvcamera.SetMatrix(inv);
            _invViewMatrix = Helpers.TVTypeConverter.FromTVMatrix(inv);
        }

        public void Dispose()
        {
            _viewport = null; // dont dispose the viewport as it may still be in use.
                              // it will be disposed via the windows control it sits on
            CoreClient._CoreClient.CameraFactory.RemoveCamera(_tvcamera);
            _tvcamera = null;
        }

        //// http://answers.unity3d.com/questions/187543/draw-orthographic-visuals-eg-health-bar-in-perspec.html
        ////
        ////Here is the solution I arrived at which does the job of rendering ortho into a perspective scene.
        ////•Setup a main camera with perspective projection, depth only clear flag, near/far clip planes 1: 100, camera depth -1.
        ////•Setup a GUI camera with orthographic projection, don't clear flag, size 100, near/far clip planes 1 : 100, camera depth 0.
        ////•Get a world position vector for the health bar prefab at a spot above character position
        ////•Convert the perspective position vector to normalized viewport coordinates via mainCamera.WorldToNormalizedViewportPoint() (see below)
        ////•Convert the viewport point back to a world point via guiCamera.NormalizedViewportToWorldPoint() (see below)
        ////
        ////Camera Extensions:
        //public static class CameraExtensions
        //{
        //    /// [summary]
        //    /// The resulting value of z' is normalized between the values of -1 and 1, 
        //    /// where the near plane is at -1 and the far plane is at 1. Values outside of 
        //    /// this range correspond to points which are not in the viewing frustum, and 
        //    /// shouldn't be rendered.
        //    /// 
        //    /// See: http://en.wikipedia.org/wiki/Z-buffering
        //    /// [/summary]
        //    /// [param name="camera"]
        //    /// The camera to use for conversion.
        //    /// [/param]
        //    /// [param name="point"]
        //    /// The point to convert.
        //    /// [/param]
        //    /// [returns]
        //    /// A world point converted to view space and normalized to values between -1 and 1.
        //    /// [/returns]
        //    public static Vector3 WorldToNormalizedViewportPoint(this Camera camera, Vector3 point)
        //    {
        //        // Use the default camera matrix to normalize XY, 
        //        // but Z will be distance from the camera in world units
        //        point = camera.WorldToViewportPoint(point);

        //        if (camera.isOrthoGraphic)
        //        {
        //            // Convert world units into a normalized Z depth value
        //            // based on orthographic projection
        //            point.z = (2 * (point.z - camera.nearClipPlane) / (camera.farClipPlane - camera.nearClipPlane)) - 1f;
        //        }
        //        else
        //        {
        //            // Convert world units into a normalized Z depth value
        //            // based on perspective projection
        //            point.z = ((camera.farClipPlane + camera.nearClipPlane) / (camera.farClipPlane - camera.nearClipPlane))
        //             + (1 / point.z) * (-2 * camera.farClipPlane * camera.nearClipPlane / (camera.farClipPlane - camera.nearClipPlane));
        //        }

        //        return point;
        //    }

        //    /// [summary]
        //    /// Takes as input a normalized viewport point with values between -1 and 1,
        //    /// and outputs a point in world space according to the given camera.
        //    /// [/summary]
        //    /// [param name="camera"]
        //    /// The camera to use for conversion.
        //    /// [/param]
        //    /// [param name="point"]
        //    /// The point to convert.
        //    /// [/param]
        //    /// [returns]
        //    /// A normalized viewport point converted to world space according to the given camera.
        //    /// [/returns]
        //    public static Vector3 NormalizedViewportToWorldPoint(this Camera camera, Vector3 point)
        //    {
        //        if (camera.isOrthoGraphic)
        //        {
        //            // Convert normalized Z depth value into world units
        //            // based on orthographic projection
        //            point.z = (point.z + 1f) * (camera.farClipPlane - camera.nearClipPlane) * 0.5f + camera.nearClipPlane;
        //        }
        //        else
        //        {
        //            // Convert normalized Z depth value into world units
        //            // based on perspective projection
        //            point.z = ((-2 * camera.farClipPlane * camera.nearClipPlane) / (camera.farClipPlane - camera.nearClipPlane)) /
        //             (point.z - ((camera.farClipPlane + camera.nearClipPlane) / (camera.farClipPlane - camera.nearClipPlane)));
        //        }

        //        // Use the default camera matrix which expects normalized XY but world unit Z 
        //        return camera.ViewportToWorldPoint(point);
        //    }
        //}
        //
        //// Usage:
        //void LateUpdate()
        //{
        //    var position = perspectiveCamera.WorldToNormalizedViewportPoint(worldTransform.position);
        //    objectTransform.position = orthographicCamera.NormalizedViewportToWorldPoint(position);
        //}


    }
}