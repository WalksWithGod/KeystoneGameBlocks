//------------------------------------
float4x4 viewProj : ViewProjection;
float4x4 world : WorldInverseTranspose;

texture planetTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

texture skyTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

float4 lightDir : Direction
<
	string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f, 0.0f};

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

float4 materialDiffuse : Diffuse
<
    string UIWidget = "Surface Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 skyColor : Diffuse
<
    string UIWidget = "Sky Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float terraform
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Specular";
> = 1.0f;

float time : Time;

float planetIndex;
float skyIndex;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float3 normal				: NORMAL;
    float2 texCoordDiffuse		: TEXCOORD0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float2 texCoordDiffuse1	: TEXCOORD0;
    float2 texCoordDiffuse2	: TEXCOORD1;
    float3 texCoordDiffuse3 : TEXCOORD2;
    float4 diffAmbColor		: COLOR0;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    float4x4 worldViewProj = mul(world, viewProj);
    OUT.hPosition = mul( IN.position , worldViewProj);
    float u = IN.texCoordDiffuse.x + time*0.007f;
    float v = IN.texCoordDiffuse.y*0.0625f+9*0.0625f;
    OUT.texCoordDiffuse1 = float2(u,v);
    u = IN.texCoordDiffuse.x + time*0.007f;
    v = IN.texCoordDiffuse.y*0.0625f+12*0.0625f;
    OUT.texCoordDiffuse3 = float3(u,v, time*0.2f % 1);
    u = IN.texCoordDiffuse.x+ time*0.013f;
    v = IN.texCoordDiffuse.y;
    OUT.texCoordDiffuse2 = float2(u,v);

	float4 N = mul(IN.normal, world); //normal vector

    float3 L = normalize( -lightDir.xyz); //light vector

    float  diff = max(0 , dot(N,L));
    
    float4 ambColor =  lightAmbient;
    float4 diffColor = diff * lightColor;
    OUT.diffAmbColor = diffColor + ambColor;
    return OUT;
}


//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <planetTexture>;
    AddressU  = WRAP;        
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

//------------------------------------
sampler TextureSampler1 = sampler_state 
{
    texture = <skyTexture>;
    AddressU  = WRAP;        
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 planetTexture = tex2D( TextureSampler, IN.texCoordDiffuse1 );
  float4 planetTexture2 = tex2D( TextureSampler, IN.texCoordDiffuse3.xy );
  float4 skyTexture = tex2D( TextureSampler1, IN.texCoordDiffuse2 );
  return IN.diffAmbColor * lerp(lerp(planetTexture,planetTexture2,IN.texCoordDiffuse3.z), skyTexture*skyColor , skyTexture.a);//+ IN.specCol;
}


//-----------------------------------
technique PlanetShader
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_2_0 PS_Textured();
    }
}

//-----------------------------------
