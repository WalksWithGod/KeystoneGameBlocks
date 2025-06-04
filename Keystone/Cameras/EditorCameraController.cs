using System;
using System.Drawing;
using Keystone.Types;

namespace Keystone.Cameras
{
    public class EditorCameraController : CameraController 
    {
        private Vector3d _startPanWorldPosition; // the start of mouse pan in world coords computed using the unprojected 2d mouse coord
        private Vector3d mPan;


        private float _translationRate = .5f;
        private float _mouseLookSpeed = .5f;
        protected double mHorizontalAngle;
        protected double mVerticalAngle;

        protected float _speedMetersPerSecond = 1000; 
        protected const float MILLISECOND_TO_SECONDS = .001f;

        public EditorCameraController(EditorCamera camera) : base(camera) 
        {
            mPan = new Vector3d();
        }

        public float Speed { get { return _speedMetersPerSecond; } set { _speedMetersPerSecond = value; } }
        public double PanX { get { return mPan.x; } set { mPan.x = value; } }
        public double PanY { get { return mPan.y; } set { mPan.y = value; } }
        public double PanZ { get { return mPan.z; } set { mPan.z = value; } }


        public Vector3d Direction
        {
            get { return mPan; }
        }


        // perspective panning
        internal static void Pan(float dx, float dz)
        {
            //Vector3 v = camera.WorldOrientation * ((Vector3.UNIT_Z * -dz) + (Vector3.UNIT_X * -dx));
            //camNode.Translate(v.x, 0, v.z);
        }

        internal static void Orbit(float dx, float dy)
        {
            //camNode.Pitch(new Radian(-dy), Node.TransformSpace.TS_LOCAL);
            //camNode.Yaw(new Radian(-dx), Node.TransformSpace.TS_WORLD);
        }

        public void BeginMousePanning(int mouseX, int mouseY)
        {
            int x, y;
            mCamera.Viewport.ScreenToViewport(mouseX, mouseY, out x, out y);
            _startPanWorldPosition = mCamera.Viewport.UnProject(x, y);
        }

        public override void HandleMousePanning(Vector3d mouseWorldPosition)
        {

            Vector3d translation = _startPanWorldPosition - mouseWorldPosition;
            _startPanWorldPosition = mouseWorldPosition + translation;
            switch (mCamera.ViewType)
            {
                case Viewport.ViewType.Top:
                case Viewport.ViewType.Bottom:
                    translation.y = 0;
                    break;

                case Viewport.ViewType.Left:
                case Viewport.ViewType.Right:
                    translation.x = 0;
                    break;

                case Viewport.ViewType.Front:
                case Viewport.ViewType.Back:
                    translation.z = 0;
                    break;

            }
            mCamera.TranslateCamera(translation);
        }

        public override void HandleMouseLook(Point delta)
        {

            // orthogonal cameras can use rotations so long as they are not about thier designated axis
            mHorizontalAngle = mCamera.Rotation.y + (delta.X * _mouseLookSpeed); // mouse x axis movement controls horizontal rotation about world Y axis 
            mVerticalAngle = mCamera.Rotation.x + (delta.Y * _mouseLookSpeed); // mouse y axis movement controls verticle rotation about world X axis

            //keep the angle values between 0 and 360
            if (mVerticalAngle < 0) mVerticalAngle += 360;
            if (mVerticalAngle > 0) mVerticalAngle %= 360;

            if (mHorizontalAngle < 0) mHorizontalAngle += 360;
            if (mHorizontalAngle > 0) mHorizontalAngle %= 360;
        }

        public override void Update(float elapsedMilliseconds)
        {

            Vector3d translation;
            translation.x = 0;
            translation.y = 0;
            translation.z = 0;
            
            // perspective camera
            double speedThisFrame = elapsedMilliseconds * MILLISECOND_TO_SECONDS * _speedMetersPerSecond;

            double fA1 = (mHorizontalAngle / 360.0F * Math.PI * 2.0F);
            double fA2 = (mHorizontalAngle + 90.0F) / 360.0F * Math.PI * 2.0F;

            translation.z = (Math.Cos(fA1) * speedThisFrame * Direction.x) + (Math.Cos(fA2) * speedThisFrame * Direction.z);
            translation.x = (Math.Sin(fA1) * speedThisFrame * Direction.x) + (Math.Sin(fA2) * speedThisFrame * Direction.z);
            translation.y = speedThisFrame * Direction.y;

            // this call occurs which leads to update of the frustum planes since it will set "Camera.Position" which results in _frustum.Update
            mCamera.TranslateCamera(translation);
            mCamera.SetRotation(mVerticalAngle, mHorizontalAngle, 0);
        }
    }
}
