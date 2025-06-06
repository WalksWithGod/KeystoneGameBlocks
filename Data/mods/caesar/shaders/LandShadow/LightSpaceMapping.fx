// -------------------------------------------------------------
// Those settings are (and must be) set by the host program
// -------------------------------------------------------------
//#define DEPTH_TESTING
//#define SHADOW_PROJECTION
//#define USE_VERTEX_COLOR
//#define TRANSPARENCY

// -------------------------------------------------------------
// Semantic mappings
// -------------------------------------------------------------
float4x4 matWorldViewProjection : WORLDVIEWPROJECTION;
float4x4 matWorld : WORLD;
#ifdef SHADOW_PROJECTION
	#ifndef USE_VERTEX_COLOR
		float4 materialDiffuse : DIFFUSE;
	#endif
#endif

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
#define ALPHA_TEST_REFERENCE_VALUE 0.5f

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texDiffuse : TEXTURE0;
sampler sampDiffuse = sampler_state 
{
	Texture = (texDiffuse);
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD0;
	#ifdef SHADOW_PROJECTION
		#ifdef USE_VERTEX_COLOR
			float4 vertexColor : COLOR;
		#endif
	#endif
	
};
struct VS_OUTPUT
{
	float4 position : POSITION;
	float depth : TEXCOORD0;
	float2 texCoord : TEXCOORD1;
	#ifdef SHADOW_PROJECTION
		float alpha : TEXCOORD2;
	#endif
};
#define PS_INPUT VS_OUTPUT

// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
VS_OUTPUT VS(const VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	OUT.position = mul(IN.position, matWorldViewProjection);	
	OUT.depth = OUT.position.z;	
	OUT.texCoord = IN.texCoord;
	
	#ifdef SHADOW_PROJECTION
		#ifdef USE_VERTEX_COLOR
			OUT.alpha = IN.vertexColor.a;
		#else
			OUT.alpha = materialDiffuse.a;
		#endif
	#endif
		
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader
// -------------------------------------------------------------
float4 PS(const PS_INPUT IN) : COLOR
{
	#ifdef DEPTH_TESTING
		#ifdef TRANSPARENCY
			clip(tex2D(sampDiffuse, IN.texCoord).a - ALPHA_TEST_REFERENCE_VALUE);
		#endif
		return float4(IN.depth.rrr, 1);
	#endif
	
	#ifdef SHADOW_PROJECTION
		#ifdef TRANSPARENCY
			return float4(0.0f.rrr, tex2D(sampDiffuse, IN.texCoord).a * IN.alpha);
		#else
			return float4(0.0f.rrr, 1);
		#endif
	#endif
	
	return 1;	
}

// -------------------------------------------------------------
// Techniques
// -------------------------------------------------------------
technique TSM3
{
    pass P1
    {
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
    }
}
technique TSM2
{
    pass P1
    {
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
    }
}