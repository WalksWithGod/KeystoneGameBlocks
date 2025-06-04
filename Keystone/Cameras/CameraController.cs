using System;
using System.Drawing;
using Keystone.Types;

namespace Keystone.Cameras
{
    public abstract class CameraController
    {
        protected Camera mCamera;
        private bool mMouseLook = false;
        private bool mMousePanning = false;
        protected float _zoom = 0;
        protected float _zoomRate = 10;
        protected float _zoomDelta;

        public CameraController(Camera camera)
        {
            if (camera == null) throw new ArgumentNullException();
            mCamera = camera;

            camera._controller = this;
        }

        public bool MouseLook { get { return mMouseLook; } set { mMouseLook = value; } }
        public bool MousePanning { get { return mMousePanning; } set { mMousePanning = value; } }
        public void ToggleMouseLook()
        {
            mMouseLook = !mMouseLook;
        }

        public float Zoom { get { return _zoom; } }

        public void ZoomIn(int amount)
        {
            _zoomDelta = amount;
            _zoom += _zoomRate;
        }

        public void ZoomOut(int amount)
        {
            _zoomDelta = amount;
            _zoom -= (_zoomRate * 100000);
        }


        public virtual void HandleMouseLook(Point delta) { }
        public virtual void HandleMousePanning(Vector3d mouseWorldPosition) { }
        public abstract void Update(float elapsed);
        
    }
}
