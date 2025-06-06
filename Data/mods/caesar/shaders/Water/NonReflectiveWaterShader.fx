// =============================================================
// Non-Reflective Water Shader
// ***************************
// Copyright (c) 2006 Renaud Bédard (Zaknafein)
// E-mail : renaud.bedard@gmail.com
// =============================================================
// Remarks
// 1) In this shader, the tiling is (1, 1) and multiplied by shader parameters
// You need to put a fair amount of polygons to get good-looking vertex fog
// The placement of the water plane in relation to the world is very important as well
// because of the new alpha map!!
// 2) The AlphaMap should be customized for the scene's particular terrain
// layout.  
// ============================================================= 

// -------------------------------------------------------------
// Compilation flags
// -------------------------------------------------------------
// Define if you use a DXT5-compressed map with the red channel
// cleared and its data in the alpha channel (the DXT5NM standard)
#define DXT5NM_NORMAL_MAP;

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj 	: WORLDVIEWPROJECTION;
float4x4 matWorldIT 		: WORLDIT;
float4x4 matWorld 			: WORLD;
float3	g_ViewPosition 		: VIEWPOSITION;
float g_Time 				: TIME;
float3 g_LightDir 			: LIGHTDIR0_DIRECTION;
float3 g_LightCol 			: LIGHTDIR0_COLOR; 
float fogStart 				: FOGSTART;
float fogEnd 				: FOGEND;
float fogDensity 			: FOGDENSITY;
float3 fogColor 			: FOGCOLOR;	  
int fogType 				: FOGTYPE;
			  
// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texNormal 			: TEXTURE1;
sampler sampNormal = sampler_state 
{
	Texture = <texNormal>;
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
};

texture texSpecularMask		: TEXTURE2;
sampler sampSpecularMask = sampler_state 
{
	Texture = <texSpecularMask>;
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;	
};

texture texAlpha			: TEXTURE3;
sampler sampAlpha = sampler_state 
{
	Texture = <texAlpha>;
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;	
};

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// Fog-related, do not modify
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
// The wave movement speed, based on texture coordinates
// scrolling/translation
float waveMovementSpeed = 3;
// The normalmap tiling -- using more tiles on Y makes the waves
// appear oval, looks a little better IMHO
float2 waveSize = {35, 45};
// The specular mask tiling, defines the size and spacing of the
// specular "sparkles"
float2 sparkleSize = {8, 10};

// Water dark (bottom) and light (top) colors, used for diffuse bump-mapping
float3 waterDarkColor;
float3 waterLightColor;


// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT 
{
	float4 PositionMS 	: POSITION;		// Vertex position in object space
	float2 texCoord 	: TEXCOORD;		// Vertex texture coordinates
};
struct VS_OUTPUT 
{
	float4 PositionCS 	: POSITION;		// Pixel position in clip space	
	float2 normalMapTC1 : TEXCOORD0;	// First normalmap sampler TC
	float2 normalMapTC2 : TEXCOORD1;	// Second normalmap sampler TC
	float3 view 		: TEXCOORD2;	// View vector in tangent space
	float3 light 		: TEXCOORD3;	// Light vector in tangent space
	float2 specularMapTC : TEXCOORD4;	// Specular mask TC
	float2 alphaMapTC 	: TEXCOORD5;	// Alpha map TC (1:1)
	float fog 			: FOG;			// Vertex-Fog thickness
};
#define	PS_INPUT VS_OUTPUT		// What comes out of VS goes into PS!

// -------------------------------------------------------------
// Other structs
// -------------------------------------------------------------
struct COMMON_PS_OUTPUT 
{
	float3 waterColor;
	float specular;
	float alpha;
};

//-------------------------------------------------------------------------------------------------
// fog
//-------------------------------------------------------------------------------------------------
float4 ComputeFogAmount(float dist) 
{
	float amount = (fogType == FOG_TYPE_NONE) +
			1.0f / exp(dist * fogDensity) * (fogType == FOG_TYPE_EXP) +
			1.0f / exp(pow(dist * fogDensity, 2.0f)) * (fogType == FOG_TYPE_EXP2) +
			saturate((fogEnd - dist) / (fogEnd - fogStart)) * (fogType == FOG_TYPE_LINEAR);
			
    return amount;
}

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT IN) 
{
	VS_OUTPUT OUT;

	OUT.PositionCS = mul(IN.PositionMS, matWorldViewProj);
	
	// Light, view transformation in plane-tangent space	
	float3 worldPos =  mul(IN.PositionMS, matWorld);
	OUT.view = (g_ViewPosition - worldPos).xzy;
	OUT.light = mul(matWorldIT, -g_LightDir).xzy;

	// Specular and alpha maps texture coords
	OUT.alphaMapTC = IN.texCoord;
	OUT.specularMapTC = IN.texCoord * sparkleSize;	

	IN.texCoord *= waveSize;
	// Scroll both normal maps texture coordinates
	OUT.normalMapTC1 = float2(IN.texCoord.x, IN.texCoord.y - g_Time * waveMovementSpeed / 100);		
	OUT.normalMapTC2 = float2(IN.texCoord.x + 0.5f, IN.texCoord.y + g_Time * waveMovementSpeed / 100);		

	// Calculate vertex fog
	float dist = distance(worldPos, g_ViewPosition);
	OUT.fog = ComputeFogAmount (dist);

	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
COMMON_PS_OUTPUT CommonPS(PS_INPUT IN) 
{
	COMMON_PS_OUTPUT OUT;

	// Perform alpha-testing with a very small epsilon (0.001f) so that
	// the water isn't rendered below the land (small optimization)
	OUT.alpha = 0.75; // tex2D(sampAlpha, IN.alphaMapTC).r;
	// clip(value) aborts PS if 'value' is negative (but apparently this does not result in early exit
	// because it acts like a branch and the rest of the function occurs anyway and final
	// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
	// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
	clip(OUT.alpha - 0.001f);

	// Sample the normal-map two times with TC's moving in opposite directions
	float2 normalMapTC1 = IN.normalMapTC1;
	float2 normalMapTC2 = IN.normalMapTC2;
	float3 firstSampling, secondSampling;
	#ifdef DXT5NM_NORMAL_MAP
		// AGB instead of RGB, R is cleared by contract
		firstSampling = tex2D(sampNormal, normalMapTC1).agb;	
		secondSampling = tex2D(sampNormal, normalMapTC2).agb;	
	#else
		firstSampling = tex2D(sampNormal, normalMapTC1).rgb;	
		secondSampling = tex2D(sampNormal, normalMapTC2).rgb;	
	#endif
	
	// Average and normalize the normal
	float3 pixelNormal = normalize(firstSampling * 2 + secondSampling * 2 - 2);
	
	// Normalize all input vectors
	float3 light = normalize(IN.light);	
	float3 view = normalize(IN.view);
	
	// Phong reflection optimization on a planar surface
	float3 reflected = float3(-light.xy, light.z);
	
	// Specular masking TC's are warped using the pixel normal to fake animation
	// 0.03 is an arbitrary factor, and depends on the tiling of the specular map
	float2 specularMapTC = IN.specularMapTC + pixelNormal.rg * 0.03f;
	// 2 factor is to make colored specular go white where the sun hits... could be removed
	OUT.specular = pow(saturate(dot(reflected, view)), 4) * tex2D(sampSpecularMask, specularMapTC) * 2;	
	
	// Greaten the normal's effect on the diffuse bump-mapping
	pixelNormal.rg *= 1.5f;
	OUT.waterColor = lerp(waterDarkColor, waterLightColor, dot(pixelNormal, light));
	
	return OUT;
}


float4 PS3(PS_INPUT IN) : COLOR 
{
	COMMON_PS_OUTPUT common = CommonPS(IN);

	// We need to LERP with fog color in PS 3.0
	float3 retColor = lerp(common.waterColor + common.specular * g_LightCol, fogColor, 1 - IN.fog);
	
	// max(alpha, specular) makes the alpha visible on small alpha values, looks nicer
	return float4(retColor, max(common.alpha, common.specular));
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
// Use SM3 if available
technique TSM3 
{
    pass pass0 
	{    
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS3();
		
		// Alphablending renderstates
		AlphablendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		AlphaTestEnable = false;  		
    }
}
