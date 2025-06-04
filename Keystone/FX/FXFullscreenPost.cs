using System;
using Keystone.Cameras;
using Keystone.Shaders;
using Keystone.Types;
using MTV3D65;

namespace Keystone.FX
{
    public class FXFullscreenPost : FXBase
    {
        private TVRenderSurface _rs;
        private TVViewport viewport;
        private Shader _shader;

        // negative is like a photo neg
        // sepia is like a brownish western look
        // desaturate.fx makes it look gloomy outside... like a stormy day. really cool.
        // edge.fx is like a strange rotoscoping outline effect
        // frost.fx is like looking through a thick and unevenly warped glass pane
        // radialblur.fx looks like you're traveling amazingly fast.  Thus is looks weird if you're not moving fast and with a very fast framerate

        public readonly string PATH_SHADERS = System.IO.Path.Combine (Core._Core.DataPath, @"Shaders\Post\");

        public FXFullscreenPost(TVViewport vp, RenderCallback cb)
        {
            _semantic = FX_SEMANTICS.FX_GLOW; // for now lets just treat like glow shader
            mRenderCB = cb;

            viewport = vp;

            _rs = CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(-1, -1,
                                                         CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8, 
                                                         false, true);
			
            // TODO: shader path is not consistantly used, needs to be like meshes and accommodate data path and zip
            _shader = (Shader)Resource.Repository.Create (PATH_SHADERS + "desaturate.fx" + "_fullscreenpost_desaturate");         }

        public override void Register(IFXSubscriber subscriber)
        {
            throw new ArgumentException("Subscribers not needed or allowed.");
        }

        public override void RenderBeforeClear(RenderingContext context)
        {
            _rs.SetNewCamera(context.Camera.TVCamera);
            _rs.StartRender();
            mRenderCB.Invoke();
            _rs.EndRender();
            _rs.SetNewCamera(null);
            _shader.TVShader.SetEffectParamTexture("SceneTexture", _rs.GetTexture());
        }

        public override void RenderPost(RenderingContext context)
        {
            // TODO:  use the current viewport's dimensions and not this one i set in constructor?
            CoreClient._CoreClient.Screen2D.Action_Begin2D();
            CoreClient._CoreClient.Screen2D.Draw_FullscreenQuadWithShader(_shader.TVShader, 0, 0, 1, 1, 0, 0);
            CoreClient._CoreClient.Screen2D.Action_End2D();
        }
    }
}