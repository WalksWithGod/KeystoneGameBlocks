using System;
using System.Drawing;
using Keystone.Types;
using MTV3D65;
using Keystone.Events;

namespace Keystone.Cameras
{
    /// <summary>
    /// A viewport class used to define the target (handle), position and rendering options to use on the viewport
    /// NOTE: Viewport MUST be created AFTER tv3d is initialized to fullscreen or a window.  Even though it will appear to work
    /// it will result in subtle errors such as Viewport.AutoResize not working
    /// NOTE: Culling and rendering must be done seperately for each active viewport.
    /// </summary>
    public class Viewport : IDisposable, IEventHandler
    {
        public enum ViewType 
        {
            None,  // uninitialized
            Top,
            Bottom,
            Left,
            Right,
            Front,
            Back,
            Free
        }

        public enum ProjectionType 
        {
            None,  // uninitialized
            Perspective,
            Orthographic,
            Isometric
        }

       
        private RenderingContext _context;
        private TVViewport _tvviewport;
        private IntPtr _handle;
        private int _height;
        private int _width;
        private string _name;
        private int _screenTop;
        private int _screenLeft;
        private Keystone.Types.Color _backColor;

        // NOTE: Do not uncomment.  We do not want to support this kind of viewport creation. 
        // Always create a new viewport via Engine.CreateViewport()
        //public Viewport(TVViewport viewport)
        //{
        //    _tvviewport = viewport;
        //    _tvviewport.SetAutoResize(false);
        //    _name = viewport.GetName();
        //    // _camera = viewport.GetCamera(); //NOTE: the camera must always be set externally or TODO: perhaps we add it required to constructor?
        //    Core._CoreClient.Viewports.Add(_name, this);
        //}

        private bool _capture = false;
        private bool _captureEnabled = true;
        private double RAY_SCALE = 1000000000d; // for picking distance

        private Viewport(System.Windows.Forms.Control control, string name, IntPtr handle)
        {
            _name = name;
            _handle = handle;
            mControl = control;
            // NOTE: If we see error VIEWPORT MANAGER ERROR : Viewport_OnReset : Couldn't create
            // rendering surfaces, unknown DirectX error. Maybe Out of video memory. 
            // dx error : -2147024809
            // it could means that the window we're using to host the viewport
            // has 0 width and 0 height
            _tvviewport = CoreClient._CoreClient.Engine.CreateViewport(_handle, _name);
            _tvviewport.SetAutoResize(false);
            System.Diagnostics.Debug.WriteLine("Viewport.ctor() - SUCCESS: Viewport '" + _name + "' created using window handle == " + _handle.ToString());
            CoreClient._CoreClient.Viewports.Add(_name, this);
            
            
            int left, top;
            System.Windows.Forms.Control c = System.Windows.Forms.Control.FromHandle(_handle);
            mParentHandle = c.Parent.Handle;
            left = c.ClientRectangle.Left;
            top = c.ClientRectangle.Top;
            Point p = c.PointToScreen(new Point(left, top));
            Resize(p.X, p.Y);
        }

        private IntPtr mParentHandle;
        System.Windows.Forms.Control mControl;
        public static Viewport Create (System.Windows.Forms.Control control, string name, IntPtr handle)
        {
            if (control == null) throw new ArgumentNullException();
            if (CoreClient._CoreClient.Viewports.ContainsKey(name))
                throw new ArgumentException("Viewport.Create() - ERROR: Duplicate viewport '" + name + "' already exists!");

            return new Viewport(control, name, handle);
        }

        #region IEventHandler Members

        public void HandleEvent(EventType et, InputCaptureEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IInputCapture Members
        public bool InputCaptureEnable { get { return _captureEnabled; } set { _captureEnabled = value; } }
        public bool HasInputCapture { get { return _capture; } }
        #endregion

       public MouseEventArgs GetMouseHitInfo(int absoluteScreenX, int absoluteScreenY)
       {
           int x, y;
           ScreenToViewport(absoluteScreenX, absoluteScreenY, out x, out y);
           Vector3d unprojectedCoords = UnProject(Context.Camera.View, Context.Camera.Projection, x, y);
           MouseEventArgs arg = new MouseEventArgs();
           arg.Viewport = this;
           arg.ViewportRelativePosition = new Point(x, y);
           arg.UnprojectedCameraSpacePosition = unprojectedCoords;
           return arg;
       }
               
        /// <summary>
        /// From a 2D Mouse Point, generate a Ray and performs interesction test against specified plane.
        /// </summary>
        /// <param name="ray_origin">Often camera's region position is used to place the ray at camera's region position</param>
        /// <param name="camera"></param>
        /// <param name="p"></param>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public Vector3d Pick_PlaneRayIntersection(Matrix view, Matrix projection, Vector3d ray_origin, Plane p, int mouseX, int mouseY)
        {
            Vector3d result = ray_origin;

            // TODO: do i need seperate UnProject's for large frustum and small?
            Vector3d nearPick;
            Vector3d farPick;
            Vector3d dir = UnProject(view, projection, mouseX, mouseY, out nearPick, out farPick);

            // origin can be use to put the ray in world space
            Ray ray = new Ray(nearPick + ray_origin, dir);

            Vector3d intersectionPoint = Vector3d.Zero();
            double hitDistance = 0;
            if (p.Intersects(ray, RAY_SCALE, ref hitDistance, ref intersectionPoint))
                result = ray.Origin + (ray.Direction * hitDistance);

            return result;
        }

        /// <summary>
        /// For a starting 2D Mouse Point and an end 2D Mouse Point, generate pick Rays 
        /// against the near plane
        /// such that we can create a 2D vector from start to end on the near plane
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="p">The plane we wish to find the intersection points of the mouse pick at the start of the
        /// operation and the mouse pick and the end of the operation</param>
        /// <param name="start_ray">Camera Space 'Start' ray</param>
        /// <param name="end_ray">Camera Space 'End' ray</param>
        /// <param name="hitDistanceStart"></param>
        /// <param name="hitDistanceEnd"></param>
        /// <returns></returns>
        public bool Pick_PlaneStartEndRays(Matrix view, Matrix projection, Point start, Point end, Plane p, out Ray start_ray, out Ray end_ray, out double hitDistanceStart, out double hitDistanceEnd)
        {
            // intersect the rays with the perpendicular rotation plane
            hitDistanceStart = 0;
            hitDistanceEnd = 0;

            // un-project rays from the start and end points of the mouse
            Vector3d startPickNear;
            Vector3d startPickFar;
            Vector3d dir = UnProject(view, projection, start.X, start.Y, out startPickNear, out startPickFar);
            // camera space start ray (caller can add context.Position to ray origin if they want)
            start_ray = new Ray(startPickNear, dir);

            Vector3d endPickNear;
            Vector3d endPickFar;
            dir = UnProject(view, projection, end.X, end.Y, out endPickNear, out endPickFar);
            // camera space start ray (caller can add context.Position to ray origin if they want)
            end_ray = new Ray(endPickNear, dir); 

            // TODO: 0, -1, 0 assume orthographic top view!
            //Vector3d rayDir = new Vector3d(0, -1, 0); // startPick - endPick;
            //rayDir = Vector3d.Normalize(startPick - endPick); 
            //start_ray = new Ray(startPick, rayDir);
            //end_ray = new Ray(endPick, rayDir); // we use near on both

//            start_ray = new Ray(_context.Position, Vector3d.Normalize(-startPick));
//            end_ray = new Ray(_context.Position, Vector3d.Normalize(-endPick));
            //// project rays from the start and end points of the mouse
            //Vector3d nearPick;
            //Vector3d farPick;
            //Vector3d rayDir = UnProject(start.X, start.Y, out nearPick, out farPick);
            //start_ray = new Ray(nearPick, rayDir);
            //rayDir = UnProject(end.X, end.Y, out nearPick, out farPick);
            //end_ray = new Ray(nearPick, rayDir); // we use near on both

            // exit if either of the intersections is invalid
            if (!p.Intersects(start_ray, RAY_SCALE, ref hitDistanceStart) || !p.Intersects(end_ray, RAY_SCALE, ref hitDistanceEnd))
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>TODO: verify  this result is always in camera space</returns>
        public Vector3d UnProject(Matrix view, Matrix projection, double x, double y)
        {
            return UnProject(view, projection, x, y, this.Context.Near);
        }

        /// <summary>
        /// Fills in the near and far out params and Returns the normalized direction between the two.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <returns>TODO: verify  this result is always in camera space</returns>
        public Vector3d UnProject(Matrix view, Matrix projection, double x, double y, out Vector3d near, out Vector3d far)
        {
            near = UnProject(view, projection, x, y, this.Context.Near);
            far = UnProject(view, projection, x, y, this.Context.Far);

            //return Vector3d.Normalize(near - far); // nov.5.2012 not sure if this is ok, but it works for top down ortho view, but not in perspective
            // above could mean we have a normal that is flipped for the plane we are creating in the ortho case or the perspective case in which case the near - far is correct
            return Vector3d.Normalize(far - near);
        }

        // TODO: note how we're not actually using View or Projection parameters!  
        public Vector3d UnProject(Matrix view, Matrix projection, double x, double y, double desiredDistance)
        {
            Vector3d result = UnProject2DPointTo3D(_context.Camera, x, y, desiredDistance);
            //result = UnProject (_context.Camera, view, projection, new Vector3d (x, y, desiredDistance));
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private Vector3d UnProject2DPointTo3D(Camera camera, double x, double y, double desiredDistance)
        {
        	// TODO: I think for large frustum we are not using proper camera.
        	
            // TODO: WARNING: THis is producting a Vector3d return value that is NaN for all members
            //       when for orthographic project the zoom value is too high.  I'm not sure exactly
            //       where it's failing because it's occurring inside tv's functions and not my own.
            TV_3DVECTOR result;
            result.x = 0;
            result.y = 0;
            result.z = 0;


            //if (_context.ProjectionType == ProjectionType.Orthographic)
            //{
            //    Vector3d unprojectResult;
            //    unprojectResult.x = x - _width / 2;
            //    unprojectResult.z = y - _height / 2;
            //    unprojectResult.y = desiredDistance;

            //    unprojectResult += _context.Position ; // = Vector3d.TransformCoord(unprojectResult, camera.View);
            //    unprojectResult.y = desiredDistance;
            //    // TODO: we aren't scaling the amount by the ortho zoom factor!
            //    return unprojectResult;
            //}


            if (_tvviewport == null) return Vector3d.Zero();
            System.Diagnostics.Debug.Assert(camera.Viewport.TVViewport == _tvviewport);
            System.Diagnostics.Debug.Assert(_context.Camera == camera);
            CoreClient._CoreClient.Engine.SetViewport(_tvviewport);
            // note: setting the viewport alone does NOT also set the camera even if that viewport has it's own camera.  
            // we must explicitly set the tv camera to tvengine as well
            CoreClient._CoreClient.Engine.SetCamera(camera.TVCamera);
            
            // TODO: are we sure that in the above when setting current camera that
            //       the camera has the proper projection matrix?  i supsect so since
            //       the .Projection starts off using small frustum and is always reset to small
            //       when culling is complete.  
            // camera.Projection = _context.Projection
           

            double farDistance = camera.Far;
            double nearDistance = camera.Near;
            //double farDistance = _context.Far2; 
            //double nearDistance = _context.Near2; 

            double depthRatio ;
            
            // NOTE: for orthographic projection, I think our depth ratio should always just be 0.0
            //       because perspective is irrelevant.
            // TODO: this is just bizarre.  the final depthRatio calc option produces values between 1 and 2.  How the hell if
            // near = 0 and far = 1?
            if (desiredDistance <= nearDistance)
                depthRatio = 0;
            else if (desiredDistance >= farDistance)
                depthRatio = 1;
            else
            {
                // NOTE: first depthRatio calc seems to work with Move/Rotate/Scale widgets.
                depthRatio = (farDistance / (farDistance - nearDistance)) + (((nearDistance * farDistance) / (farDistance - nearDistance)) * (1 / desiredDistance));
                //depthRatio = (desiredDistance - nearDistance) / (farDistance - nearDistance);
            }
            
            // #1 option is to just use tv's own project function
            // TV names this Project2DPointTo3D but really this is unproject because projection means from World to Screen
            // and this function takes screen and unprojects those coords to World coords.
            CoreClient._CoreClient.Maths.Project2DPointTo3D((float)x, (float)y, (float) depthRatio, ref result);
            return Helpers.TVTypeConverter.FromTVVector(result);

            // #2  is my own UnProject which i've not gottenworking...the following is somewhat broken... annoying
            //return UnProject(_width, _height, x, y, depth, Matrix.Identity(), camera.View, camera.Projection);


            //// #3 if we just need near or far (0.0 and 1.0)  we can return either near far from GetMousePickVectors() which
            //// is nice mostly if you want both the near and the far at once
            //TV_3DVECTOR up = CoreClient._CoreClient.Globals.Vector(0, 0, 0);
            //TV_3DVECTOR forward = CoreClient._CoreClient.Globals.Vector(0, 0, 0);
            //TV_3DVECTOR side = CoreClient._CoreClient.Globals.Vector(0, 0, 0);

            //camera.TVCamera.GetBasisVectors(ref forward, ref up, ref side);

            //TV_3DVECTOR near = CoreClient._CoreClient.Globals.Vector(0, 0, 0);
            //TV_3DVECTOR far = CoreClient._CoreClient.Globals.Vector(0, 0, 0); 

            //CoreClient._CoreClient.Maths.GetMousePickVectors((float)x, (float) y, ref near, ref far);

            //// debug section to see if we can convert the 3d point back to 2d and get the same 2d mouse coords
            ////float x2d = 0;
            ////float y2d = 0;
            ////CoreClient._CoreClient.Maths.Project3DPointTo2D(far, ref x2d, ref y2d, true);
            ////System.Diagnostics.Debug.WriteLine(string.Format("near = {0},{1},{2} far = {3},{4},{5}", near.x, near.y, near.z, far.x, far.y, far.z));
            
            //// the remaining is Brac and George's version that accepts a distance in tvunits and returns the intersection on the plane
            //// at that distance.  They did this because computing the proper farplaneratio to pass for a specific distance was too 
            //// annoying to compute
            
            ////  #4 actually requires the near/far from the GetMousePickVectors above 
            //TV_PLANE plane = CoreClient._CoreClient.Globals.TVPlane(0, 0, 0, 0);
            //// http://www.truevision3d.com/forums/tv3d_sdk_65/project2dpointto3d_problem-t19253.0.html;msg132289#msg132289
            //CoreClient._CoreClient.Maths.TVPlaneFromPointNormal(ref plane, Helpers.TVTypeConverter.ToTVVector (_context.Position) + (forward * (float)desiredDistance ), forward * -1);
            //CoreClient._CoreClient.Maths.TVPlaneIntersectLine(ref result, plane, near, far);

            //return Helpers.TVTypeConverter.FromTVVector(result);
        }

//        // http://code.google.com/p/monoxna/source/search?q=Viewport&origq=Viewport&btnG=Search+Trunk
//        public Vector3d UnProject(Camera cam, Matrix view, Matrix projection, Vector3d screenSpace)
//        {
//            Matrix matrix = Matrix.Inverse (Matrix.Multiply4x4(view, projection));
//            Vector3d v;
//
//
//            Vector3d position;
//            // convert the screenspace to viewport relative space
//            position.x = screenSpace.x; //  (((screenSpace.x - _screenLeft) / (Width)) * 2f) - 1f;
//            position.x = ((2.0f * screenSpace.x) / _width) - 1.0d;
//            position.y = screenSpace.y; // -((((screenSpace.y - _screenTop) / (Height)) * 2f) - 1f);
//            position.y = -((2.0f * screenSpace.y) / _height) - 1.0d;
//            // depth ratio - this is problematic somewhat.  near and far that is
//            position.z = (screenSpace.z - cam.Near) / (cam.Far - cam.Near);
//
//
//            v = Vector3d.TransformCoord(position, matrix);
//            double a = (((v.x * matrix.M14) + (v.y * matrix.M24)) + (v.z * matrix.M34)) + matrix.M44;
//            v /= a;
//            return v;
//
//            Vector4 us4 = new Vector4(position, 1d);
//            Vector4 up4 = Vector4.Transform(us4, Matrix.Inverse(projection));
//            Vector4 uv4 = Vector4.Transform(up4, view);
//
//            Vector3d uv3 = new Vector3d(uv4.x, uv4.y, uv4.z);
//            uv3 = uv3 / uv4.w;
//
//            return uv3;
//        }

        //// TODO: here our unproject is different
        //Vector3d UnProject(Vector3d vector, double x, double y, double minZ, double maxZ, Matrix worldViewProjection)
        //{
        //    Vector3d v;
        //    Matrix matrix = Matrix.Inverse(worldViewProjection);

        //    return v;
        //}

        //public Vector3d UnProject(double screenX, double screenY, double screenZ, float depthRatio)
        //{
        //    Matrix matrix =  Matrix.Multiply4x4(_context.Camera.View, _context.Camera.Projection);
        //    return UnProject(new Vector3d(screenX, screenY, screenZ), 0, 0, _context.Near, _context.Far, matrix);


        //    //matrix = Matrix.Inverse(matrix);
        //    // source.x - x, source.y - y, source.z -z is used to find the viewport x,y from absolute screen coords 
        //    Vector3d v;
        //    //    // http://www.toymaker.info/Games/html/picking.html
        //    //    //Convert the point's coords from screen space (0 to wholeViewport width/height) to proj space (-1 to 1)
        //    //v.x = ((2.0f * screenX) / _width ) - 1.0d;
        //    //v.x = (((screenX - this.ScreenLeft) / _width) * 2.0d) - 1.0;
        //    v.x = ((screenX / _width) * 2.0d) - 1.0; // no .ScreenLeft cuz we already pass in viewport space coords
        //    //v.y = -((2.0f * screenY) / _height ) - 1.0d;
        //    //v.y = -((((screenY - this.ScreenTop) / _height) * 2.0d) - 1.0);
        //    v.y = -(((screenY / _height) * 2.0) - 1.0); // no .ScreenTop cuz we already pass in viewport space coords
        //    v.z = (screenZ - this.Context.Camera.Near) / (this.Context.Camera.Far - this.Context.Camera.Near);
        //    Vector3d vector1 = Vector3d.TransformCoord(v, matrix);
        //    double a = (((v.x * matrix.M14) + (v.y * matrix.M24)) + (v.z * matrix.M34)) + matrix.M44;
        //    if (!(WithinEpsilon(a, 1.0)))
        //        vector1 = vector1 / a;

        //    //    // if the depthRatio > 0, we need to scale it by the proportion of the distance from near to farplane
        //    //    // so if the depthRatio was 
        //    //    // scale the vector by  depthRatio * (far - min)
        //    //    // ok, i see now why the xna version has the  /.w  part... you have to compute the 0.0 - 1.0 range and that means effectively
        //    //    // getting the direction, the start, scaling it by that proportion and adding it to the result.
        //    return vector1;
        //}





        public Vector3d Project(double x, double y, double z, Matrix view, Matrix projection, Matrix world)
        {
            Vector3d result;
            result.x = x;
            result.y = y;
            result.z = z;
            return Project(result, view, projection, world);
        }

        /// <summary>
        /// Returns the 2d projected coordinate with the result.z component being the ratio between the near and far plane you want
        /// for the world coordinate's z
        /// </summary>
        /// <param name="coordinate">Coordinate must be in world coordinates</param>
        /// <returns></returns>
        public Vector3d Project(Vector3d coordinate, Matrix view, Matrix projection, Matrix world)
        {
            if (_context == null || _context.Camera == null) return coordinate ;

            // TODO: when using near and far frustums, are we using the correct camera for planets far away? they all seem to be rendering labels in the same point
            CoreClient._CoreClient.Engine.SetViewport(_tvviewport);
            // note: setting the viewport alone does NOT also set the camera even if that viewport has it's own camera.  
            // we must explicitly set the tv camera to tvengine as well
            CoreClient._CoreClient.Engine.SetCamera(_context.Camera.TVCamera);
            
            // TODO: I suspect that if i attempted to have both Planets and Ships or missiles etc to be rendered using either
            // the .1 or 1000 near planes and then switch them over as appropriate, that you'd see a very noticeable shift 
            // as a result of the precision issue differences beteen the two.  I think for this reason we should enforce that only worlds
            // and stars use the special culling and nothing else... period.  Other far away ships can be scaled forward assuming they arent
            // occluded by a star or world.  Otherwise, they too are rendered and picked with normal camera.
            // Same with asteroids and comets and such.  I don't feel this is a significant limitation especially for 1.0.
            float x2d = 0;
            float y2d = 0;
            TV_3DVECTOR tvcoord = Helpers.TVTypeConverter.ToTVVector (coordinate); 
            
            bool front = CoreClient._CoreClient.Maths.Project3DPointTo2D(tvcoord, ref x2d, ref y2d, true);
            Vector3d result;
            result.x = x2d;
            result.y = y2d;
            result.z = _context.Near;
            if (front == false)
                result.z = -1; // use int -1, not fractional since Project generally returns whole pixel values for x, y, so z too
            return result;
            
            //System.Diagnostics.Debug.WriteLine(string.Format("near = {0},{1},{2} far = {3},{4},{5}", near.x, near.y, near.z, far.x, far.y, far.z));

//            Matrix world = Matrix.Identity();
//            Matrix matrix = world * _context.Camera.View;
//            matrix = Matrix.Multiply4x4(matrix, _context.Camera.Projection);
//            Vector3d vector1 = Vector3d.TransformCoord(coordinate, matrix);
//            double a = (((coordinate.x * matrix.M14) + (coordinate.y * matrix.M24)) + (coordinate.z * matrix.M34)) + matrix.M44;
//            
//            // i believe this attempts to normalize if it determines it's not already normalized??
//            //if (!WithinEpsilon(a, 1.00d))
//            //{
//                vector1 = (Vector3d)(vector1 / a);
//            //}
//            
//            // after the coordinate is transformed above, the coord will be 
//            // between -1 and 1 for x and y, so here we convert x and y to screen coordinates
//            vector1.x = (((vector1.x + 1.00d) * 0.50d) * Width) + 0;// this.ScreenLeft;
//            vector1.y = (((-vector1.y + 1.00d) * 0.50d) * Height) + 0; // this.ScreenTop;
//
//            // fairly certain MaxDepth and MinDepth refer to far & near
//            vector1.z = (vector1.z * (1.0 - 0)) + 0;
//            return vector1;
        }

        public Vector3d Project(Vector3d coordinate, Matrix world)
        {
            Matrix matrix = world * _context.Camera.View;
            matrix = Matrix.Multiply(matrix, _context.Camera.Projection);
            Vector3d vector1 = Vector3d.TransformCoord (coordinate, matrix);
            double a = (((coordinate.x * matrix.M14) + (coordinate.y * matrix.M24)) + (coordinate.z * matrix.M34)) + matrix.M44;

            // i believe this attempts to normalize if it determines it's not already normalized??
            if (!(WithinEpsilon(a, 1.0)))
                vector1 = (Vector3d)(vector1 / a);
            
            // after the coordinate is transformed above, the coord will be 
            // between -1 and 1 for x and y, so here we convert x and y to screen coordinates
            vector1.x = (((vector1.x + 1.00d) * 0.50d) * _width) + 0; // this.ScreenLeft; // our coordinates are already in viewport relative position
            vector1.y = (((-vector1.y + 1.00d) * 0.50d) * _height) + 0;// this.ScreenTop;
            // depth buffer value
            vector1.z = (vector1.z * (this.Context.Far - this.Context.Near)) + this.Context.Near;
            //vector1.z = (vector1.z * (1 - 0)) + 0; 
            return vector1;
        }

        private bool WithinEpsilon(double a, double b)
        {
            double difference = a - b;
            if (difference >= 0)
                return difference <= double.Epsilon;

            return false;
        }
        
        public void SetTargetArea (int left, int top, int width, int height)
        {
        	throw new NotImplementedException ();
        	CoreClient._CoreClient.Engine.GetViewport().SetTargetArea(left, top, width, height);
        }

        public void Resize(int screenPositionLeft, int screenPositionTop)
        {
        	
        	
            // NOTE: Sylvain was wrong about Engine.ResizeDevice().  He said it will resize all active viewports so only needs to be called once after all viewports are resized rather than
            // after each one.  But this is simply not what is happening.
            // Actually it's been a while and maybe he was right and i forgot to delete the above comment.  I think part of my initial problem
            // is that I wasn't manually releasing DX resources i was manually creating for things like Imposter rendering (buffers, and such)
            //if (!_isPrimary)
            //{
            //CoreClient._CoreClient.ResizeDevice();
           // _tvviewport.Resize();
            //}
            //else
            CoreClient._CoreClient.ResizeDevice(_tvviewport);

            _screenLeft = screenPositionLeft;
            _screenTop = screenPositionTop;
            _height = _tvviewport.GetHeight();
            _width = _tvviewport.GetWidth();
            // we should notify the context i think.
            if (_context != null)
                _context.ViewportResized();
            
        }

        public void ScreenToViewport( int screenPositionX, int screenPositionY, out int relX, out int relY)
        {
            int x = screenPositionX - _screenLeft;
            int y = screenPositionY - _screenTop;
            relX = x;
            relY = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeX">Viewport relative mouse position X</param>
        /// <param name="relativeY">Viewport relative mouse position Y</param>
        /// <returns></returns>
        public bool Contains(int relativeX, int relativeY)
        {
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            return rect.Contains(relativeX, relativeY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenX">Screen position X</param>
        /// <param name="screenY">Screen position Y</param>
        /// <returns></returns>
        public static Viewport GetMouseOverViewport(int screenX, int screenY)
        {
            Rectangle rect;
            System.Collections.Generic.List<Viewport> potentialViewports = new System.Collections.Generic.List<Viewport>(CoreClient._CoreClient.Viewports.Values.Count);

            foreach (Viewport vp in CoreClient._CoreClient.Viewports.Values)
            {
                if (vp.Context == null) continue;

                if (vp.Context.Workspace.IsActive) // todo: i think the concept of .IsActive for a workspace is flawed. If the workspace exists, it is always "Active" but perhaps not visible.
                {
                    // todo: we need to know the zorder too or the mouse picking will pick the first instanced vp rect that contains the mouse position
                    // todo: we should first get a list of all vp's that are potential hits and then find the one that is topmost.
                    if (vp.Visible && vp.Enabled)
                    {

                        rect = new Rectangle(vp.ScreenLeft, vp.ScreenTop, vp.Width, vp.Height);
                        if (rect.Contains(screenX, screenY))
                        {
                            potentialViewports.Add(vp);
                            //return vp;
                        }
                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine("Potential viewport count = " + potentialViewports.Count);
            if (potentialViewports.Count == 0)
                return null;
            else
                return GetTopMost(potentialViewports, CoreClient._CoreClient.Graphics.Handle);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
        private const int GW_HWNDPREV = 3;

       
        private static Viewport GetTopMost(System.Collections.Generic.List<Viewport> viewports, IntPtr handle)
        {
            if (viewports.Count == 1) return viewports[0];

            while ((handle = GetWindow(handle, GW_HWNDPREV)) != IntPtr.Zero)
            {
                for (int i = 0; i < viewports.Count; i++)
                    //if (viewports[i].mControl.Handle == handle)
                    // NOTE: we have no control over the docking and free floating windows. 
                    //       The form handle can change everytime we dock and undock a window.
                    if (viewports[i].mControl.FindForm().Handle  == handle)
                        return viewports[i];
            }

            return null;
        }

        public RenderingContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public bool Enabled
        {
            get 
            {
                if (_context == null) return false;
                return _context.Enabled; 
            }
            set
            {
                
                if (_context == null) return; 
                _context.Enabled = value;
               
            }
        }

        public System.Windows.Forms.Cursor Cursor
        {
            get { return System.Windows.Forms.Control.FromHandle(_handle).Cursor; }
            set 
            {
                System.Windows.Forms.Control control = System.Windows.Forms.Control.FromHandle(_handle);
                // TODO: not sure what is going on here
                if (control != null)
                    control.Cursor = value;
                else
                    System.Diagnostics.Debug.WriteLine("Viewport.cs.Cursor setter - must fix ");
               // System.Windows.Forms.Control.FromHandle(_handle).Cursor = value; 
            }
        }

        public Keystone.Types.Color BackColor
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                _tvviewport.SetBackgroundColor(_backColor.ToInt32());
            }
        }
        public TVViewport TVViewport
        {
            get { return _tvviewport; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IntPtr Handle
        {
            get { return _handle; }
        }

        public int TVIndex
        {
            get { return _tvviewport.GetViewportIndex(); }
        }

        public void AutoResize(bool value)
        {
            _tvviewport.SetAutoResize(value);
        }

        public int ScreenTop
        {
            get { return _screenTop; }
            set { _screenTop = value; }
        }

        public int ScreenLeft
        {
            get { return _screenLeft; }
            set { _screenLeft = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double AspectRatio
        {
            get 
            {
                //divide by zero check usually occurs if we minimize so the aspectRatio doesnt need to change anyway
                double tmpHeight = _height;
                if (tmpHeight <= 0)
                    tmpHeight = 0.000001; // avoid divide by 0

                return _width / tmpHeight;
            }
        }

        public bool Visible
        {
            get { return mControl.Visible; }
        }

        // todo: do i need to capture input when a drag operation occurs starting in one viewport and ends up over another?
        //public int ZOrder // todo: do we get this from the control/window that is assigned (yet to do)
        //{
        //    get
        //    {

        //        System.Windows.Forms.Form f;
        //        f.
        //        return mControl.;
        //    }
        //}

        #region IDisposable Members
        private bool _disposed;
        ~Viewport()
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

        }

        protected virtual void DisposeUnmanagedResources()
        {

            if (_context != null) _context.Dispose();
            _context = null;
            CoreClient._CoreClient.Viewports.Remove(_name);
            try
            {
                //_tvviewport.SetCamera(null);
                // TODO: CameraFactory has a .RemoveCamera which i call during Camera.Dispose()
                // but there is no Engine.RemoveViewport so i wonder if there could be a tv3d leak

            }
            catch { }
            finally { _tvviewport = null; }
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