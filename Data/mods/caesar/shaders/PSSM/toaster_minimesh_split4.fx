// -------------------------------------------------------------
/// Minimesh Shader - Textured + Normal Mapping + Shadow Mapping
// -------------------------------------------------------------

// -------------------------------------------------------------
// Geometry compilation switches
// -------------------------------------------------------------
// #define GEOMETRY_MESH
// #define GEOMETRY_TERRAIN  // a type of Mesh that uses splatting and slope texture lookups
// #define GEOMETRY_TEXTURED_QUAD	
// #define GEOMETRY_MINIMESH
// #define GEOMETRY_ACTOR

// -------------------------------------------------------------
// PSSM compilation switches
// -------------------------------------------------------------
// #define ESM 
//		- rather than linear, depth values are stored scaled exponentially by distance to camera
// #define NUM_SPLITS_2
//		- frustum is split into 2 halves
// #define NUM_SPLITS_3
//		- frustum is split into 3 parts
// #define NUM_SPLITS_4
//		- frustum is split into 4 parts

// -------------------------------------------------------------
// Fog Constants
// -------------------------------------------------------------
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3 

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x3 World[52] : MINIMESH_WORLD;
float4x4 g_mCameraViewProj : VIEWPROJ;
float3 viewPosition : VIEWPOSITION;

// -------------------------------------------------------------
///Light Var's
// -------------------------------------------------------------
float3 g_vLightDirection;
float3 dirLightColor : LIGHTDIR0_COLOR;

// for shadow path we should eventually use below light vars, but currently not
// also the array is designed for 6 pointlights really... 
float3 LightPos[6]; //our light position in object space
float3 LightColors[6]; //our light Colors
float LightRanges[6]; //Light Range
int LightNum = 0;

// -------------------------------------------------------------
///Material Var's
// -------------------------------------------------------------
float4 materialDiffuse :     DIFFUSE;
float4 materialAmbient :     AMBIENT;
float4 materialEmissive :    EMISSIVE;
float3 materialSpecular :    SPECULAR;
float materialPower :        SPECULARPOWER;

// -------------------------------------------------------------
///Fog Var's
// -------------------------------------------------------------
int fogType : FOGTYPE;
float3 fogColor : FOGCOLOR;
float fogDensity : FOGDENSITY;
float fogStart : FOGSTART;
float fogEnd : FOGEND;

// -------------------------------------------------------------
///Shadow Var's
// -------------------------------------------------------------
float4x4 g_mShadowMap[4];
float4x4 g_mLightViewProj[4];
#if defined (NUM_SPLITS_4)
	float4 g_fSplitDistances;
#elif defined (NUM_SPLITS_3)
	float3 g_fSplitDistances;
#elif defined (NUM_SPLITS_2)
	float2 g_fSplitDistances;
#endif


float SHADOW_OPACITY = 0.45f;
	
// -------------------------------------------------------------
///Exponential Shadow Map Var's which stores depth info in a scaled way (as opposed to linear)
// TODO: I should make ESM a #define
// -------------------------------------------------------------
float ESM_K = 60.0f; // Range [0, 80]
float ESM_MIN = -0.5f; // Range [0, -oo]
float ESM_DIFFUSE_SCALE = 1.79f; // Range [1, 10]

// -------------------------------------------------------------
/// Textures & Samplers
// -------------------------------------------------------------
texture texShadowMap;
sampler2D sampShadowMap  = sampler_state
{
	Texture = (texShadowMap);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = 0x000000;  // black border color.  "BorderColor" causes Microsoft Direct3D to 
							 // use an arbitrary color, known as the border color, for any texture 
							 // coordinates outside of the range of 0.0 through 1.0, inclusive.
};

texture BaseTex : TEXTURE0;
sampler2D BaseSampler = sampler_state
{
    Texture = <BaseTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture NormalTex : TEXTURE1;
sampler2D NormalSampler = sampler_state
{
    Texture = <NormalTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

// -------------------------------------------------------------
/// Shadow Shader
// -------------------------------------------------------------

struct VS_SHADOWED_INPUT
{
	float4 Position : POSITION;
	float3 vNormal : NORMAL;
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
	float2 vTextureUV : TEXCOORD0;
	float2 Index : TEXCOORD3; //This is ALWAYS on 3.
};

struct VS_SHADOWED_OUTPUT
{
	float4 vScreenSpacePosition : POSITION;
	float3 vNormal : TEXCOORD0;
	float3 vDepthAndTextureUV : TEXCOORD1;
	float4 fLightDepth : TEXCOORD2;
	float3 vViewDir : TEXCOORD3;
	float4 vProjectedLightPosition[2] : TEXCOORD4;
};

VS_SHADOWED_OUTPUT VS_Shadowed(VS_SHADOWED_INPUT IN) 
{
	VS_SHADOWED_OUTPUT OUT;
	
	float4x3 WorldMatrix = World[IN.Index.x];
	float4 WorldPos = float4(mul(IN.Position, WorldMatrix), 1);
	
	OUT.vViewDir = viewPosition - WorldPos;
	OUT.vScreenSpacePosition = mul(WorldPos, g_mCameraViewProj);
	OUT.vNormal = IN.vNormal;
	OUT.vDepthAndTextureUV.xy = IN.vTextureUV;
	OUT.vDepthAndTextureUV.z = OUT.vScreenSpacePosition.z;	

	float4 mLightSpace;
	
	OUT.vProjectedLightPosition[0].xy = mul(WorldPos, g_mShadowMap[0]).xy;
	OUT.vProjectedLightPosition[0].zw = mul(WorldPos, g_mShadowMap[1]).xy;
	OUT.vProjectedLightPosition[1].xy = mul(WorldPos, g_mShadowMap[2]).xy;
	OUT.vProjectedLightPosition[1].zw = mul(WorldPos, g_mShadowMap[3]).xy;
	
	for(int n = 0; n < 4; n++ )
	{
		mLightSpace =  mul(WorldPos, g_mLightViewProj[n]);
		OUT.fLightDepth[n] = mLightSpace.z / mLightSpace.w - 0.001f;
	}
	
	return OUT;
}

half GetSplitByDepth(float fDepth)
{
	#if defined (NUM_SPLITS_4)
		float4 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_3)
		float3 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_2)
		float2 fTest = fDepth > g_fSplitDistances;
	#endif
	return dot(fTest, fTest);
}


float GetShadow(half fSplitIndex, float2 splitUV[4], float4 lightDepths)
{
	float shadow = 0.0f;
    float receiver = lightDepths[fSplitIndex];
	float occluder = tex2D(sampShadowMap, splitUV[fSplitIndex].xy)[fSplitIndex];
	
	shadow = saturate(exp(max(ESM_MIN, ESM_K * (occluder - receiver))));
	shadow = 1.0f - (ESM_DIFFUSE_SCALE * (1.0f - shadow));
	//return shadow;
	
	
	// Pixels that are further from the light than the depth
	// in the projected shadow depth buffer are shadowed. 
	float fShadow = receiver > occluder;
	fShadow = 1.0f - fShadow * SHADOW_OPACITY;
	return fShadow;
}

float GetEdge(float3 normal, float3 viewDir)
{
	// Rim edge detection - look for the pixels nearly
	// 90 degrees from view direction and lowers their intensity (turns them toward black.)
	float rimEdgeAngleTolerance = 0.08; // range 0 - 1.0f
	viewDir = normalize (viewDir);
	float edge = saturate(dot (normal, viewDir));
	edge = edge < rimEdgeAngleTolerance ? edge / 4.0f : 1.0f;
	return edge;
}

// fog
float Fog(float dist) 
{
	return (fogType == FOG_TYPE_NONE) +
			1 / exp(dist * fogDensity) * (fogType == FOG_TYPE_EXP) +
			1 / exp(pow(dist * fogDensity, 2)) * (fogType == FOG_TYPE_EXP2) +
			saturate((fogEnd - dist) / (fogEnd - fogStart)) * (fogType == FOG_TYPE_LINEAR);
}

// Diffuse Lighting Models
// Diffuse Lighting Models
float Lambert(float3 normal, float3 lightDir)
{
	return saturate(dot(normal, lightDir));
}

float WrapLambert (float3 normal, float3 lightDir) 
{
        float NdotL = dot (normal, lightDir);
        float diff = NdotL * 0.5f + 0.5f;
		return diff;
        // float4 c;
        // c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
        // c.a = s.Alpha;
        // return c;
}

float DiffuseLighting (float3 normal, float3 lightDir)
{
	// #ifdef LAMBERT
		// return Lambert(normal, lightDir);
	// #elif defined(WRAP_LAMBERT)
		 return WrapLambert(normal, lightDir);
}

// Phong specular reflection model
float Phong(float3 normal, float3 view, float3 lightDir)
{					
	float3 reflected = reflect(-lightDir, normal);
	return pow(saturate(dot(reflected, view)), materialPower);
}

// Calculates the specular contribution based on the selected model
float Specular(float3 normal, float3 view, float3 lightDir) 
{
	return Phong(normal, view, lightDir);
		// #ifdef PHONG
			// Phong(normal, view, lightDir);
		// #elif defined(BLINN_PHONG)
			// BlinnPhong(normal, view, lightDir);
		// #elif defined(LYON)
			// Lyon(normal, view, lightDir);
		// #elif defined(TORRANCE_SPARROW)
			// TorranceSparrow(normal, view, lightDir);
		// #elif defined(TROWBRIDGE_REITZ)
			// TrowbridgeReitz(normal, view, lightDir);
		// #else
			// 0;
		// #endif	
}

// Calculate the final pixel color using all calculated components
float4 FinalizeColor(float3 diffuse, float3 specular, float2 texCoord) //, float specularMask, float2 vertexFog)
{
	// Constant color contributions
	float4 albedo = tex2D(BaseSampler, texCoord);
	float3 emissive = materialEmissive; // * tex2D(sampEmissive, texCoord).rgb;
	float3 ambient = materialAmbient;    
    			
    // Mask components		
	diffuse *= materialDiffuse.rgb;    
	specular *= materialSpecular; // * specularMask;
	
	// Pixel alpha
	float alpha = albedo.a * materialDiffuse.a;
   
	// Compute output color (with fog)
	float3 color = saturate(ambient + diffuse) * albedo + specular + emissive;     	  
	//color = lerp(color, fogColor, fog);
	//color = color * vertexFog.x + vertexFog.y;
	
    return float4(color, alpha);
}

float4 PS_Shadowed(VS_SHADOWED_OUTPUT IN) : COLOR
{
	// diffuse texture color of the model
    float4 diffuseColor = tex2D(BaseSampler, IN.vDepthAndTextureUV.xy);
	
    // clip aborts PS if value is negative (but apparently this does not result in early exit
	// because it acts like a branch and the rest of the function occurs anyway and final
	// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
	// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
	clip(diffuseColor.a - 0.5f);
	
	half fSplitIndex = GetSplitByDepth(IN.vDepthAndTextureUV.z);

	float2 SplitUV[4];
	SplitUV[0] = IN.vProjectedLightPosition[0].xy;
	SplitUV[1] = IN.vProjectedLightPosition[0].zw;
	SplitUV[2] = IN.vProjectedLightPosition[1].xy;
	SplitUV[3] = IN.vProjectedLightPosition[1].zw;
	
	
    // Intensity based on the direction of the light
    float diffuseIntensity = DiffuseLighting (IN.vNormal, g_vLightDirection) * dirLightColor;
	float specular = Specular(IN.vNormal, viewPosition, g_vLightDirection) * dirLightColor;
	
    // Final diffuse color with ambient color added
    float4 diffuse = FinalizeColor (diffuseIntensity, specular, IN.vDepthAndTextureUV.xy); 
    
	// draw edges using rim lighting 
//	float edge = GetEdge(IN.vNormal, IN.vViewDir);
//	diffuse *= float4(edge, edge, edge, 1.0f);
	
	// get value to lower pixel intensity by depending on whether it's shadowed or not
	float shadow = GetShadow(fSplitIndex, SplitUV, IN.fLightDepth);
		
	// Shadow the pixel by lowering the intensity
    diffuse *= float4(shadow, shadow, shadow, 1.0f);

	// fog
	float2 fog;
	fog.x = Fog (IN.vDepthAndTextureUV.z);
	fog.y = (1.0f - fog.x) * fogColor;
	
	diffuse.rgb = diffuse.rgb * fog.x + fog.y; 
	
	return diffuse;
}

technique shadowed
{
	pass pass0
	{
		AlphablendEnable = true;
		
		VertexShader = compile vs_3_0 VS_Shadowed();
		PixelShader = compile ps_3_0 PS_Shadowed();
	}
}

// -------------------------------------------------------------
// Depth Shader - Transforms vertices to screenspace and renders depth to the enabled color write channel
// -------------------------------------------------------------
void VS_DepthMap
(
	float4 vObjectSpacePosition : POSITION,
	float2 INUV : TEXCOORD0,
	float2 Index : TEXCOORD3, 
	out float4 vScreenSpacePosition : POSITION,
	out float fDepth : TEXCOORD1, 
	out float2 UV : TEXCOORD0)
{
	float4x3 worldMatrix = World[Index.x];
	float4 vWorldSpacePosition = float4(mul(vObjectSpacePosition, worldMatrix), 1);;
	
	vScreenSpacePosition = mul(vWorldSpacePosition, g_mCameraViewProj);
	fDepth = vScreenSpacePosition.z;
	UV = INUV;
}

float4 PS_DepthMap(float fDepth : TEXCOORD1, float2 UV : TEXCOORD0): COLOR
{
	clip(tex2D(BaseSampler, UV).a - 0.5f);
	return fDepth;
}

technique depth
{
	pass p0
	{
		AlphablendEnable = false;
		AlphaTestEnable = false;
		Cullmode = CCW;
		VertexShader = compile vs_3_0 VS_DepthMap();
		PixelShader = compile ps_3_0 PS_DepthMap();
	}
}