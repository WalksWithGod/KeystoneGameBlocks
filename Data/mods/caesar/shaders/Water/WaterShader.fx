// =============================================================
// Perspective-Clipped Water Shader
// ********************************
// Copyright (c) 2006 Renaud Bédard (Zaknafein)
// E-mail : renaud.bedard@gmail.com
//
// Based on http://www.gamedev.net/reference/articles/article2138.asp,
// Waterman's water shader and nVidia's Ocean shader.
// =============================================================

// -------------------------------------------------------------
// Compilation flags
// -------------------------------------------------------------
// ***** Shader quality flags *****
// Gives depth to the water surface by using the view vector and
// the height of the normal-map (in the alpha channel) to warp
// the texture coordinates
#define PARALLAX_MAPPING
// Adds detail to the specular reflection and normal-mapping in general
// but adds an additional 2D sampling, which hurts the rendering speed
#define DETAIL_NORMAL_MAPPING
// Makes troubled water less reflective and lighter
#define TROUBLED_WATER_ENHANCEMENTS

// **** Normal-mapping techniques ****
// ** One or the other can be used! **
// Uses a 3D texture as a animation, which can provide more realistic
// waves and faster rendering, but has a larger memory footprint
#define VOLUME_NORMAL_MAP
// Uses a single 2D map, sampled two times to simulate animation
// less memory usage, but slower rendering because of multiple sampling
//#define DUAL_2D_NORMAL_MAP


// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x4 matWorldIT : WORLDIT;
float4x4 matWorld : WORLD;
float3 viewPosition : VIEWPOSITION;
float time : TIME;
float3 lightDir : LIGHTDIR0_DIRECTION;
float3 lightCol : LIGHTDIR0_COLOR;
			  
				  
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
	};
#else
	texture tex2DNormal : TEXTURE1;
	sampler samp2DNormal = sampler_state {
		Texture = (tex2DNormal);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = Linear;
	};
#endif

texture texFoamMask < string name = "C:\\Documents and Settings\\Hypnotron\\My Documents\\dev\\vb.net\\test\\AGT3rdPersonTV3DDemo\\Textures\\Water\\FoamMask.dds"; >;
sampler sampFoamMask = sampler_state {
	Texture = (texFoamMask);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
};

#ifdef DETAIL_NORMAL_MAPPING
	texture texDetailNormal < string name = "C:\\Documents and Settings\\Hypnotron\\My Documents\\dev\\vb.net\\test\\AGT3rdPersonTV3DDemo\\Textures\\Water\\DetailNormalMap.dds"; >;
	sampler sampDetailNormal = sampler_state {
		Texture = (texDetailNormal);
		MagFilter = Linear;
		MinFilter = Linear;
		MipFilter = Linear;
	};
#endif

// Lookup tables (replaces costly pow() functions with 1D textures)
texture texPhongLookup < string name = "C:\\Documents and Settings\\Hypnotron\\My Documents\\dev\\vb.net\\test\\AGT3rdPersonTV3DDemo\\Textures\\Lookup\\PhongLookUp.dds"; >;
sampler1D sampPhongLookup = sampler_state {
	Texture = (texPhongLookup);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
};
texture texFresnelLookup < string name = "C:\\Documents and Settings\\Hypnotron\\My Documents\\dev\\vb.net\\test\\AGT3rdPersonTV3DDemo\\Textures\\Lookup\\FresnelLookUp.dds"; >;
sampler1D sampFresnelLookup = sampler_state {
	Texture = (texFresnelLookup);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
};
texture texFoamLookup < string name = "C:\\Documents and Settings\\Hypnotron\\My Documents\\dev\\vb.net\\test\\AGT3rdPersonTV3DDemo\\Textures\\Lookup\\FoamLookUp.dds"; >;
sampler1D sampFoamLookup = sampler_state {
	Texture = (texFoamLookup);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
};

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// Parallax mapping settings
#ifdef PARALLAX_MAPPING
	// Defines how "deep" the parallax effect is (default 0.1f)
	#define PARALLAX_AMOUNT 0.1f
	// Must be half of the parallax amount
	#define HALF_PARALLAX_AMOUNT 0.05f
#endif
// Defines how heavy the reflection map distortion will be, based on 
// the distortion normal (default 0.25f)
#define DISTORTION_AMOUNT 0.25f
// How tiled is the foam texture (default 13)
#define FOAM_TEXTURE_TILING 13 
#ifdef DETAIL_NORMAL_MAPPING
	// How tiled is the detail normal-map, when enabled (default 12)
	#define DETAIL_NORMAL_MAP_TILING 12
#endif

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
// The water base-color, which is the "refracted" color of the
// bottom of the ocean. Could be lighter than that... I like it dark.
float3 waterColor = {0.01, 0.16, 0.19};
// The wave amplitude defines how bumpy the water is, how big
// the waves are; from 0 to 1.
float waveAmplitude = 0.7;
// The wave movement speed, based on texture coordinates
// scrolling/translation
float waveMovementSpeed = 3;
// The wave movement direction, as a unit 2D vector
float2 waveMovementDirection = {0, 1};

#ifdef VOLUME_NORMAL_MAP
// When using the volume map, define the speed of the animation
float waveAnimationSpeed = 1.5;
#endif
#ifdef TROUBLED_WATER_ENHANCEMENTS
// The sky average color replaces part of the reflection
// with troubled water, which makes it kinda diffuse-lit
float3 skyAverageColor = {0.5, 0.62, 0.77};		// Blueish sky
//float3 skyAverageColor = {0.63, 0.36, 0.13};	// Reddish sky
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
	float2 texCoord : TEXCOORD0;	// Pixel texture coordinates
	float4 texProj : TEXCOORD1;		// Texture projection coordinates
	float3 view : TEXCOORD2;		// View direction in tangent space
	float3 light : TEXCOORD3;		// Light direction in tangent space
	
#ifdef VOLUME_NORMAL_MAP
	float animation : TEXCOORD4;	// When using volume mapping, the wave
									// animation speed
#else
	float2 oppositeTC : TEXCOORD4;	// When using dual-2d mapping, the second
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
	float3 worldPos =  mul(IN.pos, matWorld);
	OUT.view = (viewPosition - worldPos).xzy;
	OUT.light = mul(matWorldIT, -lightDir).xzy;	
	// Shift the texture coordinate based on parameter
	OUT.texCoord = float2(IN.texCoord.x, IN.texCoord.y - time * waveMovementSpeed / 200);	
	
	// Second texture parameter depending on method used
	#ifdef VOLUME_NORMAL_MAP
	OUT.animation = time / (5 / waveAnimationSpeed);	
	#else
	OUT.oppositeTC = float2(IN.texCoord.x, IN.texCoord.y + time * waveMovementSpeed / 200);		
	#endif
	
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
#ifdef PARALLAX_MAPPING
// -- Parallax mapping function
float2 parallaxTexCoord(float2 oldTC, float level, float2 vecViewXY) {
	return (level * PARALLAX_AMOUNT - HALF_PARALLAX_AMOUNT) * waveAmplitude * vecViewXY + oldTC;
}
#endif

// -- Main PS function
float4 PS(PS_INPUT IN) : COLOR {
	float2 texCoord = IN.texCoord;
	float4 sampledNormal;
	float waveHeight;
	#ifdef VOLUME_NORMAL_MAP 
		// First sampling
		float animation = IN.animation;
		sampledNormal = tex3D(samp3DNormal, float3(texCoord, animation));
		#ifndef PARALLAX_MAPPING
			// Last sampling if not using parallax mapping, uncompress normalmap
			sampledNormal.rgb *= 2;
			sampledNormal.rgb -= 1;
		#endif
		// Save wave height (alpha channel)
		waveHeight = sampledNormal.a;
	#else
		// Two first samplings
		float2 oppositeTC = IN.oppositeTC;
		sampledNormal = tex2D(samp2DNormal, texCoord);
		float4 secondSampling = tex2D(samp2DNormal, oppositeTC);
		#ifndef PARALLAX_MAPPING
			// Last ones if not using parallax, normalize and uncompress the sum
			sampledNormal.rgb = normalize(sampledNormal.rgb * 2 + secondSampling.rgb * 2 - 2);
		#endif
		// Calculate and save wave height
		waveHeight = sampledNormal.a + secondSampling.a;
		waveHeight *= 0.5f;
	#endif
	
	// Per-pixel view vector normalization
	float3 view = normalize(IN.view);
	#ifdef PARALLAX_MAPPING
		// Parallax the texture coordinates
		#ifdef DUAL_2D_NORMAL_MAP
			oppositeTC = parallaxTexCoord(oppositeTC, waveHeight, view.xy);
		#endif
		texCoord = parallaxTexCoord(texCoord, waveHeight, view.xy);	
		
		// Re-sampled the parallaxed texture coordinates
		#ifdef VOLUME_NORMAL_MAP
			sampledNormal.rgb = tex3D(samp3DNormal, float3(texCoord, animation)).rgb * 2 - 1;	
		#else
			sampledNormal.rgb = tex2D(samp2DNormal, texCoord).rgb * 2 + tex2D(samp2DNormal, oppositeTC).rgb * 2 - 2;
			sampledNormal.rgb = normalize(sampledNormal.rgb);
		#endif
	#endif

	// Detail normal-mapping if enabled (makes specular look MUCH better)
	#ifdef DETAIL_NORMAL_MAPPING
		sampledNormal.rg += tex2D(sampDetailNormal, IN.texCoord * DETAIL_NORMAL_MAP_TILING).rg * 2 - 1;
	#endif
	
	// Morph the normal based on wave amplitude
	float3 pixelNormal = lerp(float3(0, 0, 1), sampledNormal.rgb, waveAmplitude);
	// Clamp the projective texture coordinate to [0, 1]
	float2 texProj2D = float2(IN.texProj.x, IN.texProj.y + IN.texProj.z) / IN.texProj.w;
	texProj2D.x = 1 - texProj2D.x;
	// Distort the coordinates with the normal
	texProj2D += pixelNormal.xy * DISTORTION_AMOUNT;
	
	// Evaluate light intensity
	float lightIntensity = max(max(lightCol.r, max(lightCol.g, lightCol.b)), 0.25f);
	
	// Calculate specular (Phong model, Blinn-Phong is ugly)
	float3 light = normalize(IN.light);
	float3 reflected = normalize(2 * dot(pixelNormal, light) * pixelNormal - light);
	float specular = tex1D(sampPhongLookup, saturate(dot(reflected, view)));	

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
	float spray = tex1D(sampFoamLookup, waveHeight) * tex2D(sampFoamMask, IN.texCoord * FOAM_TEXTURE_TILING).r * waveAmplitude;
	
	// Fresnel sampling
	float fresnel = tex1D(sampFresnelLookup, dot(view, pixelNormal));
	// And final color calculation
	float3 retColor = lerp(refractColor, reflectColor, fresnel) + (specular + spray) * lightCol;
	
	return float4(retColor, 1);
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
// Use SM3 if available
technique TSM3 {
    pass P0 {  
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS();		
    }
}
// Or fallback to SM2
technique TSM2 {
    pass P0 {  
		VertexShader = compile vs_2_0 VS();
		PixelShader  = compile ps_2_0 PS();		
    }
}