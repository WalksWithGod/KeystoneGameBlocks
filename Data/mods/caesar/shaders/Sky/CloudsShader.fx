// =============================================================
// Dual-Layer Clouds Shader 
// ************************
// Copyright (c) 2006 Renaud Bédard (Zaknafein)
// http://www.tvprojects.org/?cat=63&page=1&ex=1 
// E-mail : renaud.bedard@gmail.com
// =============================================================

// -------------------------------------------------------------
// Compilation Switches
// -------------------------------------------------------------
// Trades the costly normalize() instrinsic for a cubemap lookup
#define NORMALIZE_WITH_CUBEMAP

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj 	: WORLDVIEWPROJECTION;

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
// -- The two cloud textures layers
texture texLayer0 			: TEXTURE0;
sampler sampLayer0 = sampler_state 
{
	Texture = (texLayer0);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
};
texture texLayer1 			: TEXTURE1;
sampler sampLayer1 = sampler_state 
{
	Texture = (texLayer1);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
};

// -- Cube normalization map, if enabled
#ifdef NORMALIZE_WITH_CUBEMAP
textureCUBE texCubeNormalizer : TEXTURE2;
samplerCUBE sampCubeNormalizer = sampler_state 
{
	Texture = (texCubeNormalizer);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};	
#endif

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float2 _cloudsSize;
float2 _layersOpacity;
float2 _cloudsTranslation[2];
float3 _cloudsColor;

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT {
	float4 PositionOS 		: POSITION;	
	float2 UV 				: TEXCOORD;		
	float3 Normal 			: NORMAL;			
};
struct VS_OUTPUT {
	float4 PositionCS 		: POSITION;	
	float2 UV[2] 			: TEXCOORD0;		
	float3 Normal 			: TEXCOORD3;			
};
#define	PS_INPUT VS_OUTPUT		// What comes out of VS goes into PS!

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT IN) 
{
	VS_OUTPUT OUT;
    
    OUT.PositionCS = mul(IN.PositionOS, matWorldViewProj);
    
    // uv coordinate mods for scrolling texture
    for(int i=0; i<2; i++)
		OUT.UV[i] = IN.UV * _cloudsSize[i] + _cloudsTranslation[i];
		
	// TODO: Opacity factor is bullshit.  what should be happening is 
	// a) we use a plane quad mesh for our clouds and not a dome.
	//	  - as the pixel reaches the distance of atmosphere radius, we draw increasingly
	//      transparent until fully transparent at full distance and this way no abrupt 
	//      cutting off of clouds.
	// b) fog haze kicks in on atmosphere dome mesh at horizon
	// c) imposter rendering of procedural clouds -> http://www.markmark.net/clouds/
	//    and -> http://ofb.net/~niniane/clouds-jgt.pdf
	
	// Opacity factor to prevent ugly smuches on the edges
	OUT.Normal = -IN.Normal;
        	
	return OUT;
}

// -------------------------------------------------------------
// Switch-enabled normalization
// -------------------------------------------------------------
float3 normalizeVector(float3 vec) 
{
	#ifdef NORMALIZE_WITH_CUBEMAP
		float3 retVec = texCUBE(sampCubeNormalizer, vec).rgb;
		retVec = retVec * 2.0f - 1.0f;
		return retVec;
	#else
		return normalize(vec);
	#endif
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS(PS_INPUT IN) : COLOR 
{
	float layerAlpha[2];
	layerAlpha[0] = tex2D(sampLayer0, IN.UV[0]).a * _layersOpacity[0];
	layerAlpha[1] = tex2D(sampLayer1, IN.UV[1]).a * _layersOpacity[1];
	
	float transparencyFactor = saturate(normalizeVector(IN.Normal).y * 1.5f);
	
	return float4(_cloudsColor, saturate((layerAlpha[0] + layerAlpha[1]) * transparencyFactor));
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique technique0 
{
    pass pass0
	{
		// DestBlend is for the destination color. It is the pixel that already exists in this location. 
		// The blending function we are going to use for the destination is INV_SRC_ALPHA which is the 
		// inverse of the source alpha. This equates to one minus the source alpha value. For example 
		// if the source alpha is 0.3 then the dest alpha would be 0.7 so we will combine 70% of the 
		// destination pixel color in the final combine.
		// SrcBlend is for the source pixel which will be the input texture color value in this tutorial. 
		// The function we are going to use is SRC_ALPHA which is the source color multiplied by its own 
		// alpha value. 
		AlphablendEnable = true;
		AlphaTestEnable = false;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		  
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS();		
    }
}