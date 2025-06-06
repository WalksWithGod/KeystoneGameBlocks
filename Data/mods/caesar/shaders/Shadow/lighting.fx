///////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////
///BlindSided's Specular Normal Mapping Shader (BASIC)			///
///////////////////////////////////////////////////////////////////////////
///Free for public use, but please give credit/link =)                  ///
///								        ///
///BS, aka. Stephen Smithbower.  -> smithy.s@gmail.com                  ///
///			  	    http://smithbower.com/devblog       ///
///////////////////////////////////////////////////////////////////////////
///Features:								///
///									///
///-> Diffuse normal mapping						///
///-> Specular component modulated by alpha of diffuse texture		///
///   (perhapes I should change this?)					///
///////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////
/// SYMANTICS								///
///////////////////////////////////////////////////////////////////////////
float4x4 view_proj_matrix	: VIEWPROJ;
float4 	 view_position		: VIEWPOSITION;
float4x4 WVP			: WORLDVIEWPROJECTION;
float4x4 world			: WORLD;
float4x4 worldViewIT		: WORLDVIEWIT;


///////////////////////////////////////////////////////////////////////////
/// DEFINES 								///
///////////////////////////////////////////////////////////////////////////

#define BIAS 0.98
#define NUM_DEPTH_SAMPLES 2


///////////////////////////////////////////////////////////////////////////
/// APP VARIABLES							///
///////////////////////////////////////////////////////////////////////////
float4 	lightPos;
float4 	lightColour;

float 	subFactor;
float 	texelScale = 0.02;

float	parallaxStrength = 0.015;

float 	falloff = 25;


///////////////////////////////////////////////////////////////////////////
/// TEXTURES								///
///////////////////////////////////////////////////////////////////////////
// Shadowmapping
texture ShadowMap;
sampler ShadowMapSampler = sampler_state {

    Texture = <ShadowMap>;

};

// Lighting
texture DiffuseMap 	: TEXTURE0;
sampler DiffuseMapSampler = sampler_state {

    Texture = <DiffuseMap>;

};

texture NormalMap 	: TEXTURE1;
sampler NormalMapSampler =  sampler_state {

    Texture = <NormalMap>;

};

texture HeightMap 	: TEXTURE2;
sampler HeightMapSampler =  sampler_state {

    Texture = <HeightMap>;

};

texture GlossMap 	: TEXTURE3;
sampler GlossMapSampler =  sampler_state {

    Texture = <GlossMap>;

};


///////////////////////////////////////////////////////////////////////////
/// STRUCTS								///
///////////////////////////////////////////////////////////////////////////
struct VS_OUTPUT {
   	float4 	Pos		: POSITION;
   	float2 	texCoord	: TEXCOORD0;
   	float3 	lightVec	: TEXCOORD1;
	float3 	smPos		: TEXCOORD2;
   	float3 	viewVec		: TEXCOORD3;
	float	depth		: TEXCOORD4;
};


///////////////////////////////////////////////////////////////////////////
/// VERTEX SHADER							///
///////////////////////////////////////////////////////////////////////////

VS_OUTPUT VSmain(float4 Pos: POSITION, float3 normal: NORMAL, float2 texCoord: TEXCOORD0, float3 binormal: BINORMAL, float3 tangent: TANGENT){
	VS_OUTPUT Out;

	Out.smPos 			= (mul(Pos, world)).xyz;
	Out.depth 			= length(Out.smPos - lightPos) * BIAS;

	Out.Pos 			= mul(Pos, WVP);
   	Out.texCoord 			= texCoord;

	float3 worldSpacePos 		= mul(Pos, world);

	float3x3 TBNMatrix 		= {tangent, binormal, normal};	    
   	float3x3 worldToTangentSpace 	= mul(TBNMatrix, (float4x3)world);

	Out.viewVec 			= mul(worldToTangentSpace, view_position - worldSpacePos);
	Out.lightVec 			= mul(worldToTangentSpace, (lightPos - worldSpacePos));	
	
   return Out;
}


///////////////////////////////////////////////////////////////////////////
/// PIXEL SHADER							///
///////////////////////////////////////////////////////////////////////////
float4 PSmain(float2 texCoord: TEXCOORD0, float3 lightVec: TEXCOORD1, float3 smPos: TEXCOORD2, float3 viewVec: TEXCOORD3, float inDepth: TEXCOORD4) : COLOR {


	float3 dir 		= normalize(lightPos - smPos);
	float3 nDir 		= -dir; // Saves a few instructions

	texelScale 		= texelScale * inDepth;
	texelScale 		= clamp(texelScale, 0.0, 0.0015);


	float depth 		= texCUBE(ShadowMapSampler, float4(nDir, 1.0)).r;

	float4 shadowFactor 	= {1.0, 1.0, 1.0, 1.0};

	for (int i = 0; i < NUM_DEPTH_SAMPLES; i++)
	{
		float depth1 = texCUBE(ShadowMapSampler, float4(nDir, 1.0) + texelScale * float4(2.0 + i, -1.0 - i, 1.0 + i, 0.0)).r;
		float depth2 = texCUBE(ShadowMapSampler, float4(nDir, 1.0) + texelScale * float4(1.0 + i, 2.0 + i, -1.0 - i, 0.0)).r;
		float depth3 = texCUBE(ShadowMapSampler, float4(nDir, 1.0) + texelScale * float4(-1.0 - i, 1.0 + i, 2.0 + i, 0.0)).r;

    		if(inDepth > depth1) {

        		shadowFactor.rgb -= subFactor;

    		}

    		if(inDepth > depth2) {

        		shadowFactor.rgb -= subFactor;

    		}

    		if(inDepth > depth3) {

        		shadowFactor.rgb -= subFactor;
    		}
	}


    	if(inDepth > depth) {

        	shadowFactor.rgb -= subFactor;
    	}

	//Just in case, preshader usually gets rid of this
	shadowFactor 		= clamp(shadowFactor, 0.0, 1.0);

		///////////////////////////////////////////////////////////////////////////////////

	 
	viewVec = normalize(viewVec);

	float2 newCoord 	= (tex2D(HeightMapSampler, texCoord) * parallaxStrength - parallaxStrength * 0.5) * viewVec + texCoord;

	float4 base 		= tex2D(DiffuseMapSampler, newCoord);
  	float3 bump 		= tex2D(NormalMapSampler, newCoord) * 2 - 1;
	float  spec		= tex2D(GlossMapSampler, newCoord).r;

   	// Standard lighting
   	
	float3 lVec 		= normalize(lightVec);


    	float LenSq 		= dot(lightVec, lightVec);
	float Attn 		= min((falloff * falloff) / LenSq, 1.0f);

   	float diffuse 		= saturate(dot(lVec, bump));
   	float specular 		= (pow(saturate(dot(reflect(-viewVec, bump), lVec)), 16));// * shadowFactor;

   	float4 C 		= ((diffuse * base) + (specular * spec * base)) * lightColour; //* Attn;


	return 			float4(C.r+0.1, C.g+0.1, C.b+0.1, base.a) * shadowFactor;


}


///////////////////////////////////////////////////////////////////////////
/// TECHNIQUES								///
///////////////////////////////////////////////////////////////////////////
technique main
{
    pass p0 
    {		
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_1_1 VSmain();
		PixelShader  = compile ps_2_0 PSmain();
    }
}