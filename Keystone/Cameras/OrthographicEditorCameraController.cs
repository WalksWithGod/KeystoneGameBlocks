using System;
using Keystone.Types;

namespace Keystone.Cameras
{
    public class OrthographicEditorCameraController : EditorCameraController 
    {
        public OrthographicEditorCameraController(EditorCamera camera)
            : base(camera)
        {
            _speedMetersPerSecond = 100;
        }

        public override void Update(float elapsed)
        {
            Vector3d translation;
            translation.x = 0;
            translation.y = 0;
            translation.z = 0;

            double fSpeed = elapsed*.5f; // _speed* METERS_PER_MILLISECOND;

            double fA1 =  (mHorizontalAngle/360.0d*Math.PI*2.0F);
            double fA2 =  (mHorizontalAngle + 90.0d)/360.0d*Math.PI*2.0d;

            // the pan keys should be responded too differently based on the ViewType
            if (mCamera.ViewType == Viewport.ViewType.Right || mCamera.ViewType == Viewport.ViewType.Left)
            {
                mCamera.Zoom += (float)(fSpeed * Direction.z);
            }

            translation.z =
               (Math.Cos(fA1)*fSpeed*Direction.x) + (Math.Cos(fA2)*fSpeed*Direction.z);

            if (mCamera.ViewType == Viewport.ViewType.Front || mCamera.ViewType == Viewport.ViewType.Back)
            {
                mCamera.Zoom += (float)(fSpeed * Direction.x);
            }

            translation.x =
                (Math.Sin(fA1)*fSpeed*Direction.x) + (Math.Sin(fA2)*fSpeed*Direction.z);

            if (mCamera.ViewType == Viewport.ViewType.Top || mCamera.ViewType == Viewport.ViewType.Bottom)
            {
                // the camera's position.y can be used to do things like chop off floors we dont want to see in the editor
                // but generally that would be done with menu or keyboard shortcuts and not general mouse and keyboard maneuvering
                mCamera.Zoom += (float)(fSpeed * Direction.y);
            }
            else
                translation.y = fSpeed*Direction.y;

            //_tvcamera.SetCustomAspectRatio
            // this call occurs which leads to update of the frustum planes since it will set "Camera.Position" which results in _frustum.Update
            mCamera.TranslateCamera(translation);
        }
    }
}
