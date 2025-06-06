// =============================================================
// Perspective-Clipped Water Shader
// ********************************
// Copyright (c) 2006 Renaud B�dard (Zaknafein)
// E-mail : renaud.bedard@gmail.com
//
// Based on http://www.gamedev.net/reference/articles/article2138.asp,
// Waterman's water shader and nVidia's Ocean shader.
// =============================================================

// -------------------------------------------------------------
// Compilation flags
// -> Important Update <-
// Those should be set ONLY by the host application, and not
// directly in the shader. Makes management easier.
// -------------------------------------------------------------
// ***** Shader quality flags *****
// Gives depth to the water surface by using the view vector and
// the height of the normal-map (in the alpha channel) to warp
// the texture coordinates
//#define PARALLAX_MAPPING
// Adds detail to the specular reflection and normal-mapping in general
// but adds an additional 2D sampling, which hurts the rendering speed
//#define DETAIL_NORMAL_MAPPING
// Makes troubled water less reflective and lighter
//#define TROUBLED_WATER_ENHANCEMENTS
// Foam sprays
//#define FOAM_SPRAYS

// **** Normal-mapping techniques ****
// ** One or the other can be used! **
// Uses a 3D texture as a animation, which can provide more realistic
// waves and faster rendering, but has a larger memory footprint
//#define VOLUME_NORMAL_MAP
// Uses a single 2D map, sampled two times to simulate animation
// less memory usage, but slower rendering because of multiple sampling
//#define DUAL_2D_NORMAL_MAP

// **** Specular reflection modes ****
// ** One or the other can be used! **
// Phong reflection works in both normal-mapping modes 
//#define PHONG_SPECULAR_REFLECTION
// The Lyon/Blinn model produces ugly artifacts with a volume normal map
// because of precision shortage, but is more realistic and faster too!
//#define LYON_SPECULAR_REFLECTION

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x4 matWorldIT : WORLDIT;
float4x4 matWorld : WORLD;

float3 viewPosition : VIEWPOSITION;

float3 lightDir : LIGHTDIR0_DIRECTION;
float3 lightCol : LIGHTDIR0_COLOR;

float fogStart : FOGSTART;
float fogEnd : FOGEND;
float fogDensity : FOGDENSITY;
float3 fogColor : FOGCOLOR;	  
int fogType : FOGTYPE;

float time : TIME;
				  
// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texReflection : TEXTURE0;
sampler sampReflection = sampler_state {
	Texture = (texReflection);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

#ifdef VOLUME_NORMAL_MAP 
	texture3D tex3DNormal : TEXTURE1;
	sampler3D samp3DNormal = sampler_state {
		Texture = (tex3DNormal);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = Linear;
		AddressW = Wrap;
	};
#endif
#ifdef DUAL_2D_NORMAL_MAP
	texture tex2DNormal : TEXTURE1;
	sampler samp2DNormal = sampler_state {
		Texture = (tex2DNormal);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = Linear;
	};
#endif

texture texFoamMask;
sampler sampFoamMask = sampler_state {
	Texture = (texFoamMask);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
};

#ifdef DETAIL_NORMAL_MAPPING
	texture texDetailNormal;
	sampler sampDetailNormal = sampler_state {
		Texture = (texDetailNormal);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = Linear;
		AddressU = Wrap;
		AddressV = Wrap;
	};
#endif

// Lookup tables (replaces costly functions with 1D textures)
texture texFresnelLookup;
sampler1D sampFresnelLookup = sampler_state {
	Texture = (texFresnelLookup);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
};
#ifdef FOAM_SPRAYS
	texture texFoamLookup;
	sampler1D sampFoamLookup = sampler_state {
		Texture = (texFoamLookup);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = None;
		AddressU = Clamp;
	};
#endif
#ifdef PHONG_SPECULAR_REFLECTION
	texture texPhongLookup;
	sampler1D sampPhongLookup = sampler_state {
		Texture = (texPhongLookup);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = None;
		AddressU = Clamp;
	};
#endif

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// Parallax mapping settings
#ifdef PARALLAX_MAPPING
	// Defines how "deep" the parallax effect is (default 0.15f)
	#define PARALLAX_AMOUNT 0.15f
	// Must be half of the parallax amount
	#define HALF_PARALLAX_AMOUNT 0.075f
#endif
// Defines how heavy the reflection map distortion will be, based on 
// the distortion normal (default 0.25f)
#define DISTORTION_AMOUNT 0.25f
// How tiled is the foam texture (default 13)
#define FOAM_TEXTURE_TILING 13 
#ifdef DETAIL_NORMAL_MAPPING
	// How tiled is the detail normal-map, when enabled (default 10)
	#define DETAIL_NORMAL_MAP_TILING 10
#endif
// Specular over-brightness
#define SPECULAR_OVERBRIGHTNESS 1.75f
// Negative mip mapping bias for normal map sampling
#define MIP_BIAS -0.5f
// Specular exponent for Lyon/Blinn
#define LYONBLINN_EXPONENT 40

// Fog-related, do not modify
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
// The water base-color, which is the "refracted" color of the
// bottom of the ocean. Could be lighter than that...
float3 waterColor = {0.0125, 0.213, 0.253};
// The wave amplitude defines how bumpy the water is, how big
// the waves are; from 0 to 1.
float waveAmplitude = 0.7;
// The wave movement speed, based on texture coordinates
// scrolling/translation
float waveMovementSpeed = 6;
// The wave movement direction, as a unit 2D vector
float2 waveMovementDirection = {0, 1};

#ifdef VOLUME_NORMAL_MAP
	// When using the volume map, define the speed of the animation
	float waveAnimationSpeed = 1.5;
#endif
#ifdef TROUBLED_WATER_ENHANCEMENTS
	// The sky average color replaces part of the reflection
	// with troubled water, which makes it kinda diffuse-lit
	float3 skyAverageColor;
#endif

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT {
	float4 pos : POSITION;			// Vertex position in object space
	float2 texCoord : TEXCOORD;		// Vertex texture coordinates
};
struct VS_OUTPUT {
	float4 pos : POSITION;			// Pixel position in clip space	
	float4 texCoord : TEXCOORD0;	// Pixel texture coordinates
	float4 texProj : TEXCOORD1;		// Texture projection coordinates
	float3 view : TEXCOORD2;		// View direction in tangent space
	float3 light : TEXCOORD3;		// Light direction in tangent space
	float fog : FOG;				// Fog inverse thickness
#ifdef VOLUME_NORMAL_MAP
	float animation : TEXCOORD4;	// When using volume mapping, the wave
									// animation speed
#endif
#ifdef DUAL_2D_NORMAL_MAP
	float4 oppositeTC : TEXCOORD4;	// When using dual-2d mapping, the second
									// sampler's texture coordinate	
#endif
};
#define	PS_INPUT VS_OUTPUT		// What comes out of VS goes into PS!

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT IN) {
	VS_OUTPUT OUT;

	OUT.pos = mul(IN.pos, matWorldViewProj);  

	// Projective texturing	
	OUT.texProj = (OUT.pos + OUT.pos.w) * 0.5f;
	// The non-cull-inverting reflection method used needs this
	OUT.texProj.y *= -1;

	// Light, view transformation in plane-tangent space	
	float3 worldPos = mul(IN.pos, matWorld);
	OUT.view = (viewPosition - worldPos).xzy;
	OUT.light = mul(matWorldIT, -lightDir).xzy;	
	// Shift the texture coordinate based on parameter
	OUT.texCoord = float4(IN.texCoord + time * waveMovementSpeed * waveMovementDirection / 200, 0, MIP_BIAS);	
	
	// Second texture parameter depending on method used
	#ifdef VOLUME_NORMAL_MAP
	OUT.animation = time / (5 / waveAnimationSpeed);	
	#endif
	#ifdef DUAL_2D_NORMAL_MAP
	OUT.oppositeTC = float4(IN.texCoord + 0.5f + time * -waveMovementDirection * waveMovementSpeed / 200, 0, MIP_BIAS);
	#endif
	
	// Calculate vertex fog
	float dist = OUT.pos.z;
	OUT.fog = (fogType == FOG_TYPE_NONE) +
			1 / exp(dist * fogDensity) * (fogType == FOG_TYPE_EXP) +
			1 / exp(pow(dist * fogDensity, 2)) * (fogType == FOG_TYPE_EXP2) +
			saturate((fogEnd - dist) / (fogEnd - fogStart)) * (fogType == FOG_TYPE_LINEAR);
	
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
#ifdef PARALLAX_MAPPING
// Parallax mapping function
float2 parallaxTexCoord(float2 oldTC, float level, float2 vecViewXY) {
	return (level * PARALLAX_AMOUNT - HALF_PARALLAX_AMOUNT) * waveAmplitude * vecViewXY + oldTC;
}
#endif

// Common PS 2.0 and PS 3.0 function
float3 CommonPS(PS_INPUT IN) {
	float4 texCoord = IN.texCoord;
	float4 sampledNormal = 0.0f.rrrr;
	float waveHeight;
	#ifdef VOLUME_NORMAL_MAP 
		// First sampling
		float animation = IN.animation;
		sampledNormal = tex3Dbias(samp3DNormal, float4(texCoord.xy, animation, texCoord.w));
		#ifndef PARALLAX_MAPPING
			// Last sampling if not using parallax mapping, uncompress normalmap
			sampledNormal.rgb *= 2;
			sampledNormal.rgb -= 1;
		#endif
		#if defined(PARALLAX_MAPPING) || defined(FOAM_SPRAYS)
			// Save wave height (alpha channel)
			waveHeight = sampledNormal.a;
		#endif			
	#endif
	#ifdef DUAL_2D_NORMAL_MAP
		// Two first samplings
		float4 oppositeTC = IN.oppositeTC;
		sampledNormal = tex2Dbias(samp2DNormal, texCoord).rgba;
		float4 secondSampling = tex2Dbias(samp2DNormal, oppositeTC).rgba;
		#ifndef PARALLAX_MAPPING
			// Last ones if not using parallax, normalize and uncompress the sum
			sampledNormal.rgb = normalize(sampledNormal.rgb * 2 + secondSampling.rgb * 2 - 2);
		#endif
		#if defined(PARALLAX_MAPPING) || defined(FOAM_SPRAYS)		
			// Calculate and save wave height
			waveHeight = saturate((sampledNormal.a + secondSampling.a) * 0.6);
		#endif
	#endif
	
	// Per-pixel view vector normalization
	float3 view = normalize(IN.view);
	#ifdef PARALLAX_MAPPING
		// Parallax the texture coordinates
		#ifdef DUAL_2D_NORMAL_MAP
			oppositeTC.xy = parallaxTexCoord(oppositeTC, waveHeight, view.xy);
		#endif
		texCoord.xy = parallaxTexCoord(texCoord.xy, waveHeight, view.xy);	
		
		// Re-sampled the parallaxed texture coordinates
		#ifdef VOLUME_NORMAL_MAP
			sampledNormal.rgb = tex3Dbias(samp3DNormal, float4(texCoord.xy, animation, texCoord.w)).rgb * 2 - 1;	
		#endif
		#ifdef DUAL_2D_NORMAL_MAP
			sampledNormal.rgb = tex2Dbias(samp2DNormal, texCoord).rgb * 2 + tex2Dbias(samp2DNormal, oppositeTC).rgb * 2 - 2;
			sampledNormal.rgb = normalize(sampledNormal.rgb);
		#endif
	#endif

	// Detail normal-mapping if enabled (makes specular look MUCH better)
	#ifdef DETAIL_NORMAL_MAPPING
		sampledNormal.rg += tex2D(sampDetailNormal, IN.texCoord.xy * DETAIL_NORMAL_MAP_TILING).rg * 2 - 1;
	#endif
	
	// Morph the normal based on wave amplitude
	float3 pixelNormal = lerp(float3(0, 0, 1), sampledNormal.rgb, waveAmplitude);
	// Clamp the projective texture coordinate to [0, 1]
	float2 texProj2D = float2(IN.texProj.x, IN.texProj.y + IN.texProj.z) / IN.texProj.w;
	texProj2D.x = 1 - texProj2D.x;
	// Distort the coordinates with the normal
	texProj2D += pixelNormal.xy * DISTORTION_AMOUNT;
	
	// Evaluate light intensity
	float lightIntensity = max((lightCol.r + lightCol.g + lightCol.b) / 3, 0.2f);
	
	// Calculate specular
	float3 light = normalize(IN.light);
	float specular;
	#ifdef PHONG_SPECULAR_REFLECTION
		float3 reflected = normalize(2 * dot(pixelNormal, light) * pixelNormal - light);	
		specular = tex1D(sampPhongLookup, saturate(dot(reflected, view)));
	#endif
	#ifdef LYON_SPECULAR_REFLECTION
		float3 halfway = normalize(light + view);
		float3 difference = halfway - pixelNormal;
		float xs = saturate(dot(difference, difference) * LYONBLINN_EXPONENT);
		specular = pow(1 - xs, 3);
	#endif

	float3 refractColor;
	// Sample the projected, reflected color	
	float3 reflectColor = tex2D(sampReflection, texProj2D).rgb;
	#ifdef TROUBLED_WATER_ENHANCEMENTS
		// Some LERPs to make the water less reflective and more natural when troubled
		// The lerp factors could be changed, I just played until I found a value I liked
		float noiseFactor = pow(waveAmplitude, 2);
		refractColor = lerp(waterColor, skyAverageColor, noiseFactor * 0.25f);
		reflectColor = lerp(reflectColor, skyAverageColor, noiseFactor * 0.55f);
		specular *= 1 - (noiseFactor * 0.6f);	
	#else
		refractColor = waterColor;
	#endif
	// Makes night-time water naturally black
	refractColor *= lightIntensity;
	
	// Spray/foam calculation based on wave height	
	#ifdef FOAM_SPRAYS
		float spray = tex1D(sampFoamLookup, waveHeight) * tex2D(sampFoamMask, IN.texCoord * FOAM_TEXTURE_TILING).r * waveAmplitude;
	#endif
	
	// Fresnel sampling
	float fresnel = tex1D(sampFresnelLookup, dot(view, pixelNormal));
	
	// Factoring the specular makes it brighter and look better when the light is colored
	return lerp(refractColor, reflectColor, fresnel) + (specular * lightCol * SPECULAR_OVERBRIGHTNESS)
	#ifdef FOAM_SPRAYS
		+ spray * lightCol
	#endif
	;
}

// PS 2.0 Function
float4 PS2(PS_INPUT IN) : COLOR {
	// In PS 2.0, no need to put fog in the equation, it's done for us!	
	return float4(CommonPS(IN), 1);
}

// PS 3.0 Function
float4 PS3(PS_INPUT IN) : COLOR {
	// We need to LERP with fog color in PS 3.0
	float3 retColor = lerp(CommonPS(IN), fogColor, 1 - IN.fog);
	return float4(retColor, 1);
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique TSM3 {
    pass P0 {  
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS3();		
    }
}

technique TSM2a {
    pass P0 {  
		VertexShader = compile vs_2_0 VS();
		PixelShader  = compile ps_2_a PS2();		
    }
}

technique TSM2b {
    pass P0 {  
		VertexShader = compile vs_2_0 VS();
		PixelShader  = compile ps_2_b PS2();		
    }
}


technique TSM2 {
    pass P0 {  
		VertexShader = compile vs_2_0 VS();
		PixelShader  = compile ps_2_0 PS2();		
    }
}