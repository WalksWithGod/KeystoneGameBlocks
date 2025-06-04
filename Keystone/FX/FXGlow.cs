using System;
using Keystone.Cameras;
using Keystone.RenderSurfaces;
using Keystone.Types;
using MTV3D65;

namespace Keystone.FX
{
    // This gives our FXWaterOcean free alpha.
    public class FXGlow : FXBase
    {
        private Vector3d mPosition;
        private Vector3d mLookat;
        private RenderSurface mRS;
        private TVGraphicEffect _glowEffect;

        private TVCamera mCamera;
        
        // eg: FXGlow glow = new FXGlow(RSResolution.R_256x256, new Color(1, 1, 1, 1), 0.25f, 1, context.RenderScene);
        // WARNING: the offsetScale will visually affect far away things more than near so with far away things
        // you may end up with a "double vision" effect.  If this is the case, some entities perhaps we flag as not
        // be renderable for gloweffect.
        public FXGlow(RSResolution res, Color tint, float intensity, float offsetScale, RenderCallback cb)
        {
            _semantic = FX_SEMANTICS.FX_GLOW;

            if (cb == null) throw new ArgumentNullException();
            mRenderCB = cb;

            mCamera = CoreClient._CoreClient.CameraFactory.CreateCamera ();
            
            _glowEffect = new TVGraphicEffect();
            mRS = RenderSurface.CreateRS(res, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT, true);
            mRS.SetNewCamera (mCamera);
            
            _glowEffect.InitGlowEffect(mRS.RS);
            _glowEffect.SetGlowParameters(new TV_COLOR(tint.r, tint.g, tint.b, tint.a), intensity, offsetScale);
        }

        public override void Register(IFXSubscriber subscriber)
        {
            throw new ArgumentException("Subscribers not needed or allowed.");
        }


        public override void RenderBeforeClear(RenderingContext context)
        {
            // TODO: this is strange.  It seems as though if I dont set this camera manually, TV3D will configure it's settings for the first
            //       viewport camera, and then on the second viewport it wont update it?
            mPosition = context.Position;
            mLookat = context.Camera.LookAt;
            //System.Diagnostics.Trace.WriteLine(camera.LookAt.x + "," + camera.Translation.x);
            // TODO: shouldn't this be at origin camera rendering?
//            mRS.RS.SetCamera((float)position.x, (float)position.y, (float)position.z,
//                             (float)mLookat.x, (float)mLookat.y,(float)mLookat.z);
            
            mCamera.SetCamera(0f, 0f, 0f, (float)mLookat.x, (float)mLookat.y,(float)mLookat.z);

            mRS.StartRender(false);
            // NOTE: We do not render water here.  Water only ever gets rendered once 
            // This has the advantage of giving us free alpha blending of Zak's FXWaterOcean.
            mRenderCB.Invoke();
            
            mRS.EndRender();

            _glowEffect.UpdateGlow();
        }

        public override void RenderPost(RenderingContext context)
        {
            _glowEffect.DrawGlow();

        }
        
        // dispose
		// _glowEffect.UnloadGlowEffect
		
    }
}