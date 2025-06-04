//using System;
//using Keystone.Types;
//
//namespace Keystone.Cameras
//{
//    public class OrthographicEditorViewController : EditorViewController 
//    {
//        public OrthographicEditorViewController(RenderingContext context)
//            : base(context)
//        {
//        }
//
//        // TODO: the orthographic viewpoint should host child behavior nodes for handling
//        //       updates for this projection mode, or it can simply be a Selector switch
//        //       in our camera such that correct behavior node is selected and executed
//        public override void Update(double elapsedSeconds)
//        {
//            // THIS IS MOSTLY FOR KEYBOARD INPUT OF ORTHO CAMERA ZOOM AND TRANSLATION.
//            // MOUSELOOK and MOUSE PAN ARE NOT HANDLED HERE>..
//            // IN FACT, MOUSELOOK IS IGNORED BY ORTHO CONTROLLER
//            Vector3d translation = Vector3d.Zero();
//
//            double fSpeed = elapsedSeconds * .5; // _speed* METERS_PER_MILLISECOND;
//
//            double fA1 =  (mHorizontalAngle / 360.0d * Math.PI * 2.0d);
//            double fA2 =  (mHorizontalAngle + 90.0d) / 360.0d * Math.PI * 2.0d;
//
//            // NOTE: In addition to being able to use mouse wheel for zoom, the direciton keys can be used
//            // and here we apply any directional inputs to the mContext.Zoom
//
//            // the pan keys should be responded too differently based on the ViewType
//            if (mContext.ViewType == Viewport.ViewType.Right || mContext.ViewType == Viewport.ViewType.Left)
//            {
//                if (Direction.z != 0)
//                    mContext.Zoom += (float)(fSpeed * Direction.z);
//            }
//
//            translation.z =
//               (Math.Cos(fA1) * fSpeed * Direction.x) + 
//               (Math.Cos(fA2) * fSpeed * Direction.z);
//
//            if (mContext.ViewType == Viewport.ViewType.Front || mContext.ViewType == Viewport.ViewType.Back)
//            {
//                if (Direction.x != 0)
//                    mContext.Zoom += (float)(fSpeed * Direction.x);
//            }
//
//            translation.x =
//                (Math.Sin(fA1) * fSpeed * Direction.x) + 
//                (Math.Sin(fA2) * fSpeed * Direction.z);
//
//            if (mContext.ViewType == Viewport.ViewType.Top || mContext.ViewType == Viewport.ViewType.Bottom)
//            {
//                // the camera's position.y can be used to do things like chop off floors we dont want to see in the editor
//                // but generally that would be done with menu or keyboard shortcuts and not general mouse and keyboard maneuvering
//                if (Direction.y != 0)
//                    mContext.Zoom += (float)(fSpeed * Direction.y);
//            }
//            else
//                translation.y = fSpeed * Direction.y;
//
//            // this call occurs which leads to update of the frustum planes 
//            // since it will set "Camera.Position" which results in _frustum.Update
//            mContext.Translate(translation);
//        }
//    }
//}
