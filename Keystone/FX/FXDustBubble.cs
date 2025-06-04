// OBSOLETE - See ProceduralHelper.MoveMotionField
//           - MotionFields are now assigned to RenderingContext instances so they can be unique for each
//             open viewport
//using System;
//using System.Collections.Generic;
//using Keystone.Elements;
//using Keystone.Types;
//using MTV3D65;
//using Keystone.Cameras;

//namespace Keystone.FX
//{
//    // http://medusaengine.svn.sourceforge.net/viewvc/medusaengine/Render3D/NodeStarField.cpp?revision=13&view=markup

//    // http://www.youtube.com/watch?v=ZvwIlcH8Vbc
//    public class FXDustBubble :FXBase 
//    {
//        private Entities.StaticEntity mDummyEntityt;
//        private Mesh3d mMesh;
//        private Model mModel;
//        private Random mRand;
//        private float mRadius;
//        private Vector3d mLastCameraPosition;

//        public FXDustBubble(Entities.StaticEntity  entity)
//        {
//            this._semantic = FX_SEMANTICS.FX_CAMERA_BUBBLE;
//            mDummyEntityt = entity;
            
//            mModel = mDummyEntityt.Model;
//            mMesh = (Mesh3d)mModel.Geometry;
//            Resource.Repository.IncrementRef(null, mMesh);
//            mRadius = 1000;
//            mRand = new Random();
//        }

//        ~FXDustBubble()
//        {
//            Resource.Repository.DecrementRef(null, mMesh);
//        }

//        public override void Update(double elapsedSeconds, RenderingContext context)
//        {
//            if (context.Viewport.TVIndex != 1) return;
//            base.Update(elapsedSeconds, context);
//            UpdateField(context);
//            mLastCameraPosition = context.Position;
//        }


//        public override void Render(RenderingContext context)
//        {
//            base.Render(context);
//            // the entity is always at 0,0,0 with respect to the camera since the dust particles are already in 
//            // camera space coordinates
//            throw new NotImplementedException("Since adding Entity.Model.Render() need to update");
//            //mDummyEntityt.Render(new Vector3d(0, 0, 0), new SwitchNodeOptions(), FX_SEMANTICS.FX_CAMERA_BUBBLE);
//        }

//        //public override void Render(Keystone.Cameras.Camera camera, Vector3d contextPosition)
//        //{
//        //    base.Render(camera, contextPosition);
//        //    // the entity is always at 0,0,0 with respect to the camera since the dust particles are already in 
//        //    // camera space coordinates
//        //    mDummyEntityt.Render( new Vector3d(0, 0, 0), FX_SEMANTICS.FX_CAMERA_BUBBLE);
//        //}


//        // TODO: this dustfield should be a bindable node i suspect?  like our Background3d?
//        // then we can use the Viewpoint velocity (or RenderingContext velocity) to determine how 
//        // fast to update our dust field and whether to 
//        //
//        // for each vertex in the mesh, we need to determine if it's offscreen and if so, replot it's point
//        // such a starfield should be added to the scene's root node and then the camera
//        // should get a reference so it can control it...
//        // Or i treat it like an environmental FX like a skybox
//        // FX_CAMERA_BUBBLE_EFFECT
//        // http://www.youtube.com/watch?v=ZvwIlcH8Vbc
//        private void UpdateField(RenderingContext context)
//        {
//            TVMesh m = CoreClient._CoreClient.Globals.GetMeshFromID(mMesh.TVIndex);

//            // http://creativejs.com/tutorials/three-js-part-1-make-a-star-field/
//            for (int i = 0; i < m.GetVertexCount(); i++)
//            {
//                float fX, fY, fZ, d;
//                fX = fY = fZ = d = 0;
//                int color = 0;
//                m.GetVertex(i, ref fX, ref fY, ref fZ, ref d, ref d, ref d, ref d, ref d, ref d, ref d, ref color);
//                bool offscreen = true;

//                Vector3d dustPos = new Vector3d((double)fX, (double)fY, (double) fZ)  ;
//                // TODO: we want to add new particles from the direction we're traveling such as when we're traveling
//                // backwards but facing forwards, new particles must emerge from close to near plane
//                // and then be seen traveling backwards.
//                // not sure how that's done.
//                //
//                // i mean this is part of what we're dealing with here is how to restrict the plotting on new particles
//                // to just beyond , but yet then to get rid of the ones that are beyond in the opposite hemisphere of where we're moving

//                // So a hemisphere perpendicular to where we're going and with a hemisphere bulge as long as velocity
//                Vector3d origin = new Vector3d();
//                Culling.ViewFrustum frustum = new Keystone.Culling.ViewFrustum(false, true, false);
//                frustum.Update(context.Camera.Near, context.Camera.Far, context.Camera.FOVRadians,
//                    origin, context.Camera.LookAt, context.Camera.View, context.Camera.Projection,
//                    context.Viewport.Width, context.Viewport.Height);
                
//                if (frustum.Intersects (new Vector3d[]{dustPos}) == IntersectResult.OUTSIDE )
//                {
//                    // get a random angle between 0 and 2pi
//                    double theta = mRand.NextDouble() * (Math.PI * 2);
//                    System.Diagnostics.Debug.Assert(theta >= 0 && theta <= Math.PI * 2);
//                    // get a random value between 0 and +1  <-- 0 to ensure it's in front of the camera
//                    double u = -1 + mRand.NextDouble() * (1 - -1);
//                    System.Diagnostics.Debug.Assert(u >= -1 && u <= 1);

//                    double x, y, z;
//                    double uSquared = u * u;

//                    x = Math.Cos(theta) * Math.Sqrt(1 - uSquared);
//                    y = Math.Sin(theta) * Math.Sqrt(1 - uSquared);
//                    z = u;

//                    Vector3d vec = new Vector3d(x, y, z);
//                    vec *= mRadius;
//                    // move the vertex from local space to camera space coordinates
//                    // note: since the field follows the camera, model space and camera space are same thing
//                    vec += context.Position ;

//                    // compute a new color <-- not really doable with a single mesh pointlist primitive.  would be doalbe
//                    // with minimesh and maybe thats something will try for comparison.
//                    //double Brightness =  distanceFroMCamera * cameraVelocity;
//                    //Red = .Red + Brightness;
//                    //Grn = .Grn + Brightness;
//                    //Blu = .Blu + Brightness;
//                    //if (Red < 0) Red = 0;
//                    //if (Grn < 0) Grn = 0;
//                    //if (Blu < 0) Blu = 0;
//                    //if (Red > 255) Red = 255;
//                    //if (Grn > 255) Grn = 255;
//                    //if(Blu > 255) Blu = 255;
//                    m.SetVertex(i, (float)vec.x, (float)vec.y, (float)vec.z, 0, 0, 0, 0, 0, 0, 0, color);
//                }
//            }

//            //mDummyEntityt.Translation = camera.Position;
//        }
//    }
//}
