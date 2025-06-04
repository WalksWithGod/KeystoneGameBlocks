using System;
using System.IO;
using Keystone.Cameras;
using Keystone.Shaders;
using Keystone.Types;
using MTV3D65;

namespace Keystone.FX
{
    public class FXBloom : FXBase
    {
        private TVRenderSurface mPrevFrameMainBufferRS;
        private BloomRenderSurfaceSet[] mDownscaledRSSets;
        private TVRenderSurface mCombinedRS;

        private TVShader mHighPassShader;
        private TVShader mCombineShader;
        private GaussianDistribution mGaussianBlurShader;

        private float _bloomFactor;
        private float _highPassThreshold;
        private float _bloomWidth;
        private float _sceneIntensity;
        private int mBloomSteps = 2;

        public readonly string PATH_SHADERS = Path.Combine (Core._Core.ModsPath, @"caesar\Shaders\");

        public FXBloom()
        {
            _semantic = FX_SEMANTICS.FX_GLOW;

            InitializeShaders();
            InitializeRenderSurfaces(-1, -1); 
        }

        private void InitializeShaders()
        {
            string shaderText;

            mHighPassShader = CoreClient._CoreClient.Scene.CreateShader("HighPass");
            shaderText = File.ReadAllText(PATH_SHADERS + "HighPass.fx");
            mHighPassShader.CreateFromEffectString(shaderText);
            HighPassThreshold = .5f;

            int taps = 5;
            mGaussianBlurShader = new GaussianDistribution("GaussianBlur", PATH_SHADERS + "GaussianBlur.fx", taps);

            mCombineShader = CoreClient._CoreClient.Scene.CreateShader("Combine");
            shaderText = File.ReadAllText(PATH_SHADERS + "Combine.fx");
            shaderText = string.Format("#define BLOOM_STEPS {0}", mBloomSteps) +
                         Environment.NewLine + shaderText;
            mCombineShader.CreateFromEffectString(shaderText);

            BloomWidth = .5f;
            SceneIntensity = 1;
            BloomFactor = 1f;
        }

        private void UninitializeRendersurfaces()
        {
            mPrevFrameMainBufferRS.Destroy();
            mCombinedRS.Destroy();

            for (int i = 0; i < mBloomSteps; i++)
                mDownscaledRSSets[i].Dispose();
        }

        private void InitializeRenderSurfaces(int width, int height)
        {
            mPrevFrameMainBufferRS = CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(width, height,
                                                                                    CONST_TV_RENDERSURFACEFORMAT.
                                                                                        TV_TEXTUREFORMAT_X8R8G8B8,
                                                                                    false, true);

            mCombinedRS = CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(width, height,
                                                              CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8,
                                                              false, true);

        	TVCamera camera = CoreClient._CoreClient.CameraFactory.CreateCamera ();
        	mCombinedRS.SetNewCamera (camera);
        	
            mDownscaledRSSets = new BloomRenderSurfaceSet[mBloomSteps];
            for (int i = 0; i < mBloomSteps; i++)
            {
                mDownscaledRSSets[i] = new BloomRenderSurfaceSet((int)Math.Pow(2, i + 2),
                                                              (i == 0)
                                                                  ? mPrevFrameMainBufferRS.GetTexture()
                                                                  : mDownscaledRSSets[i - 1].BlurredRS.GetTexture(),
                                                               mGaussianBlurShader, mHighPassShader);

            	camera = CoreClient._CoreClient.CameraFactory.CreateCamera ();
                mDownscaledRSSets[i].BlurredRS.SetNewCamera(camera);
                mDownscaledRSSets[i].TempRS.SetNewCamera(camera);
            }
            
            for (int i = 0; i < mBloomSteps; i++)
                mCombineShader.SetEffectParamTexture("texBloom" + i, mDownscaledRSSets[i].BlurredRS.GetTexture());

            mCombineShader.SetEffectParamTexture("texFrameBuffer", mPrevFrameMainBufferRS.GetTexture());
        }

        public override void Register(IFXSubscriber subscriber)
        {
            throw new ArgumentException("Subscribers not needed or allowed.");
        }

        public override void RenderBeforeClear(RenderingContext context)
        {
            // NOTE: The render target seems to always be the same size even if i try to 
            // create one the exact dimensions of the viewport
            int renderTargetWidth = mPrevFrameMainBufferRS.GetWidth();
            int renderTargetHeight = mPrevFrameMainBufferRS.GetHeight();

            if (mPrevFrameMainBufferRS.GetWidth() != context.Viewport.Width || mPrevFrameMainBufferRS.GetHeight() != context.Viewport.Height)
            {
               // UninitializeRendersurfaces();
               // InitializeRenderSurfaces(context.Viewport.Width, context.Viewport.Height);
            }

            // halfTexel to find offset for center of texel.  This is often needed when rendering fullscreen quads or camera facing billboards
            TV_2DVECTOR halfTexelSize = new TV_2DVECTOR(0.5f / context.Viewport.Width, 0.5f/ context.Viewport.Height);
            TV_2DVECTOR[] texCoordOffsets = new TV_2DVECTOR[mDownscaledRSSets.Length];

            // the 
            for (int i = 0; i < mDownscaledRSSets.Length; i++)
            {
                // make sure to pass the rendertarget width and height, NOT the viewport
                mDownscaledRSSets[i].Update(context, renderTargetWidth, renderTargetHeight, i == 0);
                texCoordOffsets[i] = halfTexelSize*mDownscaledRSSets[i].DownscalingFactor;
            }

            mCombineShader.SetEffectParamVector2("frameBufferTexCoordOffset", halfTexelSize);
            mCombineShader.SetEffectParamVectorArray2("bloomTexCoordOffset", texCoordOffsets, texCoordOffsets.Length);

			mCombinedRS.GetCamera().SetCamera (0f, 0f, 0f, (float)context.LookAt.x, (float)context.LookAt.y, (float)context.LookAt.z);
            mCombinedRS.StartRender();
            CoreClient._CoreClient.Screen2D.Draw_FullscreenQuadWithShader(mCombineShader, 0, 0, 1, 1);
            mCombinedRS.EndRender();
        }

        public override void RenderPost(RenderingContext context)
        {
            //IMPORTANT: Sky must be rendered as first thing in scene or else this causes all sorts of artifacts.
            // NOTE: when using multiple viewports, the last frame main buffer which is used in the above PreRender()
            // will inadvertantly be using the frame buffer from the wrong viewport!
            mPrevFrameMainBufferRS.BltFromMainBuffer();
            //lastFrameMainBuffer.SaveTexture("g:\\stuff\\zzzMain.dds", CONST_TV_IMAGEFORMAT.TV_IMAGE_DDS);

            // TODO: this must use the current viewport's dimensions and not this one i set in constructor
            CoreClient._CoreClient.Screen2D.Action_Begin2D();
            CoreClient._CoreClient.Screen2D.Draw_Texture(mCombinedRS.GetTexture(), 0, 0, context.Viewport.Width - 1,
                                             context.Viewport.Height - 1);
            //combined.SaveTexture("g:\\stuff\\zzzz.dds", CONST_TV_IMAGEFORMAT.TV_IMAGE_DDS);
            CoreClient._CoreClient.Screen2D.Action_End2D();
        }


        public float BloomFactor
        {
            get { return _bloomFactor; }
            set
            {
                _bloomFactor = value;

                if (_bloomFactor < 0) _bloomFactor = 0;

                mCombineShader.SetEffectParamFloat("bloomFactor", _bloomFactor);
            }
        }

        public float HighPassThreshold
        {
            get { return _highPassThreshold; }
            set
            {
                _highPassThreshold = value;

                if (_highPassThreshold < 0)
                    _highPassThreshold = 0;
                else if (_highPassThreshold > 1)
                    _highPassThreshold = 1;

                mHighPassShader.SetEffectParamFloat("threshold", _highPassThreshold);
            }
        }

        public float BloomWidth
        {
            get { return _bloomWidth; }
            set
            {
                _bloomWidth = value;

                if (_bloomWidth < 0)
                    _bloomWidth = 0;
                else if (_bloomWidth > 1)
                    _bloomWidth = 1;

                mCombineShader.SetEffectParamFloat("bloomWidth", _bloomWidth);
            }
        }

        public float SceneIntensity
        {
            get { return _sceneIntensity; }
            set
            {
                _sceneIntensity = value;

                if (_sceneIntensity < 0)
                    _sceneIntensity = 0;
                else if (_sceneIntensity > 1)
                    _sceneIntensity = 1;

                mCombineShader.SetEffectParamFloat("sceneIntensity", _sceneIntensity);
            }
        }

        private struct BloomRenderSurfaceSet
        {
            public TVRenderSurface TempRS;
            public TVRenderSurface BlurredRS;
            public int DownscalingFactor;
            public int SourceTexture;

            private GaussianDistribution _gaussian;
            private TVShader _highPass;

            internal BloomRenderSurfaceSet(int downscalingFactor, int sourceTexture, 
                                           GaussianDistribution gaussian, TVShader highPass)
            {
                DownscalingFactor = downscalingFactor;
                SourceTexture = sourceTexture;

                _gaussian = gaussian;
                _highPass = highPass;

                TempRS = CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(-1, -1,
                                                              CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8,
                                                              false,
                                                              true, 1.0f/DownscalingFactor);

                BlurredRS = CoreClient._CoreClient.Scene.CreateRenderSurfaceEx(-1, -1,
                                                                 CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8,
                                                                 false,
                                                                 true, 1.0f/DownscalingFactor);
            }

            internal void Dispose()
            {
                TempRS.Destroy();
                BlurredRS.Destroy();
                // NOTE: We do not dispose the shaders because those are managed by the FXBloom itself
            }

            internal void Update(RenderingContext tcontext, int renderTargetWidth, int renderTargetHeight, bool doHighPass)
            {
                // TODO: i think here the context.Viewport.Width and Height do not match
                // the actual render target size
                TV_2DVECTOR renderTargetSize = new TV_2DVECTOR(renderTargetWidth, renderTargetHeight) /
                                               DownscalingFactor;
                TV_2DVECTOR halfTexelSize = new TV_2DVECTOR(0.5f / renderTargetWidth, 0.5f / renderTargetHeight) *
                                            DownscalingFactor;

                TempRS.GetCamera().SetCamera (0f, 0f, 0f, (float)tcontext.LookAt.x, (float)tcontext.LookAt.y, (float)tcontext.LookAt.z);
                
                // temp only requires the source texture and thus never cares about any camera
                TempRS.StartRender();
                if (doHighPass)
                    CoreClient._CoreClient.Screen2D.Draw_FullscreenQuadWithShader(_highPass, halfTexelSize.x, halfTexelSize.y,
                                                                      1 + halfTexelSize.x, 1 + halfTexelSize.y,
                                                                      SourceTexture);
                else
                    CoreClient._CoreClient.Screen2D.Draw_Texture(SourceTexture, 0, 0,
                                                     (int) renderTargetSize.x - 1, (int) renderTargetSize.y - 1);
                TempRS.EndRender();

                _gaussian.TVShader.SetEffectParamVector2("texelSize", halfTexelSize*2);

                _gaussian.TVShader.SetPassEnable(0, true);
                _gaussian.TVShader.SetPassEnable(1, false);

                // blur only requires the Temp's texture and so never cares about any camera
                BlurredRS.StartRender();
                CoreClient._CoreClient.Screen2D.Draw_FullscreenQuadWithShader(_gaussian.TVShader, halfTexelSize.x, halfTexelSize.y,
                                                                  1 + halfTexelSize.x, 1 + halfTexelSize.y,
                                                                  TempRS.GetTexture());
                BlurredRS.EndRender();

                TempRS.BltFromRenderSurface(BlurredRS);

                _gaussian.TVShader.SetPassEnable(0, false);
                _gaussian.TVShader.SetPassEnable(1, true);

                BlurredRS.StartRender();
                CoreClient._CoreClient.Screen2D.Draw_FullscreenQuadWithShader(_gaussian.TVShader, halfTexelSize.x, halfTexelSize.y,
                                                                  1 + halfTexelSize.x, 1 + halfTexelSize.y,
                                                                  TempRS.GetTexture());
                BlurredRS.EndRender();
            }
        }
    }
}