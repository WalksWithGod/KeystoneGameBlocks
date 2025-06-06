// =============================================================
// Clouds Shader
// 
// Copyright (c) 2006 Renaud Bédard (Zaknafein)
// http://zaknafein.no-ip.org (renaud.bedard@gmail.com)
// =============================================================

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x4 matWorld : WORLD;	
float4x4 matWorldIT : WORLDINVERSETRANSPOSE;
float3 lightPos : LIGHTPOINT0_POSITION;

// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture texDiffuseEast : TEXTURE0;
sampler sampDiffuseEast = sampler_state {
	Texture = (texDiffuseEast);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;	
};

texture texDiffuseWest : TEXTURE1;
sampler sampDiffuseWest = sampler_state {
	Texture = (texDiffuseWest);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;	
};

texture texNormal : TEXTURE2;
sampler sampNormal = sampler_state {
	Texture = (texNormal);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT {
	float4 rawPos : POSITION;		// Vertex position in object space
	float3 normalVec : NORMAL;		// Vertex normal in object space
	float2 texCoord : TEXCOORD0;	// Vertex texture coordinate
	float3 binormalVec : BINORMAL;	// Vertex binormal in object space
	float3 tangentVec : TANGENT;	// Vertex tangent in object space	
};
struct VS_OUTPUT {
	float4 homogenousPos : POSITION;	// Transformed position
	float2 texCoord : TEXCOORD0;		// Interpolated & scaled t.c.
	float3 lightVec : TEXCOORD1;		// Light vector in tangent space
};
#define	PS_INPUT VS_OUTPUT		// What comes out of VS goes into PS!

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT IN) {
	VS_OUTPUT OUT;
    
    // Basic transformation into clip-space
    OUT.homogenousPos = mul(IN.rawPos, matWorldViewProj);	
	
	float3x3 TBNMatrix = { IN.tangentVec, IN.binormalVec, IN.normalVec };	 
	float3x4 worldITToTangentSpace = mul(TBNMatrix, matWorldIT);

	// Get the view vector and the light vector in tangent-space
	float4 worldPos = mul(IN.rawPos, matWorld);
	float3 lightVec = normalize(lightPos - worldPos); 
	OUT.lightVec = mul(worldITToTangentSpace, lightVec);
	
    // Since the TextureMod commands do not affect the coordinates,
    // we need to supply and apply them ourselves
    OUT.texCoord = IN.texCoord;
    
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS(PS_INPUT IN) : COLOR {	
	float3 sampledNormal = tex2D(sampNormal, IN.texCoord).rgb * 2 - 1;
	float dotProduct = dot(sampledNormal, normalize(IN.lightVec));

	dotProduct = dotProduct * 0.5f + 0.5f;
	float4 retColor;
	
	float3 colDiffMap;
	float2 corrTC;
	// Maps sampling
	if(IN.texCoord.x <= 0.5f) {
		corrTC = float2(IN.texCoord.x * 2.0f, IN.texCoord.y);
   		colDiffMap = tex2D(sampDiffuseEast, corrTC).r;
	} else {
		corrTC = float2((IN.texCoord.x - 0.5f) * 2.0f, IN.texCoord.y);
   		colDiffMap = tex2D(sampDiffuseWest, corrTC).r;
	}	
	
	retColor.rgb = dotProduct * 0.85f;
	retColor.a = colDiffMap * dotProduct * 0.8f;
	return retColor;
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique TShader {
    pass P0 {
		// States
		AlphablendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		AlphaTestEnable = false;    
    
        // Compile Shaders
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}