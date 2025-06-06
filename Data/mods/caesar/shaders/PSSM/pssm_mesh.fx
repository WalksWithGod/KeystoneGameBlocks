#define SHADOW_OPACITY 0.2f

float4x4 g_mCameraViewProj : VIEWPROJ;
float4x4 g_mWorld : WORLD;						          
float3 g_vLightDirection;
float4x4 g_mShadowMap[4];
float4x4 g_mLightViewProj[4];
float4 g_fSplitDistances;

const float4x3 g_mSplitTestColors = { {1.0f, 0.0f, 0.0f}, 
							          {0.0f, 1.0f, 0.0f}, 
							          {0.0f, 0.0f, 1.0f}, 
							          {1.0f, 1.0f, 0.0f} };		

///////////////////////////////////////////////////////////////////////////////
/// Textures & Samplers
///////////////////////////////////////////////////////////////////////////////
	   
texture texShadowMap;
sampler2D sampShadowMap  =
sampler_state
{
	Texture = (texShadowMap);
	MinFilter = Point; 
	MagFilter = Point;
	MipFilter = None;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = 0xFFFFFF;
};

///////////////////////////////////////////////////////////////////////////////
/// Shadow Shader
///////////////////////////////////////////////////////////////////////////////

struct VS_SHADOWED_INPUT
{
	float4 vObjectSpacePosition : POSITION;
	float2 vTextureUV : TEXCOORD0;
	float3 vNormal : NORMAL;
};

struct VS_SHADOWED_OUTPUT
{
	float4 vScreenSpacePosition : POSITION;
	float3 vNormal : TEXCOORD0;
	float4 vDepthAndTextureUV : TEXCOORD1;
	float4 fLightDepth : TEXCOORD2;
	float2 vProjectedLightPosition[4] : TEXCOORD3;
};

// Half-Lambert
float GetDiffuse(float3 vNormal, float3 vLightDirection)
{
    return dot(vLightDirection, vNormal) * 0.5f + 0.5f;  
}

VS_SHADOWED_OUTPUT VS_Shadowed(VS_SHADOWED_INPUT IN) 
{
	VS_SHADOWED_OUTPUT OUT;
	
	float4 vWorldSpacePosition = mul(IN.vObjectSpacePosition, g_mWorld);
	
	OUT.vScreenSpacePosition = mul(vWorldSpacePosition, g_mCameraViewProj);
	OUT.vNormal = IN.vNormal;
	OUT.vDepthAndTextureUV.xy = IN.vTextureUV;
	OUT.vDepthAndTextureUV.z = OUT.vScreenSpacePosition.z;
	OUT.vDepthAndTextureUV.w = GetDiffuse(IN.vNormal, g_vLightDirection);

	float4 mLightSpace;
	
	for(int n = 0; n < 4; n++ )
	{
		OUT.vProjectedLightPosition[n] = mul(vWorldSpacePosition, g_mShadowMap[n]).xy;
		mLightSpace =  mul(vWorldSpacePosition, g_mLightViewProj[n]);
		OUT.fLightDepth[n] = mLightSpace.z / mLightSpace.w - 0.0126f;
	}
	return OUT;
}

half GetSplitByDepth(float fDepth)
{
	float4 fTest = fDepth > g_fSplitDistances;
	return dot(fTest, fTest);
}

float GetShadow(half fSplitIndex, float2 faSplitUV[4], float4 faLightDepths)
{
	float fShadow = faLightDepths[fSplitIndex] > tex2D(sampShadowMap, faSplitUV[fSplitIndex].xy)[fSplitIndex];
	return 1.0f - fShadow * SHADOW_OPACITY;
}

float4 PS_Shadowed(VS_SHADOWED_OUTPUT IN) : COLOR
{
	half fSplitIndex = GetSplitByDepth(IN.vDepthAndTextureUV.z);
	float fShadow = GetShadow(fSplitIndex, IN.vProjectedLightPosition, IN.fLightDepth);
	float3 fDiffuse = fShadow * IN.vDepthAndTextureUV.w;  
	
	return float4(fDiffuse , 1.0f);
}

technique shadowed
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS_Shadowed();
		PixelShader = compile ps_3_0 PS_Shadowed();
	}
}

///////////////////////////////////////////////////////////////////////////////

void VS_DepthMap(
	float4 vObjectSpacePosition : POSITION,
	out float4 vScreenSpacePosition : POSITION,
	out float fDepth : TEXCOORD0)
{
	float4 vWorldSpacePosition = mul(vObjectSpacePosition, g_mWorld);
	
	vScreenSpacePosition = mul(vWorldSpacePosition, g_mCameraViewProj);
	fDepth = vScreenSpacePosition.z;
}

float4 PS_DepthMap(float fDepth : TEXCOORD0): COLOR
{
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

