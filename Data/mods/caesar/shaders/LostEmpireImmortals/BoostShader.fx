//------------------------------------
float4x4 viewProj : ViewProjection;
float4x4 world : WorldInverseTranspose;
float4x4 viewInverse : ViewInverse;
texture diffuseTexture : Diffuse;
float scrollTexture;

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

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float2 texCoordDiffuse		: TEXCOORD0;
    float4 color				: COLOR0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float2 texCoordDiffuse	: TEXCOORD0;
    float4 color		: COLOR0;
};

//------------------------------------
vertexOutput Transform(vertexInput IN) 
{
    vertexOutput OUT;
    float4x4 worldViewProj = mul(world, viewProj);
    OUT.hPosition = mul( IN.position , worldViewProj);
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
    OUT.color = IN.color;
    
    return OUT;
}

//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = WRAP;        
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


float4 PixelShader(vertexOutput IN): COLOR
{
  float4 selectionTexture = tex2D( TextureSampler, IN.texCoordDiffuse );
  return IN.color * selectionTexture;
}

technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_1_1 Transform();
        pixelShader = compile ps_1_1 PixelShader();
    }
}