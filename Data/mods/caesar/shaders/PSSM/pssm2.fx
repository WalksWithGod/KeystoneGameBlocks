// -------------------------------------------------------------
// Default Geometry Shader - Textured + Normal Mapping + Shadow Mapping
// -------------------------------------------------------------

// -------------------------------------------------------------
// Geometry Type compilation switches
// -------------------------------------------------------------
// #define GEOMETRY_MESH
// #define GEOMETRY_TERRAIN  // a type of Mesh that uses splatting and slope texture lookups
// #define GEOMETRY_TEXTURED_QUAD	
// #define GEOMETRY_MINIMESH
// #define GEOMETRY_INSTANCING
// #define GEOMETRY_ACTOR

// -------------------------------------------------------------
// Texturing compilation switches
// -------------------------------------------------------------
// #define NORMAL_MAPPING_ENABLED
// #define ALPHA_TEST_ENABLED
// #define TEXTURE_MAPPING_ENABLED

// -------------------------------------------------------------
// Diffuse lighting models compilation switches
// -------------------------------------------------------------
#define LAMBERT
// #define WRAP_LAMBERT
// #define RAMP_LAMBERT

// -------------------------------------------------------------
// Specular lighting models compilation switches
// -------------------------------------------------------------
// #define PHONG // <- this is the only one that doesn't seem to work. all others verified.
 #define BLINN_PHONG
// #define LYON
// #define TORRANCE_SPARROW
// #define TROWBRIDGE_REITZ
	
// -------------------------------------------------------------
// PSSM compilation switches
// -------------------------------------------------------------
// #define SHADOW_MAPPING_ENABLED
// #define ESM 
//		- rather than linear, depth values are stored scaled exponentially by distance to camera
// #define NUM_SPLITS_1 
//		- no splits effectively
// #define NUM_SPLITS_2
//		- frustum is split into 2 halves
// #define NUM_SPLITS_3
//		- frustum is split into 3 parts
// #define NUM_SPLITS_4
//		- frustum is split into 4 parts

// -------------------------------------------------------------
// Grid compilation switches
// -------------------------------------------------------------
// #define TERRAIN_GRID_ENABLE

// -------------------------------------------------------------
// FOG CONSTANTS
// -------------------------------------------------------------
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3 

// -------------------------------------------------------------
// MATRIX SEMANTICS
// -------------------------------------------------------------
float3 g_ViewPosition 			: VIEWPOSITION;
float4x4 ViewProjection			: VIEWPROJECTION;

#if defined (GEOMETRY_MINIMESH)
	// TVMinimesh needs array of world matrices
	// 	max : 52 matrices for vS2.0
	//	      16 matrices for VS1.1
	// it can be 4x3 or 4x4
	float4x3 g_World[52] 		: MINIMESH_WORLD;
#elif defined (GEOMETRY_ACTOR)
	// bone matrices only use 3 rows (float4x3). It MUST have semantic BONES
	// however, if no bones are present in the Actor, it will use g_World
	float4x4 g_World 			: WORLD;  
	float4x3 g_BoneMatrices[52] : BONES;
	int g_NumBonesPerVertex 	: BONESPERVERTEX; 
#elif defined (GEOMETRY_INSTANCING) 
	#define InstancesPerBatch 64 // 192 // 248
	// NOTE: instancing path does not use TV3D geometry 
	// so semantics are not filled automatically by TV3D.
	// (256 - InstancesPerBatch) == register(cN)
	float4 InstancePositionsWS[InstancesPerBatch] : register(c192); 
	// float3 InstanceScales[InstancesPerBatch];
	// float3 InstanceRotations[InstancesPerBatch]; // NOTE: if only Y rotation is allowed, w parameter in InstancePositionWS contains Phi in radians.  For complete rotations, we must use seperate InstanceRotations.
	// float2 InstanceUVs[InstancesPerBatch];
#else
	float4x4 g_World 			: WORLD;
#endif

// -------------------------------------------------------------
// MATERIAL SEMANTICS
// -------------------------------------------------------------
float4 materialDiffuse 			: DIFFUSE;
float4 materialAmbient 			: AMBIENT;
float4 materialEmissive 		: EMISSIVE;
float4 materialSpecular 		: SPECULAR;
float materialPower 			: SPECULARPOWER;

#if defined (GEOMETRY_MINIMESH)
//// Minimesh can have the colors only if TVMinimesh.SetColorMode is enabled.
//// NOTE: the added float4 to the vertex declaration reduces the number of 
//// meshes that can be batched by half from 52 to 26... i think?
//float4 minimeshes_color[26] : MINIMESH_COLOR;

// float4 minimeshes_fade : MINIMESH_FADE;
//// fading options from the engine.
//// VECTOR4(fFarDistance, fStartFading, fMaxAlpha, fMaxAlpha/(fStartFading - fFarDistance));
//// the last parameter can obviously be used to optimize the fade computation as seen in the example below.	
#endif
// -------------------------------------------------------------
// FOG SEMANTICS
// -------------------------------------------------------------
int fogType 					: FOGTYPE;
float4 fogColor 				: FOGCOLOR; // todo: shouldn't this be float3?  there is no alpha for fog is there?
float fogDensity 				: FOGDENSITY;
float fogStart 					: FOGSTART;
float fogEnd 					: FOGEND;

// -------------------------------------------------------------
// LIGHT VARIABLES - "SetShaderParameter" names must match exactly
// -------------------------------------------------------------
float3 g_DirLightDirection; //     : LIGHTDIR0_DIR; <-- not sure why it doesnt work with semantic
float3 g_DirLightColor 			: LIGHTDIR0_COLOR;

// for shadow path we should eventually use below light vars, but currently not
// also the array is designed for 6 pointlights really... 
//float3 LightPos[6]; //our light position in object space
//float3 LightColors[6]; //our light Colors
//float LightRanges[6]; //Light Range
//int LightNum = 0;

// -------------------------------------------------------------
// SHADOW VARIABLES - "SetShaderParameter" names must match exactly
// -------------------------------------------------------------
float4x4 g_mLightViewProj[4];               
float4x4 g_mLightViewProjTextureMatrix[4];  

#if defined (NUM_SPLITS_4)
	float4 g_fSplitDistances;
	int g_SplitCount = 4;
#elif defined (NUM_SPLITS_3)
	float3 g_fSplitDistances;
	int g_SplitCount = 3; 
#elif defined (NUM_SPLITS_2)
	float2 g_fSplitDistances;
	int g_SplitCount = 2;
#elif defined (NUM_SPLITS_1)
	float g_fSplitDistances;
	int g_SplitCount = 1; 
#endif

float SHADOW_OPACITY = 0.45f;
float3 SHADOW_COLOR = float3(1.0f, 1.0f, 1.0f);
float g_fPCFScale;
float2 g_ShadowMapSize;
float DEPTH_BIAS = 0.001f; //0.005f; //0.001f; // prevents self shadows when epsilon between casting poly and receiv poly is really low
float NormalOffset = -0.25f; // normal offset applied to position in depth write to help with shadow acne

float g_OffsetScale = 0.01f; //
float4 g_SplitDistances;
float4 CascadeOffsets[4];
float4 CascadeScales[4];
float4x4 ShadowMatrix;

// -------------------------------------------------------------
///Exponential Shadow Map Var's which stores depth info in a scaled way (as opposed to linear)
// TODO: I should make ESM a #define
// -------------------------------------------------------------
float ESM_K = 60.0f; // Range [0, 80]
float ESM_MIN = -0.5f; // Range [0, -oo]
float ESM_DIFFUSE_SCALE = 1.39f; // Range [1, 10]


// -------------------------------------------------------------
// GRID VARIABLES - "SetShaderParameter" names must match exactly
// -------------------------------------------------------------
// Camera Relative World Offset 
#if defined (TERRAIN_GRID_ENABLE)
float3 g_CameraOffset;
float4 GridColour = float4(0.0f, 0.0f, 1.0f, 1.0f);
float GridThickness = 0.128f; // 0.032f; // range (0.0 - 1.0) width in UV.  best if these are powers of 2. 
float2 GridTiling2D = float2(1.0f, 1.0f);
float3 GridTiling3D = float3(1.0f, 0.5f, 1.0f);  // grid UV scaling.  1.0 == default. 2.0 == 2x grid density 4.0 = 4x grid density, etc
float GridLineStrength = 4.25f;
float GridFadeEnd = 40;
float GridFadeStart = 20;
float GridAltitudeThreshold = 2.8f;
float _GridSpacingX = 5.0f;
float _GridSpacingY = 5.0f;
float _GridOffsetX = 0.0f;
float _GridOffsetY = 0.0f;
#endif

// -------------------------------------------------------------
// TEXTURES & SAMPLERS
// -------------------------------------------------------------
texture texShadowMap;
sampler2D sampShadowMap  =
sampler_state
{
	Texture = (texShadowMap);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = 0x0000000;
};

texture textureDiffuse  : TEXTURE0;
sampler2D diffuseSampler = sampler_state
{
    Texture = <textureDiffuse>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

#if defined (SHADOW_MAPPING_ENABLED)
	// -------------------------------------------------------------
	// TECHNIQUE 'SHADOWED' - INPUT / OUTPUT STRUCTS
	// -------------------------------------------------------------
	struct VS_INPUT_SHADOWED                    
	{
		#if defined (GEOMETRY_INSTANCING)
			// TODO: why are position and normal required to be float4?  is it because
			// unlike TV3D, D3D fills thems as float4 even though the vertex declaration is float3 for both?
			float4 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS                 : NORMAL;		// vertex normal in the default pose
		#else
			float3 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS 				: NORMAL;		// vertex normal in the default pose
		#endif
		#if defined (NORMAL_MAPPING_ENABLED)
			float3 TangentMS 			: TANGENT;
			float3 BinormalMS 			: BINORMAL;
		#endif
		#if defined (GEOMETRY_ACTOR)
			float4 Blendindices 		: BLENDINDICES;	// indices of the 4 bone indices used in the bone matrix array
			float4 Blendweights 		: BLENDWEIGHT;	// weights of each bone.
		#endif
		float2 UV 						: TEXCOORD0;	
		#if defined (GEOMETRY_MINIMESH)
			// TODO: i changed this from float2 to float but haven't tested that TV doesn't require float2
			// and to access via int instance = (int)Index.x 
			float Index 				: TEXCOORD3;	// Minimesh Element Index is ALWAYS on TEXCOORD3.
		#elif defined (GEOMETRY_INSTANCING)
			float Index 				: TEXCOORD3;	// Instance Index is ALWAYS on TEXCOORD3.
		#endif
	};

	struct VS_OUTPUT_SHADOWED
	{
		float4 PositionCS 				: POSITION;		// clip space position
		float3 NormalWS 				: TEXCOORD0;
		float3 UVandDepth 				: TEXCOORD1;	// modelspace UV and clipspace depth in z component
		float4 fLightDepth 				: TEXCOORD2; 	// holds depths for 4 splits.
		float4 ProjectedLightSpaceUV[2] : TEXCOORD3;	// holds lightViewProjected UVs for 4 splits.
		float3 ViewDir 					: TEXCOORD5;	// for cell shading and Rim edge lighting
		float4 PositionWS 				: TEXCOORD6;
	};

	// -------------------------------------------------------------
	// TECHNIQUE 'DEPTH' - INPUT / OUTPUT STRUCTS
	// -------------------------------------------------------------
	struct VS_IN_DEPTH
	{
		#if defined (GEOMETRY_INSTANCING)
		// TODO: why are position and normal required to be float4?  is it because
		// unlike TV3D, D3D fills thems as float4 even though the vertex declaration is float3 for both?
			float4 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS                 : NORMAL;		// vertex normal in the default pose
		#else
			float3 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS 				: NORMAL;		// vertex normal in the default pose
		#endif
		#if defined (GEOMETRY_ACTOR)
			float4 Blendindices 		: BLENDINDICES;	// indices of the 4 bone indices used in the bone matrix array
			float4 Blendweights 		: BLENDWEIGHT;	// weights of each bone.
		#endif
		float2 UV 						: TEXCOORD0;	
		#if defined (GEOMETRY_MINIMESH)
			float Index 				: TEXCOORD3;	// Minimesh Element Index is ALWAYS on TEXCOORD3.
		#elif defined (GEOMETRY_INSTANCING)
			float Index 				: TEXCOORD3;	// Instance Index is ALWAYS on TEXCOORD3.
		#endif
	};

	struct VS_OUT_DEPTH
	{
		float4 PositionCS 				: POSITION;		// clip space position
		float2 UV 						: TEXCOORD0;	
		float ClipspaceDepth 			: TEXCOORD1;
	};
#else
// -------------------------------------------------------------
// TECHNIQUE 'DEFAULT' - INPUT / OUTPUT STRUCTS
// -------------------------------------------------------------
	struct VS_INPUT_DEFAULT                    
	{
		#if defined (GEOMETRY_INSTANCING)
			// TODO: why are position and normal required to be float4?  is it because
			// unlike TV3D, D3D fills thems as float4 even though the vertex declaration is float3 for both?
			float4 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS                 : NORMAL;		// vertex normal in the default pose
		#else
			float3 PositionMS 				: POSITION;		// model space vertex position in the default pose
			float3 NormalOS 				: NORMAL;		// vertex normal in the default pose
		#endif
		#if defined (NORMAL_MAPPING_ENABLED)
			float3 TangentMS 			: TANGENT;
			float3 BinormalMS 			: BINORMAL;
		#endif
		#if defined (GEOMETRY_ACTOR)
			float4 Blendindices 		: BLENDINDICES;	// indices of the 4 bone indices used in the bone matrix array
			float4 Blendweights 		: BLENDWEIGHT;	// weights of each bone.
		#endif
		float2 UV 						: TEXCOORD0;	
		#if defined (GEOMETRY_MINIMESH)
			// TODO: i changed this from float2 to float but haven't tested that TV doesn't require float2
			// and to access via int instance = (int)Index.x 
			float Index 				: TEXCOORD3;	// Minimesh Element Index is ALWAYS on TEXCOORD3.
		#elif defined (GEOMETRY_INSTANCING)
			float Index 				: TEXCOORD3;	// Instance Index is ALWAYS on TEXCOORD3.
		#endif
	};

	struct VS_OUTPUT_DEFAULT
	{
		float4 PositionCS 				: POSITION;		// clip space position
		float3 NormalWS 				: TEXCOORD0;
		float2 UV						: TEXCOORD1;	// modelspace UV
		float3 ViewDir 					: TEXCOORD5;	// for cell shading and Rim edge lighting
		float4 PositionWS 				: TEXCOORD6;
	};
#endif

// -------------------------------------------------------------
// FUNCTIONS
// -------------------------------------------------------------
#if defined (GEOMETRY_ACTOR)
// here is the standard skinning function of TV to skin the position and normal
// there must be 5 versions : one for each case (0 bone blending, 1 bone blending, 2 bones blending, .. ,4 bones blending)
void TransPositionNormal(uniform int iNumBones, 
						 float4 modelPos, float3 modelNorm, 
						 float4 boneWeights, float4 fBoneIndices, 
						 out float3 pos, out float3 norm)
{
	if(iNumBones == 0)
	{
  	    pos = mul(modelPos, g_World);
		norm = mul(modelNorm, (float3x3)g_World);
	}
	else
	{
		int4 ibone = D3DCOLORtoUBYTE4(fBoneIndices);
		int   IndexArray[4]        = (int[4])ibone;

		pos = 0;
		norm = 0;
		float lastweight = 0.0f;
		for(int i = 0; i < iNumBones - 1; i++)
		{
	       float4x3 mat = g_BoneMatrices[IndexArray[i]]; 
		   lastweight = lastweight + boneWeights[i];
		   pos += mul(modelPos, mat) *  boneWeights[i];
		   // normals are transformed only by 3x3 portion of the world matrix
		   norm += mul(modelNorm, (float3x3)mat ) *  boneWeights[i];
		}		
		
		lastweight = 1 - lastweight;
   		float4x3 mat = g_BoneMatrices[IndexArray[iNumBones-1]]; 
		pos += mul(modelPos, mat) * lastweight;
		// normals are transformed only by 3x3 portion of the world matrix
		norm += mul(modelNorm, (float3x3)mat ) *  lastweight;
	}
	return;
}  
#elif defined (GEOMETRY_INSTANCING)
// GetInstanceMatrix(float4)
// - assumes w parameter of position includes a Y axis rotation value in radians.
float4x4 GetInstanceMatrix (float4 instancePositionPhi)
{
	float sinPhi, cosPhi;
	// TODO: since our rotations for terrain tends to be in 90 degree increments
	// we should be able to use a simple lookup instead of computing sin/cos
	sincos(instancePositionPhi.w, sinPhi, cosPhi);
	float4x4 instanceMatrix = 
	{
		cosPhi,	0,	-sinPhi,		0,
		0,		1,	0,				0,
		sinPhi,	0,	cosPhi,			0,
		instancePositionPhi.xyz,	1 
	};
	return instanceMatrix;
}
#endif
/*
// -------------------------------------------------------------
// Parralax-Mapping pixel fragment
// -------------------------------------------------------------
float2 parallaxTexCoord(float2 oldCoord, float2 vecViewXY) {
	float level;
	level = tex2D(sampBump, oldCoord).a;
	return (level * parallaxAmount - parallaxAmount / 2) * vecViewXY + oldCoord;
}*/

/*
float3 Wind(float3 position)
{
// pass in IN.PositionMS.xyz and assign results to same
 
	// ideally rather than just "time" we would do a texture lookup into a sort of wave map
	// that varies the wind power _and_ which has varying affect on each face... so heavy branches
	// respond to the wind slower than lighter branches and leaves.   Also the wave map could provide
	// different power based on Y world position 
	float3 result;
	result.y = position.y;
	result.x += cos(time * 0.3 + instanceWorld[3].x * 0.01) * 0.1 * position.y;
	result.z += sin(time * 0.3 + instanceWorld[3].z * 0.01) * 0.1 * position.y;
	return result;
	
}	
*/

#ifdef CEL_SHADING
float getEdgeDepth(vec2 coord) 
{
		float depth = texture2D( sampler1, coord ).x;
		float depth2 = texture2D( sampler2, coord ).x;
		if ( depth2 < 1.0 ) {
				depth = depth2;
		}
   
		if ( depth == 1.0 ) {
				return INFINITY;
		}
   
		return depth * near / ( far + ( near - far ) * depth );
}

float4 edgeDetect( float2 coord ) 
{
		float2 o11 = float2(1.0, aspectRatio) * CEL_EDGE_THICKNESS / displayWidth;
		float4 color = float4(0.0);
 
		float depth = getEdgeDepth(coord);
		float avg = 0.0;
		float laplace = 24.0 * depth;
		float sample;
		int n = 0;
   
		if (depth < INFINITY) {
				avg += depth;
				++n;
		}
 
		for (int i = -2; i <= 2; ++i) {
				for (int j = -2; j <= 2; ++j) {
						if (i != 0 || j != 0) {
								sample = getEdgeDepth(coord + float2(float( i ) * o11.s, float( j ) * o11.t));
								laplace -= sample;
								if (sample < INFINITY) {
										++n;
										avg += sample;
								}
						}
				}
		}
   
		avg = clamp( avg/ float( n ), 0.0, 1.0);
 
		if ( laplace > avg * CEL_EDGE_THRESHOLD ) {
				color.rgb = mix( vec3( 0.0 ), gl_Fog.color.rgb, 0.75 * avg * avg);
				color.a = 1.0;
		}
 
		return color;
}
#endif

// float sampleShadowMap(float2 base_uv, 
					  // float u, float v, 
					  // float2 shadowMapSizeInv, 
					  // uint cascadeIdx, 
					  // float depth, 
					  // float2 receiverPlaneDepthBias) 
// {

    // float2 uv = base_uv + float2(u, v) * shadowMapSizeInv;

    // #if UsePlaneDepthBias_
        // float z = depth + dot(float2(u, v) * shadowMapSizeInv, receiverPlaneDepthBias);
    // #else
        // float z = depth;
    // #endif

	// // TODO: this does not PCF 
	// // rgba channels accessible individually via fSplitIndex
	// float occluder = tex2D(sampShadowMap, uv)[cascadeIdx]; 
	
	// return occluder - z;
    // //return ShadowMap.SampleCmpLevelZero(sampShadowMap, float3(uv, cascadeIdx), z);
// }

// //-------------------------------------------------------------------------------------------------
// // The method used in The Witness
// //-------------------------------------------------------------------------------------------------
// float SampleShadowMapOptimizedPCF(in float3 shadowPos, in float3 shadowPosDX,
                         // in float3 shadowPosDY, in uint cascadeIdx) {
    
    // float lightDepth = shadowPos.z;
    // const float bias = DEPTH_BIAS;

    // #if UsePlaneDepthBias_
        // float2 texelSize = 1.0f / g_ShadowMapSize;

        // float2 receiverPlaneDepthBias = ComputeReceiverPlaneDepthBias(shadowPosDX, shadowPosDY);

        // // Static depth biasing to make up for incorrect fractional sampling on the shadow map grid
        // float fractionalSamplingError = 2 * dot(float2(1.0f, 1.0f) * texelSize, abs(receiverPlaneDepthBias));
        // lightDepth -= min(fractionalSamplingError, 0.01f);
    // #else
        // float2 receiverPlaneDepthBias;
        // lightDepth -= bias;
    // #endif

        // float2 uv = shadowPos.xy * g_ShadowMapSize; // 1 unit - 1 texel

        // float2 shadowMapSizeInv = 1.0 / g_ShadowMapSize;

        // float2 base_uv;
        // base_uv.x = floor(uv.x + 0.5);
        // base_uv.y = floor(uv.y + 0.5);

        // float s = (uv.x + 0.5 - base_uv.x);
        // float t = (uv.y + 0.5 - base_uv.y);

        // base_uv -= float2(0.5, 0.5);
        // base_uv *= shadowMapSizeInv;

        // float sum = 0;

    // #if FilterSize_ == 2
        // return 1.0f; // TODO: sampShadowMap.SampleCmpLevelZero(sampShadowMap, float3(shadowPos.xy, cascadeIdx), lightDepth);
    // #elif FilterSize_ == 3

        // float uw0 = (3 - 2 * s);
        // float uw1 = (1 + 2 * s);

        // float u0 = (2 - s) / uw0 - 1;
        // float u1 = s / uw1 + 1;

        // float vw0 = (3 - 2 * t);
        // float vw1 = (1 + 2 * t);

        // float v0 = (2 - t) / vw0 - 1;
        // float v1 = t / vw1 + 1;

        // sum += uw0 * vw0 * sampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw0 * sampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw0 * vw1 * sampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw1 * sampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // return sum * 1.0f / 16;

    // #elif FilterSize_ == 5

        // float uw0 = (4 - 3 * s);
        // float uw1 = 7;
        // float uw2 = (1 + 3 * s);

        // float u0 = (3 - 2 * s) / uw0 - 2;
        // float u1 = (3 + s) / uw1;
        // float u2 = s / uw2 + 2;

        // float vw0 = (4 - 3 * t);
        // float vw1 = 7;
        // float vw2 = (1 + 3 * t);

        // float v0 = (3 - 2 * t) / vw0 - 2;
        // float v1 = (3 + t) / vw1;
        // float v2 = t / vw2 + 2;

        // sum += uw0 * vw0 * sampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw0 * sampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw0 * sampleShadowMap(base_uv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // sum += uw0 * vw1 * sampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw1 * sampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw1 * sampleShadowMap(base_uv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // sum += uw0 * vw2 * sampleShadowMap(base_uv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw2 * sampleShadowMap(base_uv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw2 * sampleShadowMap(base_uv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // return sum * 1.0f / 144;

    // #else // FilterSize_ == 7

        // float uw0 = (5 * s - 6);
        // float uw1 = (11 * s - 28);
        // float uw2 = -(11 * s + 17);
        // float uw3 = -(5 * s + 1);

        // float u0 = (4 * s - 5) / uw0 - 3;
        // float u1 = (4 * s - 16) / uw1 - 1;
        // float u2 = -(7 * s + 5) / uw2 + 1;
        // float u3 = -s / uw3 + 3;

        // float vw0 = (5 * t - 6);
        // float vw1 = (11 * t - 28);
        // float vw2 = -(11 * t + 17);
        // float vw3 = -(5 * t + 1);

        // float v0 = (4 * t - 5) / vw0 - 3;
        // float v1 = (4 * t - 16) / vw1 - 1;
        // float v2 = -(7 * t + 5) / vw2 + 1;
        // float v3 = -t / vw3 + 3;

        // sum += uw0 * vw0 * sampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw0 * sampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw0 * sampleShadowMap(base_uv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw3 * vw0 * sampleShadowMap(base_uv, u3, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // sum += uw0 * vw1 * sampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw1 * sampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw1 * sampleShadowMap(base_uv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw3 * vw1 * sampleShadowMap(base_uv, u3, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // sum += uw0 * vw2 * sampleShadowMap(base_uv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw2 * sampleShadowMap(base_uv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw2 * sampleShadowMap(base_uv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw3 * vw2 * sampleShadowMap(base_uv, u3, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // sum += uw0 * vw3 * sampleShadowMap(base_uv, u0, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw1 * vw3 * sampleShadowMap(base_uv, u1, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw2 * vw3 * sampleShadowMap(base_uv, u2, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
        // sum += uw3 * vw3 * sampleShadowMap(base_uv, u3, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

        // return sum * 1.0f / 2704;

    // #endif
// }

// //-------------------------------------------------------------------------------------------------
// // Samples the appropriate shadow map cascade
// //-------------------------------------------------------------------------------------------------
// float3 SampleShadowCascade(in float3 shadowPosition, in float3 shadowPosDX,
                           // in float3 shadowPosDY, in uint cascadeIdx,
                           // in uint2 screenPos)
// {
    // shadowPosition += CascadeOffsets[cascadeIdx].xyz; 
    // shadowPosition *= CascadeScales[cascadeIdx].xyz;             

    // shadowPosDX *= CascadeScales[cascadeIdx].xyz; 
    // shadowPosDY *= CascadeScales[cascadeIdx].xyz;  

    // float3 cascadeColor = 1.0f;

    // // #if VisualizeCascades_
        // // const float3 CascadeColors[g_SplitCount] =
        // // {
            // // float3(1.0f, 0.0, 0.0f),
            // // float3(0.0f, 1.0f, 0.0f),
            // // float3(0.0f, 0.0f, 1.0f),
            // // float3(1.0f, 1.0f, 0.0f)
        // // };

        // // cascadeColor = CascadeColors[cascadeIdx];
    // // #endif

    // // #if UseEVSM_
        // // float shadow = SampleShadowMapEVSM(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
    // // #elif ShadowMode_ == ShadowModeVSM_
        // // float shadow = SampleShadowMapVSM(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
    // // #elif ShadowMode_ == ShadowModeFixedSizePCF_
        // // float shadow = SampleShadowMapFixedSizePCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
    // // #elif ShadowMode_ == ShadowModeGridPCF_
        // // float shadow = SampleShadowMapGridPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
    // // #elif ShadowMode_ == ShadowModeRandomDiscPCF_
        // // float shadow = SampleShadowMapRandomDiscPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx, screenPos);
    // // #else //if ShadowMode_ == SampleShadowMapOptimizedPCF_
         // float shadow = SampleShadowMapOptimizedPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
    // // #endif

    // return shadow * cascadeColor;
// }

// //-------------------------------------------------------------------------------------------------
// // Calculates the offset to use for sampling the shadow map, based on the surface normal
// //-------------------------------------------------------------------------------------------------
// float3 GetShadowPosOffset(in float nDotL, in float3 normal)
// {
    // float texelSize = 2.0f / g_ShadowMapSize.x;
    // float normalOffsetScale = saturate(1.0f - nDotL);
    // return texelSize * g_OffsetScale * normalOffsetScale * normal;
// }

// //-------------------------------------------------------------------------------------------------
// // Computes the visibility term by performing the shadow test
// //-------------------------------------------------------------------------------------------------
// float3 ShadowVisibility(in float3 positionWS, in float depthVS, in float nDotL, in float3 normal,
                        // in uint2 screenPos)
// {
	// float3 shadowVisibility = 1.0f;
	// uint cascadeIdx = 0;

	// // Figure out which cascade to sample from
	// [unroll]
	// for(int i = 0; i < g_SplitCount - 1; ++i)
	// {
		// [flatten]
		// if(depthVS > g_SplitDistances[i])
			// cascadeIdx = i + 1;
	// }

    // // Apply offset
    // float3 offset = GetShadowPosOffset(nDotL, normal) / abs(CascadeScales[cascadeIdx].z);

    // // Project into shadow space
    // float3 samplePos = positionWS + offset;
	// float3 shadowPosition = mul(float4(samplePos, 1.0f), ShadowMatrix).xyz;
    // float3 shadowPosDX = ddx_fine(shadowPosition);
    // float3 shadowPosDY = ddy_fine(shadowPosition);

	// shadowVisibility = SampleShadowCascade(shadowPosition, shadowPosDX, shadowPosDY,
                                           // cascadeIdx, screenPos);

    // // #if FilterAcrossCascades_
        // // // Sample the next cascade, and blend between the two results to
        // // // smooth the transition
        // // const float BlendThreshold = 0.1f;
        // // float nextSplit = g_SplitDistances[cascadeIdx];
        // // float splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - g_SplitDistances[cascadeIdx - 1];
        // // float splitDist = (nextSplit - depthVS) / splitSize;

        // // [branch]
        // // if(splitDist <= BlendThreshold && cascadeIdx != g_SplitCount - 1)
        // // {
            // // float3 nextSplitVisibility = SampleShadowCascade(shadowPosition, shadowPosDX,
                                                             // // shadowPosDY, cascadeIdx + 1,
                                                             // // screenPos);
            // // float lerpAmt = smoothstep(0.0f, BlendThreshold, splitDist);
            // // shadowVisibility = lerp(nextSplitVisibility, shadowVisibility, lerpAmt);
        // // }
    // // #endif

	// return shadowVisibility;
// }

// // http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/
// // http://mynameismjp.wordpress.com/2013/09/10/shadow-maps/
// float2 GetShadowOffsets(float3 N, float3 L) 
// {
    // float cos_alpha = saturate(dot(N, L));
    // float offset_scale_N = sqrt(1 - cos_alpha*cos_alpha); // sin(acos(L·N))
    // float offset_scale_L = offset_scale_N / cos_alpha;    // tan(acos(L·N))
    // return float2(offset_scale_N, min(2, offset_scale_L));
// }

#if defined (SHADOW_MAPPING_ENABLED)
half GetSplitByDepth(float fDepth)
{
	#if defined (NUM_SPLITS_4)
		float4 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_3)
		float3 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_2)
		float2 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_1)
		float fTest = fDepth > g_fSplitDistances;
	#endif
	return dot(fTest, fTest);
}

float3 GetShadow(half fSplitIndex, float2 uv, float4 lightDepths)
{
	float3 shadowColor = float3(1.0f, 1.0f, 1.0f);

    float receiver = lightDepths[fSplitIndex];	
	float occluder = tex2D(sampShadowMap, uv)[fSplitIndex]; // rgba channels accessible individually via fSplitIndex
	
	float shadow = saturate(exp(max(ESM_MIN, ESM_K * (occluder - receiver))));
	shadow = 1.0f - (ESM_DIFFUSE_SCALE * (1.0f - shadow));
    //if (shadow < 0.8f) shadow = max(SHADOW_COLOR, shadow.rrr);
    // return shadow;
	
	// Pixels that are further from the light than the depth
	// in the projected light camera's depth buffer are shadowed. 
	float shadowFactor = receiver - DEPTH_BIAS > occluder;
	shadowFactor = 1.0f - shadowFactor * SHADOW_OPACITY;
	//return shadowFactor;

	shadowColor *= SHADOW_OPACITY * shadowFactor;
	return shadowColor;
}
#endif

float GetEdge(float3 normal, float3 viewDir)
{
	// Rim edge detection - look for the pixels nearly
	// 90 degrees from view direction and lowers their intensity (turns them toward black.)
	float rimEdgeAngleTolerance = 0.3f; // range 0 - 1.0f
	viewDir = normalize (viewDir);
	float edge = saturate(dot (normal, viewDir));
	edge = edge < rimEdgeAngleTolerance ? edge / 4.0f : 1.0f;
	return edge;
}

//-------------------------------------------------------------------------------------------------
// smoothstep2 for grid - the intrinsic HLSL smoothstep
// creates a curve that just goes from zero up to one.
// We want one that goes from zero up to one, and then
// back down to zero again. 
//-------------------------------------------------------------------------------------------------
float smoothstep2( float min1, float max1, float input )
{
	float result = smoothstep (min1, max1, input);
	result *= (1 - result);
	return result;
	//return smoothstep(min1, max1, input) * (1 - smoothstep(min2, max2, input));
}

#if defined (TERRAIN_GRID_ENABLE)
float4 ComputeGridColor (float4 diffuseColor, float3 positionWS, float distance)
{
	// TODO: the current bug is caused when lines at the top of a terrain tile
	//       fall along the edge of that tile and get drawn on whatever geometry it
	//       catches on the way down.  The only way to fix this is to exclude
	//       drawing when the positionWS.y is out of range.
	
	// Grid - NOTE: we offset our grid coords IN.PositionWS by camera's relative position so they remain fixed relative to 
	//        our floating origin based camera rendering.
	float2 coords;   
	coords[0] = positionWS.x + g_CameraOffset.x;
	coords[1] = positionWS.z + g_CameraOffset.z;
	coords =  frac(coords * GridTiling2D);
	
	float2 weights;
	
	// these weights will produce values close to or at 1.0f when near the grid thickness
	// but then quickly drop off to 0.0 so underlying diffuse of terrain shows through.
	float x = GridThickness;
	weights[0] = smoothstep2(0.0, x, coords[0]);
	float z = 1.0f - GridThickness;
	weights[1] = smoothstep2(z, 1.0, coords[1]);
	
	// gradiant lerp using GridLineStrength between grid color and underlying color 
	// instead of just one or the other provides a blended look with less jagged 
	// line artifacts in the distance
	float w = saturate(dot(weights, float2(GridLineStrength, GridLineStrength)));
		
	// fade grid out completely if too far away from eye
	w *= saturate((GridFadeEnd - distance) / (GridFadeEnd - GridFadeStart));
		
	return lerp( diffuseColor, GridColour, w);
}

float4 ComputeGridColor3 (float4 diffuseColor, float3 positionWS, float distance)
{
	// Grid - NOTE: we offset our grid coords IN.PositionWS by camera's relative position so they remain fixed relative to 
	//        our floating origin based camera rendering.
	float3 coords;   
	coords[0] = positionWS.x + g_CameraOffset.x;
	coords[1] = positionWS.y + g_CameraOffset.y;
	coords[2] = positionWS.z + g_CameraOffset.z;
	coords =  frac(coords * GridTiling3D);

	// each weight will allow us to compute a different axis or pattern and when
	// all weights combined, the dotproduct() will preserve highest values.
	float3 weights;
	
	// these weights will produce values close to or at 1.0f when near the grid thickness
	// but then quickly drop off to 0.0 so underlying diffuse of terrain shows through.
	float x = GridThickness;
	weights[0] = smoothstep2(0.0f, x, coords[0]);
	float y = x;	
	weights[1] = 0.0f; // smoothstep2(0.0f, 1.0f, coords[1]);
	
	float z = 1.0f - GridThickness;
	weights[2] = smoothstep2(z, 1.0f, coords[2]);
	
	// modify each weight to fade away if Y altitude is at wrong interval
	// TODO: i could subtract semantic worldposition from coords[] and then smoothstep
	// using fixed 0 - 2.82983 or simply return diffuseColor if we're below 2.8
	
//	float tmp = smoothstep2(0.0f, 2.82983f, coords[1]);
	// <-- if that weight is below a minimum threshold, then we set weights[0] and weights[2] = 0
	
//	weights[0] *= tmp;
//	weights[2] *= tmp;
	
	// gradiant lerp using GridLineStrength between grid color and underlying color 
	// instead of just one or the other provides a blended look with less jagged 
	// line artifacts in the distance
	float w = saturate(dot(weights, float3(GridLineStrength, GridLineStrength, GridLineStrength)));
		
	// fade grid out completely if too far away from eye
	w *= saturate((GridFadeEnd - distance) / (GridFadeEnd - GridFadeStart));
	
	return lerp( diffuseColor, GridColour, w);
}
#endif

//-------------------------------------------------------------------------------------------------
// fog
//-------------------------------------------------------------------------------------------------
float4 ComputeFogColor(float4 diffuseColor, float dist) 
{
	float amount = (fogType == FOG_TYPE_NONE) +
			1.0f / exp(dist * fogDensity) * (fogType == FOG_TYPE_EXP) +
			1.0f / exp(pow(dist * fogDensity, 2.0f)) * (fogType == FOG_TYPE_EXP2) +
			saturate((fogEnd - dist) / (fogEnd - fogStart)) * (fogType == FOG_TYPE_LINEAR);
			
	// todo: shouldn't fogColor be float3 and if it is float4, what value is in .a?
	//       because we shouldn't be lerping alpha value.  we should keep existing
	//       diffuseColor.a
    return lerp(fogColor, diffuseColor, amount);
}

float WrapLambert (float NdotL) 
{
	// +0.5 seems to prevent diffuse from ever being 0 even for
	// polys that are completely facing away from light source.
	// WrapLambert should be used only in certain cases (eg no shadowmapping is used)
	// where you want ambient effect without ambient material and lighting.
	float diff = NdotL * 0.5f + 0.5f;
	return diff;
	
	// pointlight attenuation?
	// float4 c;
	// c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
	// c.a = s.Alpha;
	// return c;
}

// float RampLambert (float NdotL)
// {
//	//	requires 1D rampTexture is filled
	// float rampCoord = WrapLambert (NdotL);
    // return tex1D(rampTex, rampCoord);
// }

// Calculates the Diffuse Lighting Model contribution based on the selected model
// NdotL term will be closer to 0 as poly faces away from light.  If it completely
// faces away from light, the shading will be absolute since NdotL will be 0
// and multiplied by diffuse color material or texture will be 0,0,0 rgb
float DiffuseLighting (float NdotL)
{
	#if defined (LAMBERT)
		return NdotL;
	#elif defined(WRAP_LAMBERT)
		return WrapLambert(NdotL);
	#elif defined(RAMP_LAMBERT)
		return RampLabert (NdotL);
	#else 
		return 0;
	#endif
}

// Specular reflection models
float Phong(float3 normal, float3 view, float3 lightDir)
{
	float3 reflected = reflect(lightDir, normal);
	return pow(saturate(dot(reflected, view)), materialPower);
}

float BlinnPhong(float3 normal, float3 view, float3 lightDir)
{					
	//half vector average of the light and the view vectors
	float3 halfway = normalize(lightDir + view);
	return pow(saturate(dot(normal, halfway)), materialPower);
}

float Lyon(float3 normal, float3 view, float3 lightDir)
{					
	//half vector average of the light and the view vectors
	float3 halfway = normalize(lightDir + view);
	float3 diff = halfway - normal;
	float tmp = saturate(dot(diff, diff) * materialPower / 2);
	return pow(1 - tmp, 3);
}

float TorranceSparrow(float3 normal, float3 view, float3 lightDir)
{
	//half vector average of the light and the view vectors
	float3 halfway = normalize(lightDir + view);
	float NdH = dot(normal, halfway);
	float tmp = acos(NdH);
	return exp(-2 * materialPower * pow(tmp, 2));
}

float TrowbridgeReitz(float3 normal, float3 view, float3 lightDir)
{
	//half vector average of the light and the view vectors
	float3 halfway = normalize(lightDir + view);
	float NdH = saturate(dot(normal, halfway));
	return pow(1 / (1 + (1 - pow(NdH, 2)) * materialPower), 2);
}

// Calculates the specular contribution based on the selected model
float SpecularLighting(float3 normal, float3 view, float3 lightDir) 
{
	 #if defined (PHONG)
		 return Phong(normal, view, lightDir);
	 #elif defined(BLINN_PHONG)
		 return BlinnPhong(normal, view, lightDir);
	 #elif defined(LYON)
		 return Lyon(normal, view, lightDir);
	 #elif defined(TORRANCE_SPARROW)
		 return TorranceSparrow(normal, view, lightDir);
	 #elif defined(TROWBRIDGE_REITZ)
		 return TrowbridgeReitz(normal, view, lightDir);
	 #else
		 return 0;
	 #endif	
}

// Calculate the final pixel color using all calculated components
float4 ComputeColor(float4 textureDiffuse, 
					float3 diffuseLighting, 
					float3 shadowColor, 
					float3 specularLighting, 
					float2 texCoord) //, float specularMask
{
	float3 AmbientLightColor = float3(1,1,1);
		
	float3 emissiveColor = materialEmissive.rgb; // * tex2D(sampEmissive, texCoord).rgb;
	float3 ambientColor  = materialAmbient.rgb; // * AmbientLightColor;  // <-- materialAmbient assumes ambientIntensity = 1.0 and that intensity is baked into materialAmbient's values.
    float multiplier  = 2.0; // TODO: can make multiplier a shader var modifiable at run time by client
	float3 diffuseColor  = materialDiffuse.rgb * diffuseLighting * shadowColor * multiplier; 

	// Use light specular vector as intensity
	multiplier = 1.0f;
	float3 specularColor = materialSpecular.rgb * specularLighting; // * tex2D(sampSpecular, texCoord).rgb;;  multiplier

	float alpha = textureDiffuse.a * materialDiffuse.a;
   
	// combine output color
	float3 color = (saturate(ambientColor + diffuseColor) * textureDiffuse.rgb + specularColor) * g_DirLightColor + emissiveColor;
	return float4 (color, alpha);
}

#if defined (SHADOW_MAPPING_ENABLED)
// -------------------------------------------------------------
// VERTEX SHADER - TECHNIQUE 'SHADOWED'
// -------------------------------------------------------------
#if defined (GEOMETRY_ACTOR)
VS_OUTPUT_SHADOWED VS_Shadowed(uniform int iNumBones, VS_INPUT_SHADOWED IN)
#else
VS_OUTPUT_SHADOWED VS_Shadowed(VS_INPUT_SHADOWED IN)
#endif
{
	VS_OUTPUT_SHADOWED OUT;

#if defined (GEOMETRY_ACTOR)
	// actor skinning method 
	float3 pos, norm; 	// out parameters
	TransPositionNormal( iNumBones, float4(IN.PositionMS, 1.0f), IN.NormalOS, IN.Blendweights, IN.Blendindices, pos, norm);
	float4 positionWS = float4(pos, 1.0f);
	IN.NormalOS = norm;
#elif defined (GEOMETRY_MINIMESH)
	float4x3 worldMatrix = g_World[(int)IN.Index];
	float4 positionWS = float4(mul(float4(IN.PositionMS, 1.0f), worldMatrix), 1.0f); // g_World for minis is 4x3
	// normals are transformed only by 3x3 portion of the world matrix
	IN.NormalOS = mul(IN.NormalOS,(float3x3)worldMatrix);
#elif defined (GEOMETRY_INSTANCING)
	// TODO: here we need to compute a worldMatrix from the Position and Rotation of the instance
	//       if we want WS transformed normals.
	//       Otherwise for positionWS, those are passed in per instance in the InstancePositions[]
	int instanceIndex = (int)IN.Index;
	//float4 positionWS = float4(InstancePositionsWS[instanceIndex].xyz, 0);
	float4x4 instanceMatrix = GetInstanceMatrix (InstancePositionsWS[instanceIndex]);
	float4 positionWS = mul(IN.PositionMS, instanceMatrix);
	IN.NormalOS = mul(IN.NormalOS,(float3x3)instanceMatrix);
#else 
	// Mesh3d, Billboard, Terrain
	float4 positionWS = mul(float4(IN.PositionMS, 1.0f), g_World); // g_World for meshes is 4x4
	// normals are transformed only by 3x3 portion of the world matrix
	IN.NormalOS = mul(IN.NormalOS, (float3x3)g_World);
#endif

	OUT.ViewDir = normalize(g_ViewPosition - positionWS);
	// transform WS position into clip space using view proj matrix
	OUT.PositionCS = mul(positionWS, ViewProjection);
	OUT.PositionWS = positionWS;
    OUT.NormalWS = IN.NormalOS;
	// modelspace UV but we store the clipspace depth in the .z component
	OUT.UVandDepth.xy = IN.UV;
	OUT.UVandDepth.z = OUT.PositionCS.z;  // OUT.PositionCS.w;

	// transform worldspace position to the UV coords of the projected lightspace depth maps
	// NOTE: when rendering depth pass to RenderSurface we use the light's ViewProjection
	// matrices but we share the same WorldSpace position for the objects as in the normal render pass.
	// That is why we use positionWS below and not modelspace position.	
	OUT.ProjectedLightSpaceUV[0].xy = mul(positionWS, g_mLightViewProjTextureMatrix[0]).xy;
	OUT.ProjectedLightSpaceUV[0].zw = mul(positionWS, g_mLightViewProjTextureMatrix[1]).xy;
	OUT.ProjectedLightSpaceUV[1].xy = mul(positionWS, g_mLightViewProjTextureMatrix[2]).xy;
	OUT.ProjectedLightSpaceUV[1].zw = mul(positionWS, g_mLightViewProjTextureMatrix[3]).xy;

	float4 mLightSpace;
	// for each split level 
	for(int n = 0; n < 4; n++ )
	{
		// transform worldspace position to clipspace of light camera
		mLightSpace =  mul(positionWS, g_mLightViewProj[n]);
		// compute zbuffer value using light camera's clipspace
		OUT.fLightDepth[n] = mLightSpace.z / mLightSpace.w ; // - DEPTH_BIAS;
	}

	return OUT;
}

// -------------------------------------------------------------
// PIXEL SHADER - TECHNIQUE 'SHADOWED'
// -------------------------------------------------------------
float4 PS_Shadowed(VS_OUTPUT_SHADOWED IN) : COLOR0
{
    float4 diffuseTexture = float4(1,1,1,1); 
	#if defined (TEXTURE_MAPPING_ENABLED)
	//  TODO: is there a way to avoid 0,0,0,1 being returned from tex2d() if diffuseSampler does not exist?
		diffuseTexture = tex2D(diffuseSampler, IN.UVandDepth.xy);
	#else
		#if defined (ALPHA_TEST_ENABLED)
		// even if .rgb texture mapping is not enabled, we must read the .alpha
		// in order to render things like leaves and grass and fences
		diffuseTexture.a = tex2D(diffuseSampler, IN.UVandDepth.xy).a;
		
		// clip(value) aborts PS if 'value' is negative (but apparently this does not result in early exit
		// because it acts like a branch and the rest of the function occurs anyway and final
		// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
		// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
		// TODO: 0.5f should be the alpharef value.  Is there a tv semantic for that?
		clip(diffuseTexture.a - 0.5f);
		#endif
	#endif

    // Lighting
	float3 normalWS = normalize(IN.NormalWS); // normalize after interpolation
	float NdotL = saturate(dot (normalWS, g_DirLightDirection));
    float diffuseLighting = DiffuseLighting (NdotL);
	// TODO: am I supposed to normalize the ViewDir in vertex shader first and/or not at all there or here?
	// TODO: in general, when to normalize things and when not to.  I do understand that per-pixel-lighting
	// requires interpolating and normalizing more for each pixel, however, that doesn't mean everything needs to be such as view direction?  And then if a pointlight, what about light direction?  I'm sure it's a case of how accurate do we want to be and what is "good enough" and even unnoticable to do it the more expensive way	
	float specularLighting = SpecularLighting(normalWS, normalize(IN.ViewDir), g_DirLightDirection);
        
	// Shadow - get value to lower pixel intensity by depending on whether pixel is in shadow or not
	half fSplitIndex = GetSplitByDepth(IN.UVandDepth.z);
	float2 SplitUV[4];
	SplitUV[0] = IN.ProjectedLightSpaceUV[0].xy;
	SplitUV[1] = IN.ProjectedLightSpaceUV[0].zw;
	SplitUV[2] = IN.ProjectedLightSpaceUV[1].xy;
	SplitUV[3] = IN.ProjectedLightSpaceUV[1].zw;
	//float2 offset =	GetShadowOffsets (normalWS, g_DirLightDirection);
	float2 uv = SplitUV[fSplitIndex].xy; // * offset;
	float3 shadowColor = GetShadow(fSplitIndex, uv, IN.fLightDepth);
		
	// final color
	float4 diffuseColor = ComputeColor (diffuseTexture, diffuseLighting, shadowColor, specularLighting, IN.UVandDepth.xy); 

	// draw edges using rim lighting 
	//	float edge = GetEdge(normalWS, IN.ViewDir);
	//	diffuseColor *= float4(edge, edge, edge, 1.0f);
	#ifdef CEL_SHADING
        float4 outlineColor = edgeDetect(IN.UVandDepth);
        if (outlineColor.a != 0.0) 
		{
			diffuse.rgb = outlineColor.rgb;
        }
	#endif
	
	#if defined (TERRAIN_GRID_ENABLE)
		diffuseColor = ComputeGridColor3 (diffuseColor, IN.PositionWS.xyz, IN.UVandDepth.z);
	#endif
	
	// Fog - UVandDepth.z contains clipspace depth from camera, not depth map distance obviously
	// diffuseColor = ComputeFogColor (diffuseColor, IN.UVandDepth.z);
	return diffuseColor;
}

// -------------------------------------------------------------
// VERTEX SHADER - TECHNIQUE 'DEPTH'
// -------------------------------------------------------------
#if defined (GEOMETRY_ACTOR)
VS_OUT_DEPTH VS_DepthMap(uniform int iNumBones, VS_IN_DEPTH IN)
#else
VS_OUT_DEPTH VS_DepthMap(VS_IN_DEPTH IN)
#endif
{
	VS_OUT_DEPTH OUT;

	#if defined (GEOMETRY_ACTOR)
		// actor skinning method
		float3 pos, norm; // out parameters	
		TransPositionNormal( iNumBones, 
						float4(IN.PositionMS, 1.0f), 
						IN.NormalOS, 
						IN.Blendweights, 
						IN.Blendindices, 
						pos, 
						norm);
		float4 positionWS = float4(pos, 1);
	#elif defined (GEOMETRY_MINIMESH)
		float4x3 worldMatrix = g_World[(int)IN.Index]; // g_World for minis is 4x3
		float4 positionWS = float4(mul(float4(IN.PositionMS,1.0f), worldMatrix), 1.0f); 
	#elif defined (GEOMETRY_INSTANCING)
		// NOTE: here we need to compute a worldMatrix from the Position and Rotation 
		//       of the instance if we want WS transformed normals.  Otherwise for positionWS,
		// 		 those are already passed in WS per instance in the InstancePositionsWS[]
		int instanceIndex = (int)IN.Index;
		//float4 positionWS = float4(InstancePositionsWS[instanceIndex].xyz, 0);
		float4x4 instanceMatrix = GetInstanceMatrix (InstancePositionsWS[instanceIndex]);
		float4 positionWS = mul(IN.PositionMS, instanceMatrix);
	#else
		float4 positionWS = mul(float4(IN.PositionMS, 1.0f), g_World); // g_World for meshes is 4x4
	#endif

	// transform worldspace position into clip space using view proj matrix
	OUT.PositionCS = mul(positionWS, ViewProjection);

	// remember this depth is not view centric depth. This VertexShader is running
	// against our RenderSurface using LightViewProjection thus it is light centric
	// depth.  We store this depth at lightviewprojection screenspace UV coords and during 
	// normal render we can determine if the input pixel needs to be shadowed by comparing
	// it's lightviewproject depth value with the one stored in the depth texture.
	OUT.ClipspaceDepth = OUT.PositionCS.z;
	OUT.UV = IN.UV;
	return OUT;
}

// -------------------------------------------------------------
// PIXEL SHADER - TECHNIQUE 'DEPTH'
// -------------------------------------------------------------
float4 PS_DepthMap(VS_OUT_DEPTH IN) : COLOR0
{
	// alpha - clip aborts if evaluates to negative
	// clip aborts PS if value is negative (but apparently this does not result in early exit
	// because it acts like a branch and the rest of the function occurs anyway and final
	// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
	// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
	clip(tex2D(diffuseSampler, IN.UV).a - 0.5f);
	
	return IN.ClipspaceDepth;
}
#else






// -------------------------------------------------------------
// VERTEX SHADER - TECHNIQUE 'DEFAULT'
// -------------------------------------------------------------
#if defined (GEOMETRY_ACTOR)
VS_OUTPUT_DEFAULT VS_Default(uniform int iNumBones, VS_INPUT_DEFAULT IN)
#else
VS_OUTPUT_DEFAULT VS_Default(VS_INPUT_DEFAULT IN)
#endif
{
	VS_OUTPUT_DEFAULT OUT;

#if defined (GEOMETRY_ACTOR)
	// actor skinning method 
	float3 pos, norm; 	// out parameters
	TransPositionNormal( iNumBones, float4(IN.PositionMS, 1.0f), IN.NormalOS, IN.Blendweights, IN.Blendindices, pos, norm);
	float4 positionWS = float4(pos, 1.0f);
#elif defined (GEOMETRY_MINIMESH)
	float4x3 worldMatrix = g_World[(int)IN.Index];
	float4 positionWS = float4(mul(float4(IN.PositionMS, 1.0f), worldMatrix), 1.0f); // g_World for minis is 4x3
	// normals are transformed only by 3x3 portion of the world matrix
	float3 norm = mul(IN.NormalOS,(float3x3)worldMatrix);
#elif defined (GEOMETRY_INSTANCING)
	// TODO: here we need to compute a worldMatrix from the Position and Rotation of the instance
	//       if we want WS transformed normals.
	//       Otherwise for positionWS, those are passed in per instance in the InstancePositions[]
	int instanceIndex = (int)IN.Index;
	//float4 positionWS = float4(InstancePositionsWS[instanceIndex].xyz, 0);
	float4x4 instanceMatrix = GetInstanceMatrix (InstancePositionsWS[instanceIndex]);
	float4 positionWS = mul(IN.PositionMS, instanceMatrix);
	float3 norm = mul(IN.NormalOS,(float3x3)instanceMatrix);
#else
	// Mesh3d, Billboard, Terrain
	float4 positionWS = mul(float4(IN.PositionMS, 1.0f), g_World); // g_World for meshes is 4x4
	// normals are transformed only by 3x3 portion of the world matrix
	float3 norm = mul(IN.NormalOS, (float3x3)g_World);
#endif

	OUT.ViewDir = g_ViewPosition - positionWS;
	// transform WS position into clip space using view proj matrix
	OUT.PositionCS = mul(positionWS, ViewProjection);
	OUT.PositionWS = positionWS;
    OUT.NormalWS = norm;
	OUT.UV = IN.UV;

	return OUT;
}

// -------------------------------------------------------------
// PIXEL SHADER - TECHNIQUE 'DEFAULT'
// -------------------------------------------------------------
float4 PS_Default(VS_OUTPUT_DEFAULT IN) : COLOR0
{
	float4 diffuseTexture = float4(1.0f, 1.0f, 1.0f, 1.0f); 
	#if defined (TEXTURE_MAPPING_ENABLED)
	//  TODO: is there a way to avoid 0,0,0,1 being returned from tex2d() if diffuseSampler does not exist?
		diffuseTexture = tex2D(diffuseSampler, IN.UV);
	#else
		#if defined (ALPHA_TEST_ENABLED)
			// even if .rgb texture mapping is not enabled, we must read the .alpha
			// in order to render things like leaves and grass and fences
			diffuseTexture.a = tex2D(diffuseSampler, IN.UV).a;
			
			// clip(value) aborts PS if 'value' is negative (but apparently this does not result in early exit
			// because it acts like a branch and the rest of the function occurs anyway and final
			// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
			// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
			// TODO: 0.5f should be the alpharef value.  Is there a tv semantic for that?
			clip(diffuseTexture.a - 0.5f);
		#endif
	#endif
	
    // Lighting
	float3 normalWS = normalize(IN.NormalWS); // normalize after interpolation (confirm as with IN.ViewDir, normalize in PS and not also/or in VS)
	float NdotL = saturate(dot (normalWS, g_DirLightDirection));
    float diffuseLighting = DiffuseLighting (NdotL);
	// TODO: am I supposed to normalize the ViewDir in vertex shader first and/or not at all there or here?
	// TODO: in general, when to normalize things and when not to.  I do understand that per-pixel-lighting
	// requires interpolating and normalizing more for each pixel, however, that doesn't mean everything needs to be such as view direction?  And then if a pointlight, what about light direction?  Perhaps it's a case of how accurate do we want to be and what is "good enough" and even unnoticable to do it the more expensive way	
	float specularLighting = SpecularLighting(normalWS, normalize(IN.ViewDir), g_DirLightDirection);
        
	// final color
	float3 shadowColor = float3(1.0f, 1.0f, 1.0f);
	float4 diffuseColor = ComputeColor (diffuseTexture, diffuseLighting, shadowColor, specularLighting, IN.UV); 

	// draw edges using rim lighting 
	// float edge = GetEdge(normalWS, IN.ViewDir);
	// diffuseColor *= float4(edge, edge, edge, 1.0f);
	#ifdef CEL_SHADING
        float4 outlineColor = edgeDetect(float3(IN.UV, IN.PositionCS.z));
        if (outlineColor.a != 0.0f) 
		{
			diffuse.rgb = outlineColor.rgb;
        }
	#endif
	
	#if defined (TERRAIN_GRID_ENABLE)
		diffuseColor = ComputeGridColor3 (diffuseColor, IN.PositionWS.xyz, IN.PositionCS.z);
	#endif
	
	// Fog - UVandDepth.z contains clipspace depth from camera, not depth map distance obviously
	// diffuseColor = ComputeFogColor (diffuseColor, IN.UVandDepth.z);
	return diffuseColor;
}
#endif

#if defined (GEOMETRY_ACTOR)
	#if defined (SHADOW_MAPPING_ENABLED)
		VertexShader VSArray[5] = 
					{ 
						compile vs_3_0 VS_Shadowed(0),
						compile vs_3_0 VS_Shadowed(1),
						compile vs_3_0 VS_Shadowed(2),
						compile vs_3_0 VS_Shadowed(3),
						compile vs_3_0 VS_Shadowed(4)
					  };
					  
		VertexShader VSDArray[5] = 
					{ 
						compile vs_3_0 VS_DepthMap(0),
						compile vs_3_0 VS_DepthMap(1),
						compile vs_3_0 VS_DepthMap(2),
						compile vs_3_0 VS_DepthMap(3),
						compile vs_3_0 VS_DepthMap(4)
					  };
	#else
		VertexShader VSArray[5] = 
					{ 
						compile vs_3_0 VS_Default(0),
						compile vs_3_0 VS_Default(1),
						compile vs_3_0 VS_Default(2),
						compile vs_3_0 VS_Default(3),
						compile vs_3_0 VS_Default(4)
					  };
	#endif
#endif
		
// -------------------------------------------------------------
// TECHNIQUES
// -------------------------------------------------------------
#if defined (SHADOW_MAPPING_ENABLED)		
	technique depth
	{
		pass pass0
		{
			cullmode = ccw;
			//AlphablendEnable = false;
			//AlphaTestEnable = false;
			
			#if defined (GEOMETRY_ACTOR)
				VertexShader = ( VSDArray[g_NumBonesPerVertex] );
			#else
				VertexShader = compile vs_3_0 VS_DepthMap();
			#endif
			PixelShader = compile ps_3_0 PS_DepthMap();
		}
	}

	technique shadowed
	{
		pass pass0
		{
			cullmode = ccw;
			//AlphablendEnable = false;
			//AlphaTestEnable = false;
			
			#if defined (GEOMETRY_ACTOR)
				VertexShader = ( VSArray[g_NumBonesPerVertex] );
			#else
				VertexShader = compile vs_3_0 VS_Shadowed();
			#endif
			PixelShader = compile ps_3_0 PS_Shadowed();
		}
	}
#else

	technique noshadows
	{
		pass pass0
		{
			cullmode = ccw;
			//AlphablendEnable = false;
			//AlphaTestEnable = true;
			
			#if defined (GEOMETRY_ACTOR)
				VertexShader = ( VSDArray[g_NumBonesPerVertex] );
			#else
				VertexShader = compile vs_3_0 VS_Default();
			#endif
			PixelShader = compile ps_3_0 PS_Default();
		}
	}

#endif