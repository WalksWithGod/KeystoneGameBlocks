using Keystone.Cameras;
using Keystone.RenderSurfaces;
using MTV3D65;

namespace Keystone.Shaders
{
    public class OceanShader : Shader
    {
        private TV_PLANE _waterPlane;


        private bool _foamSprays;
        private bool _detailNormalMapping;
        private bool _blinnPhongSpecular;

        //texture holders. It's ok these are just id's because only the FXProvider owns the actual Texture objects
        private int _texFoamLookup;
        private int _texFoamMask;
        private int _texDetailNormal;
        private int _texPhongLookup;
        private int _texFresnelLookup;


        private TV_COLOR skyAverageColor = new TV_COLOR(0.5f, 0.62f, 0.77f, 1);
        private float waveAnimationSpeed = 1.5f;
        private TV_2DVECTOR waveMovementDirection = new TV_2DVECTOR(0, 1);
        private float waveMovementSpeed = 6;
        private float waveAmplitude = 0.7f;
        private TV_COLOR waterColor = new TV_COLOR(0.0125f, 0.213f, 0.253f, 1);


        //public OceanShader(string filepath, string[] defines, TV_PLANE waterplane)
        //    : base( filepath, defines)
        //{
        //    _waterPlane = waterplane;
        //    // setup textures and RS
        //    _rs =
        //        RenderSurface.CreateRS(RSResolution.R_256x256,
        //                               CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8, true);
        //}

        protected OceanShader(string id, string resourcePath)
            : base(id)
        {
        }

        public override object Traverse(Traversers.ITraverser target, object data)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        internal override Traversers.ChildSetter GetChildSetter()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public void BeginRender(Camera reflectionCamera)
        {
            // ocean shader requires the reflected camera we've constructed to be set into it 
            _rs.SetNewCamera(reflectionCamera.TVCamera);
            _rs.StartRender(false);
            // TODO: now render the atmosphere and sun/moon / (maybe not fog) but all really far meshes 
        }

        // NOTE: Once this custom projection is set, then when i call MiddleRender and use TreeRenderer and custom frustum
        // the frustum planes are updated using this new camera matrix.  Then when we render the scene again regularly
        // after EndRender, its reset and the frustum planes are updated again.  Works for free!
        // The only problem now is that we arent able to properly cull patches of water because it runs all under the terrain.
        // NOTE: When we do try to cull water (using our water patches to generated perfectly sculpted sizes)
        // we can still share the shader's update as a single call and not once per patch.  This is because the Oceans are
        // always going to be at same height within visible range (or at least the game should be designed this way)
        // so that they are seperate planes as opposed to one huge plane makes zero difference.  This is why 
        // it'd be important to call that shader's update with a frame counter so that if its already updated for that frame
        // it can be skipped and we can just go straight to the tvmesh.Renders() for the seperate patches... bypassing all this 
        // Begin/Middle/EndRender for subsequent visible patches.
        public void MiddleRender()
        {
        }

        //' finally end the render surface reflection rendering and restore original matrices 
        public void EndRender()
        {
            _rs.EndRender();
            // _rs.SetNewCamera(null);
        }

        public int TexFoamLookUp
        {
            get { return _texFoamLookup; }
            set
            {
                _texFoamLookup = value;
                EnableFoamSprays = _foamSprays;
            }
        }

        public int TexFoamMask
        {
            get { return _texFoamMask; }
            set
            {
                _texFoamMask = value;
                EnableFoamSprays = _foamSprays;
            }
        }

        public int TexDetailNormal
        {
            get { return _texDetailNormal; }
            set
            {
                _texDetailNormal = value;
                EnableDetailNormalMapping = _detailNormalMapping;
            }
        }

        public int TexBlinnPhongSpecular
        {
            get { return _texPhongLookup; }
            set
            {
                _texPhongLookup = value;
                EnableBlinnPhongSpecular = _blinnPhongSpecular;
            }
        }

        public int TexFresnalLookup
        {
            get { return _texFresnelLookup; }
            set
            {
                _texFresnelLookup = value;
                _tvShader.SetEffectParamTexture("texFresnelLookup", _texFresnelLookup);
            }
        }

        public bool EnableFoamSprays
        {
            get { return _foamSprays; }
            set
            {
                _foamSprays = value;
                _tvShader.SetEffectParamTexture("texFoamLookup", _texFoamLookup);
                _tvShader.SetEffectParamTexture("texFoamMask", _texFoamMask);
            }
        }

        public bool EnableDetailNormalMapping
        {
            get { return _detailNormalMapping; }
            set
            {
                _detailNormalMapping = value;
                _tvShader.SetEffectParamTexture("texDetailNormal", _texDetailNormal);
            }
        }

        public bool EnableBlinnPhongSpecular
        {
            get { return _blinnPhongSpecular; }
            set
            {
                _blinnPhongSpecular = value;
                if (!_blinnPhongSpecular)
                    _tvShader.SetEffectParamTexture("texPhongLookup", _texPhongLookup);
            }
        }


        //NOTE: These properties must be the same for every water patch because that uses a particular FXWaterOcean instance
        // our goal is to be able to use a single FXProvider for all patches that wish to use
        // this water shader.  However, we also want to be able to share the Reflection and Refraction
        // result textures so that we only have to compute them once regardless of how many water patches
        // are subscribed to use this FX. 

        #region Shader Effect Parameter Properties

        public TV_COLOR WaterColor
        {
            get { return waterColor; }
            set
            {
                waterColor = value;
                _tvShader.SetEffectParamColor("waterColor", waterColor);
            }
        }

        public float WaveAmplitude
        {
            get { return waveAmplitude; }
            set
            {
                waveAmplitude = value;
                if (waveAmplitude < 0) waveAmplitude = 0;
                if (waveAmplitude > 1) waveAmplitude = 1;
                _tvShader.SetEffectParamFloat("waveAmplitude", waveAmplitude);
            }
        }

        public float WaveMovementSpeed
        {
            get { return waveMovementSpeed; }
            set
            {
                waveMovementSpeed = value;
                if (waveMovementSpeed < 0) waveMovementSpeed = 0;
                _tvShader.SetEffectParamFloat("waveMovementSpeed", waveMovementSpeed);
            }
        }

        public TV_2DVECTOR WaveMovementDirection
        {
            get { return waveMovementDirection; }
            set
            {
                waveMovementDirection = value;
                CoreClient._CoreClient.Maths.TVVec2Normalize(ref waveMovementDirection, waveMovementDirection);
                _tvShader.SetEffectParamVector2("waveMovementDirection", waveMovementDirection);
            }
        }

        public float WaveAnimationSpeed
        {
            get { return waveAnimationSpeed; }
            set
            {
                waveAnimationSpeed = value;
                if (waveAnimationSpeed < 0) waveAnimationSpeed = 0;
                _tvShader.SetEffectParamFloat("waveAnimationSpeed", waveAnimationSpeed);
            }
        }

        public TV_COLOR SkyAverageColor
        {
            get { return skyAverageColor; }
            set
            {
                skyAverageColor = value;
                _tvShader.SetEffectParamColor("skyAverageColor", skyAverageColor);
            }
        }

        #endregion
    }
}