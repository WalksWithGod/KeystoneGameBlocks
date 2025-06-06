//------------------------------------
float4x4 viewProj : ViewProjection;

texture planetTexture : Diffuse;

texture skyTexture : Diffuse;

float4 lightColor : Diffuse
<
    string UIName = "Diffuse Light Color";
    string Object = "DirectionalLight";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 lightAmbient : Ambient
<
    string UIWidget = "Ambient Light Color";
    string Space = "material";
> = {0.0f, 0.0f, 0.0f, 1.0f};

float time : Time
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "time";
> = 30.0;

float4x4 worldMatrices[14];


float4 skyColorSkyTex[14];
float4 lightDirPlanetTex[14];

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float3 normal				: NORMAL;
    float2 texCoordDiffuse		: TEXCOORD0;
    float index	: TEXCOORD1;
};

struct vertexInputSingle {
    float4 position				: POSITION;
    float3 normal				: NORMAL;
    float2 texCoordDiffuse		: TEXCOORD0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float2 texCoordDiffuse1	: TEXCOORD0;
    float2 texCoordDiffuse2	: TEXCOORD1;
    float4 diffAmbColor		: COLOR0;
    float4 skyColor			: COLOR1;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    float4x4 worldViewProj = mul(worldMatrices[IN.index], viewProj);
    OUT.hPosition = mul( IN.position , worldViewProj);
    float u = IN.texCoordDiffuse.x + time*0.007f;
    float v = IN.texCoordDiffuse.y*0.0625f+lightDirPlanetTex[IN.index].w*0.0625f;
    OUT.texCoordDiffuse1 = float2(u,v);
    u = IN.texCoordDiffuse.x+ time*0.013f;
    v = IN.texCoordDiffuse.y;
    OUT.texCoordDiffuse2 = float2(u,v);

	float4 N = mul(IN.normal, worldMatrices[IN.index]); //normal vector

    float3 L = normalize(-lightDirPlanetTex[IN.index].xyz); //light vector

    float  diff = max(0 , dot(N,L));
    
    float4 ambColor =  lightAmbient;
    float4 diffColor = diff * lightColor;
    OUT.diffAmbColor = diffColor + ambColor;
    OUT.skyColor = float4(skyColorSkyTex[IN.index].rgb,1);
    return OUT;
}

vertexOutput VS_TransformAndTextureSingle(vertexInputSingle IN) 
{
    vertexOutput OUT;
    float4x4 worldViewProj = mul(worldMatrices[0], viewProj);
    OUT.hPosition = mul( IN.position , worldViewProj);
    float u = IN.texCoordDiffuse.x + time*0.003f;
    float v = IN.texCoordDiffuse.y*0.0625f+lightDirPlanetTex[0].w*0.0625f;
    OUT.texCoordDiffuse1 = float2(u,v);
    u = IN.texCoordDiffuse.x+ time*0.007f;
    v = IN.texCoordDiffuse.y;
    OUT.texCoordDiffuse2 = float2(u,v);

	float4 N = mul(IN.normal, worldMatrices[0]); //normal vector

    float3 L = normalize(-lightDirPlanetTex[0].xyz); //light vector

    float  diff = max(0 , dot(N,L));
    
    float4 ambColor =  lightAmbient;
    float4 diffColor = diff * lightColor;
    OUT.diffAmbColor = diffColor + ambColor;
    OUT.skyColor = float4(skyColorSkyTex[0].rgb,1);
    return OUT;
}

//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <planetTexture>;
    AddressU  = Wrap;        
    AddressV  = Clamp;
    AddressW  = Wrap;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

//------------------------------------
sampler TextureSampler1 = sampler_state 
{
    texture = <skyTexture>;
    AddressU  = Wrap;        
    AddressV  = Clamp;
    AddressW  = Wrap;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 planetTexture = tex2D( TextureSampler, IN.texCoordDiffuse1 );
  float4 skyTexture = tex2D( TextureSampler1, IN.texCoordDiffuse2 );
  return IN.diffAmbColor * lerp(planetTexture , skyTexture*IN.skyColor , skyTexture.a * IN.skyColor.a);
}


//-----------------------------------
technique PlanetShaderInstanced
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_1_1 PS_Textured();
    }
}

technique PlanetShaderSingle
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTextureSingle();
		PixelShader  = compile ps_1_1 PS_Textured();
    }
}

//-----------------------------------
