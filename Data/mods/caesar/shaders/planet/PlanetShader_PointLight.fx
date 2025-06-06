// =============================================================
// Planet Normal-Mapping Shader
// 
// Copyright (c) 2006 Renaud Bédard (Zaknafein)
// http://zaknafein.no-ip.org (renaud.bedard@gmail.com)
// =============================================================

// -------------------------------------------------------------
// Compilation switches
// -------------------------------------------------------------
// *************************************
// ** NORMAL MAPPING-RELATED FEATURES **
// *************************************
#ifdef NORMAL_MAPPING
	// Enables texture coord. perturbation based on the height map
	#define PARALLAX_MAPPING
	
	// Parralax amount, default is 0.025f
	#ifdef PARALLAX_MAPPING	
		#define PARALLAX_AMOUNT 0.025f			
		#define HALF_PARALLAX_AMOUNT 0.0125f
	#endif
#endif
	
// Specular power values for arithmetic path
#define SPEC_POW_INTERVAL 5.0f

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;	
float4x4 matWorldIT : WORLDINVERSETRANSPOSE;
float4x4 matWorld : WORLD;	
float4 vecViewPosition : CAMERAPOS; //VIEWPOS;
float3 lightPos : LIGHTPOINT0_POSITION;
float3 lightCol : LIGHTPOINT0_COLOR;

// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture texDiffuseSpecEast : TEXTURE0;
sampler sampDiffuseSpecEast = sampler_state {
	Texture = (texDiffuseSpecEast);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;	
};

texture texDiffuseSpecWest : TEXTURE1;
sampler sampDiffuseSpecWest = sampler_state {
	Texture = (texDiffuseSpecWest);
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

texture texEmissive : TEXTURE3;
sampler sampEmissive = sampler_state {
	Texture = (texEmissive);
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
	float3 binormalVec : BINORMAL;	// Vertex binormal in object space
	float3 tangentVec : TANGENT;	// Vertex tangent in object space
	float2 texCoord : TEXCOORD0;	// Vertex texture coordinate
};
struct VS_OUTPUT {
	float4 homogenousPos : POSITION;	// Transformed position
	float2 texCoord : TEXCOORD0;		// Interpolated & scaled t.c.
	float3 viewVec : TEXCOORD1;			// Eye vector in tangent space
	float4 lightVec : TEXCOORD2;		// Light vector in tangent space
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
	float4x3 matWorldCast = (float4x3)matWorld;
    
    	// The TBN (Tangent-Binormal-Normal) Matrix transforms vectors
    	// from world-space to tangent-space
    	float3x4 worldITToTangentSpace = mul(TBNMatrix, matWorldIT);
    	float3x3 worldToTangentSpace = mul(TBNMatrix, matWorldCast);

	// Since the light position is in world-space get the vertex position in world-space
    	float4 worldPos = mul(IN.rawPos, matWorld);

	// Get the view vector and the light vector in tangent-space
	OUT.viewVec = mul(worldToTangentSpace, vecViewPosition - worldPos);
		

	// we dont normalize here, normalize in the pixel shader for better precision.
	//float3 lightDir = lightPos - worldPos; 
	float3 lightDir = lightPos - float3(matWorld[3][0],matWorld[3][1],matWorld[3][2]);

	//OUT.lightVec = mul(worldITToTangentSpace, -lightDir);		
	//OUT.lightVec.xyz = mul(worldITToTangentSpace, -lightDir); 
	OUT.lightVec.xyz = mul(worldToTangentSpace, -lightDir); 
	OUT.lightVec.w  = length(lightDir); //distance(worldPos, lightPos);	

	// Since the TextureMod commands do not affect the coordinates,
    	// we need to supply and apply them ourselves
    	OUT.texCoord = IN.texCoord;
    
	return OUT;
}

// -------------------------------------------------------------
// Parralax-Mapping pixel fragment
// -------------------------------------------------------------
#ifdef PARALLAX_MAPPING
	float2 parallaxTexCoord(float2 oldCoord, float2 vecViewXY) {
		float level;
		level = tex2D(sampParallax, oldCoord).a;
		return (level * PARALLAX_AMOUNT - HALF_PARALLAX_AMOUNT) * vecViewXY + oldCoord;
	}
#endif

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS(PS_INPUT IN) : COLOR {
	// Obviously needed declarations
	float3 viewVec;
	float3 lightVec;
	float2 texCoord;	
	
	viewVec = IN.viewVec;
	lightVec = IN.lightVec;	
	// If normalization is in the pixel shader (better precision)
	viewVec = normalize(viewVec);
	//lightVec = normalize(lightVec);
   	lightVec = normalize(IN.lightVec.rgb);

	// Parallax mapping, if enabled
	#ifdef PARALLAX_MAPPING
		texCoord = parallaxTexCoord(IN.texCoord, viewVec.xy);
	#else
		texCoord = IN.texCoord;
	#endif
	
	float3 diffNormal, specNormal, sampledNormal;
	sampledNormal = tex2D(sampNormal, texCoord).rgb;
	// Normal for diffuse lightning is the uncompressed sampled one
	diffNormal = 2 * sampledNormal - 1;
	// Specular normal can be attenuated
	specNormal = diffNormal;

	// Specular reflection vector for Phong
	// MSDN	->		L - 2 * (N dot L) * N
	// W.Engel ->	2 * (N dot L) * N - L
	float3 reflectVec;
	// Using W.Engel...
	reflectVec = 2 * dot(specNormal, lightVec) * specNormal - lightVec;
	reflectVec = normalize(reflectVec);
	
	float3 colDiffMap;
	float colGloss;
	float2 corrTC;
	// Maps sampling
	// Hypnotron - I commented out this block so we dont have to use the two seperate maps
	//             But actually i think Zak is right in that some cards wont support texture sizes
	//             greater than 2048x2048.  Well, I know my FX5200 did and that's sort of low endish
	//		but i think there's some lower ones that wouldnt.
	//if(texCoord.x <= 0.5f) {
	//	corrTC = float2(texCoord.x * 2.0f, texCoord.y);
   	//	colDiffMap = tex2D(sampDiffuseSpecEast, corrTC).rgb;
   	//	colGloss = tex2D(sampDiffuseSpecEast, corrTC).a;	
	//} else {
	//	corrTC = float2((texCoord.x - 0.5f) * 2.0f, texCoord.y);
   	//	colDiffMap = tex2D(sampDiffuseSpecWest, corrTC).rgb;
   	//	colGloss = tex2D(sampDiffuseSpecWest, corrTC).a;	
	//}
	corrTC = texCoord; //float2(texCoord.x * 2.0f, texCoord.y);
   	colDiffMap = tex2D(sampDiffuseSpecEast, corrTC).rgb;
   	colGloss = tex2D(sampDiffuseSpecEast, corrTC).a;

	float3 colEmissive = tex2D(sampEmissive, texCoord).rgb;
	
	// Accumulators and intermediate dp3 variable
	float3 colDiffuse = 0.0f;
	float colSpecular = 0.0f;
	float dotProduct;
	
	// Lambertian diffuse term (N dot L)
	//dotProduct = dot(diffNormal, lightVec) * clamp(1 - length(IN.lightVec.w) / 10000, 0, 1); //10,000 is the distance
	dotProduct = dot(diffNormal, lightVec);
	colDiffuse = dotProduct * lightCol * colDiffMap.rgb;
	
	// EMissive
	float3 colEmis = colEmissive * pow((1 - dotProduct), 2) * 0.4f;
	
	// Specular highlightning term
	dotProduct = saturate(dot(reflectVec, viewVec));
	// Power of that term
	// Arithmetic, pow()-using version
	colSpecular = pow(dotProduct, SPEC_POW_INTERVAL);
	colSpecular *= colGloss * 0.5f + 0.25f;
	colSpecular *= 0.2f;
	
	// Sum of it all...
	float4 retColor = float4(colDiffuse + colSpecular, 1.0f); 
	retColor.rgb = saturate(retColor.rgb + colEmis);
    return retColor;
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique TShader {
    pass P0 {
        // Compile Shaders
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}