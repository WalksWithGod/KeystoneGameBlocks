//using System;
//using System.Collections.Generic;
//using Keystone.Cameras;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.RenderSurfaces;
//using Keystone.Shaders;
//using MTV3D65;

//namespace Keystone.FX
//{
//    // TODO: Would be nice to have caustics

//    //http://www.opengl.org/resources/code/samples/mjktips/caustics/

//    //http://www.cs.umbc.edu/~jbarcz1/causticvolume.pdf

//    //http://www.gamasutra.com/gdce/2001/jensen/jensen_01.htm

//    //http://gameprog.it/hosted/typhoon/water.htm


//    // Zak's Ocean shader.  The way I've allowed for multiple subscribers
//    // to be managed by this one instance means that I can only update the camera
//    // and the shader once and share the results across all subscribers.
//    // in this way we can cut up our water to better conform to the coastline without
//    // running all underneat the terrain and thus allowing us to cull the water (and avoid the reflection update)
//    // easier when looking inland.
//    public class FXOceanWater : FXBase
//    {
//        public enum NormalMapMode
//        {
//            VolumeMap,
//            Dual2DMapLookup
//        }

//        private OceanShader _shader;
//        private Camera _reflectionCamera;

//        private readonly string PATH_SHADERS = Core._CoreClient.DataPath + @"Shaders\Water\";
//        private readonly string PATH_LOOKUP = Core._CoreClient.DataPath + @"Shaders\Water\Lookup\";
//        private readonly string PATH_WATER = Core._CoreClient.DataPath + @"Shaders\Water\Maps\";

//        // TODO: really none of this should be read only because all of them should be user configurable 
//        private readonly string FOAM_MASK_PATH = "FoamMask.dds";
//        private readonly string DETAIL_NORMAL_MAP_PATH = "DetailNormalMap.dds";

//        private readonly string FOAM_LOOKUP_PATH = "FoamLookUp.dds";
//        private readonly string FRESNEL_LOOKUP_PATH = "FresnelLookUp.dds";
//        private readonly string PHONG_LOOKUP_PATH = "PhongLookUp.dds";

//        private float _surfaceHeight;

//        // Fix for reflection glitches where water hits the land
//        private const float FIX_DISTANCE = 1F;

//        private TV_PLANE _waterPlane;


//        public FXOceanWater(RSResolution resolution, string name, string filepath, float surfaceHeight,
//                            bool blinnPhongSpecular, bool parallaxMapping, bool detailNormalMapping,
//                            bool troubledWaterEnhancements, bool foamSprays, NormalMapMode normalMapMode,
//                            RenderCallback renderCB)
//        {
//            _semantic = FX_SEMANTICS.FX_WATER_OCEAN;
//            _renderCB = renderCB;
//            _surfaceHeight = surfaceHeight;

//            _reflectionCamera = new StaticCamera(0, 0);

//            // rebuild the shader fx file in memory to include these defines
//            List<string> defines = new List<string>();

//            if (parallaxMapping)
//                defines.Add("#define PARALLAX_MAPPING");
//            if (detailNormalMapping)
//                defines.Add("#define DETAIL_NORMAL_MAPPING");
//            if (troubledWaterEnhancements)
//                defines.Add("#define TROUBLED_WATER_ENHANCEMENTS");
//            if (foamSprays)
//                defines.Add("#define FOAM_SPRAYS");

//            if (normalMapMode == NormalMapMode.VolumeMap)
//                defines.Add("#define VOLUME_NORMAL_MAP");
//            else
//                defines.Add("#define DUAL_2D_NORMAL_MAP");

//            if (blinnPhongSpecular)
//                defines.Add("#define LYON_SPECULAR_REFLECTION");
//            else
//                defines.Add("#define PHONG_SPECULAR_REFLECTION");

//            _waterPlane = new TV_PLANE(new TV_3DVECTOR (0, 1, 0), -_surfaceHeight + FIX_DISTANCE);
//            _shader = new OceanShader("oceanshader", PATH_SHADERS + "WaterShader3.fx", defines.ToArray(), _waterPlane);


//            _shader.TVShader.SetTimePeriod(200); //?

//            // Set the shader parameters.  NOTE: These textures ideally should be passed into this FXProvider as Texture object instances
//            // and then set to the shader
//            if (detailNormalMapping)
//                _shader.TexDetailNormal = Core._CoreClient.TextureFactory.LoadTexture(PATH_WATER + DETAIL_NORMAL_MAP_PATH);

//            if (!blinnPhongSpecular)
//                _shader.TexBlinnPhongSpecular = Core._CoreClient.TextureFactory.LoadTexture(PATH_LOOKUP + PHONG_LOOKUP_PATH);

//            if (foamSprays)
//            {
//                _shader.TexFoamLookUp = Core._CoreClient.TextureFactory.LoadTexture(PATH_LOOKUP + FOAM_LOOKUP_PATH);
//                _shader.TexFoamMask = Core._CoreClient.TextureFactory.LoadTexture(PATH_WATER + FOAM_MASK_PATH);
//            }

//            if (troubledWaterEnhancements)
//                _shader.SkyAverageColor = new TV_COLOR(0.5f, 0.62f, 0.77f, 1);

//            _shader.TexFresnalLookup = Core._CoreClient.TextureFactory.LoadTexture(PATH_LOOKUP + FRESNEL_LOOKUP_PATH);

//            TV_PLANE tmpPlane = _waterPlane;
//            Core._CoreClient.Maths.TVPlaneNormalize(ref _waterPlane, tmpPlane);
//        }

//        public override void Register(IFXSubscriber subscriber)
//        {
//            if (!(subscriber is SimpleModel))
//                throw new ArgumentException("Only subscribers of type SimpleModel can be registered to FXWaterOcean.");


//            base.Register(subscriber);
//            //TODO: what if the Geometry is not added to the Model?
//            // ShadowMapping is a good reason to switch to Model from Mesh3d and Actor3d but
//            // should exceptions be made for cases where the TVMesh clearly needs to be configured directly and OWNED by the FX?
//            // after its registered?  I mean, I dont think we could even do LOD or "subModels".  
//            // Ok, maybe lets use Mesh3d and have a Shared read only property that only gets set in the constructor.
//            // This Mesh3d _can_ then be added to the scene graph (needed both for culling and for scene manager to know
//            // when to page in/out things.  But crap, this means Mesh3d would need to be a Subscriber?  Maybe instead
//            // i could create another class type to wrap it... then i dont need the shared hack... i can make it like
//            // SimpleModel  that would only have Geometry and no Appearance?
//            Geometry e = (Geometry) ((SimpleModel) subscriber).Geometry;
//            TVMesh m = Core._CoreClient.Globals.GetMeshFromID(e.TVIndex);
//            m.SetTextureEx(0, _shader.RenderSurface.GetTexture(), -1);
//            ((SimpleModel) subscriber).Shader = _shader; // must be set to the model and not to the geometry directly.
//        }

//        private TV_3DMATRIX _original_view;
//        private TV_3DMATRIX _original_projection;
//        private TV_3DMATRIX _reflView;
//        private TV_3DMATRIX _culledProj;
//        // As for the below, first everything we render will take on the reflectView matrix
//        // so that its rendering from the point of view of the camera on the water at the LookAt
//        // then we start rendering
//        // however he doesnt set the sky and such i believe because he doesnt want the sky/sun/moon getting culled.
//        // (thought it might have something to do with his custom sky code)
//        // i'm not sure why it would get culled if the custom projection matrix was setup properly?  I suspect its because
//        // of the way you'd normally do atmosphere/skybox rendering... so maybe this can use a callback to the scene
//        // to use just Render()  and then trigger callbacks to switch the camera's matrices, and to trigger the rendering
//        // of the seperate parts of the scene (atmosphere, regular scene, restore matrices)
//        // So how is this done?  I mean we do know all this rendering needs to be done beforeClear
//        // and how would this effect our multithreaded update/render goals?
//        // ideally we'd want to be able to cull our scene using the updated frustum using the water's perspective
//        // and render afterwards.  This of course would come after initial simulation update of course... and the water
//        // ripples themselves could be updated there too... along with all the camera matrix calcs above... its just the actual
//        // Culler and Render that we'd still need to do...  And whatever solution we come up with
//        // can we make it so our planet dynamic billboarding works similar?  To an extent anyway.. since it does camera matrix changes.
//        public override void Update(int elapsed, Camera camera)
//        {
//            // View matrix reflection 1
//            TV_3DMATRIX reflectMat = new TV_3DMATRIX();
//            _original_view = camera.Matrix;
//            _reflView = camera.RotationMatrix;

//#if FIXED_ORIGIN
//            _reflView.m41 = 0; // camera.Translation.x;
//            _reflView.m42 = 0; // camera.Translation.y;
//            _reflView.m43 = 0; // camera.Translation.z;
//#else
//            _reflView.m41 = (float)camera.Position.x;
//            _reflView.m42 = (float)camera.Position.y;
//            _reflView.m43 = (float)camera.Position.z;
//#endif 

//            Core._CoreClient.Maths.TVMatrixReflect(ref reflectMat, _waterPlane);
//            _reflView *= reflectMat;

//            // View matrix reflection 2
//            // Using two reflections allows not to reverse the culling mode on reflected geometry
//            // Thanks for the tip, Sylvain :)
//            TV_PLANE comb = new TV_PLANE();
//            TV_3DVECTOR vec = new TV_3DVECTOR(_reflView.m41, _reflView.m42, _reflView.m43);
//            TV_3DVECTOR vec2 = vec + new TV_3DVECTOR(_reflView.m31, _reflView.m32, _reflView.m33);
//            TV_3DVECTOR vec3 = vec + new TV_3DVECTOR(_reflView.m21, _reflView.m22, _reflView.m23);
//            Core._CoreClient.Maths.TVPlaneFromPoints(ref comb, vec, vec2, vec3);
//            Core._CoreClient.Maths.TVMatrixReflect(ref reflectMat, comb);
//            _reflView *= reflectMat;


//            // NOTE: In his newest code he gets rid of most of this and he uses a seperate waterCamera
//            // But i want to use the main camera because i get my treeRenderer culling for the reflection rendering.
//            // TODO: ask Zak about the main difference.  I mean i know i have to save/replace the original camera matrices
//            // Transform clipping plane
//            TV_3DMATRIX invTransView = new TV_3DMATRIX();
//            Core._CoreClient.Maths.TVMatrixTranspose(ref invTransView, _reflView);
//            TV_4DVECTOR clipPlane = new TV_4DVECTOR();

//            Core._CoreClient.Maths.TVVec4Transform(ref clipPlane, new TV_4DVECTOR(_waterPlane.Normal.x, _waterPlane.Normal.y,
//                                                                            _waterPlane.Normal.z,
//                                                                            _waterPlane.Dist + FIX_DISTANCE),
//                                             invTransView);

//            // Set oblique depth projection matrix
//            // See http://www.terathon.com/code/oblique.html
//            _original_projection = camera.ProjectionMatrix;
//            _culledProj = _original_projection;

//            TV_4DVECTOR q;
//            q.x = Math.Sign(clipPlane.x)/_culledProj.m11;
//            q.y = Math.Sign(clipPlane.y)/_culledProj.m22;
//            q.z = 1.0F;
//            q.w = (1.0F - _culledProj.m33)/_culledProj.m43;

//            TV_4DVECTOR scaledPlaneVector = clipPlane*(1.0F/(clipPlane*q));
//            _culledProj.m13 = scaledPlaneVector.x;
//            _culledProj.m23 = scaledPlaneVector.y;
//            _culledProj.m33 = scaledPlaneVector.z;
//            _culledProj.m43 = scaledPlaneVector.w;
//        }


//        public override void PreRender(Camera camera)
//        {
//            bool anyInFrustum = false;

//            foreach (IFXSubscriber sub in _subscribers)
//            {
//                //compute and set visibility flag
//                if (sub is EntityBase)
//                    if (camera.IsBoxVisible(((EntityBase)sub).BoundingBox.Min, ((EntityBase)sub).BoundingBox.Max))
//                    {
//                        ((EntityBase)sub).InFrustum = true;
//                        anyInFrustum = true;
//                    }
//                    else
//                        ((EntityBase)sub).InFrustum = false;
//            }

//            if (anyInFrustum)
//            {
//                // since i now use a dedicated reflection camera, I dont have to cache the original projection and matrix
//                _reflectionCamera.Matrix = _reflView;
//                _reflectionCamera.ProjectionMatrix = _original_projection;
//                _shader.BeginRender(_reflectionCamera);
//                // RenderDistant before setting the projection means that the skybox/far geometry
//                // won't be culled at height = waterplane + fix... Looks better this way.

//                // aka: Change the reflection matrix prior to rendering the skybox and sun/moon and such and then render this to a render surface

//                // now render the landscape and other normal page scenery
//                _reflectionCamera.ProjectionMatrix = _culledProj;

//                //TODO: note this like our FXSkys which have a general "Render() should be rendered first.
//                // only other types of FXWater will be skipped
//                _renderCB.Invoke(_reflectionCamera); // call to render the scene into the reflection
//                _shader.EndRender();
//            }
//        }

//        public override void PostRender(Camera camera)
//        {
//            foreach (IFXSubscriber sub in _subscribers)
//                if (sub is EntityBase && ((EntityBase)sub).InFrustum)
//                    ((EntityBase)sub).Render(camera.Position, _semantic);
//        }
//    }
//}

////    //the following version is not used.  From this thread...
////    //http://www.truevision3d.com/phpBB2/viewtopic.php?t=6335
////    //http://www.truevision3d.com/phpBB2/viewtopic.php?t=8222&highlight=stormy+sea
////    // and http://www.bitmanagement.de/developer/contact/examples/shader/shader.html
////    // and utilizes this shader which apparently can be used directly with tv3d.

////    // /*********************************************************************NVMH3****
//////Path:  NVSDK\Common\media\cgfx
//////File:  ocean.fx

//////Copyright NVIDIA Corporation 2003
//////TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
//////*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
//////OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
//////AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
//////BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
//////WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
//////BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
//////ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
//////BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


//////Comments:
//////    Simple ocean shader with animated bump map and geometric waves
//////    Based partly on "Effective Water Simulation From Physical Models", GPU Gems

//////hg minor changes

//////******************************************************************************/

//////float4x4 worldMatrix : World < string UIWidget = "none";>;	           	// World or Model matrix
//////float4x4 wvpMatrix : WorldViewProjection < string UIWidget = "none";>;	// Model*View*Projection
//////float4x4 worldViewMatrix : WorldView < string UIWidget = "none";>;
//////float4x4 viewInverseMatrix : ViewInverse < string UIWidget = "none";>;

//////double time : Time < string UIWidget = "none"; >;

//////texture normalMap : Normal
//////<
//////    string ResourceName = "waves2.dds";
//////    string TextureType = "2D";
//////>;

//////texture cubeMap : Environment
//////<
//////    string ResourceName = "CloudyHillsCubemap2.dds";
//////    string TextureType = "cube";
//////>;

//////sampler2D normalMapSampler = sampler_state
//////{
//////    Texture = <normalMap>;
//////#if 0
//////    // this is a trick from Halo - use point sampling for sparkles
//////    MagFilter = Linear;	
//////    MinFilter = Point;
//////    MipFilter = None;
//////#else
//////    MagFilter = Linear;	
//////    MinFilter = Linear;
//////    MipFilter = Linear;
//////#endif
//////};

//////samplerCUBE envMapSampler = sampler_state
//////{
//////    Texture = <cubeMap>;
//////    MinFilter = Linear;
//////    MagFilter = Linear;
//////    MipFilter = Linear;
//////    AddressU = Clamp;
//////    AddressV = Clamp;
//////};

//////double bumpHeight
//////<
//////    string UIWidget = "slider";
//////    double UIMin = 0.0; double UIMax = 2.0; double UIStep = 0.01;
//////    string UIName = "Bump Height";
//////> = 0.1;

//////float2 textureScale
//////<
//////    string UIName = "Texture scale";
//////> = { 8.0, 4.0 };

//////float2 bumpSpeed
//////<
//////    string UIName = "Bumpmap translation speed";
//////> = { -0.05, 0.0 };

//////double fresnelBias
//////<
//////    string UIName = "Fresnel bias";
//////    string UIWidget = "slider";
//////    double UIMin = 0.0; double UIMax = 1.0; double UIStep = 0.01;
//////> = 0.1;

//////double fresnelPower
//////<
//////    string UIName = "Fresnel exponent";
//////    string UIWidget = "slider";
//////    double UIMin = 1.0; double UIMax = 10.0; double UIStep = 0.01;
//////> = 4.0;

//////double hdrMultiplier
//////<
//////    string UIName = "HDR multiplier";
//////    string UIWidget = "slider";
//////    double UIMin = 0.0; double UIMax = 100.0; double UIStep = 0.01;
//////> = 3.0;

//////float4 deepColor : Diffuse
//////<
//////    string UIName = "Deep water color";
//////> = {0.0f, 0.0f, 0.1f, 1.0f};

//////float4 shallowColor : Diffuse
//////<
//////    string UIName = "Shallow water color";
//////> = {0.0f, 0.5f, 0.5f, 1.0f};

//////float4 reflectionColor : Specular
//////<
//////    string UIName = "Reflection color";
//////> = {1.0f, 1.0f, 1.0f, 1.0f};

//////// these are redundant, but makes the ui easier:
//////double reflectionAmount
//////<
//////    string UIName = "Reflection amount";
//////    string UIWidget = "slider";    
//////    double UIMin = 0.0; double UIMax = 2.0; double UIStep = 0.01;    
//////> = 1.0f;

//////double waterAmount
//////<
//////    string UIName = "Water color amount";
//////    string UIWidget = "slider";    
//////    double UIMin = 0.0; double UIMax = 2.0; double UIStep = 0.01;    
//////> = 1.0f;

//////double waveAmp
//////<
//////    string UIName = "Wave amplitude";
//////    string UIWidget = "slider";
//////    double UIMin = 0.0; double UIMax = 10.0; double UIStep = 0.1;
//////> = 1.0;

//////double waveFreq
//////<
//////    string UIName = "Wave frequency";
//////    string UIWidget = "slider";
//////    double UIMin = 0.0; double UIMax = 1.0; double UIStep = 0.001;
//////> = 0.1;


//////struct a2v {
//////    float4 Translation : POSITION;   // in object space
//////    float3 Normal   : NORMAL;
//////    float2 TexCoord : TEXCOORD0;
//////    float3 Tangent  : TEXCOORD1;
//////    float3 Binormal : TEXCOORD2;
//////};

//////struct v2f {
//////    float4 Translation  : POSITION;  // in clip space
//////    float2 TexCoord  : TEXCOORD0;
//////    float3 TexCoord1 : TEXCOORD1; // first row of the 3x3 transform from tangent to cube space
//////    float3 TexCoord2 : TEXCOORD2; // second row of the 3x3 transform from tangent to cube space
//////    float3 TexCoord3 : TEXCOORD3; // third row of the 3x3 transform from tangent to cube space

//////    float2 bumpCoord0 : TEXCOORD4;
//////    float2 bumpCoord1 : TEXCOORD5;
//////    float2 bumpCoord2 : TEXCOORD6;

//////    float3 eyeVector  : TEXCOORD7;
//////};

//////// wave functions

//////struct Wave {
//////  double freq;  // 2*PI / wavelength
//////  double amp;   // amplitude
//////  double phase; // speed * 2*PI / wavelength
//////  float2 dir;
//////};

//////#define NWAVES 2
//////Wave wave[NWAVES] = {
//////    { 1.0, 1.0, 0.5, float2(-1, 0) },
//////    { 2.0, 0.5, 1.3, float2(-0.7, 0.7) }	
//////};

//////double evaluateWave(Wave w, float2 pos, double t)
//////{
//////  return w.amp * sin( dot(w.dir, pos)*w.freq + t*w.phase);
//////}

//////// derivative of wave function
//////double evaluateWaveDeriv(Wave w, float2 pos, double t)
//////{
//////  return w.freq*w.amp * cos( dot(w.dir, pos)*w.freq + t*w.phase);
//////}

//////// sharp wave functions
//////double evaluateWaveSharp(Wave w, float2 pos, double t, double k)
//////{
//////  return w.amp * pow(sin( dot(w.dir, pos)*w.freq + t*w.phase)* 0.5 + 0.5 , k);
//////}

//////double evaluateWaveDerivSharp(Wave w, float2 pos, double t, double k)
//////{
//////  return k*w.freq*w.amp * pow(sin( dot(w.dir, pos)*w.freq + t*w.phase)* 0.5 + 0.5 , k - 1) * cos( dot(w.dir, pos)*w.freq + t*w.phase);
//////}

//////v2f BumpReflectWaveVS(a2v IN,
//////                      uniform float4x4 WorldViewProj,
//////                      uniform float4x4 World,
//////                      uniform float4x4 ViewIT,
//////                      uniform double BumpScale,
//////                      uniform float2 textureScale,
//////                      uniform float2 bumpSpeed,
//////                      uniform double time,
//////                      uniform double waveFreq,
//////                      uniform double waveAmp
//////                      )
//////{
//////    v2f OUT;

//////    wave[0].freq = waveFreq;
//////    wave[0].amp = waveAmp;

//////    wave[1].freq = waveFreq*2.0;
//////    wave[1].amp = waveAmp*0.5;

//////    time *=100.0f;

//////    float4 P = IN.Translation;

//////    // sum waves	
//////    P.y = 0.0;
//////    double ddx = 0.0, ddy = 0.0;
//////    for(int i=0; i<NWAVES; i++) {
//////        P.y += evaluateWave(wave[i], P.xz, time);
//////        double deriv = evaluateWaveDeriv(wave[i], P.xz, time);
//////        ddx += deriv * wave[i].dir.x;
//////        ddy += deriv * wave[i].dir.y;
//////    }

//////    // compute tangent basis
//////    float3 B = float3(1, ddx, 0);
//////    float3 T = float3(0, ddy, 1);
//////    float3 N = float3(-ddx, 1, -ddy);

//////    OUT.Translation = mul(P, WorldViewProj);

//////    // pass texture coordinates for fetching the normal map
//////    OUT.TexCoord.xy = IN.TexCoord*textureScale;

//////    time = fmod(time, 100.0);
//////    OUT.bumpCoord0.xy = IN.TexCoord*textureScale + time*bumpSpeed;
//////    OUT.bumpCoord1.xy = IN.TexCoord*textureScale*2.0 + time*bumpSpeed*4.0;
//////    OUT.bumpCoord2.xy = IN.TexCoord*textureScale*4.0 + time*bumpSpeed*8.0;

//////    // compute the 3x3 tranform from tangent space to object space
//////    float3x3 objToTangentSpace;
//////    // first rows are the tangent and binormal scaled by the bump scale
//////    objToTangentSpace[0] = BumpScale * normalize(T);
//////    objToTangentSpace[1] = BumpScale * normalize(B);
//////    objToTangentSpace[2] = normalize(N);

//////    OUT.TexCoord1.xyz = mul(objToTangentSpace, World[0].xyz);
//////    OUT.TexCoord2.xyz = mul(objToTangentSpace, World[1].xyz);
//////    OUT.TexCoord3.xyz = mul(objToTangentSpace, World[2].xyz);

//////    // compute the eye vector (going from shaded point to eye) in cube space
//////    float4 worldPos = mul(P, World);
//////    OUT.eyeVector = ViewIT[3] - worldPos; // view inv. transpose contains eye position in world space in last row
//////    return OUT;
//////}


//////// Pixel Shaders


//////float4 BumpReflectPS20(v2f IN,
//////                       uniform sampler2D NormalMap,
//////                       uniform samplerCUBE EnvironmentMap) : COLOR
//////{
//////    // fetch the bump normal from the normal map
//////    float4 N = tex2D(NormalMap, IN.TexCoord.xy)*2.0 - 1.0;

//////    float3x3 m; // tangent to world matrix
//////    m[0] = IN.TexCoord1;
//////    m[1] = IN.TexCoord2;
//////    m[2] = IN.TexCoord3;
//////    float3 Nw = mul(m, N.xyz);

////////	float3 E = float3(IN.TexCoord1.w, IN.TexCoord2.w, IN.TexCoord3.w);
//////    float3 E = IN.eyeVector;
//////    float3 R = reflect(-E, Nw);

//////    return texCUBE(EnvironmentMap, R);
//////}

//////float4 OceanPS20(v2f IN,
//////                 uniform sampler2D NormalMap,
//////                 uniform samplerCUBE EnvironmentMap,
//////                 uniform half4 deepColor,
//////                 uniform half4 shallowColor,
//////                 uniform half4 reflectionColor,
//////                 uniform half4 reflectionAmount,
//////                 uniform half4 waterAmount,
//////                 uniform half fresnelPower,
//////                 uniform half fresnelBias,
//////                 uniform half hdrMultiplier
//////                 ) : COLOR
//////{
//////    // sum normal maps
//////    half4 t0 = tex2D(NormalMap, IN.bumpCoord0.xy)*2.0-1.0;
//////    half4 t1 = tex2D(NormalMap, IN.bumpCoord1.xy)*2.0-1.0;
//////    half4 t2 = tex2D(NormalMap, IN.bumpCoord2.xy)*2.0-1.0;
//////    half3 N = t0.xyz + t1.xyz + t2.xyz;
////////    half3 N = t1.xyz;

//////    half3x3 m; // tangent to world matrix
//////    m[0] = IN.TexCoord1;
//////    m[1] = IN.TexCoord2;
//////    m[2] = IN.TexCoord3;
//////    half3 Nw = mul(m, N.xyz);
//////    Nw = normalize(Nw);

//////    // reflection
//////    float3 E = normalize(IN.eyeVector);
//////    half3 R = reflect(-E, Nw);

//////    half4 reflection = texCUBE(EnvironmentMap, R);
//////    // hdr effect (multiplier in alpha channel)
//////    reflection.rgb *= (1.0 + reflection.a*hdrMultiplier);

//////    // fresnel - could use 1D tex lookup for this
//////    half facing = 1.0 - max(dot(E, Nw), 0);
//////    half fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);

//////    half4 waterColor = lerp(deepColor, shallowColor, facing);

//////    return waterColor*waterAmount + reflection*reflectionColor*reflectionAmount*fresnel;
////////	return waterColor;
////////	return fresnel;
////////	return reflection;
//////}


//////technique PS20
//////{
//////    pass p0
//////    {
//////        VertexShader = compile vs_2_0 BumpReflectWaveVS(
//////                    wvpMatrix, worldMatrix, viewInverseMatrix,
//////                                        bumpHeight, textureScale, bumpSpeed, time,
//////                                        waveFreq, waveAmp);

//////        //Zenable = true;
//////        //ZWriteEnable = true;
//////        //CullMode = None;

//////    //	PixelShader = compile ps_2_0 BumpReflectPS20(normalMapSampler, envMapSampler);
//////        PixelShader = compile ps_2_0 OceanPS20(normalMapSampler, envMapSampler,
//////                                              deepColor, shallowColor, reflectionColor, reflectionAmount, waterAmount,
//////                                               fresnelPower, fresnelBias, hdrMultiplier);
//////    }
//////}

//////// hg try on PS13 fallback 

//////float4 BumpReflectPS1(v2f IN,
//////                       uniform sampler2D NormalMap,
//////                       uniform samplerCUBE EnvironmentMap,
//////                       uniform half4 deepColor,
//////                       uniform half4 shallowColor) : COLOR
//////{
//////    // fetch the bump normal from the normal map
//////    half3 N = tex2D(NormalMap, IN.TexCoord.xy)*2.0 - 1.0;
//////    //half3 E = IN.eyeVector;
//////    //   	half3 R = reflect(-E, N);
//////    //half3 dot = N*IN.eyeVector
//////    return deepColor+0.5*texCUBE(EnvironmentMap, N);
//////}

//////technique PS13
//////{
//////    // hg fallback 
//////    pass p0
//////    {
//////        VertexShader = compile vs_1_1 BumpReflectWaveVS(wvpMatrix, worldMatrix, viewInverseMatrix,
//////                                                        bumpHeight, textureScale, bumpSpeed, time,
//////                                                        waveFreq, waveAmp);

//////        PixelShader = compile ps_1_3 BumpReflectPS1(normalMapSampler, envMapSampler,deepColor, shallowColor);
//////    }
//////}

////    public class OceanShaderNvidia : Shader
////    {
////        public enum OceanRenderMode
////        {
////            CUBE = 0,
////            ALL,
////            WORLD
////        }

////        public int Elapsed;

////        public double HdrMultiplier = 0.4F;
////        public double BumpHeight = 0.3F;
////        public TV_2DVECTOR BumpSpeed = new TV_2DVECTOR(-0.01F, -0.01F);
////        public TV_2DVECTOR TextureScale = new TV_2DVECTOR(32, 16);
////        public double ReflectionAmount = 0.45F;
////        public double TimeMod = 0.01F;
////        public double WaterAmount = 1F;
////        public string NormalMap;
////        public int NormalMapIndex;

////        public string CubeMap;
////        public double FresnelBias = 0.07F;
////        public double WaveAmplitude = 0.2F;
////        public double WaveFrequency = 0.3F;
////        public Vector3d AtmosphereColVec;
////        public OceanRenderMode RenderMode = OceanRenderMode.CUBE;
////        public int UpdateFrequency;
////        public TVMesh MeshO;
////        //public Enabled As Boolean
////        public int RenderCount;


////        public OceanShaderNvidia(string name, string path)
////            : base(name, path)
////            //// TODO: move this into a seperate library?  at least to make sure we are staying modular enough?
////            //public OceanShader(string name, string filepath, OceanRenderMode renderMode, RS_RESOLUTION size)
////            //    : base(name, filepath)
////        {
////            //CubeMap = "\Textures\CloudyHillsCubemap2.dds", "cuby"
////            //NormalMap = "\Textures\waves2.dds", "waves"

////            //cubeRenderSurface = TV3DUtility.CreateCubeRenderSurface(size, True, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_A8R8G8B8);
////            // _rs.SetCubeMapProperties(true, new Vector3d(1536, 500, -1536));

////            // AddStage(new OceanStageNvidia(this));
////   }
////}

////    public class OceanStageNvidia : FXRenderStage
////    {
////        private OceanShaderNvidia _OceanShader;

////        public OceanStageNvidia(OceanShaderNvidia ocean)
////        {
////            if (ocean == null) throw new ArgumentNullException();
////            _OceanShader = ocean;
////        }

////        public override void Update()
////        {
////            if (_OceanShader.Elapsed > _OceanShader.UpdateFrequency)
////            {
////                _OceanShader.TVShader.SetEffectParamTexture("cubeMap", _OceanShader.RenderSurface.GetTexture());

////                // _OceanShader._tvShader.SetEffectParamColor("", New TV_COLOR);

////                _OceanShader.TVShader.SetEffectParamFloat("waveFreq", _OceanShader.WaveFrequency);
////                _OceanShader.TVShader.SetEffectParamFloat("waveAmp", _OceanShader.WaveAmplitude);
////                _OceanShader.TVShader.SetEffectParamFloat("hdrMultiplier", _OceanShader.HdrMultiplier);
////                _OceanShader.TVShader.SetEffectParamFloat("bumpHeight", _OceanShader.BumpHeight);
////                _OceanShader.TVShader.SetEffectParamVector2("bumpSpeed", _OceanShader.BumpSpeed);
////                _OceanShader.TVShader.SetEffectParamVector2("textureScale", _OceanShader.TextureScale);
////                _OceanShader.TVShader.SetEffectParamFloat("reflectionAmount", _OceanShader.ReflectionAmount);
////                _OceanShader.TVShader.SetEffectParamFloat("timeMod", _OceanShader.TimeMod);
////                _OceanShader.TVShader.SetEffectParamFloat("waterAmount", _OceanShader.WaterAmount);
////                _OceanShader.TVShader.SetEffectParamTexture("normalMap", _OceanShader.NormalMapIndex);
////                _OceanShader.TVShader.SetEffectParamFloat("fresnelBias", _OceanShader.FresnelBias);

////                _OceanShader.TVShader.SetEffectParamVector4("shallowColor",
////                                                            new TV_4DVECTOR(_OceanShader.AtmosphereColVec.x,
////                                                                            _OceanShader.AtmosphereColVec.y,
////                                                                            _OceanShader.AtmosphereColVec.z, 1));
////                _OceanShader.TVShader.SetEffectParamVector4("reflectionColor",
////                                                            new TV_4DVECTOR(_OceanShader.AtmosphereColVec.x/2.0F,
////                                                                            _OceanShader.AtmosphereColVec.y/2.0F,
////                                                                            _OceanShader.AtmosphereColVec.z/2.0F, 0.7F));
////                _OceanShader.Elapsed = 0;
////            }
////        }
////    }
////}
