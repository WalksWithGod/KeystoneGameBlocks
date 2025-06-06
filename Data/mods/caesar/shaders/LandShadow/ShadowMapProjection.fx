// -------------------------------------------------------------
// Those settings are (and must be) set by the host program
// -------------------------------------------------------------
//#define NO_FILTERING
//#define PCF_3X3
//#define PCF_4X4
//#define DEPTH_TESTING
//#define SHADOW_PROJECTION

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// The precision is too harsh to allow depth bias right now
// So this renders self-shadowing impossible on the landscape
//#define DEPTH_BIAS 0.0125f
#define DEPTH_BIAS    0

// -------------------------------------------------------------
// Semantic mappings
// -------------------------------------------------------------
float4x4 matViewProjection : VIEWPROJECTION;
float4x4 matWorld : WORLD;

float3 dirLightDirection : LIGHTDIR0_DIRECTION;

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texLightSpaceMap;
sampler sampLightSpaceMap = sampler_state 
{
	Texture = (texLightSpaceMap);
	AddressU = BORDER;
	AddressV = BORDER;
	BorderColor = 0xffffff;
};

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float4x4 matLightViewProjection;
float4x4 matLightViewProjectionTexture;
float3 ambientColor;
#ifdef PCF_3X3
	float2 pcfOffsets[9];
#endif
#ifdef PCF_4X4
	float2 pcfOffsets[16];
#endif
#if defined(PCF_3X3) || defined(PCF_4X4)
	float2 texelSize;
#endif

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT
{
	float4 position : POSITION;
};
struct VS_OUTPUT
{
	float4 position : POSITION;
	float depth : TEXCOORD0;	
	#if defined(SHADOW_PROJECTION) || defined(NO_FILTERING)
		float2 shadowMapCoord : TEXCOORD1;
	#endif
	#ifdef PCF_3X3
		float2 firstTap : TEXCOORD1;
		float4 taps[4] : TEXCOORD2;
	#endif
	#ifdef PCF_4X4
		float4 centerAndFirstTaps : TEXCOORD1;
		float4 taps[6] : TEXCOORD2;
	#endif	
};
#define PS_INPUT VS_OUTPUT

// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
VS_OUTPUT VS(const VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	float4 worldPos = mul(IN.position, matWorld);
	OUT.position = mul(worldPos, matViewProjection);
	float2 centerTap = mul(worldPos, matLightViewProjectionTexture);
	
	#if defined(SHADOW_PROJECTION) || defined(NO_FILTERING)
		OUT.shadowMapCoord = centerTap;
	#endif
	
	#ifdef PCF_3X3
		OUT.firstTap = centerTap + texelSize * pcfOffsets[0];
		
		for(int i=0; i<4; i++)
		{
			OUT.taps[i].xy = centerTap + texelSize * pcfOffsets[i*2+1];
			OUT.taps[i].zw = centerTap + texelSize * pcfOffsets[i*2+2];
		}
	#endif
	
	#ifdef PCF_4X4
		OUT.centerAndFirstTaps.xy = centerTap;
		OUT.centerAndFirstTaps.zw = centerTap + texelSize * pcfOffsets[0];
		
		for(int i=0; i<6; i++)
		{
			OUT.taps[i].xy = centerTap + texelSize * pcfOffsets[i*2+1];
			OUT.taps[i].zw = centerTap + texelSize * pcfOffsets[i*2+2];
		}		
	#endif
	
	float4 lightPerspectivePosition = mul(worldPos, matLightViewProjection);
	OUT.depth = lightPerspectivePosition.z - DEPTH_BIAS;
	
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
float4 PS(const PS_INPUT IN) : COLOR
{
	float shadow = 0;
	
	#ifdef DEPTH_TESTING
		#ifdef PCF_3X3
			float3x3 pcfSamples;
			pcfSamples._m00 = (IN.depth > tex2D(sampLightSpaceMap, IN.firstTap).r) ? 0 : 0.111111f;

			pcfSamples._m01 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[0].xy).r) ? 0 : 0.111111f;
			pcfSamples._m02 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[0].zw).r) ? 0 : 0.111111f;
			pcfSamples._m10 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[1].xy).r) ? 0 : 0.111111f;
			pcfSamples._m11 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[1].zw).r) ? 0 : 0.111111f;
			pcfSamples._m12 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[2].xy).r) ? 0 : 0.111111f;
			pcfSamples._m20 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[2].zw).r) ? 0 : 0.111111f;
			pcfSamples._m21 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[3].xy).r) ? 0 : 0.111111f;
			pcfSamples._m22 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[3].zw).r) ? 0 : 0.111111f;
						
			shadow = dot(pcfSamples[0], 1) + dot(pcfSamples[1], 1) + dot(pcfSamples[2], 1);
		#endif
		
		#ifdef PCF_4X4
			float4x4 pcfSamples;
			pcfSamples._m00 = (IN.depth > tex2D(sampLightSpaceMap, IN.centerAndFirstTaps.zw).r) ? 0 : 0.0625f;
			pcfSamples._m01 = (IN.depth > tex2D(sampLightSpaceMap, IN.centerAndFirstTaps.xy + texelSize * pcfOffsets[1]).r) ? 0 : 0.0625f;
			
			pcfSamples._m02 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[0].xy).r) ? 0 : 0.0625f;
			pcfSamples._m03 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[0].zw).r) ? 0 : 0.0625f;			
			pcfSamples._m10 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[1].xy).r) ? 0 : 0.0625f;
			pcfSamples._m11 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[1].zw).r) ? 0 : 0.0625f;
			pcfSamples._m12 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[2].xy).r) ? 0 : 0.0625f;
			pcfSamples._m13 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[2].zw).r) ? 0 : 0.0625f;
			pcfSamples._m20 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[3].xy).r) ? 0 : 0.0625f;
			pcfSamples._m21 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[3].zw).r) ? 0 : 0.0625f;
			pcfSamples._m22 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[4].xy).r) ? 0 : 0.0625f;
			pcfSamples._m23 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[4].zw).r) ? 0 : 0.0625f;
			pcfSamples._m30 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[5].xy).r) ? 0 : 0.0625f;
			pcfSamples._m31 = (IN.depth > tex2D(sampLightSpaceMap, IN.taps[5].zw).r) ? 0 : 0.0625f;
			
			pcfSamples._m32 = (IN.depth > tex2D(sampLightSpaceMap, IN.centerAndFirstTaps.xy + texelSize * pcfOffsets[14]).r) ? 0 : 0.0625f;
			pcfSamples._m33 = (IN.depth > tex2D(sampLightSpaceMap, IN.centerAndFirstTaps.xy + texelSize * pcfOffsets[15]).r) ? 0 : 0.0625f;
		
			shadow = dot(pcfSamples[0], 1) + dot(pcfSamples[1], 1) + dot(pcfSamples[2], 1) + dot(pcfSamples[3], 1);
		#endif
		
		#ifdef NO_FILTERING
			shadow = (IN.depth > tex2D(sampLightSpaceMap, IN.shadowMapCoord).r) ? 0 : 1;
		#endif
	#endif
	
	#ifdef SHADOW_PROJECTION
		shadow = tex2D(sampLightSpaceMap, IN.shadowMapCoord);
	#endif
	
	return float4(saturate(shadow + ambientColor), shadow);
}

// -------------------------------------------------------------
// Techniques
// -------------------------------------------------------------
technique TSM3
{
    pass P1
    {
		AlphaTestEnable = false;
		CullMode = CW;		
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
    }
}
technique TSM2b
{
    pass P1
    {
		AlphaTestEnable = false;
		CullMode = CW;		
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_b PS();
    }
}
technique TSM2a
{
    pass P1
    {
		AlphaTestEnable = false;
		CullMode = CW;
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_a PS();
    }
}
technique TSM2
{
    pass P1
    {
		AlphaTestEnable = false;
		CullMode = CW;
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
    }
}