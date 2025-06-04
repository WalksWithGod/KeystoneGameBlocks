//using System;
//using Keystone.Appearance;
//using Keystone.Cameras;
//using Keystone.Elements;
//using Keystone.Shaders;
//using Keystone.Types;
//using MTV3D65;

//namespace Keystone.FX
//{
//    public class FXSkyGradient : FXBase
//    {
//        private const string SHADER_PATH = "Shaders\\SkyGradient\\SkyGradient.fx";

//        // Readonly, constructor-set settings
//        private readonly InterpolationModes mInterpolationMode = InterpolationModes.Hermite;

//        // Instance members
//        private SkyGradient _shader;
//        private SimpleModel _model;
//        private TVMesh _tvmesh; // direct access to the mesh
//        private Material mMaterial;

//        // Realtime settings
//        private double mScale = 100; //scale

//        private TV_COLOR mCloudsColor;
//        private Texture mCloudsTexture;
//        private TV_2DVECTOR mCloudsTiling, mCloudsTranslation;
//        private TV_2DVECTOR mTranslation;
//        private TV_COLOR mBelowHorizonColor, mHorizonColor, mOverHorizonColor, mMidSkyColor, mZenithColor;

//        // The gradient color interpolation modes
//        public enum InterpolationModes
//        {
//            Linear, // Linear does a simple LERP
//            Hermite // Hermite uses the "smoothstep" semantic for a smoother yet slower transition
//        }


//        // Constructs the object with all init-time settings
//        public FXSkyGradient(InterpolationModes interpolationMode, SimpleModel skysphere, Texture cloudTexture,
//                             Material mat)
//        {
//            _semantic = FX_SEMANTICS.FX_SKY;
//            _layout = FXLayout.Background;
//            // constructor helps us know to pass in a cloud texture and material seperately.
//            // honestly, i wonder if maybe we should create the Mesh here too then...  and frankly
//            // the texture and materials and then just let user change them externally... hrm. 
//            // SkyScattering is a similiar one i think because those sun/moon/domes arent added
//            // to the scene itself.  
//            // i dunno.  I still need more comprehensive 
//            // Hrm, one thing that can be done is to allow this FX to create the object
//            // and then allow the user to retrieve the references so they can be added to the scene?
//            // But one problem with that is allowing it to be edited via Pick selection 
//            // and materials or textures removed that arent intended...
//            // Water is really the same way...
//            // There are things like Normal maps that are required and such... 

//            _model = skysphere;
//            mInterpolationMode = interpolationMode;
//            mMaterial = mat;
//            Initialize(); // init before we make calls that rely on non null mesh and shader
//            CloudsTexture = cloudTexture;
//        }

//        private void Initialize()
//        {
//            string Define;
//            if (mInterpolationMode == InterpolationModes.Linear)
//                Define = "#define LINEAR_INTERPOLATION";
//            else
//                Define = "#define HERMITE_INTERPOLATION";

//            _shader = new SkyGradient("SkyGradient", SHADER_PATH, new string[1] {Define});

//            _tvmesh = Core._CoreClient.Globals.GetMeshFromID(_model.Geometry.TVIndex);
//            if (_tvmesh == null) throw new Exception("SkyDome Model contains no geometry (TVMesh is null).");
//            _model.Shader = _shader;
//            // note: this shader is applied during traversal and even though this Model isnt directly (yet?)
//            // placed in the scene graph, we do call the Traversal method against this model to render it.

//            Material = mMaterial;

//            TV_COLOR White = new TV_COLOR(1, 1, 1, 1);
//            BelowHorizonColor = new TV_COLOR(0.345F, 0.51372549F, 0.6392156862745F, 1);
//            HorizonColor = new TV_COLOR(0.9294F, 0.898F, 0.8156862745F, 1);
//            OverHorizonColor = new TV_COLOR(0.88235294F, 1, 1, 1);
//            MidSkyColor = new TV_COLOR(0.67843F, 1, 1, 1);
//            ZenithColor = new TV_COLOR(0.2549F, 0.6941F, 1, 1);

//            CloudsColor = White;
//            CloudsTranslation = new TV_2DVECTOR(1, -5);
//            CloudsTiling = new TV_2DVECTOR(1.5F, 1.5F);
//        }

//        // The updating method centers the sphere on the player (can't get AttachTo to work well)
//        // and updates the translation
//        public override void Update(int elapsed, Camera camera)
//        {
//            _model.Position = camera.Position;

//            mTranslation += (1.0F/500000)*elapsed*mCloudsTranslation;
//            // mCloudsTranslation should be a 2d wind vector with the speed included. 
//            // technically, we should have an upper and lower wind vectors since wind is usually slower at ground level than at clould level?
//            if (mTranslation.x >= 1) mTranslation.x -= 1;
//            if (mTranslation.x <= 0) mTranslation.x += 1;
//            if (mTranslation.y >= 1) mTranslation.y -= 1;
//            if (mTranslation.y <= 0) mTranslation.y += 1;
//            _shader.CloudsTranslation = mTranslation;
//        }

//        public override void Render(Camera camera)
//        {
//            _model.Render(camera.Position, _semantic);
//        }

//        // Changes the size of the skysphere... 
//        // A bit useless since z-writes are disabled and it's always rendered at the back
//        public double Scale
//        {
//            get { return mScale; }
//            set
//            {
//                mScale = value;
//                _model.Scale = new Vector3d(mScale, mScale, mScale);
//            }
//        }

//        // The clouds' color
//        public TV_COLOR CloudsColor
//        {
//            get { return mCloudsColor; }
//            set
//            {
//                mCloudsColor = value;
//                _shader.CloudsColor = mCloudsColor;
//            }
//        }

//        // The clouds' tiling factor
//        public TV_2DVECTOR CloudsTiling
//        {
//            get { return mCloudsTiling; }
//            set
//            {
//                mCloudsTiling = value;
//                _shader.CloudsTiling = mCloudsTiling;
//            }
//        }

//        // The sky's color at horizon
//        public TV_COLOR HorizonColor
//        {
//            get { return mHorizonColor; }
//            set
//            {
//                mHorizonColor = value;
//                mMaterial.Ambient = new Color(mHorizonColor.r, mHorizonColor.g, mHorizonColor.b, mHorizonColor.a);
//            }
//        }

//        // The sky's color just over the horizon
//        public TV_COLOR OverHorizonColor
//        {
//            get { return mOverHorizonColor; }
//            set
//            {
//                mOverHorizonColor = value;
//                mMaterial.Diffuse =
//                    new Color(mOverHorizonColor.r, mOverHorizonColor.g, mOverHorizonColor.b, mOverHorizonColor.a);
//            }
//        }

//        // The sky's color at midsky
//        public TV_COLOR MidSkyColor
//        {
//            get { return mMidSkyColor; }
//            set
//            {
//                mMidSkyColor = value;
//                mMaterial.Specular = new Color(mMidSkyColor.r, mMidSkyColor.g, mMidSkyColor.b, mMidSkyColor.a);
//            }
//        }

//        // The sky's color at zenith
//        public TV_COLOR ZenithColor
//        {
//            get { return mZenithColor; }
//            set
//            {
//                mZenithColor = value;
//                mMaterial.Emissive = new Color(mZenithColor.r, mZenithColor.g, mZenithColor.b, mZenithColor.a);
//            }
//        }

//        // The sky's color just below the horizon
//        public TV_COLOR BelowHorizonColor
//        {
//            get { return mBelowHorizonColor; }
//            set
//            {
//                mBelowHorizonColor = value;
//                //TODO: pass in Fog if we want to allow this to impact it
//                //Atmosphere.Fog_SetColor(BelowHorizonColor.r, BelowHorizonColor.g, BelowHorizonColor.b);
//            }
//        }

//        // The clouds' texture (integer, not the name or filepath)
//        public Texture CloudsTexture
//        {
//            get { return mCloudsTexture; }
//            set
//            {
//                mCloudsTexture = value;
//                _tvmesh.SetTexture(mCloudsTexture.TVIndex);
//            }
//        }

//        public Material Material
//        {
//            get { return mMaterial; }
//            set
//            {
//                mMaterial = value;
//                _tvmesh.SetMaterial(mMaterial.TVIndex);
//            }
//        }

//        // The clouds' translation factor, how much they scroll/move
//        public TV_2DVECTOR CloudsTranslation
//        {
//            get { return mCloudsTranslation; }
//            set { mCloudsTranslation = value; }
//        }
//    }
//}
